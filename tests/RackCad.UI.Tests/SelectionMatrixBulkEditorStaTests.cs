using System.Linq;
using System.Windows.Controls;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-34 / PB-007 — the minimal STA guards over the bulk-edit foundation and the real <see cref="SelectionMatrix"/>
    /// control: the primary cell, scope enablement, the tooltip that says WHY a scope is off, the captions the adopter
    /// DECLARES ("Frente" vs "Poste"), and the performance invariant that a bulk edit repaints only the cells that
    /// changed instead of rebuilding the grid (AGENTS §6).
    /// </summary>
    public sealed class SelectionMatrixBulkEditorStaTests
    {
        // ---- Primary cell ----------------------------------------------------------------------------------------

        [Fact]
        public void PrimaryCell_StartsUnset_MovesOnlyOntoPresentCells_AndClears()
        {
            StaTestRunner.Run(() =>
            {
                // Column 1 is a blank front (zero levels): every one of its cells is absent.
                var model = SelectionMatrixModel.WithJaggedColumns(new[] { 3, 0, 2 }, unselected: null);
                var editor = new SelectionMatrixBulkEditor(model);

                Assert.False(editor.HasPrimaryCell);
                Assert.Null(editor.PrimaryCell);

                Assert.True(editor.TrySetPrimary(0, 2));
                Assert.Equal(new SelectionMatrixCell(0, 2), editor.PrimaryCell);

                Assert.False(editor.TrySetPrimary(1, 0)); // absent: refused
                Assert.False(editor.TrySetPrimary(2, 2)); // absent: refused
                Assert.Equal(new SelectionMatrixCell(0, 2), editor.PrimaryCell); // and the anchor did not move

                editor.ClearPrimary();
                Assert.False(editor.HasPrimaryCell);
                Assert.Null(editor.PrimaryCell);
            });
        }

        /// <summary>The primary cell is a TRANSIENT anchor: setting it must not alter a single cell's on/off state,
        /// because what the safety grids persist is the OFF set and nothing else.</summary>
        [Fact]
        public void PrimaryCell_IsNotPersistedState_AndChangesNoCell()
        {
            StaTestRunner.Run(() =>
            {
                var model = new SelectionMatrixModel(3, 2);
                model.SetSelected(1, 1, false);
                var editor = new SelectionMatrixBulkEditor(model);

                Assert.True(editor.TrySetPrimary(2, 0));
                editor.ClearPrimary();
                Assert.True(editor.TrySetPrimary(0, 1));

                Assert.Equal(1, model.UnselectedCount);
                Assert.Equal(new[] { (1, 1) }, model.UnselectedCells().Select(c => (c.Column, c.Row)).ToArray());
            });
        }

        // ---- Enablement and the reason shown as a tooltip ---------------------------------------------------------

        [Fact]
        public void WithoutAPrimaryCell_OnlyAllIsEnabled_AndTheOthersSayWhy()
        {
            StaTestRunner.Run(() =>
            {
                var editor = new SelectionMatrixBulkEditor(new SelectionMatrixModel(3, 2));

                Assert.True(editor.CanApply(SelectionMatrixScope.All));
                Assert.Null(editor.DisabledReason(SelectionMatrixScope.All));

                foreach (var scope in new[]
                         {
                             SelectionMatrixScope.Cell, SelectionMatrixScope.Row, SelectionMatrixScope.Column
                         })
                {
                    Assert.False(editor.CanApply(scope));
                    var reason = editor.DisabledReason(scope);
                    Assert.False(string.IsNullOrWhiteSpace(reason)); // never disabled without an explanation
                    Assert.Contains("celda", reason, System.StringComparison.OrdinalIgnoreCase);
                }
            });
        }

        [Fact]
        public void WithAPrimaryCell_EveryDeclaredScopeIsEnabledWithNoReason()
        {
            StaTestRunner.Run(() =>
            {
                var editor = new SelectionMatrixBulkEditor(new SelectionMatrixModel(3, 2));
                Assert.True(editor.TrySetPrimary(1, 1));

                foreach (var scope in SelectionMatrixBulkEditor.AllScopes)
                {
                    Assert.True(editor.CanApply(scope));
                    Assert.Null(editor.DisabledReason(scope));
                }
            });
        }

        [Fact]
        public void AScopeTheDialogDidNotDeclare_IsDisabledWithItsOwnReason()
        {
            StaTestRunner.Run(() =>
            {
                var editor = new SelectionMatrixBulkEditor(
                    new SelectionMatrixModel(3, 2),
                    scopes: new[] { SelectionMatrixScope.Cell, SelectionMatrixScope.All });
                editor.TrySetPrimary(0, 0);

                Assert.Equal(
                    new[] { SelectionMatrixScope.Cell, SelectionMatrixScope.All },
                    editor.Scopes.ToArray());
                Assert.False(editor.Supports(SelectionMatrixScope.Row));
                Assert.False(editor.CanApply(SelectionMatrixScope.Row));
                Assert.False(string.IsNullOrWhiteSpace(editor.DisabledReason(SelectionMatrixScope.Row)));
                Assert.Empty(editor.Apply(SelectionMatrixScope.Row, activate: false)); // and it is inert
            });
        }

        [Fact]
        public void AGridWithNoPresentCells_DisablesEveryScope()
        {
            StaTestRunner.Run(() =>
            {
                // Two fronts, both EN BLANCO: the grid has rows, but not one selectable cell.
                var model = SelectionMatrixModel.WithJaggedColumns(new[] { 0, 0 }, unselected: null);
                var editor = new SelectionMatrixBulkEditor(model);

                Assert.Equal(0, model.CellCount);
                foreach (var scope in SelectionMatrixBulkEditor.AllScopes)
                {
                    Assert.False(editor.CanApply(scope));
                    Assert.False(string.IsNullOrWhiteSpace(editor.DisabledReason(scope)));
                }
            });
        }

        // ---- Labels DECLARED by the dialog, never derived from a system -------------------------------------------

        [Fact]
        public void Labels_NameTheColumnAxisTheWayTheDialogDeclaresIt()
        {
            StaTestRunner.Run(() =>
            {
                var porFrente = new SelectionMatrixBulkEditor(
                    new SelectionMatrixModel(2, 2), SelectionMatrixScopeLabels.ByFrente);
                var porPoste = new SelectionMatrixBulkEditor(
                    new SelectionMatrixModel(2, 2), SelectionMatrixScopeLabels.ByPoste);

                Assert.Equal("Frente", porFrente.LabelFor(SelectionMatrixScope.Column));
                Assert.Equal("Poste", porPoste.LabelFor(SelectionMatrixScope.Column)); // the desviador grid

                foreach (var editor in new[] { porFrente, porPoste })
                {
                    Assert.Equal("Celda", editor.LabelFor(SelectionMatrixScope.Cell));
                    Assert.Equal("Nivel", editor.LabelFor(SelectionMatrixScope.Row));
                    Assert.Equal("Todo", editor.LabelFor(SelectionMatrixScope.All));
                }
            });
        }

        [Fact]
        public void Labels_DefaultToTheNeutralSet_AndAcceptACustomAxis()
        {
            StaTestRunner.Run(() =>
            {
                var byDefault = new SelectionMatrixBulkEditor(new SelectionMatrixModel(2, 2));
                Assert.Equal("Frente", byDefault.LabelFor(SelectionMatrixScope.Column));

                var custom = new SelectionMatrixBulkEditor(
                    new SelectionMatrixModel(2, 2), new SelectionMatrixScopeLabels("Tramo", "Fondo", "Todas"));
                Assert.Equal("Tramo", custom.LabelFor(SelectionMatrixScope.Column));
                Assert.Equal("Fondo", custom.LabelFor(SelectionMatrixScope.Row));
                Assert.Equal("Todas", custom.LabelFor(SelectionMatrixScope.All));
            });
        }

        // ---- The control: repaint the changed cells, never rebuild ------------------------------------------------

        [Fact]
        public void BulkEdit_UpdatesOnlyTheChangedCheckBoxes_WithoutRebuildingTheGrid()
        {
            StaTestRunner.Run(() =>
            {
                var model = new SelectionMatrixModel(3, 2);
                var matrix = new SelectionMatrix { Model = model };
                var before = matrix.Children.OfType<CheckBox>().ToArray();
                var editor = new SelectionMatrixBulkEditor(model);
                editor.TrySetPrimary(1, 1);

                editor.Apply(SelectionMatrixScope.Row, activate: false);

                // Same CheckBox instances, in the same order: the grid was repainted, not rebuilt.
                Assert.Equal(before, matrix.Children.OfType<CheckBox>().ToArray());

                for (var column = 0; column < model.Columns; column++)
                {
                    Assert.False(matrix.CellFor(column, 1).IsChecked); // the applied row followed the model
                    Assert.True(matrix.CellFor(column, 0).IsChecked);  // and the untouched row did not move
                }
            });
        }

        [Fact]
        public void BulkEdit_OverAJaggedGrid_LeavesAbsentCellsWithoutACheckBox()
        {
            StaTestRunner.Run(() =>
            {
                var model = SelectionMatrixModel.WithJaggedColumns(new[] { 2, 0 }, unselected: null);
                var matrix = new SelectionMatrix { Model = model };
                var editor = new SelectionMatrixBulkEditor(model);

                editor.Apply(SelectionMatrixScope.All, activate: false);

                Assert.Equal(2, matrix.Children.OfType<CheckBox>().Count()); // only column 0's two cells exist
                Assert.Null(matrix.CellFor(1, 0));
                Assert.Null(matrix.CellFor(1, 1));
                Assert.False(matrix.CellFor(0, 0).IsChecked);
                Assert.False(matrix.CellFor(0, 1).IsChecked);
            });
        }
    }
}
