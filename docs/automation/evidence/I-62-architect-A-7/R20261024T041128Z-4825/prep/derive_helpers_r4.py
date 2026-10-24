"""Deriva los auxiliares de congelación del kit de la ronda 4 (decisiones §98, punto 6: reutilizar componentes; validar por incrementos) desde los
de la ronda 3: neg_verify_clone_r4.py (clon y run de la ronda 4; la copia fiel objetivo, el TRX de RedSha de r4/), regen_faithful_r4.py (textos) y
privacy_scan_r4.py (la copia byte a byte de la ronda previa: a7-r3.md). check_dry.py se copia sin cambios.
Uso: python -I -B derive_helpers_r4.py <prep de la ronda 3> <prep de la ronda 4>
"""
import os
import sys

P3, P4 = sys.argv[1:3]


def derive(src_name, dst_name, R, glob=()):
    src = open(os.path.join(P3, src_name), encoding="utf-8").read()
    for a, b in R:
        assert src.count(a) == 1, (src_name, a[:90], src.count(a))
        src = src.replace(a, b)
    for a, b, n in glob:
        assert src.count(a) == n, (src_name, a, src.count(a))
        src = src.replace(a, b)
    open(os.path.join(P4, dst_name), "w", encoding="utf-8", newline="\n").write(src)
    print(dst_name, "ok")


derive("neg_verify_clone_r3.py", "neg_verify_clone_r4.py", [
    ('"""Negativa de verify_clone.py del kit de la ronda 3 (decisiones §96, punto 6: validar por incrementos según §88; verify_clone.py cambió), sobre',
     '"""Negativa de verify_clone.py del kit de la ronda 4 (decisiones §98, punto 6: validar por incrementos; verify_clone.py cambió), sobre'),
    ("byte a byte del run real con el byte 200 de la copia fiel del TRX de RedSha de r3/ invertido (XOR 0x01).",
     "byte a byte del run real con el byte 200 de la copia fiel del TRX de RedSha de r4/ invertido (XOR 0x01)."),
    ("Uso: python -I -B neg_verify_clone_r3.py <kit> <run> <dir> <salida.json>", "Uso: python -I -B neg_verify_clone_r4.py <kit> <run> <dir> <salida.json>"),
    ('TARGET = "fiel__docs__automation__evidence__I-62-A7__r3__inputs__fixture-tests-red.trx.txt"',
     'TARGET = "fiel__docs__automation__evidence__I-62-A7__r4__inputs__fixture-tests-red.trx.txt"'),
    ('res = {"Negative": "verify_clone.py (ronda 3) con un byte alterado en la copia fiel %s" % TARGET,',
     'res = {"Negative": "verify_clone.py (ronda 4) con un byte alterado en la copia fiel %s" % TARGET,')],
    [("r62-arch-a7r3", "r62-arch-a7r4", 4)])
derive("regen_faithful.py", "regen_faithful_r4.py", [
    ('"""Reproducción de las copias fieles del kit de la ronda 3 desde el repositorio (sesión principal; derivado del regen_faithful.py del kit de la\nronda 2).',
     '"""Reproducción de las copias fieles del kit de la ronda 4 desde el repositorio (sesión principal; derivado del regen_faithful.py del kit de la\nronda 3).'),
    ("El número de copias es el del cierre (42 en la ronda 3).", "El número de copias es el del cierre (43 en la ronda 4)."),
    ("Uso: python -I -B regen_faithful.py <kit> <clon> <salida.json>", "Uso: python -I -B regen_faithful_r4.py <kit> <clon> <salida.json>"),
    ('"Schema": "a7r3-faithful-regeneration/1"', '"Schema": "a7r4-faithful-regeneration/1"')])
derive("privacy_scan.py", "privacy_scan_r4.py", [
    ('"""Comprobación de privacidad (solo lectura) de las salidas del kit de la ronda 3:', '"""Comprobación de privacidad (solo lectura) de las salidas del kit de la ronda 4:'),
    ("(las copias fieles fiel__*.txt y a7-r2.md)", "(las copias fieles fiel__*.txt y a7-r3.md)"),
    ('EXEMPT = lambda n: n.startswith("fiel__") or n == "a7-r2.md"', 'EXEMPT = lambda n: n.startswith("fiel__") or n == "a7-r3.md"')])
