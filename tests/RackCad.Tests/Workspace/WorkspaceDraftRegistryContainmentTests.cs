#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests;

/// <summary>A comparer that counts its calls and can be told to throw: it proves when an operation does NOT ask the comparer.</summary>
internal sealed class ContainmentCountingComparer : IEqualityComparer<DraftForm>
{
    public int Calls { get; private set; }

    public bool Throw { get; set; }

    public string Message { get; set; } = "the comparer threw";

    public bool Equals(DraftForm? x, DraftForm? y)
    {
        Calls++;
        if (Throw) throw new InvalidOperationException(Message);
        return DraftKit.Comparer.Equals(x, y);
    }

    public int GetHashCode(DraftForm obj) => DraftKit.Comparer.GetHashCode(obj);
}

// I-64 F4-T2B2B-LIFECYCLE (INV-F4T2B2B-02, D-T2B2-10, F4-E29): the four explicit and additive operations of the registry of drafts that
// never depend on the comparer. The ordinary operations (Discard, DiscardAll, RetireSession) keep the semantics of F4-T1 and still
// decide from the comparer before they remove anything; these four remove and detach whatever the comparer says. Every test that
// expects a failure to be contained uses Record.Exception, so a missing behavior fails by ASSERTION and never by an escaped exception.
// Obligation 14 of the order (destroying the document retires the session and its drafts) has its registry side here and its host
// side in WorkspaceLifecycleHostTests.
public class WorkspaceDraftRegistryContainmentTests
{
    private static readonly SessionId S1 = DraftKit.S1;
    private static readonly SessionId S2 = DraftKit.S2;

    private static DraftSession<DraftForm> Born(DraftRegistry registry, string rack, IEqualityComparer<DraftForm> comparer, SessionId? session = null) =>
        DraftKit.Born(registry, session ?? S1, rack, comparer: comparer);

    private static void MakeDirty(DraftSession<DraftForm> draft) =>
        Assert.Equal(DraftEditStatus.Updated, draft.SetCurrentDraftState(draft.CurrentDraftState with { Name = "Edited" }).Status);

    // ------------------------------------------------------------------ DiscardByUser

    [Fact]
    public void DiscardByUserRemovesTheDraftWithoutCallingTheComparerEvenIfItAlwaysThrows()
    {
        var registry = new DraftRegistry();
        var comparer = new ContainmentCountingComparer();
        var draft = Born(registry, "R1", comparer);
        var other = Born(registry, "R2", DraftKit.Comparer);
        comparer.Throw = true;
        var before = comparer.Calls;

        DraftUserDiscardResult result = default;
        var thrown = Record.Exception(() => result = registry.DiscardByUser(S1, "R1"));

        Assert.Null(thrown);
        Assert.True(result.Removed);
        Assert.False(result.HadPendingText);
        Assert.Equal(before, comparer.Calls);
        Assert.False(registry.Contains(S1, "R1"));
        Assert.False(draft.IsRegistered);
        Assert.True(other.IsRegistered);
        Assert.Equal(1, registry.CountOf(S1));
        Assert.False(registry.IsRetired(S1));
    }

    [Fact]
    public void DiscardByUserSaysWhetherTheRemovedDraftHadPendingTextAndLeavesTheSessionAlive()
    {
        var registry = new DraftRegistry();
        var comparer = new ContainmentCountingComparer();
        var draft = Born(registry, "R1", comparer);
        draft.SetPendingText("name", "half typed");
        var before = comparer.Calls;

        DraftUserDiscardResult result = default;
        var thrown = Record.Exception(() => result = registry.DiscardByUser(S1, "R1"));

        Assert.Null(thrown);
        Assert.True(result.Removed);
        Assert.True(result.HadPendingText);
        Assert.Equal(before, comparer.Calls);
        Assert.Equal(0, registry.CountOf(S1));
        Assert.False(registry.IsRetired(S1));
        Assert.Equal(DraftBeginStatus.Created, registry.Begin(S1, "R1", DraftKit.Read(), DraftKit.Comparer, DraftKit.CopyForm).Status);
    }

