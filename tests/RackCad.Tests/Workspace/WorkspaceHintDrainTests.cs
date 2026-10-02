using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests.Workspace;

// I-64 F1-T1-MODEL: D-04 / INV-01, INV-06, INV-07, INV-23, INV-33.
public class WorkspaceHintDrainTests
{
    private static readonly DrainConditions Ready = new(IsQuiescent: true, ModalRackCadActive: false, PanelVisible: true);

    private static WorkspaceSession NewSession() => new WorkspaceSessionRegistry().GetOrCreate(1);

    [Fact]
    public void HintsAreQueuedAndNeverChangeTheContext()
    {
        var s = NewSession();
        Assert.True(s.EnqueueHint(HintKind.ImpliedSelectionChanged));
        Assert.True(s.EnqueueHint(HintKind.DatabaseObjectChanged));
        Assert.Equal(SelectionContext.None, s.Selection);
        Assert.True(s.PendingHintCount > 0);
    }

    [Fact]
    public void NHintsCoalesceIntoASingleDrain()
    {
        var s = NewSession();
        for (var i = 0; i < 50; i++) s.EnqueueHint(HintKind.ImpliedSelectionChanged);

        var first = s.TryDrain(Ready);
        var second = s.TryDrain(Ready);

        Assert.True(first.Drained);
        Assert.Contains(DrainOperation.ReadImplicitSelection, first.Operations);
        Assert.Contains(DrainOperation.RecomputeSelectionContext, first.Operations);
        Assert.Equal(0, s.PendingHintCount);
        Assert.False(second.Drained);
        Assert.Empty(second.Operations);
    }

    [Fact]
    public void WithoutHintsThereIsNoWorkAtAnyQuiescence()
    {
        var s = NewSession();
        for (var i = 0; i < 5; i++)
        {
            var r = s.TryDrain(Ready);
            Assert.False(r.Drained);
            Assert.Empty(r.Operations);
        }
        Assert.Equal(0, s.PendingHintCount);
        // and the session still accepts hints afterwards: the model does not depend on events being absent.
        Assert.True(s.EnqueueHint(HintKind.CommandEnded));
    }

    [Theory]
    [InlineData(false, false, true)]   // a command is running
    [InlineData(true, true, true)]     // a RackCad modal is open
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]   // hidden panel: zero reads
    public void HintsArrivingWhileBusyWaitAndDrainOnceConditionsHold(bool quiescent, bool modal, bool visible)
    {
        var s = NewSession();
        s.EnqueueHint(HintKind.ImpliedSelectionChanged);
        s.EnqueueHint(HintKind.DatabaseObjectChanged);

        var blocked = s.TryDrain(new DrainConditions(quiescent, modal, visible));

        Assert.False(blocked.Drained);
        Assert.Empty(blocked.Operations);
        Assert.True(s.PendingHintCount > 0);

        var later = s.TryDrain(Ready);
        Assert.True(later.Drained);
        Assert.Equal(0, s.PendingHintCount);
    }

    [Fact]
    public void HintsKeepArrivingDuringAModalWithoutBeingDrained()
    {
        var s = NewSession();
        var modal = new DrainConditions(true, true, true);
        for (var i = 0; i < 3; i++)
        {
            Assert.True(s.EnqueueHint(HintKind.CommandEnded));
            Assert.False(s.TryDrain(modal).Drained);
        }
        Assert.True(s.PendingHintCount > 0);
        Assert.True(s.TryDrain(Ready).Drained);
    }

    [Fact]
    public void DrainOnlyRunsTheOperationsAllowedInF1()
    {
        var allowed = new[]
        {
            DrainOperation.ReadImplicitSelection,
            DrainOperation.RecomputeSelectionContext,
            DrainOperation.InvalidateNavigationCaches,
            DrainOperation.MarkDraftsPossiblyStale,
        };
        Assert.Equal(allowed.OrderBy(x => x), System.Enum.GetValues<DrainOperation>().OrderBy(x => x));

        foreach (var kind in System.Enum.GetValues<HintKind>())
        {
            var s = NewSession();
            s.EnqueueHint(kind);
            var r = s.TryDrain(Ready);
            Assert.True(r.Drained);
            Assert.NotEmpty(r.Operations);
            Assert.All(r.Operations, op => Assert.Contains(op, allowed));
        }
    }

    [Fact]
    public void SelectionHintsReadAndRecomputeWhileObjectHintsInvalidateAndMark()
    {
        var sel = NewSession();
        sel.EnqueueHint(HintKind.ImpliedSelectionChanged);
        var selOps = sel.TryDrain(Ready).Operations;
        Assert.Contains(DrainOperation.ReadImplicitSelection, selOps);
        Assert.DoesNotContain(DrainOperation.MarkDraftsPossiblyStale, selOps);

        var obj = NewSession();
        obj.EnqueueHint(HintKind.DatabaseObjectChanged);
        var objOps = obj.TryDrain(Ready).Operations;
        Assert.Contains(DrainOperation.InvalidateNavigationCaches, objOps);
        Assert.Contains(DrainOperation.MarkDraftsPossiblyStale, objOps);
    }

    [Fact]
    public void HintsOfOneSessionNeverReachAnother()
    {
        var registry = new WorkspaceSessionRegistry();
        var a = registry.GetOrCreate(1);
        var b = registry.GetOrCreate(2);
        a.EnqueueHint(HintKind.ImpliedSelectionChanged);
        Assert.False(b.TryDrain(Ready).Drained);
        Assert.Equal(0, b.PendingHintCount);
        Assert.True(a.TryDrain(Ready).Drained);
    }
}
