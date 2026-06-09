using System;

namespace RackCad.Domain.RackFrames
{
    public sealed class FrameHorizontal
    {
        public FrameHorizontal()
        {
            Id = Guid.NewGuid().ToString("N");
            Quantity = 1;
            MountingFace = FrameSide.Front;
            State = FrameComponentState.Standard;
        }

        public string Id { get; set; }
        public int Number { get; set; }
        public double Elevation { get; set; }
        public string ProfileId { get; set; }
        public int Quantity { get; set; }
        public FrameSide MountingFace { get; set; }
        public FrameComponentState State { get; set; }
        public string Notes { get; set; }
        public bool IsStandard { get; set; }
    }
}
