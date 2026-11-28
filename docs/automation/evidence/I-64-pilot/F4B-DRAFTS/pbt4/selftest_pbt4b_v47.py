"""PB-T4b (paquete v4.7): la autoprueba de los guiones de la v4.7 por objetos git (el cierre de PB-T4a). Solo lee; escribe solo su salida.

Uso: python -I selftest_pbt4b_v47.py <checkout> <rev> <particion-de-prueba.tsv> <salida.txt>
1. el mapa v4: 358 mecánicas y 70 a mano; los 477 `Skip`, con 70 motivos «a mano» en C-0;
2. las 20 copias en el modo de medida (su conjunto y la revisión), con las cuatro `Theory` de D-S5 enteras;
3. las 20 copias con una partición de prueba (la de `partition_pbt4b_v47.py red` sobre una TRX del modo de medida): cada una de las cuatro está en
   <Clase>FullRedTests y en <Clase>FullGreenTests, con sus filas `InlineData` (juntas, las del original, una a una) o con StatusNamesRed y
   StatusNamesGreen, y los dos miembros nuevos solo en la copia de WPP, cada uno solo en su clase;
4. los miembros nuevos enumeran StatusNames() como string, sin `row[0]` (la v4.7 del primer RED, /d/r4k, no compilaba: CS0030).
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


repo, rev, part_path, out_path = sys.argv[1:5]
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
sk, mc = load('pbt4b_skips_v47'), load('make_copies_pbt4b_v47')
he = mc.he
out = []
reasons = [r for f in sk.by_file().values() for r in f.values()]
hand = sum(1 for r in reasons if 'equivalente escrito a mano' in r)
out.append('1. Skip: %d; con el motivo a mano: %d' % (len(reasons), hand))
assert len(reasons) == 477 and hand == 70
files, review = mc.build(repo, rev, None)
lines = ['%s  %s' % (hashlib.sha256(files[r].encode('utf-8')).hexdigest(), r) for r in sorted(files)]
out.append('2. modo de medida: %d copias, conjunto %s; revision: %d entradas' % (
    len(files), hashlib.sha256(('\n'.join(lines) + '\n').encode('utf-8')).hexdigest(), len(review)))
part = mc.load_partition(part_path)
pfiles, _ = mc.build(repo, rev, part)
out.append('3. con la particion de prueba (%s, sha256 %s): %d copias' % (os.path.basename(part_path), hashlib.sha256(open(part_path, 'rb').read()).hexdigest(), len(pfiles)))


def class_body(text, name):
    masked = mc.lc.mask(text)
    assert len(masked) == len(text)
    m = re.search(r'\bclass\s+' + name + r'\b', masked)
    assert m, name
    i = masked.index('{', m.end())
    depth = 0
    for j in range(i, len(masked)):
        depth += {'{': 1, '}': -1}.get(masked[j], 0)
        if depth == 0:
            return text[i:j + 1]
    raise AssertionError(name)


def method_attrs(body, method):
    m = re.search(r'\bpublic\s+void\s+' + method + r'\s*\(', body)
    if not m:
        return None
    start = body.rfind('[Theory]', 0, m.start())
    return body[start:m.start()]


for (path, method), spec in sorted(he.SPLIT.items()):
    stem = os.path.basename(path)[:-len('Tests.cs')]
    rel = os.path.dirname(path) + '/' + stem + 'Full.cs'
    text = pfiles[rel]
    red, green = (method_attrs(class_body(text, stem + 'Full%sTests' % h), method) for h in ('Red', 'Green'))
    orig = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True).stdout.decode('utf-8').replace('\r\n', '\n')
    oattrs = method_attrs(orig, method)
    if spec[0] == 'inline':
        rl = re.findall(r'\[InlineData\(.*\)\]', red)
        gl = re.findall(r'\[InlineData\(.*\)\]', green)
        ol = re.findall(r'\[InlineData\(.*\)\]', oattrs)
        if path == he.PEG:
            ol = [l.replace('PushBackPilotFieldAdapter', 'PushBackFullFieldAdapter') for l in ol]
        same = sorted(rl + gl) == sorted(ol) and not set(rl) & set(gl)
        out.append('   %sFull.%s: red=%d green=%d filas-del-original=%s' % (stem, method, len(rl), len(gl), same))
        assert same and len(rl) == len(spec[1])
    else:
        ok = '[MemberData(nameof(StatusNamesRed))]' in red and '[MemberData(nameof(StatusNamesGreen))]' in green
        bodies = {h: class_body(text, stem + 'Full%sTests' % h) for h in ('Red', 'Green')}
        members = ('StatusNamesRed()' in bodies['Red'] and 'StatusNamesGreen' not in bodies['Red']
                   and 'StatusNamesGreen()' in bodies['Green'] and 'StatusNamesRed' not in bodies['Green'])
        out.append('   %sFull.%s: MemberData Red/Green=%s cada-clase-solo-su-miembro=%s; el original no los tiene=%s' % (
            stem, method, ok, members, 'StatusNamesRed' not in orig))
        assert ok and members and 'StatusNamesRed' not in orig
# los miembros nuevos: sin acceso por índice a la fila (en xunit 2.9.2, TheoryData<string> se enumera como string; CS0030 en /d/r4k)
for _, _, text in he.C0_NEW_HELPERS[he.WPP]:
    assert 'row[0]' not in text and 'foreach (string name in StatusNames())' in text
out.append('4. los miembros nuevos enumeran StatusNames() como string (foreach (string name in ...)), sin row[0]: True')
with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\n'.join(out))
