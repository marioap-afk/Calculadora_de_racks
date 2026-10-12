# part_a.py - (a) mutation independence with observable effects (run: python -I part_a.py [--fresh]).
#
# For each of the 27 mutants of the PINNED tool (separate files built from mutate.py's definitions):
#   1. activation: run pinned and mutant on every scenario run (real fixture data first, then the synthetic
#      mini repository) and compare Facts, FactsSha256, Error, GitCalls and Inputs;
#   2. for every (mutant, scenario) that activated, run the PINNED acceptor twice:
#        config M ("mutant as pinned"): Tool.Path = mutant file, PinnedBlob = mutant blob -> R1 and R3 re-run
#                 comparison cannot detect it; whatever MISMATCHes is an independent detector;
#        config P ("plain"): Tool.Path = real pinned tool, PinnedBlob = 9a829b0e, envelopes = the mutant's.
#      Scenarios with a Verification also carry a controller-verification/v2 that the model Controller derived
#      from the SAME tool variant's facts (decision level: R4 is exercised, never declared not applicable).
#   3. baseline: pinned tool envelopes + pinned acceptor on every scenario.
# Fail-closed-only mutants without a CLI path (M15, M23) get an in-process unit probe and an envelope-injection
# check of the acceptor's R9 detector, reported separately and never counted as activation.
import importlib.util
import json
import os
import shutil
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

A = L.RUNS + "/a"
INP = A + "/inputs"
FRESH = "--fresh" in sys.argv

CRITICAL = {
    "M01": "scope coverage (Scope membership)", "M02": "MV-7 (sentence boundary)", "M03": "ancestry / Identity",
    "M04": "Scope membership (diff paths)", "M05": "MV-4 (four lists)", "M06": "MV-7 (pointer removal)",
    "M07": "MV-4 (ENTRY vs ADDED)", "M08": "effective point derivation (EFF)", "M09": "effective point derivation (trailer)",
    "M10": "canonical ASCII serialization (fidelity)", "M11": "isolation (-I)", "M12": "scope coverage (U-19)",
    "M13": "blobs and hashes", "M14": "free text (16.10; not in the A6-9 critical list)",
    "M15": "isolation (closed git list)", "M16": "effective point derivation (trailer uniqueness)",
    "M17": "MV-1 (schema subset; not in the A6-9 critical list)", "M18": "MV-3", "M19": "Classify (claim_id; not critical list)",
    "M20": "MV-7 (last sentence)", "M21": "effective point derivation (first-parent condition)", "M22": "MV-6 (preamble)",
    "M23": "isolation (forced --porcelain)", "M24": "MV-6 / Markdown code fences", "M25": "strict JSON (MV-1, inputs)",
    "M26": "isolation (forced --no-ext-diff --no-textconv)", "M27": "Classify (claim_id)"}


def jload(path):
    with open(path, "rb") as fh:
        return json.loads(fh.read().decode("utf-8"))


def file_arg(name, path):
    return ["--file", name + "=" + path, "--sha256", name + "=" + L.sha256_file(path)]


# ------------------------------------------------------------------ inputs

