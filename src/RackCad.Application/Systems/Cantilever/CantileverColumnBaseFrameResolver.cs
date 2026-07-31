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
            CantileverColumnOrientation orientation, StructuralSectionGeometry geometry, double columnStartZ)
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
                            columnStartZ),
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

        /// <summary>
        /// Reflects a frame through the column's CENTRAL PLANE <c>y = 0</c>, for the second face of a station
        /// (I-37C).
        ///
        /// It lives here, and not in the station that needs it, because constructing a placement frame is what
        /// this type is the authority for — a source guard enforces exactly that. The station transforms a
        /// side; it does not get to build a frame.
        ///
        /// The reflection is IMPROPER: it maps a right-handed triple to a left-handed one, and
        /// <c>LocalFrame3D</c> only ever builds right-handed frames. So the handedness does not live in the
        /// frame at all — the caller carries it in the section instance's own <c>Mirrored</c> flag, which is
        /// what that flag is for. A caller that took this frame and forgot the flag would silently un-mirror
        /// an asymmetric section, so the two belong together.
        /// </summary>
        public static LocalFrame3D MirrorAboutCentralPlane(LocalFrame3D frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException(nameof(frame));
            }

            // La X local se NIEGA además de reflejarse, y ahí está la corrección.
            //
            // `LocalFrame3D` fija AxisY = AxisZ × AxisX, así que reflejar sólo Z y X dejaba una AxisY volteada:
            // la sección quedaba boca abajo, y el flag `Mirrored` que el llamador voltea compensa en la X, no
            // en la Y. Las dos cosas juntas no se cancelan — dan un giro de 180° sobre el eje del miembro.
            //
            // En una W, que es doblemente simétrica, ese giro no cambia el contorno y por eso nadie lo veía;
            // en cuanto la base sea un perfil asimétrico —un canal, un ángulo— sale del revés. Medido sobre la
            // línea de referencia en gónd0la doble: el patín inferior de la base espejada se dibujaba arriba,
            // entre 11.82 y 12.2 in, en vez de abajo entre 0 y 0.38.
            //
            // Negando la X local, AxisY se conserva y el flag `Mirrored` compensa exactamente el eje que se
            // negó. La reflexión sigue siendo impropia y la mano sigue viviendo en el flag: lo único que
            // cambia es CUÁL de los dos ejes carga con ella.
            var reflectedX = Reflect(frame.AxisX);

            return LocalFrame3D.Create(
                Reflect(frame.Origin),
                Reflect(frame.AxisZ),
                new Vector3D(-reflectedX.X, -reflectedX.Y, -reflectedX.Z));
        }

        /// <summary>Reflects a point through <c>y = 0</c>.</summary>
        public static Point3D Reflect(Point3D p) => new Point3D(p.X, -p.Y, p.Z);

        /// <summary>Reflects a direction through <c>y = 0</c>.</summary>
        public static Vector3D Reflect(Vector3D v) => new Vector3D(v.X, -v.Y, v.Z);
    }
}
