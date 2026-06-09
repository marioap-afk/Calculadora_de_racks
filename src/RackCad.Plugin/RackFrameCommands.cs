using Autodesk.AutoCAD.Runtime;
using RackCad.Application.RackFrames;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    public sealed class RackFrameCommands
    {
        [CommandMethod("RACKCABECERA")]
        public void RackCabecera()
        {
            try
            {
                var standardService = new HardcodedStandardRackFrameService();
                var configuration = standardService.CreateDefault();
                var window = new RackFrameConfiguratorWindow(configuration);

                AcApplication.ShowModalWindow(window);
            }
            catch (System.Exception ex)
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                document?.Editor.WriteMessage("\nRACKCABECERA error: " + ex.Message);
            }
        }
    }
}
