"""Contrato de gate de F4B-PBT4-EDIT-ALL-RELOAD (PB-T4e, la recarga de Editar todo; ASCII), desde el paquete del contrato de PB-T4 (PKG_VERSION, abajo),
con el patron de make_pbt4d_contract.py, sobre el cierre de PB-T4d (PT4D_CLOSE; F4B-E59, la evidencia 288), con PB-T4e literal en 6.5, 6.5.1 y 6.5.2
del paquete. PB-T4e se emite con F4B-E60, la evidencia 289 (X) y 290 (BASE): parametros, comprobados al emitir.
Comprueba por objetos git:
- el anclaje: la revision desciende de la emision de PB-T4d (X y BASE), de su RED, de su GREEN y de su cierre; src y tests, los del GREEN de PB-T4d; la
  numeracion prevista; la CI del GREEN de PB-T4d en la entrada de su cierre;
- el contrato de PB-T4d de su emision: las 17 paradas estandar, PBT4D-C11 (de la que sale PBT4E-C11), la primera ExpectedEvidence y sus filtros;
- los sustitutos de nc1 y de nc2 con --task PBT4, y la Snapshot K;
- el paquete, por su sha256: 4.5 (la lista vacia), 5.7, 6.5 (el alcance ruta a ruta), sus textos literales (6.5.1: el Objective, los invariantes, las
  paradas, la ExpectedEvidence, los Skip, los archivos nuevos y los RequiredTests) y lo que necesita el generador (6.5.2: las piezas por sha256, la lista
  vacia y la precondicion 6);
- las dos piezas (el RED de tests/, solo NEWFILE, y el GREEN de src/, solo EDIT), leidas por sus bloques y aplicadas en memoria sobre los objetos de la
  base: sus rutas son el alcance, cada OLD esta una vez, los archivos nuevos son los de NewFileSha, y el RED y el GREEN dan los sha256 de la precondicion 6;
- la lista vacia: el RED no toca ningun archivo existente de tests/ ni pone ningun Skip; C02 lo dice y no admite ninguna excepcion;
- la precondicion 6 de la sesion (pbt4e-precondition6.json y compile-check.md de cc4e/; --p6-file en seco, de la revision al emitir): las piezas, la
  lista, los archivos del RED y del GREEN, los totales de cada rama, la cobertura y los RequiredTests medidos; se lee antes de decidir cualquier pendiente
  que dependa de ella (la leccion de la BASE de PB-T4b);
- los RequiredTests: los heredados del contrato de PB-T4d por su numero, con su MinSelected (ninguno baja) y su recuento estatico, y los nuevos; los totales
  de cada rama frente a la base, a la tabla del generador, a la precondicion 6 y a [14];
- la comprobacion acotada de PB-T4e (CA-T4e: independent-review-pbt4e-check.md, o --bounded-file) frente a BOUNDED_RESULT, con bounded_check_t4d;
- al emitir (sin --dry), todo de la revision X.
No escribe ningun contrato en seco: lista los PENDIENTES y los AVISOS.
Uso: python make_pbt4e_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> --dry [--objects-rev <rev> [--repo <checkout principal>]
                                   [--package-file <paquete>] [--p6-file <pbt4e-precondition6.json>] [--bounded-file <independent-review-pbt4e-check.md>]]
                                   [--json-out <archivo>] [--as-write]
Con --p6-file, compile-check.md, required-pbt4e.tsv y cov-p6-v55.txt se leen de la misma carpeta. Al custodiarlo, las rutas locales se redactan como en
PB-T0 ('<worktree-de-I-64>', '<checkout-principal>')."""
import json, os, re, sys, subprocess, hashlib, unicodedata

# la leccion del cierre de PB-T3c: el generador entero es ASCII (lo leen en ascii los check_* y la redaccion); BOUNDED_RESULT tambien
_self = open(os.path.abspath(__file__), 'rb').read()
assert all(b < 0x80 for b in _self), ('el generador lleva un caracter no ASCII', [i for i, b in enumerate(_self) if b >= 0x80][:5])

# ---------------------------------------------------------------- la base: la emision de PB-T4d (F4B-E56) y su cierre (F4B-E59)
PT4D_EMIT_X = '23c2923616352a7b32fb3c68d8181c41c7feb2c9'   # X de la emision de PB-T4d (la evidencia 286): AuthorityRevision de su contrato
PT4D_EMIT = '399c9bec6d4778aa50096ab6204859c0bd015419'     # BASE de la emision de PB-T4d (la evidencia 287)
RED_T4D = 'aa65ecfde87bfe274e9ba518f63dfe94c9e9f61a'       # el RED de PB-T4d (sin reparacion del RED)
GREEN_T4D = '7db6fd0c4e4a27ec05d56cce0daaf0883940a4e2'     # el GREEN de PB-T4d (intento 1)
PT4D_CLOSE = '160e019d773ba9095e2d5858cd593a954b8eec67'    # la custodia del cierre de PB-T4d (F4B-E59, la 288); la base de PB-T4e
# la CI del GREEN de PB-T4d en su cierre: (run, sha8 del GREEN, Core, Skipped de Core, UI, Skipped de UI); la del RED, 45054587789
CI_BASE_T4D = ('45057868915', '7db6fd0c', 19418, 403, 2464, 48)
# la numeracion (la sesion): parametros comprobados contra f4b-dispositions.md y la evidencia (una asignacion cada uno, para leerlos con ast)
T4D_EMIT_ENTRY = 'F4B-E56'
T4D_CLOSE_ENTRY = 'F4B-E59'
EMIT_ENTRY = 'F4B-E60'
T4D_X_EV = '286'
T4D_BASE_EV = '287'
CLOSE_EV = '288'
X_EV = '289'
BASE_EV = '290'
T4D_TASK = 'F4B-PBT4-RETIRE-PILOT'
# CA-T4e: su veredicto literal (ASCII), la linea BOUNDED de su informe con la cola de la recomprobacion si la hay, copiada del archivo con un guion y
# comparada; None mientras la comprobacion acotada no exista (PENDIENTE en seco)
BOUNDED_RESULT = ('EMITIBLE; B 0 / I 0 / M 5; sin importantes que corregir (M-01: con un borrador Sin decidir y marcado, el else-if evalua NeedsLazyReload sin proteger e'
                  'l comparador y TryEditAll pasa de Refused sin Report a Failed con Report, solo en el puerto e inalcanzable desde el panel porque el boton exige CanEdi'
                  't: declararlo o mover la rama tras las comprobaciones de la vista; M-02: el metodo de cobertura incluye las CT nuevas en el GREEN y difflib empareja m'
                  'al la llave de WHC:1201; M-03: la evaluacion de R-T4-39 llama pregunta 3 a la opcion 3 de la pregunta 1 y no nombra las premisas de list-proposal.md:3'
                  '1 ni de OWNER-02; M-04: post-p6 registra una v5.5 a241bfbe y la revisada es e597790f; M-05: la pregunta 1 dice toca una funcion); D-S17 y D-S18 se sos'
                  'tienen sobre 160e019d; sin decisiones nuevas del Owner; precondicion 6 sobre 160e019d: base Core 19418 = 19015 + 0 + 403 igual a la CI 45057868915, RE'
                  'D 19421 = 19015 + 3 + 403 y GREEN 19421 = 19018 + 0 + 403, UI RED 2465 = 2416 + 1 + 48 y GREEN 2465 = 2417 + 0 + 48, las 24 lineas de la sonda iguales'
                  ' a las CT, control campo y anfitrion correcto, cobertura PASA, RequiredTests 90 = 88 + 2 iguales a PB-T4d salvo el n.o 84 con ExpectRed false; GREEN y'
                  ' RED reproducidos por objetos git con los mismos sha256; 99 secciones sin cambio; recomprobacion v5.6: EMITIBLE; B 0 / I 0 / M 2 (M-06: el D-S17 del r'
                  'egistro de la sesion sigue describiendo el sitio de la v5.5, un else if antes de las comprobaciones, sin remitir a D-S19; M-07: la v5.6 cita CA-T4e co'
                  'n el sha256 27636ed5, el de antes de esta recomprobacion; los dos de texto o de custodia, sin decision del Owner); M-01 resuelto por D-S19 (la recarga'
                  ' tras las tres comprobaciones de la vista, con !begunHere && NeedsLazyReload; nada evalua la marca antes; marcado-sin-decidir da Failed con Report con'
                  ' el GREEN de la v5.5 en r76 y Refused sin fallos ni lecturas con el de la v5.6 en r82 y r83; las otras 24 lineas iguales); M-02 resuelto (los dos limi'
                  'tes declarados en 4.5 y en el guion; la corrida opcional sin las clases nuevas, Core 19418 = 19015 + 0 + 403 y la union de 18 filtros 899 = 869 + 0 + '
                  '30 tres veces, PASA con los cuerpos vigilados intactos; las 2 VARIA de RackViewBatchDialogPresenter.cs:37-38 varian entre corridas del mismo codigo, l'
                  'a base r73 0/3 frente a la r79 2/3, y el filtro 04 las cubre siempre); M-03 corregido en el registro; M-04 resuelto (post-p6 nombra la 6b8939ae; frent'
                  'e a la 38efafdb solo cambia la linea 9, y en post-p6 solo las dos lineas de la version); M-05 al registro de la emision; PBT4E-C11 es PBT4D-C11 con lo'
                  's reemplazos declarados y la v3 e68b6e59 (APTO; decision del Owner Sustituto v3 y revision, de aqui en adelante); P-01 con la Snapshot L 69781991 solo'
                  ' en PB-T4e, y 6.0 sigue con la K; 109 secciones sin cambio = 99 + 10; RequiredTests 90 y su tsv identicos a la v5.5; precondicion 6 sobre 160e019d: ba'
                  'se Core 19418 = 19015 + 0 + 403 igual a la CI 45057868915, RED 19421 = 19015 + 3 + 403 y GREEN 19421 = 19018 + 0 + 403, UI RED 2465 = 2416 + 1 + 48 y '
                  'GREEN 2465 = 2417 + 0 + 48, 21 compilaciones sin warning CS, control campo y anfitrion correcto; GREEN y RED de la v5.6 reproducidos por objetos git c'
                  'on los mismos sha256')  # la linea BOUNDED de CA-T4e y la cola de su recomprobacion
