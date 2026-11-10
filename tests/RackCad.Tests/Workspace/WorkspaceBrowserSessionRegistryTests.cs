#nullable enable
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F3-T1-BROWSER-CORE: one browser session per SessionId, in memory only, dead for good once destroyed (INV-F3T1-12, 13).
public class WorkspaceBrowserSessionRegistryTests
{
    private static string Shape(BrowserSession s) => WorkspaceBrowserTestSupport.Shape(s);

    private static SelectionContext One(string id) => SelectionContext.One(id);

    private static BrowserSession Create(BrowserSessionRegistry registry, long id)
    {
        var session = registry.GetOrCreate(new SessionId(id));
        Assert.NotNull(session);
        return session!;
    }

    [Fact]
    public void ANewRegistryHoldsNoSession()
    {
        var registry = new BrowserSessionRegistry();

        Assert.Equal(0, registry.LiveCount);
        Assert.Empty(registry.KnownSessions);
        Assert.False(registry.TryGet(new SessionId(1), out var found));
        Assert.Null(found);
    }

    [Fact]
    public void GetOrCreateReturnsTheSameInstanceWhileTheSessionLivesAndCarriesItsId()
    {
        var registry = new BrowserSessionRegistry();

        var first = Create(registry, 7);
        var again = Create(registry, 7);

        Assert.Same(first, again);
        Assert.Equal(new SessionId(7), first.Id);
        Assert.Equal(1, registry.LiveCount);
        Assert.True(registry.TryGet(new SessionId(7), out var found));
        Assert.Same(first, found);
    }

    [Fact]
    public void EachSessionIdGetsItsOwnInstanceAndTheRegistryEnumeratesTheLiveOnesInCreationOrder()
    {
        var registry = new BrowserSessionRegistry();

        var a = Create(registry, 2);
        var b = Create(registry, 1);
        var c = Create(registry, 3);

        Assert.NotSame(a, b);
        Assert.NotSame(b, c);
        Assert.Equal(3, registry.LiveCount);
        Assert.Equal(new[] { new SessionId(2), new SessionId(1), new SessionId(3) }, registry.KnownSessions.ToArray());
    }

    [Fact]
    public void TheSameRackIdInTwoSessionsHasIndependentState()
    {
        var registry = new BrowserSessionRegistry();
        var one = Create(registry, 1);
        var two = Create(registry, 2);

        one.ApplySelectionContext(One("R1"));
        one.Pin("R1");
        one.ApplySummary("R1", BrowserTabStatus.Ready, "Rack", "Selective");
        two.ApplySelectionContext(One("R1"));

        Assert.Equal("R1+|R1", Shape(one));
        Assert.Equal("R1~|R1", Shape(two));
        Assert.Equal(BrowserTabStatus.Ready, one.Tabs.Single().Status);
        Assert.Equal(BrowserTabStatus.NotLoaded, two.Tabs.Single().Status);

        two.Close("R1");
        Assert.Equal("R1+|R1", Shape(one));
        Assert.Equal("|-", Shape(two));
    }

    [Fact]
    public void TwoRegistriesDoNotShareState()
    {
        var first = Create(new BrowserSessionRegistry(), 1);
        var second = Create(new BrowserSessionRegistry(), 1);

        first.ApplySelectionContext(One("R1"));

        Assert.NotSame(first, second);
        Assert.Equal("|-", Shape(second));
    }

    [Fact]
    public void DestroyRetiresTheSessionEmptiesItsTabsAndLeavesTheInstanceInert()
    {
        var registry = new BrowserSessionRegistry();
        var session = Create(registry, 1);
        session.ApplySelectionContext(One("A"));
        session.Pin("A");
        session.ApplySelectionContext(One("B"));
        Assert.Equal("A+,B~|B", Shape(session));

        registry.Destroy(new SessionId(1));

        Assert.True(session.IsDestroyed);
        Assert.Equal("|-", Shape(session));
        Assert.Null(session.ActiveRackId);
        Assert.Null(session.LiveSurfaceRackId);
        Assert.Equal(0, registry.LiveCount);
        Assert.Empty(registry.KnownSessions);
        Assert.False(registry.TryGet(new SessionId(1), out var found));
        Assert.Null(found);
    }

