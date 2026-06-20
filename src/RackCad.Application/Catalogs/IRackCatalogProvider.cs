namespace RackCad.Application.Catalogs
{
    /// <summary>Supplies the catalog aggregate consumed by the configurator.</summary>
    public interface IRackCatalogProvider
    {
        RackCatalog Load();
    }
}
