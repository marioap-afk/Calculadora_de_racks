using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Top-level project wrapper (schema 2.0). A <c>Kind</c> discriminator selects the payload:
    /// a bare header for a selective project, or a dynamic system. Files without a <c>kind</c>
    /// (schema 1.0 bare headers) are still loadable and treated as selective.
    /// </summary>
    public sealed class RackProjectDocument
    {
        /// <summary>Schema version this build writes; a file with a higher MAJOR is rejected (see <see cref="SchemaGuard"/>).</summary>
        public const string CurrentSchemaVersion = "2.0";

        public string SchemaVersion { get; set; } = CurrentSchemaVersion;
        public RackSystemKind Kind { get; set; } = RackSystemKind.Selective;
        public RackFrameProjectDocument Header { get; set; }
        public DynamicRackSystemDocument DynamicSystem { get; set; }

        /// <summary>Selective pallet-rack design (schema-versioned DTO); set when Kind is SelectiveRack.</summary>
        public SelectivePalletDesignDocument SelectiveRack { get; set; }

        /// <summary>Flow bed (cama) design (schema-versioned, flat DTO); set when Kind is Cama.</summary>
        public FlowBedDocument FlowBed { get; set; }

        /// <summary>Larguero component (schema-versioned, flat DTO); set when Kind is Larguero.</summary>
        public LargueroDocument Larguero { get; set; }

        /// <summary>Push Back design (schema-versioned, flat DTO with its own ExtensionData); set when Kind is PushBack.
        /// Additive/optional — it does NOT bump the wrapper major (I-11).</summary>
        public PushBackDesignDocument PushBack { get; set; }

        /// <summary>
        /// Wrapper-level JSON fields this build does not know about, preserved verbatim across a load/save (I-11, D3).
        /// Only UNKNOWN keys land here; every known payload slot above is a typed property, so preserving this dictionary
        /// never resurrects an inactive known payload. Null/empty for freshly written projects (no extra keys emitted).
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; }
    }
}
