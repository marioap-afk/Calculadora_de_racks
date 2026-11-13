#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F4-T3B1-EDIT-SURFACE: the simulated edit port of the pilot for the tests of the panel. It records every call by operation, with
    /// its origin, rack, text and kind, so a test can prove which session and rack travelled and which operations were never called.
    /// The state of the field of each rack is programmable. Every answer is explicit and never a default value: a refusal is built
    /// with its reason and a boundary is built with its kind and decision.
    ///
    /// Without a responder it behaves as a minimal draft: the intention creates a clean draft, the reload clears the stale mark, the
    /// text is kept as pending (the draft turns dirty and protected), the commit applies a text that <see cref="Validate"/> accepts and
    /// answers Invalid for one it rejects, and a blocking boundary aborts on an invalid pending text and, on a valid one, proceeds having
    /// applied it and cleared the pending text, as the host does. A hook can run an action inside a
    /// read, to model a read that pumps messages.
    /// </summary>
    internal sealed class WorkspacePilotEditPortDouble : IWorkspacePilotEditPort
    {
        public const string BeginOp = "TryBeginEdit";
        public const string CheckOp = "TryCheckNow";
        public const string ReloadOp = "TryReload";
        public const string GetFieldOp = "GetField";
        public const string SetPendingOp = "SetPendingText";
        public const string CommitOp = "CommitField";
        public const string BoundaryOp = "CrossBoundary";
        public const string DiscardRackOp = "DiscardRack";
        public const string DiscardDocumentOp = "DiscardDocument";
        public const string RisksOp = "GetRacksAtRisk";
        public const string LossOp = "TakeLossNotices";
        public const string CloseOp = "TakeCloseNotices";

        /// <param name="Operation">The operation of the port.</param>
        /// <param name="Origin">The session that travelled with the call.</param>
        /// <param name="RackId">The rack of the call; null for the operations that have none.</param>
        /// <param name="Text">The text of SetPendingText.</param>
        /// <param name="Kind">The boundary of CrossBoundary.</param>
        public sealed record Call(string Operation, SessionId Origin, string? RackId, string? Text = null, PilotBoundaryKind? Kind = null);

        private readonly Dictionary<(SessionId, string), PilotFieldView> _fields = new();

        public List<Call> Calls { get; } = new();

        public int Count(string operation) => Calls.Count(c => c.Operation == operation);

        public IEnumerable<Call> Of(string operation) => Calls.Where(c => c.Operation == operation);

        /// <summary>The operations that were called at least once.</summary>
        public HashSet<string> Operations() => Calls.Select(c => c.Operation).ToHashSet(StringComparer.Ordinal);

        public IEnumerable<Call> Reads() => Calls.Where(c => c.Operation == BeginOp || c.Operation == CheckOp || c.Operation == ReloadOp);

        /// <summary>Runs inside TryBeginEdit, TryCheckNow and TryReload, as a read that pumps messages would.</summary>
        public Action<Call>? ReadHook { get; set; }

        /// <summary>The validation of the text: the message when it is not valid, null when it is. Valid by default.</summary>
        public Func<string, string?> Validate { get; set; } = _ => null;

        public Func<EditIntentRequest, EditIntentOutcome>? BeginResponder { get; set; }

        public Func<EditIntentRequest, EditIntentOutcome>? ReloadResponder { get; set; }

        public Func<WorkspaceRequest, string, string, PilotFieldResult>? SetPendingResponder { get; set; }

        public Func<WorkspaceRequest, string, PilotFieldResult>? CommitResponder { get; set; }

        public Func<WorkspaceRequest, string, PilotBoundaryKind, PilotBoundaryResult>? BoundaryResponder { get; set; }

        /// <summary>The operations that throw: the exception is thrown after the call is recorded.</summary>
        public Dictionary<string, Exception> Throws { get; } = new(StringComparer.Ordinal);

        // ------------------------------------------------------------------ the state of the fields

        public static PilotFieldView View(
            PilotFieldState state = PilotFieldState.Clean,
            string? committed = "1:50",
            string? pending = null,
            string? message = null,
            bool stale = false,
            bool isProtected = false,
            bool canEdit = true,
            string? committedReason = null) =>
            new PilotFieldView(true, state, stale, isProtected, canEdit, committed, committedReason, pending, message);

        public static PilotFieldView NoDraft() =>
            new PilotFieldView(false, PilotFieldState.NotOpen, false, false, false, null, null, null, null);

        public void Set(SessionId session, string rack, PilotFieldView view) => _fields[(session, rack)] = view;

        public void Remove(SessionId session, string rack) => _fields.Remove((session, rack));

        public PilotFieldView Current(SessionId session, string rack) =>
            _fields.TryGetValue((session, rack), out var view) ? view : NoDraft();

        // ------------------------------------------------------------------ the port

        public EditIntentOutcome TryBeginEdit(EditIntentRequest request)
        {
            var call = Record(BeginOp, request.Session, request.RackId);
            ReadHook?.Invoke(call);
            ThrowIfAsked(BeginOp);
            if (BeginResponder != null) return BeginResponder(request);

            if (_fields.ContainsKey((request.Session, request.RackId))) return EditIntentOutcome.AlreadyOpen();
            Set(request.Session, request.RackId, View());
            return EditIntentOutcome.Admitted(EditWitnessStatus.NotSupplied);
        }

        public EditCheckOutcome TryCheckNow(EditIntentRequest request)
        {
            var call = Record(CheckOp, request.Session, request.RackId);
            ReadHook?.Invoke(call);
            ThrowIfAsked(CheckOp);
            return EditCheckOutcome.Unavailable("The double does not check.");
        }

        public EditIntentOutcome TryReload(EditIntentRequest request)
        {
            var call = Record(ReloadOp, request.Session, request.RackId);
            ReadHook?.Invoke(call);
            ThrowIfAsked(ReloadOp);
            if (ReloadResponder != null) return ReloadResponder(request);

            var key = (request.Session, request.RackId);
            if (!_fields.TryGetValue(key, out var view)) return EditIntentOutcome.NotFound("There is no draft.");
            _fields[key] = View(view.State, view.CommittedText, view.PendingText, view.PendingMessage, false, view.IsProtected, view.CanEdit);
            return EditIntentOutcome.Reloaded(EditWitnessStatus.NotSupplied);
        }

        public PilotFieldView GetField(WorkspaceRequest origin, string rackId)
        {
            Record(GetFieldOp, origin.Origin, rackId);
            ThrowIfAsked(GetFieldOp);
            return Current(origin.Origin, rackId);
        }

        public PilotFieldResult SetPendingText(WorkspaceRequest origin, string rackId, string text)
        {
            Calls.Add(new Call(SetPendingOp, origin.Origin, rackId, Text: text));
            ThrowIfAsked(SetPendingOp);
            if (SetPendingResponder != null) return SetPendingResponder(origin, rackId, text);

            var key = (origin.Origin, rackId);
            if (!_fields.TryGetValue(key, out var view)) return PilotFieldResult.NotEditable("There is no draft.", NoDraft());
            if (!view.CanEdit) return PilotFieldResult.NotEditable("The field cannot be edited now.", view);

            var next = View(PilotFieldState.Dirty, view.CommittedText, text, Validate(text), view.IsPossiblyStale, true, view.CanEdit);
            _fields[key] = next;
            return PilotFieldResult.Applied(next);
        }

        public PilotFieldResult CommitField(WorkspaceRequest origin, string rackId)
        {
            Record(CommitOp, origin.Origin, rackId);
            ThrowIfAsked(CommitOp);
            if (CommitResponder != null) return CommitResponder(origin, rackId);

            var key = (origin.Origin, rackId);
            if (!_fields.TryGetValue(key, out var view)) return PilotFieldResult.NotEditable("There is no draft.", NoDraft());
            if (!view.CanEdit) return PilotFieldResult.NotEditable("The field cannot be edited now.", view);
            if (view.PendingText == null) return PilotFieldResult.Applied(view);

            var message = Validate(view.PendingText);
            if (message != null) return PilotFieldResult.Invalid(view, message);

            var next = View(PilotFieldState.Dirty, view.PendingText, null, null, view.IsPossiblyStale, true, view.CanEdit);
            _fields[key] = next;
            return PilotFieldResult.Applied(next);
        }

        public PilotBoundaryResult CrossBoundary(WorkspaceRequest origin, string rackId, PilotBoundaryKind kind)
        {
            Calls.Add(new Call(BoundaryOp, origin.Origin, rackId, Kind: kind));
            ThrowIfAsked(BoundaryOp);
            if (BoundaryResponder != null) return BoundaryResponder(origin, rackId, kind);

            var none = Array.Empty<PilotFieldProblem>();
            var blocking = kind == PilotBoundaryKind.TabChange || kind == PilotBoundaryKind.RackChange;
            if (!blocking) return new PilotBoundaryResult(kind, PilotBoundaryDecision.Kept, false, none, null);

            var view = Current(origin.Origin, rackId);
            if (!view.Exists || view.PendingText == null) return new PilotBoundaryResult(kind, PilotBoundaryDecision.Proceeded, false, none, null);

            var message = Validate(view.PendingText);
            if (message == null)
            {
                // As the host does (D-T3-02): a valid pending text proceeds, is applied and leaves the draft with no pending text.
                _fields[(origin.Origin, rackId)] = View(PilotFieldState.Dirty, view.PendingText, null, null, view.IsPossiblyStale, true, view.CanEdit);
                return new PilotBoundaryResult(kind, PilotBoundaryDecision.Proceeded, true, none, null);
            }

            var problems = new[] { new PilotFieldProblem("AnnotationScale", message) };
            return new PilotBoundaryResult(kind, PilotBoundaryDecision.Aborted, false, problems, null);
        }

        public EditDiscardOutcome DiscardRack(WorkspaceRequest origin, string rackId)
        {
            Record(DiscardRackOp, origin.Origin, rackId);
            return EditDiscardOutcome.Rejected("The double does not discard.");
        }

        public EditDiscardOutcome DiscardDocument(WorkspaceRequest origin)
        {
            Record(DiscardDocumentOp, origin.Origin, null);
            return EditDiscardOutcome.Rejected("The double does not discard.");
        }

        public PilotRiskView GetRacksAtRisk(WorkspaceRequest origin)
        {
            Record(RisksOp, origin.Origin, null);
            return new PilotRiskView(Array.Empty<string>(), Array.Empty<string>());
        }

        public IReadOnlyList<PilotLossView> TakeLossNotices()
        {
            Record(LossOp, default, null);
            return Array.Empty<PilotLossView>();
        }

        public IReadOnlyList<PilotCloseView> TakeCloseNotices()
        {
            Record(CloseOp, default, null);
            return Array.Empty<PilotCloseView>();
        }

        private Call Record(string operation, SessionId origin, string? rackId)
        {
            var call = new Call(operation, origin, rackId);
            Calls.Add(call);
            return call;
        }

        private void ThrowIfAsked(string operation)
        {
            if (Throws.TryGetValue(operation, out var exception)) throw exception;
        }
    }
}
