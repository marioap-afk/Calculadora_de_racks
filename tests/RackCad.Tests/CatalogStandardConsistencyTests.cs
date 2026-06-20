using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Guards the migration away from hardcoded ids: every catalog id referenced by
    /// the standard frame must exist in the shipped JSON catalogs. When the standard
    /// service is changed to read from catalogs, these references must already resolve.
    /// </summary>
    public class CatalogStandardConsistencyTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void StandardPostAndPlateIds_ExistInCatalog()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            var catalog = Catalog;

            Assert.NotNull(catalog.PostProfiles.FindProfile(configuration.LeftPost.PostCatalogId));
            Assert.NotNull(catalog.PostProfiles.FindProfile(configuration.RightPost.PostCatalogId));
            Assert.NotNull(catalog.BasePlates.FindBasePlate(configuration.LeftBasePlate.PlateCatalogId));
            Assert.NotNull(catalog.BasePlates.FindBasePlate(configuration.RightBasePlate.PlateCatalogId));
        }

        [Fact]
        public void StandardHorizontalProfileIds_ExistInCatalog()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            var catalog = Catalog;

            foreach (var horizontal in configuration.Horizontals)
            {
                Assert.NotNull(catalog.HorizontalProfiles.FindProfile(horizontal.ProfileId));
            }
        }

        [Fact]
        public void StandardDiagonalAndConnectionPointIds_ExistInCatalog()
        {
            var configuration = new HardcodedStandardRackFrameService().CreateDefault();
            var catalog = Catalog;

            foreach (var panel in configuration.BracingPanels)
            {
                Assert.NotNull(catalog.DiagonalProfiles.FindProfile(panel.DiagonalProfileId));
                Assert.NotNull(catalog.ConnectionPoints.FindConnectionPoint(panel.StartConnectionPointId));
                Assert.NotNull(catalog.ConnectionPoints.FindConnectionPoint(panel.EndConnectionPointId));
            }
        }
    }
}
