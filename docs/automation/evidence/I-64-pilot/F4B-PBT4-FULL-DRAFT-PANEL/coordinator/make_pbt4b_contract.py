"""Contrato de gate de F4B-PBT4-FULL-DRAFT-PANEL (PB-T4b; ASCII), desde el paquete del contrato de PB-T4 (PKG_VERSION, abajo), con el patron de
make_pbt4a_contract.py. FASE 2: por objetos git sobre el cierre de PB-T4a (PT4A_CLOSE; F4B-E51, la evidencia 279). PB-T4b se emite con
F4B-E52, la evidencia 280 (X) y 281 (BASE): parametros, comprobados al emitir.
Comprueba por objetos git:
- el anclaje: la revision desciende del cierre de PB-T3c, de la emision de PB-T4a (X y BASE) y de su cierre; la numeracion prevista;
- el contrato de PB-T4a de su emision: las 17 paradas estandar, PBT4A-C11 (de la que sale PBT4B-C11), la primera ExpectedEvidence y sus 38 filtros;
- los sustitutos de nc1 y de nc2 v2 con --task PBT4, y la Snapshot K;
- la lista de PB-T4b (LIST_NAME, por su sha256): la de 470 (la historia: el filtrado byte a byte de pbt4-substitution-lists.tsv, D-PBT4-11) en su
  orden, mas exactamente las filas de LIST_ADDED_EXPECTED (las de las tandas del Owner); sus recuentos (LIST_COUNTS), cada prueba en su archivo de la
  revision, con su atributo sin Skip y su linea dentro del rango; el mapa de la copia (COPY_MAP_NAME, por su sha256, con COPY_COUNTS);
- el alcance: AllowedWriteScope del paquete con los archivos de los Skip; ForbiddenWriteScope por la regla de cover() y el precedente de PB-T4a;
- los RequiredTests de 6.2.1 (N_REQ_ROWS filas): los heredados de PB-T4a con los MinSelected que dejan los Skip de la lista (con el criterio de
  count_tests_git.py), los nuevos, y los de las clases de las copias con los MinSelected de la precondicion 6 (--p6-file en seco, o
  pbt4b-precondition6.json en la revision; un 0 saca la fila);
- el paquete, por su sha256 (CUSTODY_SHA256): el bloque 6.2.1 de PB-T4b, literal, y las piezas por su sha256;
- la comprobacion acotada de PB-T4b (CA-T4b: las secciones 20 y 21 de independent-review-v1.md, BOUNDED_SECTIONS; o --bounded-file con ese
  archivo) frente a BOUNDED_RESULT, con bounded_check.
No escribe ningun contrato: en seco lista los PENDIENTES y los AVISOS; sin --dry aborta.
Uso: python make_pbt4b_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> --dry [--objects-rev <rev> [--repo <checkout principal>]
                                   [--package-file <paquete>] [--p6-file <pbt4b-precondition6.json>] [--bounded-file <independent-review-v1.md>]]
                                   [--json-out <archivo>] [--as-write]
Al custodiarlo, las rutas locales se redactan como en PB-T0 ('<worktree-de-I-64>', '<checkout-principal>')."""
import json, os, re, sys, subprocess, hashlib, unicodedata

# la leccion del cierre de PB-T3c: el generador entero es ASCII (lo leen en ascii los check_* y la redaccion); BOUNDED_RESULT tambien
_self = open(os.path.abspath(__file__), 'rb').read()
assert all(b < 0x80 for b in _self), ('el generador lleva un caracter no ASCII', [i for i, b in enumerate(_self) if b >= 0x80][:5])

# ---------------------------------------------------------------- la base: la emision de PB-T4a (F4B-E50) y, al emitir, su cierre (F4B-E51)
PT4A_EMIT = '5aaacce3b278c1f29375de4f24c7fab232f44ad2'     # BASE de la emision de PB-T4a (F4B-E50, la evidencia 278)
PT4A_EMIT_X = '27c13fabcd692678daca0095476c58fd175fbe79'   # X de la emision de PB-T4a (la evidencia 277): AuthorityRevision de su contrato
PT3C_CLOSE = '7f0d9aa377ca1e3301bfce3716bd29d602d762dd'    # el cierre de PB-T3c (F4B-E48): src y tests de la emision de PB-T4a
D1_REV = 'd8ccbcb870f4d1804d7e7afd3855ac64b44693a8'
# el cierre de PB-T4a (la sesion, al cerrarlo): None hasta entonces (PENDIENTE)
PT4A_CLOSE = '78a7cef3e9260f0955addd48344fc5a44b9c9957'     # la custodia del cierre de PB-T4a (F4B-E51, la 279); HEAD al emitir PB-T4b
GREEN_T4A = 'faef702055990d1350a73bf4d254eb9e898d5670'      # el GREEN de PB-T4a (intento 1)
RED_T4A = '214f8c75f2ca5a248a5592075c3bac8e667bd362'        # el RED de PB-T4a
# la CI del GREEN de PB-T4a en su cierre: (run, sha8 del GREEN, Core, Skipped de Core, UI, Skipped de UI); lo previsto por su contrato ([14])
CI_BASE_T4A = ('44814418683', 'faef7020', 19137, 7, 2451, 21)  # la CI del GREEN de PB-T4a (4/4); la del RED, 44810371492
# CT-PBT4A-06 en el cierre de PB-T4a (la sesion, sobre su GREEN faef7020): su nombre real y su linea; la lista lo nombra "CT_PBT4A_06 (nueva en PB-T4a)"
CT_PBT4A_06_NAME = 'CT_PBT4A_06_OnlyTheFolderOfTheModelThePlanTheRunAndTheAdapterOfThePluginNameItsTypes'
CT_PBT4A_06_LINE = 98
# la numeracion (la sesion): parametros comprobados contra f4b-dispositions.md y la evidencia (una asignacion cada uno, para leerlos con ast)
T4A_EMIT_ENTRY = 'F4B-E50'
T4A_CLOSE_ENTRY = 'F4B-E51'
EMIT_ENTRY = 'F4B-E52'
T4A_X_EV = '277'
T4A_BASE_EV = '278'
CLOSE_EV = '279'
X_EV = '280'
BASE_EV = '281'
T4A_TASK = 'F4B-PBT4-FULL-READ-PATH'
# el veredicto literal de CA-T4b (ASCII): la linea BOUNDED de la seccion 21.7 de independent-review-v1.md, la de la 20.11 con la cola de la recomprobacion
BOUNDED_RESULT = ('EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 8; las importantes corregidas en el paquete v4.9 (I-01, la autoprueba de la guarda de los textos '
                  'del anfitrion pasa en vacio en el GREEN: el equivalente de NoneOfThe30 trae su autoprueba sobre la tabla nueva, como G-4); sin decisiones '
                  'nuevas del Owner salvo el coste de la lectura (pb-20-ab x4, origen-es-Primary, primera corrida); precondicion 6 sobre 78a7cef3: pasos 1 a 6 '
                  'cumplen; recomprobacion v4.9: EMITIBLE, B 0 / I 0 / M 3')
# CA-T4b M-08: el informe de CA-T4b son las secciones 20 (CA-T4b, sobre CAT4B_PKG) y 21 (su recomprobacion) de independent-review-v1.md; no hay otro
# archivo. El encargo, independent-review-pbt4b-check-prompt.md (de _write_cat4b_prompt.py)
BOUNDED_SECTIONS = ('20', '21')
CAT4B_PKG = 'v4.8'
PKG_VERSION = 'v4.9'    # la version del paquete con la que se emite PB-T4b (la v4.9: CA-T4b I-01 y M-01 a M-04; la septima tanda; el paso 7 de la v4.8)
CUSTODY_SHA256 = {'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md': '043f196d8452588530784eb04fd3cd3b63c3594d3e961df5e204f0109329c886'}  # la v4.9
# la lista de PB-T4b, por sha256. La de 470 (V42-03), el filtrado de pbt4-substitution-lists.tsv (D-PBT4-11), queda como historia; la decision del
# Owner "Anadirlas a la lista" (owner-pbt4b-breaks-decision.md) la lleva a 475 con un archivo nuevo, cuyo nombre, sha256 y recuentos fija la v4.4:
# LIST_NAME, LIST_SHA y LIST_COUNTS son parametros (hoy, los de la de 478)
LIST_470_NAME = 'pbt4b-substitution-list.tsv'
LIST_470_SHA = '44393b19d1f7490081c8ec9bac633ea4acbfe792103e5e5c3ca5e1f3fa1163e4'
LISTS_ALL_SHA = 'f24c2dbc41bdd3d989796bee420b500fdf722d9f93bd408f5580c62019c8dfa5'
LIST_NAME = 'pbt4b-substitution-list-478.tsv'
LIST_SHA = '8b0fcd0ca775ce3a5e46ef2059f4b06e7ec7319883daf3cb37b1fe91290e9e8b'
LIST_COUNTS = {'PB-T4b C-0': 428, 'PB-T4b C-5': 23, 'PB-T4b UI': 27}
# las 8 que anaden las tandas del Owner (archivo, linea que nombra): las cinco de la tercera (owner-pbt4b-breaks-decision.md; la quinta sale de la lista de PB-T4d)
LIST_ADDED_EXPECTED = [('tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs', 871), ('tests/RackCad.Tests/Workspace/WorkspaceEditIntentGuardTests.cs', 883),
                       ('tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs', 627), ('tests/RackCad.Tests/Workspace/WorkspaceLifecycleGuardTests.cs', 642),
                       ('tests/RackCad.Tests/Workspace/WorkspacePilotUpdateGuardTests.cs', 823),
                       # las dos de la quinta tanda (owner-pbt4b-list-477-decision.md): CT-PBT4A-03 y CT-PBT4A-05, en el cierre de PB-T4a
                       ('tests/RackCad.Tests/PushBackFullReadPathTests.cs', 153), ('tests/RackCad.Tests/PushBackFullReadGuardTests.cs', 54),
                       # la de la sexta tanda (owner-pbt4b-list-478-decision.md)
                       ('tests/RackCad.Tests/Workspace/WorkspaceHostHardeningGuardTests.cs', 492)]
LIST_ADDED_FROM_T4D = 1
# lo que fija la version del paquete (v4.5): UNIT_CHANGE (con RIA, D-S4), las piezas por sha256 (sus nombres en el paquete), las filas de RequiredTests
UNIT_CHANGE_LINE = '- **`UNIT_CHANGE`:** WHC, WEI, FVM, PV, `NumericFieldValidation.cs` y RIA (F4B-E25).'
UNIT_CHANGE_EXTRA = ['src/RackCad.Plugin/Views/RackPilotIntentAdapter.cs']  # RIA (D-S4)
KIT_NAME = 'pbt4b-kit-WorkspaceFullReadKit-v45.cs'
# las piezas por sha256: las de la fila "El kit, el guion de la copia, los equivalentes y la particion" de 6.2.2 de la v4.9 (el kit, los cinco guiones de la copia
# y las seis ediciones a mano), y la revision de los equivalentes; su sha256, del paquete
PIECE_NAMES = [KIT_NAME, 'make_copies_pbt4b_v49.py', 'make_copies_pbt4b_v48.py', 'make_copies_pbt4b_v47.py', 'make_copies_pbt4b_v46.py', 'make_copies_pbt4b.py',
               'pbt4b_hand_edits_v49.py', 'pbt4b_hand_edits_v48.py', 'pbt4b_hand_edits_v47.py', 'pbt4b_hand_edits_v46.py', 'pbt4b_hand_edits_v45.py',
               'pbt4b_hand_edits.py',
               'pbt4b-equivalents-review-v49.txt']
