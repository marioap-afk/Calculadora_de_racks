"""Sonda desechable 7 (I-68, R-06): estado inicial del filtro Nivel y nota visible, sobre la definicion minima.
Solo Nivel lista sus dos literales fijos. Uso: build_probe7.py <directorio_salida>"""
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
src = open(os.path.join(here, "build_probe3.py"), encoding="utf-8").read().split("out = sys.argv[1]")[0]


def patch(text, old, new):
    if old not in text:
        sys.exit("no encontre: " + old[:60])
    return text.replace(old, new)


NOTE = "Con Nivel en (Todas) se suman componentes y sus piezas."
minimal = patch(src, "    def cache_field(name):\n",
                "    def cache_field(name):\n"
                "        if PIEZA and name == 'Nivel':\n"
                "            return '<cacheField name=\"Nivel\" numFmtId=\"0\"><sharedItems count=\"2\"><s v=\"Componente\"/><s v=\"Pieza\"/></sharedItems></cacheField>'\n"
                "        return f'<cacheField name={quoteattr(name)} numFmtId=\"0\"><sharedItems/></cacheField>'\n")
minimal = patch(minimal, 'k = len(racks) if name == "Rack" else len(niveles)', "k = 2 if (PIEZA and name == 'Nivel') else 0")
minimal = patch(minimal, 'k = len(descs) if name == "Descripcion" else len(lens)', "k = 0")
minimal = patch(minimal, "    ri += '<i t=\"grand\"><x/></i>'", "    ri = '<i t=\"grand\"><x/></i>'")
minimal = patch(minimal, '<rowItems count="{len(layout) + 1}">', '<rowItems count="1">')
minimal = patch(minimal, '<pageField fld="{IDX["Nivel"]}" hier="-1"/>', '<pageField fld="{IDX["Nivel"]}" {"item=" + chr(34) + "1" + chr(34) + " " if PIEZA else ""}hier="-1"/>')
minimal = patch(minimal, "cell(\"B2\", \"(Todas)\")", "cell(\"B2\", \"Pieza\" if PIEZA else \"(Todas)\")")
minimal = patch(minimal, "res += '<row r=\"1\">' + cell(\"A1\", \"Rack\") + cell(\"B1\", \"(Todas)\") + \"</row>\"",
                "res += '<row r=\"1\">' + cell(\"A1\", \"Rack\") + cell(\"B1\", \"(Todas)\") + cell(\"D1\", NOTE) + \"</row>\"")
ns = {"__name__": "probe3", "PIEZA": False, "NOTE": NOTE}
exec(compile(minimal, "build_probe3", "exec"), ns)
build = ns["build"]
out = sys.argv[1]
R = [
    ("R-A", "Selectivo", 3, "Componente", "Larguero", "L1", "Larguero 96", 96, 8),
    ("R-A", "Selectivo", 3, "Pieza", "Perfil", "L1", "Larguero 96", 96, 8),
    ("R-A", "Selectivo", 3, "Pieza", "Poste", "C3", "Poste C3", 144, 4),
    ("R-C", "Cama", 1, "Pieza", "Rodillo", "RD", "Rodillo", 40.5, 30),
]
rows = [list(r) + [r[2] * r[8]] for r in R]
ns["PIEZA"] = False
build(os.path.join(out, "n1_todas_con_nota.xlsx"), rows, pivot=True)
ns["PIEZA"] = True
pieza = [r for r in rows if r[3] == "Pieza"]
build(os.path.join(out, "n2_pieza_inicial.xlsx"), rows, pivot=True, prow=None) if False else None
# Celdas previas de n2: solo filas Pieza; la tabla de detalle conserva todas (se reescribe la hoja con el total filtrado).
src2 = patch(minimal, "def build(path, rows, pivot=True, data_rows=None, message=None):",
             "def build(path, rows, pivot=True, data_rows=None, message=None, prow=None):\n    prow = rows if prow is None else prow")
src2 = patch(src2, 'return sorted({r[IDX[name]] for r in rows}, key=', 'return sorted({r[IDX[name]] for r in prow}, key=')
src2 = patch(src2, '    for r in rows:\n        tree[', '    for r in prow:\n        tree[')
ns2 = {"__name__": "probe3", "PIEZA": True, "NOTE": NOTE}
exec(compile(src2, "build_probe3c", "exec"), ns2)
ns2["build"](os.path.join(out, "n2_pieza_inicial.xlsx"), rows, pivot=True, prow=pieza)
ns2["build"](os.path.join(out, "n3_pieza_inicial_vacio.xlsx"), [], pivot=True, data_rows=1)
print("OK")
