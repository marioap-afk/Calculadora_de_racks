using System;
using System.IO;

namespace RackCad.Application.Diagnostics
{
    /// <summary>
    /// I-03 (D2): when a best-effort store finds a file that is PRESENT but UNREADABLE (corrupt content) —
    /// as opposed to simply MISSING — record the failure with its stack trace and move the file aside to
    /// <c>&lt;path&gt;.bad</c>. This keeps the user's data (so a later save does not silently overwrite it)
    /// and makes the failure diagnosable, instead of resetting to defaults in silence. Best-effort: never throws.
    /// </summary>
    internal static class CorruptFile
    {
        public static void Quarantine(string path, string context, Exception ex)
        {
            RackLog.Exception((context ?? "-") + " (archivo ilegible: " + path + ")", ex);

            try
            {
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    return;
                }

                var bad = path + ".bad";
                if (File.Exists(bad))
                {
                    File.Delete(bad);
                }

                File.Move(path, bad);
            }
            catch
            {
                // Best-effort: if the file cannot be moved aside, the log entry already made the failure
                // diagnosable — that is the point of I-03. Never throw into the caller's flow.
            }
        }
    }
}
