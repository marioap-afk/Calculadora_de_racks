"""Escribe en el worktree el src/ del GREEN LITERAL del paquete (green_pbt4b_v45.py: el texto que prescribe el paquete, construido en memoria sobre
los objetos git de la base), con LF y UTF-8, tras comprobar que cada archivo es el de la base o el del esqueleto del RED. No toca tests/.
Uso: python -I apply_green_worktree.py <worktree> <rev-base> [--dry]"""
import hashlib
import importlib.util
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    target, rev = os.path.abspath(sys.argv[1]), sys.argv[2]
    dry = '--dry' in sys.argv
    spec = importlib.util.spec_from_file_location('green_pbt4b_v45', os.path.join(HERE, 'genfiles', 'green_pbt4b_v45.py'))
    gb = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(gb)
    base, green = gb.green(target, rev)
    _, red = gb.skeleton(target, rev)
    assert len(green) == 10, sorted(green)
    problems = []
    for rel in sorted(green):
        path = os.path.join(target, *rel.split('/'))
        have = open(path, 'rb').read().decode('utf-8').lstrip('﻿').replace('\r\n', '\n') if os.path.isfile(path) else None
        allowed = [t for t in (base.get(rel), red.get(rel)) if t is not None]
        if have is None:
            if rel in base:
                problems.append('falta: ' + rel)
        elif have not in allowed:
            problems.append('no es el de la base ni el del esqueleto: ' + rel)
    if problems:
        raise SystemExit('PARA, sin escribir nada:\n  ' + '\n  '.join(problems))
    for rel in sorted(green):
        data = green[rel].encode('utf-8')
        if not dry:
            with open(os.path.join(target, *rel.split('/')), 'wb') as f:
                f.write(data)
        print('%s  %7d  %s' % (hashlib.sha256(data).hexdigest(), len(data), rel))
    print('%s %d archivos' % ('SECO' if dry else 'ESCRITOS', len(green)))


if __name__ == '__main__':
    main()
