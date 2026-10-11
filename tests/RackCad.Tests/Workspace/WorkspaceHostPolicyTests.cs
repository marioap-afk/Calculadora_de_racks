#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T3A-HOST-CORE: INV-F1-T3A-01 (lazy single instance) and INV-F1-T3A-10 (pure host policies).
public class WorkspaceHostPolicyTests
{
    private static WorkspaceHostConditions Ok() => new(
        Started: true,
        HasActiveDocument: true,
        SessionIsActive: true,
        SessionDegraded: false,
        PanelVisible: true,
        Quiescent: true,
        ModalRackCadActive: false);

    public static TheoryData<string, WorkspaceHostConditions> BlockedConditions() => new()
    {
        { "not started", Ok() with { Started = false } },
        { "no document", Ok() with { HasActiveDocument = false } },
        { "degraded session", Ok() with { SessionDegraded = true } },
        { "hidden panel", Ok() with { PanelVisible = false } },
        { "a command is running", Ok() with { Quiescent = false } },
        { "a RackCad modal is open", Ok() with { ModalRackCadActive = true } },
    };

    [Fact]
    public void ADrainIsAllowedOnlyWhenEveryConditionHolds()
    {
        Assert.True(WorkspaceHostPolicy.MayDrain(Ok()));
    }

    [Theory]
    [MemberData(nameof(BlockedConditions))]
    public void AnySingleFailingConditionBlocksTheDrain(string reason, WorkspaceHostConditions conditions)
    {
        Assert.False(WorkspaceHostPolicy.MayDrain(conditions), reason);
    }

    [Fact]
    public void AReadIsAllowedOnlyWhenEveryConditionHolds()
    {
        Assert.True(WorkspaceHostPolicy.MayRead(Ok()));
    }

    [Theory]
    [MemberData(nameof(BlockedConditions))]
    public void AnySingleFailingConditionBlocksTheRead(string reason, WorkspaceHostConditions conditions)
    {
        Assert.False(WorkspaceHostPolicy.MayRead(conditions), reason);
    }

    [Fact]
    public void ASessionThatIsNotTheActiveOneIsNeverRead()
    {
        var other = Ok() with { SessionIsActive = false };
        Assert.False(WorkspaceHostPolicy.MayRead(other));
    }

    [Fact]
    public void WithEverythingInOrderTheActionsThatReadAndWriteTheDrawingAreEnabled()
    {
        var actions = WorkspaceHostPolicy.Actions(Ok());
        Assert.True(actions.CanReadDrawing);
        Assert.True(actions.CanWriteDrawing);
        Assert.Equal(WorkspaceActionBlock.None, actions.Block);
    }

    [Fact]
    public void AHiddenPanelDoesNotDisableTheActionsItIsNotWhatBlocksThem()
    {
        var actions = WorkspaceHostPolicy.Actions(Ok() with { PanelVisible = false });
        Assert.True(actions.CanReadDrawing);
        Assert.True(actions.CanWriteDrawing);
    }

    public static TheoryData<string, WorkspaceHostConditions, WorkspaceActionBlock> DisabledActions() => new()
    {
        { "never opened", Ok() with { Started = false }, WorkspaceActionBlock.NotStarted },
        { "no document", Ok() with { HasActiveDocument = false }, WorkspaceActionBlock.NoDocument },
        { "degraded", Ok() with { SessionDegraded = true }, WorkspaceActionBlock.Degraded },
        { "a RackCad modal", Ok() with { ModalRackCadActive = true }, WorkspaceActionBlock.ModalActive },
        { "an AutoCAD command", Ok() with { Quiescent = false }, WorkspaceActionBlock.CommandActive },
    };

    [Theory]
    [MemberData(nameof(DisabledActions))]
    public void ACommandAModalOrNoDocumentDisableTheActionsThatTouchTheDrawing(string reason, WorkspaceHostConditions conditions, WorkspaceActionBlock block)
    {
        var actions = WorkspaceHostPolicy.Actions(conditions);
        Assert.False(actions.CanReadDrawing, reason);
        Assert.False(actions.CanWriteDrawing, reason);
        Assert.Equal(block, actions.Block);
    }

