using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Workspace;

namespace RackCad.UI.Workspace
{
    /// <summary>
    /// One tab of the strip as the panel shows it: a plain value (I-64 F3-T2, P-07, INV-02). It carries no event, no delegate, no
    /// object of AutoCAD and no reference to a visual element, so a tab adds no subscription of any kind.
    /// </summary>
    public sealed record WorkspaceBrowserTabItem(string RackId, string Title, bool IsActive, bool IsPreview);

    /// <summary>
    /// One row of notice of the browser (I-64 F4-T3B2): a plain value with a stable key, the text and whether the row offers the discard of
    /// the drafts of the document. It carries no event, no delegate and no reference to a visual element.
    /// </summary>
    public sealed record WorkspaceBrowserNoticeItem(string Key, string Text, bool CanDiscardDocument);

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
    /// With the edit port of the pilot (I-64 F4-T3B1) the one surface also holds the field of the pilot (<see cref="Field"/>): the browser
    /// pins the tab and passes the gestures of the user to it, shows the mark of a protected tab, crosses the boundaries of the panel and
    /// shows the field again at the end of every rebuild. Without that port the browser is exactly as before.
    ///
    /// With the same port (I-64 F4-T3B2) it also closes the tabs with the rules of D-08 (a tab with no draft closes, a clean draft is
    /// discarded and closes, a protected one asks first, inline, to Discard or Cancel), discards the field in Error, discards the drafts of
    /// the document with a second look at the risks, shows the notices of vetoed close and of lost work that the presentation consumed, and
    /// keeps the active tab that has pending text when the selection changes. None of that writes the drawing, reads the intention of
    /// the pilot or reaches the navigation, and every failure of the port is contained here as a notice.
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

        // The one inline confirmation that may be pending: which gesture asked for it.
        private enum ConfirmationKind
        {
            None,
            CloseTab,
            DiscardField,
            DiscardDocument,
        }

        // A notice taken from the queues of the port and kept until the user dismisses it or, for a veto, until its session or its racks go.
        private sealed class StoredNotice
        {
            public StoredNotice(string key, bool isLoss, PilotCloseView close, PilotLossView loss)
            {
                Key = key;
                IsLoss = isLoss;
                Close = close;
                Loss = loss;
            }

            public string Key { get; }

            public bool IsLoss { get; }

            public PilotCloseView Close { get; set; }

            public PilotLossView Loss { get; }
        }

        private const string NoRiskText = "No hay borradores en riesgo en este documento; no se descartó nada.";

        private readonly IWorkspaceBrowserSummaryPort _port;
        private readonly Action _tabChanged;
        private readonly IWorkspacePilotEditPort _pilotEdit;
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

        // Closing, discarding and notices (I-64 F4-T3B2). All of it is memory of the browser; the drafts stay in the port.
        private string _noticeText = string.Empty;
        private string _takeFailure;
        private ConfirmationKind _confirmKind = ConfirmationKind.None;
        private SessionId _confirmSession;
        private string _confirmRack;
        private IReadOnlyList<string> _confirmRacks = Array.Empty<string>();
        private IReadOnlyList<string> _confirmUncertain = Array.Empty<string>();
        private readonly List<StoredNotice> _stored = new();
        private IReadOnlyList<WorkspaceBrowserNoticeItem> _notices = Array.Empty<WorkspaceBrowserNoticeItem>();
        private long _lossSerial;
        private readonly Dictionary<SessionId, SelectionContext> _lastContexts = new();

        /// <param name="port">The read-only summary port; null leaves the browser inert.</param>
        /// <param name="tabChanged">Called after a manual change of tabs, so the panel presents the navigation again with its last snapshot.</param>
        /// <param name="pilotEdit">The edit port of the pilot (I-64 F4-T3B1). Optional: without it the browser has no field and no protected mark.</param>
        public WorkspaceBrowserViewModel(IWorkspaceBrowserSummaryPort port, Action tabChanged = null, IWorkspacePilotEditPort pilotEdit = null)
        {
            _port = port;
            _tabChanged = tabChanged;
            _pilotEdit = pilotEdit;
            Field = new WorkspacePilotFieldViewModel(pilotEdit, OnFieldChanged);
        }

        // ------------------------------------------------------------------ what the panel shows

        /// <summary>The field of the pilot on the one surface (I-64 F4-T3B1). Never null; not available without the edit port.</summary>
        public WorkspacePilotFieldViewModel Field { get; }

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

        /// <summary>The discard of the drafts of the document is offered: with the edit port and a presented session (I-64 F4-T3B2).</summary>
        public bool CanDiscardDocument => _pilotEdit != null && _current != null;

        /// <summary>One inline confirmation is pending: to close a tab, to discard the field or to discard the drafts of the document.</summary>
        public bool HasConfirmation => _confirmKind != ConfirmationKind.None;

        /// <summary>What the confirmation asks, with the racks by their title; empty without a confirmation.</summary>
        public string ConfirmationText => HasConfirmation ? ConfirmationTextOf() : string.Empty;

        /// <summary>The notice of the last gesture or presentation of the browser; empty without one.</summary>
        public string NoticeText => _noticeText;

        /// <summary>The rows of notice (vetoed close, lost work), recalculated from the notices kept; the same instance while nothing changes.</summary>
        public IReadOnlyList<WorkspaceBrowserNoticeItem> Notices => _notices;

