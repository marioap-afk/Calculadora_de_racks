# Run with Python 3 from any directory: python eng/research/I52Ctda/oracle/make_oracle.py
# Rewrites eng/research/I52Ctda/v35-oracle.json. After any change the oracle blob pin in validate-v35-catalog.ps1 must be
# updated deliberately; that pin is the approval record the Architect reviews.
# Architecture-review oracle for V35-A1 (RC-13). Independent of the V35 generator: it never imports catalog35/derive35/gen35
# and never reads NPM-V35. Inputs: NPM-V34 (committed blob), the Architect decisions B01-B12/C02 and RC-01..RC-14 written here
# as literal tables. Only the approval hashes of RETAINED-EXTENDED texts are read from the catalog, once, at authoring time.
import json, re, subprocess, hashlib, collections, sys, os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from oracle_predicates import predicate_sets, trigger_action, canon
REPO = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..", "..", ".."))
BASE = "0c609269b47adedd0f29d8b4cbbc010fae91d2e0"
def blob(p): return subprocess.check_output(["git", "-C", REPO, "show", f"{BASE}:{p}"]).decode("utf-8")

npm = blob("docs/initiatives/I-52-native-probe-matrix-v34.md")
t = npm[npm.index("## 10. Full normative matrix"):]; t = t[t.index("```text") + 7:]; t = t[:t.index("```")]
COLS = "ProbeId;PrimaryAuthorityId;ScheduleOriginEventId;HeaderAuthority;Setup;Trigger;PrimaryTransactionId;TopTransactionId;DocumentLock;Thread/Context;Mutation;S/M/SM;SchedulePoint;ExecutionPoint;ExpectedBefore;ExpectedAfter;Verifier;LifecycleMarkers;Threats;DA-P/H-P;PASS;FAIL;UNKNOWN;Cleanup".split(";")
V = collections.OrderedDict()
for l in t.replace("\r", "").split("\n"):
    if l.strip():
        f = l.split(";"); V[f[0]] = dict(zip(COLS, f))
assert len(V) == 100

B07 = {"04N", "04NO-S", "04NO-UNDO-S"}
def is_ev(x): return x.startswith("N-")
def fam(e):
    for p in ("N-DB-", "N-TR-", "N-ED-", "N-DOC-", "N-OBJ-", "N-ENT-", "N-RX-"):
        if e.startswith(p): return p
def observation(r): return "observation" in r["Mutation"] or r["ProbeId"] in B07
def body_event(r):
    p, o = r["PrimaryAuthorityId"], r["ScheduleOriginEventId"]
    return p if is_ev(p) else (o if is_ev(o) else None)
def body_runs(r):
    return body_event(r) is not None and (is_ev(r["ScheduleOriginEventId"]) or not observation(r))

# ---------------------------------------------------------------- A: body observer and notifier
NOTIFIER = {"OR-XR": "F-XR", "OR-TXR": "F-TRIGGER-XR", "OR-REF-A": "F-REF-A", "OR-REF-B": "F-REF-B", "OR-TEO": "F-TRIGGER-ERASE-OBJ", "ER-REF-A": "F-REF-A"}
def body_observer(r):
    e = body_event(r); f = fam(e); s, tr = r["Setup"], r["Trigger"]
    if f == "N-DB-": return "RR-PAYLOAD-DB" if "H-LOAD" in r["HeaderAuthority"] else "RR-DB"   # B03: C15 origin is the payload reactor
    if f == "N-TR-": return "RR-TX"
    if f == "N-ED-": return "RR-ED"
    if f == "N-DOC-": return "RR-DOC"
    if f == "N-RX-": return "RR-DLINK"
    if f == "N-ENT-": return "ER-REF-A"
    if "semantic notifier A" in s: return "OR-TXR"                                       # B02
    for k in ("OR-REF-A", "OR-REF-B"):
        if "attach " + k in s: return k
    if "F-TRIGGER-ERASE-OBJ" in tr: return "OR-TEO"
    if "F-REF-A" in tr: return "OR-REF-A"
    if "F-XR" in tr: return "OR-XR"
    raise KeyError(r["ProbeId"])
