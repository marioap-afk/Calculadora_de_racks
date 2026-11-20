"""Contrato de gate de F4B-PBT4-FULL-READ-PATH (PB-T4a; ASCII), desde el paquete del contrato de PB-T4 (PKG_VERSION, abajo: la v4.2, con B-08), con el
patron de make_pbt3c_contract.py. La base es el cierre de PB-T3c (7f0d9aa3, F4B-E48; GREEN 938a35f0): PB-T3 queda completa. La emision de PB-T4a es
F4B-E50, con la evidencia 277 (X) y 278 (BASE); F4B-E49 son las decisiones del Owner sobre PB-T4 (owner-pbt4-decisions.md y, despues,
owner-pbt4b-retire-list-decisions.md: PB-T4d y la lista de PB-T4b), en el mismo commit X.
Comprueba por objetos git:
- el anclaje: la revision desciende de la emision de PB-T3c (F4B-E47) y de su cierre (F4B-E48), con src y tests iguales a ese cierre y a su GREEN; la
  entrada de cierre, su CI (Core 19 129 = 19 123 + 0 + 6; UI 2 451 = 2 430 + 21) y su task-close-package; la numeracion (E49, E50, 277 y 278);
- el contrato de PB-T3c del cierre (los textos comunes y PBT3C-C11, de los que PB-T4a toma los suyos con su prefijo);
- los sustitutos de nc1 (PD-F4B-01) y de nc2 v2 con su SHA-256, que admiten --task PBT4, y la primera ExpectedEvidence en F4B_EXACT_EVIDENCES;
- la Snapshot K; el generador mismo, en ASCII puro (la leccion del cierre de PB-T3c: un caracter no ASCII rompe a los check_* que lo leen en ascii);
- el paquete, por su sha256: el Objective, el alcance y su ForbiddenWriteScope, los invariantes y las paradas propios, la tabla de RequiredTests, la
  ExpectedEvidence propia y la linea de la CI, la tabla de RR-3, el Skip de CT-PBT3A-12 y B-08 (AtOrigin solo llama a PushBackOriginByText.ReadAt).
En seco lista los AVISOS de la emision; al escribir, cualquier AVISO aborta.
Uso: python make_pbt4a_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--objects-rev <rev> [--repo <checkout principal>]
                                   [--package-file <paquete>] [--bounded-file <informe de CA-T4a>]] [--json-out <archivo>] [--as-write]
Al custodiarlo, las rutas locales se redactan como en PB-T0 ('<worktree-de-I-64>', '<checkout-principal>')."""
import json, os, re, sys, subprocess, hashlib, unicodedata

# la leccion del cierre de PB-T3c: el generador entero es ASCII (lo leen en ascii los check_* y la redaccion); BOUNDED_RESULT tambien
_self = open(os.path.abspath(__file__), 'rb').read()
assert all(b < 0x80 for b in _self), ('el generador lleva un caracter no ASCII', [i for i, b in enumerate(_self) if b >= 0x80][:5])

# ---------------------------------------------------------------- la base: el cierre de PB-T3c (F4B-E48), medido por objetos git
PT3C_CLOSE = '7f0d9aa377ca1e3301bfce3716bd29d602d762dd'   # la custodia del cierre de PB-T3c (F4B-E48); HEAD al emitir PB-T4a
GREEN_T3C = '938a35f0f1636b4306d844eb9e34000e8967c05e'    # el GREEN de PB-T3c (intento 1)
RED_T3C = 'e0940a0efa314ea26777c59ab4e55c09d019bff6'      # el RED de PB-T3c
PT3C_EMIT = '7dfc7c21a0b30f5e04d691768b1717778e5c4ad7'     # la emision de PB-T3c (F4B-E47): BASE 7dfc7c21, X e3c293b1
PT3C_EMIT_X = 'e3c293b173a9d0914803bb344595f0e48dde0390'
PT3B_CLOSE = 'fe1c67f4efa23f2abb9b6ab9d636df54ae15f343'    # el cierre de PB-T3b (F4B-E46)
# la CI del GREEN de PB-T3c en su cierre: (run, sha8 del GREEN, Core, Skipped de Core, UI, Skipped de UI)
CI_BASE_T3C = ('44757675821', '938a35f0', 19129, 6, 2451, 21)
# la numeracion (la sesion): parametros comprobados contra f4b-dispositions.md y la evidencia (una asignacion cada uno, para leerlos con ast)
T3C_CLOSE_ENTRY = 'F4B-E48'
T3C_EMIT_ENTRY = 'F4B-E47'
OWNER_ENTRY = 'F4B-E49'
EMIT_ENTRY = 'F4B-E50'
CLOSE_EV = '276'
X_EV = '277'
BASE_EV = '278'
PT3C_TASK = 'F4B-PBT3C-FULL-WRITE-ERASE'
D1_REV = 'd8ccbcb870f4d1804d7e7afd3855ac64b44693a8'
BOUNDED_RESULT = ('EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; las importantes corregidas en el paquete v4.3 (I-01, la anotacion string? de '
                  'PushBackOriginByText.cs: aviso CS8632 que rompe build-plugin); sin decisiones nuevas del Owner; remedicion sobre 7f0d9aa3 sin '
                  'cambios materiales; recomprobacion v4.3: EMITIBLE, B 0 / I 0 / M 2')   # el veredicto literal de CA-T4a y su recomprobacion (ASCII)
PKG_VERSION = 'v4.3'    # la version del paquete con la que se emite PB-T4a (la custodia el commit X de la emision)
CUSTODY_SHA256 = {'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md': '4f4822798920d3f15e3c78f349b702549bef4eced6c5f26009252272247cb9d2'}  # la v4.3 (B-08; CA-T4a I-01)
SNAP_K = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-K/snapshot-k.json'
SNAP_K_DEC = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-config-snapshot-k-decision.md'
SNAP_K_SHA = 'E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE'
SNAP_K_PROBE = 'R20261119T154313Z-2a9a'
NC1_SUBST = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc1-cascade-substitute/nc1_cascade_signal_substitute.py'
NC1_SUBST_SHA = '41440edc4a963bef890e4336dccb885837424008c027cdb172d2b926e0f49b40'
NC2_DIR = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/'
NC2_V2 = NC2_DIR + 'nc2_exact_whitelist_substitute_f4b_v2.py'
NC2_V2_SHA = 'c033e96ce147963feb9c04733e9bfbec7493fea33d8ff79b1c2e8ab860798e45'
NC2_V2_REVIEW = NC2_DIR + 'review/independent-review-substitute-f4b-v2.md'
NC2_V2_LIMITS = NC2_DIR + 'limits-declared.md'
NC_TASK = 'PBT4'  # el --task de nc1 y nc2 para las tres subtareas de PB-T4 (el TaskId empieza por F4B-PBT4-)
MAIN_CHECKOUT = '<checkout-principal>'  # Redaccion para el repo publico: la ruta local del checkout principal
WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree

ARGS = sys.argv[1:]
VALUED = ('--objects-rev', '--repo', '--package-file', '--json-out', '--bounded-file')


def opt(name):
    if name not in ARGS:
        return None
    i = ARGS.index(name)
    assert i + 1 < len(ARGS) and not ARGS[i + 1].startswith('--'), ('falta el valor de', name)
    return ARGS[i + 1]


POS = [a for i, a in enumerate(ARGS) if not a.startswith('--') and not (i > 0 and ARGS[i - 1] in VALUED)]
assert len(POS) == 3, 'uso: make_pbt4a_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
DRY = '--dry' in ARGS
AS_WRITE = '--as-write' in ARGS
OBJ_REV = opt('--objects-rev')
assert OBJ_REV is None or DRY, '--objects-rev solo en seco: anade --dry'
assert not AS_WRITE or DRY, '--as-write solo en seco'
PKG_FILE = opt('--package-file')
assert PKG_FILE is None or (OBJ_REV is not None and DRY), '--package-file solo junto con --objects-rev y --dry'
PKG_FILE = os.path.abspath(PKG_FILE) if PKG_FILE else None
assert PKG_FILE is None or os.path.isfile(PKG_FILE), ('no existe el paquete', PKG_FILE)
BOUNDED_FILE = opt('--bounded-file')
assert BOUNDED_FILE is None or (OBJ_REV is not None and DRY), '--bounded-file solo junto con --objects-rev y --dry'
BOUNDED_FILE = os.path.abspath(BOUNDED_FILE) if BOUNDED_FILE else None
assert BOUNDED_FILE is None or os.path.isfile(BOUNDED_FILE), ('no existe el informe de la comprobacion acotada', BOUNDED_FILE)
JSON_OUT = opt('--json-out')
assert JSON_OUT is None or DRY, '--json-out solo en seco'
JSON_OUT = os.path.abspath(JSON_OUT) if JSON_OUT else None
for _root in (MAIN_CHECKOUT, WT):
    assert JSON_OUT is None or not os.path.normcase(JSON_OUT).startswith(os.path.normcase(os.path.abspath(_root))), '--json-out fuera del repositorio'
assert re.fullmatch(r'[0-9]{4}-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9]Z', ISSUED) and ISSUED[:10] < '2026-12-29', ISSUED
assert re.fullmatch(r'[0-9a-f]{40}', AR) and re.fullmatch(r'[0-9a-f]{40}', MAIN), (AR, MAIN)
assert BOUNDED_RESULT is None or all(ord(ch) < 0x80 for ch in BOUNDED_RESULT), 'BOUNDED_RESULT en ASCII'

if OBJ_REV is None:
    os.chdir(WT)
    REPO, REV = None, 'HEAD'
else:
    REPO, REV = opt('--repo') or MAIN_CHECKOUT, OBJ_REV
DISK = OBJ_REV is None

TASK = 'F4B-PBT4-FULL-READ-PATH'
assert len(TASK) <= 26 and TASK.startswith('F4B-' + NC_TASK + '-'), len(TASK)  # R-T1-07 (MAX_PATH, una cota superior); la ligadura de --task PBT4
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
D4 = PILOT + '/F4B-DRAFTS/pbt4/'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
PT3C_TCP = PILOT + '/' + PT3C_TASK + '/task-close-package.md'
PBT3CC = PILOT + '/' + PT3C_TASK + '/coordinator/gate-contract.json'
OWNER_DEC = CO + 'owner-pbt4-decisions.md'
OWNER_DEC2 = CO + 'owner-pbt4b-retire-list-decisions.md'  # la segunda decision del Owner sobre PB-T4 (PB-T4d; la lista de PB-T4b, 470), en F4B-E49
# B-08 (la v4.2): lo que puede cambiar con la v4.2 se fija aqui. Las rutas del alcance de PB-T4a en section 6.1, en su orden; los archivos nuevos son
# los que no estan en la base. Con la v4.2, si ReadAt va en un archivo nuevo de Draft/, se anade aqui (y PKG_VERSION y su sha256 arriba)
ALLOWED_EXPECTED = ['src/RackCad.Application/Workspace/WorkspaceEditIntent.cs', 'src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs',
                    'src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs', 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs',
                    'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs', 'src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs',
                    'tests/RackCad.Tests/PushBackFullReadPathTests.cs', 'tests/RackCad.Tests/PushBackFullReadGuardTests.cs',
                    'tests/RackCad.Tests/PushBackFullReadSupport.cs', 'tests/RackCad.Tests/PushBackDraftWriteGuardTests.cs']
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
WARN = []
PENDING_V41 = []  # lo que espera a la v4.1: el seco lo lista; al escribir, aborta
def git(*args, inp=None):
    return subprocess.run(['git'] + (['-C', REPO] if REPO else []) + list(args), input=inp, capture_output=True)


