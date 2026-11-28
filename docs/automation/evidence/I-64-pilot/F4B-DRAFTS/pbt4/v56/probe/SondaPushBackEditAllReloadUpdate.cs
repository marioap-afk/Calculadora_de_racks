using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-64 F4B PB-T4e, la sonda temporal de la precondicion 6 (nunca entra en un commit): la receta de CT-PBT4E-04 con el detalle de
    /// «Actualizar» (el estado, la clasificacion, la causa, el motivo y las diferencias), para describir lo que pasa sin la recarga (en la base)
    /// y con ella. Acaba en Assert.Fail con sus lineas.
    /// </summary>
    public sealed class SondaPushBackEditAllReloadUpdateTests
    {
        private sealed class EditorDouble : IWorkspaceFullEditHost
        {
            private readonly WorkspaceHostCore _core;

            public EditorDouble(WorkspaceHostCore core) => _core = core;

            public Action<PushBackDraftEditorLaunch> Inside { get; set; }

            public void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class
            {
                _core.SetModalRackCadActive(true);
                try
                {
                    Inside?.Invoke(launch as PushBackDraftEditorLaunch);
                }
                finally
                {
                    _core.SetModalRackCadActive(false);
                }
            }
        }

        private static PushBackPilotIntentScan ThreeViews(string name)
            => IntentKit.Scan(
                FullPrepareKit.Rack,
                "A1",
                IntentKit.Member("A1", FullPrepareKit.Frontal(name: name)),
                IntentKit.Member("B2", FullPrepareKit.Lateral(3, name: name)),
                IntentKit.Member("C3", FullPrepareKit.Planta(name: name)));

        [Fact]
        public void TheUpdateAfterEditAllOverADraftThatTheDrawingChangedOutside()
        {
            var log = new List<string>();
            try
            {
                var before = ThreeViews(null);
                var after = ThreeViews("Otro nombre");
                var reader = new FullEditIntentFakeReader();
                var rig = new FullReadRig(reader);
                var editor = new EditorDouble(rig.Core);
                rig.Core.SetFullEditHost(editor);
                reader.Answer = rack => FullEditIntentKit.ReadOf(before);
                var draft = rig.Begin();
                reader.Answer = rack => FullEditIntentKit.ReadOf(after);
                rig.Host.Bridge.Core.OnSourceEvent(rig.Session, BridgeSource.ObjectModified);
                rig.Host.Idle();
                editor.Inside = launch =>
                {
                    var edited = PushBackEditableStateComparer.Copy(launch.State);
                    edited.Editor.SetSlotCount(edited.Editor.SlotCount - 1);
                    edited.AnnotationScale = 2.5;
                    launch.Port.ApplyToDraft(edited);
                };
                var all = ((IWorkspacePilotEditPort)rig.Core).TryEditAll(rig.Request());
                log.Add("editar-todo " + all.Status + " borrador=" + draft.Status + " obsoleto=" + draft.IsPossiblyStale);
                var port = new PilotEraseScopeDouble(FullWriteKit.ItemsOf(after));
                var update = rig.Core.TryUpdatePilot(rig.Request(), port);
                log.Add("actualizar estado=" + update.Status + " clasificacion=" + update.Classification + " causa=[" + update.Cause + "] motivo=["
                    + update.Reason + "] diferencias=[" + string.Join("; ", update.Differences) + "] borrador=" + draft.Status
                    + " obsoleto=" + draft.IsPossiblyStale + " llamadas=[" + string.Join(",", port.Calls) + "]");
            }
            catch (Exception error)
            {
                log.Add("EXCEPCION " + error.GetType().Name + ": " + error.Message);
            }

            Assert.Fail("SONDA\n" + string.Join("\n", log));
        }
    }
}
