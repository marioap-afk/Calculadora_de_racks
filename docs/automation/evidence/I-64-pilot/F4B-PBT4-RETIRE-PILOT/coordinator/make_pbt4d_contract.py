"""Contrato de gate de F4B-PBT4-RETIRE-PILOT (PB-T4d, la retirada del codigo del piloto; ASCII), desde el paquete del contrato de PB-T4 (PKG_VERSION,
abajo), con el patron de make_pbt4c_contract.py, sobre el cierre de PB-T4c (PT4C_CLOSE; F4B-E55, la evidencia 285), con PB-T4d literal en 6.4, 6.4.1 y
6.4.2 del paquete. PB-T4d se emite con F4B-E56, la evidencia 286 (X) y 287 (BASE): parametros, comprobados al emitir.
Comprueba por objetos git:
- el anclaje: la revision desciende de la emision de PB-T4c (X y BASE), de su RED, de su GREEN y de su cierre; src y tests, los del GREEN de PB-T4c; la
  numeracion prevista; la CI del GREEN de PB-T4c en la entrada de su cierre;
- el contrato de PB-T4c de su emision: las 17 paradas estandar, PBT4C-C11 (de la que sale PBT4D-C11), la primera ExpectedEvidence y sus filtros;
- los sustitutos de nc1 y de nc2 v2 con --task PBT4, y la Snapshot K;
- el paquete, por su sha256: 4.4 (la lista por grupo y accion), 5.6, 6.4 (el alcance ruta a ruta, con los borrados de src/), sus textos literales
  (6.4.1: el Objective, los invariantes, las paradas, la ExpectedEvidence, los Skip, los archivos nuevos y los RequiredTests) y lo que necesita el
  generador (6.4.2: las piezas por sha256, la lista y la precondicion 6);
- las dos piezas (el RED de tests/ y el GREEN de src/), leidas por sus bloques y aplicadas en memoria sobre los objetos de la base: sus rutas son el
  alcance, cada OLD esta una vez, los archivos nuevos son los de NewFileSha, y el RED y el GREEN dan los sha256 de la precondicion 6;
- la lista de PB-T4d (LIST_NAME, por su sha256), fila a fila frente a la base y al RED: cada prueba en su archivo, en su rango y con el
  Skip de su columna; lo que se borra o se sustituye ya no esta en el RED (salvo los tres Skip), lo que se queda esta literal, y lo que se reescribe o se
  acota sigue sin Skip; C02 nombra exactamente esa lista, con sus acciones, y nada mas;
- la precondicion 6 de la sesion (pbt4d-precondition6.json y compile-check.md de cc4d/; --p6-file en seco, de la revision al emitir): la lista, las
  piezas, los archivos del RED y del GREEN, los totales de cada rama, la cobertura, el censo, el ensayo y su control, las tablas por clase y los
  RequiredTests medidos; se lee antes de decidir cualquier pendiente que dependa de ella (la leccion de la BASE de PB-T4b);
- los RequiredTests: los heredados del contrato de PB-T4c por su numero, con el MinSelected medido sobre el GREEN, los que quedan en 0 fuera, y los nuevos;
  los totales de cada rama frente a la base, a la tabla del generador, a la precondicion 6 y a [14];
- la comprobacion acotada de PB-T4d (CA-T4d: independent-review-pbt4d-check.md, o --bounded-file) frente a BOUNDED_RESULT, con bounded_check;
- al emitir (sin --dry), todo de la revision X.
No escribe ningun contrato en seco: lista los PENDIENTES y los AVISOS.
Uso: python make_pbt4d_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> --dry [--objects-rev <rev> [--repo <checkout principal>]
                                   [--package-file <paquete>] [--p6-file <pbt4d-precondition6.json>] [--bounded-file <independent-review-pbt4d-check.md>]]
                                   [--json-out <archivo>] [--as-write]
Con --p6-file, compile-check.md se lee de la misma carpeta. Al custodiarlo, las rutas locales se redactan como en PB-T0 ('<worktree-de-I-64>',
'<checkout-principal>')."""
import json, os, re, sys, subprocess, hashlib, unicodedata

# la leccion del cierre de PB-T3c: el generador entero es ASCII (lo leen en ascii los check_* y la redaccion); BOUNDED_RESULT tambien
_self = open(os.path.abspath(__file__), 'rb').read()
assert all(b < 0x80 for b in _self), ('el generador lleva un caracter no ASCII', [i for i, b in enumerate(_self) if b >= 0x80][:5])

# ---------------------------------------------------------------- la base: la emision de PB-T4c (F4B-E54) y su cierre (F4B-E55)
PT4C_EMIT_X = 'a8e9cbe686785eef5f741a798f1e54f4419f35a1'   # X de la emision de PB-T4c (la evidencia 283): AuthorityRevision de su contrato
PT4C_EMIT = 'd502601477c21972ca2109b0d5c25c6fddfe4989'     # BASE de la emision de PB-T4c (la evidencia 284)
RED_T4C = '280c79beac61d31b03f86488c03686eef4f694bc'       # el RED de PB-T4c (sin reparacion del RED)
GREEN_T4C = '7cc1076eae8159294a1498ab556851977223b64c'     # el GREEN de PB-T4c (intento 1)
PT4C_CLOSE = '6c2820bab4d8b41e3e229e23f3a1638ccbb04735'    # la custodia del cierre de PB-T4c (F4B-E55, la 285); la base de PB-T4d
# la CI del GREEN de PB-T4c en su cierre: (run, sha8 del GREEN, Core, Skipped de Core, UI, Skipped de UI); la del RED, 44956801926
CI_BASE_T4C = ('44959653142', '7cc1076e', 19619, 461, 2486, 49)
# la numeracion (la sesion): parametros comprobados contra f4b-dispositions.md y la evidencia (una asignacion cada uno, para leerlos con ast)
T4C_EMIT_ENTRY = 'F4B-E54'
T4C_CLOSE_ENTRY = 'F4B-E55'
EMIT_ENTRY = 'F4B-E56'
T4C_X_EV = '283'
T4C_BASE_EV = '284'
CLOSE_EV = '285'
X_EV = '286'
BASE_EV = '287'
T4C_TASK = 'F4B-PBT4-EDIT-ALL-WINDOW'
# CA-T4d: su veredicto literal (ASCII), la linea BOUNDED de su informe con la cola de la recomprobacion si la hay, copiada del archivo con un guion y
# comparada; None mientras la comprobacion acotada no exista (PENDIENTE en seco)
BOUNDED_RESULT = ('EMITIBLE TRAS CORRECCIONES; B 0 / I 2 / M 7; las importantes a corregir en una v5.3 (I-01: las pruebas 48 y 49 de WPUR prueban PushBackPilot'
                  'SystemRebuild, que se queda, y la lista las borra; I-02: el criterio de cobertura solo mira 436 lineas de 2 archivos); con una decision nuev'
                  'a del Owner (I-01, por su regla de owner-pbt4d-run-coverage-decision.md: sustituir la 48 y la 49, 29 sustitutas, o declarar la perdida); pre'
                  'condicion 6 sobre 6c2820ba: base 19619 = 19158 + 0 + 461 igual a la CI 44959653142 registrada, RED 19416 = 19012 + 1 + 403, GREEN 19416 = 19'
                  '013 + 0 + 403, UI 2464 = 2416 + 0 + 48, las 436 lineas PASAN pero la suite completa de Core pierde PushBackPilotSystemRebuild.cs:32 y 6 rama'
                  's (I-01) y 1 rama de PushBackCompositeEditorState.cs:233 (M-02); GREEN y RED reproducidos por objetos git con los mismos sha256; recomprobac'
                  'ion v5.3: EMITIBLE; B 0 / I 0 / M 6 (M-08 a M-13, para una v5.4 de solo texto); I-01 resuelto (29 sustitutas, 26 por el mapa; PushBackPilotS'
                  'ystemRebuild.cs:32 y sus 6 ramas cubiertas por Core en el GREEN) e I-02 resuelto (cobertura de todo src/ reproducida desde los 43 JSON de co'
                  'verlet: 0 lineas y 0 ramas perdidas, Core sola solo pierde PushBackCompositeEditorState.cs:233, que cubren 7 filtros de UI en su proceso, PE'
                  'RDIDA 0, bajan 16801 + 118, ninguna solo por las corridas juntas); M-01 a M-07 aplicados; 1 sustituta nueva sobre las 29; sin decisiones nue'
                  'vas del Owner; los tres desajustes del generador son menores, y el 1 pide fijar los sha256 de los 18 JSON de UI por filtro del GREEN, no sol'
                  'o quitar GreenUiCoverage; precondicion 6 sobre 6c2820ba: base 19619 = 19158 + 0 + 461 igual a la CI, RED 19418 = 19014 + 1 + 403, GREEN 1941'
                  '8 = 19015 + 0 + 403, UI 2464 = 2416 + 0 + 48, copia principal sola, 0 warning CS; recomprobacion v5.4: EMITIBLE; B 0 / I 0 / M 3 (M-14 a M-1'
                  '6, de texto y de custodia, sin decision del Owner); M-08 a M-13 resueltos: el JSON fija el sha256 de los 3 + 18 JSON de UI del GREEN y el co'
                  'mpleto de las TRX y los JSON de los 19 filtros de la base, recalculados por mi y casan, y 6.4.2 nombra sus 32 claves en orden con RedDeleted'
                  ' contra los DELETE y Noise sin leer; [13] dice D-S11 a D-S16; la cabecera de la v5.3 nombra 0, 1, 6.0, 7 y 9; cov_p6_v54.py exige un filtro '
                  'en su proceso para ESTABLE solo por la UI, PASA, PERDIDA 0, VARIA 0, ESTABLE 16801 + 118, y en el GREEN hay 0 claves que solo cubran las cor'
                  'ridas juntas; el RED v5.4 solo cambia comentarios de WPUR (codigo igual en los 28 archivos, aplicado por mi) y el GREEN solo su cabecera; la'
                  ' lista 271 solo cambia el porque de la fila 282 y anade la 284 (directiva); el paso de 436 a 537 queda explicado; nuevos: M-14 (Lo que cambi'
                  'a de la v5.4 nombra la seccion 7, que no cambia, y omite D-PBT4-10, 5.6 y 6.4, el modo de M-10), M-15 (la nota de D-S16 dice cov_p6_v53.py y'
                  ' la definicion de PERDIDA no se ajusto) y M-16 (RedFiles de WPUR es el RED v5.3 sin declararlo, y 11.9 no custodia las piezas de la v5.3); t'
                  'otales, RequiredTests, filtros y decisiones sin cambio')  # la linea BOUNDED de CA-T4d y las colas de sus dos recomprobaciones, copiadas del informe
BOUNDED_NAME = 'independent-review-pbt4d-check.md'  # el informe de CA-T4d, propio (como el de CA-T4a), en F4B-DRAFTS/pbt4/ (la sesion)
CAT4D_PKG = 'v5.2'  # la version del paquete sobre la que corre CA-T4d (su recomprobacion, sobre PKG_VERSION)
# la version del paquete: la v5.4 (los menores M-08 a M-13 de la recomprobacion 1 de CA-T4d y los tres desajustes de la v5.3); la v5.3, historia
PKG_VERSION = 'v5.4'
PKG_TARGET = 'v5.4'
VTAG = 'v54'  # el sufijo de las piezas y los guiones de esta version
CUSTODY_SHA256 = {'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md': 'e37843fc4bb5a1d100191928ad06ce05cbf19705f5084b809442257ba866fc63'}  # la v5.4
# la lista de PB-T4d (6.4 y 6.4.2 de la v5.3): el alcance B con las sustitutas, las 29 de WPUR, la rama muerta de BuildCore y los ayudantes sin uso
# (CA-T4d M-03); la de 229 (v5.2), historia
LIST_NAME = 'pbt4d-retire-list-271.tsv'
LIST_SHA = '05a4d3d1905529c5bc4d53e0684d27d97eaa96502098eebeb9d16bfd931c75ac'
LIST_HEADER = ['n', 'grupo', 'archivo', 'miembro', 'lineas-6c2820ba', 'skip-6c2820ba', 'accion', 'porque', 'en-A', 'necesidad-r6h', 'declaraciones-6c2820ba',
               'tramo-editado']
# las acciones de la lista y su nombre en C02 (6.4.1), en el orden de C02; "se queda" no es accion (no se emite)
ACTIONS = [('borrar con registro', 'delete with record'), ('reescribir con registro', 'rewrite with record'),
           ('sustituir con registro', 'substitute with record'), ('acotar con registro', 'bound with record')]
KEEP_ACTION = 'se queda'
OWNER_D = ['owner-pbt4d-list-decisions.md', 'owner-pbt4d-substitutes-decision.md', 'owner-pbt4d-run-coverage-decision.md',
           'owner-pbt4d-deadcode-27-decisions.md', 'owner-pbt4d-cat4d-decisions.md']  # en coordinator/, autoridades de PB-T4d (las tandas 10 a 14)
SESSION_DEC_D = 'session-pbt4d-design-decisions.md'                           # D-S11 a D-S16, en F4B-DRAFTS/pbt4/
# las piezas por sha256 que trae la v5.4 (6.4.2): las dos piezas literales, sus modulos y aplicadores, sus generadores, las ediciones como datos y el
# generador de la lista. Su sha256 lo da el paquete; el archivo, la revision (al emitir) o junto a --package-file (en seco)
PIECE_NAMES = ['pbt4d-red-v54.txt', 'red_pbt4d_v53.py', 'apply_red_pbt4d_v53.py', 'v54/make_red_v54.py', 'pbt4d_test_edits_v54.py',
               'v53/make_run_eq_v53.py', 'v53/unused_v53.py', 'v52/make_fullcopy_eq_v52.py', 'pbt4d-green-v54.txt', 'green_pbt4d_v53.py',
               'apply_green_pbt4d_v53.py', 'v53/make_green_v53.py']
