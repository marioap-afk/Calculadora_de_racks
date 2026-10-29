"""I-66 R1a, AT-2 (I-62 decisions §105 point 4): the shadow BOOTSTRAP of rackcad-automation-state/v2 for the pilot unit.

Produces, in the I-66 worktree, the rackcad-binding/v1 of Principal A (PENDING) from the AT-1 preflight already custodied in the branch, and
the BOOTSTRAP point docs/automation/evidence/I-66-R1/state.v2.yml whose StateRefs resolve in the tree of the commit that will hold it. Reuses
the accredited I-62 controls without copying them: f2-mc.py (Test-Json) and f3-mc.py (binding rules B1-B10 of README §14.2); the state text is
written in the canonical form of YamlSubset.Write from RackCad.Tests.dll built at the shadow authority revision. The validation of the point
itself (StateV2Validator) runs after the commit, on the committed tree (run_validator_r1a.ps1).

Shadow convention SH-1: there is no I62_EFFECTIVE_SHA. protocol.effective_sha records the I-62 revision whose materialized, inactive I62
texts the shadow applies, and claim_parent_contains_effective is false because the claim parent (origin/main) does not contain it.

Usage: python -B r1a_at2.py <I-66 worktree> <I-62 worktree> <RackCad.Tests.dll> <obs dir>
"""
import datetime, hashlib, importlib.util, json, os, subprocess, sys

W, I62, DLL, OBS_DIR = sys.argv[1:5]
NL = chr(10)
UNIT, BRANCH = "I-66", "fix/placedclone-copia-anotaciones"
CLAIM_ID, CLAIM_COMMIT, BASE = "e289db83-b7f7-4c6e-91c6-d2551e393055", "e1cd88bfa515258e99b34ccad6bbaec837beff13", "bb0d5522e8411f66a51fdfb3f1f0d0514b737453"
REL = "docs/automation/evidence/I-66-R1"
PF_PATH = REL + "/autoverificacion/preflight-1.json"
STATE_PATH = REL + "/state.v2.yml"
git = lambda repo, *a: subprocess.check_output(["git", "-C", repo] + list(a)).decode("utf-8").strip()
HEAD = git(W, "rev-parse", "HEAD")
AUTH_REV = git(I62, "rev-parse", "HEAD")
assert git(W, "status", "--porcelain") == "" and git(I62, "status", "--porcelain", "--", "docs/automation/agent-execution") == ""


def load_module(name, rel):
    ev = os.path.dirname(os.path.join(I62, rel))
    sys.argv = [os.path.basename(rel), I62, ev]
    spec = importlib.util.spec_from_file_location(name, os.path.join(I62, rel))
    m = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(m)
    return m


f2 = load_module("f2mc", "docs/automation/evidence/I-62-F2/f2-mc.py")
f3 = load_module("f3mc", "docs/automation/evidence/I-62-F3/f3-mc.py")
blob_at = lambda path: git(W, "rev-parse", "HEAD:" + path)
pf = json.loads(git(W, "show", "HEAD:" + PF_PATH))
gs = json.load(open(os.path.join(OBS_DIR, "obs-get-session.json"), encoding="utf-8"))["Fields"]
usage = json.load(open(os.path.join(OBS_DIR, "obs-usage.json"), encoding="utf-8"))
now = datetime.datetime.now(datetime.timezone.utc)
stamp = lambda: datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%dT%H:%M:%S.%f")[:-3] + "Z"
BID = "B%s-%s" % (now.strftime("%Y%m%dT%H%M%SZ"), hashlib.sha256((CLAIM_ID + pf["PreflightId"]).encode()).hexdigest()[:4])
BIND_PATH = REL + "/bootstrap/" + BID + ".json"
CATALOG = pf["Invalidators"]["CatalogEntryBlob"]
effort_row = next(r for r in pf["Requirements"] if r["RequirementId"] == "PRINCIPAL_COORDINATION.effort")
REQ_IDS = [rid for rid, _ in f2.principal_requirements("CUSTODY")]

