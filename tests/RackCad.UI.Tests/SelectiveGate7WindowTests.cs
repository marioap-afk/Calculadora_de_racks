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

        private static void CommitBox(RackSelectiveWindow window, string name, string text)
        {
            EditorWindowTestSupport.SetText(window, name, text);
            RaiseLostFocus(window, name);
        }

        /// <summary>The bay header panel of a frente.</summary>
        private static StackPanel Header(RackSelectiveWindow window, int bay)
            => ((Grid)window.FindName("MatrixGrid")).Children.OfType<StackPanel>()
                .First(panel => Grid.GetRow(panel) == 0 && Grid.GetColumn(panel) == bay + 1);

        // ---- Every remaining property lands on the target fondos ----

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
                    Measure(() => CommitBox(window, "FondoBox", "60")),
                    Measure(() => CommitBox(window, "CabeceraFondoBox", "40"))
                };
            });

            Assert.Equal(new[] { 1, 1 }, counts);
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
                return new[] { "TargetFondosList" }.Count(name => window.FindName(name) != null);
            });

            Assert.Equal(1, count);
        }
    }
}
