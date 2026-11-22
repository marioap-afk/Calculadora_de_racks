"""PB-T4d (paquete v5.2): escribe la pieza literal del GREEN de src/, `pbt4d-green-v52.txt`, desde la retirada B tal cual se midio.

La fuente es el texto de src/ de la copia de la medida B (`/d/r6e`, que escribio retire_pbt4d_v52.py), comprobado archivo a archivo por el
sha256 de su registro (`r6e-B/bucle/retire-4.txt`): asi la pieza es exactamente lo que el Owner eligio (alcance B), y no una version nueva.
Cada diferencia frente a 6c2820ba (por objetos git) es un borrado de lineas; cada uno va como un bloque EDIT con un OLD que esta exactamente
una vez en el texto que se va editando y un NEW vacio (o con la linea de contexto que haga falta para que el OLD sea unico). Los dos archivos
que se borran van como bloques DELETE. Despues aplica la pieza con green_pbt4d_v52.py y comprueba que da el mismo texto.

Uso: python -I make_green_v52.py <repo> <copia-B> <retire-4.txt> <salida-pieza> <salida-registro>
"""
import difflib
import hashlib
import importlib.util
import os
import subprocess
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
REPO, COPY_B, RETIRE_LOG, OUT_PIECE, OUT_LOG = sys.argv[1:6]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
P = 'src/RackCad.Application/Systems/PushBack/Pilot/'
EDITED = [P + 'PushBackPilotMaterializer.cs', P + 'PushBackPilotUpdatePlan.cs', P + 'PushBackPilotUpdateRun.cs']
DELETED = [P + 'PushBackPilotStateComparer.cs', P + 'PushBackPilotFieldAdapter.cs']
MARK = '@@@@ '


def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()


def show(path):
    raw = subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout
    return raw.decode('utf-8-sig').replace('\r\n', '\n')


def read_copy(path):
    return open(os.path.join(COPY_B, path), 'rb').read().decode('utf-8-sig').replace('\r\n', '\n')


expected = {}
for line in open(RETIRE_LOG, encoding='utf-8').read().split('\n'):
    if line.startswith('escrito ') and ' src/' in line:
        _, digest, path = line.split(' ', 2)
        expected[path] = digest
    if line.startswith('borrado src/'):
        expected[line.split(' ', 1)[1]] = None
assert sorted(p for p, d in expected.items() if d) == sorted(EDITED), expected
assert sorted(p for p, d in expected.items() if d is None) == sorted(DELETED), expected

log = ['# PB-T4d v5.2: la pieza del GREEN de src/ desde la retirada B medida, sobre ' + REV]
piece = [MARK + '# PB-T4d (I-64 F4B), paquete v5.2: el GREEN literal de src/, la retirada del piloto con el alcance B (owner-pbt4d-list-decisions.md, pregunta 1),',
         MARK + '# y la limpieza de cuatro cref y un comentario (D-S15, solo comentarios), al final.',
         MARK + '# Bloques: EDIT <ruta> / OLD / NEW / END (el OLD esta exactamente una vez en el texto que se va editando; un NEW sin lineas es el texto vacio)',
         MARK + '# y DELETE <ruta> (el archivo se borra). Los textos, con saltos de linea \\n. Lo aplica green_pbt4d_v52.py sobre 6c2820ba.',
         '']
for path in DELETED:
    assert not os.path.exists(os.path.join(COPY_B, path)), path
    piece += [MARK + 'DELETE ' + path, '']
    log.append('borrar %s (base %s)' % (path, sha(show(path))))
for path in EDITED:
    base = show(path)
    target = read_copy(path)
    assert sha(target) == expected[path], ('la copia B no es la del registro', path)
    a = base.split('\n')
    b = target.split('\n')
    ops = [op for op in difflib.SequenceMatcher(None, a, b, autojunk=False).get_opcodes() if op[0] != 'equal']
    assert all(op[0] == 'delete' for op in ops), (path, ops)
    current = base
    blocks = 0
    # Los bloques en orden de arriba abajo; cada OLD se busca en el texto ya editado por los anteriores.
    for tag, i1, i2, j1, j2 in ops:
        # Para leerlo mejor: un borrado que empieza con las mismas lineas que lo siguen se desliza hacia abajo, hasta que empieza en el
        # comentario de documentacion o en la declaracion del miembro (el texto que queda es el mismo).
        while i2 < len(a) and a[i1] == a[i2] and not a[i1].lstrip().startswith('///'):
            i1 += 1
            i2 += 1
        old_lines = a[i1:i2]
        new_lines = []
        k = i2
        while True:
            old = '\n'.join(old_lines) + '\n'
            if current.count(old) == 1:
                break
            assert k < len(a), ('sin contexto que haga unico el OLD', path, i1)
            old_lines = old_lines + [a[k]]
            new_lines = new_lines + [a[k]]
            k += 1
        new = ('\n'.join(new_lines) + '\n') if new_lines else ''
        current = current.replace(old, new, 1)
        piece += [MARK + 'EDIT ' + path, MARK + 'OLD'] + old_lines + [MARK + 'NEW'] + new_lines + [MARK + 'END', '']
        blocks += 1
        log.append('editar %s lineas %d-%d de la base (%d borradas, %d de contexto)' % (path, i1 + 1, i2, i2 - i1, len(new_lines)))
    assert current == target, path
    log.append('archivo %s: %d bloques; base %s; green %s' % (path, blocks, sha(base), sha(target)))
