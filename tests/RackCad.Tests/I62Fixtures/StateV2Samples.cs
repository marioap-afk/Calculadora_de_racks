#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;

namespace RackCad.Tests
{
    /// <summary>
    /// Synthetic <c>rackcad-automation-state/v2</c> points in the shape of Proposal V14 B.8 plus the AGREED amendment A-1 (commit ca09ade8, blob
    /// c01899a7). Every StateRef resolves in the tree of its point (I-S13): the artifacts are written with real Git blob ids. Test data only.
    /// </summary>
    public static class StateV2Samples
    {
        public const string Sha = "4c617e82b32b6c810b68d75fc19472efed22b393";
        public const string Sha2 = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        public const string Sha3 = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
        public const string Img2 = "cccccccccccccccccccccccccccccccccccccccc";
        public const string Img3 = "dddddddddddddddddddddddddddddddddddddddd";
        public const string DigitSha = "1234567890123456789012345678901234567890";
        public const string ClaimId = "5b661a17-8c18-4183-8554-3866059cba2b";
        public const string PrincipalBindingId = "B20261003T010101Z-ab12";
        public const string Unit = "I-99";

        public static YamlMap M(params (string Key, object? Value)[] entries)
        {
            var map = new YamlMap();
            foreach (var (key, value) in entries)
            {
                map.Add(key, value);
            }

            return map;
        }

        public static List<object?> L(params object?[] items) => new List<object?>(items);

        public static YamlMap Obj(string commit, string path, string blob) => M(("commit", commit), ("path", path), ("blob", blob));

        public static YamlMap Clone(YamlMap m) => (YamlMap)YamlSubset.DeepClone(m)!;

        /// <summary>
        /// The decision entries of the unit's <c>decisions/I-99.md</c>, in order. Each point writes the file with the entries published up to it, so the
        /// decision StateRefs of a later point carry the later blob (I-S13).
        /// </summary>
        public static string Decisions(params string[] entries)
        {
            var sb = new StringBuilder("# Decisiones de I-99\n\nClaim-Id: " + ClaimId + "\n");
            foreach (var e in entries)
            {
                sb.Append('\n').Append(e).Append('\n');
            }

            return sb.ToString();
        }

        public const string G0Entry = "## G0\n\n```text\nI62-DELEGATED-EXECUTION: I62_DELEGATED\nI62-CLASSIFICATION: I62\nI62-PRINCIPAL-BINDING: "
            + PrincipalBindingId + " ACCEPTED\nClaim-Id: " + ClaimId + "\nBootstrapRecordVersion: 1\n```";

        /// <summary>A world: the artifacts of one point, written into its tree with real blob ids.</summary>
        public sealed class World
        {
            public InMemoryStateTree Tree { get; } = new InMemoryStateTree();

            public YamlMap File(string path, string text) => Tree.Put(path, text);

            public YamlMap Binding(string path, string bindingId, string role, string scope, string acceptance)
            {
                var json = new JsonObject
                {
                    ["Schema"] = "rackcad-binding/v1",
                    ["BindingId"] = bindingId,
                    ["UnitId"] = Unit,
                    ["Scope"] = scope,
                    ["TaskId"] = null,
                    ["Role"] = role,
                    ["Acceptance"] = new JsonObject
                    {
                        ["State"] = acceptance,
                        ["Basis"] = acceptance == "PENDING" ? null : "INDIVIDUAL_DECISION",
                        ["DecisionRef"] = acceptance == "PENDING" ? null : new JsonObject { ["Path"] = "docs/automation/decisions/I-99.md", ["Marker"] = "I62-PRINCIPAL-BINDING" },
                        ["AuthorizationRef"] = null,
                        ["MaterializationCheck"] = null,
                    },
                };
                return Tree.PutJson(path, json);
            }
        }

