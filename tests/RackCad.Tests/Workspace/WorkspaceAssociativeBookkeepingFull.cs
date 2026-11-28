#nullable enable
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using Xunit;
using RackCad.Application.Systems.PushBack.Draft;

namespace RackCad.Tests;

// I-64 F4-T4-HOST-C1 (D-C1-02, D-C1-09, INV-C1-02, INV-C1-03, IC-10): the object events of the associative framework of the host are not a change of the
// drawing. The bridge classifies them by type alone and the pure core decides: such an event queues no hint, hooks no idle source and touches nothing
// of the own scope, of the state of rest or of the counters; every other event has exactly the effect that it has today, and every one of them still
// forgets the close sequence. Everything runs in Core, over the simulated source, with the real core of the bridge and the real host core.
public class FullPanelCopyWorkspaceAssociativeBookkeepingRedTests
{
    private const ObjectEventSubject Bookkeeping = ObjectEventSubject.AssociativeBookkeeping;

    // ------------------------------------------------------------------ test 3c: the read of the panel that only raises bookkeeping leaves the draft unmarked

    private sealed class Rig
    {
        public Rig(ObjectEventSubject subject)
        {
            Host = new WorkspaceHostRig().WithDocument(100);
            Host.Host.OpenPanel();
            Session = Host.ActiveSession;
            Host.Idle();
            Reader = new FullEditIntentFakeReader();
            Reader.Answer = rackId =>
            {
                // What the host does while the read of the panel locks and unlocks the document: the objects are touched again and again,
                // INSIDE the read, as they are when AutoCAD evaluates the associative network.
                foreach (var source in new[] { BridgeSource.ObjectModified, BridgeSource.ObjectModified, BridgeSource.ObjectAppended })
                    Host.Bridge.Core.OnObjectEvent(Session, source, subject);
                return FullEditIntentKit.Admitted(rackId);
            };
            Host.Host.SetEditIntentReader(Reader);
        }

        public WorkspaceHostRig Host { get; }

        public FullEditIntentFakeReader Reader { get; }

        public SessionId Session { get; }
    }

    [Fact]
    public void AReadThatOnlyRaisesBookkeepingLeavesTheDraftUnmarked()
    {
        var rig = new Rig(Bookkeeping);

        var outcome = rig.Host.Host.TryBeginPilotEdit(new EditIntentRequest(rig.Session, FullEditIntentKit.RackA, null, null));

        Assert.Equal(EditIntentStatus.Admitted, outcome.Status);
        Assert.Single(rig.Reader.Calls);
        var draft = rig.Host.Host.OpenDrafts.Find<PushBackFullDraftState>(rig.Session, FullEditIntentKit.RackA)!;
        Assert.NotNull(draft);
        rig.Host.Idle();

        Assert.False(draft.IsPossiblyStale);
        Assert.Equal(0, rig.Host.Bridge.SessionOf(rig.Session).PendingHintCount);
        Assert.Empty(rig.Host.Faults);

        // The control of the same flow: the same events with a plain drawing object are a change of the drawing, and they do mark.
        var control = new Rig(ObjectEventSubject.Drawing);
        var controlOutcome = control.Host.Host.TryBeginPilotEdit(new EditIntentRequest(control.Session, FullEditIntentKit.RackA, null, null));
        Assert.Equal(EditIntentStatus.Admitted, controlOutcome.Status);
        var controlDraft = control.Host.Host.OpenDrafts.Find<PushBackFullDraftState>(control.Session, FullEditIntentKit.RackA)!;
        control.Host.Idle();
        Assert.True(controlDraft.IsPossiblyStale);
    }
}

// I-64 F4-T4-HOST-C1 (D-C1-02, D-C1-09, INV-C1-02, INV-C1-03, IC-10): the object events of the associative framework of the host are not a change of the
// drawing. The bridge classifies them by type alone and the pure core decides: such an event queues no hint, hooks no idle source and touches nothing
// of the own scope, of the state of rest or of the counters; every other event has exactly the effect that it has today, and every one of them still
// forgets the close sequence. Everything runs in Core, over the simulated source, with the real core of the bridge and the real host core.
public class FullPanelCopyWorkspaceAssociativeBookkeepingGreenTests
{
    private const ObjectEventSubject Bookkeeping = ObjectEventSubject.AssociativeBookkeeping;

