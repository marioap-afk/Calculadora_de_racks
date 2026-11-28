using System;
using System.IO;
using RackCad.Application.Catalogs;

namespace RackCad.Application.StructuralSections
{
    /// <summary>Why the neutral catalogue of sections could not be used: it did not validate, or it could not be read.</summary>
    public enum StructuralSectionCatalogFailure
    {
        None,

        /// <summary>The files read but did not validate (<see cref="StructuralSectionCatalogException"/>).</summary>
        Invalid,

        /// <summary>The files could not be read (<see cref="IOException"/>).</summary>
        Unreadable,
    }

    /// <summary>
    /// What <see cref="StructuralSectionCatalogSource.TryLoad()"/> got: the catalogue, or why there is none, with the words each surface
    /// says it in. A value: it never throws.
    /// </summary>
    public sealed class StructuralSectionCatalogLoad
    {
        private StructuralSectionCatalogLoad(StructuralSectionCatalog catalog, StructuralSectionCatalogFailure failure, string detail)
        {
            Catalog = catalog;
            Failure = failure;
            Detail = detail ?? string.Empty;
        }

        /// <summary>The validated catalogue; null when it failed.</summary>
        public StructuralSectionCatalog Catalog { get; }

        public StructuralSectionCatalogFailure Failure { get; }

        /// <summary>The message of the provider (file, row and column), as it came.</summary>
        public string Detail { get; }

        public bool Loaded => Catalog != null;

        /// <summary>
        /// The words of RACKSECCION in the command line, without accents as every line of the Plugin: «RackCad: el catalogo de secciones
        /// estructurales no es valido, asi que no se dibujara nada. …». Empty when the catalogue loaded.
        /// </summary>
        public string CommandLineText
        {
            get
            {
                switch (Failure)
                {
                    case StructuralSectionCatalogFailure.Invalid:
                        return "RackCad: el catalogo de secciones estructurales no es valido, asi que no se dibujara nada. " + Detail;
                    case StructuralSectionCatalogFailure.Unreadable:
                        return "RackCad: no se pudo leer el catalogo de secciones estructurales, asi que no se dibujara nada. " + Detail;
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>The same words for a page of the RackCad window, with their accents. Empty when the catalogue loaded.</summary>
        public string PageText
        {
            get
            {
                switch (Failure)
                {
                    case StructuralSectionCatalogFailure.Invalid:
                        return ("El catálogo de secciones estructurales no es válido, así que no se dibujará nada. " + Detail).Trim();
                    case StructuralSectionCatalogFailure.Unreadable:
                        return ("No se pudo leer el catálogo de secciones estructurales, así que no se dibujará nada. " + Detail).Trim();
                    default:
                        return string.Empty;
                }
            }
        }

        internal static StructuralSectionCatalogLoad Of(StructuralSectionCatalog catalog) =>
            new StructuralSectionCatalogLoad(catalog, StructuralSectionCatalogFailure.None, string.Empty);

        internal static StructuralSectionCatalogLoad Failed(StructuralSectionCatalogFailure failure, string detail) =>
            new StructuralSectionCatalogLoad(null, failure, detail);
    }

    /// <summary>
    /// The ONE place RackCad reads the neutral catalogue of sections that ships with it, and the ONE place its failure is decided and worded
    /// (T7 per, review I-1). Until then the same read and the same «fail closed» lived in the Plugin (StructuralSectionCatalogAccess, for
    /// RACKSECCION and the Cantilever commands), in the classic Cantilever editor, and in three loaders of the RackCad window (the page
    /// «Perfil estructural», «Larguero» and Cantilever): copies that agree today and drift at the first correction.
    ///
    /// <para>The policy is FAIL CLOSED (I-36A F5): unlike the legacy product catalogue, which degrades to empty so a draw still runs, a section
    /// catalogue that does not validate means the dimensions are not trustworthy. Only the two failures of the catalogue are a value here
    /// (<see cref="StructuralSectionCatalogException"/> and <see cref="IOException"/>); anything else is not «the catalogue is wrong» but a
    /// defect, and it propagates to whoever reports defects.</para>
    ///
    /// <para>The pages that degrade on purpose («Larguero» offers its own largueros without AISC; the window says why it does not open a
    /// Cantilever) keep their notices and use <see cref="Load"/>: the read is this one, the decision of what to show is theirs.</para>
    /// </summary>
    public static class StructuralSectionCatalogSource
    {
        /// <summary>The catalogue of the distribution, validated (the provider caches it by the signature of its files). Throws what the provider throws.</summary>
        public static StructuralSectionCatalog Load() => new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        /// <summary>The catalogue of the distribution, or why not (fail closed). Any other exception propagates.</summary>
        public static StructuralSectionCatalogLoad TryLoad() => TryLoad(Load);

        /// <summary>
        /// The same over another source (the window and the tests give theirs). A source that returns no catalogue is a defect, not a
        /// failure of the catalogue: it throws <see cref="InvalidOperationException"/>.
        /// </summary>
        public static StructuralSectionCatalogLoad TryLoad(Func<StructuralSectionCatalog> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            try
            {
                var catalog = source();
                if (catalog == null)
                {
                    throw new InvalidOperationException("La fuente del catálogo de secciones estructurales no devolvió ningún catálogo.");
                }

                return StructuralSectionCatalogLoad.Of(catalog);
            }
            catch (StructuralSectionCatalogException ex)
            {
                return StructuralSectionCatalogLoad.Failed(StructuralSectionCatalogFailure.Invalid, ex.Message);
            }
            catch (IOException ex)
            {
                return StructuralSectionCatalogLoad.Failed(StructuralSectionCatalogFailure.Unreadable, ex.Message);
            }
        }
    }
}
