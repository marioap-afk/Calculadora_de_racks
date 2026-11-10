using System;
using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.Workspace;

namespace RackCad.UI.Workspace
{
    /// <summary>
    /// One tab of the strip as the panel shows it: a plain value (I-64 F3-T2, P-07, INV-02). It carries no event, no delegate, no
    /// object of AutoCAD and no reference to a visual element, so a tab adds no subscription of any kind.
    /// </summary>
    public sealed record WorkspaceBrowserTabItem(string RackId, string Title, bool IsActive, bool IsPreview);

    /// <summary>
    /// Browser of the workspace panel (I-64 F3-T2, D-03, D-06, D-08): a strip of light rack tabs and ONE live surface (K = 1) that
    /// shows the lazy summary of the active tab. It holds the single <see cref="BrowserSessionRegistry"/> of the panel and shows
    /// only the tabs of the session of the presented snapshot.
    ///
    /// The only automatic synchronization is the selection context of the presented snapshot (<see cref="BrowserSession"/>). Changing,
    /// pinning or closing a tab changes memory only: it never writes the selection, the view or the drawing and asks nothing of
    /// the navigation. The summary (name, Kind, views) is asked of the read-only port only for the tab that becomes the live
    /// surface, once per materialization, and only while the drawing can be read and no action is blocked; otherwise it is retried
    /// at the next presentation of that same tab. The views live only in the surface, never in the tab record.
    ///
    /// It is synchronous: no timer, no thread, no task and no dispatcher. Without a port it is inert: no tabs, no surface.
    /// </summary>
    public sealed class WorkspaceBrowserViewModel : ObservableObject
    {
        private enum SurfaceState
        {
            None,
            Pending,
            Busy,
            Ready,
            NotFound,
            Rejected,
            Error,
        }

        private readonly IWorkspaceBrowserSummaryPort _port;
        private readonly Action _tabChanged;
        private readonly BrowserSessionRegistry _registry = new();

        // What the presented snapshot says (set only by Present).
        private SessionId? _session;
        private BrowserSession _current;
        private bool _canRead;
        private WorkspaceActionBlock _block = WorkspaceActionBlock.NotStarted;
        private string _blockText = string.Empty;

        // The one live surface: the tab that is active in the presented session, and what was read for it.
        private SessionId? _surfaceSession;
        private string _surfaceRack;
        private SurfaceState _surfaceState = SurfaceState.None;
        private string _surfaceName;
        private string _surfaceKind;
        private IReadOnlyList<string> _surfaceViews = Array.Empty<string>();
        private string _surfaceReason = string.Empty;

        private IReadOnlyList<WorkspaceBrowserTabItem> _tabs = Array.Empty<WorkspaceBrowserTabItem>();

        /// <param name="port">The read-only summary port; null leaves the browser inert.</param>
        /// <param name="tabChanged">Called after a manual change of tabs, so the panel presents the navigation again with its last snapshot.</param>
        public WorkspaceBrowserViewModel(IWorkspaceBrowserSummaryPort port, Action tabChanged = null)
        {
            _port = port;
            _tabChanged = tabChanged;
        }

        // ------------------------------------------------------------------ what the panel shows

        /// <summary>False when the panel was built without a summary port: the browser is inert.</summary>
        public bool IsAvailable => _port != null;

        /// <summary>The tabs of the session of the presented snapshot, in presentation order; empty without a session.</summary>
        public IReadOnlyList<WorkspaceBrowserTabItem> Tabs => _tabs;

        public bool HasTabs => _tabs.Count > 0;

        /// <summary>The rack of the active tab; null if and only if there is no tab to show.</summary>
        public string ActiveRackId => _current?.ActiveRackId;

        /// <summary>The live surfaces: 0 without tabs and exactly 1 with some (K = 1).</summary>
        public int LiveSurfaceCount => HasSurface ? BrowserSession.MaxLiveSurfaces : 0;

        public bool HasSurface => _current != null && _current.ActiveRackId != null;

        /// <summary>Why there are no tabs; empty while there are.</summary>
        public string EmptyText => HasTabs ? string.Empty : EmptyReason();

        public string SurfaceTitle
        {
            get
            {
                if (!HasSurface) return string.Empty;
                return string.IsNullOrWhiteSpace(_surfaceName) ? _surfaceRack : _surfaceName;
            }
        }

        public string SurfaceKindText =>
            _surfaceState == SurfaceState.Ready
                ? "Tipo: " + (string.IsNullOrWhiteSpace(_surfaceKind) ? "sin dato" : _surfaceKind)
                : string.Empty;

        public string SurfaceViewsText
        {
            get
            {
                if (_surfaceState != SurfaceState.Ready) return string.Empty;
                return _surfaceViews.Count == 0 ? "Vistas: ninguna" : "Vistas: " + string.Join(", ", _surfaceViews);
            }
        }