RED_PIECE, GREEN_PIECE = 'pbt4d-red-%s.txt' % VTAG, 'pbt4d-green-%s.txt' % VTAG
LIST_GEN = 'v54/make_list_v54.py'
# el RED de la version con que corrio la precondicion 6 (decision de la sesion: la v5.3; la v5.4 solo cambia comentarios de los archivos que dice
# Reapply.TestsTextChanged): su nombre y su sha256 (el de la v5.3)
RED_P6 = ('pbt4d-red-v53.txt', 'de0d3a0e1961eb5c6d2f699eab086984310681bf558c14f09e5486d8ea9bc24e')
N_ALLOWED = 37
N_REQ_ROWS = 88  # 83 heredados (los n.o 37 y 85 de PB-T4c salen) y 5 nuevos
N_REQ_INH = 83
# lo que fija la version del paquete: las CT y los Failed del RED (5.6)
N_CT = 2  # CT-PBT4D-01 y 02
N_CT_CORE, N_CT_UI = 2, 0
RED_FAIL_CORE, RED_FAIL_UI = 1, 0   # los Failed del RED (5.6: solo CT-PBT4D-01, ExpectRed true)
# los totales previstos de cada rama (el paquete, [14] y 6.4.1): (total, Passed, Failed, Skipped) de Core y de UI
CI_T4D_EXPECTED = {'GREEN': (19418, 19015, 0, 403, 2464, 2416, 0, 48), 'RED': (19418, 19014, 1, 403, 2464, 2416, 0, 48)}
# la precondicion 6 de PB-T4d (hs65/pbt4dcc/compile/, en la custodia F4B-DRAFTS/pbt4/cc4d/): su resumen va byte a byte a F4B-DRAFTS/pbt4/P6_REL_NAME;
# su esquema y sus claves los fija 6.4.2. compile-check.md, en cc4d/
P6_REL_NAME = 'pbt4d-precondition6.json'
P6_SCHEMA = 'rackcad-pbt4d-precondition6/v1'
P6_DIR = 'cc4d/'
P6_CHECK_NAME = 'compile-check.md'
P6_REQ_NAME = 'required-pbt4d.tsv'
P6_CHECK_SHA = 'd13afdb02216d385c32ade81b5cf9508c648a970337cf92b9d964b936933de42'  # el registro de la corrida (6.4.1 y 6.4.2)
P6_COV_NAME = 'cov-p6-v54.txt'  # la salida de la cobertura ampliada (6.4.1), por el sha256 de Coverage.Output
# las claves del resumen que el paquete no declara en 6.4.2 (decisiones de la sesion, 2026-11-22): Package y Task se leen (la version del paquete emitida
# y este TaskId). RedDeleted (en 6.4.2 desde la v5.4) se lee: los bloques DELETE del RED y Expected.red-deleted-N. Noise (en 6.4.2 desde la v5.4, como
# dato informativo) es un campo que no lee: el ruido medido de una linea de la UI entre corridas, que no cambia ningun veredicto. Otra clave que el paquete
# no declare es PENDIENTE (la sesion decide). UiRequiredFilters (la v5.2) ya no esta en el resumen
P6_READ_EXTRA = ('Package', 'Task')
P6_UNREAD = ('Noise',)
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
# las decisiones que esta version del generador deja a la sesion: cada una es un PENDIENTE hasta que la sesion la fije (no se decide por analogia).
# Ninguna abierta: las dos de abajo las fijo la sesion (2026-11-22)
SESSION_OPEN = []
# las autoridades (decision de la sesion, 2026-11-22; 6.4.2 no da los numeros): de las 70 del contrato de PB-T4c siguen las que no son de su subtarea, con
# su seccion al dia la 0, 6, 28, 29, 36 y 37; la n.o 43 (CA-T4c) pasa, en su sitio, al informe de CA-T4d; la 48 (owner-pbt4b-code-decisions.md, M-04) se
# queda con su seccion nueva; salen la 44 a la 47 y la 49 a la 69; se anaden las de 6.4.2. Los 9 guiones de 6.4.2 se comprueban por sha256 y no son autoridad
AUTH_PLAN = {'drop': list(range(44, 47)) + [47] + list(range(49, 70)), 'ca_at': 43, 'keep_48': True}
# el ForbiddenWriteScope en tests/ (decision de la sesion, 2026-11-22): el literal de 6.4, "sus cinco clases de las CT y sus cuatro equivalentes" de
# PB-T4c ('literal'); el paquete manda, el precedente del generador de PB-T4c ('precedente', todo tests/ de su alcance) no
FORB_PREV = 'literal'
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
assert len(POS) == 3, 'uso: make_pbt4d_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> --dry [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
DRY = '--dry' in ARGS
# sin --dry, el generador emite (en el worktree, sobre el commit X): lo que en seco es un AVISO o un PENDIENTE aborta (como --as-write)
AS_WRITE = '--as-write' in ARGS or not DRY
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % 'F4B-PBT4-RETIRE-PILOT'
OBJ_REV = opt('--objects-rev')
PKG_FILE = opt('--package-file')
assert PKG_FILE is None or OBJ_REV is not None, '--package-file solo junto con --objects-rev'
PKG_FILE = os.path.abspath(PKG_FILE) if PKG_FILE else None
assert PKG_FILE is None or os.path.isfile(PKG_FILE), ('no existe el paquete', PKG_FILE)
P6_FILE = opt('--p6-file')  # la precondicion 6 de la sesion, en seco (con compile-check.md en su carpeta); al emitir, la de la revision
P6_FILE = os.path.abspath(P6_FILE) if P6_FILE else None
assert P6_FILE is None or (os.path.isfile(P6_FILE) and DRY), ('--p6-file solo en seco y con el archivo', P6_FILE)
BOUNDED_FILE = opt('--bounded-file')  # el informe de CA-T4d, en seco; al emitir, el de la revision
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

TASK = 'F4B-PBT4-RETIRE-PILOT'
assert len(TASK) <= 26 and TASK.startswith('F4B-' + NC_TASK + '-'), len(TASK)  # R-T1-07 (MAX_PATH, una cota superior); la ligadura de --task PBT4
PILOT = 'docs/automation/evidence/I-64-pilot'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
D4 = PILOT + '/F4B-DRAFTS/pbt4/'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
EVIDENCE = 'docs/automation/evidence/I-64-evidence.md'
PBT4CC = PILOT + '/' + T4C_TASK + '/coordinator/gate-contract.json'
T4C_TCP = PILOT + '/' + T4C_TASK + '/task-close-package.md'
PKG = 'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md'
P6_REL = D4 + P6_REL_NAME
P6_CHECK_REL = D4 + P6_DIR + P6_CHECK_NAME
P6_REQ_REL = D4 + P6_DIR + P6_REQ_NAME
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
PILOT_SRC = 'src/RackCad.Application/Systems/PushBack/Pilot/'
WARN = []
PENDING = []  # lo que espera a la precondicion 6, a CA-T4d o a una decision de la sesion: el seco lo lista; al emitir (AS_WRITE) aborta
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
    """Lo que solo existira con la emision de PB-T4d (su commit X, su comprobacion acotada): AVISO en seco, error con --as-write."""
    if cond:
        return True
    assert not AS_WRITE, msg
    WARN.append(msg)
    return False


def pending(msg):
    """Lo que espera a la precondicion 6, a CA-T4d o a una decision de la sesion: se lista en seco (PENDIENTE); con --as-write o al emitir aborta. Un
    pendiente que depende de la precondicion 6 o de CA-T4d se decide despues de leerlas de la revision, nunca antes (la leccion de la BASE de PB-T4b)."""
    assert not AS_WRITE, 'pendiente: ' + msg
    PENDING.append(msg)


REV_SHA = git('rev-parse', REV).stdout.decode('ascii').strip()
assert re.fullmatch(r'[0-9a-f]{40}', REV_SHA), ('revision desconocida', REV)

# ---------------------------------------------------------------- el anclaje: la emision de PB-T4c (F4B-E54) y su cierre (F4B-E55)
for _full in (PT4C_EMIT_X, PT4C_EMIT, RED_T4C, GREEN_T4C, PT4C_CLOSE):
    assert git('rev-parse', _full[:8]).stdout.decode('ascii').strip() == _full, ('SHA', _full[:8])
    assert git('merge-base', '--is-ancestor', _full, REV).returncode == 0, ('la revision no desciende de PB-T4c', _full[:8])
assert git('rev-parse', PT4C_EMIT + '^').stdout.decode('ascii').strip() == PT4C_EMIT_X, 'BASE de PB-T4c no es hijo directo de su X'
assert git('rev-parse', GREEN_T4C + '^').stdout.decode('ascii').strip() == RED_T4C, 'el GREEN de PB-T4c no es hijo directo de su RED (sin reparacion)'
assert git('diff', '--quiet', GREEN_T4C, PT4C_CLOSE, '--', 'src', 'tests').returncode == 0, 'el cierre de PB-T4c no tiene el src y los tests de su GREEN'
assert git('diff', '--quiet', GREEN_T4C, REV, '--', 'src', 'tests').returncode == 0, 'src o tests cambiaron desde el GREEN de PB-T4c'
_disp = show(DISP).replace('\r\n', '\n')
_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
assert _ids == list(range(1, len(_ids) + 1)) and _ids[-1] >= 55, ('f4b-dispositions.md', _ids[-3:])
_n = lambda e: int(e.split('E')[-1])
assert _n(T4C_CLOSE_ENTRY) == _n(T4C_EMIT_ENTRY) + 1 and _n(EMIT_ENTRY) == _n(T4C_CLOSE_ENTRY) + 1, ('la numeracion: E55 sigue a E54 y E56 a E55',
                                                                                                    T4C_CLOSE_ENTRY, EMIT_ENTRY)
assert _ids[-1] in (_n(T4C_CLOSE_ENTRY), _n(EMIT_ENTRY)), ('la ultima entrada de f4b-dispositions.md', _ids[-1])
assert re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (T4C_EMIT_ENTRY, T4C_TASK), _disp), 'F4B-E54, la emision de PB-T4c'
_dc = [p for p in re.split(r'(?m)^## ', _disp) if p.startswith(T4C_CLOSE_ENTRY + '. ')]
assert len(_dc) == 1 and re.match(r'%s\. Cierre de %s\b' % (T4C_CLOSE_ENTRY, T4C_TASK), _dc[0]), 'F4B-E55, el cierre de PB-T4c'
_dc = _dc[0]
E56_OK = bool(re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (EMIT_ENTRY, TASK), _disp))
_evs = re.findall(r'(?m)^## (\d+)\. ', show(EVIDENCE))
assert int(T4C_BASE_EV) == int(T4C_X_EV) + 1 and int(CLOSE_EV) == int(T4C_BASE_EV) + 1 and int(X_EV) == int(CLOSE_EV) + 1 and int(BASE_EV) == int(X_EV) + 1, \
    ('la numeracion de la evidencia', CLOSE_EV, X_EV, BASE_EV)
assert _evs == [str(k) for k in range(1, len(_evs) + 1)] and _evs[-1] in (CLOSE_EV, X_EV), ('la ultima seccion de la evidencia', _evs[-3:])
assert T4C_X_EV in _evs and T4C_BASE_EV in _evs and CLOSE_EV in _evs, 'las secciones 283, 284 y 285 de PB-T4c'
# la CI del GREEN de PB-T4c, en la entrada de su cierre (la linea base de PB-T4d, [14])
_run, _sha8, _core, _cskip, _ui, _uskip = CI_BASE_T4C
_sp = lambda v: format(v, ',').replace(',', ' ')
assert GREEN_T4C.startswith(_sha8) and ('GREEN `%s`' % _run) in _dc and 'CLOSED' in _dc, ('la entrada de cierre de PB-T4c', CI_BASE_T4C)
assert ('Core %s = %s + 0 + %d' % (_sp(_core), _sp(_core - _cskip), _cskip)) in _dc and ('UI %s = %s + 0 + %d' % (_sp(_ui), _sp(_ui - _uskip), _uskip)) in _dc, \
    ('los recuentos de CI_BASE_T4C no son los de la entrada de cierre de PB-T4c', CI_BASE_T4C)
assert 'CLOSED' in show(T4C_TCP), 'el task-close-package de PB-T4c no dice CLOSED'
soft(E56_OK, 'emision: %s (la emision de %s) aun no esta en f4b-dispositions.md' % (EMIT_ENTRY, TASK))
if soft(git('cat-file', '-e', AR + '^{commit}').returncode == 0, 'emision: la AuthorityRevision %s no esta en el almacen de objetos (seco)' % AR[:8]):
    assert git('merge-base', '--is-ancestor', PT4C_CLOSE, AR).returncode == 0, 'la AuthorityRevision no contiene el cierre de PB-T4c'
for _p in (PKG, PBT4CC, T4C_TCP):
    assert in_head(_p), ('custodiado antes de PB-T4d y no esta en la revision', _p)

# ---------------------------------------------------------------- el contrato de PB-T4c de su emision: de el salen los textos comunes, C11 y los filtros
pbt4c = json.loads(show(PBT4CC))
assert pbt4c['TaskId'] == T4C_TASK and pbt4c['AuthorityRevision'] == PT4C_EMIT_X, ('el contrato de PB-T4c', pbt4c['AuthorityRevision'][:8])
T4C_STOP = {s['Id']: s['Text'] for s in pbt4c['StopConditions']}
STD_STOPS = [s for s in pbt4c['StopConditions'] if not s['Id'].startswith('PBT4C-')]
assert len(STD_STOPS) == 17, ('las paradas estandar del contrato de PB-T4c', len(STD_STOPS))
EVID0 = pbt4c['ExpectedEvidence'][0]


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
# la primera ExpectedEvidence: la del contrato de PB-T4c (la de PB-T3c), byte a byte, en la lista cerrada de literales del v2
assert EVID0 in NC2_EVIDENCES, ('la primera evidencia del contrato de PB-T4c no esta en F4B_EXACT_EVIDENCES del v2', EVID0[:80])
# C11 de PB-T4d: PBT4C-C11 del contrato de PB-T4c con PB-T4c -> PB-T4d (la tarea de nc sigue siendo PBT4). El texto final lo fija el paquete
C11_SUBS = [('PB-T4c', 'PB-T4d')]
C11 = T4C_STOP['PBT4C-C11']
for _old, _new in C11_SUBS:
    assert _old in C11, ('PBT4D-C11: PB-T4c no esta en PBT4C-C11', _old)
    C11 = C11.replace(_old, _new)
for _need in ('PBT4-NC2-EXACT-WHITELIST-F4B-01', 'with --task PBT4 in the F4B-E21 mode', 'SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B',
              'with --task PBT4, recorded as nc1 = SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE', NC2_V2 + ', SHA-256 ' + NC2_V2_SHA,
              NC1_SUBST + ', SHA-256 ' + NC1_SUBST_SHA):
    assert C11.count(_need) == 1, ('PBT4D-C11', _need)
assert C11.endswith('no one-off acceptance extends to PB-T4d') and not re.search(r'PBT3C|PB-T3c|PB-T4a|PBT4A|PB-T4b|PBT4B|PB-T4c|PBT4C', C11), \
    'PBT4D-C11: restos de PB-T4c'


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


# ---------------------------------------------------------------- el paquete (PKG_VERSION, por su sha256): 4.4, 5.6, 6.0 y 6.4 de PB-T4d
if PKG_FILE:
    PKG_RAW, PKG_SRC = open(PKG_FILE, 'rb').read(), '--package-file ' + os.path.basename(PKG_FILE)
else:
    PKG_RAW, PKG_SRC = show_bytes(PKG), PKG + ' en ' + REV_SHA[:8]
assert hashlib.sha256(PKG_RAW).hexdigest() == CUSTODY_SHA256[PKG], ('el paquete leido no es la %s revisada (sha256)' % PKG_VERSION, PKG_SRC)
PKG_LINES = PKG_RAW.decode('utf-8').replace('\r\n', '\n').split('\n')
_title = [l for l in PKG_LINES[:8] if l.startswith('# I-64 F4B')]
assert len(_title) == 1 and re.search(r', (v\d+(?:\.\d+)?)\s*$', _title[0]).group(1) == PKG_VERSION, ('la version del paquete', _title)
assert PKG_VERSION == PKG_TARGET, ('el generador de PB-T4d es el de la %s (6.4.1 y 6.4.2 literales)' % PKG_TARGET, PKG_VERSION)


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


SEC64 = section('### 6.4 PB-T4d')
SEC56 = section('### 5.6 PB-T4d')
SEC44 = section('### 4.4 PB-T4d')
_s642 = '\n'.join(section('#### 6.4.2 '))


