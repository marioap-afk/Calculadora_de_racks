using System.Collections.Generic;
using System.Linq;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Preliminary side-view layout of a dynamic system. There are exactly <see cref="PalletsDeep"/>
    /// longitudinal modules (first and last = pallet depth + 6", interior = pallet depth), so the
    /// total run length is always N x depth + 12". Intermediate posts are separate position markers
    /// (X offsets) that add no length and do not change the module count.
    /// </summary>
    public sealed class DynamicRackLayout
    {
        public DynamicRackLayout(
            PalletSpecification pallet,
            int palletsDeep,
            double totalLength,
            IReadOnlyList<RackModule> modules,
            IReadOnlyList<double> intermediatePosts)
        {
            Pallet = pallet;
            PalletsDeep = palletsDeep;
            TotalLength = totalLength;
            Modules = modules ?? new List<RackModule>();
            IntermediatePosts = intermediatePosts ?? new List<double>();
        }

        public PalletSpecification Pallet { get; }
        public int PalletsDeep { get; }
        public double TotalLength { get; }

        /// <summary>Exactly N longitudinal modules.</summary>
        public IReadOnlyList<RackModule> Modules { get; }

        /// <summary>X positions of intermediate posts (zero-length markers); empty for odd N.</summary>
        public IReadOnlyList<double> IntermediatePosts { get; }

        /// <summary>Sum of the module lengths; always equals <see cref="TotalLength"/>.</summary>
        public double SumOfModuleLengths => Modules.Sum(module => module.Length);
    }
}
