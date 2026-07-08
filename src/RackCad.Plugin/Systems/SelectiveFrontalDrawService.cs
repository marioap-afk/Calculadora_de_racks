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
        /// Draws + places the selective. When <paramref name="payloadJson"/> is given (the serialized design
        /// with its Id + Name), it is embedded on the placed block so the rack can be reopened and edited.
        /// </summary>
        public HeaderPlacementResult DrawAndPlace(Document document, SelectiveRackSystem system, string payloadJson = null)
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
                var instances = builder.Build(system, catalog);
                var plan = new DynamicSystemPlan(new List<HeaderGroup>(), instances);
                var blockName = string.Format(
                    CultureInfo.InvariantCulture,
                    "Selectivo frontal - {0} frentes - H{1:0.##}",
                    system.Bays.Count,
                    system.Height);

                var block = CreateBlock(document, plan, blockName);
                var result = new LateralHeaderDrawService().PlaceAndReport(document, catalog, block);

                if (result.Success && result.Placed && !result.PlacedId.IsNull && !string.IsNullOrEmpty(payloadJson))
                {
                    WritePayload(document, result.PlacedId, payloadJson);
                }

                return result;
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        private static void WritePayload(Document document, ObjectId entityId, string payloadJson)
        {
            var database = document.Database;

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                RackBlockData.Write(transaction, entityId, payloadJson);
                transaction.Commit();
            }
        }

        private LateralHeaderBlockResult CreateBlock(Document document, DynamicSystemPlan plan, string blockName)
        {
            var database = document.Database;

            using (document.LockDocument())
            {
                BlockLibraryImporter.EnsureForPlan(database, plan);

                using (var transaction = database.TransactionManager.StartTransaction())
                {
                    var result = drawer.CreateSystemBlock(database, transaction, plan, blockName);
                    transaction.Commit();
                    return result;
                }
            }
        }
    }
}
