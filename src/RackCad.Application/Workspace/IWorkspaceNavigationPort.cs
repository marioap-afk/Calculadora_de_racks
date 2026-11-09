#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RackCad.Application.Workspace
{
    /// <summary>How a navigation request ended. A normal rejection or failure is a value, never a thrown exception.</summary>
    public enum WorkspaceNavigationStatus
    {
        /// <summary>The effect ran.</summary>
        Executed,

        /// <summary>The action cannot run here (no targets, or not in Model Space). Nothing changed.</summary>
        Unavailable,

        /// <summary>The request does not belong to the document that would execute it. Nothing changed.</summary>
        Rejected,

        /// <summary>The panel acted on old information (destroyed session, target gone or no longer attributable). Nothing changed.</summary>
        Stale,

        /// <summary>The host failed while acting. The failure is contained and reported as a value.</summary>
        Failed,
    }

    /// <summary>Immutable outcome of one navigation request.</summary>
    public readonly record struct WorkspaceNavigationResult(
        WorkspaceNavigationStatus Status,
        NavigationActionVerdict? Verdict,
        bool RefreshRequired,
        string? Reason)
    {
        public bool IsExecuted => Status == WorkspaceNavigationStatus.Executed;

        public static WorkspaceNavigationResult Executed() =>
            new(WorkspaceNavigationStatus.Executed, NavigationActionVerdict.Act, false, null);

        public static WorkspaceNavigationResult Unavailable(string reason, NavigationActionVerdict? verdict = null) =>
            new(WorkspaceNavigationStatus.Unavailable, verdict, false, reason);

        public static WorkspaceNavigationResult Rejected(string reason, NavigationActionVerdict? verdict = null) =>
            new(WorkspaceNavigationStatus.Rejected, verdict, false, reason);

        public static WorkspaceNavigationResult Stale(string reason, bool refreshRequired, NavigationActionVerdict? verdict = null) =>
            new(WorkspaceNavigationStatus.Stale, verdict, refreshRequired, reason);

        public static WorkspaceNavigationResult Failed(string reason) =>
            new(WorkspaceNavigationStatus.Failed, null, false, reason);

        /// <summary>Maps the pure verdict of the policy to the value the panel receives.</summary>
        public static WorkspaceNavigationResult From(NavigationActionDecision decision)
        {
            if (decision == null) throw new ArgumentNullException(nameof(decision));

            switch (decision.Verdict)
            {
                case NavigationActionVerdict.Act:
                    return Executed();
                case NavigationActionVerdict.RejectedNoTargets:
                    return Unavailable("The rack has no target for that action.", decision.Verdict);
                case NavigationActionVerdict.RejectedSessionMismatch:
                    return Rejected("The request belongs to another document.", decision.Verdict);
                case NavigationActionVerdict.RejectedSessionDestroyed:
                    return Stale("The document of the request is gone.", decision.RefreshRequired, decision.Verdict);
                case NavigationActionVerdict.RejectedTargetGone:
                    return Stale("A target of the rack is gone.", decision.RefreshRequired, decision.Verdict);
                default:
                    return Stale("A target no longer belongs to the rack.", decision.RefreshRequired, decision.Verdict);
            }
        }
    }

    /// <summary>What the panel asks of the navigation: the session that originated it and the explicit action.</summary>
    public sealed record WorkspaceNavigationRequest(
        WorkspaceRequest Origin,
        string RackId,
        NavigationActionKind Action,
        int CopyIndex = 0,
        NavigationViewOrder? View = null,
        NavigationActionKind LocateWith = NavigationActionKind.View);

    /// <summary>
    /// The port through which the panel navigates the drawing (I-64 D-07). No host type crosses it. Every request carries the
    /// session that originated it; the host revalidates inside the command context and answers with an immutable value.
    /// </summary>
    public interface IWorkspaceNavigationPort
    {
        Task<WorkspaceNavigationResult> ExecuteAsync(
            WorkspaceRequest origin,
            string rackId,
            NavigationActionKind action,
            int copyIndex = 0,
            NavigationViewOrder? view = null,
            NavigationActionKind locateWith = NavigationActionKind.View);
    }

    /// <summary>Skeleton of the pure core of the navigation host (RED): signatures only, no behavior yet.</summary>
    public sealed class WorkspaceNavigationRunner
    {
        public WorkspaceNavigationRunner(
            NavigationTargets targets,
            NavigationActionPolicy policy,
            NavigationEcho echo,
            Func<SessionId, bool> isSessionAlive)
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (echo == null) throw new ArgumentNullException(nameof(echo));
            if (isSessionAlive == null) throw new ArgumentNullException(nameof(isSessionAlive));
        }

        public int KnownSessionCount => 0;

        public IReadOnlyList<SessionId> PurgeDeadSessions() => Array.Empty<SessionId>();

        public void OnDrained(DrainOutcome outcome)
        {
        }

        public WorkspaceNavigationResult Run(
            RequestDecision sessionDecision,
            bool modelSpaceCurrent,
            WorkspaceNavigationRequest request,
            INavigationEffects effects) =>
            WorkspaceNavigationResult.Failed("The navigation is not implemented yet.");
    }
}
