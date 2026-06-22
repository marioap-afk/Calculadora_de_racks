namespace RackCad.Domain.RackFrames
{
    public sealed class PostAssembly
    {
        public PostSide Side { get; set; }
        public string PostCatalogId { get; set; }
        public string Description { get; set; }
        public bool HasReinforcement { get; set; }
        public string ReinforcementCatalogId { get; set; }

        /// <summary>
        /// Reinforcement height (in) — its own dynamic LONGITUD, independent of the post. The reinforcement
        /// is a second post mated at the post's FIN_POSTE (Y=0) covering the lower zone [0, this height].
        /// Celosía connections within this zone on this side attach to the reinforcement's (inner) troquel.
        /// </summary>
        public double ReinforcementHeight { get; set; }
    }
}