def local_or_rev(name):
    """Un archivo de F4B-DRAFTS/pbt4/ de la revision, o, en seco y si aun no esta, el de junto a --package-file (la custodia X de PB-T4d al emitir)."""
    if in_head(D4 + name):
        return show_bytes(D4 + name), D4 + name + ' en ' + REV_SHA[:8]
    _loc = os.path.join(os.path.dirname(PKG_FILE), *name.split('/')) if PKG_FILE else None
    if _loc and os.path.isfile(_loc):
        soft(False, 'emision: %s aun no esta en la revision (la custodia X de PB-T4d)' % (D4 + name))
        return open(_loc, 'rb').read(), name + ' junto a --package-file'
    return None, None


# las piezas por sha256 (6.4.2): en la columna "Lo que es", la pieza de la fila ("sha256 `...`") y los guiones que la acompanan ("`nombre` (`sha256`)")
PIECES = {}
for _row in [l for l in _s642.split('\n') if l.startswith('| **')]:
    _cells = _row.split(' | ')
    _where = re.findall(r'`pbt4/([\w./-]+\.(?:txt|py|tsv))`', _cells[1])
    _first = re.match(r'sha256 `([0-9a-f]{64})`', _cells[2])
    if _where and _first:
        assert len(_where) == 1, ('6.4.2: una pieza por fila', _where)
        PIECES[_where[0]] = _first.group(1)
    for _nm, _h in re.findall(r'`((?:v5\d/)?[\w.-]+\.py)`(?: \([^()`]*\))? \(`([0-9a-f]{64})`\)', _cells[2]):
        assert PIECES.get(_nm, _h) == _h, ('6.4.2: dos sha256 para una pieza', _nm)
        PIECES[_nm] = _h
assert PIECES.pop(LIST_NAME) == LIST_SHA and PIECES.pop(LIST_GEN), ('6.4.2: la lista y su generador', LIST_NAME)
assert sorted(PIECES) == sorted(PIECE_NAMES), ('6.4.2: las piezas por sha256 no son PIECE_NAMES', sorted(set(PIECES) ^ set(PIECE_NAMES)))
PIECE_SRC = {}
for _nm in PIECE_NAMES:
    _b, _src = local_or_rev(_nm)
    assert _b is not None and hashlib.sha256(_b).hexdigest() == PIECES[_nm], ('una pieza no esta, o no es la del paquete', _nm, _src)
    PIECE_SRC[_nm] = _b
_LG, _ = local_or_rev(LIST_GEN)
assert _LG is not None and _LG and LIST_GEN in _s642, ('el generador de la lista', LIST_GEN)
for _nm, _h in re.findall(r'`(pbt4d-(?:green|red)-v5\d\.txt)` \(sha256 `([0-9a-f]{64})`', '\n'.join(SEC64)):
    assert PIECES[_nm] == _h, ('6.4 y 6.4.2 dan dos sha256 para una pieza', _nm)
MARK = '@@@@ '


def piece_blocks(raw):
    """Los bloques de una pieza literal (EDIT, NEWFILE, DELETE), con la forma de red_pbt4d_v52.blocks y green_pbt4d_v52.blocks, sin importarlos."""
    lines = raw.decode('utf-8').replace('\r\n', '\n').split('\n')
    out, i = [], 0
    while i < len(lines):
        l = lines[i]
        if not l.startswith(MARK):
            assert l == '', ('una linea fuera de bloque en la pieza', i + 1)
            i += 1
            continue
        h = l[len(MARK):]
        if h.startswith('EDIT '):
            assert lines[i + 1] == MARK + 'OLD', ('EDIT sin OLD', i + 2)
            j = lines.index(MARK + 'NEW', i + 2)
            k = lines.index(MARK + 'END', j + 1)
            out.append(('EDIT', h[5:], '\n'.join(lines[i + 2:j]) + '\n', ('\n'.join(lines[j + 1:k]) + '\n') if k > j + 1 else ''))
            i = k + 1
        elif h.startswith('NEWFILE '):
            k = lines.index(MARK + 'END', i + 1)
            out.append(('NEWFILE', h[8:], '\n'.join(lines[i + 1:k]) + '\n'))
            i = k + 1
        elif h.startswith('DELETE '):
            out.append(('DELETE', h[7:]))
            i += 1
        else:
            assert h.startswith('#'), ('una marca desconocida en la pieza', i + 1)
            i += 1
    return out


def apply_piece(raw, kinds):
    """Aplica una pieza en memoria sobre los objetos de la revision (cada OLD exactamente una vez en el texto que se va editando): (base, texto,
    nuevos, borrados), con los saltos de linea a \\n."""
    base, cur, new, gone = {}, {}, [], []
    for b in piece_blocks(raw):
        assert b[0] in kinds, ('un bloque que esta pieza no admite', b[0], b[1])
        if b[0] == 'EDIT':
            if b[1] not in cur:
                assert in_head(b[1]) and b[1] not in gone, ('un EDIT sobre un archivo que no esta en la revision', b[1])
                base[b[1]] = cur[b[1]] = show_bytes(b[1]).decode('utf-8-sig').replace('\r\n', '\n')
            assert cur[b[1]].count(b[2]) == 1, ('un OLD no esta exactamente una vez', b[1], b[2][:80])
            cur[b[1]] = cur[b[1]].replace(b[2], b[3], 1)
        elif b[0] == 'NEWFILE':
            assert not in_head(b[1]) and b[1] not in cur, ('un NEWFILE que ya existe', b[1])
            cur[b[1]] = b[2]
            new.append(b[1])
        else:
            assert in_head(b[1]) and b[1] not in cur and b[1] not in gone, ('un DELETE de un archivo que no esta o que se edita', b[1])
            base[b[1]] = show_bytes(b[1]).decode('utf-8-sig').replace('\r\n', '\n')
            gone.append(b[1])
    return base, cur, new, gone


# el RED, desde la v5.3, tambien borra archivos de tests/ (bloques DELETE; CA-T4d M-03): RED_GONE; RED_FINAL los da como texto vacio
RED_BASE, RED_TEXT, RED_NEW, RED_GONE = apply_piece(PIECE_SRC[RED_PIECE], ('EDIT', 'NEWFILE', 'DELETE'))
GREEN_BASE, GREEN_TEXT, _gn, GREEN_GONE = apply_piece(PIECE_SRC[GREEN_PIECE], ('EDIT', 'DELETE'))
assert not _gn and all(p.startswith('tests/') for p in list(RED_TEXT) + RED_GONE) and all(p.startswith(PILOT_SRC) for p in list(GREEN_TEXT) + GREEN_GONE), \
    'el RED solo en tests/ y el GREEN solo en src/ de Pilot/ (D-S11)'
RED_EDITED = [p for p in RED_TEXT if p not in RED_NEW]


def code_only(text):
    """El codigo de un archivo C#, para comparar dos textos que solo deben diferir en comentarios: sin las lineas en blanco, sin las lineas que son
    solo un comentario (//, /// o un bloque /* ... */ entero en sus lineas) y sin el comentario // del final de una linea fuera de una cadena (la
    linea se corta en el primer // que no esta dentro de comillas). No interpreta cadenas verbatim de varias lineas: si una cambiara, el texto
    diferiria y la comparacion fallaria (nunca da por iguales dos codigos distintos por eso)."""
    out, blk = [], False
    for l in text.split('\n'):
        s = l.strip()
        if blk:
            blk = '*/' not in s
            continue
        if s.startswith('/*'):
            blk = '*/' not in s
            continue
        q, k = False, None
        for i in range(len(l) - 1):
            if l[i] == '"' and (i == 0 or l[i - 1] != '\\'):
                q = not q
            elif not q and l[i:i + 2] == '//':
                k = i
                break
        c = (l[:k] if k is not None else l).rstrip()
        if c.strip():
            out.append(c)
    return '\n'.join(out)
RED_FINAL = dict(RED_TEXT)
RED_FINAL.update({p: '' for p in RED_GONE})
_sha = lambda t: hashlib.sha256(t.encode('utf-8')).hexdigest()


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


# ---------------------------------------------------------------- la lista de PB-T4d (6.4 y 6.4.2), por su sha256, fila a fila frente a la base y al RED
_lb, LIST_SRC = local_or_rev(LIST_NAME)
assert _lb is not None, ('la lista de PB-T4d no esta ni en la revision ni junto a --package-file', LIST_NAME)
assert hashlib.sha256(_lb).hexdigest() == LIST_SHA, ('la lista de PB-T4d no es la que fija la version del paquete (sha256)', LIST_SRC)
_ll = _lb.decode('utf-8').split('\n')
assert _ll[0].split('\t') == LIST_HEADER and _ll[-1] == '' and '\r' not in _lb.decode('utf-8'), ('la cabecera o los saltos de la lista', _ll[0][:80])
ROWS = [l.split('\t') for l in _ll[1:-1]]
assert all(len(r) == len(LIST_HEADER) for r in ROWS) and [int(r[0]) for r in ROWS] == list(range(1, len(ROWS) + 1)), 'las filas de la lista, numeradas'
_A_ES = [a for a, _ in ACTIONS]
assert {r[6] for r in ROWS} <= set(_A_ES) | {KEEP_ACTION}, ('una accion que C02 no nombra', sorted({r[6] for r in ROWS} - set(_A_ES) - {KEEP_ACTION}))
LIST_ROWS = len(ROWS)
LIST_TOTAL = sum(1 for r in ROWS if r[6] != KEEP_ACTION)  # las filas con accion
# 4.4: la tabla por grupo y accion, frente a la lista
_t = [k for k, l in enumerate(SEC44) if l.startswith('| Grupo | Acci')]
assert len(_t) == 1, '4.4: la tabla por grupo y accion'
BY_GA = {}
for l in SEC44[_t[0] + 2:]:
    m = re.match(r'^\| ([\w-]+) \| ([a-z ]+) \| (\d+) \|$', l)
    if not m:
        _tot = re.match(r'^\| \*\*Total\*\* \| \| \*\*(\d+)\*\* \((\d+) con acci\S+n\) \|$', l)
        assert _tot, ('4.4: la fila del total', l[:80])
        break
    BY_GA[(m.group(1), m.group(2))] = int(m.group(3))
_ga = {}
for r in ROWS:
    _ga[(r[1], r[6])] = _ga.get((r[1], r[6]), 0) + 1
assert _ga == BY_GA and int(_tot.group(1)) == LIST_ROWS and int(_tot.group(2)) == LIST_TOTAL, ('4.4 frente a la lista', sorted(set(_ga.items()) ^ set(BY_GA.items())))
BY_ACTION = {}
for r in ROWS:
    BY_ACTION[r[6]] = BY_ACTION.get(r[6], 0) + 1
# fila a fila: la prueba en su archivo de la base, en su rango y con el Skip de su columna; en el RED (en memoria), lo que dice su accion
LIST_FILES = sorted({r[2] for r in ROWS})
_tb = {p: tests_of(RED_BASE[p]) for p in LIST_FILES if p in RED_BASE}
_tr = {p: tests_of(RED_FINAL[p]) for p in LIST_FILES if p in RED_FINAL}
assert sorted(_tb) == LIST_FILES == sorted(_tr), ('cada archivo de la lista lo edita o lo borra el RED', sorted(set(LIST_FILES) - set(_tb)))
LIST_CHECK = {'prueba': 0, 'no-prueba': 0, 'skip-del-red': 0}
LIST_SKIPS = []
for r in ROWS:
    _n, _g, _p, _m, _lines, _sk, _act = r[:7]
    if _lines == '-':
        # una linea que el RED anade (desde la v5.4: la directiva de StaleBase, CA-T4d M-12 d): no esta en la base, esta una vez en el RED y se dice
        # en tramo-editado; solo una reescritura con registro
        _bl, _rl = RED_BASE[_p].split('\n'), RED_FINAL[_p].split('\n')
        assert _act == 'reescribir con registro' and _sk == '-' and _m not in _bl and _rl.count(_m) == 1 and r[11].startswith('anadida'), \
            ('una fila sin lineas en la base que no es una linea anadida por el RED', _n, _m)
        LIST_CHECK['no-prueba'] += 1
        continue
    _a, _b = (int(x) for x in _lines.split('-'))
    assert 1 <= _a <= _b <= RED_BASE[_p].count('\n') + 1, ('las lineas de una fila', _n, _lines)
    _blk = '\n'.join(RED_BASE[_p].split('\n')[_a - 1:_b])
    _t0 = [t for t in _tb[_p] if t['method'] == _m and _a <= t['line'] <= _b]
    _t1 = [t for t in _tr[_p] if t['method'] == _m and t['cls'] == (_t0[0]['cls'] if _t0 else None)]
    assert (_sk == '-') == (not _t0 or not _t0[0]['skip']) and _sk in ('-', 'PB-T4b', 'PB-TC'), ('el Skip de la columna frente a la base', _n, _sk)
    if _t0:
        assert len(_t0) == 1 and len([t for t in _tb[_p] if t['method'] == _m and t['cls'] == _t0[0]['cls']]) == 1, ('una prueba de la lista, una vez', _n, _m)
        LIST_CHECK['prueba'] += 1
        if _act == 'borrar con registro' or (_act == 'sustituir con registro' and _g != 'ejecucion'):
            assert not _t1, ('una prueba que se borra o se sustituye sigue en el RED', _n, _p, _m)
        elif _act == 'sustituir con registro':
            assert len(_t1) == 1 and _t1[0]['skip'] and not _t0[0]['skip'], ('una de las tres de ejecucion sin su Skip en el RED', _n, _m)
            LIST_CHECK['skip-del-red'] += 1
            LIST_SKIPS.append((_p, _m, _t0[0]['attr']))
        elif _act == KEEP_ACTION:
            assert _blk in RED_FINAL[_p] and len(_t1) == 1 and _t1[0]['skip'] == _t0[0]['skip'], ('una prueba que se queda cambia en el RED', _n, _m)
        else:
            assert len(_t1) == 1 and not _t1[0]['skip'] and not _t0[0]['skip'], ('una prueba que se reescribe o se acota, sin Skip en el RED', _n, _m)
    else:
        LIST_CHECK['no-prueba'] += 1
        if _act == KEEP_ACTION:
            assert _blk in RED_FINAL[_p], ('un ayudante que se queda cambia en el RED', _n, _m)
        else:
            # se borra, se sustituye o se reescribe (tambien los ayudantes editados sin fila propia, CA-T4d M-04): su tramo no queda literal
            assert _blk not in RED_FINAL[_p], ('un ayudante que se borra o se reescribe sigue literal en el RED', _n, _act, _m)
assert LIST_CHECK['skip-del-red'] == BY_GA.get(('ejecucion', 'sustituir con registro')), ('los Skip del RED son los de ejecucion', LIST_CHECK)
# ninguna prueba de los archivos de la lista desaparece o se salta en el RED fuera de la lista
_listed = {(r[2], r[3]) for r in ROWS}
for _p in LIST_FILES:
    _red = {(t['cls'], t['method']): t for t in _tr[_p]}
    for t in _tb[_p]:
        _x = _red.get((t['cls'], t['method']))
        if (_p, t['method']) not in _listed:
            assert _x is not None and _x['skip'] == t['skip'], ('una prueba fuera de la lista desaparece o cambia de Skip en el RED', _p, t['method'])


