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
    ///     <b><c>z = 0</c> — the <see cref="FloorZ"/>.</b> The common elevation of the bottom of the column
    ///     section and the bottom of the base section. They MUST share it: every connection elevation is
    ///     measured from the base section's bottom edge and consumed by the column, so two vertical origins
    ///     would make the shared pattern meaningless.
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

        /// <summary>Common bottom elevation of the column section and the base section.</summary>
        public const double FloorZ = 0.0;

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
