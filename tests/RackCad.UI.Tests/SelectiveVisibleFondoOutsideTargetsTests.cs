using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.RackFrames;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43 gate 7, closing gaps. Two things the editor must get right with the REAL window: a fondo-wide edit whose
    /// VISIBLE fondo is not one of the targets must not leak the typed value into that fondo, and the <c>All</c> reach
    /// of the two frente-wide flags must actually be usable.
    /// </summary>
    public sealed class SelectiveVisibleFondoOutsideTargetsTests
    {
        private static RackSelectiveWindow OpenWith(int fondos)
        {
            var window = SelectiveWindowTestSupport.Open();
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

        private static StackPanel Header(RackSelectiveWindow window, int bay)
            => ((Grid)window.FindName("MatrixGrid")).Children.OfType<StackPanel>()
                .First(panel => Grid.GetRow(panel) == 0 && Grid.GetColumn(panel) == bay + 1);

        /// <summary>A header button found by a fragment of its tooltip (the compact "⇊" scope buttons).</summary>
        private static Button HeaderButton(RackSelectiveWindow window, int bay, string tooltipFragment)
            => Header(window, bay).Children.OfType<StackPanel>()
                .SelectMany(row => row.Children.OfType<Button>())
                .First(button => button.ToolTip is string tip && tip.Contains(tooltipFragment));

        private static TextBox HeaderBox(RackSelectiveWindow window, int bay, string tooltipFragment)
            => Header(window, bay).Children.OfType<StackPanel>()
                .SelectMany(row => row.Children.OfType<TextBox>())
                .First(box => box.ToolTip is string tip && tip.Contains(tooltipFragment));

        private static CheckBox FloorCheck(RackSelectiveWindow window, int bay)
            => Header(window, bay).Children.OfType<StackPanel>()
                .SelectMany(row => row.Children.OfType<CheckBox>())
                .First();

        private static void Click(Button button)
            => button.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, button));

        private static RackFrameConfiguration Custom(RackSelectiveWindow window, double height)
            => new RackFrameConfigurationFactory(window.Session.Catalog).Build(
                RackFrameTemplateCatalog.FindStandardOrDefault(),
                "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA", height, 42.0);

        // ---- 1. The visible fondo is not a target ----

        [Fact]
        public void PalletDepth_WithTheVisibleFondoOutsideTheTargets_LeavesItUntouched_AndTheBoxShowsItsOwnValue()
        {
            var (depths, shown) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "2,3"); // visible is fondo 1
                CommitBox(window, "FondoBox", "60");
                return (window.EditorState.FondoMatrices.Select(m => m.Depth).ToArray(), ((TextBox)window.FindName("FondoBox")).Text);
            });

            Assert.Equal(new[] { 48.0, 60.0, 60.0 }, depths);
            Assert.Equal("48", shown); // the box always describes the VISIBLE fondo
        }

        [Fact]
        public void CabeceraDepth_WithTheVisibleFondoOutsideTheTargets_LeavesItUntouched_AndTheBoxGoesBackToEmpty()
        {
            var (overrides, shown) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "2,3");
                CommitBox(window, "CabeceraFondoBox", "37");
                return (window.EditorState.FondoMatrices.Select(m => m.CabeceraOverride).ToArray(), ((TextBox)window.FindName("CabeceraFondoBox")).Text);
            });

            Assert.Equal(new[] { 0.0, 37.0, 37.0 }, overrides);
            Assert.Equal(string.Empty, shown);
        }

        [Fact]
        public void RestoringTheCabeceraDepth_TouchesOnlyTheTargets_AndTheVisibleFondoWasNeverTouched()
        {
            var (afterSet, afterRestore) = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "2,3");
                CommitBox(window, "CabeceraFondoBox", "37");
                var set = window.EditorState.FondoMatrices.Select(m => m.CabeceraOverride).ToArray();

                // Tras el commit la caja vuelve a describir el fondo VISIBLE, que no es destino y no tiene override:
                // ya está vacía. Restablecer los destinos exige entonces una edición REAL — escribir algo y borrarlo —
                // porque una caja limpia no muta nada (I-43, O-43-02). Antes bastaba con salir del campo.
                EditorWindowTestSupport.SetText(window, "CabeceraFondoBox", "40");
                CommitBox(window, "CabeceraFondoBox", string.Empty);
                return (set, window.EditorState.FondoMatrices.Select(m => m.CabeceraOverride).ToArray());
            });

            Assert.Equal(new[] { 0.0, 37.0, 37.0 }, afterSet);
            Assert.Equal(new[] { 0.0, 0.0, 0.0 }, afterRestore);
        }

        [Fact]
        public void OnlyTheTargetsCustomCabecerasAdoptTheNewDepth_TheVisibleOneKeepsItsOwn()
        {
            var depths = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                var state = window.EditorState;
                SetTargetFondos(window, "1-3");
                state.ApplyCabeceraToTargets(0, Custom(window, 300.0), c => new RackCad.Application.Persistence.RackFrameProjectStore().DeepCopy(c));

                SetTargetFondos(window, "2,3"); // visible (fondo 1) is now OUTSIDE the targets
                CommitBox(window, "FondoBox", "60");
                return new[] { state.CabeceraAt(0, 0).Depth, state.CabeceraAt(1, 0).Depth, state.CabeceraAt(2, 0).Depth };
            });

            Assert.Equal(new[] { 42.0, 54.0, 54.0 }, depths); // 48-6 stays, 60-6 lands on the targets
        }

        [Fact]
        public void AFondoWideEditWithTheVisibleFondoOutsideTheTargets_RecomputesExactlyOnce()
        {
            var count = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "2,3");
                var before = window.RecomputeCount;
                CommitBox(window, "FondoBox", "60");
                return window.RecomputeCount - before;
            });

            Assert.Equal(1, count);
        }

        [Fact]
        public void AfterAFondoWideEdit_ChangingFondoStillPreservesAnExplicitMultiFondoTarget()
        {
            var targets = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "2,3");
                CommitBox(window, "FondoBox", "60");
                ((ComboBox)window.FindName("FondoSelectorBox")).SelectedIndex = 2;
                return window.EditorState.TargetFondos.Fondos.ToArray();
            });

            Assert.Equal(new[] { 1, 2 }, targets); // the gate-3 rule still holds
        }

        [Fact]
        public void TheTypedDepthDoesNotComeBackThroughBuildDesign()
        {
            // BuildDesign reads the two boxes again: if the sync had not restored them, the visible fondo would pick
            // up the typed value at the next build.
            var depths = StaTestRunner.Run(() =>
            {
                var window = OpenWith(3);
                SetTargetFondos(window, "2,3");
                CommitBox(window, "FondoBox", "60");
                var design = window.BuildDesignForTest(out var error);
                Assert.Null(error);
                return new[] { design.PalletDepth, design.ExtraFondoDepths[0], design.ExtraFondoDepths[1] };
            });

            Assert.Equal(new[] { 48.0, 60.0, 60.0 }, depths);
        }

        // ---- 2. The All reach of the two frente-wide flags ----

        [Fact]
        public void ThereIsStillExactlyOneTargetFondoSelector()
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
