using System;
using System.Collections.Generic;
using RackCad.Application.Workspace;

namespace RackCad.Plugin.Workspace
{
    /// <summary>Skeleton of the navigation facts adapter (I-64 F2-T2A-NAVIGATION-HOST, RED): no behavior yet.</summary>
    public sealed class WorkspaceNavigationFacts : INavigationFactsPort
    {
        public WorkspaceNavigationFacts(Func<SessionId, long?> nativeInstanceOf)
        {
        }

        public IReadOnlyList<NavigationDefinitionFact> ReadDefinitionFacts(SessionId session) =>
            Array.Empty<NavigationDefinitionFact>();
    }
}
