namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Decides whether the register may be written back, given how the last read went.
    ///
    /// <para>
    /// It is a function and not a convention on purpose. "Do not overwrite a register you could not read" is
    /// the kind of rule that survives exactly as long as everyone remembers it, and forgetting it once costs
    /// every variable in a drawing: a corrupt entry would be replaced by whatever this build happened to have
    /// in memory, silently, and the user would only find out much later.
    /// </para>
    /// <para>
    /// The physical layer cannot turn a failed read into a new register. That is the point.
    /// </para>
    /// </summary>
    public static class ProjectVariablesWriteGuard
    {
        /// <summary>
        /// True when writing is allowed: the drawing had no register (so one may be created), or it had one
        /// this build fully understood.
        ///
        /// <para>A null <paramref name="lastRead"/> is refused: not having read is not permission to write.</para>
        /// </summary>
        public static bool CanOverwrite(ProjectVariablesReadResult lastRead, out string error)
        {
            if (lastRead == null)
            {
                error = "No se puede escribir el registro de variables de proyecto sin haberlo leído antes.";
                return false;
            }

            if (lastRead.CanWrite)
            {
                error = null;
                return true;
            }

            error = lastRead.Error ??
                    "El registro de variables de proyecto no se pudo leer, así que no se sobrescribe.";
            return false;
        }
    }
}
