using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-002 (I-32) — "how many load levels does a post see" is ONE rule: the tallest adjacent front owns the cut.
    /// It already governed the drawing; this states it over a plain per-front list so a dialog with no resolved system
    /// consumes the same rule instead of inventing its own.
    /// </summary>
    public class DynamicPostLevelsTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void LoadLevelsPerPost_IsTheMaxOfTheAdjacentFronts()
        {
            // 2 fronts => 3 posts. The interior post sees BOTH neighbours; each end post sees only its own.
            Assert.Equal(new[] { 3, 3, 1 }, DynamicFrontGeometry.LoadLevelsPerPost(new[] { 3, 1 }).ToArray());
            Assert.Equal(new[] { 1, 3, 3 }, DynamicFrontGeometry.LoadLevelsPerPost(new[] { 1, 3 }).ToArray());
            Assert.Equal(new[] { 2, 2 }, DynamicFrontGeometry.LoadLevelsPerPost(new[] { 2 }).ToArray());
            Assert.Equal(new[] { 2, 5, 5, 4 }, DynamicFrontGeometry.LoadLevelsPerPost(new[] { 2, 5, 4 }).ToArray());
        }

        [Fact]
        public void LoadLevelsPerPost_NeverReturnsZeroAndHandlesAnEmptyRack()
        {
            Assert.Equal(new[] { 1 }, DynamicFrontGeometry.LoadLevelsPerPost(new int[0]).ToArray());
            Assert.Equal(new[] { 1 }, DynamicFrontGeometry.LoadLevelsPerPost(null).ToArray());
        }

        /// <summary>
        /// The pure rule agrees with the resolved-system one the drawing uses, post by post — this is what makes it the
        /// same rule and not a look-alike.
        /// </summary>
        [Fact]
        public void LoadLevelsPerPost_AgreesWithTheResolvedSystemRule()
        {
            var catalog = Catalog;
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 4, DepthStartPosition = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 1, PalletsDeep = 4, DepthStartPosition = 1 });

            var system = new DynamicRackSystemResolver(catalog).Resolve(design).System;
            var perPost = DynamicFrontGeometry.LoadLevelsPerPost(
                system.Fronts.Select(front => front.LoadLevels).ToList());

            Assert.Equal(system.Fronts.Count + 1, perPost.Count);
            for (var post = 0; post < perPost.Count; post++)
            {
                Assert.Equal(DynamicFrontGeometry.LoadLevelsAtPost(system, post), perPost[post]);
            }
        }
    }
}
