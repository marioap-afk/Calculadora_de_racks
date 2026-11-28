using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using RackCad.Application.Workspace;

namespace RackCad.UI.Workspace
{
    /// <summary>One entry of the explicit view choice of "Localizar vista".</summary>
    public sealed class WorkspaceViewChoice
    {
        public WorkspaceViewChoice(NavigationViewOrder order, string label)
        {
            Order = order;
            Label = label ?? string.Empty;
        }

        public NavigationViewOrder Order { get; }

        public string Label { get; }
    }

    /// <summary>
    /// Navigation of the workspace panel (I-64 F2-T2B, D-05, D-07): for the rack of the current context it offers Seleccionar,
    /// Ver, Centrar and Localizar vista, with the copy "k of M" and the explicit view. It talks only to the
    /// <see cref="IWorkspaceNavigationPort"/>; it never derives targets, never orders references, never decides the attribution
    /// and keeps no inventory: the authority answers and this view-model presents the answer as a value.
    ///
    /// It asks the port only when the user clicks an action (<see cref="NavigateAsync"/>), never by time, never when it is
    /// built and never because the context changed. The actions are enabled only with exactly one rack in the context, a
    /// readable drawing, no action block and no action in progress; otherwise the command does not call the port and the reason
    /// is shown. Each request carries the session and the rack of the same presented snapshot, captured together at the click.
    ///
    /// The copy index k belongs to this view-model and travels to the port; M is only what the port answers
    /// (<see cref="WorkspaceNavigationResult.TargetCount"/>), unknown until then. Every failure is contained and presented.
    /// </summary>
    public sealed class WorkspaceNavigationViewModel : ObservableObject
    {
        private readonly IWorkspaceNavigationPort _port;
        private readonly Action _refreshRequested;
        private readonly List<WorkspaceViewChoice> _viewChoices = new List<WorkspaceViewChoice>
        {
            new WorkspaceViewChoice(NavigationViewOrder.Frontal, "Frontal"),
            new WorkspaceViewChoice(NavigationViewOrder.Lateral, "Lateral"),
            new WorkspaceViewChoice(NavigationViewOrder.Planta, "Planta"),
            new WorkspaceViewChoice(NavigationViewOrder.Other, "Otras"),
        };

        // What the presented snapshot says (set only by Present).
        private SessionId? _session;
        private SelectionContextKind _kind = SelectionContextKind.None;
        private string _rackId;
        private bool _canRead;
        private WorkspaceActionBlock _block = WorkspaceActionBlock.NotStarted;
        private string _blockText = string.Empty;

        private bool _busy;
        private string _resultText = string.Empty;

        // The copy: k is zero-based, M is only what the port answered. The scope is the requested view the count belongs to
        // (null for the whole set of View and Center). The epoch tells an answer that the target changed in the meantime.
        private int _copyIndex;
        private int? _targetCount;
        private NavigationViewOrder? _scopeView;
        private int _epoch;
        private int _targetEpoch;

        private NavigationViewOrder _selectedView = NavigationViewOrder.Frontal;
        private bool _locateCentered;

        /// <param name="port">The navigation port; null leaves the actions unavailable with a reason.</param>
        /// <param name="refreshRequested">Asks the panel for one new snapshot; called once per answer that requires a refresh.</param>
        public WorkspaceNavigationViewModel(IWorkspaceNavigationPort port, Action refreshRequested = null)
        {
            _port = port;
            _refreshRequested = refreshRequested;
        }

        // ------------------------------------------------------------------ what the panel shows

        /// <summary>False when the panel was built without a navigation port.</summary>
        public bool IsAvailable => _port != null;

        public bool IsBusy => _busy;

        /// <summary>Seleccionar, Ver, Centrar and Localizar vista are enabled only here.</summary>
        public bool CanNavigate =>
            _port != null
            && !_busy
            && _session.HasValue
            && _kind == SelectionContextKind.One
            && !string.IsNullOrWhiteSpace(_rackId)
            && _canRead
            && _block == WorkspaceActionBlock.None;

        /// <summary>Why the actions are not enabled; empty while they are.</summary>
        public string BlockedReason => CanNavigate ? string.Empty : ReasonOfBlock();

        public bool HasBlockedReason => !CanNavigate;

        /// <summary>The views the user can ask Localizar vista for.</summary>
        public IReadOnlyList<WorkspaceViewChoice> ViewChoices => _viewChoices;

        /// <summary>The explicit view of Localizar vista. Changing it restarts the copy.</summary>
        public NavigationViewOrder SelectedView
        {
            get => _selectedView;
            set
            {
                if (_selectedView == value) return;
                _selectedView = value;
                OnPropertyChanged(nameof(SelectedView));
                ResetCopy();
            }
        }

        /// <summary>Localizar vista centers instead of fitting the view; View is the default.</summary>
        public bool LocateCentered
        {
            get => _locateCentered;
            set
            {
                if (_locateCentered == value) return;
                _locateCentered = value;
                OnPropertyChanged(nameof(LocateCentered));
                OnPropertyChanged(nameof(LocateWith));
            }
        }

