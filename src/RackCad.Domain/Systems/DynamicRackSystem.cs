using System.Collections.Generic;
using System.Linq;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Aggregate (source of truth) for a dynamic (pallet flow) system: the pallet spec, the shared
    /// longitudinal envelope, per-front depth ranges, and the editable sequence of modules. The default sequence is
    /// produced by the Application layer (the shortest front owns the shared +12"), but every module can be edited
    /// afterwards. Total length and module positions are derived from the module lengths.
    /// </summary>
    public sealed class DynamicRackSystem
    {
        public RackSystemKind Kind { get; set; } = RackSystemKind.PalletFlow;
        public PalletSpecification Pallet { get; set; } = new PalletSpecification();
        /// <summary>Number of longitudinal positions in the complete shared envelope.</summary>
        public int PalletsDeep { get; set; }

        /// <summary>Resolved shortest-front range that owns the +6 in allowances and structural pattern.</summary>
        public int BaseDepthStartPosition { get; set; } = 1;
        public int BasePalletsDeep { get; set; }

        /// <summary>Legacy persisted value; resolved front cuts use the fixed BFR contract.</summary>
        public double PalletTolerance { get; set; } = DynamicRackDefaults.DefaultPalletTolerance;

        /// <summary>Resolved transverse fronts, in drawing order.</summary>
        public IList<DynamicRackFront> Fronts { get; } = new List<DynamicRackFront>();

        /// <summary>Resolved complete beam used at both the entrance and exit of every load level.</summary>
        public string InOutBeamCatalogId { get; set; } = DynamicRackDefaults.InOutBeamCatalogId;

        /// <summary>Resolved rack-wide post PERALTE shared by every calculated or custom header.</summary>
        public double PostPeralte { get; set; }

        /// <summary>Resolved vertical depth of the selected entrance/exit beam (catalog-driven).</summary>
        public double InOutBeamDepth { get; set; } = DynamicRackDefaults.DefaultBeamDepth;

        /// <summary>Resolved entrance/exit elevations. Derived by Application; never persisted as user input.</summary>
        public IList<DynamicLoadBeamLevel> LoadBeamLevels { get; } = new List<DynamicLoadBeamLevel>();

        /// <summary>Legacy/common intermediate PERALTE fallback. Resolved front lists are the current source.</summary>
        public IList<double> IntermediateBeamDepths { get; } = new List<double>();

        /// <summary>Resolved safety selections shared by the linked views; drawing and BOM consume independent copies.</summary>
        public IList<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();

        /// <summary>Optional custom number of separator levels per header (null = standard rule).</summary>
        public int? SeparatorCountOverride { get; set; }

        /// <summary>Optional custom spacing (in) between separator levels (null = standard rule).</summary>
        public double? SeparatorSpacingOverride { get; set; }

        /// <summary>Whether the derived (shared) post where two separators meet is reinforced. Default: true.</summary>
        public bool DerivedPostReinforced { get; set; } = true;

        /// <summary>Reinforcement length (in) for the derived post; null/&lt;=0 = full post height.</summary>
        public double? DerivedPostReinforcementHeight { get; set; }

        /// <summary>Optional manual header/tramo height (in) typed by the user; null = derived from the load levels.</summary>
        public double? ManualHeaderHeightOverride { get; set; }

        public bool NumberFronts { get; set; }
        public bool NumberLevels { get; set; }
        public bool DrawRackName { get; set; }
        public double AnnotationScale { get; set; } = 1.0;
        public DimensionDetail Dimensions { get; set; } = DimensionDetail.None;
        public string DimensionStyle { get; set; }

        /// <summary>Client-facing rack name, supplied by the DWG envelope at drawing time.</summary>
        public string Name { get; set; }

        /// <summary>Ordered, editable modules. Intermediate posts are zero-length entries.</summary>
        public IList<DynamicRackModule> Modules { get; } = new List<DynamicRackModule>();

        public double TotalLength => Modules.Sum(module => module.Length);

        /// <summary>Number of modules that actually carry length (excludes zero-length posts).</summary>
        public int LengthBearingModuleCount => Modules.Count(module => module.Length > 0.0);

        /// <summary>Default allowance-bearing module length imposed by the pallet rule: depth + 6".</summary>
        public double DefaultHeaderLength => (Pallet?.Depth ?? 0.0) + DynamicRackDefaults.HeaderEndAllowance;

        /// <summary>Lays out StartX/EndX and Index sequentially from each module's Length.</summary>
        public void RecalculatePositions()
        {
            var x = 0.0;
            var index = 0;

            foreach (var module in Modules)
            {
                module.Index = index++;
                module.StartX = x;
                module.EndX = x + module.Length;
                x = module.EndX;
            }
        }

        /// <summary>
        /// X positions of the derived intermediate posts: the boundary wherever two separators are
        /// consecutive. Posts are not modules — they are a structural consequence of the layout.
        /// </summary>
        public IReadOnlyList<double> GetDerivedPostOffsets()
        {
            var posts = new List<double>();

            for (var i = 1; i < Modules.Count; i++)
            {
                if (Modules[i - 1].Kind == DynamicRackModuleKind.Separator
                    && Modules[i].Kind == DynamicRackModuleKind.Separator)
                {
                    posts.Add(Modules[i - 1].EndX);
                }
            }

            return posts;
        }
    }
}
