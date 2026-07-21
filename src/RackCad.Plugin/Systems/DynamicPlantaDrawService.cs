using System;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
{
    /// <summary>AutoCAD orchestration for the linked top view of a dynamic system.</summary>
    public sealed class DynamicPlantaDrawService
    {
        private readonly DynamicSystemPlantaBuilder builder = new DynamicSystemPlantaBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        public HeaderPlacementResult DrawAndPlace(
            Document document,
            DynamicRackSystem system,
            string payloadJson = null,
            string rackName = null)
        {
            if (document == null) return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            if (system == null) return HeaderPlacementResult.Failure("No hay sistema dinámico para dibujar.");

            try
            {
                var catalog = RackCatalogLoader.Load();
                var block = SystemBlockWriter.CreateBlock(
                    document,
                    drawer,
                    builder.BuildPlan(system, catalog),
                    BlockName(system, rackName),
                    payloadJson);
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
                    builder.BuildPlan(system, catalog),
                    payloadJson,
                    catalog,
                    regen);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        private static string BlockName(DynamicRackSystem system, string rackName)
        {
            if (!string.IsNullOrWhiteSpace(rackName)) return rackName.Trim() + " - planta";
            return string.Format(CultureInfo.InvariantCulture, "Dinamico planta - {0} frentes", system.Fronts.Count);
        }
    }
}
