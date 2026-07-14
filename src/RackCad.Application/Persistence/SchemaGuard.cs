using System;
using System.Globalization;

namespace RackCad.Application.Persistence
{
    /// <summary>
    /// Guards against opening a design saved by a NEWER build than this one understands. Compares the stored schema's
    /// MAJOR version against what this build writes: a higher major means the file carries fields/semantics this build
    /// can't interpret, so we refuse it with a clear message instead of silently dropping data. A missing/older/unparseable
    /// version is accepted — the documents' own backward-compatible mapping (legacy fallbacks) handles older files, which
    /// is the migration path today; a real transform hook belongs here when a breaking change (major bump) first lands.
    /// </summary>
    public static class SchemaGuard
    {
        public static void CheckReadable(string storedVersion, string currentVersion, string what)
        {
            if (MajorOf(storedVersion) > MajorOf(currentVersion))
            {
                throw new InvalidOperationException(
                    what + " fue creado con una versión más nueva de RackCad (esquema " + storedVersion +
                    "); actualiza la aplicación para abrirlo.");
            }
        }

        /// <summary>Major version number (the part before the first '.'); 1 when missing/unparseable (legacy default).</summary>
        private static int MajorOf(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                return 1;
            }

            var dot = version.IndexOf('.');
            var head = dot >= 0 ? version.Substring(0, dot) : version;
            return int.TryParse(head.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var major) && major > 0
                ? major
                : 1;
        }
    }
}
