using System;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>A vector or a position in the coordinate system of the analysis program.</summary>
    public readonly struct RamVector
    {
        public RamVector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }
    }

    /// <summary>The local axes of a member: <c>L1</c> along the member, <c>L2</c> and <c>L3</c> across it.</summary>
    public readonly struct RamAxesFrame
    {
        public RamAxesFrame(RamVector l1, RamVector l2, RamVector l3, bool isVertical)
        {
            L1 = l1;
            L2 = l2;
            L3 = l3;
            IsVertical = isVertical;
        }

        public RamVector L1 { get; }

        public RamVector L2 { get; }

        public RamVector L3 { get; }

        /// <summary>True when the member is vertical under the rule of the table (horizontal component of <c>L1</c> at most 1e-12).</summary>
        public bool IsVertical { get; }
    }

    /// <summary>
    /// The rule of the local axes and of the twist (I-65 D-04, INV-07), measured in P1: pure functions of the direction of the member.
    ///
    /// <para>
    /// With <c>L1</c> the unit direction from node I to node J and Y = (0, 1, 0) the vertical of the analysis program: a vertical
    /// member (horizontal component of <c>L1</c> at most 1e-12) has <c>L3</c> = +Z; any other member has
    /// <c>L3</c> = unit(<c>L1</c> x Y); in both cases <c>L2</c> = <c>L3</c> x <c>L1</c>. The twist turns the frame about <c>L1</c>
    /// (right-handed): <c>L2</c> = cos(t) <c>L2</c>0 + sin(t) <c>L3</c>0 and <c>L3</c> = -sin(t) <c>L2</c>0 + cos(t) <c>L3</c>0.
    /// </para>
    /// </summary>
    public static class RamLocalAxes
    {
        /// <summary>The largest horizontal component of a unit <c>L1</c> that still counts as vertical.</summary>
        public const double VerticalTolerance = 1e-12;

        /// <summary>The largest inclination, from the horizontal, of a non-vertical member that V1 admits.</summary>
        public const double MaxInclinationDegrees = 10.0;

        /// <summary>The axes of a member with no twist, from the direction of node I to node J.</summary>
        public static RamAxesFrame ZeroTwist(RamVector direction)
        {
            var l1 = Unit(direction);
            var vertical = Horizontal(l1) <= VerticalTolerance;
            var l3 = vertical ? new RamVector(0.0, 0.0, 1.0) : Unit(Cross(l1, new RamVector(0.0, 1.0, 0.0)));

            return new RamAxesFrame(l1, Cross(l3, l1), l3, vertical);
        }

        /// <summary>True when the orientation is admitted in V1: vertical, or at most 10 degrees from the horizontal.</summary>
        public static bool IsAdmitted(RamVector direction)
        {
            var l1 = Unit(direction);
            var horizontal = Horizontal(l1);

            if (horizontal <= VerticalTolerance)
            {
                return true;
            }

            return Math.Atan2(Math.Abs(l1.Y), horizontal) * (180.0 / Math.PI) <= MaxInclinationDegrees;
        }

        /// <summary>The twist, in degrees, that turns <c>L2</c> of the zero-twist frame onto <paramref name="depth"/>.</summary>
        public static double ThetaDegrees(RamAxesFrame zeroTwist, RamVector depth)
        {
            // + 0.0 turns a negative zero into zero, so that the same twist is always written the same way.
            return (Math.Atan2(Dot(depth, zeroTwist.L3), Dot(depth, zeroTwist.L2)) * (180.0 / Math.PI)) + 0.0;
        }

        /// <summary>The frame after turning <paramref name="degrees"/> about <c>L1</c>.</summary>
        public static RamAxesFrame Rotate(RamAxesFrame zeroTwist, double degrees)
        {
            var radians = degrees * (Math.PI / 180.0);
            var cosine = Math.Cos(radians);
            var sine = Math.Sin(radians);

            return new RamAxesFrame(
                zeroTwist.L1,
                Combine(cosine, zeroTwist.L2, sine, zeroTwist.L3),
                Combine(-sine, zeroTwist.L2, cosine, zeroTwist.L3),
                zeroTwist.IsVertical);
        }

        internal static double Dot(RamVector a, RamVector b) => (a.X * b.X) + (a.Y * b.Y) + (a.Z * b.Z);

        internal static RamVector Cross(RamVector a, RamVector b) =>
            new RamVector((a.Y * b.Z) - (a.Z * b.Y), (a.Z * b.X) - (a.X * b.Z), (a.X * b.Y) - (a.Y * b.X));

        internal static double Length(RamVector v) => Math.Sqrt(Dot(v, v));

        internal static RamVector Unit(RamVector v)
        {
            var length = Length(v);

            if (!(length > 0) || double.IsInfinity(length))
            {
                throw new ArgumentException("A direction needs a finite, non-zero length.", nameof(v));
            }

            return new RamVector((v.X / length) + 0.0, (v.Y / length) + 0.0, (v.Z / length) + 0.0);
        }

        private static double Horizontal(RamVector unit) => Math.Sqrt((unit.X * unit.X) + (unit.Z * unit.Z));

        private static RamVector Combine(double a, RamVector u, double b, RamVector v) =>
            new RamVector(((a * u.X) + (b * v.X)) + 0.0, ((a * u.Y) + (b * v.Y)) + 0.0, ((a * u.Z) + (b * v.Z)) + 0.0);
    }
}
