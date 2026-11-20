using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4a (D-PBT4-03, A-T3-8) — the origin of an open draft, found by its text: the key of the ONLY mutable member
    /// whose raw text is, byte for byte, the origin of the draft (the rule of the run, UR:640-650), the same scan at that key, and
    /// the two pure reads over it (the pilot and the full model), as PushBackFullDraftReader does.
    /// </summary>
    public static class PushBackOriginByText
    {
        public static string Choose(DraftStaleBase staleBase, string originText)
        {
            if (staleBase == null || originText == null) return null;
            var matches = staleBase.Entries.Values
                .Where(entry => entry.Membership == DraftMembership.Mutable && string.Equals(entry.EnvelopeText, originText, StringComparison.Ordinal))
                .Select(entry => entry.Key)
                .ToList();
            return matches.Count == 1 ? matches[0] : null;
        }

        public static PushBackPilotIntentScan AtKey(PushBackPilotIntentScan scan, string originKey)
        {
            if (scan == null) throw new ArgumentNullException(nameof(scan));
            return new PushBackPilotIntentScan(
                scan.RackId,
                originKey,
                scan.Facts,
                scan.RawEnvelopes.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
                scan.CompletenessEvidence,
                scan.PreviewWitness);
        }

        public static (PushBackPilotIntentScan Scan, PushBackPilotIntentResult Pilot, PushBackFullIntentResult Full)? ReadAt(PushBackPilotIntentScan scan, string originText, RackCatalog catalog)
        {
            if (scan == null) throw new ArgumentNullException(nameof(scan));
            var key = Choose(PushBackPilotIntentReader.BuildStaleBase(scan), originText);
            if (key == null || key == scan.OriginKey) return null;
            var input = AtKey(scan, key);
            return (input, PushBackPilotIntentReader.Read(input, catalog), PushBackFullDraftReader.Read(input, catalog));
        }
    }
}
