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
    /// AutoCAD-side orchestration for drawing one roller bed ("cama de rodamiento"): builds the pure lateral
    /// plan (rail + tope + rollers + brakes), turns it into a single block and lets the user drop it with the
    /// mouse. All pieces are loose instances, so it reuses the dynamic-system drawer. Geometry stays pure.
    /// </summary>
    public sealed class FlowBedDrawService
    {
        private readonly FlowBedLateralBuilder builder = new FlowBedLateralBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        public HeaderPlacementResult DrawAndPlace(Document document, FlowBedConfiguration config, string payloadJson = null, string rackName = null)
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
                var plan = new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog));

                var block = CreateBlock(document, plan, BlockName(config, rackName), payloadJson);
                return new LateralHeaderDrawService().PlaceAndReport(document, catalog, block);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>Redraw an existing bed's block DEFINITION in place; every copy updates on regen.</summary>
        public HeaderPlacementResult RedrawInPlace(Document document, ObjectId blockId, FlowBedConfiguration config, string payloadJson)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (config == null || blockId.IsNull)
            {
                return HeaderPlacementResult.Failure("No hay cama para actualizar.");
            }

            try
            {
                var catalog = LateralHeaderDrawService.LoadCatalog();
                var plan = new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog));
                return SystemBlockWriter.RedrawInPlace(document, drawer, blockId, plan, payloadJson, catalog);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        private LateralHeaderBlockResult CreateBlock(Document document, DynamicSystemPlan plan, string blockName, string payloadJson)
            => SystemBlockWriter.CreateBlock(document, drawer, plan, blockName, payloadJson);

        private static string BlockName(FlowBedConfiguration config, string rackName)
        {
            if (!string.IsNullOrWhiteSpace(rackName))
            {
                return rackName.Trim();
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "Cama {0} - fondo {1:0.##}",
                config.BedType == FlowBedType.Pushback ? "pushback" : "dinamica",
                config.LaneDepth);
        }
    }
}
