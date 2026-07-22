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
        // The active sink. Production points it at %AppData%\RackCad\logs; a test can swap it for a temp
        // directory via RedirectForTests so no test ever writes to the real per-user log folder. volatile so
        // the swap is visible; reference writes are atomic (tests that swap run single-threaded/serialized).
        private static volatile RackDiagnosticsLog sink = new RackDiagnosticsLog(ResolveDirectory());

        /// <summary>The writable per-user log folder: <c>%AppData%\RackCad\logs</c> (mirrors <c>UserSettingsStore</c>).</summary>
        public static string LogDirectory => ResolveDirectory();

        /// <summary>Record an exception with its full stack trace under <paramref name="context"/>. Never throws.</summary>
        public static void Exception(string context, Exception ex) => sink.Exception(context, ex);

        /// <summary>Record a warning line under <paramref name="context"/>. Never throws.</summary>
        public static void Warning(string context, string message) => sink.Warning(context, message);

        /// <summary>
        /// Minimal internal test seam: redirect the diagnostics sink to <paramref name="directory"/> for the
        /// scope of the returned handle, so tests exercise real logging without ever writing to the production
        /// <c>%AppData%\RackCad\logs</c>. NOT a general filesystem abstraction — it only swaps the daily-file
        /// writer. Dispose restores the previous sink. Tests using it must run serialized (a single xUnit
        /// collection with parallelization disabled), because the sink is process-wide.
        /// </summary>
        internal static IDisposable RedirectForTests(string directory)
        {
            var previous = sink;
            sink = new RackDiagnosticsLog(directory);
            return new SinkScope(previous);
        }

        private sealed class SinkScope : IDisposable
        {
            private readonly RackDiagnosticsLog previous;
            private bool restored;

            public SinkScope(RackDiagnosticsLog previous)
            {
                this.previous = previous;
            }

            public void Dispose()
            {
                if (restored)
                {
                    return;
                }

                restored = true;
                sink = previous;
            }
        }

        private static string ResolveDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RackCad", "logs");
        }
    }
}
