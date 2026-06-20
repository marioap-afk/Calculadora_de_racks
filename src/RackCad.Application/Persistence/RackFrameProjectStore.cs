using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Saves and loads a configuration as a standalone JSON project file (not a DWG and not
    /// AutoCAD metadata). Derived members are rebuilt on load so the model is always consistent
    /// with the persisted source of truth, even if the file was hand-edited.
    /// </summary>
    public sealed class RackFrameProjectStore
    {
        public const string FileExtension = ".rackcad.json";

        private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();

        private readonly BracingPanelMemberBuilder builder = new BracingPanelMemberBuilder();

        public string Serialize(RackFrameConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return JsonSerializer.Serialize(RackFrameProjectDocument.FromConfiguration(configuration), SerializerOptions);
        }

        public RackFrameConfiguration Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("El proyecto esta vacio.");
            }

            RackFrameProjectDocument document;

            try
            {
                document = JsonSerializer.Deserialize<RackFrameProjectDocument>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("El proyecto no es un JSON valido: " + ex.Message, ex);
            }

            if (document == null)
            {
                throw new InvalidOperationException("El proyecto esta vacio o es invalido.");
            }

            var configuration = document.ToConfiguration();
            builder.RefreshPhysicalModel(configuration);
            return configuration;
        }

        public void Save(RackFrameConfiguration configuration, string path)
        {
            File.WriteAllText(path, Serialize(configuration));
        }

        public RackFrameConfiguration Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("No se encontro el proyecto.", path);
            }

            return Deserialize(File.ReadAllText(path));
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

            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}