BOUNDED_NAME = 'independent-review-pbt4e-check.md'  # el informe de CA-T4e, propio (como los de CA-T4a a CA-T4d), en F4B-DRAFTS/pbt4/ (la sesion)
CAT4E_PKG = 'v5.5'  # la version del paquete sobre la que corre CA-T4e (sus recomprobaciones, sobre PKG_VERSION)
# la version del paquete: la v5.5 (PB-T4e entera y literal)
PKG_VERSION = 'v5.6'
PKG_TARGET = 'v5.6'
VTAG = 'v56'  # el sufijo de las piezas y los guiones de esta version
CUSTODY_SHA256 = {'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md': '6b8939ae26d39392824108fe9b73bc3741f30a45657810a2ecf534e9a40cf66a'}  # la v5.6
# la lista de PB-T4e: vacia (6.5.2: el contrato no lleva archivo de lista; K = 0, D = 0 y E = 0)
LIST_K, LIST_D, LIST_E = 0, 0, 0
OWNER_E = ['owner-pbt4e-list-decisions.md']                       # en coordinator/, la 15.a tanda (autoridad de PB-T4e)
OWNER_ORIGIN = 'owner-pbt4c-code-decisions.md'                    # en coordinator/, ya custodiado: su pregunta 2, el origen (6.5.2, "Se anaden")
OWNER_ORIGIN_2 = 'owner-pbt4d-list-decisions.md'  # el otro origen, ya custodiado: NO es autoridad (6.5.2 excluye las tandas 10 a 14; la sesion)
SESSION_DEC_E = 'session-pbt4e-design-decisions.md'               # D-S17 y D-S18, en F4B-DRAFTS/pbt4/
SESSION_DS = 'D-S17, D-S18 y D-S19'
SESSION_DEC_SHA = 'b21f2af4bab5321620bf8de3aceeb95585a0e2fe9e4cc2ae96f18e37c3e41e6d'  # el registro que cita 6.5.2 de la v5.6
# las piezas por sha256 que trae la v5.5 (6.5.2): las dos piezas literales, sus modulos y aplicadores y su generador. Su sha256 lo da el paquete; el
# archivo, la revision (al emitir) o junto a --package-file (en seco)
PIECE_NAMES = ['pbt4e-red-v56.txt', 'red_pbt4e_v56.py', 'apply_red_pbt4e_v56.py', 'v56/make_pieces_pbt4e_v56.py', 'pbt4e-green-v56.txt', 'green_pbt4e_v56.py', 'apply_green_pbt4e_v56.py']
RED_PIECE, GREEN_PIECE = 'pbt4e-red-%s.txt' % VTAG, 'pbt4e-green-%s.txt' % VTAG
N_ALLOWED = 6
N_REQ_ROWS = 90  # 88 heredados (todos los de PB-T4d) y 2 nuevos
N_REQ_INH = 88
# lo que fija la version del paquete: las CT y los Failed del RED (5.7)
N_CT = 4  # CT-PBT4E-01 a 04
N_CT_CORE, N_CT_UI = 3, 1
RED_FAIL_CORE, RED_FAIL_UI = 3, 1   # los Failed del RED (5.7: las cuatro CT, ExpectRed true)
RED_CT_NOTE = {'Core': 'CT-PBT4E-01, 02 y 04', 'UI': 'CT-PBT4E-03'}  # lo que [14] anota tras los Failed del RED de cada proyecto
# los totales previstos de cada rama (el paquete, [14] y 6.5.1): (total, Passed, Failed, Skipped) de Core y de UI
CI_T4E_EXPECTED = {'GREEN': (19421, 19018, 0, 403, 2465, 2417, 0, 48), 'RED': (19421, 19015, 3, 403, 2465, 2416, 1, 48)}
# la precondicion 6 de PB-T4e (hs65/pbt4ecc/compile/, en la custodia F4B-DRAFTS/pbt4/cc4e/): su resumen va byte a byte a F4B-DRAFTS/pbt4/P6_REL_NAME;
# su esquema y sus claves los fija 6.5.2. compile-check.md, en cc4e/
P6_REL_NAME = 'pbt4e-precondition6.json'
P6_SCHEMA = 'rackcad-pbt4e-precondition6/v1'
P6_DIR = 'cc4e/'
P6_CHECK_NAME = 'compile-check.md'
P6_REQ_NAME = 'required-pbt4e-v56.tsv'  # el nombre que da la precondicion 6 de la v5.6 (RequiredTests.Tsv)
P6_SHA = 'daea59847a3629111073f0b7565d9144be90c880cf2ba61c98aae91373c9c169'        # el resumen (6.5.1)
P6_CHECK_SHA = '549766ebfb0ab0e2d75d33e334faab1e5f66454902f31b87e7b8f97818bbd426'  # el registro de la corrida (6.5.1)
P6_COV_NAME = 'cov-p6-v56.txt'  # la salida de la cobertura (6.5.1), por el sha256 de Coverage.OutputSha256
# el TaskId que lleva la precondicion 6: el provisional del paquete, con su nota (la sesion lo confirma en session-pbt4e-design-decisions.md)
P6_TASK = 'F4B-PBT4-EDIT-ALL-RELOAD (provisional; lo confirma la sesion al emitir)'
# lo que el generador lee de la precondicion 6 (6.5.2: RedFiles, GreenFiles, RequiredTests y Totals; y, como comprobacion, Schema, Package, Task,
# Revision, Pieces, List, Builds, CsWarnings, Red, Green, GreenWithCoverage, BaseCoverage, Coverage, SuiteDurationSeconds y Outcome) y lo informativo
# (6.5.2: Probe y Control; y Alone y GreenUiCoverage, que no deciden nada): se imprime, no se comprueba
P6_INFO = ('Probe', 'NewLineDS19', 'CoverageExisting', 'Control', 'Alone', 'GreenUiCoverage')
SNAP_K = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-K/snapshot-k.json'
SNAP_K_DEC = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-config-snapshot-k-decision.md'
SNAP_K_SHA = 'E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE'
SNAP_K_PROBE = 'R20261119T154313Z-2a9a'
# la Snapshot L (F4B-E57): la de P-01 de PB-T4e desde la v5.6 (6.5.1 y 6.5.2); la K queda para PB-T4a a PB-T4d (6.0)
SNAP_L = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-L/snapshot-l.json'
SNAP_L_DEC = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-config-snapshot-l-decision.md'
SNAP_L_SHA = 'B71650A637551747F522BB1D2F4BA1BB9E25AAFF1A61CE1B7BC066197FFFEBB3'
SNAP_L_FILE_SHA = '69781991612268de4ad3f46bd3d0ccfdc2275b3a9bea1390d27a723118b75e8e'
SNAP_L_NAMES = 105
SNAP_L_PROBE = 'R20261122T172638Z-2c14'  # la sonda neutra de F4B-E57
NC1_SUBST = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc1-cascade-substitute/nc1_cascade_signal_substitute.py'
NC1_SUBST_SHA = '41440edc4a963bef890e4336dccb885837424008c027cdb172d2b926e0f49b40'
NC2_DIR = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/'
NC2_V2 = NC2_DIR + 'nc2_exact_whitelist_substitute_f4b_v2.py'
NC2_V2_SHA = 'c033e96ce147963feb9c04733e9bfbec7493fea33d8ff79b1c2e8ab860798e45'
NC2_V2_REVIEW = NC2_DIR + 'review/independent-review-substitute-f4b-v2.md'
NC2_V2_LIMITS = NC2_DIR + 'limits-declared.md'
# la v3 del sustituto de nc2 (F4B-E58: "sustituye a la v2 de aqui en adelante, PB-T5 incluida")
NC2_V3_DIR = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v3/'
NC2_V3 = NC2_V3_DIR + 'nc2_exact_whitelist_substitute_f4b_v3.py'
NC2_V3_SHA = 'e68b6e5993b522df83e7cd4cd2b136ddf503e237fd560b1a97067f24664328c6'
NC2_V3_REVIEW = NC2_V3_DIR + 'review/independent-review-substitute-v3.md'
NC2_V3_DEC = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt4d-nc2-decision.md'
NC_TASK = 'PBT4'  # el --task de nc1 y nc2 para las subtareas de PB-T4 (el TaskId empieza por F4B-PBT4-)
# las decisiones que esta version del generador deja a la sesion: cada una es un PENDIENTE hasta que la sesion la fije (no se decide por analogia)
# Las respuestas de la sesion (2026-11-22): C11 y P-01 esperan a la v5.6, que los trae literales (la sesion decidio el contenido; el generador no deriva
# nada a mano); las autoridades, la propuesta aceptada (AUTH_PLAN)
SESSION_OPEN = []
# las autoridades (la propuesta, aceptada por la sesion el 2026-11-22; se lleva a lo literal de 6.5.2 de la v5.6): de las 62 del contrato de PB-T4d siguen la 0 a la 43, con su seccion al dia la 0, 6, 28, 29, 36,
# 37 y 43 (CA-T4d -> CA-T4e, en su sitio); salen la 44 a la 61 (la 44, owner-pbt4b-code-decisions.md, los vacios de M-04 para PB-T4d; las tandas 10 a 14;
# las decisiones de la sesion de PB-T4d; la lista, las piezas y la precondicion 6 de PB-T4d; sus UNIT_CHANGE); se anaden las de 6.5.2, con los dos
# origenes del Owner que admite 6.5.2 (owner-pbt4c-code-decisions.md; owner-pbt4d-list-decisions.md es la 10.a tanda y queda fuera: la sesion)
AUTH_PLAN = {'drop': [14, 22, 23, 24, 25] + list(range(44, 62)), 'ca_at': 43}  # v5.6, 6.5.2: ni la K (14) ni la v2 de nc2 (22 a 25)
C11_SUBS = [('PB-T4d', 'PB-T4e'), ('PBT4D', 'PBT4E')]  # la derivacion que describe el literal de PBT4E-C11 (6.5.1)
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
assert len(POS) == 3, 'uso: make_pbt4e_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> --dry [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
DRY = '--dry' in ARGS
# sin --dry, el generador emite (en el worktree, sobre el commit X): lo que en seco es un AVISO o un PENDIENTE aborta (como --as-write)
AS_WRITE = '--as-write' in ARGS or not DRY
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % 'F4B-PBT4-EDIT-ALL-RELOAD'
OBJ_REV = opt('--objects-rev')
PKG_FILE = opt('--package-file')
assert PKG_FILE is None or OBJ_REV is not None, '--package-file solo junto con --objects-rev'
PKG_FILE = os.path.abspath(PKG_FILE) if PKG_FILE else None
assert PKG_FILE is None or os.path.isfile(PKG_FILE), ('no existe el paquete', PKG_FILE)
P6_FILE = opt('--p6-file')  # la precondicion 6 de la sesion, en seco (con compile-check.md en su carpeta); al emitir, la de la revision
P6_FILE = os.path.abspath(P6_FILE) if P6_FILE else None
assert P6_FILE is None or (os.path.isfile(P6_FILE) and DRY), ('--p6-file solo en seco y con el archivo', P6_FILE)
BOUNDED_FILE = opt('--bounded-file')  # el informe de CA-T4e, en seco; al emitir, el de la revision
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

TASK = 'F4B-PBT4-EDIT-ALL-RELOAD'
assert len(TASK) <= 26 and TASK.startswith('F4B-' + NC_TASK + '-'), len(TASK)  # R-T1-07 (MAX_PATH, una cota superior); la ligadura de --task PBT4
PILOT = 'docs/automation/evidence/I-64-pilot'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
D4 = PILOT + '/F4B-DRAFTS/pbt4/'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
EVIDENCE = 'docs/automation/evidence/I-64-evidence.md'
PBT4DC = PILOT + '/' + T4D_TASK + '/coordinator/gate-contract.json'
T4D_TCP = PILOT + '/' + T4D_TASK + '/task-close-package.md'
PKG = 'docs/initiatives/I-64-f4b-pbt4-gate-contract-package.md'
P6_REL = D4 + P6_REL_NAME
P6_CHECK_REL = D4 + P6_DIR + P6_CHECK_NAME
P6_REQ_REL = D4 + P6_DIR + P6_REQ_NAME
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
WARN = []
PENDING = []  # lo que espera a la precondicion 6, a CA-T4e o a una decision de la sesion: el seco lo lista; al emitir (AS_WRITE) aborta
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
    """Lo que solo existira con la emision de PB-T4e (su commit X, su comprobacion acotada): AVISO en seco, error con --as-write."""
    if cond:
        return True
    assert not AS_WRITE, msg
    WARN.append(msg)
    return False


def pending(msg):
    """Lo que espera a la precondicion 6, a CA-T4e o a una decision de la sesion: se lista en seco (PENDIENTE); con --as-write o al emitir aborta. Un
    pendiente que depende de la precondicion 6 o de CA-T4e se decide despues de leerlas de la revision, nunca antes (la leccion de la BASE de PB-T4b)."""
    assert not AS_WRITE, 'pendiente: ' + msg
    PENDING.append(msg)


REV_SHA = git('rev-parse', REV).stdout.decode('ascii').strip()
assert re.fullmatch(r'[0-9a-f]{40}', REV_SHA), ('revision desconocida', REV)

# ---------------------------------------------------------------- el anclaje: la emision de PB-T4d (F4B-E56) y su cierre (F4B-E59)
for _full in (PT4D_EMIT_X, PT4D_EMIT, RED_T4D, GREEN_T4D, PT4D_CLOSE):
    assert git('rev-parse', _full[:8]).stdout.decode('ascii').strip() == _full, ('SHA', _full[:8])
    assert git('merge-base', '--is-ancestor', _full, REV).returncode == 0, ('la revision no desciende de PB-T4d', _full[:8])
assert git('rev-parse', PT4D_EMIT + '^').stdout.decode('ascii').strip() == PT4D_EMIT_X, 'BASE de PB-T4d no es hijo directo de su X'
assert git('rev-parse', RED_T4D + '^').stdout.decode('ascii').strip() == PT4D_EMIT, 'el RED de PB-T4d no es hijo directo de su BASE'
assert git('rev-parse', GREEN_T4D + '^').stdout.decode('ascii').strip() == RED_T4D, 'el GREEN de PB-T4d no es hijo directo de su RED (sin reparacion)'
assert git('diff', '--quiet', GREEN_T4D, PT4D_CLOSE, '--', 'src', 'tests').returncode == 0, 'el cierre de PB-T4d no tiene el src y los tests de su GREEN'
assert git('diff', '--quiet', GREEN_T4D, REV, '--', 'src', 'tests').returncode == 0, 'src o tests cambiaron desde el GREEN de PB-T4d'
_disp = show(DISP).replace('\r\n', '\n')
_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
_n = lambda e: int(e.split('E')[-1])
assert _ids == list(range(1, len(_ids) + 1)) and _ids[-1] >= _n(T4D_CLOSE_ENTRY), ('f4b-dispositions.md', _ids[-3:])
assert _n(EMIT_ENTRY) == _n(T4D_CLOSE_ENTRY) + 1 and _n(T4D_CLOSE_ENTRY) > _n(T4D_EMIT_ENTRY), ('la numeracion: E60 sigue a E59', T4D_CLOSE_ENTRY, EMIT_ENTRY)
assert _ids[-1] in (_n(T4D_CLOSE_ENTRY), _n(EMIT_ENTRY)), ('la ultima entrada de f4b-dispositions.md', _ids[-1])
assert re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (T4D_EMIT_ENTRY, T4D_TASK), _disp), 'F4B-E56, la emision de PB-T4d'
_dc = [p for p in re.split(r'(?m)^## ', _disp) if p.startswith(T4D_CLOSE_ENTRY + '. ')]
assert len(_dc) == 1 and re.match(r'%s\. Cierre de %s\b' % (T4D_CLOSE_ENTRY, T4D_TASK), _dc[0]), 'F4B-E59, el cierre de PB-T4d'
_dc = _dc[0]
E60_OK = bool(re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (EMIT_ENTRY, TASK), _disp))
_evs = re.findall(r'(?m)^## (\d+)\. ', show(EVIDENCE))
assert int(T4D_BASE_EV) == int(T4D_X_EV) + 1 and int(X_EV) == int(CLOSE_EV) + 1 and int(BASE_EV) == int(X_EV) + 1 and int(CLOSE_EV) > int(T4D_BASE_EV), \
    ('la numeracion de la evidencia', CLOSE_EV, X_EV, BASE_EV)
