"""Anade `exit 0` al final de cada bloque ```powershell de pre-run-tonight-v2.3.md que no lo lleve.

Razon (medido en la herramienta PowerShell 7.7.0): con Set-StrictMode -Version Latest, la envoltura de la
herramienta lee $LASTEXITCODE al terminar la orden; si el bloque no ejecuto ninguna orden externa, la variable
no existe y la orden termina con codigo 1 aunque haya escrito su «OK». `exit 0` tras el «OK» lo evita, y un
`throw` anterior sigue saliendo con codigo distinto de 0.
"""
import os
import sys

here = os.path.dirname(os.path.abspath(__file__))
p = os.path.join(os.path.dirname(here), "pre-run-tonight-v2.3.md")
lines = open(p, encoding="utf-8").read().split("\n")
out = []
inside = False
added = 0
for ln in lines:
    s = ln.strip()
    if not inside and s == "```powershell":
        inside = True
        out.append(ln)
        continue
    if inside and s == "```":
        prev = next((x for x in reversed(out) if x.strip()), "")
        if prev.strip() != "exit 0":
            indent = ln[: len(ln) - len(ln.lstrip())]
            out.append(indent + "exit 0")
            added += 1
        inside = False
        out.append(ln)
        continue
    out.append(ln)
if inside:
    sys.exit("STOP: un bloque sin cerrar")
note_old = "- **Lo no verificado de la recomprobación:** si `Get-FileHash` no puede leer una copia que AutoCAD tiene abierta"
note_new = ("- **Nuevo, medido en la herramienta (PowerShell 7.7.0):** con `Set-StrictMode -Version Latest`, la orden termina con código 1 "
            "aunque el bloque escriba «OK», si no ejecutó ninguna orden externa: la envoltura de la herramienta lee `$LASTEXITCODE`, "
            "que entonces no existe. **Todo bloque termina con `exit 0` después de su última línea;** un `throw` anterior sigue saliendo con código distinto de 0.\n"
            + note_old)
text = "\n".join(out)
if text.count(note_old) != 1:
    sys.exit("STOP: la nota no aparece una vez")
text = text.replace(note_old, note_new)
open(p, "w", encoding="utf-8", newline="\n").write(text)
print(f"exit 0 anadido a {added} bloques")
