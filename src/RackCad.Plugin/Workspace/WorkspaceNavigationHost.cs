using System;
using System.Threading.Tasks;
using RackCad.Application.Diagnostics;
using RackCad.Application.Workspace;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// The navigation host of the workspace panel (I-64 D-03, D-05, D-07, INV-22, INV-40, F2-T2A). It is the port through which
    /// the panel asks to SELECT, VIEW, CENTER or LOCATE VIEW a rack, and the only place that connects the pure navigation core
    /// (<c>NavigationTargets</c>, <c>NavigationActionPolicy</c>) with AutoCAD. It instantiates exactly one of each and nothing
    /// derives a target anywhere else.
    ///
    /// Every action runs in command context, through the public <c>DocumentCollection.ExecuteInCommandContextAsync</c> and
    /// nothing else: the asynchronous part is only a way of getting the right execution context, never background work, and
    /// every task is awaited. Inside the callback, in this order: the active document, the session of the request, the
    /// existence and the attribution of the targets, the decision of the policy, the effects, the capture of the result and
    /// the return of the focus to the drawing. Any read of the drawing and any effect of the action happens inside that
    /// callback, so a session that went stale, another active document or a target that vanished means zero effects.
    /// </summary>
    public sealed class WorkspaceNavigationHost : IWorkspaceNavigationPort
    {
        private readonly WorkspaceNavigationRunner _runner;
        private readonly WorkspaceNavigationEffects _effects;
        private readonly Func<WorkspaceRequest, long, RequestDecision> _authorize;

        public WorkspaceNavigationHost(
            NavigationEcho echo,
            Func<SessionId, long?> nativeInstanceOf,
            Func<WorkspaceRequest, long, RequestDecision> authorize)
        {
            if (echo == null) throw new ArgumentNullException(nameof(echo));
            if (nativeInstanceOf == null) throw new ArgumentNullException(nameof(nativeInstanceOf));
            _authorize = authorize ?? throw new ArgumentNullException(nameof(authorize));

            var targets = new NavigationTargets(new WorkspaceNavigationFacts(nativeInstanceOf));
            var policy = new NavigationActionPolicy(targets);
            _runner = new WorkspaceNavigationRunner(targets, policy, echo, id => nativeInstanceOf(id).HasValue);
            _effects = new WorkspaceNavigationEffects(nativeInstanceOf);
        }

        /// <summary>A drain of the session: purges the state of the sessions that no longer live and applies the cache invalidation.</summary>
        public void OnDrained(DrainOutcome outcome)
        {
            try
            {
                _runner.OnDrained(outcome);
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceNavigationHost.OnDrained", ex);
            }
        }

        public async Task<WorkspaceNavigationResult> ExecuteAsync(
            WorkspaceRequest origin,
            string rackId,
            NavigationActionKind action,
            int copyIndex = 0,
            NavigationViewOrder? view = null,
            NavigationActionKind locateWith = NavigationActionKind.View)
        {
            var request = new WorkspaceNavigationRequest(origin, rackId, action, copyIndex, view, locateWith);
            var result = WorkspaceNavigationResult.Failed("The navigation did not run.");

            try
            {
                await AcApplication.DocumentManager.ExecuteInCommandContextAsync(
                    state =>
                    {
                        result = RunInCommandContext(request);
                        return Task.CompletedTask;
                    },
                    null);
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceNavigationHost.ExecuteAsync", ex);
                result = WorkspaceNavigationResult.Failed(ex.Message);
            }

            return result;
        }

        // Runs inside the command context. A failure is contained and becomes a Failed value; the result is captured before
        // the focus goes back to the drawing.
        private WorkspaceNavigationResult RunInCommandContext(WorkspaceNavigationRequest request)
        {
            var result = WorkspaceNavigationResult.Failed("The navigation did not run.");
            Autodesk.AutoCAD.ApplicationServices.Document document = null;

            try
            {
                document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                    return WorkspaceNavigationResult.Rejected("There is no active document.", NavigationActionVerdict.RejectedSessionMismatch);

                var decision = _authorize(request.Origin, document.UnmanagedObject.ToInt64());
                result = _runner.Run(decision, document.Database.TileMode, request, _effects);
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceNavigationHost.RunInCommandContext", ex);
                result = WorkspaceNavigationResult.Failed(ex.Message);
            }

            if (result.IsExecuted) ReturnFocusToDrawing(document);
            return result;
        }

        // The one place that gives the focus back to the drawing, with the public window API of the document. A failure is
        // contained: it changes nothing in the drawing and never starts a second navigation action.
        private static void ReturnFocusToDrawing(Autodesk.AutoCAD.ApplicationServices.Document document)
        {
            try
            {
                if (document != null) document.Window.Focus();
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceNavigationHost.ReturnFocusToDrawing", ex);
            }
        }

        private static void LogFault(string context, System.Exception exception) => RackLog.Exception(context, exception);
    }
}
