"""Sonda de C-7: por archivo de pruebas que nombra algo retirado, las pruebas (sin Skip), las que lo nombran por texto,
y las menciones fuera de todo metodo (campos, propiedades, clases anidadas), con su linea.

Uso: python -I probe_retired.py <checkout> <rev>
"""
import re
import sys
import importlib.util

spec = importlib.util.spec_from_file_location('lc', __file__.replace('probe_retired.py', 'lists_core.py'))
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
lc.REPO, lc.REV = sys.argv[1:3]
RETIRED = re.compile(r'PushBackPilotStateComparer|PushBackPilotFieldAdapter|KeepPilotEditsOn|ComposeForWrite|PushBackPilotUpdatePlanner\.Prepare\s*\(|PushBackPilotUpdateRun\.(?:Execute|Precheck)\s*\(')
SKIPPED = re.compile(r'\bSkip\s*=')
for path in lc.ls('tests'):
    src = lc.blank_comments(lc.show(path))
    if not RETIRED.search(src):
        continue
    ms = lc.members(src)
    spans = [(m['first'], m['last']) for m in ms]
    tw = [(m, t) for m, t in lc.tests_with_text(path)]
    live = [(m, t) for m, t in tw if not SKIPPED.search(m['attrs'])]
    hit = [m for m, t in live if RETIRED.search(t)]
    outside = []
    for i, line in enumerate(src.split('\n'), 1):
        if RETIRED.search(line) and not any(a <= i <= b for a, b in spans):
            outside.append('%d:%s' % (i, line.strip()[:110]))
    print('%-62s pruebas=%3d (skip=%d) con-firma=%3d fuera-de-metodos=%d' % (path.rsplit('/', 1)[-1], len(live), len(tw) - len(live), len(hit), len(outside)))
    for o in outside:
        print('      ', o)
