using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.RackFrames;
using RackCad.UI.RackFrames;
using RackCad.UI.Shell;
using RackCad.UI.Systems.Cantilever;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// EL CONTRATO NUEVO de foco y severidad (I-39B), separado de la caracterización de la base igual que
    /// <c>RichEditorCloseContractTests</c>. Cada prueba dice a qué gemela con <c>Skip</c> reemplaza.
    /// </summary>
    public sealed class RichEditorContractTests
    {
        private static string UiSource(string relative)
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.True(dir != null, "no se encontro la raiz del repositorio");
            return File.ReadAllText(Path.Combine(dir.FullName, "src", "RackCad.UI", relative.Replace('/', Path.DirectorySeparatorChar)));
        }

        // ---- Foco inicial: reemplaza a FiveDeclareAnInitialFocusAndTheHeaderConfiguratorDoesNot ----

        [Fact]
        public void TheSixDeclareAnInitialFocus()
        {
            // ADR-0029 D9: el foco inicial es determinista. La Cabecera era la unica de las seis que no lo declaraba.
            foreach (var (relative, name) in new[]
                     {
                         ("Systems/Selective/RackSelectiveWindow.xaml", "Selectivo"),
                         ("Systems/Dynamic/RackDynamicSystemWindow.xaml", "Dinamico"),
                         ("Systems/PushBack/RackPushBackSystemWindow.xaml", "Push Back"),
                         ("Systems/Cantilever/RackCantileverWindow.xaml", "Cantilever"),
                         ("Systems/FlowBed/RackFlowBedWindow.xaml", "Cama"),
                         ("RackFrames/RackFrameConfiguratorWindow.xaml", "Cabecera")
                     })
            {
                Assert.True(
                    UiSource(relative).Contains("FocusManager.FocusedElement", StringComparison.Ordinal),
                    name + " deberia declarar foco inicial");
            }
        }

        [Fact]
        public void TheHeaderConfiguratorFocusesItsModelTreeAndNothingDestructive()
        {
            // Se comprueba la DECLARACION, no el foco efectivo: éste exige mostrar la ventana y que el Binding por
            // ElementName se resuelva durante el layout, y una aserción que dependa de eso es frágil. Es el mismo
            // criterio con el que I-39A caracterizó el foco de su piloto.
            var xaml = UiSource("RackFrames/RackFrameConfiguratorWindow.xaml");

            Assert.Contains(
                "FocusManager.FocusedElement=\"{Binding ElementName=ModelTree}\"",
                xaml,
                StringComparison.Ordinal);

            StaTestRunner.Run(() =>
            {
                // Y que el destino es el árbol del modelo —el control principal de la ventana—, no una acción:
                // D9 prohíbe que el foco inicial caiga en algo destructivo o bloqueado.
                var window = new RackFrameConfiguratorWindow(
                    new HardcodedStandardRackFrameService().CreateDefault(), canInsertInAutoCad: true);

                var target = window.FindName("ModelTree");

                Assert.NotNull(target);
                Assert.IsNotType<Button>(target);
                Assert.IsType<TreeView>(target);
            });
        }

        // ---- Severidad de Cantilever ----

        [Fact]
        public void ANonBlockingCantileverDiagnosticIsNotPaintedAsAnError()
        {
            StaTestRunner.Run(() =>
            {
                // ADR-0029: la severidad se representa como es. Los `Warnings` de la computacion son los
                // diagnosticos NO bloqueantes -- la linea SI resolvio -- y hasta I-39B se pintaban con el rojo de
                // error, indistinguibles de un fallo real.
                var status = new TextBlock();

                UiSupport.SetStatus(status, "aviso no bloqueante", EditorStatusSeverity.Warning);
                var warning = ((SolidColorBrush)status.Foreground).Color;

                UiSupport.SetStatus(status, "fallo", EditorStatusSeverity.Error);
                var error = ((SolidColorBrush)status.Foreground).Color;

                UiSupport.SetStatus(status, "todo bien", EditorStatusSeverity.Success);
                var success = ((SolidColorBrush)status.Foreground).Color;

                Assert.NotEqual(error, warning);
                Assert.NotEqual(success, warning);
            });
        }

        [Fact]
        public void TheSeverityScaleComesFromTheSharedTokensAndNotFromASecondPalette()
        {
            StaTestRunner.Run(() =>
            {
                // No se introduce una paleta nueva: se consume la del shell, que hasta I-39B no tenia ningun
                // consumidor productivo.
                var styles = new ResourceDictionary
                {
                    Source = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative)
                };
                var status = new TextBlock();

                foreach (var (severity, token) in new[]
                         {
                             (EditorStatusSeverity.Info, "ShellStatusInfoBrush"),
                             (EditorStatusSeverity.Success, "ShellStatusSuccessBrush"),
                             (EditorStatusSeverity.Warning, "ShellStatusWarningBrush"),
                             (EditorStatusSeverity.Error, "ShellStatusErrorBrush")
                         })
                {
                    UiSupport.SetStatus(status, "x", severity);

                    Assert.Equal(
                        ((SolidColorBrush)styles[token]).Color,
                        ((SolidColorBrush)status.Foreground).Color);
                }
            });
        }

        [Fact]
        public void TheCantileverEditorRoutesItsWarningsThroughTheSeverityScale()
        {
            // Guarda de fuente, con el idiom del repositorio: la llamada que pintaba un aviso como error ya no
            // existe, y en su lugar hay una que declara la severidad.
            var source = UiSource("Systems/Cantilever/RackCantileverWindow.xaml.cs");

            Assert.DoesNotContain(
                "SetStatus(Warnings(computation) ?? \"Línea recalculada.\", Warnings(computation) != null)",
                source,
                StringComparison.Ordinal);
            Assert.Contains("EditorStatusSeverity.Warning", source, StringComparison.Ordinal);
        }
    }
}
