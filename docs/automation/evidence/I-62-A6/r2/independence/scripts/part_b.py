# part_b.py - (b) clause-map facts of the acceptor (R6/R7) WITHOUT the circular fake tool (run: python -I part_b.py).
#
# Oracles: (1) hand-derived expected facts, written below by construction of each synthetic map (which path lands in
# which list, which headings and sentences, which line numbers); blob ids are computed by the harness with hashlib over
# `git show <commit>:<path>` content (a primitive neither piece uses for blobs); (2) the REAL pinned tool.
# Nothing here imports, copies or calls in-process the acceptor or the tool; fake_tool.py is not used.
#
# B1 tool vs hand      : pinned tool clausemap Facts compared field by field with the hand-derived values.
# B2 acceptor vs hand  : an "echo tool" (prints a fixed envelope; R1/R3 MATCH by construction) carries the hand-derived
#                        Facts; R6/R7 MATCH means the acceptor's own computation equals them on every compared field.
#                        The same request carries a controller-verification with Authority = pass, so R4's map part
#                        is checked against the hand-derived "defect present?" of each map.
# B3 acceptor vs tool  : the real pinned tool envelope judged by the pinned acceptor.
# B4 sensitivity       : echo envelopes with one hand-derived field perturbed must give R6/R7 MISMATCH on that field.
import copy
import importlib.util
import json
import os
import sys
import time

_d = os.path.dirname(os.path.abspath(__file__))


def _load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(_d, name + ".py"))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


L = _load("r3lib")
MD = _load("r3model")
BR = _load("build_repos")

W = L.RUNS + "/b"
S = L.SYN
AP, WF, MAPP, SCH = L.AP_PATH, L.WF_PATH, L.MAP_PATH, L.MAP_SCHEMA_PATH
NOTE = "docs/automation/agent-execution/note.md"
EXTRA = "docs/automation/agent-execution/extra.json"
UNL = "docs/automation/agent-execution/unlisted.md"
STAB = "docs/automation/agent-execution/stable.md"
OLD = "docs/automation/agent-execution/old.md"
ADDED_SENT = "Frase añadida después del puntero."


def show_blob(commit, path):
    """Blob id by the harness: hashlib over the content git resolves for <commit>:<path>; None if absent."""
    try:
        data = L.git(S, "show", commit + ":" + path)
    except RuntimeError:
        return None
    return L.blob_id(data)


def map_at(commit):
    return json.loads(L.git(S, "show", commit + ":" + MAPP).decode("utf-8"))


# ------------------------------------------------------------------ hand derivations

# Per scenario, by construction (build_repos.py): changed paths under Surfaces, defect lists, MV-6 sets, MV-7 values.
HAND = {
    "V0": {}, "D1-literal": {"Literal": [AP]}, "D2-added-in-eff1": {"Changed+": [NOTE], "Added1": [NOTE]},
    "D3-entry-outside-schema": {"Changed+": [EXTRA], "Outside": [EXTRA]},
    "D4-entry-in-eff1": {"Entry1": [SCH]},
    "D5-only-in": {"Changed+": [UNL], "OnlyInChanged": [UNL], "OnlyInFiles": [STAB], "Mv6Extra": [STAB]},
    "D6-deleted": {"Changed+": [OLD], "Deleted": [OLD], "OnlyInChanged": [OLD]},
    "D7-mv6": {"MapModifiedAP": [BR.S16_H], "MapRemovedAP": [BR.S1_H]},
    "D8-mv7": {"Mv7Extra": True},
    "D9-mv5-dup": {"EntryDup": [{"Path": AP, "Section": BR.S16_H}]},
    "D10-mv2": {"Surfaces": [BR.SURF[1], BR.SURF[0], BR.SURF[2]]},
    "D11-mv1-schema": {"SchemaViolations": [{"Keyword": "enum", "Pointer": "/Files/2/FileKind"}], "RenamedSchema": True},
}
# hand-derived "is there a defect of the AP 16.13 table?" (EntryFilePresentInEff1 is informative only)
DEFECT = {"V0": False, "D1-literal": True, "D2-added-in-eff1": True, "D3-entry-outside-schema": True,
          "D4-entry-in-eff1": False, "D5-only-in": True, "D6-deleted": True, "D7-mv6": True, "D8-mv7": True,
          "D9-mv5-dup": True, "D10-mv2": True, "D11-mv1-schema": True}


