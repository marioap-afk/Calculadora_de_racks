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

        private static ComposedDynamicRack ComposeStandard()
        {
            var system = new DynamicRackSystemFactory(Catalog).Create(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: "POSTE_OMEGA_3X3",
                headerHeight: 132.0);
            return new DynamicRackComposer().Compose(system);
        }

        [Fact]
        public void Build_AggregatesBothHeaders_DoublesQuantities()
        {
            var composed = ComposeStandard();

            // A single header BOM totals 12 pieces; two end frames => 24.
            var singleHeader = BomBuilder.Build(composed.System.Header, Catalog);
            var systemBom = SystemBomBuilder.Build(composed, Catalog);

            Assert.Equal(2 * singleHeader.TotalPieces, systemBom.TotalPieces);
            Assert.Equal(24, systemBom.TotalPieces);

            var post = systemBom.Lines.Single(l => l.Category == BomBuilder.Post && l.ProfileId == "POSTE_OMEGA_3X3");
            Assert.Equal(4, post.Quantity);   // 2 posts per header x 2 headers
            Assert.Equal(132.0, post.Length);
            Assert.Equal(4, systemBom.Lines.Single(l => l.Category == BomBuilder.BasePlate).Quantity);
        }

        [Fact]
        public void Merge_OfTwoIdenticalBoms_DoublesEveryLine()
        {
            var single = BomBuilder.Build(ComposeStandard().System.Header, Catalog);

            var merged = BomBuilder.Merge(new[] { single, single });

            Assert.Equal(single.Lines.Count, merged.Lines.Count); // same line set, doubled quantities
            Assert.Equal(2 * single.TotalPieces, merged.TotalPieces);
            foreach (var line in single.Lines)
            {
                var mergedLine = merged.Lines.Single(l => l.Category == line.Category && l.ProfileId == line.ProfileId && l.Length == line.Length);
                Assert.Equal(2 * line.Quantity, mergedLine.Quantity);
            }
        }

        [Fact]
        public void Build_NullComposed_ReturnsEmptyBom()
        {
            Assert.Empty(SystemBomBuilder.Build(null, Catalog).Lines);
        }
    }
}
