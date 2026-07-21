using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
{
    /// <summary>
    /// The one implementation of "turn a plan into a system block" and "redefine an existing block from a
    /// plan" — previously copied byte-for-byte into every draw service (selective frontal/planta, cabecera
    /// planta, dynamic, cama). Payloads go on the block DEFINITION so every reference/copy shares them.
    /// </summary>
    internal static class SystemBlockWriter
    {
        /// <summary>Create a system block from a plan, embedding <paramref name="payloadJson"/> on the definition.</summary>
        internal static LateralHeaderBlockResult CreateBlock(
            Document document, LateralHeaderDrawer drawer, DynamicSystemPlan plan, string blockName, string payloadJson)
        {
            var database = document.Database;

            using (document.LockDocument())
            {
                BlockLibraryImporter.EnsureForPlan(database, plan);

                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var result = drawer.CreateSystemBlock(database, transaction, plan, blockName);

                    if (!string.IsNullOrEmpty(payloadJson))
                    {
                        RackBlockData.Write(transaction, result.DefinitionId, payloadJson);
                    }

                    transaction.Commit();
                    return result;
                }
            }
        }

        /// <summary>Redefine an existing block DEFINITION in place from a plan; every copy updates on regen.
        /// Pass <paramref name="regen"/> = false when redrawing several blocks in a loop and regen ONCE after —
        /// a full drawing regeneration per block is pure waste (same pattern as LateralHeaderDrawService).</summary>
        internal static HeaderPlacementResult RedrawInPlace(
            Document document, LateralHeaderDrawer drawer, ObjectId blockId, DynamicSystemPlan plan,
            string payloadJson, RackCatalog catalog, bool regen = true)
        {
            try
            {
                var database = document.Database;

                LateralHeaderDrawOutcome outcome;
                System.Collections.Generic.IReadOnlyCollection<ObjectId> staleDefs;
                using (document.LockDocument())
                {
                    BlockLibraryImporter.EnsureForPlan(database, plan);

                    using (var transaction = database.TransactionManager.StartTransaction())
                    {
                        outcome = drawer.RedefineSystemBlock(database, transaction, blockId, plan, out staleDefs);
                        RackBlockData.Write(transaction, blockId, payloadJson);
                        transaction.Commit();
                    }

                    // Purge the nested defs the rewrite orphaned AFTER commit: on the committed state Database.Purge
                    // filters to the genuinely unreferenced ones in one optimized pass (no per-def whole-drawing scan).
                    LateralHeaderDrawer.PurgeUnreferenced(database, staleDefs);

                    ApplyRegen(document, regen);
                }

                // Report pieces skipped during the redraw too — an edit can lose blocks just like an insert.
                return new HeaderPlacementResult(true, true, null, LateralHeaderDrawService.DescribeMissing(catalog, outcome), outcome);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>Regenerate the drawing once when <paramref name="regen"/> is set. The SINGLE place the
        /// write/redraw path applies the flag, so this and <see cref="LateralHeaderDrawService"/>'s in-place redraw
        /// stay identical (I-16 F4). Callers invoke it AFTER commit + purge, inside the document lock — the position
        /// is unchanged. Multi-view editors keep passing <c>regen: false</c> on each intermediate redraw and fire
        /// their own single <c>Editor.Regen()</c> at the end; layout and fill keep their own regens.</summary>
        internal static void ApplyRegen(Document document, bool regen)
        {
            if (regen)
            {
                document.Editor.Regen();
            }
        }
    }
}
