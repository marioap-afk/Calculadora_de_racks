using System;
using System.Collections.Generic;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-02) — the comparer and the copier of <see cref="PushBackFullDraftState"/>. Two states are
    /// equal when their origin texts are equal (ordinal) and their editable states are equal for the comparer of PB-T1b.
    /// The copy shares only what the copier of PB-T1b shares (D2).
    /// </summary>
    public sealed class PushBackFullDraftStateComparer : IEqualityComparer<PushBackFullDraftState>
    {
        private PushBackFullDraftStateComparer()
        {
        }

        public static PushBackFullDraftStateComparer Instance { get; } = new PushBackFullDraftStateComparer();

        public bool Equals(PushBackFullDraftState x, PushBackFullDraftState y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return string.Equals(x.Origin, y.Origin, StringComparison.Ordinal)
                   && PushBackEditableStateComparer.Instance.Equals(x.Editable, y.Editable);
        }

        public int GetHashCode(PushBackFullDraftState obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return HashCode.Combine(
                StringComparer.Ordinal.GetHashCode(obj.Origin),
                PushBackEditableStateComparer.Instance.GetHashCode(obj.Editable));
        }

        public static PushBackFullDraftState Copy(PushBackFullDraftState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            return new PushBackFullDraftState(PushBackEditableStateComparer.Copy(state.Editable), state.Origin);
        }
    }
}
