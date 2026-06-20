using System.Collections.Generic;
using System.Linq;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// Preliminary side-view layout of a dynamic system: the total run length and the ordered
    /// list of modules with their X positions. Pure data, derived from the pallet spec and the
    /// number of pallets deep.
    /// </summary>
    public sealed class DynamicRackLayout
    {
        public DynamicRackLayout(PalletSpecification pallet, int palletsDeep, double totalLength, IReadOnlyList<RackModule> modules)
        {
            Pallet = pallet;
            PalletsDeep = palletsDeep;
            TotalLength = totalLength;
            Modules = modules ?? new List<RackModule>();
        }

        public PalletSpecification Pallet { get; }
        public int PalletsDeep { get; }
        public double TotalLength { get; }
        public IReadOnlyList<RackModule> Modules { get; }

        /// <summary>Sum of the length-bearing modules; must equal <see cref="TotalLength"/>.</summary>
        public double SumOfModuleLengths => Modules.Sum(module => module.Length);
    }
}