A = collections.OrderedDict()
for pid, r in V.items():
    if body_runs(r):
        b = body_observer(r); A[pid] = {"BodyObserverId": b, "notifier": NOTIFIER.get(b)}
    else:
        A[pid] = {"BodyObserverId": "NONE", "notifier": None}

# ---------------------------------------------------------------- E: trigger target class (from the V34 Trigger text)
def target_class(pid, r):
    tr = r["Trigger"]
    if pid == "02NO-S": return "F-TRIGGER-XR"                       # B02
    for k in ("F-TRIGGER-APPEND", "F-TRIGGER-ERASE-DB", "F-TRIGGER-ERASE-OBJ", "F-TRIGGER-MOD"):
        if k in tr: return k
    if "erase F-REF-B" in tr: return "F-REF-B"
    if "F-REF-A" in tr: return "F-REF-A"
    if "F-XR" in tr: return "F-XR"
    if "modify DB notifier A" in tr or "N-DB-MOD calls" in tr or "modify fixture notifier" in tr: return "F-TRIGGER-MOD"   # B02 / origin N-DB-MOD
    if "NX-APPCTX-SYNC callback" in tr or "executeInApplicationContext" in tr: return "SYNC-APPCTX"
    if "CANCEL-CMDCTX-01" in tr or "HFV34_CANCEL" in tr: return "CMD:HFV34_CANCEL"
    if "lock" in tr: return "DOC-LOCK"
    if "NL-ARX-PATH" in tr or "load" in tr: return "R-PAYLOAD-ARX"
    if "fixture command" in tr or "command-ended" in tr: return "CMD:I52CTDA_FIXTURE"
    if "nested" in tr: return "T-NESTED"
    if "start controlled outer transaction" in tr: return "T-CONTROLLED"
    if "abort" in tr or "cancel notifier open" in tr or "undo/cancel" in tr or "end" in tr.lower(): return "T-PRIMARY"
    raise KeyError(pid)
E = collections.OrderedDict((pid, target_class(pid, r)) for pid, r in V.items())

# ---------------------------------------------------------------- B: resource types (literal)
B = {"F-TRIGGER-MOD": "PersistentFixtureIdentity", "F-TRIGGER-ERASE-DB": "PersistentFixtureIdentity", "F-TRIGGER-ERASE-OBJ": "PersistentFixtureIdentity",
     "F-TRIGGER-XR": "PersistentFixtureIdentity", "F-XR": "PersistentFixtureIdentity", "F-REF-B": "PersistentFixtureIdentity",
     "F-REF-A": "PersistentFixtureIdentity", "F-REF-C": "PersistentFixtureIdentity",
     "R-SM-LINK": "StateCarrier", "F-TRIGGER-APPEND": "DisposableTriggerResource", "R-PAYLOAD-ARX": "PayloadResource",
     "R-NATIVE-ARX": "RuntimeModule", "R-MANAGED-OBSERVER": "RuntimeModule",
     "OR-XR": "ObserverRegistration", "OR-TXR": "ObserverRegistration", "OR-REF-A": "ObserverRegistration", "OR-REF-B": "ObserverRegistration",
     "OR-TEO": "ObserverRegistration", "ER-REF-A": "ObserverRegistration", "RR-DB": "ObserverRegistration", "RR-TX": "ObserverRegistration",
     "RR-ED": "ObserverRegistration", "RR-DOC": "ObserverRegistration", "RR-DLINK": "ObserverRegistration", "RR-PAYLOAD-DB": "ObserverRegistration",
     "RR-MANAGED-CMD": "ObserverRegistration",
     "CMD-BOOT": "CommandResource", "CMD-PROBE": "CommandResource", "CMD-FIXTURE": "CommandResource", "CMD-CANCEL": "CommandResource",
     "CMD-QUEUED": "CommandResource", "CMD-FINISH": "CommandResource"}

