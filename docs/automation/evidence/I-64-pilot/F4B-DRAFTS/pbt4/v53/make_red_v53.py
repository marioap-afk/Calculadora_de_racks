"""PB-T4d (paquete v5.3): escribe la pieza literal del RED de tests/, `pbt4d-red-v53.txt`, por objetos git de 6c2820ba.

Desde la v5.2 (make_red_v52.py): las ediciones y las sustitutas son las de pbt4d_test_edits_v53.py y v53/run/ (29 sustitutas, CA-T4d I-01;
M-01); los ayudantes sin uso tras la retirada (CA-T4d M-03) son bajas mas (pbt4d_test_edits_v53.UNUSED), y lo demas no cambia (los guiones
de la v5.2 que se llaman, retire_pbt4d_v52.py y make_fullcopy_eq_v52.py, y las CT y el equivalente de WPPG de v52/, sin tocar).

1. Las bajas con registro: las que la compilacion pide quitar con la retirada B (medido en /d/r6h, r6h-nd/bucle/necesidad.tsv), menos las que se
   quedan editadas (owner-pbt4d-substitutes-decision.md: lo que tambien prueba codigo que se queda conserva esa parte), mas las que compilan y
   solo fijan lo retirado o pasan en vacio (EXTRA_DELETE). Las quita retire_pbt4d_v52.py, sin tocarlo, en seco (--dry): su texto, sin copia.
2. Las ediciones con registro, la llamada de StaleBase y los tres Skip de pbt4d_test_edits_v52.py.
3. Los archivos nuevos: las dos CT (v52/ct/), el equivalente de las dos guardas de WPPG (v52/try/), las 27 sustitutas de WPUR (v52/run/, de
   make_run_eq_v52.py) y el equivalente nuevo de CT-PBT4B-20, que make_fullcopy_eq_v52.py calcula sobre los textos finales de los 21
   originales, sus copias y el kit.
4. La pieza: cada diferencia de un archivo existente como bloque EDIT (un OLD que esta una vez en el texto que se va editando) y cada archivo
   nuevo como NEWFILE; la aplica red_pbt4d_v52.py y se comprueba que da los textos finales.
Escribe la pieza, un arbol con los textos finales (<dir-arbol>) y el registro, en UTF-8 con LF. Nada en el checkout principal.

Uso: python -I make_red_v52.py <repo> <necesidad.tsv> <dir-arbol> <salida-pieza> <salida-registro>
"""
import collections
import difflib
import hashlib
import importlib.util
import os
import re
import shutil
import subprocess
import sys

sys.dont_write_bytecode = True
sys.stdout.reconfigure(encoding='utf-8', newline='\n')
HERE53 = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.dirname(HERE53)
HERE = os.path.join(PB4, 'v52')  # los guiones y las piezas de la v5.2 que no cambian
REPO, NEED, TREE, OUT_PIECE, OUT_LOG = sys.argv[1:6]
REV = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'
MARK = '@@@@ '
W = 'tests/RackCad.Tests/Workspace/'
T = 'tests/RackCad.Tests/'


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


ed = load('pbt4d_test_edits_v53', os.path.join(PB4, 'pbt4d_test_edits_v53.py'))
rp = load('red_pbt4d_v53', os.path.join(PB4, 'red_pbt4d_v53.py'))


def sha(t):
    return hashlib.sha256(t.encode('utf-8')).hexdigest()


def show(path):
    return subprocess.run(['git', '-C', REPO, 'show', REV + ':' + path], capture_output=True, check=True).stdout.decode('utf-8-sig').replace('\r\n', '\n')


# ------------------------------------------------------------------ 1. las bajas
OWNERSHIP = ['MutatingTheObjectOfTheReadAfterBeginChangesNeitherTheInitialStateNorDirty', 'MutatingTheObjectOfTheReadAfterDiscardAndReloadChangesNothing',
             'MutatingTheObjectOfTheReadAfterKeepMineChangesNothing', 'MutatingTheObjectOfThePostReadAfterTheRebaseOfAnAppliedUpdateChangesNothing',
             'MutatingTheObjectPassedToSetCurrentDraftStateAfterTheCallDoesNotChangeTheCurrentState',
             'MutatingTheObjectReturnedByInitialDraftStateDoesNotChangeTheInternalInitialState',
             'ReusingOneObjectOfOriginForTwoDraftsOrTwoSessionsLeavesThemIndependent', 'EditingTheCurrentStateInPlaceLeavesTheDraftDirtyAndTheCloseVetoed', 'Both']
