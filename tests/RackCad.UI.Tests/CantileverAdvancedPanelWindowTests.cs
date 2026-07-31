using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Domain.Systems.Cantilever;
using RackCad.UI.Systems.Cantilever;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-37D ronda 4, frente B — la TABLA avanzada de paneles en la ventana REAL.
    ///
    /// <para>Se conduce por su propia superficie de eventos: cambiar el combo levanta el
    /// <c>SelectionChanged</c> de la ventana, y cada botón es un <c>Button</c> cuyo <c>Click</c> corre el
    /// manejador de verdad. Lo que se prueba es la ventana que se opera, no un atajo por detrás.</para>
    ///
    /// <para>La confirmación de «volver a automático» entra por su costura y no por un <c>MessageBox</c>: un
    /// modal dentro de un manejador colgaría justo la prueba del camino que hay que probar.</para>
    /// </summary>
    public sealed class CantileverAdvancedPanelWindowTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string BaseW = "AISC-W-W12X26";
        private const string ArmHss = "AISC-HSS-RECT-HSS4X4X_250";

        private static void Configure(RackCantileverWindow window)
        {
            var template = window.Design.StationTopology.ColumnBaseTemplate;
            template.ColumnSectionId = ColumnW;
            template.Base = new CantileverBaseDesign { SectionId = BaseW, Length = 48.0 };
            template.Connection.Punches.ColumnBottomPlateEndOffset = 1.5;
            template.Connection.Punches.ColumnTopPunchOffset = 4.0;

            window.Design.DefaultArmTemplate = new CantileverArmTemplateDesign
            {
                Body = new CantileverArmBodyDesign { SectionId = ArmHss, CutLength = 36.0 },
                MountingPlate = new CantileverArmMountingPlateTemplateDesign
                {
                    VerticalPunchCount = 2,
                    VerticalEndOffset = 1.5
                }
            };

            EditorWindowTestSupport.SetNumberAndCommit(window, "StationCountBox", 3);
            EditorWindowTestSupport.SetNumberAndCommit(window, "SpacingBox", 96.0);
            EditorWindowTestSupport.SetNumberAndCommit(window, "LevelCountBox", 3);
            EditorWindowTestSupport.SetNumberAndCommit(window, "ClearHeightBox", 24.0);
        }

        private static DataGrid Grid(RackCantileverWindow w) =>
            (DataGrid)w.FindName("PanelSegmentGrid");

        private static IReadOnlyList<CantileverPanelSegmentRow> Rows(RackCantileverWindow w) =>
            Grid(w).ItemsSource.Cast<CantileverPanelSegmentRow>().ToList();

        private static void Click(RackCantileverWindow w, string name)
        {
            var button = (Button)w.FindName(name);
            button.RaiseEvent(new RoutedEventArgs(
                System.Windows.Controls.Primitives.ButtonBase.ClickEvent, button));
        }

        private static void GoAdvanced(RackCantileverWindow w) =>
            ((ComboBox)w.FindName("PanelLayoutModeBox")).SelectedIndex = 1;

        private static string ErrorText(RackCantileverWindow w)
        {
            var text = (TextBlock)w.FindName("PanelLayoutErrorText");
            return text.Visibility == Visibility.Visible ? text.Text : null;
        }

        private static RackCantileverWindow Ready()
        {
            var w = new RackCantileverWindow(canInsertInAutoCad: false);
            Configure(w);
            return w;
        }

        // ---- 1. La tabla solo existe en avanzado ----------------------------------------------------------

        [Fact]
        public void LaTablaEstaOCULTAEnAutomaticoYAparecalAlPasarAAvanzado()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Ready();
                var area = (StackPanel)w.FindName("AdvancedPanelArea");

                var hiddenAtFirst = area.Visibility != Visibility.Visible;

                GoAdvanced(w);

                return (hiddenAtFirst, area.Visibility == Visibility.Visible, Rows(w).Count);
            });

            Assert.True(r.Item1, "En automatico la tabla no debe estar: la secuencia la manda la regla.");
            Assert.True(r.Item2);

            // Y aparece MATERIALIZADA: el usuario edita lo que ya estaba viendo, no una lista en blanco.
            Assert.True(r.Item3 > 0, "Pasar a avanzado tiene que materializar la secuencia automatica.");
        }

        [Fact]
        public void LaTablaMuestraCotasAlturaDerivadaYTensores()
        {
            var rows = StaTestRunner.Run(() =>
            {
                var w = Ready();
                GoAdvanced(w);
                return Rows(w).Select(x => (x.Number, x.Y1, x.Y2, x.Height, x.Braced)).ToList();
            });

            Assert.NotEmpty(rows);

            // El ordinal se lee en base UNO, porque nadie cuenta tramos desde cero.
            Assert.Equal(1, rows[0].Number);

            // Y la altura es la resta de las dos cotas: derivada, no escrita.
            foreach (var row in rows)
            {
                var y1 = double.Parse(row.Y1, System.Globalization.CultureInfo.InvariantCulture);
                var y2 = double.Parse(row.Y2, System.Globalization.CultureInfo.InvariantCulture);
                var h = double.Parse(row.Height, System.Globalization.CultureInfo.InvariantCulture);

                Assert.Equal(y2 - y1, h, 3);
            }

            // La secuencia automatica de referencia trae paneles arriostrados.
            Assert.Contains(rows, r => r.Braced);
        }

        // ---- 2. Las acciones -----------------------------------------------------------------------------

        [Fact]
        public void AGREGARYELIMINARCambianLaCuentaDeTramos()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Ready();
                GoAdvanced(w);

                var start = Rows(w).Count;

                Click(w, "PanelAddButton");
                var afterAdd = Rows(w).Count;

                Grid(w).SelectedIndex = 0;
                Click(w, "PanelRemoveButton");

                return (start, afterAdd, Rows(w).Count);
            });

            Assert.Equal(r.Item1 + 1, r.Item2);
            Assert.Equal(r.Item1, r.Item3);
        }

        [Fact]
        public void DIVIDIRPatreElTramoYUNIRLoDevuelve()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Ready();
                GoAdvanced(w);

                var start = Rows(w).Count;

                Grid(w).SelectedIndex = 0;
                Click(w, "PanelSplitButton");
                var afterSplit = Rows(w).Count;

                // Las dos mitades heredaron el mismo arriostramiento, asi que se pueden volver a unir.
                Grid(w).SelectedIndex = 0;
                Click(w, "PanelMergeButton");

                return (start, afterSplit, Rows(w).Count, ErrorText(w));
            });

            Assert.Equal(r.Item1 + 1, r.Item2);
            Assert.Equal(r.Item1, r.Item3);
            Assert.Null(r.Item4);
        }

        [Fact]
        public void ALTERNARTensoresCambiaLaCasillaDelTramo()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Ready();
                GoAdvanced(w);

                Grid(w).SelectedIndex = 0;
                var before = Rows(w)[0].Braced;

                Click(w, "PanelToggleButton");

                return (before, Rows(w)[0].Braced);
            });

            Assert.NotEqual(r.Item1, r.Item2);
        }

        [Fact]
        public void SUBIRElPrimerTramoSeRECHAZAYElMotivoSeVeEnLaVENTANA()
        {
            // El requisito del dueño de que los errores se vean en la misma ventana. Un editor que manda a
            // buscar el motivo a otro sitio es un editor que no dice lo que pasa.
            var r = StaTestRunner.Run(() =>
            {
                var w = Ready();
                GoAdvanced(w);

                Grid(w).SelectedIndex = 0;
                Click(w, "PanelDownButton");

                return ErrorText(w);
            });

            Assert.False(string.IsNullOrWhiteSpace(r), "El rechazo tiene que verse en la ventana.");
            Assert.Contains("abajo", r, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void UNIRDosTramosDISTINTOSSeRECHAZAConSuMotivo()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Ready();
                GoAdvanced(w);

                // La secuencia automatica de esta fixture trae UN solo tramo, asi que primero hace falta un
                // vecino con quien intentar unirse.
                Click(w, "PanelAddButton");

                // Y luego se apaga el de abajo, para que deje de parecerse al de arriba.
                Grid(w).SelectedIndex = 0;
                Click(w, "PanelToggleButton");

                Grid(w).SelectedIndex = 0;
                Click(w, "PanelMergeButton");

                return (ErrorText(w), Rows(w).Count);
            });

            Assert.False(string.IsNullOrWhiteSpace(r.Item1));
            Assert.Contains("mismo", r.Item1, StringComparison.OrdinalIgnoreCase);
        }

        // ---- 3. Volver a automatico ----------------------------------------------------------------------

        [Fact]
        public void VolverAAutomaticoPREGUNTAYSiSeDiceQueNoSeQuedaEnAvanzado()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Ready();
                var asked = new List<string>();

                w.ConfirmDiscardingManualPanels = reason =>
                {
                    asked.Add(reason);
                    return false;
                };

                GoAdvanced(w);

                var box = (ComboBox)w.FindName("PanelLayoutModeBox");
                box.SelectedIndex = 0;

                var area = (StackPanel)w.FindName("AdvancedPanelArea");

                return (asked.Count, asked.FirstOrDefault(), box.SelectedIndex,
                    area.Visibility == Visibility.Visible);
            });

            Assert.Equal(1, r.Item1);
            Assert.False(string.IsNullOrWhiteSpace(r.Item2), "El aviso tiene que decir que la lista deja de mandar.");

            // Se dijo que no: la ventana se queda en avanzado y la tabla sigue ahi.
            Assert.Equal(1, r.Item3);
            Assert.True(r.Item4);
        }

        [Fact]
        public void VolverAAutomaticoConfirmadoDEVUELVELaAutoridadALaRegla()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Ready();
                w.ConfirmDiscardingManualPanels = _ => true;

                GoAdvanced(w);
                ((ComboBox)w.FindName("PanelLayoutModeBox")).SelectedIndex = 0;

                var area = (StackPanel)w.FindName("AdvancedPanelArea");

                return (w.Design.Bracing.PanelLayoutMode, area.Visibility == Visibility.Visible,
                    w.Design.Bracing.AdvancedPanelSegments.Count);
            });

            Assert.Equal(CantileverPanelLayoutMode.Automatic, r.Item1);
            Assert.False(r.Item2);

            // La lista NO se pierde: se conserva como dato dormido para poder volver sin rehacerla.
            Assert.True(r.Item3 > 0, "Volver a automatico no debe borrar el trabajo manual.");
        }

        // ---- 4. El diseño recibe lo editado --------------------------------------------------------------

        [Fact]
        public void LoEditadoLLEGAAlDiseñoQueSePersiste()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Ready();
                GoAdvanced(w);

                Grid(w).SelectedIndex = 0;
                Click(w, "PanelToggleButton");

                var bracing = w.Design.Bracing;

                return (bracing.PanelLayoutMode, bracing.AdvancedPanelSegments.Count,
                    bracing.AdvancedPanelSegments[0].BracingMode);
            });

            Assert.Equal(CantileverPanelLayoutMode.Advanced, r.Item1);
            Assert.True(r.Item2 > 0);
            Assert.Equal(CantileverPanelBracingMode.None, r.Item3);
        }
    }
}
