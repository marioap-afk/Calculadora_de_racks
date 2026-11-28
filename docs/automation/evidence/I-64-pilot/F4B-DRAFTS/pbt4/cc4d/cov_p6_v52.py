"""PB-T4d (paquete v5.2), precondicion 6: la cobertura de la corrida generica que se queda, con la retirada aplicada, linea a linea y rama por rama
(owner-pbt4d-run-coverage-decision.md y owner-pbt4d-deadcode-27-decisions.md, pregunta 2).

- Las lineas: las 436 de PushBackPilotUpdateRun.cs y PushBackPilotUpdatePlan.cs que WPUR ejercitaba en la base (r6j-cv/wpur.cobertura.xml),
  fuera de los tramos que el GREEN borra (cada OLD de un EDIT con NEW vacio), como en cov_v52.py.
- Cada linea de la base se lleva a la del GREEN (green_pbt4d_v52.green) con difflib: un tramo igual, linea a linea; un tramo cambiado del mismo
  largo, linea a linea (texto cambiado); lo demas, retirado por el GREEN.
- La referencia, rama por rama: la cobertura JSON de coverlet de la suite completa de Core en la base sin cambios (r6p-bc), la union de todas las
  pruebas, WPUR incluida. La del GREEN: la suite completa de Core sobre el GREEN literal. Una rama es (su punto de bifurcacion en la linea, por
  orden de Offset, y su Path): en una linea de texto igual el IL es el mismo y la clave casa.
- Lo que se pide: cada linea llevada al GREEN, ejecutada; cada rama cubierta en la base, cubierta en el GREEN. Las lineas de texto cambiado (la
  rama muerta de BuildCore) y las retiradas se listan aparte, con sus ramas.
Solo lee. Escribe UTF-8 con LF.

Uso: python -I cov_p6_v52.py <repo> <r6j-cv> <base.coverage.json> <green.coverage.json> <pieza-green> <salida.txt>
"""
import collections
import difflib
import importlib.util
import json
import os
import re
import subprocess
import sys
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
REPO, CV, BASE_JSON, GREEN_JSON, PIECE, OUT = sys.argv[1:7]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
P = 'src/RackCad.Application/Systems/PushBack/Pilot/'
FILES = ['PushBackPilotUpdateRun.cs', 'PushBackPilotUpdatePlan.cs']
S = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.normpath(os.path.join(S, '..', '..', 'pbt4'))
# Las ramas que el alto del relevo seno (v52/sub/cov-sides.txt) y las dos dudosas.
WATCH = {('PushBackPilotUpdateRun.cs', n) for n in (422, 460, 510, 525, 589, 594, 604, 650, 651)}


def show(path):
    return subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


