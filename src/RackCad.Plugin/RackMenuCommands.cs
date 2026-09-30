using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Application.Views.Preparation;
using RackCad.Application.Views.Policy;
using RackCad.Plugin.KindHandlers;
using RackCad.Plugin.Views;
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

                // A structural section is NOT a rack: no system kind, no design payload, no round-trip. It
                // travels as a typed ACTION rather than as an InsertionRequest, and it runs the very same flow
                // RACKSECCION runs — the menu adds a door, not a second generator. Dispatched here, after the
                // modal window closed, because the flow asks for an insertion point and needs the editor free.
                if (menu.RequestedAction == MainMenuAction.GenerateStructuralSection)
                {
                    StructuralSectionCommandFlow.Run(document);
                    return;
                }

                // I-05: an insertion is about to happen (any non-null request). Warn once if the drawing is not in
                // inches, before dispatching to the per-system draw calls (before the first DWG modification).
                // The structural-section path above is NOT covered here on purpose: its own flow warns at the
                // right moment — after the inspector confirms — and warning twice would be noise.
                if (menu.InsertionRequest != null)
                {
                    RackUnitsGuard.WarnIfNotInches(document);
                }

                // The menu now carries ONE typed payload (I-15); dispatch it by kind to the SAME per-system draw calls
                // with the SAME arguments as before (behavior-identical). A cancelled menu leaves InsertionRequest null.
                switch (menu.InsertionRequest)
                {
                    case HeaderInsertionRequest header:
                        var headerProducts = RackViewBatchProducts.Header(document, header.Configuration,
                            header.RackId, header.Configuration?.Name, null, header.SourceProject, null);
                        RackViewBatchExecution.Run(document, headerProducts,
                            RackViewBatchProductSession<RackCad.Domain.RackFrames.RackFrameConfiguration,
                                RackCad.Domain.RackFrames.RackFrameConfiguration,
                                RackCad.Application.Drawing.HeaderRunPlan>.Request(
                                    RackProductSourceKind.NewRack, header.Kind, header.RackId, header.Views));
                        break;
                    case DynamicInsertionRequest dynamic:
                        var dynamicProducts = RackViewBatchProducts.Dynamic(document, dynamic.System, dynamic.Design,
                            dynamic.RackId, dynamic.RackName, null, dynamic.SourceProject, null);
                        RackViewBatchExecution.Run(document, dynamicProducts,
                            RackViewBatchProductSession<RackCad.Domain.Systems.Dynamic.DynamicRackDesign,
                                RackCad.Domain.Systems.Dynamic.DynamicRackSystem,
                                RackCad.Application.Drawing.HeaderRunPlan>.Request(
                                    RackProductSourceKind.NewRack, dynamic.Kind, dynamic.RackId, dynamic.Views));
                        break;
                    case FlowBedInsertionRequest cama:
                        RackCamaCommands.DrawAndPlaceBed(
                            cama.FlowBed,
                            RackCamaCommands.BuildCamaPayload(cama.FlowBed, cama.RackId, cama.RackName, null, cama.SourceDocument),
                            cama.RackName);
                        break;
                    case SelectiveInsertionRequest selective:
                        var selectiveProducts = RackViewBatchProducts.SelectiveNew(document, selective.System,
                            selective.Design, selective.RackId, selective.RackName);
                        RackViewBatchExecution.Run(document, selectiveProducts,
                            RackViewBatchProductSession<RackCad.Application.Persistence.SelectivePalletDesignDocument,
                                RackCad.Domain.Systems.Selective.SelectiveRackSystem,
                                RackCad.Application.Drawing.HeaderRunPlan>.Request(
                                    RackProductSourceKind.NewRack, selective.Kind, selective.RackId, selective.Views));
                        break;
                    // The component case comes FIRST: it is a loose piece, not a line, and it writes no envelope.
                    case CantileverComponentInsertionRequest component:
                        RackCantileverCommands.DrawCantileverComponent(component);
                        break;
                    case CantileverInsertionRequest cantilever:
                        if (!RackCantileverCommands.TryGeometryFactory(document.Editor, out var geometry)) break;
                        var cantileverProducts = RackViewBatchProducts.Cantilever(document, cantilever.Line,
                            cantilever.Design, geometry, cantilever.RackId, cantilever.RackName,
                            null, cantilever.SourceProject, null);
                        RackViewBatchExecution.Run(document, cantileverProducts,
                            RackViewBatchProductSession<RackCad.Domain.Systems.Cantilever.CantileverLineDesign,
                                RackCad.Application.Systems.Cantilever.CantileverLineAssembly,
                                RackCad.Application.Systems.Cantilever.CantileverViewPlan>.Request(
                                    RackProductSourceKind.NewRack, cantilever.Kind, cantilever.RackId, cantilever.Views));
                        break;
                    case PushBackInsertionRequest pushBack:
                        var pushBackProducts = RackViewBatchProducts.PushBack(document, pushBack.System, pushBack.Design,
                            pushBack.RackId, pushBack.RackName, null, pushBack.SourceProject, null);
                        RackViewBatchExecution.Run(document, pushBackProducts,
                            RackViewBatchProductSession<RackCad.Domain.Systems.PushBack.PushBackDesign,
                                RackCad.Domain.Systems.PushBack.PushBackSystem,
                                RackCad.Application.Drawing.HeaderRunPlan>.Request(
                                    RackProductSourceKind.NewRack, pushBack.Kind, pushBack.RackId, pushBack.Views));
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
                // dynamic, push back, cabecera and cama; a kind with no registered handler surfaces the historic
                // visible error (the five embedded kinds are always registered, so real data never hits it).
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
