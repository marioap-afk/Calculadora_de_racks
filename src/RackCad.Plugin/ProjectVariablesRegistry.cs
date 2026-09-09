using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Persistence;

namespace RackCad.Plugin
{
    /// <summary>
    /// The drawing's project-variable register, as the rest of the Plugin sees it: read it, and write it only
    /// when the read said that was safe.
    ///
    /// <para>
    /// It composes the physical access (<see cref="ProjectVariablesData"/>) with the pure decisions
    /// (<see cref="ProjectVariablesStore"/>, <see cref="ProjectVariablesWriteGuard"/>) so that no caller can
    /// take the shortcut this contract exists to prevent: overwriting a register this build could not read.
    /// Passing the last read into <see cref="TryWrite"/> is what makes that structural instead of a rule
    /// somebody has to remember.
    /// </para>
    /// <para>Must be called inside an open transaction — the CALLER owns it, and the caller commits.</para>
    /// </summary>
    internal static class ProjectVariablesRegistry
    {
        /// <summary>Reads the register: absent, readable, unreadable or of an incompatible major.</summary>
        public static ProjectVariablesReadResult Read(Transaction transaction, Database database)
            => new ProjectVariablesStore().Read(ProjectVariablesData.Read(transaction, database));

        /// <summary>
        /// Writes the register, refusing when <paramref name="lastRead"/> was not a state this build could
        /// safely replace. Returns false with a visible reason instead of throwing: a refusal to write is an
        /// EXPECTED outcome here, and one the caller has to report rather than trip over.
        /// </summary>
        public static bool TryWrite(
            Transaction transaction,
            Database database,
            ProjectVariablesReadResult lastRead,
            ProjectVariablesDocument document,
            out string error)
        {
            if (!ProjectVariablesWriteGuard.CanOverwrite(lastRead, out error))
            {
                return false;
            }

            if (document == null)
            {
                error = "No hay registro de variables de proyecto que escribir.";
                return false;
            }

            ProjectVariablesData.Write(transaction, database, new ProjectVariablesStore().Serialize(document));
            error = null;
            return true;
        }
    }
}
