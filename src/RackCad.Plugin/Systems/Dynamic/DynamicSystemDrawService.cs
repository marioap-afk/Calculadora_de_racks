using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.Shared;

namespace RackCad.Plugin.Systems.Dynamic
{
    /// <summary>
    /// AutoCAD-side orchestration for drawing a whole dynamic (pallet flow) system: builds the pure plan
    /// (distinct headers + separators per level + derived posts), turns it into a system block whose headers
    /// are nested block references (identical headers shared), and lets the user drop it with the mouse. The
    /// geometry stays in the Application layer.
    /// </summary>
    public sealed class DynamicSystemDrawService
    {
        private readonly DynamicSystemLateralBuilder builder = new DynamicSystemLateralBuilder();
        private readonly LateralHeaderDrawer drawer = new LateralHeaderDrawer();

        /// <summary>
        /// Draws + places the dynamic system. The design payload (JSON incl. Id + Name) is embedded on the block
        /// DEFINITION so every copy shares it and the rack can be reopened/edited; <paramref name="rackName"/> names it.
        /// </summary>
        public HeaderPlacementResult DrawAndPlace(
            Document document,
            DynamicRackSystem system,
            string payloadJson = null,
            string rackName = null,
            int postIndex = -1)
            => ViewBlockDraw.DrawAndPlace(
                document,
                system != null,
                "No hay sistema para dibujar.",
                drawer,
                catalog => postIndex >= 0 ? builder.Build(system, catalog, postIndex) : builder.Build(system, catalog),
                () => BlockName(system, rackName),
                payloadJson);

        /// <summary>Redraw an existing dynamic system's block DEFINITION in place; every copy updates on regen.</summary>
        public HeaderPlacementResult RedrawInPlace(
            Document document,
            ObjectId blockId,
            DynamicRackSystem system,
            string payloadJson,
            bool regen = true,
            int postIndex = -1)
            => ViewBlockDraw.RedrawInPlace(
                document,
                blockId,
                system != null && !blockId.IsNull,
                "No hay sistema para actualizar.",
                drawer,
                catalog => postIndex >= 0 ? builder.Build(system, catalog, postIndex) : builder.Build(system, catalog),
                payloadJson,
                regen);

        internal PreparedViewRedraw PrepareRedraw(Database database, ObjectId blockId, DynamicRackSystem system, string payloadJson, int postIndex = -1)
            => ViewBlockDraw.PrepareRedraw(database, blockId, system != null && !blockId.IsNull,
                "No hay sistema para actualizar.", drawer,
                catalog => postIndex >= 0 ? builder.Build(system, catalog, postIndex) : builder.Build(system, catalog), payloadJson);

        internal LateralHeaderDrawOutcome RedrawInTransaction(Database database, Transaction transaction,
            PreparedViewRedraw prepared, out IReadOnlyCollection<ObjectId> staleDefinitions)
            => ViewBlockDraw.RedrawInTransaction(database, transaction, prepared, out staleDefinitions);

        private static string BlockName(DynamicRackSystem system, string rackName)
            => RackViewBaseName.DynamicLateral(system, rackName);
    }
}
