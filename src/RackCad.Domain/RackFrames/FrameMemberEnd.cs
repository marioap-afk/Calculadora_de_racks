namespace RackCad.Domain.RackFrames
{
    public sealed class FrameMemberEnd
    {
        public FrameMemberEndRole Role { get; set; }
        public PostSide PostSide { get; set; }
        public double HorizontalPositionRatio { get; set; }
        public double Elevation { get; set; }
        public string ConnectionPointId { get; set; }
    }
}
