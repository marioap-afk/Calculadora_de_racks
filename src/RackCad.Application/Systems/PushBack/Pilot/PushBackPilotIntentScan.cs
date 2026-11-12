#nullable enable
using System.Collections.Generic;
using RackCad.Application.Views.Redraw;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>I-64 F4-T2B1 (D-T2B-05) — skeleton: the key and the raw envelope text that a preview showed.</summary>
    public sealed record PushBackPilotPreviewWitness(string Key, string? RawEnvelopeText);

    /// <summary>I-64 F4-T2B1 (D-T2B-01) — inert skeleton of the input of the read of the edit intention: it keeps what it is given and checks nothing.</summary>
    public sealed class PushBackPilotIntentScan
    {
        public PushBackPilotIntentScan(
            string? rackId,
            string? originKey,
            IReadOnlyList<RackSiblingScanFact> facts,
            IReadOnlyDictionary<string, string?> rawEnvelopes,
            string completenessEvidence,
            PushBackPilotPreviewWitness? previewWitness = null)
        {
            RackId = rackId ?? string.Empty;
            OriginKey = originKey ?? string.Empty;
            Facts = facts;
            RawEnvelopes = rawEnvelopes!;
            CompletenessEvidence = completenessEvidence;
            PreviewWitness = previewWitness;
        }

        public string RackId { get; }

        public string OriginKey { get; }

        public IReadOnlyList<RackSiblingScanFact> Facts { get; }

        public IReadOnlyDictionary<string, string> RawEnvelopes { get; }

        public string CompletenessEvidence { get; }

        public PushBackPilotPreviewWitness? PreviewWitness { get; }
    }
}
