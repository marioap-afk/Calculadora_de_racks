using System.Collections.Generic;

namespace RackCad.Domain.RackFrames
{
    public sealed class BracingPanel
    {
        public BracingPanel()
        {
            Members = new List<FrameMember>();
            DiagonalDirection = DiagonalDirection.AutoAlternating;
        }

        public string PanelId { get; set; }
        public int Number { get; set; }
        public int Index
        {
            get => Number;
            set => Number = value;
        }
        public string LowerHorizontalId { get; set; }
        public string UpperHorizontalId { get; set; }
        public double StartElevation { get; set; }
        public double EndElevation { get; set; }
        public double ClearHeight => EndElevation - StartElevation;
        public BracingPattern Arrangement { get; set; }
        public FrameSide MountingFace { get; set; }
        public FrameSide SideMode
        {
            get => MountingFace;
            set => MountingFace = value;
        }
        public string DiagonalProfileId { get; set; }
        public string DefaultMemberProfileId
        {
            get => DiagonalProfileId;
            set => DiagonalProfileId = value;
        }
        public DiagonalDirection DiagonalDirection { get; set; }
        public string StartConnectionPointId { get; set; }
        public string EndConnectionPointId { get; set; }
        public bool IsStandard { get; set; }
        public bool IsException { get; set; }
        public IList<FrameMember> Members { get; private set; }
    }
}
