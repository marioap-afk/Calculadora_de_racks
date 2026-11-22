"""Revision v3 (solo lectura): equivalencia empirica de la propiedad 10 entre la v2 y la v3.
Extrae el bloque de la propiedad 10 TAL CUAL del texto de cada script (desde '# 10.' hasta 'return P, N'), lo envuelve en una funcion
p10(p, ca, changed, ct) que corre con los globales del propio modulo (covered, readme10_mutation, itertools de ese script) y compara
el booleano sobre entradas sinteticas pequenas (n <= 12): exhaustivas, de borde y aleatorias, con prefijos, duplicados, cadena vacia,
p igual a un prefijo, changed vacio y p = None. Ademas compara el resultado/excepcion con elementos no cadena.
Uso: python -I -B p10_equiv.py <v2.py> <v3.py> <salida.json> [semilla]"""
import itertools, json, random, sys, time, types

v2p, v3p, outp = sys.argv[1], sys.argv[2], sys.argv[3]
SEED = int(sys.argv[4]) if len(sys.argv) > 4 else 20261122
MAIN = "if __name__ == '__main__':"


def load(path, name):
    src = open(path, encoding='utf-8').read()
    assert src.count(MAIN) == 1
    mod = types.ModuleType(name)
    mod.__file__ = path
    exec(compile(src.replace(MAIN, 'if False:'), name, 'exec'), mod.__dict__)
    return mod, src


def make_p10(src, mod, name):
    lines = src.splitlines(keepends=True)
    i = [k for k, l in enumerate(lines) if l.startswith('    # 10. ninguna mutacion alternativa')]
    j = [k for k, l in enumerate(lines) if l.startswith('    return P, N, all(P.values())')]
    assert len(i) == 1 and len(j) == 1 and i[0] < j[0]
    body = ''.join(lines[i[0]:j[0]])
    fsrc = ('def p10(p, ca, changed, ct):\n    P, N = {}, {}\n' + body +
            "    return P['10_NoAlternativeOneFieldMutation'], N['AlternativeCandidatesExcludingPath']\n")
    g = mod.__dict__  # los auxiliares del propio script
    exec(compile(fsrc, name + '-p10', 'exec'), g)
    return g.pop('p10'), body


m2, s2 = load(v2p, 'v2')
m3, s3 = load(v3p, 'v3')
f2, body2 = make_p10(s2, m2, 'v2')
f3, body3 = make_p10(s3, m3, 'v3')
assert "len(excl) > 0 and all(c != ca for c in excl)" in body2 and 'itertools.combinations(ca, r)' in body2
assert "P['10_NoAlternativeOneFieldMutation'] = p is not None and covered(p, ca) and not covered(p, ct.get('ForbiddenWriteScope', []))" in body3
# los auxiliares que usa el bloque son identicos en los dos modulos (mismo codigo objeto)
for fn in ('covered', 'readme10_mutation'):
    assert getattr(m2, fn).__code__.co_code == getattr(m3, fn).__code__.co_code, fn


def run(f, p, ca, changed, forb, with_key=True):
    ct = {'AllowedWriteScope': ca}
    if with_key:
        ct['ForbiddenWriteScope'] = forb
    try:
        v, n = f(p, list(ca) if isinstance(ca, list) else ca, list(changed), ct)
        return ('ok', v), n
    except Exception as ex:  # noqa
        return ('exc', type(ex).__name__), None


stats = {'total': 0, 'mismatch': 0, 'v2True': 0, 'v2False': 0, 'pNone': 0, 'pIsPrefix': 0, 'caDup': 0, 'changedEmpty': 0,
         'coveredByPrefixOnly': 0, 'forbiddenHit': 0, 'excl_min_when_p': None, 'maxN': 0, 'byN': {}}
mismatches = []