_ALL = None  # los archivos de la revision (un solo ls-tree -r -z) y sus carpetas


def _all():
    global _ALL
    if _ALL is None:
        r = git('ls-tree', '-r', '-z', '--name-only', REV)
        assert r.returncode == 0, r.stderr[-300:]
        files = [p for p in r.stdout.decode('utf-8').split('\0') if p]
        dirs = {p[:i] for p in files for i in range(len(p)) if p[i] == '/'}
        _ALL = (files, set(files), dirs)
    return _ALL


def in_head(path):
    _, files, dirs = _all()
    p = path.rstrip('/')
    return p in dirs if path.endswith('/') else (p in files or p in dirs)


def show_bytes(path, rev=None):
    r = git('show', (rev or REV) + ':' + path)
    assert r.returncode == 0, ('no se puede leer', rev or REV, path)
    return r.stdout


def show(path, rev=None):
    return show_bytes(path, rev).decode('utf-8')


def lines_of(path, rev=None):
    return show(path, rev).replace('\r\n', '\n').split('\n')


def tracked(prefix):
    return [p for p in _all()[0] if p.startswith(prefix)]
def soft(cond, msg):
    """Lo que solo existira con la emision de PB-T4a (su commit X, su comprobacion acotada): AVISO en seco, error al escribir (y con --as-write)."""
    if cond:
        return True
    assert DRY and not AS_WRITE, msg
    WARN.append(msg)
    return False


def pending(msg):
    """Lo que espera al texto de la v4.1 del paquete: se lista en seco; al escribir, aborta."""
    assert DRY, 'pendiente de la v4.1: ' + msg
    PENDING_V41.append(msg)


REV_SHA = git('rev-parse', REV).stdout.decode('ascii').strip()
assert re.fullmatch(r'[0-9a-f]{40}', REV_SHA), ('revision desconocida', REV)

# ---------------------------------------------------------------- el anclaje: el cierre de PB-T3c (F4B-E48)
for _short, _full in (('7f0d9aa3', PT3C_CLOSE), ('938a35f0', GREEN_T3C), ('e0940a0e', RED_T3C), ('7dfc7c21', PT3C_EMIT), ('e3c293b1', PT3C_EMIT_X),
                      ('fe1c67f4', PT3B_CLOSE), ('d8ccbcb8', D1_REV)):
    assert git('rev-parse', _short).stdout.decode('ascii').strip() == _full, ('SHA', _short)
for _anc, _name in ((D1_REV, 'F4B-E29 (D-1)'), (PT3B_CLOSE, 'el cierre de PB-T3b (F4B-E46)'), (PT3C_EMIT, 'la emision de PB-T3c (F4B-E47)'),
                    (GREEN_T3C, 'el GREEN de PB-T3c'), (PT3C_CLOSE, 'el cierre de PB-T3c (F4B-E48)')):
    assert git('merge-base', '--is-ancestor', _anc, REV).returncode == 0, ('la revision no desciende de ' + _name)
assert git('merge-base', '--is-ancestor', RED_T3C, GREEN_T3C).returncode == 0 and git('merge-base', '--is-ancestor', PT3C_EMIT, RED_T3C).returncode == 0, \
    'el RED de PB-T3c no esta entre su emision y su GREEN'
assert git('diff', '--quiet', PT3C_CLOSE, REV, '--', 'src', 'tests').returncode == 0, \
    'src o tests cambiaron desde el cierre de PB-T3c (el diff frente al cierre de la subtarea anterior, vacio)'
assert git('diff', '--quiet', GREEN_T3C, PT3C_CLOSE, '--', 'src', 'tests').returncode == 0, 'el cierre de PB-T3c no tiene el src y los tests de su GREEN'
_disp_c = show(DISP, PT3C_CLOSE).replace('\r\n', '\n')
assert re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (T3C_EMIT_ENTRY, PT3C_TASK), _disp_c), 'F4B-E47, la emision de PB-T3c'
_heads = re.findall(r'(?m)^## (F4B-E\d+)\. Cierre de ' + PT3C_TASK + r'\b', _disp_c)
assert _heads == [T3C_CLOSE_ENTRY], ('el cierre de PB-T3c en su f4b-dispositions.md', _heads, T3C_CLOSE_ENTRY)
assert re.split(r'(?m)^## ', _disp_c)[-1].startswith(T3C_CLOSE_ENTRY + '. '), 'el cierre de PB-T3c no es la ultima entrada de su f4b-dispositions.md'
CLOSE_TEXT = _disp_c[_disp_c.index('\n## %s. ' % T3C_CLOSE_ENTRY):]
for _needle in ('CLOSED / PASS WITH LIMITATIONS', GREEN_T3C[:8], RED_T3C[:8], 'attempts 1 de 3'):
    assert _needle in CLOSE_TEXT, ('la entrada de cierre de PB-T3c sin', _needle)
assert 'CLOSED / PASS WITH LIMITATIONS' in show(PT3C_TCP, PT3C_CLOSE), 'el task-close-package de PB-T3c no dice CLOSED'
# la CI del GREEN de PB-T3c en su entrada de cierre, con el total exacto de su unica rama (F4B-E46)
_run, _sha, _core, _cskip, _ui, _skip = CI_BASE_T3C
_spaced = lambda v: format(v, ',').replace(',', ' ')
assert GREEN_T3C.startswith(_sha) and _run in CLOSE_TEXT, ('la CI del GREEN de PB-T3c no es la de su cierre', CI_BASE_T3C)
assert ('Core %s = %s + 0 + %d' % (_spaced(_core), _spaced(_core - _cskip), _cskip)) in CLOSE_TEXT and \
    ('UI %s = %s + %d' % (_spaced(_ui), _spaced(_ui - _skip), _skip)) in CLOSE_TEXT, ('los recuentos de CI_BASE_T3C no son los de la entrada de cierre', CI_BASE_T3C)
# la numeracion: F4B-E49 (las decisiones del Owner) y F4B-E50 (la emision) siguen al cierre de PB-T3c; 277 (X) y 278 (BASE), a la 276
_disp = show(DISP).replace('\r\n', '\n')
_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
assert _ids == list(range(1, len(_ids) + 1)) and _ids[-1] >= 48, ('f4b-dispositions.md', _ids[-3:])
assert int(OWNER_ENTRY.split('E')[-1]) == int(T3C_CLOSE_ENTRY.split('E')[-1]) + 1 and int(EMIT_ENTRY.split('E')[-1]) == int(OWNER_ENTRY.split('E')[-1]) + 1, \
    ('la numeracion: E49 y E50 siguen a E48', OWNER_ENTRY, EMIT_ENTRY)
E49_OK = bool(re.search(r'(?m)^## %s\. ' % OWNER_ENTRY, _disp))
E50_OK = bool(re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (EMIT_ENTRY, TASK), _disp))
soft(E49_OK, 'emision: %s (las decisiones del Owner sobre PB-T4) aun no esta en f4b-dispositions.md' % OWNER_ENTRY)
soft(E50_OK, 'emision: %s (la emision de %s) aun no esta en f4b-dispositions.md' % (EMIT_ENTRY, TASK))
_evs = re.findall(r'(?m)^## (\d+)\. ', show('docs/automation/evidence/I-64-evidence.md'))
assert _evs[-1] in (CLOSE_EV, X_EV) and int(X_EV) == int(CLOSE_EV) + 1 and int(BASE_EV) == int(X_EV) + 1, ('las secciones de la evidencia', _evs[-3:])
soft(in_head(OWNER_DEC), 'emision: el registro de las decisiones del Owner sobre PB-T4 (%s) aun no esta en la revision' % OWNER_DEC)
soft(in_head(OWNER_DEC2), 'emision: el registro de las decisiones del Owner sobre PB-T4b y PB-T4d (%s) aun no esta en la revision' % OWNER_DEC2)
if soft(git('cat-file', '-e', AR + '^{commit}').returncode == 0, 'emision: la AuthorityRevision %s no esta en el almacen de objetos (seco)' % AR[:8]):
    assert git('merge-base', '--is-ancestor', PT3C_CLOSE, AR).returncode == 0, 'la AuthorityRevision no contiene el cierre de PB-T3c'

# ---------------------------------------------------------------- el contrato de PB-T3c del cierre: de el salen los textos comunes y C11
pbt3c = json.loads(show(PBT3CC, PT3C_CLOSE))
assert pbt3c['TaskId'] == PT3C_TASK and pbt3c['AuthorityRevision'] == PT3C_EMIT_X, ('el contrato de PB-T3c del cierre', pbt3c['AuthorityRevision'][:8])
T3C_STOP = {s['Id']: s['Text'] for s in pbt3c['StopConditions']}
T3C_INV = {i.split('. ')[0]: i for i in pbt3c['Invariants']}
assert len(pbt3c['RequiredTests']) == 36 and len(pbt3c['StopConditions']) == 33, ('el contrato de PB-T3c', len(pbt3c['RequiredTests']))
EVID0 = pbt3c['ExpectedEvidence'][0]


# ---- snapk_check (inicio)
def snapk_check(record, decision, sha, probe):
    """El registro de la Snapshot K y la decision del Owner (F4B-E37; v2.9, V29-06): el registro es el de la K, con el SHA-256 del config.toml y
    la sonda neutra con Pass; la decision trae la respuesta literal del Owner. Devuelve los errores (vacia si vale)."""
    import json
    try:
        rec = json.loads(record)
    except ValueError as e:
        return ['el registro de la Snapshot K no es JSON: %s' % e]
    if not isinstance(rec, dict):
        return ['el registro de la Snapshot K no es un objeto JSON']
    errors = []
    if rec.get('Snapshot') != 'K':
        errors.append('el registro no es el de la Snapshot K: Snapshot=%r' % (rec.get('Snapshot'),))
    if str(rec.get('ConfigSha256', '')).upper() != sha:
        errors.append('el SHA-256 del registro (%s) no es el de la K (%s)' % (rec.get('ConfigSha256'), sha))
    neutral = rec.get('NeutralProbe') if isinstance(rec.get('NeutralProbe'), dict) else {}
    if neutral.get('RunId') != probe:
        errors.append('la sonda neutra del registro (%s) no es %s' % (neutral.get('RunId'), probe))
    if neutral.get('Pass') is not True:
        errors.append('la sonda neutra del registro no da Pass')
    if 'Snapshot K' not in decision or 'Sonda neutra, luego K (Recomendado)' not in decision:
        errors.append('la decision no trae la Snapshot K con la respuesta literal del Owner')
    return errors
