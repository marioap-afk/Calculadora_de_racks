using RackCad.Application.RackFrames;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// Global default ids/values for the standard frame, loaded from <c>defaults.json</c>. Property
    /// initializers fall back to <see cref="CatalogIds"/> so a missing file still yields a usable
    /// standard. Editing the JSON changes the "standard recipe" without recompiling.
    /// </summary>
    public sealed class RackDefaults
    {
        public string Post { get; set; } = CatalogIds.StandardPost;
        public string BasePlate { get; set; } = CatalogIds.BasePlate;
        public string DiagonalProfile { get; set; } = CatalogIds.Diagonal;
        public string HorizontalProfile { get; set; } = CatalogIds.IntermediateHorizontal;
        public string BraceStartConnectionPoint { get; set; } = CatalogIds.BraceStartConnectionPoint;
        public string BraceEndConnectionPoint { get; set; } = CatalogIds.BraceEndConnectionPoint;
        public string BasePlateConnectionPoint { get; set; } = CatalogIds.BasePlateConnectionPoint;
        public double DefaultHeaderHeight { get; set; } = 132.0;
        public double HeaderEndAllowance { get; set; } = 6.0;
    }
}
