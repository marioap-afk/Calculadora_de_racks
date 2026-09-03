using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.Selective;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA tests for the REAL <see cref="RackSelectiveWindow"/> wiring of I-43 Gate 3: the multi-selection of the
    /// matrix, the "Aplicar en fondos" axis and the multi-fondo apply. Everything runs through the window's OWN
    /// handlers — a genuine mouse event on a matrix cell, the real "Seleccionadas"/"Todas" buttons, the real fondo
    /// combo — so what is checked is the editor, not a re-implementation of it.
    /// </summary>
    public sealed class SelectiveMultiFondoApplyTests
    {
        /// <summary>Open a window and take it to <paramref name="fondos"/> fondos through the REAL "Número de fondos"
        /// field (typing + leaving the box, exactly as a user does).</summary>
        private static RackSelectiveWindow OpenWith(int fondos)
        {
            var window = new RackSelectiveWindow(canInsertInAutoCad: false);
            if (fondos > 1)
            {
                EditorWindowTestSupport.SetText(window, "FondosBox", fondos.ToString());
                RaiseLostFocus(window, "FondosBox");
            }

            return window;
        }

        private static void RaiseLostFocus(Window window, string name)
        {
            var box = (TextBox)window.FindName(name);
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }

        /// <summary>Choose the target fondos through the REAL dropdown (I-43, gate 8A): "todos", or a comma/plus
        /// separated list of one-based fondo numbers.</summary>
        private static void SetTargetFondos(RackSelectiveWindow window, string text)
        {
            if (text.Equals("todos", System.StringComparison.OrdinalIgnoreCase))
            {
                SelectiveTargetsTestSupport.SetAllTargets(window);
                return;
            }

            var wanted = text.Split(new[] { ',', '+', ' ' }, System.StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(token =>
                {
                    var range = token.Split('-');
                    return range.Length == 2
                        ? Enumerable.Range(int.Parse(range[0]), int.Parse(range[1]) - int.Parse(range[0]) + 1)
                        : new[] { int.Parse(token) };
                })
                .ToArray();
            SelectiveTargetsTestSupport.SetTargets(window, wanted);
        }

        /// <summary>A real left-click on a matrix cell (the Border's own MouseLeftButtonUp, which is what the window
        /// subscribes to).</summary>
        private static void ClickCell(RackSelectiveWindow window, int bay, int level)
        {
            var border = window.MatrixCell(bay, level);
            Assert.NotNull(border);
            border.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(
                System.Windows.Input.Mouse.PrimaryDevice, 0, System.Windows.Input.MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonUpEvent,
                Source = border
            });
        }

        /// <summary>Type a Frente into the cell editor so an apply has something to write.</summary>
        private static void SetFrente(RackSelectiveWindow window, double frente)
            => EditorWindowTestSupport.SetText(window, "FrenteBox", frente.ToString(System.Globalization.CultureInfo.InvariantCulture));

        private static double FrenteAt(RackSelectiveWindow window, int fondo, int front, int level)
            => window.EditorState.CellAt(new SelectiveCellAddress(fondo, front, level)).Frente;

        // ---- The target-fondo axis, through the real controls ----

        [Fact]
        public void TargetFondos_WithASingleFondo_IsThatFondo_AndTheRowIsHidden()
        {
            var (targets, visibility) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1);
                var panel = (StackPanel)window.FindName("TargetFondosPanel");
                return (window.EditorState.TargetFondos.Fondos.ToArray(), panel.Visibility);
            });

            // With one fondo every possible choice — "Todos" (the default since the gate-8 correction), "Actual", or
            // that fondo explicitly — resolves to the same single target, so there is nothing to choose between.
            Assert.Equal(new[] { 0 }, targets);
            Assert.Equal(Visibility.Collapsed, visibility);
        }

        [Fact]
        public void ChangingTheFondoBeingEdited_FollowsTheTarget_WhenItWasOnlyThatFondo()
        {
            var targets = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                // "Actual" is chosen deliberately: since the gate-8 correction the editor OPENS on "Todos", so the
                // mode this test is about has to be the one in force for the question to mean anything.
                SelectiveTargetsTestSupport.SetCurrentTarget(window);
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 2; // the real handler runs
                return window.EditorState.TargetFondos.Fondos.ToArray();
            });

            Assert.Equal(new[] { 2 }, targets); // "Actual" keeps aiming at the fondo on screen
        }

        [Fact]
        public void ChangingTheFondoBeingEdited_PreservesAnExplicitMultiFondoChoice()
        {
            var targets = StaTestRunner.Run(() =>
            {
                var window = OpenWith(4);
                SetTargetFondos(window, "1+3");
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 1;
                return window.EditorState.TargetFondos.Fondos.ToArray();
            });

            Assert.Equal(new[] { 0, 2 }, targets);
        }

        [Fact]
        public void ReducingTheFondoCount_DropsTargetsThatNoLongerExist()
        {
            var targets = StaTestRunner.Run(() =>
            {
                var window = OpenWith(4);
                SetTargetFondos(window, "1+4");
                EditorWindowTestSupport.SetText(window, "FondosBox", "2");
                RaiseLostFocus(window, "FondosBox");
                return window.EditorState.TargetFondos.Fondos.ToArray();
            });

            Assert.Equal(new[] { 0 }, targets); // fondo 4 is gone; the set is never left empty
        }

        // ---- The multi-selection, painted ----

        [Fact]
        public void APlainClickOnACell_LeavesExactlyThatCellSelected()
        {
            var positions = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1);
                ClickCell(window, 1, 2);
                return window.EditorState.SelectedPositions().Select(p => (p.FrontIndex, p.LevelIndex)).ToArray();
            });

            Assert.Equal(new[] { (1, 2) }, positions);
        }

        [Fact]
        public void AMultiSelection_IsPainted_PrimaryAndIncludedDifferFromAnUnselectedCell()
        {
            var (primary, included, plain) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1);
                ClickCell(window, 0, 0);
                window.EditorState.SelectCell(1, 3, extend: true); // Ctrl+click cannot be forged; the gesture it drives can
                window.RefreshSelectionVisualsForTest();
                return (window.MatrixCell(1, 3).BorderBrush, window.MatrixCell(0, 0).BorderBrush, window.MatrixCell(1, 1).BorderBrush);
            });

            Assert.Same(RackSelectiveWindow.PrimarySelectionStroke, primary);   // the cell the editor is bound to
            Assert.Same(RackSelectiveWindow.MultiSelectionStroke, included);    // also selected, softer outline
            Assert.NotSame(RackSelectiveWindow.PrimarySelectionStroke, plain);
            Assert.NotSame(RackSelectiveWindow.MultiSelectionStroke, plain);
        }

        // ---- The apply itself ----

        [Fact]
        public void Todas_WithTargets1And3_WritesOnlyThoseFondos()
        {
            var frentes = StaTestRunner.Run(() =>
            {
                var window = OpenWith(4);
                SetTargetFondos(window, "2+4");
                SetFrente(window, 51.0);
                EditorWindowTestSupport.ClickByContent(window, "Todas");
                return new[]
                {
                    FrenteAt(window, 0, 0, 0), FrenteAt(window, 1, 0, 0),
                    FrenteAt(window, 2, 0, 0), FrenteAt(window, 3, 0, 0)
                };
            });

            Assert.Equal(new[] { 42.0, 51.0, 42.0, 51.0 }, frentes); // fondos 2 and 4 (one-based) only
        }

        [Fact]
        public void Seleccionadas_WritesTheMarkedPositionsInEveryTargetFondo_AndNothingElse()
        {
            var (marked, unmarked, otherFondo) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "2,3");
                ClickCell(window, 0, 0);
                window.EditorState.SelectCell(1, 2, extend: true);
                SetFrente(window, 37.0);
                EditorWindowTestSupport.ClickByContent(window, "Seleccionadas");
                return (
                    new[] { FrenteAt(window, 1, 0, 0), FrenteAt(window, 1, 1, 2), FrenteAt(window, 2, 0, 0), FrenteAt(window, 2, 1, 2) },
                    FrenteAt(window, 1, 1, 0),
                    FrenteAt(window, 0, 0, 0));
            });

            Assert.All(marked, frente => Assert.Equal(37.0, frente));
            Assert.Equal(42.0, unmarked);   // a cell that was not marked
            Assert.Equal(42.0, otherFondo); // the fondo on screen was not a target
        }

        [Fact]
        public void AMultiFondoApply_RecomputesExactlyOnce()
        {
            // The promise of Gate 3: the window resolves and writes the whole plan, then recomputes ONCE — not once
            // per fondo and not once per cell. Measured on the real pipeline counter, not asserted from the design.
            var (single, multi) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(4);

                SetTargetFondos(window, "1");
                SetFrente(window, 44.0);
                var before = window.RecomputeCount;
                EditorWindowTestSupport.ClickByContent(window, "Todas");
                var afterSingle = window.RecomputeCount - before;

                SetTargetFondos(window, "todos");
                SetFrente(window, 45.0);
                before = window.RecomputeCount;
                EditorWindowTestSupport.ClickByContent(window, "Todas");
                return (afterSingle, window.RecomputeCount - before);
            });

            Assert.Equal(1, single);
            Assert.Equal(1, multi); // four fondos, 36 cells, still ONE recompute
        }

        [Fact]
        public void ASingleFondoRack_BehavesExactlyAsBefore()
        {
            var (frentes, recomputes) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1);
                ClickCell(window, 1, 1);
                SetFrente(window, 63.0);
                var before = window.RecomputeCount;
                EditorWindowTestSupport.ClickByContent(window, "Celda");
                var state = window.EditorState;
                return (new[] { state.Bays[1][1].Frente, state.Bays[0][0].Frente }, window.RecomputeCount - before);
            });

            Assert.Equal(new[] { 63.0, 42.0 }, frentes);
            Assert.Equal(1, recomputes);
        }

        [Fact]
        public void NeitherTheSelectionNorTheTargets_ReachTheBuiltDesign()
        {
            // The design is the persistence boundary: a runtime choice that leaked into it would be written to the DWG.
            var (plain, chosen) = StaTestRunner.Run(() =>
            {
                var a = OpenWith(3);
                var b = OpenWith(3);
                SetTargetFondos(b, "1-3");
                b.EditorState.SelectCell(1, 1, extend: true);
                b.EditorState.SelectCell(0, 2, extend: true);
                return (Signature(a), Signature(b));
            });

            Assert.Equal(plain, chosen);
        }

        /// <summary>Every value the built design carries for its cells — what persistence would write.</summary>
        private static string Signature(RackSelectiveWindow window)
        {
            var design = window.BuildDesignForTest(out var error);
            Assert.Null(error);
            var parts = design.Bays
                .SelectMany(bay => bay.Levels)
                .Select(level => $"{level.Pallet.Frente}:{level.Pallet.Alto}:{level.PalletCount}")
                .ToList();
            foreach (var extra in design.ExtraFondoBays)
            {
                parts.AddRange(extra.SelectMany(bay => bay.Levels).Select(level => $"{level.Pallet.Frente}:{level.Pallet.Alto}:{level.PalletCount}"));
            }

            return string.Join("|", parts);
        }
    }
}
