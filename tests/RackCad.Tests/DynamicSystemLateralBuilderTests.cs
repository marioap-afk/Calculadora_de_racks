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
            var layout = new DynamicSystemLateralBuilder().Build(StandardSystem(), Catalog).Flatten();

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
            var layout = new DynamicSystemLateralBuilder().Build(StandardSystem(), Catalog).Flatten();
            var separators = layout.OfRole(HeaderBlockRole.Separator).ToList();

            Assert.NotEmpty(separators);

            // 132" header → 3 vertical levels.
            var levels = separators.Select(s => Math.Round(s.ConnectionAnchor.Y, 2)).Distinct().Count();
            Assert.Equal(SeparatorLevelCalculator.Count(132.0), levels);

            Assert.All(separators, s =>
            {
                Assert.False(string.IsNullOrWhiteSpace(s.BlockName));      // FRONTAL separator block resolved
                Assert.Equal(48.0, s.DynamicParameters["LONGITUD"], 4);    // LONGITUD = the module length (48"), as shown in the preview
                Assert.Equal("FRONTAL", s.View);
            });
        }

        [Fact]
        public void Build_SeparatorsAnchorOnThePostTroquel_NotTheModuleEdge()
        {
            var catalog = Catalog;
            var system = StandardSystem();
            var troquelSeparadorX = catalog.ConnectionLayout
                .FindConnectionLayout(CatalogIds.StandardPost, "TROQUEL_SEPARADOR", "LATERAL").LocalX;
            var firstSeparator = system.Modules.First(m => m.Kind == DynamicRackModuleKind.Separator && m.Length > 0.0);

            var layout = new DynamicSystemLateralBuilder().Build(system, catalog).Flatten();

            // Separators of the first gap anchor at moduleStartX - troquelSeparadorX (the previous post's troquel),
            // one per vertical level.
            var anchored = layout.OfRole(HeaderBlockRole.Separator)
                .Where(s => Math.Abs(s.ConnectionAnchor.X - (firstSeparator.StartX - troquelSeparadorX)) < 1e-3)
                .ToList();

            Assert.Equal(SeparatorLevelCalculator.Count(132.0), anchored.Count);
        }

        [Fact]
        public void Build_DerivedPost_AddsReinforcedPostWithPlate()
        {
            var catalog = Catalog;
            var system = StandardSystem();
            var offsets = system.GetDerivedPostOffsets();
            Assert.NotEmpty(offsets); // pallets-deep 4 → one derived post

            var layout = new DynamicSystemLateralBuilder().Build(system, catalog).Flatten();
            var offset = offsets[0];
            var finPosteX = catalog.ConnectionLayout
                .FindConnectionLayout(CatalogIds.StandardPost, "FIN_POSTE", "LATERAL").LocalX;

            // A base plate at the derived post.
            Assert.Contains(layout.OfRole(HeaderBlockRole.BasePlate), p => Math.Abs(p.ConnectionAnchor.X - offset) < 1e-3);

            // The post and its full-height reinforcement (mated at FIN_POSTE).
            Assert.Contains(layout.OfRole(HeaderBlockRole.Post), p => Math.Abs(p.ConnectionAnchor.X - offset) < 1e-3);
            Assert.Contains(layout.OfRole(HeaderBlockRole.Post), p => Math.Abs(p.ConnectionAnchor.X - (offset + finPosteX)) < 1e-3);
        }

        [Fact]
        public void Build_GroupsIdenticalHeaders_SharingOneDefinition()
        {
            // Pallets-deep 4 → both headers are end headers (length 54) → one shared definition, two placements.
            var plan = new DynamicSystemLateralBuilder().Build(StandardSystem(), Catalog);

            Assert.Single(plan.Headers);
            Assert.Equal(2, plan.Headers[0].OffsetsX.Count);
        }

        [Fact]
        public void Build_NullSystem_ReturnsEmptyPlan()
        {
            var layout = new DynamicSystemLateralBuilder().Build(null, Catalog).Flatten();
            Assert.Empty(layout.Instances);
        }
    }
}
