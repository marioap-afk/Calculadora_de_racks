#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.Systems.Shared;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Neutral facts of ONE block definition as the host captured them for the browser summary (I-64 F3-T2, D-05). They carry no
    /// handle of any reference: only what the summary projects. They are private to the panel and are not a reusable foundation.
    /// <see cref="Section"/> is the persisted section of the view (-1 when there is none); with the Kind and the View it is what
    /// the view codec needs to name the real view of the rack.
    /// </summary>
    public sealed record BrowserRackFact(bool IsInterpretable, string? EnvelopeId, string? Kind, string? View, string? Name, int Section = -1);

    /// <summary>
    /// Pure projector and memory of the lazy rack summary (I-64 F3-T2, INV-38). It is NOT a second navigation authority and does
    /// not derive targets: from the facts of a session it projects name, Kind and views of one rack, and remembers only the
    /// summary of the racks that were asked for, keyed by (session, rack). There is no global inventory: facts are read only when
    /// no summary is remembered for that key, and nothing of the scan is kept. A session that ended is never resurrected.
    /// </summary>
    public sealed class BrowserRackSummaries
    {
        private readonly Func<SessionId, IReadOnlyList<BrowserRackFact>> _readFacts;
        private readonly Func<SessionId, bool> _isSessionAlive;
        private readonly Dictionary<RackKey, BrowserRackSummary> _cache = new(RackKeyIgnoreCase.Instance);
        private readonly HashSet<SessionId> _known = new();
        private readonly HashSet<SessionId> _ended = new();

        public BrowserRackSummaries(Func<SessionId, IReadOnlyList<BrowserRackFact>> readFacts, Func<SessionId, bool> isSessionAlive)
        {
            _readFacts = readFacts ?? throw new ArgumentNullException(nameof(readFacts));
            _isSessionAlive = isSessionAlive ?? throw new ArgumentNullException(nameof(isSessionAlive));
        }

        /// <summary>Sessions this memory keeps state for.</summary>
        public int KnownSessionCount => _known.Count;

        public bool IsCached(SessionId session, string rackId) =>
            !string.IsNullOrWhiteSpace(rackId) && _cache.ContainsKey(new RackKey(session, rackId));

        public int CachedRackCount(SessionId session)
        {
            var count = 0;
            foreach (var key in _cache.Keys)
                if (key.Session == session) count++;
            return count;
        }

        public bool IsSessionEnded(SessionId session) => _ended.Contains(session);

        /// <summary>
        /// The summary of the rack in the session. The facts are read only when nothing is remembered for (session, rack); a read
        /// that fails remembers nothing. A session that ended, or that is found dead now, is ended and answers Rejected without
        /// reading.
        /// </summary>
        public BrowserRackSummary Read(SessionId session, string rackId)
        {
            if (_ended.Contains(session)) return Gone();

            PurgeDeadSessions();
            if (_ended.Contains(session)) return Gone();
            if (!_isSessionAlive(session))
            {
                End(session);
                return Gone();
            }

            if (string.IsNullOrWhiteSpace(rackId))
                return BrowserRackSummary.Without(BrowserSummaryStatus.NotFound, "There is no rack id to summarize.");

            _known.Add(session);
            var key = new RackKey(session, rackId);
            if (_cache.TryGetValue(key, out var cached)) return cached;

            var summary = Project(_readFacts(session), rackId);
            _cache[key] = summary;
            return summary;
        }

        /// <summary>
        /// A drain of the session: purges the sessions that no longer live and, under the same condition as the navigation authority
        /// (the drain ran and invalidated the navigation caches), drops the remembered summaries of that session.
        /// </summary>
        public void OnDrained(DrainOutcome outcome)
        {
            PurgeDeadSessions();
            if (_ended.Contains(outcome.Session)) return;

            var drain = outcome.Result;
            if (!drain.Drained) return;
            foreach (var operation in drain.Operations)
            {
                if (operation != DrainOperation.InvalidateNavigationCaches) continue;
                InvalidateSession(outcome.Session);
                return;
            }
        }

        /// <summary>The session was destroyed: its summaries go away and it never answers again.</summary>
        public void EndSession(SessionId session) => End(session);

        private void PurgeDeadSessions()
        {
            var dead = new List<SessionId>();
            foreach (var session in _known)
                if (!_isSessionAlive(session)) dead.Add(session);
            foreach (var session in dead) End(session);
        }

        private void End(SessionId session)
        {
            InvalidateSession(session);
            _known.Remove(session);
            _ended.Add(session);
        }

        private void InvalidateSession(SessionId session)
        {
            var doomed = new List<RackKey>();
            foreach (var key in _cache.Keys)
                if (key.Session == session) doomed.Add(key);
            foreach (var key in doomed) _cache.Remove(key);
        }

        private static BrowserRackSummary Gone() =>
            BrowserRackSummary.Without(BrowserSummaryStatus.Rejected, "The document of the request is gone.");

        // The first non-blank name and Kind among the interpretable facts of the rack, in reading order; the distinct views, each
        // normalized by the view codec and ordered Frontal, Lateral, Planta and then the rest; no interpretable fact of that rack
        // means it is not in the drawing. The rack id is compared ignoring case.
        private static BrowserRackSummary Project(IReadOnlyList<BrowserRackFact> facts, string rackId)
        {
            string? name = null;
            string? kind = null;
            var found = false;
            var views = new List<string>();

            foreach (var fact in facts)
            {
                if (!fact.IsInterpretable || string.IsNullOrWhiteSpace(fact.EnvelopeId)) continue;
                if (!string.Equals(fact.EnvelopeId, rackId, StringComparison.OrdinalIgnoreCase)) continue;

                found = true;
                if (name == null && !string.IsNullOrWhiteSpace(fact.Name)) name = fact.Name;
                if (kind == null && !string.IsNullOrWhiteSpace(fact.Kind)) kind = fact.Kind;
                var view = ViewOf(fact);
                if (view != null && !ContainsIgnoreCase(views, view)) views.Add(view);
            }

            if (!found)
                return BrowserRackSummary.Without(BrowserSummaryStatus.NotFound, "The rack is not in the drawing of the session.");

            views.Sort(CompareViews);
            return BrowserRackSummary.Ready(name, kind, views.ToArray());
        }

        // The view the rack really has: the canonical view of the codec when the persisted syntax has an address (a Cama persists no
        // view at all and still has its lateral one) and, when it has none, the raw non-blank text.
        private static string? ViewOf(BrowserRackFact fact)
        {
            var decoded = RackViewCodec.Decode(fact.Kind ?? string.Empty, fact.View!, fact.Section);
            if (decoded.HasAddress) return decoded.Address.Kind.ToString().ToLowerInvariant();
            return string.IsNullOrWhiteSpace(fact.View) ? null : fact.View;
        }

        private static int CompareViews(string left, string right)
        {
            var byRank = RankOf(left).CompareTo(RankOf(right));
            return byRank != 0 ? byRank : StringComparer.OrdinalIgnoreCase.Compare(left, right);
        }

        private static int RankOf(string view)
        {
            if (string.Equals(view, "frontal", StringComparison.OrdinalIgnoreCase)) return 0;
            if (string.Equals(view, "lateral", StringComparison.OrdinalIgnoreCase)) return 1;
            if (string.Equals(view, "planta", StringComparison.OrdinalIgnoreCase)) return 2;
            return 3;
        }

        private static bool ContainsIgnoreCase(List<string> values, string value)
        {
            foreach (var existing in values)
                if (string.Equals(existing, value, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

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