def build_inputs(build):
    os.makedirs(INP, exist_ok=True)
    kb = L.git(L.REAL, "show", L.TIP + ":" + L.K)
    L.write_bytes(INP + "/K.json", kb)
    K = json.loads(kb.decode("utf-8"))
    L.write_bytes(INP + "/fixture-tests.trx", L.git(L.WT, "show", "HEAD:" + L.TRX_SRC))
    assert L.sha256_file(INP + "/fixture-tests.trx") == L.TRX_SHA
    L.write_json_file(INP + "/relay.json", {"Schema": "synthetic-relay-facts", "Note": "R3 evidence; no normative value"})
    # S1: the previous e2e handoff, unchanged except the worktree path
    e2e_h = {"Schema": "rackcad-worker-handoff/v1", "TaskId": "T1", "RunId": "R20261011T000000Z-e2e0",
             "DelegationRunId": "R20261011T000000Z-e2ep", "Initiative": "FX-U1", "Gate": "G1", "Attempt": 0,
             "BaseSha": L.BASE, "RedSha": L.RED, "CurrentSha": L.HEADX, "Branch": L.BR, "Worktree": L.REAL,
             "Pushed": True, "FilesChanged": ["src/Fixture.Lib/Calculator.cs"],
             "WorkCompleted": "Añadí Subtract — escenario sintético de integración B-min",
             "TestsExecuted": ["FullyQualifiedName~Subtract"], "TestResults": "sintético",
             "Evidence": ["GATE PASS (término de 16.10 a propósito, para el hecho de freetext)"],
             "UnexpectedFindings": [], "KnownLimitations": [], "Deviations": [], "OpenQuestions": [],
             "WorkerStatus": "COMPLETED", "Disposition": "NONE", "TriggeredStopConditions": [],
             "RecommendedNextAction": "verificar", "Worker": {"Trailer": "Co-Authored-By: synthetic"}}
    L.write_json_file(INP + "/handoff-e2e.json", e2e_h, pretty=True)
    # S2: delegation/v2 and worker-handoff/v1 of the simulated delivery (schema-conforming)
    dv = build["Deliv"]
    deleg = MD.delegation_v2(K, "R20261012T090000Z-d001", L.TIP, L.BR, L.DELIV, L.MAIN, L.AR)
    hand = MD.handoff_v1("R20261012T091000Z-a001", deleg["RunId"], L.TIP, dv["RedSha"], dv["GreenSha"], L.BR,
                         L.DELIV, sorted(dv["Files"]), dv["Trailer"],
                         ["Revisados los Candidates de prueba del lote: ninguno falla."], K["Gate"])
    errs = L.validate_schema(deleg, json.loads(L.schema_bytes("delegation.v2.schema.json").decode("utf-8")))
    errs += L.validate_schema(hand, json.loads(L.schema_bytes("worker-handoff.schema.json").decode("utf-8")))
    if errs:
        raise RuntimeError("deliv inputs do not conform: %s" % errs[:5])
    L.write_json_file(INP + "/deleg-deliv.json", deleg, pretty=True)
    L.write_json_file(INP + "/handoff-deliv.json", hand, pretty=True)
    dup = L.read_bytes(INP + "/deleg-deliv.json").decode("utf-8").replace(
        '"TaskId": "T1",', '"TaskId": "T1",\n "TaskId": "T1",', 1)
    L.write_bytes(INP + "/deleg-deliv-dupkey.json", dup.encode("utf-8"))
    L.write_bytes(INP + "/dupkey.json", b'{"A": 1, "A": 2}\n')
    # SYN: delegation/handoff for the synthetic repository (HEAD = V0)
    v0 = build["Syn"]["V0"]
    syn_contract = {"Gate": "R3-G1", "Objective": "R3 synthetic task", "Invariants": [], "ExpectedEvidence": [],
                    "StopConditions": [], "RoleRequirements": [], "SupersededCommits": [], "RequiredTests": [],
                    "Authorities": [{"Path": L.AP_PATH, "Section": "## 16. Ejecución delegada", "Class": "EXTERNAL"}],
                    "AllowedWriteScope": ["docs/**"], "ForbiddenWriteScope": ["src/**"]}
    sdeleg = MD.delegation_v2(syn_contract, "R20261012T100000Z-d002", v0["Base"], "r3/head", L.SYN, v0["Main"], v0["Base"],
                              unit="R3", initiative="R3")
    shand = MD.handoff_v1("R20261012T101000Z-a002", sdeleg["RunId"], v0["Base"], None, v0["Main"], "r3/head", L.SYN,
                          [L.AP_PATH], "Co-Authored-By: R3 Synthetic Worker",
                          ["Revisados los Candidates del lote sintético."], "R3-G1", initiative="R3",
                          work_completed="Activación sintética R3.")
    errs = L.validate_schema(sdeleg, json.loads(L.schema_bytes("delegation.v2.schema.json").decode("utf-8")))
    errs += L.validate_schema(shand, json.loads(L.schema_bytes("worker-handoff.schema.json").decode("utf-8")))
    if errs:
        raise RuntimeError("syn inputs do not conform: %s" % errs[:5])
    L.write_json_file(INP + "/deleg-syn.json", sdeleg, pretty=True)
    L.write_json_file(INP + "/handoff-syn.json", shand, pretty=True)
    return {"K": K, "SynContract": syn_contract}


# ------------------------------------------------------------------ scenarios

