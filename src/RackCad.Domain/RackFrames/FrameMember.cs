namespace RackCad.Domain.RackFrames
{
    public sealed class FrameMember
    {
        public FrameMember()
        {
            Quantity = 1;
            Origin = FrameMemberOrigin.Standard;
            Start = new FrameMemberEnd();
            End = new FrameMemberEnd();
        }

        public string SourcePanelId { get; set; }
        public int SourcePanelIndex { get; set; }
        public FrameMemberType MemberType { get; set; }
        public string CatalogId { get; set; }
        public string ProfileId { get; set; }
        public int Quantity { get; set; }
        public FrameSide MountingFace { get; set; }
        public FrameMemberOrigin Origin { get; set; }
        public FrameMemberEnd Start { get; set; }
        public FrameMemberEnd End { get; set; }
        public double Length { get; set; }
        public double Angle { get; set; }
        public bool IsStandard { get; set; }
    }
}
