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

// I-64 F4-T4B-MUTATE-POST (INV-F4T4B-09, 10 and 11): TryUpdatePilot, the method of the host core that runs the update through the port that it
// is handed, and CompleteApply by branch (D-T4B-06), each with the real draft session. The doubles are the reader of the drawing and the port of
// the update. Every expectation that something throws, or does not, uses Record.Exception, so a behavior that is missing fails by ASSERTION and
// never by an escaped exception.
public class WorkspacePilotUpdateHostTests
{
    private const string RackA = EditIntentKit.RackA;
    private const string RackB = EditIntentKit.RackB;

    private sealed class Rig
    {
        public Rig(bool document = true)
        {
            Host = new WorkspaceHostRig();
            if (document) Host.WithDocument(100);
            Host.Host.OpenPanel();
            Session = document ? Host.ActiveSession : new SessionId(999);
            Host.Idle();
            Host.Reader.Reads.Clear();
            Reader = new EditIntentFakeReader { Answer = _ => EditIntentKit.ReadOf(PrepareKit.RealScan()) };
            Host.Host.SetEditIntentReader(Reader);
            Port = new PilotUpdateScopeDouble(Members());
        }

        public WorkspaceHostRig Host { get; }

        public WorkspaceHostCore Core => Host.Host;

        public EditIntentFakeReader Reader { get; }

        public SessionId Session { get; }

        public PilotUpdateScopeDouble Port { get; set; }

        public static IntentItem[] Members()
        {
            var scan = PrepareKit.RealScan();
            return scan.Facts.Select(f => new IntentItem(f, scan.RawEnvelopes[f.DefinitionKey])).ToArray();
        }

        public EditIntentRequest Request(string rackId = RackA, SessionId? session = null) => new(session ?? Session, rackId, null, null);

        public DraftSession<PushBackPilotDraftState> Open(bool edit = true, string rackId = RackA)
        {
            Assert.Equal(EditIntentStatus.Admitted, Core.TryBeginPilotEdit(Request(rackId)).Status);
            var draft = Core.OpenDrafts.Find<PushBackPilotDraftState>(Session, rackId)!;
            if (edit) PrepareKit.SetScale(draft, 2.25);
            return draft;
        }

        public EditUpdateOutcome Update(string rackId = RackA) => Core.TryUpdatePilot(Request(rackId), Port);
    }

    private static InvalidOperationException Boom(string message) => new(message);

    // ------------------------------------------------------------------ the gate of its own: no rest and no reader

    [Fact]
    public void TheGateRejectsAModalAWrongSessionAndNoDocumentWithoutReadingAnythingAndWithoutTheSlot()
    {
        var modal = new Rig();
        var draft = modal.Open();
        modal.Core.SetModalRackCadActive(true);
        var rejected = modal.Update();
        Assert.Equal(EditUpdateStatus.Rejected, rejected.Status);
        Assert.False(string.IsNullOrEmpty(rejected.Reason));
        Assert.Empty(modal.Port.Calls);
        Assert.Equal(DraftStatus.Dirty, draft.Status);

        var other = new Rig();
        other.Open();
        var wrong = other.Core.TryUpdatePilot(other.Request(session: new SessionId(777)), other.Port);
        Assert.Equal(EditUpdateStatus.Rejected, wrong.Status);
        Assert.Equal("La petición no es del documento activo.", wrong.Reason);
        Assert.Empty(other.Port.Calls);

        var none = new Rig(document: false);
        var nothing = none.Core.TryUpdatePilot(none.Request(), none.Port);
        Assert.Equal(EditUpdateStatus.Rejected, nothing.Status);
        Assert.Empty(none.Port.Calls);
    }

    [Fact]
    public void TheGateHasNoRestAndNoReaderSoACommandInProgressAndAMissingReaderDoNotStopTheUpdate()
    {
        var rig = new Rig();
        rig.Open();
        rig.Host.Probe.IsActiveDocumentQuiescent = false;

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.Applied, outcome.Status);
        Assert.Empty(rig.Reader.Calls.Skip(1));

