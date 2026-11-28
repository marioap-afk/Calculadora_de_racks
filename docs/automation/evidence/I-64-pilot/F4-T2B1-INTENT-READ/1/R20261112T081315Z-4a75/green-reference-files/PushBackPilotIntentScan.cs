#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.Views.Redraw;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T2B1 (D-T2B-05) — what the preview of a navigation showed: the definition key and the raw envelope text read
    /// when it was shown. The text is null when the definition could not be read. It is only evidence for the comparison
    /// of <see cref="PushBackPilotIntentReader.Read"/>; it never reaches a draft.
    /// </summary>
    public sealed record PushBackPilotPreviewWitness(string Key, string? RawEnvelopeText);

    /// <summary>
    /// I-64 F4-T2B1 (D-T2B-01) — the pure input of the read of the edit intention of the bounded Push Back pilot: the facts of
    /// the drawing scan WITHOUT classifying, and the exact raw text of the envelope of each definition by key. The
    /// constructor copies what it receives, so later changes of the caller's collections cannot change the input.
    /// Structural violations (what the scan guarantees by construction) throw <see cref="ArgumentException"/>; no state of
    /// the drawing does: a blank rack identity, an empty envelope identity, an unreadable envelope or a text with invalid
    /// UTF-16 are all valid inputs.
    /// </summary>
    public sealed class PushBackPilotIntentScan
    {
        private readonly IReadOnlyList<RackSiblingScanFact> _facts;
        private readonly Dictionary<string, string> _rawEnvelopes;

        public PushBackPilotIntentScan(
            string? rackId,
            string? originKey,
            IReadOnlyList<RackSiblingScanFact> facts,
            IReadOnlyDictionary<string, string?> rawEnvelopes,
            string completenessEvidence,
            PushBackPilotPreviewWitness? previewWitness = null)
        {
            if (facts == null) throw new ArgumentNullException(nameof(facts));
            if (rawEnvelopes == null) throw new ArgumentNullException(nameof(rawEnvelopes));

            var copiedFacts = new List<RackSiblingScanFact>(facts.Count);
            var factKeys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var fact in facts)
            {
                if (fact == null) throw new ArgumentException("A scan fact cannot be null.", nameof(facts));
                if (string.IsNullOrWhiteSpace(fact.DefinitionKey))
                    throw new ArgumentException("Every scan fact needs a definition key.", nameof(facts));
                if (!factKeys.Add(fact.DefinitionKey))
                    throw new ArgumentException("A definition cannot occur twice in one scan: " + fact.DefinitionKey, nameof(facts));
                if (fact.LayoutReferenceCount < 0 || fact.NestedReferenceCount < 0)
                    throw new ArgumentException("Reference counts cannot be negative.", nameof(facts));
                if (fact.IsSelectedSource)
                    throw new ArgumentException("The scan of the edit intention selects no source.", nameof(facts));
                copiedFacts.Add(fact);
            }

            var copiedText = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var pair in rawEnvelopes)
            {
                if (string.IsNullOrWhiteSpace(pair.Key))
                    throw new ArgumentException("Every raw envelope needs a definition key.", nameof(rawEnvelopes));
                if (copiedText.ContainsKey(pair.Key))
                    throw new ArgumentException("A raw envelope key cannot occur twice: " + pair.Key, nameof(rawEnvelopes));

                // A null text is the same as an absent key.
                if (pair.Value != null) copiedText[pair.Key] = pair.Value;
            }

            if (string.IsNullOrWhiteSpace(completenessEvidence))
                throw new ArgumentException("The completeness evidence is required.", nameof(completenessEvidence));
            if (previewWitness != null && string.IsNullOrWhiteSpace(previewWitness.Key))
                throw new ArgumentException("The key of the preview witness is required.", nameof(previewWitness));

            RackId = rackId ?? string.Empty;
            OriginKey = originKey ?? string.Empty;
            _facts = copiedFacts.AsReadOnly();
            _rawEnvelopes = copiedText;
            CompletenessEvidence = completenessEvidence;
            PreviewWitness = previewWitness;
        }

        /// <summary>The RackId of the tab. It may be blank: that is a state of the drawing, not an error.</summary>
        public string RackId { get; }

        /// <summary>The key of the definition of origin, in the format of the key of the scan; chosen by the host.</summary>
        public string OriginKey { get; }

        /// <summary>The facts as the scan gives them: unclassified.</summary>
        public IReadOnlyList<RackSiblingScanFact> Facts => _facts;

        /// <summary>The exact raw text of each definition by key (ordinal). A key without text is simply absent.</summary>
        public IReadOnlyDictionary<string, string> RawEnvelopes => _rawEnvelopes;

        /// <summary>The evidence of completeness that is handed to the authored comparator.</summary>
        public string CompletenessEvidence { get; }

        /// <summary>What the preview showed, when the caller supplies it; otherwise null.</summary>
        public PushBackPilotPreviewWitness? PreviewWitness { get; }
    }
}
