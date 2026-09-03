using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.Selective;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA tests for the REAL <see cref="RackSelectiveWindow"/> after the gate-8A redesign: the FRENTE properties —
    /// "larguero a piso" and its elevation — are edited in the LEFT PANEL and applied with a reach (Frente /
    /// Seleccionados / Todos) over the single "Fondos destino" selector. There is no run-wide elevation any more and
    /// no per-frente control inside the matrix.
    /// </summary>
    public sealed class SelectiveFloorBeamRiseWindowTests
    {
        private static RackSelectiveWindow OpenWith(int fondos, int frentes = 2)
        {
            var window = new RackSelectiveWindow(canInsertInAutoCad: false);
            if (fondos > 1)
            {
                EditorWindowTestSupport.SetText(window, "FondosBox", fondos.ToString());
                RaiseLostFocus(window, "FondosBox");
            }

            if (frentes != 2)
            {
                SelectiveTargetsTestSupport.SetAllTargets(window); // grow every fondo, so topologies stay comparable
                EditorWindowTestSupport.SetText(window, "BayCountBox", frentes.ToString());
                RaiseLostFocus(window, "BayCountBox");
                SelectiveTargetsTestSupport.SetCurrentTarget(window);
            }

            return window;
        }

        private static void RaiseLostFocus(Window window, string name)
        {
            var box = (TextBox)window.FindName(name);
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }

        /// <summary>Edit the frente panel and apply it with the given reach — the whole gesture, as a user performs it.</summary>
        private static void ApplyFront(RackSelectiveWindow window, bool floorBeam, string rise, string scope)
        {
            ((CheckBox)window.FindName("FrontFloorBeamCheck")).IsChecked = floorBeam;
            ((TextBox)window.FindName("FrontRiseBox")).Text = rise;
            EditorWindowTestSupport.ClickByContent(window, scope);
        }

        private static double?[] RisesOf(RackSelectiveWindow window, int fondo)
        {
            var state = window.EditorState;
            var frentes = fondo == state.SelectedFondo ? state.Bays.Count : state.FondoMatrices[fondo].Bays.Count;
            return Enumerable.Range(0, frentes).Select(front => state.FloorBeamRiseOverrideAt(fondo, front)).ToArray();
        }

        private static bool[] FloorsOf(RackSelectiveWindow window, int fondo)
        {
            var state = window.EditorState;
            return (fondo == state.SelectedFondo ? state.FloorBeams : state.FondoMatrices[fondo].FloorBeams).ToArray();
        }

        // ---- The run-wide elevation is gone ----

        [Fact]
        public void ThereIsNoGlobalElevationControlAnyMore()
        {
            var (globalBox, panelBox) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1);
                return (window.FindName("FloorRiseBox"), window.FindName("FrontRiseBox"));
            });

            Assert.Null(globalBox);    // the global concept left the UI entirely
            Assert.NotNull(panelBox);  // the elevation is a property of the frente, edited in the left panel
        }

        [Fact]
        public void TheMatrixHeaderNoLongerCarriesPisoElevationOrHeight()
        {
            var texts = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1);
                var header = ((Grid)window.FindName("MatrixGrid")).Children.OfType<StackPanel>()
                    .First(panel => Grid.GetRow(panel) == 0 && Grid.GetColumn(panel) == 1);
                return (
                    header.Children.OfType<CheckBox>().Count()
                        + header.Children.OfType<StackPanel>().SelectMany(r => r.Children.OfType<CheckBox>()).Count(),
                    header.Children.OfType<StackPanel>().SelectMany(r => r.Children.OfType<TextBox>()).Count());
            });

            Assert.Equal(0, texts.Item1); // no "Piso" checkbox
            Assert.Equal(0, texts.Item2); // no "Elev." and no "Alto"
        }

        // ---- Front / Selected / All over the target fondos ----

        [Fact]
        public void Frente_WritesTheSelectedFrenteInEveryTargetFondo()
        {
            var (rises, floors, outside) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3, frentes: 3);
                SelectiveTargetsTestSupport.SetTargets(window, 1, 3);
                window.EditorState.SelectCell(1, 0, extend: false);
                ApplyFront(window, floorBeam: true, rise: "13", scope: "Este frente");
                return (RisesOf(window, 0), FloorsOf(window, 0), RisesOf(window, 1));
            });

            Assert.Equal(new double?[] { 4.0, 13.0, 4.0 }, rises);
            Assert.Equal(new[] { false, true, false }, floors);
            Assert.Equal(new double?[] { 4.0, 4.0, 4.0 }, outside); // fondo 2 was not a target
        }

        [Fact]
        public void Seleccionados_WritesTheFrentesTheCellSelectionTouches()
        {
            // A selection of (F0,N0) and (F2,N1) names the frentes {0, 2} — the levels are irrelevant to a frente
            // property, which is exactly what "Seleccionados" means here.
            var rises = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2, frentes: 3);
                SelectiveTargetsTestSupport.SetTargets(window, 1, 2);
                var state = window.EditorState;
                state.SelectCell(0, 0, extend: false);
                state.SelectCell(2, 1, extend: true);
                ApplyFront(window, floorBeam: true, rise: "16", scope: "Frentes seleccionados");
                return RisesOf(window, 0).Concat(RisesOf(window, 1)).ToArray();
            });

            Assert.Equal(new double?[] { 16.0, 4.0, 16.0, 16.0, 4.0, 16.0 }, rises);
        }

        [Fact]
        public void Todos_WritesEveryFrenteOfEveryTargetFondo()
        {
            var (targets, outside) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3, frentes: 3);
                SelectiveTargetsTestSupport.SetTargets(window, 1, 3);
                ApplyFront(window, floorBeam: true, rise: "9", scope: "Todos los frentes");
                return (RisesOf(window, 0).Concat(RisesOf(window, 2)).ToArray(), RisesOf(window, 1));
            });

            Assert.All(targets, r => Assert.Equal(9.0, r));
            Assert.All(outside, r => Assert.Equal(4.0, r));
        }

        [Fact]
        public void ADivergentTopology_SkipsTheFrenteAFondoDoesNotHave()
        {
            var (longFondo, shortFondo) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2, frentes: 3);          // both fondos start with 3 frentes
                SelectiveTargetsTestSupport.SetTargets(window, 2);
                EditorWindowTestSupport.SetText(window, "BayCountBox", "1"); // fondo 2 shrinks to one frente
                RaiseLostFocus(window, "BayCountBox");

                SelectiveTargetsTestSupport.SetTargets(window, 1, 2);
                window.EditorState.SelectCell(2, 0, extend: false);
                ApplyFront(window, floorBeam: true, rise: "21", scope: "Este frente");
                return (RisesOf(window, 0), RisesOf(window, 1));
            });

            Assert.Equal(new double?[] { 4.0, 4.0, 21.0 }, longFondo);
            Assert.Single(shortFondo);                 // never padded to reach frente 3
            Assert.Equal(4.0, shortFondo[0]);
        }

        // ---- The elevation is direct: no inheritance, and it survives "piso" being off ----

        [Fact]
        public void ChangingOneFrentesElevation_DoesNotMoveTheOthers()
        {
            var rises = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1, frentes: 3);
                window.EditorState.SelectCell(1, 0, extend: false);
                ApplyFront(window, floorBeam: true, rise: "18", scope: "Este frente");
                return RisesOf(window, 0);
            });

            Assert.Equal(new double?[] { 4.0, 18.0, 4.0 }, rises);
        }

        [Fact]
        public void TurningPisoOff_KeepsTheElevation_AndTurningItBackOnRecoversIt()
        {
            var (offRise, onRise, offFloor) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1, frentes: 2);
                window.EditorState.SelectCell(0, 0, extend: false);
                ApplyFront(window, floorBeam: true, rise: "17", scope: "Este frente");

                ApplyFront(window, floorBeam: false, rise: "17", scope: "Este frente");
                var afterOff = RisesOf(window, 0)[0];
                var floorOff = FloorsOf(window, 0)[0];

                ApplyFront(window, floorBeam: true, rise: "17", scope: "Este frente");
                return (afterOff, RisesOf(window, 0)[0], floorOff);
            });

            Assert.Equal(17.0, offRise); // the value is kept while the beam is off
            Assert.False(offFloor);
            Assert.Equal(17.0, onRise);
        }

        [Fact]
        public void TheFrentePanelShowsTheFrenteOfTheSelectedCell()
        {
            var (first, second) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1, frentes: 3);
                var state = window.EditorState;
                state.SelectCell(2, 0, extend: false);
                ApplyFront(window, floorBeam: true, rise: "23", scope: "Este frente");

                state.SelectCell(0, 0, extend: false);
                window.LoadCellEditorForTest();
                var atFrente1 = ((TextBox)window.FindName("FrontRiseBox")).Text;

                state.SelectCell(2, 0, extend: false);
                window.LoadCellEditorForTest();
                return (atFrente1, ((TextBox)window.FindName("FrontRiseBox")).Text);
            });

            Assert.Equal("4", first);
            Assert.Equal("23", second);
        }

        // ---- One recompute per bulk operation ----

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        public void AFrenteApply_RecomputesExactlyOnce(int targets)
        {
            var count = StaTestRunner.Run(() =>
            {
                var window = OpenWith(4, frentes: 3);
                if (targets == 1) SelectiveTargetsTestSupport.SetCurrentTarget(window);
                else SelectiveTargetsTestSupport.SetAllTargets(window);

                var before = window.RecomputeCount;
                ApplyFront(window, floorBeam: true, rise: "11", scope: "Todos los frentes");
                return window.RecomputeCount - before;
            });

            Assert.Equal(1, count);
        }

        [Fact]
        public void ThereIsExactlyOneTargetFondoSelector_AndItLivesInTheLeftPanel()
        {
            var (selector, oldBox) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2);
                return (window.FindName("TargetFondosList"), window.FindName("TargetFondosBox"));
            });

            Assert.NotNull(selector);
            Assert.Null(oldBox); // the old syntax field is gone
        }
    }
}