def expected_facts(name, rec):
    h = HAND[name]
    b, m, t = rec["Base"], rec["Main"], rec["Trailer"]
    mp = map_at(m)
    files = mp["Files"]
    changed = sorted([AP, WF, SCH] + h.get("Changed+", []))
    rows = []
    for f in files:
        rows.append({"Path": f["Path"], "FileKind": f["FileKind"], "MapBaseBlob": f["BaseBlob"], "MapEffBlob": f["EffBlob"],
                     "ObservedEff1Blob": show_blob(b, f["Path"]), "ObservedEffBlob": show_blob(m, f["Path"])})
    map_mod_ap = h.get("MapModifiedAP", sorted([BR.S16_H, BR.S163_H]))
    per_file = [
        {"Path": AP, "H1Duplicates": [], "H2Duplicates": [], "DerivedModified": sorted([BR.S16_H, BR.S163_H]),
         "DerivedRemoved": [], "DerivedAddedOrEntry": [BR.R163_H], "MapModified": sorted(map_mod_ap),
         "MapRemoved": h.get("MapRemovedAP", []), "MapAddedOrEntry": [BR.R163_H]},
        {"Path": WF, "H1Duplicates": [], "H2Duplicates": [], "DerivedModified": [], "DerivedRemoved": [],
         "DerivedAddedOrEntry": [BR.ENTRY_H], "MapModified": [], "MapRemoved": [], "MapAddedOrEntry": [BR.ENTRY_H]}]
    for p in h.get("Mv6Extra", []):
        per_file.append({"Path": p, "H1Duplicates": [], "H2Duplicates": [], "DerivedModified": [], "DerivedRemoved": [],
                         "DerivedAddedOrEntry": [], "MapModified": [], "MapRemoved": [], "MapAddedOrEntry": []})
    pair = [{"Path": AP, "Section": BR.R163_H}, {"Path": WF, "Section": BR.ENTRY_H}]
    s163_eff = BR.S163_BODY + (" " + ADDED_SENT if h.get("Mv7Extra") else "")
    validate = {
        "MV1": {"MapPresent": True, "MapBlob": show_blob(m, MAPP), "MapParses": True, "SchemaPresent": True,
                "SchemaBlob": show_blob(m, SCH), "SchemaViolations": h.get("SchemaViolations", [])},
        "MV2": {"MapSurfaces": h.get("Surfaces", BR.SURF), "ClosedList": BR.SURF},
        "MV3": {"ChangedUnderSurfaces": changed, "DeletedUnderSurfaces": h.get("Deleted", []),
                "FilesPaths": [f["Path"] for f in files], "FilesDuplicates": [],
                "OnlyInChanged": h.get("OnlyInChanged", []), "OnlyInFiles": h.get("OnlyInFiles", [])},
        "MV4": {"Rows": rows, "LiteralViolations": h.get("Literal", []), "EntryFilesOutsideSchema": h.get("Outside", []),
                "AddedPresentInEff1": h.get("Added1", []), "EntryFilePresentInEff1": h.get("Entry1", [])},
        "MV5": {"EntryKeyDuplicates": h.get("EntryDup", []), "EntriesOnNonModifiedPath": []},
        "MV6": {"PerFile": per_file, "EntryKindEntries": pair, "ExpectedEntryKind": pair},
        "MV7": {"EntryCountEff": 1, "EntryNamesR163Eff": True, "FirstSentenceS16": BR.POINTER,
                "PointerIsFirstSentenceS16": True, "LastSentenceS163": ADDED_SENT if h.get("Mv7Extra") else BR.POINTER,
                "PointerIsLastSentenceS163": not h.get("Mv7Extra"), "S163EffMinusPointer": s163_eff,
                "S163Eff1": BR.S163_BODY, "S163EqualAfterPointerRemoval": not h.get("Mv7Extra")}}
    derive = {"TrailerCommits": [t], "FirstParentMerges": [m], "Candidates": [m], "Eff": m, "EffParent1": b, "EffParent2": t}
    entry = {"WorkflowBlob": show_blob(m, WF), "ApBlob": show_blob(m, AP), "EntryMatches": [7], "R163Matches": [19],
             "EntryNamesR163": True}
    # sanity of the hand line numbers against the constructed text (not against either piece)
    assert BR.wf_text(True).split("\n").index(BR.ENTRY_H) + 1 == 7
    assert BR.ap_text(True).split("\n").index(BR.R163_H) + 1 == 19
    return {"Derive": derive, "Pre": None, "Classify": None, "Entry": entry, "Validate": validate, "Resolve": None}


