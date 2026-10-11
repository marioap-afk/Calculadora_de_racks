#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F1-T3B-HOST-UI (INV-F1-T3B-08): the panel presents, deterministically and for the same snapshot, NotStarted,
    /// NoDocument (empty state), Ready and Degraded, the availability of the actions with its reason, and the PICKFIRST
    /// notice. The port is simulated; the view-model queries it only when it is told to.
    /// </summary>
    public class WorkspacePanelStateTests
    {
        private static WorkspacePanelViewModel Panel(WorkspacePortDouble port, Action<bool>? toggle = null) =>
            new(port, toggle ?? (_ => { }));

        [Fact]
        public void ThePanelIsLabelledWorkspace()
        {
            var panel = Panel(new WorkspacePortDouble());

            Assert.Equal("Workspace", panel.Label);
            Assert.Equal("Workspace", WorkspacePanelView.RootLabel);
        }

        [Fact]
        public void ANotStartedSnapshotIsPresentedAsNotStartedWithItsActionsDisabled()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.NotStarted() };
            var panel = Panel(port);

            panel.Refresh();

            Assert.Equal(WorkspacePanelState.NotStarted, panel.State);
            Assert.False(panel.CanReadDrawing);
            Assert.False(panel.CanWriteDrawing);
            Assert.Equal(WorkspaceActionBlock.NotStarted, panel.ActionBlock);
            Assert.NotEmpty(panel.StateTitle);
            Assert.NotEmpty(panel.ActionBlockText);
            Assert.False(panel.IsEmptyState);
        }

        [Fact]
        public void ANoDocumentSnapshotIsTheEmptyStateWithItsActionsDisabled()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.NoDocument() };
            var panel = Panel(port);

            panel.Refresh();

            Assert.Equal(WorkspacePanelState.NoDocument, panel.State);
            Assert.True(panel.IsEmptyState);
            Assert.False(panel.CanReadDrawing);
            Assert.False(panel.CanWriteDrawing);
            Assert.Equal(WorkspaceActionBlock.NoDocument, panel.ActionBlock);
            Assert.NotEmpty(panel.StateTitle);
            Assert.NotEmpty(panel.StateDetail);
            Assert.NotEmpty(panel.ActionBlockText);
        }

        [Fact]
        public void AReadySnapshotShowsTheActiveSessionAndAvailableActionsWithoutAReason()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 7) };
            var panel = Panel(port);

            panel.Refresh();

            Assert.Equal(WorkspacePanelState.Ready, panel.State);
            Assert.False(panel.IsEmptyState);
            Assert.Contains("7", panel.SessionText);
            Assert.True(panel.CanReadDrawing);
            Assert.False(panel.CanWriteDrawing);
            Assert.Equal(WorkspaceActionBlock.None, panel.ActionBlock);
            Assert.Equal(string.Empty, panel.ActionBlockText);
        }

        [Fact]
        public void ADegradedSnapshotIsPresentedAsDegradedWithTheReasonAndNoReading()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Degraded(session: 4) };
            var panel = Panel(port);

            panel.Refresh();

            Assert.Equal(WorkspacePanelState.Degraded, panel.State);
            Assert.False(panel.CanReadDrawing);
            Assert.Equal(WorkspaceActionBlock.Degraded, panel.ActionBlock);
            Assert.NotEmpty(panel.ActionBlockText);
            Assert.Contains("4", panel.SessionText);
        }

        [Fact]
        public void TheFourStatesAreTitledDifferently()
        {
            var titles = new List<string>();
            foreach (var snapshot in new[]
                     {
                         WorkspacePanelFixtures.NotStarted(), WorkspacePanelFixtures.NoDocument(),
                         WorkspacePanelFixtures.Ready(), WorkspacePanelFixtures.Degraded(),
                     })
            {
                var panel = Panel(new WorkspacePortDouble { Snapshot = snapshot });
                panel.Refresh();
                titles.Add(panel.StateTitle);
            }

            Assert.All(titles, t => Assert.NotEmpty(t));
            Assert.Equal(4, titles.Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void TheActionBlockReasonIsEmptyOnlyWhenNothingBlocksAndDifferentForEachBlock()
        {
            var reasons = new Dictionary<WorkspaceActionBlock, string>();
            foreach (var block in Enum.GetValues<WorkspaceActionBlock>())
            {
                var panel = Panel(new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.WithBlock(block) });
                panel.Refresh();
                Assert.Equal(block, panel.ActionBlock);
                reasons[block] = panel.ActionBlockText;
            }

            Assert.Equal(string.Empty, reasons[WorkspaceActionBlock.None]);
            foreach (var pair in reasons.Where(p => p.Key != WorkspaceActionBlock.None))
                Assert.NotEmpty(pair.Value);
            Assert.Equal(reasons.Count, reasons.Values.Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void APickFirstDisabledSnapshotShowsTheNoticeAndAnEnabledOneHidesIt()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(pickFirstDisabled: true) };
            var panel = Panel(port);

            panel.Refresh();
            Assert.True(panel.PickFirstNoticeVisible);
            Assert.Contains("PICKFIRST", panel.PickFirstNotice);

            port.Snapshot = WorkspacePanelFixtures.Ready(pickFirstDisabled: false);
            panel.Refresh();
            Assert.False(panel.PickFirstNoticeVisible);
        }

        [Fact]
        public void TheSameSnapshotIsAlwaysPresentedTheSameWay()
        {
            var snapshot = WorkspacePanelFixtures.Ready(session: 9, selection: SelectionContext.Selection(2, 1), pickFirstDisabled: true);
            var first = Panel(new WorkspacePortDouble { Snapshot = snapshot });
            var second = Panel(new WorkspacePortDouble { Snapshot = snapshot });

            first.Refresh();
            second.Refresh();
            second.Refresh();

            Assert.Equal(first.State, second.State);
            Assert.Equal(first.StateTitle, second.StateTitle);
            Assert.Equal(first.StateDetail, second.StateDetail);
            Assert.Equal(first.SessionText, second.SessionText);
            Assert.Equal(first.ActionBlockText, second.ActionBlockText);
            Assert.Equal(first.PickFirstNotice, second.PickFirstNotice);
            Assert.Equal(first.ContextTitle, second.ContextTitle);
            Assert.Equal(first.ContextText, second.ContextText);
            Assert.NotEmpty(first.StateTitle);
        }

        [Fact]
        public void TheStateFollowsTheLatestSnapshotWhenTheHostAsksAgain()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(session: 1) };
            var panel = Panel(port);

            panel.Refresh();
            Assert.Equal(WorkspacePanelState.Ready, panel.State);

            port.Snapshot = WorkspacePanelFixtures.NoDocument();
            panel.Refresh();
            Assert.Equal(WorkspacePanelState.NoDocument, panel.State);
            Assert.NotEmpty(panel.SessionText);
            Assert.DoesNotContain("1", panel.SessionText);
        }

        [Fact]
        public void PresentingNeverAuthorizesNorAsksForTheSelectionContext()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(selection: SelectionContext.One("R-1")) };
            var panel = Panel(port);

            panel.Refresh();
            panel.Refresh();

            Assert.Equal(0, port.AuthorizeCalls);
            Assert.Equal(0, port.SelectionCalls);
        }

        [Fact]
        public void TheViewModelRejectsAMissingPortAndAMissingToggle()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkspacePanelViewModel(null!, _ => { }));
            Assert.Throws<ArgumentNullException>(() => new WorkspacePanelViewModel(new WorkspacePortDouble(), null!));
        }
    }
}
