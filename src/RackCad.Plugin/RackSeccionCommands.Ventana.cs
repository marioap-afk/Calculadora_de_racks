using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.VentanaRackCad;
using RackCad.Plugin.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// T7 per (I-64, séptima tanda; the Owner, 2026-11-27: «Igual que la clásica»): the door of the page «Perfil estructural» of the RackCad
    /// window. <b>RACKSECCIONINSERTAR</b> takes the request that waits in the door (<see cref="RackCadRackDoorPort"/>) and draws its section
    /// with the materialisation of RACKSECCION itself (<see cref="StructuralSectionCommandFlow.Place"/>): the units advisory, the point asked
    /// FIRST (a cancelled point creates nothing), definition and reference in ONE transaction, so one «U» undoes it, one regeneration and the
    /// same report in the command line. The command computes no geometry: the plan is the one the page drew. Without a request, or for a
    /// request of another drawing, it inserts nothing. The drawn section stays a loose block with no rack data, as with the classic.
    /// </summary>
    public sealed partial class RackSeccionCommands
    {
        [CommandMethod("RACKSECCIONINSERTAR")]
        public void RackSeccionInsertar()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingStructuralSectionInsert();
            if (pending == null)
            {
                document.Editor.WriteMessage("\n" + StructuralSectionDoorRules.NoRequestText);
                return;
            }

            // From here what runs is the insertion of the window, never a foreign change of the drawing (T6 st).
            door?.Started(pending.RequestId);
            var request = pending.Request;
            StructuralSectionInserted answer;
            string report = null;
            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    answer = StructuralSectionDoorRules.Refused(request.RequestId, StructuralSectionDoorRules.OtherDrawingText);
                }
                else if (StructuralSectionDoorRules.RefusalOf(request) is string refusal)
                {
                    answer = StructuralSectionDoorRules.Refused(request.RequestId, refusal);
                }
                else
                {
                    var placement = StructuralSectionCommandFlow.Place(document, request.Plan);
                    report = ReportOf(placement, request);
                    answer = AnswerOf(request, placement, report);
                }
            }
            catch (System.Exception ex)
            {
                // What escapes Place came BEFORE anything was created: the units advisory or the point. Its insert contains every failure
                // from the lock on, and says when the section was already committed (review M-1). Reported as the classic and the other
                // doors report a failure (review M-2).
                RackCommandSupport.Report(ex);
                answer = StructuralSectionDoorRules.Failed(request.RequestId, ex.Message, null, false);
            }

            door?.AnswerStructuralSectionInsert(answer);

            // The same line the classic flow writes, after the answer: whatever fails now, the page already knows what happened.
            if (report == null) return;
            try
            {
                document.Editor.WriteMessage("\n" + report);
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackSeccionCommands.RackSeccionInsertar (línea de órdenes)", ex);
            }
        }

        // The report of the flow (what the classic writes): a step of its own, so a failure wording it does not change what was drawn nor
        // lose the units advisory; reported as the classic reports it (review M-1, M-2).
        private static string ReportOf(StructuralSectionPlacement placement, StructuralSectionInsertRequest request)
        {
            try
            {
                return StructuralSectionCommandFlow.Describe(placement.Outcome, request.Section, request.Plan);
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                return null;
            }
        }

        // The answer of the page for what the flow did: drawn and then a failure (after the commit), a failure before the commit (nothing
        // reached the drawing), the point cancelled, or drawn.
        private static StructuralSectionInserted AnswerOf(StructuralSectionInsertRequest request, StructuralSectionPlacement placement, string report)
        {
            var outcome = placement.Outcome;
            if (!outcome.Success && outcome.Placed) return StructuralSectionDoorRules.DrawnButFailed(request.RequestId, request.Section, outcome.BlockName,
                outcome.ErrorMessage, report, placement.UnitsWarned);
            if (!outcome.Success) return StructuralSectionDoorRules.Failed(request.RequestId, outcome.ErrorMessage, report, placement.UnitsWarned);
            if (!outcome.Placed) return StructuralSectionDoorRules.Cancelled(request.RequestId, report, placement.UnitsWarned);
            return StructuralSectionDoorRules.Inserted(request.RequestId, request.Section, outcome.BlockName, report, placement.UnitsWarned);
        }
    }
}
