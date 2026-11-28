"""Recomprobacion de la v2.4 (CA-T2b §10): solo lectura por objetos git.

Imprime, para los fixtures pedidos, cada clave escalar cuyo nombre contiene alguno de los fragmentos dados,
con el Design interior desanidado. Uso:
python -I recheck_fixture_fields.py <repo> <rev> <fixture1,fixture2,...> <frag1,frag2,...>
"""
import json
import subprocess
import sys

REPO, REV, FIXTURES, FRAGS = sys.argv[1], sys.argv[2], sys.argv[3].split(','), [f.lower() for f in sys.argv[4].split(',')]
DIR = 'tests/RackCad.Tests/Fixtures/I64/'


def git(*args):
    return subprocess.run(['git', '-C', REPO] + list(args), check=True, capture_output=True).stdout


def walk(node, path, out):
    if isinstance(node, dict):
        for k, v in node.items():
            p = path + '.' + k
            if isinstance(v, str) and v.lstrip().startswith('{'):
                try:
                    walk(json.loads(v), p, out)
                    continue
                except ValueError:
                    pass
            if not isinstance(v, (dict, list)) and any(f in k.lower() for f in FRAGS):
                out.append((p, v))
            walk(v, p, out)
    elif isinstance(node, list):
        for i, v in enumerate(node):
            walk(v, path + '[%d]' % i, out)


for fx in FIXTURES:
    env = json.loads(git('show', REV + ':' + DIR + fx + '.json').decode('utf-8-sig'))
    out = []
    walk(env, '', out)
    print('== %s (%d claves)' % (fx, len(out)))
    for p, v in out:
        print('    %s = %r' % (p, v))
