#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>RED skeleton: it holds nothing and hands out a fresh, unregistered session each time.</summary>
    public sealed class BrowserSessionRegistry
    {
        public int LiveCount => 0;

        public IReadOnlyList<SessionId> KnownSessions => Array.Empty<SessionId>();

        public BrowserSession? GetOrCreate(SessionId id) => new BrowserSession(id);

        public bool TryGet(SessionId id, out BrowserSession? session)
        {
            session = null;
            return false;
        }

        public void Destroy(SessionId id)
        {
        }
    }
}
