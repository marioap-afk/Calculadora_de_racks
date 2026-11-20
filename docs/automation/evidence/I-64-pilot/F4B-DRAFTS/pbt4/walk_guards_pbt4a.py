"""PB-T4a (v4.2, I-25): el recorrido ENTERO, por diferencia, de las pruebas de las clases que leen los archivos que toca PB-T4a.

Uso: python -I walk_guards_pbt4a.py <checkout> <rev> <salida.tsv> <salida-resumen.txt>
Solo objetos git (`git show`, `git ls-tree`, `git cat-file --batch`). Escribe solo las dos salidas.

El método. Una prueba de texto que pasa en la base solo puede caer en el GREEN si alguna consulta que hace sobre el texto da
otro resultado. Para cada prueba de esas clases (sin `Skip`) se toman todos los literales de cadena de su texto y del de los
ayudantes de su archivo que alcanza (transitivo, por nombre), y cada uno se evalúa, como expresión regular y como subcadena,
sobre WHC, WEI, NH y RIA en la base y en el GREEN literal del paquete (green_pbt4a.py): la cuenta en todo el archivo y la cuenta
en cada miembro. Una prueba sin ninguna diferencia queda en verde por construcción; las que tienen alguna se listan con sus
diferencias y se leen una a una (el resumen). Los archivos nuevos se miran aparte: un literal que casa en ellos importa a las
pruebas que barren su carpeta.
"""
import re
import subprocess
import sys
import importlib.util
from collections import defaultdict

base_dir = __file__.replace('walk_guards_pbt4a.py', '')


def load(name):
    spec = importlib.util.spec_from_file_location(name, base_dir + name + '.py')
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


lc = load('lists_core')
gp = load('green_pbt4a')
repo, rev, out_tsv, out_txt = sys.argv[1:5]
lc.REPO, lc.REV = repo, rev
base, green = gp.green(repo, rev)
TOUCHED = [gp.WHC_P, gp.WEI_P, gp.NH_P, gp.RIA_P]
NEW = [gp.IFACE_P, gp.DRAFT_P]
SHORT = {gp.WHC_P: 'WHC', gp.WEI_P: 'WEI', gp.NH_P: 'NH', gp.RIA_P: 'RIA', gp.IFACE_P: 'IFACE', gp.DRAFT_P: 'DRAFT'}


def code_only(text):
    return re.sub(r'//[^\r\n]*', '', re.sub(r'/\*.*?\*/', '', text, flags=re.S))


views = {}
for p in TOUCHED:
    for side, src in (('base', base[p]), ('green', green[p])):
        c = code_only(src)
        per = defaultdict(str)
        for m in lc.members(c):
            per[m['name']] += '\n' + m['body']
        views[(p, side)] = (c, per)
new_views = {p: code_only(green[p]) for p in NEW}

MARKERS = ['WorkspaceHostCore.cs', 'WorkspaceEditIntent.cs', 'WorkspaceNavigationHost.cs', 'RackPilotIntentAdapter.cs', 'Application/Workspace',
           'ApplicationWorkspace', 'PluginWorkspace', 'Plugin/Workspace', 'Plugin/Views', 'PushBack/Draft', 'AllProduct()', 'PluginFiles()', 'ApplicationFiles()']
paths = lc.ls('tests')
batch = subprocess.run(['git', '-C', repo, 'cat-file', '--batch'], input=''.join('%s:%s\n' % (rev, p) for p in paths).encode('utf-8'), capture_output=True)
data, pos, texts = batch.stdout, 0, {}
for p in paths:
    nl = data.index(b'\n', pos)
    size = int(data[pos:nl].split()[2])
    texts[p] = data[nl + 1:nl + 1 + size].decode('utf-8', 'replace').replace('\r\n', '\n')
    pos = nl + 1 + size + 1
guards = [p for p in paths if any(m in texts[p] for m in MARKERS)]

LIT = re.compile(r'@\$"(?:[^"]|"")*"|\$@"(?:[^"]|"")*"|@"(?:[^"]|"")*"|\$"(?:\\.|[^"\\\n])*"|"(?:\\.|[^"\\\n])*"')
ESC = {'\\\\': '\\', '\\"': '"', '\\n': '\n', '\\r': '\r', '\\t': '\t', '\\0': '\0', "\\'": "'"}
FIELD = re.compile(r'^[ \t]*(?:(?:private|public|internal|protected|static|readonly|const)\s+)+[\w<>\[\],.?()]+\s+(\w+)\s*(?:=(?!>)|;|\{\s*get|=>)', re.M)
SKIPPED = re.compile(r'\bSkip\s*=')


