using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Diagnostics;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Plugin.Drawing;
using RackCad.Plugin.Systems.Selective;
using RackCad.Plugin.Systems.Shared;

namespace RackCad.Plugin
{
    /// <summary>How an execution ended.</summary>
    internal enum MutationExecutionOutcome
    {
        Applied = 1,

        /// <summary>The plan was empty. A blocked operation reaches here and writes nothing, by design.</summary>
        NothingToDo = 2,

        /// <summary>Nothing was written. Not "part of it": nothing.</summary>
        Aborted = 3,
    }

    /// <summary>What the execution did, in terms a caller can report to the user.</summary>
    internal sealed class MutationExecutionResult
    {
        private MutationExecutionResult(
            MutationExecutionOutcome outcome, int racksTouched, int viewsRedrawn, bool registryWritten, string error)
        {
            Outcome = outcome;
            RacksTouched = racksTouched;
            ViewsRedrawn = viewsRedrawn;
            RegistryWritten = registryWritten;
            Error = error;
        }

        internal MutationExecutionOutcome Outcome { get; }

        internal int RacksTouched { get; }

        internal int ViewsRedrawn { get; }

        internal bool RegistryWritten { get; }

        /// <summary>Null unless the execution was aborted.</summary>
        internal string Error { get; }

        internal bool IsApplied => Outcome == MutationExecutionOutcome.Applied;

        internal static MutationExecutionResult Applied(int racksTouched, int viewsRedrawn, bool registryWritten)
            => new MutationExecutionResult(
                MutationExecutionOutcome.Applied, racksTouched, viewsRedrawn, registryWritten, null);

        internal static MutationExecutionResult NothingToDo()
            => new MutationExecutionResult(MutationExecutionOutcome.NothingToDo, 0, 0, false, null);

        internal static MutationExecutionResult Aborted(string error)
            => new MutationExecutionResult(MutationExecutionOutcome.Aborted, 0, 0, false, error);
    }

    /// <summary>
    /// Writes a <see cref="MutationPlan"/> to the drawing — the register and every affected rack — as ONE unit.
    ///
    /// <para>
    /// It is a service and not a user surface: the operations that build plans, and the way a user reaches
    /// them, belong to later gates. Keeping the writing separate from the deciding is what makes the promise
    /// checkable, because this file is allowed to contain no decision at all.
    /// </para>
    /// <para>
    /// <b>It never replans.</b> The plan already said what must change and against which views; recomputing
    /// the effective design, the scope or the preflight here would mean the thing executed is no longer the
    /// thing that was approved — and "zero mutation on failure" would stop being provable.
    /// </para>
    /// <para>The shape is PREPARE / MUTATE / POST, and the split is not stylistic:</para>
    /// <list type="bullet">
    /// <item><b>PREPARE</b> resolves the geometry and imports the block definitions the plans need. Importing
    /// mutates the database on its own account, so it cannot happen inside a transaction that is supposed to
    /// be one atomic unit over many racks.</item>
    /// <item><b>MUTATE</b> opens ONE transaction for the register and every view of every rack, and confirms
    /// it once. A per-block transaction would leave a half-propagated drawing on the first failure.</item>
    /// <item><b>POST</b> purges what the redefinitions orphaned and regenerates ONCE — and only if something
    /// was actually redrawn: an operation that touches only the register draws nothing.</item>
    /// </list>
    /// <para>
    /// The envelope is composed PER SIBLING. Every view of a rack carries the same design JSON — that is what
    /// makes them one rack — but each keeps its OWN envelope, with the unknown fields and the schema version
    /// that block was written with (I-11). Composing one envelope and copying it to every view would destroy
    /// exactly what the composer exists to preserve.
    /// </para>
    /// <para>
    /// Physical validation is NOT closed by this file: no suite loads the Plugin (ADR-0003) and the CI has no
    /// AutoCAD. What is fixed here in source guards is ownership and order; that the drawing ends up right is
    /// owned by G16.
    /// </para>
    /// </summary>
    internal static class ProjectVariableMutationExecutor
    {
        /// <summary>How one prepared view is written. Each facade owns its own caller-owned primitive (G9.1).</summary>
        private delegate LateralHeaderDrawOutcome ViewWriter(
            Database database,
            Transaction transaction,
            PreparedViewRedraw prepared,
            out IReadOnlyCollection<ObjectId> staleDefinitions);

