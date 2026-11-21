"""PB-T4b (v4.8): la lista de 478 (owner-pbt4b-list-478-decision.md, la sexta tanda del Owner): las 477 de pbt4b-substitution-list-477.tsv, sin
cambio y en su orden, y al final la guarda de los textos del anfitrión, en C-5, medida en el cierre de PB-T4a (78a7cef3). La de 477 no se toca.

Uso: python -I make_list_478.py <checkout> <rev>
Solo `git show`. Escribe pbt4b-substitution-list-478.tsv e imprime su sha256 y sus recuentos."""
import hashlib
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
repo, rev = sys.argv[1:3]
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
OLD = os.path.join(HERE, 'pbt4b-substitution-list-477.tsv')
raw = open(OLD, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '5975c5b6b9111c3a4048193b5ebf10b619a567996dea8707b88ff6a00105a651'
text = raw.decode('utf-8')
assert text.endswith('\n') and '\r' not in text
DECISION = 'owner-pbt4b-list-478-decision.md'
NEW = [
    ('tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs', 'NoneOfThe30LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept',
     'C-5: WHHG, la guarda de los 30 textos del anfitrion cuenta tres que el GREEN cambia (2 -> 0, 5 -> 6 y 1 -> 0; D-PBT4-04 y D-PBT4-08; %s)' % DECISION),
]


def show(path):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
    assert p.returncode == 0, path
    return p.stdout.decode('utf-8').lstrip('﻿').replace('\r\n', '\n')


rows = []
for path, name, cause in NEW:
    lines = show(path).split('\n')
    hits = [i for i, l in enumerate(lines) if re.search(r'\bpublic\s+void\s+' + name + r'\s*\(', l)]
    assert len(hits) == 1, (path, name, hits)
    i = hits[0]
    start = i
    while start > 0 and lines[start - 1].strip().startswith('['):
        start -= 1
    depth, end, opened = 0, None, False
    for j in range(i, len(lines)):
        depth += lines[j].count('{') - lines[j].count('}')
        opened = opened or '{' in lines[j]
        if opened and depth == 0:
            end = j
            break
    rows.append('PB-T4b C-5\t%s\t%s\t%d-%d\t%s\n' % (path, name, start + 1, end + 1, cause))
out = text + ''.join(rows)
data = out.encode('utf-8')
with open(os.path.join(HERE, 'pbt4b-substitution-list-478.tsv'), 'wb') as f:
    f.write(data)
body = [l.split('\t') for l in out.split('\n')[1:] if l]
counts = {}
for r in body:
    counts[r[0]] = counts.get(r[0], 0) + 1
assert len(body) == 478 and counts == {'PB-T4b C-0': 428, 'PB-T4b C-5': 23, 'PB-T4b UI': 27}, counts
core = sum(1 for r in body if not r[1].startswith('tests/RackCad.UI.Tests/'))
files = len({r[1].replace('(PB-T4a) ', '') for r in body})
print('pbt4b-substitution-list-478.tsv sha256 %s %d bytes; filas %d %s; Core %d, UI %d; archivos %d' % (
    hashlib.sha256(data).hexdigest(), len(data), len(body), counts, core, len(body) - core, files))
for r in rows:
    print('nueva: ' + r.rstrip('\n'))
