using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.UI.Systems.Cantilever;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T5 ct (I-64, fifth batch; prototype «cantilever»): the four commands of the door of a Cantilever line, as those of Dinámico, Selectivo and
    /// Cabecera. Each takes the request that waits in the door (<see cref="RackCadRackDoorPort"/>) and answers with its number; without a
    /// request, or for a request of another drawing, it writes nothing.
    /// <list type="bullet">
    /// <item>RACKCANTILEVERABRIR only reads (no undo marker, no history): the design of the line and the properties of the rack.</item>
    /// <item>RACKCANTILEVERAPLICAR redraws every view of the line in place with the drawing half of «Actualizar» of RACKEDITAR
    /// (<see cref="RackCantileverCommands.ApplyCantileverEdit"/>), then writes the tab «Propiedades»: one «U» undoes it.</item>
    /// <item>RACKCANTILEVERINSERTAR is «Insertar» of RACKEDITAR: the draft in the views that exist, then the new views placed as linked
    /// copies (decision 10 of Dinámico).</item>
    /// <item>RACKCANTILEVERNUEVO inserts a new line of the window by the dispatch of «Nuevo», with the identity the window gave it and the
    /// automatic name of I-60.</item>
    /// </list>
    /// The three that read or write a line of the drawing compare its views first (M-02 of Selectivo, <see cref="RackSiblingDesigns"/>).
    /// </summary>
    public sealed class RackCadCantileverDoorCommands
    {
        private const string OtherDrawingText = "La orden corrió en otro dibujo que el de la línea: no se hizo nada. Vuelve al dibujo de la línea e inténtalo de nuevo.";

        [CommandMethod("RACKCANTILEVERABRIR", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]
        public void ReadCantilever()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingCantileverRead();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKCANTILEVERABRIR la usa la ventana RackCad: no hay ninguna lectura pendiente.");
                return;
            }

            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerCantileverRead(CantileverDesignRead.Refused(pending.RequestId, pending.RackId, null, null, OtherDrawingText));
                    return;
                }

                var blocks = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                if (blocks.Count == 0)
                {
                    door?.AnswerCantileverRead(CantileverDesignRead.Refused(pending.RequestId, pending.RackId, null, null, "La línea no está en el dibujo."));
                    return;
                }

                // M-02 of the review of Selectivo: the window opens the design of one view, so the views must carry the same one.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerCantileverRead(CantileverDesignRead.Refused(pending.RequestId, pending.RackId, null, null, siblings.Reason));
                    return;
                }

                var embed = blocks[0].Embed;
                var answer = CantileverDoorRules.AnswerForOpen(pending.RequestId, pending.RackId, embed?.Kind, embed?.Name, embed?.Design);
                if (answer.Opens) answer = answer with { Properties = RackPropertiesDoorData.Read(document, blocks[0].BlockId) }; // the tab «Propiedades» (VA-07)
                door?.AnswerCantileverRead(answer);
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadCantileverDoorCommands.ReadCantilever", ex);
                door?.AnswerCantileverRead(CantileverDesignRead.Refused(pending.RequestId, pending.RackId, null, null, ex.Message));
            }
        }

        [CommandMethod("RACKCANTILEVERAPLICAR")]
        public void WriteCantilever()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingCantileverWrite();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKCANTILEVERAPLICAR la usa la ventana RackCad: no hay ninguna línea pendiente de escribir.");
                return;
            }

            door?.Started(pending.RequestId);

            try
            {
                if (!TryLine(document, pending, door, out var blocks, out var plan)) return;
                var write = pending.Write;
                var (blockId, embed) = blocks[0];

                // «Written» only when a view was redrawn: the drawing half can stop before any geometry, or redraw nothing (I-08).
                var (redrawn, erased) = RackCantileverCommands.ApplyCantileverEdit(
                    document, blockId, embed, plan.Project, new CantileverDraftOutcome(write.RackId, write.Name, write.Design, write.Line));
                door?.AnswerWrite(RackPropertiesDoorData.WithRackProperties(document, pending.RackId, write.Properties,
                    RackDoorRules.AnswerForApply(pending.RequestId, pending.RackId, redrawn, erased))); // and its properties (VA-07)
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo escribir: " + ex.Message));
            }
        }

        /// <summary>
        /// «Insertar vista…» of a line of the window: the «Insertar» drawing half of RACKEDITAR (the branch <c>!UpdateOnly</c>), with the views
        /// the line can draw. The answer counts the views of the identity before and after, so «inserted» means placed.
        /// </summary>
        [CommandMethod("RACKCANTILEVERINSERTAR")]
        public void InsertCantileverViews()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingCantileverInsert();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKCANTILEVERINSERTAR la usa la ventana RackCad: no hay ninguna vista pendiente de insertar.");
                return;
            }

            door?.Started(pending.RequestId);

            try
            {
                if (!TryLine(document, pending, door, out var blocks, out var plan)) return;
                var write = pending.Write;
                var views = CantileverDoorRules.DrawableViews(write.InsertViews, write.Line);
                if (views.Count == 0)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false,
                        "Ninguna de las vistas elegidas se puede dibujar con el borrador: no se escribió nada."));
                    return;
                }

                var (blockId, embed) = blocks[0];
                var (redrawn, erased) = RackCantileverCommands.ApplyCantileverEdit(document, blockId, embed, plan.Project,
                    CantileverDraftOutcome.Inserting(write.RackId, write.Name, write.Design, write.Line, views));
                var inserted = System.Math.Max(0, RackCommandSupport.FindRackBlocks(document, pending.RackId).Count - (blocks.Count - erased));
                door?.AnswerWrite(RackDoorRules.AnswerForInsert(pending.RequestId, pending.RackId, redrawn, erased, inserted));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo insertar: " + ex.Message));
            }
        }

        /// <summary>
        /// A new line made in the window: the insertion of a new rack, by the same dispatch as the classic editor opened from «Nuevo»
        /// (<see cref="RackMenuCommands.DispatchInsertion"/>), with the identity the window gave it and the automatic name of I-60 when it has none.
        /// The answer counts the views of that identity that were placed, and gives the name the drawing holds.
        /// </summary>
        [CommandMethod("RACKCANTILEVERNUEVO")]
        public void InsertNewCantilever()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingCantileverNew();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKCANTILEVERNUEVO la usa la ventana RackCad: no hay ninguna línea nueva pendiente de insertar.");
                return;
            }

            door?.Started(pending.RequestId);

            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, OtherDrawingText));
                    return;
                }

                if (RackCommandSupport.FindRackBlocks(document, pending.RackId).Count > 0)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "Ya hay una línea con esa identidad en el dibujo: no se insertó nada."));
                    return;
                }

                var write = pending.Write;
                var views = CantileverDoorRules.DrawableViews(write.InsertViews, write.Line);
                if (views.Count == 0)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false,
                        "Ninguna de las vistas elegidas se puede dibujar con el borrador: no se insertó nada."));
                    return;
                }

                RackMenuCommands.DispatchInsertion(document, CantileverDraftOutcome.NewRackRequest(write.RackId, write.Name, write.Design, write.Line, views));
                var placed = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                door?.AnswerWrite(RackPropertiesDoorData.WithNewRackProperties(document, pending.RackId, write.Properties,
                    RackDoorRules.AnswerForNewRack(pending.RequestId, pending.RackId, placed.Count, placed.Count > 0 ? placed[0].Embed?.Name : null))); // T6 pr: and its properties (VA-S9)
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo insertar la línea nueva: " + ex.Message));
            }
        }

        // The checks of a write on a line of the drawing: the drawing of the request, the line still there, its views coinciding and the
        // rule of the door (a readable Cantilever line). False, with the answer given, when nothing is written.
        private static bool TryLine(Autodesk.AutoCAD.ApplicationServices.Document document, RackCadRackDoorPort.PendingCantileverWrite pending,
            RackCadRackDoorPort door,
            out System.Collections.Generic.List<(Autodesk.AutoCAD.DatabaseServices.ObjectId BlockId, RackEmbedDocument Embed)> blocks,
            out CantileverApplyPlan plan)
        {
            plan = null;
            blocks = null;
            if (pending.Document != RackCadRackDoorPort.KeyOf(document))
            {
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, OtherDrawingText));
                return false;
            }

            blocks = RackCommandSupport.FindRackBlocks(document, pending.RackId);
            if (blocks.Count == 0)
            {
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "La línea ya no está en el dibujo: no se escribió nada."));
                return false;
            }

            // M-02 of the review of Selectivo: the draft is written in every view, so they must carry the same design.
            var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
            if (!siblings.Coincide)
            {
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, siblings.Reason));
                return false;
            }

            var embed = blocks[0].Embed;
            plan = CantileverDoorRules.PlanApply(embed?.Kind, embed?.Design, pending.Write);
            if (!plan.CanWrite)
            {
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, plan.Reason));
                return false;
            }

            return true;
        }

        // The views of the rack for RackSiblingDesigns: each block definition by its handle, with its envelope.
        private static System.Collections.Generic.List<RackSiblingView> Views(
            System.Collections.Generic.List<(Autodesk.AutoCAD.DatabaseServices.ObjectId BlockId, RackEmbedDocument Embed)> blocks)
        {
            var views = new System.Collections.Generic.List<RackSiblingView>(blocks.Count);
            foreach (var (blockId, embed) in blocks) views.Add(new RackSiblingView(blockId.Handle.ToString(), embed));
            return views;
        }
    }
}
