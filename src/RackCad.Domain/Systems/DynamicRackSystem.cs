using System.Collections.Generic;
using System.Linq;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Aggregate (source of truth) for a dynamic (pallet flow) system: the pallet spec, the number
    /// of pallets deep, and the editable sequence of longitudinal modules. The default sequence is
    /// produced by the Application layer (Total = N x depth + 12"), but every module can be edited
    /// afterwards. Total length and module positions are derived from the module lengths.
    /// </summary>
    public sealed class DynamicRackSystem
    {
        public RackSystemKind Kind { get; set; } = RackSystemKind.PalletFlow;
        public PalletSpecification Pallet { get; set; } = new PalletSpecification();
        public int PalletsDeep { get; set; }

        /// <summary>Optional custom number of separator levels per header (null = standard rule).</summary>
        public int? SeparatorCountOverride { get; set; }

        /// <summary>Optional custom spacing (in) between separator levels (null = standard rule).</summary>
        public double? SeparatorSpacingOverride { get; set; }

        /// <summary>Ordered, editable modules. Intermediate posts are zero-length entries.</summary>
        public IList<DynamicRackModule> Modules { get; } = new List<DynamicRackModule>();

        public double TotalLength => Modules.Sum(module => module.Length);

        /// <summary>Number of modules that actually carry length (excludes zero-length posts).</summary>
        public int LengthBearingModuleCount => Modules.Count(module => module.Length > 0.0);

        /// <summary>Default header length imposed by the pallet rule: depth + 6".</summary>
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
