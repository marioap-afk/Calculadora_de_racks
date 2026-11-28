#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Redraw;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T2B1 (D-T2B-02, D-T2B-03, D-T2B-05..07) — the pure read of the edit intention of the bounded Push Back pilot.
    /// From ONE input (the unclassified facts of the scan and the raw text of each envelope) it gives, at the same time, the
    /// stale base, the admission (ordered, with unified diagnostics) and the initial state of the draft. It reads no
    /// drawing, writes nothing and never throws for a state of the drawing: only a structural error of the input (which the
    /// input itself rejects) or a null argument is a programming error.
    /// </summary>
    public static class PushBackPilotIntentReader
    {
        private const string NotEditable = "No editable aquí: ";

        /// <summary>Rules 1 and 4 (INV-16): the envelope has no identity.</summary>
        public const string NoIdentityDiagnostic = NotEditable + "el sobre no tiene identidad";

        /// <summary>Rule 2: the origin is not in the scan any more.</summary>
        public const string OriginMissingDiagnostic = NotEditable + "el origen ya no está en el dibujo";

        /// <summary>
        /// Rules 3, 7, 11 and 13 (INV-17): the ONLY text of what is unreadable or written by a future version. The text of
        /// the materializer, <see cref="PushBackPilotMaterializer.UnreadableOrFutureDiagnostic"/>, stays as its internal reason.
        /// </summary>
        public const string UnreadableDiagnostic = NotEditable + "ilegible o de versión futura";

        /// <summary>Rule 5: the origin belongs to another rack.</summary>
        public const string OriginNotMemberDiagnostic = NotEditable + "el origen pertenece a otro rack";

        /// <summary>Rule 6: the origin comes from an external reference.</summary>
        public const string OriginReadOnlyDiagnostic = NotEditable + "el origen es una referencia externa";

        /// <summary>Rule 8 (INV-17): the editable members of the rack are of different systems.</summary>
        public const string KindDivergentDiagnostic = NotEditable + "los miembros del rack son de sistemas distintos";

        /// <summary>Rules 9 and 12: a readable rack that the pilot does not serve (text of IC-07).</summary>
        public const string OutOfPilotDiagnostic = NotEditable + "fuera del piloto (F5)";

        /// <summary>Rule 10: the definitions of the rack do not agree on the authored value.</summary>
        public const string DivergentDiagnostic = NotEditable + "las definiciones del rack no coinciden";

        /// <summary>Rule 14 and every unexpected exception: the draft could not be prepared.</summary>
        public const string MaterializeDiagnostic = NotEditable + "no se pudo preparar el borrador";

        /// <summary>Carried by every result without admission; no file launches the command (C-F4-07).</summary>
        public const string ClassicEditorHint = "Usa RACKEDITAR para editarlo con el editor clásico.";

        /// <summary>
        /// The stale base of the input (D-T2B-02). Total and independent of the admission and of the order of the facts: it is
        /// empty for a blank RackId and otherwise holds ONE entry per member (Redraw and Erase as Mutable, ReadOnly as
        /// ReadOnly, an unreadable member as Blocking; what is not a member does not appear). It keeps only the text of the
        /// members, and the variable registry is not read.
        /// </summary>
        public static DraftStaleBase BuildStaleBase(PushBackPilotIntentScan scan)
        {
            if (scan == null) throw new ArgumentNullException(nameof(scan));
            return Analyze(scan).StaleBase;
        }

        /// <summary>
        /// ONE read: builds the base, evaluates the 14 rules of the admission in order (the first that fails decides) and,
        /// when all pass, materializes the initial state from the SAME text that stays in the entry of the origin.
        /// The result always carries the base. It never throws for a state of the drawing and never admits after an exception.
        /// </summary>
        public static PushBackPilotIntentResult Read(PushBackPilotIntentScan scan, RackCatalog? catalog)
        {
            if (scan == null) throw new ArgumentNullException(nameof(scan));

            var analysis = Analyze(scan);
            Verdict verdict;
            try
            {
                verdict = Evaluate(scan, analysis, catalog);
            }
            catch (Exception ex)
            {
                verdict = Verdict.Refused(MaterializeDiagnostic, ex.Message);
            }

            var witness = WitnessStatus(scan);
            if (verdict.State != null)
            {
                return new PushBackPilotIntentResult(
                    analysis.StaleBase,
                    DraftIntentRead<PushBackPilotDraftState>.Admitted(analysis.StaleBase, verdict.State),
                    scan.OriginKey,
                    null,
                    null,
                    witness);
            }

            return new PushBackPilotIntentResult(
                analysis.StaleBase,
                DraftIntentRead<PushBackPilotDraftState>.NotAdmitted(verdict.Diagnostic ?? MaterializeDiagnostic),
                scan.OriginKey,
                verdict.Detail,
                ClassicEditorHint,
                witness);
        }

        // ------------------------------------------------------------------ the one classification and the base

        private static Analysis Analyze(PushBackPilotIntentScan scan)
        {
            // A blank RackId classifies nothing: the base is empty and the classification is never asked.
            if (string.IsNullOrWhiteSpace(scan.RackId))
                return new Analysis(Array.Empty<Member>(), DraftStaleBase.Create(Array.Empty<DraftStaleBaseEntry>()));

            var snapshot = RackSiblingMembership.Classify(scan.Facts, scan.RackId, null, false);
            var members = new List<Member>(snapshot.Members.Count);
            var entries = new List<DraftStaleBaseEntry>(snapshot.Members.Count);
            foreach (var classified in snapshot.Members)
            {
                var key = classified.Fact.DefinitionKey;
                scan.RawEnvelopes.TryGetValue(key, out var text);
                var member = new Member(classified.Fact, classified.Kind, text);
                members.Add(member);

                switch (classified.Kind)
                {
                    case RackSiblingMembershipKind.Redraw:
                    case RackSiblingMembershipKind.Erase:
                        entries.Add(Describe(member, DraftMembership.Mutable));
                        break;
                    case RackSiblingMembershipKind.ReadOnly:
                        entries.Add(Describe(member, DraftMembership.ReadOnly));
                        break;
                    case RackSiblingMembershipKind.BlockingUnreadable:
                        entries.Add(new DraftStaleBaseEntry(key, DraftMembership.Blocking, null, null, null));
                        break;
                    // NotMember: it does not appear in the base.
                }
            }

            return new Analysis(members, DraftStaleBase.Create(entries));
        }

        private static DraftStaleBaseEntry Describe(Member member, DraftMembership membership)
        {
            var embed = member.Embed;
            var section = embed == null ? null : embed.Section.ToString(CultureInfo.InvariantCulture);
            return new DraftStaleBaseEntry(member.Fact.DefinitionKey, membership, member.Fact.View, section, member.Text);
        }

        // ------------------------------------------------------------------ the admission (D-T2B-03)

        private static Verdict Evaluate(PushBackPilotIntentScan scan, Analysis analysis, RackCatalog? catalog)
        {
            // 1
            if (string.IsNullOrWhiteSpace(scan.RackId)) return Verdict.Refused(NoIdentityDiagnostic);

            // 2
            var origin = string.IsNullOrWhiteSpace(scan.OriginKey)
                ? null
                : analysis.Members.FirstOrDefault(m => string.Equals(m.Fact.DefinitionKey, scan.OriginKey, StringComparison.Ordinal));
            if (origin == null) return Verdict.Refused(OriginMissingDiagnostic);

            // 3 (reads the fact; the deserialized envelope only proves that the text is there and can be read)
            if (!origin.Fact.EnvelopeInterpretable || origin.Embed == null) return Verdict.Refused(UnreadableDiagnostic);

            // 4
            if (string.IsNullOrWhiteSpace(origin.Fact.RackId)) return Verdict.Refused(NoIdentityDiagnostic);

            // 5: the origin belongs to another rack (its Id is not the RackId) or it is not classified as a member (NotMember or
            // Blocking). An external reference (ReadOnly) with the right identity is NOT this rule: it goes on to rule 6.
            // Defensive: with the semantics of Classify the second condition cannot hold here (an interpretable fact with the
            // RackId, ignoring case, is Redraw, Erase or ReadOnly, and an uninterpretable one was refused by rule 3); it is kept
            // so that a change of Classify can never let a non-member origin through.
            if (!string.Equals(origin.Fact.RackId, scan.RackId, StringComparison.OrdinalIgnoreCase) ||
                !(IsMutable(origin.Kind) || origin.Kind == RackSiblingMembershipKind.ReadOnly))
                return Verdict.Refused(OriginNotMemberDiagnostic);

            // 6
            if (origin.Fact.IsXrefDependent) return Verdict.Refused(OriginReadOnlyDiagnostic);
            // Defensive: after rule 5 the origin is Redraw, Erase or ReadOnly, and ReadOnly is exactly the external reference
            // that the line above refused, so this can never hold today; it keeps the origin from ever being edited unless it is
            // Redraw or Erase.
            if (!IsMutable(origin.Kind)) return Verdict.Refused(OriginNotMemberDiagnostic);

            // 7
            var mutable = analysis.Members.Where(m => IsMutable(m.Kind)).ToList();
            if (analysis.Members.Any(m => m.Kind == RackSiblingMembershipKind.BlockingUnreadable) ||
                mutable.Any(m => m.Embed == null))
                return Verdict.Refused(UnreadableDiagnostic);

            // 8
            if (mutable.Select(m => m.Embed?.Kind ?? string.Empty).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
                return Verdict.Refused(KindDivergentDiagnostic);

            // 9 (a null, empty or blank kind has no detail to give: the detail stays null)
            var originKind = origin.Embed.Kind;
            if (!string.Equals(originKind, RackEmbedDocument.KindPushBack, StringComparison.Ordinal))
                return Verdict.Refused(OutOfPilotDiagnostic, string.IsNullOrWhiteSpace(originKind) ? null : originKind);

            // 10 and 11: AUTH-13 over the editable members by key, with the real raw text. A member whose envelope or text is
            // missing (rule 7 already refused it) would simply not be handed over, and the sibling count below says so.
            var siblings = new List<RackAuthoredSibling>(mutable.Count);
            foreach (var m in mutable.OrderBy(m => m.Fact.DefinitionKey, StringComparer.Ordinal))
            {
                var embed = m.Embed;
                if (embed == null || m.Text == null) continue;
                siblings.Add(new RackAuthoredSibling(m.Fact.DefinitionKey, embed.Kind ?? string.Empty, m.Text, embed.Design));
            }

            var isComplete = !analysis.Members.Any(m => m.Kind == RackSiblingMembershipKind.BlockingUnreadable) &&
                             siblings.Count == mutable.Count;
            var comparison = RackAuthoredComparatorPorts.PushBack()
                .Compare(new RackAuthoredInput(scan.RackId, siblings, isComplete, scan.CompletenessEvidence));
            if (comparison.Outcome == RackAuthoredComparisonOutcome.Divergent)
                return Verdict.Refused(DivergentDiagnostic, comparison.Diagnostic);
            if (comparison.Outcome != RackAuthoredComparisonOutcome.Single)
                return Verdict.Refused(UnreadableDiagnostic, comparison.Diagnostic);

            // 12, 13, 14, and the admission: the text handed over is the one of the entry of the origin of the base.
            var materialized = PushBackPilotMaterializer.Materialize(scan.RawEnvelopes[scan.OriginKey], catalog!);
            if (materialized.IsAdmitted && materialized.State != null) return Verdict.Admit(materialized.State);

            if (materialized.Kind == PushBackPilotMaterializationKind.OutOfPilot)
                return Verdict.Refused(OutOfPilotDiagnostic, materialized.Reason);
            if (string.Equals(materialized.Reason, PushBackPilotMaterializer.UnreadableOrFutureDiagnostic, StringComparison.Ordinal))
                return Verdict.Refused(UnreadableDiagnostic);
            return Verdict.Refused(MaterializeDiagnostic, materialized.Reason);
        }

        private static bool IsMutable(RackSiblingMembershipKind kind) =>
            kind == RackSiblingMembershipKind.Redraw || kind == RackSiblingMembershipKind.Erase;

        // ------------------------------------------------------------------ the preview witness (D-T2B-05)

        private static PushBackPilotPreviewWitnessStatus WitnessStatus(PushBackPilotIntentScan scan)
        {
            var witness = scan.PreviewWitness;
            if (witness == null) return PushBackPilotPreviewWitnessStatus.NotSupplied;

            // Unchanged needs the origin to be in the scan: without its fact, even an equal text is only a leftover (Changed).
            return string.Equals(witness.Key, scan.OriginKey, StringComparison.Ordinal) &&
                   scan.Facts.Any(f => string.Equals(f.DefinitionKey, scan.OriginKey, StringComparison.Ordinal)) &&
                   witness.RawEnvelopeText != null &&
                   scan.RawEnvelopes.TryGetValue(scan.OriginKey, out var current) &&
                   string.Equals(witness.RawEnvelopeText, current, StringComparison.Ordinal)
                ? PushBackPilotPreviewWitnessStatus.Unchanged
                : PushBackPilotPreviewWitnessStatus.Changed;
        }

        // ------------------------------------------------------------------ private shapes

        private sealed class Analysis
        {
            public Analysis(IReadOnlyList<Member> members, DraftStaleBase staleBase)
            {
                Members = members;
                StaleBase = staleBase;
            }

            public IReadOnlyList<Member> Members { get; }

            public DraftStaleBase StaleBase { get; }
        }

        /// <summary>One classified fact with its raw text, and the envelope deserialized lazily from THAT text (never re-serialized).</summary>
        private sealed class Member
        {
            private bool _read;
            private RackEmbedDocument? _embed;

            public Member(RackSiblingScanFact fact, RackSiblingMembershipKind kind, string? text)
            {
                Fact = fact;
                Kind = kind;
                Text = text;
            }

            public RackSiblingScanFact Fact { get; }

            public RackSiblingMembershipKind Kind { get; }

            public string? Text { get; }

            /// <summary>The envelope of the raw text; null when the text is absent or does not deserialize (any exception counts).</summary>
            public RackEmbedDocument? Embed
            {
                get
                {
                    if (_read) return _embed;
                    _read = true;
                    if (Text == null) return _embed = null;
                    try
                    {
                        _embed = new RackEmbedStore().Deserialize(Text);
                    }
                    catch (Exception)
                    {
                        _embed = null;
                    }

                    return _embed;
                }
            }
        }

        private readonly struct Verdict
        {
            private Verdict(PushBackPilotDraftState? state, string? diagnostic, string? detail)
            {
                State = state;
                Diagnostic = diagnostic;
                Detail = detail;
            }

            public PushBackPilotDraftState? State { get; }

            public string? Diagnostic { get; }

            public string? Detail { get; }

            public static Verdict Admit(PushBackPilotDraftState state) => new(state, null, null);

            public static Verdict Refused(string diagnostic, string? detail = null) => new(null, diagnostic, detail);
        }
    }
}