# ---------------------------------------------------------------- la precondicion 6 de PB-T4d (--p6-file en seco, con compile-check.md y required-pbt4d.tsv
# en su carpeta; al emitir, los de la revision, en F4B-DRAFTS/pbt4/ y cc4d/): se lee antes de decidir lo que depende de ella (la leccion de la BASE de PB-T4b)
P6, P6_SRC, P6_CC, P6_REQ = None, None, None, None
if P6_FILE:
    P6 = json.loads(open(P6_FILE, 'rb').read().decode('utf-8'))
    _d = os.path.dirname(P6_FILE)
    P6_SRC = '--p6-file ' + os.path.basename(P6_FILE)
    P6_CC = open(os.path.join(_d, P6_CHECK_NAME), 'rb').read() if os.path.isfile(os.path.join(_d, P6_CHECK_NAME)) else None
    P6_REQ = open(os.path.join(_d, P6_REQ_NAME), 'rb').read() if os.path.isfile(os.path.join(_d, P6_REQ_NAME)) else None
elif in_head(P6_REL):
    P6 = json.loads(show(P6_REL))
    P6_SRC = P6_REL + ' en ' + REV_SHA[:8]
    P6_CC = show_bytes(P6_CHECK_REL) if in_head(P6_CHECK_REL) else None
    P6_REQ = show_bytes(P6_REQ_REL) if in_head(P6_REQ_REL) else None
_p6row = [l for l in _s642.split('\n') if l.startswith('| **La precondici')]
assert len(_p6row) == 1 and P6_SCHEMA in _p6row[0] and P6_CHECK_SHA in _p6row[0], '6.4.2: la fila de la precondicion 6 (su esquema y compile-check.md)'
# las claves de primer nivel que declara 6.4.2: desde el esquema hasta AllAsExpected (lo que sigue describe subclaves, como Green.UI[n].Json)
_p6k = _p6row[0].split(P6_SCHEMA + '`', 1)[1].split('. sha256', 1)[0]
_p6k = _p6k[:_p6k.index('`AllAsExpected`') + len('`AllAsExpected`')] if '`AllAsExpected`' in _p6k else _p6k
P6_DECLARED = re.findall(r'`([A-Z][A-Za-z]+)`', _p6k)
CLASS_TABLES = {}
REQ_MEASURED = {}
if P6 is None:
    pending('la precondicion 6 de PB-T4d (%s: en seco con --p6-file, o en la revision)' % P6_REL_NAME)
else:
    assert P6.get('Schema') == P6_SCHEMA and P6.get('Revision') == PT4C_CLOSE, ('la precondicion 6: su forma o su revision', P6.get('Schema'), P6.get('Revision'))
    assert P6_CC is not None and hashlib.sha256(P6_CC).hexdigest() == P6_CHECK_SHA, ('la precondicion 6: compile-check.md no esta o no es el del paquete', P6_SRC)
    assert P6['List'] == {'File': LIST_NAME, 'Sha256': LIST_SHA, 'Rows': LIST_ROWS, 'WithAction': LIST_TOTAL, 'ByAction': BY_ACTION,
                          'ByGroupAndAction': dict(('%s | %s' % k, v) for k, v in BY_GA.items())}, ('la precondicion 6: la lista', P6['List'])
    assert all(P6['Pieces'].get(k) == v for k, v in PIECES.items()) and P6['Pieces'].get(LIST_NAME) == LIST_SHA, \
        ('la precondicion 6: una pieza no es la del paquete', [k for k, v in PIECES.items() if P6['Pieces'].get(k) != v])
    # RedFiles (decision de la sesion): cada archivo, el del RED de esta version aplicado en memoria, salvo los de Reapply.TestsTextChanged, que son
    # los del RED con que corrio la precondicion 6 (RED_P6) y dan el mismo codigo en las dos piezas, sin comentarios ni lineas en blanco
    # (code_only); y Reapply sin cambios de codigo en tests/ ni en src/. La razon: la precondicion 6 corrio con el texto de RED_P6
    _rp = P6['Reapply']
    _ttc = json.loads(_rp['TestsTextChanged'].replace("'", '"'))
    assert _rp['TestsCodeChanged'] == '[]' and _rp['SrcDifferences'] == 0 and all(v == {'rc': 0, 'errors': 0, 'csWarnings': 0} for v in _rp['Builds'].values()), \
        ('la precondicion 6: Reapply con cambios de codigo', _rp['TestsCodeChanged'], _rp['SrcDifferences'])
    _pb, _ = local_or_rev(RED_P6[0])
    assert _pb is not None and hashlib.sha256(_pb).hexdigest() == RED_P6[1], ('el RED de la precondicion 6 no esta o no es el suyo', RED_P6[0])
    _, RED6_TEXT, _, _ = apply_piece(_pb, ('EDIT', 'NEWFILE', 'DELETE'))
    assert sorted(P6['RedFiles']) == sorted(RED_TEXT) and set(_ttc) <= set(RED_TEXT), 'la precondicion 6: los archivos del RED no son los de la pieza'
    for p in RED_TEXT:
        _want = RED6_TEXT[p] if p in _ttc else RED_TEXT[p]
        assert P6['RedFiles'][p] == {'kind': 'nuevo' if p in RED_NEW else 'red', 'sha256': _sha(_want)}, ('la precondicion 6: un archivo del RED', p)
        if p in _ttc:
            assert code_only(RED6_TEXT[p]) == code_only(RED_TEXT[p]) and RED6_TEXT[p] != RED_TEXT[p], ('Reapply: el codigo cambia, no solo el texto', p)
    RED_TTC = _ttc
    # los archivos que borra el RED (bloques DELETE, desde la v5.3): RedDeleted, y lo previsto red-deleted-N (decision de la sesion)
    assert sorted(P6['RedDeleted']) == sorted(RED_GONE) and P6['Expected'].get('red-deleted-%d' % len(RED_GONE)) is True, \
        ('la precondicion 6: RedDeleted no son los bloques DELETE del RED', P6.get('RedDeleted'), RED_GONE)
    assert sorted(P6['GreenFiles']) == sorted(GREEN_TEXT) and all(P6['GreenFiles'][p] == {'kind': 'green', 'sha256': _sha(GREEN_TEXT[p])} for p in GREEN_TEXT) \
        and P6['GreenDeleted'] == GREEN_GONE, 'la precondicion 6: los archivos del GREEN no son los de la pieza aplicada'
    assert P6['CsWarnings'] == 0 and all(v == {'rc': 0, 'errors': 0, 'csWarnings': 0} for v in P6['Builds'].values()) and len(P6['Builds']) == 5, \
        ('la precondicion 6: las compilaciones', P6['Builds'])
    _ct01 = [m for m in re.findall(r'public void (CT_PBT4D_01_\w+)\(', RED_TEXT[TR + 'PushBackRetirePilotGuardTests.cs'])]
    assert len(_ct01) == 1 and P6['Red']['Core']['FailedTests'] == ['RackCad.Tests.PushBackRetirePilotGuardTests.' + _ct01[0]] and \
        P6['Green']['Core']['FailedTests'] == [] and P6['GreenWithCoverage']['Core']['Failed'] == 0, 'la precondicion 6: solo cae CT-PBT4D-01 en el RED'
    for _k in ('Red', 'Green'):
        assert len(P6[_k]['UI']) == 18 and all(u['Failed'] == 0 for u in P6[_k]['UI']), ('la precondicion 6: los 18 filtros de UI', _k)
    assert P6['Coverage']['Verdict'] == 'PASA' and P6['Census']['Verdict'] == 'PASA', ('la precondicion 6: la cobertura o el censo', P6['Coverage'], P6['Census'])
    assert P6['Rehearsal'] == {'Red': {'CT-PBT4D-01': 'Failed', 'CT-PBT4D-02': 'Passed'}, 'Green': {'CT-PBT4D-01': 'Passed', 'CT-PBT4D-02': 'Passed'}}, \
        ('la precondicion 6: el ensayo del paso 8', P6['Rehearsal'])
    assert P6['Control']['base-cts']['Outcomes'] == "{'Failed': 2}" and P6['Control']['red-A']['Outcomes'] == "{'Failed': 1, 'Passed': 1}", \
        ('la precondicion 6: el control del ensayo (la base con las dos CT: caen las dos; el RED con A: cae CT-01)', P6['Control'])
    assert all(v is True for v in P6['Expected'].values()) and P6['AllAsExpected'] is True, ('la precondicion 6: lo previsto', P6['Expected'])
    # lo que el paquete cita literal de la corrida (6.4.1, "Lo medido en la v5.2"), frente al resumen
    _s641 = '\n'.join(PKG_LINES)
    for _need in ('%s s en el RED y %s s en el GREEN (%s s con la cobertura)' % (P6['SuiteDurationSeconds']['Red'], P6['SuiteDurationSeconds']['Green'],
                                                                            P6['SuiteDurationSeconds']['GreenWithCoverage']),
                  '%s s en el RED y %s s en el GREEN' % (P6['SlowTestSeconds']['Red'], P6['SlowTestSeconds']['Green']),
                  P6['Coverage']['Summary'].rsplit('; PASA', 1)[0] + '; **PASA**', P6['UiTotals']['red'] + ' y ' + P6['UiTotals']['green']):
        assert _need in _s641, ('el paquete no cita lo que da la precondicion 6', _need[:90])
    # la referencia de la cobertura (la base sin cambios, sola): Core es la base de la CI; la union de los filtros de UI, la que cita 6.4.1
    _bc, _bu = P6['BaseCoverage']['Core'], P6['BaseCoverage']['UI']
    assert (_bc['Total'], _bc['Passed'], _bc['Failed'], _bc['Skipped']) == (CI_BASE_T4C[2], CI_BASE_T4C[2] - CI_BASE_T4C[3], 0, CI_BASE_T4C[3]) and \
        ('%d = %d + %d + %d' % (_bc['Total'], _bc['Passed'], _bc['Failed'], _bc['Skipped'])) in _s641 and _bu['Failed'] == 0 and \
        ('%d = %d + %d + %d' % (_bu['Total'], _bu['Passed'], _bu['Failed'], _bu['Skipped'])) in _s641, ('la precondicion 6: la referencia de la cobertura', _bc, _bu)
    assert P6['Coverage']['Watched'] in _s641, ('el paquete no cita el conjunto vigilado de la v5.2', P6['Coverage']['Watched'][:80])
    _cov = os.path.join(os.path.dirname(P6_FILE), P6_COV_NAME) if P6_FILE else None
    _covb = open(_cov, 'rb').read() if _cov and os.path.isfile(_cov) else (show_bytes(D4 + P6_DIR + P6_COV_NAME) if in_head(D4 + P6_DIR + P6_COV_NAME) else None)
    assert _covb is not None and hashlib.sha256(_covb).hexdigest() == P6['Coverage']['Output'] and ('`%s`' % P6_COV_NAME) in _s641, \
        ('la precondicion 6: la salida de la cobertura no esta o no es la de Coverage.Output', P6_COV_NAME)
    # las tablas por clase de compile-check.md (bloques de texto), por el sha256 de ClassResults
    _cc = P6_CC.decode('utf-8')
    assert sorted(P6['ClassResults']) == ['green', 'red'], ('la precondicion 6: ClassResults', sorted(P6['ClassResults']))
    for _k in ('red', 'green'):
        _m = re.search(r'(?m)^## El resultado de cada clase que corre, %s [^\n]*\n\n```text\n(.*?)```' % _k.upper(), _cc, re.S)
        assert _m and hashlib.sha256(_m.group(1).encode('utf-8')).hexdigest() == P6['ClassResults'][_k], ('la tabla por clase de compile-check.md', _k)
        _rows = [l for l in _m.group(1).split('\n')[2:] if l.strip()]
        CLASS_TABLES[_k] = {}
        for l in _rows:
            _c = re.match(r'^\| `([\w.-]+\.trx)` \| `([\w.]+)` \| (\d+) \| (\d+) \| (\d+) \|$', l)
            assert _c, ('una fila de la tabla por clase', _k, l[:80])
            CLASS_TABLES[_k][(_c.group(1), _c.group(2))] = tuple(int(_c.group(j)) for j in (3, 4, 5))
    # los RequiredTests medidos (required-pbt4d.tsv, por el sha256 de RequiredTests.Tsv)
    assert P6_REQ is not None and hashlib.sha256(P6_REQ).hexdigest() == P6['RequiredTests']['Tsv'], ('la precondicion 6: required-pbt4d.tsv', P6_SRC)
    _rq = P6_REQ.decode('utf-8').split('\n')
    assert _rq[0].split('\t') == ['n', 'Project', 'Filter', 'MinSelected', 'ExpectRed', 'origen', 'MinSelected-PBT4c', 'base', 'medido-RED', 'medido-GREEN'], \
        ('la cabecera de required-pbt4d.tsv', _rq[0][:80])
    for l in [x for x in _rq[1:] if x.strip()]:
        _c = l.split('\t')
        REQ_MEASURED[int(_c[0])] = {'Project': _c[1], 'Filter': _c[2], 'MinSelected': int(_c[3]), 'ExpectRed': _c[4] == 'true', 'origen': _c[5],
                                    'base': None if _c[7] == '-' else int(_c[7]), 'red': int(_c[8]), 'green': int(_c[9])}
    assert P6['RequiredTests']['Overlap'] == 'solapamientos: 0 []', ('la precondicion 6: los filtros se solapan', P6['RequiredTests']['Overlap'])
    # las claves del resumen: las de 6.4.2 (y Schema); Package y Task, leidas (decision de la sesion); una que el paquete no declara es PENDIENTE (la
    # sesion decide), salvo las de P6_UNREAD
    assert P6.get('Package') == PKG_VERSION and P6.get('Task') == TASK, ('la precondicion 6: su version del paquete o su TaskId', P6.get('Package'), P6.get('Task'))
    P6_EXTRA = sorted(set(P6) - set(P6_DECLARED) - {'Schema'} - set(P6_READ_EXTRA))
    _p6undecl = [k for k in P6_EXTRA if k not in P6_UNREAD]
    if _p6undecl:
        pending('la precondicion 6 trae claves que 6.4.2 no declara: %s; la sesion decide' % ', '.join(_p6undecl))
    assert not set(P6_DECLARED) - set(P6), ('la precondicion 6 sin claves que declara 6.4.2', sorted(set(P6_DECLARED) - set(P6)))


# ================================================================ el contrato de PB-T4d desde 6.4, 6.4.1 y 6.4.2 de la v5.2
_b = [k for k, l in enumerate(PKG_LINES) if l.startswith('#### 6.4.1 ')]
assert len(_b) == 1, 'section 6.4.1'
_a = next(k for k in range(_b[0], len(PKG_LINES)) if PKG_LINES[k].strip() == '```text')
_z = next(k for k in range(_a + 1, len(PKG_LINES)) if PKG_LINES[k].strip() == '```')
BLOCK = PKG_LINES[_a + 1:_z]
assert all(ord(ch) < 0x80 for l in BLOCK for ch in l), 'el bloque de 6.4.1 no es ASCII'
_s64 = '\n'.join(SEC64)


