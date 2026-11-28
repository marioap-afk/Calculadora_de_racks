using System;
using RackCad.Application.Persistence;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4c (D-PBT4-13) — the launch of the full Push Back editor over the full draft of the panel: the editable state of the draft
    /// (the window copies it when it loads), the identity of the rack, the project of the envelope of its origin and the port through which the
    /// edits go back to the draft. It reads no drawing: the project comes from the text of the origin that the draft already holds.
    /// </summary>
    public static class PushBackFullDraftLaunch
    {
        public static PushBackDraftEditorLaunch For(DraftSession<PushBackFullDraftState> draft, IPushBackDraftStatePort port)
        {
            if (draft == null) throw new ArgumentNullException(nameof(draft));
            if (port == null) throw new ArgumentNullException(nameof(port));
            var current = draft.CurrentDraftState;
            return new PushBackDraftEditorLaunch(current.Editable, draft.RackId, ProjectOf(current.Origin), port);
        }

        // The project of the envelope of the origin, with its metadata, or null when the text does not hold one: the launch admits a null one.
        private static RackProject ProjectOf(string origin)
        {
            var envelope = new RackEmbedStore().Deserialize(origin);
            if (envelope == null) return null;
            return new RackProjectStore().TryDeserialize(envelope.Design, out var project, out _) ? project : null;
        }
    }
}
