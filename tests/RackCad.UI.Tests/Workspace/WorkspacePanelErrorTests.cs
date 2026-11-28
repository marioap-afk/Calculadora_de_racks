#nullable enable
using System;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F1-T3B-HOST-UI (INV-F1-T3B-09 and 10): every panel-originated failure is contained at the panel boundary and
    /// becomes the Error state with a message and a retry. Error never writes, never changes a session and never asks for
    /// a reconciliation; a retry asks for the snapshot again and recovers the normal presentation. No global exception
    /// handler is involved: the tests only call the view-model and nothing escapes.
    /// </summary>
    public class WorkspacePanelErrorTests
    {
        [Fact]
        public void APortThatThrowsOnTheSnapshotIsContainedAndShownAsError()
        {
            var port = new WorkspacePortDouble { SnapshotFault = new InvalidOperationException("boom-snapshot") };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            var thrown = Record.Exception(panel.Refresh);

            Assert.Null(thrown);
            Assert.Equal(WorkspacePanelState.Error, panel.State);
            Assert.True(panel.HasError);
            Assert.Contains("boom-snapshot", panel.ErrorMessage);
        }

        [Fact]
        public void APortThatThrowsOnTheMeasurementReportIsContainedAndShownAsError()
        {
            var port = new WorkspacePortDouble
            {
                Snapshot = WorkspacePanelFixtures.Ready(instrumentation: true),
                ReportFault = new InvalidOperationException("boom-report"),
            };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            var thrown = Record.Exception(panel.Refresh);

            Assert.Null(thrown);
            Assert.Equal(WorkspacePanelState.Error, panel.State);
            Assert.Contains("boom-report", panel.ErrorMessage);
        }

        [Fact]
        public void ADelegateThatThrowsOnTheToggleIsContainedAndShownAsError()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready() };
            var panel = new WorkspacePanelViewModel(port, _ => throw new InvalidOperationException("boom-toggle"));
            panel.Refresh();

            var thrown = Record.Exception(() => panel.SetInstrumentation(true));

            Assert.Null(thrown);
            Assert.Equal(WorkspacePanelState.Error, panel.State);
            Assert.True(panel.HasError);
            Assert.Contains("boom-toggle", panel.ErrorMessage);
        }

        [Fact]
        public void RetryingAfterThePortRecoversReturnsToTheNormalPresentation()
        {
            var port = new WorkspacePortDouble { SnapshotFault = new InvalidOperationException("boom") };
            var panel = new WorkspacePanelViewModel(port, _ => { });
            panel.Refresh();
            Assert.Equal(WorkspacePanelState.Error, panel.State);

            port.SnapshotFault = null;
            port.Snapshot = WorkspacePanelFixtures.Ready(session: 5);
            panel.Retry();

            Assert.Equal(WorkspacePanelState.Ready, panel.State);
            Assert.False(panel.HasError);
            Assert.Equal(string.Empty, panel.ErrorMessage);
            Assert.Contains("5", panel.SessionText);
        }

        [Fact]
        public void RetryingWhileThePortStillFailsStaysInErrorAndNeverThrows()
        {
            var port = new WorkspacePortDouble { SnapshotFault = new InvalidOperationException("still-broken") };
            var panel = new WorkspacePanelViewModel(port, _ => { });
            panel.Refresh();

            var thrown = Record.Exception(panel.Retry);

            Assert.Null(thrown);
            Assert.Equal(WorkspacePanelState.Error, panel.State);
            Assert.Contains("still-broken", panel.ErrorMessage);
            Assert.Equal(2, port.SnapshotCalls);
        }

        [Fact]
        public void RetryingAsksForTheSnapshotExactlyOnceAndForNothingElse()
        {
            var port = new WorkspacePortDouble { SnapshotFault = new InvalidOperationException("boom") };
            var panel = new WorkspacePanelViewModel(port, _ => { });
            panel.Refresh();
            var before = port.SnapshotCalls;
            port.SnapshotFault = null;

            panel.Retry();

            Assert.Equal(before + 1, port.SnapshotCalls);
            Assert.Equal(0, port.AuthorizeCalls);
            Assert.Equal(0, port.SelectionCalls);
        }

        [Fact]
        public void ErrorNeverAuthorizesNorAsksForTheSelectionNorWritesAnything()
        {
            var port = new WorkspacePortDouble { SnapshotFault = new InvalidOperationException("boom") };
            var toggled = 0;
            var panel = new WorkspacePanelViewModel(port, _ => toggled++);

            panel.Refresh();
            panel.Retry();
            panel.Retry();

            Assert.Equal(0, port.AuthorizeCalls);
            Assert.Equal(0, port.SelectionCalls);
            Assert.Equal(0, toggled);
        }

        [Fact]
        public void ErrorIsRecoveredByTheNextHostRefreshWithoutAnExplicitRetry()
        {
            var port = new WorkspacePortDouble { SnapshotFault = new InvalidOperationException("boom") };
            var panel = new WorkspacePanelViewModel(port, _ => { });
            panel.Refresh();
            Assert.Equal(WorkspacePanelState.Error, panel.State);

            port.SnapshotFault = null;
            port.Snapshot = WorkspacePanelFixtures.NoDocument();
            panel.Refresh();

            Assert.Equal(WorkspacePanelState.NoDocument, panel.State);
            Assert.False(panel.HasError);
        }

        [Fact]
        public void ThePanelIsNotInErrorUntilSomethingFails()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready() };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.Refresh();

            Assert.False(panel.HasError);
            Assert.Equal(string.Empty, panel.ErrorMessage);
            Assert.NotEqual(WorkspacePanelState.Error, panel.State);
        }

        [Fact]
        public void AFaultWithoutAMessageStillGivesAReadableError()
        {
            var port = new WorkspacePortDouble { SnapshotFault = new Exception(string.Empty) };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.Refresh();

            Assert.Equal(WorkspacePanelState.Error, panel.State);
            Assert.NotEmpty(panel.ErrorMessage);
        }

        [Fact]
        public void TheErrorStateIsAPresentationStateOfThePanelNotAHostStatus()
        {
            Assert.DoesNotContain("Error", Enum.GetNames<WorkspaceHostStatus>());
            Assert.Contains("Error", Enum.GetNames<WorkspacePanelState>());
        }
    }
}
