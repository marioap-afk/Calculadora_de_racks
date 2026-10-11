#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// The port the panel talks to (I-64 D-02, INV-14, INV-21, INV-22). Keys and values only: no host type crosses it.
    /// Every request carries the identity of the session that originated it and is authorized against the document that
    /// would execute it; a rejected request has no effect. In F1 the port is read-only: it exposes no operation that
    /// writes the drawing, the selection, the view or any stored data.
    /// </summary>
    public interface IWorkspaceHostPort
    {
        /// <summary>The state of the panel: status, active session, action availability and the selection context.</summary>
        WorkspaceHostSnapshot GetSnapshot();

        /// <summary>The verdict of the session check for a request. No side effects.</summary>
        RequestDecision Authorize(WorkspaceRequest request);

        /// <summary>The selection context of the request's session, only when the request is authorized.</summary>
        WorkspaceSelectionResponse GetSelectionContext(WorkspaceRequest request);

        /// <summary>The in-memory measurements collected so far (empty while the instrumentation is off).</summary>
        WorkspaceMeasurementReport GetMeasurementReport();
    }
}
