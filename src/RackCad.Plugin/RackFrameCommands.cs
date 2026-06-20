using Autodesk.AutoCAD.Runtime;
using RackCad.Application.RackFrames;
using RackCad.UI;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    public sealed class RackFrameCommands
    {
        /// <summary>Main entry point: opens the menu where the user picks what to design.</summary>
        [CommandMethod("RACKCAD")]
        public void RackCad()
        {
            try
            {
                AcApplication.ShowModalWindow(new RackMainMenuWindow());
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        /// <summary>Shortcut straight to the header configurator module.</summary>
        [CommandMethod("RACKCABECERA")]
        public void RackCabecera()
        {
            try
            {
                var configuration = new HardcodedStandardRackFrameService().CreateDefault();
                AcApplication.ShowModalWindow(new RackFrameConfiguratorWindow(configuration));
            }
            catch (System.Exception ex)
            {
                Report(ex);
            }
        }

        private static void Report(System.Exception ex)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            document?.Editor.WriteMessage("\nRackCad error: " + ex.Message);
        }
    }
}