# Fields compared in B1 (exact; lists the SPEC orders as stated). Path tuples into Facts.
B1_FIELDS = [("Derive", k) for k in ("TrailerCommits", "FirstParentMerges", "Candidates", "Eff", "EffParent1", "EffParent2")] + \
    [("Entry", k) for k in ("WorkflowBlob", "ApBlob", "EntryMatches", "R163Matches", "EntryNamesR163")] + \
    [("Validate", "MV1", k) for k in ("MapPresent", "MapBlob", "MapParses", "SchemaPresent", "SchemaBlob", "SchemaViolations")] + \
    [("Validate", "MV2", k) for k in ("MapSurfaces", "ClosedList")] + \
    [("Validate", "MV3", k) for k in ("ChangedUnderSurfaces", "DeletedUnderSurfaces", "FilesPaths", "FilesDuplicates",
                                      "OnlyInChanged", "OnlyInFiles")] + \
    [("Validate", "MV4", k) for k in ("Rows", "LiteralViolations", "EntryFilesOutsideSchema", "AddedPresentInEff1",
                                      "EntryFilePresentInEff1")] + \
    [("Validate", "MV5", "EntryKeyDuplicates"), ("Validate", "MV5", "EntriesOnNonModifiedPath")] + \
    [("Validate", "MV6", k) for k in ("PerFile", "EntryKindEntries", "ExpectedEntryKind")] + \
    [("Validate", "MV7", k) for k in ("EntryCountEff", "EntryNamesR163Eff", "FirstSentenceS16", "PointerIsFirstSentenceS16",
                                      "LastSentenceS163", "PointerIsLastSentenceS163", "S163EffMinusPointer", "S163Eff1",
                                      "S163EqualAfterPointerRemoval")]


def dig(obj, path):
    for k in path:
        if not isinstance(obj, dict) or k not in obj:
            return "<absent>"
        obj = obj[k]
    return obj


def setp(obj, path, value):
    for k in path[:-1]:
        obj = obj[k]
    obj[path[-1]] = value


def b1_compare(tool_facts, exp):
    out = []
    for path in B1_FIELDS:
        a, b = dig(tool_facts, path), dig(exp, path)
        if path[-1] == "PerFile" and isinstance(a, list):
            a = [dict((k, v) for k, v in r.items()) for r in a]
        out.append({"Field": ".".join(path), "Equal": L.canon(a) == L.canon(b)})
    return out


# ------------------------------------------------------------------ requests

def echo_envelope(argv, facts):
    return {"Schema": "rackcad-bmin-facts/0-experimental", "Command": argv[0], "Args": {"main": argv[argv.index("--main") + 1],
            "repo": S}, "Inputs": [], "GitCalls": [], "Facts": facts,
            "FactsSha256": L.sha256_bytes(L.canon(facts).encode("ascii")), "Error": None}


def closure(cache={}):
    if "c" not in cache:
        extra = sorted(set(BR.SURF + [OLD, "artifacts/orchestration/R3/T1/0/R20261012T102000Z-c002/output.json"]))
        cl = L.broad_closure(S, extra, [{"Name": "DELEG", "Sha256": L.sha256_file(INP + "/deleg-syn.json")},
                                        {"Name": "H", "Sha256": L.sha256_file(INP + "/handoff-syn.json")}], "r3/head")
        p = W + "/closure.json"
        cache["c"] = (p, L.write_json_file(p, cl))
    return cache["c"]


INP = W + "/inputs"