assert _evs == [str(k) for k in range(1, len(_evs) + 1)] and _evs[-1] in (CLOSE_EV, X_EV), ('la ultima seccion de la evidencia', _evs[-3:])
assert T4D_X_EV in _evs and T4D_BASE_EV in _evs and CLOSE_EV in _evs, 'las secciones 286, 287 y 288 de PB-T4d'
# la CI del GREEN de PB-T4d, en la entrada de su cierre (la linea base de PB-T4e, [14])
_run, _sha8, _core, _cskip, _ui, _uskip = CI_BASE_T4D
_sp = lambda v: format(v, ',').replace(',', ' ')
assert GREEN_T4D.startswith(_sha8) and ('GREEN `%s`' % _run) in _dc and 'CLOSED' in _dc, ('la entrada de cierre de PB-T4d', CI_BASE_T4D)
assert ('Core %s = %s + 0 + %d' % (_sp(_core), _sp(_core - _cskip), _cskip)) in _dc and ('UI %s = %s + 0 + %d' % (_sp(_ui), _sp(_ui - _uskip), _uskip)) in _dc, \
    ('los recuentos de CI_BASE_T4D no son los de la entrada de cierre de PB-T4d', CI_BASE_T4D)
assert 'CLOSED' in show(T4D_TCP), 'el task-close-package de PB-T4d no dice CLOSED'
soft(E60_OK, 'emision: %s (la emision de %s) aun no esta en f4b-dispositions.md' % (EMIT_ENTRY, TASK))
if soft(git('cat-file', '-e', AR + '^{commit}').returncode == 0, 'emision: la AuthorityRevision %s no esta en el almacen de objetos (seco)' % AR[:8]):
    assert git('merge-base', '--is-ancestor', PT4D_CLOSE, AR).returncode == 0, 'la AuthorityRevision no contiene el cierre de PB-T4d'
for _p in (PKG, PBT4DC, T4D_TCP):
    assert in_head(_p), ('custodiado antes de PB-T4e y no esta en la revision', _p)

# ---------------------------------------------------------------- el contrato de PB-T4d de su emision: de el salen los textos comunes, C11 y los filtros
pbt4d = json.loads(show(PBT4DC))
assert pbt4d['TaskId'] == T4D_TASK and pbt4d['AuthorityRevision'] == PT4D_EMIT_X, ('el contrato de PB-T4d', pbt4d['AuthorityRevision'][:8])
T4D_STOP = {s['Id']: s['Text'] for s in pbt4d['StopConditions']}
STD_STOPS = [s for s in pbt4d['StopConditions'] if not s['Id'].startswith('PBT4D-')]
assert len(STD_STOPS) == 17, ('las paradas estandar del contrato de PB-T4d', len(STD_STOPS))
EVID0 = pbt4d['ExpectedEvidence'][0]


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


# ---- snapl_check (inicio)
def snapl_check(record, decision, sha, probe):
    """El registro de la Snapshot L y la decision del Owner (F4B-E57), con la forma de snapk_check (el de la K, F4B-E37): el registro es el de la L, con
    el SHA-256 del config.toml y la sonda neutra con Pass; la decision trae la respuesta literal del Owner. Devuelve los errores (vacia si vale)."""
    import json
    try:
        rec = json.loads(record)
    except ValueError as e:
        return ['el registro de la Snapshot L no es JSON: %s' % e]
    if not isinstance(rec, dict):
        return ['el registro de la Snapshot L no es un objeto JSON']
    errors = []
    if rec.get('Snapshot') != 'L':
        errors.append('el registro no es el de la Snapshot L: Snapshot=%r' % (rec.get('Snapshot'),))
    if str(rec.get('ConfigSha256', '')).upper() != sha:
        errors.append('el SHA-256 del registro (%s) no es el de la L (%s)' % (rec.get('ConfigSha256'), sha))
    neutral = rec.get('NeutralProbe') if isinstance(rec.get('NeutralProbe'), dict) else {}
    if neutral.get('RunId') != probe:
        errors.append('la sonda neutra del registro (%s) no es %s' % (neutral.get('RunId'), probe))
    if neutral.get('Pass') is not True:
        errors.append('la sonda neutra del registro no da Pass')
    if 'Snapshot L' not in decision or 'Sonda neutra, luego L (Recomendado)' not in decision:
        errors.append('la decision no trae la Snapshot L con la respuesta literal del Owner')
    return errors
# ---- snapl_check (fin)


# la Snapshot L (F4B-E57; 6.5.1, P-01, y 6.5.2 de la v5.6): la que se comprueba, no la K (que sigue en 6.0 para PB-T4a a PB-T4d)
assert in_head(SNAP_L) and in_head(SNAP_L_DEC), ('la Snapshot L o su decision no estan en la revision', SNAP_L, SNAP_L_DEC)
assert hashlib.sha256(show_bytes(SNAP_L)).hexdigest() == SNAP_L_FILE_SHA, ('el registro de la Snapshot L no es el de 6.5.2 (sha256)', SNAP_L)
_err = snapl_check(show(SNAP_L), show(SNAP_L_DEC), SNAP_L_SHA, SNAP_L_PROBE)
assert not _err and json.loads(show(SNAP_L)).get('NameCount') == SNAP_L_NAMES, ('la Snapshot L (F4B-E57)', _err)

# ---------------------------------------------------------------- C11: los sustitutos de nc1 (PD-F4B-01) y de nc2 (v2, F4B-E43; v3, F4B-E58), con --task PBT4
assert hashlib.sha256(show_bytes(NC1_SUBST)).hexdigest() == NC1_SUBST_SHA, ('el sustituto de nc1 de PD-F4B-01 no esta en la revision con su SHA-256', NC1_SUBST)
_v2 = show_bytes(NC2_V2)
assert hashlib.sha256(_v2).hexdigest() == NC2_V2_SHA, ('el sustituto de nc2 v2 no esta en la revision con su SHA-256', NC2_V2)
for _p in (NC2_V2_REVIEW, NC2_V2_LIMITS):
    assert in_head(_p) and NC2_V2_SHA.upper()[:8] in show(_p).upper(), ('la revision o los limites del sustituto de nc2 v2', _p)
_v3 = show_bytes(NC2_V3)
assert hashlib.sha256(_v3).hexdigest() == NC2_V3_SHA, ('el sustituto de nc2 v3 (F4B-E58) no esta en la revision con su SHA-256', NC2_V3)
assert ('`' + NC2_V3_SHA.upper() + '`') in _disp and 'La v3 sustituye a la v2 de aqu' in _disp, 'F4B-E58: la v3 y su sha256'
import ast as _ast


def _literals(raw):
    return {t.id: n.value for n in _ast.parse(raw.decode('utf-8')).body if isinstance(n, _ast.Assign) for t in n.targets if isinstance(t, _ast.Name)}


NC1_TASKS = _ast.literal_eval(_literals(show_bytes(NC1_SUBST))['TASKS'])
assert NC1_TASKS.get(NC_TASK) == 'PB-T4', ('nc1 no admite --task PBT4', NC1_TASKS)
for _nm, _raw in (('v2', _v2), ('v3', _v3)):
    _lit = _literals(_raw)
    assert _ast.literal_eval(_lit['TASKS']).get(NC_TASK) == 'PB-T4', ('nc2 %s no admite --task PBT4' % _nm)
    # la primera ExpectedEvidence: la del contrato de PB-T4d (la de PB-T3c), byte a byte, en la lista cerrada de literales del sustituto
    assert EVID0 in _ast.literal_eval(_lit['F4B_EXACT_EVIDENCES']), ('la primera evidencia del contrato de PB-T4d no esta en F4B_EXACT_EVIDENCES de nc2 %s' % _nm)
# C11 de PB-T4e: el literal entero de 6.5.1 de la v5.6 (decision de la sesion: con la v3 del sustituto de nc2, F4B-E58; nada derivado a mano); en e7
C11 = None


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


# ---------------------------------------------------------------- el paquete (PKG_VERSION, por su sha256): 4.5, 5.7, 6.0 y 6.5 de PB-T4e
if PKG_FILE:
    PKG_RAW, PKG_SRC = open(PKG_FILE, 'rb').read(), '--package-file ' + os.path.basename(PKG_FILE)
else:
    PKG_RAW, PKG_SRC = show_bytes(PKG), PKG + ' en ' + REV_SHA[:8]
assert hashlib.sha256(PKG_RAW).hexdigest() == CUSTODY_SHA256[PKG], ('el paquete leido no es la %s revisada (sha256)' % PKG_VERSION, PKG_SRC)
PKG_LINES = PKG_RAW.decode('utf-8').replace('\r\n', '\n').split('\n')
_title = [l for l in PKG_LINES[:8] if l.startswith('# I-64 F4B')]
assert len(_title) == 1 and re.search(r', (v\d+(?:\.\d+)?)\s*$', _title[0]).group(1) == PKG_VERSION, ('la version del paquete', _title)
assert PKG_VERSION == PKG_TARGET, ('el generador de PB-T4e es el de la %s (6.5.1 y 6.5.2 literales)' % PKG_TARGET, PKG_VERSION)


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


SEC65 = section('### 6.5 PB-T4e')
SEC57 = section('### 5.7 PB-T4e')
SEC45 = section('### 4.5 PB-T4e')
SEC64 = section('### 6.4 PB-T4d')
_s652 = '\n'.join(section('#### 6.5.2 '))
_s65 = '\n'.join(SEC65)


def local_or_rev(name):
    """Un archivo de F4B-DRAFTS/pbt4/ de la revision, o, en seco y si aun no esta, el de junto a --package-file (la custodia X de PB-T4e al emitir)."""
    if in_head(D4 + name):
        return show_bytes(D4 + name), D4 + name + ' en ' + REV_SHA[:8]
    _loc = os.path.join(os.path.dirname(PKG_FILE), *name.split('/')) if PKG_FILE else None
    if _loc and os.path.isfile(_loc):
        soft(False, 'emision: %s aun no esta en la revision (la custodia X de PB-T4e)' % (D4 + name))
        return open(_loc, 'rb').read(), name + ' junto a --package-file'
    return None, None


# las piezas por sha256 (6.5.2): en la columna "Lo que es", la pieza de la fila ("sha256 `...`") y los guiones que la acompanan ("`nombre` (`sha256`)")
PIECES = {}
for _row in [l for l in _s652.split('\n') if l.startswith('| **')]:
    _cells = _row.split(' | ')
    _where = re.findall(r'`pbt4/([\w./-]+\.(?:txt|py|tsv))`', _cells[1])
    _first = re.match(r'sha256 `([0-9a-f]{64})`', _cells[2])
    if _where and _first:
        assert len(_where) == 1, ('6.5.2: una pieza por fila', _where)
        PIECES[_where[0]] = _first.group(1)
    for _nm, _h in re.findall(r'`((?:v5\d/)?[\w.-]+\.py)`(?: \([^()`]*\))? \(`([0-9a-f]{64})`\)', _cells[2]):
        assert PIECES.get(_nm, _h) == _h, ('6.5.2: dos sha256 para una pieza', _nm)
        PIECES[_nm] = _h
assert sorted(PIECES) == sorted(PIECE_NAMES), ('6.5.2: las piezas por sha256 no son PIECE_NAMES', sorted(set(PIECES) ^ set(PIECE_NAMES)))
PIECE_SRC = {}
for _nm in PIECE_NAMES:
    _b, _src = local_or_rev(_nm)
    assert _b is not None and hashlib.sha256(_b).hexdigest() == PIECES[_nm], ('una pieza no esta, o no es la del paquete', _nm, _src)
    PIECE_SRC[_nm] = _b
for _nm, _h in re.findall(r'`(pbt4e-(?:green|red)-v5\d\.txt)` \(sha256 `([0-9a-f]{64})`', _s65):
    assert PIECES[_nm] == _h, ('6.5 y 6.5.2 dan dos sha256 para una pieza', _nm)
assert len(re.findall(r'`(pbt4e-(?:green|red)-v5\d\.txt)` \(sha256 `([0-9a-f]{64})`', _s65)) == 2, '6.5: las dos piezas con su sha256'
MARK = '@@@@ '


def piece_blocks(raw):
    """Los bloques de una pieza literal (EDIT, NEWFILE, DELETE), con la forma de red_pbt4e_v55.blocks y green_pbt4e_v55.blocks, sin importarlos."""
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


# PB-T4e: el RED solo anade archivos de tests/ (NEWFILE; sin esqueleto) y el GREEN solo edita src/ (EDIT; ningun borrado)
RED_BASE, RED_TEXT, RED_NEW, RED_GONE = apply_piece(PIECE_SRC[RED_PIECE], ('NEWFILE',))
GREEN_BASE, GREEN_TEXT, _gn, GREEN_GONE = apply_piece(PIECE_SRC[GREEN_PIECE], ('EDIT',))
RED_BLOCKS, GREEN_BLOCKS = len(piece_blocks(PIECE_SRC[RED_PIECE])), len(piece_blocks(PIECE_SRC[GREEN_PIECE]))
assert not _gn and not GREEN_GONE and not RED_GONE and not RED_BASE and sorted(RED_TEXT) == sorted(RED_NEW), 'el RED solo NEWFILE y el GREEN solo EDIT'
assert all(p.startswith(('tests/RackCad.Tests/', 'tests/RackCad.UI.Tests/')) for p in RED_TEXT) and all(p.startswith('src/') for p in GREEN_TEXT), \
    'el RED solo en tests/ y el GREEN solo en src/'
RED_EDITED = []
RED_FINAL = dict(RED_TEXT)
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


