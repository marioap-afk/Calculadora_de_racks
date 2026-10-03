"""I-62 F3, manual reproducible controls C-11, C-12, C-13, C-14, C-30 and the F3 portion of C-40 (Proposal V14 Anexo C), class (ii).

Applies the procedures of docs/automation/agent-execution/README.md §14-§16 (rules B1-B10, A2', the independence evaluation, G1-G7, the
verification precedence of AUTOMATION_PLAN 16.9 with the deltas of 16.22, R1-R9, V1-V11 and the materialization criteria of §14.3) to SYNTHETIC
records. Every record is validated with PowerShell Test-Json against the F3 schemas before its coherence rules run. The expected value of each case
is copied from Proposal V14 (Anexo C and G.2) next to the case; the procedure never reads it. Each control also runs a mutation of the procedure
that must flip at least one verdict (the harness checks that it does). No runtime is invoked and no real binding is produced: the unit is
«I-62-MC» and every identity is synthetic.

Usage: python f3-mc.py <repo root> <evidence dir>
"""
import copy
import hashlib
import json
import os
import shutil
import subprocess
import sys
import tempfile

REPO, EV = sys.argv[1], sys.argv[2]
PROTO = os.path.join(REPO, "docs", "automation", "agent-execution")
UNIT = "I-62-MC"
ROLES_READ_ONLY = {"ARCHITECT", "EXECUTION_CONTROLLER", "REVIEWER"}
DIM_ORDER = {"NOT_REQUIRED": 0, "PREFERRED": 1, "REQUIRED": 2}
EFFORTS = ["Routine", "Balanced", "Deep", "Long-horizon", "Maximum"]
LEVELS = ["Eficiente", "Equilibrado", "Frontera"]
# AUTOMATION_PLAN 16.23: closed correspondence between (role, action) and output contract.
OUTPUT_CONTRACT = {
    ("ARCHITECT", "REVIEW_DESIGN"): "rackcad-architect-review-result/v1",
    ("REVIEWER", "REVIEW_CHANGE"): "rackcad-reviewer-result/v1",
    ("EXECUTION_CONTROLLER", "PLAN"): "rackcad-delegation/v2",
    ("EXECUTION_CONTROLLER", "VERIFY"): "rackcad-controller-verification/v2",
    ("WORKER", "IMPLEMENT"): "rackcad-worker-handoff/v1",
}
SCHEMA_FILE = {"rackcad-architect-review-result/v1": "architect-review-result.v1.schema.json",
               "rackcad-reviewer-result/v1": "reviewer-result.v1.schema.json"}


def h40(label):
    return hashlib.sha1(label.encode("utf-8")).hexdigest()


def h64(label):
    return hashlib.sha256(label.encode("utf-8")).hexdigest()


def canonical_sha256(obj):
    return h64(json.dumps(obj, ensure_ascii=False, sort_keys=True, separators=(",", ":")))


# ------------------------------------------------------------------ schema validation (one pwsh process for the whole batch)
PENDING = []


def expect_schema(name, obj, schema_file):
    PENDING.append((name, copy.deepcopy(obj), schema_file))


def run_schema_batch():
    tmp = tempfile.mkdtemp()
    listing = []
    for i, (name, obj, schema) in enumerate(PENDING):
        path = os.path.join(tmp, "%04d.json" % i)
        with open(path, "w", encoding="utf-8") as f:
            json.dump(obj, f, ensure_ascii=False)
        listing.append({"Name": name, "Json": path, "Schema": os.path.join(PROTO, "schemas", schema)})
    list_path = os.path.join(tmp, "list.json")
    with open(list_path, "w", encoding="utf-8") as f:
        json.dump(listing, f, ensure_ascii=False)
    script = ("$l = Get-Content -Raw -LiteralPath '%s' | ConvertFrom-Json; $o = @(foreach ($i in $l) { $ok = $false; $err = ''; "
              "try { $ok = Get-Content -Raw -LiteralPath $i.Json | Test-Json -SchemaFile $i.Schema -ErrorAction Stop } catch { $err = $_.Exception.Message }; "
              "[pscustomobject]@{Name = $i.Name; Ok = [bool]$ok; Error = $err} }); ConvertTo-Json -InputObject $o -Depth 3 -Compress") % list_path
    out = subprocess.run(["pwsh", "-NoProfile", "-Command", script], capture_output=True, text=True, encoding="utf-8", errors="replace")
    shutil.rmtree(tmp)
    rows = json.loads(out.stdout)
    return {r["Name"]: (r["Ok"], r["Error"][:200]) for r in rows}


PWSH_VERSION = subprocess.run(["pwsh", "-NoProfile", "-Command", "$PSVersionTable.PSVersion.ToString()"], capture_output=True, text=True).stdout.strip()

# ------------------------------------------------------------------ synthetic custody store
# path -> {"commit", "blob", "content"}; ANCESTORS = commits reachable from the custody point that uses the references.
STORE = {}
ANCESTORS = {h40("c1"), h40("c2"), h40("c3")}
CUSTODY_POINT = h40("c4")
DECISIONS_PATH = "docs/automation/decisions/I-62-MC.md"


def custody(path, content, commit_label="c2"):
    STORE[path] = {"commit": h40(commit_label), "blob": h40("blob:" + path + json.dumps(content, sort_keys=True)), "content": content}
    return STORE[path]


def actor(adapter, instance, assurance="RUNTIME_OBSERVED"):
    return {"AdapterId": adapter, "InstanceId": instance, "InstanceIdSource": "synthetic", "Assurance": assurance}


def session(adapter, sid, assurance="RUNTIME_OBSERVED"):
    return {"AdapterId": adapter, "SessionId": sid, "Assurance": assurance}


PROVIDER_OF = {"adapter-a-session": "provider-a", "adapter-a-cli": "provider-a", "adapter-b-cli": "provider-b"}

# ------------------------------------------------------------------ preflights (synthetic observations, README §13 shape)
CATALOG_BLOB = h40("model-catalog")
ROUTING_BLOB = h40("routing")
PRINCIPAL_REQ = [("PRINCIPAL_COORDINATION.level", "Frontera"), ("PRINCIPAL_COORDINATION.effort", "Long-horizon"),
                 ("PRINCIPAL_COORDINATION.remote-facts", "read"), ("PRINCIPAL_COORDINATION.introspection", "RUNTIME_OBSERVED"),
                 ("PRINCIPAL_COORDINATION.repo-write", "write")]
ARCHITECT_REQ = [("ARCHITECTURE_REVIEW.effort", "Deep"), ("ARCHITECTURE_REVIEW.level", "Equilibrado"), ("ARCHITECTURE_REVIEW.read", "measured")]


def preflight(pid, role, action, profile, adapter, act, ses, reqs, statuses=None, effort=None, omit=()):
    statuses = statuses or {}
    rows = []
    for rid, required in reqs:
        if rid in omit:
            continue
        status = statuses.get(rid, "MATCH")
        value = (effort if rid.endswith(".effort") and effort else required)
        obs = ({"State": "OBSERVED", "Value": value, "Source": "synthetic", "Assurance": "RUNTIME_OBSERVED", "ObservedUtc": "2026-10-02T23:00:00Z"}
               if status != "UNKNOWN" else {"State": "NOT_OBSERVED", "Value": None, "Source": None, "Assurance": "NONE", "ObservedUtc": None})
        rows.append({"RequirementId": rid, "Mandatory": True, "Required": required, "Observation": obs, "Status": status,
                     "Contradiction": False, "ContradictionEvidence": None})
    agg = "BELOW_REQUIRED" if any(r["Status"] == "BELOW_REQUIRED" for r in rows) else (
        "UNKNOWN" if any(r["Status"] == "UNKNOWN" for r in rows) else "MATCH")
    return {
        "Schema": "rackcad-preflight/v1", "PreflightId": pid, "UnitId": UNIT, "Role": role, "Action": action, "ProtocolSet": "rackcad-protocol/I62",
        "Profile": {"ProfileId": profile, "RoutingBlob": ROUTING_BLOB},
        "Host": {"HostLabelHash": h64("host"), "HostInstanceHash": h64("host-instance"), "HostInstanceState": "OBSERVED", "Os": "synthetic"},
        "ObservedUtc": "2026-10-02T23:00:00Z", "Actor": act, "Session": ses,
        "Adapter": {"AdapterId": adapter, "AdapterVersion": "1.0.0", "BinaryPathHash": None, "DescriptorRef": {"Path": "adapters/%s.md" % adapter, "Blob": h40("d:" + adapter)}},
        "AdapterFacts": {"SchemaRef": {"SchemaId": "rackcad-adapter-%s-facts/v1" % adapter, "Path": "schemas/adapters/%s.facts.v1.schema.json" % adapter,
                                       "Blob": h40("f:" + adapter)}, "Facts": {"SchemaId": "rackcad-adapter-%s-facts/v1" % adapter}},
        "Fingerprint": {"Kind": "UNVERIFIED", "Sha256": None, "KeyNames": [], "AcceptanceDecisionRef": None},
        "Requirements": rows, "ConfigurationStatus": agg, "Causes": [], "Disposition": "ELIGIBLE" if agg == "MATCH" else "NOT_ELIGIBLE",
        "StopConditions": [],
        "Invalidators": {"HostInstanceHash": h64("host-instance"), "AdapterVersion": "1.0.0", "BinaryPathHash": None, "AuthState": "AUTHENTICATED",
                         "FingerprintSha256": "UNKNOWN", "CatalogEntryBlob": CATALOG_BLOB, "RoutingBlob": ROUTING_BLOB},
    }


# The current observation of each host fact; a preflight whose Invalidators differ is stale (README §13.3).
CURRENT_INVALIDATORS = {"HostInstanceHash": h64("host-instance"), "AdapterVersion": "1.0.0", "BinaryPathHash": None, "AuthState": "AUTHENTICATED",
                        "FingerprintSha256": "UNKNOWN", "CatalogEntryBlob": CATALOG_BLOB, "RoutingBlob": ROUTING_BLOB}
PREFLIGHTS = {}


def put_preflight(p):
    PREFLIGHTS[p["PreflightId"]] = p
    return p


def preflight_ref(p, kind="CUSTODIED"):
    path = "docs/automation/evidence/I-62-MC/preflights/%s.json" % p["PreflightId"]
    rec = custody(path, p)
    return {"PreflightId": p["PreflightId"], "Location": {"Kind": kind, "Path": path, "Commit": rec["commit"], "Blob": rec["blob"]},
            "Sha256": canonical_sha256(p)}


# ------------------------------------------------------------------ bindings
def counters():
    return {"Attempts": 0, "CorrectionLaunches": [], "BlockedReruns": [], "RebaseRecoveries": [], "Invocations": []}


