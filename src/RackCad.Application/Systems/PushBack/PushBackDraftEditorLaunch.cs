using RackCad.Application.Persistence;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// The typed argument that opens the full Push Back editor in Workspace mode: the state to edit, the identity of the
    /// rack, the project the rack comes from and the port through which the edited state goes back.
    /// </summary>
    public sealed class PushBackDraftEditorLaunch
    {
        public PushBackDraftEditorLaunch(PushBackEditableState state, string rackId, RackProject sourceProject, IPushBackDraftStatePort port)
        {
        }

        public PushBackEditableState State { get; }

        public string RackId { get; }

        public RackProject SourceProject { get; }

        public IPushBackDraftStatePort Port { get; }
    }
}
