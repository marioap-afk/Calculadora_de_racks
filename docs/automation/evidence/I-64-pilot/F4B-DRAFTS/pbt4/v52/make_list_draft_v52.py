"""PB-T4d (paquete v5.2): el BORRADOR de la lista B fila a fila, con la accion que decidio el Owner (owner-pbt4d-list-decisions.md) y, al lado,
la medida de necesidad de /d/r6h (que fila hace falta quitar para compilar y como sale al ejecutar). No es la pieza del paquete: la lista que se
emite depende de las respuestas de la sesion (si se emite una parte, y que sustituye a cada una).

Las filas: las 235 bajas de la variante B (r6e-B/bucle/bajas-4.tsv: 138 C-7, 55 originales, 29 de apoyo, SUS-01 y 12 por compilacion), las 3
que caen al ejecutar, los 3 vacios de M-04 y la prueba lenta. Las lineas, en 6c2820ba (spans_v52.py, por objetos git). La columna en-A dice si la
fila estaba en la variante A (r6d-A/bucle/bajas-3.tsv). Escribe UTF-8 con LF.

Uso: python -I make_list_draft_v52.py <repo> <dir-compile> <salida.tsv>
"""
import collections
import hashlib
import importlib.util
import os
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
spec = importlib.util.spec_from_file_location('spans_v52', os.path.join(HERE, 'spans_v52.py'))
sp = importlib.util.module_from_spec(spec)
spec.loader.exec_module(sp)
REPO, CC, OUT = sys.argv[1:4]
W = 'tests/RackCad.Tests/Workspace/'


def tsv(path):
    return [l.split('\t') for l in open(path, encoding='utf-8').read().rstrip('\n').split('\n')[1:] if l.strip()]


b_rows = tsv(os.path.join(CC, 'r6e-B', 'bucle', 'bajas-4.tsv'))
a_keys = {(r[1], r[2]) for r in tsv(os.path.join(CC, 'r6d-A', 'bucle', 'bajas-3.tsv'))}
need = {(r[2], r[3]): r[0] for r in tsv(os.path.join(CC, 'r6h-nd', 'bucle', 'necesidad.tsv')) if r[0] != 'fuera-de-B'}
run = {}
rep = os.path.join(HERE, 'need-report-r6h.tsv')
if os.path.exists(rep):
    run = {(r[2], r[3]): r[0] for r in tsv(rep)}
old = {}
for r in tsv(os.path.join(os.path.dirname(HERE), 'pbt4d-retire-list-138.tsv')):
    old[(r[1].split('/')[-1], r[2])] = (r[0], r[4])
# las lineas en 6c2820ba de las 222 filas de la lista de la v4.4, de la remedicion (pbt4d-remeasure-v52.tsv); el resto, por spans_v52.py
remeasured = {}
for r in tsv(os.path.join(HERE, 'pbt4d-remeasure-v52.tsv')):
    if r[0] in ('PB-T4d C-7', 'PB-T4d apoyo', 'PB-T4d original') and r[4] not in ('', '-'):
        remeasured[(r[1].split('/')[-1], r[2])] = (r[4], r[7])

