#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F4-T2B2A test double of <see cref="IWorkspaceEditIntentReader"/>: it records every call and answers with the real read of
/// the pilot over a scan built in memory (the pure reader of F4-T2B1), or with whatever the test programs.
/// </summary>
internal sealed class EditIntentFakeReader : IWorkspaceEditIntentReader
{
    public List<(SessionId Session, string RackId, long? ChosenReference, EditIntentWitness? Witness)> Calls { get; } = new();

    /// <summary>The answer by rack; by default the admitted read of two views of the rack.</summary>
    public Func<string, EditIntentReadResult> Answer { get; set; } = EditIntentKit.Admitted;

    public Exception? Throw { get; set; }

    public EditIntentReadResult ReadIntent(SessionId session, string rackId, long? chosenReference, EditIntentWitness? witness)
    {
        Calls.Add((session, rackId, chosenReference, witness));
        if (Throw != null) throw Throw;
        return Answer(rackId);
    }
}

/// <summary>I-64 F4-T2B2A: the reads of the pilot that the double answers with, built from the fixtures of F4-T2A.</summary>
internal static class EditIntentKit
{
    public const string RackA = IntentKit.Rack;
    public const string RackB = "22222222-2222-4222-8222-000000000009";

    /// <summary>Two views of the same single-sided design for the rack: admitted by the real reader.</summary>
    public static PushBackPilotIntentScan TwoViews(string rackId, PushBackPilotPreviewWitness? witness = null) =>
        IntentKit.Scan(
            rackId,
            "A1",
            witness,
            IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rackId)),
            IntentKit.Member("B2", IntentKit.Variant("pb-01-base", rackId, view: "lateral", section: 3)));

    public static EditIntentReadResult Admitted(string rackId) => ReadOf(TwoViews(rackId));

    public static EditIntentReadResult ReadOf(PushBackPilotIntentScan scan) =>
        EditIntentReadResult.FromPilot(PushBackPilotIntentReader.Read(scan, PilotKit.Catalog));

    /// <summary>A read that is not admitted (the origin is not in the scan) but carries its base.</summary>
    public static EditIntentReadResult OriginMissing(string rackId) =>
        ReadOf(IntentKit.Scan(rackId, "ZZ", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rackId))));

    /// <summary>The same rack, but a member became unreadable: the base differs from the one of <see cref="Admitted"/>.</summary>
    public static EditIntentReadResult MemberBecameBlocking(string rackId) =>
        ReadOf(IntentKit.Scan(rackId, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rackId)), IntentKit.Unreadable("B2", rackId)));
}

// I-64 F4-T2B2A-INTENT-WIRING (D-T2B2-03, 04, 05, 06, 08): the host core opens and checks the edit intention of the pilot with ONE read
// of a reader that the test doubles, authorizes every request against the active document, creates a draft only from an admitted
// read and never calls anything from the drain. Evidence named by the package: obligation 4 (one intention is one read), obligation 11
// ("check now" rereads authored data), obligation 13 (the session of A never operates on B) and R6 (no write to the drawing: the
// guards of WorkspaceEditIntentGuardTests).
public class WorkspaceEditIntentHostTests
{
    private const string RackA = EditIntentKit.RackA;
    private const string RackB = EditIntentKit.RackB;

    private sealed class Rig
    {
        public Rig(bool assignReader = true)
        {
            Host = new WorkspaceHostRig().WithDocument(100);
            Host.Host.OpenPanel();
            Session = Host.ActiveSession;
            Host.Idle();
            Host.Reader.Reads.Clear();
            Reader = new EditIntentFakeReader();
            if (assignReader) Host.Host.SetEditIntentReader(Reader);
        }

        public WorkspaceHostRig Host { get; }

        public WorkspaceHostCore Core => Host.Host;

        public EditIntentFakeReader Reader { get; }

        public SessionId Session { get; }

        public EditIntentRequest Request(string rackId = RackA, long? chosen = null, EditIntentWitness? witness = null, SessionId? session = null) =>
            new(session ?? Session, rackId, chosen, witness);

        public DraftSession<PushBackPilotDraftState>? Draft(string rackId = RackA, SessionId? session = null) =>
            Core.OpenDrafts.Find<PushBackPilotDraftState>(session ?? Session, rackId);

