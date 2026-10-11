#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>One structured measurement point: when it happened (sequence), how long it took and how much memory the process held.</summary>
    public sealed record WorkspaceMeasurement(long Sequence, string Point, double Milliseconds, long ProcessMemoryBytes, int Count);

    /// <summary>A snapshot of the collected measurements; it holds no reference to the collector.</summary>
    public sealed record WorkspaceMeasurementReport(bool Enabled, IReadOnlyList<WorkspaceMeasurement> Points, long CurrentProcessMemoryBytes);

    /// <summary>The names of the measurement points the host emits.</summary>
    public static class WorkspaceMeasurementPoints
    {
        public const string PanelOpen = "panel.open";
        public const string PanelShow = "panel.show";
        public const string PanelHide = "panel.hide";
        public const string Drain = "drain";
        public const string SelectionRead = "selection.read";
        public const string CacheInvalidate = "cache.invalidate";
    }

    /// <summary>Disposable handle of one timing; disposing it records the point (when the instrumentation was on at <c>Begin</c>).</summary>
    public sealed class WorkspaceMeasurementScope : IDisposable
    {
        internal static WorkspaceMeasurementScope None { get; } = new();

        private WorkspaceMeasurementScope()
        {
        }

        public void SetCount(int count)
        {
        }

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// In-memory instrumentation of the panel host (I-64 D-18): a switch, timings, process memory and structured
    /// measurement points. It writes nothing anywhere: no log, no file, no setting. Off by default.
    /// </summary>
    public sealed class WorkspaceInstrumentation
    {
        public const int Capacity = 256;

        public WorkspaceInstrumentation(Func<double>? nowMilliseconds = null, Func<long>? processMemoryBytes = null)
        {
        }

        public bool Enabled { get; set; }

        public WorkspaceMeasurementScope Begin(string point, int count = 0) => WorkspaceMeasurementScope.None;

        public void Record(string point, double milliseconds, int count = 0)
        {
        }

        public WorkspaceMeasurementReport Report() => new(Enabled, Array.Empty<WorkspaceMeasurement>(), 0);

        public void Clear()
        {
        }
    }
}
