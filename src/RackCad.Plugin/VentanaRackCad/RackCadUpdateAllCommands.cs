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

            // The drawing halves of every system write their own lines («… actualizado…») as they go; when the set is not confirmed,
            // this last line says that none of that stayed (review of c4985a2e, minor 4).
            document.Editor.WriteMessage("\nRackCad «Actualizar todo»: " + (answer.Committed
                ? answer.Message
                : "lo que dicen las líneas anteriores se deshizo. " + answer.Message));
            door?.AnswerUpdateAll(answer);
        }
    }

    /// <summary>
    /// The host of «Actualizar todo» in AutoCAD. Every check is the one of the APLICAR command of its system, and every write calls the same
    /// apply function. What it adds, for the set (corrections of the review of c4985a2e):
    /// <list type="bullet">
    /// <item>B-1: a rack counts as written only when every view was redrawn or erased and, read again inside the transaction, its views
    /// carry one design (<see cref="UpdateAllVerification.AfterRackWrite"/>); a tab «Propiedades» that could not be written fails it too.</item>
    /// <item>I-2: while the transaction is open the scope (<see cref="RackUpdateAllScope"/>) keeps the library from importing and the purge
    /// for after the commit; the blocks a pass needed are imported here, OUTSIDE any transaction, and the set is written again.</item>
    /// <item>The blocks neither the drawing nor the library has are a notice of the draft, for every system («Escrito, con avisos»).</item>
    /// </list>
    /// Variables are written first and a Selectivo is planned again over the register after them.
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
            RackUpdateAllScope.Enter();
        }

        // I-2: what a discarded pass needed, imported with no transaction open (the importer clones only what the drawing lacks). An import
        // that throws is counted by the importer and fails the set; one the library cannot give is seen again by the next pass.
        public string ImportOutside(IReadOnlyCollection<string> blocks)
        {
            if (_transaction != null || RackUpdateAllTransaction.AnyOpen(_document))
                return "AutoCAD tiene una operación abierta en el dibujo y la biblioteca solo se importa fuera de ella.";
            var failures = BlockLibraryImporter.ImportFailures;
            BlockLibraryImporter.EnsureBlocks(_document.Database, blocks);
            return BlockLibraryImporter.ImportFailures != failures ? "la importación falló (el registro de RackCad dice por qué)." : null;
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

        // The scope forgets what the discarded pass observed and deferred (its ids belong to nothing now).
        public void Rollback()
        {
            var transaction = _transaction;
            _transaction = null;
            try
            {
                transaction?.Abort();
            }
            finally
            {
                if (RackUpdateAllScope.IsActive) RackUpdateAllScope.ClearPass();
            }
        }

        // After the commit: the purge the helpers deferred, once, on the committed drawing and in the same command (one «U»); then one
        // regeneration for the set. After a rollback, only the regeneration.
        public void Finish(bool committed)
        {
            StopMeter();
            var purge = RackUpdateAllScope.TakePurge();
            RackUpdateAllScope.Exit();
            if (committed && purge.Count > 0) LateralHeaderDrawer.PurgeUnreferenced(_document.Database, purge);
            _document.Editor.Regen();
        }

        public void Dispose()
        {
            Rollback();
            RackUpdateAllScope.Exit(); // never left active: outside the command the helpers import and purge as always
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

        // What the library observed during this draft (the scope): blocks to import outside the transaction, and blocks neither the drawing
        // nor the library has, a notice of the draft in every system (as the «Actualizar» of the rack leaves those pieces out).
        public UpdateAllApplied Apply(UpdateAllItem item, UpdateAllCheck check)
        {
            RackUpdateAllScope.BeginDraft();
            var applied = ApplyOne(item, check);
            if (!applied.Written) return applied;

            var pending = RackUpdateAllScope.DraftPending;
            var missing = RackUpdateAllScope.DraftMissing;
            var notice = missing.Count == 0 ? null
                : "Faltan en la biblioteca de bloques " + string.Join(", ", missing) + ": se escribió sin esas piezas, como en su «Actualizar».";
            var warning = string.IsNullOrWhiteSpace(applied.Warning) ? notice
                : notice == null ? applied.Warning : applied.Warning.TrimEnd() + " " + notice;
            return applied with { Warning = warning, PendingImports = pending.Count == 0 ? null : new List<string>(pending) };
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
                    return RackWritten(write.RackId, views, redrawn, erased, write.Properties, RackDoorRules.AnswerForApply(_requestId, write.RackId, redrawn, erased));
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
                    return RackWritten(write.RackId, views, redrawn, erased, write.Properties, RackDoorRules.AnswerForApply(_requestId, write.RackId, redrawn, erased));
                }

                case UpdateAllSystem.PushBack:
                {
                    var write = item.PushBack;
                    var plan = (PushBackApplyPlan)state.Plan;
                    var system = PushBackDoorRules.Named(write.System, write.Name, embed.Name);
                    var (redrawn, erased) = RackPushBackWindowDrawing.Apply(_document, blockId, embed, plan.Project, write.Design, system, write.RackId, null);
                    return RackWritten(write.RackId, views, redrawn, erased, write.Properties, RackDoorRules.AnswerForApply(_requestId, write.RackId, redrawn, erased));
                }

                case UpdateAllSystem.Cantilever:
                {
                    var write = item.Cantilever;
                    var plan = (CantileverApplyPlan)state.Plan;
                    var (redrawn, erased) = RackCantileverCommands.ApplyCantileverEdit(_document, blockId, embed, plan.Project,
                        new CantileverDraftOutcome(write.RackId, write.Name, write.Design, write.Line));
                    return RackWritten(write.RackId, views, redrawn, erased, write.Properties, RackDoorRules.AnswerForApply(_requestId, write.RackId, redrawn, erased));
                }

                case UpdateAllSystem.Cama:
                {
                    var write = item.Cama;
                    var plan = (CamaApplyPlan)state.Plan;
                    // One block, redrawn in place (its missing pieces are the notice of Apply, as in the other systems).
                    var result = RackCamaCommands.ApplyCamaUpdate(_document, blockId, embed, plan.Source, write.Bed, write.RackId, write.Name);
                    var success = result != null && result.Success;
                    return RackWritten(write.RackId, views, success ? 1 : 0, 0, write.Properties,
                        CamaDoorRules.AnswerForApply(_requestId, write.RackId, success, result?.ErrorMessage));
                }

                case UpdateAllSystem.Cabecera:
                {
                    // The rule of RACKCABECERAAPLICAR (correction 7 of its review): the blocks take the inner name, or the one of the envelope.
                    var write = item.Cabecera;
                    var plan = (CabeceraApplyPlan)state.Plan;
                    var name = string.IsNullOrWhiteSpace(write.Cabecera.Name) ? embed.Name : write.Cabecera.Name;
                    var (redrawn, erased) = RackCabeceraCommands.ApplyCabeceraUpdate(_document, plan.Project, write.Cabecera, write.RackId, name);
                    return RackWritten(write.RackId, views, redrawn, erased, null, RackDoorRules.AnswerForApply(_requestId, write.RackId, redrawn, erased));
                }

                default:
                    return UpdateAllApplied.Failed("«Actualizar todo» no escribe este sistema.");
            }
        }

        // The answer of the APLICAR command of the system, then the tab «Propiedades» as that command writes it (VA-07), then B-1: the rack
        // read again inside the transaction. A tab that could not be written, a view that was not redrawn (the helpers swallow its failure)
        // or views that do not carry one design fail the set: the rack would be half written in an operation that is confirmed.
        private UpdateAllApplied RackWritten(string rackId, int viewsBefore, int redrawn, int erased, RackPropertiesWrite properties, RackDesignWritten answer)
        {
            if (!answer.Written) return UpdateAllApplied.Failed(answer.Message);
            var withProperties = RackPropertiesDoorData.WithRackProperties(_document, rackId, properties, answer);
            if (!string.IsNullOrWhiteSpace(withProperties.PropertiesError))
                return UpdateAllApplied.Failed("Las propiedades del rack no se pudieron escribir: " + withProperties.PropertiesError);

            var after = RackCommandSupport.FindRackBlocks(_document, rackId);
            var siblings = after.Count == 0 ? null : RackSiblingDesigns.Compare(rackId, Views(after));
            var broken = UpdateAllVerification.AfterRackWrite(viewsBefore, redrawn, erased, after.Count, siblings != null && siblings.Coincide, siblings?.Reason);
            return broken != null ? UpdateAllApplied.Failed(broken) : new UpdateAllApplied(true, null, null, withProperties);
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
