using System;
using System.Globalization;
using System.IO;

namespace RackCad.Application.Diagnostics
{
    /// <summary>
    /// Writes minimal diagnostics to a daily file (<c>rackcad-yyyyMMdd.log</c>) under a directory.
    /// BEST-EFFORT by contract: every failure is swallowed, because a logging failure must never break the
    /// caller's flow. Thread-safe (writes are serialized on a private lock). Production points it at
    /// <c>%AppData%\RackCad\logs</c> via <see cref="RackLog"/>; tests point it at a temporary directory.
    /// </summary>
    internal sealed class RackDiagnosticsLog
    {
        private readonly object gate = new object();
        private readonly string directory;

        public RackDiagnosticsLog(string directory)
        {
            this.directory = directory;
        }

        public void Exception(string context, Exception ex)
        {
            Write(RackLogFormatter.Format(DateTime.UtcNow, "ERROR", context, ex));
        }

        public void Warning(string context, string message)
        {
            Write(RackLogFormatter.Format(DateTime.UtcNow, "WARN", context, message));
        }

        private void Write(string entry)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            try
            {
                lock (gate)
                {
                    Directory.CreateDirectory(directory);
                    var file = Path.Combine(
                        directory,
                        "rackcad-" + DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + ".log");
                    File.AppendAllText(file, entry);
                }
            }
            catch
            {
                // Best-effort: diagnostics must never throw into the caller's flow.
            }
        }
    }
}
