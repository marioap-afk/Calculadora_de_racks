using System;
using System.IO;

namespace RackCad.Application.Diagnostics
{
    /// <summary>
    /// The minimal RackCad diagnostics log (I-03): a best-effort, never-throwing sink under
    /// <c>%AppData%\RackCad\logs</c> consumed by the Plugin (<c>Report()</c> and the formerly-silent
    /// <c>catch</c> blocks) and by the best-effort Persistence stores. It turns failures that used to
    /// vanish without a trace into diagnosable log entries (type + message + stack trace). It is NOT a
    /// user-facing surface and NOT remote telemetry — just a local file.
    /// </summary>
    public static class RackLog
    {
        private static readonly RackDiagnosticsLog Default = new RackDiagnosticsLog(ResolveDirectory());

        /// <summary>The writable per-user log folder: <c>%AppData%\RackCad\logs</c> (mirrors <c>UserSettingsStore</c>).</summary>
        public static string LogDirectory => ResolveDirectory();

        /// <summary>Record an exception with its full stack trace under <paramref name="context"/>. Never throws.</summary>
        public static void Exception(string context, Exception ex) => Default.Exception(context, ex);

        /// <summary>Record a warning line under <paramref name="context"/>. Never throws.</summary>
        public static void Warning(string context, string message) => Default.Warning(context, message);

        private static string ResolveDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RackCad", "logs");
        }
    }
}
