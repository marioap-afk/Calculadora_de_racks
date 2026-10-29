"""I-66 R1a, AT-3 preparation (I-62 decisions §107 point 3): the shadow QU ORDINARY (record_version 2) after the G0 of decisions §4.

Writes, in the I-66 worktree, the decided version of the Principal A binding (Acceptance ACCEPTED, INDIVIDUAL_DECISION, DecisionRef to the shadow
marker of decisions §4.1) validated by f3-mc.py (B1-B10, B10 against the PENDING version of the BOOTSTRAP), and the QU point: principal.acceptance
ACCEPTED with a StateRef to the decisions blob that holds the marker; g0_acceptance stays PENDING (decisions §4.1: no I62 applicability admits the
unit without A-8). The point itself is validated after the commit, on the committed tree (validacion/run_validator_r1a_pair.ps1).

Usage: python -B r1a_qu.py <I-66 worktree> <I-62 worktree> <RackCad.Tests.dll> <obs dir>
"""
import copy, datetime, hashlib, json, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path[:0] = [HERE, os.path.dirname(HERE)]  # r1a_state.py: next to this script in the scratch, in the parent folder in the branch
import r1a_state as R  # noqa: E402

W, I62, DLL, OBS_DIR = sys.argv[1:5]
NL = R.NL
G0_COMMIT = R.git(W, "rev-parse", "HEAD")
assert R.git(W, "status", "--porcelain") == "" and R.git(W, "log", "-1", "--format=%s").startswith("I-66 G0: PASS")
f2, f3 = R.load_controls(I62)
BID = "B20261029T141351Z-af6a"
OLD = R.REL + "/bootstrap/" + BID + ".json"
NEW = R.REL + "/qu/" + BID + ".json"
PF_PATH = R.REL + "/autoverificacion/preflight-1.json"
MARKER = "I62-PRINCIPAL-BINDING: %s ACCEPTED" % BID
dec_text = R.git(W, "show", "HEAD:" + R.DEC)
assert MARKER in dec_text and R.CLAIM_ID in dec_text and "BootstrapRecordVersion: 1" in dec_text
assert "I62-CLASSIFICATION:" not in dec_text.split("### 4.1")[1].split("```text")[1].split("```")[0]
previous = json.loads(R.git(W, "show", "HEAD:" + OLD))
g0_utc = datetime.datetime.fromisoformat(R.git(W, "log", "-1", "--format=%cI")).astimezone(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")
binding = copy.deepcopy(previous)
binding["Acceptance"] = {"State": "ACCEPTED", "Basis": "INDIVIDUAL_DECISION", "DecisionRef": {"Path": R.DEC, "Marker": MARKER},
                         "AuthorizationRef": None, "MaterializationCheck": None, "Utc": g0_utc}
bind_bytes = (json.dumps(binding, ensure_ascii=False, indent=2) + NL).encode("utf-8")
os.makedirs(os.path.join(W, *R.REL.split("/"), "qu"), exist_ok=True)
open(os.path.join(W, *NEW.split("/")), "wb").write(bind_bytes)

pf = json.loads(R.git(W, "show", "HEAD:" + PF_PATH))
ok1, d1 = f2.pwsh_test_json(os.path.join(W, *NEW.split("/")), os.path.join(f2.PROTO, "schemas", "binding.v1.schema.json"))
f3.STORE[PF_PATH] = {"commit": binding["PreflightRef"]["Location"]["Commit"], "blob": R.git(W, "rev-parse", "HEAD:" + PF_PATH), "content": pf}
f3.CURRENT_INVALIDATORS = dict(pf["Invalidators"])
assert f2.git_blob(f2.PROTO_REL + "/model-catalog.md") == pf["Invalidators"]["CatalogEntryBlob"]
assert f2.git_blob(f2.PROTO_REL + "/routing.md") == pf["Invalidators"]["RoutingBlob"]
f3.DECISIONS_PATH = R.DEC
f3.DECISIONS_TEXT["text"] = dec_text
REQ_IDS = [rid for rid, _ in f2.principal_requirements("CUSTODY")]
disp, failed = f3.binding_disposition(binding, {"action": "CUSTODY", "requirements": REQ_IDS, "previous_version": previous}, schema_ok=ok1)
stamp = lambda: datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3] + "Z"
bval = {"BindingId": BID, "File": "qu/" + BID + ".json", "FileSha256": hashlib.sha256(bind_bytes).hexdigest(), "AuthorityRevision": R.AUTH_REV,
        "PowerShell": f2.PWSH_VERSION,
        "Phase1": {"Schema": f2.PROTO_REL + "/schemas/binding.v1.schema.json", "Blob": f2.git_blob(f2.PROTO_REL + "/schemas/binding.v1.schema.json"),
                   "Command": "Test-Json -SchemaFile", "Result": ok1, "Detail": d1},
        "Rules": "B1-B10 de README §14.2 por f3-mc.binding_disposition (blob %s); B10 contra la versión PENDING %s" % (f2.git_blob("docs/automation/evidence/I-62-F3/f3-mc.py"), OLD),
        "DecisionRef": {"Path": R.DEC, "Blob": R.git(W, "rev-parse", "HEAD:" + R.DEC), "Commit": G0_COMMIT, "Marker": MARKER},
        "Disposition": disp, "FailedRules": failed, "Expected": {"Disposition": "ACCEPTED", "FailedRules": []},
        "ValidatedUtc": stamp(), "Accepted": ok1 and disp == "ACCEPTED" and failed == []}
