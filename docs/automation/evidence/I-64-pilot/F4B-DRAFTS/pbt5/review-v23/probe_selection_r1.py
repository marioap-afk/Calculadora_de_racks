"""Recomprobacion 1 de CA-T5: comprueba, sin imprimir rutas ni nombres, la forma de la lista local de la seleccion
de Smoke-2 y que lo que el bloque 13 pasa a estilo.json (indice, papeles, sha256) no lleva nada de Source.

Uso: python -I probe_selection_r1.py <lista local json> <integrity.md extraido de git>
Imprime solo: claves, numero de entradas, si cada Roles son etiquetas de papel conocidas, Exists, el O de Smoke-2
de cada Sha256, si Source parece una ruta absoluta, y si algun trozo de Source aparece en Roles o en Sha256.
"""
import hashlib
import json
import re
import sys

raw = open(sys.argv[1], "rb").read()
print("sha256 de la lista:", hashlib.sha256(raw).hexdigest()[:8], "BOM:", raw[:3] == b"\xef\xbb\xbf")
d = json.loads(raw.decode("utf-8-sig"))
s2 = {v: k for k, v in re.findall(r"^\| (O\d) \| `([0-9A-F]{64})` \|", open(sys.argv[2], encoding="utf-8").read(), re.M)}
role_rx = re.compile(r"^(D1|D2|D3a|D3b|D4|D5-0[1-5])$")
print("claves:", sorted(d.keys()))
print("D1: claves", sorted(d["D1"].keys()), "O de Smoke-2:", s2.get(str(d["D1"]["Sha256"]).upper(), "SIN PAR"))
sel = d["Selection"]
print("entradas:", len(sel))
skip = 0
for i, e in enumerate(sel):
    roles = e["Roles"] if isinstance(e["Roles"], list) else [e["Roles"]]
    roles_ok = all(isinstance(r, str) and role_rx.match(r) for r in roles)
    src = str(e["Source"])
    absolute = bool(re.match(r"^[A-Za-z]:[\\/]", src)) or src.startswith("\\\\")
    parts = [p for p in re.split(r"[\\/. _-]+", src) if len(p) >= 4]
    leak = any(p.lower() in (" ".join(roles) + " " + str(e["Sha256"])).lower() for p in parts)
    skipped = any(r in ("D1", "D2") for r in roles)
    skip += skipped
    print(i, "claves", sorted(e.keys()), "papeles_son_etiquetas", roles_ok, "n_papeles", len(roles), "Exists", e["Exists"],
          "O", s2.get(str(e["Sha256"]).upper(), "SIN PAR"), "Source_absoluta", absolute, "Source_en_papeles_o_sha", leak,
          "el_bloque_13_la_salta", skipped)
print("entradas que cuenta el bloque 13:", len(sel) - skip)
