"""Contrato de gate de F4B-PBT3C-FULL-WRITE-ERASE (ASCII), desde el paquete del contrato de PB-T3 v4.7 (section 3, D-PBT3-11 y D-2; section 4 y 4.1;
section 5.0 y 5.4; section 6.0 y 6.3; precondicion 13), con el patron de make_pbt3b_contract.py. La base es el cierre de PB-T3b (fe1c67f4, F4B-E46):
lo que en el esbozo eran marcadores (PT3B_CLOSE, GREEN_T3B, CI_BASE_T3B, MINSEL_T3B) son los hechos medidos en el cierre, comprobados aqui por
objetos git; la fila de RR-3 de CT-05 la enumera la v4.7. La emision de PB-T3c es F4B-E47, con la evidencia 274 (X) y 275 (BASE), y la remedicion,
R-23. La CI prevista lleva el total exacto de su unica rama (F4B-E46). Lo que entrega el commit X de la emision (la v4.7 custodiada, F4B-E47, la
remedicion y R-23, CA-T3c y BOUNDED_RESULT, la entrada de D-2) es AVISO en seco y error al escribir.
El ForbiddenWriteScope de src/ sale del algoritmo de [PKC-C] con el AllowedWriteScope de 4 rutas (WR, la fachada y las dos clases); el de tests/ es el
del contrato reemitido de PB-T3b mas sus cinco archivos de pruebas, con el prefijo tests/RackCad.Tests/Workspace/ otra vez entero (section 6.0: "en
PB-T3a y PB-T3c, el prefijo"), y la raiz.
Comprueba sobre la revision leida (HEAD del worktree o, con --objects-rev, esa revision en el almacen de objetos del checkout principal):
- el anclaje: la revision desciende de la emision de PB-T3b (F4B-E44) y del cierre de PB-T3b (PT3B_CLOSE), con src y tests iguales a ese cierre y a su
  GREEN, su entrada de cierre (F4B-E46) y su task-close-package;
- las anclas de WR, RCS, UP y de las guardas del escritor que cita D-PBT3-11; los nombres nuevos de PBT3C-C13, libres; lo que INV-PBT3C-03 y 04 dicen
  del escritor y del censo de CT-PBT1B-15 en la base;
- el texto de la v4.7 (por su sha256): Objective, AllowedWriteScope, el RED, la tabla de RequiredTests, los invariantes propios (01 a 04) y comunes (07
  a 14), las paradas propias (C04, C13, C16, C17) y comunes, la ExpectedEvidence y la linea base, literales y sin sus notas; la regla RR con la lectura
  de PB-T3c; la tabla de RR-3 en sus filas de PB-T3c (la de CT-05, enumerada, V47-04); las clases y la lista CT-PBT3C-01 a 05 de 5.4;
- PBT3C-C11: el bloque citado de la v4.7 con el prefijo de PB-T3c; el sustituto de nc1 con su SHA-256; el de nc2 v2, con --task PBT3C en su lista de
  tareas y la primera entrada de la ExpectedEvidence en su lista cerrada de literales (F4B_EXACT_EVIDENCES);
- los MinSelected (count_tests_git.py sobre los objetos); D1; la Snapshot K; F4B-E35; F4B-E30; y CA-T3c frente a BOUNDED_RESULT (bounded_check).
Uso: python make_pbt3c_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print] [--print-json] [--json-out <archivo>]
                                   [--objects-rev <rev> [--repo <checkout principal>] [--package-file <paquete>] [--bounded-file <informe de CA-T3c>]]
                                   [--as-write]
Al custodiarlo, las rutas locales se redactan como en PB-T0 ('<worktree-de-I-64>', '<checkout-principal>')."""
import json, os, re, sys, subprocess, hashlib, unicodedata

# ---------------------------------------------------------------- la base: el cierre de PB-T3b (F4B-E46), medido por objetos git (los marcadores, rellenos)
PT3B_CLOSE = 'fe1c67f4efa23f2abb9b6ab9d636df54ae15f343'   # la custodia del cierre de PB-T3b (F4B-E46), hija directa del GREEN; HEAD al emitir PB-T3c
GREEN_T3B = 'c758278548bcdd66e7c55ca15ca1faeb1a431e53'    # el GREEN de PB-T3b (intento 2), sin reparacion del RED
RED_T3B = '9fda277e5609e62ff715450eed03220c93df8585'      # el RED de PB-T3b (intento 2)
# la CI del GREEN de PB-T3b en su cierre: (run, sha8 del GREEN, Core, Skipped de Core, UI, Skipped de UI). La TRX cuenta la Theory S-2 con Skip como una fila
CI_BASE_T3B = ('44707482956', 'c7582785', 19124, 6, 2451, 21)
# los Passed de cada clase de guardas del escritor en la TRX de esa CI (F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/count-rows-out.txt en el cierre)
MINSEL_T3B = {'WorkspacePilotUpdateGuardTests': 86, 'WorkspacePilotUpdateIntegrationGuardTests': 123}
ROWS_OUT = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/count-rows-out.txt'
# la numeracion (la sesion): parametros comprobados contra f4b-dispositions.md y la evidencia (una asignacion cada uno, para leerlos con ast)
T3B_CLOSE_ENTRY = 'F4B-E46'
EMIT_ENTRY = 'F4B-E47'
CLOSE_EV = '273'
X_EV = '274'
BASE_EV = '275'
PROVISIONAL_FILL = {}  # el seco provisional del esbozo ya no hace falta: los marcadores estan rellenos con el cierre de PB-T3b
# ---------------------------------------------------------------- lo que ya existe
PT3B_EMIT = 'c7441653f3c1c8a27448ebec8f65f72a2f295f59'    # la reemision de PB-T3b con la v4.6 (F4B-E45): BASE c7441653, X 2bb3b759
PT3B_EMIT_X = '2bb3b759a998fe8959c4a3d593c1961195c2b32f'
PT3B_EMIT_ENTRY = 'F4B-E45'
PT3B_EMIT0 = 'eab27a9e340afa9ffe66a4a9866949806b6dfee3'   # la primera emision de PB-T3b con la v4.5 (F4B-E44)
PT3B_EMIT0_ENTRY = 'F4B-E44'
PT3B_TASK = 'F4B-PBT3B-FULL-WRITE-RUN'
PT3A_CLOSE = 'c40eea4ad126ec6209dfef40e643e180b364bc97'    # el cierre de PB-T3a (F4B-E43)
D1_REV = 'd8ccbcb870f4d1804d7e7afd3855ac64b44693a8'
ISSUE_AUTH = ('F4B-E35', 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md')
BOUNDED_RESULT = ('EMITIBLE; B 0 / I 0 / M 4; sin importantes; sin decisiones nuevas del Owner; remedicion sobre fe1c67f4 sin cambios materiales; '
                  'precondicion 13 cumplida: Core 19124 = 19118 + 0 + 6; recomprobacion v4.7: EMITIBLE, B 0 / I 0 / M 1')   # el veredicto literal de CA-T3c (con su recomprobacion, la seccion 14 del informe)
PKG_VERSION = 'v4.7'    # la version del paquete con la que se emite PB-T3c (la custodia el commit X de la emision)
CUSTODY_SHA256 = {'docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md': '976361c00388c9af317a109fe3e53184b9c141cb0cab126f2dc7b6d7d4b7f758'}
PKG46_SHA = '09210abe2ed5482c87bec1829f5798790d31090de490e615574c6c845d9e804a'  # la v4.6, la custodiada en el cierre de PB-T3b hasta el commit X
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
assert len(POS) == 3, 'uso: make_pbt3c_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
DRY = '--dry' in ARGS
AS_WRITE = '--as-write' in ARGS
PROVISIONAL = '--provisional' in ARGS
OBJ_REV = opt('--objects-rev')
assert OBJ_REV is None or DRY, '--objects-rev solo en seco: anade --dry'
assert not AS_WRITE or DRY, '--as-write solo en seco'
assert not PROVISIONAL, '--provisional ya no existe: los marcadores del cierre de PB-T3b estan rellenos (fe1c67f4, F4B-E46)'
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
# los marcadores, y su relleno declarado en el seco provisional
FILLED = []
if PROVISIONAL:
    for _k, _v in PROVISIONAL_FILL.items():
        if globals()[_k] is None:
            globals()[_k] = _v
            FILLED.append(_k)

if OBJ_REV is None:
    os.chdir(WT)
    REPO, REV = None, 'HEAD'
else:
    REPO, REV = opt('--repo') or MAIN_CHECKOUT, OBJ_REV
DISK = OBJ_REV is None

TASK = 'F4B-PBT3C-FULL-WRITE-ERASE'
assert 24 <= len(TASK) <= 26, len(TASK)  # R-T1-07: MAX_PATH
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
D3 = PILOT + '/F4B-DRAFTS/pbt3/'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
F4B_E25 = CO + 'owner-f4b-correction-unit-change-decision.md'
PT3B_TCP = PILOT + '/' + PT3B_TASK + '/task-close-package.md'
PBT3BC = PILOT + '/' + PT3B_TASK + '/coordinator/gate-contract.json'
WARN = []

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
    """Lo que solo existira con la emision de PB-T3c (su commit X, su comprobacion acotada): AVISO en seco, error al escribir (y con --as-write)."""
    if cond:
        return True
    assert DRY and not AS_WRITE, msg
    WARN.append(msg)
    return False


REV_SHA = git('rev-parse', REV).stdout.decode('ascii').strip()
assert re.fullmatch(r'[0-9a-f]{40}', REV_SHA), ('revision desconocida', REV)


def names_of(r):
    return [p for p in r.stdout.decode('utf-8').split('\n') if p]


# ---------------------------------------------------------------- el alcance del producto: alias
APP = 'src/RackCad.Application/Systems/PushBack/'
PILOTD = APP + 'Pilot/'
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
WS = TR + 'Workspace/'
PLUG = 'src/RackCad.Plugin/'
WR = PLUG + 'Views/RackPilotUpdateWriter.cs'
FACADE = PLUG + 'Views/RackViewBlockEraser.cs'
RPC, RCS = PLUG + 'RackPushBackCommands.cs', PLUG + 'RackCommandSupport.cs'
UP, ERASER = PILOTD + 'IPushBackPilotUpdatePort.cs', PILOTD + 'IPushBackPilotViewEraser.cs'
UG, IG = WS + 'WorkspacePilotUpdateGuardTests.cs', WS + 'WorkspacePilotUpdateIntegrationGuardTests.cs'
SRG = TR + 'PushBackSharedReferenceGuardTests.cs'

# ---------------------------------------------------------------- el anclaje (precondiciones 1 y 5; section 6.0: frente al cierre de PB-T3b, vacio)
for _short, _full in (('fe1c67f4', PT3B_CLOSE), ('c7582785', GREEN_T3B), ('9fda277e', RED_T3B), ('c7441653', PT3B_EMIT), ('2bb3b759', PT3B_EMIT_X),
                      ('eab27a9e', PT3B_EMIT0), ('c40eea4a', PT3A_CLOSE), ('d8ccbcb8', D1_REV)):
    assert git('rev-parse', _short).stdout.decode('ascii').strip() == _full, ('SHA', _short)
for _anc, _name in ((D1_REV, 'F4B-E29 (D-1)'), (PT3A_CLOSE, 'el cierre de PB-T3a (F4B-E43)'), (PT3B_EMIT0, 'la emision de PB-T3b (F4B-E44)'),
                    (PT3B_EMIT, 'la reemision de PB-T3b (F4B-E45)'), (GREEN_T3B, 'el GREEN de PB-T3b'), (PT3B_CLOSE, 'el cierre de PB-T3b (F4B-E46)')):
    assert git('merge-base', '--is-ancestor', _anc, REV).returncode == 0, ('la revision no desciende de ' + _name)
assert git('merge-base', '--is-ancestor', RED_T3B, GREEN_T3B).returncode == 0 and git('merge-base', '--is-ancestor', PT3B_EMIT, RED_T3B).returncode == 0, \
    'el RED de PB-T3b no esta entre su reemision y su GREEN'
assert git('diff', '--quiet', PT3B_CLOSE, REV, '--', 'src', 'tests').returncode == 0, \
    'src o tests cambiaron desde el cierre de PB-T3b (section 6.0: el diff frente al cierre de la subtarea anterior, vacio)'
assert git('diff', '--quiet', GREEN_T3B, PT3B_CLOSE, '--', 'src', 'tests').returncode == 0, 'el cierre de PB-T3b no tiene el src y los tests de su GREEN'
_disp_c = show(DISP, PT3B_CLOSE).replace('\r\n', '\n')
assert re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (PT3B_EMIT0_ENTRY, PT3B_TASK), _disp_c), 'F4B-E44, la emision de PB-T3b'
assert re.search(r'(?m)^## %s\. Parada de %s' % (PT3B_EMIT_ENTRY, PT3B_TASK), _disp_c), 'F4B-E45, la parada y la reemision de PB-T3b'
_heads = re.findall(r'(?m)^## (F4B-E\d+)\. Cierre de ' + PT3B_TASK + r'\b', _disp_c)
assert _heads == [T3B_CLOSE_ENTRY], ('el cierre de PB-T3b en su f4b-dispositions.md', _heads, T3B_CLOSE_ENTRY)
assert re.split(r'(?m)^## ', _disp_c)[-1].startswith(T3B_CLOSE_ENTRY + '. '), 'el cierre de PB-T3b no es la ultima entrada de su f4b-dispositions.md'
CLOSE_ENTRY = T3B_CLOSE_ENTRY
CLOSE_TEXT = _disp_c[_disp_c.index('\n## %s. ' % CLOSE_ENTRY):]
for _needle in ('CLOSED / PASS WITH LIMITATIONS', GREEN_T3B[:8], 'owner-pbt3b-nc3-decision.md', 'SATISFIED_BY_OWNER_DISPOSITION', 'attempts 2 de 3',
                'el total exacto de cada rama'):
    assert _needle in CLOSE_TEXT, ('la entrada de cierre de PB-T3b sin', _needle)
