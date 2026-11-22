"""PB-T4d (paquete v5.3): aplica una pieza GREEN por objetos git de 6c2820ba (green_pbt4d_v52.green) y escribe los textos de src/ que cambia en
<dir>, con un indice de las lineas de comentario que nombran el piloto (para CA-T4d M-05). Solo lee el repo. UTF-8 con LF.

Uso: python -I dump_green_v53.py <repo> <pieza> <dir> <indice.txt>
"""
import importlib.util
import os
import re
import sys

sys.dont_write_bytecode = True
REPO, PIECE, OUT, IDX = sys.argv[1:5]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
PB4 = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
spec = importlib.util.spec_from_file_location('green_pbt4d_v52', os.path.join(PB4, 'green_pbt4d_v52.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
base, green, deleted = gp.green(REPO, REV, PIECE)
lines = ['# pieza %s; archivos %d; borrados %d' % (os.path.basename(PIECE), len(green), len(deleted))]
for path in sorted(green):
    target = os.path.join(OUT, path)
    os.makedirs(os.path.dirname(target), exist_ok=True)
    with open(target, 'w', encoding='utf-8', newline='\n') as f:
        f.write(green[path])
    for n, l in enumerate(green[path].split('\n'), 1):
        s = l.strip()
        if (s.startswith('//') or s.startswith('*')) and re.search(r'\bpilot\b', s, re.I):
            lines.append('%s:%d  %s' % (os.path.basename(path), n, s[:200]))
with open(IDX, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print(lines[0], len(lines) - 1)
