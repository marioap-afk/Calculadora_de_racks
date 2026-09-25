# Oracle second implementation of the per-row predicate sets and the exact trigger action (independence review F1/F2).
# Literal V34-clause tables written for the oracle; nothing is imported from the V35 generator.

PASS_V34 = {
    "PASS-S and affiliation matches commit/abort": ["OBS-T-AFFILIATION", "OBS-OUTCOME-COMMIT"],
    "PASS-S requires exact callback order and no staged state survives": ["OBS-ORDER-RULES"],
    "callback,count,visibility and cleanup exact": ["OBS-PRIMARY-CALLBACK", "OBS-COUNT", "OBS-VISIBILITY"],
    "required callback identity,count,state,OBSERVED-ORDER-COMPLETE,EXPECTED-ORDER-INVARIANT-09N-O and cleanup complete": ["OBS-PRIMARY-CALLBACK", "OBS-COUNT", "OBS-ORDER-COMPLETE", "OBS-ORDER-INVARIANT-09N-O"],
    "callback,count,reread and cleanup exact": ["OBS-PRIMARY-CALLBACK", "OBS-COUNT", "OBS-REREAD"],
    "PASS-S requires N-ED-END,T-FRESH,MUT-S,successful commit/end,callback return after T completion,STATE-S-1,lifecycle order and cleanup": ["OBS-PRIMARY-CALLBACK", "OBS-EXEC-COMPLETE", "OBS-ORDER-RULES"],
    "with candidate-boundary classification": ["OBS-CANDIDATE-BOUNDARY"],
    "with enqueue and delivery exact": ["OBS-ENQUEUE-OK", "OBS-DELIVERY"],
    "PASS-SM requires load,registration,event,enqueue,execution,state,order and CLEAN-C15-PROCESS": ["OBS-LOAD", "OBS-REGISTRATION", "OBS-ORIGIN-CALLBACK", "OBS-ENQUEUE-OK", "OBS-EXEC-COMPLETE", "OBS-CAUSAL-ORDER"],
    "PASS-S with exact notifier/T affiliation": ["OBS-NOTIFIER-WRITE", "OBS-T-AFFILIATION"],
    "PASS-M with exact callback order": ["OBS-ORDER-RULES"],
    "PASS-SM with exact erase flag/order": ["OBS-ERASE-FLAG", "OBS-ORDER-RULES"],
    "PASS-SM plus observational eWasNotifying check": ["OBS-EWASNOTIFYING"],
    "with exact cancel order and restored state": ["OBS-ORDER-RULES"],
    "with exact undo order and restored state": ["OBS-ORDER-RULES"],
    "PASS-M with exact graphics callback/order": ["OBS-PRIMARY-CALLBACK", "OBS-ORDER-RULES"],
    "PASS-SM requires callback,modes,legal T,commit,state,order and cleanup": ["OBS-PRIMARY-CALLBACK", "OBS-LOCK-MODES", "OBS-EXEC-COMPLETE", "OBS-ORDER-RULES"],
    "PASS-SM requires modes,legal T,commit,state,order and cleanup": ["OBS-LOCK-MODES", "OBS-EXEC-COMPLETE", "OBS-ORDER-RULES"],
    "PASS-SM requires WILL,veto eOk,VETOED,state unchanged and cleanup, CHANGED is logged only if observed and is not inferred": ["OBS-VETO-OK"],
    "with enqueue,delivery,state and order exact": ["OBS-ENQUEUE-OK", "OBS-DELIVERY", "OBS-CAUSAL-ORDER"],
    "with enqueue,delivery,state,lock/order and cleanup exact": ["OBS-ENQUEUE-OK", "OBS-DELIVERY", "OBS-LOCK-MODES", "OBS-CAUSAL-ORDER"],
    "PASS-ALL requires synchronous entry,mutation,return,state and cleanup": ["OBS-SYNC-ENTRY-RETURN", "OBS-EXEC-COMPLETE"],
    "requires enqueue,post-return delivery,legal context,state and cleanup": ["OBS-ENQUEUE-OK", "OBS-DELIVERY", "OBS-CONTEXT", "OBS-EXEC-COMPLETE"],
    "requires legal application origin,enqueue,command delivery,state and cleanup": ["OBS-CONTEXT", "OBS-ENQUEUE-OK", "OBS-DELIVERY", "OBS-EXEC-COMPLETE"],
    "requires callback,T affiliation,state and cleanup": ["OBS-PRIMARY-CALLBACK", "OBS-T-AFFILIATION"],
    "PASS-ALL requires callback,legal mutation,state and cleanup": ["OBS-PRIMARY-CALLBACK", "OBS-EXEC-COMPLETE"],
    "PASS-ALL requires origin,enqueue,delivery,state,causal order and cleanup": ["OBS-ORIGIN-CALLBACK", "OBS-ENQUEUE-OK", "OBS-DELIVERY", "OBS-CAUSAL-ORDER"],
    "PASS-ALL requires origin,enqueue,post-return delivery,state,causal order and cleanup": ["OBS-ORIGIN-CALLBACK", "OBS-ENQUEUE-OK", "OBS-DELIVERY", "OBS-CAUSAL-ORDER"],
    "PASS-ALL requires both enqueue legs,three contexts,state,causal order and cleanup": ["OBS-ORIGIN-CALLBACK", "OBS-ENQUEUE-OK", "OBS-DELIVERY", "OBS-CONTEXT", "OBS-CAUSAL-ORDER"],
    "PASS-ALL requires exact origin,enqueue,delivery,state,causal order and cleanup": ["OBS-ORIGIN-CALLBACK", "OBS-ENQUEUE-OK", "OBS-DELIVERY", "OBS-CAUSAL-ORDER"],
    "PASS-ALL requires cancellation callback,legal mutation,state and cleanup": ["OBS-PRIMARY-CALLBACK", "OBS-CANCELLATION", "OBS-EXEC-COMPLETE"],
    "PASS-ALL requires cancel origin,enqueue,delivery,state,causal order and cleanup": ["OBS-ORIGIN-CALLBACK", "OBS-CANCELLATION", "OBS-ENQUEUE-OK", "OBS-DELIVERY", "OBS-CAUSAL-ORDER"],
    "requires exact primary callback,legal mutation,expected state,guard reset and cleanup": ["OBS-PRIMARY-CALLBACK", "OBS-EXEC-COMPLETE", "OBS-GUARD-RESET"],
}
FAIL_V34 = [  # (substring of the V34 FAIL cell, FailPredicateIds); first match wins
    ("or escaped affiliation", ["FP-STATE", "FP-AFFILIATION"]),
    ("staged state survives or order", ["FP-STATE", "FP-ORDER"]),
    ("count/order/visibility", ["FP-COUNT", "FP-ORDER", "FP-VISIBILITY"]),
    ("observable count/order contradiction", ["FP-COUNT", "FP-ORDER"]),
    ("authority-declared invariant", ["FP-INVARIANT-09N-O", "FP-STATE"]),
    ("state/count/order", ["FP-STATE", "FP-COUNT", "FP-ORDER"]),
    ("committed T reports success", ["FP-AFFILIATION", "FP-ORDER"]),
    ("accepted-boundary", ["FP-BOUNDARY"]),
    ("causal-boundary", ["FP-CAUSAL", "FP-STATE"]),
    ("lifecycle/safety or DB", ["FP-ORDER", "FP-CLEANUP-SAFETY", "FP-STATE"]),
    ("affiliation or notifier-write", ["FP-AFFILIATION", "FP-NOTIFIER-WRITE"]),
    ("notifier or T contradiction", ["FP-ORDER", "FP-AFFILIATION"]),
    ("sibling/linkage", ["FP-STATE"]),
    ("forbidden state survives", ["FP-STATE"]),
    ("observable lifecycle/state", ["FP-ORDER", "FP-STATE"]),
    ("vetoed authority", ["FP-VETO-BYPASS", "FP-ORDER", "FP-STATE"]),
    ("synchronous/context/state", ["FP-SYNC", "FP-CONTEXT", "FP-STATE"]),
    ("causal/context/state", ["FP-CAUSAL", "FP-CONTEXT", "FP-STATE"]),
    ("chain/context/state", ["FP-CAUSAL", "FP-CONTEXT", "FP-STATE"]),
    ("origin/delivery/state", ["FP-CAUSAL", "FP-STATE"]),
    ("exact callback/state/affiliation", ["FP-ORDER", "FP-STATE", "FP-AFFILIATION"]),
    ("callback affiliation or state", ["FP-AFFILIATION", "FP-STATE"]),
    ("affiliation/state", ["FP-AFFILIATION", "FP-STATE"]),
    ("state/order contradiction", ["FP-STATE", "FP-ORDER"]),
]
UNK_V34 = [  # (substring of the V34 UNKNOWN cell, UnknownPredicateIds); first match wins; plain UNKNOWN-COMMON adds nothing
    ("missing callback or incomplete order", ["UNK-MISSING-CALLBACK", "UNK-ORDER-INCOMPLETE"]),
    ("illegal T start", ["UNK-T-START", "UNK-ILLEGAL-MUTATION", "UNK-T-END"]),
    ("missing load,registration", ["UNK-LOAD", "UNK-REGISTRATION", "UNK-NONDELIVERY", "UNK-CLEANUP-PROOF"]),
    ("legal mutation context cannot be obtained", ["UNK-ILLEGAL-CONTEXT", "UNK-NO-WRITE-LOCK"]),
    ("illegal enqueue or nondelivery", ["UNK-ILLEGAL-ENQUEUE", "UNK-NONDELIVERY"]),
    ("application-context mutation cannot", ["UNK-ILLEGAL-CONTEXT", "UNK-LOCK-DOC-T-LEAK"]),
    ("illegal DB context", ["UNK-REJECTION", "UNK-NONDELIVERY", "UNK-ILLEGAL-DB-CONTEXT", "UNK-LOCK-DOC-T-LEAK"]),
    ("invalid application context", ["UNK-INVALID-APP-CONTEXT", "UNK-REJECTION", "UNK-NONDELIVERY", "UNK-NO-WRITE-LOCK", "UNK-T-CMD-FAILURE"]),
    ("not contained by one-shot guard", ["UNK-RECURSION"]),
    ("cancellation or legal mutation", ["UNK-CANCELLATION-MISSING", "UNK-ILLEGAL-CONTEXT", "UNK-NO-WRITE-LOCK", "UNK-T-CANCEL-FAILURE"]),
    ("legal mutation context is unavailable", ["UNK-ILLEGAL-CONTEXT", "UNK-NO-WRITE-LOCK"]),
    ("either rejection", ["UNK-REJECTION", "UNK-NONDELIVERY", "UNK-UNDRAINED", "UNK-LOCK-DOC-T-LEAK", "UNK-NO-WRITE-LOCK", "UNK-T-CMD-FAILURE"]),
    ("illegal context or undrained work, lock", ["UNK-REJECTION", "UNK-NONDELIVERY", "UNK-ILLEGAL-CONTEXT", "UNK-UNDRAINED", "UNK-LOCK-DOC-T-LEAK"]),
    ("illegal context or undrained work", ["UNK-REJECTION", "UNK-NONDELIVERY", "UNK-ILLEGAL-CONTEXT", "UNK-UNDRAINED"]),
    ("rejection,nondelivery or undrained work", ["UNK-REJECTION", "UNK-NONDELIVERY", "UNK-UNDRAINED"]),
    ("illegal mutation or unexpected recursion", ["UNK-ILLEGAL-MUTATION", "UNK-RECURSION"]),
    ("including nondelivery", ["UNK-NONDELIVERY"]),
]