def binding(bid, role, p, cell_model, effort, independence_req, satisfaction, acceptance, scope="UNIT", task=None, adapter=None, eligibility=None):
    adapter = adapter or p["Adapter"]["AdapterId"]
    return {
        "Schema": "rackcad-binding/v1", "BindingId": bid, "UnitId": UNIT, "Scope": scope, "TaskId": task, "Role": role, "ProtocolSet": "rackcad-protocol/I62",
        "Actor": copy.deepcopy(p["Actor"]), "Session": copy.deepcopy(p["Session"]),
        "Cell": {"CellId": "%s:%s:%s" % (adapter, cell_model or "-", effort), "CatalogBlob": CATALOG_BLOB, "AdapterId": adapter, "Model": cell_model,
                 "EffortSemantic": effort, "EffortProvider": None},
        "PreflightRef": preflight_ref(p),
        "Eligibility": eligibility or {"MeasuredInvocation": {"State": "MEASURED", "RunRef": "R20261002T230000Z-0001"}, "ConsumptionCovered": "OFFICIAL",
                                       "CatalogVerifiedOn": "2026-10-01", "Stale": False},
        "Independence": {"Requirements": independence_req, "Satisfaction": satisfaction},
        "RejectedAlternatives": [], "RoutingReason": "synthetic", "EscalationConditions": [], "CountersSnapshot": counters(),
        "Custody": {"FromBindingId": None, "CessionRunId": None}, "Acceptance": acceptance,
    }


def pending():
    return {"State": "PENDING", "Basis": None, "DecisionRef": None, "AuthorizationRef": None, "MaterializationCheck": None, "Utc": None}


def individual(state, marker):
    return {"State": state, "Basis": "INDIVIDUAL_DECISION", "DecisionRef": {"Path": DECISIONS_PATH, "Marker": marker}, "AuthorizationRef": None,
            "MaterializationCheck": None, "Utc": "2026-10-02T23:10:00Z"}


DECISIONS_TEXT = {"text": ""}


# ------------------------------------------------------------------ README §14.5: independence evaluation
def dimension(dim, cand, ref):
    """cand / ref: {"Actor", "Session", "Context" (CLEAN | SHARED | UNKNOWN), "Adapter"}."""
    if dim in ("Actor", "Session"):
        a, b = cand[dim], ref[dim]
        if a is None or b is None or a.get("Assurance") == "NONE" or b.get("Assurance") == "NONE":
            return "UNKNOWN"
        key = ("AdapterId", "InstanceId") if dim == "Actor" else ("AdapterId", "SessionId")
        return "NOT_SATISFIED" if tuple(a[k] for k in key) == tuple(b[k] for k in key) else "SATISFIED"
    if dim == "Context":
        return {"CLEAN": "SATISFIED", "SHARED": "NOT_SATISFIED"}.get(cand["Context"], "UNKNOWN")
    pa, pb = PROVIDER_OF.get(cand["Adapter"]), PROVIDER_OF.get(ref["Adapter"])
    return "UNKNOWN" if pa is None or pb is None else ("SATISFIED" if pa != pb else "NOT_SATISFIED")


def combine(triggers):
    """Maximum per dimension (REQUIRED > PREFERRED > NOT_REQUIRED) over the triggers that apply to one reference."""
    out = {}
    for t in triggers:
        for dim, value in t.items():
            if dim not in out or DIM_ORDER[value] > DIM_ORDER[out[dim]]:
                out[dim] = value
    return out


def evaluate(requirement, cand, refs):
    """Returns (decision, rows): ACCEPTABLE if every REQUIRED dimension is SATISFIED against every reference; PREFERRED misses are recorded."""
    rows, missing, recorded = [], [], []
    for ref in refs:
        for dim in ("Actor", "Session", "Context", "Provider"):
            level = requirement.get(dim, "NOT_REQUIRED")
            if level == "NOT_REQUIRED":
                continue
            result = dimension(dim, cand, ref)
            rows.append({"Reference": ref["Label"], "Dimension": dim, "Level": level, "Result": result})
            if result != "SATISFIED":
                (missing if level == "REQUIRED" else recorded).append((ref["Label"], dim, result))
    return ("NOT_ACCEPTED" if missing else "ACCEPTABLE"), rows, missing, recorded


def lifecycle_predicate(policy, reviewer, subject):
    """INITIATIVE_LIFECYCLE §5 for I62 units (OD-6 alternative 1) and, for C-30 (d), the alternative 2 policy. reviewer: {"Mode", "Actor",
    "Session", "Human", "ReviewInstance", "Context", "Adapter"}; subject: ReviewSubject-like with "Authors" (already bounded to the unit and the object)."""
    if policy == "ALT2":
        return ("SATISFIED" if reviewer["Mode"] in ("SAME-SESSION ROLE", "SEPARATE SESSION", "EXTERNAL HUMAN") else "NOT_SATISFIED"), []
    if reviewer["Mode"] == "SAME-SESSION ROLE":
        return "NOT_SATISFIED", [("mode", "SAME-SESSION ROLE no basta")]
    unknown, failed = [], []
    for author in subject["Authors"]:
        if author["Kind"] == "UNKNOWN":
            unknown.append((author["Evidence"], "autor no establecible"))
            continue
        if reviewer["Mode"] == "SEPARATE SESSION":
            if author["Kind"] == "AI_SESSION":
                for dim in ("Actor", "Session"):
                    r = dimension(dim, {dim: reviewer[dim]}, {dim: author[dim]})
                    (failed if r == "NOT_SATISFIED" else unknown if r == "UNKNOWN" else []).append((author["Evidence"], dim))
        else:  # EXTERNAL HUMAN
            humans = []
            if author["Kind"] == "HUMAN":
                humans.append(author["HumanId"])
            if author["Kind"] == "AI_SESSION":
                if author["Operator"] in (None, "UNKNOWN"):
                    unknown.append((author["Evidence"], "operador UNKNOWN"))
                else:
                    humans.append(author["Operator"])
            if reviewer["Human"] in humans:
                failed.append((author["Evidence"], "Actor"))
    if reviewer["Mode"] == "SEPARATE SESSION" and reviewer["Context"] != "CLEAN":
        (failed if reviewer["Context"] == "SHARED" else unknown).append(("reviewer", "Context"))
    if reviewer["Mode"] == "EXTERNAL HUMAN" and (reviewer.get("Actor") is not None or reviewer.get("ReviewInstance") is None):
        failed.append(("reviewer", "identidad ficticia o sin ReviewInstanceRef"))
    if failed:
        return "NOT_SATISFIED", failed
    return ("UNKNOWN" if unknown else "SATISFIED"), unknown


def bounded_authors(subject_commits, unit_start):
    """ReviewSubject: authors of the commits of the unit's bounded set; commits before the unit are excluded (B.2)."""
    return [a for a in subject_commits if a["Seq"] >= unit_start]


# ------------------------------------------------------------------ README §14.2: binding coherence
def binding_disposition(b, ctx, schema_ok=True, unknown_as_satisfied=False):
    failures = []
    if not schema_ok:
        failures.append(("B1", "REJECTED"))
    if (b["Scope"] == "TASK") != (b["TaskId"] is not None):
        failures.append(("B2", "REJECTED"))
    p = None
    loc = b["PreflightRef"]["Location"]
    rec = STORE.get(loc["Path"])
    if rec and rec["commit"] == loc["Commit"] and rec["blob"] == loc["Blob"] and canonical_sha256(rec["content"]) == b["PreflightRef"]["Sha256"]:
        p = rec["content"]
    if p is None:
        failures.append(("B4", "P-10"))
    else:
        adapter = p["Adapter"]["AdapterId"]
        effort_row = next((r for r in p["Requirements"] if r["RequirementId"].endswith(".effort")), None)
        observed_effort = effort_row["Observation"]["Value"] if effort_row else None
        cell = b["Cell"]
        expected_id = "%s:%s:%s" % (cell["AdapterId"], cell["Model"] or "-", cell["EffortSemantic"])
        allowed = ctx.get("contract_cells")
        if not (cell["AdapterId"] == b["Actor"]["AdapterId"] == b["Session"]["AdapterId"] == adapter) or cell["CellId"] != expected_id \
                or cell["EffortSemantic"] != observed_effort or cell["CatalogBlob"] != p["Invalidators"]["CatalogEntryBlob"] \
                or (allowed is not None and cell["CellId"] not in allowed):
            failures.append(("B3", "REJECTED"))
        stale = any(p["Invalidators"].get(k) != v for k, v in CURRENT_INVALIDATORS.items())
        same_scope = (p["UnitId"], p["Role"], p["Action"]) == (b["UnitId"], b["Role"], ctx["action"])
        rows = {r["RequirementId"]: r for r in p["Requirements"]}
        required = ctx["requirements"]
        incomplete = [rid for rid in required if rid not in rows]
        bad = [rid for rid in required if rid in rows and rows[rid]["Status"] not in ("MATCH", "ABOVE_REQUIRED")
               and not (unknown_as_satisfied and rows[rid]["Status"] == "UNKNOWN")]
        if stale or not same_scope or incomplete or bad:
            failures.append(("B4", "P-10"))
    e = b["Eligibility"]
    measured_ok = e["MeasuredInvocation"]["State"] == "MEASURED" and e["MeasuredInvocation"]["RunRef"] is not None
    coherent_run = (e["MeasuredInvocation"]["RunRef"] is None) == (e["MeasuredInvocation"]["State"] == "NOT_MEASURED")
    if not measured_ok or not coherent_run or e["ConsumptionCovered"] == "UNKNOWN" or e["Stale"]:
        failures.append(("B5", "P-10"))
    ind = b["Independence"]
    keys = [(s["ReferenceRole"], s["Dimension"]) for s in ind["Satisfaction"]]
    covered = True
    for want in ctx.get("contract_independence", []):
        got = next((r for r in ind["Requirements"] if r["ReferenceRole"] == want["ReferenceRole"]), None)
        if got is None or any(DIM_ORDER[got[d]] < DIM_ORDER[want[d]] for d in ("Actor", "Session", "Context", "Provider")):
            covered = False
    req_dims = [(r["ReferenceRole"], d, r[d]) for r in ind["Requirements"] for d in ("Actor", "Session", "Context", "Provider") if r[d] != "NOT_REQUIRED"]
    sat = {(s["ReferenceRole"], s["Dimension"]): s["Result"] for s in ind["Satisfaction"]}
    rows_ok = len(keys) == len(set(keys)) and all((rr, d) in sat for rr, d, _ in req_dims)
    required_ok = all(sat.get((rr, d)) == "SATISFIED" or (unknown_as_satisfied and sat.get((rr, d)) == "UNKNOWN") for rr, d, lv in req_dims if lv == "REQUIRED")
    if not (covered and rows_ok and required_ok):
        failures.append(("B6", "NOT_ACCEPTED"))
    a = b["Acceptance"]
    combos = {
        "PENDING": a["Basis"] is None and a["DecisionRef"] is None and a["AuthorizationRef"] is None and a["MaterializationCheck"] is None and a["Utc"] is None,
        "INDIVIDUAL_DECISION": a["DecisionRef"] is not None and a["AuthorizationRef"] is None and a["MaterializationCheck"] is None,
        "AUTHORIZED_MATERIALIZATION": a["State"] == "ACCEPTED" and a["DecisionRef"] is None and a["AuthorizationRef"] is not None and a["MaterializationCheck"] is not None,
    }
    kind = "PENDING" if a["State"] == "PENDING" else a["Basis"]
    if kind not in combos or not combos[kind]:
        failures.append(("B7", "P-20"))
    if a["Basis"] == "AUTHORIZED_MATERIALIZATION" and not (b["Role"] == "ARCHITECT" or (b["Role"] == "REVIEWER" and ctx.get("reviewer_materialization"))):
        failures.append(("B8", "P-20"))
    if a["Basis"] == "INDIVIDUAL_DECISION" and a["DecisionRef"] is not None:
        marker = a["DecisionRef"]["Marker"]
        if a["DecisionRef"]["Path"] != DECISIONS_PATH or marker not in DECISIONS_TEXT["text"] or b["BindingId"] not in marker or a["State"] not in marker:
            failures.append(("B9", "P-20"))
    previous = ctx.get("previous_version")
    if previous is not None and previous["BindingId"] == b["BindingId"]:
        x, y = copy.deepcopy(previous), copy.deepcopy(b)
        x.pop("Acceptance"), y.pop("Acceptance")
        legal = previous["Acceptance"]["State"] == "PENDING" and b["Acceptance"]["State"] in ("ACCEPTED", "REJECTED") and x == y
        if not legal:
            failures.append(("B10", "S-04"))
    severity = ["S-04", "P-20", "P-10", "NOT_ACCEPTED", "REJECTED"]
    if not failures:
        return ("ACCEPTED" if a["State"] == "ACCEPTED" else a["State"]), []
    worst = min((f[1] for f in failures), key=severity.index)
    return ("REJECTED/" + worst if worst in ("P-10", "P-20") else worst), sorted({f[0] for f in failures})