def scenarios(build):
    dv, sy = build["Deliv"], build["Syn"]
    R, D, S = L.REAL, L.DELIV, L.SYN
    I = INP
    ign_real = "artifacts/orchestration/FX-U1/T1/0/R20261011T000000Z-e2e0/output.json"
    ign_deliv = "artifacts/orchestration/FX-U1/T1/0/R20261012T092000Z-c001/output.json"
    ign_syn = "artifacts/orchestration/R3/T1/0/R20261012T102000Z-c002/output.json"
    G, RD = dv["GreenSha"], dv["RedSha"]
    out = []
    # S1: exactly the previous e2e scenario (13 runs) on a fresh clone; facts level (R4 waived as before)
    out.append({"Name": "S1-REAL-E2E", "Repo": R, "Branch": L.BR, "Data": "real fixture (e2e scenario)", "Runs": [
        ("identity", ["identity", "--repo", R, "--branch", L.BR, "--base", L.BASE, "--current", L.HEADX, "--red", L.RED], True),
        ("diff", ["diff", "--repo", R, "--from", L.BASE, "--to", L.HEADX], True),
        ("scope", ["scope", "--repo", R, "--from", L.BASE, "--to", L.HEADX] + file_arg("DELEG", I + "/K.json"), True),
        ("contract", ["contract", "--repo", R] + file_arg("CONTRACT", I + "/K.json") + file_arg("DELEG", I + "/K.json"), True),
        ("hashes", ["hashes", "--repo", R, "--rev", L.TIP, "--paths", L.K, "src/Fixture.Lib/Calculator.cs"], True),
        ("cleantree", ["cleantree", "--repo", R, "--ignore-path", ign_real], True),
        ("read-text-1", ["read-text", "--repo", R, "--rev", L.TIP, "--path", L.DEC, "--lines", "1-1"], True),
        ("read-text-73", ["read-text", "--repo", R, "--rev", L.TIP, "--path", L.DEC, "--lines", "73-73"], True),
        ("read-json", ["read-json", "--repo", R, "--rev", L.TIP, "--path", L.K, "--pointer", "/Objective", "--pointer", "/Schema",
                       "--pointer", "/MainSha", "--pointer", "/AuthorityRevision"], True),
        ("trx", ["trx", "--repo", R] + file_arg("TRX", I + "/fixture-tests.trx") + ["--filter-substring", "Subtract"], True),
        ("handoff", ["handoff", "--repo", R] + file_arg("H", I + "/handoff-e2e.json") + ["--schema-rev", L.TIP], True),
        ("freetext", ["freetext", "--repo", R] + file_arg("H", I + "/handoff-e2e.json") + ["--terms-rev", L.MAIN], True),
        ("clausemap", ["clausemap", "--repo", R, "--main", L.MAIN, "--state-rev", L.BASE, "--state-path", L.STATE]
         + file_arg("CONTRACT", I + "/K.json") + ["--base", L.BASE], True)],
        "Verification": None})
    # S2: real fixture + simulated T1 delivery; decision level
    s2_runs = [
        ("identity", ["identity", "--repo", D, "--branch", L.BR, "--base", L.TIP, "--current", G, "--red", RD], True),
        ("diff", ["diff", "--repo", D, "--from", L.TIP, "--to", G], True),
        ("scope", ["scope", "--repo", D, "--from", L.TIP, "--to", G] + file_arg("DELEG", I + "/deleg-deliv.json"), True),
        ("contract", ["contract", "--repo", D] + file_arg("CONTRACT", I + "/K.json") + file_arg("DELEG", I + "/deleg-deliv.json"), True),
        ("hashes", ["hashes", "--repo", D, "--rev", G, "--paths"] + sorted(dv["Files"]), True),
        ("cleantree", ["cleantree", "--repo", D, "--ignore-path", ign_deliv], True),
        ("handoff", ["handoff", "--repo", D] + file_arg("H", I + "/handoff-deliv.json") + ["--schema-rev", G], True),
        ("freetext", ["freetext", "--repo", D] + file_arg("H", I + "/handoff-deliv.json") + ["--terms-rev", L.MAIN], True),
        ("clausemap", ["clausemap", "--repo", D, "--main", L.MAIN, "--state-rev", G, "--state-path", L.STATE]
         + file_arg("CONTRACT", I + "/K.json") + ["--base", L.TIP], True),
        ("trx", ["trx", "--repo", D] + file_arg("TRX", I + "/fixture-tests.trx") + ["--filter-substring", "Subtract"], True),
        ("read-text", ["read-text", "--repo", D, "--rev", G, "--path", L.DEC, "--lines", "1-1"], True)]
    deliv_ids = {"TaskId": "T1", "RunId": "R20261012T092000Z-c001", "DelegationRunId": "R20261012T090000Z-d001",
                 "WorkRunId": "R20261012T091000Z-a001", "Unit": "FX-U1"}
    out.append({"Name": "S2-REAL-DELIVERY", "Repo": D, "Branch": L.BR, "Data": "real fixture + simulated T1 delivery",
                "Runs": s2_runs,
                "Verification": {"Deleg": I + "/deleg-deliv.json", "Handoff": I + "/handoff-deliv.json", "IgnorePath": ign_deliv,
                                 "TermsRev": L.MAIN, "MainSha": L.MAIN, "Sources": None, "Contract": "K", "AR": L.AR,
                                 "Ids": deliv_ids, "Current": G}})
    out.append({"Name": "S2-NOI", "Repo": D, "Branch": L.BR, "Data": "real fixture + delivery; the tool started WITHOUT -I",
                "Runs": [("diff-noI", ["diff", "--repo", D, "--from", L.TIP, "--to", G], False)], "Verification": None})
    out.append({"Name": "S2-DUPKEY", "Repo": D, "Branch": L.BR, "Data": "real fixture + delivery; JSON inputs with a duplicated key",
                "Runs": [("scope-dupkey", ["scope", "--repo", D, "--from", L.TIP, "--to", G] + file_arg("DELEG", I + "/deleg-deliv-dupkey.json"), True),
                         ("read-json-dupkey", ["read-json", "--repo", D] + file_arg("N", I + "/dupkey.json") + ["--pointer", "/A"], True)],
                "Verification": None})
    # SYN-COMMON: V0 of the mini repository with every decision-level run
    v0 = sy["V0"]
    syn_ids = {"TaskId": "T1", "RunId": "R20261012T102000Z-c002", "DelegationRunId": "R20261012T100000Z-d002",
               "WorkRunId": "R20261012T101000Z-a002", "Unit": "R3"}
    common_runs = [
        ("identity", ["identity", "--repo", S, "--branch", "r3/head", "--base", v0["Base"], "--current", v0["Main"]], True),
        ("diff", ["diff", "--repo", S, "--from", v0["Base"], "--to", v0["Main"]], True),
        ("scope", ["scope", "--repo", S, "--from", v0["Base"], "--to", v0["Main"]] + file_arg("DELEG", I + "/deleg-syn.json"), True),
        ("cleantree", ["cleantree", "--repo", S, "--ignore-path", ign_syn], True),
        ("freetext", ["freetext", "--repo", S] + file_arg("H", I + "/handoff-syn.json") + ["--terms-rev", v0["Main"]], True),
        ("clausemap", ["clausemap", "--repo", S, "--main", v0["Main"]], True)]

    def syn_verif(main):
        return {"Deleg": I + "/deleg-syn.json", "Handoff": I + "/handoff-syn.json", "IgnorePath": ign_syn,
                "TermsRev": v0["Main"], "MainSha": main, "Sources": "SYN-COMMON", "Contract": "SYN", "AR": v0["Base"],
                "Ids": syn_ids, "Current": v0["Main"]}
    v = syn_verif(v0["Main"])
    v["Sources"] = None
    out.append({"Name": "SYN-COMMON", "Repo": S, "Branch": "r3/head", "Data": "synthetic mini repo, valid map (V0)",
                "Runs": common_runs, "Verification": v})
    for name, data, runs in (
            ("SYN-SA", "two first-parent merges, deletion under Surfaces, preamble change, heading inside a code fence",
             [("clausemap", ["clausemap", "--repo", S, "--main", sy["SA"]["Main"]], True)]),
            ("SYN-SA-STATE", "SA with a state holding two distinct claim_id values",
             [("clausemap", ["clausemap", "--repo", S, "--main", sy["SA"]["Main"], "--state-rev", sy["SA"]["StateRev"],
                             "--state-path", sy["SA"]["StatePath"]], True)]),
            ("SYN-SB", "trailer on the first-parent line; the only merge has both parents reaching it",
             [("clausemap", ["clausemap", "--repo", S, "--main", sy["SB"]["Main"]], True)]),
            ("SYN-SC", "map schema at EFF with an unsupported keyword (format)",
             [("clausemap", ["clausemap", "--repo", S, "--main", sy["SC"]["Main"]], True)]),
            ("SYN-SD", "clause map with a duplicated key",
             [("clausemap", ["clausemap", "--repo", S, "--main", sy["SD"]["Main"]], True)]),
            ("SYN-SE", "no exact trailer line; a merged commit quotes it inside a longer line",
             [("clausemap", ["clausemap", "--repo", S, "--main", sy["SE"]["Main"]], True)]),
            ("SYN-SF", "two exact trailer commits, each merged by its own first-parent merge",
             [("clausemap", ["clausemap", "--repo", S, "--main", sy["SF"]["Main"]], True)])):
        key = name.split("-")[1]
        out.append({"Name": name, "Repo": S, "Branch": "r3/head", "Data": "synthetic mini repo: " + data, "Runs": runs,
                    "Verification": syn_verif(sy[key]["Main"])})
    return out


