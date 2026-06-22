using System.Collections.Generic;
using RackCad.Application.Headers;

namespace RackCad.Plugin.Headers
{
    /// <summary>
    /// Result of drawing a lateral header: the pure plan that was executed plus the instances that were
    /// skipped because their block definition is not present in the drawing yet. Surfacing the skipped pieces
    /// (with their piece id) lets the command resolve a friendly name and tell the user exactly which AutoCAD
    /// blocks they still need to model.
    /// </summary>
    public sealed class LateralHeaderDrawOutcome
    {
        public LateralHeaderDrawOutcome(
            LateralHeaderLayout layout,
            int insertedCount,
            IReadOnlyList<HeaderBlockInstance> missingInstances)
        {
            Layout = layout;
            InsertedCount = insertedCount;
            MissingInstances = missingInstances ?? new List<HeaderBlockInstance>();
        }

        /// <summary>The pure plan that was executed.</summary>
        public LateralHeaderLayout Layout { get; }

        /// <summary>Number of block references actually inserted into the drawing.</summary>
        public int InsertedCount { get; }

        /// <summary>Distinct pieces referenced by the plan whose block is not defined in the drawing.</summary>
        public IReadOnlyList<HeaderBlockInstance> MissingInstances { get; }

        public bool HasMissingBlocks => MissingInstances.Count > 0;
    }
}
