"""Deriva prep/neg_verify_clone_r3.py del neg_verify_clone_r2.py de la ronda 2: clon y run de la ronda 3, la copia fiel objetivo (el TRX de
RedSha de r3/) y los textos. Uso: python -I -B derive_neg_vc_r3.py <neg_verify_clone_r2.py> <salida>
"""
import sys

src = open(sys.argv[1], encoding="utf-8").read()
R = [('"""Negativa de verify_clone.py del kit de la ronda 2 (decisiones §94, punto 5: repetir los negativos del componente de fidelidad modificado), sobre',
      '"""Negativa de verify_clone.py del kit de la ronda 3 (decisiones §96, punto 6: validar por incrementos según §88; verify_clone.py cambió), sobre'),
     ("byte a byte del run real con el byte 200 de la copia fiel del TRX invertido (XOR 0x01).",
      "byte a byte del run real con el byte 200 de la copia fiel del TRX de RedSha de r3/ invertido (XOR 0x01)."),
     ("Uso: python -I -B neg_verify_clone_r2.py <kit> <run> <p3> <salida.json>", "Uso: python -I -B neg_verify_clone_r3.py <kit> <run> <dir> <salida.json>"),
     ('TARGET = "fiel__docs__automation__evidence__I-62-A7__r2__inputs__fixture-tests-red.trx.txt"',
      'TARGET = "fiel__docs__automation__evidence__I-62-A7__r3__inputs__fixture-tests-red.trx.txt"'),
     ('res = {"Negative": "verify_clone.py (ronda 2) con un byte alterado en la copia fiel %s" % TARGET,',
      'res = {"Negative": "verify_clone.py (ronda 3) con un byte alterado en la copia fiel %s" % TARGET,')]
for a, b in R:
    assert src.count(a) == 1, a[:90]
    src = src.replace(a, b)
assert src.count("r62-arch-a7r2") == 4, src.count("r62-arch-a7r2")   # CONST (clon y run), la sustitución (clon) y git status (clon)
src = src.replace("r62-arch-a7r2", "r62-arch-a7r3")
open(sys.argv[2], "w", encoding="utf-8", newline="\n").write(src)
print("ok")
