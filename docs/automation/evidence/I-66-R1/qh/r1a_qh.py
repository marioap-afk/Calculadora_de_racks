"""I-66 R1a, AT-3 (I-62 decisions §107 point 4): the shadow QH (record_version 3) that releases Principal A after the GREEN of H4.

Writes docs/automation/evidence/I-66-R1/state.v2.yml as the QH: principal RELEASED (I-S04), same accepted binding and preflight as the QU (I-P07),
acceptance StateRef to the decisions blob of this commit (append-only: it still holds the §4.1 marker), last_evidence_commit = the GREEN commit, and
the canonical next action of the successor (TAKE_CUSTODY_AFTER_DESIGNATION of decisions §8). The decisions entries §6-§8 and the handoff must already be
in the worktree, because the StateRefs are taken from the files that this commit will hold. Validated after the commit (run_validator_r1a_pair.ps1).

Usage: python -B r1a_qh.py <I-66 worktree> <RackCad.Tests.dll> <obs dir> <GREEN commit>
"""
import json, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path[:0] = [HERE, os.path.dirname(HERE)]
import r1a_state as R  # noqa: E402

W, DLL, OBS_DIR, GREEN = sys.argv[1:5]
assert len(GREEN) == 40 and R.git(W, "merge-base", "--is-ancestor", GREEN, "HEAD") == ""
BID = "B20261029T141351Z-af6a"
BIND = R.REL + "/qu/" + BID + ".json"
PF_PATH = R.REL + "/autoverificacion/preflight-1.json"
HANDOFF = R.REL + "/qh/HANDOFF-B.md"
SCHEMA = R.REL + "/qh/response.r1.schema.json"
FREEZE = "docs/initiatives/I-66-freeze.md"
dec_text = open(os.path.join(W, *R.DEC.split("/")), encoding="utf-8").read()
assert "I62-PRINCIPAL-BINDING: %s ACCEPTED" % BID in dec_text and "## 8." in dec_text
for p in (HANDOFF, SCHEMA):
    assert os.path.exists(os.path.join(W, *p.split("/"))), p
gs = json.load(open(os.path.join(OBS_DIR, "obs-get-session-qh.json"), encoding="utf-8"))
gsf = gs["Fields"]
assert gsf["model"] == "claude-opus-5-5" and gsf["effort"] == "xhigh"
open(os.path.join(W, *(R.REL + "/qh/obs-get-session.json").split("/")), "w", encoding="utf-8", newline=R.NL).write(json.dumps(gs, ensure_ascii=False, indent=2) + R.NL)
freeze_commit = R.git(W, "log", "-1", "--format=%H", "--", FREEZE)
state = {
    "schema": "rackcad-automation-state/v2",
    "automation_state": {"initiative": R.UNIT, "branch": R.BRANCH, "claim_id": R.CLAIM_ID, "current_phase": "G1", "state": "waiting", "gate": "owner-decision",
                         "attempts": 0,
                         "next_action": "Sesión nueva e independiente que abre el Owner (decisiones de I-62 §107, punto 5): autoverificación, RESUME_DECISION publicada antes del oráculo, y solo con la comparación PASS, QR del titular nuevo por la designación de las decisiones §8 y cierre de G1 por trabajo directo; el titular liberado no opera",
                         "last_evidence_commit": GREEN},
    "protocol": {"set": "rackcad-protocol/I62", "effective_sha": R.AUTH_REV,
                 "basis": {"claim_id": R.CLAIM_ID, "claim_commit": R.CLAIM_COMMIT, "claim_parent_sha": R.BASE, "claim_parent_contains_effective": False, "adoption_at": "G0"},
                 "g0_acceptance": {"state": "PENDING", "decision": None}},
    "custody": {"record_version": 3, "point": "QH", "point_kind": None, "last_rebase": None, "rebase_history": [],
                "principal": {"state": "RELEASED", "binding": R.ref(W, BIND),
                              "acceptance": {"state": "ACCEPTED", "decision": R.ref(W, R.DEC)},
                              "preflight": R.ref(W, PF_PATH), "designation": None, "since_record_version": 1},
                "window": {"seq": 0, "state": "CLOSED"}, "task_intent": None, "last_window": None, "chains": [], "unverified_commits": []},
    "counters": {"correction_launches": [], "blocked_reruns": [], "rebase_recoveries": [], "invocations": []},
    "execution_context": {"adapter_id": "claude-desktop-session", "session_id": gsf["sessionId"], "reported_model": gsf["model"], "reported_effort": gsf["effort"],
                          "observed_utc": gsf["lastActivityAt"]},
    "orchestration": {
        "loop": {"type": "NONE", "phase": "NONE", "instance_id": None, "object": None, "authorization": None, "action_validity": None},
        "next_action": {
            "role": "PRINCIPAL_COORDINATOR", "action": "TAKE_CUSTODY_AFTER_DESIGNATION",
            "target": {"commit": freeze_commit, "path": FREEZE, "blob": R.git(W, "rev-parse", freeze_commit + ":" + FREEZE)},
            "unit": R.UNIT, "gate": "G1", "task_id": None,
            "required_inputs": [R.ref(W, R.DEC), R.ref(W, HANDOFF), R.ref(W, SCHEMA), R.ref(W, FREEZE)],
            "required_capabilities": R.CAPS, "required_independence": "NOT_REQUIRED", "invocation_permission": None, "budget_remaining": {},
            "expected_output": "RESUME_DECISION de la sesión nueva publicada antes del oráculo (qh/HANDOFF-B.md); con la comparación PASS, QR ORDINARY del titular nuevo con su binding aceptado; después, el cierre de G1 por trabajo directo",
            "completion_condition": "designación de las decisiones §8 cumplida y aceptación del titular nuevo en las decisiones con I62-PRINCIPAL-BINDING: <BindingId> ACCEPTED y el Claim-Id de la unidad",
            "stop_conditions": [
                "QH: el titular liberado no opera después",
                "registro en sombra (R1a): no autoriza ninguna acción que I-61 o WORKFLOW no autoricen",
                "P-15: sin contratos Controller/Worker ni delegaciones (R1 excluye la ejecución delegada)",
                "Q0 prohibido: g0_acceptance PENDING (I-S15)",
                "P-09: CUSTODY del titular nuevo en BELOW_REQUIRED",
                "P-10: un hecho que el estado canónico no establece es UNKNOWN y no se deduce",
                "codex-cli no se invoca (decisiones de I-62 §107, punto 8)",
            ],
            "escalation_conditions": [
                "comparación de la RESUME_DECISION distinta del oráculo: STOP sin tomar la custodia y decisión del Coordinator",
                "origin/main avanzado respecto de la base bb0d5522: el rebase de WORKFLOW §4 antes de cerrar G1, con un QR REBASE_RECONCILIATION",
            ]},
        "review_requests": [], "findings": [], "architect_budgets": [],
        "budgets": {"review_rounds": 0, "logical_requests": 0, "architect_launches": 0, "transport_reruns": [], "correction_rounds": 0, "corrections_by_lineage": [],
                    "caps": R.ref(W, "docs/AUTOMATION_PLAN.md")},
        "reviewer_closures": [], "escalation": {"state": "NONE", "reason": None, "required_decision": None}, "autonomy_gaps": []},
}
R.write_canonical(state, W, DLL, OBS_DIR, "state.v2.qh.draft.yml")
print("ok QH rv3", GREEN[:8], freeze_commit[:8])
