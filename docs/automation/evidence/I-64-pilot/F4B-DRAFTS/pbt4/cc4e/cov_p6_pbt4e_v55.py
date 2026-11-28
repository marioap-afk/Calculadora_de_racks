"""PB-T4e (paquete v5.5), precondicion 6, la cobertura de TODO el codigo de src/ que se queda (la regla permanente del Owner,
owner-pbt4d-cat4d-decisions.md, pregunta 2), linea a linea y rama por rama, con la cobertura JSON de coverlet. Es cov_p6_v54.py de PB-T4d con la
base de PB-T4e (160e019d), su pieza del GREEN, sus 19 filtros de UI y sin lo propio de la retirada (las retiradas y el conjunto vigilado de WPUR).

- La referencia: la base sin cambios, la suite COMPLETA de Core (BASE_CORE) y la UI de sus filtros (BASE_UI, que se repite: tres corridas de la
  union de los 19 filtros y una de cada filtro en su proceso, base-uifNN). Lo comparado: el GREEN literal, igual (GREEN_CORE y GREEN_UI: tres de la
  union y green-uiNN). Instrumentados: RackCad.Domain, RackCad.Application y RackCad.UI (el Plugin no lo carga ninguna prueba).
- Cada linea de la base se lleva a la del GREEN: en un archivo que el GREEN no toca, la misma; en uno que toca, con difflib (un tramo igual, linea a
  linea; un tramo cambiado del mismo largo, linea a linea, como texto cambiado; lo demas, sin pareja: una linea de la base sin pareja que se
  ejecutaba es una perdida). PB-T4e no retira nada.
- Lo que se pide: cada linea ejecutada en la base (Core o UI) y llevada al GREEN, ejecutada en el GREEN; cada rama cubierta en la base, cubierta en
  el GREEN; una linea de texto cambiado, ejecutada si lo estaba, y con todas sus ramas; una linea nueva con codigo del GREEN, ejecutada con todas sus
  ramas.
- Aparte, solo para informar: lo que Core sola pierde y cubre la UI; las lineas vigiladas (los cuerpos que el GREEN cambia); y lo que BAJA (D-S16:
  por componente emparejado; PERDIDA, VARIA o ESTABLE; ESTABLE solo por la UI exige las tres corridas juntas y al menos un filtro en su proceso).
Solo lee. Escribe UTF-8 con LF.

Uso: python -I cov_p6_pbt4e_v55.py <repo> <pieza-green> <salida.txt> BASE_CORE=<json> BASE_UI=<json>... GREEN_CORE=<json> GREEN_UI=<json>...
"""
import collections
import difflib
import importlib.util
import json
import os
import re
import subprocess
import sys

sys.dont_write_bytecode = True
REPO, PIECE, OUT = sys.argv[1:4]
ARGS = collections.defaultdict(list)
for a in sys.argv[4:]:
    k, v = a.split('=', 1)
    ARGS[k].append(v)
assert all(len(ARGS[k]) == 1 for k in ('BASE_CORE', 'GREEN_CORE')) and ARGS['BASE_UI'] and ARGS['GREEN_UI'], dict(ARGS)
REV = '160e019d773ba9095e2d5858cd593a954b8eec67'
S = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.normpath(os.path.join(S, '..', '..', 'pbt4'))
WHC = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
FVM = 'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs'