binding = {
    "Schema": "rackcad-binding/v1", "BindingId": BID, "UnitId": UNIT, "Scope": "UNIT", "TaskId": None, "Role": "PRINCIPAL_COORDINATOR",
    "ProtocolSet": "rackcad-protocol/I62", "Actor": pf["Actor"], "Session": pf["Session"],
    "Cell": {"CellId": "claude-desktop-session:%s:%s" % (gs["model"], effort_row["Observation"]["Value"]), "CatalogBlob": CATALOG,
             "AdapterId": "claude-desktop-session", "Model": gs["model"], "EffortSemantic": effort_row["Observation"]["Value"], "EffortProvider": gs["effort"]},
    "PreflightRef": {"PreflightId": pf["PreflightId"],
                     "Location": {"Kind": "CUSTODIED", "Path": PF_PATH, "Commit": HEAD, "Blob": blob_at(PF_PATH)},
                     "Sha256": f3.canonical_sha256(pf)},
    "Eligibility": {
        "MeasuredInvocation": {"State": "MEASURED", "RunRef": "%s: operación de esta sesión con effort de cliente %s observada por get_session(\"self\") a las %s (preflight %s), con lecturas, escrituras, git y push en la misma sesión"
                               % (gs["sessionId"], gs["effort"], gs["lastActivityAt"], pf["PreflightId"])},
        "ConsumptionCovered": "MEASURED", "CatalogVerifiedOn": "2026-09-30", "Stale": False},
    "Independence": {"Requirements": [], "Satisfaction": []},
    "RejectedAlternatives": [
        "codex-cli: decisiones de I-62 §105, punto 6 (sin Codex) y R1 excluye la ejecución delegada",
        "codex-desktop-session: no es esta sesión; sin preflight de la unidad, el rol y la acción",
        "claude-cli: no es una sesión de nivel superior que orqueste; sin preflight",
        "claude-subagent: comparte la sesión de su padre; no puede ser el Principal; sin preflight",
    ],
    "RoutingReason": "routing.md §9 sobre §8 (PRINCIPAL_COORDINATION, CUSTODY): única candidata con preflight vigente para %s/PRINCIPAL_COORDINATOR/CUSTODY (%s), con level, effort, remote-facts, introspection y repo-write en MATCH (ConfigurationStatus %s); celda claude-desktop-session:%s:%s; catálogo model-catalog.md blob %s verificado el 2026-09-30, no STALE; consumo medido: get_usage (%s) informa plan %s, extraUsage deshabilitado y sin aviso de límite; conformidad del Owner para esta sesión como Principal A: decisiones de I-62 §106"
                     % (UNIT, pf["PreflightId"], pf["ConfigurationStatus"], gs["model"], effort_row["Observation"]["Value"], CATALOG, usage["ObservedUtc"],
                        usage["Plan"]["plan"]),
    "EscalationConditions": [
        "effort observado por debajo de Long-horizon: CUSTODY en BELOW_REQUIRED y STOP P-09 de CUSTODY (16.16)",
        "cambio de un invalidador del preflight (host, versión o ruta del adapter, autenticación, huella, blob del catálogo o de routing.md): observación nueva, sin reconfiguración automática",
        "aviso de límite de uso o de créditos: consumo no cubierto (P-10) y decisión del Coordinator",
        "binding REJECTED en el G0 en sombra: sin Principal aceptado en la sombra; el trabajo directo de I-66 sigue por WORKFLOW",
    ],
    "CountersSnapshot": {"Attempts": 0, "CorrectionLaunches": [], "BlockedReruns": [], "RebaseRecoveries": [], "Invocations": []},
    "Custody": {"FromBindingId": None, "CessionRunId": None},
    "Acceptance": {"State": "PENDING", "Basis": None, "DecisionRef": None, "AuthorizationRef": None, "MaterializationCheck": None, "Utc": None},
}
bind_bytes = (json.dumps(binding, ensure_ascii=False, indent=2) + NL).encode("utf-8")
os.makedirs(os.path.join(W, *REL.split("/"), "bootstrap"), exist_ok=True)
open(os.path.join(W, *BIND_PATH.split("/")), "wb").write(bind_bytes)

