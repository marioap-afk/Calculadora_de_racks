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
            => ViewBlockDraw.DrawAndPlace(
                document,
                system != null,
                "No hay sistema dinámico para dibujar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog, end),
                () => BlockName(system, rackName, end),
                payloadJson);

        public HeaderPlacementResult RedrawInPlace(
            Document document,
            ObjectId blockId,
            DynamicRackSystem system,
            DynamicRackEnd end,
            string payloadJson,
            bool regen = true)
            => ViewBlockDraw.RedrawInPlace(
                document,
                blockId,
                system != null && !blockId.IsNull,
                "No hay sistema dinámico para actualizar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog, end),
                payloadJson,
                regen);

        private static string BlockName(DynamicRackSystem system, string rackName, DynamicRackEnd end)
        {
            var suffix = end == DynamicRackEnd.Entrance ? "frontal entrada" : "frontal salida";
            if (!string.IsNullOrWhiteSpace(rackName)) return rackName.Trim() + " - " + suffix;
            return string.Format(CultureInfo.InvariantCulture, "Dinamico {0} - {1} frentes", suffix, system.Fronts.Count);
        }
    }
}
