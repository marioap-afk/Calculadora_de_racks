using System.Collections.Generic;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-02) — the comparer and the copier of <see cref="PushBackFullDraftState"/>.
    /// </summary>
    public sealed class PushBackFullDraftStateComparer : IEqualityComparer<PushBackFullDraftState>
    {
        private PushBackFullDraftStateComparer()
        {
        }

        public static PushBackFullDraftStateComparer Instance { get; } = new PushBackFullDraftStateComparer();

        public bool Equals(PushBackFullDraftState x, PushBackFullDraftState y) => ReferenceEquals(x, y);

        public int GetHashCode(PushBackFullDraftState obj) => 0;

        public static PushBackFullDraftState Copy(PushBackFullDraftState state)
            => new PushBackFullDraftState(new PushBackEditableState(), state.Origin);
    }
}
