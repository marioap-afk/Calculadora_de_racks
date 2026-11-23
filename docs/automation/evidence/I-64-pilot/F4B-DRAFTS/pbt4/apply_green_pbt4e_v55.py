"""PB-T4e (paquete v5.5): escribe el GREEN literal de src/ (green_pbt4e_v55.green: la recarga de Editar todo, D-PBT4-22) sobre una copia que ya
tiene el RED del paquete. Antes comprueba, por su texto con \n, que cada archivo de src/ que toca es el de la revision (el RED no toca src/);
tests/ no se toca. Los existentes conservan sus saltos de linea y su BOM. Imprime el sha256 de cada archivo.

Uso: python -I apply_green_pbt4e_v55.py <dir-copia> --repo <checkout> --rev <rev> [--pieza <pieza>]
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
    spec = importlib.util.spec_from_file_location('green_pbt4e_v55', os.path.join(HERE, 'green_pbt4e_v55.py'))
    gp = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(gp)
    base, green = gp.green(repo, rev, args[args.index('--pieza') + 1] if '--pieza' in args else None)
    for rel in sorted(green):
        path = os.path.join(copy, rel)
        assert rel.startswith('src/'), rel
        raw = open(path, 'rb').read()
        bom = raw.startswith(b'\xef\xbb\xbf')
        text = raw.decode('utf-8-sig')
        crlf = '\r\n' in text
        assert text.replace('\r\n', '\n') == base[rel], ('la copia no es la revision', rel)
        with open(path, 'wb') as f:
            f.write((b'\xef\xbb\xbf' if bom else b'') + (green[rel].replace('\n', '\r\n') if crlf else green[rel]).encode('utf-8'))
        print('green %s %s' % (hashlib.sha256(green[rel].encode('utf-8')).hexdigest(), rel))
    print('GREEN de PB-T4e escrito: %d archivos de src/ cambiados; tests/ sin tocar' % len(green))


if __name__ == '__main__':
    main()
