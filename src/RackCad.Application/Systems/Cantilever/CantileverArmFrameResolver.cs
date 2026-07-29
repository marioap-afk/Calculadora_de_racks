using System;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// THE authority that turns a side, a slope and an orientation into an arm's placement frame.
    ///
    /// It is the arm's counterpart to <see cref="CantileverColumnBaseFrameResolver"/>, and it exists for the
    /// same reason: a declared property nobody reads is worse than an absent one. Every branch is an explicit
    /// <c>switch</c> with a throwing default, so a side or an orientation added tomorrow cannot inherit
    /// today's geometry in silence.
    /// </summary>
    public static class CantileverArmFrameResolver
    {
        /// <summary>Whether this side has a frame rule.</summary>
        public static bool IsSupported(CantileverArmSide side) =>
            side == CantileverArmSide.PositiveY || side == CantileverArmSide.NegativeY;

        /// <summary>Whether this body orientation has a frame rule.</summary>
        public static bool IsSupported(CantileverArmBodyOrientation orientation) =>
            orientation == CantileverArmBodyOrientation.DepthPerpendicularToAxis;

        /// <summary>
        /// The slope angle in radians, derived from the rise per 12 inches.
        ///
        /// Derived and never stored: <c>SlopeRisePer12</c> is the one authority, and keeping degrees beside it
        /// would be two magnitudes for one quantity (ADR-0025, D4).
        /// </summary>
        public static double AngleRadians(double slopeRisePer12)
        {
            if (!GeometryTolerance.IsFinite(slopeRisePer12) || slopeRisePer12 < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slopeRisePer12), slopeRisePer12,
                    "La pendiente debe ser un numero finito no negativo.");
            }

            return Math.Atan(slopeRisePer12 / 12.0);
        }

        /// <summary>
        /// The arm's longitudinal direction.
        ///
        /// The vertical component is <c>+sin</c> on BOTH sides: the free end rises either way, which is what
        /// makes the slope a camber rather than a tip-over. It is mirror symmetry about the X-Z plane, not a
        /// 180-degree rotation — that would invert the slope on the negative side, a defect invisible head-on
        /// and obvious in profile.
        /// </summary>
        public static Vector3D Axis(CantileverArmSide side, double angleRadians)
        {
            GeometryTolerance.RequireFinite(angleRadians, nameof(angleRadians));

            var run = Math.Cos(angleRadians);
            var rise = Math.Sin(angleRadians);

            switch (side)
            {
                case CantileverArmSide.PositiveY:
                    return new Vector3D(0.0, run, rise);
                case CantileverArmSide.NegativeY:
                    return new Vector3D(0.0, -run, rise);
                default:
                    throw Undefined(side);
            }
        }

        /// <summary>
        /// The frame of an arm member.
        ///
        /// <paramref name="transverseOffset"/> is the member's shift along the frame's own X axis, which is
        /// how the paired placements of
        /// <see cref="CantileverArmBodyArrangementResolver"/> are expressed in world terms.
        ///
        /// The depth axis is DERIVED, not written down: it is the component of world <c>+Z</c> perpendicular
        /// to the arm's axis, normalised. That is the same rule the column–base datum uses, and it is what
        /// makes the end plate's "up" fall out instead of being chosen.
        /// </summary>
        public static LocalFrame3D MemberFrame(
            CantileverArmSide side,
            CantileverArmBodyOrientation orientation,
            double angleRadians,
            Point3D origin,
            double transverseOffset)
        {
            if (!IsSupported(side))
            {
                throw Undefined(side);
            }

            GeometryTolerance.RequireFinite(transverseOffset, nameof(transverseOffset));

            switch (orientation)
            {
                case CantileverArmBodyOrientation.DepthPerpendicularToAxis:
                {
                    var axisZ = Axis(side, angleRadians);
                    var depth = DepthAxis(axisZ);

                    // LocalFrame3D.Create fixes AxisY = AxisZ x AxisX, so asking for a given AxisY means
                    // passing referenceX = AxisY x AxisZ. Written as the product rather than as a literal so
                    // the derivation stays visible.
                    var referenceX = depth.Cross(axisZ);
                    var frame = LocalFrame3D.Create(origin, axisZ, referenceX);

                    return transverseOffset == 0.0
                        ? frame
                        : LocalFrame3D.Create(
                            frame.Origin + (frame.AxisX * transverseOffset), axisZ, referenceX);
                }

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(orientation), orientation,
                        "La orientacion de cuerpo '" + orientation +
                        "' no tiene regla de marco. Anadir una orientacion exige escribirla aqui.");
            }
        }

        /// <summary>
        /// The "up" of an arm: world <c>+Z</c> projected onto the plane perpendicular to its axis.
        ///
        /// This is also the height direction of the end plate, which is why a stop grows visually upwards even
        /// on a sloped arm without anybody choosing a direction for it (ADR-0025, D7).
        /// </summary>
        public static Vector3D DepthAxis(Vector3D axisZ)
        {
            var z = axisZ.Normalized();
            var projected = Vector3D.UnitZ - (z * Vector3D.UnitZ.Dot(z));

            if (projected.Length <= 1e-8)
            {
                // Only reachable with a vertical arm, which no slope in range can produce: atan is bounded
                // below 90 degrees. Guarded anyway, because the alternative is a normalisation that throws
                // somewhere less obvious.
                throw new InvalidOperationException(
                    "El eje del brazo es vertical: no define una direccion de peralte.");
            }

            return projected.Normalized();
        }

        private static ArgumentOutOfRangeException Undefined(CantileverArmSide side) =>
            new ArgumentOutOfRangeException(
                nameof(side), side,
                "El lado de brazo '" + side + "' no tiene regla de marco. Anadir un lado exige escribirla aqui.");
    }
}
