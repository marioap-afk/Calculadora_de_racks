using System.Globalization;
using RackCad.Application.Workspace;

namespace RackCad.UI.Workspace
{
    /// <summary>
    /// One read-only row of the instrumentation table (I-64 D-18): a measurement point of the in-memory report with the
    /// time and the memory already formatted the same way for the same value.
    /// </summary>
    public sealed class WorkspaceMeasurementRow
    {
        private const double BytesPerMegabyte = 1024.0 * 1024.0;

        public WorkspaceMeasurementRow(WorkspaceMeasurement measurement)
        {
            Sequence = measurement.Sequence;
            Point = measurement.Point;
            Milliseconds = measurement.Milliseconds;
            MemoryBytes = measurement.ProcessMemoryBytes;
            Count = measurement.Count;
        }

        public long Sequence { get; }

        public string Point { get; }

        public double Milliseconds { get; }

        public long MemoryBytes { get; }

        public int Count { get; }

        public string MillisecondsText => FormatMilliseconds(Milliseconds);

        public string MemoryText => FormatMemory(MemoryBytes);

        /// <summary>Time with one decimal and the unit, culture independent.</summary>
        public static string FormatMilliseconds(double milliseconds) =>
            milliseconds.ToString("0.0", CultureInfo.InvariantCulture) + " ms";

        /// <summary>Memory in megabytes with one decimal and the unit, culture independent.</summary>
        public static string FormatMemory(long bytes) =>
            (bytes / BytesPerMegabyte).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
    }
}
