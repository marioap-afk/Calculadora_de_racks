"""Genera el borrador de la disposición única previa a la medición (decisiones §100, punto 3) desde disposition.template.md, con los marcadores
literales de disposition-markers.json del kit v2, el SHA-256 de su FREEZE.json, los blobs de DELEG y H de la designación, el RunId y las dos
líneas del Owner (line-od2.txt y line-b.txt, byte a byte). Uso: python -I -B make_disposition_draft.py <kit> <plantilla> <salida.md>
"""
import hashlib, json, os, sys

KIT, TPL, OUT = sys.argv[1:4]
m = json.load(open(os.path.join(KIT, 'disposition-markers.json'), encoding='utf-8'))
des = json.load(open(os.path.join(KIT, 'frozen', 'designation-M.json'), encoding='utf-8'))
blob = dict((a['Role'], a['Blob']) for a in des['Artifacts'])
fz = hashlib.sha256(open(os.path.join(KIT, 'FREEZE.json'), 'rb').read()).hexdigest()
line = lambda n: open(os.path.join(KIT, n), encoding='utf-8').read()
t = open(TPL, encoding='utf-8').read()
rep = {'{{MARKERS}}': '\n'.join('>     - ' + x for x in m['Markers']), '{{FREEZE}}': fz[:8] + '…', '{{FREEZE_FULL}}': fz,
       '{{DELEG}}': blob['DELEG'], '{{H}}': blob['H'], '{{RUN_ID}}': des['Invocation']['RunId'],
       '{{LINE_OD2}}': line('line-od2.txt'), '{{LINE_B}}': line('line-b.txt')}
for k, v in rep.items():
    assert t.count(k) >= 1, k
    t = t.replace(k, v)
assert '{{' not in t
open(OUT, 'w', encoding='utf-8', newline='\n').write(t)
print(OUT, hashlib.sha256(t.encode('utf-8')).hexdigest(), len(t.encode('utf-8')))
