#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    /// <summary>
    /// What the explicit discard of ONE draft did (I-64 F4-T2B2B): whether it was removed and whether it had pending text. Both facts are
    /// known without the comparer. For a rack without draft both are false.
    /// </summary>
    public readonly record struct DraftUserDiscardResult(bool Removed, bool HadPendingText);

    /// <summary>
    /// What the explicit discard of EVERY draft of a session did (I-64 F4-T2B2B): the racks removed and, among them, those that had
    /// pending text, each list ordered by rack id (ordinal) and never null.
    /// </summary>
    public sealed class DraftUserDiscardAllResult
    {
        internal DraftUserDiscardAllResult(IReadOnlyList<string> removedRackIds, IReadOnlyList<string> pendingTextRackIds)
        {
            RemovedRackIds = removedRackIds;
            PendingTextRackIds = pendingTextRackIds;
        }

        public IReadOnlyList<string> RemovedRackIds { get; }

        public IReadOnlyList<string> PendingTextRackIds { get; }
    }

    /// <summary>
    /// The visible notice of loss (D-17, INV-20): the racks whose Dirty or pending drafts disappeared with the session. Since I-64
    /// F4-T2B2B it also says which of them are UNCERTAIN (the comparer failed while deciding, so they may or may not have been dirty)
    /// and why; only the conservative retirement fills those two members, and both are contained in the notice, never null.
    /// </summary>
    public sealed class DraftLossNotice
    {
        internal DraftLossNotice(SessionId session, IReadOnlyList<string> lostRackIds)
            : this(session, lostRackIds, Array.Empty<string>(), NoReasons())
        {
        }

        internal DraftLossNotice(
            SessionId session,
            IReadOnlyList<string> lostRackIds,
            IReadOnlyList<string> uncertainRackIds,
            IReadOnlyDictionary<string, string> uncertainReasons)
        {
            Session = session;
            LostRackIds = lostRackIds;
            UncertainRackIds = uncertainRackIds;
            UncertainReasons = uncertainReasons;
        }

        public SessionId Session { get; }

        public IReadOnlyList<string> LostRackIds { get; }

        /// <summary>The racks of <see cref="LostRackIds"/> whose dirty state could not be decided; ordered (ordinal) and never null.</summary>
        public IReadOnlyList<string> UncertainRackIds { get; }

        /// <summary>The message of the exception of the comparer by uncertain rack id; read-only, ordinal and never null.</summary>
        public IReadOnlyDictionary<string, string> UncertainReasons { get; }

        public bool HasLoss => LostRackIds.Count > 0;

        private static IReadOnlyDictionary<string, string> NoReasons() =>
            new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal));
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

        // ---------------------------------------------------------------- explicit operations that never depend on the comparer (I-64 F4-T2B2B)
        //
        // The ordinary operations above decide from the comparer BEFORE they remove anything, so a comparer that always throws would
        // make a draft impossible to remove. The four operations below are additive and keep those semantics untouched: they remove
        // and detach what they touch whatever the comparer says, none of them ever throws because of it, none writes anything and
        // none frees the Applying slot. Their lists are ordered (ordinal) and never null.

        /// <summary>
        /// The explicit discard of ONE draft by the user: the user chose to lose it, so no comparison is needed and none is made. Only
        /// the pending text is known without the comparer, and it is what the result says. The session stays alive.
        /// </summary>
        public DraftUserDiscardResult DiscardByUser(SessionId session, string rackId)
        {
            if (string.IsNullOrEmpty(rackId)) throw new ArgumentException("A rack id is required.", nameof(rackId));
            if (!_bySession.TryGetValue(session, out var racks) || !racks.TryGetValue(rackId, out var entry))
                return new DraftUserDiscardResult(false, false);

            var hadPendingText = entry.HasPendingText;
            racks.Remove(rackId);
            if (racks.Count == 0) _bySession.Remove(session);
            entry.Detach();
            return new DraftUserDiscardResult(true, hadPendingText);
        }

        /// <summary>The explicit discard of EVERY draft of the session by the user; no comparison is made. The session stays alive.</summary>
        public DraftUserDiscardAllResult DiscardAllByUser(SessionId session)
        {
            if (!_bySession.TryGetValue(session, out var racks))
                return new DraftUserDiscardAllResult(Array.Empty<string>(), Array.Empty<string>());

            var removed = racks.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();
            var pending = racks.Values.Where(e => e.HasPendingText).Select(e => e.RackId).OrderBy(k => k, StringComparer.Ordinal).ToArray();
            _bySession.Remove(session);
            foreach (var entry in racks.Values) entry.Detach();
            return new DraftUserDiscardAllResult(removed, pending);
        }

        /// <summary>
        /// Retires a session whose drafts are KNOWN to be clean: it removes them all without calling the comparer and marks the session
        /// as retired. Only the drafts with pending text are lost.
        /// </summary>
        public DraftLossNotice RetireKnownSafe(SessionId session)
        {
            if (!_bySession.TryGetValue(session, out var racks))
            {
                _retired.Add(session);
                return new DraftLossNotice(session, Array.Empty<string>());
            }

            var lost = racks.Values.Where(e => e.HasPendingText).Select(e => e.RackId).OrderBy(k => k, StringComparer.Ordinal).ToArray();
            _retired.Add(session);
            _bySession.Remove(session);
            foreach (var entry in racks.Values) entry.Detach();
            return new DraftLossNotice(session, lost);
        }

        /// <summary>
        /// Retires a session whose drafts are NOT known to be clean: it removes them all and marks the session as retired, and it never
        /// infers "clean". Draft by draft, pending text comes first (a draft with pending text is lost and never uncertain); then the
        /// comparer decides, and a comparer that throws makes THAT draft lost and uncertain, with the message of its exception, without
        /// stopping the retirement of the others.
        /// </summary>
        public DraftLossNotice RetireConservative(SessionId session)
        {
            if (!_bySession.TryGetValue(session, out var racks))
            {
                _retired.Add(session);
                return new DraftLossNotice(session, Array.Empty<string>());
            }

            var lost = new List<string>();
            var uncertain = new List<string>();
            var reasons = new SortedDictionary<string, string>(StringComparer.Ordinal);
            foreach (var entry in racks.Values)
            {
                if (entry.HasPendingText)
                {
                    lost.Add(entry.RackId);
                    continue;
                }

                try
                {
                    if (entry.IsDirty) lost.Add(entry.RackId);
                }
                catch (Exception ex)
                {
                    lost.Add(entry.RackId);
                    uncertain.Add(entry.RackId);
                    reasons[entry.RackId] = ex.Message ?? string.Empty;
                }
            }

            _retired.Add(session);
            _bySession.Remove(session);
            foreach (var entry in racks.Values) entry.Detach();
            return new DraftLossNotice(
                session,
                lost.OrderBy(k => k, StringComparer.Ordinal).ToArray(),
                uncertain.OrderBy(k => k, StringComparer.Ordinal).ToArray(),
                new ReadOnlyDictionary<string, string>(reasons));
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
