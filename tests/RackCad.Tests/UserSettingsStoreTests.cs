using System;
using System.IO;
using System.Linq;
using RackCad.Application.Settings;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-03 (D2): settings load must distinguish a MISSING file (silent defaults — normal) from a
    /// PRESENT-but-UNREADABLE one (the corrupt file silently reset defaults, losing the user's library
    /// path without a trace). Now an unreadable file is quarantined to <c>.bad</c> and logged, and the
    /// save is atomic. Tests use internal path overloads so they never touch the real %APPDATA%; the
    /// corrupt-file test also redirects the log sink (RackLog collection) so logging stays off %APPDATA% too.
    /// </summary>
    [Collection("RackLog")]
    public class UserSettingsStoreTests
    {
        private static string TempPath()
            => Path.Combine(Path.GetTempPath(), "rackcad-settings-" + Guid.NewGuid().ToString("N") + ".json");

        [Fact]
        public void Load_MissingFile_ReturnsDefaults_WithoutQuarantine()
        {
            var path = TempPath();
            var settings = UserSettingsStore.Load(path);

            Assert.NotNull(settings);
            Assert.Null(settings.BlockLibraryPath);
            Assert.Null(settings.DesignLibraryPath);
            Assert.False(File.Exists(path + ".bad")); // a missing file is normal, not quarantined
        }

        [Fact]
        public void Load_CorruptFile_QuarantinesToBad_AndReturnsDefaults()
        {
            var path = TempPath();
            File.WriteAllText(path, "{ this is not valid json ][");
            using var cap = LogCapture.Begin(); // the corrupt load logs; keep it off the real %AppData%
            try
            {
                var settings = UserSettingsStore.Load(path);

                Assert.NotNull(settings);
                Assert.Null(settings.BlockLibraryPath); // reset to defaults, but NOT silently...
                Assert.False(File.Exists(path));        // ...the corrupt file was moved aside...
                Assert.True(File.Exists(path + ".bad")); // ...to a .bad quarantine so the data survives.
                Assert.Contains("not valid json", File.ReadAllText(path + ".bad"));
                Assert.Contains("UserSettings load", cap.Text); // and the reset was recorded, not silent
            }
            finally
            {
                foreach (var p in new[] { path, path + ".bad" })
                {
                    if (File.Exists(p)) File.Delete(p);
                }
            }
        }

        [Fact]
        public void SaveThenLoad_RoundTripsPaths()
        {
            var path = TempPath();
            try
            {
                UserSettingsStore.Save(
                    new UserSettings { BlockLibraryPath = @"C:\lib\blocks.dwg", DesignLibraryPath = @"C:\designs" },
                    path);

                var loaded = UserSettingsStore.Load(path);
                Assert.Equal(@"C:\lib\blocks.dwg", loaded.BlockLibraryPath);
                Assert.Equal(@"C:\designs", loaded.DesignLibraryPath);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void Save_IsAtomic_NoTempSiblingLeft()
        {
            var dir = Path.Combine(Path.GetTempPath(), "rackcad-settings-dir-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            try
            {
                var path = Path.Combine(dir, "settings.json");
                UserSettingsStore.Save(new UserSettings { BlockLibraryPath = "x" }, path);
                UserSettingsStore.Save(new UserSettings { BlockLibraryPath = "y" }, path);

                var files = Directory.EnumerateFiles(dir).Select(Path.GetFileName).ToList();
                Assert.Equal(new[] { "settings.json" }, files);
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }
    }
}
