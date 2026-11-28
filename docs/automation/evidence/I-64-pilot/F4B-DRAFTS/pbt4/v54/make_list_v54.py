"""PB-T4d (paquete v5.2): la lista B fila a fila, `pbt4d-retire-list-<n>.tsv`, con la accion de cada fila segun las decisiones del Owner
(owner-pbt4d-list-decisions.md y owner-pbt4d-substitutes-decision.md) y de la sesion (D-S11 a D-S15).

Las filas: las 235 bajas medidas de la variante B (r6e-B/bucle/bajas-4.tsv), las 3 que caen al ejecutar, los 3 vacios de M-04 y la prueba lenta.
La accion sale del RED de make_red_v52.py (que se lee, no se ejecuta):
  - `borrar con registro`: la baja va en el RED;
  - `reescribir con registro`: la prueba o el ayudante se queda, editado en su sitio (pbt4d_test_edits_v52.py);
  - `sustituir con registro`: Skip y equivalente (D-S12), o, en WPUR, baja con su equivalente sobre ExecuteFull en PushBackRetirePilotRunTests
    (las 27 de WPUR_SUBST y su ayudante; owner-pbt4d-deadcode-27-decisions.md, pregunta 2);
  - `acotar con registro`: la prueba lenta (D-S14);
  - `se queda`: compila y pasa sobre el modelo completo (KEPT); no se emite.
n es el numero de filas con accion (todas menos las que se quedan). Las lineas, en 6c2820ba. Escribe UTF-8 con LF e imprime el sha256.

Desde la v5.3 (CA-T4d):
  - I-01: la 48 y la 49 de WPUR pasan a `sustituir con registro`, con su porque (prueban PushBackPilotSystemRebuild, que se queda);
  - M-03: los ayudantes que la retirada deja sin uso (pbt4d_test_edits_v54.UNUSED), `borrar con registro (ayudante sin uso tras la retirada)`;
  - M-04: dos columnas nuevas, `declaraciones-6c2820ba` (todas las declaraciones del nombre en su archivo: las sobrecargas) y `tramo-editado`
    (las lineas de la base que el RED cambia dentro de ellas, medidas contra el arbol final del RED); una fila por cada ayudante que el RED
    edita sin fila propia; y los porque de C-7 dicen lo que se hace (borrar), no `sustituir`. Comprueba que toda linea de codigo que el RED
    cambia en un archivo existente cae en alguna fila (lo que no, PARA).

Uso: python -I make_list_v53.py <repo> <dir-compile> <necesidad.tsv> <arbol-red> <dir-salida>
"""
import collections
import difflib
import hashlib
import importlib.util
import os
import re
import sys

sys.dont_write_bytecode = True
HERE54 = os.path.dirname(os.path.abspath(__file__))
PB4 = os.path.dirname(HERE54)
HERE53 = os.path.join(PB4, "v53")
HERE = os.path.join(PB4, 'v52')
REPO, CC, NEED, REDTREE, OUTDIR = sys.argv[1:6]


