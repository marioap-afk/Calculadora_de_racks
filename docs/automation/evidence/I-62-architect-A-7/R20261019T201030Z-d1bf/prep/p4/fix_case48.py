"""Corrige la expectativa del caso 48 de selftest-post-review.py (sesión principal, 2026-10-20): el motivo de la lectura directa de la ronda 1
(V14 B.9 L2231-L2233) se sustituye por el de la ronda 2 (AUTOMATION_PLAN 16.8 L616-L617, el mismo texto que esperan los casos 121 y 121b). Solo cambia
esa línea; el SHA-256 anterior se comprueba y la copia anterior se guarda en prep/p4/. Uso: python -I -B fix_case48.py <kit> <p4>
"""
import hashlib
import shutil
import sys
import os

KIT, P4 = sys.argv[1], sys.argv[2]
p = os.path.join(KIT, "selftest-post-review.py")
b = open(p, "rb").read()
assert hashlib.sha256(b).hexdigest().startswith("d188e9ee"), hashlib.sha256(b).hexdigest()
old = b'"lectura directa: docs/initiatives/I-62-proposal-v14.md:2231-2233 no entregadas"],'
new = b'"lectura directa: %s no entregadas fielmente" % ref(AP, 616, 617)],'
assert b.count(old) == 1
shutil.copyfile(p, os.path.join(P4, "selftest-post-review.before-fix48.py"))
nb = b.replace(old, new)
open(p, "wb").write(nb)
print("ok", hashlib.sha256(b).hexdigest(), "->", hashlib.sha256(nb).hexdigest())
