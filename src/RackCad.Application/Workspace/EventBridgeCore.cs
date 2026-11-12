#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Pure policy of the central event bridge (I-64 D-04). It decides WHICH event sources are attached
    /// (through <see cref="IHostEventSource"/>), turns notifications into hints on the session they belong to, and
    /// decides WHEN a drain may run. It knows nothing of the host: no read, no write, no selection, no view,
    /// no persistence.
    ///
    /// Rules it enforces:
    /// - nothing is attached before <see cref="Start"/> (first panel open); after it, the document-collection sources
    ///   once each, and for every live document session each per-session source exactly once;
    /// - ordinary notification handlers only enqueue a hint on their own session (identity travels with the
    ///   notification) or flip in-memory state; they never drain, read or write, and never throw;
    /// - a drain happens only in <see cref="OnIdle"/>, for the active session, when it is quiescent, no RackCad modal
    ///   is open and the panel is visible; N hints coalesce into one drain;
    /// - the idle source is attached only while some synchronized session has pending hints;
    /// - a fault degrades only the affected session: its synchronization sources are detached, BeginDocumentClose stays
    ///   attached for the whole life of the session;
    /// - destroying a document retires its session and detaches every source of it.
    /// </summary>
    public sealed class EventBridgeCore
    {
        private sealed class SessionState
        {
            public SessionState(long nativeInstanceId)
            {
                NativeInstanceId = nativeInstanceId;
            }

            public long NativeInstanceId { get; }

            public bool Quiescent { get; set; }

            public bool Degraded { get; set; }

            public List<SourceAttachment> Attached { get; } = new();
        }

        private static readonly SourceAttachment IdleAttachment = SourceAttachment.Global(BridgeSource.Idle);

        private readonly IHostEventSource _source;
        private readonly Action<string, Exception>? _logFault;
        private readonly WorkspaceSessionRegistry _registry = new();
        private readonly Dictionary<SessionId, SessionState> _states = new();
        private readonly Dictionary<long, SessionId> _byNative = new();
        private readonly List<SourceAttachment> _globalAttached = new();
        private bool _started;
        private bool _idleAttached;
        private SessionId? _active;
        private bool _panelVisible;
        private bool _modalRackCadActive;
        private IWorkspaceLifecycleAdvisor? _advisor;

        // The close sequence as the core observed it (I-64 F4-T2B2B, D-T2B2-10): the session whose close the advisor just allowed,
        // and nothing else. It is forgotten (assigned null) by every other input that changes the state of the core.
        private SessionId? _lastAllowedClose;

        public EventBridgeCore(IHostEventSource source, Action<string, Exception>? logFault = null)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _logFault = logFault;
        }

        public bool IsStarted => _started;

        public bool IsIdleAttached => _idleAttached;

        public int LiveSessionCount => _registry.LiveSessionCount;

        /// <summary>The session of the active document, or none when there is no open document.</summary>
        public SessionId? ActiveSession => _active;

        /// <summary>
        /// Whether the active session is running an AutoCAD command, as this core last learned it in memory (I-64
        /// F1-T3-HOST-C2): from CommandWillStart until EnteringQuiescentState, or since it was activated while not at rest.
        /// It reads nothing of the host. It is false before <see cref="Start"/>, without an active document and for a
        /// degraded session, whose synchronization sources are detached and whose last state may be stale.
        /// </summary>
        public bool IsCommandActiveInActiveSession =>
            _started
            && _active is { } id
            && _states.TryGetValue(id, out var state)
            && !state.Degraded
            && !state.Quiescent;

        /// <summary>Snapshot of every attachment this core currently holds.</summary>
        public IReadOnlyCollection<SourceAttachment> AttachedSources
        {
            get
            {
                var all = new List<SourceAttachment>(_globalAttached);
                foreach (var state in _states.Values) all.AddRange(state.Attached);
                if (_idleAttached) all.Add(IdleAttachment);
                return all;
            }
        }

        public bool TryGetSession(SessionId id, out WorkspaceSession? session) => _registry.TryGet(id, out session);

        public bool TryGetSession(long nativeInstanceId, out WorkspaceSession? session)
        {
            session = null;
            return _byNative.TryGetValue(nativeInstanceId, out var id) && _registry.TryGet(id, out session);
        }

        /// <summary>
        /// Checks a request against the session of the document that would execute it (D-03, INV-22). It is the verdict of the
        /// one registry this core owns; it has no side effects.
        /// </summary>
        public RequestDecision Authorize(WorkspaceRequest request, long executingNativeInstanceId) =>
            _registry.Authorize(request, executingNativeInstanceId);

        /// <summary>
        /// The panel lifecycle asks for a reconciliation (first open, show, refresh). It queues the selection hint on the
        /// ACTIVE session and lets the idle policy attach the idle source; the drain happens later, at rest, through the
        /// normal path. It never drains, never reads or writes, never applies a context, never creates a session and never
        /// attaches anything but the idle source. It does nothing when there is no active session or when that session is
        /// degraded or retired.
        /// </summary>
        public void RequestReconciliation()
        {
            if (!_started) return;
            _lastAllowedClose = null;
            try
            {
                if (_active is not { } id) return;
                if (!_states.TryGetValue(id, out var state) || state.Degraded) return;
                if (!_registry.TryGet(id, out var session) || session == null) return;

                session.EnqueueHint(HintKind.ImpliedSelectionChanged);
                SyncIdle();
            }
            catch (Exception ex)
            {
                Fault(_active, "RequestReconciliation", ex);
            }
        }

        public bool IsDegraded(SessionId id) => _states.TryGetValue(id, out var state) && state.Degraded;

        /// <summary>Panel visibility as the host reports it. Only the drain conditions use it; it never changes an attachment.</summary>
        public void SetPanelVisible(bool visible)
        {
            _lastAllowedClose = null;
            _panelVisible = visible;
        }

        /// <summary>Whether a RackCad modal is open, as the host reports it. While true, nothing drains.</summary>
        public void SetModalRackCadActive(bool active)
        {
            _lastAllowedClose = null;
            _modalRackCadActive = active;
        }

        /// <summary>
        /// Assigns the advisor of the lifecycle (I-64 F4-T2B2B, D-T2B2-09). It is wired by assignment and never by constructor; a second
        /// call replaces the advisor. It is a query-like setup call: it does not forget the close sequence.
        /// </summary>
        public void SetLifecycleAdvisor(IWorkspaceLifecycleAdvisor advisor) =>
            _advisor = advisor ?? throw new ArgumentNullException(nameof(advisor));

        /// <summary>
        /// First panel open: attaches the document-collection sources once each. Repeating it changes nothing.
        /// </summary>
        public void Start()
        {
            _lastAllowedClose = null;
            _started = true;
            _panelVisible = true;
            foreach (var source in BridgeSources.DocumentCollection)
            {
                var attachment = SourceAttachment.Global(source);
                if (_globalAttached.Contains(attachment)) continue;
                try
                {
                    _source.Attach(attachment);
                    _globalAttached.Add(attachment);
                }
                catch (Exception ex)
                {
                    Log("Start." + source, ex);
                }
            }
        }

        /// <summary>Releases every attachment and every session. After it the core is back to its initial, subscription-free state.</summary>
        public void Stop()
        {
            _lastAllowedClose = null;
            foreach (var pair in _states.ToList())
            {
                foreach (var attachment in pair.Value.Attached.ToList()) DetachSafe(attachment);
                _registry.Destroy(pair.Key);
            }

            foreach (var attachment in _globalAttached.ToList()) DetachSafe(attachment);
            if (_idleAttached) DetachSafe(IdleAttachment);

            _states.Clear();
            _byNative.Clear();
            _globalAttached.Clear();
            _idleAttached = false;
            _active = null;
            _started = false;
        }

        /// <summary>A created document gets no session until it is activated (D-03: sessions are lazy). Nothing is attached.</summary>
        public void OnDocumentCreated(long nativeInstanceId)
        {
            // The lazy session policy needs no work here and nothing may fail; the input only ends the observed close sequence.
            _lastAllowedClose = null;
        }

        /// <summary>
        /// A document became active. Its session is created on first activation and the per-session sources are
        /// attached then; later activations (A to B to A) attach nothing.
        /// </summary>
        public void OnDocumentActivated(long nativeInstanceId, bool isQuiescent)
        {
            if (!_started) return;
            _lastAllowedClose = null;
            SessionId? known = null;
            try
            {
                var session = _registry.GetOrCreate(nativeInstanceId);
                known = session.Id;
                if (!_states.TryGetValue(session.Id, out var state))
                {
                    state = new SessionState(nativeInstanceId);
                    _states.Add(session.Id, state);
                    _byNative[nativeInstanceId] = session.Id;
                    AttachSessionSources(session.Id, state);
                }

                state.Quiescent = isQuiescent;
                _active = session.Id;
                SyncIdle();
            }
            catch (Exception ex)
            {
                Fault(known, "OnDocumentActivated", ex);
            }
        }

        /// <summary>
        /// The host activated something that is not a document, such as the Start tab: AutoCAD raises the activation with no
        /// document (I-64 F1-T3-HOST-C2). From now on there is no active document. Every session stays alive with its hookups,
        /// its hints and its context; nothing is attached, detached, queued, drained or read. A later activation of a document
        /// makes that document's session active again.
        /// </summary>
        public void OnNoActiveDocument()
        {
            if (!_started) return;
            _lastAllowedClose = null;
            _active = null;
        }

        /// <summary>
        /// The document is about to be destroyed: its session is retired and every source of it is detached,
        /// BeginDocumentClose included. The retired id is never paired with a later document, even if the native
        /// identifier is recycled.
        /// </summary>
        public void OnDocumentToBeDestroyed(long nativeInstanceId)
        {
            if (!_started) return;
            var allowedBefore = _lastAllowedClose;
            _lastAllowedClose = null;
            try
            {
                if (!_byNative.TryGetValue(nativeInstanceId, out var id)) return;
                _byNative.Remove(nativeInstanceId);

                // The close sequence is intact only for the session whose close was just allowed and that is not degraded; the
                // degradation is read BEFORE the state of the session goes away.
                var sequenceIntact = allowedBefore == id && !IsDegraded(id);
                if (_states.TryGetValue(id, out var state))
                {
                    foreach (var attachment in state.Attached.ToList()) DetachSafe(attachment);
                    _states.Remove(id);
                }

                _registry.Destroy(id);
                if (_active == id) _active = null;
                TellAdvisorTheSessionEnded(id, sequenceIntact);
                TrySyncIdle();
            }
            catch (Exception ex)
            {
                Log("OnDocumentToBeDestroyed", ex);
            }
        }

        /// <summary>
        /// Ordinary notification handler. It only enqueues a hint on the session named by <paramref name="session"/>
        /// (or flips the in-memory quiescence flag). It reads nothing, writes nothing and never throws.
        /// A notification for a retired or degraded session is dropped.
        /// It returns true only when the notification changed the command state of a live, healthy session (CommandWillStart
        /// at rest, EnteringQuiescentState during a command): the presentation of the panel has to be pulled again (I-64
        /// F1-T3-HOST-C2). That answer is the whole change: those two sources still queue no hint and attach no idle source.
        /// </summary>
        public bool OnSourceEvent(SessionId session, BridgeSource source)
        {
            if (!_started) return false;
            _lastAllowedClose = null;
            try
            {
                if (!_states.TryGetValue(session, out var state) || state.Degraded) return false;
                if (!_registry.TryGet(session, out var live) || live == null) return false;

                switch (source)
                {
                    case BridgeSource.CommandWillStart:
                        return SetQuiescent(state, false);
                    case BridgeSource.EnteringQuiescentState:
                        return SetQuiescent(state, true);
                    case BridgeSource.ImpliedSelectionChanged:
                        live.EnqueueHint(HintKind.ImpliedSelectionChanged);
                        break;
                    case BridgeSource.CommandEnded:
                    case BridgeSource.CommandCancelled:
                    case BridgeSource.CommandFailed:
                        live.EnqueueHint(HintKind.CommandEnded);
                        break;
                    case BridgeSource.ObjectAppended:
                    case BridgeSource.ObjectErased:
                    case BridgeSource.ObjectModified:
                    case BridgeSource.ObjectUnappended:
                    case BridgeSource.ObjectReappended:
                        live.EnqueueHint(HintKind.DatabaseObjectChanged);
                        break;
                    default:
                        return false;
                }

                SyncIdle();
                return false;
            }
            catch (Exception ex)
            {
                Fault(session, "OnSourceEvent." + source, ex);
                return false;
            }
        }

        /// <summary>
        /// Close-begin handler. It lives as long as the session, degraded or not, and decides only from in-memory
        /// state: it reads no stored data, resolves nothing, writes nothing and opens no dialog. Without an advisor it allows. With
        /// one, the decision is the one of the advisor, and any exception of the advisor is a Veto: it is logged, never degrades the
        /// session and never reaches the host. An allowed close is remembered (one session only) so that the destruction can tell
        /// whether anything happened in between; the input itself first forgets whatever was remembered.
        /// </summary>
        public CloseDecision OnBeginDocumentClose(SessionId session)
        {
            _lastAllowedClose = null;
            var advisor = _advisor;
            if (advisor == null) return CloseDecision.Allow;

            var decision = CloseDecision.Veto;
            try
            {
                decision = advisor.AdviseClose(session) == CloseDecision.Allow ? CloseDecision.Allow : CloseDecision.Veto;
            }
            catch (Exception ex)
            {
                Log("OnBeginDocumentClose", ex);
            }

            if (decision == CloseDecision.Allow) _lastAllowedClose = session;
            return decision;
        }

        /// <summary>
        /// The idle pass. The only place a drain can run: for the active session, when it is quiescent, no RackCad modal
        /// is open and the panel is visible. N pending hints become one drain. Afterwards the idle source is
        /// detached if no synchronized session has pending hints left.
        /// </summary>
        public IReadOnlyList<DrainOutcome> OnIdle()
        {
            var outcomes = new List<DrainOutcome>();
            if (!_started) return outcomes;
            _lastAllowedClose = null;
            try
            {
                if (TryGetDrainable(out var id, out var state, out var session))
                {
                    var result = session!.TryDrain(new DrainConditions(state!.Quiescent, _modalRackCadActive, _panelVisible));
                    if (result.Drained) outcomes.Add(new DrainOutcome(id, result));
                }

                SyncIdle();
            }
            catch (Exception ex)
            {
                Fault(_active, "OnIdle", ex);
            }

            return outcomes;
        }

        /// <summary>
        /// Whether the idle pass could drain right now, answered in memory (I-64 F4-T2C, IC-04): the scheduler of the host adapter asks
        /// it before it schedules a continuation and again when the continuation is about to run. It is the same eligibility the pass
        /// uses (started, active session alive and healthy, pending hints, at rest, no RackCad modal, panel visible) and one more
        /// condition of its own: it is false while ANY allowed close is pending, of the active document or of another one, so that
        /// a continuation pumped inside the question of the close never reads the document that is closing and never ends the close
        /// sequence (F4-T2B2B). It is a query: it changes nothing, forgets nothing and never throws.
        /// </summary>
        public bool CanDrainNow()
        {
            try
            {
                if (!TryGetDrainable(out _, out _, out _)) return false;
                return !_lastAllowedClose.HasValue;
            }
            catch (Exception ex)
            {
                Log("CanDrainNow", ex);
                return false;
            }
        }

        /// <summary>
        /// A fault of the bridge in the host adapter. It is logged and degrades only the named session.
        /// </summary>
        public void ReportFault(SessionId session, string context, Exception exception)
        {
            _lastAllowedClose = null;
            Fault(session, context, exception);
        }

        // Flips the in-memory command state of a session and tells whether it changed. No hint, no idle, no host call.
        private static bool SetQuiescent(SessionState state, bool quiescent)
        {
            if (state.Quiescent == quiescent) return false;
            state.Quiescent = quiescent;
            return true;
        }

        // The advisor learns, synchronously and in memory, that a session is gone. It runs after the session left the registry and
        // the active slot; its own failure is logged and never stops the rest of the destruction.
        private void TellAdvisorTheSessionEnded(SessionId id, bool closeSequenceIntact)
        {
            var advisor = _advisor;
            if (advisor == null) return;
            try
            {
                advisor.SessionDestroyed(id, closeSequenceIntact);
            }
            catch (Exception ex)
            {
                Log("OnDocumentToBeDestroyed.Advisor", ex);
            }
        }

        private void AttachSessionSources(SessionId id, SessionState state)
        {
            foreach (var source in BridgeSources.PerSession)
            {
                var attachment = new SourceAttachment(source, id, state.NativeInstanceId);
                try
                {
                    _source.Attach(attachment);
                    state.Attached.Add(attachment);
                }
                catch (Exception ex)
                {
                    Fault(id, "Attach." + source, ex);
                    return;
                }
            }
        }

        private void Degrade(SessionId id)
        {
            if (!_states.TryGetValue(id, out var state) || state.Degraded) return;
            state.Degraded = true;
            foreach (var attachment in state.Attached.Where(a => BridgeSources.IsSynchronization(a.Source)).ToList())
            {
                DetachSafe(attachment);
                state.Attached.Remove(attachment);
            }

            TrySyncIdle();
        }

        private void Fault(SessionId? session, string context, Exception exception)
        {
            Log(context, exception);
            if (session is { } id) Degrade(id);
        }

        // The eligibility of the idle pass, shared by OnIdle and CanDrainNow: a started core whose active session lives in the registry
        // and is not degraded, with pending hints, at rest, no RackCad modal and a visible panel. It only reads.
        private bool TryGetDrainable(out SessionId id, out SessionState? state, out WorkspaceSession? session)
        {
            id = default;
            state = null;
            session = null;
            if (!_started) return false;
            if (_active is not { } active) return false;
            if (!_states.TryGetValue(active, out var found) || found.Degraded) return false;
            if (!_registry.TryGet(active, out var live) || live == null) return false;
            if (live.PendingHintCount == 0) return false;
            if (!found.Quiescent || _modalRackCadActive || !_panelVisible) return false;

            id = active;
            state = found;
            session = live;
            return true;
        }

        private bool IdleNeeded() =>
            _states.Any(pair => !pair.Value.Degraded
                                && _registry.TryGet(pair.Key, out var session)
                                && session != null
                                && session.PendingHintCount > 0);

        private void SyncIdle()
        {
            var needed = IdleNeeded();
            if (needed && !_idleAttached)
            {
                _source.Attach(IdleAttachment);
                _idleAttached = true;
            }
            else if (!needed && _idleAttached)
            {
                _idleAttached = false;
                DetachSafe(IdleAttachment);
            }
        }

        private void TrySyncIdle()
        {
            try
            {
                SyncIdle();
            }
            catch (Exception ex)
            {
                Log("SyncIdle", ex);
            }
        }

        private void DetachSafe(SourceAttachment attachment)
        {
            try
            {
                _source.Detach(attachment);
            }
            catch (Exception ex)
            {
                Log("Detach." + attachment.Source, ex);
            }
        }

        private void Log(string context, Exception exception)
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
    }
}