def load(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    mod = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(mod)
    return mod


sp = load('spans_v52', os.path.join(HERE, 'spans_v52.py'))
ed = load('pbt4d_test_edits_v54', os.path.join(PB4, 'pbt4d_test_edits_v54.py'))
lc = sp.lc
# las tablas de make_red_v54.py (EDIT_KEEP, EXTRA_DELETE), sin ejecutar su parte que escribe
src = open(os.path.join(HERE54, 'make_red_v54.py'), encoding='utf-8').read()
ns = {'__name__': 'make_red_tables', 'W': 'tests/RackCad.Tests/Workspace/', 'T': 'tests/RackCad.Tests/'}
exec(compile(src[src.index('# ------------------------------------------------------------------ 1. las bajas'):src.index('need = [l.split')], 'make_red_v54.py', 'exec'), ns)
EDIT_KEEP, EXTRA_DELETE = ns['EDIT_KEEP'], ns['EXTRA_DELETE']
UNUSED = getattr(ed, 'UNUSED', [])
# Las 29 sustitutas de WPUR y su ayudante (pbt4d_test_edits_v54.WPUR_SUBST), cruzadas con los metodos de PushBackRetirePilotRunTests.cs.
WPUR = 'tests/RackCad.Tests/Workspace/WorkspacePilotUpdateRunTests.cs'
WPUR_SUBST = ed.WPUR_SUBST
run_text = open(os.path.join(HERE53, 'run', 'PushBackRetirePilotRunTests.cs'), encoding='utf-8').read()
run_tests = set(re.findall(r'^    public void (\w+)\(', run_text, re.M))
assert run_tests == {m for m, (num, how) in WPUR_SUBST.items() if num != 'ayudante'}, sorted(run_tests ^ set(WPUR_SUBST))
assert len(run_tests) == 29 and re.search(r'^    private static DraftStatus StatusAfter\(', run_text, re.M)
SUBST_WHY = ('tambien prueba la corrida generica que se queda (lineas o ramas que solo ella cubre: v52/cov-tests-r6k.txt y v52/sub/cov-sides.txt); '
             'su equivalente sobre ExecuteFull, en PushBackRetirePilotRunTests (owner-pbt4d-run-coverage-decision.md; owner-pbt4d-deadcode-27-decisions.md, pregunta 2)')
REBUILD_WHY = ('no prueba la corrida: prueba PushBackPilotSystemRebuild (Build, BuildForMember e IsComposite), que se queda (lo llaman DesiredNamesOf y '
               'el escritor real de Actualizar); es la unica prueba de una linea y 6 ramas suyas (CA-T4d I-01); su equivalente, sobre el plan del PREPARE '
               'completo, en PushBackRetirePilotRunTests (owner-pbt4d-cat4d-decisions.md, pregunta 1)')
WPUR_DELETE_WHY = ('usa la corrida del piloto como vehiculo; lo que ejercita de codigo que se queda lo cubren otras pruebas que se quedan, medido '
                   'linea a linea y rama por rama sobre todo src/ (owner-pbt4d-run-coverage-decision.md, owner-pbt4d-deadcode-27-decisions.md y '
                   'owner-pbt4d-cat4d-decisions.md: las otras se borran con registro; la cobertura ampliada de la precondicion 6 lo mide)')


# CA-T4d M-04 (a): las filas de un nombre con sobrecargas que el RED borra juntas (la baja quita todas las del nombre), con su porque de cada una
WPP = 'tests/RackCad.Tests/Workspace/WorkspacePilotPrepareTests.cs'
OVERLOAD_WHY = {
    (WPP, 'DraftOf'): ('ayudante que nombra lo retirado: sus dos sobrecargas, DraftOf(PushBackPilotIntentResult, ...) (74-80) y DraftOf(DraftStaleBase, '
                       'string) (82-90), nombran el comparador del piloto (PushBackPilotStateComparer.Instance y Copy); la baja quita las dos (CA-T4d M-04 a)'),
    (WPP, 'Hold'): ('ayudante: sus dos sobrecargas se borran juntas (la baja quita todas las del nombre; CA-T4d M-04 a): Hold(DraftSession<PushBackPilotDraftState>) '
                    '(2055-2059) nombra el comparador del piloto (PushBackPilotStateComparer.Copy); Hold(DraftSession<DraftForm>) (126-128) no nombra lo retirado, '
                    'pero queda sin uso al borrarse las pruebas de ConfirmBase que lo usaban (y con el, su record Held y AssertHeld, fila M-03)'),
}


PEG = 'tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs'
HELPER_WHY = {(PEG, 'WriteViolations'): ['M-04 (PEG:916-920, su ayudante WriteViolations, PEG:906): el ayudante busca FindPilotDraft con la firma del '
                                         'borrador completo (D-S13); recomprobacion 1 de CA-T4d, M-12 c']}
USING_WHY = {('tests/RackCad.Tests/Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs', 'using RackCad.Application.Systems.PushBack.Draft;'):
             'la directiva de Draft/ que necesita la llamada de StaleBase sobre PushBackFullDraftReader.Read (pbt4d_test_edits, la edicion de StaleBase)'}


def c7_why(text):
    """Los porque heredados de la lista de 138 decian `sustituir con registro (C-7: ...)`; la accion es borrar: queda el motivo (CA-T4d M-04 c)."""
    m = re.match(r'^sustituir con registro \((.*)\)$', text)
    return m.group(1) if m else text


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

HEAD = ['n', 'grupo', 'archivo', 'miembro', 'lineas-6c2820ba', 'skip-6c2820ba', 'accion', 'porque', 'en-A', 'necesidad-r6h',
        'declaraciones-6c2820ba', 'tramo-editado']
table = []
count = collections.Counter()


def add(group, path, member, lines, skip, action, why, in_a, necesidad):
    count[(group, action)] += 1
    table.append([group, path, member, lines, skip, action, why.replace('\t', ' '), in_a, necesidad])


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
    elif path == WPUR and member in WPUR_SUBST:
        num, how = WPUR_SUBST[member]
        action = 'sustituir con registro'
        why = ('ayudante de las sustitutas, copiado con el mapa en PushBackRetirePilotRunTests' if num == 'ayudante'
               else 'n.o %s de WPUR: %s; copiada con %s' % (num, REBUILD_WHY if member in ed.WPUR_REBUILD else SUBST_WHY, how))
    elif path == WPUR:
        action, why = 'borrar con registro', WPUR_DELETE_WHY
    elif nec == 'no-necesaria':
        # compila sin quitarla y prueba algo que se queda: se edita en su sitio (pbt4d_test_edits_v52.py)
        action, why = 'reescribir con registro', 'compila sin quitarla y tambien prueba codigo que se queda: ' + '; '.join(edit_why[path])
    else:
        action = 'borrar con registro'
        why = {'C-7': 'solo fija lo retirado: ', 'original': 'original saltada desde PB-T4b (su copia o su equivalente corre desde entonces): ',
               'apoyo': 'ayudante que nombra lo retirado: ', 'SUS-01': '', 'compilacion': 'deja de compilar con la retirada B: '}[group] + c7_why(base_why)
    why = OVERLOAD_WHY.get((path, member), why)
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
]
for group, path, member, action, why in EXTRA_ROWS:
    a, b, skip = sp.span(REPO, path, member)
    if member == 'WriteViolations':
        a, b = 255, 258
    add(group, path, member, '%d-%d' % (a, b), skip, action, why, '-', '-')
