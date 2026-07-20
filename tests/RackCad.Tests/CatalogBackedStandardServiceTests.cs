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
                    new ProfileCatalogEntry
                    {
                        Id = TestCatalogIds.Profiles.Posts.Standard,
                        Description = "Poste personalizado"
                    }
                },
                BasePlates = new List<BasePlateCatalogEntry>
                {
                    new BasePlateCatalogEntry
                    {
                        Id = TestCatalogIds.BasePlates.Standard,
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
                        PieceId = TestCatalogIds.BasePlates.Standard,
                        ConnectionPointId = "CP_CUSTOM",
                        View = TestCatalogIds.Views.Front
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
            Assert.Equal(TestCatalogIds.Profiles.Posts.Standard, configuration.LeftPost.Description);
            Assert.Equal("Placa base atornillable", configuration.LeftBasePlate.Description);
            Assert.Equal(TestCatalogIds.ConnectionPoints.PostMount, configuration.LeftBasePlate.ConnectionPointId);
        }

        [Fact]
        public void CreateDefault_WithNullCatalog_DoesNotThrowAndUsesFallback()
        {
            var configuration = new HardcodedStandardRackFrameService((RackCatalog)null).CreateDefault();

            Assert.Equal(TestCatalogIds.Profiles.Posts.Standard, configuration.LeftPost.Description);
            // Structure is unchanged regardless of catalog: 3 standard travesaños + 2 closings, 4 panels.
            Assert.Equal(5, configuration.Horizontals.Count);
            Assert.Equal(4, configuration.BracingPanels.Count);
        }

        [Fact]
        public void CreateDefault_DefaultConstructor_ReadsDescriptionFromShippedCatalog()
        {
            // The shipped post-profiles.csv describes the standard post.
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();

            Assert.Equal("Poste Omega 3x3 calibre 14", configuration.LeftPost.Description);
        }
    }
}
