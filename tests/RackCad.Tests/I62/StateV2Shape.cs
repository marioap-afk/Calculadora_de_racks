#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace RackCad.Tests
{
    /// <summary>One violation found by the I-62 state validator: the frozen invariant it breaks, the A-1 delta that shapes it (if any) and why.</summary>
    public sealed record StateViolation(string Invariant, string Clause, string Message)
    {
        public override string ToString() => Clause.Length == 0 ? Invariant + ": " + Message : Invariant + " (" + Clause + "): " + Message;
    }

    /// <summary>
    /// The declared shape of <c>rackcad-automation-state/v2</c>: Proposal V14 B.8.1 (custody and counters) and B.8.8 (orchestration) with the fields
    /// that the AGREED amendment A-1 adds (D1-1 <c>loop.instance_id</c>, D1-2 <c>architect_budgets[]</c>, D1-3 <c>review_requests[].loop_instance_id</c>,
    /// D1-16 REVIEWER_SATISFIED, D1-17 BLOCKING/ADVISORY, D1-19 <c>reviewer_closures[]</c>, D2-9 <c>custody.rebase_history[]</c>). Every key is required
    /// and closed (an unknown key is a violation), except <c>execution_context</c>, which no reader of the protocol consumes, and the open maps that the
    /// Freeze leaves untyped (<c>next_action.budget_remaining</c>).
    /// </summary>
    public static class StateV2Shape
    {
        public const string Schema = "rackcad-automation-state/v2";
        public const string ProtocolSet = "rackcad-protocol/I62";

        public static readonly string[] Points = { "BOOTSTRAP", "Q0", "Q7", "QU", "QH", "QR" };
        public static readonly string[] Roles = { "PRINCIPAL_COORDINATOR", "ARCHITECT", "EXECUTION_CONTROLLER", "WORKER", "REVIEWER" };
        public static readonly string[] ReviewPhases =
        {
            "NONE", "REVIEW_PENDING", "ARCHITECT_INVOKED", "RESULT_INGESTED", "ARCHITECT_SATISFIED", "CORRECTING", "PUBLISHED", "CI_VERIFIED",
            "REREVIEW_PENDING", "ESCALATE_OWNER", "REVIEWER_SATISFIED",
        };

        public static readonly string[] AttemptStates =
        {
            "INVOCATION_PLANNED", "BUDGET_RESERVED", "LAUNCHING", "LAUNCHED", "LAUNCH_UNCERTAIN", "RESULT_RECEIVED", "RESULT_INGESTED", "CANCELLED_BEFORE_LAUNCH",
        };

        public static readonly string[] EndedReasons = { "ARCHITECT_SATISFIED", "EXHAUSTED", "EXPIRED", "REVOKED", "SUPERSEDED", "REVIEWER_SATISFIED" };

        private static readonly Regex ShaRx = new Regex("^[0-9a-f]{40}$", RegexOptions.CultureInvariant);
        private static readonly Regex UuidRx = new Regex("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.CultureInvariant);
        private static readonly Regex InstantRx = new Regex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d+)?Z$", RegexOptions.CultureInvariant);
        private static readonly Regex PhaseRx = new Regex("^(PLANNING|WORK|VERIFICATION|REVERIFICATION|NEGATIVE:[^\\s]+)$", RegexOptions.CultureInvariant);

        private static readonly Node Tree = BuildTree();

        public static bool IsSha(object? v) => v is string s && ShaRx.IsMatch(s);

        public static bool IsStateRef(object? v) =>
            v is YamlMap m && m.Count == 2 && m.TryGetValue("path", out var p) && p is string ps && ps.Length > 0 && m.TryGetValue("blob", out var b) && IsSha(b);

        /// <summary>Shape violations (invariant id «B.8.1») of a whole state, with the exact path of each.</summary>
        public static IReadOnlyList<StateViolation> Check(YamlMap state)
        {
            var violations = new List<StateViolation>();
            Tree.Check(state, "$", violations);
            return violations;
        }

        // ------------------------------------------------------------------ the declared tree

        private static Node BuildTree()
        {
            var stateRef = new RefNode();
            var obj = Map(("commit", Sha()), ("path", Str()), ("blob", Sha()));
            var validity = Map(
                ("authorization_id", Str()), ("state", Enum("OPEN", "ENDED")), ("ended_at", Int(1).OrNull()), ("ended_utc", Instant().OrNull()),
                ("ended_reason", Enum(EndedReasons).OrNull()), ("ended_by", stateRef.OrNull()));
            var attempt = Map(
                ("attempt_seq", Int(1)), ("invocation", stateRef), ("binding", stateRef), ("state", Enum(AttemptStates)), ("reserved_at", Int(1).OrNull()),
                ("run_id", Str().OrNull()), ("launch_evidence", stateRef.OrNull()), ("not_started_evidence", stateRef.OrNull()), ("result", stateRef.OrNull()),
                ("output_state", Enum("PRESENT", "ABSENT").OrNull()), ("runtime_evidence", stateRef.OrNull()), ("read_audit", stateRef.OrNull()),
                ("input_fidelity", stateRef), ("fidelity_status", Enum("FAITHFUL", "FAITHFUL_NORMALIZED", "DEGRADED_BOUNDED", "DEGRADED_UNBOUNDED", "UNVERIFIED").OrNull()),
                ("unaccredited", List(Str())), ("premise_independence", stateRef.OrNull()), ("reserved_utc", Instant().OrNull()), ("launching_utc", Instant().OrNull()),
                ("outcome", Enum("VALID", "INVALID", "INVALID_REVIEW_CONTEXT", "INPUT_FIDELITY_INVALID", "CONTRADICTION", "NO_OUTPUT", "UNCERTAIN", "UNAUTHORIZED_LAUNCH").OrNull()),
                ("ingested_at", Int(1).OrNull()));
            var counterSet = new (string, Node)[]
            {
                ("review_rounds", Int(0)), ("logical_requests", Int(0)), ("architect_launches", Int(0)),
                ("transport_reruns", List(Map(("logical_review_request_id", Str()), ("count", Int(0))))),
                ("correction_rounds", Int(0)), ("corrections_by_lineage", List(Map(("lineage_id", Str()), ("count", Int(0))))),
            };

            return Map(
                ("schema", Const(Schema)),
                ("automation_state", Map(
                    ("initiative", Str()), ("branch", Str()), ("claim_id", Uuid()), ("current_phase", Str()),
                    ("state", Enum("claimed", "implementing", "validating", "ci-failed", "waiting", "review-ready", "integration-ready", "completed")),
                    ("gate", Enum("none", "owner-decision", "owner-validation", "autocad", "plugin-build", "ci", "dependency", "conflict", "permissions", "scope")),
                    ("attempts", Int(0)), ("next_action", Str()), ("last_evidence_commit", Sha()))),
                ("protocol", Map(
                    ("set", Const(ProtocolSet)), ("effective_sha", Sha()),
                    ("basis", Map(("claim_id", Uuid()), ("claim_commit", Sha().OrNull()), ("claim_parent_sha", Sha()), ("claim_parent_contains_effective", Bool()),
                        ("adoption_at", Enum("G0", "MID_INITIATIVE")))),
                    ("g0_acceptance", Map(("state", Enum("PENDING", "ACCEPTED", "REJECTED")), ("decision", stateRef.OrNull()))))),
                ("custody", Map(
                    ("record_version", Int(1)), ("point", Enum(Points)), ("point_kind", Enum("ORDINARY", "REBASE_RECONCILIATION").OrNull()),
                    ("last_rebase", Map(("map", stateRef), ("main_before", Sha()), ("main_after", Sha()), ("branch_before", Sha()), ("branch_after", Sha()),
                        ("record_version", Int(1))).OrNull()),
                    ("rebase_history", List(stateRef)),
                    ("principal", Map(("state", Enum("HELD", "RELEASED")), ("binding", stateRef),
                        ("acceptance", Map(("state", Enum("PENDING", "ACCEPTED", "REJECTED")), ("decision", stateRef.OrNull()))),
                        ("preflight", stateRef), ("designation", stateRef.OrNull()), ("since_record_version", Int(1)))),
                    ("window", Map(("seq", Int(0)), ("state", Enum("CLOSED", "OPENABLE")))),
                    ("task_intent", Map(("task_id", Str()), ("attempt", Int(0)), ("kind", Enum("FIRST", "CORRECTION", "RERUN", "REISSUE")), ("contract", stateRef),
                        ("continues_task_id", Str().OrNull()), ("planned_roles", List(Map(("role", Enum(Roles)), ("binding", stateRef.OrNull())), 1))).OrNull()),
                    ("last_window", Map(("seq", Int(1)), ("task_id", Str()), ("attempt", Int(0)),
                        ("closure", Enum("VERIFIED", "REWORK", "BLOCKED", "STOP", "NOT_ACCEPTED", "WITHDRAWN", "ABANDONED")),
                        ("closure_source", Enum("CONTROLLER", "SESSION", "COORDINATOR")), ("delegation_run_id", Str().OrNull()), ("verification", stateRef.OrNull()),
                        ("verified_sha", Sha().OrNull()), ("journal", stateRef.OrNull()), ("reconstruction", stateRef.OrNull())).OrNull()),
                    ("chains", List(Map(("task_id", Str()), ("state", Enum("IN_COURSE", "VERIFIED", "ABANDONED")), ("chain_base_sha", Sha()), ("chain_red_sha", Sha().OrNull()),
                        ("chain_red_files", List(Str())), ("continues_task_id", Str().OrNull()), ("correction_authorized_pending", Bool())))),
                    ("unverified_commits", List(Map(("sha", Sha()), ("task_id", Str()), ("window_seq", Int(1)), ("reason", Const("JOURNAL_UNAVAILABLE")),
                        ("superseded_by", Str().OrNull())))))),
                ("counters", Map(
                    ("correction_launches", List(Map(("seq", Int(1)), ("task_id", Str()), ("failure_class", Str()), ("correction_of_run_id", Str()), ("attempts_after", Int(0)),
                        ("record_version", Int(1))))),
                    ("blocked_reruns", List(Map(("task_id", Str()), ("phase", Pattern(PhaseRx, "PLANNING | WORK | VERIFICATION | REVERIFICATION | NEGATIVE:<id>")),
                        ("count", Int(0, 2))))),
                    ("rebase_recoveries", List(Map(("task_id", Str()), ("count", Int(0, 2)), ("last_rebase_map", stateRef)))),
                    ("invocations", List(Map(("scope", Str()), ("launched", Int(0)), ("uncertain", Int(0)), ("cap", Int(0)), ("cap_source", stateRef), ("reconstructed", Bool()),
                        ("reconstruction", stateRef.OrNull())))))),
                ("execution_context", new OpenMapNode().Optional()),
                ("orchestration", Map(
                    ("loop", Map(("type", Enum("NONE", "ARCHITECT_REVIEW", "EXECUTION", "REVIEWER")), ("phase", Str()), ("instance_id", Str().OrNull()),
                        ("object", obj.OrNull()), ("authorization", stateRef.OrNull()), ("action_validity", validity.OrNull()))),
                    ("next_action", Map(("role", Str()), ("action", Str()), ("target", obj.OrNull()), ("unit", Str()), ("gate", Str()), ("task_id", Str().OrNull()),
                        ("required_inputs", List(stateRef)), ("required_capabilities", List(Str())), ("required_independence", Str()),
                        ("invocation_permission", stateRef.OrNull()), ("budget_remaining", new OpenMapNode()), ("expected_output", Str()),
                        ("completion_condition", Str()), ("stop_conditions", List(Str())), ("escalation_conditions", List(Str())))),
                    ("review_requests", List(Map(("logical_review_request_id", Str()), ("loop_instance_id", Str().OrNull()), ("object", obj), ("round", Int(1)),
                        ("state", Enum("OPEN", "INGESTED", "EXHAUSTED", "CANCELLED")), ("attempts", List(attempt, 1))))),
                    ("findings", List(Map(("lineage_id", Str()), ("finding_ids", List(Str())), ("issuer", Enum("ARCHITECT", "COORDINATOR", "REVIEWER")),
                        ("opened_in", stateRef), ("severity", Enum("REQUIRED", "OPTIONAL", "BLOCKING", "ADVISORY")), ("class", Str()), ("affected_section", Str()),
                        ("state", Enum("OPEN", "CLOSED", "STILL_OPEN", "SUPERSEDED")), ("response", stateRef.OrNull()), ("last_disposition_in", stateRef.OrNull()),
                        ("omitted_in", List(stateRef)), ("closed_by", stateRef.OrNull()), ("downgraded_by", stateRef.OrNull())))),
                    ("architect_budgets", List(Map(new[]
                    {
                        ("loop_instance_id", (Node)Str()),
                        ("authorizations", List(Map(("authorization_id", Str()), ("authorization", stateRef), ("state", Enum("OPEN", "ENDED")), ("ended_at", Int(1).OrNull()),
                            ("ended_utc", Instant().OrNull()), ("ended_reason", Enum(EndedReasons).OrNull()), ("ended_by", stateRef.OrNull())), 1)),
                    }.Concat(counterSet).Concat(new (string, Node)[]
                    {
                        ("caps", Map(("review_rounds", Int(0)), ("logical_requests", Int(0)), ("transport_reruns_per_request", Int(0)), ("corrections_per_lineage", Int(0)),
                            ("correction_rounds", Int(0)), ("architect_launches", Int(0)))),
                        ("closed_at", Int(1).OrNull()), ("closed_by", stateRef.OrNull()),
                    }).ToArray()))),
                    ("budgets", Map(counterSet.Concat(new (string, Node)[] { ("caps", stateRef) }).ToArray())),
                    ("reviewer_closures", List(Map(("last_request", Str()), ("authorization", stateRef), ("validity", validity), ("closed_at", Int(1)),
                        ("closed_by", stateRef.OrNull())))),
                    ("escalation", Map(("state", Enum("NONE", "OWNER", "COORDINATOR")), ("reason", Str().OrNull()), ("required_decision", Str().OrNull()))),
                    ("autonomy_gaps", List(stateRef)))));
        }

        // ------------------------------------------------------------------ node kinds

        private static MapNode Map(params (string Key, Node Shape)[] fields) => new MapNode(fields);

        private static ListNode List(Node item, int min = 0) => new ListNode(item, min);

        private static Node Str() => new LeafNode("non-empty string", v => v is string s && s.Length > 0);

        private static Node Sha() => new LeafNode("40-hex SHA", IsSha);

        private static Node Uuid() => new LeafNode("UUID", v => v is string s && UuidRx.IsMatch(s));

        private static Node Instant() => new LeafNode("UTC instant", v => v is string s && InstantRx.IsMatch(s)
            && DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out _));

        private static Node Bool() => new LeafNode("bool", v => v is bool);

        private static Node Int(long min, long max = long.MaxValue) => new LeafNode("integer in " + min + ".." + (max == long.MaxValue ? "∞" : max.ToString(CultureInfo.InvariantCulture)),
            v => v is long n && n >= min && n <= max);

        private static Node Const(string value) => new LeafNode("«" + value + "»", v => v is string s && s == value);

        private static Node Enum(params string[] values) => new LeafNode(string.Join(" | ", values), v => v is string s && values.Contains(s));

        private static Node Pattern(Regex rx, string label) => new LeafNode(label, v => v is string s && rx.IsMatch(s));

        private abstract class Node
        {
            public bool Nullable { get; private set; }

            public bool IsOptional { get; private set; }

            public Node OrNull()
            {
                var c = (Node)MemberwiseClone();
                c.Nullable = true;
                return c;
            }

            public Node Optional()
            {
                var c = (Node)MemberwiseClone();
                c.IsOptional = true;
                return c;
            }

            public void Check(object? value, string path, List<StateViolation> violations)
            {
                if (value is null)
                {
                    if (!Nullable)
                    {
                        violations.Add(new StateViolation("B.8.1", string.Empty, path + ": null where " + Expected + " is required"));
                    }

                    return;
                }

                CheckValue(value, path, violations);
            }

            protected abstract string Expected { get; }

            protected abstract void CheckValue(object value, string path, List<StateViolation> violations);
        }

        private sealed class LeafNode : Node
        {
            private readonly string label;
            private readonly Func<object?, bool> accepts;

            public LeafNode(string label, Func<object?, bool> accepts)
            {
                this.label = label;
                this.accepts = accepts;
            }

            protected override string Expected => label;

            protected override void CheckValue(object value, string path, List<StateViolation> violations)
            {
                if (!accepts(value))
                {
                    violations.Add(new StateViolation("B.8.1", string.Empty, path + ": " + Describe(value) + " is not " + label));
                }
            }
        }

        private sealed class RefNode : Node
        {
            protected override string Expected => "StateRef {path, blob}";

            protected override void CheckValue(object value, string path, List<StateViolation> violations)
            {
                if (!IsStateRef(value))
                {
                    violations.Add(new StateViolation("B.8.1", string.Empty, path + ": " + Describe(value) + " is not a StateRef {path, blob}"));
                }
            }
        }

        private sealed class OpenMapNode : Node
        {
            protected override string Expected => "mapping";

            protected override void CheckValue(object value, string path, List<StateViolation> violations)
            {
                if (value is not YamlMap)
                {
                    violations.Add(new StateViolation("B.8.1", string.Empty, path + ": " + Describe(value) + " is not a mapping"));
                }
            }
        }

        private sealed class MapNode : Node
        {
            private readonly (string Key, Node Shape)[] fields;

            public MapNode((string Key, Node Shape)[] fields)
            {
                this.fields = fields;
            }

            protected override string Expected => "mapping";

            protected override void CheckValue(object value, string path, List<StateViolation> violations)
            {
                if (value is not YamlMap map)
                {
                    violations.Add(new StateViolation("B.8.1", string.Empty, path + ": " + Describe(value) + " is not a mapping"));
                    return;
                }

                foreach (var (key, shape) in fields)
                {
                    if (!map.TryGetValue(key, out var v))
                    {
                        if (!shape.IsOptional)
                        {
                            violations.Add(new StateViolation("B.8.1", string.Empty, path + "." + key + ": missing"));
                        }

                        continue;
                    }

                    shape.Check(v, path + "." + key, violations);
                }

                foreach (var key in map.Keys.Where(k => fields.All(f => f.Key != k)))
                {
                    violations.Add(new StateViolation("B.8.1", string.Empty, path + "." + key + ": unknown key"));
                }
            }
        }

        private sealed class ListNode : Node
        {
            private readonly Node item;
            private readonly int min;

            public ListNode(Node item, int min)
            {
                this.item = item;
                this.min = min;
            }

            protected override string Expected => "sequence";

            protected override void CheckValue(object value, string path, List<StateViolation> violations)
            {
                if (value is not List<object?> list)
                {
                    violations.Add(new StateViolation("B.8.1", string.Empty, path + ": " + Describe(value) + " is not a sequence"));
                    return;
                }

                if (list.Count < min)
                {
                    violations.Add(new StateViolation("B.8.1", string.Empty, path + ": " + list.Count + " items, at least " + min + " required"));
                }

                for (var i = 0; i < list.Count; i++)
                {
                    item.Check(list[i], path + "[" + i + "]", violations);
                }
            }
        }

        private static string Describe(object? v) => v switch
        {
            null => "null",
            string s => "«" + (s.Length > 40 ? s.Substring(0, 40) + "…" : s) + "»",
            YamlMap => "a mapping",
            List<object?> => "a sequence",
            _ => Convert.ToString(v, CultureInfo.InvariantCulture) ?? v.GetType().Name,
        };
    }
}
