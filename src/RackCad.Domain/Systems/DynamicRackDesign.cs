using System.Collections.Generic;
using RackCad.Domain.RackFrames;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Editable inputs of a pallet-flow system. Unlike <see cref="DynamicRackSystem"/>, this type does not carry
    /// calculated positions: Application resolves it into the drawing/BOM model. Future frontal/planta inputs can be
    /// added here without turning AutoCAD geometry into persisted user intent.
    /// </summary>
    public sealed class DynamicRackDesign
    {
        public PalletSpecification Pallet { get; set; } = new PalletSpecification();
        public int PalletsDeep { get; set; }
        public int LoadLevels { get; set; } = DynamicRackDefaults.DefaultLoadLevels;
        public double FirstLevelHeight { get; set; } = DynamicRackDefaults.DefaultFirstLevelHeight;
        public double BeamDepth { get; set; } = DynamicRackDefaults.DefaultBeamDepth;
        public string HeaderPostCatalogId { get; set; }
        public int? SeparatorCountOverride { get; set; }
        public double? SeparatorSpacingOverride { get; set; }
        public bool DerivedPostReinforced { get; set; } = true;
        public double? DerivedPostReinforcementHeight { get; set; }
        public double? ManualHeaderHeightOverride { get; set; }

        /// <summary>
        /// Optional explicit longitudinal layout. Empty means "derive the standard layout". Keeping overrides in the
        /// design makes re-resolution deterministic while leaving StartX/EndX exclusively on the resolved system.
        /// </summary>
        public IList<DynamicRackModuleDesign> Modules { get; } = new List<DynamicRackModuleDesign>();
    }

    /// <summary>Editable intent for one longitudinal module; calculated X coordinates deliberately do not live here.</summary>
    public sealed class DynamicRackModuleDesign
    {
        public string ModuleId { get; set; }
        public DynamicRackModuleKind Kind { get; set; }
        public double Length { get; set; }
        public bool IsCalculated { get; set; } = true;
        public bool IsManualOverride { get; set; }

        /// <summary>
        /// True means Application may regenerate a standard cabecera when calculated inputs change. False preserves the
        /// user's complete configuration, matching the selective system's per-post custom-cabecera contract.
        /// </summary>
        public bool UseCalculatedHeaderConfiguration { get; set; } = true;

        public RackFrameConfiguration HeaderConfiguration { get; set; }
        public string Notes { get; set; }

        public bool IsHeader =>
            Kind == DynamicRackModuleKind.HeaderStart
            || Kind == DynamicRackModuleKind.HeaderIntermediate
            || Kind == DynamicRackModuleKind.HeaderEnd;
    }
}