N_REQ_ROWS = 88
N_CT = 21  # CT-PBT4B-01..21 (CT-21, D-S4)
# los MinSelected heredados que fija la decision del Owner con la lista de 475 (por el nombre de la clase del filtro)
MIN_EXPECTED_475 = {'WorkspacePilotUpdateGuardTests': 85}
# D-S1 (session-pbt4b-design-decisions.md): el reparto de las 428 entre copias mecanicas y equivalentes a mano deja de ser fijo; lo fijan la version del
# paquete que se emita y su mapa de la copia (sha256), con el reparto final que mida la precondicion 6. Hoy, el mapa v5 de la v4.8 y la v4.9 (356/72); la lista,
# la de 478 (LIST_SHA)
COPY_COUNTS = {'mecanica': 356, 'a mano': 72}
COPY_MAP_NAME = 'pbt4b-copy-map-v5.tsv'
COPY_MAP_SHA = 'bda79bb7fb6e1dfe885cdc5942b2af45dfac8861663ccc99dbb38a61b8844524'
SESSION_DEC = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt4/session-pbt4b-design-decisions.md'  # D-S1 a D-S6, a custodiar con la emision (F4B-E52)
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
VALUED = ('--objects-rev', '--repo', '--package-file', '--json-out', '--bounded-file', '--p6-file')


def opt(name):
    if name not in ARGS:
        return None
    i = ARGS.index(name)
    assert i + 1 < len(ARGS) and not ARGS[i + 1].startswith('--'), ('falta el valor de', name)
    return ARGS[i + 1]


POS = [a for i, a in enumerate(ARGS) if not a.startswith('--') and not (i > 0 and ARGS[i - 1] in VALUED)]
assert len(POS) == 3, 'uso: make_pbt4b_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> --dry [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
DRY = '--dry' in ARGS
# sin --dry, el generador emite (en el worktree, sobre el commit X, como el de PB-T4a): lo que en seco es un AVISO o un PENDIENTE aborta (como --as-write)
AS_WRITE = '--as-write' in ARGS or not DRY
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % 'F4B-PBT4-FULL-DRAFT-PANEL'
OBJ_REV = opt('--objects-rev')
PKG_FILE = opt('--package-file')
assert PKG_FILE is None or OBJ_REV is not None, '--package-file solo junto con --objects-rev'
PKG_FILE = os.path.abspath(PKG_FILE) if PKG_FILE else None
assert PKG_FILE is None or os.path.isfile(PKG_FILE), ('no existe el paquete', PKG_FILE)
P6_FILE = opt('--p6-file')  # la precondicion 6 de la sesion (pbt4b-precondition6.json), en seco; al emitir, la de la revision
P6_FILE = os.path.abspath(P6_FILE) if P6_FILE else None
assert P6_FILE is None or os.path.isfile(P6_FILE), ('no existe la precondicion 6', P6_FILE)
BOUNDED_FILE = opt('--bounded-file')  # independent-review-v1.md con las secciones de CA-T4b, en seco; al emitir, el de la revision
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
DISK = OBJ_REV is None

TASK = 'F4B-PBT4-FULL-DRAFT-PANEL'
assert len(TASK) <= 26 and TASK.startswith('F4B-' + NC_TASK + '-'), len(TASK)  # R-T1-07 (MAX_PATH, una cota superior); la ligadura de --task PBT4
PILOT = 'docs/automation/evidence/I-64-pilot'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
D4 = PILOT + '/F4B-DRAFTS/pbt4/'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
EVIDENCE = 'docs/automation/evidence/I-64-evidence.md'
PBT4AC = PILOT + '/' + T4A_TASK + '/coordinator/gate-contract.json'
T4A_TCP = PILOT + '/' + T4A_TASK + '/task-close-package.md'
OWNER_DEC = CO + 'owner-pbt4-decisions.md'
OWNER_DEC2 = CO + 'owner-pbt4b-retire-list-decisions.md'
PKG = 'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md'
OWNER_DEC3 = CO + 'owner-pbt4b-breaks-decision.md'  # la tercera decision del Owner sobre PB-T4 (las cinco roturas: la lista pasa a 475)
OWNER_DEC4 = CO + 'owner-pbt4b-list-477-decision.md'  # la quinta tanda: CT-PBT4A-03 y 05 (la lista pasa a 477), a custodiar en X de PB-T4b
OWNER_DEC5 = CO + 'owner-pbt4b-list-478-decision.md'  # la sexta tanda: WorkspaceHostHardeningGuardTests:492 (la lista pasa a 478), a custodiar en X
OWNER_DEC6 = CO + 'owner-pbt4b-cost-decision.md'  # la decision del Owner sobre el coste de PB-T4b (la septima tanda, tras la tercera corrida): a custodiar en X
COST_DEC = CO + 'owner-pbt4a-cost-decision.md'  # la del coste de la lectura (custodiada en el cierre de PB-T4a), autoridad de PB-T4b
LISTS_ALL = D4 + 'pbt4-substitution-lists.tsv'
COPY_MAP = D4 + 'pbt4-copy-map.tsv'
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
WARN = []
PENDING = []  # lo que espera al cierre de PB-T4a o a la version del paquete con el contrato entero de PB-T4b: el seco lo lista
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
    """Lo que solo existira con la emision de PB-T4b (su commit X, su comprobacion acotada): AVISO en seco, error con --as-write."""
    if cond:
        return True
    assert not AS_WRITE, msg
    WARN.append(msg)
    return False


def pending(msg):
    """Lo que espera al cierre de PB-T4a o al contrato entero de PB-T4b en el paquete: se lista en seco (PENDIENTE); con --as-write aborta."""
    assert not AS_WRITE, 'pendiente: ' + msg
    PENDING.append(msg)


REV_SHA = git('rev-parse', REV).stdout.decode('ascii').strip()
assert re.fullmatch(r'[0-9a-f]{40}', REV_SHA), ('revision desconocida', REV)

# ---------------------------------------------------------------- el anclaje: la emision de PB-T4a (F4B-E50) y, al emitir, su cierre (F4B-E51)
for _short, _full in (('5aaacce3', PT4A_EMIT), ('27c13fab', PT4A_EMIT_X), ('7f0d9aa3', PT3C_CLOSE), ('d8ccbcb8', D1_REV)):
    assert git('rev-parse', _short).stdout.decode('ascii').strip() == _full, ('SHA', _short)
for _anc, _name in ((D1_REV, 'F4B-E29 (D-1)'), (PT3C_CLOSE, 'el cierre de PB-T3c (F4B-E48)'), (PT4A_EMIT_X, 'X de la emision de PB-T4a (F4B-E50)'),
                    (PT4A_EMIT, 'BASE de la emision de PB-T4a (F4B-E50)')):
    assert git('merge-base', '--is-ancestor', _anc, REV).returncode == 0, ('la revision no desciende de ' + _name)
assert git('rev-parse', PT4A_EMIT + '^').stdout.decode('ascii').strip() == PT4A_EMIT_X, 'BASE de PB-T4a no es hijo directo de su X'
assert git('diff', '--quiet', PT3C_CLOSE, PT4A_EMIT, '--', 'src', 'tests').returncode == 0, 'la emision de PB-T4a cambio src o tests'
_disp = show(DISP).replace('\r\n', '\n')
assert re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (T4A_EMIT_ENTRY, T4A_TASK), _disp), 'F4B-E50, la emision de PB-T4a, en f4b-dispositions.md'
_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
assert _ids == list(range(1, len(_ids) + 1)) and _ids[-1] >= 50, ('f4b-dispositions.md', _ids[-3:])
_n = lambda e: int(e.split('E')[-1])
assert _n(T4A_CLOSE_ENTRY) == _n(T4A_EMIT_ENTRY) + 1 and _n(EMIT_ENTRY) == _n(T4A_CLOSE_ENTRY) + 1, ('la numeracion: E51 sigue a E50 y E52 a E51',
                                                                                                    T4A_CLOSE_ENTRY, EMIT_ENTRY)
assert _ids[-1] in (_n(T4A_EMIT_ENTRY), _n(T4A_CLOSE_ENTRY), _n(EMIT_ENTRY)), ('la ultima entrada de f4b-dispositions.md', _ids[-1])
E51_OK = bool(re.search(r'(?m)^## %s\. Cierre de %s\b' % (T4A_CLOSE_ENTRY, T4A_TASK), _disp))
E52_OK = bool(re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (EMIT_ENTRY, TASK), _disp))
_evs = re.findall(r'(?m)^## (\d+)\. ', show(EVIDENCE))
assert int(CLOSE_EV) == int(T4A_BASE_EV) + 1 and int(X_EV) == int(CLOSE_EV) + 1 and int(BASE_EV) == int(X_EV) + 1 and int(T4A_BASE_EV) == int(T4A_X_EV) + 1, \
    ('la numeracion de la evidencia', CLOSE_EV, X_EV, BASE_EV)
assert _evs[-1] in (T4A_BASE_EV, CLOSE_EV, X_EV), ('la ultima seccion de la evidencia', _evs[-3:])
assert T4A_X_EV in _evs and T4A_BASE_EV in _evs, 'las secciones 277 y 278 de la emision de PB-T4a'
if PT4A_CLOSE is None:
    pending('el cierre de PB-T4a (%s, la evidencia %s): PT4A_CLOSE, GREEN_T4A, RED_T4A y la CI de su GREEN (CI_BASE_T4A: el run y el sha8; lo previsto, '
            'Core 19 137 = 19 130 + 0 + 7 y UI 2 451 = 2 430 + 21); hoy la ultima entrada es F4B-E%d y la ultima seccion, la %s' % (T4A_CLOSE_ENTRY, CLOSE_EV,
                                                                                                                              _ids[-1], _evs[-1]))
    SRC_BASE = PT4A_EMIT  # en la fase 1, src y tests de la revision frente a la emision de PB-T4a
else:
    for _sha in (PT4A_CLOSE, GREEN_T4A, RED_T4A):
        assert _sha and re.fullmatch(r'[0-9a-f]{40}', _sha), ('el cierre de PB-T4a: un SHA', _sha)
    for _anc, _name in ((PT4A_EMIT, 'su emision'), (RED_T4A, 'su RED'), (GREEN_T4A, 'su GREEN'), (PT4A_CLOSE, 'su cierre')):
        assert git('merge-base', '--is-ancestor', _anc, REV).returncode == 0, ('la revision no desciende de PB-T4a: ' + _name)
    assert git('diff', '--quiet', GREEN_T4A, PT4A_CLOSE, '--', 'src', 'tests').returncode == 0, 'el cierre de PB-T4a no tiene el src y los tests de su GREEN'
    _dc = show(DISP, PT4A_CLOSE).replace('\r\n', '\n')
    assert re.split(r'(?m)^## ', _dc)[-1].startswith(T4A_CLOSE_ENTRY + '. Cierre de ' + T4A_TASK), 'el cierre de PB-T4a, la ultima entrada de su DISP'
    _ct = _dc[_dc.index('\n## %s. ' % T4A_CLOSE_ENTRY):]
    _run, _sha8, _core, _cskip, _ui, _uskip = CI_BASE_T4A
    _sp = lambda v: format(v, ',').replace(',', ' ')
    assert _run and GREEN_T4A.startswith(_sha8) and _run in _ct and 'CLOSED' in _ct, ('la entrada de cierre de PB-T4a', CI_BASE_T4A)
    assert ('Core %s = %s + 0 + %d' % (_sp(_core), _sp(_core - _cskip), _cskip)) in _ct and ('UI %s = %s + %d' % (_sp(_ui), _sp(_ui - _uskip), _uskip)) in _ct, \
        ('los recuentos de CI_BASE_T4A no son los de la entrada de cierre de PB-T4a', CI_BASE_T4A)
    assert 'CLOSED' in show(T4A_TCP, PT4A_CLOSE), 'el task-close-package de PB-T4a no dice CLOSED'
    SRC_BASE = PT4A_CLOSE
