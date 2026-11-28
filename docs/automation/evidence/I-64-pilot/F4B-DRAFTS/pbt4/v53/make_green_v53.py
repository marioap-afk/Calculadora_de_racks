"""PB-T4d (paquete v5.3): escribe `pbt4d-green-v53.txt` desde el GREEN de la v5.2 (`pbt4d-green-v52.txt`, sin cambiar ninguno de sus bloques)
mas los bloques de CA-T4d M-05 (a), al final: solo comentarios del GREEN que describian el piloto como presente (la corrida, el PREPARE y la
composicion para escribir del piloto, que se retiran). Cada OLD se comprueba exactamente una vez sobre el texto que se va editando
(green_pbt4d_v52.green, por objetos git de 6c2820ba); cada bloque nuevo solo toca lineas de comentario (/// o //). Escribe la pieza y el
registro en UTF-8 con LF. Solo lee el repo.

Uso: python -I make_green_v53.py <repo> <pieza-v52> <salida-pieza> <salida-registro>
"""
import hashlib
import importlib.util
import os
import sys

sys.dont_write_bytecode = True
REPO, V52, OUT, LOG = sys.argv[1:5]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
PB4 = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
MARK = '@@@@ '
P = 'src/RackCad.Application/Systems/PushBack/Pilot/'
spec = importlib.util.spec_from_file_location('green_pbt4d_v52', os.path.join(PB4, 'green_pbt4d_v52.py'))
gp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gp)

HEAD_OLD = [
    '@@@@ # PB-T4d (I-64 F4B), paquete v5.2: el GREEN literal de src/, la retirada del piloto con el alcance B (owner-pbt4d-list-decisions.md, pregunta 1),',
    '@@@@ # la limpieza de cuatro cref y un comentario (D-S15, solo comentarios) y, al final, la rama muerta de BuildCore (el parametro',
    '@@@@ # allowErase y lo que solo existe con el; owner-pbt4d-deadcode-27-decisions.md, pregunta 1).',
]
HEAD_NEW = [
    '@@@@ # PB-T4d (I-64 F4B), paquete v5.3: el GREEN literal de src/, la retirada del piloto con el alcance B (owner-pbt4d-list-decisions.md, pregunta 1),',
    '@@@@ # la limpieza de cuatro cref y un comentario (D-S15, solo comentarios), la rama muerta de BuildCore (el parametro allowErase y lo que',
    '@@@@ # solo existe con el; owner-pbt4d-deadcode-27-decisions.md, pregunta 1) y, al final, los comentarios que describian el piloto como',
    '@@@@ # presente (CA-T4d M-05 (a), solo comentarios). Los bloques de la v5.2 no cambian.',
]

