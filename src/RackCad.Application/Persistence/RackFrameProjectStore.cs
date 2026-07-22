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

            // Same guards as RackProjectStore's cabecera branch: a file written by a NEWER major schema is
            // rejected with a clear message (instead of loading half-understood), and a degenerate document
            // ({} used to load as a 0-height cabecera in silence) is refused instead of drawn.
            SchemaGuard.CheckReadable(document.SchemaVersion, RackFrameProjectDocument.CurrentSchemaVersion, "El proyecto");

            var configuration = document.ToConfiguration();
            if (!RackDesignValidation.IsUsableHeader(configuration))
            {
                throw new InvalidOperationException("El proyecto no contiene una cabecera usable (alto, fondo y postes).");
            }

            builder.RefreshPhysicalModel(configuration);
            return configuration;
        }

        /// <summary>
        /// Deep-clone a configuration by round-tripping it through this store's JSON document — the SINGLE
        /// canonical clone every editor shares (initiative I-17). Because the copy flows through
        /// <see cref="Serialize"/> + <see cref="Deserialize"/>, the source of truth (metadata, posts, plates,
        /// horizontals, panels) is copied by the persistence schema itself and the derived model (Members,
        /// panel elevations) is REBUILT on load exactly as for a project opened from disk. A new configuration
        /// field is therefore preserved the moment it is added to <see cref="RackFrameProjectDocument"/>, with
        /// no hand-maintained per-field clone to drift out of sync (the audit's U4). Returns null for a null
        /// input, so it is a drop-in for the historical UI clone helpers.
        /// </summary>
        public RackFrameConfiguration DeepCopy(RackFrameConfiguration configuration)
        {
            return configuration == null ? null : Deserialize(Serialize(configuration));
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
