#nullable enable
using System.Diagnostics;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.Shared;

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
    /// I-64 F4-T2B2A (D-T2B2-02, INV-14) — the adapter that reads the drawing for the edit intention of the bounded Push Back
    /// pilot. It reads and writes nothing else: the one whole-drawing scan (with its raw envelope text) feeds the pure reader of the
    /// pilot, and what comes back is a value. It keeps no document, no transaction and no database object in a field.
    ///
    /// <see cref="Read"/> runs INSIDE the lock that the operation of the host already holds; it takes no lock of its own.
    /// <see cref="ReadPreviewWitness"/> is a light read with its own lock and its own short transaction, which is never committed.
    /// </summary>
    internal static class RackPilotIntentAdapter
    {
        private const string CompletenessEvidence = "RackSiblingScan/CaptureDrawing";

        /// <summary>The catalog of the pilot. It is called OUTSIDE the lock of the operation; a failure is a typed value.</summary>
        internal static RackPilotCatalogResult LoadCatalog()
        {
            try
            {
                var catalog = LateralHeaderDrawService.LoadCatalog();
                return catalog == null
                    ? new RackPilotCatalogResult(null, "The catalog is not available.")
                    : new RackPilotCatalogResult(catalog, null);
            }
            catch (System.Exception ex)
            {
                return new RackPilotCatalogResult(null, "The catalog could not be loaded: " + ex.Message);
            }
        }

        /// <summary>
        /// ONE read of the edit intention: one scan of the drawing and the pure read over it. The key of the origin is the handle
        /// of its definition as the scan writes it, and the handle 0 gives the empty key (the reader then says that the origin is
        /// missing). A witness with an empty key is a typed failure and no input is built.
        /// </summary>
        internal static RackPilotIntentReadResult Read(
            Document document,
            string rackId,
            long originDefinitionHandle,
            string? witnessKey,
            string? witnessText,
            RackCatalog catalog)
        {
            try
            {
                AssertNoOpenTransaction(document, "before");

                var originKey = originDefinitionHandle <= 0 ? string.Empty : ToHandle(originDefinitionHandle).ToString();

                PushBackPilotPreviewWitness? witness = null;
                if (witnessKey != null)
                {
                    if (string.IsNullOrWhiteSpace(witnessKey))
                        return new RackPilotIntentReadResult(null, "The preview witness has no key.");
                    witness = new PushBackPilotPreviewWitness(witnessKey, witnessText);
                }

                var scan = RackSiblingScan.CaptureDrawing(document);
                if (scan.RawEnvelopes == null)
                    return new RackPilotIntentReadResult(null, "The scan of the drawing carries no raw envelopes.");

                var input = new PushBackPilotIntentScan(rackId, originKey, scan.Facts, scan.RawEnvelopes, CompletenessEvidence, witness);
                var read = PushBackPilotIntentReader.Read(input, catalog);

                return new RackPilotIntentReadResult(read, null);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex) when (ex.ErrorStatus == ErrorStatus.LockViolation)
            {
                return new RackPilotIntentReadResult(null, "The document is locked by another operation.");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                return new RackPilotIntentReadResult(null, "The drawing could not be read: " + ex.Message);
            }
            catch (System.Exception ex)
            {
                return new RackPilotIntentReadResult(null, "The edit intention could not be read: " + ex.Message);
            }
            finally
            {
                // INV-14: every path of the read (the success, the early return and the catches) leaves no transaction open.
                AssertNoOpenTransaction(document, "after");
            }
        }

        /// <summary>
        /// The light read of what a preview shows: the key and the raw envelope text of ONE definition. It has its own lock and its
        /// own short transaction, which is only read and never committed, and it resolves the definition with the same handle
        /// helper as <see cref="Read"/>. The text is null when the definition is not there or has none. It is for the panel (F4-T3).
        /// </summary>
        internal static RackPilotWitnessResult ReadPreviewWitness(Document document, long definitionHandle)
        {
            try
            {
                // INV-14: no transaction is open before the lock and the transaction of this read, and none after both are closed.
                AssertNoOpenTransaction(document, "before");

                if (definitionHandle <= 0)
                    return new RackPilotWitnessResult(null, "The definition has no handle.");

                var handle = ToHandle(definitionHandle);
                using (document.LockDocument())
                using (var transaction = document.Database.TransactionManager.StartTransaction())
                {
                    string? text = null;
                    if (document.Database.TryGetObjectId(handle, out var id) && !id.IsNull)
                        text = RackBlockData.Read(transaction, id);

                    return new RackPilotWitnessResult(
                        new PushBackPilotPreviewWitness(handle.ToString(), string.IsNullOrEmpty(text) ? null : text),
                        null);
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex) when (ex.ErrorStatus == ErrorStatus.LockViolation)
            {
                return new RackPilotWitnessResult(null, "The document is locked by another operation.");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                return new RackPilotWitnessResult(null, "The preview witness could not be read: " + ex.Message);
            }
            catch (System.Exception ex)
            {
                return new RackPilotWitnessResult(null, "The preview witness could not be read: " + ex.Message);
            }
            finally
            {
                // The lock and the transaction are closed by now (both using blocks are inside the try).
                AssertNoOpenTransaction(document, "after");
            }
        }

        // The one conversion from the long handle of the host to the handle of the drawing.
        private static Handle ToHandle(long value) => new Handle(value);

        // In Debug, the read is never made inside an open transaction and leaves none open.
        [Conditional("DEBUG")]
        private static void AssertNoOpenTransaction(Document document, string moment)
        {
            Debug.Assert(
                document.Database.TransactionManager.TopTransaction == null,
                "The read of the edit intention found a transaction open " + moment + " it ran.");
        }
    }
}
