using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T5 cm (I-64, quinta tanda; prototype «camas»): the three commands of the door of a cama de rodamiento, as those of Dinámico, Selectivo
    /// and the cabecera. Each takes the request that waits in the door (<see cref="RackCadRackDoorPort"/>) and answers with its number;
    /// without a request, or for a request of another drawing, it writes nothing. RACKCAMAABRIR only reads (no undo marker, no history);
    /// RACKCAMAAPLICAR redraws the block of the cama in place with the drawing half of «Actualizar» of RACKEDITAR
    /// (<see cref="RackCamaCommands.ApplyCamaUpdate"/>), so one «U» undoes it, and then writes the tab «Propiedades» (VA-07); RACKCAMANUEVO
    /// inserts a new cama of the window with the dispatch of «Nuevo» (the jig and the automatic name of I-60). The read and the update compare
    /// the views of the rack first (M-02 of Selectivo, <see cref="RackSiblingDesigns"/>): a cama has one view, and two definitions with the
    /// same identity do not open (closed when in doubt).
    /// </summary>
    public sealed class RackCadCamaDoorCommands
    {
        private const string OtherDrawingText = "La orden corrió en otro dibujo que el de la cama: no se hizo nada. Vuelve al dibujo de la cama e inténtalo de nuevo.";

        [CommandMethod("RACKCAMAABRIR", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]
        public void ReadCama()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingCamaRead();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKCAMAABRIR la usa la ventana RackCad: no hay ninguna lectura pendiente.");
                return;
            }

            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerCamaRead(CamaDesignRead.Refused(pending.RequestId, pending.RackId, null, null, OtherDrawingText));
                    return;
                }

                var blocks = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                if (blocks.Count == 0)
                {
                    door?.AnswerCamaRead(CamaDesignRead.Refused(pending.RequestId, pending.RackId, null, null, "La cama no está en el dibujo."));
                    return;
                }

                // M-02 of the review of Selectivo: the window opens the design of one view, so the views must carry the same one.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerCamaRead(CamaDesignRead.Refused(pending.RequestId, pending.RackId, null, null, siblings.Reason));
                    return;
                }

                var embed = blocks[0].Embed;
                // The view of the envelope decodes as EditCama decodes it (RackCommandSupport.DecodeView), then the design (point 5 of the review).
                door?.AnswerCamaRead(CamaDoorRules.AnswerForOpen(pending.RequestId, pending.RackId, embed?.Kind, embed?.Name, embed?.View, embed?.Section ?? -1,
                    embed?.Design, RackPropertiesDoorData.Read(document, blocks[0].BlockId))); // VA-07: the tab «Propiedades»
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadCamaDoorCommands.ReadCama", ex);
                door?.AnswerCamaRead(CamaDesignRead.Refused(pending.RequestId, pending.RackId, null, null, ex.Message));
            }
        }

        [CommandMethod("RACKCAMAAPLICAR")]
        public void WriteCama()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingCamaWrite();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKCAMAAPLICAR la usa la ventana RackCad: no hay ninguna cama pendiente de escribir.");
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
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "La cama ya no está en el dibujo: no se escribió nada."));
                    return;
                }

                // M-02 of the review of Selectivo: the draft is written in the block of every view, so they must carry the same design.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, siblings.Reason));
                    return;
                }

                var (blockId, embed) = blocks[0];
                var plan = CamaDoorRules.PlanApply(embed?.Kind, embed?.View, embed?.Section ?? -1, embed?.Design, pending.Write);
                if (!plan.CanWrite)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, plan.Reason));
                    return;
                }

                // «Written» only when the block was redrawn (I-08 of Dinámico); then the tab «Propiedades» (VA-07).
                var result = RackCamaCommands.ApplyCamaUpdate(document, blockId, embed, plan.Source, pending.Write.Bed, pending.RackId, pending.Write.Name);
                door?.AnswerWrite(RackPropertiesDoorData.WithRackProperties(document, pending.RackId, pending.Write.Properties,
                    CamaDoorRules.AnswerForApply(pending.RequestId, pending.RackId, result != null && result.Success, result?.ErrorMessage)));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo escribir: " + ex.Message));
            }
        }

        /// <summary>
        /// A new cama made in the window: the insertion of a new cama by the same dispatch as the classic editor opened from «Nuevo»
        /// (RACKNUEVO Cama), with the identity the window gave it and the automatic name of I-60 when it has none. The answer counts the blocks
        /// of that identity that were placed (the jig can be cancelled), and gives the name the drawing holds.
        /// </summary>
        [CommandMethod("RACKCAMANUEVO")]
        public void InsertNewCama()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingCamaNew();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKCAMANUEVO la usa la ventana RackCad: no hay ninguna cama nueva pendiente de insertar.");
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

                var refusal = CamaDoorRules.RefusalForNewCama(pending.Write, RackCommandSupport.FindRackBlocks(document, pending.RackId).Count);
                if (refusal != null)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, refusal));
                    return;
                }

                // The document of a design of the library carries its unknown fields to the new cama (I-11); null for the defaults.
                var source = new FlowBedConfigurationStore().DeserializeDocument(pending.Write.SourceDesignJson);
                RackMenuCommands.DispatchInsertion(document,
                    new RackCad.UI.Editor.FlowBedInsertionRequest(pending.Write.Bed, pending.RackId, pending.Write.Name, source));
                var placed = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                door?.AnswerWrite(RackPropertiesDoorData.WithNewRackProperties(document, pending.RackId, pending.Write.Properties,
                    CamaDoorRules.AnswerForNewCama(pending.RequestId, pending.RackId, placed.Count, placed.Count > 0 ? placed[0].Embed?.Name : null))); // T6 pr: and its properties (VA-S9)
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo insertar la cama nueva: " + ex.Message));
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
