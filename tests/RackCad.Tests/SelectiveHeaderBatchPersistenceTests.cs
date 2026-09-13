using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Selective;
using Xunit;
using F = RackCad.Tests.SelectiveHeaderBatchFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G4 — lo que una distribucion del Selectivo deja en el rack tiene que sobrevivir a todo lo que ya existe:
    /// el BOM por la ruta de <c>RACKBOMTOTAL</c>, la reapertura de <c>RACKEDITAR</c>, guardar y reabrir, y el resultado de
    /// «Personalizar» por la ruta historica (Proposal V2 §13.3: S-22, S-23, S-24 y S-25).
    ///
    /// <para>
    /// Todo en Core y sin UI: la parte de <c>RACKEDITAR</c> que se ejercita es la de Application
    /// (<see cref="SelectiveEditorOpen"/> sobre el documento guardado), que es la que decide que diseno recibe el editor.
    /// </para>
    /// </summary>
    public class SelectiveHeaderBatchPersistenceTests
    {
        private const string RackId = "5b8f3c2a-9d41-4e7b-8a60-1c2d3e4f5a6b";

        /// <summary>Un rack de tres fondos con una personalizada distribuida desde (0, 1) a los postes 2 y 3 de todos.</summary>
        private static (F.Editor Editor, IReadOnlyList<SelectiveHeaderAddress> Applied) Distributed()
        {
            var editor = new F.Editor(F.State(F.Fondo(3, depth: 48.0), F.Fondo(3, depth: 60.0), F.Fondo(3, depth: 72.0)));
            F.StoreCustom(editor.State, 0, 1, F.Custom(200.0));
            editor.Recompute();
            editor.State.FollowAllFondos();

            var committed = F.Committed(editor.Mutate(editor.Prepare(F.Distribute(F.At(0, 1), 2, 3))));
            editor.Recompute();
            return (editor, committed.Applied);
        }

        private static SelectivePalletDesignDocument SaveAndRead(SelectivePalletDesign design)
        {
            var store = new SelectivePalletDesignStore();
            return store.Deserialize(store.Serialize(SelectivePalletDesignDocument.From(design, RackId, "Rack I-53")));
        }

        // ===== S-22 — BOM del editor = BOM por la ruta de RACKBOMTOTAL ==================================================

        [Fact]
        public void S22_EL_BOM_DEL_EDITOR_ES_EL_DE_LA_RUTA_DE_RACKBOMTOTAL_CON_PERSONALIZADAS_DISTRIBUIDAS()
        {
            var sinDistribuir = new F.Editor(F.State(F.Fondo(3, depth: 48.0), F.Fondo(3, depth: 60.0), F.Fondo(3, depth: 72.0)));
            F.StoreCustom(sinDistribuir.State, 0, 1, F.Custom(200.0));
            sinDistribuir.Recompute();

            var (editor, _) = Distributed();
            var bomDelEditor = F.BomSignature(editor.System);
            Assert.NotEqual(F.BomSignature(sinDistribuir.System), bomDelEditor); // las distribuidas SI se cotizan

            // store -> autoridad efectiva (sin registro de variables) -> resolver -> builder, como SelectiveKindHandler.
            var authored = SaveAndRead(editor.Design());
            var efectivo = new SelectiveEffectiveDesignResolver().Resolve(authored, null);
            Assert.True(efectivo.IsSuccess);
            var bomRackBomTotal = F.BomSignature(new SelectiveGeometryResolver().Resolve(efectivo.Design, F.Catalog));

            Assert.Equal(bomDelEditor, bomRackBomTotal);
        }

        // ===== S-23 — RACKEDITAR (parte de Application) ==================================================================

        [Fact]
        public void S23_RACKEDITAR_REABRE_CADA_PERSONALIZADA_DISTRIBUIDA_EN_SU_FONDO_Y_POSTE_COMO_INSTANCIA_PROPIA()
        {
            var (editor, applied) = Distributed();
            var distribuidas = applied.ToDictionary(address => address, address => F.Configuration(editor.State.CabeceraAt(address.FondoIndex, address.PostIndex)));

            var open = SelectiveEditorOpen.Resolve(SaveAndRead(editor.Design()), ProjectVariablesReadResult.Absent());
            Assert.True(open.IsOpen);
            var reabierto = open.Design;

            // Procedencia: exactamente el origen y los destinos distribuidos son personalizadas; nada mas.
            var personalizadas = new List<SelectiveHeaderAddress>();
            for (var fondo = 0; fondo < 3; fondo++)
            {
                for (var post = 0; post <= 3; post++)
                {
                    if (SelectiveCabeceraAuthority.CustomAt(reabierto, fondo, post) != null) personalizadas.Add(F.At(fondo, post));
                }
            }

            Assert.Equal(new[] { F.At(0, 1) }.Concat(applied).OrderBy(address => address).ToList(), personalizadas);

            // Configuraciones identicas a las distribuidas y ninguna compartida con otra.
            var instancias = applied.Select(address => SelectiveCabeceraAuthority.CustomAt(reabierto, address.FondoIndex, address.PostIndex)).ToList();
            for (var i = 0; i < applied.Count; i++)
            {
                Assert.Equal(
                    HeaderConfigurationFixtures.Wire(editor.State.CabeceraAt(applied[i].FondoIndex, applied[i].PostIndex)),
                    HeaderConfigurationFixtures.Wire(instancias[i]));
                for (var j = i + 1; j < applied.Count; j++)
                {
                    Assert.NotSame(instancias[i], instancias[j]);
                }
            }

            Assert.Equal(distribuidas.Count, instancias.Distinct().Count());
        }

        // ===== S-24 — guardar y reabrir ==================================================================================

        [Fact]
        public void S24_GUARDAR_Y_REABRIR_CONSERVA_LAS_DISTRIBUIDAS_SU_PROFUNDIDAD_POR_FONDO_Y_EL_BOM()
        {
            var (editor, applied) = Distributed();
            var design = editor.Design();
            var restaurado = SaveAndRead(design).ToDomain();

            var antes = new SelectiveGeometryResolver().Resolve(design, F.Catalog);
            var despues = new SelectiveGeometryResolver().Resolve(restaurado, F.Catalog);

            var instancias = new List<RackFrameConfiguration>();
            foreach (var address in applied)
            {
                var original = SelectiveCabeceraAuthority.CustomAt(design, address.FondoIndex, address.PostIndex);
                var reabierta = SelectiveCabeceraAuthority.CustomAt(restaurado, address.FondoIndex, address.PostIndex);
                Assert.NotNull(reabierta);
                Assert.Equal(HeaderConfigurationFixtures.Wire(original), HeaderConfigurationFixtures.Wire(reabierta));
                Assert.Equal(
                    SelectiveCabeceraAuthority.EffectiveCustomAt(antes, address.FondoIndex, address.PostIndex).Depth,
                    SelectiveCabeceraAuthority.EffectiveCustomAt(despues, address.FondoIndex, address.PostIndex).Depth,
                    6);
                instancias.Add(reabierta);
            }

            Assert.Equal(instancias.Count, instancias.Distinct().Count());
            Assert.Equal(new[] { 42.0, 54.0, 66.0 }, applied.Where(a => a.PostIndex == 2).Select(a => SelectiveCabeceraAuthority.CustomAt(restaurado, a.FondoIndex, 2).Depth));
            Assert.Equal(F.BomSignature(antes), F.BomSignature(despues));
        }

        // ===== S-25 — «Personalizar» por el contrato = por la ruta historica ============================================

        [Fact]
        public void S25_PERSONALIZAR_UN_POSTE_POR_EL_CONTRATO_DEJA_EL_MISMO_ESTADO_QUE_LA_RUTA_HISTORICA()
        {
            F.Editor Nuevo()
            {
                var editor = new F.Editor(F.State(F.Fondo(3, depth: 48.0), F.Fondo(3, depth: 60.0)));
                editor.State.PostPeraltes[1] = 4.0;
                editor.Recompute();
                editor.State.SetTargetFondos(new[] { 0, 1 });
                return editor;
            }

            RackFrameConfiguration Configurado(F.Editor editor)
            {
                var cfg = F.Custom(190.0);
                cfg.PostPeralte = 6.25;
                cfg.Depth = editor.State.CabeceraDepthOfFondo(0); // la ventana lo siembra desde el fondo visible
                return cfg;
            }

            // Ruta historica: la mitad de «Personalizar» posterior al configurador, sin el dialogo.
            var historica = Nuevo();
            var cfgHistorica = Configurado(historica);
            historica.State.PostPeraltes[2] = (cfgHistorica.PostPeralte > 0.0 && System.Math.Abs(cfgHistorica.PostPeralte - F.RunPeralte) > 1e-6)
                ? cfgHistorica.PostPeralte
                : 0.0;
            historica.State.ApplyCabeceraToTargets(2, cfgHistorica, F.DeepCopy);

            // Ruta del contrato: PREPARE + MUTATE.
            var contrato = Nuevo();
            F.Committed(contrato.Mutate(contrato.Prepare(SelectiveHeaderBatchRequest.Edit(Configurado(contrato), 2))));

            Assert.Equal(F.StateFingerprint(historica.State), F.StateFingerprint(contrato.State));
        }
    }
}
