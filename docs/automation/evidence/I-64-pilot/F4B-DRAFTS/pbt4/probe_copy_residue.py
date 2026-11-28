"""Sonda: en cada una de las 9 clases, lo que queda sin cubrir por el mapa M FUERA de los cuerpos de las pruebas
(ayudantes, campos, tipos de primer nivel), con su linea. Un residuo ahi impide compilar la copia entera del archivo.

Uso: python -I probe_copy_residue.py <checkout> <rev>
"""
import re
import sys
import importlib.util

base = __file__.replace('probe_copy_residue.py', '')
spec = importlib.util.spec_from_file_location('lc', base + 'lists_core.py')
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
lc.REPO, lc.REV = sys.argv[1:3]
spec2 = importlib.util.spec_from_file_location('cm_defs', base + 'copy_map_defs.py')
cm = importlib.util.module_from_spec(spec2)
spec2.loader.exec_module(cm)

TW = 'tests/RackCad.Tests/Workspace/'
KIT_TYPES = {'EditIntentFakeReader', 'EditIntentKit', 'PrepareKit'}
TOP = re.compile(r'^(?:(?:public|internal|static|sealed|abstract|file)\s+)*(?:partial\s+)?(?:class|record|struct|interface|enum)\s+(\w+)', re.M)
for cls in cm.NINE:
    path = TW + cls + '.cs'
    code = lc.blank_comments(lc.show(path))
    masked = lc.mask(code)
    # quita los tipos del kit (no se copian: los sustituye el kit nuevo)
    spans = []
    for m in TOP.finditer(masked):
        if m.group(1) in KIT_TYPES:
            i = masked.index('{', m.end())
            d, j = 1, i + 1
            while d:
                d += {'{': 1, '}': -1}.get(masked[j], 0)
                j += 1
            spans.append((m.start(), j))
    keep = list(code)
    for a, b in spans:
        for k in range(a, b):
            if keep[k] != '\n':
                keep[k] = ' '
    copy_text = ''.join(keep)
    tests = [m for m in lc.members(copy_text) if m['test']]
    lines = copy_text.split('\n')
    in_test = set()
    for m in tests:
        in_test.update(range(m['first'], m['last'] + 1))
    out = []
    for i, line in enumerate(lines, 1):
        if i in in_test:
            continue
        mapped, _ = cm.apply_map(line)
        why = [k for k, rx in cm.RESIDUAL if rx.search(mapped)]
        if why:
            out.append('%5d [%s] %s' % (i, ','.join(why), line.strip()[:150]))
    print('== %s: residuos fuera de las pruebas = %d' % (cls, len(out)))
    for o in out:
        print('   ' + o)
