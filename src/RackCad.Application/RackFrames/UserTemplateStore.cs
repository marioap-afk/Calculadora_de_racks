using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Per-user template library persisted as JSON under <c>%APPDATA%\RackCad\user-templates.json</c> — a
    /// WRITABLE location, unlike the shared <c>header-templates.json</c> that ships next to the DLL. Loads are
    /// best-effort (empty on a missing or corrupt file); <see cref="Save"/> UPSERTs by id and only throws on a
    /// real IO failure so the UI can surface it. Mirrors <c>UserSettingsStore</c> for the writable path.
    /// </summary>
    public sealed class UserTemplateStore
    {
        public const string TemplatesFile = "user-templates.json";

        private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();

        private readonly string path;

        public UserTemplateStore(string path)
        {
            this.path = path ?? throw new ArgumentNullException(nameof(path));
        }

        /// <summary>Default writable location: <c>%APPDATA%\RackCad\user-templates.json</c> (mirrors <c>UserSettingsStore.SettingsPath</c>).</summary>
        public static string DefaultPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RackCad", TemplatesFile);

        /// <summary>
        /// Loads the user templates; a MISSING or empty file yields an empty list silently (normal). A
        /// present-but-CORRUPT file is quarantined to <c>.bad</c> and logged (I-03 D2) instead of being
        /// silently discarded, so the user's library is preserved and the failure is diagnosable. Never throws.
        /// </summary>
        public IReadOnlyList<RackFrameTemplate> Load()
        {
            if (!File.Exists(path))
            {
                return new List<RackFrameTemplate>(); // missing → empty, silently (normal, not a failure)
            }

            try
            {
                var json = File.ReadAllText(path);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<RackFrameTemplate>();
                }

                return JsonSerializer.Deserialize<List<RackFrameTemplate>>(json, SerializerOptions)
                       ?? new List<RackFrameTemplate>();
            }
            catch (JsonException ex)
            {
                // Corrupt content: preserve it (.bad) and log instead of silently dropping the library.
                CorruptFile.Quarantine(path, "UserTemplateStore load", ex);
                return new List<RackFrameTemplate>();
            }
            catch (Exception ex)
            {
                // Other IO failure (e.g. a transient lock): log, but leave the file (it may be fine next time).
                RackLog.Exception("UserTemplateStore load (" + path + ")", ex);
                return new List<RackFrameTemplate>();
            }
        }

        /// <summary>
        /// Adds or replaces a template by id (<see cref="StringComparison.OrdinalIgnoreCase"/>) — updating in
        /// place so re-saving the same id keeps a single entry. Creates the folder and writes indented JSON.
        /// Only a genuine IO error propagates (so the UI can report a failed save).
        /// </summary>
        public void Save(RackFrameTemplate template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            var templates = new List<RackFrameTemplate>(Load());
            var index = templates.FindIndex(existing =>
                existing != null && string.Equals(existing.Id, template.Id, StringComparison.OrdinalIgnoreCase));

            if (index >= 0)
            {
                templates[index] = template;
            }
            else
            {
                templates.Add(template);
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // I-03 (D2): atomic write so an interrupted save cannot corrupt the existing template library.
            AtomicFile.WriteAllText(path, JsonSerializer.Serialize(templates, SerializerOptions));
        }

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            // BracingPattern is an enum: serialize it by name (same as RackFrameTemplateProvider) so the file
            // stays human-editable and round-trips with the shared header-templates.json.
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}
