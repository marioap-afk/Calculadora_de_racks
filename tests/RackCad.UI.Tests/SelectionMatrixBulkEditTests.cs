using System.Collections.Generic;
using System.Linq;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-34 / PB-007 — the PURE contract of a bulk edit over <see cref="SelectionMatrixModel"/>: the four scopes over
    /// rectangular and jagged grids, absent cells never touched, no-op and idempotence changing nothing, and exactly
    /// ONE aggregated notification per operation (none when nothing changed).
    /// <para>
    /// These are the regressions PB-007 needed: today the safety grids offer only "Todos"/"Ninguno", so removing the
    /// desviador from level 2 across 100 fronts costs 100 clicks. They were verified RED against the declared but
    /// inert surface before the foundation existed.
    /// </para>
    /// </summary>
    public sealed class SelectionMatrixBulkEditTests
    {
        private static (int Column, int Row)[] Pairs(IEnumerable<SelectionMatrixCell> cells)
            => cells.Select(cell => (cell.Column, cell.Row)).OrderBy(p => p.Column).ThenBy(p => p.Row).ToArray();

        /// <summary>Records every aggregated notification the model raises, so a test can assert there was EXACTLY one
        /// (or none) and inspect what it carried.</summary>
        private sealed class Recorder
        {
            private readonly List<SelectionMatrixScopeAppliedEventArgs> events =
                new List<SelectionMatrixScopeAppliedEventArgs>();

            public Recorder(SelectionMatrixModel model)
            {
                model.ScopeApplied += (_, e) => events.Add(e);
                model.CellChanged += (_, e) => CellChangedCount++;
                model.BulkChanged += (_, __) => BulkChangedCount++;
            }

            public IReadOnlyList<SelectionMatrixScopeAppliedEventArgs> Events => events;

            /// <summary>A bulk edit must NOT degenerate into one granular event per cell.</summary>
            public int CellChangedCount { get; private set; }

            /// <summary>Nor into the blunt repaint-everything signal that "Todos"/"Ninguno" uses.</summary>
            public int BulkChangedCount { get; private set; }

            public SelectionMatrixScopeAppliedEventArgs Single() => Assert.Single(events);
        }

        /// <summary>A 4 columns × 3 rows grid, all ON (how the safety grids open).</summary>
        private static SelectionMatrixModel Rectangular() => new SelectionMatrixModel(4, 3);

        /// <summary>A jagged grid: column 0 has 3 levels, column 1 has ZERO (a front EN BLANCO, I-33), column 2 has 2.
        /// Absent cells: every row of column 1, plus row 2 of column 2.</summary>
        private static SelectionMatrixModel Jagged()
            => SelectionMatrixModel.WithJaggedColumns(new[] { 3, 0, 2 }, unselected: null);

        private static SelectionMatrixBulkEditor Editor(SelectionMatrixModel model, int column = 0, int row = 0)
        {
            var editor = new SelectionMatrixBulkEditor(model);
            editor.TrySetPrimary(column, row);
            return editor;
        }

        // ---- The four scopes over a rectangular grid ------------------------------------------------------------

        [Fact]
        public void Cell_TouchesOnlyThePrimaryCell()
        {
            var model = Rectangular();
            var recorder = new Recorder(model);

            var changed = Editor(model, 2, 1).Apply(SelectionMatrixScope.Cell, activate: false);

            Assert.Equal(new[] { (2, 1) }, Pairs(changed));
            Assert.Equal(new[] { (2, 1) }, Pairs(model.UnselectedCells()));
            Assert.Equal(new[] { (2, 1) }, Pairs(recorder.Single().Cells));
        }

        [Fact]
        public void Row_TouchesEveryColumnAtThePrimaryRow()
        {
            var model = Rectangular();

            var changed = Editor(model, 2, 1).Apply(SelectionMatrixScope.Row, activate: false);

            // The whole LEVEL across the grid: this is the 100-clicks-in-one PB-007 asked for.
            Assert.Equal(new[] { (0, 1), (1, 1), (2, 1), (3, 1) }, Pairs(changed));
            Assert.Equal(new[] { (0, 1), (1, 1), (2, 1), (3, 1) }, Pairs(model.UnselectedCells()));
        }

        [Fact]
        public void Column_TouchesEveryRowOfThePrimaryColumn()
        {
            var model = Rectangular();

            var changed = Editor(model, 2, 1).Apply(SelectionMatrixScope.Column, activate: false);

            Assert.Equal(new[] { (2, 0), (2, 1), (2, 2) }, Pairs(changed));
            Assert.Equal(new[] { (2, 0), (2, 1), (2, 2) }, Pairs(model.UnselectedCells()));
        }

        [Fact]
        public void All_TouchesEveryCell_AndNeedsNoPrimary()
        {
            var model = Rectangular();
            var editor = new SelectionMatrixBulkEditor(model); // deliberately NO primary cell

            Assert.True(editor.CanApply(SelectionMatrixScope.All));
            var changed = editor.Apply(SelectionMatrixScope.All, activate: false);

            Assert.Equal(12, changed.Count);
            Assert.Equal(12, model.UnselectedCount);
            Assert.Equal(0, model.SelectedCount);
        }

        [Fact]
        public void Activate_TurnsTheScopeBackOn()
        {
            var model = Rectangular();
            var editor = Editor(model, 1, 2);
            editor.Apply(SelectionMatrixScope.All, activate: false);

            var changed = editor.Apply(SelectionMatrixScope.Row, activate: true);

            Assert.Equal(new[] { (0, 2), (1, 2), (2, 2), (3, 2) }, Pairs(changed));
            Assert.Equal(4, model.SelectedCount);
        }

        // ---- Jagged grids: absent cells are never touched and never reported -------------------------------------

        [Fact]
        public void Row_SkipsAbsentCells()
        {
            var model = Jagged();
            var recorder = new Recorder(model);

            // Row 2 exists in column 0 only: column 1 is a blank front and column 2 is one level shorter.
            var changed = Editor(model, 0, 2).Apply(SelectionMatrixScope.Row, activate: false);

            Assert.Equal(new[] { (0, 2) }, Pairs(changed));
            Assert.Equal(new[] { (0, 2) }, Pairs(recorder.Single().Cells));
            Assert.True(model.IsAbsent(1, 2));
            Assert.True(model.IsAbsent(2, 2));
        }

        [Fact]
        public void All_SkipsAbsentCells_AndDoesNotResurrectABlankFrontColumn()
        {
            var model = Jagged();

            var changed = Editor(model).Apply(SelectionMatrixScope.All, activate: false);

            // 3 (column 0) + 0 (column 1, blank front) + 2 (column 2) = 5 present cells.
            Assert.Equal(5, changed.Count);
            Assert.Equal(5, model.CellCount);
            Assert.DoesNotContain(Pairs(changed), pair => pair.Column == 1);
            for (var row = 0; row < model.Rows; row++)
            {
                Assert.True(model.IsAbsent(1, row)); // still absent afterwards
            }
        }

        [Fact]
        public void Column_OverAnEntirelyAbsentColumn_ChangesNothingAndNotifiesNothing()
        {
            var model = Jagged();
            var editor = new SelectionMatrixBulkEditor(model);
            var recorder = new Recorder(model);

            // The primary can never land on the blank front's column, so the scope has no anchor there at all.
            Assert.False(editor.TrySetPrimary(1, 0));
            Assert.Null(editor.PrimaryCell);
            Assert.False(editor.CanApply(SelectionMatrixScope.Column));
            Assert.Empty(editor.Apply(SelectionMatrixScope.Column, activate: false));
            Assert.Empty(recorder.Events);
        }

        // ---- No-op, idempotence and the aggregated notification --------------------------------------------------

        [Fact]
        public void NoOp_ChangesNothingAndRaisesNoEvent()
        {
            var model = Rectangular(); // already all ON
            var recorder = new Recorder(model);

            var changed = Editor(model, 1, 1).Apply(SelectionMatrixScope.Row, activate: true);

            Assert.Empty(changed);
            Assert.Empty(recorder.Events);
            Assert.Equal(12, model.SelectedCount);
        }

        [Fact]
        public void Idempotent_RepeatingTheOperationChangesNothingAndNotifiesNothing()
        {
            var model = Rectangular();
            var editor = Editor(model, 2, 1);
            var recorder = new Recorder(model);

            var first = editor.Apply(SelectionMatrixScope.Column, activate: false);
            var second = editor.Apply(SelectionMatrixScope.Column, activate: false);

            Assert.Equal(3, first.Count);
            Assert.Empty(second);
            Assert.Single(recorder.Events); // the second application is silent
            Assert.Equal(3, model.UnselectedCount);
        }

        [Fact]
        public void PartialOverlap_ReportsOnlyTheCellsThatActuallyChanged()
        {
            var model = Rectangular();
            var editor = Editor(model, 0, 1);
            model.SetSelected(1, 1, false); // one cell of the row is already OFF
            var recorder = new Recorder(model);

            var changed = editor.Apply(SelectionMatrixScope.Row, activate: false);

            Assert.Equal(new[] { (0, 1), (2, 1), (3, 1) }, Pairs(changed));
            Assert.Equal(new[] { (0, 1), (2, 1), (3, 1) }, Pairs(recorder.Single().Cells));
        }

        [Fact]
        public void OneAggregatedNotification_NotOnePerCell()
        {
            var model = Rectangular();
            var recorder = new Recorder(model);

            Editor(model, 0, 0).Apply(SelectionMatrixScope.All, activate: false);

            var raised = recorder.Single();
            Assert.Equal(SelectionMatrixScope.All, raised.Scope);
            Assert.False(raised.IsSelected);
            Assert.Equal(12, raised.Cells.Count);
            Assert.Equal(0, recorder.CellChangedCount); // NOT one granular event per cell
            Assert.Equal(0, recorder.BulkChangedCount); // NOR the blunt repaint-everything signal of SetAll
        }

        [Fact]
        public void SetAll_KeepsItsOwnSignal_Unchanged()
        {
            var model = Rectangular();
            var recorder = new Recorder(model);

            model.SetAll(false); // the historical "Todos"/"Ninguno" path must not change

            Assert.Equal(1, recorder.BulkChangedCount);
            Assert.Empty(recorder.Events);
        }

        // ---- Tolerance: a stale primary must not throw ------------------------------------------------------------

        [Fact]
        public void PrimaryOutOfRange_IsRefused_AndLeavesThePreviousOneUntouched()
        {
            var model = Rectangular();
            var editor = Editor(model, 3, 2);

            Assert.False(editor.TrySetPrimary(9, 0));
            Assert.False(editor.TrySetPrimary(-1, 0));
            Assert.False(editor.TrySetPrimary(0, 9));

            Assert.Equal(new SelectionMatrixCell(3, 2), editor.PrimaryCell);
        }

        [Fact]
        public void EmptyGrid_OffersNoScopeAndChangesNothing()
        {
            var model = new SelectionMatrixModel(0, 0);
            var editor = new SelectionMatrixBulkEditor(model);
            var recorder = new Recorder(model);

            foreach (var scope in SelectionMatrixBulkEditor.AllScopes)
            {
                Assert.False(editor.CanApply(scope));
                Assert.Empty(editor.Apply(scope, activate: false));
            }

            Assert.Empty(recorder.Events);
        }
    }
}
