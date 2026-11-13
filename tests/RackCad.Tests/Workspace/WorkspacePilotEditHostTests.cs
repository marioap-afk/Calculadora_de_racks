#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Formatting;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>I-64 F4-T3A: a comparer of the pilot state that can be told to throw (the draft then cannot decide whether it is dirty).</summary>
internal sealed class PilotEditThrowingComparer : IEqualityComparer<PushBackPilotDraftState>
{
    public bool Throw { get; set; } = true;

    public bool Equals(PushBackPilotDraftState? x, PushBackPilotDraftState? y)
    {
        if (Throw) throw new InvalidOperationException("the comparer threw");
        return PushBackPilotStateComparer.Instance.Equals(x, y);
    }

    public int GetHashCode(PushBackPilotDraftState obj) => PushBackPilotStateComparer.Instance.GetHashCode(obj);
}

/// <summary>I-64 F4-T3A: a probe whose active document fails, to prove that every operation of the port contains its exception.</summary>
internal sealed class PilotEditThrowingProbe : IWorkspaceHostProbe
{
    public long? ActiveDocumentNativeId => throw new InvalidOperationException("the probe failed");

    public bool IsActiveDocumentQuiescent => true;
}

/// <summary>The reader of the edit intention of these tests: the real double, which counts the reads and can act inside one.</summary>
internal sealed class PilotEditAuditedReader : IWorkspaceEditIntentReader
{
    private readonly EditIntentFakeReader _inner;

    public PilotEditAuditedReader(EditIntentFakeReader inner)
    {
        _inner = inner;
    }

    public Action<string>? InsideRead { get; set; }

    public int Reads { get; private set; }

    public EditIntentReadResult ReadIntent(SessionId session, string rackId, long? chosenReference, EditIntentWitness? witness)
    {
        Reads++;
        InsideRead?.Invoke(rackId);
        return _inner.ReadIntent(session, rackId, chosenReference, witness);
    }
}

// I-64 F4-T3A-EDIT-PORT (D-T3-02 to D-T3-11): the edit port of the pilot over the host core. No AutoCAD, no UI and no write of the drawing:
// doubles of the bridge, the probe and the readers, and the real pure cores. The cases 1 to 8, 11, 13, 14, 17 and 18 of the coverage of the
// Owner are here at the level of the host (9, 10, 12, 15 and 16 are of the surface of T3B); then the extra tests of D-T3-10.
public class WorkspacePilotEditHostTests
{
    private const string RackA = EditIntentKit.RackA;
    private const string RackB = EditIntentKit.RackB;
    private const string Key = PushBackPilotFieldAdapter.AnnotationScaleKey;

    private sealed class Env
    {
        public Env(bool wire = true)
        {
            Hr = new WorkspaceHostRig().WithDocument(100);
            Hr.Host.OpenPanel();
            Session = Hr.ActiveSession;
            Hr.Idle();
            Hr.Reader.Reads.Clear();
            Hr.Reader.Invalidations.Clear();
            Fake = new EditIntentFakeReader();
            Audited = new PilotEditAuditedReader(Fake);
            Core.SetEditIntentReader(Audited);
            if (wire) Bridge.SetLifecycleAdvisor(Core);
        }

        public WorkspaceHostRig Hr { get; }

        public WorkspaceHostCore Core => Hr.Host;

        public IWorkspacePilotEditPort Port => Core;

        public EventBridgeCore Bridge => Hr.Bridge.Core;

        public EditIntentFakeReader Fake { get; }

        public PilotEditAuditedReader Audited { get; }

        public SessionId Session { get; }

        public WorkspaceRequest Origin(SessionId? session = null) => new(session ?? Session);

        public EditIntentRequest Request(string rack = RackA, SessionId? session = null) => new(session ?? Session, rack, null, null);

        public CloseDecision Close(SessionId? session = null) => Bridge.OnBeginDocumentClose(session ?? Session);

        public void Destroy(long nativeId = 100) => Bridge.OnDocumentToBeDestroyed(nativeId);

        public DraftSession<PushBackPilotDraftState> Open(string rack = RackA, SessionId? session = null)
        {
            var outcome = Port.TryBeginEdit(Request(rack, session));
            Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
            var draft = Core.OpenDrafts.Find<PushBackPilotDraftState>(session ?? Session, rack);
            Assert.NotNull(draft);
            return draft!;
        }

        /// <summary>A draft of the pilot born with a comparer that throws (it cannot tell whether it is dirty).</summary>
        public DraftSession<PushBackPilotDraftState> OpenUndecidable(string rack = "R9", SessionId? session = null)
        {
            var read = EditIntentKit.Admitted(rack).PilotRead!.Read;
            var begun = Core.OpenDrafts.Begin(session ?? Session, rack, read, new PilotEditThrowingComparer(), PushBackPilotStateComparer.Copy);
            Assert.Equal(DraftBeginStatus.Created, begun.Status);
            return begun.Draft!;
        }

        public PilotFieldView Field(string rack = RackA, SessionId? session = null) => Port.GetField(Origin(session), rack);

        public PilotFieldResult Type(string text, string rack = RackA, SessionId? session = null) => Port.SetPendingText(Origin(session), rack, text);

        public PilotFieldResult Commit(string rack = RackA, SessionId? session = null) => Port.CommitField(Origin(session), rack);

        public PilotBoundaryResult Cross(PilotBoundaryKind kind, string rack = RackA, SessionId? session = null) => Port.CrossBoundary(Origin(session), rack, kind);

        /// <summary>The annotation scale of the base that the double of the reader answers with.</summary>
        public const double InitialScale = 1.5;

        public static double ScaleOf(DraftSession<PushBackPilotDraftState> draft) => draft.CurrentDraftState.Inputs.Annotations!.AnnotationScale;

        /// <summary>The way the host retired the session after the destruction: the "known clean" mark is shown by a clean draft whose comparer throws.</summary>
        public string PathAfter(Action<Env> operation)
        {
            Open();
            Assert.Equal(CloseDecision.Allow, Close());
            Core.OpenDrafts.Begin(Session, "R9", DraftKit.Read(), new DraftThrowingComparer { Throw = true }, DraftKit.CopyForm);
            operation(this);
            Destroy();
            return Core.LossNotices.Any(n => n.UncertainRackIds.Contains("R9")) ? "conservative" : "known-safe";
        }
    }

    // ------------------------------------------------------------------ 1: a valid text and the gesture of the field

    [Fact]
    public void AValidTextAndTheGestureOfTheFieldApplyItClearThePendingTextAndEndTheMark()
    {
        var env = new Env();
        var draft = env.Open();
        Assert.Equal(Env.InitialScale, Env.ScaleOf(draft));

        var typed = env.Type("2.5");
        var committed = env.Commit();

        Assert.Equal(PilotFieldStatus.Applied, typed.Status);
        Assert.Equal("2.5", typed.View.PendingText);
        Assert.Equal(PilotFieldStatus.Applied, committed.Status);
        Assert.Equal(2.5, Env.ScaleOf(draft));
        Assert.False(draft.HasPendingText);
        Assert.Null(committed.View.PendingText);
        Assert.Equal(PilotFieldState.Dirty, committed.View.State);
        Assert.True(committed.View.IsProtected);
        Assert.Empty(env.Hr.Faults);
    }

    [Fact]
    public void TheGestureOfTheFieldEndsTheMarkOfKnownCleanWhenItAppliesTheText()
    {
        // The pending text is put straight in the draft, so only the gesture of the field can end the mark that the Allow put.
        Assert.Equal("conservative", new Env().PathAfter(env =>
        {
            env.Core.OpenDrafts.Find<PushBackPilotDraftState>(env.Session, RackA)!.SetPendingText(Key, "2.5");
            Assert.Equal(PilotFieldStatus.Applied, env.Commit().Status);
        }));
    }

    [Fact]
    public void AnEditOfTheUserEndsTheMarkOfKnownClean()
    {
        Assert.Equal("conservative", new Env().PathAfter(env => Assert.Equal(PilotFieldStatus.Applied, env.Type("2.5").Status)));
    }

