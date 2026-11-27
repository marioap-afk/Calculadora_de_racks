using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.Systems.Larguero;
using RackCad.Application.VentanaRackCad;
using RackCad.Plugin.Drawing.StructuralSections;
using RackCad.Plugin.Systems.Shared;
using RackCad.Plugin.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// T6 lg (I-64, sexta tanda; the Owner, points 2 and 6: «Pieza suelta» and «Sí, una sola página»): the door of the page «Larguero» of the
    /// RackCad window. <b>RACKLARGUEROINSERTAR</b> takes the request that waits in the door (<see cref="RackCadRackDoorPort"/>) and inserts its
    /// plan as a loose block, as RACKSECCION inserts a section: it asks the point FIRST (a cancelled point creates nothing), then the definition
    /// and the reference in ONE transaction, so one «U» undoes it, and one regeneration. A bare AISC profile is materialized by the adapter of
    /// RACKSECCION itself (<see cref="StructuralSectionMaterializer"/>), so it is the same block RACKSECCION draws; a larguero by
    /// <see cref="LargueroPieceMaterializer"/>. The command computes no geometry: the plan is the one the page drew. Without a request, or for
    /// a request of another drawing, it inserts nothing. RACKSECCION and its window stay as they are (the classic stays).
    /// </summary>
    public sealed class RackLargueroCommands
    {
        [CommandMethod("RACKLARGUEROINSERTAR")]
        public void RackLargueroInsertar()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingLargueroInsert();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKLARGUEROINSERTAR la usa la ventana RackCad: no hay ninguna pieza pendiente de insertar.");
                return;
            }

            door?.Started(pending.RequestId);
            var request = pending.Request;
            var plan = request.Plan;
            string blockName;
            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerLargueroInsert(new LargueroInserted(request.RequestId, false, LargueroDoorRules.OtherDrawingText));
                    return;
                }

                var refusal = LargueroDoorRules.RefusalOf(request);
                if (refusal != null)
                {
                    door?.AnswerLargueroInsert(new LargueroInserted(request.RequestId, false, refusal));
                    return;
                }

                // I-05: an insertion path warns once, before it asks for anything. The plan is in inches and nothing converts it.
                RackUnitsGuard.WarnIfNotInches(document);

                // The point FIRST: a cancelled point leaves nothing behind (no definition without a reference to clean up).
                var point = document.Editor.GetPoint(new PromptPointOptions(LargueroDoorRules.PointPrompt(plan)) { AllowNone = false });
                if (point.Status != PromptStatus.OK)
                {
                    door?.AnswerLargueroInsert(LargueroDoorRules.Cancelled(request.RequestId));
                    return;
                }

                using (document.LockDocument())
                {
                    using (var transaction = document.Database.TransactionManager.StartTransaction())
                    {
                        var definitionId = plan.Kind == LargueroPieceKind.Profile
                            ? StructuralSectionMaterializer.CreateBlockDefinition(document.Database, transaction, plan.SectionPlan, out blockName)
                            : LargueroPieceMaterializer.CreateBlockDefinition(document.Database, transaction, plan, out blockName);
                        StructuralSectionMaterializer.InsertReference(document.Database, transaction, definitionId, point.Value);
                        transaction.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                // The transaction was not committed: no half-defined block, no orphan reference.
                RackCommandSupport.Report(ex);
                door?.AnswerLargueroInsert(LargueroDoorRules.Failed(request.RequestId, ex.Message));
                return;
            }

            // From here the block IS in the drawing: whatever fails now, the answer says it was inserted.
            try
            {
                using (document.LockDocument())
                {
                    SystemBlockWriter.ApplyRegen(document, regen: true);
                }
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackLargueroCommands.RackLargueroInsertar (regenerar)", ex);
            }

            door?.AnswerLargueroInsert(LargueroDoorRules.Inserted(request.RequestId, plan, blockName));
            try
            {
                document.Editor.WriteMessage("\n" + LargueroDoorRules.CommandLineText(plan, blockName));
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackLargueroCommands.RackLargueroInsertar (línea de órdenes)", ex);
            }
        }
    }
}
