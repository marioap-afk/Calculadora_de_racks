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
        /// LEGACY. Read by NOTHING, and never asked for.
        ///
        /// <para>It was a required margin from the ends of the column bottom plate to its outermost punch until
        /// the owner rejected it (I-37D, ronda 2, motivo 1): a parameter without product utility. What limits a
        /// hole is not a number somebody types, it is whether the HOLE ITSELF fits — a centre is legal when it
        /// clears both edges by its own RADIUS, and by nothing more. That rule lives in the punch pattern.</para>
        ///
        /// <para>The property is NOT deleted: <c>CantileverPunchParameters</c> belongs to the I-37A contract
        /// already integrated in <c>main</c>, and removing it would break an API that is not this round's to
        /// break. It is deprecated data: a design may still carry it, the resolver ignores it, the editor never
        /// shows it and the persistence never writes it again.</para>
        /// </summary>
        public double? ColumnBottomPlateEndOffset { get; set; }

        /// <summary>
        /// LEGACY. Read by NOTHING, and never asked for. See <see cref="ColumnBottomPlateEndOffset"/>.
        ///
        /// It used to STOP the regular grid; the grid now stops itself at the last whole hole that fits under
        /// the physical top of the column.
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