# ---------------------------------------------------------------- C: FailPredicate applicability
def fp_allowed(pid, r):
    a = {"FP-STATE", "FP-ORDER", "FP-CLEANUP-SAFETY"}
    if "MUT-" in r["Mutation"] and pid not in B07: a.add("FP-AFFILIATION")
    if r["S/M/SM"] == "S/M": a |= {"FP-COUNT", "FP-VISIBILITY"}
    if pid == "09N-O": a.add("FP-INVARIANT-09N-O")
    if "candidate-boundary" in r["PASS"]: a.add("FP-BOUNDARY")
    if r["PrimaryAuthorityId"].startswith("NS-"): a.add("FP-CAUSAL")
    if "CTX-APPLICATION" in r["Thread/Context"] or "CTX-COMMAND" in r["Thread/Context"]: a.add("FP-CONTEXT")
    if r["PrimaryAuthorityId"] == "NX-APPCTX-SYNC" or r["ScheduleOriginEventId"].startswith("NOT_APPLICABLE(origin NX"): a.add("FP-SYNC")
    if "notifier" in r["FAIL"]: a.add("FP-NOTIFIER-WRITE")
    if "N-DOC-LOCK-VETO" in (r["PrimaryAuthorityId"], r["ScheduleOriginEventId"]): a.add("FP-VETO-BYPASS")
    return sorted(a)
C = collections.OrderedDict((pid, fp_allowed(pid, r)) for pid, r in V.items())

# ---------------------------------------------------------------- F: async boundary / marker stage binding
MARKER_STAGES = {"MARK-ORIGIN-RETURN": ["STG-ORIGIN"], "MARK-APPCTX-ENTRY": ["STG-SYNC-APPCTX"], "MARK-APPCTX-RETURN": ["STG-SYNC-APPCTX"],
                 "MARK-APPCTX-DELIVERY": ["STG-APPCTX-DELIVERY"], "MARK-APPCTX-CALLBACK-RETURN": ["STG-APPCTX-DELIVERY"],
                 "MARK-LOAD-RESULT": ["STG-PAYLOAD-LOAD"], "MARK-REGISTRATION": ["STG-PAYLOAD-INIT"], "MARK-MANAGED-CMD-END": ["STG-FIXTURE-CMD"],
                 "MARK-LOCK-RELEASE": ["STG-SEND-DELIVERY", "STG-APPCTX-DELIVERY", "STG-SYNC-APPCTX", "STG-CMDCTX-DELIVERY", "STG-FIXTURE-CMD"],
                 "N-ED-WILL": ["STG-SEND-DELIVERY", "STG-FIXTURE-CMD", "STG-CANCEL-CMD", "STG-CMDCTX-DELIVERY", "STG-PROBE-CMD"],
                 "N-ED-END": ["STG-SEND-DELIVERY", "STG-FIXTURE-CMD", "STG-CANCEL-CMD", "STG-CMDCTX-DELIVERY", "STG-PROBE-CMD"]}
STAGE_NAMESPACE = {"STG-PROBE-CMD": "GOVERNED", "STG-TRIGGER": "GOVERNED", "STG-PRIMARY-CALLBACK": "GOVERNED", "STG-ORIGIN": "GOVERNED",
                   "STG-SEND-DELIVERY": "GOVERNED", "STG-APPCTX-DELIVERY": "GOVERNED", "STG-CMDCTX-DELIVERY": "GOVERNED", "STG-SYNC-APPCTX": "GOVERNED",
                   "STG-FIXTURE-CMD": "GOVERNED", "STG-CANCEL-CMD": "GOVERNED", "STG-PAYLOAD-LOAD": "GOVERNED", "STG-PAYLOAD-INIT": "GOVERNED",
                   "STG-EXEC": "GOVERNED", "STG-DRIVER-APP": "DRIVER", "STG-FIN-GATE": "INFRA", "STG-FINISH": "INFRA"}
FIX_EVENTS = ("N-ED-END", "N-ED-WILL")
def cmd_stage(r):
    p, o = r["PrimaryAuthorityId"], r["ScheduleOriginEventId"]
    if p == "NS-SEND": return "STG-SEND-DELIVERY"
    if p == "NS-BEGIN-CMDCTX": return "STG-CMDCTX-DELIVERY"
    if p in FIX_EVENTS or o in FIX_EVENTS: return "STG-FIXTURE-CMD"
    if "N-ED-CANCEL" in (p, o): return "STG-CANCEL-CMD"
    return "STG-PROBE-CMD"
def lock_stage(r):
    p = r["PrimaryAuthorityId"]
    return {"NS-SEND": "STG-SEND-DELIVERY", "NS-BEGIN-APPCTX": "STG-APPCTX-DELIVERY", "NS-BEGIN-CMDCTX": "STG-CMDCTX-DELIVERY",
            "NX-APPCTX-SYNC": "STG-SYNC-APPCTX", "N-ED-END": "STG-FIXTURE-CMD"}[p]
