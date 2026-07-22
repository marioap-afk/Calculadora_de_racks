using System;
using System.IO;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// I-03 (D2): write text atomically so an interrupted save cannot leave a half-written file that
    /// destroys the previous good copy. The bytes go to a temp file in the SAME directory (same volume, so
    /// the swap is atomic), then <see cref="File.Replace(string,string,string)"/> over an existing target,
    /// or <see cref="File.Move(string,string)"/> into a new one. It deliberately does NOT create the target
    /// directory: each store keeps its own directory precondition unchanged, and a real IO error still
    /// propagates (only the ATOMICITY is added, not new swallowing).
    /// </summary>
    internal static class AtomicFile
    {
        public static void WriteAllText(string path, string contents)
        {
            var directory = Path.GetDirectoryName(path);
            var temp = Path.Combine(
                string.IsNullOrEmpty(directory) ? "." : directory,
                Path.GetFileName(path) + ".tmp-" + Guid.NewGuid().ToString("N"));

            try
            {
                File.WriteAllText(temp, contents);

                if (File.Exists(path))
                {
                    // Atomic replace of the existing file (no backup file requested).
                    File.Replace(temp, path, null);
                }
                else
                {
                    File.Move(temp, path);
                }
            }
            catch
            {
                TryDelete(temp);
                throw;
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Ignore: the temp is orphaned at worst, never the target.
            }
        }
    }
}
