#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace.Drafts
{
    public enum DraftCloseVerdict
    {
        Allow,
        Veto,
    }

    /// <summary>The decision on one attempt to close a document, with the racks that cause a veto.</summary>
    public sealed class DraftCloseDecision
    {
        internal DraftCloseDecision(DraftCloseVerdict verdict, IReadOnlyList<string> rackIds)
        {
            Verdict = verdict;
            RackIds = rackIds;
        }

        public DraftCloseVerdict Verdict { get; }

        public bool IsVeto => Verdict == DraftCloseVerdict.Veto;

        public IReadOnlyList<string> RackIds { get; }
    }

    /// <summary>RED skeleton of the veto policy: it always allows.</summary>
    public sealed class DraftClosePolicy
    {
        public DraftClosePolicy(DraftRegistry registry)
        {
        }

        public DraftCloseDecision EvaluateClose(SessionId session) =>
            new(DraftCloseVerdict.Allow, Array.Empty<string>());
    }
}
