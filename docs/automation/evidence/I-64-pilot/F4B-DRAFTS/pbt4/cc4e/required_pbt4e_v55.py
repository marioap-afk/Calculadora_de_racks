"""PB-T4e (paquete v5.5), precondicion 6: los RequiredTests de PB-T4e y los totales exactos de cada rama (F4B-E46).

- Los heredados: los 88 del contrato emitido de PB-T4d (required-pbt4d-emitted.tsv, de req_pbt4d_v55.py), en su orden, todos con ExpectRed false
  (CT-PBT4D-01 pasa desde el cierre de PB-T4d). Su MinSelected es un suelo, como en PB-T4c y PB-T4d: baja en (cuenta en la base - cuenta en el
  GREEN), la cuenta de resultados no omitidos (Passed o Failed); en PB-T4e no baja ninguno, porque nada se quita (la lista vacia). Un heredado que
  cae en el RED, o que selecciona distinto en el RED y en el GREEN, PARA (assert).
- Los nuevos, por clase exacta (D-S8): las dos clases de las CT, con su cuenta del GREEN y ExpectRed true. Comprueba que ningun filtro heredado
  selecciona una clase nueva y que cada filtro nuevo solo selecciona la suya, en el RED y en el GREEN; y que en el RED cada filtro nuevo da
  Failed == Selected (todas sus CT caen).
- Los totales: Core, de la suite completa de cada rama (la TRX es la de la CI: la misma suite); UI, la linea base de la CI del GREEN de PB-T4d
  (F4B-E59, run 45057868915: 2464 = 2416 + 0 + 48) mas la diferencia, por desenlace, entre la union de los 19 filtros de la rama y la de la base
  (cada filtro en su proceso), porque en local nunca corre la suite completa de UI.
Solo lee las TRX; escribe UTF-8 con LF.

Uso: python -I required_pbt4e_v55.py <required-pbt4d-emitted.tsv> <salida.tsv> <salida.txt> BASE_CORE=<trx> BASE_UI=<trx>... RED_CORE=<trx>
     RED_UI=<trx>... GREEN_CORE=<trx> GREEN_UI=<trx>...   (las de UI, <...>-uiNN.trx o base-uifNN.trx, NN la linea de ui_filters_pbt4e_v55.txt)
"""
import collections
import os
import re
import sys
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
REQ, OUT_TSV, OUT_TXT = sys.argv[1:4]
trx = collections.defaultdict(list)
for arg in sys.argv[4:]:
    kind, path = arg.split('=', 1)
    trx[kind].append(path)
CORE = 'tests/RackCad.Tests/RackCad.Tests.csproj'
UIP = 'tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj'
NEW = [
    (CORE, 'RackCad.Tests.PushBackEditAllReloadTests', 'nuevo: CT-PBT4E-01, 02 y 04'),
    (UIP, 'RackCad.UI.Tests.PushBackEditAllReloadFieldTests', 'nuevo: CT-PBT4E-03'),
]
BASE_CI_UI = collections.Counter({'Passed': 2416, 'Failed': 0, 'NotExecuted': 48})
UIF = [l for l in open(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'ui_filters_pbt4e_v55.txt'), encoding='utf-8').read().split('\n') if l]
assert len(UIF) == 19, len(UIF)


def rows_of(path):
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
    m = re.search(r'-(?:ui|uif)(\d\d)\.trx$', path) or re.search(r'uif(\d\d)\.trx$', path)
    return UIF[int(m.group(1)) - 1]


def dedup(rows_lists):
    best = {}
    for rows in rows_lists:
        here = collections.defaultdict(list)
        for cls, full, name, outcome in rows:
            here[(cls, full, name)].append(outcome)
        for k, outs in here.items():
            if len(outs) > len(best.get(k, [])):
                best[k] = outs
    return [(k[0], k[1], k[2], o) for k, outs in best.items() for o in outs]


def count(rows, needle):
    return sum(1 for cls, full, name, outcome in rows if outcome in ('Passed', 'Failed') and (needle in full or needle in name))


def failed(rows, needle):
    return sum(1 for cls, full, name, outcome in rows if outcome == 'Failed' and (needle in full or needle in name))


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
    # en la base el filtro de la clase nueva no selecciona nada (puede no dejar TRX)
    assert len(ui) == (18 if b == 'BASE' and UIF[-1] not in ui else 19), (b, len(ui))
    for f in UIF:
        ui.setdefault(f, [])
    pools[b] = {'core': rows_of(trx[b + '_CORE'][0]), 'ui': ui, 'ui_all': dedup(ui.values())}

