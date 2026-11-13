#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RackCad.Application.Workspace
{
    /// <summary>State of the lazy summary of one rack tab. The summary is received from outside; this core never asks for it.</summary>
    public enum BrowserTabStatus
    {
        NotLoaded,
        Ready,
        Unavailable,
        Error,
    }

    /// <summary>What an operation of the browser session changed. <see cref="None"/> means exactly that nothing changed.</summary>
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

    /// <summary>
    /// Light record of one rack tab (I-64 D-08, INV-02). A value: it carries identity and the lazy summary
    /// received from outside, never a view, a resolved system, a host object, a draft or a subscription.
    /// </summary>
    public sealed record BrowserTabRecord(string RackId, string? Name, string? Kind, bool IsPreview, BrowserTabStatus Status)
    {
        public string RackId { get; init; } = !string.IsNullOrWhiteSpace(RackId)
            ? RackId
            : throw new ArgumentException("A rack id must not be empty.", nameof(RackId));
    }

    /// <summary>
    /// Pure, in-memory browser state of one workspace session (I-64 D-08): light tabs per rack, at most one preview,
    /// pinned tabs, a deterministic active tab and K = 1 live surface. The only automatic synchronization is
    /// <see cref="ApplySelectionContext"/> with a single-rack context. It has no host, no port and no write authority:
    /// closing a tab only changes memory.
    /// </summary>
    public sealed class BrowserSession
    {
        /// <summary>At most this many surfaces may be alive at once (D-08, INV-15). The policy materializes nothing.</summary>
        public const int MaxLiveSurfaces = 1;

        private readonly List<BrowserTabRecord> _tabs = new();
        private string? _activeRackId;
        private SelectionContext? _lastContext;

        internal BrowserSession(SessionId id)
        {
            Id = id;
        }

        public SessionId Id { get; }

        /// <summary>True once the registry retired the session; every later operation is a no-op returning <see cref="BrowserChange.None"/>.</summary>
        public bool IsDestroyed { get; private set; }

        /// <summary>A stable, read-only snapshot in presentation order: later operations never alter a collection already obtained.</summary>
        public IReadOnlyList<BrowserTabRecord> Tabs => new ReadOnlyCollection<BrowserTabRecord>(_tabs.ToArray());

        /// <summary>Null if and only if there are no tabs; otherwise the rack id of exactly one existing tab.</summary>
        public string? ActiveRackId => _activeRackId;

        /// <summary>The only surface that may be alive: exactly the active tab (null without tabs).</summary>
        public string? LiveSurfaceRackId => _activeRackId;

        /// <summary>
        /// The only automatic synchronization (D-06). Only a single-rack context syncs; an equal context to the last
        /// applied one (of any kind) changes nothing, so only a real transition applies again.
        /// </summary>
        public BrowserChange ApplySelectionContext(SelectionContext context, bool keepActive = false)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));
            if (IsDestroyed) return BrowserChange.None;

            if (_lastContext is not null && _lastContext == context) return BrowserChange.None;
            _lastContext = context;

            // RackId is set only for a single-rack context; None, Selection, NoIdentity and Diagnostic carry none.
            var rackId = context.RackId;
            if (string.IsNullOrWhiteSpace(rackId)) return BrowserChange.None;

            if (IndexOf(rackId) >= 0)
            {
                if (string.Equals(_activeRackId, rackId, StringComparison.Ordinal)) return BrowserChange.None;
                _activeRackId = rackId;
                return BrowserChange.Activated;
            }

            var fresh = new BrowserTabRecord(rackId, null, null, true, BrowserTabStatus.NotLoaded);
            var preview = PreviewIndex();
            BrowserChange change;
            if (preview >= 0)
            {
                _tabs[preview] = fresh;
                change = BrowserChange.Replaced;
            }
            else
            {
                _tabs.Add(fresh);
                change = BrowserChange.Created;
            }

            _activeRackId = rackId;
            return change;
        }

        /// <summary>Activates an existing tab. Modes, order and the remembered context do not change. Unknown, null or blank: nothing.</summary>
        public BrowserChange Activate(string? rackId)
        {
            if (IsDestroyed) return BrowserChange.None;
            var index = IndexOf(rackId);
            if (index < 0) return BrowserChange.None;
            if (string.Equals(_activeRackId, _tabs[index].RackId, StringComparison.Ordinal)) return BrowserChange.None;
            _activeRackId = _tabs[index].RackId;
            return BrowserChange.Activated;
        }

        /// <summary>Turns the preview into a pinned tab. Idempotent; a pinned tab is never replaced by the automatic synchronization.</summary>
        public BrowserChange Pin(string? rackId)
        {
            if (IsDestroyed) return BrowserChange.None;
            var index = IndexOf(rackId);
            if (index < 0 || !_tabs[index].IsPreview) return BrowserChange.None;
            _tabs[index] = _tabs[index] with { IsPreview = false };
            return BrowserChange.Pinned;
        }

        /// <summary>
        /// Removes a preview or pinned tab; memory only, nothing is written and nothing is asked. If it was the active one, the
        /// next position becomes active, else the previous one, else there is none.
        /// </summary>
        public BrowserChange Close(string? rackId)
        {
            if (IsDestroyed) return BrowserChange.None;
            var index = IndexOf(rackId);
            if (index < 0) return BrowserChange.None;

            var wasActive = string.Equals(_activeRackId, _tabs[index].RackId, StringComparison.Ordinal);
            _tabs.RemoveAt(index);
            if (wasActive)
            {
                if (index < _tabs.Count) _activeRackId = _tabs[index].RackId;
                else if (index > 0) _activeRackId = _tabs[index - 1].RackId;
                else _activeRackId = null;
            }

            return BrowserChange.Closed;
        }

        /// <summary>
        /// Receives the lazy summary of a tab from outside. Only Status, Name and Kind of that record change; a rack with no tab
        /// (for example a replaced preview) is ignored.
        /// </summary>
        public BrowserChange ApplySummary(string? rackId, BrowserTabStatus status, string? name, string? kind)
        {
            if (IsDestroyed) return BrowserChange.None;
            var index = IndexOf(rackId);
            if (index < 0) return BrowserChange.None;

            var current = _tabs[index];
            var next = current with { Status = status, Name = name, Kind = kind };
            if (next == current) return BrowserChange.None;
            _tabs[index] = next;
            return BrowserChange.SummaryApplied;
        }

        /// <summary>Retires the session: no tab, no active tab, no remembered context, and inert from now on.</summary>
        internal void Retire()
        {
            _tabs.Clear();
            _activeRackId = null;
            _lastContext = null;
            IsDestroyed = true;
        }

        private int IndexOf(string? rackId)
        {
            if (string.IsNullOrWhiteSpace(rackId)) return -1;
            for (var i = 0; i < _tabs.Count; i++)
                if (string.Equals(_tabs[i].RackId, rackId, StringComparison.Ordinal)) return i;
            return -1;
        }

        private int PreviewIndex()
        {
            for (var i = 0; i < _tabs.Count; i++)
                if (_tabs[i].IsPreview) return i;
            return -1;
        }
    }
}
