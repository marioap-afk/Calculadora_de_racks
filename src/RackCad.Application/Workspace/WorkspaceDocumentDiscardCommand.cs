#nullable enable
using System;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// I-64 F4-T3C2 (D-T3C2-02): the pure core of the command that discards the drafts of the panel of the active document. This is the
    /// skeleton of the first step: it compiles, throws nothing and does nothing.
    /// </summary>
    public static class WorkspaceDocumentDiscardCommand
    {
        public static void Run(
            IWorkspacePilotEditPort? port,
            Func<SessionId?> activeSession,
            Func<SessionId, bool> isTarget,
            Func<string, bool> confirm,
            Action<string> write)
        {
        }
    }
}
