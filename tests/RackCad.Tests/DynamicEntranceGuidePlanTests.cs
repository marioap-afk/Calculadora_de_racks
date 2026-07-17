using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    public class DynamicEntranceGuidePlanTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        [Fact]
        public void Build_DefaultsEveryAvailableLevelToTwoGuidesUsingItsEntranceSegmentLength()
        {
            var design = DynamicFrontGeometryTests.Design();
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 2 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 3, PalletsDeep = 3 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            var selection = new SelectiveSafetySelection { ElementId = "GUIA_ENTRADA", Quantity = 1 };

            var plan = DynamicEntranceGuidePlan.Build(system, selection);

            Assert.Equal(10, plan.Count); // (2 + 3 levels) x 2 sides.
            foreach (var front in system.Fronts)
            {
                var expectedLength = DynamicEntranceGuidePlan.EntranceSegmentLength(system, front);
                Assert.True(expectedLength > 0.0);
                Assert.All(plan.Where(item => item.FrontIndex == front.Index), item =>
                    Assert.Equal(expectedLength, item.Length, 4));
                foreach (var level in front.LoadBeamLevels)
                {
                    Assert.Equal(2, plan.Count(item => item.FrontIndex == front.Index
                                                       && item.LevelIndex == level.LevelNumber - 1));
                    Assert.All(plan.Where(item => item.FrontIndex == front.Index
                                                  && item.LevelIndex == level.LevelNumber - 1), item =>
                        Assert.Equal(level.EntranceElevation + 8.0, item.Elevation, 4));
                }
            }
        }

        [Fact]
        public void Build_OffCellOnlyDisablesItsFrontAndLevel()
        {
            var design = DynamicFrontGeometryTests.Design();
            design.Fronts.Add(new DynamicRackFrontDesign { LoadLevels = 3 });
            design.Fronts.Add(new DynamicRackFrontDesign { LoadLevels = 2 });
            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;
            var selection = new SelectiveSafetySelection { ElementId = "GUIA_ENTRADA", Quantity = 1 };
            selection.GuiaEntradaOffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });

            var plan = DynamicEntranceGuidePlan.Build(system, selection);

            Assert.DoesNotContain(plan, item => item.FrontIndex == 0 && item.LevelIndex == 1);
            Assert.Equal(2, plan.Count(item => item.FrontIndex == 1 && item.LevelIndex == 1));
            Assert.Equal(8, plan.Count); // one disabled cell removes its two mirrored pieces.
        }
    }
}
