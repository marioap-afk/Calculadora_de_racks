using System;
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
    public sealed class RackCadClassicCommandPort : IRackCadClassicCommandPort, IRackCadUtilityCommandPort
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

        // ------------------------------------------------------------------ T4 ut: «Ejecutar» of the help and a design of the library

        /// <summary>A design of the library that waits for its command (RACKBIBLIOTECAABRIR): the number, the file and the drawing it is for.</summary>
        internal sealed record PendingLibraryDesign(long RequestId, string Path, long Document);

        private static PendingLibraryDesign _pendingLibraryDesign;

        /// <summary>«Ejecutar» of «Órdenes y atajos» (UT-10): only a command of the list, by the same rules as <see cref="Run"/>.</summary>
        public RackCadClassicResult RunHelpCommand(string command)
        {
            var text = RackCadHelpCommands.TextOf(command);
            if (text == null)
                return RackCadClassicResult.Without(RackCadClassicStatus.Failed, "Esa orden no se ejecuta desde la ayuda de la ventana.");
            return Send(text, null, null);
        }

        /// <summary>
        /// «Abrir en el editor clásico…» of the library (UT-03): the request is kept for the active drawing BEFORE the command is handed over
        /// (AutoCAD may run it at once), and a newer request replaces an older one that never ran; the command takes it once.
        /// </summary>
        public RackCadClassicResult OpenLibraryDesign(RackCadLibraryDesignRequest request)
        {
            var refusal = RackCadLibraryDoorRules.RefusalOf(request);
            if (refusal != null) return RackCadClassicResult.Without(RackCadClassicStatus.Failed, refusal);
            return Send(
                RackCadLibraryDoorRules.Command + " ",
                document => _pendingLibraryDesign = new PendingLibraryDesign(request.RequestId, request.Path, RackCadRackDoorPort.KeyOf(document)),
                () => _pendingLibraryDesign = null);
        }

        /// <summary>The design of the library that waits, taken once by its command; null when there is none.</summary>
        internal static PendingLibraryDesign TakePendingLibraryDesign()
        {
            var pending = _pendingLibraryDesign;
            _pendingLibraryDesign = null;
            return pending;
        }

        // The rules of Run, with a request kept before the command is handed over and dropped if handing it over fails.
        private static RackCadClassicResult Send(string text, Action<Autodesk.AutoCAD.ApplicationServices.Document> keep, Action drop)
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                    return RackCadClassicResult.Without(RackCadClassicStatus.NoDocument, "No hay un dibujo activo.");
                if (!string.IsNullOrEmpty(document.CommandInProgress) || !document.Editor.IsQuiescent)
                    return RackCadClassicResult.Without(RackCadClassicStatus.Busy, "Hay un comando en curso.");

                keep?.Invoke(document);
                try
                {
                    document.SendStringToExecute(text, true, false, false);
                }
                catch
                {
                    drop?.Invoke();
                    throw;
                }

                ReturnFocusToDrawing(document);
                return RackCadClassicResult.Started();
            }
            catch (System.Exception ex)
            {
                RackLog.Exception("RackCadClassicCommandPort.Send", ex);
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