        public bool HasNotices => _notices.Count > 0;

        // ------------------------------------------------------------------ what the panel tells it

        /// <summary>
        /// The panel presented a snapshot. It synchronizes the browser session of that snapshot with its selection context (the only
        /// automatic synchronization), destroys the sessions that no longer live and, when the active tab is not yet the live
        /// surface, asks the port for its summary. It never touches the navigation. With the edit port it also takes the notices of
        /// vetoed close and of lost work that the port queued (memory only), and a selection does not take the active tab away from
        /// pending text. The constructor of the panel presents too, so it consumes the queues as well.
        /// </summary>
        public void Present(SessionId? session, SelectionContext context, bool canReadDrawing, WorkspaceActionBlock block, string blockText)
        {
            if (_port == null) return;

            var priorSession = _surfaceSession;
            var priorRack = _surfaceRack;
            var priorPresented = _session;
            _canRead = canReadDrawing;
            _block = block;
            _blockText = blockText ?? string.Empty;
            if (_pilotEdit != null) TakeNotices();
            PurgeDeadSessions();

            _session = session;
            _current = session.HasValue ? _registry.GetOrCreate(session.Value) : null;
            var retention = ApplySelection(context);

            Rebuild(retryPending: true, userRequested: false, sessionChanged: priorPresented != _session);
            PostPresentationNotice(retention);
            CrossObservedBoundaries(priorSession, priorRack);
        }

        // ------------------------------------------------------------------ the explicit user interactions

        /// <summary>
        /// Makes the tab the active one. Memory only; the tab becomes the live surface and the navigation is presented again. With
        /// pending text in the live tab the change crosses the blocking boundary first: if it does not proceed, nothing changes.
        /// </summary>
        public void ActivateTab(string rackId)
        {
            if (!AllowTabChange(rackId)) return;
            Interact(session => session.Activate(rackId));
        }

        /// <summary>Pins the preview tab so the automatic synchronization never replaces it. Memory only.</summary>
        public void PinTab(string rackId) => Interact(session => session.Pin(rackId));

        /// <summary>
        /// Closes the tab. Without the edit port it closes without asking anything, as it always did. With it (I-64 F4-T3B2, D-08) a tab
        /// with no draft closes; a tab with an unprotected draft discards it and closes whatever the port answers (a refusal is
        /// said in a notice); a protected tab does not close: it asks, inline, to Discard or Cancel. If the tab was the active one the
        /// next is. Memory only: it never writes the drawing, never updates and never reads the intention of the pilot.
        /// </summary>
        public void CloseTab(string rackId)
        {
            if (_pilotEdit == null)
            {
                Interact(session => session.Close(rackId));
                return;
            }

            if (_port == null || _current == null) return;

            BeginGesture();
            try
            {
                if (!HasTab(rackId)) return;

                var session = _current.Id;
                var view = ReadField(session, rackId);
                if (view.HasValue && !view.Value.Exists)
                {
                    CloseNow(rackId, null);
                    return;
                }

                if (!view.HasValue || view.Value.IsProtected)
                {
                    Ask(ConfirmationKind.CloseTab, session, rackId);
                    return;
                }

                // A clean draft: discard it and close in any case; the draft that could not be discarded stays in memory and is said.
                string notice = null;
                try
                {
                    var outcome = _pilotEdit.DiscardRack(new WorkspaceRequest(session), rackId);
                    if (outcome.Status == EditDiscardStatus.Discarded) ForgetRisk(session, rackId);
                    else notice = CleanDraftKeptText(WhyOf(outcome));
                }
                catch (Exception ex)
                {
                    notice = CleanDraftKeptText(ReasonOf(ex));
                }

                CloseNow(rackId, notice);
            }
            catch (Exception ex)
            {
                FailGesture(ex, "No se pudo cerrar la pestaña: ");
            }
        }

        /// <summary>
        /// The Discard button of the field in Error (I-64 F4-T3B2, D-08): the rule of closing a tab without closing it. A protected draft
        /// (the Error of the host is one) asks inline first; an unprotected one is discarded at once. With Discarded the tab stays, the
        /// field leaves the Error and is shown again from memory, and the mark is recalculated.
        /// </summary>
        public void DiscardField()
        {
            if (_pilotEdit == null || _current == null) return;

            BeginGesture();
            try
            {
                if (!_surfaceSession.HasValue || _surfaceRack == null) return;

                var session = _surfaceSession.Value;
                var rack = _surfaceRack;
                var view = ReadField(session, rack);
                if (view.HasValue && !view.Value.Exists) return;

                if (!view.HasValue || view.Value.IsProtected)
                {
                    Ask(ConfirmationKind.DiscardField, session, rack);
                    return;
                }

                EditDiscardOutcome outcome;
                try
                {
                    outcome = _pilotEdit.DiscardRack(new WorkspaceRequest(session), rack);
                }
                catch (Exception ex)
                {
                    FieldDiscardFailed(session, rack, ex);
                    return;
                }

                FieldDiscarded(session, rack, outcome);
            }
            catch (Exception ex)
            {
                FailGesture(ex, "No se pudo descartar el borrador: ");
            }
        }

