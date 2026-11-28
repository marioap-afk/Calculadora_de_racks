#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.PushBack.Pilot
{

    /// <summary>
    /// I-64 F4B PB-T3b (D-PBT3-09) — the explicit snapshot of a draft of the FULL model that the full PREPARE reads. It is built by ONE
    /// factory, <see cref="From"/>.
    /// The two states are copies, and <see cref="ConsideredStatus"/> is the status the draft had before an attempt that is in flight.
    /// </summary>
    public sealed class PushBackFullPrepareInput
    {
        private PushBackFullPrepareInput(
            string rackId,
            DraftStaleBase staleBase,
            PushBackFullDraftState initialDraftState,
            PushBackFullDraftState currentDraftState,
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
        public PushBackFullDraftState InitialDraftState { get; }

        /// <summary>A COPY of the current state of the draft.</summary>
        public PushBackFullDraftState CurrentDraftState { get; }

        public bool HasPendingText { get; }

        public bool IsDirty { get; }

        /// <summary>The status of the draft, and under Applying the status it had before the attempt began.</summary>
        public DraftStatus ConsideredStatus { get; }

        /// <summary>The one construction of the snapshot of a draft of the full model.</summary>
        public static PushBackFullPrepareInput From(DraftSession<PushBackFullDraftState> draft)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            var status = draft.Status;
            return new PushBackFullPrepareInput(
                draft.RackId,
                draft.StaleBase,
                draft.InitialDraftState,
                PushBackFullDraftStateComparer.Copy(draft.CurrentDraftState),
                draft.HasPendingText,
                draft.IsDirty,
                status == DraftStatus.Applying ? draft.StatusBeforeAttempt : status);
        }
    }

    /// <summary>The seven outcomes of <see cref="PushBackPilotUpdatePlanner.PrepareFull"/>, in the order in which they are evaluated.</summary>
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

    /// <summary>The result of <see cref="PushBackPilotUpdatePlanner.PrepareFull"/>. Immutable.</summary>
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
    /// What the plan redraws for one member that the update may change: the view and the section as RACKEDITAR writes them, the text
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
            : this(annotationScale, envelopes, Array.Empty<string>())
        {
        }

        internal PushBackPilotPostExpectation(double annotationScale, IEnumerable<PushBackPilotKeyedText> envelopes, IEnumerable<string> erasedKeys)
        {
            AnnotationScale = annotationScale;
            Envelopes = Array.AsReadOnly(envelopes.ToArray());
            ErasedKeys = Array.AsReadOnly(erasedKeys.ToArray());
        }

        public double AnnotationScale { get; }

        /// <summary>The outgoing envelope text of each member, ordered by key (ordinal).</summary>
        public IReadOnlyList<PushBackPilotKeyedText> Envelopes { get; }

        /// <summary>The keys of the views that the update erases, ordered by key (ordinal).</summary>
        public IReadOnlyList<string> ErasedKeys { get; }

        public bool Equals(PushBackPilotPostExpectation? other) =>
            other != null
            && AnnotationScale.Equals(other.AnnotationScale)
            && Envelopes.SequenceEqual(other.Envelopes)
            && ErasedKeys.SequenceEqual(other.ErasedKeys, StringComparer.Ordinal);

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
            : this(rackId, originKey, originKey, expectedBase, expectedVariableRegistryText, annotationScale, members, Array.Empty<string>(), postExpectation)
        {
        }

        internal PushBackPilotUpdatePlan(
            string rackId,
            string originKey,
            string postOriginKey,
            IEnumerable<DraftStaleBaseEntry> expectedBase,
            string? expectedVariableRegistryText,
            double annotationScale,
            IEnumerable<PushBackPilotMemberPlan> members,
            IEnumerable<string> erasures,
            PushBackPilotPostExpectation postExpectation)
        {
            RackId = rackId;
            OriginKey = originKey;
            PostOriginKey = postOriginKey;
            ExpectedBase = Array.AsReadOnly(expectedBase.ToArray());
            ExpectedVariableRegistryText = expectedVariableRegistryText;
            AnnotationScale = annotationScale;
            Members = Array.AsReadOnly(members.ToArray());
            Erasures = Array.AsReadOnly(erasures.ToArray());
            PostExpectation = postExpectation;
        }

        public string RackId { get; }

        /// <summary>The key of the member that is the origin of the draft.</summary>
        public string OriginKey { get; }

        /// <summary>The key of the member that POST reads again: the origin, or, when the update erases it, another one that survives.</summary>
        public string PostOriginKey { get; }

        /// <summary>The keys of the views that the update erases, ordered by key (ordinal).</summary>
        public IReadOnlyList<string> Erasures { get; }

        /// <summary>The entries of the base of the draft, ordered by key (ordinal): MUTATE compares them with its own re-read.</summary>
        public IReadOnlyList<DraftStaleBaseEntry> ExpectedBase { get; }

        /// <summary>The raw text of the variable registry of the base, when it had one.</summary>
        public string? ExpectedVariableRegistryText { get; }

        /// <summary>The new annotation scale.</summary>
        public double AnnotationScale { get; }

        /// <summary>The members that the update may change, ordered by key (ordinal).</summary>
        public IReadOnlyList<PushBackPilotMemberPlan> Members { get; }

        public PushBackPilotPostExpectation PostExpectation { get; }

        /// <summary>The outgoing envelope text of the member of that key (ordinal); null when the plan has no such member.</summary>
        public string? Envelope(string key)
        {
            foreach (var member in Members)
                if (string.Equals(member.Key, key, StringComparison.Ordinal)) return member.EnvelopeText;
            return null;
        }

        public bool Equals(PushBackPilotUpdatePlan? other) =>
            other != null
            && string.Equals(RackId, other.RackId, StringComparison.Ordinal)
            && string.Equals(OriginKey, other.OriginKey, StringComparison.Ordinal)
            && string.Equals(PostOriginKey, other.PostOriginKey, StringComparison.Ordinal)
            && Erasures.SequenceEqual(other.Erasures, StringComparer.Ordinal)
            && ExpectedBase.SequenceEqual(other.ExpectedBase)
            && string.Equals(ExpectedVariableRegistryText, other.ExpectedVariableRegistryText, StringComparison.Ordinal)
            && AnnotationScale.Equals(other.AnnotationScale)
            && Members.SequenceEqual(other.Members)
            && PostExpectation.Equals(other.PostExpectation);

        public override bool Equals(object? obj) => Equals(obj as PushBackPilotUpdatePlan);

        public override int GetHashCode() => HashCode.Combine(RackId, OriginKey, ExpectedBase.Count, AnnotationScale, Members.Count);
    }

    /// <summary>
    /// I-64 F4-T4A1 (D-T4A1-04) — the pure PREPARE of an update, born for the bounded Push Back pilot and, since I-64 F4B PB-T4d, the
    /// one of the full model only (the PREPARE of the pilot is retired). It reads a snapshot of the draft and
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

        public const string ViewAddressPrefix = "El descriptor de la vista ";

        public const string ViewAddressSuffix = " no se puede interpretar: usa RACKEDITAR.";

        public const string PrepareFailurePrefix ="No se pudo preparar la actualización: ";

        public const string BlockedPrefix = "El diseño tiene un error que impide escribirlo: ";

        public const string FrontalBOfOneWayPrefix = "La vista ";

        public const string FrontalBOfOneWaySuffix = " es un frontal del lado B en un rack sin lado B: el panel no la borra ni la redibuja; usa RACKEDITAR.";

        public const string AllViewsObsoleteReason = "Todas las vistas del rack quedarían obsoletas: no se elimina el último vínculo del rack.";

        /// <summary>
        /// The pure PREPARE of an update of the FULL model (D-PBT3-09), in the order that the PREPARE of the pilot had (D-T4A1-04; retired
        /// in I-64 F4B PB-T4d): NotApplicable,
        /// PendingText, NothingToApply, Conflict (the comparison of the base BEFORE the admission), NotAdmitted and the plan. It is total and
        /// changes nothing.
        /// </summary>
        public static PushBackPilotPrepareResult PrepareFull(PushBackFullPrepareInput input, PushBackFullIntentResult reread, RackCatalog catalog)
        {
            try
            {
                if (input == null) return PushBackPilotPrepareResult.WithInvalid(MissingInputReason);

                if (input.ConsideredStatus != DraftStatus.Clean && input.ConsideredStatus != DraftStatus.Dirty)
                    return PushBackPilotPrepareResult.Without(PushBackPilotPrepareKind.NotApplicable);
                if (input.HasPendingText) return PushBackPilotPrepareResult.Without(PushBackPilotPrepareKind.PendingText);
                if (input.ConsideredStatus == DraftStatus.Clean) return PushBackPilotPrepareResult.Without(PushBackPilotPrepareKind.NothingToApply);

                if (reread == null) return PushBackPilotPrepareResult.WithInvalid(MissingInputReason);

                var comparison = input.StaleBase.CompareWith(reread.StaleBase);
                if (!comparison.IsMatch) return PushBackPilotPrepareResult.WithConflict(comparison.Differences);

                if (!reread.Read.IsAdmitted)
                    return PushBackPilotPrepareResult.WithNotAdmitted(reread.Read.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic);

                return BuildFull(input, reread, catalog);
            }
            catch (Exception ex)
            {
                return PushBackPilotPrepareResult.WithInvalid(PrepareFailurePrefix + ex.Message);
            }
        }

        // The base of the draft and the re-read match here, and the re-read is admitted: the plan can be built, or it is Invalid. The composition
        // is the one of the model of the draft, and the plan may erase the laterals that the edition leaves obsolete.
        private static PushBackPilotPrepareResult BuildFull(PushBackFullPrepareInput input, PushBackFullIntentResult reread, RackCatalog catalog)
        {
            if (catalog == null) return PushBackPilotPrepareResult.WithInvalid(MissingInputReason);

            var composition = PushBackFullComposition.ForFullWrite(input.InitialDraftState, input.CurrentDraftState, catalog);
            return BuildCore(
                catalog,
                input.RackId,
                input.StaleBase,
                reread.StaleBase,
                input.InitialDraftState.Origin,
                input.CurrentDraftState.Editable.AnnotationScale,
                composition.Name,
                composition.Editor,
                composition.Inputs,
                composition.IsComposed ? null : composition.Reason ?? PushBackPilotMaterializer.NonPilotContentReason);
        }

        // The building of the plan: the composition (or why it was rejected), the base of the draft, the base of the re-read and the origin of the
        // draft, the scale and the name of the draft. The plan may erase the laterals that the edition leaves obsolete.
        private static PushBackPilotPrepareResult BuildCore(
            RackCatalog catalog,
            string rackId,
            DraftStaleBase staleBase,
            DraftStaleBase rereadBase,
            string origin,
            double scale,
            string? draftName,
            PushBackCompositeEditorState? editor,
            PushBackEditorInputs? inputs,
            string? rejection)
        {
            // (a) A composition that was rejected is an Invalid with its reason.
            if (rejection != null) return PushBackPilotPrepareResult.WithInvalid(rejection);

            // The members that the model may change, from the RE-READ and by key (ordinal), each with the envelope that was re-read.
            var envelopes = new RackEmbedStore();
            var members = new List<(DraftStaleBaseEntry Entry, RackEmbedDocument Embed)>();
            foreach (var entry in rereadBase.Entries.Values.Where(e => e.Membership == DraftMembership.Mutable).OrderBy(e => e.Key, StringComparer.Ordinal))
            {
                var embed = entry.EnvelopeText == null ? null : envelopes.Deserialize(entry.EnvelopeText);
                if (embed == null) return PushBackPilotPrepareResult.WithInvalid(UnreadableViewPrefix + entry.Key + ".");
                members.Add((entry, embed));
            }

            // (b) Exactly one of them is the origin: its text is, byte for byte, the origin of the draft.
            var origins = members.Where(m => string.Equals(m.Entry.EnvelopeText, origin, StringComparison.Ordinal)).ToList();
            if (origins.Count == 0) return PushBackPilotPrepareResult.WithInvalid(NoOriginReason);
            if (origins.Count > 1) return PushBackPilotPrepareResult.WithInvalid(SeveralOriginsReason);

            // (c) The inner designs of every member, as RACKEDITAR resolves them, with the one of the origin as the initiating project.
            var store = new RackProjectStore();
            if (!store.TryDeserialize(origins[0].Embed.Design, out var initiating, out _))
                return PushBackPilotPrepareResult.WithInvalid(UnreadableOriginReason);

            var preflight = store.PreflightInnerSources(members.Select(m => m.Embed.Design).ToList(), RackSystemKind.PushBack, initiating);
            if (preflight.Aborted)
            {
                return PushBackPilotPrepareResult.WithInvalid(
                    preflight.BlockingOutcome == InnerSourceOutcome.IncompatibleMajor ? InnerMajorReason : InnerKindReason);
            }

            // (d) The outgoing design and its system, built by the builders of Application, with the disposition of every view. A failure is typed
            // and nothing was written. (d2) A design that the output gate blocks is not written: it is the same criterion as the one of the window.
            PushBackDesign design;
            PushBackViewDispositionPlan dispositions;
            string? blocked = null;
            try
            {
                design = new PushBackCompositeEditorAssembler(catalog).BuildDesign(editor!, inputs!);
                var system = new PushBackResolver(catalog).Resolve(design);
                if (PushBackEditorOutputGate.IsBlocked(editor!, system)) blocked = PushBackEditorOutputGate.StatusOr(editor!, system, string.Empty);
                dispositions = PushBackViewDispositions.Of(members.Select(m => new KeyValuePair<string, RackEmbedDocument>(m.Entry.Key, m.Embed)), system, catalog);
            }
            catch (Exception ex)
            {
                return PushBackPilotPrepareResult.WithInvalid(SystemFailurePrefix + ex.Message);
            }

            if (blocked != null) return PushBackPilotPrepareResult.WithInvalid(BlockedPrefix + blocked);

            // (e) What the edition does to each view, by key (ordinal): an obsolete lateral is erased, and the frontal of side B of a rack without it
            // is rejected.
            var plans = new List<PushBackPilotMemberPlan>(members.Count);
            var erasures = new List<string>();
            for (var index = 0; index < members.Count; index++)
            {
                var (entry, embed) = members[index];
                var decoded = RackViewCodec.Decode(embed.Kind, embed.View, embed.Section);
                var disposition = dispositions.Members.First(pair => string.Equals(pair.Key, entry.Key, StringComparison.Ordinal)).Value;

                // A descriptor that does not decode has no address to ask about (M-5): it has its own text, and it is never coerced into a view.
                if (!decoded.HasAddress || disposition == PushBackViewDispositionKind.NoAddress)
                    return PushBackPilotPrepareResult.WithInvalid(ViewAddressPrefix + entry.Key + ViewAddressSuffix);
                if (disposition == PushBackViewDispositionKind.ObsoleteLateral)
                {
                    erasures.Add(entry.Key);
                    continue;
                }

                if (disposition == PushBackViewDispositionKind.FrontalBOfOneWay)
                    return PushBackPilotPrepareResult.WithInvalid(FrontalBOfOneWayPrefix + entry.Key + FrontalBOfOneWaySuffix);

                string view;
                int section;
                switch (decoded.Address.Kind)
                {
                    case DimensionViewKind.Planta:
                        view = RackEmbedDocument.ViewPlanta;
                        section = -1;
                        break;
                    case DimensionViewKind.Frontal:
                        view = RackEmbedDocument.ViewFrontal;
                        section = embed.Section;
                        break;
                    default:
                        view = RackEmbedDocument.ViewLateral;
                        section = decoded.Address.Variant.Index;
                        break;
                }

                // (f) The name: the one of the draft, or, when it is blank, the one of the envelope that was re-read for the origin, in every view.
                var memberName = string.IsNullOrWhiteSpace(draftName) ? origins[0].Embed.Name : draftName;
                var designText = store.Serialize(RackProject.ForPushBack(design).WithSourceMetadataFrom(preflight.ResolvedSources[index]));
                plans.Add(new PushBackPilotMemberPlan(entry.Key, view, section, designText, ComposeMemberEnvelope(embed, memberName, view, section, designText)));
            }

            // Every view that is left would be obsolete: the last link of the rack is never erased.
            if (dispositions.AllObsolete) return PushBackPilotPrepareResult.WithInvalid(AllViewsObsoleteReason);

            // (h) The origin of POST is the origin of the draft, or, when the update erases it, the first view that is redrawn.
            var originKey = origins[0].Entry.Key;
            var postOriginKey = erasures.Contains(originKey, StringComparer.Ordinal) ? plans[0].Key : originKey;
            var expectedBase = staleBase.Entries.Values.OrderBy(e => e.Key, StringComparer.Ordinal).ToList();
            var expectation = new PushBackPilotPostExpectation(scale, plans.Select(plan => new PushBackPilotKeyedText(plan.Key, plan.EnvelopeText)), erasures);
            return PushBackPilotPrepareResult.WithPlan(new PushBackPilotUpdatePlan(
                rackId, originKey, postOriginKey, expectedBase, staleBase.VariableRegistryText, scale, plans, erasures, expectation));
        }

        // The ONE place of this file that composes an envelope: it composes over the envelope that was re-read for that view, never
        // over one made from the draft (I-54 D-21: an existing rack composes with its real envelope).
        private static string ComposeMemberEnvelope(RackEmbedDocument source, string name, string view, int section, string designJson)
        {
            var composed = RackEmbedComposer.Compose(source, RackEmbedDocument.KindPushBack, source.Id, name, view, section, designJson);
            return new RackEmbedStore().Serialize(composed);
        }
    }
}
