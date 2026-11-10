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
    /// Every action runs synchronously on the application context, with the document locked and only when no command is in
    /// progress (D-08: the internal command of <c>ExecuteInCommandContextAsync</c> cancels the implicit selection and the view
    /// does not survive it, as Smoke-NAV showed). In this order, in one step: the active document, no command running, the lock,
    /// the session of the request, the existence and the attribution of the targets, the decision of the policy, the effects,
    /// the capture of the result and the return of the focus to the drawing. A session that went stale, another active
    /// document or a target that vanished means zero effects.
    /// </summary>
    public sealed class WorkspaceNavigationHost : IWorkspaceNavigationPort, IWorkspaceBrowserSummaryPort
    {
        private readonly WorkspaceNavigationRunner _runner;
        private readonly WorkspaceNavigationEffects _effects;
        private readonly BrowserRackSummaries _summaries;
        private readonly Func<SessionId, bool> _isSessionAlive;
        private readonly Func<WorkspaceRequest, long, RequestDecision> _authorize;

        public WorkspaceNavigationHost(
            NavigationEcho echo,
            Func<SessionId, long?> nativeInstanceOf,
            Func<WorkspaceRequest, long, RequestDecision> authorize)
        {
            if (echo == null) throw new ArgumentNullException(nameof(echo));
            if (nativeInstanceOf == null) throw new ArgumentNullException(nameof(nativeInstanceOf));
            _authorize = authorize ?? throw new ArgumentNullException(nameof(authorize));
            _isSessionAlive = id => nativeInstanceOf(id).HasValue;

            var facts = new WorkspaceNavigationFacts(nativeInstanceOf);
            var targets = new NavigationTargets(facts);
            var policy = new NavigationActionPolicy(targets);
            _runner = new WorkspaceNavigationRunner(targets, policy, echo, _isSessionAlive);
            _effects = new WorkspaceNavigationEffects(nativeInstanceOf);
            _summaries = new BrowserRackSummaries(facts.ReadBrowserFacts, _isSessionAlive);
        }

        /// <summary>A drain of the session: purges the state of the sessions that no longer live and applies the cache invalidation.</summary>
        public void OnDrained(DrainOutcome outcome)
        {
            try
            {
                _runner.OnDrained(outcome);
                _summaries.OnDrained(outcome);
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceNavigationHost.OnDrained", ex);
            }
        }

        public void OpenDrainNotice()
        {
        }

        public void CloseDrainNotice()
        {
        }

        /// <summary>Whether the session still lives; the browser of the panel destroys the tabs of a session that does not.</summary>
        public bool IsSessionAlive(SessionId session) => _isSessionAlive(session);

        /// <summary>
        /// The lazy summary of one rack for the browser of the panel (I-64 F3-T2). It answers with a value and has no effect: it
        /// writes no selection, no view and nothing of the drawing, and it does not give the focus back. A failure is contained.
        /// </summary>
        public BrowserRackSummary ReadSummary(WorkspaceRequest origin, string rackId, bool userRequested)
        {
            try
            {
                return ReadSummaryOnActiveDocument(origin, rackId);
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceNavigationHost.ReadSummary", ex);
                return BrowserRackSummary.Without(BrowserSummaryStatus.Failed, ex.Message);
            }
        }

        // In this order: the active document, no command in progress and the editor at rest, the session of the request authorized
        // for that document, and only then the memory or the read of the summary.
        private BrowserRackSummary ReadSummaryOnActiveDocument(WorkspaceRequest origin, string rackId)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Rejected, "There is no active document.");
            if (!string.IsNullOrEmpty(document.CommandInProgress) || !document.Editor.IsQuiescent)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Busy, "A command is in progress.");

            var decision = _authorize(origin, document.UnmanagedObject.ToInt64());
            if (decision != RequestDecision.Accepted)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Rejected, "The request does not belong to the active document.");

            return _summaries.Read(origin.Origin, rackId);
        }

        public Task<WorkspaceNavigationResult> ExecuteAsync(
            WorkspaceRequest origin,
            string rackId,
            NavigationActionKind action,
            int copyIndex = 0,
            NavigationViewOrder? view = null,
            NavigationActionKind locateWith = NavigationActionKind.View)
        {
            var request = new WorkspaceNavigationRequest(origin, rackId, action, copyIndex, view, locateWith);
            WorkspaceNavigationResult result;

            try
            {
                result = RunOnActiveDocument(request);
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceNavigationHost.ExecuteAsync", ex);
                result = WorkspaceNavigationResult.Failed(ex.Message);
            }

            return Task.FromResult(result);
        }

        // Runs in one synchronous step on the application context, with the document locked. A running command means the panel
        // acted on old information, so nothing happens and the panel refreshes. The result is captured before the focus goes
        // back to the drawing.
        private WorkspaceNavigationResult RunOnActiveDocument(WorkspaceNavigationRequest request)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null)
                return WorkspaceNavigationResult.Rejected("There is no active document.", NavigationActionVerdict.RejectedSessionMismatch);
            if (!string.IsNullOrEmpty(document.CommandInProgress) || !document.Editor.IsQuiescent)
                return WorkspaceNavigationResult.Stale("A command is in progress.", true);

            WorkspaceNavigationResult result;
            using (document.LockDocument())
            {
                var decision = _authorize(request.Origin, document.UnmanagedObject.ToInt64());
                result = _runner.Run(decision, document.Database.TileMode, request, _effects);
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
