using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicRackSystemResolverTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackDesign Design()
        {
            return new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0,
                HeaderPostCatalogId = "POSTE_OMEGA_3X3"
            };
        }

        [Fact]
        public void Resolve_EmptyModuleDesign_BuildsTheCurrentStandardLayoutAndHeight()
        {
            var design = Design();
            var resolver = new DynamicRackSystemResolver(Catalog);

            var result = resolver.Resolve(design);

            Assert.Equal(4, result.System.Modules.Count);
            Assert.Equal(204.0, result.System.TotalLength, 4);
            Assert.All(result.System.Modules.Where(m => m.IsHeader), m =>
            {
                Assert.True(m.UseCalculatedHeaderConfiguration);
                Assert.Equal(result.Height.HeaderHeight, m.AssociatedFrameConfiguration.Height, 4);
            });
        }

        [Fact]
        public void Resolve_RecalculatesStandardHeaders_ButPreservesACustomHeader()
        {
            var resolver = new DynamicRackSystemResolver(Catalog);
            var first = resolver.Resolve(Design()).System;
            var design = resolver.Snapshot(first, 3, 6.0, 4.0, "POSTE_OMEGA_3X3");

            var custom = design.Modules.First(m => m.IsHeader);
            custom.UseCalculatedHeaderConfiguration = false;
            custom.HeaderConfiguration.Height = 150.0;
            custom.HeaderConfiguration.LeftBasePlate.PeralteOverride = 9.0;
            design.LoadLevels = 5;

            var updated = resolver.Resolve(design);
            var headers = updated.System.Modules.Where(m => m.IsHeader).ToList();

            Assert.Equal(150.0, headers[0].AssociatedFrameConfiguration.Height, 4);
            Assert.Equal(9.0, headers[0].AssociatedFrameConfiguration.LeftBasePlate.PeralteOverride);
            Assert.False(headers[0].UseCalculatedHeaderConfiguration);
            Assert.All(headers.Skip(1), h =>
            {
                Assert.True(h.UseCalculatedHeaderConfiguration);
                Assert.Equal(updated.Height.HeaderHeight, h.AssociatedFrameConfiguration.Height, 4);
            });
        }

        [Fact]
        public void Snapshot_OwnsIndependentHeaderConfigurations()
        {
            var resolver = new DynamicRackSystemResolver(Catalog);
            var system = resolver.Resolve(Design()).System;

            var snapshot = resolver.Snapshot(system, 3, 6.0, 4.0, "POSTE_OMEGA_3X3");
            snapshot.Modules.First(m => m.IsHeader).HeaderConfiguration.Height = 999.0;

            Assert.NotEqual(999.0, system.Modules.First(m => m.IsHeader).AssociatedFrameConfiguration.Height);
        }

        [Fact]
        public void Resolve_InvalidLoadLevels_ThrowsClearly()
        {
            var design = Design();
            design.LoadLevels = 0;

            var ex = Assert.Throws<ArgumentException>(() => new DynamicRackSystemResolver(Catalog).Resolve(design));

            Assert.Contains("nivel", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
