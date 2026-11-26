using System.Collections.Generic;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
using RackCad.Application.ProjectVariables;
using RackCad.Application.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// The two commands of the page «Variables» of the RackCad window (I-64 A-14, cuarta tanda, va). The page starts them; each takes the
    /// request that waits in the door (<see cref="RackCadRackDoorPort"/>) and answers with its number; without a request, or for another
    /// drawing, it does nothing.
    /// <list type="bullet">
    /// <item><b>RACKVARIABLESLEER</b> only reads (no undo marker, no history): the register and the sweep of the racks, as RACKVARIABLES reads
    /// them in one transaction (<see cref="RackSelectivoCommands.ReadProjectVariables"/>), and the custom properties of the project.</item>
    /// <item><b>RACKVARIABLESAPLICAR</b> reads all of it AGAIN, plans the whole draft purely (<see cref="VariablesDoorRules.PlanWrite"/>: the
    /// register and the project collection must be the ones the page read) and writes it: the register and every rack that changes in ONE
    /// execution of <see cref="ProjectVariableMutationExecutor"/>, then the project collection. It is one command, so one «U» undoes it all
    /// (VA-02).</item>
    /// </list>
    /// </summary>
    public sealed class RackCadVariablesDoorCommands
    {
        private const string OtherDrawingText = "La orden corrió en otro dibujo que el de la página: no se hizo nada. Vuelve a ese dibujo e inténtalo de nuevo.";

        [CommandMethod("RACKVARIABLESLEER", CommandFlags.NoUndoMarker | CommandFlags.NoHistory)]
        public void ReadVariables()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingVariablesRead();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKVARIABLESLEER la usa la ventana RackCad: no hay ninguna lectura pendiente.");
                return;
            }

            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerVariablesRead(VariablesDoorRead.Refused(pending.RequestId, OtherDrawingText));
                    return;
                }

                var registry = RackSelectivoCommands.ReadProjectVariables(document, out var entries);
                var properties = CustomPropertiesExecutor.ReadProject(document).Collection;
                door?.AnswerVariablesRead(new VariablesDoorRead(pending.RequestId, true, registry, entries, properties, null));
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadVariablesDoorCommands.ReadVariables", ex);
                door?.AnswerVariablesRead(VariablesDoorRead.Refused(pending.RequestId, "No se pudieron leer las variables: " + ex.Message));
            }
        }

        [CommandMethod("RACKVARIABLESAPLICAR")]
        public void WriteVariables()
        {
            var door = RackCadRackDoorPort.Current;
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;

            var pending = RackCadRackDoorPort.TakePendingVariablesWrite();
            if (pending == null)
            {
                document.Editor.WriteMessage("\nRACKVARIABLESAPLICAR la usa la ventana RackCad: no hay ningún borrador pendiente de escribir.");
                return;
            }

            door?.Started(pending.RequestId);

            try
            {
                if (pending.Document != RackCadRackDoorPort.KeyOf(document))
                {
                    door?.AnswerVariablesWrite(new VariablesDoorWritten(pending.RequestId, false, OtherDrawingText));
                    return;
                }

                var write = pending.Write;
                var registry = RackSelectivoCommands.ReadProjectVariables(document, out var entries);
                var properties = CustomPropertiesExecutor.ReadProject(document).Collection;
                var plan = VariablesDoorRules.PlanWrite(registry, entries, properties, write);
                if (!plan.CanWrite)
                {
                    door?.AnswerVariablesWrite(new VariablesDoorWritten(pending.RequestId, false, plan.Reason));
                    return;
                }

                var variablesWritten = false;
                var views = 0;
                if (!plan.Variables.IsEmpty)
                {
                    var execution = ProjectVariableMutationExecutor.Execute(document, plan.Variables);
                    if (!execution.IsApplied)
                    {
                        door?.AnswerVariablesWrite(new VariablesDoorWritten(pending.RequestId, false,
                            "No se escribió nada: " + (execution.Error ?? "no había nada que aplicar.")));
                        return;
                    }

                    variablesWritten = true;
                    views = execution.ViewsRedrawn;
                }

                // The second transaction (VA-S7), with its own try: the register and the racks may be confirmed already, and a failure
                // here is said apart from them, never as «No se pudo escribir» (correction 2 of the review).
                string propertiesError = null;
                if (plan.ProjectProperties != null)
                {
                    try
                    {
                        var projectWrite = CustomPropertiesExecutor.ExecuteProjectDraft(document, write.PropertiesFingerprint, write.ProjectProperties);
                        if (!projectWrite.IsWritten) propertiesError = string.IsNullOrWhiteSpace(projectWrite.Error) ? "no se pudieron escribir." : projectWrite.Error;
                    }
                    catch (System.Exception ex)
                    {
                        RackLog.Exception("RackCadVariablesDoorCommands.WriteVariables (propiedades del proyecto)", ex);
                        propertiesError = ex.Message;
                    }
                }

                var answer = VariablesDoorRules.AnswerForWrite(pending.RequestId, variablesWritten,
                    variablesWritten ? plan.RedrawnRackIds : new List<string>(), views, plan.ProjectProperties != null && propertiesError == null,
                    propertiesError);
                document.Editor.WriteMessage("\nRackCad: " + answer.Message);
                door?.AnswerVariablesWrite(answer);
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
                door?.AnswerVariablesWrite(new VariablesDoorWritten(pending.RequestId, false, "No se pudo escribir: " + ex.Message));
            }
        }
    }
}