ACTION = {
    'C-7': ('sustituir con registro', 'C-7: fija el piloto por su letra'),
    'original': ('borrar con registro', 'original de las 9 clases, saltada desde PB-T4b (su copia o su equivalente corre desde entonces), que deja de compilar al retirar'),
    'apoyo': ('borrar con registro', 'miembro de apoyo que nombra lo retirado'),
    'SUS-01': ('borrar con registro', 'Theory de UI saltada desde PB-TC (SUS-01: la ejecuta CT-PBTC-06), que deja de compilar al retirar'),
    'compilacion': ('borrar con registro', 'deja de compilar con la retirada B (medido en /d/r6e)'),
}
out = ['n\tgrupo\tarchivo\tmiembro\tlineas-6c2820ba\tskip-6c2820ba\taccion\tporque\ten-A\tnecesidad-r6h\tejecucion-r6h']
n = 0
count = collections.Counter()
for origin, path, member in b_rows:
    if origin.startswith('lista PB-T4d C-7'):
        group = 'C-7'
    elif origin.startswith('lista PB-T4d original'):
        group = 'original'
    elif origin.startswith('lista PB-T4d apoyo'):
        group = 'apoyo'
    elif origin.startswith('SUS-01'):
        group = 'SUS-01'
    else:
        group = 'compilacion'
    key = (path.split('/')[-1], member)
    if key in remeasured and group != 'compilacion':
        lines, skip = remeasured[key]
        skip = 'PB-T4b' if 'PB-T4b' in skip else ('-' if skip in ('', '-') else skip)
    else:
        a, b, skip = sp.span(REPO, path, member)
        lines = '%d-%d' % (a, b)
    action, why = ACTION[group]
    if group in ('C-7', 'original', 'apoyo'):
        why = old.get((path.split('/')[-1], member), ('', why))[1] or why
    elif group == 'compilacion':
        why = why + '; ' + origin.split(': ', 1)[0]
    n += 1
    count[group] += 1
    out.append('\t'.join([str(n), group, path, member, lines, skip, action, why, 'si' if (path, member) in a_keys else 'no',
                          need.get((path, member), '-'), run.get((path, member), '-')]))
EXTRA = [
    ('ejecucion', 'tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs', 'CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList',
     'sustituir con registro', 'el equivalente de CT-PBT4B-20 de PB-T4c: los BaseSha, los skip y los recuentos de los originales y las copias que PB-T4d edita dejan de casar'),
    ('ejecucion', W + 'WorkspacePilotPrepareGuardTests.cs', 'TheFileOfThePlanIsNullableEnabledInTheNamespaceOfThePilotWithBracesAndDeclaresTheTypesOfThePackage',
     'sustituir con registro', 'exige PushBackPilotPrepareInput y su From, que B retira'),
    ('ejecucion', W + 'WorkspacePilotPrepareGuardTests.cs', 'TheOrderOfPrepareIsTheOneOfThePackageAndNoOutcomeBeforeConflictLooksAtTheReread',
     'sustituir con registro', 'lee el cuerpo del Prepare del piloto, que se retira (BodyOf no lo encuentra)'),
    ('M-04', W + 'WorkspacePilotEditGuardTests.cs', 'TheReadGuardAllowsAnotherReadThatALaterTaskWouldAddWithItsOwnMark',
     'reescribir', 'pasa en vacio desde PB-T4b: su Replace busca FindPilotDraft con la firma del piloto (owner-pbt4b-code-decisions.md, pregunta 2)'),
    ('M-04', W + 'WorkspacePilotEditGuardTests.cs', 'TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing',
     'reescribir', 'pasa en vacio para FindPilotDraft desde PB-T4b: su ayudante WriteViolations busca la firma del piloto (owner-pbt4b-code-decisions.md, pregunta 2)'),
    ('M-04', W + 'WorkspacePilotEditGuardFull.cs', 'WriteViolations',
     'reescribir', 'la copia de PB-T4b hereda el vacio en :255-258 (owner-pbt4b-code-decisions.md, pregunta 2)'),
    ('lenta', W + 'WorkspacePilotPrepareFull.cs', 'InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten',
     'acotar', 'su bucle hace un PREPARE por cada uno de los 4 936 toques del catalogo (unos 6,5 min; OWNER-05; owner-pbt4d-list-decisions.md, pregunta 3)'),
]
for group, path, member, action, why in EXTRA:
    a, b, skip = sp.span(REPO, path, member)
    if member == 'WriteViolations':
        a, b = 255, 258
    n += 1
    count[group] += 1
    out.append('\t'.join([str(n), group, path, member, '%d-%d' % (a, b), skip, action, why, 'si', '-', '-']))
text = '\n'.join(out) + '\n'
with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text)
print('filas %d %s sha256 %s' % (n, dict(count), hashlib.sha256(text.encode('utf-8')).hexdigest()))
