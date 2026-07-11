namespace RackCad.Domain.Systems
{
    /// <summary>
    /// A "larguero" (beam) COMPONENT definition: a steel profile of a given peralte and cut length ("A corte"), plus
    /// its ménsulas (2 per beam). It is a reusable component — the same larguero appears in the selective/dynamic BOMs.
    /// VISUAL + BOM only for now (it has no AutoCAD block yet), so the editor shows a schematic + its bill of materials
    /// and saves to the design library, but does not draw into the drawing.
    /// </summary>
    public sealed class LargueroDesign
    {
        /// <summary>Client-facing name ("Larguero 2.40 m"); may be empty (an auto-name is used then).</summary>
        public string Name { get; set; }

        /// <summary>Catalog id of the beam profile (from the LARGUERO sections).</summary>
        public string BeamProfileId { get; set; }

        /// <summary>Peralte (the beam block's PERALTE grip); one of the profile's allowed values.</summary>
        public double Peralte { get; set; }

        /// <summary>Cut length ("A corte") of the profile (in) — the clear span is this plus the two ménsula overhangs.</summary>
        public double Length { get; set; }

        /// <summary>Optional ménsula id; blank = derived from the beam profile's catalog default (its Mensula FK).</summary>
        public string MensulaOverride { get; set; }
    }
}
