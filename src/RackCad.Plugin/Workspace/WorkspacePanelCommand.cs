using Autodesk.AutoCAD.Runtime;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// RACKPANEL (I-64 F1-T3A, T3B): the one command of the workspace panel, with no alias. It obtains the single workspace
    /// controller and opens the panel state through it (which starts the event bridge lazily and asks for the first
    /// reconciliation), then asks the single palette host to show the palette. It carries no rule of its own and no palette
    /// logic, and reads and writes nothing of the drawing.
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
    }
}
