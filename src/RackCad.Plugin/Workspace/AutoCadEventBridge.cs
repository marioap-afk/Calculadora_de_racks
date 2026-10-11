using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using RackCad.Application.Diagnostics;
using RackCad.Application.Workspace;
using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// The one and only subscriber to AutoCAD events for the workspace panel (I-64 D-04, INV-02). It owns every
    /// <c>+=</c> / <c>-=</c> on AutoCAD events and nothing else: which sources are hooked, and when, is decided by the
    /// pure <see cref="EventBridgeCore"/>; this class only performs one native hookup per attachment it is asked for.
    ///
    /// Nothing is hooked at construction or at plugin load: the first hookup happens in <see cref="Start"/>, which the
    /// host of the panel calls when the panel is first opened. Ordinary handlers only forward the notification to the
    /// core (which enqueues a hint on the right session); they open no transaction, lock no document, read no stored
    /// data, ask for nothing, write no selection or view, and never let an exception reach AutoCAD.
    /// The class keeps no document, no transaction and no database object: a per-document hookup finds its document
    /// only while attaching and keeps just the delegate that undoes the hookup.
    /// </summary>
    public sealed class AutoCadEventBridge : IHostEventSource, IDisposable
    {
        private readonly EventBridgeCore _core;
        private readonly Dictionary<SourceAttachment, Action> _detachers = new();

        public AutoCadEventBridge()
        {
            _core = new EventBridgeCore(this, LogFault);
        }

        /// <summary>Raised from the idle pass with the pure result of one drain. The drain performs no read in this bridge.</summary>
        public event Action<DrainOutcome> Drained;

        /// <summary>RED skeleton: declared, not raised yet.</summary>
        internal event Action ActiveSessionChanged;

        /// <summary>First panel open: hooks the document-collection sources and the active document. Repeating it adds nothing.</summary>
        public void Start()
        {
            try
            {
                _core.Start();
                var active = AcApplication.DocumentManager.MdiActiveDocument;
                if (active != null) _core.OnDocumentActivated(NativeId(active), active.Editor.IsQuiescent);
            }
            catch (System.Exception ex)
            {
                LogFault("AutoCadEventBridge.Start", ex);
            }
        }

        // Thin facades over the single core (I-64 F1-T3A). They add no state, no subscription and no read of the drawing.

        /// <summary>The session of the active document, or none when no document is open.</summary>
        public SessionId? ActiveSession => _core.ActiveSession;

        public bool TryGetSession(SessionId id, out WorkspaceSession session)
        {
            var found = _core.TryGetSession(id, out var live);
            session = live;
            return found;
        }

        public RequestDecision Authorize(WorkspaceRequest request, long executingNativeInstanceId) =>
            _core.Authorize(request, executingNativeInstanceId);

        /// <summary>Queues the reconciliation hint of the active session; the drain happens later, at rest.</summary>
        public void RequestReconciliation() => _core.RequestReconciliation();

        public void SetPanelVisible(bool visible) => _core.SetPanelVisible(visible);

        public void SetModalRackCadActive(bool active) => _core.SetModalRackCadActive(active);

        /// <summary>Removes every hookup. The bridge is then back to zero subscriptions.</summary>
        public void Dispose()
        {
            try
            {
                _core.Stop();
            }
            catch (System.Exception ex)
            {
                LogFault("AutoCadEventBridge.Dispose", ex);
            }
        }

        void IHostEventSource.Attach(SourceAttachment attachment)
        {
            if (_detachers.ContainsKey(attachment)) return;
            _detachers.Add(attachment, Hook(attachment));
        }

        void IHostEventSource.Detach(SourceAttachment attachment)
        {
            if (!_detachers.Remove(attachment, out var detach)) return;
            detach();
        }

        private Action Hook(SourceAttachment a)
        {
            switch (a.Source)
            {
                case BridgeSource.DocumentCreated:
                {
                    var documents = AcApplication.DocumentManager;
                    DocumentCollectionEventHandler handler = (s, e) => Contain("DocumentCreated", () => _core.OnDocumentCreated(NativeId(e.Document)));
                    documents.DocumentCreated += handler;
                    return () => documents.DocumentCreated -= handler;
                }
                case BridgeSource.DocumentActivated:
                {
                    var documents = AcApplication.DocumentManager;
                    DocumentCollectionEventHandler handler = (s, e) => Contain("DocumentActivated", () => _core.OnDocumentActivated(NativeId(e.Document), e.Document.Editor.IsQuiescent));
                    documents.DocumentActivated += handler;
                    return () => documents.DocumentActivated -= handler;
                }
                case BridgeSource.DocumentToBeDestroyed:
                {
                    var documents = AcApplication.DocumentManager;
                    DocumentCollectionEventHandler handler = (s, e) => Contain("DocumentToBeDestroyed", () => _core.OnDocumentToBeDestroyed(NativeId(e.Document)));
                    documents.DocumentToBeDestroyed += handler;
                    return () => documents.DocumentToBeDestroyed -= handler;
                }
                case BridgeSource.BeginDocumentClose:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    DocumentBeginCloseEventHandler handler = (s, e) => ContainSession(a, () => _core.OnBeginDocumentClose(a.Session));
                    document.BeginDocumentClose += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.BeginDocumentClose -= handler);
                }
                case BridgeSource.ImpliedSelectionChanged:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    EventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));
                    document.ImpliedSelectionChanged += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.ImpliedSelectionChanged -= handler);
                }
                case BridgeSource.CommandWillStart:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    CommandEventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));
                    document.CommandWillStart += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.CommandWillStart -= handler);
                }
                case BridgeSource.CommandEnded:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    CommandEventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));
                    document.CommandEnded += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.CommandEnded -= handler);
                }
                case BridgeSource.CommandCancelled:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    CommandEventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));
                    document.CommandCancelled += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.CommandCancelled -= handler);
                }
                case BridgeSource.CommandFailed:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    CommandEventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));
                    document.CommandFailed += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.CommandFailed -= handler);
                }
                case BridgeSource.EnteringQuiescentState:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    EventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));
                    document.Editor.EnteringQuiescentState += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.Editor.EnteringQuiescentState -= handler);
                }
                case BridgeSource.ObjectAppended:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    ObjectEventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));
                    document.Database.ObjectAppended += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.Database.ObjectAppended -= handler);
                }
                case BridgeSource.ObjectErased:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    ObjectErasedEventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));
                    document.Database.ObjectErased += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.Database.ObjectErased -= handler);
                }
                case BridgeSource.ObjectModified:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    ObjectEventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));
                    document.Database.ObjectModified += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.Database.ObjectModified -= handler);
                }
                case BridgeSource.ObjectUnappended:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    ObjectEventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));
                    document.Database.ObjectUnappended += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.Database.ObjectUnappended -= handler);
                }
                case BridgeSource.ObjectReappended:
                {
                    var document = FindDocument(a.NativeInstanceId);
                    ObjectEventHandler handler = (s, e) => ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));
                    document.Database.ObjectReappended += handler;
                    return () => WithDocument(a.NativeInstanceId, d => d.Database.ObjectReappended -= handler);
                }
                case BridgeSource.Idle:
                {
                    EventHandler handler = (s, e) => Contain("Idle", OnIdle);
                    AcApplication.Idle += handler;
                    return () => AcApplication.Idle -= handler;
                }
                default:
                    throw new InvalidOperationException("Unknown bridge source: " + a.Source);
            }
        }

        private void OnIdle()
        {
            foreach (var outcome in _core.OnIdle())
            {
                try
                {
                    Drained?.Invoke(outcome);
                }
                catch (System.Exception ex)
                {
                    LogFault("AutoCadEventBridge.Drained", ex);
                }
            }
        }

        private static long NativeId(Document document) => document.UnmanagedObject.ToInt64();

        private static Document FindDocumentOrNull(long nativeInstanceId)
        {
            foreach (Document candidate in AcApplication.DocumentManager)
            {
                if (NativeId(candidate) == nativeInstanceId) return candidate;
            }

            return null;
        }

        private static Document FindDocument(long nativeInstanceId) =>
            FindDocumentOrNull(nativeInstanceId)
            ?? throw new InvalidOperationException("The document of the session is no longer open.");

        // Undoing a hookup looks the document up again: if it is already gone its hookups died with it.
        private static void WithDocument(long nativeInstanceId, Action<Document> undo)
        {
            var document = FindDocumentOrNull(nativeInstanceId);
            if (document != null) undo(document);
        }

        // An exception in a handler is logged and never propagates to AutoCAD.
        private void Contain(string context, Action body)
        {
            try
            {
                body();
            }
            catch (System.Exception ex)
            {
                LogFault("AutoCadEventBridge." + context, ex);
            }
        }

        // Same, and the session the handler belongs to goes to degraded mode (the other sessions are untouched).
        private void ContainSession(SourceAttachment attachment, Action body)
        {
            try
            {
                body();
            }
            catch (System.Exception ex)
            {
                try
                {
                    _core.ReportFault(attachment.Session, "AutoCadEventBridge." + attachment.Source, ex);
                }
                catch (System.Exception inner)
                {
                    LogFault("AutoCadEventBridge.ReportFault", inner);
                }
            }
        }

        private static void LogFault(string context, System.Exception exception) => RackLog.Exception(context, exception);
    }
}
