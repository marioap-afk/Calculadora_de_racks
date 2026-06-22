using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicSystemLateralBuilderTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static DynamicRackSystem StandardSystem()
        {
            return new DynamicRackSystemBuilder(Catalog).BuildDefault(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                palletsDeep: 4,
                headerTemplate: RackFrameTemplateCatalog.Default,
                headerPostCatalogId: CatalogIds.StandardPost,
                headerHeight: 132.0);
        }

        [Fact]
        public void Build_PlacesHeadersAlongTheRun()
        {
            var layout = new DynamicSystemLateralBuilder().Build(StandardSystem(), Catalog);

            var postXs = layout.OfRole(HeaderBlockRole.Post)
                .Select(p => Math.Round(p.Insertion.X, 2))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // More than one header → posts appear at more than one run position (offset by StartX).
            Assert.True(postXs.Count >= 2);
            Assert.Contains(0.0, postXs);            // first header at the origin
            Assert.Contains(postXs, x => x > 0.0);   // later headers shifted along the run
        }

        [Fact]
        public void Build_AddsSeparatorsAtEachLevel_WithResolvedBlockAndLength()
        {
            var layout = new DynamicSystemLateralBuilder().Build(StandardSystem(), Catalog);
            var separators = layout.OfRole(HeaderBlockRole.Separator).ToList();

            Assert.NotEmpty(separators);

            // 132" header → 3 vertical levels.
            var levels = separators.Select(s => Math.Round(s.ConnectionAnchor.Y, 2)).Distinct().Count();
            Assert.Equal(SeparatorLevelCalculator.Count(132.0), levels);

            Assert.All(separators, s =>
            {
                Assert.False(string.IsNullOrWhiteSpace(s.BlockName));      // FRONTAL separator block resolved
                Assert.True(s.DynamicParameters["LONGITUD"] > 0.0);        // spans the separator gap
                Assert.Equal("FRONTAL", s.View);
            });
        }

        [Fact]
        public void Build_NullSystem_ReturnsEmptyPlan()
        {
            var layout = new DynamicSystemLateralBuilder().Build(null, Catalog);
            Assert.Empty(layout.Instances);
        }
    }
}