        private static YamlMap NoLoop(World w) => M(
            ("loop", M(("type", "NONE"), ("phase", "NONE"), ("instance_id", null), ("object", null), ("authorization", null), ("action_validity", null))),
            ("next_action", M(("role", "PRINCIPAL_COORDINATOR"), ("action", "CONTINUE"), ("target", null), ("unit", Unit), ("gate", "F4"), ("task_id", null),
                ("required_inputs", L()), ("required_capabilities", L()), ("required_independence", "NONE"), ("invocation_permission", null),
                ("budget_remaining", M()), ("expected_output", "rackcad-automation-state/v2"), ("completion_condition", "QU"), ("stop_conditions", L()),
                ("escalation_conditions", L()))),
            ("review_requests", L()),
            ("findings", L()),
            ("architect_budgets", L()),
            ("budgets", M(("review_rounds", 0L), ("logical_requests", 0L), ("architect_launches", 0L), ("transport_reruns", L()), ("correction_rounds", 0L),
                ("corrections_by_lineage", L()), ("caps", w.File("docs/automation/evidence/I-99-agent/review/caps.json", "{\"MAX_REVIEW_ROUNDS\": 3}\n")))),
            ("reviewer_closures", L()),
            ("escalation", M(("state", "NONE"), ("reason", null), ("required_decision", null))),
            ("autonomy_gaps", L()));

        /// <summary>
        /// A unit's custody history with every kind of durable point: BOOTSTRAP (1) → QU acceptances (2) → Q0 (3) → Q7 VERIFIED (4) → QU ORDINARY (5)
        /// → QU REBASE_RECONCILIATION (6) → QH (7) → QR (8). Consecutive points satisfy every pair invariant; each point satisfies every file invariant.
        /// </summary>
        public static List<StatePoint> CustodyHistory()
        {
            var points = new List<StatePoint>();
            var bootstrap = Base(1, "BOOTSTRAP", null, "PENDING", false);
            points.Add(bootstrap);
            var accepted = Base(2, "QU", "ORDINARY", "ACCEPTED", true);
            points.Add(accepted);
            points.Add(WithWindow(Base(3, "Q0", null, "ACCEPTED", true), q0: true));
            points.Add(WithWindow(Base(4, "Q7", null, "ACCEPTED", true), q0: false));
            points.Add(WithWindow(Base(5, "QU", "ORDINARY", "ACCEPTED", true), q0: false));
            points.Add(Rebased(WithWindow(Base(6, "QU", "REBASE_RECONCILIATION", "ACCEPTED", true), q0: false)));
            points.Add(Released(Rebased(WithWindow(Base(7, "QH", null, "ACCEPTED", true), q0: false))));
            points.Add(Recovered(Rebased(WithWindow(Base(8, "QR", "ORDINARY", "ACCEPTED", true), q0: false))));
            return points;
        }

        private static StatePoint Base(long rv, string point, string? kind, string acceptance, bool decided)
        {
            var w = new World();
            var decisions = decided ? w.File("docs/automation/decisions/I-99.md", Decisions(G0Entry)) : null;
            var binding = w.Binding("docs/automation/evidence/I-99-agent/bootstrap/binding-" + acceptance.ToLowerInvariant() + ".json", PrincipalBindingId,
                "PRINCIPAL_COORDINATOR", "UNIT", acceptance);
            var preflight = w.File("docs/automation/evidence/I-99-agent/bootstrap/preflight.json", "{\"Schema\": \"rackcad-preflight/v1\", \"Action\": \"CUSTODY\"}\n");
            var state = M(
                ("schema", StateV2Shape.Schema),
                ("automation_state", M(
                    ("initiative", Unit), ("branch", "architecture/ejemplo"), ("claim_id", ClaimId), ("current_phase", "F4"), ("state", "implementing"),
                    ("gate", "none"), ("attempts", 0L), ("next_action", "el Principal continúa: «X: Y»"), ("last_evidence_commit", Sha))),
                ("protocol", M(
                    ("set", StateV2Shape.ProtocolSet), ("effective_sha", Sha),
                    ("basis", M(("claim_id", ClaimId), ("claim_commit", null), ("claim_parent_sha", DigitSha), ("claim_parent_contains_effective", true),
                        ("adoption_at", "G0"))),
                    ("g0_acceptance", M(("state", decided ? "ACCEPTED" : "PENDING"), ("decision", decisions))))),
                ("custody", M(
                    ("record_version", rv), ("point", point), ("point_kind", kind), ("last_rebase", null), ("rebase_history", L()),
                    ("principal", M(("state", "HELD"), ("binding", binding), ("acceptance", M(("state", acceptance), ("decision", decisions))),
                        ("preflight", preflight), ("designation", null), ("since_record_version", 1L))),
                    ("window", M(("seq", 0L), ("state", "CLOSED"))),
                    ("task_intent", null),
                    ("last_window", null),
                    ("chains", L()),
                    ("unverified_commits", L()))),
                ("counters", M(("correction_launches", L()), ("blocked_reruns", L()), ("rebase_recoveries", L()), ("invocations", L()))),
                ("execution_context", M(("mode", "manual"), ("note", "descriptivo; ningún lector lo consume"))),
                ("orchestration", NoLoop(w)));
            return new StatePoint(state, w.Tree);
        }

