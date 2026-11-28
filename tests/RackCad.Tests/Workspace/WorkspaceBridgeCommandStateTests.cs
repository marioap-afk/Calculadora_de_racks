#nullable enable
using System;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T3-HOST-C2 (C2-A). Smoke-1 R20261103T175351Z-0a77 failed SM-13 and SM-34: with LINE or RACKEDITAR running the
// panel never showed "Hay un comando en curso." because CommandWillStart only flipped the in-memory quiescence of the
// session and nothing told the visible panel to pull a fresh snapshot. The pure core now answers whether a notification
// changed the command state of its session, and knows whether the active session runs a command. That is all it adds:
// CommandWillStart and EnteringQuiescentState still queue no hint, attach no idle source and drain nothing (the T2
// oracle SourcesThatAreNotHintsQueueNothingAndNeverAttachIdle stays as it is).
public class WorkspaceBridgeCommandStateTests
{
    private static (WorkspaceBridgeRig Rig, SessionId A, SessionId B) TwoDocuments()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200); // B is the active document, at rest
        return (rig, a, b);
    }

    // ------------------------------------------------------------------ 1 and 2: the presentation notice

    [Fact]
    public void CommandWillStartReportsAPresentationChangeAndTheActiveSessionRunsACommand()
    {
        var (rig, _, b) = TwoDocuments();
        Assert.False(rig.Core.IsCommandActiveInActiveSession);

        var changed = rig.Core.OnSourceEvent(b, BridgeSource.CommandWillStart);

        Assert.True(changed, "CommandWillStart at rest did not report a change of the command state");
        Assert.True(rig.Core.IsCommandActiveInActiveSession);
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void EnteringQuiescentStateReportsTheComplementaryChange()
    {
        var (rig, _, b) = TwoDocuments();
        Assert.True(rig.Core.OnSourceEvent(b, BridgeSource.CommandWillStart));

        var changed = rig.Core.OnSourceEvent(b, BridgeSource.EnteringQuiescentState);

        Assert.True(changed, "EnteringQuiescentState during a command did not report a change of the command state");
        Assert.False(rig.Core.IsCommandActiveInActiveSession);
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void ANotificationThatChangesNothingReportsNoChange()
    {
        var (rig, _, b) = TwoDocuments();

        Assert.False(rig.Core.OnSourceEvent(b, BridgeSource.EnteringQuiescentState)); // already at rest
        Assert.True(rig.Core.OnSourceEvent(b, BridgeSource.CommandWillStart));
        Assert.False(rig.Core.OnSourceEvent(b, BridgeSource.CommandWillStart)); // a nested or transparent command
        Assert.True(rig.Core.IsCommandActiveInActiveSession);
    }

    [Theory]
    [MemberData(nameof(WorkspaceBridgeHintAndDrainTests.HintSources), MemberType = typeof(WorkspaceBridgeHintAndDrainTests))]
    public void TheHintSourcesKeepTheirT2BehaviourAndReportNoPresentationChange(BridgeSource source)
    {
        var (rig, _, b) = TwoDocuments();

        Assert.False(rig.Core.OnSourceEvent(b, source));

        Assert.Equal(1, rig.SessionOf(b).PendingHintCount);
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));
        Assert.False(rig.Core.IsCommandActiveInActiveSession);
    }

    // ------------------------------------------------------------------ 3, 4 and 5: no hint, no idle, no drain

    [Theory]
    [InlineData(BridgeSource.CommandWillStart)]
    [InlineData(BridgeSource.EnteringQuiescentState)]
    public void TheCommandStateSourcesQueueNoHintAttachNoIdleAndDrainNothingEvenWhenTheyReportAChange(BridgeSource source)
    {
        var (rig, a, b) = TwoDocuments();
        if (source == BridgeSource.EnteringQuiescentState) Assert.True(rig.Core.OnSourceEvent(b, BridgeSource.CommandWillStart));
        var attachCalls = rig.Source.AttachCalls;
        var detachCalls = rig.Source.DetachCalls;
        var hookups = rig.Source.Total;

        Assert.True(rig.Core.OnSourceEvent(b, source));

        Assert.Equal(0, rig.SessionOf(a).PendingHintCount);
        Assert.Equal(0, rig.SessionOf(b).PendingHintCount);
        Assert.Equal(attachCalls, rig.Source.AttachCalls);
        Assert.Equal(detachCalls, rig.Source.DetachCalls);
        Assert.Equal(hookups, rig.Source.Total);
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
        Assert.False(rig.Core.IsIdleAttached);
        Assert.Empty(rig.Core.OnIdle());
        Assert.Equal(SelectionContext.None, rig.SessionOf(b).Selection);
        Assert.Empty(rig.Source.Violations);
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void ACommandStateChangeKeepsTheHintsAlreadyQueuedForTheRestAndDrainsThemOnlyAtRest()
    {
        var (rig, _, b) = TwoDocuments();
        rig.Core.OnSourceEvent(b, BridgeSource.ImpliedSelectionChanged);

        Assert.True(rig.Core.OnSourceEvent(b, BridgeSource.CommandWillStart));
        Assert.Equal(1, rig.SessionOf(b).PendingHintCount);
        Assert.Empty(rig.Core.OnIdle());

        Assert.True(rig.Core.OnSourceEvent(b, BridgeSource.EnteringQuiescentState));
        Assert.Equal(1, rig.SessionOf(b).PendingHintCount); // the change itself drained nothing
        Assert.Single(rig.Core.OnIdle());
    }

    // ------------------------------------------------------------------ the in-memory command state of the active session

    [Fact]
    public void OnlyTheActiveSessionCountsAsRunningACommand()
    {
        var (rig, a, b) = TwoDocuments();

        Assert.True(rig.Core.OnSourceEvent(a, BridgeSource.CommandWillStart)); // A is not the active document
        Assert.False(rig.Core.IsCommandActiveInActiveSession);

        Assert.True(rig.Core.OnSourceEvent(b, BridgeSource.CommandWillStart));
        Assert.True(rig.Core.IsCommandActiveInActiveSession);
    }

    [Fact]
    public void ADocumentActivatedWhileNotAtRestRunsACommandUntilItComesBackToRest()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100, quiescent: false);

        Assert.True(rig.Core.IsCommandActiveInActiveSession);
        Assert.False(rig.Core.OnSourceEvent(a, BridgeSource.CommandWillStart));
        Assert.True(rig.Core.OnSourceEvent(a, BridgeSource.EnteringQuiescentState));
        Assert.False(rig.Core.IsCommandActiveInActiveSession);
    }

    [Fact]
    public void ADegradedActiveSessionIsNeverReportedAsRunningACommandAndReportsNoChange()
    {
        var (rig, _, b) = TwoDocuments();
        Assert.True(rig.Core.OnSourceEvent(b, BridgeSource.CommandWillStart));

        rig.Core.ReportFault(b, "test", new InvalidOperationException("simulated"));

        // Its synchronization sources are detached: what it last knew is stale, so the core claims nothing.
        Assert.False(rig.Core.IsCommandActiveInActiveSession);
        Assert.False(rig.Core.OnSourceEvent(b, BridgeSource.EnteringQuiescentState));
    }

    [Fact]
    public void BeforeStartAndForARetiredSessionNothingIsReportedAndNothingRuns()
    {
        var rig = new WorkspaceBridgeRig();
        Assert.False(rig.Core.OnSourceEvent(default, BridgeSource.CommandWillStart));
        Assert.False(rig.Core.IsCommandActiveInActiveSession);

        rig.Core.Start();
        var a = rig.Open(100);
        Assert.True(rig.Core.OnSourceEvent(a, BridgeSource.CommandWillStart));
        rig.Core.OnDocumentToBeDestroyed(100);

        Assert.False(rig.Core.OnSourceEvent(a, BridgeSource.EnteringQuiescentState));
        Assert.False(rig.Core.IsCommandActiveInActiveSession);

        rig.Core.Stop();
        Assert.False(rig.Core.IsCommandActiveInActiveSession);
    }

    // ------------------------------------------------------------------ 6: the host core reads nothing because of it

    [Fact]
    public void ACommandStateChangeAsksTheHostForNoReconciliationAndReadsNothing()
    {
        var rig = new WorkspaceHostRig().WithDocument(100);
        rig.Host.OpenPanel();
        rig.Idle();
        var session = rig.ActiveSession;
        var reconciliations = rig.Link.ReconciliationCalls;
        var calls = rig.Reader.Calls.Count;
        var drains = rig.Drains.Count;

        rig.Probe.IsActiveDocumentQuiescent = false;
        Assert.True(rig.Bridge.Core.OnSourceEvent(session, BridgeSource.CommandWillStart));
        var during = rig.Host.GetSnapshot();
        rig.Idle();

        Assert.False(during.Actions.CanReadDrawing);
        Assert.False(during.Actions.CanWriteDrawing);
        Assert.Equal(WorkspaceActionBlock.CommandActive, during.Actions.Block);

        Assert.True(rig.Bridge.Core.OnSourceEvent(session, BridgeSource.EnteringQuiescentState));
        rig.Probe.IsActiveDocumentQuiescent = true;
        var after = rig.Host.GetSnapshot();
        rig.Idle();

        Assert.True(after.Actions.CanReadDrawing);
        Assert.Equal(WorkspaceActionBlock.None, after.Actions.Block);
        Assert.Equal(reconciliations, rig.Link.ReconciliationCalls);
        Assert.Equal(calls, rig.Reader.Calls.Count);
        Assert.Equal(drains, rig.Drains.Count);
        Assert.Empty(rig.Faults);
    }
}
