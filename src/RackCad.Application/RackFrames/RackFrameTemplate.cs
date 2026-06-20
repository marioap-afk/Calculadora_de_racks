using System.Collections.Generic;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Defines the SHAPE of a header (how many horizontals and where, plus the default
    /// bracing) independently of the final dimensions. Elevations are reference values in
    /// inches; the factory scales them proportionally to the chosen height, so the top
    /// horizontal always lands exactly on the requested height.
    /// </summary>
    public sealed class RackFrameTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double DefaultHeight { get; set; }
        public double DefaultDepth { get; set; }

        /// <summary>Reference elevations in inches, ascending, starting at 0.</summary>
        public IReadOnlyList<double> HorizontalElevations { get; set; } = new List<double>();

        public BracingPattern DefaultArrangement { get; set; } = BracingPattern.SingleDiagonal;

        public override string ToString()
        {
            return Name ?? Id;
        }
    }
}
