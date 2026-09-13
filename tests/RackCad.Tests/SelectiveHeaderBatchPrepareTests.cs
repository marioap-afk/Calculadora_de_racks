using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using Xunit;
using F = RackCad.Tests.SelectiveHeaderBatchFixtures;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 G4 — PREPARE del Selectivo sobre el nucleo compartido de G3 (Proposal V2 §3.3-§3.8, §6.2-§6.4; ADR-0037).
    ///
    /// <para>
    /// PREPARE es la unica fase que puede fallar: resuelve la direccion de origen contra la resolucion vigente, captura
    /// su valor ACTUAL, cruza <c>TargetFondos x PostTargets</c>, omite lo que la topologia no tiene, valida y materializa
    /// una copia por destino. Y no toca nada: ni el estado del editor, ni el sistema resuelto, ni una sola cabecera
    /// guardada. Cubre S-01, S-05, S-06, S-07, S-09, S-10, S-11, S-13, S-19, S-20 y la parte de Application de RR-01.
    /// </para>
    /// </summary>
    public class SelectiveHeaderBatchPrepareTests
    {
        // ===== S-01 — solo es origen una personalizada usable ===========================================================

        [Fact]
        public void S01_UN_ORIGEN_ESTANDAR_SIN_ALTURA_O_NO_USABLE_ES_SOURCEUNUSABLE_SIN_MUTAR()
        {
            var editor = new F.Editor(F.State(3, 3));
            var sinAltura = F.Custom(180.0);
            sinAltura.Height = 0.0;
            F.StoreCustom(editor.State, 0, 2, sinAltura);
            var noUsable = F.Custom(180.0);
            noUsable.Depth = 0.0; // UsableCustomAt la acepta (tiene altura) pero no es una cabecera usable
            F.StoreCustom(editor.State, 1, 1, noUsable);
            editor.Recompute();
            editor.State.FollowAllFondos();

            foreach (var source in new[] { F.At(0, 1) /* estandar */, F.At(0, 2) /* sin altura */, F.At(1, 1) /* no usable */ })
            {
                var before = F.StateFingerprint(editor.State);

                Assert.Equal(HeaderRejectionCode.SourceUnusable, F.RejectedCode(editor.Prepare(F.Distribute(source, 3))));
                Assert.Equal(before, F.StateFingerprint(editor.State));
            }
        }

        [Fact]
        public void S01_UNA_PERSONALIZADA_USABLE_ES_ORIGEN_Y_SE_PREPARA()
        {
            var editor = new F.Editor(F.State(3, 3));
            F.StoreCustom(editor.State, 0, 1, F.Custom(180.0));
            editor.Recompute();

            var plan = F.Prepared(editor.Prepare(F.Distribute(F.At(0, 1), 2)));

            Assert.Equal(new[] { F.At(0, 2) }, plan.Targets);
        }

        // ===== S-05 — la direccion origen se omite en su posicion ======================================================

        [Fact]
        public void S05_LA_DIRECCION_ORIGEN_SE_OMITE_COMO_ISSOURCE_EN_SU_POSICION_DEL_ORDEN()
        {
            var editor = new F.Editor(F.State(3, 3));
            F.StoreCustom(editor.State, 0, 1, F.Custom(180.0));
            editor.Recompute();
            editor.State.FollowAllFondos();

            var plan = F.Prepared(editor.Prepare(F.Distribute(F.At(0, 1), 0, 1, 2)));

            Assert.Equal(new[] { F.At(0, 0), F.At(0, 2), F.At(1, 0), F.At(1, 1), F.At(1, 2) }, plan.Targets);
            var omission = Assert.Single(plan.Omitted);
            Assert.Equal(F.At(0, 1), omission.Address);
            Assert.Equal(HeaderOmissionReason.IsSource, omission.Reason);
        }

        // ===== S-06 — el origen es el valor ACTUAL de su direccion =====================================================

        [Fact]
        public void S06_RECORDAR_LA_DIRECCION_Y_EDITAR_DESPUES_EL_ORIGEN_APLICA_EL_VALOR_NUEVO()
        {
            var editor = new F.Editor(F.State(3, 3));
            F.StoreCustom(editor.State, 0, 1, F.Custom(180.0));
            editor.Recompute();

            // La UI recuerda la DIRECCION, nunca una configuracion.
            var recordada = F.At(0, 1);

            // Despues el usuario edita esa cabecera por la ruta que existe hoy («Personalizar»), y el editor recompone.
            editor.State.FollowCurrentFondo();
            editor.State.ApplyCabeceraToTargets(1, F.Custom(205.0), F.DeepCopy);
            editor.Recompute();

            editor.State.FollowAllFondos();
            var committed = F.Committed(editor.Mutate(editor.Prepare(F.Distribute(recordada, 2, 3))));

            Assert.Equal(4, committed.Applied.Count);
            Assert.All(committed.Applied, address =>
                Assert.Equal(205.0, editor.State.CabeceraAt(address.FondoIndex, address.PostIndex).Height, 6));
        }

        // ===== S-07 — origen desaparecido ==============================================================================

        [Theory]
        [InlineData("poste")]
        [InlineData("fondo")]
        public void S07_UN_ORIGEN_QUE_YA_NO_EXISTE_ES_SOURCENOTFOUND_Y_NO_MUTA(string cambio)
        {
            var editor = new F.Editor(F.State(3, 3, 3));
            F.StoreCustom(editor.State, 2, 3, F.Custom(190.0));
            editor.Recompute();
            Assert.NotNull(editor.State.CabeceraAt(2, 3)); // premisa

            if (cambio == "poste")
            {
                editor.State.SetTargetFondos(new[] { 2 });
                editor.State.ApplyBayCountToTargets(1); // el fondo 2 se queda con postes 0..1
            }
            else
            {
                editor.State.FondoMatrices.RemoveAt(2);
                editor.State.SyncTargetFondos();
                editor.State.SyncPostCabeceras();
            }

            editor.Recompute();
            editor.State.FollowAllFondos();
            var before = F.StateFingerprint(editor.State);

            Assert.Equal(HeaderRejectionCode.SourceNotFound, F.RejectedCode(editor.Prepare(F.Distribute(F.At(2, 3), 0, 1))));
            Assert.Equal(before, F.StateFingerprint(editor.State));
        }

        // ===== S-09 / S-10 — omitir, nunca crear ni clampear ============================================================

        [Fact]
        public void S09_UN_FONDO_OBJETIVO_QUE_NO_EXISTE_SE_OMITE_COMO_ABSENTINSCOPE_NUNCA_SE_RECHAZA()
        {
            var editor = new F.Editor(F.State(3, 3, 3));
            F.StoreCustom(editor.State, 0, 1, F.Custom(180.0));
            editor.State.SetTargetFondos(new[] { 0, 1, 2 });

            // El rack pierde el fondo 2 y nadie ha resincronizado todavia los objetivos.
            editor.State.FondoMatrices.RemoveAt(2);
            editor.State.SyncPostCabeceras();
            editor.Recompute();
            Assert.Equal(new[] { 0, 1, 2 }, editor.State.TargetFondos.Fondos); // premisa

            var plan = F.Prepared(editor.Prepare(F.Distribute(F.At(0, 1), 2)));

            Assert.Equal(new[] { F.At(0, 2), F.At(1, 2) }, plan.Targets);
            var omission = Assert.Single(plan.Omitted);
            Assert.Equal(F.At(2, 2), omission.Address);
            Assert.Equal(HeaderOmissionReason.AbsentInScope, omission.Reason);
        }

        [Fact]
        public void S10_UN_POSTE_AUSENTE_EN_UN_FONDO_SE_OMITE_SIN_CREARLO_NI_CLAMPEARLO_A_UN_VECINO()
        {
            var editor = new F.Editor(F.State(3, 1, 3));
            F.StoreCustom(editor.State, 0, 0, F.Custom(180.0));
            editor.Recompute();
            editor.State.SetTargetFondos(new[] { 0, 1, 2 });

            var preparation = editor.Prepare(F.Distribute(F.At(0, 0), 3));
            var plan = F.Prepared(preparation);

            Assert.Equal(new[] { F.At(0, 3), F.At(2, 3) }, plan.Targets);
            var omission = Assert.Single(plan.Omitted);
            Assert.Equal(F.At(1, 3), omission.Address);
            Assert.Equal(HeaderOmissionReason.AbsentInScope, omission.Reason);

            F.Committed(editor.Mutate(preparation));

            Assert.Null(editor.State.CabeceraAt(1, 3));
            Assert.Null(editor.State.CabeceraAt(1, 1)); // ni clampeado al ultimo poste real del fondo 1
            Assert.True(editor.State.ExtraFondoPostCabeceras[0].Count <= 2, "La fila del fondo 1 crecio mas alla de sus postes.");
        }

        [Fact]
        public void S10_LO_DISTRIBUIDO_A_UN_POSTE_QUE_DESAPARECE_NO_RESUCITA_AL_VOLVER_A_CRECER()
        {
            var editor = new F.Editor(F.State(3, 3));
            F.StoreCustom(editor.State, 0, 0, F.Custom(180.0));
            editor.Recompute();
            editor.State.FollowAllFondos();
            F.Committed(editor.Mutate(editor.Prepare(F.Distribute(F.At(0, 0), 3))));
            Assert.NotNull(editor.State.CabeceraAt(1, 3));

            editor.State.SetTargetFondos(new[] { 1 });
            editor.State.ApplyBayCountToTargets(1); // el fondo 1 pierde los postes 2 y 3
            editor.State.ApplyBayCountToTargets(3); // y los recupera
            editor.Recompute();

            Assert.Null(editor.State.CabeceraAt(1, 3));
        }

        // ===== S-11 — todo omitido =====================================================================================

        [Fact]
        public void S11_SI_TODO_QUEDA_OMITIDO_ES_NOAPPLICABLETARGETS_Y_POSTPERALTES_NO_SE_TOCA()
        {
            var editor = new F.Editor(F.State(2, 2));
            F.StoreCustom(editor.State, 0, 1, F.Custom(180.0));
            editor.Recompute();
            editor.State.FollowAllFondos();
            var before = F.StateFingerprint(editor.State);
            var peraltes = editor.State.PostPeraltes.ToList();

            // EDIT sobre un poste que ningun fondo objetivo tiene (postes 0..2): hoy la ventana escribiria el peralte
            // antes de aplicar (L-7). En el contrato no se escribe nada.
            var result = F.Custom(190.0);
            result.PostPeralte = 7.5;
            var edit = editor.Prepare(SelectiveHeaderBatchRequest.Edit(result, 5));
            Assert.Equal(HeaderRejectionCode.NoApplicableTargets, F.RejectedCode(edit));
            Assert.Equal(HeaderRejectionCode.NoApplicableTargets, F.RejectedCode(editor.Mutate(edit)));

            // DISTRIBUTE cuyo unico destino es el propio origen.
            editor.State.FollowCurrentFondo();
            var distribute = editor.Prepare(F.Distribute(F.At(0, 1), 1));
            Assert.Equal(HeaderRejectionCode.NoApplicableTargets, F.RejectedCode(distribute));

            editor.State.FollowAllFondos();
            Assert.Equal(before, F.StateFingerprint(editor.State));
            Assert.Equal(peraltes, editor.State.PostPeraltes);
        }

        // ===== S-13 — PREPARE es puro ==================================================================================

        [Fact]
        public void S13_PREPARE_NO_MUTA_ESTADO_NI_SISTEMA_NI_NORMALIZA_NINGUNA_CABECERA_GUARDADA()
        {
            var editor = new F.Editor(F.State(F.Fondo(3, depth: 48.0), F.Fondo(3, depth: 60.0)));
            F.StoreCustom(editor.State, 1, 1, F.Custom(180.0, 54.0));
            editor.Recompute();

            // Centinela de EffectiveCustomAt / ImposeFondoDepth: la receta guardada lleva una profundidad que su fondo no
            // dicta. Si PREPARE la normalizara en sitio, cambiaria (el sistema resuelto comparte la instancia).
            var guardada = editor.State.CabeceraAt(1, 1);
            guardada.Depth = 30.0;

            // Centinelas de SyncPostCabeceras: entradas mas alla de los postes del fondo 0 y peraltes cortos.
            editor.State.PostCabeceras.Add(null);
            editor.State.PostCabeceras.Add(F.Custom(170.0));
            editor.State.PostPeraltes.RemoveAt(editor.State.PostPeraltes.Count - 1);

            editor.State.FollowAllFondos();
            var stateBefore = F.StateFingerprint(editor.State);
            var instancesBefore = F.StoredInstances(editor.State);
            var systemBefore = F.SystemFingerprint(editor.System);
            var generation = editor.Generation;

            var distribute = editor.Prepare(F.Distribute(F.At(1, 1), 0, 1, 2, 3));
            var edit = editor.Prepare(SelectiveHeaderBatchRequest.Edit(F.Custom(175.0), 2));

            F.Prepared(distribute);
            F.Prepared(edit);
            Assert.Equal(stateBefore, F.StateFingerprint(editor.State));
            F.AssertSameInstances(instancesBefore, F.StoredInstances(editor.State));
            Assert.Equal(systemBefore, F.SystemFingerprint(editor.System));
            Assert.Equal(30.0, guardada.Depth);
            Assert.Equal(generation, editor.Generation);

            // Lo que SI se normalizo son las copias privadas, que ninguna instancia guardada comparte.
            var plan = F.Prepared(distribute);
            var copies = F.Copies(distribute);
            for (var i = 0; i < plan.Targets.Count; i++)
            {
                Assert.Equal(editor.State.CabeceraDepthOfFondo(plan.Targets[i].FondoIndex), copies[i].Depth, 6);
                Assert.DoesNotContain(instancesBefore, stored => ReferenceEquals(stored, copies[i]));
            }
        }

        // ===== S-19 — orden determinista ===============================================================================

        [Fact]
        public void S19_TARGETS_OMITTED_WARNINGS_Y_APPLIED_SALEN_EN_ORDEN_FONDO_POSTE()
        {
            var editor = new F.Editor(F.State(F.Fondo(3, levels: 4), F.Fondo(1), F.Fondo(3)));
            F.StoreCustom(editor.State, 0, 0, F.Custom(100.0));
            editor.Recompute();
            editor.State.SetTargetFondos(new[] { 2, 0, 1 });

            // Postes pedidos desordenados y repetidos: el orden lo decide el Selectivo, no el orden del clic.
            var preparation = editor.Prepare(F.Distribute(F.At(0, 0), 3, 1, 2, 1));
            var plan = F.Prepared(preparation);

            var expectedTargets = new[] { F.At(0, 1), F.At(0, 2), F.At(0, 3), F.At(1, 1), F.At(2, 1), F.At(2, 2), F.At(2, 3) };
            Assert.Equal(expectedTargets, plan.Targets);
            Assert.Equal(new[] { F.At(1, 2), F.At(1, 3) }, plan.Omitted.Select(omission => omission.Address));
            Assert.True(plan.Warnings.Count >= 2, "La fixture debe producir varios avisos para que el orden signifique algo.");
            Assert.Equal(
                plan.Warnings.Select(warning => warning.Address).OrderBy(address => address).ToList(),
                plan.Warnings.Select(warning => warning.Address).ToList());

            var committed = F.Committed(editor.Mutate(preparation));
            Assert.Equal(expectedTargets, committed.Applied);
        }

        // ===== S-20 — precedencia determinista =========================================================================

        [Fact]
        public void S20_UNA_PETICION_CON_VARIOS_DEFECTOS_DEVUELVE_EL_PRIMER_CODIGO_DE_LA_PRECEDENCIA()
        {
            var editor = new F.Editor(F.State(3, 1));
            F.StoreCustom(editor.State, 0, 1, F.Custom(180.0));
            editor.Recompute();
            editor.State.FollowAllFondos();

            // SourceNotFound gana a SourceUnusable, NoTargets y MalformedTarget.
            Assert.Equal(HeaderRejectionCode.SourceNotFound, F.RejectedCode(editor.Prepare(F.Distribute(F.At(1, 3)))));
            Assert.Equal(HeaderRejectionCode.SourceNotFound, F.RejectedCode(editor.Prepare(F.Distribute(F.At(5, 0), -1))));

            // SourceUnusable gana a NoTargets y MalformedTarget.
            Assert.Equal(HeaderRejectionCode.SourceUnusable, F.RejectedCode(editor.Prepare(F.Distribute(F.At(0, 2)))));
            Assert.Equal(HeaderRejectionCode.SourceUnusable, F.RejectedCode(editor.Prepare(F.Distribute(F.At(0, 2), -1))));
            Assert.Equal(HeaderRejectionCode.SourceUnusable, F.RejectedCode(editor.Prepare(SelectiveHeaderBatchRequest.Edit(null, -1))));

            // NoTargets: la intencion no tiene postes.
            Assert.Equal(HeaderRejectionCode.NoTargets, F.RejectedCode(editor.Prepare(F.Distribute(F.At(0, 1)))));

            // MalformedTarget gana a NoApplicableTargets y a DestinationInvalid.
            Assert.Equal(HeaderRejectionCode.MalformedTarget, F.RejectedCode(editor.Prepare(F.Distribute(F.At(0, 1), -3, 9))));
            editor.State.FondoMatrices[1].CabeceraOverride = double.PositiveInfinity;
            Assert.Equal(HeaderRejectionCode.MalformedTarget, F.RejectedCode(editor.Prepare(F.Distribute(F.At(0, 1), -1, 0))));

            // NoApplicableTargets y DestinationInvalid se excluyen: cero aplicables frente a un aplicable invalido.
            Assert.Equal(HeaderRejectionCode.NoApplicableTargets, F.RejectedCode(editor.Prepare(F.Distribute(F.At(0, 1), 9))));
            Assert.Equal(HeaderRejectionCode.DestinationInvalid, F.RejectedCode(editor.Prepare(F.Distribute(F.At(0, 1), 0, 9))));
        }

        // ===== RR-01 (parte de Application) — la resolucion leida es la vigente =========================================

        [Fact]
        public void RR01_SIN_SISTEMA_RESUELTO_O_CON_RECOMPUTE_PENDIENTE_EL_GESTO_TERMINA_ANTES_DE_PREPARE()
        {
            var editor = new F.Editor(F.State(3, 3));
            F.StoreCustom(editor.State, 0, 1, F.Custom(180.0));
            editor.Recompute();
            editor.State.FollowAllFondos();
            var before = F.StateFingerprint(editor.State);
            var request = F.Distribute(F.At(0, 1), 2);

            var sinSistema = SelectiveHeaderBatchPlanner.Prepare(editor.State, SelectiveHeaderResolution.Of(null, editor.Generation, false), request);
            var pendiente = SelectiveHeaderBatchPlanner.Prepare(editor.State, SelectiveHeaderResolution.Of(editor.System, editor.Generation, true), request);
            var nula = SelectiveHeaderBatchPlanner.Prepare(editor.State, null, request);

            Assert.NotNull(sinSistema);
            Assert.Equal(SelectiveHeaderPreconditionFailure.NoResolvedSystem, sinSistema.PreconditionFailure);
            Assert.Null(sinSistema.Plan);
            Assert.NotNull(pendiente);
            Assert.Equal(SelectiveHeaderPreconditionFailure.ResolutionNotCurrent, pendiente.PreconditionFailure);
            Assert.Null(pendiente.Plan);
            Assert.NotNull(nula);
            Assert.Equal(SelectiveHeaderPreconditionFailure.NoResolvedSystem, nula.PreconditionFailure);
            Assert.Null(nula.Plan);

            // Sin plan no hay Outcome: no se puede aplicar ni cancelar.
            Assert.Throws<InvalidOperationException>(() => editor.State.ApplyHeaderBatch(pendiente, editor.Current));
            Assert.Throws<InvalidOperationException>(() => pendiente.Cancel());
            Assert.Equal(before, F.StateFingerprint(editor.State));
        }

        [Fact]
        public void RR01_LA_RESOLUCION_VIGENTE_EXIGE_SISTEMA_Y_NINGUN_RECOMPUTE_PENDIENTE_NI_DIFERIDO()
        {
            var editor = new F.Editor(F.State(3));
            editor.Recompute();

            var vigente = SelectiveHeaderResolution.Of(editor.System, 7, false);
            Assert.True(vigente.IsCurrent);
            Assert.Same(editor.System, vigente.System);
            Assert.Equal(7, vigente.Generation);
            Assert.False(SelectiveHeaderResolution.Of(editor.System, 7, true).IsCurrent);
            Assert.False(SelectiveHeaderResolution.Of(null, 7, false).IsCurrent);
        }
    }
}
