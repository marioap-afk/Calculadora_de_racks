"""Imprime el texto de pruebas de v44/base con el mapa M aplicado (copy_map_defs.py), para escribir sus equivalentes.
Uso: python -I show_mapped.py <Clase> <rango|metodo> [...]   (rango como 790-830; --nomap para no aplicar M)"""
import importlib.util, os, sys
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
HERE = os.path.dirname(os.path.abspath(__file__))
P4 = os.path.dirname(HERE)
spec = importlib.util.spec_from_file_location('cm', os.path.join(P4, 'copy_map_defs.py'))
cm = importlib.util.module_from_spec(spec)
spec.loader.exec_module(cm)
args = sys.argv[1:]
nomap = '--nomap' in args
args = [a for a in args if a != '--nomap']
cls = args[0]
roots = [os.path.join(HERE, 'base', 'tests', 'RackCad.Tests', 'Workspace'), os.path.join(HERE, 'base', 'tests', 'RackCad.Tests'),
         os.path.join(HERE, 'base', 'tests', 'RackCad.UI.Tests', 'Workspace')]
path = next(os.path.join(r, cls + '.cs') for r in roots if os.path.exists(os.path.join(r, cls + '.cs')))
lines = open(path, encoding='utf-8').read().split('\n')
for spec_ in args[1:]:
    a, b = (int(x) for x in spec_.split('-'))
    body = '\n'.join('%5d %s' % (a + i, l) for i, l in enumerate(lines[a - 1:b]))
    print(body if nomap else cm.apply_map(body)[0])
    print('-' * 40)
