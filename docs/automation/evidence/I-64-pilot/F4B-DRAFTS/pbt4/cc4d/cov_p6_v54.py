"""PB-T4d (paquete v5.4; la de la v5.3, cov_p6_v53.py, queda como historia), precondicion 6, paso de la cobertura ampliada (CA-T4d I-02; owner-pbt4d-cat4d-decisions.md, pregunta 2): TODO el codigo
de src/ que se queda, linea a linea y rama por rama, con la cobertura JSON de coverlet.

- La referencia: la base sin cambios (6c2820ba), la suite COMPLETA de Core (BASE_CORE) y la UI de sus filtros (BASE_UI: una corrida con la union
  de los filtros de PB-T4c y de PB-T4d). Lo comparado: el GREEN literal, igual (GREEN_CORE y GREEN_UI). Instrumentados: RackCad.Domain,
  RackCad.Application y RackCad.UI (el Plugin no lo carga ninguna prueba).
- Cada linea de la base se lleva a la del GREEN: en un archivo que el GREEN no toca, la misma; en uno que toca, con difflib (un tramo igual, linea
  a linea; un tramo cambiado del mismo largo, linea a linea, como texto cambiado; lo demas, retirado). Un archivo borrado, retirado entero.
- Una rama es (su punto de bifurcacion en la linea, por orden de Offset, y su Path); en una linea de texto igual el IL es el mismo y la clave casa.
- Lo que se pide: cada linea ejecutada en la base (Core o UI) y llevada al GREEN, ejecutada en el GREEN (Core o UI); cada rama cubierta en la
  base, cubierta en el GREEN. Una linea de texto cambiado, ejecutada si lo estaba, y con todas sus ramas; una linea nueva con codigo del GREEN,
  ejecutada con todas sus ramas.
- Aparte, solo para informar: lo que Core sola pierde y cubre la UI (lo que la UI cubre se mide con sus filtros antes de darlo por perdido), y
  el conjunto vigilado de la v5.2 (las 436 lineas de WPUR, r6j-cv/wpur.cobertura.xml, y sus 9 ramas).
Solo lee. Escribe UTF-8 con LF.

Uso: python -I cov_p6_v53.py <repo> <pieza-green> <r6j-cv> <salida.txt> BASE_CORE=<json> BASE_UI=<json>... GREEN_CORE=<json> GREEN_UI=<json>...
     (BASE_UI y GREEN_UI se repiten: la union de las corridas de la UI, tres en cada rama)
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
REPO, PIECE, CV, OUT = sys.argv[1:5]
ARGS = collections.defaultdict(list)
for a in sys.argv[5:]:
    k, v = a.split('=', 1)
    ARGS[k].append(v)
assert all(len(ARGS[k]) == 1 for k in ('BASE_CORE', 'GREEN_CORE')) and ARGS['BASE_UI'] and ARGS['GREEN_UI'], dict(ARGS)
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
S = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.normpath(os.path.join(S, '..', '..', 'pbt4'))
P = 'src/RackCad.Application/Systems/PushBack/Pilot/'
WATCH_RUN = {(P + 'PushBackPilotUpdateRun.cs', n) for n in (422, 460, 510, 525, 589, 594, 604, 650, 651)}
# las de CA-T4d I-01 y M-02, en la base
WATCH_CAT = [('src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotSystemRebuild.cs', n) for n in (28, 29, 31, 32, 48, 51, 60)] + \
            [('src/RackCad.Application/Systems/PushBack/PushBackCompositeEditorState.cs', 233)]

spec = importlib.util.spec_from_file_location('green_pbt4d_v53', os.path.join(PB4, 'green_pbt4d_v53.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
_, green_texts, deleted = gp.green(REPO, REV, PIECE)


def show(path):
    return subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


def rel(file_path):
    p = file_path.replace('\\', '/')
    i = p.find('/src/')
    return p[i + 1:] if i >= 0 else None


def coverage(path):
    """ruta -> {linea: hits}; ruta -> {linea: {(punto, path): hits}}"""
    data = json.load(open(path, encoding='utf-8'))
    hits = collections.defaultdict(collections.Counter)
    raw = collections.defaultdict(list)
    for module in data.values():
        for file_path, classes in module.items():
            r = rel(file_path)
            if r is None:
                continue
            for cls_name, methods in classes.items():
                for method, body in methods.items():
                    for n, h in body.get('Lines', {}).items():
                        hits[r][int(n)] += h
                    for b in body.get('Branches', []):
                        raw[(r, b['Line'])].append((cls_name + '::' + method, b['Offset'], b['Path'], b['Hits']))
    branches = collections.defaultdict(dict)
    for (r, line), items in raw.items():
        points = sorted({(m, off) for m, off, _, _ in items}, key=lambda p: (p[1], p[0]))
        rank = {p: i for i, p in enumerate(points)}
        out = collections.Counter()
        for m, off, pth, h in items:
            out[(rank[(m, off)], pth)] += h
        branches[r][line] = out
    return hits, branches


def union(*covs):
    hits = collections.defaultdict(collections.Counter)
    branches = collections.defaultdict(lambda: collections.defaultdict(collections.Counter))
    for h, b in covs:
        for r, lines in h.items():
            for n, v in lines.items():
                hits[r][n] += v
        for r, lines in b.items():
            for n, keys in lines.items():
                for k, v in keys.items():
                    branches[r][n][k] += v
    return hits, branches


bc, gc = coverage(ARGS['BASE_CORE'][0]), coverage(ARGS['GREEN_CORE'][0])
# la UI, la union de sus corridas (D-S16: tres de todos los filtros juntos y una de cada filtro en su proceso), igual en las dos ramas
FILTERS = {'base-uif': [l for l in open(os.path.join(S, 'ui_filters_base19_v53.txt'), encoding='utf-8').read().split('\n') if l],
           'green-ui': [l for l in open(os.path.join(S, 'ui_filters_pbt4d_v53.txt'), encoding='utf-8').read().split('\n') if l]}


def run_label(path):
    """La clase de una corrida de UI: ('union', n) para las de todos los filtros juntos; ('filtro', <filtro>) para una de un filtro solo."""
    name = os.path.basename(path)
    m = re.match(r'(base-uif|green-ui)-?(\d\d)\.coverage\.json$', name)
    if m:
        return ('filtro', FILTERS[m.group(1)][int(m.group(2)) - 1].split('~', 1)[1])
    return ('union', name)


base_ui_runs = [(run_label(p), coverage(p)) for p in ARGS['BASE_UI']]
green_ui_runs = [(run_label(p), coverage(p)) for p in ARGS['GREEN_UI']]
bu = union(*[c for _, c in base_ui_runs])
gu = union(*[c for _, c in green_ui_runs])
base_all, green_all = union(bc, bu), union(gc, gu)
files = sorted(set(base_all[0]) | set(base_all[1]))
assert files, 'sin archivos de src/ en la cobertura de la base'

# la base -> el GREEN, linea a linea
mapping, changed, base_lines, green_lines = {}, set(), {}, {}
for f in files:
    if f in deleted:
        continue
    if f not in green_texts:
        continue  # el GREEN no lo toca: la misma linea (abajo)
    a, b = show(f).split('\n'), green_texts[f].split('\n')
    base_lines[f], green_lines[f] = a, b
    for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, a, b, autojunk=False).get_opcodes():
        if tag == 'equal' or (tag == 'replace' and i2 - i1 == j2 - j1):
            for k in range(i2 - i1):
                mapping[(f, i1 + k + 1)] = j1 + k + 1
                if tag == 'replace':
                    changed.add((f, i1 + k + 1))


def to_green(f, n):
    if f in deleted:
        return None
    if f not in green_texts:
        return n
    return mapping.get((f, n))


def covered(cov, f, n):
    return {k for k, h in cov[1].get(f, {}).get(n, {}).items() if h > 0}


def compare(base, green, label):
    """Las lineas y ramas cubiertas en `base` que se quedan y no lo estan en `green`."""
    lost_lines, lost_branches, retired, changed_out, nlines, nbr = [], [], 0, [], 0, 0
    for f in files:
        for n in sorted(set(base[0].get(f, {})) | set(base[1].get(f, {}))):
            bh = base[0].get(f, {}).get(n, 0)
            bset = covered(base, f, n)
            if bh == 0 and not bset:
                continue
            g = to_green(f, n)
            if g is None:
                retired += 1
                continue
            nlines += 1
            nbr += len(bset)
            gh = green[0].get(f, {}).get(g, 0)
            gall = green[1].get(f, {}).get(g, {})
            gset = covered(green, f, g)
            if (f, n) in changed:
                ok = (gh > 0 or bh == 0) and all(h > 0 for h in gall.values())
                changed_out.append('  %s:%d -> %d hits %d -> %d ramas base %d/%d green %d/%d %s' % (
                    f, n, g, bh, gh, len(bset), len(base[1].get(f, {}).get(n, {})), len(gset), len(gall), 'bien' if ok else 'CAE'))
                if not ok:
                    lost_lines.append('  (texto cambiado) %s:%d -> %d' % (f, n, g))
                continue
            if bh > 0 and gh == 0:
                lost_lines.append('  %s:%d -> %d (hits base %d)' % (f, n, g, bh))
            if bset - gset:
                lost_branches.append('  %s:%d -> %d base %d/%d green %d/%d sin cubrir %s' % (
                    f, n, g, len(bset), len(base[1].get(f, {}).get(n, {})), len(gset), len(gall), sorted(bset - gset)))
    return lost_lines, lost_branches, retired, changed_out, nlines, nbr


def src_line(f, n):
    lines = base_lines.get(f) or show(f).split('\n')
    base_lines[f] = lines
    return lines[n - 1].strip()[:110] if 0 < n <= len(lines) else ''


ll, lb, retired, changed_out, nlines, nbr = compare(base_all, green_all, 'union')
cl, cb, _, _, _, _ = compare(bc, gc, 'core')
# las lineas nuevas del GREEN, con codigo, en los archivos que cambia
mapped_green = collections.defaultdict(set)
for (f, n), g in mapping.items():
    mapped_green[f].add(g)
new_lines = []
for f in sorted(green_lines):
    for g, text in enumerate(green_lines[f], 1):
        s = text.strip()
        if g in mapped_green[f] or not s or s.startswith('//') or g not in green_all[0].get(f, {}):
            continue
        gall = green_all[1].get(f, {}).get(g, {})
        bad = green_all[0][f][g] == 0 or any(h == 0 for h in gall.values())
        new_lines.append('  %s:%d hits %d ramas %d/%d %s  %s' % (f, g, green_all[0][f][g], sum(1 for h in gall.values() if h > 0), len(gall),
                                                                'CAE' if bad else 'bien', s[:100]))
        if bad:
            ll.append('  (nueva) %s:%d' % (f, g))


def keyed(items):
    return {re.match(r'  (?:\([^)]*\) )?(\S+:\d+)', x).group(1) for x in items}


def annotate(x):
    k = keyed([x]).pop()
    f, n = k.rsplit(':', 1)
    return x + '  ' + src_line(f, int(n))


core_only = sorted((keyed(cl) | keyed(cb)) - (keyed(ll) | keyed(lb)))
# el conjunto vigilado de la v5.2: las 436 lineas de WPUR (r6j) y sus 9 ramas
wpur = set()
for cls in ET.parse(os.path.join(CV, 'wpur.cobertura.xml')).getroot().iter('class'):
    r = rel('/' + cls.get('filename').replace('\\', '/')) or cls.get('filename').replace('\\', '/')
    if not r.startswith('src/'):
        r = P + os.path.basename(r) if os.path.basename(r) in ('PushBackPilotUpdateRun.cs', 'PushBackPilotUpdatePlan.cs') else None
    if r and os.path.basename(r) in ('PushBackPilotUpdateRun.cs', 'PushBackPilotUpdatePlan.cs'):
        for line in cls.iter('line'):
            if int(line.get('hits')) > 0:
                wpur.add((r, int(line.get('number'))))
w_kept = [(f, n) for f, n in sorted(wpur) if to_green(f, n) is not None]
w_bad = [(f, n) for f, n in w_kept if green_all[0].get(f, {}).get(to_green(f, n), 0) == 0 or (covered(base_all, f, n) - covered(green_all, f, to_green(f, n)) and (f, n) not in changed)]
watch = []
for f, n in sorted(WATCH_RUN) + WATCH_CAT:
    g = to_green(f, n)
    watch.append('  %s:%d -> %s base hits %d ramas %s de %d; green hits %s ramas %s de %s  %s' % (
        os.path.basename(f), n, g, base_all[0].get(f, {}).get(n, 0), sorted(covered(base_all, f, n)), len(base_all[1].get(f, {}).get(n, {})),
        green_all[0].get(f, {}).get(g, 0) if g else '-', sorted(covered(green_all, f, g)) if g else '-',
        len(green_all[1].get(f, {}).get(g, {})) if g else '-', src_line(f, n)))
# ---- D-S16: lo que BAJA (informativo; no cambia el PASA ni el CAE). Por cada linea y cada rama de src/ que se queda y que la base cubre: cuantas
# corridas la cubren en cada lado (Core y cada corrida de UI). Entra si la base tiene mas. Los componentes se comparan emparejados: Core con Core,
# las tres de todos los filtros juntos con las tres, y cada filtro con el mismo filtro; las corridas de un filtro que solo esta en la base (su
# clase se borra entera en el RED) se cuentan aparte. Tipo:
#   - PERDIDA: una corrida con pruebas que se borran la cubria siempre en la base (Core; o las tres juntas) y en el GREEN no la cubre Core ni las
#     tres juntas (solo, a ratos, alguna corrida): se trata como camino perdido (la regla del Owner: sustituta);
#   - VARIA: la base tampoco la cubria siempre (ninguna de Core ni de las tres juntas, o no las tres): una cobertura que varia;
#   - ESTABLE: el GREEN la sigue cubriendo con Core, o con las tres juntas Y al menos un filtro en su proceso (recomprobacion 1 de CA-T4d, M-11 b;
#     D-S16). Cada entrada nombra la corrida que la cubria, no la prueba: coverlet no da la prueba (M-11 a).
def run_sets(core, ui_runs, f, n, key=None):
    def hit(c):
        if key is None:
            return c[0].get(f, {}).get(n, 0) > 0
        return c[1].get(f, {}).get(n, {}).get(key, 0) > 0
    union_n = sum(1 for (kind, _), c in ui_runs if kind == 'union' and hit(c))
    union_t = sum(1 for (kind, _), _c in ui_runs if kind == 'union')
    filt = {lab for (kind, lab), c in ui_runs if kind == 'filtro' and hit(c)}
    return hit(core), union_n, union_t, filt


g_filters = {lab for (kind, lab), _ in green_ui_runs if kind == 'filtro'}
b_filters = {lab for (kind, lab), _ in base_ui_runs if kind == 'filtro'}
only_base_filters = b_filters - g_filters
drops, drops_only_base = [], collections.defaultdict(list)
for f in files:
    lines_b = set(base_all[0].get(f, {})) | set(base_all[1].get(f, {}))
    for n in sorted(lines_b):
        g = to_green(f, n)
        if g is None:
            continue
        keys = [None] + ([] if (f, n) in changed else sorted(k for k, h in base_all[1].get(f, {}).get(n, {}).items() if h > 0))
        for key in keys:
            bcore, bun, but, bfil = run_sets(bc, base_ui_runs, f, n, key)
            if not (bcore or bun or bfil):
                continue
            gcore, gun, gut, gfil = run_sets(gc, green_ui_runs, f, g, key)
            nb = int(bcore) + bun + len(bfil)
            ng = int(gcore) + gun + len(gfil)
            if nb <= ng:
                continue
            paired_drop = (bcore and not gcore) or bun > gun or bool((bfil & g_filters) - gfil)
            what = 'linea' if key is None else 'rama %s' % (key,)
            if not paired_drop:
                drops_only_base[f].append('%d%s' % (n, '' if key is None else ' %s' % (key,)))
                continue
            stable_base = bcore or bun == but
            stable_green = gcore or (gun == gut and bool(gfil))
            kind = 'ESTABLE' if stable_green else ('PERDIDA' if stable_base else 'VARIA')
            lost_runs = (['Core'] if bcore and not gcore else []) + (['juntas %d->%d de %d' % (bun, gun, but)] if bun > gun else []) + \
                        ['filtro %s' % x for x in sorted((bfil & g_filters) - gfil)]
            sig = 'base %d (Core %s, juntas %d/%d, filtros %d) GREEN %d (Core %s, juntas %d/%d, filtros %d); la cubria en la base y no en el GREEN: %s' % (
                nb, 'si' if bcore else 'no', bun, but, len(bfil), ng, 'si' if gcore else 'no', gun, gut, len(gfil), ', '.join(lost_runs))
            drops.append((kind, f, n, g, what, sig, '  %s %s:%d -> %d %s: %s  %s' % (kind, f, n, g, what, sig, src_line(f, n))))
order = {'PERDIDA': 0, 'VARIA': 1, 'ESTABLE': 2}
drops.sort(key=lambda d: (order[d[0]], d[1], d[2], d[4]))
n_lost = sum(1 for d in drops if d[0] == 'PERDIDA')
# las ESTABLE, agrupadas por archivo y por recuento (cada linea o rama con el suyo; sin el texto de la fuente, para que el informe se pueda leer)
stable_groups = collections.OrderedDict()
for kind, f, n, g, what, sig, full in drops:
    if kind == 'ESTABLE':
        stable_groups.setdefault((f, sig), []).append('%d%s' % (n, '' if what == 'linea' else ' ' + what[5:]))

ok = not ll and not lb
out = ['# la cobertura ampliada de PB-T4d: todo src/ que se queda, linea a linea y rama por rama (Core completa + filtros de UI), base frente al GREEN',
       '# pieza %s; BASE_CORE %s; BASE_UI %s; GREEN_CORE %s; GREEN_UI %s' % (os.path.basename(PIECE), os.path.basename(ARGS['BASE_CORE'][0]),
                                                                           ', '.join(map(os.path.basename, ARGS['BASE_UI'])), os.path.basename(ARGS['GREEN_CORE'][0]),
                                                                           ', '.join(map(os.path.basename, ARGS['GREEN_UI']))),
       'archivos de src/ instrumentados %d; lineas ejecutadas en la base que se quedan %d (ramas cubiertas %d); retiradas por el GREEN %d; texto cambiado %d; '
       'lineas sin ejecutar en el GREEN %d; lineas con ramas perdidas %d' % (len(files), nlines, nbr, retired, len(changed_out), len(ll), len(lb)),
       'cobertura ' + ('PASA' if ok else 'CAE'),
       'conjunto vigilado de la v5.2: %d lineas de WPUR, %d se quedan, %d sin cubrir en el GREEN; bajan (informativo) %d, de ellas PERDIDA %d' % (
           len(wpur), len(w_kept), len(w_bad), len(drops), n_lost),
       '', '## lineas que se quedan, ejecutadas en la base y no en el GREEN'] + [annotate(x) for x in ll]
out += ['', '## ramas cubiertas en la base y no en el GREEN (punto, path)'] + [annotate(x) for x in lb]
out += ['', '## solo para informar: lo que Core sola pierde y cubre la UI de los filtros (%d)' % len(core_only)] + [annotate('  ' + k) for k in core_only]
out += ['', '## Core sola, lineas'] + [annotate(x) for x in cl] + ['', '## Core sola, ramas'] + [annotate(x) for x in cb]
out += ['', '## vigiladas: las 9 de la corrida (v5.2) y las de CA-T4d I-01 y M-02'] + watch
out += ['', '## texto cambiado por el GREEN'] + changed_out
out += ['', '## lineas nuevas del GREEN con codigo'] + new_lines
out += ['', '## el conjunto vigilado de la v5.2 sin cubrir'] + ['  %s:%d' % x for x in w_bad]
out += ['', '## Bajan (informativo; D-S16: no cambia el PASA ni el CAE)', '',
        'Toda linea o rama de src/ que se queda y que cubren mas corridas en la base que en el GREEN (Core y cada corrida de UI: base %d de UI, '
        'GREEN %d de UI). Por componente emparejado: Core, las %d corridas de todos los filtros juntos y cada filtro con el mismo filtro. Tipo: '
        'PERDIDA (una corrida la cubria siempre en la base y en el GREEN no la cubre ni Core ni las tres juntas: camino perdido, sustituta), VARIA '
        '(la base tampoco la cubria siempre) o ESTABLE (el GREEN la sigue cubriendo con Core, o con las tres juntas y al menos un filtro en su proceso). Cada entrada nombra la corrida, no la prueba (coverlet no da la prueba).' % (
            len(base_ui_runs), len(green_ui_runs), sum(1 for (k, _), _c in base_ui_runs if k == 'union')),
        'entradas %d: PERDIDA %d, VARIA %d, ESTABLE %d; y, solo por la corrida de un filtro que no esta en el GREEN (%s), %d lineas o ramas en %d archivos' % (
            len(drops), n_lost, sum(1 for d in drops if d[0] == 'VARIA'), sum(1 for d in drops if d[0] == 'ESTABLE'),
            ', '.join(sorted(only_base_filters)) or 'ninguno', sum(len(v) for v in drops_only_base.values()), len(drops_only_base)), '']
out += ['### PERDIDA y VARIA, una a una'] + [d[6] for d in drops if d[0] != 'ESTABLE'] + ['',
        '### ESTABLE, por archivo y por recuento (las lineas, por numero de la base; una rama, con su (punto, path))']
out += ['  ESTABLE %s: %s :: %s' % (f, sig, ', '.join(ids)) for (f, sig), ids in stable_groups.items()]
out += ['', '### solo por la corrida de un filtro que solo esta en la base (su clase se borra entera; la siguen cubriendo otras corridas)']
out += ['  %s: %s' % (f, ', '.join(v)) for f, v in sorted(drops_only_base.items())]
with open(OUT, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('\n'.join(out) + '\n')
print('\n'.join(out[2:5]))
