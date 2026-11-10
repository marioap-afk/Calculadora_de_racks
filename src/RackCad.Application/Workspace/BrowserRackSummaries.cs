#nullable enable
using System;
using System.Collections.Generic;

namespace RackCad.Application.Workspace
{
    /// <summary>
    /// Neutral facts of ONE block definition as the host captured them for the browser summary (I-64 F3-T2, D-05). They carry no
    /// handle of any reference: only what the summary projects. They are private to the panel and are not a reusable foundation.
    /// </summary>
    public sealed record BrowserRackFact(bool IsInterpretable, string? EnvelopeId, string? Kind, string? View, string? Name);

    /// <summary>
    /// Pure projector and memory of the lazy rack summary (I-64 F3-T2). SKELETON of the RED step: the surface compiles and answers
    /// nothing useful yet; the projection, the memory by (session, rack), the invalidation and the purge arrive with the GREEN step.
    /// </summary>
    public sealed class BrowserRackSummaries
    {
        private readonly Func<SessionId, IReadOnlyList<BrowserRackFact>> _readFacts;
        private readonly Func<SessionId, bool> _isSessionAlive;

        public BrowserRackSummaries(Func<SessionId, IReadOnlyList<BrowserRackFact>> readFacts, Func<SessionId, bool> isSessionAlive)
        {
            _readFacts = readFacts ?? throw new ArgumentNullException(nameof(readFacts));
            _isSessionAlive = isSessionAlive ?? throw new ArgumentNullException(nameof(isSessionAlive));
        }

        public int KnownSessionCount => 0;

        public bool IsCached(SessionId session, string rackId) => false;

        public int CachedRackCount(SessionId session) => 0;

        public bool IsSessionEnded(SessionId session) => false;

        public BrowserRackSummary Read(SessionId session, string rackId) =>
            BrowserRackSummary.Without(BrowserSummaryStatus.Failed, "Not implemented yet.");

        public void OnDrained(DrainOutcome outcome)
        {
        }

        public void EndSession(SessionId session)
        {
        }
    }
}
