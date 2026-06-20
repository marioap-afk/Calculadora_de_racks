using System.Collections.Generic;

namespace RackCad.Catalogs.RackFrames;

public sealed class RackFrameCatalog
{
    public string CatalogId { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string Units { get; set; } = "in";

    public IList<RackFrameCatalogEntry> PostProfiles { get; set; } = new List<RackFrameCatalogEntry>();

    public IList<RackFrameCatalogEntry> HorizontalProfiles { get; set; } = new List<RackFrameCatalogEntry>();

    public IList<RackFrameCatalogEntry> DiagonalProfiles { get; set; } = new List<RackFrameCatalogEntry>();

    public IList<RackFrameCatalogEntry> BasePlates { get; set; } = new List<RackFrameCatalogEntry>();

    public IList<RackFrameConnectionPoint> ConnectionPoints { get; set; } = new List<RackFrameConnectionPoint>();
}
