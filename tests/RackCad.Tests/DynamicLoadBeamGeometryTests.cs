using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicLoadBeamGeometryTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void ResolveBeamDepth_UsesTheSingleCatalogedC6Peralte()
        {
            var depth = DynamicLoadBeamGeometry.ResolveBeamDepth(
                Catalog,
                DynamicRackDefaults.InOutBeamCatalogId,
                requestedDepth: 4.0);

            Assert.Equal(6.0, depth, 4);
        }

        [Fact]
        public void ResolveLevels_UsesExitDatumSlopeAndC6ClearStep()
        {
            var levels = DynamicLoadBeamGeometry.ResolveLevels(
                new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                loadLevels: 3,
                firstLevelHeight: 6.0,
                beamDepth: 6.0,
                slope: 12.0);

            Assert.Equal(new[] { 6.0, 78.0, 150.0 }, levels.Select(level => level.ExitElevation));
            Assert.Equal(new[] { 18.0, 90.0, 162.0 }, levels.Select(level => level.EntranceElevation));
        }

        [Fact]
        public void Resolver_SnapsBothInOutBeamMatesToTheNearestPostTroquel()
        {
            var catalog = Catalog;
            var design = new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4,
                LoadLevels = 3,
                FirstLevelHeight = 6.0,
                BeamDepth = 6.0,
                HeaderPostCatalogId = CatalogIds.StandardPost
            };

            var resolution = new DynamicRackSystemResolver(catalog).Resolve(design);
            var system = resolution.System;
            var postId = DynamicFrontGeometry.PostId(system, catalog);
            var peralte = DynamicFrontGeometry.PostPeralte(system, catalog, postId);
            var troquelEntry = catalog.ConnectionLayout.FindConnectionLayout(
                postId,
                SelectiveRackDefaults.PostBeamPoint,
                "FRONTAL");
            var gridBase = SelectivePostGeometry.Resolve(troquelEntry, new System.Collections.Generic.Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = peralte
            }).Y;
            var expectedFirst = gridBase + System.Math.Round(
                (design.FirstLevelHeight - gridBase) / SelectiveRackDefaults.TroquelPaso,
                System.MidpointRounding.AwayFromZero) * SelectiveRackDefaults.TroquelPaso;

            Assert.Equal(expectedFirst, system.LoadBeamLevels[0].ExitElevation, 4);
            Assert.All(system.LoadBeamLevels.SelectMany(level => new[]
            {
                level.ExitElevation,
                level.EntranceElevation
            }), elevation => Assert.Equal(
                System.Math.Round((elevation - gridBase) / SelectiveRackDefaults.TroquelPaso),
                (elevation - gridBase) / SelectiveRackDefaults.TroquelPaso,
                4));
        }

        [Fact]
        public void Placements_UseTheFullSystemStartAndEndAsOriginMates()
        {
            var system = new DynamicRackSystem
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = 4
            };
            system.Modules.Add(new DynamicRackModule { Kind = DynamicRackModuleKind.HeaderStart, Length = 54.0 });
            system.Modules.Add(new DynamicRackModule { Kind = DynamicRackModuleKind.Separator, Length = 48.0 });
            system.Modules.Add(new DynamicRackModule { Kind = DynamicRackModuleKind.Separator, Length = 48.0 });
            system.Modules.Add(new DynamicRackModule { Kind = DynamicRackModuleKind.HeaderEnd, Length = 54.0 });
            system.RecalculatePositions();
            system.LoadBeamLevels.Add(new DynamicLoadBeamLevel(1, 6.0, 13.4375));

            var placements = DynamicLoadBeamGeometry.Placements(system);

            Assert.Equal(2, placements.Count);
            var exit = Assert.Single(placements, placement => !placement.IsEntrance);
            var entrance = Assert.Single(placements, placement => placement.IsEntrance);
            Assert.Equal(0.0, exit.X, 4);
            Assert.Equal(system.LoadBeamLevels[0].ExitElevation, exit.Y, 4);
            Assert.False(exit.MirroredX);
            Assert.Equal(system.TotalLength, entrance.X, 4);
            Assert.Equal(system.LoadBeamLevels[0].EntranceElevation, entrance.Y, 4);
            Assert.True(entrance.MirroredX);
        }
    }
}
