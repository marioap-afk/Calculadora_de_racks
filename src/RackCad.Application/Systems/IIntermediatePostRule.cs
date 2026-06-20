using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Strategy that decides where intermediate posts go. Given the length-bearing modules, it
    /// returns the X offsets at which to insert zero-length post modules. Swapping the rule
    /// (e.g. a future max-clear-span rule) changes the behavior without touching model or builder.
    /// </summary>
    public interface IIntermediatePostRule
    {
        IReadOnlyList<double> ResolvePostOffsets(int palletsDeep, IReadOnlyList<DynamicRackModule> lengthModules);
    }
}