# README §14.2: phase 1 with Test-Json, then B1-B10 through f3-mc.binding_disposition with this unit's custody facts.
ok1, d1 = f2.pwsh_test_json(os.path.join(W, *BIND_PATH.split("/")), os.path.join(f2.PROTO, "schemas", "binding.v1.schema.json"))
f3.STORE[PF_PATH] = {"commit": HEAD, "blob": blob_at(PF_PATH), "content": pf}
f3.CURRENT_INVALIDATORS = dict(pf["Invalidators"])  # host, adapter, auth, fingerprint, catalog and routing observed again below
host_now = json.load(open(os.path.join(OBS_DIR, "obs-host.json"), encoding="utf-8"))
assert host_now["HostInstanceHash"] == pf["Invalidators"]["HostInstanceHash"]
assert f2.git_blob(f2.PROTO_REL + "/model-catalog.md") == CATALOG and f2.git_blob(f2.PROTO_REL + "/routing.md") == pf["Invalidators"]["RoutingBlob"]
f3.DECISIONS_PATH = "docs/automation/decisions/%s.md" % UNIT
f3.DECISIONS_TEXT["text"] = git(W, "show", "HEAD:" + f3.DECISIONS_PATH)
disp, failed = f3.binding_disposition(binding, {"action": "CUSTODY", "requirements": REQ_IDS}, schema_ok=ok1)
bval = {"BindingId": BID, "File": BID + ".json", "FileSha256": hashlib.sha256(bind_bytes).hexdigest(), "AuthorityRevision": AUTH_REV,
        "PowerShell": f2.PWSH_VERSION,
        "Phase1": {"Schema": f2.PROTO_REL + "/schemas/binding.v1.schema.json", "Blob": f2.git_blob(f2.PROTO_REL + "/schemas/binding.v1.schema.json"),
                   "Command": "Test-Json -SchemaFile", "Result": ok1, "Detail": d1},
        "Rules": "B1-B10 de README §14.2 por f3-mc.binding_disposition (blob %s)" % f2.git_blob("docs/automation/evidence/I-62-F3/f3-mc.py"),
        "Disposition": disp, "FailedRules": failed, "Expected": {"Disposition": "PENDING", "FailedRules": []},
        "ValidatedUtc": stamp(), "Accepted": ok1 and disp == "PENDING" and failed == []}
open(os.path.join(W, *(REL + "/bootstrap/" + BID + ".validation.json").split("/")), "w", encoding="utf-8", newline=NL).write(
    json.dumps(bval, ensure_ascii=False, indent=2) + NL)
assert bval["Accepted"], bval


def ref(path, blob=None):
    return {"path": path, "blob": blob or git(W, "hash-object", path)}


DEC = "docs/automation/decisions/%s.md" % UNIT
caps = ["PRINCIPAL_COORDINATION.level", "PRINCIPAL_COORDINATION.effort", "PRINCIPAL_COORDINATION.remote-facts", "PRINCIPAL_COORDINATION.introspection",
        "PRINCIPAL_COORDINATION.repo-write"]
