"""PB-T4d (v5.2), relevo: escribe los textos de src/ que da una pieza del GREEN (green_pbt4d_v52.green) en <dir>, con su nombre corto. Solo lee el repo.
Uso: python -I dump_green.py <repo> <pieza> <dir>
"""
import importlib.util
import os
import sys

sys.dont_write_bytecode = True
REPO, PIECE, OUT = sys.argv[1:4]
PB4 = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))
spec = importlib.util.spec_from_file_location('green_pbt4d_v52', os.path.join(PB4, 'green_pbt4d_v52.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
base, green, deleted = gp.green(REPO, '6c2820bab4d8b41e3e229e23f3a1638ccbb04735', PIECE)
os.makedirs(OUT, exist_ok=True)
for path, text in green.items():
    with open(os.path.join(OUT, 'green-' + os.path.basename(path)), 'w', encoding='utf-8', newline='\n') as f:
        f.write(text)
print(sorted(green), sorted(deleted))
