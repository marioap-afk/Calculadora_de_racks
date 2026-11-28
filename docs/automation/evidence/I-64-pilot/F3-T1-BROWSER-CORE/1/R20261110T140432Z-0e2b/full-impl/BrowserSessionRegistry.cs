#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Pure, in-memory holder of the browser sessions: one per <see cref="SessionId"/> (I-64 D-03). It dies with the document
    /// and never resurrects: a destroyed id gets no session again. It has no persistence authority.
    /// </summary>
    public sealed class BrowserSessionRegistry
    {
        private readonly Dictionary<SessionId, BrowserSession> _live = new();
        private readonly List<SessionId> _order = new();
        private readonly HashSet<SessionId> _destroyed = new();

        public int LiveCount => _live.Count;

        /// <summary>The ids of the live sessions, in creation order, as a stable snapshot.</summary>
        public IReadOnlyList<SessionId> KnownSessions => Array.AsReadOnly(_order.ToArray());

        /// <summary>The same instance while the session lives; null for an id that was destroyed.</summary>
        public BrowserSession? GetOrCreate(SessionId id)
        {
            if (_destroyed.Contains(id)) return null;
            if (_live.TryGetValue(id, out var existing)) return existing;

            var session = new BrowserSession(id);
            _live.Add(id, session);
            _order.Add(id);
            return session;
        }

        public bool TryGet(SessionId id, out BrowserSession? session)
        {
            return _live.TryGetValue(id, out session);
        }

        /// <summary>
        /// Retires the session: its tabs are emptied and the instance becomes inert. The id is remembered as destroyed even if
        /// no session was ever created for it, so a stale caller can never pair it with a later document.
        /// </summary>
        public void Destroy(SessionId id)
        {
            if (_live.Remove(id, out var session))
            {
                session.Retire();
                _order.Remove(id);
            }

            _destroyed.Add(id);
        }
    }
}
