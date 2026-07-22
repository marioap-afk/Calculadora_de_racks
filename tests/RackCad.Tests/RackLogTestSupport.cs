using System;
using System.IO;
using System.Linq;
using RackCad.Application.Diagnostics;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// All tests that touch the process-wide <see cref="RackLog"/> sink share this collection so they run
    /// SERIALIZED (parallelization disabled): the sink is a single static field, so two classes swapping it
    /// concurrently would race. Membership also guarantees none of them writes to the real %AppData% while
    /// another asserts on the production folder.
    /// </summary>
    [CollectionDefinition("RackLog", DisableParallelization = true)]
    public sealed class RackLogCollection
    {
    }

    /// <summary>
    /// Redirects <see cref="RackLog"/> to a fresh temp directory for the scope (so real %AppData% is never
    /// touched) and exposes what was written. Dispose restores the previous sink and deletes the temp dir.
    /// This is the minimal internal seam the negative tests use to observe logging deterministically.
    /// </summary>
    internal sealed class LogCapture : IDisposable
    {
        private readonly IDisposable redirect;

        private LogCapture(string directory, IDisposable redirect)
        {
            Directory = directory;
            this.redirect = redirect;
        }

        public string Directory { get; }

        public static LogCapture Begin()
        {
            var dir = Path.Combine(Path.GetTempPath(), "rackcad-logcap-" + Guid.NewGuid().ToString("N"));
            return new LogCapture(dir, RackLog.RedirectForTests(dir));
        }

        /// <summary>All text written to the captured log folder (empty if nothing was logged).</summary>
        public string Text =>
            System.IO.Directory.Exists(Directory)
                ? string.Concat(System.IO.Directory.EnumerateFiles(Directory, "*.log").Select(File.ReadAllText))
                : string.Empty;

        public void Dispose()
        {
            redirect.Dispose();
            try
            {
                if (System.IO.Directory.Exists(Directory))
                {
                    System.IO.Directory.Delete(Directory, recursive: true);
                }
            }
            catch
            {
                // Best-effort cleanup of the temp capture folder.
            }
        }
    }
}
