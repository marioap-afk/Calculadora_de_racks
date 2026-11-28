"""Compara el HOST-EXTRAS AFTER de 785a con la recaptura de recomprobacion (solo lectura).

compare_host_extras.py exige el mismo RunId y las fases BEFORE/AFTER. Para reutilizar SUS reglas sin tocarlo, se escriben copias
temporales en el directorio de salida con RunId comun y fases BEFORE (= AFTER de 785a) y AFTER (= RECHECK). Nada mas cambia.
Uso: python -I compare_after_vs_recheck.py <compare_host_extras.py> <after.json> <recheck.json> <outdir>
"""
import json
import os
import subprocess
import sys

cmp_script, after_p, recheck_p, outdir = sys.argv[1:5]
a = json.load(open(after_p, encoding="utf-8"))
r = json.load(open(recheck_p, encoding="utf-8"))
label = "AFTER-785a-vs-RECHECK"
a2 = dict(a, RunId=label, Phase="BEFORE")
r2 = dict(r, RunId=label, Phase="AFTER")
pa = os.path.join(outdir, "tmp-cmp-left.json")
pr = os.path.join(outdir, "tmp-cmp-right.json")
for p, d in ((pa, a2), (pr, r2)):
    with open(p, "w", encoding="utf-8") as f:
        json.dump(d, f, ensure_ascii=False, indent=2)
out = os.path.join(outdir, sys.argv[5] if len(sys.argv) > 5 else "host-extras-diff-AFTER-vs-RECHECK.json")
if os.path.exists(out):
    sys.exit("la salida ya existe: " + out)
code = subprocess.call([sys.executable, "-I", cmp_script, pa, pr, out])
os.remove(pa)
os.remove(pr)
print("compare exit code", code)
sys.exit(code)
