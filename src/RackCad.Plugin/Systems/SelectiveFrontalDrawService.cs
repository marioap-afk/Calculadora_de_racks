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
    /// AutoCAD-side orchestration for drawing a selective rack in the frontal view: builds the pure plan
    /// (posts + base plates + largueros per level), turns it into a single block and lets the user drop it
    /// with the mouse. All pieces are loose instances, so it reuses the dynamic-system drawer + jig.
    /// The common draw/redraw flow lives in <see cref="ViewBlockDraw"/>; this facade supplies the payload
    /// (<see cref="SelectiveRackSystem"/>), the plan factory (<see cref="SelectiveFrontalBuilder.BuildPlan"/>),
    /// the block name and the messages.
    /// </summary>
    public sealed class SelectiveFrontalDrawService
    {
        private readonly SelectiveFrontalBuilder builder = new SelectiveFrontalBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        /// <summary>
        /// Draws + places the selective. The design payload (JSON incl. Id + Name) is embedded on the block
        /// DEFINITION (not the reference), so every copy of the rack shares it. <paramref name="rackName"/> names
        /// the block when given.
        /// </summary>
        public HeaderPlacementResult DrawAndPlace(Document document, SelectiveRackSystem system, string payloadJson = null, string rackName = null)
            => ViewBlockDraw.DrawAndPlace(
                document,
                system != null,
                "No hay sistema selectivo para dibujar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog), // ARRAY pattern: identical pieces share one nested def
                () => BlockName(system, rackName),
                payloadJson);

        /// <summary>
        /// Redraw an existing rack's block DEFINITION in place (found from a selected reference), keeping its id
        /// and name. Every reference to it — all the copies of that rack — updates on regen.
        /// </summary>
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
                return rackName.Trim();
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "Selectivo frontal - {0} frentes - H{1:0.##}",
                system.Bays.Count,
                system.Height);
        }
    }
}
