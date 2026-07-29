namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>
    /// The Cantilever constants the owner approved (I-37A), each one a DEFAULT and never a hard-coded rule:
    /// every value here is reachable through a design property, so changing one is an edit, not a rebuild.
    ///
    /// Two numbers the owner did NOT approve are deliberately absent — the offset from the ends of the
    /// column's bottom plate to its punches, and the offset from the column top to its last regular punch.
    /// They are REQUIRED design inputs (nullable in <see cref="CantileverPunchParameters"/>, rejected by the
    /// resolver when missing) precisely so that nobody can invent them here and have the invention look like
    /// an approved value.
    /// </summary>
    public static class CantileverDefaults
    {
        /// <summary>Punch diameter, inches. One diameter for every punch of the sub-assembly.</summary>
        public const double PunchDiameter = 0.75;

        /// <summary>
        /// Horizontal distance from each transverse edge of the COLUMN envelope to its punch row, inches.
        ///
        /// It is measured from the column and not from the plate because the column governs the two rows:
        /// the rear plate has to accept them, not define them.
        /// </summary>
        public const double PunchHorizontalEndOffset = 1.50;

        /// <summary>Vertical spacing of the connection region, inches.</summary>
        public const double ConnectionPunchPitch = 2.00;

        /// <summary>
        /// Vertical distance from the bottom edge of the rear plate to its first punch, inches, and also the
        /// distance from its last punch to its top edge. One parameter, used at both ends on purpose: the
        /// plate is symmetric in how it treats its two ends.
        /// </summary>
        public const double RearPlateVerticalEndOffset = 2.50;

        /// <summary>Vertical spacing of the column's regular region, inches. Twice the connection pitch.</summary>
        public const double RegularColumnPunchPitch = 4.00;

        /// <summary>
        /// How many connection punches sit ABOVE the base section envelope. Exactly three, by the owner's
        /// decision — the number is not derived from the plate height; the plate height is derived from it.
        /// </summary>
        public const int ConnectionPunchesAboveBase = 3;

        /// <summary>Spacing of the column bottom plate punches along the section depth, inches.</summary>
        public const double ColumnBottomPlatePunchPitch = 2.00;

        /// <summary>
        /// Default thickness of a plate or of the gusset, inches.
        ///
        /// It is ONE constant used as the default of FOUR independent properties — front plate, rear plate,
        /// column bottom plate and gusset. Sharing a default is not sharing an authority: each component
        /// keeps its own value and changing one never moves the others.
        /// </summary>
        public const double PlateThickness = 0.25;
    }
}
