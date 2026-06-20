using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Built-in header templates offered by the simple ("quick") configurator mode.
    /// Kept in code for now; can move to a versioned JSON catalog later without changing
    /// the factory contract.
    /// </summary>
    public static class RackFrameTemplateCatalog
    {
        public static IReadOnlyList<RackFrameTemplate> All { get; } = new List<RackFrameTemplate>
        {
            new RackFrameTemplate
            {
                Id = "STD-3P",
                Name = "Estandar (3 paneles)",
                DefaultHeight = 132.0,
                DefaultDepth = 42.0,
                HorizontalElevationRatios = new[] { 0.0, 1.0 / 3.0, 2.0 / 3.0, 1.0 },
                DefaultArrangement = BracingPattern.SingleDiagonal
            },
            new RackFrameTemplate
            {
                Id = "COMPACT-2P",
                Name = "Compacta (2 paneles)",
                DefaultHeight = 96.0,
                DefaultDepth = 36.0,
                HorizontalElevationRatios = new[] { 0.0, 0.5, 1.0 },
                DefaultArrangement = BracingPattern.SingleDiagonal
            },
            new RackFrameTemplate
            {
                Id = "TALL-4P",
                Name = "Alta (4 paneles, X)",
                DefaultHeight = 180.0,
                DefaultDepth = 42.0,
                HorizontalElevationRatios = new[] { 0.0, 0.25, 0.5, 0.75, 1.0 },
                DefaultArrangement = BracingPattern.XBracing
            }
        };

        public static RackFrameTemplate Default => All[0];

        public static RackFrameTemplate FindById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return All.FirstOrDefault(template =>
                string.Equals(template.Id, id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
