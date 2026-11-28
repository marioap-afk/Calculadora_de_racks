"""Lista los filtros de UI de RequiredTests del contrato de PB-T4e, por objetos git (solo lectura)."""
import json
import subprocess
import sys

repo = sys.argv[1]
rev = sys.argv[2]
path = "docs/automation/evidence/I-64-pilot/F4B-PBT4-EDIT-ALL-RELOAD/coordinator/gate-contract.json"
raw = subprocess.run(["git", "-C", repo, "show", f"{rev}:{path}"], capture_output=True, check=True).stdout
c = json.loads(raw.decode("utf-8"))
req = c.get("RequiredTests")
if req is None:
    sys.exit("STOP: sin RequiredTests; claves: " + ", ".join(sorted(c.keys())))
print("RequiredTests:", len(req), "claves de la primera fila:", sorted(req[0].keys()))
ui = [r for r in req if "UI.Tests" in json.dumps(r)]
print("filas de UI:", len(ui))
for i, r in enumerate(ui, 1):
    f = r.get("Filter") or r.get("filter")
    print(f"{i:2d}\t{f}")
