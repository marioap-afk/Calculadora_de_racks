using System.Globalization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Plugin.Drawing.StructuralSections;
using RackCad.Plugin.Systems.Shared;
using RackCad.UI.StructuralSections;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// The ONE structural-section use case: load the catalogue, show the inspector, materialise what the user
    /// accepted, and report it.
    ///
    /// It exists because there are now two ways in — the <c>RACKSECCION</c> command and the "Generar perfil
    /// estructural" button of the main menu — and only one of them may own the flow. Copying it into
    /// <see cref="RackMenuCommands"/> would produce two paths that agree today and drift at the first
    /// correction: the fail-closed catalogue load, the units advisory, the transaction, the regen and the
    /// fidelity message would each have to be fixed twice, and nothing would notice when only one was.
    ///
    /// It computes NO geometry. The picture the user approved in the preview and the entities that reach the
    /// drawing are the same plan object (ADR-0022 §7).
    ///
    /// It draws a SECTION, not a member: no role, no system, no rack payload and no round-trip. I-37 is what
    /// turns a section into a member.
    /// </summary>
    internal static class StructuralSectionCommandFlow
    {
        /// <summary>
        /// Runs the whole flow on <paramref name="document"/>. Safe to call with a null document.
        ///
        /// Exceptions are reported by the caller: both entry points already sit inside the command-level
        /// <see cref="RackCommandSupport.Report"/> handler, and swallowing them here would hide a failure from
        /// the log that handler writes.
        /// </summary>
        internal static void Run(Document document)
        {
            if (document == null)
            {
                return;
            }

            var editor = document.Editor;

            // FAIL CLOSED, through the ONE Plugin owner of that policy. The load lived here until a second system
            // needed the same catalogue (I-37D); it was extracted rather than copied, so the rule and its wording
            // still have a single home.
            if (!StructuralSectionCatalogAccess.TryLoad(editor, out var catalog))
            {
                return;
            }

            var window = new StructuralSectionInspectorWindow(catalog);
            AcApplication.ShowModalWindow(window);

            if (window.Result == null)
            {
                return; // cancelled: nothing was loaded into the drawing and nothing has to be undone
            }

            // I-05: warn once if the drawing is not in inches. The plan is in inches and nothing converts it.
            RackUnitsGuard.WarnIfNotInches(document);

            var outcome = StructuralSectionInsertService.Insert(document, window.Result);
            editor.WriteMessage("\n" + Describe(outcome, window.AcceptedSection, window.Result));
        }

        /// <summary>What the user reads afterwards: what was drawn, how faithful it is and what it weighs.</summary>
        private static string Describe(
            StructuralSectionInsertOutcome outcome,
            StructuralSectionDefinition section,
            StructuralSectionRepresentationPlan plan)
        {
            if (!outcome.Success)
            {
                return "RackCad: no se pudo dibujar la seccion. " + outcome.ErrorMessage;
            }

            if (!outcome.Placed)
            {
                return "RackCad: insercion cancelada; no se dejo nada en el dibujo.";
            }

            var message = string.Format(
                CultureInfo.InvariantCulture,
                "RackCad: {0} insertada como bloque '{1}'. Longitud {2} in, vista {3}, {4} curvas.",
                section.DisplayName,
                outcome.BlockName,
                plan.Length.ToString("0.###", CultureInfo.InvariantCulture),
                plan.View,
                plan.Curves.Count);

            message += "\nPeso: " + StructuralSectionLabelFormatter.FormatWeightWithDesignation(section) +
                       ", total " +
                       StructuralSectionUnits.WeightInPounds(section, plan.Length)
                           .ToString("0.#", CultureInfo.InvariantCulture) + " lb.";

            // The authority warning is UNCONDITIONAL and comes before the fidelity line: it does not depend on
            // the detail level, on the fidelity or on any setting, because a visually derived contour is
            // approximate at every level (ADR-0023 decision 22). There is no flag that turns it off.
            if (plan.IsVisualDerived)
            {
                message += "\nADVERTENCIA: geometria visual derivada (ADR-0023). La inclinacion del patin y " +
                           "el filete son una convencion de RackCad, no un dato de AISC: es aproximada, no " +
                           "esta garantizada por ningun fabricante y NO es apta para CNC ni fabricacion. " +
                           "Dimensiones, area, peso y propiedades siguen siendo los que tabula AISC.";
            }

            // The fidelity travels with the plan on purpose: a derived radius must be visible to whoever
            // measures the result, not buried in a log (owner decision 10).
            if (plan.Fidelity != SectionFidelity.TabulatedComplete)
            {
                message += "\nFidelidad: " + plan.Fidelity + ".";

                foreach (var diagnostic in plan.Diagnostics)
                {
                    message += "\n  - " + diagnostic.Message;
                }
            }

            return message;
        }
    }

    /// <summary>What an insert did. Mirrors <c>HeaderPlacementResult</c> without borrowing a rack's vocabulary.</summary>
    internal sealed class StructuralSectionInsertOutcome
    {
        private StructuralSectionInsertOutcome(bool success, bool placed, string blockName, string errorMessage)
        {
            Success = success;
            Placed = placed;
            BlockName = blockName;
            ErrorMessage = errorMessage;
        }

        /// <summary>False only when something failed; a cancelled placement is a SUCCESS that placed nothing.</summary>
        public bool Success { get; }

        public bool Placed { get; }

        public string BlockName { get; }

        public string ErrorMessage { get; }

        internal static StructuralSectionInsertOutcome Inserted(string blockName) =>
            new StructuralSectionInsertOutcome(true, true, blockName, null);

        internal static StructuralSectionInsertOutcome Cancelled() =>
            new StructuralSectionInsertOutcome(true, false, null, null);

        internal static StructuralSectionInsertOutcome Failure(string message) =>
            new StructuralSectionInsertOutcome(false, false, null, message);
    }

    /// <summary>
    /// Asks for the insertion point and materialises the plan there.
    ///
    /// The whole insert is ONE transaction: definition and reference commit together or neither exists. A
    /// committed definition with no reference is exactly the phantom block that
    /// <see cref="RackCad.Plugin.Drawing.BlockPlacement"/> has to clean up after a cancelled jig; asking for the
    /// point FIRST avoids creating the problem instead of repairing it.
    /// </summary>
    internal static class StructuralSectionInsertService
    {
        internal static StructuralSectionInsertOutcome Insert(
            Document document,
            StructuralSectionRepresentationPlan plan)
        {
            var options = new PromptPointOptions("\nPunto de insercion de la seccion: ")
            {
                AllowNone = false
            };

            var point = document.Editor.GetPoint(options);

            if (point.Status != PromptStatus.OK)
            {
                return StructuralSectionInsertOutcome.Cancelled();
            }

            try
            {
                string blockName;

                using (document.LockDocument())
                {
                    using (var transaction = document.Database.TransactionManager.StartTransaction())
                    {
                        var definitionId = StructuralSectionMaterializer.CreateBlockDefinition(
                            document.Database, transaction, plan, out blockName);

                        StructuralSectionMaterializer.InsertReference(
                            document.Database, transaction, definitionId, point.Value);

                        transaction.Commit();
                    }

                    SystemBlockWriter.ApplyRegen(document, regen: true);
                }

                return StructuralSectionInsertOutcome.Inserted(blockName);
            }
            catch (System.Exception ex)
            {
                // The transaction was not committed, so nothing partial reached the drawing: no half-defined
                // block, no orphan reference.
                return StructuralSectionInsertOutcome.Failure(ex.Message);
            }
        }
    }
}
