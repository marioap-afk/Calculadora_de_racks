using System;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-02) — the state of the FULL draft of a Push Back rack: the editable state of PB-T1b plus the
    /// raw text of the envelope it was materialized from. It is a plain container: it neither copies nor compares anything
    /// by itself (see <see cref="PushBackFullDraftStateComparer"/>).
    /// </summary>
    public sealed class PushBackFullDraftState
    {
        public PushBackFullDraftState(PushBackEditableState editable, string origin)
        {
            Editable = editable ?? throw new ArgumentNullException(nameof(editable));
            Origin = origin ?? throw new ArgumentNullException(nameof(origin));
        }

        public PushBackEditableState Editable { get; }

        /// <summary>The raw text of the envelope the state was materialized from; in the initial state, always the current base.</summary>
        public string Origin { get; }
    }
}
