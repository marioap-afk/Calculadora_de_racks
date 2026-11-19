"""Recomprobacion de la v2.9 (CA-T2c seccion 12): solo lectura.

Extrae la funcion bounded_check de make_pbt2c_contract.py (entre 'def bounded_check' y '# ---- bounded_check (fin)'), sin ejecutar el
resto del generador, y la aplica al informe completo de CA-T2c, al literal de BOUNDED_RESULT propuesto y a las lineas del paquete v2.9.
Dos controles: el literal sin la mencion de la v2.9 y el literal de la seccion 11 tienen que dar error.
Uso: python -I recheck_bounded_v29.py
"""
import re

PKG = 'I-64-f4b-pbt2-gate-contract-package-v29.md'
GEN = 'make_pbt2c_contract.py'
REPORT = 'independent-review-pbt2c-v25-check.md'
LITERAL = ('EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3; '
           'M-06 a M-08 aplicadas; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2; M-09 aplicada en la v2.9; '
           'recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2)')
LITERAL_S11 = ('EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3; '
               'M-06 a M-08 aplicadas; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2)')


def read(path):
    return open(path, 'rb').read().decode('utf-8')


pkg_lines = read(PKG).replace('\r\n', '\n').split('\n')
src = read(GEN)
start = src.index('def bounded_check(')
end = src.index('# ---- bounded_check (fin)', start)
ns = {'re': re}
exec(compile(src[start:end], GEN + ':bounded_check', 'exec'), ns)
report = read(REPORT)
errors = ns['bounded_check'](report, LITERAL, pkg_lines)
print('PB-T2c: informe=%s paquete=%s errores=%d ascii=%s' % (REPORT, PKG, len(errors), all(ord(ch) < 0x80 for ch in LITERAL)))
for e in errors:
    print('   ', e)
neg1 = LITERAL.replace('; recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2)', ')')
print('   control (sin la mencion de la v2.9): errores=%d' % len(ns['bounded_check'](report, neg1, pkg_lines)))
print('   control (el literal de la seccion 11): errores=%d' % len(ns['bounded_check'](report, LITERAL_S11, pkg_lines)))
