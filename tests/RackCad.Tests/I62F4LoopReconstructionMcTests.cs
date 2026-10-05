#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.CustodyMc;
using static RackCad.Tests.OrchestrationSamples;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G), C-29 and C-31 (reproducible controls, run as guards): the whole F.8 loop of the Architect written as real durable points
    /// of a disposable repository (real commits for the reviewed versions X v1 and X v2, for the authorization entries the materialized bindings cite,
    /// and for the custody SHAs). Every point and pair satisfies every invariant, including those with history, and a successor Principal in a clean
    /// clone reconstructs the loop only from the custodied state: the object, the ingested result, the open REQUIRED lineages, the remaining budget and
    /// the next action CORRECT_AND_REREVIEW.
    /// </summary>
    public class I62F4LoopReconstructionMcTests
    {
        [Fact]
        public void I62_C29_ASuccessorInACleanCloneReconstructsTheArchitectLoopFromTheCustodiedStateOnly()
        {
            using var r = new CustodyRepo();
            var h = r.Holder;
            var (points, _) = LoopMc.WriteF8(r);

            // C-31: CORRECTING → PUBLISHED → CI_VERIFIED → REREVIEW_PENDING → ARCHITECT_INVOKED → ARCHITECT_SATISFIED, with every invariant at every point.
            for (var i = 0; i < points.Count; i++)
            {
                Clean(All(h, i == 0 ? null : points[i - 1].Point, i == 0 ? null : points[i - 1].Commit, points[i].Point, points[i].Commit, points[i].Commit),
                    "F.8 punto " + i);
                Assert.Equal("NONE", Y.S(points[i].Point.State, "orchestration.escalation.state"));
                Assert.Empty(Y.L(points[i].Point.State, "orchestration.autonomy_gaps"));
            }

            // C-32 (F4 part): the transport audit of the C-31 sequence on the custody finds no relay by the Owner.
            Assert.False(AutonomyGaps.OwnerAsMessageBus(points.Select(x => x.Point)));

            // C-29: a successor in a clean clone, with neither chat nor memory, reconstructs the loop at step 7 (CORRECTING) from the custodied state.
            var c = r.CleanClone("sucesor");
            var gitC = new GitProcessHistory(c);
            var s7 = r.Read(c, points[5].Commit);
            Assert.Empty(new StateV2Validator().ValidateFile(s7));
            Assert.Empty(new StateV2Validator().ValidateHistory(s7, gitC, points[5].Commit));
            var obj = Y.M(s7.State, "orchestration.loop.object")!;
            Assert.Equal(r.Subst[B1], gitC.BlobAt(Y.S(obj, "commit")!, Y.S(obj, "path")!));
            var request = Y.L(s7.State, "orchestration.review_requests").Cast<YamlMap>().Single();
            var attempt = Y.L(request, "attempts").Cast<YamlMap>().Single();
            var result = StateTreeReader.Json(s7.Tree, attempt["result"])!;
            Assert.Equal("CHANGES REQUIRED", J.S(result, "Verdict"));
            Assert.Equal(new[] { "LIN-1", "LIN-2" }, Y.L(s7.State, "orchestration.findings").Cast<YamlMap>()
                .Where(f => Y.S(f, "state") == "OPEN" && Y.S(f, "severity") == "REQUIRED").Select(f => Y.S(f, "lineage_id")));
            var entry = (YamlMap)Y.L(s7.State, "orchestration.architect_budgets")[0]!;
            var remaining = Orchestration.FrozenCaps.Keys.Where(k => entry.TryGetValue(k, out _))
                .ToDictionary(k => k, k => Y.N(entry, "caps." + k)!.Value - Y.N(entry, k)!.Value);
            Assert.Equal(2, remaining["review_rounds"]);
            Assert.Equal(8, remaining["architect_launches"]);
            var next = NextActionDerivation.Derive(s7).Single();
            Assert.Equal(("PRINCIPAL_COORDINATOR", "CORRECT_AND_REREVIEW"), (next.Role, next.Action));
            Assert.True(YamlSubset.DeepEquals(obj, next.Target));
            var (ambiguities, mismatches) = NextActionDerivation.Check(s7);
            Assert.Empty(ambiguities.Concat(mismatches));
        }
    }
}
