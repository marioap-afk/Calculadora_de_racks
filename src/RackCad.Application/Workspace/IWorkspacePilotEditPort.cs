#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4-T3A (D-T3-02): the one port through which the panel edits the bounded field of the Push Back pilot without ever
    /// writing the drawing. The host core implements it explicitly over the in-memory work of F4-T1 and F4-T2: the pending text of
    /// the field, the gesture of the field itself, the boundaries that block and the ones that do not, the discard of one rack or of
    /// the whole session, the racks at risk and the two queues of notices (loss and veto).
    ///
    /// Rules of the port:
    /// - every result is an immutable value; its lists are never null, also for a default value;
    /// - no type of the in-memory work and no type of the pilot crosses it, and the racks are plain strings;
    /// - the session travels in the request (<see cref="WorkspaceRequest"/> or <see cref="EditIntentRequest"/>), except for
    ///   <see cref="TakeLossNotices"/>, whose notices belong to sessions that no longer exist;
    /// - only the three operations of the edit intention read the drawing, each in one action of the user; none of the operations
    ///   writes anything;
    /// - every exception is contained as a value with a reason and the session is not degraded; the one of <see cref="GetRacksAtRisk"/>
    ///   is contained as the unknown value (<see cref="PilotRiskView.IsKnown"/> false) and its reason goes to the log;
    /// - while one of the three reads is in progress, every operation that changes something answers Busy and a second read answers
    ///   Waiting, all without effect, so a key pressed during a read is never lost in silence.
    /// </summary>
    public interface IWorkspacePilotEditPort
    {
        /// <summary>The edit intention of the user for one rack of the pilot. Same contract as the host operation it delegates to.</summary>
        EditIntentOutcome TryBeginEdit(EditIntentRequest request);

        /// <summary>"Check now" for one rack of the pilot. Same contract as the host operation it delegates to.</summary>
        EditCheckOutcome TryCheckNow(EditIntentRequest request);

        /// <summary>The lazy reload of one rack of the pilot. Same contract as the host operation it delegates to.</summary>
        EditIntentOutcome TryReload(EditIntentRequest request);

        /// <summary>The state of the field of one rack. Memory only: it authorizes nothing, reads nothing and never throws.</summary>
        PilotFieldView GetField(WorkspaceRequest origin, string rackId);

        /// <summary>
        /// Keeps the text that the user typed in the field, exactly as typed (an empty text is kept too). Only an edit of the user calls
        /// it; showing a value never does. It needs the authorization, an editable field and no read in progress.
        /// </summary>
        PilotFieldResult SetPendingText(WorkspaceRequest origin, string rackId, string text);

        /// <summary>
        /// The gesture of the field itself (Enter, or leaving the field): the same two-phase step as a blocking boundary. Without
        /// pending text nothing changes; a valid text is applied and the pending text is cleaned; an invalid text answers Invalid and
        /// stays exactly as typed, with no repair.
        /// </summary>
        PilotFieldResult CommitField(WorkspaceRequest origin, string rackId);

        /// <summary>
        /// Crosses one boundary of the panel. A blocking one (a change of surface or of rack) needs the authorization: an invalid text
        /// aborts it with nothing changed, a valid one proceeds and is applied. A non-blocking one needs no authorization and keeps
        /// the pending text as it is.
        /// </summary>
        PilotBoundaryResult CrossBoundary(WorkspaceRequest origin, string rackId, PilotBoundaryKind kind);

        /// <summary>The explicit discard of ONE rack: its work and its pending text go away, no comparison is made and nothing is written.</summary>
        EditDiscardOutcome DiscardRack(WorkspaceRequest origin, string rackId);

        /// <summary>The explicit discard of the work of every rack of the session, with the same contract as the host operation.</summary>
        EditDiscardOutcome DiscardDocument(WorkspaceRequest origin);

        /// <summary>
        /// The racks of the session whose work would be lost. Memory only: it authorizes nothing and never throws. When it could not
        /// be decided it answers the default value, whose <see cref="PilotRiskView.IsKnown"/> is false: never "nothing at risk".
        /// </summary>
        PilotRiskView GetRacksAtRisk(WorkspaceRequest origin);

        /// <summary>
        /// Takes, oldest first, every notice of lost work that nobody consumed, and leaves the queue empty. A notice whose conversion
        /// fails is lost: it is taken out of the queue and stays only in the log, and the next ones are still taken.
        /// </summary>
        IReadOnlyList<PilotLossView> TakeLossNotices();

        /// <summary>
        /// Takes, oldest first, every notice of a vetoed close that nobody consumed, and leaves the queue empty. If taking them fails it
        /// answers an empty list and the queue is NOT emptied: the notices are taken again at the next presentation.
        /// </summary>
        IReadOnlyList<PilotCloseView> TakeCloseNotices();
    }

    /// <summary>The state of the field of one rack, as the port reports it. The default value is NotOpen.</summary>
    public enum PilotFieldState
    {
        /// <summary>There is no work for the rack.</summary>
        NotOpen,

        Clean,
        Dirty,
        Conflict,
        Applying,

        /// <summary>The outcome of a write is not known.</summary>
        Unknown,

        Error,

        /// <summary>
        /// It cannot be decided whether the field is changed: the comparison failed. It counts as protected. It is reported only when the
        /// ordinary state, Clean or Dirty, cannot be decided. When the draft has an explicit state (Conflict, Applying, Unknown or Error)
        /// that state is kept, whatever the comparison says, and the field is protected all the same.
        /// </summary>
        Undecidable,
    }

    /// <summary>How an operation on the field ended. The order is fixed and the default value is Rejected: it means that nothing was done.</summary>
    public enum PilotFieldStatus
    {
        /// <summary>Nothing was done: the request does not belong to the active document, or the operation failed and was contained.</summary>
        Rejected,

        Applied,

        /// <summary>The field cannot be edited now (no work for the rack, or a state that does not allow it): nothing was done.</summary>
        NotEditable,

        /// <summary>A read of the pilot is in progress: nothing was done.</summary>
        Busy,

        /// <summary>The text is not valid: nothing was applied and the text stays exactly as typed.</summary>
        Invalid,
    }

    /// <summary>What happened when a boundary was crossed. The order is fixed.</summary>
    public enum PilotBoundaryDecision
    {
        /// <summary>The transition may go on: there was nothing pending, or the pending text was valid and was applied.</summary>
        Proceeded,

        /// <summary>The transition must not go on: the pending text is invalid, the request is not authorized or the operation failed. Nothing changed.</summary>
        Aborted,

        /// <summary>
        /// The pending text was kept as it is. For a non-blocking boundary it is the only answer. For a blocking one it means that the
        /// draft cannot be edited now (for example it is in error, unknown or being applied): nothing was validated or applied, the
        /// pending text is intact and the transition goes on, exactly as for <see cref="Proceeded"/>.
        /// </summary>
        Kept,

        /// <summary>A read of the pilot is in progress: nothing changed and the transition must not go on.</summary>
        Busy,
    }

    /// <summary>The boundaries of the panel, as an ordered prefix: later tasks may add values at the end.</summary>
    public enum PilotBoundaryKind
    {
        TabChange,
        RackChange,
        DocumentChange,
        PanelHide,
        Dematerialize,
        DocumentClose,
    }

    /// <summary>One field whose pending text is not valid, with the message of the validation.</summary>
    public readonly record struct PilotFieldProblem(string FieldKey, string? Message);

    /// <summary>
    /// The state of the field of one rack. <see cref="CommittedText"/> is the committed value shown with the culture of the user, or
    /// null with its <see cref="CommittedTextReason"/>; <see cref="PendingText"/> is what the user typed and was not applied yet.
    /// </summary>
    public readonly struct PilotFieldView
    {
        public PilotFieldView(
            bool exists,
            PilotFieldState state,
            bool isPossiblyStale,
            bool isProtected,
            bool canEdit,
            string? committedText,
            string? committedTextReason,
            string? pendingText,
            string? pendingMessage,
            string? errorReason = null)
        {
            Exists = exists;
            State = state;
            IsPossiblyStale = isPossiblyStale;
            IsProtected = isProtected;
            CanEdit = canEdit;
            CommittedText = committedText;
            CommittedTextReason = committedTextReason;
            PendingText = pendingText;
            PendingMessage = pendingMessage;
            ErrorReason = errorReason;
        }

        /// <summary>True when there is work for the rack.</summary>
        public bool Exists { get; }

        public PilotFieldState State { get; }

        public bool IsPossiblyStale { get; }

        /// <summary>True when closing the document would lose something of this rack; also when that could not be decided.</summary>
        public bool IsProtected { get; }

        public bool CanEdit { get; }

        public string? CommittedText { get; }

        /// <summary>Why <see cref="CommittedText"/> is null; null otherwise.</summary>
        public string? CommittedTextReason { get; }

        public string? PendingText { get; }

        /// <summary>The message of the validation of <see cref="PendingText"/>; null when there is none or it is valid.</summary>
        public string? PendingMessage { get; }

        /// <summary>
        /// The reason of the Error of the work, exactly as whoever produced the Error gave it; null in every other state and when none was
        /// given.
        /// </summary>
        public string? ErrorReason { get; }
    }

    /// <summary>The answer to an operation on the field. The view is set for Applied and Invalid; the reason for the other statuses.</summary>
    public readonly struct PilotFieldResult
    {
        private PilotFieldResult(PilotFieldStatus status, PilotFieldView view, string? reason, string? message)
        {
            Status = status;
            View = view;
            Reason = reason;
            Message = message;
        }

        public PilotFieldStatus Status { get; }

        public PilotFieldView View { get; }

        /// <summary>Why nothing was done; null for Applied and Invalid.</summary>
        public string? Reason { get; }

        /// <summary>The message of the validation for Invalid; null otherwise.</summary>
        public string? Message { get; }

        public static PilotFieldResult Applied(PilotFieldView view) => new(PilotFieldStatus.Applied, view, null, null);

        public static PilotFieldResult Rejected(string reason) => new(PilotFieldStatus.Rejected, default, reason, null);

        public static PilotFieldResult NotEditable(string reason, PilotFieldView view) => new(PilotFieldStatus.NotEditable, view, reason, null);

        public static PilotFieldResult Busy(string reason) => new(PilotFieldStatus.Busy, default, reason, null);

        public static PilotFieldResult Invalid(PilotFieldView view, string? message) => new(PilotFieldStatus.Invalid, view, null, message);
    }

    /// <summary>
    /// The answer to the crossing of a boundary. <see cref="InvalidFields"/> is never null, also for a default value.
    /// The default value of this struct is a real answer, TabChange with Proceeded, and nothing tells it from a genuine one: so no
    /// producer of this port and no double of it may ever return a default <see cref="PilotBoundaryResult"/>; every answer is built
    /// with the constructor, with its kind and its decision.
    /// </summary>
    public readonly struct PilotBoundaryResult
    {
        private readonly IReadOnlyList<PilotFieldProblem>? _invalidFields;

        public PilotBoundaryResult(
            PilotBoundaryKind kind,
            PilotBoundaryDecision decision,
            bool stateChanged,
            IReadOnlyList<PilotFieldProblem> invalidFields,
            string? reason)
        {
            Kind = kind;
            Decision = decision;
            StateChanged = stateChanged;
            _invalidFields = (invalidFields ?? throw new ArgumentNullException(nameof(invalidFields))).ToArray();
            Reason = reason;
        }

        public PilotBoundaryKind Kind { get; }

        public PilotBoundaryDecision Decision { get; }

        /// <summary>True when the pending text was applied to the value of the field.</summary>
        public bool StateChanged { get; }

        /// <summary>The fields with their messages when the boundary was aborted by an invalid text; empty otherwise.</summary>
        public IReadOnlyList<PilotFieldProblem> InvalidFields => _invalidFields ?? Array.Empty<PilotFieldProblem>();

        /// <summary>Why the boundary was aborted or refused without a validation; null otherwise.</summary>
        public string? Reason { get; }
    }

    /// <summary>
    /// The racks of a session whose work would be lost. Both lists are ordered (ordinal) and never null.
    /// The value built with the constructor is known (<see cref="IsKnown"/> is true). The default value is the unknown: it means that
    /// it could not be decided which racks are at risk, and then both lists are empty and say nothing. It is never "nothing at risk".
    /// </summary>
    public readonly struct PilotRiskView
    {
        private readonly bool _known;
        private readonly IReadOnlyList<string>? _rackIds;
        private readonly IReadOnlyList<string>? _uncertainRackIds;

        public PilotRiskView(IReadOnlyList<string> rackIds, IReadOnlyList<string> uncertainRackIds)
        {
            _known = true;
            _rackIds = (rackIds ?? throw new ArgumentNullException(nameof(rackIds))).ToArray();
            _uncertainRackIds = (uncertainRackIds ?? throw new ArgumentNullException(nameof(uncertainRackIds))).ToArray();
        }

        /// <summary>
        /// True when the lists say what is at risk. False for the default value: it could not be decided, and the lists mean nothing.
        /// </summary>
        public bool IsKnown => _known;

        /// <summary>The racks whose work is known to be at risk.</summary>
        public IReadOnlyList<string> RackIds => _rackIds ?? Array.Empty<string>();

        /// <summary>The racks whose comparison failed: they count as at risk, apart from the ones above.</summary>
        public IReadOnlyList<string> UncertainRackIds => _uncertainRackIds ?? Array.Empty<string>();
    }

    /// <summary>A notice of lost work: the session that disappeared, the racks lost and, apart, the uncertain ones with their reasons.</summary>
    public readonly struct PilotLossView
    {
        private readonly IReadOnlyList<string>? _lostRackIds;
        private readonly IReadOnlyList<string>? _uncertainRackIds;
        private readonly IReadOnlyDictionary<string, string>? _uncertainReasons;

        public PilotLossView(
            SessionId session,
            IReadOnlyList<string> lostRackIds,
            IReadOnlyList<string> uncertainRackIds,
            IReadOnlyDictionary<string, string> uncertainReasons)
        {
            Session = session;
            _lostRackIds = (lostRackIds ?? throw new ArgumentNullException(nameof(lostRackIds))).ToArray();
            _uncertainRackIds = (uncertainRackIds ?? throw new ArgumentNullException(nameof(uncertainRackIds))).ToArray();
            _uncertainReasons = new Dictionary<string, string>(
                uncertainReasons ?? throw new ArgumentNullException(nameof(uncertainReasons)),
                StringComparer.Ordinal);
        }

        public SessionId Session { get; }

        public IReadOnlyList<string> LostRackIds => _lostRackIds ?? Array.Empty<string>();

        public IReadOnlyList<string> UncertainRackIds => _uncertainRackIds ?? Array.Empty<string>();

        /// <summary>The reason by uncertain rack; never null.</summary>
        public IReadOnlyDictionary<string, string> UncertainReasons =>
            _uncertainReasons ?? new Dictionary<string, string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// A notice of a vetoed close: the session of the document and the racks at risk that caused the veto. When the evaluation
    /// failed, <see cref="IsUncertain"/> is true and the list is empty: the notice does not know which racks are at risk.
    /// </summary>
    public readonly struct PilotCloseView
    {
        private readonly IReadOnlyList<string>? _rackIds;

        public PilotCloseView(SessionId session, IReadOnlyList<string> rackIds, bool isUncertain)
        {
            Session = session;
            _rackIds = (rackIds ?? throw new ArgumentNullException(nameof(rackIds))).ToArray();
            IsUncertain = isUncertain;
        }

        public SessionId Session { get; }

        public IReadOnlyList<string> RackIds => _rackIds ?? Array.Empty<string>();

        public bool IsUncertain { get; }
    }
}
