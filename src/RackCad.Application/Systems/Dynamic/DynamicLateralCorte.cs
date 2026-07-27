using RackCad.Application.Drawing;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>One linked lateral section at a transverse post of the dynamic front grid.</summary>
    public sealed class DynamicLateralCorte
    {
        public DynamicLateralCorte(int postIndex, double postX, HeaderRunPlan plan)
        {
            PostIndex = postIndex;
            PostX = postX;
            Plan = plan;
        }

        public int PostIndex { get; }
        public double PostX { get; }
        public HeaderRunPlan Plan { get; }
    }
}
