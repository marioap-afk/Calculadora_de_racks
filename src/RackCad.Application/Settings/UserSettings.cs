using System;
using System.IO;
using System.Text.Json;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;

namespace RackCad.Application.Settings
{
    /// <summary>Per-user, persisted preferences (not part of a project). Stored as JSON under %APPDATA%\RackCad.</summary>
    public sealed class UserSettings
    {
        /// <summary>Override path to the block library DWG; null/empty = use the default next to the catalogs.</summary>
        public string BlockLibraryPath { get; set; }

        /// <summary>Folder for the design library (named .rackcad.json designs); null/empty = the default under %APPDATA%\RackCad\Designs.</summary>
        public string DesignLibraryPath { get; set; }
    }

    /// <summary>Loads/saves <see cref="UserSettings"/> as a best-effort JSON file; failures fall back to defaults.</summary>
    public static class UserSettingsStore
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static string SettingsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RackCad", "settings.json");

        /// <summary>The configured design-library folder, or the default (%APPDATA%\RackCad\Designs) when unset.</summary>
        public static string ResolveDesignLibraryPath(UserSettings settings)
        {
            var configured = settings?.DesignLibraryPath;
            if (!string.IsNullOrWhiteSpace(configured))
            {
                return configured;
            }

            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RackCad", "Designs");
        }

        public static UserSettings Load() => Load(SettingsPath);

        /// <summary>
        /// I-03 (D2): load from an explicit path, DISTINGUISHING a missing file from a present-but-unreadable
        /// one by the EXCEPTION the read throws (not a prior <c>File.Exists</c>, which reports false for a
        /// permission-denied file and would hide it as "missing"):
        /// <list type="bullet">
        /// <item><see cref="FileNotFoundException"/>/<see cref="DirectoryNotFoundException"/> — absent: silent defaults (normal).</item>
        /// <item><see cref="JsonException"/> — corrupt content: quarantine to <c>.bad</c> + log with stack, then defaults.</item>
        /// <item>any other read failure (permissions/IO/lock) — log with stack, NO quarantine, then defaults.</item>
        /// </list>
        /// An empty/whitespace file stays a silent default (unchanged). Never throws. Internal so tests can point
        /// it at a temp file instead of the real %APPDATA%.
        /// </summary>
        internal static UserSettings Load(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return string.IsNullOrWhiteSpace(json)
                    ? new UserSettings()
                    : JsonSerializer.Deserialize<UserSettings>(json, Options) ?? new UserSettings();
            }
            catch (FileNotFoundException)
            {
                return new UserSettings(); // absent → defaults, silently (this is normal, not a failure)
            }
            catch (DirectoryNotFoundException)
            {
                return new UserSettings(); // absent (its folder does not exist) → defaults, silently
            }
            catch (JsonException ex)
            {
                // Present but corrupt content: preserve it (.bad) and log, so the reset is diagnosable — not silent.
                CorruptFile.Quarantine(path, "UserSettings load", ex);
                return new UserSettings();
            }
            catch (Exception ex)
            {
                // Present but unreadable (permissions / IO / lock): log — but do NOT quarantine (the file is not
                // corrupt and may be fine next time). Never break the flow. Distinct from the "missing" cases above.
                RackLog.Exception("UserSettings load (" + path + ")", ex);
                return new UserSettings();
            }
        }

        public static void Save(UserSettings settings) => Save(settings, SettingsPath);

        /// <summary>
        /// I-03 (D2): save best-effort but ATOMICALLY (temp + File.Replace, via <see cref="AtomicFile"/>) so an
        /// interrupted write cannot destroy the previous settings, and log the failure instead of swallowing it
        /// in complete silence. Internal path overload so tests avoid the real %APPDATA%.
        /// </summary>
        internal static void Save(UserSettings settings, string path)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                AtomicFile.WriteAllText(path, JsonSerializer.Serialize(settings ?? new UserSettings(), Options));
            }
            catch (Exception ex)
            {
                // Settings are best-effort; never let a save failure break the flow — but no longer silently.
                RackLog.Exception("UserSettings save (" + path + ")", ex);
            }
        }
    }
}
