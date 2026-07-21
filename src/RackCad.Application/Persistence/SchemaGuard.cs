using System;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Guards against opening a design saved by a NEWER build than this one understands. Refuses (throws) a stored schema
    /// whose MAJOR is higher than what this build writes — the file carries fields/semantics this build can't interpret —
    /// with a clear message instead of silently dropping data. The readability rule itself lives in
    /// <see cref="SchemaVersionPolicy.IsReadable"/> (shared with the tolerant envelope path); this type is the THROWING
    /// gate the library stores use. A missing/older/unparseable version is accepted — the documents' own backward-compatible
    /// mapping (legacy fallbacks) handles older files, which is the migration path today.
    /// </summary>
    public static class SchemaGuard
    {
        public static void CheckReadable(string storedVersion, string currentVersion, string what)
        {
            if (!SchemaVersionPolicy.IsReadable(storedVersion, currentVersion))
            {
                throw new InvalidOperationException(
                    what + " fue creado con una versión más nueva de RackCad (esquema " + storedVersion +
                    "); actualiza la aplicación para abrirlo.");
            }
        }
    }
}
