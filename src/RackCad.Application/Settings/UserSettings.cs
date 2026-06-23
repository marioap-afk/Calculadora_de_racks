using System;
using System.IO;
using System.Text.Json;

namespace RackCad.Application.Settings
{
    /// <summary>Per-user, persisted preferences (not part of a project). Stored as JSON under %APPDATA%\RackCad.</summary>
    public sealed class UserSettings
    {
        /// <summary>Override path to the block library DWG; null/empty = use the default next to the catalogs.</summary>
        public string BlockLibraryPath { get; set; }
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

        public static UserSettings Load()
        {
            try
            {
                var path = SettingsPath;
                if (!File.Exists(path))
                {
                    return new UserSettings();
                }

                var json = File.ReadAllText(path);
                return string.IsNullOrWhiteSpace(json)
                    ? new UserSettings()
                    : JsonSerializer.Deserialize<UserSettings>(json, Options) ?? new UserSettings();
            }
            catch
            {
                return new UserSettings();
            }
        }

        public static void Save(UserSettings settings)
        {
            try
            {
                var path = SettingsPath;
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, JsonSerializer.Serialize(settings ?? new UserSettings(), Options));
            }
            catch
            {
                // Settings are best-effort; never let a save failure break the flow.
            }
        }
    }
}
