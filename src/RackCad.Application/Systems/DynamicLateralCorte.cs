namespace RackCad.Application.Systems
{
    /// <summary>One linked lateral section at a transverse post of the dynamic front grid.</summary>
    public sealed class DynamicLateralCorte
    {
        public DynamicLateralCorte(int postIndex, double postX, DynamicSystemPlan plan)
        {
            PostIndex = postIndex;
            PostX = postX;
            Plan = plan;
        }

        public int PostIndex { get; }
        public double PostX { get; }
        public DynamicSystemPlan Plan { get; }
    }
}
