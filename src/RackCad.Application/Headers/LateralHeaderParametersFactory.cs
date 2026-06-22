using System;
using System.Linq;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.Headers
{
    /// <summary>
    /// Maps an editor <see cref="RackFrameConfiguration"/> onto the pure <see cref="LateralHeaderParameters"/>
    /// the builder consumes. Kept pure (no AutoCAD) so the Plugin command stays a thin caller and the
    /// mapping is unit-tested on any OS. Catalog ids come straight from the configured posts/plate/horizontals,
    /// so the lateral header always draws what the user configured — no literals duplicated here.
    /// </summary>
    public static class LateralHeaderParametersFactory
    {
        /// <summary>Panel clear used when the configuration has fewer than two distinct horizontals.</summary>
        public const double DefaultClaroPanel = 44.0;

        public static LateralHeaderParameters FromConfiguration(RackFrameConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return new LateralHeaderParameters
            {
                Height = configuration.Height,
                Depth = configuration.Depth,
                PasoTroquel = configuration.PasoTroquel > 0 ? configuration.PasoTroquel : 2.0,
                InicioCelosiaTroquel = configuration.CelosiaStartTroquel,
                ClaroPanel = ResolveClaroPanel(configuration),
                OffsetDiagonalInicioTroqueles = configuration.DiagonalStartOffsetTroqueles,
                OffsetDiagonalFinTroqueles = configuration.DiagonalEndOffsetTroqueles,
                DiagonalDoubleSpacingTroqueles = configuration.DiagonalDoubleSpacingTroqueles,
                HorizontalDoubleOffsetTroqueles = configuration.HorizontalDoubleOffsetTroqueles,
                PostId = configuration.LeftPost?.PostCatalogId,
                BasePlateId = configuration.LeftBasePlate?.PlateCatalogId,
                TrussProfileId = ResolveTrussProfileId(configuration)
            };
        }

        /// <summary>
        /// The standard panel clear is the first positive gap between consecutive horizontals; this keeps
        /// the lateral builder in step with whatever spacing the user configured (the configurator allows
        /// non-uniform panels, so we take the first real gap rather than assume 44").
        /// </summary>
        private static double ResolveClaroPanel(RackFrameConfiguration configuration)
        {
            var elevations = configuration.Horizontals?
                .Select(horizontal => horizontal.Elevation)
                .OrderBy(elevation => elevation)
                .ToList();

            if (elevations != null)
            {
                for (var index = 1; index < elevations.Count; index++)
                {
                    var gap = elevations[index] - elevations[index - 1];
                    if (gap > 1e-4)
                    {
                        return gap;
                    }
                }
            }

            return DefaultClaroPanel;
        }

        /// <summary>
        /// Horizontals and diagonals share a single celosía/truss profile (see <c>CatalogIds</c>); take the
        /// first non-empty horizontal profile, falling back to a panel's diagonal profile.
        /// </summary>
        private static string ResolveTrussProfileId(RackFrameConfiguration configuration)
        {
            var fromHorizontal = configuration.Horizontals?
                .Select(horizontal => horizontal.ProfileId)
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

            if (!string.IsNullOrWhiteSpace(fromHorizontal))
            {
                return fromHorizontal;
            }

            return configuration.BracingPanels?
                .Select(panel => panel.DiagonalProfileId)
                .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        }
    }
}
