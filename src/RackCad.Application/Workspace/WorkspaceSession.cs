#nullable enable
namespace RackCad.Application.Workspace;

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

    public int PendingHintCount => 0;

    /// <summary>Queues a hint. Constant cost, no reads. False when the session is destroyed.</summary>
    public bool EnqueueHint(HintKind kind) => false;

    /// <summary>Drains pending hints only when the conditions allow it; coalesces N hints into one drain.</summary>
    public DrainResult TryDrain(DrainConditions conditions) =>
        new(false, System.Array.Empty<DrainOperation>());

    /// <summary>Stores a recomputed context. True only when it differs from the current one.</summary>
    public bool ApplyContext(SelectionContext context) => false;
}