def decode(lit):
    if lit.startswith(('@"', '@$"', '$@"')):
        inner = lit[lit.index('"') + 1:-1].replace('""', '"')
    else:
        inner = lit[lit.index('"') + 1:-1]
        inner = re.sub(r'\\[\\"nrt0\']', lambda m: ESC[m.group(0)], inner)
    if '$' in lit[:2]:
        inner = re.sub(r'\{[^{}]*\}', '', inner)
    return inner


def counts(lit, text, per):
    out = []
    try:
        rx = re.compile(lit)
        out.append(('re', len(rx.findall(text)), tuple(sorted((k, len(rx.findall(v))) for k, v in per.items()))))
    except re.error:
        pass
    out.append(('sub', text.count(lit), tuple(sorted((k, v.count(lit)) for k, v in per.items()))))
    return out


cache = {}


def diffs_of(lit):
    """[(archivo, texto)] de las diferencias del literal entre la base y el GREEN."""
    if lit in cache:
        return cache[lit]
    res = []
    for p in TOUCHED:
        b = counts(lit, *views[(p, 'base')])
        g = counts(lit, *views[(p, 'green')])
        for (kind, bc, bper), (_, gc, gper) in zip(b, g):
            bd, gd = dict(bper), dict(gper)
            changed = sorted(k for k in set(bd) | set(gd) if bd.get(k, 0) != gd.get(k, 0))
            if bc != gc or changed:
                res.append((p, '%s %s %d->%d [%s]' % (SHORT[p], kind, bc, gc, ','.join(changed[:4]))))
    for p in NEW:
        try:
            n = len(re.findall(lit, new_views[p]))
        except re.error:
            n = new_views[p].count(lit)
        if n:
            res.append((p, '%s nuevo %d' % (SHORT[p], n)))
    cache[lit] = res
    return res


# Qué archivo tocado lee una prueba: su ruta o su carpeta como literal, o un ayudante de carpeta de WorkspaceHostSource.
FOLDERS = {
    gp.WHC_P: ['src/RackCad.Application/Workspace', 'src/RackCad.Application', 'src/RackCad.Application/'],
    gp.WEI_P: ['src/RackCad.Application/Workspace', 'src/RackCad.Application', 'src/RackCad.Application/'],
    gp.IFACE_P: ['src/RackCad.Application/Workspace', 'src/RackCad.Application', 'src/RackCad.Application/'],
    gp.DRAFT_P: ['src/RackCad.Application/Systems/PushBack/Draft', 'src/RackCad.Application/Systems/PushBack', 'src/RackCad.Application/Systems',
                 'src/RackCad.Application', 'src/RackCad.Application/'],
    gp.NH_P: ['src/RackCad.Plugin/Workspace', 'src/RackCad.Plugin', 'src/RackCad.Plugin/'],
    gp.RIA_P: ['src/RackCad.Plugin/Views', 'src/RackCad.Plugin', 'src/RackCad.Plugin/'],
}
HELPERS = {gp.WHC_P: r'\bApplicationWorkspace\b|\bApplicationFiles\s*\(|\bAllProduct\s*\(', gp.WEI_P: r'\bApplicationWorkspace\b|\bApplicationFiles\s*\(|\bAllProduct\s*\(',
           gp.IFACE_P: r'\bApplicationWorkspace\b|\bApplicationFiles\s*\(|\bAllProduct\s*\(', gp.DRAFT_P: r'\bAllProduct\s*\(',
           gp.NH_P: r'\bPluginWorkspace\b|\bPluginFiles\s*\(|\bAllProduct\s*\(', gp.RIA_P: r'\bAllProduct\s*\('}


def reads(text, lits):
    out = set()
    for p in TOUCHED + NEW:
        name = p.rsplit('/', 1)[-1]
        folders = [f.rstrip('/') for f in FOLDERS[p]] + ['src']
        if any(name in l or p in l or l.rstrip('/').replace('\\', '/') in folders for l in lits) or re.search(HELPERS[p], text):
            out.add(p)
    return out


RELEVANT = re.compile(r'[A-Za-z_]{4,}')
CHANGED = {}
for p in TOUCHED:
    b, g = views[(p, 'base')][1], views[(p, 'green')][1]
    CHANGED[p] = sorted(k for k in set(b) | set(g) if b.get(k) != g.get(k))


