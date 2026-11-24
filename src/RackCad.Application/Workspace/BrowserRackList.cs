#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// One rack of the list of the drawing that the RackCad window shows in its side column (I-64 A-14, phase 2: "Racks del dibujo"). A
    /// plain value projected from the same neutral capture facts as the lazy summary: identity, name, Kind and views. It holds no host
    /// object, no handle and no reference to the drawing. <see cref="Name"/> and <see cref="Kind"/> may be null when no fact of the rack
    /// carries them; the window decides how to show that.
    /// </summary>
    public sealed record BrowserRackListEntry(string RackId, string? Name, string? Kind, IReadOnlyList<string> Views);

    /// <summary>
    /// The answer of the port to a request of the list of racks of the drawing: the status, with the same meaning as the one of the lazy
    /// summary, and the racks in reading order. A normal rejection or failure is a value, never a thrown exception; only
    /// <see cref="BrowserSummaryStatus.Ready"/> carries racks.
    /// </summary>
    public sealed record BrowserRackList(BrowserSummaryStatus Status, IReadOnlyList<BrowserRackListEntry> Racks, string? Reason)
    {
        public bool IsReady => Status == BrowserSummaryStatus.Ready;

        public static BrowserRackList Ready(IReadOnlyList<BrowserRackListEntry> racks) =>
            new(BrowserSummaryStatus.Ready, racks ?? Array.Empty<BrowserRackListEntry>(), null);

        public static BrowserRackList Without(BrowserSummaryStatus status, string reason) =>
            new(status, Array.Empty<BrowserRackListEntry>(), reason);

        /// <summary>What a port that does not offer the list answers: nothing was read and nothing will be.</summary>
        public static BrowserRackList NotOffered { get; } =
            Without(BrowserSummaryStatus.Rejected, "This port does not offer the list of racks of the drawing.");

        /// <summary>
        /// The drawing changed since the list was read and the host does not read it again on its own (every read of the palette locks the
        /// document, O-18): the window keeps the list it has and says that it may be outdated until the user reads it again.
        /// </summary>
        public bool IsOutdated { get; init; }

        public static BrowserRackList Outdated() =>
            Without(BrowserSummaryStatus.Busy, "The drawing changed since the list was read; it is read again when the user asks.") with { IsOutdated = true };
    }
}
