using System;
using System.Collections.Generic;
using System.IO;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using Xunit;
using D = RackCad.UI.Tests.DynamicHeaderBatchTestSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53D, G7 — L-1 (Proposal V2 §7.12; D-28..D-31): «Editar cabecera» abre el configurador compartido sobre una COPIA de
    /// la cabecera del modulo, en el editor AVANZADO si ya esta personalizada, y usa como dato del EDIT el resultado REAL
    /// que el configurador deja al cerrarse: <c>window.Configuration</c>.
    /// <para>
    /// El motivo es del configurador, que no se toca: su ViewModel REEMPLAZA la configuracion en «Aplicar» de la
    /// configuracion rapida, en «Restaurar estándar» y al abrir un proyecto, asi que la instancia entregada queda obsoleta en
    /// esos tres caminos; y como construye el ViewModel SOBRE la instancia recibida, abrirlo sobre la cabecera viva la edita
    /// antes de ningun commit. Se recorre el handler REAL con la costura de presentacion, sin bucle modal.
    /// </para>
    /// </summary>
    public sealed class DynamicHeaderConfiguratorL1Tests
    {
        [Fact]
        public void D28_AplicarDeConfiguracionRapida_SobreUnaCalculada_SeLeeDeWindowConfiguration()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M3");
                var expected = Math.Round(D.Module(window, "M3").AssociatedFrameConfiguration.Height) + 18.0;
                var modes = new List<bool>();
                window.HeaderConfiguratorPresenter = configurator =>
                {
                    modes.Add(configurator.ViewModel.IsAdvancedEditor);
                    D.QuickConfigAt(expected)(configurator); // «Aplicar» REEMPLAZA la configuracion del ViewModel
                };

                D.EditHeader(window);
                var m3 = D.Module(window, "M3");
                return (
                    Modes: modes.ToArray(),
                    Height: m3.AssociatedFrameConfiguration.Height,
                    Expected: expected,
                    Custom: !m3.UseCalculatedHeaderConfiguration,
                    Outcome: window.LastHeaderBatchOutcome);
            });

            Assert.True(Math.Abs(r.Expected - r.Height) < 1e-4,
                $"la altura del «Aplicar» rapido tenia que llegar al modulo: esperada {r.Expected}, obtenida {r.Height}");
            Assert.True(r.Custom, "el resultado del configurador personaliza la cabecera");
            Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal(new[] { false }, r.Modes); // una cabecera calculada se genera en modo rapido
        }

        [Fact]
        public void D29_RestaurarEstandarDelConfigurador_SeLee_YLaEdicionDescartadaNoLlegaAlModulo()
        {
            var r = D.Run(window =>
            {
                // Brazo de control: una edicion que SI se conserva llega al modulo.
                D.Select(window, "M3");
                var m3Horizontals = D.Module(window, "M3").AssociatedFrameConfiguration.Horizontals.Count;
                window.HeaderConfiguratorPresenter = configurator =>
                {
                    configurator.ViewModel.AddCommonSegment(44.0);
                    configurator.Close();
                };
                D.EditHeader(window);
                var kept = (Horizontals: D.Module(window, "M3").AssociatedFrameConfiguration.Horizontals.Count, Expected: m3Horizontals + 1);

                // La misma edicion, descartada con «Restaurar estándar» antes de cerrar.
                D.Select(window, "M5");
                var m5 = D.Module(window, "M5");
                var m5Recipe = D.Recipe(m5.AssociatedFrameConfiguration);
                window.HeaderConfiguratorPresenter = configurator =>
                {
                    configurator.ViewModel.AddCommonSegment(44.0);
                    configurator.ViewModel.RestoreStandardConfiguration(); // REEMPLAZA la configuracion del ViewModel
                    configurator.Close();
                };
                D.EditHeader(window);
                var m5After = D.Module(window, "M5");
                return (
                    Kept: kept,
                    Discarded: (Calculated: m5After.UseCalculatedHeaderConfiguration, SameRecipe: D.Recipe(m5After.AssociatedFrameConfiguration) == m5Recipe),
                    Status: D.Status(window));
            });

            Assert.True(r.Discarded.Calculated, "lo descartado con «Restaurar estándar» no puede personalizar la cabecera");
            Assert.True(r.Discarded.SameRecipe, "lo descartado con «Restaurar estándar» no puede llegar al modulo");
            Assert.Equal("Cabecera sin cambios.", r.Status);
            Assert.Equal(r.Kept.Expected, r.Kept.Horizontals);
        }

        [Fact]
        public void D30_AbrirProyectoDelConfigurador_SeLee()
        {
            var path = Path.Combine(Path.GetTempPath(), "rackcad-i53d-d30-" + Guid.NewGuid().ToString("N") + ".rackcad.json");
            try
            {
                var r = D.Run(window =>
                {
                    D.Select(window, "M3");
                    var project = new RackFrameProjectStore().DeepCopy(D.Module(window, "M3").AssociatedFrameConfiguration);
                    project.PanelClear = 33.5;
                    new RackFrameProjectStore().Save(project, path);

                    window.HeaderConfiguratorPresenter = configurator =>
                    {
                        configurator.ViewModel.LoadProjectFrom(path); // REEMPLAZA la configuracion del ViewModel
                        configurator.Close();
                    };
                    D.EditHeader(window);
                    var m3 = D.Module(window, "M3");
                    return (
                        PanelClear: m3.AssociatedFrameConfiguration.PanelClear,
                        Custom: !m3.UseCalculatedHeaderConfiguration,
                        Outcome: window.LastHeaderBatchOutcome);
                });

                Assert.True(Math.Abs(33.5 - r.PanelClear) < 1e-4,
                    $"el proyecto abierto en el configurador tenia que llegar al modulo: PanelClear {r.PanelClear}");
                Assert.True(r.Custom);
                Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(r.Outcome);
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [Fact]
        public void D31_UnaPersonalizada_SeReabreEnEditorAvanzado_SobreUnaCopia_NuncaSobreLaInstanciaViva()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M3");
                var live = D.Module(window, "M3").AssociatedFrameConfiguration;
                bool? advanced = null;
                bool? onLive = null;
                double? liveWhileOpen = null;
                window.HeaderConfiguratorPresenter = configurator =>
                {
                    advanced = configurator.ViewModel.IsAdvancedEditor;
                    onLive = ReferenceEquals(configurator.ViewModel.Configuration, live);
                    configurator.ViewModel.Configuration.PanelClear = 30.25; // edicion incremental del editor avanzado
                    liveWhileOpen = D.Module(window, "M3").AssociatedFrameConfiguration.PanelClear;
                    configurator.Close();
                };

                D.EditHeader(window);
                var after = D.Module(window, "M3").AssociatedFrameConfiguration;
                return (
                    Advanced: advanced,
                    OnLive: onLive,
                    LiveWhileOpen: liveWhileOpen,
                    After: after.PanelClear,
                    AfterIsLive: ReferenceEquals(after, live),
                    Outcome: window.LastHeaderBatchOutcome);
            }, D.WithCustom(D.Design(), "M3"));

            Assert.True(r.Advanced == true, "una cabecera ya personalizada se reabre en el editor AVANZADO");
            Assert.True(r.OnLive == false, "el configurador trabaja sobre una COPIA, nunca sobre la instancia viva");
            Assert.True(r.LiveWhileOpen.HasValue && Math.Abs(D.Marker - r.LiveWhileOpen.Value) < 1e-4,
                "mientras el configurador esta abierto la cabecera viva no se mueve");
            Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal(30.25, r.After, 4);
            Assert.False(r.AfterIsLive);
        }

        [Fact]
        public void D31_UnaCalculada_SeAbreEnModoRapido()
        {
            var modes = D.Run(window =>
            {
                D.Select(window, "M5");
                var seen = new List<bool>();
                window.HeaderConfiguratorPresenter = D.Edit(null, seen);
                D.EditHeader(window);
                return seen.ToArray();
            }, D.WithCustom(D.Design(), "M3"));

            Assert.Equal(new[] { false }, modes);
        }
    }
}
