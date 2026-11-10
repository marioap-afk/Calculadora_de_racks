"""Sonda desechable 6 (I-68, R-01/R-02/R-03): el diseno RECOMENDADO: definicion minima + Cantidad total como VALOR.
Longitudes que difieren lexica y numericamente, varios racks, Copias y niveles, componente y pieza iguales, descripcion vacia.
Los esperados salen del detalle, no del pivote. Uso: build_probe6.py <directorio_salida>"""
import json
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
src = open(os.path.join(here, "build_probe3.py"), encoding="utf-8").read().split("out = sys.argv[1]")[0]


def patch(text, old, new):
    if old not in text:
        sys.exit("no encontre: " + old[:60])
    return text.replace(old, new)


src = patch(src, "    def cache_field(name):\n",
            "    def cache_field(name):\n        return f'<cacheField name={quoteattr(name)} numFmtId=\"0\"><sharedItems/></cacheField>'\n")
src = patch(src, 'k = len(racks) if name == "Rack" else len(niveles)', "k = 0")
src = patch(src, 'k = len(descs) if name == "Descripcion" else len(lens)', "k = 0")
src = patch(src, "    ri += '<i t=\"grand\"><x/></i>'", "    ri = '<i t=\"grand\"><x/></i>'")
src = patch(src, '<rowItems count="{len(layout) + 1}">', '<rowItems count="1">')
ns = {"__name__": "probe3"}
exec(compile(src, "build_probe3", "exec"), ns)
build = ns["build"]
out = sys.argv[1]

R = [
    ("R-A", "Selectivo", 3, "Componente", "Larguero", "L1", "Larguero 96", 96, 8),
    ("R-A", "Selectivo", 3, "Pieza", "Perfil", "L1", "Larguero 96", 96, 8),
    ("R-A", "Selectivo", 3, "Pieza", "Poste", "C3", "Poste C3", 144, 4),
    ("R-A", "Selectivo", 3, "Pieza", "Poste", "C3", "Poste C3", 9, 2),
    ("R-B", "Selectivo", 2, "Pieza", "Poste", "C3", "Poste C3", 10, 1),
    ("R-B", "Selectivo", 2, "Pieza", "Poste", "C3", "Poste C3", 100, 1),
    ("R-B", "Selectivo", 2, "Pieza", "Poste", "C3", "Poste C3", 144, 5),
    ("R-C", "Cama", 1, "Pieza", "Rodillo", "RD", "Rodillo", 40.5, 30),
    ("R-D", "Cama", 2, "Pieza", "Riel", "RL", "", 12, 5),
]
rows = [list(r) + [r[2] * r[8]] for r in R]


def total(pred=lambda r: True):
    return sum(r[9] for r in rows if pred(r))


exp = {
    "grand": total(),
    "rack_RA": total(lambda r: r[0] == "R-A"),
    "nivel_pieza": total(lambda r: r[3] == "Pieza"),
    "nivel_componente": total(lambda r: r[3] == "Componente"),
    "ra_pieza": total(lambda r: r[0] == "R-A" and r[3] == "Pieza"),
    "sin_rc": total(lambda r: r[0] != "R-C"),
    "poste_144": total(lambda r: r[6] == "Poste C3" and r[7] == 144),
    "poste_longitudes": sorted(r[7] for r in rows if r[6] == "Poste C3" and r[7] != 144) + [144],
}
for name in ("m1", "m2", "m3", "m4"):
    build(os.path.join(out, name + ".xlsx"), rows, pivot=True)
# Variante que simula que el refresco de apertura no ocurre: mismo libro con refreshOnLoad="0".
ns2 = {"__name__": "probe3"}
exec(compile(patch(src, 'refreshOnLoad="1"', 'refreshOnLoad="0"'), "build_probe3b", "exec"), ns2)
ns2["build"](os.path.join(out, "m5_sin_refresh.xlsx"), rows, pivot=True)
with open(os.path.join(out, "expected.json"), "w", encoding="utf-8") as f:
    json.dump(exp, f, ensure_ascii=False)
print(json.dumps(exp))
