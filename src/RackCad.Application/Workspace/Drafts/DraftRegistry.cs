#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace.Drafts
{
    public enum DraftBeginStatus
    {
        Created,
        AlreadyExists,
        NotAdmitted,
        SessionRetired,
    }

    public sealed class DraftBeginResult<TState>
    {
        internal DraftBeginResult(DraftBeginStatus status, DraftSession<TState>? draft, DraftAdmission admission)
        {
            Status = status;
            Draft = draft;
            Admission = admission;
        }

        public DraftBeginStatus Status { get; }

        public DraftSession<TState>? Draft { get; }

        public DraftAdmission Admission { get; }
    }

    public readonly record struct DraftDiscardResult(bool Removed, bool WasDirty);

    public sealed class DraftDiscardAllResult
    {
        internal DraftDiscardAllResult(IReadOnlyList<string> removedRackIds, IReadOnlyList<string> dirtyRackIds)
        {
            RemovedRackIds = removedRackIds;
            DirtyRackIds = dirtyRackIds;
        }

        public IReadOnlyList<string> RemovedRackIds { get; }

        public IReadOnlyList<string> DirtyRackIds { get; }
    }

    public sealed class DraftLossNotice
    {
        internal DraftLossNotice(SessionId session, IReadOnlyList<string> lostRackIds)
        {
            Session = session;
            LostRackIds = lostRackIds;
        }

        public SessionId Session { get; }

        public IReadOnlyList<string> LostRackIds { get; }

        public bool HasLoss => LostRackIds.Count > 0;
    }

    /// <summary>RED skeleton of the in-memory registry: it never creates nor keeps a draft.</summary>
    public sealed class DraftRegistry
    {
        public DraftBeginResult<TState> Begin<TState>(SessionId session, string rackId, DraftIntentRead<TState> read, IEqualityComparer<TState> comparer) =>
            new(DraftBeginStatus.NotAdmitted, null, read.Admission);

        public DraftSession<TState>? Find<TState>(SessionId session, string rackId) => null;

        public bool Contains(SessionId session, string rackId) => false;

        public int CountOf(SessionId session) => 0;

        public IReadOnlyList<string> RackIdsOf(SessionId session) => Array.Empty<string>();

        public IReadOnlyList<string> RacksAtRiskOf(SessionId session) => Array.Empty<string>();

        public bool HasUpdateInProgress => false;

        public bool IsRetired(SessionId session) => false;

        public DraftDiscardResult Discard(SessionId session, string rackId) => new(false, false);

        public DraftDiscardAllResult DiscardAll(SessionId session) => new(Array.Empty<string>(), Array.Empty<string>());

        public DraftLossNotice RetireSession(SessionId session) => new(session, Array.Empty<string>());
    }
}
