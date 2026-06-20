using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Loads header templates from a versioned <c>header-templates.json</c> file. A missing
    /// or empty file falls back to <see cref="RackFrameTemplateCatalog.All"/> so the
    /// configurator always has templates; a malformed file throws a descriptive error so the
    /// editor can surface it instead of silently ignoring the edits.
    /// </summary>
    public sealed class RackFrameTemplateProvider
    {
        public const string TemplatesFile = "header-templates.json";

        private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();

        private readonly string directory;

        public RackFrameTemplateProvider(string directory)
        {
            this.directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        /// <summary>Points at the <c>catalogs</c> folder next to the executing assembly.</summary>
        public static RackFrameTemplateProvider FromBaseDirectory()
        {
            return new RackFrameTemplateProvider(Path.Combine(AppContext.BaseDirectory, "catalogs"));
        }

        public IReadOnlyList<RackFrameTemplate> Load()
        {
            var path = Path.Combine(directory, TemplatesFile);

            if (!File.Exists(path))
            {
                return RackFrameTemplateCatalog.All;
            }

            var json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                return RackFrameTemplateCatalog.All;
            }

            List<RackFrameTemplate> templates;

            try
            {
                templates = JsonSerializer.Deserialize<List<RackFrameTemplate>>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "El catalogo de plantillas '" + TemplatesFile + "' no es JSON valido: " + ex.Message, ex);
            }

            return templates == null || templates.Count == 0
                ? RackFrameTemplateCatalog.All
                : templates;
        }

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            };

            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}