        private static YamlMap Contract(InMemoryStateTree tree) =>
            tree.Put("docs/automation/evidence/I-99-agent/T-01/gate-contract.json", "{\"Schema\": \"rackcad-gate-contract/v2\", \"TaskId\": \"T-01\"}\n");

        private static StatePoint WithWindow(StatePoint p, bool q0)
        {
            var s = p.State;
            var tree = (InMemoryStateTree)p.Tree;
            var custody = (YamlMap)s["custody"]!;
            if (q0)
            {
                custody["window"] = M(("seq", 1L), ("state", "OPENABLE"));
                custody["task_intent"] = M(("task_id", "T-01"), ("attempt", 0L), ("kind", "FIRST"), ("contract", Contract(tree)), ("continues_task_id", null),
                    ("planned_roles", L(M(("role", "EXECUTION_CONTROLLER"), ("binding", null)), M(("role", "WORKER"), ("binding", null)))));
                return p;
            }

            custody["window"] = M(("seq", 1L), ("state", "CLOSED"));
            custody["last_window"] = M(("seq", 1L), ("task_id", "T-01"), ("attempt", 0L), ("closure", "VERIFIED"), ("closure_source", "CONTROLLER"),
                ("delegation_run_id", "R20261003T010101Z-ab12"),
                ("verification", tree.Put("docs/automation/evidence/I-99-agent/T-01/w1/verification.json", "{\"Disposition\": \"VERIFIED\"}\n")),
                ("verified_sha", Sha2), ("journal", tree.Put("docs/automation/evidence/I-99-agent/T-01/w1/journal.json", "{\"Records\": 4}\n")),
                ("reconstruction", null));
            custody["chains"] = L(M(("task_id", "T-01"), ("state", "VERIFIED"), ("chain_base_sha", Sha), ("chain_red_sha", Sha3),
                ("chain_red_files", L("tests/RackCad.Tests/EjemploTests.cs")), ("continues_task_id", null), ("correction_authorized_pending", false)));
            ((YamlMap)s["counters"]!)["invocations"] = L(M(("scope", "F4"), ("launched", 2L), ("uncertain", 0L), ("cap", 12L),
                ("cap_source", tree.Put("docs/initiatives/I-99-contract.md", "# I-99\n\ncap: 12\n")), ("reconstructed", false), ("reconstruction", null)));
            return p;
        }

        public const string RebaseMapPath = "docs/automation/evidence/I-99-agent/rebase/R20261004T010101Z-cd34/rebase-map.json";

        /// <summary>The RebaseMap of the out-of-window rebase reconciled at record_version 6: Sha2 → Img2 and Sha3 → Img3 (Sha is in main).</summary>
        public static JsonObject RebaseMapJson() => new JsonObject
        {
            ["RunId"] = "R20261004T010101Z-cd34",
            ["TaskId"] = "SESSION",
            ["MainBeforeSha"] = Sha,
            ["MainAfterSha"] = "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee",
            ["BranchBeforeSha"] = Sha3,
            ["BranchAfterSha"] = Img3,
            ["Commits"] = new JsonArray(
                new JsonObject { ["OriginalSha"] = Sha2, ["ImageSha"] = Img2, ["PatchId"] = "1111111111111111111111111111111111111111", ["PatchIdEqual"] = true },
                new JsonObject { ["OriginalSha"] = Sha3, ["ImageSha"] = Img3, ["PatchId"] = "2222222222222222222222222222222222222222", ["PatchIdEqual"] = true }),
            ["StateFields"] = new JsonArray(
                new JsonObject { ["Field"] = "chains[T-01].chain_base_sha", ["OriginalSha"] = Sha, ["ImageSha"] = Sha },
                new JsonObject { ["Field"] = "chains[T-01].chain_red_sha", ["OriginalSha"] = Sha3, ["ImageSha"] = Img3 },
                new JsonObject { ["Field"] = "last_window.verified_sha", ["OriginalSha"] = Sha2, ["ImageSha"] = Img2 },
                new JsonObject { ["Field"] = "last_evidence_commit", ["OriginalSha"] = Sha, ["ImageSha"] = Sha }),
            ["CiRuns"] = new JsonArray(),
            ["Unmapped"] = new JsonArray(),
        };

