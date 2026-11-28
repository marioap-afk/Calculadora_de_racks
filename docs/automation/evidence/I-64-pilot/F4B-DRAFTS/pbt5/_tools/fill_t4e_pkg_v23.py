"""Rellena lo [T4e] de I-64-f4b-pbt5-package-v2.3.md con el cierre de PB-T4e (ebd304a3, F4B-E61, §291),
y anota OWNER-01 de PB-T4e en SPB-31, SPB-36 y la trazabilidad. Edita el archivo en su sitio: la v2.3 aún no
se ha entregado. Cada texto viejo, una sola vez; si no, para sin escribir.
"""
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
p = os.path.join(os.path.dirname(here), "I-64-f4b-pbt5-package-v2.3.md")
C = "`ebd304a3773495fa9071de6611936c4c01a7b06c`"
G = "`0eac7911a025a59628b446db8b768035175f96de`"
PAIRS = [
    ("- **[T4e]:** el cierre de PB-T4e (F4B-E61, §291), su SHA, que es el Candidato, y su CI. Los rellena la sesión principal con el SHA del cierre.",
     f"- **El cierre de PB-T4e,** leído por objetos git: F4B-E61 y la §291, en {C}, que es el Candidato. El producto es el GREEN {G}: `src/` y `tests/` del Candidato son los suyos. La CI del GREEN es `45123886125`, 4/4; la de push del Candidato, `45147867500`, estaba en curso al escribir esto. Ya no queda nada marcado [T4e]."),
    ("Empieza con el cierre de PB-T4e **[T4e]**.", "Empieza con el cierre de PB-T4e (F4B-E61, `ebd304a3`)."),
    ("- **La CI exacta:** el Candidato es la punta del cierre de PB-T4e **[T4e]**.",
     "- **La CI exacta:** el Candidato es `ebd304a3`, la punta del cierre de PB-T4e."),
    ("- 16 tareas de F4B están cerradas; PB-T4e cierra ahora **[T4e]**;",
     "- 17 tareas de F4B están cerradas: las 16 de antes y PB-T4e, CLOSED / PASS WITH LIMITATIONS sobre `0eac7911`, intento 1 de 3 (F4B-E61, en `ebd304a3`);"),
    ("(F4B-E60). **[T4e]:** si el cierre registra otros, mandan los suyos;",
     "(F4B-E60), confirmados en el cierre: la CI del GREEN `45123886125`, 4/4 (F4B-E61);"),
    ("- **El Candidato** es la punta del cierre de PB-T4e **[T4e]**: el SHA exacto desde el que se compila el DLL.",
     f"- **El Candidato** es {C}, la punta del cierre de PB-T4e: el SHA exacto desde el que se compila el DLL."),
    ("- **El producto** es el último commit que toca `src/` o `tests/` **[T4e]**.",
     f"- **El producto** es {G}, el GREEN de PB-T4e, el último commit que toca `src/` o `tests/`."),
    ("1. **18 tareas cerradas:** las 16, PB-T4e **[T4e]** y PB-T5, que cierra con el gate.",
     "1. **18 tareas cerradas:** las 17 de §2, PB-T4e incluida, y PB-T5, que cierra con el gate."),
    ("también «## PB-T4e» **[T4e]**.", "también «## PB-T4e», que el cierre de PB-T4e pasó a EJECUTADA (F4B-E61)."),
    ("2. **El cierre de PB-T4e,** F4B-E61 y §291 **[T4e]**. Su SHA es el Candidato.",
     "2. **El cierre de PB-T4e,** hecho: F4B-E61 y §291, en `ebd304a3`, que es el Candidato. La CI de push del Candidato tiene que terminar 4/4 (`45147867500`)."),
    ("- **El cierre de PB-T4e** **[T4e]**.",
     "- **La CI de push del Candidato** (`45147867500`): en curso al escribir esto."),
]


def main():
    text = open(p, encoding="utf-8").read()
    for old, new in PAIRS:
        n = text.count(old)
        if n != 1:
            sys.exit(f"STOP: aparece {n} veces: {old[:70]!r}")
        text = text.replace(old, new)
    lines = text.split("\n")
    note = " Se anota, como información, OWNER-01 de PB-T4e (F4B-E61): con la ventana abierta, la paleta no se repinta y sigue mostrando «Limpio (posiblemente obsoleto)» y el valor de antes, hasta cerrarla"
    for start, end in (("| SPB-31 |", " | S |"), ("| SPB-36 |", " | S+O |")):
        idx = [i for i, ln in enumerate(lines) if ln.startswith(start)]
        if len(idx) != 1 or not lines[idx[0]].endswith(end):
            sys.exit(f"STOP: la fila {start}")
        ln = lines[idx[0]]
        lines[idx[0]] = ln[: -len(end)] + "." + note + end
    trace = "| R-T4-39 | F4B-E60 | SPB-24 y 34 |"
    idx = [i for i, ln in enumerate(lines) if ln == trace]
    if len(idx) != 1:
        sys.exit("STOP: la fila de R-T4-39 en la trazabilidad")
    lines.insert(idx[0] + 1, "| OWNER-01 de PB-T4e: la paleta no se repinta con la ventana abierta (informativo) | F4B-E61 | SPB-31 y SPB-36 |")
    text = "\n".join(lines)
    if "[T4e]**" in text or "**[T4e]" in text:
        sys.exit("STOP: queda una marca [T4e] en negrita")
    open(p, "w", encoding="utf-8", newline="\n").write(text)
    print("rellenado; marcas [T4e] que quedan:", text.count("[T4e]"))


main()
