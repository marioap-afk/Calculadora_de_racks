using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The absent-cell extension of <see cref="SelectionMatrixModel"/>/<see cref="SelectionMatrix"/> (I-22):
    /// jagged safety grids (each frente/post with its own level count) build a rectangle whose short columns leave the
    /// top slots ABSENT — never drawn, never selectable, excluded from the on/off queries. A rectangular grid keeps
    /// exactly the pre-I-22 behavior (no absent cells).</summary>
    public sealed class SelectionMatrixAbsentCellTests
    {
        // rowsPerColumn [2,3,1] -> 3 cols x 3 rows; absent = (0,2), (2,1), (2,2); 6 present cells.
        private static readonly IReadOnlyList<int> Jagged = new[] { 2, 3, 1 };

        [Fact]
        public void WithJaggedColumns_MarksTheShortColumnsTopSlotsAbsent()
        {
            var model = SelectionMatrixModel.WithJaggedColumns(Jagged, unselected: null);

            Assert.Equal(3, model.Columns);
            Assert.Equal(3, model.Rows);
            Assert.True(model.HasAbsentCells);
            Assert.True(model.IsAbsent(0, 2));
            Assert.True(model.IsAbsent(2, 1));
            Assert.True(model.IsAbsent(2, 2));
            Assert.False(model.IsAbsent(0, 0));
            Assert.False(model.IsAbsent(1, 2));   // the tall column has no absent cell
        }

        [Fact]
        public void RectangularGrid_HasNoAbsentCells()
        {
            var model = new SelectionMatrixModel(3, 2);
            Assert.False(model.HasAbsentCells);
            for (var c = 0; c < 3; c++)
            {
                for (var r = 0; r < 2; r++)
                {
                    Assert.False(model.IsAbsent(c, r));
                }
            }
        }

        [Fact]
        public void UnselectedAndSelectedCells_ExcludeAbsent()
        {
            var model = SelectionMatrixModel.WithJaggedColumns(Jagged, new[] { new SelectionMatrixCell(0, 0) });

            var off = model.UnselectedCells();
            Assert.Equal(new SelectionMatrixCell(0, 0), Assert.Single(off));   // only the real off cell
            Assert.Equal(5, model.SelectedCells().Count);                       // 6 present - 1 off
            Assert.Equal(5, model.SelectedCount);
            Assert.Equal(1, model.UnselectedCount);                            // absent NOT counted as off
            Assert.DoesNotContain(new SelectionMatrixCell(0, 2), off);
        }

        [Fact]
        public void SetAllFalse_TurnsOffOnlyPresentCells()
        {
            var model = SelectionMatrixModel.WithJaggedColumns(Jagged, unselected: null);
            model.SetAll(false);

            Assert.Equal(6, model.UnselectedCount);   // the 6 present cells, not the 9 of the rectangle
            Assert.Equal(0, model.SelectedCount);
            Assert.All(model.UnselectedCells(), cell => Assert.False(model.IsAbsent(cell.Column, cell.Row)));
        }

        [Fact]
        public void SetSelected_OnAbsentCell_IsANoOp_AndRaisesNoEvent()
        {
            var model = SelectionMatrixModel.WithJaggedColumns(Jagged, unselected: null);
            var events = 0;
            model.CellChanged += (_, __) => events++;

            model.SetSelected(0, 2, false);   // (0,2) is absent
            model.SetSelected(2, 2, false);   // absent

            Assert.Equal(0, events);
            Assert.Empty(model.UnselectedCells());   // nothing turned off
        }

        [Fact]
        public void WithJaggedColumns_OffCellOnAnAbsentCoordinate_IsIgnored()
        {
            // Loading persisted off-cells tolerantly: an off cell that lands on an absent slot is dropped, not honored.
            var model = SelectionMatrixModel.WithJaggedColumns(Jagged, new[] { new SelectionMatrixCell(2, 2) });
            Assert.Empty(model.UnselectedCells());
        }

        [Fact]
        public void Control_JaggedModel_DrawsNoCheckBoxForAbsentCells()
        {
            var (total, absentCell, presentCell) = StaTestRunner.Run(() =>
            {
                var matrix = new SelectionMatrix { Model = SelectionMatrixModel.WithJaggedColumns(Jagged, unselected: null) };
                return (matrix.Children.OfType<CheckBox>().Count(), matrix.CellFor(2, 1), matrix.CellFor(0, 0));
            });

            Assert.Equal(6, total);        // one per PRESENT cell, not 9
            Assert.Null(absentCell);       // no check box at an absent slot
            Assert.NotNull(presentCell);
        }

        [Fact]
        public void CellCount_CountsPresentCellsForJagged_AndStaysRectangularCountForRectangular()
        {
            var jagged = SelectionMatrixModel.WithJaggedColumns(Jagged, unselected: null);
            Assert.Equal(6, jagged.CellCount);     // 2 + 3 + 1 present, NOT the 9 of the bounding rectangle
            Assert.Equal(3, jagged.AbsentCount);

            var rect = new SelectionMatrixModel(3, 2);
            Assert.Equal(6, rect.CellCount);       // rectangular: unchanged from I-14
            Assert.Equal(0, rect.AbsentCount);
        }

        [Fact]
        public void Toggle_OnAbsentCell_IsNoOp_ReturnsFalse_AndRaisesNoEvent()
        {
            var model = SelectionMatrixModel.WithJaggedColumns(Jagged, unselected: null);
            var events = 0;
            model.CellChanged += (_, __) => events++;

            Assert.False(model.Toggle(0, 2)); // (0,2) absent -> not selectable, no phantom change
            Assert.False(model.Toggle(2, 1)); // absent
            Assert.Equal(0, events);
            Assert.False(model.IsSelected(0, 2)); // an absent cell is not selected
            Assert.Empty(model.UnselectedCells());
        }

        [Fact]
        public void Toggle_OnPresentCell_TogglesAndReportsTheNewState()
        {
            var model = SelectionMatrixModel.WithJaggedColumns(Jagged, unselected: null);
            Assert.False(model.Toggle(0, 0)); // present, was on -> off (returns the new state)
            Assert.Equal(new SelectionMatrixCell(0, 0), Assert.Single(model.UnselectedCells()));
            Assert.True(model.Toggle(0, 0));  // off -> on
            Assert.Empty(model.UnselectedCells());
        }
    }
}
