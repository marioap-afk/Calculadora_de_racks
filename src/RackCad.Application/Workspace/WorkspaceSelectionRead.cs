#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// What one fine read of the implicit selection saw (I-64 D-06): one fact per selected block reference that may be a
    /// rack, plus the count of everything else (other entities, nested references, external references, paper space).
    /// </summary>
    public sealed record WorkspaceSelectionRead(
        IReadOnlyList<DefinitionFact> References,
        int NonRackEntities,
        bool PickFirstEnabled = true,
        bool Succeeded = true)
    {
        /// <summary>A read that did not happen (no matching document, nothing to read). It changes no context.</summary>
        public static WorkspaceSelectionRead Failed { get; } =
            new(Array.Empty<DefinitionFact>(), 0, true, false);
    }

    /// <summary>
    /// Reads the implicit selection of the session's document. The host calls it only from the drain, at rest, with no
    /// RackCad modal open (D-06). Implemented in the Plugin; simulated in tests.
    /// </summary>
    public interface IWorkspaceSelectionReader
    {
        WorkspaceSelectionRead Read(SessionId session, long nativeInstanceId);

        /// <summary>Forgets what was learned about definitions of that session. It reads nothing.</summary>
        void Invalidate(SessionId session);
    }

    /// <summary>Maps one fine read to the selection context through <see cref="SelectionContext.Classify"/> (D-06, INV-16, INV-17).</summary>
    public static class WorkspaceSelectionMapper
    {
        public static SelectionContext ToContext(WorkspaceSelectionRead read) => SelectionContext.None;
    }

    /// <summary>What the cache remembers about one block definition: whether it is a rack, and the fact read from its envelope.</summary>
    public readonly record struct CachedDefinition(bool IsRack, DefinitionFact Fact);

    /// <summary>
    /// Per-definition memory of the fine reader, scoped by session. Hints invalidate it; it never decides anything.
    /// </summary>
    public sealed class WorkspaceDefinitionCache
    {
        public int Count => 0;

        public int CountFor(SessionId session) => 0;

        public bool TryGet(SessionId session, long definitionKey, out CachedDefinition entry)
        {
            entry = default;
            return false;
        }

        public void Store(SessionId session, long definitionKey, CachedDefinition entry)
        {
        }

        public void Invalidate(SessionId session)
        {
        }

        public void InvalidateAll()
        {
        }
    }
}
