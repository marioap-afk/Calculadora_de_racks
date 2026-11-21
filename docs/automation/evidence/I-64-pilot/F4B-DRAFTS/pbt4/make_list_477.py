"""PB-T4b (v4.5): la lista de 477 (owner-pbt4b-list-477-decision.md): las 475 de pbt4b-substitution-list-475.tsv, sin cambio y en su orden, y al
final CT-PBT4A-03 y CT-PBT4A-05, medidas en el cierre de PB-T4a (78a7cef3). La de 475 no se toca.

Uso: python -I make_list_477.py <checkout> <rev>
Solo `git show`. Escribe pbt4b-substitution-list-477.tsv e imprime su sha256 y sus recuentos."""
import hashlib
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
repo, rev = sys.argv[1:3]
OLD = os.path.join(HERE, 'pbt4b-substitution-list-475.tsv')
raw = open(OLD, 'rb').read()
assert hashlib.sha256(raw).hexdigest() == '654c3b7b045c66c3d7bf0f1c3c1eac42721182aa85ee4c006de444314fd0707c'
text = raw.decode('utf-8')
assert text.endswith('\n') and '\r' not in text
DECISION = 'owner-pbt4b-list-477-decision.md'
NEW = [
    ('tests/RackCad.Tests/PushBackFullReadPathTests.cs', 'CT_PBT4A_03_',
     'C-5: CT-PBT4A-03 (su ayudante PushBackFullReadRig busca el borrador del piloto; %s)' % DECISION),
    ('tests/RackCad.Tests/PushBackFullReadGuardTests.cs', 'CT_PBT4A_05_',
     'C-5: CT-PBT4A-05 (su lector EditIntentFakeReader responde FromPilot, sin la lectura completa; %s)' % DECISION),
]


def show(path):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
    assert p.returncode == 0, path
    return p.stdout.decode('utf-8').lstrip('﻿').replace('\r\n', '\n')


rows = []
for path, prefix, cause in NEW:
    lines = show(path).split('\n')
    hits = [i for i, l in enumerate(lines) if re.search(r'\bpublic\s+void\s+' + prefix + r'\w*\s*\(', l)]
    assert len(hits) == 1, (path, prefix, hits)
    i = hits[0]
    name = re.search(r'\b(' + prefix + r'\w*)\s*\(', lines[i]).group(1)
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
with open(os.path.join(HERE, 'pbt4b-substitution-list-477.tsv'), 'wb') as f:
    f.write(data)
body = [l.split('\t') for l in out.split('\n')[1:] if l]
counts = {}
for r in body:
    counts[r[0]] = counts.get(r[0], 0) + 1
assert len(body) == 477 and counts == {'PB-T4b C-0': 428, 'PB-T4b C-5': 22, 'PB-T4b UI': 27}, counts
assert out.startswith(text)
print('pbt4b-substitution-list-477.tsv sha256 %s %d bytes; filas %d %s' % (hashlib.sha256(data).hexdigest(), len(data), len(body), counts))
for r in rows:
    print('  + ' + r.rstrip('\n'))
