#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    public enum NavigationRevalidationVerdict
    {
        Valid,
        TargetGone,
        NotAttributable,
    }

    /// <summary>Result of re-reading the targets right before an action runs (INV-40).</summary>
    public sealed record NavigationRevalidation(NavigationRevalidationVerdict Verdict, NavigationTargetSet Fresh);

    /// <summary>
    /// The single authority of navigation targets for (session, RackId) (I-64 D-05, INV-38). It is computed on demand from
    /// neutral facts, cached in memory by (session, RackId) and never persisted.
    /// </summary>
    public sealed class NavigationTargets
    {
        private readonly INavigationFactsPort _port;

        public NavigationTargets(INavigationFactsPort port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        public NavigationTargetSet Resolve(SessionId session, string rackId) =>
            new(new RackKey(session, rackId), Array.Empty<NavigationTarget>(), Array.Empty<NavigationDiagnostic>());

        /// <summary>The context entry: the same authority, asked for the rack of a one-rack selection context.</summary>
        public NavigationTargetSet ResolveForContext(SessionId session, SelectionContext context) =>
            new(new RackKey(session, context.RackId ?? string.Empty), Array.Empty<NavigationTarget>(), Array.Empty<NavigationDiagnostic>());

        public bool IsCached(SessionId session, string rackId) => false;

        public int CachedRackCount(SessionId session) => 0;

        public void Invalidate(SessionId session, string rackId)
        {
        }

        public void InvalidateSession(SessionId session)
        {
        }

        /// <summary>Applies a drain: when it ran the cache invalidation, the session cache is dropped. True when it did.</summary>
        public bool ApplyDrain(SessionId session, DrainResult drain) => false;

        /// <summary>The session was destroyed: its cache goes away and it never resolves again.</summary>
        public void EndSession(SessionId session)
        {
        }

        public bool IsSessionEnded(SessionId session) => false;

        public NavigationRevalidation Revalidate(SessionId session, string rackId, IReadOnlyList<long> expectedHandles) =>
            new(NavigationRevalidationVerdict.TargetGone, Resolve(session, rackId));
    }
}
