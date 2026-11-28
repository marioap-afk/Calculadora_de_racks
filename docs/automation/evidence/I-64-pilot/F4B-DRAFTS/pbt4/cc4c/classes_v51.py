"""PB-T4c (paquete v5.1), precondicion 6: el resultado de cada clase que corre (Passed, Failed y Skipped), de una o varias TRX, para
compile-check.md. Una fila por clase, en orden ordinal, con su cuenta de resultados; y, con --solo-no-verdes, solo las clases con algun Failed o
Skipped y la cuenta de las demas. Solo lee las TRX; escribe en UTF-8 con LF.

Uso: python -I classes_v51.py [--solo-no-verdes] <salida.txt> <trx> [<trx> ...]
"""
import collections
import os
import sys
import xml.etree.ElementTree as ET

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
args = sys.argv[1:]
only = args[0] == '--solo-no-verdes'
if only:
    args = args[1:]
out, trxs = args[0], args[1:]
per = collections.OrderedDict()
for trx in trxs:
    root = ET.parse(trx).getroot()
    defs = {ut.get('id'): ut.find('t:TestMethod', NS).get('className') for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS)}
    for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
        key = (os.path.basename(trx), defs[r.get('testId')])
        per.setdefault(key, collections.Counter())[r.get('outcome')] += 1
lines = ['| TRX | Clase | Passed | Failed | Skipped |', '|---|---|---|---|---|']
green = 0
for (trx, cls), c in sorted(per.items(), key=lambda kv: (kv[0][0], kv[0][1])):
    if only and not c.get('Failed') and not c.get('NotExecuted'):
        green += 1
        continue
    lines.append('| `%s` | `%s` | %d | %d | %d |' % (trx, cls, c.get('Passed', 0), c.get('Failed', 0), c.get('NotExecuted', 0)))
if only:
    lines.append('')
    lines.append('Las otras %d clases: todas Passed.' % green)
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('clases=%d filas=%d' % (len(per), len(lines) - 2))
