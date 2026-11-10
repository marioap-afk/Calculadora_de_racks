#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.Workspace;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 F3-T2-BROWSER-UI: the simulated read-only summary port of the browser tests. It records every request exactly as the
    /// browser sent it and answers what the test sets, so a test can prove which session and rack travelled, that a blocked or
    /// inactive tab was never asked for, and that a failing port is contained. It can also model the host rule of INV-F3T2-13:
    /// with an <see cref="ActiveSession"/> set, a request of another session is rejected without being recorded as a read.
    /// </summary>
    internal sealed class WorkspaceBrowserSummaryPortDouble : IWorkspaceBrowserSummaryPort
    {
        /// <param name="UserRequested">Whether an explicit action of the user asked for it (true) or a presentation of the panel did (false).</param>
        public sealed record Call(WorkspaceRequest Origin, string RackId, bool UserRequested = false);

        /// <summary>Every request that reached the read (a rejected one does not).</summary>
        public List<Call> Calls { get; } = new();

        /// <summary>Every request received, rejected or not.</summary>
        public List<Call> Received { get; } = new();

        /// <summary>The sessions that no longer live: IsSessionAlive answers false for them.</summary>
        public HashSet<SessionId> Dead { get; } = new();

        /// <summary>When set, only this session may read; a request of another one is rejected with no read.</summary>
        public SessionId? ActiveSession { get; set; }

        /// <summary>
        /// Models the host rule of INV-F3T2-21 with its drain window closed: a read that no explicit action of the user asked for is
        /// answered Busy with no read, while a user-requested one still reads.
        /// </summary>
        public bool RefuseUnrequested { get; set; }

        public int AliveCalls { get; private set; }

        /// <summary>The answer to every request, unless <see cref="Responder"/> decides.</summary>
        public Func<Call, BrowserRackSummary>? Responder { get; set; }

        public Exception? ThrowOnRead { get; set; }

        public Exception? ThrowOnAlive { get; set; }

        public static BrowserRackSummary Ready(string name, string kind, params string[] views) =>
            BrowserRackSummary.Ready(name, kind, views);

        public BrowserRackSummary ReadSummary(WorkspaceRequest origin, string rackId, bool userRequested)
        {
            var call = new Call(origin, rackId, userRequested);
            Received.Add(call);

            if (RefuseUnrequested && !userRequested)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Busy, "The drawing is not read inside a notice of AutoCAD.");

            if (ActiveSession.HasValue && ActiveSession.Value != origin.Origin)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Rejected, "The request does not belong to the active document.");

            Calls.Add(call);
            if (ThrowOnRead != null) throw ThrowOnRead;
            return Responder != null ? Responder(call) : Ready("Nombre " + rackId, "Selective", "frontal", "lateral");
        }

        public bool IsSessionAlive(SessionId session)
        {
            AliveCalls++;
            if (ThrowOnAlive != null) throw ThrowOnAlive;
            return !Dead.Contains(session);
        }
    }
}
