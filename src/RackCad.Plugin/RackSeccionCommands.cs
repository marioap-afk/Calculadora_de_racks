using System.Globalization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
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
    /// RACKSECCION: pick a catalogued section, give it a length, look at it and drop it in the drawing.
    ///
    /// The command is deliberately thin. It loads the catalogue, opens the inspector, and hands the plan the
    /// user accepted to the materializer. It computes NO geometry: the picture the user approved in the preview
    /// and the entities that reach the drawing come from the very same plan object (ADR-0022 §7), so they cannot
    /// disagree.
    ///
    /// It draws a section, not a member. There is no role, no system, no rack payload and no round-trip: the
    /// result is plain geometry the user can measure and copy. I-37 is what turns a section into a member.
    /// </summary>
    public sealed class RackSeccionCommands
    {
        [CommandMethod("RACKSECCION")]
        public void RackSeccion()
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;

            if (document == null)
            {
                return;
            }

            var editor = document.Editor;

            try
            {
                // FAIL CLOSED. Unlike the product catalogue — which degrades to empty so a draw still runs —
                // an invalid section catalogue means the dimensions are not trustworthy, and drawing a beam
                // from data that failed validation is worse than not drawing it (I-36A F5).
                StructuralSectionCatalog catalog;

                try
                {
                    catalog = new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();
                }
                catch (StructuralSectionCatalogException ex)
                {
                    editor.WriteMessage("\nRackCad: el catalogo de secciones estructurales no es valido, " +
                                        "asi que no se dibujara nada. " + ex.Message);
                    return;
                }

                var window = new StructuralSectionInspectorWindow(catalog);
                AcApplication.ShowModalWindow(window);

                if (window.Result == null)
                {
                    return;
                }

                // I-05: warn once if the drawing is not in inches. The plan is in inches and nothing converts it.
                RackUnitsGuard.WarnIfNotInches(document);

                var outcome = StructuralSectionInsertService.Insert(document, window.Result);
                editor.WriteMessage("\n" + Describe(outcome, window.AcceptedSection, window.Result));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
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
            Autodesk.AutoCAD.ApplicationServices.Document document,
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