# ---------------------------------------------------------------- la lista de PB-T4e: vacia (4.5 y 6.5.2). Ninguna prueba existente se edita, se salta, se
# reactiva, se borra ni se sustituye: el RED no toca ningun archivo existente de tests/ (solo NEWFILE) y ninguna de sus pruebas lleva Skip
_lrow = [l for l in _s652.split('\n') if l.startswith('| **La lista** |')]
assert len(_lrow) == 1 and ' | Vac' in _lrow[0] and 'no lleva archivo de lista' in _lrow[0], ('6.5.2: la fila de la lista vacia', _lrow)
_s45 = '\n'.join(SEC45)
assert '**La lista: vac' in _s45 and ('K = %d, D = %d y E = %d' % (LIST_K, LIST_D, LIST_E)) in _s45, '4.5: la lista vacia, K = 0, D = 0 y E = 0'
assert re.search(r'(?m)^\*\*Los `Skip` literales:\*\* ninguno \(la lista vac\S+a\)\.$', _s65), '6.5.1: ningun Skip literal'
assert not RED_EDITED and not RED_GONE, 'la lista vacia: el RED no edita ni borra ningun archivo existente de tests/'
NEW_TESTS = {p: tests_of(t) for p, t in RED_TEXT.items()}
assert all(not t['skip'] and t['kind'] == 'fact' and t['n'] == 1 for ts in NEW_TESTS.values() for t in ts), 'las pruebas nuevas: Fact sin Skip (5.0)'
assert sum(len(ts) for ts in NEW_TESTS.values()) == N_CT, ('las CT nuevas', {p: len(ts) for p, ts in NEW_TESTS.items()})
LIST_ROWS = LIST_TOTAL = 0


# ---------------------------------------------------------------- la precondicion 6 de PB-T4e (--p6-file en seco, con compile-check.md, required-pbt4e.tsv y
# cov-p6-v55.txt en su carpeta; al emitir, los de la revision, en F4B-DRAFTS/pbt4/ y cc4e/): se lee antes de decidir lo que depende de ella (la leccion de
# la BASE de PB-T4b)
P6, P6_SRC, P6_CC, P6_REQ, P6_COVB, P6_RAW = None, None, None, None, None, None
if P6_FILE:
    P6_RAW = open(P6_FILE, 'rb').read()
    _d = os.path.dirname(P6_FILE)
    P6_SRC = '--p6-file ' + os.path.basename(P6_FILE)
    _rd = lambda n: open(os.path.join(_d, n), 'rb').read() if os.path.isfile(os.path.join(_d, n)) else None
    P6_CC, P6_REQ, P6_COVB = _rd(P6_CHECK_NAME), _rd(P6_REQ_NAME), _rd(P6_COV_NAME)
elif in_head(P6_REL):
    P6_RAW = show_bytes(P6_REL)
    P6_SRC = P6_REL + ' en ' + REV_SHA[:8]
    _rv = lambda n: show_bytes(D4 + P6_DIR + n) if in_head(D4 + P6_DIR + n) else None
    P6_CC, P6_REQ, P6_COVB = _rv(P6_CHECK_NAME), _rv(P6_REQ_NAME), _rv(P6_COV_NAME)
# 6.5.2: la fila de la precondicion 6 (su esquema y sus claves, en su orden); 6.5.1: el sha256 del resumen y el de compile-check.md
_p6row = [l for l in _s652.split('\n') if l.startswith('| **La precondici')]
assert len(_p6row) == 1 and P6_SCHEMA in _p6row[0], '6.5.2: la fila de la precondicion 6 (su esquema)'
_p6k = _p6row[0].split(P6_SCHEMA + '`', 1)[1].split('. El generador lee', 1)[0]
P6_DECLARED = re.findall(r'`([A-Z][A-Za-z0-9]+)`', _p6k)  # NewLineDS19 lleva cifras (v5.6)
_reads = re.search(r'El generador lee ((?:`\w+`(?:, | y )?)+); ((?:`\w+`(?:, | y )?)+) son informativos', _p6row[0])
assert _reads, '6.5.2: lo que el generador lee de la precondicion 6 y lo informativo'
P6_READS, P6_INFO_PKG = re.findall(r'`(\w+)`', _reads.group(1)), re.findall(r'`(\w+)`', _reads.group(2))
assert P6_READS == ['RedFiles', 'GreenFiles', 'RequiredTests', 'Totals'] and P6_INFO_PKG == ['Probe', 'NewLineDS19', 'CoverageExisting', 'Control'] and set(P6_INFO_PKG) <= set(P6_INFO), \
    ('6.5.2: lo que lee el generador', P6_READS, P6_INFO_PKG)
assert ('`%s`, sha256 `%s`' % (P6_REL_NAME, P6_SHA)) in _s65 and ('`%s`, `%s`' % (P6_CHECK_NAME, P6_CHECK_SHA)) in _s65, '6.5.1: los sha256 de la precondicion 6'
REQ_MEASURED = {}
if P6_RAW is None:
    pending('la precondicion 6 de PB-T4e (%s: en seco con --p6-file, o en la revision)' % P6_REL_NAME)
else:
    assert hashlib.sha256(P6_RAW).hexdigest() == P6_SHA, ('la precondicion 6 no es la que cita 6.5.1 (sha256)', P6_SRC)
    P6 = json.loads(P6_RAW.decode('utf-8'))
    assert list(P6) == P6_DECLARED, ('la precondicion 6: sus claves no son las de 6.5.2 en su orden', list(P6), P6_DECLARED)
    assert P6['Schema'] == P6_SCHEMA and P6['Package'] == PKG_VERSION and P6['Task'] == P6_TASK and P6['Revision']['Base'] == PT4D_CLOSE, \
        ('la precondicion 6: su forma, su version, su tarea o su revision', P6['Schema'], P6['Package'], P6['Task'], P6['Revision'])
    assert P6_CC is not None and hashlib.sha256(P6_CC).hexdigest() == P6_CHECK_SHA, ('la precondicion 6: compile-check.md no esta o no es el del paquete', P6_SRC)
    # las piezas, la lista vacia, las compilaciones
    assert {v['File']: v['Sha256'] for v in P6['Pieces'].values()} == PIECES, 'la precondicion 6: las piezas no son las de 6.5.2'
    assert (P6['List']['Rows'], P6['List']['K'], P6['List']['D'], P6['List']['E']) == (0, LIST_K, LIST_D, LIST_E), ('la precondicion 6: la lista', P6['List'])
    assert P6['CsWarnings'] == 0 and all(v == {'Rc': 0, 'Errors': 0, 'WarningsCS': 0} for c in P6['Builds'].values() for v in c.values()), \
        ('la precondicion 6: las compilaciones', P6['Builds'])
    P6_BUILDS = sum(len(c) for c in P6['Builds'].values())
    assert ('(%d compilaciones, 0 errores y 0 `warning CS`)' % P6_BUILDS) in _s65, ('6.5.1: el numero de compilaciones', P6_BUILDS)
    # RedFiles y GreenFiles (6.5.2: los lee): los del RED y del GREEN aplicados en memoria, y la base de cada uno de src/
    assert [(f['Path'], f['Sha256']) for f in P6['RedFiles']] == [(p, _sha(RED_TEXT[p])) for p in RED_NEW], 'la precondicion 6: RedFiles no son los del RED'
    assert sorted((f['Path'], f['Sha256'], f['BaseSha256']) for f in P6['GreenFiles']) == sorted((p, _sha(GREEN_TEXT[p]), _sha(GREEN_BASE[p])) for p in GREEN_TEXT), \
        'la precondicion 6: GreenFiles no son los del GREEN aplicado (ni su base)'
    # el RED y el GREEN: Core, y los filtros de UI cada uno en su proceso
    _ct = {p: [t['method'] for t in ts] for p, ts in NEW_TESTS.items()}
    _cls = lambda p: ('RackCad.UI.Tests.' if p.startswith(UT) else 'RackCad.Tests.') + os.path.basename(p)[:-3]
    _core_ct = sorted(_cls(p) + '.' + m for p, ms in _ct.items() if p.startswith(TR) for m in ms)
    _ui_ct = sorted(_cls(p) + '.' + m for p, ms in _ct.items() if p.startswith(UT) for m in ms)
    assert sorted(t['Test'] for t in P6['Red']['Core']['FailedTests']) == _core_ct and \
        all(t['FirstStackLine'].startswith('at ' + _cls([p for p in _ct if p.startswith(TR)][0]) + '.') for t in P6['Red']['Core']['FailedTests']), \
        'la precondicion 6: en el RED de Core solo caen las CT de Core, por asercion en su clase'
    _rui = [u for u in P6['Red']['UI'] if u['Failed']]
    assert sorted(t['Test'] for u in _rui for t in u['FailedTests']) == _ui_ct and all(u['Failed'] == len(u['FailedTests']) for u in _rui), \
        'la precondicion 6: en el RED de UI solo cae la CT de UI'
    assert P6['Green']['Core']['Failed'] == 0 and not P6['Green']['Core']['FailedTests'] and all(u['Failed'] == 0 for u in P6['Green']['UI']) and \
        P6['GreenWithCoverage']['Failed'] == 0, 'la precondicion 6: el GREEN sin Failed'
    P6_UI_FILTERS = [u['Filter'] for u in P6['Green']['UI']]
    assert [u['Filter'] for u in P6['Red']['UI']] == P6_UI_FILTERS and len(P6_UI_FILTERS) == len(set(P6_UI_FILTERS)), 'la precondicion 6: los filtros de UI'
    assert P6['Coverage']['Result'] == 'PASA' and P6['Outcome']['AllAsExpected'] is True and all(c['Ok'] is True for c in P6['Outcome']['Checks']), \
        ('la precondicion 6: la cobertura o lo previsto', P6['Coverage']['Result'], P6['Outcome'])
    # lo que el paquete cita literal de la corrida (6.5.1, "Lo medido"), frente al resumen
    _sd = P6['SuiteDurationSeconds']
    for _need in ('%s s en el RED y %s s en el GREEN (%s s con la cobertura)' % (_sd['Red'], _sd['Green'], _sd['GreenWithCoverage']),
                  P6['Coverage']['Summary'] + '; **PASA**; ' + P6['Coverage']['Drops'],
                  'UI red: %s y UI green: %s' % (P6['Totals']['UI']['Red'], P6['Totals']['UI']['Green'])):
        assert _need in _s65, ('6.5.1 no cita lo que da la precondicion 6', _need[:90])
    # la referencia de la cobertura (la base sin cambios, sola): Core es la base de la CI; la union de los filtros de UI, la que cita 6.5.1
    _bc, _bu = P6['BaseCoverage']['Core'], P6['BaseCoverage']['UIUnions'][0]
    _f = lambda x: '%d = %d + %d + %d' % (x['Total'], x['Passed'], x['Failed'], x['Skipped'])
    assert (_bc['Total'], _bc['Passed'], _bc['Failed'], _bc['Skipped']) == (CI_BASE_T4D[2], CI_BASE_T4D[2] - CI_BASE_T4D[3], 0, CI_BASE_T4D[3]) and \
        _f(_bc) == P6['Totals']['Core']['Base'] and _f(_bu) == P6['Totals']['UI']['BaseFiltersUnion'] and _bu['Failed'] == 0 and \
        ('la suite de Core, %s, y la uni' % _f(_bc)) in _s65 and ('19 filtros, %s)' % _f(_bu)) in _s65, ('la precondicion 6: la referencia de la cobertura', _bc, _bu)
    assert all(u['Failed'] == 0 for u in P6['BaseCoverage']['UIUnions'] + P6['BaseCoverage']['UIByFilter']), 'la precondicion 6: la base de UI sin Failed'
    assert [u['Filter'] for u in P6['BaseCoverage']['UIByFilter']] == P6_UI_FILTERS, 'la precondicion 6: los filtros de UI de la base'
    assert P6_COVB is not None and hashlib.sha256(P6_COVB).hexdigest() == P6['Coverage']['OutputSha256'] and P6['Coverage']['Output'] == P6_COV_NAME, \
        ('la precondicion 6: la salida de la cobertura no esta o no es la de Coverage.OutputSha256', P6_COV_NAME)
    # los RequiredTests medidos (6.5.2: los lee; required-pbt4e.tsv, por el sha256 de RequiredTests.Sha256)
    assert P6['RequiredTests']['Tsv'] == P6_REQ_NAME and P6_REQ is not None and hashlib.sha256(P6_REQ).hexdigest() == P6['RequiredTests']['Sha256'], \
        ('la precondicion 6: required-pbt4e.tsv', P6_SRC)
    _rq = P6_REQ.decode('utf-8').split('\n')
    _hd = ['n', 'Project', 'Filter', 'MinSelected', 'ExpectRed', 'origen', 'MinSelected-PBT4d', 'base', 'medido-RED', 'medido-GREEN']
    assert _rq[0].split('\t') == _hd, ('la cabecera de required-pbt4e.tsv', _rq[0][:80])
    _rows = [l.split('\t') for l in _rq[1:] if l.strip()]
    assert [dict(zip(_hd, r)) for r in _rows] == P6['RequiredTests']['Rows'] and P6['RequiredTests']['Count'] == len(_rows), \
        'la precondicion 6: RequiredTests.Rows no es required-pbt4e.tsv'
    for _c in _rows:
        _red = re.fullmatch(r'(\d+)(?: \(Failed (\d+)\))?', _c[8])
        REQ_MEASURED[int(_c[0])] = {'Project': _c[1], 'Filter': _c[2], 'MinSelected': int(_c[3]), 'ExpectRed': _c[4] == 'true', 'origen': _c[5],
                                    'prev': None if _c[6] == '-' else int(_c[6]), 'base': int(_c[7]), 'red': int(_red.group(1)),
                                    'red_failed': int(_red.group(2) or 0), 'green': int(_c[9])}
    assert any(l.endswith('solapamientos: 0 []') for l in P6['RequiredTests']['Log']), ('la precondicion 6: los filtros se solapan', P6['RequiredTests']['Log'])
    P6_EXTRA = []


