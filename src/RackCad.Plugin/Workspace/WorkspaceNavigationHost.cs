using System;
using System.Threading.Tasks;
using RackCad.Application.Diagnostics;
using RackCad.Application.Workspace;
using RackCad.Plugin.Views;
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
    ///
    /// It also hosts, as a nested class, the reader of the edit intention (I-64 F4-T2B2A, D-T2B2-03). That reader is the one
    /// place where the intention reaches the drawing: it handles the result of the pilot that the adapter of the Views folder
    /// returns, as a value, and it reuses the targets of this host. It reads only; it writes no selection, no view and no data.
    /// </summary>
    public sealed class WorkspaceNavigationHost : IWorkspaceNavigationPort, IWorkspaceBrowserSummaryPort
    {
        private readonly WorkspaceNavigationRunner _runner;
        private readonly WorkspaceNavigationEffects _effects;
        private readonly BrowserRackSummaries _summaries;
        private readonly NavigationTargets _targets;
        private readonly IWorkspaceEditIntentReader _editIntentReader;
        private readonly Func<SessionId, bool> _isSessionAlive;
        private readonly Func<WorkspaceRequest, long, RequestDecision> _authorize;

        // True only while the controller announces a drain (I-64 F3-T2, T2-F2): the one moment in which a presentation of the panel may
        // ask for a summary. Outside it the notices of AutoCAD (a document switch, a command start or end) only present, never read.
        private bool _drainNoticeOpen;

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
            _targets = new NavigationTargets(facts);
            var policy = new NavigationActionPolicy(_targets);
            _runner = new WorkspaceNavigationRunner(_targets, policy, echo, _isSessionAlive);
            _effects = new WorkspaceNavigationEffects(nativeInstanceOf);
            _summaries = new BrowserRackSummaries(facts.ReadBrowserFacts, _isSessionAlive);
            _editIntentReader = new PilotIntentReader(_targets, _authorize);
        }

        /// <summary>
        /// The reader of the edit intention of this host (I-64 F4-T2B2A). The controller hands it to the host core by assignment,
        /// right after it creates this host. Nothing in this class calls it: it is called only by an action of the user.
        /// </summary>
        internal IWorkspaceEditIntentReader EditIntentReader => _editIntentReader;

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

        /// <summary>Opens the window in which a presentation of the panel may read a summary: the controller opens it around the notice of a drain.</summary>
        public void OpenDrainNotice() => _drainNoticeOpen = true;

        /// <summary>Closes that window; the controller closes it in a finally, so it never stays open.</summary>
        public void CloseDrainNotice() => _drainNoticeOpen = false;

        /// <summary>Whether the session still lives; the browser of the panel destroys the tabs of a session that does not.</summary>
        public bool IsSessionAlive(SessionId session) => _isSessionAlive(session);

        /// <summary>
        /// The lazy summary of one rack for the browser of the panel (I-64 F3-T2). It answers with a value and has no effect: it
        /// writes no selection, no view and nothing of the drawing, and it does not give the focus back. A failure is contained.
        /// A read that no explicit action of the user asked for is answered only during the notice of a drain.
        /// </summary>
        public BrowserRackSummary ReadSummary(WorkspaceRequest origin, string rackId, bool userRequested)
        {
            try
            {
                return ReadSummaryOnActiveDocument(origin, rackId, userRequested);
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceNavigationHost.ReadSummary", ex);
                return BrowserRackSummary.Without(BrowserSummaryStatus.Failed, ex.Message);
            }
        }

        // In this order: the active document, no command in progress and the editor at rest, a read that nobody asked for only while
        // a drain is announced, the session of the request authorized for that document, and only then the memory or the read of the
        // summary.
        private BrowserRackSummary ReadSummaryOnActiveDocument(WorkspaceRequest origin, string rackId, bool userRequested)
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Rejected, "There is no active document.");
            if (!string.IsNullOrEmpty(document.CommandInProgress) || !document.Editor.IsQuiescent)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Busy, "A command is in progress.");
            if (!userRequested && !_drainNoticeOpen)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Busy, "The drawing is not read inside a notice of AutoCAD; it is asked again at the next drain.");

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

        /// <summary>
        /// The reader of the edit intention (I-64 F4-T2B2A, D-T2B2-03, INV-22). ONE logical read of the drawing for one action of the
        /// user, synchronous, on the application context. In this order, in one step: the active document, no command running and
        /// the editor at rest, the catalog (outside the lock), the document lock, the authorization of the session against the
        /// document that executes it, the revalidation of the targets with the choice of the origin, and the read of the adapter.
        /// A failure is contained as a value. It writes no selection, no view and nothing of the drawing, and it gives no focus back.
        /// </summary>
        private sealed class PilotIntentReader : IWorkspaceEditIntentReader
        {
            private readonly NavigationTargets _targets;
            private readonly Func<WorkspaceRequest, long, RequestDecision> _authorize;

            public PilotIntentReader(NavigationTargets targets, Func<WorkspaceRequest, long, RequestDecision> authorize)
            {
                _targets = targets;
                _authorize = authorize;
            }

            public EditIntentReadResult ReadIntent(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness)
            {
                try
                {
                    return ReadOnActiveDocument(session, rackId, chosenReference, witness);
                }
                catch (System.Exception ex)
                {
                    LogFault("WorkspaceNavigationHost.ReadEditIntent", ex);
                    return EditIntentReadResult.Failed(ex.Message);
                }
            }

            private EditIntentReadResult ReadOnActiveDocument(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness)
            {
                var document = AcApplication.DocumentManager.MdiActiveDocument;
                if (document == null)
                    return EditIntentReadResult.NoActiveDocument("No hay documento activo.");
                if (!string.IsNullOrEmpty(document.CommandInProgress) || !document.Editor.IsQuiescent)
                    return EditIntentReadResult.Busy("Hay un comando en curso.");

                var catalog = RackPilotIntentAdapter.LoadCatalog();
                if (!catalog.Succeeded)
                    return EditIntentReadResult.Failed(catalog.FailureReason);

                using (document.LockDocument())
                {
                    var decision = _authorize(new WorkspaceRequest(session), document.UnmanagedObject.ToInt64());
                    if (decision != RequestDecision.Accepted)
                        return EditIntentReadResult.Rejected("La petición no es del documento activo.");

                    var expected = chosenReference.HasValue ? new[] { chosenReference.Value } : Array.Empty<long>();
                    var revalidation = _targets.Revalidate(session, rackId, expected);
                    var origin = WorkspaceEditIntent.ChooseOriginDefinition(revalidation.Fresh, chosenReference);

                    var read = RackPilotIntentAdapter.Read(
                        document,
                        rackId,
                        origin,
                        witness?.Key,
                        witness?.Text,
                        catalog.Catalog);
                    return read.Succeeded
                        ? EditIntentReadResult.FromPilot(read.Pilot)
                        : EditIntentReadResult.Failed(read.FailureReason);
                }
            }
        }
    }
}
