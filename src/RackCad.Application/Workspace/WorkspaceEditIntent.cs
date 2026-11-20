#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Draft;
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
        private EditIntentReadResult(EditIntentReadStatus status, string? reason, PushBackPilotIntentResult? pilotRead, PushBackFullIntentResult? fullRead, RackCatalog? catalog)
        {
            Status = status;
            Reason = reason;
            PilotRead = pilotRead;
            FullRead = fullRead;
            Catalog = catalog;
        }

        public EditIntentReadStatus Status { get; }

        public string? Reason { get; }

        /// <summary>The read of the pilot; null unless the status is Read.</summary>
        public PushBackPilotIntentResult? PilotRead { get; }

        /// <summary>I-64 F4B PB-T4a: the read of the full model over the same scan; null unless the read came from <see cref="FromReads"/>.</summary>
        public PushBackFullIntentResult? FullRead { get; }

        /// <summary>I-64 F4B PB-T4a: the catalog of the read; null unless the read came from <see cref="FromReads"/>.</summary>
        public RackCatalog? Catalog { get; }

        public static EditIntentReadResult FromPilot(PushBackPilotIntentResult pilotRead)
        {
            if (pilotRead == null) throw new ArgumentNullException(nameof(pilotRead));
            return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, null, null);
        }

        public static EditIntentReadResult FromReads(PushBackPilotIntentResult pilotRead, PushBackFullIntentResult fullRead, RackCatalog catalog)
        {
            if (pilotRead == null) throw new ArgumentNullException(nameof(pilotRead));
            if (fullRead == null) throw new ArgumentNullException(nameof(fullRead));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            return new EditIntentReadResult(EditIntentReadStatus.Read, null, pilotRead, fullRead, catalog);
        }

        public static EditIntentReadResult NoActiveDocument(string reason) => new(EditIntentReadStatus.NoActiveDocument, reason, null, null, null);

        public static EditIntentReadResult Busy(string reason) => new(EditIntentReadStatus.Busy, reason, null, null, null);

        public static EditIntentReadResult Rejected(string reason) => new(EditIntentReadStatus.Rejected, reason, null, null, null);

        public static EditIntentReadResult Failed(string reason) => new(EditIntentReadStatus.Failed, reason, null, null, null);
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

        /// <summary>
        /// The intention waits: no active document, a RackCad modal, a command or no rest, or (through the edit port of the panel) a
        /// read of the pilot already in progress or a notice of the panel being delivered. In all those cases nothing was read. In the
        /// reload it can also arrive AFTER the read: the draft was being applied or was no longer registered when the read came back,
        /// and then it was not replaced.
        /// </summary>
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

        /// <summary>
        /// The read was not admitted and the outcome carries the diagnostic. In the intention no draft exists; in the reload the draft
        /// that was there is kept as it was.
        /// </summary>
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

        // The state below belongs to "keep my draft" (I-64 F4-T4A1, D-T4A1-03). It comes AFTER the ten above, which stay as an ordered prefix.

        /// <summary>
        /// The draft was kept on the current version, after an explicit confirmation: its stale base and initial state are the new ones and
        /// its edits and its pending text stay.
        /// </summary>
        Kept,
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

        public static EditIntentOutcome Kept(EditWitnessStatus witness) =>
            new(EditIntentStatus.Kept, witness, null, null, null, null);

        private static EditIntentOutcome Without(EditIntentStatus status, string? reason) =>
            new(status, EditWitnessStatus.NotEvaluated, reason, null, null, null);
    }

    public enum EditCheckStatus
    {
        Unavailable,

        /// <summary>
        /// The check waits and nothing was read. Besides the reasons of the intention (no active document, a RackCad modal, a command or
        /// no rest), through the edit port of the panel there are two more: a read of the pilot is already in progress, or a notice of
        /// the panel is being delivered.
        /// </summary>
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

        private EditCheckOutcome(
            EditCheckStatus status,
            string? reason,
            IReadOnlyList<string> differences,
            string? admissionDiagnostic,
            string? classicEditorHint)
        {
            Status = status;
            Reason = reason;
            _differences = differences;
            AdmissionDiagnostic = admissionDiagnostic;
            ClassicEditorHint = classicEditorHint;
        }

        public EditCheckStatus Status { get; }

        public string? Reason { get; }

        /// <summary>
        /// What differs, as "key:kind" lines (the variable registry has no key); empty unless the status is Conflict. Never null,
        /// also for a default value of this struct.
        /// </summary>
        public IReadOnlyList<string> Differences => _differences ?? Array.Empty<string>();

        /// <summary>
        /// The diagnostic of the admission of the reread, set for a Recovered or a Conflict whose reread was not admitted (I-64 F4-T4C2, D-T4C-12);
        /// null otherwise. It never changes the status: the status only comes from the comparison of the bases.
        /// </summary>
        public string? AdmissionDiagnostic { get; }

        /// <summary>The hint to the classic editor; set only next to a diagnostic, null otherwise.</summary>
        public string? ClassicEditorHint { get; }

        public static EditCheckOutcome Unavailable(string reason) => Without(EditCheckStatus.Unavailable, reason);

        public static EditCheckOutcome Waiting(string reason) => Without(EditCheckStatus.Waiting, reason);

        public static EditCheckOutcome Rejected(string reason) => Without(EditCheckStatus.Rejected, reason);

        public static EditCheckOutcome NotFound(string reason) => Without(EditCheckStatus.NotFound, reason);

        public static EditCheckOutcome NotApplicable(string reason) => Without(EditCheckStatus.NotApplicable, reason);

        public static EditCheckOutcome Conflict(IReadOnlyList<string> differences, string? admissionDiagnostic = null, string? classicEditorHint = null)
        {
            if (differences == null) throw new ArgumentNullException(nameof(differences));
            return new EditCheckOutcome(EditCheckStatus.Conflict, null, differences, admissionDiagnostic, classicEditorHint);
        }

        public static EditCheckOutcome Recovered(string? admissionDiagnostic = null, string? classicEditorHint = null) =>
            new(EditCheckStatus.Recovered, null, Array.Empty<string>(), admissionDiagnostic, classicEditorHint);

        public static EditCheckOutcome Failed(string reason) => Without(EditCheckStatus.Failed, reason);

        private static EditCheckOutcome Without(EditCheckStatus status, string? reason) =>
            new(status, reason, Array.Empty<string>(), null, null);
    }

    public enum EditConfirmStatus
    {
        /// <summary>No reader of the edit intention was assigned: nothing was read. It is also the default value.</summary>
        Unavailable,

        /// <summary>
        /// The check waits and nothing was read: no active document, a RackCad modal, a command, or a read or a notice in progress. The
        /// reload and the keep answer Waiting for a draft that is being applied; this check answers NotApplicable for it instead, and the
        /// panel presents both as "not checked".
        /// </summary>
        Waiting,

        /// <summary>The session of the request is not the one of the active document: nothing was read.</summary>
        Rejected,

        /// <summary>There is no draft for (session, rack): nothing was read.</summary>
        NotFound,

        /// <summary>
        /// The draft is not clean, dirty or in conflict, so it is not checked. A draft that is being applied answers this before anything is
        /// read; a draft that starts to be applied, or leaves the registry, while the one read is in progress answers this after the read.
        /// </summary>
        NotApplicable,

        /// <summary>The reread matches the base of the draft. A draft in conflict stays in conflict.</summary>
        Confirmed,

        /// <summary>The reread differs from the base of the draft: the draft is now in conflict, with the differences.</summary>
        Conflict,

        /// <summary>A failure of the host during the operation: contained, and the session is not degraded.</summary>
        Failed,
    }

    /// <summary>
    /// I-64 F4-T4A1 (D-T4A1-03) — the answer of the host to the general "check now". A value: the differences are text, so it names no
    /// draft type and no type of the pilot. The default value is Unavailable, which says that nothing was done.
    /// </summary>
    public readonly struct EditConfirmOutcome
    {
        private readonly IReadOnlyList<string>? _differences;

        private EditConfirmOutcome(
            EditConfirmStatus status,
            string? reason,
            IReadOnlyList<string> differences,
            string? admissionDiagnostic,
            string? classicEditorHint)
        {
            Status = status;
            Reason = reason;
            _differences = differences;
            AdmissionDiagnostic = admissionDiagnostic;
            ClassicEditorHint = classicEditorHint;
        }

        public EditConfirmStatus Status { get; }

        public string? Reason { get; }

        /// <summary>What differs, as "key:kind" lines; empty unless the status is Conflict. Never null, also for a default value.</summary>
        public IReadOnlyList<string> Differences => _differences ?? Array.Empty<string>();

        /// <summary>The diagnostic of the admission of the read; set for a Confirmed or a Conflict whose read was not admitted, null otherwise.</summary>
        public string? AdmissionDiagnostic { get; }

        /// <summary>The hint to the classic editor; set for a Confirmed or a Conflict whose read was not admitted (and so only next to a diagnostic), null otherwise.</summary>
        public string? ClassicEditorHint { get; }

        public static EditConfirmOutcome Unavailable(string reason) => Without(EditConfirmStatus.Unavailable, reason);

        public static EditConfirmOutcome Waiting(string reason) => Without(EditConfirmStatus.Waiting, reason);

        public static EditConfirmOutcome Rejected(string reason) => Without(EditConfirmStatus.Rejected, reason);

        public static EditConfirmOutcome NotFound(string reason) => Without(EditConfirmStatus.NotFound, reason);

        public static EditConfirmOutcome NotApplicable(string reason) => Without(EditConfirmStatus.NotApplicable, reason);

        public static EditConfirmOutcome Confirmed(string? admissionDiagnostic = null, string? classicEditorHint = null) =>
            new(EditConfirmStatus.Confirmed, null, Array.Empty<string>(), admissionDiagnostic, classicEditorHint);

        public static EditConfirmOutcome Conflict(IReadOnlyList<string> differences, string? admissionDiagnostic = null, string? classicEditorHint = null)
        {
            if (differences == null) throw new ArgumentNullException(nameof(differences));
            return new EditConfirmOutcome(EditConfirmStatus.Conflict, null, differences, admissionDiagnostic, classicEditorHint);
        }

        public static EditConfirmOutcome Failed(string reason) => Without(EditConfirmStatus.Failed, reason);

        private static EditConfirmOutcome Without(EditConfirmStatus status, string? reason) =>
            new(status, reason, Array.Empty<string>(), null, null);
    }

    /// <summary>
    /// How an update of the pilot ended (I-64 F4-T4B, D-T4B-07). The order is the one of the package and the default value is
    /// <see cref="Unavailable"/>: it is inert, it means that nothing was done. From <see cref="NotAppliedConflict"/> to
    /// <see cref="AppliedPostRereadFailed"/> the names are the ones of the outcomes of the classification by frontier.
    /// </summary>
    public enum EditUpdateStatus
    {
        /// <summary>Nothing was done. It is also the default value.</summary>
        Unavailable,

        /// <summary>The gate refused: the session, the document or a modal of RackCad. Nothing was read and nothing was written.</summary>
        Rejected,

        /// <summary>There is no draft for the rack, or the rack has no identity.</summary>
        NoDraft,

        /// <summary>The draft is clean: there is nothing to apply.</summary>
        NothingToApply,

        /// <summary>The draft has pending text: its boundary must be crossed first.</summary>
        PendingText,

        /// <summary>The draft is not clean or dirty: it cannot be updated now.</summary>
        NotApplicable,

        /// <summary>Another update is in progress in the registry.</summary>
        AnotherUpdateInProgress,

        /// <summary>The draft refused to begin: it is not registered, it is in a status that cannot update, or it has pending text.</summary>
        Refused,

        NotAppliedConflict,
        NotAppliedAdmission,
        NotAppliedPreflight,
        NotAppliedFailure,
        CommitThrew,
        Applied,
        AppliedWithWarnings,
        AppliedPostRereadFailed,

        /// <summary>The outcome was accepted but the draft was discarded or its session retired meanwhile.</summary>
        ClosedDetached,

        /// <summary>
        /// The status of the draft could not be computed and the draft is now in Error, with its reason, or the draft did not accept the
        /// close of the update. Either way the outcome carries the classification that the update had (M-03), and it is never a success.
        /// </summary>
        ClosedInError,

        /// <summary>A failure of the host BEFORE the update began: contained, and the session is not degraded.</summary>
        Failed,
    }

    /// <summary>
    /// I-64 F4-T4B (D-T4B-07) — the answer of the host to an update of the pilot. A value: the differences and the warnings are text, so
    /// it names no draft type and no type of the pilot. Both lists are never null, also for a default value of this struct.
    /// </summary>
    public readonly struct EditUpdateOutcome
    {
        private readonly IReadOnlyList<string>? _differences;
        private readonly IReadOnlyList<string>? _warnings;

        private EditUpdateOutcome(
            EditUpdateStatus status,
            string? reason,
            IReadOnlyList<string> differences,
            IReadOnlyList<string> warnings,
            EditUpdateStatus classification = EditUpdateStatus.Unavailable,
            string? cause = null)
        {
            Status = status;
            Reason = reason;
            _differences = differences;
            _warnings = warnings;
            Classification = classification;
            Cause = cause;
        }

        public EditUpdateStatus Status { get; }

        /// <summary>
        /// The classification by frontier that the update ended with: the status itself for the eight classified outcomes, the one computed
        /// from the close for <see cref="EditUpdateStatus.ClosedInError"/> (M-03), and Unavailable for every other outcome.
        /// </summary>
        public EditUpdateStatus Classification { get; }

        /// <summary>The cause of the classified outcome of a ClosedInError, exactly as the update gave it; null otherwise.</summary>
        public string? Cause { get; }

        /// <summary>Why the update did not apply, or the cause of an uncertain outcome; null when there is none.</summary>
        public string? Reason { get; }

        /// <summary>What differs, as "key:kind" lines; empty unless the status is NotAppliedConflict. Never null.</summary>
        public IReadOnlyList<string> Differences => _differences ?? Array.Empty<string>();

        /// <summary>The warnings of the update; empty unless there were some. Never null.</summary>
        public IReadOnlyList<string> Warnings => _warnings ?? Array.Empty<string>();

        public static EditUpdateOutcome Unavailable(string reason) => Without(EditUpdateStatus.Unavailable, reason);

        public static EditUpdateOutcome Rejected(string reason) => Without(EditUpdateStatus.Rejected, reason);

        public static EditUpdateOutcome NoDraft(string reason) => Without(EditUpdateStatus.NoDraft, reason);

        public static EditUpdateOutcome NothingToApply(string reason) => Without(EditUpdateStatus.NothingToApply, reason);

        public static EditUpdateOutcome PendingText(string reason) => Without(EditUpdateStatus.PendingText, reason);

        public static EditUpdateOutcome NotApplicable(string reason) => Without(EditUpdateStatus.NotApplicable, reason);

        public static EditUpdateOutcome AnotherUpdateInProgress() => Without(EditUpdateStatus.AnotherUpdateInProgress, null);

        public static EditUpdateOutcome Refused(string reason) => Without(EditUpdateStatus.Refused, reason);

        public static EditUpdateOutcome ClosedDetached() => Without(EditUpdateStatus.ClosedDetached, null);

        /// <summary>
        /// The update was closed, but the draft is in Error or did not accept the close. It keeps the classification of the update, its
        /// cause, its differences and its warnings, which are copied on receipt.
        /// </summary>
        public static EditUpdateOutcome ClosedInError(
            string? reason, EditUpdateStatus classification, string? cause, IReadOnlyList<string> differences, IReadOnlyList<string> warnings)
        {
            if (differences == null) throw new ArgumentNullException(nameof(differences));
            if (warnings == null) throw new ArgumentNullException(nameof(warnings));
            return new EditUpdateOutcome(EditUpdateStatus.ClosedInError, reason, differences.ToArray(), warnings.ToArray(), classification, cause);
        }

        public static EditUpdateOutcome Failed(string reason) => Without(EditUpdateStatus.Failed, reason);

        /// <summary>The outcome of an update that was closed with one of the eight outcomes of the classification by frontier.</summary>
        public static EditUpdateOutcome Closed(
            EditUpdateStatus status, string? reason, IReadOnlyList<string> differences, IReadOnlyList<string> warnings)
        {
            if (differences == null) throw new ArgumentNullException(nameof(differences));
            if (warnings == null) throw new ArgumentNullException(nameof(warnings));
            // M-06 (f): a copy taken on receipt, so changing the list of the caller afterwards changes nothing here.
            return new EditUpdateOutcome(status, reason, differences.ToArray(), warnings.ToArray(), status);
        }

        private static EditUpdateOutcome Without(EditUpdateStatus status, string? reason) =>
            new(status, reason, Array.Empty<string>(), Array.Empty<string>());
    }

    /// <summary>How the request of an update ended, as the port answers at once (I-64 F4-T4C1-HOST, D-T4C-02). The default value is Unavailable.</summary>
    public enum EditUpdateStartStatus
    {
        /// <summary>Nothing was done: no executor of the update, no reader or a degraded session. It is also the default value.</summary>
        Unavailable,

        /// <summary>The update was accepted: it is in flight, and its outcome arrives through <see cref="IWorkspacePilotEditPort.TakeUpdateProgress"/>.</summary>
        Accepted,

        /// <summary>No active document, a modal of RackCad, no rest or a notice of the panel being delivered: nothing was done.</summary>
        Waiting,

        /// <summary>The request does not belong to the active document: nothing was done.</summary>
        Rejected,

        /// <summary>A read of the pilot or another update is in progress: nothing was done.</summary>
        Busy,

        /// <summary>The command context could not be requested, or the previous update of the same rack just expired: nothing was done.</summary>
        Failed,
    }

    /// <summary>
    /// The immediate answer to the request of an update. A value: it names no draft and no type of the pilot. Its default value is
    /// <see cref="EditUpdateStartStatus.Unavailable"/>, inert.
    /// </summary>
    public readonly struct EditUpdateStart
    {
        private EditUpdateStart(EditUpdateStartStatus status, string? reason)
        {
            Status = status;
            Reason = reason;
        }

        public EditUpdateStartStatus Status { get; }

        /// <summary>Why nothing was started; null for Accepted.</summary>
        public string? Reason { get; }

        public static EditUpdateStart Unavailable(string reason) => new(EditUpdateStartStatus.Unavailable, reason);

        public static EditUpdateStart Accepted() => new(EditUpdateStartStatus.Accepted, null);

        public static EditUpdateStart Waiting(string reason) => new(EditUpdateStartStatus.Waiting, reason);

        public static EditUpdateStart Rejected(string reason) => new(EditUpdateStartStatus.Rejected, reason);

        public static EditUpdateStart Busy(string reason) => new(EditUpdateStartStatus.Busy, reason);

        public static EditUpdateStart Failed(string reason) => new(EditUpdateStartStatus.Failed, reason);
    }

    /// <summary>
    /// What the port knows about the update of one rack, from memory: whether one is in flight and, once, its outcome. Taking it consumes the
    /// outcome. The default value is inert: not in flight and an Unavailable outcome.
    /// </summary>
    public readonly struct EditUpdateProgress
    {
        public EditUpdateProgress(bool inFlight, EditUpdateOutcome outcome)
        {
            InFlight = inFlight;
            Outcome = outcome;
        }

        /// <summary>True while the update of the rack is in flight and has not ended.</summary>
        public bool InFlight { get; }

        /// <summary>The outcome of the update; Unavailable when there is none.</summary>
        public EditUpdateOutcome Outcome { get; }
    }

    /// <summary>
    /// How the explicit discard of the drafts of a session, or of one draft, ended (I-64 F4-T2B2B, D-T2B2-15; F4-T3A, D-T3-02). The
    /// order is fixed and the default value is Rejected: it is inert, it means that nothing was done.
    /// </summary>
    public enum EditDiscardStatus
    {
        /// <summary>The request does not belong to the active document (INV-22): nothing was done. It is also the default value.</summary>
        Rejected,

        /// <summary>One draft, or every draft of the session, was removed (possibly none): nothing was read and nothing was written.</summary>
        Discarded,

        /// <summary>A failure of the host during the operation: contained, and the session is not degraded.</summary>
        Failed,

        /// <summary>A read of the pilot is in progress (edit port of the panel): nothing was done.</summary>
        Busy,
    }

    /// <summary>
    /// I-64 F4-T2B2B (D-T2B2-15) — the answer of the host to the explicit discard of one draft or of every draft of a session. A value:
    /// the racks are text, so it names no draft type. Both lists are never null, also for a default value of this struct.
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

        public static EditDiscardOutcome Busy(string reason) =>
            new(EditDiscardStatus.Busy, reason, Array.Empty<string>(), Array.Empty<string>());
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
