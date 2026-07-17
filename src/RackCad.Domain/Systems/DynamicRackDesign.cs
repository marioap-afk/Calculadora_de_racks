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

        /// <summary>
        /// Legacy rack-wide PERALTE list written by the first per-level implementation. New designs store the list
        /// on each front; this remains as an explicit fallback for documents produced before that refinement.
        /// </summary>
        public IList<double> IntermediateBeamDepths { get; } = new List<double>();

        /// <summary>Legacy round-trip field. Dynamic IN/OUT cuts use BFR and do not consume this value.</summary>
        public double PalletTolerance { get; set; } = DynamicRackDefaults.DefaultPalletTolerance;
        public string InOutBeamCatalogId { get; set; } = DynamicRackDefaults.InOutBeamCatalogId;
        public string HeaderPostCatalogId { get; set; }

        /// <summary>Rack-wide post PERALTE (in). Zero inherits the selected post profile's catalog width.</summary>
        public double PostPeralte { get; set; }

        public int? SeparatorCountOverride { get; set; }
        public double? SeparatorSpacingOverride { get; set; }
        public bool DerivedPostReinforced { get; set; } = true;
        public double? DerivedPostReinforcementHeight { get; set; }
        public double? ManualHeaderHeightOverride { get; set; }

        /// <summary>Drawing annotations shared by the linked lateral, frontal and planta views.</summary>
        public bool NumberFronts { get; set; }
        public bool NumberLevels { get; set; }
        public bool DrawRackName { get; set; }
        public double AnnotationScale { get; set; } = 1.0;
        public DimensionDetail Dimensions { get; set; } = DimensionDetail.None;
        public string DimensionStyle { get; set; }

        /// <summary>
        /// Safety intent shared by the linked views. It reuses the selective safety contract: Left/Right are the
        /// exterior exit/entrance faces, and post indices follow the resolved transverse front grid.
        /// </summary>
        public IList<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();

        /// <summary>
        /// Transverse fronts of the rack. Each front can carry a different number of pallet-flow lanes and therefore
        /// a different IN/OUT beam length. Empty is accepted only as the legacy representation and resolves to one
        /// single-position front.
        /// </summary>
        public IList<DynamicRackFrontDesign> Fronts { get; } = new List<DynamicRackFrontDesign>();

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
