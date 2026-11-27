using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Shared;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Views; // T7 cab: the queue of ID18 of «Insertar vista…»

namespace RackCad.Plugin
{
    /// <summary>
    /// T4 cb (I-64, fourth batch): the drawing half of «Actualizar» of RACKEDITAR on a cabecera (the branch <c>UpdateOnly</c> of
    /// <see cref="EditCabecera"/>), for RACKCABECERAAPLICAR of the door of the window. It is a COPY of that branch, not an extraction, so
    /// RACKEDITAR does not change in this batch; a guard of source in Core fixes that both run the same steps in the same order (the views
    /// of the identity split into lateral and planta, the preflight of every inner design, the redraw in place of each one with the payload
    /// of its own envelope, the name of each block, and ONE regeneration).
    /// </summary>
    public sealed partial class RackCabeceraCommands
    {
        /// <summary>Redraws in place every view of the cabecera <paramref name="id"/> with <paramref name="config"/>; how many it redrew (and erased: none).</summary>
        internal static (int Redrawn, int Erased) ApplyCabeceraUpdate(Document document, RackProject project, RackFrameConfiguration config, string id, string name)
        {
            var editor = document.Editor;
            var baseName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();

            // Keep each block's own Embed (not just its id) so its per-view unknown metadata is preserved on redraw (I-11).
            var blocks = RackCommandSupport.FindRackBlocks(document, id);
            var lateralBlocks = blocks.Where(b => !RackCommandSupport.IsPlantaView(b.Embed)).ToList();
            var plantaBlocks = blocks.Where(b => RackCommandSupport.IsPlantaView(b.Embed)).ToList();

            // PREFLIGHT every linked view's inner design BEFORE touching any geometry (I-11), as RACKEDITAR.
            var preflight = RackCommandSupport.PreflightInnerSources(lateralBlocks.Concat(plantaBlocks).ToList(), RackSystemKind.Selective, project);
            if (preflight.Aborted)
            {
                editor.WriteMessage("\nRackCad: " + preflight.ErrorMessage);
                return (0, 0);
            }

            // Redraw with regen:false and regenerate ONCE below.
            var updated = 0;
            foreach (var lat in lateralBlocks)
            {
                var r = new LateralHeaderDrawService().RedrawInPlace(
                    document, lat.BlockId, config, BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewLateral, lat.Embed, preflight.ResolvedByBlock[lat.BlockId]), regen: false);
                if (r != null && r.Success)
                {
                    RackBlockRenamer.SyncName(document, lat.BlockId, RackViewBaseName.LinkedBase(baseName));
                    updated++;
                }
            }

            foreach (var pb in plantaBlocks)
            {
                var r = new PlantaHeaderDrawService().RedrawInPlace(
                    document, pb.BlockId, config, BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewPlanta, pb.Embed, preflight.ResolvedByBlock[pb.BlockId]), regen: false);
                if (r != null && r.Success)
                {
                    RackBlockRenamer.SyncName(document, pb.BlockId, RackViewBaseName.LinkedPlanta(baseName));
                    updated++;
                }
            }

            if (updated > 0)
            {
                document.Editor.Regen(); // ONE regeneration refreshes every redefined view-block
            }

