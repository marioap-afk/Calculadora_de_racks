using System;
using RackCad.Application.Workspace;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// The Plugin implementation of the host port (I-64 D-02, INV-14, INV-21, INV-22). It only delegates to the pure host
    /// core: keys and values in, keys and values out, no host type, and no operation that writes anything.
    /// </summary>
    public sealed class WorkspaceHostPort : IWorkspaceHostPort
    {
        private readonly WorkspaceHostCore _core;

        public WorkspaceHostPort(WorkspaceHostCore core)
        {
            _core = core ?? throw new ArgumentNullException(nameof(core));
        }

        public WorkspaceHostSnapshot GetSnapshot() => _core.GetSnapshot();

        public RequestDecision Authorize(WorkspaceRequest request) => _core.Authorize(request);

        public WorkspaceSelectionResponse GetSelectionContext(WorkspaceRequest request) => _core.GetSelectionContext(request);

        public WorkspaceMeasurementReport GetMeasurementReport() => _core.GetMeasurementReport();
    }
}
