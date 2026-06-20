using System.Collections.Generic;
using RackCad.Application.RackFrames;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// Structural profile used by posts, horizontals and diagonals.
    /// Dimensions are expressed in <see cref="Units"/> (defaults to inches).
    /// </summary>
    public sealed class ProfileCatalogEntry
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Family { get; set; }
        public double Width { get; set; }
        public double Depth { get; set; }
        public double Thickness { get; set; }
        public string Units { get; set; }
    }

    /// <summary>Base plate that anchors a post to the floor.</summary>
    public sealed class BasePlateCatalogEntry
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public double Width { get; set; }
        public double Length { get; set; }
        public double Thickness { get; set; }
        public string ConnectionPointId { get; set; }
        public string Units { get; set; }
    }

    /// <summary>Named connection point (punch/troquel) referenced by members.</summary>
    public sealed class ConnectionPointCatalogEntry
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Role { get; set; }
    }

    /// <summary>
    /// Aggregate of every catalog the configurator consumes. Lists are never
    /// null so callers can enumerate without guarding.
    /// </summary>
    public sealed class RackCatalog
    {
        public IReadOnlyList<ProfileCatalogEntry> PostProfiles { get; set; } = new List<ProfileCatalogEntry>();
        public IReadOnlyList<ProfileCatalogEntry> HorizontalProfiles { get; set; } = new List<ProfileCatalogEntry>();
        public IReadOnlyList<ProfileCatalogEntry> DiagonalProfiles { get; set; } = new List<ProfileCatalogEntry>();
        public IReadOnlyList<ProfileCatalogEntry> ReinforcementProfiles { get; set; } = new List<ProfileCatalogEntry>();
        public IReadOnlyList<BasePlateCatalogEntry> BasePlates { get; set; } = new List<BasePlateCatalogEntry>();
        public IReadOnlyList<ConnectionPointCatalogEntry> ConnectionPoints { get; set; } = new List<ConnectionPointCatalogEntry>();
    }
}
