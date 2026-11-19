# Redaccion para el repo publico: las rutas locales se sustituyen por marcadores (<scratchpad>, <worktree-de-I-64>, <checkout-principal>, <datos-locales-del-usuario>). El resto no cambia.
import difflib
import json
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace")
REPO = "<checkout-principal>"
HERE = sys.argv[1]
P = "docs/automation/evidence/I-64-pilot/"
T2C = P + "F4B-PBT2C-WORKSPACE-CARRYOVER/coordinator/gate-contract.json"
T1C = P + "F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json"


def show(rev, path):
    return subprocess.run(["git", "-C", REPO, "show", f"{rev}:{path}"], capture_output=True, check=True).stdout


a = json.loads(open(HERE + "/contract-dry-pbt3a.json", "rb").read().decode("utf-8"))
c = json.loads(show("05beabfd", T2C).decode("utf-8-sig"))
t = json.loads(show("05beabfd", T1C).decode("utf-8-sig"))
get = lambda ct, i: next((s["Text"] for s in ct["StopConditions"] if s["Id"] == i), None)
for n in sys.argv[2:]:
    x = get(a, "PBT3A-" + n)
    for lab, ct, pre in (("PB-T1c", t, "PBT1C-"), ("PB-T2c", c, "PBT2C-")):
        y = get(ct, pre + n)
        print("==", n, "vs", lab)
        if y is None:
            print("   (no existe)")
            continue
        sm = difflib.SequenceMatcher(None, y, x, autojunk=False)
        for op, i1, i2, j1, j2 in sm.get_opcodes():
            if op != "equal":
                print("  ", op, repr(y[i1:i2][:300]), "->", repr(x[j1:j2][:300]))
