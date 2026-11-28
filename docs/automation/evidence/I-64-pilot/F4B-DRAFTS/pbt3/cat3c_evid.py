import ast
import json
import subprocess
import sys

# Uso: python -I cat3c_evid.py <carpeta pbt3> <checkout> <rev>
# (1) ExpectedEvidence[0] del seco de PB-T3c dentro de F4B_EXACT_EVIDENCES del sustituto de nc2 v2 (en git, en la rev), y --task PBT3C en su
# lista; (2) [3], [8] y [14] enteros; (3) el texto de PBT3C-C11 frente al de PBT3B-C11 del contrato reemitido, con el prefijo cambiado.
sys.stdout.reconfigure(encoding="utf-8", errors="replace")
HERE, REPO, REV = sys.argv[1:4]


def git(*a):
    return subprocess.run(["git", "-C", REPO, *a], capture_output=True, check=True).stdout.decode("utf-8", "replace")


c = json.load(open(HERE + "/contract-dry-pbt3c.json", encoding="utf-8"))
src = git("show", REV + ":docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/nc2_exact_whitelist_substitute_f4b_v2.py")
tree = ast.parse(src)
found = {}
for node in ast.walk(tree):
    if isinstance(node, ast.Assign):
        for t in node.targets:
            if isinstance(t, ast.Name) and t.id in ("F4B_EXACT_EVIDENCES", "TASKS", "TASK_CHOICES", "KNOWN_TASKS"):
                try:
                    found[t.id] = ast.literal_eval(node.value)
                except Exception as e:
                    found[t.id] = "no literal: %s" % e
print("nombres hallados:", list(found))
ev = found.get("F4B_EXACT_EVIDENCES")
if isinstance(ev, (list, tuple, set, frozenset)):
    print("F4B_EXACT_EVIDENCES:", len(ev), "| [0] del seco dentro:", c["ExpectedEvidence"][0] in ev)
elif isinstance(ev, dict):
    print("F4B_EXACT_EVIDENCES (dict):", list(ev)[:10], "| [0] en valores:", any(c["ExpectedEvidence"][0] in (v if isinstance(v, (list, tuple, set)) else [v]) for v in ev.values()))
print("--task PBT3C en el texto del sustituto:", "'PBT3C'" in src or '"PBT3C"' in src)
for k in (3, 8, 14):
    print("== [%d]" % k)
    print(c["ExpectedEvidence"][k])
b = json.loads(git("show", REV + ":docs/automation/evidence/I-64-pilot/F4B-PBT3B-FULL-WRITE-RUN/coordinator/gate-contract.json").lstrip("﻿"))
c11 = next(s["Text"] for s in c["StopConditions"] if s["Id"] == "PBT3C-C11")
b11 = next(s["Text"] for s in b["StopConditions"] if s["Id"] == "PBT3B-C11")
print("== C11: igual con el prefijo cambiado:", c11 == b11.replace("PBT3B", "PBT3C"), "| PBT3C en C11:", c11.count("PBT3C"), "| PBT3B en C11:", c11.count("PBT3B"))
print("   las ocurrencias de 'PBT3' en C11:", sorted(set(w for w in c11.replace(",", " ").replace(")", " ").split() if "PBT3" in w)))
