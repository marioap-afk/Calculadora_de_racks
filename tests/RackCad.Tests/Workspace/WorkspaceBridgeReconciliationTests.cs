#nullable enable
using System;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T3A-HOST-CORE: INV-F1-T3A-19 and 21 (1) to (4): the two additive members of the pure bridge core.
public class WorkspaceBridgeReconciliationTests
{
    private static (WorkspaceBridgeRig Rig, SessionId A) Synced(long nativeId = 100)
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(nativeId);
        return (rig, a);
    }

    // ------------------------------------------------------------------ (1) an active, synchronized session

    [Fact]
    public void ItQueuesExactlyTheReconciliationHintOnTheActiveSession()
    {
        var (rig, a) = Synced();
        Assert.Equal(0, rig.SessionOf(a).PendingHintCount);

        rig.Core.RequestReconciliation();

        Assert.Equal(1, rig.SessionOf(a).PendingHintCount);
        var drained = rig.SessionOf(a).TryDrain(new DrainConditions(true, false, true));
        Assert.True(drained.Drained);
        Assert.Equal(
            new[] { DrainOperation.ReadImplicitSelection, DrainOperation.RecomputeSelectionContext },
            drained.Operations.ToArray());
    }

    [Fact]
    public void ItLeavesTheIdleSourceAttachedByThePolicyExactlyOnce()
    {
        var (rig, _) = Synced();
        Assert.False(rig.Core.IsIdleAttached);

        rig.Core.RequestReconciliation();

        Assert.True(rig.Core.IsIdleAttached);
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));
        Assert.Empty(rig.Source.Violations);
    }

    [Fact]
    public void ItNeverDrainsSynchronouslyAndNeverAppliesAContext()
    {
        var (rig, a) = Synced();

        rig.Core.RequestReconciliation();

        Assert.Equal(1, rig.SessionOf(a).PendingHintCount);
        Assert.Equal(SelectionContext.None, rig.SessionOf(a).Selection);
        Assert.True(rig.Core.IsIdleAttached);
    }

    [Fact]
    public void TheDrainHappensOnlyAtTheNextIdleWithTheNormalConditions()
    {
        var (rig, a) = Synced();
        rig.Core.RequestReconciliation();

        var outcomes = rig.Core.OnIdle();

        var outcome = Assert.Single(outcomes);
        Assert.Equal(a, outcome.Session);
        Assert.Contains(DrainOperation.ReadImplicitSelection, outcome.Result.Operations);
        Assert.False(rig.Core.IsIdleAttached);
        Assert.Equal(0, rig.SessionOf(a).PendingHintCount);
    }

    [Fact]
    public void ManyRequestsCoalesceIntoOnePendingHintAndOneIdleAttachment()
    {
        var (rig, a) = Synced();

        for (var i = 0; i < 100; i++) rig.Core.RequestReconciliation();

        Assert.Equal(1, rig.SessionOf(a).PendingHintCount);
        Assert.Equal(1, rig.Source.Count(BridgeSource.Idle));
        Assert.Empty(rig.Source.Violations);
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void ItActsOnlyOnTheActiveSessionAndLeavesTheOthersAlone()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200);

        rig.Core.RequestReconciliation();

        Assert.Equal(0, rig.SessionOf(a).PendingHintCount);
        Assert.Equal(1, rig.SessionOf(b).PendingHintCount);
    }

    [Fact]
    public void ItChangesNoAttachmentOtherThanTheIdleOne()
    {
        var (rig, _) = Synced();
        var before = rig.Source.AttachCalls;
        var detaches = rig.Source.DetachCalls;
        var sessions = rig.Core.LiveSessionCount;

        rig.Core.RequestReconciliation();

        Assert.Equal(before + 1, rig.Source.AttachCalls);
        Assert.Equal(detaches, rig.Source.DetachCalls);
        Assert.Equal(sessions, rig.Core.LiveSessionCount);
        Assert.Equal(WorkspaceBridgeRig.ExpectedHookups(1, idleAttached: true), rig.Source.Total);
    }

    [Fact]
    public void StartAndActivationDoNotQueueAReconciliationOnTheirOwn()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        rig.Core.OnDocumentActivated(100, true);

        Assert.Equal(0, rig.SessionOf(a).PendingHintCount);
        Assert.False(rig.Core.IsIdleAttached);
    }

    [Fact]
    public void AHiddenPanelKeepsTheReconciliationPendingUntilItIsShown()
    {
        var (rig, a) = Synced();
        rig.Core.SetPanelVisible(false);
        rig.Core.RequestReconciliation();

        Assert.Empty(rig.Core.OnIdle());
        Assert.Equal(1, rig.SessionOf(a).PendingHintCount);
        Assert.True(rig.Core.IsIdleAttached);

        rig.Core.SetPanelVisible(true);
        Assert.Single(rig.Core.OnIdle());
    }

    // ------------------------------------------------------------------ (2) no active session

    [Fact]
    public void WithoutAnActiveSessionItIsANoOpWithoutAnExceptionAndWithoutIdle()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        Assert.Null(rig.Core.ActiveSession);
        var attach = rig.Source.AttachCalls;

        var exception = Record.Exception(() => rig.Core.RequestReconciliation());

        Assert.Null(exception);
        Assert.False(rig.Core.IsIdleAttached);
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
        Assert.Equal(attach, rig.Source.AttachCalls);
        Assert.Equal(0, rig.Core.LiveSessionCount);
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void BeforeTheCoreIsStartedItIsANoOpThatAttachesNothing()
    {
        var rig = new WorkspaceBridgeRig();

        var exception = Record.Exception(() => rig.Core.RequestReconciliation());

        Assert.Null(exception);
        Assert.Equal(0, rig.Source.AttachCalls);
        Assert.False(rig.Core.IsStarted);
        Assert.Empty(rig.Faults);
    }

    [Fact]
    public void AfterTheCoreIsStoppedItIsANoOp()
    {
        var (rig, _) = Synced();
        rig.Core.Stop();
        var attach = rig.Source.AttachCalls;

        var exception = Record.Exception(() => rig.Core.RequestReconciliation());

        Assert.Null(exception);
        Assert.Equal(attach, rig.Source.AttachCalls);
        Assert.Equal(0, rig.Source.Total);
        Assert.False(rig.Core.IsIdleAttached);
    }

    // ------------------------------------------------------------------ (3) degraded or retired session

    [Fact]
    public void ARetiredSessionIsNotResurrectedAndNothingIsSubscribed()
    {
        var (rig, a) = Synced();
        rig.Core.OnDocumentToBeDestroyed(100);
        var attach = rig.Source.AttachCalls;
        var total = rig.Source.Total;

        var exception = Record.Exception(() => rig.Core.RequestReconciliation());

        Assert.Null(exception);
        Assert.Equal(0, rig.Core.LiveSessionCount);
        Assert.False(rig.Core.TryGetSession(a, out _));
        Assert.Null(rig.Core.ActiveSession);
        Assert.Equal(attach, rig.Source.AttachCalls);
        Assert.Equal(total, rig.Source.Total);
        Assert.False(rig.Core.IsIdleAttached);
    }

    [Fact]
    public void ADegradedSessionIsNotResurrectedAndGetsNoNewSynchronizationSubscription()
    {
        var (rig, a) = Synced();
        rig.Core.ReportFault(a, "simulated fault", new InvalidOperationException("boom"));
        Assert.True(rig.Core.IsDegraded(a));
        var attach = rig.Source.AttachCalls;
        var total = rig.Source.Total;
        var pending = rig.SessionOf(a).PendingHintCount;

        var exception = Record.Exception(() => rig.Core.RequestReconciliation());

        Assert.Null(exception);
        Assert.True(rig.Core.IsDegraded(a));
        Assert.Equal(attach, rig.Source.AttachCalls);
        Assert.Equal(total, rig.Source.Total);
        Assert.Equal(pending, rig.SessionOf(a).PendingHintCount);
        Assert.False(rig.Core.IsIdleAttached);
        Assert.Equal(0, rig.Source.Count(BridgeSource.Idle));
        Assert.Equal(1, rig.Source.CountFor(a, BridgeSource.BeginDocumentClose));
        Assert.Equal(0, rig.Source.CountFor(a, BridgeSource.ImpliedSelectionChanged));
    }

    [Fact]
    public void ADegradedSessionStaysDegradedAndTheOtherSessionsStillReconcile()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200);
        rig.Core.ReportFault(b, "simulated fault", new InvalidOperationException("boom"));

        rig.Core.RequestReconciliation();

        Assert.Equal(0, rig.SessionOf(b).PendingHintCount);
        Assert.Equal(0, rig.SessionOf(a).PendingHintCount);
        Assert.False(rig.Core.IsIdleAttached);
    }

    [Fact]
    public void AFailureToAttachTheIdleSourceIsContainedAndDegradesOnlyTheActiveSession()
    {
        var (rig, a) = Synced();
        rig.Source.FailAttach = attachment => attachment.Source == BridgeSource.Idle;

        var exception = Record.Exception(() => rig.Core.RequestReconciliation());

        Assert.Null(exception);
        Assert.True(rig.Core.IsDegraded(a));
        Assert.NotEmpty(rig.Faults);
        Assert.False(rig.Core.IsIdleAttached);
    }

    [Fact]
    public void ItNeverWritesAnythingOnTheSessionBeyondTheHint()
    {
        var (rig, a) = Synced();
        var session = rig.SessionOf(a);

        rig.Core.RequestReconciliation();

        Assert.Same(session, rig.SessionOf(a));
        Assert.False(session.IsDestroyed);
        Assert.Equal(SelectionContext.None, session.Selection);
        Assert.Equal(100L, session.NativeInstanceId);
    }
}

