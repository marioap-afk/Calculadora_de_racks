#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T4C1-HOST (D-T4C-02, D-T4C-03, D-T4C-05, D-T4C-08, D-T4C-10, D-T4C-17; INV-F4T4C-01, 02, 07, 08, 10 and 13): the request of the
// update of the pilot over the REAL host core, with the executor double over the real runner, the double of the scope of T4B and the real pure
// bridge. Nothing here loads AutoCAD. Every expectation that something throws, or does not, uses Record.Exception, so a behavior that is
// missing fails by ASSERTION and never by an escaped exception.
public class WorkspacePilotUpdateRequestTests
{
    private const string RackA = EditIntentKit.RackA;
    private const string RackB = EditIntentKit.RackB;

    private sealed class Rig
    {
        public Rig(bool executor = true)
        {
            Host = new WorkspaceHostRig().WithDocument(100);
            Host.Host.OpenPanel();
            Session = Host.ActiveSession;
            Host.Idle();
            Reader = new EditIntentFakeReader { Answer = _ => EditIntentKit.ReadOf(PrepareKit.RealScan()) };
            Host.Host.SetEditIntentReader(Reader);
            Host.Bridge.Core.SetLifecycleAdvisor(Host.Host);
            Writer = new PilotUpdateScopeDouble(Members());
            Kit = new PilotHostKit(Writer);
            if (executor) Host.Host.SetPilotUpdateHost(Kit.Executor);
        }

        public WorkspaceHostRig Host { get; }

        public WorkspaceHostCore Core => Host.Host;

        public IWorkspacePilotEditPort Port => Host.Host;

        public EditIntentFakeReader Reader { get; }

        public SessionId Session { get; }

        public PilotUpdateScopeDouble Writer { get; }

        public PilotHostKit Kit { get; }

        public static IntentItem[] Members()
        {
            var scan = PrepareKit.RealScan();
            return scan.Facts.Select(f => new IntentItem(f, scan.RawEnvelopes[f.DefinitionKey])).ToArray();
        }

        public EditIntentRequest Request(string rackId = RackA, SessionId? session = null) => new(session ?? Session, rackId, null, null);

        public WorkspaceRequest Origin(SessionId? session = null) => new(session ?? Session);

        public DraftSession<PushBackPilotDraftState> Open(bool edit = true, string rackId = RackA)
        {
            Assert.Equal(EditIntentStatus.Admitted, Core.TryBeginPilotEdit(Request(rackId)).Status);
            var draft = Core.OpenDrafts.Find<PushBackPilotDraftState>(Session, rackId)!;
            if (edit) PrepareKit.SetScale(draft, 2.25);
            return draft;
        }

        public EditUpdateStart Start(string rackId = RackA) => Port.RequestUpdate(Request(rackId));

        public EditUpdateProgress Take(string rackId = RackA) => Port.TakeUpdateProgress(Origin(), rackId);

        /// <summary>A command that is not the update itself starts in the session: the handler of the bridge notes it and the core learns it.</summary>
        public void ForeignCommand()
        {
            Host.CommandStarts(Session);
            Host.Bridge.Core.NoteForeignCommandStart(Session);
            Host.CommandEnds(Session);
        }

        /// <summary>The events that H0 measured around the callback: two entries into rest and the start of the update's own command before it, the end and one entry into rest after.</summary>
        public void UseTheOrderMeasuredByH0()
        {
            Kit.Primitives.Mode = PilotHostMode.Deferred;
            Kit.Primitives.BeforeCallback = () =>
            {
                Host.Bridge.Core.OnSourceEvent(Session, BridgeSource.EnteringQuiescentState);
                Host.Bridge.Core.OnSourceEvent(Session, BridgeSource.EnteringQuiescentState);
                Host.Bridge.Core.OnSourceEvent(Session, BridgeSource.CommandWillStart);
            };
            Kit.Primitives.AfterCallback = () =>
            {
                Host.Bridge.Core.OnSourceEvent(Session, BridgeSource.CommandEnded);
                Host.Bridge.Core.OnSourceEvent(Session, BridgeSource.EnteringQuiescentState);
            };
        }

        public int WriterBegins => Writer.Calls.Count(c => c == "Begin");
    }

    private static InvalidOperationException Boom(string message) => new(message);

    // ------------------------------------------------------------------ the start values

    [Fact]
    public void ARequestOnADirtyDraftIsAcceptedAndTheOutcomeOfTheUpdateIsTakenOnceByRack()
    {
        var rig = new Rig();
        var draft = rig.Open();

        var start = rig.Start();
        var progress = rig.Take();

        Assert.Equal(EditUpdateStartStatus.Accepted, start.Status);
        Assert.Null(start.Reason);
        Assert.False(progress.InFlight);
        Assert.Equal(EditUpdateStatus.Applied, progress.Outcome.Status);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Equal(1, rig.Kit.Executor.TryRequests);
        // Taking consumes: the second take has nothing, and another rack never had anything.
        Assert.Equal(EditUpdateStatus.Unavailable, rig.Take().Outcome.Status);
        Assert.Equal(EditUpdateStatus.Unavailable, rig.Take(RackB).Outcome.Status);
    }

