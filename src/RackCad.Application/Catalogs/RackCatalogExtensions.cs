using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Catalogs
{
    public static class RackCatalogExtensions
    {
        public static ProfileCatalogEntry FindProfile(this IEnumerable<ProfileCatalogEntry> profiles, string id)
        {
            if (profiles == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return profiles.FirstOrDefault(profile =>
                string.Equals(profile?.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public static BasePlateCatalogEntry FindBasePlate(this IEnumerable<BasePlateCatalogEntry> plates, string id)
        {
            if (plates == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return plates.FirstOrDefault(plate =>
                string.Equals(plate?.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        public static ConnectionPointCatalogEntry FindConnectionPoint(this IEnumerable<ConnectionPointCatalogEntry> points, string id)
        {
            if (points == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return points.FirstOrDefault(point =>
                string.Equals(point?.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>The 2D placement of a connection point on a piece for a given view, or null when absent.</summary>
        public static ConnectionLayoutEntry FindConnectionLayout(
            this IEnumerable<ConnectionLayoutEntry> layout, string pieceId, string connectionPointId, string view)
        {
            if (layout == null || string.IsNullOrWhiteSpace(pieceId) || string.IsNullOrWhiteSpace(connectionPointId))
            {
                return null;
            }

            return layout.FirstOrDefault(entry =>
                string.Equals(entry?.PieceId, pieceId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry?.ConnectionPointId, connectionPointId, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(view) || string.Equals(entry?.View, view, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>Every connection point placed on a piece (across views), in declaration order.</summary>
        public static IEnumerable<ConnectionLayoutEntry> ConnectionLayoutFor(
            this IEnumerable<ConnectionLayoutEntry> layout, string pieceId)
        {
            if (layout == null || string.IsNullOrWhiteSpace(pieceId))
            {
                return Enumerable.Empty<ConnectionLayoutEntry>();
            }

            return layout.Where(entry => string.Equals(entry?.PieceId, pieceId, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// The connection point a piece uses to mate onto its support (role <c>BasePlate</c>), else the
        /// first one declared for it, else null. Lets the factory pick a plate's anchor from the layout.
        /// </summary>
        public static string MountConnectionPointId(this RackCatalog catalog, string pieceId)
        {
            if (catalog == null || string.IsNullOrWhiteSpace(pieceId))
            {
                return null;
            }

            var rows = catalog.ConnectionLayout.ConnectionLayoutFor(pieceId).ToList();
            if (rows.Count == 0)
            {
                return null;
            }

            var mount = rows.FirstOrDefault(row => string.Equals(
                catalog.ConnectionPoints.FindConnectionPoint(row.ConnectionPointId)?.Role,
                "BasePlate",
                StringComparison.OrdinalIgnoreCase));

            return (mount ?? rows[0]).ConnectionPointId;
        }

        public static ViewCatalogEntry FindView(this IEnumerable<ViewCatalogEntry> views, string id)
        {
            if (views == null || string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return views.FirstOrDefault(view =>
                string.Equals(view?.Id, id, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>The block for a given piece in a given view, or null when that combination is absent.</summary>
        public static BlockCatalogEntry FindBlock(this IEnumerable<BlockCatalogEntry> blocks, string pieceId, string view)
        {
            if (blocks == null || string.IsNullOrWhiteSpace(pieceId) || string.IsNullOrWhiteSpace(view))
            {
                return null;
            }

            return blocks.FirstOrDefault(block =>
                string.Equals(block?.PieceId, pieceId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(block?.View, view, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Human-readable description for a catalog id, searched across every catalog list. Falls
        /// back to the id itself when not found. Replaces fragile id-prefix string manipulation.
        /// </summary>
        public static string DescribeId(this RackCatalog catalog, string id)
        {
            if (catalog == null || string.IsNullOrWhiteSpace(id))
            {
                return id ?? string.Empty;
            }

            var profile = catalog.PostProfiles.FindProfile(id)
                ?? catalog.TrussProfiles.FindProfile(id)
                ?? catalog.SpacerProfiles.FindProfile(id);

            if (profile != null)
            {
                return profile.Label; // display name, else description, else id
            }

            var plate = catalog.BasePlates.FindBasePlate(id);
            if (plate != null)
            {
                return plate.Label;
            }

            var point = catalog.ConnectionPoints.FindConnectionPoint(id);
            if (point != null)
            {
                return point.Label;
            }

            return id;
        }
    }
}
