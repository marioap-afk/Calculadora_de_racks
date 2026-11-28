using Autodesk.AutoCAD.Runtime;

namespace RackCad.Plugin
{
    public sealed class PluginInitializer : IExtensionApplication
    {
        public void Initialize()
        {
            // I-64 A-14, fourth batch, part «ut» (UT-01): «Abrir RackCad al iniciar AutoCAD», off by default. Off, nothing is hooked.
            VentanaRackCad.RackCadStartup.Arm();
        }

        public void Terminate()
        {
            VentanaRackCad.RackCadStartup.Disarm(); // part «ut»: the wait of the box stops with the plugin
        }
    }
}