F = collections.OrderedDict()
for pid, r in V.items():
    out = []
    for m in r["LifecycleMarkers"].split(","):
        if pid == "C2APP2CMD-ALL" and m == "MARK-APPCTX-RETURN": m = "MARK-APPCTX-CALLBACK-RETURN"      # B09
        if m in ("N-ED-WILL", "N-ED-END"): out.append(f"{m}@{cmd_stage(r)}")
        elif m == "MARK-LOCK-RELEASE": out.append(f"{m}@{lock_stage(r)}")
        elif m in MARKER_STAGES: out.append(f"{m}@{MARKER_STAGES[m][0]}")
    F[pid] = sorted(out)

# ---------------------------------------------------------------- G: completion tokens
def tokens(pid, r):
    p, o, h = r["PrimaryAuthorityId"], r["ScheduleOriginEventId"], r["HeaderAuthority"]
    t = {"TOK-PROBE-RETURN"}
    if p.startswith("N-DOC-LOCK") or o.startswith("N-DOC-LOCK") or (p == "NS-BEGIN-CMDCTX" and o.startswith("NOT_APPLICABLE(origin NX")):
        t.add("TOK-DRIVER-APP-DONE")                                                     # B11 and V35 decision 3
    if p in FIX_EVENTS or o in FIX_EVENTS: t.add("TOK-FIXTURE-CMD-END")
    if "N-ED-CANCEL" in (p, o): t |= {"TOK-CANCEL-OBSERVED", "TOK-CANCEL-CMDCTX-DONE"}
    if "H-LOAD" in h: t.add("TOK-PAYLOAD-LOAD-RESULT")
    if p == "NS-SEND": t |= {"TOK-ENQUEUE-SEND", "TOK-SEND-CMD-END"}
    if p == "NS-BEGIN-APPCTX" or (p == "NS-BEGIN-CMDCTX" and is_ev(o)): t |= {"TOK-ENQUEUE-APPCTX", "TOK-APPCTX-DELIVERY-DONE"}
    if p == "NS-BEGIN-CMDCTX": t |= {"TOK-ENQUEUE-CMDCTX", "TOK-CMDCTX-DELIVERY-DONE"}
    if p == "NX-APPCTX-SYNC" or (p == "NS-BEGIN-CMDCTX" and o.startswith("NOT_APPLICABLE(origin NX")): t.add("TOK-SYNC-RETURN")
    if not observation(r): t.add("TOK-EXEC-DONE")
    if r["PrimaryTransactionId"] in ("T-PRIMARY", "T-ABORTING", "ended T id", "origin primary T") or fam(p) == "N-TR-" or (is_ev(o) and fam(o) == "N-TR-"):
        t.add("TOK-OUTCOME")
    if "MARK-MANAGED-CMD-END" in r["LifecycleMarkers"]: t.add("TOK-MANAGED-CMD-END")
    if "MARK-LOCK-RELEASE" in r["LifecycleMarkers"]: t.add("TOK-LOCK-RELEASE")
    return sorted(t)
G = collections.OrderedDict((pid, tokens(pid, r)) for pid, r in V.items())

# ---------------------------------------------------------------- H: process fence
H = collections.OrderedDict()
for pid, r in V.items():
    if r["Cleanup"].endswith("-PROCESS"):
        H[pid] = {"fence": "FENCE-PROCESS-EXIT", "reason": f"V34 {r['Cleanup']}: state released only by the dedicated process exit"}
    elif pid == "02NO-SM":
        H[pid] = {"fence": "FENCE-PROCESS-EXIT", "reason": "OR-REF-B retained on the trigger-erased F-REF-B (CLEAN-OBJ-FENCED, §164)"}
    else:
        H[pid] = {"fence": "FENCE-CLEANUP-TOKEN", "reason": "all governed runtime state released and verified in-process by CLN-BASE"}

