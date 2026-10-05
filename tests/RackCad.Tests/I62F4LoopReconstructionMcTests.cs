#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private const string Auth1 = "a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a1";
        private const string Auth2 = "a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2a2";

        [Fact]
        public void I62_C29_ASuccessorInACleanCloneReconstructsTheArchitectLoopFromTheCustodiedStateOnly()
        {
            using var r = new CustodyRepo();
            var h = r.Holder;
            r.Subst[Sha] = r.Main0;
            r.Subst[DigitSha] = r.Main0;
            r.Subst[Sha3] = r.Commit(h, "T-01 RED", (RedFile, "rojo\n"));
            r.Subst[Sha2] = r.Commit(h, "T-01 GREEN", (GreenFile, "verde\n"));

            // The AuthorizationRef of each materialized binding cites the commit where its authorization was already custodied (never the custody point).
            var f8 = F8();
            foreach (var p in f8)
            {
                var tree = (InMemoryStateTree)p.Tree;
                foreach (var (path, bytes) in tree.Files.ToList().Where(f => f.Key.Contains("/review/B2026", StringComparison.Ordinal)))
                {
                    var token = path.Contains("-b001", StringComparison.Ordinal) ? Auth1 : Auth2;
                    tree.Put(path, Encoding.UTF8.GetString(bytes).Replace("\"Commit\": \"" + Sha + "\"", "\"Commit\": \"" + token + "\"", StringComparison.Ordinal));
                }
            }

            var points = new List<(StatePoint Point, string Commit)>();
            void Write(int i)
            {
                var k = r.WritePoint(h, f8[i], "I-99: F.8 punto " + i, push: false);
                points.Add((r.Read(h, k), k));
            }

            Write(0);
            ((InMemoryStateTree)f8[1].Tree).TryRead(OrchestrationSamples.Decisions, out var rla);
            r.G.Write(h, OrchestrationSamples.Decisions, Encoding.UTF8.GetString(rla));
            r.Subst[Auth1] = r.G.CommitAll(h, "I-99: ReviewLoopAuthorization RLA-1");
            r.G.Write(h, X, "# I-99 propuesta v1\n");
            r.Subst[C1] = r.G.CommitAll(h, "I-99: X v1");
            r.Subst[B1] = r.Git(h, "rev-parse", r.Subst[C1] + ":" + X);
            for (var i = 1; i <= 6; i++)
            {
                Write(i);
            }

            r.Subst[Auth2] = points[^1].Commit;
            r.G.Write(h, X, "# I-99 propuesta v2 (corrige LIN-1 y LIN-2)\n");
            r.Subst[C2] = r.G.CommitAll(h, "I-99: X v2");
            r.Subst[B2] = r.Git(h, "rev-parse", r.Subst[C2] + ":" + X);
            for (var i = 7; i < f8.Count; i++)
            {
                Write(i);
            }

            r.Git(h, "push", "-q", "origin", "HEAD");

            // C-31: CORRECTING → PUBLISHED → CI_VERIFIED → REREVIEW_PENDING → ARCHITECT_INVOKED → ARCHITECT_SATISFIED, with every invariant at every point.
            for (var i = 0; i < points.Count; i++)
            {
                Clean(All(h, i == 0 ? null : points[i - 1].Point, i == 0 ? null : points[i - 1].Commit, points[i].Point, points[i].Commit, points[i].Commit),
                    "F.8 punto " + i);
                Assert.Equal("NONE", Y.S(points[i].Point.State, "orchestration.escalation.state"));
                Assert.Empty(Y.L(points[i].Point.State, "orchestration.autonomy_gaps"));
            }

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
