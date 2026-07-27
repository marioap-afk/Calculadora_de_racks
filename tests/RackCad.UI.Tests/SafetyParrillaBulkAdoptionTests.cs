using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.UI;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-34, addendum del Owner (§0) — the PARRILLA of the Selectivo adopts the shared bulk edit. It is the one grid
    /// I-22 deliberately left out because each cell carries a LIVE DECK COUNT next to its check box, and the Owner's
    /// condition for including it is that the counter must survive: it may not be reduced to a bare boolean.
    /// <para>
    /// These regressions were verified RED against the dialog before the adoption existed.
    /// </para>
    /// </summary>
    public sealed class SafetyParrillaBulkAdoptionTests
    {
        private const string PostId = "POSTE-3x3-16";
        private const string BeamId = "LARG-3R-4";
        private const string ParrillaId = "PARRILLA-GEN";

        private static SelectiveGridCell Cell(int frente, int level)
            => new SelectiveGridCell { Frente = frente, Level = level };

        private static (int Frente, int Level)[] Pairs(IEnumerable<SelectiveGridCell> cells)
            => cells.Select(c => (c.Frente, c.Level)).OrderBy(p => p.Frente).ThenBy(p => p.Level).ToArray();

        // ---- A REAL resolved selectivo, so the live counter is the real rule the draw and the BOM use --------------

        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static SelectiveBayDesign Bay(int levels)
        {
            var bay = new SelectiveBayDesign { FloorBeam = true };
            for (var l = 0; l < levels; l++)
            {
                bay.Levels.Add(new SelectiveCell
                {
                    Pallet = new Tarima { Frente = 40.0, Alto = 45.0 + l * 5.0 },
                    PalletCount = 2,
                    BeamId = BeamId,
                    BeamPeralte = 4.0
                });
            }

            return bay;
        }

        /// <summary>A design whose bays have DIFFERENT level counts, so the grid is really jagged.</summary>
        private static SelectivePalletDesign Design(params int[] levelsPerBay)
        {
            var design = new SelectivePalletDesign
            {
                PostId = PostId, PostPeralte = 3.0, PalletTolerance = 4.0, VerticalClearance = 6.0, PalletDepth = 48.0
            };
            foreach (var levels in levelsPerBay)
            {
                design.Bays.Add(Bay(levels));
            }

            var selection = new SelectiveSafetySelection
            {
                ElementId = ParrillaId, Side = SafetySide.Both, Quantity = 1,
                ParrillaFrontal = true, ParrillaLateral = true
            };
            design.SafetySelections.Add(selection);
            return design;
        }

        private static SelectiveRackSystem Resolve(SelectivePalletDesign design)
            => new SelectiveGeometryResolver().Resolve(design, Catalog);

        /// <summary>Builds the REAL dialog exactly as <c>SelectiveSafetyWindow</c> does.</summary>
        private static SafetyParrillaGridWindow Parrilla(
            SelectivePalletDesign design, IEnumerable<SelectiveGridCell> offCells,
            double frente = 0.0, int cantidad = 0)
        {
            var system = Resolve(design);
            var levels = SelectiveSafetyGrid.LevelCounts(system);
            var plan = SelectiveParrillaPlan.Cells(system, Catalog);
            return new SafetyParrillaGridWindow(
                "Parrilla", levels, frontal: true, lateral: true, frente: frente, cantidad: cantidad,
                offCells: offCells, plan: plan);
        }

        private static SelectionMatrix MatrixOf(DependencyObject window)
            => Descendants(window).OfType<ScrollViewer>()
                .Select(scroll => scroll.Content as SelectionMatrix)
                .FirstOrDefault(matrix => matrix != null);

        private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            if (root is Window window)
            {
                root = window.Content as DependencyObject;
                if (root == null) yield break;
            }

            yield return root;
            if (root is Panel panel)
            {
                foreach (var found in panel.Children.OfType<DependencyObject>().SelectMany(Descendants))
                {
                    yield return found;
                }
            }
            else if (root is ContentControl holder && holder.Content is DependencyObject inner)
            {
                foreach (var found in Descendants(inner)) yield return found;
            }
            else if (root is Decorator decorator && decorator.Child != null)
            {
                foreach (var found in Descendants(decorator.Child)) yield return found;
            }
        }

        private static void ClickCell(SelectionMatrix matrix, int column, int row)
        {
            var checkbox = matrix.CellFor(column, row);
            Assert.NotNull(checkbox);
            checkbox.IsChecked = !(checkbox.IsChecked == true);
            checkbox.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        }

        // ---- Construction, captions and capabilities ---------------------------------------------------------------

        [Fact]
        public void TheRealDialog_CarriesTheSharedRow_WithTheFourScopes_AndTheFrenteAxis()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Parrilla(Design(3, 3), null);
                try
                {
                    Assert.NotNull(grid.BulkBar);
                    Assert.NotNull(grid.BulkEditor);
                    Assert.NotNull(grid.Model);
                    Assert.Contains(grid.BulkBar, Descendants(grid));

                    Assert.Equal(SelectionMatrixBulkEditor.AllScopes, grid.BulkEditor.Scopes);
                    Assert.Equal("Celda", grid.BulkBar.ButtonFor(SelectionMatrixScope.Cell).Content);
                    Assert.Equal("Nivel", grid.BulkBar.ButtonFor(SelectionMatrixScope.Row).Content);
                    Assert.Equal("Frente", grid.BulkBar.ButtonFor(SelectionMatrixScope.Column).Content);
                    Assert.Equal("Todo", grid.BulkBar.ButtonFor(SelectionMatrixScope.All).Content);

                    // The Owner confirmed Desactivar as the initial state; the parrilla must not diverge.
                    Assert.True(grid.BulkBar.DeactivateOption.IsChecked);
                    Assert.False(grid.BulkBar.Activates);
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void ClickingACell_MakesItThePrimaryOne_ThroughARealInteraction()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Parrilla(Design(3, 3, 3), null);
                try
                {
                    Assert.Null(grid.BulkEditor.PrimaryCell);
                    ClickCell(MatrixOf(grid), 2, 1);

                    Assert.Equal(new SelectionMatrixCell(2, 1), grid.BulkEditor.PrimaryCell);
                    Assert.Equal("Celda: Frente 3 · Nivel 2.", grid.BulkBar.PrimaryStatus.Text);

                    ClickCell(MatrixOf(grid), 0, 0); // the LAST interaction wins
                    Assert.Equal(new SelectionMatrixCell(0, 0), grid.BulkEditor.PrimaryCell);
                }
                finally { grid.Close(); }
            });
        }

        // ---- The four scopes, Activar and Desactivar, and the exact OffCells ----------------------------------------

        [Fact]
        public void Nivel_TurnsOffThatLevelInEveryFrente_AndTheResultCarriesExactlyThoseCells()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Parrilla(Design(3, 3, 3), null);
                try
                {
                    ClickCell(MatrixOf(grid), 1, 1);                 // anchors AND turns (1,1) off
                    grid.BulkBar.Apply(SelectionMatrixScope.Row);

                    Assert.Equal(new[] { (0, 1), (1, 1), (2, 1) }, Pairs(grid.BuildResultForTest().OffCells));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Frente_TurnsOffThatWholeColumn()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Parrilla(Design(3, 3, 3), null);
                try
                {
                    ClickCell(MatrixOf(grid), 2, 0);
                    grid.BulkBar.Apply(SelectionMatrixScope.Column);

                    Assert.Equal(new[] { (2, 0), (2, 1), (2, 2) }, Pairs(grid.BuildResultForTest().OffCells));
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Celda_TouchesOnlyThePrimary_AndActivarPutsItBack()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Parrilla(Design(2, 2), null);
                try
                {
                    ClickCell(MatrixOf(grid), 1, 1);
                    grid.BulkBar.Apply(SelectionMatrixScope.Cell);            // no-op: the click already turned it off
                    Assert.Equal(new[] { (1, 1) }, Pairs(grid.BuildResultForTest().OffCells));

                    grid.BulkBar.ActivateOption.IsChecked = true;
                    grid.BulkBar.Apply(SelectionMatrixScope.Cell);
                    Assert.Empty(grid.BuildResultForTest().OffCells);
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void Todo_SwitchesEveryCell_AndNeedsNoPrimary()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Parrilla(Design(2, 2), null);
                try
                {
                    Assert.Null(grid.BulkEditor.PrimaryCell);
                    grid.BulkBar.Apply(SelectionMatrixScope.All);
                    Assert.Equal(4, grid.BuildResultForTest().OffCells.Count);

                    grid.BulkBar.ActivateOption.IsChecked = true;
                    grid.BulkBar.Apply(SelectionMatrixScope.All);
                    Assert.Empty(grid.BuildResultForTest().OffCells);
                }
                finally { grid.Close(); }
            });
        }

        // ---- Jagged grid and absent cells ---------------------------------------------------------------------------

        [Fact]
        public void AJaggedGrid_HasAbsentCells_ThatNoScopeTouchesOrReports()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Parrilla(Design(3, 1), null); // frente 2 reaches only level 0
                try
                {
                    Assert.True(grid.Model.IsAbsent(1, 1));
                    Assert.True(grid.Model.IsAbsent(1, 2));
                    Assert.Null(MatrixOf(grid).CellFor(1, 2));            // no check box at all
                    Assert.False(grid.BulkEditor.TrySetPrimary(1, 2));    // cannot be the anchor

                    ClickCell(MatrixOf(grid), 0, 2);
                    grid.BulkBar.Apply(SelectionMatrixScope.Row);          // level 2 exists only in frente 1
                    Assert.Equal(new[] { (0, 2) }, Pairs(grid.BuildResultForTest().OffCells));

                    grid.BulkBar.Apply(SelectionMatrixScope.All);
                    Assert.Equal(4, grid.Model.CellCount);                 // 3 + 1
                    Assert.Equal(4, grid.BuildResultForTest().OffCells.Count);
                }
                finally { grid.Close(); }
            });
        }

        // ---- THE OWNER'S CONDITION: the live per-cell counter survives ------------------------------------------------

        [Fact]
        public void TheLiveCounter_IsShownPerCell_BeforeAndAfterABulkOperation()
        {
            StaTestRunner.Run(() =>
            {
                var design = Design(2, 2);
                var system = Resolve(design);
                var plan = SelectiveParrillaPlan.Cells(system, Catalog);
                var expected = plan.ToDictionary(
                    cell => (cell.Frente, cell.Level),
                    cell => SelectiveParrillaPlan.CountIn(cell, 0.0, 0));
                Assert.Contains(expected, pair => pair.Value > 0); // the scenario really draws decks

                var grid = Parrilla(design, null);
                try
                {
                    // BEFORE: every ON cell shows the count the shared rule gives — the same one the draw and BOM use.
                    foreach (var pair in expected)
                    {
                        Assert.Equal(
                            pair.Value.ToString(System.Globalization.CultureInfo.CurrentCulture),
                            grid.CountTextFor(pair.Key.Frente, pair.Key.Level));
                    }

                    // AFTER a bulk DESACTIVAR: the switched-off cells stop counting...
                    ClickCell(MatrixOf(grid), 0, 0);
                    grid.BulkBar.Apply(SelectionMatrixScope.Row);
                    foreach (var pair in expected.Where(p => p.Key.Level == 0))
                    {
                        Assert.Equal(string.Empty, grid.CountTextFor(pair.Key.Frente, pair.Key.Level));
                    }

                    // ...and the untouched ones keep exactly their number.
                    foreach (var pair in expected.Where(p => p.Key.Level != 0))
                    {
                        Assert.Equal(
                            pair.Value.ToString(System.Globalization.CultureInfo.CurrentCulture),
                            grid.CountTextFor(pair.Key.Frente, pair.Key.Level));
                    }

                    // AFTER activating them again: the counters come back identical.
                    grid.BulkBar.ActivateOption.IsChecked = true;
                    grid.BulkBar.Apply(SelectionMatrixScope.All);
                    foreach (var pair in expected)
                    {
                        Assert.Equal(
                            pair.Value.ToString(System.Globalization.CultureInfo.CurrentCulture),
                            grid.CountTextFor(pair.Key.Frente, pair.Key.Level));
                    }
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void ABulkOperation_RecountsExactlyOnce_AndNotAtAllWhenNothingChanges()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Parrilla(Design(4, 4, 4), null);
                try
                {
                    ClickCell(MatrixOf(grid), 0, 0);
                    var before = grid.RecountCount;

                    grid.BulkBar.Apply(SelectionMatrixScope.All);  // 11 remaining cells change in one go
                    Assert.Equal(before + 1, grid.RecountCount);

                    var after = grid.RecountCount;
                    grid.BulkBar.Apply(SelectionMatrixScope.All);  // idempotent repeat
                    Assert.Equal(after, grid.RecountCount);
                }
                finally { grid.Close(); }
            });
        }

        [Fact]
        public void TheHistoricalTodasNinguna_StillRecountsOnce()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Parrilla(Design(3, 3), null);
                try
                {
                    var before = grid.RecountCount;
                    grid.Model.SetAll(false);
                    Assert.Equal(before + 1, grid.RecountCount);
                }
                finally { grid.Close(); }
            });
        }

        // ---- Visual instances and scroll survive a bulk operation ------------------------------------------------------

        [Fact]
        public void ABulkEdit_KeepsTheSameCheckBoxesAndCounters_TheScrollOffset_AndTheWindowSize()
        {
            StaTestRunner.Run(() =>
            {
                var grid = Parrilla(Design(10, 10, 10), null);
                try
                {
                    var matrix = MatrixOf(grid);
                    var scroll = (ScrollViewer)matrix.Parent;
                    var root = (FrameworkElement)grid.Content;
                    root.Measure(new Size(520, 220));
                    root.Arrange(new Rect(0, 0, 520, 220));
                    root.UpdateLayout();
                    scroll.ScrollToVerticalOffset(12.0);
                    root.UpdateLayout();

                    var offsetBefore = scroll.VerticalOffset;
                    Assert.True(offsetBefore > 0.0, "the grid must really be scrolled, or the assertion is vacuous");
                    var boxesBefore = Descendants(matrix).OfType<CheckBox>().ToArray();
                    var countersBefore = Enumerable.Range(0, grid.Model.Columns)
                        .SelectMany(c => Enumerable.Range(0, grid.Model.Rows).Select(r => matrix.AdornmentFor(c, r)))
                        .Where(block => block != null).ToArray();
                    var sizeBefore = (grid.Width, grid.Height);

                    ClickCell(matrix, 1, 1);
                    grid.BulkBar.Apply(SelectionMatrixScope.All);

                    Assert.Equal(boxesBefore, Descendants(matrix).OfType<CheckBox>().ToArray());
                    Assert.Equal(
                        countersBefore,
                        Enumerable.Range(0, grid.Model.Columns)
                            .SelectMany(c => Enumerable.Range(0, grid.Model.Rows).Select(r => matrix.AdornmentFor(c, r)))
                            .Where(block => block != null).ToArray());
                    Assert.Equal(offsetBefore, scroll.VerticalOffset);
                    Assert.Equal(sizeBefore, (grid.Width, grid.Height));
                }
                finally { grid.Close(); }
            });
        }

        // ---- Round trip, and the drawing/BOM the resulting pattern produces ---------------------------------------------

        [Fact]
        public void TheResultingOffCells_RoundTripThroughTheConfiguration()
        {
            StaTestRunner.Run(() =>
            {
                var design = Design(3, 3, 3);
                IReadOnlyList<SelectiveGridCell> produced;

                var first = Parrilla(design, null);
                try
                {
                    ClickCell(MatrixOf(first), 1, 2);
                    first.BulkBar.Apply(SelectionMatrixScope.Row);
                    produced = first.BuildResultForTest().OffCells;
                }
                finally { first.Close(); }

                // Reopening with what was persisted shows exactly those cells OFF and everything else ON.
                var reopened = Parrilla(design, produced);
                try
                {
                    Assert.Equal(Pairs(produced), Pairs(reopened.BuildResultForTest().OffCells));
                    Assert.Equal(new[] { (0, 2), (1, 2), (2, 2) }, Pairs(produced));
                    Assert.True(reopened.Model.IsSelected(0, 0));
                }
                finally { reopened.Close(); }
            });
        }

        /// <summary>
        /// The end that matters: a pattern produced by the BULK edit draws and quotes exactly like the same cells
        /// switched off ONE BY ONE. The bulk edit is a faster way to reach a state, never a different state.
        /// </summary>
        [Fact]
        public void TheDrawingAndTheBom_MatchTheSamePatternBuiltCellByCell()
        {
            StaTestRunner.Run(() =>
            {
                var shape = new[] { 3, 3, 3 };
                IReadOnlyList<SelectiveGridCell> fromBulk;

                var grid = Parrilla(Design(shape), null);
                try
                {
                    ClickCell(MatrixOf(grid), 1, 1);
                    grid.BulkBar.Apply(SelectionMatrixScope.Row);      // level 1 across the three frentes
                    fromBulk = grid.BuildResultForTest().OffCells;
                }
                finally { grid.Close(); }

                var byHand = new[] { Cell(0, 1), Cell(1, 1), Cell(2, 1) };
                Assert.Equal(Pairs(byHand), Pairs(fromBulk));

                Assert.Equal(Snapshot(shape, byHand), Snapshot(shape, fromBulk));
                Assert.NotEqual(Snapshot(shape, new SelectiveGridCell[0]), Snapshot(shape, fromBulk));
            });
        }

        /// <summary>
        /// The resolved DECK output for a given off-cell set: every parrilla instance of the frontal, the lateral
        /// cortes and the planta, plus the safety BOM. Built through the same chain the plugin uses, so "the drawing
        /// and the BOM" here really are the drawing and the BOM.
        /// </summary>
        private static string Snapshot(int[] levelsPerBay, IReadOnlyList<SelectiveGridCell> offCells)
        {
            var design = Design(levelsPerBay);
            var parrilla = design.SafetySelections.First(s => s.ElementId == ParrillaId);
            parrilla.ParrillaOffCells.Clear();
            foreach (var cell in offCells)
            {
                parrilla.ParrillaOffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
            }

            var catalog = Catalog;
            var system = new SelectiveGeometryResolver().Resolve(design, catalog);
            var lines = new List<string>();

            foreach (var instance in new SelectiveFrontalBuilder()
                         .Build(SelectiveDepthLayout.FondoSystemView(system, 0), catalog))
            {
                lines.Add(Key("FRONTAL", -1, instance));
            }

            var cortes = new SelectiveLateralBuilder().Cortes(system, catalog);
            for (var c = 0; c < cortes.Count; c++)
            {
                foreach (var instance in cortes[c].Largueros) lines.Add(Key("LATERAL", c, instance));
            }

            foreach (var instance in new SelectivePlantaBuilder().Build(system, catalog))
            {
                lines.Add(Key("PLANTA", -1, instance));
            }

            lines.Sort(System.StringComparer.Ordinal);

            var bom = SelectiveBomBuilder.Build(system, catalog).Components
                .Select(component => string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "BOM|{0}|{1}|{2}|{3:0.###}",
                    component.Category, component.ProfileId, component.Quantity, component.Length))
                .OrderBy(line => line, System.StringComparer.Ordinal);

            return string.Join("\n", lines.Concat(bom));
        }

        /// <summary>One canonical line per DECK instance: only the parrilla block, so the comparison is about the
        /// pattern this dialog produces and nothing else.</summary>
        private static string Key(string view, int corte, RackCad.Application.Headers.HeaderBlockInstance instance)
            => string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3}|{4:0.###}|{5:0.###}",
                view, corte, instance.Role, instance.BlockName, instance.Insertion.X, instance.Insertion.Y);
    }
}
