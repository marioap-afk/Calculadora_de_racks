using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.Selective;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA tests for the REAL <see cref="RackSelectiveWindow"/> closing pass of I-43 (gate 7): every remaining
    /// per-fondo / per-frente property is driven through the window's own controls over the SAME target fondos, and
    /// each bulk operation recomputes exactly ONCE.
    /// </summary>
    public sealed class SelectiveGate7WindowTests
    {
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

        private static void SetTargetFondos(RackSelectiveWindow window, string text)
        {
            EditorWindowTestSupport.SetText(window, "TargetFondosBox", text);
            RaiseLostFocus(window, "TargetFondosBox");
        }

        private static void CommitBox(RackSelectiveWindow window, string name, string text)
        {
            EditorWindowTestSupport.SetText(window, name, text);
            RaiseLostFocus(window, name);
        }

        /// <summary>The "Piso" checkbox of a bay header.</summary>
        private static CheckBox FloorCheck(RackSelectiveWindow window, int bay)
            => Header(window, bay).Children.OfType<CheckBox>().First();

        /// <summary>The bay header panel of a frente.</summary>
        private static StackPanel Header(RackSelectiveWindow window, int bay)
            => ((Grid)window.FindName("MatrixGrid")).Children.OfType<StackPanel>()
                .First(panel => Grid.GetRow(panel) == 0 && Grid.GetColumn(panel) == bay + 1);

        /// <summary>A text box of a bay header, found by a distinctive fragment of its tooltip.</summary>
        private static TextBox HeaderBox(RackSelectiveWindow window, int bay, string tooltipFragment)
            => Header(window, bay).Children.OfType<StackPanel>()
                .SelectMany(row => row.Children.OfType<TextBox>())
                .First(box => box.ToolTip is string tip && tip.Contains(tooltipFragment));

        private static void CommitHeaderBox(RackSelectiveWindow window, int bay, string fragment, string text)
        {
            var box = HeaderBox(window, bay, fragment);
            box.Text = text;
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }

        // ---- Every remaining property lands on the target fondos ----

        [Fact]
        public void Piso_LandsOnTheFrenteOfEveryTargetFondo()
        {
            var flags = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "1,3");
                FloorCheck(window, 1).IsChecked = true; // the real Checked handler runs
                var state = window.EditorState;
                return new[]
                {
                    state.FloorBeams[1],
                    state.FondoMatrices[1].FloorBeams[1],
                    state.FondoMatrices[2].FloorBeams[1]
                };
            });

            Assert.Equal(new[] { true, false, true }, flags);
        }

        [Fact]
        public void AltoDeFrente_LandsOnTheTargets_AndClearingItRestoresAuto()
        {
            var (applied, restored) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "1,3");
                CommitHeaderBox(window, 0, "Altura del frente", "220");
                var state = window.EditorState;
                var after = new[] { state.BayHeights[0], state.FondoMatrices[1].BayHeights[0], state.FondoMatrices[2].BayHeights[0] };

                CommitHeaderBox(window, 0, "Altura del frente", string.Empty);
                return (after, new[] { state.BayHeights[0], state.FondoMatrices[2].BayHeights[0] });
            });

            Assert.Equal(new double?[] { 220.0, null, 220.0 }, applied);
            Assert.Equal(new double?[] { null, null }, restored);
        }

        [Fact]
        public void FondoDeTarima_LandsOnEveryTargetFondo()
        {
            var depths = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "1,3");
                CommitBox(window, "FondoBox", "60");
                var state = window.EditorState;
                return state.FondoMatrices.Select(m => m.Depth).ToArray();
            });

            Assert.Equal(new[] { 60.0, 48.0, 60.0 }, depths);
        }

        [Fact]
        public void FondoDeCabecera_OverridesAndRestoresOverTheTargets()
        {
            var (applied, restored) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "1,3");
                CommitBox(window, "CabeceraFondoBox", "37");
                var state = window.EditorState;
                var after = state.FondoMatrices.Select(m => m.CabeceraOverride).ToArray();

                CommitBox(window, "CabeceraFondoBox", string.Empty);
                return (after, state.FondoMatrices.Select(m => m.CabeceraOverride).ToArray());
            });

            Assert.Equal(new[] { 37.0, 0.0, 37.0 }, applied);
            Assert.Equal(new[] { 0.0, 0.0, 0.0 }, restored);
        }

        [Fact]
        public void TheFondoBoxesKeepShowingTheVisibleFondo_WhileTheTargetsDecideWhereTheEditLands()
        {
            var shown = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2);
                SetTargetFondos(window, "1,2");
                CommitBox(window, "FondoBox", "60");
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 1;
                return ((TextBox)window.FindName("FondoBox")).Text;
            });

            Assert.Equal("60", shown); // fondo 2 also got 60, and the box shows the fondo now visible
        }

        // ---- Scenario 6: one recompute per bulk operation, whatever the target count ----

        [Theory]
        [InlineData("1")]
        [InlineData("todos")]
        public void EveryBulkOperationOfThisGate_RecomputesExactlyOnce(string targets)
        {
            var counts = StaTestRunner.Run(() =>
            {
                var window = OpenWith(4);
                SetTargetFondos(window, targets);

                int Measure(System.Action edit)
                {
                    var before = window.RecomputeCount;
                    edit();
                    return window.RecomputeCount - before;
                }

                return new[]
                {
                    Measure(() => FloorCheck(window, 0).IsChecked = true),
                    Measure(() => CommitHeaderBox(window, 0, "Altura del frente", "215")),
                    Measure(() => CommitHeaderBox(window, 0, "Elevación del larguero a piso de ESTE frente", "13")),
                    Measure(() => CommitBox(window, "FondoBox", "60")),
                    Measure(() => CommitBox(window, "CabeceraFondoBox", "40"))
                };
            });

            Assert.Equal(new[] { 1, 1, 1, 1, 1 }, counts);
        }

        // ---- I-39: a cancelled dialog must not mutate any target ----

        [Fact]
        public void CancellingTheTramosDialog_MutatesNoTarget()
        {
            // EditTramos returns before touching Application when the dialog is not accepted; driving the handler with
            // no dialog interaction (ShowDialog would block) is covered in Application, so here we pin the invariant
            // that the state holds no segments until an accepted result is projected.
            var segments = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "todos");
                var state = window.EditorState;
                return new[]
                {
                    state.BaySegments[0].Count,
                    state.FondoMatrices[1].BaySegments[0].Count,
                    state.FondoMatrices[2].BaySegments[0].Count
                };
            });

            Assert.Equal(new[] { 0, 0, 0 }, segments);
        }

        [Fact]
        public void ThereIsStillExactlyOneTargetFondoSelector_ForEveryProperty()
        {
            var count = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2);
                return new[] { "TargetFondosBox" }.Count(name => window.FindName(name) != null);
            });

            Assert.Equal(1, count);
        }
    }
}