    [Fact]
    public void AValidTextAtABlockingBoundaryProceedsAppliesItAndEndsTheMark()
    {
        var env = new Env();
        var draft = env.Open();
        env.Type("3");

        var crossed = env.Cross(PilotBoundaryKind.TabChange);

        Assert.Equal(PilotBoundaryDecision.Proceeded, crossed.Decision);
        Assert.True(crossed.StateChanged);
        Assert.Empty(crossed.InvalidFields);
        Assert.Equal(3.0, Env.ScaleOf(draft));
        Assert.False(draft.HasPendingText);

        Assert.Equal("conservative", new Env().PathAfter(e =>
        {
            e.Core.OpenDrafts.Find<PushBackPilotDraftState>(e.Session, RackA)!.SetPendingText(Key, "4");
            Assert.Equal(PilotBoundaryDecision.Proceeded, e.Cross(PilotBoundaryKind.RackChange).Decision);
        }));
    }

    [Fact]
    public void WithoutPendingTextTheGestureAndTheBlockingBoundariesChangeNothingAndWithoutADraftTheyProceed()
    {
        var env = new Env();
        var draft = env.Open();

        var committed = env.Commit();
        var tab = env.Cross(PilotBoundaryKind.TabChange);
        var none = env.Cross(PilotBoundaryKind.RackChange, RackB);

        Assert.Equal(PilotFieldStatus.Applied, committed.Status);
        Assert.Equal(PilotFieldState.Clean, committed.View.State);
        Assert.Equal(PilotBoundaryDecision.Proceeded, tab.Decision);
        Assert.False(tab.StateChanged);
        Assert.Equal(PilotBoundaryDecision.Proceeded, none.Decision);
        Assert.Equal(Env.InitialScale, Env.ScaleOf(draft));
        Assert.False(draft.IsDirty);
    }

    // ------------------------------------------------------------------ 2, 3 and 4: an invalid text

    [Theory]
    [InlineData("abc", PushBackPilotFieldAdapter.NotANumberMessage)]
    [InlineData("0", PushBackPilotFieldAdapter.NotPositiveMessage)]
    [InlineData("-3", PushBackPilotFieldAdapter.NotPositiveMessage)]
    [InlineData("", PushBackPilotFieldAdapter.EmptyMessage)]
    public void AnInvalidTextAndTheGestureOfTheFieldAnswerInvalidKeepTheTextAsTypedAndLeaveTheValue(string text, string message)
    {
        var env = new Env();
        var draft = env.Open();
        env.Type(text);

        var committed = env.Commit();

        Assert.Equal(PilotFieldStatus.Invalid, committed.Status);
        Assert.Equal(message, committed.Message);
        Assert.Equal(text, committed.View.PendingText);
        Assert.Equal(message, committed.View.PendingMessage);
        Assert.Equal(text, draft.PendingText[Key]);
        Assert.Equal(Env.InitialScale, Env.ScaleOf(draft));
        Assert.Equal(PilotFieldState.Dirty, env.Field().State);
    }

    [Fact]
    public void AnInvalidTextProtectsTheRackPutsItAtRiskAndVetoesTheCloseWithItsNotice()
    {
        var env = new Env();
        env.Open();
        env.Type("abc");

        var field = env.Field();
        var risk = env.Port.GetRacksAtRisk(env.Origin());
        var decision = env.Close();
        var notices = env.Port.TakeCloseNotices();

        Assert.True(field.IsProtected);
        Assert.Equal(new[] { RackA }, risk.RackIds.ToArray());
        Assert.Empty(risk.UncertainRackIds);
        Assert.Equal(CloseDecision.Veto, decision);
        var notice = Assert.Single(notices);
        Assert.Equal(env.Session, notice.Session);
        Assert.Equal(new[] { RackA }, notice.RackIds.ToArray());
        Assert.False(notice.IsUncertain);
    }

    [Theory]
    [InlineData(PilotBoundaryKind.TabChange)]
    [InlineData(PilotBoundaryKind.RackChange)]
    public void AnInvalidTextAbortsEachBlockingBoundaryWithTheFieldAndTheMessageAndNothingChanges(PilotBoundaryKind kind)
    {
        var env = new Env();
        var draft = env.Open();
        env.Type("1x2");

        var crossed = env.Cross(kind);

        Assert.Equal(PilotBoundaryDecision.Aborted, crossed.Decision);
        Assert.Equal(kind, crossed.Kind);
        Assert.False(crossed.StateChanged);
        var problem = Assert.Single(crossed.InvalidFields);
        Assert.Equal(Key, problem.FieldKey);
        Assert.Equal(PushBackPilotFieldAdapter.NotANumberMessage, problem.Message);
        Assert.Equal("1x2", draft.PendingText[Key]);
        Assert.Equal(Env.InitialScale, Env.ScaleOf(draft));
    }

    // ------------------------------------------------------------------ 5, 6, 7 and 8: the non-blocking boundaries and the documents

    [Theory]
    [InlineData(PilotBoundaryKind.DocumentChange)]
    [InlineData(PilotBoundaryKind.PanelHide)]
    [InlineData(PilotBoundaryKind.Dematerialize)]
    [InlineData(PilotBoundaryKind.DocumentClose)]
    public void EachNonBlockingBoundaryKeepsAnInvalidTextCharacterByCharacterWithoutValidatingCommittingOrAuthorizing(PilotBoundaryKind kind)
    {
        var env = new Env();
        var draft = env.Open();
        const string typed = " 12abc,,x\t";
        env.Type(typed);
        // The session of the rack is no longer the active one: a non-blocking boundary needs no authorization.
        env.Hr.Activate(200);

        var crossed = env.Cross(kind);
        var again = env.Field();

        Assert.Equal(PilotBoundaryDecision.Kept, crossed.Decision);
        Assert.False(crossed.StateChanged);
        Assert.Empty(crossed.InvalidFields);
        Assert.Equal(typed, draft.PendingText[Key]);
        Assert.Equal(typed, again.PendingText);
        Assert.Equal(Env.InitialScale, Env.ScaleOf(draft));
        Assert.Empty(env.Hr.Faults);
    }

    [Fact]
    public void ANonBlockingBoundaryKeepsTheTextAlsoWhenItIsValidAndDoesNotApplyIt()
    {
        var env = new Env();
        var draft = env.Open();
        env.Type("9");

        var crossed = env.Cross(PilotBoundaryKind.PanelHide);

        Assert.Equal(PilotBoundaryDecision.Kept, crossed.Decision);
        Assert.Equal("9", env.Field().PendingText);
        Assert.Equal(Env.InitialScale, Env.ScaleOf(draft));
    }

    [Fact]
    public void ThePendingTextOfADocumentStaysInItAfterTheDocumentChangeAndComesBackWhenTheDocumentIsActiveAgain()
    {
        var env = new Env();
        env.Open();
        env.Type("12abc");
        var a = env.Session;

        var b = env.Hr.Activate(200);
        env.Open(RackA, b);
        var duringB = env.Field(RackA, a);
        var other = env.Field(RackA, b);
        env.Cross(PilotBoundaryKind.DocumentChange, RackA, a);
        env.Hr.Activate(100);
        var back = env.Field(RackA, a);

        Assert.Equal("12abc", duringB.PendingText);
        Assert.Null(other.PendingText);
        Assert.Equal("12abc", back.PendingText);
        Assert.Equal(PilotFieldState.Dirty, back.State);
        Assert.True(back.CanEdit);
    }

    // ------------------------------------------------------------------ 11 and 14: the discard

    [Fact]
    public void DiscardRackRemovesTheDraftAndItsPendingTextOfOneRackWithoutTouchingTheOthers()
    {
        var env = new Env();
        var a = env.Open(RackA);
        var b = env.Open(RackB);
        env.Type("5", RackA);
        env.Type("6", RackB);
        var other = env.Hr.Activate(200);
        env.Open(RackA, other);
        env.Type("7", RackA, other);
        env.Hr.Activate(100);

        var outcome = env.Port.DiscardRack(env.Origin(), RackA);

        Assert.Equal(EditDiscardStatus.Discarded, outcome.Status);
        Assert.Equal(new[] { RackA }, outcome.RemovedRackIds.ToArray());
        Assert.Equal(new[] { RackA }, outcome.PendingTextRackIds.ToArray());
        Assert.False(a.IsRegistered);
        Assert.False(env.Field(RackA).Exists);
        Assert.Equal(PilotFieldState.NotOpen, env.Field(RackA).State);
        Assert.Equal("6", env.Field(RackB).PendingText);
        Assert.True(b.IsRegistered);
        Assert.Equal("7", env.Field(RackA, other).PendingText);
        Assert.Empty(env.Hr.Faults);
    }

