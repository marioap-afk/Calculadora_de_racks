"""PB-T4e (paquete v5.6), la sonda de las lineas de las CT: escribe en una copia (nunca en un commit) una copia temporal de cada clase de las CT
con otro nombre (prefijo Sonda) cuyo Finish acaba en Assert.Fail con las lineas medidas (INV-PBT4E-10), para leerlas de la TRX. Lee las clases de
las CT de ../ct/ (las de la pieza del RED). Escribe con \n. Solo escribe dentro de la copia.

Uso: python -I write_probe_v55.py <dir-copia>
"""
import os
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
CT = os.path.normpath(os.path.join(HERE, '..', 'ct'))
FINISH = 'Assert.Equal(string.Join("\\n", expected), string.Join("\\n", log));'
PROBE = 'Assert.Fail("SONDA\\n" + string.Join("\\n", log));'
ITEMS = [
    ('PushBackEditAllReloadTests.cs', 'tests/RackCad.Tests/SondaPushBackEditAllReloadTests.cs', 'PushBackEditAllReloadTests', 'SondaPushBackEditAllReloadTests'),
    ('PushBackEditAllReloadFieldTests.cs', 'tests/RackCad.UI.Tests/SondaPushBackEditAllReloadFieldTests.cs', 'PushBackEditAllReloadFieldTests',
     'SondaPushBackEditAllReloadFieldTests'),
]


def main():
    copy = sys.argv[1]
    for source, target, old, new in ITEMS:
        text = open(os.path.join(CT, source), encoding='utf-8').read().replace('\r\n', '\n')
        assert text.count(FINISH) == 1, source
        assert text.count('public sealed class ' + old) == 1, source
        text = text.replace(FINISH, PROBE).replace('public sealed class ' + old, 'public sealed class ' + new)
        path = os.path.join(copy, target)
        assert not os.path.exists(path), path
        with open(path, 'wb') as f:
            f.write(text.encode('utf-8'))
        print('sonda %s' % target)


if __name__ == '__main__':
    main()
