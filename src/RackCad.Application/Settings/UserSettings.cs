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
        /// I-03 (D2): load from an explicit path, DISTINGUISHING a missing file (silent defaults — normal)
        /// from a present-but-unreadable one. A corrupt file is quarantined to <c>.bad</c> and logged with a
        /// stack trace instead of silently resetting to defaults (which used to lose the user's library path
        /// without a trace); any other IO failure is logged but the file is left untouched. Never throws.
        /// Internal so tests can point it at a temp file instead of the real %APPDATA%.
        /// </summary>
        internal static UserSettings Load(string path)
        {
            if (!File.Exists(path))
            {
                return new UserSettings(); // missing → defaults, silently (this is normal, not a failure)
            }

            try
            {
                var json = File.ReadAllText(path);
                return string.IsNullOrWhiteSpace(json)
                    ? new UserSettings()
                    : JsonSerializer.Deserialize<UserSettings>(json, Options) ?? new UserSettings();
            }
            catch (JsonException ex)
            {
                // Corrupt content: preserve it (.bad) and log, so the reset is diagnosable — not silent.
                CorruptFile.Quarantine(path, "UserSettings load", ex);
                return new UserSettings();
            }
            catch (Exception ex)
            {
                // Other IO failure (e.g. a transient lock): log, but leave the file (it may be fine next time).
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
