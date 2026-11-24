using RackCad.Application.Diagnostics;
using RackCad.Application.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// The door of the RackCad window to the classic flows (I-64 A-14, phase 1). It hands the command of the action to AutoCAD with the
    /// public API of the document, only when there is an active document and no command is in progress, so the text never lands inside
    /// another command; AutoCAD runs it when it takes it, in its own command context, where the editors open as they always did. It reads
    /// and writes nothing of the drawing, holds no AutoCAD object and contains every failure as a value.
    /// </summary>
    public sealed class RackCadClassicCommandPort : IRackCadClassicCommandPort
    {
        public RackCadClassicResult Run(RackCadClassicAction action)
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                    return RackCadClassicResult.Without(RackCadClassicStatus.NoDocument, "No hay un dibujo activo.");
                if (!string.IsNullOrEmpty(document.CommandInProgress) || !document.Editor.IsQuiescent)
                    return RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");

                document.SendStringToExecute(RackCadClassicCommands.TextOf(action), true, false, false);
                ReturnFocusToDrawing(document);
                return RackCadClassicResult.Started();
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadClassicCommandPort.Run", ex);
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, ex.Message);
            }
        }

        // The palette keeps the focus (KeepFocus); the command and its prompts or jig belong to the drawing, so the focus goes back to it,
        // with the public window API of the document. A failure is contained: the command was already handed to AutoCAD.
        internal static void ReturnFocusToDrawing(Autodesk.AutoCAD.ApplicationServices.Document document)
        {
            try
            {
                document?.Window.Focus();
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadClassicCommandPort.ReturnFocusToDrawing", ex);
            }
        }
    }
}
