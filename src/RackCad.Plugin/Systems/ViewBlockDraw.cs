using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Plugin.Headers;

namespace RackCad.Plugin.Systems
{
    /// <summary>
    /// The shared orchestration behind the seven "view block" draw services (selective frontal/planta, dynamic
    /// lateral/frontal/planta, cama, cabecera planta). It owns ONLY the common flow — validate args, load the
    /// catalog via <see cref="RackCatalogLoader"/>, build a <see cref="DynamicSystemPlan"/> from a per-view
    /// factory, create the block via <see cref="SystemBlockWriter"/> and place it via <see cref="BlockPlacement"/>
    /// (draw) or redefine it via <see cref="SystemBlockWriter.RedrawInPlace"/> (redraw). Everything that varies
    /// per view is supplied by the concrete facade as a delegate/value: the null check, the "nothing to draw"
    /// message, the plan factory (which closes over the payload and any specialization — postIndex, DynamicRackEnd,
    /// the all-loose wrap), the block name and the regen flag. Non-generic on purpose: the payload type never
    /// appears here, so there is no reason for a type parameter. <see cref="LateralHeaderDrawService"/> keeps its
    /// own richer flow (extraInstances merge, DrawAt, outcome-count rebuild) and is NOT routed through here.
    /// </summary>
    internal static class ViewBlockDraw
    {
        /// <summary>Draw a fresh view block and jig-place it. <paramref name="plan"/> and <paramref name="blockName"/>
        /// are evaluated INSIDE the try (same as before) so a build error still becomes a Failure result, not a throw.</summary>
        internal static HeaderPlacementResult DrawAndPlace(
            Document document,
            bool hasPayload,
            string emptyMessage,
            LateralHeaderDrawer drawer,
            Func<RackCatalog, DynamicSystemPlan> plan,
            Func<string> blockName,
            string payloadJson)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (!hasPayload)
            {
                return HeaderPlacementResult.Failure(emptyMessage);
            }

            try
            {
                var catalog = RackCatalogLoader.Load();
                var block = SystemBlockWriter.CreateBlock(document, drawer, plan(catalog), blockName(), payloadJson);
                return BlockPlacement.PlaceAndReport(document, catalog, block);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }

        /// <summary>Redefine an existing view block's DEFINITION in place. <paramref name="hasTarget"/> is the facade's
        /// "payload present and blockId not null" check; <paramref name="regen"/> is forwarded to
        /// <see cref="SystemBlockWriter"/> verbatim — F3 does NOT uniform regen (that is F4).</summary>
        internal static HeaderPlacementResult RedrawInPlace(
            Document document,
            ObjectId blockId,
            bool hasTarget,
            string emptyMessage,
            LateralHeaderDrawer drawer,
            Func<RackCatalog, DynamicSystemPlan> plan,
            string payloadJson,
            bool regen)
        {
            if (document == null)
            {
                return HeaderPlacementResult.Failure("No hay un dibujo activo en AutoCAD.");
            }

            if (!hasTarget)
            {
                return HeaderPlacementResult.Failure(emptyMessage);
            }

            try
            {
                var catalog = RackCatalogLoader.Load();
                return SystemBlockWriter.RedrawInPlace(document, drawer, blockId, plan(catalog), payloadJson, catalog, regen);
            }
            catch (Exception ex)
            {
                return HeaderPlacementResult.Failure(ex.Message);
            }
        }
    }
}
