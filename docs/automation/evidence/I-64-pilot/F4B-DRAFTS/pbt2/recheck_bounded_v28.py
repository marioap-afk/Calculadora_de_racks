"""Recomprobacion de la v2.8 (CA-T2b seccion 12 y CA-T2c seccion 11): solo lectura.

Extrae la funcion bounded_check de cada generador (entre 'def bounded_check' y '# ---- bounded_check (fin)'), sin ejecutar el resto
del generador, y la aplica al informe completo, al literal de BOUNDED_RESULT propuesto y a las lineas del paquete v2.8, como hace el
generador (PKG_LINES = el texto del paquete con '\\n' y partido por lineas).
Uso: python -I recheck_bounded_v28.py
"""
import re

PKG = 'I-64-f4b-pbt2-gate-contract-package-v28.md'
CASES = [
    ('PB-T2b', 'make_pbt2b_contract.py', 'independent-review-pbt2b-v23-check.md',
     'EMITIBLE TRAS CORRECCIONES (B 0 / I 3 / M 7; I-01, I-02 e I-03 corregidas en el paquete; recomprobacion v2.4: EMITIBLE, B 0 / I 0 / M 4; '
     'M-08 a M-11 aplicadas en la v2.5; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2)'),
    ('PB-T2c', 'make_pbt2c_contract.py', 'independent-review-pbt2c-v25-check.md',
     'EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3; '
     'M-06 a M-08 aplicadas; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2)'),
]


def read(path):
    return open(path, 'rb').read().decode('utf-8')


pkg_lines = read(PKG).replace('\r\n', '\n').split('\n')
for name, gen, report_path, literal in CASES:
    src = read(gen)
    start = src.index('def bounded_check(')
    end = src.index('# ---- bounded_check (fin)', start)
    ns = {'re': re}
    exec(compile(src[start:end], gen + ':bounded_check', 'exec'), ns)
    report = read(report_path)
    errors = ns['bounded_check'](report, literal, pkg_lines)
    ascii_ok = all(ord(ch) < 0x80 for ch in literal)
    print('%s: generador=%s informe=%s errores=%d ascii=%s' % (name, gen, report_path, len(errors), ascii_ok))
    for e in errors:
        print('   ', e)
    # control negativo: el literal sin la mencion de la ultima recomprobacion tiene que dar error
    neg = literal.replace('; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2)', ')')
    print('   control (sin la mencion de la v2.8): errores=%d' % len(ns['bounded_check'](report, neg, pkg_lines)))
