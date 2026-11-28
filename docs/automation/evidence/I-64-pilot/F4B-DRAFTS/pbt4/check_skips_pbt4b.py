"""PB-T4b (paquete v4.4): la comprobación mecánica de los `Skip` de la lista de 475, por objetos git, que CT-PBT4B-20 hace también en ejecución.

Uso:
    python -I check_skips_pbt4b.py <checkout> <base> <revision>

Para cada uno de los archivos de las pruebas de la lista (los 9 de C-0, los 7 de las guardas, los 2 de la interfaz y, si existe en <base>, el de
CT-PBT4A-06): el archivo de <revision> tiene que ser, byte a byte con los saltos de línea normalizados a LF, el de <base> con el `Skip` literal de
cada prueba de la lista en su línea de atributo (pbt4b_skips.py), y nada más. Así se leen los 47 `Skip` de C-5 y de la interfaz, que CT-20 no
cubría en la v4.3, y los 428 de C-0. <base> es el cierre de PB-T4a; <revision>, el RED o el GREEN de PB-T4b. Imprime una línea por archivo.
Solo `git show`."""
import importlib.util
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def show(repo, rev, path):
    p = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, path)], capture_output=True)
    if p.returncode:
        return None
    return p.stdout.decode('utf-8').lstrip('﻿').replace('\r\n', '\n')


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    if len(sys.argv) != 4:
        raise SystemExit(__doc__)
    repo, base, rev = sys.argv[1:4]
    spec = importlib.util.spec_from_file_location('sk', os.path.join(HERE, 'pbt4b_skips.py'))
    sk = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(sk)
    bad, total = 0, 0
    for path, reasons in sorted(sk.by_file().items()):
        before = show(repo, base, path)
        if before is None:
            print('PENDIENTE %s: no existe en %s' % (path, base[:8]))
            continue
        expected, done = sk.apply(before, reasons)
        after = show(repo, rev, path)
        ok = after == expected
        total += len(done)
        bad += 0 if ok else 1
        print('%s %s skip=%d' % ('OK  ' if ok else 'MAL ', path, len(done)))
    print('archivos mal: %d; Skip leidos: %d' % (bad, total))
    if bad:
        raise SystemExit(1)


if __name__ == '__main__':
    main()