# ---- CA-T4d M-03: los ayudantes que la retirada deja sin uso
um = load('unused_v53', os.path.join(HERE53, 'unused_v53.py'))
for path, member, why, nth, how in UNUSED:
    found = um.spans(sp.text(REPO, path), member)
    assert found, ('M-03: sin declaracion en la base', path, member)
    a, b = found[nth - 1] if nth else (found[0][0], found[-1][1])
    add('M-03', path, member, '%d-%d' % (a, b), '-', 'borrar con registro',
        'borrar con registro (ayudante sin uso tras la retirada; CA-T4d M-03): ' + why + ('' if how == 'quitar' else ' (lo quita la edicion de WPUR_TAIL)'), '-', '-')
for path, why in getattr(ed, 'DELETE_FILES', []):
    n_lines = sp.text(REPO, path).rstrip('\n').count('\n') + 1
    add('M-03', path, 'archivo', '1-%d' % n_lines, '-', 'borrar con registro',
        'borrar con registro (clase sin pruebas tras la retirada; CA-T4d M-03): ' + why + '; se borra el archivo (bloque DELETE)', '-', '-')


# ---- CA-T4d M-04: las declaraciones de cada nombre (las sobrecargas) y lo que el RED cambia dentro de ellas
def decls(path, member):
    code = sp.text(REPO, path)
    lines = code.split('\n')
    if member.startswith('tipo:'):
        a, b, _ = sp.span(REPO, path, member)
        return [(a, b)]
    found = [(sp._up(lines, m['first']), m['last']) for m in lc.members(code) if m['name'] == member]
    masked = lc.mask(code)
    for fm in sp.FIELD.finditer(masked):
        if fm.group(1) == member:
            a = masked.count('\n', 0, fm.start()) + 1
            if not any(x <= a <= y for x, y in found):
                end = masked.find(';', fm.end() - 1)
                found.append((sp._up(lines, a), masked.count('\n', 0, end) + 1))
    if not found:
        a, b, _ = sp.span(REPO, path, member)
        found = [(a, b)]
    return sorted(set(found))


_bp = os.path.join(REDTREE, '_borrados.txt')
RED_DELETED = set(open(_bp, encoding='utf-8').read().split()) if os.path.exists(_bp) else set()


def red_text(path):
    if path in RED_DELETED:
        return ''
    p = os.path.join(REDTREE, path)
    return open(p, encoding='utf-8').read() if os.path.exists(p) else sp.text(REPO, path)


def spans_of(code):
    """Cada miembro del texto: (nombre, primera, ultima), con sus /// y atributos (metodos de lc.members y campos o propiedades)."""
    lines = code.split('\n')
    out = [(m['name'], sp._up(lines, m['first']), m['last']) for m in lc.members(code)]
    masked = lc.mask(code)
    for fm in sp.FIELD.finditer(masked):
        a = masked.count('\n', 0, fm.start()) + 1
        if any(x <= a <= y for _, x, y in out):
            continue
        end = masked.find(';', fm.end() - 1)
        if fm.group(0).rstrip().endswith('get'):
            d = 0
            for k in range(masked.index('{', fm.start()), len(masked)):
                if masked[k] == '{':
                    d += 1
                elif masked[k] == '}':
                    d -= 1
                    if d == 0:
                        end = k
                        break
        out.append((fm.group(1), sp._up(lines, a), masked.count('\n', 0, end) + 1))
    return out