def cmp(p, ca, changed, forb, tag, with_key=True):
    r2, n2 = run(f2, p, ca, changed, forb, with_key)
    r3, _ = run(f3, p, ca, changed, forb, with_key)
    stats['total'] += 1
    stats['byN'][len(ca)] = stats['byN'].get(len(ca), 0) + 1
    stats['maxN'] = max(stats['maxN'], len(ca))
    if r2 != r3:
        stats['mismatch'] += 1
        if len(mismatches) < 50:
            mismatches.append({'tag': tag, 'p': p, 'ca': ca, 'changed': changed, 'forbidden': forb, 'v2': r2, 'v3': r3})
        return
    if r2[0] == 'ok':
        stats['v2True' if r2[1] else 'v2False'] += 1
        if p is not None and n2 is not None:
            stats['excl_min_when_p'] = n2 if stats['excl_min_when_p'] is None else min(stats['excl_min_when_p'], n2)
    if p is None:
        stats['pNone'] += 1
    elif p.endswith('/'):
        stats['pIsPrefix'] += 1
    if len(set(ca)) != len(ca):
        stats['caDup'] += 1
    if not changed:
        stats['changedEmpty'] += 1
    if p is not None and p not in ca and any(e.endswith('/') and p.startswith(e) for e in ca):
        stats['coveredByPrefixOnly'] += 1
    if p is not None and any(p == e or (e.endswith('/') and p.startswith(e)) for e in forb):
        stats['forbiddenHit'] += 1


t0 = time.time()
# 1) exhaustivo: ca de longitud 0..3 sobre 7 fichas (con repeticion y orden), p, changed y forbidden de listas fijas
U7 = ['a', 'a/', 'a/b', 'a/b/', 'b', 'ab', '']
PS = [None, 'a', 'a/', 'a/b', 'a/b/', 'b', 'ab', '', 'a/b/c', 'c']
CH = lambda p: [[], ['a/b'], ['a', 'a/b'], ['a/b/c', 'a/c'], ['b', 'a'], ['a/', 'a/b'], ['a', 'a'], ([p] if p is not None else ['z']),
                ([p, 'a/b/c'] if p is not None else ['a/b/c']), ([p, p] if p is not None else [])]
FB = lambda p: [[], ['a/'], ['a/b'], ['b/', 'ab'], ([p] if p is not None else ['q']), ['']]
for n in range(4):
    for ca in itertools.product(U7, repeat=n):
        ca = list(ca)
        for p in PS:
            for ch in CH(p):
                for fb in FB(p):
                    cmp(p, ca, ch, fb, 'exh')
t_exh = time.time() - t0
n_exh = stats['total']
# 2) bordes nombrados
EDGE = [
    ('p igual a un prefijo del alcance', 'a/', ['a/'], ['a/'], []),
    ('p cubierto solo por prefijo', 'a/b/c', ['a/', 'x'], ['a/b/c'], []),
    ('p cubierto por prefijo y exacto', 'a/b', ['a/', 'a/b'], ['a/b', 'a/c'], []),
    ('p no cubierto', 'c', ['a/', 'b'], ['c'], []),
    ('changed vacio con p', 'a', ['a'], [], []),
    ('changed vacio y p None', None, ['a'], [], []),
    ('alcance vacio', 'a', [], ['a'], []),
    ('duplicados de p en el alcance', 'a', ['a', 'a', 'b'], ['a', 'b'], []),
    ('duplicados del prefijo', 'a/x', ['a/', 'a/'], ['a/x'], []),
    ('p prohibido exacto', 'a', ['a', 'b'], ['a'], ['a']),
    ('p prohibido por prefijo', 'a/x', ['a/x'], ['a/x'], ['a/']),
    ('prefijo prohibido que no cubre', 'a/x', ['a/x'], ['a/x'], ['a/x/']),
    ('cadena vacia como p', '', [''], [''], []),
    ('cadena vacia en el alcance, p distinto', 'a', [''], ['a'], []),
    ('"/" como prefijo universal de rutas con /', '/a', ['/'], ['/a'], []),
    ('readme10 sobre prefijo que cubre p y otros', 'a/b', ['a/'], ['a/b', 'a/c', 'a/d'], []),
    ('p no es sorted(changed)[0]', 'b', ['a', 'b'], ['a', 'b'], []),
    ('p = sorted(changed)[0] cubierto por prefijo', 'a/a', ['a/', 'b'], ['a/a', 'a/b', 'b'], []),
    ('sin clave ForbiddenWriteScope', 'a', ['a'], ['a'], None),
]
edge_rows = []
for d, p, ca, ch, fb in EDGE:
    before = stats['mismatch']
    cmp(p, ca, ch, fb or [], 'edge:' + d, with_key=fb is not None)
    r2, n2 = run(f2, p, ca, ch, fb or [], fb is not None)
    r3, n3 = run(f3, p, ca, ch, fb or [], fb is not None)
    edge_rows.append({'Edge': d, 'p': p, 'ca': ca, 'changed': ch, 'forbidden': fb, 'v2': r2, 'v2_excl': n2, 'v3': r3, 'Same': stats['mismatch'] == before})
