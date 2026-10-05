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
    /// I-62 F4 (slice F4-G), C-15 (reproducible control, run as a guard): each seeded mutation of the C-15 row, on real points of a disposable repository, is detected by
    /// the invariant that corresponds to it.
    /// </summary>
    public class I62F4CustodyMutationMcTests
    {
        [Fact]
        public void I62_C15_EachSeededMutationOnRealPointsIsDetectedByItsInvariant()
        {
            var (r, pts, _, _) = Window1();
            using var _r = r;
            var h = r.Holder;
            var (p1, k1) = pts[0];
            var (p2, k2) = pts[1];
            var (p3, k3) = pts[2];
            var (p4, k4) = pts[3];
            var (p5, k5) = pts[4];
            List<string> Pair(StatePoint p, string pc, StatePoint n, string nc) => Ids(All(h, p, pc, n, nc, nc));

            // Q0 omitted (QU → Q7), and a window with two Q0 (Q0 → Q0).
            Assert.Contains("I-P02", Pair(p2, k2, With(p4, s => Rv(s, 3)), k4));
            Assert.Contains("I-P02", Pair(p3, k3, With(p3, s => Rv(s, 4)), k3));

            // Q0 with g0_acceptance PENDING; a BOOTSTRAP that cites a decision.
            Assert.Contains("I-S15", Ids(new StateV2Validator().ValidateFile(With(p3, s =>
            {
                ((YamlMap)Y.M(s, "protocol.g0_acceptance")!)["state"] = "PENDING";
                ((YamlMap)Y.M(s, "protocol.g0_acceptance")!)["decision"] = null;
            }))));
            Assert.Contains("I-S16", Ids(new StateV2Validator().ValidateFile(With(p1, s =>
                ((YamlMap)Y.M(s, "protocol.g0_acceptance")!)["decision"] = Clone((YamlMap)Y.Get(p2.State, "protocol.g0_acceptance.decision")!)))));

            // A second transition of g0_acceptance; record_version skipped; a QU ORDINARY that touches the window; attempts reset.
            Assert.Contains("I-P09", Pair(p4, k4, With(p5, s => ((YamlMap)Y.M(s, "protocol.g0_acceptance")!)["state"] = "REJECTED"), k5));
            Assert.Contains("I-P01", Pair(p4, k4, With(p5, s => Rv(s, 7)), k5));
            Assert.Contains("I-P05", Pair(p4, k4, With(p5, s => ((YamlMap)Y.M(s, "custody.window")!)["seq"] = 2L), k5));
            Assert.Contains("I-P05", Pair(With(p4, s => ((YamlMap)s["automation_state"]!)["attempts"] = 1L), k4, p5, k5));

            // An acceptance inferred without the markers: the decisions entry of the acceptance QU carries none.
            r.Git(h, "checkout", "-q", "-b", "sin-marcadores", k1);
            var bare = CustodyHistory()[1];
            ((InMemoryStateTree)bare.Tree).Put("docs/automation/decisions/I-99.md", "# Decisiones de I-99\n\n## G0\n\nGATE PASS de G0.\n");
            var km = r.WritePoint(h, bare, "I-99: QU sin marcadores", push: false);
            var noMarkers = Pair(p1, k1, r.Read(h, km), km);
            Assert.Contains("I-S16", noMarkers);
            Assert.Contains("I-P09", noMarkers);

            // A commit of the session inside the window (W-2) and a Worker commit that writes the state file (W-3), on real histories.
            r.Git(h, "checkout", "-q", "-b", "ventana-con-sesion", k3);
            r.Commit(h, "sesión dentro de la ventana", ("docs/automation/evidence/I-99-agent/nota.md", "nota\n"));
            r.Subst[Sha3] = r.Commit(h, "T-01 RED", (RedFile, "rojo\n"));
            r.Subst[Sha2] = r.Commit(h, "T-01 GREEN", (GreenFile, "verde\n"));
            var kw2 = r.WritePoint(h, CustodyHistory()[3], "I-99: Q7", push: false);
            Assert.Contains("I-P03", Pair(p3, k3, r.Read(h, kw2), kw2));

            r.Git(h, "checkout", "-q", "-b", "worker-escribe-estado", k3);
            r.Subst[Sha3] = r.Commit(h, "T-01 RED", (RedFile, "rojo\n"), (CustodyRepo.StatePath, "schema: rackcad-automation-state/v2\n"));
            r.Subst[Sha2] = r.Commit(h, "T-01 GREEN", (GreenFile, "verde\n"));
            var kw3 = r.WritePoint(h, CustodyHistory()[3], "I-99: Q7", push: false);
            var w3 = Pair(p3, k3, r.Read(h, kw3), kw3);
            Assert.Contains("I-P08", w3);
            Assert.DoesNotContain("I-P03", w3);
        }
    }
}
