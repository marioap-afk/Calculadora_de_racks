"""I-62 A-6, preflight: controles negativos de extremo a extremo de a6-guards.py (por la línea de órdenes, no en memoria).

Complementan los mutantes de la autoprueba: cada caso escribe en D:/r62-a6-tmp una copia alterada de A-6 (casos de archivo) o construye en un clon
temporal un commit candidato que además toca una ruta prohibida (casos de commit), ejecuta `a6-guards.py preview` y exige que fallen las guardas
esperadas. Un control positivo (A-6 sin alterar, sin commit candidato) debe pasar. Solo lectura del worktree de I-62; los temporales se borran.
Uso: python -I -B neg_controls_a6.py --out out/neg_controls_a6.json
"""
import argparse
import json
import os
import re
import shutil
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
WT = os.path.join(os.environ["USERPROFILE"], ".codex", "worktrees", "architecture-portabilidad-coordinador-principal")
A6_LOCAL = os.path.normpath(os.path.join(HERE, "..", "..", "..", "initiatives", "I-62-A-6.md"))
GUARDS = os.path.join(HERE, "a6-guards.py")
MAKE = os.path.join(HERE, "make_candidate_commit_a6.py")
TMP = "D:/r62-a6-tmp/neg"
S765 = "A-6 no se presenta a acuerdo mientras exista una consecuencia OWNER-RESERVED sin disponer"


def sub1(text, old, new):
    assert text.count(old) >= 1, "caso no aplicable: %s" % old[:50]
    return text.replace(old, new, 1)


def resub1(text, pat, new):
    t, n = re.subn(pat, new, text, count=1, flags=re.M)
    assert n == 1, "caso no aplicable: %s" % pat[:50]
    return t


FILE_CASES = [
    ("N-01", "punto 4 del dictamen retirado de §3.2", ["G4"], lambda t: resub1(t, r"^\| 4 \| AP 16\.24: registro de fidelidad[^\n]*\n", "")),
    ("N-02", "M-05 = no", ["G4"], lambda t: sub1(t, "| M-05 | **sí** | yes |", "| M-05 | no | yes |")),
    ("N-03", "las dos líneas del Owner fundidas", ["G6"], lambda t: sub1(t, "que la cite)\nA6-CONTROLLER-MEDICION-CONSUMO = A (",
                                                                          "que la cite); A6-CONTROLLER-MEDICION-CONSUMO = A (")),
    ("N-04", "blob del instrumento cambiado en §3.3", ["G7"], lambda t: sub1(t, "| `9a829b0eeb1210238052b1feac06cfa8f7964aed` |",
                                                                              "| `9a829b0eeb1210238052b1feac06cfa8f7964aea` |")),
    ("N-05", "frase de decisiones §76, punto 5, retirada de la cabecera", ["G8"],
     lambda t: sub1(t, "de §7 (a). " + S765 + ".\nLa aplicación", "de §7 (a).\nLa aplicación")),
    ("N-06", "«A-6 = AGREED» añadido", ["G8"], lambda t: sub1(t, "## 11. Estado de revisión\n", "## 11. Estado de revisión\n\nA-6 = AGREED.\n")),
    ("N-07", "ancla de A-5 L221 alterada", ["G1"], lambda t: sub1(t, "que termina en «extiende a esta fila.» (A-5 L221)",
                                                                  "que termina en «extiende a esa fila.» (A-5 L221)")),
    ("N-08", "A6-3 ubicada en D.1", ["G5"], lambda t: sub1(t, "Ubicación: al final de V14 D.6, a continuación", "Ubicación: al final de V14 D.1, a continuación")),
    ("N-09", "A6-1 sin la prohibición de cambiar la receta", ["G5"], lambda t: sub1(t, "> 3. ningún elemento de la receta del adapter cambia:",
                                                                                     "> 3. la receta del adapter puede cambiar:")),
    ("N-10", "superficie prohibida retirada de §6.1", ["G3"], lambda t: sub1(t, "`docs/automation/evidence/I-62-F6/FX-02/bmin/`, ", "")),
]
COMMIT_CASES = [
    ("N-11", "commit candidato que además modifica AUTOMATION_PLAN (receta 16.4)", ["G2", "G3"], "docs/AUTOMATION_PLAN.md"),
    ("N-12", "commit candidato que además modifica el registro de evidencia", ["G2"], "docs/automation/evidence/I-62-evidence.md"),
    ("N-13", "commit candidato que además modifica el instrumento fijado", ["G2", "G3"], "docs/automation/evidence/I-62-F6/FX-02/bmin/tool/bmin_facts.py"),
]


