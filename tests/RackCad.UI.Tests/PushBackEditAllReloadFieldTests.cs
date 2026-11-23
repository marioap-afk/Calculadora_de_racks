using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4e (D-PBT4-22) — what the field of the panel shows when «Editar todo» meets a clean draft that may be stale
    /// (CT-PBT4E-03). The reload runs in the host, so the field only asks the port for «Editar todo», as today; when the port answers that the
    /// work that already existed was not reloaded, the field shows what «Editar» shows for the same answer of its reload: the reason, the hint
    /// of RACKEDITAR and, for a reload that is not admitted, «No se recargó: el borrador se conserva tal como estaba.». The field is the REAL
    /// view-model over a double of the edit port that lives in this class: it records every call and answers what the recipe gives it. Nothing
    /// reads or writes the drawing, nothing opens a window or a dialog and nothing touches the profile. Every call goes through <c>Paso</c> (the
    /// «EXCEPCION» rule), so a behavior that is missing fails by ASSERTION on the lines.
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackEditAllReloadFieldTests
    {
        private static readonly SessionId Session = new SessionId(7);

        private const string Rack = "R-1";

        private const string Diagnostic = "No editable aquí: cambió el rack.";

        private const string Hint = "Usa RACKEDITAR.";

        // ---------------------------------------------------------------------------------------------- helpers

        /// <summary>The «EXCEPCION» rule: a step that throws becomes a line, and the expected lines never contain one.</summary>
        private static void Paso(List<string> log, Action step)
        {
            try
            {
                step();
            }
            catch (Exception error)
            {
                log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
            }
        }

        private static void Finish(List<string> log, IEnumerable<string> expected)
            => Assert.Equal(string.Join("\n", expected), string.Join("\n", log));

        /// <summary>The edit port of the recipes: it records every operation by name and answers the view, «Editar todo» and the reload of
        /// «Editar» that it is given.</summary>
        private sealed class PortDouble : IWorkspacePilotEditPort
        {
            public List<string> Calls { get; } = new List<string>();

            public PilotFieldView View { get; set; }

            public Func<EditAllOutcome> Answer { get; set; }

            public EditIntentOutcome Reload { get; set; }

            public EditAllOutcome TryEditAll(EditIntentRequest request)
            {
                Calls.Add("TryEditAll");
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
                return default(PilotFieldResult);
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
                return Reload;
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

        /// <summary>A clean draft that exists and the editor opens, with committed text 1:50, marked as possibly stale when asked.</summary>
        private static PilotFieldView CleanView(bool stale)
            => new PilotFieldView(true, PilotFieldState.Clean, stale, false, true, "1:50", null, null, null);

        /// <summary>The field over the double, on the surface of the rack of the session, with the calls of the first paint forgotten.</summary>
        private static (WorkspacePilotFieldViewModel Field, PortDouble Port) FieldOver(PilotFieldView view)
        {
            var port = new PortDouble { View = view };
            var field = new WorkspacePilotFieldViewModel(port, () => { });
            field.Show(Session, Rack, false);
            port.Calls.Clear();
            return (field, port);
        }

        /// <summary>What the field shows after the gesture: the reason, the hint, the notice, the state, the box and the calls of the gesture.</summary>
        private static string Shown(WorkspacePilotFieldViewModel field, PortDouble port)
            => "diagnostico=[" + field.NotEditableText + "] pista=[" + field.ClassicEditorHintText + "] aviso=[" + field.NoticeText + "] estado=["
               + field.StateText + "] editable=" + field.IsEditable + " llamadas=[" + string.Join(",", port.Calls) + "]";

        /// <summary>What the field shows, without the calls: the part that «Editar todo» and «Editar» must show alike.</summary>
        private static string Seen(WorkspacePilotFieldViewModel field)
            => field.NotEditableText + "|" + field.ClassicEditorHintText + "|" + field.NoticeText + "|" + field.StateText + "|" + field.IsEditable;

        /// <summary>«Editar todo» over a clean draft marked as possibly stale whose reload answered <paramref name="reload"/>, and «Editar» over
        /// the same draft with the same answer of its reload: the line of each, and whether the field shows the same.</summary>
        private static bool BothGestures(List<string> log, string label, EditIntentOutcome reload)
        {
            var (all, allPort) = FieldOver(CleanView(stale: true));
            allPort.Answer = () => EditAllOutcome.NotBegun(reload);
            all.EditAll();
            log.Add(label + " editar-todo " + Shown(all, allPort));

            var (one, onePort) = FieldOver(CleanView(stale: true));
            onePort.Reload = reload;
            one.BeginEdit();
            log.Add(label + " editar " + Shown(one, onePort));
            return Seen(all) == Seen(one);
        }

        // ------------------------------------------------------------------------------------------------- CT-03

        [Fact]
        public void CT_PBT4E_03_TheFieldShowsAReloadOfEditAllThatDidNotReloadAsEditarShowsItAndOnlyAsksThePortForEditAll()
        {
            var log = new List<string>();
            var same = new List<string>();

            Paso(log, () => same.Add("no-recargado=" + BothGestures(log, "no-recargado",
                EditIntentOutcome.NotAdmitted(Diagnostic, null, Hint, EditWitnessStatus.NotSupplied))));
            Paso(log, () => same.Add("ocupado=" + BothGestures(log, "ocupado", EditIntentOutcome.Waiting("Hay un comando en curso."))));
            Paso(log, () => same.Add("fallo=" + BothGestures(log, "fallo", EditIntentOutcome.Failed("boom"))));
            log.Add("igual-que-editar " + string.Join(" ", same));

            // The reload of the host reloaded the draft and the editor closed over it: the field asks the port for nothing else and the box is
            // editable again, without the mark.
            Paso(log, () =>
            {
                var (field, port) = FieldOver(CleanView(stale: true));
                port.Answer = () =>
                {
                    port.View = CleanView(stale: false);
                    return EditAllOutcome.Closed(0);
                };
                field.EditAll();
                log.Add("recargado-y-cerrado " + Shown(field, port));
            });

            // Without a draft, a begin that is not admitted is shown as today: the reason and the hint, without the notice of the reload.
            Paso(log, () =>
            {
                var (field, port) = FieldOver(default(PilotFieldView));
                port.Answer = () => EditAllOutcome.NotBegun(EditIntentOutcome.NotAdmitted(Diagnostic, null, Hint, EditWitnessStatus.NotSupplied));
                field.EditAll();
                log.Add("sin-borrador-no-admitido diagnostico=[" + field.NotEditableText + "] pista=[" + field.ClassicEditorHintText + "] aviso=["
                    + field.NoticeText + "]");
            });

            Finish(
                log,
                new[]
                {
                    "no-recargado editar-todo diagnostico=[No editable aquí: cambió el rack.] pista=[Usa RACKEDITAR.] aviso=[No se recargó: el borrador se conserva tal como estaba.] estado=[Limpio (posiblemente obsoleto)] editable=False llamadas=[GetField,TryEditAll,GetField]",
                    "no-recargado editar diagnostico=[No editable aquí: cambió el rack.] pista=[Usa RACKEDITAR.] aviso=[No se recargó: el borrador se conserva tal como estaba.] estado=[Limpio (posiblemente obsoleto)] editable=False llamadas=[GetField,TryReload,GetField]",
                    "ocupado editar-todo diagnostico=[] pista=[] aviso=[En espera: Hay un comando en curso.] estado=[Limpio (posiblemente obsoleto)] editable=False llamadas=[GetField,TryEditAll,GetField]",
                    "ocupado editar diagnostico=[] pista=[] aviso=[En espera: Hay un comando en curso.] estado=[Limpio (posiblemente obsoleto)] editable=False llamadas=[GetField,TryReload,GetField]",
                    "fallo editar-todo diagnostico=[] pista=[] aviso=[Fallo: boom] estado=[Limpio (posiblemente obsoleto)] editable=False llamadas=[GetField,TryEditAll,GetField]",
                    "fallo editar diagnostico=[] pista=[] aviso=[Fallo: boom] estado=[Limpio (posiblemente obsoleto)] editable=False llamadas=[GetField,TryReload,GetField]",
                    "igual-que-editar no-recargado=True ocupado=True fallo=True",
                    "recargado-y-cerrado diagnostico=[] pista=[] aviso=[Editor cerrado sin cambios en el borrador.] estado=[Limpio] editable=True llamadas=[GetField,TryEditAll,GetField]",
                    "sin-borrador-no-admitido diagnostico=[No editable aquí: cambió el rack.] pista=[Usa RACKEDITAR.] aviso=[]",
                });
        }
    }
}
