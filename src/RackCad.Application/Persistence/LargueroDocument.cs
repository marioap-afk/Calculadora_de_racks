using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Domain.Systems;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Versioned persistence document for a <see cref="LargueroDesign"/> (beam component), stored in the library wrapper
    /// (<see cref="RackProjectDocument.Larguero"/>). Larguero is visual/BOM only — it has no drawing block, so it is
    /// never embedded via <see cref="RackEmbedDocument"/>. Like <see cref="FlowBedDocument"/> it is a FLAT object with the
    /// SAME field names and casing the domain POCO produced before, plus the additive <see cref="SchemaVersion"/>; older
    /// builds keep reading it and ignore the extra fields (initiative I-11, D1/D3). A legacy flat payload with no
    /// <c>SchemaVersion</c> loads via the fallback; unknown fields survive a load/save through <see cref="ExtensionData"/>.
    /// </summary>
    public sealed class LargueroDocument
    {
        /// <summary>Schema version this build writes; a file with a higher MAJOR is rejected (see <see cref="SchemaGuard"/>).</summary>
        public const string CurrentSchemaVersion = "1.0";

        public string SchemaVersion { get; set; } = CurrentSchemaVersion;

        // The known fields mirror LargueroDesign verbatim (names and casing).
        public string Name { get; set; }
        public string BeamProfileId { get; set; }
        public double Peralte { get; set; }
        public double Length { get; set; }
        public string MensulaOverride { get; set; }

        /// <summary>JSON fields this build does not know about, preserved verbatim across a load/save (I-11, D3).</summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; }

        public static LargueroDocument FromDomain(LargueroDesign design)
        {
            if (design == null)
            {
                return null;
            }

            return new LargueroDocument
            {
                Name = design.Name,
                BeamProfileId = design.BeamProfileId,
                Peralte = design.Peralte,
                Length = design.Length,
                MensulaOverride = design.MensulaOverride
            };
        }

        public LargueroDesign ToDomain()
        {
            return new LargueroDesign
            {
                Name = Name,
                BeamProfileId = BeamProfileId,
                Peralte = Peralte,
                Length = Length,
                MensulaOverride = MensulaOverride
            };
        }
    }
}
