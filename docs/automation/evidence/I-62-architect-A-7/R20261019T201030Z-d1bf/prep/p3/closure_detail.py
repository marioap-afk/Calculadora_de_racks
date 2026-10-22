"""Detalle de claves de kit/closure.json. Uso: python -I -B closure_detail.py <closure.json> <clave> [<clave>...]
Clave especial path:<ruta> imprime la entrada de CanonicalInputs de esa ruta (sin las líneas largas)."""
import json
import sys

c = json.load(open(sys.argv[1], encoding="utf-8"))
for k in sys.argv[2:]:
    if k.startswith("path:"):
        p = k[5:]
        for r in c["CanonicalInputs"]:
            if r["Path"] == p or (p.endswith("*") and r["Path"].startswith(p[:-1])):
                print(json.dumps(r, ensure_ascii=True, indent=1)[:6000])
    else:
        print("==", k)
        print(json.dumps(c.get(k), ensure_ascii=True, indent=1)[:8000])