def selects_changed(lits, files):
    """Un literal con forma de firma que casa con la cabecera de un miembro que cambia, en la base."""
    hits = []
    for p in files & set(TOUCHED):
        code = views[(p, 'base')][0]
        for name in CHANGED[p]:
            for mm in re.finditer(r'[^\n]*\b' + re.escape(name) + r'\b[^\n]*', code):
                head = mm.group(0)
                for l in lits:
                    if not RELEVANT.search(l) or name not in l and not re.search(r'\\s|\\b|\(', l):
                        continue
                    try:
                        if re.search(l, head) and name in head:
                            hits.append('%s:%s' % (SHORT[p], name))
                    except re.error:
                        pass
    return sorted(set(hits))


# Las constantes de cadena de todas las clases de prueba, para resolver una ruta nombrada desde otra clase
# (por ejemplo `WorkspaceBridgeSourceGuardTests.HostAdapterPath`).
CONST = re.compile(r'(?:const\s+string|static\s+readonly\s+string)\s+(\w+)\s*=\s*([^;]+);')
consts = defaultdict(list)
for p in paths:
    if not p.endswith('.cs'):
        continue
    for cm in CONST.finditer(lc.blank_comments(texts[p])):
        consts[cm.group(1)].extend(decode(l) for l in LIT.findall(cm.group(2)))
QUAL = re.compile(r'\b[A-Z]\w*\.([A-Z]\w*)\b')


def external_literals(text):
    out = []
    for qm in QUAL.finditer(text):
        out.extend(consts.get(qm.group(1), []))
    return out


rows, summary = [], defaultdict(lambda: [0, 0])
for g in guards:
    code = lc.blank_comments(texts[g])
    ms = lc.members(code)
    spans = [(m['first'], m['last']) for m in ms]
    helpers = {}
    for m in ms:
        if not m['test']:
            helpers.setdefault(m['name'], []).append(m['body'])
    for fm in FIELD.finditer(code):
        line = code.count('\n', 0, fm.start()) + 1
        if not any(a <= line <= b for a, b in spans):
            end = code.find(';', fm.end())
            helpers.setdefault(fm.group(1), []).append(code[fm.start():end + 1])
    hx = {h: re.compile(r'\b' + re.escape(h) + r'\b') for h in helpers}
    for m in ms:
        if not m['test'] or SKIPPED.search(m['attrs']):
            continue
        seen, todo, text = set(), [m['attrs'] + m['body']], ''
        while todo:
            t = todo.pop()
            text += '\n' + t
            for h, rx in hx.items():
                if h not in seen and h != m['name'] and rx.search(t):
                    seen.add(h)
                    todo.extend(helpers[h])
        lits = sorted({decode(l) for l in LIT.findall(text)} - {''})
        files = reads(text, lits + external_literals(text))
        found = []
        for l in lits:
            if not RELEVANT.search(l):
                continue
            d = [t for p, t in diffs_of(l) if p in files]
            if d:
                found.append((l, d))
        sel = selects_changed(lits, files)
        cat = 'A' if found else ('B' if sel else ('-' if files else 'no-lee'))
        summary[g][0] += 1
        if cat in ('A', 'B'):
            summary[g][1] += 1
        rows.append((g, m['name'], '%d-%d' % (m['first'], m['last']), cat, ','.join(sorted(SHORT[f] for f in files)) or '-', ','.join(sel) or '-',
                     ' || '.join('%r: %s' % (l[:70], '; '.join(d)) for l, d in found) or '-'))

with open(out_tsv, 'w', encoding='utf-8', newline='\n') as f:
    f.write('clase\tprueba\tlineas\tcategoria\tlee\tcuerpos-que-cambian\tdiferencias\n')
    for r in rows:
        f.write('\t'.join(r) + '\n')
from collections import Counter
cats = Counter(r[3] for r in rows)
lines = ['clases=%d pruebas=%d A=%d B=%d sin-diferencia=%d no-leen-archivos-tocados=%d' % (len(guards), len(rows), cats['A'], cats['B'], cats['-'], cats['no-lee']),
         'cambian: ' + '; '.join('%s: %s' % (SHORT[p], ', '.join(CHANGED[p])) for p in TOUCHED)]
for g in guards:
    lines.append('%-70s pruebas=%3d A-o-B=%3d' % (g.rsplit('/', 1)[-1], summary[g][0], summary[g][1]))
with open(out_txt, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('\n'.join(lines[:1]))
