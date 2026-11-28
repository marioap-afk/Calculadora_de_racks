"""PB-T4c (v5.0): el paseo de guardas sobre el GREEN literal de PB-T4c (green_pbt4c_v50.py), con el metodo de walk_guards_pbt4b_v48.py (que
no se toca) y su FIELD corregida de la v4.8 (las tuplas y los genericos con espacios). La base es el cierre de PB-T4b (6adff90d): sus pruebas
son las de la revision, asi que entran las de PB-T4b (sus 28 archivos nuevos, 24 de ellos nombran archivos que PB-T4c cambia), y ademas los
archivos locales que se le pasan (los tres equivalentes de la lista de 3, por su ruta virtual).

Para cada prueba sin `Skip` de una clase que nombra un archivo tocado o su carpeta, los literales de cadena de su texto y de los ayudantes de su
archivo que alcanza se evaluan, como expresion regular y como subcadena, sobre los archivos tocados en la base y en el GREEN: la cuenta en todo el
archivo y en cada miembro; y los nuevos, en el GREEN. Categorias: A (un literal cambia de cuenta en un archivo que la prueba lee), B (el cuerpo de
un miembro que cambia casa con un literal suyo), '-' (lee y nada cambia), no-lee. Solo objetos git y los archivos locales (leidos, nunca escritos).

Uso: python -I walk_guards_pbt4c_v50.py <checkout> <rev> <lista.tsv> <salida.tsv> <salida-resumen.txt> [<archivo-local>=<ruta-virtual> ...]
"""
import importlib.util
import re
import subprocess
import sys
from collections import defaultdict, Counter

base_dir = __file__.replace('walk_guards_pbt4c_v50.py', '')
sys.stdout.reconfigure(encoding='utf-8', newline='\n')


def load(name):
    spec = importlib.util.spec_from_file_location(name, base_dir + name + '.py')
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


lc = load('lists_core')
gp = load('green_pbt4c_v50')
repo, rev, list_file, out_tsv, out_txt = sys.argv[1:6]
extra = [a.split('=', 1) for a in sys.argv[6:]]
lc.REPO, lc.REV = repo, rev
base, green = gp.green(repo, rev)
TOUCHED = list(gp.EXISTING)
NEW = list(gp.NEW)
WHC_P = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
PORT_P = 'src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs'
SUP_P = 'src/RackCad.Application/Workspace/WorkspacePilotSupport.cs'
WEI_P = 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs'
WCO_P = 'src/RackCad.Plugin/Workspace/WorkspaceController.cs'
FVM_P = 'src/RackCad.UI/Workspace/WorkspacePilotFieldViewModel.cs'
PV_P = 'src/RackCad.UI/Workspace/WorkspacePanelView.xaml'
PVC_P = 'src/RackCad.UI/Workspace/WorkspacePanelView.xaml.cs'
W_P = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
HOSTI_P = 'src/RackCad.Application/Workspace/IWorkspaceFullEditHost.cs'
LAUNCH_P = 'src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftLaunch.cs'
SPORT_P = 'src/RackCad.Application/Systems/PushBack/Draft/PushBackFullDraftStatePort.cs'
OPENER_P = 'src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs'
assert sorted(TOUCHED) == sorted([WHC_P, PORT_P, SUP_P, WEI_P, WCO_P, FVM_P, PV_P, PVC_P, W_P]), TOUCHED
assert sorted(NEW) == sorted([HOSTI_P, LAUNCH_P, SPORT_P, OPENER_P]), NEW
SHORT = {WHC_P: 'WHC', PORT_P: 'PORT', SUP_P: 'SUP', WEI_P: 'WEI', WCO_P: 'WCo', FVM_P: 'FVM', PV_P: 'PV', PVC_P: 'PVC', W_P: 'W',
         HOSTI_P: 'HOSTI', LAUNCH_P: 'LAUNCH', SPORT_P: 'SPORT', OPENER_P: 'OPENER'}
LISTED = set()
for line in open(list_file, encoding='utf-8').read().split('\n')[1:]:
    if line:
        c = line.split('\t')
        LISTED.add((c[1], c[2]))


def code_only(text):
    return re.sub(r'//[^\r\n]*', '', re.sub(r'/\*.*?\*/', '', text, flags=re.S))


views = {}
for p in TOUCHED:
    for side, src in (('base', base[p]), ('green', green[p])):
        c = code_only(src) if p.endswith('.cs') else src
        per = defaultdict(str)
        if p.endswith('.cs'):
            for m in lc.members(c):
                per[m['name']] += '\n' + m['body']
        views[(p, side)] = (c, per)
new_views = {p: code_only(green[p]) for p in NEW}

MARKERS = ['WorkspaceHostCore', 'IWorkspacePilotEditPort', 'WorkspacePilotSupport', 'WorkspaceEditIntent', 'WorkspaceController', 'WorkspacePilotFieldViewModel',
           'WorkspacePanelView', 'RackPushBackSystemWindow', 'Application/Workspace', 'UI/Workspace', 'Plugin/Workspace', 'Plugin/Views', 'PushBack/Draft',
           'Systems/PushBack', 'UI/Systems', 'ApplicationWorkspace', 'AllProduct()', 'ApplicationFiles()', 'UiFiles()', 'UiFolder', 'PluginFiles()',
           'PluginWorkspace', 'src/RackCad.Plugin', 'src/RackCad.UI', 'src/RackCad.Application', '"src"']
