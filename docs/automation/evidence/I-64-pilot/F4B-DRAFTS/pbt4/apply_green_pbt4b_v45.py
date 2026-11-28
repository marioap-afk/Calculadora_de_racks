"""PB-T4b (paquete v4.5): escribe en una COPIA EXPORTADA el src/ del GREEN literal que prescribe el paquete (green_pbt4b_v45.py: el de la v4.4
y RIA con D-S4).

Uso:
    python -I apply_green_pbt4b_v45.py <dir-copia> [--repo <checkout>] [--rev <rev>] [--dry]

- <dir-copia>: la raíz de una copia exportada del cierre de PB-T4a (con src/ y tests/; con .git para sin escribir nada), en el estado de la base
  o en el del RED (después de apply_red_pbt4b_v45.py).
- --rev: la revisión de los objetos; por defecto el cierre de PB-T4a, 78a7cef3.
- --dry: comprueba y lista, sin escribir.

Lo que hace: construye en memoria el GREEN literal de PB-T4b (WHC, WEI, FVM, PV, NumericFieldValidation.cs y RIA editados; NumericTextRule.cs y
los tres archivos nuevos de Draft/); comprueba que cada archivo, en la copia, es el de la base o el del esqueleto del RED (saltos de línea a LF),
y que un archivo nuevo no existe o es el del esqueleto; si algo no casa, para sin escribir nada; escribe los diez archivos con LF y en UTF-8, y
no toca tests/. Imprime cada ruta, su tamaño y su sha256, y el del conjunto."""
import hashlib
import importlib.util
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    args = sys.argv[1:]
    if not args or args[0].startswith('--'):
        raise SystemExit(__doc__)
    target = os.path.abspath(args[0])
    repo, rev, dry = os.getcwd(), '78a7cef3', False
    i = 1
    while i < len(args):
        if args[i] == '--repo':
            repo, i = args[i + 1], i + 2
        elif args[i] == '--rev':
            rev, i = args[i + 1], i + 2
        elif args[i] == '--dry':
            dry, i = True, i + 1
        else:
            raise SystemExit('argumento desconocido: ' + args[i])
    if not os.path.isdir(os.path.join(target, 'src')) or not os.path.isdir(os.path.join(target, 'tests')):
        raise SystemExit('PARA: %s no parece la raíz de una copia (faltan src/ o tests/)' % target)
    if os.path.exists(os.path.join(target, '.git')):
        raise SystemExit('PARA: %s tiene .git: es un checkout o un worktree, no una copia exportada. No se escribe nada.' % target)

    spec = importlib.util.spec_from_file_location('gb45', os.path.join(HERE, 'green_pbt4b_v45.py'))
    gb = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(gb)
    base, green = gb.green(repo, rev)
    _, red = gb.skeleton(repo, rev)
    assert len(green) == 10, sorted(green)

    problems = []
    for rel in sorted(green):
        path = os.path.join(target, *rel.split('/'))
        have = open(path, 'rb').read().decode('utf-8').lstrip('﻿').replace('\r\n', '\n') if os.path.isfile(path) else None
        allowed = [t for t in (base.get(rel), red.get(rel)) if t is not None]
        if have is None:
            if rel in base:
                problems.append('falta en la copia: ' + rel)
        elif have not in allowed:
            problems.append('no es el de la base ni el del esqueleto: ' + rel)
    if problems:
        raise SystemExit('PARA, sin escribir nada:\n  ' + '\n  '.join(problems))

    lines = []
    for rel in sorted(green):
        data = green[rel].encode('utf-8')
        if not dry:
            path = os.path.join(target, *rel.split('/'))
            os.makedirs(os.path.dirname(path), exist_ok=True)
            with open(path, 'wb') as f:
                f.write(data)
        h = hashlib.sha256(data).hexdigest()
        lines.append('%s  %s' % (h, rel))
        print('%s  %7d  %s' % (h, len(data), rel))
    print('%s %d archivos; conjunto %s' % ('SECO' if dry else 'ESCRITOS', len(green), hashlib.sha256(('\n'.join(lines) + '\n').encode('utf-8')).hexdigest()))


if __name__ == '__main__':
    main()
