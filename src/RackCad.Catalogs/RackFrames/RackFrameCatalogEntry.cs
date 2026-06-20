using System.Collections.Generic;

namespace RackCad.Catalogs.RackFrames;

public sealed class RackFrameCatalogEntry
{
    public string Id { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
}