assert git('diff', '--quiet', SRC_BASE, REV, '--', 'src', 'tests').returncode == 0, \
    ('src o tests cambiaron desde %s (el diff frente a la base de PB-T4b, vacio)' % SRC_BASE[:8])
soft(E51_OK, 'emision: %s (el cierre de PB-T4a) aun no esta en f4b-dispositions.md' % T4A_CLOSE_ENTRY)
soft(E52_OK, 'emision: %s (la emision de %s) aun no esta en f4b-dispositions.md' % (EMIT_ENTRY, TASK))
if soft(git('cat-file', '-e', AR + '^{commit}').returncode == 0, 'emision: la AuthorityRevision %s no esta en el almacen de objetos (seco)' % AR[:8]):
    assert git('merge-base', '--is-ancestor', PT4A_EMIT, AR).returncode == 0, 'la AuthorityRevision no contiene la emision de PB-T4a'
for _p in (OWNER_DEC, OWNER_DEC2, PKG, D4 + LIST_470_NAME, LISTS_ALL, COPY_MAP):
    assert in_head(_p), ('custodiado con la emision de PB-T4a y no esta en la revision', _p)

# ---------------------------------------------------------------- el contrato de PB-T4a de su emision: de el salen los textos comunes y C11
pbt4a = json.loads(show(PBT4AC))
assert pbt4a['TaskId'] == T4A_TASK and pbt4a['AuthorityRevision'] == PT4A_EMIT_X, ('el contrato de PB-T4a', pbt4a['AuthorityRevision'][:8])
assert pbt4a['ExpectedEvidence'] and len(pbt4a['RequiredTests']) == 38 and len(pbt4a['StopConditions']) == 34, ('el contrato de PB-T4a', len(pbt4a['RequiredTests']))
T4A_STOP = {s['Id']: s['Text'] for s in pbt4a['StopConditions']}
STD_STOPS = [s for s in pbt4a['StopConditions'] if not s['Id'].startswith('PBT4A-')]
assert len(STD_STOPS) == 17, ('las paradas estandar del contrato de PB-T4a', len(STD_STOPS))
EVID0 = pbt4a['ExpectedEvidence'][0]


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
# la primera ExpectedEvidence: la del contrato de PB-T4a (que es la de PB-T3c), byte a byte, en la lista cerrada de literales del v2
assert EVID0 in NC2_EVIDENCES, ('la primera evidencia del contrato de PB-T4a no esta en F4B_EXACT_EVIDENCES del v2', EVID0[:80])
# C11 de PB-T4b: PBT4A-C11 del contrato de PB-T4a con PB-T4a -> PB-T4b (la tarea de nc sigue siendo PBT4). El texto final lo fija el paquete
C11_SUBS = [('PB-T4a', 'PB-T4b')]
C11 = T4A_STOP['PBT4A-C11']
for _old, _new in C11_SUBS:
    assert _old in C11, ('PBT4B-C11: PB-T4a no esta en PBT4A-C11', _old)
    C11 = C11.replace(_old, _new)
for _need in ('PBT4-NC2-EXACT-WHITELIST-F4B-01', 'with --task PBT4 in the F4B-E21 mode', 'SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B',
              'with --task PBT4, recorded as nc1 = SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE', NC2_V2 + ', SHA-256 ' + NC2_V2_SHA,
              NC1_SUBST + ', SHA-256 ' + NC1_SUBST_SHA):
    assert C11.count(_need) == 1, ('PBT4B-C11', _need)
assert C11.endswith('no one-off acceptance extends to PB-T4b') and not re.search(r'PBT3C|PB-T3c|PB-T4a|PBT4A', C11), 'PBT4B-C11: restos de PB-T4a'


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


# ---------------------------------------------------------------- la lista de 470 (V42-03; D-PBT4-11), la historia: su sha256 y su filtrado, byte a byte
LIST_470 = D4 + LIST_470_NAME
_l470 = show_bytes(LIST_470)
assert hashlib.sha256(_l470).hexdigest() == LIST_470_SHA, ('la lista de 470 no es la que tomo el Owner en su segunda vuelta (sha256)', LIST_470)
_ab = show_bytes(LISTS_ALL)
assert hashlib.sha256(_ab).hexdigest() == LISTS_ALL_SHA, ('la lista completa de sustituciones no es la de la v4.2 (sha256)', LISTS_ALL)
_al = _ab.decode('utf-8').splitlines()
B_LISTS = ('PB-T4b C-0', 'PB-T4b C-5', 'PB-T4b UI')
_filt = _al[0] + '\n' + '\n'.join(l for l in _al[1:] if l.split('\t')[0] in B_LISTS) + '\n'
assert _filt.encode('utf-8') == _l470, 'el filtrado de pbt4-substitution-lists.tsv (PB-T4b C-0, C-5 y UI, en su orden, con la cabecera) no da la lista de 470'


def list_rows(raw, what):
    t = raw.decode('utf-8')
    ll = t.split('\n')
    assert '\r' not in t and ll[-1] == '' and ll[0] == 'lista\tarchivo\tmetodo\tlineas\tcausa', ('la forma de la lista', what)
    rows = [l.split('\t') for l in ll[1:-1]]
    assert all(len(r) == 5 for r in rows), ('la forma de las filas de la lista', what)
    assert len({(r[1], r[2]) for r in rows}) == len(rows), ('una prueba repetida en la lista', what)
    return rows


ROWS_470 = list_rows(_l470, LIST_470)
assert len(ROWS_470) == 470
# ---------------------------------------------------------------- la lista que se emite (LIST_NAME, LIST_SHA): de la revision o, en seco, de junto a
# --package-file; si no es la de 470, es la de 470 en su orden mas exactamente las filas de LIST_ADDED_EXPECTED (owner-pbt4b-breaks-decision.md)
LIST = D4 + LIST_NAME
if in_head(LIST) and hashlib.sha256(show_bytes(LIST)).hexdigest() == LIST_SHA:
    _lb, LIST_SRC = show_bytes(LIST), LIST + ' en ' + REV_SHA[:8]
else:
    _lp = os.path.join(os.path.dirname(PKG_FILE), LIST_NAME) if PKG_FILE else None
    assert _lp and os.path.isfile(_lp), ('la lista de LIST_SHA no esta en la revision ni junto a --package-file', LIST)
    _lb, LIST_SRC = open(_lp, 'rb').read(), LIST_NAME + ' junto a --package-file'
    soft(False, 'emision: la lista de PB-T4b de LIST_SHA aun no esta en la revision (la custodia X de PB-T4b): %s' % LIST)
assert hashlib.sha256(_lb).hexdigest() == LIST_SHA, ('la lista de PB-T4b no es la que fija la version del paquete (sha256)', LIST_SRC)
ROWS = list_rows(_lb, LIST_SRC)
_cnt = {k: sum(1 for r in ROWS if r[0] in (k,)) for k in sorted({r[0] for r in ROWS})}
assert _cnt == LIST_COUNTS, ('los recuentos de la lista', LIST_COUNTS, _cnt)
_k470 = [(r[1], r[2]) for r in ROWS_470]
ADDED = [r for r in ROWS if (r[1], r[2]) not in set(_k470)]
if LIST_SHA == LIST_470_SHA:
    assert ROWS == ROWS_470 and not ADDED
else:
    assert [k for k in ((r[1], r[2]) for r in ROWS) if k in set(_k470)] == _k470, 'la lista nueva no conserva las 470 en su orden'
    assert [r for r in ROWS if (r[1], r[2]) in set(_k470)] == ROWS_470, 'la lista nueva cambia una fila de las 470'
    assert len(ADDED) == len(LIST_ADDED_EXPECTED), ('la lista nueva no anade exactamente las de la decision del Owner', len(ADDED), len(LIST_ADDED_EXPECTED))
LIST_TOTAL = len(ROWS)
T4A_MARK = '(PB-T4a) '
T4A_ROWS = [r for r in ROWS if r[1].startswith(T4A_MARK)]
assert [(r[0], r[1][len(T4A_MARK):], r[2]) for r in T4A_ROWS] == [('PB-T4b C-5', TR + 'PushBackFullReadGuardTests.cs', 'CT_PBT4A_06 (nueva en PB-T4a)')], \
    ('la fila de PB-T4a de la lista', T4A_ROWS)


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


_files = sorted({r[1] for r in ROWS if r not in T4A_ROWS})
for _p in _files:
    assert in_head(_p), ('un archivo de la lista no esta en la revision', _p)
FILE_TEXT = {p: b.decode('utf-8-sig').replace('\r\n', '\n').replace('\r', '\n') for p, b in blobs(_files).items()}
FILE_TESTS = {p: tests_of(t) for p, t in FILE_TEXT.items()}
LISTED = {}  # (archivo, metodo) -> la prueba
for r in ROWS:
    if r in T4A_ROWS:
        continue
    _hits = [t for t in FILE_TESTS[r[1]] if t['method'] == r[2]]
    assert len(_hits) == 1, ('una prueba de la lista no esta una sola vez en su archivo', r[1], r[2], len(_hits))
    _t = _hits[0]
    _a, _b = (int(x) for x in r[3].split('-'))
    assert not _t['skip'] and _t['n'] != 0, ('una prueba de la lista ya lleva Skip, o es una Theory sin datos', r[1], r[2])
    assert _a <= _t['line'] <= _b, ('la linea de una prueba de la lista no cae en su rango', r[1], r[2], r[3], _t['line'])
    LISTED[(r[1], r[2])] = _t
# las anadidas: cada una es una de LIST_ADDED_EXPECTED (su archivo y la linea de su declaracion), y la de PB-T4d sale de su lista (C-7)
if ADDED:
    _got = sorted((r[1], LISTED[(r[1], r[2])]['line']) for r in ADDED)
    assert _got == sorted(LIST_ADDED_EXPECTED), ('las anadidas no son las de la decision del Owner (archivo y linea)', _got)
    _t4d = [l.split('\t') for l in show(D4 + 'pbt4d-retire-list.tsv').split('\n')[1:] if l]
    _c7 = {(r[1], r[2]) for r in _t4d if r[0] == 'PB-T4d C-7'}
    assert sum(1 for r in ADDED if (r[1], r[2]) in _c7) == LIST_ADDED_FROM_T4D, ('las anadidas que salen de la lista de PB-T4d', LIST_ADDED_FROM_T4D)
if PT4A_CLOSE is None:
    pending('la fila CT-PBT4A-06 de la lista ("CT_PBT4A_06 (nueva en PB-T4a)" en PushBackFullReadGuardTests.cs, un archivo de PB-T4a): su nombre real se lee '
            'en el cierre de PB-T4a, con su Skip y su sustituta CT-PBT4B-13')
else:
    _gt = tests_of(show(TR + 'PushBackFullReadGuardTests.cs').replace('\r\n', '\n'))
    _h = [t for t in _gt if t['method'].startswith('CT_PBT4A_06')]
    assert len(_h) == 1 and not _h[0]['skip'], ('CT-PBT4A-06 en el cierre de PB-T4a', [t['method'] for t in _h])
    assert (_h[0]['method'], _h[0]['line']) == (CT_PBT4A_06_NAME, CT_PBT4A_06_LINE), ('CT-PBT4A-06: el nombre y la linea de la sesion', _h[0]['method'],
                                                                                     _h[0]['line'])
    LISTED[(TR + 'PushBackFullReadGuardTests.cs', _h[0]['method'])] = _h[0]
# las 9 clases (C-0): enteras, y frente al mapa de la copia
NINE = sorted({r[1] for r in ROWS if r[0] == 'PB-T4b C-0'})
assert len(NINE) == 9, NINE
for _p in NINE:
    _all_t = {t['method'] for t in FILE_TESTS[_p] if not t['skip']}
    _lst = {r[2] for r in ROWS if r[0] == 'PB-T4b C-0' and r[1] == _p}
    assert _all_t == _lst and all(not t['skip'] for t in FILE_TESTS[_p]), ('C-0: una de las 9 clases no esta entera en la lista', _p, sorted(_all_t ^ _lst)[:5])
