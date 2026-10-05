#nullable enable
using System.Collections.Generic;

namespace RackCad.Tests
{
    /// <summary>
    /// Synthetic <c>rackcad-automation-state/v2</c> documents in the shape of Proposal V14 B.8 plus the AGREED amendment A-1 (commit ca09ade8, blob
    /// c01899a7): loop identity and per-loop budgets, REVIEWER closures and the custodied rebase history. Test data only; never a real unit.
    /// </summary>
    public static class StateV2Samples
    {
        public const string Sha = "4c617e82b32b6c810b68d75fc19472efed22b393";
        public const string DigitSha = "1234567890123456789012345678901234567890";
        public const string ClaimId = "5b661a17-8c18-4183-8554-3866059cba2b";

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

        public static YamlMap Ref(string path, string blob = "13501476b775ca18c4c61ba3e1793796b51067d9") => M(("path", path), ("blob", blob));

        public static YamlMap Obj(string commit, string path, string blob) => M(("commit", commit), ("path", path), ("blob", blob));

        /// <summary>A QU ORDINARY with an open ARCHITECT_REVIEW loop whose first request holds a BUDGET_RESERVED attempt.</summary>
        public static YamlMap ReviewPendingQu()
        {
            var binding = Ref("docs/automation/evidence/I-99-agent/bootstrap/binding.json");
            return M(
                ("schema", "rackcad-automation-state/v2"),
                ("automation_state", M(
                    ("initiative", "I-99"), ("branch", "architecture/ejemplo"), ("claim_id", ClaimId), ("current_phase", "F4"), ("state", "implementing"),
                    ("gate", "none"), ("attempts", 1L), ("next_action", "el Principal lanza el intento reservado: «X: Y»"), ("last_evidence_commit", Sha))),
                ("protocol", M(
                    ("set", "rackcad-protocol/I62"), ("effective_sha", Sha),
                    ("basis", M(("claim_id", ClaimId), ("claim_commit", null), ("claim_parent_sha", DigitSha), ("claim_parent_contains_effective", true),
                        ("adoption_at", "G0"))),
                    ("g0_acceptance", M(("state", "ACCEPTED"), ("decision", Ref("docs/automation/decisions/I-99.md")))))),
                ("custody", M(
                    ("record_version", 7L), ("point", "QU"), ("point_kind", "ORDINARY"), ("last_rebase", null), ("rebase_history", L()),
                    ("principal", M(("state", "HELD"), ("binding", binding), ("acceptance", M(("state", "ACCEPTED"), ("decision", Ref("docs/automation/decisions/I-99.md")))),
                        ("preflight", Ref("docs/automation/evidence/I-99-agent/bootstrap/preflight.json")), ("designation", null), ("since_record_version", 1L))),
                    ("window", M(("seq", 2L), ("state", "CLOSED"))),
                    ("task_intent", null),
                    ("last_window", M(("seq", 2L), ("task_id", "T-01"), ("attempt", 1L), ("closure", "VERIFIED"), ("closure_source", "CONTROLLER"),
                        ("delegation_run_id", "R20261003T010101Z-ab12"), ("verification", Ref("docs/automation/evidence/I-99-agent/w2/verification.json")),
                        ("verified_sha", Sha), ("journal", Ref("docs/automation/evidence/I-99-agent/w2/journal.json")), ("reconstruction", null))),
                    ("chains", L(M(("task_id", "T-01"), ("state", "VERIFIED"), ("chain_base_sha", Sha), ("chain_red_sha", Sha),
                        ("chain_red_files", L("tests/RackCad.Tests/EjemploTests.cs")), ("continues_task_id", null), ("correction_authorized_pending", false)))),
                    ("unverified_commits", L()))),
                ("counters", M(
                    ("correction_launches", L(M(("seq", 1L), ("task_id", "T-01"), ("failure_class", "Tests"), ("correction_of_run_id", "R20261003T010101Z-ab12"),
                        ("attempts_after", 1L), ("record_version", 5L)))),
                    ("blocked_reruns", L(M(("task_id", "T-01"), ("phase", "NEGATIVE:nc2"), ("count", 1L)))),
                    ("rebase_recoveries", L()),
                    ("invocations", L(M(("scope", "F4"), ("launched", 3L), ("uncertain", 0L), ("cap", 12L), ("cap_source", Ref("docs/initiatives/I-99-contract.md")),
                        ("reconstructed", false), ("reconstruction", null)))))),
                ("execution_context", M(("mode", "manual"), ("note", "descriptivo; ningún lector lo consume"))),
                ("orchestration", M(
                    ("loop", M(("type", "ARCHITECT_REVIEW"), ("phase", "REVIEW_PENDING"), ("instance_id", "ARL-6"),
                        ("object", Obj(Sha, "docs/initiatives/I-99-proposal-v2.md", Sha)), ("authorization", Ref("docs/automation/decisions/I-99-rla-1.json")),
                        ("action_validity", M(("authorization_id", "RLA-1"), ("state", "OPEN"), ("ended_at", null), ("ended_utc", null), ("ended_reason", null),
                            ("ended_by", null))))),
                    ("next_action", M(("role", "ARCHITECT"), ("action", "REVIEW_DESIGN"), ("target", Obj(Sha, "docs/initiatives/I-99-proposal-v2.md", Sha)),
                        ("unit", "I-99"), ("gate", "F1"), ("task_id", null), ("required_inputs", L(binding)), ("required_capabilities", L("ARCHITECTURE_REVIEW.read")),
                        ("required_independence", "OD-6 alternativa 1"), ("invocation_permission", "READ_ONLY"), ("budget_remaining", M(("review_rounds", 2L))),
                        ("expected_output", "rackcad-architect-review-result/v1"), ("completion_condition", "RESULT_INGESTED"), ("stop_conditions", L("P-18", "P-19")),
                        ("escalation_conditions", L()))),
                    ("review_requests", L(M(("logical_review_request_id", "L20261003T010101Z-ab12"), ("loop_instance_id", "ARL-6"),
                        ("object", Obj(Sha, "docs/initiatives/I-99-proposal-v2.md", Sha)), ("round", 1L), ("state", "OPEN"),
                        ("attempts", L(M(("attempt_seq", 1L), ("invocation", Ref("docs/automation/evidence/I-99-agent/review/L1/1/invocation.json")), ("binding", binding),
                            ("state", "BUDGET_RESERVED"), ("reserved_at", 7L), ("run_id", null), ("launch_evidence", null), ("not_started_evidence", null),
                            ("result", null), ("output_state", null), ("runtime_evidence", null), ("read_audit", null),
                            ("input_fidelity", Ref("docs/automation/evidence/I-99-agent/review/L1/1/input-fidelity.json")), ("fidelity_status", null), ("unaccredited", L()),
                            ("premise_independence", null), ("reserved_utc", "2026-10-03T01:01:01Z"), ("launching_utc", null), ("outcome", null), ("ingested_at", null))))))),
                    ("findings", L()),
                    ("architect_budgets", L(M(("loop_instance_id", "ARL-6"),
                        ("authorizations", L(M(("authorization_id", "RLA-1"), ("authorization", Ref("docs/automation/decisions/I-99-rla-1.json")), ("state", "OPEN"),
                            ("ended_at", null), ("ended_utc", null), ("ended_reason", null), ("ended_by", null)))),
                        ("review_rounds", 1L), ("logical_requests", 1L), ("architect_launches", 1L),
                        ("transport_reruns", L(M(("logical_review_request_id", "L20261003T010101Z-ab12"), ("count", 0L)))),
                        ("correction_rounds", 0L), ("corrections_by_lineage", L()),
                        ("caps", M(("review_rounds", 3L), ("logical_requests", 3L), ("transport_reruns_per_request", 2L), ("corrections_per_lineage", 2L),
                            ("correction_rounds", 2L), ("architect_launches", 9L))),
                        ("closed_at", null), ("closed_by", null)))),
                    ("budgets", M(("review_rounds", 0L), ("logical_requests", 0L), ("architect_launches", 0L), ("transport_reruns", L()), ("correction_rounds", 0L),
                        ("corrections_by_lineage", L()), ("caps", Ref("docs/initiatives/I-62-proposal-v14.md", "34ad80ea1bfff144bfc5169f62920a4c904c1bfa")))),
                    ("reviewer_closures", L()),
                    ("escalation", M(("state", "NONE"), ("reason", null), ("required_decision", null))),
                    ("autonomy_gaps", L()))));
        }
    }
}
