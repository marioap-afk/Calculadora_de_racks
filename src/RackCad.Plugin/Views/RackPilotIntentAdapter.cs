#nullable enable
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Pilot;

namespace RackCad.Plugin.Views
{
    /// <summary>The catalog of the pilot, or why it could not be loaded. A typed failure, never an exception.</summary>
    internal sealed class RackPilotCatalogResult
    {
        internal RackPilotCatalogResult(RackCatalog? catalog, string? failureReason)
        {
            Catalog = catalog;
            FailureReason = failureReason ?? string.Empty;
        }

        internal bool Succeeded => Catalog != null;

        internal RackCatalog? Catalog { get; }

        internal string FailureReason { get; }
    }

    /// <summary>The read of the edit intention of the pilot, or why it could not be made. A typed failure, never an exception.</summary>
    internal sealed class RackPilotIntentReadResult
    {
        internal RackPilotIntentReadResult(PushBackPilotIntentResult? pilot, string? failureReason)
        {
            Pilot = pilot;
            FailureReason = failureReason ?? string.Empty;
        }

        internal bool Succeeded => Pilot != null;

        internal PushBackPilotIntentResult? Pilot { get; }

        internal string FailureReason { get; }
    }

    /// <summary>The light read of the preview witness (key and raw envelope text), or why it could not be made.</summary>
    internal sealed class RackPilotWitnessResult
    {
        internal RackPilotWitnessResult(PushBackPilotPreviewWitness? witness, string? failureReason)
        {
            Witness = witness;
            FailureReason = failureReason ?? string.Empty;
        }

        internal bool Succeeded => Witness != null;

        internal PushBackPilotPreviewWitness? Witness { get; }

        internal string FailureReason { get; }
    }

    /// <summary>
    /// I-64 F4-T2B2A (D-T2B2-02) — inert skeleton of the adapter that reads the drawing for the edit intention of the pilot: every
    /// operation answers a typed failure, reads nothing and never throws.
    /// </summary>
    internal static class RackPilotIntentAdapter
    {
        internal static RackPilotCatalogResult LoadCatalog() => new RackPilotCatalogResult(null, "Not implemented.");

        internal static RackPilotIntentReadResult Read(
            Document document,
            string rackId,
            long originDefinitionHandle,
            string? witnessKey,
            string? witnessText,
            RackCatalog catalog) => new RackPilotIntentReadResult(null, "Not implemented.");

        internal static RackPilotWitnessResult ReadPreviewWitness(Document document, long definitionHandle) =>
            new RackPilotWitnessResult(null, "Not implemented.");
    }
}
