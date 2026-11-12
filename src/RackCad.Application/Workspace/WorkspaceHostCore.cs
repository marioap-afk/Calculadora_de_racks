#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// The pure host core of the panel (I-64 D-02, D-04, D-06, D-18): the panel lifecycle policy, the active-session
    /// binding, the authorization of every request, the drain-time selection read and the context path. It touches the
    /// host only through <see cref="IWorkspaceBridgeLink"/>, <see cref="IWorkspaceHostProbe"/> and
    /// <see cref="IWorkspaceSelectionReader"/>.
    ///
    /// Rules it enforces:
    /// - before <see cref="OpenPanel"/> nothing is started, nothing is subscribed and nothing is read;
    /// - the first open starts the bridge once; later opens only show the panel again; the lifecycle asks for a
    ///   reconciliation (open, show, refresh) and the drain, never this class, performs it at the next rest;
    /// - the fine selection is read ONLY from <see cref="OnDrained"/> (the drain), only when the pure policy allows it
    ///   (panel visible, document at rest, no RackCad modal, healthy active session), and only to compute the context
    ///   through <see cref="SelectionContext.Classify"/> and <see cref="WorkspaceSession.ApplyContext"/>;
    /// - a hidden panel reads nothing; the hints keep queueing in the bridge at constant cost;
    /// - every request carries its session and is authorized against the document that would execute it;
    /// - a read that fails degrades only that session of the host and never reaches the host loop.
    /// It writes nothing: no authored data, no selection, no view, no persistence.
    /// </summary>
    public sealed class WorkspaceHostCore
    {
        /// <summary>Native identity used when no document is open: it matches no session.</summary>
        private const long NoDocumentNativeId = -1;

        private readonly IWorkspaceBridgeLink _bridge;
        private readonly IWorkspaceHostProbe _probe;
        private readonly IWorkspaceSelectionReader _reader;
        private readonly WorkspaceInstrumentation _instrumentation;
        private readonly Action<string, Exception>? _logFault;
        private readonly NavigationEcho? _echo;
        private readonly HashSet<SessionId> _faulted = new();
        private readonly HashSet<SessionId> _pickFirstOff = new();
        private readonly DraftRegistry _drafts = new();
        private readonly DraftClosePolicy _closePolicy;
        private bool _started;
        private bool _panelVisible;
        private bool _modalRackCadActive;

        public WorkspaceHostCore(
            IWorkspaceBridgeLink bridge,
            IWorkspaceHostProbe probe,
            IWorkspaceSelectionReader reader,
            WorkspaceInstrumentation instrumentation,
            Action<string, Exception>? logFault = null,
            NavigationEcho? echo = null)
        {
            _bridge = bridge ?? throw new ArgumentNullException(nameof(bridge));
            _probe = probe ?? throw new ArgumentNullException(nameof(probe));
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _instrumentation = instrumentation ?? throw new ArgumentNullException(nameof(instrumentation));
            _logFault = logFault;
            _echo = echo;
            _closePolicy = new DraftClosePolicy(_drafts);
        }

        public bool IsStarted => _started;

        public bool PanelVisible => _panelVisible;

        public bool InstrumentationEnabled => _instrumentation.Enabled;

        /// <summary>The in-memory switch of the instrumentation. It changes no setting of the host.</summary>
        public void SetInstrumentationEnabled(bool enabled) => _instrumentation.Enabled = enabled;

        /// <summary>
        /// The first RACKPANEL starts the bridge, once; every later call only shows the panel again. Opening or showing asks
        /// for a reconciliation, which happens at the next rest through the normal drain, never inside this call.
        /// </summary>
        public void OpenPanel()
        {
            using (_instrumentation.Begin(WorkspaceMeasurementPoints.PanelOpen))
            {
                if (!_started)
                {
                    _bridge.Start();
                    _started = true;
                    if (_modalRackCadActive) _bridge.SetModalRackCadActive(true);
                }

                _panelVisible = true;
                _bridge.SetPanelVisible(true);
                _bridge.RequestReconciliation();
            }
        }

        /// <summary>The panel became visible again: it reconciles at the next rest. Nothing happens before the first open.</summary>
        public void ShowPanel()
        {
            if (!_started) return;
            using (_instrumentation.Begin(WorkspaceMeasurementPoints.PanelShow))
            {
                _panelVisible = true;
                _bridge.SetPanelVisible(true);
                _bridge.RequestReconciliation();
            }
        }

        /// <summary>The panel is hidden: no read of the drawing from now on; the hints keep queueing.</summary>
        public void HidePanel()
        {
            if (!_started) return;
            using (_instrumentation.Begin(WorkspaceMeasurementPoints.PanelHide))
            {
                _panelVisible = false;
                _bridge.SetPanelVisible(false);
            }
        }

        /// <summary>Asks for a refresh reconciliation of a visible panel (for example after the active document changed).</summary>
        public void RefreshPanel()
        {
            if (!_started || !_panelVisible) return;
            _bridge.RequestReconciliation();
        }

        /// <summary>Whether a RackCad modal is open, as the host reports it. While true nothing is read.</summary>
        public void SetModalRackCadActive(bool active)
        {
            _modalRackCadActive = active;
            if (_started) _bridge.SetModalRackCadActive(active);
        }

        /// <summary>
        /// The drain: the only place where the selection is read. The bridge has already decided that a drain may run; the
        /// pure policy is checked again here and a refused drain reads nothing.
        /// </summary>
        public void OnDrained(DrainOutcome outcome)
        {
            if (!_started || !outcome.Result.Drained) return;
            try
            {
                using (_instrumentation.Begin(WorkspaceMeasurementPoints.Drain, outcome.Result.Operations.Count))
                {
                    // The echo memory of a session that no longer lives goes away before anything is read (lazy purge).
                    _echo?.PurgeDead(id => _bridge.TryGetSession(id, out _));

                    if (!_bridge.TryGetSession(outcome.Session, out var session) || session == null) return;

                    var conditions = ConditionsFor(outcome.Session);
                    if (!WorkspaceHostPolicy.MayDrain(conditions)) return;

                    // 1. Hints that invalidate: forget what the reader learned about definitions (no read of the drawing).
                    if (outcome.Result.Operations.Contains(DrainOperation.InvalidateNavigationCaches))
                    {
                        using (_instrumentation.Begin(WorkspaceMeasurementPoints.CacheInvalidate))
                        {
                            _reader.Invalidate(outcome.Session);
                        }
                    }

                    // 2. The selection hint: read the implicit selection and recompute the context.
                    if (outcome.Result.Operations.Contains(DrainOperation.ReadImplicitSelection) && WorkspaceHostPolicy.MayRead(conditions))
                    {
                        using (var measurement = _instrumentation.Begin(WorkspaceMeasurementPoints.SelectionRead))
                        {
                            var read = _reader.Read(outcome.Session, session.NativeInstanceId);
                            measurement.SetCount(read.References.Count);
                            if (!read.Succeeded) return;

                            if (read.PickFirstEnabled) _pickFirstOff.Remove(outcome.Session);
                            else _pickFirstOff.Add(outcome.Session);

                            // The selection RackCad itself just wrote (same handles as a set) is an echo of the navigation: the
                            // context, the rack and the tab do not advance. The first different selection resumes the sync.
                            if (_echo != null && _echo.IsEcho(outcome.Session)) return;

                            session.ApplyContext(WorkspaceSelectionMapper.ToContext(read));
                        }
                    }

                    // Drafts do not exist in F1: MarkDraftsPossiblyStale has nothing to mark.
                }
            }
            catch (Exception ex)
            {
                Contain(outcome.Session, "WorkspaceHostCore.OnDrained", ex);
            }
        }

        /// <summary>The state of the panel for the UI. It reads nothing from the drawing and asks for nothing.</summary>
        public WorkspaceHostSnapshot GetSnapshot()
        {
            var active = _started ? _bridge.ActiveSession : null;
            WorkspaceSession? session = null;
            var hasDocument = active.HasValue && _bridge.TryGetSession(active.Value, out session) && session != null;
            var id = hasDocument ? active!.Value : default;
            var conditions = hasDocument
                ? ConditionsFor(id)
                : new WorkspaceHostConditions(_started, false, false, false, _panelVisible, _probe.IsActiveDocumentQuiescent, _modalRackCadActive);

            return new WorkspaceHostSnapshot(
                WorkspaceHostPolicy.Status(conditions),
                hasDocument ? id : null,
                _panelVisible,
                WorkspaceHostPolicy.Actions(conditions),
                hasDocument ? session!.Selection : SelectionContext.None,
                hasDocument && _pickFirstOff.Contains(id),
                _instrumentation.Enabled);
        }

        /// <summary>The verdict for a request: it is checked against the document that would execute it. No side effects.</summary>
        public RequestDecision Authorize(WorkspaceRequest request) =>
            _bridge.Authorize(request, _probe.ActiveDocumentNativeId ?? NoDocumentNativeId);

        /// <summary>The selection context of the request's session when the request is authorized; otherwise nothing and no effect.</summary>
        public WorkspaceSelectionResponse GetSelectionContext(WorkspaceRequest request)
        {
            var decision = Authorize(request);
            if (decision != RequestDecision.Accepted) return new WorkspaceSelectionResponse(decision, null);

            return _bridge.TryGetSession(request.Origin, out var session) && session != null
                ? new WorkspaceSelectionResponse(RequestDecision.Accepted, session.Selection)
                : new WorkspaceSelectionResponse(RequestDecision.RejectedSessionDestroyed, null);
        }

        public WorkspaceMeasurementReport GetMeasurementReport() => _instrumentation.Report();

        // ---------------------------------------------------------------- the edit intention of the pilot (I-64 F4-T2B2A): inert skeleton of the RED

        internal DraftRegistry OpenDrafts => _drafts;

        internal DraftClosePolicy ClosePolicy => _closePolicy;

        /// <summary>Inert skeleton: it assigns nothing and never throws.</summary>
        public void SetEditIntentReader(IWorkspaceEditIntentReader reader)
        {
        }

        /// <summary>Inert skeleton: it reads nothing, creates nothing and never throws.</summary>
        public EditIntentOutcome TryBeginPilotEdit(EditIntentRequest request) => EditIntentOutcome.Unavailable("Not implemented.");

        /// <summary>Inert skeleton: it reads nothing, checks nothing and never throws.</summary>
        public EditCheckOutcome TryCheckPilotEdit(EditIntentRequest request) => EditCheckOutcome.Unavailable("Not implemented.");

        private WorkspaceHostConditions ConditionsFor(SessionId session)
        {
            var active = _bridge.ActiveSession;
            return new WorkspaceHostConditions(
                _started,
                active.HasValue,
                active.HasValue && active.Value == session,
                _faulted.Contains(session),
                _panelVisible,
                _probe.IsActiveDocumentQuiescent,
                _modalRackCadActive);
        }

        // A failed read degrades only that session in the host and is logged; it never reaches the host loop.
        private void Contain(SessionId session, string context, Exception exception)
        {
            _faulted.Add(session);
            try
            {
                _logFault?.Invoke(context, exception);
            }
            catch
            {
                // Logging must never become a second fault.
            }
        }
    }
}