# ---- snapk_check (fin)


# ---- rr_check (inicio)
def rr_check(text):
    """El archivo de F4B-E35 frente a lo que el contrato toma de el: la pregunta 4 con la respuesta del Owner literal, la regla RR de F4B, que
    empieza en PB-T2b, y RR-1 a RR-7, cada una una vez. Devuelve los errores (vacia si vale)."""
    import unicodedata
    t = text.replace(chr(0xAB), '"').replace(chr(0xBB), '"').replace(chr(0x2014), '-')
    t = ''.join(c for c in unicodedata.normalize('NFKD', t) if not unicodedata.combining(c))
    errors = []
    first = t.split('\n', 1)[0]
    if not (first.startswith('# ') and '(F4B-E35)' in first):
        errors.append('el titulo no es el de F4B-E35')
    q4 = t.split('\n## Pregunta 4\n')
    if len(q4) != 2:
        errors.append('no tiene una sola "## Pregunta 4"')
    else:
        body = q4[1].split('\n## ', 1)[0]
        if '**Respuesta del Owner:** "Si, con esas condiciones (Recomendado)".' not in body:
            errors.append('la respuesta del Owner a la pregunta 4 no es "Si, con esas condiciones (Recomendado)"')
    if t.count('\n## La regla operativa de la reparacion del RED (RR), regla de F4B') != 1:
        errors.append('no tiene la seccion de la regla operativa RR')
    if t.count('**Empieza en PB-T2b.**') != 1:
        errors.append('la regla no dice que empieza en PB-T2b')
    for k in range(1, 8):
        if t.count('\n- **RR-%d. ' % k) != 1:
            errors.append('RR-%d no esta una sola vez' % k)
    return errors
# ---- rr_check (fin)


# ---- q3_check (inicio)
def q3_check(text):
    """La pregunta 3 de F4B-E35, la autorizacion de emision de PB-T3 (precondicion 10; V43-07): la respuesta literal del Owner y su alcance, las
    tres subtareas en orden. Devuelve los errores (vacia si vale)."""
    import unicodedata
    t = text.replace(chr(0xAB), '"').replace(chr(0xBB), '"').replace(chr(0x2014), '-')
    t = ''.join(c for c in unicodedata.normalize('NFKD', t) if not unicodedata.combining(c))
    q3 = t.split('\n## Pregunta 3\n')
    if len(q3) != 2:
        return ['no tiene una sola "## Pregunta 3"']
    body = q3[1].split('\n## ', 1)[0]
    errors = []
    if '**Respuesta del Owner:** "Si, con esa condicion (Recomendado)".' not in body:
        errors.append('la respuesta del Owner a la pregunta 3 no es "Si, con esa condicion (Recomendado)"')
    if '**La autorizacion condicional cubre** PB-T3a, PB-T3b y PB-T3c, en ese orden' not in body:
        errors.append('la pregunta 3 no cubre PB-T3a, PB-T3b y PB-T3c en ese orden')
    return errors
# ---- q3_check (fin)


assert in_head(SNAP_K) and in_head(SNAP_K_DEC), ('la Snapshot K o su decision no estan en la revision', SNAP_K, SNAP_K_DEC)
_err = snapk_check(show(SNAP_K), show(SNAP_K_DEC), SNAP_K_SHA, SNAP_K_PROBE)
assert not _err, ('la Snapshot K (F4B-E37)', _err)

# ---------------------------------------------------------------- C11: los sustitutos de nc1 (PD-F4B-01) y de nc2 v2, con --task PBT4
assert hashlib.sha256(show_bytes(NC1_SUBST)).hexdigest() == NC1_SUBST_SHA, ('el sustituto de nc1 de PD-F4B-01 no esta en la revision con su SHA-256', NC1_SUBST)
_v2 = show_bytes(NC2_V2)
assert hashlib.sha256(_v2).hexdigest() == NC2_V2_SHA, ('el sustituto de nc2 v2 no esta en la revision con su SHA-256', NC2_V2)
for _p in (NC2_V2_REVIEW, NC2_V2_LIMITS):
    assert in_head(_p) and NC2_V2_SHA.upper()[:8] in show(_p).upper(), ('la revision o los limites del sustituto de nc2 v2', _p)
import ast as _ast


def _literals(raw):
    return {t.id: n.value for n in _ast.parse(raw.decode('utf-8')).body if isinstance(n, _ast.Assign) for t in n.targets if isinstance(t, _ast.Name)}


_lit2 = _literals(_v2)
NC2_TASKS = _ast.literal_eval(_lit2['TASKS'])
NC2_EVIDENCES = _ast.literal_eval(_lit2['F4B_EXACT_EVIDENCES'])
NC1_TASKS = _ast.literal_eval(_literals(show_bytes(NC1_SUBST))['TASKS'])
assert NC2_TASKS.get(NC_TASK) == 'PB-T4' and NC1_TASKS.get(NC_TASK) == 'PB-T4', ('nc1 o nc2 v2 no admiten --task PBT4', NC1_TASKS, NC2_TASKS)
# la primera ExpectedEvidence: la de PB-T3 (la del contrato de PB-T3c), byte a byte, en la lista cerrada de literales del v2 (v1 section 6.0 del paquete)
assert EVID0 in NC2_EVIDENCES, ('la primera evidencia no esta en F4B_EXACT_EVIDENCES del v2', EVID0[:80])
# C11 de PB-T4a: el PBT3C-C11 del contrato de PB-T3c con la tarea de nc PBT4 (el registro, --task y SATISFIED_UNDER_PBT4_...). El texto final lo fija
# la v4.1; aqui se comprueba que la sustitucion es mecanica y completa
C11_SUBS = [('PBT3C', NC_TASK), ('PB-T3c', 'PB-T4a')]
C11 = T3C_STOP['PBT3C-C11']
for _old, _new in C11_SUBS:
    assert _old in C11, ('PBT4A-C11: el prefijo de PB-T3c no esta en PBT3C-C11', _old)
    C11 = C11.replace(_old, _new)
for _need in ('PBT4-NC2-EXACT-WHITELIST-F4B-01', 'with --task PBT4 in the F4B-E21 mode', 'SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B',
              'with --task PBT4, recorded as nc1 = SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE', NC2_V2 + ', SHA-256 ' + NC2_V2_SHA,
              NC1_SUBST + ', SHA-256 ' + NC1_SUBST_SHA):
    assert C11.count(_need) == 1, ('PBT4A-C11', _need)
assert C11.endswith('no one-off acceptance extends to PB-T4a') and not re.search(r'PBT3C|PB-T3c|PBT3B-|IDENTITY-SIGNAL', C11), 'PBT4A-C11: restos de PB-T3c'


# ---------------------------------------------------------------- el paquete (PKG_VERSION): el bloque ASCII de literales (section 6.1.1), la tabla de
# RequiredTests, el alcance de section 6.1, la lista CT-PBT4A-01 a 08 de section 5.1 y la tabla de RR-3 de section 6.0
PKG = 'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md'
MEAS = 'docs/initiatives/I-64-f4b-pbt4-measurements.md'
soft(in_head(PKG) and hashlib.sha256(show_bytes(PKG)).hexdigest() == CUSTODY_SHA256[PKG],
     'emision: el paquete de PB-T4 %s aun no esta custodiado en la revision (%s): lo custodia el commit X de PB-T4a' % (PKG_VERSION, PKG))
if PKG_FILE:
    PKG_RAW, PKG_SRC = open(PKG_FILE, 'rb').read(), '--package-file ' + os.path.basename(PKG_FILE)
else:
    assert in_head(PKG), 'sin --package-file y sin el paquete en la revision'
    PKG_RAW, PKG_SRC = show_bytes(PKG), PKG + ' en ' + REV_SHA[:8]
PKG_LINES = PKG_RAW.decode('utf-8').replace('\r\n', '\n').split('\n')
_title = [l for l in PKG_LINES[:8] if l.startswith('# I-64 F4B')]
PKG_IN = re.search(r', (v\d+(?:\.\d+)?)\s*$', _title[0]).group(1) if len(_title) == 1 else None
assert PKG_IN == PKG_VERSION, ('el paquete leido (%s) es la %s, no la %s' % (PKG_SRC, PKG_IN, PKG_VERSION))


def ascii_fold_line(s):
    return ''.join(c for c in unicodedata.normalize('NFKD', s) if not unicodedata.combining(c))


def section(head_prefix):
    i = [k for k, l in enumerate(PKG_LINES) if l.startswith(head_prefix)]
    assert len(i) == 1, ('seccion', head_prefix, len(i))
    out = []
    for l in PKG_LINES[i[0] + 1:]:
        if l.startswith('## ') or l.startswith('### ') or l.startswith('#### '):
            break
        out.append(l)
    return out


SEC61, SEC611, SEC60, SEC51 = section('### 6.1 PB-T4a'), section('#### 6.1.1 '), section('### 6.0 '), section('### 5.1 ')
# el bloque ```text de section 6.1.1: ASCII, campo a campo
_a = SEC611.index('```text')
_b = SEC611.index('```', _a + 1)
BLOCK = SEC611[_a + 1:_b]
assert all(ord(ch) < 0x80 for l in BLOCK for ch in l), 'el bloque de literales de section 6.1.1 no es ASCII'


def field(name, nxt):
    i, j = BLOCK.index(name + ':'), (BLOCK.index(nxt + ':') if nxt else len(BLOCK))
    return [l for l in BLOCK[i + 1:j] if l.strip()]


assert BLOCK[0] == 'TaskId: ' + TASK, ('el TaskId del paquete', BLOCK[0])
_obj = field('Objective', 'Invariants')
assert len(_obj) == 1, 'el Objective, una linea'
OBJECTIVE = _obj[0]
assert ('PB-T4 ' + PKG_VERSION) in OBJECTIVE and 'CT-PBT4A-01..08' in OBJECTIVE and 'Solo se sustituye con registro CT-PBT3A-12.' in OBJECTIVE
INV = field('Invariants', 'StopConditions (propias)')
assert [i.split('. ')[0] for i in INV] == ['INV-PBT4A-%02d' % n for n in range(1, 15)], [i[:12] for i in INV]
STOP_OWN = field('StopConditions (propias)', 'ExpectedEvidence')
_ids = [s.split(':')[0] for s in STOP_OWN]
assert _ids == ['PBT4A-C%02d' % n for n in list(range(1, 15)) + [16, 17, 18]], _ids  # sin C15 (la v4.1)
STOP_OWN_TEXT = {s.split(':')[0]: s[len(s.split(':')[0]) + 2:] for s in STOP_OWN}
EVID = []
for l in field('ExpectedEvidence', None):
    m = re.match(r'^\[(\d+)\] (.*)$', l)
    assert m and int(m.group(1)) == len(EVID), ('la ExpectedEvidence, numerada', l[:40])
    EVID.append(m.group(2))
