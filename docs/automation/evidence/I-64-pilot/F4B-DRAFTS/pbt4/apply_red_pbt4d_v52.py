"""PB-T4d (paquete v5.2): escribe el RED del paquete en una copia exportada de la base: la pieza literal de tests/ (red_pbt4d_v52.py, con la
pieza `pbt4d-red-v52.txt` o la que se da con --pieza). PB-T4d no tiene esqueleto: el RED no toca src/ (D-S11). Antes comprueba que cada archivo
que toca es el de la revision (por su texto con \n) y que los nuevos no existen. Los existentes conservan sus saltos de linea y su BOM; los nuevos
van con \n. Escribe solo dentro de la copia e imprime el sha256 (del texto con \n) de cada archivo que escribe.

Uso: python -I apply_red_pbt4d_v52.py <dir-copia> --repo <checkout> --rev <rev> [--pieza <pieza>]
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
    spec = importlib.util.spec_from_file_location('red_pbt4d_v52', os.path.join(HERE, 'red_pbt4d_v52.py'))
    rp = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(rp)
    base, red, new = rp.red(repo, rev, piece)
    n_edit = 0
    for rel in sorted(red):
        path = os.path.join(copy, rel)
        assert rel.startswith('tests/'), rel
        if rel in new:
            assert not os.path.exists(path), rel
            data, crlf, bom = red[rel], False, False
        else:
            raw = open(path, 'rb').read()
            bom = raw.startswith(b'\xef\xbb\xbf')
            text = raw.decode('utf-8-sig')
            crlf = '\r\n' in text
            assert text.replace('\r\n', '\n') == base[rel], ('la copia no es la revision', rel)
            data = red[rel]
            n_edit += 1
        with open(path, 'wb') as f:
            f.write((b'\xef\xbb\xbf' if bom else b'') + (data.replace('\n', '\r\n') if crlf else data).encode('utf-8'))
        print('%s %s %s' % ('nuevo' if rel in new else 'red', hashlib.sha256(red[rel].encode('utf-8')).hexdigest(), rel))
    print('RED de PB-T4d escrito: %d archivos existentes de tests/ y %d nuevos; src/ sin tocar' % (n_edit, len(new)))


if __name__ == '__main__':
    main()
