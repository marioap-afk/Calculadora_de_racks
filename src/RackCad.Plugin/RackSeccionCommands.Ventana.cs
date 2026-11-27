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
                    var placement = StructuralSectionCommandFlow.Place(document, request.Section, request.Plan);
                    report = placement.Report;
                    answer = AnswerOf(request, placement);
                }
            }
            catch (System.Exception ex)
            {
                // Place contains its own failures (an uncommitted transaction): what reaches here left nothing in the drawing.
                RackLog.Exception("RackSeccionCommands.RackSeccionInsertar", ex);
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

        // The answer of the page for what the flow did: drawn, cancelled at the point, or failed (nothing committed).
        private static StructuralSectionInserted AnswerOf(StructuralSectionInsertRequest request, StructuralSectionPlacement placement)
        {
            var outcome = placement.Outcome;
            if (!outcome.Success) return StructuralSectionDoorRules.Failed(request.RequestId, outcome.ErrorMessage, placement.Report, placement.UnitsWarned);
            if (!outcome.Placed) return StructuralSectionDoorRules.Cancelled(request.RequestId, placement.Report, placement.UnitsWarned);
            return StructuralSectionDoorRules.Inserted(request.RequestId, request.Section, outcome.BlockName, placement.Report, placement.UnitsWarned);
        }
    }
}