    [Fact]
    public void TheStartValueWithoutAnExecutorOrAReaderIsUnavailableAndTheDefaultValuesAreInert()
    {
        var noExecutor = new Rig(executor: false);
        noExecutor.Open();
        var start = noExecutor.Start();
        Assert.Equal(EditUpdateStartStatus.Unavailable, start.Status);
        Assert.Equal(PushBackPilotHostTexts.HostUnavailableReason, start.Reason);

        var fresh = new WorkspaceHostRig().WithDocument(100);
        fresh.Host.OpenPanel();
        var session = fresh.ActiveSession;
        fresh.Idle();
        fresh.Host.SetPilotUpdateHost(new PilotUpdateHostExecutorDouble(new PilotHostPrimitivesDouble(new List<string>()), new List<(string, Exception)>()));
        Assert.Equal(EditUpdateStartStatus.Unavailable, ((IWorkspacePilotEditPort)fresh.Host).RequestUpdate(new EditIntentRequest(session, RackA, null, null)).Status);

        Assert.Equal(EditUpdateStartStatus.Unavailable, default(EditUpdateStart).Status);
        Assert.False(default(EditUpdateProgress).InFlight);
        Assert.Equal(EditUpdateStatus.Unavailable, default(EditUpdateProgress).Outcome.Status);
        Assert.Equal(
            new[] { "Unavailable", "Accepted", "Waiting", "Rejected", "Busy", "Failed" },
            Enum.GetNames<EditUpdateStartStatus>());
    }

    [Fact]
    public void TheGateAnswersWaitingForNoDocumentAModalAndNoRestAndRejectedForAnotherSessionAndRunsNothing()
    {
        var rig = new Rig();
        rig.Open();

        rig.Core.SetModalRackCadActive(true);
        var modal = rig.Start();
        Assert.Equal(EditUpdateStartStatus.Waiting, modal.Status);
        rig.Core.SetModalRackCadActive(false);

        rig.Host.Probe.IsActiveDocumentQuiescent = false;
        Assert.Equal(EditUpdateStartStatus.Waiting, rig.Start().Status);
        rig.Host.Probe.IsActiveDocumentQuiescent = true;

        Assert.Equal(EditUpdateStartStatus.Rejected, rig.Port.RequestUpdate(rig.Request(session: new SessionId(777))).Status);

        rig.Host.Probe.ActiveDocumentNativeId = null;
        Assert.Equal(EditUpdateStartStatus.Waiting, rig.Start().Status);
        Assert.Equal(0, rig.Kit.Executor.TryRequests);
        Assert.Empty(rig.Writer.Calls);
    }

    [Fact]
    public void ARequestInsideANoticeOfThePanelWaitsAndAnyOtherStartIsNotATerminalFailure()
    {
        var rig = new Rig();
        rig.Open();
        var depth = new WorkspaceNoticeDepth();
        var gate = new WorkspacePilotEditGate(rig.Port, depth);

        depth.Enter();
        var inside = gate.RequestUpdate(rig.Request());
        depth.Leave();

        Assert.Equal(EditUpdateStartStatus.Waiting, inside.Status);
        Assert.Equal(WorkspacePilotEditGate.InsideNoticeReason, inside.Reason);
        Assert.Equal(0, rig.Kit.Executor.TryRequests);
        Assert.Equal(EditUpdateStartStatus.Accepted, gate.RequestUpdate(rig.Request()).Status);
    }

    [Fact]
    public void ARequestWhileAReadOfThePilotIsInProgressIsBusyAndAsksNothing()
    {
        var rig = new Rig();
        rig.Open();
        EditUpdateStart? inside = null;
        rig.Reader.Answer = rack =>
        {
            inside = rig.Start();
            return EditIntentKit.Admitted(rack);
        };

        rig.Port.TryBeginEdit(rig.Request(RackB));

        Assert.NotNull(inside);
        Assert.Equal(EditUpdateStartStatus.Busy, inside!.Value.Status);
        Assert.Equal(0, rig.Kit.Executor.TryRequests);
    }

    [Fact]
    public void AnExecutorThatThrowsIsAFailedStartValueAndLeavesNoTicketNorOutcome()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Executor.Throw = true;

        var start = rig.Start();