# 3) aleatorio con n <= 12
rnd = random.Random(SEED)
U = ['a', 'a/', 'a/b', 'a/b/', 'a/b/c', 'a/b/c/', 'a/c', 'ab', 'ab/', 'b', 'b/', 'b/a', 'c', '', '/', '/a', 'src/X/A.cs', 'src/X/', 'src/', 'tests/T/TA.cs']
t1 = time.time()
n_rand = 0
for trial in range(60000):
    n = rnd.choice([0, 1, 2, 3, 4, 5, 6, 7, 8]) if trial < 52000 else rnd.choice([9, 10, 11, 12])
    ca = [rnd.choice(U) for _ in range(n)]
    if ca and rnd.random() < 0.3:
        ca.insert(rnd.randrange(len(ca) + 1), rnd.choice(ca))  # duplicado explicito
        ca = ca[:12]
    mode = rnd.random()
    changed = [rnd.choice(U) for _ in range(rnd.randrange(0, 7))]
    if mode < 0.35:  # p derivado como en evaluate: real = ca, mutado = README 10
        ra = list(ca)
        ma = m2.readme10_mutation(ra, changed)[1] if changed else ra
        removed = sorted(x for x in changed if m2.covered(x, ra) and not m2.covered(x, ma))
        p = removed[0] if len(removed) == 1 else None
    elif mode < 0.45:
        p = None
    elif mode < 0.75:
        p = rnd.choice(ca) if ca else rnd.choice(U)
    else:
        p = rnd.choice(U)
    if p is not None and rnd.random() < 0.4 and p not in changed:
        changed.append(p)
    fb = [rnd.choice(U) for _ in range(rnd.randrange(0, 5))]
    if p is not None and rnd.random() < 0.15:
        fb.append(p)
    cmp(p, ca, changed, fb, 'rand')
    n_rand += 1
    if trial >= 52000 and time.time() - t1 > 900:
        break
t_rand = time.time() - t1
# 4) elementos no cadena (JSON: int, null, bool, lista, objeto): se compara resultado o clase de excepcion, y se informa
nonstr = []
for d, p, ca, ch in [('int tras la ruta', 'a', ['a', 5], ['a']), ('int antes de la ruta', 'a', [5, 'a'], ['a']), ('null', 'a', ['a', None], ['a']),
                     ('bool', 'a', [True, 'a'], ['a']), ('lista', 'a', ['a', ['x']], ['a']), ('objeto', 'a', ['a', {'k': 1}], ['a']),
                     ('int y p None', None, ['a', 5], ['a']), ('alcance null', 'a', None, ['a']), ('alcance cadena', 'a', 'ab/', ['a'])]:
    r2, _ = run(f2, p, ca, ch, [])
    r3, _ = run(f3, p, ca, ch, [])
    nonstr.append({'Case': d, 'v2': r2, 'v3': r3, 'Same': r2 == r3})

res = {'Seed': SEED, 'Exhaustive': {'Count': n_exh, 'Seconds': round(t_exh, 1)}, 'Random': {'Count': n_rand, 'Seconds': round(t_rand, 1)},
       'Stats': stats, 'Mismatches': mismatches, 'Edges': edge_rows, 'NonString': nonstr,
       'ExtractedBlockV2': body2, 'ExtractedBlockV3': body3}
open(outp, 'w', encoding='utf-8', newline='\n').write(json.dumps(res, ensure_ascii=False, indent=1) + '\n')
print(json.dumps({k: res[k] for k in ('Seed', 'Exhaustive', 'Random')}, ensure_ascii=False))
print(json.dumps(stats, ensure_ascii=False))
print('mismatches:', len(mismatches))
for e in edge_rows:
    print('EDGE', e['Same'], e['Edge'], e['v2'], e['v2_excl'], e['v3'])
for e in nonstr:
    print('NONSTR', e['Same'], e['Case'], e['v2'], e['v3'])
