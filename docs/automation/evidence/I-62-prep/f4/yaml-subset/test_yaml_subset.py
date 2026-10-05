"""Tests of yaml_subset.py (DEP-F4-YAML): the synthetic cases the order lists, a full synthetic state/v2 in the canonical serialization (round-trip
through a canonical writer), mutations of the reader that must make a test fail, and a sweep over every real state file of the repository.

Usage: python test_yaml_subset.py <repo> <out.json>
"""
import importlib
import json
import os
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import yaml_subset as Y  # noqa: E402

REPO, OUT = sys.argv[1], sys.argv[2]


from yaml_subset import dumps  # noqa: E402  (canonical writer, kept next to the reader)


SHA = "4c617e82b32b6c810b68d75fc19472efed22b393"
REF = {"path": "docs/automation/evidence/I-62-agent/bootstrap/binding.json", "blob": "13501476b775ca18c4c61ba3e1793796b51067d9"}
STATE_V2 = {
    "schema": "rackcad-automation-state/v2",
    "automation_state": {"initiative": "I-99", "branch": "architecture/ejemplo", "claim_id": "5b661a17-8c18-4183-8554-3866059cba2b", "current_phase": "F4",
                         "state": "implementing", "gate": "none", "attempts": 1, "next_action": "el Principal reserva el intento siguiente: «X: Y»",
                         "last_evidence_commit": SHA},
    "protocol": {"set": "rackcad-protocol/I62", "effective_sha": SHA,
                 "basis": {"claim_id": "5b661a17-8c18-4183-8554-3866059cba2b", "claim_commit": None, "claim_parent_sha": "1234567890123456789012345678901234567890",
                           "claim_parent_contains_effective": True, "adoption_at": "G0"},
                 "g0_acceptance": {"state": "ACCEPTED", "decision": REF}},
    "custody": {"record_version": 7, "point": "QU", "point_kind": "REBASE_RECONCILIATION",
                "last_rebase": {"map": REF, "main_before": SHA, "main_after": SHA, "branch_before": SHA, "branch_after": SHA, "record_version": 7},
                "principal": {"state": "HELD", "binding": REF, "acceptance": {"state": "ACCEPTED", "decision": REF}, "preflight": REF, "designation": None,
                              "since_record_version": 1},
                "window": {"seq": 2, "state": "CLOSED"},
                "task_intent": {"task_id": "T-01", "attempt": 1, "kind": "CORRECTION", "contract": REF, "continues_task_id": None,
                                "planned_roles": [{"role": "EXECUTION_CONTROLLER", "binding": REF}, {"role": "WORKER", "binding": None}]},
                "last_window": {"seq": 2, "task_id": "T-01", "attempt": 1, "closure": "VERIFIED", "closure_source": "CONTROLLER", "delegation_run_id": "R20261003T010101Z-ab12",
                                "verification": REF, "verified_sha": SHA, "journal": REF, "reconstruction": None},
                "chains": [{"task_id": "T-01", "state": "IN_COURSE", "chain_base_sha": SHA, "chain_red_sha": None, "chain_red_files": [], "continues_task_id": None,
                            "correction_authorized_pending": False}],
                "unverified_commits": []},
    "counters": {"correction_launches": [{"seq": 1, "task_id": "T-01", "failure_class": "Tests", "correction_of_run_id": "R20261003T010101Z-ab12", "attempts_after": 1,
                                          "record_version": 5}],
                 "blocked_reruns": [{"task_id": "T-01", "phase": "NEGATIVE:nc2", "count": 1}], "rebase_recoveries": [],
                 "invocations": [{"scope": "F4", "launched": 3, "uncertain": 0, "cap": 12, "cap_source": REF, "reconstructed": False, "reconstruction": None}]},
    "execution_context": {"mode": "manual", "note": "descriptivo; ningún lector lo consume"},
    "orchestration": {
        "loop": {"type": "ARCHITECT_REVIEW", "phase": "REVIEW_PENDING", "object": {"commit": SHA, "path": "docs/initiatives/I-99-proposal-v2.md", "blob": SHA},
                 "authorization": REF, "action_validity": {"authorization_id": "RLA-1", "state": "OPEN", "ended_at": None, "ended_utc": None, "ended_reason": None,
                                                           "ended_by": None}},
        "next_action": {"role": "ARCHITECT", "action": "REVIEW_DESIGN", "target": {"commit": SHA, "path": "docs/x.md", "blob": SHA}, "unit": "I-99", "gate": "F1",
                        "task_id": None, "required_inputs": [REF], "required_capabilities": ["ARCHITECTURE_REVIEW.read"], "required_independence": "OD-6 alternativa 1",
                        "invocation_permission": "READ_ONLY", "budget_remaining": {"review_rounds": 2}, "expected_output": "rackcad-architect-review-result/v1",
                        "completion_condition": "RESULT_INGESTED", "stop_conditions": ["P-18", "P-19"], "escalation_conditions": []},
        "review_requests": [{"logical_review_request_id": "L20261003T010101Z-ab12", "object": {"commit": SHA, "path": "docs/x.md", "blob": SHA}, "round": 1,
                             "state": "OPEN", "attempts": [{"attempt_seq": 1, "invocation": REF, "binding": REF, "state": "BUDGET_RESERVED", "reserved_at": 6,
                                                            "run_id": None, "launch_evidence": None, "not_started_evidence": None, "result": None,
                                                            "output_state": None, "runtime_evidence": None, "read_audit": None, "input_fidelity": REF,
                                                            "fidelity_status": None, "unaccredited": [], "premise_independence": None,
                                                            "reserved_utc": "2026-10-03T01:01:01Z", "launching_utc": None, "outcome": None, "ingested_at": None}]}],
        "findings": [],
        "budgets": {"review_rounds": 1, "logical_requests": 1, "architect_launches": 1, "transport_reruns": [{"logical_review_request_id": "L20261003T010101Z-ab12", "count": 0}],
                    "correction_rounds": 0, "corrections_by_lineage": [], "caps": REF},
        "escalation": {"state": "NONE", "reason": None, "required_decision": None},
        "autonomy_gaps": []}}