state = {
    "schema": "rackcad-automation-state/v2",
    "automation_state": {"initiative": UNIT, "branch": BRANCH, "claim_id": CLAIM_ID, "current_phase": "DISCOVERY", "state": "claimed", "gate": "none",
                         "attempts": 0,
                         "next_action": "Discovery de I-66 por trabajo directo (WORKFLOW e INITIATIVE_LIFECYCLE). En la sombra R1a: decisión G0 en sombra del Coordinator sobre la clasificación y el binding del Principal A, sin efecto sobre el trabajo directo",
                         "last_evidence_commit": HEAD},
    "protocol": {"set": "rackcad-protocol/I62", "effective_sha": AUTH_REV,
                 "basis": {"claim_id": CLAIM_ID, "claim_commit": CLAIM_COMMIT, "claim_parent_sha": BASE, "claim_parent_contains_effective": False, "adoption_at": "G0"},
                 "g0_acceptance": {"state": "PENDING", "decision": None}},
    "custody": {"record_version": 1, "point": "BOOTSTRAP", "point_kind": None, "last_rebase": None, "rebase_history": [],
                "principal": {"state": "HELD", "binding": ref(BIND_PATH), "acceptance": {"state": "PENDING", "decision": None},
                              "preflight": ref(PF_PATH, blob_at(PF_PATH)), "designation": None, "since_record_version": 1},
                "window": {"seq": 0, "state": "CLOSED"}, "task_intent": None, "last_window": None, "chains": [], "unverified_commits": []},
    "counters": {"correction_launches": [], "blocked_reruns": [], "rebase_recoveries": [], "invocations": []},
    "execution_context": {"adapter_id": "claude-desktop-session", "session_id": gs["sessionId"], "reported_model": gs["model"], "reported_effort": gs["effort"],
                          "observed_utc": gs["lastActivityAt"]},
    "orchestration": {
        "loop": {"type": "NONE", "phase": "NONE", "instance_id": None, "object": None, "authorization": None, "action_validity": None},
        "next_action": {
            "role": "PRINCIPAL_COORDINATOR", "action": "AWAIT_COORDINATOR_DECISION", "target": None, "unit": UNIT, "gate": "G0", "task_id": None,
            "required_inputs": [ref(DEC, blob_at(DEC))], "required_capabilities": caps, "required_independence": "NOT_REQUIRED", "invocation_permission": None,
            "budget_remaining": {},
            "expected_output": "QU en sombra en un solo commit: entrada de decisiones de I-66 con los marcadores de G0 en sombra, Claim-Id y BootstrapRecordVersion 1; versión de %s con Acceptance decidida; g0_acceptance y principal.acceptance transicionados con StateRef a esos blobs" % BID,
            "completion_condition": "decisión G0 en sombra del Coordinator en %s con I62-CLASSIFICATION e I62-PRINCIPAL-BINDING: %s, Claim-Id %s y BootstrapRecordVersion: 1" % (DEC, BID, CLAIM_ID),
            "stop_conditions": [
                "registro en sombra (R1a): no autoriza ninguna acción que I-61 o WORKFLOW no autoricen",
                "P-15: sin contratos Controller/Worker ni delegaciones (R1 excluye la ejecución delegada)",
                "P-09: CUSTODY en BELOW_REQUIRED",
                "codex-cli no se invoca (decisiones de I-62 §105, punto 6)",
            ],
            "escalation_conditions": [
                "I62-CLASSIFICATION: REJECTED: la sombra se detiene; I-66 sigue como unidad DIRECT_ONLY por WORKFLOW",
                "I62-PRINCIPAL-BINDING REJECTED: sin Principal aceptado en la sombra; observación y propuesta nuevas en un QU",
            ]},
        "review_requests": [], "findings": [], "architect_budgets": [],
        "budgets": {"review_rounds": 0, "logical_requests": 0, "architect_launches": 0, "transport_reruns": [], "correction_rounds": 0, "corrections_by_lineage": [],
                    "caps": ref("docs/AUTOMATION_PLAN.md", blob_at("docs/AUTOMATION_PLAN.md"))},
        "reviewer_closures": [], "escalation": {"state": "NONE", "reason": None, "required_decision": None}, "autonomy_gaps": []},
}


def scalar(v):
    if v is None:
        return "null"
    if isinstance(v, bool):
        return "true" if v else "false"
    if isinstance(v, int):
        return str(v)
    assert '"' not in v and chr(92) not in v and all(ord(c) >= 32 for c in v), v
    return '"' + v + '"'


def emit(m, ind, out):
    sp = " " * ind
    for k, v in m.items():
        if isinstance(v, dict) and v:
            out.append(sp + k + ":")
            emit(v, ind + 2, out)
        elif isinstance(v, list) and v:
            out.append(sp + k + ":")
            for item in v:
                if isinstance(item, dict):
                    sub = []
                    emit(item, ind + 4, sub)
                    out.append(sp + "  - " + sub[0].lstrip())
                    out.extend(sub[1:])
                else:
                    out.append(sp + "  - " + scalar(item))
        else:
            out.append(sp + k + ": " + ("{}" if v == {} else "[]" if v == [] else scalar(v)))


lines = []
emit(state, 0, lines)
draft = NL.join(lines) + NL
draft_path = os.path.join(OBS_DIR, "state.v2.draft.yml")
open(draft_path, "w", encoding="utf-8", newline=NL).write(draft)
ps = ("Add-Type -Path '%s'; $t = [IO.File]::ReadAllText('%s', [Text.UTF8Encoding]::new($false)); "
      "$m = [RackCad.Tests.YamlSubset]::Read($t, [RackCad.Tests.YamlReadMode]::Strict); "
      "[IO.File]::WriteAllText('%s', [RackCad.Tests.YamlSubset]::Write($m), [Text.UTF8Encoding]::new($false))"
      % (DLL, draft_path, os.path.join(W, *STATE_PATH.split("/"))))
r = subprocess.run(["pwsh", "-NoProfile", "-Command", ps], capture_output=True, text=True, encoding="utf-8", errors="replace")
assert r.returncode == 0 and not r.stderr.strip(), r.stderr
print("ok", BID, disp, failed, HEAD[:8], AUTH_REV[:8])
