namespace RackCad.Domain.Systems
{
    /// <summary>Domain-level constants for dynamic (pallet flow) systems.</summary>
    public static class DynamicRackDefaults
    {
        /// <summary>Extra length each header adds beyond the pallet depth (the +12 split across both ends).</summary>
        public const double HeaderEndAllowance = 6.0;
    }
}