        private static StatePoint Rebased(StatePoint p)
        {
            var s = p.State;
            var tree = (InMemoryStateTree)p.Tree;
            var custody = (YamlMap)s["custody"]!;
            var map = tree.PutJson(RebaseMapPath, RebaseMapJson());
            custody["last_rebase"] = M(("map", map), ("main_before", Sha), ("main_after", "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"), ("branch_before", Sha3),
                ("branch_after", Img3), ("record_version", 6L));
            custody["rebase_history"] = L(Clone(map));
            ((YamlMap)custody["last_window"]!)["verified_sha"] = Img2;
            var chain = (YamlMap)((List<object?>)custody["chains"]!)[0]!;
            chain["chain_red_sha"] = Img3;
            return p;
        }

        private static StatePoint Released(StatePoint p)
        {
            ((YamlMap)Y.M(p.State, "custody.principal")!)["state"] = "RELEASED";
            return p;
        }

        public const string DesignationEntry = "## Designación de N\n\n```text\nI62-PRINCIPAL-BINDING: B20261005T010101Z-ef56 ACCEPTED\nClaim-Id: " + ClaimId + "\n```";

        private static StatePoint Recovered(StatePoint p)
        {
            var tree = (InMemoryStateTree)p.Tree;
            var decisions = tree.Put("docs/automation/decisions/I-99.md", Decisions(G0Entry, DesignationEntry));
            var principal = (YamlMap)Y.M(p.State, "custody.principal")!;
            principal["state"] = "HELD";
            principal["binding"] = tree.PutJson("docs/automation/evidence/I-99-agent/qr/binding.json", new JsonObject
            {
                ["Schema"] = "rackcad-binding/v1", ["BindingId"] = "B20261005T010101Z-ef56", ["UnitId"] = Unit, ["Scope"] = "UNIT", ["TaskId"] = null,
                ["Role"] = "PRINCIPAL_COORDINATOR", ["Acceptance"] = new JsonObject { ["State"] = "ACCEPTED", ["Basis"] = "INDIVIDUAL_DECISION" },
            });
            principal["acceptance"] = M(("state", "ACCEPTED"), ("decision", decisions));
            principal["preflight"] = tree.Put("docs/automation/evidence/I-99-agent/qr/preflight.json", "{\"Schema\": \"rackcad-preflight/v1\", \"Action\": \"CUSTODY\"}\n");
            principal["designation"] = Clone(decisions);
            principal["since_record_version"] = 8L;
            ((YamlMap)Y.M(p.State, "protocol.g0_acceptance")!)["decision"] = Clone(decisions);
            return p;
        }

