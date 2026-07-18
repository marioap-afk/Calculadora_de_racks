using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class FlowBedCatalogTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void Catalog_LoadsTheFourFlowBedComponents()
        {
            var bed = Catalog.FlowBedProfiles;

            Assert.Contains(bed, c => c.Role == "RIEL");
            Assert.Contains(bed, c => c.Role == "RODILLO");
            Assert.Contains(bed, c => c.Role == "FRENO");
            Assert.Contains(bed, c => c.Role == "TOPE");
        }

        [Fact]
        public void Catalog_FlowBed_HasLateralBlocksAndRollerCapacity()
        {
            var catalog = Catalog;

            Assert.NotNull(catalog.Blocks.FindBlock(TestCatalogIds.FlowBed.Rail, TestCatalogIds.Views.Lateral));
            Assert.NotNull(catalog.Blocks.FindBlock(
                TestCatalogIds.FlowBed.Roller1Point9,
                TestCatalogIds.Views.Lateral));
            Assert.NotNull(catalog.Blocks.FindBlock(TestCatalogIds.FlowBed.Brake, TestCatalogIds.Views.Lateral));
            Assert.NotNull(catalog.Blocks.FindBlock(TestCatalogIds.FlowBed.Stop, TestCatalogIds.Views.Lateral));

            // Roller carries a capacity (reserved for the future capacity-based count).
            var roller = catalog.FlowBedProfiles.First(c => c.Id == TestCatalogIds.FlowBed.Roller1Point9);
            Assert.Equal(110.0, roller.CapacityKg, 4);
        }
    }
}