def pass_base(cell):
    if cell in PASS_V34: return PASS_V34[cell]
    hits = [v for k, v in PASS_V34.items() if cell.endswith(k)]
    assert len(hits) == 1, ("PASS", cell)
    return hits[0]
def first(table, cell, what):
    for k, v in table:
        if k in cell: return v
    raise KeyError((what, cell))

def trigger_action(pid, r, target_class):
    tr = r["Trigger"].lower(); c = target_class
    fixed = {"F-TRIGGER-MOD": "TRG-MODIFY-TRIGGER-MOD", "F-TRIGGER-ERASE-DB": "TRG-ERASE-TRIGGER-DB", "F-TRIGGER-ERASE-OBJ": "TRG-ERASE-TRIGGER-OBJ",
             "F-TRIGGER-XR": "TRG-ASSERT-WRITE-TRIGGER-XR", "F-REF-B": "TRG-ERASE-REF-B", "T-NESTED": "TRG-END-NESTED-T", "T-CONTROLLED": "TRG-START-OUTER-T",
             "CMD:I52CTDA_FIXTURE": "TRG-RUN-FIXTURE-CMD", "CMD:HFV34_CANCEL": "TRG-CANCEL-CMDCTX", "R-PAYLOAD-ARX": "TRG-LOAD-PAYLOAD"}
    if c in fixed: return fixed[c]
    if c == "F-TRIGGER-APPEND": return "TRG-APPEND-TRIGGER-IN-PRIMARY" if pid == "02NAPP-SM" else "TRG-APPEND-TRIGGER"   # RC-10
    if c == "F-REF-A": return "TRG-TRANSFORM-CLOSE-REF-A" if "transform" in tr else "TRG-MODIFY-CLOSE-REF-A"
    if c == "F-XR": return "TRG-CANCEL-OPEN-XR" if ("cancel" in tr or "undo" in tr) else "TRG-OPEN-FOR-MODIFY-XR"
    if c == "T-PRIMARY": return "TRG-ABORT-PRIMARY-T" if ("abort" in tr or "cancel" in tr or "undo" in tr) else "TRG-END-PRIMARY-T"
    if c == "DOC-LOCK": return "TRG-LOCK-VETO" if "veto" in tr else "TRG-LOCK-CYCLE"
    if c == "SYNC-APPCTX": return "TRG-APPCTX-SYNC-BEGIN-CMDCTX" if r["PrimaryAuthorityId"] == "NS-BEGIN-CMDCTX" else "TRG-APPCTX-SYNC"
    raise KeyError(pid)

