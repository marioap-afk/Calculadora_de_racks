using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// RACKVARIABLES — the drawing's project variables, administered from product (I-47 G16).
    ///
    /// <para>
    /// This is the permanent surface that had been missing since G7: the register existed, the semantics
    /// existed and the executor existed, and there was no way for a user to reach any of it. It is a product
    /// command, not a developer one — everything it does is something the user is meant to do.
    /// </para>
    /// <para>
    /// It carries no rules of its own. It reads the drawing, projects it purely, shows a window, and hands
    /// whatever the user asked for to the preflight that already knows what each operation may do and to the
    /// executor that already knows how to write it. Between those two, this file decides nothing.
    /// </para>
    /// <para>
    /// After every operation it re-reads the drawing. The window is not the authority and never holds a model
    /// that outlives one write: the DWG is the source of truth, so the loop goes back to it every time.
    /// </para>
    /// </summary>
    public sealed class RackVariablesCommands
    {
        [CommandMethod("RVA")] public void AliasRackVariables() => RackVariables();     // RACKVARIABLES

        [CommandMethod("RACKVARIABLES")]
        public void RackVariables()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;

                if (document == null)
                {
                    return;
                }

                var editor = document.Editor;

                while (true)
                {
                    // Re-read every round: the drawing is the authority, and the previous operation changed it.
                    var workspace = Read(document);
                    var window = new RackProjectVariablesWindow(workspace);
                    AcApplication.ShowModalWindow(window);

                    if (window.Intent == null)
                    {
                        return;
                    }

                    var preflight = ProjectVariableIntentPreflight.Run(
                        window.Intent, LastRegistry, LastEntries);

                    if (!preflight.IsSuccess)
                    {
                        editor.WriteMessage("\nRackCad: " + preflight.Error);

                        foreach (var consumer in preflight.BlockingConsumers)
                        {
                            editor.WriteMessage(
                                "\n  · " + (string.IsNullOrWhiteSpace(consumer.RackName) ? "(sin nombre)" : consumer.RackName)
                                + " [" + consumer.RackId + "] " + string.Join(", ", consumer.PropertyIds));
                        }

                        continue;
                    }

                    var execution = ProjectVariableMutationExecutor.Execute(document, preflight.Plan);

                    editor.WriteMessage(execution.IsApplied
                        ? "\nRackCad: operación aplicada (racks redibujados: "
                          + execution.ViewsRedrawn.ToString(System.Globalization.CultureInfo.InvariantCulture) + ")."
                        : "\nRackCad: " + (execution.Error ?? "no había nada que aplicar."));
                }
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        /// <summary>The register snapshot the last projection was built from — what the preflight reasons over.</summary>
        private ProjectVariablesDocument LastRegistry { get; set; }

        /// <summary>The drawing sweep the last projection was built from.</summary>
        private IReadOnlyList<ProjectVariableScanEntry> LastEntries { get; set; }

        /// <summary>
        /// One read of the drawing: the register and the sweep, in ONE transaction, projected purely. The
        /// window never sees a <c>Database</c>, a <c>Transaction</c> or an <c>ObjectId</c> — and it cannot,
        /// because what leaves here is already a value.
        /// </summary>
        private ProjectVariablesWorkspace Read(Autodesk.AutoCAD.ApplicationServices.Document document)
        {
            ProjectVariablesReadResult registry;
            var entries = new List<ProjectVariableScanEntry>();

            using (document.LockDocument())
            using (var transaction = document.Database.TransactionManager.StartTransaction())
            {
                registry = ProjectVariablesRegistry.Read(transaction, document.Database);

                foreach (var envelope in RackBlockFinder.ScanEnvelopes(
                             transaction, document.Database, includeReferenceCount: true))
                {
                    entries.Add(ProjectVariableScanProjection.Project(
                        envelope.DefinitionId.Handle.ToString(), envelope.Embed, envelope.DirectReferenceCount));
                }

                transaction.Commit();
            }

            LastRegistry = registry.Document;
            LastEntries = entries;

            return ProjectVariablesWorkspace.Build(registry, entries);
        }
    }
}
