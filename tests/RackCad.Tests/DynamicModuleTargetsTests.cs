using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using Xunit;
using F = RackCad.Tests.DynamicHeaderBatchFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G6 — <c>ModuleTargets</c>, firma, generacion e invalidacion del Dinamico (Proposal V2 §7.5-§7.7; matriz §13.4:
    /// D-16, D-17 y D-18).
    ///
    /// <para>
    /// Los ids del Dinamico son POSICIONALES: una reconstruccion reutiliza "M3" para un modulo nuevo. Por eso una peticion
    /// recuerda la firma —secuencia <c>ModuleId:Kind</c> + generacion— y no la identidad de un objeto vivo: toda
    /// reconstruccion avanza la generacion e invalida el origen y los destinos explicitos, y nada se aplica nunca al modulo
    /// que heredo un id. Recomponer SIN reconstruir conserva ids y generacion, asi que la peticion sigue sirviendo; el plan
    /// no, porque vive un solo gesto y no cruza recomputes.
    /// </para>
    /// </summary>
    public class DynamicModuleTargetsTests
    {
        private static IReadOnlyList<string> Secuencia(DynamicRackSystem system)
            => system.Modules.OrderBy(module => module.Index).Select(module => module.ModuleId + ":" + module.Kind).ToList();

        // ===== Gramatica: Actual, Explicito y Todas =====================================================================

        [Fact]
        public void LOS_DESTINOS_SON_ESTADO_DE_RUNTIME_QUE_EMPIEZA_EN_ACTUAL_SIN_ORIGEN_NI_GENERACION()
        {
            var editor = new F.Editor(F.Design());
            var state = new DynamicHeaderBatchState();
            Assert.Equal(0L, state.Generation);
            Assert.Null(state.Source);
            Assert.Equal(DynamicModuleTargetMode.FollowCurrent, state.Targets.Mode);
            Assert.Empty(state.Targets.ExplicitModuleIds);
            Assert.Null(state.Targets.ExplicitSignature);

            state.Targets.SetTargetModules(new[] { "M7", "M3", "M7" }, editor.System, state.Generation);
            Assert.Equal(DynamicModuleTargetMode.Explicit, state.Targets.Mode);
            Assert.Equal(new[] { "M7", "M3" }, state.Targets.ExplicitModuleIds);
            Assert.Equal(DynamicHeaderBatch.SequenceSignature(editor.System, state.Generation), state.Targets.ExplicitSignature);

            state.Targets.FollowAllModules();
            Assert.Equal(DynamicModuleTargetMode.All, state.Targets.Mode);
            Assert.Empty(state.Targets.ExplicitModuleIds);
            Assert.Null(state.Targets.ExplicitSignature);

            state.Targets.FollowCurrentModule();
            Assert.Equal(DynamicModuleTargetMode.FollowCurrent, state.Targets.Mode);

            var origen = state.RememberSource(editor.System, "M1");
            Assert.NotNull(origen);
            Assert.Same(origen, state.Source);
            Assert.Equal("M1", origen.Address.ModuleId);
            Assert.Equal(DynamicHeaderBatch.SequenceSignature(editor.System, state.Generation), origen.Signature);
            state.ForgetSource();
            Assert.Null(state.Source);
        }

        [Fact]
        public void UN_CONJUNTO_EXPLICITO_VACIO_ES_NO_TARGETS()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 11.0);

            Assert.Equal(HeaderRejectionCode.NoTargets, F.RejectedCode(editor.Prepare(editor.Distribute("M1", editor.Explicit()))));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("M4")]
        [InlineData("M42")]
        public void ACTUAL_SIN_UNA_CABECERA_SELECCIONADA_ES_NO_TARGETS_EN_DISTRIBUTE_Y_EN_EDIT(string seleccion)
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 12.0);

            Assert.Equal(
                HeaderRejectionCode.NoTargets,
                F.RejectedCode(editor.Prepare(editor.Distribute("M1", F.Current(), seleccion))));
            Assert.Equal(
                HeaderRejectionCode.NoTargets,
                F.RejectedCode(editor.Prepare(F.Request(DynamicHeaderBatchRequest.Edit(F.Custom(13.0), seleccion)))));
        }

        [Fact]
        public void ACTUAL_SOBRE_EL_PROPIO_ORIGEN_O_SOBRE_UNA_CABECERA_QUE_NO_SE_DIBUJA_ES_NO_APPLICABLE_TARGETS()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 14.0);
            Assert.Equal(
                HeaderRejectionCode.NoApplicableTargets,
                F.RejectedCode(editor.Prepare(editor.Distribute("M1", F.Current(), "M1"))));

            var blancos = new F.Editor(F.DesignWithUndrawnModules());
            Assert.Equal(
                HeaderRejectionCode.NoApplicableTargets,
                F.RejectedCode(blancos.Prepare(F.Request(DynamicHeaderBatchRequest.Edit(F.Custom(15.0), "M8")))));
        }

        [Fact]
        public void TODAS_SE_REEXPANDE_CONTRA_LA_SECUENCIA_VIGENTE_Y_UNA_RECONSTRUCCION_NO_LA_INVALIDA()
        {
            var editor = new F.Editor(F.Design());
            editor.State.Targets.FollowAllModules();

            var result = editor.Rebuild(palletsDeep: 11);

            Assert.False(result.ExplicitTargetsInvalidated);
            Assert.Equal(DynamicModuleTargetMode.All, editor.State.Targets.Mode);
            editor.Customize("M1", 16.0);
            var plan = F.Prepared(editor.Prepare(editor.Distribute("M1", editor.State.Targets)));
            Assert.Equal(new[] { "M3", "M5", "M7", "M9", "M11" }, F.Ids(plan.Targets));
        }

        [Fact]
        public void UN_ID_QUE_DESIGNA_DOS_MODULOS_NO_ES_UNA_DIRECCION_Y_NUNCA_SE_APLICA_AL_PRIMERO()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 17.0);
            editor.System.Modules[6].ModuleId = "M3";                              // un documento corrupto: M7 se llama M3
            var antes = F.SystemFingerprint(editor.System);

            Assert.Equal(
                HeaderRejectionCode.MalformedTarget,
                F.RejectedCode(editor.Prepare(editor.Distribute("M1", editor.Explicit("M3")))));
            Assert.Equal(
                HeaderRejectionCode.MalformedTarget,
                F.RejectedCode(editor.Prepare(editor.Distribute("M1", F.All()))));
            Assert.Equal(antes, F.SystemFingerprint(editor.System));
        }

        // ===== Firma =====================================================================================================

        [Fact]
        public void LA_FIRMA_ES_LA_SECUENCIA_ID_KIND_MAS_LA_GENERACION_Y_NO_UNA_IDENTIDAD_VIVA()
        {
            var editor = new F.Editor(F.Design());
            var firma = DynamicHeaderBatch.SequenceSignature(editor.System, 0);
            Assert.NotNull(firma);
            Assert.Equal(firma, DynamicHeaderBatch.SequenceSignature(editor.System, 0));
            Assert.NotEqual(firma, DynamicHeaderBatch.SequenceSignature(editor.System, 1));

            // Cabeceras, longitudes y banderas no son la secuencia.
            editor.Customize("M3", 21.0);
            editor.ManualLength("M4", 44.0);
            Assert.Equal(firma, DynamicHeaderBatch.SequenceSignature(editor.System, 0));

            // Recomponer sin reconstruir: otro sistema, la misma secuencia.
            var anterior = editor.System;
            editor.Recompose();
            Assert.NotSame(anterior, editor.System);
            Assert.Equal(firma, DynamicHeaderBatch.SequenceSignature(editor.System, 0));

            // Un cambio de tipo sin reconstruir si la cambia.
            editor.ToSeparator("M5", 48.0);
            Assert.NotEqual(firma, DynamicHeaderBatch.SequenceSignature(editor.System, 0));
        }

        // ===== D-16 — una reconstruccion invalida origen y destinos explicitos ==========================================

        [Theory]
        [InlineData("tarima")]
        [InlineData("fondos")]
        [InlineData("restaurar")]
        public void D16_UNA_RECONSTRUCCION_INVALIDA_EL_ORIGEN_Y_LOS_DESTINOS_EXPLICITOS_Y_LO_INFORMA(string causa)
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 161.0);
            editor.RememberSource("M1");
            editor.State.Targets.SetTargetModules(new[] { "M3", "M5" }, editor.System, editor.State.Generation);

            // Recomponer SIN reconstruir no invalida nada: la peticion sigue sirviendo.
            editor.Recompose();
            Assert.NotNull(editor.State.Source);
            Assert.Equal(DynamicModuleTargetMode.Explicit, editor.State.Targets.Mode);
            Assert.Equal(0L, editor.State.Generation);
            F.Prepared(editor.Prepare(F.Request(DynamicHeaderBatchRequest.Distribute(editor.State.Source, editor.State.Targets, null))));

            var result = causa == "tarima"
                ? editor.Rebuild(palletDepth: 40.0)
                : causa == "fondos"
                    ? editor.Rebuild(palletsDeep: 11)
                    : editor.Rebuild(restoreStandard: true);

            Assert.Equal(1L, result.Generation);
            Assert.Equal(1L, editor.State.Generation);
            Assert.True(result.SourceInvalidated);
            Assert.True(result.ExplicitTargetsInvalidated);
            Assert.Null(editor.State.Source);
            Assert.Equal(DynamicModuleTargetMode.FollowCurrent, editor.State.Targets.Mode);
            Assert.Empty(editor.State.Targets.ExplicitModuleIds);
            Assert.Null(editor.State.Targets.ExplicitSignature);
            var informe = result.Describe();
            Assert.Contains("origen", informe);
            Assert.Contains("destinos", informe);
        }

        [Fact]
        public void D16_SIN_ORIGEN_NI_DESTINOS_EXPLICITOS_NO_HAY_NADA_QUE_INVALIDAR_PERO_LA_GENERACION_AVANZA()
        {
            var editor = new F.Editor(F.Design());

            var primera = editor.Rebuild(palletDepth: 40.0);
            var segunda = editor.Rebuild(palletsDeep: 7);

            Assert.False(primera.SourceInvalidated);
            Assert.False(primera.ExplicitTargetsInvalidated);
            Assert.Equal(1L, primera.Generation);
            Assert.Equal(2L, segunda.Generation);
            Assert.Equal(2L, editor.State.Generation);
            Assert.DoesNotContain("origen", primera.Describe());
            Assert.DoesNotContain("destinos", primera.Describe());
        }

        // ===== D-17 — peticion de otra generacion ========================================================================

        [Fact]
        public void D17_UNA_PETICION_DE_GENERACION_ANTERIOR_ES_STALE_AUNQUE_LOS_IDS_COINCIDAN_Y_NUNCA_TOCA_AL_HEREDERO()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 171.0);
            var secuenciaAntes = Secuencia(editor.System);
            var origenViejo = editor.Source("M1");
            var destinosViejos = editor.Explicit("M3", "M5");
            var peticionVieja = F.Request(DynamicHeaderBatchRequest.Distribute(origenViejo, destinosViejos, null));

            editor.Rebuild(palletDepth: 40.0);                                     // mismos ids y tipos; otra generacion

            Assert.Equal(secuenciaAntes, Secuencia(editor.System));
            Assert.False(editor.Module("M1").UseCalculatedHeaderConfiguration);   // el origen sobrevivio reconciliado
            var herederos = new[] { "M3", "M5" }.ToDictionary(
                id => id,
                id => SelectiveHeaderBatchFixtures.Configuration(editor.Module(id).AssociatedFrameConfiguration));
            var antes = F.SystemFingerprint(editor.System);

            var preparation = editor.Prepare(peticionVieja);
            Assert.Equal(HeaderRejectionCode.StaleTargets, F.RejectedCode(preparation));
            Assert.Equal(HeaderRejectionCode.StaleTargets, F.RejectedCode(editor.Apply(preparation)));
            Assert.Equal(
                HeaderRejectionCode.StaleTargets,
                F.RejectedCode(editor.Prepare(F.Request(DynamicHeaderBatchRequest.Distribute(editor.Source("M1"), destinosViejos, null)))));
            Assert.Equal(
                HeaderRejectionCode.StaleTargets,
                F.RejectedCode(editor.Prepare(F.Request(DynamicHeaderBatchRequest.Distribute(origenViejo, F.All(), null)))));

            Assert.Equal(antes, F.SystemFingerprint(editor.System));
            foreach (var pair in herederos)
            {
                Assert.True(editor.Module(pair.Key).UseCalculatedHeaderConfiguration);
                Assert.Equal(pair.Value, SelectiveHeaderBatchFixtures.Configuration(editor.Module(pair.Key).AssociatedFrameConfiguration));
            }

            // La misma intencion, tomada en la generacion vigente, si aplica.
            var committed = F.Committed(editor.Apply(editor.Prepare(editor.Distribute("M1", editor.Explicit("M3", "M5")))));
            Assert.Equal(new[] { "M3", "M5" }, F.Ids(committed.Applied));
        }

        // ===== D-18 — firma distinta entre PREPARE y MUTATE ==============================================================

        [Fact]
        public void D18_UN_CAMBIO_DE_TIPO_SIN_RECONSTRUIR_ENTRE_PREPARE_Y_MUTATE_ES_STALE_CON_MUTACION_CERO()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 181.0);
            var preparation = editor.Prepare(editor.Distribute("M1", editor.Explicit("M3")));
            F.Prepared(preparation);

            editor.ToSeparator("M7", 48.0);
            var antes = F.SystemFingerprint(editor.System);
            var grafo = F.ReferenceGraph(editor.System);

            Assert.Equal(HeaderRejectionCode.StaleTargets, F.RejectedCode(editor.Apply(preparation)));
            Assert.Equal(antes, F.SystemFingerprint(editor.System));
            F.AssertSameGraph(grafo, F.ReferenceGraph(editor.System));
            Assert.True(editor.Module("M3").UseCalculatedHeaderConfiguration);
        }

        [Fact]
        public void D18_UNA_RECONSTRUCCION_ENTRE_PREPARE_Y_MUTATE_ES_STALE_CON_MUTACION_CERO()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 182.0);
            var preparation = editor.Prepare(editor.Distribute("M1", editor.Explicit("M3")));
            F.Prepared(preparation);

            editor.Rebuild(palletDepth: 40.0);
            var antes = F.SystemFingerprint(editor.System);

            Assert.Equal(HeaderRejectionCode.StaleTargets, F.RejectedCode(editor.Apply(preparation)));
            Assert.Equal(antes, F.SystemFingerprint(editor.System));
            Assert.True(editor.Module("M3").UseCalculatedHeaderConfiguration);
        }

        [Fact]
        public void D18_UN_FONDO_DE_DESTINO_EDITADO_ENTRE_PREPARE_Y_MUTATE_ES_STALE_PORQUE_LA_FIRMA_CUBRE_LO_VALIDADO()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 183.0);
            var preparation = editor.Prepare(editor.Distribute("M1", editor.Explicit("M3", "M5")));
            F.Prepared(preparation);

            editor.Module("M5").Length = 0.0;
            var antes = F.SystemFingerprint(editor.System);

            Assert.Equal(HeaderRejectionCode.StaleTargets, F.RejectedCode(editor.Apply(preparation)));
            Assert.Equal(antes, F.SystemFingerprint(editor.System));
            Assert.True(editor.Module("M3").UseCalculatedHeaderConfiguration);
        }

        [Fact]
        public void D18_EL_PLAN_NO_CRUZA_UNA_RECOMPOSICION_PERO_LA_PETICION_SI_PORQUE_IDS_Y_GENERACION_VIAJAN()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 184.0);
            var request = editor.Distribute("M1", editor.Explicit("M3"));
            var preparation = editor.Prepare(request);
            F.Prepared(preparation);

            editor.Recompose();
            var antes = F.SystemFingerprint(editor.System);

            Assert.Equal(HeaderRejectionCode.StaleTargets, F.RejectedCode(editor.Apply(preparation)));
            Assert.Equal(antes, F.SystemFingerprint(editor.System));

            var committed = F.Committed(editor.Apply(editor.Prepare(request)));
            Assert.Equal(new[] { "M3" }, F.Ids(committed.Applied));
            Assert.False(editor.Module("M3").UseCalculatedHeaderConfiguration);
        }
    }
}
