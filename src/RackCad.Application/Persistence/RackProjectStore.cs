using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Saves and loads a project that can be any registered rack system, using a top-level <c>Kind</c> discriminator. The
    /// per-kind write / build / validate logic lives in the <see cref="SystemRegistry"/> descriptors, not in a switch here
    /// (initiative I-08). Backward compatible: a legacy file with no <c>kind</c> (a bare header, schema 1.0) loads as a
    /// selective project, and a wrapper whose <c>Kind</c> is not a registered system (e.g. an undefined enum number)
    /// falls back to the historical header/Selective path. Derived header members are rebuilt on load.
    /// </summary>
    public sealed class RackProjectStore
    {
        public const string FileExtension = ".rackcad.json";

        private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();

        private readonly BracingPanelMemberBuilder builder = new BracingPanelMemberBuilder();
        private readonly SystemRegistry registry;

        public RackProjectStore()
            : this(SystemRegistry.Default)
        {
        }

        /// <summary>
        /// Uses <paramref name="registry"/> for per-kind dispatch. Production uses <see cref="SystemRegistry.Default"/>;
        /// the overload is a seam that lets a test prove dispatch is registry-driven (there is no hard-coded kind switch).
        /// </summary>
        public RackProjectStore(SystemRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public string Serialize(RackProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var document = new RackProjectDocument { Kind = project.Kind };

            // The kind's descriptor writes its own payload; a kind with no payload present — and any unregistered kind —
            // falls back to the historical bare-header path, re-stamping Kind = Selective.
            if (!(registry.TryGet(project.Kind, out var descriptor) && descriptor.TryWritePayload(project, document)))
            {
                document.Kind = RackSystemKind.Selective;
                document.Header = project.Header == null ? null : RackFrameProjectDocument.FromConfiguration(project.Header);
            }

            return JsonSerializer.Serialize(document, SerializerOptions);
        }

        public RackProject Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new InvalidOperationException("El proyecto esta vacio.");
            }

            bool isWrapper;

            try
            {
                using var probe = JsonDocument.Parse(json);
                isWrapper = LooksLikeWrapper(probe.RootElement);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("El proyecto no es un JSON valido: " + ex.Message, ex);
            }

            return isWrapper ? DeserializeWrapper(json) : DeserializeLegacyHeader(json);
        }

        public void Save(RackProject project, string path)
        {
            File.WriteAllText(path, Serialize(project));
        }

        public RackProject Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("No se encontro el proyecto.", path);
            }

            return Deserialize(File.ReadAllText(path));
        }

        private RackProject DeserializeWrapper(string json)
        {
            RackProjectDocument document;

            try
            {
                document = JsonSerializer.Deserialize<RackProjectDocument>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("El proyecto no es un JSON valido: " + ex.Message, ex);
            }

            if (document == null)
            {
                throw new InvalidOperationException("El proyecto esta vacio o es invalido.");
            }

            SchemaGuard.CheckReadable(document.SchemaVersion, RackProjectDocument.CurrentSchemaVersion, "El proyecto");

            var project = BuildProject(document);
            ValidateProject(project);
            return project;
        }

        private RackProject BuildProject(RackProjectDocument document)
        {
            if (registry.TryGet(document.Kind, out var descriptor))
            {
                return descriptor.Build(document, builder);
            }

            // Unregistered / undefined numeric Kind (e.g. (RackSystemKind)999): exactly the historical header/Selective
            // fallback. Get(Selective) is always registered, and its Build is the header path.
            return registry.Get(RackSystemKind.Selective).Build(document, builder);
        }

        private RackProject DeserializeLegacyHeader(string json)
        {
            RackFrameProjectDocument document;

            try
            {
                document = JsonSerializer.Deserialize<RackFrameProjectDocument>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("El proyecto no es un JSON valido: " + ex.Message, ex);
            }

            SchemaGuard.CheckReadable(document?.SchemaVersion, RackFrameProjectDocument.CurrentSchemaVersion, "La cabecera");

            var header = document?.ToConfiguration();
            if (!RackDesignValidation.IsUsableHeader(header))
            {
                throw Degenerate("la cabecera");
            }

            builder.RefreshPhysicalModel(header);
            return RackProject.ForSelective(header);
        }

        /// <summary>
        /// Reject a semantically-empty project (e.g. <c>{}</c> → a header with height 0) instead of returning it. The
        /// per-kind predicate and grammatical noun are selected via the descriptor, not a switch; <see cref="Get"/> is
        /// safe here because a built project's <see cref="RackProject.Kind"/> is always a registered kind.
        /// </summary>
        private void ValidateProject(RackProject project)
        {
            var descriptor = registry.Get(project.Kind);
            if (!descriptor.IsUsable(project))
            {
                throw Degenerate(descriptor.ValidationNoun);
            }
        }

        private static InvalidOperationException Degenerate(string what)
            => new InvalidOperationException("El proyecto no tiene datos válidos para " + what + " (¿archivo vacío o incompleto?).");

        private static bool LooksLikeWrapper(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (var property in root.EnumerateObject())
            {
                if (string.Equals(property.Name, "kind", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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
