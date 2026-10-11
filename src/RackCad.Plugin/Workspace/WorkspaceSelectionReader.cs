using RackCad.Application.Workspace;

namespace RackCad.Plugin.Workspace
{
    /// <summary>The fine selection reader (I-64 F1-T3A): skeleton.</summary>
    public sealed class WorkspaceSelectionReader : IWorkspaceSelectionReader
    {
        public WorkspaceSelectionRead Read(SessionId session, long nativeInstanceId) => WorkspaceSelectionRead.Failed;

        public void Invalidate(SessionId session)
        {
        }
    }
}
