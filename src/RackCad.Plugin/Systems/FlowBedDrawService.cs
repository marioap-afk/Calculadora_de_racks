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
            => ViewBlockDraw.DrawAndPlace(
                document,
                config != null,
                "No hay cama para dibujar.",
                drawer,
                catalog => new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog)),
                () => BlockName(config, rackName),
                payloadJson);

        /// <summary>Redraw an existing bed's block DEFINITION in place; every copy updates on regen. The public
        /// signature keeps NO regen parameter (F3 does not add one); a bed is single-view, so it always regens
        /// once internally — passed to <see cref="ViewBlockDraw"/> as regen: true, the same as the prior default.</summary>
        public HeaderPlacementResult RedrawInPlace(Document document, ObjectId blockId, FlowBedConfiguration config, string payloadJson)
            => ViewBlockDraw.RedrawInPlace(
                document,
                blockId,
                config != null && !blockId.IsNull,
                "No hay cama para actualizar.",
                drawer,
                catalog => new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog)),
                payloadJson,
                regen: true);

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
