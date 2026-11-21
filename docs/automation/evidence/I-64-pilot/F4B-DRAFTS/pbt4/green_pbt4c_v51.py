"""PB-T4c (paquete v5.1): el texto literal del GREEN y del esqueleto (RED) de src/, construido en memoria desde los objetos git de la base.

Lee dos piezas literales del paquete, sin escapes: `pbt4c-green-v51.txt` (los bloques EDIT, que sustituyen su OLD exactamente una vez, y
NEWFILE, archivos nuevos enteros) y `pbt4c-skeleton-v51.txt` (los del esqueleto; FROMGREEN toma enteros los del GREEN de una ruta). Trabaja
con los saltos de linea a \n. Solo `git show`.

Modulo: green(repo, rev) -> (base, green); skeleton(repo, rev) -> (base, red); EXISTING (las rutas existentes que cambia el GREEN), NEW (las
nuevas), RED_EXISTING y RED_NEW (las del esqueleto); y blocks(path_txt) para leer una pieza.
Uso: python -I green_pbt4c_v51.py <repo> <rev> <salida.txt>   (imprime el sha256 de cada archivo del GREEN y del esqueleto, con \n)
"""
import hashlib
import os
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
GREEN_TXT = os.path.join(HERE, 'pbt4c-green-v51.txt')
SKELETON_TXT = os.path.join(HERE, 'pbt4c-skeleton-v51.txt')
MARK = '@@@@ '


def blocks(path_txt):
    """Los bloques de una pieza, en orden: ('EDIT', ruta, old, new), ('NEWFILE', ruta, texto) o ('FROMGREEN', ruta)."""
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
            out.append(('EDIT', path, '\n'.join(lines[i + 2:j]) + '\n', '\n'.join(lines[j + 1:k]) + '\n'))
            i = k + 1
        elif head.startswith('NEWFILE '):
            path = head[8:]
            k = lines.index(MARK + 'END', i + 1)
            out.append(('NEWFILE', path, '\n'.join(lines[i + 1:k]) + '\n'))
            i = k + 1
        elif head.startswith('FROMGREEN '):
            out.append(('FROMGREEN', head[10:]))
            i += 1
        else:
            i += 1  # una linea de comentario de la pieza
    return out


def _show(repo, rev, path):
    raw = subprocess.run(['git', '-C', repo, 'show', rev + ':' + path], capture_output=True, check=True).stdout
    assert not raw.startswith(b'\xef\xbb\xbf') or True
    return raw.decode('utf-8-sig').replace('\r\n', '\n')


def _bom(repo, rev, path):
    raw = subprocess.run(['git', '-C', repo, 'show', rev + ':' + path], capture_output=True, check=True).stdout
    return raw.startswith(b'\xef\xbb\xbf')


def _apply(repo, rev, items):
    base, out, new = {}, {}, []
    for item in items:
        if item[0] == 'EDIT':
            _, path, old, newtext = item
            if path not in out:
                base[path] = _show(repo, rev, path)
                out[path] = base[path]
            n = out[path].count(old)
            assert n == 1, ('el OLD no esta exactamente una vez', path, n, old[:120])
            out[path] = out[path].replace(old, newtext, 1)
        elif item[0] == 'NEWFILE':
            _, path, text = item
            assert path not in out, path
            out[path] = text
            new.append(path)
    existing = [p for p in out if p not in new]
    return base, out, existing, new


def _green_items():
    return [b for b in blocks(GREEN_TXT)]


def _skeleton_items():
    green = _green_items()
    items = []
    for b in blocks(SKELETON_TXT):
        if b[0] == 'FROMGREEN':
            taken = [g for g in green if g[1] == b[1]]
            assert taken, b
            items.extend(taken)
        else:
            items.append(b)
    return items


def green(repo, rev):
    base, out, _, _ = _apply(repo, rev, _green_items())
    return base, out


def skeleton(repo, rev):
    base, out, _, _ = _apply(repo, rev, _skeleton_items())
    return base, out


def _paths(items):
    seen_e, seen_n = [], []
    for it in items:
        if it[0] == 'EDIT' and it[1] not in seen_e:
            seen_e.append(it[1])
        elif it[0] == 'NEWFILE':
            seen_n.append(it[1])
    return seen_e, seen_n


EXISTING, NEW = _paths(_green_items())
RED_EXISTING, RED_NEW = _paths(_skeleton_items())


def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


if __name__ == '__main__':
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    repo, rev, out_path = sys.argv[1:4]
    b, g = green(repo, rev)
    bs, s = skeleton(repo, rev)
    lines = ['# PB-T4c v5.1: el GREEN y el esqueleto sobre %s (sha256 del texto con \\n)' % rev,
             'pieza %s pbt4c-green-v51.txt' % sha(open(GREEN_TXT, encoding='utf-8').read().replace('\r\n', '\n')),
             'pieza %s pbt4c-skeleton-v51.txt' % sha(open(SKELETON_TXT, encoding='utf-8').read().replace('\r\n', '\n'))]
    for p in EXISTING + NEW:
        lines.append('green %s %s %s' % ('existente' if p in EXISTING else 'nuevo', sha(g[p]), p))
    for p in RED_EXISTING + RED_NEW:
        lines.append('esqueleto %s %s %s' % ('existente' if p in RED_EXISTING else 'nuevo', sha(s[p]), p))
    for p in EXISTING:
        lines.append('base %s %s (bom=%s)' % (sha(b[p]), p, _bom(repo, rev, p)))
    with open(out_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(lines) + '\n')
    print('green: %d existentes y %d nuevos; esqueleto: %d existentes y %d nuevos' % (len(EXISTING), len(NEW), len(RED_EXISTING), len(RED_NEW)))
