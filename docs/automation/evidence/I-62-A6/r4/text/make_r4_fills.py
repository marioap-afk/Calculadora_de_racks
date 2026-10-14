# A-6 round 4: fills the @@...@@ placeholders of the draft (make_r4_text.py) with the values measured by r1probes/ (summary.json, p11-indep.json).
#   python -I make_r4_fills.py <r1probes dir>
import hashlib, json, os, re, sys
NL = chr(10)
D = os.path.dirname(os.path.abspath(__file__))
P = sys.argv[1]
summ = json.load(open(os.path.join(P, 'summary.json'), encoding='ascii'))
ind = json.load(open(os.path.join(P, 'p11-indep.json'), encoding='ascii'))['Cases'][0]
assert summ['Holding'] == summ['Total'], 'not every case holds'
groups = {}
for c in summ['Cases']:
    groups.setdefault(c['Group'], 0)
    groups[c['Group']] += 1
acc = groups.get('V', 0) + groups.get('N4', 0) + groups.get('nc1', 0)
rows = ind['Rows']
bench = sum(1 for r in rows if not r['Vector'].startswith('D:/r62-a6r4/indep/'))
agent = len(rows) - bench
assert ind['Holds'] and ind['Observed']['Agree'] == len(rows)
fills = {
    '@@R4.CASES@@': '%d de %d casos conformes (%d de derivación, %d de cotejo con insumos sustituidos, añadidos o inventados, %d con el aceptante fijado y %d de comparación con la implementación independiente)'
                    % (summ['Holding'], summ['Total'], groups['derive'], groups['neg'], acc, groups['indep']),
    '@@R4.INDEP@@': 'los %d vectores (%d del banco y %d del agente que la escribió: su muestra y sus vectores sintéticos)' % (len(rows), bench, agent),
}
src = os.path.join(D, 'I-62-A-6.r4.draft.md')
t = open(src, encoding='utf-8').read()
for k, v in fills.items():
    assert t.count(k) == 1, k
    t = t.replace(k, v)
assert not re.search(r'@@[A-Z0-9_. ]+@@', t)
out = os.path.join(D, 'I-62-A-6.r4.md')
open(out, 'w', encoding='utf-8', newline=NL).write(t)
b = t.encode('utf-8')
print('ok', hashlib.sha1(b'blob %d\x00' % len(b) + b).hexdigest(), json.dumps(fills, ensure_ascii=True))