paths = lc.ls('tests')
batch = subprocess.run(['git', '-C', repo, 'cat-file', '--batch'], input=''.join('%s:%s\n' % (rev, p) for p in paths).encode('utf-8'), capture_output=True)
data, pos, texts = batch.stdout, 0, {}
for p in paths:
    nl = data.index(b'\n', pos)
    size = int(data[pos:nl].split()[2])
    texts[p] = data[nl + 1:nl + 1 + size].decode('utf-8', 'replace').replace('\r\n', '\n')
    pos = nl + 1 + size + 1
for local, virtual in extra:
    assert virtual not in texts, 'la ruta virtual ya existe en la revision: ' + virtual
    texts[virtual] = open(local, encoding='utf-8').read().lstrip('﻿').replace('\r\n', '\n')
    paths.append(virtual)
guards = [p for p in paths if p.endswith('.cs') and any(m in texts[p] for m in MARKERS)]

LIT = re.compile(r'@\$"(?:[^"]|"")*"|\$@"(?:[^"]|"")*"|@"(?:[^"]|"")*"|\$"(?:\\.|[^"\\\n])*"|"(?:\\.|[^"\\\n])*"')
ESC = {'\\\\': '\\', '\\"': '"', '\\n': '\n', '\\r': '\r', '\\t': '\t', '\\0': '\0', "\\'": "'"}
FIELD_TYPE = r'(?:\((?:[^()\n]|\([^()\n]*\))*\)|<(?:[^<>\n]|<[^<>\n]*>)*>|[\w\[\],.?])+'
FIELD = re.compile(r'^[ \t]*(?:(?:private|public|internal|protected|static|readonly|const)\s+)+' + FIELD_TYPE + r'\s+(\w+)\s*(?:=(?!>)|;|\{\s*get|=>)', re.M)
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


APP_WS = ['src/RackCad.Application/Workspace', 'src/RackCad.Application', 'src/RackCad.Application/']
UI_WS = ['src/RackCad.UI/Workspace', 'src/RackCad.UI', 'src/RackCad.UI/']
UI_PB = ['src/RackCad.UI/Systems/PushBack', 'src/RackCad.UI/Systems', 'src/RackCad.UI', 'src/RackCad.UI/']
DRAFT_F = ['src/RackCad.Application/Systems/PushBack/Draft', 'src/RackCad.Application/Systems/PushBack', 'src/RackCad.Application/Systems',
           'src/RackCad.Application', 'src/RackCad.Application/']
PLUGIN_WS = ['src/RackCad.Plugin/Workspace', 'src/RackCad.Plugin', 'src/RackCad.Plugin/']
PLUGIN_VIEWS = ['src/RackCad.Plugin/Views', 'src/RackCad.Plugin', 'src/RackCad.Plugin/']
FOLDERS = {WHC_P: APP_WS, PORT_P: APP_WS, SUP_P: APP_WS, WEI_P: APP_WS, HOSTI_P: APP_WS, FVM_P: UI_WS, PV_P: UI_WS, PVC_P: UI_WS, W_P: UI_PB,
           WCO_P: PLUGIN_WS, OPENER_P: PLUGIN_VIEWS, LAUNCH_P: DRAFT_F, SPORT_P: DRAFT_F}
AH = r'\bApplicationWorkspace\b|\bApplicationFiles\s*\(|\bAllProduct\s*\('
UH = r'\bUiFiles\s*\(|\bUiFolder\b|\bAllProduct\s*\('
PH = r'\bAllProduct\s*\(|\bPluginFiles\s*\(|\bPluginWorkspace\b'
HELPERS = {WHC_P: AH, PORT_P: AH, SUP_P: AH, WEI_P: AH, HOSTI_P: AH, FVM_P: UH, PV_P: UH, PVC_P: UH, W_P: r'\bAllProduct\s*\(',
           WCO_P: PH, OPENER_P: PH, LAUNCH_P: r'\bAllProduct\s*\(', SPORT_P: r'\bAllProduct\s*\('}


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


rows, summary = [], defaultdict(lambda: [0, 0, 0])
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
        listed = 'lista' if (g, m['name']) in LISTED else '-'
        summary[g][0] += 1
        if cat in ('A', 'B'):
            summary[g][1] += 1
            if listed == '-':
                summary[g][2] += 1
        rows.append((g, m['name'], '%d-%d' % (m['first'], m['last']), cat, listed, ','.join(sorted(SHORT[f] for f in files)) or '-', ','.join(sel) or '-',
                     ' || '.join('%r: %s' % (l[:70], '; '.join(d)) for l, d in found) or '-'))

with open(out_tsv, 'w', encoding='utf-8', newline='\n') as f:
    f.write('clase\tprueba\tlineas\tcategoria\tlista\tlee\tcuerpos-que-cambian\tdiferencias\n')
    for r in rows:
        f.write('\t'.join(r) + '\n')
cats = Counter(r[3] for r in rows)
flag_out = sum(1 for r in rows if r[3] in ('A', 'B') and r[4] == '-')
lines = ['clases=%d pruebas=%d A=%d B=%d sin-diferencia=%d no-leen=%d | A-o-B fuera de la lista=%d | archivos locales=%d' % (
    len(guards), len(rows), cats['A'], cats['B'], cats['-'], cats['no-lee'], flag_out, len(extra)),
    'cambian: ' + '; '.join('%s: %s' % (SHORT[p], ', '.join(CHANGED[p])) for p in TOUCHED)]
for g in guards:
    if summary[g][1]:
        lines.append('%-70s pruebas=%3d A-o-B=%3d fuera-de-la-lista=%3d' % (g.rsplit('/', 1)[-1], summary[g][0], summary[g][1], summary[g][2]))
with open(out_txt, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('\n'.join(lines))
