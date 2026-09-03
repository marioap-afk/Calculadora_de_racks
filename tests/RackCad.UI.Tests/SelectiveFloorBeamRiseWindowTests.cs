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
            var window = SelectiveWindowTestSupport.Open();
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

        /// <summary>Choose the reach the three "Aplicar" buttons of the frente section obey.</summary>
        private static void SetFrontScope(RackSelectiveWindow window, string scope)
            => ((ComboBox)window.FindName("FrontScopeBox")).SelectedIndex =
                scope == "Todos los frentes" ? 2 : scope == "Frentes seleccionados" ? 1 : 0;

        /// <summary>Apply ONLY "larguero a piso" over the given reach.</summary>
        private static void ApplyFloorBeam(RackSelectiveWindow window, bool floorBeam, string scope)
        {
            ((CheckBox)window.FindName("FrontFloorBeamCheck")).IsChecked = floorBeam;
            SetFrontScope(window, scope);
            EditorWindowTestSupport.ClickByContent(window, "Aplicar piso");
        }

        /// <summary>Apply ONLY the elevation over the given reach.</summary>
        private static void ApplyRise(RackSelectiveWindow window, string rise, string scope)
        {
            ((TextBox)window.FindName("FrontRiseBox")).Text = rise;
            SetFrontScope(window, scope);
            EditorWindowTestSupport.ClickByContent(window, "Aplicar elevación");
        }

        /// <summary>Apply ONLY the level count over the given reach.</summary>
        private static void ApplyLevels(RackSelectiveWindow window, string levels, string scope)
        {
            ((TextBox)window.FindName("FrontLevelsBox")).Text = levels;
            SetFrontScope(window, scope);
            EditorWindowTestSupport.ClickByContent(window, "Aplicar niveles");
        }

        /// <summary>Both properties, as two deliberate operations (they never travel together any more).</summary>
        private static void ApplyFront(RackSelectiveWindow window, bool floorBeam, string rise, string scope)
        {
            ApplyFloorBeam(window, floorBeam, scope);
            ApplyRise(window, rise, scope);
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
            Assert.Equal(0, texts.Item2); // no "Elev.", no "Alto" and no level boxes
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
                ApplyRise(window, "11", scope: "Todos los frentes");
                return window.RecomputeCount - before;
            });

            Assert.Equal(1, count);
        }

        // ---- The two properties are independent ----

        [Fact]
        public void ApplyingPiso_LeavesEveryElevationExactlyWhereItWas()
        {
            // Three frentes at 4 / 8 / 12: switching the beam ON everywhere must not flatten them to the value that
            // happened to be showing for the primary frente.
            var (rises, floors) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1, frentes: 3);
                var state = window.EditorState;
                state.SelectCell(0, 0, extend: false); ApplyRise(window, "4", "Este frente");
                state.SelectCell(1, 0, extend: false); ApplyRise(window, "8", "Este frente");
                state.SelectCell(2, 0, extend: false); ApplyRise(window, "12", "Este frente");

                state.SelectCell(0, 0, extend: false);
                window.LoadCellEditorForTest();
                ApplyFloorBeam(window, true, "Todos los frentes");
                return (RisesOf(window, 0), FloorsOf(window, 0));
            });

            Assert.Equal(new double?[] { 4.0, 8.0, 12.0 }, rises);
            Assert.All(floors, floor => Assert.True(floor));
        }

        [Fact]
        public void ApplyingTheElevation_LeavesEveryPisoFlagExactlyWhereItWas()
        {
            var (floors, rises) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1, frentes: 3);
                var state = window.EditorState;
                state.SelectCell(0, 0, extend: false); ApplyFloorBeam(window, true, "Este frente");
                state.SelectCell(1, 0, extend: false); ApplyFloorBeam(window, false, "Este frente");
                state.SelectCell(2, 0, extend: false); ApplyFloorBeam(window, true, "Este frente");

                ApplyRise(window, "20", "Todos los frentes");
                return (FloorsOf(window, 0), RisesOf(window, 0));
            });

            Assert.Equal(new[] { true, false, true }, floors);
            Assert.All(rises, rise => Assert.Equal(20.0, rise));
        }

        [Fact]
        public void EachPropertyReachesItsOwnSubsetOfFondos_WithSelected()
        {
            var (rises, floors, outside) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3, frentes: 3);
                SelectiveTargetsTestSupport.SetTargets(window, 1, 3);
                var state = window.EditorState;
                state.SelectCell(0, 0, extend: false);
                state.SelectCell(2, 0, extend: true);

                ApplyRise(window, "15", "Frentes seleccionados");
                ApplyFloorBeam(window, true, "Frentes seleccionados");
                return (RisesOf(window, 0), FloorsOf(window, 2), RisesOf(window, 1));
            });

            Assert.Equal(new double?[] { 15.0, 4.0, 15.0 }, rises);
            Assert.Equal(new[] { true, false, true }, floors);
            Assert.All(outside, r => Assert.Equal(4.0, r)); // fondo 2 was not a target
        }

        // ---- Levels are a frente property in the panel now ----

        [Fact]
        public void Levels_AreAnExactCount_OverTheChosenReachAndTargets()
        {
            var (targets, outside) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3, frentes: 3);
                SelectiveTargetsTestSupport.SetTargets(window, 1, 3);
                ApplyLevels(window, "5", "Todos los frentes");

                var state = window.EditorState;
                return (
                    state.Bays.Select(c => c.Count).Concat(state.FondoMatrices[2].Bays.Select(c => c.Count)).ToArray(),
                    state.FondoMatrices[1].Bays.Select(c => c.Count).ToArray());
            });

            Assert.All(targets, count => Assert.Equal(5, count));
            Assert.All(outside, count => Assert.Equal(4, count)); // fondo 2 untouched (the editor opens with 4 levels)
        }

        [Fact]
        public void Levels_NeverEmptyAFrente()
        {
            var counts = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1, frentes: 2);
                ApplyLevels(window, "1", "Todos los frentes");
                return window.EditorState.Bays.Select(c => c.Count).ToArray();
            });

            Assert.All(counts, count => Assert.Equal(1, count));
        }

        // ---- "Actual" is a mode, not a set that happens to match ----

        [Fact]
        public void Actual_FollowsTheFondoBeingEdited()
        {
            var (caption, targets) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SelectiveTargetsTestSupport.SetCurrentTarget(window);
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 2;
                return (SelectiveTargetsTestSupport.Caption(window), window.EditorState.TargetFondos.Fondos.ToArray());
            });

            Assert.Equal("Actual", caption);
            Assert.Equal(new[] { 2 }, targets);
        }

        [Fact]
        public void AnExplicitSingleFondo_DoesNotFollowWhenNavigating()
        {
            var (caption, targets) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 1; // looking at fondo 2
                SelectiveTargetsTestSupport.SetTargets(window, 2);                 // and choosing it EXPLICITLY
                var before = SelectiveTargetsTestSupport.Caption(window);

                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 2; // navigate to fondo 3
                return (before, window.EditorState.TargetFondos.Fondos.ToArray());
            });

            Assert.Equal("Fondo 2", caption);   // reads as itself, not as "Actual"
            Assert.Equal(new[] { 1 }, targets); // and still writes fondo 2
        }

        [Fact]
        public void PressingActualWhileItIsAlreadyTheMode_LeavesUiAndStateCoherent()
        {
            // "Actual" is an action, not a tick: pressing it again re-affirms the mode and cannot leave the popup
            // showing something the editor does not hold.
            var (caption, mode, targets, marked) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SelectiveTargetsTestSupport.SetCurrentTarget(window);
                SelectiveTargetsTestSupport.SetCurrentTarget(window); // press it again
                var state = window.EditorState;
                return (SelectiveTargetsTestSupport.Caption(window), state.TargetMode,
                    state.TargetFondos.Fondos.ToArray(), SelectiveTargetsTestSupport.CheckedFondos(window));
            });

            Assert.Equal("Actual", caption);
            Assert.Equal(RackCad.Application.Systems.Selective.SelectiveTargetMode.FollowCurrent, mode);
            Assert.Equal(new[] { 0 }, targets);
            Assert.Empty(marked); // the fondo boxes stay empty while "Actual" is the mode
        }

        [Fact]
        public void PressingTodosWhileItIsAlreadyTheMode_LeavesUiAndStateCoherent()
        {
            var (caption, targets, marked) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SelectiveTargetsTestSupport.SetAllTargets(window);
                SelectiveTargetsTestSupport.SetAllTargets(window); // press it again
                return (SelectiveTargetsTestSupport.Caption(window),
                    window.EditorState.TargetFondos.Fondos.ToArray(),
                    SelectiveTargetsTestSupport.CheckedFondos(window));
            });

            Assert.Equal("Todos", caption);
            Assert.Equal(new[] { 0, 1, 2 }, targets);
            Assert.Equal(new[] { 1, 2, 3 }, marked); // and every box reads as ticked
        }

        [Fact]
        public void LeavingActualThroughAFondoBox_StartsTheExplicitSetEmpty()
        {
            // Visible = fondo 2, mode = Actual, every box empty. Ticking fondo 3 must give {3} — not {2,3}: the
            // followed fondo was never ticked, so it must not be carried into the explicit set.
            var (targets, caption, marked) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 1; // looking at fondo 2
                SelectiveTargetsTestSupport.SetCurrentTarget(window);
                SelectiveTargetsTestSupport.ToggleFondo(window, 3, true);
                return (window.EditorState.TargetFondos.Fondos.ToArray(),
                    SelectiveTargetsTestSupport.Caption(window),
                    SelectiveTargetsTestSupport.CheckedFondos(window));
            });

            Assert.Equal(new[] { 2 }, targets);
            Assert.Equal("Fondo 3", caption);
            Assert.Equal(new[] { 3 }, marked);
        }

        [Fact]
        public void TheFondoBoxesAreTheVisibleTruthOfAnExplicitSet()
        {
            var (afterAdd, afterRemove) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SelectiveTargetsTestSupport.SetCurrentTarget(window);
                SelectiveTargetsTestSupport.ToggleFondo(window, 3, true);
                SelectiveTargetsTestSupport.ToggleFondo(window, 1, true);
                var added = window.EditorState.TargetFondos.Fondos.ToArray();

                SelectiveTargetsTestSupport.ToggleFondo(window, 3, false);
                return (added, window.EditorState.TargetFondos.Fondos.ToArray());
            });

            Assert.Equal(new[] { 0, 2 }, afterAdd);
            Assert.Equal(new[] { 0 }, afterRemove);
        }

        [Fact]
        public void EmptyingTheExplicitSet_GoesBackToActual()
        {
            var (mode, caption, targets) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 1;
                SelectiveTargetsTestSupport.SetCurrentTarget(window);
                SelectiveTargetsTestSupport.ToggleFondo(window, 3, true);
                SelectiveTargetsTestSupport.ToggleFondo(window, 3, false); // the last explicit target goes away
                var state = window.EditorState;
                return (state.TargetMode, SelectiveTargetsTestSupport.Caption(window), state.TargetFondos.Fondos.ToArray());
            });

            Assert.Equal(RackCad.Application.Systems.Selective.SelectiveTargetMode.FollowCurrent, mode);
            Assert.Equal("Actual", caption);
            Assert.Equal(new[] { 1 }, targets); // the fondo on screen, because that is what "Actual" means
        }

        [Fact]
        public void AnExplicitSubsetOrAll_SurvivesNavigation()
        {
            var (subset, all) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SelectiveTargetsTestSupport.SetTargets(window, 1, 3);
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 1;
                var afterSubset = window.EditorState.TargetFondos.Fondos.ToArray();

                SelectiveTargetsTestSupport.SetAllTargets(window);
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 2;
                return (afterSubset, window.EditorState.TargetFondos.Fondos.ToArray());
            });

            Assert.Equal(new[] { 0, 2 }, subset);
            Assert.Equal(new[] { 0, 1, 2 }, all);
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
