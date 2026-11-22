"""PB-T4d (paquete v5.3), CA-T4d M-03: los miembros de tests/ (cualquier accesibilidad, salvo las pruebas [Fact]/[Theory]) que se usan en la
base y que el arbol final del RED deja sin ninguna referencia fuera de su declaracion. Por texto, sin comentarios (las cadenas cuentan: una guarda
puede nombrar un miembro). El texto de cada archivo: el del arbol del RED si esta, y si no el de 6c2820ba (por objetos git). Cuenta por nombre en
todo tests/, asi que un nombre comun no sale (falso negativo, nunca falso positivo); lo completa IDE0051/IDE0052 (ide_dead_v53.py). Solo lee.

Uso: python -I dead_text_v53.py <repo> <arbol-red> <salida.txt> [nombre ...]
"""
import collections
import importlib.util
import os
import re
import subprocess
import sys

sys.dont_write_bytecode = True
REPO, TREE, OUT = sys.argv[1:4]
ONLY = set(sys.argv[4:])
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
PB4 = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
spec = importlib.util.spec_from_file_location('lists_core', os.path.join(PB4, 'lists_core.py'))
lc = importlib.util.module_from_spec(spec)
spec.loader.exec_module(lc)
FIELD = re.compile(r'^[ \t]*(?:(?:private|public|internal|protected|static|readonly|const)\s+)+[\w<>\[\],.?() \t]+?\s+(\w+)\s*(?:=(?!>)|;|\{\s*get|=>)', re.M)
TYPE = re.compile(r'^[ \t]*(?:(?:private|public|internal|protected|static|sealed|abstract|file)\s+)*(?:class|struct|record)\s+(\w+)', re.M)


def git(*args):
    return subprocess.run(['git', '-C', REPO] + list(args), capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


paths = [p for p in git('ls-tree', '-r', '--name-only', REV, 'tests/').split('\n') if p.endswith('.cs')]
for root, _, names in os.walk(os.path.join(TREE, 'tests')):
    for n in names:
        if n.endswith('.cs'):
            rel = os.path.relpath(os.path.join(root, n), TREE).replace('\\', '/')
            if rel not in paths:
                paths.append(rel)
base_text, red_text = {}, {}
_bp = os.path.join(TREE, '_borrados.txt')
DELETED = set(open(_bp, encoding='utf-8').read().split()) if os.path.exists(_bp) else set()
for p in paths:
    try:
        base_text[p] = git('show', REV + ':' + p)
    except subprocess.CalledProcessError:
        base_text[p] = None
    t = os.path.join(TREE, p)
    red_text[p] = None if p in DELETED else (open(t, encoding='utf-8').read() if os.path.exists(t) else base_text[p])


def declared(code):
    masked = lc.mask(code)
    names = collections.Counter()
    for m in lc.members(code):
        if not m['test']:
            names[m['name']] += 1
    for m in FIELD.finditer(masked):
        names[m.group(1)] += 1
    for m in TYPE.finditer(masked):
        names[m.group(1)] += 1
    return names


def words(texts):
    c = collections.Counter()
    for t in texts:
        if t is None:
            continue
        for w in re.findall(r'\b[A-Za-z_]\w*\b', lc.blank_comments(t)):
            c[w] += 1
    return c


bw = words(base_text.values())
rw = words(red_text.values())
bdecl, rdecl = collections.Counter(), collections.Counter()
where = collections.defaultdict(set)
for p in paths:
    if base_text[p] is not None:
        for n, k in declared(base_text[p]).items():
            bdecl[n] += k
    if red_text[p] is not None:
        for n, k in declared(red_text[p]).items():
            rdecl[n] += k
            where[n].add(p)
out = ['# miembros de tests/ que el RED deja sin referencias (por texto): nombre | archivo(s) | refs base (fuera de la declaracion) | refs RED']
for n in sorted(rdecl):
    if ONLY and n not in ONLY:
        continue
    base_refs = bw[n] - bdecl[n]
    red_refs = rw[n] - rdecl[n]
    if base_refs > 0 and red_refs <= 0:
        out.append('  %s | %s | %d | %d' % (n, ', '.join(sorted(where[n])), base_refs, red_refs))
# Por archivo: los miembros privados (y los de un tipo privado anidado) de cada archivo que el RED cambia, contados en su propio archivo.
PRIV = re.compile(r'^[ \t]*(?:\[[^\]]*\]\s*)*private\b[^\n;={(]*?\b(?:class|struct|record\s+(?:struct\s+)?)?\s*(\w+)\s*(?:<[^>\n]*>)?\s*(?:\(|=(?!>)|;|\{|=>)', re.M)
out.append('')
out.append('# privados por archivo, en los archivos que el RED cambia: archivo | nombre | refs base en su archivo | refs RED en su archivo')
for p in sorted(paths):
    if base_text[p] is None or red_text[p] == base_text[p] or red_text[p] is None:
        continue
    bm, rm = lc.mask(base_text[p]), lc.mask(red_text[p])
    bkeep, rkeep = lc.blank_comments(base_text[p]), lc.blank_comments(red_text[p])
    names = {m.group(1) for m in PRIV.finditer(rm)}
    for n in sorted(names):
        rx = re.compile(r'\b' + re.escape(n) + r'\b')
        decl_b = len([m for m in PRIV.finditer(bm) if m.group(1) == n])
        decl_r = len([m for m in PRIV.finditer(rm) if m.group(1) == n])
        b_refs = len(rx.findall(bkeep)) - decl_b
        r_refs = len(rx.findall(rkeep)) - decl_r
        if b_refs > 0 and r_refs <= 0:
            out.append('  %s | %s | %d | %d' % (p, n, b_refs, r_refs))
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\n'.join(out))
