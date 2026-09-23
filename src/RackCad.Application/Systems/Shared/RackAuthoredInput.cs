using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>Original persisted evidence for one source; unreadable sources must remain in the snapshot.</summary>
    public sealed record RackAuthoredSibling(string SourceIdentity, string Kind, string RawEnvelope, string RawDesign);

    /// <summary>
    /// The consumer attests membership and scan completeness. AUTH-13 cannot infer missing physical siblings.
    /// Construction captures the list; raw strings and sibling records cannot be mutated in place.
    /// </summary>
    public sealed class RackAuthoredInput
    {
        public RackAuthoredInput(string rackId, IReadOnlyList<RackAuthoredSibling> siblings,
            bool isComplete, string completenessEvidence)
        {
            RackId = rackId;
            Siblings = siblings == null ? null : System.Array.AsReadOnly(siblings.ToArray());
            IsComplete = isComplete;
            CompletenessEvidence = completenessEvidence;
        }

        public string RackId { get; }
        public IReadOnlyList<RackAuthoredSibling> Siblings { get; }
        public bool IsComplete { get; }
        public string CompletenessEvidence { get; }
    }
}