def common_controller(ctx, name, main, authority, ar_rows):
    """controller-verification/v2: rows other than Authority from the pinned tool's SYN-COMMON-like facts."""
    res = MD.results_from_facts(ctx["CommonEnvs"], ctx["Deleg"], ctx["Hand"])
    res["Authority"] = (authority, "R3 part (b): Authority claimed %s by the test" % authority)
    cv = MD.controller_verification(res, ctx["Ids"], ctx["V0"]["Main"], ar_rows, "part (b) " + name)
    errs = L.validate_schema(cv, ctx["CvSchema"])
    if errs:
        raise RuntimeError(errs[:3])
    p = "%s/cv/%s-%s.json" % (W, name, authority)
    return p, L.write_json_file(p, cv, pretty=True)


def acc_request(ctx, name, label, tool, argv, envp, verif=None):
    cp, cs = closure()
    req = {"Schema": "rackcad-bmin-accept-request/0-experimental", "Repo": S,
           "Tool": {"Path": tool, "PinnedBlob": L.blob_id(L.read_bytes(tool)), "RunFrom": "path"},
           "Interpreter": ctx["Interp"], "Runs": [{"Name": "clausemap", "Envelope": envp, "Argv": argv}],
           "Closure": {"Path": cp, "Sha256": cs},
           "RelayFacts": {"Path": INP + "/relay.json", "CitedSha256": L.sha256_file(INP + "/relay.json")},
           "NotApplicable": {"R5": "part (b): clause-map only request"}}
    if verif is None:
        req["NotApplicable"]["R4"] = "part (b): facts-level request"
    else:
        req["Verification"] = verif
    rp = "%s/req/%s-%s.json" % (W, name, label)
    L.write_json_file(rp, req)
    rc, res = L.run_acceptor(rp)
    s = L.acceptor_summary(rc, res)
    s["Fields"] = L.mismatch_fields(res, limit=30)
    r4 = [c for c in res.get("Checks", []) if c.get("Id") == "R4"]
    if r4 and isinstance(r4[0].get("Detail"), dict):
        s["R4Rows"] = dict((row["Check"], {"Controller": row.get("Controller"), "AcceptorExpects": row.get("AcceptorExpects"),
                                           "Status": row.get("Status"), "Defects": row.get("Defects")})
                           for row in r4[0]["Detail"].get("Checks", []))
    return s


def verification(ctx, name, rec, authority, ar_rows):
    cvp, cvs = common_controller(ctx, name, rec["Main"], authority, ar_rows)
    return {"Controller": {"Path": cvp, "Sha256": cvs},
            "Delegation": {"Path": INP + "/deleg-syn.json", "Sha256": L.sha256_file(INP + "/deleg-syn.json")},
            "Handoff": {"Path": INP + "/handoff-syn.json", "Sha256": L.sha256_file(INP + "/handoff-syn.json")},
            "IgnorePath": "artifacts/orchestration/R3/T1/0/R20261012T102000Z-c002/output.json",
            "TermsRev": ctx["V0"]["Main"], "MainSha": rec["Main"]}