    [Fact]
    public void DiscardByUserOnAMissingDraftSaysFalseAndFalseAndTouchesNothingElse()
    {
        var registry = new DraftRegistry();
        var kept = Born(registry, "R1", DraftKit.Comparer);

        DraftUserDiscardResult missing = new(true, true);
        DraftUserDiscardResult otherSession = new(true, true);
        var thrown = Record.Exception(() =>
        {
            missing = registry.DiscardByUser(S1, "R9");
            otherSession = registry.DiscardByUser(S2, "R1");
        });

        Assert.Null(thrown);
        Assert.False(missing.Removed);
        Assert.False(missing.HadPendingText);
        Assert.False(otherSession.Removed);
        Assert.False(otherSession.HadPendingText);
        Assert.True(kept.IsRegistered);
        Assert.Equal(1, registry.CountOf(S1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void DiscardByUserValidatesTheRackIdLikeTheOrdinaryOperations(string? rackId)
    {
        var registry = new DraftRegistry();

        var ordinary = Record.Exception(() => registry.Discard(S1, rackId!));
        var explicitDiscard = Record.Exception(() => registry.DiscardByUser(S1, rackId!));

        Assert.IsType<ArgumentException>(ordinary);
        Assert.IsType<ArgumentException>(explicitDiscard);
    }

    // ------------------------------------------------------------------ DiscardAllByUser

    [Fact]
    public void DiscardAllByUserRemovesEveryDraftOfTheSessionWithoutCallingTheComparerAndOrdersTheListsOrdinally()
    {
        var registry = new DraftRegistry();
        var comparer = new ContainmentCountingComparer();
        var a = Born(registry, "b", comparer);
        var b = Born(registry, "A", comparer);
        var c = Born(registry, "a", comparer);
        var elsewhere = Born(registry, "A", DraftKit.Comparer, S2);
        a.SetPendingText("name", "x");
        c.SetPendingText("name", "y");
        comparer.Throw = true;
        var before = comparer.Calls;

        DraftUserDiscardAllResult? result = null;
        var thrown = Record.Exception(() => result = registry.DiscardAllByUser(S1));

        Assert.Null(thrown);
        Assert.NotNull(result);
        Assert.Equal(new[] { "A", "a", "b" }, result!.RemovedRackIds.ToArray());
        Assert.Equal(new[] { "a", "b" }, result.PendingTextRackIds.ToArray());
        Assert.Equal(before, comparer.Calls);
        Assert.Equal(0, registry.CountOf(S1));
        Assert.False(a.IsRegistered);
        Assert.False(b.IsRegistered);
        Assert.False(c.IsRegistered);
        Assert.True(elsewhere.IsRegistered);
        Assert.False(registry.IsRetired(S1));
    }

    [Fact]
    public void DiscardAllByUserOnASessionWithoutDraftsGivesEmptyListsThatAreNeverNull()
    {
        var registry = new DraftRegistry();

        DraftUserDiscardAllResult? result = null;
        var thrown = Record.Exception(() => result = registry.DiscardAllByUser(S1));

        Assert.Null(thrown);
        Assert.NotNull(result);
        Assert.NotNull(result!.RemovedRackIds);
        Assert.NotNull(result.PendingTextRackIds);
        Assert.Empty(result.RemovedRackIds);
        Assert.Empty(result.PendingTextRackIds);
        Assert.False(registry.IsRetired(S1));
    }

    [Fact]
    public void TheOrdinaryDiscardAllStillThrowsWithAComparerThatThrowsAndRemovesNothingWhileTheExplicitOneCompletes()
    {
        // The distinction of the Owner (F4-E29): the ordinary operations keep their F4-T1 semantics, the explicit one does not need the comparer.
        var registry = new DraftRegistry();
        var comparer = new ContainmentCountingComparer();
        var draft = Born(registry, "R1", comparer);
        comparer.Throw = true;

        Assert.Throws<InvalidOperationException>(() => registry.DiscardAll(S1));
        Assert.Throws<InvalidOperationException>(() => registry.Discard(S1, "R1"));
        Assert.True(draft.IsRegistered);
        Assert.Equal(1, registry.CountOf(S1));

        DraftUserDiscardAllResult? result = null;
        var thrown = Record.Exception(() => result = registry.DiscardAllByUser(S1));

        Assert.Null(thrown);
        Assert.Equal(new[] { "R1" }, result!.RemovedRackIds.ToArray());
        Assert.Equal(0, registry.CountOf(S1));
        Assert.False(draft.IsRegistered);
    }

    // ------------------------------------------------------------------ RetireKnownSafe

    [Fact]
    public void RetireKnownSafeRemovesEveryDraftWithoutCallingTheComparerAndLosesOnlyThePendingText()
    {
        var registry = new DraftRegistry();
        var comparer = new ContainmentCountingComparer();
        var clean = Born(registry, "R1", comparer);
        var pending = Born(registry, "R2", comparer);
        var elsewhere = Born(registry, "R1", DraftKit.Comparer, S2);
        pending.SetPendingText("name", "typed");
        comparer.Throw = true;
        var before = comparer.Calls;

        DraftLossNotice? notice = null;
        var thrown = Record.Exception(() => notice = registry.RetireKnownSafe(S1));

        Assert.Null(thrown);
        Assert.NotNull(notice);
        Assert.Equal(S1, notice!.Session);
        Assert.Equal(new[] { "R2" }, notice.LostRackIds.ToArray());
        Assert.True(notice.HasLoss);
        Assert.Empty(notice.UncertainRackIds);
        Assert.NotNull(notice.UncertainReasons);
        Assert.Empty(notice.UncertainReasons);
        Assert.Equal(before, comparer.Calls);
        Assert.True(registry.IsRetired(S1));
        Assert.Equal(0, registry.CountOf(S1));
        Assert.False(clean.IsRegistered);
        Assert.False(pending.IsRegistered);
        Assert.True(elsewhere.IsRegistered);
        Assert.Equal(DraftBeginStatus.SessionRetired, registry.Begin(S1, "R9", DraftKit.Read(), DraftKit.Comparer, DraftKit.CopyForm).Status);
    }

    [Fact]
    public void RetireKnownSafeOnASessionWithoutDraftsStillMarksItRetiredAndReportsNoLoss()
    {
        var registry = new DraftRegistry();

        DraftLossNotice? notice = null;
        var thrown = Record.Exception(() => notice = registry.RetireKnownSafe(S1));

        Assert.Null(thrown);
        Assert.NotNull(notice);
        Assert.False(notice!.HasLoss);
        Assert.Empty(notice.LostRackIds);
        Assert.Empty(notice.UncertainRackIds);
        Assert.True(registry.IsRetired(S1));
    }

    // ------------------------------------------------------------------ RetireConservative

    [Fact]
    public void RetireConservativeCompletesWithAComparerThatThrowsOnOneDraftAndNotOnAnotherAndReportsTheUnknownWithItsReason()
    {
        var registry = new DraftRegistry();
        var broken = new ContainmentCountingComparer { Throw = true, Message = "the comparer of R1 failed" };
        var unknown = Born(registry, "R1", broken);
        var knownDirty = Born(registry, "R2", DraftKit.Comparer);
        var knownClean = Born(registry, "R3", DraftKit.Comparer);
        var elsewhere = Born(registry, "R1", DraftKit.Comparer, S2);
        MakeDirty(knownDirty);

        DraftLossNotice? notice = null;
        var thrown = Record.Exception(() => notice = registry.RetireConservative(S1));

        Assert.Null(thrown);
        Assert.NotNull(notice);
        Assert.Equal(new[] { "R1", "R2" }, notice!.LostRackIds.ToArray());
        Assert.Equal(new[] { "R1" }, notice.UncertainRackIds.ToArray());
        Assert.Equal("the comparer of R1 failed", notice.UncertainReasons["R1"]);
        Assert.Single(notice.UncertainReasons);
        Assert.True(notice.HasLoss);
        Assert.True(registry.IsRetired(S1));
        Assert.Equal(0, registry.CountOf(S1));
        Assert.False(unknown.IsRegistered);
        Assert.False(knownDirty.IsRegistered);
        Assert.False(knownClean.IsRegistered);
        Assert.True(elsewhere.IsRegistered);
    }

    [Fact]
    public void RetireConservativeAsksTheComparerOnlyAfterThePendingTextSoADraftWithPendingTextIsLostAndNeverUncertain()
    {
        var registry = new DraftRegistry();
        var broken = new ContainmentCountingComparer { Throw = true };
        var pending = Born(registry, "R1", broken);
        pending.SetPendingText("name", "typed");
        var before = broken.Calls;

        DraftLossNotice? notice = null;
        var thrown = Record.Exception(() => notice = registry.RetireConservative(S1));

        Assert.Null(thrown);
        Assert.Equal(new[] { "R1" }, notice!.LostRackIds.ToArray());
        Assert.Empty(notice.UncertainRackIds);
        Assert.Empty(notice.UncertainReasons);
        Assert.Equal(before, broken.Calls);
    }

    [Fact]
    public void RetireConservativeNeverInfersCleanWhenTheComparerFailsOnEveryDraft()
    {
        var registry = new DraftRegistry();
        var broken = new ContainmentCountingComparer { Throw = true };
        Born(registry, "b", broken);
        Born(registry, "A", broken);
        Born(registry, "a", broken);

        DraftLossNotice? notice = null;
        var thrown = Record.Exception(() => notice = registry.RetireConservative(S1));

        Assert.Null(thrown);
        Assert.Equal(new[] { "A", "a", "b" }, notice!.LostRackIds.ToArray());
        Assert.Equal(new[] { "A", "a", "b" }, notice.UncertainRackIds.ToArray());
        Assert.Equal(new[] { "A", "a", "b" }, notice.UncertainReasons.Keys.ToArray());
        Assert.All(notice.UncertainRackIds, id => Assert.Contains(id, notice.LostRackIds));
        Assert.Equal(0, registry.CountOf(S1));
    }

    [Fact]
    public void RetireConservativeOfDraftsThatAreCleanForAWorkingComparerReportsNoLossAndEveryListIsNeverNull()
    {
        var registry = new DraftRegistry();
        Born(registry, "R1", DraftKit.Comparer);
        Born(registry, "R2", DraftKit.Comparer);

        DraftLossNotice? notice = null;
        var thrown = Record.Exception(() => notice = registry.RetireConservative(S1));

        Assert.Null(thrown);
        Assert.NotNull(notice);
        Assert.False(notice!.HasLoss);
        Assert.NotNull(notice.LostRackIds);
        Assert.NotNull(notice.UncertainRackIds);
        Assert.NotNull(notice.UncertainReasons);
        Assert.Empty(notice.LostRackIds);
        Assert.True(registry.IsRetired(S1));
        Assert.Equal(0, registry.CountOf(S1));
    }

    [Fact]
    public void TheOrdinaryRetireSessionStillThrowsAndLeavesTheSessionAliveWhileTheConservativeRetirementCompletes()
    {
        var registry = new DraftRegistry();
        var broken = new ContainmentCountingComparer { Throw = true };
        var draft = Born(registry, "R1", broken);

        Assert.Throws<InvalidOperationException>(() => registry.RetireSession(S1));
        Assert.False(registry.IsRetired(S1));
        Assert.True(draft.IsRegistered);

        DraftLossNotice? notice = null;
        var thrown = Record.Exception(() => notice = registry.RetireConservative(S1));

        Assert.Null(thrown);
        Assert.Equal(new[] { "R1" }, notice!.UncertainRackIds.ToArray());
        Assert.True(registry.IsRetired(S1));
        Assert.False(draft.IsRegistered);
    }

    [Fact]
    public void ARetiredSessionIsNeverResurrectedByTheExplicitRetirements()
    {
        var registry = new DraftRegistry();
        Born(registry, "R1", DraftKit.Comparer);
        registry.RetireConservative(S1);

        Assert.True(registry.IsRetired(S1));
        Assert.Equal(DraftBeginStatus.SessionRetired, registry.Begin(S1, "R1", DraftKit.Read(), DraftKit.Comparer, DraftKit.CopyForm).Status);

        Born(registry, "R1", DraftKit.Comparer, S2);
        registry.RetireKnownSafe(S2);
        Assert.True(registry.IsRetired(S2));
        Assert.Equal(DraftBeginStatus.SessionRetired, registry.Begin(S2, "R1", DraftKit.Read(), DraftKit.Comparer, DraftKit.CopyForm).Status);
    }

    // ------------------------------------------------------------------ none of the four frees the Applying slot or writes

    [Theory]
    [InlineData("DiscardByUser")]
    [InlineData("DiscardAllByUser")]
    [InlineData("RetireKnownSafe")]
    [InlineData("RetireConservative")]
    public void NoExplicitOperationFreesTheSingleApplyingSlotOfTheRegistry(string operation)
    {
        var registry = new DraftRegistry();
        var applying = DraftKit.ToApplying(Born(registry, "R1", DraftKit.Comparer));
        Assert.True(registry.HasUpdateInProgress);
        var second = Born(registry, "R2", DraftKit.Comparer, S2);

        var thrown = Record.Exception(() =>
        {
            switch (operation)
            {
                case "DiscardByUser": registry.DiscardByUser(S1, "R1"); break;
                case "DiscardAllByUser": registry.DiscardAllByUser(S1); break;
                case "RetireKnownSafe": registry.RetireKnownSafe(S1); break;
                default: registry.RetireConservative(S1); break;
            }
        });

        Assert.Null(thrown);
        Assert.False(applying.IsRegistered);
        Assert.Equal(0, registry.CountOf(S1));
        Assert.True(registry.HasUpdateInProgress);
        Assert.False(second.TryBeginApply().IsStarted);
    }

    // ------------------------------------------------------------------ the shape of the additions

    [Fact]
    public void TheNoticeOfLossGainsTheUncertainMembersAdditivelyAndKeepsItsConstructor()
    {
        var notice = typeof(DraftLossNotice);

        Assert.NotNull(notice.GetProperty("UncertainRackIds", BindingFlags.Public | BindingFlags.Instance));
        Assert.NotNull(notice.GetProperty("UncertainReasons", BindingFlags.Public | BindingFlags.Instance));
        Assert.Equal(typeof(IReadOnlyList<string>), notice.GetProperty("UncertainRackIds")!.PropertyType);
        Assert.Equal(typeof(IReadOnlyDictionary<string, string>), notice.GetProperty("UncertainReasons")!.PropertyType);
        Assert.NotNull(notice.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(SessionId), typeof(IReadOnlyList<string>) }, null));

        // An ordinary retirement of F4-T1 builds the notice with the old constructor: the new members are present and empty, never null.
        var registry = new DraftRegistry();
        DraftKit.Born(registry, S1, "R1");
        var ordinary = registry.RetireSession(S1);
        Assert.NotNull(ordinary.UncertainRackIds);
        Assert.NotNull(ordinary.UncertainReasons);
        Assert.Empty(ordinary.UncertainRackIds);
        Assert.Empty(ordinary.UncertainReasons);
    }

    [Fact]
    public void TheUncertainReasonsAreReadOnlyAndOrdinal()
    {
        var registry = new DraftRegistry();
        var broken = new ContainmentCountingComparer { Throw = true };
        Born(registry, "R1", broken);

        var notice = registry.RetireConservative(S1);

        Assert.True(notice.UncertainReasons.ContainsKey("R1"));
        Assert.False(notice.UncertainReasons.ContainsKey("r1"));
        var asDictionary = Assert.IsAssignableFrom<IDictionary<string, string>>(notice.UncertainReasons);
        Assert.True(asDictionary.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => asDictionary.Add("R2", "x"));
    }

    [Fact]
    public void TheFourExplicitOperationsAreAdditiveAndTheOrdinaryOnesKeepTheirSignatures()
    {
        var methods = typeof(DraftRegistry).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).ToArray();

        Assert.Equal(typeof(DraftUserDiscardResult), Assert.Single(methods, m => m.Name == "DiscardByUser").ReturnType);
        Assert.Equal(typeof(DraftUserDiscardAllResult), Assert.Single(methods, m => m.Name == "DiscardAllByUser").ReturnType);
        Assert.Equal(typeof(DraftLossNotice), Assert.Single(methods, m => m.Name == "RetireKnownSafe").ReturnType);
        Assert.Equal(typeof(DraftLossNotice), Assert.Single(methods, m => m.Name == "RetireConservative").ReturnType);
        Assert.Equal(typeof(DraftDiscardResult), Assert.Single(methods, m => m.Name == "Discard").ReturnType);
        Assert.Equal(typeof(DraftDiscardAllResult), Assert.Single(methods, m => m.Name == "DiscardAll").ReturnType);
        Assert.Equal(typeof(DraftLossNotice), Assert.Single(methods, m => m.Name == "RetireSession").ReturnType);
        foreach (var name in new[] { "DiscardByUser", "RetireKnownSafe", "RetireConservative" })
        {
            var parameters = methods.Single(m => m.Name == name).GetParameters();
            Assert.Equal(typeof(SessionId), parameters[0].ParameterType);
        }

        Assert.Equal(new[] { typeof(SessionId), typeof(string) }, methods.Single(m => m.Name == "DiscardByUser").GetParameters().Select(p => p.ParameterType).ToArray());
        Assert.Equal(new[] { typeof(SessionId) }, methods.Single(m => m.Name == "DiscardAllByUser").GetParameters().Select(p => p.ParameterType).ToArray());
    }
}
