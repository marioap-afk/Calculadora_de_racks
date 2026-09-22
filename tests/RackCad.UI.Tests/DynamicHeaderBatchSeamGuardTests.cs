using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using RackCad.UI.Systems.Dynamic;
using Xunit;
using D = RackCad.UI.Tests.DynamicHeaderBatchTestSupport;
using W = RackCad.UI.Tests.DynamicHeaderReuseWindowTests;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53D, G7 — D-34 (Proposal V2 §13.4): el configurador, el informe del lote y el de la reconstruccion pasan por
    /// COSTURAS o por superficies de la propia ventana, nunca por un modal real. Una prueba que abriera un modal colgaria el
    /// hilo STA compartido de la suite, y un dialogo que no se puede sustituir es un dialogo que no se puede probar.
    /// <para>
    /// Guarda de fuentes: I-55 G10 retira el modal historico de primera vista; permanecen cinco <c>ShowDialog</c>
    /// (seguridad, configurador, exportar BOM, lista de materiales, abrir proyecto), y los
    /// gestos nuevos no abren ninguno y el configurador se presenta detras de la costura, con su <c>ShowDialog</c> como camino
    /// de produccion.
    /// </para>
    /// </summary>
    public sealed class DynamicHeaderBatchSeamGuardTests
    {
        [Fact]
        public void D34_GUARD_ElCensoDeModalesDirectosNoCrece_YLosGestosNuevosNoAbrenNinguno()
        {
            var source = W.Source("RackDynamicSystemWindow.xaml.cs");

            Assert.Empty(Regex.Matches(source, @"MessageBox\.Show\(").Cast<Match>());
            Assert.Equal(5, Regex.Matches(source, @"\.ShowDialog\(").Count);

            foreach (var signature in new[]
                     {
                         "private void EditHeader_Click(",
                         "private void TakeHeaderSource_Click(",
                         "private void ApplyHeaderBatch_Click(",
                         "private void RunHeaderBatch(",
                         "private void ReportHeaderBatch(",
                         "private void RefreshModuleTargets(",
                         "private void RefreshHeaderSource(",
                         "private void ShowRebuildReport(",
                         "private void ConfigBox_SelectionChanged(",
                     })
            {
                var body = W.BodyOf(source, signature);
                Assert.DoesNotContain("MessageBox", body, StringComparison.Ordinal);
                Assert.DoesNotContain("ShowDialog(", body, StringComparison.Ordinal);
                Assert.DoesNotContain("new Window", body, StringComparison.Ordinal);
            }
        }

        [Fact]
        public void D34_GUARD_ElConfiguradorSePresentaDetrasDeLaCostura_YSeLeeSuResultadoReal()
        {
            var source = W.Source("RackDynamicSystemWindow.xaml.cs");

            var presenter = W.BodyOf(source, "private RackFrameConfiguration ShowHeaderConfigurator(");
            Assert.Contains("HeaderConfiguratorPresenter", presenter, StringComparison.Ordinal);
            Assert.Contains("ViewModel.IsAdvancedEditor", presenter, StringComparison.Ordinal);
            Assert.Contains("ShowDialog(", presenter, StringComparison.Ordinal);    // produccion: presenta la ventana
            Assert.Contains("window.Configuration", presenter, StringComparison.Ordinal);

            // El gesto de editar no crea ni muestra el configurador por su cuenta: lo pide a la costura, sobre una copia.
            var edit = W.BodyOf(source, "private void EditHeader_Click(");
            Assert.Contains("ShowHeaderConfigurator(", edit, StringComparison.Ordinal);
            Assert.DoesNotContain("new RackFrameConfiguratorWindow(", edit, StringComparison.Ordinal);
            Assert.Contains("Clone(", edit, StringComparison.Ordinal);
            Assert.Contains("DynamicHeaderBatchRequest.Edit(", edit, StringComparison.Ordinal);
        }

        [Fact]
        public void D34_ElInformeDelLoteYElDeLaReconstruccion_SonTextosDeLaVentana_EnLaBandaDeEstado()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad: true);
                var shell = window.Shell;
                foreach (var name in new[] { "StatusText", "RebuildReportText" })
                {
                    var element = window.FindName(name) as DependencyObject;
                    Assert.True(element != null, "No existe " + name + ".");
                    Assert.True(IsInLogicalSubtree((DependencyObject)shell.StatusContent, element), name + " no esta en la banda de estado del shell");
                }

                // Y la ventana del Dinamico no declara ninguna ventana hija nueva para ellos.
                Assert.DoesNotContain(
                    typeof(RackDynamicSystemWindow).Assembly.GetTypes(),
                    type => typeof(Window).IsAssignableFrom(type)
                            && type.Namespace == typeof(RackDynamicSystemWindow).Namespace
                            && type != typeof(RackDynamicSystemWindow));
            });
        }

        [Fact]
        public void D34_UnGestoCompleto_ConConfiguradorYReconstruccion_NoAbreNingunModal()
        {
            // Si algun paso abriera un modal real, esta prueba no terminaria: el hilo STA quedaria en su bucle.
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                window.HeaderConfiguratorPresenter = D.Edit(configuration => configuration.PanelClear = 42.25);
                D.EditHeader(window);
                D.TakeSource(window);
                D.TargetsAll(window);
                D.Apply(window);
                D.ChangePalletDepth(window, 52.0);
                D.RestoreLayout(window);
                return (Log: window.HeaderBatchLog.Count, Report: D.RebuildReport(window));
            });

            Assert.True(r.Log > 0);
        }

        private static bool IsInLogicalSubtree(DependencyObject root, DependencyObject target)
        {
            if (ReferenceEquals(root, target))
            {
                return true;
            }

            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                if (child is DependencyObject node && IsInLogicalSubtree(node, target))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
