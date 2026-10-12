# part_d.py - (d) mechanical derivation of the acceptor closure (rackcad-bmin-closure/0-experimental) from an
# EffectiveInputClosure (rackcad-input-closure/v1) for an FX-02 VERIFY invocation (run: python -I part_d.py).
#
# 1. builds a realistic input-closure/v1 for VERIFY of T1 on the delivery scenario (kit controller-contracts.md 2.1-2.2,
#    AP 16.24, RAE 15.2), schema-validated by the harness;
# 2. derives the closure with the table DERIVATION (one rule per field, no judgment), function derive_closure();
# 3. runs the PINNED acceptor: the VERIFY tool runs inside the closure (R9 MATCH expected), runs that read outside it
#    (R9 MISMATCH expected), and an I6-strict variant of the EIC (every blob = the one at AuthorityRevision).
import importlib.util
import json
import os
import re
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
DD = L.RUNS + "/d"
PA.A = DD
PA.INP = DD + "/inputs"
INP = PA.INP
D = L.DELIV

DERIVATION = [
    ("Commits", "C1",
     "{E.AuthorityRevision} U {D.BaseSha, D.MainSha, D.ChainBaseSha, D.ChainRedSha (if not null)} U {H.BaseSha, H.CurrentSha, "
     "H.RedSha (if not null)} U {I.Target.Commit} U {K.AuthorityRevision, K.MainSha} U RevList(D.BaseSha..H.CurrentSha) U "
     "FirstParentChain(D.MainSha) U Parents(m) for every m in FirstParentChain(D.MainSha) with two or more parents U "
     "{MergeBase(D.MainSha, K.AuthorityRevision)}",
     "deterministic, but NOT a lookup in input-closure/v1: it reads the delegation (D), handoff (H), contract (K) and role "
     "invocation (I) bytes pinned by E, and three Git graph primitives (rev-list range, first-parent walk, merge-base). "
     "The first-parent walk is a superset of every EFF/EFF^1/EFF^2 candidate, so it never depends on a 16.14 Derive "
     "implementation (no common mode with the fact under check)."),
    ("Refs", "C2", "{HEAD, refs/remotes/origin/<D.ExpectedBranch>, refs/remotes/origin/main}",
     "fixed by AP 16.9 #5 (Identity) and A6' plus D.ExpectedBranch; input-closure/v1 has no field for refs."),
    ("RevPaths", "C3",
     "{(c, p) : c in Commits, (p, b) in E.CanonicalInputs U E.AllowedTransitiveInputs U E.AutomaticInstructions, "
     "Blob(c:p) = b}  (blob identity: a revision may be read at p only if it holds exactly a blob that E pins for p)",
     "mechanical given Commits. Admits a read only for blob versions E lists; with RAE I6 read literally (every blob = the "
     "one at AuthorityRevision) the VERIFY reads of the delivered files at the GREEN commit fall outside (shown below)."),
    ("Paths", "C4",
     "{p : (p, _) in E.CanonicalInputs U E.AllowedTransitiveInputs U E.AutomaticInstructions} U {o.Target : o in "
     "E.Obligations, o.Class = READ, o.Target not null} U Surfaces(M) U {f.Path : f in Files(M)} U {a.Path : a in "
     "K.Authorities} U {IgnorePath(I)}, with M = the clause map whose blob E pins at its fixed path and IgnorePath(I) = "
     "artifacts/orchestration/<I.UnitId>/<I.TaskId>/<Attempt>/<I.RunId>/output.json (kit run_dir)",
     "mechanical; Surfaces and Files of M come from the content of a blob E pins (directories such as docs/adr/ and "
     "paths absent at EFF cannot be E entries, which need a Blob); the check-ignore path is the invocation's own output "
     "path, not an input, so it comes from I, not from E."),
    ("Objects", "C5", "{}  (trees of Commits and blobs of RevPaths are already implicit in the acceptor)", "mechanical."),
    ("Files", "C6",
     "{(Name(p), SHA256(bytes(p, b))) : (p, b) in E.CanonicalInputs, Name(p) defined}; Name: the contract path -> CONTRACT; "
     "artifacts/orchestration/<unit>/<task>/<attempt>/<D.RunId>/output.json -> DELEG; .../<H.RunId>/worker-handoff.json -> H; "
     "*.trx -> TRX; bytes(p, b) = the custodied bytes whose Git blob id is b (git cat-file blob b, or the custodied file at p)",
     "mechanical once the Name table is fixed; the --file NAME binding is a convention of the tool CLI that "
     "input-closure/v1 does not carry."),
]

