#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>
    /// State of the panel for ONE open document instance, for its whole life (D-03). It is in-memory only:
    /// it persists nothing and writes nothing to the DWG.
    /// </summary>
    public sealed class WorkspaceSession
    {
        internal WorkspaceSession(SessionId id, long nativeInstanceId)
        {
            Id = id;
            NativeInstanceId = nativeInstanceId;
        }

        public SessionId Id { get; }

        /// <summary>Opaque native identity of the document instance; may be recycled by the host after destruction.</summary>
        public long NativeInstanceId { get; }

        public bool IsDestroyed { get; internal set; }

        public SelectionContext Selection { get; private set; } = SelectionContext.None;

        // Pending hints are coalesced by kind: N hints of one kind are a single pending flag (constant cost).
        private readonly System.Collections.Generic.HashSet<HintKind> _pending = new();

        // I-64 F4-T4C1-HOST (D-T4C-09, IC-09): the hints that arrive while the own mutation of the update is open are kept apart, also
        // coalesced by kind. They only invalidate; the foreign ones in _pending keep their whole effect.
        private readonly System.Collections.Generic.HashSet<HintKind> _ownPending = new();

        /// <summary>Number of distinct pending hint kinds, the own ones included; zero when nothing awaits a drain.</summary>
        public int PendingHintCount => _pending.Count + _ownPending.Count;

        /// <summary>
        /// How many changes of the database of the drawing the session received (an object appended, erased, modified, unappended or
        /// reappended; the bookkeeping of the associative framework is not one), own or not, in memory (I-64 T6 st). It only counts: it
        /// reads nothing and drains nothing. The RackCad window compares it around a command to tell whether that command changed the
        /// drawing (the Owner, 2026-11-27: only the commands that change the drawing make a reading possibly out of date).
        /// </summary>
        public long DrawingChangeCount { get; private set; }

        /// <summary>Queues a hint. Constant cost, no reads. False when the session is destroyed.</summary>
        public bool EnqueueHint(HintKind kind)
        {
            if (IsDestroyed) return false;
            _pending.Add(kind);
            if (kind == HintKind.DatabaseObjectChanged) DrawingChangeCount++; // T6 st
            return true;
        }

        /// <summary>Queues a hint that the own mutation of the update caused. Constant cost, no reads. False when the session is destroyed.</summary>
        public bool EnqueueOwnHint(HintKind kind)
        {
            if (IsDestroyed) return false;
            _ownPending.Add(kind);
            if (kind == HintKind.DatabaseObjectChanged) DrawingChangeCount++; // T6 st
            return true;
        }

        /// <summary>Drains pending hints only when the conditions allow it; coalesces N hints into one drain.</summary>
        public DrainResult TryDrain(DrainConditions conditions)
        {
            var allowed = !IsDestroyed && conditions.IsQuiescent && !conditions.ModalRackCadActive && conditions.PanelVisible;
            if (!allowed || PendingHintCount == 0)
                return new DrainResult(false, System.Array.Empty<DrainOperation>());

            var ops = new System.Collections.Generic.List<DrainOperation>();
            if (_pending.Contains(HintKind.ImpliedSelectionChanged))
            {
                ops.Add(DrainOperation.ReadImplicitSelection);
                ops.Add(DrainOperation.RecomputeSelectionContext);
            }

            // What is own only invalidates (IC-08, IC-09). The drafts are marked only by a foreign hint that was pending.
            var foreignChange = _pending.Contains(HintKind.DatabaseObjectChanged) || _pending.Contains(HintKind.CommandEnded);
            if (foreignChange || _ownPending.Count > 0) ops.Add(DrainOperation.InvalidateNavigationCaches);
            if (foreignChange) ops.Add(DrainOperation.MarkDraftsPossiblyStale);
            _pending.Clear();
            _ownPending.Clear();
            return new DrainResult(true, ops);
        }

        /// <summary>Stores a recomputed context. True only when it differs from the current one.</summary>
        public bool ApplyContext(SelectionContext context)
        {
            if (IsDestroyed || Selection.Equals(context)) return false;
            Selection = context;
            return true;
        }
    }
}
