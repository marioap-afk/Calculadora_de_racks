#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.StateV2Samples;

using static RackCad.Tests.CustodyMc;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G), C-15 (reproducible control, run as a guard): CAS by Git — a rejected push is a transition not credited on the remote, the local commit is
    /// kept, and a transport failure credits nothing either.
    /// </summary>
    public class I62F4CustodyCasMcTests
    {
        [Fact]
        public void I62_C15_ARejectedPushIsATransitionNotCreditedOnTheRemoteAndTheLocalCommitIsKept()
        {
            var (r, pts, _, _) = Window1();
            using var _r = r;
            var other = r.CleanClone("otra-sesion");
            var (p5, k5) = pts[4];

            // Another session publishes the next point first (CAS: n+1 on origin/feature).
            var won = r.WritePoint(other, With(r.Read(other, k5), s => Rv(s, 6)), "I-99: QU de otra sesión");

            // The holder's own n+1 is rejected as non fast-forward: not credited on the remote, kept locally, classified after a fetch (T10).
            var local = r.WritePoint(r.Holder, With(p5, s => Rv(s, 6)), "I-99: QU del titular", push: false);
            var rejected = Assert.Throws<InvalidOperationException>(() => r.Git(r.Holder, "push", "-q", "origin", "HEAD:feature"));
            Assert.Contains("rejected", rejected.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(won, r.Git(r.Holder, "ls-remote", "origin", "refs/heads/feature").Split('\t')[0]);
            Assert.Equal(local, r.Head(r.Holder));
            r.Git(r.Holder, "fetch", "-q", "origin");
            Assert.False(new GitProcessHistory(r.Holder).IsAncestor(local, won));

            // A transport failure is not a transition either: nothing reaches the remote.
            r.Git(r.Holder, "remote", "set-url", "origin", System.IO.Path.Combine(r.G.Root, "no-existe.git"));
            Assert.Throws<InvalidOperationException>(() => r.Git(r.Holder, "push", "-q", "origin", "HEAD:feature"));
            r.Git(r.Holder, "remote", "set-url", "origin", r.Origin);
            Assert.Equal(won, r.Git(r.Holder, "ls-remote", "origin", "refs/heads/feature").Split('\t')[0]);
        }
    }
}