def field(name):
    i = BLOCK.index(name + ':')
    out = []
    for l in BLOCK[i + 1:]:
        if not l.strip() or re.match(r'^[A-Za-z]+(?: \(propias\))?:$', l):
            break
        out.append(l)
    return out


assert BLOCK[0] == 'TaskId: ' + TASK, ('el TaskId de 6.4.1', BLOCK[0])
_ob = field('Objective')
assert len(_ob) == 1, 'el Objective de 6.4.1 en una linea'
OBJECTIVE = _ob[0]
for _need in ('paquete del contrato de PB-T4 ' + PKG_VERSION, 'CT-PBT4D-01 y 02', LIST_NAME, 'D-S11', 'D-S15', 'no se ejecuta AutoCAD'):
    assert OBJECTIVE.count(_need) == 1, ('el Objective de PB-T4d sin', _need)
INV = field('Invariants')
assert [i.split('. ')[0] for i in INV] == ['INV-PBT4D-%02d' % n for n in range(1, 15)], [i[:12] for i in INV]
assert ('%d filas con accion' % LIST_TOTAL) in INV[6] and LIST_SHA in INV[6] and LIST_NAME in INV[6], 'INV-PBT4D-07 sin la lista de PB-T4d y su sha256'
assert PIECES[RED_PIECE] in INV[1] and PIECES[GREEN_PIECE] in INV[1] and ('dentro de las %d rutas' % N_ALLOWED) in INV[0], 'INV-PBT4D-01 y 02'
STOP_OWN_TEXT = field('StopConditions (propias)')
_sid = [s.split(': ', 1)[0] for s in STOP_OWN_TEXT]
assert _sid == ['PBT4D-C%02d' % n for n in list(range(1, 15)) + [16, 17, 18]], _sid
STOP_OWN = [{'Id': s.split(': ', 1)[0], 'Text': s.split(': ', 1)[1]} for s in STOP_OWN_TEXT]
_so = dict((s['Id'], s['Text']) for s in STOP_OWN)
assert _so['PBT4D-C11'] == C11, 'PBT4D-C11 del paquete no es PBT4C-C11 con PB-T4c -> PB-T4d'
assert 'IMPORTANT LIMIT' in _so['PBT4D-C17'], 'C17'
# C02: la unica excepcion es la lista cerrada de PB-T4d, con su sha256, sus acciones (las de la lista, en ingles) y la pieza del RED, y nada mas
C02_EXC = ('No exception outside the closed list of PB-T4d: the %d rows with an action of %s (sha256 %s), each as its action says (%s), with the literal text '
           'of the RED piece %s (sha256 %s), and nothing else in tests/.' % (LIST_TOTAL, LIST_NAME, LIST_SHA, ', '.join(e for _, e in ACTIONS), RED_PIECE,
                                                                               PIECES[RED_PIECE]))
_c02 = _so['PBT4D-C02']
assert _c02.count(C02_EXC) == 1 and _c02.count('exception') == 1 and len(re.findall(r'[0-9a-f]{64}', _c02)) == 2 and len(re.findall(r'\.tsv\b', _c02)) == 1, \
    ('C02: la excepcion no es exactamente la lista de PB-T4d', _c02[:200])
assert {a for a, _ in ACTIONS} == set(BY_ACTION) - {KEEP_ACTION}, ('C02 nombra acciones que la lista no tiene, o al reves', sorted(BY_ACTION))
assert ('the %d rows with an action of %s' % (LIST_TOTAL, LIST_NAME)) in _so['PBT4D-C16'], 'C16 sin la lista de PB-T4d'
STOPS = STD_STOPS + STOP_OWN
assert len(STOPS) == 34, len(STOPS)
EVID = field('ExpectedEvidence')
assert [e.split('] ')[0] + ']' for e in EVID] == ['[%d]' % n for n in range(16)], [e[:4] for e in EVID]
EVID = [e.split('] ', 1)[1] for e in EVID]
assert EVID[0] == EVID0, 'la primera evidencia no es la del contrato de PB-T4c (la de PB-T3c)'
EVID13_OWNERS = [o for o in OWNER_D if o in EVID[13]]
EVID13_DS = re.findall(r'D-S11 a D-S1\d', EVID[13])
assert '"## PB-T4d"' in EVID[13] and LIST_NAME in EVID[13] and len(EVID13_DS) == 1, ('[13], el registro declarado', EVID13_DS)
assert all(o in EVID[13] for o in OWNER_D), ('[13] sin una tanda del Owner de PB-T4d', [o for o in OWNER_D if o not in EVID[13]])
_mods = dict((p, [n for n in PIECE_NAMES if n.startswith(p)][0][:-3]) for p in ('red_pbt4d_', 'apply_red_pbt4d_', 'green_pbt4d_', 'apply_green_pbt4d_'))
for _need in (_mods['red_pbt4d_'] + '.red', _mods['apply_red_pbt4d_'] + '.py', _mods['green_pbt4d_'] + '.green', _mods['apply_green_pbt4d_'] + '.py'):
    assert _need in EVID[15], ('[15] sin', _need)
_e14 = EVID[14]
_sp = lambda v: format(v, ',').replace(',', ' ')
for _need in ('run %s sobre su GREEN %s' % (CI_BASE_T4C[0], CI_BASE_T4C[1]), 'Core %s resultados, con %s Passed y %d Skipped' % (_sp(CI_BASE_T4C[2]),
              _sp(CI_BASE_T4C[2] - CI_BASE_T4C[3]), CI_BASE_T4C[3]), 'UI %s, con %s Passed y %d Skipped' % (_sp(CI_BASE_T4C[4]),
              _sp(CI_BASE_T4C[4] - CI_BASE_T4C[5]), CI_BASE_T4C[5]), '%d CT de Core, sin ninguna CT de UI' % N_CT_CORE):
    assert _need in _e14, ('[14] sin', _need)

# ---------------------------------------------------------------- el alcance de 6.4 (37), ruta a ruta, por grupo, frente a las dos piezas
_al = [k for k, l in enumerate(SEC64) if l.startswith('- **`AllowedWriteScope`** (')]
assert len(_al) == 1 and int(re.search(r'\((\d+)\)', SEC64[_al[0]]).group(1)) == N_ALLOWED
ALLOWED, _nums, SCOPE = [], [], {}
for l in SEC64[_al[0] + 1:]:
    if not l.startswith('  - '):
        break
    _lab = re.match(r'  - \*\*(.+?)\*\*', l).group(1)
    _ps = re.findall(r'(\d+)\. `((?:src|tests)/[^`]+)`', l)
    if not _ps:
        assert _lab.startswith('ning'), ('un grupo del alcance sin rutas', _lab)
        continue
    _k = sum(int(x) for x in re.findall(r'\((?:UNIT_CHANGE, )?(\d+)[),]', _lab))  # 'edita (23) o borra (3, ...)': los dos
    assert len(_ps) == _k, ('el numero de un grupo del alcance', _lab)
    SCOPE[_lab] = [p for _, p in _ps]
    _nums += [int(n) for n, _ in _ps]
    ALLOWED += [p for _, p in _ps]
assert _nums == list(range(1, N_ALLOWED + 1)) and len(ALLOWED) == len(set(ALLOWED)) == N_ALLOWED, ('el AllowedWriteScope de 6.4', _nums)
_g = lambda pre: [v for k, v in SCOPE.items() if k.startswith(pre)]
assert len(SCOPE) == 4 and all(len(_g(p)) == 1 for p in ('src/, existentes que cambian', 'src/, existentes que se borran', 'tests/, existentes que el RED',
                                                           'tests/, nuevos')), ('los cuatro grupos del alcance', list(SCOPE))
SRC_CHANGE, SRC_DELETE = _g('src/, existentes que cambian')[0], _g('src/, existentes que se borran')[0]
TESTS_EDIT, TESTS_NEW = _g('tests/, existentes que el RED')[0], _g('tests/, nuevos')[0]
UNIT_CHANGE_P = SRC_CHANGE + SRC_DELETE
assert '- **`UNIT_CHANGE`:** las %d de `src/` (%d que cambian y %d que se borran; F4B-E25).' % (len(UNIT_CHANGE_P), len(SRC_CHANGE), len(SRC_DELETE)) in SEC64
assert SRC_CHANGE == list(GREEN_TEXT) and SRC_DELETE == GREEN_GONE, ('el alcance de src/ no es el de la pieza del GREEN', list(GREEN_TEXT), GREEN_GONE)
assert sorted(TESTS_EDIT) == sorted(RED_EDITED + RED_GONE) == LIST_FILES and sorted(TESTS_NEW) == sorted(RED_NEW), \
    'el alcance de tests/ no es el de la pieza del RED (editados y borrados) y la lista'
_lab_t = [k for k in SCOPE if k.startswith('tests/, existentes que el RED')][0]
assert re.search(r'edita \(%d\) o borra \(%d, bloques DELETE' % (len(RED_EDITED), len(RED_GONE)), _lab_t), ('el alcance: los que el RED edita y borra', _lab_t)
assert all(a.startswith(PILOT_SRC) for a in UNIT_CHANGE_P) and all(in_head(a) for a in UNIT_CHANGE_P + TESTS_EDIT) and not any(in_head(a) for a in TESTS_NEW), \
    'src/ del alcance solo en Pilot/; las existentes existen y las nuevas no'
# en una retirada, las rutas de src/ que se borran existen en la base y no despues: el GREEN las borra (DELETE), y ningun archivo de src/ de la base fuera
# del alcance cambia (el GREEN solo toca estas seis)
# los archivos nuevos, literales (6.4.1 y el anexo V52-A): su NewFileSha y su ExpectRed
NEWFILE_SHA, NEWFILE_ER = {}, {}
for _p, _h, _rest in re.findall(r'(?m)^- `(tests/[^`]+\.cs)`, `([0-9a-f]{64})`: (.*)$', _s64):
    NEWFILE_SHA[_p] = _h
    NEWFILE_ER[_p] = 'ExpectRed` true' in _rest
assert sorted(NEWFILE_SHA) == sorted(TESTS_NEW), ('6.4.1: los archivos nuevos', sorted(NEWFILE_SHA))
_ann = dict(re.findall(r'(?m)^### `(tests/[^`]+\.cs)` \(`NewFileSha` `([0-9a-f]{64})`\)$', '\n'.join(PKG_LINES)))
assert _ann == NEWFILE_SHA and all(_sha(RED_TEXT[p]) == h for p, h in NEWFILE_SHA.items()), 'NewFileSha: 6.4.1, el anexo V52-A y la pieza del RED'
assert sum(NEWFILE_ER.values()) == 1 and NEWFILE_ER[TR + 'PushBackRetirePilotGuardTests.cs'], ('ExpectRed true, solo CT-PBT4D-01', NEWFILE_ER)
# los Skip literales (6.4.1), en la linea de atributo de su prueba en la base, frente al RED y a las tres de ejecucion de la lista
SKIPS = re.findall(r'(?m)^- `(tests/[^`]+\.cs)`:(\d+) \(`(\w+)`\): `(\[Fact\(Skip = "[^"]+"\)\])`$', _s64)
assert len(SKIPS) == LIST_CHECK['skip-del-red'] and sorted((p, m, int(n)) for p, n, m, _ in SKIPS) == sorted(LIST_SKIPS), ('los Skip de 6.4.1 y la lista', SKIPS)
for _p, _ln, _m, _lit in SKIPS:
    assert RED_BASE[_p].split('\n')[int(_ln) - 1].strip() == '[Fact]', ('la linea de atributo de un Skip en la base', _p, _ln)
    _t = [t for t in tests_of(RED_TEXT[_p]) if t['method'] == _m]
    assert len(_t) == 1 and RED_TEXT[_p].split('\n')[_t[0]['attr'] - 1].strip() == _lit and all(ord(ch) < 0x80 for ch in _lit), ('un Skip del RED', _p, _m)
assert ('pone %d Skip en Core' % len(SKIPS)) in _e14 and all(p.startswith(TR) for p, _, _, _ in SKIPS), '[14]: los Skip de Core'

# ---------------------------------------------------------------- el ForbiddenWriteScope: [PKC-C] en src/ y el precedente de tests/ (la lista del contrato de
# PB-T4c, con las rutas de tests/ de su alcance que dice FORB_PREV), menos este alcance; un prefijo que cubre una ruta del alcance se parte
_s63 = section('### 6.3 PB-T4c')
_prev = {}
for l in _s63:
    _m = re.match(r'  - \*\*(las clases de las CT|los equivalentes|solo para los Skip) \(\d+\)[^*]*\*\*(.*)$', l)
    if _m:
        _prev[_m.group(1)] = re.findall(r'\d+\. `(tests/[^`]+)`', _m.group(2))
assert sorted(sum(_prev.values(), [])) == sorted(a for a in pbt4c['AllowedWriteScope'] if a.startswith('tests/')), 'las rutas de tests/ del alcance de PB-T4c'
assert FORB_PREV in ('literal', 'precedente'), FORB_PREV
PREV_TESTS = (_prev['las clases de las CT'] + _prev['los equivalentes']) if FORB_PREV == 'literal' else \
    [a for a in pbt4c['AllowedWriteScope'] if a.startswith('tests/')]
SRCF = cover('src/')
TESTF, SPLIT2 = [], []
for f in [f for f in pbt4c['ForbiddenWriteScope'] if f.startswith('tests/')] + PREV_TESTS:
    if f in ALLOWED or f in TESTF:
        continue
    if f.endswith('/') and any(a.startswith(f) for a in ALLOWED):
        SPLIT2.append(f)
        TESTF += [x for x in cover(f) if x not in ALLOWED and x not in TESTF]
    else:
        TESTF.append(f)
