using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Workspace;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// The two commands of the workspace panel, with no flags and no alias.
    ///
    /// RACKPANEL (I-64 F1-T3A, T3B): it obtains the single workspace controller and opens the panel state through it (which starts the
    /// event bridge lazily and asks for the first reconciliation), then asks the single palette host to show the palette. It carries no
    /// rule of its own and no palette logic, and reads and writes nothing of the drawing.
    ///
    /// RACKPANELDESCARTAR (I-64 F4-T3C2): the explicit discard of the drafts of the panel of the active document. It never creates the
    /// controller: without one it only says so. Every decision and every text lives in Application
    /// (<see cref="WorkspaceDocumentDiscardCommand"/>); this adapter only supplies the delegates, the question on the command line and
    /// the one writer of the command line of the panel. It opens no dialog and no window and writes nothing of the drawing.
    /// </summary>
    public sealed class WorkspacePanelCommand
    {
        [CommandMethod("RACKPANEL")]
        public void RackPanel()
        {
            try
            {
                WorkspaceController.GetOrCreate().OpenPanel();
                WorkspacePaletteHost.GetOrCreate().Show();
                AcApplication.DocumentManager.MdiActiveDocument?.Editor.WriteMessage(
                    "\nRackCad: panel Workspace mostrado.");
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        [CommandMethod("RACKPANELDESCARTAR")]
        public void RackPanelDiscard()
        {
            try
            {
                var controller = WorkspaceController.Existing;
                WorkspaceDocumentDiscardCommand.Run(
                    controller?.PilotEdit,
                    () => controller?.Port.GetSnapshot().ActiveSession,
                    session => controller.Port.Authorize(new WorkspaceRequest(session)) == RequestDecision.Accepted,
                    ConfirmOnCommandLine,
                    WriteToCommandLine);
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }

        // The question of the command: yes or no on the command line, with No as the default. Enter, Escape and cancel all mean no.
        private static bool ConfirmOnCommandLine(string question)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return false;

            var options = new PromptKeywordOptions("\n" + question);
            options.Keywords.Add("Si");
            options.Keywords.Add("No");
            options.Keywords.Default = "No";
            options.AllowNone = true;
            var answer = document.Editor.GetKeywords(options);
            return answer.Status == PromptStatus.OK && string.Equals(answer.StringResult, "Si", System.StringComparison.Ordinal);
        }

        /// <summary>
        /// The one writer of the command line of the panel: the command and the controller use it, so no other file of the Plugin names the
        /// writing API. Without an active document it writes nothing.
        /// </summary>
        internal static void WriteToCommandLine(string text)
        {
            AcApplication.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\n" + text);
        }
    }
}
