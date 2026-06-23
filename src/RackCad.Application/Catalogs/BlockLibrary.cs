using System.IO;
using RackCad.Application.Settings;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// Resolves the block library DWG path: the user override (from <see cref="UserSettings"/>) if set,
    /// otherwise the default <c>blocks-library.dwg</c> next to the catalogs. Shared by the UI (to show/pick
    /// it) and the Plugin (to import from it), so neither hardcodes the path.
    /// </summary>
    public static class BlockLibraryLocator
    {
        public const string FileName = "blocks-library.dwg";

        /// <summary>Default location: next to the catalogs (resolved like the CSV/JSON catalogs).</summary>
        public static string DefaultPath => Path.Combine(CatalogDirectory.Resolve(), FileName);

        /// <summary>The path actually used: the saved override if any, else <see cref="DefaultPath"/>.</summary>
        public static string ResolvePath()
        {
            var configured = UserSettingsStore.Load().BlockLibraryPath;
            return string.IsNullOrWhiteSpace(configured) ? DefaultPath : configured;
        }
    }
}
