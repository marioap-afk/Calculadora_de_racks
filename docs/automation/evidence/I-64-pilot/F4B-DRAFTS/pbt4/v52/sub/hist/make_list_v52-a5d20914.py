"""PB-T4d (paquete v5.2): la lista B fila a fila, `pbt4d-retire-list-<n>.tsv`, con la accion de cada fila segun las decisiones del Owner
(owner-pbt4d-list-decisions.md y owner-pbt4d-substitutes-decision.md) y de la sesion (D-S11 a D-S15).

Las filas: las 235 bajas medidas de la variante B (r6e-B/bucle/bajas-4.tsv), las 3 que caen al ejecutar, los 3 vacios de M-04 y la prueba lenta,
y las sustitutas nuevas de WPUR si las hay (WPUR_SUBST). La accion sale del RED de make_red_v52.py (que se lee, no se ejecuta):
  - `borrar con registro`: la baja va en el RED;
  - `reescribir con registro`: la prueba o el ayudante se queda, editado en su sitio (pbt4d_test_edits_v52.py);
  - `sustituir con registro`: Skip y equivalente (D-S12);
  - `acotar con registro`: la prueba lenta (D-S14);
  - `se queda`: compila y pasa sobre el modelo completo (KEPT); no se emite.
n es el numero de filas con accion (todas menos las que se quedan). Las lineas, en 6c2820ba. Escribe UTF-8 con LF e imprime el sha256.

Uso: python -I make_list_v52.py <repo> <dir-compile> <necesidad.tsv> <dir-salida>
"""
import collections
import hashlib
import importlib.util
import os
import re
import sys

sys.dont_write_bytecode = True
HERE = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.dirname(HERE)
REPO, CC, NEED, OUTDIR = sys.argv[1:5]


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


sp = load('spans_v52', os.path.join(HERE, 'spans_v52.py'))
ed = load('pbt4d_test_edits_v52', os.path.join(PB4, 'pbt4d_test_edits_v52.py'))
# las tablas de make_red_v52.py (EDIT_KEEP, EXTRA_DELETE), sin ejecutar su parte que escribe
src = open(os.path.join(HERE, 'make_red_v52.py'), encoding='utf-8').read()
ns = {'__name__': 'make_red_tables', 'W': 'tests/RackCad.Tests/Workspace/', 'T': 'tests/RackCad.Tests/'}
exec(compile(src[src.index('# ------------------------------------------------------------------ 1. las bajas'):src.index('need = [l.split')], 'make_red_v52.py', 'exec'), ns)
EDIT_KEEP, EXTRA_DELETE = ns['EDIT_KEEP'], ns['EXTRA_DELETE']
WPUR_SUBST = getattr(ed, 'WPUR_SUBST', [])


def tsv(path):
    return [l.split('\t') for l in open(path, encoding='utf-8').read().rstrip('\n').split('\n')[1:] if l.strip()]


b_rows = tsv(os.path.join(CC, 'r6e-B', 'bucle', 'bajas-4.tsv'))
a_keys = {(r[1], r[2]) for r in tsv(os.path.join(CC, 'r6d-A', 'bucle', 'bajas-3.tsv'))}
need = {(r[2], r[3]): r[0] for r in tsv(NEED) if r[0] != 'fuera-de-B'}
kept = set(ed.KEPT)
extra = {(p, m): why for p, m, why in EXTRA_DELETE}
old = {(r[1].split('/')[-1], r[2]): r[4] for r in tsv(os.path.join(PB4, 'pbt4d-retire-list-138.tsv'))}
remeasured = {(r[1].split('/')[-1], r[2]): (r[4], r[7]) for r in tsv(os.path.join(HERE, 'pbt4d-remeasure-v52.tsv'))
              if r[0] in ('PB-T4d C-7', 'PB-T4d apoyo', 'PB-T4d original') and r[4] not in ('', '-')}
edit_why = collections.defaultdict(list)
for path, frm, to, new, why in ed.EDITS:
    edit_why[path].append(why)

out = ['n\tgrupo\tarchivo\tmiembro\tlineas-6c2820ba\tskip-6c2820ba\taccion\tporque\ten-A\tnecesidad-r6h']
rows = 0
count = collections.Counter()


def add(group, path, member, lines, skip, action, why, in_a, necesidad):
    global rows
    rows += 1
    count[(group, action)] += 1
    out.append('\t'.join([str(rows), group, path, member, lines, skip, action, why.replace('\t', ' '), in_a, necesidad]))


