using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using Xunit;
using F = RackCad.Tests.DynamicHeaderBatchFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G6 — reconstruccion con reconciliacion (Proposal V2 §7.9, OD-2.b; matriz §13.4: D-20, D-21 y D-22).
    ///
    /// <para>
    /// La operacion de Application: intenciones por <c>DynamicRackSystemResolver.Snapshot</c> del sistema previo,
    /// <c>BuildDefault</c> con las entradas nuevas y <c>RackModuleReconciliation.Reconcile</c> sin restaurados. Lo que se
    /// conserva, se adapta, se elimina o cambia de tipo queda en un informe transportable; «Restaurar estandar» reconstruye
    /// sin intenciones. La ventana NO se cablea en G6: sigue con el par ordinal historico (Fact5).
    /// </para>
    /// </summary>
    public class DynamicHeaderBatchReconciliationTests
    {
        // ===== D-20 — reconciliacion por ModuleId + Kind con informe ====================================================

        [Fact]
        public void D20_RECONSTRUIR_RECONCILIA_CABECERAS_Y_SEPARADORES_POR_MODULE_ID_Y_KIND_E_INFORMA_CADA_CATEGORIA()
        {
            var editor = new F.Editor(F.Design(palletsDeep: 9));
            var m3 = editor.Customize("M3", F.Custom(201.0));
            m3.Exceptions.Add(F.RuntimeException("solo runtime"));
            editor.ManualLength("M4", 50.0);                                       // separador con fondo manual
            editor.Customize("M5", 202.0);                                         // con 8 fondos sera separador
            editor.Customize("M9", 203.0);                                         // con 8 fondos no existe
            var recetaM3 = F.Recipe(m3);

            var result = editor.Rebuild(palletsDeep: 8);

            // Premisas de la secuencia de 8 fondos.
            Assert.Equal(DynamicRackModuleKind.HeaderIntermediate, editor.Module("M3").Kind);
            Assert.Equal(DynamicRackModuleKind.Separator, editor.Module("M4").Kind);
            Assert.Equal(DynamicRackModuleKind.Separator, editor.Module("M5").Kind);
            Assert.DoesNotContain(editor.System.Modules, module => module.ModuleId == "M9");

            var informe = result.Reconciliation;
            Assert.Equal(new[] { "M3", "M4" }, informe.Preserved);
            Assert.Empty(informe.Adapted);
            Assert.Equal(new[] { "M9" }, informe.Removed);
            Assert.Equal(new[] { "M5" }, informe.Incompatible);
            Assert.Empty(informe.Restored);
            Assert.True(informe.LostAnything);
            Assert.False(string.IsNullOrWhiteSpace(informe.Describe()));
            Assert.Contains(informe.Describe(), result.Describe());

            // Nada se pierde en silencio: cada modulo personalizado cae en exactamente una categoria.
            Assert.Equal(
                new[] { "M3", "M4", "M5", "M9" },
                informe.Preserved.Concat(informe.Removed).Concat(informe.Incompatible).OrderBy(id => int.Parse(id.Substring(1))));

            // Lo conservado, en el sistema reconstruido que devuelve la operacion.
            var reconstruida = result.System.Modules.Single(module => module.ModuleId == "M3");
            Assert.False(reconstruida.UseCalculatedHeaderConfiguration);
            Assert.Equal(recetaM3, F.Recipe(reconstruida.AssociatedFrameConfiguration));
            Assert.NotEmpty(reconstruida.AssociatedFrameConfiguration.Members);
            // Las intenciones salen del Snapshot del resolver: su clon por documento no transporta el estado runtime.
            Assert.Empty(reconstruida.AssociatedFrameConfiguration.Exceptions);
            Assert.NotEmpty(m3.Exceptions);

            var separador = result.System.Modules.Single(module => module.ModuleId == "M4");
            Assert.Equal(50.0, separador.Length);
            Assert.True(separador.IsManualOverride);
            Assert.False(separador.IsCalculated);

            // Lo perdido ya no es personalizado; y la recomposicion posterior lo mantiene asi.
            Assert.Null(editor.Module("M5").AssociatedFrameConfiguration);
            Assert.False(editor.Module("M3").UseCalculatedHeaderConfiguration);
            Assert.Equal(50.0, editor.Module("M4").Length);
        }

        // ===== D-21 — cambio de tipo y reconstruccion ====================================================================

        [Fact]
        public void D21_UNA_PERSONALIZADA_DISTRIBUIDA_CUYO_MODULO_CAMBIA_DE_TIPO_AL_RECONSTRUIR_SE_INFORMA_INCOMPATIBLE()
        {
            var editor = new F.Editor(F.Design(palletsDeep: 9));
            editor.Customize("M1", 211.0);
            F.Committed(editor.Apply(editor.Prepare(editor.Distribute("M1", editor.Explicit("M3", "M5")))));

            var result = editor.Rebuild(palletsDeep: 8);

            Assert.Equal(DynamicRackModuleKind.Separator, editor.Module("M5").Kind);
            Assert.Equal(new[] { "M1", "M3" }, result.Reconciliation.Preserved);
            Assert.Equal(new[] { "M5" }, result.Reconciliation.Incompatible);
            Assert.Empty(result.Reconciliation.Removed);
            Assert.True(result.Reconciliation.LostAnything);
            Assert.Contains("M5", result.Describe());
            Assert.Null(editor.Module("M5").AssociatedFrameConfiguration);
        }

        [Fact]
        public void D21_UN_MODULO_CONVERTIDO_A_MANO_EN_SEPARADOR_QUE_LA_RECONSTRUCCION_VUELVE_CABECERA_SE_INFORMA_INCOMPATIBLE()
        {
            var editor = new F.Editor(F.Design(palletsDeep: 9));
            editor.ToSeparator("M3", 50.0);                                        // «Aplicar» del modulo: tipo y fondo manual

            var result = editor.Rebuild(palletDepth: 40.0);                         // el layout estandar pone cabecera en M3

            Assert.True(editor.Module("M3").IsHeader);
            Assert.Empty(result.Reconciliation.Preserved);
            Assert.Equal(new[] { "M3" }, result.Reconciliation.Incompatible);
            Assert.True(result.Reconciliation.LostAnything);
            Assert.Equal(40.0, editor.Module("M3").Length, 6);
            Assert.False(editor.Module("M3").IsManualOverride);
        }

        // ===== D-22 — «Restaurar estandar» = reset sin intenciones ======================================================

        [Fact]
        public void D22_RESTAURAR_ESTANDAR_RECONSTRUYE_SIN_INTENCIONES_Y_DEJA_EL_RACK_DE_UNO_RECIEN_CREADO()
        {
            var editor = new F.Editor(F.Design(palletsDeep: 9));
            editor.Customize("M1", 221.0);
            editor.Customize("M5", 222.0);
            editor.ManualLength("M4", 50.0);
            editor.ManualLength("M3", 52.0);

            var result = editor.Rebuild(restoreStandard: true);

            var informe = result.Reconciliation;
            Assert.Empty(informe.Preserved);
            Assert.Empty(informe.Adapted);
            Assert.Empty(informe.Removed);
            Assert.Empty(informe.Incompatible);
            Assert.Empty(informe.Restored);
            Assert.False(informe.LostAnything);
            Assert.Equal(string.Empty, informe.Describe());
            Assert.Equal(1L, result.Generation);

            Assert.All(editor.System.Modules, module =>
            {
                Assert.True(module.IsCalculated, module.ModuleId);
                Assert.False(module.IsManualOverride, module.ModuleId);
                Assert.True(module.UseCalculatedHeaderConfiguration, module.ModuleId);
            });
            Assert.Equal(F.SystemFingerprint(new F.Editor(F.Design(palletsDeep: 9)).System), F.SystemFingerprint(editor.System));
        }
    }
}
