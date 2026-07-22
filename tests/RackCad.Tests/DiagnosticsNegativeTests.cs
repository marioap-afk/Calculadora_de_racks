using System;
using System.Collections.Generic;
using System.IO;
using RackCad.Application.Diagnostics;
using RackCad.Application.Settings;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-03 corrective negatives. Each asserts a diagnosable outcome that the PRE-FIX code got wrong:
    /// a read error other than "missing" was silently treated as absence (no log); a failed <c>.bad</c> move
    /// was swallowed with no trace. They observe logging through the minimal <see cref="RackLog"/> redirect
    /// seam, so they are deterministic and never write to the real %AppData%.
    /// </summary>
    [Collection("RackLog")]
    public class DiagnosticsNegativeTests
    {
        [Fact]
        public void UserSettingsStore_ReadErrorOtherThanMissing_IsLoggedNotTreatedAsAbsence()
        {
            // A path that EXISTS but is not a readable file (a directory) makes File.ReadAllText throw a
            // non-missing error. The old File.Exists gate misclassified this as "absent" and returned defaults
            // SILENTLY; now it must be logged — and NOT quarantined, because the file is not corrupt.
            var dirAsPath = Path.Combine(Path.GetTempPath(), "rackcad-dir-as-file-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dirAsPath);
            using var cap = LogCapture.Begin();
            try
            {
                var settings = UserSettingsStore.Load(dirAsPath);

                Assert.NotNull(settings);                        // best-effort default, never throws
                Assert.Null(settings.BlockLibraryPath);
                Assert.Contains("UserSettings load", cap.Text);  // LOGGED — not silently classified as absence
                Assert.False(File.Exists(dirAsPath + ".bad"));   // a read error is not corruption → no quarantine
            }
            finally
            {
                if (Directory.Exists(dirAsPath))
                {
                    Directory.Delete(dirAsPath, recursive: true);
                }
            }
        }

        [Fact]
        public void CorruptFile_QuarantineFailure_IsLoggedAndDoesNotThrow()
        {
            // Force the .bad move to fail: make "<path>.bad" an existing DIRECTORY so File.Move cannot create it.
            var path = Path.Combine(Path.GetTempPath(), "rackcad-qfail-" + Guid.NewGuid().ToString("N") + ".json");
            File.WriteAllText(path, "corrupt-payload");
            var badDir = path + ".bad";
            Directory.CreateDirectory(badDir);
            using var cap = LogCapture.Begin();
            try
            {
                var boom = Record.Exception(() =>
                    CorruptFile.Quarantine(path, "UnitUnderTest", new InvalidOperationException("primary")));

                Assert.Null(boom);                                          // never propagates (best-effort)
                Assert.True(File.Exists(path));                             // the move failed → original stays
                Assert.Contains("fallo al poner en cuarentena", cap.Text);  // the SECONDARY failure was logged
            }
            finally
            {
                if (Directory.Exists(badDir))
                {
                    Directory.Delete(badDir, recursive: true);
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void Logging_UnderRedirect_DoesNotWriteToProductionLogDirectory()
        {
            var production = RackLog.LogDirectory;
            var before = SnapshotFiles(production);

            using (var cap = LogCapture.Begin())
            {
                RackLog.Exception("prod-guard", new InvalidOperationException("x"));
                RackLog.Warning("prod-guard", "y");
                Assert.Contains("prod-guard", cap.Text); // the write landed in the redirected temp folder...
            }

            Assert.Equal(before, SnapshotFiles(production)); // ...and NOT in the real %AppData%\RackCad\logs
        }

        private static HashSet<string> SnapshotFiles(string directory)
        {
            return Directory.Exists(directory)
                ? new HashSet<string>(Directory.EnumerateFiles(directory))
                : new HashSet<string>();
        }
    }
}
