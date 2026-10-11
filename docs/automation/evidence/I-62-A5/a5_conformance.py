"""I-62 A-5, preflight de A5-1: conformidad ESTRUCTURAL de un fixture con el texto de A5-1 (complementa a mv_check.py, que evalúa AP 16.13).

Comprobaciones (solo lectura: rev-parse, cat-file, ls-tree, log, show):
  K-1  main = F_eff, merge con exactamente dos padres (F_seed, F_norm)            K-2  F_seed es el commit inicial (sin padres)
  K-3  F_norm tiene un solo padre = F_seed (la rama de activación es UN commit)   K-4  un único commit alcanzable desde main con el trailer, = F_norm
  K-5  árbol de F_eff = árbol de F_norm                                           K-6  el cuerpo de F_eff termina con la cabecera de la tabla PRE
  K-7  F_seed = estado anterior a la activación según el mapa de MC_I62: MODIFIED = BaseBlob; ADDED/ENTRY y mapa ausentes; demás autoridades = blob de F_norm
  K-8  F_norm: cada autoridad de D.1 y cada Files del mapa (y el mapa) con su blob de MC_I62; mapa = 4d49d3e1
  K-9  entre F_seed y F_norm solo cambian autoridades y FIXTURE-MANIFEST.json (ningún FIXTURE_LOCAL más)
  K-10 manifiestos: F_seed sin activación, F_norm con TEST-ACTIVATION; cada COPIED con el blob del árbol y un commit de origen donde ese blob está
  K-11 refs/heads/fixture/i62-norm = F_norm (si existe)
Uso: python -I -B a5_conformance.py --repo <repo> --main <ref> --rackcad <repo RackCad> [--label txt] [--out json]
Salida: 0 = CONFORMANT; 1 = NON_CONFORMANT.
"""
import argparse
import json
import subprocess
import sys

MC_I62 = "6f0187cb30852971b49153c2763c8ba6caeaa64d"
MAP_PATH = "docs/automation/agent-execution/compatibility/I62-clause-map.json"
MAP_BLOB = "4d49d3e1a63d2b6128c9d9eeac473156a421571a"
TRAILER = "Agent-Protocol-Normative: I-62"
PRE_HEADER = "Initiative | ref | tip | original claim commit | Claim-Id | temporal evidence | transition"
D1 = ["docs/WORKFLOW.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/AUTOMATION_PLAN.md", "docs/automation/agent-execution"]


def g(repo, *args, ok=(0,)):
    r = subprocess.run(["git", "-C", repo] + list(args), capture_output=True)
    if r.returncode not in ok:
        raise RuntimeError("git %s" % args[0])
    return r.returncode, r.stdout.decode("utf-8", "replace")


