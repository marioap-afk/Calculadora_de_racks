#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.Systems.PushBack.Pilot;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4-T2B2A (D-T2B2-03, D-T2B2-06) — what the preview of a navigation showed: the key of the definition and the raw
    /// text of its envelope. The host hands it to the one read of the edit intention, which says whether it still holds (INV-27).
    /// </summary>
    public sealed record EditIntentWitness(string Key, string? Text);

    /// <summary>
    /// I-64 F4-T2B2A (D-T2B2-06) — one request of the user to open or to check a pilot edit: the session that asks, the rack, the
    /// reference the user came from (or none) and the witness of the preview (or none). Every request carries its session (INV-22).
    /// </summary>
    public sealed record EditIntentRequest(SessionId Session, string RackId, long? ChosenReference, EditIntentWitness? Witness);

    /// <summary>How the one read of the edit intention ended, as the host adapter reports it (D-T2B2-03).</summary>
    public enum EditIntentReadStatus
    {
        Read,
        NoActiveDocument,
        Busy,
        Rejected,
        Failed,
    }

    /// <summary>
    /// I-64 F4-T2B2A (D-T2B2-03) — the value the host adapter answers with. <see cref="PilotRead"/> is set if and only if the
    /// status is <see cref="EditIntentReadStatus.Read"/>; every other status carries only its reason.
    /// </summary>
    public readonly struct EditIntentReadResult
    {
        private EditIntentReadResult(EditIntentReadStatus status, string? reason, PushBackPilotIntentResult? pilotRead)
        {
            Status = status;
            Reason = reason;
            PilotRead = pilotRead;
        }

        public EditIntentReadStatus Status { get; }

        public string? Reason { get; }

        /// <summary>The read of the pilot; null unless the status is Read.</summary>
        public PushBackPilotIntentResult? PilotRead { get; }

        public static EditIntentReadResult FromPilot(PushBackPilotIntentResult pilotRead)
        {
            if (pilotRead == null) throw new ArgumentNullException(nameof(pilotRead));
            return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead);
        }

        public static EditIntentReadResult NoActiveDocument(string reason) => new(EditIntentReadStatus.NoActiveDocument, reason, null);

        public static EditIntentReadResult Busy(string reason) => new(EditIntentReadStatus.Busy, reason, null);

        public static EditIntentReadResult Rejected(string reason) => new(EditIntentReadStatus.Rejected, reason, null);

        public static EditIntentReadResult Failed(string reason) => new(EditIntentReadStatus.Failed, reason, null);
    }

    /// <summary>
    /// How the preview witness compares with the read of the intention (INV-27), as the host reports it. <see cref="NotEvaluated"/>
    /// is for every outcome that has no read behind it: it is never "unchanged".
    /// </summary>
    public enum EditWitnessStatus
    {
        NotEvaluated,
        NotSupplied,
        Unchanged,
        Changed,
    }

    public enum EditIntentStatus
    {
        /// <summary>No reader of the edit intention was assigned: nothing was read.</summary>
        Unavailable,

        /// <summary>No active document, a RackCad modal, a command or no rest: the intention waits. Nothing was read.</summary>
        Waiting,

        /// <summary>
        /// The request is refused and no draft is created. Three ways lead here. First, INV-22: the session of the request is not the
        /// one of the active document, which is decided before anything about the drafts and before any read of the drawing. Second,
        /// the reader answers Rejected, which happens after the gate and after the read was attempted. Third, the SessionRetired answer
        /// of step 8 (a session that was retired when the draft was begun), also after the gate and the read. The last two are
        /// reached AFTER the read, so a Rejected outcome does not by itself mean that nothing was read.
        /// </summary>
        Rejected,

        /// <summary>There is already a draft for (session, rack): nothing was read.</summary>
        AlreadyOpen,

        /// <summary>The read was not admitted: no draft exists, and the outcome carries the diagnostic.</summary>
        NotAdmitted,

        /// <summary>The draft was born from the one read.</summary>
        Admitted,

        /// <summary>A failure of the host during the operation: contained, and the session is not degraded.</summary>
        Failed,

        // The three states below belong to the reload (I-64 F4-T2B2B, D-T2B2-12). They come AFTER the seven of the intention, which
        // stay as an ordered prefix, and the default value of this enum is still Unavailable.

        /// <summary>The reload found no draft for (session, rack): nothing was read.</summary>
        NotFound,

        /// <summary>The draft is not marked as possibly stale and clean, so it does not need to be reloaded: nothing was read.</summary>
        NotNeeded,

        /// <summary>The draft was replaced from the one read: its stale base, initial state and current state are the new ones.</summary>
        Reloaded,
    }

    /// <summary>
    /// I-64 F4-T2B2A (D-T2B2-06) — the answer of the host to the intention of editing a rack of the pilot. A value: it carries no
    /// draft and no type of the pilot. Every state carries the status of the witness.
    /// </summary>
    public readonly struct EditIntentOutcome
    {
        private EditIntentOutcome(
            EditIntentStatus status,
            EditWitnessStatus witness,
            string? reason,
            string? diagnostic,
            string? detail,
            string? classicEditorHint)
        {
            Status = status;
            Witness = witness;
            Reason = reason;
            Diagnostic = diagnostic;
            Detail = detail;
            ClassicEditorHint = classicEditorHint;
        }

        public EditIntentStatus Status { get; }

        public EditWitnessStatus Witness { get; }

        /// <summary>Why the operation did not read or did not finish; null for AlreadyOpen, NotAdmitted and Admitted.</summary>
        public string? Reason { get; }

        /// <summary>The visible diagnostic of a read that was not admitted; null otherwise.</summary>
        public string? Diagnostic { get; }

        /// <summary>The technical detail behind the diagnostic; never part of its main text.</summary>
        public string? Detail { get; }

        /// <summary>The hint to the classic editor; present when the read was not admitted.</summary>
        public string? ClassicEditorHint { get; }

        public bool IsAdmitted => Status == EditIntentStatus.Admitted;

        public static EditIntentOutcome Unavailable(string reason) => Without(EditIntentStatus.Unavailable, reason);

        public static EditIntentOutcome Waiting(string reason) => Without(EditIntentStatus.Waiting, reason);

        public static EditIntentOutcome Rejected(string reason, EditWitnessStatus witness = EditWitnessStatus.NotEvaluated) =>
            new(EditIntentStatus.Rejected, witness, reason, null, null, null);

        public static EditIntentOutcome AlreadyOpen() => Without(EditIntentStatus.AlreadyOpen, null);

        public static EditIntentOutcome NotAdmitted(string diagnostic, string? detail, string? classicEditorHint, EditWitnessStatus witness) =>
            new(EditIntentStatus.NotAdmitted, witness, null, diagnostic, detail, classicEditorHint);

        public static EditIntentOutcome Admitted(EditWitnessStatus witness) =>
            new(EditIntentStatus.Admitted, witness, null, null, null, null);

        public static EditIntentOutcome Failed(string reason) => Without(EditIntentStatus.Failed, reason);

        public static EditIntentOutcome NotFound(string reason) => Without(EditIntentStatus.NotFound, reason);

        public static EditIntentOutcome NotNeeded(string reason) => Without(EditIntentStatus.NotNeeded, reason);

        public static EditIntentOutcome Reloaded(EditWitnessStatus witness) =>
            new(EditIntentStatus.Reloaded, witness, null, null, null, null);

        private static EditIntentOutcome Without(EditIntentStatus status, string? reason) =>
            new(status, EditWitnessStatus.NotEvaluated, reason, null, null, null);
    }

    public enum EditCheckStatus
    {
        Unavailable,
        Waiting,
        Rejected,

        /// <summary>There is no draft for (session, rack).</summary>
        NotFound,

        /// <summary>The draft is not unknown after a commit that threw: nothing was read.</summary>
        NotApplicable,

        /// <summary>The reread differs from the base before the attempt: the draft is now in conflict.</summary>
        Conflict,

        /// <summary>The reread matches the base before the attempt: the draft recovered its previous status.</summary>
        Recovered,

        Failed,
    }

    /// <summary>
    /// I-64 F4-T2B2A (D-T2B2-06) — the answer of the host to "check now". A value: the differences are text, so it names no draft
    /// type and no type of the pilot.
    /// </summary>
    public readonly struct EditCheckOutcome
    {
        private readonly IReadOnlyList<string>? _differences;

        private EditCheckOutcome(EditCheckStatus status, string? reason, IReadOnlyList<string> differences)
        {
            Status = status;
            Reason = reason;
            _differences = differences;
        }

        public EditCheckStatus Status { get; }

        public string? Reason { get; }

        /// <summary>
        /// What differs, as "key:kind" lines (the variable registry has no key); empty unless the status is Conflict. Never null,
        /// also for a default value of this struct.
        /// </summary>
        public IReadOnlyList<string> Differences => _differences ?? Array.Empty<string>();

        public static EditCheckOutcome Unavailable(string reason) => Without(EditCheckStatus.Unavailable, reason);

        public static EditCheckOutcome Waiting(string reason) => Without(EditCheckStatus.Waiting, reason);

        public static EditCheckOutcome Rejected(string reason) => Without(EditCheckStatus.Rejected, reason);

        public static EditCheckOutcome NotFound(string reason) => Without(EditCheckStatus.NotFound, reason);

        public static EditCheckOutcome NotApplicable(string reason) => Without(EditCheckStatus.NotApplicable, reason);

        public static EditCheckOutcome Conflict(IReadOnlyList<string> differences)
        {
            if (differences == null) throw new ArgumentNullException(nameof(differences));
            return new EditCheckOutcome(EditCheckStatus.Conflict, null, differences);
        }

        public static EditCheckOutcome Recovered() => Without(EditCheckStatus.Recovered, null);

        public static EditCheckOutcome Failed(string reason) => Without(EditCheckStatus.Failed, reason);

        private static EditCheckOutcome Without(EditCheckStatus status, string? reason) =>
            new(status, reason, Array.Empty<string>());
    }

    /// <summary>
    /// How the explicit discard of the drafts of a session ended (I-64 F4-T2B2B, D-T2B2-15). The order is fixed and the default value
    /// is Rejected: it is inert, it means that nothing was done.
    /// </summary>
    public enum EditDiscardStatus
    {
        /// <summary>The request does not belong to the active document (INV-22): nothing was done. It is also the default value.</summary>
        Rejected,

        /// <summary>Every draft of the session was removed (possibly none): nothing was read and nothing was written.</summary>
        Discarded,

        /// <summary>A failure of the host during the operation: contained, and the session is not degraded.</summary>
        Failed,
    }

    /// <summary>
    /// I-64 F4-T2B2B (D-T2B2-15) — the answer of the host to the explicit discard of every draft of a session. A value: the racks are
    /// text, so it names no draft type. Both lists are never null, also for a default value of this struct.
    /// </summary>
    public readonly struct EditDiscardOutcome
    {
        private readonly IReadOnlyList<string>? _removedRackIds;
        private readonly IReadOnlyList<string>? _pendingTextRackIds;

        private EditDiscardOutcome(EditDiscardStatus status, string? reason, IReadOnlyList<string> removed, IReadOnlyList<string> pendingText)
        {
            Status = status;
            Reason = reason;
            _removedRackIds = removed;
            _pendingTextRackIds = pendingText;
        }

        public EditDiscardStatus Status { get; }

        /// <summary>Why nothing was discarded; null for Discarded.</summary>
        public string? Reason { get; }

        /// <summary>The racks whose draft was removed, ordered (ordinal); empty unless the status is Discarded.</summary>
        public IReadOnlyList<string> RemovedRackIds => _removedRackIds ?? Array.Empty<string>();

        /// <summary>The removed racks that had pending text, ordered (ordinal); empty unless the status is Discarded.</summary>
        public IReadOnlyList<string> PendingTextRackIds => _pendingTextRackIds ?? Array.Empty<string>();

        public static EditDiscardOutcome Rejected(string reason) =>
            new(EditDiscardStatus.Rejected, reason, Array.Empty<string>(), Array.Empty<string>());

        public static EditDiscardOutcome Discarded(IReadOnlyList<string> removedRackIds, IReadOnlyList<string> pendingTextRackIds)
        {
            if (removedRackIds == null) throw new ArgumentNullException(nameof(removedRackIds));
            if (pendingTextRackIds == null) throw new ArgumentNullException(nameof(pendingTextRackIds));
            return new EditDiscardOutcome(EditDiscardStatus.Discarded, null, removedRackIds, pendingTextRackIds);
        }

        public static EditDiscardOutcome Failed(string reason) =>
            new(EditDiscardStatus.Failed, reason, Array.Empty<string>(), Array.Empty<string>());
    }

    /// <summary>The pure decisions of the edit intention that do not depend on the host (I-64 F4-T2B2A).</summary>
    public static class WorkspaceEditIntent
    {
        /// <summary>
        /// The definition of origin of the read (D-11, D-T2B2-04), from the targets of the SAME operation:
        /// the definition of the chosen reference when it is still among the targets; 0 when it is not (the read then says that
        /// the origin is no longer there, and it does not say that it belongs to another rack); without a chosen reference, the
        /// definition of the first target of the deterministic order, or 0 when there are no targets.
        /// </summary>
        public static long ChooseOriginDefinition(NavigationTargetSet set, long? chosenReference)
        {
            if (set == null) throw new ArgumentNullException(nameof(set));

            if (!chosenReference.HasValue) return set.Primary?.DefinitionHandle ?? 0;

            foreach (var target in set.Targets)
                if (target.ReferenceHandle == chosenReference.Value) return target.DefinitionHandle;

            return 0;
        }

        /// <summary>Translates the status of the witness of the pilot into the one of the host: the same three, plus the one for no read.</summary>
        public static EditWitnessStatus WitnessStatusOf(PushBackPilotPreviewWitnessStatus status)
        {
            switch (status)
            {
                case PushBackPilotPreviewWitnessStatus.Unchanged: return EditWitnessStatus.Unchanged;
                case PushBackPilotPreviewWitnessStatus.Changed: return EditWitnessStatus.Changed;
                case PushBackPilotPreviewWitnessStatus.NotSupplied: return EditWitnessStatus.NotSupplied;
                default: return EditWitnessStatus.Changed;
            }
        }
    }
}
