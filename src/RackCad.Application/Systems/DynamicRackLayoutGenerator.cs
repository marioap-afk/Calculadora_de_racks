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
    /// Rules (Phase 1):
    ///   Total length = N x depth + 12"
    ///   Start/End headers = depth + 6" each
    ///   Intermediate separators = depth (there are N - 2 of them)
    ///   Intermediate posts are zero-length and placed by <see cref="IIntermediatePostRule"/>.
    /// Total length always equals the sum of the length-bearing modules.
    /// </summary>
    public sealed class DynamicRackLayoutGenerator
    {
        /// <summary>The 6" each header adds beyond the pallet depth (the +12 split across both ends).</summary>
        public const double HeaderEndAllowance = DynamicRackDefaults.HeaderEndAllowance;

        private const double Tolerance = 1e-6;

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

            return Generate(pallet, palletsDeep, pallet.Depth + HeaderEndAllowance);
        }

        /// <summary>
        /// Generates the layout with an explicit header length (e.g. an overridden header depth).
        /// Separators still use the pallet depth, so the total no longer reduces to N*depth+12 when
        /// the header length differs from depth+6 — it is always the sum of the modules.
        /// </summary>
        public DynamicRackLayout Generate(PalletSpecification pallet, int palletsDeep, double headerLength)
        {
            if (pallet == null)
            {
                throw new ArgumentNullException(nameof(pallet));
            }

            if (pallet.Depth <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(pallet), "El fondo de tarima debe ser mayor que cero.");
            }

            if (headerLength <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(headerLength), "El largo de cabecera debe ser mayor que cero.");
            }

            if (palletsDeep < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(palletsDeep), "Se requieren al menos 2 tarimas de fondo.");
            }

            var depth = pallet.Depth;

            // 1) Length-bearing modules: HeaderStart, (N-2) separators, HeaderEnd.
            var lengthModules = new List<RackModule>();
            var index = 0;
            var x = 0.0;

            lengthModules.Add(new RackModule(index++, RackModuleKind.HeaderStart, x, x + headerLength));
            x += headerLength;

            var separatorCount = palletsDeep - 2;
            for (var i = 0; i < separatorCount; i++)
            {
                lengthModules.Add(new RackModule(index++, RackModuleKind.Separator, x, x + depth));
                x += depth;
            }

            lengthModules.Add(new RackModule(index++, RackModuleKind.HeaderEnd, x, x + headerLength));
            x += headerLength;

            var totalLength = x;

            // 2) Ask the rule where intermediate posts go, then weave zero-length posts in at those boundaries.
            var postOffsets = (postRule.ResolvePostOffsets(palletsDeep, lengthModules) ?? Array.Empty<double>())
                .OrderBy(offset => offset)
                .ToList();

            var modules = new List<RackModule>();
            var finalIndex = 0;
            var nextPost = 0;

            foreach (var module in lengthModules)
            {
                modules.Add(new RackModule(finalIndex++, module.Kind, module.StartOffset, module.EndOffset));

                while (nextPost < postOffsets.Count && Math.Abs(postOffsets[nextPost] - module.EndOffset) <= Tolerance)
                {
                    modules.Add(new RackModule(finalIndex++, RackModuleKind.IntermediatePost, module.EndOffset, module.EndOffset));
                    nextPost++;
                }
            }

            return new DynamicRackLayout(pallet, palletsDeep, totalLength, modules);
        }
    }
}
