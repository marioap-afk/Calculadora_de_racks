#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using static RackCad.Tests.OrchestrationSamples;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// A REVIEWER loop (OBS-A1-01 of the AGREED A-1, D1-16..D1-21) authorized by its gate contract, without ReviewLoopAuthorization or per-loop entry:
    /// open → LAUNCHING → RESULT_RECEIVED → RESULT_INGESTED with REVIEWER_SATISFIED → LOOP_CLOSED (S). Starts from the QU with record_version 5 of the
    /// custody history. Test data only.
    /// </summary>
    public static class ReviewerSamples
    {
        public const string R1 = "L20261007T010101Z-r001";
        public const string ContractPath = "docs/automation/evidence/I-99-agent/T-02/gate-contract.json";

        public static YamlMap Contract(StatePoint p, string task = "T-02") => Tree(p).PutJson(ContractPath.Replace("T-02", task, System.StringComparison.Ordinal), new JsonObject
        {
            ["Schema"] = "rackcad-gate-contract/v2", ["TaskId"] = task,
            ["RoleRequirements"] = new JsonArray(new JsonObject { ["Role"] = "REVIEWER", ["Materialization"] = new JsonObject { ["Budget"] = 3 } }),
        });

        public static string AuthorityOf(YamlMap contractRef) => Orchestration.ReviewerAuthorityId(contractRef)!;

        /// <summary>The REVIEWER points: [base, open (6), LAUNCHING (7), RESULT_RECEIVED (8), REVIEWER_SATISFIED (9), LOOP_CLOSED (10)].</summary>
        public static List<StatePoint> Loop()
        {
            var points = new List<StatePoint> { Point(5) };
            var obj = Obj(C1, X, B1);

            var r1 = Next(points[^1]);
            var contract = Contract(r1);
            var binding = Tree(r1).PutJson("docs/automation/evidence/I-99-agent/review/reviewer-binding.json", new JsonObject
            {
                ["Schema"] = "rackcad-binding/v1", ["BindingId"] = "B20261007T010101Z-r001", ["UnitId"] = Unit, ["Scope"] = "TASK", ["TaskId"] = "T-02",
                ["Role"] = "REVIEWER", ["Acceptance"] = new JsonObject { ["State"] = "ACCEPTED", ["Basis"] = "INDIVIDUAL_DECISION" },
            });
            var o = Orch(r1);
            o["loop"] = M(("type", "REVIEWER"), ("phase", "REVIEW_PENDING"), ("instance_id", null), ("object", Clone(obj)), ("authorization", Clone(contract)),
                ("action_validity", Validity(AuthorityOf(contract))));
            var attempt = Attempt(r1, R1, 1, obj, binding, reservedAt: 6, openFindings: new string[0], snapshot: (1, 1, 1, 0, 0));
            o["review_requests"] = L(M(("logical_review_request_id", R1), ("loop_instance_id", null), ("object", Clone(obj)), ("round", 1L), ("state", "OPEN"),
                ("attempts", L(attempt))));
            var b = (YamlMap)o["budgets"]!;
            b["review_rounds"] = 1L;
            b["logical_requests"] = 1L;
            b["architect_launches"] = 1L;
            b["transport_reruns"] = L(M(("logical_review_request_id", R1), ("count", 0L)));
            o["next_action"] = NextAction("REVIEWER", "REVIEW_CHANGE", obj, contract);
            points.Add(r1);

            var r2 = Next(points[^1]);
            var a = AttemptOf(r2, R1, 1);
            a["state"] = "LAUNCHING";
            a["run_id"] = "R20261007T020202Z-r001";
            a["launching_utc"] = "2026-10-07T02:02:02Z";
            OrchestrationSamples.Loop(r2)["phase"] = "ARCHITECT_INVOKED";
            points.Add(r2);

            var r3 = Next(points[^1]);
            a = AttemptOf(r3, R1, 1);
            Received(r3, a, ReviewerResult(r3, "NO_FINDINGS", obj));
            points.Add(r3);

            var r4 = Next(points[^1]);
            a = AttemptOf(r4, R1, 1);
            a["state"] = "RESULT_INGESTED";
            a["outcome"] = "VALID";
            a["ingested_at"] = Rv(r4);
            RequestOf(r4, R1)["state"] = "INGESTED";
            OrchestrationSamples.Loop(r4)["phase"] = "REVIEWER_SATISFIED";
            End((YamlMap)OrchestrationSamples.Loop(r4)["action_validity"]!, "REVIEWER_SATISFIED", Rv(r4), null);
            Orch(r4)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "NEXT_GATE", null);
            points.Add(r4);

            var r5 = Next(points[^1]);
            var loop = OrchestrationSamples.Loop(r5);
            var record = M(("last_request", R1), ("authorization", Clone((YamlMap)loop["authorization"]!)), ("validity", Clone((YamlMap)loop["action_validity"]!)),
                ("closed_at", Rv(r5)), ("closed_by", null));
            Y.L(r5.State, "orchestration.reviewer_closures").Add(record);
            loop["type"] = "NONE";
            loop["phase"] = "NONE";
            loop["object"] = null;
            loop["authorization"] = null;
            loop["action_validity"] = null;
            points.Add(r5);
            return points;
        }

        public static YamlMap ReviewerResult(StatePoint p, string disposition, YamlMap obj, params (string Id, string Severity)[] findings) =>
            Tree(p).PutJson("docs/automation/evidence/I-99-agent/review/" + R1 + "/1/result.json", new JsonObject
            {
                ["Schema"] = "rackcad-reviewer-result/v1", ["ResultId"] = "RES-" + R1, ["LogicalReviewRequestId"] = R1, ["AttemptSeq"] = 1, ["RequestedRole"] = "REVIEWER",
                ["Action"] = "REVIEW_CHANGE", ["ReviewedUnit"] = Unit, ["ReviewedCommit"] = Y.S(obj, "commit"), ["ReviewedPath"] = Y.S(obj, "path"),
                ["ReviewedBlob"] = Y.S(obj, "blob"), ["Disposition"] = disposition,
                ["Findings"] = new JsonArray(findings.Select(f => (JsonNode)new JsonObject { ["FindingId"] = f.Id, ["Severity"] = f.Severity }).ToArray()),
                ["FindingDispositions"] = new JsonArray(),
            });
    }
}
