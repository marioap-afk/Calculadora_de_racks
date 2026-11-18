using System.Collections.Generic;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// The comparer and the deep copier of <see cref="PushBackEditableState"/>. SKELETON (I-64 F4B PB-T1b, the RED): equality
    /// is reference equality, the hash is zero and the copy is an empty state.
    /// </summary>
    public sealed class PushBackEditableStateComparer : IEqualityComparer<PushBackEditableState>
    {
        public static PushBackEditableStateComparer Instance { get; } = new PushBackEditableStateComparer();

        public bool Equals(PushBackEditableState x, PushBackEditableState y) => ReferenceEquals(x, y);

        public int GetHashCode(PushBackEditableState obj) => 0;

        /// <summary>A copy of the state. Skeleton: a new empty state.</summary>
        public static PushBackEditableState Copy(PushBackEditableState state) => new PushBackEditableState();
    }
}
