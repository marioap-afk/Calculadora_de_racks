"""Fase 2 (decisiones §94, puntos 5 y 6): diff unificado de cada componente de kit/ frente al kit LANZADO de A-7 ronda 1 (base-launched/kit/,
manifiesto c27db93e…, custodia 3922f835/e71400a2) y frente al kit de la fase 1 aceptada (phase1-kit/), en prep/p3/diffs/{vs-launched,vs-phase1}/,
y la tabla «archivo → SHA-256 lanzado → fase 1 → ahora → estado → líneas añadidas/quitadas» en prep/p3/diffs/diff-table.json. Las copias fieles
(fiel__*.txt) se resumen en una fila. No escribe fuera de prep/p3/diffs/.
Uso: python -I -B make_diffs_r2.py <kit-review2>
"""
import difflib
import hashlib
import json
import os
import sys

ROOT = sys.argv[1]
KIT, BASE, P1 = (os.path.join(ROOT, *p) for p in (("kit",), ("base-launched", "kit"), ("phase1-kit",)))
OUT = os.path.join(ROOT, "prep", "p3", "diffs")


def sha(p):
    return hashlib.sha256(open(p, "rb").read()).hexdigest() if os.path.isfile(p) else None


def udiff(old, new, tag_old, name, sub):
    a = open(old, encoding="utf-8").read().split("\n") if os.path.isfile(old) else []
    b = open(new, encoding="utf-8").read().split("\n") if os.path.isfile(new) else []
    d = list(difflib.unified_diff(a, b, tag_old + "/" + name, "r2/" + name, lineterm=""))
    os.makedirs(os.path.join(OUT, sub), exist_ok=True)
    with open(os.path.join(OUT, sub, name + ".diff"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(d) + "\n")
    return sum(1 for l in d if l.startswith("+") and not l.startswith("+++")), sum(1 for l in d if l.startswith("-") and not l.startswith("---"))


def status(o, n):
    return "IDENTICAL" if o == n else ("NEW" if o is None else ("REMOVED" if n is None else "CHANGED"))


rows, copies = [], []
for name in sorted(set(os.listdir(KIT)) | set(os.listdir(BASE)) | set(os.listdir(P1))):
    paths = [os.path.join(d, name) for d in (BASE, P1, KIT)]
    if not any(os.path.isfile(p) for p in paths):
        continue
    sb, s1, sn = (sha(p) for p in paths)
    if name.startswith("fiel__"):
        copies.append((name, sn))
        continue
    row = {"File": name, "LaunchedSha256": sb, "Phase1Sha256": s1, "NowSha256": sn, "VsLaunched": status(sb, sn), "VsPhase1": status(s1, sn)}
    textual = name.endswith((".py", ".md", ".txt")) and name != "prompt.md"
    if textual and row["VsLaunched"] in ("CHANGED", "NEW", "REMOVED"):
        row["AddedVsLaunched"], row["RemovedVsLaunched"] = udiff(paths[0], paths[2], "launched", name, "vs-launched")
    if textual and row["VsPhase1"] in ("CHANGED", "NEW", "REMOVED"):
        row["AddedVsPhase1"], row["RemovedVsPhase1"] = udiff(paths[1], paths[2], "phase1", name, "vs-phase1")
    rows.append(row)
rows.append({"File": "fiel__*.txt (%d copias fieles)" % len(copies), "LaunchedSha256": None, "Phase1Sha256": None,
             "NowSha256": hashlib.sha256("".join("%s %s\n" % c for c in copies).encode()).hexdigest(), "VsLaunched": "NEW",
             "VsPhase1": "NEW (en la fase 1 las copias se generaban en el entorno de prueba, no en kit/)"})
os.makedirs(OUT, exist_ok=True)
with open(os.path.join(OUT, "diff-table.json"), "w", encoding="utf-8", newline="\n") as f:
    f.write(json.dumps(rows, ensure_ascii=False, indent=1) + "\n")
for r in rows:
    print("%-34s %-9s %-9s %s %s %s %s" % (r["File"][:34], r["VsLaunched"][:9], r["VsPhase1"][:9], (r["LaunchedSha256"] or "-")[:8],
                                          (r["Phase1Sha256"] or "-")[:8], (r["NowSha256"] or "-")[:8],
                                          ("L+%d/-%d" % (r["AddedVsLaunched"], r["RemovedVsLaunched"])) if "AddedVsLaunched" in r else ""))
