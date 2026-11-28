"""PB-T4e (paquete v5.5): escribe el RED del paquete en una copia (exportada de la base, o el worktree del Worker en la base): la pieza literal de
tests/ (red_pbt4e_v55.py, con la pieza `pbt4e-red-v55.txt` o la que se da con --pieza). PB-T4e no tiene esqueleto: el RED no toca src/ y no
edita ni borra ningun archivo existente (solo los dos archivos nuevos de las CT). Antes comprueba que la copia tiene src/ y tests/ y que los
nuevos no existen. Los nuevos van con \n. Escribe solo dentro de la copia e imprime el sha256 (del texto con \n) de cada archivo que escribe.

Uso: python -I apply_red_pbt4e_v55.py <dir-copia> --repo <checkout> --rev <rev> [--pieza <pieza>]
"""
import hashlib
import importlib.util
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def main():
    sys.dont_write_bytecode = True
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    copy = sys.argv[1]
    args = sys.argv[2:]
    repo = args[args.index('--repo') + 1]
    rev = args[args.index('--rev') + 1]
    piece = args[args.index('--pieza') + 1] if '--pieza' in args else None
    assert os.path.isdir(os.path.join(copy, 'src')) and os.path.isdir(os.path.join(copy, 'tests')), copy
    spec = importlib.util.spec_from_file_location('red_pbt4e_v55', os.path.join(HERE, 'red_pbt4e_v55.py'))
    rp = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(rp)
    _, red, new = rp.red(repo, rev, piece)
    for rel in sorted(red):
        path = os.path.join(copy, rel)
        assert rel.startswith('tests/') and rel in new, rel
        assert not os.path.exists(path), ('el archivo nuevo ya existe en la copia', rel)
        assert os.path.isdir(os.path.dirname(path)), ('la carpeta del archivo nuevo no existe', rel)
        with open(path, 'wb') as f:
            f.write(red[rel].encode('utf-8'))
        print('nuevo %s %s' % (hashlib.sha256(red[rel].encode('utf-8')).hexdigest(), rel))
    print('RED de PB-T4e escrito: %d archivos nuevos de tests/; ningun archivo existente tocado; src/ sin tocar' % len(new))


if __name__ == '__main__':
    main()
