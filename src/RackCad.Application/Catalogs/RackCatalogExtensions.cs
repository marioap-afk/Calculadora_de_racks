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
                ?? catalog.HorizontalProfiles.FindProfile(id)
                ?? catalog.DiagonalProfiles.FindProfile(id)
                ?? catalog.ReinforcementProfiles.FindProfile(id);

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
