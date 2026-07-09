using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Headers
{
    /// <summary>The computed plan for a lateral header: every block to insert plus a few useful totals.</summary>
    public sealed class LateralHeaderLayout
    {
        public LateralHeaderLayout(
            IReadOnlyList<HeaderBlockInstance> instances,
            double horizontalLength,
            int horizontalCount,
            int diagonalCount,
            double closingGap)
        {
            Instances = instances;
            HorizontalLength = horizontalLength;
            HorizontalCount = horizontalCount;
            DiagonalCount = diagonalCount;
            ClosingGap = closingGap;
        }

        public IReadOnlyList<HeaderBlockInstance> Instances { get; }

        /// <summary>Span each horizontal/diagonal covers between the two posts' troquel lines (in).</summary>
        public double HorizontalLength { get; }

        /// <summary>Standard horizontals (not counting the closing one).</summary>
        public int HorizontalCount { get; }

        public int DiagonalCount { get; }

        /// <summary>ValorClaroTravesaño actually used: leftover clear at the top (0 if none).</summary>
        public double ClosingGap { get; }

        public IEnumerable<HeaderBlockInstance> OfRole(HeaderBlockRole role) =>
            Instances.Where(instance => instance.Role == role);
    }
}
