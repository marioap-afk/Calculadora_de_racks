namespace RackCad.UI.Workspace
{
    /// <summary>
    /// What the workspace panel shows as a whole (I-64 D-02, D-03, D-17). The first four mirror the host status of the
    /// snapshot; <see cref="Error"/> is a presentation state of the panel itself: a contained failure, with a message
    /// and a retry. It never writes anything and never changes a session.
    /// </summary>
    public enum WorkspacePanelState
    {
        /// <summary>The panel was never opened by the host: nothing runs.</summary>
        NotStarted,

        /// <summary>No document is open: empty state, actions disabled.</summary>
        NoDocument,

        /// <summary>A document is active and its session is synchronized.</summary>
        Ready,

        /// <summary>The active session had a fault and is not read any more.</summary>
        Degraded,

        /// <summary>A panel-originated failure was contained; the classic commands keep working.</summary>
        Error,
    }
}
