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
    ///
    /// Since I-64 F4-T4A2 it also holds the visible, read-only part of the explicit reconciliation (D-T4A2-01 to D-T4A2-06): the check
    /// button, the two options and ONE inline choice with Yes and No that belongs to the field and to its surface. The presentation of
    /// what the check found never says the draft is clean unless the one answer of the host that says so came with the draft reread
    /// as clean or dirty. Nothing here writes the drawing.
    /// </summary>
    public sealed class WorkspacePilotFieldViewModel : ObservableObject
    {
        /// <summary>What the box shows while the rack has no draft.</summary>
        public const string NoDraftMarker = "—";

        // The one choice that waits for its Yes or its No. It is private to the field: it is not a value of the port.
        private enum ChoiceKind
        {
            None,
            DiscardAndReload,
            KeepMine,
        }

        // The texts of the check (table T-A2 of the package). The text of A2-01 and the one of A2-07 live ONLY in their constant, which is
        // referenced once, in the case of the answer that says it; no other place of the surface repeats them.
        private const string UnchangedText = "Sin cambios: el dibujo coincide con la base del borrador.";
        private const string StillInConflictText = "La base vuelve a coincidir con el dibujo, pero el borrador sigue en conflicto: elige cómo reconciliarlo.";
        private const string NotEditableConfirmedText = "La base coincide con el dibujo, pero el rack ya no es editable aquí.";
        private const string ExternalChangeText = "El dibujo cambió fuera del panel y el borrador no tiene cambios propios: «Descartar y recargar» lo pone al día.";
        private const string ConflictOwnWorkText = "El dibujo cambió fuera del panel y el borrador tiene cambios propios: elige «Conservar mi borrador» o «Descartar y recargar».";
        private const string ConflictPendingTextText = "El dibujo cambió fuera del panel y el borrador tiene texto pendiente: aplícalo o corrígelo para poder conservarlo, o elige «Descartar y recargar».";
        private const string NotEditableConflictText = "El dibujo cambió y el rack ya no es editable aquí: no se puede recargar ni conservar el borrador.";
        private const string RecoveredText = "El cambio no llegó a aplicarse: el borrador recupera el estado que tenía antes.";
        private const string NotCheckedSuffix = " No se comprobó.";
        private const string UnknownConflictText = "El dibujo no coincide con la base anterior al intento: el borrador pasa a conflicto y no se supone aplicado.";
        private const string ChangedDuringCheckText = "Se comprobó, pero el borrador cambió durante la comprobación; vuelve a comprobarlo.";
        private const string DraftGoneText = "Se comprobó, pero el borrador ya no existe.";

        // The texts of the reconciliation line.
        private const string WhatChangedPrefix = "Qué cambió: ";
        private const string CausePrefix = " Causa: ";
        private const string NoDetailText = "sin detalle.";
        private const string UnknownCheckText = "Estado desconocido; comprueba.";
        private const string UnknownAppliedText = "Aplicado; recarga para seguir editando.";
        private const string PendingBlocksKeepText = "Con texto pendiente no se puede conservar el borrador: aplícalo o corrígelo antes.";

        // The texts of the choice and of what follows it.
        private const string DiscardWithWorkText = "¿Descartar los cambios sin aplicar del borrador y recargarlo desde el dibujo?";
        private const string DiscardPendingSuffix = " También se pierde el texto pendiente.";
        private const string ReloadWithoutWorkText = "¿Recargar el borrador desde el dibujo? No tiene cambios propios que perder.";
        private const string KeepPrefix = "¿Conservar tu escala de anotación (";
        private const string KeepSuffix = ") sobre la versión actual del dibujo? El resto del rack se toma del dibujo y tu escala sustituye a la del dibujo.";
        private const string NoValueText = "sin valor";
        private const string ReloadAppliedText = "¿Recargar el borrador desde el dibujo? El cambio ya se aplicó y el borrador vuelve a leerse del dibujo.";
        private const string ReloadedText = "Borrador recargado desde el dibujo.";
        private const string KeptText = "Borrador conservado sobre la versión actual del dibujo.";
        private const string NotAppliedChoiceText = "No se aplicó la elección: el borrador se conserva tal como estaba.";
        private const string NothingDoneSuffix = " No se hizo nada.";
        private const string SituationChangedText = "La situación del borrador cambió; no se hizo nada.";
        private const string ChoiceCancelledText = "Elección cancelada: el borrador no cambió.";

        private const string SurfaceChangedText = "La superficie cambió durante la lectura; el resultado no se mostró.";

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

        // What belongs to the surface and a new gesture does NOT end (D-T4A2-06): the diagnostic of a read that was not admitted, with the hint of
        // the classic editor. Only a result that reread the drawing replaces it; a change of surface, the end of the draft, a reload or a keep remove it.
        private string _persistDiagnostic;
        private string _persistHint;

        // The one choice that waits (D-T4A2-03): its kind, the surface it was asked on and the view that justified it.
        private ChoiceKind _choice;
        private SessionId? _choiceSession;
        private string _choiceRack;
        private PilotFieldState _choiceState;
        private bool _choiceProtected;
        private string _choicePending;
        private string _choiceCommitted;
        private PilotUnknownOrigin _choiceOrigin;
        private string _choiceText = string.Empty;

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

        /// <summary>
        /// The check button (I-64 F4-T4A2, D-T4A2-01): on a pinned tab with a draft, without a failure and without a choice that waits, for a draft
        /// that is clean, dirty or in conflict, and for an unknown one after a commit that threw.
        /// </summary>
        public bool ShowCheckButton => OffersCheck(_view);

        /// <summary>
        /// The option to discard the work and reload the draft (D-T4A2-02): for a draft in conflict, and for an unknown one that was applied and
        /// whose reread failed; on a pinned tab, without a failure, without a persistent diagnostic and without a choice that waits.
        /// </summary>
        public bool ShowDiscardAndReloadButton => !HasChoice && OffersChoice(_view, ChoiceKind.DiscardAndReload);

        /// <summary>
        /// The option to keep the draft on the current version (D-T4A2-02): ONLY for a draft in conflict that has a committed change and no
        /// pending text, with the same conditions of the surface as the other option.
        /// </summary>
        public bool ShowKeepMineButton => !HasChoice && OffersChoice(_view, ChoiceKind.KeepMine);

        /// <summary>True while the inline choice waits for its Yes or its No: the box is read-only and only those two buttons act.</summary>
        public bool HasChoice => _choice != ChoiceKind.None;

        /// <summary>The question of the choice that waits; empty without one.</summary>
        public string ChoiceText => HasChoice ? _choiceText : string.Empty;

        /// <summary>What the reconciliation of the draft needs the user to know: what changed and why, or that the outcome is unknown.</summary>
        public string ReconcileText => ReconcileTextOf(_view);

        /// <summary>The state in Spanish, with the mark of a possibly stale value and the reason when there is no committed text.</summary>
        public string StateText => _stateText;

        /// <summary>The message of the validation of the pending text.</summary>
        public string MessageText => _messageText;

        /// <summary>The last notice of a refusal, an abort, a wait or a failure, with its reason.</summary>
        public string NoticeText => _noticeText;

        /// <summary>The diagnostic of a read that was not admitted: the one of the first gesture and, without it, the persistent one; empty otherwise.</summary>
        public string NotEditableText => _diagnostic ?? _persistDiagnostic ?? string.Empty;

        /// <summary>The hint of the classic editor that comes with a diagnostic; it is shown, never launched.</summary>
        public string ClassicEditorHintText => _hint ?? _persistHint ?? string.Empty;

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
                    _noticeText = SurfaceChangedText;
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

        /// <summary>
        /// The check button (D-T4A2-01): one explicit reread of the drawing, read-only. A clean, dirty or conflicting draft goes to the general
        /// check; an unknown one after a commit that threw goes to its own. The pending text is never touched, and the result is shown by the
        /// classes of the package without ever saying more than the host said.
        /// </summary>
        public void CheckNow()
        {
            if (!ShowCheckButton) return;

            var session = _session.Value;
            var rack = _rack;
            try
            {
                ClearNotices();
                var view = _port.GetField(new WorkspaceRequest(session), rack);
                var request = new EditIntentRequest(session, rack, null, null);

                if (view.Exists && view.State == PilotFieldState.Unknown && view.UnknownOrigin == PilotUnknownOrigin.CommitThrew)
                {
                    var outcome = _port.TryCheckNow(request);
                    if (!IsMine(session, rack))
                    {
                        PresentSurfaceChange();
                        return;
                    }

                    PresentCheck(outcome);
                }
                else if (view.Exists && (view.State == PilotFieldState.Clean || view.State == PilotFieldState.Dirty || view.State == PilotFieldState.Conflict))
                {
                    var outcome = _port.TryConfirmBase(request);
                    if (!IsMine(session, rack))
                    {
                        PresentSurfaceChange();
                        return;
                    }

                    PresentConfirm(outcome);
                }
                else
                {
                    // The draft changed after the button was shown: nothing is asked of the drawing.
                    Render();
                    _noticeText = SituationChangedText;
                    RaiseAll();
                }
            }
            catch (Exception ex)
            {
                EnterFailure(ex);
            }
        }

        /// <summary>The option to discard the work and reload the draft: it asks for the confirmation and calls nothing of the port but the memory.</summary>
        public void RequestDiscardAndReload() => AskChoice(ChoiceKind.DiscardAndReload);

        /// <summary>The option to keep the draft on the current version: it asks for the confirmation and calls nothing of the port but the memory.</summary>
        public void RequestKeepMine() => AskChoice(ChoiceKind.KeepMine);

        /// <summary>
        /// Yes: takes the choice that waits, checks that the draft is still what justified it and then, and only then, calls the one operation
        /// that was chosen. The other operation is never called.
        /// </summary>
        public void ConfirmChoice()
        {
            if (_port == null || _choice == ChoiceKind.None) return;

            var kind = _choice;
            var session = _choiceSession.Value;
            var rack = _choiceRack;
            var state = _choiceState;
            var isProtected = _choiceProtected;
            var pending = _choicePending;
            var committed = _choiceCommitted;
            var origin = _choiceOrigin;
            try
            {
                ClearNotices();

                var sameSurface = IsMine(session, rack);
                var view = sameSurface ? _port.GetField(new WorkspaceRequest(session), rack) : default;
                if (!sameSurface
                    || !view.Exists
                    || view.State != state
                    || view.IsProtected != isProtected
                    || view.UnknownOrigin != origin
                    || !string.Equals(view.PendingText, pending, StringComparison.Ordinal)
                    || !string.Equals(view.CommittedText, committed, StringComparison.Ordinal))
                {
                    // The situation is not the one that was asked about: nothing is called.
                    Render();
                    _noticeText = SituationChangedText;
                    RaiseAll();
                    return;
                }

                var request = new EditIntentRequest(session, rack, null, null);
                var outcome = kind == ChoiceKind.KeepMine ? _port.TryKeepMine(request) : _port.TryDiscardAndReload(request);
                if (!IsMine(session, rack))
                {
                    PresentSurfaceChange();
                    return;
                }

                PresentChoice(kind, outcome);
            }
            catch (Exception ex)
            {
                EnterFailure(ex);
            }
        }

        /// <summary>No: takes the choice away and changes nothing. It asks nothing of the port, not even its memory.</summary>
        public void CancelChoice()
        {
            if (_choice == ChoiceKind.None) return;

            ClearNotices();
            _noticeText = ChoiceCancelledText;
            Apply(_view);
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
            _persistDiagnostic = null;
            _persistHint = null;
            _readyNote = false;
            _failure = null;
            RemoveChoice();
        }

        // A new gesture ends the notice of the previous one and the choice that waited. It never touches the persistent diagnostic.
        private void ClearNotices()
        {
            _noticeText = string.Empty;
            _diagnostic = null;
            _hint = null;
            _readyNote = false;
            RemoveChoice();
        }

        private void RemoveChoice()
        {
            _choice = ChoiceKind.None;
            _choiceSession = null;
            _choiceRack = null;
            _choiceState = PilotFieldState.NotOpen;
            _choiceProtected = false;
            _choicePending = null;
            _choiceCommitted = null;
            _choiceOrigin = PilotUnknownOrigin.None;
            _choiceText = string.Empty;
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
            && _persistDiagnostic == null
            && !HasChoice
            && view.Exists
            && view.CanEdit
            && !_isPreview
            && !(view.State == PilotFieldState.Clean && view.IsPossiblyStale);

        // The surface of the options: a pinned tab with a draft and no failure of the field.
        private bool OnPinnedSurface(PilotFieldView view) =>
            _port != null && _session.HasValue && _rack != null && _failure == null && !_isPreview && view.Exists;

        private bool OffersCheck(PilotFieldView view)
        {
            if (!OnPinnedSurface(view) || HasChoice) return false;

            switch (view.State)
            {
                case PilotFieldState.Clean:
                case PilotFieldState.Dirty:
                case PilotFieldState.Conflict:
                    return true;
                case PilotFieldState.Unknown:
                    return view.UnknownOrigin == PilotUnknownOrigin.CommitThrew;
                default:
                    return false;
            }
        }

        // The offer of an option WITHOUT the clause of a choice that waits, so a choice never withdraws itself.
        private bool OffersChoice(PilotFieldView view, ChoiceKind kind)
        {
            if (!OnPinnedSurface(view) || _persistDiagnostic != null) return false;

            switch (kind)
            {
                case ChoiceKind.DiscardAndReload:
                    return view.State == PilotFieldState.Conflict
                           || (view.State == PilotFieldState.Unknown && view.UnknownOrigin == PilotUnknownOrigin.PostReread);
                case ChoiceKind.KeepMine:
                    return view.State == PilotFieldState.Conflict && view.IsProtected && view.PendingText == null;
                default:
                    return false;
            }
        }

        // The option asks for its confirmation: it reads only the memory of the port and calls no operation of the drawing.
        private void AskChoice(ChoiceKind kind)
        {
            if (kind == ChoiceKind.DiscardAndReload ? !ShowDiscardAndReloadButton : !ShowKeepMineButton) return;

            var session = _session.Value;
            var rack = _rack;
            try
            {
                var view = _port.GetField(new WorkspaceRequest(session), rack);
                if (!OffersChoice(view, kind)) return;

                ClearNotices();
                _choice = kind;
                _choiceSession = session;
                _choiceRack = rack;
                _choiceState = view.State;
                _choiceProtected = view.IsProtected;
                _choicePending = view.PendingText;
                _choiceCommitted = view.CommittedText;
                _choiceOrigin = view.UnknownOrigin;
                _choiceText = ChoiceTextOf(kind, view);
                Apply(view);
            }
            catch (Exception ex)
            {
                EnterFailure(ex);
            }
        }

        private static string ChoiceTextOf(ChoiceKind kind, PilotFieldView view)
        {
            if (kind == ChoiceKind.KeepMine) return KeepPrefix + (view.CommittedText ?? NoValueText) + KeepSuffix;
            if (view.State == PilotFieldState.Unknown) return ReloadAppliedText;
            if (view.IsProtected || view.PendingText != null) return DiscardWithWorkText + (view.PendingText != null ? DiscardPendingSuffix : string.Empty);
            return ReloadWithoutWorkText;
        }

        private void PresentSurfaceChange()
        {
            // The reading came back to another surface: its result is not shown here.
            Render();
            _noticeText = SurfaceChangedText;
            RaiseAll();
        }

        // The answer of the general check. Only an answer that reread the drawing (Confirmed or Conflict) replaces the persistent diagnostic.
        private void PresentConfirm(EditConfirmOutcome outcome)
        {
            if (outcome.Status == EditConfirmStatus.Confirmed || outcome.Status == EditConfirmStatus.Conflict)
            {
                _persistDiagnostic = outcome.AdmissionDiagnostic;
                _persistHint = outcome.AdmissionDiagnostic == null ? null : outcome.ClassicEditorHint ?? string.Empty;
            }

            Render();
            _noticeText = ConfirmNoticeOf(outcome);
            RaiseAll();
            _changed?.Invoke();
        }

        // The view is the one reread from memory after the operation: the word of the host and the state of the draft decide the text.
        private string ConfirmNoticeOf(EditConfirmOutcome outcome)
        {
            switch (outcome.Status)
            {
                case EditConfirmStatus.Confirmed:
                    if (outcome.AdmissionDiagnostic != null) return NotEditableConfirmedText;
                    if (!_view.Exists) return DraftGoneText;
                    if (_view.State == PilotFieldState.Conflict) return StillInConflictText;
                    if (_view.State == PilotFieldState.Clean || _view.State == PilotFieldState.Dirty) return UnchangedText;
                    return ChangedDuringCheckText;
                case EditConfirmStatus.Conflict:
                    if (outcome.AdmissionDiagnostic != null) return NotEditableConflictText;
                    if (_view.PendingText != null) return ConflictPendingTextText;
                    if (_view.IsProtected) return ConflictOwnWorkText;
                    return ExternalChangeText;
                default:
                    return CheckPrefixOf(outcome.Status) + DetailOf(outcome.Reason) + NotCheckedSuffix;
            }
        }

        // The answer of the check of an unknown draft after a commit that threw: a recovery or a conflict reread the drawing and retire the diagnostic.
        private void PresentCheck(EditCheckOutcome outcome)
        {
            if (outcome.Status == EditCheckStatus.Recovered || outcome.Status == EditCheckStatus.Conflict)
            {
                _persistDiagnostic = null;
                _persistHint = null;
            }

            Render();
            _noticeText = CheckNoticeOf(outcome);
            RaiseAll();
            _changed?.Invoke();
        }

        private static string CheckNoticeOf(EditCheckOutcome outcome)
        {
            switch (outcome.Status)
            {
                case EditCheckStatus.Recovered:
                    return RecoveredText;
                case EditCheckStatus.Conflict:
                    return UnknownConflictText;
                default:
                    return CheckPrefixOf(outcome.Status) + DetailOf(outcome.Reason) + NotCheckedSuffix;
            }
        }

        // The answer of the operation that was chosen. A reload or a keep that does not match the choice is never presented as done.
        private void PresentChoice(ChoiceKind kind, EditIntentOutcome outcome)
        {
            switch (outcome.Status)
            {
                case EditIntentStatus.Reloaded when kind == ChoiceKind.DiscardAndReload:
                    _persistDiagnostic = null;
                    _persistHint = null;
                    _noticeText = ReloadedText;
                    break;
                case EditIntentStatus.Kept when kind == ChoiceKind.KeepMine:
                    _persistDiagnostic = null;
                    _persistHint = null;
                    _noticeText = KeptText;
                    break;
                case EditIntentStatus.NotAdmitted:
                    _persistDiagnostic = outcome.Diagnostic ?? string.Empty;
                    _persistHint = outcome.ClassicEditorHint ?? string.Empty;
                    _noticeText = NotAppliedChoiceText;
                    break;
                default:
                    _noticeText = ChoicePrefixOf(outcome.Status) + DetailOf(outcome.Reason) + NothingDoneSuffix;
                    break;
            }

            Render();
            RaiseAll();
            _changed?.Invoke();
        }

        private string ReconcileTextOf(PilotFieldView view)
        {
            if (_failure != null || !view.Exists) return string.Empty;

            switch (view.State)
            {
                case PilotFieldState.Conflict:
                    var text = WhatChangedPrefix + (string.IsNullOrWhiteSpace(view.ConflictText) ? NoDetailText : view.ConflictText);
                    if (!string.IsNullOrWhiteSpace(view.LastCause)) text = text + CausePrefix + view.LastCause;
                    if (view.PendingText != null && _persistDiagnostic == null && !HasChoice) text = text + " " + PendingBlocksKeepText;
                    return text;
                case PilotFieldState.Unknown:
                    if (view.UnknownOrigin == PilotUnknownOrigin.CommitThrew) return UnknownCheckText;
                    if (view.UnknownOrigin == PilotUnknownOrigin.PostReread) return UnknownAppliedText;
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        private void Apply(PilotFieldView view)
        {
            _view = view;

            // Without a draft there is nothing the diagnostic could be about.
            if (!view.Exists)
            {
                _persistDiagnostic = null;
                _persistHint = null;
            }

            // A choice that the view no longer offers (for example after a discard from elsewhere) is withdrawn with a notice.
            if (HasChoice && !OffersChoice(view, _choice))
            {
                RemoveChoice();
                _noticeText = SituationChangedText;
            }

            _editable = EditableOf(view);

            var openable = view.Exists && view.CanEdit && !_editable && _failure == null && _persistDiagnostic == null && !HasChoice;
            _showEdit = _failure == null && _session.HasValue && _persistDiagnostic == null && !HasChoice && (!view.Exists || openable);
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
            return string.IsNullOrWhiteSpace(view.CommittedTextReason) ? NoDetailText : view.CommittedTextReason;
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
                RemoveChoice();
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
            OnPropertyChanged(nameof(ShowCheckButton));
            OnPropertyChanged(nameof(ShowDiscardAndReloadButton));
            OnPropertyChanged(nameof(ShowKeepMineButton));
            OnPropertyChanged(nameof(HasChoice));
            OnPropertyChanged(nameof(ChoiceText));
            OnPropertyChanged(nameof(ReconcileText));
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

        private static string DetailOf(string reason) => string.IsNullOrWhiteSpace(reason) ? NoDetailText : reason;

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

        // The prefix of a check that did not reread the drawing: the ones of Describe, the one of a draft that does not apply, and a default.
        private static string CheckPrefixOf(EditConfirmStatus status)
        {
            switch (status)
            {
                case EditConfirmStatus.Unavailable: return "No disponible: ";
                case EditConfirmStatus.Waiting: return "En espera: ";
                case EditConfirmStatus.Rejected: return "Rechazado: ";
                case EditConfirmStatus.NotFound: return "Sin borrador: ";
                case EditConfirmStatus.NotApplicable: return "No aplicable: ";
                case EditConfirmStatus.Failed: return "Fallo: ";
                default: return "No se pudo comprobar: ";
            }
        }

        private static string CheckPrefixOf(EditCheckStatus status)
        {
            switch (status)
            {
                case EditCheckStatus.Unavailable: return "No disponible: ";
                case EditCheckStatus.Waiting: return "En espera: ";
                case EditCheckStatus.Rejected: return "Rechazado: ";
                case EditCheckStatus.NotFound: return "Sin borrador: ";
                case EditCheckStatus.NotApplicable: return "No aplicable: ";
                case EditCheckStatus.Failed: return "Fallo: ";
                default: return "No se pudo comprobar: ";
            }
        }

        // The prefix of the answer to a choice: the ones of Describe, the one of a draft that is not needed, and a default.
        private static string ChoicePrefixOf(EditIntentStatus status)
        {
            switch (status)
            {
                case EditIntentStatus.Unavailable: return "No disponible: ";
                case EditIntentStatus.Waiting: return "En espera: ";
                case EditIntentStatus.Rejected: return "Rechazado: ";
                case EditIntentStatus.NotFound: return "Sin borrador: ";
                case EditIntentStatus.Failed: return "Fallo: ";
                case EditIntentStatus.NotNeeded: return "No aplicable: ";
                default: return "No se aplicó la elección: ";
            }
        }
    }
}
