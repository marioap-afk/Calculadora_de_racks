"""Corrige SOLO la docstring de post-review.py (sesión principal, 2026-10-20): la línea que describía la lectura directa de la ronda 1 (V14 L2231-L2233)
describe ahora la de la ronda 2 (AUTOMATION_PLAN L616-L617, la DirectRead del cierre, por Read en ese archivo o en su copia fiel), como ya dicen la
cabecera (línea 13) y AUDITOR_VERSION. Ninguna línea de código cambia. Uso: python -I -B fix_auditor_doc.py <kit> <p4>
"""
import hashlib
import os
import shutil
import sys

KIT, P4 = sys.argv[1], sys.argv[2]
p = os.path.join(KIT, "post-review.py")
b = open(p, "rb").read()
assert hashlib.sha256(b).hexdigest().startswith("e5626a7e"), hashlib.sha256(b).hexdigest()
old = (b"lines of a closure file or of a premise run file (order.txt), all delivered faithfully by Read in that very file; the direct read\n"
       b"    of V14 L2231-L2233 by Read in the V14 file, with an entry of DirectReads that covers it (a quotation inside A-7 is not a read of the source);\n")
new = (b"lines of a closure file or of a premise run file (order.txt), all delivered faithfully by Read in that very file; the direct read\n"
       b"    of AUTOMATION_PLAN L616-L617 (the closure's DirectRead) by Read in that file or in its faithful copy, with an entry of DirectReads that\n"
       b"    covers it (a quotation inside A-7 is not a read of the source);\n")
assert b.count(old) == 1
shutil.copyfile(p, os.path.join(P4, "post-review.before-doc.py"))
nb = b.replace(old, new)
open(p, "wb").write(nb)
print("ok", hashlib.sha256(b).hexdigest(), "->", hashlib.sha256(nb).hexdigest())
