using System;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>I-64 F4B PB-T4c (RED): the launch of the full Push Back editor over the full draft of the panel, still without the project of its origin.</summary>
    public static class PushBackFullDraftLaunch
    {
        public static PushBackDraftEditorLaunch For(DraftSession<PushBackFullDraftState> draft, IPushBackDraftStatePort port)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (port == null) throw new ArgumentNullException(nameof(port));
            return new PushBackDraftEditorLaunch(draft.CurrentDraftState.Editable, draft.RackId, null, port);
        }
    }
}
