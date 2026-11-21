"""Contrato de gate de F4B-PBT4-EDIT-ALL-WINDOW (PB-T4c; ASCII), desde el paquete del contrato de PB-T4 (PKG_VERSION, abajo), con el patron de
make_pbt4b_contract.py, sobre el cierre de PB-T4b (PT4B_CLOSE; F4B-E53, la evidencia 282), con PB-T4c literal en 6.3, 6.3.1 y 6.3.2 del paquete.
PB-T4c se emite con F4B-E54, la evidencia 283 (X) y 284 (BASE): parametros, comprobados al emitir.
Comprueba por objetos git:
- el anclaje: la revision desciende de la emision de PB-T4b (X y BASE), de su RED, de su reparacion del RED, de su GREEN y de su cierre; src y tests,
  los del GREEN de PB-T4b; la numeracion prevista; la CI del GREEN de PB-T4b en la entrada de su cierre;
- el contrato de PB-T4b de su emision: las 17 paradas estandar, PBT4B-C11 (de la que sale PBT4C-C11), la primera ExpectedEvidence y sus filtros;
- los sustitutos de nc1 y de nc2 v2 con --task PBT4, y la Snapshot K;
- la lista de PB-T4c: las filas "PB-T4c" de pbt4-substitution-lists.tsv (por su sha256), cada prueba en su archivo de la revision, con su atributo sin
  Skip y su linea dentro del rango;
- los filtros heredados del contrato de PB-T4b con los MinSelected que dejan los Skip de la lista (con el criterio de count_tests_git.py);
- el paquete, por su sha256: la seccion 6.3 de PB-T4c (el alcance, ruta a ruta), sus textos literales (6.3.1: el Objective, los invariantes, las
  paradas, los RequiredTests y la ExpectedEvidence) y lo que necesita el generador (6.3.2: las piezas por sha256, la lista y la precondicion 6);
- la precondicion 6 de la sesion (pbt4c-precondition6.json; --p6-file en seco, de la revision al emitir): la lista, las piezas, EquivalentSha, D y E y los
  totales sin las CT; las claves de la v5.1 (el ensayo, su control, el censo y las tablas por clase), leidas y declaradas;
- la comprobacion acotada de PB-T4c (CA-T4c: BOUNDED_SECTIONS de independent-review-v1.md, o --bounded-file) frente a BOUNDED_RESULT, con bounded_check;
- al emitir (sin --dry), todo de la revision X: la precondicion 6 y la comprobacion acotada se leen antes de decidir cualquier pendiente que dependa
  de ellas (la leccion de la BASE de PB-T4b).
No escribe ningun contrato en seco: lista los PENDIENTES y los AVISOS.
Uso: python make_pbt4c_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> --dry [--objects-rev <rev> [--repo <checkout principal>]
                                   [--package-file <paquete>] [--p6-file <pbt4c-precondition6.json>] [--bounded-file <independent-review-v1.md>]]
                                   [--json-out <archivo>] [--as-write]
Al custodiarlo, las rutas locales se redactan como en PB-T0 ('<worktree-de-I-64>', '<checkout-principal>')."""
import json, os, re, sys, subprocess, hashlib, unicodedata

# la leccion del cierre de PB-T3c: el generador entero es ASCII (lo leen en ascii los check_* y la redaccion); BOUNDED_RESULT tambien
_self = open(os.path.abspath(__file__), 'rb').read()
assert all(b < 0x80 for b in _self), ('el generador lleva un caracter no ASCII', [i for i, b in enumerate(_self) if b >= 0x80][:5])

# ---------------------------------------------------------------- la base: la emision de PB-T4b (F4B-E52) y su cierre (F4B-E53)
PT4B_EMIT_X = 'f5ff00eed6bf2c2b9dcd0994474ffefde88d5796'   # X de la emision de PB-T4b (la evidencia 280): AuthorityRevision de su contrato
PT4B_EMIT = '8f783df58fb0e5688d58299951fcbdcde727f33e'     # BASE de la emision de PB-T4b (la evidencia 281)
RED_T4B = 'c1e4f4585217abc8c85d90b499a07bb93c84be2c'       # el RED de PB-T4b
RR_T4B = '9b2d18937d3aa03b4cbeaf0546cd0e6b44a4f6fe'        # la reparacion del RED de PB-T4b (RR, F4B-E35)
GREEN_T4B = 'c065080d3fd1dd8bb1937b98b81e9a0c1714777b'     # el GREEN de PB-T4b (intento 1)
PT4B_CLOSE = '6adff90d856026a7a51067c1e9d7248f3d81641c'    # la custodia del cierre de PB-T4b (F4B-E53, la 282); la base de PB-T4c
# la CI del GREEN de PB-T4b en su cierre: (run, sha8 del GREEN, Core, Skipped de Core, UI, Skipped de UI); la del RED, 44893322578
CI_BASE_T4B = ('44903818806', 'c065080d', 19605, 458, 2481, 48)
# la numeracion (la sesion): parametros comprobados contra f4b-dispositions.md y la evidencia (una asignacion cada uno, para leerlos con ast)
T4B_EMIT_ENTRY = 'F4B-E52'
T4B_CLOSE_ENTRY = 'F4B-E53'
EMIT_ENTRY = 'F4B-E54'
T4B_X_EV = '280'
T4B_BASE_EV = '281'
CLOSE_EV = '282'
X_EV = '283'
BASE_EV = '284'
T4B_TASK = 'F4B-PBT4-FULL-DRAFT-PANEL'
# CA-T4c: su veredicto literal (ASCII), la linea BOUNDED de la seccion 22.10 con la cola de la recomprobacion (la 23), copiada del archivo y comparada
BOUNDED_RESULT = ('EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 3; las importantes corregidas en el paquete v5.1 (I-01, los dos archivos nuevos de Draft/ empiezan por #nullable enable y la pureza de CT-PBT4C-15 lo prohibe:'
                  ' sin la directiva y sin el ? de ProjectOf); sin decisiones nuevas del Owner; precondicion 6 sobre 6adff90d: pasos 2 a 7 cumplen, sin las CT; recomprobacion v5.1: EMITIBLE, B 0 / I 0 / M 1')
BOUNDED_SECTIONS = ('22', '23')  # CA-T4c (seccion 22, sobre la v5.0) y su recomprobacion (23, sobre la v5.1), en independent-review-v1.md (la sesion)
CAT4C_PKG = 'v5.0'  # la version del paquete sobre la que corre CA-T4c (la 22); su recomprobacion, sobre PKG_VERSION
# la version del paquete: la v5.1 (CA-T4c I-01 y M-01 a M-03 corregidas; el paso 8 de la precondicion 6); la v5.0, historia
PKG_VERSION = 'v5.1'
PKG_TARGET = 'v5.1'
CUSTODY_SHA256 = {'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md': '34ca002650780aeb1347ee2db7c4d7c285578e2e99634129ca006a6e835094c3'}  # la v5.1
# la lista de PB-T4c: las dos filas "PB-T4c" del archivo de todas las listas (D-PBT4-11; la v4.9) y las que anaden las decisiones del Owner sobre PB-T4c
# (owner-pbt4c-list-3-decision.md: CT-PBT4B-20; owner-pbt4c-list-4-decision.md: CT-PBT2B-06), en su archivo propio: LIST_NAME (la de 4) por su sha256;
# la de 3 queda como historia (LIST_HISTORY)
LISTS_ALL_SHA = 'f24c2dbc41bdd3d989796bee420b500fdf722d9f93bd408f5580c62019c8dfa5'
LISTS_ALL_ROWS = [('tests/RackCad.Tests/Workspace/WorkspacePilotEditGuardTests.cs', '155-163'), ('tests/RackCad.Tests/PushBackSharedReferenceGuardTests.cs', '306-403')]
LIST_NAME = 'pbt4c-substitution-list-4.tsv'
LIST_SHA = '9bd4c17a0eda837c92a7a0b7b3562d4ab17f1608baf3ffa8efaf560d8b1f82de'  # la lista de 4 (6.3.2 de la v5.0)
LIST_HISTORY = [('pbt4c-substitution-list-3.tsv', 'd83e0a817144fa99f95ed4bfd151b822a4fc1035c10f736f77e8f8e6350186c1')]
LIST_ADDED_EXPECTED = [('tests/RackCad.Tests/WorkspaceFullCopyGuardTests.cs', 'CT_PBT4B_20'), (None, 'CT_PBT2B_06')]  # (archivo, prefijo del metodo)
LIST_N = 4
OWNER_C = ['owner-pbt4c-list-3-decision.md', 'owner-pbt4c-list-4-decision.md']  # en coordinator/, autoridades de PB-T4c
SESSION_DEC_C = 'session-pbt4c-design-decisions.md'                          # D-S7 a D-S10, en F4B-DRAFTS/pbt4/
# las piezas por sha256 que trae la v5.1 (6.3.2: el esqueleto y el GREEN como texto literal, aplicados por su guion, la forma "generador con registro";
# los Skip, los equivalentes, su comprobacion y la lista siguen siendo los de la v5.0). El ensayo y el censo del paso 8 no van al contrato (6.3.2)
PIECE_NAMES = ['pbt4c-skeleton-v51.txt', 'pbt4c-green-v51.txt', 'green_pbt4c_v51.py', 'pbt4c_skips_v50.py', 'make_equivalents_pbt4c_v50.py',
               'apply_red_pbt4c_v51.py', 'apply_green_pbt4c_v51.py', 'check_skips_pbt4c_v50.py', 'pbt4c_ct_table_v51.py', 'make_list_pbt4c_4.py']  # 6.3.2