# ------------------------------------------------------------------ closures

def closure_for(scn, cache={}):
    key = scn["Repo"] + "|" + scn["Name"]
    if key in cache:
        return cache[key]
    repo = scn["Repo"]
    extra = set()
    files = []
    for _n, argv, _i in scn["Runs"]:
        for opt in ("--path", "--ignore-path", "--state-path"):
            if opt in argv:
                extra.add(argv[argv.index(opt) + 1])
        if "--paths" in argv:
            k = argv.index("--paths") + 1
            while k < len(argv) and not argv[k].startswith("--"):
                extra.add(argv[k])
                k += 1
        for k, a in enumerate(argv):
            if a == "--sha256":
                n, _, h = argv[k + 1].partition("=")
                if {"Name": n, "Sha256": h} not in files:
                    files.append({"Name": n, "Sha256": h})
    for c in L.all_commits(repo):
        try:
            m = json.loads(L.git(repo, "show", c + ":" + L.MAP_PATH).decode("utf-8"))
            extra.update(x for x in m.get("Surfaces", []) if isinstance(x, str))
            extra.update(f.get("Path") for f in m.get("Files", []) if isinstance(f, dict) and isinstance(f.get("Path"), str))
        except Exception:
            pass
    try:
        k = json.loads(L.read_bytes(INP + "/K.json").decode("utf-8"))
        extra.update(a["Path"] for a in k["Authorities"])
    except Exception:
        pass
    cl = L.broad_closure(repo, sorted(extra), files, scn["Branch"])
    path = A + "/closures/" + scn["Name"] + ".json"
    sha = L.write_json_file(path, cl)
    cache[key] = (path, sha)
    return cache[key]


