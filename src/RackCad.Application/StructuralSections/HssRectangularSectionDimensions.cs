namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Rectangular and square HSS measurements, in inches.
    ///
    /// The two thicknesses are NOT redundant and neither derives from the other (ADR-0021 §11):
    /// <see cref="NominalThickness"/> (<c>tnom</c>) is the wall thickness the tube is named and ordered by;
    /// <see cref="DesignThickness"/> (<c>tdes</c>) is the reduced value used in calculations for cold-formed
    /// HSS. The owner fixed that FUTURE GEOMETRY (I-36B) will use the NOMINAL one.
    ///
    /// Square is a case of rectangular, not a fifth family: AISC tabulates both under Type <c>HSS</c> with the
    /// same columns, and <see cref="IsSquare"/> reports the case instead of duplicating the type.
    /// </summary>
    public sealed class HssRectangularSectionDimensions : IStructuralSectionDimensions
    {
        public StructuralSectionFamily Family => StructuralSectionFamily.HssRectangular;

        /// <summary>AISC <c>Ht</c> — overall depth of a square HSS, or the longer wall of a rectangular one.</summary>
        public double? OverallDepth { get; init; }

        /// <summary>AISC <c>B</c> — overall width of a square HSS, or the shorter wall of a rectangular one.</summary>
        public double? OverallWidth { get; init; }

        /// <summary>AISC <c>h</c> — depth of the FLAT wall (the overall depth minus the corner radii).</summary>
        public double? FlatDepth { get; init; }

        /// <summary>AISC <c>b</c> — width of the FLAT wall.</summary>
        public double? FlatWidth { get; init; }

        /// <summary>AISC <c>tnom</c> — nominal wall thickness. The one I-36B will draw with.</summary>
        public double? NominalThickness { get; init; }

        /// <summary>AISC <c>tdes</c> — design wall thickness. Kept for calculation, never used to draw.</summary>
        public double? DesignThickness { get; init; }

        /// <summary>
        /// True when both walls are equal. Compared exactly because both numbers come from the same tabulated
        /// source with the same rounding: a square HSS publishes literally the same value in both columns.
        /// </summary>
        public bool IsSquare =>
            OverallDepth.HasValue && OverallWidth.HasValue && OverallDepth.Value == OverallWidth.Value;
    }
}
