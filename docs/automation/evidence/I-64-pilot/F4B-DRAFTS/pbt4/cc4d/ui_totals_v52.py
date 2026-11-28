"""PB-T4d (paquete v5.2), precondicion 6: el total exacto de UI de cada rama para la CI (F4B-E46), sin correr la suite completa de UI en local.
Las bajas y las ediciones de UI de PB-T4d caen todas en clases que seleccionan los 18 filtros heredados (ui_filters_pbt4c_v50b.txt), asi que lo que
PB-T4d quita o anade en UI es la diferencia entre la union de las TRX de esos filtros en la base (6c2820ba sin cambios, /d/r6u base-ui*; o /d/r5j
g-ui-*, el GREEN literal de PB-T4c, cuyas pruebas de UI en esas clases son las de 6c2820ba) y en la rama medida. El total de la CI es la linea base (la del cierre de PB-T4c, F4B-E55, GREEN
44959653142: UI 2 486 = 2 437 + 0 + 49) con esa diferencia, por resultado. Solo lee las TRX; escribe UTF-8 con LF.

Uso: python -I ui_totals_v52.py <salida.txt> <etiqueta> BASE=<trx>... RAMA=<trx>...
"""
import collections
import sys
import xml.etree.ElementTree as ET

NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
OUT, LABEL = sys.argv[1:3]
BASELINE = {'Passed': 2437, 'Failed': 0, 'NotExecuted': 49}
trx = collections.defaultdict(list)
for arg in sys.argv[3:]:
    kind, path = arg.split('=', 1)
    trx[kind].append(path)


def union(paths):
    """Cada resultado, una vez (por su clave y su numero de orden entre los de la misma clave: una Theory con nombres repetidos cuenta cada fila),
    aunque lo seleccionen dos filtros (se toma la TRX que mas resultados tiene con esa clave)."""
    best = {}
    for p in paths:
        root = ET.parse(p).getroot()
        defs = {ut.get('id'): ut.find('t:TestMethod', NS).get('className') + '.' + ut.find('t:TestMethod', NS).get('name')
                for ut in root.iterfind('.//t:TestDefinitions/t:UnitTest', NS)}
        here = collections.defaultdict(list)
        for r in root.iterfind('.//t:Results/t:UnitTestResult', NS):
            here[(defs[r.get('testId')], r.get('testName'))].append(r.get('outcome'))
        for k, outs in here.items():
            if len(outs) > len(best.get(k, [])):
                best[k] = sorted(outs)
    return {(k, i): o for k, outs in best.items() for i, o in enumerate(outs)}


base, branch = union(trx['BASE']), union(trx['RAMA'])
removed = collections.Counter(base[k] for k in base if k not in branch)
added = collections.Counter(branch[k] for k in branch if k not in base)
changed = collections.Counter((base[k], branch[k]) for k in base if k in branch and base[k] != branch[k])
total = collections.Counter(BASELINE)
for o, n in removed.items():
    total[o] -= n
for o, n in added.items():
    total[o] += n
for (a, b), n in changed.items():
    total[a] -= n
    total[b] += n
lines = ['# el total de UI de la CI en la rama %s: la linea base del cierre de PB-T4c con lo que cambia en los 18 filtros' % LABEL,
         'filtros: base %d resultados en %d TRX; rama %d en %d TRX' % (len(base), len(trx['BASE']), len(branch), len(trx['RAMA'])),
         'quitados %d %s; anadidos %d %s; cambian de resultado %d %s' % (sum(removed.values()), dict(removed), sum(added.values()), dict(added),
                                                                         sum(changed.values()), {'%s->%s' % k: v for k, v in changed.items()}),
         'UI %s: %d = %d + %d + %d (Passed + Failed + Skipped)' % (LABEL, sum(total.values()), total['Passed'], total['Failed'], total['NotExecuted'])]
for k in sorted(k for k in base if k not in branch):
    lines.append('  quitado %s %s' % (base[k], k[0][1] or k[0][0]))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('\n'.join(lines[1:4]))
