using RackCad.Application.Geometry;

namespace RackCad.Application.Headers
{
    /// <summary>
    /// The local connection-point positions and block names the header logic needs, already resolved
    /// for the chosen view. Passed to the builder so the math stays pure and unit-testable (the catalog
    /// lookup happens in <see cref="HeaderGeometryResolver"/>).
    /// </summary>
    public sealed class HeaderConnectionGeometry
    {
        /// <summary>MONTAJE_POSTE on the base plate (local). The post origin mates onto this.</summary>
        public Point2D MontajePoste { get; set; }

        /// <summary>TROQUEL_CELOSIA on the post (local): the first available troquel = celosía reference line.</summary>
        public Point2D TroquelCelosia { get; set; }

        /// <summary>CELOSIA on a horizontal/diagonal member (local): the point that lands on the troquel line.</summary>
        public Point2D Celosia { get; set; }

        // ---- AutoCAD block names for the chosen view ----
        public string PostBlock { get; set; }
        public string BasePlateBlock { get; set; }
        public string HorizontalBlock { get; set; }
        public string DiagonalBlock { get; set; }
    }
}
