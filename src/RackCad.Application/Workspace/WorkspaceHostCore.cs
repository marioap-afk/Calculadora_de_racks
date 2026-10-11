#nullable enable
using System;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// The pure host core of the panel (I-64 D-02, D-04, D-06, D-18): the panel lifecycle policy, the active-session
    /// binding, the authorization of every request, the drain-time selection read and the context path. It touches the
    /// host only through <see cref="IWorkspaceBridgeLink"/>, <see cref="IWorkspaceHostProbe"/> and
    /// <see cref="IWorkspaceSelectionReader"/>.
    /// </summary>
    public sealed class WorkspaceHostCore
    {
        public WorkspaceHostCore(
            IWorkspaceBridgeLink bridge,
            IWorkspaceHostProbe probe,
            IWorkspaceSelectionReader reader,
            WorkspaceInstrumentation instrumentation,
            Action<string, Exception>? logFault = null)
        {
        }

        public bool IsStarted => false;

        public bool PanelVisible => false;

        public bool InstrumentationEnabled => false;

        public void SetInstrumentationEnabled(bool enabled)
        {
        }

        public void OpenPanel()
        {
        }

        public void ShowPanel()
        {
        }

        public void HidePanel()
        {
        }

        public void RefreshPanel()
        {
        }

        public void SetModalRackCadActive(bool active)
        {
        }

        public void OnDrained(DrainOutcome outcome)
        {
        }

        public WorkspaceHostSnapshot GetSnapshot() => new(
            WorkspaceHostStatus.NotStarted,
            null,
            false,
            default,
            SelectionContext.None,
            false,
            false);

        public RequestDecision Authorize(WorkspaceRequest request) => RequestDecision.Accepted;

        public WorkspaceSelectionResponse GetSelectionContext(WorkspaceRequest request) =>
            new(RequestDecision.Accepted, SelectionContext.None);

        public WorkspaceMeasurementReport GetMeasurementReport() => new(false, Array.Empty<WorkspaceMeasurement>(), 0);
    }
}
