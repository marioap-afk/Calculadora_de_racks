# r3model.py - input builders and the MODEL CONTROLLER used by parts (a) and (c).
#
# The model Controller is a test device, not a Controller: it reads ONLY the facts of the tool envelopes it is
# given (pinned tool or a mutant) plus the delegation and the handoff, and derives the six Results that R4
# recomputes, with the reading of AUTOMATION_PLAN 16.9 and 16.13 (defect table) transcribed below. It never
# imports or calls the acceptor. Its purpose: show whether a false fact of the tool, taken at face value by a
# Controller, reaches a decision that the acceptor's R4 rejects.
import json
import os
import platform
import sys

ORDER = ("Termination", "Handoff", "Authority", "Contract", "Identity", "Remote", "Scope", "CleanTree", "Ci", "Tests",
         "Trailer", "Routing", "FreeText", "Denials")
R4_SET = ("Identity", "Scope", "CleanTree", "Trailer", "FreeText", "Authority")
# AP 16.9 table, column "Si falla" (not_run counts as fail with the row's disposition)
DISP = {"Termination": "BLOCKED", "Handoff": "REWORK", "Authority": "STOP", "Contract": "STOP", "Identity": "STOP",
        "Remote": "BLOCKED", "Scope": "STOP", "CleanTree": "REWORK", "Ci": "BLOCKED", "Tests": "REWORK",
        "Trailer": "REWORK", "Routing": "BLOCKED", "FreeText": "REWORK", "Denials": "BLOCKED"}
SEV = {"REWORK": 1, "BLOCKED": 2, "STOP": 3}


def _norm_path(p):
    return os.path.normcase(os.path.abspath(p)) if isinstance(p, str) and p else None


def facts(env):
    if not isinstance(env, dict) or env.get("Error") is not None or not isinstance(env.get("Facts"), dict):
        return None
    return env["Facts"]


# ------------------------------------------------------------------ model Controller rules (from tool facts)

def mc_identity(f, deleg, hand):
    if f is None:
        return "not_run", "identity facts unavailable"
    why = []
    if not (f.get("AbbrevRef") == deleg["ExpectedBranch"] == hand["Branch"]):
        why.append("branch")
    if not (_norm_path(f.get("Toplevel")) == _norm_path(deleg["ExpectedWorktree"]) == _norm_path(hand["Worktree"])):
        why.append("worktree")
    if deleg["BaseSha"] != hand["BaseSha"]:
        why.append("BaseSha delegation/handoff")
    anc = f.get("Ancestry") or {}
    if anc.get("base->current") is not True:
        why.append("BaseSha not ancestor of CurrentSha")
    if hand.get("RedSha") is not None and not (anc.get("base->red") is True and anc.get("red->current") is True):
        why.append("RedSha not between")
    if not (f.get("Head") == hand["CurrentSha"] and f.get("OriginBranch") == hand["CurrentSha"]):
        why.append("HEAD/origin/CurrentSha")
    return ("fail" if why else "pass"), "; ".join(why) or "identity facts consistent"


def mc_scope(f):
    if f is None:
        return "not_run", "scope facts unavailable"
    why = []
    if f.get("MalformedEntries"):
        why.append("malformed entries")
    for row in f.get("PerPath", []):
        if not row.get("AllowedCoveredBy"):
            why.append("not allowed: " + row["Path"])
        if row.get("ForbiddenCoveredBy"):
            why.append("forbidden: " + row["Path"])
    return ("fail" if why else "pass"), "; ".join(why[:4]) or "every path covered by AllowedWriteScope, none forbidden"


def mc_cleantree(f):
    if f is None:
        return "not_run", "cleantree facts unavailable"
    ok = f.get("StatusPorcelain") == [] and f.get("CheckIgnoreRc") == 0
    return ("pass" if ok else "fail"), "status=%d lines, check-ignore rc=%s" % (len(f.get("StatusPorcelain") or []), f.get("CheckIgnoreRc"))