# ================================================================ el contrato de PB-T4e desde 6.5, 6.5.1 y 6.5.2 de la v5.5
_b = [k for k, l in enumerate(PKG_LINES) if l.startswith('#### 6.5.1 ')]
assert len(_b) == 1, 'section 6.5.1'
_a = next(k for k in range(_b[0], len(PKG_LINES)) if PKG_LINES[k].strip() == '```text')
_z = next(k for k in range(_a + 1, len(PKG_LINES)) if PKG_LINES[k].strip() == '```')
BLOCK = PKG_LINES[_a + 1:_z]
assert all(ord(ch) < 0x80 for l in BLOCK for ch in l), 'el bloque de 6.5.1 no es ASCII'


def field(name):
    i = BLOCK.index(name + ':')
    out = []
    for l in BLOCK[i + 1:]:
        if not l.strip() or re.match(r'^[A-Za-z]+(?: \(propias\))?:$', l):
            break
        out.append(l)
    return out


assert BLOCK[0] == 'TaskId: ' + TASK, ('el TaskId de 6.5.1', BLOCK[0])
_ob = field('Objective')
assert len(_ob) == 1, 'el Objective de 6.5.1 en una linea'
OBJECTIVE = _ob[0]
for _need in ('paquete del contrato de PB-T4 ' + PKG_VERSION, 'CT-PBT4E-01 a 04', 'owner-pbt4e-list-decisions.md', 'sin esqueleto', 'la lista vacia',
              'no se ejecuta AutoCAD'):
    assert OBJECTIVE.count(_need) == 1, ('el Objective de PB-T4e sin', _need)
INV = field('Invariants')
assert [i.split('. ')[0] for i in INV] == ['INV-PBT4E-%02d' % n for n in range(1, 15)], [i[:12] for i in INV]
assert 'la lista cerrada de PB-T4e esta vacia (K = 0)' in INV[6], 'INV-PBT4E-07 sin la lista vacia'
assert PIECES[RED_PIECE] in INV[1] and PIECES[GREEN_PIECE] in INV[1] and ('dentro de las %d rutas' % N_ALLOWED) in INV[0], 'INV-PBT4E-01 y 02'
STOP_OWN_TEXT = field('StopConditions (propias)')
_sid = [s.split(': ', 1)[0] for s in STOP_OWN_TEXT]
assert _sid == ['PBT4E-C%02d' % n for n in list(range(1, 15)) + [16, 17, 18]], _sid
STOP_OWN = [{'Id': s.split(': ', 1)[0], 'Text': s.split(': ', 1)[1]} for s in STOP_OWN_TEXT]
_so = dict((s['Id'], s['Text']) for s in STOP_OWN)
# PBT4E-C11: el literal entero de 6.5.1 (la v5.6: el texto de PBT4D-C11 con la v3 del sustituto de nc2, F4B-E58; decision de la sesion), comprobado
C11 = C11_LITERAL = _so['PBT4E-C11']
for _need in ('the version 3 of PBTx-NC2-EXACT-WHITELIST-F4B-01', 'with --task PBT4 in the F4B-E21 mode', 'SATISFIED_UNDER_PBT4_EXACT_WHITELIST_SUBSTITUTE_F4B',
              'with --task PBT4, recorded as nc1 = SATISFIED_UNDER_PBT4_NC1_CASCADE_SUBSTITUTE', NC2_V3 + ', SHA-256 ' + NC2_V3_SHA,
              NC1_SUBST + ', SHA-256 ' + NC1_SUBST_SHA, 'an nc2 that the version 3 of the substitute does not give PASS'):
    assert C11.count(_need) == 1, ('PBT4E-C11 de 6.5.1', _need)
assert C11.endswith('no one-off acceptance extends to PB-T4e') and NC2_V2 not in C11 and NC2_V2_SHA not in C11, 'PBT4E-C11: la v3, no la v2'
C11_NAMES_V2 = False
assert 'IMPORTANT LIMIT' in _so['PBT4E-C17'], 'C17'
# C02: la lista cerrada de PB-T4e esta vacia: ninguna excepcion, ningun sha256, ninguna lista
_c02 = _so['PBT4E-C02']
assert _c02.count('The closed list of PB-T4e is empty: no exception.') == 1 and _c02.count('exception') == 1 and not re.findall(r'[0-9a-f]{64}', _c02) and \
    not re.findall(r'\.tsv\b', _c02), ('C02: la lista vacia, sin excepcion', _c02[:200])
assert '(the closed list of PB-T4e is empty)' in _so['PBT4E-C16'], 'C16 sin la lista vacia'
# C13: la lista cerrada de declaraciones nuevas (una), frente al GREEN aplicado
_c13 = re.search(r'this closed list \((\d+)\): the private constant (\w+) of (\w+);', _so['PBT4E-C13'])
assert _c13 and int(_c13.group(1)) == 1, 'C13: la lista cerrada de declaraciones'
_fvm = [p for p in GREEN_TEXT if p.endswith('/' + _c13.group(3) + '.cs')]
_decl = r'\bprivate\s+const\s+string\s+%s\b' % _c13.group(2)
assert len(_fvm) == 1 and len(re.findall(_decl, GREEN_TEXT[_fvm[0]])) == 1 and not re.findall(_decl, GREEN_BASE[_fvm[0]]), \
    ('C13: la constante nueva, una vez en el GREEN y ninguna en la base', _c13.groups())
# P-01 (la parada estandar que cambia para PB-T4e, 6.5.1 de la v5.6): el literal, con la Snapshot L; solo cambia la Snapshot frente a la de PB-T4d
_p01h = [k for k, l in enumerate(BLOCK) if l.startswith('StopConditions (estandar, la que cambia para PB-T4e')]
assert len(_p01h) == 1 and BLOCK[_p01h[0] + 1].startswith('P-01: ') and not BLOCK[_p01h[0] + 2].strip(), '6.5.1: P-01, la parada estandar que cambia'
P01_TEXT = BLOCK[_p01h[0] + 1].split(': ', 1)[1]
_k01 = [s['Text'] for s in STD_STOPS if s['Id'] == 'P-01']
assert len(_k01) == 1 and _k01[0].replace('today Snapshot K, ' + SNAP_K, 'today Snapshot L, ' + SNAP_L) == P01_TEXT != _k01[0], 'P-01: solo cambia la Snapshot'
STD_STOPS = [dict(s, Text=P01_TEXT) if s['Id'] == 'P-01' else s for s in STD_STOPS]
STOPS = STD_STOPS + STOP_OWN
assert len(STOPS) == 34, len(STOPS)
EVID = field('ExpectedEvidence')
assert [e.split('] ')[0] + ']' for e in EVID] == ['[%d]' % n for n in range(16)], [e[:4] for e in EVID]
EVID = [e.split('] ', 1)[1] for e in EVID]
assert EVID[0] == EVID0, 'la primera evidencia no es la del contrato de PB-T4d (la de PB-T3c)'
EVID13_OWNERS = [o for o in OWNER_E + [OWNER_ORIGIN, 'owner-pbt4d-list-decisions.md'] if o in EVID[13]]
assert '"## PB-T4e"' in EVID[13] and 'la lista vacia' in EVID[13] and ('(%s)' % SESSION_DS) in EVID[13] and \
    len(EVID13_OWNERS) == 3, ('[13], el registro declarado', EVID13_OWNERS)
_mods = dict((p, [n for n in PIECE_NAMES if n.startswith(p)][0][:-3]) for p in ('red_pbt4e_', 'apply_red_pbt4e_', 'green_pbt4e_', 'apply_green_pbt4e_'))
for _need in (_mods['red_pbt4e_'] + '.red', _mods['apply_red_pbt4e_'] + '.py', _mods['green_pbt4e_'] + '.green', _mods['apply_green_pbt4e_'] + '.py'):
    assert _need in EVID[15], ('[15] sin', _need)
_e14 = EVID[14]
for _need in ('run %s sobre su GREEN %s' % (CI_BASE_T4D[0], CI_BASE_T4D[1]), 'Core %s resultados, con %s Passed y %d Skipped' % (_sp(CI_BASE_T4D[2]),
              _sp(CI_BASE_T4D[2] - CI_BASE_T4D[3]), CI_BASE_T4D[3]), 'UI %s, con %s Passed y %d Skipped' % (_sp(CI_BASE_T4D[4]),
              _sp(CI_BASE_T4D[4] - CI_BASE_T4D[5]), CI_BASE_T4D[5]), 'PB-T4e anade %d CT de Core y %d de UI, sin Skip, sin equivalentes y sin bajas'
              % (N_CT_CORE, N_CT_UI)):
    assert _need in _e14, ('[14] sin', _need)

# ---------------------------------------------------------------- el alcance de 6.5 (6), ruta a ruta, por grupo, frente a las dos piezas
_al = [k for k, l in enumerate(SEC65) if l.startswith('- **`AllowedWriteScope`** (')]
assert len(_al) == 1 and int(re.search(r'\((\d+)\)', SEC65[_al[0]]).group(1)) == N_ALLOWED
ALLOWED, _nums, SCOPE = [], [], {}
for l in SEC65[_al[0] + 1:]:
    if not l.startswith('  - '):
        break
    _lab = re.match(r'  - \*\*(.+?)\*\*', l).group(1)
    _ps = re.findall(r'(\d+)\. `((?:src|tests)/[^`]+)`', l)
    if not _ps:
        assert _lab.startswith('ning'), ('un grupo del alcance sin rutas', _lab)
        continue
    _k = sum(int(x) for x in re.findall(r'\((?:UNIT_CHANGE, )?(\d+)[),]', _lab))
    assert len(_ps) == _k, ('el numero de un grupo del alcance', _lab)
    SCOPE[_lab] = [p for _, p in _ps]
    _nums += [int(n) for n, _ in _ps]
    ALLOWED += [p for _, p in _ps]
assert _nums == list(range(1, N_ALLOWED + 1)) and len(ALLOWED) == len(set(ALLOWED)) == N_ALLOWED, ('el AllowedWriteScope de 6.5', _nums)
_g = lambda pre: [v for k, v in SCOPE.items() if k.startswith(pre)]
assert len(SCOPE) == 2 and all(len(_g(p)) == 1 for p in ('src/, existentes que cambian', 'tests/, nuevos')), ('los dos grupos del alcance', list(SCOPE))
SRC_CHANGE, TESTS_NEW = _g('src/, existentes que cambian')[0], _g('tests/, nuevos')[0]
SRC_DELETE, TESTS_EDIT = [], []
UNIT_CHANGE_P = SRC_CHANGE
assert '- **`UNIT_CHANGE`:** las %d de `src/` (F4B-E25).' % len(UNIT_CHANGE_P) in SEC65, '6.5: UNIT_CHANGE'
assert sorted(SRC_CHANGE) == sorted(GREEN_TEXT) and sorted(TESTS_NEW) == sorted(RED_NEW), ('el alcance no es el de las dos piezas', list(GREEN_TEXT), RED_NEW)
assert all(in_head(a) for a in SRC_CHANGE) and not any(in_head(a) for a in TESTS_NEW), 'las de src/ existen en la base y las nuevas de tests/ no'
# el GREEN: 8 bloques EDIT en 4 archivos (6.5); el RED, 2 NEWFILE
assert ('4 archivos cambiados, %d bloques EDIT' % GREEN_BLOCKS) in _s65 and len(SRC_CHANGE) == 4 and RED_BLOCKS == len(RED_NEW) == 2, \
    ('6.5: el GREEN, sus archivos y sus bloques', GREEN_BLOCKS, RED_BLOCKS)
# los archivos de src/ del GREEN, con su sha256 y el de su base (6.5.1), frente a la pieza aplicada
_gs = dict((p, (h, hb)) for p, h, hb in re.findall(r'(?m)^- `(src/[^`]+\.cs)`: `([0-9a-f]{64})` \(base `([0-9a-f]{64})`\)$', _s65))
assert _gs == {p: (_sha(GREEN_TEXT[p]), _sha(GREEN_BASE[p])) for p in GREEN_TEXT}, '6.5.1: los archivos de src/ del GREEN y su base'
# los archivos nuevos, literales (6.5.1 y el anexo V55-A): su NewFileSha y su ExpectRed
NEWFILE_SHA, NEWFILE_ER = {}, {}
for _p, _h, _rest in re.findall(r'(?m)^- `(tests/[^`]+\.cs)`, `([0-9a-f]{64})`: (.*)$', _s65):
    NEWFILE_SHA[_p] = _h
    NEWFILE_ER[_p] = 'ExpectRed` true' in _rest
