namespace RackCad.Domain.RackFrames
{
    /// <summary>
    /// The ONE rule for a double-diagonal panel's two parallel diagonals, shared by the preview/BOM member builder and
    /// the lateral drawer so they can never drift apart. The diagonals span the full depth and are offset VERTICALLY by
    /// <paramref name="doubleSpacing"/> (a V-style celosía), each set back <paramref name="startOffset"/> from the panel's
    /// lower edge and <paramref name="endOffset"/> from its upper edge. Pure — no snapping (the caller snaps its own
    /// base elevations to the troquel grid before calling, if it can).
    /// </summary>
    public static class BracingDiagonalGeometry
    {
        public static DoubleDiagonalElevations DoubleDiagonal(
            double bottomElevation, double topElevation, double startOffset, double endOffset, double doubleSpacing)
        {
            return new DoubleDiagonalElevations(
                lowerStart: bottomElevation + startOffset,
                lowerEnd: topElevation - (endOffset + doubleSpacing),
                upperStart: bottomElevation + startOffset + doubleSpacing,
                upperEnd: topElevation - endOffset);
        }
    }

    /// <summary>The start/end elevations of the two parallel diagonals of a double-diagonal panel (lower + upper).</summary>
    public readonly struct DoubleDiagonalElevations
    {
        public DoubleDiagonalElevations(double lowerStart, double lowerEnd, double upperStart, double upperEnd)
        {
            LowerStart = lowerStart;
            LowerEnd = lowerEnd;
            UpperStart = upperStart;
            UpperEnd = upperEnd;
        }

        public double LowerStart { get; }
        public double LowerEnd { get; }
        public double UpperStart { get; }
        public double UpperEnd { get; }
    }
}
