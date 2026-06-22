using System.Collections.Generic;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>One horizontal of a template: its reference elevation (in), the profile and quantity.</summary>
    public sealed class TemplateHorizontal
    {
        public double Elevation { get; set; }
        public string Profile { get; set; }
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// Self-describing header template. Beyond the shape (which horizontals, at which reference
    /// elevations, with which profile and quantity), it names the diagonal profile, brace
    /// connection points, base plate and post it uses. The factory reads everything from here, so a
    /// template fully defines its frame with no hardcoded ids. Empty ids fall back to the catalog
    /// defaults. Elevations are reference values (in) scaled by the chosen height.
    /// </summary>
    public sealed class RackFrameTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double DefaultHeight { get; set; }
        public double DefaultDepth { get; set; }

        /// <summary>Horizontals (ascending by elevation, starting at 0).</summary>
        public IReadOnlyList<TemplateHorizontal> Horizontals { get; set; } = new List<TemplateHorizontal>();

        public BracingPattern DefaultArrangement { get; set; } = BracingPattern.SingleDiagonal;
        public string DiagonalProfile { get; set; }
        public string BraceStartConnectionPoint { get; set; }
        public string BraceEndConnectionPoint { get; set; }
        public string BasePlate { get; set; }
        public string Post { get; set; }

        public override string ToString()
        {
            return Name ?? Id;
        }
    }
}