        /// <summary>One view, ready to be written, and the facade that writes it.</summary>
        private sealed class PreparedDestination
        {
            internal PreparedDestination(PreparedViewRedraw prepared, ViewWriter write)
            {
                Prepared = prepared;
                Write = write;
            }

            internal PreparedViewRedraw Prepared { get; }

            internal ViewWriter Write { get; }
        }

        internal static MutationExecutionResult Execute(Document document, MutationPlan plan)
        {
            if (document == null)
            {
                return MutationExecutionResult.Aborted("No hay un dibujo activo en AutoCAD.");
            }

            if (plan == null || plan.IsEmpty)
            {
                return MutationExecutionResult.NothingToDo();
            }

            var database = document.Database;
            var prepared = new List<PreparedDestination>();

            try
            {
                using (document.LockDocument())
                {
                    // ==================================================== PREPARE
                    var catalog = RackCatalogLoader.Load();

                    foreach (var rack in plan.RackMutations)
                    {
                        var views = RackCommandSupport.FindRackBlocks(document, rack.RackId);
                        var present = new List<string>();

                        foreach (var found in views)
                        {
                            // The handle is the definition's physical name. It is read straight off the id, so
                            // naming the views costs no transaction of its own.
                            present.Add(found.BlockId.Handle.ToString());
                        }

                        // The drawing may have moved since the plan was built. Whether the difference is
                        // executable is a pure question, and it is answered before anything is written.
                        var binding = MutationDestinationBinding.Bind(rack.RackId, rack.Destinations, present);

                        if (!binding.IsBound)
                        {
                            return MutationExecutionResult.Aborted(binding.Error);
                        }

                        var name = rack.AuthoredOutput == null ? null : rack.AuthoredOutput.Name;
                        var system = new SelectiveGeometryResolver().Resolve(rack.EffectiveOutput, catalog);
                        system.Name = name;

                        // ONE serialization per rack: the design JSON is identical across the views, and only
                        // the envelope differs. Serializing per view would also promote the sticky schema once
                        // per view — the same work, done N times, on N copies of the same document.
                        var designJson = new SelectivePalletDesignStore().Serialize(rack.AuthoredOutput);

                        var fondoCount = SelectiveDepthLayout.Count(system);
                        IReadOnlyList<SelectiveCorte> cortes = null;

                        foreach (var view in views)
                        {
                            var embed = view.Embed;
                            var isLateral = embed != null && string.Equals(
                                embed.View, RackEmbedDocument.ViewLateral, StringComparison.OrdinalIgnoreCase);
                            var isPlanta = RackCommandSupport.IsPlantaView(embed);

                            // A legacy frontal block with Section = -1 draws fondo 0 (same reading as the editor).
                            var fondo = embed != null && embed.Section >= 0 ? embed.Section : 0;
                            SelectiveCorte corte = null;

                            if (isLateral)
                            {
                                cortes = cortes ?? new SelectiveLateralBuilder().Cortes(system, catalog);
                                corte = FindCorte(cortes, embed.Section);

                                if (corte == null)
                                {
                                    // The editor erases views a shrink orphaned; a propagation must not. It was
                                    // asked to give every view the same value, and a view it cannot draw is a
                                    // view that would keep the old one.
                                    return MutationExecutionResult.Aborted(Orphan(rack.RackId, "lateral", embed.Section));
                                }
                            }
                            else if (!isPlanta && fondo >= fondoCount)
                            {
                                return MutationExecutionResult.Aborted(Orphan(rack.RackId, "frontal", fondo));
                            }

                            var payload = new RackEmbedStore().Serialize(
                                RackEmbedComposer.Compose(
                                    view.Embed,
                                    RackEmbedDocument.KindSelective,
                                    rack.RackId,
                                    name,
                                    isLateral
                                        ? RackEmbedDocument.ViewLateral
                                        : isPlanta ? RackEmbedDocument.ViewPlanta : RackEmbedDocument.ViewFrontal,
                                    isPlanta ? -1 : isLateral ? embed.Section : fondo,
                                    designJson));

                            if (isLateral)
                            {
                                var lateral = new LateralHeaderDrawService();
                                prepared.Add(new PreparedDestination(
                                    lateral.PrepareRedraw(
                                        database, view.BlockId, corte.Cabecera, payload, corte.Largueros),
                                    lateral.RedrawInTransaction));
                            }
                            else if (isPlanta)
                            {
                                var planta = new SelectivePlantaDrawService();
                                prepared.Add(new PreparedDestination(
                                    planta.PrepareRedraw(database, view.BlockId, system, payload),
                                    planta.RedrawInTransaction));
                            }
                            else
                            {
                                var frontal = new SelectiveFrontalDrawService();
                                var fondoView = SelectiveDepthLayout.FondoSystemView(system, fondo);
                                fondoView.Name = name;
                                prepared.Add(new PreparedDestination(
                                    frontal.PrepareRedraw(database, view.BlockId, fondoView, payload),
                                    frontal.RedrawInTransaction));
                            }
                        }
                    }

                    // ==================================================== MUTATE
                    var stale = new List<ObjectId>();
                    var registryWritten = false;

                    using (var transaction = database.TransactionManager.StartTransaction())
                    {
                        if (plan.RegistryMutation.Kind != RegistryMutationKind.None)
                        {
                            var lastRead = ProjectVariablesRegistry.Read(transaction, database);

                            if (!ProjectVariablesRegistry.TryWrite(
                                    transaction,
                                    database,
                                    lastRead,
                                    plan.RegistryMutation.ApplyTo(lastRead.Document),
                                    out var registryError))
                            {
                                // Leaving without confirming: the transaction unwinds and the drawing is
                                // untouched — including every view that was already redefined in it.
                                return MutationExecutionResult.Aborted(registryError);
                            }

                            registryWritten = true;
                        }

                        foreach (var destination in prepared)
                        {
                            destination.Write(
                                database, transaction, destination.Prepared, out var staleDefinitions);

                            if (staleDefinitions != null)
                            {
                                stale.AddRange(staleDefinitions);
                            }
                        }

                        transaction.Commit();
                    }

                    // ==================================================== POST
                    SystemBlockWriter.PurgeAfterCommit(database, stale);
                    SystemBlockWriter.ApplyRegen(document, prepared.Count > 0);

                    return MutationExecutionResult.Applied(
                        plan.RackMutations.Count, prepared.Count, registryWritten);
                }
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("Ejecutar mutacion de variables de proyecto", ex);
                return MutationExecutionResult.Aborted(ex.Message);
            }
        }

        private static SelectiveCorte FindCorte(IReadOnlyList<SelectiveCorte> cortes, int postIndex)
        {
            foreach (var corte in cortes)
            {
                if (corte != null && corte.PostIndex == postIndex)
                {
                    return corte;
                }
            }

            return null;
        }

        private static string Orphan(string rackId, string view, int section)
            => "El rack " + rackId + " tiene una vista " + view + " (" + section.ToString(
                   System.Globalization.CultureInfo.InvariantCulture) +
               ") que el diseño resultante ya no tiene, así que se quedaría con el valor anterior. " +
               "La operación se cancela sin tocar nada.";
    }
}
