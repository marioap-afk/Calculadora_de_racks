using System;
using RackCad.Application.Workspace;

namespace RackCad.UI.Workspace
{
    /// <summary>
    /// The field of the bounded pilot (AnnotationScale of a Push Back rack) on the ONE surface of the browser (I-64 F4-T3B1, D-T3B1-02).
    /// The browser creates it, owns it and exposes it as <c>Browser.Field</c>; it is never null. It talks only to the edit port of the
    /// pilot, which keeps the text in memory and never writes the drawing: the pending text lives in the draft, never here.
    ///
    /// The box is editable only with an existing draft that can be edited, on a pinned tab, and not for a clean draft that is possibly
    /// stale. In every other case it is read-only, and a gesture (the Edit button, a click, a key, a double click on the active tab)
    /// can open it. The only entry of the text typed by the user is the public setter of <see cref="Text"/>.
    ///
    /// It is synchronous and holds no static state. It never reaches the browser by a subscription: the browser hands it one method
    /// that it calls after every operation that changed the draft. Only a gesture reads the drawing, and every failure of a gesture
    /// is contained here as the Error state of the field; it never reaches the panel.
    /// </summary>
    public sealed class WorkspacePilotFieldViewModel : ObservableObject
    {
        /// <summary>What the box shows while the rack has no draft.</summary>
        public const string NoDraftMarker = "—";

        private readonly IWorkspacePilotEditPort _port;
        private readonly Action _changed;

        private SessionId? _session;
        private string _rack;
        private bool _isPreview;

        private PilotFieldView _view;
        private string _text = NoDraftMarker;
        private bool _editable;
        private bool _showEdit;
        private bool _showReload;
        private bool _showDiscard;
        private string _stateText = string.Empty;
        private string _messageText = string.Empty;

        // What belongs to the surface and stays until a new gesture or a change of surface.
        private string _noticeText = string.Empty;
        private string _diagnostic;
        private string _hint;
        private bool _readyNote;
        private string _failure;

        /// <param name="port">The edit port of the pilot; null leaves the field inert and not available.</param>
        /// <param name="changed">Called after an operation that changed the draft, so the browser rebuilds its strip. It is a method, not a subscription.</param>
        public WorkspacePilotFieldViewModel(IWorkspacePilotEditPort port, Action changed = null)
        {
            _port = port;
            _changed = changed;
        }

        // ------------------------------------------------------------------ what the panel shows

        /// <summary>False when the panel was built without the edit port of the pilot: the view shows nothing of the field.</summary>
        public bool IsAvailable => _port != null;

