"""PB-T4b, precondición 6 (paquete v4.6): la comprobación mecánica de los `Skip` de la lista de 477 sobre una COPIA EXPORTADA, por su texto. Cada
uno de los 20 archivos de la copia tiene que ser, con los saltos de línea normalizados a LF, el de la base con el `Skip` literal de cada prueba de
la lista (pbt4b_skips_v46.py, con el mapa de la copia v3) y nada más.

Uso: python -I check_skips_copy_v46.py <checkout> <base> <dir-copia> <salida.txt>
Solo lee (git show y la copia); escribe solo la salida."""
import importlib.util
import os
import subprocess
import sys

PB4 = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', 'pbt4'))
spec = importlib.util.spec_from_file_location('sk', os.path.join(PB4, 'pbt4b_skips_v46.py'))
sk = importlib.util.module_from_spec(spec)
spec.loader.exec_module(sk)
repo, base, copy_dir, out = sys.argv[1:5]
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
lines, bad, total = [], 0, 0
for path, reasons in sorted(sk.by_file().items()):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (base, path)], capture_output=True)
    before = p.stdout.decode('utf-8').lstrip('﻿').replace('\r\n', '\n')
    expected, done = sk.apply(before, reasons)
    have = open(os.path.join(copy_dir, *path.split('/')), 'rb').read().decode('utf-8').lstrip('﻿').replace('\r\n', '\n')
    ok = have == expected
    bad += 0 if ok else 1
    total += len(done)
    lines.append('%s %s skip=%d' % ('OK  ' if ok else 'MAL ', path, len(done)))
lines.append('archivos: %d; mal: %d; Skip leidos: %d' % (len(lines), bad, total))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print('\n'.join(lines))
if bad or total != 477:
    raise SystemExit(1)