# el mapa de la copia (D-S1): el de la revision (COPY_MAP_NAME) si es el que fija COPY_MAP_SHA; en seco, si no, el de la carpeta de --package-file
COPY_MAP = D4 + COPY_MAP_NAME
_cmb = show_bytes(COPY_MAP) if in_head(COPY_MAP) else b''
if hashlib.sha256(_cmb).hexdigest() == COPY_MAP_SHA:
    CM_SRC = COPY_MAP + ' en ' + REV_SHA[:8]
else:
    _cmp = os.path.join(os.path.dirname(PKG_FILE), COPY_MAP_NAME) if PKG_FILE else None
    assert _cmp and os.path.isfile(_cmp), ('el mapa de la copia de la revision no es el de COPY_MAP_SHA, y no hay otro junto a --package-file', COPY_MAP)
    _cmb, CM_SRC = open(_cmp, 'rb').read(), COPY_MAP_NAME + ' junto a --package-file'
    soft(False, 'emision: el mapa de la copia de COPY_MAP_SHA aun no esta en la revision (lo custodia X de PB-T4b): %s' % COPY_MAP)
assert hashlib.sha256(_cmb).hexdigest() == COPY_MAP_SHA, ('el mapa de la copia no es el que fija la version del paquete (sha256)', CM_SRC)
_cm = _cmb.decode('utf-8').replace('\r\n', '\n').split('\n')
assert _cm[0] == 'clase\tmetodo\tlineas\tmapa\tdestino' and _cm[-1] == '', 'la forma del mapa de la copia'
CM = [l.split('\t') for l in _cm[1:-1]]
assert len(CM) == 428 and all(len(r) == 5 for r in CM)
assert {(r[0], r[1], r[2]) for r in CM} == {(os.path.basename(r[1])[:-3], r[2], r[3]) for r in ROWS if r[0] == 'PB-T4b C-0'}, \
    'el mapa de la copia no es, prueba a prueba, la parte C-0 de la lista'
_dest = {}
for r in CM:
    _dest[r[4]] = _dest.get(r[4], 0) + 1
_hand = {k: v for k, v in _dest.items() if k != 'mecanica'}
assert sum(COPY_COUNTS.values()) == LIST_COUNTS['PB-T4b C-0'], ('COPY_COUNTS no reparte las 428 de C-0', COPY_COUNTS)
assert _dest.get('mecanica') == COPY_COUNTS['mecanica'] and sum(_hand.values()) == COPY_COUNTS['a mano'], \
    ('el reparto del mapa de la copia no es el de COPY_COUNTS (D-S1)', COPY_COUNTS, _dest)
HAND_TALLY = '; '.join('%s %d' % kv for kv in sorted(_hand.items(), key=lambda kv: (-kv[1], kv[0])))
assert all(re.fullmatch(r'M-0[0-8](?:,M-0[0-8])*|-', r[3]) for r in CM), 'una columna mapa fuera de M-00..M-08'
# el sha256 de cada original en la base (CT-PBT4B-20 lo lleva en BaseSha) y su forma de fin de linea
ORIG = {p: hashlib.sha256(show_bytes(p)).hexdigest() for p in NINE}
ORIG_CRLF = {p: b'\r\n' in show_bytes(p) for p in NINE}
SKIP_FILES = sorted({r[1] for r in ROWS if r not in T4A_ROWS})  # los archivos que solo cambian por sus Skip (mas el de PB-T4a, al cerrarlo)
UI_FILES = {p: sum(1 for r in ROWS if r[0] == 'PB-T4b UI' and r[1] == p) for p in sorted({r[1] for r in ROWS if r[0] == 'PB-T4b UI'})}
C5_FILES = {p: sum(1 for r in ROWS if r[0] == 'PB-T4b C-5' and r[1] == p) for p in sorted({r[1] for r in ROWS if r[0] == 'PB-T4b C-5'})}


# ---------------------------------------------------------------- el alcance que nombra el paquete (section 6.2): lo que existe, lo nuevo y Pilot/
APP, UIS, PLG = 'src/RackCad.Application/', 'src/RackCad.UI/', 'src/RackCad.Plugin/'
DRAFT = APP + 'Systems/PushBack/Draft/'
EXISTING_SRC = [APP + 'Workspace/WorkspaceHostCore.cs', APP + 'Workspace/WorkspaceEditIntent.cs', APP + 'Workspace/WorkspacePilotSupport.cs',
                UIS + 'Workspace/WorkspacePilotFieldViewModel.cs', UIS + 'Workspace/WorkspacePanelView.xaml', UIS + 'Workspace/WorkspacePanelView.xaml.cs',
                PLG + 'Workspace/WorkspaceController.cs', UIS + 'Controls/NumericFieldValidation.cs']
NEW_SRC = [APP + 'Formatting/NumericTextRule.cs', DRAFT + 'PushBackAnnotationScaleRule.cs', DRAFT + 'PushBackFullFieldAdapter.cs',
           DRAFT + 'PushBackKeepMinePresentation.cs']
# las clases nuevas de tests/ que el paquete nombra hoy: el kit (D-PBT4-11.3), las 9 copias (D-PBT4-11.2), CT-19 y CT-20 (section 5.2); las de CT-01 a
# CT-18 las nombra la v3 (section 5.2 de la v4.3 remite a ella): PENDIENTE
NEW_TESTS = [TR + 'Workspace/WorkspaceFullReadKit.cs'] + [TR + 'Workspace/%sFull.cs' % os.path.basename(p)[:-len('Tests.cs')] for p in NINE] + \
            [TR + 'PushBackPanelDraftGuardTests.cs', TR + 'WorkspaceFullCopyGuardTests.cs']
for _p in EXISTING_SRC:
    assert in_head(_p), ('un archivo del alcance de PB-T4b no esta en la revision', _p)
for _p in NEW_SRC + NEW_TESTS:
    assert not in_head(_p), ('un archivo nuevo de PB-T4b ya existe en la revision', _p)
