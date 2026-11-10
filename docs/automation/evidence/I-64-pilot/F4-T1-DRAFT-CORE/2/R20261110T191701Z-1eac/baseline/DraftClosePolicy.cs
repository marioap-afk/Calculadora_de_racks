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

        /// <summary>The racks whose draft is Dirty or has pending text, ordered (ordinal); empty for Allow.</summary>
        public IReadOnlyList<string> RackIds { get; }
    }

    /// <summary>
    /// The veto policy for closing a document (D-17, INV-20), decided from the in-memory drafts only. Every attempt is
    /// evaluated anew: repeating an attempt is never consent, and only discarding the drafts lets the next close through.
    /// The decision does not depend on the degraded mode of the session.
    /// </summary>
    public sealed class DraftClosePolicy
    {
        private readonly DraftRegistry _registry;

        public DraftClosePolicy(DraftRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public DraftCloseDecision EvaluateClose(SessionId session)
        {
            var atRisk = _registry.RacksAtRiskOf(session);
            return atRisk.Count > 0
                ? new DraftCloseDecision(DraftCloseVerdict.Veto, atRisk)
                : new DraftCloseDecision(DraftCloseVerdict.Allow, Array.Empty<string>());
        }
    }
}
