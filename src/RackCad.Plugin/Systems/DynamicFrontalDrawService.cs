using System;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
{
    /// <summary>AutoCAD orchestration for one entrance/exit frontal cut of a linked dynamic system.</summary>
    public sealed class DynamicFrontalDrawService
    {
        private readonly DynamicSystemFrontalBuilder builder = new DynamicSystemFrontalBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        public HeaderPlacementResult DrawAndPlace(
            Document document,
            DynamicRackSystem system,
            DynamicRackEnd end,
            string payloadJson = null,
            string rackName = null)
        {
            if (document == null) return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            if (system == null) return HeaderPlacementResult.Failure("No hay sistema dinámico para dibujar.");

            try
            {
                var catalog = RackCatalogLoader.Load();
                var plan = builder.BuildPlan(system, catalog, end);
                var block = SystemBlockWriter.CreateBlock(document, drawer, plan, BlockName(system, rackName, end), payloadJson);
                return new LateralHeaderDrawService().PlaceAndReport(document, catalog, block);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        public HeaderPlacementResult RedrawInPlace(
            Document document,
            ObjectId blockId,
            DynamicRackSystem system,
            DynamicRackEnd end,
            string payloadJson,
            bool regen = true)
        {
            if (document == null) return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            if (system == null || blockId.IsNull) return HeaderPlacementResult.Failure("No hay sistema dinámico para actualizar.");

            try
            {
                var catalog = RackCatalogLoader.Load();
                return SystemBlockWriter.RedrawInPlace(
                    document,
                    drawer,
                    blockId,
                    builder.BuildPlan(system, catalog, end),
                    payloadJson,
                    catalog,
                    regen);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        private static string BlockName(DynamicRackSystem system, string rackName, DynamicRackEnd end)
        {
            var suffix = end == DynamicRackEnd.Entrance ? "frontal entrada" : "frontal salida";
            if (!string.IsNullOrWhiteSpace(rackName)) return rackName.Trim() + " - " + suffix;
            return string.Format(CultureInfo.InvariantCulture, "Dinamico {0} - {1} frentes", suffix, system.Fronts.Count);
        }
    }
}
