using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Pure, UI-independent model checks shared by the configurator. Lives in the
    /// Application layer so it is unit-testable without the WPF runtime. Returns
    /// human-readable warnings (Spanish) using the same tolerance the model uses for
    /// equality, so the warnings stay consistent with how the model treats elevations.
    /// </summary>
    public static class FrameModelValidator
    {
        /// <summary>True when <paramref name="candidate"/> is within tolerance of any existing elevation.</summary>
        public static bool CollidesWithExisting(IEnumerable<double> existingElevations, double candidate, double tolerance)
        {
            return existingElevations != null
                && existingElevations.Any(elevation => Math.Abs(elevation - candidate) <= tolerance);
        }

        public static IReadOnlyList<string> Validate(RackFrameConfiguration configuration, RackCatalog catalog, double tolerance)
        {
            var warnings = new List<string>();

            if (configuration == null)
            {
                return warnings;
            }

            var ordered = configuration.Horizontals
                .Where(horizontal => horizontal != null)
                .OrderBy(horizontal => horizontal.Elevation)
                .ToList();

            // Near-equal elevations, using the model's tolerance instead of exact rounding.
            for (var index = 1; index < ordered.Count; index++)
            {
                if (Math.Abs(ordered[index].Elevation - ordered[index - 1].Elevation) <= tolerance)
                {
                    warnings.Add("Elevacion duplicada o muy cercana en horizontales: " + FormatInches(ordered[index].Elevation) + ".");
                }
            }

            // The celosía starts at the configured start troquel (not at 0), so the lowest horizontal should
            // land there. Comparing against the expected troquel position keeps the check valid for the
            // parametric standard model (first travesaño = (CelosiaStartTroquel-1) * PasoTroquel).
            var paso = configuration.PasoTroquel > 0 ? configuration.PasoTroquel : 2.0;
            var expectedBase = Math.Max(0.0, (configuration.CelosiaStartTroquel - 1) * paso);
            if (ordered.Count > 0 && Math.Abs(ordered[0].Elevation - expectedBase) > tolerance)
            {
                warnings.Add("La horizontal inferior deberia estar en el troquel de inicio (" +
                    FormatInches(expectedBase) + "); esta en " + FormatInches(ordered[0].Elevation) + ".");
            }

            AddZeroHeightPanelWarnings(warnings, configuration, ordered, tolerance);
            AddReinforcementHeightWarnings(warnings, configuration);

            if (catalog != null)
            {
                AddUnknownCatalogIdWarnings(warnings, configuration, catalog);
            }

            return warnings;
        }

        private static void AddZeroHeightPanelWarnings(List<string> warnings, RackFrameConfiguration configuration, IList<FrameHorizontal> ordered, double tolerance)
        {
            var elevationById = ordered
                .GroupBy(horizontal => horizontal.Id ?? string.Empty)
                .ToDictionary(group => group.Key, group => group.First().Elevation);

            foreach (var panel in configuration.BracingPanels.Where(panel => panel != null))
            {
                if (elevationById.TryGetValue(panel.LowerHorizontalId ?? string.Empty, out var lower) &&
                    elevationById.TryGetValue(panel.UpperHorizontalId ?? string.Empty, out var upper) &&
                    Math.Abs(upper - lower) <= tolerance)
                {
                    var label = string.IsNullOrWhiteSpace(panel.PanelId)
                        ? panel.Number.ToString(CultureInfo.InvariantCulture)
                        : panel.PanelId;
                    warnings.Add("Panel " + label + " de altura insuficiente.");
                }
            }
        }

        /// <summary>A first structural sanity check (to grow gradually with capacity/holgura rules): a post reinforcement
        /// taller than the frame itself is physically impossible — the refuerzo cannot exceed the post it braces.</summary>
        private static void AddReinforcementHeightWarnings(List<string> warnings, RackFrameConfiguration configuration)
        {
            var height = configuration.Height;
            if (height <= 0.0)
            {
                return;
            }

            void Check(PostAssembly post, string side)
            {
                if (post != null && post.HasReinforcement && post.ReinforcementHeight > height + 1e-6)
                {
                    warnings.Add("El refuerzo del poste " + side + " (" + FormatInches(post.ReinforcementHeight)
                        + ") supera la altura del marco (" + FormatInches(height) + ").");
                }
            }

            Check(configuration.LeftPost, "izquierdo");
            Check(configuration.RightPost, "derecho");
        }

        private static void AddUnknownCatalogIdWarnings(List<string> warnings, RackFrameConfiguration configuration, RackCatalog catalog)
        {
            var unknown = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            void CheckProfile(IReadOnlyList<ProfileCatalogEntry> list, string id)
            {
                // Skip when the list is empty: a missing catalog cannot prove an id is unknown.
                if (list != null && list.Count > 0 && !string.IsNullOrWhiteSpace(id) && list.FindProfile(id) == null)
                {
                    unknown.Add(id.Trim());
                }
            }

            CheckProfile(catalog.PostProfiles, configuration.LeftPost?.PostCatalogId);
            CheckProfile(catalog.PostProfiles, configuration.RightPost?.PostCatalogId);

            // Reinforcements are posts, so their ids are validated against the post catalog.
            if (configuration.LeftPost != null && configuration.LeftPost.HasReinforcement)
            {
                CheckProfile(catalog.PostProfiles, configuration.LeftPost.ReinforcementCatalogId);
            }

            if (configuration.RightPost != null && configuration.RightPost.HasReinforcement)
            {
                CheckProfile(catalog.PostProfiles, configuration.RightPost.ReinforcementCatalogId);
            }

            if (catalog.BasePlates != null && catalog.BasePlates.Count > 0)
            {
                CheckBasePlate(unknown, catalog, configuration.LeftBasePlate?.PlateCatalogId);
                CheckBasePlate(unknown, catalog, configuration.RightBasePlate?.PlateCatalogId);
            }

            foreach (var horizontal in configuration.Horizontals.Where(horizontal => horizontal != null))
            {
                CheckProfile(catalog.TrussProfiles, horizontal.ProfileId);
            }

            foreach (var panel in configuration.BracingPanels.Where(panel => panel != null))
            {
                CheckProfile(catalog.TrussProfiles, panel.DiagonalProfileId);
            }

            if (catalog.ConnectionPoints != null && catalog.ConnectionPoints.Count > 0)
            {
                CheckConnectionPoint(unknown, catalog, configuration.LeftBasePlate?.ConnectionPointId);
                CheckConnectionPoint(unknown, catalog, configuration.RightBasePlate?.ConnectionPointId);

                foreach (var panel in configuration.BracingPanels.Where(panel => panel != null))
                {
                    CheckConnectionPoint(unknown, catalog, panel.StartConnectionPointId);
                    CheckConnectionPoint(unknown, catalog, panel.EndConnectionPointId);
                }
            }

            foreach (var id in unknown)
            {
                warnings.Add("Referencia desconocida en catalogo: " + id + ".");
            }
        }

        private static void CheckBasePlate(SortedSet<string> unknown, RackCatalog catalog, string id)
        {
            if (!string.IsNullOrWhiteSpace(id) && catalog.BasePlates.FindBasePlate(id) == null)
            {
                unknown.Add(id.Trim());
            }
        }

        private static void CheckConnectionPoint(SortedSet<string> unknown, RackCatalog catalog, string id)
        {
            if (!string.IsNullOrWhiteSpace(id) && catalog.ConnectionPoints.FindConnectionPoint(id) == null)
            {
                unknown.Add(id.Trim());
            }
        }

        private static string FormatInches(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture) + " in";
        }
    }
}