def tree_map(repo, rev):
    out = {}
    for line in g(repo, "ls-tree", "-r", rev)[1].splitlines():
        meta, path = line.split("\t", 1)
        out[path] = meta.split()[2]
    return out


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    ap = argparse.ArgumentParser()
    ap.add_argument("--repo", required=True)
    ap.add_argument("--main", required=True)
    ap.add_argument("--rackcad", required=True)
    ap.add_argument("--label", default="")
    ap.add_argument("--out")
    a = ap.parse_args()
    R, RC = a.repo, a.rackcad
    fails, info = [], {}
    E = g(R, "rev-parse", a.main + "^{commit}")[1].strip()
    parents = g(R, "rev-list", "--parents", "-n", "1", E)[1].split()[1:]
    info["F_eff"] = E
    if len(parents) != 2:
        fails.append("K-1 main no es un merge de dos padres (%d)" % len(parents))
        S = F = None
    else:
        S, F = parents
        info.update({"F_seed": S, "F_norm": F})
        if g(R, "rev-list", "--parents", "-n", "1", S)[1].split()[1:]:
            fails.append("K-2 F_seed no es el commit inicial")
        fp = g(R, "rev-list", "--parents", "-n", "1", F)[1].split()[1:]
        if fp != [S]:
            fails.append("K-3 F_norm no tiene un solo padre = F_seed (padres: %s)" % [p[:12] for p in fp])
    trailer = [c for c in g(R, "log", "--format=%H", "--grep=^" + TRAILER + "$", E)[1].split()]
    info["TrailerCommits"] = trailer
    if len(trailer) != 1 or (F and trailer[0] != F):
        fails.append("K-4 commits con el trailer alcanzables desde main: %s (se espera solo F_norm)" % [t[:12] for t in trailer])
    if F:
        if g(R, "rev-parse", E + "^{tree}")[1] != g(R, "rev-parse", F + "^{tree}")[1]:
            fails.append("K-5 árbol de F_eff distinto del de F_norm")
        body = g(R, "show", "-s", "--format=%B", E)[1].rstrip("\n").split("\n")
        if not body or body[-1].strip() != PRE_HEADER or len(body) < 2 or body[-2].strip() != "Derived formal claim table:":
            fails.append("K-6 el cuerpo de F_eff no termina con la tabla PRE (cabecera, sin filas)")
        cmap = json.loads(g(RC, "show", "%s:%s" % (MC_I62, MAP_PATH))[1])
        kinds = {f["Path"]: f for f in cmap["Files"]}
        eff = {}
        for line in g(RC, "ls-tree", "-r", MC_I62, "--", *D1)[1].splitlines():
            meta, path = line.split("\t", 1)
            eff[path] = meta.split()[2]
        for p, f in kinds.items():
            eff[p] = f["EffBlob"]
        ts, tf = tree_map(R, S), tree_map(R, F)
        k7, k8 = [], []
        for p, sha in sorted(eff.items()):
            f = kinds.get(p)
            if tf.get(p) != sha:
                k8.append(p)
            if p == MAP_PATH or (f and f["FileKind"] in ("ADDED", "ENTRY")):
                if p in ts:
                    k7.append(p + " (presente; debe faltar)")
            elif f and f["FileKind"] == "MODIFIED":
                if ts.get(p) != f["BaseBlob"]:
                    k7.append(p + " (no es el BaseBlob)")
            elif ts.get(p) != tf.get(p):
                k7.append(p + " (distinto de F_norm)")
        if tf.get(MAP_PATH) != MAP_BLOB:
            k8.append(MAP_PATH + " (blob del mapa)")
        if k7:
            fails.append("K-7 F_seed no es el estado anterior a la activación: %d rutas, p. ej. %s" % (len(k7), k7[:3]))
        if k8:
            fails.append("K-8 F_norm sin la copia byte a byte de MC_I62: %d rutas, p. ej. %s" % (len(k8), k8[:3]))
        changed = sorted(p for p in set(ts) | set(tf) if ts.get(p) != tf.get(p))
        stray = [p for p in changed if p not in eff and p != "FIXTURE-MANIFEST.json"]
        if stray:
            fails.append("K-9 F_norm cambia rutas que no son autoridades ni el manifiesto: %s" % stray[:5])
        info.update({"AuthoritiesInF_norm": len(eff), "ChangedSeedToNorm": len(changed), "Map": {"ADDED/ENTRY": sum(f["FileKind"] != "MODIFIED"
                     for f in kinds.values()), "MODIFIED": sum(f["FileKind"] == "MODIFIED" for f in kinds.values())}})
        k10 = []
        for label, rev, tm, want_act in (("F_seed", S, ts, False), ("F_norm", F, tf, True)):
            rc, txt = g(R, "show", "%s:FIXTURE-MANIFEST.json" % rev, ok=(0, 128))
            if rc:
                k10.append(label + ": sin manifiesto")
                continue
            m = json.loads(txt)
            act = m.get("Activation")
            if bool(act) != want_act or (want_act and act.get("Marker") != "TEST-ACTIVATION"):
                k10.append(label + ": activación incoherente")
            top = m.get("SourceCommit")
            for e in m.get("Entries", []):
                if e.get("Kind") == "COPIED":
                    if tm.get(e["Path"]) != e.get("SourceBlob"):
                        k10.append("%s: %s con SourceBlob distinto del árbol" % (label, e["Path"]))
                    src = e.get("SourceCommit") or top
                    if not src:
                        k10.append("%s: %s sin commit de origen" % (label, e["Path"]))
                        continue
                    rcb, blob = g(RC, "rev-parse", "-q", "--verify", "%s:%s" % (src, e["Path"]), ok=(0, 1, 128))
                    if blob.strip() != e.get("SourceBlob"):
                        k10.append("%s: %s no está con ese blob en su commit de origen" % (label, e["Path"]))
        if k10:
            fails.append("K-10 manifiestos: %d problemas, p. ej. %s" % (len(k10), k10[:3]))
        rc, nref = g(R, "rev-parse", "-q", "--verify", "refs/heads/fixture/i62-norm", ok=(0, 1, 128))
        if rc == 0 and nref.strip() != F:
            fails.append("K-11 fixture/i62-norm no apunta a F_norm")
    verdict = "CONFORMANT" if not fails else "NON_CONFORMANT"
    out = {"Label": a.label, "Main": E, "Verdict": verdict, "Failures": fails, "Info": info}
    print("[%s] %s -> %s" % (a.label, E[:12], verdict))
    for f in fails:
        print("  " + f)
    if a.out:
        with open(a.out, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")
    sys.exit(0 if not fails else 1)


if __name__ == "__main__":
    main()
