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
    /// I-42 (A3-GATE-LIBRARY, contrato del dueño) — GUARDAR EN LA BIBLIOTECA TAMBIEN ES UNA SALIDA.
    ///
    /// <para>
    /// A3-GATE cerro la carrera de Insertar, Actualizar y el BOM: los tres confirman la edicion escenificada y
    /// RECALCULAN dentro del click, asi que el veredicto que vale es el de despues. «Guardar en biblioteca» se quedo
    /// con la forma vieja —recalcular y mirar solo <c>currentInputsAreValid</c>—, de modo que un diseño que pasaba a
    /// Bloqueante justo en ese recalculo llegaba igualmente al disco. Medido: con la estructura del lado A congelada
    /// en 8 fondos y «Fondos = 12» tecleado sin salir del campo, el estado antes del click era valido y no
    /// bloqueado, y el click llegaba a abrir el dialogo de guardado con el diagnostico «la cama necesita 588" y la
    /// estructura efectiva solo ofrece 396"» ya vigente.
    /// </para>
    ///
    /// <para>
    /// No hay regla nueva: se consulta LA MISMA compuerta —<c>outputIsBlocked</c>, releida por
    /// <c>OutputBlockedAfterRecompute</c>—, la que ya gobierna las otras tres salidas. Un archivo es persistente, asi
    /// que la unica diferencia es el consumidor.
    /// </para>
    /// </summary>
    public class PushBackLibrarySaveGateTests
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

        /// <summary>Un compuesto valido con la estructura del lado A congelada, el fixture natural de A3-GATE.</summary>
        private static RackPushBackSystemWindow Composite(double structure = 8.0, double? corridaDepth = null)
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
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

        /// <summary>La edicion ESCENIFICADA: se teclea y NO se sale del campo, asi que solo el click la confirma.</summary>
        private static void StageBlockingEdit(RackPushBackSystemWindow window)
            => Field(window, "FondosBox").SetNumber(12.0);

        /// <summary>
        /// Pulsa «Guardar en biblioteca» y dice si el click llego al DIALOGO de guardado. En el banco la ventana no
        /// esta mostrada, asi que abrir el dialogo lanza: esa excepcion es justo la señal de que la salida siguio su
        /// curso. Detenerse en la compuerta no lanza nada.
        /// </summary>
        private static bool ReachedTheSaveDialog(RackPushBackSystemWindow window)
            => Record.Exception(() => Click(Button(window, "SaveLibraryButton"))) != null;

        private static void AssertValidAndUnblockedBeforeTheClick(RackPushBackSystemWindow window)
        {
            Assert.True(window.CurrentInputsAreValid);
            Assert.False(window.OutputIsBlockedForTest);
            Assert.NotNull(window.BuildLibraryProjectForTest());
        }

        // ---------------------------------------------------------------- la compuerta

        [Fact]
        public void SaveLibrary_Click_RechecksBlockingAfterCommittedRecompute()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite();
                var closed = false;
                window.Closed += (s, e) => closed = true;
                StageBlockingEdit(window);
                AssertValidAndUnblockedBeforeTheClick(window);

                var reached = ReachedTheSaveDialog(window);

                Assert.False(reached);                              // no se pidio guardar nada
                Assert.Null(window.BuildLibraryProjectForTest());   // ni hay proyecto que escribir
                Assert.True(window.OutputIsBlockedForTest);
                Assert.True(window.CurrentInputsAreValid);          // las entradas nunca fueron el problema
                Assert.False(closed);                               // la ventana sigue viva
                Assert.Contains(BlockedFragment, Status(window), StringComparison.Ordinal);
            });
        }

        [Fact]
        public void SaveLibrary_Click_PostRecomputeValidDesignStillSaves()
        {
            StaTestRunner.Run(() =>
            {
                // La MISMA edicion escenificada, sobre una estructura que si la admite.
                var window = Composite();
                Field(window, "StructureOverrideBox").SetNumber(16.0);
                Click(Button(window, "ApplyStructureButton"));
                StageBlockingEdit(window);
                AssertValidAndUnblockedBeforeTheClick(window);

                var reached = ReachedTheSaveDialog(window);

                Assert.True(reached);                                // el guardado sigue su curso
                Assert.False(window.OutputIsBlockedForTest);
                Assert.NotNull(window.BuildLibraryProjectForTest());
            });
        }

        [Fact]
        public void SaveLibrary_Click_PostRecomputeWarningDoesNotBlockSave()
        {
            StaTestRunner.Run(() =>
            {
                // Estructura manual por debajo de la propuesta con celdas que aun caben: Warning, no bloqueo.
                var window = Composite(structure: 5.0, corridaDepth: 4.0);
                StageBlockingEdit(window);
                Assert.Contains("es menor que la propuesta derivada", Status(window), StringComparison.Ordinal);
                AssertValidAndUnblockedBeforeTheClick(window);

                var reached = ReachedTheSaveDialog(window);

                Assert.True(reached);
                Assert.False(window.OutputIsBlockedForTest);
            });
        }

        [Fact]
        public void SaveLibrary_Click_BlockingKeepsTheStagedEdit()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite();
                StageBlockingEdit(window);

                Assert.False(ReachedTheSaveDialog(window));

                // La edicion del usuario sigue en el campo Y en el modelo —el recalculo del click la confirmo—,
                // asi que no hay reset ni vuelta al ultimo modelo valido: se puede corregir desde donde quedo.
                Assert.Equal(12.0, Field(window, "FondosBox").Value);
                Assert.Equal(12, window.CompositeState.SideA.Structure.Fronts[0].PalletsDeep);

                // Y el bloqueo no es definitivo: se agranda la estructura y el mismo guardado sale.
                Field(window, "StructureOverrideBox").SetNumber(16.0);
                Click(Button(window, "ApplyStructureButton"));
                Assert.False(window.OutputIsBlockedForTest);
                Assert.True(ReachedTheSaveDialog(window));
            });
        }

        // ---------------------------------------------------------------- una sola compuerta para las salidas

        [Fact]
        public void SaveLibrary_AndInsertUseSamePostRecomputeBlockingAuthority()
        {
            StaTestRunner.Run(() =>
            {
                // Mismo fixture, dos salidas: si una bloqueara y la otra no, habria dos reglas.
                var insert = Composite();
                StageBlockingEdit(insert);
                Click(Button(insert, "InsertButton"));

                var library = Composite();
                StageBlockingEdit(library);
                var reached = ReachedTheSaveDialog(library);

                Assert.False(insert.Session.InsertRequested);
                Assert.False(reached);
                Assert.True(insert.OutputIsBlockedForTest);
                Assert.True(library.OutputIsBlockedForTest);
                Assert.Contains(BlockedFragment, Status(insert), StringComparison.Ordinal);
                Assert.Contains(BlockedFragment, Status(library), StringComparison.Ordinal);
            });
        }
    }
}
