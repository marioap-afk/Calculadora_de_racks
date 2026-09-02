using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43 gate 7, the real "⇊" gesture. Clicking the scope button next to a text box must perform EXACTLY one
    /// Application operation — the <c>All</c> one — even though WPF moves focus to a Button on mouse-down and would
    /// otherwise make the box commit its <c>Front</c> value first.
    /// <para>
    /// The tests reproduce WPF's own order and its dependence on <c>Handled</c> rather than raising <c>Click</c>
    /// directly, which is precisely what let the double application hide.
    /// </para>
    /// </summary>
    public sealed class SelectiveScopeButtonGestureTests
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
                EditorWindowTestSupport.SetText(window, "BayCountBox", frentes.ToString());
                RaiseLostFocus(window, "BayCountBox");
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

        private static StackPanel Header(RackSelectiveWindow window, int bay)
            => ((Grid)window.FindName("MatrixGrid")).Children.OfType<StackPanel>()
                .First(panel => Grid.GetRow(panel) == 0 && Grid.GetColumn(panel) == bay + 1);

        private static TextBox HeaderBox(RackSelectiveWindow window, int bay, string tooltipFragment)
            => Header(window, bay).Children.OfType<StackPanel>()
                .SelectMany(row => row.Children.OfType<TextBox>())
                .First(box => box.ToolTip is string tip && tip.Contains(tooltipFragment));

        private static Button HeaderButton(RackSelectiveWindow window, int bay, string tooltipFragment)
            => Header(window, bay).Children.OfType<StackPanel>()
                .SelectMany(row => row.Children.OfType<Button>())
                .First(button => button.ToolTip is string tip && tip.Contains(tooltipFragment));

        /// <summary>
        /// The REAL gesture: type into the box, then click its "⇊". WPF raises PreviewMouseLeftButtonDown first; only
        /// if nobody handles it does ButtonBase take focus — which makes the box raise LostFocus — and then Click.
        /// Reproducing that dependence is what makes this test able to see a double application.
        /// </summary>
        private static void TypeThenClickScopeButton(TextBox box, Button button, string text)
        {
            box.Text = text;

            var down = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent,
                Source = button
            };
            button.RaiseEvent(down);
            if (down.Handled)
            {
                return; // the gesture was consumed before focus could move: nothing else happens
            }

            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));           // WPF moved focus first
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        }

        /// <summary>The rise overrides of a fondo, over the frentes THAT FONDO really has. The frente count is
        /// per fondo, so reading a fixed number would ask for frentes a shorter fondo never had.</summary>
        private static System.Collections.Generic.IEnumerable<double?> RisesOf(
            RackCad.Application.Systems.Selective.SelectiveEditorState state, int fondo)
        {
            var frentes = fondo == state.SelectedFondo ? state.Bays.Count : state.FondoMatrices[fondo].Bays.Count;
            return Enumerable.Range(0, frentes).Select(front => state.FloorBeamRiseOverrideAt(fondo, front)).ToList();
        }

        private const string HeightTip = "Altura del frente";
        private const string HeightAllTip = "Aplicar esta altura a TODOS los frentes";
        private const string RiseTip = "Elevación del larguero a piso de ESTE frente";
        private const string RiseAllTip = "Aplicar esta elevación a TODOS los frentes";

        // ---- Alto de frente ----

        [Fact]
        public void Height_TheRealGesture_RunsOnlyTheAllOperation_Once()
        {
            var (log, recomputes, targets, outside) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3, frentes: 3);
                SetTargetFondos(window, "1,3");
                window.FrontApplyLog.Clear();
                var before = window.RecomputeCount;

                TypeThenClickScopeButton(HeaderBox(window, 0, HeightTip), HeaderButton(window, 0, HeightAllTip), "220");

                var state = window.EditorState;
                return (
                    window.FrontApplyLog.ToArray(),
                    window.RecomputeCount - before,
                    state.BayHeights.Concat(state.FondoMatrices[2].BayHeights).ToArray(),
                    state.FondoMatrices[1].BayHeights.ToArray());
            });

            Assert.Equal(new[] { "Alto:All" }, log); // exactly ONE operation, and it is the All one
            Assert.Equal(1, recomputes);
            Assert.All(targets, h => Assert.Equal(220.0, h));
            Assert.All(outside, h => Assert.Null(h));
        }

        [Fact]
        public void Height_TheRealGestureWithAnEmptyBox_RestoresAutoOnEveryTargetFrente_Once()
        {
            var (log, recomputes, heights) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2, frentes: 3);
                SetTargetFondos(window, "1,2");
                TypeThenClickScopeButton(HeaderBox(window, 0, HeightTip), HeaderButton(window, 0, HeightAllTip), "220");

                window.FrontApplyLog.Clear();
                var before = window.RecomputeCount;
                TypeThenClickScopeButton(HeaderBox(window, 0, HeightTip), HeaderButton(window, 0, HeightAllTip), string.Empty);

                var state = window.EditorState;
                return (window.FrontApplyLog.ToArray(), window.RecomputeCount - before,
                    state.BayHeights.Concat(state.FondoMatrices[1].BayHeights).ToArray());
            });

            Assert.Equal(new[] { "Alto:All" }, log);
            Assert.Equal(1, recomputes);
            Assert.All(heights, h => Assert.Null(h));
        }

        // ---- Elevación de larguero a piso ----

        [Fact]
        public void Rise_TheRealGesture_RunsOnlyTheAllOperation_Once()
        {
            var (log, recomputes, targets, outside) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3, frentes: 3);
                SetTargetFondos(window, "1,3");
                window.FrontApplyLog.Clear();
                var before = window.RecomputeCount;

                TypeThenClickScopeButton(HeaderBox(window, 0, RiseTip), HeaderButton(window, 0, RiseAllTip), "13");

                var state = window.EditorState;
                return (
                    window.FrontApplyLog.ToArray(),
                    window.RecomputeCount - before,
                    RisesOf(state, 0).Concat(RisesOf(state, 2)).ToArray(),
                    RisesOf(state, 1).ToArray());
            });

            Assert.Equal(new[] { "Elevacion:All" }, log);
            Assert.Equal(1, recomputes);
            Assert.All(targets, r => Assert.Equal(13.0, r));
            Assert.All(outside, r => Assert.Null(r));
        }

        [Fact]
        public void Rise_TheRealGestureWithAnEmptyBox_RestoresTheGlobalOnEveryTargetFrente_Once()
        {
            var (log, recomputes, rises) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2, frentes: 3);
                SetTargetFondos(window, "1,2");
                TypeThenClickScopeButton(HeaderBox(window, 0, RiseTip), HeaderButton(window, 0, RiseAllTip), "13");

                window.FrontApplyLog.Clear();
                var before = window.RecomputeCount;
                TypeThenClickScopeButton(HeaderBox(window, 0, RiseTip), HeaderButton(window, 0, RiseAllTip), string.Empty);

                var state = window.EditorState;
                return (window.FrontApplyLog.ToArray(), window.RecomputeCount - before,
                    RisesOf(state, 0).Concat(RisesOf(state, 1)).ToArray());
            });

            Assert.Equal(new[] { "Elevacion:All" }, log);
            Assert.Equal(1, recomputes);
            Assert.All(rises, r => Assert.Null(r));
        }

        [Fact]
        public void Rise_TheRealGestureWithZero_IsAnExplicitOverride_NotARestore()
        {
            var (log, rises) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2, frentes: 3);
                SetTargetFondos(window, "1,2");
                window.FrontApplyLog.Clear();

                TypeThenClickScopeButton(HeaderBox(window, 0, RiseTip), HeaderButton(window, 0, RiseAllTip), "0");

                var state = window.EditorState;
                return (window.FrontApplyLog.ToArray(), RisesOf(state, 0).Concat(RisesOf(state, 1)).ToArray());
            });

            Assert.Equal(new[] { "Elevacion:All" }, log);
            Assert.All(rises, r => Assert.Equal(0.0, r)); // a value, not an inheritance
        }

        // ---- Control: the ordinary paths still commit Front ----

        [Fact]
        public void LeavingTheBoxTowardsAnOrdinaryControl_StillCommitsFront()
        {
            var (log, heights) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2, frentes: 3);
                SetTargetFondos(window, "1,2");
                window.FrontApplyLog.Clear();

                var box = HeaderBox(window, 0, HeightTip);
                box.Text = "215";
                box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box)); // focus went somewhere ordinary

                var state = window.EditorState;
                return (window.FrontApplyLog.ToArray(), state.BayHeights.ToArray());
            });

            Assert.Equal(new[] { "Alto:Front" }, log);
            Assert.Equal(new double?[] { 215.0, null, null }, heights); // only the edited frente
        }

        [Fact]
        public void PressingEnterInTheBox_StillCommitsFront()
        {
            var (log, rises) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(2, frentes: 3);
                SetTargetFondos(window, "1,2");
                window.FrontApplyLog.Clear();

                var box = HeaderBox(window, 0, RiseTip);
                box.Text = "11";
                box.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, new TestPresentationSource(), 0, Key.Enter)
                {
                    RoutedEvent = UIElement.KeyDownEvent
                });

                var state = window.EditorState;
                return (window.FrontApplyLog.ToArray(), RisesOf(state, 0).ToArray());
            });

            Assert.Equal(new[] { "Elevacion:Front" }, log);
            Assert.Equal(new double?[] { 11.0, null, null }, rises);
        }

        /// <summary>A minimal source so a <see cref="KeyEventArgs"/> can be constructed for a window that is never shown.</summary>
        private sealed class TestPresentationSource : PresentationSource
        {
            protected override CompositionTarget GetCompositionTargetCore() => null;

            public override Visual RootVisual { get; set; }

            public override bool IsDisposed => false;
        }
    }
}
