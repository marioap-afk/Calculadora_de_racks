"""PB-T4c (paquete v5.0): pasa la lectura RR-3 de la cadena (pbt4ccc/pbt4c_rr3_reading.py, rr3_table_check, sin tocarla) sobre la tabla de RR-3
de §6.0 de la v5.0 y su §5.3, para las cinco clases de las CT y los cuatro equivalentes (que no tienen fila: sin-fila). Comprueba ademas que las
lineas que lee de cada clase son, en su orden y sin repetir, las de sus CT en pbt4c_ct_table_v50.py (mas los literales propios de la fila de
CT-15). Solo lee; escribe en stdout (UTF-8, LF).

Uso: python -I check_rr3_v50.py
"""
import hashlib
import importlib.util
import os
import sys

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
sys.dont_write_bytecode = True  # ni __pycache__ en pbt4ccc/ (el lector es de la cadena) ni en pbt4/
HERE = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.dirname(HERE)
READER = os.path.normpath(os.path.join(PB4, '..', 'pbt4ccc', 'pbt4c_rr3_reading.py'))
PKG = os.path.join(PB4, 'I-64-f4b-pbt4-gate-contract-package-v50.md')


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


print('lector', hashlib.sha256(open(READER, 'rb').read()).hexdigest())
print('paquete', hashlib.sha256(open(PKG, 'rb').read()).hexdigest())
rd = load('pbt4c_rr3_reading', READER)
ct = load('pbt4c_ct_table_v50', os.path.join(PB4, 'pbt4c_ct_table_v50.py'))
pkg = open(PKG, encoding='utf-8').read().split('\n')
i0 = pkg.index(next(l for l in pkg if l.startswith('### 6.0 ')))
i1 = pkg.index(next(l for l in pkg[i0 + 1:] if l.startswith('### 6.1 ')), i0 + 1)
sec = pkg[i0:i1]
CLASSES = ['PushBackEditAllTests', 'PushBackEditAllGuardTests', 'PushBackOpenerClassicPathGuardTests', 'PushBackDraftLaunchGuardTests',
           'PushBackEditAllWindowTests', 'PushBackEditAllPortEquivalenceGuardTests', 'PushBackEditAllNameWritersGuardTests',
           'PushBackEditAllFullCopyGuardTests', 'PushBackEditAllApplyCloseTests']
errors, info = rd.rr3_table_check(sec, CLASSES, 'PB-T4c', pkg)
print('errores', len(errors))
for e in errors:
    print('  ERROR', e)
bad = 0
for cls in CLASSES:
    i = info[cls]
    expected = []
    for n, what, lines, c, proj, red in ct.CT:
        if c == cls:
            expected += lines
    expected = list(dict.fromkeys(expected))
    if cls == 'PushBackDraftLaunchGuardTests' or i['tipo'] == 'sin-fila':
        same = None  # la fila de CT-15 trae sus tres literales propios y las fichas y los terminos de CT-PBT4A-08 (la forma de CT-19 de PB-T4b)
    else:
        same = i['lineas'] == expected
        if i['tipo'] == 'ct' and not same:
            bad += 1
    print('%-44s tipo=%-8s filas=%d lineas=%d valores=%s pendiente=%d %s' % (
        cls, i['tipo'], i['filas'], len(i['lineas']), i['valores'], len(i['pendiente']),
        '' if same is None else ('= las de sus CT' if same else '!= las de sus CT (%d)' % len(expected))))
    for p in i['pendiente']:
        print('    PENDIENTE', p)
ok = not errors and bad == 0 and all(not info[c]['pendiente'] for c in CLASSES[:5]) and all(info[c]['tipo'] == 'sin-fila' for c in CLASSES[5:])
print('RESULTADO', 'PASA' if ok else 'FALLA')
