using System;
using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Current rule: one intermediate post at the center boundary when the number of pallets deep
    /// is even (and there is a real middle boundary, i.e. N >= 4). For an odd count the center
    /// falls inside a separator, so no post is added.
    /// </summary>
    public sealed class CenterWhenEvenRule : IIntermediatePostRule
    {
        public IReadOnlyList<double> ResolvePostOffsets(int palletsDeep, IReadOnlyList<DynamicRackModule> lengthModules)
        {
            if (palletsDeep < 4 || palletsDeep % 2 != 0 || lengthModules == null || lengthModules.Count == 0)
            {
                return Array.Empty<double>();
            }

            // The last length module ends at the total run length; its midpoint is a separator boundary.
            var totalLength = lengthModules[lengthModules.Count - 1].EndX;
            return new[] { totalLength / 2.0 };
        }
    }
}