# ------------------------------------------------------------------ stages

def env_path(variant, scn, run):
    return "%s/env/%s/%s/%s.json" % (A, variant, scn, run)


def produce_envelopes(variants, scns):
    for vid, tool in variants:
        t0 = time.time()
        for scn in scns:
            for rname, argv, iso in scn["Runs"]:
                p = env_path(vid, scn["Name"], rname)
                if os.path.exists(p) and not FRESH:
                    continue
                rc, out = L.run_tool(tool, argv, isolated=iso, cwd=scn["Repo"])
                L.write_bytes(p, out)
                L.write_bytes(p + ".rc", str(rc).encode("ascii"))
        L.log("envelopes %s %.1fs" % (vid, time.time() - t0))


def load_env(vid, scn, run):
    return json.loads(L.read_bytes(env_path(vid, scn, run)).decode("utf-8"))


def controller_for(vid, scn, ctx):
    v = scn["Verification"]
    deleg = jload(v["Deleg"])
    hand = jload(v["Handoff"])
    envs = {}
    src = scn["Name"] if v["Sources"] is None else v["Sources"]
    src_scn = [s for s in ctx["Scenarios"] if s["Name"] == src][0]
    for rname, _a, _i in src_scn["Runs"]:
        envs[rname] = load_env(vid, src, rname)
    if v["Sources"] is not None:
        envs["clausemap"] = load_env(vid, scn["Name"], "clausemap")
    res = MD.results_from_facts(envs, deleg, hand)
    contract_auth = ctx["K"]["Authorities"] if v["Contract"] == "K" else ctx["SynContract"]["Authorities"]
    ar_rows = MD.authority_resolution(MD.facts(envs.get("clausemap")), contract_auth, v["MainSha"], v["AR"])
    cv = MD.controller_verification(res, v["Ids"], v["Current"], ar_rows, "facts of tool variant " + vid)
    errs = L.validate_schema(cv, ctx["CvSchema"])
    if errs:
        raise RuntimeError("model controller output does not conform: %s" % errs[:5])
    path = "%s/cv/%s/%s.json" % (A, vid, scn["Name"])
    sha = L.write_json_file(path, cv, pretty=True)
    return path, sha, dict((k, res[k][0]) for k in MD.R4_SET)


