#nullable enable
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F2-T1-NAVIGATION-CORE: D-05 on-demand computation, cache per (session, RackId), invalidation and no persistence.
public class WorkspaceNavigationCacheTests
{
    private static readonly SessionId S1 = new(1);
    private static readonly SessionId S2 = new(2);

    private static (NavigationTargets Targets, FakeNavigationFacts Port) Build()
    {
        var port = new FakeNavigationFacts()
            .With(S1, NavFact.Member(10, "R-1", "frontal", 0, 0x101), NavFact.Member(11, "R-2", "frontal", 0, 0x201), NavFact.Member(12, "R-3", "frontal", 0, 0x301))
            .With(S2, NavFact.Member(20, "R-1", "frontal", 0, 0x401));
        return (new NavigationTargets(port), port);
    }

    [Fact]
    public void ResolvingTwiceReadsTheDocumentOnceAndReturnsTheSameSet()
    {
        var (targets, port) = Build();

        var first = targets.Resolve(S1, "R-1");
        var second = targets.Resolve(S1, "R-1");

        Assert.Same(first, second);
        Assert.Equal(1, port.Reads);
        Assert.True(targets.IsCached(S1, "R-1"));
    }

    [Fact]
    public void NothingIsComputedUntilItIsAsked()
    {
        var (targets, port) = Build();

        Assert.Equal(0, port.Reads);
        Assert.False(targets.IsCached(S1, "R-1"));
        Assert.Equal(0, targets.CachedRackCount(S1));
    }

    [Fact]
    public void TheRackIdOfTheCacheKeyIgnoresCase()
    {
        var (targets, port) = Build();

        var first = targets.Resolve(S1, "r-1");
        var second = targets.Resolve(S1, "R-1");

        Assert.Same(first, second);
        Assert.Equal(1, port.Reads);
        Assert.True(targets.IsCached(S1, "R-1"));
        Assert.Equal(1, targets.CachedRackCount(S1));
    }

    [Fact]
    public void OnlyTheRequestedRackIsRetainedNeverTheWholeDocument()
    {
        var (targets, _) = Build();

        targets.Resolve(S1, "R-1");

        Assert.Equal(1, targets.CachedRackCount(S1));
        Assert.False(targets.IsCached(S1, "R-2"));
        Assert.False(targets.IsCached(S1, "R-3"));
    }

    [Fact]
    public void TheSameRackIdInTwoSessionsIsTwoIndependentEntries()
    {
        var (targets, port) = Build();

        var a = targets.Resolve(S1, "R-1");
        var b = targets.Resolve(S2, "R-1");

        Assert.NotSame(a, b);
        Assert.Equal(new long[] { 0x101 }, NavFact.Handles(a));
        Assert.Equal(new long[] { 0x401 }, NavFact.Handles(b));
        Assert.Equal(2, port.Reads);

        targets.Invalidate(S1, "R-1");
        Assert.False(targets.IsCached(S1, "R-1"));
        Assert.True(targets.IsCached(S2, "R-1"));
    }

    [Fact]
    public void InvalidatingOneRackIgnoresCaseAndKeepsTheOthers()
    {
        var (targets, port) = Build();
        targets.Resolve(S1, "R-1");
        targets.Resolve(S1, "R-2");

        targets.Invalidate(S1, "r-1");

        Assert.False(targets.IsCached(S1, "R-1"));
        Assert.True(targets.IsCached(S1, "R-2"));
        port.For(S1)[0] = NavFact.Member(10, "R-1", "frontal", 0, 0x102);
        Assert.Equal(new long[] { 0x102 }, NavFact.Handles(targets.Resolve(S1, "R-1")));
    }

    [Fact]
    public void InvalidatingASessionDropsAllItsRacksAndNoOtherSession()
    {
        var (targets, _) = Build();
        targets.Resolve(S1, "R-1");
        targets.Resolve(S1, "R-2");
        targets.Resolve(S2, "R-1");

        targets.InvalidateSession(S1);

        Assert.Equal(0, targets.CachedRackCount(S1));
        Assert.Equal(1, targets.CachedRackCount(S2));
    }

