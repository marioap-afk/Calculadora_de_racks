#nullable enable
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F2-T1-NAVIGATION-CORE: D-07 (planning of Select, View, Center, LocateView), D-03/INV-22 (session rejection) and
// INV-40 (revalidation right before acting) over a simulated effects port.
public class WorkspaceNavigationActionPolicyTests
{
    private static readonly SessionId S1 = new(1);
    private static readonly SessionId S2 = new(2);

    private sealed class Rig
    {
        public Rig()
        {
            Port = new FakeNavigationFacts().With(
                S1,
                NavFact.Member(10, "R-1", "frontal", 0, 0x101, 0x102),
                NavFact.Member(11, "R-1", "lateral", 0, 0x103, 0x104),
                NavFact.Member(12, "R-1", "planta", -1, 0x105),
                NavFact.Member(13, "R-9", "frontal", 0, 0x901));
            Targets = new NavigationTargets(Port);
            Policy = new NavigationActionPolicy(Targets);
            Effects = new RecordingNavigationEffects();
        }

        public FakeNavigationFacts Port { get; }

        public NavigationTargets Targets { get; }

        public NavigationActionPolicy Policy { get; }

        public RecordingNavigationEffects Effects { get; }

        public NavigationActionPlan Plan(
            NavigationActionKind action,
            int copy = 0,
            NavigationViewOrder? view = null,
            NavigationActionKind locateWith = NavigationActionKind.View) =>
            Policy.Plan(new WorkspaceRequest(S1), "R-1", action, copy, view, locateWith);
    }

    [Fact]
    public void SelectPlansEveryReferenceOfTheRackInTheDeterministicOrder()
    {
        var rig = new Rig();

        var plan = rig.Plan(NavigationActionKind.Select);

        Assert.Equal(new long[] { 0x101, 0x102, 0x103, 0x104, 0x105 }, plan.Handles);
        Assert.Equal(NavigationActionKind.Select, plan.Effect);
        Assert.Equal(new WorkspaceRequest(S1), plan.Origin);
        Assert.Equal("R-1", plan.RackId);
    }

    [Theory]
    [InlineData(NavigationActionKind.View)]
    [InlineData(NavigationActionKind.Center)]
    public void ViewAndCenterPlanThePrimaryReferenceAndCopyKWalksTheRest(NavigationActionKind action)
    {
        var rig = new Rig();

        Assert.Equal(new long[] { 0x101 }, rig.Plan(action).Handles);
        Assert.Equal(new long[] { 0x102 }, rig.Plan(action, 1).Handles);
        Assert.Equal(new long[] { 0x105 }, rig.Plan(action, 4).Handles);
        Assert.Empty(rig.Plan(action, 5).Handles);
        Assert.Empty(rig.Plan(action, -1).Handles);
        Assert.Equal(action, rig.Plan(action).Effect);
    }

    [Fact]
    public void LocateViewPlansTheFirstReferenceOfThatViewAndItsCopiesWithTheChosenEffect()
    {
        var rig = new Rig();

        var first = rig.Plan(NavigationActionKind.LocateView, 0, NavigationViewOrder.Lateral);
        var second = rig.Plan(NavigationActionKind.LocateView, 1, NavigationViewOrder.Lateral, NavigationActionKind.Center);

        Assert.Equal(new long[] { 0x103 }, first.Handles);
        Assert.Equal(NavigationActionKind.View, first.Effect);
        Assert.Equal(new long[] { 0x104 }, second.Handles);
        Assert.Equal(NavigationActionKind.Center, second.Effect);
        Assert.Empty(rig.Plan(NavigationActionKind.LocateView, 2, NavigationViewOrder.Lateral).Handles);
        Assert.Empty(rig.Plan(NavigationActionKind.LocateView, 0, NavigationViewOrder.Other).Handles);
        Assert.Empty(rig.Plan(NavigationActionKind.LocateView, 0, null).Handles);
    }

    [Fact]
    public void PlanningUsesTheCachedTargetsAndDoesNotTouchTheDocumentAgain()
    {
        var rig = new Rig();
        rig.Plan(NavigationActionKind.Select);
        var reads = rig.Port.Reads;

        rig.Plan(NavigationActionKind.View);
        rig.Plan(NavigationActionKind.Center, 1);

        Assert.Equal(reads, rig.Port.Reads);
    }