def declared_trailer(hand):
    v = (hand.get("Worker") or {}).get("Trailer") or ""
    v = v.strip()
    if v.lower().startswith("co-authored-by:"):
        v = v[len("co-authored-by:"):].strip()
    return v


def mc_trailer(f, hand):
    if f is None:
        return "not_run", "diff facts unavailable"
    want = declared_trailer(hand)
    miss = [c for c in f.get("Commits", []) if want not in (f.get("CoAuthoredBy") or {}).get(c, [])]
    return ("fail" if (miss or not want) else "pass"), ("missing in %d commits" % len(miss)) if miss else "declared trailer in every commit"


def mc_freetext(f):
    if f is None:
        return "not_run", "freetext facts unavailable"
    m = f.get("Matches") or []
    return ("fail" if m else "pass"), "%d matches" % len(m)


def map_defects(f):
    """Defect table of AP 16.13 (Validate(M)), read from the tool facts only."""
    if f is None:
        return None
    d = []
    der, ent, v = f.get("Derive") or {}, f.get("Entry") or {}, f.get("Validate")
    if der.get("Eff") is None:
        d.append("no EFF")
        return d
    if len(ent.get("EntryMatches") or []) != 1 or len(ent.get("R163Matches") or []) != 1 or ent.get("EntryNamesR163") is not True:
        d.append("entry point / 16.13")
    if v is None:
        d.append("no Validate")
        return d
    m1 = v["MV1"]
    if not (m1.get("MapPresent") and m1.get("MapParses") and m1.get("SchemaPresent") and m1.get("SchemaViolations") == []):
        d.append("MV-1")
    if v["MV2"].get("MapSurfaces") is None or v["MV2"].get("MapSurfaces") != v["MV2"].get("ClosedList"):
        d.append("MV-2")
    m3, m4, m5, m6 = v.get("MV3"), v.get("MV4"), v.get("MV5"), v.get("MV6") or {}
    if m3 is None or m4 is None or m5 is None or m6.get("PerFile") is None:
        d.append("MV-3..MV-6 not computed")
    else:
        for k in ("OnlyInChanged", "OnlyInFiles", "FilesDuplicates", "DeletedUnderSurfaces"):
            if m3.get(k):
                d.append("MV-3 " + k)
        for k in ("LiteralViolations", "EntryFilesOutsideSchema", "AddedPresentInEff1"):
            if m4.get(k):
                d.append("MV-4 " + k)
        for k in ("EntryKeyDuplicates", "EntriesOnNonModifiedPath"):
            if m5.get(k):
                d.append("MV-5 " + k)
        for r in m6["PerFile"]:
            if r.get("H1Duplicates") or r.get("H2Duplicates"):
                d.append("MV-6 duplicates " + r["Path"])
            for a, b in (("DerivedModified", "MapModified"), ("DerivedRemoved", "MapRemoved"),
                         ("DerivedAddedOrEntry", "MapAddedOrEntry")):
                if sorted(r.get(a) or []) != sorted(r.get(b) or []):
                    d.append("MV-6 %s %s" % (a, r["Path"]))
        pair = lambda xs: sorted((x["Path"], " ".join(x["Section"].split())) for x in xs or [])
        if pair(m6.get("EntryKindEntries")) != pair(m6.get("ExpectedEntryKind")):
            d.append("MV-6 ENTRY pair")
    m7 = v["MV7"]
    if m7.get("EntryCountEff") != 1 or m7.get("EntryNamesR163Eff") is not True:
        d.append("MV-7 entry")
    if m7.get("PointerIsFirstSentenceS16") is not True or m7.get("PointerIsLastSentenceS163") is not True:
        d.append("MV-7 pointers")
    if m7.get("S163EqualAfterPointerRemoval") is not True:
        d.append("MV-7 16.3")
    return d


