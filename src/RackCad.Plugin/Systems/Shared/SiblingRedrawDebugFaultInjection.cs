#if DEBUG
using System;
namespace RackCad.Plugin.Systems.Shared
{
    internal static class SiblingRedrawDebugFaultInjection
    {
        private const string Variable = "RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT";

        internal static void ThrowIfRequested(int oneBasedUnit)
        {
            if (int.TryParse(Environment.GetEnvironmentVariable(Variable), out var requested) && requested == oneBasedUnit)
            {
                throw new InvalidOperationException("DEBUG sibling redraw fault at unit " + oneBasedUnit + ".");
            }
        }
    }
}
#endif
