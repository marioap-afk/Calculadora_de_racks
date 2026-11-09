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

    /// <summary>
    /// The pure core of the navigation host: lazy purge of the state of the sessions that no longer live, the cache
    /// invalidation of a drain and the single execution path that turns a request into a value. It touches the host only
    /// through <see cref="INavigationEffects"/> and the liveness delegate, so it is simulated in tests.
    /// </summary>
    public sealed class WorkspaceNavigationRunner
    {
        private readonly NavigationTargets _targets;
        private readonly NavigationActionPolicy _policy;
        private readonly NavigationEcho _echo;
        private readonly Func<SessionId, bool> _isSessionAlive;
        private readonly HashSet<SessionId> _known = new();

        public WorkspaceNavigationRunner(
            NavigationTargets targets,
            NavigationActionPolicy policy,
            NavigationEcho echo,
            Func<SessionId, bool> isSessionAlive)
        {
            _targets = targets ?? throw new ArgumentNullException(nameof(targets));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
            _echo = echo ?? throw new ArgumentNullException(nameof(echo));
            _isSessionAlive = isSessionAlive ?? throw new ArgumentNullException(nameof(isSessionAlive));
        }

        /// <summary>Sessions the navigation keeps state for.</summary>
        public int KnownSessionCount => _known.Count;

        /// <summary>
        /// Ends the navigation state (targets, echo, pending state) of every session the authority no longer finds alive. A
        /// purged session is never revived. Returns the sessions it purged.
        /// </summary>
        public IReadOnlyList<SessionId> PurgeDeadSessions()
        {
            var dead = new HashSet<SessionId>();
            foreach (var session in _known)
                if (!_isSessionAlive(session)) dead.Add(session);
            foreach (var session in _echo.TrackedSessions)
                if (!_isSessionAlive(session)) dead.Add(session);

            var purged = new List<SessionId>(dead);
            foreach (var session in purged) End(session);
            return purged;
        }

        /// <summary>A drain of the session: purges first, then applies the cache invalidation the drain ran.</summary>
        public void OnDrained(DrainOutcome outcome)
        {
            PurgeDeadSessions();
            if (_targets.IsSessionEnded(outcome.Session)) return;
            _targets.ApplyDrain(outcome.Session, outcome.Result);
        }

        /// <summary>
        /// Runs one request. The session decision and the Model Space condition are taken by the host inside the command
        /// context; everything else (targets, existence, attribution, verdict) is the policy. A verdict other than Act has no
        /// effect. A failing effect is a Failed value.
        /// </summary>
        public WorkspaceNavigationResult Run(
            RequestDecision sessionDecision,
            bool modelSpaceCurrent,
            WorkspaceNavigationRequest request,
            INavigationEffects effects)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (effects == null) throw new ArgumentNullException(nameof(effects));

            PurgeDeadSessions();

            var session = request.Origin.Origin;
            if (_targets.IsSessionEnded(session))
                return WorkspaceNavigationResult.Stale("The document of the request is gone.", false, NavigationActionVerdict.RejectedSessionDestroyed);
            if (sessionDecision == RequestDecision.RejectedSessionDestroyed)
            {
                End(session);
                return WorkspaceNavigationResult.Stale("The document of the request is gone.", false, NavigationActionVerdict.RejectedSessionDestroyed);
            }

            if (sessionDecision != RequestDecision.Accepted)
                return WorkspaceNavigationResult.Rejected("The request belongs to another document.", NavigationActionVerdict.RejectedSessionMismatch);

            if (!_isSessionAlive(session))
            {
                End(session);
                return WorkspaceNavigationResult.Stale("The document of the request is gone.", false, NavigationActionVerdict.RejectedSessionDestroyed);
            }

            _known.Add(session);
            if (!modelSpaceCurrent)
                return WorkspaceNavigationResult.Unavailable("Navigation needs the Model Space.");

            try
            {
                var plan = _policy.Plan(request.Origin, request.RackId, request.Action, request.CopyIndex, request.View, request.LocateWith);
                var decision = _policy.Execute(plan, session, new EchoRecordingEffects(effects, _echo));
                return WorkspaceNavigationResult.From(decision);
            }
            catch (Exception ex)
            {
                return WorkspaceNavigationResult.Failed(ex.Message);
            }
        }

        private void End(SessionId session)
        {
            _targets.EndSession(session);
            _echo.End(session);
            _known.Remove(session);
        }

        // Runs the effect and, only when a selection was really written, remembers its handles as the expected echo.
        private sealed class EchoRecordingEffects : INavigationEffects
        {
            private readonly INavigationEffects _inner;
            private readonly NavigationEcho _echo;

            public EchoRecordingEffects(INavigationEffects inner, NavigationEcho echo)
            {
                _inner = inner;
                _echo = echo;
            }

            public void SelectReferences(SessionId session, IReadOnlyList<long> handles)
            {
                _inner.SelectReferences(session, handles);
                _echo.Expect(session, handles);
            }

            public void ViewReference(SessionId session, long handle) => _inner.ViewReference(session, handle);

            public void CenterOnReference(SessionId session, long handle) => _inner.CenterOnReference(session, handle);
        }
    }
}