def member_diff(base_code, red_code):
    """Las lineas de la base que el RED cambia, miembro a miembro (asi una baja no se lleva el atributo identico del vecino), y fuera de los
    miembros por el esqueleto que queda."""
    a, b = base_code.split('\n'), red_code.split('\n') if red_code else []
    sa, sb = spans_of(base_code), spans_of(red_code) if red_code else []
    by_b = collections.defaultdict(list)
    for name, x, y in sb:
        by_b[name].append('\n'.join(b[x - 1:y]))
    by_a = collections.defaultdict(list)
    for name, x, y in sa:
        by_a[name].append((x, y))
    ch = set()
    for name, occ in by_a.items():
        reds = list(by_b.get(name, []))
        pending = []
        for x, y in occ:
            t = '\n'.join(a[x - 1:y])
            if t in reds:
                reds.remove(t)
            else:
                pending.append((x, y))
        for i, (x, y) in enumerate(pending):
            if i < len(reds):
                for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, a[x - 1:y], reds[i].split('\n'), autojunk=False).get_opcodes():
                    if tag in ('replace', 'delete'):
                        ch.update(range(x + i1, x + i2))
                    elif tag == 'insert':
                        ch.add(x + max(i1 - 1, 0))
            else:
                ch.update(range(x, y + 1))
    inside_a = {k for _, x, y in sa for k in range(x, y + 1)}
    inside_b = {k for _, x, y in sb for k in range(x, y + 1)}
    ka = [k for k in range(1, len(a) + 1) if k not in inside_a]
    kb = [k for k in range(1, len(b) + 1) if k not in inside_b]
    for tag, i1, i2, j1, j2 in difflib.SequenceMatcher(None, [a[k - 1] for k in ka], [b[k - 1] for k in kb], autojunk=False).get_opcodes():
        if tag in ('replace', 'delete'):
            ch.update(ka[i1:i2])
        elif tag == 'insert' and ka:
            ch.add(ka[max(i1 - 1, 0)])
    return ch


red_changed = set(RED_DELETED)
for root, _, names in os.walk(os.path.join(REDTREE, 'tests')):
    for nm in names:
        rel = os.path.relpath(os.path.join(root, nm), REDTREE).replace('\\', '/')
        try:
            if sp.text(REPO, rel) != red_text(rel):
                red_changed.add(rel)
        except Exception:
            pass  # un archivo nuevo del RED: no es de la base
changed_lines = {}
for path in sorted({r[1] for r in table} | red_changed):
    changed_lines[path] = member_diff(sp.text(REPO, path), red_text(path))


def ranges(ns):
    ns = sorted(ns)
    out, start = [], None
    for i, k in enumerate(ns):
        if start is None:
            start = k
        if i + 1 == len(ns) or ns[i + 1] != k + 1:
            out.append('%d-%d' % (start, k) if k != start else '%d' % k)
            start = None
    return ', '.join(out)


def trivial(s):
    s = s.strip()
    return s in ('', '{', '}', '};', ')', ');', '});') or s.startswith('//') or s.startswith('using ')


# el porque de una edicion, por su texto de inicio en la base
edit_at = collections.defaultdict(list)
for path, frm, to, new, why in ed.EDITS:
    base_lines = sp.text(REPO, path).split('\n')
    first = frm.split('\n')[0]
    hits = [i + 1 for i, l in enumerate(base_lines) if l == first]
    if len(hits) == 1:
        edit_at[path].append((hits[0], why))
attributed = collections.defaultdict(set)
for r in table:
    path, member = r[1], r[2]
    if member == 'archivo':
        a, b = map(int, r[3].split('-'))
        ds = [(a, b)]
    elif r[0] == 'M-03':
        nth = {(p, m): k for p, m, _, k, _ in UNUSED}[(path, member)]
        found = um.spans(sp.text(REPO, path), member)
        ds = [found[nth - 1]] if nth else found
    else:
        ds = decls(path, member)
    inside = {k for a, b in ds for k in range(a, b + 1)}
    attributed[path] |= inside
    edited = changed_lines[path] & inside
    whole = inside and all(k in changed_lines[path] or trivial(sp.text(REPO, path).split('\n')[k - 1]) for k in inside)
    r += [', '.join('%d-%d' % d for d in ds), ('todo: ' if whole and edited else '') + (ranges(edited) if edited else '-')]
