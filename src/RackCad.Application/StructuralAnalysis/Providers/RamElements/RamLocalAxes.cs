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
    /// </summary>
    public static class RamLocalAxes
    {
        /// <summary>The axes of a member with no twist, from the direction of node I to node J.</summary>
        public static RamAxesFrame ZeroTwist(RamVector direction) => throw new NotImplementedException();

        /// <summary>True when the orientation is admitted in V1: vertical, or at most 10 degrees from the horizontal.</summary>
        public static bool IsAdmitted(RamVector direction) => throw new NotImplementedException();

        /// <summary>The twist, in degrees, that turns <c>L2</c> of the zero-twist frame onto <paramref name="depth"/>.</summary>
        public static double ThetaDegrees(RamAxesFrame zeroTwist, RamVector depth) => throw new NotImplementedException();

        /// <summary>The frame after turning <paramref name="degrees"/> about <c>L1</c>.</summary>
        public static RamAxesFrame Rotate(RamAxesFrame zeroTwist, double degrees) => throw new NotImplementedException();
    }
}