        /// <summary>
        /// The Discard button of the drafts of the document (I-64 F4-T3B2, R-B2-2): reads the racks at risk of the presented session.
        /// With none the notice says nothing was discarded and nothing happens; otherwise it asks, inline, listing the racks by their title
        /// and, apart, the ones that could not be decided, and it keeps both lists for the second look of <see cref="ConfirmDiscard"/>.
        /// </summary>
        public void RequestDocumentDiscard()
        {
            if (_pilotEdit == null || _current == null) return;

            BeginGesture();
            try
            {
                var session = _current.Id;
                PilotRiskView risks;
                try
                {
                    risks = _pilotEdit.GetRacksAtRisk(new WorkspaceRequest(session));
                }
                catch (Exception ex)
                {
                    FailGesture(ex, "No se pudieron leer los borradores en riesgo: ");
                    return;
                }

                if (risks.RackIds.Count == 0 && risks.UncertainRackIds.Count == 0)
                {
                    if (_confirmKind == ConfirmationKind.DiscardDocument) RemoveConfirmation();
                    PostNotice(NoRiskText);
                    return;
                }

                Ask(ConfirmationKind.DiscardDocument, session, null);
                _confirmRacks = risks.RackIds;
                _confirmUncertain = risks.UncertainRackIds;
                RaiseConfirmation();
            }
            catch (Exception ex)
            {
                FailGesture(ex, "No se pudieron leer los borradores en riesgo: ");
            }
        }

        /// <summary>
        /// The Discard button of the inline confirmation. For a tab it discards the draft of that session and rack and, with Discarded,
        /// closes the tab; with another answer the tab stays and a notice says why. For the field it discards and the tab stays. For the
        /// document it reads the racks at risk again and compares both lists in order and by ordinal text: if they changed nothing is
        /// discarded and the new list is shown to be reviewed; if not, it discards the drafts of the whole session, closing no tab.
        /// </summary>
        public void ConfirmDiscard()
        {
            if (_pilotEdit == null || _current == null) return;

            BeginGesture();
            var kind = _confirmKind;
            if (kind == ConfirmationKind.None) return;

            var session = _confirmSession;
            var rack = _confirmRack;
            if (session != _current.Id)
            {
                RemoveConfirmation();
                return;
            }

            try
            {
                if (kind == ConfirmationKind.DiscardDocument)
                {
                    PilotRiskView again;
                    try
                    {
                        again = _pilotEdit.GetRacksAtRisk(new WorkspaceRequest(session));
                    }
                    catch (Exception ex)
                    {
                        FailGesture(ex, "No se pudieron leer los borradores en riesgo: ");
                        return;
                    }

                    if (!SameList(again.RackIds, _confirmRacks) || !SameList(again.UncertainRackIds, _confirmUncertain))
                    {
                        if (again.RackIds.Count == 0 && again.UncertainRackIds.Count == 0)
                        {
                            RemoveConfirmation();
                            PostNotice(NoRiskText);
                        }
                        else
                        {
                            _confirmRacks = again.RackIds;
                            _confirmUncertain = again.UncertainRackIds;
                            RaiseConfirmation();
                            PostNotice("La lista de borradores en riesgo cambió; revísala.");
                        }

                        return;
                    }

                    var confirmed = _confirmRacks;
                    var uncertain = _confirmUncertain;
                    EditDiscardOutcome discarded;
                    try
                    {
                        discarded = _pilotEdit.DiscardDocument(new WorkspaceRequest(session));
                    }
                    catch (Exception ex)
                    {
                        FailGesture(ex, "No se descartaron los borradores del documento: ");
                        return;
                    }

                    RemoveConfirmation();
                    if (discarded.Status == EditDiscardStatus.Discarded)
                    {
                        ForgetVeto(session);
                        Rebuild(retryPending: false, userRequested: false);
                        Field.Rematerialize();
                        PostNotice(DocumentDiscardedText(confirmed, uncertain, discarded));
                    }
                    else
                    {
                        PostNotice("No se descartaron los borradores del documento: " + WhyOf(discarded));
                    }

                    return;
                }

                EditDiscardOutcome outcome;
                try
                {
                    outcome = _pilotEdit.DiscardRack(new WorkspaceRequest(session), rack);
                }
                catch (Exception ex)
                {
                    if (kind == ConfirmationKind.DiscardField) FieldDiscardFailed(session, rack, ex);
                    else TabDiscardFailed(session, rack, ex);
                    return;
                }

                if (kind == ConfirmationKind.DiscardField)
                {
                    FieldDiscarded(session, rack, outcome);
                    return;
                }

                RemoveConfirmation();
                if (outcome.Status == EditDiscardStatus.Discarded)
                {
                    ForgetRisk(session, rack);
                    CloseNow(rack, null);
                }
                else
                {
                    PostNotice("No se cerró la pestaña: " + WhyOf(outcome));
                }
            }
            catch (Exception ex)
            {
                FailGesture(ex, "No se pudo descartar: ");
            }
        }

        /// <summary>The Cancel button of the inline confirmation: it only removes the confirmation and asks nothing of the port.</summary>
        public void CancelDiscard()
        {
            BeginGesture();
            RemoveConfirmation();
        }

        /// <summary>The Understood button of a row of notice: it removes that row. It asks nothing of the port.</summary>
        public void DismissNotice(string key)
        {
            BeginGesture();
            var index = _stored.FindIndex(n => string.Equals(n.Key, key, StringComparison.Ordinal));
            if (index < 0) return;

            _stored.RemoveAt(index);
            Publish();
        }

