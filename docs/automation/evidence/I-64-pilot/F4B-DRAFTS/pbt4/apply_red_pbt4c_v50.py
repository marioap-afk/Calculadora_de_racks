"""PB-T4c (paquete v5.0): escribe el RED del paquete en una copia exportada de la base, salvo las CT: el esqueleto exacto de src/
(green_pbt4c_v50.skeleton), los cuatro Skip literales (pbt4c_skips_v50.py) y los cuatro equivalentes (make_equivalents_pbt4c_v50.py). Antes
comprueba que cada archivo que toca es el de la revision (por su texto con \n). Los existentes conservan sus saltos de linea; los nuevos van
con \n. Escribe solo dentro de la copia e imprime el sha256 (del texto con \n) de cada archivo que escribe.

Uso: python -I apply_red_pbt4c_v50.py <dir-copia> --repo <checkout> --rev <rev>
"""
import hashlib
import importlib.util
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))


def load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(HERE, name + '.py'))
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


def read(copy, rel):
    raw = open(os.path.join(copy, rel), 'rb').read()
    bom = raw.startswith(b'\xef\xbb\xbf')
    text = raw.decode('utf-8-sig')
    return text.replace('\r\n', '\n'), ('\r\n' in text), bom


def write(copy, rel, text, crlf, bom):
    data = (text.replace('\n', '\r\n') if crlf else text).encode('utf-8')
    with open(os.path.join(copy, rel), 'wb') as f:
        f.write((b'\xef\xbb\xbf' if bom else b'') + data)


def main():
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    copy = sys.argv[1]
    args = sys.argv[2:]
    repo = args[args.index('--repo') + 1]
    rev = args[args.index('--rev') + 1]
    assert os.path.isdir(os.path.join(copy, 'src')) and os.path.isdir(os.path.join(copy, 'tests')), copy
    gp = load('green_pbt4c_v50')
    sk = load('pbt4c_skips_v50')
    eq = load('make_equivalents_pbt4c_v50')

    base, red = gp.skeleton(repo, rev)
    written = []
    for rel in gp.RED_EXISTING:
        text, crlf, bom = read(copy, rel)
        assert text == base[rel], ('la copia no es la revision', rel)
        write(copy, rel, red[rel], crlf, bom)
        written.append(('esqueleto', rel, red[rel]))
    for rel in gp.RED_NEW:
        assert not os.path.exists(os.path.join(copy, rel)), rel
        write(copy, rel, red[rel], False, False)
        written.append(('esqueleto-nuevo', rel, red[rel]))

    for s in sk.SKIPS:
        rel = s[0]
        text, crlf, bom = read(copy, rel)
        assert text == subprocess.run(['git', '-C', repo, 'show', rev + ':' + rel], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n'), rel
        new = sk.apply(text, rel)
        write(copy, rel, new, crlf, bom)
        written.append(('skip', rel, new))

    for rel, text in eq.equivalents(repo, rev).items():
        assert not os.path.exists(os.path.join(copy, rel)), rel
        write(copy, rel, text, False, False)
        written.append(('equivalente', rel, text))

    for kind, rel, text in written:
        print('%s %s %s' % (kind, sha(text), rel))
    print('RED de PB-T4c escrito: %d archivos (esqueleto %d, Skip %d, equivalentes %d), sin las CT'
          % (len(written), len(gp.RED_EXISTING) + len(gp.RED_NEW), len(sk.SKIPS), len(eq.PATHS)))


if __name__ == '__main__':
    main()
