#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.PushBack.Pilot;
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
    ///
    /// Since I-64 F4-T2B2A it also owns the one registry of drafts of the process and its close policy (keys: session and rack),
    /// and offers two operations of value for the user: <see cref="TryBeginPilotEdit"/> (the edit intention of the bounded Push
    /// Back pilot) and <see cref="TryCheckPilotEdit"/> ("check now"). Each is ONE user action with at most ONE logical read, made
    /// through <see cref="IWorkspaceEditIntentReader"/>; neither runs from the drain or from the idle pass, and until the panel
    /// exposes them (F4-T3) nothing calls them.
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
        private IWorkspaceEditIntentReader? _editIntentReader;
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

        // ---------------------------------------------------------------- the edit intention of the pilot (I-64 F4-T2B2A)

        /// <summary>The registry of drafts of the process, for the tests of this assembly. Nothing of the product reads it from outside.</summary>
        internal DraftRegistry OpenDrafts => _drafts;

        /// <summary>The close policy over <see cref="OpenDrafts"/>. Until the lifecycle is wired nothing consults it.</summary>
        internal DraftClosePolicy ClosePolicy => _closePolicy;

        /// <summary>
        /// Assigns the reader of the edit intention. It is wired by assignment and never by constructor, so the constructor of the
        /// controller and the arguments of the navigation host do not change. Without a reader the operations answer Unavailable.
        /// </summary>
        public void SetEditIntentReader(IWorkspaceEditIntentReader reader) =>
            _editIntentReader = reader ?? throw new ArgumentNullException(nameof(reader));

        /// <summary>
        /// The edit intention of the user for one rack of the bounded Push Back pilot (D-T2B2-06): at most ONE logical read, and a
        /// draft only when the read is admitted. Steps 1 to 5 read nothing; the order is the one of the table of the package.
        /// Every exception is contained as Failed and the session is not degraded: it is an action of the user, not the drain.
        /// </summary>
        public EditIntentOutcome TryBeginPilotEdit(EditIntentRequest request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                // 1: a blank RackId has no identity, and the registry would throw on it.
                if (string.IsNullOrWhiteSpace(request.RackId))
                {
                    return EditIntentOutcome.NotAdmitted(
                        PushBackPilotIntentReader.NoIdentityDiagnostic,
                        null,
                        PushBackPilotIntentReader.ClassicEditorHint,
                        EditWitnessStatus.NotEvaluated);
                }

                // 2 to 5: the reader, the probe of the active document, the modal or the command, the authorization.
                var gate = PassGate(request.Session, out var reason);
                if (gate == EditGate.Unavailable) return EditIntentOutcome.Unavailable(reason);
                if (gate == EditGate.Waiting) return EditIntentOutcome.Waiting(reason);
                if (gate == EditGate.Rejected) return EditIntentOutcome.Rejected(reason);

                // 6: an open draft is never read again.
                if (_drafts.Contains(request.Session, request.RackId)) return EditIntentOutcome.AlreadyOpen();

                // 7: the one read.
                var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
                switch (read.Status)
                {
                    case EditIntentReadStatus.Read:
                        break;
                    case EditIntentReadStatus.NoActiveDocument:
                    case EditIntentReadStatus.Busy:
                        return EditIntentOutcome.Waiting(read.Reason ?? "The drawing is not ready to be read.");
                    case EditIntentReadStatus.Rejected:
                        return EditIntentOutcome.Rejected(read.Reason ?? "The request does not belong to the active document.");
                    default:
                        return EditIntentOutcome.Failed(read.Reason ?? "The read of the edit intention failed.");
                }

                var pilot = read.PilotRead;
                if (pilot == null) return EditIntentOutcome.Failed("The read of the edit intention carries no result.");
                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);

                // 8: the base of the read only goes to Begin, with the comparer and the copier of the pilot.
                var begun = _drafts.Begin(
                    request.Session,
                    request.RackId,
                    pilot.Read,
                    PushBackPilotStateComparer.Instance,
                    PushBackPilotStateComparer.Copy);
                switch (begun.Status)
                {
                    case DraftBeginStatus.Created:
                        return EditIntentOutcome.Admitted(witness);
                    case DraftBeginStatus.AlreadyExists:
                        return EditIntentOutcome.AlreadyOpen();
                    case DraftBeginStatus.NotAdmitted:
                        return EditIntentOutcome.NotAdmitted(
                            begun.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,
                            pilot.Detail,
                            pilot.ClassicEditorHint,
                            witness);
                    default:
                        return EditIntentOutcome.Rejected("The session was retired.", witness);
                }
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.TryBeginPilotEdit", ex);
                return EditIntentOutcome.Failed(ex.Message);
            }
        }

        /// <summary>
        /// "Check now" for one rack of the pilot (D-T2B2-06, D-12): it acts only on a draft that is Unknown after a commit that
        /// threw, rereads once and compares the new base with the one from before the attempt. Anything else reads nothing.
        /// Every exception is contained as Failed and the session is not degraded.
        /// </summary>
        public EditCheckOutcome TryCheckPilotEdit(EditIntentRequest request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                // 1: a blank RackId has no draft, and the registry would throw on it.
                if (string.IsNullOrWhiteSpace(request.RackId))
                    return EditCheckOutcome.NotFound("A blank rack has no draft.");

                // 2 to 5: the same gate as the intention; nothing is read.
                var gate = PassGate(request.Session, out var reason);
                if (gate == EditGate.Unavailable) return EditCheckOutcome.Unavailable(reason);
                if (gate == EditGate.Waiting) return EditCheckOutcome.Waiting(reason);
                if (gate == EditGate.Rejected) return EditCheckOutcome.Rejected(reason);

                // 6: only an Unknown draft after a commit that threw is checked; nothing is read for any other.
                var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);
                if (draft == null) return EditCheckOutcome.NotFound("There is no draft for the rack.");
                if (draft.Status != DraftStatus.Unknown || draft.UnknownOrigin != DraftUnknownOrigin.CommitThrew)
                    return EditCheckOutcome.NotApplicable("The draft is not unknown after a commit that threw.");

                // 7: the one reread.
                var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
                switch (read.Status)
                {
                    case EditIntentReadStatus.Read:
                        break;
                    case EditIntentReadStatus.NoActiveDocument:
                    case EditIntentReadStatus.Busy:
                        return EditCheckOutcome.Waiting(read.Reason ?? "The drawing is not ready to be read.");
                    case EditIntentReadStatus.Rejected:
                        return EditCheckOutcome.Rejected(read.Reason ?? "The request does not belong to the active document.");
                    default:
                        return EditCheckOutcome.Failed(read.Reason ?? "The reread failed.");
                }

                var pilot = read.PilotRead;
                if (pilot == null) return EditCheckOutcome.Failed("The reread carries no result.");

                // 8: the base of the reread only goes to CheckNow.
                var check = draft.CheckNow(pilot.StaleBase);
                switch (check.Status)
                {
                    case DraftCheckStatus.Recovered:
                        return EditCheckOutcome.Recovered();
                    case DraftCheckStatus.Conflict:
                        return EditCheckOutcome.Conflict(DescribeDifferences(check.Differences));
                    default:
                        return EditCheckOutcome.NotApplicable("The draft is not unknown after a commit that threw.");
                }
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.TryCheckPilotEdit", ex);
                return EditCheckOutcome.Failed(ex.Message);
            }
        }

        private enum EditGate
        {
            Open,
            Unavailable,
            Waiting,
            Rejected,
        }

        // Steps 2 to 5 of both operations: nothing of the drawing is read here.
        private EditGate PassGate(SessionId session, out string reason)
        {
            reason = string.Empty;
            if (_editIntentReader == null)
            {
                reason = "No reader of the edit intention is assigned.";
                return EditGate.Unavailable;
            }

            if (!_probe.ActiveDocumentNativeId.HasValue)
            {
                reason = "There is no active document.";
                return EditGate.Waiting;
            }

            if (_modalRackCadActive)
            {
                reason = "A RackCad window is open.";
                return EditGate.Waiting;
            }

            if (!_probe.IsActiveDocumentQuiescent)
            {
                reason = "A command is in progress.";
                return EditGate.Waiting;
            }

            if (Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted)
            {
                reason = "The request does not belong to the active document.";
                return EditGate.Rejected;
            }

            return EditGate.Open;
        }

        private static IReadOnlyList<string> DescribeDifferences(IReadOnlyList<DraftDifference> differences) =>
            differences.Select(d => (d.Key ?? "(variable registry)") + ":" + d.Kind).ToArray();

        // A failed operation of the user is logged and returned as a value; unlike the drain it never degrades the session.
        private void Report(string context, Exception exception)
        {
            try
            {
                _logFault?.Invoke(context, exception);
            }
            catch
            {
                // Logging must never become a second fault.
            }
        }

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
