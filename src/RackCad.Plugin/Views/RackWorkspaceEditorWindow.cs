#nullable enable
using RackCad.Application.Systems.PushBack;
using RackCad.UI.Systems.PushBack;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Views
{
    /// <summary>
    /// I-64 F4B PB-T4c (D-PBT4-15) — the opener of the full Push Back editor for "Editar todo". It builds the window over the launch and shows
    /// it modally, without an owner, like the classic opener; the modal of the panel is its caller's. It reads and writes nothing of the
    /// drawing and does nothing after the window closes: the window only hands its edits to the port of the launch.
    /// </summary>
    internal static class RackWorkspaceEditorWindow
    {
        internal static void Show(object launch)
        {
            var window = RackPushBackSystemWindow.ForDraft((PushBackDraftEditorLaunch)launch);
            AcApplication.ShowModalWindow(window);
        }
    }
}
