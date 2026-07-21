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
            => ViewBlockDraw.DrawAndPlace(
                document,
                system != null,
                "No hay sistema dinámico para dibujar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog),
                () => BlockName(system, rackName),
                payloadJson);

        public HeaderPlacementResult RedrawInPlace(
            Document document,
            ObjectId blockId,
            DynamicRackSystem system,
            string payloadJson,
            bool regen = true)
            => ViewBlockDraw.RedrawInPlace(
                document,
                blockId,
                system != null && !blockId.IsNull,
                "No hay sistema dinámico para actualizar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog),
                payloadJson,
                regen);

        private static string BlockName(DynamicRackSystem system, string rackName)
        {
            if (!string.IsNullOrWhiteSpace(rackName)) return rackName.Trim() + " - planta";
            return string.Format(CultureInfo.InvariantCulture, "Dinamico planta - {0} frentes", system.Fronts.Count);
        }
    }
}
