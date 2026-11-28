"""Control positivo y negativo de los patrones de check_pbt5.py.

Los patrones salen de patterns.local.txt y las muestras de control-samples.local.txt,
junto a este guion. Ninguno de los dos se custodia: llevan nombres que no deben publicarse.
Este guion no lleva ninguna muestra literal (M-19). Sale con 1 si el control falla.
"""
import os
import re
import sys

here = os.path.dirname(os.path.abspath(__file__))
pats = [re.compile(ln.rstrip("\n")) for ln in open(os.path.join(here, "patterns.local.txt"), encoding="utf-8") if ln.strip()]
ok = True
n_hit = n_nohit = 0
for ln in open(os.path.join(here, "control-samples.local.txt"), encoding="utf-8"):
    ln = ln.rstrip("\n")
    if not ln.strip():
        continue
    kind, sample = ln.split("\t", 1)
    found = any(p.search(sample) for p in pats)
    if kind == "HIT":
        n_hit += 1
        if not found:
            print("NO DETECTA la muestra HIT n.", n_hit)
            ok = False
    elif kind == "NOHIT":
        n_nohit += 1
        if found:
            print("FALSO POSITIVO en la muestra NOHIT n.", n_nohit)
            ok = False
    else:
        print("LINEA MAL FORMADA")
        ok = False
print(f"CONTROL {'PASS' if ok else 'FAIL'} (HIT {n_hit}, NOHIT {n_nohit})")
sys.exit(0 if ok else 1)
