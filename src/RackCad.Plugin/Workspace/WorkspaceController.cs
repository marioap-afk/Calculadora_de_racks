using RackCad.Application.Workspace;

namespace RackCad.Plugin.Workspace
{
    /// <summary>The one workspace controller of the process (I-64 F1-T3A): skeleton.</summary>
    public sealed class WorkspaceController
    {
        public static WorkspaceController GetOrCreate() => new WorkspaceController();

        public static bool IsCreated => false;

        public IWorkspaceHostPort Port => null;

        public void OpenPanel()
        {
        }
    }
}