    [Fact]
    public void ADrainThatRanTheNavigationInvalidationDropsTheSessionCache()
    {
        var (targets, port) = Build();
        targets.Resolve(S1, "R-1");
        targets.Resolve(S2, "R-1");
        var drain = new DrainResult(true, new[] { DrainOperation.InvalidateNavigationCaches, DrainOperation.MarkDraftsPossiblyStale });

        var applied = targets.ApplyDrain(S1, drain);

        Assert.True(applied);
        Assert.False(targets.IsCached(S1, "R-1"));
        Assert.True(targets.IsCached(S2, "R-1"));
        targets.Resolve(S1, "R-1");
        Assert.Equal(3, port.Reads);
    }

    [Fact]
    public void ADrainWithoutTheNavigationInvalidationOrThatDidNotRunLeavesTheCache()
    {
        var (targets, _) = Build();
        targets.Resolve(S1, "R-1");

        Assert.False(targets.ApplyDrain(S1, new DrainResult(true, new[] { DrainOperation.ReadImplicitSelection, DrainOperation.RecomputeSelectionContext })));
        Assert.False(targets.ApplyDrain(S1, new DrainResult(false, System.Array.Empty<DrainOperation>())));
        Assert.True(targets.IsCached(S1, "R-1"));
    }

    [Theory]
    [InlineData(HintKind.DatabaseObjectChanged)]
    [InlineData(HintKind.CommandEnded)]
    public void ARealSessionDrainForAChangeHintInvalidatesTheNavigationCache(HintKind hint)
    {
        var (targets, _) = Build();
        var registry = new WorkspaceSessionRegistry();
        var session = registry.GetOrCreate(500);
        targets.Resolve(session.Id, "R-1");
        session.EnqueueHint(hint);

        var drain = session.TryDrain(new DrainConditions(true, false, true));

        Assert.True(targets.ApplyDrain(session.Id, drain));
        Assert.False(targets.IsCached(session.Id, "R-1"));
    }

    [Fact]
    public void ASelectionOnlyHintNeverInvalidatesTheNavigationCache()
    {
        var (targets, _) = Build();
        var registry = new WorkspaceSessionRegistry();
        var session = registry.GetOrCreate(501);
        targets.Resolve(session.Id, "R-1");
        session.EnqueueHint(HintKind.ImpliedSelectionChanged);

        var drain = session.TryDrain(new DrainConditions(true, false, true));

        Assert.False(targets.ApplyDrain(session.Id, drain));
        Assert.True(targets.IsCached(session.Id, "R-1"));
    }

    [Fact]
    public void EndingASessionDropsItsCacheAndItNeverResolvesAgain()
    {
        var (targets, port) = Build();
        targets.Resolve(S1, "R-1");
        targets.Resolve(S2, "R-1");
        var readsBefore = port.Reads;

        targets.EndSession(S1);

        Assert.True(targets.IsSessionEnded(S1));
        Assert.False(targets.IsSessionEnded(S2));
        Assert.Equal(0, targets.CachedRackCount(S1));
        Assert.True(targets.Resolve(S1, "R-1").IsEmpty);
        Assert.False(targets.IsCached(S1, "R-1"));
        Assert.Equal(readsBefore, port.Reads);
        Assert.True(targets.IsCached(S2, "R-1"));
    }

    [Fact]
    public void AnInvalidatedEntryIsRecomputedFromTheCurrentFacts()
    {
        var (targets, port) = Build();
        Assert.Single(targets.Resolve(S1, "R-1").Targets);
        port.For(S1).Add(NavFact.Member(13, "R-1", "lateral", 0, 0x102));
        Assert.Single(targets.Resolve(S1, "R-1").Targets);

        targets.InvalidateSession(S1);

        Assert.Equal(new long[] { 0x101, 0x102 }, NavFact.Handles(targets.Resolve(S1, "R-1")));
    }

    [Fact]
    public void TheCacheIsInstanceMemoryOnlyAndNeverPersisted()
    {
        var code = Src.Code("src/RackCad.Application/Workspace/NavigationTargets.cs");

        Assert.DoesNotMatch(new Regex(@"\bstatic\b[^;(]*(Dictionary|List|HashSet)"), code);
        Assert.DoesNotMatch(new Regex(@"System\.IO|\bFile\b|Serialize|Json|Xrecord|XData|Registry|UserSettings"), code);
        Assert.Matches(new Regex(@"private\s+readonly\s+Dictionary<"), code);
    }
}