open(os.path.join(W, *(R.REL + "/qu/" + BID + ".validation.json").split("/")), "w", encoding="utf-8", newline=NL).write(json.dumps(bval, ensure_ascii=False, indent=2) + NL)
assert bval["Accepted"], bval

gs = json.load(open(os.path.join(OBS_DIR, "obs-get-session-qu.json"), encoding="utf-8"))
gsf = gs["Fields"]
assert gsf["sessionId"] == pf["Session"]["SessionId"] and gsf["model"] == "claude-opus-5-5" and gsf["effort"] == "xhigh"
gs_rel = R.REL + "/qu/obs-get-session.json"
open(os.path.join(W, *gs_rel.split("/")), "w", encoding="utf-8", newline=NL).write(json.dumps(gs, ensure_ascii=False, indent=2) + NL)
CONTRACT = "docs/initiatives/I-66-placedclone-copia-anotaciones.md"
state = {
    "schema": "rackcad-automation-state/v2",
    "automation_state": {"initiative": R.UNIT, "branch": R.BRANCH, "claim_id": R.CLAIM_ID, "current_phase": "DISCOVERY", "state": "implementing", "gate": "none",
                         "attempts": 0,
                         "next_action": "Discovery Core, Freeze breve con su revisión del Architect y RED → GREEN de H4 por trabajo directo del Principal A (decisiones §4); después QH en sombra con un oráculo sellado fuera del repositorio (decisiones de I-62 §107, punto 4)",
                         "last_evidence_commit": G0_COMMIT},
    "protocol": {"set": "rackcad-protocol/I62", "effective_sha": R.AUTH_REV,
                 "basis": {"claim_id": R.CLAIM_ID, "claim_commit": R.CLAIM_COMMIT, "claim_parent_sha": R.BASE, "claim_parent_contains_effective": False, "adoption_at": "G0"},
                 "g0_acceptance": {"state": "PENDING", "decision": None}},
    "custody": {"record_version": 2, "point": "QU", "point_kind": "ORDINARY", "last_rebase": None, "rebase_history": [],
                "principal": {"state": "HELD", "binding": R.ref(W, NEW),
                              "acceptance": {"state": "ACCEPTED", "decision": {"path": R.DEC, "blob": R.git(W, "rev-parse", "HEAD:" + R.DEC)}},
                              "preflight": {"path": PF_PATH, "blob": R.git(W, "rev-parse", "HEAD:" + PF_PATH)}, "designation": None, "since_record_version": 1},
                "window": {"seq": 0, "state": "CLOSED"}, "task_intent": None, "last_window": None, "chains": [], "unverified_commits": []},
    "counters": {"correction_launches": [], "blocked_reruns": [], "rebase_recoveries": [], "invocations": []},
    "execution_context": {"adapter_id": "claude-desktop-session", "session_id": gsf["sessionId"], "reported_model": gsf["model"], "reported_effort": gsf["effort"],
                          "observed_utc": gsf["lastActivityAt"]},
    "orchestration": {
        "loop": {"type": "NONE", "phase": "NONE", "instance_id": None, "object": None, "authorization": None, "action_validity": None},
        "next_action": {
            "role": "PRINCIPAL_COORDINATOR", "action": "DIRECT_WORK", "target": None, "unit": R.UNIT, "gate": "G1", "task_id": None,
            "required_inputs": [{"path": R.DEC, "blob": R.git(W, "rev-parse", "HEAD:" + R.DEC)}, {"path": CONTRACT, "blob": R.git(W, "rev-parse", "HEAD:" + CONTRACT)}],
            "required_capabilities": R.CAPS, "required_independence": "NOT_REQUIRED", "invocation_permission": None, "budget_remaining": {},
            "expected_output": "Discovery (docs/initiatives/I-66-discovery.md), Freeze breve acordado (docs/initiatives/I-66-freeze.md), RED y GREEN de H4 con su CI; después el QH en sombra",
            "completion_condition": "GREEN de H4 empujado con la CI de su SHA en success y el RED→GREEN focal registrado; QH en sombra válido para StateV2Validator",
            "stop_conditions": [
                "registro en sombra (R1a): no autoriza ninguna acción que I-61 o WORKFLOW no autoricen",
                "P-15: sin contratos Controller/Worker ni delegaciones (R1 excluye la ejecución delegada)",
                "Q0 prohibido: g0_acceptance PENDING (I-S15)",
                "P-09: CUSTODY en BELOW_REQUIRED",
                "codex-cli no se invoca (decisiones de I-62 §107, punto 8)",
            ],
            "escalation_conditions": [
                "contradicción consumida clase A en el Discovery (EXP-01): STOP y decisión del Coordinator",
                "un disparador M-01..M-08 activado o un cambio observable fuera de H4: el arquetipo y el alcance los decide el Coordinator",
                "un REQUIRED del Architect que cambie el alcance o una materia OWNER-RESERVED: Coordinator u Owner",
            ]},
        "review_requests": [], "findings": [], "architect_budgets": [],
        "budgets": {"review_rounds": 0, "logical_requests": 0, "architect_launches": 0, "transport_reruns": [], "correction_rounds": 0, "corrections_by_lineage": [],
                    "caps": {"path": "docs/AUTOMATION_PLAN.md", "blob": R.git(W, "rev-parse", "HEAD:docs/AUTOMATION_PLAN.md")}},
        "reviewer_closures": [], "escalation": {"state": "NONE", "reason": None, "required_decision": None}, "autonomy_gaps": []},
}
R.write_canonical(state, W, DLL, OBS_DIR, "state.v2.qu.draft.yml")
print("ok", BID, disp, failed, G0_COMMIT[:8], g0_utc)
