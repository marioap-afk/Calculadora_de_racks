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
    }
}
