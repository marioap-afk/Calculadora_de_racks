using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.ProjectVariables;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// The drawing-level register of project variables, as it is persisted (ADR-0034 / I-47 D-01-bis). One per
    /// DWG, with its own schema line — independent of the selective design's, because they are two different
    /// documents and their versions do not mix.
    ///
    /// <para>
    /// <b><see cref="SchemaVersion"/> has NO field initializer, and that is the contract, not a slip</b>
    /// (C4.8-1). Every other versioned DTO here declares
    /// <c>SchemaVersion { get; set; } = CurrentSchemaVersion</c>, and <c>System.Text.Json</c> does not touch a
    /// property missing from the JSON: it keeps the initializer. Under that pattern a register whose JSON
    /// never carried the field would deserialize as <c>"1.0"</c>, INDISTINGUISHABLE from one that declared
    /// it — a default value turning an UNKNOWN state into a success. This document is the one place where
    /// that distinction has to survive, because it is born in ID22A and therefore ALWAYS writes its version:
    /// a register without a readable version is not legacy, it is corruption.
    /// </para>
    /// <para>
    /// The rule is deliberately narrow. The historical documents were born before there was a version, for
    /// them a missing one correctly means legacy, and neither their pattern nor
    /// <see cref="SchemaVersionPolicy"/> changes.
    /// </para>
    /// <para>
    /// READ never invents a version; CREATE and WRITE stamp one explicitly
    /// (<see cref="CreateNew"/>, <see cref="ProjectVariablesStore.Serialize"/>).
    /// </para>
    /// </summary>
    public sealed class ProjectVariablesDocument
    {
        /// <summary>The version this build writes. The register is born here, so there is no legacy line below it.</summary>
        public const string CurrentSchemaVersion = "1.0";

        /// <summary>The highest MAJOR this build can read. A stored document above it is an error, never a downgrade.</summary>
        public const int SupportedMajor = 1;

        /// <summary>
        /// The stored schema version. <b>No initializer on purpose</b> (C4.8-1): <c>null</c> here means the
        /// JSON did not carry the property, and that has to stay distinguishable from an explicit
        /// <c>"1.0"</c>.
        /// </summary>
        public string SchemaVersion { get; set; }

        /// <summary>
        /// The variables of the drawing. This one DOES keep an initializer, and the asymmetry is intentional:
        /// an absent list is a register with zero variables, which is a perfectly valid state. It is the
        /// VERSION that carries authority, and only the version.
        /// </summary>
        public List<ProjectVariableDocument> Variables { get; set; } = new List<ProjectVariableDocument>();

        /// <summary>Whatever a later build of the same MAJOR wrote and this one does not understand, preserved verbatim.</summary>
        [JsonExtensionData]
        public IDictionary<string, JsonElement> ExtensionData { get; set; }

        /// <summary>A brand-new register, with its version STAMPED explicitly rather than defaulted into existence.</summary>
        public static ProjectVariablesDocument CreateNew()
            => new ProjectVariablesDocument { SchemaVersion = CurrentSchemaVersion };

        /// <summary>
        /// Projects the persisted entries onto the pure model. Only ever called on a document the store
        /// already accepted, so every entry is known to be valid here.
        /// </summary>
        public IReadOnlyList<ProjectVariable> ToProjectVariables()
        {
            var variables = new List<ProjectVariable>();

            if (Variables == null)
            {
                return variables;
            }

            foreach (var entry in Variables)
            {
                variables.Add(ProjectVariable.Create(
                    VariableId.Parse(entry.VariableId),
                    entry.Name,
                    VariableType.Length,
                    VariableDefinition.Literal(entry.Definition.Value.Value)));
            }

            return variables;
        }
    }

    /// <summary>One persisted project variable. The identity is the <see cref="VariableId"/>; the name is a label.</summary>
    public sealed class ProjectVariableDocument
    {
        public string VariableId { get; set; }

        public string Name { get; set; }

        /// <summary>The type token. A value this build does not know is a HARD error, never an ignored entry.</summary>
        public string Type { get; set; }

        public ProjectVariableDefinitionDocument Definition { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JsonElement> ExtensionData { get; set; }
    }

    /// <summary>
    /// Where a variable's value comes from, discriminated by <see cref="Kind"/> from day one so a future
    /// case can be additive. ID22A writes and accepts exactly one kind.
    /// </summary>
    public sealed class ProjectVariableDefinitionDocument
    {
        /// <summary>The discriminator. An unknown kind is a HARD error: this build cannot resolve a value it does not understand.</summary>
        public string Kind { get; set; }

        public double? Value { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JsonElement> ExtensionData { get; set; }
    }
}
