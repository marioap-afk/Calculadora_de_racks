using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43, gate 8.6H (R2-05 y R2-08): la cobertura que faltaba y que dejó pasar los bloqueadores.
    /// <para>
    /// Un commit puede tocar varios campos y cada uno puede tener algo que decir; quedarse con el último aviso
    /// esconde justo el que el usuario no esperaba. Y Enter tiene que comprometer igual que salir del campo, porque
    /// O-43-02 lo declara pero ninguna prueba lo miraba.
    /// </para>
    /// </summary>
    public sealed class SelectiveCommitCoverageTests
    {
        private static TextBox Box(RackSelectiveWindow window, string name) => (TextBox)window.FindName(name);

        private static ComboBox FondoSelector(RackSelectiveWindow window) => (ComboBox)window.FindName("FondoSelectorBox");

        private static void Type(RackSelectiveWindow window, string name, string text) => Box(window, name).Text = text;

        private static void LostFocus(RackSelectiveWindow window, string name)
        {
            var box = Box(window, name);
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }

        /// <summary>Pulsar Enter DE VERDAD sobre el campo, que es el otro gesto que O-43-02 declara.</summary>
        private static void PressEnter(RackSelectiveWindow window, string name)
        {
            var box = Box(window, name);
            box.RaiseEvent(new KeyEventArgs(
                Keyboard.PrimaryDevice,
                new System.Windows.Interop.HwndSource(0, 0, 0, 0, 0, "t", System.IntPtr.Zero),
                0,
                Key.Enter)
            { RoutedEvent = UIElement.KeyDownEvent });
        }

        private static double[] Depths(RackSelectiveWindow window)
            => window.EditorState.FondoMatrices.Select(m => m.Depth).ToArray();

        private static double[] Overrides(RackSelectiveWindow window)
            => window.EditorState.FondoMatrices.Select(m => m.CabeceraOverride).ToArray();

        // =====================================================================================
        // R2-08 — Enter compromete igual que salir del campo (O-43-02)
        // =====================================================================================

        [Fact]
        public void R2_08_EnterOnThePalletDepth_Commits()
        {
            var depths = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                SelectiveTargetsTestSupport.SetAllTargets(window);
                Type(window, "FondoBox", "60");
                PressEnter(window, "FondoBox");
                return Depths(window);
            });

            Assert.Equal(new[] { 60.0, 60.0 }, depths);
        }

        [Fact]
        public void R2_08_EnterOnTheCabeceraDepth_Commits()
        {
            var overrides = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                SelectiveTargetsTestSupport.SetAllTargets(window);
                Type(window, "CabeceraFondoBox", "37");
                PressEnter(window, "CabeceraFondoBox");
                return Overrides(window);
            });

            Assert.Equal(new[] { 37.0, 37.0 }, overrides);
        }

        [Fact]
        public void R2_08_EnterWithoutEditing_ChangesNothing()
        {
            // La otra mitad: Enter es un gesto de commit, no una orden de aplicar (O-43-02).
            var (depths, recomputes) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                SelectiveTargetsTestSupport.SetAllTargets(window);
                var before = window.RecomputeCount;
                PressEnter(window, "FondoBox");
                return (Depths(window), window.RecomputeCount - before);
            });

            Assert.Equal(new[] { 48.0, 48.0 }, depths);
            Assert.Equal(0, recomputes);
        }

        // =====================================================================================
        // R2-08 — cambio de fondo con «Número de fondos» sucio, válido e inválido
        // =====================================================================================

        [Fact]
        public void R2_08_NavigatingWithAValidDirtyFondoCount_CommitsItFirst()
        {
            var (count, selected) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                Type(window, "FondosBox", "3");
                FondoSelector(window).SelectedIndex = 1;
                return (window.EditorState.FondoCount, window.EditorState.SelectedFondo);
            });

            Assert.Equal(3, count);    // el pendiente se comprometió
            Assert.Equal(1, selected); // y la navegación se completó
        }

        [Fact]
        public void R2_08_NavigatingWithAnInvalidDirtyFondoCount_AbortsAndKeepsTheText()
        {
            var (count, selected, text, comboIndex) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                Type(window, "FondosBox", "x");
                FondoSelector(window).SelectedIndex = 1;
                return (
                    window.EditorState.FondoCount,
                    window.EditorState.SelectedFondo,
                    Box(window, "FondosBox").Text,
                    FondoSelector(window).SelectedIndex);
            });

            Assert.Equal(2, count);
            Assert.Equal(0, selected);   // no se movió
            Assert.Equal("x", text);     // sin Show sobre el campo sucio (INV-14)
            Assert.Equal(0, comboIndex); // y el combo volvió
        }

        // =====================================================================================
        // R2-05 — los avisos de un commit multi-campo se acumulan
        // =====================================================================================

        [Fact]
        public void R2_05_AMultiFieldCommit_KeepsEveryWarning_NotOnlyTheLast()
        {
            // Dos campos que escriben sobre varios fondos: cada uno describe su alcance. Quedarse con el último
            // escondía el otro.
            var status = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(3);
                SelectiveTargetsTestSupport.SetAllTargets(window);

                Type(window, "FondoBox", "60");
                Type(window, "CabeceraFondoBox", "37");
                EditorWindowTestSupport.ClickByContent(window, "Recalcular tramo");

                return ((TextBlock)window.FindName("StatusText")).Text ?? string.Empty;
            });

            Assert.Contains("tarima", status);   // el aviso del fondo de tarima
            Assert.Contains("cabecera", status); // Y el de la cabecera, no solo uno
        }

        [Fact]
        public void R2_05_TheSameWarningIsNotRepeated()
        {
            var status = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(3);
                SelectiveTargetsTestSupport.SetAllTargets(window);
                Type(window, "FondoBox", "60");
                Type(window, "CabeceraFondoBox", "37");
                EditorWindowTestSupport.ClickByContent(window, "Recalcular tramo");
                return ((TextBlock)window.FindName("StatusText")).Text ?? string.Empty;
            });

            var first = status.IndexOf("tarima", System.StringComparison.Ordinal);
            Assert.True(first >= 0);
            Assert.Equal(first, status.LastIndexOf("tarima", System.StringComparison.Ordinal)); // una sola vez
        }
    }
}
