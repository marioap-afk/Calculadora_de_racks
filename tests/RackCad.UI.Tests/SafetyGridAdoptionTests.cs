using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>Characterization (STA) of the safety grids that adopt <see cref="RackCad.UI.Controls.SelectionMatrix"/>
    /// (I-22): the real window builds over a jagged model and still loads / returns exactly the off-cell set it did with
    /// the hand-built CheckBox grid, ignoring absent (jagged) slots.</summary>
    public sealed class SafetyGridAdoptionTests
    {
        [Fact]
        public void GuiaEntrada_LoadsOffCells_TogglesAndReturnsThem_ExcludingAbsent()
        {
            var afterCount = StaTestRunner.Run(() =>
            {
                var levels = new[] { 2, 3, 1 };
                var off = new List<SelectiveGridCell> { new SelectiveGridCell { Frente = 0, Level = 0 } };
                var window = new SafetyGuiaEntradaGridWindow("Guía", levels, off);

                // Loaded: the input off-cell is off; the absent slots (0,2), (2,1), (2,2) are ignored.
                var loaded = window.CurrentOffCells();
                Assert.Single(loaded);
                Assert.Contains(loaded, c => c.Frente == 0 && c.Level == 0);

                window.Model.SetSelected(1, 2, false); // front 1 has 3 levels, so (1,2) is present
                var after = window.CurrentOffCells();
                Assert.Contains(after, c => c.Frente == 1 && c.Level == 2);
                Assert.DoesNotContain(after, c => c.Frente == 2 && c.Level == 1); // an absent slot never appears
                return after.Count;
            });

            Assert.Equal(2, afterCount);
        }

        [Fact]
        public void GuiaEntrada_LoadedOffCellOnAnAbsentSlot_IsIgnored()
        {
            var loadedCount = StaTestRunner.Run(() =>
            {
                // (2,1) is absent (front 2 has 1 level): a persisted off-cell there must be dropped, not honored.
                var off = new List<SelectiveGridCell> { new SelectiveGridCell { Frente = 2, Level = 1 } };
                return new SafetyGuiaEntradaGridWindow("Guía", new[] { 2, 3, 1 }, off).CurrentOffCells().Count;
            });

            Assert.Equal(0, loadedCount);
        }

        [Fact]
        public void GuiaEntrada_SetAllFalse_TurnsOffOnlyPresentCells()
        {
            var count = StaTestRunner.Run(() =>
            {
                var window = new SafetyGuiaEntradaGridWindow("Guía", new[] { 2, 3, 1 }, null);
                window.Model.SetAll(false);
                return window.CurrentOffCells().Count;
            });

            Assert.Equal(6, count); // 2 + 3 + 1 present cells, not the 9 of the bounding rectangle
        }
    }
}
