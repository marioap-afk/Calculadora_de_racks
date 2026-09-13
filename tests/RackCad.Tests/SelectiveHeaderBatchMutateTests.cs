using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using Xunit;
using F = RackCad.Tests.SelectiveHeaderBatchFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G4 — MUTATE del Selectivo y su atomicidad (Proposal V2 §3.4, §3.9, §3.10, §6.7, §6.9; RR-01 reglas 5 y 6).
    ///
    /// <para>
    /// MUTATE no decide nada: verifica la firma ANTES de la primera asignacion y, si coincide, asigna en orden las copias
    /// que PREPARE ya preparo —sin copiar, validar ni resolver—, mas el escalar de peralte precomputado de un EDIT. Si la
    /// firma cambio, <c>StaleTargets</c> y ninguna escritura. Cubre S-02, S-03, S-04, S-08, S-12, S-17 y S-21, mas la
    /// correspondencia 1:1 entre Targets y copias y la vida de un solo gesto del plan.
    /// </para>
    /// </summary>
    public class SelectiveHeaderBatchMutateTests
    {
        // ===== S-02 / S-03 / S-04 — aplicar a uno, a varios y a todos ==================================================

        [Fact]
        public void S02_APLICAR_A_UNO_ES_EL_POSTE_ACTUAL_POR_EL_FONDO_ACTUAL()
        {
            var editor = new F.Editor(F.State(3, 3));
            F.StoreCustom(editor.State, 0, 1, F.Custom(180.0));
            editor.Recompute();
            var postTargets = new SelectivePostTargets(); // Actual
            editor.State.FollowCurrentFondo();

            var preparation = editor.Prepare(SelectiveHeaderBatchRequest.Distribute(F.At(0, 1), postTargets.Resolve(editor.State, currentPost: 2)));
            var committed = F.Committed(editor.Mutate(preparation));

            Assert.Equal(new[] { F.At(0, 2) }, committed.Applied);
            var stored = editor.State.CabeceraAt(0, 2);
            Assert.NotNull(stored);
            Assert.Equal(180.0, stored.Height, 6);
            Assert.Equal(editor.State.CabeceraDepthOfFondo(0), stored.Depth, 6);
            Assert.NotSame(editor.State.CabeceraAt(0, 1), stored);
            Assert.Null(editor.State.CabeceraAt(1, 2));
        }

        [Fact]
        public void S03_APLICAR_A_POSTES_EXPLICITOS_POR_FONDOS_EXPLICITOS()
        {
            var editor = new F.Editor(F.State(3, 3, 3));
            F.StoreCustom(editor.State, 1, 0, F.Custom(175.0));
            editor.Recompute();
            editor.State.SetTargetFondos(new[] { 0, 2 });
            var postTargets = new SelectivePostTargets();
            postTargets.SetTargetPosts(new[] { 3, 1 }, editor.State);

            var committed = F.Committed(editor.Mutate(editor.Prepare(
                SelectiveHeaderBatchRequest.Distribute(F.At(1, 0), postTargets.Resolve(editor.State, currentPost: 0)))));

            Assert.Equal(new[] { F.At(0, 1), F.At(0, 3), F.At(2, 1), F.At(2, 3) }, committed.Applied);
            Assert.All(committed.Applied, address => Assert.Equal(175.0, editor.State.CabeceraAt(address.FondoIndex, address.PostIndex).Height, 6));
            Assert.Null(editor.State.CabeceraAt(1, 1)); // un fondo que no es objetivo no recibe nada
            Assert.Null(editor.State.CabeceraAt(0, 2)); // ni un poste que no es objetivo
        }

        [Fact]
        public void S04_APLICAR_A_TODOS_POR_TODOS_CON_FONDOS_DE_DISTINTA_LONGITUD()
        {
            var editor = new F.Editor(F.State(3, 1, 2));
            F.StoreCustom(editor.State, 0, 0, F.Custom(185.0));
            editor.Recompute();
            editor.State.FollowAllFondos();
            var postTargets = new SelectivePostTargets();
            postTargets.FollowAllPosts();

            var preparation = editor.Prepare(SelectiveHeaderBatchRequest.Distribute(F.At(0, 0), postTargets.Resolve(editor.State, currentPost: 0)));
            var plan = F.Prepared(preparation);
            var committed = F.Committed(editor.Mutate(preparation));

            var expected = new[]
            {
                F.At(0, 1), F.At(0, 2), F.At(0, 3),
                F.At(1, 0), F.At(1, 1),
                F.At(2, 0), F.At(2, 1), F.At(2, 2),
            };
            Assert.Equal(expected, committed.Applied);
            Assert.Equal(
                new[] { (F.At(0, 0), HeaderOmissionReason.IsSource), (F.At(1, 2), HeaderOmissionReason.AbsentInScope), (F.At(1, 3), HeaderOmissionReason.AbsentInScope), (F.At(2, 3), HeaderOmissionReason.AbsentInScope) },
                plan.Omitted.Select(omission => (omission.Address, omission.Reason)));
            Assert.All(expected, address => Assert.Equal(185.0, editor.State.CabeceraAt(address.FondoIndex, address.PostIndex).Height, 6));
        }

        // ===== Correspondencia 1:1 y MUTATE sin copiar =================================================================

        [Fact]
        public void G4_MUTATE_ASIGNA_EN_CADA_TARGET_SU_COPIA_PREPARADA_TAL_CUAL_SIN_COPIAR_DE_NUEVO()
        {
            var editor = new F.Editor(F.State(3, 2));
            F.StoreCustom(editor.State, 0, 0, F.Custom(180.0));
            editor.Recompute();
            editor.State.FollowAllFondos();

            var preparation = editor.Prepare(F.Distribute(F.At(0, 0), 1, 2));
            var plan = F.Prepared(preparation);
            var copies = F.Copies(preparation);
            var committed = F.Committed(editor.Mutate(preparation));

            Assert.Equal(plan.Targets, committed.Applied);
            for (var i = 0; i < plan.Targets.Count; i++)
            {
                Assert.Same(copies[i], editor.State.CabeceraAt(plan.Targets[i].FondoIndex, plan.Targets[i].PostIndex));
            }

            Assert.Equal(copies.Count, copies.Distinct().Count()); // nunca una materializacion reutilizada entre destinos
        }

        [Fact]
        public void G4_UN_PLAN_VIVE_UN_SOLO_GESTO_Y_SOLO_SE_APLICA_AL_ESTADO_QUE_LO_PREPARO()
        {
            var editor = new F.Editor(F.State(3));
            F.StoreCustom(editor.State, 0, 0, F.Custom(180.0));
            editor.Recompute();
            var otro = new F.Editor(F.State(3));
            F.StoreCustom(otro.State, 0, 0, F.Custom(180.0));
            otro.Recompute();

            var preparation = editor.Prepare(F.Distribute(F.At(0, 0), 1));
            var otroBefore = F.StateFingerprint(otro.State);
            Assert.Throws<InvalidOperationException>(() => otro.State.ApplyHeaderBatch(preparation, otro.Current));
            Assert.Equal(otroBefore, F.StateFingerprint(otro.State));

            F.Committed(editor.Mutate(preparation));
            var after = F.StateFingerprint(editor.State);
            Assert.Throws<InvalidOperationException>(() => editor.Mutate(preparation));
            Assert.Throws<InvalidOperationException>(() => preparation.Cancel());
            Assert.Equal(after, F.StateFingerprint(editor.State));
        }

        // ===== S-08 — independencia ====================================================================================

        [Fact]
        public void S08_ORIGEN_Y_DESTINOS_QUEDAN_INDEPENDIENTES_ENTRE_SI()
        {
            var editor = new F.Editor(F.State(3, 3));
            F.StoreCustom(editor.State, 0, 0, HeaderConfigurationFixtures.Rich());
            editor.Recompute();
            editor.State.FollowAllFondos();

            var committed = F.Committed(editor.Mutate(editor.Prepare(F.Distribute(F.At(0, 0), 1, 2))));
            var source = editor.State.CabeceraAt(0, 0);
            var destinations = committed.Applied.Select(address => editor.State.CabeceraAt(address.FondoIndex, address.PostIndex)).ToList();

            Assert.Equal(4, destinations.Count);
            foreach (var destination in destinations)
            {
                HeaderConfigurationFixtures.AssertNoSharedMutableState(source, destination);
            }

            for (var i = 0; i < destinations.Count; i++)
            {
                for (var j = i + 1; j < destinations.Count; j++)
                {
                    HeaderConfigurationFixtures.AssertNoSharedMutableState(destinations[i], destinations[j]);
                }
            }

            // I4: editar el origen despues no mueve a los destinos.
            var destinationsBefore = destinations.Select(F.Configuration).ToList();
            HeaderConfigurationFixtures.MutateEverywhere(source);
            Assert.Equal(destinationsBefore, destinations.Select(F.Configuration).ToList());

            // I5/I6: editar un destino no mueve a los demas.
            HeaderConfigurationFixtures.MutateEverywhere(destinations[0]);
            Assert.Equal(destinationsBefore.Skip(1).ToList(), destinations.Skip(1).Select(F.Configuration).ToList());
        }

        // ===== S-12 — todo o nada =====================================================================================

        [Fact]
        public void S12_SI_EL_ULTIMO_DESTINO_ES_INVALIDO_SE_RECHAZA_TODO_EL_LOTE_SIN_ESCRIBIR_NINGUNO()
        {
            var editor = new F.Editor(F.State(3, 3, 3));
            F.StoreCustom(editor.State, 0, 0, F.Custom(180.0));
            editor.Recompute();
            editor.State.FollowAllFondos();

            // El ULTIMO fondo del orden de informe no tiene una profundidad de cabecera valida; los anteriores si.
            editor.State.FondoMatrices[2].CabeceraOverride = double.PositiveInfinity;
            Assert.True(double.IsInfinity(editor.State.CabeceraDepthOfFondo(2)));
            Assert.All(new[] { 0, 1 }, fondo =>
            {
                var depth = editor.State.CabeceraDepthOfFondo(fondo);
                Assert.True(double.IsFinite(depth) && depth > 0.0);
            });

            var before = F.StateFingerprint(editor.State);
            var instancesBefore = F.StoredInstances(editor.State);

            var preparation = editor.Prepare(F.Distribute(F.At(0, 0), 1, 2, 3));
            Assert.Equal(HeaderRejectionCode.DestinationInvalid, F.RejectedCode(preparation));
            Assert.Equal(HeaderRejectionCode.DestinationInvalid, F.RejectedCode(editor.Mutate(preparation)));

            Assert.Equal(before, F.StateFingerprint(editor.State));
            F.AssertSameInstances(instancesBefore, F.StoredInstances(editor.State));
            Assert.All(new[] { F.At(0, 1), F.At(0, 3), F.At(1, 1), F.At(1, 3) }, address =>
                Assert.Null(editor.State.CabeceraAt(address.FondoIndex, address.PostIndex)));
        }

        [Fact]
        public void S12_UNA_PROFUNDIDAD_DE_CABECERA_NO_POSITIVA_RECHAZA_TODO_EL_LOTE_SIN_ESCRIBIR()
        {
            // El unico <= 0 que produce la autoridad real: un fondo sin slot comprometido (CabeceraDepthOfFondo = 0).
            var reference = new F.Editor(F.State(3));
            F.StoreCustom(reference.State, 0, 0, F.Custom(180.0));
            reference.Recompute();

            var state = new SelectiveEditorState { DefaultBeamId = F.BeamId };
            state.InitMatrix(3, 2); // la matriz viva, sin slot todavia
            F.StoreCustom(state, 0, 0, F.Custom(180.0));
            Assert.Equal(0.0, state.CabeceraDepthOfFondo(0));
            var before = F.StateFingerprint(state);

            var preparation = SelectiveHeaderBatchPlanner.Prepare(state, reference.Current, F.Distribute(F.At(0, 0), 1, 2, 3));

            Assert.Equal(HeaderRejectionCode.DestinationInvalid, F.RejectedCode(preparation));
            Assert.Equal(HeaderRejectionCode.DestinationInvalid, F.RejectedCode(state.ApplyHeaderBatch(preparation, reference.Current)));
            Assert.Equal(before, F.StateFingerprint(state));
        }

        // ===== S-17 — PostPeraltes =====================================================================================

        [Fact]
        public void S17_DISTRIBUTE_NUNCA_ESCRIBE_POSTPERALTES()
        {
            var editor = new F.Editor(F.State(3, 3));
            editor.State.PostPeraltes[1] = 5.5;
            F.StoreCustom(editor.State, 0, 1, F.Custom(180.0));
            editor.Recompute();
            editor.State.FollowAllFondos();
            var peraltes = editor.State.PostPeraltes.ToList();

            F.Committed(editor.Mutate(editor.Prepare(F.Distribute(F.At(0, 1), 0, 2, 3))));

            Assert.Equal(peraltes, editor.State.PostPeraltes);
        }

        [Fact]
        public void S17_EDIT_ESCRIBE_EL_PERALTE_PRECOMPUTADO_DEL_POSTE_VISIBLE_SOLO_AL_QUEDAR_APLICADO()
        {
            var editor = new F.Editor(F.State(3, 1));
            editor.Recompute();
            editor.State.FollowAllFondos();
            var peraltes = editor.State.PostPeraltes.ToList();

            // Aplicado en el fondo 0 (tiene el poste 2) y omitido en el fondo 1 (postes 0..1).
            var result = F.Custom(190.0);
            result.PostPeralte = 6.25;
            var preparation = editor.Prepare(SelectiveHeaderBatchRequest.Edit(result, 2));
            var plan = F.Prepared(preparation);
            Assert.Equal(new[] { F.At(0, 2) }, plan.Targets);
            Assert.Equal(peraltes, editor.State.PostPeraltes); // PREPARE no escribe

            F.Committed(editor.Mutate(preparation));

            var expected = peraltes.ToList();
            expected[2] = 6.25;
            Assert.Equal(expected, editor.State.PostPeraltes);

            // Igual al peralte del tramo: se guarda 0 (hereda el del tramo), la regla historica del editor. El poste 1
            // arranca con un override propio para que esa escritura se vea.
            editor.State.PostPeraltes[1] = 4.0;
            editor.Recompute();
            var igual = F.Custom(190.0);
            igual.PostPeralte = F.RunPeralte;
            F.Committed(editor.Mutate(editor.Prepare(SelectiveHeaderBatchRequest.Edit(igual, 1))));
            expected[1] = 0.0;
            Assert.Equal(expected, editor.State.PostPeraltes);
        }

        // ===== S-21 — firma anti-stale (RR-01 reglas 5 y 6) ============================================================

        [Theory]
        [InlineData("generacion")]
        [InlineData("topologia")]
        [InlineData("profundidad")]
        [InlineData("peralte")]
        [InlineData("pendiente")]
        [InlineData("sin-sistema")]
        public void S21_SI_CAMBIA_LO_QUE_PREPARE_LEYO_MUTATE_ES_STALETARGETS_SIN_ESCRIBIR(string cambio)
        {
            var editor = new F.Editor(F.State(3, 3));
            F.StoreCustom(editor.State, 0, 1, F.Custom(180.0));
            editor.Recompute();
            editor.State.FollowAllFondos();
            var preparation = editor.Prepare(F.Distribute(F.At(0, 1), 2, 3));
            F.Prepared(preparation);

            SelectiveHeaderResolution current;
            switch (cambio)
            {
                case "generacion": // el sistema se reconstruyo, aunque la topologia coincida
                    current = SelectiveHeaderResolution.Of(editor.System, editor.Generation + 1, false);
                    break;
                case "topologia":
                    editor.State.SetTargetFondos(new[] { 1 });
                    editor.State.ApplyBayCountToTargets(2);
                    editor.State.FollowAllFondos();
                    current = editor.Current;
                    break;
                case "profundidad":
                    editor.State.FondoMatrices[1].CabeceraOverride = 50.0;
                    current = editor.Current;
                    break;
                case "peralte": // otra resolucion con el mismo numero de generacion
                    editor.State.PostPeraltes[3] = 6.0;
                    var otroSistema = new RackCad.Application.Systems.Selective.SelectiveGeometryResolver()
                        .Resolve(editor.Design(), F.Catalog);
                    current = SelectiveHeaderResolution.Of(otroSistema, editor.Generation, false);
                    break;
                case "pendiente":
                    current = SelectiveHeaderResolution.Of(editor.System, editor.Generation, true);
                    break;
                default:
                    current = SelectiveHeaderResolution.Of(null, editor.Generation, false);
                    break;
            }

            var before = F.StateFingerprint(editor.State);
            var outcome = editor.State.ApplyHeaderBatch(preparation, current);

            Assert.Equal(HeaderRejectionCode.StaleTargets, F.RejectedCode(outcome));
            Assert.Equal(before, F.StateFingerprint(editor.State));
            Assert.Null(editor.State.CabeceraAt(0, 2));
            Assert.Null(editor.State.CabeceraAt(1, 3));
        }

        [Fact]
        public void S21_SIN_CAMBIOS_LA_FIRMA_COINCIDE_Y_EL_LOTE_SE_COMPROMETE()
        {
            var editor = new F.Editor(F.State(3, 3));
            F.StoreCustom(editor.State, 0, 1, F.Custom(180.0));
            editor.Recompute();
            editor.State.FollowAllFondos();
            var preparation = editor.Prepare(F.Distribute(F.At(0, 1), 2, 3));

            // Una resolucion equivalente pero construida aparte (misma generacion y mismo sistema) no es stale.
            var equivalente = SelectiveHeaderResolution.Of(editor.System, editor.Generation, false);
            var committed = F.Committed(editor.State.ApplyHeaderBatch(preparation, equivalente));

            Assert.Equal(4, committed.Applied.Count);
            Assert.NotNull(editor.State.CabeceraAt(0, 2));
            Assert.NotNull(editor.State.CabeceraAt(1, 3));
        }
    }
}
