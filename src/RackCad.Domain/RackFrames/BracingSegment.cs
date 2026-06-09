namespace RackCad.Domain.RackFrames
{
    public sealed class BracingSegment
    {
        public int Index { get; set; }
        public double StartElevation { get; set; }
        public double EndElevation { get; set; }
        public double ClearHeight { get; set; }
        public BracingPattern Pattern { get; set; }
        public FrameSide SideMode { get; set; }
        public string BraceProfileId { get; set; }
        public string StartConnectionPointId { get; set; }
        public string EndConnectionPointId { get; set; }
        public bool IsException { get; set; }
    }
}
