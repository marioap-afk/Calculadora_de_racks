import hashlib
import json
import subprocess
import sys

sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE = sys.argv[1]
REPO = sys.argv[2]  # el checkout principal, por argumento (sin ruta local en el guion)
T2C = "docs/automation/evidence/I-64-pilot/F4B-PBT2C-WORKSPACE-CARRYOVER/coordinator/gate-contract.json"

raw = open(HERE + "/contract-dry-pbt3a.json", "rb").read()
print("dry sha256", hashlib.sha256(raw).hexdigest().upper(), "bytes", len(raw))
a = json.loads(raw.decode("utf-8"))
c = json.loads(subprocess.run(["git", "-C", REPO, "show", "05beabfd:" + T2C], capture_output=True, check=True).stdout.decode("utf-8-sig"))
ev = a["ExpectedEvidence"]
print("evidence entries:", len(ev))
for k, e in enumerate(ev):
    print("[%d]" % k, e[:160].replace("\n", " "), "..." if len(e) > 160 else "")
print()
print("== [8] completa:")
print(ev[8])
print()
print("== PB-T2c, la entrada de la verificacion con una reparacion:")
for e in c["ExpectedEvidence"]:
    if e.startswith("Con una reparacion del RED, la verificacion"):
        print(e)
print()
print("Objective con v4.4:", "v4.4" in a["Objective"], "| v4.3 en Objective:", "v4.3" in a["Objective"])
pk = [x for x in a["Authorities"] if x["Path"].endswith("I-64-f4b-pbt3-gate-contract-package.md")][0]
print("autoridad del paquete:", pk["Section"][:400])
stops = {s["Id"]: s["Text"] for s in a["StopConditions"]}
sc = {s["Id"]: s["Text"] for s in c["StopConditions"]}
print("17 estandar == PB-T2c:", all(stops[k] == sc[k] for k in sc if not k.startswith("PBT2C")))
print("BeamDepth en el contrato:", "BeamDepth" in raw.decode("utf-8"), "| PalletDepth:", "PalletDepth" in raw.decode("utf-8"))
