"""PB-T4c, precondicion 6 (paquete v5.1): la tabla de resultados por clase de una corrida entera (Core y UI), para compile-check.md: una fila
por clase, con Passed, Failed y Skipped en cada rama, sumados sobre las TRX de esa rama (la suite de Core y cada filtro de UI), en orden ordinal.
La custodia lleva solo por sha256 las salidas de ruta larga; esta tabla deja su resultado en el registro. Lee solo las TRX (en la copia
exportada de la corrida); escribe en UTF-8 con LF, con el sha256 de cada TRX leida.

Uso: python -I run_classes_v51.py <salida.txt> <rama>=<trx> [<rama>=<trx> ...]
"""
import collections
import hashlib
import os
import sys
import xml.etree.ElementTree as ET

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
out, pairs = sys.argv[1], [a.split('=', 1) for a in sys.argv[2:]]
branches = []
per = collections.defaultdict(lambda: collections.defaultdict(collections.Counter))
read = []
for branch, trx in pairs:
    if branch not in branches:
        branches.append(branch)
    data = open(trx, 'rb').read()
    read.append((branch, os.path.basename(trx), hashlib.sha256(data).hexdigest()))
    root = ET.fromstring(data)
    defs = {ut.get('id'): ut.find('t:TestMethod', NS).get('className') for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS)}
    for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
        per[defs[r.get('testId')]][branch][r.get('outcome')] += 1
lines = ['| Clase | ' + ' | '.join('%s (P / F / S)' % b for b in branches) + ' |', '|---|' + '---|' * len(branches)]
tot = {b: collections.Counter() for b in branches}
for cls in sorted(per):
    cells = []
    for b in branches:
        c = per[cls].get(b)
        if c is None:
            cells.append('—')
        else:
            cells.append('%d / %d / %d' % (c.get('Passed', 0), c.get('Failed', 0), c.get('NotExecuted', 0)))
            tot[b].update(c)
    lines.append('| `%s` | %s |' % (cls, ' | '.join(cells)))
lines.append('| **Total (%d clases)** | %s |' % (len(per), ' | '.join('**%d / %d / %d**' % (tot[b].get('Passed', 0), tot[b].get('Failed', 0),
                                                                                        tot[b].get('NotExecuted', 0)) for b in branches)))
lines.append('')
lines.append('TRX leidas (%d): %s' % (len(read), '; '.join('%s %s %s' % (b, n, s[:12]) for b, n, s in read)))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('clases=%d trx=%d %s' % (len(per), len(read), {b: dict(tot[b]) for b in branches}))
