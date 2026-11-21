"""Vuelca, para leer, la base de PB-T4b: los archivos de 5aaacce3 con el GREEN literal de PB-T4a aplicado en memoria (green_pbt4a.py).
Solo lee objetos git del checkout principal; escribe en v44/base/ (dentro de hs65/pbt4). Uso: python -I dump_base.py"""
import importlib.util, os, subprocess, sys
HERE = os.path.dirname(os.path.abspath(__file__))
P4 = os.path.dirname(HERE)
REPO = r'D:' + chr(92) + 'Documentos' + chr(92) + 'Codex' + chr(92) + 'Calculadora de racks'
REV = '5aaacce3'
OUT = os.path.join(HERE, 'base')
spec = importlib.util.spec_from_file_location('gp', os.path.join(P4, 'green_pbt4a.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
base, green = gp.green(REPO, REV)
r = subprocess.run(['git', '-C', REPO, 'ls-tree', '-r', '--name-only', REV, '--', 'src', 'tests'], capture_output=True)
files = r.stdout.decode('utf-8').split('\n')
want = [f for f in files if f.endswith(('.cs', '.xaml')) and (
    '/Workspace/' in f or '/Systems/PushBack/' in f or '/Controls/Numeric' in f or 'Formatting/' in f or
    f.startswith('tests/RackCad.Tests/PushBack') or f.startswith('tests/RackCad.UI.Tests/Numeric') or 'RackPilotIntentAdapter' in f)]
n = 0
for f in want:
    if f in green:
        text = green[f]
    else:
        text = subprocess.run(['git', '-C', REPO, 'show', REV + ':' + f], capture_output=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')
    p = os.path.join(OUT, *f.split('/'))
    os.makedirs(os.path.dirname(p), exist_ok=True)
    with open(p, 'w', encoding='utf-8', newline='\n') as h:
        h.write(text)
    n += 1
for f in green:
    if f not in base:
        p = os.path.join(OUT, *f.split('/'))
        os.makedirs(os.path.dirname(p), exist_ok=True)
        with open(p, 'w', encoding='utf-8', newline='\n') as h:
            h.write(green[f])
        n += 1
print('volcados', n)
