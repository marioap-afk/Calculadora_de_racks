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
    /// drawing and its transaction belong. The Selectivo has its own pair (Selectivo E1-A), RACKSELECTIVOABRIR and RACKSELECTIVOAPLICAR,
    /// with the same rules; both read the register of project variables, as RACKEDITAR does before its window exists. Its second delivery
    /// adds RACKSELECTIVOINSERTAR («Insertar vista…») and RACKSELECTIVONUEVO (a new Selectivo of the window), as Dinámico has them.
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

                // M-02 of the review of Selectivo: the window opens the design of one view, so the views must carry the same one.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerRead(new RackDesignRead(pending.RequestId, pending.RackId, false, null, null, null, siblings.Reason));
                    return;
                }

                var embed = blocks[0].Embed;
                if (embed == null || string.IsNullOrEmpty(embed.Design))
                {
                    door?.AnswerRead(new RackDesignRead(pending.RequestId, pending.RackId, false, embed?.Kind, embed?.Name, null, "El rack no tiene datos editables."));
                    return;
                }

                door?.AnswerRead(new RackDesignRead(pending.RequestId, pending.RackId, true, embed.Kind, embed.Name, embed.Design, null,
                    RackPropertiesDoorData.Read(document, blocks[0].BlockId))); // T4 va: the tab «Propiedades» (VA-07)
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
                door?.AnswerWrite(RackPropertiesDoorData.WithRackProperties(document, pending.RackId, pending.Properties,
                    RackDoorRules.AnswerForApply(pending.RequestId, pending.RackId, redrawn, erased))); // T4 va: and its properties (VA-07)
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo escribir: " + ex.Message));
            }
        }

        /// <summary>
        /// «Insertar vista…» of the window (step 4 of Dinámico; decision 10 of the prototype: the draft is first written in the views that
        /// exist): the drawing half of RACKEDITAR with an insertion, as «Insertar» of the classic editor does on a rack of the drawing. It
        /// redraws every view of the rack with the draft, then places the new views as linked copies (the same identity) with the jig. The
        /// answer counts the views of the rack before and after, so «inserted» means placed.
        /// </summary>
        [CommandMethod("RACKDINAMICOINSERTAR")]
        public void InsertViews()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingInsert();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKDINAMICOINSERTAR la usa la ventana RackCad: no hay ninguna vista pendiente de insertar.");
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

                // M-02 of the review of Selectivo: the draft is first written in every view, so they must carry the same design.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, siblings.Reason));
                    return;
                }

                var (blockId, embed) = blocks[0];
                var project = new RackProjectStore().Deserialize(embed.Design);
                if (project?.DynamicDesign == null)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "El rack del dibujo no es un Dinámico legible: no se escribió nada."));
                    return;
                }

                var (redrawn, erased) = RackDinamicoCommands.ApplyDynamicEdit(
                    document, blockId, embed, project,
                    RackCad.UI.Systems.Dynamic.DynamicDraftOutcome.Inserting(pending.RackId, pending.Name, pending.Design, pending.System, pending.Views));
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
        /// A new Dinámico made in the window (step 4 of Dinámico): the insertion of a new rack, by the same dispatch as the classic editor
        /// opened from «Nuevo» (RACKNUEVO), with the identity the window gave it and the automatic name of I-60 when it has none. The answer
        /// counts the views of that identity that were placed, and gives the name the drawing holds.
        /// </summary>
        [CommandMethod("RACKDINAMICONUEVO")]
        public void InsertNewRack()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingNew();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKDINAMICONUEVO la usa la ventana RackCad: no hay ningún rack nuevo pendiente de insertar.");
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

                RackMenuCommands.DispatchInsertion(document,
                    RackCad.UI.Systems.Dynamic.DynamicDraftOutcome.NewRackRequest(pending.RackId, pending.Name, pending.Design, pending.System, pending.Views,
                        pending.SourceDesignJson)); // T6 lib (UT-S10): a design of the library carries its project, as the classic editor
                var placed = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                door?.AnswerWrite(RackDoorRules.AnswerForNewRack(pending.RequestId, pending.RackId, placed.Count, placed.Count > 0 ? placed[0].Embed?.Name : null));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo insertar el rack nuevo: " + ex.Message));
            }
        }

        /// <summary>
        /// The read of a Selectivo for the window (Selectivo E1-A): what RACKEDITAR reads and decides before its window exists — the inner
        /// design of the rack, the register of project variables and <c>SelectiveEditorOpen.Resolve</c> — with no undo marker and no
        /// history. A broken variable reference blocks the opening exactly as it blocks RACKEDITAR. A rack that opens carries the dimension
        /// styles of the drawing, which RACKEDITAR also reads before it opens (S-27).
        /// </summary>
        [CommandMethod("RACKSELECTIVOABRIR", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]
        public void ReadSelectiveDesign()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingSelectiveRead();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKSELECTIVOABRIR la usa la ventana RackCad: no hay ninguna lectura pendiente.");
                return;
            }

            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerSelectiveRead(SelectiveDesignRead.Refused(pending.RequestId, pending.RackId, null, null, OtherDrawingText));
                    return;
                }

                var blocks = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                if (blocks.Count == 0)
                {
                    door?.AnswerSelectiveRead(SelectiveDesignRead.Refused(pending.RequestId, pending.RackId, null, null, "El rack no está en el dibujo."));
                    return;
                }

                // M-02 of the review of Selectivo: the window opens the design of one view, so the views must carry the same one.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerSelectiveRead(SelectiveDesignRead.Refused(pending.RequestId, pending.RackId, null, null, siblings.Reason));
                    return;
                }

                var embed = blocks[0].Embed;
                if (embed == null || string.IsNullOrEmpty(embed.Design))
                {
                    door?.AnswerSelectiveRead(SelectiveDesignRead.Refused(pending.RequestId, pending.RackId, embed?.Kind, embed?.Name, "El rack no tiene datos editables."));
                    return;
                }

                var registry = RackSelectivoCommands.ReadProjectVariables(document, out _);
                var answer = RackDoorRules.AnswerForSelectiveOpen(pending.RequestId, pending.RackId, embed.Kind, embed.Design, registry);

                // The dimension styles of the drawing, as RACKEDITAR gives them to its list (S-27): only for a rack that opens.
                if (answer.Opens) answer = answer with { DimensionStyles = RackCommandSupport.ReadDimensionStyleNames(document) };
                if (answer.Opens) answer = answer with { Properties = RackPropertiesDoorData.Read(document, blocks[0].BlockId) }; // T4 va (VA-07)
                door?.AnswerSelectiveRead(answer);
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadRackDoorCommands.ReadSelectiveDesign", ex);
                door?.AnswerSelectiveRead(SelectiveDesignRead.Refused(pending.RequestId, pending.RackId, null, null, ex.Message));
            }
        }

        /// <summary>
        /// «Actualizar» of a Selectivo of the window (Selectivo E1-A): the rack and the register are read AGAIN, the write is planned with
        /// the opened linked states (<see cref="RackDoorRules.PlanSelectiveApply"/>), and the «Actualizar» drawing half of RACKEDITAR
        /// (<see cref="RackSelectivoCommands.ApplySelectiveEdit"/>) reconciles and redraws every view of the rack. One «U» undoes it.
        /// </summary>
        [CommandMethod("RACKSELECTIVOAPLICAR")]
        public void WriteSelectiveDesign()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingSelectiveWrite();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKSELECTIVOAPLICAR la usa la ventana RackCad: no hay ningún borrador pendiente de escribir.");
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

                // M-02: «Actualizar» writes the draft in every view, so the views must carry the same design as the one that was opened.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, siblings.Reason));
                    return;
                }

                var (blockId, embed) = blocks[0];
                var write = pending.Write;
                var registry = RackSelectivoCommands.ReadProjectVariables(document, out _);
                var plan = RackDoorRules.PlanSelectiveApply(embed?.Kind, embed?.Design, registry, write);
                if (!plan.CanWrite)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, plan.Reason));
                    return;
                }

                // «Written» only when a view was redrawn: the reconciliation can refuse before any geometry, or nothing be redrawn (I-08).
                var (redrawn, erased) = RackSelectivoCommands.ApplySelectiveEdit(
                    document, blockId, embed, plan.Saved, registry,
                    new RackCad.UI.Systems.Selective.SelectiveDraftOutcome(write.RackId, write.Name, write.Design, write.System,
                        write.LinkedPropertyFinalStates));
                door?.AnswerWrite(RackPropertiesDoorData.WithRackProperties(document, pending.RackId, write.Properties,
                    RackDoorRules.AnswerForApply(pending.RequestId, pending.RackId, redrawn, erased))); // T4 va: and its properties (VA-07)
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

        /// <summary>
        /// «Insertar vista…» of a Selectivo of the window (Selectivo E2, part A): the rack and the register are read AGAIN and the write is
        /// planned as for RACKSELECTIVOAPLICAR (<see cref="RackDoorRules.PlanSelectiveApply"/>); then the «Insertar» drawing half of
        /// RACKEDITAR (<see cref="RackSelectivoCommands.InsertSelectiveViews"/>, the branch <c>!UpdateOnly</c> of EditSelective) writes the
        /// draft in the views that exist and places the chosen ones the rack can draw (<see cref="SelectiveDoorInsertion.DrawableViews"/>).
        /// Declared: the counts come from the views of the identity before and after (inserted = placed; erased = what is missing after).
        /// </summary>
        [CommandMethod("RACKSELECTIVOINSERTAR")]
        public void InsertSelectiveViews()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingSelectiveInsert();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKSELECTIVOINSERTAR la usa la ventana RackCad: no hay ninguna vista pendiente de insertar.");
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

                // M-02 (integration of E2 with the rule of the views): the draft is first written in every view, so they must carry the same design.
                var siblings = RackSiblingDesigns.Compare(pending.RackId, Views(blocks));
                if (!siblings.Coincide)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, siblings.Reason));
                    return;
                }

                var (blockId, embed) = blocks[0];
                var write = pending.Write;
                var refusal = SelectiveDoorInsertion.RefusalOf(write);
                if (refusal != null)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, refusal));
                    return;
                }

                var registry = RackSelectivoCommands.ReadProjectVariables(document, out _);
                var plan = RackDoorRules.PlanSelectiveApply(embed?.Kind, embed?.Design, registry, write);
                if (!plan.CanWrite)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, plan.Reason));
                    return;
                }

                var views = SelectiveDoorInsertion.DrawableViews(write.InsertViews, write.System, RackCad.Plugin.Drawing.LateralHeaderDrawService.LoadCatalog());
                if (views.Count == 0)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false,
                        "Ninguna de las vistas elegidas se puede dibujar con el borrador: no se escribió nada."));
                    return;
                }

                RackUnitsGuard.WarnIfNotInches(document);
                var placed = RackSelectivoCommands.InsertSelectiveViews(
                    document, blockId, embed, plan.Saved, registry,
                    new RackCad.UI.Systems.Selective.SelectiveDraftOutcome(write.RackId, write.Name, write.Design, write.System,
                        write.LinkedPropertyFinalStates),
                    views);
                if (placed < 0)
                {
                    door?.AnswerWrite(SelectiveDoorInsertion.AnswerForInsert(pending.RequestId, pending.RackId, 0, 0, 0));
                    return;
                }

                var after = RackCommandSupport.FindRackBlocks(document, pending.RackId).Count;
                var erased = System.Math.Max(0, blocks.Count + placed - after);
                door?.AnswerWrite(SelectiveDoorInsertion.AnswerForInsert(pending.RequestId, pending.RackId, blocks.Count - erased, erased, placed));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo insertar: " + ex.Message));
            }
        }

        /// <summary>
        /// A new Selectivo made in the window (Selectivo E2, as RACKDINAMICONUEVO for Dinámico): the insertion of a new rack, by the same
        /// dispatch as the classic editor opened from «Nuevo» (RACKNUEVO, <see cref="RackMenuCommands.DispatchInsertion"/>), with the
        /// identity the window gave it and the automatic name of I-60 when it has none. What it inserts is decided first by the pure rule
        /// (<see cref="SelectiveDoorInsertion.PlanNewRack"/>): the views the draft can draw, never over a rack of the drawing with the same
        /// identity. The answer counts the views of that identity that were placed, and gives the name the drawing holds.
        /// </summary>
        [CommandMethod("RACKSELECTIVONUEVO")]
        public void InsertNewSelective()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingSelectiveNew();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKSELECTIVONUEVO la usa la ventana RackCad: no hay ningún rack nuevo pendiente de insertar.");
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

                var write = pending.Write;
                var plan = SelectiveDoorInsertion.PlanNewRack(
                    write,
                    RackCommandSupport.FindRackBlocks(document, pending.RackId).Count > 0,
                    RackCad.Plugin.Drawing.LateralHeaderDrawService.LoadCatalog());
                if (!plan.CanInsert)
                {
                    door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, plan.Reason));
                    return;
                }

                RackMenuCommands.DispatchInsertion(document,
                    RackCad.UI.Systems.Selective.SelectiveDraftOutcome.NewRackRequest(write.RackId, write.Name, write.Design, write.System, plan.Views));
                var placed = RackCommandSupport.FindRackBlocks(document, pending.RackId);
                door?.AnswerWrite(SelectiveDoorInsertion.AnswerForNewRack(pending.RequestId, pending.RackId, placed.Count, placed.Count > 0 ? placed[0].Embed?.Name : null));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerWrite(new RackDesignWritten(pending.RequestId, pending.RackId, false, "No se pudo insertar el rack nuevo: " + ex.Message));
            }
        }
    }
}
