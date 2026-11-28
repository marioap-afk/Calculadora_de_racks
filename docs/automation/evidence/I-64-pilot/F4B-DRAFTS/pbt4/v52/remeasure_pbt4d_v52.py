"""PB-T4d (paquete v5.2, fase 1): la remedicion de la lista de la retirada del piloto sobre el cierre de PB-T4c (6c2820ba, F4B-E55), por objetos
git (`git show`, `git ls-tree`, `git cat-file --batch`). Frente a la base de la lista de la v4.4 (pbt4d-retire-list-138.tsv, medida en 7f0d9aa3):

1. cada fila de la lista (138 C-7, 55 originales y 29 de apoyo): si su miembro sigue, en que lineas, si su texto cambio y si lleva Skip (de PB-T4b,
   de PB-T4c u otro);
2. la lectura transitiva de lo que deja de compilar al retirar lo de D-PBT4-10 (el metodo de retire_compile_pbt4.py, sin filtrar los Skip): en
   7f0d9aa3, como comprobacion, y en 6c2820ba, con lo nuevo de PB-T4a, PB-T4b y PB-T4c; `PushBackPilotUpdateRun.Precheck(` es ambiguo por texto
   (tiene una sobrecarga del borrador completo, que se queda) y se separa;
3. las referencias por texto a lo retirado, archivo a archivo, en las dos revisiones;
4. en src/: donde esta lo retirado, quien lo nombra fuera de Pilot/ y que mas queda sin uso al retirarlo;
5. los vacios de M-04 (PEG y su copia), el punto 1 de CT-PBT4B-20 y de su equivalente de PB-T4c, y la prueba lenta de OWNER-05.
La compilacion y la ejecucion las mide la fase 2, en una copia exportada. Escribe la tabla en <salida.tsv> y el resumen en stdout (UTF-8, LF).

Uso: python -I remeasure_pbt4d_v52.py <checkout> <salida.tsv>
"""
import collections
import hashlib
import importlib.util
import os
import re
import subprocess
import sys

sys.dont_write_bytecode = True
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
HERE = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.dirname(HERE)
spec = importlib.util.spec_from_file_location('lists_core', os.path.join(PB4, 'lists_core.py'))
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
REPO, OUT = sys.argv[1:3]
lc.REPO = REPO
OLD, NEW = '7f0d9aa3', '6c2820ba'
LIST = os.path.join(PB4, 'pbt4d-retire-list-138.tsv')
LIST_SHA = hashlib.sha256(open(LIST, 'rb').read()).hexdigest()
assert LIST_SHA == 'd5467dc162e2cda8c539a829cc8f3a7c46260a119f70e840b34e6f78dde0387c'
print('# PB-T4d, la remedicion sobre %s (%s), frente a %s; la lista %s' % (NEW, lc.git('rev-parse', NEW).strip(), OLD, LIST_SHA))

RETIRED_STRICT = re.compile(r'\bPushBackPilotStateComparer\b|\bPushBackPilotFieldAdapter\b|\bKeepPilotEditsOn\b|\bComposeForWrite\b'
                            r'|\bPushBackPilotUpdatePlanner\s*\.\s*Prepare\s*\(|\bPushBackPilotUpdateRun\s*\.\s*Execute\s*\(')
PRECHECK = re.compile(r'\bPushBackPilotUpdateRun\s*\.\s*Precheck\s*\(')
SKIP = re.compile(r'\bSkip\s*=\s*"([^"]*)"')
FIELD = re.compile(r'^[ \t]*(?:(?:private|public|internal|protected|static|readonly|const)\s+)+[\w<>\[\],.?]+\s+(\w+)\s*(?:=(?!>)|;|\{\s*get|=>)', re.M)
TOP = re.compile(r'^[ \t]*(?:(?:public|internal|private|static|sealed|abstract|file)\s+)*(?:partial\s+)?(?:class|record|struct)\s+(\w+)', re.M)


def tree(rev, prefix):
    return [p for p in lc.git('ls-tree', '-r', '--name-only', rev, prefix).splitlines() if p.endswith('.cs')]


def batch(rev, paths):
    out = subprocess.run(['git', '-C', REPO, 'cat-file', '--batch'], input=''.join('%s:%s\n' % (rev, p) for p in paths).encode('utf-8'),
                         capture_output=True, check=True).stdout
    pos, texts = 0, {}
    for p in paths:
        nl = out.index(b'\n', pos)
        size = int(out[pos:nl].split()[2])
        texts[p] = out[nl + 1:nl + 1 + size].decode('utf-8', 'replace').replace('\r\n', '\n')
        pos = nl + 1 + size + 1
    return texts


