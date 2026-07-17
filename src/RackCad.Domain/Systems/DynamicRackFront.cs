using System.Collections.Generic;

namespace RackCad.Domain.Systems
{
    /// <summary>Editable transverse intent for one dynamic rack front.</summary>
    public sealed class DynamicRackFrontDesign
    {
        /// <summary>Number of pallet-flow lanes placed side by side in this front.</summary>
        public int PalletCount { get; set; } = 1;

        /// <summary>Optional number of load levels in this front; null keeps the design-wide legacy value.</summary>
        public int? LoadLevels { get; set; }

        /// <summary>Optional pallets deep for this front; null keeps the design-wide legacy value.</summary>
        public int? PalletsDeep { get; set; }

        /// <summary>One-based longitudinal position where this front starts in the shared structure.</summary>
        public int? DepthStartPosition { get; set; }

        /// <summary>Optional IN/OUT beam cut length (in). Null uses the pallet-driven standard rule.</summary>
        public double? BeamLengthOverride { get; set; }

        /// <summary>Optional first load-beam elevation for this front; null keeps the rack-wide legacy value.</summary>
        public double? FirstLevelHeight { get; set; }

        /// <summary>Editable intermediate-beam PERALTE by level for this front, level 1 first.</summary>
        public IList<double> IntermediateBeamDepths { get; } = new List<double>();

        /// <summary>Editable cell values, level 1 first. Missing entries inherit the rack-wide legacy fields.</summary>
        public IList<DynamicRackLevelDesign> Levels { get; } = new List<DynamicRackLevelDesign>();
    }

    /// <summary>Resolved transverse width of one front; drawing and BOM consume this same result.</summary>
    public sealed class DynamicRackFront
    {
        public int Index { get; set; }
        public int PalletCount { get; set; }
        public int LoadLevels { get; set; }
        public int PalletsDeep { get; set; }
        public int DepthStartPosition { get; set; } = 1;

        /// <summary>Resolved longitudinal limits in the shared system coordinates.</summary>
        public double StartX { get; set; }
        public double EndX { get; set; }

        /// <summary>Bed-frame width (BFR) of one lane: pallet front + 2 in.</summary>
        public double Bfr { get; set; }

        public double BeamLength { get; set; }
        public double? BeamLengthOverride { get; set; }

        /// <summary>Resolved first load-beam elevation owned by this front.</summary>
        public double FirstLevelHeight { get; set; } = DynamicRackDefaults.DefaultFirstLevelHeight;

        /// <summary>Resolved commercial post height required by this front's own load levels.</summary>
        public double Height { get; set; }

        /// <summary>Resolved end-beam elevations for this front's own depth and slope.</summary>
        public IList<DynamicLoadBeamLevel> LoadBeamLevels { get; } = new List<DynamicLoadBeamLevel>();

        /// <summary>Resolved catalog-valid intermediate-beam PERALTE by level for this front.</summary>
        public IList<double> IntermediateBeamDepths { get; } = new List<double>();

        /// <summary>Resolved values of each front x level cell, level 1 first.</summary>
        public IList<DynamicRackLevel> Levels { get; } = new List<DynamicRackLevel>();
    }

    /// <summary>Editable intent of one dynamic front x level cell. Nullable fields preserve legacy fallbacks.</summary>
    public sealed class DynamicRackLevelDesign
    {
        public double? PalletFront { get; set; }
        public double? PalletHeight { get; set; }
        public double? PalletWeight { get; set; }
        public double? ClearHeight { get; set; }
        public string InOutBeamCatalogId { get; set; }
        public double? InOutBeamDepth { get; set; }
        public double? BeamLengthOverride { get; set; }
        public string IntermediateBeamCatalogId { get; set; }
        public double? IntermediateBeamDepth { get; set; }
    }

    /// <summary>Resolved physical and commercial values of one dynamic front x level cell.</summary>
    public sealed class DynamicRackLevel
    {
        public int LevelNumber { get; set; }
        public PalletSpecification Pallet { get; set; } = new PalletSpecification();
        public double ClearHeight { get; set; } = DynamicRackDefaults.DefaultClearHeight;
        public string InOutBeamCatalogId { get; set; } = DynamicRackDefaults.InOutBeamCatalogId;
        public double InOutBeamDepth { get; set; } = DynamicRackDefaults.DefaultBeamDepth;
        public double? BeamLengthOverride { get; set; }
        public double Bfr { get; set; }
        public double BeamLength { get; set; }
        public string IntermediateBeamCatalogId { get; set; } = DynamicRackDefaults.IntermediateBeamCatalogId;
        public double IntermediateBeamDepth { get; set; } = DynamicRackDefaults.DefaultIntermediateBeamDepth;
    }

    /// <summary>The two physical end cuts of a pallet-flow lane.</summary>
    public enum DynamicRackEnd
    {
        Exit = 0,
        Entrance = 1
    }
}
