#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// In-memory echo memory of the navigation (I-64 D-07, INV-03), by content and by session. When RackCad itself writes the
    /// implicit selection (the SELECT action), the set of handles it wrote is remembered here. The fine selection reader feeds
    /// the set of handles it read on the normal drain, and the host core asks whether that read is the echo of the write: the
    /// two sets are compared as SETS, so neither the order nor a duplicate matters. An equal set is the echo of the
    /// navigation and changes no context; the first different selection forgets the expectation and the normal
    /// synchronization resumes. There is no token of the next event, no flag, no time window and no timer.
    /// The state belongs to a session and goes away when the session no longer lives.
    /// </summary>
    public sealed class NavigationEcho
    {
        private readonly Dictionary<SessionId, HashSet<long>> _expected = new();
        private readonly Dictionary<SessionId, HashSet<long>> _observed = new();
        private readonly HashSet<SessionId> _ended = new();

        /// <summary>The sessions that hold any expectation or observation.</summary>
        public IReadOnlyList<SessionId> TrackedSessions
        {
            get
            {
                var sessions = new HashSet<SessionId>(_expected.Keys);
                sessions.UnionWith(_observed.Keys);
                return new List<SessionId>(sessions);
            }
        }

        public bool HasExpectation(SessionId session) => _expected.ContainsKey(session);

        public bool IsEnded(SessionId session) => _ended.Contains(session);

        /// <summary>RackCad wrote this implicit selection. An empty set expects nothing. An ended session is never revived.</summary>
        public void Expect(SessionId session, IEnumerable<long> handles)
        {
            if (handles == null) throw new ArgumentNullException(nameof(handles));
            if (_ended.Contains(session)) return;

            var set = new HashSet<long>(handles);
            if (set.Count == 0) _expected.Remove(session);
            else _expected[session] = set;
        }

        /// <summary>The reader read this complete implicit selection on the drain. An ended session is never revived.</summary>
        public void Observe(SessionId session, IEnumerable<long> impliedHandles)
        {
            if (impliedHandles == null) throw new ArgumentNullException(nameof(impliedHandles));
            if (_ended.Contains(session)) return;

            _observed[session] = new HashSet<long>(impliedHandles);
        }

        /// <summary>
        /// Consumes the last observation: true when it equals, as a set, what RackCad wrote (the echo of the navigation). A
        /// different observation forgets the expectation, so the first different selection resumes the normal flow.
        /// </summary>
        public bool IsEcho(SessionId session)
        {
            if (!_observed.TryGetValue(session, out var observed)) return false;
            _observed.Remove(session);

            if (!_expected.TryGetValue(session, out var expected)) return false;
            if (expected.SetEquals(observed)) return true;

            _expected.Remove(session);
            return false;
        }

        /// <summary>The session no longer lives: its state goes away and it is never revived.</summary>
        public void End(SessionId session)
        {
            _expected.Remove(session);
            _observed.Remove(session);
            _ended.Add(session);
        }

        /// <summary>Ends every tracked session that the authority no longer finds alive. Returns the sessions it ended.</summary>
        public IReadOnlyList<SessionId> PurgeDead(Func<SessionId, bool> isAlive)
        {
            if (isAlive == null) throw new ArgumentNullException(nameof(isAlive));

            var purged = new List<SessionId>();
            foreach (var session in TrackedSessions)
            {
                if (isAlive(session)) continue;
                End(session);
                purged.Add(session);
            }

            return purged;
        }
    }
}
