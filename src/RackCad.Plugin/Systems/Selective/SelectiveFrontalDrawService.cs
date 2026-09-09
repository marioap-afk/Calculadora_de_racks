using System;
using System.Collections.Generic;
using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.Shared;

namespace RackCad.Plugin.Systems.Selective
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

        /// <summary>PREPARE — catalog, plan and imports, before the caller opens its transaction (I-47 G9.1).</summary>
        internal PreparedViewRedraw PrepareRedraw(
            Database database, ObjectId blockId, SelectiveRackSystem system, string payloadJson)
            => ViewBlockDraw.PrepareRedraw(
                database,
                blockId,
                system != null && !blockId.IsNull,
                "No hay rack para actualizar.",
                drawer,
                catalog => builder.BuildPlan(system, catalog),
                payloadJson);

        /// <summary>MUTATE — redefine inside the CALLER's transaction. No lock, commit, regen or import here.</summary>
        internal LateralHeaderDrawOutcome RedrawInTransaction(
            Database database,
            Transaction transaction,
            PreparedViewRedraw prepared,
            out IReadOnlyCollection<ObjectId> staleDefinitions)
            => ViewBlockDraw.RedrawInTransaction(database, transaction, prepared, out staleDefinitions);

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