        /// <summary>
        /// A QU ORDINARY (record_version 5 of the custody history) with an open ARCHITECT_REVIEW loop whose first request holds a BUDGET_RESERVED attempt.
        /// Used by the canonical-serialization guard as a complete document; the orchestration guards build their own loops.
        /// </summary>
        public static YamlMap ReviewPendingQu()
        {
            var p = Point(5);
            var tree = (InMemoryStateTree)p.Tree;
            var binding = tree.Put("docs/automation/evidence/I-99-agent/review/L1/1/binding.json", "{\"Role\": \"ARCHITECT\"}\n");
            var rla = tree.Put("docs/automation/decisions/I-99.md", Decisions(G0Entry, "```text\nI62-REVIEW-LOOP-AUTHORIZATION: RLA-1\n```"));
            var obj = Obj(Sha, "docs/initiatives/I-99-proposal-v2.md", Sha);
            ((YamlMap)p.State["orchestration"]!)["loop"] = M(("type", "ARCHITECT_REVIEW"), ("phase", "REVIEW_PENDING"), ("instance_id", "ARL-5"),
                ("object", Clone(obj)), ("authorization", rla), ("action_validity", M(("authorization_id", "RLA-1"), ("state", "OPEN"), ("ended_at", null),
                ("ended_utc", null), ("ended_reason", null), ("ended_by", null))));
            ((YamlMap)p.State["orchestration"]!)["review_requests"] = L(M(("logical_review_request_id", "L20261003T010101Z-ab12"), ("loop_instance_id", "ARL-5"),
                ("object", Clone(obj)), ("round", 1L), ("state", "OPEN"),
                ("attempts", L(M(("attempt_seq", 1L), ("invocation", tree.Put("docs/automation/evidence/I-99-agent/review/L1/1/invocation.json", "{}\n")),
                    ("binding", binding), ("state", "BUDGET_RESERVED"), ("reserved_at", 5L), ("run_id", null), ("launch_evidence", null),
                    ("not_started_evidence", null), ("result", null), ("output_state", null), ("runtime_evidence", null), ("read_audit", null),
                    ("input_fidelity", tree.Put("docs/automation/evidence/I-99-agent/review/L1/1/input-fidelity.json", "{}\n")), ("fidelity_status", null),
                    ("unaccredited", L()), ("premise_independence", null), ("reserved_utc", "2026-10-03T01:01:01Z"), ("launching_utc", null), ("outcome", null),
                    ("ingested_at", null))))));
            ((YamlMap)p.State["orchestration"]!)["architect_budgets"] = L(M(("loop_instance_id", "ARL-5"),
                ("authorizations", L(M(("authorization_id", "RLA-1"), ("authorization", Clone(rla)), ("state", "OPEN"), ("ended_at", null), ("ended_utc", null),
                    ("ended_reason", null), ("ended_by", null)))),
                ("review_rounds", 1L), ("logical_requests", 1L), ("architect_launches", 1L),
                ("transport_reruns", L(M(("logical_review_request_id", "L20261003T010101Z-ab12"), ("count", 0L)))), ("correction_rounds", 0L),
                ("corrections_by_lineage", L()), ("caps", M(("review_rounds", 3L), ("logical_requests", 3L), ("transport_reruns_per_request", 2L),
                    ("corrections_per_lineage", 2L), ("correction_rounds", 2L), ("architect_launches", 9L))), ("closed_at", null), ("closed_by", null)));
            return p.State;
        }

        public const string WindowMapPath = "docs/automation/evidence/I-99-agent/T-01/w1/rebase-map.json";

        /// <summary>The RebaseMap of a rebase of 16.7 inside window 1, recorded in its journal: the Worker's W1 (Sha2) → Img2.</summary>
        public static JsonObject WindowRebaseMapJson() => new JsonObject
        {
            ["RunId"] = "R20261003T020202Z-ef01",
            ["TaskId"] = "T-01",
            ["MainBeforeSha"] = Sha,
            ["MainAfterSha"] = "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee",
            ["BranchBeforeSha"] = Sha2,
            ["BranchAfterSha"] = Img2,
            ["Commits"] = new JsonArray(
                new JsonObject { ["OriginalSha"] = Sha2, ["ImageSha"] = Img2, ["PatchId"] = "3333333333333333333333333333333333333333", ["PatchIdEqual"] = true }),
            ["StateFields"] = new JsonArray(new JsonObject { ["Field"] = "last_evidence_commit", ["OriginalSha"] = Sha, ["ImageSha"] = Sha }),
            ["CiRuns"] = new JsonArray(),
            ["Unmapped"] = new JsonArray(),
        };

        /// <summary>
        /// The Q7 that closes window 1 after a rebase of 16.7 inside it (I-P12): last_rebase of its own record_version, the window's map custodied
        /// and appended to rebase_history (A-1 D2-9), and the window's own results (verified_sha) on the rebased branch.
        /// </summary>
        public static StatePoint WindowRebaseClose()
        {
            var q7 = Point(4);
            var tree = (InMemoryStateTree)q7.Tree;
            var map = tree.PutJson(WindowMapPath, WindowRebaseMapJson());
            var custody = (YamlMap)q7.State["custody"]!;
            custody["last_rebase"] = M(("map", map), ("main_before", Sha), ("main_after", "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee"), ("branch_before", Sha2),
                ("branch_after", Img2), ("record_version", 4L));
            custody["rebase_history"] = L(Clone(map));
            ((YamlMap)custody["last_window"]!)["verified_sha"] = Img2;
            return q7;
        }

        /// <summary>The point with the given record_version from a fresh custody history (each call builds independent copies).</summary>
        public static StatePoint Point(long rv) => CustodyHistory().Single(x => Y.N(x.State, "custody.record_version") == rv);

        /// <summary>A copy of a point whose tree can be mutated without touching the original.</summary>
        public static StatePoint Copy(StatePoint p) => new StatePoint(Clone(p.State), ((InMemoryStateTree)p.Tree).Clone());
    }
}
