"""PB-T4d (paquete v5.2): la lectura de la cobertura de p6d_cov_v52.sh. Para la corrida generica (PushBackPilotUpdateRun.cs) y el plan
(PushBackPilotUpdatePlan.cs) de 6c2820ba: cada linea que ejercita WPUR, fuera de los miembros que retira el GREEN (los tramos OLD de la pieza),
si la ejercita tambien el resto de la suite de Core (sin WPUR), con sus ramas, y que clase de las de las citas la ejercita. Lo que WPUR cubre y
el resto no es un camino que solo WPUR prueba. Solo lee; escribe UTF-8 con LF.

Uso: python -I cov_v52.py <repo> <dir-cv> <pieza-green> <salida.txt>
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
CITES = [('fwr', 'PushBackFullWriteRunTests'), ('wpuh', 'copias de WorkspacePilotUpdateHost'), ('wpuq', 'copias de WorkspacePilotUpdateRequest'),
         ('fpt', 'PushBackFullPanelTests y PushBackEditAllTests'), ('fwp', 'PushBackFullWritePrepareTests')]


def show(path):
    return subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


def cov(label):
    """(archivo, linea) -> (hits, ramas cubiertas, ramas)"""
    path = os.path.join(CV, label + '.cobertura.xml')
    out = {}
    if not os.path.exists(path):
        return None
    for cls in ET.parse(path).getroot().iter('class'):
        name = os.path.basename(cls.get('filename').replace('\\', '/'))
        if name not in FILES:
            continue
        for line in cls.iter('line'):
            n = int(line.get('number'))
            hits = int(line.get('hits'))
            m = re.search(r'\((\d+)/(\d+)\)', line.get('condition-coverage') or '')
            got, total = (int(m.group(1)), int(m.group(2))) if m else (0, 0)
            h0, g0, t0 = out.get((name, n), (0, 0, 0))
            out[(name, n)] = (max(h0, hits), max(g0, got), max(t0, total))
    return out


# los tramos que retira el GREEN, por la pieza: cada OLD de un EDIT, en el texto de la base, para los dos archivos
spec = importlib.util.spec_from_file_location('green_pbt4d_v52', os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), 'green_pbt4d_v52.py'))
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


def member_of(name, n):
    lines = texts[name]
    for k in range(n - 1, -1, -1):
        m = re.match(r'^\s*(?:(?:public|private|internal|protected|static|sealed|override|readonly)\s+)+[\w<>\[\],.?()\s]*?\b(\w+)\s*(?:<[^>]*>)?\s*\(', lines[k])
        if m and not lines[k].strip().startswith('//'):
            return '%s:%d %s' % (name, k + 1, m.group(1))
    return name + ':?'


wpur, rest = cov('wpur'), cov('resto')
cites = {label: cov(label) for label, _ in CITES}
if rest is None:  # una lectura previa, sin la suite completa: el resto es la union de las clases de las citas (una cota inferior)
    rest = {}
    for c in cites.values():
        for key, (h, g, t) in (c or {}).items():
            h0, g0, t0 = rest.get(key, (0, 0, 0))
            rest[key] = (max(h0, h), max(g0, g), max(t0, t))
out = ['# la corrida generica que ejercita WPUR (WorkspacePilotUpdateRunTests), frente al resto de la suite de Core, en 6c2820ba',
       '# solo las lineas que no retira el GREEN (pieza %s)' % os.path.basename(PIECE)]
uncovered, partial, covered = [], [], collections.Counter()
by_member = collections.defaultdict(list)
for (name, n), (hits, got, total) in sorted(wpur.items()):
    if hits == 0 or n in retired[name]:
        continue
    r = rest.get((name, n), (0, 0, 0))
    src = texts[name][n - 1].strip()
    if r[0] == 0:
        uncovered.append((name, n, src))
        by_member[member_of(name, n)].append(n)
        continue
    if got > r[1]:
        partial.append((name, n, got, total, r[1], src))
    who = [label for label, _ in CITES if cites[label] and cites[label].get((name, n), (0, 0, 0))[0] > 0]
    covered[tuple(who) if who else ('otras',)] += 1
out.append('lineas que WPUR ejercita (sin las retiradas): %d; las ejercita tambien el resto: %d; solo WPUR: %d; con ramas que solo cubre WPUR: %d'
           % (len(uncovered) + sum(covered.values()), sum(covered.values()), len(uncovered), len(partial)))
out.append('')
out.append('## quien cubre las lineas compartidas (las clases de las citas que las ejercitan)')
for who, k in sorted(covered.items(), key=lambda kv: -kv[1]):
    out.append('  %4d %s' % (k, ', '.join(who)))
out.append('')
out.append('## las lineas que solo ejercita WPUR, por miembro')
for member, ns in sorted(by_member.items()):
    out.append('  %s: %d lineas (%s)' % (member, len(ns), ', '.join(str(n) for n in ns)))
for name, n, src in uncovered:
    out.append('    %s:%d  %s' % (name, n, src[:150]))
out.append('')
out.append('## las ramas que solo cubre WPUR (linea: WPUR cubre g de t; el resto, r)')
for name, n, got, total, r, src in partial:
    out.append('  %s:%d  %d/%d frente a %d  %s' % (name, n, got, total, r, src[:140]))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\n'.join(out[:3]))
