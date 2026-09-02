using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.Selective;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// STA tests for the REAL <see cref="RackSelectiveWindow"/> wiring of I-43 / ID14: the per-frente "elevacion de
    /// larguero a piso" is edited in the bay header, applies over the SAME target fondos, and a multi-fondo write
    /// recomputes ONCE.
    /// </summary>
    public sealed class SelectiveFloorBeamRiseWindowTests
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

        /// <summary>The per-frente "Elev." box of a bay header, found by its tooltip text.</summary>
        private static TextBox RiseBox(RackSelectiveWindow window, int bay)
        {
            var grid = (Grid)window.FindName("MatrixGrid");
            var header = grid.Children.OfType<StackPanel>().First(panel => Grid.GetRow(panel) == 0 && Grid.GetColumn(panel) == bay + 1);
            return header.Children.OfType<StackPanel>()
                .SelectMany(row => row.Children.OfType<TextBox>())
                .First(box => box.ToolTip is string tip && tip.Contains("Elevación del larguero a piso de ESTE frente"));
        }

        private static void CommitRise(RackSelectiveWindow window, int bay, string text)
        {
            var box = RiseBox(window, bay);
            box.Text = text;
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }

        [Fact]
        public void TheBayHeaderShowsAPerFrenteRiseBox_EmptyWhenItInheritsTheGlobal()
        {
            var text = StaTestRunner.Run(() => RiseBox(OpenWith(1), 0).Text);

            Assert.Equal(string.Empty, text);
        }

        [Fact]
        public void EditingTheBoxWritesThatFrenteInEveryTargetFondo()
        {
            var (fondo1, fondo2, fondo3, otherFrente) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "1,3");
                CommitRise(window, 1, "14");
                var state = window.EditorState;
                return (
                    state.FloorBeamRiseOverrideAt(0, 1),
                    state.FloorBeamRiseOverrideAt(1, 1),
                    state.FloorBeamRiseOverrideAt(2, 1),
                    state.FloorBeamRiseOverrideAt(0, 0));
            });

            Assert.Equal(14.0, fondo1);
            Assert.Null(fondo2);   // fondo 2 was not a target
            Assert.Equal(14.0, fondo3);
            Assert.Null(otherFrente);
        }

        [Fact]
        public void ClearingTheBoxRestoresTheGlobal_AndAnExplicitZeroIsKept()
        {
            var (restored, zero) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1);
                CommitRise(window, 0, "14");
                CommitRise(window, 0, string.Empty);
                var afterRestore = window.EditorState.FloorBeamRiseOverrideAt(0, 0);

                CommitRise(window, 0, "0");
                return (afterRestore, window.EditorState.FloorBeamRiseOverrideAt(0, 0));
            });

            Assert.Null(restored);
            Assert.Equal(0.0, zero); // a value, not an inheritance
        }

        [Fact]
        public void AMultiFondoRiseEdit_RecomputesExactlyOnce()
        {
            var (single, multi) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(4);

                SetTargetFondos(window, "1");
                var before = window.RecomputeCount;
                CommitRise(window, 0, "10");
                var afterSingle = window.RecomputeCount - before;

                SetTargetFondos(window, "todos");
                before = window.RecomputeCount;
                CommitRise(window, 0, "12");
                return (afterSingle, window.RecomputeCount - before);
            });

            Assert.Equal(1, single);
            Assert.Equal(1, multi); // four fondos, still ONE recompute
        }

        [Fact]
        public void UncheckingPiso_DoesNotClearTheFrentesRise()
        {
            var kept = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1);
                CommitRise(window, 0, "16");
                window.EditorState.FloorBeams[0] = false; // the same effect as unchecking "Piso"
                window.EditorState.FloorBeams[0] = true;
                return window.EditorState.FloorBeamRiseOverrideAt(0, 0);
            });

            Assert.Equal(16.0, kept);
        }

        [Fact]
        public void TheGlobalBoxStillExists_AndSaysItIsTheDefault()
        {
            var (exists, tip) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(1);
                var box = (TextBox)window.FindName("FloorRiseBox");
                return (box != null, box.ToolTip as string);
            });

            Assert.True(exists);
            Assert.Contains("PREDETERMINADO", tip);
        }

        [Fact]
        public void TheRiseReusesTheSameTargetFondos_WithNoSecondSelector()
        {
            var boxes = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2);
                return new[] { "TargetFondosBox" }.Count(name => window.FindName(name) != null);
            });

            Assert.Equal(1, boxes);
        }
    }
}