spec = importlib.util.spec_from_file_location('green_pbt4e_v55', os.path.join(PB4, 'green_pbt4e_v55.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
_, green_texts = gp.green(REPO, REV, PIECE)


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
UIF = [l for l in open(os.path.join(S, 'ui_filters_pbt4e_v55.txt'), encoding='utf-8').read().split('\n') if l]
assert len(UIF) == 19, len(UIF)


def run_label(path):
    """La clase de una corrida de UI: ('union', nombre) para las de todos los filtros juntos; ('filtro', <filtro>) para una de un filtro solo."""
    name = os.path.basename(path)
    m = re.match(r'(base-uif|green-ui)(\d\d)\.coverage\.json$', name)
    if m:
        return ('filtro', UIF[int(m.group(2)) - 1].split('~', 1)[1])
    return ('union', name)


base_ui_runs = [(run_label(p), coverage(p)) for p in ARGS['BASE_UI']]
green_ui_runs = [(run_label(p), coverage(p)) for p in ARGS['GREEN_UI']]
bu = union(*[c for _, c in base_ui_runs])
gu = union(*[c for _, c in green_ui_runs])
base_all, green_all = union(bc, bu), union(gc, gu)
files = sorted(set(base_all[0]) | set(base_all[1]))
assert files, 'sin archivos de src/ en la cobertura de la base'

mapping, changed, base_lines, green_lines = {}, set(), {}, {}
for f in sorted(green_texts):
    a, b = show(f).split('\n'), green_texts[f].split('\n')
    base_lines[f], green_lines[f] = a, b
    for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, a, b, autojunk=False).get_opcodes():
        if tag == 'equal' or (tag == 'replace' and i2 - i1 == j2 - j1):
            for k in range(i2 - i1):
                mapping[(f, i1 + k + 1)] = j1 + k + 1
                if tag == 'replace':
                    changed.add((f, i1 + k + 1))


def to_green(f, n):
    if f not in green_texts:
        return n
    return mapping.get((f, n))


def covered(cov, f, n):
    return {k for k, h in cov[1].get(f, {}).get(n, {}).items() if h > 0}


def src_line(f, n):
    lines = base_lines.get(f) or show(f).split('\n')
    base_lines[f] = lines
    return lines[n - 1].strip()[:110] if 0 < n <= len(lines) else ''


def compare(base, green):
    lost_lines, lost_branches, unpaired, changed_out, nlines, nbr = [], [], [], [], 0, 0
    for f in files:
        for n in sorted(set(base[0].get(f, {})) | set(base[1].get(f, {}))):
            bh = base[0].get(f, {}).get(n, 0)
            bset = covered(base, f, n)
            if bh == 0 and not bset:
                continue
            g = to_green(f, n)
            if g is None:
                unpaired.append('  %s:%d (hits base %d) %s' % (f, n, bh, src_line(f, n)))
                lost_lines.append('  (sin pareja) %s:%d' % (f, n))
                continue
            nlines += 1
            nbr += len(bset)
            gh = green[0].get(f, {}).get(g, 0)
            gall = green[1].get(f, {}).get(g, {})
            gset = covered(green, f, g)
            if (f, n) in changed:
                ok = (gh > 0 or bh == 0) and all(h > 0 for h in gall.values())
                changed_out.append('  %s:%d -> %d hits %d -> %d ramas base %d/%d green %d/%d %s  %s' % (
                    f, n, g, bh, gh, len(bset), len(base[1].get(f, {}).get(n, {})), len(gset), len(gall), 'bien' if ok else 'CAE', src_line(f, n)))
                if not ok:
                    lost_lines.append('  (texto cambiado) %s:%d -> %d' % (f, n, g))
                continue
            if bh > 0 and gh == 0:
                lost_lines.append('  %s:%d -> %d (hits base %d)' % (f, n, g, bh))
            if bset - gset:
                lost_branches.append('  %s:%d -> %d base %d/%d green %d/%d sin cubrir %s' % (
                    f, n, g, len(bset), len(base[1].get(f, {}).get(n, {})), len(gset), len(gall), sorted(bset - gset)))
    return lost_lines, lost_branches, unpaired, changed_out, nlines, nbr


ll, lb, unpaired, changed_out, nlines, nbr = compare(base_all, green_all)
cl, cb, _, _, _, _ = compare(bc, gc)
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
        new_lines.append('  %s:%d hits %d (Core %d) ramas %d/%d %s  %s' % (f, g, green_all[0][f][g], gc[0].get(f, {}).get(g, 0),
                                                                        sum(1 for h in gall.values() if h > 0), len(gall), 'CAE' if bad else 'bien', s[:100]))
        if bad:
            ll.append('  (nueva) %s:%d' % (f, g))


def keyed(items):
    return {re.match(r'  (?:\([^)]*\) )?(\S+:\d+)', x).group(1) for x in items}


def annotate(x):
    k = keyed([x]).pop()
    f, n = k.rsplit(':', 1)
    return x + '  ' + src_line(f, int(n))


core_only = sorted((keyed(cl) | keyed(cb)) - (keyed(ll) | keyed(lb)))


def body_range(text, signature):
    """Las lineas (1-based) del cuerpo de un metodo: desde su firma hasta la llave que la cierra."""
    lines = text.split('\n')
    start = next(i for i, l in enumerate(lines) if re.search(signature, l))
    depth, seen = 0, False
    for j in range(start, len(lines)):
        depth += lines[j].count('{') - lines[j].count('}')
        seen = seen or '{' in lines[j]
        if seen and depth == 0:
            return range(start + 1, j + 2)
    raise AssertionError(signature)


WATCH = [(WHC, r'private EditAllOutcome EditAllOfPilot\('), (WHC, r'public EditIntentOutcome TryReloadPilotEdit\('),
         (FVM, r'public void EditAll\(\)'), (FVM, r'public void BeginEdit\(\)')]
watch = []
for f, sig in WATCH:
    rng = body_range(show(f), sig)
    exe = [n for n in rng if base_all[0].get(f, {}).get(n, 0) > 0]
    keep = [n for n in exe if to_green(f, n) is not None and green_all[0].get(f, {}).get(to_green(f, n), 0) > 0]
    br_b = sum(len(covered(base_all, f, n)) for n in rng)
    br_g = sum(len(covered(green_all, f, to_green(f, n))) for n in rng if to_green(f, n) is not None)
    watch.append('  %s %s: lineas %d-%d de la base; ejecutadas en la base %d, siguen ejecutadas en el GREEN %d; ramas cubiertas base %d, GREEN %d' % (
        os.path.basename(f), sig.split('\\(')[0].split()[-1], rng[0], rng[-1], len(exe), len(keep), br_b, br_g))


# ---- D-S16: lo que BAJA (informativo; no cambia el PASA ni el CAE), como en cov_p6_v54.py.
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
drops = []
for f in files:
    for n in sorted(set(base_all[0].get(f, {})) | set(base_all[1].get(f, {}))):
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
            if not paired_drop:
                continue
            what = 'linea' if key is None else 'rama %s' % (key,)
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
stable_groups = collections.OrderedDict()
for kind, f, n, g, what, sig, full in drops:
    if kind == 'ESTABLE':
        stable_groups.setdefault((f, sig), []).append('%d%s' % (n, '' if what == 'linea' else ' ' + what[5:]))

ok = not ll and not lb
out = ['# la cobertura de PB-T4e: todo src/ que se queda, linea a linea y rama por rama (Core completa + filtros de UI), base frente al GREEN',
       '# pieza %s; BASE_CORE %s; BASE_UI %s; GREEN_CORE %s; GREEN_UI %s' % (os.path.basename(PIECE), os.path.basename(ARGS['BASE_CORE'][0]),
                                                                           ', '.join(map(os.path.basename, ARGS['BASE_UI'])), os.path.basename(ARGS['GREEN_CORE'][0]),
                                                                           ', '.join(map(os.path.basename, ARGS['GREEN_UI']))),
       'archivos de src/ instrumentados %d; lineas ejecutadas en la base que se quedan %d (ramas cubiertas %d); sin pareja en el GREEN %d; texto cambiado %d; '
       'lineas nuevas con codigo %d; lineas sin ejecutar en el GREEN %d; lineas con ramas perdidas %d' % (
           len(files), nlines, nbr, len(unpaired), len(changed_out), len(new_lines), len(ll), len(lb)),
       'cobertura ' + ('PASA' if ok else 'CAE'),
       'bajan (informativo) %d: PERDIDA %d, VARIA %d, ESTABLE %d' % (len(drops), n_lost, sum(1 for d in drops if d[0] == 'VARIA'),
                                                                   sum(1 for d in drops if d[0] == 'ESTABLE')),
       '', '## lineas que se quedan, ejecutadas en la base y no en el GREEN'] + [annotate(x) for x in ll]
out += ['', '## ramas cubiertas en la base y no en el GREEN (punto, path)'] + [annotate(x) for x in lb]
out += ['', '## lineas de la base sin pareja en el GREEN que se ejecutaban'] + unpaired
out += ['', '## solo para informar: lo que Core sola pierde y cubre la UI de los filtros (%d)' % len(core_only)] + [annotate('  ' + k) for k in core_only]
out += ['', '## vigiladas: los cuerpos que el GREEN cambia o reutiliza'] + watch
out += ['', '## texto cambiado por el GREEN'] + changed_out
out += ['', '## lineas nuevas del GREEN con codigo'] + new_lines
out += ['', '## Bajan (informativo; D-S16: no cambia el PASA ni el CAE)', '',
        'Toda linea o rama de src/ que se queda y que cubren mas corridas en la base que en el GREEN (Core y cada corrida de UI: base %d de UI, GREEN %d '
        'de UI), por componente emparejado: Core, las corridas de todos los filtros juntos y cada filtro con el mismo filtro. Cada entrada nombra la '
        'corrida, no la prueba (coverlet no da la prueba).' % (len(base_ui_runs), len(green_ui_runs)), '']
out += ['### PERDIDA y VARIA, una a una'] + [d[6] for d in drops if d[0] != 'ESTABLE'] + ['', '### ESTABLE, por archivo y por recuento']
out += ['  ESTABLE %s: %s :: %s' % (f, sig, ', '.join(ids)) for (f, sig), ids in stable_groups.items()]
with open(OUT, 'w', encoding='utf-8', newline='\n') as fh:
    fh.write('\n'.join(out) + '\n')
print('\n'.join(out[2:5]))