# ------------------------------------------------------------------ README §14.4: references (A2')
def binding_ref(b, kind="CUSTODIED", commit_label="c2"):
    path = "docs/automation/evidence/I-62-MC/bindings/%s@%s.json" % (b["BindingId"], commit_label)
    if kind == "CUSTODIED":
        rec = custody(path, b, commit_label)
        loc = {"Kind": "CUSTODIED", "Path": path, "Commit": rec["commit"], "Blob": rec["blob"]}
    else:
        loc = {"Kind": "TRANSIENT", "Path": ".agent-runs/I-62-MC/R1/binding.json", "Commit": None, "Blob": None}
    return {"UnitId": b["UnitId"], "Scope": b["Scope"], "TaskId": b["TaskId"], "Role": b["Role"], "BindingId": b["BindingId"],
            "Sha256": canonical_sha256(b), "Location": loc}


def a2_reference(ref, contract, all_bindings, custodied_artifact=True):
    fails = []
    if ref["UnitId"] != contract["Unit"]:
        fails.append("otra unidad")
    if (ref["Scope"] == "TASK" and ref["TaskId"] != contract["TaskId"]) or (ref["Scope"] == "UNIT" and ref["TaskId"] is not None):
        fails.append("otra tarea")
    loc = ref["Location"]
    if loc["Kind"] == "CUSTODIED":
        rec = STORE.get(loc["Path"])
        if rec is None or rec["commit"] != loc["Commit"] or rec["blob"] != loc["Blob"] or loc["Commit"] not in ANCESTORS \
                or canonical_sha256(rec["content"]) != ref["Sha256"] or rec["content"]["BindingId"] != ref["BindingId"]:
            fails.append("otra revisión o no resuelve")
    elif custodied_artifact:
        fails.append("TRANSIENT presentado como custodiado")
    later = [x for x in all_bindings if x["Role"] == ref["Role"] and x["UnitId"] == ref["UnitId"] and x["Scope"] == ref["Scope"]
             and x["TaskId"] == ref["TaskId"] and x["Seq"] > next((y["Seq"] for y in all_bindings if y["BindingId"] == ref["BindingId"]), 10 ** 9)]
    if later:
        fails.append("obsoleto por rebinding")
    return ("pass" if not fails else "rechazo A2'"), fails


# ------------------------------------------------------------------ AUTOMATION_PLAN 16.9 + 16.22: verification precedence and 16.8 counting
CHECK_ORDER = ["Termination", "Handoff", "Authority", "Contract", "Identity", "Remote", "Scope", "CleanTree", "Ci", "Tests", "Trailer", "Routing",
               "FreeText", "Denials"]


def row_disposition(check, detail, results):
    if check == "Termination":
        return "BLOCKED"
    if check == "Handoff":
        if detail == "absent":
            return "BLOCKED"
        return "REWORK" if results.get("Identity") == "pass" else "STOP"
    if check in ("Authority", "Contract", "Identity", "Scope"):
        return "STOP"
    if check == "Remote":
        return "STOP" if detail == "main-advanced" else "BLOCKED"
    if check == "Ci":
        return "BLOCKED" if detail in ("absent", "running") else "REWORK"
    if check == "Routing":
        return "BLOCKED"
    if check == "Denials":
        return "STOP" if detail == "credentials" else "BLOCKED"
    return "REWORK"


def classify(results, details, precedence=("STOP", "BLOCKED", "REWORK")):
    """results: id -> pass | fail | not_run | not_run_by_prior (a not_run caused by an earlier fail, without disposition)."""
    first = next((c for c in CHECK_ORDER if results[c] in ("fail", "not_run")), None)
    if first is None:
        return {"FailureClass": "NONE", "Classification": "VERIFIED", "Disposition": "NONE"}
    dispositions = [row_disposition(c, details.get(c), results) for c in CHECK_ORDER if results[c] in ("fail", "not_run")]
    worst = next(d for d in precedence if d in dispositions)
    classification = {"STOP": "BLOCKED", "BLOCKED": "BLOCKED", "REWORK": "REWORK_REQUIRED"}[worst]
    return {"FailureClass": first, "Classification": classification, "Disposition": worst}


def all_pass(**override):
    r = {c: "pass" for c in CHECK_ORDER}
    r.update(override)
    return r


def routing_result(enforcement, effective, requested, observed_level):
    """16.22: Routing with the added case (effective not observed at the minimum level)."""
    if observed_level != "RUNTIME_OBSERVED" or effective is None:
        return ("fail" if enforcement == "required" else "pass"), "limitación: efectivo no observado al nivel mínimo"
    if effective != requested:
        return ("fail" if enforcement == "required" else "pass"), "discrepancia anotada"
    return "pass", ""


def scope_result(evidence, allowed):
    """16.22 / README §14.7: pass only with the reproducible diff and the membership of every path; a missing or failed comparison is not_run."""
    if evidence.get("ComparisonFailed") or evidence.get("DiffNameOnly") is None or evidence.get("Membership") is None:
        return "not_run"

    def covers(entry, path):
        return path == entry or (entry.endswith("/") and path.startswith(entry))

    for path in evidence["DiffNameOnly"]:
        declared = evidence["Membership"].get(path)
        if declared is None or declared not in allowed or not covers(declared, path):
            return "fail" if not any(covers(e, path) for e in allowed) else "not_run"
    return "pass"


class Counters:
    def __init__(self):
        self.attempts, self.per_class, self.blocked, self.invocations = 0, {}, {}, 0

    def invoke(self):
        self.invocations += 1

    def correction(self, failure_class):
        self.attempts += 1
        self.per_class[failure_class] = self.per_class.get(failure_class, 0) + 1

    def rerun(self, phase):
        self.blocked[phase] = self.blocked.get(phase, 0) + 1

    def snapshot(self):
        return {"attempts": self.attempts, "per_class": dict(self.per_class), "blocked": dict(self.blocked), "invocations": self.invocations}


# ------------------------------------------------------------------ README §14.3: materialization criteria
def materialization_check(cand, auth, now, unknown_as_satisfied=False):
    crit = []

    def add(cid, required, observed, result):
        crit.append({"CriterionId": cid, "Required": required, "Observed": observed, "Result": result, "Evidence": "synthetic"})

    add("ROLE_ACTION", "%s/%s" % (auth["Role"], ",".join(auth["AuthorizedActions"])), "%s/%s" % (cand["Role"], cand["Action"]),
        "SATISFIED" if cand["Role"] == auth["Role"] and cand["Action"] in auth["AuthorizedActions"] else "NOT_SATISFIED")
    for c in auth["MinimumCapabilities"]:
        st = cand["Capabilities"].get(c["RequirementId"], "UNKNOWN")
        add("CAPABILITY:" + c["RequirementId"], c["Required"], st,
            "SATISFIED" if st in ("MATCH", "ABOVE_REQUIRED") else ("UNKNOWN" if st == "UNKNOWN" else "NOT_SATISFIED"))
    for r in auth["RequiredIndependence"]:
        for dim in ("Actor", "Session", "Context", "Provider"):
            if r[dim] == "NOT_REQUIRED":
                continue
            res = cand["Independence"].get(dim, "UNKNOWN")
            if r[dim] == "PREFERRED" and res != "SATISFIED":
                continue  # recorded, does not block
            add("INDEPENDENCE:%s:%s" % (r["ReferenceRole"], dim), r[dim], res, res)
    cells = auth["EligibleCells"]
    add("CELL", cells["Mode"], cand["CellId"], "SATISFIED" if cells["Mode"] == "LIST" and cand["CellId"] in cells["Cells"] else "NOT_SATISFIED")
    b = auth["ModelEffortBounds"]
    in_bounds = (EFFORTS.index(cand["Effort"]) >= EFFORTS.index(b["MinEffort"]) and LEVELS.index(cand["Level"]) >= LEVELS.index(b["MinLevel"])
                 and (b["MaxEffort"] is None or EFFORTS.index(cand["Effort"]) <= EFFORTS.index(b["MaxEffort"]))
                 and (b["MaxLevel"] is None or LEVELS.index(cand["Level"]) <= LEVELS.index(b["MaxLevel"])))
    add("MODEL_EFFORT", json.dumps(b, sort_keys=True), "%s/%s" % (cand["Level"], cand["Effort"]), "SATISFIED" if in_bounds else "NOT_SATISFIED")
    add("PERMISSIONS", "READ_ONLY", cand["Permissions"], "SATISFIED" if cand["Permissions"] == "READ_ONLY" else "NOT_SATISFIED")
    fam = auth["ObjectFamily"]
    import fnmatch
    add("OBJECT", fam["Unit"] + " " + fam["PathPattern"], cand["ObjectUnit"] + " " + cand["ObjectPath"],
        "SATISFIED" if cand["ObjectUnit"] == fam["Unit"] and fnmatch.fnmatch(cand["ObjectPath"], fam["PathPattern"]) else "NOT_SATISFIED")
    ended = auth.get("Ended")
    add("VALIDITY", "<= " + auth["Validity"]["Until"] + " y sin fin registrado", now + (" fin=" + ended if ended else ""),
        "SATISFIED" if ended is None and now <= auth["Validity"]["Until"] else "NOT_SATISFIED")
    ok = all(c["Result"] == "SATISFIED" or (unknown_as_satisfied and c["Result"] == "UNKNOWN") for c in crit)
    return ok, crit