for origin, path, member in b_rows:
    group = ('C-7' if 'C-7' in origin else 'original' if 'original' in origin else 'apoyo' if 'apoyo' in origin else 'SUS-01' if 'SUS-01' in origin else 'compilacion')
    key = (path.split('/')[-1], member)
    if key in remeasured and group != 'compilacion':
        lines, skip = remeasured[key]
        skip = 'PB-T4b' if 'PB-T4b' in skip else ('-' if skip in ('', '-') else skip)
    else:
        a, b, skip = sp.span(REPO, path, member)
        lines = '%d-%d' % (a, b)
    nec = need.get((path, member), '-')
    base_why = old.get(key, '') if group in ('C-7', 'original', 'apoyo') else (origin.split(': ', 1)[0] + ' (medido en /d/r6e)' if group == 'compilacion' else
                                                                               'Theory de UI saltada desde PB-TC (SUS-01: la ejecuta CT-PBTC-06)')
    if (path, member) in kept:
        action, why = 'se queda', 'compila y pasa sobre el modelo completo (medido en /d/r6h; owner-pbt4d-substitutes-decision.md)'
    elif (path, member) in EDIT_KEEP:
        action, why = 'reescribir con registro', 'tambien prueba codigo que se queda: ' + '; '.join(edit_why[path])
    elif (path, member) in extra:
        action, why = 'borrar con registro', extra[(path, member)] + ' (medido en /d/r6h)'
    elif nec == 'no-necesaria':
        # compila sin quitarla y prueba algo que se queda: se edita en su sitio (pbt4d_test_edits_v52.py)
        action, why = 'reescribir con registro', 'compila sin quitarla y tambien prueba codigo que se queda: ' + '; '.join(edit_why[path])
    else:
        action = 'borrar con registro'
        why = {'C-7': 'solo fija lo retirado: ', 'original': 'original saltada desde PB-T4b (su copia o su equivalente corre desde entonces): ',
               'apoyo': 'ayudante que nombra lo retirado: ', 'SUS-01': '', 'compilacion': 'deja de compilar con la retirada B: '}[group] + base_why
    add(group, path, member, lines, skip, action, why, 'si' if (path, member) in a_keys or group == 'C-7' else 'no', nec)

W = 'tests/RackCad.Tests/Workspace/'
EXTRA_ROWS = [
    ('ejecucion', 'tests/RackCad.Tests/PushBackEditAllFullCopyGuardTests.cs', 'CT_PBT4B_20_TheScriptOfSkipAndDuplicateGivesTheCopiesTheKitAndTheSkipsOfTheClosedList',
     'sustituir con registro', 'el equivalente de CT-PBT4B-20 de PB-T4c cae al ejecutar: cambian los BaseSha, los Skip y los recuentos de los originales y las copias que PB-T4d edita; su equivalente, PushBackRetirePilotFullCopyGuardTests (D-S12)'),
    ('ejecucion', W + 'WorkspacePilotPrepareGuardTests.cs', 'TheFileOfThePlanIsNullableEnabledInTheNamespaceOfThePilotWithBracesAndDeclaresTheTypesOfThePackage',
     'sustituir con registro', 'cae al ejecutar: exige PushBackPilotPrepareInput y su From, que B retira; su equivalente, en PushBackRetirePilotPlanGuardTests (D-S12)'),
    ('ejecucion', W + 'WorkspacePilotPrepareGuardTests.cs', 'TheOrderOfPrepareIsTheOneOfThePackageAndNoOutcomeBeforeConflictLooksAtTheReread',
     'sustituir con registro', 'cae al ejecutar: lee el cuerpo del Prepare del piloto; su equivalente lee el de PrepareFull, en PushBackRetirePilotPlanGuardTests (D-S12)'),
    ('M-04', W + 'WorkspacePilotEditGuardTests.cs', 'TheReadGuardAllowsAnotherReadThatALaterTaskWouldAddWithItsOwnMark',
     'reescribir con registro', 'pasa en vacio desde PB-T4b (owner-pbt4b-code-decisions.md, pregunta 2): su Replace busca FindPilotDraft con la firma del piloto (D-S13)'),
    ('M-04', W + 'WorkspacePilotEditGuardTests.cs', 'TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing',
     'reescribir con registro', 'pasa en vacio para FindPilotDraft desde PB-T4b: su ayudante WriteViolations (PEG:906) busca la firma del piloto (D-S13)'),
    ('M-04', W + 'WorkspacePilotEditGuardFull.cs', 'WriteViolations',
     'reescribir con registro', 'la copia de PB-T4b hereda el vacio en :255-258 (D-S13)'),
    ('lenta', W + 'WorkspacePilotPrepareFull.cs', 'InvalidD_ABuilderThatCannotBuildTheOutgoingSystemIsInvalidWithTheTypedErrorAndNothingIsWritten',
     'acotar con registro', 'un PREPARE por cada uno de los 4 936 toques del catalogo, unos 6,5 min (OWNER-05; owner-pbt4d-list-decisions.md, pregunta 3); cada 97.o toque y el ultimo (D-S14)'),
] + [tuple(r) for r in WPUR_SUBST]
for group, path, member, action, why in EXTRA_ROWS:
    a, b, skip = sp.span(REPO, path, member)
    if member == 'WriteViolations':
        a, b = 255, 258
    add(group, path, member, '%d-%d' % (a, b), skip, action, why, '-', '-')
n = sum(c for (g, act), c in count.items() if act != 'se queda')
text = '\n'.join(out) + '\n'
os.makedirs(OUTDIR, exist_ok=True)
target = os.path.join(OUTDIR, 'pbt4d-retire-list-%d.tsv' % n)
with open(target, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text)
print('filas %d, con accion %d; %s' % (rows, n, os.path.basename(target)))
print('sha256 %s' % hashlib.sha256(text.encode('utf-8')).hexdigest())
for (g, act), c in sorted(count.items()):
    print('  %s | %s | %d' % (g, act, c))
