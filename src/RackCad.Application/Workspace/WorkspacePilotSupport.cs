#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4-T3A (D-T3-03): the text that shows the committed value of the field. In this first step it is inert: it builds nothing.
    /// </summary>
    public static class WorkspacePilotValueText
    {
        public static string? Show(double value, out string? reason)
        {
            reason = null;
            return string.Empty;
        }
    }

    /// <summary>
    /// I-64 F4-T3A (D-T3-05): the scope of a modal window of RackCad opened by the panel. In this first step it is inert: it calls nothing.
    /// </summary>
    public sealed class WorkspacePanelModalScope
    {
        public WorkspacePanelModalScope(Action<bool> setActive)
        {
        }

        public int Depth => 0;

        public IDisposable Enter() => new WorkspacePanelModalHold();
    }

    /// <summary>The handle of one open <see cref="WorkspacePanelModalScope"/>; inert.</summary>
    internal sealed class WorkspacePanelModalHold : IDisposable
    {
        public void Dispose()
        {
        }
    }

    /// <summary>
    /// I-64 F4-T3A (D-T3-06): how deep inside a notice of the panel the process is. In this first step it is inert: it counts nothing.
    /// </summary>
    public sealed class WorkspaceNoticeDepth
    {
        public int Value => 0;

        public bool IsInside => false;

        public void Enter()
        {
        }

        public void Leave()
        {
        }
    }

    /// <summary>
    /// I-64 F4-T3A (D-T3-06): the decorator of the port that the panel receives. In this first step it only delegates.
    /// </summary>
    public sealed class WorkspacePilotEditGate : IWorkspacePilotEditPort
    {
        public const string InsideNoticeReason = "A notice of the panel is being delivered.";

        private readonly IWorkspacePilotEditPort _inner;

        public WorkspacePilotEditGate(IWorkspacePilotEditPort inner, WorkspaceNoticeDepth depth)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            if (depth == null) throw new ArgumentNullException(nameof(depth));
        }

        public EditIntentOutcome TryBeginEdit(EditIntentRequest request) => _inner.TryBeginEdit(request);

        public EditCheckOutcome TryCheckNow(EditIntentRequest request) => _inner.TryCheckNow(request);

        public EditIntentOutcome TryReload(EditIntentRequest request) => _inner.TryReload(request);

        public PilotFieldView GetField(WorkspaceRequest origin, string rackId) => _inner.GetField(origin, rackId);

        public PilotFieldResult SetPendingText(WorkspaceRequest origin, string rackId, string text) =>
            _inner.SetPendingText(origin, rackId, text);

        public PilotFieldResult CommitField(WorkspaceRequest origin, string rackId) => _inner.CommitField(origin, rackId);

        public PilotBoundaryResult CrossBoundary(WorkspaceRequest origin, string rackId, PilotBoundaryKind kind) =>
            _inner.CrossBoundary(origin, rackId, kind);

        public EditDiscardOutcome DiscardRack(WorkspaceRequest origin, string rackId) => _inner.DiscardRack(origin, rackId);

        public EditDiscardOutcome DiscardDocument(WorkspaceRequest origin) => _inner.DiscardDocument(origin);

        public PilotRiskView GetRacksAtRisk(WorkspaceRequest origin) => _inner.GetRacksAtRisk(origin);

        public IReadOnlyList<PilotLossView> TakeLossNotices() => _inner.TakeLossNotices();

        public IReadOnlyList<PilotCloseView> TakeCloseNotices() => _inner.TakeCloseNotices();
    }
}
