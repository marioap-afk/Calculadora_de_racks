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
    /// I-62 F4 (slice F4-H), C-29 (reproducible control, run as a guard): a crash at each boundary of F.8 is recovered by a successor in a clean clone,
    /// without transient artifacts, from the custodied state alone — (A) after BUDGET_RESERVED it resumes the same attempt without a new launch; (B) after
    /// LAUNCHING it never relaunches before resolving the start: a proven non-start replans the same reservation and an unaccreditable termination is
    /// LAUNCH_UNCERTAIN, counted; (C) after LAUNCHED with the result outside Git it custodies and ingests the result once (idempotent); (D) after
    /// RESULT_RECEIVED or RESULT_INGESTED it never recounts. At every boundary the successor also reconstructs the fidelity evidence and the historical
    /// accreditation of the binding from the custody.
    /// </summary>
    public class I62F4LoopRecoveryMcTests
    {
        // Indexes of the F.8 points: [base, s2, s3, s4, s6, s7, s9, s11, s12, s13, s14, s16, s17].
        private const int S2 = 1, S3 = 2, S4 = 3, S6 = 4, S7 = 5, S9 = 6;

        [Fact]
        public void I62_C29_ACrashAtEachBoundaryOfF8IsRecoveredByASuccessorFromTheCustodyAlone()
        {
            using var r = new CustodyRepo();
            var (pts, f8) = LoopMc.WriteF8(r);
            var c = r.CleanClone("sucesor");
            var git = new GitProcessHistory(c);
            StatePoint At(int i) => r.Read(c, pts[i].Commit);

            List<string> Recover(int boundary, StatePoint next, string message)
            {
                r.Git(c, "reset", "-q", "--hard", pts[boundary].Commit);
                var k = r.WritePoint(c, next, message, push: false);
                return Ids(All(c, At(boundary), pts[boundary].Commit, r.Read(c, k), k, k));
            }

            // At every boundary the custody alone is complete: every invariant holds, the fidelity evidence and the binding's AuthorizationRef resolve.
            foreach (var i in new[] { S2, S3, S4, S6, S7 })
            {
                var p = At(i);
                Clean(new StateV2Validator().ValidateFile(p).Concat(new StateV2Validator().ValidateHistory(p, git, pts[i].Commit)).ToList(), "boundary " + i);
                var a = AttemptOf(p, L1, 1);
                Assert.NotNull(StateTreeReader.Json(p.Tree, a["input_fidelity"]));
                Assert.NotNull(StateTreeReader.Json(p.Tree, a["binding"])?["Acceptance"]?["AuthorizationRef"]);
            }

            // (A) After BUDGET_RESERVED: the same attempt goes on to LAUNCHING; a new reservation instead would be a second live attempt.
            Assert.Empty(Recover(S2, f8[S3], "I-99: C lanza el intento reservado"));
            var second = Next(f8[S2]);
            Y.L(RequestOf(second, L1), "attempts").Add(Attempt(second, L1, 2, Obj(C1, X, B1), (YamlMap)AttemptOf(second, L1, 1)["binding"]!, Rv(second), new string[0],
                (1, 1, 2, 1, 0)));
            Assert.Contains("I-S18", Recover(S2, second, "I-99: C reserva otro intento"));

            // (B) After LAUNCHING, a non-start proven by the invoker: the same reservation is replanned with a new invocation, no new launch counted.
            var notStarted = Next(f8[S3]);
            var b = AttemptOf(notStarted, L1, 1);
            var invocation = InvocationJson("I20261006T999999Z-0001", L1, 1, Obj(C1, X, B1), new string[0], (1, 1, 1, 0, 0));
            b["invocation"] = Tree(notStarted).PutJson("docs/automation/evidence/I-99-agent/review/" + L1 + "/1/invocation-replanned.json", invocation);
            b["input_fidelity"] = Tree(notStarted).PutJson("docs/automation/evidence/I-99-agent/review/" + L1 + "/1/input-fidelity-replanned.json", new JsonObject
            {
                ["Schema"] = "rackcad-input-fidelity/v1", ["Kind"] = "FIDELITY", ["InvocationId"] = "I20261006T999999Z-0001",
                ["Fidelity"] = new JsonObject { ["Phase"] = "PREFLIGHT", ["FidelityStatus"] = "FAITHFUL" },
            });
            b["state"] = "BUDGET_RESERVED";
            b["run_id"] = null;
            b["launching_utc"] = null;
            b["not_started_evidence"] = Tree(notStarted).Put("docs/automation/evidence/I-99-agent/review/" + L1 + "/1/not-started.json",
                "{\"Schema\": \"rackcad-relay-record/v2\", \"Started\": false}\n");
            Loop(notStarted)["phase"] = "REVIEW_PENDING";
            Assert.Empty(Recover(S3, notStarted, "I-99: C replanifica tras no arranque probado"));

            // (B) After LAUNCHING, termination not accreditable: LAUNCH_UNCERTAIN, counted conservatively; relaunching it is not a recovery.
            var uncertain = Next(f8[S3]);
            AttemptOf(uncertain, L1, 1)["state"] = "LAUNCH_UNCERTAIN";
            AttemptOf(uncertain, L1, 1)["outcome"] = "UNCERTAIN";
            Loop(uncertain)["phase"] = "REVIEW_PENDING";
            Assert.Empty(Recover(S3, uncertain, "I-99: C registra LAUNCH_UNCERTAIN"));
            Assert.Equal(1L, Y.N((YamlMap)Y.L(uncertain.State, "orchestration.architect_budgets")[0]!, "architect_launches"));
            var relaunched = Next(uncertain);
            AttemptOf(relaunched, L1, 1)["state"] = "LAUNCHING";
            AttemptOf(relaunched, L1, 1)["outcome"] = null;
            Loop(relaunched)["phase"] = "ARCHITECT_INVOKED";
            Assert.Contains("I-P13", Ids(new StateV2Validator().ValidatePair(uncertain, relaunched)));

            // (C) After LAUNCHED with the result outside Git: C custodies it (RESULT_RECEIVED) and ingests it once; a second ingestion with another blob is S-04.
            Assert.Empty(Recover(S4, f8[S6], "I-99: C custodia el resultado"));
            Assert.Empty(Recover(S6, f8[S7], "I-99: C ingiere el resultado"));
            var reingested = Next(f8[S7]);
            AttemptOf(reingested, L1, 1)["result"] = Tree(reingested).Put("docs/automation/evidence/I-99-agent/review/L1/1/result-bis.json", "{\"Schema\": \"x\"}\n");
            Assert.Contains("I-P13", Recover(S7, reingested, "I-99: segunda ingestión"));

            // (D) After RESULT_RECEIVED or RESULT_INGESTED nothing is recounted; after the ingestion the successor takes custody with its QR (F.8 step 9).
            var recounted = Copy(f8[S7]);
            ((YamlMap)Y.L(recounted.State, "orchestration.architect_budgets")[0]!)["architect_launches"] = 2L;
            Assert.Contains("I-P13", Recover(S6, recounted, "I-99: ingestión que recuenta"));
            Assert.Empty(Recover(S7, f8[S9], "I-99: QR del sucesor"));
        }
    }
}
