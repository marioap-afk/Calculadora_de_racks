using System;
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
    /// I-53 G6 — PREPARE del Dinamico (Proposal V2 §3.3-§3.8, §7.4-§7.8; matriz §13.4: D-01, D-05, D-06, D-07, D-09, D-10,
    /// D-13 y D-25).
    ///
    /// <para>
    /// El origen es una DIRECCION (<c>ModuleId</c> + firma de la secuencia) capturada al aplicar con su valor ACTUAL; los
    /// destinos se resuelven contra la secuencia vigente en orden de <c>Index</c>; lo que no se dibuja se omite y lo que no
    /// pertenece al universo se rechaza. PREPARE es puro: el sistema vivo, sus identidades y su forma serializada no cambian.
    /// </para>
    /// </summary>
    public class DynamicHeaderBatchPrepareTests
    {
        private static bool Present(F.Editor editor, string moduleId)
            => RackModuleDescriptor.Describe(editor.System).Single(descriptor => descriptor.ModuleId == moduleId).IsPhysicallyPresent;

        // ===== D-01 — origen elegible ===================================================================================

        [Fact]
        public void D01_UN_ORIGEN_PERSONALIZADO_USABLE_Y_DIBUJADO_PREPARA_UNA_COPIA_PROPIA_POR_DESTINO()
        {
            var editor = new F.Editor(F.Design());
            var origen = editor.Customize("M1", 21.0);

            var preparation = editor.Prepare(editor.Distribute("M1", editor.Explicit("M3", "M5")));

            var plan = F.Prepared(preparation);
            Assert.Equal(DynamicHeaderBatchOperation.Distribute, preparation.Operation);
            Assert.Equal(new[] { "M3", "M5" }, F.Ids(plan.Targets));
            Assert.Empty(plan.Omitted);
            Assert.Empty(plan.Warnings);
            Assert.False(plan.RequiresConfirmation);
            var copies = F.Copies(preparation);
            Assert.All(copies, copy => Assert.NotSame(origen, copy));
            Assert.All(copies, copy => Assert.Equal(F.Recipe(origen), F.Recipe(copy)));
            Assert.NotSame(copies[0], copies[1]);
        }

        [Fact]
        public void D01_UN_ORIGEN_CALCULADO_ES_SOURCE_UNUSABLE_SIN_MUTAR_NADA()
        {
            var editor = new F.Editor(F.Design());
            var antes = F.SystemFingerprint(editor.System);

            var preparation = editor.Prepare(editor.Distribute("M1", editor.Explicit("M3")));

            Assert.Equal(HeaderRejectionCode.SourceUnusable, F.RejectedCode(preparation));
            Assert.Equal(antes, F.SystemFingerprint(editor.System));
        }

        [Fact]
        public void D01_UN_ORIGEN_PERSONALIZADO_QUE_NO_SE_DIBUJA_ES_SOURCE_UNUSABLE()
        {
            var editor = new F.Editor(F.DesignWithUndrawnModules());
            Assert.False(Present(editor, "M6"), "Premisa: M6 existe logicamente pero no se dibuja.");
            Assert.True(Present(editor, "M4"), "Premisa: M4 se dibuja.");
            editor.Customize("M6", 22.0);

            var preparation = editor.Prepare(editor.Distribute("M6", editor.Explicit("M4")));

            Assert.Equal(HeaderRejectionCode.SourceUnusable, F.RejectedCode(preparation));
        }

        [Fact]
        public void D01_UN_ORIGEN_MARCADO_PERSONALIZADO_SIN_CONFIGURACION_O_CON_UNA_NO_USABLE_ES_SOURCE_UNUSABLE()
        {
            var sinConfiguracion = new F.Editor(F.Design());
            var m1 = sinConfiguracion.Module("M1");
            m1.UseCalculatedHeaderConfiguration = false;
            m1.AssociatedFrameConfiguration = null;
            Assert.Equal(
                HeaderRejectionCode.SourceUnusable,
                F.RejectedCode(sinConfiguracion.Prepare(sinConfiguracion.Distribute("M1", sinConfiguracion.Explicit("M3")))));

            var noUsable = new F.Editor(F.Design());
            noUsable.Customize("M1", 23.0).Height = 0.0;
            Assert.Equal(
                HeaderRejectionCode.SourceUnusable,
                F.RejectedCode(noUsable.Prepare(noUsable.Distribute("M1", noUsable.Explicit("M3")))));
        }

        // ===== D-05 — el origen se omite ================================================================================

        [Fact]
        public void D05_EL_MODULO_ORIGEN_SE_OMITE_COMO_IS_SOURCE_Y_CONSERVA_SU_PROPIA_INSTANCIA()
        {
            var editor = new F.Editor(F.Design());
            var origen = editor.Customize("M5", 55.0);
            var receta = F.Recipe(origen);

            var preparation = editor.Prepare(editor.Distribute("M5", F.All()));
            var plan = F.Prepared(preparation);
            Assert.Equal(new[] { "M1", "M3", "M7", "M9" }, F.Ids(plan.Targets));
            Assert.Equal(new[] { "M5:IsSource" }, F.Omissions(plan.Omitted));

            var committed = F.Committed(editor.Apply(preparation));
            Assert.DoesNotContain("M5", F.Ids(committed.Applied));
            Assert.Equal(new[] { "M5:IsSource" }, F.Omissions(committed.Omitted));
            Assert.Same(origen, editor.Module("M5").AssociatedFrameConfiguration);
            Assert.False(editor.Module("M5").UseCalculatedHeaderConfiguration);
            Assert.Equal(receta, F.Recipe(origen));
        }

        // ===== D-06 — origen en vivo ====================================================================================

        [Fact]
        public void D06_ORIGEN_EN_VIVO_LA_DIRECCION_RECORDADA_APLICA_EL_VALOR_ACTUAL_Y_NO_UN_PORTAPAPELES()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 61.0);
            editor.RememberSource("M1");                                     // «Tomar como origen»: solo la direccion

            // El usuario edita el origen DESPUES: primero otra instancia, luego esa misma instancia en sitio. Un portapapeles
            // por referencia veria 61 y uno por copia tambien; la direccion ve el valor actual.
            editor.Customize("M1", 62.0);
            editor.Module("M1").AssociatedFrameConfiguration.PanelClear = 63.0;

            var request = F.Request(DynamicHeaderBatchRequest.Distribute(editor.State.Source, editor.Explicit("M3", "M5"), null));
            var preparation = editor.Prepare(request);

            Assert.All(F.Copies(preparation), copy => Assert.Equal(63.0, copy.PanelClear));
            F.Committed(editor.Apply(preparation));
            Assert.Equal(63.0, editor.Module("M3").AssociatedFrameConfiguration.PanelClear);
            Assert.Equal(63.0, editor.Module("M5").AssociatedFrameConfiguration.PanelClear);
        }

        // ===== D-07 — origen desaparecido ===============================================================================

        /// <summary>
        /// Con la firma vigente, el id recordado ya no designa una cabecera. Una direccion recordada ANTES de un cambio de la
        /// secuencia no llega aqui: la precedencia la rechaza antes como <c>StaleTargets</c> (D-17, D-25).
        /// </summary>
        [Theory]
        [InlineData("M42")]
        [InlineData("M4")]
        [InlineData("")]
        public void D07_ORIGEN_DESAPARECIDO_EL_ID_YA_NO_DESIGNA_UNA_CABECERA_CON_FIRMA_VIGENTE_ES_SOURCE_NOT_FOUND(string sourceId)
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 71.0);
            var antes = F.SystemFingerprint(editor.System);

            var preparation = editor.Prepare(editor.Distribute(sourceId, editor.Explicit("M3")));

            Assert.Equal(HeaderRejectionCode.SourceNotFound, F.RejectedCode(preparation));
            Assert.Equal(HeaderRejectionCode.SourceNotFound, F.RejectedCode(editor.Apply(preparation)));
            Assert.Equal(antes, F.SystemFingerprint(editor.System));
        }

        // ===== D-09 — destino malformado ================================================================================

        [Theory]
        [InlineData("M4")]
        [InlineData("M42")]
        [InlineData("")]
        public void D09_UN_SEPARADOR_O_UN_ID_DESCONOCIDO_CON_FIRMA_VIGENTE_ES_MALFORMED_TARGET_CON_MUTACION_CERO(string malformado)
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 91.0);
            var antes = F.SystemFingerprint(editor.System);

            var preparation = editor.Prepare(editor.Distribute("M1", editor.Explicit("M3", malformado, "M5")));

            Assert.Equal(HeaderRejectionCode.MalformedTarget, F.RejectedCode(preparation));
            Assert.Equal(HeaderRejectionCode.MalformedTarget, F.RejectedCode(editor.Apply(preparation)));
            Assert.Equal(antes, F.SystemFingerprint(editor.System));
        }

        // ===== D-10 — sin presencia fisica ==============================================================================

        [Fact]
        public void D10_UNA_CABECERA_SIN_PRESENCIA_FISICA_SE_OMITE_COMO_NOT_PHYSICALLY_PRESENT_Y_NO_SE_TOCA()
        {
            var editor = new F.Editor(F.DesignWithUndrawnModules());
            Assert.Equal(new[] { "M1", "M4", "M6", "M8", "M10" }, editor.HeaderIds);
            Assert.True(Present(editor, "M1") && Present(editor, "M4"), "Premisa: M1 y M4 se dibujan.");
            Assert.False(Present(editor, "M6") || Present(editor, "M8") || Present(editor, "M10"), "Premisa: M6, M8 y M10 no.");

            editor.Customize("M1", 101.0);
            var sinDibujar = new[] { "M6", "M8", "M10" }.ToDictionary(id => id, id => editor.Module(id).AssociatedFrameConfiguration);
            var huellas = sinDibujar.ToDictionary(pair => pair.Key, pair => SelectiveHeaderBatchFixtures.Configuration(pair.Value));

            var preparation = editor.Prepare(editor.Distribute("M1", F.All()));
            var plan = F.Prepared(preparation);
            Assert.Equal(new[] { "M4" }, F.Ids(plan.Targets));
            Assert.Equal(
                new[] { "M1:IsSource", "M6:NotPhysicallyPresent", "M8:NotPhysicallyPresent", "M10:NotPhysicallyPresent" },
                F.Omissions(plan.Omitted));

            var committed = F.Committed(editor.Apply(preparation));
            Assert.Equal(new[] { "M4" }, F.Ids(committed.Applied));
            Assert.Equal(F.Omissions(plan.Omitted), F.Omissions(committed.Omitted));
            Assert.False(editor.Module("M4").UseCalculatedHeaderConfiguration);
            foreach (var pair in sinDibujar)
            {
                var module = editor.Module(pair.Key);
                Assert.True(module.UseCalculatedHeaderConfiguration);
                Assert.Same(pair.Value, module.AssociatedFrameConfiguration);
                Assert.Equal(huellas[pair.Key], SelectiveHeaderBatchFixtures.Configuration(module.AssociatedFrameConfiguration));
            }
        }

        [Fact]
        public void D10_SI_TODOS_LOS_DESTINOS_QUEDAN_OMITIDOS_ES_NO_APPLICABLE_TARGETS_CON_MUTACION_CERO()
        {
            var editor = new F.Editor(F.DesignWithUndrawnModules());
            editor.Customize("M1", 102.0);
            var antes = F.SystemFingerprint(editor.System);

            var preparation = editor.Prepare(editor.Distribute("M1", editor.Explicit("M10", "M6", "M1")));

            Assert.Equal(HeaderRejectionCode.NoApplicableTargets, F.RejectedCode(preparation));
            Assert.Equal(antes, F.SystemFingerprint(editor.System));
        }

        // ===== D-13 — pureza de PREPARE ==================================================================================

        [Fact]
        public void D13_PREPARE_ES_PURO_SISTEMA_SERIALIZADO_IDENTIDADES_Y_ESTADO_INTACTOS_SIN_REFRESH_PERALTE_NI_RECONCILE()
        {
            var editor = new F.Editor(F.Design());
            var origen = editor.Customize("M1", 131.0);
            origen.Exceptions.Add(F.RuntimeException("estado runtime"));

            // Un sistema deliberadamente NO normalizado: un Refresh (Depth = Length), un ApplyPostPeralte (peralte del rack)
            // o un Reconcile (reemplaza configuraciones) lo delatarian.
            origen.Depth = 70.0;
            origen.PostPeralte = editor.System.PostPeralte + 2.5;
            editor.Module("M3").AssociatedFrameConfiguration.Depth = 41.0;
            editor.System.HeaderLineOverrides.Add(new DynamicHeaderLineOverride { PostIndex = 1, ModuleId = "M3", Header = F.Custom(132.0) });
            editor.System.DerivedPostLineOverrides.Add(new DynamicDerivedPostLineOverride { PostIndex = 1, Height = 90.0 });
            editor.State.Targets.SetTargetModules(new[] { "M5", "M3" }, editor.System, editor.State.Generation);
            Assert.NotEqual(editor.Module("M1").Length, origen.Depth);

            var peticiones = new List<DynamicHeaderBatchRequest>
            {
                editor.Distribute("M1", F.All()),
                editor.Distribute("M1", editor.State.Targets),
                F.Request(DynamicHeaderBatchRequest.Edit(F.Custom(133.0, depth: 60.0), "M5")),
                editor.Distribute("M1", editor.Explicit("M3", "M4")),
                F.Request(DynamicHeaderBatchRequest.Distribute(DynamicHeaderSource.Of(editor.System, 7, "M1"), F.All(), null)),
            };

            var serializado = F.Serialized(editor);
            var huella = F.SystemFingerprint(editor.System);
            var grafo = F.ReferenceGraph(editor.System);
            var generacion = editor.State.Generation;

            var resultados = new List<string>();
            foreach (var peticion in peticiones)
            {
                var preparation = editor.Prepare(peticion);
                Assert.NotNull(preparation);
                resultados.Add(preparation.Plan is HeaderBatchPlan<DynamicHeaderAddress>.Rejected rejected ? rejected.Code.ToString() : "Prepared");

                Assert.Equal(huella, F.SystemFingerprint(editor.System));
                Assert.Equal(serializado, F.Serialized(editor));
                F.AssertSameGraph(grafo, F.ReferenceGraph(editor.System));
                Assert.Equal(generacion, editor.State.Generation);
                Assert.Equal(DynamicModuleTargetMode.Explicit, editor.State.Targets.Mode);
                Assert.Equal(new[] { "M5", "M3" }, editor.State.Targets.ExplicitModuleIds);
            }

            Assert.Equal(new[] { "Prepared", "Prepared", "Prepared", "MalformedTarget", "StaleTargets" }, resultados);
            Assert.Equal(70.0, origen.Depth);
            Assert.Equal(48.0, editor.Module("M5").Length);
        }

        // ===== D-25 — orden y precedencia deterministas =================================================================

        [Fact]
        public void D25_EL_ORDEN_ES_EL_INDEX_LONGITUDINAL_Y_NUNCA_EL_ORDEN_DE_LOS_IDS()
        {
            var editor = new F.Editor(F.Design(palletsDeep: 11));
            Assert.Equal(new[] { "M1", "M3", "M5", "M7", "M9", "M11" }, editor.HeaderIds);
            editor.Customize("M5", 251.0);

            var esperado = new[] { "M1", "M3", "M9", "M11" };
            Assert.NotEqual(esperado, esperado.OrderBy(id => id, StringComparer.Ordinal));   // premisa: M1, M11, M3, M9

            var explicito = F.Prepared(editor.Prepare(editor.Distribute("M5", editor.Explicit("M11", "M9", "M3", "M1", "M5"))));
            Assert.Equal(esperado, F.Ids(explicito.Targets));
            Assert.Equal(new[] { "M5:IsSource" }, F.Omissions(explicito.Omitted));

            var preparation = editor.Prepare(editor.Distribute("M5", F.All()));
            var todas = F.Prepared(preparation);
            Assert.Equal(new[] { "M1", "M3", "M7", "M9", "M11" }, F.Ids(todas.Targets));

            var committed = F.Committed(editor.Apply(preparation));
            Assert.Same(todas.Targets, committed.Applied);
            Assert.Equal(new[] { "M1", "M3", "M7", "M9", "M11" }, F.Ids(committed.Applied));
        }

        [Fact]
        public void D25_UNA_PETICION_CON_VARIOS_DEFECTOS_DEVUELVE_EL_PRIMER_CODIGO_DE_LA_PRECEDENCIA()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 252.0);
            editor.Module("M9").Length = 0.0;
            var antes = F.SystemFingerprint(editor.System);

            var casos = new List<(string Caso, DynamicHeaderBatchRequest Peticion, HeaderRejectionCode Esperado)>
            {
                ("stale + origen inexistente + malformado",
                    F.Request(DynamicHeaderBatchRequest.Distribute(DynamicHeaderSource.Of(editor.System, 7, "M42"), editor.Explicit("M4"), null)),
                    HeaderRejectionCode.StaleTargets),
                ("origen separador + malformado",
                    editor.Distribute("M4", editor.Explicit("M42")),
                    HeaderRejectionCode.SourceNotFound),
                ("origen calculado + sin seleccion",
                    editor.Distribute("M3", F.Current()),
                    HeaderRejectionCode.SourceUnusable),
                ("EDIT sin resultado + sin seleccion",
                    F.Request(DynamicHeaderBatchRequest.Edit(null, null)),
                    HeaderRejectionCode.SourceUnusable),
                ("seleccion en un separador",
                    editor.Distribute("M1", F.Current(), "M4"),
                    HeaderRejectionCode.NoTargets),
                ("origen omitido + malformado + destino invalido",
                    editor.Distribute("M1", editor.Explicit("M1", "M4", "M9")),
                    HeaderRejectionCode.MalformedTarget),
                ("origen omitido + destino invalido",
                    editor.Distribute("M1", editor.Explicit("M1", "M9")),
                    HeaderRejectionCode.DestinationInvalid),
                ("solo el origen",
                    editor.Distribute("M1", editor.Explicit("M1")),
                    HeaderRejectionCode.NoApplicableTargets),
            };

            var obtenidos = casos.Select(caso => caso.Caso + " -> " + F.RejectedCode(editor.Prepare(caso.Peticion))).ToList();

            Assert.Equal(casos.Select(caso => caso.Caso + " -> " + caso.Esperado).ToList(), obtenidos);
            Assert.Equal(antes, F.SystemFingerprint(editor.System));
        }

        // ===== Precondicion (§3.3, §3.8): sistema resuelto no nulo =======================================================

        [Fact]
        public void SIN_SISTEMA_RESUELTO_EL_GESTO_TERMINA_ANTES_DE_PREPARE_SIN_PLAN_NI_OUTCOME()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 261.0);
            var request = editor.Distribute("M1", F.All());

            var preparation = DynamicHeaderBatch.Prepare(null, editor.State, request);

            Assert.NotNull(preparation);
            Assert.Equal(DynamicHeaderPreconditionFailure.NoResolvedSystem, preparation.PreconditionFailure);
            Assert.Null(preparation.Plan);
            Assert.Empty(preparation.PreparedCopies);
            Assert.Throws<InvalidOperationException>(() => DynamicHeaderBatch.Apply(preparation, editor.System, editor.State, editor.Builder));
        }
    }
}