        // No reader was ever assigned to this host: the update answers like any other, and not Unavailable.
        var fresh = new WorkspaceHostRig().WithDocument(100);
        fresh.Host.OpenPanel();
        var session = fresh.ActiveSession;
        fresh.Idle();
        Assert.Equal(EditUpdateStatus.NoDraft, fresh.Host.TryUpdatePilot(new EditIntentRequest(session, RackA, null, null), new PilotUpdateScopeDouble()).Status);
    }

    [Fact]
    public void TheUpdateOfThePilotNeverUsesTheReaderOfTheIntention()
    {
        var rig = new Rig();
        rig.Open();
        var reads = rig.Reader.Calls.Count;

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.Applied, outcome.Status);
        Assert.Equal(reads, rig.Reader.Calls.Count);
    }

    // ------------------------------------------------------------------ the draft and the check before the slot

    [Fact]
    public void ABlankRackAndAnUnknownRackHaveNoDraftAndNothingIsRun()
    {
        var rig = new Rig();
        rig.Open();

        foreach (var rack in new[] { "", "  ", RackB })
        {
            var outcome = rig.Core.TryUpdatePilot(rig.Request(rack), rig.Port);
            Assert.Equal(EditUpdateStatus.NoDraft, outcome.Status);
            Assert.Equal("No hay borrador para el rack.", outcome.Reason);
        }

        Assert.Empty(rig.Port.Calls);
    }

    [Fact]
    public void TheCheckBeforeTheSlotAnswersNothingToApplyPendingTextAndNotApplicableWithoutTakingTheSlot()
    {
        var clean = new Rig();
        var cleanDraft = clean.Open(edit: false);
        Assert.Equal(DraftStatus.Clean, cleanDraft.Status);
        var nothing = clean.Update();
        Assert.Equal(EditUpdateStatus.NothingToApply, nothing.Status);
        Assert.Equal(PushBackPilotUpdateRun.NothingToApplyReason, nothing.Reason);
        Assert.Empty(clean.Port.Calls);
        Assert.Equal(DraftStatus.Clean, cleanDraft.Status);

        var pending = new Rig();
        var pendingDraft = pending.Open();
        pendingDraft.SetPendingText("AnnotationScale", "3");
        var typed = pending.Update();
        Assert.Equal(EditUpdateStatus.PendingText, typed.Status);
        Assert.Equal(PushBackPilotUpdateRun.PendingTextReason, typed.Reason);
        Assert.Equal(DraftStatus.Dirty, pendingDraft.Status);
        Assert.Empty(pending.Port.Calls);

        var conflict = new Rig();
        var conflictDraft = conflict.Open();
        conflictDraft.ConfirmBase(PrepareKit.BaseOf(("A1", PrepareKit.Frontal("pb-02-modulos"))));
        Assert.Equal(DraftStatus.Conflict, conflictDraft.Status);
        var notApplicable = conflict.Update();
        Assert.Equal(EditUpdateStatus.NotApplicable, notApplicable.Status);
        Assert.Equal(PushBackPilotUpdateRun.NotApplicableReason, notApplicable.Reason);
        Assert.Empty(conflict.Port.Calls);
    }

    // ------------------------------------------------------------------ the slot (INV-F4T4B-09)

    [Fact]
    public void AnotherUpdateInProgressInTheRegistryAnswersItAndRunsNothing()
    {
        var rig = new Rig();
        rig.Open();
        rig.Reader.Answer = EditIntentKit.Admitted;
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request(RackB)).Status);
        var other = rig.Core.OpenDrafts.Find<PushBackPilotDraftState>(rig.Session, RackB)!;
        Assert.True(other.TryBeginApply().IsStarted);

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.AnotherUpdateInProgress, outcome.Status);
        Assert.Empty(rig.Port.Calls);
        Assert.Equal(DraftStatus.Dirty, rig.Core.OpenDrafts.Find<PushBackPilotDraftState>(rig.Session, RackA)!.Status);
    }

    [Fact]
    public void ADraftThatIsAlreadyApplyingIsRefusedWithTheNameOfTheRefusal()
    {
        var rig = new Rig();
        var draft = rig.Open();
        Assert.True(draft.TryBeginApply().IsStarted);

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.Refused, outcome.Status);
        Assert.Equal(DraftApplyRefusal.InvalidStatus.ToString(), outcome.Reason);
        Assert.Empty(rig.Port.Calls);
    }

    [Fact]
    public void AFailureBeforeTheSlotIsFailedAndLoggedAndTheSessionIsNotDegraded()
    {
        var rig = new Rig();
        rig.Open();

        var noRequest = rig.Core.TryUpdatePilot(null!, rig.Port);
        var noPort = rig.Core.TryUpdatePilot(rig.Request(), null!);

        Assert.Equal(EditUpdateStatus.Failed, noRequest.Status);
        Assert.Equal(EditUpdateStatus.Failed, noPort.Status);
        Assert.False(string.IsNullOrEmpty(noRequest.Reason));
        Assert.Equal(2, rig.Host.Faults.Count(f => f.Context == "WorkspaceHostCore.TryUpdatePilot"));
        Assert.Equal(EditUpdateStatus.Applied, rig.Update().Status);
    }

    [Fact]
    public void TheSlotIsClosedExactlyOnceAndIsFreeAfterEveryAnswer()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Port.ThrowAt["PrepareUnits"] = Boom("import boom");

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.NotAppliedFailure, outcome.Status);
        // The close already happened: a second close finds no update in progress, and another draft can take the slot.
        Assert.False(draft.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedFailure("again")).Accepted);
        rig.Reader.Answer = EditIntentKit.Admitted;
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request(RackB)).Status);
        Assert.True(rig.Core.OpenDrafts.Find<PushBackPilotDraftState>(rig.Session, RackB)!.TryBeginApply().IsStarted);
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    // ------------------------------------------------------------------ the answer comes from the close of the draft

    [Fact]
    public void AnAppliedUpdateRebasesTheDraftAndRunsThePortWithTheAuthorizedDocumentAndTheRack()
    {
        var rig = new Rig();
        var draft = rig.Open();

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.Applied, outcome.Status);
        Assert.Null(outcome.Reason);
        Assert.Empty(outcome.Warnings);
        Assert.Empty(outcome.Differences);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
        Assert.Equal(new PushBackPilotUpdateRequest(100, RackA), rig.Port.Request);
        Assert.Equal(PilotUpdateScopeDouble.Steps(), rig.Port.Calls.ToArray());
        // The draft is Clean now: a second update has nothing to apply and does not touch the port.
        Assert.Equal(EditUpdateStatus.NothingToApply, rig.Update().Status);
    }

    [Fact]
    public void AConflictOfMutateIsNotAppliedConflictWithTheDifferencesAndTheDraftGoesToConflict()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Port.RereadDrawing = Rig.Members().Select(m => m.Fact.DefinitionKey == "C3" ? IntentKit.Member("C3", PrepareKit.Planta("pb-02-modulos")) : m).ToArray();

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.NotAppliedConflict, outcome.Status);
        Assert.Equal(new[] { "C3:EnvelopeText" }, outcome.Differences.ToArray());
        Assert.Null(outcome.Reason);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Empty(rig.Port.Written);
    }

    [Fact]
    public void ANotAdmittedReadWithTheBaseOfTheDraftIsNotAppliedAdmissionAndTheDraftKeepsItsStatus()
    {
        var rig = new Rig();
        var draft = rig.Open();
        // Without a catalog the read cannot materialize the state: not admitted, with the base unchanged. The comparison goes first and the
        // missing catalog is answered after the admission (M-6).
        rig.Port.NullCatalog = true;

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.NotAppliedAdmission, outcome.Status);
        Assert.False(string.IsNullOrEmpty(outcome.Reason));
        Assert.Equal(DraftStatus.Dirty, draft.Status);
    }

    [Fact]
    public void APreflightAndAFailureKeepTheDraftAsItWasAndCarryTheirCause()
    {
        var preflight = new Rig();
        var first = preflight.Open();
        preflight.Port.LockedAtPrepare = new[] { "B2" };
        var locked = preflight.Update();
        Assert.Equal(EditUpdateStatus.NotAppliedPreflight, locked.Status);
        Assert.Equal(PushBackPilotUpdateRun.LockedLayerPrefix + "B2", locked.Reason);
        Assert.Equal(DraftStatus.Dirty, first.Status);

        var failure = new Rig();
        var second = failure.Open();
        failure.Port.ThrowAt["Reread"] = Boom("reread boom");
        var failed = failure.Update();
        Assert.Equal(EditUpdateStatus.NotAppliedFailure, failed.Status);
        Assert.Contains("reread boom", failed.Reason!, StringComparison.Ordinal);
        Assert.Equal(DraftStatus.Dirty, second.Status);
    }

    [Fact]
    public void ASealThatThrewIsCommitThrewAndTheDraftIsUnknownWithItsOrigin()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Port.ThrowAt["Seal"] = Boom("seal boom");

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.CommitThrew, outcome.Status);
        Assert.StartsWith(PushBackPilotUpdateRun.SealFailedPrefix, outcome.Reason!, StringComparison.Ordinal);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.CommitThrew, draft.UnknownOrigin);
    }

    [Fact]
    public void AnAppliedUpdateWithWarningsKeepsThemAndTheDraftIsClean()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Port.MissingPieces["B2"] = new[] { "POSTE_X" };

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.AppliedWithWarnings, outcome.Status);
        var warning = Assert.Single(outcome.Warnings);
        Assert.Contains("POSTE_X", warning, StringComparison.Ordinal);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Equal(outcome.Warnings.ToArray(), draft.Warnings.ToArray());
    }

    [Fact]
    public void APostThatFailedIsAppliedPostRereadFailedWithTheCauseAndTheWarningsAndTheDraftIsUnknown()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Port.MissingPieces["C3"] = new[] { "PIEZA_Z" };
        rig.Port.ThrowAt["Post"] = Boom("post boom");

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.AppliedPostRereadFailed, outcome.Status);
        Assert.Contains("post boom", outcome.Reason!, StringComparison.Ordinal);
        Assert.Single(outcome.Warnings);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.PostReread, draft.UnknownOrigin);
        Assert.Single(draft.Warnings);
    }

    [Fact]
    public void ADraftThatWasDiscardedDuringTheRunIsClosedDetachedAndTheSlotIsFree()
    {
        var rig = new Rig();
        rig.Open();
        rig.Port.HookAt["WriteUnit:A1"] = () => rig.Core.OpenDrafts.Discard(rig.Session, RackA);

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.ClosedDetached, outcome.Status);
        Assert.Null(rig.Core.OpenDrafts.Find<PushBackPilotDraftState>(rig.Session, RackA));
        rig.Reader.Answer = EditIntentKit.Admitted;
        Assert.Equal(EditIntentStatus.Admitted, rig.Core.TryBeginPilotEdit(rig.Request(RackB)).Status);
        Assert.True(rig.Core.OpenDrafts.Find<PushBackPilotDraftState>(rig.Session, RackB)!.TryBeginApply().IsStarted);
    }

    [Fact]
    public void AnErrorThatArrivesDuringTheRunIsAppliedAfterTheClassificationAndTheAnswerIsClosedInError()
    {
        var rig = new Rig();
        var draft = rig.Open();
        rig.Port.HookAt["Seal"] = () => draft.EnterError("the surface failed");

        var outcome = rig.Update();

        Assert.Equal(EditUpdateStatus.ClosedInError, outcome.Status);
        Assert.Equal("the surface failed", outcome.Reason);
        Assert.Equal(DraftStatus.Error, draft.Status);
        // The write happened and the re-read of POST was made: the content is the rebased one, whatever the status says.
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);
    }

    [Fact]
    public void TheAnswerOfAnUpdateAlwaysComesFromTheCloseAndTheOutcomeNeverNamesADraft()
    {
        var method = typeof(WorkspaceHostCore).GetMethod("TryUpdatePilot", BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(method);
        Assert.Equal(typeof(EditUpdateOutcome), method!.ReturnType);
        Assert.Equal(new[] { typeof(EditIntentRequest), typeof(IPushBackPilotUpdatePort) }, method.GetParameters().Select(p => p.ParameterType).ToArray());
        Assert.True(typeof(EditUpdateOutcome).IsValueType);
        Assert.Equal(EditUpdateStatus.Unavailable, default(EditUpdateOutcome).Status);
        Assert.Null(default(EditUpdateOutcome).Reason);
        Assert.NotNull(default(EditUpdateOutcome).Differences);
        Assert.Empty(default(EditUpdateOutcome).Differences);
        Assert.NotNull(default(EditUpdateOutcome).Warnings);
        Assert.Empty(default(EditUpdateOutcome).Warnings);
    }

    [Fact]
    public void TheStatesOfTheUpdateHaveTheExactNamesAndOrderOfThePackageAndNotApplyingIsNotOneOfThem()
    {
        Assert.Equal(
            new[]
            {
                "Unavailable", "Rejected", "NoDraft", "NothingToApply", "PendingText", "NotApplicable", "AnotherUpdateInProgress", "Refused",
                "NotAppliedConflict", "NotAppliedAdmission", "NotAppliedPreflight", "NotAppliedFailure", "CommitThrew", "Applied",
                "AppliedWithWarnings", "AppliedPostRereadFailed", "ClosedDetached", "ClosedInError", "Failed",
            },
            Enum.GetNames<EditUpdateStatus>());
        Assert.Equal(EditUpdateStatus.Unavailable, default(EditUpdateStatus));
        Assert.DoesNotContain("NotApplying", Enum.GetNames<EditUpdateStatus>());

        // From NotAppliedConflict to AppliedPostRereadFailed the names are the ones of the classification by frontier, in the same order.
        var closed = Enum.GetNames<EditUpdateStatus>().Skip(8).Take(8).ToArray();
        Assert.Equal(Enum.GetNames<DraftApplyOutcomeKind>().ToArray(), closed);
    }

    [Fact]
    public void TheFactoriesOfTheOutcomeCarryTheirStatusTheirReasonAndCopiesOfTheLists()
    {
        Assert.Equal(EditUpdateStatus.Rejected, EditUpdateOutcome.Rejected("r").Status);
        Assert.Equal("r", EditUpdateOutcome.NoDraft("r").Reason);
        Assert.Equal(EditUpdateStatus.NothingToApply, EditUpdateOutcome.NothingToApply("r").Status);
        Assert.Equal(EditUpdateStatus.PendingText, EditUpdateOutcome.PendingText("r").Status);
        Assert.Equal(EditUpdateStatus.NotApplicable, EditUpdateOutcome.NotApplicable("r").Status);
        Assert.Equal(EditUpdateStatus.AnotherUpdateInProgress, EditUpdateOutcome.AnotherUpdateInProgress().Status);
        Assert.Equal(EditUpdateStatus.Refused, EditUpdateOutcome.Refused("r").Status);
        Assert.Equal(EditUpdateStatus.ClosedDetached, EditUpdateOutcome.ClosedDetached().Status);
        Assert.Equal("why", EditUpdateOutcome.ClosedInError("why").Reason);
        Assert.Equal(EditUpdateStatus.Failed, EditUpdateOutcome.Failed("r").Status);
        Assert.Equal(EditUpdateStatus.Unavailable, EditUpdateOutcome.Unavailable("r").Status);

        var closed = EditUpdateOutcome.Closed(EditUpdateStatus.AppliedWithWarnings, "c", new[] { "d" }, new[] { "w" });
        Assert.Equal(EditUpdateStatus.AppliedWithWarnings, closed.Status);
        Assert.Equal("c", closed.Reason);
        Assert.Equal(new[] { "d" }, closed.Differences.ToArray());
        Assert.Equal(new[] { "w" }, closed.Warnings.ToArray());
        Assert.Throws<ArgumentNullException>(() => EditUpdateOutcome.Closed(EditUpdateStatus.Applied, null, null!, Array.Empty<string>()));
        Assert.Throws<ArgumentNullException>(() => EditUpdateOutcome.Closed(EditUpdateStatus.Applied, null, Array.Empty<string>(), null!));
    }

    // ================================================================== CompleteApply by branch (D-T4B-06), one test per row

    private sealed class ArmedCopy
    {
        private int _calls;
        private int _throwOn = -1;

        public Action? OnCopy { get; set; }

        public void Arm(int call)
        {
            _calls = 0;
            _throwOn = call;
        }

        public DraftForm Copy(DraftForm form)
        {
            _calls++;
            OnCopy?.Invoke();
            if (_calls == _throwOn) throw new InvalidOperationException("copier-boom");
            return form with { };
        }
    }

    private static DraftSession<DraftForm> Born(IEqualityComparer<DraftForm> comparer, Func<DraftForm, DraftForm> copy, DraftRegistry? registry = null, string rack = "R1")
    {
        var result = (registry ?? new DraftRegistry()).Begin(DraftKit.S1, rack, DraftKit.Read(), comparer, copy);
        Assert.Equal(DraftBeginStatus.Created, result.Status);
        return result.Draft!;
    }

    private static DraftIntentRead<DraftForm> Fresh() => DraftKit.Read(DraftKit.Form("Rack", 40), DraftKit.Base(DraftKit.Mutable("D9")));

    [Theory]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    public void RowAppliedAndAppliedWithWarningsWhoseCopierThrowsIsUnknownPostRereadWithCauseAndNothingIsRebased(bool withWarnings, int failingCopy)
    {
        var copier = new ArmedCopy();
        var draft = Born(DraftKit.Comparer, copier.Copy);
        draft.SetCurrentDraftState(DraftKit.Form("Edited", 31));
        var staleBefore = draft.StaleBase;
        var currentBefore = draft.CurrentDraftState;
        var initialBefore = draft.InitialDraftState;
        Assert.True(draft.TryBeginApply().IsStarted);
        copier.Arm(failingCopy);
        DraftApplyResult result = default;

        var thrown = Record.Exception(() =>
            result = draft.CompleteApply(withWarnings
                ? DraftApplyOutcome<DraftForm>.AppliedWithWarnings(Fresh(), new[] { "w1" })
                : DraftApplyOutcome<DraftForm>.Applied(Fresh())));

        Assert.Null(thrown);
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, result.Kind);
        Assert.False(result.Rebased);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.PostReread, draft.UnknownOrigin);
        Assert.Contains("copier-boom", draft.LastApplyCause!, StringComparison.Ordinal);
        Assert.Same(staleBefore, draft.StaleBase);
        Assert.Same(currentBefore, draft.CurrentDraftState);
        Assert.Equal(initialBefore, draft.InitialDraftState);
        Assert.Equal(withWarnings ? new[] { "w1" } : Array.Empty<string>(), draft.Warnings.ToArray());
        // The slot is free: the close happened exactly once.
        Assert.False(draft.CompleteApply(DraftApplyOutcome<DraftForm>.NotAppliedFailure("again")).Accepted);
    }

    [Fact]
    public void RowAppliedWhoseComparatorThrowsIsUnknownPostRereadAndNothingIsRebased()
    {
        var comparer = new DraftThrowingComparer();
        var draft = Born(comparer, DraftKit.CopyForm);
        draft.SetCurrentDraftState(DraftKit.Form("Edited", 31));
        var staleBefore = draft.StaleBase;
        Assert.True(draft.TryBeginApply().IsStarted);
        comparer.Throw = true;
        DraftApplyResult result = default;

        var thrown = Record.Exception(() => result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.Applied(Fresh())));

        Assert.Null(thrown);
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, result.Kind);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.PostReread, draft.UnknownOrigin);
        Assert.Contains("the comparer threw", draft.LastApplyCause!, StringComparison.Ordinal);
        Assert.Same(staleBefore, draft.StaleBase);
        comparer.Throw = false;
        Assert.Equal("Edited", draft.CurrentDraftState.Name);
    }

    [Fact]
    public void RowAppliedCopiesBeforeTheSlotIsFreedSoAnotherUpdateStillCannotTakeItWhileTheCopyRuns()
    {
        var registry = new DraftRegistry();
        var copier = new ArmedCopy();
        var draft = Born(DraftKit.Comparer, copier.Copy, registry, "R1");
        var other = Born(DraftKit.Comparer, DraftKit.CopyForm, registry, "R2");
        draft.SetCurrentDraftState(DraftKit.Form("Edited", 31));
        Assert.True(draft.TryBeginApply().IsStarted);
        var refusals = new List<DraftApplyRefusal>();
        copier.OnCopy = () => refusals.Add(other.TryBeginApply().Refusal);
        var read = Fresh();
        copier.Arm(-1);

        var result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.Applied(read));

        Assert.Equal(DraftApplyOutcomeKind.Applied, result.Kind);
        Assert.Equal(2, refusals.Count);
        Assert.All(refusals, r => Assert.Equal(DraftApplyRefusal.AnotherUpdateInProgress, r));
        // And once the close is done, the slot is free.
        Assert.True(other.TryBeginApply().IsStarted);
    }

    [Fact]
    public void RowCommitThrewIsKeptAsUnknownCommitThrewAndNoStateIsComputed()
    {
        var comparer = new DraftThrowingComparer();
        var draft = Born(comparer, DraftKit.CopyForm);
        draft.SetCurrentDraftState(DraftKit.Form("Edited", 31));
        Assert.True(draft.TryBeginApply().IsStarted);
        comparer.Throw = true;
        DraftApplyResult result = default;

        var thrown = Record.Exception(() => result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.CommitThrew("seal threw")));

        Assert.Null(thrown);
        Assert.Equal(DraftApplyOutcomeKind.CommitThrew, result.Kind);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(DraftUnknownOrigin.CommitThrew, draft.UnknownOrigin);
        Assert.Equal("seal threw", draft.LastApplyCause);
    }

    [Theory]
    [InlineData(DraftApplyOutcomeKind.NotAppliedAdmission)]
    [InlineData(DraftApplyOutcomeKind.NotAppliedPreflight)]
    [InlineData(DraftApplyOutcomeKind.NotAppliedFailure)]
    public void RowNotAppliedWhoseStateCannotBeComputedGoesToErrorWithItsReasonAndNeverToApplied(DraftApplyOutcomeKind kind)
    {
        var comparer = new DraftThrowingComparer();
        var draft = Born(comparer, DraftKit.CopyForm);
        draft.SetCurrentDraftState(DraftKit.Form("Edited", 31));
        Assert.True(draft.TryBeginApply().IsStarted);
        var staleBefore = draft.StaleBase;
        comparer.Throw = true;
        DraftApplyResult result = default;
        var outcome = kind switch
        {
            DraftApplyOutcomeKind.NotAppliedAdmission => DraftApplyOutcome<DraftForm>.NotAppliedAdmission("no longer admitted"),
            DraftApplyOutcomeKind.NotAppliedPreflight => DraftApplyOutcome<DraftForm>.NotAppliedPreflight("refused"),
            _ => DraftApplyOutcome<DraftForm>.NotAppliedFailure("failed"),
        };

        var thrown = Record.Exception(() => result = draft.CompleteApply(outcome));

        Assert.Null(thrown);
        Assert.Equal(kind, result.Kind);
        Assert.Equal(DraftStatus.Error, result.Status);
        Assert.Equal(DraftStatus.Error, draft.Status);
        Assert.Contains("the comparer threw", draft.ErrorReason!, StringComparison.Ordinal);
        Assert.Null(draft.UnknownOrigin);
        Assert.False(result.Rebased);
        Assert.Same(staleBefore, draft.StaleBase);
        comparer.Throw = false;
        Assert.Equal("Edited", draft.CurrentDraftState.Name);
        Assert.False(draft.CompleteApply(DraftApplyOutcome<DraftForm>.NotAppliedFailure("again")).Accepted);
    }

    [Fact]
    public void RowNotAppliedConflictKeepsItsClassificationEvenWhenTheComparerThrows()
    {
        var comparer = new DraftThrowingComparer();
        var draft = Born(comparer, DraftKit.CopyForm);
        draft.SetCurrentDraftState(DraftKit.Form("Edited", 31));
        Assert.True(draft.TryBeginApply().IsStarted);
        comparer.Throw = true;
        DraftApplyResult result = default;

        var thrown = Record.Exception(() => result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.NotAppliedConflict(DraftKit.OneDifference())));

        Assert.Null(thrown);
        Assert.Equal(DraftApplyOutcomeKind.NotAppliedConflict, result.Kind);
        Assert.Equal(DraftStatus.Conflict, draft.Status);
        Assert.Single(draft.ConflictDifferences);
    }

    [Fact]
    public void RowAllTheDeferredErrorIsStillAppliedAfterTheBranchWhetherOrNotTheCopierThrew()
    {
        foreach (var armed in new[] { false, true })
        {
            var copier = new ArmedCopy();
            var draft = Born(DraftKit.Comparer, copier.Copy);
            draft.SetCurrentDraftState(DraftKit.Form("Edited", 31));
            Assert.True(draft.TryBeginApply().IsStarted);
            draft.EnterError("deferred reason");
            Assert.Equal(DraftStatus.Applying, draft.Status);
            copier.Arm(armed ? 1 : -1);

            var thrown = Record.Exception(() => draft.CompleteApply(DraftApplyOutcome<DraftForm>.Applied(Fresh())));

            Assert.Null(thrown);
            Assert.Equal(DraftStatus.Error, draft.Status);
            Assert.Equal("deferred reason", draft.ErrorReason);
        }
    }

    [Fact]
    public void RowAllADraftThatLeftTheRegistryDuringTheUpdateIsClosedDetachedWithoutEvaluatingTheComparer()
    {
        var registry = new DraftRegistry();
        var comparer = new DraftThrowingComparer();
        var copier = new ArmedCopy();
        var draft = Born(comparer, copier.Copy, registry);
        draft.SetCurrentDraftState(DraftKit.Form("Edited", 31));
        Assert.True(draft.TryBeginApply().IsStarted);
        registry.Discard(DraftKit.S1, "R1");
        comparer.Throw = true;
        copier.Arm(1);
        DraftApplyResult result = default;

        var thrown = Record.Exception(() => result = draft.CompleteApply(DraftApplyOutcome<DraftForm>.Applied(Fresh())));

        Assert.Null(thrown);
        Assert.True(result.Accepted);
        Assert.False(result.DraftWasRegistered);
        Assert.Equal(DraftStatus.Applying, result.Status);
        // The slot was freed anyway.
        var next = Born(DraftKit.Comparer, DraftKit.CopyForm, registry, "R2");
        Assert.True(next.TryBeginApply().IsStarted);
    }

    // ------------------------------------------------------------------ F4-E16: the warnings survive an outcome without an admitted re-read

    [Fact]
    public void TheWarningsThatWereKnownSurviveWhenThePostRereadFailsOrIsNotAdmitted()
    {
        var failed = DraftApplyOutcome<DraftForm>.AppliedPostRereadFailed("post failed", new[] { "w1", "w2" });
        Assert.Equal(new[] { "w1", "w2" }, failed.Warnings.ToArray());
        Assert.Equal("post failed", failed.Cause);
        Assert.Empty(DraftApplyOutcome<DraftForm>.AppliedPostRereadFailed("post failed").Warnings);
        Assert.Empty(DraftApplyOutcome<DraftForm>.AppliedPostRereadFailed("post failed", null).Warnings);
        Assert.Throws<ArgumentException>(() => DraftApplyOutcome<DraftForm>.AppliedPostRereadFailed(string.Empty, new[] { "w" }));

        // A copy is taken on receipt.
        var list = new List<string> { "w" };
        var copy = DraftApplyOutcome<DraftForm>.AppliedPostRereadFailed("post failed", list);
        list.Add("later");
        Assert.Equal(new[] { "w" }, copy.Warnings.ToArray());

        var draft = DraftKit.Dirty();
        Assert.True(draft.TryBeginApply().IsStarted);
        draft.CompleteApply(failed);
        Assert.Equal(DraftStatus.Unknown, draft.Status);
        Assert.Equal(new[] { "w1", "w2" }, draft.Warnings.ToArray());

        var notAdmitted = DraftKit.Dirty();
        Assert.True(notAdmitted.TryBeginApply().IsStarted);
        var result = notAdmitted.CompleteApply(DraftApplyOutcome<DraftForm>.AppliedWithWarnings(DraftIntentRead<DraftForm>.NotAdmitted("not admitted"), new[] { "w3" }));
        Assert.Equal(DraftApplyOutcomeKind.AppliedPostRereadFailed, result.Kind);
        Assert.Equal(DraftStatus.Unknown, notAdmitted.Status);
        Assert.Equal(new[] { "w3" }, notAdmitted.Warnings.ToArray());
    }
}
