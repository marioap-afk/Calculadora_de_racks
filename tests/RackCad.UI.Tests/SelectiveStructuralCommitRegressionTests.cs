using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.Selective;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43, gate 8.6H: las tres regresiones que la segunda revisión arquitectónica encontró en los gestos
    /// ESTRUCTURALES («Número de fondos» y «Frentes»).
    /// <para>
    /// R2-01 — encoger el número de fondos estando en uno que desaparece pisaba la matriz de un superviviente.
    /// R2-02 — el índice de destino del combo se capturaba ANTES del commit, así que un commit que encoge la lista
    /// lo dejaba fuera de rango.
    /// R2-03 — un gesto estructural podía comprometer una celda editada y no recalcular nada, dejando la preview y
    /// el modelo resuelto describiendo un estado que ya no existe.
    /// </para>
    /// </summary>
    public sealed class SelectiveStructuralCommitRegressionTests
    {
        private static TextBox Box(RackSelectiveWindow window, string name) => (TextBox)window.FindName(name);

        private static ComboBox FondoSelector(RackSelectiveWindow window) => (ComboBox)window.FindName("FondoSelectorBox");

        private static void Type(RackSelectiveWindow window, string name, string text) => Box(window, name).Text = text;

        private static void LostFocus(RackSelectiveWindow window, string name)
        {
            var box = Box(window, name);
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }

        private static void TypeAndLeave(RackSelectiveWindow window, string name, string text)
        {
            Type(window, name, text);
            LostFocus(window, name);
        }

        private static void Show(RackSelectiveWindow window, int oneBased) => FondoSelector(window).SelectedIndex = oneBased - 1;

        /// <summary>Frentes comprometidos de cada fondo (matriz viva para el visible, slot para el resto).</summary>
        private static int[] Counts(RackSelectiveWindow window)
        {
            var state = window.EditorState;
            return Enumerable.Range(0, state.FondoCount)
                .Select(k => k == state.SelectedFondo ? state.Bays.Count : state.FondoMatrices[k].Bays.Count)
                .ToArray();
        }

        /// <summary>Da a UN fondo su propio número de frentes, sin tocar los demás.</summary>
        private static void SetFrentesOfFondo(RackSelectiveWindow window, int oneBased, int frentes)
        {
            Show(window, oneBased);
            SelectiveTargetsTestSupport.SetTargets(window, oneBased);
            TypeAndLeave(window, "BayCountBox", frentes.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Un rack de 3 fondos con contenido distinguible: 2, 3 y 4 frentes.</summary>
        private static RackSelectiveWindow ThreeDistinctFondos()
        {
            var window = SelectiveWindowTestSupport.Open(3);
            SetFrentesOfFondo(window, 1, 2);
            SetFrentesOfFondo(window, 2, 3);
            SetFrentesOfFondo(window, 3, 4);
            return window;
        }

        // =====================================================================================
        // R2-01 — encoger fondos NO puede pisar la matriz de un superviviente
        // =====================================================================================

        [Fact]
        public void R2_01_ShrinkingToOneFondoFromTheLastOne_LeavesTheSurvivorUntouched()
        {
            var counts = StaTestRunner.Run(() =>
            {
                var window = ThreeDistinctFondos();
                Show(window, 3);                                 // se está viendo el fondo que va a desaparecer
                TypeAndLeave(window, "FondosBox", "1");
                return Counts(window);
            });

            Assert.Equal(new[] { 2 }, counts); // el fondo 1 conserva SUS 2 frentes, no los 4 del fondo 3
        }

        [Fact]
        public void R2_01_ShrinkingToTwoFondosFromTheThird_KeepsBothSurvivors()
        {
            var counts = StaTestRunner.Run(() =>
            {
                var window = ThreeDistinctFondos();
                Show(window, 3);
                TypeAndLeave(window, "FondosBox", "2");
                return Counts(window);
            });

            Assert.Equal(new[] { 2, 3 }, counts); // cada superviviente con lo suyo; el fondo 3 desaparece
        }

        [Fact]
        public void R2_01_ShrinkingWithAPendingFrenteCount_DoesNotStampTheLiveMatrixOnTheWrongSlot()
        {
            // Los dos pendientes juntos: primero la estructura de fondos, luego los frentes sobre los destinos ya
            // resueltos (INV-17). El fondo superviviente no puede recibir la matriz del que se fue.
            var counts = StaTestRunner.Run(() =>
            {
                var window = ThreeDistinctFondos();
                Show(window, 3);
                SelectiveTargetsTestSupport.SetTargets(window, 1);
                Type(window, "FondosBox", "1");
                TypeAndLeave(window, "BayCountBox", "5");
                return Counts(window);
            });

            Assert.Equal(new[] { 5 }, counts); // 5 por la edición explícita, no 4 heredados del fondo 3
        }

        // =====================================================================================
        // R2-02 — el destino del combo se revalida DESPUÉS del commit
        // =====================================================================================

        [Fact]
        public void R2_02_NavigatingWithAPendingShrink_DoesNotUseAStaleIndex()
        {
            var (threw, selected, comboIndex, count) = StaTestRunner.Run(() =>
            {
                var window = ThreeDistinctFondos();
                Show(window, 1);
                Type(window, "FondosBox", "1"); // pendiente: el commit dejará UN solo fondo

                string error = null;
                try
                {
                    Show(window, 2); // el destino 2 dejará de existir en cuanto se comprometa
                }
                catch (Exception ex)
                {
                    error = ex.GetType().Name + ": " + ex.Message;
                }

                return (error, window.EditorState.SelectedFondo, FondoSelector(window).SelectedIndex, window.EditorState.FondoCount);
            });

            Assert.Null(threw);                       // ninguna excepción
            Assert.Equal(1, count);                   // el commit sí ocurrió
            Assert.InRange(selected, 0, count - 1);   // y el fondo seleccionado quedó dentro de rango
            Assert.InRange(comboIndex, 0, count - 1); // igual que el combo
        }

        [Fact]
        public void R2_02_NavigatingWithAPendingGrow_ReachesTheRequestedFondo()
        {
            // La otra mitad: si tras el commit el destino SÍ existe, la navegación tiene que completarse.
            var (selected, count) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                Show(window, 1);
                Type(window, "FondosBox", "4"); // pendiente: habrá 4 fondos
                Show(window, 3);                // destino que solo existe DESPUÉS del commit
                return (window.EditorState.SelectedFondo, window.EditorState.FondoCount);
            });

            Assert.Equal(4, count);
            Assert.Equal(2, selected); // llegó al fondo 3 (índice 2)
        }

        // =====================================================================================
        // R2-03 — una celda comprometida por un gesto estructural obliga a recalcular
        // =====================================================================================

        [Fact]
        public void R2_03_AStructuralGestureThatCommitsACell_Recomputes()
        {
            var (recomputes, frente) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                window.EditorState.SelectCell(0, 0, extend: false);
                window.LoadCellEditorForTest();

                Type(window, "FrenteBox", "55"); // celda editada, SIN pulsar ningún botón de alcance

                var before = window.RecomputeCount;
                LostFocus(window, "BayCountBox"); // gesto estructural con la caja LIMPIA
                return (window.RecomputeCount - before, window.EditorState.CellAt(new SelectiveCellAddress(0, 0, 0)).Frente);
            });

            Assert.Equal(55.0, frente);  // la celda sí se comprometió
            Assert.Equal(1, recomputes); // así que el modelo resuelto tiene que haberse rehecho
        }

        [Fact]
        public void R2_03_AStructuralGestureThatCommitsNothing_StillDoesNotRecompute()
        {
            // La otra mitad del contrato: sin mutación real sigue sin haber recompute (O-43-02).
            var recomputes = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                window.EditorState.SelectCell(0, 0, extend: false);
                window.LoadCellEditorForTest();

                var before = window.RecomputeCount;
                LostFocus(window, "BayCountBox");
                return window.RecomputeCount - before;
            });

            Assert.Equal(0, recomputes);
        }

        [Fact]
        public void R2_03_TheFondoCountGestureThatCommitsACell_AlsoRecomputes()
        {
            var (recomputes, frente) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                window.EditorState.SelectCell(0, 0, extend: false);
                window.LoadCellEditorForTest();

                Type(window, "FrenteBox", "51");

                var before = window.RecomputeCount;
                LostFocus(window, "FondosBox"); // caja limpia: el único cambio real es la celda
                return (window.RecomputeCount - before, window.EditorState.CellAt(new SelectiveCellAddress(0, 0, 0)).Frente);
            });

            Assert.Equal(51.0, frente);
            Assert.Equal(1, recomputes);
        }
    }
}
