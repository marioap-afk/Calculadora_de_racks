using System;
using System.Globalization;
using RackCad.Application.Catalogs;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Builds a <see cref="RackFrameConfiguration"/> from a template plus the four simple
    /// inputs (template, post type, height, depth). Descriptions are resolved from the
    /// catalog; the structure comes from the template scaled to the requested height.
    /// </summary>
    public sealed class RackFrameConfigurationFactory
    {
        private const string BasePlateCatalogId = "PLACA_BASE_ATORNILLABLE";
        private const string DiagonalProfileId = "TRAVESANO_DINAMICO_OMEGA_3X3";
        private const string LowerHorizontalProfileId = "HORIZONTAL_INFERIOR";
        private const string IntermediateHorizontalProfileId = "HORIZONTAL_INTERMEDIA";
        private const string UpperHorizontalProfileId = "HORIZONTAL_SUPERIOR";
        private const string BraceStartConnectionPointId = "TroquelCelosia_01";
        private const string BraceEndConnectionPointId = "TroquelCelosia_02";
        private const string FallbackBasePlateConnectionPointId = "PlacaBase_01";
        private const string FallbackPostCatalogId = "POSTE_OMEGA_3X3";

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

            var referenceElevations = template.HorizontalElevations;

            if (referenceElevations == null || referenceElevations.Count < 2)
            {
                throw new ArgumentException("La plantilla debe tener al menos dos horizontales.", nameof(template));
            }

            var maxReferenceElevation = referenceElevations[referenceElevations.Count - 1];

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

            var resolvedPostId = string.IsNullOrWhiteSpace(postCatalogId) ? FallbackPostCatalogId : postCatalogId.Trim();
            var basePlate = catalog.BasePlates.FindBasePlate(BasePlateCatalogId);
            var basePlateConnectionPointId = NormalizeOrFallback(basePlate?.ConnectionPointId, FallbackBasePlateConnectionPointId);

            var configuration = new RackFrameConfiguration
            {
                Name = template.Name,
                Units = "in",
                Height = height,
                Depth = depth,
                StandardBaselineId = "TEMPLATE-" + (template.Id ?? "CUSTOM"),
                StandardBaselineVersion = "0.1",
                LeftPost = CreatePost(PostSide.Left, resolvedPostId),
                RightPost = CreatePost(PostSide.Right, resolvedPostId),
                LeftBasePlate = CreateBasePlate(PostSide.Left, basePlate, basePlateConnectionPointId),
                RightBasePlate = CreateBasePlate(PostSide.Right, basePlate, basePlateConnectionPointId)
            };

            for (var index = 0; index < referenceElevations.Count; index++)
            {
                var ratio = Math.Clamp(referenceElevations[index] / maxReferenceElevation, 0.0, 1.0);
                var elevation = Math.Round(height * ratio, 4);
                var profileId = ResolveHorizontalProfile(index, referenceElevations.Count);
                var quantity = index == 0 ? 2 : 1;
                AddHorizontal(configuration, index + 1, elevation, profileId, quantity);
            }

            for (var index = 0; index < referenceElevations.Count - 1; index++)
            {
                var lowerId = "H" + (index + 1).ToString(CultureInfo.InvariantCulture);
                var upperId = "H" + (index + 2).ToString(CultureInfo.InvariantCulture);
                AddPanel(configuration, index + 1, lowerId, upperId, template.DefaultArrangement);
            }

            return configuration;
        }

        private static string ResolveHorizontalProfile(int index, int count)
        {
            if (index == 0)
            {
                return LowerHorizontalProfileId;
            }

            return index == count - 1 ? UpperHorizontalProfileId : IntermediateHorizontalProfileId;
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

        private static BasePlatePlacement CreateBasePlate(PostSide side, BasePlateCatalogEntry plate, string connectionPointId)
        {
            return new BasePlatePlacement
            {
                PostSide = side,
                PlateCatalogId = BasePlateCatalogId,
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

        private static void AddPanel(RackFrameConfiguration configuration, int number, string lowerHorizontalId, string upperHorizontalId, BracingPattern arrangement)
        {
            configuration.BracingPanels.Add(new BracingPanel
            {
                PanelId = "P" + number.ToString(CultureInfo.InvariantCulture),
                Number = number,
                LowerHorizontalId = lowerHorizontalId,
                UpperHorizontalId = upperHorizontalId,
                Arrangement = arrangement,
                MountingFace = FrameSide.Front,
                DiagonalProfileId = DiagonalProfileId,
                DiagonalDirection = DiagonalDirection.AutoAlternating,
                StartConnectionPointId = BraceStartConnectionPointId,
                EndConnectionPointId = BraceEndConnectionPointId,
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

        private static string NormalizeOrFallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
