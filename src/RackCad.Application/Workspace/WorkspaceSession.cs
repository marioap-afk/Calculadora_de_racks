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

        /// <summary>Number of distinct pending hint kinds; zero when nothing awaits a drain.</summary>
        public int PendingHintCount => _pending.Count;

        /// <summary>Queues a hint. Constant cost, no reads. False when the session is destroyed.</summary>
        public bool EnqueueHint(HintKind kind)
        {
            if (IsDestroyed) return false;
            _pending.Add(kind);
            return true;
        }

        /// <summary>Drains pending hints only when the conditions allow it; coalesces N hints into one drain.</summary>
        public DrainResult TryDrain(DrainConditions conditions)
        {
            var allowed = !IsDestroyed && conditions.IsQuiescent && !conditions.ModalRackCadActive && conditions.PanelVisible;
            if (!allowed || _pending.Count == 0)
                return new DrainResult(false, System.Array.Empty<DrainOperation>());

            var ops = new System.Collections.Generic.List<DrainOperation>();
            if (_pending.Contains(HintKind.ImpliedSelectionChanged))
            {
                ops.Add(DrainOperation.ReadImplicitSelection);
                ops.Add(DrainOperation.RecomputeSelectionContext);
            }
            if (_pending.Contains(HintKind.DatabaseObjectChanged) || _pending.Contains(HintKind.CommandEnded))
            {
                ops.Add(DrainOperation.InvalidateNavigationCaches);
                ops.Add(DrainOperation.MarkDraftsPossiblyStale);
            }
            _pending.Clear();
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
