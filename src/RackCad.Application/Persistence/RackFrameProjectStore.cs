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
        /// Deep-clone a configuration — the SINGLE canonical clone every editor shares (initiative I-17). The
        /// PERSISTED model (metadata, posts, plates, horizontals, panels) is copied by round-tripping through
        /// this store's JSON document, so it is owned by the persistence schema and a new persisted field is
        /// preserved the moment it is added to <see cref="RackFrameProjectDocument"/> (the audit's U4). The
        /// DERIVED model (<see cref="RackFrameConfiguration.Members"/>, per-panel members and panel elevations)
        /// is rebuilt on load by <see cref="Deserialize"/>, exactly as for a project opened from disk.
        /// <para>
        /// The document deliberately does NOT persist <see cref="RackFrameConfiguration.Exceptions"/> (the
        /// override audit trail is runtime-only — I-17 must not change the wire format), and
        /// <see cref="Deserialize"/> does NOT rebuild it (it is not a derived geometric artifact). A faithful
        /// clone must still carry that runtime-only state, so it is re-attached here — inside the single
        /// canonical mechanism — as an independent deep copy, without changing what Save/Load write to disk.
        /// </para>
        /// Returns null for a null input, so it is a drop-in for the historical UI clone helpers.
        /// </summary>
        public RackFrameConfiguration DeepCopy(RackFrameConfiguration configuration)
        {
            if (configuration == null)
            {
                return null;
            }

            var clone = Deserialize(Serialize(configuration));

            // Runtime-only overrides omitted by the document AND not rebuilt by RefreshPhysicalModel: carry them
            // across so the canonical clone is complete (they never reach the persisted wire form).
            foreach (var exception in configuration.Exceptions)
            {
                clone.Exceptions.Add(CloneException(exception));
            }

            return clone;
        }

        private static FrameExceptionOverride CloneException(FrameExceptionOverride source)
        {
            return source == null ? null : new FrameExceptionOverride
            {
                ExceptionType = source.ExceptionType,
                TargetId = source.TargetId,
                StandardValue = source.StandardValue,
                OverrideValue = source.OverrideValue,
                Reason = source.Reason
            };
        }

        public void Save(RackFrameConfiguration configuration, string path)
        {
            // I-03 (D2): atomic write (temp + File.Replace) so an interrupted save cannot leave a half-written
            // project that destroys the previous good copy. Load already distinguishes missing from unreadable.
            AtomicFile.WriteAllText(path, Serialize(configuration));
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
