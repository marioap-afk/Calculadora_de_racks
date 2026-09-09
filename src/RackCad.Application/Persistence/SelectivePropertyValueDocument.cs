using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// A persisted binding: how one property of a selective design takes its value when it is not the
    /// literal beside it (I-47 D-04).
    ///
    /// <para>
    /// The value is DISCRIMINATED by <see cref="Kind"/> and not a bare id string, and the reason is a future
    /// one: a reference to another rack needs two data (a rack and a property), so a bare id would force the
    /// value type to change — which is a schema break. Discriminating from the first day makes that a new
    /// case instead.
    /// </para>
    /// <para>
    /// ID22A writes and accepts exactly one kind. An unknown one is NOT ignored and does NOT fall back to the
    /// literal: a build that cannot interpret the reference cannot claim the literal is the right value.
    /// </para>
    /// </summary>
    public sealed class SelectivePropertyValueDocument
    {
        /// <summary>The only kind ID22A implements: the property is governed by a drawing-level variable.</summary>
        public const string ProjectVariableKind = "projectVariable";

        /// <summary>The discriminator. Compared exactly; an unrecognised value is a state this build cannot resolve.</summary>
        public string Kind { get; set; }

        /// <summary>The referenced variable, as a GUID string.</summary>
        public string VariableId { get; set; }

        [JsonExtensionData]
        public IDictionary<string, JsonElement> ExtensionData { get; set; }

        /// <summary>A reference to a drawing-level project variable.</summary>
        public static SelectivePropertyValueDocument ToProjectVariable(string variableId)
            => new SelectivePropertyValueDocument { Kind = ProjectVariableKind, VariableId = variableId };
    }
}
