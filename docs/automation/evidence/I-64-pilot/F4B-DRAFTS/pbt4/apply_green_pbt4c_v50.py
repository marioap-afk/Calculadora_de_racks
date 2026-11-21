"""PB-T4c (paquete v5.0): escribe el GREEN literal de src/ (green_pbt4c_v50.green) sobre una copia que ya tiene el RED del paquete
(apply_red_pbt4c_v50.py). Antes comprueba, por su texto con \n, que cada archivo es el del esqueleto (si el esqueleto lo toca) o el de la
revision; tests/ no se toca. Los existentes conservan sus saltos de linea; los nuevos van con \n. Imprime el sha256 de cada archivo.

Uso: python -I apply_green_pbt4c_v50.py <dir-copia> --repo <checkout> --rev <rev>
"""
import hashlib
import importlib.util
import os
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


def main():
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    copy = sys.argv[1]
    args = sys.argv[2:]
    repo = args[args.index('--repo') + 1]
    rev = args[args.index('--rev') + 1]
    gp = load('green_pbt4c_v50')
    base, green = gp.green(repo, rev)
    _, red = gp.skeleton(repo, rev)
    n = 0
    for rel in gp.EXISTING + gp.NEW:
        path = os.path.join(copy, rel)
        expected = red.get(rel, base.get(rel))
        assert rel.startswith('src/'), rel
        crlf, bom = False, False
        if os.path.exists(path):
            raw = open(path, 'rb').read()
            bom = raw.startswith(b'\xef\xbb\xbf')
            text = raw.decode('utf-8-sig')
            crlf = '\r\n' in text
            assert text.replace('\r\n', '\n') == expected, ('la copia no tiene el RED del paquete', rel)
        else:
            assert expected is None, rel
        data = (green[rel].replace('\n', '\r\n') if crlf else green[rel]).encode('utf-8')
        with open(path, 'wb') as f:
            f.write((b'\xef\xbb\xbf' if bom else b'') + data)
        print('green %s %s' % (sha(green[rel]), rel))
        n += 1
    print('GREEN de PB-T4c escrito: %d archivos de src/' % n)


if __name__ == '__main__':
    main()
