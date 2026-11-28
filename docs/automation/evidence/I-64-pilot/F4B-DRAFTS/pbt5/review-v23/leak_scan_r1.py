"""Recomprobacion 1 de CA-T5: busca en la carpeta que barre la emision (--src) cualquier trozo de la lista local de la
seleccion de Smoke-2: cada Source entero, su nombre de archivo, su nombre sin extension y cada carpeta de su ruta
de 5 caracteres o mas (sin la letra de unidad ni carpetas genericas). Solo lee. Nunca imprime lo buscado: por cada
archivo con alguna coincidencia, su ruta relativa y cuantas.

Uso: python -I leak_scan_r1.py <lista local json> <carpeta pbt5>
"""
import json
import ntpath
import os
import re
import sys

d = json.loads(open(sys.argv[1], "rb").read().decode("utf-8-sig"))
srcs = [str(d["D1"]["Source"])] + [str(e["Source"]) for e in d["Selection"]]
generic = {"users", "documents", "documentos", "desktop", "escritorio", "proyectos", "projects", "dibujos"}
needles = set()
for s in srcs:
    needles.add(s.lower())
    base = ntpath.basename(s)
    needles.add(base.lower())
    stem = os.path.splitext(base)[0]
    if len(stem) >= 5:
        needles.add(stem.lower())
    for part in re.split(r"[\\/]", ntpath.dirname(s)):
        if len(part) >= 5 and part.lower() not in generic and not re.fullmatch(r"[A-Za-z]:", part):
            needles.add(part.lower())
print("cadenas buscadas:", len(needles))
root = os.path.abspath(sys.argv[2])
files = hits = 0
for dp, dn, fn in os.walk(root):
    dn.sort()
    for f in sorted(fn):
        full = os.path.join(dp, f)
        rel = os.path.relpath(full, root).replace(os.sep, "/")
        files += 1
        try:
            text = open(full, "rb").read().decode("utf-8", "replace").lower()
        except OSError:
            continue
        n = sum(text.count(x) for x in needles)
        if n:
            hits += 1
            print(f"COINCIDE {rel}: {n}")
print(f"archivos: {files}; con coincidencias: {hits}")