        /// <summary>The state of the summary in Spanish with its reason: pending or blocked, ready, unavailable or failed.</summary>
        public string SurfaceStatusText => StatusText();

        // ------------------------------------------------------------------ what the panel tells it

        /// <summary>
        /// The panel presented a snapshot. It synchronizes the browser session of that snapshot with its selection context (the only
        /// automatic synchronization), destroys the sessions that no longer live and, when the active tab is not yet the live
        /// surface, asks the port for its summary. It never touches the navigation.
        /// </summary>
        public void Present(SessionId? session, SelectionContext context, bool canReadDrawing, WorkspaceActionBlock block, string blockText)
        {
            if (_port == null) return;

            _canRead = canReadDrawing;
            _block = block;
            _blockText = blockText ?? string.Empty;
            PurgeDeadSessions();

            _session = session;
            _current = session.HasValue ? _registry.GetOrCreate(session.Value) : null;
            if (_current != null && context != null) _current.ApplySelectionContext(context);

            Rebuild(retryPending: true, userRequested: false);
        }

        // ------------------------------------------------------------------ the explicit user interactions

        /// <summary>Makes the tab the active one. Memory only; the tab becomes the live surface and the navigation is presented again.</summary>
        public void ActivateTab(string rackId) => Interact(session => session.Activate(rackId));

        /// <summary>Pins the preview tab so the automatic synchronization never replaces it. Memory only.</summary>
        public void PinTab(string rackId) => Interact(session => session.Pin(rackId));

        /// <summary>Closes the tab without asking anything; if it was active the next one is. Memory only.</summary>
        public void CloseTab(string rackId) => Interact(session => session.Close(rackId));

        public void ReloadSummary()
        {
        }

        // ------------------------------------------------------------------ internals

        private void Interact(Func<BrowserSession, BrowserChange> operation)
        {
            if (_port == null || _current == null) return;

            try
            {
                if (operation(_current) == BrowserChange.None) return;
                Rebuild(retryPending: false, userRequested: true);
                _tabChanged?.Invoke();
            }
            catch (Exception ex)
            {
                ContainFailure(ex);
            }
        }

        private void PurgeDeadSessions()
        {
            foreach (var known in _registry.KnownSessions)
                if (!_port.IsSessionAlive(known)) _registry.Destroy(known);
        }

        // Brings the live surface to the active tab and publishes what the panel shows. Only a presentation retries a summary that was
        // waiting; a tab change asks only for the tab that just became the live surface.
        private void Rebuild(bool retryPending, bool userRequested)
        {
            SyncSurface(retryPending, userRequested);
            RebuildTabs();
            RaiseAll();
        }

        private void SyncSurface(bool retryPending, bool userRequested)
        {
            var rack = _current?.ActiveRackId;
            if (rack == null)
            {
                ClearSurface();
                return;
            }

            // A tab that becomes the live surface starts a new materialization; the same tab presented again does not.
            var started = _surfaceSession != _session || !string.Equals(_surfaceRack, rack, StringComparison.Ordinal);
            if (started)
            {
                _surfaceSession = _session;
                _surfaceRack = rack;
                _surfaceState = SurfaceState.Pending;
                _surfaceName = null;
                _surfaceKind = null;
                _surfaceViews = Array.Empty<string>();
                _surfaceReason = string.Empty;
            }

            if (started || (retryPending && (_surfaceState == SurfaceState.Pending || _surfaceState == SurfaceState.Busy))) Materialize(userRequested);
        }

        private void ClearSurface()
        {
            _surfaceSession = null;
            _surfaceRack = null;
            _surfaceState = SurfaceState.None;
            _surfaceName = null;
            _surfaceKind = null;
            _surfaceViews = Array.Empty<string>();
            _surfaceReason = string.Empty;
        }

        // One query of the port for the rack of the live surface. A blocked drawing is not read at all; the surface stays pending and
        // the next presentation of the same tab tries again.
        private void Materialize(bool userRequested)
        {
            if (!_canRead || _block != WorkspaceActionBlock.None)
            {
                _surfaceState = SurfaceState.Pending;
                return;
            }

            BrowserRackSummary summary;
            try
            {
                summary = _port.ReadSummary(new WorkspaceRequest(_session.Value), _surfaceRack, userRequested)
                    ?? BrowserRackSummary.Without(BrowserSummaryStatus.Failed, "El puerto no devolvió un resumen.");
            }
            catch (Exception ex)
            {
                summary = BrowserRackSummary.Without(BrowserSummaryStatus.Failed, ReasonOf(ex));
            }

            Absorb(summary);
        }

