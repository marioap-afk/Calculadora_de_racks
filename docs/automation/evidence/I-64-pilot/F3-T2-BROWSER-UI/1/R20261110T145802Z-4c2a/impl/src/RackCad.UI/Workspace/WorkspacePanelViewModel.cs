using System;
using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.Workspace;

namespace RackCad.UI.Workspace
{
    /// <summary>
    /// View-model of the workspace panel (I-64 F1-T3B). It presents, from the read-only host port, the state of the panel
    /// (not started, no document, ready, degraded), the availability of the actions with its reason, the PICKFIRST notice,
    /// the selection context and the in-memory instrumentation. It is fed only by the injected
    /// <see cref="IWorkspaceHostPort"/> and by a neutral delegate for the instrumentation switch; it takes no service from
    /// the host through a static member and names no host type.
    ///
    /// It asks the port only when it is told to: <see cref="Refresh"/> (the host announced a change) or an explicit user
    /// interaction (<see cref="Retry"/>, <see cref="SetInstrumentation"/>). Never by time. A refresh is one query of the
    /// snapshot and, only when the snapshot says the instrumentation is on, one of the measurement report. It never
    /// authorizes and never asks for the selection context: the context it shows is the one of the snapshot.
    ///
    /// Every entry is contained: a failure becomes the <see cref="WorkspacePanelState.Error"/> presentation state with a
    /// message and a retry, nothing escapes, and the classic commands keep working. Error writes nothing, changes no
    /// session and asks for no reconciliation.
    /// </summary>
    public sealed class WorkspacePanelViewModel : ObservableObject
    {
        private readonly IWorkspaceHostPort _port;
        private readonly Action<bool> _setInstrumentationEnabled;

        private WorkspacePanelState _state = WorkspacePanelState.NotStarted;
        private string _stateTitle = string.Empty;
        private string _stateDetail = string.Empty;
        private string _sessionText = string.Empty;
        private bool _canReadDrawing;
        private bool _canWriteDrawing;
        private WorkspaceActionBlock _actionBlock = WorkspaceActionBlock.None;
        private string _actionBlockText = string.Empty;
        private bool _pickFirstDisabled;
        private SelectionContextKind _contextKind = SelectionContextKind.None;
        private string _contextTitle = string.Empty;
        private string _contextText = string.Empty;
        private string _rackId;
        private SessionId? _presentedSession;
        private SelectionContextKind _presentedKind = SelectionContextKind.None;
        private bool _presentedCanRead;
        private WorkspaceActionBlock _presentedBlock = WorkspaceActionBlock.NotStarted;
        private string _errorMessage = string.Empty;
        private bool _instrumentationEnabled;
        private string _instrumentationSummary = string.Empty;
        private string _processMemoryText = string.Empty;
        private IReadOnlyList<WorkspaceMeasurementRow> _measurements = Array.Empty<WorkspaceMeasurementRow>();

        /// <param name="port">The read-only host port.</param>
        /// <param name="setInstrumentationEnabled">The neutral delegate of the instrumentation switch.</param>
        /// <param name="navigation">
        /// The navigation port (I-64 F2-T2B). Optional: without it the navigation section is present but unavailable. Building
        /// the panel never asks it for anything; it is asked only when the user clicks a navigation action.
        /// </param>
        /// <param name="browserSummary">
        /// The read-only summary port of the browser (I-64 F3-T2). Optional: without it the browser is inert and the panel behaves
        /// exactly as before. It is asked only for the rack of the live surface, never when the panel is built.
        /// </param>
        public WorkspacePanelViewModel(IWorkspaceHostPort port, Action<bool> setInstrumentationEnabled, IWorkspaceNavigationPort navigation = null, IWorkspaceBrowserSummaryPort browserSummary = null)
        {
            _port = port ?? throw new ArgumentNullException(nameof(port));
            _setInstrumentationEnabled = setInstrumentationEnabled ?? throw new ArgumentNullException(nameof(setInstrumentationEnabled));
            Navigation = new WorkspaceNavigationViewModel(navigation, Refresh);
            Browser = new WorkspaceBrowserViewModel(browserSummary, RepresentNavigation);
            Present(InitialSnapshot(), null);
        }

        // ------------------------------------------------------------------ what the panel shows

        /// <summary>The label of the root of the panel.</summary>
        public string Label => "Workspace";

