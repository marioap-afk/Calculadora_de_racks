"""PB-T4d (v5.2), relevo: de las 436 lineas de la corrida generica que ejercita WPUR (r6j-cv), las que el resto cubre SIN ninguna de las cinco
clases del modelo completo de las citas (el grupo 'otras'), y las ramas: por linea, que ramas cubre WPUR, el resto y las cinco clases. Solo lee.
Uso: python -I cov_otras.py <repo> <dir-cv> <pieza-green> <salida.txt>
"""
import collections
import importlib.util
import os
import re
import subprocess
import sys
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
REPO, CV, PIECE, OUT = sys.argv[1:5]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
P = 'src/RackCad.Application/Systems/PushBack/Pilot/'
FILES = ['PushBackPilotUpdateRun.cs', 'PushBackPilotUpdatePlan.cs']
FULL = ['fwr', 'wpuh', 'wpuq', 'fpt', 'fwp']


def show(path):
    return subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


def cov(label):
    out = {}
    for cls in ET.parse(os.path.join(CV, label + '.cobertura.xml')).getroot().iter('class'):
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


spec = importlib.util.spec_from_file_location('green_pbt4d_v52', os.path.join(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))), 'green_pbt4d_v52.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
retired = collections.defaultdict(set)
for item in gp.blocks(PIECE):
    if item[0] != 'EDIT' or os.path.basename(item[1]) not in FILES or item[3] != '':
        continue
    base = show(item[1])
    at = base.find(item[2])
    if at < 0:
        continue
    first = base.count('\n', 0, at) + 1
    for k in range(first, first + item[2].count('\n')):
        retired[os.path.basename(item[1])].add(k)
texts = {f: show(P + f).split('\n') for f in FILES}
wpur, rest = cov('wpur'), cov('resto')
full = {}
for label in FULL:
    for key, (h, g, t) in cov(label).items():
        h0, g0, t0 = full.get(key, (0, 0, 0))
        full[key] = (max(h0, h), max(g0, g), max(t0, t))
out = ['# de las lineas de la corrida generica que ejercita WPUR (sin las retiradas): las que el resto cubre sin las cinco clases del modelo completo',
       '# (fwr, wpuh, wpuq, fpt, fwp), y las ramas que WPUR cubre por encima de esas cinco clases']
otras, ramas = [], []
total = 0
for (name, n), (h, g, t) in sorted(wpur.items()):
    if h == 0 or n in retired[name]:
        continue
    total += 1
    r = rest.get((name, n), (0, 0, 0))
    f = full.get((name, n), (0, 0, 0))
    src = texts[name][n - 1].strip()[:140]
    if r[0] > 0 and f[0] == 0:
        otras.append('  %s:%d  resto %d/%d  %s' % (name, n, r[1], r[2], src))
    if t > 0 and g > f[1]:
        ramas.append('  %s:%d  wpur %d/%d  resto %d  completo %d  %s' % (name, n, g, t, r[1], f[1], src))
out.append('lineas %d; otras %d; ramas de WPUR por encima del modelo completo %d' % (total, len(otras), len(ramas)))
out.append('')
out.append('## las otras (las cubre el resto, ninguna de las cinco clases)')
out += otras
out.append('')
out.append('## las ramas (WPUR cubre mas que las cinco clases)')
out += ramas
with open(OUT, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('\n'.join(out) + '\n')
print(out[2])
