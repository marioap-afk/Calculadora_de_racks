"""PB-T4b, «Saltar y duplicar»: el mapa cerrado de la copia mecánica, medido por texto en una revisión.

Uso: python -I copy_map_pbt4.py <checkout> <rev> <salida.tsv>
Solo objetos git (lists_core.py); el mapa y los residuos, en copy_map_defs.py. Escribe solo <salida.tsv>.

Para cada una de las 428 pruebas de las 9 clases: aplica el mapa M a su texto (atributos, cuerpo y ayudantes de un nivel,
sin los tipos del kit, que no se copian) y mira si queda algo que el mapa no cubre. Si no queda nada, la copia es mecánica
(la comprueba CT-PBT4B-20). Si queda algo, va a la sublista «a mano», con su motivo. Mira también los residuos fuera de
las pruebas (ayudantes, campos y tipos de primer nivel), que impedirían compilar la copia del archivo.
"""
import re
import sys
import importlib.util
from collections import Counter

base = __file__.replace('copy_map_pbt4.py', '')
spec = importlib.util.spec_from_file_location('lc', base + 'lists_core.py')
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
spec2 = importlib.util.spec_from_file_location('cm', base + 'copy_map_defs.py')
cm = importlib.util.module_from_spec(spec2)
spec2.loader.exec_module(cm)
lc.REPO, lc.REV = sys.argv[1:3]
out_tsv = sys.argv[3]
TW = 'tests/RackCad.Tests/Workspace/'
SKIPPED = re.compile(r'\bSkip\s*=')
COMPILE = ('R-retirado', 'R-estado')

rows, uses, tops, outside = [], Counter(), {}, {}
for cls in cm.NINE:
    path = TW + cls + '.cs'
    code = lc.blank_comments(lc.show(path))
    masked = lc.mask(code)
    tops[cls] = [t for t in cm.top_types(masked) if t not in cm.KIT_TYPES]
    text = cm.without_kit(code, masked)
    ms = lc.members(text)
    helpers = {}
    for m in ms:
        if not m['test']:
            helpers.setdefault(m['name'], []).append(m['body'])
    in_test = set()
    for m in ms:
        if not m['test'] or SKIPPED.search(m['attrs']):
            continue
        in_test.update(range(m['first'], m['last'] + 1))
        called = [h for h in helpers if h != m['name'] and re.search(r'\b' + re.escape(h) + r'\s*[(<]', m['body'])]
        full = m['attrs'] + m['body'] + '\n' + '\n'.join(b for h in called for b in helpers[h])
        mapped, used = cm.apply_map(full)
        for k in used:
            uses[k] += 1
        why = [k for k, rx in cm.RESIDUAL if rx.search(mapped)]
        rows.append((cls, m['name'], '%d-%d' % (m['first'], m['last']), ','.join(used) or '-', ','.join(why) or 'mecanica'))
    res = []
    for i, line in enumerate(text.split('\n'), 1):
        if i in in_test:
            continue
        mapped, _ = cm.apply_map(line)
        why = [k for k, rx in cm.RESIDUAL if rx.search(mapped)]
        if why:
            res.append((i, ','.join(why), line.strip()))
    outside[cls] = res

hand = [r for r in rows if r[4] != 'mecanica']
mix = Counter(r[4] for r in hand)
per_class = Counter(r[0] for r in hand)
with open(out_tsv, 'w', encoding='utf-8', newline='\n') as f:
    f.write('clase\tmetodo\tlineas\tmapa\tdestino\n')
    for r in rows:
        f.write('\t'.join(r) + '\n')
print('pruebas', len(rows), 'mecanicas', len(rows) - len(hand), 'a-mano', len(hand))
print('a mano por motivo', sorted(mix.items(), key=lambda x: -x[1]))
print('a mano por clase', [(c, per_class[c]) for c in cm.NINE])
print('mecanicas por clase', [(c, sum(1 for r in rows if r[0] == c and r[4] == 'mecanica')) for c in cm.NINE])
print('usos del mapa (pruebas)', sorted(uses.items()))
for cls in cm.NINE:
    bad = [x for x in outside[cls] if any(k in x[1] for k in COMPILE)]
    print('fuera de las pruebas en %s: %d residuos, %d de compilacion; tipos de primer nivel que se copian: %s' % (cls, len(outside[cls]), len(bad), tops[cls]))
    for i, why, line in outside[cls]:
        print('    %d [%s] %s' % (i, why, line[:140]))