        public WorkspacePanelState State => _state;

        public string StateTitle => _stateTitle;

        public string StateDetail => _stateDetail;

        /// <summary>The empty state: no document is open.</summary>
        public bool IsEmptyState => _state == WorkspacePanelState.NoDocument;

        /// <summary>The session of the active document, or a text that says there is none.</summary>
        public string SessionText => _sessionText;

        public bool CanReadDrawing => _canReadDrawing;

        public bool CanWriteDrawing => _canWriteDrawing;

        public WorkspaceActionBlock ActionBlock => _actionBlock;

        /// <summary>Why the actions are blocked; empty when nothing blocks them.</summary>
        public string ActionBlockText => _actionBlockText;

        public bool PickFirstNoticeVisible => _pickFirstDisabled;

        public string PickFirstNotice => _pickFirstDisabled
            ? "PICKFIRST está desactivado: la selección previa a un comando no se puede leer."
            : string.Empty;

        public SelectionContextKind ContextKind => _contextKind;

        public string ContextTitle => _contextTitle;

        public string ContextText => _contextText;

        /// <summary>The exact RackId of the snapshot when exactly one rack is selected; null for every other context.</summary>
        public string RackId => _rackId;

        public bool HasError => _state == WorkspacePanelState.Error;

        public string ErrorMessage => _errorMessage;

        public bool InstrumentationEnabled => _instrumentationEnabled;

        public string InstrumentationSummary => _instrumentationSummary;

        /// <summary>Current memory of the process as the report gives it; empty while the instrumentation is off.</summary>
        public string ProcessMemoryText => _processMemoryText;

        /// <summary>The measurement points of the report, read-only.</summary>
        public IReadOnlyList<WorkspaceMeasurementRow> Measurements => _measurements;

        /// <summary>The navigation of the rack of the current context (Seleccionar, Ver, Centrar, Localizar vista).</summary>
        public WorkspaceNavigationViewModel Navigation { get; }

        /// <summary>The browser of racks: the tab strip and its one live surface (I-64 F3-T2). Inert without a summary port.</summary>
        public WorkspaceBrowserViewModel Browser { get; }

        // ------------------------------------------------------------------ the only entries that ask the port

        /// <summary>
        /// The host announced a change of state: one query of the snapshot and, when the snapshot says the instrumentation is
        /// on, one of the measurement report. Contained; it never throws.
        /// </summary>
        public void Refresh() => Query();

        /// <summary>Explicit retry of the user, usually after an error: asks for the snapshot again and recovers the normal presentation.</summary>
        public void Retry() => Query();

        /// <summary>
        /// Explicit change of the in-memory instrumentation switch: the injected delegate reaches the host, then the state is
        /// pulled once to show what the host says now. Contained; it never throws.
        /// </summary>
        public void SetInstrumentation(bool enabled)
        {
            try
            {
                _setInstrumentationEnabled(enabled);
            }
            catch (Exception ex)
            {
                EnterError(ex);
                return;
            }

            Query();
            OnPropertyChanged(nameof(InstrumentationEnabled));
        }

        /// <summary>Shows a failure that the view contained as the Error state. It only changes what the panel shows.</summary>
        public void ShowError(Exception exception) => EnterError(exception);

        // ------------------------------------------------------------------ internals

        // What the panel shows before the host has said anything: not started, nothing asked of the port.
        private static WorkspaceHostSnapshot InitialSnapshot() => new WorkspaceHostSnapshot(
            WorkspaceHostStatus.NotStarted,
            null,
            false,
            new WorkspaceActionAvailability(false, false, WorkspaceActionBlock.NotStarted),
            SelectionContext.None,
            false,
            false);

        private void Query()
        {
            try
            {
                var snapshot = _port.GetSnapshot();
                var report = snapshot.InstrumentationEnabled ? _port.GetMeasurementReport() : null;
                Present(snapshot, report);
            }
            catch (Exception ex)
            {
                EnterError(ex);
            }
        }

