using System;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
{
    /// <summary>
    /// AutoCAD-side orchestration for the LATERAL view of a selective rack: each post drawn as its cabecera
    /// (via <see cref="SelectiveLateralBuilder"/>) — a system block with the distinct cabeceras as nested,
    /// shared blocks. Same identity/round-trip as the frontal, distinguished by the embed's View = "lateral".
    /// </summary>
    public sealed class SelectiveLateralDrawService
    {
        private readonly SelectiveLateralBuilder builder = new SelectiveLateralBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        public HeaderPlacementResult DrawAndPlace(Document document, SelectiveRackSystem system, string payloadJson = null, string rackName = null)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (system == null)
            {
                return HeaderPlacementResult.Failure("No hay sistema selectivo para dibujar.");
            }

            try
            {
                var catalog = LateralHeaderDrawService.LoadCatalog();
                var plan = builder.Build(system, catalog);
                var block = CreateBlock(document, plan, BlockName(system, rackName), payloadJson);
                return new LateralHeaderDrawService().PlaceAndReport(document, catalog, block);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>Redraw the selective's LATERAL block DEFINITION in place; every copy updates on regen.</summary>
        public HeaderPlacementResult RedrawInPlace(Document document, ObjectId blockId, SelectiveRackSystem system, string payloadJson)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (system == null || blockId.IsNull)
            {
                return HeaderPlacementResult.Failure("No hay rack para actualizar.");
            }

            try
            {
                var catalog = LateralHeaderDrawService.LoadCatalog();
                var plan = builder.Build(system, catalog);
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

                return new HeaderPlacementResult(true, true, null, Array.Empty<string>(), outcome);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        private LateralHeaderBlockResult CreateBlock(Document document, DynamicSystemPlan plan, string blockName, string payloadJson)
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

        private static string BlockName(SelectiveRackSystem system, string rackName)
        {
            var baseName = !string.IsNullOrWhiteSpace(rackName)
                ? rackName.Trim()
                : string.Format(CultureInfo.InvariantCulture, "Selectivo - {0} frentes - H{1:0.##}", system.Bays.Count, system.Height);

            return baseName + " (lateral)";
        }
    }
}