for _p in (PT3B_TCP, PBT3BC, CO + 'owner-pbt3b-nc3-decision.md', ROWS_OUT):
    assert git('cat-file', '-e', PT3B_CLOSE + ':' + _p).returncode == 0, ('el cierre de PB-T3b sin', _p)
assert 'CLOSED / PASS WITH LIMITATIONS' in show(PT3B_TCP, PT3B_CLOSE), 'el task-close-package de PB-T3b no dice CLOSED'
# la CI del GREEN de PB-T3b, en su entrada de cierre y en el recuento por clase de la sesion (count-rows-out.txt), con sus fuentes
_run, _sha, _core, _cskip, _ui, _skip = CI_BASE_T3B
assert re.fullmatch(r'\d+', _run) and GREEN_T3B.startswith(_sha) and _run in CLOSE_TEXT, ('la CI del GREEN de PB-T3b no es la de su cierre', CI_BASE_T3B)
_spaced = lambda v: format(v, ',').replace(',', ' ')
assert ('Core %s (%d omitidas, las de D-1) y UI %s (%s pasadas y %d omitidas)' % (_spaced(_core), _cskip, _spaced(_ui), _spaced(_ui - _skip), _skip)) in CLOSE_TEXT, \
    ('los recuentos de CI_BASE_T3B no son los de la entrada de cierre', CI_BASE_T3B)
ROWS_TEXT = show(ROWS_OUT, PT3B_CLOSE)
_green_rows = ROWS_TEXT[ROWS_TEXT.index('rv/ci-core/core.trx'):ROWS_TEXT.index('rv/red-ci-core/core.trx')]
assert _green_rows.startswith("rv/ci-core/core.trx {'Passed': %d, 'NotExecuted': %d} %d" % (_core - _cskip, _cskip, _core)), ('el GREEN en count-rows-out.txt', _green_rows[:80])
for _cls, _m in MINSEL_T3B.items():
    assert ("('%s', 'Passed') %d" % (_cls, _m)) in _green_rows, ('MINSEL_T3B no es el de la TRX del GREEN de PB-T3b', _cls, _m)
assert "GREEN %s core.trx " % _run in ROWS_TEXT, 'count-rows-out.txt no nombra la CI del GREEN'
ANCHOR_SRC = ('desciende de %s (F4B-E29), %s (F4B-E43), %s (%s), %s (%s), el GREEN %s y el cierre %s (%s); src y tests == el cierre == el GREEN de '
              'PB-T3b; la CI %s (Core %d con %d Skipped; UI %d con %d) en la entrada de cierre y en count-rows-out.txt' % (
                  D1_REV[:8], PT3A_CLOSE[:8], PT3B_EMIT0[:8], PT3B_EMIT0_ENTRY, PT3B_EMIT[:8], PT3B_EMIT_ENTRY, GREEN_T3B[:8], PT3B_CLOSE[:8], CLOSE_ENTRY,
                  _run, _core, _cskip, _ui, _skip))

# ---------------------------------------------------------------- el contrato de PB-T3b, el reemitido: las listas, los filtros, las celdas y las paradas
pbt3b = json.loads(show(PBT3BC))
assert pbt3b['TaskId'] == PT3B_TASK and pbt3b['AuthorityRevision'] == PT3B_EMIT_X, 'el contrato de PB-T3b no es el reemitido (F4B-E45)'
assert len(pbt3b['AllowedWriteScope']) == 11 and len(pbt3b['ForbiddenWriteScope']) == 294 and len(pbt3b['StopConditions']) == 34, 'el contrato de PB-T3b'
assert len(pbt3b['Authorities']) == 66 and 'PrepareFull, BuildCore' in [s['Text'] for s in pbt3b['StopConditions'] if s['Id'] == 'PBT3B-C13'][0], \
    'el contrato de PB-T3b no es el de la v4.6 (66 autoridades y PBT3B-C13 con PrepareFull)'
