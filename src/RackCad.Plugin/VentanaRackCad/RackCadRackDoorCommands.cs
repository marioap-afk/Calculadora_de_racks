using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// The two commands of the door of the RackCad window (I-64 A-14, line D). The window starts them; each takes the request that waits in
    /// the door (<see cref="RackCadRackDoorPort"/>) and answers with its number. Without a request, or for a request of another drawing,
    /// it writes nothing. RACKDINAMICOABRIR only reads (no undo marker, no history); RACKDINAMICOAPLICAR writes the copies of the draft with
    /// the drawing half of RACKEDITAR, so one «U» undoes the whole update. They run in their own command context, where the read of the
    /// drawing and its transaction belong.
    /// </summary>
    public sealed class RackCadRackDoorCommands
    {
        private const string OtherDrawingText = "La orden corrió en otro dibujo que el del rack: no se hizo nada. Vuelve al dibujo del rack e inténtalo de nuevo.";

        [CommandMethod("RACKDINAMICOABRIR", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]
        public void ReadDesign()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingRead();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKDINAMICOABRIR la usa la ventana RackCad: no hay ninguna lectura pendiente.");
                return;
            }

            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerRead(new RackDesignRead(pending.RequestId, pending.RackId, false, null, null, null, OtherDrawingText));
                    return;
                }

                var blocks = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                if (blocks.Count == 0)
                {
                    door?.AnswerRead(new RackDesignRead(pending.RequestId, pending.RackId, false, null, null, null, "El rack no está en el dibujo."));
                    return;
                }

                var embed = blocks[0].Embed;
                if (embed == null || string.IsNullOrEmpty(embed.Design))
                {
                    door?.AnswerRead(new RackDesignRead(pending.RequestId, pending.RackId, false, embed?.Kind, embed?.Name, null, "El rack no tiene datos editables."));
                    return;
                }

                door?.AnswerRead(new RackDesignRead(pending.RequestId, pending.RackId, true, embed.Kind, embed.Name, embed.Design, null));
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadRackDoorCommands.ReadDesign", ex);
                door?.AnswerRead(new RackDesignRead(pending.RequestId, pending.RackId, false, null, null, null, ex.Message));
            }
        }

        [CommandMethod("RACKDINAMICOAPLICAR")]
        public void WriteDesign()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingWrite();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKDINAMICOAPLICAR la usa la ventana RackCad: no hay ningún borrador pendiente de escribir.");
                return;
            }

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

                var (blockId, embed) = blocks[0];
                var project = new RackProjectStore().Deserialize(embed.Design);
                if (project?.DynamicDesign == null)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "El rack del dibujo no es un Dinámico legible: no se escribió nada."));
                    return;
                }

                // «Written» only when a view was redrawn: the drawing half can stop before any geometry, or redraw nothing (I-08).
                var (redrawn, erased) = RackDinamicoCommands.ApplyDynamicEdit(
                    document, blockId, embed, project,
                    new RackCad.UI.Systems.Dynamic.DynamicDraftOutcome(pending.RackId, pending.Name, pending.Design, pending.System));
                door?.AnswerWrite(RackDoorRules.AnswerForApply(pending.RequestId, pending.RackId, redrawn, erased));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo escribir: " + ex.Message));
            }
        }
    }
}