rows = [l.split('\t') for l in open(REQ, encoding='utf-8').read().rstrip('\n').split('\n')]
head = rows[0]
assert len(rows) - 1 == 88, len(rows)
out = ['n\tProject\tFilter\tMinSelected\tExpectRed\torigen\tMinSelected-PBT4d\tbase\tmedido-RED\tmedido-GREEN']
log = ['# los RequiredTests de PB-T4e: los 88 heredados de PB-T4d con su MinSelected comprobado en la base, el RED y el GREEN, y los 2 nuevos por clase exacta']
overlap, n = [], 0
new_classes = {cls for _, cls, _ in NEW}
for i, r in enumerate(rows[1:], 1):
    d = dict(zip(head, r))
    needle = d['Filter'].split('~', 1)[1]
    ui_side = 'UI.Tests' in d['Project']
    pick = (lambda b: pools[b]['ui'][d['Filter']]) if ui_side else (lambda b: pools[b]['core'])
    base, red, green = count(pick('BASE'), needle), count(pick('RED'), needle), count(pick('GREEN'), needle)
    assert red == green, ('el RED y el GREEN seleccionan distinto', needle, red, green)
    assert failed(pick('RED'), needle) == 0 and failed(pick('GREEN'), needle) == 0, ('un heredado cae', needle)
    assert base >= int(d['MinSelected']), ('la base no llega al suelo de PB-T4d', needle, base, d['MinSelected'])
    for b in ('RED', 'GREEN'):
        hit = [c for c in classes(pick(b), needle) if c in new_classes]
        if hit:
            overlap.append('heredado %d %s selecciona en el %s %s' % (i, needle, b, hit))
    floor = int(d['MinSelected']) - max(0, base - green)
    assert floor > 0 and green > 0, ('un heredado se queda sin pruebas', needle)
    n += 1
    out.append('\t'.join([str(n), d['Project'], d['Filter'], str(floor), 'false', 'PB-T4d n.o %d' % i, d['MinSelected'], str(base), str(red), str(green)]))
    if floor != int(d['MinSelected']):
        log.append('  baja el heredado n.o %d: %s de %s a %d (base %d, GREEN %d)' % (i, needle, d['MinSelected'], floor, base, green))
    if d['ExpectRed'] != 'false':
        log.append('  el heredado n.o %d (%s) pasa a ExpectRed false (era %s en PB-T4d)' % (i, needle, d['ExpectRed']))
inherited = n
for project, cls, what in NEW:
    side = 'ui_all' if project == UIP else 'core'
    red, green = count(pools['RED'][side], cls), count(pools['GREEN'][side], cls)
    red_failed = failed(pools['RED'][side], cls)
    assert red_failed == red and red > 0, ('en el RED no caen todas las CT de la clase', cls, red_failed, red)
    assert failed(pools['GREEN'][side], cls) == 0, ('una CT cae en el GREEN', cls)
    assert count(pools['BASE'][side], cls) == 0, cls
    for b in ('RED', 'GREEN'):
        sel = classes(pools[b][side], cls)
        if sel != [cls]:
            overlap.append('nuevo %s selecciona en el %s %s' % (cls, b, sel))
    n += 1
    out.append('\t'.join([str(n), project, 'FullyQualifiedName~' + cls, str(green), 'true', what, '-', '0', '%d (Failed %d)' % (red, red_failed), str(green)]))
log.append('heredados %d + nuevos %d = %d; solapamientos: %d %s' % (inherited, len(NEW), n, len(overlap), overlap))
assert not overlap, overlap


def fmt(c):
    return '%d = %d + %d + %d' % (sum(c.values()), c.get('Passed', 0), c.get('Failed', 0), c.get('NotExecuted', 0))


core = {b: collections.Counter(o for _, _, _, o in pools[b]['core']) for b in ('BASE', 'RED', 'GREEN')}
ui_runs = {b: collections.Counter(o for _, _, _, o in pools[b]['ui_all']) for b in ('BASE', 'RED', 'GREEN')}
for b in ('BASE', 'RED', 'GREEN'):
    log.append('core %s: %s (Passed + Failed + Skipped)' % (b.lower(), fmt(core[b])))
for b in ('BASE', 'RED', 'GREEN'):
    log.append('ui-filtros %s (la union de los 19, cada uno en su proceso): %s' % (b.lower(), fmt(ui_runs[b])))
for b in ('RED', 'GREEN'):
    total = collections.Counter(BASE_CI_UI)
    for k in ('Passed', 'Failed', 'NotExecuted'):
        total[k] += ui_runs[b].get(k, 0) - ui_runs['BASE'].get(k, 0)
    log.append('ui-ci %s: %s (la linea base de la CI 2464 = 2416 + 0 + 48 mas la diferencia de los filtros)' % (b.lower(), fmt(total)))
with open(OUT_TSV, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
with open(OUT_TXT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(log) + '\n')
print('\n'.join(log))
