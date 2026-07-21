using System;
using System.Collections.Generic;
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

            PreserveUnknownMetadata(document, project.SourceDocument);
            return JsonSerializer.Serialize(document, SerializerOptions);
        }

        /// <summary>
        /// Carry the JSON fields this build does not know about from the loaded <paramref name="source"/> onto the freshly
        /// written <paramref name="document"/> (I-11 D3). Only UNKNOWN keys move: wrapper-level extension data, plus the
        /// extension data of the two I-11 payloads (FlowBed / Larguero) when the active payload matches. Known payload slots
        /// are typed properties, never extension data, so an inactive known payload is never resurrected; the fresh
        /// document's own known fields (just written from the possibly-edited model) always win.
        /// </summary>
        private static void PreserveUnknownMetadata(RackProjectDocument document, RackProjectDocument source)
        {
            if (source == null)
            {
                return;
            }

            // Preserve the wrapper's unknown fields AND its schema version — never DOWNGRADE a newer same-major minor a
            // future build wrote (I-11). The known payload slots are typed properties (never extension data), so this
            // never resurrects an inactive known payload.
            document.SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(source.SchemaVersion, RackProjectDocument.CurrentSchemaVersion);
            document.ExtensionData = source.ExtensionData;

            if (document.FlowBed != null && source.FlowBed != null)
            {
                document.FlowBed.SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(source.FlowBed.SchemaVersion, FlowBedDocument.CurrentSchemaVersion);
                document.FlowBed.ExtensionData = source.FlowBed.ExtensionData;
            }

            if (document.Larguero != null && source.Larguero != null)
            {
                document.Larguero.SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(source.Larguero.SchemaVersion, LargueroDocument.CurrentSchemaVersion);
                document.Larguero.ExtensionData = source.Larguero.ExtensionData;
            }
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

        /// <summary>
        /// Try to deserialize a persisted design into a project (e.g. the inner <c>Design</c> a drawing block carries for
        /// its dynamic/cabecera view). Returns false, <paramref name="project"/> null, on any failure.
        /// <paramref name="incompatibleMajor"/> is true ONLY when the failure is a higher wrapper/header MAJOR this build
        /// cannot read — so a caller re-stamping a sibling block can avoid HIDING that incompatibility behind a fallback
        /// (I-11); a benign failure (malformed/legacy/empty) leaves it false.
        /// </summary>
        public bool TryDeserialize(string json, out RackProject project, out bool incompatibleMajor)
        {
            project = null;
            incompatibleMajor = false;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                project = Deserialize(json);
                return true;
            }
            catch (Exception)
            {
                // Any read failure yields false (matching the "on any failure" contract) so a corrupt sibling inner
                // design never escapes the caller's redraw. incompatibleMajor is derived from the readability gate.
                incompatibleMajor = !IsReadable(json);
                return false;
            }
        }

        /// <summary>
        /// True unless the JSON declares a wrapper/header schema MAJOR higher than this build writes — the non-throwing
        /// readability gate that mirrors the store's own wrapper-vs-header decision (a <c>kind</c> property = wrapper,
        /// schema 2.x; otherwise a bare header, schema 1.x). Missing/malformed/legacy versions read as readable.
        /// </summary>
        public bool IsReadable(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return true;
            }

            try
            {
                using var probe = JsonDocument.Parse(json);
                var root = probe.RootElement;
                if (root.ValueKind != JsonValueKind.Object)
                {
                    return true;
                }

                string version = null;
                var isWrapper = false;
                foreach (var property in root.EnumerateObject())
                {
                    if (string.Equals(property.Name, "kind", StringComparison.OrdinalIgnoreCase))
                    {
                        isWrapper = true;
                    }
                    else if (string.Equals(property.Name, "schemaVersion", StringComparison.OrdinalIgnoreCase)
                        && property.Value.ValueKind == JsonValueKind.String)
                    {
                        version = property.Value.GetString();
                    }
                }

                var current = isWrapper
                    ? RackProjectDocument.CurrentSchemaVersion
                    : RackFrameProjectDocument.CurrentSchemaVersion;
                return SchemaVersionPolicy.IsReadable(version, current);
            }
            catch (JsonException)
            {
                return true; // malformed → benign, not a MAJOR incompatibility
            }
        }

        /// <summary>
        /// Resolve the source project a view-block carries in its inner <c>Design</c> for a redraw that must preserve that
        /// block's metadata (I-11), as a DISCRIMINATED result so the caller can PREFLIGHT every linked view and abort the
        /// whole edit without overwriting anything:
        /// <list type="bullet">
        /// <item>readable and matching <paramref name="expectedKind"/> =&gt; <see cref="InnerSourceOutcome.Success"/>;</item>
        /// <item>readable but a DIFFERENT kind =&gt; <see cref="InnerSourceOutcome.WrongKind"/> (blocking);</item>
        /// <item>a higher schema MAJOR =&gt; <see cref="InnerSourceOutcome.IncompatibleMajor"/> (blocking — never downgraded);</item>
        /// <item>a benign failure (missing/malformed/legacy) =&gt; <see cref="InnerSourceOutcome.BenignFallback"/> to
        /// <paramref name="initiating"/>.</item>
        /// </list>
        /// </summary>
        public InnerSourceResolution ResolveInnerSource(string designJson, RackSystemKind expectedKind, RackProject initiating)
        {
            if (TryDeserialize(designJson, out var source, out var incompatibleMajor))
            {
                return source.Kind == expectedKind
                    ? InnerSourceResolution.Success(source)
                    : InnerSourceResolution.WrongKind();
            }

            return incompatibleMajor
                ? InnerSourceResolution.IncompatibleMajor()
                : InnerSourceResolution.BenignFallback(initiating);
        }

        /// <summary>
        /// Preflight the inner designs of ALL linked view-blocks at once (I-11): resolve each via <see cref="ResolveInnerSource"/>
        /// and, on the FIRST blocking outcome (incompatible major or wrong kind), abort with NO resolved sources so the caller
        /// modifies no block (no partial update). Otherwise return the per-view resolved sources aligned with
        /// <paramref name="designJsons"/>.
        /// </summary>
        public InnerSourcePreflightResult PreflightInnerSources(
            IReadOnlyList<string> designJsons, RackSystemKind expectedKind, RackProject initiating)
        {
            var resolved = new List<RackProject>(designJsons.Count);
            foreach (var designJson in designJsons)
            {
                var resolution = ResolveInnerSource(designJson, expectedKind, initiating);
                if (resolution.IsBlocking)
                {
                    return InnerSourcePreflightResult.Abort(resolution.Outcome);
                }

                resolved.Add(resolution.Project);
            }

            return InnerSourcePreflightResult.Ok(resolved);
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
            // Unregistered / undefined numeric Kind (e.g. (RackSystemKind)999): exactly the historical header/Selective
            // fallback. Get(Selective) is always registered, and its Build is the header path.
            var project = registry.TryGet(document.Kind, out var descriptor)
                ? descriptor.Build(document, builder)
                : registry.Get(RackSystemKind.Selective).Build(document, builder);

            // Keep the source document so a later re-save can carry forward unknown JSON fields (I-11 D3).
            project.SourceDocument = document;
            return project;
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
