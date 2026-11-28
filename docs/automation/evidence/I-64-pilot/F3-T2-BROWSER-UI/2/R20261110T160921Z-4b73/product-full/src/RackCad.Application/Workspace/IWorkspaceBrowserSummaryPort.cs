#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>How a summary request ended. A normal rejection or failure is a value, never a thrown exception.</summary>
    public enum BrowserSummaryStatus
    {
        /// <summary>The summary was read (or served from memory).</summary>
        Ready,

        /// <summary>The rack is not in the drawing of the session. Nothing changed.</summary>
        NotFound,

        /// <summary>The host is not at rest (a command is in progress), so nothing was read; the caller asks again later.</summary>
        Busy,

        /// <summary>The request does not belong to the document that would read it, or its session is gone. Nothing was read.</summary>
        Rejected,

        /// <summary>The host failed while reading. The failure is contained and reported as a value.</summary>
        Failed,
    }

    /// <summary>
    /// The lazy summary of ONE rack: name, Kind and the views it has. A plain value projected from the neutral capture facts; it
    /// holds no host object, no handle and no reference to the drawing. <see cref="Reason"/> says why it is not ready.
    /// </summary>
    public sealed record BrowserRackSummary(
        BrowserSummaryStatus Status,
        string? Name,
        string? Kind,
        IReadOnlyList<string> Views,
        string? Reason)
    {
        public bool IsReady => Status == BrowserSummaryStatus.Ready;

        public static BrowserRackSummary Ready(string? name, string? kind, IReadOnlyList<string> views) =>
            new(BrowserSummaryStatus.Ready, name, kind, views ?? Array.Empty<string>(), null);

        public static BrowserRackSummary Without(BrowserSummaryStatus status, string reason) =>
            new(status, null, null, Array.Empty<string>(), reason);
    }

    /// <summary>
    /// The read-only port through which the browser of the panel asks for the summary of the rack of its live surface (I-64 F3-T2).
    /// No host type crosses it. Every request carries the session of the presented snapshot; the host authorizes it against the
    /// document that would read it and answers with an immutable value. It writes nothing: no selection, no view, no drawing.
    /// </summary>
    public interface IWorkspaceBrowserSummaryPort
    {
        /// <summary>
        /// The summary of one rack. <paramref name="userRequested"/> says whether an explicit action of the user asked for it (a tab
        /// click or the reload of the surface) or a presentation of the panel did: a presentation arrives inside a notice of the
        /// host, where the drawing must not be read, so the host answers it only while it is draining; an explicit action does not
        /// depend on that and keeps every other check.
        /// </summary>
        BrowserRackSummary ReadSummary(WorkspaceRequest origin, string rackId, bool userRequested);

        /// <summary>Whether the session still lives; the browser destroys the tabs of a session that does not.</summary>
        bool IsSessionAlive(SessionId session);
    }
}