T3B_STOP = {s['Id']: s['Text'] for s in pbt3b['StopConditions']}
P01 = 'config.toml no longer matches the accepted Snapshot in force (today Snapshot %s, %s)'
STD_IDS = ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
P_STOPS = [dict(s) for s in pbt3b['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
assert [s['Id'] for s in P_STOPS] == STD_IDS and P_STOPS[9]['Text'] == P01 % ('K', SNAP_K), 'las 17 estandar de PB-T3b, con P-01 en la K'


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

# ---------------------------------------------------------------- el alcance (section 6.3: 4 rutas; section 5.4: las clases)
CLASS_CTS = [('Core', 'PushBackViewEraseGuardTests', [1, 2, 3, 4]), ('Core', 'PushBackWritePathPluginGuardTests', [5])]
assert sorted(n for _, _, cts in CLASS_CTS for n in cts) == list(range(1, 6)), 'la lista cerrada CT-PBT3C-01 a 05'
TEST_NEW = [TR + c + '.cs' for _, c, _ in CLASS_CTS]
NEW_FILES = [FACADE] + TEST_NEW
ALLOWED = [WR, FACADE] + TEST_NEW
assert len(ALLOWED) == 4 == len(set(ALLOWED))  # INV-PBT3C-01 (4 rutas)
for p in (WR, RCS, RPC, UP):
    assert in_head(p) and (not DISK or os.path.exists(p)), ('no existe', p)
assert all(not in_head(p) and (not DISK or not os.path.exists(p)) for p in NEW_FILES), [p for p in NEW_FILES if in_head(p)]
for _pref in ('PushBackViewErase', 'PushBackWritePathPlugin', 'RackViewBlockEraser'):
    assert not [p for p in tracked('tests/') + tracked('src/') if _pref in p], ('el prefijo ya existe', _pref)
# la interfaz de PB-T3b que WR implementa en PB-T3c: llega con el GREEN de PB-T3b
soft(in_head(ERASER), 'cierre de PB-T3b: la interfaz IPushBackPilotViewEraser.cs (PB-T3b) aun no esta en la revision: WR la implementa en PB-T3c')


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


SRCF = cover('src/')
for a in ALLOWED:
    assert not any(a == f or (f.endswith('/') and a.startswith(f)) for f in SRCF), a
for p in tracked('src/'):
    assert p in ALLOWED or any(p == f or (f.endswith('/') and p.startswith(f)) for f in SRCF), ('sin cubrir', p)
# INV-PBT3C-01: Application, la ventana y Domain enteros; en el Plugin, todo menos WR y la fachada (RPC y RCS entre lo prohibido)
assert {'src/RackCad.Application/', 'src/RackCad.UI/', 'src/RackCad.Domain/'} <= set(SRCF) and RPC in SRCF and RCS in SRCF
assert [p for p in tracked(PLUG + 'Views/') if p not in SRCF] == [WR], 'Plugin/Views/: fuera del ForbiddenWriteScope, solo WR'

# tests/ (section 6.0): las del contrato de PB-T3b, con tests/RackCad.Tests/Workspace/ otra vez como prefijo ("en PB-T3a y PB-T3c, el prefijo"), mas
# las cinco de pruebas de PB-T3b (sus cuatro clases y el doble)
T3B_TESTS = [f for f in pbt3b['AllowedWriteScope'] if f.startswith('tests/') and not f.startswith(WS)]
assert [os.path.basename(f) for f in T3B_TESTS] == ['PushBackFullWritePrepareTests.cs', 'PushBackFullWriteRunTests.cs', 'PushBackWritePathGuardTests.cs',
                                                     'PushBackPilotUpdateEquivalenceGuardTests.cs', 'PushBackFullWriteSupport.cs'], T3B_TESTS
CORE_T3B = [f for f in pbt3b['ForbiddenWriteScope'] if f.startswith(TR)]
UI_T3B = [f for f in pbt3b['ForbiddenWriteScope'] if f.startswith(UT)]
assert len(CORE_T3B) == 109 and len(UI_T3B) == 69 and WS not in CORE_T3B, (len(CORE_T3B), len(UI_T3B))
_ws = [i for i, f in enumerate(CORE_T3B) if f.startswith(WS)]
assert len(_ws) == 76 and _ws == list(range(_ws[0], _ws[0] + 76)), 'la lista explicita de Workspace/ de PB-T3b, contigua'
CORE_T = CORE_T3B[:_ws[0]] + [WS] + CORE_T3B[_ws[-1] + 1:] + T3B_TESTS
UI_T = UI_T3B
assert len(CORE_T) == 28 + 1 + 5 + 5 == len(set(CORE_T)) and len(UI_T) == 69, (len(CORE_T), len(UI_T))
TESTF = CORE_T + UI_T
assert not any(a == t or (t.endswith('/') and a.startswith(t)) for a in ALLOWED for t in TESTF)
ROOT = [f for f in pbt3b['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
assert len(ROOT) == 17 and ROOT[0] == 'docs/' and ROOT[-1] == '.gitignore', ROOT
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == len(set(FORBIDDEN)), 'duplicados en el ForbiddenWriteScope'
T3B_MISSING = [t for t in T3B_TESTS if not in_head(t)]  # las clases de PB-T3b llegan con su RED y su GREEN
for f in FORBIDDEN:
    if f not in T3B_MISSING:
        assert in_head(f) and (not DISK or os.path.exists(f.rstrip('/'))), ('no existe', f)
soft(not T3B_MISSING, 'cierre de PB-T3b: clases de PB-T3b del ForbiddenWriteScope aun no en la revision: ' + ', '.join(os.path.basename(f) for f in T3B_MISSING))

# ---------------------------------------------------------------- las anclas de PB-T3c en la revision (archivo, linea o None si va por contenido, texto)
ANCHORS = [
    # D-PBT3-11: WR (R-18; :26 y :225, los dos comentarios que cambian), sus clases, PrepareUnits, BeginMutation, el ambito de MUTATE y Reread
    (WR, 26, 'It never erases a view.'), (WR, 32, 'internal sealed class RackPilotUpdateWriter : IPushBackPilotUpdatePort'),
    (WR, 70, 'internal sealed class RackPilotUpdateScope : IPushBackPilotUpdateScope'),
    (WR, 74, 'private readonly Dictionary<string, ObjectId> _definitions = new(StringComparer.Ordinal);'),
    (WR, 76, 'private readonly HashSet<ObjectId> _staleDefinitions = new();'),
    (WR, 101, 'public PushBackPilotImportResult PrepareUnits(PushBackPilotUpdatePlan plan)'),
    (WR, 113, 'return PushBackPilotImportResult.Failed("La definici'), (WR, 146, 'public PushBackPilotMutationStart BeginMutation()'),
    (WR, 156, 'return PushBackPilotMutationStart.Started(new RackPilotMutationScope(database, transaction, _prepared, _staleDefinitions));'),
    (WR, 174, 'SystemBlockWriter.PurgeAfterCommit(database, _staleDefinitions.ToList());'),
    (WR, 225, 'It never purges, renames, regenerates or erases'), (WR, 227, 'internal sealed class RackPilotMutationScope : IPushBackPilotMutationScope'),
    (WR, 239, 'internal RackPilotMutationScope('), (WR, 252, 'public PushBackPilotScanValues Reread()'),
    (WR, 258, 'matches[pair.Key] = scan.Definitions.TryGetValue(pair.Key, out var found) && found == pair.Value.Prepared.BlockId;'),
    (WR, 262, 'public PushBackPilotUnitResult WriteUnit(string memberKey, string envelopeText)'), (WR, 305, '_transaction.Commit();'),
    (WR, 319, 'private void EnsureOwned()'), (WR, 329, 'private static void FailUnitIfRequested(int oneBasedUnit)'),
    # el borrado clasico que copia la fachada (RCS:233-266; la definicion nula o de layout, RCS:240-243)
    (RCS, None, 'EraseViewBlocks('), (RCS, 240, 'definition.IsLayout'),
    # UP: el resultado de una unidad, que devuelve EraseUnit
    (UP, 139, 'public sealed class PushBackPilotUnitResult'),
    # las guardas del escritor de F4 que siguen en verde (UpdateGuard:182-212, con su comentario :188) y NamesThePilot de G-9 (IntegrationGuard:679)
    (UG, 182, 'private static IEnumerable<string> WriterProhibitionViolations(string code)'), (UG, 188, 'Never: the lock, an erasure'),
    (IG, 679, 'NamesThePilot = new(@"PushBackPilot\\w*|RackPilot\\w*|RackCad\\.Application\\.Systems\\.PushBack\\.Pilot")'),
    # CT-PBT1B-15: los 14 archivos del Plugin con PushBack
    (SRG, 384, '"archivos=14"'),
    ('docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md', 1338, '- **PBT1C-C13:** keeping a gesture without observable change'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 655, 'vencen el 2026-12-29'),
]
TEXT = {}
for path, line, text in ANCHORS:
    if path not in TEXT:
        TEXT[path] = lines_of(path)
    if line is None:
        assert sum(1 for l in TEXT[path] if text in l) >= 1, ('ancla por contenido ausente', path, text)
    else:
        assert text in TEXT[path][line - 1], ('ancla', path, line, TEXT[path][line - 1].strip())


def code_of(raw):
    s = re.sub(r'"(?:\\.|[^"\\])*"', '""', raw)
    s = re.sub(r"'(?:\\.|[^'\\])'", "''", s)
    return s.split('//')[0]


def member_end(lines, d):
    """Ultima linea del miembro declarado en d (1-based): la llave que cierra lo que abre."""
    depth, opened = 0, False
    for i in range(d, len(lines) + 1):
        code = code_of(lines[i - 1])
        depth += code.count('{') - code.count('}')
        opened = opened or '{' in code
        if (opened and depth == 0) or (not opened and code.strip().endswith(';') and depth == 0):
            return i
    raise AssertionError(('miembro sin final', d))


def norm_code(lines):
    return re.sub(r'\s+', ' ', ' '.join(l.split('//')[0].strip() for l in lines)).strip()


# PBT3C-C13 (section 6.3): los nombres nuevos de D-PBT3-11 no existen en WR (los campos y el metodo) ni en src/ y tests/ (la fachada); el constructor
# interno de RackPilotMutationScope, el que gana un parametro, esta una vez y se llama una vez (WR:156; R-18)
_wr = '\n'.join(TEXT[WR])
for _n in ('_erasures', 'EraseUnit'):
    assert not re.search(r'\b%s\b' % re.escape(_n), _wr), ('un nombre nuevo de PBT3C-C13 ya existe en WR', _n)
_gg = git('grep', '-l', '-w', '-e', 'RackViewBlockEraser', '-e', 'EraseInTransaction', REV, '--', 'src', 'tests')
assert _gg.returncode == 1, ('la fachada de PB-T3c ya se nombra en src/ o tests/', _gg.stdout.decode('utf-8')[:300])
assert _wr.count('internal RackPilotMutationScope(') == 1 and git('grep', '-c', '-F', 'new RackPilotMutationScope(', REV, '--', 'src', 'tests').stdout.decode().strip() \
    .endswith(':1'), 'el constructor interno de RackPilotMutationScope: uno, y una sola construccion (WR:156)'


def decl_census(lines):
    """Las declaraciones de un archivo, contadas una a una con su nombre (el metodo de R-18)."""
    out = []
    for raw in lines:
        s = code_of(raw).strip()
        m = re.match(r'(?:public|private|internal|protected)\b(.*)$', s)
        if not m:
            continue
        t = re.search(r'\b(?:class|struct|interface|enum|record(?:\s+struct)?)\s+(\w+)', m.group(1))
        n = t or re.search(r'(\w+)\s*(?:<[^()]*>)?\s*(?:\(|\{|;|=>|=)', m.group(1))
        if n:
            out.append(n.group(1))
    return out


CENSUS_BASE = decl_census(TEXT[WR])
# INV-PBT3C-03, en la base: tres clases RackPilot*, un StartTransaction, un Commit, una purga, dos [Conditional("DEBUG")]; ninguno de los nombres que
# el escritor no puede llevar; ninguna asignacion .Name = (sobre el codigo sin comentarios ni cadenas)
_code = '\n'.join(code_of(l) for l in TEXT[WR])
INV03 = {'clases RackPilot*': len(re.findall(r'\bclass\s+RackPilot\w*', _code)), 'StartTransaction': _code.count('StartTransaction('),
         'Commit': _code.count('.Commit('), 'purga': _code.count('PurgeAfterCommit('), 'Conditional(DEBUG)': '\n'.join(TEXT[WR]).count('[Conditional("DEBUG")]')}
assert INV03 == {'clases RackPilot*': 3, 'StartTransaction': 1, 'Commit': 1, 'purga': 1, 'Conditional(DEBUG)': 2}, ('INV-PBT3C-03 en la base', INV03)
for _bad in ('GetBlockReferenceIds', 'Erase(', 'EraseViewBlocks', 'ForWrite', 'UpgradeOpen'):
    assert _bad not in _code, ('el escritor ya nombra', _bad)
assert not re.search(r'\.Name\s*=(?!=)', _code), 'el escritor ya asigna .Name ='
# INV-PBT3C-04: el censo de CT-PBT1B-15, los archivos del Plugin cuyo codigo contiene PushBack: 14 en la base
_plug = [p for p in tracked(PLUG) if p.endswith('.cs')]
PB_FILES = sorted(p for p in _plug if 'PushBack' in '\n'.join(code_of(l) for l in lines_of(p)))
assert len(PB_FILES) == 14 and WR in PB_FILES, ('el censo de CT-PBT1B-15: 14 archivos del Plugin con PushBack', len(PB_FILES))

# D1 (section 6.0, la lectura de Ci en el RED): build-plugin con needs [tests, build-ui], asi que en el RED, con tests en failure, sale omitido
CI_LINES = lines_of('.github/workflows/ci.yml')
_i = CI_LINES.index('  build-plugin:')
_needs, _in_needs = [], False
for l in CI_LINES[_i + 1:]:
    if re.match(r'^  [A-Za-z0-9_-]+:\s*$', l):
        break
    if l.strip().startswith('needs:'):
        _in_needs = True
        inline = l.split(':', 1)[1].strip()
        if inline:
            _needs += [x.strip() for x in inline.strip('[]').split(',') if x.strip()]
        continue
    if _in_needs:
        mm = re.match(r'^\s+-\s+([A-Za-z0-9_-]+)\s*$', l)
        if mm:
            _needs.append(mm.group(1))
        else:
            _in_needs = False
assert _needs == ['tests', 'build-ui'] and {'  tests:', '  ui-tests:', '  build-ui:'} <= set(CI_LINES), ('needs de build-plugin', _needs)

# ---------------------------------------------------------------- el paquete: los textos de la v4.7, literales en ASCII y sin sus notas

def ascii_fold(s):
    for x, y in ((chr(0xAB), '"'), (chr(0xBB), '"'), (chr(0x2014), '-'), (chr(0x2013), '-'), (chr(0x2192), '->'), (chr(0x2264), '<='), (chr(0x2265), '>='),
                 (chr(0x2026), '...'), (chr(0xA7), 'section '), (chr(0x2032), "'"), (chr(0x2212), '-'), (chr(0x222A), 'U'), (chr(0x221E), 'inf')):
        s = s.replace(x, y)
    s = ''.join(c for c in unicodedata.normalize('NFKD', s) if not unicodedata.combining(c))
    assert all(ord(c) < 0x80 for c in s), [c for c in s if ord(c) >= 0x80]
    return s


def md_fold(lines):
    """Una vineta del paquete con sus subvinetas, en una linea ASCII: sin negritas ni comillas de codigo, sin el guion de vineta."""
    return ascii_fold(' '.join(re.sub(r'^- ', '', l.strip()) for l in lines if l.strip()).replace('**', '').replace('`', ''))


VARIANT = "(A')"  # la variante (A') de D-1, no elegida (F4B-E29): no va al contrato
PKG = 'docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md'
MEAS = 'docs/initiatives/I-64-f4b-pbt3-measurements.md'
_pkg_head = hashlib.sha256(show_bytes(PKG)).hexdigest()
soft(_pkg_head == CUSTODY_SHA256[PKG], 'emision: el paquete custodiado en la revision no es la %s (sha256 %s; %s): la custodia el commit X de PB-T3c'
     % (PKG_VERSION, _pkg_head[:8], 'la v4.6' if _pkg_head == PKG46_SHA else 'otra'))
if PKG_FILE:
    PKG_RAW, PKG_SRC = open(PKG_FILE, 'rb').read(), '--package-file ' + os.path.basename(PKG_FILE)
else:
    PKG_RAW, PKG_SRC = show_bytes(PKG), PKG + ' en ' + REV_SHA[:8]
PKG_LINES = PKG_RAW.decode('utf-8').replace('\r\n', '\n').split('\n')
_title = [l for l in PKG_LINES[:6] if l.startswith('# I-64 F4B')]
PKG_IN = re.search(r', (v\d+(?:\.\d+)?)\s*$', _title[0]).group(1) if len(_title) == 1 else None
assert PKG_IN == PKG_VERSION, ('el paquete leido (%s) es la %s, no la %s' % (PKG_SRC, PKG_IN, PKG_VERSION))


def section(head_prefix):
    i = [k for k, l in enumerate(PKG_LINES) if l.startswith(head_prefix)]
    assert len(i) == 1, ('seccion', head_prefix, len(i))
    out = []
    for l in PKG_LINES[i[0] + 1:]:
        if l.startswith('## ') or l.startswith('### '):
            break
        out.append(l)
    return out


def bullets(sec, indent):
    """Las vinetas de un nivel (indent espacios), cada una con sus subvinetas y sin las "Nota (no va al contrato)" ni lo que cuelga de ellas."""
    res, cur = [], None
    for l in sec:
        if l.startswith(' ' * indent + '- ') and not l.startswith(' ' * (indent + 1)):
            if cur:
                res.append(cur)
            cur = [l]
        elif cur is not None and (l.startswith(' ' * (indent + 2)) or l.strip() == ''):
            cur.append(l)
        elif cur is not None:
            res.append(cur)
            cur = None
    if cur:
        res.append(cur)
    out = []
    for b in res:
        keep, skip = [], None
        for l in b:
            if l.strip().startswith('- **Nota (no va al contrato).**'):
                skip = len(l) - len(l.lstrip())
                continue
            if skip is not None and (l.strip() == '' or len(l) - len(l.lstrip()) > skip):
                continue
            skip = None
            keep.append(l)
        out.append(md_fold(keep))
    return [b for b in out if b]


def one(folded, prefix):
    hits = [b for b in folded if b.startswith(prefix)]
    assert len(hits) == 1, ('vineta del paquete', prefix, len(hits))
    return hits[0]


# ---- rr_pkg_check (inicio)
def rr_pkg_check(sec_lines, classes, sub='PB-T3c'):
    """La regla RR en el section 6.0 del paquete y su lectura para PB-T3c: el parrafo de F4B-E35, RR-1 a RR-6 una vez cada una; las dos lecturas de
    PB-T3 con el RED de PB-T3c (la fachada); el RED de la cadena en una correccion de PB-T3c; la CI del RED reparado; la verificacion con una reparacion,
    con el tramo RedSha..RedRepairSha de PB-T3c (ni WR ni la fachada); y la tabla cerrada de RR-3, cuyas filas de PB-T3c son exactamente las clases
    nuevas (classes). Devuelve los errores (vacia si vale)."""
    import re, unicodedata
    fold = lambda s: ''.join(c for c in unicodedata.normalize('NFKD', s) if not unicodedata.combining(c)).replace('**', '').replace('`', '')
    o = chr(0xF3)
    t = '\n'.join(sec_lines)
    errors = []
    if t.count('**La reparaci' + o + 'n del RED** (F4B-E35, pregunta 4') != 1:
        errors.append('falta, una sola vez, el parrafo de la regla RR (F4B-E35, pregunta 4)')
    for k in range(1, 7):
        if sum(1 for l in sec_lines if l.startswith('- **RR-%d, ' % k)) != 1:
            errors.append('RR-%d no esta una sola vez' % k)
    two = [fold(l) for l in sec_lines if l.startswith('- **Las dos lecturas de PB-T3, porque los tres RED ya tocan `src/`**')]
    if not (len(two) == 1 and 'el de PB-T3c, el de la fachada' in two[0]):
        errors.append('las dos lecturas de PB-T3 sin el RED de PB-T3c (la fachada)')
    if sum(1 for l in sec_lines if l == '- en PB-T3c, el esqueleto de la fachada y WR de la base;') != 1:
        errors.append('el RED de la cadena en una correccion de PB-T3c no es "el esqueleto de la fachada y WR de la base"')
    if sum(1 for l in sec_lines if l.startswith('- **la CI del RED reparado** (F4B-E35, RR-4; V43-04)')) != 1:
        errors.append('falta la CI del RED reparado (V43-04)')
    if t.count('**La verificaci' + o + 'n con una reparaci' + o + 'n del RED** (F4B-E35, RR-7') != 1:
        errors.append('falta la verificacion con una reparacion del RED (V43-05)')
    if [fold(l) for l in sec_lines].count('  - en PB-T3c, WR ni la fachada (RackViewBlockEraser.cs).') != 1:
        errors.append('la verificacion con una reparacion sin el tramo RedSha..RedRepairSha de PB-T3c (ni WR ni la fachada)')
    rows = [l for l in sec_lines if re.match(r'^\| PB-T3[abc] \| `', l)]
    mine = [re.match(r'^\| %s \| `(\w+)`' % re.escape(sub), l).group(1) for l in rows if l.startswith('| %s |' % sub)]
    if sorted(mine) != sorted(classes):
        errors.append('la tabla cerrada de RR-3 no nombra exactamente las clases nuevas de %s: %s' % (sub, ascii(mine)))
    return errors
# ---- rr_pkg_check (fin)


# ---- rr3_table_check (inicio)
def rr3_table_check(sec_lines, classes, sub='PB-T3c'):
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
        cm = re.match(r'`(\w+)` \(CT-', cells[1]) if len(cells) == 3 else None
        if not cm:
            errors.append('una fila de %s de la tabla de RR-3 sin la forma "| %s | `Clase` (CT-...) | ... |": %s' % (sub, sub, ascii(row[:80])))
            continue
        cls, body = cm.group(1), cells[2]
        seen.append(cls)
        if re.search(r'Se enumeran con su referencia .* en la emisi\S+n de PB-T3c', body):
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


# Los textos de la v4.7 para PB-T3c, en ASCII (md_fold) y sin las "Nota (no va al contrato)". El contrato los lleva tal cual.
OBJECTIVE = (
    'Dar al escritor de la actualizacion del Workspace de Push Back la capacidad de borrar una vista obsoleta dentro de la unica transaccion de MUTATE: '
    'una fachada caller-owned nueva (RackViewBlockEraser.EraseInTransaction, sin bloqueo, sin transaccion propia, sin confirmacion y sin purga, con el '
    'borrado de las referencias directas y la captura de las definiciones anidadas del borrado clasico) y su uso desde EraseUnit del ambito de MUTATE, '
    'con PrepareUnits y la relectura de MUTATE que cubren las vistas a borrar, segun el paquete del contrato de PB-T3 v4.7 y la lista cerrada '
    'CT-PBT3C-01..05. El RED son las guardas de texto nuevas y el esqueleto inerte de la fachada; el GREEN toca solo el escritor y la fachada. Las '
    'guardas del escritor de F4 siguen en verde, con su registro declarado sin Skip. Si hiciera falta mutar el rack resuelto o el informe de modulos '
    'dentro del tiempo de vida del borrador: STOP y se reabre L-1. Application, la ventana, el clasico y Domain no cambian; no se ejecuta AutoCAD.')
OBJ_LABEL = 'Objective (ASCII): '
ALLOWED_PKG = ('AllowedWriteScope (4): WR; src/RackCad.Plugin/Views/RackViewBlockEraser.cs; tests/RackCad.Tests/PushBackViewEraseGuardTests.cs y '
               'tests/RackCad.Tests/PushBackWritePathPluginGuardTests.cs.')
RED_PKG = 'El RED (V2-05): las dos clases de prueba y el esqueleto inerte de la fachada (section 5.4). WR no cambia.'
REQ_TABLE_PKG = (
    'RequiredTests: | Proyecto | Filtro | MinSelected | ExpectRed | |---|---|---|---| | Core | PushBackViewEraseGuardTests | 4 | true | | Core | '
    'PushBackWritePathPluginGuardTests | 1 | false | | Core | WorkspacePilotUpdateGuardTests, WorkspacePilotUpdateIntegrationGuardTests | 86, 123: los '
    'Passed de cada clase en la CI del GREEN de PB-T3b (44707482956), sin los Skip de D-1 (1 y 2) | false | | Core | PushBackSharedReferenceGuardTests '
    '(CT-PBT1B-15: 14 archivos y 7 asignaciones) | 2 | '
    'false | | UI | PushBackFullDraftWindowParityTests (PB-T3a; V46-03) | 1 | false | | | Y los de section 6.2 sin ExpectRed | | false |')
INV_OWN = [
    'INV-PBT3C-01. Alcance: dentro de las 4 rutas. git diff BaseSha..CurrentSha -- src/RackCad.Application src/RackCad.UI src/RackCad.Domain '
    'src/RackCad.Plugin/RackPushBackCommands.cs src/RackCad.Plugin/RackCommandSupport.cs vacio.',
    'INV-PBT3C-02. RED y GREEN: el RED son las dos clases de prueba y el esqueleto de la fachada, con WR sin cambio; el GREEN solo toca WR y la fachada. '
    'Criterio del RED como INV-PBT3A-02. Con una reparacion del RED (F4B-E35, RR; V43-03): RedSha..RedRepairSha solo toca las dos clases de prueba (no '
    'WR ni la fachada); el criterio se repite entero sobre RedRepairSha; y RedRepairSha..CurrentSha es el GREEN, solo WR y la fachada.',
    'INV-PBT3C-03. El escritor: sigue con tres clases RackPilot*, un StartTransaction, un Commit, una purga y dos [Conditional("DEBUG")]; no nombra '
    'GetBlockReferenceIds, Erase(, EraseViewBlocks, ForWrite ni UpgradeOpen; no anade ninguna asignacion .Name =.',
    'INV-PBT3C-04. La fachada no contiene la cadena PushBack ni nombra el piloto; el censo de CT-PBT1B-15 sigue en 14 archivos.',
]
C13_LABEL = 'PBT3C-C13 (V2-30 y V3-02)'
C13_TXT = (
    'doing the task would require touching Application, RPC, RCS or any other file of the Plugin; or adding in WR or RackViewBlockEraser.cs a '
    'declaration whose name does not exist in WR at BaseSha, or a further declaration of a name that does (an overload), of any visibility (type, '
    'constructor, method, property, field, constant or nested type), other than the five that D-PBT3-11 prescribes: the field _erasures of '
    'RackPilotUpdateScope, the field _erasures of RackPilotMutationScope and the method EraseUnit of RackPilotMutationScope, in WR; and the class '
    'RackViewBlockEraser and its method EraseInTransaction, in RackViewBlockEraser.cs. One change of an existing member is prescribed and is not a new '
    'declaration: the internal constructor of RackPilotMutationScope gains one parameter, for _erasures. The Worker stops before adding it.')
STOP_OWN_PKG = [('C04', 'as PBT3A-C04.'), ('C13', C13_TXT), ('C16', 'an F4 writer guard fails. It returns to the Owner: D-1 does not cover the writer.'),
                ('C17', 'the D2 limit.')]
EVID_FIRST_PKG = (
    'lo de section 6.1, con la frase propia de la subtarea en la lectura de la verificacion con una reparacion, en lugar de la de PB-T3a: "En PB-T3c, '
    'ademas: RedSha..RedRepairSha no toca WR ni la fachada (RackViewBlockEraser.cs)" (V44-04); y, en la tabla "CT -> la asercion que falla en el RED", '
    'las autopruebas de CT-PBT3C-01, 04 y 05, en lugar de "las autopruebas de las tres guardas" (V46-03; CA-T3b M-03);')
EVID_OWN = [
    'la CI del RED con tests en failure, ui-tests y build-ui en success y build-plugin omitido;',
    'la del GREEN 4/4, con el Plugin compilado;',
    'el censo de declaraciones de WR y de la fachada en BaseSha y en CurrentSha, contadas una a una con su nombre (una sobrecarga es una declaracion '
    'mas): la diferencia son las cinco de PBT3C-C13 (V3-02);',
    'el Trailer real;',
    'la entrada de D-2 en superseding-characterization.md, escrita por la sesion en el commit de emision (V2-11).',
]
CI_LINE_PKG = ('Linea base de la CI: la del cierre de PB-T3b (F4B-E46): run 44707482956 sobre el GREEN c7582785, Core 19 124 resultados, con 19 118 '
               'Passed y 6 Skipped, los de D-1 (la TRX cuenta la Theory S-2 con Skip como una fila; F4B-DRAFTS/pbt3/nc3-row-count-pbt3b/count-rows-out.txt), '
               'y UI 2 451, con 2 430 Passed y 21 Skipped. Previstos, con el total exacto de cada rama (F4B-E46; V47-03): PB-T3c no quita ningun Skip de D-1 '
               'ni anade ninguna Theory, asi que hay una sola rama. El RED: Core 19 129 resultados, con 19 119 Passed, 4 Failed (CT-01 a 04) y 6 Skipped. El '
               'GREEN: Core 19 129, con 19 123 Passed, 0 Failed y 6 Skipped. UI, en los dos: 2 451, con 2 430 Passed y 21 Skipped.')
# lo de section 6.1 con lo de PB-T3c: la CI del RED (la de su vineta propia), las autopruebas de sus guardas y la frase de la verificacion
EVID_T3A_SUBS = {2: ('(tests y ui-tests en failure, build-ui en success, build-plugin omitido, sin cuelgue)',
                     '(tests en failure, ui-tests y build-ui en success, build-plugin omitido, sin cuelgue)'),
                 3: ('las autopruebas de las tres guardas', 'las autopruebas de CT-PBT3C-01, 04 y 05'),
                 8: ('En PB-T3a, ademas: RedSha..RedRepairSha no toca ningun archivo de src/ (el esqueleto de Draft/).',
                     'En PB-T3c, ademas: RedSha..RedRepairSha no toca WR ni la fachada (RackViewBlockEraser.cs).')}
STOP_LABELS = {'C07': 'C07 (con la excepcion de F4B-E35, V43-02)', 'C18': 'C18 (nueva, V43-02; el texto de PBT2x-C18 de PB-T2)'}
COMMON_STOPS = ['C01', 'C02', 'C03', 'C05', 'C06', 'C07', 'C09', 'C10', 'C18']


SEC60, SEC63, SEC54 = section('### 6.0 '), section('### 6.3 Contrato de PB-T3c'), section('### 5.4 ')
B60, B63, B63_2 = bullets(SEC60, 0), bullets(SEC63, 0), bullets(SEC63, 2)
_q11 = [l for l in SEC60 if l.startswith('  > ')]
assert len(_q11) == 1, ('el bloque citado de PBT3x-C11 del section 6.0', len(_q11))
C11_FOLD = ascii_fold(_q11[0][4:]).replace('`', '')
_ev0 = [i for i, b in enumerate(B63_2) if b.startswith('lo de section 6.1, ')]
assert len(_ev0) == 1
_copied = ([b for b in B63 if b.startswith((OBJ_LABEL, 'Linea base de la CI'))] + [b for b in B63_2 if b.startswith(('INV-PBT3C-', 'PBT3C-C'))] +
           B63_2[_ev0[0]:] + [b for b in B60 if re.match(r'^(0[7-9]|1[0-4])\. |^C\d\d(: | \()', b)] + [C11_FOLD])
assert len(_copied) == 2 + 4 + 4 + 6 + 8 + 9 + 1, ('los textos que se copian del paquete', len(_copied))
assert not [b[:60] for b in _copied if VARIANT in b], ("un texto que se copia al contrato lleva la variante (A') no elegida", [b[:60] for b in _copied if VARIANT in b])
assert one(B63, OBJ_LABEL) == OBJ_LABEL + '"' + OBJECTIVE + '"', 'el Objective del paquete difiere'
assert ascii_fold(OBJECTIVE) == OBJECTIVE and ('PB-T3 ' + PKG_VERSION) in OBJECTIVE
assert one(B63, 'AllowedWriteScope (4): ') == ALLOWED_PKG, 'el AllowedWriteScope del section 6.3 difiere'
assert one(B63, 'UNIT_CHANGE: ') == 'UNIT_CHANGE: WR (section 6.0; F4B-E25).', 'UNIT_CHANGE de PB-T3c'
assert one(B63, 'El RED (V2-05): ') == RED_PKG, 'el RED del section 6.3 difiere'
assert one(B63, 'RequiredTests: ') == REQ_TABLE_PKG, 'la tabla de RequiredTests del section 6.3 cambio'
for txt in INV_OWN:
    assert one(B63_2, txt.split('. ')[0] + '.') == txt, (txt.split('. ')[0] + ' del paquete difiere')
assert [b.split('. ')[0] for b in B63_2 if b.startswith('INV-PBT3C-')] == ['INV-PBT3C-%02d' % n for n in (1, 2, 3, 4)]
for cid, txt in STOP_OWN_PKG:
    lab = C13_LABEL if cid == 'C13' else 'PBT3C-' + cid
    assert one(B63_2, lab + ': ') == lab + ': ' + txt, (lab + ' del paquete difiere')
assert [b.split(':')[0].split(' ')[0] for b in B63_2 if b.startswith('PBT3C-C')] == ['PBT3C-C04', 'PBT3C-C13', 'PBT3C-C16', 'PBT3C-C17']
assert B63_2[_ev0[0]:] == [EVID_FIRST_PKG] + EVID_OWN, ('la ExpectedEvidence del section 6.3', B63_2[_ev0[0]:])
assert one(B63, 'ExpectedEvidence: ') == 'ExpectedEvidence: ' + ' '.join([EVID_FIRST_PKG] + EVID_OWN), 'la ExpectedEvidence del section 6.3 difiere'
assert one(B63, 'Linea base de la CI') == CI_LINE_PKG, 'la linea base de la CI del section 6.3 difiere'
assert sum(1 for l in SEC63 if l.strip().startswith('- **Nota (no va al contrato).**')) == 2, 'las notas que no van al contrato del section 6.3'
# los comunes del section 6.0, que el contrato de PB-T3b ya lleva, comprobados contra el paquete y con el prefijo de PB-T3c
T3B_INV = {i.split('. ')[0]: i for i in pbt3b['Invariants']}
INV_COMMON = []
for n in ['%02d' % k for k in range(7, 15)]:
    _t = one(B60, n + '. ')
    assert T3B_INV['INV-PBT3B-' + n] == 'INV-PBT3B-' + _t, ('el invariante comun %s del paquete difiere del de PB-T3b' % n)
    INV_COMMON.append('INV-PBT3C-' + _t)
STOP_COMMON = {}
for cid in COMMON_STOPS:
    _lab = STOP_LABELS.get(cid, cid)
    _t = one(B60, _lab + ': ')[len(_lab) + 2:]
    assert T3B_STOP['PBT3B-' + cid] == _t, ('la parada comun %s del paquete difiere de la de PB-T3b' % cid)
    STOP_COMMON[cid] = _t
one(B60, 'PBT3x-C08 y PBT3x-C12, con el texto de [PKT1] section 6.0 y el prefijo cambiado: ')
assert 'en PB-T3c, con su prefijo (PBT3C, --task PBT3C y "PB-T3c"):' in one(
    B60, 'PBT3x-C11, con los sustitutos de las decisiones del Owner (F4B-E40 y F4B-E43; V46-01, CA-T3b M-01; '), 'la cabecera del bloque de C11 (V46-01)'
assert C11_FOLD == T3B_STOP['PBT3B-C11'], 'el bloque citado de PBT3x-C11 de la %s no es el PBT3B-C11 del contrato reemitido de PB-T3b' % PKG_VERSION
_err = rr_pkg_check(SEC60, [c for _, c, _ in CLASS_CTS])
assert not _err, ('la regla RR del section 6.0 (PB-T3c)', _err)
_err, RR3_INFO = rr3_table_check(SEC60, [c for _, c, _ in CLASS_CTS])
assert not _err, ('la tabla de RR-3 del section 6.0, filas de PB-T3c', _err)
RR3_DEFERRED = [c for c, v in RR3_INFO.items() if v is None]
assert RR3_DEFERRED == [], ('la v4.7 enumera la fila de CT-05 (V47-04): ninguna fila aplazada', RR3_DEFERRED)
# la fila de CT-05 (V47-04): las ocho fichas de D2 y los nombres de las tres reglas, comprobados contra PushBackSharedReferenceGuardTests.cs de la revision
_ct05 = RR3_INFO['PushBackWritePathPluginGuardTests'][0]
D2_TOKENS = ['WorkingBaseline', 'workingBaseline', 'LastModuleReconciliation', 'computation.System', 'lastComputation.System', 'session.System',
             'lastCompositeSystem', 'SystemToInsert']
assert _ct05 == D2_TOKENS + ['Rule1', 'Rule1b', 'Rule1c'], ('la fila de CT-05 de la tabla de RR-3', _ct05)
_srg = lines_of(SRG)
assert _srg[34].strip() == '"WorkingBaseline", "workingBaseline", "LastModuleReconciliation", "computation.System", "lastComputation.System",' and \
    _srg[35].strip() == '"session.System", "lastCompositeSystem", "SystemToInsert",', 'las fichas de D2 en PushBackSharedReferenceGuardTests.cs:35-36'
for _k, _r in ((57, 'Rule1('), (60, 'Rule1b('), (68, 'Rule1c(')):
    assert ('private static List<string> ' + _r) in _srg[_k - 1], ('la regla de CT-PBT1B-14 en su linea', _k, _r)
RR3_SUMMARY = ('2 clases, una fila cada una; PushBackViewEraseGuardTests %d lineas y %d valores; PushBackWritePathPluginGuardTests %d lineas (las ocho '
               'fichas de D2 de %s:35-36 y las reglas Rule1, Rule1b y Rule1c de :57-69, en la revision)'
               % (len(RR3_INFO['PushBackViewEraseGuardTests'][0]), len(RR3_INFO['PushBackViewEraseGuardTests'][1]), len(_ct05), os.path.basename(SRG)))
# la v4.7: el total por rama (V47-03), la precondicion 13 (V47-02) y el estado del cierre de PB-T3b (V47-01), en el texto
_pk = '\n'.join(PKG_LINES)
for _need in ('**Una prueba, una fila** (V47-03): CT-PBT3C-01 a 05 son **un `[Fact]` cada una**, sin `Theory`.',
              '- **El total exacto de cada rama** (F4B-E46; V47-03): un recuento previsto que dependa de c' + chr(0xF3) + 'mo cuente la TRX lleva el total exacto de cada rama',
              '13. **Para PB-T3c, antes de emitir** (V47-02 y V47-05):', '**ejecutando la suite completa de Core sobre esa copia** (V47-02; CA-T3b',
              '   - **En `fe1c67f4`** (V47-01): **PB-T3b, CERRADA** (F4B-E46'):
    assert _pk.count(_need) == 1, ('la v4.7 sin', _need[:70])
# la linea base y lo previsto, con sus cuentas: la de la CI del cierre, y el RED y el GREEN en su unica rama (+5: 4 que fallan en el RED y 1 que pasa)
_run, _sha, _core, _cskip, _ui, _skip = CI_BASE_T3B
for _frag in ('run %s sobre el GREEN %s, Core %s resultados, con %s Passed y %d Skipped' % (_run, _sha, _spaced(_core), _spaced(_core - _cskip), _cskip),
              'y UI %s, con %s Passed y %d Skipped.' % (_spaced(_ui), _spaced(_ui - _skip), _skip),
              'El RED: Core %s resultados, con %s Passed, 4 Failed (CT-01 a 04) y %d Skipped.' % (_spaced(_core + 5), _spaced(_core - _cskip + 1), _cskip),
              'El GREEN: Core %s, con %s Passed, 0 Failed y %d Skipped.' % (_spaced(_core + 5), _spaced(_core - _cskip + 5), _cskip),
              'UI, en los dos: %s, con %s Passed y %d Skipped.' % (_spaced(_ui), _spaced(_ui - _skip), _skip)):
    assert CI_LINE_PKG.count(_frag) == 1, ('la linea base y lo previsto de la CI (section 6.3) no cuadran con CI_BASE_T3B', _frag)
_s60 = md_fold(SEC60)
assert ('P-01 dice "' + P01 % ('K', SNAP_K) + '"') in _s60, 'P-01 del section 6.0 (la Snapshot K)'
assert 'El limite de D2, en cada contrato: PBT3x-C17, con el texto literal del Owner ([PKT1] section 6.0).' in _s60, 'C17 del section 6.0'
assert 'Los TaskId: de 24 a 26 caracteres, por MAX_PATH (R-T1-07).' in _s60 and 'StopConditions estandar (17), copiadas de [PKC2]' in _s60
assert 'y WR en PB-T3c' in one(B60, 'cada contrato declara UNIT_CHANGE '), 'UNIT_CHANGE de PB-T3c del section 6.0'
assert 'ui-tests en failure (PB-T3a) o en success (PB-T3b y PB-T3c)' in one(B60, 'ExpectedEvidence lo pide asi: ')
for cell in ('Worker anthropic/subagent/claude-sonnet-5-5, effort high;', 'Controller openai/process/gpt-6-luna, effort high;', 'vencen el 2026-12-29;'):
    one(B60, cell)
assert 'en PB-T3a y PB-T3c, el prefijo tests/RackCad.Tests/Workspace/, que la lista curada de PB-T0 ya trae;' in one(B60, 'en tests/: ')
assert 'en PB-T3b y PB-T3c, frente al cierre de la subtarea anterior de PB-T3 (vacio);' in one(B60, 'el diff de src (V42-01): ')
one(B60, 'las 17 entradas de la raiz.')
# 5.4: las clases, la lista CT-PBT3C-01 a 05 (05 no falla en el RED) y el esqueleto de la fachada
_s54 = md_fold(SEC54)
assert 'Las clases: PushBackViewEraseGuardTests (01 a 04) y PushBackWritePathPluginGuardTests (05), en tests/RackCad.Tests/.' in _s54, 'las clases de 5.4'
_rows = [re.match(r'^\| \*\*(\d\d)\*\*', l).group(1) for l in SEC54 if re.match(r'^\| \*\*\d\d\*\*', l)]
assert _rows == ['%02d' % n for n in range(1, 6)], ('la lista CT-PBT3C-01 a 05 de 5.4', _rows)
_nofail = [n for n in range(1, 6) if [l for l in SEC54 if l.startswith('| **%02d**' % n)][0].rstrip().endswith('**no falla en el RED** |')]
assert _nofail == [5], ('5.4: solo CT-05 no falla en el RED (lo previsto del RED, 4 Failed, depende de ello)', _nofail)
assert 'cuyo cuerpo es solo throw new NotSupportedException();, sin ninguna otra sentencia y sin la cadena PushBack' in _s54, 'el esqueleto de la fachada'
PKG_CHECKS = ["sin (A') en %d textos copiados" % len(_copied), 'Objective (v4.7)', 'AllowedWriteScope (4)', 'UNIT_CHANGE (WR)', 'el RED', 'RequiredTests',
              'INV-PBT3C-01..04', 'paradas propias (C04, C13, C16, C17)', 'ExpectedEvidence (7)', 'linea base', 'dos notas fuera (6.3)',
              'comunes 07..14 y paradas == PB-T3b', 'C11 de la v4.7 == PBT3B-C11 reemitido', 'regla RR con la lectura de PB-T3c', 'tabla de RR-3 de PB-T3c',
              'P-01 K', 'C17', 'TaskId', 'UNIT_CHANGE de 6.0', 'CI del RED', 'celdas', 'prefijo Workspace/ en tests/', 'anclaje frente al cierre de PB-T3b',
              'clases, CT y esqueleto de 5.4']
assert hashlib.sha256(PKG_RAW).hexdigest() == CUSTODY_SHA256[PKG], ('el paquete leido no es el %s (sha256)' % PKG_VERSION, hashlib.sha256(PKG_RAW).hexdigest())
PKG_CHECKS.append('sha256 de la v4.7')


# ---------------------------------------------------------------- PBT3C-C11 (V46-01): el bloque citado de la v4.6 (el PBT3B-C11 emitido) con el prefijo de PB-T3c
C11_SUBS = [('PBT3B', 'PBT3C'), ('PB-T3b', 'PB-T3c')]  # PBT3B cubre el id, el registro de nc2 y nc1 y --task PBT3B
C11 = C11_FOLD
for _old, _new in C11_SUBS:
    assert _old in C11, ('PBT3C-C11: el prefijo de PB-T3b no esta en el bloque citado', _old)
    C11 = C11.replace(_old, _new)
for _need in ('PBT3C-NC2-EXACT-WHITELIST-F4B-01', 'with --task PBT3C in the F4B-E21 mode', 'SATISFIED_UNDER_PBT3C_EXACT_WHITELIST_SUBSTITUTE_F4B',
              'with --task PBT3C, recorded as nc1 = SATISFIED_UNDER_PBT3C_NC1_CASCADE_SUBSTITUTE', NC2_V2 + ', SHA-256 ' + NC2_V2_SHA,
              NC1_SUBST + ', SHA-256 ' + NC1_SUBST_SHA, 'the nc2 of PB-T3a (F4B-E43) does not extend'):
    assert C11.count(_need) == 1, ('PBT3C-C11', _need)
assert C11.endswith('no one-off acceptance extends to PB-T3c') and not re.search(r'PBT3B|PB-T3b|PBT3A-|IDENTITY-SIGNAL', C11), 'PBT3C-C11: restos de otra subtarea'
# los sustitutos, en la revision con su SHA-256 (nc1 desde el cierre de PB-T3a; nc2 v2, su revision y sus limites desde la emision de PB-T3b)
assert hashlib.sha256(show_bytes(NC1_SUBST)).hexdigest() == NC1_SUBST_SHA, ('el sustituto de nc1 de PD-F4B-01 no esta en la revision con su SHA-256', NC1_SUBST)
_v2 = show_bytes(NC2_V2)
assert hashlib.sha256(_v2).hexdigest() == NC2_V2_SHA, ('el sustituto de nc2 v2 no esta en la revision con su SHA-256', NC2_V2)
for _p in (NC2_V2_REVIEW, NC2_V2_LIMITS):
    assert in_head(_p) and NC2_V2_SHA.upper()[:8] in show(_p).upper(), ('la revision o los limites del sustituto de nc2 v2', _p)
# el v2 admite --task PBT3C (su lista TASKS) y su lista cerrada de literales (F4B_EXACT_EVIDENCES): se leen del script, sin ejecutarlo
import ast as _ast
_tree = _ast.parse(_v2.decode('utf-8'))
_lit = {t.id: n.value for n in _tree.body if isinstance(n, _ast.Assign) for t in n.targets if isinstance(t, _ast.Name)}
NC2_TASKS = _ast.literal_eval(_lit['TASKS'])
NC2_EVIDENCES = _ast.literal_eval(_lit['F4B_EXACT_EVIDENCES'])
assert NC2_TASKS.get('PBT3C') == 'PB-T3c', ('el sustituto de nc2 v2 no admite --task PBT3C', NC2_TASKS)
assert TASK.startswith('F4B-PBT3C-'), 'la ligadura de --task PBT3C: el TaskId de la delegacion empieza por F4B-PBT3C-'

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


# ---------------------------------------------------------------- RequiredTests (section 6.3): los nuevos, las guardas del escritor de F4 (medidas tras PB-T3b),
# CT-PBT1B-15, la UI de paridad (V46-03) y "los de section 6.2 sin ExpectRed", con ExpectRed false (CA-T3c M-01) y los minimos del contrato de PB-T3b
_t3b_req = [(r['Project'], r['Filter'], r['MinSelected'], r['ExpectRed']) for r in pbt3b['RequiredTests']]
assert len(_t3b_req) == 31, len(_t3b_req)


def fq(p, f):
    return ('FullyQualifiedName~%s.%s' % ('RackCad.UI.Tests' if p == 'UI' else 'RackCad.Tests', f), UIP if p == 'UI' else CP)


F4_SKIPS = {'WorkspacePilotUpdateGuardTests': 1, 'WorkspacePilotUpdateIntegrationGuardTests': 2}  # S-4; S-5 y S-6 (D-1), con Skip desde el RED de PB-T3b
REQ_SPEC = [('Core', 'PushBackViewEraseGuardTests', 4, True, 'new'), ('Core', 'PushBackWritePathPluginGuardTests', 1, False, 'new'),
            ('Core', 'WorkspacePilotUpdateGuardTests', None, False, 'f4'), ('Core', 'WorkspacePilotUpdateIntegrationGuardTests', None, False, 'f4'),
            ('Core', 'PushBackSharedReferenceGuardTests', 2, False, 't3b'), ('UI', 'PushBackFullDraftWindowParityTests', 1, False, 'parity')]
# CA-T3c M-01 (la duda 1 de generator-doubts-pbt3c.md, decidida por CA-T3c: incluir): la celda "Y los de section 6.2 sin ExpectRed" se lee "con
# ExpectRed false". Entran todos los filtros de PB-T3b, tambien los dos que alli llevaban ExpectRed verdadero, con ExpectRed false y el MinSelected
# del contrato reemitido de PB-T3b (como el section 6.2 reanade con false PushBackFullDraft de PB-T3a). La v4.7 no cambia
M01_FILTERS = {'PushBackFullWrite': 13, 'PushBackPilotUpdateEquivalenceGuardTests': 6}
for proj, filt, m, er in _t3b_req:
    p = 'UI' if proj == UIP else 'Core'
    _ns = 'FullyQualifiedName~' + ('RackCad.UI.Tests.' if p == 'UI' else 'RackCad.Tests.')
    assert filt.startswith(_ns), filt
    f = filt[len(_ns):]
    if (p, f) in [(x[0], x[1]) for x in REQ_SPEC]:
        continue
    if er:
        assert (p, f) in [('Core', k) for k in M01_FILTERS] and M01_FILTERS[f] == m, ('CA-T3c M-01: un filtro de PB-T3b con ExpectRed que no es de M-01', p, f, m)
    REQ_SPEC.append((p, f, m, False, 't3b'))
for _f, _m in M01_FILTERS.items():
    _hit = [r for r in REQ_SPEC if (r[0], r[1]) == ('Core', _f)]
    assert len(_hit) == 1 and _hit[0][2] == _m and _hit[0][3] is False and _hit[0][4] == 't3b', \
        ('CA-T3c M-01: el filtro de PB-T3b tiene que estar una vez, con ExpectRed false y su MinSelected', _f, _m, _hit)
assert [r for r in REQ_SPEC if r[3]] == REQ_SPEC[:1] and [r for r in REQ_SPEC if r[4] == 'new'] == REQ_SPEC[:2]
_t3b_by = {(proj, filt): (m, er) for proj, filt, m, er in _t3b_req}
COUNTS, UI_FILES = {}, set()
for proj in ('UI', 'Core'):
    ns_prefix, totals, texts = count_static(proj)
    for p, f, _, _, _ in REQ_SPEC:
        if p == proj:
            sel = {k: v for k, v in totals.items() if k.startswith(ns_prefix + '.' + f)}
            COUNTS[(p, f)] = (len(sel), sum(v[0] for v in sel.values()), sum(v[1] for v in sel.values()), sum(v[2] for v in sel.values()))
            if proj == 'UI':
                UI_FILES |= {x for v in sel.values() for x in v[3]}
    if proj == 'UI':
        UI_TEXTS = texts
_cfg = sorted(p for p in UI_FILES if re.search(r'new\s+RackFrameConfiguratorWindow\s*\(', re.sub(r'//[^\n]*', '', re.sub(r'"(?:\\.|[^"\\\n])*"', '""', UI_TEXTS[p]))))
assert not _cfg, ('F4B-E30: un filtro de UI de PB-T3c crea el configurador real', _cfg)
REQ, REQ_REPORT, T3B_PENDING = [], [], []
for p, f, pkg_min, er, kind in REQ_SPEC:
    filt, proj = fq(p, f)
    classes, run, skip, member = COUNTS[(p, f)]
    note = None
    if kind == 'new':
        assert classes == 0 and run == 0, ('la clase nueva ya existe', f)
        mins = pkg_min
    elif kind == 'f4':
        # "los medidos tras PB-T3b": los no saltados en el cierre de PB-T3b (MINSEL_T3B, un marcador). Sin el marcador, lo medido en la revision menos
        # los Skip de D-1 que pone el RED de PB-T3b en esa clase, si aun no estan (una cota declarada)
        name = f.split('.')[-1]
        if MINSEL_T3B and name in MINSEL_T3B:
            mins = MINSEL_T3B[name]  # los Passed de la TRX del GREEN de PB-T3b (count-rows-out.txt); lo estatico no cuenta las filas de MemberData
            assert run >= mins or (member > 0 and run + member * 50 >= mins), (f, run, member, mins)
            note = ('     %s: %d, los Passed de la TRX del GREEN de PB-T3b (%s); medido estatico %d ejecutables, %d skip y %d Theory con MemberData'
                    % (name, mins, os.path.basename(ROWS_OUT), run, skip, member))
        else:
            mins = run - (F4_SKIPS[name] if skip == 0 else 0)
            note = '     %s: medido %d (skip %d) - %d de D-1 = %d: MINSEL_T3B es un marcador (<tras el cierre de PB-T3b>)' % (name, run, skip, F4_SKIPS[name] if skip == 0 else 0, mins)
            T3B_PENDING.append(name)
    elif kind == 'parity':
        assert classes == 1 and run >= pkg_min, ('la UI de paridad de PB-T3a', run)
        mins = pkg_min
    else:
        prev, _ = _t3b_by[(proj, filt)]
        assert prev == pkg_min, (f, prev, pkg_min)
        if classes == 0:  # las clases de PB-T3b llegan con su GREEN
            T3B_PENDING.append(f)
        elif run < pkg_min:
            assert member > 0, ('el recuento medido de %s %s (%d) es menor que el minimo del contrato de PB-T3b (%d)' % (p, f, run, pkg_min))
            note = '     estatico %d (+%d Theory con MemberData) < %d: vale porque el contrato de PB-T3b fija el mismo filtro con %d' % (run, member, pkg_min, pkg_min)
        mins = pkg_min
    REQ.append((proj, filt, mins, er))
    REQ_REPORT.append('%-4s %-48s %-6s paquete=%-5s medido: clases=%d ejecutables=%d skip=%d memberdata=%d -> MinSelected=%d ExpectRed=%s'
                      % (p, f, kind, pkg_min if pkg_min is not None else '-', classes, run, skip, member, mins, er))
    if note:
        REQ_REPORT.append(note)
soft(not T3B_PENDING, 'cierre de PB-T3b: filtros cuyo minimo se mide tras PB-T3b o cuya clase aun no esta en la revision: ' + ', '.join(T3B_PENDING))
for proj, _, _, _ in REQ:
    assert in_head(proj), proj
NEW_CLASSES = [c for _, c, _ in CLASS_CTS]
for p, f, _, _, k in REQ_SPEC:
    news = [c for c in NEW_CLASSES if p == 'Core' and c.startswith(f)]
    assert (k == 'new') == bool(news), ('un filtro y las clases nuevas', p, f, news)
cells = json.loads(json.dumps(pbt3b['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna'] and all('high' in c['TestedEfforts'] for c in cells)

# ---------------------------------------------------------------- invariantes: los propios (01 a 04) y los comunes (07 a 14), literales
INV = INV_OWN + INV_COMMON
assert [i.split('. ')[0] for i in INV] == ['INV-PBT3C-%02d' % n for n in (1, 2, 3, 4, 7, 8, 9, 10, 11, 12, 13, 14)] and 'PBT3C-C13' in INV[10]

# ---------------------------------------------------------------- la evidencia: lo de section 6.1 con lo de PB-T3c, las propias y la linea base
_t3a_ev = json.loads(show(PILOT + '/F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json'))['ExpectedEvidence']
assert len(_t3a_ev) == 10, 'la ExpectedEvidence del contrato de PB-T3a'
EVID_61 = []
for k, e in enumerate(_t3a_ev[:9]):
    if k in EVID_T3A_SUBS:
        _a, _b = EVID_T3A_SUBS[k]
        assert e.count(_a) == 1, ('la vineta %d de la ExpectedEvidence de PB-T3a sin su texto de PB-T3a' % k, _a[:60])
        e = e.replace(_a, _b)
    EVID_61.append(e)
# nc2 v2: la primera entrada es la de PB-T3a, byte a byte, y esta en la lista cerrada de literales del sustituto (F4B_EXACT_EVIDENCES)
assert EVID_61[0] == pbt3b['ExpectedEvidence'][0] and EVID_61[0] in NC2_EVIDENCES, ('la primera evidencia no esta en F4B_EXACT_EVIDENCES del v2', EVID_61[0])
# la linea base y lo previsto (F4B-E46, el total exacto de cada rama): el literal de la v4.7, cuyas cuentas c10 cuadra con CI_BASE_T3B; sin alternativa
CI_TXT = CI_LINE_PKG
assert ' o ' not in CI_TXT.split('Previstos,')[1].replace(' o Failed', ''), 'lo previsto de la CI lleva una alternativa: F4B-E46 pide el total de cada rama'
EVID = EVID_61 + EVID_OWN + [CI_TXT]
assert len(EVID) == 15  # 9 de section 6.1, 5 propias y la de la CI

# ---------------------------------------------------------------- paradas: las 17 estandar, las comunes, las de [DP] y las propias del section 6.3
STOP_TEXT = dict(STOP_COMMON)
for cid in ('C08', 'C12'):
    STOP_TEXT[cid] = T3B_STOP['PBT3B-' + cid]
STOP_TEXT['C11'] = C11
for cid, txt in STOP_OWN_PKG:
    STOP_TEXT[cid] = T3B_STOP['PBT3B-' + cid] if cid in ('C04', 'C17') else txt  # "as PBT3A-C04" y "the D2 limit": los textos de PB-T3b
STOP_IDS = ['C01', 'C02', 'C03', 'C04', 'C05', 'C06', 'C07', 'C08', 'C09', 'C10', 'C11', 'C12', 'C13', 'C16', 'C17', 'C18']
assert sorted(STOP_TEXT) == sorted(STOP_IDS)
STOPS = P_STOPS + [{'Id': 'PBT3C-' + cid, 'Text': STOP_TEXT[cid]} for cid in STOP_IDS]
assert len(STOPS) == 33 and len({s['Id'] for s in STOPS}) == 33
for s in STOPS[17:]:
    # los comunes del section 6.0 van literales, y C02 nombra sus dos excepciones de PB-T3b (INV-PBT3B-15): igual que en el contrato de PB-T3b
    if s['Id'][len('PBT3C-'):] in COMMON_STOPS:
        assert s['Text'] == T3B_STOP['PBT3B-' + s['Id'][len('PBT3C-'):]], s['Id']
        continue
    assert not re.search(r'PBT1[ABC]-(C\d|NC)|SUS3[ABC]|(?<!CT-)PBT3[AB]-|as PBT3A', s['Text']), s['Id']


# ---------------------------------------------------------------- autoridades
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
PKT1 = 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md'
SUPERSEDING = PILOT + '/F4B-DRAFTS/superseding-characterization.md'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
_disp = show(DISP).replace('\r\n', '\n')
_disp_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
assert _disp_ids == list(range(1, len(_disp_ids) + 1)) and _disp_ids[-1] >= 46, ('f4b-dispositions.md', _disp_ids[-3:])
DISP_LAST = 'F4B-E%02d' % _disp_ids[-1]
for _e, _needle in (('F4B-E25', 'owner-f4b-correction-unit-change-decision.md'), ('F4B-E29', 'owner-pbt3-d1-decision.md'),
                    (ISSUE_AUTH[0], 'owner-f4b-pbt3-emission-red-repair-decisions.md'), ('F4B-E37', 'Snapshot K'), ('F4B-E40', 'owner-pbt2c-nc1-decision.md'),
                    ('F4B-E43', 'owner-pbt3a-nc2-decision.md'), (PT3B_EMIT0_ENTRY, PT3B_TASK), (PT3B_EMIT_ENTRY, 'owner-pbt3b-overload-decision.md'),
                    (T3B_CLOSE_ENTRY, 'owner-pbt3b-nc3-decision.md')):
    _i = _disp.index('\n## %s. ' % _e)
    assert _needle in _disp[_i:_disp.find('\n## F4B-E', _i + 1) if _disp.find('\n## F4B-E', _i + 1) > 0 else len(_disp)], (_e, _needle)
# la emision de PB-T3c (F4B-E47): la entrega el commit X; hasta entonces, AVISO. La numeracion: sigue al cierre de PB-T3b
E47_OK = bool(re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (EMIT_ENTRY, TASK), _disp))
soft(E47_OK, 'emision: %s (la emision de %s) aun no esta en f4b-dispositions.md' % (EMIT_ENTRY, TASK))
assert DISP_LAST in (T3B_CLOSE_ENTRY, EMIT_ENTRY), ('f4b-dispositions.md termina en una entrada que no es el cierre de PB-T3b ni la emision de PB-T3c', DISP_LAST)
assert int(EMIT_ENTRY.split('E')[-1]) == int(T3B_CLOSE_ENTRY.split('E')[-1]) + 1, ('la numeracion: la emision de PB-T3c sigue al cierre de PB-T3b', EMIT_ENTRY)
_evs = re.findall(r'(?m)^## (\d+)\. ', show('docs/automation/evidence/I-64-evidence.md'))
assert _evs[-1] in (CLOSE_EV, X_EV) and int(X_EV) == int(CLOSE_EV) + 1 and int(BASE_EV) == int(X_EV) + 1, ('las secciones de la evidencia', _evs[-3:],
                                                                                                         CLOSE_EV, X_EV, BASE_EV)
_e35 = show(ISSUE_AUTH[1])
_err = rr_check(_e35) + q3_check(_e35)
assert not _err, ('F4B-E35', _err)
if soft(git('cat-file', '-e', AR + '^{commit}').returncode == 0, 'emision: la AuthorityRevision %s no esta en el almacen de objetos (seco)' % AR[:8]):
    assert git('merge-base', '--is-ancestor', PT3B_CLOSE, AR).returncode == 0, 'la AuthorityRevision no contiene el cierre de PB-T3b'
F4T4 = sorted(p for p in tracked('docs/initiatives/') if re.fullmatch(r'docs/initiatives/I-64-f4-t4[a-z0-9-]*-gate-contract-package\.md', p))
assert len(F4T4) == 6, F4T4
UNIT_CHANGE = [(WR, 'el escritor de la actualizacion del piloto: PB-T3c le da la capacidad de borrar por la fachada (D-PBT3-11; section 6.0 del '
                    'paquete, F4B-E25)')]
EXISTING = [
    (PHASE, 'paquete de la fase F4B v3.1 ([P]): la fila de PB-T3 del section 5 (:505); OB-04.1 y OB-04.2; R-PB-03; section 7.3 y 7.4'),
    (CO + 'owner-f4b-disposition.md', 'archivo completo ([OD]): la fila de PB-T3; D-PB-04 (borrar si); D-PB-10; el bloque sobre T1B (el registro declarado de D-2)'),
    (PILOT + '/F4B-PBT0-CHARACTERIZATION/block-m/block-m-analysis.md', 'el bloque M de PB-T0: el borrado de las referencias, tambien anidadas (BM:54-60)'),
    (PKT1, 'el paquete de PB-T1 v2.8 custodiado ([PKT1]): D-PBT1-15 (D2); section 6.0 (C08, C12 y C17); la forma de PBT1C-C13 por nombre (:1338)'),
    (CO + 'owner-d1-d2-decisions.md', 'archivo completo ([OD2], F4B-E11): D2 con sus diez condiciones y su limite, el texto literal de PBT3C-C17'),
    (CO + 'owner-f4b-procedure-decisions.md', 'archivo completo ([DP], F4B-E03): firmas exactas de los sustitutos y la replanificacion unica'),
    (CO + 'owner-pbt1a-nc1-decisions.md', 'archivo completo (F4B-E15): las tres comprobaciones deterministas de la verificacion'),
    (SNAP_K_DEC, 'archivo completo (F4B-E37): la Snapshot K (' + SNAP_K + ', ConfigSha256 ' + SNAP_K_SHA[:8] + '), la que cita P-01'),
    (CO + 'owner-f4b-opaque-collections-decision.md', 'archivo completo (F4B-E21): la sesion pone byte a byte las ocho colecciones opacas del contrato'),
    (CO + 'owner-pbt1b-rework-decisions.md', 'archivo completo (F4B-E22): este contrato lleva el texto correcto de INV-13, C13 y la CI del RED'),
    (F4B_E25, 'archivo completo (F4B-E25): las rutas UNIT_CHANGE en una correccion; PB-T3c declara WR (section 6.0 del paquete)'),
    (CO + 'owner-pbt3-d1-decision.md', 'archivo completo (F4B-E29): D-1, la variante (A); no cubre el escritor (PBT3C-C16)'),
    (ISSUE_AUTH[1], 'archivo completo (F4B-E35): pregunta 3, la emision condicional de PB-T3a, PB-T3b y PB-T3c en ese orden; pregunta 4, la regla de la '
                    'reparacion del RED, RR-1 a RR-7, con el tramo RedSha..RedRepairSha que en PB-T3c no toca WR ni la fachada'),
    (CO + 'owner-pbt2c-nc1-decision.md', 'archivo completo (F4B-E40): la firma nueva de nc1 PD-F4B-01 y su sustituto, el de PBT3C-C11'),
    (NC1_SUBST, 'el sustituto de nc1 de PD-F4B-01 (F4B-E40), SHA-256 ' + NC1_SUBST_SHA + ', revisado; PBT3C-C11 lo usa con --task PBT3C'),
    (CO + 'owner-pbt3a-nc2-decision.md', 'archivo completo (F4B-E43): el sustituto de nc2 ampliado, de PB-T3b en adelante (PBT3C-C11)'),
    (NC2_V2, 'el sustituto de nc2 v2 (F4B-E43), SHA-256 ' + NC2_V2_SHA + ': el de PBT3C-C11, con --task PBT3C y su lista cerrada de literales'),
    (NC2_V2_LIMITS, 'la version revisada del sustituto de nc2 v2 y sus limites declarados (M-V1 a M-V4)'),
    (NC2_V2_REVIEW, 'la revision independiente del sustituto de nc2 y su seccion "Recomprobacion v2": APTO, B 0 / I 0 / M 4'),
    (CO + 'owner-pbt3b-overload-decision.md', 'archivo completo (F4B-E45): "Renombrar los metodos nuevos" y su nota posterior (PrepareFull): la leccion de '
                                              'las sobrecargas y la reflexion, que PB-T3c aplica con su barrido (T3C-1) y la suite completa sobre la copia'),
    (CO + 'owner-pbt3b-nc3-decision.md', 'archivo completo (F4B-E46): "Aceptar por disposicion (Recomendado)": desde PB-T3c, una evidencia esperada que '
                                         'dependa de como cuente la TRX lleva el total exacto de cada rama (la linea base y lo previsto de la CI de este '
                                         'contrato, en su unica rama)'),
    (ROWS_OUT, 'el recuento por clase de la sesion en el cierre de PB-T3b (F4B-E46): la fuente de la linea base de la CI (19 124, con 6 Skipped; la Theory '
               'S-2 con Skip como una fila) y de los MinSelected 86 y 123 de las guardas del escritor'),
    (DISP, 'archivo completo: F4B-E01 a ' + DISP_LAST),
    (PBT3BC, 'el contrato reemitido de PB-T3b (F4B-E45; AuthorityRevision ' + PT3B_EMIT_X[:8] + '): las listas de tests/, los filtros y sus MinSelected, las '
             'celdas, las paradas estandar y de [DP], los textos comunes del section 6.0 y PBT3B-C11, que este contrato lleva con el prefijo de PB-T3c'),
    (PILOT + '/F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json', 'el contrato de PB-T3a: INV-PBT3A-02 (el criterio del RED que cita '
                                                                         'INV-PBT3C-02) y la ExpectedEvidence del section 6.1, con lo de PB-T3c'),
    (PILOT + '/F4-DRAFTS/gate-close.md', 'el cierre de F4: el escritor del piloto y sus guardas (G-1 a G-10), que siguen en verde'),
] + [(p, 'paquete de F4-T4: el escritor, su puerto y sus guardas (UpdateGuard:182-212)') for p in F4T4] + [
    ('docs/initiatives/I-64-proposal-v5.md', 'la Proposal V5 congelada de I-64: D-10, D-13, D-15 (punto 5) y D-16'),
    ('docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md', 'el paquete de PB-TC ([PKC-C]): el algoritmo del ForbiddenWriteScope'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 'el paquete de PB-TC2 ([PKC2]): las 17 paradas estandar y la raiz; las celdas (:655)'),
    (PILOT + '/F4-DRAFTS/f4-dispositions.md', 'las fuentes de PD-F4-01 y PD-F4-04'),
    ('docs/automation/state/I-64.yml', 'estado vigente completo'),
    ('docs/automation/evidence/I-64-evidence.md', 'las secciones de F4B (231 y siguientes)'),
]
BOUNDED_CHECK = D3 + 'independent-review-pbt3c-check.md'  # el nombre que propone el generador
# las de la emision de PB-T3c (el commit X): el paquete v4.7, la remedicion, CA-T3c y el registro declarado de D-2; las de CUSTODY son autoridades
CUSTODY = [
    (PKG, 'archivo completo (' + PKG_VERSION + ', sha256 ' + CUSTODY_SHA256[PKG][:8] + '): D-PBT3-11 (la fachada y el escritor) y D-2; section 4 y 4.1 (con '
          'los barridos de PB-T3c sobre el cierre de PB-T3b); section 5.0 y 5.4 (CT-PBT3C-01 a 05, un [Fact] cada una); section 6.0 (con PBT3x-C11 de '
          'F4B-E40 y F4B-E43, la regla RR, la tabla de RR-3 y el total exacto de cada rama de F4B-E46) y 6.3 (el contrato de PB-T3c); las precondiciones '
          '1, 9 y 13'),
    (D3 + 'pbt3c-anchors-out.txt', 'la salida de la remedicion de PB-T3c sobre el cierre de PB-T3b (precondicion 2 y section 6.0 del paquete)'),
    (BOUNDED_CHECK, 'la comprobacion acotada de PB-T3c (CA-T3c), sobre el contrato en seco y la v4.7 (precondiciones 3 y 13)'),
    (SUPERSEDING, 'el registro declarado de las sustituciones: su entrada de D-2, sin Skip (D-PBT3-11; section 8.2 del paquete; V2-11)'),
]
CUSTODY_HEADS = {SUPERSEDING: ['## PB-T3c '], 'docs/initiatives/I-64-f4b-pbt3-measurements.md': ['## R-21. ', '## R-22. ', '## R-23. ']}
CUSTODY_EXTRA = [D3 + n for n in ('package-v46.md', 'v47-delta.md', 'make_v47.py', 'check_v47.py', 'check-v47-out.txt', 'sweep-pbt3c-close.txt',
                                  'overload_sweep_t3c.py', 'overload-sweep-pbt3c-fe1c67f4.txt', 'name_check_t3c.py', 'name-check-pbt3c-fe1c67f4.txt',
                                  'pbt3c-skeleton.patch', 'make_pbt3c_skeleton.py', 'independent-review-pbt3c-check-prompt.md', 'make_pbt3c_contract-dry.txt',
                                  'measure_pbt3c.py', 'compile-check-pbt3c/compile-check.md')]
assert len(CUSTODY_EXTRA) == len(set(CUSTODY_EXTRA)) == 16  # con el informe de la compilacion de comprobacion de la sesion (precondicion 13)
CUSTODY_FREE = [D3 + 'pbt3c-anchors-out.txt', BOUNDED_CHECK] + CUSTODY_EXTRA
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D12 y D13'),
    ('docs/AUTOMATION_PLAN.md', 'section 16.3, 16.5, 16.8, 16.9, 16.10 y 16.11'),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4)'),
    ('docs/automation/agent-execution/routing.md', 'archivo completo'),
    ('docs/automation/agent-execution/model-catalog.md', 'celdas medidas y vigentes para Controller y Worker'),
]
CLOSE_AUTH = [(PT3B_TCP, 'el cierre de PB-T3b (' + CLOSE_ENTRY + '): la base de PB-T3c, con su linea base de la CI (' + CI_BASE_T3B[0] + ')')]
_auth_all = CUSTODY + EXISTING + CLOSE_AUTH + UNIT_CHANGE + EXTERNAL
assert len({p for p, _ in _auth_all}) == len(_auth_all), 'una autoridad repetida'
for p, _ in EXISTING + EXTERNAL + UNIT_CHANGE + CLOSE_AUTH:
    assert in_head(p), ('autoridad inexistente en la revision', p)
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
    'IssuedBy': ('Sesion principal de I-64 (Claude), emision de la tercera subtarea de PB-T3 (' + EMIT_ENTRY + ') por ' + ISSUE_AUTH[0] + ', pregunta 3, la '
                 'emision condicional de PB-T3: PB-T3c tras el cierre de PB-T3b (' + PT3B_CLOSE[:8] + ', ' + CLOSE_ENTRY + '; CLOSED / PASS WITH '
                 'LIMITATIONS sobre ' + GREEN_T3B[:8] + ', intento 2 de 3), con su remedicion sobre ese cierre y su comprobacion acotada ('
                 + (BOUNDED_RESULT or '<resultado por rellenar>') + '); F4B-E46, el total exacto de cada rama de recuento (owner-pbt3b-nc3-decision.md); '
                 'F4B-E45, la leccion de las sobrecargas y la reflexion (owner-pbt3b-overload-decision.md); D-2, el registro declarado de la premisa del '
                 'escritor (D-PB-04 y OB-04.1); F4B-E40 y F4B-E43, los sustitutos de nc1 y nc2 de PBT3C-C11; F4B-E29, D-1, que no cubre el escritor; [OD]; '
                 'F4B-E11, D2; F4B-E03; F4B-E37, la Snapshot K; F4B-E35, pregunta 4, la regla RR; F4B-E21; F4B-E22; F4B-E25, con WR; y el paquete del '
                 'contrato ' + PKG_VERSION + ', con su comprobacion acotada de PB-T3c'),
    'IssuedUtc': ISSUED,
}
N_AUTH = len(_auth_all) + 1
assert len(c['Authorities']) == N_AUTH, len(c['Authorities'])
b = (json.dumps(c, indent=2, ensure_ascii=True) + '\n').encode('ascii')
assert all(x < 0x80 for x in b) and b'\\u' not in b, 'no ASCII'
assert not set(c['AllowedWriteScope']) & set(c['ForbiddenWriteScope'])
for a in c['AllowedWriteScope']:
    assert not any(a.startswith(f) for f in c['ForbiddenWriteScope'] if f.endswith('/')), a
TERMS = ['GATE PASS', 'GATE_PASS', 'Candidato', 'Candidate', 'FINAL_CANDIDATE_SHA', 'gate cerrado', 'gate closed', 'cierre del gate', 'rama integrada', 'integrada en main',
         'integrated into main', 'merged into main', 'lista para integrar', 'ready to merge', 'Owner Validation APROBADA', 'Owner Validation APPROVED']
for t in TERMS:
    assert not re.search(r'(?<![A-Za-z0-9_])' + re.escape(t) + r'(?![A-Za-z0-9_])', b.decode('ascii'), re.I), t
for bad in ('C:' + chr(92), 'C:' + '/', 'D:' + '/', 'D:' + chr(92), 'Users', 'alej' + 'andra', 'App' + 'Data', 'scratchpad', '.co' + 'dex', 'hs' + '65', 'Docu' + 'mentos'):
    assert bad not in b.decode('ascii'), bad
assert VARIANT.encode('ascii') not in b, "el contrato lleva la variante (A') no elegida"


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


# la comprobacion acotada de PB-T3c (CA-T3c; precondiciones 3 y 13): BOUNDED_RESULT, el veredicto literal, y bounded_check, estricto, sobre el informe
soft(BOUNDED_RESULT, 'emision: falta BOUNDED_RESULT, el veredicto literal de la comprobacion acotada de PB-T3c (CA-T3c; precondiciones 3 y 13)')
assert BOUNDED_RESULT is None or (all(ord(ch) < 0x80 for ch in BOUNDED_RESULT) and re.match(r'EMITIBLE(?: TRAS CORRECCIONES)?[.,;]', BOUNDED_RESULT)), \
    ('BOUNDED_RESULT: solo la familia EMITIBLE (EMITIBLE o EMITIBLE TRAS CORRECCIONES)', BOUNDED_RESULT)
BOUNDED_SRC = None  # de donde leyo bounded_check el informe de CA-T3c: en seco, el local (--bounded-file) manda, porque crece antes del commit X
if BOUNDED_FILE:
    _rep = open(BOUNDED_FILE, 'rb').read().decode('utf-8')
    BOUNDED_SRC = '--bounded-file ' + os.path.basename(BOUNDED_FILE)
elif in_head(BOUNDED_CHECK):
    _rep = show(BOUNDED_CHECK)
    BOUNDED_SRC = BOUNDED_CHECK + ' en ' + REV_SHA[:8]
BOUNDED_LAST = None  # la version de la ultima recomprobacion del informe, si la hay
if BOUNDED_SRC:
    _vers = re.findall(r'(?m)^## ' + chr(0xA7) + r'\w+\. Recomprobaci' + chr(0xF3) + r'n (?:sobre|de) la (v\d+(?:\.\d+)?)\b', _rep.replace('\r\n', '\n'))
    BOUNDED_LAST = _vers[-1] if _vers else None
    if BOUNDED_RESULT:
        _err = bounded_check(_rep, BOUNDED_RESULT, PKG_LINES, reviewer='CA-T3c')
        assert not _err, ('la comprobacion acotada de PB-T3c', BOUNDED_SRC, _err)
else:
    soft(False, 'emision: la comprobacion acotada de PB-T3c (%s) aun no existe (ni en la revision ni --bounded-file)' % BOUNDED_CHECK)


# ---- local_paths (inicio)
def local_paths(text):
    """Las rutas locales de un archivo a custodiar (DU-11; el repositorio es publico): una ruta con letra de unidad (X seguida de dos puntos y una
    barra), la carpeta de perfiles de Windows seguida de una barra, o la carpeta de datos de aplicacion del perfil. No busca nombres de personas:
    eso lo cubre la redaccion comun. Devuelve las coincidencias distintas (vacia si no hay). Las palabras de la expresion van con una letra entre
    corchetes ([e], [D]): la misma expresion, sin que su texto case con FORBIDDEN de la redaccion comun cuando el generador se custodia."""
    import re
    return sorted(set(m.group(0) for m in re.finditer(r'(?<![A-Za-z0-9])[A-Za-z]:[\\/]|\bUs[e]rs[\\/]|\bApp[D]ata\b', text)))
# ---- local_paths (fin)
for p, _ in CUSTODY + [(MEAS, None)]:
    if soft(in_head(p), 'emision: a custodiar con la emision de PB-T3c, aun no en la revision: %s' % p):
        _lp = local_paths(show_bytes(p).decode('utf-8', 'replace'))
        assert not _lp, ('rutas locales en un custodiado (se redacta antes de custodiar)', p, _lp)
        if p in CUSTODY_HEADS:
            _lines = lines_of(p)
            soft(all(any(l.startswith(h) for l in _lines) for h in CUSTODY_HEADS[p]),
                 'emision: al custodiado le faltan apartados (%s; los trae el commit X): %s'
                 % (', '.join(h.strip() for h in CUSTODY_HEADS[p] if not any(l.startswith(h) for l in _lines)), p))
for p in CUSTODY_EXTRA:
    if soft(in_head(p), 'emision: a custodiar con la emision, sin ser autoridad, aun no en la revision: %s' % p):
        _lp = local_paths(show_bytes(p).decode('utf-8', 'replace'))
        assert not _lp, ('rutas locales en un custodiado (se redacta antes de custodiar)', p, _lp)
if DRY:
    print('DRY rev', REV_SHA[:8], '(objetos del checkout principal)' if OBJ_REV else '(HEAD del worktree)', 'allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN),
          '(src', len(SRCF), 'tests', len(TESTF), '[core', len(CORE_T), 'ui', str(len(UI_T)) + '] root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'required', len(REQ), 'evidence', len(EVID), 'authorities', len(c['Authorities']), 'anchors-ok', len(ANCHORS), 'bytes', len(b),
          'sha256', hashlib.sha256(b).hexdigest().upper())
    print('anclaje:', ANCHOR_SRC, '| f4b-dispositions hasta', DISP_LAST)
    print('paquete leido:', PKG_SRC, '->', PKG_IN, '| sha256', hashlib.sha256(PKG_RAW).hexdigest()[:16], '| textos de la %s comprobados:' % PKG_VERSION,
          ', '.join(PKG_CHECKS))
    print('contrato de PB-T3b: %s (reemitido, AuthorityRevision %s, %d autoridades) | tests de PB-T3c: Core %d y UI %d; src: %d'
          % (PBT3BC + ' en ' + REV_SHA[:8], pbt3b['AuthorityRevision'][:8], len(pbt3b['Authorities']), len(CORE_T), len(UI_T), len(SRCF)))
    print('PBT3C-C11: el bloque citado con el prefijo de PB-T3c; nc1 %s; nc2 v2 %s, con --task PBT3C en su lista (%s) y la primera evidencia en '
          'F4B_EXACT_EVIDENCES' % (NC1_SUBST_SHA[:8], NC2_V2_SHA[:8], ', '.join(sorted(NC2_TASKS))))
    _free = [p for p in CUSTODY_FREE if not in_head(p)]
    print('nombres de custodia nuevos de F4B-DRAFTS/pbt3/ libres en la revision: %d de %d' % (len(_free), len(CUSTODY_FREE)))
    print('Snapshot K (F4B-E37): ConfigSha256 %s y sonda neutra %s con Pass | F4B-E35: preguntas 3 y 4 | ISSUE_AUTH = %s, pregunta 3'
          % (SNAP_K_SHA[:8], SNAP_K_PROBE, ISSUE_AUTH[0]))
    print('BOUNDED_RESULT: %s | bounded_check: %s | ultima recomprobacion de CA-T3c: %s' % (
        BOUNDED_RESULT or 'por rellenar',
        (('sin errores sobre ' + BOUNDED_SRC) if BOUNDED_RESULT else ('no corre sin BOUNDED_RESULT (informe: %s)' % BOUNDED_SRC)) if BOUNDED_SRC
        else 'sin informe (ni en la revision ni --bounded-file)', BOUNDED_LAST))
    print('emision (%s, tras el cierre %s): %s en f4b-dispositions.md: %s | evidencia: X %s y BASE %s, tras la %s'
          % (EMIT_ENTRY, CLOSE_ENTRY, EMIT_ENTRY, 'si' if E47_OK else 'no (lo entrega el commit X)', X_EV, BASE_EV, CLOSE_EV))
    print('tabla de RR-3 (filas de PB-T3c): %s' % RR3_SUMMARY)
    print('la CI prevista (F4B-E46, el total exacto de su unica rama): %s' % CI_TXT[CI_TXT.index('Previstos,'):])
    print('D1: needs de build-plugin =', _needs, "| (A') en el contrato: %d" % b.count(VARIANT.encode('ascii')))
    print('RequiredTests (criterio de count_tests_git.py sobre los objetos de la revision):')
    for l in REQ_REPORT:
        print('  ' + l)
    print('AVISOS (%d):' % len(WARN))
    for w in WARN:
        print('  - ' + w)
    if '--print' in ARGS:
        for i, f in enumerate(FORBIDDEN, 1):
            print(i, f)
    if JSON_OUT:
        open(JSON_OUT, 'wb').write(b)
    if '--print-json' in ARGS:
        sys.stdout.flush()
        sys.stdout.buffer.write(b)
    sys.exit(0)
assert ISSUE_AUTH and BOUNDED_RESULT and in_head(BOUNDED_CHECK) and E47_OK and hashlib.sha256(show_bytes(PKG)).hexdigest() == CUSTODY_SHA256[PKG], \
    'faltan BOUNDED_RESULT, la comprobacion acotada, F4B-E47 o la v4.7 custodiada'
assert not WARN, WARN
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