def no_data_waivers(scn, has_ar_rows):
    """R5/R6/R7 are declared not applicable only when the scenario has no run of their commands (no data)."""
    cmds = set(a[0] for _n, a, _i in scn["Runs"])
    na = {}
    if not cmds & {"identity", "diff", "scope", "hashes"}:
        na["R5"] = "no identity/diff/scope/hashes run in this scenario"
    if "clausemap" not in cmds and not has_ar_rows:
        na["R6"] = "no clausemap run and no AuthorityResolution in this scenario"
    if not cmds & {"scope", "contract", "clausemap"}:
        na["R7"] = "no scope/contract/clausemap run in this scenario"
    return na


def request(scn, vid, tool_path, tool_blob, cfg, ctx):
    runs = [{"Name": r, "Envelope": env_path(vid, scn["Name"], r), "Argv": a} for r, a, _i in scn["Runs"]]
    cpath, csha = closure_for(scn)
    req = {"Schema": "rackcad-bmin-accept-request/0-experimental", "Repo": scn["Repo"],
           "Tool": {"Path": tool_path, "PinnedBlob": tool_blob, "RunFrom": "path"},
           "Interpreter": ctx["Interp"], "Runs": runs, "Closure": {"Path": cpath, "Sha256": csha},
           "RelayFacts": {"Path": INP + "/relay.json", "CitedSha256": L.sha256_file(INP + "/relay.json")}}
    ctl = None
    if scn["Verification"] is not None and cfg != "MF":
        v = scn["Verification"]
        cvp, cvs, ctl = controller_for(vid, scn, ctx)
        req["Verification"] = {"Controller": {"Path": cvp, "Sha256": cvs},
                               "Delegation": {"Path": v["Deleg"], "Sha256": L.sha256_file(v["Deleg"])},
                               "Handoff": {"Path": v["Handoff"], "Sha256": L.sha256_file(v["Handoff"])},
                               "IgnorePath": v["IgnorePath"], "TermsRev": v["TermsRev"], "MainSha": v["MainSha"]}
    else:
        req["NotApplicable"] = {"R4": "facts-level request: no controller-verification in this request (part (a))"}
    na = no_data_waivers(scn, ctl is not None and bool(jload(req["Verification"]["Controller"]["Path"]).get("AuthorityResolution")))
    if na:
        req.setdefault("NotApplicable", {}).update(na)
    path = "%s/req/%s/%s/%s.json" % (A, cfg, vid, scn["Name"])
    L.write_json_file(path, req)
    return path, ctl


def judge(scn, vid, tool_path, tool_blob, cfg, ctx):
    rpath = "%s/acc/%s/%s/%s.json" % (A, cfg, vid, scn["Name"])
    req, ctl = request(scn, vid, tool_path, tool_blob, cfg, ctx)
    if os.path.exists(rpath) and not FRESH:
        res = jload(rpath)
        rc = res.pop("_rc")
    else:
        rc, res = L.run_acceptor(req)
        res2 = dict(res)
        res2["_rc"] = rc
        L.write_json_file(rpath, res2)
    s = L.acceptor_summary(rc, res)
    s["Fields"] = L.mismatch_fields(res)
    if ctl is not None:
        s["ModelControllerResults"] = ctl
    s["Request"] = os.path.relpath(req, L.WORK).replace("\\", "/")
    return s


# ------------------------------------------------------------------ unit probes and R9 injection

PROBE = r'''
import importlib.util, json, sys
spec = importlib.util.spec_from_file_location("m", sys.argv[1]); m = importlib.util.module_from_spec(spec); spec.loader.exec_module(m)
out = {}
for case, args in (("subcommand-outside-list", ["version"]), ("status-without-porcelain", ["status", "--short"]),
                    ("diff-forced-flags", ["diff", "--name-only", "HEAD", "HEAD"])):
    ctx = m.Ctx(sys.argv[2])
    try:
        rc, data = m.git_exec(ctx, args)
        out[case] = {"Rc": rc, "GitCalls": ctx.git_calls}
    except m.FactsError as e:
        out[case] = {"Error": e.code, "GitCalls": ctx.git_calls}
sys.stdout.write(json.dumps(out, sort_keys=True))
'''


def unit_probes(variants):
    os.makedirs(A + "/probe", exist_ok=True)
    pf = A + "/probe/probe.py"
    L.write_bytes(pf, PROBE.encode("ascii"))
    out = {}
    for vid, tool in variants:
        if vid not in ("PINNED", "M15", "M23", "M26"):
            continue
        p = L.subprocess.run([sys.executable, "-I", pf, tool, L.DELIV], stdout=L.subprocess.PIPE, stderr=L.subprocess.PIPE,
                             env=L.clean_env(), cwd=L.WORK)
        out[vid] = json.loads(p.stdout.decode("ascii"))
    return out