        Assert.Equal(EditUpdateStartStatus.Failed, start.Status);
        Assert.StartsWith(PushBackPilotHostTexts.HostRequestFailedPrefix, start.Reason, StringComparison.Ordinal);
        Assert.False(rig.Take().InFlight);
        Assert.Equal(EditUpdateStatus.Unavailable, rig.Take().Outcome.Status);
        rig.Kit.Executor.Throw = false;
        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
    }

    // ------------------------------------------------------------------ one update at a time (tests 9 and 18)

    [Fact]
    public void ASecondRequestWhileTheFirstIsInFlightIsBusyAndTheExecutorIsAskedOnceAndAnotherIsAcceptedAfterTheEnd()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;

        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
        var second = rig.Start();
        var inFlight = rig.Take();

        Assert.Equal(EditUpdateStartStatus.Busy, second.Status);
        Assert.Equal(PushBackPilotHostTexts.UpdateInFlightReason, second.Reason);
        Assert.Equal(1, rig.Kit.Executor.TryRequests);
        Assert.True(inFlight.InFlight);
        Assert.Equal(EditUpdateStatus.Unavailable, inFlight.Outcome.Status);

        rig.Kit.Primitives.RunDeferred();
        Assert.Equal(EditUpdateStatus.Applied, rig.Take().Outcome.Status);
        // The draft is clean now: the second update has nothing to apply, but the request itself is accepted again.
        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
    }

    [Fact]
    public void TheUpdateOfT4BRunsOncePerAcceptedRequestAlsoWithTwoClicksAndARepeatedCallback()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Kit.Primitives.RepeatCallback = true;

        rig.Start();
        rig.Start();
        rig.Kit.Primitives.RunDeferred();

        Assert.Equal(1, rig.WriterBegins);
        Assert.Equal(1, rig.Kit.Executor.TryRequests);
        Assert.Equal(1, rig.Kit.Target.CreateWriters);
        Assert.Equal(EditUpdateStatus.Applied, rig.Take().Outcome.Status);
    }

    // ------------------------------------------------------------------ the document and the session (tests 2 and 3)

    [Fact]
    public void ADocumentThatChangedBeforeTheCallbackIsARejectedOutcomeAndTheDraftIsUntouched()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Start();
        rig.Kit.Primitives.ActiveNativeId = 999;

        rig.Kit.Primitives.RunDeferred();

        var outcome = rig.Take().Outcome;
        Assert.Equal(EditUpdateStatus.Rejected, outcome.Status);
        Assert.Equal(PushBackPilotUpdateRun.DocumentMismatchReason, outcome.Reason);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        Assert.Empty(rig.Writer.Calls);
        Assert.Equal(0, rig.Kit.Target.Locks);
        // The ticket is gone with the completion: another request is accepted.
        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
    }

    [Fact]
    public void ASessionDestroyedBetweenTheAcceptanceAndTheCallbackIsStoppedBeforeTheLockAndLeavesNoTicketNorOutcome()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Start();

        rig.Host.CloseActive(100);
        rig.Kit.Primitives.ActiveNativeId = null;
        rig.Kit.Primitives.RunDeferred();

        Assert.Equal(0, rig.Kit.Target.Locks);
        Assert.Empty(rig.Writer.Calls);
        Assert.False(rig.Take().InFlight);
        Assert.Equal(EditUpdateStatus.Unavailable, rig.Take().Outcome.Status);
    }

    [Fact]
    public void ARecycledNativeIdentityFindsTheNewDocumentAndTheAdmissionOfTheOldSessionStopsItBeforeTheLock()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Start();

        rig.Host.CloseActive(100);
        var newSession = rig.Host.Activate(100);
        Assert.NotEqual(rig.Session, newSession);
        rig.Kit.Primitives.RunDeferred();

        Assert.Equal(1, rig.Kit.Primitives.Opens);
        Assert.Equal(0, rig.Kit.Target.Locks);
        Assert.Equal(0, rig.Kit.Target.CreateWriters);
        Assert.Empty(rig.Writer.Calls);
        Assert.False(rig.Port.TakeUpdateProgress(rig.Origin(newSession), RackA).InFlight);
    }

    // ------------------------------------------------------------------ a failure of the host before the update (tests 10 and R-02)

    [Fact]
    public void AContextThatFailsBeforeTheCallbackIsOnlyAFailedStartValueAndNoUpdateRunsAndTheDraftKeepsEverything()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Kit.Primitives.ThrowOnEnter = true;
        var before = (draft.Status, draft.LastApplyCause, draft.IsPossiblyStale);

        var start = rig.Start();

        Assert.Equal(EditUpdateStartStatus.Failed, start.Status);
        Assert.StartsWith(PushBackPilotHostTexts.HostRequestFailedPrefix, start.Reason, StringComparison.Ordinal);
        Assert.Empty(rig.Writer.Calls);
        Assert.Equal(before, (draft.Status, draft.LastApplyCause, draft.IsPossiblyStale));
        Assert.Equal(EditUpdateStatus.Unavailable, rig.Take().Outcome.Status);
        Assert.False(rig.Take().InFlight);
    }

    [Fact]
    public void ANativeRefusalThatNeverRunsNorCompletesExpiresWithTheFirstForeignCommandAndLeavesAFailedOnceAndFreesEverything()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Never;

        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
        Assert.True(rig.Take().InFlight);
        Assert.Equal(EditUpdateStartStatus.Busy, rig.Start().Status);

        rig.ForeignCommand();
        var progress = rig.Take();

        Assert.False(progress.InFlight);
        Assert.Equal(EditUpdateStatus.Failed, progress.Outcome.Status);
        Assert.Equal(PushBackPilotHostTexts.HostNotRunReason, progress.Outcome.Reason);
        Assert.Equal(EditUpdateStatus.Unavailable, rig.Take().Outcome.Status);
        Assert.Empty(rig.Writer.Calls);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
        // Everything is free again: the request, and the discard of the rack.
        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
        rig.ForeignCommand();
        Assert.Equal(EditDiscardStatus.Discarded, rig.Port.DiscardRack(rig.Origin(), RackA).Status);
    }

    [Fact]
    public void ALateCallbackWithTheWitnessOfAnExpiredTicketDoesNothingAndLeavesNoSecondOutcome()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Start();

        rig.ForeignCommand();
        Assert.Equal(EditUpdateStatus.Failed, rig.Take().Outcome.Status);
        rig.Kit.Primitives.RunDeferred();

        Assert.Equal(0, rig.Kit.Target.Locks);
        Assert.Equal(0, rig.Kit.Target.CreateWriters);
        Assert.Empty(rig.Writer.Calls);
        Assert.Equal(EditUpdateStatus.Unavailable, rig.Take().Outcome.Status);
        Assert.False(rig.Take().InFlight);
    }

    [Fact]
    public void ARetiredSessionTakesTheTicketAndTheStoreWithIt()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Start();
        Assert.True(rig.Take().InFlight);

        rig.Host.CloseActive(100);

        Assert.False(rig.Take().InFlight);
        Assert.Equal(EditUpdateStatus.Unavailable, rig.Take().Outcome.Status);
    }

    // ------------------------------------------------------------------ the bound of the ticket: complete or foreign command (decision of the Owner on C12)

    [Fact]
    public void TheEntriesIntoRestAndTheStartOfTheOwnCommandThatH0MeasuredBeforeTheCallbackDoNotExpireTheTicket()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.UseTheOrderMeasuredByH0();

        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
        rig.Kit.Primitives.RunCallback();

        // The callback ran and executed the update: nothing expired it.
        Assert.Equal(1, rig.Kit.Target.Locks);
        Assert.Equal(1, rig.WriterBegins);
        var progress = rig.Take();
        Assert.Equal(EditUpdateStatus.Applied, progress.Outcome.Status);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        rig.Kit.Primitives.Complete();
        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
    }

    [Fact]
    public void TheCompletionReleasesTheTicketAfterTheUpdateAndUntilThenARequestIsBusyAndTheProgressIsNotInFlight()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Start();
        rig.Kit.Primitives.RunCallback();

        var between = rig.Take();
        Assert.False(between.InFlight);
        Assert.Equal(EditUpdateStatus.Applied, between.Outcome.Status);
        Assert.Equal(EditUpdateStartStatus.Busy, rig.Start().Status);

        rig.Kit.Primitives.Complete();
        rig.Kit.Primitives.Complete();

        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
    }

    [Fact]
    public void ACompletionWithoutAnyCallbackLeavesAFailedOutcomeOnceAndReleasesTheTicket()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Start();

        rig.Kit.Primitives.Complete();

        var progress = rig.Take();
        Assert.False(progress.InFlight);
        Assert.Equal(EditUpdateStatus.Failed, progress.Outcome.Status);
        Assert.Equal(PushBackPilotHostTexts.HostNotRunReason, progress.Outcome.Reason);
        Assert.Equal(EditUpdateStatus.Unavailable, rig.Take().Outcome.Status);
        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
    }

    [Fact]
    public void ATicketThatNeverEndedAfterAnAdmissionExpiresWithTheNextForeignCommandAndAnUpdateInExecuteNeverExpires()
    {
        // An executor that admits the run and returns without a terminal call: the defense in depth of the bound.
        var rig = new Rig(executor: false);
        rig.Open();
        rig.Core.SetPilotUpdateHost(new AdmitOnlyExecutor());
        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
        Assert.True(rig.Take().InFlight);
        Assert.Equal(EditUpdateStartStatus.Busy, rig.Start().Status);

        rig.ForeignCommand();

        var expired = rig.Take();
        Assert.False(expired.InFlight);
        Assert.Equal(EditUpdateStatus.Failed, expired.Outcome.Status);

        // In Execute: a foreign command that arrives during the update does not expire it.
        var inside = new Rig();
        inside.Open();
        inside.Kit.Primitives.Mode = PilotHostMode.Deferred;
        inside.Writer.HookAt["Reread"] = () => inside.Host.Bridge.Core.NoteForeignCommandStart(inside.Session);
        inside.Start();
        inside.Kit.Primitives.RunCallback();
        Assert.Equal(EditUpdateStatus.Applied, inside.Take().Outcome.Status);
    }

    private sealed class AdmitOnlyExecutor : IPushBackPilotUpdateHost
    {
        public bool TryRequest(long documentNativeId, IPushBackPilotHostRun run, out string failure)
        {
            failure = string.Empty;
            return run.Admit(out _);
        }
    }

    [Fact]
    public void ASessionThatIsDegradedIsUnavailableAndOneDegradedBeforeTheCallbackExpiresTheTicket()
    {
        var degraded = new Rig();
        degraded.Open();
        degraded.Host.Bridge.Core.ReportFault(degraded.Session, "test", Boom("fault"));
        var start = degraded.Start();
        Assert.Equal(EditUpdateStartStatus.Unavailable, start.Status);
        Assert.Equal(0, degraded.Kit.Executor.TryRequests);

        var later = new Rig();
        later.Open();
        later.Kit.Primitives.Mode = PilotHostMode.Deferred;
        Assert.Equal(EditUpdateStartStatus.Accepted, later.Start().Status);
        later.Host.Bridge.Core.ReportFault(later.Session, "test", Boom("fault"));

        var progress = later.Take();
        Assert.False(progress.InFlight);
        Assert.Equal(EditUpdateStatus.Failed, progress.Outcome.Status);
        later.Kit.Primitives.RunDeferred();
        Assert.Empty(later.Writer.Calls);
    }

    [Fact]
    public void TheExpiryOfATicketOfTheSameRackInsideARequestIsTheStartValueFailedAndNothingIsAcceptedNorStored()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Never;
        rig.Start();
        rig.ForeignCommand();

        var start = rig.Start();

        Assert.Equal(EditUpdateStartStatus.Failed, start.Status);
        Assert.Equal(PushBackPilotHostTexts.HostNotRunReason, start.Reason);
        Assert.Equal(1, rig.Kit.Executor.TryRequests);
        var progress = rig.Take();
        Assert.False(progress.InFlight);
        Assert.Equal(EditUpdateStatus.Unavailable, progress.Outcome.Status);
        // The user presses again and it is accepted.
        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
        Assert.Equal(2, rig.Kit.Executor.TryRequests);
    }

    [Fact]
    public void TheExpiryOfATicketOfAnotherRackInsideARequestKeepsItsFailedUnderItsRackAndTheRequestGoesOn()
    {
        var rig = new Rig();
        rig.Open();
        rig.Reader.Answer = EditIntentKit.Admitted;
        rig.Open(edit: false, rackId: RackB);
        rig.Kit.Primitives.Mode = PilotHostMode.Never;
        rig.Start(RackA);
        rig.ForeignCommand();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;

        var start = rig.Start(RackB);

        Assert.Equal(EditUpdateStartStatus.Accepted, start.Status);
        var other = rig.Take(RackA);
        Assert.Equal(EditUpdateStatus.Failed, other.Outcome.Status);
        Assert.Equal(PushBackPilotHostTexts.HostNotRunReason, other.Outcome.Reason);
        Assert.Equal(EditUpdateStatus.Unavailable, rig.Take(RackA).Outcome.Status);
        Assert.True(rig.Take(RackB).InFlight);
    }

    // ------------------------------------------------------------------ the outcome of T4B is kept as it is (tests 12 to 16)

    [Fact]
    public void AConflictOfT4BIsKeptWithItsDifferencesAndTheHostWritesNothingMore()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Writer.RereadDrawing = Rig.Members().Select(m => m.Fact.DefinitionKey == "C3" ? IntentKit.Member("C3", PrepareKit.Planta("pb-02-modulos")) : m).ToArray();

        rig.Start();
        var outcome = rig.Take().Outcome;

        Assert.Equal(EditUpdateStatus.NotAppliedConflict, outcome.Status);
        Assert.Equal(new[] { "C3:EnvelopeText" }, outcome.Differences.ToArray());
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Empty(rig.Writer.Written);
        Assert.Equal(1, rig.Kit.Target.CreateWriters);
        Assert.Equal(1, rig.WriterBegins);
    }

    public static IEnumerable<object[]> TheOutcomesOfT4B()
    {
        foreach (var name in new[] { "Admission", "Preflight", "Failure", "CommitThrew", "PostFailed", "Applied", "Warnings" })
            yield return new object[] { name };
    }

    private static void Arrange(PilotUpdateScopeDouble writer, string name)
    {
        switch (name)
        {
            case "Admission": writer.NullCatalog = true; break;
            case "Preflight": writer.LockedAtPrepare = new[] { "B2" }; break;
            case "Failure": writer.ThrowAt["Reread"] = Boom("reread boom"); break;
            case "CommitThrew": writer.ThrowAt["Seal"] = Boom("seal boom"); break;
            case "PostFailed": writer.ThrowAt["Post"] = Boom("post boom"); break;
            case "Warnings": writer.MissingPieces["B2"] = new[] { "POSTE_X" }; break;
        }
    }

    [Theory]
    [MemberData(nameof(TheOutcomesOfT4B))]
    public void TheOutcomeThatT4BGivesIsKeptWithTheSameStatusReasonDifferencesAndWarningsAsThePlainCallOfTheUpdate(string name)
    {
        var direct = new Rig();
        direct.Open();
        Arrange(direct.Writer, name);
        var expected = direct.Core.TryUpdatePilot(direct.Request(), direct.Writer);

        var rig = new Rig();
        rig.Open();
        Arrange(rig.Writer, name);
        rig.Start();
        var kept = rig.Take().Outcome;

        Assert.Equal(expected.Status, kept.Status);
        Assert.Equal(expected.Reason, kept.Reason);
        Assert.Equal(expected.Differences.ToArray(), kept.Differences.ToArray());
        Assert.Equal(expected.Warnings.ToArray(), kept.Warnings.ToArray());
    }

    [Fact]
    public void AnUnknownOutcomeOfT4BIsNeverTranslatedAndAnUnlockThatThrowsAfterwardsDoesNotChangeIt()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Writer.ThrowAt["Seal"] = Boom("seal boom");
        rig.Kit.Target.ThrowOnUnlock = true;

        rig.Start();
        var outcome = rig.Take().Outcome;

        Assert.Equal(EditUpdateStatus.CommitThrew, outcome.Status);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.CommitThrew, draft.UnknownOrigin);
        Assert.Equal(1, rig.Kit.Target.LastLock!.Disposed);

        var post = new Rig();
        post.Open();
        post.Writer.ThrowAt["Post"] = Boom("post boom");
        post.Kit.Target.ThrowOnUnlock = true;
        post.Start();
        Assert.Equal(EditUpdateStatus.AppliedPostRereadFailed, post.Take().Outcome.Status);
    }

    [Fact]
    public void AnAppliedOutcomeKeepsTheDraftCleanOnlyByTheClassificationOfT4BAndTheWarningsAreCopied()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Writer.MissingPieces["B2"] = new[] { "POSTE_X" };

        rig.Start();
        var outcome = rig.Take().Outcome;

        Assert.Equal(EditUpdateStatus.AppliedWithWarnings, outcome.Status);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Equal(outcome.Warnings.ToArray(), draft.Warnings.ToArray());
        Assert.NotSame(draft.Warnings, outcome.Warnings);
    }

    // ------------------------------------------------------------------ the failures of the host after the admission (N-01)

    public static IEnumerable<object[]> TheHostFailures()
    {
        foreach (var name in new[] { "Lock", "Begin", "CreateWriter", "Retire", "Unlock" })
            yield return new object[] { name };
    }

    [Theory]
    [MemberData(nameof(TheHostFailures))]
    public void AHostFailureAfterTheAdmissionAlwaysRemovesTheTicketClosesTheOwnMutationIfItWasOpenedAndALaterRequestIsAccepted(string failure)
    {
        var rig = new Rig();
        var draft = rig.Open();
        switch (failure)
        {
            case "Lock": rig.Kit.Target.ThrowOnLock = true; break;
            case "Begin": rig.Host.Link.ThrowOnOpenOwnMutation = true; break;
            case "CreateWriter": rig.Kit.Target.ThrowOnCreateWriter = true; break;
            case "Retire": rig.Kit.Target.ThrowOnRetire = true; break;
            case "Unlock": rig.Kit.Target.ThrowOnUnlock = true; break;
        }

        var link = rig.Host.Link;
        var start = rig.Start();

        Assert.Equal(EditUpdateStartStatus.Accepted, start.Status);
        var outcome = rig.Take().Outcome;
        var ranTheUpdate = failure is "Retire" or "Unlock";
        Assert.Equal(ranTheUpdate ? EditUpdateStatus.Applied : EditUpdateStatus.Failed, outcome.Status);
        if (!ranTheUpdate) Assert.Equal(DraftStatus.Dirty, draft.Status);
        if (!ranTheUpdate) Assert.Empty(rig.Writer.Calls);
        if (failure == "Begin") Assert.Equal(1, rig.Kit.Target.Retires);
        if (failure == "CreateWriter")
        {
            Assert.Equal(0, rig.Kit.Target.Retires);
            Assert.True(link.CloseOwnMutationCalls >= 1);
        }

        rig.Host.Link.ThrowOnOpenOwnMutation = false;
        rig.Kit.Target.ThrowOnLock = false;
        rig.Kit.Target.ThrowOnCreateWriter = false;
        rig.Kit.Target.ThrowOnRetire = false;
        rig.Kit.Target.ThrowOnUnlock = false;
        Assert.Equal(EditUpdateStartStatus.Accepted, rig.Start().Status);
        Assert.False(rig.Take().InFlight);
    }

    [Fact]
    public void ALockThatThrowsLeavesAFailedOutcomeWithItsPrefixAndNeverOpensTheOwnMutation()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Target.ThrowOnLock = true;

        rig.Start();
        var outcome = rig.Take().Outcome;

        Assert.Equal(EditUpdateStatus.Failed, outcome.Status);
        Assert.StartsWith(PushBackPilotHostTexts.LockFailedPrefix, outcome.Reason, StringComparison.Ordinal);
        Assert.Equal(0, rig.Host.Link.OpenOwnMutationCalls);
        Assert.Empty(rig.Writer.Calls);
    }

    // ------------------------------------------------------------------ the own mutation of the run

    [Fact]
    public void TheRunOpensTheOwnMutationOnceBeforeTheUpdateAndAFailureBeforeTheUpdateClosesIt()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Target.ThrowOnCreateWriter = true;

        rig.Start();

        Assert.Equal(1, rig.Host.Link.OpenOwnMutationCalls);
        Assert.Equal(1, rig.Host.Link.CloseOwnMutationCalls);

        var ok = new Rig();
        ok.Open();
        ok.Start();
        Assert.Equal(1, ok.Host.Link.OpenOwnMutationCalls);
        // The own mutation of a run that updated is closed by the end of its command, never by the end of the callback.
        Assert.Equal(0, ok.Host.Link.CloseOwnMutationCalls);
    }

    // ------------------------------------------------------------------ the draft does not change during the flight (D-T4C-08) and R-T4-07 (D-T4C-10)

    private static (DraftStatus Status, string Pending, double Scale) Snapshot(DraftSession<PushBackPilotDraftState> draft) =>
        (draft.Status, string.Join("|", draft.PendingText.Select(p => p.Key + "=" + p.Value)), draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);

    [Fact]
    public void DuringTheFlightTheFiveOperationsThatCouldChangeTheDraftAnswerBusyOrWaitingAndTheDraftDoesNotChange()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Start();
        var before = Snapshot(draft);
        var reads = rig.Reader.Calls.Count;

        var set = rig.Port.SetPendingText(rig.Origin(), RackA, "4");
        var commit = rig.Port.CommitField(rig.Origin(), RackA);
        var keep = rig.Port.TryKeepMine(rig.Request());
        var discard = rig.Port.TryDiscardAndReload(rig.Request());
        var confirm = rig.Port.TryConfirmBase(rig.Request());

        Assert.Equal(PilotFieldStatus.Busy, set.Status);
        Assert.Equal(PushBackPilotHostTexts.UpdateInFlightReason, set.Reason);
        Assert.Equal(PilotFieldStatus.Busy, commit.Status);
        Assert.Equal(EditIntentStatus.Waiting, keep.Status);
        Assert.Equal(PushBackPilotHostTexts.UpdateInFlightReason, keep.Reason);
        Assert.Equal(EditIntentStatus.Waiting, discard.Status);
        Assert.Equal(EditConfirmStatus.Waiting, confirm.Status);
        Assert.Equal(PushBackPilotHostTexts.UpdateInFlightReason, confirm.Reason);
        Assert.Equal(before, Snapshot(draft));
        Assert.Equal(reads, rig.Reader.Calls.Count);

        // After the end they are admitted again.
        rig.Kit.Primitives.RunDeferred();
        rig.Take();
        Assert.NotEqual(PilotFieldStatus.Busy, rig.Port.SetPendingText(rig.Origin(), RackA, "4").Status);
    }

    [Fact]
    public void TheGeneralCheckDuringTheFlightWaitsBeforeItsRereadEvenWhenTheRereadWouldDifferAndTheDraftStaysDirty()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Start();
        rig.Reader.Answer = _ => EditIntentKit.ReadOf(PrepareKit.Scan(IntentKit.Member("A1", PrepareKit.Frontal("pb-02-modulos"))));
        var reads = rig.Reader.Calls.Count;

        var confirm = rig.Port.TryConfirmBase(rig.Request());

        Assert.Equal(EditConfirmStatus.Waiting, confirm.Status);
        Assert.Equal(reads, rig.Reader.Calls.Count);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Fact]
    public void AnotherRackOfTheSessionIsStillEditableDuringTheFlightOfTheFirst()
    {
        var rig = new Rig();
        rig.Open();
        rig.Reader.Answer = EditIntentKit.Admitted;
        rig.Open(edit: false, rackId: RackB);
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Start(RackA);

        Assert.Equal(PilotFieldStatus.Applied, rig.Port.SetPendingText(rig.Origin(), RackB, "3").Status);
    }

    [Fact]
    public void TheDiscardsAnswerBusyDuringTheFlightAndOnAnApplyingDraftAndTheRegistryIsUnchanged()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Deferred;
        rig.Start();
        var count = rig.Core.OpenDrafts.CountOf(rig.Session);

        var rack = rig.Port.DiscardRack(rig.Origin(), RackA);
        var document = rig.Port.DiscardDocument(rig.Origin());

        Assert.Equal(EditDiscardStatus.Busy, rack.Status);
        Assert.Equal(PushBackPilotHostTexts.UpdateInFlightReason, rack.Reason);
        Assert.Equal(EditDiscardStatus.Busy, document.Status);
        Assert.Equal(count, rig.Core.OpenDrafts.CountOf(rig.Session));
        Assert.Same(draft, rig.Core.OpenDrafts.Find<PushBackPilotDraftState>(rig.Session, RackA));

        // After the end the discards are served again.
        rig.Kit.Primitives.RunDeferred();
        rig.Take();
        Assert.Equal(EditDiscardStatus.Discarded, rig.Port.DiscardRack(rig.Origin(), RackA).Status);

        // A draft that is being applied, with no ticket at all, is not discarded either.
        var applying = new Rig();
        var other = applying.Open();
        Assert.True(other.TryBeginApply().IsStarted);
        Assert.Equal(EditDiscardStatus.Busy, applying.Port.DiscardRack(applying.Origin(), RackA).Status);
        Assert.Equal(EditDiscardStatus.Busy, applying.Port.DiscardDocument(applying.Origin()).Status);
        Assert.NotNull(applying.Core.OpenDrafts.Find<PushBackPilotDraftState>(applying.Session, RackA));
    }

    [Fact]
    public void TheDiscardsAreServedAgainWhenTheTicketExpiresWithAForeignCommand()
    {
        var rig = new Rig();
        rig.Open();
        rig.Kit.Primitives.Mode = PilotHostMode.Never;
        rig.Start();
        Assert.Equal(EditDiscardStatus.Busy, rig.Port.DiscardDocument(rig.Origin()).Status);

        rig.ForeignCommand();

        Assert.Equal(EditDiscardStatus.Discarded, rig.Port.DiscardDocument(rig.Origin()).Status);
    }

    // ------------------------------------------------------------------ the frontier of the update and the arrastres of T4B

    [Fact]
    public void TheUpdateBoundaryIsBlockingAndAppliesAValidPendingTextAndAbortsAnInvalidOneAndNeedsTheAuthorization()
    {
        var rig = new Rig();
        var draft = rig.Open(edit: false);

        var clean = rig.Port.CrossBoundary(rig.Origin(), RackA, PilotBoundaryKind.Update);
        Assert.Equal(PilotBoundaryKind.Update, clean.Kind);
        Assert.Equal(PilotBoundaryDecision.Proceeded, clean.Decision);

        Assert.Equal(PilotFieldStatus.Applied, rig.Port.SetPendingText(rig.Origin(), RackA, "3").Status);
        var valid = rig.Port.CrossBoundary(rig.Origin(), RackA, PilotBoundaryKind.Update);
        Assert.Equal(PilotBoundaryDecision.Proceeded, valid.Decision);
        Assert.True(valid.StateChanged);
        Assert.Equal(3.0, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
        Assert.False(draft.HasPendingText);

        Assert.Equal(PilotFieldStatus.Applied, rig.Port.SetPendingText(rig.Origin(), RackA, "no es un numero").Status);
        var invalid = rig.Port.CrossBoundary(rig.Origin(), RackA, PilotBoundaryKind.Update);
        Assert.Equal(PilotBoundaryDecision.Aborted, invalid.Decision);
        Assert.NotEmpty(invalid.InvalidFields);

        var wrong = rig.Port.CrossBoundary(rig.Origin(new SessionId(777)), RackA, PilotBoundaryKind.Update);
        Assert.Equal(PilotBoundaryDecision.Aborted, wrong.Decision);
        Assert.Equal(
            new[] { "TabChange", "RackChange", "DocumentChange", "PanelHide", "Dematerialize", "DocumentClose", "Update" },
            Enum.GetNames<PilotBoundaryKind>());
    }

    [Fact]
    public void TheTextsOfTheGateOfTheUpdateAreTheConstantsOfTheRunWithTheirOldWording()
    {
        var modal = new Rig();
        modal.Open();
        modal.Core.SetModalRackCadActive(true);
        var rejected = modal.Core.TryUpdatePilot(modal.Request(), modal.Writer);
        Assert.Equal(EditUpdateStatus.Rejected, rejected.Status);
        Assert.Equal(PushBackPilotUpdateRun.ModalOpenReason, rejected.Reason);

        var none = new Rig();
        none.Open();
        none.Host.Probe.ActiveDocumentNativeId = null;
        var waiting = none.Core.TryUpdatePilot(none.Request(), none.Writer);
        Assert.Equal(PushBackPilotUpdateRun.NoActiveDocumentReason, waiting.Reason);
    }

    // ------------------------------------------------------------------ the answer of a close that was not accepted is never a success (M-01)

    [Fact]
    public void AnUpdateOfTheCoreThatRunsOverASlotThatWasTakenAwayNeverAnswersAnApplied()
    {
        // The slot is taken from the draft while the run is in its write: CompleteApply finds no update to close (NotApplying).
        var rig = new Rig();
        var draft = rig.Open();
        rig.Writer.HookAt["Seal"] = () => Assert.True(draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure("taken away")).Accepted);

        var outcome = rig.Core.TryUpdatePilot(rig.Request(), rig.Writer);

        Assert.NotEqual(EditUpdateStatus.Applied, outcome.Status);
        Assert.NotEqual(EditUpdateStatus.AppliedWithWarnings, outcome.Status);
        Assert.Equal(EditUpdateStatus.ClosedInError, outcome.Status);
        Assert.Equal(EditUpdateStatus.Applied, outcome.Classification);
        Assert.Equal(PushBackPilotHostTexts.UpdateNotClosedReason, outcome.Reason);
    }
}
