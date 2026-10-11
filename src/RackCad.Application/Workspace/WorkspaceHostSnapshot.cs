#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>Overall state of the panel host.</summary>
    public enum WorkspaceHostStatus
    {
        /// <summary>The panel was never opened: nothing is subscribed and nothing runs.</summary>
        NotStarted,

        /// <summary>No document is open: empty state, actions disabled.</summary>
        NoDocument,

        /// <summary>A document is active and its session is synchronized.</summary>
        Ready,

        /// <summary>The active session had a fault: it is not read any more.</summary>
        Degraded,
    }

    /// <summary>Why the actions that read or write the drawing are disabled, if they are.</summary>
    public enum WorkspaceActionBlock
    {
        None,
        NotStarted,
        NoDocument,
        Degraded,
        ModalActive,
        CommandActive,
    }

    /// <summary>Whether the panel actions that read or write the drawing are available right now (D-02).</summary>
    public readonly record struct WorkspaceActionAvailability(bool CanReadDrawing, bool CanWriteDrawing, WorkspaceActionBlock Block);

    /// <summary>A plain-value picture of the panel state for the UI. It holds no host object.</summary>
    public sealed record WorkspaceHostSnapshot(
        WorkspaceHostStatus Status,
        SessionId? ActiveSession,
        bool PanelVisible,
        WorkspaceActionAvailability Actions,
        SelectionContext Selection,
        bool PickFirstDisabled,
        bool InstrumentationEnabled);

    /// <summary>Answer to a selection-context request: the verdict, and the context only when accepted.</summary>
    public sealed record WorkspaceSelectionResponse(RequestDecision Decision, SelectionContext? Context);
}
