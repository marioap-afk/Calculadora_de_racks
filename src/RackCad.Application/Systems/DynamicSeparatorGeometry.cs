using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Resolves the separator elevations of one dynamic lateral section. Drawing and BOM consume the same result so
    /// front-specific post heights cannot change the visible separators without changing their physical quantity.
    /// </summary>
    public static class DynamicSeparatorGeometry
    {
        public static IReadOnlyList<double> Levels(
            DynamicRackSystem system,
            RackCatalog catalog,
            double height)
        {
            if (system == null || height <= 0.0)
            {
                return Array.Empty<double>();
            }

            var configuration = system.Modules.FirstOrDefault(module => module.IsHeader
                && module.AssociatedFrameConfiguration != null)?.AssociatedFrameConfiguration;
            if (configuration == null)
            {
                return Array.Empty<double>();
            }

            var postId = configuration.LeftPost?.PostCatalogId;
            var troquel = CatalogLookup.Local(
                catalog,
                postId,
                DynamicRackDefaults.SeparatorPostPoint,
                DynamicRackDefaults.IntermediateBeamView);
            var paso = configuration.PasoTroquel > 0.0 ? configuration.PasoTroquel : 2.0;
            return SeparatorLevelCalculator.Levels(
                height,
                troquel.Y,
                paso,
                system.SeparatorCountOverride,
                system.SeparatorSpacingOverride);
        }
    }
}