def reproduce_materialized(b, auth_store, candidate):
    """README §14.3 «Reproducción»: A7' re-reads the authorization at AuthorizationRef.Commit/Blob and recomputes every criterion."""
    a = b["Acceptance"]
    if a["DecisionRef"] is not None or a["AuthorizationRef"] is None or a["MaterializationCheck"] is None:
        return "REJECTED/P-20", "DecisionRef no nulo o sin AuthorizationRef"
    ref = a["AuthorizationRef"]
    entry = auth_store.get((ref["Commit"], ref["Blob"]))
    if entry is None or ref["Commit"] == CUSTODY_POINT or ref["Commit"] not in ANCESTORS or ("I62-REVIEW-LOOP-AUTHORIZATION: " + ref["AuthorizationId"]) not in entry["text"]:
        return "REJECTED/P-20", "la autorización no se reproduce en Commit/Blob"
    ok, crit = materialization_check(candidate, entry["auth"], a["Utc"])
    if not ok or [c["Result"] for c in crit] != [c["Result"] for c in a["MaterializationCheck"]["Criteria"]]:
        return "REJECTED/P-20", "la comprobación no se reproduce"
    return "ACCEPTED", "reproducida"


# ------------------------------------------------------------------ README §15.1 and §16: invocation and result rules
def invocation_disposition(inv, accepted_bindings, schema_ok=True, table=OUTPUT_CONTRACT):
    fails = []
    if not schema_ok:
        return "REJECTED", ["schema"]
    if table.get((inv["RequestedRole"], inv["Action"])) != inv["OutputContract"]:
        fails.append(("R1", "P-19"))
    if inv["Target"] is None and inv["Action"] != "PLAN":
        fails.append(("R2", "REJECTED"))
    if inv["RequestedRole"] in ROLES_READ_ONLY and inv["Permissions"] != "READ_ONLY":
        fails.append(("R3", "REJECTED"))
    bnd = accepted_bindings.get(inv["Binding"]["BindingId"])
    if bnd is None or bnd["Role"] != inv["RequestedRole"]:
        fails.append(("R4", "P-10"))
    forbidden = set(inv["ForbiddenInputs"])
    if any(i["Path"] in forbidden for i in inv["CanonicalInputs"] + inv["AllowedTransitiveInputs"]):
        fails.append(("R6", "REJECTED"))
    if any(x["Class"] == "EXEMPTED" and (x["ExemptionRef"] is None or x["ExemptionScope"] is None) for x in inv["AllowedActions"]):
        fails.append(("R7", "REJECTED"))
    if not fails:
        return "ACCEPTED", []
    order = ["P-10", "P-19", "REJECTED"]
    worst = min((f[1] for f in fails), key=order.index)
    return ("INVALID/P-19" if worst == "P-19" else "REJECTED/P-10" if worst == "P-10" else "REJECTED"), sorted({f[0] for f in fails})


def result_disposition(res, inv, schema_name, observed_identity, purpose="ARCHITECT_SATISFIED"):
    fails = []
    if SCHEMA_FILE.get(inv["OutputContract"]) != schema_name or res["RequestedRole"] != inv["RequestedRole"] or res["Action"] != inv["Action"]:
        fails.append(("V1", "P-19"))
    t = inv["Target"]
    if res["InvocationId"] != inv["InvocationId"] or (t and (res["ReviewedCommit"], res["ReviewedPath"], res["ReviewedBlob"]) != (t["commit"], t["path"], t["blob"])):
        fails.append(("V2", "P-19"))
    icd = res["InjectedContextDeclaration"]
    if icd["State"] != "DECLARED" or len(icd["Items"]) != len(inv["DeclaredRuntimeContext"]):
        fails.append(("V3", "P-19"))
    if res["RequestedRole"] == "ARCHITECT":
        if any(not (f["FindingId"] and f["AffectedSection"] and f["Evidence"] and f["CorrectionRequired"]) for f in res["RequiredFindings"]):
            fails.append(("V4", "P-19"))
        open_required = {f["FindingId"] for f in inv["OpenFindings"] if f["Severity"] == "REQUIRED"}
        closed = {d["FindingId"] for d in res["FindingDispositions"] if d["State"] in ("CLOSED", "SUPERSEDED")}
        if res["Verdict"] == "AGREED" and (res["RequiredFindings"] or not open_required <= closed):
            fails.append(("V5", "P-19"))
        if res["Verdict"] == "CHANGES REQUIRED" and not (res["RequiredFindings"] or open_required - closed):
            fails.append(("V6", "P-19"))
        if res["Verdict"].startswith("BLOCKED") and not res["OwnerDecisions"]:
            fails.append(("V6", "P-19"))
    if any((d["EvaluatedObject"]["commit"], d["EvaluatedObject"]["path"], d["EvaluatedObject"]["blob"]) != (res["ReviewedCommit"], res["ReviewedPath"], res["ReviewedBlob"])
           for d in res["FindingDispositions"]):
        fails.append(("V7", "P-19"))
    declared = res["ReviewerDeclaredIdentity"]
    if declared["SessionOrThread"] not in ("UNKNOWN", observed_identity["SessionOrThread"]) or declared["Model"] not in ("UNKNOWN", observed_identity["Model"]):
        fails.append(("V8", "P-23"))
    if res["RequestedRole"] == "REVIEWER" and purpose in ("ARCHITECT_SATISFIED", "CLOSE_ARCHITECT_FINDING"):
        fails.append(("V10", "P-20"))
    if not fails:
        return "INGESTED", []
    order = ["P-20", "P-23", "P-19"]
    worst = min((f[1] for f in fails), key=order.index)
    return "REJECTED/" + worst if worst != "P-19" else "INVALID/P-19", sorted({f[0] for f in fails})


# ================================================================== normative anchors
# Each control applies a written procedure; if the procedure is absent from the tree, the control fails (absence-driven RED).
ANCHORS = {
    "C-11": [("docs/automation/agent-execution/README.md", "### 14.2 Coherencia del binding"), ("docs/AUTOMATION_PLAN.md", "### 16.20 Binding y aceptación (I62)")],
    "C-12": [("docs/automation/agent-execution/README.md", "### 14.4 Referencias (A2')")],
    "C-13": [("docs/automation/agent-execution/README.md", "### 14.5 Evaluación de la independencia"),
             ("docs/AUTOMATION_PLAN.md", "### 16.21 Independencia por riesgo (I62)"),
             ("docs/INITIATIVE_LIFECYCLE.md", "**Unidades I62 (materializado por I-62; inactivo hasta su vigencia, AUTOMATION_PLAN 16.14).**")],
    "C-14": [("docs/automation/agent-execution/README.md", "### 14.7 A1'-A8' y las comprobaciones de la verificación"),
             ("docs/AUTOMATION_PLAN.md", "### 16.22 Aceptación del paquete y comprobaciones de la verificación (I62)")],
    "C-30": [("docs/automation/agent-execution/README.md", "### 15.1 Construcción de la `RoleInvocation`"),
             ("docs/automation/agent-execution/README.md", "## 16. Validación de los resultados por rol (unidades I62)"),
             ("docs/AUTOMATION_PLAN.md", "### 16.23 Orquestación de roles (I62): invocación de rol y contratos de salida")],
    "C-40": [("docs/automation/agent-execution/README.md", "### 14.3 Materialización autorizada")],
}


def anchors_present(control):
    out = {}
    for rel, line in ANCHORS[control]:
        path = os.path.join(REPO, rel)
        text = open(path, encoding="utf-8").read().replace(chr(13) + chr(10), chr(10)) if os.path.exists(path) else ""
        out[rel + " :: " + line] = any(l.strip() == line or l.startswith(line) for l in text.split(chr(10)))
    return out


# ================================================================== cases
RESULTS = {}


def record(control, case, expected, got, extra=None):
    RESULTS.setdefault(control, {"Cases": []})["Cases"].append(dict({"Case": case, "Expected": expected, "Got": got, "Verdict": "PASS" if got == expected else "FAIL"}, **(extra or {})))


