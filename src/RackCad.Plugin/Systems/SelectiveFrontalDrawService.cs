using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
{
    /// <summary>
    /// AutoCAD-side orchestration for drawing a selective rack in the frontal view: builds the pure plan
    /// (posts + base plates + largueros per level), turns it into a single block and lets the user drop it
    /// with the mouse. All pieces are loose instances, so it reuses the dynamic-system drawer + jig.
    /// </summary>
    public sealed class SelectiveFrontalDrawService
    {
        private readonly SelectiveFrontalBuilder builder = new SelectiveFrontalBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        /// <summary>
        /// Draws + places the selective. The design payload (JSON incl. Id + Name) is embedded on the block
        /// DEFINITION (not the reference), so every copy of the rack shares it. <paramref name="rackName"/> names
        /// the block when given.
        /// </summary>
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
                var plan = new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(system, catalog));

                var block = CreateBlock(document, plan, BlockName(system, rackName), payloadJson);
                return new LateralHeaderDrawService().PlaceAndReport(document, catalog, block);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>
        /// Redraw an existing rack's block DEFINITION in place (found from a selected reference), keeping its id
        /// and name. Every reference to it — all the copies of that rack — updates on regen.
        /// </summary>
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
                var plan = new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(system, catalog));
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

                return new HeaderPlacementResult(true, true, null, LateralHeaderDrawService.DescribeMissing(catalog, outcome), outcome);
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

                    // Payload on the DEFINITION so every reference/copy shares it.
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
            if (!string.IsNullOrWhiteSpace(rackName))
            {
                return rackName.Trim();
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "Selectivo frontal - {0} frentes - H{1:0.##}",
                system.Bays.Count,
                system.Height);
        }
    }
}
