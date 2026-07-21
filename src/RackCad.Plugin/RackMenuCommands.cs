using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Plugin.KindHandlers;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// The RackCad menu (RACKCAD) and round-trip editor (RACKEDITAR) commands, plus their aliases. Each rack type's
    /// commands live in its own RackXCommands class; the menu and RACKEDITAR dispatch into those classes' internal
    /// static draw/edit entry points, and the cross-type helpers live in <see cref="RackCommandSupport"/>.
    /// </summary>
    public sealed class RackMenuCommands
    {
        [CommandMethod("RK")]  public void AliasRackCad() => RackCad();                    // menú principal
        [CommandMethod("RED")] public void AliasRackEditar() => RackEditar();              // RACKEDITAR

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
                        // Transport-only (I-11): carry the library source metadata into the new DWG embed. No handler change.
                        RackCabeceraCommands.DrawAndPlace(menu.ConfigurationToInsert, menu.ConfigurationSourceProjectToInsert);
                    }
                    else if (menu.DynamicSystemToInsert != null)
                    {
                        RackDinamicoCommands.DrawDynamicView(
                            menu.DynamicView,
                            menu.DynamicSection,
                            menu.DynamicSystemToInsert,
                            menu.DynamicDesignToInsert,
                            menu.DynamicRackId,
                            menu.DynamicRackName,
                            source: null,
                            innerSource: menu.DynamicSourceProjectToInsert);
                    }
                    else if (menu.FlowBedToInsert != null)
                    {
                        RackCamaCommands.DrawAndPlaceBed(
                            menu.FlowBedToInsert,
                            RackCamaCommands.BuildCamaPayload(menu.FlowBedToInsert, menu.FlowBedRackId, menu.FlowBedRackName, null, menu.FlowBedSourceDocumentToInsert),
                            menu.FlowBedRackName);
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

                // Dispatch by rack type via the Plugin's kind-handler seam. The same round-trip serves selective,
                // dynamic, cabecera and cama; a kind with no registered handler surfaces the historic visible error
                // (the four embedded kinds are always registered, so real data never hits it).
                if (!KindHandlerDispatch.TryResolve(editor, embed.Kind, out var handler))
                {
                    return;
                }

                handler.Edit(document, blockId, embed);
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }
    }
}
