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
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                var menu = new RackMainMenuWindow(
                    canInsertInAutoCad: true,
                    dimensionStyles: RackCommandSupport.ReadDimensionStyleNames(document));
                AcApplication.ShowModalWindow(menu);

                if (menu.InsertRequested)
                {
                    if (menu.ConfigurationToInsert != null)
                    {
                        RackCabeceraCommands.DrawAndPlace(menu.ConfigurationToInsert);
                    }
                    else if (menu.DynamicSystemToInsert != null)
                    {
                        RackDinamicoCommands.DrawDynamicView(
                            menu.DynamicView,
                            menu.DynamicSection,
                            menu.DynamicSystemToInsert,
                            menu.DynamicDesignToInsert,
                            menu.DynamicRackId,
                            menu.DynamicRackName);
                    }
                    else if (menu.FlowBedToInsert != null)
                    {
                        RackCamaCommands.DrawAndPlaceBed(menu.FlowBedToInsert, RackCamaCommands.BuildCamaPayload(menu.FlowBedToInsert, menu.FlowBedRackId, menu.FlowBedRackName), menu.FlowBedRackName);
                    }
                    else if (menu.SelectiveSystemToInsert != null)
                    {
                        RackSelectivoCommands.DrawSelectiveView(menu.SelectiveView, menu.SelectiveSystemToInsert, menu.SelectiveDesignToInsert, menu.SelectiveRackId, menu.SelectiveRackName);
                    }
                }
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
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
                if (!RackCommandSupport.PickRackBlock(document, "\nSelecciona un rack para editar: ", out var embed, out var blockId))
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
                        RackSelectivoCommands.EditSelective(document, blockId, embed);
                        break;
                    case RackEmbedDocument.KindDynamic:
                        RackDinamicoCommands.EditDynamic(document, blockId, embed);
                        break;
                    case RackEmbedDocument.KindCabecera:
                        RackCabeceraCommands.EditCabecera(document, blockId, embed);
                        break;
                    case RackEmbedDocument.KindCama:
                        RackCamaCommands.EditCama(document, blockId, embed);
                        break;
                    default:
                        editor.WriteMessage("\nRackCad: tipo de rack no reconocido (" + embed.Kind + ").");
                        break;
                }
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }
    }
}
