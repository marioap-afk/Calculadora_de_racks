#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// The sequence F.8 of Proposal V14 Anexo F (review loop of the Architect with a change of Principal) as synthetic durable points with the
    /// orchestration of B.8.8 and the AGREED A-1 (per-loop entry, loop identity), every artifact custodied in the tree of its point. It starts from the
    /// QU ORDINARY with record_version 5 of <see cref="StateV2Samples.CustodyHistory"/>. Test data only.
    /// </summary>
    public static class OrchestrationSamples
    {
        public const string C1 = "c1c1c1c1c1c1c1c1c1c1c1c1c1c1c1c1c1c1c1c1";
        public const string B1 = "b1b1b1b1b1b1b1b1b1b1b1b1b1b1b1b1b1b1b1b1";
        public const string C2 = "c2c2c2c2c2c2c2c2c2c2c2c2c2c2c2c2c2c2c2c2";
        public const string B2 = "b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2b2";
        public const string X = "docs/initiatives/I-99-proposal-v2.md";
        public const string Decisions = "docs/automation/decisions/I-99.md";
        public const string L1 = "L20261006T010101Z-0001";
        public const string L2 = "L20261006T020202Z-0002";

        public const string RlaEntry = "## ReviewLoopAuthorization RLA-1\n\n```text\nI62-REVIEW-LOOP-AUTHORIZATION: RLA-1\nRole: ARCHITECT\n"
            + "ContinuesLoopInstanceId: null\nValidity.Until: 2026-12-31T00:00:00Z\nClaim-Id: " + ClaimId + "\n```";

        /// <summary>The F.8 points, step by step: [base, 2, 3, 4, 6, 7, 9 (QR), 11, 12, 13, 14, 16, 17].</summary>
        public static List<StatePoint> F8()
        {
            var points = new List<StatePoint> { Point(5) };
            var entries = new List<string> { G0Entry };

            // Step 2: QU opening the loop on X {c1, b1}; r1 OPEN with a1.1 BUDGET_RESERVED and B materialized.
            var s2 = Next(points[^1]);
            entries.Add(RlaEntry);
            var decisions = WriteDecisions(s2, entries);
            var bindingB = MaterializedBinding(s2, "B20261006T010101Z-b001", decisions);
            var obj1 = Obj(C1, X, B1);
            var o = Orch(s2);
            o["loop"] = M(("type", "ARCHITECT_REVIEW"), ("phase", "REVIEW_PENDING"), ("instance_id", "ARL-6"), ("object", Clone(obj1)),
                ("authorization", Clone(decisions)), ("action_validity", Validity("RLA-1")));
            o["review_requests"] = L(Request(L1, obj1, 1, Attempt(s2, L1, 1, obj1, bindingB, reservedAt: 6, openFindings: new string[0], snapshot: (1, 1, 1, 0, 0))));
            o["architect_budgets"] = L(Entry("ARL-6", decisions, rounds: 1, requests: 1, launches: 1, reruns: new[] { (L1, 0L) }, corrections: 0));
            o["next_action"] = NextAction("ARCHITECT", "REVIEW", obj1, decisions);
            points.Add(s2);

            // Step 3: a1.1 LAUNCHING with run_id R1; ARCHITECT_INVOKED.
            var s3 = Next(points[^1]);
            var a11 = AttemptOf(s3, L1, 1);
            a11["state"] = "LAUNCHING";
            a11["run_id"] = "R20261006T030303Z-0001";
            a11["launching_utc"] = "2026-10-06T03:03:03Z";
            Loop(s3)["phase"] = "ARCHITECT_INVOKED";
            points.Add(s3);

            // Step 4: a1.1 LAUNCHED with its evidence.
            var s4 = Next(points[^1]);
            a11 = AttemptOf(s4, L1, 1);
            a11["state"] = "LAUNCHED";
            a11["launch_evidence"] = Tree(s4).Put("docs/automation/evidence/I-99-agent/review/L1/1/launch.json", "{\"Schema\": \"rackcad-relay-record/v2\"}\n");
            points.Add(s4);

            // Step 6: a1.1 RESULT_RECEIVED with res1 (CHANGES REQUIRED, R1 and R2) custodied.
            var s6 = Next(points[^1]);
            a11 = AttemptOf(s6, L1, 1);
            var res1 = ArchitectResult(s6, "docs/automation/evidence/I-99-agent/review/L1/1/result.json", L1, obj1, "CHANGES REQUIRED",
                required: new[] { "A62-X-01", "A62-X-02" }, dispositions: new (string, string)[0]);
            Received(s6, a11, res1);
            points.Add(s6);

            // Step 7: a1.1 RESULT_INGESTED VALID; r1 INGESTED; L1 and L2 OPEN; CORRECTING.
            var s7 = Next(points[^1]);
            a11 = AttemptOf(s7, L1, 1);
            a11["state"] = "RESULT_INGESTED";
            a11["outcome"] = "VALID";
            a11["ingested_at"] = Rv(s7);
            RequestOf(s7, L1)["state"] = "INGESTED";
            Orch(s7)["findings"] = L(Lineage("LIN-1", "A62-X-01", res1), Lineage("LIN-2", "A62-X-02", res1));
            Loop(s7)["phase"] = "CORRECTING";
            Orch(s7)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "CORRECT_AND_REREVIEW", obj1);
            points.Add(s7);

            // Step 9: QR of the successor C (designation; no change in findings, requests or budgets).
            var s9 = Next(points[^1], "QR");
            entries.Add(DesignationEntry);
            WriteDecisions(s9, entries);
            var principal = (YamlMap)Y.M(s9.State, "custody.principal")!;
            principal["binding"] = Tree(s9).PutJson("docs/automation/evidence/I-99-agent/qr/binding.json", new JsonObject
            {
                ["Schema"] = "rackcad-binding/v1", ["BindingId"] = "B20261005T010101Z-ef56", ["UnitId"] = Unit, ["Scope"] = "UNIT", ["TaskId"] = null,
                ["Role"] = "PRINCIPAL_COORDINATOR", ["Acceptance"] = new JsonObject { ["State"] = "ACCEPTED", ["Basis"] = "INDIVIDUAL_DECISION" },
            });
            principal["preflight"] = Tree(s9).Put("docs/automation/evidence/I-99-agent/qr/preflight.json", "{\"Schema\": \"rackcad-preflight/v1\", \"Action\": \"CUSTODY\"}\n");
            principal["designation"] = Clone((YamlMap)Y.Get(s9.State, "protocol.g0_acceptance.decision")!);
            principal["since_record_version"] = Rv(s9);
            points.Add(s9);

            // Step 11: PUBLISHED with loop.object = X2 {c2, b2}; correction_rounds 1; corrections_by_lineage L1 = L2 = 1; responses custodied.
            var s11 = Next(points[^1]);
            var obj2 = Obj(C2, X, B2);
            Loop(s11)["phase"] = "PUBLISHED";
            Loop(s11)["object"] = Clone(obj2);
            var e = (YamlMap)Y.L(s11.State, "orchestration.architect_budgets")[0]!;
            e["correction_rounds"] = 1L;
            e["corrections_by_lineage"] = L(M(("lineage_id", "LIN-1"), ("count", 1L)), M(("lineage_id", "LIN-2"), ("count", 1L)));
            foreach (var f in Y.L(s11.State, "orchestration.findings").Cast<YamlMap>())
            {
                f["response"] = Tree(s11).Put("docs/automation/evidence/I-99-agent/review/responses/" + Y.S(f, "lineage_id") + ".md", "corrige " + Y.S(f, "lineage_id") + "\n");
            }

            Orch(s11)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "VERIFY_CI", obj2);
            points.Add(s11);

            // Step 12: CI_VERIFIED.
            var s12 = Next(points[^1]);
            Loop(s12)["phase"] = "CI_VERIFIED";
            points.Add(s12);

            // Step 13: REREVIEW_PENDING; r2 OPEN on X2; a2.1 BUDGET_RESERVED with D materialized; OpenFindings {LIN-1, LIN-2}.
            var s13 = Next(points[^1]);
            var bindingD = MaterializedBinding(s13, "B20261006T131313Z-d001", (YamlMap)Loop(s13)["authorization"]!);
            Loop(s13)["phase"] = "REREVIEW_PENDING";
            Y.L(s13.State, "orchestration.review_requests").Add(Request(L2, obj2, 2,
                Attempt(s13, L2, 1, obj2, bindingD, reservedAt: Rv(s13), openFindings: new[] { "LIN-1", "LIN-2" }, snapshot: (2, 2, 2, 0, 1))));
            e = (YamlMap)Y.L(s13.State, "orchestration.architect_budgets")[0]!;
            e["review_rounds"] = 2L;
            e["logical_requests"] = 2L;
            e["architect_launches"] = 2L;
            Y.L(e, "transport_reruns").Add(M(("logical_review_request_id", L2), ("count", 0L)));
            Orch(s13)["next_action"] = NextAction("ARCHITECT", "REVIEW", obj2, (YamlMap)Loop(s13)["authorization"]!);
            points.Add(s13);

            // Step 14: a2.1 LAUNCHING with run_id R2; ARCHITECT_INVOKED.
            var s14 = Next(points[^1]);
            var a21 = AttemptOf(s14, L2, 1);
            a21["state"] = "LAUNCHING";
            a21["run_id"] = "R20261006T141414Z-0002";
            a21["launching_utc"] = "2026-10-06T14:14:14Z";
            Loop(s14)["phase"] = "ARCHITECT_INVOKED";
            points.Add(s14);

            // Step 16: a2.1 RESULT_RECEIVED with res2 (AGREED, L1 and L2 CLOSED on b2).
            var s16 = Next(points[^1]);
            a21 = AttemptOf(s16, L2, 1);
            var res2 = ArchitectResult(s16, "docs/automation/evidence/I-99-agent/review/L2/1/result.json", L2, obj2, "AGREED",
                required: new string[0], dispositions: new[] { ("A62-X-01", "CLOSED"), ("A62-X-02", "CLOSED") });
            Received(s16, a21, res2);
            points.Add(s16);

            // Step 17: a2.1 RESULT_INGESTED; LIN-1 and LIN-2 CLOSED by res2; ARCHITECT_SATISFIED on b2; the validity ends with it.
            var s17 = Next(points[^1]);
            a21 = AttemptOf(s17, L2, 1);
            a21["state"] = "RESULT_INGESTED";
            a21["outcome"] = "VALID";
            a21["ingested_at"] = Rv(s17);
            RequestOf(s17, L2)["state"] = "INGESTED";
            foreach (var f in Y.L(s17.State, "orchestration.findings").Cast<YamlMap>())
            {
                f["state"] = "CLOSED";
                f["closed_by"] = Clone(res2);
                f["last_disposition_in"] = Clone(res2);
            }

            Loop(s17)["phase"] = "ARCHITECT_SATISFIED";
            End((YamlMap)Loop(s17)["action_validity"]!, "ARCHITECT_SATISFIED", Rv(s17), null);
            End((YamlMap)Y.L((YamlMap)Y.L(s17.State, "orchestration.architect_budgets")[0]!, "authorizations")[0]!, "ARCHITECT_SATISFIED", Rv(s17), null);
            Orch(s17)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "NEXT_GATE", obj2);
            points.Add(s17);
            return points;
        }

        public static StatePoint F8Step(int step)
        {
            var steps = new[] { 0, 2, 3, 4, 6, 7, 9, 11, 12, 13, 14, 16, 17 };
            return F8()[System.Array.IndexOf(steps, step)];
        }

        // ------------------------------------------------------------------ builders

        public static StatePoint Next(StatePoint prev, string point = "QU")
        {
            var c = Copy(prev);
            var custody = (YamlMap)c.State["custody"]!;
            custody["record_version"] = Rv(prev) + 1;
            custody["point"] = point;
            custody["point_kind"] = "ORDINARY";
            return c;
        }

        public static long Rv(StatePoint p) => Y.N(p.State, "custody.record_version")!.Value;

        public static YamlMap Orch(StatePoint p) => (YamlMap)p.State["orchestration"]!;

        public static YamlMap Loop(StatePoint p) => (YamlMap)Orch(p)["loop"]!;

        public static InMemoryStateTree Tree(StatePoint p) => (InMemoryStateTree)p.Tree;

        public static YamlMap RequestOf(StatePoint p, string id) =>
            Y.L(p.State, "orchestration.review_requests").Cast<YamlMap>().Single(r => Y.S(r, "logical_review_request_id") == id);

        public static YamlMap AttemptOf(StatePoint p, string id, long seq) => Y.L(RequestOf(p, id), "attempts").Cast<YamlMap>().Single(a => Y.N(a, "attempt_seq") == seq);

        /// <summary>Rewrites the unit's decisions file and refreshes every StateRef of the state that points to it (I-S13).</summary>
        public static YamlMap WriteDecisions(StatePoint p, IEnumerable<string> entries)
        {
            var r = Tree(p).Put(Decisions, StateV2Samples.Decisions(entries.ToArray()));
            foreach (var (_, stateRef) in Y.StateRefs(p.State, "$").ToList())
            {
                if (Y.S(stateRef, "path") == Decisions)
                {
                    stateRef["blob"] = r["blob"];
                }
            }

            foreach (var reference in AllMaps(p.State).Where(m => StateV2Shape.IsStateRef(m) && Y.S(m, "path") == Decisions))
            {
                reference["blob"] = r["blob"];
            }

            return r;
        }

        private static IEnumerable<YamlMap> AllMaps(object? node)
        {
            if (node is YamlMap m)
            {
                yield return m;
                foreach (var kv in m)
                {
                    foreach (var x in AllMaps(kv.Value))
                    {
                        yield return x;
                    }
                }
            }
            else if (node is List<object?> l)
            {
                foreach (var item in l)
                {
                    foreach (var x in AllMaps(item))
                    {
                        yield return x;
                    }
                }
            }
        }

        public static YamlMap Validity(string id) => M(("authorization_id", id), ("state", "OPEN"), ("ended_at", null), ("ended_utc", null), ("ended_reason", null),
            ("ended_by", null));

        public static void End(YamlMap validity, string reason, long at, YamlMap? by)
        {
            validity["state"] = "ENDED";
            validity["ended_at"] = at;
            validity["ended_utc"] = "2026-10-06T17:17:17Z";
            validity["ended_reason"] = reason;
            validity["ended_by"] = by;
        }

        public static YamlMap NextAction(string role, string action, YamlMap? target, YamlMap? permission = null) => M(("role", role), ("action", action),
            ("target", target == null ? null : Clone(target)), ("unit", Unit), ("gate", "F4"), ("task_id", null), ("required_inputs", L()), ("required_capabilities", L()),
            ("required_independence", "NONE"), ("invocation_permission", permission == null ? null : Clone(permission)), ("budget_remaining", M()),
            ("expected_output", role == "REVIEWER" ? "rackcad-reviewer-result/v1" : "rackcad-architect-review-result/v1"),
            ("completion_condition", "RESULT_INGESTED"), ("stop_conditions", L("P-18", "P-19")), ("escalation_conditions", L()));

        public static YamlMap Entry(string iid, YamlMap authorization, long rounds, long requests, long launches, (string, long)[] reruns, long corrections) => M(
            ("loop_instance_id", iid),
            ("authorizations", L(M(("authorization_id", "RLA-1"), ("authorization", Clone(authorization)), ("state", "OPEN"), ("ended_at", null), ("ended_utc", null),
                ("ended_reason", null), ("ended_by", null)))),
            ("review_rounds", rounds), ("logical_requests", requests), ("architect_launches", launches),
            ("transport_reruns", new List<object?>(reruns.Select(r => (object?)M(("logical_review_request_id", r.Item1), ("count", r.Item2))))),
            ("correction_rounds", corrections), ("corrections_by_lineage", L()),
            ("caps", M(("review_rounds", 3L), ("logical_requests", 3L), ("transport_reruns_per_request", 2L), ("corrections_per_lineage", 2L), ("correction_rounds", 2L),
                ("architect_launches", 9L))),
            ("closed_at", null), ("closed_by", null));

        public static YamlMap Request(string id, YamlMap obj, long round, YamlMap attempt) => M(("logical_review_request_id", id), ("loop_instance_id", "ARL-6"),
            ("object", Clone(obj)), ("round", round), ("state", "OPEN"), ("attempts", L(attempt)));

        public static YamlMap MaterializedBinding(StatePoint p, string id, YamlMap authorization) => Tree(p).PutJson("docs/automation/evidence/I-99-agent/review/" + id + ".json",
            new JsonObject
            {
                ["Schema"] = "rackcad-binding/v1", ["BindingId"] = id, ["UnitId"] = Unit, ["Scope"] = "UNIT", ["TaskId"] = null, ["Role"] = "ARCHITECT",
                ["Acceptance"] = new JsonObject
                {
                    ["State"] = "ACCEPTED", ["Basis"] = "AUTHORIZED_MATERIALIZATION", ["DecisionRef"] = null,
                    ["AuthorizationRef"] = new JsonObject
                    {
                        ["Path"] = Decisions, ["Marker"] = "I62-REVIEW-LOOP-AUTHORIZATION", ["AuthorizationId"] = "RLA-1", ["Commit"] = Sha,
                        ["Blob"] = Y.S(authorization, "blob"),
                    },
                    ["MaterializationCheck"] = new JsonObject
                    {
                        ["Criteria"] = new JsonArray(new JsonObject { ["CriterionId"] = "MinimumCapabilities", ["Required"] = "MATCH", ["Observed"] = "MATCH",
                            ["Result"] = "SATISFIED", ["Evidence"] = "preflight" }),
                    },
                },
            });

        public static YamlMap Attempt(StatePoint p, string request, long seq, YamlMap target, YamlMap binding, long reservedAt, string[] openFindings,
            (long Rounds, long Requests, long Launches, long Reruns, long Corrections) snapshot)
        {
            var dir = "docs/automation/evidence/I-99-agent/review/" + request + "/" + seq + "/";
            var invocationId = "I20261006T" + reservedAt.ToString("000000", System.Globalization.CultureInfo.InvariantCulture) + "Z-" + seq.ToString("0000", System.Globalization.CultureInfo.InvariantCulture);
            var invocation = Tree(p).PutJson(dir + "invocation.json", InvocationJson(invocationId, request, seq, target, openFindings, snapshot));
            var fidelity = Tree(p).PutJson(dir + "input-fidelity.json", new JsonObject
            {
                ["Schema"] = "rackcad-input-fidelity/v1", ["Kind"] = "FIDELITY", ["InvocationId"] = invocationId,
                ["Fidelity"] = new JsonObject { ["Phase"] = "PREFLIGHT", ["FidelityStatus"] = "FAITHFUL" },
            });
            return M(("attempt_seq", seq), ("invocation", invocation), ("binding", Clone(binding)), ("state", "BUDGET_RESERVED"), ("reserved_at", reservedAt),
                ("run_id", null), ("launch_evidence", null), ("not_started_evidence", null), ("result", null), ("output_state", null), ("runtime_evidence", null),
                ("read_audit", null), ("input_fidelity", fidelity), ("fidelity_status", null), ("unaccredited", L()), ("premise_independence", null),
                ("reserved_utc", "2026-10-06T01:01:01Z"), ("launching_utc", null), ("outcome", null), ("ingested_at", null));
        }

        public static JsonObject InvocationJson(string invocationId, string request, long seq, YamlMap target, string[] openFindings,
            (long Rounds, long Requests, long Launches, long Reruns, long Corrections) snapshot) => new JsonObject
        {
            ["Schema"] = "rackcad-role-invocation/v1", ["InvocationId"] = invocationId, ["LogicalReviewRequestId"] = request, ["AttemptSeq"] = seq,
            ["RequestedRole"] = "ARCHITECT", ["Action"] = "REVIEW_DESIGN",
            ["Target"] = new JsonObject { ["commit"] = Y.S(target, "commit"), ["path"] = Y.S(target, "path"), ["blob"] = Y.S(target, "blob") },
            ["AuthorityRevision"] = Sha,
            ["OpenFindings"] = new JsonArray(openFindings.Select(l => (JsonNode)new JsonObject
                { ["FindingId"] = l == "LIN-1" ? "A62-X-01" : "A62-X-02", ["LineageId"] = l, ["Severity"] = "REQUIRED", ["Issuer"] = "ARCHITECT" }).ToArray()),
            ["BudgetSnapshot"] = new JsonObject
            {
                ["ReviewRounds"] = snapshot.Rounds, ["LogicalRequests"] = snapshot.Requests, ["ArchitectLaunches"] = snapshot.Launches,
                ["TransportReruns"] = snapshot.Reruns, ["CorrectionRounds"] = snapshot.Corrections, ["CorrectionsByLineage"] = new JsonArray(),
            },
        };

        public static YamlMap ArchitectResult(StatePoint p, string path, string request, YamlMap obj, string verdict, string[] required, (string Id, string State)[] dispositions) =>
            Tree(p).PutJson(path, new JsonObject
            {
                ["Schema"] = "rackcad-architect-review-result/v1", ["ResultId"] = "RES-" + request, ["LogicalReviewRequestId"] = request, ["AttemptSeq"] = 1,
                ["RequestedRole"] = "ARCHITECT", ["Action"] = "REVIEW_DESIGN", ["ReviewedUnit"] = Unit, ["ReviewedCommit"] = Y.S(obj, "commit"),
                ["ReviewedPath"] = Y.S(obj, "path"), ["ReviewedBlob"] = Y.S(obj, "blob"), ["Verdict"] = verdict,
                ["RequiredFindings"] = new JsonArray(required.Select(id => (JsonNode)new JsonObject { ["FindingId"] = id }).ToArray()),
                ["FindingDispositions"] = new JsonArray(dispositions.Select(d => (JsonNode)new JsonObject { ["FindingId"] = d.Id, ["State"] = d.State }).ToArray()),
            });

        public static void Received(StatePoint p, YamlMap attempt, YamlMap result)
        {
            var dir = Y.S(result, "path")!.Substring(0, Y.S(result, "path")!.LastIndexOf('/') + 1);
            attempt["state"] = "RESULT_RECEIVED";
            attempt["result"] = Clone(result);
            attempt["output_state"] = "PRESENT";
            attempt["runtime_evidence"] = Tree(p).Put(dir + "runtime-evidence.json", "{\"ObservedActor\": \"other\"}\n");
            attempt["read_audit"] = Tree(p).Put(dir + "read-audit.json", "{\"Reads\": []}\n");
            attempt["fidelity_status"] = "FAITHFUL";
        }

        public static YamlMap Lineage(string id, string findingId, YamlMap openedIn) => M(("lineage_id", id), ("finding_ids", L(findingId)), ("issuer", "ARCHITECT"),
            ("opened_in", Clone(openedIn)), ("severity", "REQUIRED"), ("class", "defecto"), ("affected_section", "§1"), ("state", "OPEN"), ("response", null),
            ("last_disposition_in", null), ("omitted_in", L()), ("closed_by", null), ("downgraded_by", null));
    }
}
