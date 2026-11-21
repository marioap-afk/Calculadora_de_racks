using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;
using static RackCad.UI.Tests.PushBackWorkspaceSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4c (owner-pbt4c-list-4-decision.md; D-PBT4-17, A-T2b-OWNER-01): the equivalent of CT-PBT2B-06 of
    /// PushBackWorkspaceApplyCloseTests, whose Skip is in the closed list of PB-T4c. It is that test with the members of its file that it
    /// reaches (Sesion, Nueva and Cierre), literal, with one change: the expected line of window 4, where a staged module and unapplied changes
    /// are pending together, is the one question that names both (WorkspaceStagedAndUnappliedQuestion of the window).
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackEditAllApplyCloseTests
    {
        /// <summary>Una ventana de WORKSPACE con su borrador, su puerto contado y el estado del borrador de antes de abrirla.</summary>
        private sealed class Sesion
        {
            public RackPushBackSystemWindow W;
            public DraftSession<PushBackEditableState> Draft;
            public PuertoContado Puerto;
            public PushBackEditableState Antes;
        }

        private static Sesion Nueva(List<string> log, PushBackEditableState estado, RackProject proyecto = null)
        {
            var (_, draft) = Borrador(estado);
            var puerto = new PuertoContado(draft);
            var antes = Copy(draft.CurrentDraftState);
            var w = Paso(log, () => VentanaBorrador(draft, proyecto, puerto));
            return w == null ? null : new Sesion { W = w, Draft = draft, Puerto = puerto, Antes = antes };
        }

        /// <summary>Una ventana de WORKSPACE sobre su propio borrador: sonda, gestos y, en el <c>finally</c>, «Si» y <c>Cerrar</c>.</summary>
        private static bool Cierre(List<string> log, PromptProbe avisos, Action<Sesion> gestos)
        {
            var s = Nueva(log, Estado(Fixture(DisenoBase)));
            if (s == null) return false;
            try
            {
                avisos.Reiniciar();
                if (!Sondeo(log, s.W)) return false;
                gestos(s);
                return true;
            }
            finally
            {
                // `CloseQuietly` de T-CL no cierra una ventana sin mostrar: se contesta «Si» y se cierra con `Cerrar`.
                avisos.Answer = true;
                CerrarVentana(s.W);
            }
        }

        [Fact]
        public void CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied()
        {
            var log = new List<string>();
            var avisos = new PromptProbe(true);
            // El primer Sta del proceso crea el hilo STA e instala en el su aviso de descarte permanente: se arranca ANTES de sustituir
            // el aviso de esta prueba, o el primero que corra en el proceso la dejaria sin su sustituto.
            Sta(() => { });
            using (EditorDiscardPrompt.Substitute(avisos.Ask))
            {
                Sta(() =>
                {
                    // Ventana 1: sin cambios, «Cerrar» cierra sin preguntar.
                    if (!Cierre(log, avisos, s =>
                    {
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-sin-cambios avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(s.W)
                            + " borrador-igual=" + Igual(s.Draft.CurrentDraftState, s.Antes));
                    }))
                    {
                        return;
                    }

                    // Ventana 2: una edicion sin aplicar; «No» la deja abierta, «Si» la cierra y el borrador queda como estaba.
                    if (!Cierre(log, avisos, s =>
                    {
                        SetNumberR(s.W, "ToleranceBox", 4.5);
                        avisos.Answer = false;
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-con-cambios-no avisos=" + avisos.Asked + " pregunta=" + avisos.Question + " cerrada=" + EstaCerradaVentana(s.W));
                        avisos.Answer = true;
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-con-cambios-si avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(s.W)
                            + " borrador-igual=" + Igual(s.Draft.CurrentDraftState, s.Antes) + " llamadas=" + s.Puerto.Llamadas);
                    }))
                    {
                        return;
                    }

                    // Ventana 3: lo editado se aplico, y «Cerrar» no pregunta.
                    if (!Cierre(log, avisos, s =>
                    {
                        SetNumberR(s.W, "ToleranceBox", 4.5);
                        Click(s.W, "UpdateButton");
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-tras-aplicar avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(s.W) + " llamadas=" + s.Puerto.Llamadas);
                    }))
                    {
                        return;
                    }

                    // Ventana 4: un modulo escenificado sin confirmar manda primero, con su texto de siempre.
                    if (!Cierre(log, avisos, s =>
                    {
                        StageModuleEdit(s.W);
                        avisos.Answer = true;
                        Click(s.W, "CloseButton");
                        log.Add("cerrar-con-modulo avisos=" + avisos.Asked + " pregunta=" + avisos.Question + " cerrada=" + EstaCerradaVentana(s.W));
                    }))
                    {
                        return;
                    }

                    // CLASSIC: «Cerrar» no pregunta por lo editado, como siempre (ADR-0029 D8).
                    var clasica = Ventana();
                    try
                    {
                        avisos.Reiniciar();
                        Cargar(clasica, Fixture(DisenoBase));
                        SetNumberR(clasica, "ToleranceBox", 4.5);
                        Click(clasica, "CloseButton");
                        log.Add("clasico-con-cambios avisos=" + avisos.Asked + " cerrada=" + EstaCerradaVentana(clasica)
                            + " peticion=" + clasica.InsertRequested);
                    }
                    finally
                    {
                        avisos.Answer = true;
                        CerrarVentana(clasica);
                    }
                });
            }

            var esperadas = new List<string>
            {
                "cerrar-sin-cambios avisos=0 cerrada=True borrador-igual=True",
                "cerrar-con-cambios-no avisos=1 pregunta=" + PreguntaSinAplicar + " cerrada=False",
                "cerrar-con-cambios-si avisos=2 cerrada=True borrador-igual=True llamadas=0",
                "cerrar-tras-aplicar avisos=0 cerrada=True llamadas=1",
                "cerrar-con-modulo avisos=1 pregunta=Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los dos al cerrar. ¿Deseas continuar? cerrada=True",
                "clasico-con-cambios avisos=0 cerrada=True peticion=False",
            };

            Assert.Equal(esperadas, log);
        }
    }
}