    [Fact]
    public void DiscardRackNeedsNoComparerAndRemovesAnUndecidableDraftToo()
    {
        var env = new Env();
        var draft = env.OpenUndecidable("R9");
        Assert.Equal(PilotFieldState.Undecidable, env.Field("R9").State);

        PilotEditOutcomeProbe.Run(() => env.Port.DiscardRack(env.Origin(), "R9"), out var outcome, out var thrown);

        Assert.Null(thrown);
        Assert.Equal(EditDiscardStatus.Discarded, outcome.Status);
        Assert.Equal(new[] { "R9" }, outcome.RemovedRackIds.ToArray());
        Assert.Empty(outcome.PendingTextRackIds);
        Assert.False(draft.IsRegistered);
        Assert.Empty(env.Hr.Faults);
    }

    [Fact]
    public void DiscardRackOfARackWithoutADraftIsDiscardedWithEmptyListsAndABlankRackToo()
    {
        var env = new Env();

        var none = env.Port.DiscardRack(env.Origin(), RackB);
        var blank = env.Port.DiscardRack(env.Origin(), "  ");

        foreach (var outcome in new[] { none, blank })
        {
            Assert.Equal(EditDiscardStatus.Discarded, outcome.Status);
            Assert.Empty(outcome.RemovedRackIds);
            Assert.Empty(outcome.PendingTextRackIds);
        }
    }

    [Fact]
    public void DiscardRackAndDiscardDocumentRemoveTheStateAndTheNextCloseIsAllowed()
    {
        var env = new Env();
        env.Open(RackA);
        env.Open(RackB);
        env.Type("abc", RackA);
        Assert.Equal(CloseDecision.Veto, env.Close());

        Assert.Equal(EditDiscardStatus.Discarded, env.Port.DiscardRack(env.Origin(), RackA).Status);
        Assert.Equal(CloseDecision.Allow, env.Close());

        env.Type("abc", RackB);
        Assert.Equal(CloseDecision.Veto, env.Close());
        var all = env.Port.DiscardDocument(env.Origin());

        Assert.Equal(EditDiscardStatus.Discarded, all.Status);
        Assert.Equal(new[] { RackB }, all.RemovedRackIds.ToArray());
        Assert.False(env.Field(RackB).Exists);
        Assert.Equal(CloseDecision.Allow, env.Close());
    }

    [Fact]
    public void TheDiscardOfARackEndsTheMarkOfKnownClean()
    {
        Assert.Equal("conservative", new Env().PathAfter(env => Assert.Equal(EditDiscardStatus.Discarded, env.Port.DiscardRack(env.Origin(), RackA).Status)));
    }

    // ------------------------------------------------------------------ 13: the state persists without a discard

    [Fact]
    public void WithoutADiscardTheStateOfTheDraftPersistsThroughQueriesAndNonBlockingBoundaries()
    {
        var env = new Env();
        var draft = env.Open();
        env.Type("1.5x");
        var before = env.Field();

        for (var i = 0; i < 3; i++)
        {
            env.Field();
            env.Port.GetRacksAtRisk(env.Origin());
            foreach (var kind in new[] { PilotBoundaryKind.DocumentChange, PilotBoundaryKind.PanelHide, PilotBoundaryKind.Dematerialize, PilotBoundaryKind.DocumentClose })
                env.Cross(kind);
        }

        var after = env.Field();
        Assert.True(draft.IsRegistered);
        Assert.Equal(before.PendingText, after.PendingText);
        Assert.Equal(before.State, after.State);
        Assert.Equal(before.CommittedText, after.CommittedText);
        Assert.Equal(new[] { RackA }, env.Port.GetRacksAtRisk(env.Origin()).RackIds.ToArray());
    }

    // ------------------------------------------------------------------ 17: isolation between documents

    [Fact]
    public void TheOperationsOfADocumentNeverTouchTheDraftsOfAnotherAndABlockingMutationOfAnotherDocumentIsRejected()
    {
        var env = new Env();
        var a = env.Session;
        var draftA = env.Open(RackA, a);
        env.Type("2", RackA, a);
        var b = env.Hr.Activate(200);
        var draftB = env.Open(RackA, b);
        env.Type("8x", RackA, b);
        env.Hr.Activate(100);

        // With A active, the operations that change something and name B are refused and change nothing.
        var typed = env.Type("77", RackA, b);
        var committed = env.Commit(RackA, b);
        var crossed = env.Cross(PilotBoundaryKind.TabChange, RackA, b);
        var discarded = env.Port.DiscardRack(env.Origin(b), RackA);
        var all = env.Port.DiscardDocument(env.Origin(b));

        Assert.Equal(PilotFieldStatus.Rejected, typed.Status);
        Assert.Equal(PilotFieldStatus.Rejected, committed.Status);
        Assert.Equal(PilotBoundaryDecision.Aborted, crossed.Decision);
        Assert.False(string.IsNullOrEmpty(crossed.Reason));
        Assert.Equal(EditDiscardStatus.Rejected, discarded.Status);
        Assert.Equal(EditDiscardStatus.Rejected, all.Status);
        Assert.Equal("8x", draftB.PendingText[Key]);
        Assert.True(draftB.IsRegistered);
        Assert.Equal("2", draftA.PendingText[Key]);

        // And the operations of A change A alone.
        Assert.Equal(PilotFieldStatus.Applied, env.Type("3", RackA, a).Status);
        Assert.Equal("8x", draftB.PendingText[Key]);
        Assert.Equal(PilotFieldStatus.Applied, env.Commit(RackA, a).Status);
        Assert.Equal(3.0, Env.ScaleOf(draftA));
        Assert.Equal(Env.InitialScale, Env.ScaleOf(draftB));
    }

    // ------------------------------------------------------------------ 18: the modal scope

    [Fact]
    public void TheModalScopeSwitchesOnBeforeItReturnsOffAtTheDisposeAlsoWithAnExceptionAndNestsAndDisposesTwiceHarmlessly()
    {
        var calls = new List<bool>();
        var scope = new WorkspacePanelModalScope(calls.Add);

        var first = scope.Enter();
        Assert.Equal(new[] { true }, calls.ToArray());
        first.Dispose();
        Assert.Equal(new[] { true, false }, calls.ToArray());
        first.Dispose();
        Assert.Equal(new[] { true, false }, calls.ToArray());

        calls.Clear();
        Action throwing = () =>
        {
            using (scope.Enter())
            {
                Assert.Equal(new[] { true }, calls.ToArray());
                throw new InvalidOperationException("body");
            }
        };
        Assert.Throws<InvalidOperationException>(throwing);
        Assert.Equal(new[] { true, false }, calls.ToArray());

        calls.Clear();
        var outer = scope.Enter();
        var inner = scope.Enter();
        Assert.Equal(2, scope.Depth);
        inner.Dispose();
        Assert.Equal(new[] { true }, calls.ToArray());
        inner.Dispose();
        Assert.Equal(new[] { true }, calls.ToArray());
        Assert.Equal(1, scope.Depth);
        outer.Dispose();
        Assert.Equal(new[] { true, false }, calls.ToArray());
        Assert.Equal(0, scope.Depth);
    }

    [Fact]
    public void WithTheModalActiveTheIntentionWaitsAndCanDrainNowIsFalseUntilTheLastScopeEnds()
    {
        var env = new Env();
        env.Bridge.OnSourceEvent(env.Session, BridgeSource.ObjectModified);
        var scope = new WorkspacePanelModalScope(env.Core.SetModalRackCadActive);
        Assert.True(env.Bridge.CanDrainNow());

        var outer = scope.Enter();
        var inner = scope.Enter();
        var waiting = env.Port.TryBeginEdit(env.Request());
        var drain = env.Bridge.CanDrainNow();
        inner.Dispose();
        var stillWaiting = env.Port.TryBeginEdit(env.Request());
        outer.Dispose();

        Assert.Equal(EditIntentStatus.Waiting, waiting.Status);
        Assert.False(drain);
        Assert.Equal(EditIntentStatus.Waiting, stillWaiting.Status);
        Assert.Equal(0, env.Audited.Reads);
        Assert.True(env.Bridge.CanDrainNow());
        Assert.Equal(EditIntentStatus.Admitted, env.Port.TryBeginEdit(env.Request()).Status);
        Assert.Equal(new[] { true, false }, env.Hr.Link.ModalHistory.ToArray());
    }

