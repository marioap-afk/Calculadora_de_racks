using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-33 — STA tests of the REAL safety grids opened by the Dinámico and by Push Back when a front is EN BLANCO.
    /// They lock the four things the residue was about: a blank front's level cells are ABSENT (not drawn, not
    /// selectable, not appliable), its stored configuration stays DORMANT across the dialog, reactivating brings it
    /// back editable and intact, and the SELECTIVO — which has no blank fronts — is untouched.
    /// </summary>
    public sealed class BlankFrontSafetyGridTests
    {
        private static SelectiveGridCell Cell(int frente, int level)
            => new SelectiveGridCell { Frente = frente, Level = level };

        private static (int Frente, int Level)[] Pairs(IEnumerable<SelectiveGridCell> cells)
            => cells.Select(cell => (cell.Frente, cell.Level)).OrderBy(p => p.Frente).ThenBy(p => p.Level).ToArray();

        /// <summary>
        /// A PRESENT cell really is editable: toggling FLIPS it and the off-set moves by exactly one, whatever state
        /// the cell started in; toggling back restores it. (<c>Toggle</c> returns the cell's NEW state, so its value
        /// alone proves nothing — an absent cell and an on-cell being switched off both answer false.)
        /// </summary>
        private static void AssertEditable(SelectionMatrixModel model, int column, int row)
        {
            Assert.False(model.IsAbsent(column, row));
            var offBefore = model.UnselectedCount;
            var wasOff = model.UnselectedCells().Any(cell => cell.Column == column && cell.Row == row);

            Assert.Equal(wasOff, model.Toggle(column, row));          // flipped: OFF -> ON returns true, ON -> OFF false
            Assert.Equal(wasOff ? offBefore - 1 : offBefore + 1, model.UnselectedCount);

            Assert.Equal(!wasOff, model.Toggle(column, row));         // and back
            Assert.Equal(offBefore, model.UnselectedCount);
        }

        /// <summary>An ABSENT cell cannot be selected or applied to: toggling changes nothing at all.</summary>
        private static void AssertNotSelectable(SelectionMatrixModel model, int column, int row)
        {
            Assert.True(model.IsAbsent(column, row));
            var before = model.UnselectedCount;
            Assert.False(model.Toggle(column, row));
            Assert.Equal(before, model.UnselectedCount);
        }

        // ---- The desviador grid (per POST), which both systems open --------------------------------------------

        private static SafetyDesviadorGridWindow Desviador(
            IReadOnlyList<int> levelsPerPost, IEnumerable<SelectiveGridCell> offCells, bool allowBlank)
            => new SafetyDesviadorGridWindow(
                "DESVIADOR", "Desviador", system: null, catalog: null,
                longitud: 12.0, firstHeight: 12.0, side: SafetySide.Both,
                offCells: offCells,
                fallbackPostCount: levelsPerPost.Count,
                fallbackLevelsPerFrente: levelsPerPost,
                fallbackLevelsArePerPost: true,
                showSide: true,
                allowBlankColumns: allowBlank);

        [Fact]
        public void Desviador_ActiveFronts_BehaveExactlyAsBefore()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Desviador(new[] { 3, 3, 2 }, new[] { Cell(1, 1) }, allowBlank: true);
                try
                {
                    // Nothing is absent beyond the ordinary jagged tail: only (2,2), the shorter last post's top row.
                    Assert.Equal(1, grid.Model.AbsentCount);
                    Assert.True(grid.Model.IsAbsent(2, 2));
                    AssertEditable(grid.Model, 0, 0);
                    AssertEditable(grid.Model, 1, 1);
                    Assert.Equal(new[] { (1, 1) }, Pairs(grid.PersistedOffCells()));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Desviador_ABlankColumn_IsAbsentUnselectableAndKeepsItsStoredCellsDormant()
        {
            StaTestRunner.Run(() =>
            {
                // Post 1's only neighbours are blank, so the authority hands the dialog a zero for it.
                var stored = new[] { Cell(0, 0), Cell(1, 0), Cell(1, 2) };
                var grid = Desviador(new[] { 3, 0, 3 }, stored, allowBlank: true);
                try
                {
                    // 1) The whole column is ABSENT: not drawn, not selectable, and toggling reports NO change.
                    for (var level = 0; level < 3; level++)
                    {
                        AssertNotSelectable(grid.Model, 1, level);
                    }

                    // The live view cannot even see them...
                    Assert.DoesNotContain(Pairs(grid.CurrentOffCells()), pair => pair.Frente == 1);

                    // 2) ...but what the dialog PERSISTS keeps them intact, so the dormant configuration survives.
                    Assert.Equal(
                        new[] { (0, 0), (1, 0), (1, 2) },
                        Pairs(grid.PersistedOffCells()));

                    // 3) Applying changes elsewhere does not disturb the dormant cells.
                    grid.Model.Toggle(2, 0);
                    Assert.Equal(
                        new[] { (0, 0), (1, 0), (1, 2), (2, 0) },
                        Pairs(grid.PersistedOffCells()));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Desviador_ReactivatingTheFront_BringsTheStoredCellsBackEditable()
        {
            StaTestRunner.Run(() =>
            {
                var stored = new[] { Cell(1, 0), Cell(1, 2) };

                // While blank the cells are untouchable but preserved...
                var blank = Desviador(new[] { 3, 0, 3 }, stored, allowBlank: true);
                IReadOnlyList<SelectiveGridCell> persisted;
                try { persisted = blank.PersistedOffCells(); }
                finally { blank.Close(); }

                // ...and reopening once the front is ACTIVE again shows them, off and editable, unchanged.
                var active = Desviador(new[] { 3, 3, 3 }, persisted, allowBlank: true);
                try
                {
                    Assert.False(active.Model.IsAbsent(1, 0));
                    Assert.False(active.Model.IsAbsent(1, 2));
                    Assert.Equal(new[] { (1, 0), (1, 2) }, Pairs(active.CurrentOffCells()));
                    // Editable again: switching one back ON is honoured and the persisted set follows.
                    Assert.True(active.Model.Toggle(1, 0));
                    Assert.Equal(new[] { (1, 2) }, Pairs(active.PersistedOffCells()));
                }
                finally { active.Close(); }
            });
        }

        [Fact]
        public void Desviador_ConsecutiveBlankFronts_LeaveTheirSharedPostEmptyWithoutLosingAnything()
        {
            StaTestRunner.Run(() =>
            {
                var stored = new[] { Cell(1, 0), Cell(2, 1) };
                var grid = Desviador(new[] { 3, 0, 0, 3 }, stored, allowBlank: true);
                try
                {
                    for (var level = 0; level < 3; level++)
                    {
                        AssertNotSelectable(grid.Model, 1, level);
                        AssertNotSelectable(grid.Model, 2, level);
                    }

                    Assert.Equal(new[] { (1, 0), (2, 1) }, Pairs(grid.PersistedOffCells()));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Desviador_WithoutTheOptIn_KeepsTheHistoricalFlooring_SoTheSelectivoIsUnchanged()
        {
            StaTestRunner.Run(() =>
            {
                // The Selectivo never opts in: a zero is still floored to one level and nothing becomes absent.
                var grid = Desviador(new[] { 3, 0, 3 }, new SelectiveGridCell[0], allowBlank: false);
                try
                {
                    AssertEditable(grid.Model, 1, 0);
                }
                finally { grid.Close(); }
            });
        }

        // ---- The entrance-guide grid (per FRONT), which only the Dinámico opens --------------------------------

        [Fact]
        public void Guia_ABlankFront_IsAbsentAndItsStoredCellsStayDormant()
        {
            StaTestRunner.Run(() =>
            {
                var stored = new[] { Cell(0, 0), Cell(1, 1) };
                var grid = new SafetyGuiaEntradaGridWindow("Guía", new[] { 2, 0, 3 }, stored, allowBlankColumns: true);
                try
                {
                    AssertNotSelectable(grid.Model, 1, 1);
                    Assert.DoesNotContain(Pairs(grid.CurrentOffCells()), pair => pair.Frente == 1);
                    Assert.Equal(new[] { (0, 0), (1, 1) }, Pairs(grid.PersistedOffCells()));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Guia_WithoutTheOptIn_KeepsTheHistoricalFlooring()
        {
            StaTestRunner.Run(() =>
            {
                var grid = new SafetyGuiaEntradaGridWindow("Guía", new[] { 2, 0, 3 }, new SelectiveGridCell[0]);
                try
                {
                    AssertEditable(grid.Model, 1, 0);
                }
                finally { grid.Close(); }
            });
        }

        // ---- The tope grid (per FRONT), shared by the Selectivo and by Push Back's rear stop -------------------

        [Fact]
        public void Tope_ABlankFront_IsAbsentAndItsStoredCellsStayDormant()
        {
            StaTestRunner.Run(() =>
            {
                var stored = new[] { Cell(0, 1), Cell(1, 0) };
                var grid = new SafetyTopeGridWindow(
                    "Tope", new[] { 2, 0, 3 }, shared: false, side: SafetySide.Both, saque: 2.0,
                    frontal: false, offCells: stored);
                try
                {
                    AssertNotSelectable(grid.Model, 1, 0);

                    var result = grid.BuildResult();
                    Assert.NotNull(result);
                    Assert.Equal(new[] { (0, 1), (1, 0) }, Pairs(result.OffCells));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Tope_WithoutABlankFront_ReportsExactlyWhatItShows_SoTheSelectivoIsUnchanged()
        {
            StaTestRunner.Run(() =>
            {
                // A jagged grid with no zero column: out-of-range stored cells keep being dropped, as always.
                var stored = new[] { Cell(0, 1), Cell(1, 5) };
                var grid = new SafetyTopeGridWindow(
                    "Tope", new[] { 2, 1, 3 }, shared: false, side: SafetySide.Both, saque: 2.0,
                    frontal: false, offCells: stored);
                try
                {
                    var result = grid.BuildResult();
                    Assert.NotNull(result);
                    Assert.Equal(new[] { (0, 1) }, Pairs(result.OffCells));
                }
                finally { grid.Close(); }
            });
        }

        // ---- What the two windows actually hand the dialog ----------------------------------------------------

        private static PushBackDesign PushBackDesign(params int[] levels)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = levels.Max(),
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            foreach (var level in levels)
            {
                design.Structure.Fronts.Add(new DynamicRackFrontDesign
                {
                    PalletCount = 1, LoadLevels = level, PalletsDeep = 4, DepthStartPosition = 1
                });
                design.Fronts.Add(new PushBackFrontConfig());
            }

            return design;
        }

        [Fact]
        public void PushBack_HandsTheDesviadorZeroLevelsForABlankFront()
        {
            var perPost = StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                window.LoadExisting(PushBackDesign(3, 2, 4), "GUID-PB-I33", "PB");
                var before = window.DesviadorLevelsPerPost().ToArray();
                window.State.SetActive(1, false);
                return (Before: before, After: window.DesviadorLevelsPerPost().ToArray());
            });

            // Blanking the middle front leaves both its posts on their ACTIVE neighbour's cut, not on its own.
            Assert.Equal(new[] { 3, 3, 4, 4 }, perPost.Before);
            Assert.Equal(new[] { 3, 3, 4, 4 }, perPost.After);
        }

        [Fact]
        public void PushBack_HandsTheDesviadorAnEmptyColumnBetweenTwoBlankFronts()
        {
            var perPost = StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                window.LoadExisting(PushBackDesign(3, 2, 2, 4), "GUID-PB-I33B", "PB");
                window.State.SetActive(1, false);
                window.State.SetActive(2, false);
                return window.DesviadorLevelsPerPost().ToArray();
            });

            Assert.Equal(new[] { 3, 3, 0, 4, 4 }, perPost);
        }
    }
}
