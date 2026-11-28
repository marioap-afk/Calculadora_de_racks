"""PB-T4e (paquete v5.5), precondicion 6, el control de las CT: dos piezas parciales del GREEN, sacadas de la pieza literal por sus bloques (nunca
entran en un commit): `control-anfitrion.txt` (solo los bloques de src/RackCad.Application: el anfitrion y sus comentarios) y `control-campo.txt`
(solo los de src/RackCad.UI: el campo). Con la primera deben pasar las CT de Core y caer CT-PBT4E-03; con la segunda, al reves. Asi se ve que cada
CT mira su parte y que ningun oraculo es ciego (AGENTS.md, Guardia de fuente y prueba de comportamiento). Escribe UTF-8 con LF.

Uso: python -I control_pieces_v55.py <pieza-green> <carpeta-salida>
"""
import os
import sys

sys.dont_write_bytecode = True
MARK = '@@@@ '
piece, out_dir = sys.argv[1:3]
text = open(piece, encoding='utf-8').read().replace('\r\n', '\n')
blocks, head, cur = [], [], None
for line in text.split('\n'):
    if line.startswith(MARK + 'EDIT '):
        cur = [line]
        blocks.append(cur)
    elif cur is not None:
        cur.append(line)
        if line == MARK + 'END':
            cur = None
    elif line.startswith(MARK + '#'):
        head.append(line)
for name, prefix in (('control-anfitrion.txt', 'src/RackCad.Application/'), ('control-campo.txt', 'src/RackCad.UI/')):
    chosen = [b for b in blocks if b[0][len(MARK) + 5:].startswith(prefix)]
    assert chosen, name
    body = [MARK + '# control de PB-T4e: solo los bloques de ' + prefix + ' de la pieza ' + os.path.basename(piece)] + \
           ['\n'.join(b) for b in chosen]
    with open(os.path.join(out_dir, name), 'w', encoding='utf-8', newline='\n') as f:
        f.write('\n\n'.join(body) + '\n')
    print('%s: %d bloques' % (name, len(chosen)))
