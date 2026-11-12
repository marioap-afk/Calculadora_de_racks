#nullable enable
using System;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Pure scheduler of the idle pass of the panel (I-64 F4-T2C, IC-04). Skeleton of the red step: the API exists and compiles, it
    /// never throws and answers inert values; it schedules nothing yet.
    /// </summary>
    public sealed class WorkspaceIdleScheduler
    {
        public WorkspaceIdleScheduler(Func<bool> canRunNow, Action continuation, Action<string, Exception>? logFault = null)
        {
        }

        public bool IsScheduled => false;

        public int ContinuationsRun => 0;

        public int ContinuationsSkipped => 0;

        public void SetPost(Action<Action> post)
        {
        }

        public void OnIdleTick()
        {
        }
    }
}
