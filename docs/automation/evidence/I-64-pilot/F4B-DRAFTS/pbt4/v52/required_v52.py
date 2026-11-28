"""PB-T4d (paquete v5.2): los MinSelected de los RequiredTests heredados de PB-T4c sobre unas TRX (la suite de Core y los filtros de UI de una
copia). MinSelected cuenta resultados no omitidos (como en los contratos de PB-T4b y PB-T4c): para cada filtro `FullyQualifiedName~X`, los
resultados Passed o Failed cuyo nombre completo (clase.metodo) contiene X, en la TRX de su proyecto. Imprime la tabla con el valor de PB-T4c
y el medido, y si el filtro se queda en 0 (su fila sale). Escribe UTF-8 con LF.

Uso: python -I required_v52.py <required.tsv> <salida.tsv> CORE=<trx> UI=<trx> [UI=<trx> ...]
"""
import collections
import sys
import xml.etree.ElementTree as ET

NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
REQ, OUT = sys.argv[1:3]
trx = collections.defaultdict(list)
for arg in sys.argv[3:]:
    kind, path = arg.split('=', 1)
    trx[kind].append(path)


def results(paths):
    seen = {}
    for p in paths:
        root = ET.parse(p).getroot()
        defs = {}
        for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS):
            tm = ut.find('t:TestMethod', NS)
            defs[ut.get('id')] = tm.get('className') + '.' + tm.get('name')
        for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
            seen[(defs[r.get('testId')], r.get('testName'))] = (defs[r.get('testId')], r.get('testName'), r.get('outcome'))
    return list(seen.values())


core = results(trx['CORE'])
ui = results(trx['UI'])
rows = [l.split('\t') for l in open(REQ, encoding='utf-8').read().rstrip('\n').split('\n')]
head = rows[0]
out = ['n\tProject\tFilter\tMinSelected-PBT4c\tmedido\tExpectRed-PBT4c\tsale']
for n, r in enumerate(rows[1:], 1):
    d = dict(zip(head, r))
    needle = d['Filter'].split('~', 1)[1]
    pool = ui if 'UI.Tests' in d['Project'] else core
    count = sum(1 for full, name, outcome in pool if outcome in ('Passed', 'Failed') and (needle in full or needle in (name or '')))
    out.append('\t'.join([str(n), d['Project'], d['Filter'], d['MinSelected'], str(count), d['ExpectRed'], 'si' if count == 0 else 'no']))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
changed = [l for l in out[1:] if l.split('\t')[3] != l.split('\t')[4]]
print('filtros %d, cambian %d, salen %d' % (len(out) - 1, len(changed), sum(1 for l in out[1:] if l.endswith('\tsi'))))
for l in changed:
    print('  ' + l)
