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

        /// <summary>
        /// Plans an action from the cached targets of the rack (no new read once cached). Select takes every reference. View and
        /// Center take copy <paramref name="copyIndex"/> of the ordered references. LocateView takes copy
        /// <paramref name="copyIndex"/> among the references of <paramref name="view"/> and applies View or Center
        /// (<paramref name="locateWith"/>). Anything without a target is an empty plan.
        /// </summary>
        public NavigationActionPlan Plan(
            WorkspaceRequest origin,
            string rackId,
            NavigationActionKind action,
            int copyIndex = 0,
            NavigationViewOrder? view = null,
            NavigationActionKind locateWith = NavigationActionKind.View)
        {
            var effect = action == NavigationActionKind.LocateView
                ? (locateWith == NavigationActionKind.Center ? NavigationActionKind.Center : NavigationActionKind.View)
                : action;
            var targets = _targets.Resolve(origin.Origin, rackId).Targets;

            var pool = new List<long>();
            foreach (var target in targets)
            {
                if (action == NavigationActionKind.LocateView && (view == null || target.ViewOrder != view.Value)) continue;
                pool.Add(target.ReferenceHandle);
            }

            IReadOnlyList<long> handles;
            if (action == NavigationActionKind.Select) handles = pool;
            else if (copyIndex >= 0 && copyIndex < pool.Count) handles = new[] { pool[copyIndex] };
            else handles = Array.Empty<long>();

            return new NavigationActionPlan(origin, rackId, action, effect, handles);
        }

        /// <summary>
        /// Revalidates right before acting (INV-40, D-03): the executing session must be the origin session and alive, and every
        /// planned reference must still be an attributable target of the rack. Otherwise nothing acts, the rack cache is
        /// invalidated and a refresh is requested. It performs no effect.
        /// </summary>
        public NavigationActionDecision Decide(NavigationActionPlan plan, SessionId executingSession)
        {
            var origin = plan.Origin.Origin;
            if (origin != executingSession) return Rejected(NavigationActionVerdict.RejectedSessionMismatch, false);
            if (_targets.IsSessionEnded(origin)) return Rejected(NavigationActionVerdict.RejectedSessionDestroyed, false);
            if (plan.Handles.Count == 0) return Rejected(NavigationActionVerdict.RejectedNoTargets, false);

            var revalidation = _targets.Revalidate(origin, plan.RackId, plan.Handles);
            switch (revalidation.Verdict)
            {
                case NavigationRevalidationVerdict.Valid:
                    var handles = plan.Effect == NavigationActionKind.Select ? revalidation.Fresh.ReferenceHandles() : plan.Handles;
                    return new NavigationActionDecision(NavigationActionVerdict.Act, handles, false, false);
                case NavigationRevalidationVerdict.NotAttributable:
                    return Rejected(NavigationActionVerdict.RejectedNotAttributable, true);
                default:
                    return Rejected(NavigationActionVerdict.RejectedTargetGone, true);
            }
        }

        /// <summary>Decides and, only when the decision is Act, runs the single effect of the plan on the simulated or real port.</summary>
        public NavigationActionDecision Execute(NavigationActionPlan plan, SessionId executingSession, INavigationEffects effects)
        {
            if (effects == null) throw new ArgumentNullException(nameof(effects));

            var decision = Decide(plan, executingSession);
            if (decision.Verdict != NavigationActionVerdict.Act || decision.Handles.Count == 0) return decision;

            switch (plan.Effect)
            {
                case NavigationActionKind.Select:
                    effects.SelectReferences(executingSession, decision.Handles);
                    break;
                case NavigationActionKind.Center:
                    effects.CenterOnReference(executingSession, decision.Handles[0]);
                    break;
                default:
                    effects.ViewReference(executingSession, decision.Handles[0]);
                    break;
            }

            return decision;
        }

        private static NavigationActionDecision Rejected(NavigationActionVerdict verdict, bool invalidateAndRefresh) =>
            new(verdict, Array.Empty<long>(), invalidateAndRefresh, invalidateAndRefresh);
    }
}
