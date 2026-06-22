using System;
using System.Collections.Generic;
using System.IO;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// Resolves the on-disk <c>catalogs</c> folder at runtime. This matters inside AutoCAD: a NETLOADed
    /// plugin's <see cref="AppContext.BaseDirectory"/> points at <c>acad.exe</c>, not at the plugin, so the
    /// catalogs shipped next to the assemblies are missed and every lookup (blocks, display names) comes back
    /// empty. We therefore prefer the directory of THIS assembly — the catalogs are copied next to it both in
    /// the plugin output and in the test output — and only fall back to the process base directory.
    /// </summary>
    public static class CatalogDirectory
    {
        public const string FolderName = "catalogs";

        /// <summary>The first existing <c>catalogs</c> folder among the candidate base directories.</summary>
        public static string Resolve()
        {
            foreach (var baseDirectory in CandidateBaseDirectories())
            {
                if (string.IsNullOrEmpty(baseDirectory))
                {
                    continue;
                }

                var candidate = Path.Combine(baseDirectory, FolderName);

                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }

            // Nothing found: keep the historical default so error messages point somewhere sensible.
            return Path.Combine(AppContext.BaseDirectory, FolderName);
        }

        private static IEnumerable<string> CandidateBaseDirectories()
        {
            // The assembly directory is the reliable one inside AutoCAD; the process base dir is the fallback.
            yield return AssemblyDirectory();
            yield return AppContext.BaseDirectory;
        }

        private static string AssemblyDirectory()
        {
            try
            {
                var location = typeof(CatalogDirectory).Assembly.Location;
                return string.IsNullOrEmpty(location) ? null : Path.GetDirectoryName(location);
            }
            catch
            {
                return null;
            }
        }
    }
}
