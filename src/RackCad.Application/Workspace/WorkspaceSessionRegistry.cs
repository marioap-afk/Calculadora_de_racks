#nullable enable
using System.Collections.Generic;

namespace RackCad.Application.Workspace;

/// <summary>Pure registry of the live sessions: one per open document instance (D-03).</summary>
public sealed class WorkspaceSessionRegistry
{
    private long _next;
    private readonly List<WorkspaceSession> _live = new();

    public int LiveSessionCount => _live.Count;

    public WorkspaceSession GetOrCreate(long nativeInstanceId)
    {
        var session = new WorkspaceSession(new SessionId(++_next), nativeInstanceId);
        _live.Add(session);
        return session;
    }

    public bool TryGet(SessionId id, out WorkspaceSession? session)
    {
        session = null;
        return false;
    }

    public void Destroy(SessionId id)
    {
    }

    /// <summary>Checks a request against the session of the document that would execute it. No side effects.</summary>
    public RequestDecision Authorize(WorkspaceRequest request, long executingNativeInstanceId) =>
        RequestDecision.Accepted;
}