STALE = ['ARereadThatIsNotAdmittedBecauseAMemberBecameBlockingTakesTheUnknownDraftToConflictWithTheDifferences',
         'ARereadThatIsNotAdmittedBecauseAuth13FindsDivergenceTakesTheUnknownDraftToConflictWithTheDifferences', 'ARereadThatIsEqualRecoversTheUnknownDraft',
         'ARereadThatIsNotAdmittedOnlyForTheCatalogStillRecoversBecauseTheBaseIsTheSame', 'UnknownAfterACommitThatThrew']
EDIT_KEEP = {(W + 'WorkspaceDraftPilotOwnershipTests.cs', m) for m in OWNERSHIP}
EDIT_KEEP |= {(W + 'WorkspaceDraftPilotIntentStaleBaseTests.cs', m) for m in STALE}
EDIT_KEEP |= {(T + 'PushBackFullWritePrepareTests.cs', 'CT_PBT3B_05_TheNameOfTheDraftReachesEveryViewAndABlankNameTakesTheOneOfTheOrigin'),
              (T + 'PushBackFullWritePrepareTests.cs', 'CT_PBT3B_06_TheBarrierOfTheDiagnosticsStopsABlockedDesignOfTheFullModelAndOfThePilot'),
              (T + 'PushBackFullWriteRunTests.cs', 'CT_PBT3B_09_ComparePostFullChecksTheErasedKeysTheKeysTheEnvelopesAndTheScaleAndThePilotKeepsItsComparison')}
EXTRA_DELETE = [
    (W + 'WorkspacePilotUpdateGuardTests.cs', 'TheCompositionForWritingComparesTheCurrentStateAsItIsAndNeverCopiesIt', 'solo fija ComposeForWrite (cae al ejecutar)'),
    (W + 'WorkspacePilotUpdateGuardTests.cs', 'TheGuardOfTheCompositionTripsWhenTheCurrentStateIsCopiedBeforeTheComparison', 'solo fija ComposeForWrite (cae al ejecutar)'),
    (W + 'WorkspacePilotUpdateIntegrationGuardTests.cs', 'G4TripsOnACallerInTheControllerTheAdapterOrASecondCallInTheCore',
     'pasa en vacio desde PB-T4b; su autoprueba la lleva el equivalente de G-4 de PB-T4b'),
] + [(W + 'WorkspaceDraftPilotStateTests.cs', m, 'solo fija el comparador del piloto (compila por accidente: Comparer pasa a System.Collections.Comparer)') for m in (
    'AnEditedScaleMakesTheStatesDifferent', 'EveryEditableFieldIsExercisedByTheRichStateAndMutatingItMakesTheStatesDifferent',
    'MutatingEverySelectionFieldTheModuleAuthorityAndTheOriginLeavesTheStatesEqual', 'OriginsWithDifferentTextAndEquivalentContentAreEqual',
    'TheComparatorIsSymmetricAndSeparatesDifferentDesigns', 'TheComparatorTreatsNullAndNaNWithoutBreakingReflexivity')]

need = [l.split('\t') for l in open(NEED, encoding='utf-8').read().rstrip('\n').split('\n')[1:]]
bajas = []
for state, origin, path, member, why in need:
    if state == 'necesaria' and (path, member) not in EDIT_KEEP:
        bajas.append((origin, path, member))
for path, member, why in EXTRA_DELETE:
    bajas.append(('compila; ' + why, path, member))
os.makedirs(TREE, exist_ok=True)
bpath = os.path.join(TREE, '_bajas.tsv')
with open(bpath, 'w', encoding='utf-8', newline='\n') as f:
    f.write('origen\truta\tmiembro\n')
    for o, p, m in bajas:
        f.write('%s\t%s\t%s\n' % (o, p, m))
