using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Persistence;

namespace RackCad.Plugin
{
    /// <summary>
    /// Shared Plugin helper (I-09): locate block references/definitions in the drawing. Extracted from the
    /// cross-command <c>FindFirstModelSpaceReference</c> that lived in <see cref="RackFrameCommands"/> (used by
    /// RACKLISTA's zoom and RACKLAYOUT's planta seed), so reference lookup lives in ONE place. The caller supplies
    /// (and owns) the transaction; the returned <see cref="BlockReference"/> is used within that same transaction.
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
    }
}
