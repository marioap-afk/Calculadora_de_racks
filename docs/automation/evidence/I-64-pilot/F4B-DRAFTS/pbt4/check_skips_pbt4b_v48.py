"""PB-T4b (paquete v4.8): la comprobación mecánica de los `Skip` de la lista de 478, por objetos git, que CT-PBT4B-20 hace también en ejecución.
Como check_skips_pbt4b_v47.py (que no se toca), con los motivos de pbt4b_skips_v48.py (la lista de 478 y el mapa de la copia v5).

Uso:
    python -I check_skips_pbt4b_v48.py <checkout> <base> <revision>
"""
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
    spec = importlib.util.spec_from_file_location('sk', os.path.join(HERE, 'pbt4b_skips_v48.py'))
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
    if bad or total != 478 or files != 21:
        raise SystemExit(1)


if __name__ == '__main__':
    main()
