using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.FlowBed;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.FlowBed;

namespace RackCad.Plugin
{
    /// <summary>
    /// T5 cm (I-64, quinta tanda): the drawing half of «Actualizar» of RACKEDITAR on a cama (what <see cref="EditCama"/> does once its window
    /// closed with «Actualizar en AutoCAD»), for RACKCAMAAPLICAR of the door of the window. It is a COPY of those steps, not an extraction, so
    /// RACKEDITAR does not change in this batch (and its guard of I-61 keeps reading <see cref="EditCama"/> as it is); a guard of source in
    /// Core fixes that both run the same steps in the same order: the name resolved once (blank = the one of the envelope, I-61), the redraw
    /// of the block DEFINITION in place with the payload that carries the envelope and the source document (I-11), the name of the block
    /// only when it was redrawn, and the same line in the command line.
    /// </summary>
    public sealed partial class RackCamaCommands
    {
        /// <summary>Redraws in place the block of the cama <paramref name="rackId"/> with <paramref name="config"/>; every copy changes with it.</summary>
        internal static HeaderPlacementResult ApplyCamaUpdate(Document document, ObjectId blockId, RackEmbedDocument embed, FlowBedDocument sourceDesign,
            FlowBedConfiguration config, string rackId, string name)
        {
            var editor = document.Editor;

            // I-61 (D-1a): blank name -> the envelope's, like the other edit routes.
            var resolvedName = EditedRackNameResolver.Resolve(name, embed.Name);
            var result = new FlowBedDrawService().RedrawInPlace(
                document, blockId, config,
                BuildCamaPayload(config, rackId, resolvedName, embed, sourceDesign));

            if (result != null && result.Success)
            {
                RackBlockRenamer.SyncName(document, blockId, RackViewBaseName.LinkedBase(resolvedName));
            }

            editor.WriteMessage(result != null && result.Success
                ? "\nRackCad: cama actualizada; todas sus copias reflejan el cambio."
                : "\nRackCad: no se pudo actualizar la cama. " + (result?.ErrorMessage ?? string.Empty));
            return result;
        }
    }
}
