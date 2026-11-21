"""PB-T4b (paquete v4.8): escribe en una COPIA EXPORTADA el RED exacto que prescribe el paquete, para la precondición 6.

Uso:
    python -I apply_red_pbt4b_v48.py <dir-copia> [--repo <checkout>] [--rev <rev>] [--partition <pbt4b-copy-partition.tsv>] [--dry]

Como apply_red_pbt4b_v45.py, v46 y v47 (que no se tocan), con los `Skip` de pbt4b_skips_v48.py (la lista de 478 y el mapa v5) y las copias de
make_copies_pbt4b_v48.py (D-S6: el espacio raíz y el prefijo FullPanelCopy; 21 copias), que lee la partición con su load_partition (una `Theory` partida por filas, D-S5, tiene dos filas). Escribe, con LF y en
UTF-8: el esqueleto de src/ (el de la v4.4; WHC, RIA, PV y NumericFieldValidation.cs no cambian), los 478 `Skip` en los 21 archivos, el kit
(pbt4b-kit-WorkspaceFullReadKit-v45.cs) y las 21 copias (con la partición o en modo Probe). Antes comprueba que cada archivo que cambia es el de la
revisión y que los nuevos no existen. No escribe las CT. Imprime el sha256 de cada archivo y el del conjunto."""
import hashlib
import importlib.util
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
KIT_FILE = 'pbt4b-kit-WorkspaceFullReadKit-v45.cs'


def load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


def main():
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    args = sys.argv[1:]
    if not args or args[0].startswith('--'):
        raise SystemExit(__doc__)
    target = os.path.abspath(args[0])
    repo, rev, partition_path, dry = os.getcwd(), '78a7cef3', None, False
    i = 1
    while i < len(args):
        if args[i] == '--repo':
            repo, i = args[i + 1], i + 2
        elif args[i] == '--rev':
            rev, i = args[i + 1], i + 2
        elif args[i] == '--partition':
            partition_path, i = args[i + 1], i + 2
        elif args[i] == '--dry':
            dry, i = True, i + 1
        else:
            raise SystemExit('argumento desconocido: ' + args[i])
    if not os.path.isdir(os.path.join(target, 'src')) or not os.path.isdir(os.path.join(target, 'tests')):
        raise SystemExit('PARA: %s no parece una copia (faltan src/ o tests/)' % target)
    if os.path.exists(os.path.join(target, '.git')):
        raise SystemExit('PARA: %s tiene .git: no es una copia exportada' % target)

    gb, sk, mc = load('green_pbt4b_v45'), load('pbt4b_skips_v48'), load('make_copies_pbt4b_v48')
    base, red = gb.skeleton(repo, rev)
    out, problems = {}, []

    def current(rel):
        p = os.path.join(target, *rel.split('/'))
        if not os.path.isfile(p):
            return None
        return open(p, 'rb').read().decode('utf-8').lstrip('﻿').replace('\r\n', '\n')

    for rel, text in red.items():
        have = current(rel)
        if rel in base:
            if have != base[rel]:
                problems.append('no es el de la base (src con el GREEN de PB-T4a): ' + rel)
        elif have is not None:
            problems.append('ya existe (es nuevo en PB-T4b): ' + rel)
        out[rel] = text
    ria = gb.t4a_plugin(repo, rev)[gb.RIA_P]
    if current(gb.RIA_P) != ria:
        problems.append('no es el de la base (RIA del GREEN de PB-T4a): ' + gb.RIA_P)

    for rel, reasons in sk.by_file().items():
        have = current(rel)
        if have is None:
            problems.append('falta: ' + rel)
            continue
        git_text = subprocess.run(['git', '-C', repo, 'show', '%s:%s' % (rev, rel)], capture_output=True).stdout.decode('utf-8').lstrip('﻿').replace('\r\n', '\n')
        if have != git_text:
            problems.append('no es el de %s: %s' % (rev, rel))
            continue
        out[rel], _ = sk.apply(have, reasons)

    kit_rel = 'tests/RackCad.Tests/Workspace/WorkspaceFullReadKit.cs'
    if current(kit_rel) is not None:
        problems.append('ya existe: ' + kit_rel)
    out[kit_rel] = open(os.path.join(HERE, KIT_FILE), 'rb').read().decode('utf-8')

    partition = mc.load_partition(partition_path) if partition_path else None
    copies, _ = mc.build(repo, rev, partition)
    for rel, text in copies.items():
        if current(rel) is not None:
            problems.append('ya existe: ' + rel)
        out[rel] = text

    if problems:
        raise SystemExit('PARA, sin escribir nada:\n  ' + '\n  '.join(problems))
    lines = []
    for rel in sorted(out):
        data = out[rel].encode('utf-8')
        if not dry:
            dest = os.path.join(target, *rel.split('/'))
            os.makedirs(os.path.dirname(dest), exist_ok=True)
            with open(dest, 'wb') as f:
                f.write(data)
        h = hashlib.sha256(data).hexdigest()
        lines.append('%s  %s' % (h, rel))
        print('%s  %7d  %s' % (h, len(data), rel))
    print('%s %d archivos; conjunto %s' % ('SECO' if dry else 'ESCRITOS', len(out), hashlib.sha256(('\n'.join(lines) + '\n').encode('utf-8')).hexdigest()))


if __name__ == '__main__':
    main()