PERTURB = [
    ("V0", "Derive.Eff", ("Derive", "Eff"), lambda e, r: r["Base"]),
    ("V0", "Derive.FirstParentMerges order/content", ("Derive", "FirstParentMerges"), lambda e, r: [r["Main"], r["Base"]]),
    ("V0", "Entry.EntryMatches", ("Entry", "EntryMatches"), lambda e, r: [8]),
    ("V0", "MV3.OnlyInFiles", ("Validate", "MV3", "OnlyInFiles"), lambda e, r: [AP]),
    ("V0", "MV6.AP.DerivedModified", ("Validate", "MV6", "PerFile"),
     lambda e, r: [dict(e["Validate"]["MV6"]["PerFile"][0], DerivedModified=[BR.S16_H])] + e["Validate"]["MV6"]["PerFile"][1:]),
    ("V0", "MV7.S163EffMinusPointer (whole line removed)", ("Validate", "MV7", "S163EffMinusPointer"),
     lambda e, r: ""),
    ("D1-literal", "MV4.LiteralViolations emptied", ("Validate", "MV4", "LiteralViolations"), lambda e, r: []),
    ("D2-added-in-eff1", "MV4 lists merged (R61 b into literal)", ("Validate", "MV4", "LiteralViolations"), lambda e, r: [NOTE]),
    ("D3-entry-outside-schema", "MV4.EntryFilesOutsideSchema emptied", ("Validate", "MV4", "EntryFilesOutsideSchema"), lambda e, r: []),
    ("D4-entry-in-eff1", "ENTRY file treated as ADDED", ("Validate", "MV4", "AddedPresentInEff1"), lambda e, r: [SCH]),
    ("D5-only-in", "MV3.OnlyInChanged emptied", ("Validate", "MV3", "OnlyInChanged"), lambda e, r: []),
    ("D6-deleted", "MV3 deleted path dropped from changed (M18 form)", ("Validate", "MV3", "ChangedUnderSurfaces"),
     lambda e, r: [p for p in e["Validate"]["MV3"]["ChangedUnderSurfaces"] if p != OLD]),
    ("D7-mv6", "MV6.AP.MapRemoved emptied", ("Validate", "MV6", "PerFile"),
     lambda e, r: [dict(e["Validate"]["MV6"]["PerFile"][0], MapRemoved=[])] + e["Validate"]["MV6"]["PerFile"][1:]),
    ("D8-mv7", "MV7.PointerIsLastSentenceS163 flipped", ("Validate", "MV7", "PointerIsLastSentenceS163"), lambda e, r: True),
    ("D9-mv5-dup", "MV5.EntryKeyDuplicates emptied", ("Validate", "MV5", "EntryKeyDuplicates"), lambda e, r: []),
    ("D10-mv2", "MV2.MapSurfaces equal to the closed list", ("Validate", "MV2", "MapSurfaces"), lambda e, r: BR.SURF),
    ("D11-mv1-schema", "MV1.SchemaViolations emptied", ("Validate", "MV1", "SchemaViolations"), lambda e, r: []),
]


