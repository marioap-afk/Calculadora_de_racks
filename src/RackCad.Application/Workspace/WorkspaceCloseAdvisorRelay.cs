#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4-T3C2 (D-T3C2-04): the wrapper of the lifecycle advisor. This is the skeleton of the first step: it only delegates to the
    /// inner advisor and keeps no message.
    /// </summary>
    public sealed class WorkspaceCloseAdvisorRelay : IWorkspaceLifecycleAdvisor
    {
        private readonly IWorkspaceLifecycleAdvisor _inner;

        public WorkspaceCloseAdvisorRelay(
            IWorkspaceLifecycleAdvisor inner,
            IWorkspacePilotEditPort risks,
            Func<SessionId, string?> documentNameOf,
            Func<SessionId, bool> isActive,
            Action<string> writeNow,
            Action<string, Exception>? report)
        {
            _inner = inner;
        }

        public CloseDecision AdviseClose(SessionId session) => _inner.AdviseClose(session);

        public void SessionDestroyed(SessionId session, bool closeSequenceIntact) => _inner.SessionDestroyed(session, closeSequenceIntact);

        public IReadOnlyList<string> TakeMessages(SessionId? activeSession) => Array.Empty<string>();
    }
}