N_REQ_ROWS = 85  # 76 heredados (el n.o 43 sale) y 9 nuevos
# lo que fija la version del paquete: las CT, las filas de RequiredTests, D_C y los totales (5.3)
N_CT = 15  # CT-PBT4C-01..15
N_CT_CORE, N_CT_UI = 11, 4          # las CT de Core y de UI (5.3 de la v4.9: C + 11 + ...; UI + 4)
RED_FAIL_CORE, RED_FAIL_UI = 9, 4   # los Failed del RED (5.3)
D_C = 0    # las que se pierden (5.3; se confirma en la precondicion 6)
# los totales previstos con la lista de 4 (medidos por el autor; los da la v5.0): (total, Passed, Failed, Skipped) de Core y de UI
CI_T4C_EXPECTED = {'GREEN': (19619, 19158, 0, 461, 2486, 2437, 0, 49), 'RED': (19619, 19149, 9, 461, 2486, 2432, 5, 49)}  # el RED de UI con el equivalente de CT-PBT2B-06
# la precondicion 6 de PB-T4c (hs65/pbt4ccc/compile/): su resumen para el generador; su esquema lo fija la v5.0, y la v5.1 le anade cuatro claves con el
# mismo Schema (6.3.2): las lee y las declara. Las demas que no lee, las de la v5.0; una clave que el paquete no declara es PENDIENTE (la sesion decide)
P6_REL_NAME = 'pbt4c-precondition6.json'
P6_SCHEMA = 'rackcad-pbt4c-precondition6/v1'
P6_UNREAD_V50 = ('Binaries', 'Builds', 'CheckSkips', 'DriverPieces', 'FilterOverlap', 'GreenFiles', 'History', 'InheritedCoreFiltersGreen', 'IsDirtyCost',
                 'Package', 'Probes', 'Skeleton', 'SkipFiles', 'SuiteDurationSeconds')
P6_V51_KEYS = ('Rehearsal', 'Control', 'Census', 'ClassResults')
# decision de la sesion: Checks (los resultados de check-core y check-ui de los pasos 5 y 6), que el paquete no nombra, se declara como campo que no lee,
# como Cost en PB-T4b; el contrato no cambia
P6_UNREAD_V51 = ('Checks',)
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
NC_TASK = 'PBT4'  # el --task de nc1 y nc2 para las subtareas de PB-T4 (el TaskId empieza por F4B-PBT4-)
MAIN_CHECKOUT = '<checkout-principal>'  # Redaccion para el repo publico: la ruta local del checkout principal
WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree

ARGS = sys.argv[1:]
VALUED = ('--objects-rev', '--repo', '--package-file', '--json-out', '--bounded-file', '--p6-file')


def opt(name):
    if name not in ARGS:
        return None
    i = ARGS.index(name)
    assert i + 1 < len(ARGS) and not ARGS[i + 1].startswith('--'), ('falta el valor de', name)
    return ARGS[i + 1]


POS = [a for i, a in enumerate(ARGS) if not a.startswith('--') and not (i > 0 and ARGS[i - 1] in VALUED)]
assert len(POS) == 3, 'uso: make_pbt4c_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> --dry [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
DRY = '--dry' in ARGS
# sin --dry, el generador emite (en el worktree, sobre el commit X): lo que en seco es un AVISO o un PENDIENTE aborta (como --as-write)
AS_WRITE = '--as-write' in ARGS or not DRY
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % 'F4B-PBT4-EDIT-ALL-WINDOW'
OBJ_REV = opt('--objects-rev')
PKG_FILE = opt('--package-file')
assert PKG_FILE is None or OBJ_REV is not None, '--package-file solo junto con --objects-rev'
PKG_FILE = os.path.abspath(PKG_FILE) if PKG_FILE else None
assert PKG_FILE is None or os.path.isfile(PKG_FILE), ('no existe el paquete', PKG_FILE)
P6_FILE = opt('--p6-file')  # la precondicion 6 de la sesion, en seco; al emitir, la de la revision
P6_FILE = os.path.abspath(P6_FILE) if P6_FILE else None
assert P6_FILE is None or (os.path.isfile(P6_FILE) and DRY), ('--p6-file solo en seco y con el archivo', P6_FILE)
BOUNDED_FILE = opt('--bounded-file')  # independent-review-v1.md con las secciones de CA-T4c, en seco; al emitir, el de la revision
assert BOUNDED_FILE is None or (OBJ_REV is not None and DRY), '--bounded-file solo junto con --objects-rev y --dry'
BOUNDED_FILE = os.path.abspath(BOUNDED_FILE) if BOUNDED_FILE else None
assert BOUNDED_FILE is None or os.path.isfile(BOUNDED_FILE), ('no existe el informe de la comprobacion acotada', BOUNDED_FILE)
JSON_OUT = opt('--json-out')
JSON_OUT = os.path.abspath(JSON_OUT) if JSON_OUT else None
assert re.fullmatch(r'[0-9]{4}-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9]Z', ISSUED) and ISSUED[:10] < '2026-12-29', ISSUED
assert re.fullmatch(r'[0-9a-f]{40}', AR) and re.fullmatch(r'[0-9a-f]{40}', MAIN), (AR, MAIN)
assert BOUNDED_RESULT is None or all(ord(ch) < 0x80 for ch in BOUNDED_RESULT), 'BOUNDED_RESULT en ASCII'

if OBJ_REV is None:
    os.chdir(WT)
    REPO, REV = None, 'HEAD'
else:
    REPO, REV = opt('--repo') or MAIN_CHECKOUT, OBJ_REV

TASK = 'F4B-PBT4-EDIT-ALL-WINDOW'
assert len(TASK) <= 26 and TASK.startswith('F4B-' + NC_TASK + '-'), len(TASK)  # R-T1-07 (MAX_PATH, una cota superior); la ligadura de --task PBT4
PILOT = 'docs/automation/evidence/I-64-pilot'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
D4 = PILOT + '/F4B-DRAFTS/pbt4/'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
EVIDENCE = 'docs/automation/evidence/I-64-evidence.md'
PBT4BC = PILOT + '/' + T4B_TASK + '/coordinator/gate-contract.json'
T4B_TCP = PILOT + '/' + T4B_TASK + '/task-close-package.md'
PKG = 'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md'
LISTS_ALL = D4 + 'pbt4-substitution-lists.tsv'
P6_REL = D4 + P6_REL_NAME
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
WARN = []
PENDING = []  # lo que espera a la v5.0, a la precondicion 6 o a CA-T4c: el seco lo lista; al emitir (AS_WRITE) aborta
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
    """Lo que solo existira con la emision de PB-T4c (su commit X, su comprobacion acotada): AVISO en seco, error con --as-write."""
    if cond:
        return True
    assert not AS_WRITE, msg
    WARN.append(msg)
    return False


def pending(msg):
    """Lo que espera a la v5.0, a la precondicion 6 o a CA-T4c: se lista en seco (PENDIENTE); con --as-write o al emitir aborta. Un pendiente que
    depende de la precondicion 6 o de CA-T4c se decide despues de leerlas de la revision, nunca antes (la leccion de la BASE de PB-T4b)."""
    assert not AS_WRITE, 'pendiente: ' + msg
    PENDING.append(msg)


REV_SHA = git('rev-parse', REV).stdout.decode('ascii').strip()
assert re.fullmatch(r'[0-9a-f]{40}', REV_SHA), ('revision desconocida', REV)

# ---------------------------------------------------------------- el anclaje: la emision de PB-T4b (F4B-E52) y su cierre (F4B-E53)
for _full in (PT4B_EMIT_X, PT4B_EMIT, RED_T4B, RR_T4B, GREEN_T4B, PT4B_CLOSE):
    assert git('rev-parse', _full[:8]).stdout.decode('ascii').strip() == _full, ('SHA', _full[:8])
    assert git('merge-base', '--is-ancestor', _full, REV).returncode == 0, ('la revision no desciende de PB-T4b', _full[:8])
assert git('rev-parse', PT4B_EMIT + '^').stdout.decode('ascii').strip() == PT4B_EMIT_X, 'BASE de PB-T4b no es hijo directo de su X'
assert git('rev-parse', RR_T4B + '^').stdout.decode('ascii').strip() == RED_T4B, 'la reparacion del RED de PB-T4b no es hija directa de su RED'
assert git('diff', '--quiet', GREEN_T4B, PT4B_CLOSE, '--', 'src', 'tests').returncode == 0, 'el cierre de PB-T4b no tiene el src y los tests de su GREEN'
assert git('diff', '--quiet', GREEN_T4B, REV, '--', 'src', 'tests').returncode == 0, 'src o tests cambiaron desde el GREEN de PB-T4b'
_disp = show(DISP).replace('\r\n', '\n')
_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
assert _ids == list(range(1, len(_ids) + 1)) and _ids[-1] >= 53, ('f4b-dispositions.md', _ids[-3:])
_n = lambda e: int(e.split('E')[-1])
assert _n(T4B_CLOSE_ENTRY) == _n(T4B_EMIT_ENTRY) + 1 and _n(EMIT_ENTRY) == _n(T4B_CLOSE_ENTRY) + 1, ('la numeracion: E53 sigue a E52 y E54 a E53',
                                                                                                    T4B_CLOSE_ENTRY, EMIT_ENTRY)
