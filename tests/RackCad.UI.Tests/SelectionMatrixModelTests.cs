using System.Collections.Generic;
using System.Linq;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>The pure grid state behind <see cref="SelectionMatrix"/>. No WPF.</summary>
    public sealed class SelectionMatrixModelTests
    {
        [Fact]
        public void NewModel_DefaultsAllSelected()
        {
            var model = new SelectionMatrixModel(3, 2);

            Assert.Equal(6, model.CellCount);
            Assert.Equal(6, model.SelectedCount);
            Assert.Equal(0, model.UnselectedCount);
            Assert.Empty(model.UnselectedCells());
        }

        [Fact]
        public void NewModel_InitialUnselected_StartsAllOff()
        {
            var model = new SelectionMatrixModel(2, 2, initialSelected: false);

            Assert.Equal(0, model.SelectedCount);
            Assert.Equal(4, model.UnselectedCount);
        }

        [Fact]
        public void SetSelected_RecordsOffCell()
        {
            var model = new SelectionMatrixModel(3, 2);

            model.SetSelected(1, 1, false);

            Assert.False(model[1, 1]);
            Assert.Equal(1, model.UnselectedCount);
            Assert.Contains(new SelectionMatrixCell(1, 1), model.UnselectedCells());
        }

        [Fact]
        public void CellChanged_FiresOnRealChangeOnly()
        {
            var model = new SelectionMatrixModel(2, 2);
            var events = new List<SelectionMatrixCell>();
            model.CellChanged += (_, e) => events.Add(e.Cell);

            model.SetSelected(0, 0, false); // change
            model.SetSelected(0, 0, false); // no-op, must not fire

            Assert.Single(events);
            Assert.Equal(new SelectionMatrixCell(0, 0), events[0]);
        }

        [Fact]
        public void Toggle_FlipsAndReturnsNewState()
        {
            var model = new SelectionMatrixModel(1, 1);

            var afterFirst = model.Toggle(0, 0);
            var afterSecond = model.Toggle(0, 0);

            Assert.False(afterFirst);
            Assert.True(afterSecond);
        }

        [Fact]
        public void SetAll_RaisesBulkChangedOnce_NotCellChanged()
        {
            var model = new SelectionMatrixModel(3, 3);
            var bulk = 0;
            var cell = 0;
            model.BulkChanged += (_, __) => bulk++;
            model.CellChanged += (_, __) => cell++;

            model.SetAll(false);

            Assert.Equal(0, model.SelectedCount);
            Assert.Equal(1, bulk);
            Assert.Equal(0, cell); // bulk change must not spam per-cell events
        }

        [Fact]
        public void WithUnselected_LoadsOffCells_AndIgnoresOutOfRange()
        {
            var off = new[]
            {
                new SelectionMatrixCell(0, 0),
                new SelectionMatrixCell(2, 1),
                new SelectionMatrixCell(9, 9), // out of range — tolerated, not an error
            };

            var model = SelectionMatrixModel.WithUnselected(3, 2, off);

            Assert.False(model[0, 0]);
            Assert.False(model[2, 1]);
            Assert.Equal(2, model.UnselectedCount);
            Assert.True(model[1, 0]);
        }

        [Fact]
        public void OutOfRange_Throws()
        {
            var model = new SelectionMatrixModel(2, 2);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => model[2, 0]);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => model.SetSelected(0, -1, false));
        }

        [Fact]
        public void UnselectedCells_AreColumnMajorOrdered()
        {
            var model = new SelectionMatrixModel(2, 2, initialSelected: false);

            var cells = model.UnselectedCells().ToList();

            // column 0 rows 0,1 then column 1 rows 0,1
            Assert.Equal(new SelectionMatrixCell(0, 0), cells[0]);
            Assert.Equal(new SelectionMatrixCell(0, 1), cells[1]);
            Assert.Equal(new SelectionMatrixCell(1, 0), cells[2]);
            Assert.Equal(new SelectionMatrixCell(1, 1), cells[3]);
        }
    }
}
