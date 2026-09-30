using Autodesk.AutoCAD.Runtime;
using RackCad.Application.Views.Placement;
using RackCad.Plugin.Views;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin
{
    /// <summary>
    /// RACKPROYECTAR (alias RPY) — ID19: projects selected rack views onto another class of view, as LINKED views of the
    /// same logical rack (never copies: for those there is RACKDUPLICAR).
    ///
    /// <para>
    /// The command carries no rules. It gives the AutoCAD side to <see cref="RackProjectionCommandRun"/>, which reads the
    /// drawing once, asks the pure ID19 plan (every blocking failure before any point), shows the warnings, asks the two
    /// points, imports and verifies the library blocks, applies ONE common transform and writes everything in ONE
    /// caller-owned transaction with ONE commit. Cancel, None, an error or any failure writes nothing.
    /// </para>
    /// </summary>
    public sealed class RackProyectarCommands
    {
        [CommandMethod("RPY")] public void AliasRackProyectar() => RackProyectar();          // RACKPROYECTAR

        [CommandMethod("RACKPROYECTAR")]
        public void RackProyectar()
        {
            try
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                {
                    return;
                }

                RackProjectionCommandRun.Execute(new AutoCadRackProjectionCommandPort(document));
            }
            catch (System.Exception ex)
            {
                RackCommandSupport.Report(ex);
            }
        }
    }
}
