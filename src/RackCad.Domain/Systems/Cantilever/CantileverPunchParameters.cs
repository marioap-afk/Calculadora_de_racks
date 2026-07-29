namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>
    /// The numeric parameters of every punch of the column–base sub-assembly.
    ///
    /// They live in ONE type, and not spread over the column and the base, because the connection punches
    /// belong to both: the rear plate of the base and the connecting face of the column carry the SAME
    /// holes. Two copies of these numbers would be two sources for one bolt.
    ///
    /// <see cref="ColumnBottomPlateEndOffset"/> and <see cref="ColumnTopPunchOffset"/> are nullable because
    /// the owner has NOT approved a default for them. A null is not "use a sensible value": the resolver
    /// rejects the design with <c>CANT_REQUIRED_PARAMETER_MISSING</c>. Giving them a number here would turn
    /// a guess into an approved constant, which is exactly what the initiative forbids.
    /// </summary>
    public sealed class CantileverPunchParameters
    {
        /// <summary>Diameter of every punch, inches.</summary>
        public double Diameter { get; set; } = CantileverDefaults.PunchDiameter;

        /// <summary>Distance from each transverse edge of the COLUMN envelope to its punch row, inches.</summary>
        public double HorizontalEndOffset { get; set; } = CantileverDefaults.PunchHorizontalEndOffset;

        /// <summary>Vertical spacing of the connection region, inches.</summary>
        public double ConnectionPitch { get; set; } = CantileverDefaults.ConnectionPunchPitch;

        /// <summary>Vertical offset from the rear plate's bottom edge to its first punch, and from its last punch to its top edge, inches.</summary>
        public double RearPlateVerticalEndOffset { get; set; } = CantileverDefaults.RearPlateVerticalEndOffset;

        /// <summary>Vertical spacing of the column's regular region, inches.</summary>
        public double RegularColumnPitch { get; set; } = CantileverDefaults.RegularColumnPunchPitch;

        /// <summary>How many connection punches sit above the base section envelope.</summary>
        public int ConnectionPunchesAboveBase { get; set; } = CantileverDefaults.ConnectionPunchesAboveBase;

        /// <summary>Spacing of the column bottom plate punches along the section depth, inches.</summary>
        public double ColumnBottomPlatePitch { get; set; } = CantileverDefaults.ColumnBottomPlatePunchPitch;

        /// <summary>
        /// REQUIRED, no approved default. Minimum distance from both ends of the column bottom plate — along
        /// the section depth — to the outermost punch it may keep. Null makes the design invalid.
        /// </summary>
        public double? ColumnBottomPlateEndOffset { get; set; }

        /// <summary>
        /// REQUIRED, no approved default. Minimum distance from the top of the column to its last regular
        /// punch. It is what STOPS the regular grid; without it the grid has no end. Null makes the design
        /// invalid.
        /// </summary>
        public double? ColumnTopPunchOffset { get; set; }

        public CantileverPunchParameters DeepCopy() =>
            new CantileverPunchParameters
            {
                Diameter = Diameter,
                HorizontalEndOffset = HorizontalEndOffset,
                ConnectionPitch = ConnectionPitch,
                RearPlateVerticalEndOffset = RearPlateVerticalEndOffset,
                RegularColumnPitch = RegularColumnPitch,
                ConnectionPunchesAboveBase = ConnectionPunchesAboveBase,
                ColumnBottomPlatePitch = ColumnBottomPlatePitch,
                ColumnBottomPlateEndOffset = ColumnBottomPlateEndOffset,
                ColumnTopPunchOffset = ColumnTopPunchOffset
            };
    }
}
