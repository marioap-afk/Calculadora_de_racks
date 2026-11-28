using System.IO;
using Autodesk.AutoCAD.Runtime;
using RackCad.UI.Editor;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// The door of the RackCad window to a design of its library in the classic editor (I-64 A-14, fourth batch, part «ut», UT-03): the
    /// systems that are not edited in the window yet (Push Back, Cantilever, cabecera, cama, larguero) open their classic editor, apart and
    /// already loaded with the design the user chose in «Biblioteca de diseños», without the list of RACKBIBLIOTECA. It is RACKBIBLIOTECA
    /// with the choice made: the same modal owner, the same module of the registry, the same dispatch of the menu
    /// (<see cref="RackMenuCommands.DispatchInsertion"/>), and the design always opens as a NEW rack.
    /// </summary>
    public sealed class RackBibliotecaCommands
    {
        /// <summary>
        /// RACKBIBLIOTECAABRIR: takes the request the window kept (its number, its file and the drawing that was active) and asks nothing on
        /// the command line. Typed by hand with no request, or run in another drawing, it only says so and opens nothing.
        /// </summary>
        [CommandMethod("RACKBIBLIOTECAABRIR")]
        public void RackBibliotecaAbrir()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null) return;

                var pending = VentanaRackCad.RackCadClassicCommandPort.TakePendingLibraryDesign();
                var refusal = global::RackCad.Application.VentanaRackCad.RackCadLibraryDoorRules.RefusalWhenRun(
                    pending != null,
                    pending != null && pending.Document == VentanaRackCad.RackCadRackDoorPort.KeyOf(document),
                    pending != null && File.Exists(pending.Path));
                if (refusal != null)
                {
                    document.Editor.WriteMessage("\nRackCad: " + refusal);
                    return;
                }

                var styles = RackCommandSupport.ReadDimensionStyleNames(document);
                var host = new global::RackCad.UI.VentanaRackCad.RackModalHostWindow(
                    owner => global::RackCad.UI.VentanaRackCad.RackCadLibraryFlows.OpenDesign(owner, pending.Path, styles));
                AcApplication.ShowModalWindow(host.Window);
                host.ThrowIfFailed();

                RackMenuCommands.DispatchInsertion(document, host.Result as RackInsertionRequest);
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }
    }
}
