"""I-62 A-5: ejecución del preflight, apartados (a) MV-1..MV-7 con el método de A5-1 y (b) autoridades intactas.

Solo lectura sobre RackCad (worktree de I-62) y sobre el fixture actual (se usa un clon temporal tmp-a5-ref). Escribe solo en ./out y en
D:\\r62-fixture\\tmp-a5-* (temporales propios, que se borran al final del preflight). Ninguna ruta del host se escribe en las salidas.
Rutas: el worktree se deriva de os.environ['USERPROFILE']; nunca se escribe el nombre de usuario.
Uso: python -I -B preflight_run.py
"""
import json
import os
import re
import shutil
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "out")
WT = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
MAPINV = os.path.join(WT, "docs", "automation", "evidence", "I-62-F6", "MAP-INVALID")
MV = os.path.join(MAPINV, "mv_check.py")
BUNDLE = os.path.join(MAPINV, "candidate-b.bundle")
CLAUSE_MAP = os.path.join(WT, "docs", "automation", "evidence", "I-62-F4", "compat", "clause_map.py")
FX = "D:/r62-fixture"
CAND, REF, NEG, CB = FX + "/tmp-a5-b1", FX + "/tmp-a5-ref", FX + "/tmp-a5-neg", FX + "/tmp-a5-cb"
MC_I62 = "6f0187cb30852971b49153c2763c8ba6caeaa64d"
OLD_EFF = "fbe25347799e5b801ef708454212335537448bd1"
FX_U1 = "95bdc29d428b35d2188a10e1c5462d6caf2be4df"
SURFACES = ["AGENTS.md", "CLAUDE.md", "docs/AUTOMATION_PLAN.md", "docs/FOUNDATIONS.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/WORKFLOW.md",
            "docs/adr/", "docs/automation/agent-execution/", "docs/initiatives/PROMPT_TEMPLATES.md"]
FIXTURE_LOCAL_SURFACES = {"AGENTS.md", "CLAUDE.md"}
INVALIDATORS = ["docs/automation/agent-execution/routing.md", "docs/automation/agent-execution/model-catalog.md",
                "docs/automation/agent-execution/adapters/codex-cli.md", "docs/automation/agent-execution/adapters/claude-cli.md",
                "docs/automation/agent-execution/compatibility/I62-clause-map.json",
                "docs/automation/agent-execution/compatibility/clause-map.schema.json", "docs/AUTOMATION_PLAN.md", "docs/WORKFLOW.md"]


PROFILE_RE = re.compile(r"(?i)[a-z]:(?:\\\\|\\|/)+users(?:\\\\|\\|/)+[^\\/\s\"']+")


def sanitize(text):
    for s, rep in ((WT, "%WT%"), (WT.replace("\\", "/"), "%WT%"), (WT.replace("\\", "\\\\"), "%WT%"), (HERE, "."), (HERE.replace("\\", "/"), "."),
                   (HERE.replace("\\", "\\\\"), ".")):
        text = text.replace(s, rep)
    return PROFILE_RE.sub("%USERPROFILE%", text)


def git(repo, *args, ok=(0,)):
    r = subprocess.run(["git", "-C", repo] + list(args), capture_output=True)
    if r.returncode not in ok:
        raise SystemExit("git %s: %s" % (args[0], r.stderr.decode("utf-8", "replace")[-200:]))
    return r.stdout.decode("utf-8", "replace")


def py(args, cwd=None):
    r = subprocess.run([sys.executable, "-I", "-B"] + args, capture_output=True, encoding="utf-8", cwd=cwd)
    return r.returncode, sanitize(r.stdout + r.stderr)


def tree(repo, rev):
    out = {}
    for line in git(repo, "ls-tree", "-r", rev).splitlines():
        meta, path = line.split("\t", 1)
        out[path] = meta.split()[2]
    return out


def under(p):
    return any(p == s or (s.endswith("/") and p.startswith(s)) for s in SURFACES)


