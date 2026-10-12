# part_c.py - (c) R4 (facts -> decision) exercised on the real scenario (run: python -I part_c.py).
#
# Two real-data scenarios, every request WITHOUT any NotApplicable declaration:
#   C-DELIV: real fixture + simulated T1 delivery (Q0 = fixture tip, RED, GREEN pushed locally): most checks pass.
#   C-REAL : the previous e2e commits exactly as they are in the fixture (HEAD != CurrentSha, foreign trailer, a 16.10
#            term in the free text): most checks fail.
# For each: a CONFORMING controller-verification/v2 (schema-valid, coherent, Results derived by the model Controller
# from the pinned tool's facts), six ALTERED outputs (each R4 Result flipped, Classification/Disposition/FailureClass
# recomputed so the output stays coherent), six not_run outputs, one with a wrong AuthorityResolution.EffectiveSha,
# and (C-DELIV) a genuine CleanTree failure (untracked file) with its conforming and flipped outputs.
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
PA = _load("part_a")
C = L.RUNS + "/c"
PA.A = C
PA.INP = C + "/inputs"
INP = PA.INP


def real_inputs(ctx):
    K = ctx["K"]
    deleg = MD.delegation_v2(K, "R20261012T110000Z-d003", L.BASE, L.BR, L.REAL, L.MAIN, L.AR)
    hand = MD.handoff_v1("R20261012T111000Z-a003", deleg["RunId"], L.BASE, L.RED, L.HEADX, L.BR, L.REAL,
                         ["src/Fixture.Lib/Calculator.cs"], "Co-Authored-By: synthetic",
                         ["GATE PASS (término de 16.10 a propósito, para el hecho de freetext)"], K["Gate"])
    errs = L.validate_schema(deleg, json.loads(L.schema_bytes("delegation.v2.schema.json").decode("utf-8")))
    errs += L.validate_schema(hand, json.loads(L.schema_bytes("worker-handoff.schema.json").decode("utf-8")))
    if errs:
        raise RuntimeError(errs[:5])
    L.write_json_file(INP + "/deleg-real.json", deleg, pretty=True)
    L.write_json_file(INP + "/handoff-real.json", hand, pretty=True)


def scen_real():
    R, I = L.REAL, INP
    ign = "artifacts/orchestration/FX-U1/T1/0/R20261012T112000Z-c003/output.json"
    fa = PA.file_arg
    return {"Name": "C-REAL", "Repo": R, "Branch": L.BR, "Data": "real fixture, e2e commits as they are", "Runs": [
        ("identity", ["identity", "--repo", R, "--branch", L.BR, "--base", L.BASE, "--current", L.HEADX, "--red", L.RED], True),
        ("diff", ["diff", "--repo", R, "--from", L.BASE, "--to", L.HEADX], True),
        ("scope", ["scope", "--repo", R, "--from", L.BASE, "--to", L.HEADX] + fa("DELEG", I + "/deleg-real.json"), True),
        ("contract", ["contract", "--repo", R] + fa("CONTRACT", I + "/K.json") + fa("DELEG", I + "/deleg-real.json"), True),
        ("cleantree", ["cleantree", "--repo", R, "--ignore-path", ign], True),
        ("handoff", ["handoff", "--repo", R] + fa("H", I + "/handoff-real.json") + ["--schema-rev", L.TIP], True),
        ("freetext", ["freetext", "--repo", R] + fa("H", I + "/handoff-real.json") + ["--terms-rev", L.MAIN], True),
        ("clausemap", ["clausemap", "--repo", R, "--main", L.MAIN, "--state-rev", L.BASE, "--state-path", L.STATE]
         + fa("CONTRACT", I + "/K.json") + ["--base", L.BASE], True)],
        "Verification": {"Deleg": I + "/deleg-real.json", "Handoff": I + "/handoff-real.json", "IgnorePath": ign,
                         "TermsRev": L.MAIN, "MainSha": L.MAIN, "Sources": None, "Contract": "K", "AR": L.AR,
                         "Ids": {"TaskId": "T1", "RunId": "R20261012T112000Z-c003", "DelegationRunId": "R20261012T110000Z-d003",
                                 "WorkRunId": "R20261012T111000Z-a003", "Unit": "FX-U1"}, "Current": L.HEADX}}


def envelopes(scn, tag):
    out = {}
    for rname, argv, iso in scn["Runs"]:
        rc, raw = L.run_tool(L.TOOL, argv, isolated=iso, cwd=scn["Repo"])
        p = "%s/env/%s/%s.json" % (C, tag, rname)
        L.write_bytes(p, raw)
        out[rname] = (p, json.loads(raw.decode("ascii")))
    return out