# (ruta, old, new): solo comentarios.
M05 = [
    (P + 'PushBackPilotMaterializer.cs',
     '    /// I-64 F4-T2A (D-T2-03, D-T2-04) — materializes the pilot state from the authored envelope text and composes the\n'
     '    /// state to write. It owns NO parser: it reads through the existing RACKEDITAR path (<see cref="RackEmbedStore"/>, the\n',
     '    /// I-64 F4-T2A (D-T2-03, D-T2-04) — materializes the pilot state from the authored envelope text; since I-64 F4B PB-T4d it no\n'
     '    /// longer composes a state to write (that went with the run of the pilot). It owns NO parser: it reads through the existing\n'
     '    /// RACKEDITAR path (<see cref="RackEmbedStore"/>, the\n'),
    (P + 'PushBackPilotUpdateRun.cs',
     '    /// I-64 F4-T4B (D-T4B-02) — the pure and TOTAL run of the update of the bounded Push Back pilot: it decides, classifies and\n',
     '    /// I-64 F4-T4B (D-T4B-02) — the pure and TOTAL run of the update, born for the bounded Push Back pilot and, since I-64 F4B\n'
     '    /// PB-T4d, the one of the full model only (the run of the pilot is retired): it decides, classifies and\n'),
    (P + 'PushBackPilotUpdateRun.cs',
     '    /// I-64 F4B PB-T3b (D-PBT3-10) — the same run serves the two models of the draft: the one of the pilot and the full one. The views that the\n'
     '    /// update leaves obsolete are taken away between the start of MUTATE and its seal, by the capacity that the scope of MUTATE may implement.\n',
     '    /// I-64 F4B PB-T3b (D-PBT3-10) — the same run served the two models of the draft, the one of the pilot and the full one; since I-64 F4B\n'
     '    /// PB-T4d it serves only the full one (ExecuteFull), because the run of the pilot is retired. The views that the update leaves obsolete\n'
     '    /// are taken away between the start of MUTATE and its seal, by the capacity that the scope of MUTATE may implement.\n'),
    (P + 'PushBackPilotUpdateRun.cs',
     '        /// The whole update under the slot for a draft of the FULL model (D-PBT3-10). TOTAL: it never throws. It is the same run as the one of the\n'
     '        /// pilot, with the plan, the re-read and the comparison of the full model.\n',
     '        /// The whole update under the slot for a draft of the FULL model (D-PBT3-10). TOTAL: it never throws. It is the generic run, with the plan,\n'
     '        /// the re-read and the comparison of the full model (the run of the pilot that shared it is retired in I-64 F4B PB-T4d).\n'),
    (P + 'PushBackPilotUpdateRun.cs',
     '        /// <summary>The comparison of POST for the FULL model (D-PBT3-10): the erased views are gone, and the rest as for the pilot.</summary>\n',
     '        /// <summary>The comparison of POST for the FULL model (D-PBT3-10): the erased views are gone, and the keys, the envelopes and the\n'
     '        /// scale are the ones of the plan.</summary>\n'),
    (P + 'PushBackPilotUpdatePlan.cs',
     '    /// What the plan redraws for one member that the pilot may change: the view and the section as RACKEDITAR writes them, the text\n',
     '    /// What the plan redraws for one member that the update may change: the view and the section as RACKEDITAR writes them, the text\n'),
    (P + 'PushBackPilotUpdatePlan.cs',
     '        public IReadOnlyList<PushBackPilotKeyedText> Envelopes { get; }\n\n'
     '        /// <summary>The keys of the views that the update erases, ordered by key (ordinal); empty in the pilot.</summary>\n',
     '        public IReadOnlyList<PushBackPilotKeyedText> Envelopes { get; }\n\n'
     '        /// <summary>The keys of the views that the update erases, ordered by key (ordinal).</summary>\n'),
    (P + 'PushBackPilotUpdatePlan.cs',
     '        public string PostOriginKey { get; }\n\n'
     '        /// <summary>The keys of the views that the update erases, ordered by key (ordinal); empty in the pilot.</summary>\n',
     '        public string PostOriginKey { get; }\n\n'
     '        /// <summary>The keys of the views that the update erases, ordered by key (ordinal).</summary>\n'),
    (P + 'PushBackPilotUpdatePlan.cs',
     '        /// <summary>The members that the pilot may change, ordered by key (ordinal).</summary>\n',
     '        /// <summary>The members that the update may change, ordered by key (ordinal).</summary>\n'),
    (P + 'PushBackPilotUpdatePlan.cs',
     '    /// I-64 F4-T4A1 (D-T4A1-04) — the pure PREPARE of an update of the bounded Push Back pilot. It reads a snapshot of the draft and\n',
     '    /// I-64 F4-T4A1 (D-T4A1-04) — the pure PREPARE of an update, born for the bounded Push Back pilot and, since I-64 F4B PB-T4d, the\n'
     '    /// one of the full model only (the PREPARE of the pilot is retired). It reads a snapshot of the draft and\n'),
    (P + 'PushBackPilotUpdatePlan.cs',
     '        /// The pure PREPARE of an update of the FULL model (D-PBT3-09), with the same order as the one of the pilot: NotApplicable,\n',
     '        /// The pure PREPARE of an update of the FULL model (D-PBT3-09), in the order that the PREPARE of the pilot had (D-T4A1-04; retired\n'
     '        /// in I-64 F4B PB-T4d): NotApplicable,\n'),
]


def sha(t):
    return hashlib.sha256(t.encode('utf-8')).hexdigest()


def comment_only(text):
    for line in text.split('\n'):
        s = line.strip()
        if s and not s.startswith('//'):
            return False
    return True


v52 = open(V52, encoding='utf-8').read()
lines = v52.split('\n')
assert lines[:3] == HEAD_OLD, lines[:3]
body = '\n'.join(HEAD_NEW + lines[3:]).rstrip('\n') + '\n'
add = ['', MARK + '# CA-T4d M-05 (a) (la sesion): solo comentarios, sobre el texto anterior: los que describian como presentes la corrida, el PREPARE y',
       MARK + '# la composicion para escribir del piloto, que se retiran.', '']
for path, old, new in M05:
    # en una linea de contexto que no es comentario (Envelopes, PostOriginKey), el OLD y el NEW la llevan igual
    o, n = old.split('\n'), new.split('\n')
    if not comment_only(old):
        assert o[0] == n[0] and o[1] == n[1] == '', (path, o[:2])
        assert comment_only('\n'.join(o[2:])) and comment_only('\n'.join(n[2:])), path
    else:
        assert comment_only(new), path
    add += [MARK + 'EDIT ' + path, MARK + 'OLD'] + old.rstrip('\n').split('\n') + [MARK + 'NEW'] + new.rstrip('\n').split('\n') + [MARK + 'END', '']
text = body + '\n'.join(add).rstrip('\n') + '\n'
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text)
b52, g52, d52 = gp.green(REPO, REV, V52)
b53, g53, d53 = gp.green(REPO, REV, OUT)
assert d52 == d53 and sorted(g52) == sorted(g53)
log = ['# el GREEN de la v5.3: el de la v5.2 (%s) y %d bloques de solo comentarios (CA-T4d M-05 (a))' % (sha(v52), len(M05)),
       'pieza %s %s (%d lineas)' % (sha(text), os.path.basename(OUT), text.count('\n'))]
for path in sorted(g53):
    a, b = g52[path].split('\n'), g53[path].split('\n')
    code_a = [l for l in a if not l.strip().startswith('//')]
    code_b = [l for l in b if not l.strip().startswith('//')]
    assert code_a == code_b, ('un bloque de M-05 cambia codigo', path)
    log.append('%s: v52 %s -> v53 %s (%d -> %d lineas; el codigo, sin comentarios, igual)' % (path, sha(g52[path])[:16], sha(g53[path])[:16], len(a), len(b)))
with open(LOG, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(log) + '\n')
print('\n'.join(log))
