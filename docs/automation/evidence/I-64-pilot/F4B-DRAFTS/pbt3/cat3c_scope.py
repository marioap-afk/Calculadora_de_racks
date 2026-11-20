import json
import subprocess
import sys

# Uso: python -I cat3c_scope.py <carpeta pbt3> <checkout> <rev> [PB-T3b|PB-T3c]
# Imprime el ForbiddenWriteScope del seco de PB-T3c (o del contrato de PB-T3b en la rev) por grupos, y comprueba contra el arbol de la rev:
# que cada entrada existe (como archivo o carpeta), que ninguna cubre una ruta del AllowedWriteScope, y que todo archivo de la rev queda
# cubierto por el prohibido o por el permitido (lo que no lo esta se lista). No escribe nada.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO, REV = sys.argv[1:4]
WHICH = sys.argv[4] if len(sys.argv) > 4 else "PB-T3c"


def git(*a):
    return subprocess.run(["git", "-C", REPO, *a], capture_output=True, check=True).stdout.decode("utf-8", "replace")


if WHICH == "PB-T3b":
    c = json.loads(git("show", REV + ":docs/automation/evidence/I-64-pilot/F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json").lstrip("﻿"))
else:
    c = json.load(open(HERE + "/contract-dry-pbt3c.json", encoding="utf-8"))
allowed, forb = c["AllowedWriteScope"], c["ForbiddenWriteScope"]
files = [p for p in git("ls-tree", "-r", "--name-only", REV).split("\n") if p]
dirs = set()
for p in files:
    parts = p.split("/")
    for k in range(1, len(parts)):
        dirs.add("/".join(parts[:k]) + "/")
print(WHICH, "allowed", len(allowed), "forbidden", len(forb))
groups = {}
for f in forb:
    key = "/".join(f.split("/")[:2]) if f.startswith(("src/", "tests/")) else "(raiz)"
    groups.setdefault(key, []).append(f)
for k in sorted(groups):
    print("== %s (%d)" % (k, len(groups[k])))
    print("   " + "; ".join(groups[k]))
missing = [f for f in forb if not (f in dirs or f in files)]
print("== prohibidas que no existen en la rev:", missing)
clash = [(f, a) for f in forb for a in allowed if a == f or (f.endswith("/") and a.startswith(f))]
print("== prohibidas que cubren una permitida:", clash)
cov = lambda p: any(p == f or (f.endswith("/") and p.startswith(f)) for f in forb) or p in allowed
unc = [p for p in files if not cov(p)]
print("== archivos de la rev sin cubrir (ni prohibidos ni permitidos): %d" % len(unc))
byd = {}
for p in unc:
    byd.setdefault("/".join(p.split("/")[:-1]) or "(raiz)", []).append(p.split("/")[-1])
for d in sorted(byd):
    print("   %s/: %d  %s" % (d, len(byd[d]), ", ".join(byd[d][:8]) + (" ..." if len(byd[d]) > 8 else "")))