    [Fact]
    public void ADecisionWithEveryTargetStillAttributedActsAndDoesNotRunTheEffectsItself()
    {
        var rig = new Rig();
        var plan = rig.Plan(NavigationActionKind.View);

        var decision = rig.Policy.Decide(plan, S1);

        Assert.Equal(NavigationActionVerdict.Act, decision.Verdict);
        Assert.Equal(new long[] { 0x101 }, decision.Handles);
        Assert.False(decision.CacheInvalidated);
        Assert.False(decision.RefreshRequired);
        Assert.Empty(rig.Effects.Calls);
    }

    [Fact]
    public void SelectExecutesOnceOverAllTheReferences()
    {
        var rig = new Rig();

        var decision = rig.Policy.Execute(rig.Plan(NavigationActionKind.Select), S1, rig.Effects);

        Assert.Equal(NavigationActionVerdict.Act, decision.Verdict);
        Assert.Equal(new[] { "select:1:257,258,259,260,261" }, rig.Effects.Calls);
    }

    [Fact]
    public void ViewAndCenterExecuteOnTheirReference()
    {
        var rig = new Rig();

        rig.Policy.Execute(rig.Plan(NavigationActionKind.View, 1), S1, rig.Effects);
        rig.Policy.Execute(rig.Plan(NavigationActionKind.Center), S1, rig.Effects);

        Assert.Equal(new[] { "view:1:258", "center:1:257" }, rig.Effects.Calls);
    }

    [Fact]
    public void LocateViewExecutesViewOrCenterOnTheFirstReferenceOfThatView()
    {
        var rig = new Rig();

        rig.Policy.Execute(rig.Plan(NavigationActionKind.LocateView, 0, NavigationViewOrder.Planta), S1, rig.Effects);
        rig.Policy.Execute(rig.Plan(NavigationActionKind.LocateView, 0, NavigationViewOrder.Lateral, NavigationActionKind.Center), S1, rig.Effects);

        Assert.Equal(new[] { "view:1:261", "center:1:259" }, rig.Effects.Calls);
    }

    [Fact]
    public void SelectActsOnTheFreshSetWhenAnotherCopyAppearedAfterThePlan()
    {
        var rig = new Rig();
        var plan = rig.Plan(NavigationActionKind.Select);
        rig.Port.For(S1)[2] = NavFact.Member(12, "R-1", "planta", -1, 0x105, 0x106);

        var decision = rig.Policy.Execute(plan, S1, rig.Effects);

        Assert.Equal(NavigationActionVerdict.Act, decision.Verdict);
        Assert.Equal(new[] { "select:1:257,258,259,260,261,262" }, rig.Effects.Calls);
    }

    [Fact]
    public void RevalidationReadsTheDocumentFreshOnceRightBeforeActing()
    {
        var rig = new Rig();
        var plan = rig.Plan(NavigationActionKind.View);
        var reads = rig.Port.Reads;

        rig.Policy.Execute(plan, S1, rig.Effects);

        Assert.Equal(reads + 1, rig.Port.Reads);
        Assert.Equal(new[] { S1, S1 }, rig.Port.ReadSessions);
    }

    [Fact]
    public void ARequestOfAnotherSessionIsRejectedWithoutEffectsReadsOrCacheChanges()
    {
        var rig = new Rig();
        var plan = rig.Plan(NavigationActionKind.Select);
        var reads = rig.Port.Reads;

        var decision = rig.Policy.Execute(plan, S2, rig.Effects);

        Assert.Equal(NavigationActionVerdict.RejectedSessionMismatch, decision.Verdict);
        Assert.Empty(decision.Handles);
        Assert.False(decision.CacheInvalidated);
        Assert.False(decision.RefreshRequired);
        Assert.Empty(rig.Effects.Calls);
        Assert.Equal(reads, rig.Port.Reads);
        Assert.True(rig.Targets.IsCached(S1, "R-1"));
    }

    [Fact]
    public void ARequestOfADestroyedSessionIsRejectedWithoutEffectsOrReads()
    {
        var rig = new Rig();
        var plan = rig.Plan(NavigationActionKind.View);
        var reads = rig.Port.Reads;
        rig.Targets.EndSession(S1);

        var decision = rig.Policy.Execute(plan, S1, rig.Effects);

        Assert.Equal(NavigationActionVerdict.RejectedSessionDestroyed, decision.Verdict);
        Assert.Empty(rig.Effects.Calls);
        Assert.Equal(reads, rig.Port.Reads);
    }