def mc_authority(f):
    d = map_defects(f)
    if d is None:
        return "not_run", "clausemap facts unavailable"
    return ("fail" if d else "pass"), ("defects: " + ", ".join(d[:6])) if d else "no map defect in the facts"


def authority_resolution(f, contract_auth, main, ar):
    """AuthorityResolution rows (16.13 steps 3 and 5) from the clausemap facts; [] without EFF."""
    if f is None or (f.get("Derive") or {}).get("Eff") is None or f.get("Validate") is None:
        return []
    eff = f["Derive"]["Eff"]
    mblob = f["Validate"]["MV1"].get("MapBlob")
    if not isinstance(mblob, str):
        return []
    rows = []
    for a in contract_auth:
        rev = ar if a["Class"] in ("UNIT_DOC", "UNIT_CHANGE") else main
        rows.append({"Path": a["Path"], "Section": a["Section"], "Class": a["Class"], "Mode": "NORMAL",
                     "RevisionSha": rev, "Parts": [], "EffectiveSha": eff, "ClauseMapBlob": mblob,
                     "UnitProtocol": "rackcad-protocol/I62"})
    return rows


def results_from_facts(envs, deleg, hand):
    """envs: {"identity","diff","scope","cleantree","freetext","clausemap","contract"?,"handoff"?} -> parsed envelopes."""
    g = lambda k: facts(envs.get(k))
    res = {}
    res["Identity"] = mc_identity(g("identity"), deleg, hand)
    res["Scope"] = mc_scope(g("scope"))
    res["CleanTree"] = mc_cleantree(g("cleantree"))
    res["Trailer"] = mc_trailer(g("diff"), hand)
    res["FreeText"] = mc_freetext(g("freetext"))
    res["Authority"] = mc_authority(g("clausemap"))
    c = g("contract")
    if c is None:
        res["Contract"] = ("not_run", "contract facts unavailable")
    else:
        ok = all(x["ContainedBy"] for x in c["A3"]) and all(x["ContainedBy"] for x in c["A4"]) and \
            not any(c["A5Missing"].values()) and c["SupersededEqual"] is True
        res["Contract"] = ("pass" if ok else "fail", "A3-A5 from contract facts")
    h = g("handoff")
    if h is None:
        res["Handoff"] = ("pass", "handoff present (model: not re-derived)")
    else:
        ok = h.get("Present") and h.get("ValidJson") and h.get("MissingRequiredKeys") == []
        res["Handoff"] = ("pass" if ok else "fail", "handoff facts")
    res["Termination"] = ("pass", "model: worker run recorded COMPLETED (synthetic)")
    res["Remote"] = ("not_run", "model: no relay Exit facts in the R3 harness")
    res["Ci"] = ("not_run", "model: no CI run for the synthetic delivery")
    res["Tests"] = ("not_run", "model: no TRX of the exact SHA")
    res["Routing"] = ("not_run", "model: no runtime observation")
    res["Denials"] = ("pass", "model: no denial events")
    return res


def coherent(results):
    """Classification, Disposition and FailureClass coherent with Checks (AP 16.9; RAE 8)."""
    bad = [k for k in ORDER if results[k] != "pass"]
    if not bad:
        return "EXECUTION_VERIFIED", "NONE", "NONE"
    worst = "REWORK"
    for k in bad:
        dsp = DISP[k]
        if k == "Handoff" and results.get("Identity") != "pass":
            dsp = "STOP"
        if SEV[dsp] > SEV[worst]:
            worst = dsp
    cls = "EXECUTION_REWORK_REQUIRED" if worst == "REWORK" else "EXECUTION_BLOCKED"
    return cls, worst, bad[0]


