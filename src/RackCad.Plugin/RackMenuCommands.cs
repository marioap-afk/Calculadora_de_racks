using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Plugin.KindHandlers;
using RackCad.UI;
using RackCad.UI.Editor;
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

                // I-05: an insertion is about to happen (any non-null request). Warn once if the drawing is not in
                // inches, before dispatching to the per-system draw calls (before the first DWG modification).
                if (menu.InsertionRequest != null)
                {
                    RackUnitsGuard.WarnIfNotInches(document);
                }

                // The menu now carries ONE typed payload (I-15); dispatch it by kind to the SAME per-system draw calls
                // with the SAME arguments as before (behavior-identical). A cancelled menu leaves InsertionRequest null.
                switch (menu.InsertionRequest)
                {
                    case HeaderInsertionRequest header:
                        // Transport-only (I-11): carry the library source metadata into the new DWG embed. No handler change.
                        RackCabeceraCommands.DrawAndPlace(header.Configuration, header.SourceProject);
                        break;
                    case DynamicInsertionRequest dynamic:
                        RackDinamicoCommands.DrawDynamicView(
                            dynamic.View,
                            dynamic.Section,
                            dynamic.System,
                            dynamic.Design,
                            dynamic.RackId,
                            dynamic.RackName,
                            source: null,
                            innerSource: dynamic.SourceProject);
                        break;
                    case FlowBedInsertionRequest cama:
                        RackCamaCommands.DrawAndPlaceBed(
                            cama.FlowBed,
                            RackCamaCommands.BuildCamaPayload(cama.FlowBed, cama.RackId, cama.RackName, null, cama.SourceDocument),
                            cama.RackName);
                        break;
                    case SelectiveInsertionRequest selective:
                        RackSelectivoCommands.DrawSelectiveView(
                            selective.View, selective.System, selective.Design, selective.RackId, selective.RackName);
                        break;
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
