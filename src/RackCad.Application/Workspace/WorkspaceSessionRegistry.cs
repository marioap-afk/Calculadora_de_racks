#nullable enable
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>Pure registry of the live sessions: one per open document instance (D-03).</summary>
    public sealed class WorkspaceSessionRegistry
    {
        private long _next;
        private readonly List<WorkspaceSession> _live = new();

        public int LiveSessionCount => _live.Count;

        public WorkspaceSession GetOrCreate(long nativeInstanceId)
        {
            var existing = _live.Find(x => x.NativeInstanceId == nativeInstanceId);
            if (existing != null) return existing;
            var session = new WorkspaceSession(new SessionId(++_next), nativeInstanceId);
            _live.Add(session);
            return session;
        }

        public bool TryGet(SessionId id, out WorkspaceSession? session)
        {
            session = _live.Find(x => x.Id == id);
            return session != null;
        }

        /// <summary>Retires the session. Its id is never reused, so it cannot be paired with a later document.</summary>
        public void Destroy(SessionId id)
        {
            var session = _live.Find(x => x.Id == id);
            if (session == null) return;
            session.IsDestroyed = true;
            _live.Remove(session);
        }

        /// <summary>Checks a request against the session of the document that would execute it. No side effects.</summary>
        public RequestDecision Authorize(WorkspaceRequest request, long executingNativeInstanceId)
        {
            if (!TryGet(request.Origin, out _)) return RequestDecision.RejectedSessionDestroyed;
            var executing = _live.Find(x => x.NativeInstanceId == executingNativeInstanceId);
            return executing != null && executing.Id == request.Origin
                ? RequestDecision.Accepted
                : RequestDecision.RejectedSessionMismatch;
        }
    }
}
