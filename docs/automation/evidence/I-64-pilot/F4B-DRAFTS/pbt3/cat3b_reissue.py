import difflib
import hashlib
import json
import subprocess
import sys

# Uso: python -I cat3b_reissue.py <carpeta pbt3> <checkout> <rev del contrato emitido>
# El seco de la reemision de PB-T3b frente al contrato EMITIDO (F4B-E44, en git), campo a campo y entrada a entrada; C13 entero.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO, REV = sys.argv[1:4]
P = "docs/automation/evidence/I-64-pilot/F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json"
ra = subprocess.run(["git", "-C", REPO, "show", REV + ":" + P], capture_output=True, check=True).stdout
rb = open(HERE + "/contract-dry-pbt3b-reissue.json", "rb").read()
print("emitido", REV, hashlib.sha256(ra).hexdigest().upper()[:16], len(ra), "| reemision", hashlib.sha256(rb).hexdigest().upper()[:16], len(rb),
      "| ASCII:", all(c < 128 for c in rb))
a, b = json.loads(ra.decode("utf-8-sig")), json.loads(rb.decode("utf-8"))
print("claves iguales:", list(a) == list(b))
for k in a:
    if a[k] == b.get(k):
        continue
    print("== cambia:", k)
    if isinstance(a[k], list):
        sa = [json.dumps(x, ensure_ascii=False, sort_keys=True) for x in a[k]]
        sb = [json.dumps(x, ensure_ascii=False, sort_keys=True) for x in b[k]]
        print("   entradas:", len(sa), "->", len(sb))
        for line in difflib.unified_diff(sa, sb, lineterm="", n=0):
            if not line.startswith(("---", "+++")):
                print("   ", line[:420])
    else:
        sm = difflib.SequenceMatcher(None, str(a[k]), str(b[k]), autojunk=False)
        for op, i1, i2, j1, j2 in sm.get_opcodes():
            if op != "equal":
                print("   ", op, repr(str(a[k])[i1:i2][:200]), "->", repr(str(b[k])[j1:j2][:200]))
ca = next(s["Text"] for s in a["StopConditions"] if s["Id"] == "PBT3B-C13")
cb = next(s["Text"] for s in b["StopConditions"] if s["Id"] == "PBT3B-C13")
print("== C13, diff:")
sm = difflib.SequenceMatcher(None, ca, cb, autojunk=False)
for op, i1, i2, j1, j2 in sm.get_opcodes():
    if op != "equal":
        print("   ", op, repr(ca[max(0, i1 - 30):i2 + 10]), "->", repr(cb[max(0, j1 - 30):j2 + 10]))
print("== C13 de la reemision:")
print(cb)
