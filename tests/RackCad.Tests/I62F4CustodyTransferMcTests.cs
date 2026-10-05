#nullable enable
using System;
using Xunit;
using static RackCad.Tests.CustodyMc;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-H), C-15 (reproducible controls, run as guards) on disposable repositories with real SHAs: T17 (the holder releases the unit
    /// with a QH and never operates after it), T16 (a transfer without an advance of <c>main</c>: the designated successor publishes one QR ORDINARY from a
    /// clean host) and F.3 with T12b from a last point other than Q0 (two recoveries, one writer: the first CAS wins, the other session's push is
    /// rejected and, without a designation of its own, it does not operate).
    /// </summary>
    public class I62F4CustodyTransferMcTests
    {
        private const string Designated = "B20261005T010101Z-ef56";
        private const string NotDesignated = "B20261005T020202Z-ef99";

        [Fact]
        public void I62_C15_T17_T16_AReleaseEndsTheHolderAndATransferWithoutAnAdvanceOfMainIsOneOrdinaryQr()
        {
            var (r, pts, _, _) = Window1();
            using var _r = r;
            var h = r.Holder;
            var (p5, k5) = pts[4];

            // T17: the holder releases the unit with a QH (CAS); a later point of the old holder is not a transition from QH (I-P02).
            var kh = r.WritePoint(h, With(p5, s =>
            {
                Rv(s, 6);
                ((YamlMap)s["custody"]!)["point"] = "QH";
                ((YamlMap)s["custody"]!)["point_kind"] = null;
                ((YamlMap)Y.M(s, "custody.principal")!)["state"] = "RELEASED";
            }), "I-99: QH");
            var ph = r.Read(h, kh);
            Clean(All(h, p5, k5, ph, kh, kh), "QH");
            var late = r.WritePoint(h, With(ph, s =>
            {
                Rv(s, 7);
                ((YamlMap)s["custody"]!)["point"] = "QU";
                ((YamlMap)s["custody"]!)["point_kind"] = "ORDINARY";
                ((YamlMap)Y.M(s, "custody.principal")!)["state"] = "HELD";
            }), "I-99: QU del titular tras liberar", push: false);
            Assert.Contains("I-P02", Ids(All(h, ph, kh, r.Read(h, late), late, late)));

            // T16: main does not advance; the designated B, from a clean host, publishes one QR ORDINARY with its accepted binding and its designation.
            var b = r.CleanClone("b");
            var (s7, t7) = NewHolder(r.Read(b, kh), 7, Designated, Designated);
            var k7 = r.WritePoint(b, new StatePoint(s7, t7), "I-99: QR (T16)");
            Clean(All(b, ph, kh, r.Read(b, k7), k7, k7), "QR (T16)");
            Assert.Equal(k7, r.Git(b, "ls-remote", "origin", "refs/heads/feature").Split('\t')[0]);

            // A QR by a session that the designation does not name: its acceptance cites a decision without its binding (I-S16).
            r.Git(b, "reset", "-q", "--hard", kh);
            var (sx, tx) = NewHolder(r.Read(b, kh), 7, NotDesignated, Designated);
            var kx = r.WritePoint(b, new StatePoint(sx, tx), "I-99: QR sin designación", push: false);
            Assert.Contains("I-S16", Ids(All(b, ph, kh, r.Read(b, kx), kx, kx)));
        }

        [Fact]
        public void I62_C15_F3_T12b_TwoRecoveriesHaveOneWriterTheFirstCasWinsAndTheOtherNeverOperates()
        {
            // The holder P is orphaned after a QU (last point ≠ Q0: T12b); its termination is accredited and the Coordinator designates N1 (Dk).
            var (r, pts, _, _) = Window1();
            using var _r = r;
            var (p5, k5) = pts[4];
            var n1 = r.CleanClone("n1");
            var n2 = r.CleanClone("n2");

            var (s6, t6) = NewHolder(r.Read(n1, k5), 6, Designated, Designated);
            var k6 = r.WritePoint(n1, new StatePoint(s6, t6), "I-99: QR de N1 (T12b)");
            Clean(All(n1, p5, k5, r.Read(n1, k6), k6, k6), "QR N1");

            // N2 prepares its own QR from the same record_version: the push is rejected (no fast-forward) and the remote keeps N1's QR.
            var (s6b, t6b) = NewHolder(r.Read(n2, k5), 6, NotDesignated, Designated);
            var k6b = r.WritePoint(n2, new StatePoint(s6b, t6b), "I-99: QR de N2", push: false);
            var rejected = Assert.Throws<InvalidOperationException>(() => r.Git(n2, "push", "-q", "origin", "HEAD:feature"));
            Assert.Contains("rejected", rejected.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(k6, r.Git(n2, "ls-remote", "origin", "refs/heads/feature").Split('\t')[0]);
            Assert.Contains("I-S16", Ids(All(n2, p5, k5, r.Read(n2, k6b), k6b, k6b)));

            // After the fetch N2 sees N1 designated by Dk; a point of N2 on top of it has no designation of its own (I-S16): N2 does not operate.
            r.Git(n2, "fetch", "-q", "origin");
            r.Git(n2, "reset", "-q", "--hard", "origin/feature");
            var p6 = r.Read(n2, k6);
            var (s7, t7) = NewHolder(p6, 7, NotDesignated, Designated);
            var k7 = r.WritePoint(n2, new StatePoint(s7, t7), "I-99: QR de N2 tras ver a N1", push: false);
            Assert.Contains("I-S16", Ids(All(n2, p6, k6, r.Read(n2, k7), k7, k7)));
        }
    }
}
