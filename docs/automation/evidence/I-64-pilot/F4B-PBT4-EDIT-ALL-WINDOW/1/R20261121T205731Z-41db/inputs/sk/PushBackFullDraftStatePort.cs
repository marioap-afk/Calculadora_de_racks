using System;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>I-64 F4B PB-T4c (RED): the port of the full editor over the full draft of the panel, still without effect: it hands nothing to the draft.</summary>
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
            return new PushBackDraftApplyResult(PushBackDraftApplyStatus.NotEditable, _draft.IsDirty && _applied != null);
        }
    }
}
