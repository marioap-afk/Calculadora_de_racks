using Autodesk.AutoCAD.EditorInput;
using RackCad.Application.StructuralSections;

namespace RackCad.Plugin
{
    /// <summary>
    /// The ONE way the Plugin loads the neutral section catalogue, and the one place its failure is worded.
    ///
    /// It exists because there are now two consumers — <see cref="StructuralSectionCommandFlow"/>, which draws a
    /// bare section, and the Cantilever system, which builds members out of sections — and only one of them may
    /// own the policy. The policy is FAIL CLOSED (I-36A F5): unlike the legacy product catalogue, which degrades to
    /// empty so a draw still runs, an invalid section catalogue means the dimensions are not trustworthy, and
    /// drawing steel from data that failed validation is worse than not drawing it.
    ///
    /// It was EXTRACTED from the structural-section flow rather than copied into the second caller: two loaders
    /// would agree today and drift at the first correction, and the guard that pinned "exactly one place owns the
    /// load" would have had to be weakened instead of re-aimed.
    ///
    /// T7 per (I-64, séptima tanda; review I-1): a third surface, the page «Perfil estructural» of the RackCad window, needs the same load
    /// and the same failure, and it does not live in the Plugin. So the read and the policy went one layer down, to
    /// <see cref="StructuralSectionCatalogSource"/> in Application; this class stays the Plugin's door to it and the place that writes the
    /// reason on the command line.
    /// </summary>
    internal static class StructuralSectionCatalogAccess
    {
        /// <summary>
        /// Loads the catalogue, or writes the reason on the command line and returns false.
        ///
        /// The message is the USER's and not a log line: whoever ran the command has to know why nothing was
        /// drawn. A null <paramref name="editor"/> is tolerated so a non-interactive caller can still fail closed.
        /// </summary>
        internal static bool TryLoad(Editor editor, out StructuralSectionCatalog catalogue)
        {
            // T7 per (review I-1): the read, the two failures of the catalogue and their words moved to the ONE source of Application,
            // StructuralSectionCatalogSource, which the RackCad window uses too. The words written here are the same as before, and any other
            // exception still propagates to the command that reports it.
            var load = StructuralSectionCatalogSource.TryLoad();
            catalogue = load.Catalog;
            if (load.Loaded) return true;

            editor?.WriteMessage("\n" + load.CommandLineText);
            return false;
        }
    }
}