    // ------------------------------------------------------------------ GetField in every state

    [Fact]
    public void GetFieldReportsEveryStateOfTheFieldAndWhatCanBeEdited()
    {
        var env = new Env();
        Assert.Equal(PilotFieldState.NotOpen, env.Field().State);
        Assert.False(env.Field().Exists);
        Assert.False(env.Field().CanEdit);

        var clean = env.Open(RackA);
        var view = env.Field();
        Assert.Equal((true, PilotFieldState.Clean, false, false, true), (view.Exists, view.State, view.IsPossiblyStale, view.IsProtected, view.CanEdit));
        Assert.Equal(WorkspacePilotValueText.Show(Env.InitialScale, out _), view.CommittedText);
        Assert.Null(view.PendingText);
        Assert.Null(view.PendingMessage);

        env.Type("abc");
        var dirty = env.Field();
        Assert.Equal((PilotFieldState.Dirty, true, true), (dirty.State, dirty.IsProtected, dirty.CanEdit));
        Assert.Equal(PushBackPilotFieldAdapter.NotANumberMessage, dirty.PendingMessage);
        Assert.Equal("abc", dirty.PendingText);

        var conflict = env.Open(RackB);
        Assert.True(conflict.TryBeginApply().IsStarted);
        var applying = env.Field(RackB);
        Assert.Equal((PilotFieldState.Applying, false), (applying.State, applying.CanEdit));
        conflict.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.NotAppliedConflict(new[] { new DraftDifference("A1", DraftDifferenceKind.EnvelopeText) }));
        var conflicted = env.Field(RackB);
        Assert.Equal((PilotFieldState.Conflict, true), (conflicted.State, conflicted.CanEdit));

