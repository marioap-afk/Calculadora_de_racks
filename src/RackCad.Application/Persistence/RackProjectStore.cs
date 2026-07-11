using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Saves and loads a project that can be either a selective header or a dynamic system,
    /// using a top-level <c>kind</c> discriminator. Backward compatible: a legacy file with no
    /// <c>kind</c> (a bare header, schema 1.0) loads as a selective project. Derived header
    /// members are rebuilt on load.
    /// </summary>
    public sealed class RackProjectStore
    {
        public const string FileExtension = ".rackcad.json";

        private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();

        private readonly BracingPanelMemberBuilder builder = new BracingPanelMemberBuilder();

        public string Serialize(RackProject project)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var document = new RackProjectDocument { Kind = project.Kind };

            if (project.Kind == RackSystemKind.PalletFlow && project.DynamicSystem != null)
            {
                document.DynamicSystem = DynamicRackSystemDocument.From(project.DynamicSystem);
            }
            else if (project.Kind == RackSystemKind.SelectiveRack && project.SelectiveRack != null)
            {
                document.SelectiveRack = project.SelectiveRack;
            }
            else if (project.Kind == RackSystemKind.Cama && project.FlowBed != null)
            {
                document.FlowBed = project.FlowBed;
            }
            else if (project.Kind == RackSystemKind.Larguero && project.Larguero != null)
            {
                document.Larguero = project.Larguero;
            }
            else
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

            if (document.Kind == RackSystemKind.PalletFlow && document.DynamicSystem != null)
            {
                var system = document.DynamicSystem.ToDomain();

                foreach (var module in system.Modules)
                {
                    if (module.AssociatedFrameConfiguration != null)
                    {
                        builder.RefreshPhysicalModel(module.AssociatedFrameConfiguration);
                    }
                }

                return RackProject.ForDynamic(system);
            }

            if (document.Kind == RackSystemKind.SelectiveRack && document.SelectiveRack != null)
            {
                return RackProject.ForSelectiveRack(document.SelectiveRack);
            }

            if (document.Kind == RackSystemKind.Cama && document.FlowBed != null)
            {
                return RackProject.ForCama(document.FlowBed);
            }

            if (document.Kind == RackSystemKind.Larguero && document.Larguero != null)
            {
                return RackProject.ForLarguero(document.Larguero);
            }

            var header = document.Header?.ToConfiguration();

            if (header != null)
            {
                builder.RefreshPhysicalModel(header);
            }

            return RackProject.ForSelective(header);
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

            var header = document?.ToConfiguration();

            if (header != null)
            {
                builder.RefreshPhysicalModel(header);
            }

            return RackProject.ForSelective(header);
        }

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
