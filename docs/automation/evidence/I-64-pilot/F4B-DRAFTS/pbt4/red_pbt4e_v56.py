"""PB-T4e (paquete v5.6): el texto literal del RED de tests/ (solo tests/; src/ no cambia en el RED: PB-T4e no tiene esqueleto), construido en
memoria desde los objetos git de la base.

Lee la pieza literal del paquete, sin escapes, `pbt4e-red-v56.txt`: bloques NEWFILE (un archivo nuevo entero). Las lineas que empiezan por
'@@@@ #' son comentarios. Trabaja con los saltos de linea a \n. PB-T4e no edita, no salta y no borra ninguna prueba existente (la lista vacia:
K = 0), asi que la pieza no tiene bloques EDIT ni DELETE y el modulo los rechaza. Es red_pbt4d_v53.py con la pieza de PB-T4e.

Modulo: red(repo, rev, pieza=None) -> (base, red, nuevos); blocks(pieza).
Uso: python -I red_pbt4e_v56.py <repo> <rev> <salida.txt>   (imprime el sha256 de cada archivo, con \n)
"""
import hashlib
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
RED_TXT = os.path.join(HERE, 'pbt4e-red-v56.txt')
MARK = '@@@@ '


def blocks(path_txt):
    lines = open(path_txt, encoding='utf-8').read().replace('\r\n', '\n').split('\n')
    out, i = [], 0
    while i < len(lines):
        line = lines[i]
        if not line.startswith(MARK):
            assert line == '', ('linea fuera de bloque', i + 1, line)
            i += 1
            continue
        head = line[len(MARK):]
        if head.startswith('NEWFILE '):
            path = head[8:]
            k = lines.index(MARK + 'END', i + 1)
            out.append(('NEWFILE', path, '\n'.join(lines[i + 1:k]) + '\n'))
            i = k + 1
        else:
            assert head.startswith('#'), ('marca desconocida o no admitida en PB-T4e (solo NEWFILE)', i + 1, line)
            i += 1
    return out


def _exists(repo, rev, path):
    return subprocess.run(['git', '-C', repo, 'cat-file', '-e', rev + ':' + path], capture_output=True).returncode == 0


def red(repo, rev, piece=None):
    base, out, new = {}, {}, []
    for _, path, text in blocks(piece or RED_TXT):
        assert path.startswith('tests/') and path not in out, path
        assert not _exists(repo, rev, path), ('el archivo nuevo ya existe en la revision', path)
        out[path] = text
        new.append(path)
    return base, out, new


def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    repo, rev, out_path = sys.argv[1:4]
    b, r, n = red(repo, rev)
    lines = ['# PB-T4e v5.6: el RED de tests/ sobre %s (sha256 del texto con \\n)' % rev,
             'pieza %s pbt4e-red-v56.txt' % sha(open(RED_TXT, encoding='utf-8').read().replace('\r\n', '\n'))]
    for p in sorted(r):
        lines.append('red nuevo %s %s' % (sha(r[p]), p))
    with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(lines) + '\n')
    print('red: %d nuevos' % len(n))