class Rev:
    def __init__(self, rev, prefix):
        self.rev = rev
        self.paths = tree(rev, prefix)
        self.texts = batch(rev, self.paths)
        self.masked = {p: lc.mask(lc.blank_comments(t)) for p, t in self.texts.items()}
        self._model = {}

    def model(self, p):
        if p not in self._model:
            code = self.masked[p]
            ms = lc.members(lc.blank_comments(self.texts[p]))
            spans = [(m['first'], m['last']) for m in ms]
            fields = []
            for fm in FIELD.finditer(code):
                line = code.count('\n', 0, fm.start()) + 1
                if not any(a <= line <= b for a, b in spans):
                    end = code.find(';', fm.end())
                    fields.append({'name': fm.group(1), 'test': False, 'field': True, 'attrs': '', 'body': code[fm.start():end + 1], 'first': line,
                                   'last': line})
            for m in ms:
                m['field'] = False
                m['mbody'] = lc.mask(m['attrs'] + m['body'])
            for f in fields:
                f['mbody'] = f['body']
            self._model[p] = {'members': ms + fields, 'tops': [t.group(1) for t in TOP.finditer(code)]}
        return self._model[p]


def affected_of(r, rx):
    """La lectura transitiva de retire_compile_pbt4.py: lo que nombra lo retirado (por codigo, sin cadenas ni comentarios) y, dentro del archivo y
    entre archivos, lo que nombra a esos miembros."""
    aff = {}
    for p in r.paths:
        if rx.search(r.masked[p]):
            for m in r.model(p)['members']:
                if rx.search(m['mbody']):
                    aff[(p, m['name'], m['field'])] = 'texto'

    def ref(name, is_field):
        return (r'\b' + re.escape(name) + r'\b') if is_field else (r'\b' + re.escape(name) + r'\s*[(<]')

    changed = True
    while changed:
        changed = False
        by_file = collections.defaultdict(set)
        for p, n, f in aff:
            by_file[p].add((n, f))
        for p, names in list(by_file.items()):
            rxf = re.compile('|'.join(ref(n, f) for n, f in names))
            for m in r.model(p)['members']:
                key = (p, m['name'], m['field'])
                if key not in aff and rxf.search(m['mbody']):
                    aff[key] = 'compilacion'
                    changed = True
        for p, names in list(by_file.items()):
            for t in r.model(p)['tops']:
                trx = re.compile(r'\b' + re.escape(t) + r'\b')
                qual = re.compile('|'.join(r'\b' + re.escape(t) + r'\s*\.\s*' + re.escape(n) + r'\b' for n, f in names))
                loose = re.compile('|'.join(r'\.\s*' + re.escape(n) + r'\s*[(<]' for n, f in names if not f) or r'(?!x)x')
                bare = re.compile('|'.join(r'(?<![\w.])' + re.escape(n) + r'\s*[(<]' for n, f in names if not f) or r'(?!x)x')
                static_use = re.compile(r'using\s+static\s+[\w.]*\b' + re.escape(t) + r'\s*;')
                for q in r.paths:
                    if q == p or not trx.search(r.masked[q]):
                        continue
                    imported = static_use.search(r.masked[q]) is not None
                    for m in r.model(q)['members']:
                        key = (q, m['name'], m['field'])
                        text = m['mbody']
                        if key not in aff and (qual.search(text) or loose.search(text) or (imported and bare.search(text))):
                            aff[key] = 'compilacion'
                            changed = True
    return aff


def skip_of(m):
    s = SKIP.search(m['attrs']) if m.get('attrs') else None
    if not s:
        return '-'
    reason = s.group(1)
    return 'Skip PB-T4c' if 'PB-T4c' in reason else ('Skip PB-T4b' if 'PB-T4b' in reason else 'Skip otro')


def norm(text):
    return re.sub(r'\s+', ' ', text).strip()


old = Rev(OLD, 'tests')
new = Rev(NEW, 'tests')
rows = [l.split('\t') for l in open(LIST, encoding='utf-8').read().rstrip('\n').split('\n')[1:]]
assert len(rows) == 222 and collections.Counter(r[0] for r in rows) == {'PB-T4d C-7': 138, 'PB-T4d original': 55, 'PB-T4d apoyo': 29}

