# -*- coding: utf-8 -*-
"""Solo lectura: remide sobre el cierre de PB-T1c las anclas [T1c] de W y WC que cita el paquete de PB-T2 (todas las W:<n> y WC:<n>, con sus
continuaciones ", :<n>"). Lee por objetos git desde el checkout principal: W y WC de la base de las mediciones (95373af6, igual que 2f5d3dec y
833ea6bf) y de la revision nueva. Mapea cada linea vieja a la nueva con difflib sobre las lineas (los bloques iguales) y clasifica cada cita:
- igual: la misma linea y el mismo texto;
- movida: el mismo texto en otra linea (se corrige el numero);
- cambiada: un extremo cae en lineas que PB-T1c cambio o quito (se revisa a mano, con el texto viejo y el nuevo).
Con --final (v2.4, V24-07; CA-T2b M-04) el paquete es el final, con las anclas ya en la revision: lista cada cita de W, WC y WX del cuerpo (fuera
de los registros de cambios) con el texto de su primera y su ultima linea en la revision, y los blobs de W, WC y WX frente a los de 161a0dad.
Uso: python -I measure_pbt2b_t1c.py <checkout principal> <revision nueva> <paquete.md> [--map <salida.json>] [--final]. No escribe nada salvo --map."""
import difflib, hashlib, json, re, subprocess, sys

REPO, NEW, PKGF = sys.argv[1], sys.argv[2], sys.argv[3]
MAP_OUT = sys.argv[sys.argv.index('--map') + 1] if '--map' in sys.argv else None
FINAL = '--final' in sys.argv
GREEN_T1C = '161a0dadccaf5b71886344d834146737b90bb4f3'
OLD = '95373af6116cf120bd0a8ba490833524e33c47da'
FILES = {'W': 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs',
         'WC': 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs',
         'WX': 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml'}
sys.stdout.reconfigure(encoding='utf-8')


def show(rev, path):
    r = subprocess.run(['git', '-C', REPO, 'show', rev + ':' + path], capture_output=True)
    assert r.returncode == 0, (rev, path)
    return r.stdout.decode('utf-8').replace('\r\n', '\n').split('\n')


TEXT = {k: (show(OLD, p), show(NEW, p)) for k, p in FILES.items()}
MAP = {}
for k, (old, new) in TEXT.items():
    m = {}
    for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, old, new, autojunk=False).get_opcodes():
        if tag == 'equal':
            for d in range(i2 - i1):
                m[i1 + d + 1] = j1 + d + 1
    MAP[k] = m
same_wx = TEXT['WX'][0] == TEXT['WX'][1]
print('== base de las mediciones', OLD[:8], '-> revision', NEW, '| lineas: W %d -> %d, WC %d -> %d, WX igual: %s' % (
    len(TEXT['W'][0]), len(TEXT['W'][1]), len(TEXT['WC'][0]), len(TEXT['WC'][1]), same_wx))

pkg = open(PKGF, 'rb').read().decode('utf-8').replace('\r\n', '\n').split('\n')
# las secciones historicas (los registros de cambios) no se remiden: son las lineas de entonces
hist, cur = set(), False
for n, l in enumerate(pkg, 1):
    if l.startswith('## '):
        cur = l.startswith('## Registro de cambios')
    if cur:
        hist.add(n)
CITE = re.compile(r'\b(WC|WX|W):(\d+)(?:-(\d+))?((?:(?:,| y)\s*:\d+(?:-\d+)?)*)')
CONT = re.compile(r':(\d+)(?:-(\d+))?')
cites = []  # (linea del paquete, archivo, desde, hasta)
for n, l in enumerate(pkg, 1):
    for m in CITE.finditer(l):
        f = m.group(1)
        cites.append((n, f, int(m.group(2)), int(m.group(3) or m.group(2))))
        for c in CONT.finditer(m.group(4) or ''):
            cites.append((n, f, int(c.group(1)), int(c.group(2) or c.group(1))))
uniq = sorted({(f, a, b) for _, f, a, b in cites})
result = {}
if FINAL:
    def blob(rev, path):
        r = subprocess.run(['git', '-C', REPO, 'rev-parse', rev + ':' + path], capture_output=True)
        assert r.returncode == 0, (rev, path)
        return r.stdout.decode('ascii').strip()
    print('== paquete final, sha256 %s; revision %s' % (hashlib.sha256(open(PKGF, 'rb').read()).hexdigest(), NEW))
    for k, p in FILES.items():
        bn, bg = blob(NEW, p), blob(GREEN_T1C, p)
        print('   blob de %-2s en la revision %s, en 161a0dad %s: %s' % (k, bn[:8], bg[:8], 'igual' if bn == bg else 'DISTINTO'))
    body = sorted({(f, a, b) for n, f, a, b in cites if n not in hist})
    print('== citas del cuerpo (fuera de los registros de cambios): %d apariciones, %d distintas (W %d, WC %d, WX %d)' % (
        sum(1 for n, _, _, _ in cites if n not in hist), len(body), sum(1 for x in body if x[0] == 'W'), sum(1 for x in body if x[0] == 'WC'),
        sum(1 for x in body if x[0] == 'WX')))
    for f, a, b in body:
        new = TEXT[f][1]
        lines = sorted({n for n, ff, aa, bb in cites if (ff, aa, bb) == (f, a, b) and n not in hist})
        rng = ('%s:%d' % (f, a)) if a == b else ('%s:%d-%d' % (f, a, b))
        print('%-14s lineas del paquete %s' % (rng, ','.join(str(x) for x in lines)))
        print('         %d: %s' % (a, new[a - 1].strip()[:120]))
        if b != a:
            print('         %d: %s' % (b, new[b - 1].strip()[:120]))
    uniq = []