def main():
    t0 = time.time()
    build = json.loads(L.read_bytes(L.WORK + "/build.json").decode("ascii"))
    syn = build["Syn"]
    os.makedirs(INP, exist_ok=True)
    # inputs of the R4 part (same construction as part (a), rebuilt here so part (b) stands alone)
    v0 = syn["V0"]
    syn_contract = {"Gate": "R3-G1", "Objective": "R3 synthetic task", "Invariants": [], "ExpectedEvidence": [],
                    "StopConditions": [], "RoleRequirements": [], "SupersededCommits": [], "RequiredTests": [],
                    "Authorities": [{"Path": AP, "Section": BR.S16_H, "Class": "EXTERNAL"}],
                    "AllowedWriteScope": ["docs/**"], "ForbiddenWriteScope": ["src/**"]}
    deleg = MD.delegation_v2(syn_contract, "R20261012T100000Z-d002", v0["Base"], "r3/head", S, v0["Main"], v0["Base"],
                             unit="R3", initiative="R3")
    hand = MD.handoff_v1("R20261012T101000Z-a002", deleg["RunId"], v0["Base"], None, v0["Main"], "r3/head", S, [AP],
                         "Co-Authored-By: R3 Synthetic Worker", ["Revisados los Candidates del lote sintético."], "R3-G1",
                         initiative="R3", work_completed="Activación sintética R3.")
    L.write_json_file(INP + "/deleg-syn.json", deleg, pretty=True)
    L.write_json_file(INP + "/handoff-syn.json", hand, pretty=True)
    L.write_json_file(INP + "/relay.json", {"Schema": "synthetic-relay-facts", "Note": "R3 part (b)"})
    ign = "artifacts/orchestration/R3/T1/0/R20261012T102000Z-c002/output.json"
    common = {}
    for n, argv in (("identity", ["identity", "--repo", S, "--branch", "r3/head", "--base", v0["Base"], "--current", v0["Main"]]),
                    ("diff", ["diff", "--repo", S, "--from", v0["Base"], "--to", v0["Main"]]),
                    ("scope", ["scope", "--repo", S, "--from", v0["Base"], "--to", v0["Main"], "--file", "DELEG=" + INP + "/deleg-syn.json",
                               "--sha256", "DELEG=" + L.sha256_file(INP + "/deleg-syn.json")]),
                    ("cleantree", ["cleantree", "--repo", S, "--ignore-path", ign]),
                    ("freetext", ["freetext", "--repo", S, "--file", "H=" + INP + "/handoff-syn.json", "--sha256",
                                  "H=" + L.sha256_file(INP + "/handoff-syn.json"), "--terms-rev", v0["Main"]])):
        rc, out = L.run_tool(L.TOOL, argv, cwd=S)
        common[n] = json.loads(out.decode("ascii"))
    ctx = {"Interp": {"Python": L.platform.python_version(), "ExecutableSha256": L.exe_sha256()},
           "CvSchema": json.loads(L.schema_bytes("controller-verification.v2.schema.json").decode("utf-8")),
           "CommonEnvs": common, "Deleg": deleg, "Hand": hand, "V0": v0,
           "Ids": {"TaskId": "T1", "RunId": "R20261012T102000Z-c002", "DelegationRunId": deleg["RunId"],
                   "WorkRunId": hand["RunId"], "Unit": "R3"}}
    rows = []
    for name in HAND:
        rec = syn[name]
        exp = expected_facts(name, rec)
        argv = ["clausemap", "--repo", S, "--main", rec["Main"]]
        # B1 + B3: the real pinned tool
        rc, raw = L.run_tool(L.TOOL, argv, cwd=S)
        tenv = json.loads(raw.decode("ascii"))
        tp = "%s/env/%s-tool.json" % (W, name)
        L.write_bytes(tp, raw)
        b1 = b1_compare(tenv.get("Facts") or {}, exp)
        b3 = acc_request(ctx, name, "B3-tool", L.TOOL, argv, tp)
        # B2: echo tool with the hand-derived facts, plus R4 with Authority = pass
        echo = MD.write_echo_tool("%s/echo/%s/bmin_facts.py" % (W, name), echo_envelope(argv, exp))
        rc2, raw2 = L.run_tool(echo, argv, cwd=S)
        ep = "%s/env/%s-hand.json" % (W, name)
        L.write_bytes(ep, raw2)
        ar_rows = MD.authority_resolution(exp, syn_contract["Authorities"], rec["Main"], v0["Base"])
        b2 = acc_request(ctx, name, "B2-hand", echo, argv, ep, verification(ctx, name, rec, "pass", ar_rows))
        row = {"Scenario": name, "HandDefect": DEFECT[name], "Main": rec["Main"],
               "B1ToolVsHand": {"Compared": len(b1), "Unequal": [x["Field"] for x in b1 if not x["Equal"]],
                                "ToolError": tenv.get("Error")},
               "B2AcceptorVsHand": {"R6": b2["Status"].get("R6"), "R7": b2["Status"].get("R7"),
                                    "R4Authority": (b2.get("R4Rows") or {}).get("Authority"), "Decision": b2["Decision"],
                                    "Mismatch": b2["Mismatch"], "Fields": b2["Fields"]},
               "B3AcceptorVsTool": {"R6": b3["Status"].get("R6"), "R7": b3["Status"].get("R7"), "Decision": b3["Decision"],
                                    "Mismatch": b3["Mismatch"], "Fields": b3["Fields"]}}
        if name == "V0":
            # Authority = fail with a valid map: the acceptor leaves Authority unconstrained (asymmetry of R4)
            b2f = acc_request(ctx, name, "B2-hand-authority-fail", echo, argv, ep, verification(ctx, name, rec, "fail", ar_rows))
            row["B2AuthorityFailOnValidMap"] = {"R4Authority": (b2f.get("R4Rows") or {}).get("Authority"),
                                                "R4": b2f["Status"].get("R4"), "Decision": b2f["Decision"]}
        if name == "D11-mv1-schema":
            # the tool's null form of MV2.MapSurfaces / MV3-MV6, in an echo envelope (isolates R6 from R3)
            tf = copy.deepcopy(exp)
            for k in ("MV3", "MV4", "MV5"):
                tf["Validate"][k] = None
            tf["Validate"]["MV2"]["MapSurfaces"] = None
            tf["Validate"]["MV6"]["PerFile"] = None
            tf["Validate"]["MV6"]["EntryKindEntries"] = None
            e3 = MD.write_echo_tool("%s/echo/%s-toolform/bmin_facts.py" % (W, name), echo_envelope(argv, tf))
            rc3, raw3 = L.run_tool(e3, argv, cwd=S)
            p3 = "%s/env/%s-toolform.json" % (W, name)
            L.write_bytes(p3, raw3)
            b3b = acc_request(ctx, name, "B3-toolform-echo", e3, argv, p3)
            row["ToolNullFormThroughEcho"] = {"R6": b3b["Status"].get("R6"), "Fields": b3b["Fields"].get("R6")}
        rows.append(row)
        L.log(name, "B1 unequal:", row["B1ToolVsHand"]["Unequal"], "| B2:", b2["Status"].get("R6"), b2["Status"].get("R7"),
              (b2.get("R4Rows") or {}).get("Authority", {}).get("Status"), "| B3:", b3["Decision"], b3["Mismatch"])
    sens = []
    for name, label, path, fn in PERTURB:
        rec = syn[name]
        exp = expected_facts(name, rec)
        bad = copy.deepcopy(exp)
        setp(bad, list(path), fn(exp, rec))
        argv = ["clausemap", "--repo", S, "--main", rec["Main"]]
        tag = "%s-%d" % (name, PERTURB.index((name, label, path, fn)))
        echo = MD.write_echo_tool("%s/echo/%s/bmin_facts.py" % (W, tag), echo_envelope(argv, bad))
        rc, raw = L.run_tool(echo, argv, cwd=S)
        ep = "%s/env/%s.json" % (W, tag)
        L.write_bytes(ep, raw)
        s = acc_request(ctx, tag, "B4", echo, argv, ep)
        sens.append({"Scenario": name, "Perturbation": label, "Field": ".".join(path), "R6": s["Status"].get("R6"),
                     "R7": s["Status"].get("R7"), "Fields": s["Fields"].get("R6", []) + s["Fields"].get("R7", [])})
        L.log("B4", tag, label, s["Status"].get("R6"), s["Status"].get("R7"))
    summary = {
        "B1ToolEqualsHandOnAllFields": [r["Scenario"] for r in rows if not r["B1ToolVsHand"]["Unequal"]],
        "B1ToolDiffersFromHand": dict((r["Scenario"], r["B1ToolVsHand"]["Unequal"]) for r in rows if r["B1ToolVsHand"]["Unequal"]),
        "B2AcceptorEqualsHand_R6R7Match": [r["Scenario"] for r in rows
                                           if r["B2AcceptorVsHand"]["R6"] == "MATCH" and r["B2AcceptorVsHand"]["R7"] == "MATCH"],
        "B2R4AuthorityPassJudgedAsHand": [r["Scenario"] for r in rows if (r["B2AcceptorVsHand"]["R4Authority"] or {}).get("Status")
                                          == ("MISMATCH" if r["HandDefect"] else "MATCH")],
        "B3AcceptorAcceptsRealTool": [r["Scenario"] for r in rows if r["B3AcceptorVsTool"]["Decision"] == "ACCEPT_FACTS"],
        "B3AcceptorRejectsRealTool": dict((r["Scenario"], r["B3AcceptorVsTool"]["Mismatch"]) for r in rows
                                          if r["B3AcceptorVsTool"]["Decision"] != "ACCEPT_FACTS"),
        "B4PerturbationsDetected": sum(1 for s in sens if "MISMATCH" in (s["R6"], s["R7"])), "B4Total": len(sens),
        "Seconds": round(time.time() - t0, 1)}
    res = {"Schema": "r3-part-b/1", "Header": L.common_header(), "Scenarios": rows, "Sensitivity": sens, "Summary": summary,
           "Method": "hand-derived expected facts (by construction) + harness blob ids (hashlib over git show); echo tool; "
                     "no import of the acceptor or of fake_tool.py"}
    p, sha = L.write_result("b-clausemap.json", res)
    L.log("written", p, sha, json.dumps(summary)[:1500])


if __name__ == "__main__":
    main()
