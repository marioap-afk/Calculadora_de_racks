"""Recomprobacion final de CA-T2c (seccion 13, sobre el cierre de PB-T2b): solo lectura.

Extrae la funcion bounded_check de make_pbt2c_contract.py (entre 'def bounded_check' y '# ---- bounded_check (fin)'), sin ejecutar el
resto del generador, y la aplica al informe completo, al literal final propuesto y a las lineas de la v2.9 final.
Controles: el literal de la seccion 12 sin la mencion de la 13 sigue pasando (la mencion de la v2.9 coincide), y el informe con el titulo
literal del encargo ('## §13. Recomprobacion final sobre el cierre de PB-T2b') tiene que dar el error de forma del encabezado.
Uso: python -I recheck_bounded_final.py
"""
import re

PKG = 'I-64-f4b-pbt2-gate-contract-package-v29.md'
GEN = 'make_pbt2c_contract.py'
REPORT = 'independent-review-pbt2c-v25-check.md'
LITERAL = ('EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, B 0 / I 0 / M 3; '
           'M-06 a M-08 aplicadas; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2; M-09 aplicada en la v2.9; '
           'recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2; M-11 y M-12 aplicadas en la v2.9; '
           'recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2 (seccion 13, final sobre el cierre de PB-T2b))')
SEC = chr(0xA7)
MINE = '## ' + SEC + '13. Recomprobaci' + chr(0xF3) + 'n sobre la v2.9, final sobre el cierre de PB-T2b'
ASKED = '## ' + SEC + '13. Recomprobaci' + chr(0xF3) + 'n final sobre el cierre de PB-T2b'


def read(path):
    return open(path, 'rb').read().decode('utf-8')


pkg_lines = read(PKG).replace('\r\n', '\n').split('\n')
src = read(GEN)
start = src.index('def bounded_check(')
end = src.index('# ---- bounded_check (fin)', start)
ns = {'re': re}
exec(compile(src[start:end], GEN + ':bounded_check', 'exec'), ns)
report = read(REPORT)
assert report.count(MINE) == 1, 'el encabezado de la seccion 13'
errors = ns['bounded_check'](report, LITERAL, pkg_lines)
print('PB-T2c final: informe=%s paquete=%s errores=%d ascii=%s' % (REPORT, PKG, len(errors), all(ord(ch) < 0x80 for ch in LITERAL)))
for e in errors:
    print('   ', e)
asked = ns['bounded_check'](report.replace(MINE, ASKED), LITERAL, pkg_lines)
print('   control (el titulo literal del encargo): errores=%d %s' % (len(asked), asked[:1]))
neg = LITERAL.replace('recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2', 'recomprobacion v2.9 EMITIBLE')
print('   control (sin ninguna mencion exacta de la v2.9): errores=%d' % len(ns['bounded_check'](report, neg, pkg_lines)))
