"""PB-T4b (paquete v4.9): la autoprueba de CA-T4b I-01 en el guion de la copia, por objetos git del cierre de PB-T4a. Solo lee; escribe solo su salida.

Uso: python -I selftest_pbt4b_v49.py <checkout> <rev> <particion-v48.tsv> <salida.txt>
1. en el modo de medida, las 21 copias de la v4.9 son las de la v4.8 salvo `WorkspaceHostHardeningGuardFull.cs`;
2. esa copia es la de la v4.8 con un solo bloque añadido tras el `Assert.Empty` de `NoneOfThe30…`: el comentario de I-01 y, línea a línea, las
   líneas de la autoprueba original (WHHG:504-525 de la revisión, de `var host` a la última `Assert.NotEmpty`), con sus 13 `Assert.NotEmpty`;
3. con la partición de la v4.8 (la de /d/r4q), la prueba de la copia está en `FullPanelCopyWorkspaceHostHardeningGuardRedTests`, y las otras 20
   copias son las de la v4.8 con esa partición.
"""
import hashlib
import importlib.util
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def load(name, alias):
    spec = importlib.util.spec_from_file_location(alias, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


repo, rev, part48, out_path = sys.argv[1:5]
sys.stdout.reconfigure(encoding='utf-8', errors='replace')
m49 = load('make_copies_pbt4b_v49', 'm49')
m48 = load('make_copies_pbt4b_v48', 'm48')
he = m49.he
COPY = 'tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardFull.cs'
out = []
f49, _ = m49.build(repo, rev, None)
f48, _ = m48.build(repo, rev, None)
same = sorted(k for k in f49 if f49[k] == f48.get(k))
diff = sorted(set(f49) ^ set(f48)) + sorted(k for k in f49 if k in f48 and f49[k] != f48[k])
out.append('1. modo de medida: %d copias; iguales a la v4.8: %d; distintas: %s' % (len(f49), len(same), diff))
assert len(f49) == 21 and len(same) == 20 and diff == [COPY], diff
orig = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, he.WHHG)], capture_output=True).stdout.decode('utf-8').replace('\r\n', '\n').split('\n')
assert orig[500].strip() == '[Fact]' and he.SELFTEST in orig[501], orig[500:502]
want = '\n'.join(orig[503:525]) + '\n'
assert orig[503].strip() == 'var host = Src.Code(HostPath);' and orig[524].strip().startswith('Assert.NotEmpty(') and orig[525].strip() == '}', orig[523:526]
a, b = f48[COPY], f49[COPY]
anchor = '        Assert.Empty(LiteralViolations(Src.Code(HostPath), Src.Code(SupportPath)));\n'
assert a.count(anchor) == 1 and b.count(anchor) == 1
i = a.index(anchor) + len(anchor)
added = b[i:len(b) - (len(a) - i)]
ok_rest = b[:i] == a[:i] and b[i + len(added):] == a[i:]
ok_lines = added.endswith(want) and added[:-len(want)] == he._WHY
n_notempty = want.count('Assert.NotEmpty(')
out.append('2. %s: un solo bloque tras el Assert.Empty=%s; el bloque es el comentario de I-01 y las lineas WHHG:504-525 de %s, literales=%s; '
           'Assert.NotEmpty: %d' % (os.path.basename(COPY), ok_rest, rev, ok_lines, n_notempty))
assert ok_rest and ok_lines and n_notempty == 13
part = m49.load_partition(part48)
p49, _ = m49.build(repo, rev, part)
p48, _ = m48.build(repo, rev, part)
psame = sorted(k for k in p49 if p49[k] == p48.get(k))
red = 'class FullPanelCopyWorkspaceHostHardeningGuardRedTests' in p49[COPY] and 'GreenTests' not in p49[COPY]
out.append('3. con la particion de la v4.8 (%s, sha256 %s): iguales a la v4.8: %d de %d; la de WHHG en su clase Red=%s' % (
    os.path.basename(part48), hashlib.sha256(open(part48, 'rb').read()).hexdigest(), len(psame), len(p49), red))
assert len(psame) == 20 and red
with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(out) + '\n')
print('\n'.join(out))