assert len(EVID) == 15, len(EVID)
# la tabla de RequiredTests de section 6.1.1 (38)
REQ_PKG = []
for l in SEC611:
    m = re.match(r'^\| (\d+) \| `([^`]+)` \| `([^`]+)` \| \**(\d+)\** \| (true|false) \|$', l)
    if m:
        REQ_PKG.append((int(m.group(1)), m.group(2), m.group(3), int(m.group(4)), m.group(5) == 'true'))
assert [r[0] for r in REQ_PKG] == list(range(1, 39)), ('la tabla de RequiredTests de section 6.1.1', len(REQ_PKG))
# el alcance de section 6.1 (10 rutas, en ese orden) y UNIT_CHANGE
_al = SEC61.index([l for l in SEC61 if l.startswith('- **`AllowedWriteScope`** (%d):' % len(ALLOWED_EXPECTED))][0])
ALLOWED = []
for l in SEC61[_al + 1:]:
    if not l.startswith('  - '):
        break
    ALLOWED += re.findall(r'`((?:src|tests)/[^`]+)`', l)
WEI = 'src/RackCad.Application/Workspace/WorkspaceEditIntent.cs'
ORIGIN_IFACE = 'src/RackCad.Application/Workspace/IWorkspaceEditIntentOriginReader.cs'
BY_TEXT = 'src/RackCad.Application/Systems/PushBack/Draft/PushBackOriginByText.cs'
WHC = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
RIA = 'src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs'
NH = 'src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs'
DWG = TR + 'PushBackDraftWriteGuardTests.cs'
TEST_NEW = [TR + n + '.cs' for n in ('PushBackFullReadPathTests', 'PushBackFullReadGuardTests', 'PushBackFullReadSupport')]
assert ALLOWED == ALLOWED_EXPECTED and ALLOWED[-1] == DWG and ALLOWED[-4:-1] == TEST_NEW, ALLOWED  # INV-PBT4A-01 (las rutas de section 6.1, en su orden)
assert '- **`UNIT_CHANGE`:** WEI, WHC, RIA y NH (F4B-E25).' in SEC61, 'UNIT_CHANGE de PB-T4a'
UNIT_CHANGE_P = [WEI, WHC, RIA, NH]
# la lista CT-PBT4A-01 a 08 de section 5.1: un [Fact] cada una (section 5.0), su clase y su ExpectRed
CTS = []
for l in SEC51:
    m = re.match(r'^\| \**(\d\d)\** \|', l)
    if m:
        cells = [c.strip() for c in l.strip()[2:-2].split(' | ')]
        CTS.append((int(m.group(1)), cells[3].strip('`'), cells[4].strip('*') == 'true'))
assert [c[0] for c in CTS] == list(range(1, 9)), [c[0] for c in CTS]
assert {c[1] for c in CTS} == {'PushBackFullReadPathTests', 'PushBackFullReadGuardTests'}, {c[1] for c in CTS}
N_CT, N_RED = len(CTS), sum(1 for c in CTS if c[2])
assert (N_CT, N_RED) == (8, 5) and [c[0] for c in CTS if c[2]] == [1, 2, 3, 4, 7], (N_CT, N_RED)
assert all((c[1] == 'PushBackFullReadPathTests') == c[2] for c in CTS), 'ExpectRed por clase (las de PushBackFullReadPathTests, true)'


def cover(prefix):
    """Prefijos y archivos minimos que cubren todo prefix salvo las rutas permitidas (algoritmo de [PKC-C])."""
    inside = [a for a in ALLOWED if a.startswith(prefix)]
    if not inside:
        return [prefix]
    out = []
    names = sorted({p[len(prefix):].split('/')[0] + ('/' if '/' in p[len(prefix):] else '') for p in tracked(prefix)})
    for n in names:
        if n.endswith('/'):
            out += cover(prefix + n)
        elif prefix + n not in ALLOWED:
            out.append(prefix + n)
    return out


for p in (WEI, WHC, RIA, NH, DWG):
    assert in_head(p) and (not DISK or os.path.exists(p)), ('no existe', p)
NEW_FILES = [p for p in ALLOWED if p not in (WEI, WHC, RIA, NH, DWG)]  # los nuevos de src/ (la interfaz, Draft/) y de tests/ (B-08: con la v4.2, uno mas)
assert all(not in_head(p) and (not DISK or not os.path.exists(p)) for p in NEW_FILES), [p for p in NEW_FILES if in_head(p)]
SRCF = cover('src/')
for a in ALLOWED:
    assert not any(a == f or (f.endswith('/') and a.startswith(f)) for f in SRCF), a
for p in tracked('src/'):
    assert p in ALLOWED or any(p == f or (f.endswith('/') and p.startswith(f)) for f in SRCF), ('sin cubrir', p)
