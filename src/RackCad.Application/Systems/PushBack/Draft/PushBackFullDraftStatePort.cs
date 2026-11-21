using System;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4c (D-PBT4-13) — the port of the full editor over the full draft of the panel. It hands the edited state to the draft with
    /// the origin that the draft holds, so the origin is kept, and the draft keeps a COPY of it. Every edit that the draft takes calls back
    /// once, so the host ends the "known clean" mark of the session. It copies nothing and calls no other operation of the draft.
    /// </summary>
    public sealed class PushBackFullDraftStatePort : IPushBackDraftStatePort
    {
        private readonly DraftSession<PushBackFullDraftState> _draft;
        private readonly Action _applied;

        public PushBackFullDraftStatePort(DraftSession<PushBackFullDraftState> draft, Action applied)
        {
            _draft = draft ?? throw new ArgumentNullException(nameof(draft));
            _applied = applied ?? throw new ArgumentNullException(nameof(applied));
        }

        public PushBackDraftApplyResult ApplyToDraft(PushBackEditableState edited)
        {
            if (edited == null) throw new ArgumentNullException(nameof(edited));
            var result = _draft.SetCurrentDraftState(new PushBackFullDraftState(edited, _draft.CurrentDraftState.Origin));
            if (result.Status != DraftEditStatus.Updated) return new PushBackDraftApplyResult(PushBackDraftApplyStatus.NotEditable, result.IsDirty);
            _applied();
            return new PushBackDraftApplyResult(PushBackDraftApplyStatus.Applied, result.IsDirty);
        }
    }
}