NOT_MECHANICAL = [
    "Commits (C1): input-closure/v1 has only AuthorityRevision; the revisions come from the delegation, handoff, contract "
    "and role-invocation bytes (pinned by E) plus three Git graph primitives.",
    "Refs (C2): not representable in input-closure/v1; fixed by the norm (16.9 #5) and D.ExpectedBranch.",
    "Files names (C6): a fixed table path-role -> --file NAME; not in input-closure/v1.",
    "IgnorePath (C4): the invocation's own output path (role invocation RunId + kit run_dir), not an input.",
    "RevPaths (C3) versus RAE I6: if every E blob must equal the blob at AuthorityRevision, the GREEN blobs, the window's "
    "decision/state records and the custodied contract (absent at AuthorityRevision) cannot be listed, and legitimate "
    "VERIFY reads fall outside; resolving it is a normative decision, not a derivation step.",
]


def file_arg(name, path):
    return PA.file_arg(name, path)


def blob_at(c, p):
    try:
        return L.gits(D, "rev-parse", "--verify", "--quiet", c + ":" + p)
    except RuntimeError:
        return None


def build_eic(ctx, build, strict_i6=False):
    dv = build["Deliv"]
    G = dv["GreenSha"]
    K = ctx["K"]
    deleg = PA.jload(INP + "/deleg-deliv.json")
    hand = PA.jload(INP + "/handoff-deliv.json")
    ap_b = blob_at(L.AR, L.AP_PATH)
    agents_b = blob_at(L.AR, "AGENTS.md")
    delegp = "artifacts/orchestration/FX-U1/T1/0/%s/output.json" % deleg["RunId"]
    handp = "artifacts/orchestration/FX-U1/T1/0/%s/worker-handoff.json" % hand["RunId"]
    trxp = "artifacts/ci/R37949808974/fixture-tests.trx"
    canon_in = [{"Path": L.K, "Blob": blob_at(L.TIP, L.K)}]
    for a in K["Authorities"]:                       # the contract's authorities at AuthorityRevision (I6)
        b = blob_at(L.AR, a["Path"])
        if b and {"Path": a["Path"], "Blob": b} not in canon_in:
            canon_in.append({"Path": a["Path"], "Blob": b})
    if not strict_i6:
        for p in sorted(dv["Files"]):               # the object under verification: delivered files at G
            canon_in.append({"Path": p, "Blob": blob_at(G, p)})
        for p in (L.DEC, L.STATE):                   # the unit's revision records at the verified commit
            canon_in.append({"Path": p, "Blob": blob_at(G, p)})
    canon_in += [{"Path": delegp, "Blob": L.blob_id(L.read_bytes(INP + "/deleg-deliv.json"))},
                 {"Path": handp, "Blob": L.blob_id(L.read_bytes(INP + "/handoff-deliv.json"))},
                 {"Path": trxp, "Blob": L.blob_id(L.read_bytes(INP + "/fixture-tests.trx"))}]
    auto = [{"Path": "AGENTS.md", "Blob": agents_b, "Adapter": "codex-cli"}]
    rb_ap = {"Path": L.AP_PATH, "Section": "### 16.13 Compatibilidad de protocolos de ejecución delegada", "Blob": ap_b}
    rb_agents = {"Path": "AGENTS.md", "Section": "## Leer primero", "Blob": agents_b}
    mapdoc = json.loads(L.git(D, "show", L.MAIN + ":" + L.MAP_PATH).decode("utf-8"))
    trans = [{"Path": L.WF_PATH, "Blob": blob_at(L.AR, L.WF_PATH), "RequiredBy": rb_agents},
             {"Path": "docs/automation/agent-execution/README.md", "Blob": blob_at(L.AR, "docs/automation/agent-execution/README.md"),
              "RequiredBy": rb_agents},
             {"Path": L.MAP_PATH, "Blob": blob_at(L.MAIN, L.MAP_PATH), "RequiredBy": rb_ap},
             {"Path": L.MAP_SCHEMA_PATH, "Blob": blob_at(L.MAIN, L.MAP_SCHEMA_PATH), "RequiredBy": rb_ap},
             {"Path": L.HANDOFF_SCHEMA_PATH, "Blob": blob_at(L.AR, L.HANDOFF_SCHEMA_PATH),
              "RequiredBy": {"Path": L.AP_PATH, "Section": "### 16.9 Verificación: comprobaciones obligatorias y modos de fallo",
                             "Blob": ap_b}}]
    eff1 = L.gits(D, "rev-parse", L.MAIN + "^1")
    for f in mapdoc["Files"]:                        # MV-3..MV-6 read the map's MODIFIED files at EFF and EFF^1
        if f["FileKind"] != "MODIFIED":
            continue
        for c in (L.MAIN, eff1):
            b = blob_at(c, f["Path"])
            ent = {"Path": f["Path"], "Blob": b, "RequiredBy": rb_ap}
            if b and ent not in trans and {"Path": f["Path"], "Blob": b} not in canon_in:
                trans.append(ent)
    obl = []
    for t in (L.WF_PATH, L.AP_PATH, "docs/automation/agent-execution/README.md"):
        obl.append({"SourcePath": "AGENTS.md", "SourceSection": "## Leer primero", "SourceBlob": agents_b,
                    "Text": "Leer primero: " + t, "Class": "READ", "Resolution": "INCLUDED", "Target": t, "Authority": None,
                    "Reason": None})
    obl.append({"SourcePath": "AGENTS.md", "SourceSection": "## Comandos canonicos", "SourceBlob": agents_b,
                "Text": "git status; git log --oneline -10", "Class": "ACTION_COMPATIBLE", "Resolution": "ALLOWED",
                "Target": "git status", "Authority": None, "Reason": None})
    obl.append({"SourcePath": "AGENTS.md", "SourceSection": "## Comandos canonicos", "SourceBlob": agents_b,
                "Text": "dotnet test tests/Fixture.Tests/Fixture.Tests.csproj", "Class": "ACTION_INCOMPATIBLE",
                "Resolution": "EXEMPTED", "Target": "dotnet test", "Authority": "CD-07 / OQ-17 (assumed exemption of the R3 evidence)",
                "Reason": None})
    for t in (L.MAP_PATH, L.MAP_SCHEMA_PATH):
        obl.append({"SourcePath": L.AP_PATH, "SourceSection": rb_ap["Section"], "SourceBlob": ap_b,
                    "Text": "M := mapa en EFF (ruta fija); valido contra el esquema en EFF; Validate(M)", "Class": "READ",
                    "Resolution": "INCLUDED", "Target": t, "Authority": None, "Reason": None})
    obl.append({"SourcePath": L.AP_PATH, "SourceSection": "### 16.9 Verificación: comprobaciones obligatorias y modos de fallo",
                "SourceBlob": ap_b, "Text": "Handoff: entrega presente y valida", "Class": "READ", "Resolution": "INCLUDED",
                "Target": L.HANDOFF_SCHEMA_PATH, "Authority": None, "Reason": None})
    eic = {"Schema": "rackcad-input-closure/v1", "AuthorityRevision": L.AR, "CanonicalInputs": canon_in,
           "AutomaticInstructions": auto, "Obligations": obl, "AllowedTransitiveInputs": trans,
           "AllowedActions": [{"Action": "git status", "RequiredBy": "AGENTS.md#Comandos canonicos", "Class": "ACTION_COMPATIBLE",
                               "ExemptionRef": None, "ExemptionScope": None},
                              {"Action": "dotnet test tests/Fixture.Tests/Fixture.Tests.csproj", "RequiredBy": "AGENTS.md#Comandos canonicos",
                               "Class": "EXEMPTED", "ExemptionRef": "CD-07 / OQ-17 (assumed)", "ExemptionScope": "this VERIFY invocation"}],
           "DeclaredRuntimeContext": [{"Kind": "ADAPTER_SYSTEM_INSTRUCTIONS", "Source": "codex-cli descriptor 155f3469 (not exposed)",
                                       "SizeOrSha256": "0"}],
           "ForbiddenInputs": ["transcripción y memoria del autor", "sesiones ajenas", "worktrees reales de otras unidades",
                               "artefactos transitorios no enumerados"],
           "FixedPointIterations": 2}
    inv = {"Schema": "rackcad-role-invocation/v1 (excerpt used by the derivation)", "UnitId": "FX-U1", "TaskId": "T1",
           "RunId": "R20261012T092000Z-c001", "Attempt": 0, "Target": {"Commit": G, "Path": "src/Fixture.Lib/Calculator.cs",
                                                                       "Blob": blob_at(G, "src/Fixture.Lib/Calculator.cs")}}
    errs = L.validate_schema(eic, json.loads(L.schema_bytes("input-closure.v1.schema.json").decode("utf-8")))
    return eic, inv, errs, {"DelegPath": delegp, "HandPath": handp, "TrxPath": trxp}