assert _ids[-1] in (_n(T4B_CLOSE_ENTRY), _n(EMIT_ENTRY)), ('la ultima entrada de f4b-dispositions.md', _ids[-1])
assert re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (T4B_EMIT_ENTRY, T4B_TASK), _disp), 'F4B-E52, la emision de PB-T4b'
_dc = [p for p in re.split(r'(?m)^## ', _disp) if p.startswith(T4B_CLOSE_ENTRY + '. ')]
assert len(_dc) == 1 and re.match(r'%s\. Cierre de %s\b' % (T4B_CLOSE_ENTRY, T4B_TASK), _dc[0]), 'F4B-E53, el cierre de PB-T4b'
_dc = _dc[0]
E54_OK = bool(re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (EMIT_ENTRY, TASK), _disp))
_evs = re.findall(r'(?m)^## (\d+)\. ', show(EVIDENCE))
assert int(T4B_BASE_EV) == int(T4B_X_EV) + 1 and int(CLOSE_EV) == int(T4B_BASE_EV) + 1 and int(X_EV) == int(CLOSE_EV) + 1 and int(BASE_EV) == int(X_EV) + 1, \
    ('la numeracion de la evidencia', CLOSE_EV, X_EV, BASE_EV)
assert _evs[-1] in (CLOSE_EV, X_EV), ('la ultima seccion de la evidencia', _evs[-3:])
assert T4B_X_EV in _evs and T4B_BASE_EV in _evs and CLOSE_EV in _evs, 'las secciones 280, 281 y 282 de PB-T4b'
# la CI del GREEN de PB-T4b, en la entrada de su cierre (la linea base de PB-T4c, la C de 5.3)
_run, _sha8, _core, _cskip, _ui, _uskip = CI_BASE_T4B
_sp = lambda v: format(v, ',').replace(',', ' ')
assert GREEN_T4B.startswith(_sha8) and ('GREEN `%s`' % _run) in _dc and 'CLOSED' in _dc, ('la entrada de cierre de PB-T4b', CI_BASE_T4B)
assert ('Core %s = %s + 0 + %d' % (_sp(_core), _sp(_core - _cskip), _cskip)) in _dc and ('UI %s = %s + 0 + %d' % (_sp(_ui), _sp(_ui - _uskip), _uskip)) in _dc, \
    ('los recuentos de CI_BASE_T4B no son los de la entrada de cierre de PB-T4b', CI_BASE_T4B)
assert 'CLOSED' in show(T4B_TCP), 'el task-close-package de PB-T4b no dice CLOSED'
soft(E54_OK, 'emision: %s (la emision de %s) aun no esta en f4b-dispositions.md' % (EMIT_ENTRY, TASK))
if soft(git('cat-file', '-e', AR + '^{commit}').returncode == 0, 'emision: la AuthorityRevision %s no esta en el almacen de objetos (seco)' % AR[:8]):
    assert git('merge-base', '--is-ancestor', PT4B_CLOSE, AR).returncode == 0, 'la AuthorityRevision no contiene el cierre de PB-T4b'
for _p in (PKG, LISTS_ALL, PBT4BC):
    assert in_head(_p), ('custodiado antes de PB-T4c y no esta en la revision', _p)

# ---------------------------------------------------------------- el contrato de PB-T4b de su emision: de el salen los textos comunes, C11 y los filtros
pbt4b = json.loads(show(PBT4BC))
assert pbt4b['TaskId'] == T4B_TASK and pbt4b['AuthorityRevision'] == PT4B_EMIT_X, ('el contrato de PB-T4b', pbt4b['AuthorityRevision'][:8])
T4B_STOP = {s['Id']: s['Text'] for s in pbt4b['StopConditions']}
STD_STOPS = [s for s in pbt4b['StopConditions'] if not s['Id'].startswith('PBT4B-')]
assert len(STD_STOPS) == 17, ('las paradas estandar del contrato de PB-T4b', len(STD_STOPS))
EVID0 = pbt4b['ExpectedEvidence'][0]


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
# la primera ExpectedEvidence: la del contrato de PB-T4b (la de PB-T4a y PB-T3c), byte a byte, en la lista cerrada de literales del v2
assert EVID0 in NC2_EVIDENCES, ('la primera evidencia del contrato de PB-T4b no esta en F4B_EXACT_EVIDENCES del v2', EVID0[:80])
# C11 de PB-T4c: PBT4B-C11 del contrato de PB-T4b con PB-T4b -> PB-T4c (la tarea de nc sigue siendo PBT4). El texto final lo fija el paquete
C11_SUBS = [('PB-T4b', 'PB-T4c')]
C11 = T4B_STOP['PBT4B-C11']
for _old, _new in C11_SUBS:
    assert _old in C11, ('PBT4C-C11: PB-T4b no esta en PBT4B-C11', _old)
    C11 = C11.replace(_old, _new)
for _need in ('PBT4-NC2-EXACT-WHITELIST-F4B-01', 'with --task PBT4 in the F4B-E21 mode', 'SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B',
              'with --task PBT4, recorded as nc1 = SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE', NC2_V2 + ', SHA-256 ' + NC2_V2_SHA,
              NC1_SUBST + ', SHA-256 ' + NC1_SUBST_SHA):
    assert C11.count(_need) == 1, ('PBT4C-C11', _need)
assert C11.endswith('no one-off acceptance extends to PB-T4c') and not re.search(r'PBT3C|PB-T3c|PB-T4a|PBT4A|PB-T4b|PBT4B', C11), 'PBT4C-C11: restos de PB-T4b'


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

# ---------------------------------------------------------------- el criterio de count_tests_git.py (y blobs, para leer muchos archivos)
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


# ---- tests_of (inicio)
def tests_of(text):
    """Las pruebas de un archivo con el criterio de count_tests_git.py: nombre, linea del atributo y de la declaracion, Fact o Theory, Skip, cuantos
    resultados da (Theory: sus InlineData; MemberData o ClassData: negativo) y su clase con su espacio de nombres."""
    out, ns, cur, pend, inline = [], '', None, None, 0
    for i, line in enumerate(text.split('\n'), 1):
        m = _NS.match(line)
        if m:
            ns = m.group(1).rstrip(';')
        m = _CLASS.match(line)
        if m:
            cur = m.group(1)
        s = line.strip()
        if s.startswith('[Fact') or s.startswith('[Theory'):
            pend, inline = ('fact' if s.startswith('[Fact') else 'theory', 'Skip' in s, i), 0
        elif s.startswith('[InlineData') and pend and pend[0] == 'theory':
            inline += 1
        elif (s.startswith('[MemberData') or s.startswith('[ClassData')) and pend and pend[0] == 'theory':
            inline = -10 ** 6
        elif re.match(r'(public|internal|private)\s+.*\(', s) and pend:
            out.append({'method': re.search(r'(\w+)\s*\(', s).group(1), 'attr': pend[2], 'line': i, 'kind': pend[0], 'skip': pend[1],
                        'n': 1 if pend[0] == 'fact' else inline, 'cls': (ns + '.' + cur) if cur else ns})
            pend = None
    return out
# ---- tests_of (fin)


# ---------------------------------------------------------------- la lista de PB-T4c: las filas "PB-T4c" del archivo de todas las listas (D-PBT4-11)
_la = show_bytes(LISTS_ALL)
assert hashlib.sha256(_la).hexdigest() == LISTS_ALL_SHA, ('pbt4-substitution-lists.tsv no es el de la emision de PB-T4a (sha256)', LISTS_ALL)
ROWS_ALL = [l.split('\t') for l in _la.decode('utf-8').replace('\r\n', '\n').split('\n')[1:] if l.startswith('PB-T4c\t')]
assert [(r[1], r[3]) for r in ROWS_ALL] == LISTS_ALL_ROWS, ('las filas de PB-T4c del archivo de todas las listas (PEG:155-163 y CT-PBT1B-15)',
                                                           [(r[1], r[3]) for r in ROWS_ALL])


def list_bytes(name):
    """La lista de la revision, o junto a --package-file en seco (custodia X de PB-T4c al emitir)."""
    if in_head(D4 + name):
        return show_bytes(D4 + name), D4 + name + ' en ' + REV_SHA[:8]
    _loc = os.path.join(os.path.dirname(PKG_FILE), name) if PKG_FILE else None
    if _loc and os.path.isfile(_loc):
        soft(False, 'emision: la lista de PB-T4c %s aun no esta en la revision (la custodia X de PB-T4c)' % (D4 + name))
        return open(_loc, 'rb').read(), name + ' junto a --package-file'
    return None, None


# la lista propia de PB-T4c (la de 4) y, mientras no exista, la de 3 (la historia); sin ninguna, las dos filas del archivo de todas las listas
LIST_SRC, _lb = None, None
for _nm, _sha in [(LIST_NAME, LIST_SHA)] + LIST_HISTORY:
    _lb, _src = list_bytes(_nm)
    if _lb is not None:
        LIST_SRC = (_nm, _src)
        if _sha:
            assert hashlib.sha256(_lb).hexdigest() == _sha, ('la lista de PB-T4c no es la que fija la version del paquete (sha256)', _nm)
        break
if LIST_SRC is None:
    ROWS = ROWS_ALL
    pending('la lista de PB-T4c (%s, %d filas): no esta ni en la revision ni junto a --package-file; el seco usa las dos filas del archivo de todas las listas'
            % (LIST_NAME, LIST_N))