        var unknown = env.Open("R7");
        Assert.True(unknown.TryBeginApply().IsStarted);
        unknown.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("the write threw"));
        var unknownView = env.Field("R7");
        Assert.Equal((PilotFieldState.Unknown, false), (unknownView.State, unknownView.CanEdit));

        var error = env.Open("R8");
        error.EnterError("boom");
        var errorView = env.Field("R8");
        Assert.Equal((PilotFieldState.Error, false), (errorView.State, errorView.CanEdit));

        Assert.True(clean.IsRegistered);
    }

    [Fact]
    public void AComparerThatThrowsMakesTheFieldUndecidableProtectedAndNotEditableAndGetFieldNeverThrows()
    {
        var env = new Env();
        env.OpenUndecidable("R9");

        PilotFieldView view = default;
        var thrown = Record.Exception(() => view = env.Field("R9"));

        Assert.Null(thrown);
        Assert.Equal(PilotFieldState.Undecidable, view.State);
        Assert.True(view.IsProtected);
        Assert.False(view.CanEdit);
        Assert.True(view.Exists);
        Assert.Equal(PilotFieldStatus.NotEditable, env.Type("2", "R9").Status);
        Assert.Equal(PilotFieldStatus.NotEditable, env.Commit("R9").Status);
        Assert.Empty(env.Hr.Faults);
    }

    [Fact]
    public void GetFieldAndGetRacksAtRiskNeedNoAuthorizationAndNeverThrowEvenForABlankRackOrADocumentThatIsNotActive()
    {
        var env = new Env();
        env.Open();
        env.Type("5");
        var a = env.Session;
        env.Hr.Activate(200);

        var field = env.Field(RackA, a);
        var blank = env.Field("   ", a);
        var nothing = env.Field(RackA, new SessionId(999));
        var risk = env.Port.GetRacksAtRisk(env.Origin(a));

        Assert.True(field.Exists);
        Assert.Equal("5", field.PendingText);
        Assert.False(blank.Exists);
        Assert.False(nothing.Exists);
        Assert.Equal(new[] { RackA }, risk.RackIds.ToArray());
        Assert.Equal(1, env.Audited.Reads);
    }

    // ------------------------------------------------------------------ SetPendingText: refusals, the key and the text that lives

    [Fact]
    public void SetPendingTextIsRefusedWhenTheFieldCannotBeEditedWithoutAuthorizationAndWhenThereIsNoDraft()
    {
        var env = new Env();
        var draft = env.Open();
        var error = env.Open(RackB);
        error.EnterError("boom");

        var notEditable = env.Type("2", RackB);
        var noDraft = env.Type("2", "R7");
        var blank = env.Type("2", " ");
        var nullText = env.Port.SetPendingText(env.Origin(), RackA, null!);
        env.Hr.Activate(200);
        var unauthorized = env.Type("2", RackA, env.Session);

        Assert.Equal(PilotFieldStatus.NotEditable, notEditable.Status);
        Assert.False(string.IsNullOrEmpty(notEditable.Reason));
        Assert.False(error.HasPendingText);
        Assert.Equal(PilotFieldStatus.NotEditable, noDraft.Status);
        Assert.Equal(PilotFieldStatus.NotEditable, blank.Status);
        Assert.Equal(PilotFieldStatus.Rejected, nullText.Status);
        Assert.Equal(PilotFieldStatus.Rejected, unauthorized.Status);
        Assert.False(string.IsNullOrEmpty(unauthorized.Reason));
        Assert.False(draft.HasPendingText);
        Assert.Empty(env.Hr.Faults);
    }

    [Fact]
    public void TheKeyOfThePendingTextIsAlwaysTheOneOfTheAdapterAndAnEmptyTextIsKeptToo()
    {
        var env = new Env();
        var draft = env.Open();

        env.Type("4");
        env.Type(string.Empty);

        Assert.Equal(new[] { PushBackPilotFieldAdapter.AnnotationScaleKey }, draft.PendingText.Keys.ToArray());
        Assert.Equal(string.Empty, draft.PendingText[Key]);
        Assert.Equal(string.Empty, env.Field().PendingText);
        Assert.Equal(PushBackPilotFieldAdapter.EmptyMessage, env.Field().PendingMessage);
    }

    [Fact]
    public void AfterAnySequenceOfAcceptedAndRejectedEditsTheFieldHoldsTheLastAcceptedText()
    {
        var env = new Env();
        var draft = env.Open();
        var lastAccepted = (string?)null;

        foreach (var text in new[] { "1", "12", "12x", "", "  7 ", "9,5" })
        {
            Assert.Equal(PilotFieldStatus.Applied, env.Type(text).Status);
            lastAccepted = text;
            Assert.Equal(lastAccepted, env.Field().PendingText);
        }

        // The field stops being editable: the edits are refused and the last accepted text stays.
        draft.EnterError("boom");
        foreach (var text in new[] { "100", "abc" })
        {
            Assert.Equal(PilotFieldStatus.NotEditable, env.Type(text).Status);
            Assert.Equal(lastAccepted, env.Field().PendingText);
        }

        // A request of another session is refused too.
        env.Hr.Activate(200);
        Assert.Equal(PilotFieldStatus.Rejected, env.Type("5", RackA, env.Session).Status);
        Assert.Equal(lastAccepted, env.Field(RackA, env.Session).PendingText);
        Assert.Equal(lastAccepted, draft.PendingText[Key]);
    }

    // ------------------------------------------------------------------ the gate of reentry of the three reads

    private static void MakeStale(Env env)
    {
        env.Bridge.OnSourceEvent(env.Session, BridgeSource.ObjectModified);
        env.Hr.Idle();
    }

    [Fact]
    public void WhileTheReloadReadsEveryOperationThatChangesSomethingAnswersBusyAndASecondReadWaitsAllWithoutEffect()
    {
        var env = new Env();
        var draft = env.Open();
        MakeStale(env);
        Assert.True(draft.NeedsLazyReload);
        var inside = new Dictionary<string, object>();
        env.Audited.InsideRead = _ =>
        {
            inside["type"] = env.Type("55");
            inside["commit"] = env.Commit();
            inside["tab"] = env.Cross(PilotBoundaryKind.TabChange);
            inside["rack"] = env.Cross(PilotBoundaryKind.RackChange);
            inside["hide"] = env.Cross(PilotBoundaryKind.PanelHide);
            inside["discardRack"] = env.Port.DiscardRack(env.Origin(), RackA);
            inside["discardDoc"] = env.Port.DiscardDocument(env.Origin());
            inside["begin"] = env.Port.TryBeginEdit(env.Request());
            inside["check"] = env.Port.TryCheckNow(env.Request());
            inside["reload"] = env.Port.TryReload(env.Request());
            inside["field"] = env.Field();
            inside["risk"] = env.Port.GetRacksAtRisk(env.Origin());
        };

        var before = env.Audited.Reads;

        var outcome = env.Port.TryReload(env.Request());

        Assert.Equal(EditIntentStatus.Reloaded, outcome.Status);
        Assert.Equal(before + 1, env.Audited.Reads);
        Assert.Equal(PilotFieldStatus.Busy, ((PilotFieldResult)inside["type"]).Status);
        Assert.Equal(PilotFieldStatus.Busy, ((PilotFieldResult)inside["commit"]).Status);
        Assert.Equal(PilotBoundaryDecision.Busy, ((PilotBoundaryResult)inside["tab"]).Decision);
        Assert.Equal(PilotBoundaryDecision.Busy, ((PilotBoundaryResult)inside["rack"]).Decision);
        Assert.Equal(PilotBoundaryDecision.Kept, ((PilotBoundaryResult)inside["hide"]).Decision);
        Assert.Equal(EditDiscardStatus.Busy, ((EditDiscardOutcome)inside["discardRack"]).Status);
        Assert.Equal(EditDiscardStatus.Busy, ((EditDiscardOutcome)inside["discardDoc"]).Status);
        Assert.Equal(EditIntentStatus.Waiting, ((EditIntentOutcome)inside["begin"]).Status);
        Assert.Equal(EditCheckStatus.Waiting, ((EditCheckOutcome)inside["check"]).Status);
        Assert.Equal(EditIntentStatus.Waiting, ((EditIntentOutcome)inside["reload"]).Status);
        Assert.True(((PilotFieldView)inside["field"]).Exists);
        Assert.Empty(((PilotRiskView)inside["risk"]).RackIds);

        // Nothing was lost in silence: the press was refused (Busy) and the reload went on; the draft is the reloaded one, clean and registered.
        Assert.True(draft.IsRegistered);
        Assert.False(draft.HasPendingText);
        Assert.Equal(PilotFieldState.Clean, env.Field().State);
    }

    [Fact]
    public void TheMarkOfReadInProgressIsClearedWhenTheReadEndsAlsoWhenTheReaderThrows()
    {
        var env = new Env();
        env.Open();
        MakeStale(env);
        env.Fake.Throw = new InvalidOperationException("the read threw");

        var failed = env.Port.TryReload(env.Request());
        env.Fake.Throw = null;
        var typed = env.Type("4");
        var again = env.Port.TryReload(env.Request());

        Assert.Equal(EditIntentStatus.Failed, failed.Status);
        Assert.Equal(PilotFieldStatus.Applied, typed.Status);
        // The draft now has pending text and is dirty: it is not reloaded and no read is made.
        Assert.Equal(EditIntentStatus.NotNeeded, again.Status);
    }

    [Fact]
    public void ABeginInsideABeginAndACheckInsideACheckWaitWithoutReading()
    {
        var env = new Env();
        EditIntentOutcome? nestedBegin = null;
        env.Audited.InsideRead = _ => nestedBegin ??= env.Port.TryBeginEdit(env.Request(RackB));

        var outcome = env.Port.TryBeginEdit(env.Request());

        Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
        Assert.Equal(EditIntentStatus.Waiting, nestedBegin!.Value.Status);
        Assert.Equal(1, env.Audited.Reads);
        Assert.False(env.Field(RackB).Exists);

        var unknown = env.Open(RackB);
        Assert.True(unknown.TryBeginApply().IsStarted);
        unknown.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("the write threw"));
        EditCheckOutcome? nestedCheck = null;
        env.Audited.InsideRead = _ => nestedCheck ??= env.Port.TryCheckNow(env.Request(RackB));
        var reads = env.Audited.Reads;

        var check = env.Port.TryCheckNow(env.Request(RackB));

        Assert.Equal(reads + 1, env.Audited.Reads);
        Assert.Equal(EditCheckStatus.Waiting, nestedCheck!.Value.Status);
        Assert.Contains(check.Status, new[] { EditCheckStatus.Recovered, EditCheckStatus.Conflict });
    }

    [Fact]
    public void TheThreeReadsOfThePortAreTheOperationsOfTheHostAndNothingElseReadsTheDrawing()
    {
        var env = new Env();
        var begin = env.Port.TryBeginEdit(env.Request());
        Assert.Equal(EditIntentStatus.Admitted, begin.Status);
        Assert.Equal(EditWitnessStatus.NotSupplied, begin.Witness);
        Assert.Equal(1, env.Audited.Reads);
        var call = Assert.Single(env.Fake.Calls);
        Assert.Null(call.Witness);

        env.Type("3");
        env.Commit();
        env.Cross(PilotBoundaryKind.TabChange);
        env.Cross(PilotBoundaryKind.DocumentClose);
        env.Field();
        env.Port.GetRacksAtRisk(env.Origin());
        env.Port.TakeCloseNotices();
        env.Port.TakeLossNotices();
        env.Port.DiscardRack(env.Origin(), RackA);
        env.Port.DiscardDocument(env.Origin());

        Assert.Equal(1, env.Audited.Reads);
        Assert.Equal(0, env.Hr.Reader.ReadCalls);
        Assert.Empty(env.Hr.Reader.Invalidations);

        // The reload and the check go through the same reader, once each.
        env.Open();
        MakeStale(env);
        Assert.Equal(EditIntentStatus.Reloaded, env.Port.TryReload(env.Request()).Status);
        Assert.Equal(EditCheckStatus.NotApplicable, env.Port.TryCheckNow(env.Request()).Status);
        Assert.Equal(3, env.Audited.Reads);
    }

    // ------------------------------------------------------------------ the gate of notices

    [Fact]
    public void InsideANoticeOfThePanelTheThreeReadsWaitWithoutDelegatingAndWithoutReading()
    {
        var env = new Env();
        var depth = new WorkspaceNoticeDepth();
        var gate = new WorkspacePilotEditGate(env.Core, depth);
        var results = new List<(EditIntentStatus Begin, EditCheckStatus Check, EditIntentStatus Reload)>();
        Action stateChanged = () => results.Add((gate.TryBeginEdit(env.Request()).Status, gate.TryCheckNow(env.Request()).Status, gate.TryReload(env.Request()).Status));

        // The one place that raises the notice: the depth is above zero while the handler runs, and goes back after it, also with a throw.
        void Notify()
        {
            depth.Enter();
            try
            {
                stateChanged();
            }
            finally
            {
                depth.Leave();
            }
        }

        Notify();

        Assert.Equal((EditIntentStatus.Waiting, EditCheckStatus.Waiting, EditIntentStatus.Waiting), Assert.Single(results));
        Assert.Equal(0, env.Audited.Reads);
        Assert.False(depth.IsInside);
        Assert.Equal(EditIntentStatus.Admitted, gate.TryBeginEdit(env.Request()).Status);
        Assert.Equal(1, env.Audited.Reads);

        stateChanged = () => throw new InvalidOperationException("handler");
        Assert.Throws<InvalidOperationException>(Notify);
        Assert.Equal(0, depth.Value);
    }

    [Fact]
    public void TheDepthOfNoticesNestsNeverGoesBelowZeroAndTheGateDelegatesEverythingElseAsItIs()
    {
        var env = new Env();
        var depth = new WorkspaceNoticeDepth();
        var gate = new WorkspacePilotEditGate(env.Core, depth);
        env.Open();
        depth.Enter();
        depth.Enter();
        depth.Leave();

        Assert.True(depth.IsInside);
        Assert.Equal(PilotFieldStatus.Applied, gate.SetPendingText(env.Origin(), RackA, "6").Status);
        Assert.Equal("6", gate.GetField(env.Origin(), RackA).PendingText);
        Assert.Equal(PilotFieldStatus.Applied, gate.CommitField(env.Origin(), RackA).Status);
        Assert.Equal(PilotBoundaryDecision.Proceeded, gate.CrossBoundary(env.Origin(), RackA, PilotBoundaryKind.TabChange).Decision);
        Assert.Equal(new[] { RackA }, gate.GetRacksAtRisk(env.Origin()).RackIds.ToArray());
        Assert.Equal(EditDiscardStatus.Discarded, gate.DiscardRack(env.Origin(), RackA).Status);
        Assert.Equal(EditDiscardStatus.Discarded, gate.DiscardDocument(env.Origin()).Status);
        Assert.Empty(gate.TakeLossNotices());
        Assert.Empty(gate.TakeCloseNotices());

        depth.Leave();
        depth.Leave();
        depth.Leave();
        Assert.Equal(0, depth.Value);
        Assert.False(depth.IsInside);
        Assert.Throws<ArgumentNullException>(() => new WorkspacePilotEditGate(null!, depth));
        Assert.Throws<ArgumentNullException>(() => new WorkspacePilotEditGate(env.Core, null!));
    }

    // ------------------------------------------------------------------ D-T3-03: the text of the committed value

    private static void InCulture(string name, Action body)
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = name.Length == 0 ? CultureInfo.InvariantCulture : new CultureInfo(name);
            body();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    private static IEnumerable<double> TextCorpus()
    {
        yield return 1;
        yield return 2.25;
        yield return 1.23456;
        yield return 0.1;
        yield return 1E-05;
        yield return 1E+15;
        yield return 1E+20;
        yield return 123456789.125;
        yield return 100;
        yield return double.Epsilon;
        yield return double.MaxValue;
        var random = new Random(20261113);
        var count = 0;
        while (count < 400)
        {
            var value = BitConverter.Int64BitsToDouble(random.NextInt64());
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0.0) continue;
            count++;
            yield return value;
        }
    }

    [Theory]
    [InlineData("es-MX")]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    [InlineData("")]
    public void TheShownTextOfAFiniteValueGreaterThanZeroIsReadBackAsTheSameDoubleAndPassesTheValidationInEveryCulture(string culture)
    {
        InCulture(culture, () =>
        {
            var separator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            var adapter = new PushBackPilotFieldAdapter();
            foreach (var value in TextCorpus())
            {
                var text = WorkspacePilotValueText.Show(value, out var reason);

                Assert.NotNull(text);
                Assert.Null(reason);
                Assert.True(LocalizedNumberParser.TryDouble(text!, out var back), "unreadable: " + text);
                Assert.Equal(BitConverter.DoubleToInt64Bits(value), BitConverter.DoubleToInt64Bits(back));
                Assert.True(adapter.Validate(Key, text!).IsValid, "invalid: " + text);
                Assert.DoesNotContain("E", text!, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("+", text!, StringComparison.Ordinal);
                Assert.DoesNotContain("-", text!, StringComparison.Ordinal);
                Assert.True(text!.Split(separator).Length <= 2);
                Assert.False(text.StartsWith(separator, StringComparison.Ordinal));
                Assert.False(text.EndsWith(separator, StringComparison.Ordinal));
                Assert.True(text.Length == 1 || text[0] != '0' || text.StartsWith("0" + separator, StringComparison.Ordinal));
            }
        });
    }

    [Theory]
    [InlineData("es-MX", ".")]
    [InlineData("en-US", ".")]
    [InlineData("de-DE", ",")]
    [InlineData("", ".")]
    public void TheShownTextHasTheExpectedPlainDigitsAndTheDecimalSeparatorOfTheCurrentCulture(string culture, string separator)
    {
        InCulture(culture, () =>
        {
            Assert.Equal("1", WorkspacePilotValueText.Show(1, out _));
            Assert.Equal("2" + separator + "25", WorkspacePilotValueText.Show(2.25, out _));
            Assert.Equal("1" + separator + "23456", WorkspacePilotValueText.Show(1.23456, out _));
            Assert.Equal("0" + separator + "1", WorkspacePilotValueText.Show(0.1, out _));
            Assert.Equal("0" + separator + "00001", WorkspacePilotValueText.Show(1E-05, out _));
            Assert.Equal("1000000000000000", WorkspacePilotValueText.Show(1E+15, out _));
            Assert.Equal("100000000000000000000", WorkspacePilotValueText.Show(1E+20, out _));
            Assert.Equal("100", WorkspacePilotValueText.Show(100, out _));
            Assert.Equal("0" + separator + new string('0', 323) + "5", WorkspacePilotValueText.Show(double.Epsilon, out _));
            Assert.Equal("17976931348623157" + new string('0', 292), WorkspacePilotValueText.Show(double.MaxValue, out _));
        });
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void AValueOutsideTheDomainHasNoTextAndABlankTextIsNeverCreated(double value)
    {
        var text = WorkspacePilotValueText.Show(value, out var reason);

        Assert.Null(text);
        Assert.False(string.IsNullOrEmpty(reason));
    }

    [Fact]
    public void ShowingTheCommittedValueNeverCreatesPendingTextAndCommittingTheShownTextIsNotADirtyChange()
    {
        InCulture("de-DE", () =>
        {
            var env = new Env();
            var draft = env.Open();

            var shown = env.Field().CommittedText;
            env.Field();
            env.Field();
            Assert.Equal("1,5", shown);
            Assert.False(draft.HasPendingText);
            Assert.False(draft.IsDirty);

            // The user types the very text that was shown and commits it: the value does not change and the draft stays clean.
            Assert.Equal(PilotFieldStatus.Applied, env.Type(shown!).Status);
            Assert.Equal(PilotFieldStatus.Applied, env.Commit().Status);
            Assert.Equal(Env.InitialScale, Env.ScaleOf(draft));
            Assert.False(draft.IsDirty);
            Assert.Equal(PilotFieldState.Clean, env.Field().State);

            Assert.Equal(PilotFieldStatus.Applied, env.Type("2,5").Status);
            Assert.Equal(PilotFieldStatus.Applied, env.Commit().Status);
            Assert.Equal("2,5", env.Field().CommittedText);
        });
    }

    // ------------------------------------------------------------------ the queues of notices

    [Fact]
    public void TheLossNoticesAreTakenInOrderAsValuesAndLeaveTheQueueEmpty()
    {
        var env = new Env();
        var a = env.Session;
        env.Open();
        env.Type("12x");
        env.Core.OpenDrafts.Begin(a, "R9", DraftKit.Read(), new DraftThrowingComparer { Throw = true }, DraftKit.CopyForm);
        var b = env.Hr.Activate(200);
        env.Open(RackA, b);
        env.Type("3", RackA, b);

        env.Destroy(100);
        env.Destroy(200);
        var taken = env.Port.TakeLossNotices();
        var second = env.Port.TakeLossNotices();

        Assert.Equal(new[] { a, b }, taken.Select(n => n.Session).ToArray());
        Assert.Equal(new[] { RackA, "R9" }, taken[0].LostRackIds.ToArray());
        Assert.Equal(new[] { "R9" }, taken[0].UncertainRackIds.ToArray());
        Assert.Equal("the comparer threw", taken[0].UncertainReasons["R9"]);
        Assert.Equal(new[] { RackA }, taken[1].LostRackIds.ToArray());
        Assert.Empty(taken[1].UncertainRackIds);
        Assert.Empty(second);
        Assert.Empty(env.Core.LossNotices);
    }

    [Fact]
    public void TheCloseNoticesAreOnePerSessionTheLastOneAndAreTakenInOrderLeavingTheQueueEmpty()
    {
        var env = new Env();
        var a = env.Session;
        env.Open();
        env.Type("abc");
        var b = env.Hr.Activate(200);
        env.Open(RackA, b);
        env.Type("def", RackA, b);
        env.Open(RackB, b);
        env.Type("ghi", RackB, b);

        Assert.Equal(CloseDecision.Veto, env.Close(a));
        Assert.Equal(CloseDecision.Veto, env.Close(b));
        Assert.Equal(CloseDecision.Veto, env.Close(a));
        var taken = env.Port.TakeCloseNotices();
        var second = env.Port.TakeCloseNotices();

        Assert.Equal(new[] { b, a }, taken.Select(n => n.Session).ToArray());
        Assert.Equal(new[] { RackA, RackB }.OrderBy(r => r, StringComparer.Ordinal).ToArray(), taken[0].RackIds.ToArray());
        Assert.Equal(new[] { RackA }, taken[1].RackIds.ToArray());
        Assert.All(taken, n => Assert.False(n.IsUncertain));
        Assert.Empty(second);
    }

    [Fact]
    public void ATestOfTheCloseThatFailsLeavesAnUncertainNoticeWithoutAListOfRacks()
    {
        var env = new Env();
        env.Open();
        env.Core.OpenDrafts.Begin(env.Session, "R9", DraftKit.Read(), new DraftThrowingComparer { Throw = true }, DraftKit.CopyForm);

        Assert.Equal(CloseDecision.Veto, env.Close());
        var notice = Assert.Single(env.Port.TakeCloseNotices());

        Assert.Equal(env.Session, notice.Session);
        Assert.True(notice.IsUncertain);
        Assert.Empty(notice.RackIds);
        Assert.Single(env.Hr.Faults, f => f.Context == "WorkspaceHostCore.AdviseClose");
        Assert.False(env.Bridge.IsDegraded(env.Session));
    }

    [Fact]
    public void TheNoticeOfAVetoOfADocumentThatIsNotActiveIsQueuedToo()
    {
        var env = new Env();
        var a = env.Session;
        env.Open();
        env.Type("abc");
        env.Hr.Activate(200);

        Assert.Equal(CloseDecision.Veto, env.Close(a));

        var notice = Assert.Single(env.Port.TakeCloseNotices());
        Assert.Equal(a, notice.Session);
        Assert.Equal(new[] { RackA }, notice.RackIds.ToArray());
    }

    [Fact]
    public void TheNoticeOfAVetoExpiresWhenTheSessionIsDiscardedWhenTheNextAdviceAllowsAndWhenTheSessionIsRetired()
    {
        // DiscardRack that leaves the session without risk.
        var env = new Env();
        env.Open();
        env.Type("abc");
        Assert.Equal(CloseDecision.Veto, env.Close());
        env.Port.DiscardRack(env.Origin(), RackA);
        Assert.Empty(env.Port.TakeCloseNotices());

        // DiscardRack that leaves another rack at risk keeps the notice.
        env = new Env();
        env.Open(RackA);
        env.Open(RackB);
        env.Type("abc", RackA);
        env.Type("def", RackB);
        Assert.Equal(CloseDecision.Veto, env.Close());
        env.Port.DiscardRack(env.Origin(), RackA);
        Assert.Single(env.Port.TakeCloseNotices());

        // DiscardDocument.
        env = new Env();
        env.Open();
        env.Type("abc");
        Assert.Equal(CloseDecision.Veto, env.Close());
        env.Port.DiscardDocument(env.Origin());
        Assert.Empty(env.Port.TakeCloseNotices());

        // The next advice gives Allow because the pending text went away by other means.
        env = new Env();
        var draft = env.Open();
        env.Type("abc");
        Assert.Equal(CloseDecision.Veto, env.Close());
        draft.RemovePendingText(Key);
        Assert.Equal(CloseDecision.Allow, env.Close());
        Assert.Empty(env.Port.TakeCloseNotices());

        // The session is retired: its notice goes and the loss notice takes over.
        env = new Env();
        env.Open();
        env.Type("abc");
        Assert.Equal(CloseDecision.Veto, env.Close());
        env.Destroy();
        Assert.Empty(env.Port.TakeCloseNotices());
        Assert.Single(env.Port.TakeLossNotices());
    }

    [Fact]
    public void AdviseCloseStillReadsNothingAuthorizesNothingAndDoesNotDiscardOrRetireAndTheDecisionIsTheSame()
    {
        var env = new Env();
        env.Open();
        env.Type("abc");
        var drafts = env.Core.OpenDrafts.CountOf(env.Session);

        var first = env.Close();
        var second = env.Close();

        Assert.Equal(CloseDecision.Veto, first);
        Assert.Equal(CloseDecision.Veto, second);
        Assert.Equal(1, env.Audited.Reads);
        Assert.Equal(drafts, env.Core.OpenDrafts.CountOf(env.Session));
        Assert.False(env.Core.OpenDrafts.IsRetired(env.Session));
        Assert.Single(env.Port.TakeCloseNotices());
    }

    // ------------------------------------------------------------------ the racks at risk

    [Fact]
    public void TheRacksAtRiskAreOrderedOrdinallyAndAComparerThatThrowsMakesThemUncertainApart()
    {
        var env = new Env();
        env.Open(RackB);
        env.Open(RackA);
        env.Type("1x", RackB);
        env.Type("2x", RackA);
        env.OpenUndecidable("R9");
        env.Open("R5");

        PilotRiskView risk = default;
        var thrown = Record.Exception(() => risk = env.Port.GetRacksAtRisk(env.Origin()));

        Assert.Null(thrown);
        Assert.Equal(new[] { RackA, RackB }.OrderBy(r => r, StringComparer.Ordinal).ToArray(), risk.RackIds.ToArray());
        Assert.Equal(new[] { "R9" }, risk.UncertainRackIds.ToArray());
        Assert.Empty(env.Hr.Faults);
    }

    [Fact]
    public void TheRacksAtRiskOfASessionThatIsNotTheActiveOneAreAnsweredAndADefaultValueNeverHasANullList()
    {
        var env = new Env();
        var a = env.Session;
        env.Open();
        env.Type("zz");
        env.Hr.Activate(200);

        var risk = env.Port.GetRacksAtRisk(env.Origin(a));
        var nobody = env.Port.GetRacksAtRisk(env.Origin(new SessionId(777)));

        Assert.Equal(new[] { RackA }, risk.RackIds.ToArray());
        Assert.Empty(nobody.RackIds);
        Assert.Empty(nobody.UncertainRackIds);
        Assert.NotNull(default(PilotRiskView).RackIds);
        Assert.NotNull(default(PilotRiskView).UncertainRackIds);
        Assert.NotNull(default(PilotBoundaryResult).InvalidFields);
        Assert.NotNull(default(PilotLossView).LostRackIds);
        Assert.NotNull(default(PilotLossView).UncertainRackIds);
        Assert.NotNull(default(PilotLossView).UncertainReasons);
        Assert.NotNull(default(PilotCloseView).RackIds);
        Assert.Equal(PilotFieldState.NotOpen, default(PilotFieldView).State);
        Assert.Equal(PilotFieldStatus.Rejected, default(PilotFieldResult).Status);
        Assert.Equal(EditDiscardStatus.Rejected, default(EditDiscardOutcome).Status);
    }

    // ------------------------------------------------------------------ exceptions are contained as values

    [Fact]
    public void AnExceptionOfAnOperationIsContainedAsAValueWithAReasonAndDoesNotDegradeTheSession()
    {
        var hr = new WorkspaceHostRig().WithDocument(100);
        hr.Host.OpenPanel();
        var session = hr.ActiveSession;
        var faults = new List<(string Context, Exception Error)>();
        var core = new WorkspaceHostCore(hr.Link, new PilotEditThrowingProbe(), hr.Reader, new WorkspaceInstrumentation(), (c, e) => faults.Add((c, e)));
        core.OpenPanel();
        IWorkspacePilotEditPort port = core;
        var origin = new WorkspaceRequest(session);

        var typed = port.SetPendingText(origin, RackA, "2");
        var committed = port.CommitField(origin, RackA);
        var crossed = port.CrossBoundary(origin, RackA, PilotBoundaryKind.TabChange);
        var discarded = port.DiscardRack(origin, RackA);
        var all = port.DiscardDocument(origin);
        var field = port.GetField(origin, RackA);
        var risk = port.GetRacksAtRisk(origin);

        Assert.Equal(PilotFieldStatus.Rejected, typed.Status);
        Assert.Equal("the probe failed", typed.Reason);
        Assert.Equal(PilotFieldStatus.Rejected, committed.Status);
        Assert.Equal(PilotBoundaryDecision.Aborted, crossed.Decision);
        Assert.Equal("the probe failed", crossed.Reason);
        Assert.Equal(EditDiscardStatus.Failed, discarded.Status);
        Assert.Equal(EditDiscardStatus.Failed, all.Status);
        Assert.False(field.Exists);
        Assert.Empty(risk.RackIds);
        Assert.Equal(5, faults.Count);
        Assert.Equal(WorkspaceHostStatus.Ready, core.GetSnapshot().Status);
        Assert.False(hr.Bridge.Core.IsDegraded(session));
    }

    // ------------------------------------------------------------------ the critical oracle: zero authored writes

    [Fact]
    public void NoOperationOfThePortWritesAuthoredDataOfTheDrawing()
    {
        var env = new Env();
        var port = env.Port;
        var a = env.Session;

        // The three reads.
        Assert.Equal(EditIntentStatus.Admitted, port.TryBeginEdit(env.Request()).Status);
        Assert.Equal(EditIntentStatus.Admitted, port.TryBeginEdit(env.Request(RackB)).Status);
        MakeStale(env);
        port.TryReload(env.Request());
        port.TryCheckNow(env.Request());

        // Every operation that changes something in memory, in every way it can end.
        env.Type("abc");
        env.Commit();
        env.Cross(PilotBoundaryKind.TabChange);
        env.Cross(PilotBoundaryKind.RackChange);
        env.Type("5");
        env.Cross(PilotBoundaryKind.PanelHide);
        env.Commit();
        env.Type("6");
        env.Cross(PilotBoundaryKind.TabChange);
        env.Close();
        env.Hr.Activate(200);
        env.Type("7", RackA, a);
        env.Cross(PilotBoundaryKind.DocumentChange, RackA, a);
        env.Hr.Activate(100);
        port.GetField(env.Origin(), RackA);
        port.GetRacksAtRisk(env.Origin());
        port.TakeCloseNotices();
        port.TakeLossNotices();
        port.DiscardRack(env.Origin(), RackA);
        port.DiscardDocument(env.Origin());
        env.Destroy();

        // I-64 F4-T3B1 (minor 1 of F4-T3A): this test asserts what discriminates in memory, and no more: the reader of the intention reads (and
        // only the three operations of the intention do), the bridge was asked only what it is asked in the ordinary life of the panel,
        // no RackCad modal was opened and no fault was logged. That no operation of the port writes the drawing is held by the guard by body
        // TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing (WorkspacePilotEditGuardTests) and by the fact that the
        // Application project has no dependency of writing: no double here can carry a write, so an assertion on one would be empty.
        Assert.Equal(env.Audited.Reads, env.Fake.Calls.Count);
        Assert.Equal(0, env.Hr.Reader.ReadCalls);
        Assert.Empty(env.Hr.Link.ModalHistory);
        Assert.Empty(env.Hr.Faults);
        Assert.Empty(env.Hr.Bridge.Faults);
    }

    // ------------------------------------------------------------------ I-64 F4-T3B1 (minor 3 of F4-T3A): what the review of T3A asked to cover

    // (a) A blocking boundary on a draft that cannot be edited (in error, unknown or being applied) with pending text does not validate and
    // does not apply: it answers Kept, the transition is allowed to go on and the pending text is exactly where it was (D-T3B1-11, minor 3).
    [Fact]
    public void ABlockingBoundaryOnADraftThatCannotBeEditedKeepsItsPendingTextAndNeverValidatesOrApplies()
    {
        var env = new Env();

        var error = env.Open("R8");
        Assert.Equal(DraftEditStatus.Updated, error.SetPendingText(Key, "abc").Status);
        error.EnterError("boom");

        // A draft is never applied with pending text (TryBeginApply refuses it) and a draft that cannot be edited takes none, so an unknown
        // or applying draft has no pending text and its crossing proceeds with nothing to validate; only a draft that went into error with
        // pending text can be blocked-and-not-editable at once, and for that one the crossing keeps.
        var unknown = env.Open("R7");
        Assert.True(unknown.TryBeginApply().IsStarted);
        unknown.CompleteApply(DraftApplyOutcome<PushBackPilotDraftState>.CommitThrew("the write threw"));

        var applying = env.Open(RackB);
        Assert.True(applying.TryBeginApply().IsStarted);

        foreach (var (rack, state, pending, decision) in new[]
                 {
                     ("R8", PilotFieldState.Error, "abc", PilotBoundaryDecision.Kept),
                     ("R7", PilotFieldState.Unknown, (string?)null, PilotBoundaryDecision.Proceeded),
                     (RackB, PilotFieldState.Applying, (string?)null, PilotBoundaryDecision.Proceeded),
                 })
        {
            Assert.False(env.Field(rack).CanEdit);
            Assert.Equal(state, env.Field(rack).State);

            foreach (var kind in new[] { PilotBoundaryKind.TabChange, PilotBoundaryKind.RackChange })
            {
                var crossed = env.Cross(kind, rack);

                Assert.Equal(decision, crossed.Decision);
                Assert.False(crossed.StateChanged);
                Assert.Empty(crossed.InvalidFields);
                Assert.Null(crossed.Reason);
                Assert.Equal(pending, env.Field(rack).PendingText);
            }
        }

        Assert.Empty(env.Hr.Faults);
    }

    // (b) The operations that change nothing keep the mark of "known clean" that the Allow of the close put: the session is retired as known
    // safe, not conservatively, after each of them.
    [Fact]
    public void TheOperationsThatChangeNothingKeepTheMarkOfKnownClean()
    {
        Assert.Equal("known-safe", new Env().PathAfter(env => env.Field()));
        Assert.Equal("known-safe", new Env().PathAfter(env => env.Port.GetRacksAtRisk(env.Origin())));
        Assert.Equal("known-safe", new Env().PathAfter(env =>
        {
            foreach (var kind in new[] { PilotBoundaryKind.DocumentChange, PilotBoundaryKind.PanelHide, PilotBoundaryKind.Dematerialize, PilotBoundaryKind.DocumentClose })
                Assert.Equal(PilotBoundaryDecision.Kept, env.Cross(kind).Decision);
        }));
        Assert.Equal("known-safe", new Env().PathAfter(env =>
        {
            Assert.Equal(PilotFieldStatus.Applied, env.Commit().Status);
            Assert.Equal(PilotFieldStatus.Applied, env.Commit(RackA).Status);
        }));
    }

    [Fact]
    public void ARejectedOrBusyTextKeepsTheMarkOfKnownClean()
    {
        Assert.Equal("known-safe", new Env().PathAfter(env =>
        {
            // Rejected: no text, a request of another session and a rack without a draft.
            Assert.Equal(PilotFieldStatus.Rejected, env.Port.SetPendingText(env.Origin(), RackA, null!).Status);
            Assert.Equal(PilotFieldStatus.Rejected, env.Type("2.5", RackA, new SessionId(env.Session.Value + 1000)).Status);
            Assert.Equal(PilotFieldStatus.NotEditable, env.Type("2.5", RackB).Status);
        }));

        PilotFieldStatus? busy = null;
        Assert.Equal("known-safe", new Env().PathAfter(env =>
        {
            // A read that does not admit anything is in progress when the text arrives: Busy, with no effect.
            env.Fake.Answer = EditIntentKit.OriginMissing;
            env.Audited.InsideRead = _ => busy = env.Type("2.5").Status;
            Assert.Equal(EditIntentStatus.NotAdmitted, env.Port.TryBeginEdit(env.Request(RackB)).Status);
        }));
        Assert.Equal(PilotFieldStatus.Busy, busy);
    }

    // (c) The outer catch of GetField, reached through OpenDrafts with a copier of the state that answers null: the state of the draft is null,
    // the view cannot be built, and the answer is the view in Error, protected and not editable, with the fault logged and nothing thrown.
    [Fact]
    public void TheOuterCatchOfGetFieldAnswersTheViewInErrorWhenTheCopierOfTheStateAnswersNull()
    {
        var env = new Env();
        var read = EditIntentKit.Admitted("R9").PilotRead!.Read;
        var begun = env.Core.OpenDrafts.Begin(env.Session, "R9", read, new PilotEditThrowingComparer(), _ => null!);
        Assert.Equal(DraftBeginStatus.Created, begun.Status);
        env.Hr.Faults.Clear();

        PilotFieldView view = default;
        var thrown = Record.Exception(() => view = env.Field("R9"));

        Assert.Null(thrown);
        Assert.True(view.Exists);
        Assert.Equal(PilotFieldState.Error, view.State);
        Assert.True(view.IsProtected);
        Assert.False(view.CanEdit);
        Assert.Null(view.CommittedText);
        Assert.False(string.IsNullOrWhiteSpace(view.CommittedTextReason));
        Assert.Contains(env.Hr.Faults, fault => fault.Context == "WorkspaceHostCore.GetField");
    }
}

/// <summary>A tiny helper that runs an operation and gives its value and the exception that escaped, if any.</summary>
internal static class PilotEditOutcomeProbe
{
    public static void Run(Func<EditDiscardOutcome> operation, out EditDiscardOutcome outcome, out Exception? thrown)
    {
        EditDiscardOutcome value = default;
        thrown = Record.Exception(() => value = operation());
        outcome = value;
    }
}
