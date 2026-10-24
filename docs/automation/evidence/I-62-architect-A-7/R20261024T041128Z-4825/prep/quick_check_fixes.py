"""Comprobación rápida (validación incremental) de los dos casos corregidos de la autoprueba de la ronda 4 antes de repetirla entera: los casos de la
compuerta 11 (owner_cases, con O03 sobre §101) y los de fidelidad (fidelity_cases, con F00 sobre decisiones y A-7). No sustituye a la autoprueba
completa, que se repite después. Uso: python -X utf8 -B quick_check_fixes.py <kit>
"""
import importlib.util
import json
import os
import sys

KIT = sys.argv[1]
sys.path.insert(0, KIT)
spec = importlib.util.spec_from_file_location("selftest_post_review", os.path.join(KIT, "selftest-post-review.py"))
ST = importlib.util.module_from_spec(spec)
spec.loader.exec_module(ST)
p = ST.load_auditor()
ST.init_globals(p)
og = ST.owner_cases()
fc = ST.fidelity_cases()
bad = [c for c in og + fc if c["Result"] != "PASS"]
print(json.dumps({"OwnerCases": "%d/%d" % (sum(1 for c in og if c["Result"] == "PASS"), len(og)),
                  "FidelityCases": "%d/%d" % (sum(1 for c in fc if c["Result"] == "PASS"), len(fc)),
                  "O03": [c["Result"] for c in og if c["Case"].startswith("O03")], "F00": [c["Result"] for c in fc if c["Case"].startswith("F00")],
                  "Failing": [c["Case"] for c in bad]}, ensure_ascii=False))
ST.safe_remove_tmproot()
sys.exit(1 if bad else 0)