log = ['# el RED de tests/ de PB-T4d sobre %s' % REV, 'bajas %d (de las necesarias en r6h %d, se quedan editadas %d; compilan y se quitan %d)'
       % (len(bajas), sum(1 for r in need if r[0] == 'necesaria'), len(EDIT_KEEP), len(EXTRA_DELETE))]

# retire_pbt4d_v52.py en seco: su texto de cada archivo con las bajas quitadas (y la retirada de src/, que aqui no se usa)
ns = {'__name__': 'retire_dry', '__file__': os.path.join(HERE, 'retire_pbt4d_v52.py')}
saved = sys.argv
sys.argv = ['retire_pbt4d_v52.py', 'sin-copia', 'B', bpath, os.path.join(TREE, '_retire-dry.txt'), '--repo', REPO, '--dry']
exec(compile(open(os.path.join(HERE, 'retire_pbt4d_v52.py'), encoding='utf-8').read(), 'retire_pbt4d_v52.py', 'exec'), ns)
sys.argv = saved
missing = [l for l in open(os.path.join(TREE, '_retire-dry.txt'), encoding='utf-8').read().split('\n') if l.startswith('NO ENCONTRADO')]
assert not missing, missing
# retire_pbt4d_v52.py puede dejar un archivo sin el salto de linea final (si quita su ultimo miembro y la linea en blanco que lo sigue): se le
# devuelve, para que el texto acabe en \n como el de la base.
texts = {p: (v[0] if v[0].endswith('\n') else v[0] + '\n') for p, v in ns['files'].items() if p.startswith('tests/')}

# ------------------------------------------------------------------ 2. las ediciones, la llamada de StaleBase y los Skip


def get(path):
    if path not in texts:
        texts[path] = show(path)
    return texts[path]


for path, frm, to, new, why in ed.EDITS:
    text = get(path)
    if to is None:
        assert text.count(frm) == 1, ('edicion: el texto no esta una vez', path, frm[:100], text.count(frm))
        texts[path] = text.replace(frm, new, 1)
    else:
        lines = text.split('\n')
        starts = [i for i, l in enumerate(lines) if l + '\n' == frm or (frm.endswith('\n') and l == frm[:-1])]
        assert len(starts) == 1, ('edicion: la linea de inicio no esta una vez', path, frm[:100], len(starts))
        a = starts[0]
        b = next(i for i in range(a + 1, len(lines)) if lines[i] + '\n' == to)
        texts[path] = '\n'.join(lines[:a]) + '\n' + new + '\n'.join(lines[b:])
    log.append('editar %s: %s' % (path, why))
old_call, new_call, times = ed.STALE_CALL
p = W + 'WorkspaceDraftPilotIntentStaleBaseTests.cs'
assert texts[p].count(old_call) == times, (texts[p].count(old_call), times)
texts[p] = texts[p].replace(old_call, new_call)
log.append('editar %s: las %d llamadas al ayudante, con el escaneo' % (p, times))
for path, method, reason in ed.SKIPS:
    text = get(path)
    pat = re.compile(r'^([ \t]*)\[Fact\]\n(\s*public\s+void\s+' + re.escape(method) + r'\s*\()', re.M)
    assert len(pat.findall(text)) == 1, ('Skip: el [Fact] no esta una vez', path, method)
    texts[path] = pat.sub(lambda m: m.group(1) + '[Fact(Skip = "' + reason + '")]\n' + m.group(2), text, count=1)
    log.append('skip %s.%s' % (path, method))

# CA-T4d M-03: los ayudantes que la retirada deja sin uso, despues de las ediciones (los de `edicion` ya los quito WPUR_TAIL)
um = load('unused_v53', os.path.join(HERE53, 'unused_v53.py'))
for path, member, why, nth, how in getattr(ed, 'UNUSED', []):
    text = get(path)
    if how == 'edicion':
        assert not um.spans(text, member), ('M-03: la edicion no lo quito', path, member)
        log.append('sin uso %s.%s: lo quita la edicion de WPUR_TAIL' % (path, member))
        continue
    texts[path], (a, b) = um.remove(text, member, nth)
    log.append('sin uso %s.%s: quitado (lineas %d-%d del texto de entonces)' % (path, member, a, b))

