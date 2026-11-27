namespace RackCad.Domain.Systems.Larguero
{
    /// <summary>
    /// A "larguero" (beam) COMPONENT definition: a steel profile of a given peralte and cut length ("A corte"), plus
    /// its ménsulas (2 per beam). It is a reusable component — the same larguero appears in the selective/dynamic BOMs.
    /// It is saved to the design library; the page «Nuevo › Larguero» of the RackCad window also inserts it in the drawing as
    /// a loose piece (sexta tanda, part «lg»), which is never a rack and is never read back.
    /// <para>
    /// The profile is EITHER a larguero of RackCad (<see cref="BeamProfileId"/>, a row of secciones.csv with rol LARGUERO), as every
    /// larguero saved before, OR a section of another catalog (sexta tanda, part «lg», the Owner: «Un campo nuevo»):
    /// <see cref="SectionCatalog"/> names the catalog (<see cref="AiscCatalog"/> today) and <see cref="SectionId"/> the section in it.
    /// </para>
    /// </summary>
    public sealed class LargueroDesign
    {
        /// <summary>The value of <see cref="SectionCatalog"/> for a section of the neutral catalog of structural sections (AISC v16.0).</summary>
        public const string AiscCatalog = "AISC";

        /// <summary>Client-facing name ("Larguero 2.40 m"); may be empty (an auto-name is used then).</summary>
        public string Name { get; set; }

        /// <summary>Catalog id of the beam profile (from the LARGUERO sections); blank for a section of another catalog.</summary>
        public string BeamProfileId { get; set; }

        /// <summary>Peralte (the beam block's PERALTE grip); one of the profile's allowed values. For an AISC section, its depth.</summary>
        public double Peralte { get; set; }

        /// <summary>Cut length ("A corte") of the profile (in) — the clear span is this plus the two ménsula overhangs.</summary>
        public double Length { get; set; }

        /// <summary>Optional ménsula id; blank = derived from the beam profile's catalog default (its Mensula FK).</summary>
        public string MensulaOverride { get; set; }

        /// <summary>
        /// The catalog of the section when it is NOT a larguero of RackCad: <see cref="AiscCatalog"/> for a section of the neutral
        /// catalog. Blank (every larguero saved before this field existed) = a larguero of RackCad, <see cref="BeamProfileId"/>.
        /// </summary>
        public string SectionCatalog { get; set; }

        /// <summary>The id of the section in <see cref="SectionCatalog"/> (for AISC, the id of the neutral catalog: «AISC-C-C10X15_3»).</summary>
        public string SectionId { get; set; }
    }
}
