using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Diagnostics;

namespace RackCad.Plugin
{
    public sealed class PluginInitializer : IExtensionApplication
    {
        public void Initialize()
        {
            RackCadLogger.Configure();
            RackCadLogger.Information("RackCad AutoCAD plugin initialized.");
        }

        public void Terminate()
        {
            RackCadLogger.Information("RackCad AutoCAD plugin terminated.");
            RackCadLogger.CloseAndFlush();
        }
    }
}
