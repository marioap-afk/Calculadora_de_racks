using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Persistence;
using RackCad.Application.VentanaRackCad;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.Shared;
using RackCad.UI.Systems.Cantilever;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// T6 at (I-64, sexta tanda): «Actualizar todo» of the RackCad window, RACKACTUALIZARTODO. It takes the request that waits in the door
    /// (<see cref="RackCadRackDoorPort"/>) and answers with its number; without a request, or for a request of another drawing, it writes
    /// nothing. It is ONE command, so one «U» undoes all of it, and it runs the pure composition (<see cref="UpdateAllRun"/>) over the host
    /// below: ONE outer transaction (<see cref="RackUpdateAllTransaction"/>); inside it, first the re-read and the comparison of every draft
    /// exactly as its own APLICAR command does them, then the writes with the same functions those commands call; confirmed only when every
    /// draft was written. The «Actualizar» of every rack and the seam of one rack of I-55 do not change: this is a composition over them.
    /// </summary>
    public sealed class RackCadUpdateAllCommands
    {
        private const string OtherDrawingText = "La orden corrió en otro dibujo que el de los borradores: no se hizo nada. Vuelve a ese dibujo e inténtalo de nuevo.";

        [CommandMethod("RACKACTUALIZARTODO")]
        public void UpdateAll()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingUpdateAll();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKACTUALIZARTODO la usa la ventana RackCad: no hay ningún borrador pendiente de escribir.");
                return;
            }

            door?.Started(pending.RequestId);

            UpdateAllWritten answer;
            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    answer = UpdateAllRun.Refused(pending.Write, OtherDrawingText);
                }
                else
                {
                    using (var units = new RackCadUpdateAllUnits(document, pending.RequestId))
                    {
                        answer = UpdateAllRun.Execute(pending.Write, units);
                    }
                }
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                answer = UpdateAllRun.Refused(pending.Write, "No se pudo escribir: " + ex.Message);
            }

            document.Editor.WriteMessage("\nRackCad: " + answer.Message);
            door?.AnswerUpdateAll(answer);
        }
    }

    /// <summary>
    /// The host of «Actualizar todo» in AutoCAD. Every check is the one of the APLICAR command of its system, and every write calls the same
    /// apply function, with the rule of that command for «written» (at least one view redrawn, I-08; the cama redrawn). What it adds, for
    /// the set: a view that was not redrawn is a notice («faltantes», D-13); a tab «Propiedades» that could not be written, or an import of
    /// the block library that failed inside the transaction, is a failure of the set (all or nothing over what is authored). Variables are
    /// written first and a Selectivo is planned again over the register after them.
    /// </summary>
    internal sealed class RackCadUpdateAllUnits : IUpdateAllUnitPort, System.IDisposable
    {
        private readonly Document _document;
        private readonly long _requestId;
        private RackUpdateAllTransaction _transaction;
        private ProgressMeter _meter;

        internal RackCadUpdateAllUnits(Document document, long requestId)
        {
            _document = document;
            _requestId = requestId;
        }

        // What the re-read of a rack keeps for its write: the views of the rack and the plan of its system.
        private sealed record RackState(List<(ObjectId BlockId, RackEmbedDocument Embed)> Blocks, object Plan);

        public void Begin()
        {
            // A transaction left open by something else would make this one a nested one that is not ours to confirm.
            if (RackUpdateAllTransaction.AnyOpen(_document))
                throw new System.InvalidOperationException("AutoCAD tiene otra operación abierta en el dibujo.");
            _transaction = RackUpdateAllTransaction.Begin(_document);
        }

        public bool CancelRequested() => HostApplicationServices.Current.UserBreak();

        public void Progress(string step, int done, int total, UpdateAllItem item)
        {
            if (_meter == null)
            {
                _meter = new ProgressMeter();
                _meter.SetLimit(total * 2 + 1);
                _meter.Start("RackCad: Actualizar todo");
            }

            if (done == 0 && item != null)
                _document.Editor.WriteMessage("\nRackCad «Actualizar todo»: " + step.ToLowerInvariant() + " " + UpdateAllText.Count(total, "borrador", "borradores") + "…");
            _meter.MeterProgress();
        }

        public void Commit()
        {
            if (_transaction == null) throw new System.InvalidOperationException("La operación no está abierta.");
            _transaction.Commit();
            _transaction = null;
        }

        public void Rollback()
        {
            var transaction = _transaction;
            _transaction = null;
            transaction?.Abort();
        }

        public void Finish(bool committed)
        {
            StopMeter();

            // One regeneration for the set, after the operation is confirmed or discarded: the drawing shows what it holds now.
            _document.Editor.Regen();
        }

        public void Dispose()
        {
            Rollback();
            StopMeter();
        }

        private void StopMeter()
        {
            var meter = _meter;
            _meter = null;
            if (meter == null) return;
            try
            {
                meter.Stop();
            }
            finally
            {
                meter.Dispose();
            }
        }

        // ------------------------------------------------------------------ the re-read, before any write

        public UpdateAllCheck Preflight(UpdateAllItem item)
        {
            switch (item.System)
            {
                case UpdateAllSystem.Variables:
                    return PreflightVariables(item.Variables);
                case UpdateAllSystem.Dynamic:
                    return PreflightRack(item.RackId, "El rack ya no está en el dibujo.", blocks =>
                        new RackProjectStore().Deserialize(blocks[0].Embed.Design)?.DynamicDesign == null
                            ? UpdateAllCheck.Refused("El rack del dibujo no es un Dinámico legible.")
                            : Ready(blocks, null));
                case UpdateAllSystem.Selective:
                    return PreflightRack(item.RackId, "El rack ya no está en el dibujo.", blocks =>
                    {
                        var embed = blocks[0].Embed;
                        var registry = RackSelectivoCommands.ReadProjectVariables(_document, out _);
                        var plan = RackDoorRules.PlanSelectiveApply(embed?.Kind, embed?.Design, registry, item.Selective);
                        return plan.CanWrite ? Ready(blocks, null) : UpdateAllCheck.Refused(plan.Reason);
                    });
                case UpdateAllSystem.PushBack:
                    return PreflightRack(item.RackId, "El rack ya no está en el dibujo.", blocks =>
                    {
                        var plan = PushBackDoorRules.PlanApply(blocks[0].Embed?.Kind, blocks[0].Embed?.Design, item.PushBack);
                        return plan.CanWrite ? Ready(blocks, plan) : UpdateAllCheck.Refused(plan.Reason);
                    });
                case UpdateAllSystem.Cantilever:
                    return PreflightRack(item.RackId, "La línea ya no está en el dibujo.", blocks =>
                    {
                        var plan = CantileverDoorRules.PlanApply(blocks[0].Embed?.Kind, blocks[0].Embed?.Design, item.Cantilever);
                        return plan.CanWrite ? Ready(blocks, plan) : UpdateAllCheck.Refused(plan.Reason);
                    });
                case UpdateAllSystem.Cama:
                    return PreflightRack(item.RackId, "La cama ya no está en el dibujo.", blocks =>
                    {
                        var embed = blocks[0].Embed;
                        var plan = CamaDoorRules.PlanApply(embed?.Kind, embed?.View, embed?.Section ?? -1, embed?.Design, item.Cama);
                        return plan.CanWrite ? Ready(blocks, plan) : UpdateAllCheck.Refused(plan.Reason);
                    });
                case UpdateAllSystem.Cabecera:
                    return PreflightRack(item.RackId, "La cabecera ya no está en el dibujo.", blocks =>
                    {
                        var plan = CabeceraDoorRules.PlanApply(blocks[0].Embed?.Kind, blocks[0].Embed?.Design, item.Cabecera);
                        return plan.CanWrite ? Ready(blocks, plan) : UpdateAllCheck.Refused(plan.Reason);
                    });
                default:
                    return UpdateAllCheck.Refused("«Actualizar todo» no escribe este sistema.");
            }
        }

        // The checks every APLICAR command makes first: the rack is still in the drawing, and its views carry the same design (M-02).
        private UpdateAllCheck PreflightRack(string rackId, string missing, System.Func<List<(ObjectId BlockId, RackEmbedDocument Embed)>, UpdateAllCheck> plan)
        {
            var blocks = RackCommandSupport.FindRackBlocks(_document, rackId);
            if (blocks.Count == 0) return UpdateAllCheck.Refused(missing);
            var siblings = RackSiblingDesigns.Compare(rackId, Views(blocks));
            if (!siblings.Coincide) return UpdateAllCheck.Refused(siblings.Reason);
            return plan(blocks);
        }

        private static UpdateAllCheck Ready(List<(ObjectId BlockId, RackEmbedDocument Embed)> blocks, object plan) => UpdateAllCheck.Ready(new RackState(blocks, plan));

        // What RACKVARIABLESAPLICAR reads and plans before writing (VariablesDoorRules.PlanWrite), with the racks its write redraws.
        private UpdateAllCheck PreflightVariables(VariablesDoorWrite write)
        {
            var registry = RackSelectivoCommands.ReadProjectVariables(_document, out var entries);
            var properties = CustomPropertiesExecutor.ReadProject(_document).Collection;
            var plan = VariablesDoorRules.PlanWrite(registry, entries, properties, write);
            return plan.CanWrite ? UpdateAllCheck.Ready(plan, plan.RedrawnRackIds) : UpdateAllCheck.Refused(plan.Reason);
        }

        // ------------------------------------------------------------------ the writes, inside the one transaction

        public UpdateAllApplied Apply(UpdateAllItem item, UpdateAllCheck check)
        {
            var imports = BlockLibraryImporter.ImportFailures;
            var applied = ApplyOne(item, check);
            if (applied.Written && BlockLibraryImporter.ImportFailures != imports)
            {
                return UpdateAllApplied.Failed("No se pudieron importar bloques de la biblioteca dentro de «Actualizar todo» (el registro de RackCad dice por qué). "
                                               + "Escribe este borrador con su «Actualizar».");
            }

            return applied;
        }

        private UpdateAllApplied ApplyOne(UpdateAllItem item, UpdateAllCheck check)
        {
            if (item.System == UpdateAllSystem.Variables) return ApplyVariables(item.Variables, (VariablesWritePlan)check.State);

            var state = (RackState)check.State;
            var (blockId, embed) = state.Blocks[0];
            var views = state.Blocks.Count;
            switch (item.System)
            {
                case UpdateAllSystem.Dynamic:
                {
                    var write = item.Dynamic;
                    var project = new RackProjectStore().Deserialize(embed.Design);
                    var (redrawn, erased) = RackDinamicoCommands.ApplyDynamicEdit(_document, blockId, embed, project,
                        new RackCad.UI.Systems.Dynamic.DynamicDraftOutcome(write.RackId, write.Name, write.Design, write.System));
                    return RackWritten(write.RackId, write.Properties, RackDoorRules.AnswerForApply(_requestId, write.RackId, redrawn, erased), Missing(views, redrawn, erased));
                }

                case UpdateAllSystem.Selective:
                {
                    // Planned again over the register of NOW, inside the transaction: after «Variables», the draft must still match the
                    // values that govern its holguras, or nothing is written (the rule of RACKSELECTIVOAPLICAR).
                    var write = item.Selective;
                    var registry = RackSelectivoCommands.ReadProjectVariables(_document, out _);
                    var plan = RackDoorRules.PlanSelectiveApply(embed?.Kind, embed?.Design, registry, write);
                    if (!plan.CanWrite) return UpdateAllApplied.Failed(plan.Reason);
                    var (redrawn, erased) = RackSelectivoCommands.ApplySelectiveEdit(_document, blockId, embed, plan.Saved, registry,
                        new RackCad.UI.Systems.Selective.SelectiveDraftOutcome(write.RackId, write.Name, write.Design, write.System, write.LinkedPropertyFinalStates));
                    return RackWritten(write.RackId, write.Properties, RackDoorRules.AnswerForApply(_requestId, write.RackId, redrawn, erased), Missing(views, redrawn, erased));
                }

                case UpdateAllSystem.PushBack:
                {
                    var write = item.PushBack;
                    var plan = (PushBackApplyPlan)state.Plan;
                    var system = PushBackDoorRules.Named(write.System, write.Name, embed.Name);
                    var (redrawn, erased) = RackPushBackWindowDrawing.Apply(_document, blockId, embed, plan.Project, write.Design, system, write.RackId, null);
                    return RackWritten(write.RackId, write.Properties, RackDoorRules.AnswerForApply(_requestId, write.RackId, redrawn, erased), Missing(views, redrawn, erased));
                }

                case UpdateAllSystem.Cantilever:
                {
                    var write = item.Cantilever;
                    var plan = (CantileverApplyPlan)state.Plan;
                    var (redrawn, erased) = RackCantileverCommands.ApplyCantileverEdit(_document, blockId, embed, plan.Project,
                        new CantileverDraftOutcome(write.RackId, write.Name, write.Design, write.Line));
                    return RackWritten(write.RackId, write.Properties, RackDoorRules.AnswerForApply(_requestId, write.RackId, redrawn, erased), Missing(views, redrawn, erased));
                }

                case UpdateAllSystem.Cama:
                {
                    var write = item.Cama;
                    var plan = (CamaApplyPlan)state.Plan;
                    var result = RackCamaCommands.ApplyCamaUpdate(_document, blockId, embed, plan.Source, write.Bed, write.RackId, write.Name);
                    var warning = result != null && result.Success && result.HasMissingBlocks
                        ? "Faltan bloques de la biblioteca en la cama: " + string.Join(", ", result.MissingBlocks) + "."
                        : null;
                    return RackWritten(write.RackId, write.Properties,
                        CamaDoorRules.AnswerForApply(_requestId, write.RackId, result != null && result.Success, result?.ErrorMessage), warning);
                }

                case UpdateAllSystem.Cabecera:
                {
                    // The rule of RACKCABECERAAPLICAR (correction 7 of its review): the blocks take the inner name, or the one of the envelope.
                    var write = item.Cabecera;
                    var plan = (CabeceraApplyPlan)state.Plan;
                    var name = string.IsNullOrWhiteSpace(write.Cabecera.Name) ? embed.Name : write.Cabecera.Name;
                    var (redrawn, erased) = RackCabeceraCommands.ApplyCabeceraUpdate(_document, plan.Project, write.Cabecera, write.RackId, name);
                    return RackWritten(write.RackId, null, RackDoorRules.AnswerForApply(_requestId, write.RackId, redrawn, erased), Missing(views, redrawn, erased));
                }

                default:
                    return UpdateAllApplied.Failed("«Actualizar todo» no escribe este sistema.");
            }
        }

        // The answer of the APLICAR command of the system, then the tab «Propiedades» as that command writes it (VA-07). For the set, a tab
        // that could not be written is a failure: the rack would be written and its properties not.
        private UpdateAllApplied RackWritten(string rackId, RackPropertiesWrite properties, RackDesignWritten answer, string warning)
        {
            if (!answer.Written) return UpdateAllApplied.Failed(answer.Message);
            var withProperties = RackPropertiesDoorData.WithRackProperties(_document, rackId, properties, answer);
            if (!string.IsNullOrWhiteSpace(withProperties.PropertiesError))
                return UpdateAllApplied.Failed("Las propiedades del rack no se pudieron escribir: " + withProperties.PropertiesError);
            return new UpdateAllApplied(true, null, warning, withProperties);
        }

        // «Faltantes» (D-13): views of the rack that were neither redrawn nor erased.
        private static string Missing(int views, int redrawn, int erased)
        {
            var missing = views - redrawn - erased;
            return missing <= 0 ? null
                : "AutoCAD no redibujó " + UpdateAllText.Count(missing, "vista", "vistas") + " de " + views + "; la línea de órdenes dice cuáles.";
        }

        // What RACKVARIABLESAPLICAR writes: the register and every rack that changes in one execution, then the project collection. In the
        // set both nest in the one transaction, so a failure of the second discards the first too.
        private UpdateAllApplied ApplyVariables(VariablesDoorWrite write, VariablesWritePlan plan)
        {
            var variablesWritten = false;
            var views = 0;
            if (!plan.Variables.IsEmpty)
            {
                var execution = ProjectVariableMutationExecutor.Execute(_document, plan.Variables);
                if (!execution.IsApplied) return UpdateAllApplied.Failed(execution.Error ?? "No había nada que aplicar.");
                variablesWritten = true;
                views = execution.ViewsRedrawn;
            }

            if (plan.ProjectProperties != null)
            {
                var projectWrite = CustomPropertiesExecutor.ExecuteProjectDraft(_document, write.PropertiesFingerprint, write.ProjectProperties);
                if (!projectWrite.IsWritten)
                    return UpdateAllApplied.Failed("Las propiedades del proyecto no se pudieron escribir: "
                                                   + (string.IsNullOrWhiteSpace(projectWrite.Error) ? "sin detalle." : projectWrite.Error));
            }

            var answer = VariablesDoorRules.AnswerForWrite(_requestId, variablesWritten, variablesWritten ? plan.RedrawnRackIds : new List<string>(), views,
                plan.ProjectProperties != null, null);
            return new UpdateAllApplied(true, null, null, VariablesAnswer: answer);
        }

        // The views of the rack for RackSiblingDesigns: each block definition by its handle, with its envelope.
        private static List<RackSiblingView> Views(List<(ObjectId BlockId, RackEmbedDocument Embed)> blocks)
        {
            var views = new List<RackSiblingView>(blocks.Count);
            foreach (var (blockId, embed) in blocks) views.Add(new RackSiblingView(blockId.Handle.ToString(), embed));
            return views;
        }
    }
}
