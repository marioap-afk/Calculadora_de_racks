import difflib
import json
import subprocess
import sys

# Uso: python -I cat3c_c11.py <carpeta pbt3> <checkout> <rev>
# PBT3C-C11 del seco frente a PBT3B-C11 del contrato reemitido de PB-T3b (en la rev), con PBT3B -> PBT3C: los tramos distintos, y el bloque
# PBT3x-C11 citado en la v4.7 (con PBT3x -> PBT3C) frente al seco.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO, REV = sys.argv[1:4]
c = json.load(open(HERE + "/contract-dry-pbt3c.json", encoding="utf-8"))
b = json.loads(subprocess.run(["git", "-C", REPO, "show", REV + ":docs/automation/evidence/I-64-pilot/F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json"],
                              capture_output=True, check=True).stdout.decode("utf-8-sig"))
c11 = next(s["Text"] for s in c["StopConditions"] if s["Id"] == "PBT3C-C11")
b11 = next(s["Text"] for s in b["StopConditions"] if s["Id"] == "PBT3B-C11").replace("PBT3B", "PBT3C")
print("longitudes:", len(b11), len(c11))
sm = difflib.SequenceMatcher(None, b11, c11, autojunk=False)
for op, i1, i2, j1, j2 in sm.get_opcodes():
    if op != "equal":
        print("  ", op, repr(b11[max(0, i1 - 50):i2 + 20]), "->", repr(c11[max(0, j1 - 50):j2 + 20]))
pkg = open(HERE + "/I-64-f4b-pbt3-gate-contract-package-v47.md", encoding="utf-8").read()
i = pkg.find("PBT3x-C11")
print("primera mencion de PBT3x-C11 en la v4.7, linea", pkg[:i].count("\n") + 1)
