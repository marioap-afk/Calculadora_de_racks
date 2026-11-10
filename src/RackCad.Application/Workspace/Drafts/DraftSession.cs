#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace.Drafts
{
    /// <summary>Verdict of the admission evaluated by the one logical read of the edit intention (D-11). Carried as a value.</summary>
    public readonly record struct DraftAdmission(bool IsAdmitted, string? Diagnostic)
    {
        public static DraftAdmission Admitted => new(true, null);

        public static DraftAdmission Rejected(string diagnostic) => new(false, diagnostic);
    }

    /// <summary>
    /// The result of ONE logical read of the edit intention (D-10): the stale base, the admission and the initial draft
    /// state all come from the same read. It can only be built through <see cref="Admitted"/> or <see cref="NotAdmitted"/>.
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

        public DraftStaleBase? StaleBase { get; }

        public TState? InitialState { get; }

        public static DraftIntentRead<TState> Admitted(DraftStaleBase staleBase, TState initialState) =>
            new(DraftAdmission.Admitted, staleBase, initialState);

        public static DraftIntentRead<TState> NotAdmitted(string diagnostic) =>
            new(DraftAdmission.Rejected(diagnostic), null, default);
    }

    /// <summary>Status of a draft (D-10). Clean and Dirty are derived; the other four are explicit.</summary>
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
        public static bool IsBlocking(DraftBoundary boundary) => false;
    }

    /// <summary>Verdict of the adapter's validator for one pending field.</summary>
    public readonly record struct DraftFieldValidation(bool IsValid, string? Message)
    {
        public static DraftFieldValidation Valid => new(true, null);

        public static DraftFieldValidation Invalid(string message) => new(false, message);
    }

    /// <summary>A pending field that failed validation at a blocking boundary.</summary>
    public readonly record struct DraftInvalidField(string FieldKey, string? Message);

    /// <summary>What the adapter of a system contributes to the two-phase commit (ADR-0032 D6).</summary>
    public interface IDraftPendingTextAdapter<TState>
    {
        /// <summary>Phase one: validates one pending text without mutating anything.</summary>
        DraftFieldValidation Validate(string fieldKey, string text);

        /// <summary>Phase two: builds the next state from ALL the pending text, which is already known to be valid.</summary>
        TState Apply(TState current, IReadOnlyDictionary<string, string> pendingText);
    }

    public enum DraftBoundaryDecision
    {
        Proceeded,
        Aborted,
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

        public bool CurrentStateChanged { get; }

        public IReadOnlyList<DraftInvalidField> InvalidFields { get; }
    }

    public enum DraftEditStatus
    {
        Updated,
        NotEditable,
    }

    /// <summary>Result of an edit: whether it was taken and whether the draft is Dirty afterwards.</summary>
    public readonly record struct DraftEditResult(DraftEditStatus Status, bool IsDirty);

    public enum DraftReloadStatus
    {
        Reloaded,
        NotAdmitted,
        NotConfirmed,
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
        NotApplicable,
        Recovered,
        Conflict,
    }

    public readonly record struct DraftCheckResult(DraftCheckStatus Status, DraftStatus DraftStatus, IReadOnlyList<DraftDifference> Differences);

    /// <summary>RED skeleton of the generic draft: it holds the first state and every operation is inert.</summary>
    public sealed class DraftSession<TState>
    {
        internal DraftSession(DraftRegistry owner, SessionId session, string rackId, DraftStaleBase staleBase, TState initialState, IEqualityComparer<TState> comparer)
        {
            Session = session;
            RackId = rackId;
            StaleBase = staleBase;
            InitialDraftState = initialState;
            CurrentDraftState = initialState;
        }

        public SessionId Session { get; }

        public string RackId { get; }

        public DraftStaleBase StaleBase { get; }

        public TState InitialDraftState { get; }

        public TState CurrentDraftState { get; }

        public IReadOnlyDictionary<string, string> PendingText => new Dictionary<string, string>();

        public bool HasPendingText => false;

        public bool IsDirty => false;

        public DraftStatus Status => DraftStatus.Clean;

        public bool IsPossiblyStale => false;

        public bool NeedsLazyReload => false;

        public bool IsRegistered => false;

        public DraftUnknownOrigin? UnknownOrigin => null;

        public IReadOnlyList<DraftDifference> ConflictDifferences => Array.Empty<DraftDifference>();

        public string? LastApplyCause => null;

        public IReadOnlyList<string> Warnings => Array.Empty<string>();

        public string? ErrorReason => null;

        public DraftStatus StatusBeforeAttempt => DraftStatus.Clean;

        public DraftEditResult SetCurrentDraftState(TState state) => new(DraftEditStatus.NotEditable, false);

        public DraftEditResult SetPendingText(string fieldKey, string text) => new(DraftEditStatus.NotEditable, false);

        public DraftEditResult RemovePendingText(string fieldKey) => new(DraftEditStatus.NotEditable, false);

        public DraftBoundaryResult CrossBoundary(DraftBoundary boundary, IDraftPendingTextAdapter<TState> adapter) =>
            new(boundary, DraftBoundaryDecision.Kept, false, Array.Empty<DraftInvalidField>());

        public DraftMarkResult MarkPossiblyStale() => DraftMarkResult.NotRegistered;

        public DraftReloadResult DiscardAndReload(DraftIntentRead<TState> read) => new(DraftReloadStatus.NotAllowed, read.Admission);

        public DraftReloadResult KeepMineOnCurrentVersion(DraftIntentRead<TState> read, bool confirmed) => new(DraftReloadStatus.NotAllowed, read.Admission);

        public DraftApplyStart TryBeginApply() => new(false, DraftStatus.Clean, DraftApplyRefusal.NotRegistered);

        public DraftApplyResult CompleteApply(DraftApplyOutcome<TState> outcome) => new(false, outcome.Kind, DraftStatus.Clean, false);

        public DraftCheckResult CheckNow(DraftStaleBase reread) =>
            new(DraftCheckStatus.NotApplicable, DraftStatus.Clean, Array.Empty<DraftDifference>());

        public DraftTransition EnterError(string reason) => new(DraftStatus.Clean, DraftStatus.Clean);
    }
}
