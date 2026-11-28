"""PB-T4b (paquete v4.8): la autoprueba de los guiones de la v4.8 por objetos git (el cierre de PB-T4a). Solo lee; escribe solo su salida y, si se
le pide, la partición sintética de prueba.

Uso: python -I selftest_pbt4b_v48.py <checkout> <rev> <particion-v47.tsv> <particion-sintetica.tsv> <salida.txt>
1. la lista de 478 (la guarda nueva al final, en C-5) y el mapa v5 (356 mecánicas y 72 a mano); los 478 `Skip`, con 72 «a mano» en C-0 y uno con
   la decisión owner-pbt4b-list-478-decision.md;
2. las 21 copias en el modo de medida (su conjunto y la revisión): D-S6, cada una en el espacio raíz de su ensamblado, sin `FullPanelCopies`, con
   una sola clase `FullPanelCopy<Raíz>ProbeTests`;
3. D-S1 en las dos de WPP: el lateral fantasma, `Ready` con borrados [B2]; `side-b`, con `FrontalBOfOneWayPrefix`; ningún `ViewUnavailablePrefix`;
4. la guarda nueva: su copia lleva la tabla del original con tres cuentas nuevas (2 -> 0, 5 -> 6 y 1 -> 0, con su `// before N`) y las otras 27
   filas iguales;
5. las 21 copias con una partición sintética: la de la v4.7 (`partition_pbt4b_v47.py red` sobre las TRX de /d/r4l, de la v4.7) con la guarda nueva
   como Red y las dos de WPP de D-S1 como Green, que es lo previsto (lo que valga lo mide la precondición 6): las cuatro de D-S5 siguen partidas por
   filas, con sus clases `FullPanelCopy<Raíz>RedTests` y `…GreenTests`.
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


repo, rev, part47, part_syn, out_path = sys.argv[1:6]
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
sk, mc = load('pbt4b_skips_v48'), load('make_copies_pbt4b_v48')
he = mc.he
out = []
by_file = sk.by_file()
reasons = [r for f in by_file.values() for r in f.values()]
hand = sum(1 for r in reasons if 'equivalente escrito a mano' in r)
l478 = sum(1 for r in reasons if 'owner-pbt4b-list-478-decision.md' in r)
out.append('1. Skip: %d en %d archivos; con el motivo a mano: %d; con la decision de la lista de 478: %d (%s)' % (
    len(reasons), len(by_file), hand, l478, by_file[he.WHHG][he.NONE30]))
assert len(reasons) == 478 and len(by_file) == 21 and hand == 72 and l478 == 1
files, review = mc.build(repo, rev, None)
lines = ['%s  %s' % (hashlib.sha256(files[r].encode('utf-8')).hexdigest(), r) for r in sorted(files)]
out.append('2. modo de medida: %d copias, conjunto %s; revision: %d entradas' % (
    len(files), hashlib.sha256(('\n'.join(lines) + '\n').encode('utf-8')).hexdigest(), len(review)))
assert len(files) == 21
for rel, text in sorted(files.items()):
    root = 'RackCad.UI.Tests' if rel.startswith('tests/RackCad.UI.Tests/') else 'RackCad.Tests'
    ns = re.findall(r'^namespace\s+([\w.]+)', text, re.M)
    classes = re.findall(r'\bclass\s+(FullPanelCopy\w+Tests)\b', text)
    stem = os.path.basename(rel)[:-len('Full.cs')]
    ok = ns == [root] and 'FullPanelCopies' not in text and classes == ['FullPanelCopy%sProbeTests' % stem]
    assert ok, (rel, ns, classes)
out.append('   las 21: el espacio raiz de su ensamblado, sin FullPanelCopies, con una sola clase FullPanelCopy<Raiz>ProbeTests: True')

wpp = files['tests/RackCad.Tests/Workspace/WorkspacePilotPrepareFull.cs']


def method(text, name):
    m = re.search(r'\bpublic\s+void\s+' + name + r'\s*\(', text)
    assert m, name
    start = text.rfind('[Fact]', 0, m.start())
    depth, i = 0, text.index('{', m.end())
    for j in range(i, len(text)):
        depth += {'{': 1, '}': -1}.get(text[j], 0)
        if depth == 0:
            return text[start:j + 1]
    raise AssertionError(name)


inv, nev = method(wpp, he.INVALID_E), method(wpp, he.NEVER_ERASES)
checks = {
    'InvalidE sin el fantasma entre los Invalid': '["phantom-lateral"]' not in inv,
    'InvalidE con FrontalBOfOneWayPrefix y sin ViewUnavailablePrefix': 'FrontalBOfOneWayPrefix' in inv and 'ViewUnavailable' not in inv,
    'InvalidE: el fantasma Ready con borrados [B2]': 'Assert.Equal(PushBackPilotPrepareKind.Ready, phantom.Kind);' in inv and '"B2" }, phantom.Plan!.Erasures' in inv,
    'NeverErases: Ready, miembros [A1] y borrados [B2]': 'Assert.Equal(PushBackPilotPrepareKind.Ready, result.Kind);' in nev and '"B2" }, result.Plan!.Erasures' in nev,
    'NeverErases: sin el fantasma, sin borrados, y ningun Invalid': 'Assert.Empty(ready.Plan!.Erasures);' in nev and 'PushBackPilotPrepareKind.Invalid' not in nev,
}
out.append('3. D-S1 en las dos de WPP: ' + '; '.join('%s=%s' % kv for kv in checks.items()))
assert all(checks.values())

orig = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, he.WHHG)], capture_output=True).stdout.decode('utf-8').replace('\r\n', '\n')
copy = files['tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardFull.cs']
row = re.compile(r'^        \("[^"]*", "[^"]*", \d+\),.*$', re.M)
o_rows, c_rows = row.findall(orig), row.findall(copy)
changed = [(a, b) for a, b in zip(o_rows, c_rows) if a != b]
want = [(old.rstrip('\n'), new.rstrip('\n')) for old, new, _ in he.HELPER_EDITS[he.WHHG]]
out.append('4. la guarda nueva: filas de la tabla %d en el original y %d en la copia; cambian %d, las de la decision: %s' % (
    len(o_rows), len(c_rows), len(changed), changed == want))
assert len(o_rows) == len(c_rows) == 30 and changed == want
for _, new in want:
    out.append('   ' + new.strip())

# la partición sintética: la de la v4.7, la guarda nueva Red y las dos de WPP de D-S1 Green
rows = [l.split('\t') for l in open(part47, encoding='utf-8').read().split('\n')[1:] if l]
syn = []
for r in rows:
    if r[0] == he.WPP and r[1] in (he.INVALID_E, he.NEVER_ERASES):
        assert r[2] == 'Red', r
        r = [r[0], r[1], 'Green', r[3]]
    syn.append(r)
syn.append([he.WHHG, he.NONE30, 'Red', '1'])
text = 'archivo\tmetodo\tparticion\tresultados\n' + ''.join('\t'.join(r) + '\n' for r in syn)
with open(part_syn, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text)
part = mc.load_partition(part_syn)
pfiles, _ = mc.build(repo, rev, part)
out.append('5. con la particion sintetica (%s, sha256 %s, %d filas): %d copias' % (
    os.path.basename(part_syn), hashlib.sha256(text.encode('utf-8')).hexdigest(), len(syn), len(pfiles)))
for (path, name), spec in sorted(he.SPLIT.items()):
    stem = os.path.basename(path)[:-len('Tests.cs')]
    t = pfiles[os.path.dirname(path) + '/' + stem + 'Full.cs']
    both = all(re.search(r'\bclass\s+FullPanelCopy%s%sTests\b' % (stem, h), t) for h in ('Red', 'Green'))
    twice = len(re.findall(r'\bpublic\s+void\s+' + name + r'\s*\(', t)) == 2
    out.append('   %sFull.%s: en FullPanelCopy%sRedTests y ...GreenTests=%s, dos veces=%s' % (stem, name, stem, both, twice))
    assert both and twice
whhg = pfiles['tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardFull.cs']
out.append('   la guarda nueva, en FullPanelCopyWorkspaceHostHardeningGuardRedTests: %s' % bool(re.search(r'\bclass FullPanelCopyWorkspaceHostHardeningGuardRedTests\b', whhg)))
assert re.search(r'\bclass FullPanelCopyWorkspaceHostHardeningGuardRedTests\b', whhg) and 'GreenTests' not in whhg
with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\n'.join(out))