def run_case(scn, tag, envs, cv, label, ctx):
    cvp = "%s/cv/%s/%s.json" % (C, tag, label)
    errs = L.validate_schema(cv, ctx["CvSchema"])
    cvs = L.write_json_file(cvp, cv, pretty=True)
    v = scn["Verification"]
    cpath, csha = PA.closure_for(dict(scn, Name=tag))
    req = {"Schema": "rackcad-bmin-accept-request/0-experimental", "Repo": scn["Repo"],
           "Tool": {"Path": L.TOOL, "PinnedBlob": L.TOOL_BLOB, "RunFrom": "path"}, "Interpreter": ctx["Interp"],
           "Runs": [{"Name": r, "Envelope": envs[r][0], "Argv": a} for r, a, _i in scn["Runs"]],
           "Closure": {"Path": cpath, "Sha256": csha},
           "RelayFacts": {"Path": INP + "/relay.json", "CitedSha256": L.sha256_file(INP + "/relay.json")},
           "Verification": {"Controller": {"Path": cvp, "Sha256": cvs},
                            "Delegation": {"Path": v["Deleg"], "Sha256": L.sha256_file(v["Deleg"])},
                            "Handoff": {"Path": v["Handoff"], "Sha256": L.sha256_file(v["Handoff"])},
                            "IgnorePath": v["IgnorePath"], "TermsRev": v["TermsRev"], "MainSha": v["MainSha"]}}
    assert "NotApplicable" not in req
    rp = "%s/req/%s/%s.json" % (C, tag, label)
    L.write_json_file(rp, req)
    rc, res = L.run_acceptor(rp)
    s = L.acceptor_summary(rc, res)
    r4 = [c for c in res.get("Checks", []) if c.get("Id") == "R4"][0]
    rows = dict((row["Check"], {"Controller": row.get("Controller"), "AcceptorExpects": row.get("AcceptorExpects"),
                                "Status": row.get("Status"), "Reasons": row.get("Reasons"), "Defects": row.get("Defects")})
                for row in (r4.get("Detail") or {}).get("Checks", []))
    r6 = [c for c in res.get("Checks", []) if c.get("Id") == "R6"][0]
    return {"Label": label, "CvSchemaErrors": errs, "Classification": cv["Classification"], "Disposition": cv["Disposition"],
            "FailureClass": cv["FailureClass"], "Results": dict((k, cv["Checks"][k]["Result"]) for k in MD.R4_SET),
            "Decision": s["Decision"], "Exit": s["Exit"], "Status": s["Status"], "R4Status": r4.get("Status"),
            "R4Rows": rows, "R6AuthorityResolution": (r6.get("Detail") or {}).get("AuthorityResolution"),
            "R4NotApplicableDeclared": False}


def exercise(scn, tag, ctx, only=MD.R4_SET):
    envs = envelopes(scn, tag)
    deleg = PA.jload(scn["Verification"]["Deleg"])
    hand = PA.jload(scn["Verification"]["Handoff"])
    res = MD.results_from_facts(dict((k, v[1]) for k, v in envs.items()), deleg, hand)
    v = scn["Verification"]
    ar_rows = MD.authority_resolution(MD.facts(envs["clausemap"][1]), ctx["K"]["Authorities"], v["MainSha"], v["AR"])
    cv = MD.controller_verification(res, v["Ids"], v["Current"], ar_rows, "pinned tool facts, " + tag)
    cases = [run_case(scn, tag, envs, cv, "conforming", ctx)]
    L.log(tag, "conforming", cases[-1]["Decision"], cases[-1]["R4Status"], cases[-1]["Results"])
    for chk in only:
        cur = cv["Checks"][chk]["Result"]
        alt = "fail" if cur == "pass" else "pass"
        cases.append(run_case(scn, tag, envs, MD.flip(cv, chk, alt), "flip-%s-%s" % (chk, alt), ctx))
        L.log(tag, cases[-1]["Label"], cases[-1]["R4Status"], cases[-1]["R4Rows"][chk]["Status"])
        if cur != "not_run":
            cases.append(run_case(scn, tag, envs, MD.flip(cv, chk, "not_run"), "notrun-%s" % chk, ctx))
            L.log(tag, cases[-1]["Label"], cases[-1]["R4Status"], cases[-1]["R4Rows"][chk]["Status"])
    if ar_rows and only == MD.R4_SET:
        bad = json.loads(json.dumps(cv))
        bad["AuthorityResolution"][0]["EffectiveSha"] = L.TIP
        cases.append(run_case(scn, tag, envs, bad, "authorityresolution-effectivesha-wrong", ctx))
        L.log(tag, cases[-1]["Label"], cases[-1]["Decision"], cases[-1]["Status"].get("R6"))
    return {"Scenario": tag, "Data": scn["Data"], "ConformingResults": dict((k, res[k][0]) for k in MD.R4_SET),
            "Cases": cases}


