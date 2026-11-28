"""PB-T4b (paquete v4.6): la autoprueba de los guiones de la v4.6, por objetos git de una revisión (el cierre de PB-T4a). Solo lee; escribe solo su
salida.

Uso: python -I selftest_pbt4b_v46.py <checkout> <rev> <salida.txt>
1. el mapa v3: 360 mecánicas y 68 a mano; los 477 `Skip` (pbt4b_skips_v46.py), con 68 motivos «a mano» en C-0;
2. las 20 copias de make_copies_pbt4b_v46.py en el modo de medida: su conjunto, la revisión (68 a mano, 21 de guarda y el ayudante editado), MU
   (qué copias ganan la directiva de Draft/) y la edición del ayudante Env.OpenUndecidable, solo en la copia;
3. que ninguna copia de C-0 abra un borrador con una lectura del piloto (`PilotRead!.Read` o `IntentKit.Read(...).Read` en un `Begin`).
"""
import hashlib
import importlib.util
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


repo, rev, out_path = sys.argv[1:4]
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
sk, mc = load('pbt4b_skips_v46'), load('make_copies_pbt4b_v46')
out = []
reasons = [r for f in sk.by_file().values() for r in f.values()]
hand = sum(1 for r in reasons if 'equivalente escrito a mano' in r)
out.append('1. Skip: %d; con el motivo a mano: %d' % (len(reasons), hand))
assert len(reasons) == 477 and hand == 68
files, review = mc.build(repo, rev, None)
lines = ['%s  %s' % (hashlib.sha256(files[r].encode('utf-8')).hexdigest(), r) for r in sorted(files)]
with_using = sorted(r for r, t in files.items() if mc.USING_DRAFT in t)
out.append('2. copias: %d, conjunto %s; revision: %d entradas (%d a mano, %d de guarda, %d ayudantes)' % (
    len(files), hashlib.sha256(('\n'.join(lines) + '\n').encode('utf-8')).hexdigest(), len(review),
    sum(1 for (p, n) in review if '/Workspace/' in p and p.rsplit('/', 1)[-1][:-3] in mc.mc.cm.NINE and not n.startswith('ayudante ')),
    sum(1 for (p, n) in review if p.rsplit('/', 1)[-1][:-3] not in mc.mc.cm.NINE), sum(1 for (p, n) in review if n.startswith('ayudante '))))
out.append('   con la directiva de Draft/ (MU): %d: %s' % (len(with_using), ', '.join(os.path.basename(r) for r in with_using)))
wpeh = files['tests/RackCad.Tests/Workspace/WorkspacePilotEditHostFull.cs']
orig = subprocess.run(['git', '-C', repo, 'show', '%s:tests/RackCad.Tests/Workspace/WorkspacePilotEditHostTests.cs' % rev], capture_output=True).stdout.decode('utf-8')
out.append('   Env.OpenUndecidable: en la copia, lectura completa %d y del piloto %d; en el original (sin tocar), del piloto %d' % (
    wpeh.count('FullEditIntentKit.Admitted(rack).FullRead!.Read'), wpeh.count('Admitted(rack).PilotRead!.Read'), orig.count('EditIntentKit.Admitted(rack).PilotRead!.Read')))
bad = []
for r, t in files.items():
    if os.path.basename(r)[:-len('Full.cs')] + 'Tests' not in mc.mc.cm.NINE:
        continue
    for m in re.finditer(r'\.Begin\s*\([^;]*?(PilotRead!\.Read|IntentKit\.Read\([^;]*?\)\.Read\b|\bread\.Read\b)[^;]*;', t):
        bad.append('%s: %s' % (os.path.basename(r), m.group(0)[:140]))
out.append('3. Begin con una lectura del piloto en las copias de C-0: %d %s' % (len(bad), bad[:6]))
with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\n'.join(out))
