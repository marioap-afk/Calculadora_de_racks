using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Workspace;
using RackCad.UI.Shell;
using RackCad.UI.Systems.PushBack;
using RackCad.UI.Workspace;
using Xunit;
using static RackCad.UI.Tests.PushBackDeclaredCorrectionSupport;
using static RackCad.UI.Tests.PushBackFullEditorCharacterizationSupport;
using static RackCad.UI.Tests.PushBackWorkspaceSupport;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4c — the gesture «Editar todo» of the field of the panel and the question of «Cerrar» of the window (CT-PBT4C-02, 06, 09
    /// and 14). The field is the REAL view-model over a double of the edit port that lives in this class: it records every call and answers what
    /// the recipe gives it, and its «Editar todo» answers Closed without applying unless the recipe says otherwise (CT-02, 06 and 14). CT-09
    /// reads <c>PendingWork()</c> of the REAL window, in WORKSPACE and in CLASSIC, without showing it and without closing it. Nothing reads or
    /// writes the drawing, nothing opens a dialog and nothing touches the profile. Every call goes through <c>Paso</c> (the «EXCEPCION» rule), so
    /// a behavior that is missing fails by ASSERTION on the lines. Every window is created without showing it, with its load inside the
    /// <c>try</c> and its close in the <c>finally</c>; the notice of discard is always substituted.
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackEditAllWindowTests
    {
        private static readonly SessionId Session = new SessionId(7);

        private const string Rack = "R-1";

        // ---------------------------------------------------------------------------------------------- the double of the port

        /// <summary>The edit port of the recipes: it records every operation by name, answers the view and the commit it is given and Closed
        /// (without applying) to «Editar todo» unless it is given another answer.</summary>
        private sealed class PortDouble : IWorkspacePilotEditPort
        {
            public List<string> Calls { get; } = new List<string>();

            public PilotFieldView View { get; set; }

            public PilotFieldResult Commit { get; set; }

            public Func<EditAllOutcome> Answer { get; set; }

            public EditIntentRequest Request { get; private set; }

            public int Count(string operation) => Calls.Count(call => call == operation);

            public EditAllOutcome TryEditAll(EditIntentRequest request)
            {
                Calls.Add("TryEditAll");
                Request = request;
                return Answer == null ? EditAllOutcome.Closed(0) : Answer();
            }

            public PilotFieldView GetField(WorkspaceRequest origin, string rackId)
            {
                Calls.Add("GetField");
                return View;
            }

            public PilotFieldResult CommitField(WorkspaceRequest origin, string rackId)
            {
                Calls.Add("CommitField");
                return Commit;
            }

            public EditIntentOutcome TryBeginEdit(EditIntentRequest request)
            {
                Calls.Add("TryBeginEdit");
                return default(EditIntentOutcome);
            }

            public EditCheckOutcome TryCheckNow(EditIntentRequest request)
            {
                Calls.Add("TryCheckNow");
                return default(EditCheckOutcome);
            }

            public EditIntentOutcome TryReload(EditIntentRequest request)
            {
                Calls.Add("TryReload");
                return default(EditIntentOutcome);
            }

            public EditConfirmOutcome TryConfirmBase(EditIntentRequest request)
            {
                Calls.Add("TryConfirmBase");
                return default(EditConfirmOutcome);
            }

            public EditIntentOutcome TryKeepMine(EditIntentRequest request)
            {
                Calls.Add("TryKeepMine");
                return default(EditIntentOutcome);
            }

            public EditIntentOutcome TryDiscardAndReload(EditIntentRequest request)
            {
                Calls.Add("TryDiscardAndReload");
                return default(EditIntentOutcome);
            }

            public PilotFieldResult SetPendingText(WorkspaceRequest origin, string rackId, string text)
            {
                Calls.Add("SetPendingText");
                return default(PilotFieldResult);
            }

            public PilotBoundaryResult CrossBoundary(WorkspaceRequest origin, string rackId, PilotBoundaryKind kind)
            {
                Calls.Add("CrossBoundary");
                return default(PilotBoundaryResult);
            }

            public EditDiscardOutcome DiscardRack(WorkspaceRequest origin, string rackId)
            {
                Calls.Add("DiscardRack");
                return default(EditDiscardOutcome);
            }

            public EditDiscardOutcome DiscardDocument(WorkspaceRequest origin)
            {
                Calls.Add("DiscardDocument");
                return default(EditDiscardOutcome);
            }

            public PilotRiskView GetRacksAtRisk(WorkspaceRequest origin)
            {
                Calls.Add("GetRacksAtRisk");
                return default(PilotRiskView);
            }

            public IReadOnlyList<PilotLossView> TakeLossNotices()
            {
                Calls.Add("TakeLossNotices");
                return new List<PilotLossView>();
            }

            public IReadOnlyList<PilotCloseView> TakeCloseNotices()
            {
                Calls.Add("TakeCloseNotices");
                return new List<PilotCloseView>();
            }

            public EditUpdateStart RequestUpdate(EditIntentRequest request)
            {
                Calls.Add("RequestUpdate");
                return default(EditUpdateStart);
            }

            public EditUpdateProgress TakeUpdateProgress(WorkspaceRequest origin, string rackId)
            {
                Calls.Add("TakeUpdateProgress");
                return default(EditUpdateProgress);
            }
        }

        /// <summary>A draft that exists, with committed text 1:50 and, when given, a pending text and its message.</summary>
        private static PilotFieldView ViewOf(PilotFieldState state, bool canEdit, string pending = null, string message = null)
            => new PilotFieldView(true, state, false, state != PilotFieldState.Clean, canEdit, "1:50", null, pending, message);

        /// <summary>The field over the double, on the surface of the rack of the session, with the calls of the first paint forgotten.</summary>
        private static (WorkspacePilotFieldViewModel Field, PortDouble Port, Func<int> Changed) FieldOver(PilotFieldView view, bool isPreview = false)
        {
            var port = new PortDouble { View = view };
            var changed = 0;
            var field = new WorkspacePilotFieldViewModel(port, () => changed++);
            field.Show(Session, Rack, isPreview);
            port.Calls.Clear();
            changed = 0;
            return (field, port, () => changed);
        }

        // ------------------------------------------------------------------------------------------------- CT-02

        [Fact]
        public void CT_PBT4C_02_EditAllCommitsThePendingTextFirstAndOnlyOpensTheEditorWhenTheTextIsAppliedOrTheFieldIsNotEditable()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var (field, port, _) = FieldOver(ViewOf(PilotFieldState.Dirty, true, "abc", "Valor no numérico."));
                port.Commit = PilotFieldResult.Invalid(ViewOf(PilotFieldState.Dirty, true, "abc", "Valor no numérico."), "Valor no numérico.");
                field.EditAll();
                log.Add("invalido TryEditAll=" + port.Count("TryEditAll") + " mensaje=[" + field.MessageText + "] aviso=[" + field.NoticeText + "]");
            });

            Paso(log, () =>
            {
                var (field, port, _) = FieldOver(ViewOf(PilotFieldState.Dirty, true, "2.5"));
                port.Commit = PilotFieldResult.Busy("Hay una lectura del piloto en curso.");
                field.EditAll();
                log.Add("ocupado TryEditAll=" + port.Count("TryEditAll") + " aviso=[" + field.NoticeText + "]");
            });

            Paso(log, () =>
            {
                var (field, port, _) = FieldOver(ViewOf(PilotFieldState.Dirty, true, "2.5"));
                port.Commit = PilotFieldResult.Rejected("La petición no es del documento activo.");
                field.EditAll();
                log.Add("rechazado TryEditAll=" + port.Count("TryEditAll") + " aviso=[" + field.NoticeText + "]");
            });

            Paso(log, () =>
            {
                var (field, port, _) = FieldOver(ViewOf(PilotFieldState.Dirty, true, "2.5"));
                port.Commit = PilotFieldResult.Applied(ViewOf(PilotFieldState.Dirty, true));
                field.EditAll();
                log.Add("valido TryEditAll=" + port.Count("TryEditAll") + " aviso=[" + field.NoticeText + "]");
            });

            Paso(log, () =>
            {
                var (field, port, _) = FieldOver(ViewOf(PilotFieldState.Dirty, true, "2.5"));
                port.Commit = PilotFieldResult.NotEditable("El campo no se puede editar ahora.", ViewOf(PilotFieldState.Dirty, true));
                field.EditAll();
                log.Add("no-editable TryEditAll=" + port.Count("TryEditAll") + " aviso=[" + field.NoticeText + "]");
            });

            Finish(
                log,
                new[]
                {
                    "invalido TryEditAll=0 mensaje=[Valor no numérico.] aviso=[No se abrió el editor: Valor no numérico.]",
                    "ocupado TryEditAll=0 aviso=[El texto no se aplicó: Hay una lectura del piloto en curso.]",
                    "rechazado TryEditAll=0 aviso=[El texto no se aplicó: La petición no es del documento activo.]",
                    "valido TryEditAll=1 aviso=[Editor cerrado sin cambios en el borrador.]",
                    "no-editable TryEditAll=1 aviso=[Editor cerrado sin cambios en el borrador.]",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-06

        [Fact]
        public void CT_PBT4C_06_WhenTheEditorClosesTheFieldShowsTheSummaryAndModifiedAndTheBrowserIsToldOnce()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var (field, port, changed) = FieldOver(ViewOf(PilotFieldState.Clean, true));
                port.Answer = () =>
                {
                    port.View = ViewOf(PilotFieldState.Dirty, true);
                    return EditAllOutcome.Closed(1);
                };
                field.EditAll();
                log.Add("con-aplicar estado=[" + field.StateText + "] aviso=[" + field.NoticeText + "] cambiado=" + changed());
            });

            Paso(log, () =>
            {
                var (field, port, changed) = FieldOver(ViewOf(PilotFieldState.Clean, true));
                port.Answer = () => EditAllOutcome.Closed(0);
                field.EditAll();
                log.Add("sin-aplicar estado=[" + field.StateText + "] aviso=[" + field.NoticeText + "] cambiado=" + changed());
            });

            Finish(
                log,
                new[]
                {
                    "con-aplicar estado=[Modificado] aviso=[Editor cerrado: sus cambios están en el borrador.] cambiado=1",
                    "sin-aplicar estado=[Limpio] aviso=[Editor cerrado sin cambios en el borrador.] cambiado=1",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-14

        [Fact]
        public void CT_PBT4C_14_TheFieldCallsThePortInItsOrderWithTheRequestOfTheGestureAndOffersTheButtonOnlyWhereTheEditorOpens()
        {
            var log = new List<string>();

            Paso(log, () =>
            {
                var (field, port, changed) = FieldOver(ViewOf(PilotFieldState.Dirty, true, "2.5"));
                port.Commit = PilotFieldResult.Applied(ViewOf(PilotFieldState.Dirty, true));
                field.EditAll();
                log.Add("con-texto llamadas=[" + string.Join(",", port.Calls) + "] cambiado=" + changed());
            });

            Paso(log, () =>
            {
                var (field, port, changed) = FieldOver(ViewOf(PilotFieldState.Clean, true));
                field.EditAll();
                log.Add("sin-texto llamadas=[" + string.Join(",", port.Calls) + "] cambiado=" + changed());
                var request = port.Request;
                log.Add("peticion sesion-del-campo=" + (request != null && request.Session == Session) + " rack-del-campo=" + (request != null && request.RackId == Rack)
                    + " referencia=" + (request == null || request.ChosenReference == null ? "null" : request.ChosenReference.Value.ToString()) + " testigo="
                    + (request == null || request.Witness == null ? "null" : "dado"));
            });

            Paso(log, () =>
            {
                var (field, port, _) = FieldOver(default(PilotFieldView));
                var withoutDraft = field.ShowEditAllButton;
                port.View = ViewOf(PilotFieldState.Clean, true);
                field.Show(Session, Rack, false);
                var editable = field.ShowEditAllButton;
                port.View = ViewOf(PilotFieldState.Unknown, false);
                field.Show(Session, Rack, false);
                var notEditable = field.ShowEditAllButton;
                port.View = ViewOf(PilotFieldState.Clean, true);
                field.Show(Session, Rack, true);
                var preview = field.ShowEditAllButton;
                log.Add("boton sin-borrador=" + withoutDraft + " editable=" + editable + " no-editable=" + notEditable + " vista-previa=" + preview);
            });

            Finish(
                log,
                new[]
                {
                    "con-texto llamadas=[GetField,CommitField,TryEditAll,GetField] cambiado=1",
                    "sin-texto llamadas=[GetField,TryEditAll,GetField] cambiado=1",
                    "peticion sesion-del-campo=True rack-del-campo=True referencia=null testigo=null",
                    "boton sin-borrador=True editable=True no-editable=False vista-previa=False",
                });
        }

        // ------------------------------------------------------------------------------------------------- CT-09

        private static string Question(EditorPendingWork work) => work.HasPendingWork ? "[" + work.Question + "]" : "ninguna";

        /// <summary>The staged header of a module, with the seam of D-S7 (no configurator is built): a copy of the project with another PanelClear.</summary>
        private static void Header(RackPushBackSystemWindow w)
        {
            SelectModule(w, "M3");
            w.HeaderConfiguratorDialog = argument =>
            {
                var edited = new RackFrameProjectStore().DeepCopy(argument);
                edited.PanelClear = 40.0;
                return edited;
            };
            Click(w, "ConfigureModuleHeaderButton");
        }

        /// <summary>One window of WORKSPACE over the draft of a fixture: the gesture, and the line of what «Cerrar» would ask (PendingWork, without closing).</summary>
        private static void InWorkspace(List<string> log, string label, string fixture, Action<RackPushBackSystemWindow> gesture)
        {
            var (_, draft) = Borrador(Estado(Fixture(fixture)));
            var w = VentanaBorrador(draft, null, new PuertoContado(draft));
            try
            {
                gesture(w);
                var work = w.PendingWork();
                log.Add(label == "ninguna" ? label + " pendiente=" + work.HasPendingWork : label + " pregunta=" + Question(work));
            }
            catch (Exception ex)
            {
                log.Add(label + " EXCEPCION " + ex.GetType().Name + ": " + ex.Message);
            }
            finally
            {
                CerrarVentana(w);
            }
        }

        [Fact]
        public void CT_PBT4C_09_ClosingInWorkspaceAsksOneQuestionWhenAStagedModuleAndUnappliedChangesWouldBeLostTogether()
        {
            var log = new List<string>();
            var avisos = new PromptProbe(true);

            // The first Sta of the process creates the STA thread and installs its permanent notice of discard: it starts BEFORE this test
            // substitutes the notice, or the first one that runs in the process would leave it without its substitute.
            Sta(() => { });
            using (EditorDiscardPrompt.Substitute(avisos.Ask))
            {
                Sta(() =>
                {
                    InWorkspace(log, "ambas", "pb-31-v2-modulos", Header);
                    InWorkspace(log, "solo-sin-aplicar", DisenoBase, w => SetNumberR(w, "ToleranceBox", 4.5));
                    InWorkspace(log, "ninguna", DisenoBase, w => { });

                    var clasica = Ventana();
                    try
                    {
                        Cargar(clasica, Fixture("pb-31-v2-modulos"));
                        Header(clasica);
                        log.Add("clasico-modulo pregunta=" + Question(clasica.PendingWork()));
                    }
                    catch (Exception ex)
                    {
                        log.Add("clasico-modulo EXCEPCION " + ex.GetType().Name + ": " + ex.Message);
                    }
                    finally
                    {
                        avisos.Answer = true;
                        CerrarVentana(clasica);
                    }
                });
            }

            Finish(
                log,
                new[]
                {
                    "ambas pregunta=[Hay un cambio de módulo sin confirmar y cambios sin aplicar al borrador; se perderán los dos al cerrar. ¿Deseas continuar?]",
                    "solo-sin-aplicar pregunta=[Hay cambios sin aplicar al borrador que se perderán al cerrar. ¿Deseas continuar?]",
                    "ninguna pendiente=False",
                    "clasico-modulo pregunta=[Hay cambios de módulo sin confirmar que se perderán al cerrar. ¿Deseas continuar?]",
                });
        }

        private static void Finish(List<string> log, IEnumerable<string> expected)
            => Assert.Equal(string.Join("\n", expected), string.Join("\n", log));
    }
}
