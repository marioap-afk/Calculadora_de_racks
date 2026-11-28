"""Contrato de gate de F4B-PBT2B-WORKSPACE-MODE-WINDOW (ASCII), desde el paquete del contrato de PB-T2 v2.8
(docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md: section 1, D-PBT2-04 a D-PBT2-12, section 5.0, section 5.3, section 6.0 y section 6.2)
y el patron de make_pbt2a_contract.py. La BaseSha es hija del cierre de PB-T2a (PT2A_CLOSE, por rellenar), que desciende de la emision de PB-T2a
(0dc623ca, F4B-E32) y del cierre de PB-T1c (5a8366c1, F4B-E31; W y WC, el GREEN 161a0dad).
El ForbiddenWriteScope de src/ sale de cover() de [PKC-C]; los de tests/ y la raiz, de las listas del contrato emitido de PB-T2a mas sus dos
clases nuevas de Core (section 6.0 del paquete).
Comprueba sobre la revision leida (HEAD del worktree o, con --objects-rev, esa revision en el almacen de objetos del checkout principal):
- el anclaje del section 6.0: la revision desciende del cierre de PB-T1c y de la emision de PB-T2a; src/RackCad.UI, Domain y Plugin, iguales que en
  el GREEN de PB-T1c; Application solo gana los dos archivos de PB-T2a; tests solo las clases de PB-T1c y PB-T2a; con PT2A_CLOSE (constante o
  --pt2a-close), la revision desciende de el, src es igual y es de verdad el cierre (entrada y task-close-package.md);
- las anclas de W y WC: las 100 lineas de las 72 citas del cuerpo del paquete (V23-03 y v2.4), los blobs de W, WC y WX iguales que en el GREEN de
  PB-T1c (V24-16), los rangos de INV-PBT2B-03 con el metodo de INV-PBT1C-03 y sus autopruebas, las 41 costuras, las firmas, S-7, S-9, CT-22, CT-23,
  los testigos, R3 sobre la clausura, que los 26 nombres de PBT2B-C13 estan libres, WX igual; las anclas de las pruebas que copian los CT; G-L1c y
  sus quince autopruebas; las guardas de PB-T1c; los datos de las recetas de la v2.4 (la tolerancia 4 de los 17 fixtures, el nombre de pb-01-base,
  los 49 + 4 gestos de CT-PBT1C-02);
- CA-T2a M-6 (a): las clases de los filtros de UI de PB-T2b que crean un RackFrameConfiguratorWindow real;
- el texto del paquete (la v2.8, por --package-file en seco o custodiada en HEAD): el Objective, el AllowedWriteScope, la tabla de RequiredTests,
  los invariantes propios y comunes (literales y SIN su "Nota (no va al contrato)"), las paradas propias (C13 por nombre, contrastada con
  INV-PBT2B-03, la tabla de los once literales y el enum), P-01, la ExpectedEvidence, las autoridades, la custodia, la costura inerte del section 5.3,
  los once testigos ASCII, la linea base de la CI, los cambios de la v2.3 (V23-01 a V23-07) y los de la v2.4 (V24-01 a V24-16: CA-T2b y la
  revision de las CT), los de la v2.5 (V25-01 a V25-04: la recomprobacion de la seccion 10 de CA-T2b), los de la v2.8 (V28-01 a V28-06) y los de la v2.7 (V27-01 a V27-07: la 
  regla RR de la reparacion del RED de F4B-E35, con su autoridad y rr_check);
- los MinSelected, con el criterio de count_tests_git.py sobre los objetos de la revision: cada minimo, frente a los ejecutables (v2.4, V24-05);
- D1, D2, F4B-E23 y F4B-E28 custodiadas, la parada del intento 1 de PB-T2a (F4B-E33), las celdas.
Lo que depende del cierre de PB-T2a (PT2A_CLOSE, CI_BASE_T2A, el puerto implementado, sus dos clases de prueba y su task-close-package.md) o de la
custodia de F4B-E35 o de la emision de PB-T2b (la v2.8 en HEAD, la comprobacion acotada custodiada, las rutas de CUSTODY y CUSTODY_EXTRA y la entrada de emision en
f4b-dispositions.md) es AVISO en seco y error al escribir; todo lo demas es error siempre.
Uso: python make_pbt2b_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print] [--print-json]
                                   [--objects-rev <rev> [--repo <checkout principal>] [--package-file <paquete>]] [--pt2a-close <sha>]
Con --dry no escribe nada. --objects-rev exige --dry: no hace chdir al worktree y lee todo con git -C <checkout principal> show <rev>:<ruta>.
--package-file exige --objects-rev y --dry, y tiene que ser la v2.8. Al escribir, el paquete es el custodiado en HEAD y tiene que ser la v2.8.
La v2.5 (V25-01..04) es la v2.4 con la recomprobacion de la seccion 10 de CA-T2b: bounded_check lee tambien el veredicto de la ultima recomprobacion.
La v2.7 (V27-01..07) incorpora la regla RR de F4B-E35; PB-T2b se emite ya con ella (revierte V26-06 solo en la version). La v2.8 (V28-01..06)
aplica las recomprobaciones de la v2.7: la verificacion con una reparacion (RR-7) y el bounded_check robusto de PB-T2c.
Al custodiarlo, las rutas locales se redactan ('<worktree-de-I-64>', '<checkout-principal>')."""
import datetime, hashlib, json, os, re, subprocess, sys, unicodedata

# ---------------------------------------------------------------- los cierres de PB-T1b y PB-T1c, la emision de PB-T2a y su cierre (por rellenar)
PB1B_CLOSE = '95373af6116cf120bd0a8ba490833524e33c47da'  # F4B-E26; la base de las mediciones de la v2.2
PT1C_EMIT = '833ea6bfaadfab9afc698aba7f12ad2124a78585'   # F4B-E27
PT1C_CLOSE = '5a8366c166cd1034293811e5c2ac0f7c6e05a761'  # F4B-E31: el cierre de PB-T1c
PT1C_CLOSE_ENTRY = 'F4B-E31'
PT1C_GREEN = '161a0dadccaf5b71886344d834146737b90bb4f3'  # el GREEN de PB-T1c: W y WC de la BaseSha de PB-T2b
CI_BASE_T1C = ('44463212627', '161a0dad', 19088, 2439, 21)  # la CI del GREEN de PB-T1c (F4B-E31)
PT2A_EMIT = '0dc623ca7a4bf1640654440276aa3d27147f48d4'   # F4B-E32: la emision de PB-T2a (AuthorityRevision 1ee10d3f)
PT2A_EMIT_ENTRY = 'F4B-E32'
PT2A_STOP_ENTRY = 'F4B-E33'  # el intento 1 de PB-T2a, parado con PBT2A-C07 (C-1: el momento de una lectura); el cierre es posterior
PT2A_STOP_ANALYSIS = 'docs/automation/evidence/I-64-pilot/F4B-PBT2A-DRAFT-STATE-PORT/1/R20261119T060524Z-55f2/analysis.md'
WINDOW_BLOBS = {'W': 'f8756a88', 'WC': '1a3300b4', 'WX': 'fb094bef'}  # los de 161a0dad, 0dc623ca y 17872be8 (CA-T2b section 4; V24-16)
PT2A_CLOSE = 'b29551aee98234c420832e6c035a0bf7f53bc280'  # F4B-E34: la custodia del cierre de PB-T2a, hija de 5ddeb3b4; tambien --pt2a-close <sha>
PT2A_CLOSE_ENTRY = 'F4B-E34'  # se comprueba frente al titulo "Cierre de F4B-PBT2A-DRAFT-STATE-PORT"
CI_BASE_T2A = ('44480898605', '5ddeb3b4ed99cb17d05142c67d47a26e89e238f9', 19092, 2439, 21)  # la corrida push 4/4 del GREEN de PB-T2a (F4B-E34): (run, sha, Core Passed, UI total, UI Skipped)
CI_T2A_EXPECTED = (19092, 2439, 21)  # la prevision del section 6.2: el cierre de PB-T1c mas PB-T2a (Core + 4)
CI_T2A_EXPLAINED = None  # POR RELLENAR solo si las cifras de CI_BASE_T2A difieren de las previstas: la explicacion por el diff de PB-T2a
# el resultado de la comprobacion acotada de PB-T2b (CA-T2b, section 6.0): el literal que propone su informe una vez corregidas I-01 a I-03 (v2.4)
# (v2.5, V25-01) con la recomprobacion de la v2.4 (la seccion 10 del informe: EMITIBLE, B 0 / I 0 / M 4) y sus cuatro menores aplicadas
BOUNDED_RESULT = ('EMITIBLE TRAS CORRECCIONES (B 0 / I 3 / M 7; I-01, I-02 e I-03 corregidas en el paquete; recomprobacion v2.4: EMITIBLE, B 0 / I 0 / M 4; '
                  'M-08 a M-11 aplicadas en la v2.5; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2)')
PKG_VERSION = 'v2.8'
OWNER_PBT2_ENTRY = 'F4B-E28'
MAIN_CHECKOUT = '<checkout-principal>'  # Redaccion para el repo publico: la ruta local del checkout principal  # solo con --objects-rev; se redacta al custodiar ('<checkout-principal>')

ARGS = sys.argv[1:]
VALUED = ('--objects-rev', '--repo', '--pt2a-close', '--package-file')


def opt(name):
    if name not in ARGS:
        return None
    i = ARGS.index(name)
    assert i + 1 < len(ARGS) and not ARGS[i + 1].startswith('--'), ('falta el valor de', name)
    return ARGS[i + 1]


POS = [a for i, a in enumerate(ARGS) if not a.startswith('--') and not (i > 0 and ARGS[i - 1] in VALUED)]
assert len(POS) == 3, 'uso: make_pbt2b_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print] [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
OBJ_REV = opt('--objects-rev')
PT2A_CLOSE = opt('--pt2a-close') or PT2A_CLOSE
DRY = '--dry' in ARGS
assert OBJ_REV is None or DRY, '--objects-rev solo en seco: anade --dry'
PKG_FILE = opt('--package-file')
assert PKG_FILE is None or (OBJ_REV is not None and DRY), '--package-file solo junto con --objects-rev y --dry'
PKG_FILE = os.path.abspath(PKG_FILE) if PKG_FILE else None
assert PKG_FILE is None or os.path.isfile(PKG_FILE), ('no existe el paquete', PKG_FILE)
assert re.fullmatch(r'[0-9]{4}-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9]Z', ISSUED), ISSUED
assert re.fullmatch(r'[0-9a-f]{40}', AR) and re.fullmatch(r'[0-9a-f]{40}', MAIN), (AR, MAIN)
assert PT2A_CLOSE is None or re.fullmatch(r'[0-9a-f]{40}', PT2A_CLOSE), PT2A_CLOSE

if OBJ_REV is None:
    WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree
    os.chdir(WT)
    REPO, REV = None, 'HEAD'
else:
    REPO, REV = opt('--repo') or MAIN_CHECKOUT, OBJ_REV
DISK = OBJ_REV is None

TASK = 'F4B-PBT2B-WORKSPACE-MODE-WINDOW'
assert len(TASK) == 31, len(TASK)  # section 6.0, Identidad (MAX_PATH; v2.1, V21-05)
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'
WARN = []


def git(*args, inp=None):
    return subprocess.run(['git'] + (['-C', REPO] if REPO else []) + list(args), input=inp, capture_output=True)


_ALL = None


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


def show(path, rev=None):
    r = git('show', (rev or REV) + ':' + path)
    assert r.returncode == 0, ('no se puede leer', rev or REV, path)
    return r.stdout.decode('utf-8')


def lines_of(path, rev=None):
    return show(path, rev).replace('\r\n', '\n').split('\n')


def tracked(prefix):
    return [p for p in _all()[0] if p.startswith(prefix)]


def soft(cond, msg):
    """Lo que solo existira con el cierre de PB-T2a o con la emision de PB-T2b: error al escribir, AVISO en seco."""
    if cond:
        return True
    assert DRY, msg
    WARN.append(msg)
    return False


def nspace(s):
    return s.replace(chr(0xA0), ' ').replace(chr(0x202F), ' ')


def names_of(cmd_out):
    return [p for p in cmd_out.stdout.decode('utf-8').split('\n') if p]


REV_SHA = git('rev-parse', REV).stdout.decode('ascii').strip()
assert re.fullmatch(r'[0-9a-f]{40}', REV_SHA), ('revision desconocida', REV)
assert git('cat-file', '-e', MAIN + '^{commit}').returncode == 0, ('MainSha no esta en el almacen de objetos', MAIN)

# ---------------------------------------------------------------- el anclaje del section 6.0
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
WC = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs'
WX = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
PB1C_TCP = PILOT + '/F4B-PBT1C-LOSSLESS-STATE-WINDOW/task-close-package.md'
PB2A_TCP = PILOT + '/F4B-PBT2A-DRAFT-STATE-PORT/task-close-package.md'
PBT2AC = PILOT + '/F4B-PBT2A-DRAFT-STATE-PORT/coordinator/gate-contract.json'
PBT1CC = PILOT + '/F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json'
for sha, full in (('5a8366c1', PT1C_CLOSE), ('161a0dad', PT1C_GREEN), ('0dc623ca', PT2A_EMIT), ('833ea6bf', PT1C_EMIT), ('95373af6', PB1B_CLOSE)):
    assert git('rev-parse', sha).stdout.decode('ascii').strip() == full, ('SHA', sha)
# el cierre de PB-T1c (F4B-E31): la revision desciende de el, que tiene su entrada, su task-close-package y el src y los tests de su GREEN
assert git('merge-base', '--is-ancestor', PT1C_CLOSE, REV).returncode == 0, 'la revision no desciende del cierre de PB-T1c'
assert ('\n## %s. Cierre de F4B-PBT1C-LOSSLESS-STATE-WINDOW' % PT1C_CLOSE_ENTRY) in show(DISP, PT1C_CLOSE).replace('\r\n', '\n'), 'PT1C_CLOSE no es el cierre de PB-T1c'
assert git('cat-file', '-e', PT1C_CLOSE + ':' + PB1C_TCP).returncode == 0, 'el cierre de PB-T1c sin su task-close-package.md'
assert git('diff', '--quiet', PT1C_GREEN, PT1C_CLOSE, '--', 'src', 'tests').returncode == 0, 'el cierre de PB-T1c no tiene el src y los tests de su GREEN'
# la emision de PB-T2a (F4B-E32)
assert git('merge-base', '--is-ancestor', PT2A_EMIT, REV).returncode == 0, 'la revision no desciende de la emision de PB-T2a (0dc623ca)'
assert ('\n## %s. Emisi' % PT2A_EMIT_ENTRY) in show(DISP, PT2A_EMIT).replace('\r\n', '\n') and git('cat-file', '-e', PT2A_EMIT + ':' + PBT2AC).returncode == 0, (
    'PT2A_EMIT no es la emision de PB-T2a')
# la parada del intento 1 de PB-T2a (F4B-E33): su entrada y el analisis de la sesion, con C-1 (el momento de la lectura que la v2.4 aplica a PB-T2b)
_disp_rev = show(DISP).replace('\r\n', '\n')
assert ('\n## %s. PB-T2a, intento 1: parada PBT2A-C07' % PT2A_STOP_ENTRY) in _disp_rev, 'la parada del intento 1 de PB-T2a (F4B-E33)'
_an = show(PT2A_STOP_ANALYSIS).replace('\r\n', '\n')
assert '**C-1. CT-PBT2A-02, la l' in _an and 'se toman **ANTES** de aplicar la edici' in _an, 'C-1 del analisis de F4B-E33'
pbt2a = json.loads(show(PBT2AC))
pbt1c = json.loads(show(PBT1CC))
assert pbt2a['TaskId'] == 'F4B-PBT2A-DRAFT-STATE-PORT' and pbt1c['TaskId'] == 'F4B-PBT1C-LOSSLESS-STATE-WINDOW'
T2A_SRC = [f for f in pbt2a['AllowedWriteScope'] if f.startswith('src/')]
T2A_TESTS = [f for f in pbt2a['AllowedWriteScope'] if f.startswith('tests/')]
T1C_TESTS = [f for f in pbt1c['AllowedWriteScope'] if f.startswith('tests/')]
assert len(T2A_SRC) == 2 and len(T2A_TESTS) == 2 and len(T1C_TESTS) == 3
# PB-T2a solo anade sus dos archivos de Application: la UI, Domain y Plugin son los del GREEN de PB-T1c; tests, los de PB-T1b mas PB-T1c y PB-T2a
_src = names_of(git('diff', '--name-only', PT1C_GREEN, REV, '--', 'src'))
assert set(_src) <= set(T2A_SRC), ('src cambio fuera de los dos archivos de PB-T2a desde el GREEN de PB-T1c', _src)
assert git('diff', '--quiet', PT1C_GREEN, REV, '--', 'src/RackCad.UI', 'src/RackCad.Domain', 'src/RackCad.Plugin').returncode == 0
_tests = names_of(git('diff', '--name-only', PB1B_CLOSE, REV, '--', 'tests'))
assert set(_tests) <= set(T1C_TESTS + T2A_TESTS), ('tests cambio fuera de las clases de PB-T1c y PB-T2a desde el cierre de PB-T1b', _tests)
ANCHOR_SRC = 'desciende de %s (%s) y de %s (%s); src/RackCad.UI, Domain y Plugin == %s; src fuera de ellos: %s' % (
    PT1C_CLOSE[:8], PT1C_CLOSE_ENTRY, PT2A_EMIT[:8], PT2A_EMIT_ENTRY, PT1C_GREEN[:8], ', '.join(_src) or 'igual')
# el cierre de PB-T2a: "git diff <cierre de PB-T2a> <BaseSha> -- src/RackCad.UI vacio" (section 6.0), y src entero igual
PT2A_ENTRY = None
if PT2A_CLOSE:
    assert git('cat-file', '-e', PT2A_CLOSE + '^{commit}').returncode == 0, ('el cierre de PB-T2a no esta en el almacen de objetos', PT2A_CLOSE)
    assert git('merge-base', '--is-ancestor', PT2A_EMIT, PT2A_CLOSE).returncode == 0, 'PT2A_CLOSE no desciende de la emision de PB-T2a'
    assert git('merge-base', '--is-ancestor', PT2A_CLOSE, REV).returncode == 0, 'la revision no desciende del cierre de PB-T2a'
    assert git('diff', '--quiet', PT2A_CLOSE, REV, '--', 'src').returncode == 0, 'src cambio desde el cierre de PB-T2a'
    _r = git('show', PT2A_CLOSE + ':' + DISP)
    _m = re.findall(r'(?m)^## (F4B-E\d+)\. Cierre de F4B-PBT2A-DRAFT-STATE-PORT\b', _r.stdout.decode('utf-8')) if _r.returncode == 0 else []
    assert len(_m) == 1, ('PT2A_CLOSE no es el cierre de PB-T2a: su f4b-dispositions.md no tiene la entrada de cierre', PT2A_CLOSE)
    PT2A_ENTRY = _m[0]
    assert PT2A_CLOSE_ENTRY in (None, PT2A_ENTRY), ('PT2A_CLOSE_ENTRY', PT2A_CLOSE_ENTRY, PT2A_ENTRY)
    assert int(PT2A_ENTRY.split('E')[-1]) > int(PT2A_STOP_ENTRY.split('E')[-1]), ('el cierre de PB-T2a es anterior a su parada', PT2A_ENTRY, PT2A_STOP_ENTRY)
    assert git('cat-file', '-e', PT2A_CLOSE + ':' + PB2A_TCP).returncode == 0, ('PT2A_CLOSE no es el cierre de PB-T2a: no tiene su task-close-package.md', PT2A_CLOSE)
    ANCHOR_T2A = 'src == cierre de PB-T2a (%s, %s)' % (PT2A_CLOSE[:8], PT2A_ENTRY)
else:
    ANCHOR_T2A = 'sin el cierre de PB-T2a'
soft(PT2A_CLOSE, 'falta PT2A_CLOSE (el cierre de PB-T2a, la constante o --pt2a-close): no se comprueba "git diff <cierre de PB-T2a> <BaseSha> -- src/RackCad.UI vacio"')
# el puerto que usa la ventana (D-PBT2-04 a D-PBT2-06): AVISO mientras no exista; si existe, con la API de D-PBT2-02 y D-PBT2-03
PORT_API = {T2A_SRC[0]: ['public enum PushBackDraftApplyStatus', 'public PushBackDraftApplyStatus Status { get; }', 'public bool IsDirty { get; }',
                         'PushBackDraftApplyResult ApplyToDraft(PushBackEditableState edited);', 'public sealed class PushBackDraftSessionPort : IPushBackDraftStatePort'],
            T2A_SRC[1]: ['public sealed class PushBackDraftEditorLaunch', 'public PushBackEditableState State { get; }', 'public string RackId { get; }',
                         'public RackProject SourceProject { get; }', 'public IPushBackDraftStatePort Port { get; }']}
PORT_OK = all(in_head(p) for p in PORT_API)
if PORT_OK:
    for p, sigs in PORT_API.items():
        stripped = [l.strip() for l in lines_of(p)]
        for s in sigs:
            assert sum(1 for l in stripped if l.startswith(s)) >= 1, ('la API del puerto de PB-T2a', p, s)
    assert 'Applied' in show(T2A_SRC[0]) and 'NotEditable' in show(T2A_SRC[0])
soft(PORT_OK, 'el puerto de PB-T2a (%s y %s) aun no esta en la revision: la ventana de WORKSPACE lo usa (del cierre de PB-T2a)' % tuple(os.path.basename(p) for p in T2A_SRC))

