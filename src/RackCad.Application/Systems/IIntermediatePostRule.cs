using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Strategy that decides where intermediate posts go. The layout generator builds the
    /// length-bearing modules first and asks the rule for the X offsets at which to insert
    /// zero-length posts. Swapping the rule (e.g. a future max-clear-span rule) changes the
    /// behavior without touching the model or the generator.
    /// </summary>
    public interface IIntermediatePostRule
    {
        IReadOnlyList<double> ResolvePostOffsets(int palletsDeep, IReadOnlyList<RackModule> lengthModules);
    }
}
