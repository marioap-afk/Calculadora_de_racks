"""Tabla «SHA-256 bytes archivo» de las salidas del kit de la ronda 2 (para el README): los archivos de primer nivel de <kit-review2> (salvo
README.md), kit/ (las 43 copias fieles en una sola fila agregada, con su recuento y bytes; sus SHA-256 están en closure.json y en
kit-manifest.json), prep/p2/ y prep/p3/ (sin subdirectorios temporales). Uso: python -I -B file_table.py <kit-review2> [<salida.txt>]
"""
import hashlib
import os
import sys

ROOT = sys.argv[1]
rows = []


def add(rel):
    b = open(os.path.join(ROOT, rel), "rb").read()
    rows.append((hashlib.sha256(b).hexdigest(), len(b), rel.replace(os.sep, "/")))


for n in sorted(os.listdir(ROOT)):
    if os.path.isfile(os.path.join(ROOT, n)) and n != "README.md":
        add(n)
copies = []
for n in sorted(os.listdir(os.path.join(ROOT, "kit"))):
    if n.startswith("fiel__"):
        copies.append(open(os.path.join(ROOT, "kit", n), "rb").read())
    else:
        add(os.path.join("kit", n))
agg = hashlib.sha256(b"".join(hashlib.sha256(c).digest() for c in copies)).hexdigest()
rows.append((agg, sum(len(c) for c in copies), "kit/fiel__*.txt (%d copias fieles; SHA-256 de la concatenación de sus SHA-256 en orden de nombre)" % len(copies)))
for sub in (("prep", "p2"), ("prep", "p3")):
    base = os.path.join(ROOT, *sub)
    for root, dirs, files in os.walk(base):
        dirs[:] = sorted(d for d in dirs if d not in ("vcneg", "__pycache__"))
        for n in sorted(files):
            add(os.path.relpath(os.path.join(root, n), ROOT))
text = "\n".join("%s %9d  %s" % r for r in rows) + "\n"
if len(sys.argv) > 2:
    with open(sys.argv[2], "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
sys.stdout.write(text)