def controller_verification(results, ids, current, ar_rows, note):
    """rackcad-controller-verification/v2 instance. results: {check: (Result, Evidence)} for the 14 checks."""
    flat = dict((k, results[k][0]) for k in ORDER)
    cls, dsp, fc = coherent(flat)
    checks = {}
    for k in ORDER:
        r, ev = results[k]
        row = {"Result": r, "Evidence": "[R3 model Controller] " + ev}
        if k in ("Ci", "Tests"):
            row["RedPart"] = "not_applicable"
        checks[k] = row
    return {
        "Schema": "rackcad-controller-verification/v2", "TaskId": ids["TaskId"], "RunId": ids["RunId"],
        "DelegationRunId": ids["DelegationRunId"], "WorkRunId": ids["WorkRunId"], "Attempt": 0, "VerifiedSha": current,
        "Verifier": {"Role": "EXECUTION_CONTROLLER",
                     "BindingRef": {"UnitId": ids["Unit"], "Scope": "TASK", "TaskId": ids["TaskId"],
                                    "Role": "EXECUTION_CONTROLLER", "BindingId": "B20261012T080000Z-c0a3",
                                    "Sha256": "0" * 64,
                                    "Location": {"Kind": "TRANSIENT", "Path": "artifacts/orchestration/r3/binding.json",
                                                 "Commit": None, "Blob": None}},
                     "Actor": {"AdapterId": "r3-model-controller", "InstanceId": "UNOBSERVED",
                               "InstanceIdSource": "none: synthetic model Controller of the R3 evidence",
                               "Assurance": "NONE"}},
        "Checks": checks, "AuthorityResolution": ar_rows, "Classification": cls, "Disposition": dsp,
        "FailureClass": fc, "TriggeredStopConditions": [],
        "Findings": ["Synthetic output of the R3 model Controller (" + note + "); not a verification."],
        "RecommendedNextAction": "R3 evidence only."}


def flip(cv, check, value):
    """Copy of cv with one Result changed; Classification/Disposition/FailureClass recomputed (stays coherent)."""
    out = json.loads(json.dumps(cv))
    out["Checks"][check]["Result"] = value
    out["Checks"][check]["Evidence"] = "[R3 altered] " + out["Checks"][check]["Evidence"]
    flat = dict((k, out["Checks"][k]["Result"]) for k in ORDER)
    out["Classification"], out["Disposition"], out["FailureClass"] = coherent(flat)
    return out


# ------------------------------------------------------------------ input builders

def delegation_v2(contract, run_id, base, branch, worktree, main, ar, unit="FX-U1", initiative="FX-U1"):
    return {
        "Schema": "rackcad-delegation/v2", "TaskId": "T1", "RunId": run_id, "Initiative": initiative, "Unit": unit,
        "Gate": contract["Gate"], "Attempt": 0, "AuthorityRevision": ar, "MainSha": main, "BaseSha": base,
        "ExpectedBranch": branch, "ExpectedWorktree": worktree,
        "Owner": {"Kind": "external-process", "Id": "external-process:" + run_id},
        "TaskClass": "ROUTINE_IMPLEMENTATION",
        "Dimensions": dict((k, "Low") for k in ("Ambiguity", "ArchitecturalSensitivity", "Breadth", "ToolUse",
                                                 "FailureCost", "MechanicalRepetition", "Horizon")),
        "Executor": {"Provider": "synthetic", "Transport": "none", "Role": "WORKER", "Cell": "r3-synthetic",
                     "BindingRef": {"UnitId": unit, "Scope": "TASK", "TaskId": "T1", "Role": "WORKER",
                                    "BindingId": "B20261012T080000Z-b0a3", "Sha256": "0" * 64,
                                    "Location": {"Kind": "TRANSIENT", "Path": "artifacts/orchestration/r3/worker-binding.json",
                                                 "Commit": None, "Blob": None}}},
        "Model": "synthetic-worker", "Effort": {"Semantic": "Balanced", "Provider": "high"},
        "PromptProfile": "ROUTINE_IMPLEMENTATION", "RoutingReason": "R3 synthetic delegation (no routing performed)",
        "ModelEscalationReason": None, "RoutingEnforcement": "required",
        "Authorities": contract["Authorities"], "Objective": contract["Objective"],
        "AllowedWriteScope": contract["AllowedWriteScope"], "ForbiddenWriteScope": contract["ForbiddenWriteScope"],
        "Invariants": contract["Invariants"], "AcceptanceCriteria": ["R3 synthetic: the contract's criteria"],
        "RequiredTests": contract["RequiredTests"], "ExpectedEvidence": contract["ExpectedEvidence"],
        "StopConditions": contract["StopConditions"], "MaxReworkLoops": 3, "AttemptsRemaining": 3,
        "ChainBaseSha": base, "ChainRedSha": None, "ChainRedFiles": [], "CorrectionOf": None,
        "ExpectedHandoffPath": "artifacts/orchestration/%s/T1/0/{WorkRunId}/worker-handoff.json" % unit,
        "IssuedBy": "EXECUTION_CONTROLLER (synthetic R3 planning)", "RoleRequirements": contract["RoleRequirements"],
        "SupersededCommits": contract["SupersededCommits"]}


