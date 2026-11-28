"""PB-T4b, «Retirarlo»: las pruebas que dejan de compilar al retirar el código del piloto, por referencia de CÓDIGO (sin cadenas
ni comentarios) y transitiva: dentro de su archivo (métodos por `nombre(`, campos y propiedades por su nombre) y entre archivos
(un miembro afectado de un tipo de apoyo, nombrado como `Tipo.Miembro`, o como `.Miembro(` en un archivo que nombra el tipo).

Uso: python -I retire_compile_pbt4.py <checkout> <rev> <pbt4-substitution-lists.tsv> <salida.tsv>
Solo lee objetos git (`git ls-tree` y `git cat-file --batch`, vía lists_core.py y aquí). Escribe solo <salida.tsv>.
"""
import re
import subprocess
import sys
import importlib.util
from collections import Counter, defaultdict

spec = importlib.util.spec_from_file_location('lc', __file__.replace('retire_compile_pbt4.py', 'lists_core.py'))
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
repo, rev = sys.argv[1:3]
lc.REPO, lc.REV = repo, rev
lists_tsv, out_tsv = sys.argv[3:5]

RETIRED = re.compile(r'\bPushBackPilotStateComparer\b|\bPushBackPilotFieldAdapter\b|\bKeepPilotEditsOn\b|\bComposeForWrite\b|\bPushBackPilotUpdatePlanner\s*\.\s*Prepare\s*\(|\bPushBackPilotUpdateRun\s*\.\s*(?:Execute|Precheck)\s*\(')
SKIPPED = re.compile(r'\bSkip\s*=')
FIELD = re.compile(r'^[ \t]*(?:(?:private|public|internal|protected|static|readonly|const)\s+)+[\w<>\[\],.?]+\s+(\w+)\s*(?:=(?!>)|;|\{\s*get|=>)', re.M)
TOP = re.compile(r'^[ \t]*(?:(?:public|internal|private|static|sealed|abstract|file)\s+)*(?:partial\s+)?(?:class|record|struct)\s+(\w+)', re.M)
NINE = {'tests/RackCad.Tests/Workspace/%s.cs' % c for c in [
    'WorkspaceLifecycleHostTests', 'WorkspacePilotPrepareTests', 'WorkspaceEditIntentHostTests', 'WorkspacePilotEditHostTests',
    'WorkspaceCommandAdvisorTests', 'WorkspacePilotUpdateRequestTests', 'WorkspacePilotUpdateHostTests',
    'WorkspacePilotReconciliationHostTests', 'WorkspaceAssociativeBookkeepingTests']}

paths = lc.ls('tests')
batch = subprocess.run(['git', '-C', repo, 'cat-file', '--batch'], input=''.join('%s:%s\n' % (rev, p) for p in paths).encode('utf-8'), capture_output=True)
data, pos, texts = batch.stdout, 0, {}
for p in paths:
    nl = data.index(b'\n', pos)
    header = data[pos:nl].split()
    size = int(header[2])
    texts[p] = data[nl + 1:nl + 1 + size].decode('utf-8', 'replace').replace('\r\n', '\n')
    pos = nl + 1 + size + 1
masked = {p: lc.mask(lc.blank_comments(t)) for p, t in texts.items()}

model = {}


def model_of(p):
    if p not in model:
        code = masked[p]
        ms = lc.members(code)
        spans = [(m['first'], m['last']) for m in ms]
        fields = []
        for fm in FIELD.finditer(code):
            line = code.count('\n', 0, fm.start()) + 1
            if not any(a <= line <= b for a, b in spans):
                end = code.find(';', fm.end())
                fields.append({'name': fm.group(1), 'test': False, 'field': True, 'attrs': '', 'body': code[fm.start():end + 1], 'first': line, 'last': line})
        for m in ms:
            m['field'] = False
        model[p] = {'members': ms + fields, 'tops': [t.group(1) for t in TOP.finditer(code)]}
    return model[p]


affected = set()
for p in paths:
    if RETIRED.search(masked[p]):
        for m in model_of(p)['members']:
            if RETIRED.search(m['attrs'] + m['body']):
                affected.add((p, m['name'], m['field']))


def ref(name, is_field):
    return (r'\b' + re.escape(name) + r'\b') if is_field else (r'\b' + re.escape(name) + r'\s*[(<]')


changed = True
while changed:
    changed = False
    by_file = defaultdict(set)
    for p, n, f in affected:
        by_file[p].add((n, f))
    # dentro del archivo
    for p, names in list(by_file.items()):
        rx = re.compile('|'.join(ref(n, f) for n, f in names))
        for m in model_of(p)['members']:
            key = (p, m['name'], m['field'])
            if key not in affected and rx.search(m['attrs'] + m['body']):
                affected.add(key)
                changed = True
    # entre archivos: los tipos de apoyo (no las clases de prueba) con miembros afectados
    for p, names in list(by_file.items()):
        types = [t for t in model_of(p)['tops']]
        for t in types:
            trx = re.compile(r'\b' + re.escape(t) + r'\b')
            qual = re.compile('|'.join(r'\b' + re.escape(t) + r'\s*\.\s*' + re.escape(n) + r'\b' for n, f in names))
            loose = re.compile('|'.join(r'\.\s*' + re.escape(n) + r'\s*[(<]' for n, f in names if not f) or r'(?!x)x')
            bare = re.compile('|'.join(r'(?<![\w.])' + re.escape(n) + r'\s*[(<]' for n, f in names if not f) or r'(?!x)x')
            static_use = re.compile(r'using\s+static\s+[\w.]*\b' + re.escape(t) + r'\s*;')
            for q in paths:
                if q == p or not trx.search(masked[q]):
                    continue
                imported = static_use.search(masked[q]) is not None
                for m in model_of(q)['members']:
                    key = (q, m['name'], m['field'])
                    text = m['attrs'] + m['body']
                    if key not in affected and (qual.search(text) or loose.search(text) or (imported and bare.search(text))):
                        affected.add(key)
                        changed = True

listed = set()
for line in open(lists_tsv, encoding='utf-8').read().splitlines()[1:]:
    cols = line.split('\t')
    listed.add((cols[1], cols[2]))
rows = []
for p, n, f in affected:
    for m in model[p]['members']:
        if m['name'] == n and m['test'] and not SKIPPED.search(m['attrs']):
            where = 'las 9 clases' if p in NINE else ('ya en la lista' if (p, n) in listed else 'NUEVA')
            rows.append((p, n, '%d-%d' % (m['first'], m['last']), where))
rows = sorted(set(rows))
with open(out_tsv, 'w', encoding='utf-8', newline='\n') as f:
    f.write('archivo\tmetodo\tlineas\testado\n')
    for r in rows:
        f.write('\t'.join(r) + '\n')
print('pruebas afectadas por compilacion:', len(rows), dict(Counter(r[3] for r in rows)))
for p, c in sorted(Counter(r[0] for r in rows if r[3] == 'NUEVA').items()):
    print('  NUEVA', p, c)
helpers = sorted({(p.rsplit('/', 1)[-1], n) for p, n, f in affected
                  if not any(m['name'] == n and m['test'] for m in model[p]['members'])})
print('miembros de apoyo afectados (no pruebas):', len(helpers))
for h in helpers:
    print('   ', *h)
