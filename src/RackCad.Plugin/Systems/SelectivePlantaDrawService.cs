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
    /// AutoCAD-side orchestration for the selective's PLANTA (top-down) view: one block with a cabecera-planta per
    /// frame + the front/back largueros. Mirrors <see cref="SelectiveFrontalDrawService"/> (loose instances → plan →
    /// one jig-placed block, payload embedded on the definition so RACKEDITAR reopens the whole system).
    /// </summary>
    public sealed class SelectivePlantaDrawService
    {
        private readonly SelectivePlantaBuilder builder = new SelectivePlantaBuilder();
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
                var plan = new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(system, catalog));
                var block = CreateBlock(document, plan, BlockName(system, rackName), payloadJson);
                return new LateralHeaderDrawService().PlaceAndReport(document, catalog, block);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

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
                return SystemBlockWriter.RedrawInPlace(document, drawer, blockId, plan, payloadJson, catalog);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        private LateralHeaderBlockResult CreateBlock(Document document, DynamicSystemPlan plan, string blockName, string payloadJson)
            => SystemBlockWriter.CreateBlock(document, drawer, plan, blockName, payloadJson);

        private static string BlockName(SelectiveRackSystem system, string rackName)
        {
            if (!string.IsNullOrWhiteSpace(rackName))
            {
                return rackName.Trim() + " - planta";
            }

            return string.Format(CultureInfo.InvariantCulture, "Selectivo planta - {0} frentes", system.Bays.Count);
        }
    }
}
