#nullable enable
namespace RackCad.Application.Workspace
{
    /// <summary>What the host knows when it decides whether to drain, read or enable an action. Plain values only.</summary>
    public readonly record struct WorkspaceHostConditions(
        bool Started,
        bool HasActiveDocument,
        bool SessionIsActive,
        bool SessionDegraded,
        bool PanelVisible,
        bool Quiescent,
        bool ModalRackCadActive);

    /// <summary>The pure host policies (I-64 D-02, D-04, D-18). No host access: they only combine the conditions given.</summary>
    public static class WorkspaceHostPolicy
    {
        public static bool MayDrain(WorkspaceHostConditions conditions) => false;

        public static bool MayRead(WorkspaceHostConditions conditions) => false;

        public static WorkspaceActionAvailability Actions(WorkspaceHostConditions conditions) => default;

        public static WorkspaceHostStatus Status(WorkspaceHostConditions conditions) => WorkspaceHostStatus.NotStarted;
    }
}