# D-S15 (la sesion, P4): solo comentarios. Los cuatro <see cref> a lo retirado y el comentario que se queda sin su Build, sobre el texto B.
CLEANUP = [
    (P + 'PushBackPilotDraftState.cs',
     '    /// operation that assembles a design: a design is only ever assembled from a composition made by\n'
     '    /// <see cref="PushBackPilotMaterializer.ComposeForWrite"/>, never directly over the state of a draft.\n',
     '    /// operation that assembles a design: a design is never assembled directly over the state of a draft.\n'),
    (P + 'PushBackPilotUpdatePlan.cs',
     '    /// I-64 F4B PB-T3b (D-PBT3-09) — the explicit snapshot of a draft of the FULL model that the full PREPARE reads. It is the twin of\n'
     '    /// <see cref="PushBackPilotPrepareInput"/>, which does not change, and it is built by ONE factory, <see cref="From"/>.\n',
     '    /// I-64 F4B PB-T3b (D-PBT3-09) — the explicit snapshot of a draft of the FULL model that the full PREPARE reads. It is built by ONE\n'
     '    /// factory, <see cref="From"/>.\n'),
    (P + 'PushBackPilotUpdatePlan.cs',
     '    /// <summary>The seven outcomes of <see cref="PushBackPilotUpdatePlanner.Prepare"/>, in the order in which they are evaluated.</summary>\n',
     '    /// <summary>The seven outcomes of <see cref="PushBackPilotUpdatePlanner.PrepareFull"/>, in the order in which they are evaluated.</summary>\n'),
    (P + 'PushBackPilotUpdatePlan.cs',
     '    /// <summary>The result of <see cref="PushBackPilotUpdatePlanner.Prepare"/>. Immutable.</summary>\n',
     '    /// <summary>The result of <see cref="PushBackPilotUpdatePlanner.PrepareFull"/>. Immutable.</summary>\n'),
    (P + 'PushBackPilotUpdatePlan.cs',
     '        // The base of the draft and the re-read match here, and the re-read is admitted: the plan can be built, or it is Invalid.\n'
     '\n'
     '        // The same, for the full model: the composition is the one of the model of the draft, and the plan may erase the laterals that the edition leaves obsolete.\n',
     '        // The base of the draft and the re-read match here, and the re-read is admitted: the plan can be built, or it is Invalid. The composition\n'
     '        // is the one of the model of the draft, and the plan may erase the laterals that the edition leaves obsolete.\n'),
]
piece += [MARK + '# D-S15 (la sesion): solo comentarios, sobre el texto anterior: los cuatro cref a lo retirado y el comentario que se queda sin su Build.', '']
final = {p: read_copy(p) for p in EDITED}
for path, old, new in CLEANUP:
    if path not in final:
        final[path] = show(path)
    assert final[path].count(old) == 1, ('limpieza: el OLD no esta una vez', path, old[:80])
    final[path] = final[path].replace(old, new, 1)
    piece += [MARK + 'EDIT ' + path, MARK + 'OLD'] + old.rstrip('\n').split('\n') + [MARK + 'NEW'] + new.rstrip('\n').split('\n') + [MARK + 'END', '']
    log.append('limpieza %s: %d lineas -> %d' % (path, old.count('\n'), new.count('\n')))
EDITED_ALL = EDITED + [p for p in final if p not in EDITED]
text = '\n'.join(piece).rstrip('\n') + '\n'
with open(OUT_PIECE, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text)
log.append('pieza %s %s' % (sha(text), os.path.basename(OUT_PIECE)))

spec = importlib.util.spec_from_file_location('green_pbt4d_v52', os.path.join(os.path.dirname(HERE), 'green_pbt4d_v52.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)
base_g, green_g, deleted_g = gp.green(REPO, REV, OUT_PIECE)
assert sorted(deleted_g) == sorted(DELETED)
assert sorted(green_g) == sorted(EDITED_ALL), sorted(green_g)
for path in EDITED_ALL:
    assert green_g[path] == final[path], ('la pieza aplicada no da el texto B con la limpieza', path)
    log.append('green %s %s' % (sha(green_g[path]), path))
log.append('comprobado: green_pbt4d_v52.green(pieza) da el texto de la medida B con la limpieza de D-S15 en los %d archivos y borra los 2' % len(EDITED_ALL))
with open(OUT_LOG, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(log) + '\n')
print(log[-2])
print(log[-1])
