using Autodesk.AutoCAD.EditorInput;
using RackCad.Application.Catalogs;
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
            catalogue = null;

            try
            {
                catalogue = new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();
                return true;
            }
            catch (StructuralSectionCatalogException ex)
            {
                editor?.WriteMessage("\nRackCad: el catalogo de secciones estructurales no es valido, " +
                                     "asi que no se dibujara nada. " + ex.Message);
                return false;
            }
            catch (System.IO.IOException ex)
            {
                editor?.WriteMessage("\nRackCad: no se pudo leer el catalogo de secciones estructurales, " +
                                     "asi que no se dibujara nada. " + ex.Message);
                return false;
            }
        }
    }
}