        /// <summary>
        /// The first gesture on the field of the pilot (the Edit button, a click, a key on the read-only box): pins the tab if it is a
        /// preview and then asks the field to open the edition. Contained in the field; it never reaches the navigation.
        /// </summary>
        public void BeginFieldEdit()
        {
            if (_pilotEdit == null || _current == null) return;

            try
            {
                var rack = _current.ActiveRackId;
                if (rack == null) return;

                if (IsPreviewTab(rack)) PinTab(rack);
                Field.BeginEdit();
            }
            catch (Exception ex)
            {
                Field.Fail(ex);
            }
        }

        /// <summary>
        /// A click or a key on the read-only box: it is the first gesture only while the field offers the Edit button (no diagnostic of
        /// a read that was not admitted for this surface and no Error). Otherwise it does nothing: only the button reads again.
        /// </summary>
        public void BeginFieldEditFromBox()
        {
            if (_pilotEdit == null || !Field.CanOpenByGesture) return;
            BeginFieldEdit();
        }

        /// <summary>
        /// The double click on a tab button: it is a gesture of the field only when, after the clicks, that tab is the active one and the
        /// field offers the Edit button. With an editable box, a diagnostic or an Error it does nothing and keeps the notice.
        /// </summary>
        public void BeginFieldEditFromTab(string rackId)
        {
            if (_pilotEdit == null || _current == null || rackId == null) return;
            if (!string.Equals(_current.ActiveRackId, rackId, StringComparison.Ordinal)) return;
            if (!Field.CanOpenByGesture) return;
            BeginFieldEdit();
        }

        /// <summary>The Reload button of the Error state of the field: shows the draft again from memory. It never reads the drawing.</summary>
        public void ReloadField() => Field.Rematerialize();

