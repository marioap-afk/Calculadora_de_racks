"""Cuenta las filas de un TRX por resultado y lista las que no son Passed (clase.metodo y el motivo, recortado).
Uso: python -I trx_outcomes.py <trx> <salida.txt>   (la salida en UTF-8 con LF, sin depender de la consola)"""
import sys, collections, builtins
_out = open(sys.argv[2], 'w', encoding='utf-8', newline='\n')
print = lambda *a: builtins.print(*a, file=_out)
import xml.etree.ElementTree as ET
NS = '{http://microsoft.com/schemas/VisualStudio/TeamTest/2010}'
t = ET.parse(sys.argv[1]).getroot()
oc = collections.Counter()
other = []
for r in t.iter(f'{NS}UnitTestResult'):
    o = r.get('outcome')
    oc[o] += 1
    if o != 'Passed':
        msg = r.find(f'{NS}Output/{NS}ErrorInfo/{NS}Message')
        reason = ''
        if msg is None:
            so = r.find(f'{NS}Output/{NS}StdOut')
            reason = (so.text or '') if so is not None else ''
        else:
            reason = msg.text or ''
        other.append((o, r.get('testName'), ' '.join(reason.split())[:200]))
print('filas', sum(oc.values()), dict(oc))
for o, n, why in sorted(other):
    print(' ', o, n)
    if why:
        print('     ', why)