spec = importlib.util.spec_from_file_location('green_pbt4d_v52', os.path.join(PB4, 'green_pbt4d_v52.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
_, green_texts, _ = gp.green(REPO, REV, PIECE)
base = {f: show(P + f).split('\n') for f in FILES}
green = {f: green_texts[P + f].split('\n') for f in FILES}

# las lineas que retira el GREEN (los OLD de los EDIT con NEW vacio), en la base
retired = collections.defaultdict(set)
for item in gp.blocks(PIECE):
    if item[0] != 'EDIT' or os.path.basename(item[1]) not in FILES or item[3] != '':
        continue
    text = '\n'.join(base[os.path.basename(item[1])])
    at = text.find(item[2])
    if at < 0:
        continue
    first = text.count('\n', 0, at) + 1
    for k in range(first, first + item[2].count('\n')):
        retired[os.path.basename(item[1])].add(k)

# las 436: lo que WPUR ejecuto en la base (Cobertura de r6j)
wpur = set()
for cls in ET.parse(os.path.join(CV, 'wpur.cobertura.xml')).getroot().iter('class'):
    name = os.path.basename(cls.get('filename').replace('\\', '/'))
    if name in FILES:
        for line in cls.iter('line'):
            n = int(line.get('number'))
            if int(line.get('hits')) > 0 and n not in retired[name]:
                wpur.add((name, n))

# la base -> el GREEN, linea a linea
mapping, changed = {}, set()
for f in FILES:
    for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, base[f], green[f], autojunk=False).get_opcodes():
        if tag == 'equal' or (tag == 'replace' and i2 - i1 == j2 - j1):
            for k in range(i2 - i1):
                mapping[(f, i1 + k + 1)] = j1 + k + 1
                if tag == 'replace':
                    changed.add((f, i1 + k + 1))


def coverage(path):
    """(archivo, linea) -> hits; (archivo, linea) -> {(punto, path): hits}"""
    data = json.load(open(path, encoding='utf-8'))
    hits = collections.Counter()
    raw = collections.defaultdict(list)
    for module in data.values():
        for file_path, classes in module.items():
            name = os.path.basename(file_path.replace('\\', '/'))
            if name not in FILES:
                continue
            for cls_name, methods in classes.items():
                for method, body in methods.items():
                    for n, h in body.get('Lines', {}).items():
                        hits[(name, int(n))] += h
                    for b in body.get('Branches', []):
                        raw[(name, b['Line'])].append((cls_name + '::' + method, b['Offset'], b['Path'], b['Hits']))
    branches = {}
    for key, items in raw.items():
        points = sorted({(m, off) for m, off, _, _ in items}, key=lambda p: (p[1], p[0]))
        rank = {p: i for i, p in enumerate(points)}
        out = collections.Counter()
        for m, off, pth, h in items:
            out[(rank[(m, off)], pth)] += h
        branches[key] = out
    return hits, branches


bh, bb = coverage(BASE_JSON)
gh, gb = coverage(GREEN_JSON)
lines_out, uncovered, missing, retired_lines, changed_lines, watch = [], [], [], [], [], []
for f, n in sorted(wpur):
    src = base[f][n - 1].strip()
    if (f, n) not in mapping:
        retired_lines.append('  %s:%d  %s' % (f, n, src[:130]))
        continue
    g = mapping[(f, n)]
    if gh[(f, g)] == 0:
        uncovered.append('  %s:%d -> %d  %s' % (f, n, g, src[:130]))
    bset = {k for k, h in bb.get((f, n), {}).items() if h > 0}
    gall = gb.get((f, g), {})
    gset = {k for k, h in gall.items() if h > 0}
    if (f, n) in changed:
        changed_lines.append('  %s:%d -> %d base %d/%d green %d/%d  %s  =>  %s' % (
            f, n, g, len(bset), len(bb.get((f, n), {})), len(gset), len(gall), src[:90], green[f][g - 1].strip()[:90]))
    elif bset - gset:
        missing.append('  %s:%d -> %d base %d/%d green %d/%d sin cubrir %s  %s' % (
            f, n, g, len(bset), len(bb.get((f, n), {})), len(gset), len(gall), sorted(bset - gset), src[:110]))
    if (f, n) in WATCH:
        watch.append('  %s:%d -> %d base %s de %d; green %s de %d  %s' % (
            f, n, g, sorted(bset), len(bb.get((f, n), {})), sorted(gset), len(gall), src[:100]))
# las lineas del GREEN que no vienen de una linea de la base (los tramos cambiados de otro largo): su ejecucion y sus ramas en el GREEN
mapped_green = collections.defaultdict(set)
for (f, n), g in mapping.items():
    mapped_green[f].add(g)
new_lines = []
for f in FILES:
    for g in range(1, len(green[f]) + 1):
        src = green[f][g - 1].strip()
        if g in mapped_green[f] or not src or src.startswith('//') or (f, g) not in gh:
            continue
        gall = gb.get((f, g), {})
        new_lines.append('  %s:%d hits %d ramas %d/%d  %s' % (f, g, gh[(f, g)], sum(1 for h in gall.values() if h > 0), len(gall), src[:120]))
        if gh[(f, g)] == 0 or any(h == 0 for h in gall.values()):
            uncovered.append('  %s:(nueva) %d  %s' % (f, g, src[:130]))
ok = not uncovered and not missing
out = ['# la corrida generica que se queda, con la retirada aplicada: las lineas que WPUR ejercitaba en la base y sus ramas, en el GREEN',
       '# referencia: %s; GREEN: %s; pieza %s' % (os.path.basename(BASE_JSON), os.path.basename(GREEN_JSON), os.path.basename(PIECE)),
       'lineas %d; llevadas al GREEN %d (texto cambiado %d); retiradas por el GREEN %d; sin ejecutar en el GREEN %d; lineas con ramas de la base sin '
       'cubrir en el GREEN %d' % (len(wpur), len(wpur) - len(retired_lines), len(changed_lines), len(retired_lines), len(uncovered), len(missing)),
       'cobertura ' + ('PASA' if ok else 'CAE'), '', '## sin ejecutar en el GREEN'] + uncovered + ['', '## ramas cubiertas en la base y no en el GREEN (punto, path)'] + missing
out += ['', '## las ramas vigiladas (las siete del alto del relevo, 525 y 594): las cubiertas, (punto, path)'] + watch
out += ['', '## texto cambiado por el GREEN (la rama muerta de BuildCore): ramas cubiertas de las que hay'] + changed_lines
out += ['', '## retiradas por el GREEN'] + retired_lines
out += ['', '## las lineas nuevas del GREEN, con codigo, en los tramos que cambia (tienen que ejecutarse con todas sus ramas)'] + new_lines
with open(OUT, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('\n'.join(out) + '\n')
print('\n'.join(out[2:4]))