assert sorted(NEWFILE_SHA) == sorted(TESTS_NEW), ('6.5.1: los archivos nuevos', sorted(NEWFILE_SHA))
_av = '\n'.join(section('## Anexo %s-A' % VTAG.upper()))  # el anexo de la version (V56-A desde la v5.6)
_ann = dict(re.findall(r'(?m)^### `(tests/[^`]+\.cs)` \(`NewFileSha` `([0-9a-f]{64})`\)$', _av))
assert _ann == NEWFILE_SHA and all(_sha(RED_TEXT[p]) == h for p, h in NEWFILE_SHA.items()), 'NewFileSha: 6.5.1, el anexo de la version y la pieza del RED'
assert all(NEWFILE_ER.values()), ('ExpectRed true en las dos clases nuevas (5.7)', NEWFILE_ER)
SKIPS = []
_e14_ok = 'sin Skip' in _e14

# ---------------------------------------------------------------- el ForbiddenWriteScope (6.5): [PKC-C] en src/; en tests/, la lista explicita del contrato de
# PB-T4d (menos las rutas de tests/ de este alcance, que son nuevas: un prefijo que cubre una se parte), mas las cinco clases nuevas de PB-T4d tras su cierre
_t4d_new = [k for k in SEC64 if re.match(r'  - \*\*tests/, nuevos \(\d+\)', k)]
assert len(_t4d_new) == 1, '6.4: el grupo de los archivos nuevos de PB-T4d'
PREV_TESTS = re.findall(r'\d+\. `(tests/[^`]+)`', _t4d_new[0])
assert len(PREV_TESTS) == int(re.search(r'nuevos \((\d+)\)', _t4d_new[0]).group(1)) == 5 and all(p in pbt4d['AllowedWriteScope'] and in_head(p) for p in PREV_TESTS), \
    ('las cinco clases nuevas de PB-T4d (sus dos de las CT y sus tres que no son CT)', PREV_TESTS)
assert 'sus cinco clases nuevas: las dos de las CT y las tres que no son CT' in _s65, '6.5: el ForbiddenWriteScope'
SRCF = cover('src/')
TESTF, SPLIT2 = [], []
for f in [f for f in pbt4d['ForbiddenWriteScope'] if f.startswith('tests/')] + PREV_TESTS:
    if f in ALLOWED or f in TESTF:
        continue
    if f.endswith('/') and any(a.startswith(f) for a in ALLOWED):
        SPLIT2.append(f)
        TESTF += [x for x in cover(f) if x not in ALLOWED and x not in TESTF]
    else:
        TESTF.append(f)
