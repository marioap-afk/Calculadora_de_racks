"""PB-T4b (paquete v4.5): la autoprueba de los guiones de la v4.5 y las medidas que el paquete cita, por objetos git de una revisión (el cierre de
PB-T4a, 78a7cef3). Solo lee objetos git y los archivos de hs65; escribe solo su salida.

Uso: python -I selftest_pbt4b_v45.py <checkout> <rev> <salida.txt>

1. el GREEN de la v4.5 (diez archivos) y el esqueleto; WHC no nombra ninguna lectura del piloto (`pilot.Read`, con sus variantes), así que la copia
   de EIG con las dos lecturas (M-01) sigue en verde;
2. los 477 `Skip` sobre los 20 archivos de la revisión, por archivo, en Core y en UI;
3. el `BaseSha` de los 20 archivos de origen (sha256 del texto con los saltos de línea normalizados a LF), para CT-PBT4B-20;
4. las 20 copias en el modo de medida (su conjunto) y el sha256 del kit;
5. M-00: los tipos de primer nivel de cada archivo de origen que la copia renombra (T -> T + Full), para CT-PBT4B-20;
6. la regla de la aserción de la partición (partition_pbt4b_v45.py selftest).
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
gb, sk, mc, pt = load('green_pbt4b_v45'), load('pbt4b_skips_v45'), load('make_copies_pbt4b_v45'), load('partition_pbt4b_v45')
out = []

base, green = gb.green(repo, rev)
_, red = gb.skeleton(repo, rev)
assert len(green) == 10 and gb.RIA_P in green and gb.RIA_P not in red
whc = green[gb.WHC_P]
pilot_reads = re.findall(r'\bpilot\.Read\b|\bpilot\s*[?!]?\s*\.\s*Read\b', re.sub(r'//[^\n]*', '', whc))
out.append('1. GREEN v4.5: %d archivos (%s); esqueleto: %d (RIA no cambia en el RED); WHC nombra la lectura del piloto %d veces (M-01: 0)' % (
    len(green), ', '.join(sorted(os.path.basename(p) for p in green)), len(red), len(pilot_reads)))
assert not pilot_reads


def show(path):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
    assert p.returncode == 0, path
    return p.stdout.decode('utf-8').lstrip('﻿').replace('\r\n', '\n')


core = ui = 0
base_sha = []
for path, reasons in sorted(sk.by_file().items()):
    text = show(path)
    _, done = sk.apply(text, reasons)
    if path.startswith('tests/RackCad.UI.Tests/'):
        ui += len(done)
    else:
        core += len(done)
    base_sha.append('%s  %s  skip=%d' % (hashlib.sha256(text.encode('utf-8')).hexdigest(), path, len(done)))
out.append('2. Skip: %d archivos, %d (Core %d, UI %d)' % (len(base_sha), core + ui, core, ui))
assert (core, ui, len(base_sha)) == (450, 27, 20)
out.append('3. BaseSha (LF) en %s:' % rev)
out.extend('   ' + l for l in base_sha)

files, review = mc.build(repo, rev, None)
lines = ['%s  %s' % (hashlib.sha256(files[r].encode('utf-8')).hexdigest(), r) for r in sorted(files)]
kit = open(os.path.join(HERE, 'pbt4b-kit-WorkspaceFullReadKit-v45.cs'), 'rb').read()
assert b'\r' not in kit
out.append('4. copias en el modo de medida: %d archivos, conjunto %s; revision: %d pruebas; kit v4.5 sha256 %s' % (
    len(files), hashlib.sha256(('\n'.join(lines) + '\n').encode('utf-8')).hexdigest(), len(review), hashlib.sha256(kit).hexdigest()))
assert len(files) == 20 and len(review) == 81

out.append('5. M-00, los tipos de primer nivel que la copia renombra (T -> T + Full), por archivo de origen:')
m0 = mc.mc
sources = sorted(set(list(m0.NINE) + m0.GUARDS + m0.UI))
for path in sources:
    text = show(path)
    prefix, units, suffix, nskind, ns, masked = m0.parse(text)
    base_name = os.path.basename(path)[:-3]
    names = []
    for s, e in units:
        tm = m0.TYPE_DECL.search(masked[s:e])
        name = tm.group(1) if tm else None
        if name and name != base_name and not (path in m0.NINE and name in m0.KIT):
            names.append(name)
    out.append('   %s: %s' % (base_name, ', '.join(names) if names else '(ninguno)'))

import io
import contextlib
buf = io.StringIO()
with contextlib.redirect_stdout(buf):
    pt.selftest()
out.append('6. ' + buf.getvalue().strip())

with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\n'.join(out))
