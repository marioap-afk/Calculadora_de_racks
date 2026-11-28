"""Muestra solo la FORMA de la lista local de la seleccion de Smoke-2: claves, tipos, Roles, Exists y prefijos de Sha256.
No imprime ninguna ruta ni ningun nombre de archivo (Source no se muestra). Uso: python -I probe_selection_shape.py <json>"""
import hashlib
import json
import sys

p = sys.argv[1]
raw = open(p, "rb").read()
print("sha256", hashlib.sha256(raw).hexdigest())
d = json.loads(raw.decode("utf-8-sig"))
print("claves", sorted(d.keys()))
d1 = d.get("D1", {})
print("D1 claves", sorted(d1.keys()), "Sha256", str(d1.get("Sha256", ""))[:8])
sel = d.get("Selection", [])
print("Selection", type(sel).__name__, len(sel))
for i, e in enumerate(sel, 1):
    print(i, "claves", sorted(e.keys()), "Roles", e.get("Roles"), "Exists", e.get("Exists"), "Sha256", str(e.get("Sha256", ""))[:8],
          "Source-es-texto", isinstance(e.get("Source"), str))
