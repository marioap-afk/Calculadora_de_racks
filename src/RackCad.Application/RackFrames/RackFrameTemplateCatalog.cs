using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Built-in header templates used as a FALLBACK when <c>header-templates.json</c> is missing or
    /// empty. The JSON catalog is the authoritative source at runtime; these keep the configurator
    /// usable without it. Ids mirror the catalog so the built-ins are self-describing too.
    /// </summary>
    public static class RackFrameTemplateCatalog
    {
        /// <summary>Id of the standard template — the seed every default cabecera starts from.</summary>
        public const string StandardTemplateId = "STD-3P";

        /// <summary>The standard template if present, else the first available (never null).</summary>
        public static RackFrameTemplate FindStandardOrDefault() => FindById(StandardTemplateId) ?? Default;

        public static IReadOnlyList<RackFrameTemplate> All { get; } = new List<RackFrameTemplate>
        {
            new RackFrameTemplate
            {
                Id = StandardTemplateId,
                Name = "Estandar (3 paneles)",
                DefaultHeight = 132.0,
                DefaultDepth = 42.0,
                Horizontals = new[]
                {
                    new TemplateHorizontal { Elevation = 0.0, Profile = CatalogIds.LowerHorizontal, Quantity = 2 },
                    new TemplateHorizontal { Elevation = 44.0, Profile = CatalogIds.IntermediateHorizontal, Quantity = 1 },
                    new TemplateHorizontal { Elevation = 88.0, Profile = CatalogIds.IntermediateHorizontal, Quantity = 1 },
                    new TemplateHorizontal { Elevation = 132.0, Profile = CatalogIds.UpperHorizontal, Quantity = 1 }
                },
                DefaultArrangement = BracingPattern.SingleDiagonal,
                DiagonalProfile = CatalogIds.Diagonal,
                BraceStartConnectionPoint = CatalogIds.BraceStartConnectionPoint,
                BraceEndConnectionPoint = CatalogIds.BraceEndConnectionPoint,
                BasePlate = CatalogIds.BasePlate,
                Post = CatalogIds.StandardPost
            },
            new RackFrameTemplate
            {
                Id = "COMPACT-2P",
                Name = "Compacta (2 paneles)",
                DefaultHeight = 96.0,
                DefaultDepth = 36.0,
                Horizontals = new[]
                {
                    new TemplateHorizontal { Elevation = 0.0, Profile = CatalogIds.LowerHorizontal, Quantity = 2 },
                    new TemplateHorizontal { Elevation = 48.0, Profile = CatalogIds.IntermediateHorizontal, Quantity = 1 },
                    new TemplateHorizontal { Elevation = 96.0, Profile = CatalogIds.UpperHorizontal, Quantity = 1 }
                },
                DefaultArrangement = BracingPattern.SingleDiagonal,
                DiagonalProfile = CatalogIds.Diagonal,
                BraceStartConnectionPoint = CatalogIds.BraceStartConnectionPoint,
                BraceEndConnectionPoint = CatalogIds.BraceEndConnectionPoint,
                BasePlate = CatalogIds.BasePlate,
                Post = CatalogIds.StandardPost
            },
            new RackFrameTemplate
            {
                Id = "TALL-4P",
                Name = "Alta (4 paneles, X)",
                DefaultHeight = 180.0,
                DefaultDepth = 42.0,
                Horizontals = new[]
                {
                    new TemplateHorizontal { Elevation = 0.0, Profile = CatalogIds.LowerHorizontal, Quantity = 2 },
                    new TemplateHorizontal { Elevation = 45.0, Profile = CatalogIds.IntermediateHorizontal, Quantity = 1 },
                    new TemplateHorizontal { Elevation = 90.0, Profile = CatalogIds.IntermediateHorizontal, Quantity = 1 },
                    new TemplateHorizontal { Elevation = 135.0, Profile = CatalogIds.IntermediateHorizontal, Quantity = 1 },
                    new TemplateHorizontal { Elevation = 180.0, Profile = CatalogIds.UpperHorizontal, Quantity = 1 }
                },
                DefaultArrangement = BracingPattern.XBracing,
                DiagonalProfile = CatalogIds.Diagonal,
                BraceStartConnectionPoint = CatalogIds.BraceStartConnectionPoint,
                BraceEndConnectionPoint = CatalogIds.BraceEndConnectionPoint,
                BasePlate = CatalogIds.BasePlate,
                Post = CatalogIds.StandardPost
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
