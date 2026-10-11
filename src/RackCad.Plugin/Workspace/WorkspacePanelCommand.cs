using Autodesk.AutoCAD.Runtime;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// RACKPANEL (I-64 F1-T3A): the one command of the workspace panel, with no alias. In this task it only creates or
    /// obtains the single workspace controller and opens the panel state through it, which starts the event bridge lazily and
    /// asks for the first reconciliation. It carries no rule of its own, builds no palette and no WPF (T3B), and reads and
    /// writes nothing of the drawing.
    /// </summary>
    public sealed class WorkspacePanelCommand
    {
        [CommandMethod("RACKPANEL")]
        public void RackPanel()
        {
            try
            {
                WorkspaceController.GetOrCreate().OpenPanel();
                AcApplication.DocumentManager.MdiActiveDocument?.Editor.WriteMessage(
                    "\nRackCad: panel de trabajo iniciado (esta version aun no muestra la interfaz).");
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }
    }
}
