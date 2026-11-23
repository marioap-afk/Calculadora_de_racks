"""PB-T4e (paquete v5.6): genera las dos piezas literales de la subtarea desde sus fuentes, sobre los objetos git de la base, y las comprueba.

- La pieza del GREEN, `../pbt4e-green-v56.txt` (solo src/): la cabecera de comentarios y los bloques EDIT de `frag/green-src.txt`, en su orden,
  tal cual (sin escapes). Cada OLD tiene que estar exactamente una vez en el texto que se va editando, desde `git show <rev>:<ruta>`.
- La pieza del RED, `../pbt4e-red-v56.txt` (solo tests/): la cabecera y un bloque NEWFILE por cada clase nueva de `ct/`, con su ruta de destino.
  Ninguna ruta nueva existe en la revision. PB-T4e no edita, no salta y no borra ninguna prueba existente (la lista vacia, K = 0).
- Comprueba que ninguna linea de contenido empieza por la marca de bloque y que las dos piezas se leen con green_pbt4e_v56.py y red_pbt4e_v56.py
  (en `..`); escribe el registro con el sha256 de cada pieza y de cada archivo (NewFileSha y el texto del GREEN), del texto con \n.
Este guion es ASCII: el texto no ASCII de las piezas (los mensajes en espanol) solo se lee de sus fuentes, en UTF-8.

Uso (desde v56/): python -I make_pieces_pbt4e_v56.py <repo> <rev> <registro.txt>
"""
import hashlib
import importlib.util
import os
import subprocess
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.normpath(os.path.join(HERE, '..'))
MARK = '@@@@ '
NEW_FILES = [
    ('ct/PushBackEditAllReloadTests.cs', 'tests/RackCad.Tests/PushBackEditAllReloadTests.cs'),
    ('ct/PushBackEditAllReloadFieldTests.cs', 'tests/RackCad.UI.Tests/PushBackEditAllReloadFieldTests.cs'),
]
GREEN_HEAD = [
    '# PB-T4e (I-64 F4B), paquete v5.6: el GREEN literal de src/ (la recarga de Editar todo, D-PBT4-22; desde la v5.6, tras las comprobaciones de la vista, D-S19).',
    '# Bloques EDIT <ruta> / OLD / NEW / END (el OLD esta exactamente una vez en el texto que se va editando; un NEW sin lineas es el texto vacio). Cuatro archivos existentes',
    '# cambian (UNIT_CHANGE): WorkspaceHostCore.cs (el cuerpo de EditAllOfPilot), WorkspacePilotFieldViewModel.cs (la constante del aviso,',
    '# la linea de BeginEdit que la usa, el resumen de EditAll y el aviso de EditAll), WorkspaceEditIntent.cs e IWorkspacePilotEditPort.cs (solo',
    '# comentarios). Ningun archivo nuevo ni borrado; tests/ no se toca.',
    '# Saltos de linea \\n; lo aplica green_pbt4e_v56.py (apply_green_pbt4e_v56.py) sobre 160e019d.',
]
RED_HEAD = [
    '# PB-T4e (I-64 F4B), paquete v5.6: el RED literal de tests/: las dos clases nuevas de las CT (CT-PBT4E-01, 02 y 04 en Core; CT-PBT4E-03',
    '# en UI). Bloques NEWFILE <ruta> / END. Ninguna prueba existente se edita, se salta ni se borra (la lista vacia, K = 0), y src/ no se toca',
    '# (sin esqueleto). Saltos de linea \\n; lo aplica red_pbt4e_v56.py (apply_red_pbt4e_v56.py) sobre 160e019d.',
]


def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


def load(name):
    spec = importlib.util.spec_from_file_location(name, os.path.join(PB4, name + '.py'))
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def show(repo, rev, path):
    raw = subprocess.run(['git', '-C', repo, 'show', rev + ':' + path], capture_output=True, check=True).stdout
    return raw.decode('utf-8-sig').replace('\r\n', '\n')


