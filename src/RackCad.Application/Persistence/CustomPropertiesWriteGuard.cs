namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Decides whether a collection of custom properties may be written, given how its last read went (I-54 D-07.5,
    /// INV-06 / ADR-0039 §10).
    ///
    /// <para>
    /// A function and not a convention: "never overwrite what you could not read" survives only as long as everyone
    /// remembers it, and forgetting it once replaces a collection a newer or healthier build could still read. There is
    /// no counterpart that discards, repairs or forces: V1 has no destructive recovery.
    /// </para>
    /// </summary>
    public static class CustomPropertiesWriteGuard
    {
        /// <summary>
        /// True only after <see cref="CustomPropertiesReadOutcome.Absent"/> or <see cref="CustomPropertiesReadOutcome.Readable"/>.
        /// A null <paramref name="lastRead"/> is refused: not having read is not permission to write.
        /// </summary>
        public static bool CanOverwrite(CustomPropertiesReadResult lastRead, out string error)
        {
            if (lastRead == null)
            {
                error = "No se pueden escribir las propiedades personalizadas sin haberlas leído antes.";
                return false;
            }

            if (lastRead.CanWrite)
            {
                error = null;
                return true;
            }

            error = lastRead.Error ??
                    "Las propiedades personalizadas no se pudieron leer, así que no se sobrescriben.";
            return false;
        }
    }
}
