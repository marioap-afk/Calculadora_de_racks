"""PB-T4b (paquete v4.5): escribe en una COPIA EXPORTADA el RED exacto que prescribe el paquete, para la precondición 6.

Uso:
    python -I apply_red_pbt4b_v45.py <dir-copia> [--repo <checkout>] [--rev <rev>] [--partition <pbt4b-copy-partition.tsv>] [--dry]

- <dir-copia>: la raíz de una copia exportada (git archive) del cierre de PB-T4a, con src/ y tests/; con .git para sin escribir nada.
- --rev: la revisión de los objetos; por defecto el cierre de PB-T4a, 78a7cef3 (su src/ y sus tests/ son los del GREEN faef7020).
- --partition: la partición cerrada (método -> Red | Green); sin ella, las copias van en una clase Probe por archivo, para medirla.

Escribe, con LF y en UTF-8:
1. el esqueleto de src/ (green_pbt4b_v45.skeleton, el de la v4.4): WEI y FVM, y los cuatro archivos nuevos inertes; WHC, RIA, PV y
   NumericFieldValidation.cs no cambian;
2. los `Skip` literales de las 477 (pbt4b_skips_v45.py), en los 20 archivos de las pruebas de la lista;
3. el kit (pbt4b-kit-WorkspaceFullReadKit-v45.cs) en tests/RackCad.Tests/Workspace/WorkspaceFullReadKit.cs;
4. las 20 copias (make_copies_pbt4b_v45.py), con la partición o en modo Probe.
Antes comprueba que cada archivo que cambia es el de la revisión y que los nuevos no existen. No escribe las CT. Imprime el sha256 de cada
archivo y el del conjunto."""
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

    gb, sk, mc = load('green_pbt4b_v45'), load('pbt4b_skips_v45'), load('make_copies_pbt4b_v45')
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

    partition = None
    if partition_path:
        partition = {}
        for l in open(partition_path, encoding='utf-8').read().split('\n')[1:]:
            if l:
                c = l.split('\t')
                partition[(c[0], c[1])] = c[2]
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