// I-64 F1-T3A-HOST-CORE: INV-F1-T3A-19 and 21 (4): the Authorize member of the pure bridge core.
public class WorkspaceBridgeAuthorizeTests
{
    private static (WorkspaceBridgeRig Rig, SessionId A, SessionId B) TwoDocuments()
    {
        var rig = new WorkspaceBridgeRig();
        rig.Core.Start();
        var a = rig.Open(100);
        var b = rig.Open(200);
        return (rig, a, b);
    }

    [Fact]
    public void TheCorrectSessionOfTheExecutingDocumentIsAccepted()
    {
        var (rig, a, b) = TwoDocuments();

        Assert.Equal(RequestDecision.Accepted, rig.Core.Authorize(new WorkspaceRequest(a), 100));
        Assert.Equal(RequestDecision.Accepted, rig.Core.Authorize(new WorkspaceRequest(b), 200));
    }

    [Fact]
    public void AWrongExecutingDocumentIsRejectedAsAMismatch()
    {
        var (rig, a, b) = TwoDocuments();

        Assert.Equal(RequestDecision.RejectedSessionMismatch, rig.Core.Authorize(new WorkspaceRequest(a), 200));
        Assert.Equal(RequestDecision.RejectedSessionMismatch, rig.Core.Authorize(new WorkspaceRequest(b), 100));
        Assert.Equal(RequestDecision.RejectedSessionMismatch, rig.Core.Authorize(new WorkspaceRequest(a), 999));
    }