def r9_injection(ctx):
    """The acceptor's R9 detector for the classes of M15/M23/M26, fed through an echo tool (R1/R3 MATCH)."""
    base = load_env("PINNED", "S2-REAL-DELIVERY", "diff")
    argv = [a for (n, a, _i) in [r for r in ctx["ScnByName"]["S2-REAL-DELIVERY"]["Runs"]] if n == "diff"][0]
    cases = {"injected-subcommand-outside-list": ["-c", "core.quotepath=false", "-c", "core.fsmonitor=false", "version"],
             "injected-status-without-porcelain": ["-c", "core.quotepath=false", "-c", "core.fsmonitor=false", "status", "--short"],
             "injected-diff-without-no-ext-diff": ["-c", "core.quotepath=false", "-c", "core.fsmonitor=false", "diff",
                                                   "--name-only", "--no-renames", "-z", L.TIP + ".." + ctx["Build"]["Deliv"]["GreenSha"]],
             "control-no-injection": None}
    out = {}
    for case, extra in cases.items():
        env = json.loads(json.dumps(base))
        if extra is not None:
            env["GitCalls"] = env["GitCalls"] + [extra]
        tool = MD.write_echo_tool(A + "/echo/" + case + "/bmin_facts.py", env)
        rc, raw = L.run_tool(tool, argv, cwd=L.DELIV)
        envp = A + "/echo/" + case + "/envelope.json"
        L.write_bytes(envp, raw)
        cl = closure_for(ctx["ScnByName"]["S2-REAL-DELIVERY"])
        req = {"Schema": "rackcad-bmin-accept-request/0-experimental", "Repo": L.DELIV,
               "Tool": {"Path": tool, "PinnedBlob": L.blob_id(L.read_bytes(tool)), "RunFrom": "path"},
               "Interpreter": ctx["Interp"], "Runs": [{"Name": "diff", "Envelope": envp, "Argv": argv}],
               "Closure": {"Path": cl[0], "Sha256": cl[1]},
               "RelayFacts": {"Path": INP + "/relay.json", "CitedSha256": L.sha256_file(INP + "/relay.json")},
               "NotApplicable": {"R4": "envelope injection check of R9 only",
                                 "R6": "no clausemap run in the injection request",
                                 "R7": "no scope/contract/clausemap run in the injection request"}}
        rp = A + "/echo/" + case + "/request.json"
        L.write_json_file(rp, req)
        arc, res = L.run_acceptor(rp)
        s = L.acceptor_summary(arc, res)
        s["Fields"] = L.mismatch_fields(res)
        out[case] = s
    return out


# ------------------------------------------------------------------ main

