using System;
using System.Linq;
using System.Reflection;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.UI.Systems.Dynamic;
using Xunit;
using D = RackCad.UI.Tests.DynamicHeaderBatchTestSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53D, G7 — D-32 (Proposal V2 §7.8; ADR-0037, decision 8): un recompute por operacion confirmada y ninguno en un
    /// rechazo, una peticion obsoleta o un cierre sin cambios.
    /// <para>
    /// El recompute del gesto es el de Application —<c>ApplyPostPeralte</c> + <c>Refresh</c>, dentro de MUTATE— y la
    /// ventana no agrega otro. Se mide con la costura <see cref="RackDynamicSystemWindow.RecomputeCount"/>, que cuenta cada
    /// recompute en el sitio donde ocurre, y se contrasta con evidencia independiente del propio rack: por IDENTIDAD, una
    /// recomposicion de la ventana reemplaza los modulos y un recompute regenera el modelo fisico de las cabeceras. El
    /// conteo no se infiere del resultado final.
    /// </para>
    /// </summary>
    public sealed class DynamicHeaderBatchRecomputeTests
    {
        [Fact]
        public void D32_DistribuirConfirmado_UnSoloRecompute_ElDeApplication_SinRecomposicionDeLaVentana()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);
                D.TargetsAll(window);
                var modules = D.Modules(window);
                var graph = D.Graph(modules);
                var before = window.RecomputeCount;

                D.Apply(window);

                var after = D.Modules(window);
                return (
                    Outcome: window.LastHeaderBatchOutcome,
                    Recomputes: window.RecomputeCount - before,
                    SameModules: modules.Count == after.Count && modules.Zip(after, (a, b) => ReferenceEquals(a, b)).All(same => same),
                    Regenerated: !graph.SetEquals(D.Graph(after)),
                    Log: window.HeaderBatchLog.ToArray());
            }, D.WithCustom(D.Design(), "M1"));

            Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal(1, r.Recomputes);
            Assert.True(r.SameModules, "la ventana no recompuso: los modulos son los mismos objetos");
            Assert.True(r.Regenerated, "el recompute de Application corrio: regenero el modelo fisico");
            Assert.Single(r.Log, entry => entry.StartsWith("recompute:", StringComparison.Ordinal));
            var plan = Array.FindIndex(r.Log, entry => entry.StartsWith("plan:", StringComparison.Ordinal));
            var recompute = Array.FindIndex(r.Log, entry => entry.StartsWith("recompute:", StringComparison.Ordinal));
            var outcome = Array.FindIndex(r.Log, entry => entry.StartsWith("outcome:", StringComparison.Ordinal));
            Assert.True(plan >= 0 && plan < recompute && recompute < outcome, "PREPARE -> MUTATE + recompute -> Outcome: " + string.Join(" / ", r.Log));
        }

        [Fact]
        public void D32_EditarConfirmado_UnSoloRecompute()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M3");
                window.HeaderConfiguratorPresenter = D.Edit(configuration => configuration.PanelClear = 40.0);
                var modules = D.Modules(window);
                var before = window.RecomputeCount;

                D.EditHeader(window);

                var after = D.Modules(window);
                return (
                    Outcome: window.LastHeaderBatchOutcome,
                    Recomputes: window.RecomputeCount - before,
                    SameModules: modules.Count == after.Count && modules.Zip(after, (a, b) => ReferenceEquals(a, b)).All(same => same));
            });

            Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal(1, r.Recomputes);
            Assert.True(r.SameModules);
        }

        [Theory]
        [InlineData("calculated-source")]
        [InlineData("separator-selected")]
        public void D32_Rechazado_CeroRecomputes_YElRackIntacto(string scenario)
        {
            var design = scenario == "calculated-source" ? D.Design() : D.WithCustom(D.Design(), "M1");
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);
                if (scenario == "calculated-source")
                {
                    D.Targets(window, "M3");
                }
                else
                {
                    D.TargetsCurrent(window);
                    D.Select(window, "M2"); // «Actual» con un separador seleccionado cuenta como sin seleccion
                }

                var graph = D.Graph(D.Modules(window));
                var before = window.RecomputeCount;
                D.Apply(window);
                return (
                    Outcome: window.LastHeaderBatchOutcome,
                    Recomputes: window.RecomputeCount - before,
                    Intact: graph.SetEquals(D.Graph(D.Modules(window))));
            }, design);

            var rejected = Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Rejected>(r.Outcome);
            Assert.Equal(
                scenario == "calculated-source" ? HeaderRejectionCode.SourceUnusable : HeaderRejectionCode.NoTargets,
                rejected.Code);
            Assert.Equal(0, r.Recomputes);
            Assert.True(r.Intact, "un rechazo no escribe ni recalcula nada");
        }

        [Fact]
        public void D32_PeticionObsoleta_EsStaleTargets_CeroRecomputes_YCeroEscrituras()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);
                D.TargetsAll(window);

                // Un cambio de tipo SIN reconstruccion cambia la secuencia: el origen tomado sobre la anterior queda obsoleto.
                D.ChangeKind(window, "M3", "Separador");

                var graph = D.Graph(D.Modules(window));
                var generation = window.HeaderBatchStateForTest.Generation;
                var before = window.RecomputeCount;
                D.Apply(window);
                return (
                    Outcome: window.LastHeaderBatchOutcome,
                    Recomputes: window.RecomputeCount - before,
                    Intact: graph.SetEquals(D.Graph(D.Modules(window))),
                    GenerationKept: window.HeaderBatchStateForTest.Generation == generation,
                    Status: D.Status(window));
            }, D.WithCustom(D.Design(), "M1"));

            Assert.Equal(HeaderRejectionCode.StaleTargets,
                Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Rejected>(r.Outcome).Code);
            Assert.Equal(0, r.Recomputes);
            Assert.True(r.Intact);
            Assert.True(r.GenerationKept);
            Assert.Contains("cambió", r.Status);
        }

        [Fact]
        public void D32_EnElDinamicoNadaPideConfirmacion_AsiQueNingunGestoTerminaCancelado()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);
                D.TargetsAll(window);
                D.Apply(window);
                var distribute = window.HeaderBatchLog.ToArray();

                D.Select(window, "M3");
                window.HeaderConfiguratorPresenter = D.Edit(configuration => configuration.PanelClear = 39.5);
                D.EditHeader(window);
                return (Distribute: distribute, Edit: window.HeaderBatchLog.ToArray(), Outcome: window.LastHeaderBatchOutcome);
            }, D.WithCustom(D.Design(), "M1"));

            // Cancelled solo existe para un plan que exigia confirmacion (nucleo compartido, G3), y el plan del Dinamico no
            // lleva avisos (G6): ningun plan pide confirmacion y la preparacion del Dinamico no ofrece cancelar.
            foreach (var log in new[] { r.Distribute, r.Edit })
            {
                Assert.Contains(log, entry => entry.StartsWith("plan:preparado", StringComparison.Ordinal) && entry.EndsWith("confirmacion=False", StringComparison.Ordinal));
                Assert.DoesNotContain(log, entry => entry.StartsWith("outcome:cancelado", StringComparison.Ordinal));
            }

            Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(r.Outcome);
            Assert.Null(typeof(DynamicHeaderBatchPreparation).GetMethod("Cancel", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
        }

        [Fact]
        public void D32_CerrarElConfiguradorSinCambios_NoEsUnaOperacion_CeroRecomputes()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M3");
                window.HeaderConfiguratorPresenter = D.Edit(null);
                var graph = D.Graph(D.Modules(window));
                var before = window.RecomputeCount;
                D.EditHeader(window);
                return (
                    Outcome: window.LastHeaderBatchOutcome,
                    Recomputes: window.RecomputeCount - before,
                    Intact: graph.SetEquals(D.Graph(D.Modules(window))),
                    Calculated: D.Module(window, "M3").UseCalculatedHeaderConfiguration,
                    Status: D.Status(window));
            });

            Assert.Null(r.Outcome);
            Assert.Equal(0, r.Recomputes);
            Assert.True(r.Intact);
            Assert.True(r.Calculated);
            Assert.Equal("Cabecera sin cambios.", r.Status);
        }
    }
}
