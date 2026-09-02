using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Domain.Systems.PushBack;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42 (A3-GATE, contrato del dueño) — LA AUTORIDAD DE SALIDA SE RELEE DESPUES DEL RECALCULO DEL CLICK.
    ///
    /// <para>
    /// Insertar, Actualizar y el BOM no leen la pantalla: confirman la edicion escenificada y RECALCULAN dentro del
    /// propio click. Ese recalculo vuelve a decidir si el sistema resuelto esta bloqueado, asi que el veredicto que
    /// vale es el de despues. Hasta esta ronda los tres solo miraban <c>currentInputsAreValid</c> —que habla de las
    /// ENTRADAS: «12 fondos» es un numero perfectamente valido— y dibujaban igual un sistema que acababa de quedar
    /// bloqueado. El boton deshabilitado no protegia nada, porque se deshabilita con el veredicto ANTERIOR.
    /// </para>
    ///
    /// <para>
    /// El fixture es natural, no inventado: la estructura del lado A se congela en 8 fondos —valida, sin aviso— y el
    /// usuario teclea «Fondos = 12» y pulsa el boton SIN salir del campo. La celda pasa a pedir 588" sobre 396"
    /// disponibles. Antes del click: valido y no bloqueado. Durante el click: bloqueado.
    /// </para>
    ///
    /// <para>
    /// La regla es de BLOQUEO, no de aviso: un diagnostico Warning describe una consecuencia que el usuario ya
    /// acepto y no detiene ninguna salida.
    /// </para>
    /// </summary>
    public class PushBackOutputGateAfterRecomputeTests
    {
        private const string BlockedFragment = "la estructura efectiva solo ofrece";

        private static ComboBox Combo(RackPushBackSystemWindow window, string name)
            => (ComboBox)window.FindName(name);

        private static CheckBox Check(RackPushBackSystemWindow window, string name)
            => (CheckBox)window.FindName(name);

        private static NumericField Field(RackPushBackSystemWindow window, string name)
            => (NumericField)window.FindName(name);

        private static Button Button(RackPushBackSystemWindow window, string name)
            => (Button)window.FindName(name);

        private static string Status(RackPushBackSystemWindow window)
            => ((TextBlock)window.FindName("StatusText")).Text;

        private static void LoseFocus(FrameworkElement element)
            => element.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element));

        private static void Click(ButtonBase button)
            => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));

        /// <summary>Un compuesto A + hueco + B, valido, con la estructura del lado A congelada en <paramref name="structure"/>.</summary>
        private static RackPushBackSystemWindow Composite(double structure = 8.0, bool editingExisting = false, double? corridaDepth = null)
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            if (editingExisting)
            {
                // Solo un sistema abierto con RACKEDITAR ofrece «Actualizar».
                window.LoadExisting(window.LastComputation.Design, "GUID-A3-GATE", "PB");
            }

            Check(window, "SideBPresentCheck").IsChecked = true;
            var matrix = window.CompositeState.Of(PushBackSide.B).Structure;
            for (var front = 0; front < matrix.Count; front++)
            {
                matrix.Fronts[front].IsActive = true;
            }

            var gap = Field(window, "GapBox");
            gap.SetNumber(window.CompositeState.Gap);
            LoseFocus(gap);

            if (corridaDepth.HasValue)
            {
                Combo(window, "CellPropertyScopeBox").SelectedIndex = 4; // Todo
                var cell = Field(window, "CellFondoOverrideBox");
                cell.SetNumber(corridaDepth.Value);
                Click(Button(window, "ApplyCellFondoButton"));
            }

            var frozen = Field(window, "StructureOverrideBox");
            frozen.SetNumber(structure);
            Click(Button(window, "ApplyStructureButton"));
            return window;
        }

        /// <summary>La edicion ESCENIFICADA: se teclea en el campo y NO se sale de el, asi que solo llega al modelo
        /// cuando el propio click la confirma y recalcula.</summary>
        private static void StageBlockingEdit(RackPushBackSystemWindow window)
            => Field(window, "FondosBox").SetNumber(12.0);

        private static void AssertValidAndUnblockedBeforeTheClick(RackPushBackSystemWindow window)
        {
            Assert.True(window.CurrentInputsAreValid);
            Assert.False(window.OutputIsBlockedForTest);
        }

        // ---------------------------------------------------------------- los tres caminos de salida

        [Fact]
        public void Insert_Click_RechecksBlockingAfterCommittedRecompute()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite();
                var closed = false;
                window.Closed += (s, e) => closed = true;
                StageBlockingEdit(window);
                AssertValidAndUnblockedBeforeTheClick(window);
                var passes = window.RecomputePassesForTest;

                Click(Button(window, "InsertButton"));

                // El recalculo SI ocurrio dentro del click, y dejo el sistema bloqueado.
                Assert.True(window.RecomputePassesForTest > passes);
                Assert.True(window.OutputIsBlockedForTest);
                Assert.True(window.CurrentInputsAreValid); // las entradas nunca fueron el problema
                Assert.False(window.Session.InsertRequested);
                Assert.False(closed);
                Assert.Contains(BlockedFragment, Status(window), StringComparison.Ordinal);
            });
        }

        [Fact]
        public void Update_Click_RechecksBlockingAfterCommittedRecompute()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite(editingExisting: true);
                var closed = false;
                window.Closed += (s, e) => closed = true;
                StageBlockingEdit(window);
                AssertValidAndUnblockedBeforeTheClick(window);

                Click(Button(window, "UpdateButton"));

                Assert.True(window.OutputIsBlockedForTest);
                Assert.False(window.Session.InsertRequested);
                Assert.False(window.Session.UpdateOnly);
                Assert.False(closed);
                Assert.Contains(BlockedFragment, Status(window), StringComparison.Ordinal);
            });
        }

        [Fact]
        public void Bom_Click_RechecksBlockingAfterCommittedRecompute()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite();
                StageBlockingEdit(window);
                AssertValidAndUnblockedBeforeTheClick(window);

                // Con el defecto, el click llegaba a abrir la ventana del BOM del sistema bloqueado.
                var error = Record.Exception(() => Click(Button(window, "BomButton")));

                Assert.Null(error);
                Assert.True(window.OutputIsBlockedForTest);
                Assert.Contains(BlockedFragment, Status(window), StringComparison.Ordinal);
            });
        }

        // ---------------------------------------------------------------- la puerta no se cierra de mas

        [Fact]
        public void Insert_Click_PostRecomputeValidDesignStillRequestsInsert()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite();

                // La MISMA edicion escenificada, sobre una estructura que si la admite: la salida debe salir.
                Field(window, "StructureOverrideBox").SetNumber(16.0);
                Click(Button(window, "ApplyStructureButton"));
                StageBlockingEdit(window);
                AssertValidAndUnblockedBeforeTheClick(window);

                Click(Button(window, "InsertButton"));

                Assert.False(window.OutputIsBlockedForTest);
                Assert.True(window.Session.InsertRequested);
                Assert.NotNull(window.Session.InsertionRequest);
            });
        }

        [Fact]
        public void Insert_Click_WarningAfterRecomputeDoesNotStopTheOutput()
        {
            StaTestRunner.Run(() =>
            {
                // Estructura manual por debajo de la propuesta, con celdas que aun caben: Warning, no bloqueo.
                var window = Composite(structure: 5.0, corridaDepth: 4.0);
                StageBlockingEdit(window);
                AssertValidAndUnblockedBeforeTheClick(window);
                Assert.Contains("es menor que la propuesta derivada", Status(window), StringComparison.Ordinal);

                Click(Button(window, "InsertButton"));

                Assert.False(window.OutputIsBlockedForTest);
                Assert.True(window.Session.InsertRequested);
            });
        }

        [Fact]
        public void Blocked_Click_KeepsTheStagedEditAndInsertsOnceElBloqueoSeCorrige()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite();
                StageBlockingEdit(window);
                Click(Button(window, "InsertButton"));
                Assert.False(window.Session.InsertRequested);

                // La edicion del usuario sigue en el campo: no hay que volver a teclearla.
                Assert.Equal(12.0, Field(window, "FondosBox").Value);

                // Y el bloqueo no es definitivo: se agranda la estructura y la misma salida sale.
                Field(window, "StructureOverrideBox").SetNumber(16.0);
                Click(Button(window, "ApplyStructureButton"));
                Assert.False(window.OutputIsBlockedForTest);

                Click(Button(window, "InsertButton"));

                Assert.True(window.Session.InsertRequested);
            });
        }
    }
}
