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
        /// <summary>
        /// How short the projection of world +Z onto the arm's transverse plane may get before the frame is
        /// undefined. It is the SAME bound <see cref="TryDepthAxis"/> applies, shared so the gate in the
        /// resolver and the computation itself can never disagree.
        /// </summary>
        public const double DegenerateProjection = 1e-8;

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
            if (!TryDepthAxis(axisZ, out var depth))
            {
                throw new InvalidOperationException(
                    "El eje del brazo es practicamente vertical: no define una direccion de peralte.");
            }

            return depth;
        }

        /// <summary>
        /// The same computation, reporting failure instead of throwing.
        ///
        /// It exists because <c>atan</c> is bounded below 90 degrees but CONVERGES on it: a finite,
        /// non-negative <c>SlopeRisePer12</c> — <c>double.MaxValue</c>, say — produces an axis that is
        /// vertical to within floating point, and the projection of world +Z onto its transverse plane
        /// vanishes. That is user input, so it has to come back as a diagnostic and never as an exception.
        /// </summary>
        public static bool TryDepthAxis(Vector3D axisZ, out Vector3D depthAxis)
        {
            depthAxis = default;

            if (!axisZ.IsFinite || axisZ.Length <= DegenerateProjection)
            {
                return false;
            }

            var z = axisZ.Normalized();
            var projected = Vector3D.UnitZ - (z * Vector3D.UnitZ.Dot(z));

            if (projected.Length <= DegenerateProjection)
            {
                return false;
            }

            depthAxis = projected.Normalized();
            return true;
        }

        /// <summary>
        /// Whether a slope produces an arm whose frame can be built at all.
        ///
        /// The bound is the GEOMETRIC tolerance the projection itself uses — not an invented commercial
        /// maximum. A product limit on how steep an arm may be is a decision nobody has taken, and pretending
        /// otherwise here would put a number in the code that no owner approved.
        /// </summary>
        public static bool IsRepresentableSlope(CantileverArmSide side, double slopeRisePer12)
        {
            if (!IsSupported(side) ||
                !GeometryTolerance.IsFinite(slopeRisePer12) ||
                slopeRisePer12 < 0.0)
            {
                return false;
            }

            return TryDepthAxis(Axis(side, AngleRadians(slopeRisePer12)), out _);
        }

        private static ArgumentOutOfRangeException Undefined(CantileverArmSide side) =>
            new ArgumentOutOfRangeException(
                nameof(side), side,
                "El lado de brazo '" + side + "' no tiene regla de marco. Anadir un lado exige escribirla aqui.");
    }
}