assert all(p.endswith('Tests.cs') for p in NINE), NINE
_scope = EXISTING_SRC + NEW_SRC + NEW_TESTS + SKIP_FILES
assert not any('/Pilot/' in p for p in _scope), ('el alcance de PB-T4b toca Pilot/ (section 6.2: ningun archivo de Pilot/)', [p for p in _scope if '/Pilot/' in p])
assert len(set(_scope)) == len(_scope), 'una ruta repetida en el alcance provisional'
# la pureza de Draft/ (CT-PBT3A-14 y CT-PBT4B-19) y la regla de section 5.0 sobre los '?' (CA-T4a I-01): se comprueban sobre el texto literal del GREEN,
# que el paquete aun no lleva para PB-T4b: PENDIENTE
ALLOWED = ALLOWED_PROV = EXISTING_SRC + NEW_SRC + NEW_TESTS + SKIP_FILES  # cover() de [PKC-C] lee ALLOWED
SRCF_PROV = cover('src/')
TESTF_PROV = cover('tests/')
ROOT_PROV = [f for f in pbt4a['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
for a in ALLOWED_PROV:
    assert not any(a == f or (f.endswith('/') and a.startswith(f)) for f in SRCF_PROV + TESTF_PROV), a
# el precedente de tests/ (PB-T3c y PB-T4a: una lista explicita, no cover('tests/')): las de PB-T4a menos las rutas de tests/ del alcance de PB-T4b; las
# tres clases de PB-T4a pasan a prohibidas tras su cierre, salvo PushBackFullReadGuardTests (el Skip de CT-PBT4A-06)
_t4a_tests = [f for f in pbt4a['ForbiddenWriteScope'] if f.startswith('tests/')]
TESTS_OUT_OF_T4A = [a for a in ALLOWED_PROV if a.startswith('tests/') and any(a == f or (f.endswith('/') and a.startswith(f)) for f in _t4a_tests)]
# un prefijo prohibido de PB-T4a que cubre una ruta de PB-T4b se sustituye por su cover() (la lista explicita menos esas rutas, como en PB-T3b)
TESTF_PREC, SPLIT_PREFIXES = [], []
for f in _t4a_tests:
    if f in ALLOWED_PROV:
        continue
    if f.endswith('/') and any(a.startswith(f) for a in ALLOWED_PROV):
        SPLIT_PREFIXES.append((f, len(cover(f))))
        TESTF_PREC += cover(f)
    else:
        TESTF_PREC.append(f)
assert not any(f.endswith('/') and any(a.startswith(f) for a in ALLOWED_PROV) for f in TESTF_PREC), 'un prefijo prohibido cubre una ruta de PB-T4b'
T4A_OWN_TESTS = [a for a in pbt4a['AllowedWriteScope'] if a.startswith('tests/')]
for p in tracked('src/') + tracked('tests/'):
    assert p in ALLOWED_PROV or any(p == f or (f.endswith('/') and p.startswith(f)) for f in SRCF_PROV + TESTF_PROV), ('sin cubrir', p)


# ---------------------------------------------------------------- los filtros heredados: los 38 del contrato de PB-T4a, con lo que dejan los Skip de la
# lista (section 6.2: "los heredados, con los MinSelected que dejen los Skip, medidos en la emision"), con el criterio de count_tests_git.py
COUNTS, _CLASSES = {}, {}
for _proj in ('UI', 'Core'):
    _nsp, _totals, _texts = count_static(_proj)
    _CLASSES[_proj] = _totals
    for r in pbt4a['RequiredTests']:
        if (r['Project'] == UIP) == (_proj == 'UI'):
            _f = r['Filter'].split('~', 1)[1]
            _sel = {k: v for k, v in _totals.items() if k.startswith(_f)}
            COUNTS[r['Filter']] = (len(_sel), sum(v[0] for v in _sel.values()), sum(v[1] for v in _sel.values()), sum(v[2] for v in _sel.values()))
# una Theory de MemberData o ClassData (n negativo) no cuenta en los ejecutables estaticos de count_tests_git.py: resta 0 y se cuenta aparte
_LST = [(UIP if p.startswith(UT) else CP, t['cls'] + '.' + t['method'], max(t['n'], 0)) for (p, _m), t in sorted(LISTED.items())]
LIST_MEMBERDATA = sorted('%s.%s' % (os.path.basename(p)[:-3], m) for (p, m), t in LISTED.items() if t['n'] < 0)
T4A_NEW = {'FullyQualifiedName~RackCad.Tests.PushBackFullReadPathTests': 0, 'FullyQualifiedName~RackCad.Tests.PushBackFullReadGuardTests': 1}
REQ_INH, REQ_REPORT, REQ_ZERO = [], [], []
for r in pbt4a['RequiredTests']:
    _f = r['Filter'].split('~', 1)[1]
    classes, run, skip, member = COUNTS[r['Filter']]
    sk = sum(n for proj, fqn, n in _LST if proj == r['Project'] and fqn.startswith(_f))
    if r['Filter'] in T4A_NEW and PT4A_CLOSE and REV_SHA != PT4A_EMIT:
        # tras el cierre de PB-T4a sus clases existen: se cuentan como las demas (la fila de CT-PBT4A-06, con su nombre real, ya esta en LISTED)
        assert classes > 0 and run >= r['MinSelected'], ('una clase de PB-T4a en su cierre', r['Filter'], classes, run)
        kind = 'de PB-T4a (en su cierre)'
    elif r['Filter'] in T4A_NEW:
        assert classes == 0, ('una clase nueva de PB-T4a ya esta en la revision de la fase 1', r['Filter'])
        sk = T4A_NEW[r['Filter']]  # CT-PBT4A-06 (la fila de PB-T4a de la lista)
        kind = 'de PB-T4a: existe tras su cierre (PENDIENTE); %d - %d = %d' % (r['MinSelected'], sk, r['MinSelected'] - sk)
    else:
        assert classes > 0, ('un filtro heredado no selecciona ninguna clase en la revision', r['Filter'])
        kind = 'heredado'
    new_min = r['MinSelected'] - sk
    if new_min <= 0:
        REQ_ZERO.append(r['Filter'])
    REQ_INH.append((r['Project'], r['Filter'], new_min, r['ExpectRed']))
    REQ_REPORT.append('%-4s %-62s MinSelected %4d -> %-4d (Skip de la lista %3d) medido: clases=%d ejecutables=%d skip=%d memberdata=%d  %s'
                      % ('UI' if r['Project'] == UIP else 'Core', _f.split('.', 2)[-1], r['MinSelected'], new_min, sk, classes, run, skip, member, kind))
LIST_RESULTS = sum(n for _, _, n in _LST)  # los resultados estaticos que saltan los Skip de la lista (Theory: sus InlineData), sin la fila de PB-T4a
# el pendiente de las Theory de MemberData lo cierra la precondicion 6 (los totales de [14] los midio la ejecucion): se decide al leerla (b8), no aqui; si
# se llamara ya a pending(), al emitir (AS_WRITE) abortaria antes de leer la precondicion 6 de la revision (el fallo de la BASE sobre f5ff00ee)
MEMBERDATA_PENDING = (('%d pruebas de la lista son Theory de MemberData o ClassData: su numero de resultados no se cuenta por texto (count_tests_git.py) y '
                       'el total de cada rama tiene que medirlo la precondicion 6: %s' % (len(LIST_MEMBERDATA), ', '.join(LIST_MEMBERDATA)))
                      if LIST_MEMBERDATA else None)
# con la lista de 475, los MinSelected que fija la decision del Owner (WorkspacePilotUpdateGuardTests, de 86 a 85)
if LIST_SHA != LIST_470_SHA:
    for _name, _want in MIN_EXPECTED_475.items():
        _h = [m for p, f, m, e in REQ_INH if f.endswith('.' + _name)]
        assert _h == [_want], ('un MinSelected que fija la decision del Owner sobre las cinco roturas', _name, _h, _want)
if REQ_ZERO:
    pending('filtros heredados que se quedan sin pruebas ejecutables con los Skip de la lista (el paquete decide si salen o cambian): ' + ', '.join(REQ_ZERO))
# los filtros nuevos que nombra section 6.2 y ya existen en la base: NumericFieldValidationTests y NumericFieldTests (UI), en verde
# (por archivo: count_static atribuye las pruebas de NumericFieldTests a su clase anidada BorderBrushSource, que va antes de ellas)
REQ_NEW_EXISTING = []
for _c in ('NumericFieldValidationTests', 'NumericFieldTests'):
    _p = UT + _c + '.cs'
    assert in_head(_p), ('un filtro nuevo de UI de section 6.2', _p)
    _tt = tests_of(show(_p).replace('\r\n', '\n'))
    REQ_NEW_EXISTING.append(('UI', _c, sum(max(t['n'], 0) for t in _tt if not t['skip']), sum(1 for t in _tt if t['skip']),
                             sum(1 for t in _tt if t['n'] < 0 and not t['skip']),
                             sorted({k for k, v in _CLASSES['UI'].items() if _p in v[3]})))


# ---------------------------------------------------------------- el paquete (PKG_VERSION, por su sha256): la seccion 6.2 de PB-T4b y lo que le falta
if PKG_FILE:
    PKG_RAW, PKG_SRC = open(PKG_FILE, 'rb').read(), '--package-file ' + os.path.basename(PKG_FILE)
else:
    PKG_RAW, PKG_SRC = show_bytes(PKG), PKG + ' en ' + REV_SHA[:8]
assert hashlib.sha256(PKG_RAW).hexdigest() == CUSTODY_SHA256[PKG], ('el paquete leido no es la %s revisada (sha256)' % PKG_VERSION, PKG_SRC)
PKG_LINES = PKG_RAW.decode('utf-8').replace('\r\n', '\n').split('\n')
_title = [l for l in PKG_LINES[:8] if l.startswith('# I-64 F4B')]
assert len(_title) == 1 and re.search(r', (v\d+(?:\.\d+)?)\s*$', _title[0]).group(1) == PKG_VERSION, ('la version del paquete', _title)


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


SEC62 = section('### 6.2 PB-T4b')
SEC60 = section('### 6.0 ')
SEC52 = section('### 5.2 PB-T4b')


# ================================================================ FASE 2 (la v4.4): el contrato de PB-T4b desde section 6.2 y 6.2.1, con los marcadores
# <tras el cierre de PB-T4a> (PT4A_CLOSE, CI_BASE_T4A) y <medido en la precondicion 6> (el archivo de la sesion, P6: pbt4b-precondition6.json)
MARK_T4A, MARK_P6 = '<tras el cierre de PB-T4a>', '<medido en la precondicion 6>'
_b = [k for k, l in enumerate(PKG_LINES) if l.startswith('#### 6.2.1 ')]
assert len(_b) == 1, 'section 6.2.1'
_a = next(k for k in range(_b[0], len(PKG_LINES)) if PKG_LINES[k].strip() == '```text')
_z = next(k for k in range(_a + 1, len(PKG_LINES)) if PKG_LINES[k].strip() == '```')
BLOCK = PKG_LINES[_a + 1:_z]
assert all(ord(ch) < 0x80 for l in BLOCK for ch in l), 'el bloque de 6.2.1 no es ASCII'


def field(name):
    i = BLOCK.index(name + ':')
    out = []
    for l in BLOCK[i + 1:]:
        if not l.strip() or re.match(r'^[A-Za-z]+(?: \(propias\))?:$', l):
            break
        out.append(l)
    return out


assert BLOCK[0] == 'TaskId: ' + TASK, ('el TaskId de 6.2.1', BLOCK[0])
OBJ2 = field('Objective')
assert len(OBJ2) == 1, 'el Objective de 6.2.1 en una linea'
OBJECTIVE = OBJ2[0]
for _need in ('paquete del contrato de PB-T4 ' + PKG_VERSION, 'CT-PBT4B-01..%02d' % N_CT, '%d sustituciones' % LIST_TOTAL, 'Skip', 'lo retira PB-T4d'):
    assert _need in OBJECTIVE, ('el Objective de PB-T4b sin', _need)
INV = field('Invariants')
assert [i.split('. ')[0] for i in INV] == ['INV-PBT4B-%02d' % n for n in range(1, 15)], [i[:12] for i in INV]
STOP_OWN_TEXT = field('StopConditions (propias)')
_sid = [s.split(': ', 1)[0] for s in STOP_OWN_TEXT]
assert _sid == ['PBT4B-C%02d' % n for n in list(range(1, 15)) + [16, 17, 18, 20]], _sid
STOP_OWN = [{'Id': s.split(': ', 1)[0], 'Text': s.split(': ', 1)[1]} for s in STOP_OWN_TEXT]
assert dict((s['Id'], s['Text']) for s in STOP_OWN)['PBT4B-C11'] == C11, 'PBT4B-C11 del paquete no es PBT4A-C11 con PB-T4a -> PB-T4b'
assert 'IMPORTANT LIMIT' in dict((s['Id'], s['Text']) for s in STOP_OWN)['PBT4B-C17'], 'C17'
_c02 = dict((s['Id'], s['Text']) for s in STOP_OWN)['PBT4B-C02']
assert ('the %d tests of %s' % (LIST_TOTAL, LIST_NAME)) in _c02 and 'No exception outside the closed list of PB-T4b' in _c02, 'C02 sin excepcion fuera de la lista'
STOPS = STD_STOPS + STOP_OWN
assert len(STOPS) == 35, len(STOPS)
EVID = field('ExpectedEvidence')
assert [e.split('] ')[0] + ']' for e in EVID] == ['[%d]' % n for n in range(16)], [e[:4] for e in EVID]
EVID = [e.split('] ', 1)[1] for e in EVID]
assert EVID[0] == EVID0, 'la primera evidencia no es la del contrato de PB-T4a (la de PB-T3c)'
assert LIST_SHA in INV[6] and LIST_NAME in INV[6], 'INV-PBT4B-07 sin la lista de PB-T4b y su sha256'
# [14]: la CI prevista, con la aritmetica de los Skipped (cada Theory saltada da un resultado): Core 7 + 448 y UI 21 + 27
_e14 = EVID[14]
_core_skip = CI_BASE_T4A[3] + LIST_COUNTS['PB-T4b C-0'] + LIST_COUNTS['PB-T4b C-5']
_ui_skip = CI_BASE_T4A[5] + LIST_COUNTS['PB-T4b UI']
_sp = lambda v: format(v, ',').replace(',', ' ')
_m = re.search(r'GREEN: Core (\d+ \d{3}) resultados, con (\d+ \d{3}) Passed, 0 Failed y (\d+) Skipped', _e14)
_mu = re.search(r'UI (\d \d{3}), con (\d \d{3}) Passed, 0 Failed y (\d+) Skipped\. El RED', _e14)
assert _m and _mu, ('[14]: los totales del GREEN', _e14[:200])
_gc, _gp, _gs = (int(x.replace(' ', '')) for x in _m.groups())
_uc, _up, _us = (int(x.replace(' ', '')) for x in _mu.groups())
assert _gc == _gp + _gs and _gs == _core_skip and _uc == _up + _us and _us == _ui_skip, ('[14]: Passed + Skipped y los Skip de la lista', _gc, _gp, _gs, _uc, _up, _us)
assert ('Core %s resultados, con %s Passed y %d Skipped' % (_sp(CI_BASE_T4A[2]), _sp(CI_BASE_T4A[2] - CI_BASE_T4A[3]), CI_BASE_T4A[3])) in _e14, '[14]: la base'
GREEN_T = (_gc, _gp, _gs, _uc, _up, _us)
# el alcance de section 6.2 (53): las rutas explicitas de sus vinetas (con la carpeta en curso para los nombres sueltos) y los 19 archivos de los Skip
_al = [k for k, l in enumerate(SEC62) if l.startswith('- **`AllowedWriteScope`** (')]
assert len(_al) == 1
N_ALLOWED = int(re.search(r'\((\d+)\)', SEC62[_al[0]]).group(1))
ALLOWED = []
for l in SEC62[_al[0] + 1:]:
    if not l.startswith('  - '):
        break
    if l.startswith('  - **solo para los `Skip`'):
        continue
    if l.startswith('  - **Ningun') or l.startswith('  - **Ning' + chr(250) + 'n'):
        continue
    folder = None
    for tok in re.findall(r'`([^`]+)`', l):
        if tok.endswith('/') and tok.startswith(('src/', 'tests/')):
            folder = tok
        elif tok.startswith(('src/', 'tests/')):
            ALLOWED.append(tok)
            folder = None if '/' in tok else folder
        elif tok.endswith('.cs') and folder:
            ALLOWED.append(folder + tok)
SKIP_ALL = list(dict.fromkeys([r[1] for r in ROWS if not r[1].startswith(T4A_MARK)] + [TR + 'PushBackFullReadGuardTests.cs']))
ALLOWED += SKIP_ALL
assert len(ALLOWED) == len(set(ALLOWED)) == N_ALLOWED, ('el AllowedWriteScope de section 6.2', len(ALLOWED), N_ALLOWED)
assert not any('/Pilot/' in a for a in ALLOWED), 'ninguna ruta de Pilot/'
assert UNIT_CHANGE_LINE in SEC62, 'UNIT_CHANGE de PB-T4b'
UNIT_CHANGE_P = [APP + 'Workspace/WorkspaceHostCore.cs', APP + 'Workspace/WorkspaceEditIntent.cs', UIS + 'Workspace/WorkspacePilotFieldViewModel.cs',
                 UIS + 'Workspace/WorkspacePanelView.xaml', UIS + 'Controls/NumericFieldValidation.cs'] + UNIT_CHANGE_EXTRA
assert ALLOWED[:len(UNIT_CHANGE_P)] == UNIT_CHANGE_P, ALLOWED[:len(UNIT_CHANGE_P)]
for a in ALLOWED:
    _new = a not in UNIT_CHANGE_P and a not in SKIP_ALL
    assert in_head(a) != _new or a == TR + 'PushBackFullReadGuardTests.cs', ('una ruta del alcance existe o falta en la revision', a)
# el ForbiddenWriteScope: [PKC-C] en src/ y el precedente de tests/ de PB-T4a, con las clases de PB-T4a salvo PushBackFullReadGuardTests (tras su cierre)
SRCF = cover('src/')
TESTF, SPLIT2 = [], []
for f in [f for f in pbt4a['ForbiddenWriteScope'] if f.startswith('tests/')] + [a for a in pbt4a['AllowedWriteScope'] if a.startswith('tests/')]:
    if f in ALLOWED:
        continue
    if f.endswith('/') and any(a.startswith(f) for a in ALLOWED):
        SPLIT2.append(f)
        TESTF += cover(f)
    else:
        TESTF.append(f)
ROOT = [f for f in pbt4a['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == len(set(FORBIDDEN)) and not set(FORBIDDEN) & set(ALLOWED)
# el numero que toma el paquete (su vineta del ForbiddenWriteScope, "**N entradas y K KB**"): si no es el medido, PENDIENTE (la v4.4 toma 452, el de la
# fase 1 con el alcance provisional; con su alcance son 405, que la sesion pasa al autor para la v4.5)
_fw = [l for l in SEC62 if l.startswith('- **`ForbiddenWriteScope`:**')]
_fn = re.findall(r'\*\*(\d+) entradas y [\d,]+ KB\*\*', _fw[0]) if len(_fw) == 1 else []
FORBIDDEN_PKG = int(_fn[0]) if len(_fn) == 1 else None
if FORBIDDEN_PKG is not None and FORBIDDEN_PKG != len(FORBIDDEN):  # sin numero en el paquete: la regla del generador (la v4.5)
    pending('el ForbiddenWriteScope sale con %d entradas con el alcance de la %s, y el paquete toma %s: la version siguiente fija %d (la v4.5)'
            % (len(FORBIDDEN), PKG_VERSION, FORBIDDEN_PKG, len(FORBIDDEN)))
# las piezas por sha256: el kit, el guion de la copia y los equivalentes (junto a --package-file en seco; en la revision al emitir). El sha256 de cada
# una es el codigo que sigue a su nombre (con 'sha256' delante o sin el, como `nombre` (`sha256`) en la v4.9); todas sus menciones dan el mismo
_s62all = '\n'.join(PKG_LINES)
PIECES = {}
for _n in PIECE_NAMES:
    _h = re.findall(r'`%s`\*{0,2}[^`]{0,40}?(?:sha256 \*{0,2})?`([0-9a-f]{64})`' % re.escape(_n), _s62all)
    assert _h and len(set(_h)) == 1, ('el sha256 de una pieza en el paquete', _n, _h)
    PIECES[_n] = _h[0]
    _rev, _loc = D4 + _n, (os.path.join(os.path.dirname(PKG_FILE), _n) if PKG_FILE else None)
    if in_head(_rev):
        assert hashlib.sha256(show_bytes(_rev)).hexdigest() == _h[0], ('una pieza de la revision no es la del paquete', _n)
    else:
        assert _loc and os.path.isfile(_loc) and hashlib.sha256(open(_loc, 'rb').read()).hexdigest() == _h[0], ('una pieza no esta, o no es la del paquete', _n)
        soft(False, 'emision: %s (sha256 %s) aun no esta en la revision (la custodia X de PB-T4b)' % (_n, _h[0][:8]))
# los RequiredTests de 6.2.1 (N_REQ_ROWS filas; 88 en la v4.8 y la v4.9): los 38 heredados con los MinSelected medidos aqui, las 8 nuevas y las 42 de las copias
# (precondicion 6)
_t = [k for k, l in enumerate(PKG_LINES) if l.startswith('| # | Project | Filter | MinSelected | ExpectRed |') and k > _z]
assert len(_t) == 1
REQ_PKG = []
for l in PKG_LINES[_t[0] + 2:]:
    m = re.match(r'^\| (\d+) \| `([^`]+)` \| `([^`]+)` \| (\*\*\d+\*\*|\d+|`<medido en la precondicion 6>`) \| (true|false) \|$', l)
    if not m:
        break
    REQ_PKG.append((int(m.group(1)), m.group(2), m.group(3), m.group(4).strip('*`'), m.group(5) == 'true'))
assert [r[0] for r in REQ_PKG] == list(range(1, N_REQ_ROWS + 1)), ('la tabla de RequiredTests de 6.2.1', len(REQ_PKG))
for (k, proj, filt, m, er), (p2, f2, m2, e2) in zip(REQ_PKG[:38], REQ_INH):
    assert (proj, filt, int(m), er) == (p2, f2, m2, False), ('la fila %d de RequiredTests frente a lo medido' % k, filt, m, m2)
P6 = None
if P6_FILE:
    P6 = json.loads(open(P6_FILE, 'rb').read().decode('utf-8'))
elif in_head(D4 + 'pbt4b-precondition6.json'):
    P6 = json.loads(show(D4 + 'pbt4b-precondition6.json'))
P6_CLASSES = [filt.rsplit('.', 1)[1] for k, proj, filt, m, er in REQ_PKG if m == MARK_P6]
if P6 is not None:
    # la forma del archivo de la sesion: su revision es el cierre de PB-T4a, un MinSelected por cada clase de las copias del paquete (ni una de mas ni de
    # menos), y CopySha con las copias del alcance; los campos de mas (Cost: las TRX del coste por sha256) no se leen
    assert P6.get('Schema') == 'rackcad-pbt4b-precondition6/v1' and P6.get('Revision') == PT4A_CLOSE, ('la precondicion 6: su forma o su revision',
                                                                                                         P6.get('Schema'), P6.get('Revision'))
    assert sorted(P6['MinSelected']) == sorted(P6_CLASSES) and all(isinstance(v, int) and v >= 0 for v in P6['MinSelected'].values()), \
        ('la precondicion 6: los MinSelected no son los de las clases de las copias del paquete', sorted(set(P6['MinSelected']) ^ set(P6_CLASSES))[:4])
    assert re.fullmatch(r'[0-9a-f]{64}', P6['PartitionSha256']) and isinstance(P6['R_C'], int) and isinstance(P6['R_UI'], int)
    _copies = [a for a in ALLOWED if a.endswith('Full.cs')]
    assert sorted(P6['CopySha']) == sorted(_copies) and all(re.fullmatch(r'[0-9a-f]{64}', v) for v in P6['CopySha'].values()), \
        ('la precondicion 6: CopySha no cubre las copias del alcance', sorted(set(P6['CopySha']) ^ set(_copies))[:4])
    P6_EXTRA = sorted(set(P6) - {'Schema', 'Revision', 'PartitionSha256', 'CopySha', 'R_C', 'R_UI', 'MinSelected'})
    # con la precondicion 6, los resultados de las Theory de MemberData los midio la ejecucion (los totales de [14]): su pendiente no se abre (abajo)
REQ = []
for k, proj, filt, m, er in REQ_PKG:
    if m == MARK_P6:
        _cls = filt.rsplit('.', 1)[1]
        if P6 is None:
            continue
        n = P6['MinSelected'][_cls]
        if n == 0:
            continue  # una clase sin pruebas sale de la tabla (6.2.2)
        REQ.append((proj, filt, n, er))
    else:
        REQ.append((proj, filt, int(m), er))
if P6 is None:
    if MEMBERDATA_PENDING:
        pending(MEMBERDATA_PENDING)
    pending('la precondicion 6 de la sesion (pbt4b-precondition6.json, junto a --package-file en seco con --p6-file, o en la revision): Schema '
            '"rackcad-pbt4b-precondition6/v1", Revision (el cierre de PB-T4a), PartitionSha256, CopySha, R_C, R_UI y MinSelected por clase de las %d '
            'filas de las copias (0: la fila sale); sin ella, el contrato lleva %d filas de RequiredTests' % (len(P6_CLASSES), len(REQ)))
for proj, filt, m, er in REQ:
    assert in_head(proj), proj
# los marcadores: los sustituye el generador al emitir
def fill_marks(text):
    if MARK_T4A in text and PT4A_CLOSE:
        text = text.replace(MARK_T4A, CI_BASE_T4A[0], 1).replace(MARK_T4A, GREEN_T4A[:8], 1)
    if MARK_P6 in text and P6:
        text = text.replace(MARK_P6, 'R_C = %d y R_UI = %d' % (P6['R_C'], P6['R_UI']))
    return text


EVID = [fill_marks(e) for e in EVID]
_marks = sum(e.count(MARK_T4A) + e.count(MARK_P6) for e in EVID + INV + [s['Text'] for s in STOP_OWN] + [OBJECTIVE])
if _marks:
    pending('quedan %d marcadores en los textos literales del contrato (<tras el cierre de PB-T4a> y <medido en la precondicion 6>)' % _marks)


# ---------------------------------------------------------------- el contrato de PB-T4b (fase 2): las autoridades de PB-T4a, con las de PB-T4b
BOUNDED_CHECK = D4 + 'independent-review-v1.md'  # CA-T4b M-08: sus secciones BOUNDED_SECTIONS, sin copiarlas a otro archivo
P6_REL = D4 + 'pbt4b-precondition6.json'
_auth = []
for a in pbt4a['Authorities']:
    if a['Class'] == 'UNIT_CHANGE':
        continue
    a = dict(a)
    if a['Path'] == PKG:
        a['Section'] = ('archivo completo (' + PKG_VERSION + ', sha256 ' + CUSTODY_SHA256[PKG][:8] + '): D-PBT4-04 a D-PBT4-11 (el texto literal del GREEN, '
                        'green_pbt4b.py); section 4.2, 5.2 y 6.0 (identidad, paradas, RR y la tabla de RR-3) y 6.2 (el contrato de PB-T4b, sus literales de '
                        '6.2.1 y lo que necesita el generador, 6.2.2)')
    _auth.append(a)
_have = {a['Path'] for a in _auth}
for p, s in ([(OWNER_DEC3, 'archivo completo (' + EMIT_ENTRY + '): la tercera decision del Owner sobre PB-T4, literal: las cinco roturas, "Anadirlas a la '
                          'lista" (475; PB-T4d, 138; WorkspacePilotUpdateGuardTests en 85)'),
             (SESSION_DEC, 'archivo completo (' + EMIT_ENTRY + '): las decisiones de diseno de la sesion, D-S1 (copias que pasan a equivalentes) a D-S6'),
             # CA-T4b, seccion 21, M-02: solo cuatro tandas tocan la lista (la 2.a, 470; la 3.a, +5; la 5.a, +2; la 6.a, +1)
             (LIST, 'la lista de PB-T4b (%d), sha256 %s: la de 470 (la segunda tanda del Owner) mas las %d filas de la tercera, la quinta y la sexta'
              % (LIST_TOTAL, LIST_SHA[:8], len(ADDED))),
             (COPY_MAP, 'el mapa de la copia, sha256 %s: %d mecanicas y %d a mano' % (COPY_MAP_SHA[:8], COPY_COUNTS['mecanica'], COPY_COUNTS['a mano'])),
             (D4 + KIT_NAME, 'el kit, literal, sha256 %s (D-PBT4-11.4)' % PIECES[KIT_NAME][:8])] +
            [(D4 + _n, 'el guion de la copia o de los equivalentes, sha256 %s' % PIECES[_n][:8]) for _n in PIECE_NAMES if _n != KIT_NAME] +
            [(OWNER_DEC4, 'archivo completo (' + EMIT_ENTRY + '): la quinta tanda del Owner sobre PB-T4, literal: CT-PBT4A-03 y CT-PBT4A-05 (la lista pasa '
                          'a 477)'),
             (OWNER_DEC5, 'archivo completo (' + EMIT_ENTRY + '): la sexta tanda del Owner sobre PB-T4, literal: WorkspaceHostHardeningGuardTests (la '
                          'lista pasa a 478)'),
             (OWNER_DEC6, 'archivo completo (' + EMIT_ENTRY + '): la decision del Owner sobre el coste de PB-T4b, literal (la pregunta y la respuesta)'),
             (COST_DEC, 'archivo completo (F4B-E51): la decision del Owner sobre el coste de la lectura (la optimizacion de la v4.5, D-S4)'),
             (P6_REL, 'la precondicion 6 de la sesion: la particion de las copias, CopySha, R_C, R_UI y los MinSelected de las clases de las copias'),
             # las secciones que el paquete nombra entre las autoridades de PB-T4b (6.2.2): la 17 y la 18, la 19 y la comprobacion acotada (CA-T4b M-08)
             (BOUNDED_CHECK, 'secciones 17 y 18 (las roturas de PB-T4b fuera de la lista y la revision de PB-T4b en la v4.4), 19 (la recomprobacion de la '
                             'v4.5), %s (la comprobacion acotada de PB-T4b, CA-T4b, sobre el contrato en seco y la %s) y %s (su recomprobacion sobre la %s)'
                             % (BOUNDED_SECTIONS[0], CAT4B_PKG, BOUNDED_SECTIONS[1], PKG_VERSION))]):
    if p not in _have:
        _auth.append({'Path': p, 'Section': s, 'Class': 'UNIT_DOC'})
_auth += [{'Path': p, 'Section': 'el texto literal del GREEN de PB-T4b (green_pbt4b_v45.py; D-PBT4-04 a D-PBT4-09 y D-S4)', 'Class': 'UNIT_CHANGE'} for p in UNIT_CHANGE_P]
c = {}
for k in pbt4a:
    c[k] = pbt4a[k]
c.update({'TaskId': TASK, 'Objective': OBJECTIVE, 'AuthorityRevision': AR, 'MainSha': MAIN, 'Authorities': _auth, 'AllowedWriteScope': ALLOWED,
          'ForbiddenWriteScope': FORBIDDEN, 'Invariants': INV,
          'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
          'ExpectedEvidence': EVID, 'StopConditions': STOPS,
          'IssuedBy': ('Sesion principal de I-64 (Claude), emision de la segunda subtarea de PB-T4 (' + EMIT_ENTRY + ') por F4B-E49, pregunta 4 (la emision '
                       'condicional de PB-T4a, PB-T4b y PB-T4c en ese orden; owner-pbt4-decisions.md), con la lista exacta de ' + str(LIST_TOTAL) + ' que el '
                       'Owner tomo como la que vio (owner-pbt4b-retire-list-decisions.md, owner-pbt4b-breaks-decision.md, owner-pbt4b-list-477-decision.md y '
                       'owner-pbt4b-list-478-decision.md) y su decision sobre el coste (owner-pbt4b-cost-decision.md): PB-T4b tras el cierre de PB-T4a ('
                       + (PT4A_CLOSE[:8] if PT4A_CLOSE else MARK_T4A) + ', ' + T4A_CLOSE_ENTRY + '), con su comprobacion acotada (' +
                       (BOUNDED_RESULT or '<resultado por rellenar>') + '); las decisiones de diseno de la sesion (D-S1 a D-S6); F4B-E46; F4B-E40 y F4B-E43, '
                       'los sustitutos de nc1 y nc2; F4B-E29, D-1; F4B-E11, D2; F4B-E35, la regla RR; F4B-E21; F4B-E25, con WHC, WEI, FVM, PV, '
                       'NumericFieldValidation.cs y RIA; y el paquete del contrato de PB-T4 ' + PKG_VERSION),
          'IssuedUtc': ISSUED})
assert list(c) == list(pbt4a), 'las claves del contrato de PB-T4b no son las del de PB-T4a'
b = (json.dumps(c, indent=2, ensure_ascii=True) + '\n').encode('ascii')
assert all(x < 0x80 for x in b) and b'\\u' not in b, 'no ASCII'
SIZES = {k: len(json.dumps(c[k], indent=2, ensure_ascii=True).encode('ascii')) for k in c}
OPAQUE = ('AllowedWriteScope', 'ForbiddenWriteScope', 'Invariants', 'RequiredTests', 'ExpectedEvidence', 'StopConditions', 'EligibleCells', 'Authorities')
_c0 = dict(c)
for k in OPAQUE:
    _c0[k] = []
NON_OPAQUE = len((json.dumps(_c0, indent=2, ensure_ascii=True) + '\n').encode('ascii'))
# el tamano con todas las filas de las copias (las de MinSelected 0 tambien; sin precondicion 6, cada una con un MinSelected de prueba de 3 cifras)
_req_full = [{'Project': p, 'Filter': f, 'MinSelected': 999 if m == MARK_P6 else int(m), 'ExpectRed': e} for _, p, f, m, e in REQ_PKG]
REQ_FULL_BYTES = len(json.dumps(_req_full, indent=2, ensure_ascii=True).encode('ascii'))
CONTRACT_FULL_BYTES = len(b) - SIZES['RequiredTests'] + REQ_FULL_BYTES
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


# ---------------------------------------------------------------- lo que se custodiara con la emision de PB-T4b: AVISOS en la fase 1
# la comprobacion acotada de PB-T4b (CA-T4b): BOUNDED_RESULT y bounded_check (el de make_pbt3a_contract.py, tal cual), sobre el tramo de
# independent-review-v1.md que va del encabezado de la seccion BOUNDED_SECTIONS[0] (el de CA-T4b) al primer encabezado '## ' de una seccion que no es
# de BOUNDED_SECTIONS (CA-T4b M-08): las secciones anteriores del mismo archivo son de otras revisiones y no entran
soft(BOUNDED_RESULT, 'emision: falta BOUNDED_RESULT, el veredicto literal de la comprobacion acotada de PB-T4b (CA-T4b)')
assert BOUNDED_RESULT is None or re.match(r'EMITIBLE(?: TRAS CORRECCIONES)?[.,;]', BOUNDED_RESULT), ('BOUNDED_RESULT: solo la familia EMITIBLE', BOUNDED_RESULT)


def cat4b_report(text):
    """El tramo de CA-T4b de independent-review-v1.md y los numeros de sus secciones; (None, []) si no tiene la primera (la de CA-T4b)."""
    _h = re.compile(r'## ' + chr(0xA7) + r'(\d+)\. ')
    lines = text.replace('\r\n', '\n').split('\n')
    k0 = [k for k, l in enumerate(lines) if _h.match(l) and _h.match(l).group(1) == BOUNDED_SECTIONS[0]]
    if len(k0) != 1 or 'CA-T4b' not in lines[k0[0]]:
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
if BOUNDED_SRC:
    _rep, BOUNDED_FOUND = cat4b_report(_rep_all)
    if _rep is None:
        soft(False, 'emision: la comprobacion acotada de PB-T4b aun no esta: %s no tiene la seccion %s de CA-T4b' % (BOUNDED_SRC, BOUNDED_SECTIONS[0]))
        BOUNDED_SRC = None
    else:
        assert BOUNDED_FOUND == list(BOUNDED_SECTIONS[:len(BOUNDED_FOUND)]), ('las secciones de CA-T4b, seguidas y en su orden', BOUNDED_FOUND)
        soft(BOUNDED_FOUND == list(BOUNDED_SECTIONS), 'emision: de las secciones %s de CA-T4b, %s solo tiene la %s (falta la recomprobacion)'
             % (' y '.join(BOUNDED_SECTIONS), BOUNDED_SRC, ' y la '.join(BOUNDED_FOUND)))
        if BOUNDED_RESULT:
            _err = bounded_check(_rep, BOUNDED_RESULT, PKG_LINES, reviewer='CA-T4b')
            assert not _err, ('la comprobacion acotada de PB-T4b', BOUNDED_SRC, _err)
else:
    soft(False, 'emision: la comprobacion acotada de PB-T4b (%s, secciones %s) no esta ni en la revision ni en --bounded-file'
         % (BOUNDED_CHECK, ' y '.join(BOUNDED_SECTIONS)))
soft(in_head(SESSION_DEC), 'emision: el registro de las decisiones de diseno de la sesion (D-S1 a D-S6), a custodiar y declarar en %s: %s' % (EMIT_ENTRY, SESSION_DEC))
_sup = show(PILOT + '/F4B-DRAFTS/superseding-characterization.md')
soft('\n## PB-T4b ' in _sup, 'emision: el registro declarado aun no tiene la entrada de PB-T4b (las %d, por clase y por causa, con su tabla al cerrar)' % LIST_TOTAL)
for _p in [q for q in (OWNER_DEC, OWNER_DEC2, LIST_470, LISTS_ALL, COPY_MAP, LIST) if in_head(q)]:
    assert not local_paths(show(_p)), ('rutas locales en un custodiado', _p)
if LIST_SHA == LIST_470_SHA:
    pending('la lista de 475 (owner-pbt4b-breaks-decision.md, "Anadirlas a la lista"): LIST_NAME, LIST_SHA y LIST_COUNTS los fija la v4.4 con su archivo '
            'nuevo; hoy el generador lee la de 470, la historia')
soft(in_head(OWNER_DEC3), 'emision: el registro de la decision del Owner sobre las cinco roturas (%s) aun no esta en la revision' % OWNER_DEC3)
soft(in_head(OWNER_DEC4), 'emision: el registro de la quinta tanda del Owner, la lista de 477 (%s), aun no esta en la revision' % OWNER_DEC4)
soft(in_head(OWNER_DEC6), 'emision: la septima tanda del Owner, la decision sobre el coste de PB-T4b (%s), aun no esta en la revision' % OWNER_DEC6)
soft(in_head(OWNER_DEC5), 'emision: el registro de la sexta tanda del Owner, la lista de 478 (%s), aun no esta en la revision' % OWNER_DEC5)
assert in_head(COST_DEC), ('la decision del Owner sobre el coste, custodiada en el cierre de PB-T4a', COST_DEC)
print('DRY (FASE 2) rev', REV_SHA[:8], '(objetos del checkout principal)' if OBJ_REV else '(HEAD del worktree)', 'TaskId', TASK, '| contrato en seco: PENDIENTES',
      len(PENDING), 'AVISOS', len(WARN))
print('anclaje: desciende de d8ccbcb8 (F4B-E29), %s (F4B-E48), %s y %s (PB-T4a, %s; X y BASE); src y tests == %s; el cierre de PB-T4a: %s'
      % (PT3C_CLOSE[:8], PT4A_EMIT_X[:8], PT4A_EMIT[:8], T4A_EMIT_ENTRY, SRC_BASE[:8], PT4A_CLOSE[:8] if PT4A_CLOSE else 'PENDIENTE'))
print('numeracion prevista: %s (cierre de PB-T4a) y %s (la emision); la evidencia %s (cierre), %s (X) y %s (BASE) | hoy: F4B-E%d y la %s'
      % (T4A_CLOSE_ENTRY, EMIT_ENTRY, CLOSE_EV, X_EV, BASE_EV, _ids[-1], _evs[-1]))
print('contrato de PB-T4a (%s, AuthorityRevision %s): 17 paradas estandar, PBT4A-C11 -> PBT4B-C11 (PB-T4a -> PB-T4b, %d caracteres), EVID[0] en '
      'F4B_EXACT_EVIDENCES del v2, 38 filtros' % (PBT4AC, pbt4a['AuthorityRevision'][:8], len(C11)))
print('nc1 %s y nc2 v2 %s con --task PBT4; la Snapshot K' % (NC1_SUBST_SHA[:8], NC2_V2_SHA[:8]))
print('paquete leido: %s -> %s | sha256 %s | el Objective de 6.2.1 (ASCII, %d caracteres)' % (PKG_SRC, PKG_VERSION, hashlib.sha256(PKG_RAW).hexdigest()[:16],
                                                                                                  len(OBJECTIVE)))
print('la lista de %d: %s sha256 %s (%s); C-0 %d, C-5 %d, UI %d; %d pruebas en su archivo, con su atributo sin Skip y su linea en el rango (%d resultados), '
      '%s | la de 470 (historia): sha256 %s, el filtrado de pbt4-substitution-lists.tsv %s, byte a byte | anadidas: %s'
      % (LIST_TOTAL, LIST_SRC, LIST_SHA[:8], 'la de 470' if LIST_SHA == LIST_470_SHA else 'la de 470 mas las de la decision del Owner',
         LIST_COUNTS.get('PB-T4b C-0', 0), LIST_COUNTS.get('PB-T4b C-5', 0), LIST_COUNTS.get('PB-T4b UI', 0), len(LISTED), LIST_RESULTS,
         ('con CT-PBT4A-06, %s:%d' % (CT_PBT4A_06_NAME, CT_PBT4A_06_LINE)) if PT4A_CLOSE and REV_SHA != PT4A_EMIT else 'mas la fila de CT-PBT4A-06 (PENDIENTE)',
         LIST_470_SHA[:8], LISTS_ALL_SHA[:8],
         ', '.join('%s:%d' % (os.path.basename(r[1]), LISTED[(r[1], r[2])]['line']) for r in ADDED) or 'ninguna (la lista es la de 470)'))
print('las 9 clases (C-0), enteras; el mapa de la copia (D-S1: %s, sha256 %s): %d mecanicas y %d a mano (%s)'
      % (CM_SRC, COPY_MAP_SHA[:8], COPY_COUNTS['mecanica'], COPY_COUNTS['a mano'], HAND_TALLY))
for _p in NINE:
    print('  %-70s pruebas=%3d sha256=%s %s' % (_p, sum(1 for r in ROWS if r[1] == _p), ORIG[_p], 'CRLF' if ORIG_CRLF[_p] else 'LF'))
print('C-5 (%d): %s' % (sum(C5_FILES.values()), '; '.join('%s %d' % (os.path.basename(p), n) for p, n in C5_FILES.items())))
print('UI (%d): %s' % (sum(UI_FILES.values()), '; '.join('%s %d' % (os.path.basename(p), n) for p, n in UI_FILES.items())))
print('el alcance provisional (section 6.2, prosa): %d de src existentes, %d nuevos de src, %d nuevos de tests (sin las clases de CT-01 a 18), %d de los Skip '
      '= %d; ForbiddenWriteScope provisional: src %d, tests %d, raiz %d (de PB-T4a) = %d' % (len(EXISTING_SRC), len(NEW_SRC), len(NEW_TESTS), len(SKIP_FILES),
                                                                                         len(ALLOWED_PROV), len(SRCF_PROV), len(TESTF_PROV), len(ROOT_PROV),
                                                                                         len(SRCF_PROV) + len(TESTF_PROV) + len(ROOT_PROV)))
# el tamano (F4B-E21: la sesion pone las colecciones opacas): lo que ya se puede medir, frente al contrato de PB-T4a
_fj = len(json.dumps(SRCF_PROV + TESTF_PROV + ROOT_PROV, indent=2, ensure_ascii=True).encode('ascii'))
_fa = len(json.dumps(pbt4a['ForbiddenWriteScope'], indent=2, ensure_ascii=True).encode('ascii'))
_ca = len((json.dumps(pbt4a, indent=2, ensure_ascii=True) + '\n').encode('ascii'))
_tf = {}
for f in TESTF_PROV:
    k = '/'.join(f.split('/')[:3]) + '/'
    _tf[k] = _tf.get(k, 0) + 1
print('tamano: ForbiddenWriteScope provisional %d entradas y %d bytes en JSON, frente a %d entradas y %d bytes del de PB-T4a (su contrato entero: %d bytes); '
      'tests por carpeta: %s' % (len(SRCF_PROV) + len(TESTF_PROV) + len(ROOT_PROV), _fj, len(pbt4a['ForbiddenWriteScope']), _fa, _ca,
                                 ', '.join('%s %d' % kv for kv in sorted(_tf.items(), key=lambda kv: -kv[1])[:6])))
_prec = SRCF_PROV + TESTF_PREC + [a for a in T4A_OWN_TESTS if a not in ALLOWED_PROV and not a.endswith('PushBackFullReadGuardTests.cs')] + ROOT_PROV
print('tamano con el precedente de tests/ (la lista de PB-T4a, %d, menos %d rutas del alcance de PB-T4b: %s; los prefijos partidos: %s; mas las clases de '
      'PB-T4a tras su cierre): ForbiddenWriteScope %d entradas y %d bytes en JSON'
      % (len(_t4a_tests), len(TESTS_OUT_OF_T4A), ', '.join(os.path.basename(a) for a in TESTS_OUT_OF_T4A),
         ', '.join('%s en %d' % kv for kv in SPLIT_PREFIXES) or 'ninguno', len(_prec), len(json.dumps(_prec, indent=2, ensure_ascii=True).encode('ascii'))))
print('CONTRATO (en seco) allowed %d forbidden %d (src %d tests %d [prefijos partidos: %s] root %d) inv %d stops %d required %d (de %d; %s) '
      'evidence %d authorities %d bytes %d sha256 %s' % (len(ALLOWED), len(FORBIDDEN), len(SRCF), len(TESTF), ', '.join(SPLIT2) or 'ninguno', len(ROOT), len(INV),
                                                        len(STOPS), len(REQ), N_REQ_ROWS,
                                                        ('las de las copias, de la precondicion 6: %d con MinSelected, %d con 0 salen'
                                                         % (sum(1 for v in P6['MinSelected'].values() if v), sum(1 for v in P6['MinSelected'].values() if not v)))
                                                        if P6 is not None else 'las de las copias esperan a la precondicion 6',
                                                        len(EVID), len(c['Authorities']), len(b), hashlib.sha256(b).hexdigest().upper()))
print('la CI prevista ([14]): GREEN Core %d = %d + 0 + %d (los %d Skipped: %d de la base y %d de la lista de Core) y UI %d = %d + %d (%d y %d); el RED, con '
      'R_C y R_UI de la precondicion 6' % (GREEN_T[0], GREEN_T[1], GREEN_T[2], GREEN_T[2], CI_BASE_T4A[3], GREEN_T[2] - CI_BASE_T4A[3], GREEN_T[3], GREEN_T[4],
                                            GREEN_T[5], CI_BASE_T4A[5], GREEN_T[5] - CI_BASE_T4A[5]))
print('piezas por sha256: %s' % '; '.join('%s %s' % (k, v[:8]) for k, v in PIECES.items()))
if P6 is not None:
    print('la precondicion 6 (%s, sha256 %s): R_C %d, R_UI %d; %d clases de las copias, %d con pruebas (las de 0 salen de la tabla); CopySha de %d copias; '
          'particion %s; campos que no lee: %s' % (os.path.basename(P6_FILE) if P6_FILE else 'en la revision', hashlib.sha256(open(P6_FILE, 'rb').read()).hexdigest()[:8]
                                                  if P6_FILE else '-', P6['R_C'], P6['R_UI'], len(P6['MinSelected']),
                                                  sum(1 for v in P6['MinSelected'].values() if v), len(P6['CopySha']), P6['PartitionSha256'][:8],
                                                  ', '.join(P6_EXTRA) or 'ninguno'))
print('BOUNDED_RESULT: %s | bounded_check: %s' % (BOUNDED_RESULT or 'por rellenar',
                                                  ((('sin errores sobre ' + BOUNDED_SRC) if BOUNDED_RESULT else ('no corre sin BOUNDED_RESULT (informe: %s)'
                                                                                                                  % BOUNDED_SRC))
                                                   + ', secciones ' + ' y '.join(BOUNDED_FOUND)) if BOUNDED_SRC else 'sin informe'))
print('tamano del contrato: %d bytes con %d filas de RequiredTests; con las %d (las de las copias con un MinSelected de 3 cifras): %d bytes; RequiredTests %d '
      'bytes con todas; sin las ocho colecciones opacas (F4B-E21): %d bytes. Por coleccion: %s'
      % (len(b), len(REQ), N_REQ_ROWS, CONTRACT_FULL_BYTES, REQ_FULL_BYTES, NON_OPAQUE, ', '.join('%s %d' % (k, SIZES[k]) for k in OPAQUE)))
print('RequiredTests heredados de PB-T4a, con los Skip de la lista (criterio de count_tests_git.py sobre los objetos de la revision):')
for l in REQ_REPORT:
    print('  ' + l)
for _proj, _c, _run, _skip, _mem, _keys in REQ_NEW_EXISTING:
    print('  %-4s %-62s nuevo de section 6.2, ya en la base: ejecutables=%d skip=%d memberdata=%d (count_static las atribuye a %s)'
          % (_proj, _c, _run, _skip, _mem, ', '.join(_keys)))
print('PENDIENTES (%d):' % len(PENDING))
for p in PENDING:
    print('  - ' + p)
AUTH_MISSING = [a['Path'] for a in c['Authorities'] if not in_head(a['Path'])]
print('autoridades que aun no estan en la revision (las custodia X de PB-T4b): %d%s' % (len(AUTH_MISSING), ''.join('\n  ' + p for p in AUTH_MISSING)))
print('AVISOS (%d):' % len(WARN))
for w in WARN:
    print('  - ' + w)
assert not (AS_WRITE and AUTH_MISSING), ('como al emitir: una autoridad no esta en la revision', AUTH_MISSING)
if DRY:
    sys.exit(0)
# ---------------------------------------------------------------- la emision (sin --dry): en el worktree, sobre el commit X, como el de PB-T4a (lo llama
# issue_pbt4b.py --commit): todo lo que en seco es AVISO o PENDIENTE ya abortaba (AS_WRITE); aqui, lo que tiene que estar en la revision
assert OBJ_REV is None and not PKG_FILE and not P6_FILE and not BOUNDED_FILE and not JSON_OUT, \
    'al emitir, solo el worktree: sin --objects-rev, --package-file, --p6-file, --bounded-file ni --json-out'
assert REV_SHA == AR, ('al emitir, la AuthorityRevision es el commit X, el HEAD del worktree', AR[:8], REV_SHA[:8])
assert BOUNDED_RESULT and BOUNDED_SRC and BOUNDED_FOUND == list(BOUNDED_SECTIONS) and E52_OK and P6 is not None and in_head(PKG) and \
    hashlib.sha256(show_bytes(PKG)).hexdigest() == CUSTODY_SHA256[PKG], ('faltan BOUNDED_RESULT, las secciones de CA-T4b, F4B-E52, la precondicion 6 o la %s '
                                                                         'custodiada' % PKG_VERSION)
assert not PENDING and not WARN and not AUTH_MISSING, (PENDING, WARN, AUTH_MISSING)
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', "Get-Content -Raw '%s' | Test-Json -SchemaFile "
                    "'docs/automation/agent-execution/schemas/gate-contract.schema.json'" % OUT], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'required', len(REQ), 'evidence', len(EVID),
      'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
sys.exit(0)
