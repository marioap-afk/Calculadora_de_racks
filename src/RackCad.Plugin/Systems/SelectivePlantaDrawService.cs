using System;
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
    /// frame + the front/back largueros, jig-placed with the payload embedded on the definition so RACKEDITAR
    /// reopens the whole system. Uses <see cref="SelectivePlantaBuilder.BuildPlan"/> so identical frames share ONE
    /// nested definition referenced per frente (the dynamic system's ARRAY pattern) — a 30-frente run appends the
    /// frame pieces once instead of 31 times, which is what made the planta slow to generate. The common
    /// draw/redraw flow lives in <see cref="ViewBlockDraw"/>.
    /// </summary>
    public sealed class SelectivePlantaDrawService
    {
        private readonly SelectivePlantaBuilder builder = new SelectivePlantaBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        public HeaderPlacementResult DrawAndPlace(Document document, SelectiveRackSystem system, string payloadJson = null, string rackName = null)
            => ViewBlockDraw.DrawAndPlace(
                document,
                system != null,
                "No hay sistema selectivo para dibujar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog),
                () => BlockName(system, rackName),
                payloadJson);

        public HeaderPlacementResult RedrawInPlace(Document document, ObjectId blockId, SelectiveRackSystem system, string payloadJson, bool regen = true)
            => ViewBlockDraw.RedrawInPlace(
                document,
                blockId,
                system != null && !blockId.IsNull,
                "No hay rack para actualizar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog),
                payloadJson,
                regen);

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
