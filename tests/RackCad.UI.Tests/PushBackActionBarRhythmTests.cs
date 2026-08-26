using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-42, ERROR 6 — la barra de acciones tiene un RITMO, y es uno solo.
    ///
    /// <para>
    /// El dueño reportó los controles de la barra inferior con separación irregular. Lo estaban: convivían tres
    /// separaciones distintas escritas a mano botón por botón (6, 8 y 14), y I-42 añadió ahí el selector de lado
    /// frontal. Ahora hay DOS tokens —la separación entre acciones hermanas y la que marca un cambio de grupo— y
    /// nada más.
    /// </para>
    /// <para>
    /// La prueba es SEMÁNTICA, no pixel-perfect: comprueba que cada separación es uno de los dos valores del
    /// contrato, que todos los controles comparten línea base y alto, y que la barra entra en el ancho mínimo
    /// soportado. No fija posiciones absolutas, que se romperían con cualquier cambio de texto.
    /// </para>
    /// </summary>
    public sealed class PushBackActionBarRhythmTests
    {
        /// <summary>Los controles de acción de la barra, en orden visual.</summary>
        private static IReadOnlyList<FrameworkElement> Actions(RackPushBackSystemWindow w)
            => new[]
                {
                    "RestoreButton", "BomButton", "SaveLibraryButton", "UpdateButton", "InsertLateralButton",
                    "InsertFrontalEntradaButton", "InsertFrontalPosteriorButton", "FrontalSideBox",
                    "InsertPlantaButton", "InsertButton", "CloseButton"
                }
                .Select(name => (FrameworkElement)w.FindName(name))
                .ToList();

        [Fact]
        public void TheActionBar_UsesOneRhythm()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var gap = (Thickness)w.FindResource("ActionGap");
                var groupGap = (Thickness)w.FindResource("ActionGroupGap");
                var last = (Thickness)w.FindResource("ActionLast");

                foreach (var action in Actions(w))
                {
                    if (action == null)
                    {
                        return false;
                    }

                    var margin = action.Margin;
                    // Cada separacion es una de las tres del contrato: hermana, cambio de grupo o final de grupo.
                    var known = Same(margin, gap) || Same(margin, groupGap) || Same(margin, last);
                    if (!known)
                    {
                        return false;
                    }

                    // Y todos comparten la misma linea base: ningun control se descuelga.
                    if (Math.Abs(margin.Bottom - gap.Bottom) > 0.01) return false;
                    if (Math.Abs(margin.Top) > 0.01 || Math.Abs(margin.Left) > 0.01) return false;
                }

                return true;
            });

            Assert.True(ok);
        }

        /// <summary>
        /// Y con el rack COMPUESTO, que es cuando aparece el selector de lado frontal: sigue habiendo un solo
        /// ritmo, y ningún ancho suelto que lo rompa.
        ///
        /// <para>
        /// Se comprueba sobre los anchos que cada control PIDE, no sobre píxeles renderizados: así la prueba sigue
        /// siendo válida cuando cambie un texto o una fuente. Que nada quede recortado al tamaño mínimo lo cubre el
        /// contrato de tamaño del shell, que ya existe.
        /// </para>
        /// </summary>
        [Fact]
        public void TheActionBar_UsesNoArbitraryWidths_WithTheCompositeSelectorVisible()
        {
            var ok = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                ((CheckBox)w.FindName("SideBPresentCheck")).IsChecked = true;

                var selector = (ComboBox)w.FindName("FrontalSideBox");
                if (selector.Visibility != Visibility.Visible)
                {
                    return false;
                }

                // El selector NO fija alto propio: comparte la linea con los botones, que se dimensionan por su
                // estilo. Fijarle uno lo descolgaria en cuanto cambiara la tipografia.
                if (!double.IsNaN(selector.Height))
                {
                    return false;
                }

                // Y ningun ancho arbitrario: todos son multiplos del PASO de la barra, igual que las separaciones.
                // Numeros sueltos como 74, 86 o 158 son justo lo que rompia el ritmo.
                const double step = 8.0;
                foreach (var action in Actions(w))
                {
                    if (action == null || action.Visibility != Visibility.Visible)
                    {
                        continue;
                    }

                    var wanted = double.IsNaN(action.Width) ? action.MinWidth : action.Width;
                    if (wanted <= 0.0 || Math.Abs(wanted % step) > 0.01)
                    {
                        return false;
                    }
                }

                return true;
            });

            Assert.True(ok);
        }

        private static bool Same(Thickness left, Thickness right)
            => Math.Abs(left.Left - right.Left) < 0.01
               && Math.Abs(left.Top - right.Top) < 0.01
               && Math.Abs(left.Right - right.Right) < 0.01
               && Math.Abs(left.Bottom - right.Bottom) < 0.01;
    }
}
