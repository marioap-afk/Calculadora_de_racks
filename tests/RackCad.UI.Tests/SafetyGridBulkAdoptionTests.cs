using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-34 / PB-007 — the ADOPTION of the shared bulk edit by the three grids that use
    /// <see cref="SelectionMatrix"/>, over the REAL dialogs (STA): the desviador (column axis = POSTE, opened by
    /// Selectivo, Dinámico and Push Back), the tope (column axis = FRENTE, serving BOTH the Selectivo tope and Push
    /// Back's rear tope) and the entrance guide (FRENTE, Dinámico).
    /// <para>
    /// What they lock: the row is really there and wired; a click makes that cell the primary one; Activar/Desactivar
    /// by cell, level, column and all; the captions each dialog DECLARES; the tooltips; jagged grids and partially
    /// absent rows/columns; the desviador's live note recomputed exactly ONCE per bulk operation; the resulting
    /// OffCells; blank fronts and their dormant configuration untouched; and that a bulk edit keeps the same visual
    /// instances, the scroll offset and the window size.
    /// </para>
    /// </summary>
    public sealed class SafetyGridBulkAdoptionTests
    {
        private static SelectiveGridCell Cell(int frente, int level)
            => new SelectiveGridCell { Frente = frente, Level = level };

        private static (int Frente, int Level)[] Pairs(IEnumerable<SelectiveGridCell> cells)
            => cells.Select(cell => (cell.Frente, cell.Level)).OrderBy(p => p.Frente).ThenBy(p => p.Level).ToArray();

        // ---- Real dialog builders (the same shapes the three systems hand over) -----------------------------------

        /// <summary>The desviador grid. `levelsPerPost` is what the authority computes for Dinámico and Push Back and
        /// what the Selectivo's plan yields; `allowBlank` is the I-33 opt-in the two dynamic systems pass.</summary>
        private static SafetyDesviadorGridWindow Desviador(
            IReadOnlyList<int> levelsPerPost, IEnumerable<SelectiveGridCell> offCells,
            bool allowBlank = true, bool showSide = true)
            => new SafetyDesviadorGridWindow(
                "DESVIADOR", "Desviador", system: null, catalog: null,
                longitud: 12.0, firstHeight: 12.0, side: SafetySide.Both,
                offCells: offCells,
                fallbackPostCount: levelsPerPost.Count,
                fallbackLevelsPerFrente: levelsPerPost,
                fallbackLevelsArePerPost: true,
                showSide: showSide,
                allowBlankColumns: allowBlank);

        private static SafetyTopeGridWindow Tope(
            IReadOnlyList<int> levelsPerFrente, IEnumerable<SelectiveGridCell> offCells,
            bool showSharedAndSide = true, double saque = 4.0)
            => new SafetyTopeGridWindow(
                "Larguero tope", levelsPerFrente, shared: true, side: SafetySide.Both, saque: saque, frontal: false,
                offCells: offCells, fondoCount: 1, fondo: -1, showSharedAndSide: showSharedAndSide);

        private static SafetyGuiaEntradaGridWindow Guia(
            IReadOnlyList<int> levelsPerFront, IEnumerable<SelectiveGridCell> offCells, bool allowBlank = true)
            => new SafetyGuiaEntradaGridWindow("Guía de entrada", levelsPerFront, offCells, allowBlank);

        private static SelectionMatrix MatrixOf(DependencyObject window)
        {
            foreach (var scroll in Descendants(window).OfType<ScrollViewer>())
            {
                if (scroll.Content is SelectionMatrix matrix)
                {
                    return matrix;
                }
            }

            return null;
        }

        private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            if (root is Window window && window.Content is DependencyObject content)
            {
                foreach (var found in Descendants(content))
                {
                    yield return found;
                }

                yield break;
            }

            yield return root;
            if (root is Panel panel)
            {
                foreach (var child in panel.Children.OfType<DependencyObject>())
                {
                    foreach (var found in Descendants(child))
                    {
                        yield return found;
                    }
                }
            }
            else if (root is ContentControl holder && holder.Content is DependencyObject inner)
            {
                foreach (var found in Descendants(inner))
                {
                    yield return found;
                }
            }
            else if (root is Decorator decorator && decorator.Child != null)
            {
                foreach (var found in Descendants(decorator.Child))
                {
                    yield return found;
                }
            }
        }

        /// <summary>Simulates the user clicking a cell: the check box flips and raises Click, exactly as WPF does.</summary>
        private static void ClickCell(SelectionMatrix matrix, int column, int row)
        {
            var checkbox = matrix.CellFor(column, row);
            Assert.NotNull(checkbox);
            checkbox.IsChecked = !(checkbox.IsChecked == true);
            checkbox.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        }

        // ---- The row exists and is wired in the three dialogs -----------------------------------------------------

        [Fact]
        public void EveryAdoptedDialog_CarriesTheSharedRow_WithTheFourScopes()
        {
            StaTestRunner.Run(() =>
            {
                var desviador = Desviador(new[] { 3, 3, 2 }, null);
                var tope = Tope(new[] { 3, 2 }, null);
                var guia = Guia(new[] { 3, 2 }, null);
                try
                {
                    foreach (var bar in new[] { desviador.BulkBar, tope.BulkBar, guia.BulkBar })
                    {
                        Assert.NotNull(bar);
                        Assert.Equal(SelectionMatrixBulkEditor.AllScopes, bar.Editor.Scopes);
                        foreach (var scope in SelectionMatrixBulkEditor.AllScopes)
                        {
                            Assert.NotNull(bar.ButtonFor(scope));
                        }

                        Assert.NotNull(bar.ActivateOption);
                        Assert.NotNull(bar.DeactivateOption);
                        Assert.True(bar.DeactivateOption.IsChecked); // subtractive by default (the PB-007 case)
                        Assert.False(bar.Activates);
                    }
                }
                finally
                {
                    desviador.Close();
                    tope.Close();
                    guia.Close();
                }
            });
        }

        /// <summary>The row is really in the window's tree, not just held in a field.</summary>
        [Fact]
        public void TheSharedRow_IsPartOfEachDialogsVisualContent()
        {
            StaTestRunner.Run(() =>
            {
                var desviador = Desviador(new[] { 2, 2 }, null);
                var tope = Tope(new[] { 2, 2 }, null);
                var guia = Guia(new[] { 2, 2 }, null);
                try
                {
                    Assert.Contains(desviador.BulkBar, Descendants(desviador));
                    Assert.Contains(tope.BulkBar, Descendants(tope));
                    Assert.Contains(guia.BulkBar, Descendants(guia));
                }
                finally
                {
                    desviador.Close();
                    tope.Close();
                    guia.Close();
                }
            });
        }

        // ---- Declared captions: FRENTE vs POSTE -------------------------------------------------------------------

        [Fact]
        public void TheDesviador_NamesItsColumnAxisPoste_AndTheOtherTwoFrente()
        {
            StaTestRunner.Run(() =>
            {
                var desviador = Desviador(new[] { 2, 2 }, null);
                var tope = Tope(new[] { 2, 2 }, null);
                var guia = Guia(new[] { 2, 2 }, null);
                try
                {
                    Assert.Equal("Poste", desviador.BulkEditor.Labels.ColumnAxis);
                    Assert.Equal("Poste", desviador.BulkBar.ButtonFor(SelectionMatrixScope.Column).Content);

                    foreach (var window in new (SelectionMatrixBulkBar Bar, SelectionMatrixBulkEditor Editor)[]
                             {
                                 (tope.BulkBar, tope.BulkEditor), (guia.BulkBar, guia.BulkEditor)
                             })
                    {
                        Assert.Equal("Frente", window.Editor.Labels.ColumnAxis);
                        Assert.Equal("Frente", window.Bar.ButtonFor(SelectionMatrixScope.Column).Content);
                    }

                    // The other three captions are shared by all of them.
                    foreach (var bar in new[] { desviador.BulkBar, tope.BulkBar, guia.BulkBar })
                    {
                        Assert.Equal("Celda", bar.ButtonFor(SelectionMatrixScope.Cell).Content);
                        Assert.Equal("Nivel", bar.ButtonFor(SelectionMatrixScope.Row).Content);
                        Assert.Equal("Todo", bar.ButtonFor(SelectionMatrixScope.All).Content);
                    }
                }
                finally
                {
                    desviador.Close();
                    tope.Close();
                    guia.Close();
                }
            });
        }

        // ---- Tooltips and enablement ------------------------------------------------------------------------------

        [Fact]
        public void WithoutAPrimaryCell_OnlyTodoIsEnabled_AndTheRestExplainWhyInTheirTooltip()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Desviador(new[] { 3, 3 }, null);
                try
                {
                    var bar = grid.BulkBar;
                    Assert.True(bar.ButtonFor(SelectionMatrixScope.All).IsEnabled);

                    foreach (var scope in new[]
                             {
                                 SelectionMatrixScope.Cell, SelectionMatrixScope.Row, SelectionMatrixScope.Column
                             })
                    {
                        var button = bar.ButtonFor(scope);
                        Assert.False(button.IsEnabled);
                        Assert.Equal(grid.BulkEditor.DisabledReason(scope), button.ToolTip);
                        Assert.Contains("celda", (string)button.ToolTip, System.StringComparison.OrdinalIgnoreCase);
                    }

                    Assert.Equal("Ninguna celda seleccionada.", bar.PrimaryStatus.Text);
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void AnEnabledScope_ExplainsWhatItWillTouch_InTheDialogsOwnWords()
        {
            StaTestRunner.Run(() =>
            {
                var desviador = Desviador(new[] { 3, 3 }, null);
                var guia = Guia(new[] { 3, 3 }, null);
                try
                {
                    ClickCell(MatrixOf(desviador), 1, 1);
                    ClickCell(MatrixOf(guia), 1, 1);

                    // The desviador speaks of POSTES, the guide of FRENTES: same control, declared vocabulary.
                    var desviadorColumn = (string)desviador.BulkBar.ButtonFor(SelectionMatrixScope.Column).ToolTip;
                    Assert.Contains("poste", desviadorColumn, System.StringComparison.OrdinalIgnoreCase);
                    var desviadorRow = (string)desviador.BulkBar.ButtonFor(SelectionMatrixScope.Row).ToolTip;
                    Assert.Contains("postes", desviadorRow, System.StringComparison.OrdinalIgnoreCase);

                    var guiaColumn = (string)guia.BulkBar.ButtonFor(SelectionMatrixScope.Column).ToolTip;
                    Assert.Contains("frente", guiaColumn, System.StringComparison.OrdinalIgnoreCase);

                    // And it follows the Activar/Desactivar state.
                    Assert.StartsWith("Desactiva", desviadorColumn);
                    desviador.BulkBar.ActivateOption.IsChecked = true;
                    Assert.StartsWith(
                        "Activa", (string)desviador.BulkBar.ButtonFor(SelectionMatrixScope.Column).ToolTip);
                }
                finally
                {
                    desviador.Close();
                    guia.Close();
                }
            });
        }

        // ---- The primary cell is the last valid cell the user interacted with -------------------------------------

        [Fact]
        public void ClickingACell_MakesItThePrimaryOne_AndEnablesTheAnchoredScopes()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Guia(new[] { 3, 3, 3 }, null);
                try
                {
                    var matrix = MatrixOf(grid);
                    Assert.Null(grid.BulkEditor.PrimaryCell);

                    ClickCell(matrix, 2, 1);
                    Assert.Equal(new SelectionMatrixCell(2, 1), grid.BulkEditor.PrimaryCell);
                    Assert.Equal("Celda: Frente 3 · Nivel 2.", grid.BulkBar.PrimaryStatus.Text);

                    foreach (var scope in SelectionMatrixBulkEditor.AllScopes)
                    {
                        Assert.True(grid.BulkBar.ButtonFor(scope).IsEnabled);
                    }

                    // The LAST interaction wins.
                    ClickCell(matrix, 0, 2);
                    Assert.Equal(new SelectionMatrixCell(0, 2), grid.BulkEditor.PrimaryCell);
                }
                finally { grid.Close(); }
            });
        }

        // ---- Activar / Desactivar by cell, level, column and all --------------------------------------------------

        [Fact]
        public void Desactivar_ByLevel_TurnsOffThatLevelInEveryPost_AndOffCellsFollow()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Desviador(new[] { 3, 3, 3 }, null);
                try
                {
                    var matrix = MatrixOf(grid);
                    ClickCell(matrix, 1, 1);                       // primary AND this click already turned (1,1) off
                    grid.BulkBar.Apply(SelectionMatrixScope.Row);  // Desactivar is the default state

                    Assert.Equal(new[] { (0, 1), (1, 1), (2, 1) }, Pairs(grid.CurrentOffCells()));
                    Assert.Equal(new[] { (0, 1), (1, 1), (2, 1) }, Pairs(grid.PersistedOffCells()));
                    Assert.Equal(new[] { (0, 1), (1, 1), (2, 1) }, Pairs(grid.BuildResult().OffCells));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Desactivar_ByColumn_TurnsOffThatWholePost()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Desviador(new[] { 3, 3, 3 }, null);
                try
                {
                    ClickCell(MatrixOf(grid), 2, 0);
                    grid.BulkBar.Apply(SelectionMatrixScope.Column);

                    Assert.Equal(new[] { (2, 0), (2, 1), (2, 2) }, Pairs(grid.CurrentOffCells()));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Desactivar_ByCell_TouchesOnlyThatOne_AndActivarPutsItBack()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Tope(new[] { 3, 3 }, null);
                try
                {
                    var matrix = MatrixOf(grid);
                    ClickCell(matrix, 0, 0);                    // turns (0,0) off and anchors there
                    ClickCell(matrix, 1, 2);                    // ...then moves the anchor, turning (1,2) off too
                    grid.BulkBar.Apply(SelectionMatrixScope.Cell);
                    // The tope dialog reports through BuildResult (it has no CurrentOffCells seam of its own).
                    Assert.Equal(new[] { (0, 0), (1, 2) }, Pairs(grid.BuildResult().OffCells)); // no-op: already off

                    grid.BulkBar.ActivateOption.IsChecked = true;
                    grid.BulkBar.Apply(SelectionMatrixScope.Cell);
                    Assert.Equal(new[] { (0, 0) }, Pairs(grid.BuildResult().OffCells));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Todo_SwitchesTheWholeGrid_AndNeedsNoPrimaryCell()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Guia(new[] { 3, 3 }, null);
                try
                {
                    Assert.Null(grid.BulkEditor.PrimaryCell);
                    grid.BulkBar.Apply(SelectionMatrixScope.All);
                    Assert.Equal(6, grid.CurrentOffCells().Count);

                    grid.BulkBar.ActivateOption.IsChecked = true;
                    grid.BulkBar.Apply(SelectionMatrixScope.All);
                    Assert.Empty(grid.CurrentOffCells());
                }
                finally { grid.Close(); }
            });
        }

        // ---- Jagged grids and partially absent rows/columns --------------------------------------------------------

        [Fact]
        public void ByLevel_OverAJaggedGrid_SkipsThePostsThatDoNotReachThatLevel()
        {
            StaTestRunner.Run(() =>
            {
                // Level 2 exists only in post 0; post 1 has 2 levels and post 2 has 1.
                var grid = Desviador(new[] { 3, 2, 1 }, null);
                try
                {
                    ClickCell(MatrixOf(grid), 0, 2);
                    grid.BulkBar.Apply(SelectionMatrixScope.Row);

                    Assert.Equal(new[] { (0, 2) }, Pairs(grid.CurrentOffCells()));
                    Assert.True(grid.Model.IsAbsent(1, 2));
                    Assert.True(grid.Model.IsAbsent(2, 2));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Todo_OverAJaggedGrid_TouchesOnlyThePresentCells()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Tope(new[] { 3, 2, 1 }, null);
                try
                {
                    grid.BulkBar.Apply(SelectionMatrixScope.All);

                    Assert.Equal(6, grid.Model.CellCount); // 3 + 2 + 1
                    Assert.Equal(
                        new[] { (0, 0), (0, 1), (0, 2), (1, 0), (1, 1), (2, 0) },
                        Pairs(grid.BuildResult().OffCells));
                }
                finally { grid.Close(); }
            });
        }

        // ---- Blank fronts (I-33): absent columns and DORMANT configuration ------------------------------------------

        [Fact]
        public void ABlankFrontsColumn_CannotBeMadePrimary_AndNoScopeTouchesIt()
        {
            StaTestRunner.Run(() =>
            {
                var stored = new[] { Cell(1, 0), Cell(1, 2) };            // dormant cells of the blank column
                var grid = Desviador(new[] { 3, 0, 3 }, stored);
                try
                {
                    var matrix = MatrixOf(grid);
                    Assert.Null(matrix.CellFor(1, 0));                    // no check box at all: cannot be clicked
                    Assert.False(grid.BulkEditor.TrySetPrimary(1, 0));
                    Assert.Null(grid.BulkEditor.PrimaryCell);

                    ClickCell(matrix, 0, 0);
                    grid.BulkBar.Apply(SelectionMatrixScope.Row);          // level 0 across every PRESENT post
                    grid.BulkBar.Apply(SelectionMatrixScope.All);

                    // The blank column is never in the live view...
                    Assert.DoesNotContain(Pairs(grid.CurrentOffCells()), pair => pair.Frente == 1);
                    for (var level = 0; level < 3; level++)
                    {
                        Assert.True(grid.Model.IsAbsent(1, level));       // and stays absent
                    }

                    // ...and its DORMANT cells survive the bulk edits untouched, merged back on accept.
                    var persisted = Pairs(grid.PersistedOffCells());
                    Assert.Contains((1, 0), persisted);
                    Assert.Contains((1, 2), persisted);
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void AfterABulkEdit_ReactivatingTheFront_BringsTheDormantCellsBackIntact()
        {
            StaTestRunner.Run(() =>
            {
                var stored = new[] { Cell(1, 0), Cell(1, 2) };
                IReadOnlyList<SelectiveGridCell> persisted;

                var blank = Desviador(new[] { 3, 0, 3 }, stored);
                try
                {
                    blank.BulkBar.Apply(SelectionMatrixScope.All); // the widest possible mass edit while blank
                    persisted = blank.PersistedOffCells();
                }
                finally { blank.Close(); }

                var active = Desviador(new[] { 3, 3, 3 }, persisted);
                try
                {
                    // Exactly the two dormant cells come back, and they are editable again.
                    Assert.False(active.Model.IsAbsent(1, 0));
                    Assert.False(active.Model.IsAbsent(1, 2));
                    Assert.Contains((1, 0), Pairs(active.CurrentOffCells()));
                    Assert.Contains((1, 2), Pairs(active.CurrentOffCells()));
                }
                finally { active.Close(); }
            });
        }

        // ---- The desviador's live note: ONE recomputation per bulk operation ----------------------------------------

        [Fact]
        public void ABulkOperation_RecomputesTheLiveNote_ExactlyOnce()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Desviador(new[] { 4, 4, 4, 4 }, null);
                try
                {
                    ClickCell(MatrixOf(grid), 0, 0);
                    var before = grid.NoteRefreshCount;

                    grid.BulkBar.Apply(SelectionMatrixScope.All); // 15 remaining cells change in one go

                    Assert.Equal(before + 1, grid.NoteRefreshCount);
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void ABulkOperationThatChangesNothing_DoesNotRecomputeTheNoteAtAll()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Desviador(new[] { 3, 3 }, null);
                try
                {
                    grid.BulkBar.Apply(SelectionMatrixScope.All);   // everything OFF
                    var before = grid.NoteRefreshCount;

                    grid.BulkBar.Apply(SelectionMatrixScope.All);   // idempotent repeat

                    Assert.Equal(before, grid.NoteRefreshCount);
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void TheHistoricalTodosNinguno_StillRefreshesTheNoteOnce()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Desviador(new[] { 3, 3 }, null);
                try
                {
                    var before = grid.NoteRefreshCount;
                    grid.Model.SetAll(false); // the "Ninguno" button's path, unchanged by I-34
                    Assert.Equal(before + 1, grid.NoteRefreshCount);
                }
                finally { grid.Close(); }
            });
        }

        // ---- Push Back's REAR tope: the same dialog, its own persistence destination --------------------------------

        [Fact]
        public void PushBackRearTope_UsesTheSameSharedRow_AndItsBulkResultReachesTheConfig()
        {
            StaTestRunner.Run(() =>
            {
                var config = new PushBackRearTopeConfig { Saque = 6.0 };
                config.OffCells.Add(Cell(0, 0));

                // Exactly how RackPushBackSystemWindow opens it (PB-006: no shared/side controls).
                var grid = Tope(
                    PushBackRearTopeDialogAdapter.LevelsPerFrente(new[] { 3, 3 }, allowBlankFronts: true),
                    PushBackRearTopeDialogAdapter.OffCells(config),
                    showSharedAndSide: false,
                    saque: PushBackRearTopeDialogAdapter.Saque(config));
                try
                {
                    Assert.NotNull(grid.BulkBar);
                    Assert.Equal("Frente", grid.BulkEditor.Labels.ColumnAxis);

                    ClickCell(MatrixOf(grid), 1, 2);                    // anchor on frente 2, level 3
                    grid.BulkBar.Apply(SelectionMatrixScope.Column);    // the whole frente off

                    var result = grid.BuildResult();
                    Assert.NotNull(result);
                    PushBackRearTopeDialogAdapter.Apply(result, config);

                    Assert.Equal(
                        new[] { (0, 0), (1, 0), (1, 1), (1, 2) },
                        Pairs(config.OffCells));
                    Assert.Equal(6.0, config.Saque); // SAQUE untouched by the bulk edit
                    Assert.False(config.At(1, 1));   // and the domain predicate agrees
                    Assert.True(config.At(0, 1));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void PushBackRearTope_WithABlankFront_KeepsThatFrentesCellsDormantAcrossABulkEdit()
        {
            StaTestRunner.Run(() =>
            {
                var config = new PushBackRearTopeConfig();
                config.OffCells.Add(Cell(1, 0)); // dormant: frente 1 is EN BLANCO

                var grid = Tope(
                    PushBackRearTopeDialogAdapter.LevelsPerFrente(new[] { 3, 0, 3 }, allowBlankFronts: true),
                    PushBackRearTopeDialogAdapter.OffCells(config),
                    showSharedAndSide: false);
                try
                {
                    grid.BulkBar.Apply(SelectionMatrixScope.All);
                    PushBackRearTopeDialogAdapter.Apply(grid.BuildResult(), config);
                }
                finally { grid.Close(); }

                // The blank frente's stored cell survived, and no phantom cell was invented for it.
                Assert.Contains((1, 0), Pairs(config.OffCells));
                Assert.Equal(1, Pairs(config.OffCells).Count(pair => pair.Frente == 1));
            });
        }

        // ---- The three systems open the desviador; the row must behave identically in all of them --------------------

        [Fact]
        public void TheDesviadorRow_BehavesIdentically_ForSelectivoDinamicoAndPushBack()
        {
            StaTestRunner.Run(() =>
            {
                // Selectivo never supplies a zero and keeps its side selector; Dinámico opts into blank columns and
                // keeps the selector; Push Back opts in and hides it (PB-003). None of that reaches the shared row.
                var selectivo = Desviador(new[] { 3, 3, 3 }, null, allowBlank: false, showSide: true);
                var dinamico = Desviador(new[] { 3, 3, 3 }, null, allowBlank: true, showSide: true);
                var pushBack = Desviador(new[] { 3, 3, 3 }, null, allowBlank: true, showSide: false);
                try
                {
                    foreach (var grid in new[] { selectivo, dinamico, pushBack })
                    {
                        Assert.Equal("Poste", grid.BulkEditor.Labels.ColumnAxis);
                        ClickCell(MatrixOf(grid), 1, 1);
                        grid.BulkBar.Apply(SelectionMatrixScope.Row);
                        Assert.Equal(new[] { (0, 1), (1, 1), (2, 1) }, Pairs(grid.CurrentOffCells()));
                    }
                }
                finally
                {
                    selectivo.Close();
                    dinamico.Close();
                    pushBack.Close();
                }
            });
        }

        // ---- Visual instances, scroll offset and window size are preserved -------------------------------------------

        [Fact]
        public void ABulkEdit_KeepsTheSameCheckBoxes_TheScrollOffset_AndTheWindowSize()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Desviador(new[] { 10, 10, 10, 10, 10 }, null);
                try
                {
                    var matrix = MatrixOf(grid);
                    var scroll = (ScrollViewer)matrix.Parent;

                    // Lay the CONTENT out (an unshown Window does not run a layout pass of its own) in a viewport far
                    // shorter than the 10-level grid, so the ScrollViewer really has somewhere to scroll.
                    var root = (FrameworkElement)grid.Content;
                    root.Measure(new Size(500, 220));
                    root.Arrange(new Rect(0, 0, 500, 220));
                    root.UpdateLayout();
                    scroll.ScrollToVerticalOffset(12.0);
                    root.UpdateLayout();

                    var offsetBefore = scroll.VerticalOffset;
                    Assert.True(offsetBefore > 0.0, "the grid must really be scrolled, or the assertion below is vacuous");
                    var boxesBefore = matrix.Children.OfType<CheckBox>().ToArray();
                    var sizeBefore = (grid.Width, grid.Height);

                    ClickCell(matrix, 2, 2);
                    grid.BulkBar.Apply(SelectionMatrixScope.All);

                    Assert.Equal(boxesBefore, matrix.Children.OfType<CheckBox>().ToArray()); // repainted, not rebuilt
                    Assert.Equal(offsetBefore, scroll.VerticalOffset);
                    Assert.Equal(sizeBefore, (grid.Width, grid.Height));
                    Assert.All(boxesBefore, box => Assert.False(box.IsChecked));
                }
                finally { grid.Close(); }
            });
        }

        // ---- The auxiliary controls and validations of each dialog are untouched --------------------------------------

        [Fact]
        public void TheAuxiliaryControlsAndValidations_StillBehaveAsBefore()
        {
            StaTestRunner.Run(() =>
            {
                // The desviador still refuses an invalid LONGITUD after a bulk edit...
                var desviador = Desviador(new[] { 3, 3 }, null);
                try
                {
                    desviador.BulkBar.Apply(SelectionMatrixScope.All);
                    Assert.NotNull(desviador.BuildResult()); // valid dimensions survive the bulk edit
                }
                finally { desviador.Close(); }

                // ...and the tope still reports its SAQUE and its other options unchanged.
                var tope = Tope(new[] { 2, 2 }, null);
                try
                {
                    tope.BulkBar.Apply(SelectionMatrixScope.All);
                    var result = tope.BuildResult();
                    Assert.Equal(4.0, result.Saque);
                    Assert.True(result.Shared);
                    Assert.Equal(SafetySide.Both, result.Side);
                    Assert.False(result.Frontal);
                }
                finally { tope.Close(); }
            });
        }
    }
}
