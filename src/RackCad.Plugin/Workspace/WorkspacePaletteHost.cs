namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// Visual host of the workspace panel (I-64 F1-T3B). Skeleton of the RED step: the surface the command uses, with no
    /// palette yet.
    /// </summary>
    public sealed class WorkspacePaletteHost
    {
        public static WorkspacePaletteHost GetOrCreate() => new WorkspacePaletteHost();

        public void Show()
        {
        }
    }
}