ROOT = [f for f in pbt4d['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == len(set(FORBIDDEN)) and not set(FORBIDDEN) & set(ALLOWED), 'el ForbiddenWriteScope: sin repetidos ni rutas del alcance'
assert not any(a.startswith(f) for f in FORBIDDEN if f.endswith('/') for a in ALLOWED), 'un prefijo prohibido cubre una ruta del alcance'
TESTF_GONE = [f for f in TESTF if not in_head(f)]  # lo de la lista anterior que ya no esta en la revision (se dice en la salida)


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
    _r = [per_class(RED_FINAL[p]) for p in RED_FINAL if p.startswith(_root)]
    _sum = lambda ds, i: sum(v[i] for d in ds for v in d.values())
    DSTAT[_proj] = (_sum(_r, 0), _sum(_r, 1), 0, _sum(_r, 2))
assert (DSTAT['Core'][:2], DSTAT['UI'][:2]) == ((N_CT_CORE, 0), (N_CT_UI, 0)), ('el RED anade las CT de 5.7 y nada mas', DSTAT)

# ---------------------------------------------------------------- los RequiredTests de 6.5.1 (90): los heredados con su numero de PB-T4d y su MinSelected (ninguno
# baja: PB-T4e no quita nada), comprobado sobre la base, el RED y el GREEN (required-pbt4e.tsv de la precondicion 6) y por su recuento estatico; y los
# nuevos por clase exacta (D-S8)
_t = [k for k, l in enumerate(PKG_LINES) if l.startswith('| n.') and '| Project | Filter | MinSelected | ExpectRed | Origen |' in l and k > _z]
_t = [k for k in _t if k < [j for j, l in enumerate(PKG_LINES) if l.startswith('#### 6.5.2 ')][0]]
assert len(_t) == 1, '6.5.1: la tabla de RequiredTests'
REQ_PKG = []
for l in PKG_LINES[_t[0] + 2:]:
    m = re.match(r'^\| (\d+) \| `([^`]+)` \| `([^`]+)` \| (\d+) \| (true|false) \| (.+) \|$', l)
    if not m:
        break
    _o = re.match(r'PB-T4d n\.\S (\d+)$', m.group(6))
    REQ_PKG.append((int(m.group(1)), m.group(2), m.group(3), int(m.group(4)), m.group(5) == 'true', int(_o.group(1)) if _o else None, m.group(6)))
assert len(REQ_PKG) == N_REQ_ROWS and [r[0] for r in REQ_PKG] == list(range(1, N_REQ_ROWS + 1)), ('la tabla de RequiredTests de 6.5.1', len(REQ_PKG))
_inh = [r for r in REQ_PKG if r[5] is not None]
_new = [r for r in REQ_PKG if r[5] is None]
assert len(_inh) == N_REQ_INH and [r[5] for r in _inh] == list(range(1, N_REQ_INH + 1)) and _inh == REQ_PKG[:N_REQ_INH], 'los heredados, todos y en el orden de PB-T4d'
_T4D_REQ = dict((n, r) for n, r in enumerate(pbt4d['RequiredTests'], 1))
assert sorted(_T4D_REQ) == [r[5] for r in _inh], ('los heredados son todos los filtros del contrato de PB-T4d', len(_T4D_REQ))
NEW_CLASSES = [('RackCad.UI.Tests.' if p.startswith(UT) else 'RackCad.Tests.') + os.path.basename(p)[:-3] for p in TESTS_NEW]


def static_sel(proj, filt, base=False):
    _f = filt.split('~', 1)[1]
    sel = {k: v for k, v in (STATIC_BASE if base else STATIC)['UI' if proj == UIP else 'Core'].items() if k.startswith(_f)}
    if not sel:
        # como en PB-T4b a PB-T4d: count_static atribuye las pruebas de NumericFieldTests a su clase anidada BorderBrushSource, que va antes de ellas; se
        # cuenta por el archivo que declara la clase
        _cn = _f.rsplit('.', 1)[1]
        _fs = [p for p in tracked(UT if proj == UIP else TR) if p.endswith('.cs') and re.search(r'\bclass\s+%s\b' % _cn, show(p))]
        _ts = tests_of(show(_fs[0])) if len(_fs) == 1 else []
        return (sum(t['n'] for t in _ts if not t['skip'] and t['n'] > 0), sum(1 for t in _ts if t['n'] < 0 or (t['skip'] and t['n'] > 1)),
                ['<archivo> ' + p for p in _fs])
    return sum(v[0] for v in sel.values()), sum(v[2] for v in sel.values()), sorted(sel)


REQ_REPORT, REQ_STATIC, REQ_EQ = [], [0, 0], [0, 0]
for n, proj, filt, m, er, k, _orig in _inh:
    _r = _T4D_REQ[k]
    # todos con ExpectRed false (6.5.1), tambien el de CT-PBT4D-01 (n.o 84), que en PB-T4d era true; el MinSelected, el de PB-T4d (ninguno baja)
    assert (_r['Project'], _r['Filter']) == (proj, filt) and not er and m == _r['MinSelected'], ('un heredado no es el de PB-T4d con su numero', n, k, filt)
    _run, _md, _cls = static_sel(proj, filt)
    _run0, _md0, _ = static_sel(proj, filt, base=True)
    assert not any(c.startswith(filt.split('~', 1)[1]) for c in NEW_CLASSES), ('un filtro heredado selecciona una clase nueva', filt)
    assert (_run, _md) == (_run0, _md0), ('el RED cambia lo estatico de un filtro heredado', n, filt)
    REQ_STATIC[0 if (_md == 0 and _md0 == 0) else 1] += 1
    if P6 is not None:
        _q = REQ_MEASURED[n]
        assert (_q['Project'], _q['Filter'], _q['MinSelected'], _q['ExpectRed'], _q['origen'], _q['prev']) == (proj, filt, m, er, 'PB-T4d n.o %d' % k, m), \
            ('la fila %d frente a lo medido' % n, _q, m)
        # nada baja: la base, el RED y el GREEN seleccionan lo mismo, sin Failed, y al menos el MinSelected
        assert _q['base'] == _q['red'] == _q['green'] >= m > 0 and _q['red_failed'] == 0, ('un filtro heredado cambia o baja de su MinSelected', n, _q)
        assert (_md > 0 or _md0 > 0) or _q['green'] == _run, ('lo estatico de un filtro heredado no es lo medido', n, filt, _run, _q)
        REQ_EQ[0 if _q['green'] == m else 1] += 1
    if _r['ExpectRed']:
        REQ_REPORT.append('n.o %2d (PB-T4d %2d) %-4s %-60s ExpectRed true -> false (su CT pasa desde el cierre de PB-T4d)'
                          % (n, k, 'UI' if proj == UIP else 'Core', filt.split('.', 2)[-1]))
assert sorted(r[2].split('~', 1)[1] for r in _new) == sorted(NEW_CLASSES), ('los filtros nuevos no son las clases nuevas', [r[2] for r in _new])
for n, proj, filt, m, er, k, _orig in _new:
    _p = (UT if proj == UIP else TR) + filt.split('.')[-1] + '.cs'
    _c = per_class(RED_TEXT[_p])
    assert proj == (UIP if _p.startswith(UT) else CP) and list(_c) == [filt.split('~', 1)[1]] and _c[filt.split('~', 1)[1]] == [m, 0, 0] and \
        er == NEWFILE_ER[_p] and _orig.startswith('nuevo: CT-PBT4E-'), ('una fila nueva', n, filt, _c)
    if P6 is not None:
        _q = REQ_MEASURED[n]
        assert (_q['Project'], _q['Filter'], _q['MinSelected'], _q['ExpectRed'], _q['base'], _q['red'], _q['red_failed'], _q['green']) == \
            (proj, filt, m, er, 0, m, m, m), ('una fila nueva medida', n, _q)
assert P6 is None or sorted(REQ_MEASURED) == list(range(1, N_REQ_ROWS + 1)), 'required-pbt4e.tsv: las 90 filas'
REQ = [(p, f, m, e) for _, p, f, m, e, _, _ in REQ_PKG]
_er = [r for r in REQ if r[3]]
assert sorted(r[1].split('~', 1)[1] for r in _er) == sorted(NEW_CLASSES), ('ExpectRed true, solo los dos filtros nuevos', _er)
REQ_UI = [r[1] for r in REQ if r[0] == UIP]
assert P6 is None or sorted(REQ_UI) == sorted(P6_UI_FILTERS), ('los filtros de UI de la precondicion 6 no son los del contrato', len(REQ_UI))
assert ('los %d filtros de UI del contrato' % len(REQ_UI)) in _s65, ('6.5.1: el numero de filtros de UI', len(REQ_UI))

# ---------------------------------------------------------------- los totales de cada rama: la base de PB-T4d (F4B-E59), lo que mide la precondicion 6 sobre el
# GREEN, y el RED desde el GREEN con los Failed de los filtros con ExpectRed; frente a CI_T4E_EXPECTED, a la precondicion 6 y a [14], por expresion
_tot = lambda s: tuple(int(x) for x in re.fullmatch(r'(\d+) = (\d+) \+ (\d+) \+ (\d+)', s).groups())
if P6 is not None:
    _gc, _ut = P6['Green']['Core'], _tot(P6['Totals']['UI']['Green'])
    _GREEN = (_gc['Total'], _gc['Passed'], _gc['Failed'], _gc['Skipped'], _ut[0], _ut[1], _ut[2], _ut[3])
    assert _tot(P6['Totals']['Core']['Green']) == _GREEN[:4], 'la precondicion 6: Totals.Core.Green no es Green.Core'
    CI_SRC = 'la precondicion 6'
else:
    _GREEN = CI_T4E_EXPECTED['GREEN']
    CI_SRC = 'CI_T4E_EXPECTED (sin la precondicion 6)'
_fc = sum(r[2] for r in REQ if r[3] and r[0] == CP)
_fu = sum(r[2] for r in REQ if r[3] and r[0] == UIP)
assert (_fc, _fu) == (RED_FAIL_CORE, RED_FAIL_UI), ('los Failed del RED: los MinSelected de los filtros con ExpectRed', _fc, _fu)
CI_T4E = {'GREEN': _GREEN, 'RED': (_GREEN[0], _GREEN[1] - _fc, _fc, _GREEN[3], _GREEN[4], _GREEN[5] - _fu, _fu, _GREEN[7])}
for _k, (_t, _p, _f, _sk, _ut, _up, _uf, _usk) in CI_T4E.items():
    assert _t == _p + _f + _sk and _ut == _up + _uf + _usk, ('la aritmetica de las ramas', _k)
assert CI_T4E['GREEN'][2] == 0 and CI_T4E['GREEN'][6] == 0, 'el GREEN sin Failed'
assert CI_T4E == CI_T4E_EXPECTED, ('los totales no son los de la tabla del generador (CI_T4E_EXPECTED)', CI_T4E, CI_T4E_EXPECTED)
# la base mas lo que anade el RED (las CT nuevas, sin Skip ni bajas): la del GREEN
assert (CI_T4E['GREEN'][0], CI_T4E['GREEN'][3], CI_T4E['GREEN'][4], CI_T4E['GREEN'][7]) == (CI_BASE_T4D[2] + N_CT_CORE, CI_BASE_T4D[3], CI_BASE_T4D[4] + N_CT_UI,
                                                                                         CI_BASE_T4D[5]), ('el GREEN no es la base mas las CT nuevas', CI_T4E)
if P6 is not None:
    _rc = P6['Red']['Core']
    assert (_rc['Total'], _rc['Passed'], _rc['Failed'], _rc['Skipped']) + _tot(P6['Totals']['UI']['Red']) == CI_T4E['RED'] and \
        _tot(P6['Totals']['Core']['Red']) == CI_T4E['RED'][:4], ('la precondicion 6: el RED', _rc)
    assert _tot(P6['Totals']['UI']['BaseCI']) == (CI_BASE_T4D[4], CI_BASE_T4D[4] - CI_BASE_T4D[5], 0, CI_BASE_T4D[5]), 'la precondicion 6: la base de UI de la CI'
for _k, _lbl in (('RED', 'El RED: '), ('GREEN', 'El GREEN: ')):
    _t = CI_T4E[_k]
    _nc = (' (%s)' % RED_CT_NOTE['Core']) if _t[2] else ''
    _nu = (' (%s)' % RED_CT_NOTE['UI']) if _t[6] else ''
    _need = ('%sCore %d resultados, con %d Passed, %d Failed y %d Skipped%s; UI %d resultados, con %d Passed, %d Failed y %d Skipped%s'
             % (_lbl, _t[0], _t[1], _t[2], _t[3], _nc, _t[4], _t[5], _t[6], _t[7], _nu))
    assert _need in _e14, ('[14] no da los totales', _k, _need)
    assert ('%d = %d + %d + %d' % (_t[0], _t[1], _t[2], _t[3])) in _s65, ('6.5.1, lo medido, sin el Core de la rama', _k)
# 5.7: la tabla de los totales en cifras
for _k in ('GREEN', 'RED'):
    _t = CI_T4E[_k]
    assert re.search(r'(?m)^\| \*\*%s\*\* \| \*\*%d = %d \+ %d \+ %d\*\*(?: \([^)]*\))? \| \*\*%d = %d \+ %d \+ %d\*\* \|$' % ((_k,) + _t), '\n'.join(SEC57)), \
        ('5.7: la tabla de los totales', _k)
DELTA = {'Core': (CI_T4E['GREEN'][0] - CI_BASE_T4D[2], CI_T4E['GREEN'][3] - CI_BASE_T4D[3]), 'UI': (CI_T4E['GREEN'][4] - CI_BASE_T4D[4], CI_T4E['GREEN'][7] - CI_BASE_T4D[5])}


# ---------------------------------------------------------------- las autoridades (6.5.2): las del contrato de PB-T4d, por su numero, con AUTH_PLAN (la
# propuesta, pendiente de la sesion: 6.5.2 no da los numeros)
_A = pbt4d['Authorities']
assert len(_A) == 62, ('las autoridades del contrato de PB-T4d', len(_A))
for _i, _end in ((0, PKG), (6, 'superseding-characterization.md'), (28, 'f4b-dispositions.md'), (29, 'F4B-PBT4-EDIT-ALL-WINDOW/coordinator/gate-contract.json'),
                 (36, 'F4B-PBT4-EDIT-ALL-WINDOW/task-close-package.md'), (37, T4D_TASK + '/coordinator/gate-contract.json'),
                 (14, 'owner-config-snapshot-k-decision.md'), (22, 'owner-pbt3a-nc2-decision.md'), (23, 'nc2_exact_whitelist_substitute_f4b_v2.py'),
                 (24, 'nc2-substitute-f4b-v2/limits-declared.md'), (25, 'independent-review-substitute-f4b-v2.md'), (43, 'independent-review-pbt4d-check.md'), (44, 'owner-pbt4b-code-decisions.md'), (45, 'owner-pbt4d-list-decisions.md'),
                 (49, 'owner-pbt4d-cat4d-decisions.md'), (50, 'session-pbt4d-design-decisions.md'), (51, 'pbt4d-retire-list-271.tsv'),
                 (54, 'pbt4d-precondition6.json'), (55, 'cc4d/compile-check.md')):
    assert _A[_i]['Path'].endswith(_end), ('la autoridad n.o %d del contrato de PB-T4d no es la que se espera' % _i, _A[_i]['Path'])
assert _A[37]['Section'] == 'este contrato' and all(a['Class'] == 'UNIT_CHANGE' for a in _A[56:62]), 'la n.o 37 y los UNIT_CHANGE de PB-T4d (56 a 61)'
assert all(_A[i]['Path'].startswith(D4) and _A[i]['Class'] == 'UNIT_DOC' for i in range(50, 56)), 'las n.o 50 a 55: la sesion, la lista, las piezas y la P6 de PB-T4d'
# las que se anaden (6.5.2): la 15.a tanda, su origen, las decisiones de la sesion, CA-T4e (en la n.o 43), esta version (la n.o 0), las piezas y la P6
_add = re.search(r'\*\*Se a\S+aden:\*\* (.*)$', _s652, re.M).group(1)
for _need in ('`%s` (la 15.' % OWNER_E[0], '`%s` (pregunta 2, el origen)' % OWNER_ORIGIN, 'las decisiones de la sesi', '%s (`%s`, sha256 `%s`)' % (SESSION_DS, SESSION_DEC_E, SESSION_DEC_SHA),
              'la comprobaci', 'esta versi', '(%s)' % PKG_VERSION, 'las dos piezas', '(`%s` y `%s`)' % (P6_REL_NAME, P6_CHECK_NAME)):
    assert _need in _add, ('6.5.2: lo que se anade', _need)
for _p in (CO + OWNER_ORIGIN, CO + 'owner-pbt4d-list-decisions.md'):
    assert in_head(_p), ('el origen de PB-T4e, ya custodiado', _p)
_sb, _ = local_or_rev(SESSION_DEC_E)
assert _sb is not None and hashlib.sha256(_sb).hexdigest() == SESSION_DEC_SHA, ('el registro de la sesion no es el que cita 6.5.2', SESSION_DEC_E)
for _p in (NC2_V3, NC2_V3_REVIEW, NC2_V3_DEC, SNAP_L, SNAP_L_DEC):
    assert ('`%s`' % _p) in _add and in_head(_p), ('6.5.2: se anade, y esta en la revision', _p)
assert ('`%s`, sha256 `%s`, con el `config.toml` `%s` y %d nombres' % (SNAP_L, SNAP_L_FILE_SHA, SNAP_L_SHA, SNAP_L_NAMES)) in _add, '6.5.2: la Snapshot L'
_auth = []
for _i, a in enumerate(_A):
    if _i in AUTH_PLAN['drop']:
        continue
    a = dict(a)
    if _i == 0:
        a['Section'] = ('archivo completo (' + PKG_VERSION + ', sha256 ' + CUSTODY_SHA256[PKG][:8] + '): D-PBT4-22, section 4.5, 5.7, 6.0 y 6.5 (el contrato de '
                        'PB-T4e, sus literales de 6.5.1 y lo que necesita el generador, 6.5.2) y el anexo ' + VTAG.upper() + '-A')
    elif _i == 6:
        a['Section'] = 'la entrada "## PB-T4e" (la lista vacia, K = 0, D = 0 y E = 0; las dos clases nuevas con su NewFileSha; su tabla al cerrar; [13])'
    elif _i == 28:
        a['Section'] = ('hasta ' + T4D_CLOSE_ENTRY + ' (el cierre de PB-T4d; F4B-E57, la Snapshot L, y F4B-E58, la version 3 del sustituto de nc2); ' + EMIT_ENTRY +
                        ', la emision de PB-T4e')
    elif _i == 29:
        a['Path'], a['Section'] = PBT4DC, 'el contrato anterior, el de PB-T4d (emitido; sus textos comunes, C11 y sus filtros)'
    elif _i == 36:
        a['Path'], a['Section'] = T4D_TCP, 'la base: el cierre de PB-T4d (' + T4D_CLOSE_ENTRY + '), con la CI de la linea base (' + CI_BASE_T4D[0] + ')'
    elif _i == 37:
        a['Path'], a['Section'] = PILOT + '/' + TASK + '/coordinator/gate-contract.json', 'este contrato'
    elif _i == AUTH_PLAN['ca_at']:
        a['Path'], a['Section'] = D4 + BOUNDED_NAME, ('la comprobacion acotada de PB-T4e (CA-T4e), sobre el contrato en seco y la ' + CAT4E_PKG +
                                                     ', y sus recomprobaciones sobre la ' + PKG_VERSION + ' (si las hay)')
    _auth.append(a)
_have = {a['Path'] for a in _auth}
for _p, _s in ([(NC2_V3, 'la version 3 del sustituto de nc2 (F4B-E58), SHA-256 ' + NC2_V3_SHA + ': el de PBT4E-C11, revisado APTO'),
                (NC2_V3_REVIEW, 'la revision independiente de la version 3 del sustituto de nc2: APTO'),
                (NC2_V3_DEC, 'archivo completo (F4B-E58): la decision del Owner, Sustituto v3 y revision (decision del Owner, literal)'),
                (SNAP_L, 'la Snapshot L (F4B-E57), sha256 ' + SNAP_L_FILE_SHA[:8] + ', con el config.toml ' + SNAP_L_SHA + ' y %d nombres: la de P-01' % SNAP_L_NAMES),
                (SNAP_L_DEC, 'archivo completo (F4B-E57): la Snapshot L, con la respuesta literal del Owner'),
                (CO + OWNER_E[0], 'archivo completo (' + EMIT_ENTRY + '): la 15.a tanda: Solo la recarga, No abrir y avisar, Antes de PB-T5 y Si, con condiciones '
                 '(la emision condicional; decision del Owner, literal)'),
                (CO + OWNER_ORIGIN, 'pregunta 2 (Aceptarlo y corregir luego): OWNER-02 de la revision del codigo de PB-T4c, el origen de PB-T4e'),
                (D4 + SESSION_DEC_E, 'archivo completo (' + EMIT_ENTRY + '): las decisiones de diseno de la sesion, ' + SESSION_DS + ', el TaskId y R-T4-39')] +
               [(D4 + _n, 'una pieza literal de PB-T4e (6.5.2), sha256 %s' % PIECES[_n][:8]) for _n in (RED_PIECE, GREEN_PIECE)] +
               [(P6_REL, 'la precondicion 6 de la sesion: las piezas, la lista vacia, el RED y el GREEN, los totales, la cobertura y los RequiredTests'),
                (P6_CHECK_REL, 'el registro de la precondicion 6 (sha256 %s): las corridas y el resultado de cada clase en cada rama' % P6_CHECK_SHA[:8])]):
    if _p not in _have:
        _auth.append({'Path': _p, 'Section': _s, 'Class': 'UNIT_DOC'})
_auth += [{'Path': p, 'Section': 'el texto literal del GREEN de PB-T4e (' + GREEN_PIECE + '; D-PBT4-22): cambia', 'Class': 'UNIT_CHANGE'} for p in UNIT_CHANGE_P]

# ---------------------------------------------------------------- el contrato de PB-T4e
c = {}
for k in pbt4d:
    c[k] = pbt4d[k]
STOPS_C = [dict(s) for s in STOPS]  # PBT4E-C11: el literal de 6.5.1 hasta que la sesion decida (SESSION_OPEN)
c.update({'TaskId': TASK, 'Objective': OBJECTIVE, 'AuthorityRevision': AR, 'MainSha': MAIN, 'Authorities': _auth, 'AllowedWriteScope': ALLOWED,
          'ForbiddenWriteScope': FORBIDDEN, 'Invariants': INV,
          'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
          'ExpectedEvidence': EVID, 'StopConditions': STOPS_C,
          'IssuedBy': ('Sesion principal de I-64 (Claude), emision de la quinta subtarea de PB-T4 (' + EMIT_ENTRY + ') por ' + OWNER_E[0] + ', pregunta 4 '
                       '(la emision condicional de PB-T4e), con la lista vacia (ninguna prueba existente sustituida; K = %d, D = %d y E = %d): PB-T4e tras el '
                       'cierre de PB-T4d (' % (LIST_K, LIST_D, LIST_E) + PT4D_CLOSE[:8] + ', ' + T4D_CLOSE_ENTRY + '), con su comprobacion acotada (' +
                       (BOUNDED_RESULT or '<resultado por rellenar>') + '); el origen, ' + OWNER_ORIGIN + ', pregunta 2; las decisiones de diseno de la '
                       'sesion (' + SESSION_DS + '); F4B-E46; F4B-E40 y F4B-E58, los sustitutos de nc1 y nc2 (su version 3); F4B-E57, la Snapshot L; F4B-E29, D-1; F4B-E11, D2; F4B-E35, la regla RR; '
                       'F4B-E21; F4B-E25, con las %d de src/ que cambian; y el paquete del contrato de PB-T4 ' % len(UNIT_CHANGE_P) + PKG_VERSION),
          'IssuedUtc': ISSUED})
assert list(c) == list(pbt4d), 'las claves del contrato de PB-T4e no son las del de PB-T4d'
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


# ---- bounded_check_t4e (inicio)
def bounded_check_t4e(report, result, pkg_lines, reviewer='CA-T4e'):
    """bounded_check (el de make_pbt3a_contract.py) adaptado a la forma del informe propio de CA-T4e, independent-review-pbt4e-check.md (con la forma del de CA-T4d) (decision de la
    sesion: sin aflojar lo que comprueba). Lo que cambia es solo la forma:
    - el veredicto de CA-T4e, antes de la primera recomprobacion: '**<veredicto>. B n / I n / M n.**' (con o sin 'Veredicto: ');
    - cada recomprobacion: '## Recomprobacion <n> (vX.Y)', con su veredicto en su seccion: '**Veredicto:** <veredicto>, B n / I n / M n';
    - BOUNDED_RESULT es, caracter a caracter, la linea BOUNDED del informe ('> EMITIBLE...') mas la cola de cada recomprobacion ('> ; recomprobacion
      vX.Y: <veredicto>; B n / I n / M n ...'), sin el '> ', en su orden: se copia del archivo, no se escribe a mano.
    Lo que comprueba, como bounded_check: solo la familia EMITIBLE; 0 bloqueantes; BOUNDED_RESULT empieza por el veredicto y lleva sus B/I/M; cada
    importante con su fila en los hallazgos y una fila del registro de cambios del paquete que la nombra (**CA-T4e I-nn**, con o sin dos puntos), y resuelta
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
# ---- bounded_check_t4e (fin)


# ---------------------------------------------------------------- la comprobacion acotada de PB-T4e (CA-T4e): BOUNDED_RESULT y bounded_check_t4e (la forma del
# informe de CA-T4d, sin aflojar lo que comprueba), sobre el informe propio de CA-T4e; se lee antes de decidir su pendiente
BOUNDED_CHECK = D4 + BOUNDED_NAME
BOUNDED_SRC, _rep = None, None
if BOUNDED_FILE:
    _rep = open(BOUNDED_FILE, 'rb').read().decode('utf-8')
    BOUNDED_SRC = '--bounded-file ' + os.path.basename(BOUNDED_FILE)
elif in_head(BOUNDED_CHECK):
    _rep = show(BOUNDED_CHECK)
    BOUNDED_SRC = BOUNDED_CHECK + ' en ' + REV_SHA[:8]
if BOUNDED_SRC is None:
    soft(False, 'emision: la comprobacion acotada de PB-T4e (%s) no esta ni en la revision ni en --bounded-file' % BOUNDED_CHECK)
if BOUNDED_RESULT is None:
    pending('la comprobacion acotada de PB-T4e (CA-T4e, %s): BOUNDED_RESULT, el veredicto literal, copiado del informe' % (BOUNDED_SRC or 'sin informe'))
elif BOUNDED_SRC is not None:
    assert re.match(r'EMITIBLE(?: TRAS CORRECCIONES)?[.,;]', BOUNDED_RESULT), ('BOUNDED_RESULT: solo la familia EMITIBLE', BOUNDED_RESULT)
    assert 'CA-T4e' in _rep and CAT4E_PKG in _rep, ('el informe no es el de CA-T4e sobre la ' + CAT4E_PKG, BOUNDED_SRC)
    # si hay recomprobaciones, la ultima es la de la version del paquete (como la precondicion 11 de PB-T3a): si no, la emision espera a la recomprobacion
    _vers = re.findall(r'(?m)^## Recomprobaci\S+n \d+ \((v\d+(?:\.\d+)?)(?:, [^)]*)?\)\s*$', _rep.replace('\r\n', '\n'))
    soft((_vers[-1] if _vers else CAT4E_PKG) == PKG_VERSION, 'emision: la ultima comprobacion de CA-T4e (%s) no es de la %s' % ((_vers or [CAT4E_PKG])[-1], PKG_VERSION))
    _err = bounded_check_t4e(_rep, BOUNDED_RESULT, PKG_LINES)
    assert not _err, ('la comprobacion acotada de PB-T4e', BOUNDED_SRC, _err)
_sup = show(PILOT + '/F4B-DRAFTS/superseding-characterization.md')
soft('\n## PB-T4e ' in _sup, 'emision: el registro declarado aun no tiene la entrada de PB-T4e (la lista vacia y las dos clases nuevas, con su tabla al cerrar)')
# las decisiones que esta version deja a la sesion (SESSION_OPEN), despues de leer todo lo demas: al emitir abortan
for _q in SESSION_OPEN:
    pending('decision de la sesion: ' + _q)
# la n.o 37, "este contrato" (6.5.2), no esta en X por construccion: lo escribe la BASE. No cuenta como autoridad que falta (al emitir sobre X abortaria)
SELF_CONTRACT = PILOT + '/' + TASK + '/coordinator/gate-contract.json'
_self37 = [i for i, a in enumerate(c['Authorities']) if a['Section'] == 'este contrato']
assert len(_self37) == 1 and c['Authorities'][_self37[0]]['Path'] == SELF_CONTRACT, ('este contrato, por su seccion (6.5.2; la n.o 37 de PB-T4d)', _self37)
assert [a['Path'] for a in c['Authorities']].count(SELF_CONTRACT) == 1
AUTH_MISSING = [a['Path'] for a in c['Authorities'] if not in_head(a['Path']) and a['Path'] != SELF_CONTRACT]

print('DRY (FASE 2) rev', REV_SHA[:8], '(objetos del checkout principal)' if OBJ_REV else '(HEAD del worktree)', 'TaskId', TASK,
      '| contrato en seco: PENDIENTES', len(PENDING), 'AVISOS', len(WARN))
print('anclaje: desciende de %s y %s (PB-T4d, %s; X y BASE), de su RED %s, de su GREEN %s y de su cierre %s; src y tests == %s'
      % (PT4D_EMIT_X[:8], PT4D_EMIT[:8], T4D_EMIT_ENTRY, RED_T4D[:8], GREEN_T4D[:8], PT4D_CLOSE[:8], GREEN_T4D[:8]))
print('numeracion prevista: %s (la emision); la evidencia %s (X) y %s (BASE) | hoy: F4B-E%d y la %s' % (EMIT_ENTRY, X_EV, BASE_EV, _ids[-1], _evs[-1]))
print('la CI del GREEN de PB-T4d (%s, la base): Core %d = %d + 0 + %d, UI %d = %d + 0 + %d' % (CI_BASE_T4D[0], CI_BASE_T4D[2], CI_BASE_T4D[2] - CI_BASE_T4D[3],
                                                                                        CI_BASE_T4D[3], CI_BASE_T4D[4], CI_BASE_T4D[4] - CI_BASE_T4D[5], CI_BASE_T4D[5]))
print('contrato de PB-T4d (%s, AuthorityRevision %s): 17 paradas estandar (P-01, la literal de 6.5.1, con la Snapshot L); PBT4E-C11, el literal de 6.5.1 (%d '
      'caracteres; nombra la v2 de nc2: %s); EVID[0] en F4B_EXACT_EVIDENCES de nc2 v2 y v3; %d filtros; la Snapshot L comprobada (C11: %d caracteres)'
      % (PBT4DC, pbt4d['AuthorityRevision'][:8], len(C11), C11_NAMES_V2, len(_T4D_REQ), len(C11_LITERAL)))
print('6.5 de la %s (%s): Objective %d caracteres, ASCII; alcance %d rutas (src/: %d que cambian, UNIT_CHANGE; tests/: %d nuevas); ningun borrado'
      % (PKG_VERSION, PKG_SRC, len(OBJECTIVE), len(ALLOWED), len(SRC_CHANGE), len(TESTS_NEW)))
print('las piezas (6.5.2, por sha256): %s' % '; '.join('%s %s' % (k, v[:8]) for k, v in PIECES.items()))
print('el RED en memoria: %d bloques NEWFILE (NewFileSha iguales, 6.5.1 y el anexo de la version); el GREEN: %d bloques EDIT en %d archivos (sus sha256 y los de su '
      'base, los de 6.5.1); cada OLD una vez; C13: %s, una declaracion nueva' % (RED_BLOCKS, GREEN_BLOCKS, len(GREEN_TEXT), _c13.group(2)))
print('[13] nombra "## PB-T4e", la lista vacia, %s y %d registros del Owner (%s)' % (SESSION_DS, len(EVID13_OWNERS), ', '.join(EVID13_OWNERS)))
print('la lista: vacia (4.5 y 6.5.2; K = %d, D = %d y E = %d); el RED no edita ni borra ningun archivo existente de tests/; %d CT nuevas, Fact sin Skip; C02 sin '
      'excepcion' % (LIST_K, LIST_D, LIST_E, sum(len(ts) for ts in NEW_TESTS.values())))
print('el recuento estatico del RED frente a la base: Core ejecutables %+d, saltadas %+d; UI %+d, %+d' % (DSTAT['Core'][0], DSTAT['Core'][1], DSTAT['UI'][0],
                                                                                                    DSTAT['UI'][1]))
if P6 is not None:
    print('la precondicion 6 (%s, sha256 %s; compile-check.md %s): las 23 claves de 6.5.2 en su orden; Package %s y Task con su nota; %d piezas, la lista vacia, '
          '%d archivos del RED y %d del GREEN con su sha256 (y el de su base), %d compilaciones con 0 avisos CS, en el RED solo caen las %d CT de Core y la de '
          'UI, el GREEN sin Failed (Core y %d filtros de UI), cobertura %s, Outcome %s; leidos: %s; informativos, sin comprobar: %s' % (
              P6_SRC, P6_SHA[:8], P6_CHECK_SHA[:8], P6['Package'], len(PIECES), len(P6['RedFiles']), len(P6['GreenFiles']), P6_BUILDS, N_CT_CORE,
              len(P6_UI_FILTERS), P6['Coverage']['Result'], P6['Outcome']['AllAsExpected'], ', '.join(P6_READS), ', '.join(P6_INFO)))
    print('  informativo, Control: campo Core %s UI %s; anfitrion Core %s UI %s' % tuple(
        '%d = %d + %d + %d' % (x['Total'], x['Passed'], x['Failed'], x['Skipped']) for x in (P6['Control']['Campo']['Core'], P6['Control']['Campo']['UI'],
                                                                                        P6['Control']['Anfitrion']['Core'], P6['Control']['Anfitrion']['UI'])))
print('RequiredTests: %d (%d heredados de PB-T4d con su numero y su MinSelected, ninguno baja; %d sin Theory que el texto no cuente, %d con ellas; %s; %d '
      'nuevos por clase exacta); con ExpectRed: %s' % (len(REQ), len(_inh), REQ_STATIC[0], REQ_STATIC[1],
                                                       ('MinSelected igual a lo medido en %d, por debajo en %d' % tuple(REQ_EQ)) if P6 is not None else 'sin medir',
                                                       len(_new), '; '.join('%s %d' % (r[1].split('.')[-1], r[2]) for r in _er)))
for _l in REQ_REPORT:
    print('  ' + _l)
print('la CI prevista (desde %s; RED = GREEN con los Failed de los filtros con ExpectRed): RED Core %d = %d + %d Failed + %d, UI %d = %d + %d Failed + %d; GREEN '
      'Core %d = %d + 0 + %d, UI %d = %d + 0 + %d; iguales a CI_T4E_EXPECTED, a la precondicion 6, a [14] y a 5.7' % ((CI_SRC,) + CI_T4E['RED'] + (CI_T4E['GREEN'][0],
      CI_T4E['GREEN'][1], CI_T4E['GREEN'][3], CI_T4E['GREEN'][4], CI_T4E['GREEN'][5], CI_T4E['GREEN'][7])))
print('frente a la base: Core %+d resultados (%+d saltados), UI %+d (%+d): las CT nuevas' % (DELTA['Core'][0], DELTA['Core'][1], DELTA['UI'][0], DELTA['UI'][1]))
print('CONTRATO (en seco) allowed %d forbidden %d (src %d tests %d [prefijos partidos: %s; las cinco de PB-T4d anadidas; ya no estan en la revision: %d] root %d) '
      'inv %d stops %d required %d (de %d) evidence %d authorities %d bytes %d sha256 %s'
      % (len(ALLOWED), len(FORBIDDEN), len(SRCF), len(TESTF), ', '.join(SPLIT2) or 'ninguno', len(TESTF_GONE), len(ROOT), len(INV),
         len(STOPS_C), len(REQ), N_REQ_ROWS, len(EVID), len(c['Authorities']), len(b), hashlib.sha256(b).hexdigest().upper()))
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
# ---------------------------------------------------------------- la emision (sin --dry): en el worktree, sobre el commit X, como la de PB-T4d; todo lo que
# en seco es AVISO o PENDIENTE ya abortaba (AS_WRITE), y la precondicion 6 y CA-T4e se leen de la revision antes de decidirlos
assert OBJ_REV is None and not PKG_FILE and not P6_FILE and not BOUNDED_FILE and not JSON_OUT, \
    'al emitir, solo el worktree: sin --objects-rev, --package-file, --p6-file, --bounded-file ni --json-out'
assert REV_SHA == AR, ('al emitir, la AuthorityRevision es el commit X, el HEAD del worktree', AR[:8], REV_SHA[:8])
assert PKG_VERSION == PKG_TARGET and BOUNDED_RESULT and BOUNDED_SRC and E60_OK and P6 is not None and not SESSION_OPEN and \
    in_head(PKG) and hashlib.sha256(show_bytes(PKG)).hexdigest() == CUSTODY_SHA256[PKG], 'faltan la v5.5, CA-T4e, F4B-E60, la precondicion 6, las decisiones o el paquete'
assert not PENDING and not WARN and not AUTH_MISSING, (PENDING, WARN, AUTH_MISSING)
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', "Get-Content -Raw '%s' | Test-Json -SchemaFile "
                    "'docs/automation/agent-execution/schemas/gate-contract.schema.json'" % OUT], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True', 'sha256', hashlib.sha256(b).hexdigest().upper())
sys.exit(0)
