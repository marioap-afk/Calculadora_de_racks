using System.Collections.Generic;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Defines the SHAPE of a header (how many horizontals and where, plus the default
    /// bracing) independently of the concrete dimensions. Elevations are expressed as
    /// ratios of the total height so the same template scales to any height.
    /// </summary>
    public sealed class RackFrameTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double DefaultHeight { get; set; }
        public double DefaultDepth { get; set; }

        /// <summary>Ascending ratios in [0,1]; must start at 0 and end at 1.</summary>
        public IReadOnlyList<double> HorizontalElevationRatios { get; set; } = new List<double>();

        public BracingPattern DefaultArrangement { get; set; } = BracingPattern.SingleDiagonal;

        public override string ToString()
        {
            return Name ?? Id;
        }
    }
}