ROOT = [f for f in pbt4c['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == len(set(FORBIDDEN)) and not set(FORBIDDEN) & set(ALLOWED), 'el ForbiddenWriteScope: sin repetidos ni rutas del alcance'
assert not any(f.startswith(PILOT_SRC) and f != PILOT_SRC and a.startswith(f) for f in SRCF for a in UNIT_CHANGE_P), 'Pilot/ partido en src/'
# lo que separa las dos lecturas (las rutas de tests/ de los Skip de PB-T4c fuera de este alcance): lo decide la sesion
FORB_DIFF = sorted(set(_prev['solo para los Skip']) - set(ALLOWED))


# ---------------------------------------------------------------- el recuento estatico del RED (el criterio de count_tests_git.py sobre los objetos de la base,
# con los archivos del RED en memoria): por clase, lo ejecutable, lo saltado y las Theory que el texto no cuenta (MemberData, o saltadas con varias filas)
def per_class(text):
    out = {}
    for t in tests_of(text):
        e = out.setdefault(t['cls'], [0, 0, 0])
        if t['n'] < 0 or (t['skip'] and t['n'] > 1):
            e[2] += 1  # no estatica: la mide la precondicion 6
        if t['n'] >= 0:
            e[1 if t['skip'] else 0] += t['n']
    return out


STATIC, STATIC_BASE = {}, {}
for _proj in ('UI', 'Core'):
    _nsp, _totals, _texts = count_static(_proj)
    _root = UT if _proj == 'UI' else TR
    _red = dict(_texts)
    _red.update({p: t for p, t in RED_TEXT.items() if p.startswith(_root)})
    for _p in RED_GONE:
        _red.pop(_p, None)
    for _dst, _src in ((STATIC_BASE, _texts), (STATIC, _red)):
        _st = {}
        for _txt in _src.values():
            for _k, _v in per_class(_txt).items():
                _e = _st.setdefault(_k, [0, 0, 0])
                for _j in range(3):
                    _e[_j] += _v[_j]
        _dst[_proj] = _st
    # el mismo criterio que count_static en la base: lo ejecutable y las MemberData por clase
    assert all(STATIC_BASE[_proj].get(k, [0])[0] == v[0] for k, v in _totals.items()), ('per_class no es el criterio de count_tests_git.py', _proj)
DSTAT = {}
for _proj in ('Core', 'UI'):
    _root = TR if _proj == 'Core' else UT
    _b = [per_class(RED_BASE[p]) for p in RED_BASE if p.startswith(_root)]
    _r = [per_class(RED_FINAL[p]) for p in RED_FINAL if p.startswith(_root)]
    _sum = lambda ds, i: sum(v[i] for d in ds for v in d.values())
    DSTAT[_proj] = (_sum(_r, 0) - _sum(_b, 0), _sum(_r, 1) - _sum(_b, 1), _sum(_b, 2), _sum(_r, 2))

# ---------------------------------------------------------------- los RequiredTests de 6.4.1 (88): los heredados con su numero de PB-T4c, su MinSelected medido
# sobre el GREEN (required-pbt4d.tsv de la precondicion 6) y su recuento estatico, los que quedan en 0 fuera, y los nuevos por clase exacta (D-S12)
_t = [k for k, l in enumerate(PKG_LINES) if l.startswith('| n.') and '| Project | Filter | MinSelected | ExpectRed | Origen |' in l and k > _z]
assert len(_t) == 1, '6.4.1: la tabla de RequiredTests'
REQ_PKG = []
for l in PKG_LINES[_t[0] + 2:]:
    m = re.match(r'^\| (\d+) \| `([^`]+)` \| `([^`]+)` \| (\d+) \| (true|false) \| (.+) \|$', l)
    if not m:
        break
    _o = re.match(r'PB-T4c n\.\S (\d+)$', m.group(6))
    REQ_PKG.append((int(m.group(1)), m.group(2), m.group(3), int(m.group(4)), m.group(5) == 'true', int(_o.group(1)) if _o else None, m.group(6)))
assert len(REQ_PKG) == N_REQ_ROWS and [r[0] for r in REQ_PKG] == list(range(1, N_REQ_ROWS + 1)), ('la tabla de RequiredTests de 6.4.1', len(REQ_PKG))
_inh = [r for r in REQ_PKG if r[5] is not None]
_new = [r for r in REQ_PKG if r[5] is None]
assert len(_inh) == N_REQ_INH and [r[5] for r in _inh] == sorted(r[5] for r in _inh) and _inh == REQ_PKG[:N_REQ_INH], 'los heredados, en el orden de PB-T4c'
# el numero de PB-T4c es el de su tabla de 6.3.1 (1 a 86, sin el 43), que es su contrato emitido en su orden
_t3 = [k for k, l in enumerate(PKG_LINES) if l.startswith('| # | Project | Filter | MinSelected | ExpectRed |')]
_b3 = [k for k, l in enumerate(PKG_LINES) if l.startswith('#### 6.3.1 ')][0]
_b4 = [k for k, l in enumerate(PKG_LINES) if l.startswith('#### 6.4.1 ')][0]
_t3 = [k for k in _t3 if _b3 < k < _b4]
assert len(_t3) == 1, '6.3.1: la tabla de RequiredTests de PB-T4c'
_T4C_NUM = []
for l in PKG_LINES[_t3[0] + 2:]:
    m = re.match(r'^\| (\d+) \| `([^`]+)` \| `([^`]+)` \| (?:\*\*)?\d+(?:\*\*)? \| (true|false) \|$', l)
    if not m:
        break
    _T4C_NUM.append((int(m.group(1)), m.group(2), m.group(3), m.group(4) == 'true'))
assert [(p, f, e) for _, p, f, e in _T4C_NUM] == [(r['Project'], r['Filter'], r['ExpectRed']) for r in pbt4c['RequiredTests']], \
    '6.3.1: la tabla de PB-T4c no es su contrato emitido, en su orden'
_T4C_REQ = dict((n, r) for (n, _, _, _), r in zip(_T4C_NUM, pbt4c['RequiredTests']))
REQ_ZERO_PKG = dict((int(n), c) for n, c in re.findall(r'el n\.\S (\d+) \(`([\w.]+)`, de \d+ a 0', _s64))
assert sorted(REQ_ZERO_PKG) == sorted(set(_T4C_REQ) - {r[5] for r in _inh}), ('los n.o de PB-T4c que salen', REQ_ZERO_PKG)
NEW_CLASSES = ['RackCad.Tests.' + os.path.basename(p)[:-3] for p in TESTS_NEW]


def static_sel(proj, filt, base=False):
    _f = filt.split('~', 1)[1]
    sel = {k: v for k, v in (STATIC_BASE if base else STATIC)['UI' if proj == UIP else 'Core'].items() if k.startswith(_f)}
    if not sel:
        # como en PB-T4b y PB-T4c: count_static atribuye las pruebas de NumericFieldTests a su clase anidada BorderBrushSource, que va antes de ellas; se
        # cuenta por el archivo que declara la clase
        _cn = _f.rsplit('.', 1)[1]
        _fs = [p for p in tracked(UT if proj == UIP else TR) if p.endswith('.cs') and re.search(r'\bclass\s+%s\b' % _cn, show(p))]
        if len(_fs) == 1 and not base and _fs[0] in RED_FINAL:
            _ts = tests_of(RED_FINAL[_fs[0]])
        else:
            _ts = tests_of(show(_fs[0])) if len(_fs) == 1 else []
        return (sum(t['n'] for t in _ts if not t['skip'] and t['n'] > 0), sum(1 for t in _ts if t['n'] < 0 or (t['skip'] and t['n'] > 1)),
                ['<archivo> ' + p for p in _fs])
    return sum(v[0] for v in sel.values()), sum(v[2] for v in sel.values()), sorted(sel)


# el MinSelected de un heredado es un suelo (required_pbt4d_v52.py): el de PB-T4c menos lo que el filtro pierde de la base al GREEN, medido en las TRX de
# la precondicion 6; lo estatico da lo mismo donde el texto lo cuenta todo
REQ_REPORT, REQ_STATIC = [], [0, 0]
for n, proj, filt, m, er, k, _orig in _inh:
    _r = _T4C_REQ[k]
    # todos con ExpectRed false (6.4.1), tambien los de las CT de PB-T4c, que en PB-T4c eran true
    assert (_r['Project'], _r['Filter']) == (proj, filt) and not er, ('un heredado no es el de PB-T4c con su numero', n, k, filt)
    _run, _md, _cls = static_sel(proj, filt)
    _run0, _md0, _ = static_sel(proj, filt, base=True)
    assert not any(c.startswith(filt.split('~', 1)[1]) for c in NEW_CLASSES), ('un filtro heredado selecciona una clase nueva', filt)
    if _md == 0 and _md0 == 0:
        assert m == _r['MinSelected'] - (_run0 - _run), ('el suelo de un filtro heredado frente a lo estatico', n, filt, m, _run0, _run)
        REQ_STATIC[0] += 1
    else:
        REQ_STATIC[1] += 1
    if P6 is not None:
        _q = REQ_MEASURED[n]
        assert (_q['Project'], _q['Filter'], _q['MinSelected'], _q['ExpectRed'], _q['origen'], _q['red']) == (proj, filt, m, er, 'PB-T4c n.o %d' % k, _q['green']), \
            ('la fila %d frente a lo medido' % n, _q, m)
        assert m == _r['MinSelected'] - (_q['base'] - _q['green']) > 0 and _q['green'] > 0, ('el suelo de la fila %d no es el medido' % n, _q, m)
        assert (_md > 0 or _md0 > 0) or (_q['base'], _q['green']) == (_run0, _run), ('lo estatico de un filtro heredado no es lo medido', n, filt, _run0, _run, _q)
    if m != _r['MinSelected']:
        REQ_REPORT.append('n.o %2d (PB-T4c %2d) %-4s %-60s MinSelected %4d -> %-4d (medido: base %s, GREEN %s; estatico %d -> %d, Theory sin contar %d -> %d)'
                          % (n, k, 'UI' if proj == UIP else 'Core', filt.split('.', 2)[-1], _r['MinSelected'], m,
                             REQ_MEASURED[n]['base'] if P6 is not None else '-', REQ_MEASURED[n]['green'] if P6 is not None else '-', _run0, _run, _md0, _md))
for k, cls in REQ_ZERO_PKG.items():
    _r = _T4C_REQ[k]
    _run, _md, _cls = static_sel(_r['Project'], _r['Filter'])
    assert _r['Filter'] == 'FullyQualifiedName~' + cls and _run == 0 and _md == 0, ('un filtro que sale no queda en 0 en el RED', k, cls, _run, _md)
for k in sorted(_T4C_REQ):
    if k not in REQ_ZERO_PKG and k not in {r[5] for r in _inh}:
        raise AssertionError(('un filtro de PB-T4c ni heredado ni fuera', k))
_zero = [k for k in sorted(_T4C_REQ) if static_sel(_T4C_REQ[k]['Project'], _T4C_REQ[k]['Filter'])[:2] == (0, 0)]
assert _zero == sorted(REQ_ZERO_PKG), ('los filtros de PB-T4c que el RED deja en 0 no son los que salen', _zero)
assert [r[2] for r in _new] == ['FullyQualifiedName~' + c for c in sorted(NEW_CLASSES, key=lambda c: [r[2] for r in _new].index('FullyQualifiedName~' + c))], \
    'los nuevos, por clase exacta'
assert sorted(r[2].split('~', 1)[1] for r in _new) == sorted(NEW_CLASSES), ('los filtros nuevos no son las clases nuevas', [r[2] for r in _new])
for n, proj, filt, m, er, k, _orig in _new:
    _p = TR + filt.split('.')[-1] + '.cs'
    _c = per_class(RED_TEXT[_p])
    assert proj == CP and list(_c) == [filt.split('~', 1)[1]] and _c[filt.split('~', 1)[1]] == [m, 0, 0] and er == NEWFILE_ER[_p], ('una fila nueva', n, filt, _c)
    if P6 is not None:
        _q = REQ_MEASURED[n]
        assert (_q['Project'], _q['Filter'], _q['MinSelected'], _q['ExpectRed'], _q['red'], _q['green']) == (proj, filt, m, er, m, m), ('una fila nueva medida', n)
assert P6 is None or sorted(REQ_MEASURED) == list(range(1, N_REQ_ROWS + 1)), 'required-pbt4d.tsv: las 88 filas'
REQ = [(p, f, m, e) for _, p, f, m, e, _, _ in REQ_PKG]
_er = [r for r in REQ if r[3]]
assert len(_er) == 1 and _er[0][0] == CP and _er[0][2] == RED_FAIL_CORE and _er[0][1] == 'FullyQualifiedName~RackCad.Tests.PushBackRetirePilotGuardTests', \
    ('un solo filtro con ExpectRed, el de CT-PBT4D-01', _er)

# ---------------------------------------------------------------- los totales de cada rama: la base de PB-T4c (F4B-E55), lo que mide la precondicion 6 sobre el
# GREEN, y el RED desde el GREEN con los Failed de los filtros con ExpectRed; frente a CI_T4D_EXPECTED, a la precondicion 6 y a [14], por expresion
_ui_tot = lambda s, k: tuple(int(x) for x in re.fullmatch(r'UI %s: (\d+) = (\d+) \+ (\d+) \+ (\d+) \(Passed \+ Failed \+ Skipped\)' % k, s).groups())
if P6 is not None:
    _gc, _ut = P6['Green']['Core'], _ui_tot(P6['UiTotals']['green'], 'green')
    _GREEN = (_gc['Total'], _gc['Passed'], _gc['Failed'], _gc['Skipped'], _ut[0], _ut[1], _ut[2], _ut[3])
    CI_SRC = 'la precondicion 6'
else:
    _GREEN = CI_T4D_EXPECTED['GREEN']
    CI_SRC = 'CI_T4D_EXPECTED (sin la precondicion 6)'
_fc = sum(r[2] for r in REQ if r[3] and r[0] == CP)
_fu = sum(r[2] for r in REQ if r[3] and r[0] == UIP)
assert (_fc, _fu) == (RED_FAIL_CORE, RED_FAIL_UI), ('los Failed del RED: los MinSelected de los filtros con ExpectRed', _fc, _fu)
CI_T4D = {'GREEN': _GREEN, 'RED': (_GREEN[0], _GREEN[1] - _fc, _fc, _GREEN[3], _GREEN[4], _GREEN[5] - _fu, _fu, _GREEN[7])}
for _k, (_t, _p, _f, _sk, _ut, _up, _uf, _usk) in CI_T4D.items():
    assert _t == _p + _f + _sk and _ut == _up + _uf + _usk, ('la aritmetica de las ramas', _k)
assert CI_T4D['GREEN'][2] == 0 and CI_T4D['GREEN'][6] == 0, 'el GREEN sin Failed'
assert CI_T4D == CI_T4D_EXPECTED, ('los totales no son los de la tabla del generador (CI_T4D_EXPECTED)', CI_T4D, CI_T4D_EXPECTED)
if P6 is not None:
    _rc = P6['Red']['Core']
    assert (_rc['Total'], _rc['Passed'], _rc['Failed'], _rc['Skipped']) + _ui_tot(P6['UiTotals']['red'], 'red') == CI_T4D['RED'], ('la precondicion 6: el RED', _rc)
for _k, _lbl in (('RED', 'El RED: '), ('GREEN', 'El GREEN: ')):
    _t = CI_T4D[_k]
    _need = (re.escape('%sCore %d' % (_lbl, _t[0])) + '(?: resultados)?' + re.escape(', con %d Passed, %d Failed%s y %d Skipped; UI %d, con %d Passed, %d Failed y %d Skipped'
                                                                                      % (_t[1], _t[2], ' (CT-PBT4D-01)' if _t[2] else '', _t[3], _t[4], _t[5], _t[6], _t[7])))
    assert re.search(_need, _e14), ('[14] no da los totales', _k, _t)
    assert ('%d = %d + %d + %d' % (_t[0], _t[1], _t[2], _t[3])) in _s64, ('6.4.1, lo medido, sin el Core de la rama', _k)
# la diferencia con la base: lo estatico (count_tests_git.py) y lo que el texto no cuenta (MemberData y Theory saltadas), medido por la precondicion 6
DELTA = {'Core': (CI_T4D['GREEN'][0] - CI_BASE_T4C[2], CI_T4D['GREEN'][3] - CI_BASE_T4C[3]), 'UI': (CI_T4D['GREEN'][4] - CI_BASE_T4C[4], CI_T4D['GREEN'][7] - CI_BASE_T4C[5])}
# por clase, en los archivos del RED: lo estatico frente a la tabla del GREEN de la precondicion 6, donde el texto lo cuenta todo
CLASS_CHECK = [0, 0]
if P6 is not None:
    _gt = {}
    for (_trx, _cls), (_pp, _ff, _ss) in CLASS_TABLES['green'].items():
        assert _gt.get(_cls, (_pp, _ff, _ss)) == (_pp, _ff, _ss), ('una clase con dos resultados en la tabla del GREEN', _cls)
        _gt[_cls] = (_pp, _ff, _ss)
    for _p in RED_TEXT:
        for _cls, (_r, _s, _x) in per_class(RED_TEXT[_p]).items():
            _proj = 'UI' if _p.startswith(UT) else 'Core'
            _stc = STATIC[_proj].get(_cls, [0, 0, 0])
            if _stc[2] or _cls not in _gt:
                CLASS_CHECK[1] += 1
                assert _proj == 'UI' or _cls in _gt or _stc[:2] == [0, 0], ('una clase de Core del RED no esta en la tabla del GREEN', _cls)
                continue
            assert (_gt[_cls][0] + _gt[_cls][1], _gt[_cls][2]) == (_stc[0], _stc[1]), ('una clase del RED: lo estatico no es lo medido', _cls, _stc, _gt[_cls])
            CLASS_CHECK[0] += 1


# ---------------------------------------------------------------- las autoridades (6.4.2): las del contrato de PB-T4c, por su numero, con AUTH_PLAN (la
# propuesta, pendiente de la sesion: 6.4.2 no da los numeros)
_A = pbt4c['Authorities']
assert len(_A) == 70, ('las autoridades del contrato de PB-T4c', len(_A))
for _i, _end in ((0, PKG), (6, 'superseding-characterization.md'), (28, 'f4b-dispositions.md'), (29, 'F4B-PBT4-FULL-DRAFT-PANEL/coordinator/gate-contract.json'),
                 (36, 'F4B-PBT4-FULL-DRAFT-PANEL/task-close-package.md'), (37, T4C_TASK + '/coordinator/gate-contract.json'), (43, 'independent-review-v1.md'),
                 (47, 'owner-pbt2b-close-decisions.md'), (48, 'owner-pbt4b-code-decisions.md'), (49, 'pbt4c-substitution-list-4.tsv'),
                 (60, 'pbt4c-precondition6.json')):
    assert _A[_i]['Path'].endswith(_end), ('la autoridad n.o %d del contrato de PB-T4c no es la que se espera' % _i, _A[_i]['Path'])
assert _A[37]['Section'] == 'este contrato' and all(a['Class'] == 'UNIT_CHANGE' for a in _A[61:70]), 'la n.o 37 y los UNIT_CHANGE de PB-T4c (61 a 69)'
assert all(_A[i]['Path'].startswith(D4) and _A[i]['Class'] == 'UNIT_DOC' for i in range(49, 61)), 'las n.o 49 a 60: la lista, las piezas y la precondicion 6 de PB-T4c'
_auth = []
# las decisiones de la sesion que cita 6.4.2 ("Se anaden"); [13] puede citar otro tramo (se dice en la salida)
_sds = re.findall(r'`%s` \((D-S11 a D-S\d+)\)' % re.escape(SESSION_DEC_D), _s642)
assert len(_sds) == 1, ('6.4.2: el tramo de las decisiones de la sesion', _sds)
SESSION_DS = _sds[0]
# [13] cita el mismo tramo que 6.4.2 (decision de la sesion: la v5.4 lo corrige a D-S11 a D-S16)
assert EVID13_DS == [SESSION_DS], ('[13] y 6.4.2 citan tramos distintos de las decisiones de la sesion', EVID13_DS, SESSION_DS)
for _i, a in enumerate(_A):
    if _i in AUTH_PLAN['drop'] or (_i == 48 and not AUTH_PLAN['keep_48']):
        continue
    a = dict(a)
    if _i == 0:
        a['Section'] = ('archivo completo (' + PKG_VERSION + ', sha256 ' + CUSTODY_SHA256[PKG][:8] + '): D-PBT4-10, section 4.4, 5.6, 6.0 y 6.4 (el contrato de '
                        'PB-T4d, sus literales de 6.4.1 y lo que necesita el generador, 6.4.2) y el anexo V52-A')
    elif _i == 6:
        a['Section'] = 'la entrada "## PB-T4d" (la lista de %d con accion, por grupo y accion, con su tabla al cerrar; [13])' % LIST_TOTAL
    elif _i == 28:
        a['Section'] = 'hasta ' + T4C_CLOSE_ENTRY + ' (el cierre de PB-T4c); ' + EMIT_ENTRY + ', la emision de PB-T4d'
    elif _i == 29:
        a['Path'], a['Section'] = PBT4CC, 'el contrato anterior, el de PB-T4c (emitido; sus textos comunes, C11 y sus filtros)'
    elif _i == 36:
        a['Path'], a['Section'] = T4C_TCP, 'la base: el cierre de PB-T4c (' + T4C_CLOSE_ENTRY + '), con la CI de la linea base (' + CI_BASE_T4C[0] + ')'
    elif _i == 37:
        a['Path'], a['Section'] = PILOT + '/' + TASK + '/coordinator/gate-contract.json', 'este contrato'
    elif _i == AUTH_PLAN['ca_at']:
        a['Path'], a['Section'] = D4 + BOUNDED_NAME, ('la comprobacion acotada de PB-T4d (CA-T4d), sobre el contrato en seco y la ' + CAT4D_PKG +
                                                     ', y su recomprobacion sobre la ' + PKG_VERSION + ' (si la hay)')
    elif _i == 48:
        a['Section'] = 'F4B-E53, pregunta 2: los vacios de M-04 van a PB-T4d, que los reescribe en su sitio (D-S13)'
    _auth.append(a)
_have = {a['Path'] for a in _auth}
_OWNER_D_SEC = {'owner-pbt4d-list-decisions.md': 'la 10.a tanda: Alcance B, la recarga a una tarea propia despues, la prueba lenta acotada en PB-T4d y la emision '
                                                 'condicional',
                'owner-pbt4d-substitutes-decision.md': 'la 11.a tanda: Sustitutas donde hace falta',
                'owner-pbt4d-run-coverage-decision.md': 'la 12.a tanda: Sustituir las 21 (la corrida generica)',
                'owner-pbt4d-deadcode-27-decisions.md': 'la 13.a tanda: Quitarla en PB-T4d (la rama muerta de BuildCore) y Si, 27 (las sustitutas de la corrida)',
                'owner-pbt4d-cat4d-decisions.md': 'la 14.a tanda, sobre CA-T4d: Sustituirlas (las n.o 48 y 49 de WPUR) y Sustituirlos siempre (la '
                                                  'cobertura de todo src/ que se queda)'}
for _p, _s in ([(CO + _n, 'archivo completo (' + EMIT_ENTRY + '): ' + _OWNER_D_SEC[_n] + ' (decision del Owner, literal)') for _n in OWNER_D] +
               [(D4 + SESSION_DEC_D, 'archivo completo (' + EMIT_ENTRY + '): las decisiones de diseno de la sesion, ' + SESSION_DS),
                (D4 + LIST_NAME, 'la lista de PB-T4d (%d filas, %d con accion), sha256 %s' % (LIST_ROWS, LIST_TOTAL, LIST_SHA[:8]))] +
               [(D4 + _n, 'una pieza literal de PB-T4d (6.4.2), sha256 %s' % PIECES[_n][:8]) for _n in (RED_PIECE, GREEN_PIECE)] +
               [(P6_REL, 'la precondicion 6 de la sesion: la lista, las piezas, el RED y el GREEN, los totales, la cobertura, el censo y los RequiredTests'),
                (P6_CHECK_REL, 'el registro de la precondicion 6 (sha256 %s): las corridas y el resultado de cada clase en cada rama' % P6_CHECK_SHA[:8])]):
    if _p not in _have:
        _auth.append({'Path': _p, 'Section': _s, 'Class': 'UNIT_DOC'})
_auth += [{'Path': p, 'Section': 'el texto literal del GREEN de PB-T4d (' + GREEN_PIECE + '; D-PBT4-10): ' + ('se borra' if p in SRC_DELETE else 'cambia'),
           'Class': 'UNIT_CHANGE'} for p in UNIT_CHANGE_P]

# ---------------------------------------------------------------- el contrato de PB-T4d
c = {}
for k in pbt4c:
    c[k] = pbt4c[k]
c.update({'TaskId': TASK, 'Objective': OBJECTIVE, 'AuthorityRevision': AR, 'MainSha': MAIN, 'Authorities': _auth, 'AllowedWriteScope': ALLOWED,
          'ForbiddenWriteScope': FORBIDDEN, 'Invariants': INV,
          'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
          'ExpectedEvidence': EVID, 'StopConditions': STOPS,
          'IssuedBy': ('Sesion principal de I-64 (Claude), emision de la cuarta subtarea de PB-T4 (' + EMIT_ENTRY + ') por owner-pbt4d-list-decisions.md, pregunta 4 '
                       '(la emision condicional de PB-T4d), con la lista exacta ' + LIST_NAME + ' (' + str(LIST_TOTAL) + ' filas con accion; ' +
                       ', '.join(OWNER_D[:-1]) + ' y ' + OWNER_D[-1] + '): PB-T4d '
                       'tras el cierre de PB-T4c (' + PT4C_CLOSE[:8] + ', ' + T4C_CLOSE_ENTRY + '), con su comprobacion acotada (' +
                       (BOUNDED_RESULT or '<resultado por rellenar>') + '); las decisiones de diseno de la sesion (D-S11 a D-S15); F4B-E46; F4B-E40 y F4B-E43, '
                       'los sustitutos de nc1 y nc2; F4B-E29, D-1; F4B-E11, D2; F4B-E35, la regla RR; F4B-E21; F4B-E25, con las 6 de src/ (4 que cambian y 2 '
                       'que se borran); y el paquete del contrato de PB-T4 ' + PKG_VERSION),
          'IssuedUtc': ISSUED})
assert list(c) == list(pbt4c), 'las claves del contrato de PB-T4d no son las del de PB-T4c'
b = (json.dumps(c, indent=2, ensure_ascii=True) + '\n').encode('ascii')
assert all(x < 0x80 for x in b) and b'\\u' not in b, 'no ASCII'
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


# ---- bounded_check_t4d (inicio)
def bounded_check_t4d(report, result, pkg_lines, reviewer='CA-T4d'):
    """bounded_check (el de make_pbt3a_contract.py) adaptado a la forma del informe propio de CA-T4d, independent-review-pbt4d-check.md (decision de la
    sesion: sin aflojar lo que comprueba). Lo que cambia es solo la forma:
    - el veredicto de CA-T4d, antes de la primera recomprobacion: '**<veredicto>. B n / I n / M n.**' (con o sin 'Veredicto: ');
    - cada recomprobacion: '## Recomprobacion <n> (vX.Y)', con su veredicto en su seccion: '**Veredicto:** <veredicto>, B n / I n / M n';
    - BOUNDED_RESULT es, caracter a caracter, la linea BOUNDED del informe ('> EMITIBLE...') mas la cola de cada recomprobacion ('> ; recomprobacion
      vX.Y: <veredicto>; B n / I n / M n ...'), sin el '> ', en su orden: se copia del archivo, no se escribe a mano.
    Lo que comprueba, como bounded_check: solo la familia EMITIBLE; 0 bloqueantes; BOUNDED_RESULT empieza por el veredicto y lleva sus B/I/M; cada
    importante con su fila en los hallazgos y una fila del registro de cambios del paquete que la nombra (**CA-T4d I-nn**, con o sin dos puntos), y resuelta
    en la cola de una recomprobacion o 'corregidas en el paquete'; la ultima recomprobacion, EMITIBLE con 0 bloqueantes y 0 importantes, nombrada en
    BOUNDED_RESULT con su version y sus B/I/M. Devuelve los errores (vacia si vale)."""
    import re, unicodedata
    o = chr(0xF3)
    fold = lambda s: ''.join(c for c in unicodedata.normalize('NFKD', s) if not unicodedata.combining(c))
    result = result or ''
    ok_verdict = lambda v: v in ('EMITIBLE', 'EMITIBLE TRAS CORRECCIONES')
    lines = report.replace('\r\n', '\n').split('\n')
    heads = [k for k, l in enumerate(lines) if re.match(r'^## Recomprobaci' + o + r'n \d+ \(v\d+(?:\.\d+)?(?:, [^)]*)?\)\s*$', l)]
    if [k for k, l in enumerate(lines) if l.startswith('## ') and ('Recomprobaci' + o + 'n') in l] != heads:
        return ['un encabezado de recomprobacion no tiene la forma "## Recomprobacion <n> (vX.Y[, ...])"']
    first = '\n'.join(lines[:heads[0]] if heads else lines)
    m = re.search(r'(?m)^\*\*(?:Veredicto: )?(EMITIBLE(?: TRAS CORRECCIONES)?|[A-Z][A-Z ]+?)\. B (\d+) / I (\d+) / M (\d+)\.\*\*\s*$', first)
    if not m:
        return ['la comprobacion acotada no tiene su veredicto, en la forma "**<veredicto>. B n / I n / M n.**"']
    verdict, b, i, mm = fold(m.group(1)), int(m.group(2)), int(m.group(3)), int(m.group(4))
    errors = []
    if not ok_verdict(verdict):
        errors.append('el veredicto de la comprobacion acotada no es emitible: %s (solo EMITIBLE o EMITIBLE TRAS CORRECCIONES)' % ascii(verdict))
    if b:
        errors.append('la comprobacion acotada tiene %d bloqueantes' % b)
    if not result.startswith(verdict + ';') or ('B %d / I %d / M %d' % (b, i, mm)) not in result.split('; recomprobacion ')[0]:
        errors.append('BOUNDED_RESULT no es el veredicto de la comprobacion acotada: %s frente a %s' % (ascii(result[:80]), ascii(m.group(0))))
    # BOUNDED_RESULT, caracter a caracter: la linea BOUNDED y las colas de las recomprobaciones, en su orden
    bl = [l[2:] for l in lines if l.startswith('> EMITIBLE')]
    tails = [l[2:] for l in lines if l.startswith('> ; recomprobacion ')]
    if len(bl) != 1:
        errors.append('el informe no tiene una sola linea BOUNDED ("> EMITIBLE..."): %d' % len(bl))
    elif result != bl[0] + ''.join(tails):
        errors.append('BOUNDED_RESULT no es la linea BOUNDED del informe con las colas de sus recomprobaciones (%d), caracter a caracter' % len(tails))
    if len(tails) != len(heads):
        errors.append('el informe tiene %d recomprobaciones y %d colas "> ; recomprobacion ..."' % (len(heads), len(tails)))
    rechecks = []
    for n, k in enumerate(heads):
        ver = re.match(r'^## Recomprobaci' + o + r'n (\d+) \((v\d+(?:\.\d+)?)(?:, [^)]*)?\)', lines[k]).group(2)
        end = next((j for j in range(k + 1, len(lines)) if lines[j].startswith('## ')), len(lines))
        body = '\n'.join(lines[k + 1:end])
        vm = re.findall(r'(?m)^\*\*Veredicto:\*\* (EMITIBLE(?: TRAS CORRECCIONES)?|[A-Z][A-Z ]+?), B (\d+) / I (\d+) / M (\d+)\b', body)
        if len(vm) != 1:
            errors.append('la recomprobacion %d (%s) no tiene un solo veredicto "**Veredicto:** <veredicto>, B n / I n / M n" en su seccion' % (n + 1, ver))
            continue
        v, rb, ri, rm = fold(vm[0][0]), int(vm[0][1]), int(vm[0][2]), int(vm[0][3])
        rechecks.append((ver, v, rb, ri, rm))
        want = '; recomprobacion %s: %s; B %d / I %d / M %d' % (ver, v, rb, ri, rm)
        if n < len(tails) and not tails[n].startswith(want):
            errors.append('la cola de la recomprobacion %d no es su veredicto: falta %s' % (n + 1, ascii(want)))
    if i:
        for x in ['I-%02d' % k for k in range(1, i + 1)]:
            if not re.search(r'\|\s*\**%s\**\s*\|\s*\**(?:I|Importante)\**\s*\|' % x, report):  # la gravedad, 'I' o '**Importante**'
                errors.append('la comprobacion acotada no tiene la fila de ' + x)
            if not [l for l in pkg_lines if re.match(r'^\| \**V\d+-\d\d\** \|', l) and re.search(r'\*\*%s %s\b[^*]*\*\*' % (re.escape(reviewer), x), l)]:
                errors.append('ningun registro de cambios del paquete corrige %s de %s' % (x, reviewer))
            if 'corregidas en el paquete' not in result and not re.search(r'\b%s resuelto\b' % x, ''.join(tails)):
                errors.append('BOUNDED_RESULT no dice que %s esta corregida (ni "corregidas en el paquete" ni "%s resuelto" en una recomprobacion)' % (x, x))
    if not rechecks and i:
        errors.append('hay importantes y ninguna recomprobacion que las de por resueltas')
    if rechecks:
        ver, v, rb, ri, rm = rechecks[-1]
        if rb or ri or not ok_verdict(v):
            errors.append('la ultima recomprobacion (%s) da %s con %d bloqueantes y %d importantes' % (ver, ascii(v), rb, ri))
    return errors
# ---- bounded_check_t4d (fin)


# ---------------------------------------------------------------- la comprobacion acotada de PB-T4d (CA-T4d): BOUNDED_RESULT y bounded_check (el de
# make_pbt3a_contract.py, tal cual), sobre el informe propio de CA-T4d (como el de CA-T4a); se lee antes de decidir su pendiente
BOUNDED_CHECK = D4 + BOUNDED_NAME
BOUNDED_SRC, _rep = None, None
if BOUNDED_FILE:
    _rep = open(BOUNDED_FILE, 'rb').read().decode('utf-8')
    BOUNDED_SRC = '--bounded-file ' + os.path.basename(BOUNDED_FILE)
elif in_head(BOUNDED_CHECK):
    _rep = show(BOUNDED_CHECK)
    BOUNDED_SRC = BOUNDED_CHECK + ' en ' + REV_SHA[:8]
if BOUNDED_SRC is None:
    soft(False, 'emision: la comprobacion acotada de PB-T4d (%s) no esta ni en la revision ni en --bounded-file' % BOUNDED_CHECK)
elif BOUNDED_RESULT is None:
    pending('la comprobacion acotada de PB-T4d (CA-T4d, %s): BOUNDED_RESULT, el veredicto literal, copiado del informe' % BOUNDED_SRC)
else:
    assert re.match(r'EMITIBLE(?: TRAS CORRECCIONES)?[.,;]', BOUNDED_RESULT), ('BOUNDED_RESULT: solo la familia EMITIBLE', BOUNDED_RESULT)
    assert 'CA-T4d' in _rep and CAT4D_PKG in _rep, ('el informe no es el de CA-T4d sobre la ' + CAT4D_PKG, BOUNDED_SRC)
    # la ultima recomprobacion es la de la version del paquete (como la precondicion 11 de PB-T3a): si no, la emision espera a la recomprobacion
    _vers = re.findall(r'(?m)^## Recomprobaci\S+n \d+ \((v\d+(?:\.\d+)?)(?:, [^)]*)?\)\s*$', _rep.replace('\r\n', '\n'))
    soft(_vers and _vers[-1] == PKG_VERSION, 'emision: la ultima recomprobacion de CA-T4d (%s) no es de la %s' % (_vers[-1:] or '-', PKG_VERSION))
    _err = bounded_check_t4d(_rep, BOUNDED_RESULT, PKG_LINES)
    assert not _err, ('la comprobacion acotada de PB-T4d', BOUNDED_SRC, _err)
_sup = show(PILOT + '/F4B-DRAFTS/superseding-characterization.md')
soft('\n## PB-T4d ' in _sup, 'emision: el registro declarado aun no tiene la entrada de PB-T4d (las %d con accion, con su tabla al cerrar)' % LIST_TOTAL)
# las decisiones que esta version deja a la sesion (SESSION_OPEN), despues de leer todo lo demas: al emitir abortan
for _q in SESSION_OPEN:
    pending('decision de la sesion: ' + _q)
# la n.o 37, "este contrato" (6.4.2), no esta en X por construccion: lo escribe la BASE. No cuenta como autoridad que falta (al emitir sobre X abortaria)
SELF_CONTRACT = PILOT + '/' + TASK + '/coordinator/gate-contract.json'
_self37 = [i for i, a in enumerate(c['Authorities']) if a['Section'] == 'este contrato']
assert _self37 == [37] and c['Authorities'][37]['Path'] == SELF_CONTRACT, ('la n.o 37 es este contrato, por su seccion (6.4.2)', _self37)
assert [a['Path'] for a in c['Authorities']].count(SELF_CONTRACT) == 1
AUTH_MISSING = [a['Path'] for a in c['Authorities'] if not in_head(a['Path']) and a['Path'] != SELF_CONTRACT]

print('DRY (FASE 2) rev', REV_SHA[:8], '(objetos del checkout principal)' if OBJ_REV else '(HEAD del worktree)', 'TaskId', TASK,
      '| contrato en seco: PENDIENTES', len(PENDING), 'AVISOS', len(WARN))
print('anclaje: desciende de %s y %s (PB-T4c, %s; X y BASE), de su RED %s, de su GREEN %s y de su cierre %s; src y tests == %s'
      % (PT4C_EMIT_X[:8], PT4C_EMIT[:8], T4C_EMIT_ENTRY, RED_T4C[:8], GREEN_T4C[:8], PT4C_CLOSE[:8], GREEN_T4C[:8]))
print('numeracion prevista: %s (la emision); la evidencia %s (X) y %s (BASE) | hoy: F4B-E%d y la %s' % (EMIT_ENTRY, X_EV, BASE_EV, _ids[-1], _evs[-1]))
print('la CI del GREEN de PB-T4c (%s, la base): Core %d = %d + 0 + %d, UI %d = %d + 0 + %d' % (CI_BASE_T4C[0], CI_BASE_T4C[2], CI_BASE_T4C[2] - CI_BASE_T4C[3],
                                                                                        CI_BASE_T4C[3], CI_BASE_T4C[4], CI_BASE_T4C[4] - CI_BASE_T4C[5], CI_BASE_T4C[5]))
print('contrato de PB-T4c (%s, AuthorityRevision %s): 17 paradas estandar, PBT4C-C11 -> PBT4D-C11 (%d caracteres), EVID[0] en F4B_EXACT_EVIDENCES del v2, '
      '%d filtros' % (PBT4CC, pbt4c['AuthorityRevision'][:8], len(C11), len(_T4C_REQ)))
print('6.4 de la %s (%s): Objective %d caracteres, ASCII; alcance %d rutas (src/: %d cambian y %d se borran, todas de Pilot/; tests/: %d existentes, %d que '
      'edita el RED y %d que borra, y %d nuevas)' % (PKG_VERSION, PKG_SRC, len(OBJECTIVE), len(ALLOWED), len(SRC_CHANGE), len(SRC_DELETE), len(TESTS_EDIT),
                                                 len(RED_EDITED), len(RED_GONE), len(TESTS_NEW)))
print('las piezas (6.4.2, por sha256): %s' % '; '.join('%s %s' % (k, v[:8]) for k, v in PIECES.items()))
print('el RED en memoria: %d bloques, %d archivos editados, %d borrados y %d nuevos (NewFileSha iguales); el GREEN: %d editados y %d borrados; cada '
      'OLD una vez' % (len(piece_blocks(PIECE_SRC[RED_PIECE])), len(RED_EDITED), len(RED_GONE), len(RED_NEW), len(GREEN_TEXT), len(GREEN_GONE)))
print('[13] nombra %s (6.4.2: %s) y %d de las %d tandas del Owner de PB-T4d%s' % (EVID13_DS[0], SESSION_DS, len(EVID13_OWNERS), len(OWNER_D),
                                                                (' (no: %s)' % ', '.join(o for o in OWNER_D if o not in EVID13_OWNERS)) if len(EVID13_OWNERS) < len(OWNER_D) else ''))
print('la lista (%s, %s): %d filas, %d con accion; por accion %s; fila a fila: %d pruebas y %d ayudantes, %d Skip del RED (los de 6.4.1); 4.4 igual; C02 con '
      'exactamente esa excepcion' % (LIST_NAME, LIST_SRC, LIST_ROWS, LIST_TOTAL, ', '.join('%s %d' % kv for kv in sorted(BY_ACTION.items())),
                                     LIST_CHECK['prueba'], LIST_CHECK['no-prueba'], LIST_CHECK['skip-del-red']))
print('el recuento estatico del RED frente a la base (sin MemberData): Core ejecutables %+d, saltadas %+d (Theory sin contar: %d -> %d); UI %+d, %+d (%d -> %d)'
      % (DSTAT['Core'] + DSTAT['UI']))
if P6 is not None:
    print('la precondicion 6 (%s, sha256 %s; compile-check.md %s): la lista, %d piezas, %d archivos del RED y %d del GREEN (%d borrados) con su sha256, 5 '
          'compilaciones con 0 avisos CS, solo CT-PBT4D-01 cae en el RED, 18 filtros de UI sin Failed, cobertura %s, censo %s, ensayo y control como 6.4.1, '
          '%d tablas por clase, required-pbt4d.tsv; Package y Task, leidas; claves sin leer (decision de la sesion): %s' % (
              P6_SRC, hashlib.sha256(open(P6_FILE, 'rb').read()).hexdigest()[:8] if P6_FILE else '-', P6_CHECK_SHA[:8], len(PIECES), len(P6['RedFiles']),
              len(P6['GreenFiles']), len(P6['GreenDeleted']), P6['Coverage']['Verdict'], P6['Census']['Verdict'], len(CLASS_TABLES),
              ', '.join(P6_EXTRA) or 'ninguna'))
    print('por clase, en los archivos del RED: %d clases con lo estatico igual a lo medido en el GREEN, %d sin comprobar (Theory sin contar o fuera de los filtros)'
          % tuple(CLASS_CHECK))
print('RequiredTests: %d (%d heredados de PB-T4c con su numero, %d fuera por quedar en 0: %s; %d nuevos por clase exacta); MinSelected %s; con ExpectRed: %s'
      % (len(REQ), len(_inh), len(REQ_ZERO_PKG), ', '.join('n.o %d %s' % (k, v.split('.')[-1]) for k, v in sorted(REQ_ZERO_PKG.items())), len(_new),
         'medido en el GREEN (required-pbt4d.tsv)' if P6 is not None else 'de la tabla (sin la precondicion 6)', '; '.join('%s %d' % (r[1].split('.')[-1], r[2]) for r in _er)))
for _l in REQ_REPORT:
    print('  ' + _l)
print('la CI prevista (desde %s; RED = GREEN con los Failed de los filtros con ExpectRed): RED Core %d = %d + %d Failed + %d, UI %d = %d + %d Failed + %d; GREEN '
      'Core %d = %d + 0 + %d, UI %d = %d + 0 + %d; iguales a CI_T4D_EXPECTED, a la precondicion 6 y a [14]' % ((CI_SRC,) + CI_T4D['RED'] + (CI_T4D['GREEN'][0],
      CI_T4D['GREEN'][1], CI_T4D['GREEN'][3], CI_T4D['GREEN'][4], CI_T4D['GREEN'][5], CI_T4D['GREEN'][7])))
print('frente a la base: Core %+d resultados (%+d saltados), UI %+d (%+d); de ellos, lo estatico: Core %+d, UI %+d; el resto, Theory que el texto no cuenta'
      % (DELTA['Core'][0], DELTA['Core'][1], DELTA['UI'][0], DELTA['UI'][1], DSTAT['Core'][0] + DSTAT['Core'][1], DSTAT['UI'][0] + DSTAT['UI'][1]))
print('CONTRATO (en seco) allowed %d forbidden %d (src %d tests %d [prefijos partidos: %s; FORB_PREV %s, lo que separa las dos lecturas: %s] root %d) inv %d '
      'stops %d required %d (de %d) evidence %d authorities %d bytes %d sha256 %s'
      % (len(ALLOWED), len(FORBIDDEN), len(SRCF), len(TESTF), ', '.join(SPLIT2) or 'ninguno', FORB_PREV, ', '.join(FORB_DIFF) or 'nada', len(ROOT), len(INV),
         len(STOPS), len(REQ), N_REQ_ROWS, len(EVID), len(c['Authorities']), len(b), hashlib.sha256(b).hexdigest().upper()))
print('BOUNDED_RESULT: %s | bounded_check: %s' % (BOUNDED_RESULT or 'por rellenar', (('sin errores sobre ' + BOUNDED_SRC) if (BOUNDED_SRC and BOUNDED_RESULT)
                                                  else ('no corre (informe: %s)' % BOUNDED_SRC if BOUNDED_SRC else 'sin informe'))))
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
# ---------------------------------------------------------------- la emision (sin --dry): en el worktree, sobre el commit X, como la de PB-T4c; todo lo que
# en seco es AVISO o PENDIENTE ya abortaba (AS_WRITE), y la precondicion 6 y CA-T4d se leen de la revision antes de decidirlos
assert OBJ_REV is None and not PKG_FILE and not P6_FILE and not BOUNDED_FILE and not JSON_OUT, \
    'al emitir, solo el worktree: sin --objects-rev, --package-file, --p6-file, --bounded-file ni --json-out'
assert REV_SHA == AR, ('al emitir, la AuthorityRevision es el commit X, el HEAD del worktree', AR[:8], REV_SHA[:8])
assert PKG_VERSION == PKG_TARGET and BOUNDED_RESULT and BOUNDED_SRC and E56_OK and P6 is not None and not SESSION_OPEN and \
    in_head(PKG) and hashlib.sha256(show_bytes(PKG)).hexdigest() == CUSTODY_SHA256[PKG], 'faltan la v5.2, CA-T4d, F4B-E56, la precondicion 6, las decisiones o el paquete'
assert not PENDING and not WARN and not AUTH_MISSING, (PENDING, WARN, AUTH_MISSING)
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', "Get-Content -Raw '%s' | Test-Json -SchemaFile "
                    "'docs/automation/agent-execution/schemas/gate-contract.schema.json'" % OUT], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True', 'sha256', hashlib.sha256(b).hexdigest().upper())
sys.exit(0)
