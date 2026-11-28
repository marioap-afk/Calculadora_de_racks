"""PB-T4c (v5.0): la lectura del paseo de guardas (pbt4c-guard-walk-v50.tsv). Agrupa las pruebas marcadas (A) por el tipo de diferencia:
- 'existente': un literal cambia de cuenta en un archivo existente que la prueba lee (WHC, PORT, SUP, WEI, WCo, FVM, PV, PVC o W);
- 'solo-nuevo': solo aparece en un archivo nuevo (HOSTI, LAUNCH, SPORT u OPENER).
Para las de 'existente', da el literal y el cambio; para las de 'solo-nuevo', los literales distintos y en que archivo nuevo. Solo lee el TSV;
escribe la salida en UTF-8 con LF. Uso: python -I walk_analysis_pbt4c_v50.py <walk.tsv> <salida.txt>
"""
import re
import sys
from collections import defaultdict, Counter

sys.stdout.reconfigure(encoding='utf-8', newline='\n')
src, out = sys.argv[1], sys.argv[2]
rows = [l.split('\t') for l in open(src, encoding='utf-8').read().split('\n')[1:] if l]
EXISTING = ('WHC', 'PORT', 'SUP', 'WEI', 'WCo', 'FVM', 'PV', 'PVC', 'W')
groups = defaultdict(list)
lits_new = Counter()
lits_existing = defaultdict(set)
for r in rows:
    if r[3] not in ('A', 'B'):
        continue
    diffs = r[7]
    parts = [p for p in diffs.split(' || ') if p != '-']
    kind = 'solo-nuevo'
    for p in parts:
        lit, _, d = p.partition(': ')
        for item in d.split('; '):
            short = item.split(' ', 1)[0]
            if short in EXISTING:
                kind = 'existente'
                lits_existing[(lit, item)].add(r[0].rsplit('/', 1)[-1] + '.' + r[1])
            else:
                lits_new[(lit, short)] += 1
    groups[kind].append(r)
lines = ['marcadas A o B: %d; con cambio en un archivo existente: %d; solo por los archivos nuevos: %d; en la lista: %d'
         % (sum(len(v) for v in groups.values()), len(groups['existente']), len(groups['solo-nuevo']),
            sum(1 for v in groups.values() for r in v if r[4] == 'lista'))]
lines.append('')
lines.append('## Los literales que cambian de cuenta en un archivo existente (literal -> cambio: pruebas)')
for (lit, item), tests in sorted(lits_existing.items(), key=lambda kv: (-len(kv[1]), kv[0])):
    lines.append('%s -> %s: %d pruebas: %s' % (lit, item, len(tests), ', '.join(sorted(tests)[:6]) + (' ...' if len(tests) > 6 else '')))
lines.append('')
lines.append('## Las pruebas con cambio en un archivo existente')
for r in groups['existente']:
    lines.append('%s.%s (%s) %s | %s' % (r[0].rsplit('/', 1)[-1], r[1], r[2], r[4], r[7][:300]))
lines.append('')
lines.append('## Los literales que solo aparecen en los archivos nuevos (literal, archivo: pruebas)')
for (lit, short), n in sorted(lits_new.items(), key=lambda kv: -kv[1])[:80]:
    lines.append('%s, %s: %d' % (lit, short, n))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print(lines[0])
