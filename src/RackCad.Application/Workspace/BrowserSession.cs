#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    public enum BrowserTabStatus
    {
        NotLoaded,
        Ready,
        Unavailable,
        Error,
    }

    public enum BrowserChange
    {
        None,
        Activated,
        Created,
        Replaced,
        Pinned,
        Closed,
        SummaryApplied,
    }

    public sealed record BrowserTabRecord(string RackId, string? Name, string? Kind, bool IsPreview, BrowserTabStatus Status)
    {
        public string RackId { get; init; } = !string.IsNullOrWhiteSpace(RackId)
            ? RackId
            : throw new ArgumentException("A rack id must not be empty.", nameof(RackId));
    }

    /// <summary>RED skeleton: every operation does nothing.</summary>
    public sealed class BrowserSession
    {
        public const int MaxLiveSurfaces = 1;

        internal BrowserSession(SessionId id)
        {
            Id = id;
        }

        public SessionId Id { get; }

        public bool IsDestroyed => false;

        public IReadOnlyList<BrowserTabRecord> Tabs => Array.Empty<BrowserTabRecord>();

        public string? ActiveRackId => null;

        public string? LiveSurfaceRackId => null;

        public BrowserChange ApplySelectionContext(SelectionContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            return BrowserChange.None;
        }

        public BrowserChange Activate(string? rackId) => BrowserChange.None;

        public BrowserChange Pin(string? rackId) => BrowserChange.None;

        public BrowserChange Close(string? rackId) => BrowserChange.None;

        public BrowserChange ApplySummary(string? rackId, BrowserTabStatus status, string? name, string? kind) => BrowserChange.None;

        internal void Retire()
        {
        }
    }
}