    private static (WorkspaceBridgeRig Rig, SessionId Session) Started()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var session = rig.Open(1);
        return (rig, session);
    }

    // The private state of a session in the core has no public reader on purpose (the surface of the core is a closed list): read by reflection.
    private static T StateValue<T>(EventBridgeCore core, SessionId session, string property)
    {
        var states = (IDictionary)typeof(EventBridgeCore).GetField("_states", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(core)!;
        var state = states[session]!;
        return (T)state.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(state)!;
    }

    private static bool OwnOpen(EventBridgeCore core, SessionId session) => StateValue<bool>(core, session, "OwnMutationOpen");

    private static DrainOperation[] Drain(EventBridgeCore core)
    {
        var outcome = Assert.Single(core.OnIdle());
        Assert.True(outcome.Result.Drained);
        return outcome.Result.Operations.ToArray();
    }

    // What an event leaves in a freshly started core, observed from the outside and from the private state: the answer, the hints that wait, the
    // idle source, the scope, the state of rest, the count of foreign commands and what the next idle pass drains.
    private sealed record Effect(bool Answer, int Pending, bool IdleAttached, bool ScopeOpen, bool Quiescent, int ForeignStarts, string Drained, int Faults);

    private static Effect Observe(bool ownScopeOpen, Func<EventBridgeCore, SessionId, bool> act)
    {
        var (rig, session) = Started();
        if (ownScopeOpen) rig.Core.OpenOwnMutation(session);

        var answer = act(rig.Core, session);
        var pending = rig.SessionOf(session).PendingHintCount;
        var idleAttached = rig.Core.IsIdleAttached;
        var scopeOpen = OwnOpen(rig.Core, session);
        var quiescent = StateValue<bool>(rig.Core, session, "Quiescent");
        var foreignStarts = StateValue<int>(rig.Core, session, "ForeignCommandStarts");
        var drained = string.Join(",", rig.Core.OnIdle().SelectMany(o => o.Result.Operations));
        return new Effect(answer, pending, idleAttached, scopeOpen, quiescent, foreignStarts, drained, rig.Faults.Count);
    }

    private static readonly BridgeSource[] ObjectSources = WorkspaceBridgeRig.ObjectSources;

    // ------------------------------------------------------------------ test 3c: the read of the panel that only raises bookkeeping leaves the draft unmarked

    private sealed class Rig
    {
        public Rig(ObjectEventSubject subject)
        {
            Host = new WorkspaceHostRig().WithDocument(100);
            Host.Host.OpenPanel();
            Session = Host.ActiveSession;
            Host.Idle();
            Reader = new FullEditIntentFakeReader();
            Reader.Answer = rackId =>
            {
                // What the host does while the read of the panel locks and unlocks the document: the objects are touched again and again,
                // INSIDE the read, as they are when AutoCAD evaluates the associative network.
                foreach (var source in new[] { BridgeSource.ObjectModified, BridgeSource.ObjectModified, BridgeSource.ObjectAppended })
                    Host.Bridge.Core.OnObjectEvent(Session, source, subject);
                return FullEditIntentKit.Admitted(rackId);
            };
            Host.Host.SetEditIntentReader(Reader);
        }

        public WorkspaceHostRig Host { get; }

        public FullEditIntentFakeReader Reader { get; }

        public SessionId Session { get; }
    }

    private sealed class AllowAdvisor : IWorkspaceLifecycleAdvisor
    {
        public bool LastIntact { get; private set; }

        public bool Destroyed { get; private set; }

        public CloseDecision AdviseClose(SessionId session) => CloseDecision.Allow;

        public void SessionDestroyed(SessionId session, bool closeSequenceIntact)
        {
            Destroyed = true;
            LastIntact = closeSequenceIntact;
        }
    }

    // ------------------------------------------------------------------ test 3a: a bookkeeping event queues nothing and hooks nothing

    [Theory]
    [InlineData(BridgeSource.ObjectAppended)]
    [InlineData(BridgeSource.ObjectErased)]
    [InlineData(BridgeSource.ObjectModified)]
    [InlineData(BridgeSource.ObjectUnappended)]
    [InlineData(BridgeSource.ObjectReappended)]
    public void AnAssociativeBookkeepingObjectEventQueuesNoHintAndHooksNoIdle(BridgeSource source)
    {
        foreach (var ownScopeOpen in new[] { false, true })
        {
            var (rig, session) = Started();
            if (ownScopeOpen) rig.Core.OpenOwnMutation(session);
            Assert.False(rig.Core.IsIdleAttached);

            Assert.False(rig.Core.OnObjectEvent(session, source, Bookkeeping));

            Assert.Equal(0, rig.SessionOf(session).PendingHintCount);
            Assert.False(rig.Core.IsIdleAttached);
            Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
            Assert.Empty(rig.Faults);
            Assert.False(rig.Core.IsDegraded(session));
            Assert.Empty(rig.Core.OnIdle());
        }
    }

    // ------------------------------------------------------------------ test 3b: the bookkeeping of the end of the update does not mark the drafts

    [Fact]
    public void AfterTheUpdateClosedItsScopeTheBookkeepingOfTheReReadDoesNotMarkTheDrafts()
    {
        var (rig, session) = Started();
        rig.Core.OpenOwnMutation(session);

        // The update writes the drawing (own), its command ends and closes the scope, and then the host evaluates the associative network
        // three times before the idle pass: those are no foreign change.
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, ObjectEventSubject.Drawing);
        rig.Core.OnSourceEvent(session, BridgeSource.CommandEnded);
        Assert.False(OwnOpen(rig.Core, session));
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, Bookkeeping);
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, Bookkeeping);
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, Bookkeeping);

        var operations = Drain(rig.Core);

        Assert.Equal(new[] { DrainOperation.InvalidateNavigationCaches }, operations);
        Assert.DoesNotContain(DrainOperation.MarkDraftsPossiblyStale, operations);
    }

    // ------------------------------------------------------------------ test 4: what the correction must not change

    [Theory]
    [InlineData(BridgeSource.ObjectAppended)]
    [InlineData(BridgeSource.ObjectErased)]
    [InlineData(BridgeSource.ObjectModified)]
    [InlineData(BridgeSource.ObjectUnappended)]
    [InlineData(BridgeSource.ObjectReappended)]
    public void ADrawingObjectEventHasExactlyTheEffectOfASourceEventAlsoInsideTheOwnScope(BridgeSource source)
    {
        foreach (var ownScopeOpen in new[] { false, true })
        {
            var viaObject = Observe(ownScopeOpen, (core, session) => core.OnObjectEvent(session, source, ObjectEventSubject.Drawing));
            var viaSource = Observe(ownScopeOpen, (core, session) => core.OnSourceEvent(session, source));

            Assert.Equal(viaSource, viaObject);
            Assert.Equal(1, viaObject.Pending);
            Assert.True(viaObject.IdleAttached);
            // Own, it only invalidates; foreign, it also marks.
            Assert.Equal(ownScopeOpen ? "InvalidateNavigationCaches" : "InvalidateNavigationCaches,MarkDraftsPossiblyStale", viaObject.Drained);
        }
    }

    [Fact]
    public void ADrawingObjectChangeAndTheEndOfACommandDrainTheListOfTheBridgeTestsOfTheDrain()
    {
        // The list that WorkspaceBridgeHintAndDrainTests fixes for ObjectModified followed by CommandEnded, now through the new entry.
        var (rig, session) = Started();
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, ObjectEventSubject.Drawing);
        rig.Core.OnObjectEvent(session, BridgeSource.CommandEnded, ObjectEventSubject.Drawing);

        var outcome = Assert.Single(rig.Core.OnIdle());

        Assert.Equal(new[] { DrainOperation.InvalidateNavigationCaches, DrainOperation.MarkDraftsPossiblyStale }, outcome.Result.Operations);
    }

    [Fact]
    public void AnySourceThatIsNotAnObjectOneHasTheEffectOfASourceEventWhateverTheSubject()
    {
        foreach (var source in Enum.GetValues<BridgeSource>().Where(s => !ObjectSources.Contains(s)))
        foreach (var subject in Enum.GetValues<ObjectEventSubject>())
        foreach (var ownScopeOpen in new[] { false, true })
        {
            var viaObject = Observe(ownScopeOpen, (core, session) => core.OnObjectEvent(session, source, subject));
            var viaSource = Observe(ownScopeOpen, (core, session) => core.OnSourceEvent(session, source));

            Assert.True(viaSource == viaObject, source + " / " + subject + " / own scope " + ownScopeOpen);
        }

        // The ones that carry a real effect, so that the comparison above is not about nothing.
        Assert.Equal("InvalidateNavigationCaches,MarkDraftsPossiblyStale", Observe(false, (core, s) => core.OnObjectEvent(s, BridgeSource.CommandEnded, Bookkeeping)).Drained);
        Assert.Equal("ReadImplicitSelection,RecomputeSelectionContext", Observe(false, (core, s) => core.OnObjectEvent(s, BridgeSource.ImpliedSelectionChanged, Bookkeeping)).Drained);
        Assert.True(Observe(false, (core, s) => core.OnObjectEvent(s, BridgeSource.CommandWillStart, Bookkeeping)).Answer);
    }

    [Fact]
    public void EveryObjectEventAlsoTheBookkeepingOneStillForgetsTheCloseSequence()
    {
        // Control: an allowed close followed by nothing is intact when the document is destroyed.
        {
            var rig = new WorkspaceBridgeRig();
            rig.Core.Start();
            var id = rig.Open(5);
            var advisor = new AllowAdvisor();
            rig.Core.SetLifecycleAdvisor(advisor);
            Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(id));
            rig.Core.OnDocumentToBeDestroyed(5);
            Assert.True(advisor.Destroyed);
            Assert.True(advisor.LastIntact);
        }

        foreach (var source in ObjectSources)
        foreach (var subject in Enum.GetValues<ObjectEventSubject>())
        {
            var rig = new WorkspaceBridgeRig();
            rig.Core.Start();
            var id = rig.Open(5);
            var advisor = new AllowAdvisor();
            rig.Core.SetLifecycleAdvisor(advisor);
            Assert.Equal(CloseDecision.Allow, rig.Core.OnBeginDocumentClose(id));

            rig.Core.OnObjectEvent(id, source, subject);
            rig.Core.OnDocumentToBeDestroyed(5);

            Assert.True(advisor.Destroyed);
            Assert.False(advisor.LastIntact, source + " / " + subject);
        }
    }

    [Theory]
    [InlineData(BridgeSource.ObjectAppended)]
    [InlineData(BridgeSource.ObjectErased)]
    [InlineData(BridgeSource.ObjectModified)]
    [InlineData(BridgeSource.ObjectUnappended)]
    [InlineData(BridgeSource.ObjectReappended)]
    public void TheBookkeepingNeitherOpensNorClosesTheOwnScopeNorChangesTheStateOfRestNorTheCountOfForeignCommands(BridgeSource source)
    {
        foreach (var ownScopeOpen in new[] { false, true })
        {
            var (rig, session) = Started();
            if (ownScopeOpen) rig.Core.OpenOwnMutation(session);
            rig.Core.NoteForeignCommandStart(session);
            var quiescentBefore = StateValue<bool>(rig.Core, session, "Quiescent");

            rig.Core.OnObjectEvent(session, source, Bookkeeping);

            Assert.Equal(ownScopeOpen, OwnOpen(rig.Core, session));
            Assert.Equal(quiescentBefore, StateValue<bool>(rig.Core, session, "Quiescent"));
            Assert.Equal(1, rig.Core.ForeignCommandStartsOf(session));
            Assert.False(rig.Core.IsCommandActiveInActiveSession);
        }

        // At a state of rest that is not rest, the same: a command that runs keeps running.
        var (running, runningSession) = Started();
        running.Core.OnSourceEvent(runningSession, BridgeSource.CommandWillStart);
        Assert.True(running.Core.IsCommandActiveInActiveSession);
        running.Core.OnObjectEvent(runningSession, source, Bookkeeping);
        Assert.True(running.Core.IsCommandActiveInActiveSession);
        Assert.False(StateValue<bool>(running.Core, runningSession, "Quiescent"));
    }

    [Fact]
    public void ASessionThatIsUnknownDegradedOrDestroyedGivesNoEffectToAnObjectEventOfEitherSubject()
    {
        foreach (var subject in Enum.GetValues<ObjectEventSubject>())
        foreach (var source in ObjectSources)
        {
            var (rig, session) = Started();
            var other = rig.Open(2);

            // Unknown.
            Assert.False(rig.Core.OnObjectEvent(new SessionId(9999), source, subject));
            Assert.Equal(0, rig.SessionOf(session).PendingHintCount);
            Assert.Equal(0, rig.SessionOf(other).PendingHintCount);

            // Degraded: the fault is the one that was reported, and the event adds nothing.
            rig.Core.ReportFault(other, "test", new InvalidOperationException("fault"));
            var faults = rig.Faults.Count;
            Assert.False(rig.Core.OnObjectEvent(other, source, subject));
            Assert.Equal(0, rig.SessionOf(other).PendingHintCount);
            Assert.Equal(faults, rig.Faults.Count);

            // Destroyed.
            rig.Core.OnDocumentToBeDestroyed(1);
            Assert.False(rig.Core.OnObjectEvent(session, source, subject));
            Assert.Equal(faults, rig.Faults.Count);
            Assert.Empty(rig.Core.OnIdle());
        }

        // A core that was never started has no effect either, and does not throw.
        var cold = new WorkspaceBridgeRig();
        Assert.Null(Record.Exception(() => Assert.False(cold.Core.OnObjectEvent(new SessionId(1), BridgeSource.ObjectModified, Bookkeeping))));
        Assert.Equal(0, cold.Source.AttachCalls);
    }

    [Fact]
    public void AnUndoRightAfterTheUpdateIsForeignAndMarksTheDraftsAlsoWithTheBookkeepingAroundIt()
    {
        var (rig, session) = Started();
        rig.Core.OpenOwnMutation(session);
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, ObjectEventSubject.Drawing);
        rig.Core.OnSourceEvent(session, BridgeSource.CommandEnded);
        Assert.DoesNotContain(DrainOperation.MarkDraftsPossiblyStale, Drain(rig.Core));

        // The next command, U, is not the update: its own writes and its end are foreign, whatever the bookkeeping does around them.
        rig.Core.OnSourceEvent(session, BridgeSource.CommandWillStart);
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, Bookkeeping);
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, ObjectEventSubject.Drawing);
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, Bookkeeping);
        rig.Core.OnSourceEvent(session, BridgeSource.CommandEnded);
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, Bookkeeping);
        rig.Core.OnSourceEvent(session, BridgeSource.EnteringQuiescentState);

        Assert.Contains(DrainOperation.MarkDraftsPossiblyStale, Drain(rig.Core));

        // An undo that only ends: the end of a command alone still marks, because every command marks (IC-08).
        rig.Core.OnSourceEvent(session, BridgeSource.CommandWillStart);
        rig.Core.OnObjectEvent(session, BridgeSource.ObjectModified, Bookkeeping);
        rig.Core.OnSourceEvent(session, BridgeSource.CommandEnded);
        rig.Core.OnSourceEvent(session, BridgeSource.EnteringQuiescentState);
        Assert.Contains(DrainOperation.MarkDraftsPossiblyStale, Drain(rig.Core));
    }
}
