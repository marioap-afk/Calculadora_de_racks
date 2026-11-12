#nullable enable
using System;
using RackCad.Application.Catalogs;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T2B1 (D-T2B-02, D-T2B-03) — inert skeleton of the read of the edit intention: it returns non-null values (an empty
    /// base, a refusal with a provisional text and a witness that was not supplied) and never throws.
    /// </summary>
    public static class PushBackPilotIntentReader
    {
        public const string NoIdentityDiagnostic = "provisional";
        public const string OriginMissingDiagnostic = "provisional";
        public const string UnreadableDiagnostic = "provisional";
        public const string OriginNotMemberDiagnostic = "provisional";
        public const string OriginReadOnlyDiagnostic = "provisional";
        public const string KindDivergentDiagnostic = "provisional";
        public const string OutOfPilotDiagnostic = "provisional";
        public const string DivergentDiagnostic = "provisional";
        public const string MaterializeDiagnostic = "provisional";
        public const string ClassicEditorHint = "provisional";

        public static DraftStaleBase BuildStaleBase(PushBackPilotIntentScan scan) =>
            DraftStaleBase.Create(Array.Empty<DraftStaleBaseEntry>());

        public static PushBackPilotIntentResult Read(PushBackPilotIntentScan scan, RackCatalog? catalog) =>
            new PushBackPilotIntentResult(
                BuildStaleBase(scan),
                DraftIntentRead<PushBackPilotDraftState>.NotAdmitted("provisional"),
                scan == null ? string.Empty : scan.OriginKey,
                null,
                null,
                PushBackPilotPreviewWitnessStatus.NotSupplied);
    }
}
