using Autodesk.AutoCAD.Runtime;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// RACKSECCION: pick a catalogued section, give it a length, look at it and drop it in the drawing.
    ///
    /// The command is a thin entry point over <see cref="StructuralSectionCommandFlow"/>, which is also what
    /// the main menu's "Generar perfil estructural" button runs. Two doors, one room: the catalogue load, the
    /// inspector, the insertion, the units advisory and the fidelity message live there and nowhere else.
    /// </summary>
    public sealed class RackSeccionCommands
    {
        [CommandMethod("RACKSECCION")]
        public void RackSeccion()
        {
            try
            {
                StructuralSectionCommandFlow.Run(AcApplication.DocumentManager.MdiActiveDocument);
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }
    }
}