def rmtree_ro(path):
    def _writable(func, p, exc):
        os.chmod(p, 0o600)
        func(p)
    if os.path.exists(path):
        shutil.rmtree(path, onexc=_writable)


def run_preview(a6_path, out, cand=None):
    cmd = [sys.executable, "-I", "-B", GUARDS, "preview", "--repo", WT, "--a6", a6_path, "--pkgdir", HERE, "--out", out, "--tmp", TMP + "/gt"]
    if cand:
        cmd += ["--cand-repo", cand[0], "--cand-head", cand[1]]
    r = subprocess.run(cmd, capture_output=True)
    res = json.load(open(out, encoding="utf-8"))
    return r.returncode, res


def main():
    try:
        sys.stdout.reconfigure(encoding="utf-8")
    except Exception:
        pass
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", default=os.path.join(HERE, "out", "neg_controls_a6.json"))
    a = ap.parse_args()
    os.environ["GIT_OPTIONAL_LOCKS"] = "0"
    rmtree_ro(TMP)
    os.makedirs(TMP)
    a6 = open(A6_LOCAL, "rb").read().replace(b"\r\n", b"\n").decode("utf-8")
    rows = []
    rc, res = run_preview(A6_LOCAL, TMP + "/p0.json")
    fails = sorted(g for g, r in res["Guards"].items() if r["Result"] == "FAIL")
    rows.append({"Id": "P-0", "Case": "control positivo: A-6 sin alterar, sin commit candidato", "Expected": "PASS", "Result": res["Result"],
                 "FailingGuards": fails, "AsExpected": res["Result"] == "PASS" and rc == 0})
    for cid, name, expected, fn in FILE_CASES:
        path = "%s/%s.md" % (TMP, cid)
        with open(path, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(fn(a6))
        rc, res = run_preview(path, "%s/%s.json" % (TMP, cid))
        fails = sorted(g for g, r in res["Guards"].items() if r["Result"] == "FAIL")
        first = {g: (res["Guards"][g]["Findings"] or [""])[0][:140] for g in expected if g in res["Guards"]}
        rows.append({"Id": cid, "Case": name, "Expected": expected, "Result": res["Result"], "FailingGuards": fails, "FirstFinding": first,
                     "AsExpected": rc == 1 and res["Result"] == "FAIL" and set(expected) <= set(fails)})
    for cid, name, expected, path in COMMIT_CASES:
        extra = "%s/%s.extra" % (TMP, cid)
        cur = subprocess.run(["git", "-C", WT, "show", "HEAD:" + path], capture_output=True).stdout.replace(b"\r\n", b"\n")
        with open(extra, "wb") as fh:
            fh.write(cur + b"\nlinea negativa del preflight de A-6\n")
        dest = "%s/rc-%s" % (TMP, cid)
        subprocess.run([sys.executable, "-I", "-B", MAKE, "--dest", dest, "--out", "%s/%s-cand.json" % (TMP, cid), "--extra", "%s=%s" % (path, extra)],
                       capture_output=True)
        cand = json.load(open("%s/%s-cand.json" % (TMP, cid), encoding="utf-8"))["Candidate"]
        rc, res = run_preview(A6_LOCAL, "%s/%s.json" % (TMP, cid), (dest, cand))
        fails = sorted(g for g, r in res["Guards"].items() if r["Result"] == "FAIL")
        first = {g: (res["Guards"][g]["Findings"] or [""])[0][:140] for g in expected if g in res["Guards"]}
        rows.append({"Id": cid, "Case": name, "Expected": expected, "Result": res["Result"], "FailingGuards": fails, "FirstFinding": first,
                     "AsExpected": rc == 1 and res["Result"] == "FAIL" and set(expected) <= set(fails)})
        rmtree_ro(dest)
    rmtree_ro(TMP)
    summary = {"Cases": len(rows), "AsExpected": sum(r["AsExpected"] for r in rows),
               "Note": "en los casos de archivo, G5b también falla porque la autoprueba custodiada fija el blob de la A-6 sin alterar"}
    out = {"Tool": "neg_controls_a6.py", "Summary": summary, "Rows": rows}
    with open(a.out, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(json.dumps(out, ensure_ascii=False, indent=1) + "\n")
    print(json.dumps(summary, ensure_ascii=False))
    sys.exit(0 if summary["AsExpected"] == summary["Cases"] else 1)


if __name__ == "__main__":
    main()
