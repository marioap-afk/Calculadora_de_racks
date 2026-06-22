using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class SystemBomBuilderTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem StandardSystem()
        {
            return new DynamicRackSystemBuilder(Catalog).BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: "POSTE_OMEGA_3X3",
                headerHeight: 132.0);
        }

        [Fact]
        public void Build_AggregatesBothHeaderModules_DoublesQuantities()
        {
            var system = StandardSystem();
            var singleHeader = BomBuilder.Build(system.Modules.First().AssociatedFrameConfiguration, Catalog);
            var systemBom = SystemBomBuilder.Build(system, Catalog);

            Assert.Equal(2 * singleHeader.TotalPieces, systemBom.TotalPieces);
            Assert.Equal(22, systemBom.TotalPieces); // 2 headers x 11 pieces (5 travesaños + 2 diagonals + 2 posts + 2 plates)

            var post = systemBom.Lines.Single(l => l.Category == BomBuilder.Post && l.ProfileId == "POSTE_OMEGA_3X3");
            Assert.Equal(4, post.Quantity);
            Assert.Equal(132.0, post.Length);
            Assert.Equal(4, systemBom.Lines.Single(l => l.Category == BomBuilder.BasePlate).Quantity);
        }

        [Fact]
        public void Build_NullSystem_ReturnsEmptyBom()
        {
            Assert.Empty(SystemBomBuilder.Build(null, Catalog).Lines);
        }
    }
}
