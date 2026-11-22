"""PB-T4d (paquete v5.2), precondicion 6, el control de las CT: escribe en una copia exportada de la base SOLO las dos clases de las CT de la pieza
del RED (CT-PBT4D-01 y 02, NEWFILE de pbt4d-red-v52.txt, por red_pbt4d_v52.red), o las borra con --quitar. Escribe solo dentro de la copia, con \n.

Uso: python -I write_cts_v52.py <copia> --repo <checkout> --rev <rev> [--quitar]
"""
import hashlib
import importlib.util
import os
import sys

sys.dont_write_bytecode = True
S = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.normpath(os.path.join(S, '..', '..', 'pbt4'))
copy = sys.argv[1]
repo = sys.argv[sys.argv.index('--repo') + 1]
rev = sys.argv[sys.argv.index('--rev') + 1]
CTS = ['tests/RackCad.Tests/PushBackRetirePilotGuardTests.cs', 'tests/RackCad.Tests/PushBackRetirePilotCensusGuardTests.cs']
assert os.path.isdir(os.path.join(copy, 'src')) and os.path.isdir(os.path.join(copy, 'tests')), copy
if '--quitar' in sys.argv:
    for rel in CTS:
        path = os.path.join(copy, rel)
        assert os.path.isfile(path), rel
        os.remove(path)
        print('quitado ' + rel)
    sys.exit(0)
spec = importlib.util.spec_from_file_location('red_pbt4d_v52', os.path.join(PB4, 'red_pbt4d_v52.py'))
rp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(rp)
base, red, new = rp.red(repo, rev)
for rel in CTS:
    assert rel in new, rel
    path = os.path.join(copy, rel)
    assert not os.path.exists(path), rel
    with open(path, 'wb') as f:
        f.write(red[rel].encode('utf-8'))
    print('nuevo %s %s' % (hashlib.sha256(red[rel].encode('utf-8')).hexdigest(), rel))
