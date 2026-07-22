using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Characterization + equivalence tests for the pure front x level grid extracted from the dynamic editor window
    /// (I-21): front growth/shrink, selection and multi-cell toggling, scoped apply, snapshot/rollback, resolved
    /// refresh/restore and the design projection. Each behavior mirrors the window's old inline logic so a regression in
    /// the extraction fails here without needing WPF or AutoCAD.
    /// </summary>
    public class DynamicFrontMatrixTests
    {
        private static DynamicEditorValues Values(int palletCount = 2, int levels = 3, int deep = 5, int depthStart = 1,
            double front = 44.0, double intermediateDepth = 3.0)
            => new DynamicEditorValues
            {
                PalletCount = palletCount,
                LoadLevels = levels,
                PalletsDeep = deep,
                DepthStartPosition = depthStart,
                FirstLevelHeight = 6.0,
                BeamLengthOverride = null,
                PalletFront = front,
                PalletHeight = 60.0,
                PalletWeight = 1000.0,
                ClearHeight = 5.0,
                InOutBeamCatalogId = DynamicRackDefaults.InOutBeamCatalogId,
                InOutBeamDepth = DynamicRackDefaults.DefaultBeamDepth,
                IntermediateBeamCatalogId = DynamicRackDefaults.IntermediateBeamCatalogId,
                IntermediateBeamDepth = intermediateDepth
            };

        private static DynamicRackFront ResolvedFront(int index, int palletCount, int levels, double bfr, double beam)
        {
            var front = new DynamicRackFront
            {
                Index = index,
                PalletCount = palletCount,
                LoadLevels = levels,
                PalletsDeep = 5,
                DepthStartPosition = 1,
                Bfr = bfr,
                BeamLength = beam,
                FirstLevelHeight = 6.0
            };
            for (var level = 0; level < levels; level++)
            {
                front.Levels.Add(new DynamicRackLevel
                {
                    LevelNumber = level + 1,
                    Pallet = new PalletSpecification(front: 42.0, depth: 48.0, height: 60.0, weight: 1000.0, weightUnit: "kg"),
                    IntermediateBeamDepth = 3.0
                });
            }

            return front;
        }

        [Fact]
        public void NewMatrix_HasOneDefaultFront_WithBottomCellSelected()
        {
            var matrix = new DynamicFrontMatrix();

            Assert.Equal(1, matrix.Count);
            Assert.Equal(0, matrix.SelectedFrontIndex);
            Assert.Equal(0, matrix.SelectedLevelIndex);
            Assert.Equal(1, matrix.SelectedCellCount);
            var front = matrix.Fronts[0];
            Assert.Equal(DynamicRackDefaults.DefaultPalletsWide, front.PalletCount);
            Assert.Equal(DynamicRackDefaults.DefaultLoadLevels, front.LoadLevels);
            Assert.Equal(DynamicRackDefaults.DefaultPalletsDeep, front.PalletsDeep);
            Assert.True(front.Cells.Count >= front.LoadLevels);
        }

        [Fact]
        public void SetFrontCount_Grow_ClonesTheSelectedTemplate_AndRenumbers()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.Fronts[0].PalletCount = 4;
            matrix.Fronts[0].LoadLevels = 2;
            matrix.Fronts[0].EnsureCellCount(2);
            matrix.Fronts[0].Cells[0].PalletFront = 50.0;

            matrix.SetFrontCount(3);

            Assert.Equal(3, matrix.Count);
            Assert.Equal(new[] { 1, 2, 3 }, matrix.Fronts.Select(f => f.Index).ToArray());
            Assert.All(matrix.Fronts, f => Assert.Equal(4, f.PalletCount));
            Assert.All(matrix.Fronts, f => Assert.Equal(50.0, f.Cells[0].PalletFront));
            // Clones are independent.
            matrix.Fronts[2].Cells[0].PalletFront = 12.0;
            Assert.Equal(50.0, matrix.Fronts[0].Cells[0].PalletFront);
        }

        [Fact]
        public void SetFrontCount_Shrink_RemovesTrailingFronts_AndClampsSelection()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(4);
            matrix.AdjustPositions(3, 0); // select the last front
            Assert.Equal(3, matrix.SelectedFrontIndex);

            matrix.SetFrontCount(2);

            Assert.Equal(2, matrix.Count);
            Assert.Equal(1, matrix.SelectedFrontIndex);
        }

        [Fact]
        public void AdjustPositions_And_AdjustLevels_StayAtLeastOne_AndSelectTheFront()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(2);
            matrix.Fronts[1].PalletCount = 1;
            matrix.Fronts[1].LoadLevels = 1;

            matrix.AdjustPositions(1, -5);
            Assert.Equal(1, matrix.Fronts[1].PalletCount);
            Assert.Equal(1, matrix.SelectedFrontIndex);

            matrix.AdjustLevels(1, -5);
            Assert.Equal(1, matrix.Fronts[1].LoadLevels);

            matrix.AdjustPositions(0, 2);
            Assert.Equal(DynamicRackDefaults.DefaultPalletsWide + 2, matrix.Fronts[0].PalletCount);
            Assert.Equal(0, matrix.SelectedFrontIndex);
        }

        [Fact]
        public void ToggleCell_NoExtend_ReplacesSelectionWithASingleCell()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(2);

            matrix.ToggleCell(1, 1, extendSelection: false);

            Assert.Equal(1, matrix.SelectedCellCount);
            Assert.True(matrix.IsSelected(1, 1));
            Assert.Equal(1, matrix.SelectedFrontIndex);
            Assert.Equal(1, matrix.SelectedLevelIndex);
        }

        [Fact]
        public void ToggleCell_Extend_AddsAndRemovesButNeverGoesEmpty()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(2);
            matrix.ToggleCell(0, 0, extendSelection: false);

            matrix.ToggleCell(1, 2, extendSelection: true);
            Assert.Equal(2, matrix.SelectedCellCount);
            Assert.True(matrix.IsSelected(0, 0));
            Assert.True(matrix.IsSelected(1, 2));
            Assert.Equal(1, matrix.SelectedFrontIndex);
            Assert.Equal(2, matrix.SelectedLevelIndex);

            // Removing the current primary re-seats the primary onto a remaining cell.
            matrix.ToggleCell(1, 2, extendSelection: true);
            Assert.Equal(1, matrix.SelectedCellCount);
            Assert.True(matrix.IsSelected(0, 0));
            Assert.Equal(0, matrix.SelectedFrontIndex);
            Assert.Equal(0, matrix.SelectedLevelIndex);

            // Extend-toggling the ONLY remaining cell keeps it (never empties the selection).
            matrix.ToggleCell(0, 0, extendSelection: true);
            Assert.Equal(1, matrix.SelectedCellCount);
            Assert.True(matrix.IsSelected(0, 0));
        }

        [Fact]
        public void NormalizeSelection_DropsCellsBeyondShrunkLevels()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.Fronts[0].LoadLevels = 4;
            matrix.ToggleCell(0, 3, extendSelection: false);
            Assert.Equal(3, matrix.SelectedLevelIndex);

            matrix.Fronts[0].LoadLevels = 2;
            matrix.NormalizeSelection();

            Assert.True(matrix.SelectedLevelIndex <= 1);
            Assert.DoesNotContain(matrix.SelectedCells(), a => a.LevelIndex >= 2);
        }

        [Fact]
        public void CommitEditorValues_AppliesFrontAndCellValuesToTheSelection()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.ToggleCell(0, 0, extendSelection: false);

            matrix.CommitEditorValues(Values(palletCount: 3, levels: 4, deep: 6, front: 45.0));

            var front = matrix.Fronts[0];
            Assert.Equal(3, front.PalletCount);
            Assert.Equal(4, front.LoadLevels);
            Assert.Equal(6, front.PalletsDeep);
            Assert.Equal(45.0, front.Cells[0].PalletFront);
        }

        [Fact]
        public void ApplyScope_Cell_WritesOnlyTheSelectedCell()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(2);
            matrix.Fronts[0].LoadLevels = 2;
            matrix.Fronts[1].LoadLevels = 2;
            matrix.ToggleCell(0, 0, extendSelection: false);

            var count = matrix.ApplyScope(Values(levels: 2, front: 77.0), DynamicRackCellScope.Cell);

            Assert.Equal(1, count);
            Assert.Equal(77.0, matrix.Fronts[0].Cells[0].PalletFront);
            Assert.NotEqual(77.0, matrix.Fronts[1].Cells[0].PalletFront);
        }

        [Fact]
        public void ApplyScope_All_WritesEveryCell()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(2);
            matrix.Fronts[0].LoadLevels = 2;
            matrix.Fronts[1].LoadLevels = 2;
            matrix.ToggleCell(0, 0, extendSelection: false);

            var count = matrix.ApplyScope(Values(levels: 2, front: 88.0), DynamicRackCellScope.All);

            Assert.Equal(4, count);
            foreach (var front in matrix.Fronts)
            {
                foreach (var cell in front.Cells.Take(front.LoadLevels))
                {
                    Assert.Equal(88.0, cell.PalletFront);
                }
            }
        }

        [Fact]
        public void ApplyScope_Selected_WritesExactlyTheSelectedAddresses()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(3);
            foreach (var f in matrix.Fronts) f.LoadLevels = 2;
            matrix.ToggleCell(0, 0, extendSelection: false);
            matrix.ToggleCell(2, 1, extendSelection: true);

            var count = matrix.ApplyScope(Values(levels: 2, front: 66.0), DynamicRackCellScope.Selected);

            Assert.Equal(2, count);
            Assert.Equal(66.0, matrix.Fronts[0].Cells[0].PalletFront);
            Assert.Equal(66.0, matrix.Fronts[2].Cells[1].PalletFront);
            Assert.NotEqual(66.0, matrix.Fronts[1].Cells[0].PalletFront);
        }

        [Fact]
        public void NormalizeFrontTargets_FiltersInvalidAndDeduplicates()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(3);

            var targets = matrix.NormalizeFrontTargets(new[] { -1, 0, 0, 2, 5 });

            Assert.Equal(new[] { 0, 2 }, targets.ToArray());
        }

        [Fact]
        public void SnapshotAndRestore_RollBackToAnIndependentCopy()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(2);
            matrix.Fronts[0].Cells[0].PalletFront = 40.0;

            var snapshot = matrix.Snapshot();
            matrix.Fronts[0].Cells[0].PalletFront = 99.0;
            matrix.SetFrontCount(1);

            matrix.Restore(snapshot);

            Assert.Equal(2, matrix.Count);
            Assert.Equal(40.0, matrix.Fronts[0].Cells[0].PalletFront);
        }

        [Fact]
        public void RefreshFromResolved_MatchingCount_CopiesResolvedValues()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(2);

            var resolved = new List<DynamicRackFront>
            {
                ResolvedFront(1, palletCount: 3, levels: 2, bfr: 44.0, beam: 130.0),
                ResolvedFront(2, palletCount: 4, levels: 3, bfr: 46.0, beam: 140.0)
            };

            Assert.True(matrix.RefreshFromResolved(resolved));
            Assert.Equal(44.0, matrix.Fronts[0].Bfr);
            Assert.Equal(130.0, matrix.Fronts[0].BeamLength);
            Assert.Equal(3, matrix.Fronts[1].LoadLevels);
            Assert.Equal(3, matrix.Fronts[1].Cells.Count);
        }

        [Fact]
        public void RefreshFromResolved_MismatchedCount_ReturnsFalse_AndLeavesGridUntouched()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(2);
            var before = matrix.Fronts[0].PalletCount;

            var refreshed = matrix.RefreshFromResolved(new List<DynamicRackFront> { ResolvedFront(1, 3, 2, 44.0, 130.0) });

            Assert.False(refreshed);
            Assert.Equal(before, matrix.Fronts[0].PalletCount);
        }

        [Fact]
        public void RestoreFromResolved_RebuildsFronts()
        {
            var matrix = new DynamicFrontMatrix();
            var resolved = new List<DynamicRackFront>
            {
                ResolvedFront(1, 2, 2, 44.0, 130.0),
                ResolvedFront(2, 3, 3, 46.0, 140.0),
                ResolvedFront(3, 4, 1, 48.0, 150.0)
            };

            matrix.RestoreFromResolved(resolved);

            Assert.Equal(3, matrix.Count);
            Assert.Equal(new[] { 1, 2, 3 }, matrix.Fronts.Select(f => f.Index).ToArray());
            Assert.Equal(2, matrix.Fronts[0].Cells.Count);
        }

        [Fact]
        public void RestoreFromResolved_Empty_KeepsOneDefaultFront()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(3);

            matrix.RestoreFromResolved(new List<DynamicRackFront>());

            Assert.Equal(1, matrix.Count);
            Assert.Equal(DynamicRackDefaults.DefaultPalletsWide, matrix.Fronts[0].PalletCount);
        }

        [Fact]
        public void BuildFrontDesigns_ProjectsEachFrontAndTakesOnlyLoadLevelCells()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.Fronts[0].PalletCount = 3;
            matrix.Fronts[0].LoadLevels = 2;
            matrix.Fronts[0].DepthStartPosition = 1;
            matrix.Fronts[0].PalletsDeep = 5;
            matrix.Fronts[0].EnsureCellCount(4); // more cells than levels on purpose
            matrix.Fronts[0].Cells[0].IntermediateBeamDepth = 3.5;

            var designs = matrix.BuildFrontDesigns();

            Assert.Single(designs);
            var design = designs[0];
            Assert.Equal(3, design.PalletCount);
            Assert.Equal(2, design.LoadLevels);
            Assert.Equal(2, design.Levels.Count);              // only LoadLevels cells are projected
            Assert.Equal(2, design.IntermediateBeamDepths.Count);
            Assert.Equal(3.5, design.IntermediateBeamDepths[0]);
        }

        [Fact]
        public void SelectedEditorDiffers_TrueWhenAFieldChanges_FalseWhenEqual()
        {
            var matrix = new DynamicFrontMatrix();
            matrix.ToggleCell(0, 0, extendSelection: false);
            var values = Values(
                palletCount: matrix.Fronts[0].PalletCount,
                levels: matrix.Fronts[0].LoadLevels,
                deep: matrix.Fronts[0].PalletsDeep,
                depthStart: matrix.Fronts[0].DepthStartPosition);
            matrix.CommitEditorValues(values);

            // Re-reading the same buffer must report "no change".
            Assert.False(matrix.SelectedEditorDiffers(values));

            var changed = Values(
                palletCount: matrix.Fronts[0].PalletCount + 1,
                levels: matrix.Fronts[0].LoadLevels,
                deep: matrix.Fronts[0].PalletsDeep,
                depthStart: matrix.Fronts[0].DepthStartPosition);
            Assert.True(matrix.SelectedEditorDiffers(changed));
        }

        [Fact]
        public void SelectedEditorDiffers_NullValues_IsFalse()
        {
            var matrix = new DynamicFrontMatrix();
            Assert.False(matrix.SelectedEditorDiffers(null));
        }
    }
}
