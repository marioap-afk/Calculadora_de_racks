namespace RackCad.Catalogs.RackFrames;

public sealed class RackFrameConnectionPoint
{
    public string Id { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public double OffsetX { get; set; }

    public double OffsetY { get; set; }
}
