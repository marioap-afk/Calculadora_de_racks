using RackCad.Application.Catalogs;

namespace RackCad.Plugin
{
    /// <summary>
    /// The one place the Plugin loads the product catalog: the base-directory provider, swallowing a load
    /// failure into an empty <see cref="RackCatalog"/> so commands and draw services keep working when the
    /// catalog is missing or invalid. Extracted from <c>LateralHeaderDrawService.LoadCatalog</c> (I-16 F2),
    /// which now forwards here. Kept INTERNAL to RackCad.Plugin — deliberately NOT unified with the UI's own
    /// <c>UiSupport.LoadCatalogSafe</c> (the Plugin -> UI dependency direction is out of I-16's scope).
    /// </summary>
    internal static class RackCatalogLoader
    {
        internal static RackCatalog Load()
        {
            try
            {
                return JsonRackCatalogProvider.FromBaseDirectory().Load();
            }
            catch
            {
                return new RackCatalog();
            }
        }
    }
}
