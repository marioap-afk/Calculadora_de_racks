using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Plugin.Systems;

namespace RackCad.Plugin.Headers
{
    /// <summary>
    /// Draws the PLANTA (top-down) view of a cabecera — the post footprints, plates and the collapsed celosía — as a
    /// single block, jig-placed, with the rack payload embedded on the block DEFINITION so every copy shares it and
    /// RACKEDITAR can reopen it. Mirrors <c>SelectiveFrontalDrawService</c> (loose instances → plan → block).
    /// </summary>
    public sealed class PlantaHeaderDrawService
    {
        private readonly PlantaHeaderLayoutBuilder builder = new PlantaHeaderLayoutBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        public HeaderPlacementResult DrawAndPlace(Document document, RackFrameConfiguration config, string payloadJson = null, string rackName = null)
            => ViewBlockDraw.DrawAndPlace(
                document,
                config != null,
                "No hay configuracion para dibujar.",
                drawer,
                catalog => new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog)),
                () => BlockName(rackName),
                payloadJson);

        /// <summary>Redraw an existing planta block DEFINITION in place (all copies update on regen).</summary>
        public HeaderPlacementResult RedrawInPlace(Document document, ObjectId blockId, RackFrameConfiguration config, string payloadJson, bool regen = true)
            => ViewBlockDraw.RedrawInPlace(
                document,
                blockId,
                config != null && !blockId.IsNull,
                "No hay cabecera para actualizar.",
                drawer,
                catalog => new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog)),
                payloadJson,
                regen);

        private static string BlockName(string rackName)
            => string.IsNullOrWhiteSpace(rackName) ? "Cabecera planta" : rackName.Trim() + " - planta";
    }
}