    [Fact]
    public void WhenSeveralReasonsHoldTheMostFundamentalOneIsReported()
    {
        var everythingWrong = new WorkspaceHostConditions(false, false, false, true, false, false, true);
        Assert.Equal(WorkspaceActionBlock.NotStarted, WorkspaceHostPolicy.Actions(everythingWrong).Block);

        var started = everythingWrong with { Started = true };
        Assert.Equal(WorkspaceActionBlock.NoDocument, WorkspaceHostPolicy.Actions(started).Block);

        var withDocument = started with { HasActiveDocument = true };
        Assert.Equal(WorkspaceActionBlock.Degraded, WorkspaceHostPolicy.Actions(withDocument).Block);

        var healthy = withDocument with { SessionDegraded = false };
        Assert.Equal(WorkspaceActionBlock.ModalActive, WorkspaceHostPolicy.Actions(healthy).Block);

        var noModal = healthy with { ModalRackCadActive = false };
        Assert.Equal(WorkspaceActionBlock.CommandActive, WorkspaceHostPolicy.Actions(noModal).Block);
    }

    [Fact]
    public void TheStatusIsEmptyWithoutADocumentAndNotStartedBeforeTheFirstOpen()
    {
        Assert.Equal(WorkspaceHostStatus.NotStarted, WorkspaceHostPolicy.Status(Ok() with { Started = false, HasActiveDocument = false }));
        Assert.Equal(WorkspaceHostStatus.NoDocument, WorkspaceHostPolicy.Status(Ok() with { HasActiveDocument = false }));
    }

    [Fact]
    public void ADegradedSessionHasTheDegradedStatus()
    {
        Assert.Equal(WorkspaceHostStatus.Degraded, WorkspaceHostPolicy.Status(Ok() with { SessionDegraded = true }));
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, true)]
    public void AHealthyActiveSessionIsReadyWhateverTheVisibilityTheCommandOrTheModal(bool visible, bool quiescent, bool modal)
    {
        var conditions = Ok() with { PanelVisible = visible, Quiescent = quiescent, ModalRackCadActive = modal };
        Assert.Equal(WorkspaceHostStatus.Ready, WorkspaceHostPolicy.Status(conditions));
    }

    // ------------------------------------------------------------------ INV-F1-T3A-01: the lazy single instance

    private sealed class Probe
    {
    }

    [Fact]
    public void NothingIsCreatedBeforeTheFirstRequest()
    {
        var created = 0;
        var holder = new WorkspaceLazyInstance<Probe>(() =>
        {
            created++;
            return new Probe();
        });

        Assert.False(holder.IsCreated);
        Assert.Equal(0, holder.CreateCount);
        Assert.Equal(0, created);
    }

    [Fact]
    public void TheFirstRequestCreatesTheInstanceExactlyOnce()
    {
        var created = 0;
        var holder = new WorkspaceLazyInstance<Probe>(() =>
        {
            created++;
            return new Probe();
        });

        var first = holder.GetOrCreate();

        Assert.NotNull(first);
        Assert.True(holder.IsCreated);
        Assert.Equal(1, holder.CreateCount);
        Assert.Equal(1, created);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(100)]
    public void RepeatedRequestsReturnTheSameInstanceAndNeverCreateASecond(int requests)
    {
        var created = 0;
        var holder = new WorkspaceLazyInstance<Probe>(() =>
        {
            created++;
            return new Probe();
        });

        var first = holder.GetOrCreate();
        for (var i = 1; i < requests; i++) Assert.Same(first, holder.GetOrCreate());

        Assert.Equal(1, created);
        Assert.Equal(1, holder.CreateCount);
    }

    [Fact]
    public void ConcurrentFirstRequestsStillCreateOneInstance()
    {
        var created = 0;
        var holder = new WorkspaceLazyInstance<Probe>(() =>
        {
            System.Threading.Interlocked.Increment(ref created);
            return new Probe();
        });

        var results = new Probe[32];
        Parallel.For(0, results.Length, i => results[i] = holder.GetOrCreate());

        Assert.Equal(1, created);
        Assert.All(results, r => Assert.Same(results[0], r));
    }

    [Fact]
    public void AFactoryThatFailsLeavesNothingCreatedAndTheNextRequestCanSucceed()
    {
        var attempts = 0;
        var holder = new WorkspaceLazyInstance<Probe>(() =>
        {
            attempts++;
            if (attempts == 1) throw new InvalidOperationException("first attempt fails");
            return new Probe();
        });

        Assert.Throws<InvalidOperationException>(() => holder.GetOrCreate());
        Assert.False(holder.IsCreated);
        Assert.Equal(0, holder.CreateCount);

        var second = holder.GetOrCreate();
        Assert.NotNull(second);
        Assert.True(holder.IsCreated);
        Assert.Same(second, holder.GetOrCreate());
    }

    [Fact]
    public void AMissingFactoryIsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new WorkspaceLazyInstance<Probe>(null!));
    }
}
