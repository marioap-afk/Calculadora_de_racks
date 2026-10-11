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

    /// <summary>
    /// The pure host policies (I-64 D-02, D-04, D-18). No host access: they only combine the conditions given.
    /// A drain and a read need a started panel, an active healthy document session, a visible panel, a quiescent document
    /// and no RackCad modal; a read also needs the session to be the active one.
    /// </summary>
    public static class WorkspaceHostPolicy
    {
        public static bool MayDrain(WorkspaceHostConditions conditions) =>
            conditions.Started
            && conditions.HasActiveDocument
            && !conditions.SessionDegraded
            && conditions.PanelVisible
            && conditions.Quiescent
            && !conditions.ModalRackCadActive;

        public static bool MayRead(WorkspaceHostConditions conditions) =>
            MayDrain(conditions) && conditions.SessionIsActive;

        /// <summary>
        /// A running AutoCAD command or an open RackCad modal disable the actions that read or write the drawing, and so do
        /// the missing document, the missing start and a degraded session. The visibility of the panel does not.
        /// </summary>
        public static WorkspaceActionAvailability Actions(WorkspaceHostConditions conditions)
        {
            var block = BlockOf(conditions);
            var enabled = block == WorkspaceActionBlock.None;
            return new WorkspaceActionAvailability(enabled, enabled, block);
        }

        public static WorkspaceHostStatus Status(WorkspaceHostConditions conditions)
        {
            if (!conditions.Started) return WorkspaceHostStatus.NotStarted;
            if (!conditions.HasActiveDocument) return WorkspaceHostStatus.NoDocument;
            return conditions.SessionDegraded ? WorkspaceHostStatus.Degraded : WorkspaceHostStatus.Ready;
        }

        private static WorkspaceActionBlock BlockOf(WorkspaceHostConditions conditions)
        {
            if (!conditions.Started) return WorkspaceActionBlock.NotStarted;
            if (!conditions.HasActiveDocument) return WorkspaceActionBlock.NoDocument;
            if (conditions.SessionDegraded) return WorkspaceActionBlock.Degraded;
            if (conditions.ModalRackCadActive) return WorkspaceActionBlock.ModalActive;
            if (!conditions.Quiescent) return WorkspaceActionBlock.CommandActive;
            return WorkspaceActionBlock.None;
        }
    }
}
