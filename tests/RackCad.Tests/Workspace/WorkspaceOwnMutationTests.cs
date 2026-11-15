#nullable enable
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T4C1-HOST (D-T4C-09, IC-09; INV-F4T4C-09, D-T4C-20 rows 16 and 17): the own mutation of the update, in the pure core of the bridge
// and in the session, over the simulated source. The hints that arrive while it is open are kept apart and only invalidate; what was pending
// from other sources keeps its whole effect; and the scope closes with the first end or start of a command, the entry into rest, the
// explicit close, the degradation and the destruction. Nothing reads or writes a drawing.
public class WorkspaceOwnMutationTests
{
    private static (WorkspaceBridgeRig Rig, SessionId Session) Started()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var session = rig.Open(1);
        return (rig, session);
    }

    // The flag of the scope is part of the private state of the session in the core: it has no public reader on purpose (the surface of the
    // core is a closed list), so the tests read it by reflection.
    private static bool OwnOpen(EventBridgeCore core, SessionId session)
    {
        var states = (IDictionary)typeof(EventBridgeCore).GetField("_states", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(core)!;
        var state = states[session]!;
        return (bool)state.GetType().GetProperty("OwnMutationOpen", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(state)!;
    }

    private static DrainOperation[] Drain(EventBridgeCore core)
    {
        var outcomes = core.OnIdle();
        var outcome = Assert.Single(outcomes);
        Assert.True(outcome.Result.Drained);
        return outcome.Result.Operations.ToArray();
    }

    // ------------------------------------------------------------------ what is own only invalidates (INV-28, test 17)

    [Fact]
    public void TheHintsOfTheOwnMutationOnlyInvalidateAndNeverMarkTheDrafts()
    {
        var (rig, session) = Started();
        rig.Core.OpenOwnMutation(session);

        rig.Core.OnSourceEvent(session, BridgeSource.ObjectAppended);
        rig.Core.OnSourceEvent(session, BridgeSource.ObjectModified);
        rig.Core.OnSourceEvent(session, BridgeSource.CommandEnded);

        var operations = Drain(rig.Core);

        Assert.Equal(new[] { DrainOperation.InvalidateNavigationCaches }, operations);
        Assert.DoesNotContain(DrainOperation.MarkDraftsPossiblyStale, operations);
    }

    [Fact]
    public void AForeignHintThatWasAlreadyPendingKeepsItsWholeEffectAlsoWhenTheOwnMutationOpensAfterIt()
    {
        var (rig, session) = Started();
        rig.Core.OnSourceEvent(session, BridgeSource.ObjectModified);
        rig.Core.OpenOwnMutation(session);
        rig.Core.OnSourceEvent(session, BridgeSource.ObjectAppended);
        rig.Core.OnSourceEvent(session, BridgeSource.CommandEnded);

        var operations = Drain(rig.Core);

        Assert.Contains(DrainOperation.InvalidateNavigationCaches, operations);
        Assert.Contains(DrainOperation.MarkDraftsPossiblyStale, operations);
    }

    [Fact]
    public void AnImpliedSelectionThatArrivesWhileTheScopeIsOpenStaysForeignAndNeverMarksTheDrafts()
    {
        var (rig, session) = Started();
        rig.Core.OpenOwnMutation(session);
        rig.Core.OnSourceEvent(session, BridgeSource.ImpliedSelectionChanged);
        rig.Core.OnSourceEvent(session, BridgeSource.ObjectModified);

        var operations = Drain(rig.Core);

        Assert.Contains(DrainOperation.ReadImplicitSelection, operations);
        Assert.Contains(DrainOperation.RecomputeSelectionContext, operations);
        Assert.Contains(DrainOperation.InvalidateNavigationCaches, operations);
        Assert.DoesNotContain(DrainOperation.MarkDraftsPossiblyStale, operations);

        // And alone, with no own hint at all, it neither invalidates nor marks.
        rig.Core.OnSourceEvent(session, BridgeSource.ImpliedSelectionChanged);
        var alone = Drain(rig.Core);
        Assert.DoesNotContain(DrainOperation.InvalidateNavigationCaches, alone);
        Assert.DoesNotContain(DrainOperation.MarkDraftsPossiblyStale, alone);
    }

    [Fact]
    public void TheIdleSourceHooksWhileOnlyOwnHintsArePendingBecauseTheCountOfPendingHintsCountsBothSets()
    {
        var (rig, session) = Started();
        Assert.False(rig.Core.IsIdleAttached);
        rig.Core.OpenOwnMutation(session);

        rig.Core.OnSourceEvent(session, BridgeSource.ObjectErased);

        Assert.True(rig.Core.IsIdleAttached);
        Assert.Equal(1, rig.SessionOf(session).PendingHintCount);
        // Many own events coalesce by kind, with constant cost.
        for (var i = 0; i < 50; i++) rig.Core.OnSourceEvent(session, BridgeSource.ObjectModified);
        Assert.Equal(1, rig.SessionOf(session).PendingHintCount);
        Drain(rig.Core);
        Assert.False(rig.Core.IsIdleAttached);
    }

    [Fact]
    public void ADestroyedSessionTakesNoOwnHintAndAnOwnHintCarriesNothingButItsKind()
    {
        var (rig, session) = Started();
        var live = rig.SessionOf(session);

        Assert.True(live.EnqueueOwnHint(HintKind.DatabaseObjectChanged));
        Assert.Equal(1, live.PendingHintCount);
        rig.Core.OnDocumentToBeDestroyed(1);
        Assert.False(live.EnqueueOwnHint(HintKind.CommandEnded));
        Assert.Empty(typeof(WorkspaceSession).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(f => f.Name.Contains("andle", StringComparison.Ordinal)));
    }

    // ------------------------------------------------------------------ the closing (test 17)

    [Fact]
    public void TheFirstEndOfACommandClosesTheScopeAndCountsAsOwnAndALaterOneIsForeignAndMarks()
    {
        var (rig, session) = Started();
        rig.Core.OpenOwnMutation(session);
        Assert.True(OwnOpen(rig.Core, session));

        rig.Core.OnSourceEvent(session, BridgeSource.CommandEnded);
        Assert.False(OwnOpen(rig.Core, session));
        var first = Drain(rig.Core);
        Assert.DoesNotContain(DrainOperation.MarkDraftsPossiblyStale, first);

        // A second end has no effect on the scope, and it is the ordinary hint: it marks.
        rig.Core.OnSourceEvent(session, BridgeSource.CommandEnded);
        Assert.False(OwnOpen(rig.Core, session));
        Assert.Contains(DrainOperation.MarkDraftsPossiblyStale, Drain(rig.Core));
    }

    [Theory]
    [InlineData(BridgeSource.CommandCancelled)]
    [InlineData(BridgeSource.CommandFailed)]
    public void ACancelledOrAFailedCommandClosesTheScopeToo(BridgeSource end)
    {
        var (rig, session) = Started();
        rig.Core.OpenOwnMutation(session);

        rig.Core.OnSourceEvent(session, end);

        Assert.False(OwnOpen(rig.Core, session));
        Assert.DoesNotContain(DrainOperation.MarkDraftsPossiblyStale, Drain(rig.Core));
    }

    [Fact]
    public void TheFirstStartOfACommandClosesTheScopeEvenWhenTheRestDoesNotChangeAndAnEntryIntoRestClosesItToo()
    {
        var (rig, session) = Started();

        // The session is at rest: the start of a command changes the state of the command. Open it again with the command already running, so
        // that a second start does not change it: the scope still closes (the close is before the comparison of the state).
        rig.Core.OnSourceEvent(session, BridgeSource.CommandWillStart);
        rig.Core.OpenOwnMutation(session);
        Assert.True(OwnOpen(rig.Core, session));
        var changed = rig.Core.OnSourceEvent(session, BridgeSource.CommandWillStart);
        Assert.False(changed);
        Assert.False(OwnOpen(rig.Core, session));

        rig.Core.OpenOwnMutation(session);
        rig.Core.OnSourceEvent(session, BridgeSource.EnteringQuiescentState);
        Assert.False(OwnOpen(rig.Core, session));
    }

    [Fact]
    public void WithoutTheEndOfTheInnerCommandALaterCommandClosesTheScopeBeforeItsHintsSoThatItMarksTheDrafts()
    {
        var (rig, session) = Started();
        rig.Core.OpenOwnMutation(session);
        rig.Core.OnSourceEvent(session, BridgeSource.ObjectModified);

        // No end of command for the update itself: the next command of the user starts, and its own writes are foreign.
        rig.Core.OnSourceEvent(session, BridgeSource.CommandWillStart);
        rig.Core.OnSourceEvent(session, BridgeSource.ObjectModified);
        rig.Core.OnSourceEvent(session, BridgeSource.EnteringQuiescentState);

        Assert.Contains(DrainOperation.MarkDraftsPossiblyStale, Drain(rig.Core));
    }

    [Fact]
    public void TheExplicitCloseIsIdempotentAndAnImmediateUndoAfterTheUpdateMarks()
    {
        var (rig, session) = Started();
        rig.Core.OpenOwnMutation(session);

        rig.Core.CloseOwnMutation(session);
        rig.Core.CloseOwnMutation(session);
        Assert.False(OwnOpen(rig.Core, session));
        rig.Core.OnSourceEvent(session, BridgeSource.ObjectModified);

        Assert.Contains(DrainOperation.MarkDraftsPossiblyStale, Drain(rig.Core));
    }

    [Fact]
    public void ANewSessionAfterTheDestructionOfTheOldOneStartsWithTheScopeClosed()
    {
        var (rig, session) = Started();
        rig.Core.OpenOwnMutation(session);

        rig.Core.OnDocumentToBeDestroyed(1);
        var next = rig.Open(2);

        Assert.False(OwnOpen(rig.Core, next));
        Assert.Null(rig.Core.ForeignCommandStartsOf(session));
        // Closing or opening a session that is gone does nothing and never throws.
        Assert.Null(Record.Exception(() => rig.Core.OpenOwnMutation(session)));
        Assert.Null(Record.Exception(() => rig.Core.CloseOwnMutation(session)));
    }

    [Fact]
    public void TheDegradationClosesTheScopeAtTheOnePointOfDegradationAndAFailureInsideTheSourceEventIsOneWayToIt()
    {
        var (rig, session) = Started();
        rig.Core.OpenOwnMutation(session);
        Assert.True(OwnOpen(rig.Core, session));

        // The failure comes from inside OnSourceEvent (it asks for the idle source and the source refuses), not from ReportFault: that is
        // the way that tells the one point of degradation (Degrade) from a close that a handler would make by itself.
        rig.Source.FailAttach = a => a.Source == BridgeSource.Idle;
        rig.Core.OnSourceEvent(session, BridgeSource.ObjectModified);

        Assert.True(rig.Core.IsDegraded(session));
        Assert.False(OwnOpen(rig.Core, session));
        Assert.Null(rig.Core.ForeignCommandStartsOf(session));
    }

    [Fact]
    public void AReportedFaultAlsoClosesTheScopeBecauseItGoesThroughTheSamePointOfDegradation()
    {
        var (rig, session) = Started();
        rig.Core.OpenOwnMutation(session);

        rig.Core.ReportFault(session, "test", new InvalidOperationException("fault"));

        Assert.False(OwnOpen(rig.Core, session));
        // A degraded session cannot open it again: its events no longer arrive.
        rig.Core.OpenOwnMutation(session);
        Assert.False(OwnOpen(rig.Core, session));
    }

    // ------------------------------------------------------------------ the count of foreign commands (D-T4C-02, D-T4C-09)

    [Fact]
    public void ForeignCommandStartsAreCountedInMemoryAndTheEntriesIntoRestAndTheOwnStartsAreNot()
    {
        var (rig, session) = Started();
        Assert.Equal(0, rig.Core.ForeignCommandStartsOf(session));

        // The starts and the entries into rest of the events do not count by themselves: only the handler that sees a command other than the update does.
        rig.Core.OnSourceEvent(session, BridgeSource.EnteringQuiescentState);
        rig.Core.OnSourceEvent(session, BridgeSource.EnteringQuiescentState);
        rig.Core.OnSourceEvent(session, BridgeSource.CommandWillStart);
        Assert.Equal(0, rig.Core.ForeignCommandStartsOf(session));

        rig.Core.NoteForeignCommandStart(session);
        rig.Core.NoteForeignCommandStart(session);
        Assert.Equal(2, rig.Core.ForeignCommandStartsOf(session));
        Assert.Null(rig.Core.ForeignCommandStartsOf(new SessionId(999)));
    }

    [Fact]
    public void ASessionThatIsDegradedOrUnknownCountsNothingAndHasNoCount()
    {
        var (rig, session) = Started();
        rig.Core.NoteForeignCommandStart(session);
        rig.Core.ReportFault(session, "test", new InvalidOperationException("fault"));

        Assert.Null(rig.Core.ForeignCommandStartsOf(session));
        Assert.Null(Record.Exception(() => rig.Core.NoteForeignCommandStart(session)));
        Assert.Null(rig.Core.ForeignCommandStartsOf(session));
        Assert.Null(rig.Core.ForeignCommandStartsOf(new SessionId(424242)));
    }

    [Fact]
    public void TheThreeEntriesOfTheOwnMutationAndTheCountForgetTheCloseSequenceAndTheQueryDoesNot()
    {
        // An allowed close, then one of the three entries: the sequence is no longer intact when the document is destroyed.
        foreach (var entry in new Action<EventBridgeCore, SessionId>[]
                 {
                     (core, s) => core.OpenOwnMutation(s),
                     (core, s) => core.CloseOwnMutation(s),
                     (core, s) => core.NoteForeignCommandStart(s),
                 })
        {
            var local = new WorkspaceBridgeRig();
            local.Core.Start();
            var id = local.Open(5);
            var localAdvisor = new CloseAllowAdvisor();
            local.Core.SetLifecycleAdvisor(localAdvisor);
            Assert.Equal(CloseDecision.Allow, local.Core.OnBeginDocumentClose(id));
            entry(local.Core, id);
            local.Core.OnDocumentToBeDestroyed(5);
            Assert.False(localAdvisor.LastIntact);
        }

        var queryRig = new WorkspaceBridgeRig();
        queryRig.Core.Start();
        var queryId = queryRig.Open(6);
        var queryAdvisor = new CloseAllowAdvisor();
        queryRig.Core.SetLifecycleAdvisor(queryAdvisor);
        Assert.Equal(CloseDecision.Allow, queryRig.Core.OnBeginDocumentClose(queryId));
        _ = queryRig.Core.ForeignCommandStartsOf(queryId);
        queryRig.Core.OnDocumentToBeDestroyed(6);
        Assert.True(queryAdvisor.LastIntact);
    }

    private sealed class CloseAllowAdvisor : IWorkspaceLifecycleAdvisor
    {
        public bool LastIntact { get; private set; }

        public CloseDecision AdviseClose(SessionId session) => CloseDecision.Allow;

        public void SessionDestroyed(SessionId session, bool closeSequenceIntact) => LastIntact = closeSequenceIntact;
    }

    // ------------------------------------------------------------------ the host core drains what is own the same way

    [Fact]
    public void TheHostCoreDoesNotMarkTheDraftsOfADrainThatCarriesOnlyOwnHints()
    {
        var host = new WorkspaceHostRig().WithDocument(100);
        host.Host.OpenPanel();
        var session = host.ActiveSession;
        host.Idle();
        host.Bridge.Core.OpenOwnMutation(session);

        host.Bridge.Core.OnSourceEvent(session, BridgeSource.ObjectModified);
        host.Bridge.Core.OnSourceEvent(session, BridgeSource.CommandEnded);
        var drains = host.Idle();

        var drained = Assert.Single(drains);
        Assert.Equal(new[] { DrainOperation.InvalidateNavigationCaches }, drained.Result.Operations.ToArray());
        Assert.Contains(session, host.Reader.Invalidations);
    }
}
