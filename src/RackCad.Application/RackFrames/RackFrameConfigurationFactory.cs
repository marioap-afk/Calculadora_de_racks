using System;
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
            var plateConnectionId = FirstNonEmpty(basePlate?.ConnectionPointId, defaults.BasePlateConnectionPoint);

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

            for (var index = 0; index < horizontals.Count; index++)
            {
                var horizontal = horizontals[index];
                var ratio = Math.Clamp(horizontal.Elevation / maxReferenceElevation, 0.0, 1.0);
                var elevation = Math.Round(height * ratio, 4);
                AddHorizontal(configuration, index + 1, elevation, NormalizeText(horizontal.Profile), Math.Max(1, horizontal.Quantity));
            }

            for (var index = 0; index < horizontals.Count - 1; index++)
            {
                var lowerId = "H" + (index + 1).ToString(CultureInfo.InvariantCulture);
                var upperId = "H" + (index + 2).ToString(CultureInfo.InvariantCulture);
                AddPanel(configuration, index + 1, lowerId, upperId, template.DefaultArrangement, diagonalId, braceStartId, braceEndId);
            }

            return configuration;
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
