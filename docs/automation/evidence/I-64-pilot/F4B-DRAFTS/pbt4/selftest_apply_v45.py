"""PB-T4b (paquete v4.5): la autoprueba de apply_red_pbt4b_v45.py y apply_green_pbt4b_v45.py sobre una copia mínima, hecha aquí desde los objetos
git de la revisión (solo los archivos que tocan: el src/ de PB-T4b y los 20 archivos de los `Skip`). No es una copia exportada entera: no compila;
comprueba que los dos guiones escriben lo que dicen y en el orden de la precondición 6 (RED en modo de medida y, después, el GREEN).

Uso: python -I selftest_apply_v45.py <checkout> <rev> <dir-nueva> <salida.txt>
Escribe solo en <dir-nueva> (que no puede existir) y en <salida.txt>."""
import hashlib
import importlib.util
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
repo, rev, target, out_path = sys.argv[1:5]
assert not os.path.exists(target), target
sys.stdout.reconfigure(encoding='utf-8', errors='replace')


def load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


gb, sk = load('green_pbt4b_v45'), load('pbt4b_skips_v45')
base, green = gb.green(repo, rev)
files = dict(base)
for rel in sk.by_file():
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, rel)], capture_output=True)
    assert p.returncode == 0, rel
    files[rel] = p.stdout.decode('utf-8').lstrip('﻿').replace('\r\n', '\n')
for rel, text in files.items():
    dest = os.path.join(target, *rel.split('/'))
    os.makedirs(os.path.dirname(dest), exist_ok=True)
    with open(dest, 'wb') as f:
        f.write(text.encode('utf-8'))
out = ['copia minima: %d archivos' % len(files)]


def run(script, *args):
    p = subprocess.run([sys.executable, '-I', os.path.join(HERE, script), target, '--repo', repo, '--rev', rev] + list(args), capture_output=True)
    text = p.stdout.decode('utf-8', 'replace') + p.stderr.decode('utf-8', 'replace')
    return p.returncode, text


for script, args in (('apply_red_pbt4b_v45.py', ['--dry']), ('apply_red_pbt4b_v45.py', []), ('apply_green_pbt4b_v45.py', ['--dry']), ('apply_green_pbt4b_v45.py', [])):
    code, text = run(script, *args)
    last = [l for l in text.strip().split('\n') if l][-1]
    out.append('%s %s -> %d: %s' % (script, ' '.join(args), code, last))
    assert code == 0, text[-2000:]
skips = 0
for rel in sk.by_file():
    skips += len(re.findall(r'\[(?:Fact|Theory)\(Skip = "I-64 F4B PB-T4b: ', open(os.path.join(target, *rel.split('/')), encoding='utf-8').read()))
copies = [os.path.relpath(os.path.join(d, f), target).replace('\\', '/') for d, _, fs in os.walk(target) for f in fs if f.endswith('Full.cs')]
ria = open(os.path.join(target, *gb.RIA_P.split('/')), encoding='utf-8').read()
kit = open(os.path.join(target, 'tests', 'RackCad.Tests', 'Workspace', 'WorkspaceFullReadKit.cs'), 'rb').read()
out.append('Skip escritos: %d; copias: %d; kit: %s; RIA con la lectura diferida: %s' % (
    skips, len(copies), hashlib.sha256(kit).hexdigest(), 'new System.Lazy<PushBackFullIntentResult>(() => PushBackFullDraftReader.Read(input, catalog))' in ria))
assert skips == 477 and len(copies) == 20
with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\n'.join(out))