        /// <summary>
        /// The text of the box: the pending text or, without it, the committed one; the marker without a draft. Its public setter is the
        /// only entry of what the user types: it keeps the text in the draft and restores the box when the text is not accepted.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                try
                {
                    if (!_editable || !_session.HasValue || _rack == null)
                    {
                        // A box that is not editable accepts nothing: the value goes back to what the draft says.
                        OnPropertyChanged(nameof(Text));
                        return;
                    }

                    if (string.Equals(value, _text, StringComparison.Ordinal)) return;

                    ClearNotices();
                    var result = _port.SetPendingText(Origin(), _rack, value ?? string.Empty);
                    if (result.Status == PilotFieldStatus.Applied)
                    {
                        Apply(result.View);
                        _changed?.Invoke();
                        return;
                    }

                    Render();
                    _noticeText = RefusalOf(result);
                    RaiseAll();
                }
                catch (Exception ex)
                {
                    EnterFailure(ex);
                }
            }
        }

        /// <summary>True when the box accepts the typing of the user.</summary>
        public bool IsEditable => _editable;

        public bool IsReadOnly => !_editable;

        /// <summary>The Edit button: the box is read-only and a gesture can open it.</summary>
        public bool ShowEditButton => _showEdit;

        /// <summary>
        /// Whether a click or a key on the box, or the double click on the tab, may start the first gesture: only while the Edit button is
        /// offered, with no diagnostic of a read that was not admitted for this surface and with no Error. Only the Edit button reads again
        /// in the other cases, and in the Error state only Reload acts.
        /// </summary>
        public bool CanOpenByGesture => _showEdit && !_showReload && _failure == null && _diagnostic == null;

        /// <summary>The Reload button of the Error state: it shows the draft again, with no read of the drawing.</summary>
        public bool ShowReloadButton => _showReload;

        /// <summary>The Discard button of the Error state (I-64 F4-T3B2): the Reload button, with a draft shown for the surface.</summary>
        public bool ShowDiscardButton => _showDiscard;

        /// <summary>The state in Spanish, with the mark of a possibly stale value and the reason when there is no committed text.</summary>
        public string StateText => _stateText;

        /// <summary>The message of the validation of the pending text.</summary>
        public string MessageText => _messageText;

        /// <summary>The last notice of a refusal, an abort, a wait or a failure, with its reason.</summary>
        public string NoticeText => _noticeText;

        /// <summary>The diagnostic of a read that was not admitted; empty otherwise.</summary>
        public string NotEditableText => _diagnostic ?? string.Empty;

        /// <summary>The hint of the classic editor that comes with a diagnostic; it is shown, never launched.</summary>
        public string ClassicEditorHintText => _hint ?? string.Empty;

        // The skeleton of I-64 F4-T4A2 (INV-F4T4A2-14): the members of the reconciliation exist and do nothing.
        public bool ShowCheckButton => false;

        public bool ShowDiscardAndReloadButton => false;

        public bool ShowKeepMineButton => false;

        public bool HasChoice => false;

        public string ChoiceText => string.Empty;

        public string ReconcileText => string.Empty;

        public void CheckNow()
        {
        }

        public void RequestDiscardAndReload()
        {
        }

        public void RequestKeepMine()
        {
        }

        public void ConfirmChoice()
        {
        }

        public void CancelChoice()
        {
        }

        // ------------------------------------------------------------------ what the browser tells it

        /// <summary>
        /// The surface is this rack of this session. A new surface starts with no notice; the same one keeps its notice and its
        /// diagnostic. It only reads the memory of the port: it never reads the drawing and never writes anything.
        /// </summary>
        public void Show(SessionId session, string rackId, bool isPreview)
        {
            if (_port == null) return;

            if (!_session.HasValue || _session.Value != session || !string.Equals(_rack, rackId, StringComparison.Ordinal))
            {
                ResetSurfaceState();
                _session = session;
                _rack = rackId;
            }

            _isPreview = isPreview;
            Render();
        }

        /// <summary>There is no surface: the field goes back to its initial state.</summary>
        public void Clear()
        {
            if (_port == null) return;

            ResetSurfaceState();
            _session = null;
            _rack = null;
            _isPreview = false;
            _view = default;
            Apply(default);
        }

        /// <summary>A failure that the browser contained for this surface: the field shows it as its Error state.</summary>
        public void Fail(Exception exception)
        {
            if (_port == null) return;
            EnterFailure(exception);
        }

        /// <summary>A notice for this surface, with its reason.</summary>
        public void ShowNotice(string notice)
        {
            if (_port == null) return;
            _noticeText = notice ?? string.Empty;
            RaiseAll();
        }

        // ------------------------------------------------------------------ the gestures of the user

        /// <summary>
        /// The first gesture on a read-only box: asks the port for the intention without a witness (or the lazy reload of a clean
        /// draft that is possibly stale). It is the only entry that reads the drawing, once per gesture; the browser pins the tab first.
        /// </summary>
        public void BeginEdit()
        {
            if (_port == null || !_session.HasValue || _rack == null) return;

            var session = _session.Value;
            var rack = _rack;
            try
            {
                ClearNotices();
                var view = _port.GetField(new WorkspaceRequest(session), rack);
                var stale = view.Exists && view.State == PilotFieldState.Clean && view.IsPossiblyStale;
                var request = new EditIntentRequest(session, rack, null, null);
                var outcome = stale ? _port.TryReload(request) : _port.TryBeginEdit(request);

                if (!IsMine(session, rack))
                {
                    // The reading came back to another surface: its result is not shown here.
                    Render();
                    _noticeText = "La superficie cambió durante la lectura; el resultado no se mostró.";
                    RaiseAll();
                    return;
                }

                switch (outcome.Status)
                {
                    case EditIntentStatus.Admitted:
                    case EditIntentStatus.AlreadyOpen:
                    case EditIntentStatus.Reloaded:
                    case EditIntentStatus.NotNeeded:
                        _readyNote = true;
                        break;
                    case EditIntentStatus.NotAdmitted:
                        _diagnostic = outcome.Diagnostic ?? string.Empty;
                        _hint = outcome.ClassicEditorHint ?? string.Empty;
                        if (stale) _noticeText = "No se recargó: el borrador se conserva tal como estaba.";
                        break;
                    default:
                        _noticeText = Describe(outcome.Status, outcome.Reason);
                        break;
                }

                Render();
                _changed?.Invoke();
            }
            catch (Exception ex)
            {
                EnterFailure(ex);
            }
        }

        /// <summary>Enter or leaving the box: commits the pending text, only with pending text and an editable box.</summary>
        public void CommitPending()
        {
            if (_port == null || !_session.HasValue || _rack == null) return;

            try
            {
                var view = _port.GetField(Origin(), _rack);
                if (view.PendingText == null || !EditableOf(view)) return;

                ClearNotices();
                var result = _port.CommitField(Origin(), _rack);
                switch (result.Status)
                {
                    case PilotFieldStatus.Applied:
                        Apply(result.View);
                        break;
                    case PilotFieldStatus.Invalid:
                        // The text stays exactly as typed, with its message: nothing is repaired.
                        Apply(result.View);
                        _messageText = result.Message ?? result.View.PendingMessage ?? string.Empty;
                        RaiseAll();
                        break;
                    default:
                        Render();
                        _noticeText = RefusalOf(result);
                        RaiseAll();
                        break;
                }

                _changed?.Invoke();
            }
            catch (Exception ex)
            {
                EnterFailure(ex);
            }
        }

        /// <summary>The Reload button: shows the draft again from the memory of the port, without any read of the drawing.</summary>
        public void Rematerialize()
        {
            if (_port == null) return;

            try
            {
                ClearNotices();
                _failure = null;
                Render();
            }
            catch (Exception ex)
            {
                EnterFailure(ex);
            }
        }

        // ------------------------------------------------------------------ internals

        private WorkspaceRequest Origin() => new WorkspaceRequest(_session.Value);

        private bool IsMine(SessionId session, string rack) =>
            _session.HasValue && _session.Value == session && string.Equals(_rack, rack, StringComparison.Ordinal);

        private void ResetSurfaceState()
        {
            _noticeText = string.Empty;
            _diagnostic = null;
            _hint = null;
            _readyNote = false;
            _failure = null;
        }

        // A new gesture ends the notice of the previous one.
        private void ClearNotices()
        {
            _noticeText = string.Empty;
            _diagnostic = null;
            _hint = null;
            _readyNote = false;
        }

        // Reads the memory of the port for the surface and shows it. It never reads the drawing and keeps the notices.
        private void Render()
        {
            if (!_session.HasValue || _rack == null)
            {
                Apply(default);
                return;
            }

            PilotFieldView view;
            try
            {
                view = _port.GetField(Origin(), _rack);
            }
            catch (Exception ex)
            {
                EnterFailure(ex);
                return;
            }

            Apply(view);
        }

        private bool EditableOf(PilotFieldView view) =>
            _failure == null
            && view.Exists
            && view.CanEdit
            && !_isPreview
            && !(view.State == PilotFieldState.Clean && view.IsPossiblyStale);

        private void Apply(PilotFieldView view)
        {
            _view = view;
            _editable = EditableOf(view);

            var openable = view.Exists && view.CanEdit && !_editable && _failure == null;
            _showEdit = _failure == null && _session.HasValue && (!view.Exists || openable);
            _showReload = _failure != null || (view.Exists && view.State == PilotFieldState.Error);
            _showDiscard = _showReload && view.Exists;

            _text = view.Exists ? view.PendingText ?? view.CommittedText ?? string.Empty : NoDraftMarker;
            _messageText = view.Exists ? view.PendingMessage ?? string.Empty : string.Empty;
            _stateText = StateOf(view);
            RaiseAll();
        }

        private string StateOf(PilotFieldView view)
        {
            if (_failure != null) return "Error: " + _failure;
            if (!view.Exists) return _diagnostic != null ? "No editable aquí" : "Sin borrador.";
            if (view.State == PilotFieldState.Error) return "Error: " + ErrorDetailOf(view);

            string state;
            if (_readyNote && _editable && view.State == PilotFieldState.Clean && view.PendingText == null) state = "Listo para editar";
            else state = NameOf(view.State);

            if (view.IsPossiblyStale) state = state + " (posiblemente obsoleto)";
            if (view.CommittedText == null && !string.IsNullOrWhiteSpace(view.CommittedTextReason)) state = state + ": " + view.CommittedTextReason;
            return state;
        }

        // The reason of the Error of the draft when the port carries it; otherwise the reason of the committed text, and then no detail.
        private static string ErrorDetailOf(PilotFieldView view)
        {
            if (!string.IsNullOrWhiteSpace(view.ErrorReason)) return view.ErrorReason;
            return string.IsNullOrWhiteSpace(view.CommittedTextReason) ? "sin detalle." : view.CommittedTextReason;
        }

        private static string NameOf(PilotFieldState state)
        {
            switch (state)
            {
                case PilotFieldState.Clean: return "Limpio";
                case PilotFieldState.Dirty: return "Modificado";
                case PilotFieldState.Conflict: return "Conflicto";
                case PilotFieldState.Applying: return "Aplicando";
                case PilotFieldState.Unknown: return "Desconocido";
                case PilotFieldState.Error: return "Error";
                case PilotFieldState.Undecidable: return "Sin decidir";
                default: return "Sin borrador.";
            }
        }

        // The failure of a gesture or of a read: the field shows it as its own Error state, read-only, with the reload button.
        private void EnterFailure(Exception exception)
        {
            try
            {
                _failure = string.IsNullOrWhiteSpace(exception?.Message) ? exception?.GetType().Name : exception.Message;
                Apply(_view);
            }
            catch (Exception)
            {
                // Nothing else can be done here without risking the message loop of the host.
            }
        }

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(Text));
            OnPropertyChanged(nameof(IsEditable));
            OnPropertyChanged(nameof(IsReadOnly));
            OnPropertyChanged(nameof(ShowEditButton));
            OnPropertyChanged(nameof(ShowReloadButton));
            OnPropertyChanged(nameof(ShowDiscardButton));
            OnPropertyChanged(nameof(StateText));
            OnPropertyChanged(nameof(MessageText));
            OnPropertyChanged(nameof(NoticeText));
            OnPropertyChanged(nameof(NotEditableText));
            OnPropertyChanged(nameof(ClassicEditorHintText));
        }

        private static string RefusalOf(PilotFieldResult result)
        {
            var noReason = string.IsNullOrWhiteSpace(result.Reason);
            var reason = noReason ? "sin detalle." : result.Reason;
            switch (result.Status)
            {
                // The reason of the port already says it in Spanish: it is shown alone, and the generic text only stands in for a missing one.
                case PilotFieldStatus.Busy: return "El texto no se aplicó: " + (noReason ? "hay una lectura en curso." : result.Reason);
                case PilotFieldStatus.NotEditable: return "El texto no se aplicó: " + (noReason ? "el campo no se puede editar ahora." : result.Reason);
                case PilotFieldStatus.Invalid: return "El texto no es válido: " + (result.Message ?? reason);
                default: return "El texto no se aplicó: " + reason;
            }
        }

        private static string Describe(EditIntentStatus status, string reason)
        {
            var detail = string.IsNullOrWhiteSpace(reason) ? "sin detalle." : reason;
            switch (status)
            {
                case EditIntentStatus.Waiting: return "En espera: " + detail;
                case EditIntentStatus.Unavailable: return "No disponible: " + detail;
                case EditIntentStatus.Rejected: return "Rechazado: " + detail;
                case EditIntentStatus.NotFound: return "Sin borrador: " + detail;
                case EditIntentStatus.Failed: return "Fallo: " + detail;
                default: return "No se pudo abrir la edición: " + detail;
            }
        }
    }
}