# CA-T4d M-03: los archivos cuya clase la retirada deja vacia (o con solo ayudantes sin uso) se borran enteros (bloques DELETE)
deleted_files = []
for path, why in getattr(ed, 'DELETE_FILES', []):
    show(path)  # existe en la base
    texts.pop(path, None)
    deleted_files.append(path)
    log.append('borrar %s: %s' % (path, why))

# ------------------------------------------------------------------ 3. los archivos nuevos
new_files = collections.OrderedDict()
new_files[T + 'PushBackRetirePilotGuardTests.cs'] = open(os.path.join(HERE, 'ct', 'PushBackRetirePilotGuardTests.cs'), encoding='utf-8').read()
new_files[T + 'PushBackRetirePilotCensusGuardTests.cs'] = open(os.path.join(HERE, 'ct', 'PushBackRetirePilotCensusGuardTests.cs'), encoding='utf-8').read()
new_files[T + 'PushBackRetirePilotPlanGuardTests.cs'] = open(os.path.join(HERE, 'try', 'PushBackRetirePilotPlanGuardTests.cs'), encoding='utf-8').read()
# las 27 sustitutas de WPUR sobre ExecuteFull (owner-pbt4d-run-coverage-decision.md; owner-pbt4d-deadcode-27-decisions.md, pregunta 2), que
# escribe make_run_eq_v52.py; sus originales siguen como bajas de WPUR
new_files[T + 'PushBackRetirePilotRunTests.cs'] = open(os.path.join(HERE53, 'run', 'PushBackRetirePilotRunTests.cs'), encoding='utf-8').read()
# el arbol con los textos finales de lo que lee el equivalente de CT-PBT4B-20 (los 21 originales, sus copias y el kit)
eq_src = show(T + 'PushBackEditAllFullCopyGuardTests.cs')
paths = set(re.findall(r'"(tests/[^"]+\.cs)"', eq_src))
for path in sorted(paths):
    target = os.path.join(TREE, path)
    os.makedirs(os.path.dirname(target), exist_ok=True)
    with open(target, 'w', encoding='utf-8', newline='\n') as f:
        f.write(texts.get(path, show(path)))
eq_out = os.path.join(TREE, '_fullcopy-eq.cs')
r = subprocess.run([sys.executable, '-I', os.path.join(HERE, 'make_fullcopy_eq_v52.py'), REPO, TREE, 'PushBackRetirePilotFullCopyGuardTests', eq_out,
                    os.path.join(TREE, '_fullcopy-eq.txt')], capture_output=True)
assert r.returncode == 0, r.stderr.decode('utf-8', 'replace')
new_files[T + 'PushBackRetirePilotFullCopyGuardTests.cs'] = open(eq_out, encoding='utf-8').read()
log += ['  ' + l for l in open(os.path.join(TREE, '_fullcopy-eq.txt'), encoding='utf-8').read().rstrip('\n').split('\n')]

# ------------------------------------------------------------------ 4. la pieza
piece = [MARK + '# PB-T4d (I-64 F4B), paquete v5.3: el RED literal de tests/ (D-S11): las bajas con registro, las ediciones con registro, los tres',
         MARK + '# Skip (D-S12), la prueba lenta acotada (D-S14), los vacios de M-04 (D-S13), las 29 sustitutas de WPUR',
         MARK + '# (owner-pbt4d-deadcode-27-decisions.md y owner-pbt4d-cat4d-decisions.md), el corpus de CT-PBT3B-06 sobre el modelo completo',
         MARK + '# (CA-T4d M-01), los ayudantes sin uso tras la retirada (CA-T4d M-03) y los archivos nuevos. Bloques EDIT <ruta> / OLD / NEW /',
         MARK + '# END (el OLD esta exactamente una vez en el texto que se va editando; un NEW sin lineas es el texto vacio), DELETE <ruta> (el',
         MARK + '# archivo se borra; desde la v5.3) y NEWFILE <ruta> / END.',
         MARK + '# Saltos de linea \\n; lo aplica red_pbt4d_v53.py sobre 6c2820ba. No toca src/.', '']
