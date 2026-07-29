namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>
    /// Editable intent of a Cantilever COLUMN: which catalogued section it is made of, how tall it is, and
    /// the plate at its foot.
    ///
    /// <see cref="SectionId"/> is TEXT, exactly like <c>DynamicRackDesign.InOutBeamCatalogId</c>, and for a
    /// reason that is structural rather than stylistic: <c>StructuralSectionId</c> lives in
    /// <c>RackCad.Application</c>, this project declares no project reference at all, and the persisted
    /// document would store text in any case — the id type has a private constructor, a getter-only value
    /// and no JSON converter. The single place that turns this string into an id is the Application
    /// resolver (ADR-0024).
    ///
    /// It may be a DIFFERENT section from the base's. Nothing here assumes they match.
    /// </summary>
    public sealed class CantileverColumnDesign
    {
        /// <summary>Catalogued section of the column, as the text of its <c>StructuralSectionId</c>.</summary>
        public string SectionId { get; set; }

        /// <summary>Length of the column piece along its own axis, inches. Strictly positive.</summary>
        public double Height { get; set; }

        /// <summary>The plate at the foot of the column. Its outline is the column section envelope.</summary>
        public CantileverPlateDesign BottomPlate { get; set; } = new CantileverPlateDesign();

        public CantileverColumnDesign DeepCopy() =>
            new CantileverColumnDesign
            {
                SectionId = SectionId,
                Height = Height,
                BottomPlate = BottomPlate?.DeepCopy() ?? new CantileverPlateDesign()
            };
    }
}
