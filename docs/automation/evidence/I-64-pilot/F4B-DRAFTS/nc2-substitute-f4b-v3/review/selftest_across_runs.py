"""Revision v3 (solo lectura): la autoprueba registrada en cada salida (v2 y v3, todos los casos) es la misma lista.
Uso: python -I -B selftest_across_runs.py <salida.json> [...]"""
import json, sys

ref = None
for p in sys.argv[1:]:
    r = json.load(open(p, encoding='utf-8'))
    st = r.get('SelfTest')
    ref = st if ref is None else ref
    print('%-14s casos=%d ok=%d pass=%s det=%s igual_a_la_primera=%s' % (p.replace('\\', '/').split('/')[-1], len(st), sum(1 for x in st if x['Ok']),
          r.get('SelfTestPass'), r.get('SelfTestDeterministic'), st == ref))
