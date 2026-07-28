namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Where the neutral catalog comes from. A provider boundary, exactly like
    /// <c>IRackCatalogProvider</c> is for the legacy catalog: today only the CSV implementation exists, and the
    /// interface earns its place because the STORAGE is the thing most likely to change (ADR-0007 keeps that
    /// door explicitly open) while everything downstream keeps consuming a
    /// <see cref="StructuralSectionCatalog"/>.
    /// </summary>
    public interface IStructuralSectionCatalogProvider
    {
        /// <summary>Loads the whole catalog. Implementations may cache; the result is immutable for consumers.</summary>
        StructuralSectionCatalog Load();
    }
}
