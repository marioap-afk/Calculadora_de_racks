#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Workspace.Drafts
{
    public enum DraftBeginStatus
    {
        Created,

        /// <summary>The (session, rack) pair already has a draft: it is returned untouched and nothing is replaced.</summary>
        AlreadyExists,

        /// <summary>The read was not admitted: no draft was created and the admission carries the diagnostic.</summary>
        NotAdmitted,

        /// <summary>The session was retired: a retired session is never resurrected.</summary>
        SessionRetired,
    }

    /// <summary>What Begin did. <see cref="Draft"/> is set for Created, and for AlreadyExists when the existing draft has the same state type.</summary>
    public sealed class DraftBeginResult<TState>
    {
        internal DraftBeginResult(DraftBeginStatus status, DraftSession<TState>? draft, DraftAdmission admission)
        {
            Status = status;
            Draft = draft;
            Admission = admission;
        }

        public DraftBeginStatus Status { get; }

        public DraftSession<TState>? Draft { get; }

        public DraftAdmission Admission { get; }
    }

    /// <summary>What Discard did: whether a draft was removed and whether it was Dirty or had pending text.</summary>
    public readonly record struct DraftDiscardResult(bool Removed, bool WasDirty);

    /// <summary>What DiscardAll did: the racks whose drafts were removed, ordered by rack id (ordinal).</summary>
    public sealed class DraftDiscardAllResult
    {
        internal DraftDiscardAllResult(IReadOnlyList<string> removedRackIds, IReadOnlyList<string> dirtyRackIds)
        {
            RemovedRackIds = removedRackIds;
            DirtyRackIds = dirtyRackIds;
        }

        public IReadOnlyList<string> RemovedRackIds { get; }

        /// <summary>The removed drafts that were Dirty or had pending text.</summary>
        public IReadOnlyList<string> DirtyRackIds { get; }
    }

    /// <summary>The visible notice of loss (D-17, INV-20): the racks whose Dirty or pending drafts disappeared with the session.</summary>
    public sealed class DraftLossNotice
    {
        internal DraftLossNotice(SessionId session, IReadOnlyList<string> lostRackIds)
        {
            Session = session;
            LostRackIds = lostRackIds;
        }

        public SessionId Session { get; }

        public IReadOnlyList<string> LostRackIds { get; }

        public bool HasLoss => LostRackIds.Count > 0;
    }

    /// <summary>
    /// The drafts of the workspace by (SessionId, RackId), in memory only (D-10, D-17). The rack id compares ordinally.
    /// The same rack id in two sessions gives two independent drafts. Nothing here depends on any surface: a draft outlives
    /// its dematerialization. Discarding or retiring never writes anything.
    /// </summary>
    public sealed class DraftRegistry
    {
        private readonly Dictionary<SessionId, Dictionary<string, IDraftEntry>> _bySession = new();
        private readonly HashSet<SessionId> _retired = new();
        private IDraftEntry? _applying;

        /// <summary>
        /// The only way a draft is born (INV-F4T1-03): from the result of ONE read of the edit intention that brings the
        /// stale base, the admission and the initial state together. A read that is not admitted creates nothing.
        /// </summary>
        /// <remarks>
        /// <paramref name="copy"/> is injected next to the comparer. The draft copies every state that enters it, the initial
        /// state of this read included, so the object the read carries is never shared with the draft: mutating it after
        /// <c>Begin</c> changes neither the initial state nor dirty, and two drafts born from the same object are
        /// independent (N-2). The current state is a copy of the initial one, so an in-place edit of the current state can
        /// never change the initial state (INV-F4T1-19).
        /// </remarks>
        public DraftBeginResult<TState> Begin<TState>(SessionId session, string rackId, DraftIntentRead<TState> read, IEqualityComparer<TState> comparer, Func<TState, TState> copy)
        {
            if (string.IsNullOrEmpty(rackId)) throw new ArgumentException("A rack id is required.", nameof(rackId));
            if (read == null) throw new ArgumentNullException(nameof(read));
            if (comparer == null) throw new ArgumentNullException(nameof(comparer));
            if (copy == null) throw new ArgumentNullException(nameof(copy));

            if (_retired.Contains(session))
                return new DraftBeginResult<TState>(DraftBeginStatus.SessionRetired, null, read.Admission);
            if (!read.IsAdmitted)
                return new DraftBeginResult<TState>(DraftBeginStatus.NotAdmitted, null, read.Admission);

            if (_bySession.TryGetValue(session, out var racks) && racks.TryGetValue(rackId, out var existing))
                return new DraftBeginResult<TState>(DraftBeginStatus.AlreadyExists, existing as DraftSession<TState>, read.Admission);

            if (racks == null)
            {
                racks = new Dictionary<string, IDraftEntry>(StringComparer.Ordinal);
                _bySession[session] = racks;
            }

            var draft = new DraftSession<TState>(this, session, rackId, read.StaleBase!, read.InitialState!, comparer, copy);
            racks[rackId] = draft;
            return new DraftBeginResult<TState>(DraftBeginStatus.Created, draft, read.Admission);
        }

        public DraftSession<TState>? Find<TState>(SessionId session, string rackId)
        {
            if (string.IsNullOrEmpty(rackId)) throw new ArgumentException("A rack id is required.", nameof(rackId));
            return _bySession.TryGetValue(session, out var racks) && racks.TryGetValue(rackId, out var entry)
                ? entry as DraftSession<TState>
                : null;
        }

        public bool Contains(SessionId session, string rackId)
        {
            if (string.IsNullOrEmpty(rackId)) throw new ArgumentException("A rack id is required.", nameof(rackId));
            return _bySession.TryGetValue(session, out var racks) && racks.ContainsKey(rackId);
        }

        public int CountOf(SessionId session) => _bySession.TryGetValue(session, out var racks) ? racks.Count : 0;

        /// <summary>The rack ids that have a draft in the session, ordered (ordinal).</summary>
        public IReadOnlyList<string> RackIdsOf(SessionId session) =>
            _bySession.TryGetValue(session, out var racks)
                ? racks.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray()
                : Array.Empty<string>();

        /// <summary>The rack ids of the session whose draft is Dirty or has pending text, ordered (ordinal).</summary>
        public IReadOnlyList<string> RacksAtRiskOf(SessionId session) =>
            _bySession.TryGetValue(session, out var racks)
                ? racks.Values.Where(e => e.IsDirty || e.HasPendingText).Select(e => e.RackId).OrderBy(k => k, StringComparer.Ordinal).ToArray()
                : Array.Empty<string>();

        /// <summary>True while some draft of the whole registry is Applying (at most one can be).</summary>
        public bool HasUpdateInProgress => _applying != null;

        /// <summary>True once the session was retired.</summary>
        public bool IsRetired(SessionId session) => _retired.Contains(session);

        /// <summary>Removes one draft without writing anything. The session stays alive.</summary>
        public DraftDiscardResult Discard(SessionId session, string rackId)
        {
            if (string.IsNullOrEmpty(rackId)) throw new ArgumentException("A rack id is required.", nameof(rackId));
            if (!_bySession.TryGetValue(session, out var racks) || !racks.TryGetValue(rackId, out var entry))
                return new DraftDiscardResult(false, false);

            var wasDirty = entry.IsDirty || entry.HasPendingText;
            racks.Remove(rackId);
            if (racks.Count == 0) _bySession.Remove(session);
            entry.Detach();
            return new DraftDiscardResult(true, wasDirty);
        }

        /// <summary>The explicit discard of the panel: removes every draft of the session without writing anything. The session stays alive.</summary>
        public DraftDiscardAllResult DiscardAll(SessionId session)
        {
            if (!_bySession.TryGetValue(session, out var racks))
                return new DraftDiscardAllResult(Array.Empty<string>(), Array.Empty<string>());

            var removed = racks.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();
            var dirty = racks.Values.Where(e => e.IsDirty || e.HasPendingText).Select(e => e.RackId).OrderBy(k => k, StringComparer.Ordinal).ToArray();
            _bySession.Remove(session);
            foreach (var entry in racks.Values) entry.Detach();
            return new DraftDiscardAllResult(removed, dirty);
        }

        /// <summary>
        /// Retires a destroyed session: removes all its drafts, never resurrects it, and reports the loss with the list
        /// of racks that were Dirty or had pending text.
        /// </summary>
        public DraftLossNotice RetireSession(SessionId session)
        {
            if (!_bySession.TryGetValue(session, out var racks))
            {
                _retired.Add(session);
                return new DraftLossNotice(session, Array.Empty<string>());
            }

            // The loss list is computed first: if the injected comparer throws, nothing was changed and the session is not
            // left half retired.
            var lost = racks.Values.Where(e => e.IsDirty || e.HasPendingText).Select(e => e.RackId).OrderBy(k => k, StringComparer.Ordinal).ToArray();
            _retired.Add(session);
            _bySession.Remove(session);
            foreach (var entry in racks.Values) entry.Detach();
            return new DraftLossNotice(session, lost);
        }

        // ---------------------------------------------------------------- the single Applying slot

        internal bool TryAcquireApplySlot(IDraftEntry entry)
        {
            if (_applying != null) return false;
            _applying = entry;
            return true;
        }

        // The slot belongs to the operation, not to the registration of the draft: discarding or retiring never frees it.
        internal void ReleaseApplySlot(IDraftEntry entry)
        {
            if (ReferenceEquals(_applying, entry)) _applying = null;
        }
    }
}
