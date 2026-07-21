using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Persistence;
using RackCad.Plugin.Systems;

namespace RackCad.Plugin
{
    /// <summary>
    /// Shared Plugin helper (I-09): locate block references/definitions in the drawing and scan rack envelopes.
    /// Extracted from the cross-command lookups that lived in <see cref="RackFrameCommands"/> (used by RACKLISTA's
    /// zoom, RACKLAYOUT's planta seed, RACKEDITAR/RACKLAYOUT's rack lookup and RACKLISTA/RACKBOMTOTAL's inventory),
    /// so definition/reference lookup lives in ONE place. The caller supplies (and owns) the transaction; returned
    /// live objects are used within that same transaction and no <c>DBObject</c> leaves it.
    /// </summary>
    internal static class RackBlockFinder
    {
        /// <summary>First model-space reference among the rack's view-blocks, frontal first (scan order otherwise).</summary>
        public static BlockReference FindFirstModelSpaceReference(Transaction transaction, Database database, List<(ObjectId BlockId, RackEmbedDocument Embed)> blocks)
        {
            var modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(database);
            var ordered = blocks.OrderBy(block =>
                string.Equals(block.Embed.View, RackEmbedDocument.ViewFrontal, StringComparison.OrdinalIgnoreCase) ? 0 : 1);

            foreach (var block in ordered)
            {
                var record = (BlockTableRecord)transaction.GetObject(block.BlockId, OpenMode.ForRead);
                foreach (ObjectId referenceId in record.GetBlockReferenceIds(directOnly: true, forceValidity: false))
                {
                    var reference = (BlockReference)transaction.GetObject(referenceId, OpenMode.ForRead);
                    if (reference.OwnerId == modelSpaceId)
                    {
                        return reference;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// The ONE reusable BlockTable traversal for rack envelopes (I-09 F3): one pass over the block DEFINITIONS
        /// (never physical references as the entry point), skipping layouts, anonymous and xref-owned records, reading
        /// each embedded payload with <see cref="RackBlockData.Read"/> and deserializing it with
        /// <see cref="RackEmbedStore"/>. It preserves the block-table traversal order and returns plain snapshots — a
        /// definition <see cref="ObjectId"/>, its (possibly null) <see cref="RackEmbedDocument"/> and, only when
        /// <paramref name="includeReferenceCount"/> is set, the count of DIRECT references — so no BlockTableRecord or
        /// BlockReference escapes the caller's transaction.
        ///
        /// The consumer-specific policy stays with the consumer: which envelopes are valid (FindRackBlocks matches by
        /// GUID and tolerates a missing Kind; RACKLISTA/RACKBOMTOTAL require a non-blank Id and Kind), whether to keep
        /// every view or one representative, and how to aggregate copies. The reference count keeps
        /// <c>directOnly: true</c> and <c>forceValidity: false</c> (the expensive true variant is deliberately avoided
        /// on large drawings); definitions with no references still surface as snapshots with a zero count.
        /// </summary>
        public static List<RackEnvelopeScan> ScanEnvelopes(Transaction transaction, Database database, bool includeReferenceCount)
        {
            var results = new List<RackEnvelopeScan>();
            var store = new RackEmbedStore();

            var blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
            foreach (ObjectId id in blockTable)
            {
                var record = (BlockTableRecord)transaction.GetObject(id, OpenMode.ForRead);
                if (record.IsLayout || record.IsAnonymous || record.IsFromExternalReference)
                {
                    continue;
                }

                var json = RackBlockData.Read(transaction, id);
                if (string.IsNullOrEmpty(json))
                {
                    continue;
                }

                var embed = store.Deserialize(json);
                var referenceCount = includeReferenceCount && embed != null
                    ? record.GetBlockReferenceIds(directOnly: true, forceValidity: false).Count
                    : 0;
                results.Add(new RackEnvelopeScan(id, embed, referenceCount));
            }

            return results;
        }
    }

    /// <summary>
    /// A plain snapshot of one scanned rack block definition (I-09 F3): its definition id, the deserialized envelope
    /// (null when the payload did not deserialize) and — only when the scan requested it — the number of direct
    /// references to the definition. Value data only; nothing here is a live AutoCAD object.
    /// </summary>
    internal readonly struct RackEnvelopeScan
    {
        public RackEnvelopeScan(ObjectId definitionId, RackEmbedDocument embed, int directReferenceCount)
        {
            DefinitionId = definitionId;
            Embed = embed;
            DirectReferenceCount = directReferenceCount;
        }

        public ObjectId DefinitionId { get; }

        public RackEmbedDocument Embed { get; }

        public int DirectReferenceCount { get; }
    }
}