# ---------------------------------------------------------------- D: retained V34 authority
v34ids = collections.OrderedDict(); sec = None
for l in npm.replace("\r", "").split("\n"):
    m = re.match(r"## (\d+)\.", l)
    if m: sec = int(m.group(1))
    m = re.match(r"\| ([A-Z][A-Z0-9_-]+(?:-[A-Z0-9]+)*) \| (.+) \|$", l)
    if m and sec in (2, 3, 4, 5, 7, 8) and m.group(1) != "ContractId": v34ids[m.group(1)] = sec
EXTENDED = {"MUT-S": "V35 write-set attribute", "MUT-M": "V35 write-set attribute", "MUT-SM": "§164 write set", "MUT-ALL": "§164 disjoint write sets",
            "VER-SM": "§164 verifier (never opens F-REF-B)", "LOCK-OBS": "V35 lock authority entry", "T-PRIMARY": "driver-owned outermost T",
            "T-ABORTING": "driver-owned T-PRIMARY under abort", "APPCTX-LOCK-01": "RC-06: APPCTX entry incl. 13A-SM synchronous entry",
            "APPCTX-EXEC-01": "APPCTX entry incl. 13A-SM", "CMDCTX-EXEC-01": "V35 execution context entry", "CMDCTX-LOCK-01": "V35 lock authority entry",
            "EDC-EXEC-01": "V35 execution context entry", "RG-DB-MOD": "RC-11 key schema", "RG-DB-ERASE": "RC-11 key schema", "RG-OBJ-ERASE": "RC-11 key schema",
            "CLEAN-BASE": "V35 steps (B04, C02, RC-04)", "CLEAN-OBJ": "B04", "CLEAN-DEFER": "V35 steps", "CLEAN-DOC": "V35 steps", "CLEAN-OBJ-DEFER": "C02",
            "CLEAN-DOC-DEFER": "C02", "CLEAN-APPCTX-PROCESS": "V35 steps", "CLEAN-CMDCTX-PROCESS": "V35 steps", "CLEAN-CHAIN-PROCESS": "V35 steps",
            "CLEAN-C15-PROCESS": "V35 steps", "CLEAN-APPCTX-LOCK-PROCESS": "V35 steps", "CLEAN-CANCEL-PROCESS": "V35 steps"}
SUPERSEDED = {"T-FRESH": "T_CB"}
UNUSED = {}
cat = json.load(open(os.path.join(REPO, "docs", "initiatives", "I-52-execution-catalog-v35.json"), encoding="utf-8"))["entries"]   # approval hashes only
D = collections.OrderedDict()
for k, s in v34ids.items():
    if k in EXTENDED:
        D[k] = {"status": "RETAINED-EXTENDED", "extension": EXTENDED[k], "approvedSha256": hashlib.sha256(canon(cat[k]).encode("ascii")).hexdigest().upper()}
    elif k in SUPERSEDED:
        D[k] = {"status": "SUPERSEDED", "supersededBy": SUPERSEDED[k]}
    elif k in UNUSED:
        D[k] = {"status": "UNUSED"}
    else:
        D[k] = {"status": "RETAINED-UNCHANGED", "v34Section": s}

P = collections.OrderedDict()
for pid, r in V.items():
    o_, f_, u_ = predicate_sets(pid, r, E[pid], observation, body_runs, is_ev)
    P[pid] = {"TriggerActionId": trigger_action(pid, r, E[pid]), "ObservationPredicateIds": o_, "FailPredicateIds": f_, "UnknownPredicateIds": u_}
PINS = {k: subprocess.check_output(["git", "-C", REPO, "rev-parse", f"{BASE}:{p}"]).decode().strip() for k, p in {
    "docs/initiatives/I-52-native-probe-matrix-v34.md": "docs/initiatives/I-52-native-probe-matrix-v34.md",
    "docs/initiatives/I-52-native-event-catalog-v34.md": "docs/initiatives/I-52-native-event-catalog-v34.md",
    "eng/research/I52Ctda/traceability-v34.json": "eng/research/I52Ctda/traceability-v34.json",
    "docs/automation/evidence/I-52-r3-governed-executor-discovery.json": "docs/automation/evidence/I-52-r3-governed-executor-discovery.json"}.items()}
# Z: approval freeze of the reviewed V35-A1 package (verification round V3/V4) plus the V35-A2 errata (Architect MINOR M1). These values are a snapshot of the exact
# texts submitted to the Architect, not an independent derivation; the independent tables are A-H and P above.
def git_blob_of(path):
    return subprocess.check_output(["git", "-C", REPO, "hash-object", path]).decode().strip()
