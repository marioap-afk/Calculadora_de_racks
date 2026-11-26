using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T4 cb (I-64, fourth batch): the two commands of the door of a cabecera of its own, as those of Dinámico and Selectivo. Each takes the
    /// request that waits in the door (<see cref="RackCadRackDoorPort"/>) and answers with its number; without a request, or for a request of
    /// another drawing, it writes nothing. RACKCABECERAABRIR only reads (no undo marker, no history); RACKCABECERAAPLICAR redraws the views of
    /// the cabecera in place with the drawing half of «Actualizar» of RACKEDITAR (<see cref="RackCabeceraCommands.ApplyCabeceraUpdate"/>), so
    /// one «U» undoes it. Both compare the views of the rack first (M-02 of Selectivo, <see cref="RackSiblingDesigns"/>).
    /// </summary>
    public sealed class RackCadCabeceraDoorCommands
    {
        private const string OtherDrawingText = "La orden corrió en otro dibujo que el de la cabecera: no se hizo nada. Vuelve al dibujo de la cabecera e inténtalo de nuevo.";

        [CommandMethod("RACKCABECERAABRIR", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]
        public void ReadCabecera()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingCabeceraRead();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKCABECERAABRIR la usa la ventana RackCad: no hay ninguna lectura pendiente.");
                return;
            }

            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerCabeceraRead(CabeceraDesignRead.Refused(pending.RequestId, pending.RackId, null, null, OtherDrawingText));
                    return;
                }

                var blocks = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                if (blocks.Count == 0)
                {
                    door?.AnswerCabeceraRead(CabeceraDesignRead.Refused(pending.RequestId, pending.RackId, null, null, "La cabecera no está en el dibujo."));
                    return;
                }

                // M-02 of the review of Selectivo: the window opens the design of one view, so the views must carry the same one.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerCabeceraRead(CabeceraDesignRead.Refused(pending.RequestId, pending.RackId, null, null, siblings.Reason));
                    return;
                }

                var embed = blocks[0].Embed;
                door?.AnswerCabeceraRead(CabeceraDoorRules.AnswerForOpen(pending.RequestId, pending.RackId, embed?.Kind, embed?.Name, embed?.Design));
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadCabeceraDoorCommands.ReadCabecera", ex);
                door?.AnswerCabeceraRead(CabeceraDesignRead.Refused(pending.RequestId, pending.RackId, null, null, ex.Message));
            }
        }

        [CommandMethod("RACKCABECERAAPLICAR")]
        public void WriteCabecera()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingCabeceraWrite();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKCABECERAAPLICAR la usa la ventana RackCad: no hay ninguna cabecera pendiente de escribir.");
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
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "La cabecera ya no está en el dibujo: no se escribió nada."));
                    return;
                }

                // M-02 of the review of Selectivo: the cabecera is written in every view, so they must carry the same design.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, siblings.Reason));
                    return;
                }

                var embed = blocks[0].Embed;
                var plan = CabeceraDoorRules.PlanApply(embed?.Kind, embed?.Design, pending.Write);
                if (!plan.CanWrite)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, plan.Reason));
                    return;
                }

                // The rule of the classic editor (EditCabecera, correction 7 of the review): the blocks take the inner name, or the one of
                // the envelope when the inner one is blank; the inner name is not touched (AUTH-15 admits it blank).
                var cabecera = pending.Write.Cabecera;
                var name = string.IsNullOrWhiteSpace(cabecera.Name) ? embed.Name : cabecera.Name;
                var (redrawn, erased) = RackCabeceraCommands.ApplyCabeceraUpdate(document, plan.Project, cabecera, pending.RackId, name);
                door?.AnswerWrite(RackDoorRules.AnswerForApply(pending.RequestId, pending.RackId, redrawn, erased));
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