else:
    ROWS = [l.split('\t') for l in _lb.decode('utf-8').replace('\r\n', '\n').split('\n')[1:] if l.strip()]
    if LIST_SRC[0] != LIST_NAME:
        pending('la lista de 4 (%s, con CT-PBT2B-06 de owner-pbt4c-list-4-decision.md) aun no existe: el seco usa %s, la historia' % (LIST_NAME, LIST_SRC[0]))
    elif not LIST_SHA:
        pending('el sha256 de la lista de 4 (LIST_SHA): lo fija la %s' % PKG_TARGET)
# la lista conserva las dos del archivo de todas las listas, en su orden, y anade exactamente las de las decisiones del Owner
assert [(r[1], r[2], r[3]) for r in ROWS[:2]] == [(r[1], r[2], r[3]) for r in ROWS_ALL], 'la lista de PB-T4c no empieza por las dos de la v4.9'
for _r, (_f, _pfx) in zip(ROWS[2:], LIST_ADDED_EXPECTED):
    assert (_f is None or _r[1] == _f) and _r[2].startswith(_pfx), ('una fila anadida no es la de la decision del Owner', _r[1], _r[2][:40], _f, _pfx)
assert len(ROWS) <= 2 + len(LIST_ADDED_EXPECTED), ('la lista de PB-T4c tiene filas de mas', len(ROWS))
LIST_TOTAL = len(ROWS)
K_CORE = sum(1 for r in ROWS if r[1].startswith(TR))
K_UI = sum(1 for r in ROWS if r[1].startswith(UT))
assert K_CORE + K_UI == LIST_TOTAL
LISTED = {}
for _lista, _path, _method, _lines, _cause in ROWS:
    _t = [x for x in tests_of(show(_path)) if x['method'] == _method]
    assert len(_t) == 1, ('una prueba de la lista de PB-T4c no esta una sola vez en su archivo', _path, _method)
    _a, _b = (int(x) for x in _lines.split('-'))
    assert _t[0]['kind'] == 'fact' and not _t[0]['skip'] and _a <= _t[0]['line'] <= _b, ('una prueba de la lista: [Fact] sin Skip y en su rango', _path, _method,
                                                                                          _t[0]['line'], _lines)
    LISTED[(_path, _method)] = _t[0]
LIST_FILES = sorted({p for p, _ in LISTED})


# ---------------------------------------------------------------- los filtros heredados: los del contrato de PB-T4b, con lo que dejan los Skip de la lista de
# PB-T4c (6.3: "y el resto"), con el criterio de count_tests_git.py sobre los objetos de la revision
COUNTS = {}
for _proj in ('UI', 'Core'):
    _nsp, _totals, _texts = count_static(_proj)
    for r in pbt4b['RequiredTests']:
        if (r['Project'] == UIP) == (_proj == 'UI'):
            _f = r['Filter'].split('~', 1)[1]
            _sel = {k: v for k, v in _totals.items() if k.startswith(_f)}
            COUNTS[r['Filter']] = (len(_sel), sum(v[0] for v in _sel.values()), sum(v[1] for v in _sel.values()), sum(v[2] for v in _sel.values()))
_LST = [(UIP if p.startswith(UT) else CP, t['cls'] + '.' + t['method'], max(t['n'], 0)) for (p, _m), t in sorted(LISTED.items())]
REQ_INH, REQ_REPORT, REQ_ZERO = [], [], []
for _ri, r in enumerate(pbt4b['RequiredTests'], 1):
    _f = r['Filter'].split('~', 1)[1]
    classes, run, skip, member = COUNTS[r['Filter']]
    if classes == 0:
        # como en PB-T4b: count_static atribuye las pruebas de NumericFieldTests a su clase anidada BorderBrushSource, que va antes de ellas; se cuenta
        # por el archivo que declara la clase
        _cn = _f.rsplit('.', 1)[1]
        _fs = [p for p in tracked(UT if r['Project'] == UIP else TR) if p.endswith('.cs') and re.search(r'\bclass\s+%s\b' % _cn, show(p))]
        assert len(_fs) == 1, ('un filtro heredado de PB-T4b no selecciona ninguna clase en la revision', r['Filter'], _fs)
        _ts = tests_of(show(_fs[0]))
        classes, run, skip = 1, sum(t['n'] for t in _ts if not t['skip'] and t['n'] > 0), sum(1 for t in _ts if t['skip'])
        member = sum(1 for t in _ts if t['n'] < 0)
    sk = sum(n for proj, fqn, n in _LST if proj == r['Project'] and fqn.startswith(_f))  # FullyQualifiedName~ (como en PB-T4b): el prefijo, sin el punto
    new_min = r['MinSelected'] - sk
    if new_min <= 0:
        REQ_ZERO.append(r['Filter'])
    REQ_INH.append((r['Project'], r['Filter'], new_min, r['ExpectRed']))
    if sk:
        REQ_REPORT.append('n.o %2d %-4s %-62s MinSelected %4d -> %-4d (Skip de la lista %d) medido: clases=%d ejecutables=%d skip=%d memberdata=%d'
                          % (_ri, 'UI' if r['Project'] == UIP else 'Core', _f.split('.', 2)[-1], r['MinSelected'], new_min, sk, classes, run, skip, member))
# cada prueba de la lista cae en algun filtro heredado (si no, el paquete dice como se cuenta)
LIST_UNFILTERED = [fqn for proj, fqn, n in _LST if not any(proj == r['Project'] and fqn.startswith(r['Filter'].split('~', 1)[1])
                                                          for r in pbt4b['RequiredTests'])]
# los filtros heredados que se quedan sin pruebas ejecutables (REQ_ZERO) los decide el paquete en 6.3.1 (el n.o 43 sale, V50-16), mas abajo; aqui no se
# abre ningun pendiente: al emitir (AS_WRITE) abortaria antes de leer esa seccion (la leccion de la BASE de PB-T4b)


# ---------------------------------------------------------------- el paquete (PKG_VERSION, por su sha256): la seccion 6.3 de PB-T4c y lo que le falta
if PKG_FILE:
    PKG_RAW, PKG_SRC = open(PKG_FILE, 'rb').read(), '--package-file ' + os.path.basename(PKG_FILE)
else:
    PKG_RAW, PKG_SRC = show_bytes(PKG), PKG + ' en ' + REV_SHA[:8]
assert hashlib.sha256(PKG_RAW).hexdigest() == CUSTODY_SHA256[PKG], ('el paquete leido no es la %s revisada (sha256)' % PKG_VERSION, PKG_SRC)
PKG_LINES = PKG_RAW.decode('utf-8').replace('\r\n', '\n').split('\n')
_title = [l for l in PKG_LINES[:8] if l.startswith('# I-64 F4B')]
assert len(_title) == 1 and re.search(r', (v\d+(?:\.\d+)?)\s*$', _title[0]).group(1) == PKG_VERSION, ('la version del paquete', _title)
assert PKG_VERSION == PKG_TARGET, ('el generador de PB-T4c es el de la %s (6.3.1 y 6.3.2 literales)' % PKG_TARGET, PKG_VERSION)


def section(head):
    i = [k for k, l in enumerate(PKG_LINES) if l.startswith(head)]
    assert len(i) == 1, ('la seccion del paquete', head, len(i))
    lvl = len(head) - len(head.lstrip('#'))
    out = []
    for l in PKG_LINES[i[0] + 1:]:
        if re.match(r'#{1,%d} ' % lvl, l):
            break
        out.append(l)
    return out


SEC63 = section('### 6.3 PB-T4c')
SEC60 = section('### 6.0 ')
SEC53 = section('### 5.3 PB-T4c')
SEC43 = section('### 4.3 PB-T4c')
# las abreviaturas del paquete (WHC, WEI, FVM, PV, W, WCo, PEG...): su ruta; '.../' es tests/RackCad.Tests/Workspace/ y 'X.xaml(.cs)', los dos archivos
_i = PKG_LINES.index('**Abreviaturas:**')
ABBR = {}
for _l in PKG_LINES[_i + 1:]:
    if not _l.startswith('- '):
        break
    _m = re.match(r'- ([A-Za-z]+): `([^`]+)`;?$', _l)
    if _m:
        _p = _m.group(2).replace(chr(0x2026) + '/', TR + 'Workspace/')
        ABBR[_m.group(1)] = [_p[:-len('(.cs)')], _p[:-len('(.cs)')] + '.cs'] if _p.endswith('(.cs)') else [_p]
for _k in ('WHC', 'WEI', 'FVM', 'PV', 'W', 'WCo', 'PEG'):
    assert _k in ABBR and all(in_head(p) for p in ABBR[_k]), ('una abreviatura del paquete sin su archivo en la revision', _k, ABBR.get(_k))


def field(sec, name):
    i = [k for k, l in enumerate(sec) if l.startswith('- **%s**' % name) or l.startswith('- **`%s`' % name)]
    assert len(i) == 1, ('el campo de 6.3', name, len(i))
    out = []
    for l in sec[i[0] + 1:]:
        if l.startswith('- ') or not l.strip():
            break
        out.append(l)
    return out


# 4.3: las dos guardas de la lista, con su causa
_s43 = '\n'.join(SEC43)
assert 'PEG:155-163' in _s43 and 'CT-PBT1B-15' in _s43, '4.3 sin las dos guardas de la lista'


