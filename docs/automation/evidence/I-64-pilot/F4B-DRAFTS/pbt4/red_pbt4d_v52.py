"""PB-T4d (paquete v5.2): el texto literal del RED de tests/ (D-S11: el RED lleva todo lo de tests/ y no toca src/), construido en memoria desde
los objetos git de la base.

Lee la pieza literal del paquete, sin escapes, `pbt4d-red-v52.txt`: bloques EDIT (un OLD que esta exactamente una vez en el texto que se va
editando, y su NEW; un NEW sin lineas es el texto vacio) y NEWFILE (un archivo nuevo entero). Las lineas que empiezan por '@@@@ #' son
comentarios. Trabaja con los saltos de linea a \n. Solo `git show`.

Modulo: red(repo, rev, pieza=None) -> (base, red, nuevos); blocks(pieza).
Uso: python -I red_pbt4d_v52.py <repo> <rev> <salida.txt>   (imprime el sha256 de cada archivo, con \n)
"""
import hashlib
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
RED_TXT = os.path.join(HERE, 'pbt4d-red-v52.txt')
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
        if head.startswith('EDIT '):
            path = head[5:]
            assert lines[i + 1] == MARK + 'OLD', (i + 2, lines[i + 1])
            j = lines.index(MARK + 'NEW', i + 2)
            k = lines.index(MARK + 'END', j + 1)
            out.append(('EDIT', path, '\n'.join(lines[i + 2:j]) + '\n', ('\n'.join(lines[j + 1:k]) + '\n') if k > j + 1 else ''))
            i = k + 1
        elif head.startswith('NEWFILE '):
            path = head[8:]
            k = lines.index(MARK + 'END', i + 1)
            out.append(('NEWFILE', path, '\n'.join(lines[i + 1:k]) + '\n'))
            i = k + 1
        else:
            assert head.startswith('#'), ('marca desconocida', i + 1, line)
            i += 1
    return out


def _show(repo, rev, path):
    raw = subprocess.run(['git', '-C', repo, 'show', rev + ':' + path], capture_output=True, check=True).stdout
    return raw.decode('utf-8-sig').replace('\r\n', '\n')


def red(repo, rev, piece=None):
    base, out, new = {}, {}, []
    for item in blocks(piece or RED_TXT):
        if item[0] == 'EDIT':
            _, path, old, newtext = item
            assert path.startswith('tests/'), path
            if path not in out:
                base[path] = _show(repo, rev, path)
                out[path] = base[path]
            n = out[path].count(old)
            assert n == 1, ('el OLD no esta exactamente una vez', path, n, old[:120])
            out[path] = out[path].replace(old, newtext, 1)
        else:
            _, path, text = item
            assert path.startswith('tests/') and path not in out, path
            out[path] = text
            new.append(path)
    return base, out, new


def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    repo, rev, out_path = sys.argv[1:4]
    b, r, n = red(repo, rev)
    lines = ['# PB-T4d v5.2: el RED de tests/ sobre %s (sha256 del texto con \\n)' % rev,
             'pieza %s pbt4d-red-v52.txt' % sha(open(RED_TXT, encoding='utf-8').read().replace('\r\n', '\n'))]
    for p in sorted(r):
        lines.append('red %s %s %s' % ('nuevo' if p in n else 'existente', sha(r[p]), p) + ('' if p in n else ' (base %s)' % sha(b[p])))
    with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(lines) + '\n')
    print('red: %d existentes que cambian y %d nuevos' % (len(r) - len(n), len(n)))
