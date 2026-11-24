using System.Windows;
using RackCad.UI.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.VentanaRackCad
{
    /// <summary>
    /// The classic dialogs that the RackCad window reuses, shown as modals of AutoCAD (step 4 of Dinámico): AutoCAD owns them and waits for
    /// them, as it waits for the classic editors. The dialogs only edit copies in memory; this file reads and writes nothing of the drawing.
    /// </summary>
    public sealed class RackCadModalPort : IRackCadModalPort
    {
        public bool? ShowModal(Window dialog) => dialog == null ? false : AcApplication.ShowModalWindow(dialog);
    }
}
