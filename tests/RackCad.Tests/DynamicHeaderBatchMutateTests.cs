using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using Xunit;
using F = RackCad.Tests.DynamicHeaderBatchFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G6 — MUTATE y RECOMPUTE del Dinamico (Proposal V2 §3.4, §3.9, §3.10, §7.8; matriz §13.4: D-02, D-03, D-04,
    /// D-08, D-11, D-12 y D-19).
    ///
    /// <para>
    /// MUTATE verifica la firma antes de la primera escritura y asigna a cada destino SU copia preparada, con
    /// <c>UseCalculatedHeaderConfiguration = false</c> y nada mas: ni longitud, ni banderas, ni tipo, ni overrides por linea.
    /// Despues, un unico recompute canonico —<c>ApplyPostPeralte</c> + <c>Refresh</c>— impone el fondo de cada modulo y el
    /// peralte del rack. Un rechazo no escribe ni recomputa.
    /// </para>
    /// </summary>
    public class DynamicHeaderBatchMutateTests
    {
        /// <summary>Lo que un destino debe tener tras distribuir: receta del origen, instancia propia, procedencia personalizada
        /// y la normalizacion del recompute (fondo = longitud, peralte del rack, derivado presente).</summary>
        private static void AssertDistributed(F.Editor editor, RackFrameConfiguration origen, string moduleId)
        {
            var module = editor.Module(moduleId);
            var cabecera = module.AssociatedFrameConfiguration;
            Assert.False(module.UseCalculatedHeaderConfiguration);
            Assert.NotSame(origen, cabecera);
            Assert.Equal(F.Recipe(origen), F.Recipe(cabecera));
            Assert.Equal(module.Length, cabecera.Depth, 6);
            Assert.Equal(editor.System.PostPeralte, cabecera.PostPeralte, 6);
            Assert.NotEmpty(cabecera.Members);
        }

        private static Dictionary<string, string> Huellas(F.Editor editor, params string[] moduleIds)
            => moduleIds.ToDictionary(id => id, id => SelectiveHeaderBatchFixtures.Configuration(editor.Module(id).AssociatedFrameConfiguration));

        private static void AssertUntouched(F.Editor editor, Dictionary<string, string> huellas)
        {
            foreach (var pair in huellas)
            {
                Assert.True(editor.Module(pair.Key).UseCalculatedHeaderConfiguration, pair.Key + " dejo de ser calculada.");
                Assert.Equal(pair.Value, SelectiveHeaderBatchFixtures.Configuration(editor.Module(pair.Key).AssociatedFrameConfiguration));
            }
        }

        // ===== D-02 / D-03 / D-04 — aplicar a uno, a varios y a todas ===================================================

        [Fact]
        public void D02_APLICAR_A_UNO_LA_CABECERA_SELECCIONADA()
        {
            var editor = new F.Editor(F.Design());
            var origen = editor.Customize("M1", 21.0);
            var otras = Huellas(editor, "M3", "M7", "M9");

            var committed = F.Committed(editor.Apply(editor.Prepare(editor.Distribute("M1", F.Current(), selectedModuleId: "M5"))));

            Assert.Equal(new[] { "M5" }, F.Ids(committed.Applied));
            Assert.Empty(committed.Omitted);
            AssertDistributed(editor, origen, "M5");
            AssertUntouched(editor, otras);
        }

        [Fact]
        public void D03_APLICAR_A_VARIOS_MODULOS_EXPLICITOS_CADA_UNO_CON_SU_COPIA_PREPARADA()
        {
            var editor = new F.Editor(F.Design());
            var origen = editor.Customize("M1", 31.0);
            var otras = Huellas(editor, "M5");

            var preparation = editor.Prepare(editor.Distribute("M1", editor.Explicit("M9", "M3", "M7")));
            var copies = F.Copies(preparation);
            var committed = F.Committed(editor.Apply(preparation));

            Assert.Equal(new[] { "M3", "M7", "M9" }, F.Ids(committed.Applied));
            for (var i = 0; i < committed.Applied.Count; i++)
            {
                // Plan.Targets[i] <-> copia[i]: MUTATE instala exactamente la copia preparada para ese destino.
                Assert.Same(copies[i], editor.Module(committed.Applied[i].ModuleId).AssociatedFrameConfiguration);
                AssertDistributed(editor, origen, committed.Applied[i].ModuleId);
            }

            Assert.Equal(3, copies.Distinct(ReferenceEqualityComparer.Instance).Count());
            AssertUntouched(editor, otras);
        }

        [Fact]
        public void D04_APLICAR_A_TODAS_LAS_CABECERAS_DE_LA_SECUENCIA_CADA_UNA_CON_SU_PROPIA_LONGITUD()
        {
            var editor = new F.Editor(F.Design());
            var origen = editor.Customize("M1", 41.0);

            var committed = F.Committed(editor.Apply(editor.Prepare(editor.Distribute("M1", F.All()))));

            Assert.Equal(new[] { "M3", "M5", "M7", "M9" }, F.Ids(committed.Applied));
            Assert.Equal(new[] { "M1:IsSource" }, F.Omissions(committed.Omitted));
            foreach (var id in F.Ids(committed.Applied))
            {
                AssertDistributed(editor, origen, id);
            }

            Assert.Equal(48.0, editor.Module("M3").AssociatedFrameConfiguration.Depth, 6);
            Assert.Equal(54.0, editor.Module("M9").AssociatedFrameConfiguration.Depth, 6);
            Assert.Empty(editor.System.HeaderLineOverrides);
            Assert.Empty(editor.System.DerivedPostLineOverrides);
        }

        // ===== D-08 — independencia (I4-I6) ==============================================================================

        [Fact]
        public void D08_ORIGEN_Y_DESTINOS_NO_COMPARTEN_ESTADO_Y_EDITAR_UNO_NO_CAMBIA_LOS_DEMAS()
        {
            var editor = new F.Editor(F.Design());
            var origen = editor.Customize("M1", HeaderConfigurationFixtures.Rich());
            Assert.NotEmpty(origen.Exceptions);

            F.Committed(editor.Apply(editor.Prepare(editor.Distribute("M1", editor.Explicit("M3", "M5")))));
            var m3 = editor.Module("M3").AssociatedFrameConfiguration;
            var m5 = editor.Module("M5").AssociatedFrameConfiguration;

            HeaderConfigurationFixtures.AssertNoSharedMutableState(origen, m3);
            HeaderConfigurationFixtures.AssertNoSharedMutableState(origen, m5);
            HeaderConfigurationFixtures.AssertNoSharedMutableState(m3, m5);

            var m3Antes = SelectiveHeaderBatchFixtures.Configuration(m3);
            var m5Antes = SelectiveHeaderBatchFixtures.Configuration(m5);
            HeaderConfigurationFixtures.MutateEverywhere(origen);                 // I4: editar el origen despues de aplicar
            Assert.Equal(m3Antes, SelectiveHeaderBatchFixtures.Configuration(m3));
            Assert.Equal(m5Antes, SelectiveHeaderBatchFixtures.Configuration(m5));

            var origenAntes = SelectiveHeaderBatchFixtures.Configuration(origen);
            HeaderConfigurationFixtures.MutateEverywhere(m3);                     // I5/I6: editar un destino
            Assert.Equal(origenAntes, SelectiveHeaderBatchFixtures.Configuration(origen));
            Assert.Equal(m5Antes, SelectiveHeaderBatchFixtures.Configuration(m5));
        }

        // ===== D-11 — fallo en el ultimo destino ========================================================================

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void D11_UN_FALLO_EN_EL_ULTIMO_DESTINO_RECHAZA_TODO_EL_LOTE_CON_MUTACION_CERO_EN_TODOS(double longitudInvalida)
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 111.0);
            var validos = new[] { "M3", "M5", "M7" };
            Assert.All(validos, id => Assert.True(editor.Module(id).Length > 0.0));
            editor.Module("M9").Length = longitudInvalida;
            Assert.Equal("M9", editor.HeaderIds.Last());                           // premisa: el ULTIMO en orden de Index

            var instancias = validos.ToDictionary(id => id, id => editor.Module(id).AssociatedFrameConfiguration);
            var antes = F.SystemFingerprint(editor.System);
            var grafo = F.ReferenceGraph(editor.System);

            var preparation = editor.Prepare(editor.Distribute("M1", editor.Explicit("M3", "M5", "M7", "M9")));

            Assert.Equal(HeaderRejectionCode.DestinationInvalid, F.RejectedCode(preparation));
            Assert.Empty(preparation.PreparedCopies);
            Assert.Equal(antes, F.SystemFingerprint(editor.System));

            Assert.Equal(HeaderRejectionCode.DestinationInvalid, F.RejectedCode(editor.Apply(preparation)));
            Assert.Equal(antes, F.SystemFingerprint(editor.System));
            F.AssertSameGraph(grafo, F.ReferenceGraph(editor.System));            // ni escrituras ni recompute
            foreach (var pair in instancias)
            {
                Assert.True(editor.Module(pair.Key).UseCalculatedHeaderConfiguration);
                Assert.Same(pair.Value, editor.Module(pair.Key).AssociatedFrameConfiguration);
            }
        }

        // ===== D-12 — DISTRIBUTE no toca estructura, longitud ni banderas ===============================================

        [Fact]
        public void D12_DISTRIBUTE_NO_CREA_MODULOS_NI_CAMBIA_TIPOS_Y_SU_MUTATE_NO_TOCA_LONGITUD_NI_BANDERAS()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 121.0);
            editor.ManualLength("M3", 50.0);                                       // destino con fondo manual
            editor.ManualLength("M4", 46.0);                                       // separador con fondo manual
            var antes = editor.System.Modules.Select(Escalares).ToList();

            var committed = F.Committed(editor.Apply(editor.Prepare(editor.Distribute("M1", F.All()))));

            Assert.Equal(new[] { "M3", "M5", "M7", "M9" }, F.Ids(committed.Applied));
            Assert.Equal(antes, editor.System.Modules.Select(Escalares).ToList());
            Assert.Equal(50.0, editor.Module("M3").Length);
            Assert.True(editor.Module("M3").IsManualOverride);
            Assert.False(editor.Module("M3").IsCalculated);
            Assert.False(editor.Module("M5").IsManualOverride);
            Assert.True(editor.Module("M5").IsCalculated);
            Assert.All(F.Ids(committed.Applied), id => Assert.False(editor.Module(id).UseCalculatedHeaderConfiguration));
        }

        private static string Escalares(DynamicRackModule module)
            => string.Join(
                "|",
                module.ModuleId,
                module.Kind,
                module.Index.ToString(CultureInfo.InvariantCulture),
                module.Length.ToString("R", CultureInfo.InvariantCulture),
                module.StartX.ToString("R", CultureInfo.InvariantCulture),
                module.IsManualOverride,
                module.IsCalculated,
                module.Notes);

        // ===== D-19 — PREPARE no normaliza; un recompute tras Committed =================================================

        [Fact]
        public void D19_PREPARE_NO_NORMALIZA_Y_EL_RECOMPUTE_TRAS_COMMITTED_IMPONE_FONDO_PERALTE_DEL_RACK_Y_DERIVADO()
        {
            var editor = new F.Editor(F.Design());
            var origen = editor.Customize("M1", 191.0);
            var peralteDelRack = editor.System.PostPeralte;
            Assert.True(peralteDelRack > 0.0);
            origen.PostPeralte = peralteDelRack + 1.5;                             // un origen fuera de la norma del rack
            new BracingPanelMemberBuilder().RefreshPhysicalModel(origen);
            Assert.Equal(54.0, origen.Depth, 6);

            var preparation = editor.Prepare(editor.Distribute("M1", F.All()));
            var plan = F.Prepared(preparation);
            var copies = F.Copies(preparation);

            // PREPARE no normaliza: cada copia sale con el fondo y el peralte del ORIGEN, y con su derivado.
            Assert.All(copies, copy => Assert.Equal(54.0, copy.Depth, 6));
            Assert.All(copies, copy => Assert.Equal(peralteDelRack + 1.5, copy.PostPeralte, 6));
            var derivadoPreparadoM3 = string.Join("/", HeaderConfigurationFixtures.MembersFingerprint(copies[0]));
            Assert.Equal("M3", plan.Targets[0].ModuleId);

            var committed = F.Committed(editor.Apply(preparation));

            Assert.Equal(new[] { "M3", "M5", "M7", "M9" }, F.Ids(committed.Applied));
            for (var i = 0; i < plan.Targets.Count; i++)
            {
                var module = editor.Module(plan.Targets[i].ModuleId);
                var cabecera = module.AssociatedFrameConfiguration;
                Assert.Same(copies[i], cabecera);
                Assert.Equal(module.Length, cabecera.Depth, 6);
                Assert.Equal(peralteDelRack, cabecera.PostPeralte, 6);
                // El derivado es el de los valores impuestos: el mismo que reconstruye su copia canonica.
                Assert.Equal(
                    HeaderConfigurationFixtures.MembersFingerprint(new RackFrameProjectStore().DeepCopy(cabecera)),
                    HeaderConfigurationFixtures.MembersFingerprint(cabecera));
            }

            Assert.NotEqual(derivadoPreparadoM3, string.Join("/", HeaderConfigurationFixtures.MembersFingerprint(editor.Module("M3").AssociatedFrameConfiguration)));
            Assert.Equal(peralteDelRack, editor.System.PostPeralte, 6);
        }

        // ===== Vida del plan (§3.9) =====================================================================================

        [Fact]
        public void UN_PLAN_VIVE_UN_SOLO_GESTO_Y_SOLO_SE_APLICA_CON_EL_ESTADO_QUE_LO_PREPARO()
        {
            var editor = new F.Editor(F.Design());
            editor.Customize("M1", 201.0);

            var preparation = editor.Prepare(editor.Distribute("M1", editor.Explicit("M3")));
            F.Committed(editor.Apply(preparation));
            Assert.Throws<InvalidOperationException>(() => editor.Apply(preparation));

            var otra = editor.Prepare(editor.Distribute("M1", editor.Explicit("M5")));
            Assert.Throws<InvalidOperationException>(
                () => DynamicHeaderBatch.Apply(otra, editor.System, new DynamicHeaderBatchState(), editor.Builder));
            Assert.True(editor.Module("M5").UseCalculatedHeaderConfiguration);
        }
    }
}