# ------------------------------------------------------------------ 1. cada fila de la lista
out = ['lista\tarchivo\tmiembro\tlineas-7f0d9aa3\tlineas-6c2820ba\testado\ttexto\tskip-6c2820ba']
state = collections.Counter()
def resolve(path):
    """Las filas de apoyo traen solo el nombre del archivo: su ruta, unica, en 7f0d9aa3."""
    if '/' in path:
        return path
    hits = [p for p in old.paths if p.endswith('/' + path)]
    assert len(hits) == 1, (path, hits)
    return hits[0]


rows = [[r[0], resolve(r[1])] + r[2:] for r in rows]
for kind, path, member, lines, action in rows:
    a, b = (int(x) for x in lines.split('-')) if lines != '-' else (0, 10 ** 9)
    om = [m for m in old.model(path)['members'] if m['name'] == member] if path in old.texts else []
    nm = [m for m in new.model(path)['members'] if m['name'] == member] if path in new.texts else []
    o = next((m for m in om if m['first'] <= b and m['last'] >= a), om[0] if om else None)
    idx = om.index(o) if o in om else 0
    n = nm[idx] if idx < len(nm) else None
    if path not in new.texts:
        st, same, sk, nl = 'archivo-borrado', '-', '-', '-'
    elif n is None:
        st, same, sk, nl = 'miembro-borrado', '-', '-', '-'
    else:
        same = 'igual' if o is not None and norm(lc.mask(o['body'])) == norm(lc.mask(n['body'])) else 'cambia'
        sk = skip_of(n)
        nl = '%d-%d' % (n['first'], n['last'])
        st = 'sigue' if nl == lines else 'se-movio'
    state[(kind, st, same, sk)] += 1
    out.append('\t'.join([kind, path, member, lines, nl, st, same, sk]))
print('\n## 1. Las filas de la lista en %s' % NEW)
for k, c in sorted(state.items()):
    print('  %-16s %-16s %-7s %-12s %d' % (k + (c,)))

# ------------------------------------------------------------------ 2. lo que deja de compilar, por texto y transitivo
LISTED = {(r[1], r[2]): r[0] for r in rows}
print('\n## 2. Lo que deja de compilar al retirar (lectura transitiva por texto; la fase 2 lo mide compilando)')
for label, r in (('7f0d9aa3', old), ('6c2820ba', new)):
    strict = affected_of(r, RETIRED_STRICT)
    both = affected_of(r, re.compile(RETIRED_STRICT.pattern + '|' + PRECHECK.pattern))
    tests = {k: v for k, v in both.items() if any(m['name'] == k[1] and m['test'] for m in r.model(k[0])['members'])}
    helpers = {k: v for k, v in both.items() if k not in tests}
    only_precheck = {k for k in both if k not in strict}
    cls = collections.Counter()
    for (p, n, f), how in tests.items():
        m = next(m for m in r.model(p)['members'] if m['name'] == n and m['test'])
        where = LISTED.get((p, n), 'NUEVA')
        cls[(where, skip_of(m), 'solo-Precheck' if (p, n, f) in only_precheck else 'firme')] += 1
        if label == '6c2820ba':
            out.append('\t'.join(['afectada ' + where, p, n, '-', '%d-%d' % (m['first'], m['last']), how, 'solo-Precheck' if (p, n, f) in only_precheck
                                  else 'firme', skip_of(m)]))
    print('  %s: %d pruebas y %d miembros de apoyo' % (label, len(tests), len(helpers)))
    for k, c in sorted(cls.items()):
        print('    %-20s %-12s %-14s %d' % (k + (c,)))
    if label == '6c2820ba':
        newfiles = collections.Counter(p for (p, n, f) in tests if LISTED.get((p, n)) is None)
        for p, c in sorted(newfiles.items()):
            print('    NUEVA en %s: %d' % (p, c))
        hel_new = sorted((p.rsplit('/', 1)[-1], n) for (p, n, f) in helpers if (p, n) not in LISTED)
        print('    apoyo nuevo (no en la lista): %d %s' % (len(hel_new), hel_new))
        hel_gone = sorted((r_[1].rsplit('/', 1)[-1], r_[2]) for r_ in rows if r_[0] == 'PB-T4d apoyo' and not any(
            (p, n) == (r_[1], r_[2]) for (p, n, f) in helpers))
        print('    apoyo de la lista que ya no sale: %d %s' % (len(hel_gone), hel_gone))
        for (p, n, f) in sorted(only_precheck):
            print('    solo por Precheck( (ambiguo): %s %s' % (p.rsplit('/', 1)[-1], n))