def main():
    t0 = time.time()
    build = json.loads(L.read_bytes(L.WORK + "/build.json").decode("ascii"))
    ctx = PA.build_inputs(build)
    real_inputs(ctx)
    ctx["Interp"] = {"Python": L.platform.python_version(), "ExecutableSha256": L.exe_sha256()}
    ctx["CvSchema"] = json.loads(L.schema_bytes("controller-verification.v2.schema.json").decode("utf-8"))
    s2 = [s for s in PA.scenarios(build) if s["Name"] == "S2-REAL-DELIVERY"][0]
    out = [exercise(dict(s2, Name="C-DELIV"), "C-DELIV", ctx), exercise(scen_real(), "C-REAL", ctx)]
    # genuine CleanTree failure on the delivery: an untracked file in the worktree (removed afterwards)
    junk = L.DELIV + "/r3-untracked.txt"
    L.write_bytes(junk, b"untracked file of the R3 CleanTree case\n")
    try:
        out.append(exercise(dict(s2, Name="C-DELIV-DIRTY"), "C-DELIV-DIRTY", ctx, only=("CleanTree",)))
    finally:
        os.remove(junk)
    assert L.git(L.DELIV, "status", "--porcelain") == b""
    flips = [c for s in out for c in s["Cases"] if c["Label"].startswith("flip-")]
    notrun = [c for s in out for c in s["Cases"] if c["Label"].startswith("notrun-")]
    conf = [c for s in out for c in s["Cases"] if c["Label"] == "conforming"]
    summary = {
        "ConformingAccepted": dict((s["Scenario"], [c for c in s["Cases"] if c["Label"] == "conforming"][0]["Decision"]) for s in out),
        "ConformingR4": dict((s["Scenario"], [c for c in s["Cases"] if c["Label"] == "conforming"][0]["R4Status"]) for s in out),
        "FlipsRejectedByR4": "%d/%d" % (sum(1 for c in flips if c["R4Status"] == "MISMATCH"), len(flips)),
        "FlipsWhereOnlyTheFlippedRowMismatches": "%d/%d" % (
            sum(1 for c in flips if [k for k, r in c["R4Rows"].items() if r["Status"] == "MISMATCH"] == [c["Label"].split("-")[1]]),
            len(flips)),
        "NotRun": dict((s["Scenario"] + ":" + c["Label"], {"R4": c["R4Status"],
                        "AcceptorExpects": c["R4Rows"][c["Label"].split("-")[1]]["AcceptorExpects"]})
                       for s in out for c in s["Cases"] if c["Label"].startswith("notrun-")),
        "AnyNotApplicableDeclared": False, "AllCvSchemaValid": all(not c["CvSchemaErrors"] for s in out for c in s["Cases"]),
        "Seconds": round(time.time() - t0, 1)}
    r4_inputs = [
        {"Input": "Verification.Controller {Path, Sha256}", "Built": "model Controller output (r3model.py) from the pinned tool facts; "
         "schema controller-verification.v2 validated by the harness; Results flipped / not_run for the altered cases"},
        {"Input": "Verification.Delegation {Path, Sha256}", "Built": "rackcad-delegation/v2 instance: scopes, authorities, invariants, "
         "tests, stop conditions and role requirements copied from the custodied contract FX-U1-T1 (blob 628d89af); BaseSha = Q0 "
         "(fixture tip) or the e2e BASE; ExpectedBranch fx/u1; ExpectedWorktree = the clone; schema-validated"},
        {"Input": "Verification.Handoff {Path, Sha256}", "Built": "rackcad-worker-handoff/v1 instance: BaseSha/RedSha/CurrentSha/"
         "Branch/Worktree of the scenario, Worker.Trailer = the declared Co-Authored-By, the seven free-text fields; schema-validated"},
        {"Input": "Verification.IgnorePath", "Built": "artifacts/orchestration/FX-U1/T1/0/<VERIFY RunId>/output.json (kit run_dir)"},
        {"Input": "Verification.TermsRev", "Built": "MainSha fbe25347 (AP with 16.10 terms)"},
        {"Input": "Verification.MainSha", "Built": "origin/main of the fixture, fbe25347 (Derive input of the map part)"},
        {"Input": "Verification.DeclaredCoAuthoredBy (optional)", "Built": "not supplied: R4 reads handoff.Worker.Trailer"},
        {"Input": "Repo (shared with R5-R9)", "Built": "the clone; R4 recomputes Identity/Scope/CleanTree/Trailer from it"},
        {"Input": "RelayFacts {Path, CitedSha256} (R10, not R4)", "Built": "synthetic relay facts file; R10 is not an R4 input"},
        {"Input": "Controller AuthorityResolution (R6, not R4)", "Built": "rows from the contract citations, EffectiveSha/ClauseMapBlob "
         "from the clausemap facts; checked by R6 against the acceptor's own Derive at Verification.MainSha"}]
    res = {"Schema": "r3-part-c/1", "Header": L.common_header(), "Scenarios": out, "R4Inputs": r4_inputs, "Summary": summary}
    p, sha = L.write_result("c-r4.json", res)
    L.log("written", p, sha, json.dumps(summary)[:2000])


if __name__ == "__main__":
    main()
