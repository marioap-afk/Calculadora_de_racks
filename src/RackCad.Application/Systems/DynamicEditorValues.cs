namespace RackCad.Application.Systems
{
    /// <summary>
    /// Parsed edit buffer of the dynamic editor's per-cell panel: the window reads the WPF fields into this pure DTO and
    /// hands it to <see cref="DynamicFrontMatrix"/> to apply to one cell, a scope of cells, or a set of fronts (I-21).
    /// Keeping it free of WPF lets the matrix's apply/scope logic be tested without a window.
    /// </summary>
    public sealed class DynamicEditorValues
    {
        public int PalletCount { get; set; }
        public int LoadLevels { get; set; }
        public int PalletsDeep { get; set; }
        public int DepthStartPosition { get; set; }
        public double? BeamLengthOverride { get; set; }
        public double FirstLevelHeight { get; set; }
        public double PalletFront { get; set; }
        public double PalletHeight { get; set; }
        public double PalletWeight { get; set; }
        public double ClearHeight { get; set; }
        public string InOutBeamCatalogId { get; set; }
        public double InOutBeamDepth { get; set; }
        public string IntermediateBeamCatalogId { get; set; }
        public double IntermediateBeamDepth { get; set; }
    }
}