def write(name, obj):
    with open(os.path.join(OUT, name), "w", encoding="utf-8", newline="\n") as fh:
        if isinstance(obj, str):
            fh.write(obj if obj.endswith("\n") else obj + "\n")
        else:
            fh.write(sanitize(json.dumps(obj, ensure_ascii=False, indent=1)) + "\n")


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    b = json.load(open(os.path.join(OUT, "build_a5.json"), encoding="utf-8"))
    S, F, E, C = b["F_seed"], b["F_norm"], b["F_eff"], b["Claim"]["C"]
    res = {"Build": {k: b[k] for k in ("F_seed", "F_norm", "F_eff", "TagObject", "SeedCopied", "NormCopied", "Local", "ProvenanceChecks")}}
    repro = json.load(open(os.path.join(OUT, "build_a5_repro.json"), encoding="utf-8"))
    res["Build"]["Reproducible"] = all(b[k] == repro[k] for k in ("F_seed", "F_norm", "F_eff", "TagObject")) and b["Claim"]["C"] == repro["Claim"]["C"]
    res["Build"]["Remotes"] = git(CAND, "remote").split()
    # ---- (a) MV-1..MV-7
    mv = {}
    for key, repo, ref, label in (("A5_main", CAND, "refs/heads/main", "candidato A5-1, main = F_eff"),
                                  ("A5_claim", CAND, "refs/heads/fx/u2", "candidato A5-1, punta fx/u2 (reclamo tras la precondición)"),
                                  ("CurrentFixture_main", REF, "refs/heads/main", "fixture actual (referencia), main = fbe25347")):
        jf = os.path.join(OUT, "mv_%s.json" % key)
        rc, txt = py([MV, "--repo", repo, "--main", ref, "--label", label, "--out", jf])
        write("mv_%s.txt" % key, txt)
        o = json.load(open(jf, encoding="utf-8"))
        write("mv_%s.json" % key, o)
        mv[key] = {"ExitCode": rc, "Verdict": o.get("Verdict"), "MVFailed": o.get("MVFailed"), "EFF": (o.get("Derive") or {}).get("EFF"),
                   "Trailers": len((o.get("Derive") or {}).get("TrailerCommits", [])),
                   "MV": {k: v.get("Result") if isinstance(v, dict) else v for k, v in (o.get("Validate") or {}).items()}}
    res["MV"] = mv
    # F4 C-20b sobre el candidato
    jf = os.path.join(OUT, "f4_clause_map_a5.json")
    rc, txt = py([CLAUSE_MAP, "check", S, E, jf], cwd=CAND)
    o = json.load(open(jf, encoding="utf-8"))
    write("f4_clause_map_a5.json", o)
    res["F4_clause_map"] = {"ExitCode": rc, "Result": o["Result"], "Files": len(o["Files"]), "EntriesByKind": o["EntriesByKind"], "MapBlob": o["MapBlob"]}
    # negativos MV (copia del candidato)
    def _writable(func, path, exc):  # los objetos de Git son de solo lectura en Windows
        os.chmod(path, 0o600)
        func(path)
    for d in (NEG, CB):
        if os.path.exists(d):
            shutil.rmtree(d, onexc=_writable)
    git(FX, "clone", "-q", "--bare", "--no-local", CAND, NEG)
    git(NEG, "remote", "remove", "origin")
    rc, txt = py([os.path.join(HERE, "neg_controls_a5.py"), "--repo", NEG, "--build", os.path.join(OUT, "build_a5.json"), "--checker", MV,
                  "--out", os.path.join(OUT, "neg_controls_a5.json"), "--tips-out", os.path.join(OUT, "neg_tips_a5.json")])
    write("neg_controls_a5.txt", txt)
    neg = json.load(open(os.path.join(OUT, "neg_controls_a5.json"), encoding="utf-8"))
    res["NegativeMV"] = {"ExitCode": rc, "Cases": len(neg["Cases"]), "AsExpected": sum(c["AsExpected"] for c in neg["Cases"]),
                         "AllAsExpected": neg["AllAsExpected"]}
    tips = json.load(open(os.path.join(OUT, "neg_tips_a5.json"), encoding="utf-8"))
    # conformidad estructural con A5-1
    git(FX, "clone", "-q", "--bare", BUNDLE, CB)
    git(CB, "remote", "remove", "origin")
    conf = {}
    for key, repo, ref, label, want in (("A5_candidate", CAND, "refs/heads/main", "candidato A5-1", "CONFORMANT"),
                                        ("CurrentFixture", REF, "refs/heads/main", "fixture actual (V14 D.1 literal)", "NON_CONFORMANT"),
                                        ("CandidateB_bundle", CB, "refs/heads/main", "candidato (b) de §66 (paquete 59b24493; dos commits en la rama)",
                                         "NON_CONFORMANT"),
                                        ("Neg_N8", NEG, tips["N-8"], "N-8 (V14 D.1 literal sobre el grafo de A5-1)", "NON_CONFORMANT"),
                                        ("Neg_N9", NEG, tips["N-9"], "N-9 (autoridades por el primer padre)", "NON_CONFORMANT")):
        jf = os.path.join(OUT, "conf_%s.json" % key)
        rc, txt = py([os.path.join(HERE, "a5_conformance.py"), "--repo", repo, "--main", ref, "--rackcad", WT, "--label", label, "--out", jf])
        o = json.load(open(jf, encoding="utf-8"))
        conf[key] = {"Expected": want, "Verdict": o["Verdict"], "AsExpected": o["Verdict"] == want, "Failures": [f.split(" ")[0] for f in o["Failures"]]}
        os.remove(jf)
    write("conformance_a5.json", conf)
    res["Conformance"] = conf
    # árboles frente al candidato (b) de §66: solo cambia el manifiesto
    cb = json.load(open(os.path.join(MAPINV, "build_b.json"), encoding="utf-8"))
    tcb_s, tcb_e = tree(CB, cb["S_seed"]), tree(CB, cb["E_eff"])
    ts, tf, te = tree(CAND, S), tree(CAND, F), tree(CAND, E)
    diff = lambda x, y: sorted(p for p in set(x) | set(y) if x.get(p) != y.get(p))
    res["TreesVsCandidateB"] = {"F_seed_vs_S": diff(ts, tcb_s), "F_eff_vs_E": diff(te, tcb_e)}
    # ---- (b) autoridades intactas
    rc_eff = tree(WT, MC_I62)
    rc_head = tree(WT, "HEAD")
    ref_eff = tree(REF, OLD_EFF)
    ref_u1 = tree(REF, FX_U1)
    surf_rows, bad = [], []
    for p in sorted(x for x in te if under(x)):
        if p in FIXTURE_LOCAL_SURFACES:
            row = {"Path": p, "Kind": "FIXTURE_LOCAL", "EFF'": te[p][:12], "CurrentFixtureEFF": (ref_eff.get(p) or "")[:12], "Equal": te[p] == ref_eff.get(p)}
        else:
            row = {"Path": p, "Kind": "AUTHORITY", "EFF'": te[p][:12], "MC_I62": (rc_eff.get(p) or "")[:12], "RackCadHEAD": (rc_head.get(p) or "")[:12],
                   "Equal": te[p] == rc_eff.get(p) == rc_head.get(p)}
        surf_rows.append(row)
        if not row["Equal"]:
            bad.append(p)
    missing = sorted(p for p in rc_eff if (p.startswith(("docs/automation/agent-execution/", "docs/WORKFLOW.md", "docs/INITIATIVE_LIFECYCLE.md",
                                                          "docs/AUTOMATION_PLAN.md"))) and p not in te)
    inval = [{"Path": p, "EFF'": te.get(p), "CurrentFixtureEFF": ref_eff.get(p), "FX-U1_95bdc29d": ref_u1.get(p), "MC_I62": rc_eff.get(p),
              "Equal": te.get(p) == ref_eff.get(p) == ref_u1.get(p) == rc_eff.get(p)} for p in INVALIDATORS]
    auth = {"SurfaceFilesAtEFF'": len(surf_rows), "Rows": surf_rows, "NotEqual": bad, "D1AuthoritiesMissingAtEFF'": missing,
            "Invalidators": inval, "InvalidatorsEqual": sum(r["Equal"] for r in inval),
            "ClauseMapSchemaToolBlob": git(WT, "rev-parse", "HEAD:docs/automation/evidence/I-62-F4/compat/clause_map.py").strip()}
    write("authorities_a5.json", auth)
    res["Authorities"] = {"SurfaceFiles": len(surf_rows), "Equal": len(surf_rows) - len(bad), "NotEqual": bad, "MissingD1": missing,
                          "Invalidators": "%d/%d" % (auth["InvalidatorsEqual"], len(inval))}
    # digest de las refs del fixture actual (no debe cambiar)
    res["CurrentFixtureRefs"] = git("D:/r62-fixture/fixture-origin.git", "for-each-ref", "--format=%(objectname) %(refname)")
    write("preflight_ab.json", res)
    print(json.dumps({k: v for k, v in res.items() if k != "CurrentFixtureRefs"}, ensure_ascii=False, indent=1)[:6000])


if __name__ == "__main__":
    main()
