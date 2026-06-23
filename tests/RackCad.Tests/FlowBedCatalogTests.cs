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

            Assert.NotNull(catalog.Blocks.FindBlock(FlowBedDefaults.RailId, FlowBedDefaults.View));
            Assert.NotNull(catalog.Blocks.FindBlock(FlowBedDefaults.RollerId, FlowBedDefaults.View));
            Assert.NotNull(catalog.Blocks.FindBlock(FlowBedDefaults.BrakeId, FlowBedDefaults.View));
            Assert.NotNull(catalog.Blocks.FindBlock(FlowBedDefaults.StopId, FlowBedDefaults.View));

            // Roller carries a capacity (reserved for the future capacity-based count).
            var roller = catalog.FlowBedProfiles.First(c => c.Id == FlowBedDefaults.RollerId);
            Assert.Equal(100.0, roller.CapacityKg, 4);
        }
    }
}