def predicate_sets(pid, r, target_class, observation, body_runs, is_ev):
    p, o, h = r["PrimaryAuthorityId"], r["ScheduleOriginEventId"], r["HeaderAuthority"]
    obs_row = observation(r)
    sched = p.startswith("NS-"); sync = p == "NX-APPCTX-SYNC" or (p == "NS-BEGIN-CMDCTX" and o.startswith("NOT_APPLICABLE(origin NX"))
    veto = "N-DOC-LOCK-VETO" in (p, o)
    markers = [] if r["LifecycleMarkers"].startswith("RG-") else r["LifecycleMarkers"].split(",")
    obs = set(pass_base(r["PASS"]))
    if markers: obs.add("OBS-MARKERS")
    if not obs_row: obs |= {"OBS-EXEC-COMPLETE", "OBS-T-AFFILIATION"}
    if body_runs(r) or "SA-VETO" in r["Setup"]: obs.add("OBS-GUARD-RESET")
    opens_commit = (r["PrimaryTransactionId"] in ("T-PRIMARY", "origin primary T") or target_class == "T-NESTED") and target_class != "T-PRIMARY"
    if opens_commit: obs.add("OBS-OUTCOME-COMMIT")
    if is_ev(p): obs.add("OBS-PRIMARY-CALLBACK")
    if is_ev(o): obs.add("OBS-ORIGIN-CALLBACK")
    if sched: obs |= {"OBS-ENQUEUE-OK", "OBS-DELIVERY", "OBS-CAUSAL-ORDER"}
    if sync: obs.add("OBS-SYNC-ENTRY-RETURN")
    if "CTX-APPLICATION" in r["Thread/Context"] or "CTX-COMMAND" in r["Thread/Context"]: obs.add("OBS-CONTEXT")
    if "N-ED-CANCEL" in (p, o): obs.add("OBS-CANCELLATION")
    if "candidate-boundary" in r["PASS"]: obs.add("OBS-CANDIDATE-BOUNDARY")
    if veto: obs.add("OBS-VETO-OK")
    if "H-LOAD" in h: obs |= {"OBS-LOAD", "OBS-PAYLOAD-DB-BINDING"}
    if pid in ("04N", "04NO-S", "04NO-UNDO-S"): obs.add("OBS-STAGED")                               # B07 staging
    if "MARK-LOCK-RELEASE" in markers: obs.add("OBS-LOCK-RELEASE-BOUND")                           # RC-02
    if o in ("N-OBJ-CANCEL", "N-OBJ-UNDO"): obs.add("OBS-CANCEL-RESTORED")                         # RC-09
    fail = set(first(FAIL_V34, r["FAIL"], "FAIL")) | {"FP-CLEANUP-SAFETY"}                          # RC-04
    if "candidate-boundary" in r["PASS"]: fail.add("FP-BOUNDARY")
    if veto: fail.add("FP-VETO-BYPASS")
    unk = {"UNKNOWN-COMMON", "UNK-FINISH-TIMEOUT", "UNK-LOG-BINDING", "UNK-LATE-DELIVERY"}             # RC-01, RC-07, lifecycle review
    if r["UNKNOWN"] != "UNKNOWN-COMMON": unk |= set(first(UNK_V34, r["UNKNOWN"], "UNKNOWN"))
    if pid in ("04N", "04NO-S", "04NO-UNDO-S"): unk.add("UNK-STAGING")
    if p in ("N-TR-ABORTED", "N-TR-ABOUT-ABORT") and not obs_row: unk.add("UNK-NESTED-IN-ABORT")
    if any(m.startswith("MARK-") or m.startswith("N-ED-") for m in markers): unk.add("UNK-MARKER-BINDING")
    if "H-LOAD" in h: unk.add("UNK-PAYLOAD-DB")
    if o in ("N-OBJ-CANCEL", "N-OBJ-UNDO"): unk.add("UNK-CANCEL-STAGING")
    return sorted(obs), sorted(fail), sorted(unk)

def canon(v):
    """Canonical text used for approval hashes; the validator implements the same function."""
    if isinstance(v, dict):
        return "{" + ",".join(canon(k) + ":" + canon(v[k]) for k in sorted(v.keys())) + "}"
    if isinstance(v, (list, tuple)):
        return "[" + ",".join(canon(x) for x in v) + "]"
    if isinstance(v, bool):
        return "true" if v else "false"
    if v is None:
        return "null"
    if isinstance(v, int):
        return str(v)
    s = str(v); out = []
    for ch in s:
        c = ord(ch)
        if ch in '"\\': out.append("\\" + ch)
        elif 0x20 <= c <= 0x7E: out.append(ch)
        else: out.append("\\u%04X" % c)
    return '"' + "".join(out) + '"'
