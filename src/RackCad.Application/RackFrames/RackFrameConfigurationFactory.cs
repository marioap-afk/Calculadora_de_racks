using System;
using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.Catalogs;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Builds a <see cref="RackFrameConfiguration"/> from a self-describing template plus the
    /// dimensions (post type, height, depth). The template names every piece (horizontal profiles
    /// and quantities, diagonal profile, brace connection points, base plate, post); empty ids fall
    /// back to <see cref="RackDefaults"/>. No catalog ids or profile rules are hardcoded here, so the
    /// frame is fully defined by the JSON catalogs.
    /// </summary>
    public sealed class RackFrameConfigurationFactory
    {
        /// <summary>
        /// The post ends this many inches above the TOP celosía horizontal (the "remate"). So the total height is
        /// <c>topHorizontal + PostTopRemate</c>: the elevations are computed so the top horizontal lands exactly at
        /// <c>height - PostTopRemate</c>, which makes the built height equal the requested height (240 in → 240 in,
        /// not 242). Shared with the configurator VM so both agree on what "height" means.
        /// </summary>
        public const double PostTopRemate = 4.0;

        /// <summary>Largest leftover (in) above the last standard travesaño that a SINGLE closing travesaño
        /// covers; a bigger gap takes two closings so no panel exceeds a safe unbraced clear.</summary>
        private const double SingleClosingMaxGap = 24.0;

        private readonly RackCatalog catalog;

        public RackFrameConfigurationFactory()
            : this(LoadDefaultCatalog())
        {
        }

        public RackFrameConfigurationFactory(RackCatalog catalog)
        {
            this.catalog = catalog ?? new RackCatalog();
        }

        public RackFrameConfiguration Build(RackFrameTemplate template, string postCatalogId, double height, double depth)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            var horizontals = template.Horizontals;

            if (horizontals == null || horizontals.Count < 2)
            {
                throw new ArgumentException("La plantilla debe tener al menos dos horizontales.", nameof(template));
            }

            for (var index = 1; index < horizontals.Count; index++)
            {
                if (horizontals[index].Elevation - horizontals[index - 1].Elevation <= 1e-4)
                {
                    throw new ArgumentException(
                        "La plantilla debe tener elevaciones estrictamente ascendentes y distintas.", nameof(template));
                }
            }

            var maxReferenceElevation = horizontals[horizontals.Count - 1].Elevation;

            if (maxReferenceElevation <= 0.0)
            {
                throw new ArgumentException("La ultima elevacion de la plantilla debe ser mayor que cero.", nameof(template));
            }

            if (height <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "El alto debe ser mayor que cero.");
            }

            if (depth <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), "El fondo debe ser mayor que cero.");
            }

            var defaults = catalog.Defaults ?? new RackDefaults();
            var postId = FirstNonEmpty(postCatalogId, template.Post, defaults.Post);
            var plateId = FirstNonEmpty(template.BasePlate, defaults.BasePlate);
            var diagonalId = FirstNonEmpty(template.DiagonalProfile, defaults.DiagonalProfile);
            var braceStartId = FirstNonEmpty(template.BraceStartConnectionPoint, defaults.BraceStartConnectionPoint);
            var braceEndId = FirstNonEmpty(template.BraceEndConnectionPoint, defaults.BraceEndConnectionPoint);
            var basePlate = catalog.BasePlates.FindBasePlate(plateId);
            // The plate's mate-to-post anchor now comes from the connection-layout table (a plate can
            // declare several points per view); fall back to the global default.
            var plateConnectionId = FirstNonEmpty(catalog.MountConnectionPointId(plateId), defaults.BasePlateConnectionPoint);

            var configuration = new RackFrameConfiguration
            {
                Name = template.Name,
                Units = "in",
                Height = height,
                Depth = depth,
                StandardBaselineId = "TEMPLATE-" + (template.Id ?? "CUSTOM"),
                StandardBaselineVersion = "0.1",
                LeftPost = CreatePost(PostSide.Left, postId),
                RightPost = CreatePost(PostSide.Right, postId),
                LeftBasePlate = CreateBasePlate(PostSide.Left, plateId, basePlate, plateConnectionId),
                RightBasePlate = CreateBasePlate(PostSide.Right, plateId, basePlate, plateConnectionId)
            };

            // Parametric standard model: the first travesaño sits at the celosía troquel, then panels of
            // PanelClear, then 0/1/2 closing travesaños that absorb the leftover while clearing the post top.
            // Closing panels carry no diagonal. The shape no longer comes from the template's elevations (it
            // is computed), but the template still supplies the profiles, plate, post and connection points.
            var horizontalProfileId = FirstNonEmpty(horizontals[0].Profile, defaults.HorizontalProfile, diagonalId);
            var elevations = ComputeStandardElevations(
                height, configuration.CelosiaStartTroquel, configuration.PasoTroquel, configuration.PanelClear, PostTopRemate, out var standardCount);

            for (var index = 0; index < elevations.Count; index++)
            {
                AddHorizontal(configuration, index + 1, elevations[index], horizontalProfileId, quantity: 1);
            }

            for (var index = 0; index < elevations.Count - 1; index++)
            {
                var lowerId = "H" + (index + 1).ToString(CultureInfo.InvariantCulture);
                var upperId = "H" + (index + 2).ToString(CultureInfo.InvariantCulture);
                // Standard panels get the template's diagonal; the closing panels above them carry none.
                var arrangement = index < standardCount - 1 ? template.DefaultArrangement : BracingPattern.NoBracing;
                AddPanel(configuration, index + 1, lowerId, upperId, arrangement, diagonalId, braceStartId, braceEndId);
            }

            return configuration;
        }

        /// <summary>
        /// Standard celosía elevations: the first travesaño at the start troquel, then one every
        /// <paramref name="claro"/> up to the top target, then 0/1/2 closing travesaños that land the TOP horizontal
        /// exactly at <c>height - <paramref name="remate"/></c>. <paramref name="standardCount"/> is the number of
        /// evenly-spaced travesaños before the closings (the panels among them carry diagonals). Landing the top at
        /// <c>height - remate</c> makes the built height equal the requested one (post = top horizontal + remate).
        /// </summary>
        private static IReadOnlyList<double> ComputeStandardElevations(
            double height, int troquelStart, double paso, double claro, double remate, out int standardCount)
        {
            if (paso <= 0.0) paso = 2.0;
            if (claro <= 0.0) claro = 44.0;
            if (troquelStart < 1) troquelStart = 1;
            if (remate < 0.0) remate = 0.0;

            var yFirst = (troquelStart - 1) * paso;
            // The top horizontal sits `remate` below the post top, so target it there; then the built height = the
            // requested height (top + remate) instead of drifting a troquel above it.
            var topTarget = height - remate;
            var elevations = new List<double>();

            if (topTarget <= yFirst)
            {
                standardCount = 1;
                elevations.Add(yFirst < height ? Math.Round(yFirst, 4) : 0.0);
                return elevations;
            }

            standardCount = (int)Math.Floor((topTarget - yFirst) / claro) + 1;

            for (var index = 0; index < standardCount; index++)
            {
                elevations.Add(Math.Round(yFirst + index * claro, 4));
            }

            var lastStandard = yFirst + (standardCount - 1) * claro;
            var gap = topTarget - lastStandard; // in [0, claro): the distance still to reach the top target

            if (gap <= 1e-4)
            {
                // The last standard IS the top target (a "perfect" height); no closing needed.
                return elevations;
            }

            if (gap <= SingleClosingMaxGap)
            {
                // One closing travesaño AT the top target.
                elevations.Add(Math.Round(topTarget, 4));
            }
            else
            {
                // Two closings: a middle one on an even troquel, then the top target.
                var middle = lastStandard + Math.Floor(gap / 2.0 / paso) * paso;
                if (middle <= lastStandard + 1e-4) middle = lastStandard + paso;
                if (middle >= topTarget - 1e-4) middle = topTarget - paso;
                elevations.Add(Math.Round(middle, 4));
                elevations.Add(Math.Round(topTarget, 4));
            }

            return elevations;
        }

        private PostAssembly CreatePost(PostSide side, string postCatalogId)
        {
            var profile = catalog.PostProfiles.FindProfile(postCatalogId);

            return new PostAssembly
            {
                Side = side,
                PostCatalogId = postCatalogId,
                Description = NormalizeOrFallback(profile?.Description, postCatalogId),
                HasReinforcement = false
            };
        }

        private static BasePlatePlacement CreateBasePlate(PostSide side, string plateCatalogId, BasePlateCatalogEntry plate, string connectionPointId)
        {
            return new BasePlatePlacement
            {
                PostSide = side,
                PlateCatalogId = plateCatalogId,
                Description = NormalizeOrFallback(plate?.Description, "Placa base atornillable"),
                ConnectionPointId = connectionPointId
            };
        }

        private static void AddHorizontal(RackFrameConfiguration configuration, int number, double elevation, string profileId, int quantity)
        {
            configuration.Horizontals.Add(new FrameHorizontal
            {
                Id = "H" + number.ToString(CultureInfo.InvariantCulture),
                Number = number,
                Elevation = elevation,
                ProfileId = profileId,
                Quantity = quantity,
                MountingFace = FrameSide.Front,
                State = FrameComponentState.Standard,
                IsStandard = true
            });
        }

        private static void AddPanel(
            RackFrameConfiguration configuration,
            int number,
            string lowerHorizontalId,
            string upperHorizontalId,
            BracingPattern arrangement,
            string diagonalProfileId,
            string braceStartConnectionPointId,
            string braceEndConnectionPointId)
        {
            configuration.BracingPanels.Add(new BracingPanel
            {
                PanelId = "P" + number.ToString(CultureInfo.InvariantCulture),
                Number = number,
                LowerHorizontalId = lowerHorizontalId,
                UpperHorizontalId = upperHorizontalId,
                Arrangement = arrangement,
                MountingFace = FrameSide.Front,
                DiagonalProfileId = diagonalProfileId,
                DiagonalDirection = DiagonalDirection.AutoAlternating,
                StartConnectionPointId = braceStartConnectionPointId,
                EndConnectionPointId = braceEndConnectionPointId,
                IsStandard = true,
                IsException = false
            });
        }

        private static RackCatalog LoadDefaultCatalog()
        {
            try
            {
                return JsonRackCatalogProvider.FromBaseDirectory().Load();
            }
            catch
            {
                return new RackCatalog();
            }
        }

        private static string FirstNonEmpty(params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate.Trim();
                }
            }

            return string.Empty;
        }

        private static string NormalizeText(string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        private static string NormalizeOrFallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
