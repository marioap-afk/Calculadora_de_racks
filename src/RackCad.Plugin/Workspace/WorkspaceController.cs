using System;
using System.Diagnostics;
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
    /// in-memory notices (I-64 F1-T3B): <c>Drained</c> (the drain is the only place where the selection is read) and
    /// <c>ActiveSessionChanged</c>. It relays them to the visual host as one in-memory <see cref="StateChanged"/> notice
    /// and never subscribes to AutoCAD. It never starts a thread, holds no host object, writes nothing to the drawing, the
    /// selection or the view and persists nothing.
    /// </summary>
    public sealed class WorkspaceController
    {
        private static readonly WorkspaceLazyInstance<WorkspaceController> Holder =
            new WorkspaceLazyInstance<WorkspaceController>(() => new WorkspaceController());

        private readonly AutoCadEventBridge _bridge;
        private readonly WorkspaceHostCore _host;
        private readonly IWorkspaceHostPort _port;

        private WorkspaceController()
        {
            _bridge = new AutoCadEventBridge();
            _host = new WorkspaceHostCore(
                new BridgeLink(_bridge),
                new HostProbe(),
                new WorkspaceSelectionReader(),
                new WorkspaceInstrumentation(),
                LogFault);
            _port = new WorkspaceHostPort(_host);
            _bridge.Drained += OnDrained;
            _bridge.ActiveSessionChanged += OnActiveSessionChanged;
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

        /// <summary>The read-only port the panel talks to.</summary>
        public IWorkspaceHostPort Port => _port;

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

            NotifyStateChanged();
        }

        private void OnDrained(DrainOutcome outcome)
        {
            try
            {
                _host.OnDrained(outcome);
                AssertNoOpenTransaction();
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceController.OnDrained", ex);
            }

            NotifyStateChanged();
        }

        // The one place that raises the notice; a failing subscriber is logged and never reaches the bridge or AutoCAD.
        private void NotifyStateChanged()
        {
            try
            {
                StateChanged?.Invoke();
            }
            catch (System.Exception ex)
            {
                LogFault("WorkspaceController.StateChanged", ex);
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
            public long? ActiveDocumentNativeId
            {
                get
                {
                    var document = AcApplication.DocumentManager.MdiActiveDocument;
                    return document == null ? (long?)null : document.UnmanagedObject.ToInt64();
                }
            }

            public bool IsActiveDocumentQuiescent
            {
                get
                {
                    var document = AcApplication.DocumentManager.MdiActiveDocument;
                    return document != null && document.Editor.IsQuiescent;
                }
            }
        }
    }
}
