#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T4A1 (D-T4A1-04, N-7) — the explicit snapshot of a draft that <see cref="PushBackPilotUpdatePlanner.Prepare"/> reads. It
    /// is built by ONE factory, <see cref="From"/>, so no other code names the base or the states of a draft to prepare an update.
    /// The two states are copies, and <see cref="ConsideredStatus"/> is the status the draft had before an attempt that is in flight.
    /// </summary>
    public sealed class PushBackPilotPrepareInput
    {
        private PushBackPilotPrepareInput(
            string rackId,
            DraftStaleBase staleBase,
            PushBackPilotDraftState initialDraftState,
            PushBackPilotDraftState currentDraftState,
            bool hasPendingText,
            bool isDirty,
            DraftStatus consideredStatus)
        {
            RackId = rackId;
            StaleBase = staleBase;
            InitialDraftState = initialDraftState;
            CurrentDraftState = currentDraftState;
            HasPendingText = hasPendingText;
            IsDirty = isDirty;
            ConsideredStatus = consideredStatus;
        }

        public string RackId { get; }

        /// <summary>The base of the draft: what the last read of the drawing saw.</summary>
        public DraftStaleBase StaleBase { get; }

        /// <summary>A COPY of the initial state of the draft.</summary>
        public PushBackPilotDraftState InitialDraftState { get; }

        /// <summary>A COPY of the current state of the draft.</summary>
        public PushBackPilotDraftState CurrentDraftState { get; }

        public bool HasPendingText { get; }

        public bool IsDirty { get; }

        /// <summary>The status of the draft, and under Applying the status it had before the attempt began.</summary>
        public DraftStatus ConsideredStatus { get; }

        /// <summary>The one construction of the snapshot of a draft.</summary>
        public static PushBackPilotPrepareInput From(DraftSession<PushBackPilotDraftState> draft)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            var status = draft.Status;
            return new PushBackPilotPrepareInput(
                draft.RackId,
                draft.StaleBase,
                draft.InitialDraftState,
                PushBackPilotStateComparer.Copy(draft.CurrentDraftState),
                draft.HasPendingText,
                draft.IsDirty,
                status == DraftStatus.Applying ? draft.StatusBeforeAttempt : status);
        }
    }

    /// <summary>The seven outcomes of <see cref="PushBackPilotUpdatePlanner.Prepare"/>, in the order in which they are evaluated.</summary>
    public enum PushBackPilotPrepareKind
    {
        /// <summary>The draft is not Clean or Dirty: there is nothing to prepare.</summary>
        NotApplicable,

        /// <summary>The draft has pending text: the boundary of the update must be crossed first.</summary>
        PendingText,

        /// <summary>The draft is Clean: there is nothing to apply. The re-read is not looked at.</summary>
        NothingToApply,

        /// <summary>The re-read differs from the base of the draft (raw comparison, before the admission).</summary>
        Conflict,

        /// <summary>The base matches but the re-read is not admitted.</summary>
        NotAdmitted,

        /// <summary>The update cannot be prepared: the reason says why.</summary>
        Invalid,

        /// <summary>The update is prepared: the plan is set.</summary>
        Ready,
    }

    /// <summary>The result of <see cref="PushBackPilotUpdatePlanner.Prepare"/>. Immutable.</summary>
    public sealed class PushBackPilotPrepareResult
    {
        private PushBackPilotPrepareResult(
            PushBackPilotPrepareKind kind, string? reason, IReadOnlyList<DraftDifference> differences, PushBackPilotUpdatePlan? plan)
        {
            Kind = kind;
            Reason = reason;
            Differences = differences;
            Plan = plan;
        }

        public PushBackPilotPrepareKind Kind { get; }

        /// <summary>The diagnostic of NotAdmitted or the reason of Invalid; null otherwise.</summary>
        public string? Reason { get; }

        /// <summary>What differs, ordered, for Conflict; empty otherwise.</summary>
        public IReadOnlyList<DraftDifference> Differences { get; }

        /// <summary>The plan, for Ready; null otherwise.</summary>
        public PushBackPilotUpdatePlan? Plan { get; }

        internal static PushBackPilotPrepareResult Without(PushBackPilotPrepareKind kind) =>
            new(kind, null, Array.Empty<DraftDifference>(), null);

        internal static PushBackPilotPrepareResult WithConflict(IReadOnlyList<DraftDifference> differences) =>
            new(PushBackPilotPrepareKind.Conflict, null, Array.AsReadOnly(differences.ToArray()), null);

        internal static PushBackPilotPrepareResult WithNotAdmitted(string diagnostic) =>
            new(PushBackPilotPrepareKind.NotAdmitted, diagnostic, Array.Empty<DraftDifference>(), null);

        internal static PushBackPilotPrepareResult WithInvalid(string reason) =>
            new(PushBackPilotPrepareKind.Invalid, reason, Array.Empty<DraftDifference>(), null);

        internal static PushBackPilotPrepareResult WithPlan(PushBackPilotUpdatePlan plan) =>
            new(PushBackPilotPrepareKind.Ready, null, Array.Empty<DraftDifference>(), plan);
    }

    /// <summary>A key with a serialized text. Strings only: ordinal equality.</summary>
    public readonly record struct PushBackPilotKeyedText(string Key, string Text);

    /// <summary>
    /// What the plan redraws for one member that the pilot may change: the view and the section as RACKEDITAR writes them, the text
    /// of the outgoing design of that view and the text of its outgoing envelope. Strings and one number, immutable.
    /// </summary>
    public sealed class PushBackPilotMemberPlan : IEquatable<PushBackPilotMemberPlan>
    {
        internal PushBackPilotMemberPlan(string key, string view, int section, string designText, string envelopeText)
        {
            Key = key;
            View = view;
            Section = section;
            DesignText = designText;
            EnvelopeText = envelopeText;
        }

        public string Key { get; }

        public string View { get; }

        public int Section { get; }

        /// <summary>The serialized outgoing design of the view, with the metadata of the inner project of that view.</summary>
        public string DesignText { get; }

        /// <summary>The serialized outgoing envelope, composed over the envelope that was RE-READ for that view.</summary>
        public string EnvelopeText { get; }

        public bool Equals(PushBackPilotMemberPlan? other) =>
            other != null
            && string.Equals(Key, other.Key, StringComparison.Ordinal)
            && string.Equals(View, other.View, StringComparison.Ordinal)
            && Section == other.Section
            && string.Equals(DesignText, other.DesignText, StringComparison.Ordinal)
            && string.Equals(EnvelopeText, other.EnvelopeText, StringComparison.Ordinal);

        public override bool Equals(object? obj) => Equals(obj as PushBackPilotMemberPlan);

        public override int GetHashCode() => HashCode.Combine(Key, View, Section, DesignText, EnvelopeText);
    }

    /// <summary>What the re-read of POST must show before the draft can be Clean again: the scale and the outgoing envelopes by key.</summary>
    public sealed class PushBackPilotPostExpectation : IEquatable<PushBackPilotPostExpectation>
    {
        internal PushBackPilotPostExpectation(double annotationScale, IEnumerable<PushBackPilotKeyedText> envelopes)
        {
            AnnotationScale = annotationScale;
            Envelopes = Array.AsReadOnly(envelopes.ToArray());
        }

        public double AnnotationScale { get; }

        /// <summary>The outgoing envelope text of each member, ordered by key (ordinal).</summary>
        public IReadOnlyList<PushBackPilotKeyedText> Envelopes { get; }

        public bool Equals(PushBackPilotPostExpectation? other) =>
            other != null && AnnotationScale.Equals(other.AnnotationScale) && Envelopes.SequenceEqual(other.Envelopes);

        public override bool Equals(object? obj) => Equals(obj as PushBackPilotPostExpectation);

        public override int GetHashCode() => HashCode.Combine(AnnotationScale, Envelopes.Count);
    }

    /// <summary>
    /// I-64 F4-T4A1 (D-T4A1-04) — what an update would write, prepared without touching the drawing. It is immutable and keeps only
    /// strings and values: the rack, the key of the origin, the base that the write expects to find again, the new scale, the
    /// members to redraw in order and what the re-read of POST must show. Equality is explicit: strings are compared ordinally,
    /// the collections element by element and in order, and the scale with <see cref="double.Equals(double)"/>.
    /// </summary>
    public sealed class PushBackPilotUpdatePlan : IEquatable<PushBackPilotUpdatePlan>
    {
        internal PushBackPilotUpdatePlan(
            string rackId,
            string originKey,
            IEnumerable<DraftStaleBaseEntry> expectedBase,
            string? expectedVariableRegistryText,
            double annotationScale,
            IEnumerable<PushBackPilotMemberPlan> members,
            PushBackPilotPostExpectation postExpectation)
        {
            RackId = rackId;
            OriginKey = originKey;
            ExpectedBase = Array.AsReadOnly(expectedBase.ToArray());
            ExpectedVariableRegistryText = expectedVariableRegistryText;
            AnnotationScale = annotationScale;
            Members = Array.AsReadOnly(members.ToArray());
            PostExpectation = postExpectation;
        }

        public string RackId { get; }

        /// <summary>The key of the member that is the origin of the draft.</summary>
        public string OriginKey { get; }

        /// <summary>The entries of the base of the draft, ordered by key (ordinal): MUTATE compares them with its own re-read.</summary>
        public IReadOnlyList<DraftStaleBaseEntry> ExpectedBase { get; }

        /// <summary>The raw text of the variable registry of the base, when it had one.</summary>
        public string? ExpectedVariableRegistryText { get; }

        /// <summary>The new annotation scale.</summary>
        public double AnnotationScale { get; }

        /// <summary>The members that the pilot may change, ordered by key (ordinal).</summary>
        public IReadOnlyList<PushBackPilotMemberPlan> Members { get; }

        public PushBackPilotPostExpectation PostExpectation { get; }

        public bool Equals(PushBackPilotUpdatePlan? other) =>
            other != null
            && string.Equals(RackId, other.RackId, StringComparison.Ordinal)
            && string.Equals(OriginKey, other.OriginKey, StringComparison.Ordinal)
            && ExpectedBase.SequenceEqual(other.ExpectedBase)
            && string.Equals(ExpectedVariableRegistryText, other.ExpectedVariableRegistryText, StringComparison.Ordinal)
            && AnnotationScale.Equals(other.AnnotationScale)
            && Members.SequenceEqual(other.Members)
            && PostExpectation.Equals(other.PostExpectation);

        public override bool Equals(object? obj) => Equals(obj as PushBackPilotUpdatePlan);

        public override int GetHashCode() => HashCode.Combine(RackId, OriginKey, ExpectedBase.Count, AnnotationScale, Members.Count);
    }

    /// <summary>
    /// I-64 F4-T4A1 (D-T4A1-04) — the pure PREPARE of an update of the bounded Push Back pilot. It reads a snapshot of the draft and
    /// ONE re-read of the intention and gives the plan of what would be written, or why nothing can be. It never changes the draft,
    /// never reads the drawing and never writes it.
    /// </summary>
    public static class PushBackPilotUpdatePlanner
    {
        public const string MissingInputReason = "Falta el borrador, la relectura o el catálogo para preparar la actualización.";

        public const string NoOriginReason = "Ningún sobre editable de la relectura coincide con el origen del borrador.";

        public const string SeveralOriginsReason = "Varios sobres editables de la relectura coinciden con el origen del borrador.";

        public const string UnreadableOriginReason = "El diseño interior del sobre de origen no se puede leer.";

        public const string InnerMajorReason = "Una vista de este rack tiene un diseño interior de una versión más nueva de RackCad; no se prepara la actualización.";

        public const string InnerKindReason = "Una vista de este rack tiene un diseño interior de otro tipo; no se prepara la actualización.";

        public const string SystemFailurePrefix = "No se pudo construir el sistema del diseño saliente: ";

        public const string UnreadableViewPrefix = "No se pudo leer el sobre de la vista ";

        public const string ViewUnavailablePrefix = "El piloto no borra vistas y la vista ";

        public const string ViewUnavailableSuffix = " ya no está disponible en el sistema saliente: usa RACKEDITAR.";

        public const string PrepareFailurePrefix = "No se pudo preparar la actualización: ";

        /// <summary>
        /// The pure PREPARE of an update. It evaluates, in this order and returning at the first that holds: NotApplicable (the draft is
        /// not Clean or Dirty), PendingText, NothingToApply (Clean; the re-read is not looked at), Conflict (the raw comparison of the
        /// base with the re-read, BEFORE the admission), NotAdmitted (only with a match), Invalid (a to e) and Ready. It is total: any
        /// exception is an Invalid with its reason. It changes nothing, reads nothing and writes nothing.
        /// </summary>
        public static PushBackPilotPrepareResult Prepare(PushBackPilotPrepareInput input, PushBackPilotIntentResult reread, RackCatalog catalog)
        {
            // RED skeleton (INV-F4T4A1-13): inert, it never throws and it always answers NotApplicable.
            return PushBackPilotPrepareResult.Without(PushBackPilotPrepareKind.NotApplicable);
        }

        // The ONE place of this file that composes an envelope: it composes over the envelope that was re-read for that view, never
        // over one made from the draft (I-54 D-21: an existing rack composes with its real envelope).
        private static string ComposeMemberEnvelope(RackEmbedDocument source, string view, int section, string designJson)
        {
            var composed = RackEmbedComposer.Compose(source, RackEmbedDocument.KindPushBack, source.Id, source.Name, view, section, designJson);
            return new RackEmbedStore().Serialize(composed);
        }
    }
}
