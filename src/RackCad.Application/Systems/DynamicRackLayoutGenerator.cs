using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Generates the preliminary side-view layout of a dynamic (pallet flow) system from the
    /// pallet spec and the number of pallets deep. Pure logic, no UI/AutoCAD.
    ///
    /// Rules:
    ///   Number of modules = N (one per pallet deep).
    ///   First and last module = depth + 6" (they absorb the +12).
    ///   Interior modules = depth.
    ///   Total length = N x depth + 12" = sum of the module lengths.
    ///   Intermediate posts are zero-length position markers (a separate list), not modules.
    /// </summary>
    public sealed class DynamicRackLayoutGenerator
    {
        public const double HeaderEndAllowance = DynamicRackDefaults.HeaderEndAllowance;

        private readonly IIntermediatePostRule postRule;

        public DynamicRackLayoutGenerator()
            : this(new CenterWhenEvenRule())
        {
        }

        public DynamicRackLayoutGenerator(IIntermediatePostRule postRule)
        {
            this.postRule = postRule ?? new CenterWhenEvenRule();
        }

        public DynamicRackLayout Generate(PalletSpecification pallet, int palletsDeep)
        {
            if (pallet == null)
            {
                throw new ArgumentNullException(nameof(pallet));
            }

            if (pallet.Depth <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(pallet), "El fondo de tarima debe ser mayor que cero.");
            }

            if (palletsDeep < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(palletsDeep), "Se requieren al menos 2 tarimas de fondo.");
            }

            var depth = pallet.Depth;
            var headerLength = depth + HeaderEndAllowance;

            // Exactly N modules: HeaderStart, (N-2) separators, HeaderEnd.
            var modules = new List<RackModule>(palletsDeep);
            var index = 0;
            var x = 0.0;

            modules.Add(new RackModule(index++, RackModuleKind.HeaderStart, x, x + headerLength));
            x += headerLength;

            for (var i = 0; i < palletsDeep - 2; i++)
            {
                modules.Add(new RackModule(index++, RackModuleKind.Separator, x, x + depth));
                x += depth;
            }

            modules.Add(new RackModule(index++, RackModuleKind.HeaderEnd, x, x + headerLength));
            x += headerLength;

            var totalLength = x;

            // Intermediate posts are markers, not modules; they add no length and do not affect the count.
            var posts = (postRule.ResolvePostOffsets(palletsDeep, modules) ?? Array.Empty<double>())
                .OrderBy(offset => offset)
                .ToList();

            return new DynamicRackLayout(pallet, palletsDeep, totalLength, modules, posts);
        }
    }
}