            editor.WriteMessage(updated > 0
                ? "\nRackCad: cabecera actualizada; sus vistas (lateral/planta) se redibujaron."
                : "\nRackCad: no se pudo actualizar la cabecera.");
            return (updated, 0);
        }

        /// <summary>
        /// T7 cab (I-64, séptima tanda; the Owner on 2026-11-27: «Las dos»): the drawing half of «Insertar» of RACKEDITAR on a cabecera (the
        /// branch <c>!window.UpdateOnly</c> of <see cref="EditCabecera"/>), for RACKCABECERAINSERTAR («Insertar vista…» of the window), with
        /// the cabecera of the window as its input instead of the classic window. It is a COPY of that branch, as <see cref="ApplyCabeceraUpdate"/>
        /// is of the other one (CB4-09), so RACKEDITAR does not change; a guard of source in Core ties the two, step by step:
        /// <list type="bullet">
        /// <item>the authorized batch of AUTH-13 gives the views of the identity; the preflight of every inner design runs BEFORE any geometry;</item>
        /// <item>I-05 warns once, before the first redraw;</item>
        /// <item>B-1: the queue of ID18 prepares the new views from the EDITED source (<see cref="RackViewBatchProducts.Header"/> with the
        /// envelope and the project of the picked view), before the first redraw too;</item>
        /// <item>every view is redrawn in place with the payload of its own envelope and renamed, with ONE regeneration;</item>
        /// <item>the rule of the incomplete redraw (T6 b1, <c>RackInsertRedrawOutcome</c>): the queue places the new views only when every
        /// view of the cabecera was redrawn (a cabecera has no obsolete views).</item>
        /// </list>
        /// It answers how many views it redrew and erased (none): zero and zero when it stopped before any geometry.
        /// </summary>
        internal static (int Redrawn, int Erased) ApplyCabeceraInsert(Document document, ObjectId blockId, RackEmbedDocument embed, RackProject project,
            RackFrameConfiguration config, string rackId, string name, IReadOnlyList<RackViewAddress> insertViews)
        {
            var editor = document.Editor;
            if (config == null || insertViews == null || insertViews.Count == 0)
            {
                editor.WriteMessage("\nRackCad: la ventana RackCad no entrego una cabecera y sus vistas; no se modifico nada.");
                return (0, 0);
            }

            var baseName = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
            // Identity: keep the GUID of the picked envelope; the one of the request is only a fallback. NEVER mint a new GUID on an edit.
            var id = string.IsNullOrWhiteSpace(embed.Id) ? rackId : embed.Id;
            if (!RackUnsupportedSiblingInsert.TryAuthorize(document, blockId, embed, id, out var authorized)) return (0, 0);

            // Keep each block's own Embed (not just its id) so its per-view unknown metadata is preserved on redraw (I-11).
            var blocks = authorized.Blocks.ToList();
            var lateralBlocks = blocks.Where(b => !RackCommandSupport.IsPlantaView(b.Embed)).ToList();
            var plantaBlocks = blocks.Where(b => RackCommandSupport.IsPlantaView(b.Embed)).ToList();

            // Make sure the picked block is handled even if the GUID scan missed it.
            if (lateralBlocks.Count == 0 && plantaBlocks.All(b => b.BlockId != blockId) && !RackCommandSupport.IsPlantaView(embed))
            {
                lateralBlocks.Add((blockId, embed));
            }

            // PREFLIGHT every linked view's inner design BEFORE touching any geometry (I-11), as RACKEDITAR.
            var preflight = RackCommandSupport.PreflightInnerSources(
                lateralBlocks.Concat(plantaBlocks).ToList(), RackSystemKind.Selective, project);
            if (preflight.Aborted)
            {
                editor.WriteMessage("\nRackCad: " + preflight.ErrorMessage);
                return (0, 0);
            }

            // I-05: a NEW linked view is inserted below; warn once if the drawing is not in inches, BEFORE any block is (re)drawn.
            RackUnitsGuard.WarnIfNotInches(document);

            // B-1: the new views take the cabecera of the window, its identity and its name, as the views redrawn below.
            var batchProducts = RackViewBatchProducts.Header(
                document, config, id, name, embed, project, authorized.AuthoredInput);
            var batchRequest = RackViewBatchProductSession<RackFrameConfiguration, RackFrameConfiguration,
                RackCad.Application.Drawing.HeaderRunPlan>.Request(
                    RackCad.Application.Views.Preparation.RackProductSourceKind.ExistingRack,
                    RackSystemKind.Selective, id, insertViews);
            if (!batchProducts.PrepareAll(batchRequest, out var batchDiagnostic))
            {
                editor.WriteMessage("\nRackCad: no se preparo la cola ID18; no se modifico ningun bloque. " + batchDiagnostic);
                return (0, 0);
            }

            // Redraw with regen:false and regenerate ONCE below.
            var updated = 0;
            foreach (var lat in lateralBlocks)
            {
                var r = new LateralHeaderDrawService().RedrawInPlace(
                    document, lat.BlockId, config, BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewLateral, lat.Embed, preflight.ResolvedByBlock[lat.BlockId]), regen: false);
                if (r != null && r.Success)
                {
                    RackBlockRenamer.SyncName(document, lat.BlockId, RackViewBaseName.LinkedBase(baseName));
                    updated++;
                }
            }

            foreach (var pb in plantaBlocks)
            {
                var r = new PlantaHeaderDrawService().RedrawInPlace(
                    document, pb.BlockId, config, BuildCabeceraPayload(config, id, name, RackEmbedDocument.ViewPlanta, pb.Embed, preflight.ResolvedByBlock[pb.BlockId]), regen: false);
                if (r != null && r.Success)
                {
                    RackBlockRenamer.SyncName(document, pb.BlockId, RackViewBaseName.LinkedPlanta(baseName));
                    updated++;
                }
            }

            if (updated > 0)
            {
                document.Editor.Regen(); // ONE regeneration refreshes every redefined view-block
            }

            // T6 b1 (I-02 of the review of B-1): the queue places the new view only when every view of the cabecera was redrawn (a
            // cabecera has no obsolete views).
            RackViewBatchExecution.Run(document, batchProducts, batchRequest,
                _ => RackCad.Application.Views.Batch.RackInsertRedrawOutcome.Of(lateralBlocks.Count + plantaBlocks.Count, 0, updated, 0));
            return (updated, 0);
        }
    }
}
