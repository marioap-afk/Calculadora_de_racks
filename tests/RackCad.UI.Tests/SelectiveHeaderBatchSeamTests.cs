using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.UI.Systems.Selective;
using Xunit;
using S = RackCad.UI.Tests.SelectiveHeaderBatchTestSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53S, G5 — S-30 (Proposal V2 §13.3): la confirmacion y el informe de un lote de cabeceras pasan por COSTURAS, nunca
    /// por un modal real. Una prueba que abriera un MessageBox colgaria el hilo STA compartido de la suite, y un aviso que no
    /// se puede sustituir es un aviso que no se puede probar.
    /// <para>
    /// Dos mitades: comportamiento (la costura de altura recibe la pregunta o el aviso; el informe del Outcome llega al
    /// estado de la ventana, con aplicados, omitidos y su motivo, rechazos y cancelaciones) y una guarda de fuentes que falla
    /// si el gesto gana un modal directo.
    /// </para>
    /// </summary>
    public sealed class SelectiveHeaderBatchSeamTests
    {
        private static RackSelectiveWindow WithSourceAt(int post, double height)
        {
            var window = SelectiveWindowTestSupport.Open();
            S.PlaceCustom(window, 1, post, height);
            S.SelectPost(window, post);
            S.TakeSource(window);
            return window;
        }

        // =============================================================================================================
        // Comportamiento
        // =============================================================================================================

        [Fact]
        public void S30_ASevereNotice_AsksOnceThroughTheSeam_NamingEveryAffectedDestination()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = WithSourceAt(1, 20.0); // por debajo del nivel superior en todos los destinos
                S.SetPostTargets(window, 2, 3);
                var severe = new List<string>();
                var informative = new List<string>();
                using (SelectiveCabeceraHeightPrompt.Substitute(m => { severe.Add(m); return true; }, m => informative.Add(m)))
                {
                    S.Apply(window);
                }

                return (Severe: severe.ToArray(), Informative: informative.Count, Outcome: window.LastHeaderBatchOutcome);
            });

            Assert.Single(r.Severe);
            Assert.Contains("Poste 2", r.Severe[0]);
            Assert.Contains("Poste 3", r.Severe[0]);
            Assert.Equal(0, r.Informative);
            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
        }

        [Fact]
        public void S30_OnlyInformativeNotices_InformThroughTheSeam_WithoutAskingToConfirm()
        {
            var r = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open();
                var high = window.CustomizeSeedHeightForTest(0) + 40.0; // difiere, pero nunca queda por debajo
                S.PlaceCustom(window, 1, 1, high);
                S.SelectPost(window, 1);
                S.TakeSource(window);
                S.SetPostTargets(window, 2);
                var asked = 0;
                var informed = new List<string>();
                using (SelectiveCabeceraHeightPrompt.Substitute(_ => { asked++; return true; }, m => informed.Add(m)))
                {
                    S.Apply(window);
                }

                return (Asked: asked, Informed: informed.Count, Outcome: window.LastHeaderBatchOutcome);
            });

            Assert.Equal(0, r.Asked);
            Assert.Equal(1, r.Informed);
            Assert.IsType<HeaderBatchOutcome<SelectiveHeaderAddress>.Committed>(r.Outcome);
        }

        [Fact]
        public void S30_TheReportOfACommittedBatch_ListsTheAppliedDestinations_AndEveryOmissionWithItsReason()
        {
            var status = StaTestRunner.Run(() =>
            {
                var window = SelectiveWindowTestSupport.Open(2);
                S.SetFrentesOfFondo(window, 1, 2); // postes 0..2
                S.SetFrentesOfFondo(window, 2, 1); // postes 0..1
                S.ShowFondo(window, 1);
                S.PlaceCustom(window, 1, 1, window.CustomizeSeedHeightForTest(0));
                S.SelectPost(window, 1);
                S.TakeSource(window);
                SelectiveTargetsTestSupport.SetAllTargets(window);
                S.SetAllPostTargets(window);
                using (SelectiveCabeceraHeightPrompt.Substitute(_ => true))
                {
                    S.Apply(window);
                }

                return window.StatusText.Text;
            });

            Assert.Contains("F1, Poste 2", status);
            Assert.Contains("F2, Poste 1", status);
            Assert.Contains("F1, Poste 1 (es el origen)", status);
            Assert.Contains("F2, Poste 3 (no existe en ese fondo)", status);
        }

        [Fact]
        public void S30_TheReportOfARejectionAndOfACancellation_SaysWhatHappened()
        {
            var r = StaTestRunner.Run(() =>
            {
                var standard = SelectiveWindowTestSupport.Open();
                S.SelectPost(standard, 1);
                S.TakeSource(standard);
                S.SetPostTargets(standard, 2);
                using (SelectiveCabeceraHeightPrompt.Substitute(_ => true))
                {
                    S.Apply(standard);
                }

                var cancelled = WithSourceAt(1, 20.0);
                S.SetPostTargets(cancelled, 2);
                using (SelectiveCabeceraHeightPrompt.Substitute(_ => false))
                {
                    S.Apply(cancelled);
                }

                return (Rejected: standard.StatusText.Text, Cancelled: cancelled.StatusText.Text);
            });

            Assert.Contains("no es una cabecera personalizada", r.Rejected);
            Assert.Contains("Cancelado", r.Cancelled);
        }

        // =============================================================================================================
        // Guarda de fuentes: ningun modal directo nuevo
        // =============================================================================================================

        private static string WindowSource()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln"))) dir = dir.Parent;
            Assert.True(dir != null, "No se localizo la raiz del repo (RackCad.sln).");
            return File.ReadAllText(Path.Combine(dir.FullName, "src", "RackCad.UI", "Systems", "Selective", "RackSelectiveWindow.xaml.cs"));
        }

        /// <summary>The body of the member whose declaration starts with <paramref name="signature"/>, by brace matching.</summary>
        private static string BodyOf(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);
            Assert.True(start >= 0, "No se encontro '" + signature + "' en RackSelectiveWindow.xaml.cs.");
            var open = source.IndexOf('{', start);
            var depth = 0;
            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{') depth++;
                else if (source[i] == '}' && --depth == 0) return source.Substring(start, i - start + 1);
            }

            return source.Substring(start);
        }

        [Fact]
        public void S30_GUARD_TheBatchGestureOpensNoModalOfItsOwn_AndConfirmsThroughTheSeam()
        {
            var source = WindowSource();

            // El censo de modales directos de la ventana NO crece: los cuatro MessageBox y los cuatro ShowDialog que ya
            // existian (medio frente, configurador, seguridad, BOM; cambios sin aplicar y vistas ligadas).
            Assert.Equal(4, Regex.Matches(source, @"MessageBox\.Show\(").Count);
            Assert.Equal(4, Regex.Matches(source, @"\.ShowDialog\(").Count);

            foreach (var signature in new[]
                     {
                         "private void TakeHeaderSource_Click(",
                         "private void ApplyHeaderBatch_Click(",
                         "private void RunHeaderBatch(",
                         "private void ApplyCustomizedCabecera(",
                         "private void RefreshPostTargets(",
                         "private void ReportHeaderBatch(",
                     })
            {
                var body = BodyOf(source, signature);
                Assert.DoesNotContain("MessageBox", body);
                Assert.DoesNotContain("ShowDialog(", body);
            }

            var gesture = BodyOf(source, "private void RunHeaderBatch(");
            Assert.Contains("SelectiveCabeceraHeightPrompt.ConfirmSevere(", gesture);
            Assert.Contains("SelectiveCabeceraHeightPrompt.Inform(", gesture);

            // El configurador se muestra detras de la costura de presentacion; su ShowDialog es el camino de produccion.
            var presenter = BodyOf(source, "private RackFrameConfiguration ShowHeaderConfigurator(");
            Assert.Contains("HeaderConfiguratorPresenter", presenter);
            Assert.Contains("ViewModel.IsAdvancedEditor", presenter);
            Assert.Contains("ShowDialog(", presenter);
        }
    }
}
