namespace RackCad.Domain.RackFrames
{
    public sealed class PostAssembly
    {
        public PostSide Side { get; set; }
        public string PostCatalogId { get; set; }
        public string Description { get; set; }
        public bool HasReinforcement { get; set; }
        public string ReinforcementCatalogId { get; set; }
    }
}
