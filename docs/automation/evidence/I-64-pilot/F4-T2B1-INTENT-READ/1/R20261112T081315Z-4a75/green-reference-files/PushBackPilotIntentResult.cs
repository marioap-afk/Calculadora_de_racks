#nullable enable
using System;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T2B1 (D-T2B-05, INV-27) — how the preview witness compares with the read: <see cref="NotSupplied"/> when the
    /// caller gave none (it is NOT "unchanged"), <see cref="Unchanged"/> when it is the same key and the same raw text, and
    /// <see cref="Changed"/> for anything else.
    /// </summary>
    public enum PushBackPilotPreviewWitnessStatus
    {
        NotSupplied,
        Unchanged,
        Changed,
    }

    /// <summary>
    /// I-64 F4-T2B1 (D-T2B-04) — the result of ONE read of the edit intention of the pilot. It ALWAYS carries the stale base
    /// (also when the read is not admitted, so "Check now" has something to compare), the read of the intention, the key of
    /// the origin, the technical detail of the diagnostic, the hint to the classic editor (only without admission) and the
    /// status of the preview witness. It is immutable and holds no raw text of its own.
    /// </summary>
    public sealed class PushBackPilotIntentResult
    {
        internal PushBackPilotIntentResult(
            DraftStaleBase staleBase,
            DraftIntentRead<PushBackPilotDraftState> read,
            string originKey,
            string? detail,
            string? classicEditorHint,
            PushBackPilotPreviewWitnessStatus previewWitness)
        {
            StaleBase = staleBase ?? throw new ArgumentNullException(nameof(staleBase));
            Read = read ?? throw new ArgumentNullException(nameof(read));
            OriginKey = originKey ?? string.Empty;
            Detail = detail;
            ClassicEditorHint = classicEditorHint;
            PreviewWitness = previewWitness;
        }

        /// <summary>The base of the read. Never null, and the same instance the admitted read carries.</summary>
        public DraftStaleBase StaleBase { get; }

        /// <summary>Admitted with the base and the initial state, or not admitted with the unified diagnostic.</summary>
        public DraftIntentRead<PushBackPilotDraftState> Read { get; }

        /// <summary>The key of the definition of origin of the input.</summary>
        public string OriginKey { get; }

        /// <summary>The technical detail behind the diagnostic; null when there is none. Never part of the main text.</summary>
        public string? Detail { get; }

        /// <summary>Present if and only if the read is not admitted.</summary>
        public string? ClassicEditorHint { get; }

        /// <summary>The status of the preview witness (D-T2B-05); computed also when the read is not admitted.</summary>
        public PushBackPilotPreviewWitnessStatus PreviewWitness { get; }
    }
}
