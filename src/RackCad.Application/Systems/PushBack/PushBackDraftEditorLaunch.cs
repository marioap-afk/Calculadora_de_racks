using System;
using RackCad.Application.Persistence;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>
    /// The typed argument that opens the full Push Back editor in Workspace mode: the state to edit, the identity of the
    /// rack, the project the rack comes from and the port through which the edited state goes back. It keeps the
    /// references it is given as they are: it copies nothing.
    /// </summary>
    public sealed class PushBackDraftEditorLaunch
    {
        public PushBackDraftEditorLaunch(PushBackEditableState state, string rackId, RackProject sourceProject, IPushBackDraftStatePort port)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (string.IsNullOrWhiteSpace(rackId)) throw new ArgumentException("A rack id is required.", nameof(rackId));
            if (port == null) throw new ArgumentNullException(nameof(port));
            State = state;
            RackId = rackId;
            SourceProject = sourceProject;
            Port = port;
        }

        public PushBackEditableState State { get; }

        public string RackId { get; }

        /// <summary>The inner project of the envelope, with its metadata. It may be null.</summary>
        public RackProject SourceProject { get; }

        public IPushBackDraftStatePort Port { get; }
    }
}
