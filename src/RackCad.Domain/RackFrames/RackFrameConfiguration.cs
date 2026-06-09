using System;
using System.Collections.Generic;

namespace RackCad.Domain.RackFrames
{
    public sealed class RackFrameConfiguration
    {
        public RackFrameConfiguration()
        {
            FrameId = Guid.NewGuid();
            Horizontals = new List<FrameHorizontal>();
            BracingSegments = new List<BracingSegment>();
            BracingPanels = new List<BracingPanel>();
            Members = new List<FrameMember>();
            Exceptions = new List<FrameExceptionOverride>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;
        }

        public Guid FrameId { get; set; }
        public string Name { get; set; }
        public string Units { get; set; }
        public double Height { get; set; }
        public double Depth { get; set; }
        public string StandardBaselineId { get; set; }
        public string StandardBaselineVersion { get; set; }
        public PostAssembly LeftPost { get; set; }
        public PostAssembly RightPost { get; set; }
        public BasePlatePlacement LeftBasePlate { get; set; }
        public BasePlatePlacement RightBasePlate { get; set; }
        public IList<FrameHorizontal> Horizontals { get; private set; }
        public IList<BracingSegment> BracingSegments { get; private set; }
        public IList<BracingPanel> BracingPanels { get; private set; }
        public IList<FrameMember> Members { get; private set; }
        public IList<FrameExceptionOverride> Exceptions { get; private set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
