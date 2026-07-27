using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-33 — the safety side of the blank-front contract, in its PURE form: the per-front and per-post level counts a
    /// safety dialog receives, and the rule that keeps a blank front's stored cells DORMANT across that dialog. The STA
    /// tests over the real grids consume exactly these two things, so what is asserted here is the same rule the UI runs.
    /// </summary>
    public class BlankFrontSafetyTests
    {
        private static DynamicFrontMatrix Matrix(int fronts, params int[] levels)
        {
            var matrix = new DynamicFrontMatrix();
            matrix.SetFrontCount(fronts);
            for (var index = 0; index < fronts && index < levels.Length; index++)
            {
                matrix.AdjustLevels(index, levels[index] - matrix.Fronts[index].LoadLevels);
            }

            return matrix;
        }

        private static SelectiveGridCell Cell(int frente, int level)
            => new SelectiveGridCell { Frente = frente, Level = level };

        private static (int Frente, int Level)[] Pairs(IEnumerable<SelectiveGridCell> cells)
            => cells.Select(cell => (cell.Frente, cell.Level)).OrderBy(p => p.Frente).ThenBy(p => p.Level).ToArray();

        // ---- The counts the dialogs receive ------------------------------------------------------------------

        [Fact]
        public void ActiveFronts_FeedTheDialogTheSameCountsAsBefore()
        {
            var matrix = Matrix(3, 3, 2, 4);

            Assert.Equal(new[] { 3, 2, 4 }, matrix.EffectiveLevelCounts().ToArray());
            // A post takes the tallest adjacent front, exactly as the drawing does.
            Assert.Equal(
                new[] { 3, 3, 4, 4 },
                DynamicFrontActivation.EffectiveLevelsPerPost(matrix.EffectiveLevelCounts()).ToArray());
        }

        [Fact]
        public void ABlankFront_FeedsTheDialogZeroLevels_SoItsColumnHasNoCells()
        {
            var matrix = Matrix(3, 3, 2, 4);

            matrix.SetActive(1, false);

            Assert.Equal(new[] { 3, 0, 4 }, matrix.EffectiveLevelCounts().ToArray());
            // Posts 1 and 2 touch the blank front but keep their ACTIVE neighbour's cut.
            Assert.Equal(
                new[] { 3, 3, 4, 4 },
                DynamicFrontActivation.EffectiveLevelsPerPost(matrix.EffectiveLevelCounts()).ToArray());
        }

        [Fact]
        public void ConsecutiveBlankFronts_LeaveTheirSharedPostWithNoLevelsAtAll()
        {
            var matrix = Matrix(4, 3, 2, 2, 4);

            matrix.SetActive(1, false);
            matrix.SetActive(2, false);

            Assert.Equal(new[] { 3, 0, 0, 4 }, matrix.EffectiveLevelCounts().ToArray());
            // Post 2 sits between the two blank fronts: no adjacent front carries load, so its column is empty.
            Assert.Equal(
                new[] { 3, 3, 0, 4, 4 },
                DynamicFrontActivation.EffectiveLevelsPerPost(matrix.EffectiveLevelCounts()).ToArray());
        }

        [Fact]
        public void ReactivatingRestoresTheCountsTheFrontHadBeforeBeingBlanked()
        {
            var matrix = Matrix(3, 3, 2, 4);
            var before = matrix.EffectiveLevelCounts().ToArray();

            matrix.SetActive(1, false);
            matrix.SetActive(1, true);

            Assert.Equal(before, matrix.EffectiveLevelCounts().ToArray());
        }

        // ---- Dormancy of the stored cells --------------------------------------------------------------------

        [Fact]
        public void Dormancy_KeepsTheStoredCellsOfABlankColumnAndDropsNothingElse()
        {
            var levels = new[] { 3, 0, 4 };                       // front 1 en blanco
            var stored = new[] { Cell(0, 1), Cell(1, 0), Cell(1, 2), Cell(2, 3) };
            // The grid only ever shows columns 0 and 2, so those are the only ones it can report back.
            var live = new[] { Cell(0, 1), Cell(2, 3) };

            var merged = SafetyDormantCells.Merge(live, stored, levels);

            Assert.Equal(
                new[] { (0, 1), (1, 0), (1, 2), (2, 3) },
                Pairs(merged));
        }

        [Fact]
        public void Dormancy_DoesNotResurrectACellTheUserTurnedOnInAVisibleColumn()
        {
            var levels = new[] { 3, 0, 4 };
            var stored = new[] { Cell(0, 1), Cell(1, 0) };
            var live = new SelectiveGridCell[0];                  // the user switched (0,1) back ON

            var merged = SafetyDormantCells.Merge(live, stored, levels);

            // Only the dormant cell of the blank column survives; the visible one obeys the user.
            Assert.Equal(new[] { (1, 0) }, Pairs(merged));
        }

        [Fact]
        public void Dormancy_IsANoOpForAJaggedGridWithoutBlankFronts()
        {
            // A merely SHORTER column is not dormant: its out-of-range cells keep being dropped, as they always were.
            var levels = new[] { 3, 1, 4 };
            var stored = new[] { Cell(1, 0), Cell(1, 2) };        // (1,2) is past front 1's single level
            var live = new[] { Cell(1, 0) };

            var merged = SafetyDormantCells.Merge(live, stored, levels);

            Assert.Equal(new[] { (1, 0) }, Pairs(merged));
            Assert.False(SafetyDormantCells.IsDormantColumn(levels, 1));
            Assert.True(SafetyDormantCells.IsDormantColumn(new[] { 3, 0, 4 }, 1));
            // A column outside the supplied counts never had cells to preserve.
            Assert.False(SafetyDormantCells.IsDormantColumn(levels, 9));
        }

        [Fact]
        public void Dormancy_SurvivesAFullBlankThenReactivateRoundTrip()
        {
            var active = new[] { 3, 2, 4 };
            var blanked = new[] { 3, 0, 4 };
            var stored = new[] { Cell(1, 0), Cell(1, 1) };        // two cells switched off while the front was active

            // Opening and accepting the dialog while the front is EN BLANCO must not touch those cells...
            var afterBlankDialog = SafetyDormantCells.Merge(new SelectiveGridCell[0], stored, blanked);
            Assert.Equal(new[] { (1, 0), (1, 1) }, Pairs(afterBlankDialog));

            // ...and once reactivated the grid shows them again, editable, with nothing added or lost.
            var afterReactivation = SafetyDormantCells.Merge(afterBlankDialog, afterBlankDialog, active);
            Assert.Equal(new[] { (1, 0), (1, 1) }, Pairs(afterReactivation));
        }

        [Fact]
        public void Dormancy_HandlesConsecutiveBlankFrontsAndAnEmptyStoredSet()
        {
            var levels = new[] { 3, 0, 0, 4 };
            var stored = new[] { Cell(1, 0), Cell(2, 1), Cell(2, 3) };

            Assert.Equal(
                new[] { (1, 0), (2, 1), (2, 3) },
                Pairs(SafetyDormantCells.Merge(new SelectiveGridCell[0], stored, levels)));
            Assert.Empty(SafetyDormantCells.Merge(null, null, levels));
            Assert.Empty(SafetyDormantCells.Merge(new SelectiveGridCell[0], stored, null));
        }
    }
}
