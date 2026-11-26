using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Shared;
using RackCad.Plugin.Drawing;

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
    }
}
