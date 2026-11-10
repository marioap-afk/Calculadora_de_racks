"""Sonda desechable 5 (I-68): pivote MINIMO. La definicion no lista valores: Excel los reconstruye al abrir (refreshOnLoad).
Pregunta: evita esto las tres formas medidas de archivo que Excel no abre? Uso: build_probe5.py <directorio_salida>"""
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
src = open(os.path.join(here, "build_probe3.py"), encoding="utf-8").read().split("out = sys.argv[1]")[0]


def patch(text, old, new):
    if old not in text:
        sys.exit("no encontre: " + old[:60])
    return text.replace(old, new)


src = patch(src, "def build(path, rows, pivot=True, data_rows=None, message=None):",
            "def build(path, rows, pivot=True, data_rows=None, message=None, prow=None):\n    prow = rows if prow is None else prow")
src = patch(src, 'return sorted({r[IDX[name]] for r in rows}, key=', 'return sorted({r[IDX[name]] for r in prow}, key=')
src = patch(src, '    for r in rows:\n        tree[', '    for r in prow:\n        tree[')
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


def mk(names, lens=None):
    return [["R-A", "Selectivo", 1, "Pieza", "Cat", "P", x, (lens[i] if lens else 10), i + 1, i + 1] for i, x in enumerate(names)]


base = ["Poste", "Zeta", "Ñandú", "Nube", "10 perfil", "9 perfil", "_guion"]
# t6: los tres casos que rompian (enteros, mayusculas en filas y en Rack); celdas previas unificadas como Excel
rows6 = mk(base + ["POSTE"])
rows6[1][0] = "r-a"
prow6 = [list(r) for r in rows6]
for r in prow6:
    if r[6] == "POSTE":
        r[6] = "Poste"
build(os.path.join(out, "t6_min_casos_rotos.xlsx"), rows6, prow=prow6)
# t7: celdas previas con MAS filas que el resultado real (no unificadas): deben desaparecer al refrescar
build(os.path.join(out, "t7_min_celdas_sobrantes.xlsx"), rows6)
# t8: con Longitud decimal y varias longitudes por descripcion
build(os.path.join(out, "t8_min_longitudes.xlsx"), mk(["A", "A", "B"], [40.5, 48, 40.5]))
print("OK")
