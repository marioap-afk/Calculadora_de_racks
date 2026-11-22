"""PB-T4d (paquete v5.2), precondicion 6: los RequiredTests de PB-T4d. Los heredados son los del contrato de PB-T4c (v52/pbt4c-required-6c2820ba.tsv,
en su orden; su numero en ese contrato salta el 43, que salio en PB-T4c), con su ExpectRed (todos false). Su MinSelected es un suelo, no una cuenta
exacta: como en PB-T4c, baja en lo que la lista quita de lo que el filtro selecciona, la cuenta de resultados no omitidos (Passed o Failed) en la
base menos la del GREEN literal; un filtro cuyo suelo llega a 0, o que no selecciona nada en el GREEN, sale. La base: la suite completa de Core en
6c2820ba sin cambios (BASE_CORE) y, en UI, las TRX de sus filtros en la base (BASE_UI). Los nuevos, por clase exacta (D-S12): las dos CT y las tres
clases nuevas que no son CT, con su cuenta del GREEN. Comprueba que ningun filtro heredado selecciona una clase nueva y que cada filtro nuevo solo
selecciona la suya, en el RED y en el GREEN. Solo lee las TRX; escribe UTF-8 con LF.

Uso: python -I required_pbt4d_v52.py <required-pbt4c.tsv> <salida.tsv> <salida.txt> BASE_CORE=<trx> BASE_UI=<trx>... RED_CORE=<trx> RED_UI=<trx>...
     GREEN_CORE=<trx> GREEN_UI=<trx>...
"""
import collections
import os
import re
import sys
import xml.etree.ElementTree as ET

NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
REQ, OUT_TSV, OUT_TXT = sys.argv[1:4]
trx = collections.defaultdict(list)
for arg in sys.argv[4:]:
    kind, path = arg.split('=', 1)
    trx[kind].append(path)
CORE = 'tests/RackCad.Tests/RackCad.Tests.csproj'
NEW = [
    ('RackCad.Tests.PushBackRetirePilotGuardTests', True, 'CT-PBT4D-01'),
    ('RackCad.Tests.PushBackRetirePilotCensusGuardTests', False, 'CT-PBT4D-02'),
    ('RackCad.Tests.PushBackRetirePilotFullCopyGuardTests', False, 'equivalente de CT-PBT4B-20 (D-S12)'),
    ('RackCad.Tests.PushBackRetirePilotPlanGuardTests', False, 'equivalente de las dos guardas de WPPG (D-S12)'),
    ('RackCad.Tests.PushBackRetirePilotRunTests', False, 'las 27 sustitutas de WPUR'),
]


UIF = [l for l in open(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', 'pbt4ccc', 'compile', 'ui_filters_pbt4c_v50b.txt'),
                       encoding='utf-8').read().split('\n') if l] + ['FullyQualifiedName~RackCad.UI.Tests.PushBackEditAllWindowTests']


def rows_of(path):
    """Cada resultado de una TRX, uno por executionId (MinSelected cuenta resultados: una Theory con nombres repetidos cuenta cada fila)."""
    root = ET.parse(path).getroot()
    defs = {}
    for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS):
        tm = ut.find('t:TestMethod', NS)
        defs[ut.get('id')] = (tm.get('className'), tm.get('className') + '.' + tm.get('name'))
    seen = {}
    for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
        cls, full = defs[r.get('testId')]
        seen[r.get('executionId')] = (cls, full, r.get('testName') or '', r.get('outcome'))
    return list(seen.values())


def ui_filter_of(path):
    """El filtro de una TRX de UI, por su nombre: <rama>-uiNN, NN la linea de ui_filters_pbt4c_v50b.txt, y 19 el de PushBackEditAllWindowTests."""
    m = re.search(r'-ui(\d\d)\.trx$', path)
    return UIF[int(m.group(1)) - 1]


def count(rows, needle):
    return sum(1 for cls, full, name, outcome in rows if outcome in ('Passed', 'Failed') and (needle in full or needle in name))


def classes(rows, needle):
    return sorted({cls for cls, full, name, outcome in rows if needle in full or needle in name})


