#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Identity of one workspace session (I-64 D-03). Issued monotonically by the registry and never
    /// reused, so a destroyed session can never be paired with a later document.
    /// </summary>
    public readonly record struct SessionId(long Value);

    /// <summary>Key of a rack inside the workspace: always (session, RackId), never a bare RackId (D-03, INV-07).</summary>
    public readonly record struct RackKey(SessionId Session, string RackId);

    /// <summary>Verdict of the pure session check applied to a panel request (D-03, INV-22).</summary>
    public enum RequestDecision
    {
        Accepted,
        RejectedSessionMismatch,
        RejectedSessionDestroyed,
    }

    /// <summary>A panel request carries the identity of the session that originated it (D-03).</summary>
    public readonly record struct WorkspaceRequest(SessionId Origin);
}
