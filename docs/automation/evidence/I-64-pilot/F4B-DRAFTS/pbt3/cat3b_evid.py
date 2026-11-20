import difflib
import json
import subprocess
import sys

# Uso: python -I cat3b_evid.py <carpeta pbt3> <checkout> [<json del seco>]
# La ExpectedEvidence del seco de PB-T3b frente a la de PB-T3a reemitido (485b5651), entrada por entrada, con el diff de las que cambian.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO = sys.argv[1], sys.argv[2]
DRY = sys.argv[3] if len(sys.argv) > 3 else "contract-dry-pbt3b.json"
b = json.loads(open(HERE + "/" + DRY, encoding="utf-8").read())
a = json.loads(subprocess.run(["git", "-C", REPO, "show", "485b5651:docs/automation/evidence/I-64-pilot/F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json"],
                              capture_output=True, check=True).stdout.decode("utf-8-sig"))
ea, eb = a["ExpectedEvidence"], b["ExpectedEvidence"]
for k, e in enumerate(eb):
    same = [j for j, x in enumerate(ea) if x == e]
    if same:
        print("[%d] == PB-T3a[%d]" % (k, same[0]))
        continue
    best = max(range(len(ea)), key=lambda j: difflib.SequenceMatcher(None, ea[j], e, autojunk=False).ratio())
    r = difflib.SequenceMatcher(None, ea[best], e, autojunk=False)
    if r.ratio() > 0.6:
        print("[%d] ~ PB-T3a[%d] (%.2f):" % (k, best, r.ratio()))
        for op, i1, i2, j1, j2 in r.get_opcodes():
            if op != "equal":
                print("      ", op, repr(ea[best][i1:i2][:220]), "->", repr(e[j1:j2][:220]))
    else:
        print("[%d] propia:" % k, e)