CASES = [  # (name, text, expected: "OK" or a substring of the rejection reason)
    ("valid-minimal", "schema: rackcad-automation-state/v2\nautomation_state:\n  initiative: I-99\n  attempts: 0\n  claim_id: null\n", "OK"),
    ("nested-list-map", "a:\n  items:\n    - k: 1\n      v: \"x\"\n    - k: 2\n      v: []\n  flags:\n    - true\n    - null\n", "OK"),
    ("quoted-colon", "a: \"X: Y\"\nb: 'it''s: fine'\n", "OK"),
    ("plain-colon", "a: X: Y\n", "contains ': '"),
    ("invalid-indentation", "a:\n  b:\n    c: 1\n   d: 2\n", "unexpected indentation"),
    ("duplicate-key", "a: 1\na: 2\n", "duplicate key"),
    ("anchor", "a: &x 1\nb: 2\n", "anchors, aliases and tags"),
    ("alias", "a: 1\nb: *x\n", "anchors, aliases and tags"),
    ("multiline-folded", "a: >-\n  uno\n  dos\n", "block scalars"),
    ("multiline-plain", "a: uno\n  dos\n", "multi-line scalar"),
    ("multiline-quoted", "a: \"uno\n  dos\"\n", "unterminated or multi-line"),
    ("malformed-list", "a:\n  - 1\n  b: 2\n", "mapping key where a sequence item was expected"),
    ("list-not-indented", "a:\n- 1\n", "sequence must be indented"),
    ("empty-value", "a:\nb: 1\n", "empty value without a nested block"),
    ("tab-indent", "a:\n\tb: 1\n", "tab in indentation"),
    ("flow-nonempty", "a: [1, 2]\n", "flow collections"),
    ("tag", "a: !!str 1\n", "anchors, aliases and tags"),
    ("doc-marker", "---\na: 1\n", "document markers"),
    ("ambiguous-yes", "a: yes\n", "ambiguous plain scalar"),
    ("ambiguous-octal", "a: 0123\n", "ambiguous plain scalar"),
    ("trailing-comment", "a: x # c\n", "' #'"),
    ("comment-lines", "# cabecera\na: 1\n  # sangrado\nb: \"#no\"\n", "OK"),
]
HEADER_CASES = [  # (name, text, ("CLAIM", expected claim_id) or ("ERR", substring of the rejection reason))
    ("v1-folded-next-action", "schema: rackcad-automation-state/v1\nautomation_state:\n  initiative: I-23\n  claim_id: fade0f8a-1438-4bfb-a894-2a9671685e60\n"
     "  next_action: >-\n    uno: dos\n    tres\n  attempts: 0\nexecution_context:\n  x: [1, 2]\n", ("CLAIM", "fade0f8a-1438-4bfb-a894-2a9671685e60")),
    ("v1-duplicate-claim-id", "schema: rackcad-automation-state/v1\nautomation_state:\n  claim_id: a\n  claim_id: b\n", ("ERR", "duplicate key")),
    ("v1-folded-claim-id", "schema: rackcad-automation-state/v1\nautomation_state:\n  claim_id: >-\n    abc\n", ("ERR", "plain scalar starts with an indicator")),
    ("v1-claim-id-continuation", "schema: rackcad-automation-state/v1\nautomation_state:\n  claim_id: abc\n    def\n", ("ERR", "continuation found")),
    ("v1-claim-id-colon", "schema: rackcad-automation-state/v1\nautomation_state:\n  claim_id: a: b\n", ("ERR", "contains ': '")),
]


