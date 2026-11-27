using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Views.Placement; // T6 ui (O-3): RackJigPrompt
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T5 pb (I-64, fifth batch): the four commands of the door of a Push Back of the RackCad window, as those of Dinámico and Selectivo. Each
    /// takes the request that waits in the door (<see cref="RackCadRackDoorPort"/>) and answers with its number; without a request, or for a
    /// request of another drawing, it writes nothing.
    /// <list type="bullet">
    /// <item>RACKPUSHBACKABRIR only reads (no undo marker, no history): the design of the first view, after comparing the views of the rack
    /// (M-02 of Selectivo, <see cref="RackSiblingDesigns"/>), and the properties of the rack (VA-07).</item>
    /// <item>RACKPUSHBACKAPLICAR redraws every view of the rack in place with the drawing half of «Actualizar» of RACKEDITAR
    /// (<see cref="RackPushBackWindowDrawing.Apply"/>), so one «U» undoes it; then it writes the tab «Propiedades».</item>
    /// <item>RACKPUSHBACKINSERTAR («Insertar vista…») first writes the draft in the views that exist, then places the new ones, linked.</item>
    /// <item>RACKPUSHBACKNUEVO inserts a new Push Back of the window by the same dispatch as the classic editor opened from «Nuevo», with the
    /// identity the window gave it and the automatic name of I-60 when it has none.</item>
    /// </list>
    /// </summary>
    public sealed class RackCadPushBackDoorCommands
    {
        private const string OtherDrawingText = "La orden corrió en otro dibujo que el del rack: no se hizo nada. Vuelve al dibujo del rack e inténtalo de nuevo.";

        [CommandMethod("RACKPUSHBACKABRIR", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]
        public void ReadPushBack()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingPushBackRead();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKPUSHBACKABRIR la usa la ventana RackCad: no hay ninguna lectura pendiente.");
                return;
            }

            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerPushBackRead(new RackDesignRead(pending.RequestId, pending.RackId, false, null, null, null, OtherDrawingText));
                    return;
                }

                var blocks = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                if (blocks.Count == 0)
                {
                    door?.AnswerPushBackRead(new RackDesignRead(pending.RequestId, pending.RackId, false, null, null, null, "El rack no está en el dibujo."));
                    return;
                }

                // M-02 of the review of Selectivo: the window opens the design of one view, so the views must carry the same one.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerPushBackRead(new RackDesignRead(pending.RequestId, pending.RackId, false, null, null, null, siblings.Reason));
                    return;
                }

                var (blockId, embed) = blocks[0];
                door?.AnswerPushBackRead(PushBackDoorRules.AnswerForOpen(pending.RequestId, pending.RackId, embed?.Kind, embed?.Name, embed?.Design,
                    RackPropertiesDoorData.Read(document, blockId)));
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadPushBackDoorCommands.ReadPushBack", ex);
                door?.AnswerPushBackRead(new RackDesignRead(pending.RequestId, pending.RackId, false, null, null, null, ex.Message));
            }
        }

        [CommandMethod("RACKPUSHBACKAPLICAR")]
        public void WritePushBack() => Write(RackCadRackDoorPort.TakePendingPushBackWrite(), "RACKPUSHBACKAPLICAR", "ningún borrador pendiente de escribir");

        /// <summary>«Insertar vista…» of the window: the draft in the views that exist, then the new views placed as linked copies.</summary>
        [CommandMethod("RACKPUSHBACKINSERTAR")]
        public void InsertPushBackViews()
        {
            using var jigPrompt = RackJigPrompt.Scope(RackJigPrompt.PushBackView); // T6 ui (O-3): the jig names the view of the Push Back
            Write(RackCadRackDoorPort.TakePendingPushBackInsert(), "RACKPUSHBACKINSERTAR", "ninguna vista pendiente de insertar");
        }

        /// <summary>A new Push Back of the window: the insertion of a new rack, by the dispatch of «Nuevo» (RACKNUEVO).</summary>
        [CommandMethod("RACKPUSHBACKNUEVO")]
        public void InsertNewPushBack()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingPushBackNew();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKPUSHBACKNUEVO la usa la ventana RackCad: no hay ningún rack nuevo pendiente de insertar.");
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
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "Ya hay un rack con esa identidad en el dibujo: no se insertó nada."));
                    return;
                }

                var write = pending.Write;
                // T6 ui (O-3): the dispatch of «Nuevo» names the view of the Push Back in the prompt of the jig (RackJigPrompt.ForSystem).
                RackMenuCommands.DispatchInsertion(document,
                    RackCad.UI.VentanaRackCad.PushBackDoorInsertion.NewRackRequest(write.RackId, write.Name, write.Design, write.System, write.InsertViews));
                var placed = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                door?.AnswerWrite(RackDoorRules.AnswerForNewRack(pending.RequestId, pending.RackId, placed.Count, placed.Count > 0 ? placed[0].Embed?.Name : null));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo insertar el rack nuevo: " + ex.Message));
            }
        }

        // RACKPUSHBACKAPLICAR and RACKPUSHBACKINSERTAR: the same checks, then the drawing half with or without views to insert.
        private static void Write(RackCadRackDoorPort.PendingPushBackWrite pending, string command, string nothing)
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;
            if (pending == null)
            {
                document.Editor.WriteMessage("\n" + command + " la usa la ventana RackCad: no hay " + nothing + ".");
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

                var blocks = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                if (blocks.Count == 0)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "El rack ya no está en el dibujo: no se escribió nada."));
                    return;
                }

                // M-02 of the review of Selectivo: the draft is written in every view, so they must carry the same design.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, siblings.Reason));
                    return;
                }

                var (blockId, embed) = blocks[0];
                var write = pending.Write;
                var plan = PushBackDoorRules.PlanApply(embed?.Kind, embed?.Design, write);
                if (!plan.CanWrite)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, plan.Reason));
                    return;
                }

                var system = PushBackDoorRules.Named(write.System, write.Name, embed.Name);
                var (redrawn, erased) = RackPushBackWindowDrawing.Apply(document, blockId, embed, plan.Project, write.Design, system, pending.RackId, write.InsertViews);
                if (!write.Inserts)
                {
                    door?.AnswerWrite(RackPropertiesDoorData.WithRackProperties(document, pending.RackId, write.Properties,
                        RackDoorRules.AnswerForApply(pending.RequestId, pending.RackId, redrawn, erased))); // the tab «Propiedades» (VA-07)
                    return;
                }

                var inserted = System.Math.Max(0, RackCommandSupport.FindRackBlocks(document, pending.RackId).Count - (blocks.Count - erased));
                door?.AnswerWrite(RackDoorRules.AnswerForInsert(pending.RequestId, pending.RackId, redrawn, erased, inserted));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo escribir: " + ex.Message));
            }
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