def handoff_v1(run_id, deleg_run_id, base, red, current, branch, worktree, files, trailer, evidence, gate,
               initiative="FX-U1", work_completed=None):
    return {
        "Schema": "rackcad-worker-handoff/v1", "TaskId": "T1", "RunId": run_id, "DelegationRunId": deleg_run_id,
        "Initiative": initiative, "Gate": gate, "Attempt": 0, "BaseSha": base, "RedSha": red, "CurrentSha": current,
        "Branch": branch, "Worktree": worktree, "Pushed": True, "FilesChanged": files,
        "WorkCompleted": work_completed or "Añadí Subtract a Calculator con su prueba (entrega simulada R3).",
        "TestsExecuted": [{"RunRef": "r3-red", "Command": "dotnet test --filter FullyQualifiedName~Subtract", "Phase": "RED",
                           "TreeSha": None, "ResultsFile": None},
                          {"RunRef": "r3-green", "Command": "dotnet test --filter FullyQualifiedName~Subtract",
                           "Phase": "GREEN", "TreeSha": None, "ResultsFile": None}],
        "TestResults": [{"RunRef": "r3-green", "Selected": 1, "Passed": 1, "Failed": 0, "Skipped": 0, "FailedTests": []}],
        "Evidence": evidence, "UnexpectedFindings": [], "KnownLimitations": [], "Deviations": [], "OpenQuestions": [],
        "WorkerStatus": "IMPLEMENTATION_COMPLETE", "Disposition": "NONE", "TriggeredStopConditions": [],
        "RecommendedNextAction": "Verificar T1 (R3).",
        "Worker": {"Provider": "synthetic", "ModelRequested": "synthetic-worker", "EffortRequested": "high", "Trailer": trailer}}


ECHO_TEMPLATE = '''# r3 echo tool: prints one fixed envelope (Runtime filled at run time). Test device of the R3 evidence;
# it lets R6/R7/R9 judge hand-derived or injected Facts/GitCalls while R1 and R3 still match.
import json, platform, sys
ENV = json.loads(%r)
ENV["Runtime"] = {"Python": platform.python_version(), "Isolated": sys.flags.isolated == 1}
sys.stdout.buffer.write((json.dumps(ENV, ensure_ascii=True, sort_keys=True, separators=(",", ":")) + "\\n").encode("ascii"))
'''


def write_echo_tool(path, envelope):
    env = dict(envelope)
    env.pop("Runtime", None)
    text = ECHO_TEMPLATE % json.dumps(env, ensure_ascii=True, sort_keys=True, separators=(",", ":"))
    d = os.path.dirname(path)
    os.makedirs(d, exist_ok=True)
    with open(path, "wb") as fh:
        fh.write(text.encode("ascii"))
    return path
