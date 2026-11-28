import ast
import json
import subprocess
import sys

# Uso: python -I cat3b_nc2.py <carpeta pbt3> <script nc2 v2> <checkout> [<json del seco>]
# La propiedad 8 del sustituto de nc2 v2: ExpectedEvidence del seco de PB-T3b frente a la lista cerrada F4B_EXACT_EVIDENCES (sin ejecutar el script:
# se lee su AST). Tambien la entrada de PB-T3a en 485b5651.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, SCRIPT, REPO = sys.argv[1], sys.argv[2], sys.argv[3]
DRY = sys.argv[4] if len(sys.argv) > 4 else "contract-dry-pbt3b.json"
tree = ast.parse(open(SCRIPT, encoding="utf-8").read())
lits = None
for node in ast.walk(tree):
    if isinstance(node, ast.Assign) and any(isinstance(t, ast.Name) and t.id.startswith("F4B_EXACT_EVIDENCE") for t in node.targets):
        try:
            val = ast.literal_eval(node.value)
        except Exception:
            val = None
        print("asignacion:", [t.id for t in node.targets if isinstance(t, ast.Name)], type(val).__name__, len(val) if hasattr(val, "__len__") else "")
        if isinstance(val, (tuple, list)):
            lits = list(val)
b = json.loads(open(HERE + "/" + DRY, encoding="utf-8").read())
a = json.loads(subprocess.run(["git", "-C", REPO, "show", "485b5651:docs/automation/evidence/I-64-pilot/F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json"],
                              capture_output=True, check=True).stdout.decode("utf-8-sig"))
for k, l in enumerate(lits or []):
    print("literal %d (%d car.): %s" % (k, len(l), l[:160]))
    print("   == PB-T3b ExpectedEvidence[0]:", l == b["ExpectedEvidence"][0], "| en alguna entrada de PB-T3b:", l in b["ExpectedEvidence"],
          "| == PB-T3a [0]:", l == a["ExpectedEvidence"][0])
print("PB-T3b [0] == PB-T3a [0]:", b["ExpectedEvidence"][0] == a["ExpectedEvidence"][0])
for name in ("ID", "TASKS", "ALLOWED_TASKS"):
    for node in ast.walk(tree):
        if isinstance(node, ast.Assign) and any(isinstance(t, ast.Name) and t.id == name for t in node.targets):
            try:
                print(name, "=", ast.literal_eval(node.value))
            except Exception:
                print(name, "= (no literal)")
