#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// What the host core needs from the one event bridge (I-64 D-02, D-04). The Plugin implements it over the single
    /// AutoCadEventBridge facades; tests implement it over the pure bridge core and a simulated event source.
    /// </summary>
    public interface IWorkspaceBridgeLink
    {
        /// <summary>First panel open: hooks the document collection and the active document. Called at most once by the host.</summary>
        void Start();

        void SetPanelVisible(bool visible);

        void SetModalRackCadActive(bool active);

        SessionId? ActiveSession { get; }

        bool TryGetSession(SessionId id, out WorkspaceSession? session);

        RequestDecision Authorize(WorkspaceRequest request, long executingNativeInstanceId);

        /// <summary>Queues the reconciliation hint of the active session. It drains later, at rest, never now.</summary>
        void RequestReconciliation();

        /// <summary>
        /// Opens the scope of the own mutation of the update of the session (I-64 F4-T4C1-HOST, D-T4C-09, IC-09): from now on the hints of the
        /// session that the update causes are kept apart and only invalidate. Memory only.
        /// </summary>
        void OpenOwnMutation(SessionId session);

        /// <summary>Closes the scope of the own mutation of the session. Idempotent. Memory only.</summary>
        void CloseOwnMutation(SessionId session);

        /// <summary>
        /// How many commands that are not the update itself started in the session, as the bridge learned it in memory. Null for a session
        /// that is degraded or unknown: its events no longer arrive.
        /// </summary>
        int? ForeignCommandStartsOf(SessionId session);
    }

    /// <summary>In-memory facts about the host that need no read of the drawing.</summary>
    public interface IWorkspaceHostProbe
    {
        /// <summary>Opaque native identity of the active document instance, or none when no document is open.</summary>
        long? ActiveDocumentNativeId { get; }

        /// <summary>False while a command is running in the active document.</summary>
        bool IsActiveDocumentQuiescent { get; }
    }
}
