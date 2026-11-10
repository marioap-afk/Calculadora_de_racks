"""Sonda desechable 8 (I-68, R-15/R-16): libros para encadenar en una instancia y variante con acentos, definicion minima + valor.
Reutiliza build_probe6.py. Uso: build_probe8.py <directorio_salida>"""
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
path6 = os.path.join(here, "build_probe6.py")
prelude = open(path6, encoding="utf-8").read().split("\nout = sys.argv[1]")[0]
ns0 = {"__name__": "p6", "__file__": path6}
exec(compile(prelude, "build_probe6", "exec"), ns0)
build = ns0["build"]
out = sys.argv[1]
R = [
    ("R-A", "Selectivo", 3, "Componente", "Larguero", "L1", "Larguero 96", 96, 8),
    ("R-A", "Selectivo", 3, "Pieza", "Perfil", "L1", "Larguero 96", 96, 8),
    ("R-B", "Selectivo", 2, "Pieza", "Poste", "C3", "Poste C3", 144, 5),
    ("R-C", "Cama", 1, "Pieza", "Rodillo", "RD", "Rodillo", 40.5, 30),
]
rows = [list(r) + [r[2] * r[8]] for r in R]
for i in range(1, 7):
    # Contenido distinto por archivo: cambia una cantidad para que ningun libro sea igual a otro.
    variant = [list(r) for r in rows]
    variant[3][8] = 30 + i
    variant[3][9] = 30 + i
    build(os.path.join(out, "c%d.xlsx" % i), variant, pivot=True)
acc = [
    ["R-A", "Selectivo", 1, "Pieza", "P", "P", "Poste", 10, 1, 1],
    ["R-A", "Selectivo", 1, "Pieza", "P", "P", "Póste", 10, 2, 2],
    ["R-A", "Selectivo", 1, "Pieza", "P", "P", "POSTE", 10, 4, 4],
    ["R-A", "Selectivo", 1, "Pieza", "P", "P", "Zeta", 10, 8, 8],
]
build(os.path.join(out, "a1_acentos.xlsx"), acc, pivot=True)
print("OK")