catalog_full = json.load(open(os.path.join(REPO, "docs", "initiatives", "I-52-execution-catalog-v35.json"), encoding="utf-8"))
npm35 = open(os.path.join(REPO, "docs", "initiatives", "I-52-native-probe-matrix-v35.md"), encoding="utf-8").read()
t35 = npm35[npm35.index("## 5. Full normative matrix"):]; t35 = t35[t35.index("```text") + 7:]; t35 = t35[:t35.index("```")]
cols35 = catalog_full["rowSchema"]
rows35 = {l.split(";")[0]: collections.OrderedDict(zip(cols35, l.split(";"))) for l in t35.replace(chr(13), "").strip().split(chr(10))}
FREEZE = collections.OrderedDict([
    ("note", "Approval freeze of the reviewed package: canonical SHA-256 of every catalog entry and every NPM-V35 row, and the git blob ids of the lineage, traceability and fixture files."),
    ("catalogEntries", collections.OrderedDict((k, hashlib.sha256(canon(v).encode("ascii")).hexdigest().upper()) for k, v in catalog_full["entries"].items())),
    ("rows", collections.OrderedDict((k, hashlib.sha256(canon(v).encode("ascii")).hexdigest().upper()) for k, v in rows35.items())),
    ("blobs", collections.OrderedDict((p, git_blob_of(os.path.join(REPO, *p.split("/")))) for p in (
        "docs/automation/evidence/I-52-v35-clause-lineage.json", "eng/research/I52Ctda/traceability-v35.json", "eng/research/I52Ctda/fixture-v35.json"))),
])
state_bytes = sorted(set(re.findall(r"HFV30:[A-Z]+:[A-Z0-9,]+", npm)))
oracle = collections.OrderedDict([
    ("schemaVersion", 1), ("initiative", "I-52"), ("revision", "V35-A2"),
    ("purpose", "Architecture-review oracle data (Architect RC-13). The validator compares the V35 package against these frozen tables and never regenerates them from NPM-V35 or the catalog."),
    ("provenance", "Derived from NPM-V34 at " + BASE + " and the Architect decisions B01-B12, C02 and RC-01..RC-14 written as literal rules; only approvedSha256 values were read from the V35-A1 catalog when the oracle was frozen; V35-A2 adds the Architect MINOR M1 fence-read ABI rule."),
    ("A_bodyObserver", A), ("B_resourceTypes", B), ("C_failPredicateApplicability", C), ("D_retainedAuthority", D),
    ("E_triggerTargetClass", E), ("F_markerStageBindings", F), ("F_markerStages", MARKER_STAGES), ("F_stageNamespace", STAGE_NAMESPACE),
    ("G_completionTokens", G), ("H_processFence", H), ("P_rowPredicatesAndTrigger", P), ("V34_inputBlobPins", PINS),
    ("guardKeySchema", {"RG-DB-MOD": ["ProbeId", "GuardId", "DatabaseId", "EventId", "CallbackMember", "TargetObjectId", "StageId"],
                        "RG-DB-ERASE": ["ProbeId", "GuardId", "DatabaseId", "EventId", "CallbackMember", "TargetObjectId", "ErasingFlag", "StageId"],
                        "RG-OBJ-ERASE": ["ProbeId", "GuardId", "ReactorInstance", "EventId", "CallbackMember", "TargetObjectId", "StageId"],
                        "RG-DB-APPEND": ["ProbeId", "GuardId", "DatabaseId", "EventId", "CallbackMember", "TargetInstancePointer", "StageId"],
                        "RG-DB-OPEN": ["ProbeId", "GuardId", "DatabaseId", "EventId", "CallbackMember", "TargetObjectId", "StageId"],
                        "RG-TX": ["ProbeId", "GuardId", "TransactionManagerId", "EventId", "CallbackMember", "TransactionIdentity", "StageId"]},
     ),
    ("guardKeyOneShot", {"N-OBJ-": ["ReactorInstance", "TargetObjectId"], "N-ENT-": ["ReactorInstance", "TargetObjectId"], "N-ED-": ["DocumentId", "CommandName"],
                         "N-DOC-": ["DocumentId", "RequestId"], "N-RX-": ["ModulePath"], "N-DB-": ["ReactorInstance", "DatabaseId", "TargetObjectId"],
                         "SA-VETO": ["DocumentId", "RequestId"]}),
    ("driverPhaseOrder", {"DRIVER-CMD-01": ["REGISTER", "SETUP", "ARM", "TRIGGER", "CALLBACK-WINDOW", "POST-TRIGGER-OBLIGATIONS", "OUTCOME-RECORD", "DISARM", "TOKEN"],
                          "DRIVER-APP-01": ["ENQUEUE", "DELIVERY", "ARM", "TRIGGER", "CALLBACK-WINDOW", "POST-TRIGGER-OBLIGATIONS", "OUTCOME-RECORD", "DISARM", "TOKEN"]}),
    ("setupOrder", ["SET-OPEN-PRIMARY-T", "SET-OPEN-NESTED-T", "SET-STAGE-S-IN-PRIMARY", "SET-PAYLOAD-PATH", "SET-LOAD-PAYLOAD",
                    "SET-RETAIN-CALLBACK-DATA", "SET-RETAIN-TWO-STAGE-DATA", "SET-ARM-VETO", "SET-OUTCOME-COMMIT"]),
    ("allowedCleanupExtension", {"02NO-SM": "CLEAN-OBJ-FENCED"}),
    ("payloadReactorRows", ["C15N16N-SM"]),
    ("Z_approvalFreeze", FREEZE),
    ("cancelStaging", {"TRG-CANCEL-OPEN-XR": {"stagedBytes": "HFV35:XR:CANCEL-STAGED", "forbiddenValues": state_bytes, "requiredObservation": "OBS-CANCEL-RESTORED"}}),
    ("appendTransaction", {"02NAPP-SM": "T-PRIMARY"}),
    ("managedHost", {"RR-MANAGED-CMD": "R-MANAGED-OBSERVER"}),
    ("payloadRows", [pid for pid, r in V.items() if "H-LOAD" in r["HeaderAuthority"]]),
    ("resultAuthority", {"authority": "CONTROL-PLANE-RESULT-01", "evaluationOrder": ["SAFETY-FAIL", "UNKNOWN", "FAIL", "PASS", "UNKNOWN-RESIDUAL"]}),
    ("finishGate", {"gate": "FIN-GATE-01", "scriptContainsFinish": False, "timeoutResult": "UNK-FINISH-TIMEOUT",
                    "finishFirstAction": "FINISH-FENCE-01", "lateDeliveryResult": "UNK-LATE-DELIVERY", "cleanupClosesDocument": False}),
    ("lockReleaseAnchor", {"STG-SEND-DELIVERY": "COMMAND-END-WINDOW", "STG-FIXTURE-CMD": "COMMAND-END-WINDOW", "STG-CMDCTX-DELIVERY": "COMMAND-END-WINDOW",
                           "STG-APPCTX-DELIVERY": "APPCTX-UNLOCK-01-CALL", "STG-SYNC-APPCTX": "APPCTX-UNLOCK-01-CALL"}),
    ("payloadModule", {"exports": ["I52CtdaPayload_RemoveReactor"], "registersOnlyWhenArmed": True}),
    # Architect delta review of V35-A1, MINOR M1: the fence is read through one LOG-SEQ-01 export; its setter is never exported.
    ("finishFenceAbi", {"fence": "FINISH-FENCE-01", "owner": "R-NATIVE-ARX", "readApi": "I52Ctda_FinishFenceIsSet", "setter": "INTERNAL-R-NATIVE-ARX",
                        "setterExported": False, "readers": ["R-PAYLOAD-ARX", "R-MANAGED-OBSERVER"]}),
    ("syncStageIncludesCallerReturn", True),
    ("cleanupSafetyScope", "GOVERNED-NAMESPACE-ONLY"),
])
out = os.path.join(REPO, "eng", "research", "I52Ctda", "v35-oracle.json")
with open(out, "w", encoding="utf-8", newline="\n") as fh:
    json.dump(oracle, fh, indent=2, ensure_ascii=False); fh.write("\n")
print("oracle written", {k: len(v) for k, v in oracle.items() if isinstance(v, dict)})
