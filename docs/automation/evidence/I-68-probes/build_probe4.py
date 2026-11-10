"""Sonda desechable 4 (I-68): que textos de fila rompen la apertura y como agrupa/ordena Excel al refrescar.
Uso: build_probe4.py <directorio_salida>. Reutiliza build() de build_probe3.py, separando filas de hoja y filas del pivote."""
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
src = open(os.path.join(here, "build_probe3.py"), encoding="utf-8").read().split("out = sys.argv[1]")[0]
src = src.replace("def build(path, rows, pivot=True, data_rows=None, message=None):",
                  "def build(path, rows, pivot=True, data_rows=None, message=None, prow=None):\n    prow = rows if prow is None else prow")
src = src.replace('return sorted({r[IDX[name]] for r in rows}, key=', 'return sorted({r[IDX[name]] for r in prow}, key=')
src = src.replace('    for r in rows:\n        tree[', '    for r in prow:\n        tree[')
old = "containsNumber=\"1\" '\n                    f'minValue="
new = "containsNumber=\"1\" ' + ('containsInteger=\"1\" ' if INT_ATTR and all(isinstance(x, int) for x in lens) else '') +\n                    f'minValue="
if old not in src:
    sys.exit("no encontre el punto de parche")
src = src.replace(old, new)
ns = {"__name__": "probe3", "INT_ATTR": True}
exec(compile(src, "build_probe3", "exec"), ns)
build = ns["build"]
out = sys.argv[1]
# Control: mismos datos que t1 pero SIN containsInteger (longitudes todas enteras)
ns["INT_ATTR"] = False
build(os.path.join(out, "t0_control_sin_containsInteger.xlsx"),
      [["R-A", "Selectivo", 1, "Pieza", "Cat", "P", x, 10, i + 1, i + 1] for i, x in enumerate(["Poste", "Zeta"])])
ns["INT_ATTR"] = True
# Tabla con solo encabezado, abierta despues de otros archivos para descartar efecto de arranque
build(os.path.join(out, "t9_header_only.xlsx"), [], pivot=False, data_rows=0, message="Sin partidas")


def mk(names):
    return [["R-A", "Selectivo", 1, "Pieza", "Cat", "P", x, 10, i + 1, i + 1] for i, x in enumerate(names)]


base = ["Poste", "Zeta", "Ñandú", "Nube", "10 perfil", "9 perfil", "_guion"]
# t1: sin variantes de mayusculas ni acentos
build(os.path.join(out, "t1_base.xlsx"), mk(base))
# t2: variantes solo de acento
build(os.path.join(out, "t2_acentos.xlsx"), mk(base + ["Póste"]))
# t3: variante de mayusculas en la hoja y tambien listada en el pivote (lo que rompio)
build(os.path.join(out, "t3_mayus_listada.xlsx"), mk(base + ["POSTE"]))
# t4: variante de mayusculas en la hoja, pero el pivote la lista una sola vez y suma ambas
rows = mk(base + ["POSTE"])
prow = [list(r) for r in rows]
for r in prow:
    if r[6] == "POSTE":
        r[6] = "Poste"
build(os.path.join(out, "t4_mayus_unificada.xlsx"), rows, prow=prow)
# t5: rack con variante de mayusculas (campo de filtro) listada dos veces
rows5 = mk(base)
rows5[1][0] = "r-a"
build(os.path.join(out, "t5_rack_mayus_listada.xlsx"), rows5)
print("OK")
