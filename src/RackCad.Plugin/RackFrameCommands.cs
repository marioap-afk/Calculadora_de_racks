using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;
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
            RackCadLogger.Configure();
            RackCadLogger.Information("Executing AutoCAD command RACKCABECERA.");

            try
            {
                var standardService = new HardcodedStandardRackFrameService();
                var configuration = standardService.CreateDefault();
                var window = new RackFrameConfiguratorWindow(configuration);

                AcApplication.ShowModalWindow(window);
                RackCadLogger.Information("AutoCAD command RACKCABECERA completed.");
            }
            catch (System.Exception ex)
            {
                RackCadLogger.Error(ex, "AutoCAD command RACKCABECERA failed.");
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                document?.Editor.WriteMessage("\nRACKCABECERA error: " + ex.Message);
                document?.Editor.WriteMessage("\nRackCad log: " + RackCadLogger.LogDirectory);
            }
        }
    }
}
