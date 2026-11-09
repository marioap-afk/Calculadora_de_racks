using System;
using System.Threading.Tasks;
using RackCad.Application.Workspace;

namespace RackCad.Plugin.Workspace
{
    /// <summary>Skeleton of the navigation host (I-64 F2-T2A-NAVIGATION-HOST, RED): the port and nothing else yet.</summary>
    public sealed class WorkspaceNavigationHost : IWorkspaceNavigationPort
    {
        public WorkspaceNavigationHost(
            NavigationEcho echo,
            Func<SessionId, long?> nativeInstanceOf,
            Func<WorkspaceRequest, long, RequestDecision> authorize)
        {
        }

        public void OnDrained(DrainOutcome outcome)
        {
        }

        public Task<WorkspaceNavigationResult> ExecuteAsync(
            WorkspaceRequest origin,
            string rackId,
            NavigationActionKind action,
            int copyIndex = 0,
            NavigationViewOrder? view = null,
            NavigationActionKind locateWith = NavigationActionKind.View) =>
            Task.FromResult(WorkspaceNavigationResult.Failed("The navigation is not implemented yet."));
    }
}
