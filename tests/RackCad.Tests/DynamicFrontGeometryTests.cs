using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicFrontGeometryTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void Resolve_AllowsDifferentPositionCountsAndAnExplicitBeamLength()
        {
            var fronts = DynamicFrontGeometry.Resolve(new[]
            {
                new DynamicRackFrontDesign { PalletCount = 1 },
                new DynamicRackFrontDesign { PalletCount = 3, LoadLevels = 5 },
                new DynamicRackFrontDesign { PalletCount = 2, BeamLengthOverride = 120.0 }
            }, palletFront: 42.0, tolerance: 4.0);

            // BFR = frente de tarima + 2; largo IN/OUT = BFR por posicion + 6.
            Assert.Equal(new[] { 50.0, 138.0, 120.0 }, fronts.Select(front => front.BeamLength));
            Assert.All(fronts, front => Assert.Equal(44.0, front.Bfr));
            Assert.Equal(new[] { 1, 3, 2 }, fronts.Select(front => front.PalletCount));
            Assert.Equal(new[] { 3, 5, 3 }, fronts.Select(front => front.LoadLevels));
        }

        [Fact]
        public void Compute_UsesTheSameCatalogDrivenPostGridForEveryView()
        {
            var design = Design();
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 3 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            var layout = DynamicFrontGeometry.Compute(system, Catalog);

            Assert.Equal(3, layout.PostPositions.Count);
            Assert.Equal(0.0, layout.PostPositions[0], 4);
            Assert.True(layout.PostPositions[1] > system.Fronts[0].BeamLength);
            Assert.True(layout.PostPositions[2] - layout.PostPositions[1] > system.Fronts[1].BeamLength);
            Assert.Equal(layout.TroquelPositions[0], layout.TroquelPositions[2], 4);
        }

        [Fact]
        public void PreviewFrontal_UsesTheTallestAdjacentFrontAtEachPost()
        {
            var design = Design();
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 5 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            var preview = DynamicSystemPreviewGeometry.Frontal(system, Catalog);

            Assert.Equal(4, preview.PostHeights.Count);
            Assert.Equal(system.Fronts[0].Height, preview.PostHeights[0], 4);
            Assert.Equal(system.Fronts[1].Height, preview.PostHeights[1], 4);
            Assert.Equal(system.Fronts[1].Height, preview.PostHeights[2], 4);
            Assert.Equal(system.Fronts[2].Height, preview.PostHeights[3], 4);
            Assert.Equal(system.Fronts[1].Height, preview.Height, 4);
        }

        [Fact]
        public void PreviewLateral_UsesOnlyTheSelectedPostsResolvedDepthRange()
        {
            var design = Design();
            design.PalletsDeep = 4;
            design.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1,
                LoadLevels = 2,
                PalletsDeep = 2,
                DepthStartPosition = 3
            });
            design.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1,
                LoadLevels = 4,
                PalletsDeep = 4,
                DepthStartPosition = 1
            });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            var exterior = DynamicSystemPreviewGeometry.Lateral(system, Catalog, 0);
            var shared = DynamicSystemPreviewGeometry.Lateral(system, Catalog, 1);

            Assert.Equal((3, 4), (exterior.Range.StartPosition, exterior.Range.EndPosition));
            Assert.Equal(new[] { 3, 4 }, exterior.Modules.Select(module => module.Index + 1));
            Assert.Equal(system.Fronts[0].StartX, exterior.StartX, 4);
            Assert.Equal(system.Fronts[0].EndX, exterior.EndX, 4);
            Assert.Equal(2, exterior.LoadLevels);
            Assert.Equal((1, 4), (shared.Range.StartPosition, shared.Range.EndPosition));
            Assert.Equal(4, shared.LoadLevels);
            Assert.NotNull(exterior.Plan);
        }

        internal static DynamicRackDesign Design()
            => new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0
            };
    }
}