# ================================================================ FASE 2 (la v5.0): el contrato de PB-T4c desde 6.3, 6.3.1 y 6.3.2
_b = [k for k, l in enumerate(PKG_LINES) if l.startswith('#### 6.3.1 ')]
assert len(_b) == 1, 'section 6.3.1'
_a = next(k for k in range(_b[0], len(PKG_LINES)) if PKG_LINES[k].strip() == '```text')
_z = next(k for k in range(_a + 1, len(PKG_LINES)) if PKG_LINES[k].strip() == '```')
BLOCK = PKG_LINES[_a + 1:_z]
assert all(ord(ch) < 0x80 for l in BLOCK for ch in l), 'el bloque de 6.3.1 no es ASCII'
_s632 = '\n'.join(section('#### 6.3.2 '))


def field(name):
    i = BLOCK.index(name + ':')
    out = []
    for l in BLOCK[i + 1:]:
        if not l.strip() or re.match(r'^[A-Za-z]+(?: \(propias\))?:$', l):
            break
        out.append(l)
    return out


assert BLOCK[0] == 'TaskId: ' + TASK, ('el TaskId de 6.3.1', BLOCK[0])
_ob = field('Objective')
assert len(_ob) == 1, 'el Objective de 6.3.1 en una linea'
OBJECTIVE = _ob[0]
for _need in ('paquete del contrato de PB-T4 ' + PKG_VERSION, 'CT-PBT4C-01..%02d' % N_CT, '%d sustituciones' % LIST_TOTAL):
    assert _need in OBJECTIVE, ('el Objective de PB-T4c sin', _need)
INV = field('Invariants')
assert [i.split('. ')[0] for i in INV] == ['INV-PBT4C-%02d' % n for n in range(1, 15)], [i[:12] for i in INV]
assert LIST_SHA in INV[6] and LIST_NAME in INV[6], 'INV-PBT4C-07 sin la lista de PB-T4c y su sha256'
STOP_OWN_TEXT = field('StopConditions (propias)')
_sid = [s.split(': ', 1)[0] for s in STOP_OWN_TEXT]
assert _sid == ['PBT4C-C%02d' % n for n in list(range(1, 15)) + [16, 17, 18]], _sid
STOP_OWN = [{'Id': s.split(': ', 1)[0], 'Text': s.split(': ', 1)[1]} for s in STOP_OWN_TEXT]
_so = dict((s['Id'], s['Text']) for s in STOP_OWN)
assert _so['PBT4C-C11'] == C11, 'PBT4C-C11 del paquete no es PBT4B-C11 con PB-T4b -> PB-T4c'
assert 'IMPORTANT LIMIT' in _so['PBT4C-C17'], 'C17'
assert ('the %d tests of %s' % (LIST_TOTAL, LIST_NAME)) in _so['PBT4C-C02'] and 'No exception outside the closed list of PB-T4c' in _so['PBT4C-C02'], \
    'C02 sin excepcion fuera de la lista'
STOPS = STD_STOPS + STOP_OWN
assert len(STOPS) == 34, len(STOPS)
EVID = field('ExpectedEvidence')
assert [e.split('] ')[0] + ']' for e in EVID] == ['[%d]' % n for n in range(16)], [e[:4] for e in EVID]
EVID = [e.split('] ', 1)[1] for e in EVID]
assert EVID[0] == EVID0, 'la primera evidencia no es la del contrato de PB-T4b (la de PB-T3c)'
# [14]: la base de PB-T4b y los totales de cada rama (los medidos por el autor; la precondicion 6 los da)
_e14 = EVID[14]
_sp = lambda v: format(v, ',').replace(',', ' ')
for _need in ('run %s sobre su GREEN %s' % (CI_BASE_T4B[0], CI_BASE_T4B[1]), 'Core %s resultados' % _sp(CI_BASE_T4B[2]), 'UI %s' % _sp(CI_BASE_T4B[4])):
    assert _need in _e14, ('[14] sin', _need)
# el alcance de 6.3 (26), ruta a ruta y numerado
_al = [k for k, l in enumerate(SEC63) if l.startswith('- **`AllowedWriteScope`** (')]
assert len(_al) == 1
N_ALLOWED = int(re.search(r'\((\d+)\)', SEC63[_al[0]]).group(1))
ALLOWED, _nums = [], []
for l in SEC63[_al[0] + 1:]:
    if not l.startswith('  - '):
        break
    for _n, _p in re.findall(r'(\d+)\. `((?:src|tests)/[^`]+)`', l):
        _nums.append(int(_n))
        ALLOWED.append(_p)
assert _nums == list(range(1, N_ALLOWED + 1)) and len(ALLOWED) == len(set(ALLOWED)) == N_ALLOWED == 26, ('el AllowedWriteScope de 6.3', _nums)
assert not any('/Pilot/' in a for a in ALLOWED), 'ninguna ruta de Pilot/'
assert [a for a in ALLOWED if a.startswith('src/RackCad.Plugin/')] == [ABBR['WCo'][0], 'src/RackCad.Plugin/Views/RackWorkspaceEditorWindow.cs'], 'del Plugin, solo WCo y la ventana'
UNIT_CHANGE_P = ALLOWED[:9]
assert all(in_head(a) and a.startswith('src/') for a in UNIT_CHANGE_P), 'UNIT_CHANGE: las 9 existentes de src/'
SKIP_FILES = ALLOWED[-4:]
assert sorted(SKIP_FILES) == sorted(LIST_FILES), ('los cuatro archivos de los Skip', SKIP_FILES, LIST_FILES)
for a in ALLOWED:
    assert in_head(a) == (a in UNIT_CHANGE_P or a in SKIP_FILES), ('una ruta del alcance existe o falta en la revision', a)
# el ForbiddenWriteScope: [PKC-C] en src/ y el precedente de tests/ (la lista del contrato de PB-T4b, con sus rutas de tests/), menos este alcance
SRCF = cover('src/')
TESTF, SPLIT2 = [], []
for f in [f for f in pbt4b['ForbiddenWriteScope'] if f.startswith('tests/')] + [a for a in pbt4b['AllowedWriteScope'] if a.startswith('tests/')]:
    if f in ALLOWED or f in TESTF:
        continue
    if f.endswith('/') and any(a.startswith(f) for a in ALLOWED):
        SPLIT2.append(f)
        TESTF += [x for x in cover(f) if x not in ALLOWED and x not in TESTF]
    else:
        TESTF.append(f)
