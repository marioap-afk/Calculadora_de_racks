using Autodesk.AutoCAD.Runtime;

namespace RackCad.Plugin.Workspace
{
    /// <summary>
    /// RACKPANEL (I-64 F1-T3A): skeleton. The real behavior arrives in the GREEN commit.
    /// </summary>
    public sealed class WorkspacePanelCommand
    {
        [CommandMethod("RACKPANEL")]
        public void RackPanel()
        {
        }
    }
}
