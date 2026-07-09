using RackCad.Application.Geometry;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// The catalog lookups every layout builder repeats: a piece's local connection point in a view, its block
    /// name in a view, and the first non-empty id of a fallback chain. One copy here instead of one per builder.
    /// </summary>
    public static class CatalogLookup
    {
        /// <summary>Local (X,Y) of a piece's connection point in a view; (0,0) when the catalog has no row.</summary>
        public static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(pieceId, connectionPointId, view);
            return entry == null ? new Point2D(0.0, 0.0) : new Point2D(entry.LocalX, entry.LocalY);
        }

        /// <summary>Block name of a piece in a view; null when the catalog has no row.</summary>
        public static string Block(RackCatalog catalog, string pieceId, string view)
            => catalog?.Blocks.FindBlock(pieceId, view)?.BlockName;

        /// <summary>First candidate that is not null/whitespace (trimmed); null when none.</summary>
        public static string FirstNonEmpty(params string[] candidates)
        {
            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate))
                {
                    return candidate.Trim();
                }
            }

            return null;
        }
    }
}
