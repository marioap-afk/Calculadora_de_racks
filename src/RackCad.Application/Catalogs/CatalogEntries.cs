using System.Collections.Generic;
using System.Text.Json.Serialization;
using RackCad.Application.RackFrames;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// Common fields shared by every catalog piece. Beyond the typed fields, the open
    /// <see cref="Properties"/> bag holds any future key/value without changing the model, so the
    /// catalog is extensible: new attributes can be added in JSON and round-trip untouched.
    /// </summary>
    public abstract class CatalogEntryBase
    {
        public string Id { get; set; }

        /// <summary>User-facing name shown in the UI (falls back to description, then id).</summary>
        public string DisplayName { get; set; }

        public string Description { get; set; }

        // ---- CAD (used by the future drawing phase) ----
        public string BlockName { get; set; }
        public string Layer { get; set; }
        public int Color { get; set; }

        // ---- Commercial / quoting ----
        public string Material { get; set; }
        public string PartNumber { get; set; }
        public string Manufacturer { get; set; }
        public string Finish { get; set; }
        public double UnitCost { get; set; }
        public string Currency { get; set; }

        /// <summary>Unit the cost/weight refer to, e.g. "m", "ft", "pieza".</summary>
        public string CostUnit { get; set; }

        /// <summary>Open bag for arbitrary future properties (extensibility seam).</summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <summary>Best human label: display name, else description, else id.</summary>
        [JsonIgnore]
        public string Label =>
            !string.IsNullOrWhiteSpace(DisplayName) ? DisplayName
            : !string.IsNullOrWhiteSpace(Description) ? Description
            : Id;
    }

    /// <summary>
    /// Structural profile used by posts, horizontals and diagonals. Dimensions are in
    /// <see cref="Units"/> (defaults to inches).
    /// </summary>
    public sealed class ProfileCatalogEntry : CatalogEntryBase
    {
        public string Family { get; set; }
        public double Width { get; set; }
        public double Depth { get; set; }
        public double Thickness { get; set; }
        public string Units { get; set; }

        // ---- Engineering ----
        public string Gauge { get; set; }
        public double WeightPerMeter { get; set; }
    }

    /// <summary>Base plate that anchors a post to the floor.</summary>
    public sealed class BasePlateCatalogEntry : CatalogEntryBase
    {
        public double Width { get; set; }
        public double Length { get; set; }
        public double Thickness { get; set; }
        public string ConnectionPointId { get; set; }
        public string Units { get; set; }
        public double WeightEach { get; set; }
    }

    /// <summary>
    /// Named connection point (punch/troquel) referenced by members. Carries a local 2D offset
    /// (inches) within the owning piece's frame so the mate solver can resolve absolute positions.
    /// </summary>
    public sealed class ConnectionPointCatalogEntry : CatalogEntryBase
    {
        public string Role { get; set; }
        public double LocalX { get; set; }
        public double LocalY { get; set; }
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
        public RackDefaults Defaults { get; set; } = new RackDefaults();
    }
}