        private void Present(WorkspaceHostSnapshot snapshot, WorkspaceMeasurementReport report)
        {
            var state = StateOf(snapshot.Status);
            Set(ref _state, state, nameof(State));
            Set(ref _stateTitle, TitleOf(state), nameof(StateTitle));
            Set(ref _stateDetail, DetailOf(state), nameof(StateDetail));
            Set(ref _errorMessage, string.Empty, nameof(ErrorMessage));
            OnPropertyChanged(nameof(IsEmptyState));
            OnPropertyChanged(nameof(HasError));

            Set(ref _sessionText, SessionOf(snapshot), nameof(SessionText));
            Set(ref _canReadDrawing, snapshot.Actions.CanReadDrawing, nameof(CanReadDrawing));
            Set(ref _canWriteDrawing, snapshot.Actions.CanWriteDrawing, nameof(CanWriteDrawing));
            Set(ref _actionBlock, snapshot.Actions.Block, nameof(ActionBlock));
            Set(ref _actionBlockText, BlockText(snapshot.Actions.Block), nameof(ActionBlockText));

            Set(ref _pickFirstDisabled, snapshot.PickFirstDisabled, nameof(PickFirstNoticeVisible));
            OnPropertyChanged(nameof(PickFirstNotice));

            var context = snapshot.Selection ?? SelectionContext.None;
            Set(ref _contextKind, context.Kind, nameof(ContextKind));
            Set(ref _contextTitle, ContextTitleOf(context), nameof(ContextTitle));
            Set(ref _contextText, ContextTextOf(context), nameof(ContextText));
            Set(ref _rackId, context.Kind == SelectionContextKind.One ? context.RackId : null, nameof(RackId));

            // The browser takes the session and the context of this very snapshot first (the one automatic synchronization); then the
            // navigation receives what the snapshot says, in one piece. The values are kept so a manual change of tab can present the
            // navigation again without asking the host for another snapshot.
            _presentedSession = snapshot.ActiveSession;
            _presentedKind = context.Kind;
            _presentedCanRead = snapshot.Actions.CanReadDrawing;
            _presentedBlock = snapshot.Actions.Block;
            Browser.Present(_presentedSession, context, _presentedCanRead, _presentedBlock, _actionBlockText);
            PresentNavigation();

            PresentInstrumentation(snapshot.InstrumentationEnabled, report);
        }

        // With an active tab the navigation follows that rack (a one-rack context); without one it follows the context of the snapshot.
        // It asks nothing of its port: it only stores what it is given.
        private void PresentNavigation()
        {
            var active = Browser.ActiveRackId;
            if (active != null)
                Navigation.Present(_presentedSession, SelectionContextKind.One, active, _presentedCanRead, _presentedBlock, _actionBlockText);
            else
                Navigation.Present(_presentedSession, _presentedKind, _rackId, _presentedCanRead, _presentedBlock, _actionBlockText);
        }

        // A manual change of tab: the navigation is presented again with the data of the last snapshot. While the panel shows an
        // error nothing is known about the drawing, so the navigation stays as the error left it.
        private void RepresentNavigation()
        {
            if (_state == WorkspacePanelState.Error) return;
            PresentNavigation();
        }

        private void PresentInstrumentation(bool enabled, WorkspaceMeasurementReport report)
        {
            Set(ref _instrumentationEnabled, enabled, nameof(InstrumentationEnabled));
            var rows = new List<WorkspaceMeasurementRow>();
            if (enabled && report != null && report.Points != null)
            {
                foreach (var point in report.Points) rows.Add(new WorkspaceMeasurementRow(point));
            }

            _measurements = rows;
            OnPropertyChanged(nameof(Measurements));
            Set(
                ref _instrumentationSummary,
                enabled
                    ? "Instrumentación activada: " + rows.Count.ToString(CultureInfo.InvariantCulture) + " puntos de medida."
                    : "Instrumentación desactivada.",
                nameof(InstrumentationSummary));
            Set(
                ref _processMemoryText,
                enabled && report != null
                    ? "Memoria del proceso: " + WorkspaceMeasurementRow.FormatMemory(report.CurrentProcessMemoryBytes)
                    : string.Empty,
                nameof(ProcessMemoryText));
        }

