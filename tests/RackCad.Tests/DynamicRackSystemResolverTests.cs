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
            Assert.Equal(TestCatalogIds.Profiles.Beams.DynamicInOut, result.System.InOutBeamCatalogId);
            Assert.Equal(6.0, result.System.InOutBeamDepth, 4);
            Assert.Equal(3, result.System.LoadBeamLevels.Count);
            Assert.Equal(6.0, result.System.LoadBeamLevels[0].ExitElevation, 4);
            Assert.Equal(6.0 + result.Height.Slope, result.System.LoadBeamLevels[0].EntranceElevation, 4);
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
        public void Resolve_RackWidePostPeralteOverridesCalculatedAndCustomHeaders()
        {
            var resolver = new DynamicRackSystemResolver(Catalog);
            var first = resolver.Resolve(Design()).System;
            var design = resolver.Snapshot(first, 3, 6.0, 6.0, "POSTE_OMEGA_3X3");
            var custom = design.Modules.First(module => module.IsHeader);
            custom.UseCalculatedHeaderConfiguration = false;
            custom.HeaderConfiguration.PostPeralte = 2.5;
            design.PostPeralte = 4.5;

            var system = resolver.Resolve(design).System;

            Assert.Equal(4.5, system.PostPeralte, 4);
            Assert.Equal(4.5, DynamicFrontGeometry.PostPeralte(system, Catalog), 4);
            Assert.All(system.Modules.Where(module => module.IsHeader), module =>
                Assert.Equal(4.5, module.AssociatedFrameConfiguration.PostPeralte, 4));
            Assert.Equal(4.5, resolver.Snapshot(system, 3, 6.0, 6.0, "POSTE_OMEGA_3X3").PostPeralte, 4);
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
        public void ResolveAndSnapshot_DeepCopySafetySelections()
        {
            var design = Design();
            var safety = new SelectiveSafetySelection
            {
                ElementId = TestCatalogIds.Safety.Boots.C6,
                Quantity = 1,
                Side = SafetySide.Both
            };
            safety.PostSides.Add(new SafetyPostSide { PostIndex = 0, Side = SafetySide.Left });
            safety.DefensaPosts.Add(new SafetyPostDefense
            {
                PostIndex = 0,
                ExitLength = 18.0,
                EntranceLength = 24.0
            });
            safety.GuiaEntradaOffCells.Add(new SelectiveGridCell { Frente = 0, Level = 1 });
            design.SafetySelections.Add(safety);
            var resolver = new DynamicRackSystemResolver(Catalog);

            var system = resolver.Resolve(design).System;
            design.SafetySelections[0].Side = SafetySide.None;
            var snapshot = resolver.Snapshot(system, 3, 6.0, 6.0, "POSTE_OMEGA_3X3");
            system.SafetySelections[0].PostSides[0].Side = SafetySide.Right;
            system.SafetySelections[0].DefensaPosts[0].ExitLength = 99.0;
            system.SafetySelections[0].GuiaEntradaOffCells[0].Level = 0;

            Assert.Equal(SafetySide.Both, snapshot.SafetySelections[0].Side);
            Assert.Equal(SafetySide.Left, snapshot.SafetySelections[0].PostSides[0].Side);
            Assert.Equal(18.0, snapshot.SafetySelections[0].DefensaPosts[0].ExitLength, 4);
            Assert.Equal(1, snapshot.SafetySelections[0].GuiaEntradaOffCells[0].Level);
        }

        [Fact]
        public void Resolve_InvalidLoadLevels_ThrowsClearly()
        {
            var design = Design();
            design.LoadLevels = 0;

            var ex = Assert.Throws<ArgumentException>(() => new DynamicRackSystemResolver(Catalog).Resolve(design));

            Assert.Contains("nivel", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ResolveAndSnapshot_PreserveDifferentFrontWidths()
        {
            var design = Design();
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 3, BeamLengthOverride = 150.0 });
            var resolver = new DynamicRackSystemResolver(Catalog);

            var system = resolver.Resolve(design).System;
            var snapshot = resolver.Snapshot(system, 3, 6.0, 6.0, "POSTE_OMEGA_3X3");

            Assert.Equal(new[] { 50.0, 150.0 }, system.Fronts.Select(front => front.BeamLength));
            Assert.Equal(new[] { 1, 3 }, snapshot.Fronts.Select(front => front.PalletCount));
            Assert.Null(snapshot.Fronts[0].BeamLengthOverride);
            Assert.Equal(150.0, snapshot.Fronts[1].BeamLengthOverride);
        }

        [Fact]
        public void Resolve_MissingIntermediateBeamPeraltes_DefaultsEveryFrontLevelToThreePointFive()
        {
            var design = Design();
            design.LoadLevels = 3;
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2 });
            design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 3 });

            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            Assert.Equal(new[] { 3.5, 3.5 }, system.Fronts[0].IntermediateBeamDepths);
            Assert.Equal(new[] { 3.5, 3.5, 3.5 }, system.Fronts[1].IntermediateBeamDepths);
        }

        [Fact]
        public void Resolve_FrontAndLevelInputsRemainIndependent()
        {
            var design = Design();
            var first = new DynamicRackFrontDesign
            {
                PalletCount = 1,
                LoadLevels = 2,
                FirstLevelHeight = 6.0
            };
            first.Levels.Add(new DynamicRackLevelDesign
            {
                PalletFront = 40.0,
                PalletHeight = 50.0,
                PalletWeight = 800.0,
                ClearHeight = 4.0,
                InOutBeamDepth = 6.0,
                IntermediateBeamDepth = 3.5
            });
            first.Levels.Add(new DynamicRackLevelDesign
            {
                PalletFront = 50.0,
                PalletHeight = 70.0,
                PalletWeight = 1200.0,
                ClearHeight = 8.0,
                InOutBeamDepth = 6.0,
                IntermediateBeamDepth = 5.0
            });
            var second = new DynamicRackFrontDesign
            {
                PalletCount = 2,
                LoadLevels = 1,
                FirstLevelHeight = 30.0
            };
            second.Levels.Add(new DynamicRackLevelDesign
            {
                PalletFront = 42.0,
                PalletHeight = 60.0,
                PalletWeight = 1000.0,
                ClearHeight = 6.0,
                InOutBeamDepth = 6.0,
                IntermediateBeamDepth = 4.0
            });
            design.Fronts.Add(first);
            design.Fronts.Add(second);

            var system = new DynamicRackSystemResolver(Catalog).Resolve(design).System;

            Assert.Equal(6.0, system.Fronts[0].FirstLevelHeight, 4);
            Assert.Equal(30.0, system.Fronts[1].FirstLevelHeight, 4);
            Assert.Equal(6.0, system.Fronts[0].LoadBeamLevels[0].ExitElevation, 4);
            Assert.Equal(30.0, system.Fronts[1].LoadBeamLevels[0].ExitElevation, 4);
            Assert.Equal(new[] { 40.0, 50.0 }, system.Fronts[0].Levels.Select(level => level.Pallet.Front));
            Assert.Equal(new[] { 4.0, 8.0 }, system.Fronts[0].Levels.Select(level => level.ClearHeight));
            Assert.Equal(new[] { 3.5, 5.0 }, system.Fronts[0].Levels.Select(level => level.IntermediateBeamDepth));
            Assert.Equal(58.0, system.Fronts[0].BeamLength, 4); // Longest cell governs the physical posts.
            Assert.Equal(94.0, system.Fronts[1].BeamLength, 4);
        }

        [Fact]
        public void CellScope_FrontTargetsOnlyTheSelectedFrontLevels()
        {
            var targets = DynamicRackCellScopeResolver.Targets(
                new[] { 2, 3, 1 },
                sourceFrontIndex: 1,
                sourceLevelIndex: 1,
                DynamicRackCellScope.Front);

            Assert.Equal(3, targets.Count);
            Assert.All(targets, target => Assert.Equal(1, target.FrontIndex));
            Assert.Equal(new[] { 0, 1, 2 }, targets.Select(target => target.LevelIndex));
        }

        [Fact]
        public void CellScope_SelectedKeepsOnlyValidDistinctCells()
        {
            var targets = DynamicRackCellScopeResolver.Targets(
                new[] { 2, 3, 1 },
                sourceFrontIndex: 0,
                sourceLevelIndex: 0,
                DynamicRackCellScope.Selected,
                new[]
                {
                    new DynamicRackCellAddress(0, 1),
                    new DynamicRackCellAddress(1, 2),
                    new DynamicRackCellAddress(1, 2),
                    new DynamicRackCellAddress(2, 1),
                    new DynamicRackCellAddress(8, 0)
                });

            Assert.Equal(2, targets.Count);
            Assert.Contains(targets, target => target.FrontIndex == 0 && target.LevelIndex == 1);
            Assert.Contains(targets, target => target.FrontIndex == 1 && target.LevelIndex == 2);
        }
    }
}
