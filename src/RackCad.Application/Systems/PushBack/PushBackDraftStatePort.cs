using RackCad.Application.Persistence;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>What the draft answered when the editor handed it an edited state.</summary>
    public enum PushBackDraftApplyStatus
    {
        /// <summary>The draft does not accept edits (discarded, retired or in error): nothing changed.</summary>
        NotEditable = 0,

        /// <summary>The draft now holds a copy of the edited state.</summary>
        Applied = 1
    }

    /// <summary>The outcome of one <see cref="IPushBackDraftStatePort.ApplyToDraft"/> call.</summary>
    public readonly struct PushBackDraftApplyResult
    {
        public PushBackDraftApplyResult(PushBackDraftApplyStatus status, bool isDirty)
        {
            Status = status;
            IsDirty = isDirty;
        }

        public PushBackDraftApplyStatus Status { get; }

        /// <summary>Whether the draft differs from its initial state after the call (the answer of the draft itself).</summary>
        public bool IsDirty { get; }
    }

    /// <summary>The only channel through which the full Push Back editor, in Workspace mode, returns its edited state.</summary>
    public interface IPushBackDraftStatePort
    {
        PushBackDraftApplyResult ApplyToDraft(PushBackEditableState edited);
    }

    /// <summary>The port over the draft of a rack: it hands the edited state to the draft, which keeps a copy of it.</summary>
    public sealed class PushBackDraftSessionPort : IPushBackDraftStatePort
    {
        public PushBackDraftSessionPort(DraftSession<PushBackEditableState> draft)
        {
        }

        public PushBackDraftApplyResult ApplyToDraft(PushBackEditableState edited)
        {
            return new PushBackDraftApplyResult(PushBackDraftApplyStatus.NotEditable, false);
        }

        public static PushBackDraftEditorLaunch LaunchFor(DraftSession<PushBackEditableState> draft, RackProject sourceProject)
        {
            return null;
        }
    }
}