        /// <summary>
        /// Asks again for the summary of the active tab, as a new materialization requested by the user, from any state of the surface
        /// (ready, not found, rejected, failed, waiting). It respects the drawing availability and the action block exactly as a
        /// presentation does: if the drawing cannot be read nothing is asked and the surface stays pending. Memory only: it never
        /// reaches the navigation and changes no tab.
        /// </summary>
        public void ReloadSummary()
        {
            if (_port == null || _current == null) return;

            try
            {
                var rack = _current.ActiveRackId;
                if (rack == null) return;

                StartSurface(rack);
                Materialize(userRequested: true);
                Publish();
                RenderField();
            }
            catch (Exception ex)
            {
                ContainFailure(ex);
            }
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

        // The sessions that no longer live are destroyed, and so are the notices of veto of a session that no longer lives, in the same
        // loop and with ONE question to the port per session. The notices of loss belong to sessions that are always gone: only the
        // user removes them.
        private void PurgeDeadSessions()
        {
            var known = _registry.KnownSessions;
            var sessions = new List<SessionId>(known);
            foreach (var stored in _stored)
                if (!stored.IsLoss && !sessions.Contains(stored.Close.Session)) sessions.Add(stored.Close.Session);

            foreach (var session in sessions)
            {
                if (_port.IsSessionAlive(session)) continue;

                if (known.Contains(session))
                {
                    _registry.Destroy(session);
                    _lastContexts.Remove(session);
                }

                _stored.RemoveAll(n => !n.IsLoss && n.Close.Session == session);
            }
        }

        // The selection of the snapshot is the only automatic synchronization. With the edit port a new context of one rack does not take the
        // active tab away from pending text (or when the port cannot say). Returns the notice of such a retention, or null.
        private string ApplySelection(SelectionContext context)
        {
            if (_current == null || context == null) return null;

            var id = _current.Id;
            var isNew = !(_lastContexts.TryGetValue(id, out var last) && last == context);
            _lastContexts[id] = context;

            var rack = context.RackId;
            var active = _current.ActiveRackId;
            var keepActive = isNew && !string.IsNullOrWhiteSpace(rack) && KeepsActive(id, active);
            var change = _current.ApplySelectionContext(context, keepActive);

            if (!keepActive || string.Equals(active, rack, StringComparison.Ordinal)) return null;
            if (change == BrowserChange.Created || change == BrowserChange.Replaced)
                return "La selección abrió " + rack + " en segundo plano; la pestaña activa conserva su texto pendiente.";
            return "La selección de " + rack + " no cambió la pestaña activa, que conserva su texto pendiente.";
        }

        // Only pending text holds the active tab; so does a port that cannot say. A tab that is only dirty does not.
        private bool KeepsActive(SessionId session, string active)
        {
            if (_pilotEdit == null || active == null) return false;

            try
            {
                return _pilotEdit.GetField(new WorkspaceRequest(session), active).PendingText != null;
            }
            catch (Exception)
            {
                return true;
            }
        }

        // The queues of notices are taken only here, only by the presentation, each one in its own try: a failure is a notice of the browser
        // and never stops the presentation.
        private void TakeNotices()
        {
            string failure = null;

            try
            {
                var closes = _pilotEdit.TakeCloseNotices();
                if (closes != null)
                    foreach (var close in closes) StoreVeto(close);
            }
            catch (Exception ex)
            {
                failure = ReasonOf(ex);
            }

            try
            {
                var losses = _pilotEdit.TakeLossNotices();
                if (losses != null)
                    foreach (var loss in losses) StoreLoss(loss);
            }
            catch (Exception ex)
            {
                failure = failure == null ? ReasonOf(ex) : failure + " " + ReasonOf(ex);
            }

            _takeFailure = failure == null ? null : "No se pudieron leer los avisos: " + failure;
        }

        // One veto per session: a new one of the same session replaces the old one where it was.
        private void StoreVeto(PilotCloseView close)
        {
            var key = "veto-" + close.Session.Value.ToString(CultureInfo.InvariantCulture);
            var notice = new StoredNotice(key, false, close, default);
            var index = _stored.FindIndex(n => string.Equals(n.Key, key, StringComparison.Ordinal));
            if (index >= 0) _stored[index] = notice;
            else _stored.Add(notice);
        }

        private void StoreLoss(PilotLossView loss)
        {
            _lossSerial = _lossSerial + 1;
            _stored.Add(new StoredNotice("loss-" + _lossSerial.ToString(CultureInfo.InvariantCulture), true, default, loss));
        }

        // The notice of the failure of the queues and the one of a retention are said after the rebuild, so a change of surface does not
        // take them away.
        private void PostPresentationNotice(string retention)
        {
            var notice = retention;
            if (_takeFailure != null)
            {
                notice = notice == null ? _takeFailure : _takeFailure + " " + notice;
                _takeFailure = null;
            }

            if (notice != null) PostNotice(notice);
        }

        // Brings the live surface to the active tab and publishes what the panel shows. Only a presentation retries a summary that was
        // waiting; a tab change asks only for the tab that just became the live surface. A presentation asks on its own account
        // (userRequested false); every explicit interaction of the user asks on his. A change of the live surface, or of the presented
        // session, ends the notice of the browser and the confirmations that belonged to the previous one.
        private void Rebuild(bool retryPending, bool userRequested, bool sessionChanged = false)
        {
            var priorSession = _surfaceSession;
            var priorRack = _surfaceRack;
            SyncSurface(retryPending, userRequested);
            if (sessionChanged || priorSession != _surfaceSession || !string.Equals(priorRack, _surfaceRack, StringComparison.Ordinal))
                _noticeText = string.Empty;

            ValidateConfirmation();
            Publish();
            RenderField();
        }

        // The strip, the rows of notice and everything the panel reads from the browser.
        private void Publish()
        {
            var tabsChanged = RebuildTabs();
            var noticesChanged = RebuildNotices();
            RaiseAll(tabsChanged, noticesChanged);
        }

        // The field follows the live surface: it is shown again from the memory of the port after the strip, with the tab as it is now
        // (a tab that was just pinned is no longer a preview). It reads no drawing and writes nothing.
        private void RenderField()
        {
            if (_pilotEdit == null) return;

            try
            {
                if (_surfaceSession.HasValue && _surfaceRack != null) Field.Show(_surfaceSession.Value, _surfaceRack, IsPreviewTab(_surfaceRack));
                else Field.Clear();
            }
            catch (Exception ex)
            {
                Field.Fail(ex);
            }
        }

        // The field asked for the strip again after it changed its draft: the mark of a protected tab and the pin may have changed.
        private void OnFieldChanged()
        {
            if (_port == null || _current == null) return;
            Rebuild(retryPending: false, userRequested: false);
        }

        private bool IsPreviewTab(string rackId)
        {
            var tab = _current?.Tabs.FirstOrDefault(t => string.Equals(t.RackId, rackId, StringComparison.Ordinal));
            return tab != null && tab.IsPreview;
        }

        private bool HasTab(string rackId) =>
            _current != null && _current.Tabs.Any(t => string.Equals(t.RackId, rackId, StringComparison.Ordinal));

        // The blocking boundary of a change of tab in the panel (D-T3B1-06): only to an existing tab other than the live one and only with
        // pending text in the live tab. It proceeds when the host proceeds or keeps; otherwise the tab does not change and the field says why.
        private bool AllowTabChange(string destination)
        {
            if (_pilotEdit == null || _current == null || !_session.HasValue) return true;

            var live = _current.ActiveRackId;
            if (live == null || string.Equals(destination, live, StringComparison.Ordinal)) return true;
            if (!_current.Tabs.Any(t => string.Equals(t.RackId, destination, StringComparison.Ordinal))) return true;

            try
            {
                var origin = new WorkspaceRequest(_session.Value);
                if (_pilotEdit.GetField(origin, live).PendingText == null) return true;

                var crossed = _pilotEdit.CrossBoundary(origin, live, PilotBoundaryKind.TabChange);
                if (crossed.Decision == PilotBoundaryDecision.Proceeded || crossed.Decision == PilotBoundaryDecision.Kept) return true;

                Field.ShowNotice(AbortText(crossed));
                return false;
            }
            catch (Exception ex)
            {
                Field.Fail(ex);
                return false;
            }
        }

        private static string AbortText(PilotBoundaryResult crossed)
        {
            string reason;
            if (crossed.InvalidFields.Count > 0)
                reason = string.Join(" ", crossed.InvalidFields.Select(f => string.IsNullOrWhiteSpace(f.Message) ? f.FieldKey : f.Message));
            else
                reason = string.IsNullOrWhiteSpace(crossed.Reason) ? "sin detalle." : crossed.Reason;
            return "No se cambió de pestaña: " + reason;
        }

        // The boundaries the panel observes when a presentation changed the live surface (D-T3B1-06). A change of the presented session
        // crosses DocumentChange on the previous tab; a change of the active tab by the selection crosses Dematerialize on the previous
        // one when it had a draft. Neither blocks, neither reads the drawing and neither changes the pending text.
        private void CrossObservedBoundaries(SessionId? priorSession, string priorRack)
        {
            if (_pilotEdit == null || !priorSession.HasValue || priorRack == null) return;
            if (priorSession == _surfaceSession && string.Equals(priorRack, _surfaceRack, StringComparison.Ordinal)) return;

            try
            {
                var origin = new WorkspaceRequest(priorSession.Value);
                if (priorSession != _surfaceSession)
                    _pilotEdit.CrossBoundary(origin, priorRack, PilotBoundaryKind.DocumentChange);
                else if (_pilotEdit.GetField(origin, priorRack).Exists)
                    _pilotEdit.CrossBoundary(origin, priorRack, PilotBoundaryKind.Dematerialize);
            }
            catch (Exception)
            {
                // The boundary of the previous surface is not the new one's business: a failure here is contained and does not put the
                // field of the surface that is now live in Error.
            }
        }

        // ------------------------------------------------------------------ closing, discarding and notices (I-64 F4-T3B2)

        // A gesture of this task starts by ending the notice of the browser.
        private void BeginGesture()
        {
            if (_noticeText.Length == 0) return;
            _noticeText = string.Empty;
            OnPropertyChanged(nameof(NoticeText));
        }

        private void PostNotice(string text)
        {
            _noticeText = text ?? string.Empty;
            OnPropertyChanged(nameof(NoticeText));
        }

        // The failure of a gesture of this task: the notice says why and the confirmation goes. It never reaches the panel.
        private void FailGesture(Exception exception, string prefix)
        {
            RemoveConfirmation();
            PostNotice(prefix + ReasonOf(exception));
        }

        // Closes the tab in memory; a notice for after the rebuild may come with it.
        private void CloseNow(string rackId, string noticeAfter)
        {
            Interact(session => session.Close(rackId));
            if (noticeAfter != null) PostNotice(noticeAfter);
        }

        // What the port can say about a rack, from its memory only; null when it cannot (the tab is then treated as protected).
        private PilotFieldView? ReadField(SessionId session, string rackId)
        {
            try
            {
                return _pilotEdit.GetField(new WorkspaceRequest(session), rackId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string CleanDraftKeptText(string why) =>
            "Se cerró la pestaña; su borrador limpio quedó en memoria: " + why;

        private static string WhyOf(EditDiscardOutcome outcome)
        {
            var reason = string.IsNullOrWhiteSpace(outcome.Reason) ? "sin detalle." : outcome.Reason;
            switch (outcome.Status)
            {
                case EditDiscardStatus.Busy: return "hay una lectura en curso. " + reason;
                case EditDiscardStatus.Failed: return "falló. " + reason;
                case EditDiscardStatus.Rejected: return "se rechazó. " + reason;
                default: return reason;
            }
        }

        // The field was discarded (directly or after the confirmation). With Discarded the tab stays, the field leaves the Error and is
        // shown from memory and the mark is recalculated; with another answer the field stays as it was and a notice says why.
        private void FieldDiscarded(SessionId session, string rack, EditDiscardOutcome outcome)
        {
            RemoveConfirmation();
            if (outcome.Status == EditDiscardStatus.Discarded)
            {
                ForgetRisk(session, rack);
                Rebuild(retryPending: false, userRequested: false);
                Field.Rematerialize();
                return;
            }

            PostNotice("No se descartó el borrador: " + WhyOf(outcome));
        }

        // The port threw while discarding the field: the notice says why, the confirmation goes, and the field shows the Error with its draft.
        private void FieldDiscardFailed(SessionId session, string rack, Exception exception)
        {
            FailGesture(exception, "No se descartó el borrador: ");
            FailFieldIfLive(session, rack, exception);
        }

        // The port threw while discarding the draft of a tab to close it: the tab stays open and is not closed.
        private void TabDiscardFailed(SessionId session, string rack, Exception exception)
        {
            FailGesture(exception, "No se cerró la pestaña: ");
            FailFieldIfLive(session, rack, exception);
        }

        // The field shows the failure only while its tab is still open and is the live surface.
        private void FailFieldIfLive(SessionId session, string rack, Exception exception)
        {
            if (_current == null || _current.Id != session || !HasTab(rack)) return;
            if (!_surfaceSession.HasValue || _surfaceSession.Value != session || !string.Equals(_surfaceRack, rack, StringComparison.Ordinal)) return;
            Field.Fail(exception);
        }

        // ------------------------------------------------------------------ the inline confirmation

        private void Ask(ConfirmationKind kind, SessionId session, string rack)
        {
            _confirmKind = kind;
            _confirmSession = session;
            _confirmRack = rack;
            _confirmRacks = Array.Empty<string>();
            _confirmUncertain = Array.Empty<string>();
            RaiseConfirmation();
        }

        private void RemoveConfirmation()
        {
            if (_confirmKind == ConfirmationKind.None) return;
            _confirmKind = ConfirmationKind.None;
            _confirmRack = null;
            _confirmRacks = Array.Empty<string>();
            _confirmUncertain = Array.Empty<string>();
            RaiseConfirmation();
        }

        private void RaiseConfirmation()
        {
            OnPropertyChanged(nameof(HasConfirmation));
            OnPropertyChanged(nameof(ConfirmationText));
        }

        // The confirmation ends with a change of the presented session, when its tab stops existing and, the one of the field, when the live
        // surface changes.
        private void ValidateConfirmation()
        {
            if (_confirmKind == ConfirmationKind.None) return;

            var keep = _current != null && _confirmSession == _current.Id;
            if (keep && _confirmKind != ConfirmationKind.DiscardDocument) keep = HasTab(_confirmRack);
            if (keep && _confirmKind == ConfirmationKind.DiscardField)
                keep = _surfaceSession.HasValue && _surfaceSession.Value == _confirmSession && string.Equals(_surfaceRack, _confirmRack, StringComparison.Ordinal);
            if (!keep) RemoveConfirmation();
        }

        private string ConfirmationTextOf()
        {
            switch (_confirmKind)
            {
                case ConfirmationKind.CloseTab:
                    return "La pestaña " + TitleOf(_confirmRack) + " tiene un borrador con trabajo sin aplicar. ¿Descartarlo y cerrar la pestaña?";
                case ConfirmationKind.DiscardField:
                    return "¿Descartar el borrador de " + TitleOf(_confirmRack) + "? La pestaña se queda abierta.";
                default:
                    var text = "¿Descartar los borradores de este documento? Se perderá el trabajo sin aplicar de: " + TitlesOf(_confirmRacks) + ".";
                    if (_confirmUncertain.Count > 0) text = text + " No se pudo decidir: " + TitlesOf(_confirmUncertain) + ".";
                    return text;
            }
        }

        // The title of a rack as the strip shows it without the mark: the name of its record, or its rack id.
        private string TitleOf(string rackId)
        {
            var tab = _current?.Tabs.FirstOrDefault(t => string.Equals(t.RackId, rackId, StringComparison.Ordinal));
            return tab == null || string.IsNullOrWhiteSpace(tab.Name) ? rackId : tab.Name;
        }

        private string TitlesOf(IReadOnlyList<string> racks) => string.Join(", ", racks.Select(TitleOf));

        private static bool SameList(IReadOnlyList<string> first, IReadOnlyList<string> second)
        {
            if (first.Count != second.Count) return false;
            for (var i = 0; i < first.Count; i++)
                if (!string.Equals(first[i], second[i], StringComparison.Ordinal)) return false;
            return true;
        }

        private string DocumentDiscardedText(IReadOnlyList<string> confirmed, IReadOnlyList<string> uncertain, EditDiscardOutcome outcome)
        {
            var parts = new List<string>();
            if (confirmed.Count > 0) parts.Add("Se descartaron los borradores en riesgo: " + TitlesOf(confirmed) + ".");
            else parts.Add("Se descartaron los borradores del documento.");
            if (uncertain.Count > 0) parts.Add("Sin decidir, también descartados: " + TitlesOf(uncertain) + ".");

            var clean = outcome.RemovedRackIds.Count(r =>
                !confirmed.Contains(r, StringComparer.Ordinal) && !uncertain.Contains(r, StringComparer.Ordinal));
            parts.Add("Borradores limpios retirados además: " + clean.ToString(CultureInfo.InvariantCulture) + ".");
            return string.Join(" ", parts);
        }

        // ------------------------------------------------------------------ the notices of veto and loss

        // The rack left the veto of its session because its draft was discarded; the row goes when nothing is left to say.
        private void ForgetRisk(SessionId session, string rackId)
        {
            for (var i = 0; i < _stored.Count; i++)
            {
                var stored = _stored[i];
                if (stored.IsLoss || stored.Close.Session != session) continue;

                var left = stored.Close.RackIds.Where(r => !string.Equals(r, rackId, StringComparison.Ordinal)).ToList();
                if (left.Count == stored.Close.RackIds.Count) return;

                if (left.Count == 0 && !stored.Close.IsUncertain) _stored.RemoveAt(i);
                else stored.Close = new PilotCloseView(session, left, stored.Close.IsUncertain);
                return;
            }
        }

        // The drafts of the session were discarded: its veto has nothing left to say.
        private void ForgetVeto(SessionId session) =>
            _stored.RemoveAll(n => !n.IsLoss && n.Close.Session == session);

        // The rows are recalculated from the notices kept, classified now against the presented session, and keep their instance when
        // they do not change by value (as the strip does). Returns whether the rows changed.
        private bool RebuildNotices()
        {
            var rows = new List<WorkspaceBrowserNoticeItem>();
            foreach (var stored in _stored)
            {
                var row = RowOf(stored);
                var index = rows.Count;
                rows.Add(index < _notices.Count && _notices[index].Equals(row) ? _notices[index] : row);
            }

            if (_notices.Count == rows.Count)
            {
                var same = true;
                for (var i = 0; i < rows.Count && same; i++)
                    same = ReferenceEquals(_notices[i], rows[i]);
                if (same) return false;
            }

            _notices = rows;
            return true;
        }

        private WorkspaceBrowserNoticeItem RowOf(StoredNotice stored)
        {
            if (stored.IsLoss) return new WorkspaceBrowserNoticeItem(stored.Key, LossText(stored.Loss), false);

            var close = stored.Close;
            if (!_session.HasValue || close.Session != _session.Value)
                return new WorkspaceBrowserNoticeItem(stored.Key, "No se cerró otro documento con borradores sin aplicar; actívalo para descartarlos.", false);

            var head = close.IsUncertain || close.RackIds.Count == 0
                ? "No se cerró el documento: no se pudo decidir qué borradores están en riesgo."
                : "No se cerró el documento: hay borradores sin aplicar en " + TitlesOf(close.RackIds) + ".";
            var text = head + " Descártalos con \"Descartar borradores de este documento\"; cada pestaña también se descarta con su \"Cerrar\".";
            return new WorkspaceBrowserNoticeItem(stored.Key, text, true);
        }

        private static string LossText(PilotLossView loss)
        {
            var parts = new List<string>();
            if (loss.LostRackIds.Count > 0)
                parts.Add("Se perdieron borradores al cerrar un documento: " + string.Join(", ", loss.LostRackIds) + ".");
            else
                parts.Add("Se cerró un documento con borradores que no se pudieron conservar.");

            if (loss.UncertainRackIds.Count > 0)
            {
                var uncertain = loss.UncertainRackIds.Select(r =>
                    loss.UncertainReasons.TryGetValue(r, out var reason) && !string.IsNullOrWhiteSpace(reason) ? r + " (" + reason + ")" : r);
                parts.Add("Sin decidir: " + string.Join(", ", uncertain) + ".");
            }

            return string.Join(" ", parts);
        }

        // ------------------------------------------------------------------ the live surface

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
            if (started) StartSurface(rack);

            if (started || (retryPending && (_surfaceState == SurfaceState.Pending || _surfaceState == SurfaceState.Busy))) Materialize(userRequested);
        }

        // The live surface starts waiting for the summary of the rack: nothing of a previous reading stays on it.
        private void StartSurface(string rack)
        {
            _surfaceSession = _session;
            _surfaceRack = rack;
            _surfaceState = SurfaceState.Pending;
            _surfaceName = null;
            _surfaceKind = null;
            _surfaceViews = Array.Empty<string>();
            _surfaceReason = string.Empty;
            if (_pilotEdit != null && _session.HasValue) Field.Show(_session.Value, rack, IsPreviewTab(rack));
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
            Field.Clear();
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
                Publish();
            }
            catch (Exception)
            {
                // Nothing else can be done here without risking the message loop of the host.
            }
        }

        // The strip keeps its instance when the new tabs are equal by value to the ones it shows (T2-F6): a presentation that changed
        // nothing does not make the view rebuild every container of the strip, which would lose the keyboard focus and a click.
        // Returns whether the strip changed.
        private bool RebuildTabs()
        {
            var tabs = new List<WorkspaceBrowserTabItem>();
            if (_current != null)
            {
                var active = _current.ActiveRackId;
                foreach (var tab in _current.Tabs)
                {
                    var title = string.IsNullOrWhiteSpace(tab.Name) ? tab.RackId : tab.Name;
                    var isPreview = tab.IsPreview;

                    // A tab whose draft would lose something when the document closes carries the mark; a preview with such a draft is
                    // pinned so the selection never replaces it. Both read the memory of the port only.
                    if (IsProtected(tab.RackId))
                    {
                        title = title + " *";
                        if (isPreview)
                        {
                            _current.Pin(tab.RackId);
                            isPreview = false;
                        }
                    }

                    tabs.Add(new WorkspaceBrowserTabItem(
                        tab.RackId, title, string.Equals(tab.RackId, active, StringComparison.Ordinal), isPreview));
                }
            }

            if (SameTabs(_tabs, tabs)) return false;

            _tabs = tabs;
            return true;
        }

        private bool IsProtected(string rackId)
        {
            if (_pilotEdit == null || _current == null) return false;

            try
            {
                var view = _pilotEdit.GetField(new WorkspaceRequest(_current.Id), rackId);
                return view.Exists && view.IsProtected;
            }
            catch (Exception)
            {
                // The conservative answer: when the port cannot say, the tab is treated as protected (it carries the mark and a preview is pinned).
                return true;
            }
        }

        private static bool SameTabs(IReadOnlyList<WorkspaceBrowserTabItem> current, IReadOnlyList<WorkspaceBrowserTabItem> next)
        {
            if (current.Count != next.Count) return false;
            for (var i = 0; i < current.Count; i++)
                if (!current[i].Equals(next[i])) return false;
            return true;
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

        private void RaiseAll(bool tabsChanged, bool noticesChanged = false)
        {
            OnPropertyChanged(nameof(IsAvailable));
            if (tabsChanged) OnPropertyChanged(nameof(Tabs));
            OnPropertyChanged(nameof(HasTabs));
            OnPropertyChanged(nameof(ActiveRackId));
            OnPropertyChanged(nameof(LiveSurfaceCount));
            OnPropertyChanged(nameof(HasSurface));
            OnPropertyChanged(nameof(EmptyText));
            OnPropertyChanged(nameof(SurfaceTitle));
            OnPropertyChanged(nameof(SurfaceKindText));
            OnPropertyChanged(nameof(SurfaceViewsText));
            OnPropertyChanged(nameof(SurfaceStatusText));
            OnPropertyChanged(nameof(CanDiscardDocument));
            OnPropertyChanged(nameof(HasConfirmation));
            OnPropertyChanged(nameof(ConfirmationText));
            OnPropertyChanged(nameof(NoticeText));
            if (noticesChanged) OnPropertyChanged(nameof(Notices));
            OnPropertyChanged(nameof(HasNotices));
        }
    }
}
