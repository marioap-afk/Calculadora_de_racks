using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Built-in header templates used as a FALLBACK when the external
    /// <c>header-templates.json</c> catalog is missing or empty. The JSON catalog is the
    /// authoritative source at runtime; these keep the configurator usable without it.
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
                HorizontalElevations = new[] { 0.0, 44.0, 88.0, 132.0 },
                DefaultArrangement = BracingPattern.SingleDiagonal
            },
            new RackFrameTemplate
            {
                Id = "COMPACT-2P",
                Name = "Compacta (2 paneles)",
                DefaultHeight = 96.0,
                DefaultDepth = 36.0,
                HorizontalElevations = new[] { 0.0, 48.0, 96.0 },
                DefaultArrangement = BracingPattern.SingleDiagonal
            },
            new RackFrameTemplate
            {
                Id = "TALL-4P",
                Name = "Alta (4 paneles, X)",
                DefaultHeight = 180.0,
                DefaultDepth = 42.0,
                HorizontalElevations = new[] { 0.0, 45.0, 90.0, 135.0, 180.0 },
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
