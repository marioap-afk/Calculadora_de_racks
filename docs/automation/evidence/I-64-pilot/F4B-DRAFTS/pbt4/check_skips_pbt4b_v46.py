"""PB-T4b (paquete v4.6): la comprobación mecánica de los `Skip` de la lista de 477, por objetos git, que CT-PBT4B-20 hace también en ejecución.
Como check_skips_pbt4b_v45.py (que no se toca), con los motivos de pbt4b_skips_v46.py (el mapa de la copia v3).

Uso:
    python -I check_skips_pbt4b_v46.py <checkout> <base> <revision>

Para cada uno de los 20 archivos de las pruebas de la lista: el archivo de <revision> tiene que ser, byte a byte con los saltos de línea
normalizados a LF, el de <base> con el `Skip` literal de cada prueba de la lista en su línea de atributo, y nada más. <base> es el cierre de PB-T4a
(78a7cef3); <revision>, el RED o el GREEN de PB-T4b. Imprime una línea por archivo y el total de `Skip` leídos (477). Solo `git show`."""
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
    spec = importlib.util.spec_from_file_location('sk', os.path.join(HERE, 'pbt4b_skips_v46.py'))
    sk = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(sk)
    bad, total, files = 0, 0, 0
    for path, reasons in sorted(sk.by_file().items()):
        before = show(repo, base, path)
        if before is None:
            print('MAL  %s: no existe en %s' % (path, base[:8]))
            bad += 1
            continue
        expected, done = sk.apply(before, reasons)
        after = show(repo, rev, path)
        ok = after == expected
        total += len(done)
        files += 1
        bad += 0 if ok else 1
        print('%s %s skip=%d' % ('OK  ' if ok else 'MAL ', path, len(done)))
    print('archivos: %d; mal: %d; Skip leidos: %d' % (files, bad, total))
    if bad or total != 477 or files != 20:
        raise SystemExit(1)


if __name__ == '__main__':
    main()
