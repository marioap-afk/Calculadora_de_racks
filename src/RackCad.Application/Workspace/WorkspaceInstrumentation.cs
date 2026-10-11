#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        private readonly string _point;
        private readonly double _startedAt;
        private WorkspaceInstrumentation? _owner;
        private int _count;

        internal WorkspaceMeasurementScope(WorkspaceInstrumentation? owner, string point, double startedAt, int count)
        {
            _owner = owner;
            _point = point;
            _startedAt = startedAt;
            _count = count;
        }

        internal static WorkspaceMeasurementScope None { get; } = new(null, string.Empty, 0, 0);

        /// <summary>Sets the count (for example the selected references) that the point will carry. Ignored by an inactive scope.</summary>
        public void SetCount(int count)
        {
            if (_owner != null) _count = count;
        }

        public void Dispose()
        {
            var owner = _owner;
            if (owner == null) return;
            _owner = null;
            owner.Complete(_point, _startedAt, _count);
        }
    }

    /// <summary>
    /// In-memory instrumentation of the panel host (I-64 D-18): a switch, timings, process memory and structured
    /// measurement points. It writes nothing anywhere: no log, no file, no setting. Off by default; while off it reads
    /// neither the clock nor the memory. It keeps only the most recent <see cref="Capacity"/> points.
    /// </summary>
    public sealed class WorkspaceInstrumentation
    {
        public const int Capacity = 256;

        private readonly Func<double> _nowMilliseconds;
        private readonly Func<long> _processMemoryBytes;
        private readonly object _gate = new();
        private readonly WorkspaceMeasurement?[] _ring = new WorkspaceMeasurement?[Capacity];
        private int _next;
        private int _size;
        private long _sequence;

        public WorkspaceInstrumentation(Func<double>? nowMilliseconds = null, Func<long>? processMemoryBytes = null)
        {
            _nowMilliseconds = nowMilliseconds ?? DefaultNow;
            _processMemoryBytes = processMemoryBytes ?? DefaultMemory;
        }

        public bool Enabled { get; set; }

        public WorkspaceMeasurementScope Begin(string point, int count = 0) =>
            Enabled ? new WorkspaceMeasurementScope(this, point, _nowMilliseconds(), count) : WorkspaceMeasurementScope.None;

        public void Record(string point, double milliseconds, int count = 0)
        {
            if (!Enabled) return;
            Append(point, milliseconds, count);
        }

        public WorkspaceMeasurementReport Report()
        {
            lock (_gate)
            {
                var points = new List<WorkspaceMeasurement>(_size);
                var first = _size < Capacity ? 0 : _next;
                for (var i = 0; i < _size; i++) points.Add(_ring[(first + i) % Capacity]!);
                return new WorkspaceMeasurementReport(Enabled, points, Enabled ? _processMemoryBytes() : 0);
            }
        }

        public void Clear()
        {
            lock (_gate)
            {
                Array.Clear(_ring, 0, _ring.Length);
                _next = 0;
                _size = 0;
            }
        }

        internal void Complete(string point, double startedAt, int count)
        {
            if (!Enabled) return;
            Append(point, _nowMilliseconds() - startedAt, count);
        }

        private void Append(string point, double milliseconds, int count)
        {
            var memory = _processMemoryBytes();
            lock (_gate)
            {
                _ring[_next] = new WorkspaceMeasurement(++_sequence, point, milliseconds < 0 ? 0 : milliseconds, memory, count);
                _next = (_next + 1) % Capacity;
                if (_size < Capacity) _size++;
            }
        }

        private static double DefaultNow() => Stopwatch.GetTimestamp() * 1000.0 / Stopwatch.Frequency;

        private static long DefaultMemory() => Environment.WorkingSet;
    }
}
