#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.Systems.Shared;

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
    /// neutral facts, cached in memory by (session, RackId) and never persisted. The context, the tabs and the navigator all
    /// ask it; none derives targets by itself.
    /// </summary>
    public sealed class NavigationTargets
    {
        private readonly INavigationFactsPort _port;
        private readonly Dictionary<RackKey, NavigationTargetSet> _cache = new(RackKeyIgnoreCase.Instance);
        private readonly HashSet<SessionId> _ended = new();

        public NavigationTargets(INavigationFactsPort port)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
        }

        /// <summary>Targets of the rack in the session, computed on the first request and cached afterwards.</summary>
        public NavigationTargetSet Resolve(SessionId session, string rackId)
        {
            if (string.IsNullOrWhiteSpace(rackId) || _ended.Contains(session))
                return EmptySet(session, rackId);

            var key = new RackKey(session, rackId);
            if (_cache.TryGetValue(key, out var cached)) return cached;

            var computed = Compute(session, rackId, out _);
            _cache[key] = computed;
            return computed;
        }

        /// <summary>The context entry: the same authority, asked for the rack of a one-rack selection context.</summary>
        public NavigationTargetSet ResolveForContext(SessionId session, SelectionContext context)
        {
            if (context.Kind != SelectionContextKind.One || string.IsNullOrWhiteSpace(context.RackId))
                return EmptySet(session, null);
            return Resolve(session, context.RackId!);
        }

        public bool IsCached(SessionId session, string rackId) =>
            !string.IsNullOrWhiteSpace(rackId) && _cache.ContainsKey(new RackKey(session, rackId));

        public int CachedRackCount(SessionId session)
        {
            var count = 0;
            foreach (var key in _cache.Keys)
                if (key.Session == session) count++;
            return count;
        }

        public void Invalidate(SessionId session, string rackId)
        {
            if (string.IsNullOrWhiteSpace(rackId)) return;
            _cache.Remove(new RackKey(session, rackId));
        }

        public void InvalidateSession(SessionId session)
        {
            var doomed = new List<RackKey>();
            foreach (var key in _cache.Keys)
                if (key.Session == session) doomed.Add(key);
            foreach (var key in doomed) _cache.Remove(key);
        }

        /// <summary>Applies a drain: when it ran the cache invalidation, the session cache is dropped. True when it did.</summary>
        public bool ApplyDrain(SessionId session, DrainResult drain)
        {
            if (!drain.Drained) return false;
            foreach (var operation in drain.Operations)
            {
                if (operation != DrainOperation.InvalidateNavigationCaches) continue;
                InvalidateSession(session);
                return true;
            }

            return false;
        }

        /// <summary>The session was destroyed: its cache goes away and it never resolves again.</summary>
        public void EndSession(SessionId session)
        {
            InvalidateSession(session);
            _ended.Add(session);
        }

        public bool IsSessionEnded(SessionId session) => _ended.Contains(session);

        /// <summary>
        /// Re-reads the document right before an action (INV-40) and compares the expected references with the members the rule
        /// attributes today. A failure drops the cache entry of that rack so the next request refreshes it.
        /// </summary>
        public NavigationRevalidation Revalidate(SessionId session, string rackId, IReadOnlyList<long> expectedHandles)
        {
            if (string.IsNullOrWhiteSpace(rackId) || _ended.Contains(session))
                return new NavigationRevalidation(NavigationRevalidationVerdict.TargetGone, EmptySet(session, rackId));

            var fresh = Compute(session, rackId, out var otherReferences);
            var members = new HashSet<long>(fresh.ReferenceHandles());
            var verdict = NavigationRevalidationVerdict.Valid;
            foreach (var handle in expectedHandles)
            {
                if (members.Contains(handle)) continue;
                verdict = otherReferences.Contains(handle)
                    ? NavigationRevalidationVerdict.NotAttributable
                    : NavigationRevalidationVerdict.TargetGone;
                break;
            }

            var key = new RackKey(session, rackId);
            if (verdict == NavigationRevalidationVerdict.Valid) _cache[key] = fresh;
            else _cache.Remove(key);
            return new NavigationRevalidation(verdict, fresh);
        }

        private static NavigationTargetSet EmptySet(SessionId session, string? rackId) =>
            new(new RackKey(session, rackId ?? string.Empty), Array.Empty<NavigationTarget>(), Array.Empty<NavigationDiagnostic>());

        /// <summary>
        /// The one derivation. Members are non-xref definitions with an interpretable envelope whose non-blank Id equals the RackId
        /// ignoring case. A definition known only by the probe is a diagnostic and never a target. Nothing from the scan is retained
        /// except the targets of the requested rack.
        /// </summary>
        private NavigationTargetSet Compute(SessionId session, string rackId, out HashSet<long> otherReferences)
        {
            var facts = _port.ReadDefinitionFacts(session);
            var candidates = new List<Candidate>();
            var diagnostics = new List<NavigationDiagnostic>();
            otherReferences = new HashSet<long>();

            foreach (var fact in facts)
            {
                if (fact.IsXref) continue;

                if (IsMember(fact, rackId))
                {
                    var order = ViewOrderOf(fact);
                    foreach (var handle in fact.ModelSpaceReferenceHandles)
                        candidates.Add(new Candidate(handle, fact, order));
                    continue;
                }

                if (!fact.IsInterpretable && !string.IsNullOrWhiteSpace(fact.ProbedId)
                    && string.Equals(fact.ProbedId, rackId, StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.Add(new NavigationDiagnostic(NavigationDiagnosticKind.UnreadableMember, fact.DefinitionHandle));
                }

                foreach (var handle in fact.ModelSpaceReferenceHandles) otherReferences.Add(handle);
            }

            candidates.Sort(CompareCandidates);
            var targets = new List<NavigationTarget>(candidates.Count);
            var seen = new HashSet<long>();
            foreach (var candidate in candidates)
            {
                if (!seen.Add(candidate.Handle)) continue;
                var fact = candidate.Fact;
                targets.Add(new NavigationTarget(
                    candidate.Handle, fact.DefinitionHandle, candidate.Order, fact.Kind, fact.View, fact.Section, fact.Name));
            }

            diagnostics.Sort((a, b) => a.DefinitionHandle.CompareTo(b.DefinitionHandle));
            return new NavigationTargetSet(new RackKey(session, rackId), targets, diagnostics);
        }

        private static bool IsMember(NavigationDefinitionFact fact, string rackId) =>
            !fact.IsXref
            && fact.IsInterpretable
            && !string.IsNullOrWhiteSpace(fact.EnvelopeId)
            && string.Equals(fact.EnvelopeId, rackId, StringComparison.OrdinalIgnoreCase);

        private static NavigationViewOrder ViewOrderOf(NavigationDefinitionFact fact)
        {
            var decoded = RackViewCodec.Decode(fact.Kind ?? string.Empty, fact.View!, fact.Section);
            if (!decoded.HasAddress) return NavigationViewOrder.Other;
            switch (decoded.Address.Kind)
            {
                case DimensionViewKind.Frontal: return NavigationViewOrder.Frontal;
                case DimensionViewKind.Lateral: return NavigationViewOrder.Lateral;
                case DimensionViewKind.Planta: return NavigationViewOrder.Planta;
                default: return NavigationViewOrder.Other;
            }
        }

        private static int CompareCandidates(Candidate a, Candidate b)
        {
            var byOrder = a.Order.CompareTo(b.Order);
            if (byOrder != 0) return byOrder;

            if (a.Order == NavigationViewOrder.Other)
            {
                var byView = StringComparer.OrdinalIgnoreCase.Compare(a.Fact.View ?? string.Empty, b.Fact.View ?? string.Empty);
                if (byView != 0) return byView;
                var bySection = a.Fact.Section.CompareTo(b.Fact.Section);
                if (bySection != 0) return bySection;
            }

            var byHandle = a.Handle.CompareTo(b.Handle);
            return byHandle != 0 ? byHandle : a.Fact.DefinitionHandle.CompareTo(b.Fact.DefinitionHandle);
        }

        private readonly record struct Candidate(long Handle, NavigationDefinitionFact Fact, NavigationViewOrder Order);

        private sealed class RackKeyIgnoreCase : IEqualityComparer<RackKey>
        {
            public static readonly RackKeyIgnoreCase Instance = new();

            public bool Equals(RackKey x, RackKey y) =>
                x.Session == y.Session && string.Equals(x.RackId, y.RackId, StringComparison.OrdinalIgnoreCase);

            public int GetHashCode(RackKey key) =>
                HashCode.Combine(key.Session.Value, StringComparer.OrdinalIgnoreCase.GetHashCode(key.RackId ?? string.Empty));
        }
    }
}