count = collections.Counter()
for path in sorted(p for p in texts if texts[p] != show(p)):
    base, final = show(path), texts[path]
    a, b = base.split('\n'), final.split('\n')
    current = base
    ops = difflib.SequenceMatcher(None, a, b, autojunk=False).get_opcodes()
    for idx, (tag, i1, i2, j1, j2) in enumerate(ops):
        if tag == 'equal':
            continue
        if tag == 'delete' and idx + 1 < len(ops) and ops[idx + 1][0] == 'equal':
            limit = ops[idx + 1][2]  # el deslizamiento no sale del tramo igual que sigue
            while i2 < limit and a[i1] == a[i2] and not a[i1].lstrip().startswith('///'):
                i1 += 1
                i2 += 1
        if tag == 'delete' and i2 == len(a) and a[i2 - 1] == '' and i1 > 0 and a[i1 - 1] == '':
            i1 -= 1  # un borrado hasta el final del archivo: se desliza una linea hacia arriba (la ultima vacia es la del salto final)
            i2 -= 1
        old_lines, new_lines = a[i1:i2], b[j1:j2]
        if not old_lines:  # una insercion: con la linea de antes como contexto
            old_lines, new_lines = [a[i1 - 1]], [a[i1 - 1]] + new_lines
        k = i2 if tag != 'insert' else i1
        back = i1 - 1 if tag != 'insert' else i1 - 2
        while True:
            old = '\n'.join(old_lines) + '\n'
            if current.count(old) == 1:
                break
            if k < len(a) - 1:
                old_lines, new_lines = old_lines + [a[k]], new_lines + [a[k]]
                k += 1
            else:  # al final del archivo: el contexto, hacia arriba
                if back < 0:
                    with open(os.path.join(TREE, '_fallo.txt'), 'w', encoding='utf-8', newline='\n') as f:
                        f.write(repr(ops) + '\n' + repr((tag, i1, i2, j1, j2)) + '\n' + repr(old_lines[:5]) + '\n' + current)
                assert back >= 0, ('sin contexto unico', path, i1)
                old_lines, new_lines = [a[back]] + old_lines, [a[back]] + new_lines
                back -= 1
        new = ('\n'.join(new_lines) + '\n') if new_lines else ''
        current = current.replace(old, new, 1)
        piece += [MARK + 'EDIT ' + path, MARK + 'OLD'] + old_lines + [MARK + 'NEW'] + new_lines + [MARK + 'END', '']
        count[path] += 1
    assert current == final, ('la pieza no da el texto final', path)
    log.append('archivo %s: %d bloques; base %s; red %s' % (path, count[path], sha(base), sha(final)))
for path in deleted_files:
    piece += [MARK + 'DELETE ' + path, '']
    log.append('borrado %s (base %s)' % (path, sha(show(path))))
for path, text in new_files.items():
    piece += [MARK + 'NEWFILE ' + path] + text.rstrip('\n').split('\n') + [MARK + 'END', '']
    log.append('nuevo %s %s' % (sha(text), path))
text = '\n'.join(piece).rstrip('\n') + '\n'
with open(OUT_PIECE, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text)
log.append('pieza %s %s (%d lineas)' % (sha(text), os.path.basename(OUT_PIECE), text.count('\n')))
b0, r0, n0, d0 = rp.red_full(REPO, REV, OUT_PIECE)
for path in r0:
    want = new_files[path] if path in n0 else texts[path]
    assert r0[path] == want, ('red_pbt4d_v53.red no da el texto final', path)
assert sorted(n0) == sorted(new_files) and sorted(d0) == sorted(deleted_files)
log.append('comprobado: red_pbt4d_v53.red_full(pieza) da los %d archivos existentes, los %d nuevos y los %d borrados'
           % (len(r0) - len(n0), len(n0), len(d0)))
with open(os.path.join(TREE, '_borrados.txt'), 'w', encoding='utf-8', newline='\n') as f:
    f.write(''.join(p + '\n' for p in deleted_files))
# el arbol final completo, para revisarlo y para las sondas
for path, t in list(texts.items()) + list(new_files.items()):
    target = os.path.join(TREE, path)
    os.makedirs(os.path.dirname(target), exist_ok=True)
    with open(target, 'w', encoding='utf-8', newline='\n') as f:
        f.write(t)
with open(OUT_LOG, 'w', encoding='utf-8', newline='\n') as f:
    f.write('\n'.join(log) + '\n')
print('\n'.join(log[-3:]))
