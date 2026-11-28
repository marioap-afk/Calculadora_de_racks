"""PB-T4d (paquete v5.3): el texto literal del GREEN de src/ (la retirada del piloto, alcance B), construido en memoria desde los objetos git de
la base. PB-T4d no tiene esqueleto: el RED no toca src/.

Lee la pieza literal del paquete, sin escapes, `pbt4d-green-v53.txt`: bloques EDIT (un OLD que esta exactamente una vez en el texto que se va
editando, y su NEW; un NEW sin lineas es el texto vacio) y DELETE (el archivo se borra). Las lineas que empiezan por '@@@@ #' son comentarios.
Trabaja con los saltos de linea a \n. Solo `git show`.

Modulo: green(repo, rev, pieza=None) -> (base, green, borrados); EXISTING (las rutas existentes que cambia el GREEN), DELETED (las que borra);
y blocks(pieza) para leerla.
Uso: python -I green_pbt4d_v53.py <repo> <rev> <salida.txt>   (imprime el sha256 de cada archivo, con \n)
"""
import hashlib
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
GREEN_TXT = os.path.join(HERE, 'pbt4d-green-v53.txt')
MARK = '@@@@ '


def blocks(path_txt):
    """Los bloques de la pieza, en orden: ('EDIT', ruta, old, new) o ('DELETE', ruta)."""
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
            old = '\n'.join(lines[i + 2:j]) + '\n'
            new = ('\n'.join(lines[j + 1:k]) + '\n') if k > j + 1 else ''
            out.append(('EDIT', path, old, new))
            i = k + 1
        elif head.startswith('DELETE '):
            out.append(('DELETE', head[7:]))
            i += 1
        else:
            assert head.startswith('#'), ('marca desconocida', i + 1, line)
            i += 1
    return out


def _show(repo, rev, path):
    raw = subprocess.run(['git', '-C', repo, 'show', rev + ':' + path], capture_output=True, check=True).stdout
    return raw.decode('utf-8-sig').replace('\r\n', '\n')


def green(repo, rev, piece=None):
    base, out, deleted = {}, {}, []
    for item in blocks(piece or GREEN_TXT):
        if item[0] == 'EDIT':
            _, path, old, new = item
            if path not in out:
                base[path] = _show(repo, rev, path)
                out[path] = base[path]
            n = out[path].count(old)
            assert n == 1, ('el OLD no esta exactamente una vez', path, n, old[:120])
            out[path] = out[path].replace(old, new, 1)
        else:
            path = item[1]
            assert path not in out and path not in deleted, path
            base[path] = _show(repo, rev, path)
            deleted.append(path)
    return base, out, deleted


def _paths(piece=None):
    edited, deleted = [], []
    for item in blocks(piece or GREEN_TXT):
        if item[0] == 'EDIT' and item[1] not in edited:
            edited.append(item[1])
        elif item[0] == 'DELETE':
            deleted.append(item[1])
    return edited, deleted


EXISTING, DELETED = _paths() if os.path.exists(GREEN_TXT) else ([], [])


def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    repo, rev, out_path = sys.argv[1:4]
    b, g, d = green(repo, rev)
    lines = ['# PB-T4d v5.3: el GREEN de src/ sobre %s (sha256 del texto con \\n)' % rev,
             'pieza %s pbt4d-green-v53.txt' % sha(open(GREEN_TXT, encoding='utf-8').read().replace('\r\n', '\n'))]
    for p in EXISTING:
        lines.append('green existente %s %s (base %s)' % (sha(g[p]), p, sha(b[p])))
    for p in DELETED:
        lines.append('green borrado %s (base %s)' % (p, sha(b[p])))
    with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(lines) + '\n')
    print('green: %d existentes que cambian y %d que se borran' % (len(EXISTING), len(DELETED)))
