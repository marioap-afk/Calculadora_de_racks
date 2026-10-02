using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T1-MODEL: D-03 / INV-02, INV-07, INV-09, INV-22.
public class WorkspaceSessionTests
{
    [Fact]
    public void SameLiveDocumentInstanceYieldsTheSameSession()
    {
        var registry = new WorkspaceSessionRegistry();
        var first = registry.GetOrCreate(100);
        var again = registry.GetOrCreate(100);
        Assert.Same(first, again);
        Assert.Equal(1, registry.LiveSessionCount);
    }

    [Fact]
    public void TwoOpenInstancesGiveTwoIndependentSessions()
    {
        var registry = new WorkspaceSessionRegistry();
        var a = registry.GetOrCreate(1);
        var b = registry.GetOrCreate(2);
        Assert.NotEqual(a.Id, b.Id);
        Assert.Equal(2, registry.LiveSessionCount);
        Assert.True(a.ApplyContext(SelectionContext.One("R1")));
        Assert.Equal(SelectionContext.None, b.Selection);
    }

    [Fact]
    public void DestroyedSessionIsRetiredAndRecycledNativeIdGetsAFreshEmptySession()
    {
        var registry = new WorkspaceSessionRegistry();
        var old = registry.GetOrCreate(7);
        old.ApplyContext(SelectionContext.One("R1"));
        old.EnqueueHint(HintKind.ImpliedSelectionChanged);

        registry.Destroy(old.Id);

        Assert.True(old.IsDestroyed);
        Assert.Equal(0, registry.LiveSessionCount);
        Assert.False(registry.TryGet(old.Id, out _));

        var reborn = registry.GetOrCreate(7);
        Assert.NotEqual(old.Id, reborn.Id);
        Assert.NotSame(old, reborn);
        Assert.False(reborn.IsDestroyed);
        Assert.Equal(SelectionContext.None, reborn.Selection);
        Assert.Equal(0, reborn.PendingHintCount);
        Assert.True(old.IsDestroyed);
    }

    [Fact]
    public void DestroyedSessionDoesNotAcceptHintsOrContext()
    {
        var registry = new WorkspaceSessionRegistry();
        var s = registry.GetOrCreate(3);
        registry.Destroy(s.Id);
        Assert.True(s.IsDestroyed);
        Assert.False(s.EnqueueHint(HintKind.CommandEnded));
        Assert.False(s.ApplyContext(SelectionContext.One("R1")));
        Assert.Equal(0, s.PendingHintCount);
    }

    [Fact]
    public void RequestFromTheSessionOfTheExecutingDocumentIsAccepted()
    {
        var registry = new WorkspaceSessionRegistry();
        var a = registry.GetOrCreate(1);
        Assert.Equal(RequestDecision.Accepted, registry.Authorize(new WorkspaceRequest(a.Id), 1));
        Assert.Equal(RequestDecision.RejectedSessionMismatch, registry.Authorize(new WorkspaceRequest(a.Id), 99));
    }

    [Fact]
    public void RequestBornInAExecutedWithBActiveIsRejectedWithoutEffects()
    {
        var registry = new WorkspaceSessionRegistry();
        var a = registry.GetOrCreate(1);
        var b = registry.GetOrCreate(2);
        a.ApplyContext(SelectionContext.One("RA"));
        a.EnqueueHint(HintKind.DatabaseObjectChanged);
        b.EnqueueHint(HintKind.CommandEnded);

        var decision = registry.Authorize(new WorkspaceRequest(a.Id), 2);

        Assert.Equal(RequestDecision.RejectedSessionMismatch, decision);
        Assert.Equal(2, registry.LiveSessionCount);
        Assert.Equal(SelectionContext.One("RA"), a.Selection);
        Assert.Equal(SelectionContext.None, b.Selection);
        Assert.Equal(1, a.PendingHintCount);
        Assert.Equal(1, b.PendingHintCount);
    }

    [Fact]
    public void RequestOfADestroyedSessionIsRejectedEvenIfTheNativeIdIsRecycled()
    {
        var registry = new WorkspaceSessionRegistry();
        var old = registry.GetOrCreate(9);
        registry.Destroy(old.Id);
        registry.GetOrCreate(9);

        Assert.NotEqual(RequestDecision.Accepted, registry.Authorize(new WorkspaceRequest(old.Id), 9));
    }

    [Fact]
    public void RackKeysNeverCrossSessions()
    {
        var registry = new WorkspaceSessionRegistry();
        var a = registry.GetOrCreate(1);
        var b = registry.GetOrCreate(2);
        var keyA = new RackKey(a.Id, "same-guid");
        var keyB = new RackKey(b.Id, "same-guid");
        Assert.NotEqual(keyA, keyB);
        Assert.NotEqual(a.Id, b.Id);
    }
}
