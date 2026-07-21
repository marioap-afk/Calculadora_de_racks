using Autodesk.AutoCAD.Runtime;

namespace RackCad.Plugin
{
    /// <summary>
    /// Short command aliases for the RackCad commands, so they can be typed like AutoCAD's native ones (COPY → CP).
    /// Each alias is just a thin command that forwards to the full command, kept together here so the whole scheme is
    /// visible and easy to change. They avoid the native two-letter PGP aliases (RE=REGEN, RO=ROTATE). If one clashes
    /// with a user's own acad.pgp alias the PGP wins (the alias is expanded before command lookup) — pick another
    /// letter here, or the user can add their own PGP alias instead.
    /// </summary>
    public sealed partial class RackFrameCommands
    {
        [CommandMethod("RK")]  public void AliasRackCad() => RackCad();                    // menú principal
        [CommandMethod("RED")] public void AliasRackEditar() => RackEditar();              // RACKEDITAR
        [CommandMethod("RA")]  public void AliasRackAyuda() => RackAyuda();                // RACKAYUDA
    }
}
