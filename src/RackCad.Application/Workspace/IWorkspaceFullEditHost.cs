#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4B PB-T4c (D-PBT4-15) — the host of the full editor of "Editar todo": it shows the editor modally over the launch that the host
    /// core hands it, inside the modal of the panel, and returns when the editor closes. It is generic so that the workspace names no type of
    /// the systems; it keeps nothing, and the editor only hands its edits to the port of the launch. It is a declared exception to R-PB-05
    /// and to ADR-0029 D12: it lives with the workspace and not with the system.
    /// </summary>
    public interface IWorkspaceFullEditHost
    {
        void ShowModal<TLaunch>(TLaunch launch) where TLaunch : class;
    }
}
