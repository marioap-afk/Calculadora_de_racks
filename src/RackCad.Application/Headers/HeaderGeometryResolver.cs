using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;

namespace RackCad.Application.Headers
{
    /// <summary>
    /// Resolves the connection geometry and block names a header needs from the catalog tables
    /// (<c>connection-layout.csv</c> for point positions, <c>blocks.csv</c> for block names), for the
    /// parameters' view. Missing data resolves to (0,0)/null rather than throwing, so a partially filled
    /// catalog still produces a usable plan.
    /// </summary>
    public static class HeaderGeometryResolver
    {
        public static HeaderConnectionGeometry Resolve(RackCatalog catalog, LateralHeaderParameters parameters)
        {
            var view = parameters.View;

            return new HeaderConnectionGeometry
            {
                MontajePoste = Local(catalog, parameters.BasePlateId, parameters.MontajePostePoint, view),
                TroquelCelosia = Local(catalog, parameters.PostId, parameters.TroquelCelosiaPoint, view),
                Celosia = Local(catalog, parameters.TrussProfileId, parameters.CelosiaPoint, view),

                BasePlateBlock = Block(catalog, parameters.BasePlateId, view),
                PostBlock = Block(catalog, parameters.PostId, view),
                HorizontalBlock = Block(catalog, parameters.TrussProfileId, view),
                DiagonalBlock = Block(catalog, parameters.TrussProfileId, view)
            };
        }

        private static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(pieceId, connectionPointId, view);
            return entry == null ? new Point2D(0.0, 0.0) : new Point2D(entry.LocalX, entry.LocalY);
        }

        private static string Block(RackCatalog catalog, string pieceId, string view)
            => catalog?.Blocks.FindBlock(pieceId, view)?.BlockName;
    }
}
