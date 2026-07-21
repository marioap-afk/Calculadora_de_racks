using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Versioned persistence document for a <see cref="FlowBedConfiguration"/> (roller flow bed / cama), used both in
    /// the library wrapper (<see cref="RackProjectDocument.FlowBed"/>) and embedded in the drawing via
    /// <see cref="FlowBedConfigurationStore"/>. It stays a FLAT object with the SAME field names and casing the domain
    /// POCO produced before (no wrapping node), so older builds keep reading it and just ignore the additive
    /// <see cref="SchemaVersion"/> and any extension data (initiative I-11, D1/D3). A legacy flat JSON with no
    /// <c>SchemaVersion</c> loads via the fallback; unknown fields survive a load/save through <see cref="ExtensionData"/>.
    /// </summary>
    public sealed class FlowBedDocument
    {
        /// <summary>Schema version this build writes; a file with a higher MAJOR is rejected (see <see cref="SchemaGuard"/>).</summary>
        public const string CurrentSchemaVersion = "1.0";

        public string SchemaVersion { get; set; } = CurrentSchemaVersion;

        // The known fields mirror FlowBedConfiguration verbatim (names, casing and defaults) so the on-disk shape is
        // unchanged apart from the additive SchemaVersion.
        public FlowBedType BedType { get; set; } = FlowBedType.Dynamic;
        public double LaneDepth { get; set; }
        public double PalletDepth { get; set; }
        public string RollerId { get; set; } = FlowBedDefaults.RollerId;
        public double? RollerPitchOverride { get; set; }

        /// <summary>JSON fields this build does not know about, preserved verbatim across a load/save (I-11, D3).</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; }

        public static FlowBedDocument FromDomain(FlowBedConfiguration config)
        {
            if (config == null)
            {
                return null;
            }

            return new FlowBedDocument
            {
                BedType = config.BedType,
                LaneDepth = config.LaneDepth,
                PalletDepth = config.PalletDepth,
                RollerId = config.RollerId,
                RollerPitchOverride = config.RollerPitchOverride
            };
        }

        public FlowBedConfiguration ToDomain()
        {
            return new FlowBedConfiguration
            {
                BedType = BedType,
                LaneDepth = LaneDepth,
                PalletDepth = PalletDepth,
                RollerId = RollerId,
                RollerPitchOverride = RollerPitchOverride
            };
        }
    }
}
