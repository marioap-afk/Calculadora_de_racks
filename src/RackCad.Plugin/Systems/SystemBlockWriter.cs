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

        /// <summary>Redefine an existing block DEFINITION in place from a plan; every copy updates on regen.</summary>
        internal static HeaderPlacementResult RedrawInPlace(
            Document document, LateralHeaderDrawer drawer, ObjectId blockId, DynamicSystemPlan plan,
            string payloadJson, RackCatalog catalog)
        {
            try
            {
                var database = document.Database;

                LateralHeaderDrawOutcome outcome;
                using (document.LockDocument())
                {
                    BlockLibraryImporter.EnsureForPlan(database, plan);

                    using (var transaction = database.TransactionManager.StartTransaction())
                    {
                        outcome = drawer.RedefineSystemBlock(database, transaction, blockId, plan);
                        RackBlockData.Write(transaction, blockId, payloadJson);
                        transaction.Commit();
                    }

                    document.Editor.Regen();
                }

                // Report pieces skipped during the redraw too — an edit can lose blocks just like an insert.
                return new HeaderPlacementResult(true, true, null, LateralHeaderDrawService.DescribeMissing(catalog, outcome), outcome);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }
    }
}
