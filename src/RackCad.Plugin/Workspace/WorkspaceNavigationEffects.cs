using System;
using System.Collections.Generic;
using RackCad.Application.Workspace;

namespace RackCad.Plugin.Workspace
{
    /// <summary>Skeleton of the navigation effects adapter (I-64 F2-T2A-NAVIGATION-HOST, RED): no behavior yet.</summary>
    public sealed class WorkspaceNavigationEffects : INavigationEffects
    {
        public WorkspaceNavigationEffects(Func<SessionId, long?> nativeInstanceOf)
        {
        }

        public void SelectReferences(SessionId session, IReadOnlyList<long> handles)
        {
        }

        public void ViewReference(SessionId session, long handle)
        {
        }

        public void CenterOnReference(SessionId session, long handle)
        {
        }
    }
}