    [Fact]
    public void EveryOperationOnADestroyedSessionReturnsNoneAndChangesNothing()
    {
        var registry = new BrowserSessionRegistry();
        var session = Create(registry, 1);
        session.ApplySelectionContext(One("A"));
        registry.Destroy(new SessionId(1));

        Assert.Equal(BrowserChange.None, session.ApplySelectionContext(One("A")));
        Assert.Equal(BrowserChange.None, session.ApplySelectionContext(One("B")));
        Assert.Equal(BrowserChange.None, session.ApplySelectionContext(SelectionContext.None));
        Assert.Equal(BrowserChange.None, session.Activate("A"));
        Assert.Equal(BrowserChange.None, session.Pin("A"));
        Assert.Equal(BrowserChange.None, session.Close("A"));
        Assert.Equal(BrowserChange.None, session.ApplySummary("A", BrowserTabStatus.Ready, "n", "k"));

        Assert.True(session.IsDestroyed);
        Assert.Equal("|-", Shape(session));
    }

    [Fact]
    public void ADestroyedSessionIdIsNeverResurrected()
    {
        var registry = new BrowserSessionRegistry();
        var session = Create(registry, 1);
        session.ApplySelectionContext(One("A"));
        registry.Destroy(new SessionId(1));

        Assert.Null(registry.GetOrCreate(new SessionId(1)));
        Assert.False(registry.TryGet(new SessionId(1), out var found));
        Assert.Null(found);
        Assert.Equal(0, registry.LiveCount);
        Assert.Empty(registry.KnownSessions);
        Assert.Equal("|-", Shape(session));
    }

    [Fact]
    public void DestroyingAnIdThatHadNoSessionStillBlocksItAndDestroyingTwiceIsHarmless()
    {
        var registry = new BrowserSessionRegistry();

        registry.Destroy(new SessionId(9));
        registry.Destroy(new SessionId(9));

        Assert.Null(registry.GetOrCreate(new SessionId(9)));
        Assert.Equal(0, registry.LiveCount);
    }

    [Fact]
    public void DestroyingOneSessionLeavesTheOthersAlive()
    {
        var registry = new BrowserSessionRegistry();
        var one = Create(registry, 1);
        var two = Create(registry, 2);
        var three = Create(registry, 3);
        one.ApplySelectionContext(One("A"));
        two.ApplySelectionContext(One("A"));
        three.ApplySelectionContext(One("A"));

        registry.Destroy(new SessionId(2));

        Assert.Equal(2, registry.LiveCount);
        Assert.Equal(new[] { new SessionId(1), new SessionId(3) }, registry.KnownSessions.ToArray());
        Assert.Same(one, Create(registry, 1));
        Assert.Same(three, Create(registry, 3));
        Assert.Equal("A~|A", Shape(one));
        Assert.Equal("A~|A", Shape(three));
        Assert.Equal("|-", Shape(two));
        Assert.False(one.IsDestroyed);
        Assert.True(two.IsDestroyed);
    }

    [Fact]
    public void ASessionCreatedAfterADestructionStartsEmptyAndNoTabSurvives()
    {
        var registry = new BrowserSessionRegistry();
        var old = Create(registry, 1);
        old.ApplySelectionContext(One("A"));
        old.Pin("A");
        registry.Destroy(new SessionId(1));

        var fresh = Create(registry, 2);

        Assert.Equal("|-", Shape(fresh));
        Assert.Equal(1, registry.LiveCount);
        Assert.Equal(new[] { new SessionId(2) }, registry.KnownSessions.ToArray());
    }

    [Fact]
    public void KnownSessionsIsAStableSnapshot()
    {
        var registry = new BrowserSessionRegistry();
        Create(registry, 1);
        var snapshot = registry.KnownSessions;

        Create(registry, 2);
        registry.Destroy(new SessionId(1));

        Assert.Equal(new[] { new SessionId(1) }, snapshot.ToArray());
        Assert.Equal(new[] { new SessionId(2) }, registry.KnownSessions.ToArray());
    }
}