        public NavigationActionKind LocateWith => _locateCentered ? NavigationActionKind.Center : NavigationActionKind.View;

        /// <summary>The copy index k (zero-based) that View, Center and Localizar vista send to the port.</summary>
        public int CopyIndex => _copyIndex;

        /// <summary>M: the number of targets the port answered for the last action; null while it is unknown.</summary>
        public int? TargetCount => _targetCount;

        public string CopyText => _targetCount.HasValue && _targetCount.Value == 0
            ? "Sin objetivos"
            : "Copia " + Ordinal(_copyIndex) + " de " + CountText(_targetCount);

        /// <summary>The next copy is offered only once M is known and there is more than one.</summary>
        public bool CanNextCopy => _targetCount.HasValue && _targetCount.Value > 1;

        /// <summary>The outcome of the last action in Spanish; empty before the first one.</summary>
        public string ResultText => _resultText;

        public bool HasResult => _resultText.Length > 0;

        // ------------------------------------------------------------------ what the panel tells it

        /// <summary>
        /// The panel presented a snapshot. It only stores what the snapshot says; it asks nothing of the port. A new rack or
        /// session restarts the copy and clears the last answer. It never redirects an action already in progress.
        /// </summary>
        public void Present(
            SessionId? session,
            SelectionContextKind kind,
            string rackId,
            bool canReadDrawing,
            WorkspaceActionBlock block,
            string blockText)
        {
            var rack = kind == SelectionContextKind.One ? rackId : null;
            var sameTarget = _session == session && string.Equals(_rackId, rack, StringComparison.Ordinal);

            _session = session;
            _kind = kind;
            _rackId = rack;
            _canRead = canReadDrawing;
            _block = block;
            _blockText = blockText ?? string.Empty;

            if (!sameTarget)
            {
                _targetEpoch++;
                ResetCopy();
                SetResult(string.Empty);
            }

            RaiseAvailability();
        }

        // ------------------------------------------------------------------ the explicit user interactions

        /// <summary>Moves to the next copy; after the last one it returns to the first. Only when M is known.</summary>
        public void NextCopy()
        {
            if (!CanNextCopy) return;
            _copyIndex = (_copyIndex + 1) % _targetCount.Value;
            RaiseCopy();
        }

        public Task SelectAsync() => NavigateAsync(NavigationActionKind.Select);

        public Task ViewAsync() => NavigateAsync(NavigationActionKind.View);

        public Task CenterAsync() => NavigateAsync(NavigationActionKind.Center);

        public Task LocateAsync() => NavigateAsync(NavigationActionKind.LocateView);

        /// <summary>
        /// The only entry that asks the port: one request per click, awaited on the UI thread. Disabled actions return without
        /// calling the port. Every failure is contained and presented, so the returned task never faults.
        /// </summary>
        public async Task NavigateAsync(NavigationActionKind action)
        {
            try
            {
                if (!CanNavigate) return;

                // One capture: the session and the rack of the same presented snapshot, and the copy and the view asked for.
                var origin = new WorkspaceRequest(_session.Value);
                var rackId = _rackId;
                NavigationViewOrder? view = action == NavigationActionKind.LocateView ? _selectedView : (NavigationViewOrder?)null;
                var locateWith = LocateWith;
                var copyIndex = 0;
                if (action != NavigationActionKind.Select)
                {
                    EnterScope(view);
                    copyIndex = _copyIndex;
                }

                var epoch = _epoch;
                var targetEpoch = _targetEpoch;
                SetBusy(true);

                WorkspaceNavigationResult result;
                try
                {
                    result = await _port.ExecuteAsync(origin, rackId, action, copyIndex, view, locateWith);
                }
                catch (Exception ex)
                {
                    result = WorkspaceNavigationResult.Failed(ex.Message);
                }

                SetBusy(false);
                Absorb(action, view, copyIndex, epoch, targetEpoch, result);
                if (result.RefreshRequired && _refreshRequested != null) _refreshRequested();
            }
            catch (Exception ex)
            {
                ContainFailure(ex);
            }
        }

        // ------------------------------------------------------------------ internals

        // View, Center and Locate share one k; a different requested view is a different scope with its own count.
        private void EnterScope(NavigationViewOrder? view)
        {
            if (_scopeView == view) return;
            ResetCopy();
            _scopeView = view;
        }

        private void ResetCopy()
        {
            _epoch++;
            _copyIndex = 0;
            _targetCount = null;
            _scopeView = null;
            RaiseCopy();
        }

        private void Absorb(NavigationActionKind action, NavigationViewOrder? view, int copyIndex, int epoch, int targetEpoch, WorkspaceNavigationResult result)
        {
            // An answer about a rack that is no longer the presented one is not shown and does not feed the copy.
            if (targetEpoch != _targetEpoch) return;

            SetResult(TextOf(action, view, copyIndex, result));
            if (action == NavigationActionKind.Select || epoch != _epoch) return;

            // M comes only from the answer of the port; a target that shrank sends k back to the first copy.
            _targetCount = result.TargetCount;
            if (_targetCount.HasValue && _copyIndex >= _targetCount.Value) _copyIndex = 0;
            RaiseCopy();
        }

