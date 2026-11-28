"""Comprobacion acotada de PB-T5 (CA-T5): simula, en solo lectura, la redaccion y el control de privacidad
de collect() de emit_pbt5.py sobre la carpeta pbt5, sin ejecutar ese guion y sin escribir nada.

Uso: python -I sim_collect.py <carpeta pbt5>
Imprime, por archivo: el modo que le daria la emision (copia o solo-sha256), el numero de coincidencias
de los patrones locales tras la redaccion (nunca el texto que casa) y la longitud de la ruta de destino.
Tambien cuenta lo que la redaccion no tapa: 'spb1' que sobreviva y rutas con barra o letra de unidad.
"""
import os
import re
import sys

src = os.path.abspath(sys.argv[1])
BS = chr(92)
REDACT = [
    ("D:" + BS + "spb1" + BS + "smoke", "<carpeta-del-Smoke-PB>"),
    ("D:" + BS + "spb1" + BS + "respaldo", "<carpeta-de-respaldo>"),
    ("D:" + "/spb1/smoke", "<carpeta-del-Smoke-PB-con-/>"),
    ("D:" + "/spb1/respaldo", "<carpeta-de-respaldo-con-/>"),
    ("D:" + BS + "spb1", "<raiz-de-trabajo>"),
    ("D:" + "/spb1", "<raiz-de-trabajo-con-/>"),
]
HIST = "docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt5"
SPECIAL = {
    "I-64-f4b-pbt5-package-v2.3.md": "docs/initiatives/I-64-f4b-pbt5-package.md",
    "owner-pbt5-decisions.md": "docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt5-decisions.md",
}
pats = [re.compile(ln.rstrip("\n")) for ln in open(os.path.join(src, "_tools", "patterns.local.txt"), encoding="utf-8") if ln.strip()]
n_copy = n_sha = 0
maxlen = 0
for dp, dn, fn in os.walk(src):
    dn[:] = sorted(d for d in dn if d != "__pycache__")
    for f in sorted(fn):
        full = os.path.join(dp, f)
        rel = os.path.relpath(full, src).replace(os.sep, "/")
        if rel.startswith("_tools/") and rel.endswith(".local.txt"):
            continue
        if f.endswith(".pyc"):
            continue
        target = SPECIAL.get(rel, HIST + "/" + rel)
        maxlen = max(maxlen, len(target))
        data = open(full, "rb").read()
        try:
            text = data.decode("utf-8")
        except UnicodeDecodeError:
            print(f"solo-sha256 (no UTF-8)  {rel}")
            n_sha += 1
            continue
        red = text
        for a, b in REDACT:
            red = red.replace(a, b)
        hits = sum(1 for rx in pats for _ in rx.finditer(red))
        left_spb1 = len(re.findall(r"(?i)spb1", red))
        mode = "copia" if hits == 0 else "solo-sha256"
        if mode == "copia":
            n_copy += 1
        else:
            n_sha += 1
        print(f"{mode:12s} hits={hits:3d} spb1_restante={left_spb1:3d} len_destino={len(target):3d}  {rel}")
print(f"copia={n_copy} solo-sha256={n_sha} ruta_de_destino_mas_larga={maxlen}")
