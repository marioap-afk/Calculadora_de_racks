import difflib
import json
import re
import subprocess
import sys

# Uso: python -I cat3b_c11.py <carpeta pbt3> <checkout> [<json del seco>]
# PBT3B-C11 entero, frente a PBT3A-C11 (485b5651) con el prefijo cambiado, por frases; y las entradas F4B-E40 y F4B-E43 de f4b-dispositions.md.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO = sys.argv[1], sys.argv[2]
DRY = sys.argv[3] if len(sys.argv) > 3 else "contract-dry-pbt3b.json"


def show(rev, path):
    return subprocess.run(["git", "-C", REPO, "show", rev + ":" + path], capture_output=True, check=True).stdout.decode("utf-8-sig", "replace")


b = json.loads(open(HERE + "/" + DRY, encoding="utf-8").read())
a = json.loads(show("485b5651", "docs/automation/evidence/I-64-pilot/F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json"))
cb = next(s["Text"] for s in b["StopConditions"] if s["Id"] == "PBT3B-C11")
ca = next(s["Text"] for s in a["StopConditions"] if s["Id"] == "PBT3A-C11").replace("PBT3A", "PBT3B").replace("PB-T3a", "PB-T3b")
print("== PBT3B-C11 (%d car.)" % len(cb))
print(cb)
print()
sa = re.split(r"(?<=[;:)])\s+", ca)
sb = re.split(r"(?<=[;:)])\s+", cb)
for line in difflib.unified_diff(sa, sb, "PBT3A-C11 (prefijo PBT3B)", "PBT3B-C11", lineterm="", n=0):
    print(line[:400])
print()
disp = show("c40eea4a", "docs/automation/evidence/I-64-pilot/F4B-DRAFTS/f4b-dispositions.md").replace("\r\n", "\n")
for e in ("F4B-E40", "F4B-E43"):
    i = disp.find("\n## %s. " % e)
    j = disp.find("\n## F4B-E", i + 1)
    print("=====", e)
    print(disp[i:j if j > 0 else len(disp)][:6000])
