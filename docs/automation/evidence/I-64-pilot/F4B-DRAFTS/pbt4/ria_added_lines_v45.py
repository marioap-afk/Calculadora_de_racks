"""PB-T4b (v4.5): qué literales de las pruebas que leen el Plugin (o todo src/) casan con una línea AÑADIDA o CAMBIADA de RIA por la optimización
(green_pbt4b_v45.py), como la ampliación de CA-T4a (cat4a_added_lines.py) para el hueco del recorrido: los barridos con Path.Combine o de todo
src/. Solo lee objetos git y archivos locales; escribe solo su salida.

Uso: python -I ria_added_lines_v45.py <checkout> <rev> <salida.txt> [<archivo-local>=<ruta-virtual> ...]
Una prueba entra si su archivo nombra el Plugin o todo src/ ("RackCad.Plugin", "Plugin", "\"src\"", "src/" o AllProduct). Cada literal (con al menos
cuatro letras) se prueba como expresión regular y como subcadena sobre cada línea añadida, sin comentarios.
"""
import difflib
import importlib.util
import os
import re
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('gb45', os.path.join(HERE, 'green_pbt4b_v45.py'))
gb45 = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gb45)
repo, rev, out = sys.argv[1:4]
extra = [a.split('=', 1) for a in sys.argv[4:]]


def code_only(text):
    return re.sub(r'//[^\r\n]*', '', re.sub(r'/\*.*?\*/', '', text, flags=re.S))


red = gb45.t4a_plugin(repo, rev)[gb45.RIA_P]
green = gb45.ria_green(red)
added = [l[2:] for l in difflib.ndiff(code_only(red).split('\n'), code_only(green).split('\n')) if l.startswith('+ ') and l[2:].strip()]
LIT = re.compile(r'@\$"(?:[^"]|"")*"|\$@"(?:[^"]|"")*"|@"(?:[^"]|"")*"|\$"(?:\\.|[^"\\\n])*"|"(?:\\.|[^"\\\n])*"')
ESC = {'\\\\': '\\', '\\"': '"', '\\n': '\n', '\\r': '\r', '\\t': '\t', '\\0': '\0', "\\'": "'"}


def decode(lit):
    if lit.startswith(('@"', '@$"', '$@"')):
        inner = lit[lit.index('"') + 1:-1].replace('""', '"')
    else:
        inner = lit[lit.index('"') + 1:-1]
        inner = re.sub(r'\\[\\"nrt0\']', lambda m: ESC[m.group(0)], inner)
    if '$' in lit[:2]:
        inner = re.sub(r'\{[^{}]*\}', '', inner)
    return inner


ls = subprocess.run(['git', '-C', repo, 'ls-tree', '-r', '--name-only', rev, 'tests'], capture_output=True).stdout.decode('utf-8').split('\n')
paths = [p for p in ls if p.endswith('.cs')]
batch = subprocess.run(['git', '-C', repo, 'cat-file', '--batch'], input=''.join('%s:%s\n' % (rev, p) for p in paths).encode('utf-8'), capture_output=True)
data, pos, texts = batch.stdout, 0, {}
for p in paths:
    nl = data.index(b'\n', pos)
    size = int(data[pos:nl].split()[2])
    texts[p] = data[nl + 1:nl + 1 + size].decode('utf-8', 'replace').replace('\r\n', '\n')
    pos = nl + 1 + size + 1
for local, virtual in extra:
    texts[virtual] = open(local, encoding='utf-8').read().lstrip('﻿').replace('\r\n', '\n')
    paths.append(virtual)
SWEEP = re.compile(r'RackCad\.Plugin|"Plugin"|Plugin/|"src"|"src/"|AllProduct\s*\(|PluginFiles\s*\(')
L = ['lineas anadidas o cambiadas de RIA: %d' % len(added)] + ['  + ' + a.strip() for a in added]
n_files = 0
for p in paths:
    if not SWEEP.search(texts[p]):
        continue
    n_files += 1
    hits = []
    for lit in sorted({decode(l) for l in LIT.findall(texts[p])} - {''}):
        if len(re.findall(r'[A-Za-z]', lit)) < 4:
            continue
        try:
            rx = re.compile(lit)
        except re.error:
            rx = None
        for line in added:
            try:
                ok = (rx is not None and rx.search(line) and rx.search(line).group(0) != '') or lit in line
            except re.error:
                ok = lit in line
            if ok:
                hits.append('%r ~ %s' % (lit[:90], line.strip()[:110]))
                break
    if hits:
        L.append('== %s (%d)' % (p, len(hits)))
        L.extend('   ' + h for h in hits)
L.insert(1 + len(added), 'archivos que leen el Plugin o todo src/: %d' % n_files)
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(L) + '\n')
print('\n'.join(L))