def exists(repo, rev, path):
    return subprocess.run(['git', '-C', repo, 'cat-file', '-e', rev + ':' + path], capture_output=True).returncode == 0


def main():
    sys.stdout.reconfigure(encoding='utf-8', newline='\n')
    repo, rev, log_path = sys.argv[1:4]
    log = ['# PB-T4e v5.6: make_pieces_pbt4e_v56.py sobre %s (%s)' % (rev, subprocess.run(
        ['git', '-C', repo, 'rev-parse', rev], capture_output=True, text=True, check=True).stdout.strip())]

    # ---- el GREEN
    src = open(os.path.join(HERE, 'frag', 'green-src.txt'), encoding='utf-8').read().replace('\r\n', '\n')
    assert not src.startswith(chr(0xFEFF))
    green_text = '\n'.join(MARK + line for line in GREEN_HEAD) + '\n\n' + src.strip('\n') + '\n'
    green_path = os.path.join(PB4, 'pbt4e-green-v56.txt')
    with open(green_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(green_text)
    gm = load('green_pbt4e_v56')
    edits = gm.blocks(green_path)
    texts = {}
    for _, path, old, new in edits:
        assert path.startswith('src/'), path
        if path not in texts:
            texts[path] = show(repo, rev, path)
        n = texts[path].count(old)
        assert n == 1, ('OLD no esta una vez', path, n)
        texts[path] = texts[path].replace(old, new, 1)
        log.append('green EDIT %s old %d lineas new %d lineas' % (path, old.count('\n'), new.count('\n')))
    base, green = gm.green(repo, rev, green_path)
    assert green == texts
    log.append('pieza-green %s %s (%d bloques EDIT, %d archivos)' % (sha(green_text), os.path.basename(green_path), len(edits), len(green)))
    for path in sorted(green):
        log.append('  green %s %s (base %s)' % (sha(green[path]), path, sha(base[path])))

    # ---- el RED
    parts = ['\n'.join(MARK + line for line in RED_HEAD)]
    for source, target in NEW_FILES:
        text = open(os.path.join(HERE, source), encoding='utf-8').read().replace('\r\n', '\n')
        assert not text.startswith(chr(0xFEFF)) and text.endswith('\n') and not text.endswith('\n\n'), source
        assert not any(line.startswith(MARK) for line in text.split('\n')), source
        assert not exists(repo, rev, target), ('ya existe', target)
        parts.append(MARK + 'NEWFILE ' + target + '\n' + text + MARK + 'END')
    red_text = '\n\n'.join(parts) + '\n'
    red_path = os.path.join(PB4, 'pbt4e-red-v56.txt')
    with open(red_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write(red_text)
    rm = load('red_pbt4e_v56')
    _, red, new = rm.red(repo, rev, red_path)
    assert sorted(new) == sorted(t for _, t in NEW_FILES)
    log.append('pieza-red %s %s (%d bloques NEWFILE)' % (sha(red_text), os.path.basename(red_path), len(new)))
    for source, target in NEW_FILES:
        assert red[target] == open(os.path.join(HERE, source), encoding='utf-8').read().replace('\r\n', '\n')
        log.append('  NewFileSha %s %s' % (sha(red[target]), target))

    # ---- lo que no hay: ningun no ASCII en este guion ni en los modulos y herramientas
    for name in ('make_pieces_pbt4e_v56.py',):
        assert all(ord(c) < 128 for c in open(os.path.join(HERE, name), encoding='utf-8').read()), name
    for name in ('green_pbt4e_v56.py', 'red_pbt4e_v56.py', 'apply_green_pbt4e_v56.py', 'apply_red_pbt4e_v56.py'):
        assert all(ord(c) < 128 for c in open(os.path.join(PB4, name), encoding='utf-8').read()), name
        log.append('  herramienta %s %s' % (sha(open(os.path.join(PB4, name), encoding='utf-8').read().replace('\r\n', '\n')), name))
    with open(log_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n'.join(log) + '\n')
    print('\n'.join(log))


if __name__ == '__main__':
    main()