def main():
    # ---------------------------------------------------------- common actors
    A_PRIN = actor("adapter-a-session", "principal-1")
    S_PRIN = session("adapter-a-session", "principal-1")
    A_ARCH = actor("adapter-b-cli", "architect-1")
    S_ARCH = session("adapter-b-cli", "architect-1")

    # ---------------------------------------------------------- C-11: no binding with an unaccredited mandatory requirement
    principal_reqs = [r for r, _ in PRINCIPAL_REQ]
    ctx11 = {"action": "CUSTODY", "requirements": principal_reqs, "contract_cells": ["adapter-a-session:model-a1:Long-horizon"], "contract_independence": []}
    p_ok = put_preflight(preflight("P20261002T230000Z-0001", "PRINCIPAL_COORDINATOR", "CUSTODY", "PRINCIPAL_COORDINATION", "adapter-a-session", A_PRIN, S_PRIN,
                                   PRINCIPAL_REQ))
    base = binding("B20261002T230000Z-0001", "PRINCIPAL_COORDINATOR", p_ok, "model-a1", "Long-horizon", [], [],
                   individual("ACCEPTED", "I62-PRINCIPAL-BINDING: B20261002T230000Z-0001 ACCEPTED"))
    DECISIONS_TEXT["text"] = "I62-PRINCIPAL-BINDING: B20261002T230000Z-0001 ACCEPTED\n"
    cases11 = [("c11-positivo", base, "ACCEPTED")]
    p_unknown = put_preflight(preflight("P20261002T230000Z-0002", "PRINCIPAL_COORDINATOR", "CUSTODY", "PRINCIPAL_COORDINATION", "adapter-a-session", A_PRIN,
                                        S_PRIN, PRINCIPAL_REQ, statuses={"PRINCIPAL_COORDINATION.introspection": "UNKNOWN"}))
    b = copy.deepcopy(base); b["PreflightRef"] = preflight_ref(p_unknown); cases11.append(("c11-obligatorio-UNKNOWN", b, "REJECTED/P-10"))
    b = copy.deepcopy(base); b["Eligibility"]["MeasuredInvocation"] = {"State": "NOT_MEASURED", "RunRef": None}
    cases11.append(("c11-MeasuredInvocation-NOT_MEASURED", b, "REJECTED/P-10"))
    b = copy.deepcopy(base); b["Eligibility"]["ConsumptionCovered"] = "UNKNOWN"; cases11.append(("c11-cobertura-UNKNOWN", b, "REJECTED/P-10"))
    p_omit = put_preflight(preflight("P20261002T230000Z-0003", "PRINCIPAL_COORDINATOR", "CUSTODY", "PRINCIPAL_COORDINATION", "adapter-a-session", A_PRIN,
                                     S_PRIN, PRINCIPAL_REQ, omit=("PRINCIPAL_COORDINATION.repo-write",)))
    b = copy.deepcopy(base); b["PreflightRef"] = preflight_ref(p_omit); cases11.append(("c11-requisito-omitido-E11", b, "REJECTED/P-10"))
    b = copy.deepcopy(base); b["Cell"]["AdapterId"] = "adapter-a-cli"; b["Cell"]["CellId"] = "adapter-a-cli:model-a1:Long-horizon"
    cases11.append(("c11-celda-adapter-incompatible", b, "REJECTED"))
    b = copy.deepcopy(base); b["Cell"]["EffortSemantic"] = "Deep"; b["Cell"]["CellId"] = "adapter-a-session:model-a1:Deep"
    cases11.append(("c11-effort-incompatible", b, "REJECTED"))
    b = copy.deepcopy(base); b["Cell"]["Model"] = "model-a2"; b["Cell"]["CellId"] = "adapter-a-session:model-a2:Long-horizon"
    cases11.append(("c11-modelo-fuera-de-las-celdas", b, "REJECTED"))
    for name, rec, _ in cases11:
        expect_schema(name, rec, "binding.v1.schema.json")
    expect_schema("c11-preflight-positivo", p_ok, "preflight.v1.schema.json")

    # ---------------------------------------------------------- C-12: references and acceptance identities
    contract = {"Unit": UNIT, "TaskId": "T-01"}
    w1 = binding("B20261002T230000Z-0011", "WORKER", p_ok, "model-a1", "Long-horizon", [], [], individual("ACCEPTED", "x"), scope="TASK", task="T-01")
    w1["Seq"] = 1
    w2 = copy.deepcopy(w1); w2["BindingId"] = "B20261002T230000Z-0012"; w2["Seq"] = 2
    other_unit = copy.deepcopy(w2); other_unit["UnitId"] = "I-99-MC"; other_unit["BindingId"] = "B20261002T230000Z-0013"; other_unit["Seq"] = 3
    other_task = copy.deepcopy(w2); other_task["TaskId"] = "T-02"; other_task["BindingId"] = "B20261002T230000Z-0014"; other_task["Seq"] = 4
    all_b = [w1, w2, other_unit, other_task]
    clean = lambda x: {k: v for k, v in x.items() if k != "Seq"}
    ref_ok = binding_ref(clean(w2))
    ref_rev = copy.deepcopy(ref_ok); ref_rev["Sha256"] = h64("other revision")
    ref_obsolete = binding_ref(clean(w1))
    ref_unit = binding_ref(clean(other_unit))
    ref_task = binding_ref(clean(other_task))
    ref_transient = binding_ref(clean(w2), kind="TRANSIENT")
    ref_not_ancestor = binding_ref(clean(w2), commit_label="c9")
    c12 = [("c12-pass-custodiado-vigente", ref_ok, "pass"), ("c12-otra-unidad", ref_unit, "rechazo A2'"), ("c12-otra-tarea", ref_task, "rechazo A2'"),
           ("c12-otra-revision", ref_rev, "rechazo A2'"), ("c12-obsoleto-por-rebinding", ref_obsolete, "rechazo A2'"),
           ("c12-TRANSIENT-presentado-como-custodiado", ref_transient, "rechazo A2'"), ("c12-commit-no-ancestro", ref_not_ancestor, "rechazo A2'")]
    for name, ref, exp in c12:
        got, why = a2_reference(ref, contract, all_b)
        record("C-12", name, exp, got, {"Reasons": why})
    # acceptance identities: candidate ≠ accepted; fabricated DecisionRef; stale and incompatible observations
    cand = copy.deepcopy(base); cand["Acceptance"] = pending()
    got, rules = binding_disposition(cand, ctx11)
    record("C-12", "c12-candidato-PENDING-no-es-aceptado", "PENDING", got, {"Rules": rules})
    fake = copy.deepcopy(base); fake["Acceptance"] = individual("ACCEPTED", "I62-PRINCIPAL-BINDING: B20261002T230000Z-0001 ACCEPTED (fabricado)")
    got, rules = binding_disposition(fake, ctx11)
    record("C-12", "c12-DecisionRef-fabricado", "REJECTED/P-20", got, {"Rules": rules})
    mixed = copy.deepcopy(base); mixed["Acceptance"]["AuthorizationRef"] = {"Path": DECISIONS_PATH, "Marker": "m", "AuthorizationId": "A1", "Commit": h40("c1"), "Blob": h40("x")}
    got, rules = binding_disposition(mixed, ctx11)
    record("C-12", "c12-individual-con-AuthorizationRef", "REJECTED/P-20", got, {"Rules": rules})
    p_stale = copy.deepcopy(p_ok); p_stale["PreflightId"] = "P20261002T230000Z-0004"; p_stale["Invalidators"]["AuthState"] = "NOT_AUTHENTICATED"
    put_preflight(p_stale)
    b = copy.deepcopy(base); b["PreflightRef"] = preflight_ref(p_stale)
    got, rules = binding_disposition(b, ctx11)
    record("C-12", "c12-observacion-obsoleta", "REJECTED/P-10", got, {"Rules": rules})
    p_other = put_preflight(preflight("P20261002T230000Z-0005", "PRINCIPAL_COORDINATOR", "RESUME_DECISION", "PRINCIPAL_COORDINATION", "adapter-a-session",
                                      A_PRIN, S_PRIN, PRINCIPAL_REQ))
    b = copy.deepcopy(base); b["PreflightRef"] = preflight_ref(p_other)
    got, rules = binding_disposition(b, ctx11)
    record("C-12", "c12-observacion-incompatible-otra-accion", "REJECTED/P-10", got, {"Rules": rules})
    prev = copy.deepcopy(base); prev["Acceptance"] = pending()
    ok_transition = copy.deepcopy(base)
    got, rules = binding_disposition(ok_transition, dict(ctx11, previous_version=prev))
    record("C-12", "c12-transicion-unica-PENDING-ACCEPTED", "ACCEPTED", got, {"Rules": rules})
    tampered = copy.deepcopy(base); tampered["RoutingReason"] = "cambiado"
    got, rules = binding_disposition(tampered, dict(ctx11, previous_version=prev))
    record("C-12", "c12-mismo-BindingId-otro-contenido", "S-04", got, {"Rules": rules})
    expect_schema("c12-candidato", cand, "binding.v1.schema.json")

    # ---------------------------------------------------------- C-11 evaluation (after the C-12 store exists) and its mutation
    batch = run_schema_batch()
    for name, rec, exp in cases11:
        ok, err = batch[name]
        got, rules = binding_disposition(rec, ctx11, schema_ok=ok)
        record("C-11", name, exp, got, {"SchemaValid": ok, "Rules": rules})
    mutated = binding_disposition(cases11[1][1], ctx11, unknown_as_satisfied=True)[0]
    RESULTS["C-11"]["Mutation"] = {"Change": "el procedimiento trata UNKNOWN como satisfecho", "Case": "c11-obligatorio-UNKNOWN",
                                   "GotUnderMutation": mutated, "Detected": mutated != "REJECTED/P-10"}
    m_ok = all(batch[n][0] for n in ("c11-preflight-positivo", "c12-candidato"))
    RESULTS["C-12"]["SchemaValid"] = m_ok
    rec_unmut = a2_reference(ref_obsolete, contract, all_b)[0]
    rec_mut = a2_reference(ref_obsolete, contract, [x for x in all_b if x["BindingId"] != w2["BindingId"]])[0]
    RESULTS["C-12"]["Mutation"] = {"Change": "se quita el rebinding posterior (el «obsoleto» deja de serlo)", "Case": "c12-obsoleto-por-rebinding",
                                   "GotUnderMutation": rec_mut, "Detected": rec_unmut != rec_mut and rec_mut == "pass"}

    # ---------------------------------------------------------- C-13: independence (1)-(11)
    worker = {"Label": "WORKER", "Actor": actor("adapter-a-cli", "worker-1"), "Session": session("adapter-a-session", "principal-1"), "Context": "CLEAN",
              "Adapter": "adapter-a-cli"}
    ctrl_sep = {"Actor": actor("adapter-a-cli", "controller-1"), "Session": session("adapter-a-cli", "controller-1"), "Context": "CLEAN", "Adapter": "adapter-a-cli"}
    t_all = {"Actor": "REQUIRED", "Session": "REQUIRED", "Context": "REQUIRED", "Provider": "NOT_REQUIRED"}
    t_shared = {"Actor": "REQUIRED", "Session": "REQUIRED", "Context": "REQUIRED", "Provider": "PREFERRED"}
    # (1) two simultaneous triggers: every delegation + shared-authority change → maximum per dimension
    combined = combine([t_all, t_shared])
    record("C-13", "c13-1-dos-requisitos-simultaneos", {"Actor": "REQUIRED", "Session": "REQUIRED", "Context": "REQUIRED", "Provider": "PREFERRED"}, combined)
    # (2) same provider, separate context
    r2 = {d: dimension(d, ctrl_sep, worker) for d in ("Context", "Provider")}
    record("C-13", "c13-2-mismo-proveedor-contexto-separado", {"Context": "SATISFIED", "Provider": "NOT_SATISFIED"}, r2)
    # (3) different provider, shared context
    other = {"Actor": actor("adapter-b-cli", "controller-2"), "Session": session("adapter-b-cli", "controller-2"), "Context": "SHARED", "Adapter": "adapter-b-cli"}
    r3 = {d: dimension(d, other, worker) for d in ("Context", "Provider")}
    record("C-13", "c13-3-distinto-proveedor-contexto-compartido", {"Context": "NOT_SATISFIED", "Provider": "SATISFIED"}, r3)
    # (4) same actor instance, two BindingIds (Worker and verifier)
    same = {"Actor": copy.deepcopy(worker["Actor"]), "Session": session("adapter-a-cli", "verifier-x"), "Context": "CLEAN", "Adapter": "adapter-a-cli"}
    record("C-13", "c13-4-misma-instancia-dos-BindingId", "NOT_ACCEPTED", evaluate(t_all, same, [worker])[0])
    # (5) PREFERRED vs REQUIRED unmet
    pref_only = {"Actor": actor("adapter-a-cli", "c3"), "Session": session("adapter-a-cli", "c3"), "Context": "CLEAN", "Adapter": "adapter-a-cli"}
    d5a = evaluate(t_shared, pref_only, [worker])
    d5b = evaluate(t_shared, dict(pref_only, Context="SHARED"), [worker])
    record("C-13", "c13-5-PREFERRED-frente-a-REQUIRED", {"PreferredUnmet": "ACCEPTABLE (registrado)", "RequiredUnmet": "NOT_ACCEPTED (bloqueo)"},
           {"PreferredUnmet": d5a[0] + (" (registrado)" if d5a[3] else ""), "RequiredUnmet": d5b[0] + (" (bloqueo)" if d5b[2] else "")})
    # (6)-(11): LIFECYCLE §5 predicate for I62 units (alternative 1)
    ai1 = {"Kind": "AI_SESSION", "Actor": actor("adapter-a-session", "holder-1"), "Session": session("adapter-a-session", "holder-1"), "HumanId": None,
           "Operator": "owner", "Commits": [h40("k1")], "Evidence": "titular 1", "Seq": 5}
    ai2 = {"Kind": "AI_SESSION", "Actor": actor("adapter-a-session", "holder-2"), "Session": session("adapter-a-session", "holder-2"), "HumanId": None,
           "Operator": "owner", "Commits": [h40("k2")], "Evidence": "titular 2", "Seq": 6}
    subject = {"Kind": "DESIGN", "Authors": [ai1, ai2]}
    human_ok = {"Mode": "EXTERNAL HUMAN", "Actor": None, "Session": None, "Human": "external-reviewer", "ReviewInstance": "RV-1", "Context": "CLEAN"}
    record("C-13", "c13-6-EXTERNAL-HUMAN-ajeno", "SATISFIED", lifecycle_predicate("ALT1", human_ok, subject)[0])
    rev_eq_first = {"Mode": "SEPARATE SESSION", "Actor": copy.deepcopy(ai1["Actor"]), "Session": copy.deepcopy(ai1["Session"]), "Context": "CLEAN"}
    record("C-13", "c13-7-revisor-igual-al-primer-titular", "NOT_SATISFIED", lifecycle_predicate("ALT1", rev_eq_first, subject)[0])
    unknown_author = {"Kind": "UNKNOWN", "Actor": None, "Session": None, "HumanId": None, "Operator": None, "Commits": [h40("k3")], "Evidence": "commit sin autor", "Seq": 7}
    sep_ok = {"Mode": "SEPARATE SESSION", "Actor": actor("adapter-b-cli", "rev-1"), "Session": session("adapter-b-cli", "rev-1"), "Context": "CLEAN"}
    record("C-13", "c13-8-autor-no-establecible", "UNKNOWN", lifecycle_predicate("ALT1", sep_ok, {"Kind": "DESIGN", "Authors": [ai1, unknown_author]})[0])
    # (9) §16 verification with SupersededCommits: the reference includes the Worker of the superseded commits
    superseded_worker = {"Label": "WORKER (SupersededCommits)", "Actor": actor("adapter-a-cli", "worker-old"), "Session": session("adapter-a-cli", "worker-old"),
                         "Context": "CLEAN", "Adapter": "adapter-a-cli"}
    ctrl_eq_old = {"Actor": copy.deepcopy(superseded_worker["Actor"]), "Session": copy.deepcopy(superseded_worker["Session"]), "Context": "CLEAN", "Adapter": "adapter-a-cli"}
    record("C-13", "c13-9-Controller-igual-Worker-de-SupersededCommits", "NOT_ACCEPTED", evaluate(t_all, ctrl_eq_old, [worker, superseded_worker])[0])
    # (10) a commit before the unit by the reviewer's identity is outside the bounded set
    pre_unit = {"Kind": "AI_SESSION", "Actor": copy.deepcopy(sep_ok["Actor"]), "Session": copy.deepcopy(sep_ok["Session"]), "HumanId": None, "Operator": "owner",
                "Commits": [h40("k0")], "Evidence": "commit anterior a la unidad", "Seq": 1}
    authors10 = bounded_authors([pre_unit, ai1, ai2], unit_start=5)
    record("C-13", "c13-10-commit-anterior-a-la-unidad", "SATISFIED", lifecycle_predicate("ALT1", sep_ok, {"Kind": "IMPLEMENTATION", "Authors": authors10})[0])
    # (11) the human who operated an authoring AI session reviews as EXTERNAL HUMAN
    operator_reviewer = dict(human_ok, Human="owner")
    record("C-13", "c13-11-operador-humano-como-EXTERNAL-HUMAN", "NOT_SATISFIED", lifecycle_predicate("ALT1", operator_reviewer, subject)[0])
    same_session_role = {"Mode": "SAME-SESSION ROLE", "Actor": None, "Session": None, "Context": "CLEAN"}
    record("C-13", "c13-SAME-SESSION-ROLE-no-basta-en-revision-mayor", "NOT_SATISFIED", lifecycle_predicate("ALT1", same_session_role, subject)[0])
    # mutations named by the dossier: (2) with shared context → Context no; (10) with the commit inside the unit → disqualifies
    m2 = dimension("Context", dict(ctrl_sep, Context="SHARED"), worker)
    m10 = lifecycle_predicate("ALT1", sep_ok, {"Kind": "IMPLEMENTATION", "Authors": bounded_authors([dict(pre_unit, Seq=5), ai1], unit_start=5)})[0]
    RESULTS["C-13"]["Mutation"] = {"Change": "(2) con el contexto compartido; (10) con el commit dentro de la unidad",
                                   "GotUnderMutation": {"2": m2, "10": m10}, "Detected": m2 == "NOT_SATISFIED" and m10 == "NOT_SATISFIED"}

    # ---------------------------------------------------------- C-14: closure cases G.2 rows 1-7 + Scope membership (I-64 debt)
    # 1: same delivery, two verifications (1st BLOCKED/STOP S-04 resolved without changing the work; 2nd VERIFIED)
    c = Counters(); c.invoke(); c.rerun("verificación"); c.invoke()
    v2 = classify(all_pass(), {})
    record("C-14", "c14-1-dos-verificaciones-misma-entrega", {"attempts": 0, "per_class": {}, "blocked": {"verificación": 1}, "invocations": 2, "final": "VERIFIED"},
           dict(c.snapshot(), final=v2["Classification"]))
    # 2: correction launched, the Worker falls, Handoff BLOCKED, rerun of the work
    c = Counters(); c.correction("Tests")
    v = classify(all_pass(Handoff="fail", **{k: "not_run_by_prior" for k in CHECK_ORDER[2:]}), {"Handoff": "absent"})
    c.rerun("WORK")
    record("C-14", "c14-2-correccion-sin-entrega", {"attempts": 1, "per_class": {"Tests": 1}, "blocked": {"WORK": 1}, "verification": "BLOCKED/BLOCKED"},
           {"attempts": c.attempts, "per_class": c.per_class, "blocked": c.blocked, "verification": v["Classification"] + "/" + v["Disposition"]})
    # 3: Controller context loss → S-12 STOP → analysis + decision → rerun without change
    c = Counters(); stop = "STOP (S-12)"; c.rerun("verificación")
    record("C-14", "c14-3-perdida-de-contexto-del-Controller", {"stop": "STOP (S-12)", "attempts": 0, "blocked": {"verificación": 1}},
           {"stop": stop, "attempts": c.attempts, "blocked": c.blocked})
    # 4: Routing effective ≠ requested
    rr, _ = routing_result("required", "model-x", "model-y", "RUNTIME_OBSERVED")
    ra, note = routing_result("advisory", "model-x", "model-y", "RUNTIME_OBSERVED")
    vr = classify(all_pass(Routing=rr), {}); va = classify(all_pass(Routing=ra), {})
    rn, _ = routing_result("required", None, "model-y", "REQUESTED")
    vn = classify(all_pass(Routing=rn), {})
    record("C-14", "c14-4-Routing-efectivo-distinto", {"required": "BLOCKED/BLOCKED", "advisory": "VERIFIED/NONE", "required-no-observado": "BLOCKED/BLOCKED"},
           {"required": vr["Classification"] + "/" + vr["Disposition"], "advisory": va["Classification"] + "/" + va["Disposition"],
            "required-no-observado": vn["Classification"] + "/" + vn["Disposition"]})
    # 5 and 6: handoff of another run
    v5 = classify(all_pass(Handoff="fail"), {"Handoff": "other-run"})
    record("C-14", "c14-5-handoff-otra-corrida-Identity-pass", {"FailureClass": "Handoff", "Classification": "REWORK_REQUIRED", "Disposition": "REWORK"}, v5)
    v6 = classify(all_pass(Handoff="fail", Identity="fail"), {"Handoff": "other-run"})
    record("C-14", "c14-6-handoff-otra-corrida-Identity-fail", {"FailureClass": "Handoff", "Classification": "BLOCKED", "Disposition": "STOP"}, v6)
    # 7: process with a failed turn
    v7 = classify(all_pass(Termination="fail", **{k: "not_run_by_prior" for k in CHECK_ORDER[1:]}), {})
    record("C-14", "c14-7-turno-fallido", {"FailureClass": "Termination", "Classification": "BLOCKED", "Disposition": "BLOCKED"}, v7)
    # S: Scope without membership, or with a failed auxiliary comparison (I-64 debt; AUTOMATION_PLAN 16.22)
    allowed = ["docs/a.md", "src/"]
    s_fail_cmp = scope_result({"ComparisonFailed": True, "DiffNameOnly": ["tests/x.cs"], "Membership": None}, allowed)
    s_no_member = scope_result({"DiffNameOnly": ["docs/a.md"], "Membership": None}, allowed)
    s_out = scope_result({"DiffNameOnly": ["tests/x.cs"], "Membership": {"tests/x.cs": None}}, ["docs/a.md"])
    s_ok = scope_result({"DiffNameOnly": ["src/k.cs", "docs/a.md"], "Membership": {"src/k.cs": "src/", "docs/a.md": "docs/a.md"}}, allowed)
    record("C-14", "c14-scope-sin-pertenencia", {"comparacion-fallida": "not_run", "sin-pertenencia": "not_run", "fuera-de-alcance": "fail", "con-pertenencia": "pass",
                                                 "verificacion-comparacion-fallida": "BLOCKED/STOP"},
           {"comparacion-fallida": s_fail_cmp, "sin-pertenencia": s_no_member, "fuera-de-alcance": s_out, "con-pertenencia": s_ok,
            "verificacion-comparacion-fallida": "/".join(list(classify(all_pass(Scope=s_fail_cmp), {}).values())[1:])})
    # 16.9: with several in fail, STOP > BLOCKED > REWORK (G.2 row 6 cites it); a case where a STOP and a REWORK coexist
    vp = classify(all_pass(Identity="fail", CleanTree="fail"), {})
    record("C-14", "c14-precedencia-STOP-sobre-REWORK", {"FailureClass": "Identity", "Classification": "BLOCKED", "Disposition": "STOP"}, vp)
    vpm = classify(all_pass(Identity="fail", CleanTree="fail"), {}, precedence=("REWORK", "BLOCKED", "STOP"))
    RESULTS["C-14"]["Mutation"] = {"Change": "se intercambia la precedencia STOP > REWORK", "Case": "c14-precedencia-STOP-sobre-REWORK",
                                   "GotUnderMutation": vpm, "Detected": vpm["Disposition"] != "STOP"}
    RESULTS["C-14"]["NotInF3"] = ("G.2 filas 8-9 (R-1, R-2) necesitan state/v2 → C-15 (F4); 10-11 (resolver, PENDING_G0) → C-20c (F4); 12-27 (bucle y fidelidad) → "
                                  "C-29..C-42 (F4)")
    RESULTS["C-14"]["Note"] = "la regla de Scope es un control de procedimiento de F3 para unidades I62; no repara la deuda nc2 de I-64, que es una unidad I61"

    # ---------------------------------------------------------- C-40 (F3 portion): authorized materialization of Architect bindings
    auth = {"Role": "ARCHITECT", "AuthorizedActions": ["REVIEW_DESIGN"],
            "MinimumCapabilities": [{"RequirementId": r, "Required": v} for r, v in ARCHITECT_REQ],
            "RequiredIndependence": [{"ReferenceRole": "AUTHOR", "Actor": "REQUIRED", "Session": "REQUIRED", "Context": "REQUIRED", "Provider": "PREFERRED"}],
            "EligibleCells": {"Mode": "LIST", "Cells": ["adapter-b-cli:model-b1:Deep"]},
            "ModelEffortBounds": {"MinLevel": "Equilibrado", "MinEffort": "Deep", "MaxLevel": None, "MaxEffort": "Long-horizon"},
            "Permissions": "READ_ONLY", "Budget": {"MaxReviewRounds": 4, "MaxLogicalRequests": 4, "MaxTransportRerunsPerRequest": 2, "MaxCorrectionsPerLineage": 3},
            "ObjectFamily": {"Unit": UNIT, "PathPattern": "docs/initiatives/I-62-MC-proposal-v*.md", "InitialVersion": "v1", "CorrectionScope": "synthetic"},
            "Validity": {"FromRecordVersion": 3, "Until": "2026-10-03T23:59:59Z"}}
    auth_text = "I62-REVIEW-LOOP-AUTHORIZATION: RLA-1\n" + json.dumps(auth, sort_keys=True)
    auth_commit, auth_blob = h40("c1"), h40("decisions@c1")
    AUTH_STORE = {(auth_commit, auth_blob): {"text": auth_text, "auth": auth}}
    auth_ref = {"Path": DECISIONS_PATH, "Marker": "I62-REVIEW-LOOP-AUTHORIZATION: RLA-1", "AuthorizationId": "RLA-1", "Commit": auth_commit, "Blob": auth_blob}
    now = "2026-10-03T10:00:00Z"
    good = {"Role": "ARCHITECT", "Action": "REVIEW_DESIGN", "Capabilities": {r: "MATCH" for r, _ in ARCHITECT_REQ},
            "Independence": {"Actor": "SATISFIED", "Session": "SATISFIED", "Context": "SATISFIED", "Provider": "SATISFIED"},
            "CellId": "adapter-b-cli:model-b1:Deep", "Effort": "Deep", "Level": "Frontera", "Permissions": "READ_ONLY",
            "ObjectUnit": UNIT, "ObjectPath": "docs/initiatives/I-62-MC-proposal-v2.md"}
    ok, crit = materialization_check(good, auth, now)
    p_arch = put_preflight(preflight("P20261003T100000Z-0001", "ARCHITECT", "REVIEW_DESIGN", "ARCHITECTURE_REVIEW", "adapter-b-cli", A_ARCH, S_ARCH, ARCHITECT_REQ))
    arch_req = [{"ReferenceRole": "AUTHOR", "ReferenceActor": copy.deepcopy(A_PRIN), "Actor": "REQUIRED", "Session": "REQUIRED", "Context": "REQUIRED", "Provider": "PREFERRED"}]
    arch_sat = [{"ReferenceRole": "AUTHOR", "Dimension": d, "Result": "SATISFIED", "Evidence": "synthetic"} for d in ("Actor", "Session", "Context", "Provider")]
    materialized = binding("B20261003T100000Z-0001", "ARCHITECT", p_arch, "model-b1", "Deep", arch_req, arch_sat,
                           {"State": "ACCEPTED", "Basis": "AUTHORIZED_MATERIALIZATION", "DecisionRef": None, "AuthorizationRef": auth_ref,
                            "MaterializationCheck": {"Criteria": crit}, "Utc": now})
    ctx40 = {"action": "REVIEW_DESIGN", "requirements": [r for r, _ in ARCHITECT_REQ], "contract_cells": None, "contract_independence": []}
    expect_schema("c40-a-materializado", materialized, "binding.v1.schema.json")
    got_b, rules_b = binding_disposition(materialized, ctx40)
    got_r, why_r = reproduce_materialized(materialized, AUTH_STORE, good)
    record("C-40", "c40-a-Architect-nuevo-materializado", {"criteria": True, "binding": "ACCEPTED", "reproduction": "ACCEPTED", "DecisionRef": None},
           {"criteria": ok, "binding": got_b, "reproduction": got_r, "DecisionRef": materialized["Acceptance"]["DecisionRef"]}, {"Rules": rules_b})
    seven = [("capacidad-UNKNOWN", dict(good, Capabilities=dict(good["Capabilities"], **{"ARCHITECTURE_REVIEW.read": "UNKNOWN"}))),
             ("effort-fuera-de-ModelEffortBounds", dict(good, Effort="Maximum")),
             ("misma-sesion-que-un-autor", dict(good, Independence=dict(good["Independence"], Session="NOT_SATISFIED"))),
             ("celda-fuera-de-EligibleCells", dict(good, CellId="adapter-a-cli:model-a1:Deep")),
             ("permisos-de-escritura", dict(good, Permissions="WRITE_IN_SCOPE")),
             ("objeto-fuera-de-ObjectFamily", dict(good, ObjectPath="docs/initiatives/I-63-proposal-v2.md"))]
    for name, cnd in seven:
        okc, _ = materialization_check(cnd, auth, now)
        record("C-40", "c40-b-" + name, "rechazo sin invocación", "materializable" if okc else "rechazo sin invocación")
    revoked = dict(auth, Ended="revocación custodiada (I62-REVIEW-LOOP-REVOCATION: RLA-1)")
    okc, _ = materialization_check(good, revoked, now)
    record("C-40", "c40-b-autorizacion-revocada", "rechazo sin invocación", "materializable" if okc else "rechazo sin invocación")
    okc, _ = materialization_check(good, auth, "2026-10-04T00:00:01Z")
    record("C-40", "c40-b-autorizacion-caducada", "rechazo sin invocación", "materializable" if okc else "rechazo sin invocación")

    def loop_decision(candidates):
        """§20.5.1 / README §14.3: no candidate satisfies → STOP; ESCALATION_OWNER only when what is missing is Owner matter (authentication)."""
        if any(materialization_check(x, auth, now)[0] for x in candidates):
            return "INVOKE"
        owner = all(x.get("AuthState") == "NOT_AUTHENTICATED" for x in candidates)
        return "STOP + ESCALATION_OWNER" if owner else "STOP + COORDINATOR_DECISION"

    record("C-40", "c40-c-ningun-candidato-satisface", {"criterios": "STOP + COORDINATOR_DECISION", "autenticacion": "STOP + ESCALATION_OWNER"},
           {"criterios": loop_decision([c2 for _, c2 in seven]),
            "autenticacion": loop_decision([dict(good, AuthState="NOT_AUTHENTICATED", Capabilities={r: "UNKNOWN" for r, _ in ARCHITECT_REQ})])})
    fabricated = copy.deepcopy(materialized); fabricated["Acceptance"]["DecisionRef"] = {"Path": DECISIONS_PATH, "Marker": "I62-PRINCIPAL-BINDING: B20261003T100000Z-0001 ACCEPTED"}
    no_auth = copy.deepcopy(materialized); no_auth["Acceptance"]["AuthorizationRef"] = None
    self_commit = copy.deepcopy(materialized); self_commit["Acceptance"]["AuthorizationRef"] = dict(auth_ref, Commit=CUSTODY_POINT)
    record("C-40", "c40-d-DecisionRef-fabricado", "REJECTED/P-20", binding_disposition(fabricated, ctx40)[0])
    record("C-40", "c40-d-sin-AuthorizationRef", "REJECTED/P-20", binding_disposition(no_auth, ctx40)[0])
    record("C-40", "c40-d-AuthorizationRef-en-el-propio-commit", "REJECTED/P-20", reproduce_materialized(self_commit, AUTH_STORE, good)[0])
    okc, crit_h = materialization_check(good, dict(auth, Validity={"FromRecordVersion": 3, "Until": "2026-10-02T00:00:00Z"}), now)
    record("C-40", "c40-h-reutilizar-autorizacion-caducada", "rechazo", "materializable" if okc else "rechazo",
           {"Validity": next(x["Result"] for x in crit_h if x["CriterionId"] == "VALIDITY")})
    mut = materialization_check(seven[0][1], auth, now, unknown_as_satisfied=True)[0]
    RESULTS["C-40"]["Mutation"] = {"Change": "un criterio UNKNOWN se cuenta como SATISFIED", "Case": "c40-b-capacidad-UNKNOWN",
                                   "GotUnderMutation": "materializable" if mut else "rechazo", "Detected": mut}
    RESULTS["C-40"]["NotInF3"] = "(e)-(g) e (i)-(n): vigencia de acción con intentos (LAUNCHING, LAUNCHED, CANCELLED_BEFORE_LAUNCH, LAUNCH_UNCERTAIN) → F4"

    # ---------------------------------------------------------- C-30: Architect invocation and output contracts
    accepted = {materialized["BindingId"]: materialized}
    ctrl_b = binding("B20261003T100000Z-0002", "EXECUTION_CONTROLLER", p_arch, "model-b1", "Deep", [], [], individual("ACCEPTED", "x"), scope="TASK", task="T-01")
    accepted[ctrl_b["BindingId"]] = ctrl_b
    target = {"commit": h40("proposal-v2"), "path": "docs/initiatives/I-62-MC-proposal-v2.md", "blob": h40("proposal-v2-blob")}
    closure_ref = {"path": "docs/automation/evidence/I-62-MC/closure.json", "blob": h40("closure")}
    fidelity_ref = {"path": "docs/automation/evidence/I-62-MC/fidelity.json", "blob": h40("fidelity")}

    def invocation(role, action, contract, bnd, target_obj=target, perms="READ_ONLY", canonical=None):
        return {"Schema": "rackcad-role-invocation/v1", "InvocationId": "I20261003T100000Z-0001", "LogicalReviewRequestId": "L20261003T100000Z-0001",
                "AttemptSeq": 1, "UnitId": UNIT, "Gate": "G1", "TaskId": None, "ProtocolSet": "rackcad-protocol/I62", "RequestedRole": role, "Action": action,
                "Target": target_obj, "AuthorityRevision": h40("authority"),
                "CanonicalInputs": canonical or [{"Path": target["path"], "Blob": target["blob"]}],
                "AllowedTransitiveInputs": [], "AllowedActions": [{"Action": "git log --oneline -10", "RequiredBy": "AGENTS.md#leer-primero", "Class": "ACTION_COMPATIBLE",
                                                                   "ExemptionRef": None, "ExemptionScope": None}],
                "HealthSignals": [], "DeclaredRuntimeContext": [{"Kind": "system", "Source": "adapter", "SizeOrSha256": "1024"}],
                "ForbiddenInputs": [".agent-runs/I-62-MC/author-transcript.jsonl", "author-memory"],
                "EffectiveInputClosure": closure_ref, "InputFidelityPreflight": fidelity_ref, "OpenFindings": [],
                "RequiredCapabilities": [{"RequirementId": r, "Required": v} for r, v in ARCHITECT_REQ],
                "IndependenceRequirements": {"Requirements": [{"ReferenceRole": "AUTHOR", "Actor": "REQUIRED", "Session": "REQUIRED", "Context": "REQUIRED",
                                                               "Provider": "PREFERRED"}], "ReviewSubject": None},
                "Permissions": perms, "Binding": binding_ref(bnd), "OutputContract": contract,
                "BudgetSnapshot": {"ReviewRounds": 1, "LogicalRequests": 1, "ArchitectLaunches": 1, "TransportReruns": 0, "CorrectionRounds": 0, "CorrectionsByLineage": []},
                "StopConditions": ["P-10", "P-19", "P-22"], "EscalationConditions": [], "Authorization": {"path": DECISIONS_PATH, "blob": auth_blob}}

    inv_ok = invocation("ARCHITECT", "REVIEW_DESIGN", "rackcad-architect-review-result/v1", materialized)
    invs = {"c30-base-architect": inv_ok}
    # (a) a binding in the same session as the author: the candidate fails the REQUIRED Session dimension → no accepted binding
    same_session = dict(good, Independence=dict(good["Independence"], Session="NOT_SATISFIED"))
    a_ok, _ = materialization_check(same_session, auth, now)
    # (b) inputs carrying the author's transcript
    inv_b = invocation("ARCHITECT", "REVIEW_DESIGN", "rackcad-architect-review-result/v1", materialized,
                       canonical=[{"Path": target["path"], "Blob": target["blob"]}, {"Path": ".agent-runs/I-62-MC/author-transcript.jsonl", "Blob": h40("tr")}])
    invs["c30-b"] = inv_b
    # (e) crossed output contracts
    inv_e1 = invocation("ARCHITECT", "REVIEW_DESIGN", "rackcad-reviewer-result/v1", materialized)
    reviewer_b = copy.deepcopy(materialized); reviewer_b["Role"] = "REVIEWER"; reviewer_b["BindingId"] = "B20261003T100000Z-0003"
    accepted_r = dict(accepted, **{reviewer_b["BindingId"]: reviewer_b})
    inv_e2 = invocation("REVIEWER", "REVIEW_CHANGE", "rackcad-architect-review-result/v1", reviewer_b)
    invs["c30-e1"], invs["c30-e2"] = inv_e1, inv_e2
    # (f) no EffectiveInputClosure
    inv_f = copy.deepcopy(inv_ok); del inv_f["EffectiveInputClosure"]; invs["c30-f"] = inv_f
    # (g), (h), (i)
    inv_g = invocation("EXECUTION_CONTROLLER", "PLAN", "rackcad-controller-verification/v2", ctrl_b, target_obj=None)
    inv_h = invocation("EXECUTION_CONTROLLER", "VERIFY", "rackcad-delegation/v2", ctrl_b)
    inv_i1 = invocation("EXECUTION_CONTROLLER", "PLAN", "rackcad-delegation/v2", ctrl_b, target_obj=None)
    inv_i2 = invocation("EXECUTION_CONTROLLER", "VERIFY", "rackcad-controller-verification/v2", ctrl_b)
    invs.update({"c30-g": inv_g, "c30-h": inv_h, "c30-i1": inv_i1, "c30-i2": inv_i2})
    # (c) result without the injected-context declaration
    observed_identity = {"SessionOrThread": "architect-1", "Model": "model-b1"}

    def arch_result(inv, verdict="CHANGES REQUIRED", icd_state="DECLARED", required=None):
        req = required if required is not None else [{"FindingId": "F-1", "LineageId": None, "Source": "§1", "AffectedSection": "§1", "Evidence": "e",
                                                      "PremiseRefs": [{"Path": target["path"], "Section": "§1", "LineStart": 1, "LineEnd": 2, "Quote": "q"}],
                                                      "WhyItMatters": "w", "CorrectionRequired": "c"}]
        return {"Schema": "rackcad-architect-review-result/v1", "ResultId": "X-1", "LogicalReviewRequestId": inv["LogicalReviewRequestId"],
                "InvocationId": inv["InvocationId"], "AttemptSeq": 1, "RequestedRole": "ARCHITECT", "Action": "REVIEW_DESIGN", "ReviewedUnit": UNIT,
                "ReviewedCommit": target["commit"], "ReviewedPath": target["path"], "ReviewedBlob": target["blob"], "ReviewerBinding": inv["Binding"],
                "ReviewerDeclaredIdentity": {"Runtime": "UNKNOWN", "Model": "UNKNOWN", "Effort": "UNKNOWN", "SessionOrThread": "UNKNOWN"},
                "ReviewerMode": "SEPARATE SESSION",
                "InjectedContextDeclaration": {"State": icd_state, "Items": [{"Kind": "system", "Source": "adapter", "SizeOrSha256": "1024"}] if icd_state == "DECLARED" else []},
                "InputsRead": [target["path"]], "InputFidelityEvidenceRef": fidelity_ref,
                "IndependenceEvidence": {"Dimensions": [{"ReferenceRole": "AUTHOR", "Dimension": d, "Result": "SATISFIED", "Evidence": "e"} for d in ("Actor", "Session", "Context")],
                                         "ReviewSubject": None},
                "KnownLimitations": [], "RecommendedNextAction": "corregir", "Verdict": verdict, "RequiredFindings": req, "OptionalFindings": [],
                "FindingDispositions": [], "Downgrades": [], "OwnerDecisions": []}

    res_ok = arch_result(inv_ok)
    res_c = arch_result(inv_ok, icd_state="NONE_DECLARED")
    reviewer_res = {k: v for k, v in res_ok.items() if k not in ("Verdict", "RequiredFindings", "OptionalFindings", "Downgrades", "OwnerDecisions")}
    reviewer_res.update({"Schema": "rackcad-reviewer-result/v1", "RequestedRole": "REVIEWER", "Action": "REVIEW_CHANGE", "Disposition": "NO_FINDINGS", "Findings": [],
                         "Recommendations": []})
    reviewer_with_verdict = dict(reviewer_res, Verdict="AGREED")
    for name, inv in invs.items():
        expect_schema(name, inv, "role-invocation.v1.schema.json")
    expect_schema("c30-res-ok", res_ok, "architect-review-result.v1.schema.json")
    expect_schema("c30-res-c", res_c, "architect-review-result.v1.schema.json")
    expect_schema("c30-reviewer-res", reviewer_res, "reviewer-result.v1.schema.json")
    expect_schema("c30-reviewer-with-verdict", reviewer_with_verdict, "reviewer-result.v1.schema.json")
    PENDING[:] = [x for x in PENDING if x[0].startswith("c30") or x[0].startswith("c40")]
    batch30 = run_schema_batch()
    record("C-30", "c30-a-binding-misma-sesion-que-el-autor", "REJECTED/P-10", "REJECTED/P-10" if not a_ok else "ACCEPTED")
    record("C-30", "c30-b-insumos-con-la-transcripcion-del-autor", "REJECTED", invocation_disposition(inv_b, accepted, batch30["c30-b"][0])[0])
    record("C-30", "c30-c-sin-declaracion-de-contexto-inyectado", "INVALID/P-19",
           result_disposition(res_c, inv_ok, "architect-review-result.v1.schema.json", observed_identity)[0] if batch30["c30-res-c"][0] else "schema")
    d_alt1 = lifecycle_predicate("ALT1", sep_ok, subject)[0]
    d_alt2 = lifecycle_predicate("ALT2", dict(sep_ok, Context="UNKNOWN"), subject)[0]
    record("C-30", "c30-d-binding-valido-alternativas-1-y-2", {"ALT1 (vigente)": "SATISFIED", "ALT2 (contrafactual)": "SATISFIED", "invocacion": "ACCEPTED"},
           {"ALT1 (vigente)": d_alt1, "ALT2 (contrafactual)": d_alt2, "invocacion": invocation_disposition(inv_ok, accepted, batch30["c30-base-architect"][0])[0]})
    record("C-30", "c30-e-OutputContract-cruzado", {"ARCHITECT+reviewer-result": "INVALID/P-19", "REVIEWER+architect-review-result": "INVALID/P-19"},
           {"ARCHITECT+reviewer-result": invocation_disposition(inv_e1, accepted, batch30["c30-e1"][0])[0],
            "REVIEWER+architect-review-result": invocation_disposition(inv_e2, accepted_r, batch30["c30-e2"][0])[0]})
    record("C-30", "c30-f-sin-EffectiveInputClosure", "REJECTED", invocation_disposition(inv_f, accepted, batch30["c30-f"][0])[0])
    record("C-30", "c30-g-PLAN-con-controller-verification", "INVALID/P-19", invocation_disposition(inv_g, accepted, batch30["c30-g"][0])[0])
    record("C-30", "c30-h-VERIFY-con-delegation", "INVALID/P-19", invocation_disposition(inv_h, accepted, batch30["c30-h"][0])[0])
    record("C-30", "c30-i-PLAN-delegation-VERIFY-controller-verification", {"PLAN": "ACCEPTED", "VERIFY": "ACCEPTED"},
           {"PLAN": invocation_disposition(inv_i1, accepted, batch30["c30-i1"][0])[0], "VERIFY": invocation_disposition(inv_i2, accepted, batch30["c30-i2"][0])[0]})
    inv_rev = invocation("REVIEWER", "REVIEW_CHANGE", "rackcad-reviewer-result/v1", reviewer_b)
    record("C-30", "c30-REVIEWER-no-satisface-al-ARCHITECT", "REJECTED/P-20",
           result_disposition(reviewer_res, inv_rev, "reviewer-result.v1.schema.json", observed_identity, purpose="ARCHITECT_SATISFIED")[0])
    record("C-30", "c30-reviewer-result-con-Verdict", "schema rejects", "schema rejects" if not batch30["c30-reviewer-with-verdict"][0] else "schema accepts")
    record("C-30", "c30-resultado-architect-valido", "INGESTED",
           result_disposition(res_ok, inv_ok, "architect-review-result.v1.schema.json", observed_identity)[0] if batch30["c30-res-ok"][0] else "schema")
    swapped = dict(OUTPUT_CONTRACT); swapped[("EXECUTION_CONTROLLER", "PLAN")], swapped[("EXECUTION_CONTROLLER", "VERIFY")] = \
        OUTPUT_CONTRACT[("EXECUTION_CONTROLLER", "VERIFY")], OUTPUT_CONTRACT[("EXECUTION_CONTROLLER", "PLAN")]
    m_i = invocation_disposition(inv_i1, accepted, True, table=swapped)[0]
    RESULTS["C-30"]["Mutation"] = {"Change": "se intercambia la fila PLAN/VERIFY de la correspondencia de 16.23", "Case": "c30-i (PLAN)",
                                   "GotUnderMutation": m_i, "Detected": m_i != "ACCEPTED"}
    RESULTS["C-30"]["NotInF3"] = "la ingestión dentro del bucle (estados, intentos, presupuestos) y C-41/C-42 → F4"
    RESULTS["C-40"]["SchemaValid"] = batch30["c40-a-materializado"][0]
    # the negatives rejected by a rule must be schema-valid, so that the rule (not the schema) is what rejects them; only (f) and the
    # reviewer result with a Verdict are meant to fail the schema
    rule_cases = ["c30-base-architect", "c30-b", "c30-e1", "c30-e2", "c30-g", "c30-h", "c30-i1", "c30-i2", "c30-res-ok", "c30-res-c", "c30-reviewer-res"]
    RESULTS["C-30"]["SchemaValid"] = all(batch30[n][0] for n in rule_cases) and not batch30["c30-f"][0] and not batch30["c30-reviewer-with-verdict"][0]

    # ---------------------------------------------------------- verdicts
    summary = {}
    for control, body in RESULTS.items():
        cases_ok = all(c["Verdict"] == "PASS" for c in body["Cases"])
        mutation_ok = body.get("Mutation", {}).get("Detected", False)
        schema_ok = body.get("SchemaValid", True)
        body["Anchors"] = anchors_present(control)
        body["Verdict"] = "PASS" if cases_ok and mutation_ok and schema_ok and all(body["Anchors"].values()) else "FAIL"
        summary[control] = body["Verdict"]
    out = {"Harness": "f3-mc.py", "PowerShell": PWSH_VERSION, "Unit": UNIT, "Results": dict(sorted(RESULTS.items())), "Summary": dict(sorted(summary.items()))}
    with open(os.path.join(EV, "f3-mc-result.json"), "w", encoding="utf-8", newline="\n") as f:
        json.dump(out, f, ensure_ascii=False, indent=1)
        f.write("\n")
    print(json.dumps(out["Summary"]))
    return 0 if all(v == "PASS" for v in summary.values()) else 1


if __name__ == "__main__":
    sys.exit(main())
