#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RackCad.Application.Workspace;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F2-T2B-NAVIGATION-UI: the simulated navigation port of the panel tests. It records every request exactly as the panel
    /// sent it and answers what the test sets, so a test can prove which session, rack, copy and view travelled and that a
    /// disabled or contained action never reached the port. It can also hold an answer back (an action in progress), throw
    /// before returning a task, or return a faulted task.
    /// </summary>
    internal sealed class WorkspaceNavigationPortDouble : IWorkspaceNavigationPort
    {
        public sealed record Call(
            WorkspaceRequest Origin,
            string RackId,
            NavigationActionKind Action,
            int CopyIndex,
            NavigationViewOrder? View,
            NavigationActionKind LocateWith);

        public List<Call> Calls { get; } = new();

        /// <summary>The answer to every request, unless <see cref="Responder"/> decides.</summary>
        public WorkspaceNavigationResult Result { get; set; } = Executed(3);

        public Func<Call, WorkspaceNavigationResult>? Responder { get; set; }

        /// <summary>Thrown by ExecuteAsync itself, before it returns a task.</summary>
        public Exception? ThrowSynchronously { get; set; }

        /// <summary>The task ExecuteAsync returns is faulted with this exception.</summary>
        public Exception? FaultTheTask { get; set; }

        /// <summary>While set, every request waits for the test to complete it (an action in progress).</summary>
        public TaskCompletionSource<WorkspaceNavigationResult>? Gate { get; set; }

        public static WorkspaceNavigationResult Executed(int? count) => WorkspaceNavigationResult.Executed() with { TargetCount = count };

        public Task<WorkspaceNavigationResult> ExecuteAsync(
            WorkspaceRequest origin,
            string rackId,
            NavigationActionKind action,
            int copyIndex = 0,
            NavigationViewOrder? view = null,
            NavigationActionKind locateWith = NavigationActionKind.View)
        {
            var call = new Call(origin, rackId, action, copyIndex, view, locateWith);
            Calls.Add(call);

            if (ThrowSynchronously != null) throw ThrowSynchronously;
            if (FaultTheTask != null) return Task.FromException<WorkspaceNavigationResult>(FaultTheTask);
            if (Gate != null) return Gate.Task;
            return Task.FromResult(Responder != null ? Responder(call) : Result);
        }
    }
}
