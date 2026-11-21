#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// PB-T4c (paquete v5.0), precondicion 6, paso 7: la sonda de las lineas de CT-PBT4C-03, 04, 05, 10 (sin «Actualizar») y 12 sobre el GREEN
    /// literal, con los ayudantes de PB-T4b (FullReadRig, FullEditIntentFakeReader y FullPrepareKit) sin editarlos. Es una clase temporal: no
    /// entra en ningun commit y acaba en Assert.Fail con sus lineas.
    /// </summary>
    public sealed class TmpPbt4cEditAllProbe
    {
        private sealed class EditorDouble : IWorkspaceFullEditHost
        {
            private readonly WorkspaceHostCore _core;

            public EditorDouble(WorkspaceHostCore core)
            {
                _core = core;
            }

            public Action<PushBackDraftEditorLaunch>? Inside { get; set; }

            public int Shown { get; private set; }

            public PushBackDraftEditorLaunch? Last { get; private set; }

            public void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class
            {
                Shown++;
                Last = launch as PushBackDraftEditorLaunch;
                _core.SetModalRackCadActive(true);
                try
                {
                    Inside?.Invoke(Last!);
                }
                finally
                {
                    _core.SetModalRackCadActive(false);
                }
            }
        }

        private static string R(string? reason) => reason == null ? "null" : "[" + reason + "]";

        private static (FullReadRig Rig, FullEditIntentFakeReader Reader, EditorDouble Editor) Rig(bool withEditor = true)
        {
            var reader = new FullEditIntentFakeReader();
            var rig = new FullReadRig(reader);
            var editor = new EditorDouble(rig.Core);
            if (withEditor) rig.Core.SetFullEditHost(editor);
            return (rig, reader, editor);
        }

        private static string Line(string label, EditAllOutcome o, EditorDouble editor, FullEditIntentFakeReader reader) =>
            label + " estado=" + o.Status + " motivo=" + R(o.Reason) + " aplicadas=" + o.Applied + " abierto=" + (editor.Shown > 0) + " lecturas=" + reader.Calls.Count
            + (o.Status == EditAllStatus.NotBegun ? " inicio=" + o.Begin.Status + " diagnostico=" + R(o.Begin.Diagnostic) : string.Empty);

        [Fact]
        public void Probe()
        {
            var lines = new List<string>();

            // CT-03: sin anfitrion, sin borrador, Clean, Dirty, Applying, Unknown, texto pendiente y otra sesion.
            {
                var (rig, reader, editor) = Rig(withEditor: false);
                var o = ((IWorkspacePilotEditPort)rig.Core).TryEditAll(rig.Request());
                lines.Add(Line("sin-anfitrion", o, editor, reader) + " borrador=" + (rig.Draft() != null));
            }
            {
                var (rig, reader, editor) = Rig();
                var o = ((IWorkspacePilotEditPort)rig.Core).TryEditAll(rig.Request());
                lines.Add(Line("sin-borrador", o, editor, reader) + " borrador=" + (rig.Draft() != null) + " estado-del-borrador=" + rig.Draft()?.Status);
            }
            {
                var (rig, reader, editor) = Rig();
                var draft = rig.Begin()!;
                reader.Calls.Clear();
                var o = ((IWorkspacePilotEditPort)rig.Core).TryEditAll(rig.Request());
                lines.Add(Line("clean", o, editor, reader));
            }
            {
                var (rig, reader, editor) = Rig();
                var draft = rig.Begin()!;
                FullPrepareKit.SetScale(draft, 2.5);
                reader.Calls.Clear();
                var o = ((IWorkspacePilotEditPort)rig.Core).TryEditAll(rig.Request());
                lines.Add(Line("dirty", o, editor, reader) + " estado-del-borrador=" + draft.Status);
            }
            {
                var (rig, reader, editor) = Rig();
                var draft = rig.Begin()!;
                Assert.True(draft.TryBeginApply().IsStarted);
                reader.Calls.Clear();
                var o = ((IWorkspacePilotEditPort)rig.Core).TryEditAll(rig.Request());
                lines.Add(Line("applying", o, editor, reader));
            }
            {
                var (rig, reader, editor) = Rig();
                var draft = rig.Begin()!;
                Assert.True(draft.TryBeginApply().IsStarted);
                draft.CompleteApply(DraftApplyOutcome<PushBackFullDraftState>.CommitThrew("the commit threw"));
                reader.Calls.Clear();
                var o = ((IWorkspacePilotEditPort)rig.Core).TryEditAll(rig.Request());
                lines.Add(Line("unknown", o, editor, reader) + " estado-del-borrador=" + draft.Status);
            }
            {
                var (rig, reader, editor) = Rig();
                var draft = rig.Begin()!;
                draft.SetPendingText(PushBackFullFieldAdapter.AnnotationScaleKey, "abc");
                reader.Calls.Clear();
                var o = ((IWorkspacePilotEditPort)rig.Core).TryEditAll(rig.Request());
                lines.Add(Line("texto-pendiente", o, editor, reader));
            }
            {
                var (rig, reader, editor) = Rig();
                var o = ((IWorkspacePilotEditPort)rig.Core).TryEditAll(new EditIntentRequest(new SessionId(777), FullEditIntentKit.RackA, null, null));
                lines.Add(Line("otra-sesion", o, editor, reader));
            }

            // CT-04 y CT-10 (sin «Actualizar»): el editor aplica por el puerto del lanzamiento, dos veces; el origen se conserva.
            {
                var (rig, reader, editor) = Rig();
                var draft = rig.Begin()!;
                var origin = draft.CurrentDraftState.Origin;
                var results = new List<string>();
                editor.Inside = launch =>
                {
                    results.Add("lanzamiento estado-igual=" + PushBackEditableStateComparer.Instance.Equals(launch.State, draft.CurrentDraftState.Editable)
                                + " rack=" + (launch.RackId == FullEditIntentKit.RackA) + " proyecto=" + (launch.SourceProject != null)
                                + " proyecto-con-diseno=" + (launch.SourceProject?.PushBackDesign != null));
                    var edited = PushBackEditableStateComparer.Copy(launch.State);
                    edited.AnnotationScale = 2.5;
                    var first = launch.Port.ApplyToDraft(edited);
                    results.Add("aplicar-1 estado=" + first.Status + " isdirty=" + first.IsDirty + " origen-igual=" + (draft.CurrentDraftState.Origin == origin)
                                + " escala=" + draft.CurrentDraftState.Editable.AnnotationScale.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                    var again = PushBackEditableStateComparer.Copy(launch.State);
                    again.AnnotationScale = 3;
                    var second = launch.Port.ApplyToDraft(again);
                    results.Add("aplicar-2 estado=" + second.Status + " isdirty=" + second.IsDirty
                                + " escala=" + draft.CurrentDraftState.Editable.AnnotationScale.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                };
                var o = ((IWorkspacePilotEditPort)rig.Core).TryEditAll(rig.Request());
                lines.AddRange(results);
                lines.Add(Line("tras-aplicar", o, editor, reader) + " estado-del-borrador=" + draft.Status + " origen-igual=" + (draft.CurrentDraftState.Origin == origin));
            }
            {
                // El adaptador por si solo, sobre un borrador que no admite cambios (Applying): NotEditable y ninguna llamada de vuelta.
                var (rig, _, _) = Rig();
                var draft = rig.Begin()!;
                var calls = 0;
                var port = new PushBackFullDraftStatePort(draft, () => calls++);
                Assert.True(draft.TryBeginApply().IsStarted);
                var edited = PushBackEditableStateComparer.Copy(draft.CurrentDraftState.Editable);
                edited.AnnotationScale = 2.5;
                var r = port.ApplyToDraft(edited);
                lines.Add("adaptador-no-editable estado=" + r.Status + " llamadas=" + calls);
            }

            // CT-05: K = 1, cada operacion del puerto dentro de ShowModal; despues, la misma lectura de memoria.
            {
                var (rig, reader, editor) = Rig();
                var draft = rig.Begin()!;
                IWorkspacePilotEditPort port = rig.Core;
                var origin = new WorkspaceRequest(rig.Session);
                var rack = FullEditIntentKit.RackA;
                var request = rig.Request();
                var k1 = new List<string>();
                editor.Inside = _ =>
                {
                    var t = port.SetPendingText(origin, rack, "3"); k1.Add("k1 SetPendingText " + t.Status + " " + R(t.Reason));
                    var c = port.CommitField(origin, rack); k1.Add("k1 CommitField " + c.Status + " " + R(c.Reason));
                    var b = port.CrossBoundary(origin, rack, PilotBoundaryKind.TabChange); k1.Add("k1 CrossBoundary " + b.Decision + " " + R(b.Reason));
                    var d = port.DiscardRack(origin, rack); k1.Add("k1 DiscardRack " + d.Status + " " + R(d.Reason));
                    var dd = port.DiscardDocument(origin); k1.Add("k1 DiscardDocument " + dd.Status + " " + R(dd.Reason));
                    var b1 = port.TryBeginEdit(request); k1.Add("k1 TryBeginEdit " + b1.Status + " " + R(b1.Reason));
                    var b2 = port.TryCheckNow(request); k1.Add("k1 TryCheckNow " + b2.Status + " " + R(b2.Reason));
                    var b3 = port.TryReload(request); k1.Add("k1 TryReload " + b3.Status + " " + R(b3.Reason));
                    var b4 = port.TryConfirmBase(request); k1.Add("k1 TryConfirmBase " + b4.Status + " " + R(b4.Reason));
                    var b5 = port.TryKeepMine(request); k1.Add("k1 TryKeepMine " + b5.Status + " " + R(b5.Reason));
                    var b6 = port.TryDiscardAndReload(request); k1.Add("k1 TryDiscardAndReload " + b6.Status + " " + R(b6.Reason));
                    var b7 = port.TryEditAll(request); k1.Add("k1 TryEditAll " + b7.Status + " " + R(b7.Reason));
                    var u = port.RequestUpdate(request); k1.Add("k1 RequestUpdate " + u.Status + " " + R(u.Reason));
                    var g = port.GetField(origin, rack); k1.Add("k1 GetField existe=" + g.Exists + " estado=" + g.State);
                    k1.Add("k1 GetRacksAtRisk conocido=" + port.GetRacksAtRisk(origin).IsKnown);
                    k1.Add("k1 TakeLossNotices " + port.TakeLossNotices().Count + " TakeCloseNotices " + port.TakeCloseNotices().Count);
                    k1.Add("k1 TakeUpdateProgress en-vuelo=" + port.TakeUpdateProgress(origin, rack).InFlight);
                };
                var o = port.TryEditAll(request);
                lines.AddRange(k1);
                lines.Add(Line("k1-cerrado", o, editor, reader) + " lecturas-dentro=" + reader.Calls.Count);
                var after = port.SetPendingText(origin, rack, "3");
                lines.Add("despues SetPendingText " + after.Status + " TryBeginEdit " + port.TryBeginEdit(request).Status);
            }

            // CT-12: la compuerta reenvia TryEditAll; dentro de un aviso, Waiting.
            {
                var (rig, reader, editor) = Rig();
                var depth = new WorkspaceNoticeDepth();
                IWorkspacePilotEditPort gate = new WorkspacePilotEditGate(rig.Core, depth);
                depth.Enter();
                var inside = gate.TryEditAll(rig.Request());
                depth.Leave();
                lines.Add(Line("compuerta-dentro", inside, editor, reader));
                var outside = gate.TryEditAll(rig.Request());
                lines.Add(Line("compuerta-fuera", outside, editor, reader));
            }

            Assert.Fail(string.Join("\n", lines));
        }
    }
}
