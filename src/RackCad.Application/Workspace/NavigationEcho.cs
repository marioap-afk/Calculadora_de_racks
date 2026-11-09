#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>Skeleton of the navigation echo memory (I-64 F2-T2A-NAVIGATION-HOST, RED): signatures only, no behavior yet.</summary>
    public sealed class NavigationEcho
    {
        public IReadOnlyList<SessionId> TrackedSessions => Array.Empty<SessionId>();

        public bool HasExpectation(SessionId session) => false;

        public bool IsEnded(SessionId session) => false;

        public void Expect(SessionId session, IEnumerable<long> handles)
        {
        }

        public void Observe(SessionId session, IEnumerable<long> impliedHandles)
        {
        }

        public bool IsEcho(SessionId session) => false;

        public void End(SessionId session)
        {
        }

        public IReadOnlyList<SessionId> PurgeDead(Func<SessionId, bool> isAlive) => Array.Empty<SessionId>();
    }
}
