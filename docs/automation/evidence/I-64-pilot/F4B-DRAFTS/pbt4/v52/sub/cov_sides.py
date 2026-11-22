"""PB-T4d (v5.2), relevo: para cada linea con ramas en la que WPUR (r6j-cv) cubre mas ramas que el resto de Core, que pruebas de WPUR (r6k-ct,
una cobertura por prueba) la ejecutan y cuantas ramas cubre cada una, marcando las 21 que se sustituyen. Solo lee.
Uso: python -I cov_sides.py <dir-cv> <dir-ct> <cov-tests-r6k.txt> <salida.txt>
"""
import os
import re
import sys
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
CV, CT, LIST, OUT = sys.argv[1:5]
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
            got, total = (int(m.group(1)), int(m.group(2))) if m else (0, 0)
            h0, g0, t0 = out.get((name, n), (0, 0, 0))
            out[(name, n)] = (max(h0, int(line.get('hits'))), max(g0, got), max(t0, total))
    return out


chosen = set(re.findall(r'^(\d\d) \w+: \d+ \(', open(LIST, encoding='utf-8').read(), re.M))
wpur, rest = cov(os.path.join(CV, 'wpur.cobertura.xml')), cov(os.path.join(CV, 'resto.cobertura.xml'))
tests = {}
for f in sorted(os.listdir(CT)):
    m = re.match(r'^(\d\d)\.cobertura\.xml$', f)
    if m:
        tests[m.group(1)] = cov(os.path.join(CT, f))
out = ['# las lineas con ramas en las que WPUR cubre mas que el resto: por prueba de WPUR, ramas cubiertas (* = una de las 21)']
for key, (h, g, t) in sorted(wpur.items()):
    r = rest.get(key, (0, 0, 0))
    if t == 0 or g <= r[1]:
        continue
    who = []
    for num, c in sorted(tests.items()):
        hh, gg, tt = c.get(key, (0, 0, 0))
        if hh > 0:
            who.append('%s%s:%d' % (num, '*' if num in chosen else '', gg))
    max21 = max([tests[n].get(key, (0, 0, 0))[1] for n in chosen if n in tests] or [0])
    out.append('%s:%d wpur %d/%d resto %d; max de una de las 21 = %d; %s' % (key[0], key[1], g, t, r[1], max21, ' '.join(who)))
with open(OUT, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('\n'.join(out) + '\n')
print(len(out) - 1)
