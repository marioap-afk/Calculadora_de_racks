using System.Linq;
using System.Windows.Controls;
using RackCad.Application.Drawing;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Systems.Selective;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43, gate 8.6F (O-43-03) en la VENTANA real: la preview dibuja el fondo VISIBLE, así que la cabecera que
    /// pinta en un poste tiene que ser la custom de <c>(fondo visible, poste)</c> — no la del fondo 0 ni la estándar.
    /// </summary>
    public sealed class SelectivePreviewCabeceraTests
    {
        private static RackFrameConfiguration Custom(RackSelectiveWindow window, double height)
            => new RackFrameConfigurationFactory(window.Session.Catalog).Build(
                RackFrameTemplateCatalog.FindStandardOrDefault(),
                "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA",
                height,
                42.0);

        /// <summary>Las alturas (LONGITUD) de los postes que la preview acaba de dibujar, de izquierda a derecha.</summary>
        private static double[] PreviewPostHeights(RackSelectiveWindow window)
            => (window.PreviewInstancesForTest ?? Enumerable.Empty<HeaderBlockInstance>())
                .Where(i => i.Role == HeaderBlockRole.Post)
                .OrderBy(i => i.Insertion.X)
                .Select(i => i.DynamicParameters.TryGetValue(SelectiveRackDefaults.LengthParam, out var l) ? l : 0.0)
                .ToArray();

        [Fact]
        public void O_ThePreviewOfAFondoDrawsItsOwnCustomCabecera()
        {
            var (onFondoTwo, onFondoOne) = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                var combo = (ComboBox)window.FindName("FondoSelectorBox");

                // Una custom SOLO en el fondo 2, en el poste 1.
                combo.SelectedIndex = 1;
                SelectiveTargetsTestSupport.SetTargets(window, 2);
                window.EditorState.SyncPostCabeceras();
                window.EditorState.ApplyCabeceraToTargets(1, Custom(window, 300.0), c => c);

                combo.SelectedIndex = 0;   // fuerza un recompute con el fondo 1 visible
                combo.SelectedIndex = 1;   // y vuelve al fondo 2
                var two = PreviewPostHeights(window);

                combo.SelectedIndex = 0;
                return (two, PreviewPostHeights(window));
            });

            Assert.Contains(300.0, onFondoTwo);        // el fondo VISIBLE es el 2: su poste 1 mide 300
            Assert.DoesNotContain(300.0, onFondoOne);  // el fondo 1 no tiene esa custom y no la hereda
        }

        [Fact]
        public void O_ThePreviewOfTheFirstFondoKeepsUsingItsOwnCustom()
        {
            // La otra mitad: corregir la vista no puede romper el caso que ya funcionaba.
            var heights = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                SelectiveTargetsTestSupport.SetTargets(window, 1);
                window.EditorState.SyncPostCabeceras();
                window.EditorState.ApplyCabeceraToTargets(1, Custom(window, 250.0), c => c);

                var combo = (ComboBox)window.FindName("FondoSelectorBox");
                combo.SelectedIndex = 1;
                combo.SelectedIndex = 0;
                return PreviewPostHeights(window);
            });

            Assert.Contains(250.0, heights);
        }
    }
}
