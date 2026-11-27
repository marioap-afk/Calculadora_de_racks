using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Domain.Systems.Larguero;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Versioned persistence document for a <see cref="LargueroDesign"/> (beam component), stored in the library wrapper
    /// (<see cref="RackProjectDocument.Larguero"/>). A larguero has no rack block, so it is never embedded via
    /// <see cref="RackEmbedDocument"/>. Like <see cref="FlowBedDocument"/> it is a FLAT object with the
    /// SAME field names and casing the domain POCO produced before, plus the additive <see cref="SchemaVersion"/>; older
    /// builds keep reading it and ignore the extra fields (initiative I-11, D1/D3). A legacy flat payload with no
    /// <c>SchemaVersion</c> loads via the fallback; unknown fields survive a load/save through <see cref="ExtensionData"/>.
    /// <para>
    /// 1.1 (I-64, sexta tanda, part «lg»; the Owner: «Un campo nuevo»): the ADDITIVE <see cref="SectionCatalog"/> and
    /// <see cref="SectionId"/>, for a larguero made of a section of another catalog (AISC). A 1.0 file loads as before (both blank: a
    /// larguero of RackCad). A build of 1.0 reads a 1.1 file (same major): it keeps the two fields as unknown ones, and a larguero of
    /// AISC, which has no <see cref="BeamProfileId"/>, is refused there as incomplete instead of opening as something else.
    /// </para>
    /// </summary>
    public sealed class LargueroDocument
    {
        /// <summary>Schema version this build writes; a file with a higher MAJOR is rejected (see <see cref="SchemaGuard"/>).</summary>
        public const string CurrentSchemaVersion = "1.1"; // T6 lg: 1.0 -> 1.1, SectionCatalog and SectionId (additive)

        public string SchemaVersion { get; set; } = CurrentSchemaVersion;

        // The known fields mirror LargueroDesign verbatim (names and casing).
        public string Name { get; set; }
        public string BeamProfileId { get; set; }
        public double Peralte { get; set; }
        public double Length { get; set; }
        public string MensulaOverride { get; set; }

        /// <summary>T6 lg (1.1): the catalog of a section that is not a larguero of RackCad («AISC»); null for a larguero of RackCad.</summary>
        public string SectionCatalog { get; set; }

        /// <summary>T6 lg (1.1): the id of that section in its catalog; null for a larguero of RackCad.</summary>
        public string SectionId { get; set; }

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
                MensulaOverride = design.MensulaOverride,
                SectionCatalog = design.SectionCatalog,
                SectionId = design.SectionId
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
                MensulaOverride = MensulaOverride,
                SectionCatalog = SectionCatalog,
                SectionId = SectionId
            };
        }
    }
}
