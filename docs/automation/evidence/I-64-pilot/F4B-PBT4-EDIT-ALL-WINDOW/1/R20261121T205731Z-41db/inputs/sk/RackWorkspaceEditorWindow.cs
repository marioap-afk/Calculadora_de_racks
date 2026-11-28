#nullable enable
using RackCad.Application.Systems.PushBack;

namespace RackCad.Plugin.Views
{
    /// <summary>I-64 F4B PB-T4c (RED): the opener of the full Push Back editor for "Editar todo", still without effect.</summary>
    internal static class RackWorkspaceEditorWindow
    {
        internal static void Show(object launch)
        {
            _ = launch as PushBackDraftEditorLaunch;
        }
    }
}
