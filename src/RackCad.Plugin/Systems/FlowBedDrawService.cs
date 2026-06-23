using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
{
    /// <summary>
    /// AutoCAD-side orchestration for drawing one roller bed ("cama de rodamiento"): builds the pure lateral
    /// plan (rail + tope + rollers + brakes), turns it into a single block and lets the user drop it with the
    /// mouse. All pieces are loose instances, so it reuses the dynamic-system drawer. Geometry stays pure.
    /// </summary>
    public sealed class FlowBedDrawService
    {
        private readonly FlowBedLateralBuilder builder = new FlowBedLateralBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        public HeaderPlacementResult DrawAndPlace(Document document, FlowBedConfiguration config)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (config == null)
            {
                return HeaderPlacementResult.Failure("No hay cama para dibujar.");
            }

            try
            {
                var catalog = LateralHeaderDrawService.LoadCatalog();
                var instances = builder.Build(config, catalog);
                var plan = new DynamicSystemPlan(new List<HeaderGroup>(), instances);
                var blockName = string.Format(
                    CultureInfo.InvariantCulture,
                    "Cama {0} - fondo {1:0.##}",
                    config.BedType == FlowBedType.Pushback ? "pushback" : "dinamica",
                    config.LaneDepth);

                var block = CreateBlock(document, plan, blockName);
                return new LateralHeaderDrawService().PlaceAndReport(document, catalog, block);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        private LateralHeaderBlockResult CreateBlock(Document document, DynamicSystemPlan plan, string blockName)
        {
            var database = document.Database;

            using (document.LockDocument())
            using (var transaction = database.TransactionManager.StartTransaction())
            {
                var result = drawer.CreateSystemBlock(database, transaction, plan, blockName);
                transaction.Commit();
                return result;
            }
        }
    }
}