    [Fact]
    public void ATargetDeletedBetweenResolutionAndExecutionDoesNotActInvalidatesAndAsksForARefresh()
    {
        var rig = new Rig();
        var plan = rig.Plan(NavigationActionKind.View);
        rig.Port.For(S1)[0] = NavFact.Member(10, "R-1", "frontal", 0, 0x102);

        var decision = rig.Policy.Execute(plan, S1, rig.Effects);

        Assert.Equal(NavigationActionVerdict.RejectedTargetGone, decision.Verdict);
        Assert.Empty(decision.Handles);
        Assert.True(decision.CacheInvalidated);
        Assert.True(decision.RefreshRequired);
        Assert.Empty(rig.Effects.Calls);
        Assert.False(rig.Targets.IsCached(S1, "R-1"));
    }

    [Fact]
    public void AReferenceThatLeftTheRackDoesNotActEvenIfItStillExists()
    {
        var rig = new Rig();
        var plan = rig.Plan(NavigationActionKind.Center);
        rig.Port.For(S1)[0] = NavFact.Member(10, "R-OTHER", "frontal", 0, 0x101, 0x102);

        var decision = rig.Policy.Execute(plan, S1, rig.Effects);

        Assert.Equal(NavigationActionVerdict.RejectedNotAttributable, decision.Verdict);
        Assert.True(decision.CacheInvalidated);
        Assert.True(decision.RefreshRequired);
        Assert.Empty(rig.Effects.Calls);
        Assert.False(rig.Targets.IsCached(S1, "R-1"));
    }

    [Fact]
    public void AMemberThatBecameUnreadableIsNotAttributableAndDoesNotAct()
    {
        var rig = new Rig();
        var plan = rig.Plan(NavigationActionKind.Select);
        rig.Port.For(S1)[1] = NavFact.Raw(11, false, false, null, "R-1", "selective", "lateral", 0, 0x103, 0x104);

        var decision = rig.Policy.Execute(plan, S1, rig.Effects);

        Assert.Equal(NavigationActionVerdict.RejectedNotAttributable, decision.Verdict);
        Assert.Empty(rig.Effects.Calls);
    }

    [Fact]
    public void AfterARejectedRevalidationTheNextResolutionRefreshesFromTheDocument()
    {
        var rig = new Rig();
        var plan = rig.Plan(NavigationActionKind.Select);
        rig.Port.For(S1).RemoveAt(2);
        rig.Policy.Execute(plan, S1, rig.Effects);

        var refreshed = rig.Targets.Resolve(S1, "R-1");

        Assert.Equal(new long[] { 0x101, 0x102, 0x103, 0x104 }, NavFact.Handles(refreshed));
        Assert.Empty(rig.Effects.Calls);
    }

    [Fact]
    public void ARackWithoutTargetsPlansNothingAndNeverActs()
    {
        var rig = new Rig();
        var plan = rig.Policy.Plan(new WorkspaceRequest(S1), "R-404", NavigationActionKind.Select);

        var decision = rig.Policy.Execute(plan, S1, rig.Effects);

        Assert.Empty(plan.Handles);
        Assert.Equal(NavigationActionVerdict.RejectedNoTargets, decision.Verdict);
        Assert.Empty(rig.Effects.Calls);
    }

    [Fact]
    public void APlanForADestroyedSessionHasNoHandles()
    {
        var rig = new Rig();
        rig.Targets.EndSession(S1);

        var plan = rig.Plan(NavigationActionKind.Select);

        Assert.Empty(plan.Handles);
    }

    [Fact]
    public void ARejectionOfOneSessionNeverTouchesTheCacheOfAnother()
    {
        var rig = new Rig();
        rig.Port.With(S2, NavFact.Member(20, "R-1", "frontal", 0, 0x401));
        rig.Targets.Resolve(S2, "R-1");
        var plan = rig.Plan(NavigationActionKind.View);
        rig.Port.For(S1).Clear();

        rig.Policy.Execute(plan, S1, rig.Effects);

        Assert.True(rig.Targets.IsCached(S2, "R-1"));
        Assert.False(rig.Targets.IsCached(S1, "R-1"));
    }
}
