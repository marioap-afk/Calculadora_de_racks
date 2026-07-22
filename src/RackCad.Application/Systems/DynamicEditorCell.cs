using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Editable per-cell (front x level) values of the dynamic editor, extracted verbatim from the window so the grid
    /// state can be built and tested without WPF (I-21). Defaults mirror the previous inline row: a fresh cell reproduces
    /// the values a brand-new dynamic system starts with. The window renders/edits these; it no longer owns their shape.
    /// </summary>
    public sealed class DynamicEditorCell
    {
        public double PalletFront { get; set; } = 42.0;
        public double PalletHeight { get; set; } = 60.0;
        public double PalletWeight { get; set; } = 1000.0;
        public double ClearHeight { get; set; } = DynamicRackDefaults.DefaultClearHeight;
        public string InOutBeamCatalogId { get; set; } = DynamicRackDefaults.InOutBeamCatalogId;
        public double InOutBeamDepth { get; set; } = DynamicRackDefaults.DefaultBeamDepth;
        public double? BeamLengthOverride { get; set; }
        public string IntermediateBeamCatalogId { get; set; } = DynamicRackDefaults.IntermediateBeamCatalogId;
        public double IntermediateBeamDepth { get; set; } = DynamicRackDefaults.DefaultIntermediateBeamDepth;

        public bool IsValid => PalletFront > 0.0 && PalletHeight > 0.0 && PalletWeight >= 0.0
                               && ClearHeight >= 0.0 && InOutBeamDepth > 0.0
                               && IntermediateBeamDepth > 0.0
                               && (!BeamLengthOverride.HasValue || BeamLengthOverride.Value > 0.0);

        /// <summary>Copy the editable structural cell values from an edit buffer (was the window's ApplyCellValues).</summary>
        public void Apply(DynamicEditorValues values)
        {
            PalletFront = values.PalletFront;
            PalletHeight = values.PalletHeight;
            PalletWeight = values.PalletWeight;
            ClearHeight = values.ClearHeight;
            InOutBeamCatalogId = values.InOutBeamCatalogId;
            InOutBeamDepth = values.InOutBeamDepth;
            BeamLengthOverride = values.BeamLengthOverride;
            IntermediateBeamCatalogId = values.IntermediateBeamCatalogId;
            IntermediateBeamDepth = values.IntermediateBeamDepth;
        }

        public DynamicRackLevelDesign ToDesign()
            => new DynamicRackLevelDesign
            {
                PalletFront = PalletFront,
                PalletHeight = PalletHeight,
                PalletWeight = PalletWeight,
                ClearHeight = ClearHeight,
                InOutBeamCatalogId = InOutBeamCatalogId,
                InOutBeamDepth = InOutBeamDepth,
                BeamLengthOverride = BeamLengthOverride,
                IntermediateBeamCatalogId = IntermediateBeamCatalogId,
                IntermediateBeamDepth = IntermediateBeamDepth
            };

        public DynamicEditorCell Clone()
            => new DynamicEditorCell
            {
                PalletFront = PalletFront,
                PalletHeight = PalletHeight,
                PalletWeight = PalletWeight,
                ClearHeight = ClearHeight,
                InOutBeamCatalogId = InOutBeamCatalogId,
                InOutBeamDepth = InOutBeamDepth,
                BeamLengthOverride = BeamLengthOverride,
                IntermediateBeamCatalogId = IntermediateBeamCatalogId,
                IntermediateBeamDepth = IntermediateBeamDepth
            };

        public static DynamicEditorCell Default() => new DynamicEditorCell();

        public static DynamicEditorCell From(DynamicRackLevel level)
            => level == null
                ? Default()
                : new DynamicEditorCell
                {
                    PalletFront = level.Pallet?.Front ?? 42.0,
                    PalletHeight = level.Pallet?.Height ?? 60.0,
                    PalletWeight = level.Pallet?.Weight ?? 0.0,
                    ClearHeight = level.ClearHeight,
                    InOutBeamCatalogId = level.InOutBeamCatalogId,
                    InOutBeamDepth = level.InOutBeamDepth,
                    BeamLengthOverride = level.BeamLengthOverride,
                    IntermediateBeamCatalogId = level.IntermediateBeamCatalogId,
                    IntermediateBeamDepth = level.IntermediateBeamDepth
                };
    }
}
