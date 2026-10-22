"""Decisiones §95, punto 4 («Repetir los controles afectados por la fijación»): mutación DIRIGIDA de owner_gate.py tras fijar §95 (decisiones §88,
punto 3: los mutantes del componente cambiado). Usa el arnés congelado del kit (kit/mutation-post-review.py, importado como módulo, sin modificarlo):
la misma línea base (lista completa de casos de selftest-post-review.py sin L00, con la autoprueba de fidelidad), los mismos sitios (sites), el mismo
mutante (mutant_source) y el mismo juicio (judge, con su memo); solo se recorre owner_gate.py, ya fijado. La batería completa (207/207) se ejecutó con
owner_gate.py pendiente (3d420a9f); entre los dos módulos solo cambian las tres constantes AUTH_*, ninguna línea de regla.
Uso: python -X utf8 -B mutation_owner_gate.py <kit> <salida.json>
"""
import hashlib
import importlib.util
import json
import os
import sys

KIT, OUT = sys.argv[1], os.path.abspath(sys.argv[2])
sys.path.insert(0, KIT)
spec = importlib.util.spec_from_file_location("mutation_post_review", os.path.join(KIT, "mutation-post-review.py"))
MP = importlib.util.module_from_spec(spec)
spec.loader.exec_module(MP)
ST, SF = MP.ST, MP.SF
NAME = "owner_gate.py"
mut_dir = os.path.join(ST.TMPROOT, "mut")
os.makedirs(mut_dir, exist_ok=True)
failing, n_cases = MP.judge(ST.load_auditor())
fid_failing = [c["Case"] for c in SF.cases(SF.load()) if c["Result"] != "PASS"]
og_sha = hashlib.sha256(open(os.path.join(KIT, NAME), "rb").read()).hexdigest()
doc = {"Mutation": "DIRIGIDA: owner_gate.py fijado en §95 (decisiones §95, punto 4; §88, punto 3), con el arnés congelado del kit",
       "Harness": {"mutation-post-review.py": hashlib.sha256(open(os.path.join(KIT, "mutation-post-review.py"), "rb").read()).hexdigest(),
                   "selftest-post-review.py": hashlib.sha256(open(os.path.join(KIT, "selftest-post-review.py"), "rb").read()).hexdigest()},
       "Pins": ST.load(NAME.replace(".py", "_pinned"), os.path.join(KIT, NAME)).pinned_authorization(),
       "Baseline": {"Cases": n_cases, "Failing": failing, "FidelityUnitFailing": fid_failing, "AllPass": not failing and not fid_failing},
       "Module": {"Name": NAME, "Sha256": og_sha}, "Mutants": []}
src, found = MP.sites(NAME)
for k, (i, a, b) in enumerate(found):
    path = os.path.join(mut_dir, "owner_gate.m%03d.py" % k)
    open(path, "w", encoding="utf-8", newline="\n").write(MP.mutant_source(src, i, a, b))
    try:
        pr = ST.load_auditor()
        pr.OG = ST.load("owner_gate_mutant", path)
        fails, _ = MP.judge(pr)
    except Exception as e:
        fails = ["EXCEPTION %s: %s" % (type(e).__name__, str(e)[:200])]
    doc["Mutants"].append({"Line": i + 1, "Code": src[i].strip()[:150], "Killed": bool(fails), "KilledBy": fails[:4], "KillingCases": len(fails)})
    print("%-14s %4d %-6s %s" % (NAME, i + 1, "KILLED" if fails else "ALIVE", fails[:2] if fails else src[i].strip()[:80]))
    sys.stdout.flush()
    os.remove(path)
doc["Module"]["Sites"] = len(found)
doc["Module"]["Killed"] = sum(1 for m in doc["Mutants"] if m["Killed"])
doc["Survivors"] = [m for m in doc["Mutants"] if not m["Killed"]]
doc["AllKilled"] = doc["Baseline"]["AllPass"] and not doc["Survivors"] and len(found) == 19
text = ST.USER_RX.sub("%USERPROFILE%", json.dumps(doc, ensure_ascii=False, indent=1))
with open(OUT, "w", encoding="utf-8", newline="\n") as f:
    f.write(text + "\n")
print(json.dumps({"Baseline": doc["Baseline"]["AllPass"], "Sites": len(found), "Killed": doc["Module"]["Killed"], "AllKilled": doc["AllKilled"]}))
ST.safe_remove_tmproot()
sys.exit(0 if doc["AllKilled"] else 1)
