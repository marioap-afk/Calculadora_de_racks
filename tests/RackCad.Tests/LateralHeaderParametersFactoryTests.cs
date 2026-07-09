using System;
using RackCad.Application.Headers;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class LateralHeaderParametersFactoryTests
    {
        private static RackFrameConfiguration StandardConfiguration()
        {
            var configuration = new RackFrameConfiguration
            {
                Height = 132.0,
                Depth = 42.0,
                CelosiaStartTroquel = 3,
                DiagonalStartOffsetTroqueles = 2,
                DiagonalEndOffsetTroqueles = 2,
                LeftPost = new PostAssembly { PostCatalogId = "POSTE_X" },
                LeftBasePlate = new BasePlatePlacement { PlateCatalogId = "PLACA_X" }
            };

            configuration.Horizontals.Add(new FrameHorizontal { Number = 1, Elevation = 0.0, ProfileId = "TRUSS_X" });
            configuration.Horizontals.Add(new FrameHorizontal { Number = 2, Elevation = 44.0, ProfileId = "TRUSS_X" });
            configuration.Horizontals.Add(new FrameHorizontal { Number = 3, Elevation = 88.0, ProfileId = "TRUSS_X" });

            return configuration;
        }

        [Fact]
        public void FromConfiguration_CopiesDimensionsAndCelosiaParameters()
        {
            var parameters = LateralHeaderParametersFactory.FromConfiguration(StandardConfiguration());

            Assert.Equal(132.0, parameters.Height, 4);
            Assert.Equal(42.0, parameters.Depth, 4);
            Assert.Equal(2, parameters.OffsetDiagonalInicioTroqueles);
            Assert.Equal(2, parameters.OffsetDiagonalFinTroqueles);
        }

        [Fact]
        public void FromConfiguration_TakesCatalogIdsFromConfiguredPieces()
        {
            var parameters = LateralHeaderParametersFactory.FromConfiguration(StandardConfiguration());

            Assert.Equal("POSTE_X", parameters.PostId);
            Assert.Equal("PLACA_X", parameters.BasePlateId);
            Assert.Equal("TRUSS_X", parameters.TrussProfileId);
        }

        [Fact]
        public void FromConfiguration_FallsBackToDiagonalProfileWhenHorizontalsHaveNoProfile()
        {
            var configuration = StandardConfiguration();
            foreach (var horizontal in configuration.Horizontals)
            {
                horizontal.ProfileId = null;
            }

            configuration.BracingPanels.Add(new BracingPanel { DiagonalProfileId = "DIAG_X" });

            var parameters = LateralHeaderParametersFactory.FromConfiguration(configuration);

            Assert.Equal("DIAG_X", parameters.TrussProfileId);
        }

        [Fact]
        public void FromConfiguration_NullConfiguration_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => LateralHeaderParametersFactory.FromConfiguration(null));
        }
    }
}