def derive_closure(eic, inv, deleg, hand, contract, offgit):
    """The table DERIVATION, applied literally. offgit: {path: local file} for E entries that are not in Git."""
    commits = set([eic["AuthorityRevision"], deleg["BaseSha"], deleg["MainSha"], deleg["ChainBaseSha"], hand["BaseSha"],
                   hand["CurrentSha"], inv["Target"]["Commit"], contract["AuthorityRevision"], contract["MainSha"]])
    for x in (deleg.get("ChainRedSha"), hand.get("RedSha")):
        if x:
            commits.add(x)
    commits.update(L.gits(D, "rev-list", deleg["BaseSha"] + ".." + hand["CurrentSha"]).split())
    fp = L.gits(D, "rev-list", "--first-parent", deleg["MainSha"]).split()
    commits.update(fp)
    for m in fp:
        ps = L.gits(D, "rev-list", "--parents", "-n", "1", m).split()[1:]
        if len(ps) >= 2:
            commits.update(ps)
    commits.add(L.gits(D, "merge-base", deleg["MainSha"], contract["AuthorityRevision"]))
    refs = ["HEAD", "refs/remotes/origin/" + deleg["ExpectedBranch"], "refs/remotes/origin/main"]
    entries = [(e["Path"], e["Blob"]) for k in ("CanonicalInputs", "AllowedTransitiveInputs", "AutomaticInstructions")
               for e in eic[k]]
    want = {}
    for p, b in entries:
        want.setdefault(p, set()).add(b)
    revpaths = []
    for c in sorted(commits):
        for p in sorted(want):
            b = blob_at(c, p)
            if b is not None and b in want[p]:
                revpaths.append({"Rev": c, "Path": p})
    mp = [e for e in eic["CanonicalInputs"] + eic["AllowedTransitiveInputs"] if e["Path"] == L.MAP_PATH]
    m = json.loads(L.git(D, "cat-file", "blob", mp[0]["Blob"]).decode("utf-8")) if mp else {"Surfaces": [], "Files": []}
    paths = set(p for p, _ in entries)
    paths.update(o["Target"] for o in eic["Obligations"] if o["Class"] == "READ" and o["Target"])
    paths.update(m.get("Surfaces", []))
    paths.update(f["Path"] for f in m.get("Files", []))
    paths.update(a["Path"] for a in contract["Authorities"])
    paths.add("artifacts/orchestration/%s/%s/%d/%s/output.json" % (inv["UnitId"], inv["TaskId"], inv["Attempt"], inv["RunId"]))
    files = []
    names = {}
    for e in eic["CanonicalInputs"]:
        p = e["Path"]
        if p == L.K:
            n = "CONTRACT"
        elif re.fullmatch(r"artifacts/orchestration/[^/]+/[^/]+/[0-9]+/%s/output\.json" % re.escape(deleg["RunId"]), p):
            n = "DELEG"
        elif re.fullmatch(r"artifacts/orchestration/[^/]+/[^/]+/[0-9]+/%s/worker-handoff\.json" % re.escape(hand["RunId"]), p):
            n = "H"
        elif p.endswith(".trx"):
            n = "TRX"
        else:
            continue
        if p in offgit:
            data = L.read_bytes(offgit[p])
        else:
            data = L.git(D, "cat-file", "blob", e["Blob"])
        if L.blob_id(data) != e["Blob"]:
            raise RuntimeError("custodied bytes do not match the E blob for " + p)
        files.append({"Name": n, "Sha256": L.sha256_bytes(data)})
        names[p] = n
    return {"Schema": "rackcad-bmin-closure/0-experimental", "Commits": sorted(commits), "RevPaths": revpaths, "Refs": refs,
            "Paths": sorted(paths), "Objects": [], "Files": files}, names