def main():
    t0 = time.time()
    if FRESH and os.path.isdir(A):
        shutil.rmtree(A)
    build = json.loads(L.read_bytes(L.WORK + "/build.json").decode("ascii"))
    ctx = build_inputs(build)
    ctx["Build"] = build
    ctx["Interp"] = {"Python": L.platform.python_version(), "ExecutableSha256": L.exe_sha256()}
    ctx["CvSchema"] = json.loads(L.schema_bytes("controller-verification.v2.schema.json").decode("utf-8"))
    scns = scenarios(build)
    ctx["Scenarios"] = scns
    ctx["ScnByName"] = dict((s["Name"], s) for s in scns)
    muts = build["Mutants"]
    only = os.environ.get("R3_ONLY")
    if only:
        muts = [m for m in muts if m["Id"] in only.split(",")]
    variants = [("PINNED", L.TOOL)] + [(m["Id"], m["Path"]) for m in muts]
    produce_envelopes(variants, scns)
    # baseline
    baseline = {}
    for scn in scns:
        baseline[scn["Name"]] = judge(scn, "PINNED", L.TOOL, L.TOOL_BLOB, "baseline", ctx)
        L.log("baseline", scn["Name"], baseline[scn["Name"]]["Decision"], baseline[scn["Name"]]["Mismatch"])
    rows = []
    for m in muts:
        mid = m["Id"]
        acts, dets = [], []
        for scn in scns:
            diffs = {}
            paths = {}
            for rname, _a, _i in scn["Runs"]:
                pa = L.read_bytes(env_path("PINNED", scn["Name"], rname))
                pb = L.read_bytes(env_path(mid, scn["Name"], rname))
                d = L.env_diff(pa, pb)
                if d:
                    diffs[rname] = d
                    paths[rname] = L.facts_paths_diff(pa, pb)
            if not diffs:
                continue
            acts.append({"Scenario": scn["Name"], "Data": scn["Data"], "RunsChanged": diffs, "FactsChanged": paths})
            jm = judge(scn, mid, m["Path"], m["Blob"], "M", ctx)
            jp = judge(scn, mid, L.TOOL, L.TOOL_BLOB, "P", ctx)
            base = baseline[scn["Name"]]
            for j in (jm, jp):
                # attributable = MISMATCH that the baseline (pinned tool, same scenario) does not show with the same fields
                j["Attributable"] = [c for c in j["Mismatch"] if c not in base["Mismatch"]
                                     or j["Fields"].get(c) != base["Fields"].get(c)]
                j["SameAsBaseline"] = [c for c in j["Mismatch"] if c not in j["Attributable"]]
            det = {"Scenario": scn["Name"], "BaselineDecision": base["Decision"], "BaselineMismatch": base["Mismatch"],
                   "ConfigM": jm, "ConfigP": jp}
            if scn["Verification"] is not None and jm["Attributable"] and set(jm["Attributable"]) <= {"R4"}:
                # caught only through the decision: the same request at facts level (R4 waived, as in the old e2e)
                jf = judge(scn, mid, m["Path"], m["Blob"], "MF", ctx)
                det["ConfigMFactsLevel"] = jf
                L.log(mid, scn["Name"], "MF (facts level):", jf["Decision"], jf["Mismatch"])
            dets.append(det)
            L.log(mid, scn["Name"], "M:", jm["Decision"], jm["Attributable"], "P:", jp["Decision"], jp["Attributable"])
        detected_by = sorted(set(c for d in dets for c in d["ConfigM"]["Attributable"]), key=lambda x: int(x[1:]))
        not_det = [d["Scenario"] for d in dets if not d["ConfigM"]["Attributable"]]
        facts_level_accept = [d["Scenario"] for d in dets if d.get("ConfigMFactsLevel", {}).get("Decision") == "ACCEPT_FACTS"]
        rows.append({"Id": mid, "Description": m["Description"], "CriticalFact": CRITICAL[mid], "MutantBlob": m["Blob"],
                     "AnchorLine": m["AnchorLine"], "Activated": bool(acts), "Activations": acts, "Judgements": dets,
                     "DetectedByConfigM": detected_by if acts else None,
                     "ActivationsWithoutAttributableDetection": not_det,
                     "AcceptedAtFactsLevel": facts_level_accept,
                     "DetectedOnSomeActivation": bool(detected_by)})
    probes = unit_probes(variants)
    inj = r9_injection(ctx)
    summary = {
        "Activated": sorted(r["Id"] for r in rows if r["Activated"]),
        "NotActivated": sorted(r["Id"] for r in rows if not r["Activated"]),
        "ActivatedAndDetectedConfigM": sorted(r["Id"] for r in rows if r["DetectedOnSomeActivation"]),
        "ActivatedNotDetectedConfigM": sorted(r["Id"] for r in rows if r["Activated"] and not r["DetectedOnSomeActivation"]),
        "ActivationsWithoutAttributableDetection": dict((r["Id"], r["ActivationsWithoutAttributableDetection"]) for r in rows
                                                        if r["ActivationsWithoutAttributableDetection"]),
        "DetectedOnlyByR4": sorted(r["Id"] for r in rows if r["DetectedByConfigM"] == ["R4"]),
        "AcceptedAtFactsLevel": dict((r["Id"], r["AcceptedAtFactsLevel"]) for r in rows if r["AcceptedAtFactsLevel"]),
        "DetectorsConfigM": dict((r["Id"], r["DetectedByConfigM"]) for r in rows if r["Activated"]),
        "Seconds": round(time.time() - t0, 1)}
    res = {"Schema": "r3-part-a/1", "Header": L.common_header(),
           "Scenarios": [{"Name": s["Name"], "Data": s["Data"], "Repo": s["Repo"], "Runs": [[n, a, i] for n, a, i in s["Runs"]],
                          "DecisionLevel": s["Verification"] is not None} for s in scns],
           "Baseline": baseline, "Mutants": rows, "UnitProbes": probes, "R9Injection": inj, "Summary": summary}
    p, sha = L.write_result("a-mutation.json", res)
    L.log("written", p, sha, summary)


if __name__ == "__main__":
    main()
