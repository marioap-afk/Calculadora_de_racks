#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>The explicit navigation actions of the panel (D-07). None of them writes the authored data.</summary>
    public enum NavigationActionKind
    {
        Select,
        View,
        Center,
        LocateView,
    }

    public enum NavigationActionVerdict
    {
        Act,
        RejectedNoTargets,
        RejectedSessionMismatch,
        RejectedSessionDestroyed,
        RejectedTargetGone,
        RejectedNotAttributable,
    }

    /// <summary>What the panel asked for, bound to the session that originated it (D-03). Built from the cached targets.</summary>
    public sealed record NavigationActionPlan(
        WorkspaceRequest Origin,
        string RackId,
        NavigationActionKind Action,
        NavigationActionKind Effect,
        IReadOnlyList<long> Handles);

    /// <summary>The pure verdict taken right before executing (INV-40).</summary>
    public sealed record NavigationActionDecision(
        NavigationActionVerdict Verdict,
        IReadOnlyList<long> Handles,
        bool CacheInvalidated,
        bool RefreshRequired);

    /// <summary>The only effects a navigation action may have. The Plugin implements it in F2-T2; T1 only uses a simulated one.</summary>
    public interface INavigationEffects
    {
        void SelectReferences(SessionId session, IReadOnlyList<long> handles);

        void ViewReference(SessionId session, long handle);

        void CenterOnReference(SessionId session, long handle);
    }

    /// <summary>Pure policy: plans from the cached targets and revalidates before any effect. It has no host dependency.</summary>
    public sealed class NavigationActionPolicy
    {
        private readonly NavigationTargets _targets;

        public NavigationActionPolicy(NavigationTargets targets)
        {
            _targets = targets ?? throw new ArgumentNullException(nameof(targets));
        }

        public NavigationActionPlan Plan(
            WorkspaceRequest origin,
            string rackId,
            NavigationActionKind action,
            int copyIndex = 0,
            NavigationViewOrder? view = null,
            NavigationActionKind locateWith = NavigationActionKind.View) =>
            new(origin, rackId, action, action, Array.Empty<long>());

        public NavigationActionDecision Decide(NavigationActionPlan plan, SessionId executingSession) =>
            new(NavigationActionVerdict.RejectedNoTargets, Array.Empty<long>(), false, false);

        public NavigationActionDecision Execute(NavigationActionPlan plan, SessionId executingSession, INavigationEffects effects) =>
            Decide(plan, executingSession);
    }
}
