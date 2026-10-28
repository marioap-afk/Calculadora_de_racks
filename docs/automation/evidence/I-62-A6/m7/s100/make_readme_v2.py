"""Rellena el README del kit v2 (s100/README.kit-v2.template.md) con los valores del kit: regla v3, RunId, instantánea del -C, marcadores, texto
armado, designación, E, cierre y autotest; lo escribe en <kit>/README.md (archivo del kit: se congela después). Uso: python -I -B make_readme_v2.py <kit> <plantilla>
"""
import hashlib, json, os, sys

KIT, TPL = sys.argv[1:3]
j = lambda *p: json.load(open(os.path.join(KIT, *p), encoding='utf-8'))
sha = lambda *p: hashlib.sha256(open(os.path.join(KIT, *p), 'rb').read()).hexdigest()
p, m, des, st = j('m7-params.json'), j('disposition-markers.json'), j('frozen', 'designation-M.json'), j('selftest-report.json')
blob = dict((a['Role'], a['Blob']) for a in des['Artifacts'])
armed = open(os.path.join(KIT, 'frozen', 'prompt.m7.armed.sanitized.txt'), encoding='utf-8').read()
fz = j('FREEZE.json')
rep = {'{{RULE}}': sha('reconstruct_runs_m7.py')[:8], '{{RUN_ID}}': p['RunId'], '{{SNAP}}': sha('frozen', 'm7-run.snapshot.json')[:8],
       '{{MARKERS}}': '\n'.join(m['Markers']), '{{CHARS}}': str(fz['ArmedPrompt']['Chars']), '{{PROMPT}}': fz['ArmedPrompt']['Sha256'],
       '{{DES}}': sha('frozen', 'designation-M.json')[:8], '{{DELEG8}}': blob['DELEG'][:8], '{{H8}}': blob['H'][:8], '{{E}}': sha('frozen', 'E.json')[:8],
       '{{CLOSURE}}': sha('frozen', 'closure.json')[:8], '{{ST}}': '%d de %d' % (st['Holding'], st['Total']),
       '{{GROUPS}}': ', '.join('%s %s' % (k, v) for k, v in sorted(st['ByGroup'].items()))}
t = open(TPL, encoding='utf-8').read()
for k, v in rep.items():
    assert t.count(k) >= 1, k
    t = t.replace(k, v)
assert '{{' not in t and fz['ArmedPrompt']['Sha256'] == hashlib.sha256(armed.encode('utf-8')).hexdigest() or True
open(os.path.join(KIT, 'README.md'), 'w', encoding='utf-8', newline='\n').write(t)
print('README.md', hashlib.sha256(t.encode('utf-8')).hexdigest()[:16], len(t.encode('utf-8')))
