using RackCad.Domain.RackFrames;

namespace RackCad.Application.RackFrames
{
    public sealed class HardcodedStandardRackFrameService
    {
        public RackFrameConfiguration CreateDefault()
        {
            var configuration = new RackFrameConfiguration
            {
                Name = "Cabecera estandar temporal",
                Units = "in",
                Height = 132.0,
                Depth = 42.0,
                StandardBaselineId = "STD-CABECERA-TEMP-001",
                StandardBaselineVersion = "0.1",
                LeftPost = new PostAssembly
                {
                    Side = PostSide.Left,
                    PostCatalogId = "POSTE_OMEGA_3X3",
                    Description = "Poste omega 3x3",
                    HasReinforcement = false
                },
                RightPost = new PostAssembly
                {
                    Side = PostSide.Right,
                    PostCatalogId = "POSTE_OMEGA_3X3",
                    Description = "Poste omega 3x3",
                    HasReinforcement = false
                },
                LeftBasePlate = new BasePlatePlacement
                {
                    PostSide = PostSide.Left,
                    PlateCatalogId = "PLACA_BASE_ATORNILLABLE",
                    Description = "Placa base atornillable",
                    ConnectionPointId = "PlacaBase_01"
                },
                RightBasePlate = new BasePlatePlacement
                {
                    PostSide = PostSide.Right,
                    PlateCatalogId = "PLACA_BASE_ATORNILLABLE",
                    Description = "Placa base atornillable",
                    ConnectionPointId = "PlacaBase_01"
                }
            };

            AddStandardHorizontal(configuration, 1, 0.0, "HORIZONTAL_INFERIOR", 2);
            AddStandardHorizontal(configuration, 2, 44.0, "HORIZONTAL_INTERMEDIA", 1);
            AddStandardHorizontal(configuration, 3, 88.0, "HORIZONTAL_INTERMEDIA", 1);
            AddStandardHorizontal(configuration, 4, 132.0, "HORIZONTAL_SUPERIOR", 1);

            AddStandardPanel(configuration, 1, "H1", "H2");
            AddStandardPanel(configuration, 2, "H2", "H3");
            AddStandardPanel(configuration, 3, "H3", "H4");

            return configuration;
        }

        private static void AddStandardHorizontal(RackFrameConfiguration configuration, int number, double elevation, string profileId, int quantity)
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
                DiagonalProfileId = "TRAVESANO_DINAMICO_OMEGA_3X3",
                DiagonalDirection = DiagonalDirection.AutoAlternating,
                StartConnectionPointId = "TroquelCelosia_01",
                EndConnectionPointId = "TroquelCelosia_02",
                IsStandard = true,
                IsException = false
            });
        }
    }
}
