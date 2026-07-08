namespace RackCad.Domain.RackFrames
{
    public sealed class BasePlatePlacement
    {
        public PostSide PostSide { get; set; }
        public string PlateCatalogId { get; set; }
        public string Description { get; set; }
        public string ConnectionPointId { get; set; }

        /// <summary>Manual override for the plate's PERALTE (in). Null = derived from the post (StandardPeralte).</summary>
        public double? PeralteOverride { get; set; }
    }
}