    [Fact]
    public void ADestroyedOriginIsRejectedAsDestroyed()
    {
        var (rig, a, _) = TwoDocuments();
        rig.Core.OnDocumentToBeDestroyed(100);

        Assert.Equal(RequestDecision.RejectedSessionDestroyed, rig.Core.Authorize(new WorkspaceRequest(a), 100));
        Assert.Equal(RequestDecision.RejectedSessionDestroyed, rig.Core.Authorize(new WorkspaceRequest(a), 200));
    }

    [Fact]
    public void ARecycledNativeIdentityDoesNotAuthorizeTheOldSession()
    {
        var (rig, a, _) = TwoDocuments();
        rig.Core.OnDocumentToBeDestroyed(100);
        var fresh = rig.Open(100);

        Assert.Equal(RequestDecision.RejectedSessionDestroyed, rig.Core.Authorize(new WorkspaceRequest(a), 100));
        Assert.Equal(RequestDecision.Accepted, rig.Core.Authorize(new WorkspaceRequest(fresh), 100));
    }

    [Fact]
    public void TheVerdictIsIdenticalToTheOneOfTheRegistryOverTheWholeMatrix()
    {
        var (rig, _, _) = TwoDocuments();
        rig.Core.OnDocumentToBeDestroyed(100);
        rig.Open(300);

        // An independent registry that went through the same life: ids are issued monotonically, so they match.
        var registry = new WorkspaceSessionRegistry();
        var first = registry.GetOrCreate(100);
        registry.GetOrCreate(200);
        registry.Destroy(first.Id);
        registry.GetOrCreate(300);

        foreach (var origin in Enumerable.Range(0, 6).Select(i => new SessionId(i)))
        {
            foreach (var native in new long[] { 0, 100, 200, 300, 400 })
            {
                var request = new WorkspaceRequest(origin);
                Assert.Equal(registry.Authorize(request, native), rig.Core.Authorize(request, native));
            }
        }
    }

    [Fact]
    public void AuthorizingHasNoEffectOnSessionsAttachmentsOrHints()
    {
        var (rig, a, b) = TwoDocuments();
        var attach = rig.Source.AttachCalls;
        var detach = rig.Source.DetachCalls;
        var live = rig.Core.LiveSessionCount;

        for (var i = 0; i < 100; i++)
        {
            rig.Core.Authorize(new WorkspaceRequest(a), 200);
            rig.Core.Authorize(new WorkspaceRequest(b), 200);
            rig.Core.Authorize(new WorkspaceRequest(new SessionId(77)), 1);
        }

        Assert.Equal(attach, rig.Source.AttachCalls);
        Assert.Equal(detach, rig.Source.DetachCalls);
        Assert.Equal(live, rig.Core.LiveSessionCount);
        Assert.Equal(0, rig.SessionOf(a).PendingHintCount);
        Assert.Equal(0, rig.SessionOf(b).PendingHintCount);
        Assert.Equal(b, rig.Core.ActiveSession);
        Assert.False(rig.Core.IsIdleAttached);
    }

    [Fact]
    public void BeforeAnySessionExistsEveryRequestIsRejected()
    {
        var rig = new WorkspaceBridgeRig();

        Assert.NotEqual(RequestDecision.Accepted, rig.Core.Authorize(new WorkspaceRequest(new SessionId(1)), 100));
    }
}
