using Autodesk.AutoCAD.DatabaseServices;
using I52Ct21d.HostFacts.Rs.Core;

namespace I52Ct21d.HostFacts.Rs;

/// <summary>
/// THE only type of the RS DLL that saves a database (the single <c>Database.SaveAs</c>). It saves a <see cref="SideDbHandle"/> (so only a
/// database the instrument created) to a <see cref="ScratchTarget"/> that the <see cref="ScratchRootGuard"/> authorized (a NEW flat file under
/// the declared scratch root), after re-validating it with <see cref="ScratchRootGuard.AssertCreatable"/>, and records the SHA-256 of the
/// result in the guard's ledger. Create-new semantics come from the guard (a target that exists is refused); a race with another process
/// between the check and the save is a residual limit (README). It saves nothing else, to nowhere else, and does not delete.
/// </summary>
internal sealed class ScratchSaver
{
    private readonly ScratchRootGuard _guard;

    internal ScratchSaver(ScratchRootGuard guard) => _guard = guard;

    internal ScratchFileEntry SaveNew(SideDbHandle side, ScratchTarget target)
    {
        _guard.AssertCreatable(target);
        // HOST-TO-CONFIRM: SaveAs(string, DwgVersion) on a database made with new Database(true, true); whether the host also leaves a
        // companion file (backup, temporary or lock file) in the folder: the scratch verifier would report it as an undeclared entry.
        side.Database.SaveAs(target.FullPath, DwgVersion.Current);
        return _guard.RecordSaved(target);
    }
}
