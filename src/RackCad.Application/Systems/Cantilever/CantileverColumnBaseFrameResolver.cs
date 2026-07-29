using System;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// THE authority that turns a registered orientation into a placement frame.
    ///
    /// It exists because the orientation was data with no reader: <see cref="CantileverColumnBaseVariant"/>
    /// declared <c>ColumnOrientation</c> and <c>BaseOrientation</c> and the resolver built fixed frames
    /// regardless, so registering a second orientation would have changed nothing about the drawing while
    /// looking as though it had. A declared property that nobody consumes is worse than an absent one — it
    /// invites a caller to trust it.
    ///
    /// Pure and static: it reads a <see cref="StructuralSectionGeometry"/> and returns a
    /// <see cref="LocalFrame3D"/>. It takes the GEOMETRY rather than a bounding box so that the promise
    /// "every exterior dimension comes from <c>Bounds</c>" (ADR-0024, D5) is structural — no caller can
    /// hand it a box it computed some other way.
    ///
    /// Every branch is an explicit <c>switch</c> with a throwing default, for the same reason
    /// <c>StructuralSectionGeometryFactory</c> dispatches by family without reflection: a new orientation
    /// must not be addable without the compiler, or a test, saying so.
    /// </summary>
    public static class CantileverColumnBaseFrameResolver
    {
        /// <summary>Whether this column orientation has a frame rule. False for anything not declared.</summary>
        public static bool IsSupported(CantileverColumnOrientation orientation) =>
            orientation == CantileverColumnOrientation.DepthAlongBase;

        /// <summary>Whether this base orientation has a frame rule. False for anything not declared.</summary>
        public static bool IsSupported(CantileverBaseOrientation orientation) =>
            orientation == CantileverBaseOrientation.DepthVertical;

        /// <summary>
        /// The column's placement frame.
        ///
        /// For <see cref="CantileverColumnOrientation.DepthAlongBase"/>: the section's local X runs across
        /// the run and its local Y along the base direction, and the origin is pushed so the CONNECTING
        /// FACE lands on the datum plane and the transverse centre on <c>x = 0</c>. The frame origin sits at
        /// the section centroid, so both shifts are offsets read from the envelope — never "half of d".
        /// </summary>
        public static LocalFrame3D ColumnFrame(
            CantileverColumnOrientation orientation, StructuralSectionGeometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            var bounds = geometry.Bounds;

            switch (orientation)
            {
                case CantileverColumnOrientation.DepthAlongBase:
                    return LocalFrame3D.Create(
                        new Point3D(
                            -bounds.Center.X,
                            CantileverColumnBaseDatum.ConnectionPlaneY - bounds.MaxY,
                            CantileverColumnBaseDatum.FloorZ),
                        Vector3D.UnitZ,
                        Vector3D.UnitX);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(orientation), orientation,
                        "La orientacion de columna '" + orientation +
                        "' no tiene regla de marco. Anadir una orientacion exige escribirla aqui.");
            }
        }

        /// <summary>
        /// The base's placement frame.
        ///
        /// For <see cref="CantileverBaseOrientation.DepthVertical"/>: the longitudinal axis is <c>+Y</c> and
        /// the section's local Y maps onto world <c>+Z</c>, which is what "depth vertical" means. Asking for
        /// that mapping fixes the X reference as <c>up × forward</c>; the expression is written that way
        /// rather than as a literal so the derivation stays visible.
        ///
        /// The base starts AFTER the rear plate, so its origin is offset along Y by
        /// <paramref name="rearPlateThickness"/>: everything on the far side of the datum plane belongs to
        /// the base assembly.
        /// </summary>
        public static LocalFrame3D BaseFrame(
            CantileverBaseOrientation orientation, StructuralSectionGeometry geometry, double rearPlateThickness)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            GeometryTolerance.RequirePositive(rearPlateThickness, nameof(rearPlateThickness));

            var bounds = geometry.Bounds;

            switch (orientation)
            {
                case CantileverBaseOrientation.DepthVertical:
                    return LocalFrame3D.Create(
                        new Point3D(
                            bounds.Center.X,
                            CantileverColumnBaseDatum.ConnectionPlaneY + rearPlateThickness,
                            CantileverColumnBaseDatum.FloorZ - bounds.MinY),
                        Vector3D.UnitY,
                        Vector3D.UnitZ.Cross(Vector3D.UnitY));

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(orientation), orientation,
                        "La orientacion de base '" + orientation +
                        "' no tiene regla de marco. Anadir una orientacion exige escribirla aqui.");
            }
        }
    }
}
