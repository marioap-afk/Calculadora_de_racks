using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.UI.Systems.Dynamic;
using RackCad.UI.Systems.FlowBed;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-39B: ADR-0029 D4 y D6 sobre los editores ricos — una acción que no puede producir salida válida se apaga
    /// **con su motivo visible**, y un preview que ya no corresponde a la captura actual se declara obsoleto.
    ///
    /// <para>Antes de I-39B la defensa era en tiempo de clic: el usuario pulsaba y recibía un error en la línea de
    /// estado. Estas pruebas fijan que ahora el botón se apaga y dice por qué.</para>
    /// </summary>
    public sealed class RichEditorBlockedActionTests
    {
        private static T Named<T>(Window window, string name) where T : FrameworkElement =>
            window.FindName(name) as T;

        // ---- Cama: Insertar no puede quedar habilitado sin modelo ----

        [Fact]
        public void TheFlowBedInsertActionIsEnabledWhileTheModelIsValid()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                var insert = Named<Button>(window, "InsertButton");

                Assert.NotNull(insert);
                Assert.True(insert.IsEnabled, "con la captura por defecto la cama es valida y se puede insertar");
            });
        }

        [Fact]
        public void TheFlowBedInsertActionIsBlockedWithItsReasonWhenTheCaptureIsInvalid()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                var insert = Named<Button>(window, "InsertButton");
                var laneDepth = Named<TextBox>(window, "LaneDepthBox");

                Assert.NotNull(laneDepth);

                // Una profundidad no numerica invalida la captura: ReadConfig devuelve null y no hay modelo.
                laneDepth.Text = "no-es-un-numero";
                EditorWindowTestSupport.ClickByContent(window, "Actualizar vista");

                Assert.False(insert.IsEnabled);
                Assert.Contains("no se puede insertar", (string)insert.ToolTip, StringComparison.OrdinalIgnoreCase);
                Assert.True(ToolTipService.GetShowOnDisabled(insert), "el motivo debe leerse con el boton apagado");
            });
        }

        [Fact]
        public void TheFlowBedInsertActionRecoversWhenTheCaptureIsValidAgain()
        {
            StaTestRunner.Run(() =>
            {
                // El contrato solo apaga lo que no puede producir salida valida: en cuanto vuelve a haber modelo, la
                // accion vuelve exactamente a como estaba.
                var window = new RackFlowBedWindow(canInsertInAutoCad: true);
                var insert = Named<Button>(window, "InsertButton");
                var laneDepth = Named<TextBox>(window, "LaneDepthBox");
                var original = laneDepth.Text;

                laneDepth.Text = "no-es-un-numero";
                EditorWindowTestSupport.ClickByContent(window, "Actualizar vista");
                Assert.False(insert.IsEnabled);

                laneDepth.Text = original;
                EditorWindowTestSupport.ClickByContent(window, "Actualizar vista");

                Assert.True(insert.IsEnabled);
                Assert.DoesNotContain("no se puede insertar", (string)insert.ToolTip, StringComparison.OrdinalIgnoreCase);
            });
        }

        [Fact]
        public void TheFlowBedInsertActionKeepsItsAutoCadReasonOutsideAutoCad()
        {
            StaTestRunner.Run(() =>
            {
                // Comportamiento previo a I-39B que NO cambia: fuera de AutoCAD el motivo sigue siendo ese.
                var window = new RackFlowBedWindow(canInsertInAutoCad: false);
                var insert = Named<Button>(window, "InsertButton");

                Assert.False(insert.IsEnabled);
                Assert.Contains("solo cuando la cama se abre desde AutoCAD", (string)insert.ToolTip, StringComparison.Ordinal);
            });
        }

        // ---- Dinámico: el preview obsoleto se declara y bloquea la materialización ----

        [Fact]
        public void TheDynamicDrawActionsAreAvailableWhileTheCaptureIsValid()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);

                Assert.True(Named<Button>(window, "InsertLateralButton").IsEnabled);
            });
        }

        [Fact]
        public void AStalePreviewBlocksTheDynamicDrawActionsAndSaysWhy()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var insertLateral = Named<Button>(window, "InsertLateralButton");
                var levels = Named<TextBox>(window, "PostPeralteBox");

                Assert.NotNull(levels);
                Assert.True(insertLateral.IsEnabled);

                // Captura invalida: el nucleo sale temprano SIN tocar el lienzo, asi que el dibujo anterior sigue en
                // pantalla. Esa semantica se conserva; lo que cambia es que deja de ser muda.
                levels.Text = "no-es-un-numero";
                EditorWindowTestSupport.ClickByContent(window, "Actualizar vista (recalcular)");

                Assert.False(insertLateral.IsEnabled);
                Assert.Contains("ÚLTIMO cálculo válido", (string)insertLateral.ToolTip, StringComparison.Ordinal);

                var status = Named<TextBlock>(window, "StatusText");
                Assert.Contains("ÚLTIMO cálculo válido", status.Text, StringComparison.Ordinal);
            });
        }

        // Nota: que el lienzo NO se borre al quedar obsoleto no se asevera aqui. Sin mostrar la ventana el canvas no
        // se mide y no dibuja nada, asi que la comparacion seria vacua; y mostrarla haria la prueba fragil por una
        // afirmacion que el propio codigo garantiza estructuralmente (las salidas tempranas de RecomposeCore
        // retornan sin tocar el lienzo). Lo observable —que la imagen queda MARCADA como obsoleta y que las
        // acciones que materializan se apagan— si esta cubierto arriba, y el Owner lo valida en AutoCAD.

        [Fact]
        public void TheDynamicDrawActionsRecoverWhenTheCaptureIsValidAgain()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var insertLateral = Named<Button>(window, "InsertLateralButton");
                var levels = Named<TextBox>(window, "PostPeralteBox");
                var original = levels.Text;

                levels.Text = "no-es-un-numero";
                EditorWindowTestSupport.ClickByContent(window, "Actualizar vista (recalcular)");
                Assert.False(insertLateral.IsEnabled);

                levels.Text = original;
                EditorWindowTestSupport.ClickByContent(window, "Actualizar vista (recalcular)");

                Assert.True(insertLateral.IsEnabled);
            });
        }
    }
}
