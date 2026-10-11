using RackCad.Application.Workspace;

namespace RackCad.Plugin.Workspace
{
    /// <summary>The Plugin implementation of the host port (I-64 F1-T3A): skeleton.</summary>
    public sealed class WorkspaceHostPort : IWorkspaceHostPort
    {
        public WorkspaceHostPort(WorkspaceHostCore core)
        {
        }

        public WorkspaceHostSnapshot GetSnapshot() => null;

        public RequestDecision Authorize(WorkspaceRequest request) => RequestDecision.Accepted;

        public WorkspaceSelectionResponse GetSelectionContext(WorkspaceRequest request) => null;

        public WorkspaceMeasurementReport GetMeasurementReport() => null;
    }
}
