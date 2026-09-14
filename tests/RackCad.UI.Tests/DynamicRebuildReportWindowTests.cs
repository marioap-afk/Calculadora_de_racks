using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.UI.Shell;
using Xunit;
using D = RackCad.UI.Tests.DynamicHeaderBatchTestSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-53D, G7 — D-33 y la reconstruccion de la ventana (Proposal V2 §7.6, §7.9; OD-2.b).
    /// <para>
    /// Toda reconstruccion del Dinamico —cambio de tarima, cambio de fondos y «Restaurar layout»— pasa por
    /// <see cref="DynamicRackRebuild"/>: intenciones por el snapshot del resolver, reconciliacion por <c>ModuleId + Kind</c>
    /// e informe. La ventana muestra ese informe con las palabras de Application (<c>Describe</c>, <c>LostAnything</c>) y lo
    /// mantiene visible aunque otro mensaje ocupe la linea de estado: nada se pierde en silencio. La generacion avanza en
    /// cada reconstruccion y descarta el origen y los destinos elegidos; una recomposicion sin reconstruccion no.
    /// </para>
    /// </summary>
    public sealed class DynamicRebuildReportWindowTests
    {
        [Fact]
        public void D33_CambioDeTarima_ConservaYAdaptaLaPersonalizada_YElInformeSeVeYSeQueda()
        {
            var design = D.WithManualLength(D.WithCustom(D.Design(), "M3"), "M2", 44.0);
            var r = D.Run(window =>
            {
                var generation = window.HeaderBatchStateForTest.Generation;
                D.ChangePalletDepth(window, 52.0);
                var rebuild = window.LastRebuildResult;
                var reportAfterRebuild = D.RebuildReport(window);
                var statusAfterRebuild = D.Status(window);

                // Otro mensaje ocupa la linea de estado (un alcance de celdas recompone sin reconstruir): el informe se queda.
                EditorWindowTestSupport.ClickNamed(window, "ApplyCellButton");
                var m3 = D.Module(window, "M3");
                return (
                    Rebuild: rebuild,
                    Generation: window.HeaderBatchStateForTest.Generation - generation,
                    Report: reportAfterRebuild,
                    Status: statusAfterRebuild,
                    ReportAfterOtherStatus: D.RebuildReport(window),
                    OtherStatus: D.Status(window),
                    M3Custom: !m3.UseCalculatedHeaderConfiguration,
                    M3PanelClear: m3.AssociatedFrameConfiguration.PanelClear,
                    M3Depth: m3.AssociatedFrameConfiguration.Depth,
                    M3Length: m3.Length,
                    M2Length: D.Module(window, "M2").Length,
                    M2Manual: D.Module(window, "M2").IsManualOverride);
            }, design);

            Assert.NotNull(r.Rebuild);
            Assert.Equal(1, r.Generation);
            Assert.Equal(new[] { "M2", "M3" }, r.Rebuild.Reconciliation.Preserved.OrderBy(id => id, StringComparer.Ordinal));
            Assert.Contains("M3", r.Rebuild.Reconciliation.Adapted);
            Assert.False(r.Rebuild.Reconciliation.LostAnything);
            Assert.Equal("Última reconstrucción: " + r.Rebuild.Describe() + ".", r.Report);
            Assert.Contains(r.Rebuild.Describe(), r.Status);
            Assert.Equal(r.Report, r.ReportAfterOtherStatus);
            Assert.DoesNotContain(r.Rebuild.Describe(), r.OtherStatus);
            Assert.True(r.M3Custom);
            Assert.Equal(D.Marker, r.M3PanelClear, 4);
            Assert.Equal(52.0, r.M3Length, 4);
            Assert.Equal(r.M3Length, r.M3Depth, 4);
            Assert.Equal(44.0, r.M2Length, 4);   // la longitud manual de un SEPARADOR tambien se conserva
            Assert.True(r.M2Manual);
        }

        [Fact]
        public void D33_CambioDeFondos_QuePierdePersonalizaciones_LoInformaComoAviso_YNoLoTapaElEstadoDelAlcance()
        {
            var design = D.WithCustom(D.WithCustom(D.Design(), "M3"), "M5");
            var r = D.Run(window =>
            {
                D.ChangeFondosOfAllFronts(window, 4); // M1 C, M2 S, M3 S, M4 C
                var block = window.FindName("RebuildReportText") as TextBlock;
                return (
                    Rebuild: window.LastRebuildResult,
                    Report: D.RebuildReport(window),
                    Status: D.Status(window),
                    Brush: (block?.Foreground as SolidColorBrush)?.Color,
                    Warning: (EditorStatusPalette.For(EditorStatusSeverity.Warning) as SolidColorBrush)?.Color);
            }, design);

            Assert.NotNull(r.Rebuild);
            Assert.Equal(new[] { "M5" }, r.Rebuild.Reconciliation.Removed);
            Assert.Equal(new[] { "M3" }, r.Rebuild.Reconciliation.Incompatible);
            Assert.True(r.Rebuild.Reconciliation.LostAnything);
            Assert.Contains("eliminado", r.Report);
            Assert.Contains("incompatible", r.Report);
            Assert.StartsWith("Datos estructurales aplicados", r.Status); // el gesto de fondos dice lo suyo...
            Assert.NotNull(r.Warning);
            Assert.Equal(r.Warning, r.Brush);                              // ...y la perdida sigue a la vista, como aviso
        }

        [Fact]
        public void D33_RestaurarLayout_ReconstruyeSinIntenciones_YDescartaOrigenYDestinos_Informandolo()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);
                D.Targets(window, "M3");
                var generation = window.HeaderBatchStateForTest.Generation;

                D.RestoreLayout(window);

                var state = window.HeaderBatchStateForTest;
                return (
                    Rebuild: window.LastRebuildResult,
                    Generation: state.Generation - generation,
                    Source: state.Source,
                    Mode: state.Targets.Mode,
                    SourceCaption: D.SourceCaption(window),
                    TargetsCaption: D.TargetsCaption(window),
                    Report: D.RebuildReport(window),
                    M1Calculated: D.Module(window, "M1").UseCalculatedHeaderConfiguration);
            }, D.WithCustom(D.Design(), "M1"));

            Assert.NotNull(r.Rebuild);
            Assert.Empty(r.Rebuild.Reconciliation.Preserved); // «Restaurar estándar» = reconstruir SIN intenciones
            Assert.False(r.Rebuild.Reconciliation.LostAnything);
            Assert.True(r.M1Calculated);
            Assert.Equal(1, r.Generation);
            Assert.True(r.Rebuild.SourceInvalidated);
            Assert.True(r.Rebuild.ExplicitTargetsInvalidated);
            Assert.Null(r.Source);
            Assert.Equal(DynamicModuleTargetMode.FollowCurrent, r.Mode);
            Assert.Equal("Sin origen.", r.SourceCaption);
            Assert.Equal("Actual", r.TargetsCaption);
            Assert.Contains("se descartó el origen recordado", r.Report);
            Assert.Contains("se descartaron los destinos elegidos", r.Report);
        }

        [Fact]
        public void D33_UnaReconstruccion_DescartaElOrigen_YAplicarDespuesNoTocaAlModuloQueHeredoSuId()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M3");
                D.TakeSource(window);
                D.TargetsAll(window);
                D.ChangePalletDepth(window, 52.0); // reconstruye: M3 sigue existiendo con el mismo id
                var graph = D.Graph(D.Modules(window));
                D.Apply(window);
                return (
                    Log: window.HeaderBatchLog.ToArray(),
                    Outcome: window.LastHeaderBatchOutcome,
                    Intact: graph.SetEquals(D.Graph(D.Modules(window))),
                    Status: D.Status(window));
            }, D.WithCustom(D.Design(), "M3"));

            Assert.Contains("sin-origen", r.Log);
            Assert.Null(r.Outcome);
            Assert.True(r.Intact);
            Assert.Contains("Tomar como origen", r.Status);
        }

        [Fact]
        public void D33_UnaRecomposicionSinReconstruccion_NoAvanzaLaGeneracion_NiDescartaElOrigen()
        {
            var r = D.Run(window =>
            {
                D.Select(window, "M1");
                D.TakeSource(window);
                var generation = window.HeaderBatchStateForTest.Generation;
                var rebuild = window.LastRebuildResult;

                D.ChangePostPeralte(window, 3.5);

                var kept = (
                    Generation: window.HeaderBatchStateForTest.Generation == generation,
                    Source: window.HeaderBatchStateForTest.Source?.Address.ModuleId,
                    SameRebuild: ReferenceEquals(rebuild, window.LastRebuildResult));
                D.Select(window, "M3");
                D.TargetsCurrent(window);
                D.Apply(window);
                return (Kept: kept, Outcome: window.LastHeaderBatchOutcome);
            }, D.WithCustom(D.Design(), "M1"));

            Assert.True(r.Kept.Generation);
            Assert.Equal("M1", r.Kept.Source);
            Assert.True(r.Kept.SameRebuild);
            var committed = Assert.IsType<HeaderBatchOutcome<DynamicHeaderAddress>.Committed>(r.Outcome);
            Assert.Equal("M3", string.Join(" ", committed.Applied.Select(address => address.ModuleId)));
        }

        [Fact]
        public void D33_UnRackSinPersonalizaciones_ReconstruyeSinNadaQueInformar()
        {
            var r = D.Run(window =>
            {
                D.ChangePalletDepth(window, 52.0);
                return (Rebuild: window.LastRebuildResult, Report: D.RebuildReport(window), Status: D.Status(window));
            });

            Assert.NotNull(r.Rebuild);
            Assert.Equal(string.Empty, r.Rebuild.Describe());
            Assert.Equal(string.Empty, r.Report);
            Assert.Equal("Vista recalculada (layout estándar).", r.Status);
        }
    }
}
