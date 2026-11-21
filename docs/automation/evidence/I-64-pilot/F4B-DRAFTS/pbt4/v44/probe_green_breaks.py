"""PB-T4b: las copias mecanicas (375 de pbt4-copy-map.tsv) cuyo texto, ya mapeado, afirma algo que el GREEN de PB-T4b cambia por diseno.
Lee los originales de v44/base (5aaacce3 + el GREEN de PB-T4a, volcado por dump_base.py) con las lineas del mapa; aplica M (copy_map_defs.py).
Solo lee; imprime por firma las pruebas que la tocan, con su linea. Uso: python -I probe_green_breaks.py"""
import os, re, sys, importlib.util
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
P4 = os.path.dirname(HERE)
spec = importlib.util.spec_from_file_location('cm', os.path.join(P4, 'copy_map_defs.py'))
cm = importlib.util.module_from_spec(spec)
spec.loader.exec_module(cm)
BASE = os.path.join(HERE, 'base', 'tests', 'RackCad.Tests', 'Workspace')
SIGS = [
    ('G-keep', re.compile(r'\bTryKeepPilotEdit\s*\(')),
    ('G-enum', re.compile(r'GetNames<EditIntentStatus>|GetValues<EditIntentStatus>|typeof\(EditIntentStatus\)')),
    ('G-ab', re.compile(r'pb-20|pb-2[2-9]|"[^"]*-ab"|Composite|SideB')),
    ('G-sin-escala', re.compile(r'no tiene escala de anotaci')),
    ('G-campo', re.compile(r'UnknownFieldMessage|Campo desconocido')),
    ('G-annot-null', re.compile(r'Annotations\s*=\s*null|Annotations\s*==\s*null')),
    ('G-presented', re.compile(r'\bPresented\b')),
]
rows = [l.rstrip('\n').split('\t') for l in open(os.path.join(P4, 'pbt4-copy-map.tsv'), encoding='utf-8')][1:]
texts = {}
hits = {}
for cls, meth, lines, maps, dest in rows:
    if dest != 'mecanica':
        continue
    if cls not in texts:
        texts[cls] = open(os.path.join(BASE, cls + '.cs'), encoding='utf-8').read().split('\n')
    a, b = (int(x) for x in lines.split('-'))
    body = '\n'.join(texts[cls][a - 1:b])
    mapped, _ = cm.apply_map(body)
    for name, rx in SIGS:
        for i, line in enumerate(mapped.split('\n')):
            if rx.search(line):
                hits.setdefault(name, []).append((cls, meth, a + i, line.strip()[:150]))
for name, _ in SIGS:
    found = hits.get(name, [])
    print('==', name, len(found), 'lineas en', len({(c, m) for c, m, _, _ in found}), 'pruebas')
    for c, m, n, l in found:
        print('  %s.%s:%d  %s' % (c, m, n, l))
