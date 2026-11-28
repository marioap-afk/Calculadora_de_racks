using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T4 cb: the commands of the door of a cabecera of its own, as those of Dinámico and Selectivo. Each takes the request that waits in
    /// the door (<see cref="RackCadRackDoorPort"/>) and answers with its number; without a request, or for a request of another drawing, it
    /// writes nothing. RACKCABECERAABRIR only reads (no undo marker, no history); RACKCABECERAAPLICAR redraws the views of the cabecera in
    /// place with the drawing half of «Actualizar» of RACKEDITAR (<see cref="RackCabeceraCommands.ApplyCabeceraUpdate"/>), so one «U»
    /// undoes it. Both compare the views of the rack first (M-02 of Selectivo, <see cref="RackSiblingDesigns"/>).
    /// <para>T7 cab (I-64, séptima tanda; the Owner on 2026-11-27: «Las dos»):</para>
    /// <list type="bullet">
    /// <item>RACKCABECERAABRIR also reads the properties of the rack, and RACKCABECERAAPLICAR writes its tab «Propiedades» (VA-07), as the
    /// doors of the other systems;</item>
    /// <item>RACKCABECERAINSERTAR («Insertar vista…») first writes the cabecera in the views that exist, then places the new ones, linked,
    /// with the drawing half of «Insertar» of RACKEDITAR (<see cref="RackCabeceraCommands.ApplyCabeceraInsert"/>: B-1 and the rule of the
    /// incomplete redraw);</item>
    /// <item>RACKCABECERANUEVO inserts a new cabecera of the window by the same dispatch as the classic configurator opened from «Nuevo», with
    /// the identity the window gave it and the automatic name of I-60 when it has none, then its tab «Propiedades» (VA-S9).</item>
    /// </list>
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

                var (blockId, embed) = blocks[0];
                door?.AnswerCabeceraRead(CabeceraDoorRules.AnswerForOpen(pending.RequestId, pending.RackId, embed?.Kind, embed?.Name, embed?.Design,
                    RackPropertiesDoorData.Read(document, blockId))); // T7 cab: and the properties of the rack (VA-07)
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
                door?.AnswerWrite(RackPropertiesDoorData.WithRackProperties(document, pending.RackId, pending.Write.Properties,
                    RackDoorRules.AnswerForApply(pending.RequestId, pending.RackId, redrawn, erased))); // T7 cab: the tab «Propiedades» (VA-07)
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo escribir: " + ex.Message));
            }
        }

        /// <summary>
        /// T7 cab: «Insertar vista…» of the window: the cabecera in the views that exist, then the new views placed as linked copies (the same
        /// checks as RACKCABECERAAPLICAR, then the drawing half of «Insertar» of RACKEDITAR).
        /// </summary>
        [CommandMethod("RACKCABECERAINSERTAR")]
        public void InsertCabeceraViews()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingCabeceraInsert();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKCABECERAINSERTAR la usa la ventana RackCad: no hay ninguna vista pendiente de insertar.");
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

                var (blockId, embed) = blocks[0];
                var plan = CabeceraDoorRules.PlanApply(embed?.Kind, embed?.Design, pending.Write);
                if (!plan.CanWrite)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, plan.Reason));
                    return;
                }

                // The name of the blocks is the rule of RACKCABECERAAPLICAR and of EditCabecera (the inner one, else the one of the envelope).
                var cabecera = pending.Write.Cabecera;
                var name = string.IsNullOrWhiteSpace(cabecera.Name) ? embed.Name : cabecera.Name;
                // The identity by the rule of RACKEDITAR «Insertar»: the GUID of the envelope; the one of the request only as a fallback.
                var id = string.IsNullOrWhiteSpace(embed.Id) ? pending.RackId : embed.Id;
                var (redrawn, erased) = RackCabeceraCommands.ApplyCabeceraInsert(document, blockId, embed, plan.Project, cabecera, id, name, pending.Write.InsertViews);
                var inserted = System.Math.Max(0, RackCommandSupport.FindRackBlocks(document, pending.RackId).Count - (blocks.Count - erased));
                door?.AnswerWrite(RackDoorRules.AnswerForInsert(pending.RequestId, pending.RackId, redrawn, erased, inserted));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo escribir: " + ex.Message));
            }
        }

        /// <summary>T7 cab: a new cabecera of the window: the insertion of a new rack, by the dispatch of «Nuevo» (RACKNUEVO), then its properties.</summary>
        [CommandMethod("RACKCABECERANUEVO")]
        public void InsertNewCabecera()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingCabeceraNew();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKCABECERANUEVO la usa la ventana RackCad: no hay ninguna cabecera nueva pendiente de insertar.");
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
                // The dispatch of «Nuevo» names the cabecera (I-60) and places every view with the batch of the classic configurator; the jig
                // asks «Punto de insercion de la cabecera» (RackJigPrompt.ForSystem of a cabecera keeps it).
                RackMenuCommands.DispatchInsertion(document,
                    RackCad.UI.VentanaRackCad.CabeceraDoorInsertion.NewRackRequest(write.RackId, write.Cabecera, write.InsertViews));
                var placed = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                door?.AnswerWrite(RackPropertiesDoorData.WithNewRackProperties(document, pending.RackId, write.Properties,
                    RackDoorRules.AnswerForNewRack(pending.RequestId, pending.RackId, placed.Count, placed.Count > 0 ? placed[0].Embed?.Name : null))); // VA-S9
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo insertar la cabecera nueva: " + ex.Message));
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
