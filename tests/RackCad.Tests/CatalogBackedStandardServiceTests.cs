using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    public class CatalogBackedStandardServiceTests
    {
        [Fact]
        public void CreateDefault_WithInjectedCatalog_UsesCatalogDescriptionsAndConnectionPoint()
        {
            var catalog = new RackCatalog
            {
                PostProfiles = new List<ProfileCatalogEntry>
                {
                    new ProfileCatalogEntry { Id = "POSTE_OMEGA_3X3", Description = "Poste personalizado" }
                },
                BasePlates = new List<BasePlateCatalogEntry>
                {
                    new BasePlateCatalogEntry
                    {
                        Id = "PLACA_BASE_ATORNILLABLE",
                        Description = "Placa personalizada"
                    }
                },
                ConnectionPoints = new List<ConnectionPointCatalogEntry>
                {
                    new ConnectionPointCatalogEntry { Id = "CP_CUSTOM", Role = "BasePlate" }
                },
                // The plate's anchor now lives in the connection-layout table, not on the plate itself.
                ConnectionLayout = new List<ConnectionLayoutEntry>
                {
                    new ConnectionLayoutEntry
                    {
                        PieceId = "PLACA_BASE_ATORNILLABLE",
                        ConnectionPointId = "CP_CUSTOM",
                        View = "FRONTAL"
                    }
                }
            };

            var configuration = new HardcodedStandardRackFrameService(catalog).CreateDefault();

            Assert.Equal("Poste personalizado", configuration.LeftPost.Description);
            Assert.Equal("Poste personalizado", configuration.RightPost.Description);
            Assert.Equal("Placa personalizada", configuration.LeftBasePlate.Description);
            Assert.Equal("CP_CUSTOM", configuration.LeftBasePlate.ConnectionPointId);
        }

        [Fact]
        public void CreateDefault_WithEmptyCatalog_FallsBackToLiteralDescriptions()
        {
            var configuration = new HardcodedStandardRackFrameService(new RackCatalog()).CreateDefault();

            // With no catalog the post description falls back to its id; the plate keeps a literal fallback.
            Assert.Equal("POSTE_OMEGA_3X3", configuration.LeftPost.Description);
            Assert.Equal("Placa base atornillable", configuration.LeftBasePlate.Description);
            Assert.Equal("PlacaBase_01", configuration.LeftBasePlate.ConnectionPointId);
        }

        [Fact]
        public void CreateDefault_WithNullCatalog_DoesNotThrowAndUsesFallback()
        {
            var configuration = new HardcodedStandardRackFrameService((RackCatalog)null).CreateDefault();

            Assert.Equal("POSTE_OMEGA_3X3", configuration.LeftPost.Description);
            // Structure is unchanged regardless of catalog.
            Assert.Equal(4, configuration.Horizontals.Count);
            Assert.Equal(3, configuration.BracingPanels.Count);
        }

        [Fact]
        public void CreateDefault_DefaultConstructor_ReadsDescriptionFromShippedCatalog()
        {
            // The shipped assets/catalogs/post-profiles.json describes POSTE_OMEGA_3X3.
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();

            Assert.Equal("Poste omega 3x3", configuration.LeftPost.Description);
        }
    }
}