for f, a, b in uniq:
    old, new = TEXT[f]
    na, nb = MAP[f].get(a), MAP[f].get(b)
    if f == 'WX' and same_wx:
        status, na, nb = 'igual', a, b
    elif na is not None and nb is not None:
        status = 'igual' if (na, nb) == (a, b) else 'movida'
    else:
        status = 'cambiada'
    result['%s:%d-%d' % (f, a, b)] = {'file': f, 'old': [a, b], 'new': [na, nb], 'status': status,
                                      'old_text': [old[a - 1].strip(), old[b - 1].strip()],
                                      'new_text': [new[na - 1].strip() if na else None, new[nb - 1].strip() if nb else None],
                                      'pkg_lines': sorted({n for n, ff, aa, bb in cites if (ff, aa, bb) == (f, a, b)}),
                                      'only_history': all(n in hist for n, ff, aa, bb in cites if (ff, aa, bb) == (f, a, b))}
count = {}
for v in result.values():
    count[v['status']] = count.get(v['status'], 0) + 1
if not FINAL:
    print('== citas en el paquete:', len(cites), '| distintas:', len(uniq), '|', ', '.join('%s %d' % kv for kv in sorted(count.items())))
for key, v in result.items():
    rng = lambda p: ('%s:%d' % (v['file'], p[0])) if p[0] == p[1] else ('%s:%d-%d' % (v['file'], p[0], p[1]))
    newr = rng(v['new']) if v['new'][0] and v['new'][1] else 'sin mapa'
    print('%-8s %-14s -> %-14s %s lineas del paquete %s' % (v['status'], rng(v['old']), newr, '(solo registros)' if v['only_history'] else '',
                                                           ','.join(str(x) for x in v['pkg_lines'])))
    if v['status'] != 'igual':
        print('         viejo: %s | %s' % (v['old_text'][0][:110], v['old_text'][1][:110] if v['old'][1] != v['old'][0] else ''))
        print('         nuevo: %s | %s' % ((v['new_text'][0] or '-')[:110], (v['new_text'][1] or '-')[:110] if v['old'][1] != v['old'][0] else ''))
# INV-PBT2B-03: los rangos de BaseSha de los miembros que cambian, documentacion incluida (el metodo de INV-PBT1C-03: desde la primera linea de
# la documentacion o los atributos que lo preceden hasta la llave o el ';' que lo cierra)
W_NEW = TEXT['W'][1]


def code_of(raw):
    s = re.sub(r'"(?:\\.|[^"\\])*"', '""', raw)
    s = re.sub(r"'(?:\\.|[^'\\])'", "''", s)
    return s.split('//')[0]


def member_end(lines, d):
    depth, opened = 0, False
    for i in range(d, len(lines) + 1):
        code = code_of(lines[i - 1])
        depth += code.count('{') - code.count('}')
        opened = opened or '{' in code
        if (opened and depth == 0) or (not opened and code.strip().endswith(';') and depth == 0):
            return i
    raise AssertionError(('miembro sin final', d))


DECL = re.compile(r'^ {8}(?:\[[^\]]*\]\s*)*(?:(?:public|private|internal|protected|static|readonly|override|virtual|sealed|async|partial|new)\s+)+[\w<>\[\]\?,\.\s]*?\b(\w+)\s*\(')
CHANGING = ['LoadNew', 'LoadDesignForNew', 'LoadExisting', 'Update_Click', 'InsertBatch_Click', 'RequestDraw', 'PendingWork', 'Bom_Click',
            'SaveLibrary_Click', 'UpdateButtons']
print('== INV-PBT2B-03: los rangos de los miembros que cambian en W (%s), documentacion incluida' % NEW[:8])
RANGES = {}
for name in CHANGING:
    ks = [k for k, l in enumerate(W_NEW, 1) if (lambda m: m and m.group(1) == name)(DECL.match(l))]
    assert len(ks) == 1, (name, ks)
    k = ks[0]
    s = k
    while W_NEW[s - 2].strip().startswith(('///', '//', '[')):
        s -= 1
    e = member_end(W_NEW, k)
    RANGES[name] = (s, k, e)
    print('  %-18s W:%d-%d (declaracion en W:%d)' % (name, s, e, k))
rs = sorted(RANGES.values())
assert all(rs[i][2] < rs[i + 1][0] for i in range(len(rs) - 1)), 'rangos solapados'
if MAP_OUT:
    json.dump({'old': OLD, 'new': NEW, 'cites': result, 'map': {k: {str(a): b for a, b in m.items()} for k, m in MAP.items()}},
              open(MAP_OUT, 'w', encoding='utf-8'), ensure_ascii=True, indent=1)