ROOT = [f for f in pbt4b['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == len(set(FORBIDDEN)) and not set(FORBIDDEN) & set(ALLOWED), 'el ForbiddenWriteScope: sin repetidos ni rutas del alcance'
# los RequiredTests de 6.3.1 (85): los 76 heredados con su numero (el n.o 43 sale) y sus MinSelected medidos aqui, y los 9 nuevos (78 a 86)
_t = [k for k, l in enumerate(PKG_LINES) if l.startswith('| # | Project | Filter | MinSelected | ExpectRed |') and k > _z]
assert len(_t) == 1
REQ_PKG = []
for l in PKG_LINES[_t[0] + 2:]:
    m = re.match(r'^\| (\d+) \| `([^`]+)` \| `([^`]+)` \| (\*\*\d+\*\*|\d+) \| (true|false) \|$', l)
    if not m:
        break
    REQ_PKG.append((int(m.group(1)), m.group(2), m.group(3), int(m.group(4).strip('*')), m.group(5) == 'true'))
assert len(REQ_PKG) == N_REQ_ROWS and [r[0] for r in REQ_PKG] == [n for n in range(1, 87) if n != 43], ('la tabla de RequiredTests de 6.3.1', len(REQ_PKG))
_inh = {i: x for i, x in enumerate(REQ_INH, 1)}
for k, proj, filt, m, er in REQ_PKG:
    if k <= 77:
        assert (proj, filt, m, er) == (_inh[k][0], _inh[k][1], _inh[k][2], False), ('la fila %d de RequiredTests frente a lo medido' % k, filt, m, _inh[k][2])
    else:
        assert sum(1 for r in REQ_PKG if r[2] == filt) == 1 and m > 0, ('una fila nueva: un filtro unico y un MinSelected', k, filt)
assert REQ_ZERO == [_inh[43][1]], ('el n.o 43 es el unico que queda en 0: su fila sale de la tabla (V50-16)', REQ_ZERO)
REQ = [(p, f, m, e) for _, p, f, m, e in REQ_PKG]
_er = [r for r in REQ if r[3]]
assert len(_er) == 4 and sum(1 for r in _er if r[0] == CP) == 2 and sum(r[2] for r in _er) == 14, ('los filtros con ExpectRed: 4, 2 de Core, 14 resultados', _er)
# los totales de cada rama, desde la lista y la tabla: los Skip de cada proyecto, cada uno con su equivalente, las CT nuevas, y en el RED los Failed de las CT y
# los de los equivalentes con ExpectRed (el de CT-PBT2B-06 cae en el RED: el esqueleto no cambia la ventana). Lo calculado tiene que ser lo del paquete
_eqc = {os.path.basename(a)[:-3] for a in ALLOWED[18:22]}
_eqfail = {CP: 0, UIP: 0}
for _p, _f, _m, _e in REQ:
    if _e and _f.rsplit('.', 1)[1] in _eqc:
        _eqfail[_p] += _m
_c, _cs, _u, _us = CI_BASE_T4B[2], CI_BASE_T4B[3], CI_BASE_T4B[4], CI_BASE_T4B[5]
CI_T4C = {'RED': (_c + N_CT_CORE + K_CORE - D_C, (_c - _cs) + N_CT_CORE - RED_FAIL_CORE - _eqfail[CP], RED_FAIL_CORE + _eqfail[CP], _cs + K_CORE,
                  _u + N_CT_UI + K_UI, (_u - _us) + N_CT_UI - RED_FAIL_UI - _eqfail[UIP], RED_FAIL_UI + _eqfail[UIP], _us + K_UI),
          'GREEN': (_c + N_CT_CORE + K_CORE - D_C, (_c - _cs) + N_CT_CORE, 0, _cs + K_CORE, _u + N_CT_UI + K_UI, (_u - _us) + N_CT_UI, 0, _us + K_UI)}
for _k, (_t, _p, _f, _sk, _ut, _up, _uf, _usk) in CI_T4C.items():
    assert _t == _p + _f + _sk and _ut == _up + _uf + _usk, ('la aritmetica de las ramas', _k)
assert CI_T4C == CI_T4C_EXPECTED, ('los totales calculados no son los medidos (CI_T4C_EXPECTED)', CI_T4C, CI_T4C_EXPECTED)
for _k, _lbl in (('RED', 'El RED: '), ('GREEN', 'El GREEN: ')):
    _t = CI_T4C[_k]
    _need = (re.escape('%sCore %s' % (_lbl, _sp(_t[0]))) + '(?: resultados)?' +
             re.escape(', con %s Passed, %d Failed y %s Skipped; UI %s, con %s Passed, %d Failed' % (_sp(_t[1]), _t[2], _sp(_t[3]), _sp(_t[4]), _sp(_t[5]), _t[6])))
    assert re.search(_need, _e14), ('[14] no da los totales calculados', _k, _t)
CI_MATCH = True
# las piezas por sha256 (6.3.2): su sha256 es el codigo que sigue a su nombre; en la revision al emitir, junto a --package-file en seco
PIECES = {}
# la fila "El esqueleto y el GREEN" de 6.3.2 da los tres sha256 en el orden de los tres nombres de su columna "Donde"
_row = [l for l in _s632.split('\n') if l.startswith('| **El esqueleto y el GREEN**')]
assert len(_row) == 1, '6.3.2: la fila del esqueleto y el GREEN'
_cells = _row[0].split(' | ')
_rn = re.findall(r'`([\w.-]+\.(?:txt|py))`', _cells[1])
_rh = re.findall(r'`([0-9a-f]{64})`', _cells[2].split('sha256:', 1)[1])
assert len(_rn) == len(_rh) == 3, ('6.3.2: tres nombres y tres sha256 en la fila del esqueleto y el GREEN', _rn, len(_rh))
_ROW_SHA = dict(zip(_rn, _rh))
for _n in PIECE_NAMES:
    _h = re.findall(r'`%s`\*{0,2}[^`]{0,40}?(?:sha256 \*{0,2})?`([0-9a-f]{64})`' % re.escape(_n), '\n'.join(PKG_LINES))
    _h += [_ROW_SHA[_n]] if _n in _ROW_SHA else []
    assert _h and len(set(_h)) == 1, ('el sha256 de una pieza en el paquete', _n, _h)
    PIECES[_n] = _h[0]
    _rev, _loc = D4 + _n, (os.path.join(os.path.dirname(PKG_FILE), _n) if PKG_FILE else None)
    if in_head(_rev):
        assert hashlib.sha256(show_bytes(_rev)).hexdigest() == _h[0], ('una pieza de la revision no es la del paquete', _n)
    else:
        assert _loc and os.path.isfile(_loc) and hashlib.sha256(open(_loc, 'rb').read()).hexdigest() == _h[0], ('una pieza no esta, o no es la del paquete', _n)
        soft(False, 'emision: %s (sha256 %s) aun no esta en la revision (la custodia X de PB-T4c)' % (_n, _h[0][:8]))
assert LIST_SHA in _s632 and P6_SCHEMA in _s632, '6.3.2 sin el sha256 de la lista o el esquema de la precondicion 6'


# ---------------------------------------------------------------- la precondicion 6 de PB-T4c (--p6-file en seco; al emitir, la de la revision): se lee
# antes de decidir los pendientes que dependen de ella (la leccion de la BASE de PB-T4b)
P6 = None
if P6_FILE:
    P6 = json.loads(open(P6_FILE, 'rb').read().decode('utf-8'))
elif in_head(P6_REL):
    P6 = json.loads(show(P6_REL))
if P6 is None:
    pending('la precondicion 6 de PB-T4c (%s: en seco con --p6-file, o en la revision)' % P6_REL_NAME)
else:
    assert P6.get('Schema') == P6_SCHEMA and P6.get('Revision') == PT4B_CLOSE, ('la precondicion 6: su forma o su revision', P6.get('Schema'), P6.get('Revision'))
    assert P6['List'] == {'Core': K_CORE, 'File': LIST_NAME, 'K': LIST_TOTAL, 'Sha256': LIST_SHA, 'UI': K_UI}, ('la precondicion 6: la lista', P6['List'])
    _pp = {k: v for k, v in PIECES.items() if k in P6['Pieces']}
    assert all(P6['Pieces'][k] == v for k, v in _pp.items()), ('la precondicion 6: una pieza no es la del paquete', [k for k, v in _pp.items() if P6['Pieces'][k] != v])
    assert sorted(P6['EquivalentSha']) == sorted(ALLOWED[18:22]), ('la precondicion 6: EquivalentSha no son los cuatro equivalentes del alcance', sorted(P6['EquivalentSha']))
    assert P6['D'] == D_C and P6['E'] == 4 and P6['CsWarnings'] == 0, ('la precondicion 6: D, E o los avisos CS', P6['D'], P6['E'], P6['CsWarnings'])
    for _k in ('Green', 'Red'):
        _g = P6[_k]['Core']  # sin las CT: la base, los Skip y sus equivalentes
        assert (_g['Total'], _g['Passed'], _g['Failed'], _g['Skipped']) == (CI_BASE_T4B[2] + K_CORE - D_C, CI_BASE_T4B[2] - CI_BASE_T4B[3], 0,
                                                                         CI_BASE_T4B[3] + K_CORE), ('la precondicion 6: Core sin las CT', _k, _g)
    # las cuatro claves de la v5.1, con el mismo Schema (6.3.2): el ensayo de las CT de texto (el paso 8) pasa en las dos ramas, su control sobre las piezas de
    # la v5.0 ve I-01, el censo de C13 pasa y las tablas de cada clase van por su sha256. Se leen antes de decidir su pendiente
    _p6miss = [k for k in P6_V51_KEYS if k not in P6]
    if _p6miss:
        pending('la precondicion 6 sin las claves de la %s que declara 6.3.2: %s' % (PKG_VERSION, ', '.join(_p6miss)))
    else:
        assert P6['Rehearsal'].get('Red') == 'PASA' and P6['Rehearsal'].get('Green') == 'PASA', ('la precondicion 6: el ensayo del paso 8', P6['Rehearsal'])
        assert P6['Control'] == 'VE I-01', ('la precondicion 6: el control del ensayo sobre la v5.0 tiene que ver I-01', P6['Control'])
        assert P6['Census'] == 'PASA', ('la precondicion 6: el censo de C13', P6['Census'])
        assert isinstance(P6['ClassResults'], dict) and P6['ClassResults'] and all(re.fullmatch(r'[0-9a-f]{64}', v) for v in P6['ClassResults'].values()), \
            'la precondicion 6: ClassResults, el sha256 de las tablas de cada clase'
    P6_EXTRA = sorted(set(P6) - {'Schema', 'Revision', 'List', 'Pieces', 'EquivalentSha', 'D', 'E', 'CsWarnings', 'Green', 'Red'} - set(P6_V51_KEYS))
    _p6undecl = [k for k in P6_EXTRA if k not in P6_UNREAD_V50 + P6_UNREAD_V51]
    if _p6undecl:
        pending('la precondicion 6 trae claves que el paquete no declara (6.3.2): %s; la sesion decide' % ', '.join(_p6undecl))

# ---------------------------------------------------------------- las autoridades (6.3.2): las del contrato de PB-T4b, por su numero
_A = pbt4b['Authorities']
assert len(_A) == 72, ('las autoridades del contrato de PB-T4b', len(_A))
for _i, _end in ((0, PKG), (6, 'superseding-characterization.md'), (28, 'f4b-dispositions.md'), (29, 'coordinator/gate-contract.json'),
                 (36, 'task-close-package.md'), (37, '/coordinator/gate-contract.json'), (65, 'independent-review-v1.md')):
    assert _A[_i]['Path'].endswith(_end), ('la autoridad n.o %d del contrato de PB-T4b no es la que nombra 6.3.2' % _i, _A[_i]['Path'])
assert all(a['Class'] == 'UNIT_CHANGE' for a in _A[66:72]), 'las n.o 66 a 71 del contrato de PB-T4b son sus UNIT_CHANGE'
_auth = []
for _i, a in enumerate(_A):
    if _i in range(43, 65) or _i in range(66, 72):
        continue  # las tandas, la lista, el mapa, el kit, los guiones y la precondicion 6 de PB-T4b, y sus UNIT_CHANGE: no son de PB-T4c
    a = dict(a)
    if _i == 0:
        a['Section'] = ('archivo completo (' + PKG_VERSION + ', sha256 ' + CUSTODY_SHA256[PKG][:8] + '): D-PBT4-12 a D-PBT4-17, section 4.3, 5.3, 6.0 y 6.3 (el '
                        'contrato de PB-T4c, sus literales de 6.3.1 y lo que necesita el generador, 6.3.2)')
    elif _i == 6:
        a['Section'] = 'la entrada "## PB-T4c" (las 4, por causa, con su tabla al cerrar; [13])'
    elif _i == 28:
        a['Section'] = 'hasta ' + T4B_CLOSE_ENTRY + ' (el cierre de PB-T4b); ' + EMIT_ENTRY + ', la emision de PB-T4c'
    elif _i == 29:
        a['Path'], a['Section'] = PBT4BC, 'el contrato anterior, el de PB-T4b (emitido; sus textos comunes, C11 y sus filtros)'
    elif _i == 36:
        a['Path'], a['Section'] = T4B_TCP, 'la base: el cierre de PB-T4b (' + T4B_CLOSE_ENTRY + '), con la CI de la linea base (' + CI_BASE_T4B[0] + ')'
    elif _i == 37:
        a['Path'], a['Section'] = PILOT + '/' + TASK + '/coordinator/gate-contract.json', 'este contrato'
    elif _i == 65:
        a['Section'] = ('secciones %s (la comprobacion acotada de PB-T4c, CA-T4c, sobre el contrato en seco y la %s) y %s (su recomprobacion, si la hay)'
                        % (BOUNDED_SECTIONS[0], CAT4C_PKG, BOUNDED_SECTIONS[1]))
    _auth.append(a)
_have = {a['Path'] for a in _auth}
_OWNER_C_SEC = {'owner-pbt4c-list-3-decision.md': 'la octava tanda: la lista pasa a 3, con CT-PBT4B-20',
                'owner-pbt4c-list-4-decision.md': 'la novena tanda: la lista pasa a 4, con CT-PBT2B-06'}
for _p, _s in ([(CO + _n, 'archivo completo (' + EMIT_ENTRY + '): ' + _OWNER_C_SEC[_n] + ' (decision del Owner, literal)') for _n in OWNER_C] +
               [(D4 + SESSION_DEC_C, 'archivo completo (' + EMIT_ENTRY + '): las decisiones de diseno de la sesion, D-S7 a D-S10'),
                (CO + 'owner-pbt2b-close-decisions.md', 'F4B-E38: A-T2b-OWNER-01 (:36)'),
                (CO + 'owner-pbt4b-code-decisions.md', 'F4B-E53: los vacios de M-04 van a la lista de PB-T4d, no a esta'),
                (D4 + LIST_NAME, 'la lista de PB-T4c (%d), sha256 %s' % (LIST_TOTAL, LIST_SHA[:8]))] +
               [(D4 + _n, 'una pieza de PB-T4c por sha256 (6.3.2), sha256 %s' % PIECES[_n][:8]) for _n in PIECE_NAMES] +
               [(P6_REL, 'la precondicion 6 de la sesion: la lista, las piezas, EquivalentSha, D y E, y los totales sin las CT')]):
    if _p not in _have:
        _auth.append({'Path': _p, 'Section': _s, 'Class': 'UNIT_DOC'})
_green_piece = [n for n in PIECE_NAMES if n.startswith('pbt4c-green-')]
assert len(_green_piece) == 1, ('la pieza del GREEN en PIECE_NAMES', _green_piece)
_auth += [{'Path': p, 'Section': 'el texto literal del GREEN de PB-T4c (' + _green_piece[0] + '; D-PBT4-12 a D-PBT4-17)', 'Class': 'UNIT_CHANGE'}
          for p in UNIT_CHANGE_P]

# ---------------------------------------------------------------- el contrato de PB-T4c
c = {}
for k in pbt4b:
    c[k] = pbt4b[k]
c.update({'TaskId': TASK, 'Objective': OBJECTIVE, 'AuthorityRevision': AR, 'MainSha': MAIN, 'Authorities': _auth, 'AllowedWriteScope': ALLOWED,
          'ForbiddenWriteScope': FORBIDDEN, 'Invariants': INV,
          'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
          'ExpectedEvidence': EVID, 'StopConditions': STOPS,
          'IssuedBy': ('Sesion principal de I-64 (Claude), emision de la tercera subtarea de PB-T4 (' + EMIT_ENTRY + ') por F4B-E49, pregunta 4 (la emision '
                       'condicional de PB-T4a, PB-T4b y PB-T4c en ese orden; owner-pbt4-decisions.md), con la lista exacta de ' + str(LIST_TOTAL) + ' que el '
                       'Owner tomo como la que vio (owner-pbt4-decisions.md, owner-pbt4c-list-3-decision.md y owner-pbt4c-list-4-decision.md): PB-T4c tras el '
                       'cierre de PB-T4b (' + PT4B_CLOSE[:8] + ', ' + T4B_CLOSE_ENTRY + '), con su comprobacion acotada (' +
                       (BOUNDED_RESULT or '<resultado por rellenar>') + '); las decisiones de diseno de la sesion (D-S7 a D-S10); F4B-E46; F4B-E40 y F4B-E43, '
                       'los sustitutos de nc1 y nc2; F4B-E29, D-1; F4B-E11, D2; F4B-E35, la regla RR; F4B-E21; F4B-E25, con las 9 de src/; y el paquete del '
                       'contrato de PB-T4 ' + PKG_VERSION),
          'IssuedUtc': ISSUED})
assert list(c) == list(pbt4b), 'las claves del contrato de PB-T4c no son las del de PB-T4b'
b = (json.dumps(c, indent=2, ensure_ascii=True) + '\n').encode('ascii')
assert all(x < 0x80 for x in b) and b'\\u' not in b, 'no ASCII'
SIZES = {k: len(json.dumps(c[k], indent=2, ensure_ascii=True).encode('ascii')) for k in c}
if JSON_OUT:
    open(JSON_OUT, 'wb').write(b)


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


# ---------------------------------------------------------------- la comprobacion acotada de PB-T4c (CA-T4c): BOUNDED_RESULT y bounded_check (el de
# make_pbt3a_contract.py, tal cual), sobre el tramo de independent-review-v1.md de BOUNDED_SECTIONS, como en PB-T4b; se lee antes de decidir su pendiente
BOUNDED_CHECK = D4 + 'independent-review-v1.md'


def cat_report(text):
    """El tramo de CA-T4c de independent-review-v1.md y los numeros de sus secciones; (None, []) si no tiene la primera (la de CA-T4c)."""
    _h = re.compile(r'## ' + chr(0xA7) + r'(\d+)\. ')
    lines = text.replace('\r\n', '\n').split('\n')
    k0 = [k for k, l in enumerate(lines) if _h.match(l) and _h.match(l).group(1) == BOUNDED_SECTIONS[0]]
    if len(k0) != 1 or 'CA-T4c' not in lines[k0[0]]:
        return None, []
    end = next((j for j in range(k0[0] + 1, len(lines)) if lines[j].startswith('## ') and not (_h.match(lines[j]) and _h.match(lines[j]).group(1)
                                                                                               in BOUNDED_SECTIONS)), len(lines))
    return '\n'.join(lines[k0[0]:end]), [_h.match(l).group(1) for l in lines[k0[0]:end] if _h.match(l)]


BOUNDED_SRC, BOUNDED_FOUND = None, []
if BOUNDED_FILE:
    _rep_all = open(BOUNDED_FILE, 'rb').read().decode('utf-8')
    BOUNDED_SRC = '--bounded-file ' + os.path.basename(BOUNDED_FILE)
elif in_head(BOUNDED_CHECK):
    _rep_all = show(BOUNDED_CHECK)
    BOUNDED_SRC = BOUNDED_CHECK + ' en ' + REV_SHA[:8]
if BOUNDED_SECTIONS is None:
    pending('la comprobacion acotada de PB-T4c (CA-T4c) sobre la %s: BOUNDED_SECTIONS (sus secciones de independent-review-v1.md), CAT4C_PKG y '
            'BOUNDED_RESULT, el veredicto literal' % PKG_TARGET)
    BOUNDED_SRC = None
elif BOUNDED_SRC:
    _rep, BOUNDED_FOUND = cat_report(_rep_all)
    if _rep is None:
        soft(False, 'emision: la comprobacion acotada de PB-T4c aun no esta: %s no tiene la seccion %s de CA-T4c' % (BOUNDED_SRC, BOUNDED_SECTIONS[0]))
        BOUNDED_SRC = None
    else:
        assert BOUNDED_FOUND == list(BOUNDED_SECTIONS[:len(BOUNDED_FOUND)]), ('las secciones de CA-T4c, seguidas y en su orden', BOUNDED_FOUND)
        soft(BOUNDED_FOUND == list(BOUNDED_SECTIONS), 'emision: de las secciones %s de CA-T4c, %s solo tiene la %s (falta la recomprobacion)'
             % (' y '.join(BOUNDED_SECTIONS), BOUNDED_SRC, ' y la '.join(BOUNDED_FOUND)))
        soft(BOUNDED_RESULT, 'emision: falta BOUNDED_RESULT, el veredicto literal de CA-T4c')
        if BOUNDED_RESULT:
            assert re.match(r'EMITIBLE(?: TRAS CORRECCIONES)?[.,;]', BOUNDED_RESULT), ('BOUNDED_RESULT: solo la familia EMITIBLE', BOUNDED_RESULT)
            _err = bounded_check(_rep, BOUNDED_RESULT, PKG_LINES, reviewer='CA-T4c')
            assert not _err, ('la comprobacion acotada de PB-T4c', BOUNDED_SRC, _err)
else:
    soft(False, 'emision: la comprobacion acotada de PB-T4c (%s) no esta ni en la revision ni en --bounded-file' % BOUNDED_CHECK)
_sup = show(PILOT + '/F4B-DRAFTS/superseding-characterization.md')
soft('\n## PB-T4c ' in _sup, 'emision: el registro declarado aun no tiene la entrada de PB-T4c (las %d, con su tabla al cerrar)' % LIST_TOTAL)
# la n.o 37, "este contrato" (6.3.2), no esta en X por construccion: lo escribe la BASE. No cuenta como autoridad que falta (al emitir sobre X abortaria)
SELF_CONTRACT = PILOT + '/' + TASK + '/coordinator/gate-contract.json'
assert c['Authorities'][37]['Path'] == SELF_CONTRACT and c['Authorities'][37]['Section'] == 'este contrato', ('la n.o 37 es este contrato (6.3.2)',
                                                                                                           c['Authorities'][37])
assert [a['Path'] for a in c['Authorities']].count(SELF_CONTRACT) == 1
AUTH_MISSING = [a['Path'] for a in c['Authorities'] if not in_head(a['Path']) and a['Path'] != SELF_CONTRACT]

print('DRY (FASE 2) rev', REV_SHA[:8], '(objetos del checkout principal)' if OBJ_REV else '(HEAD del worktree)', 'TaskId', TASK,
      '| contrato en seco: PENDIENTES', len(PENDING), 'AVISOS', len(WARN))
print('anclaje: desciende de %s y %s (PB-T4b, %s; X y BASE), de su RED %s, de su reparacion %s, de su GREEN %s y de su cierre %s; src y tests == %s'
      % (PT4B_EMIT_X[:8], PT4B_EMIT[:8], T4B_EMIT_ENTRY, RED_T4B[:8], RR_T4B[:8], GREEN_T4B[:8], PT4B_CLOSE[:8], GREEN_T4B[:8]))
print('numeracion prevista: %s (la emision); la evidencia %s (X) y %s (BASE) | hoy: F4B-E%d y la %s' % (EMIT_ENTRY, X_EV, BASE_EV, _ids[-1], _evs[-1]))
print('la CI del GREEN de PB-T4b (%s, la base C): Core %d = %d + 0 + %d, UI %d = %d + 0 + %d' % (CI_BASE_T4B[0], CI_BASE_T4B[2], CI_BASE_T4B[2] - CI_BASE_T4B[3],
                                                                                              CI_BASE_T4B[3], CI_BASE_T4B[4], CI_BASE_T4B[4] - CI_BASE_T4B[5],
                                                                                              CI_BASE_T4B[5]))
print('contrato de PB-T4b (%s, AuthorityRevision %s): 17 paradas estandar, PBT4B-C11 -> PBT4C-C11 (%d caracteres), EVID[0] en F4B_EXACT_EVIDENCES del v2, '
      '%d filtros' % (PBT4BC, pbt4b['AuthorityRevision'][:8], len(C11), len(pbt4b['RequiredTests'])))
print('la lista de PB-T4c (%d; %s): %s'
      % (LIST_TOTAL, ('%s, %s' % LIST_SRC) if LIST_SRC else 'las filas "PB-T4c" de pbt4-substitution-lists.tsv', '; '.join('%s:%d %s' % (os.path.basename(p), t['line'], m[:40]) for (p, m), t in sorted(LISTED.items()))))
print('los filtros heredados de PB-T4b con los Skip de la lista: %d filas, %d cambian; pruebas de la lista fuera de todo filtro heredado: %s'
      % (len(REQ_INH), len(REQ_REPORT), ', '.join(LIST_UNFILTERED) or 'ninguna'))
for _l in REQ_REPORT:
    print('  ' + _l)
print('6.3 de la %s: Objective %d caracteres, ASCII; alcance %d rutas (UNIT_CHANGE %d, nuevas de src/ %d, de tests/ %d, Skip %d); RequiredTests %d (76 heredados, el n.o 43 '
      'fuera, y 9 nuevos; %d con ExpectRed); piezas por sha256: %s' % (PKG_VERSION, len(OBJECTIVE), len(ALLOWED), len(UNIT_CHANGE_P), len([a for a in ALLOWED[9:13]]),
                                                                    len(ALLOWED[13:22]), len(SKIP_FILES), len(REQ), len(_er), '; '.join('%s %s' % (k, v[:8]) for k, v in PIECES.items())))
if P6 is not None:
    print('la precondicion 6 (%s, sha256 %s): la lista, las %d piezas que nombra, EquivalentSha de 4, D %d, E %d, 0 avisos CS y Core sin las CT %d (GREEN y RED); '
          'las claves de la v5.1, leidas: %s; campos que no lee: %s' % (
              os.path.basename(P6_FILE) if P6_FILE else 'en la revision', hashlib.sha256(open(P6_FILE, 'rb').read()).hexdigest()[:8] if P6_FILE else '-', len(_pp),
              P6['D'], P6['E'], P6['Green']['Core']['Total'],
              ('Rehearsal (RED %s, GREEN %s), Control %s, Census %s, ClassResults %d tablas' % (P6['Rehearsal']['Red'], P6['Rehearsal']['Green'], P6['Control'],
                                                                                              P6['Census'], len(P6['ClassResults'])))
              if all(k in P6 for k in P6_V51_KEYS) else 'faltan', ', '.join(P6_EXTRA)))
print('la CI prevista (la lista de %d: K_CORE %d y K_UI %d; D_C %d; sobre C): RED Core %d = %d + %d Failed + %d, UI %d = %d + %d Failed + %d; GREEN Core %d = %d '
      '+ 0 + %d, UI %d = %d + 0 + %d; %s los que midio el autor con la de 4'
      % ((LIST_TOTAL, K_CORE, K_UI, D_C) + tuple(CI_T4C['RED'][:4]) + tuple(CI_T4C['RED'][4:]) + (CI_T4C['GREEN'][0], CI_T4C['GREEN'][1], CI_T4C['GREEN'][3],
                                                                                                CI_T4C['GREEN'][4], CI_T4C['GREEN'][5], CI_T4C['GREEN'][7],
                                                                                                'iguales a' if CI_MATCH else 'DISTINTOS de')))
print('CONTRATO (en seco) allowed %d forbidden %d (src %d tests %d [prefijos partidos: %s] root %d) inv %d stops %d required %d (de %d) evidence %d authorities %d bytes %d sha256 %s'
      % (len(ALLOWED), len(FORBIDDEN), len(SRCF), len(TESTF), ', '.join(SPLIT2) or 'ninguno', len(ROOT), len(INV), len(STOPS), len(REQ), N_REQ_ROWS,
         len(EVID), len(c['Authorities']), len(b), hashlib.sha256(b).hexdigest().upper()))
print('BOUNDED_RESULT: %s | bounded_check: %s' % (BOUNDED_RESULT or 'por rellenar', (('sin errores sobre ' + BOUNDED_SRC) if (BOUNDED_SRC and BOUNDED_RESULT)
                                                  else ('no corre (informe: %s)' % BOUNDED_SRC if BOUNDED_SRC else 'sin informe'))
                                                  + ((', secciones ' + ' y '.join(BOUNDED_FOUND)) if BOUNDED_SRC else '')))
print('autoridades que aun no estan en la revision (sin la n.o 37, este contrato, que escribe la BASE): %d%s'
      % (len(AUTH_MISSING), ''.join('\n  ' + p for p in AUTH_MISSING)))
print('PENDIENTES (%d):' % len(PENDING))
for p in PENDING:
    print('  - ' + p)
print('AVISOS (%d):' % len(WARN))
for w in WARN:
    print('  - ' + w)
assert not (AS_WRITE and AUTH_MISSING), ('como al emitir: una autoridad no esta en la revision', AUTH_MISSING)
if DRY:
    sys.exit(0)
# ---------------------------------------------------------------- la emision (sin --dry): en el worktree, sobre el commit X, como la de PB-T4b; todo lo que
# en seco es AVISO o PENDIENTE ya abortaba (AS_WRITE), y la precondicion 6 y CA-T4c se leen de la revision antes de decidirlos
assert OBJ_REV is None and not PKG_FILE and not P6_FILE and not BOUNDED_FILE and not JSON_OUT, \
    'al emitir, solo el worktree: sin --objects-rev, --package-file, --p6-file, --bounded-file ni --json-out'
assert REV_SHA == AR, ('al emitir, la AuthorityRevision es el commit X, el HEAD del worktree', AR[:8], REV_SHA[:8])
assert PKG_VERSION == PKG_TARGET and BOUNDED_RESULT and BOUNDED_SRC and BOUNDED_FOUND == list(BOUNDED_SECTIONS) and E54_OK and P6 is not None and \
    in_head(PKG) and hashlib.sha256(show_bytes(PKG)).hexdigest() == CUSTODY_SHA256[PKG], 'faltan la v5.0, CA-T4c, F4B-E54, la precondicion 6 o el paquete'
assert not PENDING and not WARN and not AUTH_MISSING, (PENDING, WARN, AUTH_MISSING)
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', "Get-Content -Raw '%s' | Test-Json -SchemaFile "
                    "'docs/automation/agent-execution/schemas/gate-contract.schema.json'" % OUT], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True', 'sha256', hashlib.sha256(b).hexdigest().upper())
sys.exit(0)
