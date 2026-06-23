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

        public HeaderPlacementResult DrawAndPlace(Document document, DynamicRackSystem system)
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
                var blockName = string.Format(
                    CultureInfo.InvariantCulture,
                    "Sistema dinamico - {0} fondos - L{1:0.##}",
                    system.PalletsDeep,
                    system.TotalLength);

                var block = CreateSystemBlock(document, plan, blockName);
                return new LateralHeaderDrawService().PlaceAndReport(document, catalog, block);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        private LateralHeaderBlockResult CreateSystemBlock(Document document, DynamicSystemPlan plan, string blockName)
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
