"""PB-T4c (paquete v5.0): la comprobacion mecanica de los Skip de la lista de 4 (ExpectedEvidence [15]). Cada uno de los cuatro archivos de la
lista, en el arbol que se le da (una copia o el worktree, leido por su texto con \n), es su base (BaseSha, por objetos git) con exactamente su
linea de Skip literal (pbt4c_skips_v50.py) en el lugar de su `[Fact]`, y nada mas. Solo lee; escribe la salida en UTF-8 con LF.

Uso: python -I check_skips_pbt4c_v50.py <repo> <base-rev> <arbol> <salida.txt>
"""
import importlib.util
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('pbt4c_skips_v50', os.path.join(HERE, 'pbt4c_skips_v50.py'))
sk = importlib.util.module_from_spec(spec)
spec.loader.exec_module(sk)
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
repo, base, tree, out = sys.argv[1:5]
lines, ok = [], True
for s in sk.SKIPS:
    rel = s[0]
    want = sk.apply(subprocess.run(['git', '-C', repo, 'show', base + ':' + rel], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n'), rel)
    have = open(os.path.join(tree, rel), 'rb').read().decode('utf-8-sig').replace('\r\n', '\n')
    good = have == want
    ok = ok and good
    lines.append('%s skip-linea=%d igual-a-la-base-con-su-skip=%s skips-pbt4c=%d' % (rel, s[1], good, have.count('[Fact(Skip = "I-64 F4B PB-T4c: ')))
lines.append('check_skips PB-T4c: %s (%d archivos, %d Skip)' % ('PASA' if ok else 'PARA', len(sk.SKIPS), len(sk.SKIPS)))
with open(out, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(lines) + '\n')
print(lines[-1])
sys.exit(0 if ok else 1)
