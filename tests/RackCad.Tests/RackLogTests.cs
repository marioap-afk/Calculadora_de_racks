using System;
using System.IO;
using System.Linq;
using RackCad.Application.Diagnostics;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-03: the minimal diagnostics logger. Covers the PURE formatter (stack trace + context are present),
    /// the real file writer (a temp directory, never touching %AppData%), the best-effort contract (never
    /// throws, even on an unwritable directory), and the public façade (shape + a REDIRECTED write, so no test
    /// touches the real %AppData%). In the "RackLog" collection because the façade tests swap the static sink.
    /// </summary>
    [Collection("RackLog")]
    public class RackLogTests
    {
        /// <summary>An exception that actually carries a stack trace (thrown + caught), like a real failure.</summary>
        private static Exception Thrown(string message)
        {
            try
            {
                throw new InvalidOperationException(message);
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        [Fact]
        public void Formatter_Exception_ContainsLevelContextTypeMessageAndStackTrace()
        {
            var stamp = new DateTime(2026, 7, 22, 13, 45, 12, DateTimeKind.Utc);
            var entry = RackLogFormatter.Format(stamp, "ERROR", "RackCad command", Thrown("boom-42"));

            Assert.Contains("ERROR", entry);
            Assert.Contains("RackCad command", entry);
            Assert.Contains("InvalidOperationException", entry); // the exception TYPE
            Assert.Contains("boom-42", entry);                   // the message
            Assert.Contains("   at ", entry);                    // the stack trace (the whole point of I-03)
            Assert.Contains("2026-07-22", entry);                // the timestamp
        }

        [Fact]
        public void Formatter_Warning_ContainsLevelContextAndMessage_NoThrow()
        {
            var stamp = new DateTime(2026, 7, 22, 13, 45, 12, DateTimeKind.Utc);
            var entry = RackLogFormatter.Format(stamp, "WARN", "Catalogo", "El catalogo se cargo vacio");

            Assert.Contains("WARN", entry);
            Assert.Contains("Catalogo", entry);
            Assert.Contains("El catalogo se cargo vacio", entry);
        }

        [Fact]
        public void Formatter_NullContextAndException_DoesNotThrow()
        {
            var stamp = new DateTime(2026, 7, 22, 13, 45, 12, DateTimeKind.Utc);
            var entry = RackLogFormatter.Format(stamp, "ERROR", null, (Exception)null);
            Assert.False(string.IsNullOrEmpty(entry));
        }

        [Fact]
        public void DiagnosticsLog_Exception_WritesFileWithMessageAndStackTrace()
        {
            var dir = Path.Combine(Path.GetTempPath(), "rackcad-log-" + Guid.NewGuid().ToString("N"));
            try
            {
                var log = new RackDiagnosticsLog(dir);
                log.Exception("Carga de catalogo", Thrown("catalogo-roto-777"));

                var files = Directory.EnumerateFiles(dir, "*.log").ToList();
                Assert.Single(files);
                var text = File.ReadAllText(files[0]);
                Assert.Contains("Carga de catalogo", text);
                Assert.Contains("catalogo-roto-777", text);
                Assert.Contains("   at ", text); // stack trace persisted to disk
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, recursive: true);
                }
            }
        }

        [Fact]
        public void DiagnosticsLog_MultipleEntries_AreAppended()
        {
            var dir = Path.Combine(Path.GetTempPath(), "rackcad-log-" + Guid.NewGuid().ToString("N"));
            try
            {
                var log = new RackDiagnosticsLog(dir);
                log.Warning("A", "primero-abc");
                log.Exception("B", Thrown("segundo-xyz"));

                var text = string.Concat(Directory.EnumerateFiles(dir, "*.log").Select(File.ReadAllText));
                Assert.Contains("primero-abc", text);
                Assert.Contains("segundo-xyz", text);
            }
            finally
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, recursive: true);
                }
            }
        }

        [Fact]
        public void DiagnosticsLog_UnwritableDirectory_NeverThrows()
        {
            // A FILE where the log expects a directory: creating the log dir under it fails. Best-effort
            // logging must swallow that (a logging failure must never break the caller's flow).
            var blocker = Path.Combine(Path.GetTempPath(), "rackcad-block-" + Guid.NewGuid().ToString("N"));
            File.WriteAllText(blocker, "x");
            try
            {
                var log = new RackDiagnosticsLog(Path.Combine(blocker, "logs"));
                var boom = Record.Exception(() => log.Exception("ctx", Thrown("nope")));
                Assert.Null(boom);
            }
            finally
            {
                if (File.Exists(blocker))
                {
                    File.Delete(blocker);
                }
            }
        }

        [Fact]
        public void Facade_LogDirectory_IsUnderRackCadLogs()
        {
            // Reads the property only — no write, so this never touches the real %AppData%.
            Assert.Contains("RackCad", RackLog.LogDirectory);
            Assert.Contains("logs", RackLog.LogDirectory);
        }

        [Fact]
        public void Facade_UnderRedirect_WritesToTempAndNeverThrows()
        {
            // The façade is used by Report() and the 14 catches, so it must never throw. Redirect the sink to a
            // temp folder so this real write never lands in the production %AppData%\RackCad\logs.
            using var capture = LogCapture.Begin();

            var boom = Record.Exception(() =>
            {
                RackLog.Exception("facade-test", Thrown("facade-boom"));
                RackLog.Warning("facade-test", "facade-warn");
            });

            Assert.Null(boom);
            Assert.Contains("facade-boom", capture.Text);
            Assert.Contains("facade-warn", capture.Text);
        }
    }
}
