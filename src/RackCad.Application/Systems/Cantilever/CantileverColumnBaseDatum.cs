using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The local coordinate system of a column–base sub-assembly, declared once so that no piece has to
    /// guess it and no view can re-decide it.
    ///
    /// <list type="bullet">
    ///   <item><b>X</b> — transverse, from one side of a profile to the other.</item>
    ///   <item><b>Y</b> — the direction the base projects. The base goes out in <b>+Y</b>.</item>
    ///   <item><b>Z</b> — vertical, up.</item>
    /// </list>
    ///
    /// Three planes fix the origin, and all three are DERIVED from resolved geometry — never from a
    /// tabulated dimension:
    ///
    /// <list type="number">
    ///   <item>
    ///     <b><c>y = 0</c> — the <see cref="ConnectionPlaneY"/>.</b> The contact plane between the column's
    ///     connecting face and the bearing face of the base's rear plate. The column occupies
    ///     <c>y &lt;= 0</c>; the whole base assembly occupies <c>y &gt;= 0</c>. The connection punch axis is
    ///     <b>+Y</b>.
    ///   </item>
    ///   <item>
    ///     <b><c>z = 0</c> — the <see cref="FloorZ"/>.</b> The floor. The BASE section rests on it and so does
    ///     the column's bottom PLATE; the COLUMN itself starts on the top face of that plate
    ///     (<see cref="ColumnStartZ"/>).
    ///
    ///     <para>Base and column share the LOGICAL connection datum — the punch elevations of the base's rear
    ///     plate and of the column's connecting face are the same absolute numbers — but they do NOT share a
    ///     physical origin in Z, and until the owner's correction of I-37D they did. A column standing on the
    ///     floor passes through its own bottom plate, which is a piece nobody could build (owner decision,
    ///     I-37D, corrección de columna y base).</para>
    ///   </item>
    ///   <item>
    ///     <b><c>x = 0</c> — the <see cref="CentrePlaneX"/>.</b> The transverse centre of the COLUMN section
    ///     envelope. The column governs the two punch columns, so it also governs what "centred" means.
    ///   </item>
    /// </list>
    /// </summary>
    public static class CantileverColumnBaseDatum
    {
        /// <summary>The contact plane between column face and rear plate. The base projects towards +Y.</summary>
        public const double ConnectionPlaneY = 0.0;

        /// <summary>The floor. Everything that rests on the ground starts here.</summary>
        public const double FloorZ = 0.0;

        /// <summary>
        /// The bottom of the BASE section. It rests on the floor and is NOT lifted with the column.
        ///
        /// The base is a piece that stands on the ground; only the column is bolted on top of a plate. Lifting
        /// it too would put the whole assembly a plate-thickness in the air.
        /// </summary>
        public const double BaseBottomZ = FloorZ;

        /// <summary>The bottom of the column's bottom plate: it rests on the floor.</summary>
        public const double ColumnBottomPlateBottomZ = FloorZ;

        /// <summary>The bearing face of the column's bottom plate — the elevation the column stands on.</summary>
        public static double ColumnBottomPlateTopZ(double bottomPlateThickness) =>
            ColumnBottomPlateBottomZ + bottomPlateThickness;

        /// <summary>
        /// Where the COLUMN section begins: the top face of its bottom plate, never the floor.
        ///
        /// It is a separate name from <see cref="FloorZ"/> on purpose. One variable meaning «floor», «start of
        /// base» and «start of column» at once is how the column ended up passing through its own plate.
        /// </summary>
        public static double ColumnStartZ(double bottomPlateThickness) =>
            ColumnBottomPlateTopZ(bottomPlateThickness);

        /// <summary>
        /// The physical top of the column: its start plus its NOMINAL cut length.
        ///
        /// The cut length does not change when the plate does — a thicker plate lifts the column, it does not
        /// make it longer. Keeping the two apart is what stops the thickness being added twice.
        /// </summary>
        public static double ColumnTopZ(double bottomPlateThickness, double nominalColumnCutLength) =>
            ColumnStartZ(bottomPlateThickness) + nominalColumnCutLength;

        /// <summary>Transverse centre, taken from the column section envelope.</summary>
        public const double CentrePlaneX = 0.0;

        /// <summary>The direction the base projects, and the axis of every connection punch.</summary>
        public static Vector3D BaseDirection => Vector3D.UnitY;

        /// <summary>The column's own axis, and the axis of every column bottom plate punch.</summary>
        public static Vector3D ColumnAxis => Vector3D.UnitZ;

        /// <summary>The transverse direction.</summary>
        public static Vector3D TransverseDirection => Vector3D.UnitX;
    }
}