def accept(ctx, label, runs, envs, closure_path, verification):
    req = {"Schema": "rackcad-bmin-accept-request/0-experimental", "Repo": D,
           "Tool": {"Path": L.TOOL, "PinnedBlob": L.TOOL_BLOB, "RunFrom": "path"}, "Interpreter": ctx["Interp"],
           "Runs": [{"Name": r, "Envelope": envs[r], "Argv": a} for r, a, _i in runs],
           "Closure": {"Path": closure_path, "Sha256": L.sha256_file(closure_path)},
           "RelayFacts": {"Path": INP + "/relay.json", "CitedSha256": L.sha256_file(INP + "/relay.json")},
           "Verification": verification}
    rp = "%s/req/%s.json" % (DD, label)
    L.write_json_file(rp, req)
    rc, res = L.run_acceptor(rp)
    s = L.acceptor_summary(rc, res)
    r9 = [c for c in res.get("Checks", []) if c.get("Id") == "R9"][0]
    s["R9"] = r9.get("Status")
    s["R9Runs"] = dict((r["Name"], r.get("Violations")) for r in (r9.get("Detail") or {}).get("Runs", []))
    return s


def main():
    t0 = time.time()
    build = json.loads(L.read_bytes(L.WORK + "/build.json").decode("ascii"))
    ctx = PA.build_inputs(build)
    ctx["Interp"] = {"Python": L.platform.python_version(), "ExecutableSha256": L.exe_sha256()}
    ctx["CvSchema"] = json.loads(L.schema_bytes("controller-verification.v2.schema.json").decode("utf-8"))
    s2 = [s for s in PA.scenarios(build) if s["Name"] == "S2-REAL-DELIVERY"][0]
    G = build["Deliv"]["GreenSha"]
    deleg = PA.jload(INP + "/deleg-deliv.json")
    hand = PA.jload(INP + "/handoff-deliv.json")
    out = {"Derivation": [{"Field": f, "Rule": r, "Formula": t, "Mechanical": n} for f, r, t, n in DERIVATION],
           "NotMechanical": NOT_MECHANICAL, "Cases": []}
    for variant, strict in (("EIC-VERIFY", False), ("EIC-I6-STRICT", True)):
        eic, inv, errs, offp = build_eic(ctx, build, strict_i6=strict)
        offgit = {offp["DelegPath"]: INP + "/deleg-deliv.json", offp["HandPath"]: INP + "/handoff-deliv.json",
                  offp["TrxPath"]: INP + "/fixture-tests.trx"}
        L.write_json_file("%s/%s.input-closure.json" % (DD, variant), eic, pretty=True)
        cl, names = derive_closure(eic, inv, deleg, hand, ctx["K"], offgit)
        clp = "%s/%s.bmin-closure.json" % (DD, variant)
        L.write_json_file(clp, cl)
        # VERIFY runs = the S2 runs; plus runs that read outside the closure
        runs = list(s2["Runs"])
        L.write_bytes(INP + "/dupkey-free.json", b'{"A": 1}\n')
        outside = [
            ("outside-path", ["read-text", "--repo", D, "--rev", G, "--path", "README.md", "--lines", "1-1"], True),
            ("outside-commit", ["read-text", "--repo", D, "--rev", L.gits(D, "rev-parse", "54f2a4a"),
                                "--path", "AGENTS.md", "--lines", "1-1"], True),
            ("outside-hashes", ["hashes", "--repo", D, "--rev", G, "--paths", "FIXTURE-MANIFEST.json"], True),
            ("outside-file", ["read-json", "--repo", D] + file_arg("N", INP + "/dupkey-free.json") + ["--pointer", "/A"], True)]
        envs = {}
        for r, a, iso in runs + outside:
            rc, raw = L.run_tool(L.TOOL, a, isolated=iso, cwd=D)
            p = "%s/env/%s/%s.json" % (DD, variant, r)
            L.write_bytes(p, raw)
            envs[r] = p
        res_mc = MD.results_from_facts(dict((r, PA.jload(envs[r])) for r, _a, _i in runs), deleg, hand)
        ar_rows = MD.authority_resolution(MD.facts(PA.jload(envs["clausemap"])), ctx["K"]["Authorities"], L.MAIN, L.AR)
        cv = MD.controller_verification(res_mc, {"TaskId": "T1", "RunId": inv["RunId"], "DelegationRunId": deleg["RunId"],
                                                 "WorkRunId": hand["RunId"], "Unit": "FX-U1"}, G, ar_rows, "part (d)")
        cvp = "%s/cv-%s.json" % (DD, variant)
        cvs = L.write_json_file(cvp, cv, pretty=True)
        verif = {"Controller": {"Path": cvp, "Sha256": cvs},
                 "Delegation": {"Path": INP + "/deleg-deliv.json", "Sha256": L.sha256_file(INP + "/deleg-deliv.json")},
                 "Handoff": {"Path": INP + "/handoff-deliv.json", "Sha256": L.sha256_file(INP + "/handoff-deliv.json")},
                 "IgnorePath": "artifacts/orchestration/FX-U1/T1/0/%s/output.json" % inv["RunId"], "TermsRev": L.MAIN,
                 "MainSha": L.MAIN}
        inside = accept(ctx, variant + "-inside", runs, envs, clp, verif)
        case = {"Variant": variant, "EicSchemaErrors": errs,
                "EicCounts": dict((k, len(eic[k])) for k in ("CanonicalInputs", "AllowedTransitiveInputs", "AutomaticInstructions",
                                                              "Obligations")),
                "ClosureCounts": dict((k, len(cl[k])) for k in ("Commits", "RevPaths", "Refs", "Paths", "Objects", "Files")),
                "FileNames": names, "Inside": {"Decision": inside["Decision"], "Status": inside["Status"], "R9": inside["R9"],
                                               "R9Runs": inside["R9Runs"]}}
        if not strict:
            case["Outside"] = {}
            for r, a, i in outside:
                s = accept(ctx, variant + "-" + r, runs + [(r, a, i)], envs, clp, verif)
                case["Outside"][r] = {"Argv": a, "R9": s["R9"], "Violations": s["R9Runs"].get(r),
                                      "OtherRunsViolations": dict((k, v) for k, v in s["R9Runs"].items() if k != r and v)}
                L.log(variant, r, s["R9"], (s["R9Runs"].get(r) or [])[:3])
            case["EicSample"] = {"CanonicalInputs": eic["CanonicalInputs"], "AllowedTransitiveInputs": eic["AllowedTransitiveInputs"],
                                 "AuthorityRevision": eic["AuthorityRevision"]}
            case["Closure"] = cl
        out["Cases"].append(case)
        L.log(variant, "inside:", inside["Decision"], inside["R9"], dict((k, v[:2]) for k, v in inside["R9Runs"].items() if v))
    out["Seconds"] = round(time.time() - t0, 1)
    p, sha = L.write_result("d-closure.json", {"Schema": "r3-part-d/1", "Header": L.common_header(), "Result": out})
    L.log("written", p, sha)


if __name__ == "__main__":
    main()