pools = {}
for b in ('BASE', 'RED', 'GREEN'):
    assert len(trx[b + '_CORE']) == 1, b
    ui = {}
    for p in trx[b + '_UI']:
        f = ui_filter_of(p)
        assert f not in ui, (b, f)
        ui[f] = rows_of(p)
    pools[b] = {'core': rows_of(trx[b + '_CORE'][0]), 'ui': ui, 'ui_all': [x for v in ui.values() for x in v]}
rows = [l.split('\t') for l in open(REQ, encoding='utf-8').read().rstrip('\n').split('\n')]
head = rows[0]
assert len(rows) - 1 == 85, len(rows)
out = ['n\tProject\tFilter\tMinSelected\tExpectRed\torigen\tMinSelected-PBT4c\tbase\tmedido-RED\tmedido-GREEN']
log = ['# los RequiredTests de PB-T4d: los heredados de PB-T4c con su MinSelected medido en el GREEN, y los nuevos por clase exacta']
overlap = []
n = 0
gone = []
for i, r in enumerate(rows[1:], 1):
    k = i if i < 43 else i + 1  # el numero en el contrato de PB-T4c
    d = dict(zip(head, r))
    needle = d['Filter'].split('~', 1)[1]
    ui_side = 'UI.Tests' in d['Project']
    pick = (lambda b: pools[b]['ui'][d['Filter']]) if ui_side else (lambda b: pools[b]['core'])
    base, red, green = count(pick('BASE'), needle), count(pick('RED'), needle), count(pick('GREEN'), needle)
    assert red == green, ('el RED y el GREEN seleccionan distinto', needle, red, green)
    # todos los heredados pasan en el RED de PB-T4d (las CT de PB-T4c pasan desde su cierre): su ExpectRed es false
    assert not [x for x in pick('RED') if x[3] == 'Failed' and (needle in x[1] or needle in x[2])], ('un heredado cae en el RED', needle)
    assert base >= int(d['MinSelected']), ('la base no llega al suelo de PB-T4c', needle, base, d['MinSelected'])
    for b in ('RED', 'GREEN'):
        hit = [c for c in classes(pools[b]['ui_all' if ui_side else 'core'], needle) if any(c == cls for cls, _, _ in NEW)]
        if hit:
            overlap.append('heredado %d %s selecciona en el %s %s' % (k, needle, b, hit))
    floor = int(d['MinSelected']) - (base - green)
    if green == 0 or floor <= 0:
        gone.append('  sale el heredado n.o %d de PB-T4c: %s (%s; base %d, GREEN %d)' % (k, needle, d['MinSelected'], base, green))
        continue
    n += 1
    out.append('\t'.join([str(n), d['Project'], d['Filter'], str(floor), 'false', 'PB-T4c n.o %d' % k, d['MinSelected'], str(base), str(red), str(green)]))
    if floor != int(d['MinSelected']):
        log.append('  baja el heredado n.o %d de PB-T4c (PB-T4d n.o %d): %s de %s a %d (base %d, GREEN %d)' % (k, n, needle, d['MinSelected'], floor, base, green))
inherited = n
for cls, expect_red, what in NEW:
    needle = cls
    red, green = count(pools['RED']['core'], needle), count(pools['GREEN']['core'], needle)
    for b in ('RED', 'GREEN'):
        sel = classes(pools[b]['core'], needle)
        if sel != [cls]:
            overlap.append('nuevo %s selecciona en el %s %s' % (cls, b, sel))
    n += 1
    out.append('\t'.join([str(n), CORE, 'FullyQualifiedName~' + needle, str(green), 'true' if expect_red else 'false', 'nuevo: ' + what, '-', '-', str(red), str(green)]))
log += gone
log.append('heredados %d de %d (salen %d) + nuevos %d = %d' % (inherited, len(rows) - 1, len(gone), len(NEW), n))
log.append('solapamientos: %d %s' % (len(overlap), overlap))
for b in ('RED', 'GREEN'):
    for side in ('core', 'ui_all'):
        c = collections.Counter(o for _, _, _, o in pools[b][side])
        log.append('totales %s %s: %d %s' % (b, side, sum(c.values()), dict(sorted(c.items()))))
with open(OUT_TSV, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
with open(OUT_TXT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(log) + '\n')
print('\n'.join(log))
