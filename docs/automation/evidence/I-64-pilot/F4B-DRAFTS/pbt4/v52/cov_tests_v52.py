"""PB-T4d (paquete v5.2): que pruebas de WPUR son las unicas que ejercitan cada camino de la corrida generica que el resto de la suite de Core no
ejercita. Lee la cobertura de la suite sin WPUR (`resto`, de p6d_cov_v52.sh) y la de cada prueba de WPUR sola (p6d_covtest_v52.sh), sobre las
lineas que no retira el GREEN (de cov_v52.py, que se importa). Una linea cuenta si WPUR la ejecuta y el resto no; una linea con ramas, si la
prueba cubre mas ramas que el resto. Da, por prueba, sus lineas y ramas propias, y el conjunto minimo (voraz) de pruebas que las cubre todas.
Solo lee; escribe UTF-8 con LF.

Uso: python -I cov_tests_v52.py <repo> <dir-cv> <dir-ct> <pieza-green> <salida.txt>
"""
import collections
import glob
import os
import re
import sys
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
REPO, CV, CT, PIECE, OUT = sys.argv[1:6]
FILES = ['PushBackPilotUpdateRun.cs', 'PushBackPilotUpdatePlan.cs']


def cov(path):
    out = {}
    for cls in ET.parse(path).getroot().iter('class'):
        name = os.path.basename(cls.get('filename').replace('\\', '/'))
        if name not in FILES:
            continue
        for line in cls.iter('line'):
            n = int(line.get('number'))
            m = re.search(r'\((\d+)/(\d+)\)', line.get('condition-coverage') or '')
            got = int(m.group(1)) if m else 0
            h0, g0 = out.get((name, n), (0, 0))
            out[(name, n)] = (max(h0, int(line.get('hits'))), max(g0, got))
    return out


# los tramos retirados, con la misma lectura que cov_v52.py
src = open(os.path.join(os.path.dirname(os.path.abspath(__file__)), 'cov_v52.py'), encoding='utf-8').read()
ns = {'__name__': 'cov_v52_part', '__file__': os.path.join(os.path.dirname(os.path.abspath(__file__)), 'cov_v52.py')}
saved = sys.argv
sys.argv = ['cov_v52.py', REPO, CV, PIECE, os.devnull]
exec(compile(src[:src.index('wpur, rest = ')], 'cov_v52.py', 'exec'), ns)
sys.argv = saved
retired, texts = ns['retired'], ns['texts']
rest = cov(os.path.join(CV, 'resto.cobertura.xml'))
names = {}
for line in open(os.path.join(CT, 'pruebas.txt'), encoding='utf-8'):
    parts = line.split()
    names[parts[0]] = parts[1]
own = {}
for path in sorted(glob.glob(os.path.join(CT, '*.cobertura.xml'))):
    nn = os.path.basename(path).split('.')[0]
    c = cov(path)
    keys = set()
    for (f, n), (h, g) in c.items():
        if h == 0 or n in retired[f]:
            continue
        rh, rg = rest.get((f, n), (0, 0))
        if rh == 0:
            keys.add((f, n, 'linea'))
        elif g > rg:
            keys.add((f, n, 'rama'))
    own[nn] = keys
universe = set().union(*own.values()) if own else set()
out = ['# las pruebas de WPUR que ejercitan lo que el resto de la suite de Core no ejercita (lineas y ramas de la corrida generica que se queda)',
       'lineas o ramas propias de WPUR: %d (lineas %d, ramas %d); pruebas que tienen alguna: %d de %d'
       % (len(universe), sum(1 for k in universe if k[2] == 'linea'), sum(1 for k in universe if k[2] == 'rama'),
          sum(1 for v in own.values() if v), len(own))]
out.append('')
out.append('## por prueba (las que tienen algo propio)')
for nn in sorted(own):
    if own[nn]:
        ks = sorted(own[nn])
        out.append('%s %s: %d (%s)' % (nn, names.get(nn, '?'), len(ks), ', '.join('%s:%d%s' % (f.replace('PushBackPilot', '')[:-3], n, '' if t == 'linea' else 'r') for f, n, t in ks)))
left, chosen = set(universe), []
while left:
    best = max(own, key=lambda k: (len(own[k] & left), k))
    if not own[best] & left:
        break
    chosen.append(best)
    left -= own[best]
out.append('')
out.append('## el conjunto minimo (voraz) que las cubre todas: %d pruebas' % len(chosen))
for nn in chosen:
    out.append('  %s %s' % (nn, names.get(nn, '?')))
out.append('')
out.append('## las lineas y ramas, con su texto')
for f, n, t in sorted(universe):
    who = [nn for nn in sorted(own) if (f, n, t) in own[nn]]
    out.append('  %s:%d %s [%s] %s' % (f, n, t, ','.join(who), texts[f][n - 1].strip()[:120]))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\n'.join(out[:2]))