# Las 17 estandar, del contrato emitido de PB-T2a, con P-01 en la Snapshot J
SNAP_J = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-J/snapshot-j.json'
P01_J = 'config.toml no longer matches the accepted Snapshot in force (today Snapshot J, %s)' % SNAP_J
P_STOPS = [dict(s) for s in pbt2a['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
STD_IDS = ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
assert [s['Id'] for s in P_STOPS] == STD_IDS and P_STOPS[9]['Text'] == P01_J
assert P_STOPS == [dict(s) for s in pbt1c['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
assert in_head(SNAP_J)

# ---------------------------------------------------------------- el alcance
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
MODE = 'src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs'
NEW_UI = [UT + n + '.cs' for n in ('PushBackWorkspaceSupport', 'PushBackWorkspaceModeTests', 'PushBackWorkspaceApplyCloseTests', 'PushBackFieldValidationConcordanceTests')]
NEW_CORE = [TR + n + '.cs' for n in ('PushBackWorkspaceModeSourceGuardTests', 'PushBackWorkspaceSharedReferenceGuardTests')]
ALLOWED = [W, MODE] + NEW_UI + NEW_CORE
assert len(ALLOWED) == 8 == len(set(ALLOWED))
assert in_head(W) and (not DISK or os.path.isfile(W))
assert all(not in_head(p) and (not DISK or not os.path.exists(p)) for p in ALLOWED[1:]), [p for p in ALLOWED[1:] if in_head(p)]
for pref in ('PushBackEditorMode', 'PushBackWorkspaceSupport', 'PushBackWorkspaceMode', 'PushBackWorkspaceApplyClose', 'PushBackFieldValidationConcordance',
             'PushBackWorkspaceSharedReferenceGuard'):
    assert not [p for p in tracked('src/') + tracked('tests/') if pref in p], ('el prefijo ya existe', pref)
_free = git('grep', '-n', '-w', '-E', 'PushBackEditorMode|ForDraft|ModeForTest|BomDialog|LibraryPathDialog|ApplyModeChrome|RefuseInWorkspace|'
            'ApplyEditsToEditingState|HasUnappliedDraftChanges|draftPort|appliedState|Workspace(ApplyLabel|ApplyToolTip|ApplyBlockedToolTip|'
            'RefusalMessage|LoadRefusalMessage|InvalidMessage|BlockedMessage|NothingToApplyMessage|AppliedMessage|NotEditableMessage|UnappliedQuestion)|'
            'CT_PBT2B_[01][0-9]_\\w*', REV, '--', 'src', 'tests')
assert _free.returncode == 1, ('un nombre nuevo de PB-T2b ya existe en la revision', _free.stdout.decode('utf-8')[:300])


def cover(prefix):
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
assert len(SRCF) == 60, len(SRCF)  # section 6.0 y 6.2: 60 de src/ (WC y WX incluidos)
# INV-PBT2B-01: lo que tiene que quedar vacio cae entero en el ForbiddenWriteScope
for p in (WX, WC, 'src/RackCad.Application/', 'src/RackCad.Domain/', 'src/RackCad.Plugin/', 'src/RackCad.UI/Editor/', 'src/RackCad.UI/Controls/',
          'src/RackCad.UI/Workspace/', 'src/RackCad.UI/Shell/'):
    assert p in SRCF, p
# tests/: las listas del contrato de PB-T2a mas sus dos clases nuevas de Core
CORE_T2A = [f for f in pbt2a['ForbiddenWriteScope'] if f.startswith(TR)]
UI_T2A = [f for f in pbt2a['ForbiddenWriteScope'] if f.startswith(UT)]
assert len(CORE_T2A) == 25 and len(UI_T2A) == 63 and UI_T2A[-1] == UT + 'Workspace/', (len(CORE_T2A), len(UI_T2A))
assert not [f for f in pbt2a['AllowedWriteScope'] if f.startswith(UT)]
CORE_T = CORE_T2A + T2A_TESTS
UI_T = UI_T2A
assert len(CORE_T) == 27 == len(set(CORE_T)) and len(UI_T) == 63
TESTF = CORE_T + UI_T
assert not any(a == t or (t.endswith('/') and a.startswith(t)) for a in ALLOWED for t in TESTF)
ROOT = [f for f in pbt2a['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
assert ROOT == ['docs/', '.github/', 'eng/', 'tools/', 'assets/', 'deploy/', 'RackCad.sln', 'Directory.Build.props', 'Directory.Build.targets', 'global.json',
                'NuGet.Config', 'README.md', 'AGENTS.md', 'CLAUDE.md', '.editorconfig', '.gitattributes', '.gitignore'], ROOT
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == 167 == len(set(FORBIDDEN)), len(FORBIDDEN)
for f in FORBIDDEN:
    if f in T2A_TESTS:
        soft(in_head(f), 'una clase de PB-T2a del ForbiddenWriteScope aun no esta en la revision (del cierre de PB-T2a): %s' % f)
    else:
        assert in_head(f) and (not DISK or os.path.exists(f.rstrip('/'))), ('no existe', f)

# ---------------------------------------------------------------- las anclas de W y WC (v2.3, V23-03): las lineas de las citas del cuerpo del paquete
W_LINES, WC_LINES, WX_LINES = lines_of(W), lines_of(WC), lines_of(WX)
TEXT = {'W': W_LINES, 'WC': WC_LINES}
assert git('diff', '--quiet', PB1B_CLOSE, REV, '--', WX).returncode == 0, 'WX cambio'
# V24-16 (CA-T2b section 6, punto 2): W, WC y WX son los mismos blobs que en el GREEN de PB-T1c
for _k, _p in (('W', W), ('WC', WC), ('WX', WX)):
    _b = git('rev-parse', REV + ':' + _p).stdout.decode('ascii').strip()
    assert _b.startswith(WINDOW_BLOBS[_k]) and _b == git('rev-parse', PT1C_GREEN + ':' + _p).stdout.decode('ascii').strip(), ('el blob de ' + _k, _b[:8])
WANCHORS = [
    ('W', 63, 'private PushBackEditorState state => composite.Active;'), ('W', 82, 'private PushBackEditorState moduleOwner => composite.SideA;'),
    ('W', 269, '/// <summary>A brand-new Push Back system: one dynamic-default front, rear peralte 3.5, topes active, selection (0,0),'), ('W', 287, '}'),
    ('W', 289, '/// <summary>A design opened from the library, edited as a NEW insert: keeps the suggested name, carries the source'), ('W', 304, '}'),
    ('W', 306, '/// <summary>A system opened from the DWG (RACKEDITAR): keeps its GUID + name, carries the source project, enables'),
    ('W', 314, 'UpdateCompositeEnabled();'), ('W', 320, '}'), ('W', 543, 'private void Recompute()'),
    ('W', 550, '// A control in error blocks the model rebuild: do NOT touch state/session/computation/baseline, keep the last'),
    ('W', 552, 'if (!AllFieldsValid(out var fieldError))'), ('W', 560, '}'), ('W', 589, 'catch (Exception error)'), ('W', 597, '}'),
    ('W', 638, 'outputIsBlocked = CompositeHasBlocking(computation.System);'), ('W', 650, '}'), ('W', 654, 'private bool AllFieldsValid(out string error)'),
    ('W', 682, '};'), ('W', 760, 'private PushBackEditorInputs ReadInputs()'), ('W', 782, '}'),
    ('W', 1126, 'private void Name_Changed(object sender, RoutedEventArgs e)'), ('W', 1131, '}'),
    ('W', 1418, 'internal Func<IReadOnlyList<SelectiveSafetySelection>, IReadOnlyList<SelectiveSafetySelection>> SafetyDialog;'),
    ('W', 1424, 'internal Func<PushBackRearTopeConfig, IReadOnlyList<int>, SafetyTopeGridWindow.TopeResult> RearTopeDialog;'),
    ('W', 1430, 'internal Func<PushBackDefenseSection, IReadOnlyList<SafetyPostDefense>> DefenseDialog;'),
    ('W', 1450, 'internal Func<RackFrameConfiguration, RackFrameConfiguration> HeaderConfiguratorDialog { get; set; }'),
    ('W', 2351, 'var window = new RackFrameConfiguratorWindow(copy) { Owner = this };'), ('W', 2460, '{'), ('W', 2536, 'RearTopeSectionBForTest = topeSectionB;'),
    ('W', 2625, 'internal Func<SafetyPerPostWindow, bool?> BootPerPostWindowDialog;'), ('W', 2646, 'PushBackBootSection.ModeLabels) { Owner = this };'),
    ('W', 2790, 'Owner = this'), ('W', 2941, 'Owner = this'), ('W', 2972, 'Owner = this'), ('W', 3029, 'if (!hasValidModel || lastComputation == null)'),
    ('W', 3093, 'if (lastComputation == null) return null;'), ('W', 3192, 'private void Restore_Click(object sender, RoutedEventArgs e)'),
    ('W', 3194, 'if (lastComputation?.Design == null)'), ('W', 3198, '}'), ('W', 3213, '}'),
    ('W', 3215, 'private void Update_Click(object sender, RoutedEventArgs e) => RequestDraw(null, -1, updateOnly: true);'),
    ('W', 3217, 'private void InsertBatch_Click(object sender, RoutedEventArgs e)'),
    ('W', 3234, 'if (!Views.RackViewBatchDialogPresenter.TryShow(this, RackSystemKind.PushBack, options, out var views)) return;'), ('W', 3245, '}'),
    ('W', 3247, 'private void RequestDraw(string view, int section, bool updateOnly)'), ('W', 3249, 'if (!canInsertInAutoCad)'),
    ('W', 3255, 'if (updateOnly && !isEditingExisting)'), ('W', 3259, '}'), ('W', 3270, 'if (moduleOwner.ModuleSession.HasPendingChanges)'), ('W', 3273, '}'),
    ('W', 3275, 'RequestRecompute(); // synchronous validate + build'), ('W', 3276, 'if (!currentInputsAreValid || session.System == null)'),
    ('W', 3280, '}'), ('W', 3284, 'if (OutputBlockedAfterRecompute("Corrige los datos: no se puede dibujar un modelo bloqueado."))'), ('W', 3287, '}'),
    ('W', 3289, 'session.Identity.SetName(NameBox.Text?.Trim());'), ('W', 3291, 'session.SetModel(lastComputation.Design, lastComputation.System);'),
    ('W', 3294, 'session.RequestUpdate(ctx => new PushBackInsertionRequest('), ('W', 3299, 'session.RequestInsert(view, section, ctx => new PushBackInsertionRequest('),
    ('W', 3303, 'Close();'), ('W', 3304, '}'), ('W', 3306, 'private void Close_Click(object sender, RoutedEventArgs e) => Close();'),
    ('W', 3308, '/// <summary>'), ('W', 3319, '/// </summary>'), ('W', 3320, 'protected override void OnClosing(CancelEventArgs e)'),
    ('W', 3322, 'if (!InsertRequested && !EditorClosePolicy.MayClose(PendingWork()))'), ('W', 3329, '}'), ('W', 3331, '/// <summary>'),
    ('W', 3336, '/// <para>Lo demas que el usuario haya tocado ' + chr(0x2014) + 'matriz, seguridad, topes' + chr(0x2014) + ' no es un ambito declarado: se aplica'),
    ('W', 3341, '/// </summary>'), ('W', 3342, 'internal EditorPendingWork PendingWork() => EditorPendingWork.When('),
    ('W', 3344, '"Hay cambios de m' + chr(0xF3) + 'dulo sin confirmar que se perder' + chr(0xE1) + 'n al cerrar. ' + chr(0xBF) + 'Deseas continuar?");'),
    ('W', 3348, 'private void Bom_Click(object sender, RoutedEventArgs e)'),
    ('W', 3358, 'if (OutputBlockedAfterRecompute("Corrige los datos: no se puede listar un modelo bloqueado."))'),
    ('W', 3363, 'new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();'), ('W', 3364, '}'),
    ('W', 3366, 'private void SaveLibrary_Click(object sender, RoutedEventArgs e)'),
    ('W', 3378, 'if (OutputBlockedAfterRecompute("Corrige los datos: no se puede guardar un modelo bloqueado."))'),
    ('W', 3383, 'var path = UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema");'), ('W', 3384, 'if (path == null) return;'),
    ('W', 3388, 'session.Identity.EnsureId();'), ('W', 3389, 'session.Identity.SetName(NameBox.Text?.Trim());'),
    ('W', 3391, "// Save ONLY the active Push Back payload; WithSourceMetadataFrom preserves the opened project's unknown JSON"),
    ('W', 3392, '// fields + non-downgraded schema version (I-11). Saving never flags an insert.'), ('W', 3401, '}'),
    ('W', 3449, 'private void UpdateButtons()'), ('W', 3492, 'RestoreButton.IsEnabled = lastComputation != null;'),
    ('W', 3495, ': "A' + chr(0xFA) + 'n no hay un sistema v' + chr(0xE1) + 'lido que restaurar.";'), ('W', 3496, '}'),
    ('WC', 790, 'private bool CompositeHasBlocking(PushBackSystem system)'), ('WC', 794, '}'),
    # v2.4: las que citan las reglas del section 5.0 (el momento de cada lectura y los gestos), D-PBT2-07, CT-PBT2B-02 y CT-PBT2B-07
    ('W', 607, 'lastComputation = computation;'), ('W', 1135, 'private void Input_Changed(object sender, RoutedEventArgs e) => RequestRecompute();'),
    ('W', 3350, 'RequestRecompute();'), ('W', 3368, 'RequestRecompute();'), ('W', 3569, 'private static double Keep(NumericField field, double current)'),
    ('W', 3570, '=> !field.Value.HasValue || SameShown(field.Value.Value, current) ? current : field.Value.Value;'),
    ('WC', 154, 'var selection = SideSelectorBox.SelectedIndex == 2'),
    ('WC', 156, ': SideSelectorBox.SelectedIndex == 1 ? PushBackSideSelection.B : PushBackSideSelection.A;'),
]
for f, line, text in WANCHORS:
    assert TEXT[f][line - 1].strip() == text, ('ancla de %s' % f, line, TEXT[f][line - 1].strip())
assert len(WANCHORS) == 100
# el recalculo de BOM y de la biblioteca es su primera sentencia (CT-PBT2B-07; V24-02)
assert W_LINES[3347].strip() == 'private void Bom_Click(object sender, RoutedEventArgs e)' and W_LINES[3348].strip() == '{'
assert W_LINES[3365].strip() == 'private void SaveLibrary_Click(object sender, RoutedEventArgs e)' and W_LINES[3366].strip() == '{'
assert W_LINES[598].strip() == 'if (computation.IsValid)', 'W:607 dentro del ramal valido de Recompute (W:599)'
assert [l.strip() for l in WX_LINES[102:104]] == ['<controls:NumericField x:Name="ToleranceBox" LostFocus="Input_Changed"',
                                                  'Minimum="0" MinimumInclusive="False" />'], 'WX:103, la tolerancia recalcula en LostFocus'
# las 72 citas del cuerpo de la v2.4 (fuera de los registros de cambios): las 66 de la v2.3 y las 6 nuevas; cada una con sus dos extremos en WANCHORS
CITES = [('W', 63, 63), ('W', 82, 82), ('W', 269, 287), ('W', 289, 304), ('W', 306, 320), ('W', 314, 314), ('W', 543, 650), ('W', 550, 560), ('W', 552, 552),
         ('W', 589, 597), ('W', 638, 638), ('W', 654, 682), ('W', 760, 782), ('W', 1126, 1131), ('W', 1418, 1418), ('W', 1424, 1424), ('W', 1430, 1430),
         ('W', 1450, 1450), ('W', 2351, 2351), ('W', 2460, 2536), ('W', 2625, 2625), ('W', 2646, 2646), ('W', 2790, 2790), ('W', 2941, 2941),
         ('W', 2972, 2972), ('W', 3029, 3029), ('W', 3093, 3093), ('W', 3192, 3213), ('W', 3194, 3198), ('W', 3215, 3215), ('W', 3217, 3245),
         ('W', 3234, 3234), ('W', 3247, 3304), ('W', 3249, 3249), ('W', 3249, 3259), ('W', 3255, 3255), ('W', 3270, 3273), ('W', 3270, 3287),
         ('W', 3275, 3275), ('W', 3276, 3280), ('W', 3284, 3287), ('W', 3289, 3289), ('W', 3289, 3291), ('W', 3294, 3294), ('W', 3299, 3299),
         ('W', 3303, 3303), ('W', 3306, 3306), ('W', 3308, 3319), ('W', 3320, 3329), ('W', 3322, 3322), ('W', 3331, 3344), ('W', 3336, 3341),
         ('W', 3342, 3344), ('W', 3348, 3364), ('W', 3358, 3358), ('W', 3363, 3363), ('W', 3366, 3401), ('W', 3378, 3378), ('W', 3383, 3383),
         ('W', 3384, 3384), ('W', 3388, 3388), ('W', 3389, 3389), ('W', 3391, 3392), ('W', 3449, 3496), ('W', 3492, 3495), ('WC', 790, 794),
         ('W', 607, 607), ('W', 1135, 1135), ('W', 3350, 3350), ('W', 3368, 3368), ('W', 3569, 3570), ('WC', 154, 156)]
assert len(CITES) == 72 and {(f, x) for f, a, b in CITES for x in (a, b)} == {(f, l) for f, l, _ in WANCHORS}


# INV-PBT2B-03 (v2.3, V23-04): los rangos de BaseSha de los miembros que cambian, con el metodo de INV-PBT1C-03 (make_pbt1c_contract.py)
def code_of(raw):
    s = re.sub(r'"(?:\\.|[^"\\])*"', '""', raw)
    s = re.sub(r"'(?:\\.|[^'\\])'", "''", s)
    return s.split('//')[0]


def member_end(lines, d):
    depth, opened = 0, False
    for i in range(d, len(lines) + 1):
        code = code_of(lines[i - 1])
        depth += code.count('{') - code.count('}')
        opened = opened or '{' in code
        if (opened and depth == 0) or (not opened and code.strip().endswith(';') and depth == 0):
            return i
    raise AssertionError(('miembro sin final', d))


MEMBER_DECL = re.compile(r'^ {8}(?:\[[^\]]*\]\s*)*(?:(?:public|private|internal|protected|static|readonly|override|virtual|sealed|async|partial|new|const|event)\s+)+')


def is_comment(l):
    return l.strip().startswith(('//', '['))


def check_range(lines, a, b, names):
    decls = [k for k in range(a, b + 1) if MEMBER_DECL.match(lines[k - 1])]
    named = [k for k in decls if any(re.search(r'\b%s\b\s*(\(|=|;|=>|$)' % n, lines[k - 1]) for n in names)]
    assert decls == named and len(named) == len(names), ('rango con otros miembros o sin los suyos', a, b, decls, named)
    spans = [(k, member_end(lines, k)) for k in named]
    covered = {x for s, e in spans for x in range(s, e + 1)}
    assert lines[a - 1].strip() != '' and lines[b - 1].strip() != '', ('borde en blanco', a, b)
    assert not is_comment(lines[a - 2]), ('el rango deja fuera documentacion por arriba', a)
    assert a == spans[0][0] or is_comment(lines[a - 1]), ('el rango no empieza en la documentacion ni en la declaracion', a)
    assert b == max(e for _, e in spans), ('el rango no acaba en el final del miembro', b)
    for k in range(a, b + 1):
        if k not in covered:
            assert lines[k - 1].strip() == '' or is_comment(lines[k - 1]), ('linea ajena dentro del rango', k)
    return spans


RANGES = [(269, 287, 'LoadNew'), (289, 304, 'LoadDesignForNew'), (306, 320, 'LoadExisting'), (3215, 3215, 'Update_Click'), (3217, 3245, 'InsertBatch_Click'),
          (3247, 3304, 'RequestDraw'), (3331, 3344, 'PendingWork'), (3348, 3364, 'Bom_Click'), (3366, 3401, 'SaveLibrary_Click'), (3449, 3496, 'UpdateButtons')]
RANGE_REPORT, RANGE_SELFTESTS = [], 0
for a, b, n in RANGES:
    spans = check_range(W_LINES, a, b, [n])
    RANGE_REPORT.append('%s W:%d-%d (miembro %d-%d)' % (n, a, b, spans[0][0], spans[0][1]))
    for a2, b2 in ((a + 1, b), (a - 1, b), (a, b - 1), (a, b + 1)):
        if a2 > b2:
            continue
        try:
            check_range(W_LINES, a2, b2, [n])
        except AssertionError:
            RANGE_SELFTESTS += 1
            continue
        raise AssertionError(('autoprueba de rango que no falla', n, a2, b2))
_rs = sorted((a, b) for a, b, _ in RANGES)
assert all(_rs[k][1] < _rs[k + 1][0] for k in range(len(_rs) - 1)), 'rangos solapados'

# las demas anclas de W en las que se apoya el diseno de PB-T2b (D-PBT2-04 a D-PBT2-12, section 4)
_ws = [l.strip() for l in W_LINES]
WT_TEXT, WC_TEXT = '\n'.join(W_LINES), '\n'.join(WC_LINES)
EDITABLE_DECL = 'private readonly PushBackEditableState editable = new PushBackEditableState();'
assert _ws.count(EDITABLE_DECL) == 1 and 'private PushBackCompositeEditorState composite => editable.Editor;' in _ws and 'internal PushBackEditableState EditableStateForTest => editable;' in _ws
CTORS = ['public RackPushBackSystemWindow()', 'public RackPushBackSystemWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)']  # R6 e INV-PBT2B-04
assert all(_ws.count(c) == 1 for c in CTORS) and sum(1 for l in _ws if re.match(r'^(?:public|private|internal|protected)\s+RackPushBackSystemWindow\s*\(', l)) == 2
for decl in ('private bool isEditingExisting;', 'private RackProject sourceProject;', 'private bool outputIsBlocked;', 'private void SyncName() => editable.Name = session.Identity.Name;',
             'private void LoadFromModel(PushBackEditorInputs inputs, string rackName)', 'private void SetStatus(string message, bool isError) => UiSupport.SetStatus(StatusText, message, isError);'):
    assert _ws.count(decl) == 1, ('declaracion de W que usa el constructor de WORKSPACE', decl)
assert sum(1 for l in _ws if l.startswith('private bool hasValidModel;')) == 1 and sum(1 for l in _ws if l.startswith('private bool currentInputsAreValid;')) == 1
assert 'private void UpdateCompositeEnabled()' in [l.strip() for l in WC_LINES], 'UpdateCompositeEnabled en WC'
SEAMS = [l for l in W_LINES + WC_LINES if re.match(r'^\s+internal ', l)]
assert len(SEAMS) == 41, len(SEAMS)  # INV-PBT2B-06: las 41 de hoy [T1c]
S7 = 'new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();'
S9 = 'UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")'
assert WT_TEXT.count(S7) == 1 and WT_TEXT.count(S9) == 1 and WT_TEXT.count('.RequestUpdate(') == 1 and '.RequestUpdate(' not in WC_TEXT
_save = W_LINES[3366 - 1:3401]
_order = [next(i for i, l in enumerate(_save) if tok in l) for tok in ('PromptSaveToLibrary(', 'session.Identity.EnsureId();', 'session.Identity.SetName(NameBox.Text?.Trim());',
                                                                         'RackProject.ForPushBack(', '.Save(project, path);')]
assert _order == sorted(_order), ('CT-22: el orden de las cinco sentencias', _order)
_batch = '\n'.join(W_LINES[3217 - 1:3245])
assert _batch.index('CommitModuleEdits') < _batch.index('TryShow(') < _batch.index('RequestInsertViews') < _batch.rindex('Close();') and 'canInsertInAutoCad' not in _batch, 'CT-23'
WITNESS_TODAY = ['sin confirmar que se perder', 'Corrige los datos: no se puede insertar un modelo inv', 'Corrige los datos: no se puede dibujar un modelo bloqueado.']
assert all(all(ord(ch) < 0x80 for ch in t) and WT_TEXT.count(t) >= 1 for t in WITNESS_TODAY), 'los testigos de hoy de INV-PBT2B-04'
# D-PBT2-06 (1): el paso que se extrae de RequestDraw, en su orden (W:3270-3287)
_ext = [3270, 3272, 3275, 3276, 3278, 3284]
assert [_ws[k - 1] for k in _ext] == ['if (moduleOwner.ModuleSession.HasPendingChanges)', 'moduleOwner.CommitModuleEdits();', 'RequestRecompute(); // synchronous validate + build',
                                       'if (!currentInputsAreValid || session.System == null)', 'SetStatus("Corrige los datos: no se puede insertar un modelo inv' + chr(0xE1) + 'lido.", true);',
                                       'if (OutputBlockedAfterRecompute("Corrige los datos: no se puede dibujar un modelo bloqueado."))'], 'D-PBT2-06 (1)'
# WX: los once botones de D-PBT2-09 y CT-PBT2B-01
for btn in ('UpdateButton', 'InsertButton', 'InsertLateralButton', 'InsertFrontalEntradaButton', 'InsertFrontalPosteriorButton', 'InsertPlantaButton',
            'InsertBatchButton', 'BomButton', 'SaveLibraryButton', 'RestoreButton', 'CloseButton'):
    assert sum(1 for l in WX_LINES if ('x:Name="%s"' % btn) in l) == 1, ('WX', btn)
assert 'TextChanged="Name_Changed"' in WX_LINES[47 - 1] and 'Minimum="0"' in '\n'.join(WX_LINES[67:69]) and 'MinimumInclusive="False"' in '\n'.join(WX_LINES[67:69])


# R3 (CT-PBT2B-08) en la base: en la clausura src/RackCad.UI/Systems/PushBack/*.cs, los miembros con .RequestUpdate(, .RequestInsert( o
# .RequestInsertViews( son exactamente RequestDraw e InsertBatch_Click, de W
def census_strip(text):
    text = re.sub(r'@"(?:[^"]|"")*"', '""', text)
    text = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', text)
    text = re.sub(r"'(?:\\.|[^'\\\n])'", "''", text)
    text = re.sub(r'/\*.*?\*/', ' ', text, flags=re.S)
    return re.sub(r'//[^\n]*', '', text)


_MODS = r'(?:public|private|internal|protected|static|readonly|sealed|override|virtual|abstract|partial|async|const|event|unsafe|extern|volatile)'
_CDECL = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:' + _MODS + r'\s+)+(?:class\s+(\w+)|[^=;({]*?\b(\w+)\s*(?:<[^>]*>)?\s*(?:\(|=>|=|;|\{|$))')


def members_with_bodies(text):
    """(nombre, texto) de cada declaracion a profundidad 2, con todo lo que cuelga de ella hasta la siguiente."""
    out, depth, cur = [], 0, None
    for line in census_strip(text).split('\n'):
        if depth == 2:
            m = _CDECL.match(line)
            if m:
                cur = [m.group(1) or m.group(2), []]
                out.append(cur)
        if cur is not None:
            cur[1].append(line)
        depth += line.count('{') - line.count('}')
    return [(n, '\n'.join(b)) for n, b in out]


CLOSURE = [p for p in tracked('src/RackCad.UI/Systems/PushBack/') if p.endswith('.cs')]
_req = sorted((os.path.basename(p), n) for p in CLOSURE for n, body in members_with_bodies(show(p))
              if re.search(r'\.\s*(?:RequestUpdate|RequestInsert|RequestInsertViews)\s*\(', body))
assert _req == [('RackPushBackSystemWindow.xaml.cs', 'InsertBatch_Click'), ('RackPushBackSystemWindow.xaml.cs', 'RequestDraw')], ('R3 en la base', _req)
# los nombres nuevos de PBT2B-C13 no existen como miembros de W ni de WC (CA-T2a M-3)
CENSUS = [n for n, _ in members_with_bodies(WT_TEXT)] + [n for n, _ in members_with_bodies(WC_TEXT)]
NEW_T2B = ['mode', 'draftPort', 'appliedState', 'BomDialog', 'LibraryPathDialog', 'ModeForTest', 'ForDraft', 'ApplyModeChrome', 'RefuseInWorkspace',
           'ApplyEditsToEditingState', 'ApplyToDraft', 'HasUnappliedDraftChanges', 'WorkspaceApplyLabel', 'WorkspaceApplyToolTip', 'WorkspaceApplyBlockedToolTip',
           'WorkspaceRefusalMessage', 'WorkspaceLoadRefusalMessage', 'WorkspaceInvalidMessage', 'WorkspaceBlockedMessage', 'WorkspaceNothingToApplyMessage',
           'WorkspaceAppliedMessage', 'WorkspaceNotEditableMessage', 'WorkspaceUnappliedQuestion', 'PushBackEditorMode', 'Classic', 'Workspace']
assert len(NEW_T2B) == 26 and not set(CENSUS) & set(NEW_T2B), ('un nombre de PBT2B-C13 ya es un miembro de W o WC', set(CENSUS) & set(NEW_T2B))
for n in ('LoadFromModel', 'ReadInputs', 'Recompute', 'OnClosing'):  # INV-PBT2B-04: no cambian; cada uno, una declaracion
    assert CENSUS.count(n) == 1, ('miembro de INV-PBT2B-04', n, CENSUS.count(n))
for n in [r[2] for r in RANGES]:
    assert CENSUS.count(n) == 1, ('miembro de INV-PBT2B-03', n, CENSUS.count(n))

# las guardas de PB-T1c siguen sin nombrar los miembros nuevos (CT-PBT1C-03 y 04 quedan en verde sin tocarlas)


def array_items(text, head):
    m = re.search(re.escape(head) + r'\s*\{(.*?)\};', text, re.S)
    assert m, ('lista de la guarda', head)
    return re.findall(r'"([^"]*)"', m.group(1))


_sg, _d2 = show(TR + 'PushBackWindowOnStateSourceGuardTests.cs'), show(TR + 'PushBackWindowSharedReferenceGuardTests.cs')
for names in (array_items(_sg, 'private static readonly string[] Forwarded ='), array_items(_d2, 'private static readonly string[] Tokens ='),
              array_items(_d2, 'new HashSet<string>(StringComparer.Ordinal)')):
    assert not set(names) & set(NEW_T2B), ('una guarda de PB-T1c nombra un miembro de INV-PBT2B-03', set(names) & set(NEW_T2B))

# ---------------------------------------------------------------- las anclas de las pruebas que copian o leen los CT de PB-T2b (registros K y Q)
T_ANCHORS = [
    (UT + 'PushBackClassicCharacterizationCloseTests.cs', 47, 'private sealed class PromptProbe'),
    (UT + 'PushBackClassicCharacterizationCloseTests.cs', 90, 'private static void StageModuleEdit(RackPushBackSystemWindow window)'),
    (UT + 'PushBackClassicCharacterizationCloseTests.cs', 124, 'private static void CloseQuietly(RackPushBackSystemWindow window, PromptProbe probe)'),
    (UT + 'PushBackClassicCharacterizationCloseTests.cs', 126, 'probe.Answer = true;'),
    (UT + 'PushBackDeclaredCorrection2MixedBoxTests.cs', 96, 'log.Add(linea + " descartes=" + descartes);'),
    (UT + 'PushBackFullEditorCharacterizationSubdialogTests.cs', 38, 'public void CT_PB0_12_TheHeaderConfiguratorSeamStagesWhatItReturnsAndNullStagesNothing()'),
    (UT + 'PushBackFullEditorCharacterizationSubdialogTests.cs', 104, 'public void CT_PB0_13_AcceptedSafetyAppliesTheReturnedListAndTheDefenseFamilyIsRecreatedByItsSections()'),
    (UT + 'PushBackFullEditorCharacterizationSubdialogTests.cs', 161, 'public void CT_PB0_14_ThePerPostBootGridResultReachesTheOutputThroughTheBootSection()'),
    (UT + 'PushBackFullEditorCharacterizationSubdialogTests.cs', 200, 'public void CT_PB0_15_ThePerPostDefenseRecordsReachTheOutputThroughTheDefenseSection()'),
    (UT + 'PushBackFullEditorCharacterizationSubdialogTests.cs', 237, 'public void CT_PB0_16_TheRearTopeDialogResultReachesTheOutputInASingleSidedRack()'),
    (UT + 'PushBackFullEditorCharacterizationSupport.cs', 347, 'internal static void SelectModule(RackPushBackSystemWindow window, string moduleId)'),
    (UT + 'PushBackLibrarySaveGateTests.cs', 95, '=> Record.Exception(() => Click(Button(window, "SaveLibraryButton"))) != null;'),
    (UT + 'RichEditorCloseContractTests.cs', 77, '// valor legitimo, y no se inventa un dirty global que el producto no tiene.'),
    (UT + 'PushBackClassicCharacterizationSourceGuardTests.cs', 169, 'private static string ClassicWindowSources()'),
    (UT + 'PushBackClassicCharacterizationSourceGuardTests.cs', 236, 'public void TheClassicWindowHasNoKeyboardHandlersNoTabOrderNoForcedFocusAndNoDialogResult()'),
    (UT + 'PushBackClassicCharacterizationSourceGuardTests.cs', 281, 'public void TheBomWindowIsOwnedByTheWindow()'),
    (UT + 'PushBackClassicCharacterizationSourceGuardTests.cs', 294, 'public void TheSaveFileDialogIsOwnedByTheWindowThroughPromptSaveToLibrary()'),
    (UT + 'PushBackClassicCharacterizationSourceGuardTests.cs', 324, 'public void SaveToLibraryPromptsFirstThenMarksTheIdentityNamesItAndSavesTheLibraryProject()'),
    (UT + 'PushBackClassicCharacterizationSourceGuardTests.cs', 344, 'public void TheHeaderConfiguratorWritesItsLayoutOnlyWhenItIsClosed()'),
    (UT + 'PushBackClassicCharacterizationSourceGuardTests.cs', 357, 'public void InsertBatchShowsItsDialogThenRequestsTheBatchAndClosesWithoutLookingAtCanInsertInAutoCad()'),
    (UT + 'PushBackWindowOnStateTests.cs', 449, 'private static void Recalcular(RackPushBackSystemWindow w)'),
    (UT + 'PushBackWindowOnStateTests.cs', 450, '=> Field(w, "AnnotationScaleBox").RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, Field(w, "AnnotationScaleBox")));'),
    (UT + 'EditorWindowTestSupport.cs', 22, 'public static void ClickNamed(Window window, string name)'),
    (UT + 'EditorWindowTestSupport.cs', 111, '=> button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));'),
    (TR + 'Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs', 30, 'private static IReadOnlyList<(string Path, string Code)> Product() => Src.AllProduct();'),
    (TR + 'Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs', 725, 'private static readonly string[] OwnRequestUpdateWindows ='),
    ('src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotFieldAdapter.cs', 35, 'public DraftFieldValidation Validate(string fieldKey, string text)'),
    ('src/RackCad.UI/Controls/NumericFieldValidation.cs', 76, 'public static NumericFieldValidationResult Validate('),
    # v2.4 (V24-01, V24-04, V24-06, V24-15): el Vigia, SetNumberR, S0.Ventana y S0.Cerrar, CloseQuietly solo con la ventana mostrada, y la copia nueva
    # que guarda el borrador (C-1 de F4B-E33)
    (UT + 'PushBackWindowOnStateTests.cs', 390, '/// <summary>The two shared references of the state of ONE window, each measured from the moment it is adopted.</summary>'),
    (UT + 'PushBackWindowOnStateTests.cs', 391, 'private sealed class Vigia'),
    (UT + 'PushBackWindowOnStateTests.cs', 408, 'public bool Intacta() => items.All(item => Fingerprint(item.Object) == item.Before);'),
    (UT + 'PushBackWindowOnStateTests.cs', 409, '}'),
    (UT + 'PushBackFullEditorCharacterizationSupport.cs', 63, 'internal static RackPushBackSystemWindow Ventana()'),
    (UT + 'PushBackFullEditorCharacterizationSupport.cs', 71, 'internal static void Cerrar(Window window)'),
    (UT + 'PushBackFullEditorCharacterizationSupport.cs', 336, 'internal static void SetNumberR(Window window, string name, double value)'),
    (UT + 'PushBackFullEditorCharacterizationSupport.cs', 340, 'field.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, field));'),
    (UT + 'PushBackClassicCharacterizationCloseTests.cs', 127, 'if (window.IsVisible)'),
    ('src/RackCad.Application/Workspace/Drafts/DraftSession.cs', 284, 'public TState InitialDraftState => _copy(_initial);'),
    ('src/RackCad.Application/Workspace/Drafts/DraftSession.cs', 287, 'public TState CurrentDraftState => _current;'),
    ('src/RackCad.Application/Workspace/Drafts/DraftSession.cs', 348, '_current = _copy(state);'),
]
_tcache = {}
for path, line, text in T_ANCHORS:
    if path not in _tcache:
        _tcache[path] = lines_of(path)
    assert _tcache[path][line - 1].strip() == text, ('ancla de prueba', path, line, _tcache[path][line - 1].strip())
# los datos de las recetas de la v2.4: la tolerancia de los 17 fixtures (V24-11), el nombre de pb-01-base (CT-PBT2B-07, V24-02) y los gestos de
# CT-PBT1C-02 que copia CT-PBT2C-03 (49 con Compara y los cuatro de D2, V24-14)
FX = TR + 'Fixtures/I64/'
FIXTURES = sorted(p for p in tracked(FX) if p.endswith('.json'))
assert len(FIXTURES) == 17, len(FIXTURES)
FX_TOL, FX_NAME = {}, {}
for p in FIXTURES:
    _env = json.loads(show(p).lstrip(chr(0xFEFF)))
    FX_TOL[os.path.basename(p)[:-5]] = json.loads(_env['Design'])['PushBack']['Structure']['PalletTolerance']
    FX_NAME[os.path.basename(p)[:-5]] = _env['Name']
assert set(FX_TOL.values()) == {4}, ('V24-11: la tolerancia de los fixtures', FX_TOL)
assert FX_NAME['pb-01-base'] == 'PB-01' and FX_NAME['pb-20-ab'] == 'PB-20', 'V24-02: el nombre de pb-01-base'
_ws = show(UT + 'PushBackWindowOnStateTests.cs')
_gs = _ws[_ws.index('private static IEnumerable<Gesto> Gestos()'):_ws.index('private static string DisenoDeUnaCopia(')]
assert _gs.count('yield return G(') == 49 and _gs.count('yield return SinComparar(') == 4, 'V24-14: los gestos de CT-PBT1C-02'
assert [n for n in ('"campo-en-error"', '"calculo-invalido"', '"caja-escrita-sin-salir"', '"cerrar-sin-pedir"') if n in _gs] == [
    '"campo-en-error"', '"calculo-invalido"', '"caja-escrita-sin-salir"', '"cerrar-sin-pedir"'], 'V24-14: los cuatro de D2'
# G-L1c y sus quince autopruebas (CT-PBT2B-09 las copia) y las huellas (CT-PBT2B-05), iguales que en el cierre de PB-T1b
GL1C, CPY = TR + 'PushBackSharedReferenceGuardTests.cs', TR + 'PushBackEditableStateCopyTests.cs'
assert git('diff', '--quiet', PB1B_CLOSE, REV, '--', GL1C, CPY).returncode == 0, 'G-L1c o las huellas cambiaron'
_g = lines_of(GL1C)
SELFTESTS = ['a', 'b', 'c', 'e', 'f1', 'f2', 'f3', 'g1', 'g2', 'h1', 'h2', 'i', 'j', 'k1', 'k2']
SELFTESTS15 = '(a), (b), (c), (e), (f1), (f2), (f3), (g1), (g2), (h1), (h2), (i), (j), (k1) y (k2)'
_st = [re.match(r'^\s*\("(\w+)", ".*"\),$', _g[k - 1]) for k in list(range(223, 236)) + [238, 239]]
assert all(_st) and [m.group(1) for m in _st] == SELFTESTS, 'las quince autopruebas de G-L1c'
_c = lines_of(CPY)
assert _c[277].strip() == 'private static HashSet<object> AllObjects(object root, Func<FieldInfo, bool> skip = null)' and _c[1079].strip() == 'private static string Fingerprint(object root)'

# ---------------------------------------------------------------- CA-T2a M-6 (a): las clases de los filtros de UI de PB-T2b que crean un RackFrameConfiguratorWindow real
UI_FILTERS = ['PushBackWindowOnStateTests', 'PushBackEditableStateParityTests', 'PushBackStateNet', 'PushBackClassicCharacterization', 'PushBackFullEditorCharacterization',
              'PushBackDeclaredCorrection', 'PushBackCompositeWindowTests', 'PushBackCompositeLifecycleWiringTests', 'PushBackCompositeLoadExistingModulesTests',
              'PushBackBothModeCellBeamOverrideTests', 'PushBackModuleEditorWindowTests', 'PushBackAdvancedRackParametersWindowTests', 'PushBackRichPreviewTests',
              'PushBackLibrarySaveGateTests', 'DialogWindowCharacterizationTests', 'BlankFront', 'WindowCensusGuardTests', 'RichEditor', 'Workspace', 'Cantilever']
_cls = re.compile(r'^\s*(?:public|internal)\s+(?:sealed\s+|static\s+|partial\s+|abstract\s+)*class\s+(\w+)')
CONFIGURATOR_FILES = []
for p in [p for p in tracked(UT) if p.endswith('.cs') and '/bin/' not in p and '/obj/' not in p]:
    t = show(p)
    classes = [m.group(1) for m in (_cls.match(l) for l in t.split('\n')) if m]
    fq = ['RackCad.UI.Tests.' + c for c in classes]
    if any(('RackCad.UI.Tests.' + f) in q for f in UI_FILTERS for q in fq) and 'new RackFrameConfiguratorWindow(' in census_strip(t):  # en codigo, no en un literal de una guarda de texto
        CONFIGURATOR_FILES.append(os.path.basename(p))
CONFIGURATOR_FILES.sort()
assert CONFIGURATOR_FILES == ['RichEditorCharacterizationTests.cs', 'RichEditorCloseContractTests.cs', 'RichEditorContractTests.cs'], ('V23-01', CONFIGURATOR_FILES)

# ---------------------------------------------------------------- las decisiones del Owner (F4B-E23 y F4B-E28), custodiadas
CO = PILOT + '/F4B-DRAFTS/coordinator/'
OWNER_PBT2 = CO + 'owner-pbt2-decisions.md'
_disp = show(DISP).replace('\r\n', '\n')
_disp_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
assert _disp_ids == list(range(1, len(_disp_ids) + 1)), ('f4b-dispositions.md no esta numerado seguido', _disp_ids[-3:])
DISP_LAST = 'F4B-E%02d' % _disp_ids[-1]
assert _disp_ids[-1] >= 32 and '\n## F4B-E23. ' in _disp and ('\n## %s. Decisiones del Owner antes de los contratos de PB-T2' % OWNER_PBT2_ENTRY) in _disp
_own = show(OWNER_PBT2)
for q, ans in (('Q-PBT2-1', 'Aplica sin cerrar; Cerrar avisa (Recomendado)'), ('Q-PBT2-2', 'Lo rechaza, como Actualizar (Recomendado)'),
               ('Q-PBT2-3', 'En PB-T4, con prueba ya (Recomendado)'), ('Q-PBT2-4', 'S' + chr(0xED) + ', con esa condici' + chr(0xF3) + 'n (Recomendado)')):
    _blk = _own[_own.index('## ' + q + '.'):]
    _blk = _blk[:_blk.index('\n## ', 3)] if '\n## ' in _blk[3:] else _blk
    assert ('**Respuesta del Owner:** ' + chr(0xAB) + ans + chr(0xBB)) in _blk, ('la respuesta del Owner', q)
_e23 = show(CO + 'owner-f4b-dpb07-extensiondata-decisions.md')
assert 'BOM y biblioteca (Recomendado)' in _e23, 'F4B-E23'

# ---------------------------------------------------------------- D1, las celdas
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
        m = re.match(r'^\s+-\s+([A-Za-z0-9_-]+)\s*$', l)
        if m:
            _needs.append(m.group(1))
        else:
            _in_needs = False
assert 'tests' in _needs and {'  tests:', '  ui-tests:', '  build-ui:'} <= set(CI_LINES), ('needs de build-plugin', _needs)
cells = json.loads(json.dumps(pbt2a['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna'] and cells == pbt1c['EligibleCells']
MC = lines_of('docs/automation/agent-execution/model-catalog.md')
assert '+ 90 d' in MC[13] and all(MC[k].startswith('- Fecha de verificaci') and MC[k].strip().endswith('2026-09-30') for k in (50, 96))
CELLS_EXPIRY = (datetime.date(2026, 9, 30) + datetime.timedelta(days=90)).isoformat()
assert CELLS_EXPIRY == '2026-12-29' and ISSUED[:10] < CELLS_EXPIRY, (ISSUED, CELLS_EXPIRY)


# ---------------------------------------------------------------- el paquete
def ascii_fold(s):
    for x, y in ((chr(0xAB), '"'), (chr(0xBB), '"'), (chr(0x2014), '-'), (chr(0x2013), '-'), (chr(0x2192), '->'), (chr(0x2264), '<='), (chr(0x2265), '>='),
                 (chr(0x2026), '...'), (chr(0xA7), 'section '), (chr(0x222A), 'o'), (chr(0xBF), ''), (chr(0xBA), 'o')):
        s = s.replace(x, y)
    s = ''.join(c for c in unicodedata.normalize('NFKD', nspace(s)) if not unicodedata.combining(c))
    assert all(ord(c) < 0x80 for c in s), [c for c in s if ord(c) >= 0x80]
    return s


def md_fold(lines):
    return ascii_fold(' '.join(re.sub(r'^- ', '', l.strip()) for l in lines if l.strip()).replace('**', '').replace('`', ''))


def title_version(lines):
    t = [l for l in lines[:6] if l.startswith('# I-64 F4B')]
    m = re.search(r', (v\d+(?:\.\d+)?)\s*$', t[0]) if len(t) == 1 else None
    return m.group(1) if m else None


PKG = 'docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md'
MEAS = 'docs/initiatives/I-64-f4b-pbt2-measurements.md'
PKG_HEAD_VERSION = title_version(lines_of(PKG)) if in_head(PKG) else None
if PKG_FILE:
    PKG_LINES = open(PKG_FILE, 'rb').read().decode('utf-8').replace('\r\n', '\n').split('\n')
    PKG_SRC = '--package-file ' + os.path.basename(PKG_FILE)
    assert title_version(PKG_LINES) == PKG_VERSION, ('--package-file no es la ' + PKG_VERSION, title_version(PKG_LINES))
else:
    PKG_LINES = lines_of(PKG) if in_head(PKG) else []
    PKG_SRC = PKG + ' en ' + REV_SHA[:8]
PKG_READ_VERSION = title_version(PKG_LINES) if PKG_LINES else None
PKG_OK = PKG_READ_VERSION == PKG_VERSION
soft(PKG_OK, 'el paquete leido (%s) es %s: no se comprueban sus textos de la %s (en seco, --package-file con la %s)' % (PKG_SRC, PKG_READ_VERSION, PKG_VERSION, PKG_VERSION))


def section(head_prefix):
    i = [k for k, l in enumerate(PKG_LINES) if l.startswith(head_prefix)]
    assert len(i) == 1, ('seccion', head_prefix, len(i))
    out = []
    for l in PKG_LINES[i[0] + 1:]:
        if l.startswith('## ') or l.startswith('### '):
            break
        out.append(l)
    return out


def once(folded, line, what):
    assert folded.count(line) == 1, ('el paquete difiere del texto que trae el contrato', what, line[:160])


def lc(title):
    return title[0].lower() + title[1:] if len(title) > 1 and title[1].islower() else title


def inv_bullet(first, subs):
    m = re.match(r'^\s*- \*\*((?:INV-PBT2B-)?\d\d[a-z]?)\.\s*(.*?)\*\*\s*(.*)$', first)
    assert m, ('vineta de invariante', first[:80])
    idd, t, rest = m.groups()
    idd = idd.split('-')[-1]
    t = t.strip()
    if t.endswith(':'):
        return idd, ascii_fold(t[:-1]), md_fold([rest] + subs)
    if t == '':
        return idd, None, md_fold([rest] + subs)
    return idd, None, md_fold([t + ' ' + rest] + subs)


def bullets(block, top, sub):
    """Las vinetas de un bloque: [(primera linea, [subvinetas])], con la sangria de la vineta y de la subvineta."""
    items = []
    for l in block:
        if not l.strip():
            continue
        if l.startswith(top + '- '):
            items.append([l, []])
        else:
            assert l.startswith(sub + '- ') and items, ('linea suelta', l[:80])
            items[-1][1].append(l)
    return items


OBJECTIVE_PKG = (
    'Dar a la ventana de Push Back sus dos modos explicitos: CLASSIC sin cambio observable, y WORKSPACE solo por la factoria tipada ForDraft. Separar '
    'aplicar al estado de edicion, aplicar al borrador, cerrar y la escritura clasica. En WORKSPACE la ventana carga una copia del estado del borrador; '
    'Aplicar al borrador devuelve una copia por el puerto de PB-T2a y no cierra; Insertar y el lote no se muestran y ninguna ruta llega a la peticion '
    'clasica; BOM y Guardar en biblioteca siguen, con dos costuras de prueba nuevas; la validacion de campos es la misma en los dos modos. Todo segun el '
    'paquete del contrato de PB-T2 v<n>, con las decisiones del Owner F4B-E23 y Q-PBT2-1..4, y la lista cerrada CT-PBT2B-01..10. El RED es la costura '
    'inerte y las pruebas nuevas; el GREEN toca solo W y el archivo nuevo del modo. El XAML, WC, Application, el Plugin, el piloto y los constructores '
    'actuales no cambian; no se ejecuta AutoCAD. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: '
    'STOP y se reabre L-1 (decision D2 del Owner, F4B-E11).')
assert OBJECTIVE_PKG.count('v<n>') == 1
OBJECTIVE = OBJECTIVE_PKG.replace('v<n>', PKG_VERSION)
RANGES_TXT = ', '.join('%s (W:%s)' % (n, ('%d-%d' % (a, b)) if a != b else str(a)) for a, b, n in RANGES[:-1]) + ' y %s (W:%d-%d)' % (RANGES[-1][2], RANGES[-1][0], RANGES[-1][1])
INV_OWN = [
    ('01', 'Alcance', 'dentro de las 8 rutas, y en src/ solo W y el archivo del modo. git diff BaseSha..CurrentSha -- src/RackCad.UI/Systems/PushBack/'
                      'RackPushBackSystemWindow.xaml src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs src/RackCad.Application '
                      'src/RackCad.Domain src/RackCad.Plugin src/RackCad.UI/Editor src/RackCad.UI/Controls src/RackCad.UI/Workspace src/RackCad.UI/Shell vacio.'),
    ('02', 'RED y GREEN', 'el RED son las clases nuevas y, en W, solo la costura inerte del section 5.3; el GREEN solo W y el archivo del modo. El criterio, '
                          'como INV-PBT1A-02, con Failed == Selected en cada filtro con ExpectRed. CT-09 y CT-10 pasan en el RED y en el GREEN. Con una '
                          'reparacion del RED (F4B-E35, RR): RedSha..RedRepairSha solo toca clases de prueba que el RED anadio, el criterio se '
                          'repite entero sobre RedRepairSha y RedRepairSha..CurrentSha es el GREEN de arriba (v2.7, V27-03).'),
    ('03', None, 'Donde cambia W. Cada hunk de git diff -U0 BaseSha..CurrentSha -- W cae, por su rango en BaseSha y con su documentacion, en un miembro de '
                 'esta lista, o es una insercion entre dos miembros que solo declara uno de los nuevos: los miembros que cambian, con sus rangos de BaseSha, '
                 'documentacion incluida (v2.3, V23-04): ' + RANGES_TXT + '; las declaraciones nuevas: los campos mode, draftPort, appliedState, BomDialog y '
                 'LibraryPathDialog; ModeForTest; ForDraft; el constructor privado; ApplyModeChrome, RefuseInWorkspace, ApplyEditsToEditingState, '
                 'ApplyToDraft y HasUnappliedDraftChanges; y las once constantes de D-PBT2-09.'),
    ('04', None, 'Los testigos y las cuentas, sobre CurrentSha (testigos ASCII, D-PBT2-09): siguen byte a byte S-7, S-9, las cinco sentencias de CT-22 en '
                 'su orden y los literales de CT-23; siguen los testigos sin confirmar que se perder, Corrige los datos: no se puede insertar un modelo inv '
                 'y Corrige los datos: no se puede dibujar un modelo bloqueado.; cada testigo de las once constantes aparece en W; las dos firmas publicas '
                 'de los constructores no cambian; W tiene una sola .RequestUpdate(; ninguna linea anadida de W o del archivo del modo contiene '
                 'forceRebuild, KeyDown, PreviewKeyDown, InputBindings, KeyBinding, TabIndex, IsTabStop, KeyboardNavigation, .Focus( ni DialogResult; '
                 'ninguna linea con SetNumber( se anade ni se quita; LoadFromModel, ReadInputs, Recompute y OnClosing no cambian; el archivo del modo '
                 'solo declara el enum.'),
    ('05', 'CLASSIC sin cambio', 'T1B, PB-T0, PB-TC, PB-TC2, PB-T1a (con las huellas de CT-PBT1A-14 y F-03), PB-T1b y PB-T1c, en verde, en local y en '
                                 'la CI.'),
    ('06', 'Las costuras', 'las 41 de hoy [T1c], con su nombre y su tipo; solo se anaden ModeForTest, BomDialog y LibraryPathDialog.'),
    ('07c', 'D2', 'CT-PBT2B-05 y 09 en verde; CT-PBT1C-02 y 04 y CT-PBT1B-14 y 15, en verde sin tocarlas; ninguna linea anadida asigna el Name de un '
                  'sistema ni muta una linea base o un informe.'),
    ('08b', None, 'La unica escritura en disco de la red es ArchivoTemporal (v2.3, V23-07).'),
]
INV03_NOTE = ('Nota (no va al contrato). Los rangos se midieron en la v2.3 sobre el texto de PB-T1c (0dc623ca), con el metodo de INV-PBT1C-03, y el '
              'generador los vuelve a comprobar en la emision. Es la leccion M10.')
INV_COMMON = [
    ('07', 'Las redes', 'las del section 5.1, las guardas del section 4, el censo del piloto y las subtareas anteriores, en verde. Ninguna prueba '
                        'existente se edita, se salta, se reactiva ni se borra. En PB-T2c, en el RED, solo pueden fallar por los mutantes, con su atribucion.'),
    ('08', 'Las pruebas de UI', 'el regimen del section 5.0 y la sonda del modo antes de cualquier gesto de WORKSPACE; el BOM y "Guardar en biblioteca" '
                                'solo por las costuras nuevas; el configurador solo por HeaderConfiguratorDialog; la unica escritura en disco es '
                                'ArchivoTemporal, borrado en el finally; nunca PromptSaveToLibrary, un RackBomWindow mostrado, un MessageBox real, el '
                                'configurador real ni "Insertar" real; ninguna asercion dentro de una costura; ningun fixture nuevo en Fixtures/I64.'),
    ('09', 'Determinismo', 'GUID-PBT2-0001, SessionId(64), la cultura invariante, "R" y el orden ordinal.'),
    ('10', 'Las corridas locales', 'en UI, solo los filtros del contrato, nunca la suite completa; la de Core, si; sin copias de arboles; las corridas '
                                   'temporales terminan en Assert.Fail, corren dentro de EditorDiscardPrompt.Substitute, no entran en ningun commit y '
                                   'guardan TRX y log; el perfil no cambia, salvo RackCad/RackFrameConfigurator.layout de los datos locales del usuario, que '
                                   'escriben al cerrarse los RackFrameConfiguratorWindow reales de pruebas que ya existian, en los filtros RichEditor y '
                                   'WindowCensusGuardTests (F4B-E30 y F4B-E31; v2.3, V23-01): la prueba de limpieza del perfil se registra, y si solo cambia '
                                   'ese archivo se atribuye a esos filtros, sin parar; cualquier otro cambio del perfil es parada.'),
    ('11', 'La evidencia', 'rutas relativas comprobadas; TreeSha real o null; el Worker no escribe en docs/; una sustitucion seria C02.'),
    ('12', None, 'Las reglas de F4B, sin AutoCAD, con las tres comprobaciones deterministas de F4B-E15 en la verificacion.'),
    ('14', 'D2', 'ninguna copia nueva comparte un objeto mutable fuera de las dos referencias declaradas de cada lado, y ninguna linea anadida muta un '
                 'PushBackSystem ni un RackModuleReconciliationResult adoptado.'),
]
C04_PKG = 'a method of a filter with ExpectRed passes over the inert seam, or fails by an exception.'
C13_NAMES = list(NEW_T2B)
C13_PKG = ('keeping a CLASSIC gesture without observable change, or doing the WORKSPACE one, would require touching a member outside INV-PBT2B-03, WX, WC, '
           'Application or the Plugin, or changing LoadFromModel, ReadInputs, Recompute, OnClosing or the order of a load; or doing the task would require '
           'adding, in W or in PushBackEditorMode.cs, a member declaration with a name that does not exist in BaseSha (of any visibility: method, property, '
           'field, constant, enum member or nested type) other than ' + ', '.join(C13_NAMES[:-1]) + ' and ' + C13_NAMES[-1] + '. The private constructor '
           'RackPushBackSystemWindow(PushBackDraftEditorLaunch) is the only new constructor. The Worker stops before adding it.')
C14_PKG = 'a fingerprint of CT-PBT1A-14 changes, or a CLASSIC test fails. STOP (C03).'
C15_PKG = ('CT-PBT2B-02 gives a difference outside N-PBT2-01, diseno-igual=False, or a design without a branch in step 1. It is a new fact: return to the '
           'Owner (OB-06.5, D-PB-03).')
C16_PKG = 'a gesture would need a real modal, the profile or the disk outside ArchivoTemporal, or the mode probe is not enough to avoid it.'
# ---------------------------------------------------------------- la reparacion del RED (F4B-E35, regla RR del Owner; v2.7, V27-01 a V27-07; v2.8, V28-01 a V28-06)
F4B_E35 = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md'
F4B_E35_ENTRY = 'F4B-E35'
C07_RR = ('except under F4B-E35 (the RED repair rule, RR): a writing error in a new test of this task, found after the RED push and before the GREEN, '
          'is repaired without this stop and without spending an attempt by one declared RED repair commit that touches only test files the RED '
          'added and changes only the when or the how of a reading or a gesture the package already prescribes, and no expected line, literal or '
          'oracle of the package or the contract, with the full RED criterion rerun on RedRepairSha; a second writing error in the same attempt, a '
          'repair that needs an expected line changed, or a literal line the package fixes for the repaired class (an expected line or a literal) '
          'that does not stay byte for byte in it, is this stop')
C18_RR = ('a RED repair does not meet the F4B-E35 rule (RR-1 to RR-6): it is not after the RED push and before the GREEN, or it is the second one in '
          'the attempt; it touches a file the RED did not add, an existing test, src/, a fixture or anything outside the AllowedWriteScope; it changes '
          'an expected line, literal or oracle, or anything other than the when or the how of a reading or a gesture the package already prescribes '
          '(RR-3); the full RED criterion and its CI are not rerun on RedRepairSha; or it is not declared as RR-5 says. The rule does not apply and '
          'the repair counts as touching tests/ after the RED (AUTOMATION_PLAN 16.8): STOP')
CI_RR = ('La CI del RED reparado (F4B-E35, RR-4; v2.7, V27-04): en PB-T2b y PB-T2c, con una reparacion del RED, la CI de RedRepairSha se registra '
         'como la del RED reparado y da lo mismo que la del RED de su subtarea, arriba.')
# la verificacion con una reparacion (RR-7; v2.8, V28-01 y V28-02): las lineas del section 6.0, plegadas
RR_V_HEAD = ('La verificacion con una reparacion del RED (F4B-E35, RR-7, que pide que la verificacion del Controller lea la reparacion como parte de '
             'la entrega; solo en PB-T2b y PB-T2c; v2.8, V28-01). Como F4B-E25 dijo como se comprueba su excepcion a 16.3, esto dice como se leen, '
             'con una reparacion, las comprobaciones de AUTOMATION_PLAN 16.9 que la tocan:')
RR_V_N10 = ('n.o 10 (Tests): el RED se acredita con RedSha y con la CI del RED original, y sus TRX, con los filtros con ExpectRed entre los fallidos. '
            'La clausula "git diff --name-only RedSha CurrentSha no toca RT o ChainRedFiles" se comprueba desde RedRepairSha: git diff --name-only '
            'RedRepairSha CurrentSha.')
RR_V_N11 = 'n.o 11 (Trailer): el trailer va tambien en el commit "RED repair".'
RR_V_CTRL = ('Lo que el Controller comprueba de RR (si algo falla, es C18): RR-1, un solo commit "RED repair", hijo directo de RedSha y anterior al '
             'GREEN; RR-2, git diff --name-only RedSha RedRepairSha dentro de las clases de prueba que anadio el RED (git diff --name-only '
             '--diff-filter=A BaseSha RedSha -- tests/); RR-4, la CI de RedRepairSha, registrada como la del RED reparado, y los TRX de los filtros '
             'con ExpectRed sobre el; RR-5, la declaracion de la entrega, con el diff, y RedSha, el commit RED original; RR-6, con el tramo '
             'RedSha..RedRepairSha.')
RR_V_T2C = ('En PB-T2c, ademas: RedSha..RedRepairSha es solo tests/RackCad.UI.Tests/PushBackWorkspaceCarryoverTests.cs, sin tocar W ni WC (los '
            'mutantes del RED), aunque el GREEN los revierta despues.')
RR_V_AYUDA_T2B = ('La ayuda mecanica de RR-3 (v2.8, V28-02), en PB-T2b: es parada C07, una violacion cierta de RR-3, que una linea literal que el '
                  'paquete fija para sus clases (las tres listas de CT-PBT2B-03 y las lineas fijas de CT-PBT2B-01 a 07) no siga, byte a byte, en la '
                  'clase reparada como en RedSha.')
RR_V_AYUDA_T2C = ('La ayuda mecanica de RR-3 (v2.8, V28-02), en PB-T2c: es parada C07, una violacion cierta de RR-3, que las 49 lineas esperadas de '
                  'CT-PBT2C-03 o las lineas fijas de CT-PBT2C-01 y 02 (los exactos, las doce de CT-PBTC-07 y la de "Aplicar") no sean, byte a byte, las '
                  'mismas en RedSha y en RedRepairSha.')
RR_V_INDICIOS = ('Indicios para la revision, no parada (v2.8, V28-02): una cadena <campo>= anadida o quitada en git diff -U0 RedSha RedRepairSha, o '
                 'un cambio en el primer argumento de un Assert.Equal. Pueden dar falsos positivos, como una linea que solo se mueve de sitio, asi que '
                 'van a Findings y a la revision, con la declaracion de RR-5.')
RR_V_CONTRATO = 'En el contrato: la autoridad F4B-E35 lo resume, y la ExpectedEvidence lo lleva como lo que lee la verificacion.'
RR_VERIF_60 = [RR_V_HEAD, RR_V_N10, RR_V_N11, RR_V_CTRL, RR_V_T2C, RR_V_AYUDA_T2B, RR_V_AYUDA_T2C, RR_V_INDICIOS, RR_V_CONTRATO]
RR_VERIF_EVID = ('Con una reparacion del RED, la verificacion la lee como parte de la entrega (F4B-E35, RR-7; section 6.0 del paquete, v2.8, V28-01 '
                 'y V28-02): ')
RR_VERIF_AUTH = ('La verificacion del Controller la lee como parte de la entrega (RR-7): la n.o 10 (Tests) acredita el RED con RedSha y la CI del RED '
                 'original, y su clausula sobre RT o ChainRedFiles se comprueba desde RedRepairSha; la n.o 11 (Trailer) incluye el commit "RED '
                 'repair"; el Controller comprueba RR-1, RR-2, RR-4, RR-5 y RR-6 con el tramo RedSha..RedRepairSha; y la ayuda mecanica de RR-3: es '
                 'C07 que una linea literal del paquete no siga byte a byte en la clase reparada, y los indicios heuristicos van a Findings y a la '
                 'revision (section 6.0 del paquete, v2.8, V28-01 y V28-02)')
RR_60 = [  # el section 6.0, plegado: la regla, sus siete vinetas, la CI del RED reparado y la verificacion (v2.8)
    'La reparacion del RED (F4B-E35, pregunta 4, la regla RR del Owner; solo en PB-T2b y PB-T2c; v2.7, V27-01): un error de escritura en una prueba '
    'nueva de la tarea, visto despues del commit RED, se repara sin gastar intento (RR-7) si se cumple todo lo que sigue. La regla excepciona '
    'AUTOMATION_PLAN 16.8 ("tests/ no se toca tras el RED") solo en estas condiciones:',
    'RR-1, cuando: despues del commit RED y su push, y antes del commit GREEN; como mucho una reparacion por intento. Un segundo error de escritura '
    'en el mismo intento es la parada de siempre (C07).',
    'RR-2, que toca: solo archivos de tests/ que el commit RED de la tarea anadio; nunca una prueba existente, ni src/ (tampoco la costura inerte ni '
    'los mutantes del RED), ni un fixture, ni nada fuera del AllowedWriteScope.',
    'RR-3, sin cambiar lo esperado: ninguna linea esperada, literal ni oraculo del paquete o del contrato cambia; la reparacion corrige solo el '
    'cuando y el como de una lectura o de un gesto que el paquete ya prescribe. Si para que la prueba pase hace falta cambiar una linea esperada, '
    'es C07.',
    'RR-4, el RED repetido entero: sobre RedRepairSha se vuelve a pasar el criterio completo del RED del contrato; el commit se empuja y su CI se '
    'registra como la del RED reparado.',
    'RR-5, declarado: el commit se titula "RED repair" y nombra la tarea; la entrega declara RedRepairSha, la prueba y la linea corregidas, el error '
    'y por que no cambia ninguna linea esperada, con el diff; RedSha sigue siendo el commit RED original.',
    'RR-6, la cadena: BaseSha..RedSha, el RED de cada contrato (en PB-T2b y PB-T2c ya toca src/); RedSha..RedRepairSha, solo archivos que el RED '
    'anadio; RedRepairSha..CurrentSha, el GREEN de cada contrato. Sin reparacion, la cadena es la de siempre.',
    'En el contrato (ASCII, literales): PBT2x-C07 es el texto de PBT2A-C07 seguido de "; " y de "' + C07_RR + '"; PBT2x-C18 es "' + C18_RR + '".',
    CI_RR,
] + RR_VERIF_60
RR_COMMON = ('Las propias comunes (con el prefijo de cada contrato): C01, C02 (una guarda del section 4 que no queda en verde vuelve al Owner, OB-T1B.3), '
             'C03 (un cambio observable de RACKEDITAR, RPB o el menu: STOP y vuelta al Owner), C05 (un CT necesitaria AutoCAD, un modal real, el '
             'perfil o el disco fuera de ArchivoTemporal), C06, C07 (el Worker no ajusta lo esperado; en PB-T2b y PB-T2c, con la excepcion de la '
             'reparacion del RED de F4B-E35), C09 (rige las pruebas nuevas; la disposicion del configurador que escriben las existentes se atribuye '
             'como dice el invariante 10; v2.3, V23-01) y C10 (corregir algo fuera del alcance: el piloto, el panel, el host core, "Editar todo", la '
             'escritura del panel, el campo de escala del panel, T-2, M-07, pb-21-cola, AUTH-13, PB-TC o PB-TC2) y, en PB-T2b y PB-T2c, C18 (una '
             'reparacion del RED que rompe la regla RR de F4B-E35; v2.7, V27-02).')
RR_CUSTODY = ('en los de PB-T2b y PB-T2c: su contrato y su generador, la salida de sus anclajes y su comprobacion acotada; en el de PB-T2b, ademas, '
              'esta v2.8 (con la v2.5, la v2.6 y la v2.7 en la historia), v23-delta.md, v24-delta.md, v25-delta.md, v26-delta.md, v27-delta.md, '
              'v28-delta.md, measure_pbt2b_t1c.py y el encargo literal de su comprobacion acotada y de su recomprobacion '
              '(independent-review-pbt2b-v23-check-prompt.md), junto al informe (v2.3, V23-06; v2.4, V24-07; v2.5, V25-04; v2.7, V27-07; v2.8, V28-06); '
              'en el de PB-T2c, ademas, el encargo literal de su comprobacion acotada y de su recomprobacion '
              '(independent-review-pbt2c-v25-check-prompt.md), junto al informe (v2.6, V26-06; v2.7, V27-07);')
RR_E_RED = ('el RED: el commit RED y su push antes del GREEN, con su CI tal como dice arriba (sin cuelgue, ni blame-hang ni timeout), y los TRX de '
            'cada filtro con ExpectRed; con una reparacion del RED (PB-T2b y PB-T2c; F4B-E35, RR-4), ademas, el commit "RED repair" y su push antes '
            'del GREEN, con su CI, registrada como la del RED reparado, y los TRX de cada filtro con ExpectRed sobre RedRepairSha;')
RR_E_LOCAL = ('en local: el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED, sobre RedRepairSha si hay reparacion, y sobre el '
              'GREEN, y tres corridas en verde de los filtros con ExpectRed sobre el GREEN;')
RR_E_ENTREGA = ('la entrega: el mapa CT -> metodo, las rutas relativas comprobadas y el Trailer de worker-handoff.json, que es el trailer real de los '
                'commits RED, "RED repair" (si lo hay) y GREEN (Co-Authored-By: ...; la mitad (b) de PBT1B-IR-M14); con una reparacion del RED, '
                'ademas, RedRepairSha, la prueba y la linea corregidas, el error y por que no cambia ninguna linea esperada, con el diff, y RedSha, '
                'que sigue siendo el commit RED original (F4B-E35, RR-5);')
RR_S1 = ('F4B-E35 (F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md, pregunta 4) (v2.7, V27-06)',
         '"Si, con esas condiciones (Recomendado)": la regla operativa de la reparacion del RED, RR-1 a RR-7, regla de F4B que empieza en PB-T2b y '
         'excepciona el 16.8 solo en sus condiciones')
RR_REG_SRC = 'F4B-E35 (Owner), RR'
RR_HDR = ('- **v2.7:** incorpora en PB-T2b y PB-T2c la regla de la reparaci' + chr(0xF3) + 'n del RED (RR-1 a RR-7) que el Owner aprob' + chr(0xF3)
          + ' en F4B-E35, pregunta 4')
RR28_HDR = ('- **v2.8:** las recomprobaciones de la v2.7 (la ' + chr(0xA7) + '11 de CA-T2b y la ' + chr(0xA7) + '10 de CA-T2c) dan **EMITIBLE TRAS '
            'CORRECCIONES**, B 0 / I 1 / M 4 y B 0 / I 1 / M 3')
RR28_REG = [('V28-01', ['**CA-T2b ' + chr(0xA7) + '11 I-01**', '**CA-T2c ' + chr(0xA7) + '10 I-01**']),
            ('V28-02', ['**CA-T2b ' + chr(0xA7) + '11 M-01**', '**CA-T2c ' + chr(0xA7) + '10 M-01**']),
            ('V28-03', ['**CA-T2b ' + chr(0xA7) + '11 M-02**', '**CA-T2c ' + chr(0xA7) + '10 M-02**']),
            ('V28-04', ['**CA-T2b ' + chr(0xA7) + '11 M-03**', '**CA-T2c ' + chr(0xA7) + '10 M-03**']),
            ('V28-05', ['**CA-T2b ' + chr(0xA7) + '11 M-04**']),
            ('V28-06', ['La sesi' + chr(0xF3) + 'n'])]
for _x in [C07_RR, C18_RR, CI_RR, RR_COMMON, RR_CUSTODY, RR_E_RED, RR_E_LOCAL, RR_E_ENTREGA, RR_VERIF_EVID, RR_VERIF_AUTH] + RR_60 + list(RR_S1):
    assert all(ord(ch) < 0x80 for ch in _x), _x[:60]


def rr28_check(hdr, reg_rows):
    """La cabecera de la v2.8 y el registro V28-01 a V28-06, cada fila con su fuente."""
    assert RR28_HDR in hdr, 'la cabecera de la v2.8'
    ids = [re.match(r'^\| (V28-\d\d) \|', l).group(1) for l in reg_rows]
    assert ids == [r for r, _ in RR28_REG], ('el registro V28', ids)
    for l, (rid, srcs) in zip(reg_rows, RR28_REG):
        assert all(s in l.split(' | ')[1] for s in srcs), ('el registro V28: la fuente de cada fila', rid)


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


E_ANTES = ('antes del Worker: el contrato, la salida del Controller (output.json, con las colecciones opacas vacias), la delegacion final con esas '
           'colecciones puestas por la sesion (delegation.json), la comparacion byte a byte, A1-A8 y nc4;')
E_RED = RR_E_RED
E_GREEN = 'el GREEN: el commit GREEN y su CI 4/4;'
E_LOCAL = RR_E_LOCAL
E_ENTREGA = RR_E_ENTREGA
E_DESPUES = 'despues: la verificacion con las tres comprobaciones de F4B-E15, y nc1 a nc3.'
E_COMMON = [E_ANTES, E_RED, E_GREEN, E_LOCAL, E_ENTREGA, E_DESPUES]
CI_RED_HEAD = ('La CI del RED (leccion M14 de PB-T1b): en los tres RED, Core falla, asi que build-plugin sale omitido por su needs (registro O). RedPart '
               'es pass con:')
CI_RED_T2B = 'PB-T2b: tests y ui-tests en failure, build-ui en success;'
RED_CHAIN_HEAD = 'El RED de la cadena en una correccion (section 16.8): una correccion que toque RT o ChainRedFiles exige RED otra vez:'
RED_CHAIN_T2B = 'en PB-T2b, la costura inerte;'
E_T2B = ['el paso 1;', 'la tabla "CT -> la asercion que falla en el RED";', 'la salida de INV-PBT2B-03 y 04 sobre el diff;', 'las autopruebas de CT-08 a 10;',
         'en local, la clase de huellas de PB-T1a tres veces en verde sobre el GREEN.']
LB_PKG = ['Linea base de la CI: la del cierre de PB-T2a, prevista: Core 19 092, UI 2 439. GREEN de PB-T2b:', 'Core + 2 (CT-08 y 09) = 19 094;',
          'UI + 8 (CT-01 a 07 y 10) = 2 447, con 21 Skipped y 2 426 Passed;', 'si Q-PBT2-3 no es la opcion A, UI 2 446.']
CORR_PKG = ('Las correcciones: se leen con F4B-E25. Una ruta UNIT_CHANGE de git diff --name-only AuthorityRevision BaseSha no incumple 16.3 si se cumplen '
            'sus tres condiciones, que comprueba el Controller. Las EXTERNAL, nunca. No hace falta reemitir el contrato para corregir.')
UC_T2B_PKG = 'PB-T2b: W, con la seccion "los miembros de INV-PBT2B-03 (el modo WORKSPACE, D-PBT2-04 a D-PBT2-10)";'
STD_LINE = 'StopConditions estandar (17): S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08, con P-01 = "' + P01_J + '".'
C11_REF = 'section 6.0 of the PB-T1 contract package ([PT1], docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md)'
S1 = {
    'OD': ('[OD] (F4B-DRAFTS/coordinator/owner-f4b-disposition.md)',
           'La fila de PB-T2 (literal arriba). D-PB-01 ("ZERO authored DWG writes", "The mode must be explicit", "Do not let a Workspace invocation '
           'accidentally execute the classic drawing-write path"), D-PB-06, D-PB-08, D-PB-10, section 7.3 (T1B) y section 7.4 (RACKEDITAR)'),
    'P': ('[P] (docs/initiatives/I-64-f4b-push-back-completo-package.md, v3.1)',
          'section 4 (los dos modos y su tabla, :477-493); OB-01.1 a OB-01.6; OB-06.3; OB-07.1; OB-08.2; OB-10.1; OB-T1B.1 a OB-T1B.3; OB-RE.1; H-PB-30 a '
          'H-PB-33; R-PB-09 y R-PB-10; la fila de PB-T2 del section 5 (:504)'),
    'E23': ('F4B-E23 (F4B-DRAFTS/coordinator/owner-f4b-dpb07-extensiondata-decisions.md, pregunta 1; la vuelta, F4B-DRAFTS/pbfull/owner-return-dpb07.md)',
            'D-PB-07: "BOM y biblioteca (Recomendado)", con sus obligaciones para PB-T2'),
    'E24': ('F4B-E24 (el mismo archivo, pregunta 2; la vuelta, F4B-DRAFTS/pbfull/owner-return-extensiondata.md)',
            '"Basta asi (Recomendado)": el panel rechaza todo campo desconocido (AUTH-13); la perdida de RACKEDITAR es una limitacion anterior a F4B'),
    'E25': ('F4B-E25 (F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md; la vuelta, F4B-DRAFTS/pbt1b/owner-return-verify-stop.md)',
            '"Regla F4B y reverificar (Recomendado)": una ruta UNIT_CHANGE de git diff --name-only AuthorityRevision BaseSha de una correccion no incumple '
            '16.3 si (1) todo commit del rango que la toca es el RedSha o el CurrentSha de una entrega de un Worker de la misma cadena, (2) la ruta esta en '
            'el AllowedWriteScope de la delegacion de esa entrega y (3) ningun commit de la sesion la toca. Las EXTERNAL, sin excepcion'),
    'E26': ('F4B-E26 (F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md)',
            'PB-T1b CLOSED / PASS WITH LIMITATIONS sobre a18ba345; sus arrastres M09, M11, M12 y M15 a PB-T2 y PB-T4; la CI del cierre 44438060317'),
    'E27': ('F4B-E27 (f4b-dispositions.md:761; el contrato emitido, F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json) (v2.1, V21-06)',
            'La emision de PB-T1c con [PT1] v2.8: el alcance (W, WC y tres clases), PBT1C-C13, INV-PBT1C-03 con rangos, los testigos ASCII de INV-PBT1C-05 '
            'y las dieciseis autopruebas de CT-PBT1C-04. Es el texto de PB-T1c sobre el que se vuelven a medir las anclas [T1c]'),
    'PV5D15': ('Proposal V5 D-15 (docs/initiatives/I-64-proposal-v5.md:557-582)',
               'Punto 2: "la superficie emite la intencion de Actualizar hacia su host. El host clasico la interpreta como hoy". Punto 3: ida y vuelta del '
               'estado sin la ventana. Punto 4: "el host decide el dueno"'),
    'PV5D16': ('D-16 e INV-19 (:586-593)', 'Sin cambio observable en RACKEDITAR'),
    'ADR': ('ADR-0029 D7, D8, D12 y D13', 'Una politica de cierre; dirty por ambito; sin modelo paralelo; la caracterizacion no se edita para que pase'),
    'PT1': ('[PT1]', 'D-PBT1-02 a D-PBT1-13 (el estado y la ventana sobre el), D-PBT1-15 (D2), section 4 (guardas), section 5.5 (censo), section 10 (A-1, '
                     'A-2, A-7 a A-10)'),
    'OD2': ('[OD2] (F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md, F4B-E11)', 'D2: las dos referencias compartidas, las diez condiciones y el "IMPORTANT LIMIT"'),
    'DP': ('[DP] (F4B-DRAFTS/coordinator/owner-f4b-procedure-decisions.md, F4B-E03)', 'C08 y C11'),
    'E19-22': ('F4B-E19, F4B-E21 y F4B-E22 (owner-config-snapshot-j-decision.md, owner-f4b-opaque-collections-decision.md, owner-pbt1b-rework-decisions.md)',
               'La Snapshot J (P-01); la sesion pone las ocho colecciones opacas; las lecciones M10 y M14'),
    'E06-09': ('F4B-E06 y F4B-E09 (f4b-dispositions.md)', 'Las obligaciones de PB-T2: el paso 4 de D-PB-06 y el aviso de D-PB-08 en WORKSPACE'),
    'CLOSES': ('Los cierres de PB-TC, PB-TC2, PB-T1a, PB-T1b y PB-T1c', 'Las redes que siguen en verde; CT-PBTC-02, 05 y 07 como oraculos'),
    'T1B': ('T1B (docs/initiatives/I-64-f4-t1b-gate-contract-package.md)', '55 casos de UI y 10 de Core, y sus guardas de texto'),
    'E35': RR_S1,
    'PROTO': ('docs/AUTOMATION_PLAN.md section 16.3 a section 16.11; docs/automation/agent-execution/README.md section 4 y section 10; routing.md; model-catalog.md',
              'El protocolo, A1-A8, el RED de la cadena, las 14 comprobaciones, los terminos, las paradas y las celdas'),
}
OD_ROW = ('ACCEPT IN PRINCIPLE. Separate: "apply edits to current editing state" from: "close editor" and from: "write authored DWG state". Workspace mode '
          'must return edited state to DraftSession. Classic RACKEDITAR may continue its compatible existing outer workflow. Avoid two independent '
          'implementations of field validation.')
EXPECT_60 = [
    'git diff <cierre de PB-T2a> <BaseSha> -- src/RackCad.UI vacio;',
    'todas las anclas [T1c] del section 2 y del section 4 sobre el texto real de PB-T1c, con los rangos de INV-PBT2B-03;',
    'La comprobacion acotada: un revisor independiente, en solo lectura, sobre este paquete, la seccion de la subtarea y la salida de sus anclajes. Su '
    'resultado, EMITIBLE o NO EMITIBLE, se custodia en F4B-DRAFTS/pbt2/.',
    RR_CUSTODY,
    'La delegacion (F4B-E21): el Controller deja vacias las ocho colecciones opacas, y la sesion las pone byte a byte. La comparacion byte a byte, A1-A8 '
    'y nc4 se hacen sobre la delegacion final.',
    '[P];', '[PT1];', 'este paquete, el registro y v2-delta.md;', 'IR1 y las comprobaciones acotadas;',
    'las decisiones del Owner F4B-E03, E09, E11, E19, E21, E22, E23, E24 y E25 (sus archivos de coordinator/) y sus vueltas;',
    'f4b-dispositions.md y los cierres de PB-T1b y PB-T1c;', 'la Proposal V5 (docs/initiatives/I-64-proposal-v5.md);', 'el propio contrato.',
    UC_T2B_PKG,
    'EXTERNAL: docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md (D7, D8, D12 y D13), docs/AUTOMATION_PLAN.md (section 16), '
    'docs/automation/agent-execution/README.md, routing.md y model-catalog.md.',
    CORR_PKG,
    'src/: 87 en PB-T2a, 60 en PB-T2b y 59 en PB-T2c (medidos en 95373af6). En PB-T2c sube a 60 tras PB-T2b, por el archivo del modo;',
    'tests/: la lista curada de PB-T0 (18 entradas); en tests/RackCad.UI.Tests/, la de [PKC2] (51 archivos y Workspace/), las clases de PB-TC2, PB-T1a, '
    'PB-T1b y PB-T1c y las de las subtareas anteriores de PB-T2;',
    'la raiz: las 17 entradas de [PKC2].', STD_LINE,
    'Las de [DP], literales en cada contrato (x = A, B o C): PBT2x-C08, PBT2x-C11 y PBT2x-C12, con el texto de [PT1] section 6.0, cambiando solo el '
    'prefijo y, en PBT2x-C11, la referencia "section 6.0 of the package" por "' + C11_REF + '" (v2.2, V22-01). Ninguna aceptacion puntual anterior de '
    'un control se extiende.',
    'La parada del limite de D2: PBT2x-C17, con el texto de [PT1] section 6.0 y el literal del Owner. El tiempo de vida incluye el de la ventana de WORKSPACE.',
    RED_CHAIN_HEAD, RED_CHAIN_T2B, CI_RED_HEAD, CI_RED_T2B,
    'EligibleCells: Worker anthropic/subagent/claude-sonnet-5-5 con effort high; Controller openai/process/gpt-6-luna con effort high. Vencen el 2026-12-29 '
    '(model-catalog.md:14, :51, :97). Se comprueban en cada emision y delegacion (A7).',
    'Invariantes comunes (cada contrato los numera INV-PBT2x-07 a 12 y 14). Lo normativo va en el contrato; las notas no (v2, V2-27).',
    RR_COMMON,
    'ExpectedEvidence comun (v2, V2-28):',
] + E_COMMON + RR_60
EXPECT_62 = [
    'AllowedWriteScope (8):', 'W;', 'src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs;',
    'en tests/RackCad.UI.Tests/: PushBackWorkspaceSupport.cs, PushBackWorkspaceModeTests.cs, PushBackWorkspaceApplyCloseTests.cs y PushBackFieldValidationConcordanceTests.cs;',
    'en tests/RackCad.Tests/: PushBackWorkspaceModeSourceGuardTests.cs y PushBackWorkspaceSharedReferenceGuardTests.cs.',
    'ForbiddenWriteScope: 60 de src/ (WC y WX incluidos) y lo comun.', 'Authorities: las comunes, y W como UNIT_CHANGE ("los miembros de INV-PBT2B-03").',
    'StopConditions: las 17 estandar, C08, C11, C12, C17, las comunes, y:', 'ExpectedEvidence: la comun, y:',
] + E_T2B + LB_PKG
EXPECT_53 = [
    'internal PushBackEditorMode ModeForTest => PushBackEditorMode.Classic;;',
    'public static RackPushBackSystemWindow ForDraft(PushBackDraftEditorLaunch launch) => new RackPushBackSystemWindow(canInsertInAutoCad: false);;',
    'internal Action<RackCad.Application.Bom.BillOfMaterials, Window> BomDialog; e internal Func<Window, string, string> LibraryPathDialog;, declaradas y sin usar;',
    'PushBackEditorMode.cs, completo.',
    'en tests/RackCad.UI.Tests/: PushBackWorkspaceSupport (el ayudante), PushBackWorkspaceModeTests (01, 02 y 07), PushBackWorkspaceApplyCloseTests (03 a 06) y '
    'PushBackFieldValidationConcordanceTests (10);',
    'en tests/RackCad.Tests/: PushBackWorkspaceModeSourceGuardTests (08) y PushBackWorkspaceSharedReferenceGuardTests (09).',
    'CT-PBT2B-08 falla porque sus reglas no se cumplen;', 'CT-PBT2B-09 y 10 pasan en el RED y discriminan por sus autopruebas.',
]
CT_METHODS = ['CT_PBT2B_01_TheModeIsExplicitAndFixedAndClassicDoesNotChange', 'CT_PBT2B_02_WorkspaceLoadsACopyOfTheDraftStateWithoutGoingThroughADesign',
              'CT_PBT2B_07_InWorkspaceTheWindowOwnsTheBomAndTheSaveDialog',
              'CT_PBT2B_03_NoWorkspaceGestureFlagsAClassicRequestAndBomAndLibraryLeaveTheDraftAndItsIdentityAlone',
              'CT_PBT2B_04_ApplyToDraftHandsTheEditedStateToTheDraftOnceAndKeepsTheWindowOpen',
              'CT_PBT2B_05_WorkspaceAndClassicShareOneValidationAndNeitherMutatesTheSharedReferences', 'CT_PBT2B_06_ClosingAsksOnlyForWhatWasNotApplied',
              'CT_PBT2B_08_NoWorkspacePathReachesTheClassicWritePath', 'CT_PBT2B_09_TheNewFileOfTheModeMutatesNoSharedReferenceAndOnlyDeclaresTheEnum',
              'CT_PBT2B_10_ThePanelScaleFieldAndTheWindowScaleBoxAcceptAndRejectTheSameTexts']
# D-PBT2-09: los once literales y sus testigos ASCII (V2-29)
LITERALS = [
    ('WorkspaceApplyLabel', 'Aplicar al borrador'), ('WorkspaceApplyToolTip', 'Guarda los cambios en el borrador. El dibujo cambia con '),
    ('WorkspaceApplyBlockedToolTip', ' bloqueante: el rack no puede pasar al borrador as'),
    ('WorkspaceRefusalMessage', 'En el editor del borrador no se dibuja: aplica al borrador y actualiza desde el panel.'),
    ('WorkspaceLoadRefusalMessage', 'La ventana del borrador solo carga el estado del borrador.'),
    ('WorkspaceInvalidMessage', 'Corrige los datos: no se puede aplicar al borrador un modelo inv'),
    ('WorkspaceBlockedMessage', 'Corrige los datos: no se puede aplicar al borrador un modelo bloqueado.'),
    ('WorkspaceNothingToApplyMessage', 'No hay cambios que aplicar al borrador.'),
    ('WorkspaceAppliedMessage', 'Cambios aplicados al borrador. El dibujo cambia con '),
    ('WorkspaceNotEditableMessage', 'El borrador no admite cambios ahora: cierra el editor y revisa el panel.'),
    ('WorkspaceUnappliedQuestion', 'Hay cambios sin aplicar al borrador que se perder'),
]
assert [n for n, _ in LITERALS] == NEW_T2B[12:23]
for n, wtn in LITERALS:
    assert all(ord(ch) < 0x80 for ch in wtn) and wtn not in WT_TEXT, ('testigo ASCII de un literal nuevo', n)
PKG_CHECKS = []
REQ_PKG_ROWS = None
if PKG_OK:
    SEC62, SEC60, SEC1, SEC53 = section('### 6.2 '), section('### 6.0 '), section('## 1. '), section('### 5.3 ')
    F62, F60, F1, F53 = ([md_fold([l]) for l in s if l.strip()] for s in (SEC62, SEC60, SEC1, SEC53))
    _obj = [l for l in SEC62 if l.startswith('- **Objective** (ASCII): ' + chr(0xAB))]
    assert len(_obj) == 1 and ascii_fold(_obj[0][_obj[0].index(chr(0xAB)) + 1:_obj[0].rindex(chr(0xBB))]) == OBJECTIVE_PKG, 'el Objective del paquete difiere'
    for x in EXPECT_62:
        once(F62, x, 'section 6.2')
    for x in EXPECT_60:
        once(F60, x, 'section 6.0')
    for x in EXPECT_53:
        once(F53, x, 'section 5.3')
    # los invariantes propios: ocho vinetas (03 y 04 con subvinetas) y una sola nota, la de 03, que no va al contrato
    _a = SEC62.index('- **Invariantes propios:**')
    _b = [k for k, l in enumerate(SEC62) if l.startswith('- **`StopConditions`:**')][0]
    _items = bullets(SEC62[_a + 1:_b], '  ', '    ')
    _notes = [s for _, subs in _items for s in subs if s.strip().startswith('- **Nota (no va al contrato).**')]
    assert len(_notes) == 1 and md_fold(_notes) == INV03_NOTE, ('la nota de INV-PBT2B-03', _notes)
    _own = [inv_bullet(f, [s for s in subs if s not in _notes]) for f, subs in _items]
    assert _own == INV_OWN, ('los invariantes propios del paquete difieren', _own)
    assert sum(1 for l in PKG_LINES if l.strip().startswith('- **Nota (no va al contrato).**')) == 3, 'las notas del paquete (6.1, 6.2 y 6.3)'
    # las paradas propias: cinco vinetas entre "StopConditions" y "ExpectedEvidence"
    _c = [k for k, l in enumerate(SEC62) if l.startswith('- **`ExpectedEvidence`:**')][0]
    _stops = [md_fold([l]) for l in SEC62[_b + 1:_c] if l.strip()]
    assert _stops == ['PBT2B-C04: ' + C04_PKG, 'PBT2B-C13 (v2, V2-26): ' + C13_PKG, 'PBT2B-C14: ' + C14_PKG, 'PBT2B-C15: ' + C15_PKG, 'PBT2B-C16: ' + C16_PKG], (
        'las paradas propias del section 6.2', _stops)
    # los invariantes comunes del section 6.0
    _h = [k for k, l in enumerate(SEC60) if l.startswith('**Invariantes comunes**')][0]
    _blk = []
    for l in SEC60[_h + 1:]:
        if not l.strip():
            break
        _blk.append(l)
    _com = [inv_bullet(f, s) for f, s in bullets(_blk, '', '  ')]
    assert _com == INV_COMMON, ('los invariantes comunes del paquete difieren', _com)
    # PBT2B-C13 por nombre: los nombres son las declaraciones nuevas de INV-PBT2B-03, los once literales de D-PBT2-09 y el enum de D-PBT2-04
    _decl = [s for f, subs in _items if 'INV-PBT2B-03.' in f for s in subs if 'las declaraciones nuevas' in s]
    assert len(_decl) == 1
    _new = re.findall(r'`(\w+)`', _decl[0])
    _d09 = section('### D-PBT2-09. ')
    _rows = [re.match(r'^\| `(\w+)` \| `(.*)` \| (?:`(.*)`|el texto entero) \|$', l) for l in _d09 if l.startswith('| `Workspace')]
    assert len(_rows) == 11 and all(_rows), 'la tabla de los once literales'
    _lit = [(m.group(1), m.group(3) if m.group(3) is not None else m.group(2)) for m in _rows]
    assert _lit == LITERALS, ('los testigos ASCII de D-PBT2-09', _lit)
    for m in _rows:
        assert (m.group(3) or m.group(2)) in m.group(2), ('el testigo no es parte del literal', m.group(1))
    _enum = re.findall(r'`public enum (\w+) \{ (\w+) = 0, (\w+) = 1 \}`', '\n'.join(section('### D-PBT2-04. ')))
    assert len(_enum) == 1, 'el enum de D-PBT2-04'
    assert _new + [n for n, _ in _lit] + list(_enum[0]) == C13_NAMES, ('PBT2B-C13: los nombres no son los de INV-PBT2B-03, D-PBT2-09 y D-PBT2-04', _new)
    _d04 = '\n'.join(section('### D-PBT2-04. '))
    for frag in ('`private readonly PushBackEditorMode mode;`', '`public static RackPushBackSystemWindow ForDraft(PushBackDraftEditorLaunch launch)`',
                 '`private RackPushBackSystemWindow(PushBackDraftEditorLaunch launch) : this(canInsertInAutoCad: false)`', '`internal PushBackEditorMode ModeForTest => mode;`',
                 '`throw new ArgumentNullException(nameof(launch));`'):
        assert frag in _d04, ('D-PBT2-04', frag)
    assert 'UpdateCompositeEnabled();                                      // el sitio de W:314, como en LoadExisting' in '\n'.join(section('### D-PBT2-05. ')), 'V23-03: D-PBT2-05'
    # los CT de la lista cerrada y las quince autopruebas de CT-09
    _ct = [l for l in F53 if re.match(r'^\| \d\d ', l)]
    assert [re.match(r'^\| (\d\d) (?:\(\w+\) )?(\w+) \|', l).group(2) for l in _ct] == CT_METHODS, 'los CT de la lista cerrada del section 5.3'
    assert 'con las quince autopruebas inyectadas en su texto' in [l for l in _ct if 'CT_PBT2B_09' in l][0], 'CT-PBT2B-09: las quince autopruebas'
    # V23-02: CT-02 tras Recalcular, CT-PBT2C-03 tras Aplicar, la receta y L-PBT2-13
    assert 'Toda linea con diseno-igual de este CT se toma tras un recalculo neutro (Recalcular(w), section 5.0; v2.3, V23-02)' in [l for l in _ct if 'CT_PBT2B_02' in l][0]
    _f54 = [md_fold([l]) for l in section('### 5.4 ') if l.strip()]
    assert any('diseno-igual se toma tras "Aplicar", que recalcula antes de aplicar (ApplyEditsToEditingState; v2.3, V23-02)' in l for l in _f54), 'V23-02: CT-PBT2C-03'
    _f50 = [md_fold([l]) for l in section('### 5.0 ') if l.strip()]
    assert any(l.startswith('Recalcular(w) (v2.3, V23-02): un recalculo sin editar ningun campo') and 'PushBackWindowOnStateTests.cs:449-450' in l for l in _f50), 'V23-02: la receta'
    _f83 = [md_fold([l]) for l in section('### 8.3 ') if l.strip()]
    assert any(l.startswith('L-PBT2-13 (v2.3, V23-02; F4B-E31, OWNER-01). "Restaurar" no recalcula antes de leer el diseno') for l in _f83), 'V23-02: L-PBT2-13'
    # V23-03: las citas de W y WC del cuerpo son exactamente las de CITES (fuera de los registros de cambios)
    _cite = re.compile(r'\b(WC|W):(\d+)(?:-(\d+))?((?:(?:,| y)\s*:\d+(?:-\d+)?)*)')
    _cont = re.compile(r':(\d+)(?:-(\d+))?')
    _body, _hist = set(), False
    for l in PKG_LINES:
        if l.startswith('## '):
            _hist = l.startswith('## Registro de cambios')
        if _hist:
            continue
        for m in _cite.finditer(l):
            _body.add((m.group(1), int(m.group(2)), int(m.group(3) or m.group(2))))
            for c2 in _cont.finditer(m.group(4) or ''):
                _body.add((m.group(1), int(c2.group(1)), int(c2.group(2) or c2.group(1))))
    assert _body == set(CITES), ('V23-03: las citas de W y WC del cuerpo del paquete', sorted(_body ^ set(CITES))[:6])
    # la cabecera y los registros de la v2.3, de la v2.4 y de la v2.5
    _hdr = '\n'.join(PKG_LINES[:20])  # con las lineas de emision delante (v2.8)
    assert '- **v2.3:** prepara la emisi' in _hdr and '(F4B-E31; W y WC, el GREEN `161a0dad`)' in _hdr and '(F4B-E32; la punta `0dc623ca`)' in _hdr, 'la cabecera de la v2.3'
    assert ('- **v2.4:** la comprobaci' in _hdr and '`F4B-DRAFTS/pbt2/independent-review-pbt2b-v23-check.md`, citada **CA-T2b**' in _hdr
            and '**EMITIBLE TRAS CORRECCIONES, 0/3/7**' in _hdr and 'va en el intento 2 de 3 (F4B-E33)' in _hdr), 'la cabecera de la v2.4'
    assert ('- **v2.5:** la recomprobaci' in _hdr and '**EMITIBLE, nuevos B 0 / I 0 / M 4**' in _hdr
            and 'M-08 en los generadores y M-09 a M-11 en este texto' in _hdr), 'la cabecera de la v2.5'
    assert RR_HDR in _hdr and 'PB-T2b se emite ya con la v2.7.' in _hdr, 'la cabecera de la v2.7'
    _r27 = [l for l in section('## Registro de cambios v2.6 ') if l.startswith('| V27-')]
    assert [re.match(r'^\| (V27-\d\d) \|', l).group(1) for l in _r27] == ['V27-%02d' % k for k in range(1, 8)], 'el registro V27'
    for _l in _r27:
        assert _l.split(' | ')[1].startswith(RR_REG_SRC), ('el registro V27: la fuente de cada fila', _l[:12])
    # v2.8: las recomprobaciones de la v2.7 (V28-01 a V28-06)
    rr28_check(_hdr, [l for l in section('## Registro de cambios v2.7 ') if l.startswith('| V28-')])
    _r25 = [l for l in section('## Registro de cambios v2.4 ') if l.startswith('| V25-')]
    assert [re.match(r'^\| (V25-\d\d) \|', l).group(1) for l in _r25] == ['V25-%02d' % k for k in range(1, 5)], 'el registro V25'
    for k, ca in enumerate(['M-08', 'M-09', 'M-10', 'M-11'], 1):
        assert ('**CA-T2b ' + chr(0xA7) + '10 %s**' % ca) in _r25[k - 1], ('el registro V25: cada hallazgo de la recomprobacion tiene su fila', ca)
    # V25-02 (M-09) y V25-03 (M-10)
    _f50v = ' '.join(md_fold([l]) for l in section('### 5.0 ') if l.strip())
    assert ('En Push Back la tolerancia no entra en ninguna geometria: AutoBeamLength la recibe y no la usa, el resolutor solo comprueba que es positiva y '
            'la guarda en el sistema') in _f50v and 'geometria transversal' not in _f50v, 'V25-02: la tolerancia (section 5.0)'
    assert ('Antes de cada clic en "Guardar en biblioteca", devuelva la costura ArchivoTemporal o null, el archivo se borra si existe: archivo=True dice '
            'que ese clic escribio, y archivo=False, que no escribio (v2.4, V24-13; v2.5, V25-03).') in _f50v, 'V25-03: ArchivoTemporal (section 5.0)'
    assert 'geometria transversal' not in md_fold([l for l in section('## Registro de cambios v2.3 ') if l.startswith('| V24-11 |')]), 'V25-02: V24-11'
    _reg = section('## Registro de cambios v2.2 ')
    assert [re.match(r'^\| (V23-0\d) \|', l).group(1) for l in _reg if l.startswith('| V23-')] == ['V23-0%d' % k for k in range(1, 8)], 'el registro V23'
    _reg24 = section('## Registro de cambios v2.3 ')
    _r24 = [l for l in _reg24 if l.startswith('| V24-')]
    assert [re.match(r'^\| (V24-\d\d) \|', l).group(1) for l in _r24] == ['V24-%02d' % k for k in range(1, 17)], 'el registro V24'
    for k, ca in enumerate(['I-01', 'I-02', 'I-03', 'M-01', 'M-02', 'M-03', 'M-04', 'M-05', 'M-06', 'M-07'], 1):
        assert ('**CA-T2b %s**' % ca) in _r24[k - 1], ('el registro V24: cada hallazgo de CA-T2b tiene su fila', ca)
    assert ('- **La v2.4** (V24-16) lee `17872be8`' in '\n'.join(PKG_LINES[:80])
            and all(('`%s`' % b) in '\n'.join(PKG_LINES[:80]) for b in WINDOW_BLOBS.values())), 'V24-16: la base de la medicion'
    # V24: las recetas corregidas, fila por fila (lo que no puede volver atras sin que se note)
    _row = {n: [l for l in _ct if ('CT_PBT2B_%s' % n) in l][0] for n in ('01', '02', '03', '04', '05', '06', '07')}
    V24_FP = [
        ('01', 'En la misma ventana de (ii) (v2.4, V24-09)'), ('01', 'registran su Closed como S0.Ventana'),
        ('02', 'nombre=<Nombre(w.RackName)> peticion=False'), ('02', 'Lo esperado de nombre es Nombre(draft.CurrentDraftState.Name)'),
        ('02', 'seleccion=<Sel(w.EditableStateForTest)>'), ('02', 'Lo esperado sale del estado de entrada (v2.4, V24-03; CA-T2b I-03)'),
        ('02', 'seleccion=Both/<f>,<n>, seleccion=B/<f>,<n> y seleccion=A/<f>,<n>'), ('02', 'SideSelectorBox a 2 es Both y a 1 es B (WC:154-156)'),
        ('02', 'tras-restaurar=Todavia no hay un sistema valido que restaurar. llamadas-tras-aplicar=0'),
        ('02', 'Click(RestoreButton) y, justo despues, tras-restaurar = StatusText; despues "Aplicar"'),
        ('04', '- SetNumberR(ToleranceBox, 4.5) -> editar aplicar-habilitado=True'), ('04', 'SetNumberR(ToleranceBox, 5) y "Aplicar"'),
        ('04', 'sin-cambio = ReferenceEquals(draft.CurrentDraftState, actual) && Igual(draft.CurrentDraftState, c)'),
        ('04', 'tomados justo antes de ese "Aplicar"'),
        ('05', 'SetNumberR(ToleranceBox, 4.5) y "Aplicar") -> cuarta-ventana llamadas=1 huellas-iguales=True descartes=0'),
        ('05', 'Las toma un solo Vigia (el de CT-PBT1C-02, copiado: PushBackWindowOnStateTests.cs:390-409)'),
        ('05', 'de draft.CurrentDraftState y de draft.InitialDraftState justo tras la carga y tras cada paso'),
        ('06', 'cada una sobre su propio Borrador(Estado(pb-01-base))'), ('06', 'pone la respuesta en "Si" y llama a Cerrar(w)'),
        ('06', 'ventana 2, SetNumberR(ToleranceBox, 4.5) y "Cerrar" con "No" -> cerrar-con-cambios-no avisos=1'),
        ('06', 'ventana 3, SetNumberR(ToleranceBox, 4.5), "Aplicar" y "Cerrar" -> cerrar-tras-aplicar avisos=0'),
        ('06', 'en la CLASSIC, SetNumberR(ToleranceBox, 4.5) y "Cerrar" -> clasico-con-cambios avisos=0'),
        ('07', 'En una ventana de WORKSPACE sobre Estado(pb-01-base)'), ('07', 'nombre=PB-01 archivo=False peticion=False descartes=0'),
        ('07', 'La linea se arma justo tras el y antes de "Guardar en biblioteca", que vuelve a recalcular (W:3368)'),
        ('07', 'Bom_Click recalcula antes de llamar a la costura (W:3350)'),
        ('07', 'ArchivoTemporal se borra antes del clic, tambien con la costura devolviendo null (v2.5, V25-03)'),
    ]
    for n, fp in V24_FP:
        assert fp in _row[n], ('V24: CT-PBT2B-' + n, fp)
    assert 'ToleranceBox=4 y' not in _row['06'] and 'nombre=<NameBox.Text>' not in _row['07'] and '<ActiveSelection>' not in _row['02'], 'V24: lo viejo'
    _53 = [md_fold([l]) for l in SEC53 if l.strip()]
    assert '  ToleranceBox=4.5 <C> borrador-igual=True llamadas=0' in SEC53 and '  ToleranceBox=4 <C> borrador-igual=True llamadas=0' not in SEC53, 'V24-08'
    assert any('La edicion es SetNumberR(ToleranceBox, 4.5) (v2.4, V24-08 y V24-11; CA-T2b M-05)' in l for l in _53), 'V24-08: la pasada 1'
    for fp in ('El momento de cada lectura (v2.4, V24-06; CA-T2b M-03; las lecciones de F4B-E30 y F4B-E33). Es normativo en PB-T2b y PB-T2c:',
               'cada linea se arma justo despues de su ultimo gesto y antes del siguiente.',
               'cada recalculo valido sustituye la computacion (lastComputation = computation;, W:607)',
               'aplicado (PB-T2c) = !ReferenceEquals(actual, draft.CurrentDraftState), con actual = draft.CurrentDraftState tomado justo antes del clic en "Aplicar"',
               'Los gestos (v2.4, V24-01 y V24-11; CA-T2b I-01 y M-05).',
               'una edicion de una caja numerica es SetNumberR, que eleva LostFocus y recalcula (Input_Changed, W:1135)',
               'Los 17 fixtures de Fixtures/I64 tienen PalletTolerance = 4, asi que las ediciones de tolerancia de esta red escriben 4.5',
               'la primera linea de cada ventana de WORKSPACE es modo=<w.ModeForTest>; en CT-PBT2B-01 (ii) es el primer campo de su linea',
               'Antes de cada clic en "Guardar en biblioteca", devuelva la costura ArchivoTemporal o null, el archivo se borra si existe',
               'Nombre(x) (v2.4, V24-12) = <null> si x es nulo; si no, [x].', 'Sel(e) (v2.4, V24-03) = e.Editor.ActiveSelection'):
        assert any(fp in l for l in _f50), ('V24: section 5.0', fp)
    _f52 = [md_fold([l]) for l in section('### 5.2 ') if l.strip()]
    assert any('En este orden (v2.4, V24-10; CA-T2b M-07; C-1 de F4B-E33): estado-es-el-actual' in l and 'e id se leen antes de aplicar' in l
               for l in _f52), 'V24-10: CT-PBT2A-02'
    for fp in ('con Compara = true (los 49 del censo, "Restaurar" incluido)', 'Los cuatro gestos de D2 (campo-en-error, calculo-invalido, '
               'caja-escrita-sin-salir y cerrar-sin-pedir) no entran', '"Aplicar" aplica: aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0',
               'aplicado=False igual-a-la-ventana=True diseno-igual=True descartes=0',
               'aplicado=False igual-a-la-ventana=False diseno-igual=False descartes=1', 'aplicado es el del section 5.0: el borrador cambio de objeto (v2.4, V24-14)'):
        assert any(fp in l for l in _f54), ('V24-14: section 5.4', fp)
    assert any(l.startswith('| R-PBT2-22 | El momento de la lectura y los gestos que no hacen lo que dicen') for l in [md_fold([l]) for l in section('## 7. ') if l.strip()]), 'R-PBT2-22'
    assert 'En la red (v2.4, V24-01; CA-T2b I-01) no hay foco real' in md_fold(section('### D-PBT2-07. ')), 'V24-01: D-PBT2-07'
    assert any('En la red, sin foco real, el gesto es SetNumberR (D-PBT2-07; v2.4, V24-01).' in l for l in _f83), 'V24-01: L-PBT2-10'
    assert any('DialogWindowCharacterizationTests (20: los ejecutables;' in l for l in [md_fold([l]) for l in section('### 5.1 ') if l.strip()]), 'V24-05: section 5.1'
    # section 1, [OD] e IR1
    for k, (c1, c2) in S1.items():
        once(F1, '| %s | %s |' % (c1, c2), 'section 1, ' + k)
    assert ('> ' + OD_ROW) in PKG_LINES[:24] and '(`F4B-DRAFTS/pbt2/independent-review-v1.md`, citada **IR1**)' in '\n'.join(PKG_LINES[:12])
    REQ_PKG_ROWS = SEC62
    PKG_CHECKS = ['Objective', 'section 6.2 (%d lineas)' % len(EXPECT_62), 'section 6.0 (%d lineas)' % len(EXPECT_60), 'costura inerte y clases (5.3)',
                  'INV propios literales sin su nota', 'paradas C04, C13, C14, C15 y C16', 'INV comunes (10 con V23-01)', 'P-01 J',
                  'C13 = INV-PBT2B-03 + D-PBT2-09 + enum', 'los once testigos ASCII', 'D-PBT2-04 y D-PBT2-05', 'CT-PBT2B-01..10 y las quince de CT-09',
                  'V23-02 (CT-02, CT-PBT2C-03, receta, L-PBT2-13)', 'citas del cuerpo (%d)' % len(CITES),
                  'cabecera y registros V23-01..07, V24-01..16 y V25-01..04', 'V25-02 y V25-03 (section 5.0, V24-11, CT-07)',
                  'V24 (%d huellas de CT-01 a 07, pasada 1, section 5.0, 5.1, 5.2, 5.4, R-PBT2-22, D-PBT2-07 y L-PBT2-10)' % len(V24_FP), 'section 1 (%d filas)' % len(S1),
                  'ExpectedEvidence comun y de 6.2', 'linea base de la CI', 'custodia', 'F4B-E25', 'RequiredTests',
                  'la regla RR de F4B-E35 (V27-01 a V27-07: section 6.0, C07, C18, INV-PBT2B-02, cabecera y registro)',
                  'V28-01 a V28-06 (la verificacion con una reparacion, la ayuda de RR-3, el trailer, C07 y C18, cabecera y registro)']


def package_required(sec):
    """La tabla de RequiredTests del section 6.2: (proyecto, filtro, minimo del paquete, ExpectRed, en negrita)."""
    rows = []
    for l in sec:
        m = re.match(r'^  \| (UI|Core) \| (.*?) \| (.*?) \| (true|false) \|$', l)
        if not m:
            continue
        proj, fcell, mcell, er = m.groups()
        filters = re.findall(r'`([^`]+)`', fcell)
        vals = [v.strip() for v in mcell.split(', ')]
        assert len(vals) == len(filters), ('fila', l, vals)
        for f, v in zip(filters, vals):
            mm = re.fullmatch(r'(\*\*)?(\d{1,3}(?: \d{3})+|\d+)(\*\*)?', nspace(v))
            assert mm and bool(mm.group(1)) == bool(mm.group(3)), ('minimo', l, v)
            rows.append((proj, f, int(mm.group(2).replace(' ', '')), er == 'true', bool(mm.group(1))))
    return rows


# ---------------------------------------------------------------- RequiredTests: (proyecto, filtro, minimo, ExpectRed, clase): 'new' (negrita), 'pt2a' (de PB-T2a), 'fixed'
UIP, CP = UT + 'RackCad.UI.Tests.csproj', TR + 'RackCad.Tests.csproj'
REQ_SPEC = [
    ('UI', 'PushBackWorkspaceModeTests', 3, True, 'new'), ('UI', 'PushBackWorkspaceApplyCloseTests', 4, True, 'new'),
    ('UI', 'PushBackFieldValidationConcordanceTests', 1, False, 'new'), ('Core', 'PushBackWorkspaceModeSourceGuardTests', 1, True, 'new'),
    ('Core', 'PushBackWorkspaceSharedReferenceGuardTests', 1, False, 'new'),
    ('Core', 'PushBackDraftStatePortTests', 3, False, 'pt2a'), ('Core', 'PushBackDraftPortSharedReferenceGuardTests', 1, False, 'pt2a'),
    ('Core', 'PushBackWindowOnStateSourceGuardTests', 1, False, 'fixed'), ('Core', 'PushBackWindowSharedReferenceGuardTests', 1, False, 'fixed'),
    ('Core', 'PushBackSharedReferenceGuardTests', 2, False, 'fixed'),
    ('Core', 'PushBackEditableState', 13, False, 'fixed'), ('Core', 'PushBackEditorOutputGate', 1, False, 'fixed'),
    ('Core', 'PushBackClassicCharacterization', 10, False, 'fixed'), ('Core', 'PushBackFullEditorCharacterization', 3, False, 'fixed'),
    ('Core', 'PushBackModuleEditorCharacterizationTests', 15, False, 'fixed'), ('Core', 'I55DeferredProductFixCharacterizationTests', 3, False, 'fixed'),
    ('Core', 'NamespaceFolderGuardTests', 7, False, 'fixed'), ('Core', 'Workspace', 3271, False, 'fixed'),
    ('UI', 'PushBackWindowOnStateTests', 2, False, 'fixed'), ('UI', 'PushBackEditableStateParityTests', 1, False, 'fixed'),
    ('UI', 'PushBackStateNet', 14, False, 'fixed'), ('UI', 'PushBackClassicCharacterization', 55, False, 'fixed'),
    ('UI', 'PushBackFullEditorCharacterization', 16, False, 'fixed'), ('UI', 'PushBackDeclaredCorrection', 14, False, 'fixed'),
    ('UI', 'PushBackCompositeWindowTests', 52, False, 'fixed'), ('UI', 'PushBackCompositeLifecycleWiringTests', 7, False, 'fixed'),
    ('UI', 'PushBackCompositeLoadExistingModulesTests', 3, False, 'fixed'), ('UI', 'PushBackBothModeCellBeamOverrideTests', 10, False, 'fixed'),
    ('UI', 'PushBackModuleEditorWindowTests', 13, False, 'fixed'), ('UI', 'PushBackAdvancedRackParametersWindowTests', 9, False, 'fixed'),
    ('UI', 'PushBackRichPreviewTests', 9, False, 'fixed'), ('UI', 'PushBackLibrarySaveGateTests', 5, False, 'fixed'),
    ('UI', 'DialogWindowCharacterizationTests', 20, False, 'fixed'),  # v2.4, V24-05 (CA-T2b M-02): los ejecutables
    ('UI', 'BlankFront', 33, False, 'fixed'), ('UI', 'WindowCensusGuardTests', 8, False, 'fixed'), ('UI', 'RichEditor', 25, False, 'fixed'),
    ('UI', 'Workspace', 547, False, 'fixed'), ('UI', 'Cantilever', 67, False, 'fixed'),
]
if REQ_PKG_ROWS is not None:
    _pkg_req = package_required(REQ_PKG_ROWS)
    assert [(p, f, m, e, k == 'new') for p, f, m, e, k in REQ_SPEC] == _pkg_req, ('la tabla de RequiredTests del section 6.2 cambio', _pkg_req)
assert len(REQ_SPEC) == 38 and [r[1] for r in REQ_SPEC if r[3]] == ['PushBackWorkspaceModeTests', 'PushBackWorkspaceApplyCloseTests', 'PushBackWorkspaceModeSourceGuardTests']
assert sorted(os.path.basename(p)[:-3] for p in NEW_UI[1:] + NEW_CORE) == sorted(r[1] for r in REQ_SPEC if r[4] == 'new')
# ningun filtro sin ExpectRed contiene una clase con ExpectRed (FullyQualifiedName~ es "contiene"): la razon de los filtros por clase exacta
for p, f, _, er, _ in REQ_SPEC:
    for p2, f2, _, er2, _ in REQ_SPEC:
        if er2 and not er and p == p2:
            ns = 'RackCad.UI.Tests.' if p == 'UI' else 'RackCad.Tests.'
            assert (ns + f) not in (ns + f2 + '.'), ('un filtro sin ExpectRed contiene una clase con ExpectRed', f, f2)
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
    root = UT if project == 'UI' else TR
    ns_prefix = 'RackCad.UI.Tests' if project == 'UI' else 'RackCad.Tests'
    files = [p for p in tracked(root) if p.endswith('.cs') and '/bin/' not in p and '/obj/' not in p]
    totals = {}
    for path, data in blobs(files).items():
        text = data.decode('utf-8-sig').replace('\r\n', '\n').replace('\r', '\n')
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
                e = totals.setdefault(key, [0, 0, 0])
                if n < 0:
                    e[2] += 1
                elif skip:
                    e[1] += n
                else:
                    e[0] += n
                pending = None
    return ns_prefix, totals


COUNTS = {}
for proj in ('UI', 'Core'):
    ns_prefix, totals = count_static(proj)
    for p, f, _, _, _ in REQ_SPEC:
        if p == proj:
            sel = {k: v for k, v in totals.items() if k.startswith(ns_prefix + '.' + f)}
            COUNTS[(p, f)] = (len(sel), sum(v[0] for v in sel.values()), sum(v[1] for v in sel.values()), sum(v[2] for v in sel.values()))
# los recuentos de 0dc623ca (clases, ejecutables, Skip, MemberData) de los filtros que existen; los de PB-T2a, los que deja su cierre
COUNTS_BASE = {
    ('Core', 'PushBackWindowOnStateSourceGuardTests'): (1, 1, 0, 0), ('Core', 'PushBackWindowSharedReferenceGuardTests'): (1, 1, 0, 0),
    ('Core', 'PushBackSharedReferenceGuardTests'): (1, 2, 0, 0), ('Core', 'PushBackEditableState'): (3, 13, 0, 0), ('Core', 'PushBackEditorOutputGate'): (1, 1, 0, 0),
    ('Core', 'PushBackClassicCharacterization'): (1, 7, 0, 1), ('Core', 'PushBackFullEditorCharacterization'): (3, 3, 0, 0),
    ('Core', 'PushBackModuleEditorCharacterizationTests'): (1, 15, 0, 0), ('Core', 'I55DeferredProductFixCharacterizationTests'): (1, 3, 0, 0),
    ('Core', 'NamespaceFolderGuardTests'): (1, 7, 0, 0), ('Core', 'Workspace'): (74, 3271, 0, 33),
    ('UI', 'PushBackWindowOnStateTests'): (1, 2, 0, 0), ('UI', 'PushBackEditableStateParityTests'): (1, 1, 0, 0), ('UI', 'PushBackStateNet'): (6, 14, 0, 0),
    ('UI', 'PushBackClassicCharacterization'): (6, 55, 0, 0), ('UI', 'PushBackFullEditorCharacterization'): (6, 16, 2, 0),
    ('UI', 'PushBackDeclaredCorrection'): (7, 14, 0, 0), ('UI', 'PushBackCompositeWindowTests'): (1, 52, 0, 0),
    ('UI', 'PushBackCompositeLifecycleWiringTests'): (1, 7, 0, 0), ('UI', 'PushBackCompositeLoadExistingModulesTests'): (1, 3, 0, 0),
    ('UI', 'PushBackBothModeCellBeamOverrideTests'): (1, 10, 0, 0), ('UI', 'PushBackModuleEditorWindowTests'): (1, 13, 0, 0),
    ('UI', 'PushBackAdvancedRackParametersWindowTests'): (1, 9, 0, 0), ('UI', 'PushBackRichPreviewTests'): (1, 9, 0, 0),
    ('UI', 'PushBackLibrarySaveGateTests'): (1, 5, 0, 0), ('UI', 'DialogWindowCharacterizationTests'): (1, 20, 3, 0), ('UI', 'BlankFront'): (3, 33, 0, 0),
    ('UI', 'WindowCensusGuardTests'): (1, 8, 0, 0), ('UI', 'RichEditor'): (4, 25, 4, 0), ('UI', 'Workspace'): (20, 547, 0, 16), ('UI', 'Cantilever'): (3, 67, 0, 0),
}
COUNTS_T2A = {('Core', 'PushBackDraftStatePortTests'): (1, 3, 0, 0), ('Core', 'PushBackDraftPortSharedReferenceGuardTests'): (1, 1, 0, 0)}
assert sorted(COUNTS_BASE) == sorted((p, f) for p, f, _, _, k in REQ_SPEC if k == 'fixed') and sorted(COUNTS_T2A) == sorted((p, f) for p, f, _, _, k in REQ_SPEC if k == 'pt2a')
COUNTS_CHANGED = ['%s %s %s -> %s' % (p, f, COUNTS_BASE[(p, f)], COUNTS[(p, f)]) for (p, f) in COUNTS_BASE if COUNTS[(p, f)] != COUNTS_BASE[(p, f)]]
soft(not COUNTS_CHANGED, 'el recuento de los RequiredTests cambio desde 0dc623ca (la sesion lo explica y pone al dia COUNTS_BASE): ' + '; '.join(COUNTS_CHANGED))
_prev_req = {}
for prev in (pbt1c, pbt2a):
    _prev_req.update({(r['Project'], r['Filter']): r['MinSelected'] for r in prev['RequiredTests']})
REQ, REQ_REPORT = [], []
for p, f, pkg_min, er, kind in REQ_SPEC:
    proj = UIP if p == 'UI' else CP
    filt = 'FullyQualifiedName~%s.%s' % ('RackCad.UI.Tests' if p == 'UI' else 'RackCad.Tests', f)
    classes, run, skip, member = COUNTS[(p, f)]
    note = None
    if kind == 'new':
        assert classes == 0 and run == 0, ('la clase nueva ya existe', f)
    elif kind == 'pt2a':
        if soft(classes > 0, 'la clase de PB-T2a %s aun no esta en la revision: su MinSelected (%d) se comprueba sobre el cierre de PB-T2a' % (f, pkg_min)):
            assert COUNTS[(p, f)] == COUNTS_T2A[(p, f)] and run >= pkg_min, ('la clase de PB-T2a', f, COUNTS[(p, f)])
    else:
        # v2.4, V24-05 (CA-T2b M-02): cada minimo, frente a los ejecutables; asi vale con las dos lecturas de "seleccionados" (con o sin los Skip)
        assert classes > 0, ('el filtro no selecciona ninguna clase en la revision', p, f)
        if run < pkg_min:
            assert member > 0 and _prev_req.get((proj, filt)) == pkg_min, (
                'el recuento de %s %s (%d ejecutables, %d Skip, %d MemberData) es menor que el minimo del paquete (%d)' % (p, f, run, skip, member, pkg_min))
            note = '     estatico %d (+%d Theory con MemberData) < %d: vale porque un contrato anterior fija el mismo filtro con %d' % (run, member, pkg_min, pkg_min)
        elif skip:
            note = '     ejecutables %d >= %d; los %d Skip no cuentan para el minimo' % (run, pkg_min, skip)
    REQ.append((proj, filt, pkg_min, er))
    REQ_REPORT.append('%-4s %-45s %-5s paquete=%-5s medido: clases=%d ejecutables=%d skip=%d memberdata=%d -> MinSelected=%d ExpectRed=%s'
                      % (p, f, kind, pkg_min, classes, run, skip, member, pkg_min, er))
    if note:
        REQ_REPORT.append(note)
for proj, filt, mins, er in REQ:
    assert in_head(proj), proj
    if (proj, filt) in _prev_req:
        assert _prev_req[(proj, filt)] <= mins, (filt, _prev_req[(proj, filt)], mins)


# ---------------------------------------------------------------- invariantes
def inv_text(idd, title, body):
    return 'INV-PBT2B-%s%s: %s' % (idd, (' (%s)' % lc(title)) if title else '', body)


_order = ['01', '02', '03', '04', '05', '06', '07', '07c', '08', '08b', '09', '10', '11', '12', '14']
_by = {x[0]: x for x in INV_OWN + INV_COMMON}
assert sorted(_by) == sorted(_order)
INV = [inv_text(*_by[k]) for k in _order]
assert len(INV) == 15 and len({i.split(' ')[0].rstrip(':') for i in INV}) == 15 and not [i for i in INV if 'Nota' in i]

# ---------------------------------------------------------------- la evidencia
_r, _s8, _core, _ui, _skip = CI_BASE_T1C
assert PT1C_GREEN.startswith(_s8) and (_core + 4, _ui, _skip) == CI_T2A_EXPECTED  # la prevision del section 6.2: el GREEN de PB-T1c mas PB-T2a
assert ('GREEN `%s`: 4/4, con Core 19 088 y UI 2 439' % _r) in nspace(show(DISP)), 'la CI del GREEN de PB-T1c en F4B-E31'


# ---- ci_base_check (inicio)
def ci_base_check(git, ci, close, expected, explained):
    """La linea base de la CI del cierre de PB-T2a, atada a PT2A_CLOSE (como CA-T2a M-1 en PB-T2a). Devuelve los errores (vacia si vale)."""
    if not close:
        return ['CI_BASE_T2A sin PT2A_CLOSE: la linea base se ata al cierre de PB-T2a']
    if not (isinstance(ci, tuple) and len(ci) == 5 and re.fullmatch(r'\d+', str(ci[0])) and re.fullmatch(r'[0-9a-f]{8,40}', str(ci[1]))
            and all(isinstance(x, int) for x in ci[2:])):
        return ['CI_BASE_T2A mal formada: %r' % (ci,)]
    run, sha, core, ui, skip = ci
    if git('cat-file', '-e', sha + '^{commit}').returncode != 0:
        return ['el SHA de la CI no esta en el almacen de objetos: ' + sha]
    errors = []
    if git('diff', '--quiet', sha, close, '--', 'src', 'tests').returncode != 0:
        errors.append('el SHA de la CI (%s) no tiene el src y los tests del cierre de PB-T2a (%s)' % (sha[:8], close[:8]))
    if (core, ui, skip) != expected and not explained:
        errors.append('las cifras de la CI del cierre de PB-T2a (Core %d, UI %d, Skipped %d) difieren de las previstas (Core %d, UI %d, Skipped %d): '
                      'la sesion las explica por el diff de PB-T2a en CI_T2A_EXPLAINED' % ((core, ui, skip) + expected))
    return errors
# ---- ci_base_check (fin)


soft(CI_BASE_T2A, 'falta CI_BASE_T2A (la CI del cierre de PB-T2a): la ExpectedEvidence lleva la linea base prevista del section 6.2')
if CI_BASE_T2A:
    _err = ci_base_check(git, CI_BASE_T2A, PT2A_CLOSE, CI_T2A_EXPECTED, CI_T2A_EXPLAINED)
    assert not _err, _err
    _run, _sha, _cc, _uu, _ss = CI_BASE_T2A
    CI_TXT = ('la linea base del cierre de PB-T2a (run %s sobre %s: Core %d Passed; UI %d resultados, %d Skipped y %d Passed): Core + 2 (CT-PBT2B-08 y 09) = '
              '%d Passed; UI + 8 (CT-PBT2B-01 a 07 y 10) = %d resultados: 0 Failed, %d Skipped y %d Passed, o la diferencia explicada por el diff'
              % (_run, _sha[:8], _cc, _uu, _ss, _uu - _ss, _cc + 2, _uu + 8, _ss, _uu + 8 - _ss))
    if (_cc, _uu, _ss) != CI_T2A_EXPECTED:
        assert all(ord(ch) < 0x80 for ch in CI_T2A_EXPLAINED)
        CI_TXT += ' (frente a la prevista del section 6.2, Core %d y UI %d con %d Skipped: %s)' % (CI_T2A_EXPECTED + (CI_T2A_EXPLAINED,))
else:
    _c2, _u2, _s2 = CI_T2A_EXPECTED
    CI_TXT = ('la linea base prevista del cierre de PB-T2a (section 6.2 del paquete: el GREEN de PB-T1c medido, run %s sobre %s, Core %d Passed y UI %d '
              'resultados con %d Skipped, mas PB-T2a, Core + 4): Core + 2 (CT-PBT2B-08 y 09) = %d Passed; UI + 8 (CT-PBT2B-01 a 07 y 10) = %d resultados: 0 '
              'Failed, %d Skipped y %d Passed, o la diferencia explicada por el diff' % (_r, _s8, _core, _ui, _skip, _c2 + 2, _u2 + 8, _s2, _u2 + 8 - _s2))
assert (CI_T2A_EXPECTED[0] + 2, CI_T2A_EXPECTED[1] + 8, CI_T2A_EXPECTED[1] + 8 - CI_T2A_EXPECTED[2]) == (19094, 2447, 2426)  # LB_PKG; Q-PBT2-3 es la A


def cap(x):
    return x[0].upper() + x[1:]


def dot(x):
    return x[:-1] + '.' if x.endswith(';') else x


EVID = [
    cap(dot(E_ANTES)),
    CI_RED_HEAD + ' ' + dot(CI_RED_T2B) + ' ' + CI_RR,
    cap(dot(E_RED)),
    RED_CHAIN_HEAD + ' ' + dot(RED_CHAIN_T2B),
    cap(E_GREEN[:-1]) + ', con los conteos de ' + CI_TXT + '.',
    cap(dot(E_LOCAL)),
    cap(dot(E_ENTREGA)),
    cap(E_DESPUES),
    RR_VERIF_EVID + ' '.join([RR_V_N10, RR_V_N11, RR_V_CTRL, RR_V_AYUDA_T2B, RR_V_INDICIOS]),  # v2.8, V28-01 y V28-02
    'De PB-T2b (section 6.2 del paquete): ' + ' '.join(E_T2B),
]
assert 'build-plugin sale omitido por su needs' in EVID[1] and 'que es el trailer real de los commits RED, "RED repair" (si lo hay) y GREEN' in EVID[6]

# ---------------------------------------------------------------- paradas
T2A_STOP = {s['Id']: s['Text'] for s in pbt2a['StopConditions']}


def prefixed(text):
    """El texto de PB-T2a, cambiando solo el prefijo (section 6.0; C11 ya trae la referencia de V22-01)."""
    return text.replace('PBT2A', 'PBT2B').replace('PB-T2a', 'PB-T2b')


C08, C11, C12 = prefixed(T2A_STOP['PBT2A-C08']), prefixed(T2A_STOP['PBT2A-C11']), prefixed(T2A_STOP['PBT2A-C12'])
assert C08 == T2A_STOP['PBT2A-C08'] and C12 == T2A_STOP['PBT2A-C12'] and C11.endswith('neither one-off acceptance extends to PB-T2b')
assert ('in ' + C11_REF + ';') in C11 and 'PBT2B-NC2-EXACT-WHITELIST-01' in C11 and 'PBT2A' not in C11
C17 = T2A_STOP['PBT2A-C17']
assert 'The lifetime includes that of the WORKSPACE window (section 6.0 of the package).' in C17 and 'PB-T2a' not in C17
C02 = T2A_STOP['PBT2A-C02'].replace('PB-T1a, PB-T1b or PB-T1c test', 'PB-T1a, PB-T1b, PB-T1c or PB-T2a test')
assert 'PB-T2a test' in C02
C09 = T2A_STOP['PBT2A-C09'] + ' (C09 governs the new tests; the configurator layout written by existing tests is attributed as INV-PBT2B-10 says; v2.3, V23-01)'
STOPS = P_STOPS + [
    {'Id': 'PBT2B-C01', 'Text': 'doing it right would require writing outside the AllowedWriteScope (W, the new mode file PushBackEditorMode.cs, the four new UI '
                                'test files PushBackWorkspaceSupport, PushBackWorkspaceModeTests, PushBackWorkspaceApplyCloseTests and '
                                'PushBackFieldValidationConcordanceTests, and the two new Core test classes PushBackWorkspaceModeSourceGuardTests and '
                                'PushBackWorkspaceSharedReferenceGuardTests)'},
    {'Id': 'PBT2B-C02', 'Text': C02},
    {'Id': 'PBT2B-C03', 'Text': 'an observable change of RACKEDITAR, RACKPUSHBACK (RPB) or the menu against the base appears (in PB-T2b CLASSIC does not change: '
                                'the current constructors, the four sites, WX, WC, Application and the Plugin do not change, and the WORKSPACE mode is reached '
                                'only through ForDraft): STOP and back to the Owner (D-16, OB-RE.1)'},
    {'Id': 'PBT2B-C04', 'Text': C04_PKG},
    {'Id': 'PBT2B-C05', 'Text': T2A_STOP['PBT2A-C05']},
    {'Id': 'PBT2B-C06', 'Text': T2A_STOP['PBT2A-C06']},
    {'Id': 'PBT2B-C07', 'Text': T2A_STOP['PBT2A-C07'] + '; ' + C07_RR},
    {'Id': 'PBT2B-C08', 'Text': C08},
    {'Id': 'PBT2B-C09', 'Text': C09},
    {'Id': 'PBT2B-C10', 'Text': T2A_STOP['PBT2A-C10']},
    {'Id': 'PBT2B-C11', 'Text': C11},
    {'Id': 'PBT2B-C12', 'Text': C12},
    {'Id': 'PBT2B-C13', 'Text': C13_PKG},
    {'Id': 'PBT2B-C14', 'Text': C14_PKG},
    {'Id': 'PBT2B-C15', 'Text': C15_PKG},
    {'Id': 'PBT2B-C16', 'Text': C16_PKG},
    {'Id': 'PBT2B-C17', 'Text': C17},
    {'Id': 'PBT2B-C18', 'Text': C18_RR},
]
assert [s['Id'] for s in STOPS[17:]] == ['PBT2B-C%02d' % n for n in range(1, 19)] and len(STOPS) == 35
for s in STOPS[17:]:
    assert not re.search(r'PBT2A-|PBT1C-|In PB-T1c', s['Text']), s['Id']

# ---------------------------------------------------------------- autoridades
D2_ = PILOT + '/F4B-DRAFTS/pbt2/'
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
PT1 = 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md'
PV5 = 'docs/initiatives/I-64-proposal-v5.md'
T1BPKG = 'docs/initiatives/I-64-f4-t1b-gate-contract-package.md'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
F4B_E25 = CO + 'owner-f4b-correction-unit-change-decision.md'
BOUNDED_CHECK = D2_ + 'independent-review-pbt2b-v23-check.md'
assert title_version(lines_of(PT1)) == 'v2.8'
_ph, _pv = lines_of(PHASE), lines_of(PV5)
assert _ph[476].startswith('## 4. Los dos modos') and _ph[503].startswith('| **PB-T2 ') and _pv[556].startswith('### D-15 ') and 'Sin cambio observable' in _pv[585]
_close_txt = (' (' + PT2A_ENTRY + ')') if PT2A_ENTRY else ' (por rellenar)'
CUSTODY = [  # las que se custodian con la emision de PB-T2b y son autoridades
    (PKG, 'archivo completo (' + PKG_VERSION + '): los registros de cambios v2.7 -> v2.8 (V28-01 a V28-06: las recomprobaciones de la v2.7, la verificacion con una reparacion), v2.6 -> v2.7 (V27-01 a V27-07: F4B-E35, la reparacion del RED), '
          'v2.5 -> v2.6 (V26-01 a V26-08: CA-T2c y su section 9, que solo cambian PB-T2c), v2.4 -> v2.5 (V25-01 a V25-04: la recomprobacion de la seccion 10 de '
          'CA-T2b, M-08 a M-11), v2.3 -> v2.4 (V24-01 a V24-16: CA-T2b, el momento de cada lectura y los gestos de la red, section 5.0), v2.2 -> v2.3 (V23-01 a V23-07: M-6 de CA-T2a, las anclas [T1c] remedidas sobre 0dc623ca, los rangos de '
          'INV-PBT2B-03 y INV-PBT2B-08b), v2.1 -> v2.2, v2 -> v2.1 y v1 -> v2; section 1 (autoridades), section 2 (hechos), section 3 (D-PBT2-04 a '
          'D-PBT2-12: la ventana en dos modos), section 4 (las guardas), section 5.0 y 5.3 (el regimen, las recetas, el momento de cada lectura, los '
          'gestos, la costura inerte y CT-PBT2B-01 a 10), section 6.0 y 6.2 (este contrato), section 7 (R-PBT2-22), section 8 (las decisiones del '
          'Owner) y section 10 (arrastres)'),
    (BOUNDED_CHECK, 'la comprobacion acotada de PB-T2b (CA-T2b; section 6.0 del paquete: sobre el paquete, la section 6.2 y la salida de sus '
                    'anclajes) y su recomprobacion sobre la v2.4 (su seccion 10): ' + (BOUNDED_RESULT or 'resultado por rellenar')),
]
# el encargo literal de CA-T2b y de su seccion 10 va junto al informe (v2.5, V25-04; CA-T2b section 10, M-11)
CUSTODY_EXTRA = [D2_ + n for n in ('v23-delta.md', 'v24-delta.md', 'v25-delta.md', 'v26-delta.md', 'v27-delta.md', 'v28-delta.md', 'measure_pbt2b_t1c.py',
                                   'pbt2b-anchors-out.txt',
                                   'independent-review-pbt2b-v23-check-prompt.md', 'make_pbt2b_contract-dry.txt')]
EXISTING = [
    (MEAS, 'el registro de mediciones del paquete, apartados A a Q; las anclas [T1c] de PB-T2b, remedidas en la v2.3 sobre 0dc623ca (V23-03, v23-delta.md)'),
    (D2_ + 'v2-delta.md', 'de la v1 a la v2 del paquete, hallazgo por hallazgo (IR1)'),
    (D2_ + 'independent-review-v1.md', 'la revision independiente de la v1 (IR1: NECESITA v2, 0/8/16) y su recomprobacion de la v2 (IR1-R, section 10: LISTO '
                                       'PARA EMITIR TRAS REMEDICION, 0/0/7), aplicada en la v2.1'),
    (D2_ + 'independent-review-pbt2a-v21-check.md', 'la comprobacion acotada de PB-T2a (CA-T2a: EMITIBLE TRAS CORRECCIONES 0/0/6, aplicadas): su M-6, '
                                                    'aplicado en la v2.3 (V23-01 y V23-02)'),
    (PBT2AC, 'F4B-E32, el contrato emitido de PB-T2a: el puerto IPushBackDraftStatePort y el lanzamiento PushBackDraftEditorLaunch que usa la ventana; sus '
             'listas de tests/, la raiz, las paradas estandar, C08, C11, C12, C17 y las celdas'),
    (PB2A_TCP, 'el cierre de PB-T2a' + _close_txt + ': ' + S1['CLOSES'][1] + '; el puerto implementado sobre el que se escribe la ventana'),
    (PT2A_STOP_ANALYSIS, PT2A_STOP_ENTRY + ', la parada del intento 1 de PB-T2a (PBT2A-C07): C-1, el momento de una lectura (estado-es-el-actual '
                         'antes de aplicar), que la v2.4 aplica a CT-PBT2A-02 y a las recetas de PB-T2b (section 5.0, El momento de cada lectura; '
                         'V24-06 y V24-10)'),
    (PHASE, '[P], v3.1: ' + S1['P'][1]),
    (PT1, '[PT1], v2.8 (con el que se emitio PB-T1c): ' + S1['PT1'][1]),
    (CO + 'owner-f4b-disposition.md', '[OD]: ' + S1['OD'][1].replace('(literal arriba)', '(literal: ' + OD_ROW + ')')),
    (CO + 'owner-f4b-procedure-decisions.md', '[DP], F4B-E03: ' + S1['DP'][1]),
    (CO + 'owner-pbt1-pre-decisions.md', 'F4B-E09 (con F4B-E06, en f4b-dispositions.md): ' + S1['E06-09'][1]),
    (CO + 'owner-d1-d2-decisions.md', '[OD2], F4B-E11: ' + S1['OD2'][1]),
    (PILOT + '/F4B-DRAFTS/pbt1/owner-return-l1.md', 'la vuelta L-1 al Owner, respondida por F4B-E11 (D2)'),
    (CO + 'owner-config-snapshot-j-decision.md', 'F4B-E19: ' + S1['E19-22'][1].split('; ')[0]),
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-config.md', 'la vuelta al Owner por el config.toml tras el reinicio, respondida por F4B-E19 (la Snapshot J)'),
    (CO + 'owner-f4b-opaque-collections-decision.md', 'F4B-E21: ' + S1['E19-22'][1].split('; ')[1]),
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-planning2.md', 'la vuelta al Owner por la salida truncada del Controller, respondida por F4B-E21'),
    (CO + 'owner-pbt1b-rework-decisions.md', 'F4B-E22: ' + S1['E19-22'][1].split('; ')[2]),
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-rework.md', 'la vuelta al Owner tras la revision independiente del intento 1 de PB-T1b (M10 y M14), respondida por F4B-E22'),
    (CO + 'owner-f4b-dpb07-extensiondata-decisions.md', 'pregunta 1 (F4B-E23): ' + S1['E23'][1] + ': en PB-T2b, las dos costuras BomDialog y LibraryPathDialog, '
                                                        'S-7, S-9 y CT-22 en verde y la UI-STA de CT-PBT2B-03 (D-PBT2-10); pregunta 2 (F4B-E24): ' + S1['E24'][1]),
    (PILOT + '/F4B-DRAFTS/pbfull/owner-return-dpb07.md', 'la vuelta al Owner por D-PB-07, respondida por F4B-E23'),
    (PILOT + '/F4B-DRAFTS/pbfull/owner-return-extensiondata.md', 'la vuelta al Owner por la ExtensionData anidada, respondida por F4B-E24'),
    (F4B_E25, 'F4B-E25: ' + S1['E25'][1] + '. Las tres condiciones las comprueba el Controller, y no hace falta reemitir el contrato para corregir '
              '(section 6.0, Las correcciones). W es UNIT_CHANGE en este contrato (section 6.0)'),
    (F4B_E35, F4B_E35_ENTRY + ', pregunta 4: ' + RR_S1[1] + '. El contrato la incorpora en PBT2B-C07 (la excepcion), PBT2B-C18, INV-PBT2B-02 (la '
              'cadena RR-6) y la ExpectedEvidence (la CI del RED reparado y lo que declara la entrega) (section 6.0 del paquete, v2.7, V27-01 a '
              'V27-06). ' + RR_VERIF_AUTH),
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-verify-stop.md', 'la vuelta al Owner por la parada de la primera verificacion del intento 2 de PB-T1b (16.3), respondida por F4B-E25'),
    (OWNER_PBT2, 'archivo completo (' + OWNER_PBT2_ENTRY + '): Q-PBT2-1 a Q-PBT2-4, las cuatro "(Recomendado)": la pregunta 1, "Aplica sin cerrar; Cerrar '
                 'avisa" (D-PBT2-06 y D-PBT2-07; la excepcion a ADR-0029 D8, solo en WORKSPACE); la 2, "Lo rechaza, como Actualizar" (la misma compuerta en los '
                 'dos modos, D-PBT2-06 y D-PBT2-09); la 3, "En PB-T4, con prueba ya" (CT-PBT2B-10); y la 4, "Si, con esa condicion", la autorizacion '
                 'condicional de emision de PB-T2a, PB-T2b y PB-T2c'),
    (DISP, 'archivo completo: F4B-E01 a ' + DISP_LAST),
    (PILOT + '/F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md', 'el cierre de PB-T1b, F4B-E26: ' + S1['E26'][1] + '; ' + lc(S1['CLOSES'][1])),
    (PB1C_TCP, 'el cierre de PB-T1c (' + PT1C_CLOSE_ENTRY + ', sobre 161a0dad): ' + S1['CLOSES'][1] + '; la ventana sobre la que se escribe PB-T2b'),
    (PBT1CC, 'F4B-E27, el contrato emitido de PB-T1c: ' + S1['E27'][1]),
    (PV5, 'la Proposal V5: D-15 (:557-582): ' + S1['PV5D15'][1] + '; D-16 e INV-19 (:586-593): ' + S1['PV5D16'][1]),
    (T1BPKG, 'T1B: ' + S1['T1B'][1]),
    (PILOT + '/F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md', 'el cierre de PB-TC: ' + S1['CLOSES'][1]),
    (PILOT + '/F4B-PBTC2-DECLARED-CORRECTIONS/task-close-package.md', 'el cierre de PB-TC2: ' + S1['CLOSES'][1]),
    (PILOT + '/F4B-PBT1A-LOSSLESS-STATE-NET/task-close-package.md', 'el cierre de PB-T1a: ' + S1['CLOSES'][1]),
]
UNIT_CHANGE = [
    (W, 'la ventana de Push Back: los miembros de INV-PBT2B-03 (el modo WORKSPACE, D-PBT2-04 a D-PBT2-10; section 6.0); en el RED, solo la costura inerte '
        'del section 5.3. En una correccion tras un GREEN, su presencia en git diff --name-only AuthorityRevision BaseSha se lee con la regla de F4B-E25'),
]
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D7, D8, D12 y D13: ' + S1['ADR'][1]),
    ('docs/AUTOMATION_PLAN.md', 'section 16 (16.3 a 16.11; 16.3 en una correccion, con F4B-E25): ' + S1['PROTO'][1]),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4): ' + S1['PROTO'][1]),
    ('docs/automation/agent-execution/routing.md', 'archivo completo: ' + S1['PROTO'][1]),
    ('docs/automation/agent-execution/model-catalog.md', 'las celdas medidas y vigentes (:14, :51 y :97: vencen el ' + CELLS_EXPIRY + '): ' + S1['PROTO'][1]),
]
assert len(CUSTODY) == 2 and len(CUSTODY_EXTRA) == 10 and len(EXISTING) == 36 and len(UNIT_CHANGE) == 1 and len(EXTERNAL) == 5
assert len({p for p, _ in CUSTODY + EXISTING + UNIT_CHANGE + EXTERNAL}) == len(CUSTODY + EXISTING + UNIT_CHANGE + EXTERNAL)
for p, _ in EXISTING + EXTERNAL + UNIT_CHANGE:
    if p not in (PB2A_TCP, F4B_E35):
        assert in_head(p), ('autoridad inexistente en la revision', p)
soft(in_head(PB2A_TCP), 'el cierre de PB-T2a (autoridad) aun no esta en la revision: ' + PB2A_TCP)
if soft(in_head(F4B_E35), 'F4B-E35, la regla RR (autoridad; de la custodia del cierre de PB-T2a) aun no esta en la revision: ' + F4B_E35):
    _err = rr_check(show(F4B_E35))
    assert not _err, ('F4B-E35: la regla RR', _err)
if in_head(D2_ + 'independent-review-pbt2a-v21-check.md'):
    assert 'EMITIBLE TRAS CORRECCIONES' in show(D2_ + 'independent-review-pbt2a-v21-check.md') and '| M-6 | MENOR |' in show(D2_ + 'independent-review-pbt2a-v21-check.md')
_close = (' (' + PT2A_ENTRY + ')') if PT2A_ENTRY else ' (<cierre de PB-T2a por rellenar>)'
c = {
    'Schema': 'rackcad-gate-contract/v1', 'Unit': 'I-64', 'Gate': 'F4B', 'TaskId': TASK,
    'Objective': OBJECTIVE,
    'AuthorityRevision': AR, 'MainSha': MAIN,
    'Authorities': [{'Path': p, 'Section': s, 'Class': 'UNIT_DOC'} for p, s in CUSTODY + EXISTING] + [
        {'Path': CONTRACT_REL, 'Section': 'este contrato', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_CHANGE'} for p, s in UNIT_CHANGE] + [{'Path': p, 'Section': s, 'Class': 'EXTERNAL'} for p, s in EXTERNAL],
    'AllowedWriteScope': ALLOWED,
    'ForbiddenWriteScope': FORBIDDEN,
    'Invariants': INV,
    'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
    'ExpectedEvidence': EVID, 'StopConditions': STOPS, 'EligibleCells': cells, 'RoutingEnforcement': 'required', 'CorrectionsAuthorized': True,
    'IssuedBy': ('Sesion principal de I-64 (Claude) por la decision del Owner ' + OWNER_PBT2_ENTRY + ', pregunta 4, la emision condicional de PB-T2a, PB-T2b '
                 'y PB-T2c (0 bloqueantes y 0 importantes sin resolver en su comprobacion acotada, y la remedicion sobre el cierre de la subtarea anterior '
                 'sin cambio material), con las preguntas 1 a 3, que fijan PB-T2b (docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/'
                 'owner-pbt2-decisions.md): la emision de la segunda tarea de PB-T2 tras el cierre de PB-T2a' + _close + ', sus anclajes y su comprobacion '
                 'acotada; F4B-E23, BOM y biblioteca en WORKSPACE (owner-f4b-dpb07-extensiondata-decisions.md); F4B-E11, D2 (owner-d1-d2-decisions.md); '
                 'las decisiones de procedimiento owner-f4b-procedure-decisions.md (F4B-E03); F4B-E19, la Snapshot J; F4B-E21, las colecciones opacas '
                 'que pone la sesion; F4B-E25, la regla de F4B sobre las rutas UNIT_CHANGE en una correccion; y el paquete del contrato de PB-T2 '
                 + PKG_VERSION + ', con IR1, IR1-R, la comprobacion acotada de PB-T2a y la de PB-T2b; y F4B-E35, la regla de la reparacion del RED '
                 '(RR-1 a RR-7), que el contrato incorpora en PBT2B-C07, PBT2B-C18, INV-PBT2B-02 y la ExpectedEvidence'),
    'IssuedUtc': ISSUED,
}
assert len(c['Authorities']) == 45, len(c['Authorities'])
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
# lo que depende de la emision de PB-T2b
soft(PKG_HEAD_VERSION == PKG_VERSION, 'el paquete custodiado en la revision es %s; al escribir hace falta la %s en HEAD (%s)' % (PKG_HEAD_VERSION, PKG_VERSION, PKG))
soft(BOUNDED_RESULT, 'falta BOUNDED_RESULT (el resultado de la comprobacion acotada de PB-T2b, section 6.0)')
assert BOUNDED_RESULT is None or (BOUNDED_RESULT.startswith('EMITIBLE') and all(ord(ch) < 0x80 for ch in BOUNDED_RESULT)), BOUNDED_RESULT
soft(in_head(BOUNDED_CHECK), 'a custodiar con la emision de PB-T2b, aun no en la revision: %s' % BOUNDED_CHECK)
for p in CUSTODY_EXTRA:
    soft(in_head(p), 'a custodiar con la emision, sin ser autoridad, aun no en la revision: %s' % p)



# ---- bounded_check (inicio)
def bounded_check(report, result, pkg_lines, reviewer='CA-T2b'):
    """La comprobacion acotada custodiada frente a BOUNDED_RESULT y al paquete (F4B-E28, pregunta 4: 0 bloqueantes y 0 importantes SIN RESOLVER).
    Lee el primer veredicto y, si las hay, las recomprobaciones (v2.5, V25-01; CA-T2b seccion 10, M-08): cada una con su encabezado
    '## <seccion>n. Recomprobacion sobre la vX.Y' y su linea '**Veredicto de <seccion>n: ... Nuevos: B n / I n / M n.**'; la ultima tiene que dar
    EMITIBLE con 0 bloqueantes y 0 importantes nuevos, y BOUNDED_RESULT la nombra ('recomprobacion vX.Y: <veredicto>, B n / I n / M n').
    La n de la seccion puede ser un numero o una letra, como R (v2.6, CA-T2c M-07). Un encabezado '## ...Recomprobacion...' o una linea que empieza
    por '**Veredicto de <seccion>' sin su forma, o que no se empareja en orden con el otro, da un error claro en lugar de una excepcion.
    Es la funcion de make_pbt2c_contract.py (CA-T2c M-07), copiada aqui por la v2.8 (V28-05; CA-T2b seccion 11, M-04).
    Devuelve los errores (vacia si vale)."""
    sec = chr(0xA7)
    m = re.search(r'\*\*Veredicto: (EMITIBLE[^.]*)\. B (\d+) / I (\d+) / M (\d+)\.\*\*', report)
    if not m:
        return ['la comprobacion acotada no tiene la linea "**Veredicto: EMITIBLE... B n / I n / M n.**"']
    b, i, mm = int(m.group(2)), int(m.group(3)), int(m.group(4))
    errors = []
    if b:
        errors.append('la comprobacion acotada tiene %d bloqueantes' % b)
    if not (result or '').startswith(m.group(1)) or ('B %d / I %d / M %d' % (b, i, mm)) not in (result or ''):
        errors.append('BOUNDED_RESULT no es el veredicto de la comprobacion acotada: %r frente a %r' % (result, m.group(0)))
    if i:
        ids = ['I-%02d' % k for k in range(1, i + 1)]
        for x in ids:
            if ('| %s | I |' % x) not in report:
                errors.append('la comprobacion acotada no tiene la fila de ' + x)
        if 'corregidas en el paquete' not in (result or ''):
            errors.append('BOUNDED_RESULT no dice que las importantes estan corregidas en el paquete')
        reg = [l for l in pkg_lines if re.match(r'^\| V2\d-\d\d \|', l)]
        for x in ids:
            if not [l for l in reg if ('**%s %s**' % (reviewer, x)) in l]:
                errors.append('ningun registro de cambios del paquete corrige %s de %s' % (x, reviewer))
    # v2.6, CA-T2c M-07: la seccion puede no tener numero ('## <seccion>R. ...'); cada encabezado de recomprobacion y cada linea que empieza por
    # '**Veredicto de <seccion>' tienen que tener su forma, y emparejarse en orden por su identificador; si no, un error claro
    rid = r'([^\s.:*]+)'
    h_rx = r'^## ' + sec + rid + r'\. Recomprobaci' + chr(0xF3) + r'n sobre la (v\d+\.\d+)[^\n]*$'
    v_rx = r'^\*\*Veredicto de ' + sec + rid + r': ((?:NO )?EMITIBLE[^.]*)\. Nuevos: B (\d+) / I (\d+) / M (\d+)\.\*\*\s*$'
    heads_any = re.findall(r'(?m)^## [^\n]*Recomprobaci' + chr(0xF3) + r'n[^\n]*$', report)
    heads = re.findall('(?m)' + h_rx, report)
    verd_any = re.findall(r'(?m)^\*\*Veredicto de ' + sec + r'[^\n]*$', report)
    rechecks = re.findall('(?m)' + v_rx, report)
    if heads_any or verd_any:
        pair = []
        if len(heads) != len(heads_any):
            pair.append('un encabezado de recomprobacion no tiene la forma "## <seccion>n. Recomprobacion sobre la vX.Y" (n puede ser un numero o '
                        'una letra, como R): %s' % ascii([h for h in heads_any if not re.match(h_rx, h)][:1]))
        if len(rechecks) != len(verd_any):
            pair.append('una linea de veredicto de recomprobacion no tiene la forma "**Veredicto de <seccion>n: ... Nuevos: B n / I n / M n.**": %s'
                        % ascii([v for v in verd_any if not re.match(v_rx, v)][:1]))
        if not pair and [h[0] for h in heads] != [r[0] for r in rechecks]:
            pair.append('no se puede emparejar cada encabezado de recomprobacion con su veredicto: encabezados %s, veredictos %s'
                        % ([h[0] for h in heads], [r[0] for r in rechecks]))
        if pair:
            errors.extend(pair)
        else:
            n, verdict, rb, ri, rm = rechecks[-1]
            if int(rb) or int(ri) or not verdict.startswith('EMITIBLE'):
                errors.append('la ultima recomprobacion (seccion %s) da %s con %s bloqueantes y %s importantes nuevos' % (n, verdict, rb, ri))
            want = 'recomprobacion %s: %s, B %s / I %s / M %s' % (heads[-1][1], verdict, rb, ri, rm)
            if want not in (result or ''):
                errors.append('BOUNDED_RESULT no nombra la ultima recomprobacion: falta %r' % want)
    return errors
# ---- bounded_check (fin)


if in_head(BOUNDED_CHECK):
    _err = bounded_check(show(BOUNDED_CHECK), BOUNDED_RESULT, PKG_LINES)
    assert not _err, ('la comprobacion acotada de PB-T2b', _err)
_last = re.split(r'(?m)^## ', _disp)[-1]
EMIT_ENTRY_OK = bool(re.match(r'F4B-E\d+\. Emisi(?:o|' + chr(0xF3) + r')n de F4B-PBT2B-WORKSPACE-MODE-WINDOW', _last)) and OWNER_PBT2_ENTRY in _last and 'pregunta 4' in _last
soft(EMIT_ENTRY_OK, 'la ultima entrada de f4b-dispositions.md (%s) no es la emision de F4B-PBT2B-WORKSPACE-MODE-WINDOW con las condiciones de %s, pregunta 4'
     % (DISP_LAST, OWNER_PBT2_ENTRY))
ORIGIN_MAIN = git('rev-parse', 'origin/main').stdout.decode('ascii').strip()
soft(ORIGIN_MAIN == MAIN, 'MainSha (%s) no es origin/main (%s)' % (MAIN[:8], ORIGIN_MAIN[:8]))
if not DRY:
    assert git('merge-base', '--is-ancestor', AR, REV).returncode == 0, 'la AuthorityRevision no es la revision ni un antepasado suyo'
REMEDIATION = [
    '1 el cierre de PB-T2a: ' + (ANCHOR_T2A if PT2A_CLOSE else 'pendiente (PT2A_CLOSE)') + ', posterior a su parada ' + PT2A_STOP_ENTRY
    + '; el puerto: ' + ('presente, con su API' if PORT_OK else 'pendiente'),
    '2 las anclas [T1c] de W y WC: blobs W %s, WC %s y WX %s (los de 161a0dad); %d lineas de %d citas OK; rangos de INV-PBT2B-03 OK (%d autopruebas); '
    'costuras 41; R3, S-7, S-9, CT-22 y CT-23 OK' % (WINDOW_BLOBS['W'], WINDOW_BLOBS['WC'], WINDOW_BLOBS['WX'], len(WANCHORS), len(CITES), RANGE_SELFTESTS),
    '3 la linea base de la CI: ' + ('atada al cierre de PB-T2a' if CI_BASE_T2A else 'pendiente (CI_BASE_T2A; que sea la corrida push 4/4 lo comprueba la sesion)'),
    '4 el producto: UI, Domain y Plugin == 161a0dad; Application == 161a0dad + PB-T2a; recuentos == 0dc623ca: ' + ('OK' if not COUNTS_CHANGED else 'cambian'),
    '5 la custodia y la entrada de emision: %d AVISOS' % sum(1 for w in WARN if 'custodi' in w or 'entrada de f4b' in w or 'paquete custodiado' in w),
    '6 el entorno: MainSha == origin/main: %s; celdas hasta %s; Test-Json al escribir; config.toml = Snapshot J, el binario y una sola delegacion: la sesion'
    % (ORIGIN_MAIN == MAIN, CELLS_EXPIRY),
    '7 la autorizacion (F4B-E28, pregunta 4): BOUNDED_RESULT = %s; la remedicion sin cambio material y PB-T2a sin intento 3: la sesion' % BOUNDED_RESULT,
]
if DRY:
    print('DRY rev', REV_SHA[:8], '(objetos del checkout principal)' if OBJ_REV else '(HEAD del worktree)', 'allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN),
          '(src', len(SRCF), 'tests', len(TESTF), '[core', len(CORE_T), 'ui', str(len(UI_T)) + '] root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'required', len(REQ), 'evidence', len(EVID), 'authorities', len(c['Authorities']), 'w-anchors', len(WANCHORS), 'test-anchors', len(T_ANCHORS),
          'bytes', len(b), 'sha256', hashlib.sha256(b).hexdigest().upper())
    print('anclaje:', ANCHOR_SRC, '|', ANCHOR_T2A, '| f4b-dispositions hasta', DISP_LAST, '| F4B-E23 y F4B-E28 custodiadas')
    print('paquete leido:', PKG_SRC, '->', PKG_READ_VERSION, '| en la revision:', PKG_HEAD_VERSION)
    print('textos de la %s comprobados:' % PKG_VERSION, ', '.join(PKG_CHECKS) if PKG_CHECKS else 'ninguno')
    print('INV-PBT2B-03 (V23-04): %d rangos comprobados, con %d autopruebas en negativo (cada rango desplazado una linea), todas fallan:' % (len(RANGES), RANGE_SELFTESTS))
    for l in RANGE_REPORT:
        print('  ' + l)
    print('W y WC (V23-03 y v2.4): %d lineas ancla de las %d citas del cuerpo del paquete, iguales; costuras %d; R3 en la base: %s; nombres de PBT2B-C13 libres: %d'
          % (len(WANCHORS), len(CITES), len(SEAMS), ', '.join(n for _, n in _req), len(NEW_T2B)))
    print('datos de las recetas (v2.4): tolerancia de los %d fixtures = %s; pb-01-base se llama %s; CT-PBT1C-02: 49 gestos con Compara y 4 de D2; '
          'F4B-E33 (C-1) custodiada' % (len(FX_TOL), sorted(set(FX_TOL.values())), FX_NAME['pb-01-base']))
    print('V23-01 (CA-T2a M-6 (a)): clases de los filtros de UI de PB-T2b que crean un RackFrameConfiguratorWindow real:', ', '.join(CONFIGURATOR_FILES))
    print('G-L1c: %d autopruebas (%s); huellas AllObjects :278 y Fingerprint :1080' % (len(SELFTESTS), SELFTESTS15))
    print('D1: needs de build-plugin =', _needs, '| D2: Snapshot J:', in_head(SNAP_J), '| celdas hasta', CELLS_EXPIRY)
    print('remedicion al escribir (section 6.0, anclajes de PB-T2b):')
    for l in REMEDIATION:
        print('  ' + l)
    print('RequiredTests (criterio de count_tests_git.py sobre los objetos de la revision; cada minimo frente a los ejecutables, v2.4, V24-05):')
    for l in REQ_REPORT:
        print('  ' + l)
    print('AVISOS (%d):' % len(WARN))
    for w in WARN:
        print('  - ' + w)
    if '--print' in ARGS:
        for i, f in enumerate(FORBIDDEN, 1):
            print(i, f)
    if '--print-json' in ARGS:
        sys.stdout.flush()
        sys.stdout.buffer.write(b)
    sys.exit(0)
assert PT2A_CLOSE and PT2A_ENTRY and CI_BASE_T2A and BOUNDED_RESULT, 'faltan PT2A_CLOSE, CI_BASE_T2A o BOUNDED_RESULT'
assert not WARN, WARN
print('remedicion (section 6.0):\n  ' + '\n  '.join(REMEDIATION))
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