assert {'src/RackCad.UI/', 'src/RackCad.Domain/'} <= set(SRCF), 'INV-PBT4A-01: la UI y Domain enteros'
# tests/ (section 6.0): las del contrato de PB-T3c, mas sus dos clases (las de PB-T3c), menos PushBackDraftWriteGuardTests.cs (el Skip: en el alcance)
T3C_TESTS = [f for f in pbt3c['AllowedWriteScope'] if f.startswith('tests/')]
assert [os.path.basename(f) for f in T3C_TESTS] == ['PushBackViewEraseGuardTests.cs', 'PushBackWritePathPluginGuardTests.cs'], T3C_TESTS
CORE_T3C = [f for f in pbt3c['ForbiddenWriteScope'] if f.startswith(TR)]
UI_T3C = [f for f in pbt3c['ForbiddenWriteScope'] if f.startswith(UT)]
assert len(CORE_T3C) == 39 and len(UI_T3C) == 69 and CORE_T3C.count(DWG) == 1, (len(CORE_T3C), len(UI_T3C))
CORE_T = [f for f in CORE_T3C if f != DWG] + T3C_TESTS
UI_T = UI_T3C
TESTF = CORE_T + UI_T
assert len(CORE_T) == 40 == len(set(CORE_T)) and len(UI_T) == 69
assert not any(a == t or (t.endswith('/') and a.startswith(t)) for a in ALLOWED for t in TESTF), 'una ruta del alcance cae en lo prohibido de tests/'
ROOT = [f for f in pbt3c['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
assert len(ROOT) == 17 and ROOT[0] == 'docs/' and ROOT[-1] == '.gitignore', ROOT
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == len(set(FORBIDDEN)), 'duplicados en el ForbiddenWriteScope'
for f in FORBIDDEN:
    assert in_head(f) and (not DISK or os.path.exists(f.rstrip('/'))), ('no existe', f)
assert '- **salvo** las rutas de `tests/` del `AllowedWriteScope`, que se sacan de la lista (en PB-T4a, `PushBackDraftWriteGuardTests.cs`, una clase de ' \
       'PB-T3, por el `Skip`). Si una de ellas cae dentro de un prefijo prohibido, el prefijo se sustituye por la lista explicita de sus archivos menos esa, ' \
       'como en PB-T3b;' == ascii_fold_line([l for l in SEC60 if l.startswith('- **salvo** las rutas de `tests/`')][0]), 'la regla del ForbiddenWriteScope de section 6.0'

# ---------------------------------------------------------------- las anclas de PB-T4a en la revision (archivo, linea o None si va por contenido, texto)
EIG = TR + 'Workspace/WorkspaceEditIntentGuardTests.cs'
ANCHORS = [
    (WEI, 37, 'private EditIntentReadResult(EditIntentReadStatus status, string? reason, PushBackPilotIntentResult? pilotRead)'),
    (WEI, 51, 'public static EditIntentReadResult FromPilot(PushBackPilotIntentResult pilotRead)'),
    (RIA, 30, 'internal sealed class RackPilotIntentReadResult'), (RIA, 94, 'internal static RackPilotIntentReadResult Read('),
    (NH, 196, 'private sealed class PilotIntentReader : IWorkspaceEditIntentReader'),
    (NH, 220, 'private EditIntentReadResult ReadOnActiveDocument(SessionId session, string rackId, long? chosenReference, EditIntentWitness witness)'),
    (DWG, 60, '[Fact]'), (DWG, 61, 'public void CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes()'),
    (EIG, 139, 'Assert.Matches(@"internal\\s+static\\s+RackPilotCatalogResult\\s+LoadCatalog'),
    (EIG, 141, '@"internal\\s+static\\s+RackPilotIntentReadResult\\s+Read\\s*\\(\\s*Document\\s+document'),
    (EIG, 357, 'Assert.Equal(1, Src.Count(code, @"PushBackPilotIntentReader\\.Read\\s*\\("));'),  # B-08 (section 16 de la revision)
    (RIA, 121, 'var read = PushBackPilotIntentReader.Read(input, catalog);'),
]
TEXT = {}
for path, line, text in ANCHORS:
    if path not in TEXT:
        TEXT[path] = lines_of(path)
    assert text in TEXT[path][line - 1], ('ancla', path, line, TEXT[path][line - 1].strip())
# B-08: EIG:357 cuenta una sola llamada a PushBackPilotIntentReader.Read( en todo RIA (la de RIA:121), asi que el AtOrigin del GREEN no puede llamarlo.
# Con la v4.2 la relectura va en PushBackOriginByText.ReadAt (Draft/), y AtOrigin solo la llama. En la v4.1 es un AVISO; desde la v4.2, error.
assert '\n'.join(TEXT[RIA]).count('PushBackPilotIntentReader.Read(') == 1, 'RIA nombra PushBackPilotIntentReader.Read( una vez en la base'
B08_CALL = 'PushBackOriginByText.ReadAt('


def csharp_blocks(lines):
    out, cur = [], None
    for l in lines:
        if cur is None and l.strip() == '```csharp':
            cur = []
        elif cur is not None and l.strip() == '```':
            out.append('\n'.join(cur))
            cur = None
        elif cur is not None:
            cur.append(l)
    return out


_ao = [blk for blk in csharp_blocks(PKG_LINES) if 'RackPilotIntentReadResult AtOrigin(' in blk]
assert len(_ao) == 1, ('el bloque del GREEN de AtOrigin (RIA) en el paquete', len(_ao))
B08_OK = _ao[0].count('PushBackPilotIntentReader.Read(') == 0 and B08_CALL in _ao[0]
if PKG_VERSION == 'v4.1':
    soft(B08_OK, 'B-08 (section 16 de la revision): el AtOrigin del GREEN de la v4.1 llama a PushBackPilotIntentReader.Read( (EIG:357 cuenta 1 en RIA) '
                 'y no a %s; lo corrige la v4.2' % B08_CALL)
else:
    assert B08_OK, ('B-08: el AtOrigin del GREEN llama a PushBackPilotIntentReader.Read( o no llama a ' + B08_CALL, PKG_VERSION)
# las seis lecturas del anfitrion (WHC:328, :409, :503, :590, :675 y :759) y el Skip, una sola linea [Fact] que cambia
assert '\n'.join(TEXT.get(WHC) or lines_of(WHC)).count('_editIntentReader!.ReadIntent(request.Session, request.RackId, request.ChosenReference, request.Witness)') == 6
SKIP_TEXT = '[Fact(Skip = "I-64 F4B PB-T4a: sustituida por CT-PBT4A-06 de PushBackFullReadGuardTests (registro declarado superseding-characterization.md; ' \
            'decision del Owner owner-pbt4-decisions.md, pregunta 4).")]'
assert [l.strip() for l in SEC61].count(SKIP_TEXT) == 1, 'el Skip literal de section 6.1'
SKIP_LINE = '        ' + SKIP_TEXT  # con la indentacion de la linea 60 del archivo (8 espacios); el texto es el literal de section 6.1
assert TEXT[DWG][59] == '        [Fact]' and sum(1 for l in TEXT[DWG] if 'Skip' in l) == 0, 'la linea 60 de PushBackDraftWriteGuardTests.cs'

# ---------------------------------------------------------------- RequiredTests: los MinSelected con el criterio de count_tests_git.py
UIP, CP = UT + 'RackCad.UI.Tests.csproj', TR + 'RackCad.Tests.csproj'
_CLASS = re.compile(r'^\s*(?:public|internal)\s+(?:sealed\s+|static\s+|partial\s+|abstract\s+)*class\s+(\w+)')
_NS = re.compile(r'^\s*namespace\s+([\w\.]+)')


def blobs(paths):
    data = ''.join('%s:%s\n' % (REV, p) for p in paths).encode('utf-8')
    r = git('cat-file', '--batch', inp=data)
    assert r.returncode == 0, r.stderr[-300:]
    out, res, i = r.stdout, {}, 0
    for p in paths:
        nl = out.index(b'\n', i)
        head = out[i:nl].split()
        assert len(head) == 3 and head[1] == b'blob', (p, head)
        size = int(head[2])
        res[p] = out[nl + 1:nl + 1 + size]
        i = nl + 1 + size + 1
    return res


def count_static(project):
    """El criterio de count_tests_git.py (Fact = 1; Theory = sus InlineData; MemberData/ClassData aparte; Skip aparte) sobre los objetos de la
    revision, con el archivo de cada clase."""
    root = UT if project == 'UI' else TR
    ns_prefix = 'RackCad.UI.Tests' if project == 'UI' else 'RackCad.Tests'
    files = [p for p in tracked(root) if p.endswith('.cs') and '/bin/' not in p and '/obj/' not in p]
    totals, texts = {}, {}
    for path, data in blobs(files).items():
        text = data.decode('utf-8-sig').replace('\r\n', '\n').replace('\r', '\n')
        texts[path] = text
        ns, current, pending, inline = '', None, None, 0
        for line in text.split('\n'):
            m = _NS.match(line)
            if m:
                ns = m.group(1).rstrip(';')
            m = _CLASS.match(line)
            if m:
                current = m.group(1)
            s = line.strip()
            if s.startswith('[Fact'):
                pending = ('fact', 'Skip' in s)
            elif s.startswith('[Theory'):
                pending = ('theory', 'Skip' in s)
                inline = 0
            elif s.startswith('[InlineData') and pending and pending[0] == 'theory':
                inline += 1
            elif (s.startswith('[MemberData') or s.startswith('[ClassData')) and pending and pending[0] == 'theory':
                inline = -10 ** 6
            elif re.match(r'(public|internal|private)\s+.*\(', s) and pending:
                key = (ns + '.' + current) if current else ns
                kind, skip = pending
                n = 1 if kind == 'fact' else inline
                e = totals.setdefault(key, [0, 0, 0, set()])
                e[3].add(path)
                if n < 0:
                    e[2] += 1
                elif skip:
                    e[1] += n
                else:
                    e[0] += n
                pending = None
    return ns_prefix, totals, texts


# ---------------------------------------------------------------- RequiredTests (section 6.1.1): las dos clases nuevas y los 36 de PB-T3c, todos con
# ExpectRed false (CA-T3c M-01), PushBackDraftWriteGuardTests en 2 (V41-08: el Skip de CT-PBT3A-12)
def fq(p, f):
    return ('FullyQualifiedName~%s.%s' % ('RackCad.UI.Tests' if p == 'UI' else 'RackCad.Tests', f), UIP if p == 'UI' else CP)


_new = [(n, c) for n, c in (('PushBackFullReadPathTests', 5), ('PushBackFullReadGuardTests', 3))]
for (k, proj, filt, m, er), (cls, n) in zip(REQ_PKG[:2], _new):
    assert (filt, proj) == fq('Core', cls) and m == n == sum(1 for c in CTS if c[1] == cls) and er == (cls == 'PushBackFullReadPathTests'), (k, filt, m, er)
assert len(pbt3c['RequiredTests']) == 36
for (k, proj, filt, m, er), r in zip(REQ_PKG[2:], pbt3c['RequiredTests']):
    want = r['MinSelected'] - (1 if r['Filter'] == fq('Core', 'PushBackDraftWriteGuardTests')[0] else 0)
    assert (proj, filt, m, er) == (r['Project'], r['Filter'], want, False), ('la fila %d de RequiredTests frente al contrato de PB-T3c' % k, filt, m, want)
REQ = [(proj, filt, m, er) for _, proj, filt, m, er in REQ_PKG]
COUNTS, REQ_REPORT = {}, []
for _proj in ('UI', 'Core'):
    _nsp, _totals, _texts = count_static(_proj)
    for proj, filt, m, er in REQ:
        if (proj == UIP) == (_proj == 'UI'):
            _f = filt.split('~', 1)[1]
            _sel = {k: v for k, v in _totals.items() if k.startswith(_f)}
            COUNTS[filt] = (len(_sel), sum(v[0] for v in _sel.values()), sum(v[1] for v in _sel.values()), sum(v[2] for v in _sel.values()))
for proj, filt, m, er in REQ:
    classes, run, skip, member = COUNTS[filt]
    f = filt.split('.', 2)[-1] if filt.count('.') >= 2 else filt
    if filt in (fq('Core', 'PushBackFullReadPathTests')[0], fq('Core', 'PushBackFullReadGuardTests')[0]):
        assert classes == 0 and run == 0, ('la clase nueva ya existe', filt)
        kind = 'new'
    else:
        dwg = filt == fq('Core', 'PushBackDraftWriteGuardTests')[0]
        assert classes > 0 and (run - (1 if dwg else 0) >= m or member > 0), ('un filtro heredado baja de su minimo', filt, run, m)
        kind = 't3c' + (' (el Skip de CT-PBT3A-12: %d - 1 = %d)' % (run, run - 1) if dwg else '')
    REQ_REPORT.append('%-4s %-48s MinSelected=%-5d ExpectRed=%-5s medido: clases=%d ejecutables=%d skip=%d memberdata=%d  %s'
                      % ('UI' if proj == UIP else 'Core', filt.split('~', 1)[1].split('.', 1)[1], m, er, classes, run, skip, member, kind))
assert sum(1 for r in REQ if r[3]) == 1 and len(REQ) == 38
for proj, _, _, _ in REQ:
    assert in_head(proj), proj


# ---------------------------------------------------------------- las paradas: las 17 estandar del contrato de PB-T3c y las 17 propias (sin C15)
P_STOPS = [dict(s) for s in pbt3c['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
assert len(P_STOPS) == 17 and P_STOPS == [dict(s) for s in pbt3c['StopConditions'][:17]]
assert STOP_OWN_TEXT['PBT4A-C11'] == C11, 'PBT4A-C11 del paquete no es PBT3C-C11 con PBT3C -> PBT4 y PB-T3c -> PB-T4a'
for _cid in ('C08', 'C12', 'C17'):  # [DP] y el limite de D2: los mismos textos que en PB-T3c
    assert STOP_OWN_TEXT['PBT4A-' + _cid] == T3C_STOP['PBT3C-' + _cid], ('la parada %s del paquete difiere de la de PB-T3c' % _cid)
STOPS = P_STOPS + [{'Id': cid, 'Text': STOP_OWN_TEXT[cid]} for cid in _ids]
assert len(STOPS) == 34 and len({s['Id'] for s in STOPS}) == 34
# ---------------------------------------------------------------- la evidencia: [0] la de PB-T3c y del sustituto de nc2; [14] el total exacto de la rama
assert EVID[0] == EVID0 and EVID[0] in NC2_EVIDENCES, 'la primera evidencia no es la de PB-T3c o no esta en F4B_EXACT_EVIDENCES del v2'
_sp = lambda v: format(v, ',').replace(',', ' ')
_base_pass = _core - _cskip
RED_T = (_core + N_CT, _base_pass - 1 + (N_CT - N_RED), N_RED, _cskip + 1)  # un Skip de un [Fact]: un Passed menos y un Skipped mas
GREEN_T = (_core + N_CT, _base_pass - 1 + N_CT, 0, _cskip + 1)
assert RED_T == (19137, 19125, 5, 7) and GREEN_T == (19137, 19130, 0, 7), (RED_T, GREEN_T)
for _frag in ('run %s sobre el GREEN %s, Core %s resultados, con %s Passed y %d Skipped' % (_run, _sha, _sp(_core), _sp(_base_pass), _cskip),
              'UI %s, con %s Passed y %d Skipped' % (_sp(_ui), _sp(_ui - _skip), _skip),
              'El RED: Core %s resultados, con %s Passed, %d Failed (CT-PBT4A-01 a 04 y 07) y %d Skipped.' % (_sp(RED_T[0]), _sp(RED_T[1]), RED_T[2], RED_T[3]),
              'El GREEN: Core %s, con %s Passed, 0 Failed y %d Skipped.' % (_sp(GREEN_T[0]), _sp(GREEN_T[1]), GREEN_T[3]),
              'UI, en los dos: %s, con %s Passed y %d Skipped.' % (_sp(_ui), _sp(_ui - _skip), _skip),
              'PB-T4a anade ocho [Fact] y un Skip de un [Fact], sin ninguna Theory, asi que hay una sola rama.'):
    assert EVID[14].count(_frag) == 1, ('la linea base y lo previsto de la CI ([14]) no cuadran con CI_BASE_T3C y la lista de section 5.1', _frag)
assert ' o ' not in EVID[14].split('Previstos,')[1], 'lo previsto de la CI lleva una alternativa: F4B-E46 pide el total de cada rama'
assert EVID[13].startswith('la entrada "## PB-T4a" de superseding-characterization.md') and 'CT-PBT4A-06' in EVID[13]

# ---------------------------------------------------------------- autoridades
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
PKT1 = 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md'
PKT3 = 'docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md'
SUPERSEDING = PILOT + '/F4B-DRAFTS/superseding-characterization.md'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
_dids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
DISP_LAST = 'F4B-E%02d' % _dids[-1]
BOUNDED_CHECK = D4 + 'independent-review-pbt4a-check.md'
SUBST_LISTS = D4 + 'pbt4-substitution-lists.md'
# CA-T4a M-02: el rango del registro de mediciones se lee del registro (en la revision; en seco, sin el en la revision, el pbt4-measurements.md de la
# carpeta de --package-file), con sus R-n consecutivos desde R-0, y se comprueba contra la cabecera del paquete ("R-0 a R-<n>")
if in_head(MEAS):
    _meas, MEAS_SRC = show(MEAS), MEAS + ' en ' + REV_SHA[:8]
else:
    _mp = os.path.join(os.path.dirname(PKG_FILE), 'pbt4-measurements.md') if PKG_FILE else None
    assert DRY and _mp and os.path.isfile(_mp), ('el registro de mediciones no esta en la revision ni junto a --package-file', MEAS)
    _meas, MEAS_SRC = open(_mp, 'rb').read().decode('utf-8'), 'pbt4-measurements.md junto a --package-file'
_rn = [int(n) for n in re.findall(r'(?m)^## R-(\d+)\. ', _meas.replace('\r\n', '\n'))]
assert _rn and _rn == list(range(len(_rn))), ('los R-n del registro de mediciones no son R-0..R-n consecutivos', MEAS_SRC, _rn[-5:])
MEAS_LAST = _rn[-1]
assert ('(R-0 a R-%d;' % MEAS_LAST) in '\n'.join(PKG_LINES), ('la cabecera del paquete no cita el registro de mediciones hasta su ultimo R', MEAS_LAST)
CUSTODY = [
    (PKG, 'archivo completo (' + PKG_VERSION + ', sha256 ' + CUSTODY_SHA256[PKG][:8] + '): D-PBT4-02 y D-PBT4-03 (el texto literal del GREEN); '
          'section 4.1; section 5.0 y 5.1 (CT-PBT4A-01 a 08, un [Fact] cada una); section 6.0 (identidad, paradas, RR y la tabla de RR-3) y 6.1 '
          '(el contrato de PB-T4a, sus literales de 6.1.1)'),
    (MEAS, 'el registro de mediciones de PB-T4 (R-0 a R-%d)' % MEAS_LAST),
    (OWNER_DEC, 'archivo completo (' + OWNER_ENTRY + '): las cuatro decisiones del Owner sobre PB-T4, literales; la pregunta 4, la emision condicional de '
                'PB-T4a, PB-T4b y PB-T4c en ese orden, que autoriza esta emision'),
    (OWNER_DEC2, 'archivo completo (' + OWNER_ENTRY + '): las dos decisiones del Owner sobre PB-T4b, literales: el codigo del piloto se retira en una '
                 'subtarea nueva, PB-T4d, despues de PB-T4c, que vuelve al Owner antes de emitirse; la lista de PB-T4b son 470 pruebas, la que vio; '
                 'PB-T4a y PB-T4c, sin cambios'),
    (SUBST_LISTS, 'la lista de sustituciones de PB-T4: la de PB-T4a, CT-PBT3A-12 (CT_PBT3A_12_NoSourceFileOutsideTheFolderOfTheModelNamesItsTypes)'),
    (BOUNDED_CHECK, 'la comprobacion acotada de PB-T4a (CA-T4a), sobre el contrato en seco y la ' + PKG_VERSION),
    (SUPERSEDING, 'el registro declarado de las sustituciones: su entrada de PB-T4a (el Skip de CT-PBT3A-12, su motivo literal y CT-PBT4A-06)'),
]
CUSTODY_HEADS = {SUPERSEDING: ['## PB-T4a ']}
EXISTING = [
    (PHASE, 'paquete de la fase F4B v3.1 ([P]): la fila de PB-T4 del section 5; OB-01.1 a 6, OB-03.2, OB-05.1 a 5, R-PB-10; section 7.3 y 7.4'),
    (CO + 'owner-f4b-disposition.md', 'archivo completo ([OD]): la fila de PB-T4, D-PB-01, D-PB-05 y D-PB-10; el bloque sobre T1B (el registro declarado)'),
    (PKT1, 'el paquete de PB-T1 ([PT1]): A-5, A-9, A-10 y R-T1-17; section 6.0 (C08, C12 y C17)'),
    (PKT3, 'el paquete de PB-T3 v4.7 ([P3]): section 6.0 (RR) y section 10 (A-T3-1 a A-T3-11; A-T3-8 es el que aplica PB-T4a)'),
    (CO + 'owner-d1-d2-decisions.md', 'archivo completo ([OD2], F4B-E11): D2 con sus diez condiciones y su limite, el texto literal de PBT4A-C17'),
    (CO + 'owner-f4b-procedure-decisions.md', 'archivo completo ([DP], F4B-E03): firmas exactas de los sustitutos y la replanificacion unica'),
    (CO + 'owner-pbt1a-nc1-decisions.md', 'archivo completo (F4B-E15): las tres comprobaciones deterministas de la verificacion'),
    (SNAP_K_DEC, 'archivo completo (F4B-E37): la Snapshot K (' + SNAP_K + ', ConfigSha256 ' + SNAP_K_SHA[:8] + '), la que cita P-01'),
    (CO + 'owner-f4b-opaque-collections-decision.md', 'archivo completo (F4B-E21): la sesion pone byte a byte las ocho colecciones opacas del contrato'),
    (CO + 'owner-pbt1b-rework-decisions.md', 'archivo completo (F4B-E22): el texto correcto de INV-13, C13 y la CI del RED'),
    (CO + 'owner-f4b-correction-unit-change-decision.md', 'archivo completo (F4B-E25): las rutas UNIT_CHANGE en una correccion; PB-T4a declara WEI, WHC, RIA y NH'),
    (CO + 'owner-pbt3-d1-decision.md', 'archivo completo (F4B-E29): D-1, la variante (A)'),
    (CO + 'owner-f4b-pbt3-emission-red-repair-decisions.md', 'archivo completo (F4B-E35): pregunta 4, la regla de la reparacion del RED, RR-1 a RR-7, '
                                                            'con el tramo RedSha..RedRepairSha que en PB-T4a no toca src/ ni PushBackDraftWriteGuardTests.cs'),
    (CO + 'owner-pbt2c-nc1-decision.md', 'archivo completo (F4B-E40): la firma nueva de nc1 PD-F4B-01 y su sustituto, el de PBT4A-C11'),
    (NC1_SUBST, 'el sustituto de nc1 de PD-F4B-01 (F4B-E40), SHA-256 ' + NC1_SUBST_SHA + ', revisado; PBT4A-C11 lo usa con --task PBT4'),
    (CO + 'owner-pbt3a-nc2-decision.md', 'archivo completo (F4B-E43): el sustituto de nc2 ampliado (PBT4A-C11)'),
    (NC2_V2, 'el sustituto de nc2 v2 (F4B-E43), SHA-256 ' + NC2_V2_SHA + ': el de PBT4A-C11, con --task PBT4 y su lista cerrada de literales'),
    (NC2_V2_LIMITS, 'la version revisada del sustituto de nc2 v2 y sus limites declarados (M-V1 a M-V4)'),
    (NC2_V2_REVIEW, 'la revision independiente del sustituto de nc2 y su seccion "Recomprobacion v2": APTO, B 0 / I 0 / M 4'),
    (CO + 'owner-pbt3b-overload-decision.md', 'archivo completo (F4B-E45): la leccion de las sobrecargas y la reflexion (el barrido de PB-T4a)'),
    (CO + 'owner-pbt3b-nc3-decision.md', 'archivo completo (F4B-E46): una evidencia esperada que dependa de como cuente la TRX lleva el total exacto de '
                                         'cada rama ([14] de este contrato, en su unica rama)'),
    (DISP, 'archivo completo: F4B-E01 a ' + DISP_LAST),
    (PBT3CC, 'el contrato de PB-T3c (F4B-E47): las listas de tests/, los filtros y sus MinSelected, las celdas, las paradas estandar, PBT3C-C11 (que '
             'este contrato lleva con PBT4) y la primera ExpectedEvidence'),
    (PILOT + '/F4-DRAFTS/f4-dispositions.md', 'las fuentes de PD-F4-01 y PD-F4-04'),
    ('docs/initiatives/I-64-proposal-v5.md', 'la Proposal V5 congelada de I-64: D-10, D-13, D-15 (punto 5) y D-16'),
    ('docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md', 'el paquete de PB-TC ([PKC-C]): el algoritmo del ForbiddenWriteScope'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 'el paquete de PB-TC2 ([PKC2]): las 17 paradas estandar y la raiz; las celdas (:655)'),
    ('docs/automation/state/I-64.yml', 'estado vigente completo'),
    ('docs/automation/evidence/I-64-evidence.md', 'las secciones de F4B (231 y siguientes)'),
]
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D12 y D13'),
    ('docs/AUTOMATION_PLAN.md', 'section 16.3, 16.5, 16.8, 16.9, 16.10 y 16.11'),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4)'),
    ('docs/automation/agent-execution/routing.md', 'archivo completo'),
    ('docs/automation/agent-execution/model-catalog.md', 'celdas medidas y vigentes para Controller y Worker'),
]
CLOSE_AUTH = [(PT3C_TCP, 'el cierre de PB-T3c (' + T3C_CLOSE_ENTRY + '): la base de PB-T4a, con su linea base de la CI (' + _run + ')')]
UNIT_CHANGE = [(WEI, 'el resultado de la lectura de la intencion: FullRead, Catalog y FromReads (D-PBT4-02; section 6.0 del paquete, F4B-E25)'),
               (WHC, 'el anfitrion del Workspace: la sentencia en linea de la relectura con el origen en cinco cuerpos (D-PBT4-03)'),
               (RIA, 'el adaptador de la lectura del Plugin: el lector completo sobre el mismo escaneo y AtOrigin (D-PBT4-02 y 03)'),
               (NH, 'el anfitrion de la navegacion del Plugin: ReadIntentAtOrigin en el lector anidado y FromReads (D-PBT4-02 y 03)')]
_auth_all = CUSTODY + EXISTING + CLOSE_AUTH + UNIT_CHANGE + EXTERNAL
assert len({p for p, _ in _auth_all}) == len(_auth_all), 'una autoridad repetida'
for p, _ in EXISTING + EXTERNAL + UNIT_CHANGE + CLOSE_AUTH:
    assert in_head(p), ('autoridad inexistente en la revision', p)
cells = json.loads(json.dumps(pbt3c['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna'] and all('high' in c['TestedEfforts'] for c in cells)
c = {
    'Schema': 'rackcad-gate-contract/v1', 'Unit': 'I-64', 'Gate': 'F4B', 'TaskId': TASK,
    'Objective': OBJECTIVE,
    'AuthorityRevision': AR, 'MainSha': MAIN,
    'Authorities': [{'Path': p, 'Section': s, 'Class': 'UNIT_DOC'} for p, s in CUSTODY + EXISTING + CLOSE_AUTH] + [
        {'Path': CONTRACT_REL, 'Section': 'este contrato', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_CHANGE'} for p, s in UNIT_CHANGE]
      + [{'Path': p, 'Section': s, 'Class': 'EXTERNAL'} for p, s in EXTERNAL],
    'AllowedWriteScope': ALLOWED,
    'ForbiddenWriteScope': FORBIDDEN,
    'Invariants': INV,
    'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
    'ExpectedEvidence': EVID, 'StopConditions': STOPS, 'EligibleCells': cells, 'RoutingEnforcement': 'required', 'CorrectionsAuthorized': True,
    'IssuedBy': ('Sesion principal de I-64 (Claude), emision de la primera subtarea de PB-T4 (' + EMIT_ENTRY + ') por ' + OWNER_ENTRY + ', pregunta 4 (la '
                 'emision condicional de PB-T4a, PB-T4b y PB-T4c en ese orden; owner-pbt4-decisions.md): PB-T4a tras el cierre de PB-T3c (' + PT3C_CLOSE[:8]
                 + ', ' + T3C_CLOSE_ENTRY + '; CLOSED / PASS WITH LIMITATIONS sobre ' + GREEN_T3C[:8] + ', intento 1 de 3), con su comprobacion acotada ('
                 + (BOUNDED_RESULT or '<resultado por rellenar>') + '); F4B-E46, el total exacto de cada rama; F4B-E45, las sobrecargas y la reflexion; '
                 'F4B-E40 y F4B-E43, los sustitutos de nc1 y nc2 de PBT4A-C11; F4B-E29, D-1; [OD]; F4B-E11, D2; F4B-E03; F4B-E37, la Snapshot K; '
                 'F4B-E35, pregunta 4, la regla RR; F4B-E21; F4B-E22; F4B-E25, con WEI, WHC, RIA y NH; y el paquete del contrato de PB-T4 ' + PKG_VERSION
                 + ', con su comprobacion acotada de PB-T4a'),
    'IssuedUtc': ISSUED,
}
N_AUTH = len(_auth_all) + 1
assert len(c['Authorities']) == N_AUTH, len(c['Authorities'])
b = (json.dumps(c, indent=2, ensure_ascii=True) + '\n').encode('ascii')
assert all(x < 0x80 for x in b) and b'\\u' not in b, 'no ASCII'
assert not set(c['AllowedWriteScope']) & set(c['ForbiddenWriteScope'])
for a in c['AllowedWriteScope']:
    assert not any(a.startswith(f) for f in c['ForbiddenWriteScope'] if f.endswith('/')), a
# ---- rr3_table_check (inicio)
def rr3_table_check(sec_lines, classes, sub='PB-T4a'):
    """La tabla cerrada de la ayuda de RR-3 en sus filas de PB-T3c: una fila por cada clase nueva (classes) y ninguna otra; cada fila, en partes
    '**Lineas:**' o '**Valores:**', con sus literales entre comillas invertidas, fuera de parentesis o dentro de uno que solo los enumera; '\\|' de la
    tabla es '|' en el literal. Una fila que dice que sus literales "se enumeran ... en la emision" (la de CT-05) queda APLAZADA: es el marcador RR3_CT05
    del generador. Devuelve (errores, info): info[clase] = (lineas, valores) o None si esta aplazada."""
    import re, unicodedata
    fold = lambda s: ''.join(c for c in unicodedata.normalize('NFKD', s) if not unicodedata.combining(c))

    def scan(seg):
        lits, groups, stack, gid, buf, inlit = [], {}, [], 0, '', False
        for ch in seg:
            if inlit:
                if ch == '`':
                    lits.append((buf, stack[-1] if stack else None))
                    buf, inlit = '', False
                else:
                    buf += ch
                continue
            if ch == '`':
                inlit = True
            elif ch == '(':
                gid += 1
                stack.append(gid)
                groups[gid] = ''
            elif ch == ')':
                if stack:
                    stack.pop()
            elif stack:
                groups[stack[-1]] += ch
        return [x.replace('\\|', '|') for x, g in lits if g is None or re.fullmatch(r'[\s,y]*', groups[g])]

    errors, info, seen = [], {}, []
    for row in [l for l in sec_lines if l.startswith('| %s | `' % sub)]:
        cells = row.strip()[2:-2].split(' | ') if row.strip().endswith(' |') else []
        cm = re.match(r'`(\w+)` \((?:CT-|el kit|el doble )', cells[1]) if len(cells) == 3 else None  # PB-T4a: la fila del kit y el doble
        if not cm:
            errors.append('una fila de %s de la tabla de RR-3 sin la forma "| %s | `Clase` (CT-...) | ... |": %s' % (sub, sub, ascii(row[:80])))
            continue
        cls, body = cm.group(1), cells[2]
        seen.append(cls)
        if re.search(r'Se enumeran con su referencia .* en la emisi\S+n de PB-T4a', body):
            info[cls] = None
            continue
        lines, values = [], []
        for seg in [s.strip() for s in body.split('<br>')]:
            lm = re.match(r'\*\*([LlVv])(?:ineas|alores):\*\* ', fold(seg))
            if not lm:
                errors.append('una parte de la fila de %s no empieza por "Lineas:" o "Valores:": %s' % (cls, ascii(fold(seg)[:70])))
                continue
            items = scan(seg)
            if not items:
                errors.append('la parte de %s no da ningun literal' % cls)
            for x in items:
                if re.search(r'(?:^|[\s\[=])<\w+>', x) or chr(0x2026) in x:
                    errors.append('un literal de %s no se enumera sin interpretar: %s' % (cls, ascii(x)))
                elif lm.group(1) in 'Vv':
                    if not (re.fullmatch(r'"[^"]+"', x) or (re.fullmatch(r'\d+(?:\.\d+)?', x) and len(x) >= 2)):
                        errors.append('un valor de %s que no se puede contar como token: %s' % (cls, ascii(x)))
                    values.append(x)
                else:
                    lines.append(x)
        info[cls] = (lines, values)
    if sorted(seen) != sorted(classes):
        errors.append('las filas de %s de la tabla de RR-3 no son, una por clase, las clases nuevas: %s' % (sub, ascii(seen)))
    return errors, info
# ---- rr3_table_check (fin)


# ---- bounded_check (inicio)
def bounded_check(report, result, pkg_lines, reviewer='CA-T3a'):
    """La comprobacion acotada de PB-T3a custodiada (CA-T3a) frente a BOUNDED_RESULT y al paquete (F4B-E35, pregunta 3: 0 bloqueantes y 0
    importantes SIN RESOLVER). Solo vale el vocabulario de la comprobacion acotada, la familia EMITIBLE: 'EMITIBLE' o 'EMITIBLE TRAS CORRECCIONES'
    (DU-18, decidida por la sesion). 'LISTO PARA EMITIR...' es el de la revision del paquete, otro informe, y aqui es un error. Admite las dos
    formas de la linea: la de PB-T2 ('**Veredicto: EMITIBLE... B n / I n / M n.**') y la de las revisiones de PB-T3 ('**Veredicto: ....** B/I/M =
    **n / n / n**'). Las recomprobaciones van por secciones '## <seccion>id. Recomprobacion (sobre|de) la vX.Y', con id numero (el informe de
    CA-T3a las numera) o letra, y su veredicto dentro de su seccion, en la forma 'Veredicto de <seccion>id: ... Nuevos: B n / I n / M n.' o en
    la de PB-T3; la ultima tiene que dar EMITIBLE con 0 bloqueantes y 0 importantes, y BOUNDED_RESULT la nombra ('recomprobacion vX.Y:
    <veredicto>, B n / I n / M n'). Los veredictos se comparan sin tildes (BOUNDED_RESULT es ASCII). Lo que no casa da un error claro, sin
    excepciones. Devuelve los errores (vacia si vale)."""
    import re, unicodedata
    sec, o = chr(0xA7), chr(0xF3)
    fold = lambda s: ''.join(c for c in unicodedata.normalize('NFKD', s) if not unicodedata.combining(c))
    result = result or ''
    ok_verdict = lambda v: v in ('EMITIBLE', 'EMITIBLE TRAS CORRECCIONES')
    why = lambda v: (' ("LISTO PARA EMITIR..." es el vocabulario de la revision del paquete, no el de la comprobacion acotada: solo EMITIBLE o '
                     'EMITIBLE TRAS CORRECCIONES)' if v.startswith('LISTO') else ' (solo EMITIBLE o EMITIBLE TRAS CORRECCIONES)')
    # anclados al principio de la linea: un veredicto citado dentro de una vineta (como '`**Veredicto de <seccion>R: ...**`' en la seccion 9 de
    # CA-T2c) no es un veredicto
    v_t2 = r'(?m)^\*\*Veredicto: ([A-Z][^.*\n]*)\. B (\d+) / I (\d+) / M (\d+)\.\*\*'
    v_t3 = r'(?m)^\*\*Veredicto: ([A-Z][^*\n]*?)\.\*\* B/I/M = \*\*(\d+) / (\d+) / (\d+)\*\*'
    lines = report.replace('\r\n', '\n').split('\n')
    heads = [k for k, l in enumerate(lines) if l.startswith('## ') and ('Recomprobaci' + o + 'n') in l]
    first = '\n'.join(lines[:heads[0]] if heads else lines)
    m = re.search(v_t2, first) or re.search(v_t3, first)
    if not m:
        return ['la comprobacion acotada no tiene su veredicto, en la forma "**Veredicto: ... B n / I n / M n.**" o "**Veredicto: ....** B/I/M = '
                '**n / n / n**"']
    verdict, b, i, mm = fold(m.group(1)), int(m.group(2)), int(m.group(3)), int(m.group(4))
    errors = []
    if not ok_verdict(verdict):
        errors.append('el veredicto de la comprobacion acotada no es emitible: %s%s' % (ascii(verdict), why(verdict)))
    if b:
        errors.append('la comprobacion acotada tiene %d bloqueantes' % b)
    if not result.startswith(verdict) or ('B %d / I %d / M %d' % (b, i, mm)) not in result:
        errors.append('BOUNDED_RESULT no es el veredicto de la comprobacion acotada: %s frente a %s' % (ascii(result), ascii(m.group(0))))
    if i:
        for x in ['I-%02d' % k for k in range(1, i + 1)]:
            if not re.search(r'\|\s*\**%s\**\s*\|\s*I\s*\|' % x, report):
                errors.append('la comprobacion acotada no tiene la fila de ' + x)
            if not [l for l in pkg_lines if re.match(r'^\| \**V\d+-\d\d\** \|', l) and ('**%s %s**' % (reviewer, x)) in l]:
                errors.append('ningun registro de cambios del paquete corrige %s de %s' % (x, reviewer))
        if 'corregidas en el paquete' not in result:
            errors.append('BOUNDED_RESULT no dice que las importantes estan corregidas en el paquete')
    h_rx = r'^## ' + sec + r'([^\s.:*]+)\. Recomprobaci' + o + r'n (?:sobre|de) la (v\d+(?:\.\d+)?)\b'
    rechecks = []
    for n, k in enumerate(heads):
        hm = re.match(h_rx, lines[k])
        if not hm:
            errors.append('un encabezado de recomprobacion no tiene la forma "## <seccion>id. Recomprobacion (sobre|de) la vX.Y" (id, numero o letra): %s'
                          % ascii(lines[k][:80]))
            continue
        end = next((j for j in range(k + 1, len(lines)) if lines[j].startswith('## ')), len(lines))
        body = '\n'.join(lines[k + 1:end])
        vm = (re.search(r'(?m)^\*\*Veredicto de ' + sec + re.escape(hm.group(1)) + r': ([A-Z][^.*\n]*)\. Nuevos: B (\d+) / I (\d+) / M (\d+)\.\*\*',
                        body)
              or re.search(v_t3, body))
        if not vm:
            errors.append('la recomprobacion %s no tiene su veredicto en su seccion, en ninguna de las dos formas' % ascii(sec + hm.group(1)))
            continue
        others = re.findall(r'(?m)^\*\*Veredicto de ' + sec + r'([^\s:*]+):', body)
        if [x for x in others if x != hm.group(1)]:
            errors.append('no se puede emparejar la recomprobacion %s con su veredicto: %s' % (ascii(sec + hm.group(1)), ascii(others)))
            continue
        rechecks.append((hm.group(1), hm.group(2), fold(vm.group(1)), int(vm.group(2)), int(vm.group(3)), int(vm.group(4))))
    if rechecks and len(rechecks) == len(heads):
        rid, ver, v, rb, ri, rm = rechecks[-1]
        if rb or ri or not ok_verdict(v):
            errors.append('la ultima recomprobacion (%s) da %s con %d bloqueantes y %d importantes%s'
                          % (ascii(sec + rid), ascii(v), rb, ri, '' if ok_verdict(v) else why(v)))
        want = 'recomprobacion %s: %s, B %d / I %d / M %d' % (ver, v, rb, ri, rm)
        if want not in result:
            errors.append('BOUNDED_RESULT no nombra la ultima recomprobacion: falta %s' % ascii(want))
    return errors
# ---- bounded_check (fin)


# ---- local_paths (inicio)
def local_paths(text):
    """Las rutas locales de un archivo a custodiar (DU-11; el repositorio es publico): una ruta con letra de unidad (X seguida de dos puntos y una
    barra), la carpeta de perfiles de Windows seguida de una barra, o la carpeta de datos de aplicacion del perfil. No busca nombres de personas:
    eso lo cubre la redaccion comun. Devuelve las coincidencias distintas (vacia si no hay). Las palabras de la expresion van con una letra entre
    corchetes ([e], [D]): la misma expresion, sin que su texto case con FORBIDDEN de la redaccion comun cuando el generador se custodia."""
    import re
    return sorted(set(m.group(0) for m in re.finditer(r'(?<![A-Za-z0-9])[A-Za-z]:[\\/]|\bUs[e]rs[\\/]|\bApp[D]ata\b', text)))
# ---- local_paths (fin)


# ---------------------------------------------------------------- la tabla de RR-3 de section 6.0: una fila por clase nueva de PB-T4a, sin aplazar
RR3_CLASSES = ['PushBackFullReadPathTests', 'PushBackFullReadGuardTests', 'PushBackFullReadSupport']
_err, RR3_INFO = rr3_table_check(SEC60, RR3_CLASSES)
assert not _err, ('la tabla de RR-3 del section 6.0, filas de PB-T4a', _err)
assert [k for k, v in RR3_INFO.items() if v is None] == [], 'una fila de PB-T4a de la tabla de RR-3 aplazada'
RR3_SUMMARY = '; '.join('%s %d lineas y %d valores' % (k, len(v[0]), len(v[1])) for k, v in RR3_INFO.items())
_s60 = '\n'.join(SEC60)
for _k in range(1, 7):  # RR-1 a RR-6, cada una con su apartado; RR-7 (no gasta intento), en el parrafo de la regla
    assert ('**RR-%d, ' % _k) in _s60, ('RR-%d no tiene su apartado en section 6.0' % _k)
assert '(RR-7)' in _s60, 'RR-7 no esta en section 6.0'

# ---------------------------------------------------------------- la comprobacion acotada de PB-T4a (CA-T4a): BOUNDED_RESULT y bounded_check
soft(BOUNDED_RESULT, 'emision: falta BOUNDED_RESULT, el veredicto literal de la comprobacion acotada de PB-T4a (CA-T4a)')
assert BOUNDED_RESULT is None or re.match(r'EMITIBLE(?: TRAS CORRECCIONES)?[.,;]', BOUNDED_RESULT), ('BOUNDED_RESULT: solo la familia EMITIBLE', BOUNDED_RESULT)
BOUNDED_SRC = None
if BOUNDED_FILE:
    _rep = open(BOUNDED_FILE, 'rb').read().decode('utf-8')
    BOUNDED_SRC = '--bounded-file ' + os.path.basename(BOUNDED_FILE)
elif in_head(BOUNDED_CHECK):
    _rep = show(BOUNDED_CHECK)
    BOUNDED_SRC = BOUNDED_CHECK + ' en ' + REV_SHA[:8]
if BOUNDED_SRC:
    if BOUNDED_RESULT:
        _err = bounded_check(_rep, BOUNDED_RESULT, PKG_LINES, reviewer='CA-T4a')
        assert not _err, ('la comprobacion acotada de PB-T4a', BOUNDED_SRC, _err)
else:
    soft(False, 'emision: la comprobacion acotada de PB-T4a (%s) aun no existe (ni en la revision ni --bounded-file)' % BOUNDED_CHECK)
for p, _ in CUSTODY:
    if soft(in_head(p), 'emision: a custodiar con la emision de PB-T4a, aun no en la revision: %s' % p):
        _lp = local_paths(show_bytes(p).decode('utf-8', 'replace'))
        assert not _lp, ('rutas locales en un custodiado (se redacta antes de custodiar)', p, _lp)
        if p in CUSTODY_HEADS:
            _lines = lines_of(p)
            soft(all(any(l.startswith(h) for l in _lines) for h in CUSTODY_HEADS[p]), 'emision: al custodiado le faltan apartados (%s): %s'
                 % (', '.join(CUSTODY_HEADS[p]), p))
# el sha256 del paquete (PKG_VERSION), al final: las comprobaciones de contenido de arriba fallan antes por su motivo (las negativas lo usan)
assert hashlib.sha256(PKG_RAW).hexdigest() == CUSTODY_SHA256[PKG], ("el paquete leido no es la %s revisada (sha256)" % PKG_VERSION)
if DRY:
    print('DRY rev', REV_SHA[:8], '(objetos del checkout principal)' if OBJ_REV else '(HEAD del worktree)', 'allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN),
          '(src', len(SRCF), 'tests', len(TESTF), '[core', len(CORE_T), 'ui', str(len(UI_T)) + '] root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'required', len(REQ), 'evidence', len(EVID), 'authorities', len(c['Authorities']), 'anchors-ok', len(ANCHORS), 'bytes', len(b),
          'sha256', hashlib.sha256(b).hexdigest().upper())
    print('anclaje: desciende de %s (F4B-E29), %s (F4B-E46), %s (%s) y el cierre %s (%s); src y tests == el cierre == el GREEN %s; la CI %s (Core %d con %d '
          'Skipped; UI %d con %d) | f4b-dispositions hasta %s' % (D1_REV[:8], PT3B_CLOSE[:8], PT3C_EMIT[:8], T3C_EMIT_ENTRY, PT3C_CLOSE[:8], T3C_CLOSE_ENTRY,
                                                                  GREEN_T3C[:8], _run, _core, _cskip, _ui, _skip, DISP_LAST))
    print('paquete leido: %s -> %s | sha256 %s | el bloque ASCII de 6.1.1: TaskId, Objective, INV-PBT4A-01..14, PBT4A-C01..C18 sin C15, [0]..[14]; la '
          'tabla de RequiredTests (38); el alcance de 6.1 (10); la lista CT-PBT4A-01..08 de 5.1 (%d CT, %d con ExpectRed)'
          % (PKG_SRC, PKG_IN, hashlib.sha256(PKG_RAW).hexdigest()[:16], N_CT, N_RED))
    print('contrato de PB-T3c: %s en %s (AuthorityRevision %s): las 17 paradas estandar, C08/C12/C17 iguales, EVID[0], los 36 filtros (DWG en 2), la raiz '
          'y las listas de tests/' % (PBT3CC, PT3C_CLOSE[:8], pbt3c['AuthorityRevision'][:8]))
    print('PBT4A-C11 == PBT3C-C11 con PBT3C -> PBT4 y PB-T3c -> PB-T4a; nc1 %s y nc2 v2 %s con --task PBT4; EVID[0] en F4B_EXACT_EVIDENCES'
          % (NC1_SUBST_SHA[:8], NC2_V2_SHA[:8]))
    print('el registro de mediciones (CA-T4a M-02): R-0 a R-%d, de %s; la cabecera del paquete lo cita igual' % (MEAS_LAST, MEAS_SRC))
    print('el Skip (6.1): %s' % SKIP_TEXT)
    print('tabla de RR-3 (filas de PB-T4a): %s' % RR3_SUMMARY)
    print('la CI prevista (F4B-E46, su unica rama): RED %s = %s + %d Failed + %d Skipped; GREEN %s = %s + 0 + %d; UI %d = %d + %d en los dos'
          % (RED_T[0], RED_T[1], RED_T[2], RED_T[3], GREEN_T[0], GREEN_T[1], GREEN_T[3], _ui, _ui - _skip, _skip))
    print('BOUNDED_RESULT: %s | bounded_check: %s' % (BOUNDED_RESULT or 'por rellenar',
                                                      (('sin errores sobre ' + BOUNDED_SRC) if BOUNDED_RESULT else ('no corre sin BOUNDED_RESULT (informe: %s)'
                                                                                                                      % BOUNDED_SRC)) if BOUNDED_SRC else 'sin informe'))
    print('RequiredTests (criterio de count_tests_git.py sobre los objetos de la revision):')
    for l in REQ_REPORT:
        print('  ' + l)
    print('AVISOS (%d):' % len(WARN))
    for w in WARN:
        print('  - ' + w)
    if JSON_OUT:
        open(JSON_OUT, 'wb').write(b)
    sys.exit(0)
assert BOUNDED_RESULT and in_head(BOUNDED_CHECK) and E49_OK and E50_OK and in_head(PKG) and hashlib.sha256(show_bytes(PKG)).hexdigest() == CUSTODY_SHA256[PKG], \
    'faltan BOUNDED_RESULT, la comprobacion acotada, F4B-E49, F4B-E50 o la %s custodiada' % PKG_VERSION
assert not WARN, WARN
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"],
                   capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']),
      'sha256', hashlib.sha256(b).hexdigest().upper())