def run_cases(mod):
    res = []
    for name, text, exp in CASES:
        try:
            mod.loads(text)
            got = "OK"
        except mod.YamlSubsetError as e:
            got = str(e)
        res.append({"Case": name, "Expected": exp, "Got": got, "Pass": (got == "OK") if exp == "OK" else (got != "OK" and exp in got)})
    for name, text, (kind, exp) in HEADER_CASES:
        try:
            got, err = mod.loads(text, "HEADER")["automation_state"].get("claim_id"), None
        except mod.YamlSubsetError as e:
            got, err = None, str(e)
        ok = (kind == "CLAIM" and err is None and got == exp) or (kind == "ERR" and err is not None and exp in err)
        res.append({"Case": "HEADER:" + name, "Expected": exp, "Got": got if err is None else err, "Pass": ok})
    return res


results = {"Cases": run_cases(Y)}
# round trip of the full synthetic state/v2 in the canonical form
txt = dumps(STATE_V2)
back = Y.loads(txt)
results["RoundTripStateV2"] = {"Lines": txt.count("\n"), "Equal": back == STATE_V2}
# SHA made only of digits must be quoted by the writer (otherwise it would read as an integer)
results["DigitShaQuoted"] = '"1234567890123456789012345678901234567890"' in txt and isinstance(back["protocol"]["basis"]["claim_parent_sha"], str)

# mutations of the reader: each must make at least one case fail
MUTATIONS = {
    "accept-duplicate-keys": ("raise YamlSubsetError(n, \"duplicate key: \" + k)", "pass"),
    "accept-plain-colon": ("raise YamlSubsetError(ln, \"plain scalar contains ': ' (quote it)\")", "pass"),
    "accept-block-scalars": ("raise YamlSubsetError(ln, \"block scalars (| >) are not supported\")", "pass"),
    "accept-multiline-plain": ("raise YamlSubsetError(lines[i + 1][0], \"indented content after a scalar value (multi-line scalar)\")", "pass"),
}
src = open(os.path.join(os.path.dirname(os.path.abspath(__file__)), "yaml_subset.py"), encoding="utf-8").read()
mres = {}
for name, (a, b) in MUTATIONS.items():
    assert src.count(a) == 1, name
    mod = type(sys)("mut_" + name.replace("-", "_"))
    exec(compile(src.replace(a, b), name, "exec"), mod.__dict__)
    failed = [r["Case"] for r in run_cases(mod) if not r["Pass"]]
    mres[name] = {"FailingCases": failed, "Detected": bool(failed)}
results["Mutations"] = mres

# sweep of the real state files (all refs): STRICT and HEADER
refs = ["origin/main", "origin/architecture/portabilidad-coordinador-principal", "origin/architecture/parametros-calculados-resumen-proyecto",
        "origin/architecture/workspace-persistente-rackcad", "origin/feature/rackmirror-espejo-semantico"]
seen, sweep = set(), []
for ref in refs:
    names = subprocess.check_output(["git", "-C", REPO, "ls-tree", "--name-only", ref, "docs/automation/state/"]).decode().split()
    for f in names:
        if not f.endswith(".yml"):
            continue
        blob = subprocess.check_output(["git", "-C", REPO, "rev-parse", ref + ":" + f]).decode().strip()
        if blob in seen:
            continue
        seen.add(blob)
        text = subprocess.check_output(["git", "-C", REPO, "show", ref + ":" + f]).decode("utf-8")
        row = {"Ref": ref, "File": f, "Blob": blob[:8]}
        for mode in ("STRICT", "HEADER"):
            try:
                v = Y.loads(text, mode)
                ok = mode == "STRICT" or (isinstance(v.get("automation_state"), dict) and "claim_id" in v["automation_state"])
                row[mode] = "OK" if ok else "NO_CLAIM_ID (no es un estado rackcad-automation-state)"
            except Y.YamlSubsetError as e:
                row[mode] = "ERR: " + str(e)[:90]
        sweep.append(row)
results["RealStateFiles"] = {"Distinct": len(sweep), "StrictOK": sum(r["STRICT"] == "OK" for r in sweep), "HeaderOK": sum(r["HEADER"] == "OK" for r in sweep),
                             "Rows": sweep}
ok = (all(r["Pass"] for r in results["Cases"]) and results["RoundTripStateV2"]["Equal"] and results["DigitShaQuoted"]
      and all(m["Detected"] for m in mres.values()) and all(r["HEADER"] == "OK" or r["HEADER"].startswith("NO_CLAIM_ID") for r in sweep))
results["AllPass"] = ok
json.dump(results, open(OUT, "w", encoding="utf-8", newline="\n"), ensure_ascii=False, indent=1)
print(json.dumps({"cases": "%d/%d" % (sum(r["Pass"] for r in results["Cases"]), len(results["Cases"])), "roundtrip": results["RoundTripStateV2"],
                  "mutations": {k: v["Detected"] for k, v in mres.items()}, "real": {k: results["RealStateFiles"][k] for k in ("Distinct", "StrictOK", "HeaderOK")},
                  "all": ok}, ensure_ascii=True))
sys.exit(0 if ok else 1)
