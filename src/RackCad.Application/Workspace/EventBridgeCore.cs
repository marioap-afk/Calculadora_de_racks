#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Pure policy of the central event bridge (I-64 D-04): which event sources are attached, how notifications
    /// become hints on their own session, and when a drain may run. This is the compilable skeleton: the full
    /// signatures exist, the behavior is not implemented yet.
    /// </summary>
    public sealed class EventBridgeCore
    {
        public EventBridgeCore(IHostEventSource source, Action<string, Exception>? logFault = null)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
        }

        public bool IsStarted => false;

        public bool IsIdleAttached => false;

        public int LiveSessionCount => 0;

        public SessionId? ActiveSession => null;

        public IReadOnlyCollection<SourceAttachment> AttachedSources => Array.Empty<SourceAttachment>();

        public bool TryGetSession(SessionId id, out WorkspaceSession? session)
        {
            session = null;
            return false;
        }

        public bool TryGetSession(long nativeInstanceId, out WorkspaceSession? session)
        {
            session = null;
            return false;
        }

        public bool IsDegraded(SessionId id) => false;

        public void SetPanelVisible(bool visible)
        {
        }

        public void SetModalRackCadActive(bool active)
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void OnDocumentCreated(long nativeInstanceId)
        {
        }

        public void OnDocumentActivated(long nativeInstanceId, bool isQuiescent)
        {
        }

        public void OnDocumentToBeDestroyed(long nativeInstanceId)
        {
        }

        public void OnSourceEvent(SessionId session, BridgeSource source)
        {
        }

        public CloseDecision OnBeginDocumentClose(SessionId session) => CloseDecision.Allow;

        public IReadOnlyList<DrainOutcome> OnIdle() => Array.Empty<DrainOutcome>();

        public void ReportFault(SessionId session, string context, Exception exception)
        {
        }
    }
}
