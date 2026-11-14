using System;
using System.Diagnostics;
using Autodesk.AutoCAD.ApplicationServices;
using RackCad.Application.Diagnostics;
using RackCad.Application.Workspace;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// The one workspace controller of the process (I-64 D-02, P-01). It is created lazily by the first RACKPANEL and lives
    /// until AutoCAD ends; before that there is no controller, no subscription and no work, and the plugin load creates
    /// nothing. It owns the single <see cref="AutoCadEventBridge"/>, the fine selection reader and the pure
    /// <see cref="WorkspaceHostCore"/> that holds every decision; this class only wires them to the host and forwards.
    ///
    /// Every access to the bridge goes through the bridge facades. The only subscriptions it makes are to the bridge's own
    /// in-memory notices (I-64 F1-T3B): <c>Drained</c> (the drain is the only place where the selection is read),
    /// <c>ActiveSessionChanged</c> and, since I-64 F1-T3-HOST-C2, <c>CommandStateChanged</c>. It relays them to the visual
    /// host as one in-memory <see cref="StateChanged"/> notice and never subscribes to AutoCAD. It never starts a thread, holds no host object, writes nothing to the drawing, the
    /// selection or the view and persists nothing.
    /// </summary>
    public sealed class WorkspaceController
    {
        private static readonly WorkspaceLazyInstance<WorkspaceController> Holder =
            new WorkspaceLazyInstance<WorkspaceController>(() => new WorkspaceController());

        private readonly AutoCadEventBridge _bridge;
        private readonly WorkspaceHostCore _host;
        private readonly IWorkspaceHostPort _port;
        private readonly NavigationEcho _echo;
        private readonly WorkspaceNoticeDepth _noticeDepth = new WorkspaceNoticeDepth();
        private readonly IWorkspacePilotEditPort _pilotEdit;
        private readonly WorkspaceCloseAdvisorRelay _closeAdvisor;
        private WorkspaceNavigationHost _navigation;
        private WorkspacePanelModalScope _modalScope;

        private WorkspaceController()
        {
            _bridge = new AutoCadEventBridge();
            _echo = new NavigationEcho();
            _host = new WorkspaceHostCore(
                new BridgeLink(_bridge),
                new HostProbe(_bridge),
                new WorkspaceSelectionReader(_echo),
                new WorkspaceInstrumentation(),
                LogFault,
                _echo);
            _port = new WorkspaceHostPort(_host);

            // The edit port of the pilot (I-64 F4-T3A): the panel never receives the host core, only an adapter over the gate of notices.
            _pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));

            // The lifecycle of the drafts (I-64 F4-T2B2B, F4-T3C2): the bridge asks one advisor about the close and the destruction of a
            // document. That advisor is the wrapper over the host: every decision is the host's, and the wrapper only adds the notices of
            // the command line. It is wired by assignment, so the constructor of the core and the subscriptions of the bridge do not change.
            _closeAdvisor = new WorkspaceCloseAdvisorRelay(
                _host,
                _pilotEdit,
                DocumentNameOf,
                id => _bridge.ActiveSession == id,
                WorkspacePanelCommand.WriteToCommandLine,
                LogFault);
            _bridge.SetLifecycleAdvisor(_closeAdvisor);
            _bridge.Drained += OnDrained;
            _bridge.ActiveSessionChanged += OnActiveSessionChanged;
            _bridge.CommandStateChanged += OnCommandStateChanged;
        }

        /// <summary>
        /// In-memory notice that the state the panel presents may have changed: after a drain, after the active session
        /// changed and after every panel transition. It carries no argument and no AutoCAD type. The visual host pulls a
        /// fresh snapshot through the port when it arrives; nothing is persisted and nothing runs on a timer.
        /// </summary>
        public event Action StateChanged;

        /// <summary>The controller of the process. The first call creates it; later calls return the same one.</summary>
        public static WorkspaceController GetOrCreate() => Holder.GetOrCreate();

        /// <summary>Whether the first RACKPANEL already created the controller.</summary>
        public static bool IsCreated => Holder.IsCreated;

        /// <summary>The controller that was already created, or null. It never creates one (I-64 F4-T3C2).</summary>
        public static WorkspaceController Existing => Holder.Existing;

        /// <summary>The read-only port the panel talks to.</summary>
        public IWorkspaceHostPort Port => _port;

        /// <summary>
        /// The navigation port of the panel (I-64 D-07, F2-T2A). It is created on the first access, never when the controller
        /// is created, and it shares the echo memory of the selection reader. It subscribes to nothing in AutoCAD.
        /// </summary>
        public IWorkspaceNavigationPort Navigation
        {
            get
            {
                if (_navigation == null)
                {
                    _navigation = new WorkspaceNavigationHost(
                        _echo,
                        id => _bridge.TryGetSession(id, out var session) && !session.IsDestroyed ? session.NativeInstanceId : (long?)null,
                        (request, nativeId) => _bridge.Authorize(request, nativeId));

                    // The reader of the edit intention (I-64 F4-T2B2A) is wired by assignment, right after the host exists, so the
                    // constructor of this controller and the arguments of the host do not change. Nothing calls it until F4-T3.
                    _host.SetEditIntentReader(_navigation.EditIntentReader);
                }

                return _navigation;
            }
        }

        /// <summary>
        /// The read-only summary port of the browser of the panel (I-64 F3-T2): the same lazy navigation host, so it creates nothing
        /// new and subscribes to nothing. It writes no selection, no view and nothing of the drawing.
        /// </summary>
        public IWorkspaceBrowserSummaryPort BrowserSummary => (IWorkspaceBrowserSummaryPort)Navigation;

        /// <summary>
        /// The edit port of the bounded Push Back pilot (I-64 F4-T3A, D-T3-06). It is the gate of notices over the host: inside a notice
        /// of the panel the operations that read the drawing answer Waiting. Reading it makes sure the navigation host exists, so the
        /// reader of the edit intention is assigned. It writes nothing and subscribes to nothing.
        /// </summary>
        public IWorkspacePilotEditPort PilotEdit
        {
            get
            {
                _ = Navigation;
                return _pilotEdit;
            }
        }

        /// <summary>
        /// Opens the scope of a modal window of RackCad opened by the panel (I-64 F4-T3A, D-T3-05): the host is told that a modal is
        /// active before this returns, and the last scope that is disposed tells it that it ended, also when the body throws.
        /// </summary>
        public IDisposable EnterPanelModal()
        {
            _modalScope ??= new WorkspacePanelModalScope(active => _host.SetModalRackCadActive(active));
            return _modalScope.Enter();
        }

        public bool PanelVisible => _host.PanelVisible;

        /// <summary>First call: starts the bridge once. Always: shows the panel state and asks for a reconciliation at the next rest.</summary>
        public void OpenPanel()
        {
            _host.OpenPanel();
            NotifyStateChanged();
        }

        public void ShowPanel()
        {
            _host.ShowPanel();
            NotifyStateChanged();
        }

        public void HidePanel()
        {
            _host.HidePanel();
            NotifyStateChanged();
        }

        public void RefreshPanel() => _host.RefreshPanel();

        public void SetModalRackCadActive(bool active) => _host.SetModalRackCadActive(active);

        /// <summary>The in-memory switch of the instrumentation. It changes no setting of the host.</summary>
        public void SetInstrumentationEnabled(bool enabled) => _host.SetInstrumentationEnabled(enabled);

        // The active document changed: a visible panel asks for one reconciliation (at the next rest, through the normal
        // drain); a hidden panel asks for nothing and reads nothing. Either way the visual host is told to pull the state.
        private void OnActiveSessionChanged()
        {
            try
            {
                if (PanelVisible) _host.RefreshPanel();
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceController.OnActiveSessionChanged", ex);
            }

            WriteCloseMessages();
            NotifyStateChanged();
        }

        // The notices of the lifecycle advisor that waited for an active document (a veto is written at once; a loss waits here). The
        // advisor keeps them until it is asked with an active session; a failure is logged and never reaches the bridge.
        private void WriteCloseMessages()
        {
            try
            {
                foreach (var text in _closeAdvisor.TakeMessages(_bridge.ActiveSession))
                    WorkspacePanelCommand.WriteToCommandLine(text);
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceController.WriteCloseMessages", ex);
            }
        }

        // The file name of the document of a session, from memory only (the session record and the document manager); null when the
        // session or the document is gone. No document is kept.
        private string DocumentNameOf(SessionId id)
        {
            if (!_bridge.TryGetSession(id, out var session) || session.IsDestroyed) return null;

            var nativeId = session.NativeInstanceId;
            foreach (Document candidate in AcApplication.DocumentManager)
            {
                if (candidate.UnmanagedObject.ToInt64() == nativeId) return candidate.Name;
            }

            return null;
        }

        // A command started or the document came back to rest (I-64 F1-T3-HOST-C2): the visual host only pulls a fresh
        // snapshot. Nothing is asked of the host core: no reconciliation, no hint, no idle source, no drain and no read.
        private void OnCommandStateChanged() => NotifyStateChanged();

        private void OnDrained(DrainOutcome outcome)
        {
            try
            {
                _host.OnDrained(outcome);
                if (_navigation != null) _navigation.OnDrained(outcome);
                AssertNoOpenTransaction();
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceController.OnDrained", ex);
            }

            // A presentation may read a rack summary only here (I-64 F3-T2, T2-F2): the window of the navigation host is open just
            // around this notice and is always closed, so the notices of a document switch or a command never read the drawing.
            if (_navigation != null) _navigation.OpenDrainNotice();
            try
            {
                NotifyStateChanged();
            }
            finally
            {
                if (_navigation != null) _navigation.CloseDrainNotice();
            }
        }

        // The one place that raises the notice; a failing subscriber is logged and never reaches the bridge or AutoCAD.
        // While the notice is delivered the depth is above zero (I-64 F4-T3A, D-T3-06): the gate of the edit port makes every read of the
        // pilot answer Waiting, so a handler of the notice can never start one.
        private void NotifyStateChanged()
        {
            _noticeDepth.Enter();
            try
            {
                StateChanged?.Invoke();
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceController.StateChanged", ex);
            }
            finally
            {
                _noticeDepth.Leave();
            }
        }

        [Conditional("DEBUG")]
        private static void AssertNoOpenTransaction()
        {
            var document = AcApplication.DocumentManager.MdiActiveDocument;
            if (document == null) return;
            Debug.Assert(document.Database.TransactionManager.TopTransaction == null, "A workspace drain left a transaction open.");
        }

        private static void LogFault(string context, System.Exception exception) => RackLog.Exception(context, exception);

        // The adapter that the panel receives as the edit port: it only forwards to the gate of notices.
        private sealed class PilotEditAdapter : IWorkspacePilotEditPort
        {
            private readonly IWorkspacePilotEditPort _gate;

            public PilotEditAdapter(IWorkspacePilotEditPort gate)
            {
                _gate = gate;
            }

            public EditIntentOutcome TryBeginEdit(EditIntentRequest request) => _gate.TryBeginEdit(request);

            public EditCheckOutcome TryCheckNow(EditIntentRequest request) => _gate.TryCheckNow(request);

            public EditIntentOutcome TryReload(EditIntentRequest request) => _gate.TryReload(request);

            public EditConfirmOutcome TryConfirmBase(EditIntentRequest request) => _gate.TryConfirmBase(request);

            public EditIntentOutcome TryKeepMine(EditIntentRequest request) => _gate.TryKeepMine(request);

            public EditIntentOutcome TryDiscardAndReload(EditIntentRequest request) => _gate.TryDiscardAndReload(request);

            public PilotFieldView GetField(WorkspaceRequest origin, string rackId) => _gate.GetField(origin, rackId);

            public PilotFieldResult SetPendingText(WorkspaceRequest origin, string rackId, string text) =>
                _gate.SetPendingText(origin, rackId, text);

            public PilotFieldResult CommitField(WorkspaceRequest origin, string rackId) => _gate.CommitField(origin, rackId);

            public PilotBoundaryResult CrossBoundary(WorkspaceRequest origin, string rackId, PilotBoundaryKind kind) =>
                _gate.CrossBoundary(origin, rackId, kind);

            public EditDiscardOutcome DiscardRack(WorkspaceRequest origin, string rackId) => _gate.DiscardRack(origin, rackId);

            public EditDiscardOutcome DiscardDocument(WorkspaceRequest origin) => _gate.DiscardDocument(origin);

            public PilotRiskView GetRacksAtRisk(WorkspaceRequest origin) => _gate.GetRacksAtRisk(origin);

            public System.Collections.Generic.IReadOnlyList<PilotLossView> TakeLossNotices() => _gate.TakeLossNotices();

            public System.Collections.Generic.IReadOnlyList<PilotCloseView> TakeCloseNotices() => _gate.TakeCloseNotices();
        }

        // The link of the pure host core to the single bridge: forwards to its facades and nothing else.
        private sealed class BridgeLink : IWorkspaceBridgeLink
        {
            private readonly AutoCadEventBridge _bridge;

            public BridgeLink(AutoCadEventBridge bridge)
            {
                _bridge = bridge;
            }

            public void Start() => _bridge.Start();

            public void SetPanelVisible(bool visible) => _bridge.SetPanelVisible(visible);

            public void SetModalRackCadActive(bool active) => _bridge.SetModalRackCadActive(active);

            public SessionId? ActiveSession => _bridge.ActiveSession;

            public bool TryGetSession(SessionId id, out WorkspaceSession session) => _bridge.TryGetSession(id, out session);

            public RequestDecision Authorize(WorkspaceRequest request, long executingNativeInstanceId) =>
                _bridge.Authorize(request, executingNativeInstanceId);

            public void RequestReconciliation() => _bridge.RequestReconciliation();
        }

        // In-memory facts of the host: the active document and whether a command runs. They read nothing of the drawing.
        private sealed class HostProbe : IWorkspaceHostProbe
        {
            private readonly AutoCadEventBridge _bridge;

            public HostProbe(AutoCadEventBridge bridge)
            {
                _bridge = bridge;
            }

            public long? ActiveDocumentNativeId
            {
                get
                {
                    var document = AcApplication.DocumentManager.MdiActiveDocument;
                    return document == null ? (long?)null : document.UnmanagedObject.ToInt64();
                }
            }

            // A command runs when the editor says so or when the bridge saw it start and not yet come back to rest (I-64
            // F1-T3-HOST-C2): the panel pulls its snapshot inside CommandWillStart, before the editor may report the command.
            public bool IsActiveDocumentQuiescent
            {
                get
                {
                    var document = AcApplication.DocumentManager.MdiActiveDocument;
                    return document != null && document.Editor.IsQuiescent && !_bridge.IsCommandActiveInActiveSession;
                }
            }
        }
    }
}
