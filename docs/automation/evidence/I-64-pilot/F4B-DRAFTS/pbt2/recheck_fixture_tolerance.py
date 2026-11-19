"""Recomprobacion de la v2.4 (CA-T2b §10): solo lectura por objetos git.

Para cada fixture de tests/RackCad.Tests/Fixtures/I64 en <rev>, imprime cada clave que contiene
'Tolerance' (a cualquier profundidad, con el Design interior desanidado), el nombre del sobre y
la presencia del lado B. Uso: python -I recheck_fixture_tolerance.py <repo> <rev>
"""
import json
import subprocess
import sys

REPO, REV = sys.argv[1], sys.argv[2]
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
            if 'tolerance' in k.lower():
                out.append((p, v))
            walk(v, p, out)
    elif isinstance(node, list):
        for i, v in enumerate(node):
            walk(v, path + '[%d]' % i, out)


names = git('ls-tree', '--name-only', REV, '--', DIR).decode().split()
for name in names:
    raw = git('show', REV + ':' + name).decode('utf-8-sig')
    env = json.loads(raw)
    out = []
    walk(env, '', out)
    vals = sorted(set(repr(v) for _, v in out))
    print('%-30s Name=%-14r claves=%d valores=%s' % (name.split('/')[-1], env.get('Name'), len(out), ','.join(vals)))
    for p, v in out:
        print('    %s = %r' % (p, v))
