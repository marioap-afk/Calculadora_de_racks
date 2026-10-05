#nullable enable
using System.Collections.Generic;

namespace RackCad.Tests
{
    public sealed partial class StateV2Validator
    {
        // I-S18 (orchestration file invariants, with A-1): slice F4-D.
        private void FileOrchestration(StatePoint point, List<StateViolation> v)
        {
        }

        // I-P13 (orchestration pair invariants, with A-1): slice F4-D.
        private void PairOrchestration(StatePoint p, StatePoint n, PairContext ctx, List<StateViolation> v)
        {
        }
    }
}
