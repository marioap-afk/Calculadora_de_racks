using System;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
{
    /// <summary>
    /// AutoCAD-side orchestration for drawing a whole dynamic (pallet flow) system: builds the pure plan
    /// (distinct headers + separators per level + derived posts), turns it into a system block whose headers
    /// are nested block references (identical headers shared), and lets the user drop it with the mouse. The
    /// geometry stays in the Application layer.
    /// </summary>
    public sealed class DynamicSystemDrawService
    {
        private readonly DynamicSystemLateralBuilder builder = new DynamicSystemLateralBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        /// <summary>
        /// Draws + places the dynamic system. The design payload (JSON incl. Id + Name) is embedded on the block
        /// DEFINITION so every copy shares it and the rack can be reopened/edited; <paramref name="rackName"/> names it.
        /// </summary>
        public HeaderPlacementResult DrawAndPlace(Document document, DynamicRackSystem system, string payloadJson = null, string rackName = null)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (system == null)
            {
                return HeaderPlacementResult.Failure("No hay sistema para dibujar.");
            }

            try
            {
                var catalog = LateralHeaderDrawService.LoadCatalog();
                var plan = builder.Build(system, catalog);
                var block = CreateSystemBlock(document, plan, BlockName(system, rackName), payloadJson);
                return new LateralHeaderDrawService().PlaceAndReport(document, catalog, block);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>Redraw an existing dynamic system's block DEFINITION in place; every copy updates on regen.</summary>
        public HeaderPlacementResult RedrawInPlace(Document document, ObjectId blockId, DynamicRackSystem system, string payloadJson)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (system == null || blockId.IsNull)
            {
                return HeaderPlacementResult.Failure("No hay sistema para actualizar.");
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

                return new HeaderPlacementResult(true, true, null, LateralHeaderDrawService.DescribeMissing(catalog, outcome), outcome);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        private LateralHeaderBlockResult CreateSystemBlock(Document document, DynamicSystemPlan plan, string blockName, string payloadJson)
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

        private static string BlockName(DynamicRackSystem system, string rackName)
        {
            if (!string.IsNullOrWhiteSpace(rackName))
            {
                return rackName.Trim();
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "Sistema dinamico - {0} fondos - L{1:0.##}",
                system.PalletsDeep,
                system.TotalLength);
        }
    }
}