        private string TextOf(NavigationActionKind action, NavigationViewOrder? view, int copyIndex, WorkspaceNavigationResult result)
        {
            switch (result.Status)
            {
                case WorkspaceNavigationStatus.Executed:
                    return ExecutedText(action, view, copyIndex, result.TargetCount);
                case WorkspaceNavigationStatus.Unavailable:
                    if (result.Verdict.HasValue && result.TargetCount.HasValue && result.TargetCount.Value > 0)
                        return "La copia pedida ya no existe; no se hizo nada. Se vuelve a la primera copia.";
                    return result.Verdict.HasValue
                        ? "El rack no tiene objetivos para esa acción; no se hizo nada."
                        : "La navegación necesita el Espacio Modelo; el panel no cambia de espacio. Vuelve a Modelo y repite la acción.";
                case WorkspaceNavigationStatus.Rejected:
                    return "La petición pertenece a otro documento; no se hizo nada.";
                case WorkspaceNavigationStatus.Stale when !result.Verdict.HasValue && result.RefreshRequired:
                    return "Hay un comando en curso; no se hizo nada. Termínalo y repite la acción.";
                case WorkspaceNavigationStatus.Stale:
                    return result.RefreshRequired
                        ? "El dibujo cambió desde que se mostró el panel; no se hizo nada. El panel se actualiza."
                        : "El documento de la petición ya no existe; no se hizo nada.";
                default:
                    return "No se pudo completar la navegación: "
                        + (string.IsNullOrWhiteSpace(result.Reason) ? "fallo sin detalle." : result.Reason);
            }
        }

        private string ExecutedText(NavigationActionKind action, NavigationViewOrder? view, int copyIndex, int? count)
        {
            var copy = "copia " + Ordinal(copyIndex) + " de " + CountText(count);
            switch (action)
            {
                case NavigationActionKind.Select:
                    return "Rack seleccionado en el dibujo.";
                case NavigationActionKind.Center:
                    return "Dibujo centrado en la " + copy + ".";
                case NavigationActionKind.LocateView:
                    return "Vista " + LabelOf(view) + " localizada (" + copy + ").";
                default:
                    return "Vista ajustada a la " + copy + ".";
            }
        }

        private string LabelOf(NavigationViewOrder? view)
        {
            foreach (var choice in _viewChoices)
                if (view.HasValue && choice.Order == view.Value) return choice.Label;
            return string.Empty;
        }

        private string ReasonOfBlock()
        {
            if (_port == null) return "La navegación no está disponible en este panel.";
            if (_busy) return "Hay una navegación en curso.";
            if (_block != WorkspaceActionBlock.None || !_canRead)
                return string.IsNullOrWhiteSpace(_blockText) ? "El dibujo no se puede leer ahora." : _blockText;
            if (!_session.HasValue) return "No hay un documento activo para navegar.";

            switch (_kind)
            {
                case SelectionContextKind.Selection: return "Lo seleccionado no es un rack individual; selecciona un solo rack para navegar.";
                case SelectionContextKind.NoIdentity: return "El objeto seleccionado no tiene identidad de rack.";
                case SelectionContextKind.Diagnostic: return "No se pudo interpretar el objeto seleccionado.";
                default: return "Selecciona un rack en el dibujo para navegar.";
            }
        }

        // The last defence: whatever failed while presenting is shown, and the actions are released.
        private void ContainFailure(Exception exception)
        {
            try
            {
                _busy = false;
                OnPropertyChanged(nameof(IsBusy));
                var detail = string.IsNullOrWhiteSpace(exception?.Message) ? exception?.GetType().Name : exception.Message;
                SetResult("No se pudo completar la navegación: " + detail);
                RaiseAvailability();
            }
            catch (Exception)
            {
                // Nothing else can be done here without risking the message loop of the host.
            }
        }

        private void SetBusy(bool busy)
        {
            if (_busy == busy) return;
            _busy = busy;
            OnPropertyChanged(nameof(IsBusy));
            RaiseAvailability();
        }

        private void SetResult(string text)
        {
            if (_resultText == text) return;
            _resultText = text;
            OnPropertyChanged(nameof(ResultText));
            OnPropertyChanged(nameof(HasResult));
        }

        private void RaiseAvailability()
        {
            OnPropertyChanged(nameof(IsAvailable));
            OnPropertyChanged(nameof(CanNavigate));
            OnPropertyChanged(nameof(BlockedReason));
            OnPropertyChanged(nameof(HasBlockedReason));
        }

        private void RaiseCopy()
        {
            OnPropertyChanged(nameof(CopyIndex));
            OnPropertyChanged(nameof(TargetCount));
            OnPropertyChanged(nameof(CopyText));
            OnPropertyChanged(nameof(CanNextCopy));
        }

        private static string Ordinal(int zeroBased) => (zeroBased + 1).ToString(CultureInfo.InvariantCulture);

        private static string CountText(int? count) => count.HasValue ? count.Value.ToString(CultureInfo.InvariantCulture) : "?";
    }
}