        /// <summary>The draft of the rack, taken to Unknown after a commit that threw (the one way to Unknown that "check now" serves).</summary>
        public DraftSession<PushBackPilotDraftState> UnknownDraft(string rackId = RackA)
        {
            var intent = Core.TryBeginPilotEdit(Request(rackId));
            Assert.Equal(EditIntentStatus.Admitted, intent.Status);
            var draft = Draft(rackId)!;
            Assert.True(draft.TryBeginApply().IsStarted);
            draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("the commit threw"));
            Assert.Equal(DraftStatus.Unknown, draft.Status);
            Assert.Equal(DraftUnknownOrigin.CommitThrew, draft.UnknownOrigin);
            return draft;
        }
    }

    // ------------------------------------------------------------------ obligation 4: one intention is one read

    [Fact]
    public void AnIntentionIsOneReadOfTheReaderAndAnAdmittedReadCreatesTheDraftFromThatSameRead()
    {
        var rig = new Rig();

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
        Assert.True(outcome.IsAdmitted);
        Assert.Null(outcome.Diagnostic);
        Assert.Null(outcome.ClassicEditorHint);
        var call = Assert.Single(rig.Reader.Calls);
        Assert.Equal(rig.Session, call.Session);
        Assert.Equal(RackA, call.RackId);

        var draft = rig.Draft();
        Assert.NotNull(draft);
        Assert.Equal(DraftStatus.Clean, draft!.Status);
        Assert.Equal(RackA, draft.RackId);
        Assert.Equal(rig.Session, draft.Session);
        var read = EditIntentKit.Admitted(RackA).PilotRead!;
        Assert.True(draft.StaleBase.CompareWith(read.StaleBase).IsMatch);
        Assert.Equal(2, draft.StaleBase.Entries.Count);
    }

    [Fact]
    public void TheDraftIsBornWithTheComparerAndTheCopierOfThePilot()
    {
        var rig = new Rig();
        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());
        Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
        var draft = rig.Draft()!;

        // Clean means "the current state equals the initial one for the comparer of the pilot", and neither is the object of the read.
        Assert.False(draft.IsDirty);
        Assert.True(PushBackPilotStateComparer.Instance.Equals(draft.CurrentDraftState, draft.InitialDraftState));
        Assert.NotSame(draft.CurrentDraftState, draft.InitialDraftState);
        var origin = draft.CurrentDraftState.Origin;
        Assert.Equal(IntentKit.Variant("pb-01-base", RackA), origin);

        // The editable content decides Dirty (INV-26): changing the pending text makes it Dirty, and that is not a raw JSON comparison.
        draft.SetPendingText("field", "1");
        Assert.True(draft.IsDirty);
    }

    [Fact]
    public void TheRequestReachesTheReaderUnchangedWithTheChosenReferenceAndTheWitness()
    {
        var rig = new Rig();
        var witness = new EditIntentWitness("A1", "{ preview text");

        rig.Core.TryBeginPilotEdit(rig.Request(chosen: 0x102, witness: witness));

        var call = Assert.Single(rig.Reader.Calls);
        Assert.Equal(0x102L, call.ChosenReference);
        Assert.Same(witness, call.Witness);

        var other = new Rig();
        other.Core.TryBeginPilotEdit(other.Request());
        var plain = Assert.Single(other.Reader.Calls);
        Assert.Null(plain.ChosenReference);
        Assert.Null(plain.Witness);
    }

    [Fact]
    public void TheIntentionReadsNothingByItselfAndTheDrawingIsReadOnlyThroughTheReader()
    {
        var rig = new Rig();

        rig.Core.TryBeginPilotEdit(rig.Request());

        // The fine selection reader of the drain is a different reader and the intention never calls it.
        Assert.Equal(0, rig.Host.Reader.ReadCalls);
        Assert.Empty(rig.Host.Reader.Invalidations);
        Assert.Empty(rig.Host.Faults);
    }

    // ------------------------------------------------------------------ the states before the read: nothing is read

    [Fact]
    public void WithoutAReaderTheIntentionIsUnavailableAndReadsNothing()
    {
        var rig = new Rig(assignReader: false);

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Unavailable, outcome.Status);
        Assert.Equal(EditWitnessStatus.NotEvaluated, outcome.Witness);
        Assert.Empty(rig.Reader.Calls);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(rig.Session));
    }

    [Fact]
    public void ABlankRackIdIsNotAdmittedWithNoIdentityBeforeAnythingElseAndTheRegistryIsNeverTouched()
    {
        foreach (var assign in new[] { true, false })
        {
            foreach (var blank in new[] { "", " ", "\t"})
            {
                var rig = new Rig(assign);

                var outcome = rig.Core.TryBeginPilotEdit(rig.Request(blank));

                Assert.Equal(EditIntentStatus.NotAdmitted, outcome.Status);
                Assert.Equal(PushBackPilotIntentReader.NoIdentityDiagnostic, outcome.Diagnostic);
                Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
                Assert.Equal(EditWitnessStatus.NotEvaluated, outcome.Witness);
                Assert.Empty(rig.Reader.Calls);
                Assert.Empty(rig.Host.Faults);
                Assert.Equal(0, rig.Core.OpenDrafts.CountOf(rig.Session));
            }
        }
    }

    [Fact]
    public void WithNoActiveDocumentTheIntentionWaitsAndReadsNothing()
    {
        var rig = new Rig();
        rig.Host.Probe.ActiveDocumentNativeId = null;

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Waiting, outcome.Status);
        Assert.False(string.IsNullOrWhiteSpace(outcome.Reason));
        Assert.Equal(EditWitnessStatus.NotEvaluated, outcome.Witness);
        Assert.Empty(rig.Reader.Calls);
    }

    [Fact]
    public void WhileARackCadModalIsOpenTheIntentionWaitsAndReadsNothing()
    {
        var rig = new Rig();
        rig.Core.SetModalRackCadActive(true);

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Waiting, outcome.Status);
        Assert.Empty(rig.Reader.Calls);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(rig.Session));

        rig.Core.SetModalRackCadActive(false);
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request()).Status);
    }

    [Fact]
    public void WhileACommandRunsOrTheDocumentIsNotAtRestTheIntentionWaitsAndReadsNothing()
    {
        var rig = new Rig();
        rig.Host.CommandStarts(rig.Session);

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Waiting, outcome.Status);
        Assert.Empty(rig.Reader.Calls);

        rig.Host.CommandEnds(rig.Session);
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request()).Status);
        Assert.Single(rig.Reader.Calls);
    }

    [Fact]
    public void AnOpenDraftIsAlreadyOpenAndIsNeverReadAgain()
    {
        var rig = new Rig();
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request()).Status);
        var draft = rig.Draft()!;
        draft.SetPendingText("field", "keep me");

        var again = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.AlreadyOpen, again.Status);
        Assert.Equal(EditWitnessStatus.NotEvaluated, again.Witness);
        Assert.Single(rig.Reader.Calls);
        Assert.Same(draft, rig.Draft());
        Assert.Equal("keep me", draft.PendingText["field"]);
    }

    // ------------------------------------------------------------------ a draft only from an admitted read

    [Fact]
    public void AReadThatIsNotAdmittedCreatesNoDraftAndCarriesTheDiagnosticTheDetailAndTheClassicEditorHint()
    {
        var rig = new Rig();
        rig.Reader.Answer = EditIntentKit.OriginMissing;

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.NotAdmitted, outcome.Status);
        Assert.False(outcome.IsAdmitted);
        Assert.Equal(PushBackPilotIntentReader.OriginMissingDiagnostic, outcome.Diagnostic);
        Assert.Null(outcome.Detail);
        Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
        Assert.Single(rig.Reader.Calls);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(rig.Session));
        Assert.Null(rig.Draft());
        Assert.False(rig.Core.OpenDrafts.Contains(rig.Session, RackA));
    }

    [Fact]
    public void TheTechnicalDetailOfTheDiagnosticTravelsApartFromItsMainText()
    {
        var rig = new Rig();
        rig.Reader.Answer = rack => EditIntentKit.ReadOf(IntentKit.Scan(rack, "A1", IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rack, kind: "selective"))));

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.NotAdmitted, outcome.Status);
        Assert.Equal(PushBackPilotIntentReader.OutOfPilotDiagnostic, outcome.Diagnostic);
        Assert.Equal("selective", outcome.Detail);
        Assert.DoesNotContain("selective", outcome.Diagnostic!);
        Assert.Equal(PushBackPilotIntentReader.ClassicEditorHint, outcome.ClassicEditorHint);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(rig.Session));
    }

    [Fact]
    public void ARackThatWasNotAdmittedCanBeAdmittedLaterWhenTheDrawingAllowsIt()
    {
        var rig = new Rig();
        rig.Reader.Answer = EditIntentKit.OriginMissing;
        Assert.Equal(EditIntentStatus.NotAdmitted, rig.Core.TryBeginPilotEdit(rig.Request()).Status);

        rig.Reader.Answer = EditIntentKit.Admitted;
        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
        Assert.Equal(2, rig.Reader.Calls.Count);
        Assert.NotNull(rig.Draft());
    }

    [Fact]
    public void TheRegistryIsOneForEverySessionWithKeysOfSessionAndRack()
    {
        var rig = new Rig();
        var a = rig.Session;
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request(RackA)).Status);
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request(RackB)).Status);
        var b = rig.Host.Activate(200);
        rig.Core.RefreshPanel();
        rig.Host.Idle();

        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request(RackA, session: b)).Status);

        Assert.Equal(2, rig.Core.OpenDrafts.CountOf(a));
        Assert.Equal(1, rig.Core.OpenDrafts.CountOf(b));
        Assert.NotSame(rig.Draft(RackA, a), rig.Draft(RackA, b));
        Assert.Equal(new[] { RackA, RackB }.OrderBy(r => r, StringComparer.Ordinal).ToArray(), rig.Core.OpenDrafts.RackIdsOf(a).ToArray());
        Assert.Equal(new[] { RackA }, rig.Core.OpenDrafts.RackIdsOf(b).ToArray());
    }

    [Fact]
    public void ARetiredSessionNeverGetsADraftAndTheRequestIsRejected()
    {
        var rig = new Rig();
        rig.Core.OpenDrafts.RetireSession(rig.Session);

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Rejected, outcome.Status);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(rig.Session));
        Assert.Null(rig.Draft());
    }

    [Fact]
    public void TheHostOwnsTheClosePolicyOfItsOneRegistry()
    {
        var rig = new Rig();
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request()).Status);

        Assert.Equal(DraftCloseVerdict.Allow, rig.Core.ClosePolicy.EvaluateClose(rig.Session).Verdict);
        rig.Draft()!.SetPendingText("field", "unsaved");

        var decision = rig.Core.ClosePolicy.EvaluateClose(rig.Session);
        Assert.Equal(DraftCloseVerdict.Veto, decision.Verdict);
        Assert.Equal(new[] { RackA }, decision.RackIds.ToArray());
    }

    // ------------------------------------------------------------------ what the reader answers when it does not read

    [Theory]
    [InlineData(EditIntentReadStatus.NoActiveDocument, EditIntentStatus.Waiting)]
    [InlineData(EditIntentReadStatus.Busy, EditIntentStatus.Waiting)]
    [InlineData(EditIntentReadStatus.Rejected, EditIntentStatus.Rejected)]
    [InlineData(EditIntentReadStatus.Failed, EditIntentStatus.Failed)]
    public void EveryAnswerOfTheReaderThatIsNotARead_IsMappedToItsStateAndCreatesNoDraft(EditIntentReadStatus read, EditIntentStatus expected)
    {
        var rig = new Rig();
        rig.Reader.Answer = _ => read switch
        {
            EditIntentReadStatus.NoActiveDocument => EditIntentReadResult.NoActiveDocument("no document"),
            EditIntentReadStatus.Busy => EditIntentReadResult.Busy("busy"),
            EditIntentReadStatus.Rejected => EditIntentReadResult.Rejected("not yours"),
            _ => EditIntentReadResult.Failed("could not read"),
        };

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(expected, outcome.Status);
        Assert.Equal(EditWitnessStatus.NotEvaluated, outcome.Witness);
        Assert.Contains(outcome.Reason, new[] { "no document", "busy", "not yours", "could not read" });
        Assert.Single(rig.Reader.Calls);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(rig.Session));
        Assert.Empty(rig.Host.Faults);
    }

    [Fact]
    public void AnEmptyAnswerOfTheReaderIsAFailureAndNeverAnAdmission()
    {
        var rig = new Rig();
        rig.Reader.Answer = _ => default;

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Failed, outcome.Status);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(rig.Session));
    }

    // ------------------------------------------------------------------ D-17: a failure is contained and does not degrade the session

    [Fact]
    public void AnExceptionOfTheReaderIsContainedAsFailedLoggedAndDoesNotDegradeTheSession()
    {
        var rig = new Rig();
        rig.Reader.Throw = new InvalidOperationException("reader boom");

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Failed, outcome.Status);
        Assert.Equal("reader boom", outcome.Reason);
        var fault = Assert.Single(rig.Host.Faults);
        Assert.Equal("WorkspaceHostCore.TryBeginPilotEdit", fault.Context);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(rig.Session));

        // The session is not faulted: the drain still reads the selection of the active session.
        rig.Host.SelectionChanged(rig.Session);
        rig.Host.Idle();
        Assert.Equal(1, rig.Host.Reader.ReadCalls);
        Assert.Equal(WorkspaceHostStatus.Ready, rig.Core.GetSnapshot().Status);
    }

    [Fact]
    public void ANullRequestIsContainedAsFailedAndNeverThrows()
    {
        var rig = new Rig();

        var intent = Record.Exception(() => rig.Core.TryBeginPilotEdit(null!));
        var check = Record.Exception(() => rig.Core.TryCheckPilotEdit(null!));

        Assert.Null(intent);
        Assert.Null(check);
        Assert.Equal(EditIntentStatus.Failed, rig.Core.TryBeginPilotEdit(null!).Status);
        Assert.Equal(EditCheckStatus.Failed, rig.Core.TryCheckPilotEdit(null!).Status);
        Assert.Empty(rig.Reader.Calls);
    }

    [Fact]
    public void AFailingLogDoesNotBecomeASecondFailure()
    {
        var rig = new Rig();
        rig.Reader.Throw = new InvalidOperationException("reader boom");
        var core = new WorkspaceHostCore(rig.Host.Link, rig.Host.Probe, rig.Host.Reader, new WorkspaceInstrumentation(), (_, _) => throw new InvalidOperationException("log boom"));
        core.SetEditIntentReader(rig.Reader);

        var outcome = Record.Exception(() => core.TryBeginPilotEdit(rig.Request()));

        Assert.Null(outcome);
        Assert.Equal(EditIntentStatus.Failed, core.TryBeginPilotEdit(rig.Request()).Status);
    }

    [Fact]
    public void TheReaderCanBeAssignedAgainAndOnlyANullReaderIsRefused()
    {
        var rig = new Rig();
        var second = new EditIntentFakeReader();
        rig.Core.SetEditIntentReader(second);

        rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Empty(rig.Reader.Calls);
        Assert.Single(second.Calls);
        Assert.Throws<ArgumentNullException>(() => rig.Core.SetEditIntentReader(null!));
    }

    // ------------------------------------------------------------------ INV-22 and obligation 13: the session of A never operates on B

    [Fact]
    public void ARequestOfTheSessionOfADocumentThatIsNotTheActiveOneIsRejectedWithZeroReadsAndNoDraft()
    {
        var rig = new Rig();
        var a = rig.Session;
        var b = rig.Host.Activate(200);
        rig.Core.RefreshPanel();
        rig.Host.Idle();
        rig.Host.Reader.Reads.Clear();

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request(session: a));

        Assert.Equal(EditIntentStatus.Rejected, outcome.Status);
        Assert.Equal(EditWitnessStatus.NotEvaluated, outcome.Witness);
        Assert.Empty(rig.Reader.Calls);
        Assert.Equal(0, rig.Host.Reader.ReadCalls);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(a));
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(b));

        // The session of the active document is served, and still only for itself.
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request(session: b)).Status);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(a));
        Assert.Equal(1, rig.Core.OpenDrafts.CountOf(b));
    }

    [Fact]
    public void ARecycledNativeIdentityNeverRevivesTheOldSessionForTheIntention()
    {
        var rig = new Rig();
        var old = rig.Session;
        rig.Host.CloseActive(100);

        // No document is active right now: the intention waits.
        Assert.Equal(EditIntentStatus.Waiting, rig.Core.TryBeginPilotEdit(rig.Request(session: old)).Status);

        var fresh = rig.Host.Activate(100);
        Assert.NotEqual(old, fresh);

        var oldOutcome = rig.Core.TryBeginPilotEdit(rig.Request(session: old));
        Assert.Equal(EditIntentStatus.Rejected, oldOutcome.Status);
        Assert.Empty(rig.Reader.Calls);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(old));

        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request(session: fresh)).Status);
        Assert.Single(rig.Reader.Calls);
    }

    [Fact]
    public void ASessionThatNeverExistedIsRejectedWithoutAReadOrADraft()
    {
        var rig = new Rig();
        var ghost = new SessionId(9999);

        var intent = rig.Core.TryBeginPilotEdit(rig.Request(session: ghost));
        var check = rig.Core.TryCheckPilotEdit(rig.Request(session: ghost));

        Assert.Equal(EditIntentStatus.Rejected, intent.Status);
        Assert.Equal(EditCheckStatus.Rejected, check.Status);
        Assert.Empty(rig.Reader.Calls);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(ghost));
    }

    [Fact]
    public void WithADraftOfSessionAAndBActiveTheIntentionOfARejectsBeforeAnythingAboutTheDraftAndReadsNothing()
    {
        // M-9: the authorization (step 5) comes before the open draft (step 6), so the session that is not the active one is
        // Rejected and not AlreadyOpen, even for a rack that has a draft in its own session.
        var rig = new Rig();
        var a = rig.Session;
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request(session: a)).Status);
        var draft = rig.Draft(RackA, a)!;
        var reads = rig.Reader.Calls.Count;
        var b = rig.Host.Activate(200);
        rig.Core.RefreshPanel();
        rig.Host.Idle();

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request(RackA, session: a));

        Assert.Equal(EditIntentStatus.Rejected, outcome.Status);
        Assert.NotEqual(EditIntentStatus.AlreadyOpen, outcome.Status);
        Assert.False(string.IsNullOrWhiteSpace(outcome.Reason));
        Assert.Equal(EditWitnessStatus.NotEvaluated, outcome.Witness);
        Assert.Equal(reads, rig.Reader.Calls.Count);
        Assert.Same(draft, rig.Draft(RackA, a));
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(b));
    }

    // ------------------------------------------------------------------ M-3: a witness without a key is a failure of the request, before anything is read

    public static IEnumerable<object?[]> WitnessKeysWithoutAValue() => new[]
    {
        new object?[] { null },
        new object?[] { "" },
        new object?[] { "  " },
        new object?[] { "\t" },
    };

    [Theory]
    [MemberData(nameof(WitnessKeysWithoutAValue))]
    public void APresentWitnessWithoutAKeyIsAFailureOfTheIntentionWithAReasonAndReadsNothingAndTouchesNoRegistry(string? key)
    {
        var rig = new Rig();

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request(witness: new EditIntentWitness(key!, "{ preview")));

        Assert.Equal(EditIntentStatus.Failed, outcome.Status);
        Assert.False(string.IsNullOrWhiteSpace(outcome.Reason));
        Assert.Equal(EditWitnessStatus.NotEvaluated, outcome.Witness);
        Assert.Empty(rig.Reader.Calls);
        Assert.Equal(0, rig.Core.OpenDrafts.CountOf(rig.Session));
        Assert.Null(rig.Draft());
        Assert.False(rig.Core.OpenDrafts.Contains(rig.Session, RackA));

        // The same request without a witness, or with a witness that has a key, is served as usual.
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request(RackB, witness: new EditIntentWitness("A1", null))).Status);
        Assert.Single(rig.Reader.Calls);
    }

    [Theory]
    [MemberData(nameof(WitnessKeysWithoutAValue))]
    public void APresentWitnessWithoutAKeyIsAFailureOfCheckNowWithAReasonAndReadsNothingAndLeavesTheUnknownDraftAsItWas(string? key)
    {
        var rig = new Rig();
        var draft = rig.UnknownDraft();
        var reads = rig.Reader.Calls.Count;

        var outcome = rig.Core.TryCheckPilotEdit(rig.Request(witness: new EditIntentWitness(key!, "{ preview")));

        Assert.Equal(EditCheckStatus.Failed, outcome.Status);
        Assert.False(string.IsNullOrWhiteSpace(outcome.Reason));
        Assert.Empty(outcome.Differences);
        Assert.Equal(reads, rig.Reader.Calls.Count);
        Assert.Same(draft, rig.Draft());
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.CommitThrew, draft.UnknownOrigin);
    }

    // ------------------------------------------------------------------ INV-27: the witness of the preview

    [Fact]
    public void WithoutAWitnessTheStatusIsNotSuppliedAndNeverUnchanged()
    {
        var rig = new Rig();

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request());

        Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
        Assert.Equal(EditWitnessStatus.NotSupplied, outcome.Witness);
        Assert.NotEqual(EditWitnessStatus.Unchanged, outcome.Witness);
    }

    [Fact]
    public void AWitnessThatMatchesTheReadIsUnchanged()
    {
        var rig = new Rig();
        var text = IntentKit.Variant("pb-01-base", RackA);
        rig.Reader.Answer = rack => EditIntentKit.ReadOf(EditIntentKit.TwoViews(rack, new PushBackPilotPreviewWitness("A1", text)));

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request(witness: new EditIntentWitness("A1", text)));

        Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
        Assert.Equal(EditWitnessStatus.Unchanged, outcome.Witness);
    }

    [Fact]
    public void AChangedWitnessIsReportedAsChangedAndNoKeystrokeIsAppliedToTheNewDraft()
    {
        var rig = new Rig();
        rig.Reader.Answer = rack => EditIntentKit.ReadOf(EditIntentKit.TwoViews(rack, new PushBackPilotPreviewWitness("A1", "{ stale preview")));

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request(witness: new EditIntentWitness("A1", "{ stale preview")));

        // The surface is re-rendered from the new read: the draft is born from THAT read, Clean, with no pending text.
        Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
        Assert.Equal(EditWitnessStatus.Changed, outcome.Witness);
        var draft = rig.Draft()!;
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.False(draft.IsDirty);
        Assert.False(draft.HasPendingText);
        Assert.Empty(draft.PendingText);
        Assert.True(PushBackPilotStateComparer.Instance.Equals(draft.CurrentDraftState, draft.InitialDraftState));
        Assert.Equal(IntentKit.Variant("pb-01-base", RackA), draft.CurrentDraftState.Origin);
    }

    [Fact]
    public void TheWitnessIsAlsoReportedWhenTheReadIsNotAdmitted()
    {
        var rig = new Rig();
        rig.Reader.Answer = rack => EditIntentKit.ReadOf(IntentKit.Scan(
            rack,
            "ZZ",
            new PushBackPilotPreviewWitness("ZZ", "{ preview"),
            IntentKit.Member("A1", IntentKit.Variant("pb-01-base", rack))));

        var outcome = rig.Core.TryBeginPilotEdit(rig.Request(witness: new EditIntentWitness("ZZ", "{ preview")));

        Assert.Equal(EditIntentStatus.NotAdmitted, outcome.Status);
        Assert.Equal(EditWitnessStatus.Changed, outcome.Witness);
    }

    [Fact]
    public void EveryOutcomeWithoutAReadCarriesNotEvaluatedForTheWitnessAndTheEnumHasTheFourValues()
    {
        Assert.Equal(new[] { "NotEvaluated", "NotSupplied", "Unchanged", "Changed" }, Enum.GetNames<EditWitnessStatus>());
        Assert.Equal(EditWitnessStatus.NotEvaluated, EditIntentOutcome.Unavailable("x").Witness);
        Assert.Equal(EditWitnessStatus.NotEvaluated, EditIntentOutcome.Waiting("x").Witness);
        Assert.Equal(EditWitnessStatus.NotEvaluated, EditIntentOutcome.Rejected("x").Witness);
        Assert.Equal(EditWitnessStatus.NotEvaluated, EditIntentOutcome.AlreadyOpen().Witness);
        Assert.Equal(EditWitnessStatus.NotEvaluated, EditIntentOutcome.Failed("x").Witness);
        Assert.Equal(EditWitnessStatus.NotEvaluated, default(EditIntentOutcome).Witness);
        Assert.Equal(EditIntentStatus.Unavailable, default(EditIntentOutcome).Status);
    }

    [Theory]
    [InlineData(PushBackPilotPreviewWitnessStatus.NotSupplied, EditWitnessStatus.NotSupplied)]
    [InlineData(PushBackPilotPreviewWitnessStatus.Unchanged, EditWitnessStatus.Unchanged)]
    [InlineData(PushBackPilotPreviewWitnessStatus.Changed, EditWitnessStatus.Changed)]
    public void TheWitnessStatusOfThePilotIsTranslatedToTheOneOfTheHost(PushBackPilotPreviewWitnessStatus pilot, EditWitnessStatus expected)
    {
        Assert.Equal(expected, WorkspaceEditIntent.WitnessStatusOf(pilot));
    }

    [Fact]
    public void AnUnknownStatusOfThePilotIsNeverTranslatedToUnchanged()
    {
        Assert.Equal(EditWitnessStatus.Changed, WorkspaceEditIntent.WitnessStatusOf((PushBackPilotPreviewWitnessStatus)99));
    }

    // ------------------------------------------------------------------ "check now" (obligation 11): the reread of authored data

    [Fact]
    public void CheckNowRecoversAnUnknownDraftWhenTheRereadMatchesTheBaseFromBeforeTheAttempt()
    {
        var rig = new Rig();
        var draft = rig.UnknownDraft();
        var before = rig.Reader.Calls.Count;

        var outcome = rig.Core.TryCheckPilotEdit(rig.Request());

        Assert.Equal(EditCheckStatus.Recovered, outcome.Status);
        Assert.Empty(outcome.Differences);
        Assert.Equal(before + 1, rig.Reader.Calls.Count);
        Assert.NotEqual(DraftStatus.Unknown, draft.Status);
        Assert.NotEqual(DraftStatus.Conflict, draft.Status);
        Assert.Same(draft, rig.Draft());
    }

    [Fact]
    public void CheckNowTakesTheUnknownDraftToConflictWhenTheRereadDiffersAndNamesWhatDiffers()
    {
        var rig = new Rig();
        var draft = rig.UnknownDraft();
        rig.Reader.Answer = EditIntentKit.MemberBecameBlocking;

        var outcome = rig.Core.TryCheckPilotEdit(rig.Request());

        Assert.Equal(EditCheckStatus.Conflict, outcome.Status);
        Assert.Contains("B2:Membership", outcome.Differences);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.All(outcome.Differences, line => Assert.StartsWith("B2:", line));
    }

    [Fact]
    public void CheckNowUsesTheBaseOfARereadThatIsNotAdmittedToo()
    {
        // The base is total and independent of the admission: a reread without admission still says whether the drawing changed.
        var rig = new Rig();
        var draft = rig.UnknownDraft();
        var without = PushBackPilotIntentReader.Read(EditIntentKit.TwoViews(RackA), null);
        Assert.False(without.Read.IsAdmitted);
        rig.Reader.Answer = _ => EditIntentReadResult.FromPilot(without);

        var outcome = rig.Core.TryCheckPilotEdit(rig.Request());

        Assert.Equal(EditCheckStatus.Recovered, outcome.Status);
        Assert.NotEqual(DraftStatus.Unknown, draft.Status);
    }

    [Fact]
    public void WithoutAnUnknownDraftCheckNowIsNotApplicableAndReadsNothing()
    {
        var rig = new Rig();
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request()).Status);
        var draft = rig.Draft()!;
        var reads = rig.Reader.Calls.Count;

        // Clean.
        var clean = rig.Core.TryCheckPilotEdit(rig.Request());
        // Dirty.
        draft.SetPendingText("field", "x");
        var dirty = rig.Core.TryCheckPilotEdit(rig.Request());
        draft.RemovePendingText("field");
        // Conflict.
        draft.TryBeginApply();
        draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedConflict(new[] { new DraftDifference("B2", DraftDifferenceKind.View) }));
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        var conflict = rig.Core.TryCheckPilotEdit(rig.Request());

        Assert.Equal(EditCheckStatus.NotApplicable, clean.Status);
        Assert.Equal(EditCheckStatus.NotApplicable, dirty.Status);
        Assert.Equal(EditCheckStatus.NotApplicable, conflict.Status);
        Assert.Equal(reads, rig.Reader.Calls.Count);
    }

    [Fact]
    public void AnUnknownDraftWhoseOriginIsThePostRereadIsNotApplicableAndReadsNothing()
    {
        var rig = new Rig();
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request()).Status);
        var draft = rig.Draft()!;
        Assert.True(draft.TryBeginApply().IsStarted);
        draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.AppliedPostRereadFailed("the reread failed"));
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.PostReread, draft.UnknownOrigin);
        var reads = rig.Reader.Calls.Count;

        var outcome = rig.Core.TryCheckPilotEdit(rig.Request());

        Assert.Equal(EditCheckStatus.NotApplicable, outcome.Status);
        Assert.Equal(reads, rig.Reader.Calls.Count);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
    }

    [Fact]
    public void CheckNowWithoutADraftIsNotFoundAndReadsNothing()
    {
        var rig = new Rig();

        var outcome = rig.Core.TryCheckPilotEdit(rig.Request());
        var blank = rig.Core.TryCheckPilotEdit(rig.Request(" "));

        Assert.Equal(EditCheckStatus.NotFound, outcome.Status);
        Assert.Equal(EditCheckStatus.NotFound, blank.Status);
        Assert.Empty(rig.Reader.Calls);
    }

    [Fact]
    public void CheckNowHasTheSameGateAsTheIntentionAndReadsNothingBeforeIt()
    {
        var noReader = new Rig(assignReader: false);
        Assert.Equal(EditCheckStatus.Unavailable, noReader.Core.TryCheckPilotEdit(noReader.Request()).Status);

        var rig = new Rig();
        var draft = rig.UnknownDraft();
        var reads = rig.Reader.Calls.Count;

        rig.Core.SetModalRackCadActive(true);
        Assert.Equal(EditCheckStatus.Waiting, rig.Core.TryCheckPilotEdit(rig.Request()).Status);
        rig.Core.SetModalRackCadActive(false);

        rig.Host.CommandStarts(rig.Session);
        Assert.Equal(EditCheckStatus.Waiting, rig.Core.TryCheckPilotEdit(rig.Request()).Status);
        rig.Host.CommandEnds(rig.Session);

        rig.Host.Probe.ActiveDocumentNativeId = null;
        Assert.Equal(EditCheckStatus.Waiting, rig.Core.TryCheckPilotEdit(rig.Request()).Status);
        rig.Host.Probe.ActiveDocumentNativeId = 100;

        Assert.Equal(reads, rig.Reader.Calls.Count);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
    }

    [Fact]
    public void CheckNowOfAnotherSessionIsRejectedWithZeroReadsAndTheDraftIsUntouched()
    {
        var rig = new Rig();
        var a = rig.Session;
        var draft = rig.UnknownDraft();
        var reads = rig.Reader.Calls.Count;
        rig.Host.Activate(200);
        rig.Core.RefreshPanel();
        rig.Host.Idle();

        var outcome = rig.Core.TryCheckPilotEdit(rig.Request(session: a));

        Assert.Equal(EditCheckStatus.Rejected, outcome.Status);
        Assert.Equal(reads, rig.Reader.Calls.Count);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
    }

    [Theory]
    [InlineData(EditIntentReadStatus.NoActiveDocument, EditCheckStatus.Waiting)]
    [InlineData(EditIntentReadStatus.Busy, EditCheckStatus.Waiting)]
    [InlineData(EditIntentReadStatus.Rejected, EditCheckStatus.Rejected)]
    [InlineData(EditIntentReadStatus.Failed, EditCheckStatus.Failed)]
    public void EveryAnswerOfTheReaderThatIsNotAReadLeavesTheUnknownDraftAsItWasForCheckNow(EditIntentReadStatus read, EditCheckStatus expected)
    {
        var rig = new Rig();
        var draft = rig.UnknownDraft();
        rig.Reader.Answer = _ => read switch
        {
            EditIntentReadStatus.NoActiveDocument => EditIntentReadResult.NoActiveDocument("no document"),
            EditIntentReadStatus.Busy => EditIntentReadResult.Busy("busy"),
            EditIntentReadStatus.Rejected => EditIntentReadResult.Rejected("not yours"),
            _ => EditIntentReadResult.Failed("could not read"),
        };

        var outcome = rig.Core.TryCheckPilotEdit(rig.Request());

        Assert.Equal(expected, outcome.Status);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.CommitThrew, draft.UnknownOrigin);
    }

    [Fact]
    public void AnExceptionOfTheReaderDuringCheckNowIsContainedAsFailedAndDoesNotDegradeTheSession()
    {
        var rig = new Rig();
        var draft = rig.UnknownDraft();
        rig.Reader.Throw = new InvalidOperationException("reread boom");

        var outcome = rig.Core.TryCheckPilotEdit(rig.Request());

        Assert.Equal(EditCheckStatus.Failed, outcome.Status);
        Assert.Equal("reread boom", outcome.Reason);
        Assert.Equal("WorkspaceHostCore.TryCheckPilotEdit", Assert.Single(rig.Host.Faults).Context);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        rig.Host.SelectionChanged(rig.Session);
        rig.Host.Idle();
        Assert.Equal(1, rig.Host.Reader.ReadCalls);
    }

    // ------------------------------------------------------------------ INV-F4T2B2A-09: nothing is called from the drain

    [Fact]
    public void NoOperationIsCalledFromTheDrainAndTheDrainNeverCallsTheReaderOfTheIntention()
    {
        var rig = new Rig();
        var draft = rig.UnknownDraft();
        var reads = rig.Reader.Calls.Count;
        var status = draft.Status;

        for (var i = 0; i < 3; i++)
        {
            rig.Host.SelectionChanged(rig.Session);
            rig.Host.CommandStarts(rig.Session);
            rig.Host.CommandEnds(rig.Session);
            rig.Core.RefreshPanel();
            rig.Host.Idle();
        }

        Assert.True(rig.Host.Reader.ReadCalls > 0, "the drain did read the selection");
        Assert.Equal(reads, rig.Reader.Calls.Count);
        Assert.Equal(status, draft.Status);
        Assert.Same(draft, rig.Draft());
    }

    // ------------------------------------------------------------------ the values carry no draft and no type of the pilot (D-T2B2-06)

    private static IEnumerable<Type> Flatten(Type type)
    {
        var current = type.IsByRef || type.IsArray ? type.GetElementType()! : type;
        yield return current;
        if (current.IsGenericType)
            foreach (var argument in current.GetGenericArguments().SelectMany(Flatten)) yield return argument;
    }

    [Fact]
    public void TheSignaturesOfTheTwoOperationsAndTheirValuesNameNoTypeOfADraftNorOfThePilot()
    {
        var begin = typeof(WorkspaceHostCore).GetMethod("TryBeginPilotEdit", BindingFlags.Public | BindingFlags.Instance);
        var check = typeof(WorkspaceHostCore).GetMethod("TryCheckPilotEdit", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(begin);
        Assert.NotNull(check);
        Assert.Equal(typeof(EditIntentOutcome), begin!.ReturnType);
        Assert.Equal(typeof(EditCheckOutcome), check!.ReturnType);
        Assert.Equal(new[] { typeof(EditIntentRequest) }, begin.GetParameters().Select(p => p.ParameterType).ToArray());
        Assert.Equal(new[] { typeof(EditIntentRequest) }, check.GetParameters().Select(p => p.ParameterType).ToArray());

        var types = new List<Type>();
        foreach (var carrier in new[] { typeof(EditIntentRequest), typeof(EditIntentWitness), typeof(EditIntentOutcome), typeof(EditCheckOutcome) })
        {
            foreach (var property in carrier.GetProperties(BindingFlags.Public | BindingFlags.Instance)) types.AddRange(Flatten(property.PropertyType));
            foreach (var method in carrier.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                types.AddRange(Flatten(method.ReturnType));
                foreach (var parameter in method.GetParameters()) types.AddRange(Flatten(parameter.ParameterType));
            }
        }

        Assert.NotEmpty(types);
        foreach (var type in types)
        {
            var ns = type.Namespace ?? string.Empty;
            Assert.False(ns.StartsWith("RackCad.Application.Workspace.Drafts", StringComparison.Ordinal), type.FullName);
            Assert.False(ns.StartsWith("RackCad.Application.Systems", StringComparison.Ordinal), type.FullName);
        }
    }

    [Fact]
    public void NoFieldOfTheHostCoreIsOfATypeOfThePilotOrOfTheResultOfTheReadAndTheFieldOfTheReaderIsNotCalledReader()
    {
        var fields = typeof(WorkspaceHostCore).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

        Assert.NotEmpty(fields);
        foreach (var field in fields)
        {
            foreach (var type in Flatten(field.FieldType))
            {
                Assert.False((type.Namespace ?? string.Empty).StartsWith("RackCad.Application.Systems", StringComparison.Ordinal), field.Name + " is a type of the pilot");
                Assert.NotEqual(typeof(EditIntentReadResult), type);
            }
        }

        Assert.Single(fields, f => f.Name == "_reader");
        Assert.Equal(typeof(IWorkspaceSelectionReader), fields.Single(f => f.Name == "_reader").FieldType);
        Assert.Single(fields, f => f.FieldType == typeof(IWorkspaceEditIntentReader));
        Assert.Single(fields, f => f.FieldType == typeof(DraftRegistry));
        Assert.Single(fields, f => f.FieldType == typeof(DraftClosePolicy));
    }

    [Fact]
    public void TheOutcomesAreValuesAndTheStatesHaveTheExactNamesAndOrderOfThePackage()
    {
        Assert.True(typeof(EditIntentOutcome).IsValueType);
        Assert.True(typeof(EditCheckOutcome).IsValueType);
        Assert.True(typeof(EditIntentReadResult).IsValueType);
        Assert.Equal(new[] { "Read", "NoActiveDocument", "Busy", "Rejected", "Failed" }, Enum.GetNames<EditIntentReadStatus>());
        // F4-T2B2B (N-1 of F4-T2B2A, F4-E33): the seven states of F4-T2B2A are an ORDERED PREFIX; later states (the reload adds NotFound,
        // NotNeeded and Reloaded) come after them, and the default value stays Unavailable.
        Assert.Equal(new[] { "Unavailable", "Waiting", "Rejected", "AlreadyOpen", "NotAdmitted", "Admitted", "Failed" }, Enum.GetNames<EditIntentStatus>().Take(7).ToArray());
        Assert.Equal(EditIntentStatus.Unavailable, default(EditIntentStatus));
        Assert.Equal(new[] { "Unavailable", "Waiting", "Rejected", "NotFound", "NotApplicable", "Conflict", "Recovered", "Failed" }, Enum.GetNames<EditCheckStatus>());
    }

    [Fact]
    public void TheDifferencesOfACheckOutcomeAreNeverNullAndAreEmptyUnlessTheStatusIsConflictAlsoForTheDefaultValue()
    {
        // M-7: a consumer may count the differences of any outcome, including a default one, without a null reference.
        var unset = default(EditCheckOutcome);
        Assert.Equal(EditCheckStatus.Unavailable, unset.Status);
        Assert.NotNull(unset.Differences);
        Assert.Empty(unset.Differences);

        foreach (var outcome in new[]
                 {
                     EditCheckOutcome.Unavailable("x"), EditCheckOutcome.Waiting("x"), EditCheckOutcome.Rejected("x"), EditCheckOutcome.NotFound("x"),
                     EditCheckOutcome.NotApplicable("x"), EditCheckOutcome.Recovered(), EditCheckOutcome.Failed("x"),
                 })
        {
            Assert.NotNull(outcome.Differences);
            Assert.Empty(outcome.Differences);
        }

        var conflict = EditCheckOutcome.Conflict(new[] { "B2:Membership" });
        Assert.Equal(new[] { "B2:Membership" }, conflict.Differences.ToArray());
    }

    [Fact]
    public void TheReadResultCarriesThePilotReadOnlyWhenItIsARead()
    {
        var read = EditIntentKit.Admitted(RackA);

        Assert.Equal(EditIntentReadStatus.Read, read.Status);
        Assert.NotNull(read.PilotRead);
        Assert.Null(read.Reason);
        foreach (var other in new[]
                 {
                     EditIntentReadResult.NoActiveDocument("a"), EditIntentReadResult.Busy("b"), EditIntentReadResult.Rejected("c"), EditIntentReadResult.Failed("d"),
                 })
        {
            Assert.NotEqual(EditIntentReadStatus.Read, other.Status);
            Assert.Null(other.PilotRead);
            Assert.False(string.IsNullOrEmpty(other.Reason));
        }

        Assert.Throws<ArgumentNullException>(() => EditIntentReadResult.FromPilot(null!));
    }

    // ------------------------------------------------------------------ D-T2B2-04: the choice of the origin (a pure function)

    private static readonly SessionId S1 = new(1);

    private static NavigationTargetSet Set(params (long Reference, long Definition)[] targets) =>
        new(
            new RackKey(S1, "R-1"),
            targets.Select(t => new NavigationTarget(t.Reference, t.Definition, NavigationViewOrder.Frontal, "pushback", "frontal", 0, "name")).ToArray(),
            Array.Empty<NavigationDiagnostic>());

    [Theory]
    [InlineData(0x101L, 0xAL)]
    [InlineData(0x102L, 0xAL)]
    [InlineData(0x103L, 0xBL)]
    public void AChosenReferenceThatIsStillAmongTheTargetsGivesItsDefinition(long chosen, long expected)
    {
        var set = Set((0x101, 0xA), (0x102, 0xA), (0x103, 0xB));

        Assert.Equal(expected, WorkspaceEditIntent.ChooseOriginDefinition(set, chosen));
    }

    [Theory]
    [InlineData(0x999L)]
    [InlineData(0L)]
    [InlineData(-5L)]
    [InlineData(0xAL)]
    public void AChosenReferenceThatIsNoLongerAmongTheTargetsGivesZeroEvenIfTheNumberIsADefinitionOfTheSet(long chosen)
    {
        var set = Set((0x101, 0xA), (0x102, 0xA), (0x103, 0xB));

        Assert.Equal(0L, WorkspaceEditIntent.ChooseOriginDefinition(set, chosen));
    }

    [Fact]
    public void WithoutAChosenReferenceTheOriginIsThePrimaryTargetInTheDeterministicOrderNotTheLowestHandle()
    {
        var set = Set((0x300, 0xC), (0x101, 0xA), (0x102, 0xB));

        Assert.Equal(0xCL, WorkspaceEditIntent.ChooseOriginDefinition(set, null));
        Assert.Equal(set.Primary!.DefinitionHandle, WorkspaceEditIntent.ChooseOriginDefinition(set, null));
    }

    [Fact]
    public void WithoutTargetsTheOriginIsZeroWhetherOrNotAReferenceWasChosen()
    {
        var empty = Set();

        Assert.Equal(0L, WorkspaceEditIntent.ChooseOriginDefinition(empty, null));
        Assert.Equal(0L, WorkspaceEditIntent.ChooseOriginDefinition(empty, 0x101));
    }

    [Fact]
    public void ANullSetOfTargetsIsAProgrammingErrorAndNotAnOrigin()
    {
        Assert.Throws<ArgumentNullException>(() => WorkspaceEditIntent.ChooseOriginDefinition(null!, null));
    }

    [Fact]
    public void AReferenceThatVanishedOrNowBelongsToAnotherRackGivesZeroAfterTheRevalidationOfTheSameOperation()
    {
        var facts = new FakeNavigationFacts().With(
            S1,
            NavFact.Member(10, "R-1", "frontal", 0, 0x101, 0x102),
            NavFact.Member(11, "R-1", "lateral", 0, 0x103));
        var targets = new NavigationTargets(facts);

        // Still there: the origin is the definition of that reference.
        var valid = targets.Revalidate(S1, "R-1", new long[] { 0x103 });
        Assert.Equal(NavigationRevalidationVerdict.Valid, valid.Verdict);
        Assert.Equal(11L, WorkspaceEditIntent.ChooseOriginDefinition(valid.Fresh, 0x103));

        // The reference left the rack for another one: the read says the origin is no longer there.
        facts.For(S1).Clear();
        facts.With(S1, NavFact.Member(10, "R-1", "frontal", 0, 0x101), NavFact.Member(12, "R-2", "frontal", 0, 0x102, 0x103));
        var moved = targets.Revalidate(S1, "R-1", new long[] { 0x103 });
        Assert.Equal(NavigationRevalidationVerdict.NotAttributable, moved.Verdict);
        Assert.Equal(0L, WorkspaceEditIntent.ChooseOriginDefinition(moved.Fresh, 0x103));

        // The reference vanished.
        facts.For(S1).Clear();
        facts.With(S1, NavFact.Member(10, "R-1", "frontal", 0, 0x101));
        var gone = targets.Revalidate(S1, "R-1", new long[] { 0x103 });
        Assert.Equal(NavigationRevalidationVerdict.TargetGone, gone.Verdict);
        Assert.Equal(0L, WorkspaceEditIntent.ChooseOriginDefinition(gone.Fresh, 0x103));

        // Without a chosen reference, the first target of the rack as it is NOW.
        Assert.Equal(10L, WorkspaceEditIntent.ChooseOriginDefinition(gone.Fresh, null));
    }

    [Fact]
    public void TheDiagnosticOfAnOriginThatIsGoneSaysItIsNoLongerThereAndNeverThatItBelongsToAnotherRack()
    {
        // N-13: a reference that now belongs to another rack also gives 0, so the read answers OriginMissing, and its text must not
        // claim anything about another rack.
        Assert.Contains("ya no está", PushBackPilotIntentReader.OriginMissingDiagnostic);
        Assert.DoesNotContain("otro rack", PushBackPilotIntentReader.OriginMissingDiagnostic);
        Assert.Contains("otro rack", PushBackPilotIntentReader.OriginNotMemberDiagnostic);

        var scan = IntentKit.Scan(RackA, string.Empty, IntentKit.Member("A1", IntentKit.Variant("pb-01-base", RackA)));
        var read = PushBackPilotIntentReader.Read(scan, PilotKit.Catalog);
        Assert.Equal(PushBackPilotIntentReader.OriginMissingDiagnostic, read.Read.Admission.Diagnostic);
    }
}