extra_rows, outside = [], []
for path in sorted(changed_lines):
    base_lines = sp.text(REPO, path).split('\n')
    loose = [k for k in sorted(changed_lines[path] - attributed[path]) if k <= len(base_lines) and not trivial(base_lines[k - 1])]
    if not loose:
        continue
    ms = [(sp._up(base_lines, m['first']), m['last'], m['name']) for m in lc.members(sp.text(REPO, path))]
    seen = {}
    for k in loose:
        owner = [m for m in ms if m[0] <= k <= m[1]]
        if not owner:
            outside.append('%s:%d %s' % (path, k, base_lines[k - 1].strip()[:100]))
            continue
        a, b, name = min(owner, key=lambda m: m[1] - m[0])
        seen.setdefault(name, set()).add(k)
    for name, ks in seen.items():
        still = re.search(r'\b' + re.escape(name) + r'\s*[(<]', lc.mask(red_text(path)))
        whys = [w for at, w in edit_at[path] if any(d[0] <= at <= d[1] for d in decls(path, name))] or edit_why[path]
        # recomprobacion 1 de CA-T4d, M-12 (c): el porque de un ayudante, el de la edicion que lo cambia, cuando la busqueda por linea de inicio no
        # lo encuentra (WriteViolations, PEG:906, lo edita la de M-04 de PEG:916-920, cuyo texto empieza en la prueba y no en el ayudante)
        whys = HELPER_WHY.get((path, name), whys)
        action = 'reescribir con registro' if still else 'borrar con registro'
        ds = decls(path, name)
        edited = changed_lines[path] & {x for a, b in ds for x in range(a, b + 1)}
        extra_rows.append(['ayudante-editado', path, name, '%d-%d' % (ds[0][0], ds[-1][1]), '-', action,
                           'ayudante que el RED edita sin fila propia (CA-T4d M-04 b): ' + '; '.join(dict.fromkeys(whys)), '-', '-',
                           ', '.join('%d-%d' % d for d in ds), ranges(edited)])
        count[('ayudante-editado', action)] += 1
assert not outside, ('lineas de codigo que el RED cambia fuera de todo miembro', outside)
# recomprobacion 1 de CA-T4d, M-12 (d): las directivas using que el RED anade o quita (fuera de todo miembro), una fila cada una
USING = re.compile(r'^\s*using\s+(?:static\s+)?[\w.]+\s*;', re.M)
for path in sorted(changed_lines):
    if path in RED_DELETED:
        continue
    bu_ = [l.strip() for l in sp.text(REPO, path).split('\n') if USING.match(l)]
    ru_ = [l.strip() for l in red_text(path).split('\n') if USING.match(l)]
    for d in sorted(set(ru_) - set(bu_)):
        n_red = [l.strip() for l in red_text(path).split('\n')].index(d) + 1
        extra_rows.append(['directiva', path, d, '-', '-', 'reescribir con registro', USING_WHY.get((path, d), 'directiva que el RED anade para una edicion con registro de este archivo')
                           + ' (recomprobacion 1 de CA-T4d, M-12 d)', '-', '-', '-', 'anadida (linea %d del RED)' % n_red])
        count[('directiva', 'reescribir con registro')] += 1
    for d in sorted(set(bu_) - set(ru_)):
        n_base = [l.strip() for l in sp.text(REPO, path).split('\n')].index(d) + 1
        extra_rows.append(['directiva', path, d, '%d-%d' % (n_base, n_base), '-', 'borrar con registro',
                           'directiva que queda sin uso con las bajas de este archivo (recomprobacion 1 de CA-T4d, M-12 d)', '-', '-', '%d-%d' % (n_base, n_base), 'todo: %d' % n_base])
        count[('directiva', 'borrar con registro')] += 1
table += extra_rows
out = ['\t'.join(HEAD)] + ['\t'.join([str(i)] + r) for i, r in enumerate(table, 1)]
rows = len(table)
n = sum(c for (g, act), c in count.items() if act != 'se queda')
text = '\n'.join(out) + '\n'
os.makedirs(OUTDIR, exist_ok=True)
target = os.path.join(OUTDIR, 'pbt4d-retire-list-%d.tsv' % n)
with open(target, 'w', encoding='utf-8', newline='\n') as f:
    f.write(text)
print('filas %d, con accion %d; %s' % (rows, n, os.path.basename(target)))
for r in extra_rows:
    print('  fila nueva (M-04 b): %s %s %s %s' % (r[1].split('/')[-1], r[2], r[5], r[10]))
print('sha256 %s' % hashlib.sha256(text.encode('utf-8')).hexdigest())
for (g, act), c in sorted(count.items()):
    print('  %s | %s | %d' % (g, act, c))
