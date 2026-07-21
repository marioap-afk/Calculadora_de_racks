using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>RACKAYUDA: an in-app reference of every RackCad command and its short alias, plus its own alias.</summary>
    public sealed class RackAyudaCommands
    {
        [CommandMethod("RA")]  public void AliasRackAyuda() => RackAyuda();                // RACKAYUDA

        [CommandMethod("RACKAYUDA")]
        public void RackAyuda()
        {
            try
            {
                AcApplication.ShowModalWindow(new RackCommandHelpWindow());
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }
    }
}
