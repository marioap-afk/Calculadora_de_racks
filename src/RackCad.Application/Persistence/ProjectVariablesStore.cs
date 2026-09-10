using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.ProjectVariables;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Serializes and reads the drawing's project-variable register. PURE: the Plugin contributes only the
    /// access to the drawing dictionary, so everything that decides whether a register is usable is covered
    /// by the Core suite (I-47 D-01).
    ///
    /// <para>
    /// It returns a <see cref="ProjectVariablesReadResult"/> instead of throwing or returning null, because
    /// the states that matter here are EXPECTED ones — a corrupt register, a future major, a version this
    /// build cannot parse — and an expected state that travels as an exception ends up decided by whichever
    /// <c>catch</c> happens to be above it.
    /// </para>
    /// <para>
    /// Three hard errors, and none of them degrades to a warning (D-01-bis): a MAJOR above
    /// <see cref="ProjectVariablesDocument.SupportedMajor"/>, an unknown variable type, and an unknown
    /// definition kind. The rule underneath is one: <b>never ignore data this build does not understand</b>.
    /// Carrying on would mean drawing with an authority nobody can resolve.
    /// </para>
    /// </summary>
    public sealed class ProjectVariablesStore
    {
        private const string LiteralKind = "literal";

        private static readonly JsonSerializerOptions SerializerOptions = CreateOptions();

        /// <summary>
        /// Writes the register, STAMPING its version explicitly (C4.8-1): a new document goes out as
        /// <see cref="ProjectVariablesDocument.CurrentSchemaVersion"/>, and one stored with a newer MINOR of
        /// the same major keeps that minor so a re-save never downgrades a file this build can still read.
        /// The version is put there by whoever writes, on purpose — it never appears by itself on read.
        /// </summary>
        public string Serialize(ProjectVariablesDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            if (TryParseVersion(document.SchemaVersion, out var major, out _) &&
                major > ProjectVariablesDocument.SupportedMajor)
            {
                throw new InvalidOperationException(
                    "Las variables de proyecto fueron creadas con una versión más nueva de RackCad (esquema " +
                    document.SchemaVersion + "); no se sobrescriben.");
            }

            document.SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(
                document.SchemaVersion,
                ProjectVariablesDocument.CurrentSchemaVersion);

            return JsonSerializer.Serialize(document, SerializerOptions);
        }

        /// <summary>
        /// Turns what the drawing PHYSICALLY held into what it MEANS.
        ///
        /// <para>
        /// This is the whole of the boundary the register crosses: the layer that touches AutoCAD reports
        /// absent, present-with-text or present-and-unusable, and every judgement about versions, unknown
        /// types and unknown definition kinds is made here, where the Core suite can reach it.
        /// </para>
        /// <para>
        /// A null payload is treated as PRESENT AND UNREADABLE rather than absent. "I was not told" is not
        /// evidence that the drawing has no register, and turning it into an empty one is how every variable
        /// in a drawing gets erased by the next write.
        /// </para>
        /// </summary>
        public ProjectVariablesReadResult Read(ProjectVariablesPayload payload)
        {
            if (payload == null)
            {
                return ProjectVariablesReadResult.Unreadable(
                    "No se pudo determinar el estado del registro de variables de proyecto en el dibujo.");
            }

            switch (payload.State)
            {
                case ProjectVariablesPayloadState.Absent:
                    return ProjectVariablesReadResult.Absent();

                case ProjectVariablesPayloadState.PresentButUnreadable:
                    return ProjectVariablesReadResult.Unreadable(payload.Error);

                default:
                    return Deserialize(payload.Json);
            }
        }

        /// <summary>
        /// Reads a register whose entry the caller has already established is PRESENT. The absence of the
        /// entry is a different thing and is modelled by <see cref="ProjectVariablesReadResult.Absent"/>:
        /// this method never turns a failure into an empty register.
        /// </summary>
        public ProjectVariablesReadResult Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return ProjectVariablesReadResult.Unreadable(
                    "El registro de variables de proyecto está presente pero vacío.");
            }

            ProjectVariablesDocument document;

            try
            {
                document = JsonSerializer.Deserialize<ProjectVariablesDocument>(json, SerializerOptions);
            }
            catch (JsonException ex)
            {
                return ProjectVariablesReadResult.Unreadable(
                    "El registro de variables de proyecto no es un JSON válido: " + ex.Message);
            }

            if (document == null)
            {
                return ProjectVariablesReadResult.Unreadable(
                    "El registro de variables de proyecto no contiene un documento.");
            }

            // C4.8-1: null here means the property was ABSENT from the JSON. It is NOT "1.0".
            if (string.IsNullOrWhiteSpace(document.SchemaVersion))
            {
                return ProjectVariablesReadResult.Unreadable(
                    "El registro de variables de proyecto no declara su versión de esquema. " +
                    "Este registro siempre la escribe, así que su ausencia es corrupción, no un archivo heredado.");
            }

            if (!TryParseVersion(document.SchemaVersion, out var major, out _))
            {
                return ProjectVariablesReadResult.Unreadable(
                    "La versión de esquema del registro de variables de proyecto no es interpretable ('" +
                    document.SchemaVersion + "').");
            }

            if (major > ProjectVariablesDocument.SupportedMajor)
            {
                return ProjectVariablesReadResult.IncompatibleMajor(
                    "Las variables de proyecto fueron creadas con una versión más nueva de RackCad (esquema " +
                    document.SchemaVersion + "); actualiza la aplicación para abrirlas.");
            }

            var invalid = FirstInvalidEntry(document);

            return invalid == null
                ? ProjectVariablesReadResult.Readable(document)
                : ProjectVariablesReadResult.Unreadable(invalid);
        }

        /// <summary>The visible reason the first unusable entry is unusable, or null when every entry is understood.</summary>
        private static string FirstInvalidEntry(ProjectVariablesDocument document)
        {
            if (document.Variables == null)
            {
                return null;
            }

            foreach (var entry in document.Variables)
            {
                if (entry == null)
                {
                    return "El registro de variables de proyecto contiene una entrada vacía.";
                }

                if (!VariableId.TryParse(entry.VariableId, out var id))
                {
                    return "El registro de variables de proyecto contiene un VariableId inválido ('" +
                           (entry.VariableId ?? "<null>") + "'); debe ser un GUID.";
                }

                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    return "La variable de proyecto " + id + " no declara nombre.";
                }

                if (!IsKnownType(entry.Type))
                {
                    return "La variable de proyecto " + id + " declara un tipo desconocido ('" +
                           (entry.Type ?? "<null>") + "').";
                }

                if (entry.Definition == null)
                {
                    return "La variable de proyecto " + id + " no declara definición.";
                }

                if (!string.Equals(entry.Definition.Kind, LiteralKind, StringComparison.OrdinalIgnoreCase))
                {
                    return "La variable de proyecto " + id + " declara una definición de clase desconocida ('" +
                           (entry.Definition.Kind ?? "<null>") + "').";
                }

                if (!entry.Definition.Value.HasValue ||
                    double.IsNaN(entry.Definition.Value.Value) ||
                    double.IsInfinity(entry.Definition.Value.Value))
                {
                    return "La variable de proyecto " + id + " no declara un valor literal finito.";
                }
            }

            return null;
        }

        private static bool IsKnownType(string type)
            => string.Equals(type, VariableType.Length.ToString(), StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// A STRICT <c>MAJOR.MINOR</c> parse, local to the register on purpose.
        /// <see cref="SchemaVersionPolicy.MajorOf"/> treats an unparseable version as legacy major 1, which is
        /// right for the historical documents and wrong here: this register always writes its version, so a
        /// value that cannot be read is corruption. The global policy is NOT changed.
        /// </summary>
        private static bool TryParseVersion(string version, out int major, out int minor)
        {
            major = 0;
            minor = 0;

            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            var parts = version.Trim().Split('.');

            if (parts.Length != 2 ||
                !int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out major) ||
                major <= 0 ||
                !int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out minor) ||
                minor < 0)
            {
                major = 0;
                minor = 0;
                return false;
            }

            return true;
        }

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };

            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}
