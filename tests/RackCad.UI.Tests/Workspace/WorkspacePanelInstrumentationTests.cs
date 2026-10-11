#nullable enable
using System.Linq;
using System.Reflection;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F1-T3B-HOST-UI (INV-F1-T3B-13): the panel presents the measurement report of the injected port, read-only
    /// (enabled flag, points by sequence with time, memory and count, and the current process memory), and the switch
    /// reaches the host as a neutral injected delegate. Nothing is written anywhere by the panel.
    /// </summary>
    public class WorkspacePanelInstrumentationTests
    {
        private const long Megabyte = 1024 * 1024;

        private static WorkspaceMeasurementReport ThreePoints() => WorkspacePanelFixtures.Report(
            memoryBytes: 64 * Megabyte,
            new WorkspaceMeasurement(1, WorkspaceMeasurementPoints.PanelOpen, 12.5, 40 * Megabyte, 0),
            new WorkspaceMeasurement(2, WorkspaceMeasurementPoints.Drain, 3.5, 41 * Megabyte, 4),
            new WorkspaceMeasurement(3, WorkspaceMeasurementPoints.SelectionRead, 0.04, 2 * Megabyte, 9));

        [Fact]
        public void AnEnabledReportIsPresentedPointBySequenceWithTimeMemoryAndCount()
        {
            var port = new WorkspacePortDouble
            {
                Snapshot = WorkspacePanelFixtures.Ready(instrumentation: true),
                Report = ThreePoints(),
            };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.Refresh();

            Assert.True(panel.InstrumentationEnabled);
            Assert.Equal(3, panel.Measurements.Count);
            Assert.Equal(new long[] { 1, 2, 3 }, panel.Measurements.Select(m => m.Sequence).ToArray());
            Assert.Equal(
                new[] { WorkspaceMeasurementPoints.PanelOpen, WorkspaceMeasurementPoints.Drain, WorkspaceMeasurementPoints.SelectionRead },
                panel.Measurements.Select(m => m.Point).ToArray());
            Assert.Equal("12.5 ms", panel.Measurements[0].MillisecondsText);
            Assert.Equal("3.5 ms", panel.Measurements[1].MillisecondsText);
            Assert.Equal("40.0 MB", panel.Measurements[0].MemoryText);
            Assert.Equal("2.0 MB", panel.Measurements[2].MemoryText);
            Assert.Equal(4, panel.Measurements[1].Count);
            Assert.Equal(9, panel.Measurements[2].Count);
        }

        [Fact]
        public void TheCurrentProcessMemoryIsPresentedWhenTheInstrumentationIsOn()
        {
            var port = new WorkspacePortDouble
            {
                Snapshot = WorkspacePanelFixtures.Ready(instrumentation: true),
                Report = ThreePoints(),
            };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.Refresh();

            Assert.Contains("64.0 MB", panel.ProcessMemoryText);
            Assert.NotEmpty(panel.InstrumentationSummary);
            Assert.Contains("3", panel.InstrumentationSummary);
        }

        [Fact]
        public void ADisabledInstrumentationPresentsNoPointsNoMemoryAndAnOffSummary()
        {
            var port = new WorkspacePortDouble
            {
                Snapshot = WorkspacePanelFixtures.Ready(instrumentation: false),
                Report = ThreePoints(),
            };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.Refresh();

            Assert.False(panel.InstrumentationEnabled);
            Assert.Empty(panel.Measurements);
            Assert.Equal(string.Empty, panel.ProcessMemoryText);
            Assert.NotEmpty(panel.InstrumentationSummary);
        }

        [Fact]
        public void TheOnAndOffSummariesDiffer()
        {
            var on = new WorkspacePanelViewModel(
                new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(instrumentation: true), Report = ThreePoints() },
                _ => { });
            var off = new WorkspacePanelViewModel(new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready() }, _ => { });

            on.Refresh();
            off.Refresh();

            Assert.NotEmpty(on.InstrumentationSummary);
            Assert.NotEqual(on.InstrumentationSummary, off.InstrumentationSummary);
        }

        [Fact]
        public void EnablingCallsTheInjectedDelegateOnceWithTrueAndThenRefreshesTheSnapshot()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready() };
            var received = new System.Collections.Generic.List<bool>();
            var panel = new WorkspacePanelViewModel(port, received.Add);

            panel.SetInstrumentation(true);

            Assert.Equal(new[] { true }, received.ToArray());
            Assert.Equal(1, port.SnapshotCalls);
        }

        [Fact]
        public void DisablingCallsTheInjectedDelegateOnceWithFalse()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(instrumentation: true), Report = ThreePoints() };
            var received = new System.Collections.Generic.List<bool>();
            var panel = new WorkspacePanelViewModel(port, received.Add);
            panel.Refresh();

            panel.SetInstrumentation(false);

            Assert.Equal(new[] { false }, received.ToArray());
        }

        [Fact]
        public void ThePanelPresentsWhatTheHostSaysAfterTheSwitchNotWhatWasRequested()
        {
            var port = new WorkspacePortDouble { Snapshot = WorkspacePanelFixtures.Ready(instrumentation: false) };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.SetInstrumentation(true);

            Assert.False(panel.InstrumentationEnabled);
            Assert.Empty(panel.Measurements);
        }

        [Fact]
        public void AnEnabledSnapshotWithNoPointsPresentsAnEmptyTable()
        {
            var port = new WorkspacePortDouble
            {
                Snapshot = WorkspacePanelFixtures.Ready(instrumentation: true),
                Report = WorkspacePanelFixtures.Report(8 * Megabyte),
            };
            var panel = new WorkspacePanelViewModel(port, _ => { });

            panel.Refresh();

            Assert.True(panel.InstrumentationEnabled);
            Assert.Empty(panel.Measurements);
            Assert.Contains("8.0 MB", panel.ProcessMemoryText);
        }

        [Fact]
        public void TheMeasurementsAreReadOnlyForTheView()
        {
            var property = typeof(WorkspacePanelViewModel).GetProperty(nameof(WorkspacePanelViewModel.Measurements))!;

            Assert.True(property.GetSetMethod(nonPublic: false) == null);
            Assert.Equal(typeof(System.Collections.Generic.IReadOnlyList<WorkspaceMeasurementRow>), property.PropertyType);
            Assert.NotEmpty(typeof(WorkspacePanelViewModel).GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.Name == "SetInstrumentation"));
        }

        [Fact]
        public void TheRowsFormatTimeAndMemoryTheSameWayForTheSameValue()
        {
            Assert.Equal("0.0 ms", WorkspaceMeasurementRow.FormatMilliseconds(0));
            Assert.Equal("1234.6 ms", WorkspaceMeasurementRow.FormatMilliseconds(1234.56));
            Assert.Equal("0.0 MB", WorkspaceMeasurementRow.FormatMemory(0));
            Assert.Equal("1.5 MB", WorkspaceMeasurementRow.FormatMemory(Megabyte + Megabyte / 2));
        }
    }
}