# ------------------------------------------------------------------ 3. las referencias por texto, archivo a archivo
print('\n## 3. Las referencias por texto a lo retirado (codigo sin cadenas ni comentarios), por archivo de tests/')
ALLRX = re.compile(RETIRED_STRICT.pattern + '|' + PRECHECK.pattern)
files = sorted(set(old.paths) | set(new.paths))
for p in files:
    a = len(ALLRX.findall(old.masked[p])) if p in old.masked else None
    b = len(ALLRX.findall(new.masked[p])) if p in new.masked else None
    if a or b:
        print('  %-90s %s -> %s' % (p, '-' if a is None else a, '-' if b is None else b))

# ------------------------------------------------------------------ 4. src/
print('\n## 4. src/ en %s' % NEW)
src = Rev(NEW, 'src')
PILOT = 'src/RackCad.Application/Systems/PushBack/Pilot/'
for p in src.paths:
    hits = [(src.masked[p].count('\n', 0, m.start()) + 1, m.group(0)) for m in ALLRX.finditer(src.masked[p])]
    defs = [(src.masked[p].count('\n', 0, m.start()) + 1, m.group(0)) for m in re.finditer(
        r'class\s+(?:PushBackPilotStateComparer|PushBackPilotFieldAdapter)\b|\b(?:KeepPilotEditsOn|ComposeForWrite)\s*\(', src.masked[p])]
    if hits or defs:
        print('  %s%s: nombra %s' % ('' if p.startswith(PILOT) else 'FUERA DE Pilot/ ', p, [h for h in hits][:8]))
CANDIDATES = ['PushBackPilotPrepareInput', 'PushBackPilotComposition', 'ComparePost', 'NonPilotContentReason', 'InvalidScaleReason',
              'BaseChangedReason', 'ComposeFailurePrefix', 'PushBackPilotHostRun', 'PushBackPilotSystemRebuild', 'PushBackPilotIntentResult',
              'PushBackPilotMaterializer', 'PushBackPilotIntentReader', 'PushBackPilotDraftState', 'IPushBackPilotUpdatePort', 'IPushBackPilotViewEraser']
print('  Lo que queda con uso o sin el (referencias por codigo, fuera de su archivo; src/ y tests/):')
for name in CANDIDATES:
    rx = re.compile(r'\b' + re.escape(name) + r'\b')
    s_out = sum(len(rx.findall(t)) for p, t in src.masked.items() if not p.endswith('/' + name + '.cs'))
    s_out_pilot = sum(len(rx.findall(t)) for p, t in src.masked.items() if not p.startswith(PILOT))
    t_out = sum(len(rx.findall(t)) for t in new.masked.values())
    print('    %-30s src=%d (fuera de Pilot/: %d) tests=%d' % (name, s_out, s_out_pilot, t_out))

# ------------------------------------------------------------------ 5. M-04, CT-PBT4B-20 y OWNER-05
print('\n## 5. M-04, el punto 1 de CT-PBT4B-20 y OWNER-05')
PEG = 'tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs'
for name in ('TheReadGuardAllowsAnotherReadThatALaterTaskWouldAddWithItsOwnMark', 'TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing'):
    ms = [m for m in new.model(PEG)['members'] if m['name'] == name]
    print('  M-04 PEG %s: %s' % (name, ['%d-%d %s' % (m['first'], m['last'], skip_of(m)) for m in ms]))
copy = [p for p in new.paths if p.endswith('/WorkspacePilotEditGuardFull.cs')]
for p in copy:
    lines = new.texts[p].split('\n')
    print('  M-04 la copia %s:255-258:' % p)
    for i in range(254, 258):
        print('    %d| %s' % (i + 1, lines[i].rstrip()[:150]))
for p in [q for q in new.paths if q.endswith('/WorkspaceFullCopyGuardTests.cs') or q.endswith('/PushBackEditAllFullCopyGuardTests.cs')]:
    t = new.texts[p]
    originals = sorted(set(re.findall(r'"(tests/RackCad[^"]+\.cs)"', t)))
    nine = [o for o in originals if any(r[1] == o and r[0] == 'PB-T4d original' for r in rows)]
    print('  CT-PBT4B-20 en %s: %d rutas de tests/ nombradas; con originales que PB-T4d borra: %s' % (p, len(originals), nine))
for p in new.paths:
    for m in new.model(p)['members']:
        if m['name'].startswith('InvalidD_ABuilderThatCannotBuild'):
            print('  OWNER-05 %s %s %d-%d %s retirado=%s' % (p, m['name'][:60], m['first'], m['last'], skip_of(m), bool(ALLRX.search(m['mbody']))))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\ntabla', OUT, len(out) - 1, 'filas')
