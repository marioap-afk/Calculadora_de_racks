#nullable enable
using System.Threading;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F1-T3B-HOST-UI (INV-F1-T3B-06): a visible panel creates no work. Without a state notice (the host asking the
    /// view-model to refresh) or an explicit user interaction there is no query at all, however much time passes; with a
    /// notice there is one query of the snapshot and, only when the instrumentation is on, one of the measurement report;
    /// the panel never authorizes and never asks for the selection context.
    /// </summary>
    public class WorkspacePanelNoWorkTests
    {
        [Fact]
        public void ConstructingTheViewModelQueriesNothing()
        {
            var port = new WorkspacePortDouble();

            _ = new WorkspacePanelViewModel(port, _ => { });

            Assert.Equal(0, port.TotalCalls);
        }

        [Fact]
        public void ConstructingTheRootControlQueriesNothingAndTimePassingQueriesNothing()
        {
            var port = new WorkspacePortDouble();
            WorkspacePanelView? view = null;
            StaTestRunner.Run(() => view = new WorkspacePanelView(new WorkspacePanelViewModel(port, _ => { })));
            Assert.NotNull(view);

            // The shared STA thread keeps pumping its dispatcher while this thread waits: a timer or a polling loop
            // living in the control would tick during this wait.
            Thread.Sleep(400);
            StaTestRunner.Run(() => { });
            Thread.Sleep(200);

            Assert.Equal(0, port.TotalCalls);
        }

        [Fact]
        public void ARestingPanelWithAStateAlreadyPresentedAsksForNothingMore()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready() };
            var panel = new WorkspacePanelViewModel(port, _ => { });
            panel.Refresh();
            var after = port.TotalCalls;

            Thread.Sleep(300);

            Assert.Equal(1, after);
            Assert.Equal(after, port.TotalCalls);
        }

        [Fact]
        public void OneNoticeIsOneSnapshotQueryAndNoReportQueryWhileTheInstrumentationIsOff()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(instrumentation: false) };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.Refresh();

            Assert.Equal(1, port.SnapshotCalls);
            Assert.Equal(0, port.ReportCalls);
        }

        [Fact]
        public void OneNoticeWithTheInstrumentationOnIsOneSnapshotQueryAndOneReportQuery()
        {
            var port = new WorkspacePortDouble
            {
                Snapshot = WorkspacePanelFixtures.Ready(instrumentation: true),
                Report = WorkspacePanelFixtures.Report(1024),
            };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.Refresh();

            Assert.Equal(1, port.SnapshotCalls);
            Assert.Equal(1, port.ReportCalls);
        }

        [Fact]
        public void EachNoticeCostsExactlyOneQueryOfTheSnapshot()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready() };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.Refresh();
            panel.Refresh();
            panel.Refresh();

            Assert.Equal(3, port.SnapshotCalls);
        }

        [Fact]
        public void NoStateNeverAuthorizesNorAsksForTheSelectionContext()
        {
            foreach (var snapshot in new[]
                     {
                         WorkspacePanelFixtures.NotStarted(), WorkspacePanelFixtures.NoDocument(),
                         WorkspacePanelFixtures.Ready(instrumentation: true), WorkspacePanelFixtures.Degraded(),
                     })
            {
                var port = new WorkspacePortDouble { Snapshot = snapshot, Report = WorkspacePanelFixtures.Report(0) };
                var panel = new WorkspacePanelViewModel(port, _ => { });

                panel.Refresh();
                panel.SetInstrumentation(true);
                panel.Retry();

                Assert.Equal(0, port.AuthorizeCalls);
                Assert.Equal(0, port.SelectionCalls);
            }
        }

        [Fact]
        public void TheExplicitToggleCostsOneSnapshotQueryAfterTheDelegate()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready() };
            var snapshotCallsSeenByTheDelegate = -1;
            var panel = new WorkspacePanelViewModel(port, _ => snapshotCallsSeenByTheDelegate = port.SnapshotCalls);

            panel.SetInstrumentation(true);

            Assert.Equal(0, snapshotCallsSeenByTheDelegate);
            Assert.Equal(1, port.SnapshotCalls);
        }
    }
}