        // The result enters the tab only through ApplySummary; the views stay here, in the surface.
        private void Absorb(BrowserRackSummary summary)
        {
            _surfaceReason = summary.Reason ?? string.Empty;
            switch (summary.Status)
            {
                case BrowserSummaryStatus.Ready:
                    _surfaceState = SurfaceState.Ready;
                    _surfaceName = summary.Name;
                    _surfaceKind = summary.Kind;
                    _surfaceViews = summary.Views ?? Array.Empty<string>();
                    _current.ApplySummary(_surfaceRack, BrowserTabStatus.Ready, summary.Name, summary.Kind);
                    break;
                case BrowserSummaryStatus.Busy:
                    _surfaceState = SurfaceState.Busy;
                    break;
                case BrowserSummaryStatus.NotFound:
                    _surfaceState = SurfaceState.NotFound;
                    _current.ApplySummary(_surfaceRack, BrowserTabStatus.Unavailable, null, null);
                    break;
                case BrowserSummaryStatus.Rejected:
                    _surfaceState = SurfaceState.Rejected;
                    _current.ApplySummary(_surfaceRack, BrowserTabStatus.Unavailable, null, null);
                    break;
                default:
                    _surfaceState = SurfaceState.Error;
                    _current.ApplySummary(_surfaceRack, BrowserTabStatus.Error, null, null);
                    break;
            }
        }

        // The last defence of an interaction: the live tab shows the failure and nothing escapes.
        private void ContainFailure(Exception exception)
        {
            try
            {
                _surfaceState = SurfaceState.Error;
                _surfaceReason = ReasonOf(exception);
                if (_current != null && _surfaceRack != null)
                    _current.ApplySummary(_surfaceRack, BrowserTabStatus.Error, null, null);
                RebuildTabs();
                RaiseAll();
            }
            catch (Exception)
            {
                // Nothing else can be done here without risking the message loop of the host.
            }
        }

        private void RebuildTabs()
        {
            var tabs = new List<WorkspaceBrowserTabItem>();
            if (_current != null)
            {
                var active = _current.ActiveRackId;
                foreach (var tab in _current.Tabs)
                {
                    var title = string.IsNullOrWhiteSpace(tab.Name) ? tab.RackId : tab.Name;
                    tabs.Add(new WorkspaceBrowserTabItem(
                        tab.RackId, title, string.Equals(tab.RackId, active, StringComparison.Ordinal), tab.IsPreview));
                }
            }

            _tabs = tabs;
        }

        private bool IsBlocked => !_canRead || _block != WorkspaceActionBlock.None;

        private string StatusText()
        {
            switch (_surfaceState)
            {
                case SurfaceState.None:
                    return string.Empty;
                case SurfaceState.Pending:
                    return IsBlocked
                        ? "Resumen en espera, no se leyó el dibujo: "
                            + (string.IsNullOrWhiteSpace(_blockText) ? "el dibujo no se puede leer ahora." : _blockText)
                        : "Resumen pendiente de leer.";
                case SurfaceState.Busy:
                    return "Resumen en espera: hay un comando en curso; se reintenta en la siguiente actualización.";
                case SurfaceState.Ready:
                    return "Resumen cargado.";
                case SurfaceState.NotFound:
                    return "Resumen no disponible: el rack ya no está en el dibujo activo.";
                case SurfaceState.Rejected:
                    return "Resumen no disponible: la petición pertenece a otro documento o el documento ya no existe.";
                default:
                    return "No se pudo leer el resumen: " + (string.IsNullOrWhiteSpace(_surfaceReason) ? "fallo sin detalle." : _surfaceReason);
            }
        }

        private string EmptyReason()
        {
            if (_port == null) return "El navegador no está disponible en este panel.";
            if (!_session.HasValue) return "No hay un documento activo: no hay pestañas.";
            return "Selecciona un solo rack en el dibujo para abrir su pestaña.";
        }

        private static string ReasonOf(Exception exception) =>
            string.IsNullOrWhiteSpace(exception?.Message) ? exception?.GetType().Name : exception.Message;

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(IsAvailable));
            OnPropertyChanged(nameof(Tabs));
            OnPropertyChanged(nameof(HasTabs));
            OnPropertyChanged(nameof(ActiveRackId));
            OnPropertyChanged(nameof(LiveSurfaceCount));
            OnPropertyChanged(nameof(HasSurface));
            OnPropertyChanged(nameof(EmptyText));
            OnPropertyChanged(nameof(SurfaceTitle));
            OnPropertyChanged(nameof(SurfaceKindText));
            OnPropertyChanged(nameof(SurfaceViewsText));
            OnPropertyChanged(nameof(SurfaceStatusText));
        }
    }
}
