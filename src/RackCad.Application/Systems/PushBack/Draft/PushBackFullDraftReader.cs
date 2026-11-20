using System;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-04) — the result of ONE read of the edit intention of the FULL model: the stale base, the
    /// read, the key of the origin, the technical detail and the hint to the classic editor.
    /// </summary>
    public sealed class PushBackFullIntentResult
    {
        internal PushBackFullIntentResult(
            DraftStaleBase staleBase,
            DraftIntentRead<PushBackFullDraftState> read,
            string originKey,
            string detail,
            string classicEditorHint)
        {
            StaleBase = staleBase ?? throw new ArgumentNullException(nameof(staleBase));
            Read = read ?? throw new ArgumentNullException(nameof(read));
            OriginKey = originKey ?? string.Empty;
            Detail = detail;
            ClassicEditorHint = classicEditorHint;
        }

        /// <summary>The base of the read. Never null.</summary>
        public DraftStaleBase StaleBase { get; }

        /// <summary>Admitted with the base and the initial state, or not admitted with the unified diagnostic.</summary>
        public DraftIntentRead<PushBackFullDraftState> Read { get; }

        /// <summary>The key of the definition of origin of the input.</summary>
        public string OriginKey { get; }

        /// <summary>The technical detail behind the diagnostic; null when there is none.</summary>
        public string Detail { get; }

        /// <summary>Present if and only if the read is not admitted.</summary>
        public string ClassicEditorHint { get; }
    }

    /// <summary>I-64 F4B PB-T3a (D-PBT3-04) — the reader of the FULL model, on top of the reader of the pilot.</summary>
    public static class PushBackFullDraftReader
    {
        public static PushBackFullIntentResult Read(PushBackPilotIntentScan scan, RackCatalog catalog)
            => new PushBackFullIntentResult(
                PushBackPilotIntentReader.BuildStaleBase(scan),
                DraftIntentRead<PushBackFullDraftState>.NotAdmitted(PushBackPilotIntentReader.MaterializeDiagnostic),
                scan.OriginKey,
                null,
                PushBackPilotIntentReader.ClassicEditorHint);
    }
}
