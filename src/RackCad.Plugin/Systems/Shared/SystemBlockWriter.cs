using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Plugin.Drawing;

namespace RackCad.Plugin.Systems.Shared
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
            Document document, LateralHeaderDrawer drawer, HeaderRunPlan plan, string blockName, string payloadJson)
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
            Document document, LateralHeaderDrawer drawer, ObjectId blockId, HeaderRunPlan plan,
            string payloadJson, RackCatalog catalog, bool regen = true)
        {
            try
            {
                var database = document.Database;

                LateralHeaderDrawOutcome outcome;
                System.Collections.Generic.IReadOnlyCollection<ObjectId> staleDefs;
                using (document.LockDocument())
                {
                    // PREPARE — outside the transaction, because it can IMPORT block definitions and therefore
                    // mutate the database on its own account.
                    BlockLibraryImporter.EnsureForPlan(database, plan);

                    using (var transaction = database.TransactionManager.StartTransaction())
                    {
                        // MUTATE — through the shared primitive, so this path and the lateral one cannot drift.
                        outcome = RedefineInTransaction(database, transaction, drawer, blockId, plan, payloadJson, out staleDefs);
                        transaction.Commit();
                    }

                    // POST — after the commit.
                    PurgeAfterCommit(database, staleDefs);
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

        /// <summary>
        /// MUTATE — redefine a block definition and write its payload INSIDE A TRANSACTION THE CALLER OWNS
        /// (I-47 G9). This is the one authority for that step; both writers of the slice go through it.
        ///
        /// <para>
        /// It does NOT commit, does NOT regenerate, does NOT open a nested transaction, does NOT take the
        /// document lock and does NOT import block definitions. Every one of those belongs to the caller,
        /// before or after — and that is what makes it possible for one operation to touch the register and
        /// every view of every affected rack under a SINGLE commit.
        /// </para>
        /// <para>
        /// The old shape could not offer that: each redraw opened and committed its own transaction, so a
        /// batch over N racks that failed at rack <c>k</c> left <c>k-1</c> already committed and the drawing
        /// showing two different values for the same variable. A preflight does not fix that — it avoids
        /// starting badly, it does not undo what is already committed.
        /// </para>
        /// <para>
        /// Importing is excluded on purpose and not by omission: with the block library absent
        /// <c>EnsureBlocks</c> returns 0 silently, so relying on it mid-mutation would turn a missing library
        /// into incomplete geometry with no warning.
        /// </para>
        /// </summary>
        internal static LateralHeaderDrawOutcome RedefineInTransaction(
            Database database,
            Transaction transaction,
            LateralHeaderDrawer drawer,
            ObjectId blockId,
            HeaderRunPlan plan,
            string payloadJson,
            out System.Collections.Generic.IReadOnlyCollection<ObjectId> staleDefinitions)
        {
            var outcome = drawer.RedefineSystemBlock(database, transaction, blockId, plan, out staleDefinitions);
            RackBlockData.Write(transaction, blockId, payloadJson);
            return outcome;
        }

        /// <summary>
        /// POST — purge the nested definitions the rewrite orphaned, AFTER the commit. On the committed state
        /// <c>Database.Purge</c> filters to the genuinely unreferenced ones in one optimized pass.
        ///
        /// <para>A failure here is a DIAGNOSTIC, not a semantic rollback: the operation already committed, and
        /// leftover orphaned definitions are untidy rather than wrong.</para>
        /// </summary>
        internal static void PurgeAfterCommit(
            Database database,
            System.Collections.Generic.IReadOnlyCollection<ObjectId> staleDefinitions)
            => LateralHeaderDrawer.PurgeUnreferenced(database, staleDefinitions);

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
