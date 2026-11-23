"""PB-T4e (paquete v5.6): solo lee. Las secciones sin cambio de la v5.5 a la v5.6 que NO son de las 99 de PB-T4a a PB-T4d (las de la v5.4
sin cambio en la v5.5), con las funciones de make_v56.py.

Uso (desde v56/): python -I same_extra_v56.py <repo> <v5.6.md>
"""
import importlib.util
import os
import subprocess
import sys

sys.dont_write_bytecode = True
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('make_v56', os.path.join(HERE, 'make_v56.py'))
m = importlib.util.module_from_spec(spec)
spec.loader.exec_module(m)
repo, v56p = sys.argv[1:3]
v54 = subprocess.run(['git', '-C', repo, 'show', m.REV + ':' + m.PKG], capture_output=True, check=True).stdout.decode('utf-8')
v55 = m.read(os.path.join(m.PB4, 'I-64-f4b-pbt4-gate-contract-package-v55.md'))
v56 = m.read(v56p)
same54 = set(m.diff_same_keys(v54, v55)) & set(m.diff_same_keys(v54, v56))
same55 = set(m.diff_same_keys(v55, v56))
print('v5.4->v5.5->v5.6 sin cambio: %d; v5.5->v5.6 sin cambio (con el principio y la cabecera): %d' % (len(same54), len(same55)))
for k in sorted(same55 - same54, key=lambda k: v55.index(k[0])):
    print('  fuera de las 99: %r' % (k,))
print('de las 99, no en v5.5->v5.6: %r' % sorted(same54 - same55))
