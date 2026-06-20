using RackCad.Application.Catalogs;
using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    /// <summary>
    /// Builds the temporary standard frame. The structure (elevations, quantities,
    /// arrangement) is still defined here, but descriptions and connection points are
    /// resolved from the external catalogs instead of being hardcoded literals. When a
    /// catalog entry is missing, a literal fallback keeps the result usable.
    /// </summary>
    public sealed class HardcodedStandardRackFrameService
    {
        private const string PostCatalogId = "POSTE_OMEGA_3X3";
        private const string BasePlateCatalogId = "PLACA_BASE_ATORNILLABLE";
        private const string LowerHorizontalProfileId = "HORIZONTAL_INFERIOR";
        private const string IntermediateHorizontalProfileId = "HORIZONTAL_INTERMEDIA";
        private const string UpperHorizontalProfileId = "HORIZONTAL_SUPERIOR";
        private const string DiagonalProfileId = "TRAVESANO_DINAMICO_OMEGA_3X3";
        private const string BraceStartConnectionPointId = "TroquelCelosia_01";
        private const string BraceEndConnectionPointId = "TroquelCelosia_02";
        private const string FallbackBasePlateConnectionPointId = "PlacaBase_01";

        private readonly RackCatalog catalog;

        public HardcodedStandardRackFrameService()
            : this(LoadDefaultCatalog())
        {
        }

        public HardcodedStandardRackFrameService(RackCatalog catalog)
        {
            this.catalog = catalog ?? new RackCatalog();
        }

        public RackFrameConfiguration CreateDefault()
        {
            var basePlate = catalog.BasePlates.FindBasePlate(BasePlateCatalogId);
            var basePlateConnectionPointId = NormalizeOrFallback(basePlate?.ConnectionPointId, FallbackBasePlateConnectionPointId);

            var configuration = new RackFrameConfiguration
            {
                Name = "Cabecera estandar temporal",
                Units = "in",
                Height = 132.0,
                Depth = 42.0,
                StandardBaselineId = "STD-CABECERA-TEMP-001",
                StandardBaselineVersion = "0.1",
                LeftPost = CreatePost(PostSide.Left),
                RightPost = CreatePost(PostSide.Right),
                LeftBasePlate = CreateBasePlate(PostSide.Left, basePlate, basePlateConnectionPointId),
                RightBasePlate = CreateBasePlate(PostSide.Right, basePlate, basePlateConnectionPointId)
            };

            AddStandardHorizontal(configuration, 1, 0.0, LowerHorizontalProfileId, 2);
            AddStandardHorizontal(configuration, 2, 44.0, IntermediateHorizontalProfileId, 1);
            AddStandardHorizontal(configuration, 3, 88.0, IntermediateHorizontalProfileId, 1);
            AddStandardHorizontal(configuration, 4, 132.0, UpperHorizontalProfileId, 1);

            AddStandardPanel(configuration, 1, "H1", "H2");
            AddStandardPanel(configuration, 2, "H2", "H3");
            AddStandardPanel(configuration, 3, "H3", "H4");

            return configuration;
        }

        private PostAssembly CreatePost(PostSide side)
        {
            var profile = catalog.PostProfiles.FindProfile(PostCatalogId);

            return new PostAssembly
            {
                Side = side,
                PostCatalogId = PostCatalogId,
                Description = NormalizeOrFallback(profile?.Description, "Poste omega 3x3"),
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

        private void AddStandardHorizontal(RackFrameConfiguration configuration, int number, double elevation, string profileId, int quantity)
        {
            configuration.Horizontals.Add(new FrameHorizontal
            {
                Id = "H" + number.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Number = number,
                Elevation = elevation,
                ProfileId = profileId,
                Quantity = quantity,
                MountingFace = FrameSide.Front,
                State = FrameComponentState.Standard,
                IsStandard = true
            });
        }

        private static void AddStandardPanel(RackFrameConfiguration configuration, int number, string lowerHorizontalId, string upperHorizontalId)
        {
            configuration.BracingPanels.Add(new BracingPanel
            {
                PanelId = "P" + number.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Number = number,
                LowerHorizontalId = lowerHorizontalId,
                UpperHorizontalId = upperHorizontalId,
                Arrangement = BracingPattern.SingleDiagonal,
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
                // A missing or malformed catalog must not stop the configurator from opening;
                // CreateDefault falls back to literal descriptions.
                return new RackCatalog();
            }
        }

        private static string NormalizeOrFallback(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