        // A contained failure of the panel: the Error presentation state, with a readable message and the retry. It only
        // changes what the panel shows.
        private void EnterError(Exception exception)
        {
            var detail = string.IsNullOrWhiteSpace(exception?.Message) ? exception?.GetType().Name : exception.Message;
            Set(ref _errorMessage, "No se pudo actualizar el panel: " + detail, nameof(ErrorMessage));
            Set(ref _state, WorkspacePanelState.Error, nameof(State));
            Set(ref _stateTitle, TitleOf(WorkspacePanelState.Error), nameof(StateTitle));
            Set(ref _stateDetail, DetailOf(WorkspacePanelState.Error), nameof(StateDetail));
            OnPropertyChanged(nameof(IsEmptyState));
            OnPropertyChanged(nameof(HasError));
            OnPropertyChanged(nameof(InstrumentationEnabled));

            // While the panel shows an error nothing is known about the drawing: the navigation is not offered.
            Navigation.Present(null, SelectionContextKind.None, null, false, WorkspaceActionBlock.None, "El panel tuvo un fallo contenido; reintenta antes de navegar.");
        }

        private void Set<T>(ref T field, T value, string name) => SetProperty(ref field, value, name);

        private static WorkspacePanelState StateOf(WorkspaceHostStatus status)
        {
            switch (status)
            {
                case WorkspaceHostStatus.Ready: return WorkspacePanelState.Ready;
                case WorkspaceHostStatus.Degraded: return WorkspacePanelState.Degraded;
                case WorkspaceHostStatus.NoDocument: return WorkspacePanelState.NoDocument;
                default: return WorkspacePanelState.NotStarted;
            }
        }

        private static string TitleOf(WorkspacePanelState state)
        {
            switch (state)
            {
                case WorkspacePanelState.NoDocument: return "Sin documento";
                case WorkspacePanelState.Ready: return "Sesión activa";
                case WorkspacePanelState.Degraded: return "Sesión degradada";
                case WorkspacePanelState.Error: return "Error del panel";
                default: return "Panel sin iniciar";
            }
        }

        private static string DetailOf(WorkspacePanelState state)
        {
            switch (state)
            {
                case WorkspacePanelState.NoDocument: return "Abre o crea un dibujo para trabajar con el panel.";
                case WorkspacePanelState.Ready: return "El panel sigue el dibujo activo.";
                case WorkspacePanelState.Degraded: return "La sesión del dibujo tuvo un fallo y ya no se lee; los comandos de RackCad siguen disponibles.";
                case WorkspacePanelState.Error: return "El panel tuvo un fallo contenido; los comandos de RackCad siguen disponibles.";
                default: return "Abre el panel con RACKPANEL para iniciar el seguimiento del dibujo.";
            }
        }

        private static string SessionOf(WorkspaceHostSnapshot snapshot) =>
            snapshot.ActiveSession.HasValue
                ? "Sesión " + snapshot.ActiveSession.Value.Value.ToString(CultureInfo.InvariantCulture)
                : "Sin sesión activa";

        private static string BlockText(WorkspaceActionBlock block)
        {
            switch (block)
            {
                case WorkspaceActionBlock.NotStarted: return "El panel aún no se inició.";
                case WorkspaceActionBlock.NoDocument: return "No hay un documento abierto.";
                case WorkspaceActionBlock.Degraded: return "La sesión está degradada y no se lee.";
                case WorkspaceActionBlock.ModalActive: return "Hay un cuadro de diálogo de RackCad abierto.";
                case WorkspaceActionBlock.CommandActive: return "Hay un comando en curso.";
                default: return string.Empty;
            }
        }

        private static string ContextTitleOf(SelectionContext context)
        {
            switch (context.Kind)
            {
                case SelectionContextKind.One: return "Un rack";
                case SelectionContextKind.Selection: return "Selección actual";
                case SelectionContextKind.NoIdentity: return "Sin identidad";
                case SelectionContextKind.Diagnostic: return "Diagnóstico";
                default: return "Ninguno";
            }
        }

        private static string ContextTextOf(SelectionContext context)
        {
            switch (context.Kind)
            {
                case SelectionContextKind.One:
                    return "Rack seleccionado: " + context.RackId;
                case SelectionContextKind.Selection:
                    return "Selección actual: "
                        + context.RackCount.ToString(CultureInfo.InvariantCulture) + " racks y "
                        + context.OtherCount.ToString(CultureInfo.InvariantCulture) + " otros objetos.";
                case SelectionContextKind.NoIdentity:
                    return "El objeto seleccionado no tiene identidad de rack.";
                case SelectionContextKind.Diagnostic:
                    return "No se pudo interpretar la definición del objeto seleccionado.";
                default:
                    return "No hay objetos seleccionados.";
            }
        }
    }
}
