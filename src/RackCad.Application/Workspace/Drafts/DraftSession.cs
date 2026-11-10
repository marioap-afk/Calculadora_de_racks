#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RackCad.Application.Workspace.Drafts
{
    /// <summary>Verdict of the admission evaluated by the one logical read of the edit intention (D-11). Carried as a value.</summary>
    public readonly record struct DraftAdmission(bool IsAdmitted, string? Diagnostic)
    {
        public static DraftAdmission Admitted => new(true, null);

        public static DraftAdmission Rejected(string diagnostic)
        {
            if (string.IsNullOrEmpty(diagnostic)) throw new ArgumentException("An admission diagnostic is required.", nameof(diagnostic));
            return new DraftAdmission(false, diagnostic);
        }
    }

    /// <summary>
    /// The result of ONE logical read of the edit intention (D-10): the stale base, the admission and the initial draft
    /// state all come from the same read. It can only be built through <see cref="Admitted"/> or <see cref="NotAdmitted"/>;
    /// a navigation preview has no way to produce one.
    /// </summary>
    public sealed class DraftIntentRead<TState>
    {
        private DraftIntentRead(DraftAdmission admission, DraftStaleBase? staleBase, TState? initialState)
        {
            Admission = admission;
            StaleBase = staleBase;
            InitialState = initialState;
        }

        public DraftAdmission Admission { get; }

        public bool IsAdmitted => Admission.IsAdmitted;

        /// <summary>The base of the read; null when the read was not admitted.</summary>
        public DraftStaleBase? StaleBase { get; }

        /// <summary>The editable state materialized from the same read; default when the read was not admitted.</summary>
        public TState? InitialState { get; }

        public static DraftIntentRead<TState> Admitted(DraftStaleBase staleBase, TState initialState)
        {
            if (staleBase == null) throw new ArgumentNullException(nameof(staleBase));
            return new DraftIntentRead<TState>(DraftAdmission.Admitted, staleBase, initialState);
        }

        public static DraftIntentRead<TState> NotAdmitted(string diagnostic) =>
            new(DraftAdmission.Rejected(diagnostic), null, default);
    }

    /// <summary>
    /// Status of a draft (D-10). Clean and Dirty are derived; the other four are explicit. "Possibly stale" is NOT a
    /// status: it is an orthogonal mark (<see cref="DraftSession{TState}.IsPossiblyStale"/>).
    /// </summary>
    public enum DraftStatus
    {
        Clean,
        Dirty,
        Conflict,
        Applying,
        Unknown,
        Error,
    }

    /// <summary>Why a draft is Unknown (D-13): the commit threw, or the re-read after the write failed.</summary>
    public enum DraftUnknownOrigin
    {
        CommitThrew,
        PostReread,
    }

    /// <summary>Internal boundaries of the panel (D-10). The first three are blocking, the rest are not.</summary>
    public enum DraftBoundary
    {
        TabChange,
        RackChange,
        Update,
        DocumentChange,
        HidePanel,
        Dematerialize,
        DocumentClose,
    }

    public static class DraftBoundaries
    {
        /// <summary>True for the user-initiated boundaries that run the two-phase commit of the pending text (ADR-0032 D6).</summary>
        public static bool IsBlocking(DraftBoundary boundary) =>
            boundary == DraftBoundary.TabChange || boundary == DraftBoundary.RackChange || boundary == DraftBoundary.Update;
    }

    /// <summary>Verdict of the adapter's validator for one pending field.</summary>
    public readonly record struct DraftFieldValidation(bool IsValid, string? Message)
    {
        public static DraftFieldValidation Valid => new(true, null);

        public static DraftFieldValidation Invalid(string message) => new(false, message);
    }

    /// <summary>A pending field that failed validation at a blocking boundary.</summary>
    public readonly record struct DraftInvalidField(string FieldKey, string? Message);

    /// <summary>
    /// What the adapter of a system contributes to the two-phase commit (ADR-0032 D6). The core never interprets the text.
    /// </summary>
    public interface IDraftPendingTextAdapter<TState>
    {
        /// <summary>Phase one: validates one pending text without mutating anything.</summary>
        DraftFieldValidation Validate(string fieldKey, string text);

        /// <summary>Phase two: builds the next state from ALL the pending text, which is already known to be valid.</summary>
        TState Apply(TState current, IReadOnlyDictionary<string, string> pendingText);
    }

    public enum DraftBoundaryDecision
    {
        /// <summary>A blocking boundary: nothing was pending, or all the pending text was valid and was applied in one step.</summary>
        Proceeded,

        /// <summary>A blocking boundary with an invalid field: nothing was applied and all the pending text is untouched.</summary>
        Aborted,

        /// <summary>The pending text was kept as is: a non-blocking boundary (never commits, never repairs), or a draft that cannot commit now.</summary>
        Kept,
    }

    /// <summary>What happened when a boundary was crossed.</summary>
    public sealed class DraftBoundaryResult
    {
        internal DraftBoundaryResult(DraftBoundary boundary, DraftBoundaryDecision decision, bool stateChanged, IReadOnlyList<DraftInvalidField> invalidFields)
        {
            Boundary = boundary;
            Decision = decision;
            CurrentStateChanged = stateChanged;
            InvalidFields = invalidFields;
        }

        public DraftBoundary Boundary { get; }

        public DraftBoundaryDecision Decision { get; }

        /// <summary>True when the pending text was applied to the current draft state.</summary>
        public bool CurrentStateChanged { get; }

        public IReadOnlyList<DraftInvalidField> InvalidFields { get; }
    }

    public enum DraftEditStatus
    {
        Updated,

        /// <summary>The draft is Applying, Unknown, in Error or no longer registered, so nothing changed.</summary>
        NotEditable,
    }

    /// <summary>Result of an edit: whether it was taken and whether the draft is Dirty afterwards.</summary>
    public readonly record struct DraftEditResult(DraftEditStatus Status, bool IsDirty);

    public enum DraftReloadStatus
    {
        Reloaded,

        /// <summary>The read was not admitted: nothing changed and the admission carries the diagnostic.</summary>
        NotAdmitted,

        /// <summary>KeepMineOnCurrentVersion without confirmation: nothing changed.</summary>
        NotConfirmed,

        /// <summary>The draft is Applying, no longer registered or in a status this operation does not serve: nothing changed.</summary>
        NotAllowed,
    }

    public readonly record struct DraftReloadResult(DraftReloadStatus Status, DraftAdmission Admission);

    public enum DraftMarkResult
    {
        Marked,
        AlreadyMarked,
        NotRegistered,
    }

    /// <summary>A status change reported by an operation.</summary>
    public readonly record struct DraftTransition(DraftStatus From, DraftStatus To)
    {
        public bool Changed => From != To;
    }

    public enum DraftCheckStatus
    {
        /// <summary>The draft is not Unknown with origin CommitThrew: nothing changed.</summary>
        NotApplicable,

        /// <summary>The re-read matches the base before the attempt: the draft recovered its previous status and content.</summary>
        Recovered,

        /// <summary>The re-read differs: the draft is now in Conflict.</summary>
        Conflict,
    }

    public readonly record struct DraftCheckResult(DraftCheckStatus Status, DraftStatus DraftStatus, IReadOnlyList<DraftDifference> Differences);

    internal interface IDraftEntry
    {
        SessionId Session { get; }

        string RackId { get; }

        bool IsDirty { get; }

        bool HasPendingText { get; }

        DraftStatus Status { get; }

        void Detach();
    }

    /// <summary>
    /// The draft of one rack in one session (D-10): stale base, initial and current draft state, pending text and status,
    /// all in memory. It is generic over the editable state that already exists in Application; the core defines no
    /// state of any system. It can only be born through <c>DraftRegistry.Begin</c>.
    /// </summary>
    public sealed class DraftSession<TState> : IDraftEntry
    {
        private readonly DraftRegistry _registry;
        private readonly IEqualityComparer<TState> _comparer;
        private readonly Func<TState, TState> _copy;
        private readonly SortedDictionary<string, string> _pending = new(StringComparer.Ordinal);
        private bool _registered;
        private bool _holdsApplySlot;
        private string? _deferredErrorReason;
        private DraftStatus? _explicitStatus;
        private DraftStatus _statusBeforeAttempt;

        internal DraftSession(
            DraftRegistry registry,
            SessionId session,
            string rackId,
            DraftStaleBase staleBase,
            TState initialState,
            IEqualityComparer<TState> comparer,
            Func<TState, TState> copy)
        {
            _registry = registry;
            _registered = true;
            Session = session;
            RackId = rackId;
            StaleBase = staleBase;
            _comparer = comparer;
            _copy = copy;
            InitialDraftState = initialState;
            CurrentDraftState = _copy(initialState);
        }

        public SessionId Session { get; }

        public string RackId { get; }

        public DraftStaleBase StaleBase { get; private set; }

        public TState InitialDraftState { get; private set; }

        public TState CurrentDraftState { get; private set; }

        /// <summary>
        /// The pending text by field key, kept exactly as given. A snapshot whose enumeration is ordered by key (ordinal):
        /// the order is guaranteed by the sorted dictionary behind the read-only view that is exposed.
        /// </summary>
        public IReadOnlyDictionary<string, string> PendingText => PendingSnapshot();

        public bool HasPendingText => _pending.Count > 0;

        /// <summary>True when the comparator says the current state differs from the initial one, or there is pending text (INV-26).</summary>
        public bool IsDirty => _pending.Count > 0 || !_comparer.Equals(CurrentDraftState, InitialDraftState);

        public DraftStatus Status => _explicitStatus ?? (IsDirty ? DraftStatus.Dirty : DraftStatus.Clean);

        /// <summary>The orthogonal "possibly stale" mark. It changes neither the editable state, nor dirty, nor the pending text.</summary>
        public bool IsPossiblyStale { get; private set; }

        /// <summary>True only for a Clean, marked draft: it reloads lazily when it is activated (O-01).</summary>
        public bool NeedsLazyReload => _registered && IsPossiblyStale && Status == DraftStatus.Clean;

        /// <summary>False once the draft was discarded or its session retired.</summary>
        public bool IsRegistered => _registered;

        /// <summary>Not null if and only if the draft is Unknown.</summary>
        public DraftUnknownOrigin? UnknownOrigin { get; private set; }

        /// <summary>What changed, when the draft is in Conflict.</summary>
        public IReadOnlyList<DraftDifference> ConflictDifferences { get; private set; } = Array.Empty<DraftDifference>();

        /// <summary>The cause of the last attempt that did not apply, or of an Unknown outcome.</summary>
        public string? LastApplyCause { get; private set; }

        /// <summary>The warnings of the last AppliedWithWarnings outcome.</summary>
        public IReadOnlyList<string> Warnings { get; private set; } = Array.Empty<string>();

        public string? ErrorReason { get; private set; }

        /// <summary>
        /// The reason recorded by EnterError while an Update was in progress. It is applied when CompleteApply closes that
        /// Update (after the D-13 classification); null otherwise.
        /// </summary>
        public string? DeferredErrorReason => _deferredErrorReason;

        private bool CanEdit => _registered
            && (_explicitStatus == null || _explicitStatus == DraftStatus.Conflict);

        // ---------------------------------------------------------------- the two states never share an object

        private TState SeparatedFromInitial(TState state) =>
            ReferenceEquals(state, InitialDraftState) ? _copy(state) : state;

        private IReadOnlyDictionary<string, string> PendingSnapshot() =>
            new ReadOnlyDictionary<string, string>(new SortedDictionary<string, string>(_pending, StringComparer.Ordinal));

        // ---------------------------------------------------------------- edits

        public DraftEditResult SetCurrentDraftState(TState state)
        {
            if (!CanEdit) return new DraftEditResult(DraftEditStatus.NotEditable, IsDirty);
            CurrentDraftState = SeparatedFromInitial(state);
            return new DraftEditResult(DraftEditStatus.Updated, IsDirty);
        }

        /// <summary>Keeps the text of one field exactly as given (D-10). Empty text is a legitimate pending text.</summary>
        public DraftEditResult SetPendingText(string fieldKey, string text)
        {
            if (string.IsNullOrEmpty(fieldKey)) throw new ArgumentException("A field key is required.", nameof(fieldKey));
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (!CanEdit) return new DraftEditResult(DraftEditStatus.NotEditable, IsDirty);
            _pending[fieldKey] = text;
            return new DraftEditResult(DraftEditStatus.Updated, IsDirty);
        }

        public DraftEditResult RemovePendingText(string fieldKey)
        {
            if (string.IsNullOrEmpty(fieldKey)) throw new ArgumentException("A field key is required.", nameof(fieldKey));
            if (!CanEdit) return new DraftEditResult(DraftEditStatus.NotEditable, IsDirty);
            _pending.Remove(fieldKey);
            return new DraftEditResult(DraftEditStatus.Updated, IsDirty);
        }

        // ---------------------------------------------------------------- boundaries

        /// <summary>
        /// Crosses a boundary (D-10, ADR-0032 D6). Blocking: validate ALL the pending text without mutating; if any field is
        /// invalid, abort and leave everything as it was; otherwise apply all of it in one step to the current state and
        /// clear the pending text. Non-blocking: keep the pending text as is, neither committing nor repairing.
        /// </summary>
        public DraftBoundaryResult CrossBoundary(DraftBoundary boundary, IDraftPendingTextAdapter<TState> adapter)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));
            var none = Array.Empty<DraftInvalidField>();
            if (!DraftBoundaries.IsBlocking(boundary))
                return new DraftBoundaryResult(boundary, DraftBoundaryDecision.Kept, false, none);
            if (_pending.Count == 0)
                return new DraftBoundaryResult(boundary, DraftBoundaryDecision.Proceeded, false, none);
            if (!CanEdit)
                return new DraftBoundaryResult(boundary, DraftBoundaryDecision.Kept, false, none);

            var snapshot = PendingSnapshot();
            var invalid = new List<DraftInvalidField>();
            foreach (var pair in _pending)
            {
                var verdict = adapter.Validate(pair.Key, pair.Value);
                if (!verdict.IsValid) invalid.Add(new DraftInvalidField(pair.Key, verdict.Message));
            }

            if (invalid.Count > 0)
                return new DraftBoundaryResult(boundary, DraftBoundaryDecision.Aborted, false, invalid.ToArray());

            var next = adapter.Apply(CurrentDraftState, snapshot);
            CurrentDraftState = SeparatedFromInitial(next);
            _pending.Clear();
            return new DraftBoundaryResult(boundary, DraftBoundaryDecision.Proceeded, true, none);
        }

        // ---------------------------------------------------------------- possibly stale

        public DraftMarkResult MarkPossiblyStale()
        {
            if (!_registered) return DraftMarkResult.NotRegistered;
            if (IsPossiblyStale) return DraftMarkResult.AlreadyMarked;
            IsPossiblyStale = true;
            return DraftMarkResult.Marked;
        }

        // ---------------------------------------------------------------- reloads and reconciliation

        /// <summary>
        /// Replaces the stale base, the initial state AND the current state from one read, and empties the pending text.
        /// Allowed from every status except Applying.
        /// </summary>
        public DraftReloadResult DiscardAndReload(DraftIntentRead<TState> read)
        {
            if (read == null) throw new ArgumentNullException(nameof(read));
            if (!_registered || _explicitStatus == DraftStatus.Applying)
                return new DraftReloadResult(DraftReloadStatus.NotAllowed, read.Admission);
            if (!read.IsAdmitted)
                return new DraftReloadResult(DraftReloadStatus.NotAdmitted, read.Admission);

            Rebase(read);
            CurrentDraftState = _copy(InitialDraftState);
            _pending.Clear();
            return new DraftReloadResult(DraftReloadStatus.Reloaded, read.Admission);
        }

        /// <summary>
        /// Replaces the stale base and the initial state from one read and KEEPS the current state and the pending text,
        /// only with an explicit confirmation. Served in Dirty and Conflict. There is no automatic merge.
        /// </summary>
        public DraftReloadResult KeepMineOnCurrentVersion(DraftIntentRead<TState> read, bool confirmed)
        {
            if (read == null) throw new ArgumentNullException(nameof(read));
            var status = Status;
            if (!_registered || (status != DraftStatus.Dirty && status != DraftStatus.Conflict))
                return new DraftReloadResult(DraftReloadStatus.NotAllowed, read.Admission);
            if (!confirmed)
                return new DraftReloadResult(DraftReloadStatus.NotConfirmed, read.Admission);
            if (!read.IsAdmitted)
                return new DraftReloadResult(DraftReloadStatus.NotAdmitted, read.Admission);

            Rebase(read);
            CurrentDraftState = SeparatedFromInitial(CurrentDraftState);
            return new DraftReloadResult(DraftReloadStatus.Reloaded, read.Admission);
        }

        private void Rebase(DraftIntentRead<TState> read)
        {
            StaleBase = read.StaleBase!;
            InitialDraftState = read.InitialState!;
            IsPossiblyStale = false;
            _explicitStatus = null;
            UnknownOrigin = null;
            ConflictDifferences = Array.Empty<DraftDifference>();
            LastApplyCause = null;
            Warnings = Array.Empty<string>();
            ErrorReason = null;
        }


        // ---------------------------------------------------------------- Update as a state machine

        /// <summary>
        /// Starts an Update. Only from Clean, or from Dirty without pending text, and only when no other Update is in
        /// progress in the whole registry. It reports the previous status. The single Applying slot of the registry is
        /// taken here and belongs to this operation: only <see cref="CompleteApply"/> frees it.
        /// </summary>
        public DraftApplyStart TryBeginApply()
        {
            var previous = Status;
            if (!_registered) return DraftApplyStart.Refused(previous, DraftApplyRefusal.NotRegistered);
            if (previous != DraftStatus.Clean && previous != DraftStatus.Dirty)
                return DraftApplyStart.Refused(previous, DraftApplyRefusal.InvalidStatus);
            if (_pending.Count > 0) return DraftApplyStart.Refused(previous, DraftApplyRefusal.PendingText);
            if (!_registry.TryAcquireApplySlot(this)) return DraftApplyStart.Refused(previous, DraftApplyRefusal.AnotherUpdateInProgress);

            _holdsApplySlot = true;
            _deferredErrorReason = null;
            _statusBeforeAttempt = previous;
            _explicitStatus = DraftStatus.Applying;
            LastApplyCause = null;
            Warnings = Array.Empty<string>();
            return DraftApplyStart.Started(previous);
        }

        /// <summary>
        /// Closes the Update that is in progress with its outcome (D-13 table). Nothing is ever assumed applied: only the
        /// re-read of POST rebases the draft and leaves it Clean. The outcome is accepted even if the draft was discarded or
        /// its session retired in the meantime: the slot is freed, nothing else changes and the result says so.
        /// </summary>
        public DraftApplyResult CompleteApply(DraftApplyOutcome<TState> outcome)
        {
            if (outcome == null) throw new ArgumentNullException(nameof(outcome));
            if (!_holdsApplySlot)
                return DraftApplyResult.NotApplying(Status, outcome.Kind);

            _holdsApplySlot = false;
            _registry.ReleaseApplySlot(this);
            if (!_registered)
                return DraftApplyResult.ClosedDetached(outcome.Kind, Status);

            _explicitStatus = null;
            var result = Classify(outcome);
            var deferred = _deferredErrorReason;
            if (deferred == null) return result;

            // EnterError arrived while the Update was in flight: the D-13 classification came first, now the draft goes to Error
            // keeping whatever resulted (current state, pending text and stale base).
            _deferredErrorReason = null;
            _explicitStatus = DraftStatus.Error;
            ErrorReason = deferred;
            UnknownOrigin = null;
            return DraftApplyResult.Closed(result.Kind, Status, result.Rebased);
        }

        private DraftApplyResult Classify(DraftApplyOutcome<TState> outcome)
        {
            switch (outcome.Kind)
            {
                case DraftApplyOutcomeKind.NotAppliedConflict:
                    _explicitStatus = DraftStatus.Conflict;
                    ConflictDifferences = outcome.Differences;
                    LastApplyCause = outcome.Cause;
                    return DraftApplyResult.Closed(outcome.Kind, Status, false);

                case DraftApplyOutcomeKind.NotAppliedAdmission:
                case DraftApplyOutcomeKind.NotAppliedPreflight:
                case DraftApplyOutcomeKind.NotAppliedFailure:
                    LastApplyCause = outcome.Cause;
                    return DraftApplyResult.Closed(outcome.Kind, Status, false);

                case DraftApplyOutcomeKind.CommitThrew:
                    _explicitStatus = DraftStatus.Unknown;
                    UnknownOrigin = DraftUnknownOrigin.CommitThrew;
                    LastApplyCause = outcome.Cause;
                    return DraftApplyResult.Closed(outcome.Kind, Status, false);

                case DraftApplyOutcomeKind.Applied:
                case DraftApplyOutcomeKind.AppliedWithWarnings:
                {
                    var read = outcome.PostRead;
                    if (read == null || !read.IsAdmitted)
                    {
                        // The write happened but its re-read cannot rebase the draft: never assume it is clean.
                        _explicitStatus = DraftStatus.Unknown;
                        UnknownOrigin = DraftUnknownOrigin.PostReread;
                        LastApplyCause = read?.Admission.Diagnostic;
                        return DraftApplyResult.Closed(DraftApplyOutcomeKind.AppliedPostRereadFailed, Status, false);
                    }

                    Rebase(read);
                    CurrentDraftState = _copy(InitialDraftState);
                    _pending.Clear();
                    if (outcome.Kind == DraftApplyOutcomeKind.AppliedWithWarnings) Warnings = outcome.Warnings;
                    return DraftApplyResult.Closed(outcome.Kind, Status, true);
                }

                default:
                    _explicitStatus = DraftStatus.Unknown;
                    UnknownOrigin = DraftUnknownOrigin.PostReread;
                    LastApplyCause = outcome.Cause;
                    return DraftApplyResult.Closed(DraftApplyOutcomeKind.AppliedPostRereadFailed, Status, false);
            }
        }

        /// <summary>
        /// "Check now" (D-13, O3-06), only for Unknown with origin CommitThrew. If the re-read matches the base from before
        /// the attempt, the draft recovers exactly its previous status and content; otherwise it goes to Conflict.
        /// </summary>
        public DraftCheckResult CheckNow(DraftStaleBase reread)
        {
            if (reread == null) throw new ArgumentNullException(nameof(reread));
            if (!_registered || _explicitStatus != DraftStatus.Unknown || UnknownOrigin != DraftUnknownOrigin.CommitThrew)
                return new DraftCheckResult(DraftCheckStatus.NotApplicable, Status, Array.Empty<DraftDifference>());

            var comparison = StaleBase.CompareWith(reread);
            UnknownOrigin = null;
            if (comparison.IsMatch)
            {
                _explicitStatus = null;
                LastApplyCause = null;
                return new DraftCheckResult(DraftCheckStatus.Recovered, Status, Array.Empty<DraftDifference>());
            }

            _explicitStatus = DraftStatus.Conflict;
            ConflictDifferences = comparison.Differences;
            return new DraftCheckResult(DraftCheckStatus.Conflict, Status, comparison.Differences);
        }

        /// <summary>The status the draft had when the current or last attempt began.</summary>
        public DraftStatus StatusBeforeAttempt => _statusBeforeAttempt;

        // ---------------------------------------------------------------- error

        /// <summary>
        /// Puts the draft in Error keeping the current state, the pending text and the stale base. It leaves Error only
        /// through DiscardAndReload or Discard. While an Update is in progress it changes neither the status nor the slot: the
        /// reason is recorded and applied when CompleteApply closes that Update.
        /// </summary>
        public DraftTransition EnterError(string reason)
        {
            if (string.IsNullOrEmpty(reason)) throw new ArgumentException("A reason is required.", nameof(reason));
            var from = Status;
            if (!_registered) return new DraftTransition(from, from);
            if (_explicitStatus == DraftStatus.Applying)
            {
                _deferredErrorReason = reason;
                return new DraftTransition(from, from);
            }

            _explicitStatus = DraftStatus.Error;
            ErrorReason = reason;
            UnknownOrigin = null;
            return new DraftTransition(from, DraftStatus.Error);
        }

        // ---------------------------------------------------------------- registry plumbing

        /// <summary>
        /// Unregisters the draft. It never frees the Applying slot: that belongs to the operation in flight and only
        /// CompleteApply releases it.
        /// </summary>
        void IDraftEntry.Detach()
        {
            _registered = false;
        }
    }
}
