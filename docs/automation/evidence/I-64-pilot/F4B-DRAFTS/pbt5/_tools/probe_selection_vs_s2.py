"""Compara los Sha256 completos de la lista local de la seleccion con los O1 a O7 de F4-SMOKE-2/run/integrity.md.
No imprime rutas ni nombres. Uso: python -I probe_selection_vs_s2.py <json> <integrity.md extraido de git>"""
import json
import re
import sys

d = json.loads(open(sys.argv[1], "rb").read().decode("utf-8-sig"))
s2 = dict(re.findall(r"^\| (O\d) \| `([0-9A-F]{64})` \|", open(sys.argv[2], encoding="utf-8").read(), re.M))
print("O en integrity.md:", len(s2))
inv = {v: k for k, v in s2.items()}
print("D1", inv.get(str(d["D1"]["Sha256"]).upper(), "SIN PAR"))
for i, e in enumerate(d["Selection"]):
    print(i, e["Roles"], inv.get(str(e["Sha256"]).upper(), "SIN PAR"), "mayusculas" if e["Sha256"] == str(e["Sha256"]).upper() else "minusculas")
