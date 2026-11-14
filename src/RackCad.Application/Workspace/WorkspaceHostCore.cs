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
    ///
    /// Since I-64 F4-T2B2B it also implements, explicitly, <see cref="IWorkspaceLifecycleAdvisor"/>: the veto of the close of a document
    /// that may hold unsaved drafts, and the retirement of the drafts of a destroyed session, with a notice when work was lost. It adds
    /// the stale hint of the drain, the lazy reload, the explicit discard and the in-memory "known clean" mark of a session.
    /// </summary>
    public sealed class WorkspaceHostCore : IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort
    {
        // I-64 F4-T3A (D-T3-02): the one adapter of the pending text of the pilot. It keeps no state, so it is shared; it is never an
        // instance field of the host.
        private static readonly PushBackPilotFieldAdapter PilotFieldAdapter = new();

        private const string PilotReadInProgressReason = "Hay una lectura del piloto en curso.";
        private const string NotThisDocumentReason = "La petición no es del documento activo.";
        private const string NoDraftReason = "No hay borrador para el rack.";
        private const string NotCheckableReason = "El borrador no admite comprobación en su estado actual.";
        private const string NotKeepableReason = "Solo se conserva un borrador en conflicto o con cambios marcados como posiblemente obsoletos.";

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

        // I-64 F4-T2B2B (D-T2B2-10, D-T2B2-13): the sessions whose drafts the last evaluation of the close found clean (the "known
        // clean" mark, in memory only), and the notices of loss that nobody consumed yet.
        private readonly HashSet<SessionId> _knownClean = new();
        private readonly Queue<DraftLossNotice> _lossNotices = new();

        // I-64 F4-T3A (D-T3-02, D-T3-07): the notices of a vetoed close, at most one per session (the last), and the mark that a read of
        // the pilot is in progress (set with try/finally around the three reads of the edit port).
        private readonly List<PilotCloseView> _closeNotices = new();
        private bool _pilotReadInProgress;
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
                // I-64 F4-T2B2B (D-T2B2-10, D-T2B2-11), before any early return: every drain of the session ends the "known clean"
                // mark, and a drain that carries the stale hint marks every draft of the pilot of that session. Only a hint: it reads
                // nothing and raises no conflict.
                _knownClean.Remove(outcome.Session);
                if (outcome.Result.Operations.Contains(DrainOperation.MarkDraftsPossiblyStale)) MarkPilotDraftsPossiblyStale(outcome.Session);

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

                // 1, continued: a witness that is present must carry a key; one without it is a failure of the request, before the
                // reader, the gate and the registry are touched, and the witness is not evaluated.
                if (HasWitnessWithoutKey(request))
                    return EditIntentOutcome.Failed("El testigo de la vista previa no tiene clave.");

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
                        return EditIntentOutcome.Waiting(read.Reason ?? "El dibujo no está listo para leerse.");
                    case EditIntentReadStatus.Rejected:
                        return EditIntentOutcome.Rejected(read.Reason ?? "La petición no es del documento activo.");
                    default:
                        return EditIntentOutcome.Failed(read.Reason ?? "Falló la lectura de la intención de edición.");
                }

                var pilot = read.PilotRead;
                if (pilot == null) return EditIntentOutcome.Failed("La lectura de la intención de edición no trajo resultado.");
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
                        _knownClean.Remove(request.Session);
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
                        return EditIntentOutcome.Rejected("La sesión se retiró.", witness);
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
                    return EditCheckOutcome.NotFound("Un rack sin identificador no tiene borrador.");

                // 1, continued: the same request error as the intention; nothing is read and the draft is not touched.
                if (HasWitnessWithoutKey(request))
                    return EditCheckOutcome.Failed("El testigo de la vista previa no tiene clave.");

                // 2 to 5: the same gate as the intention; nothing is read.
                var gate = PassGate(request.Session, out var reason);
                if (gate == EditGate.Unavailable) return EditCheckOutcome.Unavailable(reason);
                if (gate == EditGate.Waiting) return EditCheckOutcome.Waiting(reason);
                if (gate == EditGate.Rejected) return EditCheckOutcome.Rejected(reason);

                // 6: only an Unknown draft after a commit that threw is checked; nothing is read for any other.
                var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);
                if (draft == null) return EditCheckOutcome.NotFound("No hay borrador para el rack.");
                if (draft.Status != DraftStatus.Unknown || draft.UnknownOrigin != DraftUnknownOrigin.CommitThrew)
                    return EditCheckOutcome.NotApplicable("El borrador no quedó desconocido tras una aplicación fallida.");

                // 7: the one reread.
                var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
                switch (read.Status)
                {
                    case EditIntentReadStatus.Read:
                        break;
                    case EditIntentReadStatus.NoActiveDocument:
                    case EditIntentReadStatus.Busy:
                        return EditCheckOutcome.Waiting(read.Reason ?? "El dibujo no está listo para leerse.");
                    case EditIntentReadStatus.Rejected:
                        return EditCheckOutcome.Rejected(read.Reason ?? "La petición no es del documento activo.");
                    default:
                        return EditCheckOutcome.Failed(read.Reason ?? "Falló la relectura.");
                }

                var pilot = read.PilotRead;
                if (pilot == null) return EditCheckOutcome.Failed("La relectura no trajo resultado.");

                // 8: the base of the reread only goes to CheckNow.
                var check = draft.CheckNow(pilot.StaleBase);
                switch (check.Status)
                {
                    case DraftCheckStatus.Recovered:
                        _knownClean.Remove(request.Session);
                        return EditCheckOutcome.Recovered();
                    case DraftCheckStatus.Conflict:
                        _knownClean.Remove(request.Session);
                        return EditCheckOutcome.Conflict(DescribeDifferences(check.Differences));
                    default:
                        return EditCheckOutcome.NotApplicable("El borrador no quedó desconocido tras una aplicación fallida.");
                }
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.TryCheckPilotEdit", ex);
                return EditCheckOutcome.Failed(ex.Message);
            }
        }

        // ---------------------------------------------------------------- the lifecycle of the drafts (I-64 F4-T2B2B)

        /// <summary>
        /// The lazy reload of one rack of the pilot (D-T2B2-12), by action of the user and never from the drain. It follows steps 1 to 8
        /// of the table of the package: a missing draft is NotFound and a draft that is not marked as possibly stale and clean is
        /// NotNeeded, both without any read; otherwise it makes the one read and replaces the draft from it. Every exception is contained
        /// as Failed and the session is not degraded.
        /// </summary>
        public EditIntentOutcome TryReloadPilotEdit(EditIntentRequest request)
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

                // 1, continued: the same request error as the intention, before the reader, the gate and the draft are touched.
                if (HasWitnessWithoutKey(request))
                    return EditIntentOutcome.Failed("El testigo de la vista previa no tiene clave.");

                // 2 to 5: the same gate as the intention; nothing is read.
                var gate = PassGate(request.Session, out var reason);
                if (gate == EditGate.Unavailable) return EditIntentOutcome.Unavailable(reason);
                if (gate == EditGate.Waiting) return EditIntentOutcome.Waiting(reason);
                if (gate == EditGate.Rejected) return EditIntentOutcome.Rejected(reason);

                // 6: only a clean draft marked as possibly stale is reloaded; nothing is read for any other.
                var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);
                if (draft == null) return EditIntentOutcome.NotFound("No hay borrador para el rack.");
                if (!draft.NeedsLazyReload) return EditIntentOutcome.NotNeeded("El borrador no necesita recargarse.");

                // 7: the one read.
                var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
                switch (read.Status)
                {
                    case EditIntentReadStatus.Read:
                        break;
                    case EditIntentReadStatus.NoActiveDocument:
                    case EditIntentReadStatus.Busy:
                        return EditIntentOutcome.Waiting(read.Reason ?? "El dibujo no está listo para leerse.");
                    case EditIntentReadStatus.Rejected:
                        return EditIntentOutcome.Rejected(read.Reason ?? "La petición no es del documento activo.");
                    default:
                        return EditIntentOutcome.Failed(read.Reason ?? "Falló la lectura para la recarga.");
                }

                var pilot = read.PilotRead;
                if (pilot == null) return EditIntentOutcome.Failed("La lectura para la recarga no trajo resultado.");
                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);

                // 8: the read only goes to DiscardAndReload.
                var reloaded = draft.DiscardAndReload(pilot.Read);
                switch (reloaded.Status)
                {
                    case DraftReloadStatus.Reloaded:
                        _knownClean.Remove(request.Session);
                        return EditIntentOutcome.Reloaded(witness);
                    case DraftReloadStatus.NotAdmitted:
                        return EditIntentOutcome.NotAdmitted(
                            reloaded.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,
                            pilot.Detail,
                            pilot.ClassicEditorHint,
                            witness);
                    case DraftReloadStatus.NotAllowed:
                        return EditIntentOutcome.Waiting("El borrador se está aplicando o ya no está registrado.");
                    default:
                        return EditIntentOutcome.Failed("El borrador no se recargó.");
                }
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.TryReloadPilotEdit", ex);
                return EditIntentOutcome.Failed(ex.Message);
            }
        }

        // ---------------------------------------------------------------- the explicit reconciliation of the pilot (I-64 F4-T4A1)
        //
        // The three operations below follow steps 1 to 8 of the check and of the reload: the errors of the request, the gate, a missing draft,
        // then WHETHER THE OPERATION APPLIES to the status of the draft (all of it before anything is read), the one read, the consumption of
        // the read and the end of the mark of "known clean" when the draft changed. None of them reads the drawing more than once, and none
        // writes anything. Every exception is contained as Failed and the session is not degraded.

        /// <summary>
        /// The general "check now" of one rack of the pilot (D-12, D-T4A1-03): it serves a draft that is clean, dirty or in conflict,
        /// rereads once and compares the new base with the one of the draft. A difference puts the draft in conflict; a match never takes
        /// it out of conflict.
        /// </summary>
        public EditConfirmOutcome TryConfirmPilotBase(EditIntentRequest request)
        {
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                // 1: a blank RackId has no draft, and the registry would throw on it.
                if (string.IsNullOrWhiteSpace(request.RackId))
                    return EditConfirmOutcome.NotFound("Un rack sin identificador no tiene borrador.");

                // 1, continued: the same request error as the other operations; nothing is read.
                if (HasWitnessWithoutKey(request))
                    return EditConfirmOutcome.Failed("El testigo de la vista previa no tiene clave.");

                // 2 to 5: the same gate; nothing is read.
                var gate = PassGate(request.Session, out var reason);
                if (gate == EditGate.Unavailable) return EditConfirmOutcome.Unavailable(reason);
                if (gate == EditGate.Waiting) return EditConfirmOutcome.Waiting(reason);
                if (gate == EditGate.Rejected) return EditConfirmOutcome.Rejected(reason);

                // 3 and 4: a missing draft, and a draft whose status is not clean, dirty or in conflict, read nothing.
                var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);
                if (draft == null) return EditConfirmOutcome.NotFound(NoDraftReason);
                var before = draft.Status;
                if (before != DraftStatus.Clean && before != DraftStatus.Dirty && before != DraftStatus.Conflict)
                    return EditConfirmOutcome.NotApplicable(NotCheckableReason);

                // 6: the one reread.
                var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
                switch (read.Status)
                {
                    case EditIntentReadStatus.Read:
                        break;
                    case EditIntentReadStatus.NoActiveDocument:
                    case EditIntentReadStatus.Busy:
                        return EditConfirmOutcome.Waiting(read.Reason ?? "El dibujo no está listo para leerse.");
                    case EditIntentReadStatus.Rejected:
                        return EditConfirmOutcome.Rejected(read.Reason ?? NotThisDocumentReason);
                    default:
                        return EditConfirmOutcome.Failed(read.Reason ?? "Falló la relectura.");
                }

                var pilot = read.PilotRead;
                if (pilot == null) return EditConfirmOutcome.Failed("La relectura no trajo resultado.");

                // 7: the base of the reread only goes to ConfirmBase.
                var confirm = draft.ConfirmBase(pilot.StaleBase);
                var admission = pilot.Read.IsAdmitted ? null : pilot.Read.Admission.Diagnostic;
                var hint = admission == null ? null : pilot.ClassicEditorHint;
                switch (confirm.Status)
                {
                    case DraftConfirmStatus.Confirmed:
                        // M-5: a confirmed check whose read was not admitted still carries the diagnostic of the admission and the hint of the classic editor.
                        return EditConfirmOutcome.Confirmed(admission, hint);
                    case DraftConfirmStatus.Conflict:
                        // A difference found with a read that was not admitted carries them too, so the panel never takes "gone" or "not editable" for an outside change.
                        _knownClean.Remove(request.Session);
                        return EditConfirmOutcome.Conflict(DescribeDifferences(confirm.Differences), admission, hint);
                    default:
                        return EditConfirmOutcome.NotApplicable(NotCheckableReason);
                }
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.TryConfirmPilotBase", ex);
                return EditConfirmOutcome.Failed(ex.Message);
            }
        }

        /// <summary>
        /// "Keep my work" for one rack of the pilot (D-T4A1-02, D-T4A1-03): it serves a draft in conflict, or a dirty one marked as possibly
        /// stale. A draft that is being applied waits; any other draft is not needed. It rereads once and, with the read admitted, the
        /// draft stays on the current version with its edits and its pending text.
        /// </summary>
        public EditIntentOutcome TryKeepPilotEdit(EditIntentRequest request)
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

                // 1, continued: the same request error as the other operations; nothing is read.
                if (HasWitnessWithoutKey(request))
                    return EditIntentOutcome.Failed("El testigo de la vista previa no tiene clave.");

                // 2 to 5: the same gate; nothing is read.
                var gate = PassGate(request.Session, out var reason);
                if (gate == EditGate.Unavailable) return EditIntentOutcome.Unavailable(reason);
                if (gate == EditGate.Waiting) return EditIntentOutcome.Waiting(reason);
                if (gate == EditGate.Rejected) return EditIntentOutcome.Rejected(reason);

                // 3 and 4: a missing draft is NotFound, a draft that is being applied waits and one that does not need to be kept is not
                // needed; none of them reads anything.
                var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);
                if (draft == null) return EditIntentOutcome.NotFound(NoDraftReason);
                var before = draft.Status;
                if (before == DraftStatus.Applying) return EditIntentOutcome.Waiting("El borrador se está aplicando o ya no está registrado.");
                if (before != DraftStatus.Conflict && !(before == DraftStatus.Dirty && draft.IsPossiblyStale))
                    return EditIntentOutcome.NotNeeded(NotKeepableReason);

                // 6: the one read.
                var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
                switch (read.Status)
                {
                    case EditIntentReadStatus.Read:
                        break;
                    case EditIntentReadStatus.NoActiveDocument:
                    case EditIntentReadStatus.Busy:
                        return EditIntentOutcome.Waiting(read.Reason ?? "El dibujo no está listo para leerse.");
                    case EditIntentReadStatus.Rejected:
                        return EditIntentOutcome.Rejected(read.Reason ?? NotThisDocumentReason);
                    default:
                        return EditIntentOutcome.Failed(read.Reason ?? "Falló la lectura para la recarga.");
                }

                var pilot = read.PilotRead;
                if (pilot == null) return EditIntentOutcome.Failed("La lectura para la recarga no trajo resultado.");
                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);

                // 7: the read only goes to the atomic form of KeepMineOnCurrentVersion, which has been confirmed by the gesture itself.
                var kept = draft.KeepMineOnCurrentVersion(pilot.Read, PushBackPilotMaterializer.KeepPilotEditsOn, confirmed: true);
                switch (kept.Status)
                {
                    case DraftReloadStatus.Reloaded:
                        _knownClean.Remove(request.Session);
                        return EditIntentOutcome.Kept(witness);
                    case DraftReloadStatus.NotAdmitted:
                        return EditIntentOutcome.NotAdmitted(
                            kept.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,
                            pilot.Detail,
                            pilot.ClassicEditorHint,
                            witness);
                    case DraftReloadStatus.NotAllowed:
                        return EditIntentOutcome.Waiting("El borrador se está aplicando o ya no está registrado.");
                    default:
                        return EditIntentOutcome.Failed("El borrador no se recargó.");
                }
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.TryKeepPilotEdit", ex);
                return EditIntentOutcome.Failed(ex.Message);
            }
        }

        /// <summary>
        /// The explicit "discard and reload" of one rack of the pilot (D-T4A1-03): the work of the draft goes away and the draft is replaced
        /// from ONE read. It serves every status but the one of being applied, which waits. A read that is not admitted keeps the draft as it
        /// was.
        /// </summary>
        public EditIntentOutcome TryDiscardAndReloadPilotEdit(EditIntentRequest request)
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

                // 1, continued: the same request error as the other operations; nothing is read.
                if (HasWitnessWithoutKey(request))
                    return EditIntentOutcome.Failed("El testigo de la vista previa no tiene clave.");

                // 2 to 5: the same gate; nothing is read.
                var gate = PassGate(request.Session, out var reason);
                if (gate == EditGate.Unavailable) return EditIntentOutcome.Unavailable(reason);
                if (gate == EditGate.Waiting) return EditIntentOutcome.Waiting(reason);
                if (gate == EditGate.Rejected) return EditIntentOutcome.Rejected(reason);

                // 3 and 4: a missing draft is NotFound and a draft that is being applied waits; none of them reads anything.
                var draft = _drafts.Find<PushBackPilotDraftState>(request.Session, request.RackId);
                if (draft == null) return EditIntentOutcome.NotFound(NoDraftReason);
                if (draft.Status == DraftStatus.Applying) return EditIntentOutcome.Waiting("El borrador se está aplicando o ya no está registrado.");

                // 6: the one read.
                var read = _editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness);
                switch (read.Status)
                {
                    case EditIntentReadStatus.Read:
                        break;
                    case EditIntentReadStatus.NoActiveDocument:
                    case EditIntentReadStatus.Busy:
                        return EditIntentOutcome.Waiting(read.Reason ?? "El dibujo no está listo para leerse.");
                    case EditIntentReadStatus.Rejected:
                        return EditIntentOutcome.Rejected(read.Reason ?? NotThisDocumentReason);
                    default:
                        return EditIntentOutcome.Failed(read.Reason ?? "Falló la lectura para la recarga.");
                }

                var pilot = read.PilotRead;
                if (pilot == null) return EditIntentOutcome.Failed("La lectura para la recarga no trajo resultado.");
                var witness = WorkspaceEditIntent.WitnessStatusOf(pilot.PreviewWitness);

                // 7: the read only goes to DiscardAndReload; the status is the one of the reload of the lazy kind.
                var reloaded = draft.DiscardAndReload(pilot.Read);
                switch (reloaded.Status)
                {
                    case DraftReloadStatus.Reloaded:
                        _knownClean.Remove(request.Session);
                        return EditIntentOutcome.Reloaded(witness);
                    case DraftReloadStatus.NotAdmitted:
                        return EditIntentOutcome.NotAdmitted(
                            reloaded.Admission.Diagnostic ?? PushBackPilotIntentReader.MaterializeDiagnostic,
                            pilot.Detail,
                            pilot.ClassicEditorHint,
                            witness);
                    case DraftReloadStatus.NotAllowed:
                        return EditIntentOutcome.Waiting("El borrador se está aplicando o ya no está registrado.");
                    default:
                        return EditIntentOutcome.Failed("El borrador no se recargó.");
                }
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.TryDiscardAndReloadPilotEdit", ex);
                return EditIntentOutcome.Failed(ex.Message);
            }
        }

        /// <summary>
        /// The explicit discard of every draft of a session (D-T2B2-15), for the command that F4-T3 will expose. The only gate is the
        /// authorization of the session against the active document (INV-22): it needs no reader, no rest, no absence of command and no
        /// absence of modal. It reads nothing, never calls the comparer, and ends the "known clean" mark of the session. Every exception
        /// is contained as Failed and the session is not degraded.
        /// </summary>
        public EditDiscardOutcome DiscardPilotEdits(SessionId session)
        {
            try
            {
                if (Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted)
                    return EditDiscardOutcome.Rejected("La petición no es del documento activo.");

                var discarded = _drafts.DiscardAllByUser(session);
                _knownClean.Remove(session);
                return EditDiscardOutcome.Discarded(discarded.RemovedRackIds, discarded.PendingTextRackIds);
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.DiscardPilotEdits", ex);
                return EditDiscardOutcome.Failed(ex.Message);
            }
        }

        /// <summary>
        /// Tells the host that something changed a draft of the session in place (edit, pending text, update): the "known clean" mark of
        /// the session no longer holds. It has no gate, reads nothing and never throws. Until the panel edits drafts (F4-T3, F4-T4)
        /// nothing calls it.
        /// </summary>
        public void NotePilotEdit(SessionId session) => _knownClean.Remove(session);

        // ---------------------------------------------------------------- the edit port of the pilot (I-64 F4-T3A, D-T3-02)
        //
        // Every operation below is implemented explicitly, so the public surface of the host does not grow. None of them writes the
        // drawing: the three that read go through the operations of F4-T2B2A and F4-T2B2B, the rest only touch memory. Every exception
        // is contained as a value and the session is never degraded. NotePilotEdit is called by every operation that changes a draft.

        // The three reads: the mark of "read in progress" is set with try/finally around each one, and a second read that arrives while
        // it is set answers Waiting without reading.
        EditIntentOutcome IWorkspacePilotEditPort.TryBeginEdit(EditIntentRequest request)
        {
            if (_pilotReadInProgress) return EditIntentOutcome.Waiting(PilotReadInProgressReason);
            _pilotReadInProgress = true;
            try
            {
                return TryBeginPilotEdit(request);
            }
            finally
            {
                _pilotReadInProgress = false;
            }
        }

        EditCheckOutcome IWorkspacePilotEditPort.TryCheckNow(EditIntentRequest request)
        {
            if (_pilotReadInProgress) return EditCheckOutcome.Waiting(PilotReadInProgressReason);
            _pilotReadInProgress = true;
            try
            {
                return TryCheckPilotEdit(request);
            }
            finally
            {
                _pilotReadInProgress = false;
            }
        }

        EditIntentOutcome IWorkspacePilotEditPort.TryReload(EditIntentRequest request)
        {
            if (_pilotReadInProgress) return EditIntentOutcome.Waiting(PilotReadInProgressReason);
            _pilotReadInProgress = true;
            try
            {
                return TryReloadPilotEdit(request);
            }
            finally
            {
                _pilotReadInProgress = false;
            }
        }

        // I-64 F4-T4A1: the three reads of the explicit reconciliation (the general check, keep my work and discard with a reload) go
        // through the same mark of "read in progress", with the same try/finally.
        EditConfirmOutcome IWorkspacePilotEditPort.TryConfirmBase(EditIntentRequest request)
        {
            if (_pilotReadInProgress) return EditConfirmOutcome.Waiting(PilotReadInProgressReason);
            _pilotReadInProgress = true;
            try
            {
                return TryConfirmPilotBase(request);
            }
            finally
            {
                _pilotReadInProgress = false;
            }
        }

        EditIntentOutcome IWorkspacePilotEditPort.TryKeepMine(EditIntentRequest request)
        {
            if (_pilotReadInProgress) return EditIntentOutcome.Waiting(PilotReadInProgressReason);
            _pilotReadInProgress = true;
            try
            {
                return TryKeepPilotEdit(request);
            }
            finally
            {
                _pilotReadInProgress = false;
            }
        }

        EditIntentOutcome IWorkspacePilotEditPort.TryDiscardAndReload(EditIntentRequest request)
        {
            if (_pilotReadInProgress) return EditIntentOutcome.Waiting(PilotReadInProgressReason);
            _pilotReadInProgress = true;
            try
            {
                return TryDiscardAndReloadPilotEdit(request);
            }
            finally
            {
                _pilotReadInProgress = false;
            }
        }

        // Memory only: no authorization, no read, never throws.
        PilotFieldView IWorkspacePilotEditPort.GetField(WorkspaceRequest origin, string rackId)
        {
            try
            {
                var draft = FindPilotDraft(origin.Origin, rackId);
                return draft == null ? default : ViewOfPilotDraft(draft);
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.GetField", ex);
                return new PilotFieldView(true, PilotFieldState.Error, false, true, false, null, "No se pudo leer el estado del campo.", null, null);
            }
        }

        // Only an edit of the user calls it. It keeps the text exactly as typed, with the one key of the adapter of the pilot.
        PilotFieldResult IWorkspacePilotEditPort.SetPendingText(WorkspaceRequest origin, string rackId, string text)
        {
            try
            {
                if (Authorize(origin) != RequestDecision.Accepted) return PilotFieldResult.Rejected(NotThisDocumentReason);
                if (_pilotReadInProgress) return PilotFieldResult.Busy(PilotReadInProgressReason);
                if (text == null) return PilotFieldResult.Rejected("El texto es obligatorio.");

                var draft = FindPilotDraft(origin.Origin, rackId);
                if (draft == null) return PilotFieldResult.NotEditable(NoDraftReason, default);
                var before = ViewOfPilotDraft(draft);
                if (!before.CanEdit) return PilotFieldResult.NotEditable("El campo no se puede editar ahora.", before);

                var edit = draft.SetPendingText(PushBackPilotFieldAdapter.AnnotationScaleKey, text);
                if (edit.Status != DraftEditStatus.Updated) return PilotFieldResult.NotEditable("El campo no se puede editar ahora.", before);

                NotePilotEdit(origin.Origin);
                return PilotFieldResult.Applied(ViewOfPilotDraft(draft));
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.SetPendingText", ex);
                return PilotFieldResult.Rejected(ex.Message);
            }
        }

        // The gesture of the field itself: the same two-phase step as a blocking boundary, run on the draft with the adapter of the pilot.
        PilotFieldResult IWorkspacePilotEditPort.CommitField(WorkspaceRequest origin, string rackId)
        {
            try
            {
                if (Authorize(origin) != RequestDecision.Accepted) return PilotFieldResult.Rejected(NotThisDocumentReason);
                if (_pilotReadInProgress) return PilotFieldResult.Busy(PilotReadInProgressReason);

                var draft = FindPilotDraft(origin.Origin, rackId);
                if (draft == null) return PilotFieldResult.NotEditable(NoDraftReason, default);
                var before = ViewOfPilotDraft(draft);
                if (!before.CanEdit) return PilotFieldResult.NotEditable("El campo no se puede editar ahora.", before);

                var crossed = draft.CrossBoundary(DraftBoundary.TabChange, PilotFieldAdapter);
                switch (crossed.Decision)
                {
                    case DraftBoundaryDecision.Proceeded:
                        if (crossed.CurrentStateChanged) NotePilotEdit(origin.Origin);
                        return PilotFieldResult.Applied(ViewOfPilotDraft(draft));
                    case DraftBoundaryDecision.Aborted:
                        var view = ViewOfPilotDraft(draft);
                        var message = crossed.InvalidFields.Count > 0 ? crossed.InvalidFields[0].Message : view.PendingMessage;
                        return PilotFieldResult.Invalid(view, message);
                    default:
                        return PilotFieldResult.NotEditable("El campo no se puede editar ahora.", ViewOfPilotDraft(draft));
                }
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.CommitField", ex);
                return PilotFieldResult.Rejected(ex.Message);
            }
        }

        PilotBoundaryResult IWorkspacePilotEditPort.CrossBoundary(WorkspaceRequest origin, string rackId, PilotBoundaryKind kind)
        {
            var none = Array.Empty<PilotFieldProblem>();
            try
            {
                switch (kind)
                {
                    case PilotBoundaryKind.TabChange:
                    case PilotBoundaryKind.RackChange:
                        break;
                    case PilotBoundaryKind.DocumentChange:
                    case PilotBoundaryKind.PanelHide:
                    case PilotBoundaryKind.Dematerialize:
                    case PilotBoundaryKind.DocumentClose:
                        // Non-blocking: the session may no longer be the active one, so nothing is authorized, validated or applied.
                        // The pending text of the draft stays exactly as it is.
                        return new PilotBoundaryResult(kind, PilotBoundaryDecision.Kept, false, none, null);
                    default:
                        return new PilotBoundaryResult(kind, PilotBoundaryDecision.Aborted, false, none, "La frontera no se conoce.");
                }

                if (Authorize(origin) != RequestDecision.Accepted)
                    return new PilotBoundaryResult(kind, PilotBoundaryDecision.Aborted, false, none, NotThisDocumentReason);
                if (_pilotReadInProgress)
                    return new PilotBoundaryResult(kind, PilotBoundaryDecision.Busy, false, none, PilotReadInProgressReason);

                var draft = FindPilotDraft(origin.Origin, rackId);
                if (draft == null) return new PilotBoundaryResult(kind, PilotBoundaryDecision.Proceeded, false, none, null);

                var boundary = kind == PilotBoundaryKind.TabChange ? DraftBoundary.TabChange : DraftBoundary.RackChange;
                var crossed = draft.CrossBoundary(boundary, PilotFieldAdapter);
                switch (crossed.Decision)
                {
                    case DraftBoundaryDecision.Proceeded:
                        if (crossed.CurrentStateChanged) NotePilotEdit(origin.Origin);
                        return new PilotBoundaryResult(kind, PilotBoundaryDecision.Proceeded, crossed.CurrentStateChanged, none, null);
                    case DraftBoundaryDecision.Aborted:
                        var problems = crossed.InvalidFields.Select(f => new PilotFieldProblem(f.FieldKey, f.Message)).ToArray();
                        return new PilotBoundaryResult(kind, PilotBoundaryDecision.Aborted, false, problems, null);
                    default:
                        return new PilotBoundaryResult(kind, PilotBoundaryDecision.Kept, false, none, null);
                }
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.CrossBoundary", ex);
                return new PilotBoundaryResult(kind, PilotBoundaryDecision.Aborted, false, none, ex.Message);
            }
        }

        // The explicit discard of ONE draft: no comparer is called, nothing is written, the others are not touched.
        EditDiscardOutcome IWorkspacePilotEditPort.DiscardRack(WorkspaceRequest origin, string rackId)
        {
            try
            {
                if (Authorize(origin) != RequestDecision.Accepted) return EditDiscardOutcome.Rejected(NotThisDocumentReason);
                if (_pilotReadInProgress) return EditDiscardOutcome.Busy(PilotReadInProgressReason);

                var session = origin.Origin;
                if (string.IsNullOrWhiteSpace(rackId)) return EditDiscardOutcome.Discarded(Array.Empty<string>(), Array.Empty<string>());

                var discarded = _drafts.DiscardByUser(session, rackId);
                NotePilotEdit(session);
                if (!SessionStillAtRisk(session)) DropCloseNotice(session);
                return EditDiscardOutcome.Discarded(
                    discarded.Removed ? new[] { rackId } : Array.Empty<string>(),
                    discarded.HadPendingText ? new[] { rackId } : Array.Empty<string>());
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.DiscardRack", ex);
                return EditDiscardOutcome.Failed(ex.Message);
            }
        }

        EditDiscardOutcome IWorkspacePilotEditPort.DiscardDocument(WorkspaceRequest origin)
        {
            try
            {
                if (Authorize(origin) != RequestDecision.Accepted) return EditDiscardOutcome.Rejected(NotThisDocumentReason);
                if (_pilotReadInProgress) return EditDiscardOutcome.Busy(PilotReadInProgressReason);

                var outcome = DiscardPilotEdits(origin.Origin);
                if (outcome.Status == EditDiscardStatus.Discarded) DropCloseNotice(origin.Origin);
                return outcome;
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.DiscardDocument", ex);
                return EditDiscardOutcome.Failed(ex.Message);
            }
        }

        // Memory only: no authorization (the veto of a close can belong to a document that is not the active one), never throws.
        PilotRiskView IWorkspacePilotEditPort.GetRacksAtRisk(WorkspaceRequest origin)
        {
            try
            {
                var session = origin.Origin;
                try
                {
                    return new PilotRiskView(_drafts.RacksAtRiskOf(session), Array.Empty<string>());
                }
                catch (Exception)
                {
                    // A comparer failed: decide draft by draft, and let the ones that cannot be decided count as at risk, apart.
                }

                var atRisk = new List<string>();
                var uncertain = new List<string>();
                foreach (var rackId in _drafts.RackIdsOf(session))
                {
                    var draft = _drafts.Find<PushBackPilotDraftState>(session, rackId);
                    if (draft == null)
                    {
                        uncertain.Add(rackId);
                        continue;
                    }

                    try
                    {
                        if (draft.HasPendingText || draft.IsDirty) atRisk.Add(rackId);
                    }
                    catch (Exception)
                    {
                        uncertain.Add(rackId);
                    }
                }

                return new PilotRiskView(atRisk, uncertain);
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.GetRacksAtRisk", ex);
                return default;
            }
        }

        IReadOnlyList<PilotLossView> IWorkspacePilotEditPort.TakeLossNotices()
        {
            var taken = new List<PilotLossView>();
            while (_lossNotices.Count > 0)
            {
                var notice = _lossNotices.Dequeue();
                try
                {
                    taken.Add(new PilotLossView(notice.Session, notice.LostRackIds, notice.UncertainRackIds, notice.UncertainReasons));
                }
                catch (Exception ex)
                {
                    // The conversion of one notice failed: that notice is lost and stays only in the log; the next one is still taken.
                    Report("WorkspaceHostCore.TakeLossNotices", ex);
                }
            }

            return taken;
        }

        IReadOnlyList<PilotCloseView> IWorkspacePilotEditPort.TakeCloseNotices()
        {
            try
            {
                var taken = _closeNotices.ToArray();
                _closeNotices.Clear();
                return taken;
            }
            catch (Exception ex)
            {
                // The queue is not emptied: it is taken again at the next presentation.
                Report("WorkspaceHostCore.TakeCloseNotices", ex);
                return Array.Empty<PilotCloseView>();
            }
        }

        private DraftSession<PushBackPilotDraftState>? FindPilotDraft(SessionId session, string rackId) =>
            string.IsNullOrWhiteSpace(rackId) ? null : _drafts.Find<PushBackPilotDraftState>(session, rackId);

        // True when the session still has racks at risk, or when that cannot be decided (a comparer failed): then the notice stays.
        private bool SessionStillAtRisk(SessionId session)
        {
            try
            {
                return _drafts.RacksAtRiskOf(session).Count > 0;
            }
            catch (Exception)
            {
                return true;
            }
        }

        private static PilotFieldView ViewOfPilotDraft(DraftSession<PushBackPilotDraftState> draft)
        {
            PilotFieldState state;
            try
            {
                state = StateOfDraft(draft.Status);
            }
            catch (Exception)
            {
                state = PilotFieldState.Undecidable;
            }

            bool isProtected;
            if (state == PilotFieldState.Undecidable)
            {
                isProtected = true;
            }
            else
            {
                try
                {
                    isProtected = draft.IsDirty;
                }
                catch (Exception)
                {
                    isProtected = true;
                }
            }

            var canEdit = draft.IsRegistered
                && (state == PilotFieldState.Clean || state == PilotFieldState.Dirty || state == PilotFieldState.Conflict);

            string? committed = null;
            string? reason;
            var annotations = draft.CurrentDraftState.Inputs.Annotations;
            if (annotations == null) reason = "El rack no tiene escala de anotación.";
            else committed = WorkspacePilotValueText.Show(annotations.AnnotationScale, out reason);

            string? pending = null;
            string? message = null;
            if (draft.PendingText.TryGetValue(PushBackPilotFieldAdapter.AnnotationScaleKey, out var typed))
            {
                pending = typed;
                var verdict = PilotFieldAdapter.Validate(PushBackPilotFieldAdapter.AnnotationScaleKey, typed);
                message = verdict.IsValid ? null : verdict.Message;
            }

            // The reason of the Error travels only in the state Error, exactly as the draft holds it.
            var errorReason = state == PilotFieldState.Error ? draft.ErrorReason : null;

            // I-64 F4-T4A1 (D-T4A1-03): why the work is Unknown (only in that state), what differs (only in Conflict) and the cause of the
            // last attempt, exactly as the draft holds it.
            var unknownOrigin = PilotUnknownOrigin.None;
            if (state == PilotFieldState.Unknown)
            {
                if (draft.UnknownOrigin == DraftUnknownOrigin.CommitThrew) unknownOrigin = PilotUnknownOrigin.CommitThrew;
                else if (draft.UnknownOrigin == DraftUnknownOrigin.PostReread) unknownOrigin = PilotUnknownOrigin.PostReread;
            }

            var conflictText = state == PilotFieldState.Conflict ? string.Join(", ", DescribeDifferences(draft.ConflictDifferences)) : null;
            return new PilotFieldView(
                true, state, draft.IsPossiblyStale, isProtected, canEdit, committed, reason, pending, message, errorReason,
                unknownOrigin, conflictText, draft.LastApplyCause);
        }

        private static PilotFieldState StateOfDraft(DraftStatus status)
        {
            switch (status)
            {
                case DraftStatus.Clean: return PilotFieldState.Clean;
                case DraftStatus.Dirty: return PilotFieldState.Dirty;
                case DraftStatus.Conflict: return PilotFieldState.Conflict;
                case DraftStatus.Applying: return PilotFieldState.Applying;
                case DraftStatus.Unknown: return PilotFieldState.Unknown;
                default: return PilotFieldState.Error;
            }
        }

        // The advice for one attempt to close the document of the session (D-T2B2-09, D-T2B2-10): the verdict of the close policy,
        // and nothing else. It reads nothing, authorizes nothing, discards nothing and retires nothing. A Veto or a failure leaves the
        // session without the mark; an Allow puts it as the very last action, only when the evaluation worked.
        // Since F4-T3A (D-T3-07) every way to a Veto also queues, in memory, the notice of the veto for the panel: the racks at risk of the
        // decision, or the uncertain flag without a list when the evaluation failed. An Allow takes the notice of the session away.
        CloseDecision IWorkspaceLifecycleAdvisor.AdviseClose(SessionId session)
        {
            _knownClean.Remove(session);
            try
            {
                var decision = _closePolicy.EvaluateClose(session);
                if (decision.IsVeto)
                {
                    QueueCloseNotice(session, decision.RackIds, false);
                    return CloseDecision.Veto;
                }

                DropCloseNotice(session);
                _knownClean.Add(session);
                return CloseDecision.Allow;
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.AdviseClose", ex);
                QueueCloseNotice(session, Array.Empty<string>(), true);
                return CloseDecision.Veto;
            }
        }

        private void QueueCloseNotice(SessionId session, IReadOnlyList<string> rackIds, bool uncertain)
        {
            DropCloseNotice(session);
            _closeNotices.Add(new PilotCloseView(session, rackIds, uncertain));
        }

        private void DropCloseNotice(SessionId session) => _closeNotices.RemoveAll(notice => notice.Session == session);

        // The session was destroyed (D-T2B2-10, D-T2B2-13). It always retires the drafts: without the comparer only when the mark of the
        // host and the intact close sequence of the bridge both hold, conservatively in any other case. Afterwards, and only when work
        // was lost, the notice is queued and logged on the spot. The retirement and the notice are contained apart. The notice of a vetoed
        // close of the session is dropped inside the first try, after the retirement: if the retirement threw, the veto notice stays and
        // the panel purges it when its session no longer lives.
        void IWorkspaceLifecycleAdvisor.SessionDestroyed(SessionId session, bool closeSequenceIntact)
        {
            var marked = _knownClean.Remove(session);
            DraftLossNotice? notice = null;
            try
            {
                notice = marked && closeSequenceIntact ? _drafts.RetireKnownSafe(session) : _drafts.RetireConservative(session);
                DropCloseNotice(session);
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.SessionDestroyed", ex);
            }

            try
            {
                if (notice == null || !notice.HasLoss) return;
                _lossNotices.Enqueue(notice);
                Report("WorkspaceHostCore.DraftLoss", new InvalidOperationException(DescribeLoss(notice)));
            }
            catch (Exception ex)
            {
                Report("WorkspaceHostCore.SessionDestroyed", ex);
            }
        }

        /// <summary>The notices of lost work that nobody consumed yet, oldest first. F4-T3 consumes them; until then nothing does.</summary>
        internal IReadOnlyCollection<DraftLossNotice> LossNotices => _lossNotices;

        // The text of the record: the racks lost and, apart, the uncertain ones with the reason that the comparer gave.
        private static string DescribeLoss(DraftLossNotice notice)
        {
            var text = "Panel drafts were lost when the document closed. Session " + notice.Session.Value
                       + ". Lost racks: " + string.Join(", ", notice.LostRackIds) + ".";
            if (notice.UncertainRackIds.Count > 0)
            {
                text += " Uncertain racks (the comparison failed, so they may not have been modified): "
                        + string.Join("; ", notice.UncertainRackIds.Select(id => id + " (" + ReasonOf(notice, id) + ")")) + ".";
            }

            return text;
        }

        private static string ReasonOf(DraftLossNotice notice, string rackId) =>
            notice.UncertainReasons.TryGetValue(rackId, out var reason) ? reason : string.Empty;

        // The stale hint of a drain: every draft of the pilot of the session is marked. Nothing is read and no conflict is raised.
        private void MarkPilotDraftsPossiblyStale(SessionId session)
        {
            foreach (var rackId in _drafts.RackIdsOf(session))
                _drafts.Find<PushBackPilotDraftState>(session, rackId)?.MarkPossiblyStale();
        }

        // A witness that is present (not null) with a null, empty or blank key cannot be compared with anything (INV-27).
        private static bool HasWitnessWithoutKey(EditIntentRequest request) =>
            request.Witness != null && string.IsNullOrWhiteSpace(request.Witness.Key);

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
                reason = "No hay un lector de la intención de edición asignado.";
                return EditGate.Unavailable;
            }

            if (!_probe.ActiveDocumentNativeId.HasValue)
            {
                reason = "No hay documento activo.";
                return EditGate.Waiting;
            }

            if (_modalRackCadActive)
            {
                reason = "Hay una ventana de RackCad abierta.";
                return EditGate.Waiting;
            }

            if (!_probe.IsActiveDocumentQuiescent)
            {
                reason = "Hay un comando en curso.";
                return EditGate.Waiting;
            }

            if (Authorize(new WorkspaceRequest(session)) != RequestDecision.Accepted)
            {
                reason = "La petición no es del documento activo.";
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
