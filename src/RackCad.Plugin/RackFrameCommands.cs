using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;
using RackCad.Plugin.Headers;
using RackCad.Plugin.Systems;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// AutoCAD command entry points for every rack type. Split into partial files by rack type
    /// (Cabecera / Dynamic / FlowBed / Selective); this file holds the shared surface: the RACKCAD menu
    /// dispatcher, the RACKEDITAR round-trip dispatcher, and the cross-type helpers they share.
    /// </summary>
    public sealed partial class RackFrameCommands
    {
        /// <summary>Main entry point: opens the menu where the user picks what to design.</summary>
        [CommandMethod("RACKCAD")]
        public void RackCad()
        {
            try
            {
                var menu = new RackMainMenuWindow(canInsertInAutoCad: true);
                AcApplication.ShowModalWindow(menu);

                if (menu.InsertRequested)
                {
                    if (menu.ConfigurationToInsert != null)
                    {
                        DrawAndPlace(menu.ConfigurationToInsert);
                    }
                    else if (menu.DynamicSystemToInsert != null)
                    {
                        DrawAndPlaceSystem(menu.DynamicSystemToInsert, BuildDynamicPayload(menu.DynamicSystemToInsert, menu.DynamicRackId, menu.DynamicRackName), menu.DynamicRackName);
                    }
                    else if (menu.FlowBedToInsert != null)
                    {
                        DrawAndPlaceBed(menu.FlowBedToInsert, BuildCamaPayload(menu.FlowBedToInsert, menu.FlowBedRackId, menu.FlowBedRackName), menu.FlowBedRackName);
                    }
                    else if (menu.SelectiveSystemToInsert != null)
                    {
                        DrawSelectiveView(menu.SelectiveView, menu.SelectiveSystemToInsert, menu.SelectiveDesignToInsert, menu.SelectiveRackId, menu.SelectiveRackName);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>Select an already-drawn selective rack, reopen its editor with all its data, and redraw it.</summary>
        [CommandMethod("RACKEDITAR")]
        public void RackEditar()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    return;
                }

                var editor = document.Editor;
                if (!PickRackBlock(document, "\nSelecciona un rack para editar: ", out var embed, out var blockId))
                {
                    return;
                }

                if (embed == null || string.IsNullOrEmpty(embed.Design))
                {
                    editor.WriteMessage("\nRackCad: ese bloque no tiene datos de rack editables.");
                    return;
                }

                // Dispatch by rack type — the same round-trip serves selective, dynamic (and later cabecera/cama).
                switch (embed.Kind)
                {
                    case RackEmbedDocument.KindSelective:
                        EditSelective(document, blockId, embed);
                        break;
                    case RackEmbedDocument.KindDynamic:
                        EditDynamic(document, blockId, embed);
                        break;
                    case RackEmbedDocument.KindCabecera:
                        EditCabecera(document, blockId, embed);
                        break;
                    case RackEmbedDocument.KindCama:
                        EditCama(document, blockId, embed);
                        break;
                    default:
                        editor.WriteMessage("\nRackCad: tipo de rack no reconocido (" + embed.Kind + ").");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>
        /// Duplicate the selected rack as an INDEPENDENT copy: the same design, but a NEW GUID and a "- copia" name,
        /// drawn as its own block via the normal insertion path (jig). Because the copy carries a different GUID,
        /// RACKEDITAR on it reopens ONLY the copy — the original is never touched. Duplicates the clicked block's view.
        /// </summary>
        [CommandMethod("RACKDUPLICAR")]
        public void RackDuplicar()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    return;
                }

                var editor = document.Editor;
                if (!PickRackBlock(document, "\nSelecciona un rack para duplicar: ", out var embed, out _))
                {
                    return;
                }

                if (embed == null || string.IsNullOrEmpty(embed.Design))
                {
                    editor.WriteMessage("\nRackCad: ese bloque no tiene datos de rack para duplicar.");
                    return;
                }

                // A fresh GUID makes the copy an independent rack; the name gets a "- copia" suffix.
                var newId = System.Guid.NewGuid().ToString();
                var newName = CopyName(embed.Name);

                switch (embed.Kind)
                {
                    case RackEmbedDocument.KindSelective:
                        DuplicateSelective(document, embed, newId, newName);
                        break;
                    case RackEmbedDocument.KindDynamic:
                        DuplicateDynamic(document, embed, newId, newName);
                        break;
                    case RackEmbedDocument.KindCabecera:
                        DuplicateCabecera(document, embed, newId, newName);
                        break;
                    case RackEmbedDocument.KindCama:
                        DuplicateCama(document, embed, newId, newName);
                        break;
                    default:
                        editor.WriteMessage("\nRackCad: tipo de rack no reconocido (" + embed.Kind + ").");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>
        /// Prompt the user to pick a rack block and read its embedded payload from the DEFINITION. Returns false only
        /// when the user cancelled the selection; a picked-but-non-rack block returns true with a null <paramref
        /// name="embed"/> so the caller can report it. Shared by RACKEDITAR and RACKDUPLICAR.
        /// </summary>
        private static bool PickRackBlock(Document document, string prompt, out RackEmbedDocument embed, out ObjectId blockId)
        {
            embed = null;
            blockId = ObjectId.Null;

            var options = new PromptEntityOptions(prompt);
            options.SetRejectMessage("\nEse objeto no es un rack.");
            options.AddAllowedClass(typeof(BlockReference), exactMatch: false);

            var selection = document.Editor.GetEntity(options);
            if (selection.Status != PromptStatus.OK)
            {
                return false;
            }

            string json;
            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                var reference = (BlockReference)transaction.GetObject(selection.ObjectId, OpenMode.ForRead);
                blockId = reference.BlockTableRecord;
                json = RackBlockData.Read(transaction, blockId);
                transaction.Commit();
            }

            embed = new RackEmbedStore().Deserialize(json);
            return true;
        }

        /// <summary>The client name for a duplicate: the original plus a "- copia" suffix.</summary>
        private static string CopyName(string name)
        {
            var baseName = string.IsNullOrWhiteSpace(name) ? "Rack" : name.Trim();
            return baseName + " - copia";
        }

        /// <summary>True when a cabecera view-block draws the PLANTA view (so it is the top view, not the lateral).</summary>
        private static bool IsPlantaView(RackEmbedDocument embed) =>
            embed != null && string.Equals(embed.View, RackEmbedDocument.ViewPlanta, System.StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Every rack block DEFINITION in the drawing whose embedded payload has the given rack id — i.e. all the
        /// view-blocks (frontal + lateral sections) of the same rack, so an edit can redraw them together.
        /// </summary>
        private static System.Collections.Generic.List<(ObjectId BlockId, RackEmbedDocument Embed)> FindRackBlocks(Document document, string rackId)
        {
            var results = new System.Collections.Generic.List<(ObjectId, RackEmbedDocument)>();
            if (document == null || string.IsNullOrEmpty(rackId))
            {
                return results;
            }

            var store = new RackEmbedStore();
            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)transaction.GetObject(document.Database.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId id in blockTable)
                {
                    var record = (BlockTableRecord)transaction.GetObject(id, OpenMode.ForRead);
                    if (record.IsLayout || record.IsAnonymous || record.IsFromExternalReference)
                    {
                        continue;
                    }

                    var json = RackBlockData.Read(transaction, id);
                    if (string.IsNullOrEmpty(json))
                    {
                        continue;
                    }

                    var embed = store.Deserialize(json);
                    if (embed != null && string.Equals(embed.Id, rackId, System.StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add((id, embed));
                    }
                }

                transaction.Commit();
            }

            return results;
        }

        private static void Report(System.Exception ex)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            document?.Editor.WriteMessage("\nRackCad error: " + ex.Message);
        }
    }
}
