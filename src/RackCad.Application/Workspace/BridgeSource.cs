#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Every event source the central bridge governs (I-64 D-04). Each one is attached at most once per owner:
    /// the global ones once per process, the per-session ones once per live document session.
    /// </summary>
    public enum BridgeSource
    {
        // Document collection: global, one attachment each.
        DocumentCreated,
        DocumentActivated,
        DocumentToBeDestroyed,

        // Per document session. BeginDocumentClose is not a synchronization source: it lives as long as the session.
        BeginDocumentClose,
        ImpliedSelectionChanged,
        CommandWillStart,
        CommandEnded,
        CommandCancelled,
        CommandFailed,
        EnteringQuiescentState,

        // Per document session: the object events of that document's content store.
        ObjectAppended,
        ObjectErased,
        ObjectModified,
        ObjectUnappended,
        ObjectReappended,

        // Global and dynamic: attached only while some session has pending hints.
        Idle,
    }

    /// <summary>Static classification of <see cref="BridgeSource"/>: which ones are global, which per session, which are synchronization.</summary>
    public static class BridgeSources
    {
        /// <summary>The document-collection sources: activation, creation and destruction.</summary>
        public static IReadOnlyList<BridgeSource> DocumentCollection { get; } = new[]
        {
            BridgeSource.DocumentCreated,
            BridgeSource.DocumentActivated,
            BridgeSource.DocumentToBeDestroyed,
        };

        /// <summary>The sources attached for every live session, in attach order (BeginDocumentClose first).</summary>
        public static IReadOnlyList<BridgeSource> PerSession { get; } = new[]
        {
            BridgeSource.BeginDocumentClose,
            BridgeSource.ImpliedSelectionChanged,
            BridgeSource.CommandWillStart,
            BridgeSource.CommandEnded,
            BridgeSource.CommandCancelled,
            BridgeSource.CommandFailed,
            BridgeSource.EnteringQuiescentState,
            BridgeSource.ObjectAppended,
            BridgeSource.ObjectErased,
            BridgeSource.ObjectModified,
            BridgeSource.ObjectUnappended,
            BridgeSource.ObjectReappended,
        };

        /// <summary>Every governed source.</summary>
        public static IReadOnlyList<BridgeSource> All { get; } =
            DocumentCollection.Concat(PerSession).Concat(new[] { BridgeSource.Idle }).ToArray();

        public static bool IsPerSession(BridgeSource source) => PerSession.Contains(source);

        /// <summary>A per-session source that a degraded session gives up. BeginDocumentClose is never one of them (A64-PV3-04).</summary>
        public static bool IsSynchronization(BridgeSource source) =>
            source != BridgeSource.BeginDocumentClose && IsPerSession(source);
    }

    /// <summary>
    /// One attachment of one governed source to one owner. Global sources carry the default session and no native id.
    /// Value equality: the same attachment is never made twice.
    /// </summary>
    public readonly record struct SourceAttachment(BridgeSource Source, SessionId Session, long NativeInstanceId)
    {
        public static SourceAttachment Global(BridgeSource source) => new(source, default, 0);

        public bool IsGlobal => Session.Value == 0;
    }
}
