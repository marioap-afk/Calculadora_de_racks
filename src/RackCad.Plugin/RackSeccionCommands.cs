using Autodesk.AutoCAD.Runtime;
using RackCad.Application.VentanaRackCad;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// RACKSECCION: pick a catalogued section, give it a length, look at it and drop it in the drawing.
    ///
    /// T7 per (I-64, séptima tanda; the Owner, 2026-11-27: «Abren la ventana»): the command opens the page «Perfil estructural» of the RackCad
    /// window, which shows the inspector of sections without blocking AutoCAD, as RACKVARIABLES opens «Variables» (the pattern of
    /// <see cref="RackCadWindowShortcuts"/>): it reads nothing of the drawing and writes nothing, so it leaves no undo step. The classic
    /// inspector stays in the classic menu: «Generar perfil estructural» of RACKMENU runs <see cref="StructuralSectionCommandFlow"/>, the one
    /// room where the catalogue load, the inspector, the insertion, the units advisory and the fidelity message live. The page inserts through
    /// RACKSECCIONINSERTAR (<c>RackSeccionCommands.Ventana.cs</c>), with the materialisation of that same flow.
    /// </summary>
    public sealed partial class RackSeccionCommands
    {
        [CommandMethod("RACKSECCION", RackCadWindowShortcuts.WindowFlags)]
        public void RackSeccion()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    return;
                }

                RackCadWindowShortcuts.Open(document, new RackCadShortcutAsk(RackCadShortcutTarget.StructuralSection, null, null));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }
    }
}
