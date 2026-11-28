"""Contrato de gate de F4B-PBT2A-DRAFT-STATE-PORT (ASCII), desde el paquete del contrato de PB-T2 v2.2
(docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md: section 1, D-PBT2-02, D-PBT2-03, section 5.2, section 6.0 y section 6.1) y el
patron de make_pbt1c_contract.py, con las correcciones de la comprobacion acotada de PB-T2a (CA-T2a, EMITIBLE TRAS CORRECCIONES 0/0/6:
M-1 a M-5 y sus respuestas a D-1 a D-14; la v2.2 lleva M-4) y las comprobaciones de remedicion de su section 3, para cuando escriba.
La BaseSha es hija del cierre de PB-T1c (PT1C_CLOSE, por rellenar), que a su vez desciende de la emision de PB-T1c (F4B-E27) y del cierre de
PB-T1b (F4B-E26).
El ForbiddenWriteScope de src/ sale del algoritmo de [PKC-C] (cover()); los de tests/ y la raiz, de las listas curadas del contrato emitido
de PB-T1c mas sus tres clases nuevas (section 6.0 del paquete).
Comprueba sobre la revision leida (HEAD del worktree o, con --objects-rev, esa revision en el almacen de objetos del checkout principal):
- el anclaje del section 6.0: la revision desciende del cierre de PB-T1b (95373af6, F4B-E26) y de la emision de PB-T1c (833ea6bf, F4B-E27);
  entre el cierre de PB-T1b y la revision, src solo cambia en W y WC (Application, Domain y Plugin, iguales que en los GREEN de PB-T1b);
  con PT1C_CLOSE (constante o --pt1c-close): la revision desciende de el, src es igual al suyo y es de verdad el cierre (su
  f4b-dispositions.md tiene la entrada "Cierre de F4B-PBT1C-LOSSLESS-STATE-WINDOW" y su task-close-package.md);
- las anclas de Application de los registros G, H y Q (PushBackEditableState, su comparador, PushBackEditorOutputGate, el mapa de
  propiedad, RackModuleEditSession, DraftSession y DraftRegistry) y la API que usan el puerto y sus pruebas; G-L1c con sus quince
  autopruebas y las huellas (Q-3);
- V2-05 del section 6.0: que las guardas de PB-T1c (sus tres clases, en la revision) no impiden reasignar editable ni anadir los miembros
  de INV-PBT2B-03, que ningun campo ni asignacion de constructor de W o WC captura una referencia del estado y que ningun miembro se llama
  como los 26 nombres de PBT2B-C13 (CA-T2a M-3; siempre), y que W declara editable como D-PBT1-13 (1) con la regla de D-13 (CA-T2a M-2),
  con lo que cuelga de el calculado (depende de la ventana de PB-T1c: AVISO en seco, error al escribir);
- CA-T2a M-1: la linea base de la CI del cierre de PB-T1c, atada a PT1C_CLOSE (mismo src y tests) y con las cifras previstas, o explicadas;
- el texto del paquete (la v2.2, por --package-file en seco o custodiada en HEAD): el Objective, el AllowedWriteScope, la tabla de
  RequiredTests, los invariantes propios y comunes (literales y SIN su "Nota (no va al contrato)"), las paradas propias (C13 por nombre,
  contrastada con la API de D-PBT2-02 y D-PBT2-03), P-01 con la Snapshot J, la ExpectedEvidence comun (el RED con build-plugin omitido y el
  Trailer real), las autoridades del section 1 y del section 6.0, F4B-E25, la custodia, el esqueleto del section 5.2 y la linea base de la CI;
- los MinSelected, con el criterio de count_tests_git.py (Fact = 1; Theory = sus InlineData; MemberData aparte) sobre los objetos de la
  revision: los del paquete como minimos;
- D1: build-plugin depende de tests en .github/workflows/ci.yml; D2: la Snapshot J en la revision; F4B-E28 (Q-PBT2-1..4) custodiada.
Lo que depende del cierre de PB-T1c (PT1C_CLOSE, CI_BASE_T1C, la ventana de PB-T1c en W, su task-close-package.md y que los recuentos de
RequiredTests sigan los de d8ccbcb8) o de la emision de PB-T2a (el paquete v2.2 y el registro en HEAD, las rutas de CUSTODY y CUSTODY_EXTRA
en HEAD, la entrada de emision de f4b-dispositions.md con las condiciones de F4B-E28, pregunta 4, y MainSha = origin/main) es AVISO en seco y
error al escribir; todo lo demas es error siempre.
Uso: python make_pbt2a_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print] [--print-json]
                                   [--objects-rev <rev> [--repo <checkout principal>] [--package-file <paquete>]] [--pt1c-close <sha>]
Con --dry no escribe nada: comprueba e imprime los recuentos, los bytes y los AVISOS (con --print, ademas, el ForbiddenWriteScope numerado;
con --print-json, el contrato). --objects-rev exige --dry: no hace chdir al worktree y lee todo con git -C <checkout principal> show
<rev>:<ruta>. Sin --objects-rev hace chdir al worktree y lee HEAD. --package-file exige --objects-rev y --dry, y tiene que ser la v2.2.
Al escribir, el paquete es el custodiado en HEAD y tiene que ser la v2.2.
Al custodiarlo, las rutas locales se redactan como en PB-T0 ('<worktree-de-I-64>', '<checkout-principal>')."""
import datetime, hashlib, json, os, re, subprocess, sys, unicodedata

# ---------------------------------------------------------------- el cierre de PB-T1b, la emision de PB-T1c y su cierre (por rellenar)
PB1B_CLOSE = '95373af6116cf120bd0a8ba490833524e33c47da'  # la custodia del cierre de PB-T1b (F4B-E26)
PB1B_CLOSE_ENTRY = 'F4B-E26'
PB1B_GREEN = 'a18ba345eddc5333c22b8bacbad5c2ed4dbf5959'  # el GREEN acreditado de PB-T1b (intento 2)
PB1B_GREEN_A1 = 'a0a5f699b138c1b5251fb0c88d28e3ca9e1610c3'  # el GREEN del intento 1, con el mismo src
CI_BASE_T1B = ('44438060317', 'a18ba345', 19086, 2437, 21)  # la CI del cierre de PB-T1b: (run, sha8, Core Passed, UI total, UI Skipped)
PT1C_EMIT = '833ea6bfaadfab9afc698aba7f12ad2124a78585'  # la emision de PB-T1c (F4B-E27; v2.1, V21-06)
PT1C_EMIT_ENTRY = 'F4B-E27'
PT1C_CLOSE = None        # POR RELLENAR: la custodia del cierre de PB-T1c (aun no existe); tambien --pt1c-close <sha>
PT1C_CLOSE_ENTRY = None  # POR RELLENAR (opcional): la entrada que cierra PB-T1c; con None se busca por el titulo en PT1C_CLOSE
CI_BASE_T1C = ('44463212627', '161a0dad', 19088, 2439, 21)  # la CI del GREEN de PB-T1c (mismo src y tests que el cierre 5a8366c1). Antes, POR RELLENAR: la corrida push de la punta que cierra PB-T1c, 4/4 en success (CA-T2a section 3, paso 3; M-1):
                         # (run, sha de esa punta, Core Passed, UI total, UI Skipped); la punta tiene el src y los tests de PT1C_CLOSE
CI_T1C_DELTA = (2, 2)    # la prevision del section 6.1: PB-T1c suma Core + 2 y UI + 2 (Core 19 088 y UI 2 439, con 21 Skipped)
CI_T1C_EXPLAINED = None  # POR RELLENAR solo si las cifras de CI_BASE_T1C difieren de las previstas: la explicacion de la sesion por el diff de
                         # PB-T1c (no es material, pero cambia la ExpectedEvidence y el GREEN previsto); sin ella, error
BOUNDED_RESULT = 'EMITIBLE TRAS CORRECCIONES 0/0/6, aplicadas'  # la comprobacion acotada de PB-T2a (CA-T2a), literal de la sesion
PKG_VERSION = 'v2.2'     # la version del paquete que se custodia con la emision de PB-T2a (v2.1 mas M-4 de CA-T2a)
MEAS_SECTION = 'Q-7'     # el apartado del registro con la punta 833ea6bf (v2.1, V21-06)
OWNER_PBT2_ENTRY = 'F4B-E28'  # las decisiones del Owner Q-PBT2-1 a Q-PBT2-4
MAIN_CHECKOUT = '<checkout-principal>'  # Redaccion para el repo publico: la ruta local del checkout principal  # solo con --objects-rev; se redacta al custodiar ('<checkout-principal>')

ARGS = sys.argv[1:]
VALUED = ('--objects-rev', '--repo', '--pt1c-close', '--package-file')


def opt(name):
    if name not in ARGS:
        return None
    i = ARGS.index(name)
    assert i + 1 < len(ARGS) and not ARGS[i + 1].startswith('--'), ('falta el valor de', name)
    return ARGS[i + 1]


POS = [a for i, a in enumerate(ARGS) if not a.startswith('--') and not (i > 0 and ARGS[i - 1] in VALUED)]
assert len(POS) == 3, 'uso: make_pbt2a_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print] [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
OBJ_REV = opt('--objects-rev')
PT1C_CLOSE = opt('--pt1c-close') or PT1C_CLOSE
DRY = '--dry' in ARGS
assert OBJ_REV is None or DRY, '--objects-rev solo en seco: anade --dry'
PKG_FILE = opt('--package-file')
assert PKG_FILE is None or (OBJ_REV is not None and DRY), '--package-file solo junto con --objects-rev y --dry'
PKG_FILE = os.path.abspath(PKG_FILE) if PKG_FILE else None  # antes de cualquier chdir
assert PKG_FILE is None or os.path.isfile(PKG_FILE), ('no existe el paquete', PKG_FILE)
assert re.fullmatch(r'[0-9]{4}-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9]Z', ISSUED), ISSUED
assert re.fullmatch(r'[0-9a-f]{40}', AR) and re.fullmatch(r'[0-9a-f]{40}', MAIN), (AR, MAIN)
assert PT1C_CLOSE is None or re.fullmatch(r'[0-9a-f]{40}', PT1C_CLOSE), PT1C_CLOSE

if OBJ_REV is None:
    WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree
    os.chdir(WT)
    REPO, REV = None, 'HEAD'
else:
    REPO, REV = opt('--repo') or MAIN_CHECKOUT, OBJ_REV
DISK = OBJ_REV is None  # solo en el worktree se mira tambien el disco

TASK = 'F4B-PBT2A-DRAFT-STATE-PORT'
assert len(TASK) == 26, len(TASK)  # section 6.0, Identidad (MAX_PATH; v2.1, V21-05)
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'
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


def show(path, rev=None):
    r = git('show', (rev or REV) + ':' + path)
    assert r.returncode == 0, ('no se puede leer', rev or REV, path)
    return r.stdout.decode('utf-8')


def lines_of(path, rev=None):
    return show(path, rev).replace('\r\n', '\n').split('\n')


def tracked(prefix):
    return [p for p in _all()[0] if p.startswith(prefix)]


def soft(cond, msg):
    """Lo que solo existira con el cierre de PB-T1c o con la emision de PB-T2a: error al escribir, AVISO en seco."""
    if cond:
        return True
    assert DRY, msg
    WARN.append(msg)
    return False


def nspace(s):
    return s.replace(chr(0xA0), ' ').replace(chr(0x202F), ' ')


REV_SHA = git('rev-parse', REV).stdout.decode('ascii').strip()
assert re.fullmatch(r'[0-9a-f]{40}', REV_SHA), ('revision desconocida', REV)
assert git('cat-file', '-e', MAIN + '^{commit}').returncode == 0, ('MainSha no esta en el almacen de objetos', MAIN)

# ---------------------------------------------------------------- el anclaje del section 6.0
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
WC = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs'
WX = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
PB1B_TCP = PILOT + '/F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md'
PB1C_TCP = PILOT + '/F4B-PBT1C-LOSSLESS-STATE-WINDOW/task-close-package.md'
PBT1CC = PILOT + '/F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json'
for sha, full in (('95373af6', PB1B_CLOSE), ('a18ba345', PB1B_GREEN), ('a0a5f699', PB1B_GREEN_A1), ('833ea6bf', PT1C_EMIT)):
    assert git('rev-parse', sha).stdout.decode('ascii').strip() == full, ('SHA', sha)
# el cierre de PB-T1b (F4B-E26), como en make_pbt1c_contract.py
assert git('merge-base', '--is-ancestor', PB1B_CLOSE, REV).returncode == 0, 'la revision no desciende del cierre de PB-T1b'
_d = show(DISP, PB1B_CLOSE).replace('\r\n', '\n')
assert ('\n## %s.' % PB1B_CLOSE_ENTRY) in _d and git('cat-file', '-e', PB1B_CLOSE + ':' + PB1B_TCP).returncode == 0, 'PB1B_CLOSE no es el cierre de PB-T1b'
# la emision de PB-T1c (F4B-E27): la revision desciende de ella, que custodia su contrato emitido
assert git('merge-base', '--is-ancestor', PT1C_EMIT, REV).returncode == 0, 'la revision no desciende de la emision de PB-T1c (833ea6bf)'
_d = show(DISP, PT1C_EMIT).replace('\r\n', '\n')
assert ('\n## %s. ' % PT1C_EMIT_ENTRY) in _d and git('cat-file', '-e', PT1C_EMIT + ':' + PBT1CC).returncode == 0, 'PT1C_EMIT no es la emision de PB-T1c'
# PB-T1c solo toca W y WC (su contrato): entre el cierre de PB-T1b y la revision, src no cambia fuera de W y WC
_src = [p for p in git('diff', '--name-only', PB1B_CLOSE, REV, '--', 'src').stdout.decode('utf-8').split('\n') if p]
assert set(_src) <= {W, WC}, ('src cambio fuera de W y WC desde el cierre de PB-T1b', _src)
for green in (PB1B_GREEN, PB1B_GREEN_A1):
    assert git('diff', '--quiet', green, REV, '--', 'src/RackCad.Application', 'src/RackCad.Domain', 'src/RackCad.Plugin').returncode == 0, (
        'Application, Domain o Plugin difieren de un GREEN de PB-T1b', green[:8])
ANCHOR_SRC = 'src == cierre de PB-T1b (%s, %s) salvo %s; Application/Domain/Plugin == a18ba345 == a0a5f699; desciende de %s (%s)' % (
    PB1B_CLOSE[:8], PB1B_CLOSE_ENTRY, ', '.join('W' if p == W else 'WC' for p in sorted(_src)) or 'nada', PT1C_EMIT[:8], PT1C_EMIT_ENTRY)
# el cierre de PB-T1c: "git diff <cierre de PB-T1c> <BaseSha> -- src vacio" (section 6.0, Anclajes de PB-T2a)
PT1C_ENTRY = None
if PT1C_CLOSE:
    assert git('cat-file', '-e', PT1C_CLOSE + '^{commit}').returncode == 0, ('el cierre de PB-T1c no esta en el almacen de objetos', PT1C_CLOSE)
    assert git('merge-base', '--is-ancestor', PT1C_EMIT, PT1C_CLOSE).returncode == 0, 'PT1C_CLOSE no desciende de la emision de PB-T1c'
    assert git('merge-base', '--is-ancestor', PT1C_CLOSE, REV).returncode == 0, 'la revision no desciende del cierre de PB-T1c'
    assert git('diff', '--quiet', PT1C_CLOSE, REV, '--', 'src').returncode == 0, 'src cambio desde el cierre de PB-T1c'
    _r = git('show', PT1C_CLOSE + ':' + DISP)
    _m = re.findall(r'(?m)^## (F4B-E\d+)\. Cierre de F4B-PBT1C-LOSSLESS-STATE-WINDOW\b', _r.stdout.decode('utf-8')) if _r.returncode == 0 else []
    assert len(_m) == 1, ('PT1C_CLOSE no es el cierre de PB-T1c: su f4b-dispositions.md no tiene la entrada de cierre', PT1C_CLOSE)
    PT1C_ENTRY = _m[0]
    assert PT1C_CLOSE_ENTRY in (None, PT1C_ENTRY), ('PT1C_CLOSE_ENTRY', PT1C_CLOSE_ENTRY, PT1C_ENTRY)
    assert git('cat-file', '-e', PT1C_CLOSE + ':' + PB1C_TCP).returncode == 0, ('PT1C_CLOSE no es el cierre de PB-T1c: no tiene su task-close-package.md', PT1C_CLOSE)
    ANCHOR_T1C = 'src == cierre de PB-T1c (%s, %s)' % (PT1C_CLOSE[:8], PT1C_ENTRY)
else:
    ANCHOR_T1C = 'sin el cierre de PB-T1c'
soft(PT1C_CLOSE, 'falta PT1C_CLOSE (el cierre de PB-T1c, la constante o --pt1c-close): no se comprueba "git diff <cierre de PB-T1c> <BaseSha> -- src vacio"')

# ---------------------------------------------------------------- los contratos anteriores
PBT1BC = PILOT + '/F4B-PBT1B-LOSSLESS-STATE-TYPE/coordinator/gate-contract.json'
PBT0C = PILOT + '/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json'
PBTC2C = PILOT + '/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json'
pbt1c = json.loads(show(PBT1CC))
pbt1b = json.loads(show(PBT1BC))
pbt0 = json.loads(show(PBT0C))
assert pbt1c['TaskId'] == 'F4B-PBT1C-LOSSLESS-STATE-WINDOW' and pbt1b['TaskId'] == 'F4B-PBT1B-LOSSLESS-STATE-TYPE'

# Las 17 estandar, del contrato emitido de PB-T1c, con P-01 en la Snapshot J (F4B-E19; section 6.0 del paquete)
SNAP_J = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-J/snapshot-j.json'
P01_J = 'config.toml no longer matches the accepted Snapshot in force (today Snapshot J, %s)' % SNAP_J
P_STOPS = [dict(s) for s in pbt1c['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
STD_IDS = ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
assert [s['Id'] for s in P_STOPS] == STD_IDS and P_STOPS[9]['Text'] == P01_J, P_STOPS[9]
assert [s for s in P_STOPS if s['Id'] != 'P-01'] == [s for s in pbt1b['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id']) and s['Id'] != 'P-01']
assert in_head(SNAP_J), ('la Snapshot J no esta en la revision', SNAP_J)

# ---------------------------------------------------------------- el alcance
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
APP = 'src/RackCad.Application/Systems/PushBack/'
NEW_SRC = [APP + 'PushBackDraftStatePort.cs', APP + 'PushBackDraftEditorLaunch.cs']
NEW_TESTS = [TR + 'PushBackDraftStatePortTests.cs', TR + 'PushBackDraftPortSharedReferenceGuardTests.cs']
ALLOWED = NEW_SRC + NEW_TESTS
assert len(ALLOWED) == 4 == len(set(ALLOWED))
assert all(not in_head(p) and (not DISK or not os.path.exists(p)) for p in ALLOWED), [p for p in ALLOWED if in_head(p)]
# los prefijos y los nombres nuevos estan libres en src/ y tests/ (registro L)
for pref in ('PushBackDraftStatePort', 'PushBackDraftEditorLaunch', 'PushBackDraftPortSharedReferenceGuard', 'PushBackDraftSessionPort', 'PushBackDraftApply'):
    assert not [p for p in tracked('src/') + tracked('tests/') if pref in p], ('el prefijo ya existe', pref)
_free = git('grep', '-n', '-w', '-E', 'PushBackDraftApplyStatus|PushBackDraftApplyResult|IPushBackDraftStatePort|PushBackDraftSessionPort|'
            'PushBackDraftEditorLaunch|ApplyToDraft|LaunchFor|CT_PBT2A_0[1-4]_\\w*', REV, '--', 'src', 'tests')
assert _free.returncode == 1, ('un nombre nuevo de PB-T2a ya existe en la revision', _free.stdout.decode('utf-8')[:300])


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
assert len(SRCF) == 87, len(SRCF)  # section 6.0: 87 en PB-T2a (registro N)
# INV-PBT2A-01: lo que tiene que quedar vacio cae entero en el ForbiddenWriteScope; ningun archivo existente de APP cambia
for p in ('src/RackCad.UI/', 'src/RackCad.Plugin/', 'src/RackCad.Domain/', 'src/RackCad.Application/Workspace/', APP + 'Pilot/'):
    assert p in SRCF, p
assert all(p in SRCF or p.startswith(APP + 'Pilot/') for p in tracked(APP)), 'un archivo existente de Systems/PushBack fuera del ForbiddenWriteScope'
# PBT2A-C13: los archivos de PB-T1b (su AllowedWriteScope de src/) estan prohibidos
T1B_SRC = [f for f in pbt1b['AllowedWriteScope'] if f.startswith('src/')]
assert T1B_SRC and all(any(f == x or (x.endswith('/') and f.startswith(x)) for x in SRCF) for f in T1B_SRC), T1B_SRC

# tests/: las listas del contrato de PB-T1c (la curada de PB-T0 mas las clases de PB-T1b en Core; la de [PKC2] con PB-TC2, PB-T1a y PB-T1b
# en UI) mas las clases nuevas de PB-T1c (section 6.0)
CORE_T1C = [f for f in pbt1c['ForbiddenWriteScope'] if f.startswith(TR)]
UI_T1C = [f for f in pbt1c['ForbiddenWriteScope'] if f.startswith(UT)]
CORE_T0 = [f for f in pbt0['ForbiddenWriteScope'] if f.startswith(TR)] + [f for f in pbt0['AllowedWriteScope'] if f.startswith(TR)]
T1B_NEW_CORE = [f for f in pbt1b['AllowedWriteScope'] if f.startswith(TR)]
assert len(CORE_T0) == 18 and CORE_T1C == CORE_T0 + T1B_NEW_CORE and len(CORE_T1C) == 23, len(CORE_T1C)
assert len(UI_T1C) == 62 and UI_T1C[-1] == UT + 'Workspace/', len(UI_T1C)
assert UI_T1C == [f for f in pbt1b['ForbiddenWriteScope'] if f.startswith(UT)][:-1] + [f for f in pbt1b['AllowedWriteScope'] if f.startswith(UT)] + [UT + 'Workspace/']
T1C_NEW_CORE = [f for f in pbt1c['AllowedWriteScope'] if f.startswith(TR)]
T1C_NEW_UI = [f for f in pbt1c['AllowedWriteScope'] if f.startswith(UT)]
assert len(T1C_NEW_CORE) == 2 and len(T1C_NEW_UI) == 1, (T1C_NEW_CORE, T1C_NEW_UI)
CORE_T = CORE_T1C + T1C_NEW_CORE
UI_T = UI_T1C[:-1] + T1C_NEW_UI + [UT + 'Workspace/']
assert len(CORE_T) == 25 == len(set(CORE_T)) and len(UI_T) == 63 == len(set(UI_T)), (len(CORE_T), len(UI_T))
TESTF = CORE_T + UI_T
assert not any(a == t or (t.endswith('/') and a.startswith(t)) for a in ALLOWED for t in TESTF)
ROOT = [f for f in pbt1c['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
assert ROOT == ['docs/', '.github/', 'eng/', 'tools/', 'assets/', 'deploy/', 'RackCad.sln', 'Directory.Build.props', 'Directory.Build.targets', 'global.json',
                'NuGet.Config', 'README.md', 'AGENTS.md', 'CLAUDE.md', '.editorconfig', '.gitattributes', '.gitignore'], ROOT
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == 192 == len(set(FORBIDDEN)), len(FORBIDDEN)
for f in FORBIDDEN:
    assert in_head(f) and (not DISK or os.path.exists(f.rstrip('/'))), ('no existe', f)

# ---------------------------------------------------------------- las anclas de Application (registros G, H, Q-3 y Q-4), en la revision
DS = 'src/RackCad.Application/Workspace/Drafts/DraftSession.cs'
DR = 'src/RackCad.Application/Workspace/Drafts/DraftRegistry.cs'
DSB = 'src/RackCad.Application/Workspace/Drafts/DraftStaleBase.cs'
SID = 'src/RackCad.Application/Workspace/SessionId.cs'
PES = APP + 'PushBackEditableState.cs'
PESC = APP + 'PushBackEditableStateComparer.cs'
POG = APP + 'PushBackEditorOutputGate.cs'
OWN = APP + 'PushBackEditableStateOwnership.cs'
RMES = 'src/RackCad.Application/Systems/Shared/RackModuleEditSession.cs'
GL1C = TR + 'PushBackSharedReferenceGuardTests.cs'
CPY = TR + 'PushBackEditableStateCopyTests.cs'
ANCHORS = [
    # G: lo que dejo PB-T1b
    (PES, 40, 'public PushBackEditableState()'), (PES, 52, '}'), (PES, 55, 'public PushBackCompositeEditorState Editor { get; }'),
    (PES, 95, 'public string Name { get; set; }'),
    (PES, 109, 'public PushBackEditorInputs LoadDesignEditor(PushBackDesign design, PushBackResolver resolver)'), (PES, 116, '}'),
    (PES, 119, 'public PushBackEditorInputs RestoreEditor(PushBackDesign lastValidDesign, PushBackResolver resolver)'), (PES, 126, '}'),
    (PES, 130, 'public void AdoptCarriers(PushBackEditorInputs inputs, RackCatalog catalog)'), (PES, 157, 'DimensionViewsTouched = false;'),
    (PES, 164, 'SafetySelections.Clear();'), (PES, 169, '}'),
    (PES, 367, 'public PushBackEditorInputs BuildInputs(PushBackEditorDesignAssembler assembler)'),
    (PES, 399, 'foreach (var safety in assembler.AuthorizedSafety(SafetySelections))'), (PES, 405, '}'),
    (PES, 454, 'public static PushBackEditableSupport SupportOf(PushBackDesign design)'), (PES, 465, '}'),
    (PESC, 33, 'public bool Equals(PushBackEditableState x, PushBackEditableState y)'), (PESC, 43, '}'),
    (PESC, 63, 'public static PushBackEditableState Copy(PushBackEditableState state)'), (PESC, 136, '}'),
    (PESC, 140, 'The target is a'), (PESC, 141, 'fresh state, so nothing of the authority of the source is carried by them.'),
    (PESC, 403, 'private static bool CarriersEqual(PushBackEditableState x, PushBackEditableState y)'),
    (PESC, 429, '|| !string.Equals(x.Name, y.Name, StringComparison.Ordinal))'), (PESC, 441, '}'),
    (POG, 16, 'public static IReadOnlyList<PushBackCompositeDiagnostic> Diagnostics('),
    (POG, 23, 'public static bool IsBlocked(PushBackCompositeEditorState composite, PushBackSystem system)'),
    (POG, 28, 'public static string StatusOr(PushBackCompositeEditorState composite, PushBackSystem system, string fallback)'), (POG, 40, '}'),
    # Q-4: M09 y M12
    (OWN, 82, 'public static PushBackFieldOwner Of(PushBackDocumentLevel level, string jsonPath)'), (OWN, 109, 'var first = FirstProperty(path);'),
    (OWN, 110, 'var declared = Table.Any(row => row.Level == level'), (OWN, 112, 'PushBackFieldOwner.Envelope : PushBackFieldOwner.Unclassified;'),
    (RMES, 76, 'public bool SameAs(RackModuleCommit other)'), (RMES, 102, '|| a.Length != b.Length'),
    (RMES, 136, '|| DerivedPostOverrides[index].Height != other.DerivedPostOverrides[index].Height)'),
    (RMES, 864, 'public bool SameEditAs(RackModuleEditSession other)'),
    (PESC, 228, 'else if (!a.ModuleCommit.SameAs(b.ModuleCommit))'), (PESC, 253, 'if (!OpenOver(x).Commit().SameAs(OpenOver(y).Commit())) return false;'),
    # H: el nucleo de borradores
    (DS, 44, 'public static DraftIntentRead<TState> Admitted(DraftStaleBase staleBase, TState initialState)'),
    (DS, 150, 'public enum DraftEditStatus'), (DS, 152, 'Updated,'), (DS, 155, 'NotEditable,'), (DS, 156, '}'),
    (DS, 239, 'public sealed class DraftSession<TState> : IDraftEntry'), (DS, 284, 'public TState InitialDraftState => _copy(_initial);'),
    (DS, 287, 'public TState CurrentDraftState => _current;'),
    (DS, 298, 'public bool IsDirty => _pending.Count > 0 || !_comparer.Equals(_current, _initial);'),
    (DS, 331, 'private bool CanEdit => _registered'), (DS, 332, '&& (_explicitStatus == null || _explicitStatus == DraftStatus.Conflict);'),
    (DS, 345, 'public DraftEditResult SetCurrentDraftState(TState state)'),
    (DS, 347, 'if (!CanEdit) return new DraftEditResult(DraftEditStatus.NotEditable, IsDirty);'), (DS, 348, '_current = _copy(state);'),
    (DS, 349, 'return new DraftEditResult(DraftEditStatus.Updated, IsDirty);'), (DS, 350, '}'),
    (DS, 773, 'public DraftTransition EnterError(string reason)'), (DS, 800, '}'),
    (DR, 143, 'public DraftBeginResult<TState> Begin<TState>(SessionId session, string rackId, DraftIntentRead<TState> read, IEqualityComparer<TState> comparer, Func<TState, TState> copy)'),
    (DR, 145, 'throw new ArgumentException("A rack id is required.", nameof(rackId));'), (DR, 167, '}'),
    (DR, 204, 'public DraftDiscardResult Discard(SessionId session, string rackId)'),
    # Q-3: G-L1c y las huellas
    (GL1C, 33, 'private static readonly string[] Tokens ='), (GL1C, 39, 'private static readonly HashSet<string> Rebind'),
    (GL1C, 86, 'private static List<string> Members(string source)'), (GL1C, 110, 'private static Dictionary<string, string> Aliases('),
    (GL1C, 152, 'private static List<string> Violations(string source)'), (GL1C, 203, 'private static string Inject(string source, string line)'),
    (GL1C, 212, 'public void CT_PBT1B_14_NoLineOfTheEditingPathMutatesTheResolvedRackOrTheModuleReport()'),
    (CPY, 278, 'private static HashSet<object> AllObjects(object root, Func<FieldInfo, bool> skip = null)'),
    (CPY, 1080, 'private static string Fingerprint(object root)'),
    # H-2 y Q-4: las guardas que dicen donde no va el puerto (Drafts/) y por que no lleva .RequestUpdate( (G-10 mira todo el producto)
    (TR + 'Workspace/WorkspaceDraftBoundaryTests.cs', 193, 'var systems = new Regex(@"PushBack|Selectiv|'),
    (TR + 'Workspace/WorkspaceDraftBoundaryTests.cs', 204, 'private static readonly Regex ForbiddenTypeNames = new("Subscription|Editor|Tab|Handler"'),
    (TR + 'Workspace/WorkspacePilotUpdateIntegrationGuardTests.cs', 30, 'private static IReadOnlyList<(string Path, string Code)> Product() => Src.AllProduct();'),
]
TEXT = {}
for path, line, text in ANCHORS:
    if path not in TEXT:
        TEXT[path] = lines_of(path)
    got = TEXT[path][line - 1].strip()
    assert (got == text) if text == '}' else (text in got), ('ancla', path, line, got)
# DS:239-800: la clase acaba en :800 (la llave a cuatro espacios)
assert TEXT[DS][799] == '    }' and len(TEXT[DS]) - (TEXT[DS][-1] == '') == 801, len(TEXT[DS])
# G-L1c y las huellas no cambian desde el cierre de PB-T1b (Q-3: las mismas en 95373af6 y a18ba345)
assert git('diff', '--quiet', PB1B_CLOSE, REV, '--', GL1C, CPY).returncode == 0, 'G-L1c o las huellas cambiaron desde el cierre de PB-T1b'
# las quince autopruebas de G-L1c (:223-235 y :238-239; Q-3), con (k1) y (k2) literales
SELFTESTS = ['a', 'b', 'c', 'e', 'f1', 'f2', 'f3', 'g1', 'g2', 'h1', 'h2', 'i', 'j', 'k1', 'k2']
SELFTESTS15 = '(a), (b), (c), (e), (f1), (f2), (f3), (g1), (g2), (h1), (h2), (i), (j), (k1) y (k2)'
_st = [(k, re.match(r'^\s*\("(\w+)", "(.*)"\),$', TEXT[GL1C][k - 1])) for k in list(range(223, 236)) + [238, 239]]
assert all(m for _, m in _st) and [m.group(1) for _, m in _st] == SELFTESTS, 'las quince autopruebas de G-L1c'
assert _st[13][1].group(2) == 'var r = side.LastModuleReconciliation; (r.Preserved as List<string>).Add(\\"x\\");'
assert _st[14][1].group(2) == 'var p = side.LastModuleReconciliation.Preserved; ((List<string>)p).Add(\\"x\\");'
assert TEXT[GL1C][235].strip() == '' and TEXT[GL1C][239].strip() == '};', 'el arreglo de autopruebas de G-L1c'
# la API de Application que usan el puerto, el lanzamiento y sus pruebas (D-PBT2-02, D-PBT2-03 y las recetas del section 5.0)
API = {
    DS: ['public string RackId { get; }', 'public TState CurrentDraftState => _current;', 'public DraftEditResult SetCurrentDraftState(TState state)',
         'public bool IsDirty =>', 'public DraftTransition EnterError(string reason)'],
    DR: ['public DraftBeginResult<TState> Begin<TState>(', 'public DraftDiscardResult Discard(SessionId session, string rackId)'],
    DSB: ['public static DraftStaleBase Create(IEnumerable<DraftStaleBaseEntry> entries'],
    SID: ['public readonly record struct SessionId(long Value);'],
    PES: ['public PushBackEditorInputs LoadNew(RackCatalog catalog)', 'public void SeedDefaultSafety(RackCatalog catalog)',
          'public PushBackEditorInputs LoadDesign(PushBackDesign design, string rackName, PushBackResolver resolver)', 'public double PalletTolerance { get; set; }'],
    PESC: ['public static PushBackEditableStateComparer Instance { get; }', 'public static PushBackEditableState Copy(PushBackEditableState state)'],
}
for path, sigs in API.items():
    stripped = [l.strip() for l in lines_of(path)]
    for sig in sigs:
        assert sum(1 for l in stripped if l.startswith(sig)) == 1, ('API', path, sig)
assert 'public readonly record struct DraftEditResult(DraftEditStatus Status, bool IsDirty);' in [l.strip() for l in TEXT[DS]], 'DraftEditResult'
assert 'Mutable,' in [l.strip() for l in lines_of(DSB)] and 'public enum DraftMembership' in [l.strip() for l in lines_of(DSB)]
# INV-PBT2A-05: los miembros de DraftSession que los archivos nuevos no pueden nombrar existen (CrossBoundary ... SetPendingText)
DS_FORBIDDEN = ['CrossBoundary', 'TryBeginApply', 'CompleteApply', 'DiscardAndReload', 'KeepMineOnCurrentVersion', 'EnterError', 'MarkPossiblyStale', 'SetPendingText']
for n in DS_FORBIDDEN:
    assert sum(1 for l in TEXT[DS] if re.match(r'^\s{8}public [\w<>]+ %s\(' % n, l)) >= 1, ('INV-PBT2A-05: no es un miembro publico de DraftSession', n)
# INV-PBT2A-04: Application usa un namespace de bloque por archivo
for p in (PES, PESC, POG):
    assert 'namespace RackCad.Application.Systems.PushBack' in [l.strip() for l in lines_of(p)], ('namespace de bloque', p)

# ---------------------------------------------------------------- V2-05 (section 6.0): reasignar editable y anadir los miembros de INV-PBT2B-03
# los nombres nuevos de INV-PBT2B-03 (PBT2B-C13 del section 6.2) y editable no estan en las listas de las guardas de PB-T1c
NEW_T2B = ['mode', 'draftPort', 'appliedState', 'BomDialog', 'LibraryPathDialog', 'ModeForTest', 'ForDraft', 'ApplyModeChrome', 'RefuseInWorkspace',
           'ApplyEditsToEditingState', 'ApplyToDraft', 'HasUnappliedDraftChanges', 'WorkspaceApplyLabel', 'WorkspaceApplyToolTip',
           'WorkspaceApplyBlockedToolTip', 'WorkspaceRefusalMessage', 'WorkspaceLoadRefusalMessage', 'WorkspaceInvalidMessage', 'WorkspaceBlockedMessage',
           'WorkspaceNothingToApplyMessage', 'WorkspaceAppliedMessage', 'WorkspaceNotEditableMessage', 'WorkspaceUnappliedQuestion', 'PushBackEditorMode',
           'Classic', 'Workspace']
T1C_TESTS = [f for f in pbt1c['AllowedWriteScope'] if f.startswith('tests/')]
assert len(T1C_TESTS) == 3 and all(in_head(f) for f in T1C_TESTS), ('las tres clases de PB-T1c no estan en la revision', T1C_TESTS)
T1C_SG, T1C_D2 = TR + 'PushBackWindowOnStateSourceGuardTests.cs', TR + 'PushBackWindowSharedReferenceGuardTests.cs'
assert {T1C_SG, T1C_D2} <= set(T1C_TESTS)


def array_items(text, head):
    m = re.search(re.escape(head) + r'\s*\{(.*?)\};', text, re.S)
    assert m, ('lista de la guarda', head)
    return re.findall(r'"([^"]*)"', m.group(1))


_sg, _d2 = show(T1C_SG), show(T1C_D2)
GUARD_LISTS = {'Forwarded (CT-PBT1C-03)': array_items(_sg, 'private static readonly string[] Forwarded ='),
               'Tokens (CT-PBT1C-04)': array_items(_d2, 'private static readonly string[] Tokens ='),
               'Rebind (CT-PBT1C-04)': array_items(_d2, 'new HashSet<string>(StringComparer.Ordinal)')}
assert len(GUARD_LISTS['Forwarded (CT-PBT1C-03)']) == 12 and len(GUARD_LISTS['Rebind (CT-PBT1C-04)']) == 4, GUARD_LISTS
for k, names in GUARD_LISTS.items():
    assert not set(names) & ({'editable'} | set(NEW_T2B)), ('V2-05: una guarda de PB-T1c nombra editable o un miembro de INV-PBT2B-03', k, set(names) & ({'editable'} | set(NEW_T2B)))
# ningun literal de las tres clases de PB-T1c fija una asignacion de editable, su declaracion o su forma readonly
_lits = []
for f in T1C_TESTS:
    for lit in re.findall(r'@?"(?:[^"\\\n]|\\.)*"', show(f)):
        if re.search(r'\beditable\b', lit):
            _lits.append(lit)
            assert not re.search(r'\beditable\s*=(?![=>])|readonly|PushBackEditableState\s+editable', lit), ('V2-05: un literal de PB-T1c fija editable', f, lit)
EDITABLE_LITERALS = len(_lits)
# lo que depende de la ventana de PB-T1c: editable declarado como D-PBT1-13 (1) y todo lo que cuelga de el, calculado


# ---- window_checks (inicio): run_pbt2a_checks.py extrae este bloque y lo prueba sobre la ventana simulada de PB-T1c
EDITABLE_DECL = 'private readonly PushBackEditableState editable = new PushBackEditableState();'
_WMODS = r'(?:public|private|internal|protected|static|readonly|volatile|new|const)'
_WTYPE = r'[\w\.]+(?:\s*<[^;=(){}]*>)?(?:\s*\[\s*\])*\??'
_WFIELD = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*((?:' + _WMODS + r'\s+)+)(?!(?:class|struct|enum|interface|record|event|delegate)\b)(' + _WTYPE + r')\s+(\w+)\s*(=(?![=>])|;)')
_WOTHER = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:' + _WMODS + r'\s+)+' + _WTYPE + r'\s+editable\s*(?:=>|\{|\()')
_WREF = re.compile(r'^(?:\(\s*[\w\.<>\?]+\s*\)\s*)?(?:this\s*\.\s*)?(?:editable|composite|state|moduleOwner)\b(?:\s*\??\.\s*\w+|\s*\[[^\]]*\])*$')
_WCMODS = r'(?:public|private|internal|protected|static|readonly|sealed|override|virtual|abstract|partial|async|const|event|unsafe|extern|volatile)'
_WCDECL = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:' + _WCMODS + r'\s+)+(?:class\s+(\w+)|[^=;({]*?\b(\w+)\s*(?:<[^>]*>)?\s*(?:\(|=>|=|;|\{|$))')
WINDOW_ALWAYS = ['state y moduleOwner calculados', 'sin otra asignacion de editable', 'sin suscripciones al estado',
                 'sin capturas del estado en campos ni constructores (M-3)', 'sin nombres de PBT2B-C13 (M-3)']


def _wclean(lines):
    """El texto sin comentarios y con los literales vaciados (el de census_strip de PB-T1c), linea a linea."""
    t = '\n'.join(lines)
    t = re.sub(r'@"(?:[^"]|"")*"', '""', t)
    t = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', t)
    t = re.sub(r"'(?:\\.|[^'\\\n])'", "''", t)
    t = re.sub(r'/\*.*?\*/', ' ', t, flags=re.S)
    return re.sub(r'//[^\n]*', '', t).split('\n')


def _wstmt(code, k):
    """La sentencia que empieza en la linea k de code, hasta su ';', con los espacios colapsados."""
    out = []
    for l in code[k:]:
        out.append(l)
        if ';' in l:
            break
    s = ' '.join(' '.join(out).split())
    return s[:s.index(';') + 1] if ';' in s else s


def window_checks(w_lines, wc_lines, t2b_names):
    """V2-05 (section 6.0) y CA-T2a M-2 y M-3 sobre la ventana: (hay una declaracion de editable, {condicion: cumple}, nota de forma o None,
    detalle). La regla de D-13 compara con los espacios colapsados; un cambio solo de forma (un campo private readonly PushBackEditableState con
    inicializador y una sola declaracion) se anota y no es material."""
    files = [('W', _wclean(w_lines)), ('WC', _wclean(wc_lines))]
    decls, fields, captures, names, assign, subs = [], set(), [], [], [], []
    for tag, code in files:
        depth = 0
        for k, l in enumerate(code):
            if depth == 2:
                m = _WCDECL.match(l)
                if m:
                    names.append(m.group(1) or m.group(2))
                f = _WFIELD.match(l)
                if f:
                    mods, typ, name, op = f.groups()
                    fields.add(name)
                    stmt = _wstmt(code, k)
                    rhs = stmt.split('=', 1)[1].rstrip(';').strip() if op.startswith('=') else ''
                    if name == 'editable':
                        decls.append((tag, k, set(mods.split()), ' '.join(typ.split()), rhs, stmt))
                    elif rhs and _WREF.match(rhs):
                        captures.append('%s campo %s = %s' % (tag, name, rhs))
                elif _WOTHER.match(l):
                    decls.append((tag, k, None, None, '', ' '.join(l.split())))
            depth += l.count('{') - l.count('}')
    decl_at = {(t, k) for t, k, _, _, _, _ in decls}
    for tag, code in files:
        for k, l in enumerate(code):
            if (tag, k) not in decl_at and re.search(r'(?<![\w.])(?:this\s*\.\s*)?editable\s*=(?![=>])', l):
                assign.append('%s:%d' % (tag, k + 1))
        subs += re.findall(r'\b(?:editable|composite)(?:\s*\.\s*\w+)+\s*[+-]=', '\n'.join(code))
        # las asignaciones de los constructores: field = <una cadena de acceso que sale del estado>
        for k, l in enumerate(code):
            if re.match(r'^\s*(?:public|private|internal|protected)\s+RackPushBackSystemWindow\s*\(', l):
                body, depth, opened = [], 0, False
                for l2 in code[k:]:
                    body.append(l2)
                    depth += l2.count('{') - l2.count('}')
                    opened = opened or '{' in l2
                    if opened and depth == 0:
                        break
                text = ' '.join(' '.join(body).split())
                text = text[text.index('{') + 1:] if '{' in text else ''
                for st in re.split(r'[;{}]', text):
                    m = re.match(r'^\s*(?:this\s*\.\s*)?(\w+)\s*=(?![=>])\s*(.*?)\s*$', st)
                    if m and m.group(1) in fields and m.group(1) != 'editable' and _WREF.match(m.group(2)):
                        captures.append('%s constructor %s = %s' % (tag, m.group(1), m.group(2)))
    w_flat = ' '.join('\n'.join(files[0][1]).split())
    one = decls[0] if len(decls) == 1 else None
    decl_ok = bool(one and one[0] == 'W' and one[2] == {'private', 'readonly'} and one[3] == 'PushBackEditableState' and one[4])
    note = None if not decl_ok or one[5] == EDITABLE_DECL else 'la declaracion de editable cambia solo de forma (hecho no material, D-13): ' + one[5]
    collisions = sorted(set(names) & set(t2b_names))
    checks = {'editable: un campo private readonly PushBackEditableState con inicializador y una sola declaracion (D-13)': decl_ok,
              'composite => editable.Editor': bool(re.search(r'\bcomposite\s*=>\s*editable\s*\.\s*Editor\s*;', w_flat)),
              'EditableStateForTest => editable;': bool(re.search(r'\bEditableStateForTest\s*=>\s*editable\s*;', w_flat)),
              'state y moduleOwner calculados': all(re.search(r'\b%s\s*=>' % n, w_flat) for n in ('state', 'moduleOwner')),
              'sin otra asignacion de editable': not assign, 'sin suscripciones al estado': not subs,
              'sin capturas del estado en campos ni constructores (M-3)': not captures, 'sin nombres de PBT2B-C13 (M-3)': not collisions}
    return len(decls) > 0, checks, note, {'asignaciones': assign, 'capturas': captures, 'nombres': collisions, 'declaraciones': [d[5] for d in decls]}
# ---- window_checks (fin)


# autopruebas de window_checks sobre un texto sintetico con la forma de D-PBT1-13 (1) y (9), dentro de un namespace y una clase
def _syn(body, wc_body=()):
    head, tail = ['namespace RackCad.UI.Systems.PushBack', '{', '    public partial class RackPushBackSystemWindow', '    {'], ['    }', '}']
    return head + list(body) + tail, head + list(wc_body) + tail


_SB = ['        ' + EDITABLE_DECL, '        private PushBackCompositeEditorState composite => editable.Editor;',
       '        private PushBackEditorState state => composite.Active;', '        private PushBackEditorState moduleOwner => composite.SideA;',
       '        internal PushBackEditableState EditableStateForTest => editable;', '        private readonly PushBackEditorDesignAssembler assembler;',
       '        private PushBackEditableState appliedCopy;', '        public RackPushBackSystemWindow()', '        {',
       '            assembler = new PushBackEditorDesignAssembler();', '            appliedCopy = PushBackEditableStateComparer.Copy(editable);', '        }']
_CTOR = _SB.index('        {') + 1
WINDOW_SELFTESTS = 0
# las que tienen que pasar enteras: la forma base, un cambio solo de forma (con nota), la declaracion partida en dos lineas (sin nota, por los
# espacios colapsados) y una propiedad calculada nueva que sale del estado
for body, wc, with_note in ((_SB, (), False), (['        private readonly PushBackEditableState editable = new();'] + _SB[1:], (), True),
                            (['        private readonly PushBackEditableState editable =', '            new PushBackEditableState();'] + _SB[1:], (), False),
                            (_SB + ['        private PushBackEditorState sideA => composite.SideA;'], (), False)):
    declared, checks, note, detail = window_checks(*_syn(body, wc), NEW_T2B)
    assert declared and all(checks.values()) and (note is not None) == with_note, ('autoprueba de window_checks que no pasa', body, checks, note, detail)
    WINDOW_SELFTESTS += 1
# las que tienen que fallar
for body, wc in ((_SB[1:], ()), (_SB[:_CTOR] + ['            editable = new PushBackEditableState();'] + _SB[_CTOR:], ()),
                 ([_SB[0], '        private readonly PushBackCompositeEditorState composite = null;'] + _SB[2:], ()),
                 (_SB[:2] + ['        private PushBackEditorState state = null;'] + _SB[3:], ()),
                 (_SB[:_CTOR] + ['            composite.Changed += OnChanged;'] + _SB[_CTOR:], ()), (_SB[:4] + _SB[5:], ()),
                 (_SB, ['        private void Reset()', '        {', '            this.editable = null;', '        }']),
                 (['        private PushBackEditableState editable = new PushBackEditableState();'] + _SB[1:], ()),
                 (['        private readonly PushBackEditableState editable;'] + _SB[1:], ()),
                 (['        private readonly object editable = new PushBackEditableState();'] + _SB[1:], ()),
                 (_SB, ['        ' + EDITABLE_DECL]),
                 (_SB + ['        private readonly PushBackEditorState sideA = composite.SideA;'], ()),
                 (_SB[:_CTOR - 2] + ['        private PushBackEditorState sideA;'] + _SB[_CTOR - 2:_CTOR] + ['            sideA = state;'] + _SB[_CTOR:], ()),
                 (_SB[:_CTOR - 2] + ['        private PushBackEditorState sideA;'] + _SB[_CTOR - 2:_CTOR] + ['            this.sideA = editable.Editor.SideA;'] + _SB[_CTOR:], ()),
                 (_SB + ['        private void ApplyToDraft()', '        {', '        }'], ()), (_SB, ['        private int mode;'])):
    declared, checks, note, detail = window_checks(*_syn(body, wc), NEW_T2B)
    assert not (declared and all(checks.values())), ('autoprueba de window_checks que no falla', body, wc, checks, detail)
    WINDOW_SELFTESTS += 1
W_LINES, WC_LINES = lines_of(W), lines_of(WC)
T1C_WINDOW, _checks, FORM_NOTE, _wdetail = window_checks(W_LINES, WC_LINES, NEW_T2B)
# CA-T2a M-3: lo que no depende de la ventana se cumple ya, y se vuelve a comprobar en el cierre
for k in WINDOW_ALWAYS:
    assert _checks[k], ('V2-05 (CA-T2a M-3): STOP y v3 del paquete', k, _wdetail)
_subs = [] if _checks['sin suscripciones al estado'] else ['?']
EDITABLE_REPORT = 'W no declara editable (la ventana de PB-T1c no esta en la revision); ' + ', '.join('%s=%s' % (k, _checks[k]) for k in WINDOW_ALWAYS)
if T1C_WINDOW:
    EDITABLE_REPORT = ', '.join('%s=%s' % kv for kv in _checks.items()) + ('; ' + FORM_NOTE if FORM_NOTE else '')
    soft(all(_checks.values()), 'V2-05: en la ventana de PB-T1c falla una condicion para reasignar editable (STOP y v3 del paquete): ' + EDITABLE_REPORT)
soft(T1C_WINDOW, 'V2-05: W no declara editable como D-PBT1-13 (1) (la ventana de PB-T1c, de su cierre); la reasignacion se comprueba sobre el cierre')

# ---------------------------------------------------------------- las decisiones del Owner de PB-T2 (F4B-E28), custodiadas
CO = PILOT + '/F4B-DRAFTS/coordinator/'
OWNER_PBT2 = CO + 'owner-pbt2-decisions.md'
_disp = show(DISP).replace('\r\n', '\n')
_disp_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
assert _disp_ids == list(range(1, len(_disp_ids) + 1)), ('f4b-dispositions.md no esta numerado seguido', _disp_ids[-3:])
DISP_LAST = 'F4B-E%02d' % _disp_ids[-1]
assert _disp_ids[-1] >= 28 and ('\n## %s. Decisiones del Owner antes de los contratos de PB-T2' % OWNER_PBT2_ENTRY) in _disp, 'F4B-E28 en f4b-dispositions.md'
_own = show(OWNER_PBT2)
for q, ans in (('Q-PBT2-1', 'Aplica sin cerrar; Cerrar avisa (Recomendado)'), ('Q-PBT2-2', 'Lo rechaza, como Actualizar (Recomendado)'),
               ('Q-PBT2-3', 'En PB-T4, con prueba ya (Recomendado)'), ('Q-PBT2-4', 'S' + chr(0xED) + ', con esa condici' + chr(0xF3) + 'n (Recomendado)')):
    _blk = _own[_own.index('## ' + q + '.'):]
    _blk = _blk[:_blk.index('\n## ', 3)] if '\n## ' in _blk[3:] else _blk
    assert ('**Respuesta del Owner:** ' + chr(0xAB) + ans + chr(0xBB)) in _blk, ('la respuesta del Owner', q)

# ---------------------------------------------------------------- D1: build-plugin depende de tests (la CI del RED con Core en failure)
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

# ---------------------------------------------------------------- las celdas (section 6.0): las de PB-T1c, que vencen el 2026-12-29
cells = json.loads(json.dumps(pbt1c['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna'] and all(c['TestedEfforts'] == ['high'] for c in cells)
assert cells == pbt1b['EligibleCells'] == json.loads(show(PBTC2C))['EligibleCells'], 'las celdas difieren de las de PB-TC2'
MC = lines_of('docs/automation/agent-execution/model-catalog.md')
assert '+ 90 d' in MC[13] and all(MC[k].startswith('- Fecha de verificaci') and MC[k].strip().endswith('2026-09-30') for k in (50, 96)), 'model-catalog.md:14, :51 y :97'
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
    """Una vineta del paquete con sus subvinetas, en una linea ASCII: sin negritas ni comillas de codigo, sin el guion de vineta."""
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
    """(id, titulo o None, cuerpo) de una vineta de invariante: '**INV-PBT2A-01. Titulo:** ...', '**INV-PBT2A-04.** ...' o '**07. Titulo:** ...'."""
    m = re.match(r'^\s*- \*\*((?:INV-PBT2A-)?\d\d)\.\s*(.*?)\*\*\s*(.*)$', first)
    assert m, ('vineta de invariante', first[:80])
    idd, t, rest = m.groups()
    t = t.strip()
    if t.endswith(':'):
        return idd[-2:], ascii_fold(t[:-1]), md_fold([rest] + subs)
    if t == '':
        return idd[-2:], None, md_fold([rest] + subs)
    return idd[-2:], None, md_fold([t + ' ' + rest] + subs)


# ---- los textos que trae el contrato, en ASCII (el plegado de md_fold), literales del paquete v2.1 y comprobados contra el si se lee
OBJECTIVE_PKG = (
    'Crear en Application el puerto por el que el editor completo de Push Back en modo WORKSPACE devuelve su estado editado al borrador '
    '(IPushBackDraftStatePort, PushBackDraftApplyResult y el adaptador PushBackDraftSessionPort sobre DraftSession<PushBackEditableState>) y el '
    'lanzamiento tipado de ese modo (PushBackDraftEditorLaunch), segun el paquete del contrato de PB-T2 v<n> y la lista cerrada CT-PBT2A-01..04. El '
    'RED es el esqueleto inerte y las pruebas nuevas, que fallan por asercion salvo la guarda G-L1; el GREEN implementa solo los dos archivos nuevos de '
    'Application. La ventana, el piloto, el host core, el nucleo de borradores, el Plugin y los archivos de PB-T1b no cambian; no se ejecuta AutoCAD. '
    'Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: STOP y se reabre L-1 (decision D2 del '
    'Owner, F4B-E11).')
assert OBJECTIVE_PKG.count('v<n>') == 1
OBJECTIVE = OBJECTIVE_PKG.replace('v<n>', PKG_VERSION)
INV_OWN = [
    ('01', 'Alcance', 'el diff dentro de las 4 rutas; en src/, solo los dos archivos nuevos. git diff BaseSha..CurrentSha -- src/RackCad.UI '
                      'src/RackCad.Plugin src/RackCad.Domain src/RackCad.Application/Workspace src/RackCad.Application/Systems/PushBack/Pilot vacio, y '
                      'ningun archivo existente de src/RackCad.Application/Systems/PushBack/ cambia.'),
    ('02', 'RED y GREEN', 'el RED es el esqueleto exacto del section 5.2 y las dos clases; el GREEN solo toca los dos archivos de src/. El criterio '
                          'del RED: el filtro con ExpectRed da Failed == Selected, todos por asercion. La guarda pasa en el RED y en el GREEN, y sus '
                          'autopruebas fallan sobre lo inyectado.'),
    ('03', 'La API', 'exactamente la de D-PBT2-02 y D-PBT2-03, con las sentencias de excepcion literales. El unico miembro privado de producto es el '
                     'campo draft del adaptador.'),
    ('04', None, 'Ningun campo estatico. Un solo namespace RackCad.Application.Systems.PushBack de bloque por archivo; las pruebas, namespace RackCad.Tests.'),
    ('05', None, 'Los archivos nuevos de src/ no contienen RackCad.UI, System.Windows, Autodesk, Pilot, RackSystemKind, .RequestUpdate(, CrossBoundary, '
                 'TryBeginApply, CompleteApply, DiscardAndReload, KeepMineOnCurrentVersion, EnterError, MarkPossiblyStale ni SetPendingText.'),
    ('06', None, 'ApplyToDraft hace una sola llamada al borrador: SetCurrentDraftState(edited).'),
]
INV_NOTE = 'Nota (no va al contrato). El RED y su CI siguen la leccion M14 (section 6.0). G-L1 se copia de G-L1c (registro Q-3). La lista cerrada de miembros es la leccion M10.'
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
                                   'guardan TRX y log; el perfil no cambia.'),
    ('11', 'La evidencia', 'rutas relativas comprobadas; TreeSha real o null; el Worker no escribe en docs/; una sustitucion seria C02.'),
    ('12', None, 'Las reglas de F4B, sin AutoCAD, con las tres comprobaciones deterministas de F4B-E15 en la verificacion.'),
    ('14', 'D2', 'ninguna copia nueva comparte un objeto mutable fuera de las dos referencias declaradas de cada lado, y ninguna linea anadida muta un '
                 'PushBackSystem ni un RackModuleReconciliationResult adoptado.'),
]
C04_PKG = 'a method of a filter with ExpectRed passes over the skeleton, or fails by an exception and not by an assertion.'
C13_NAMES = ['PushBackDraftApplyStatus', 'NotEditable', 'Applied', 'PushBackDraftApplyResult', 'Status', 'IsDirty', 'IPushBackDraftStatePort', 'ApplyToDraft',
             'PushBackDraftSessionPort', 'draft', 'LaunchFor', 'PushBackDraftEditorLaunch', 'State', 'RackId', 'SourceProject', 'Port']
C13_PKG = ('doing the task would require touching src/RackCad.Application/Workspace/Drafts/, a file of PB-T1b, Pilot/ or WorkspaceHostCore; or adding, in '
           'PushBackDraftStatePort.cs or PushBackDraftEditorLaunch.cs, a member or type declaration (of any visibility: type, method, property, field, '
           'constant, enum member or nested type) whose name is not one of ' + ', '.join(C13_NAMES[:-1]) + ' and ' + C13_NAMES[-1] + ', besides the '
           'constructors of those types. The Worker stops before adding it.')
C15_PKG = ('CT-PBT2A-03 gives iguales=False, diseno-igual=False, huellas-iguales=False or something shared outside D2. It is a new fact of PB-T1b: STOP and '
           'return to the Owner (OB-06.5).')
E_ANTES = ('antes del Worker: el contrato, la salida del Controller (output.json, con las colecciones opacas vacias), la delegacion final con esas '
           'colecciones puestas por la sesion (delegation.json), la comparacion byte a byte, A1-A8 y nc4;')
E_RED = ('el RED: el commit RED y su push antes del GREEN, con su CI tal como dice arriba (sin cuelgue, ni blame-hang ni timeout), y los TRX de cada filtro '
         'con ExpectRed;')
E_GREEN = 'el GREEN: el commit GREEN y su CI 4/4;'
E_LOCAL = ('en local: el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED y el GREEN, y tres corridas en verde de los filtros con '
           'ExpectRed sobre el GREEN;')
E_ENTREGA = ('la entrega: el mapa CT -> metodo, las rutas relativas comprobadas y el Trailer de worker-handoff.json, que es el trailer real de los commits '
             'RED y GREEN (Co-Authored-By: ...; la mitad (b) de PBT1B-IR-M14);')
E_DESPUES = 'despues: la verificacion con las tres comprobaciones de F4B-E15, y nc1 a nc3.'
E_COMMON = [E_ANTES, E_RED, E_GREEN, E_LOCAL, E_ENTREGA, E_DESPUES]
CI_RED_HEAD = ('La CI del RED (leccion M14 de PB-T1b): en los tres RED, Core falla, asi que build-plugin sale omitido por su needs (registro O). RedPart '
               'es pass con:')
CI_RED_T2A = 'PB-T2a: tests en failure, ui-tests y build-ui en success;'
RED_CHAIN_HEAD = 'El RED de la cadena en una correccion (section 16.8): una correccion que toque RT o ChainRedFiles exige RED otra vez:'
RED_CHAIN_T2A = 'en PB-T2a, vuelve a poner el esqueleto;'
E_T2A_PKG = 'ExpectedEvidence: la comun, con la tabla "CT -> la asercion que falla en el RED" y la salida de las autopruebas de la guarda.'
LB_PKG = ('Linea base de la CI: la del cierre de PB-T1c. Prevista: el cierre de PB-T1b medido (44438060317: Core 19 086; UI 2 437, con 2 416 Passed y 21 '
          'Skipped), mas PB-T1c (Core + 2, UI + 2): Core 19 088, UI 2 439. GREEN de PB-T2a: Core + 4 = 19 092; UI 2 439.')
CORR_PKG = ('Las correcciones: se leen con F4B-E25. Una ruta UNIT_CHANGE de git diff --name-only AuthorityRevision BaseSha no incumple 16.3 si se cumplen '
            'sus tres condiciones, que comprueba el Controller. Las EXTERNAL, nunca. No hace falta reemitir el contrato para corregir.')
UC_T2A_PKG = 'PB-T2a: ninguna. Sus dos archivos no existen en la AuthorityRevision y no son autoridades;'
# CA-T2a M-4 (v2.2, V22-01): la referencia de PBT2x-C11 a las diferencias admitidas de cada firma, que estan en [PT1] section 6.0
C11_REF_OLD = 'section 6.0 of the package'
C11_REF_NEW = 'section 6.0 of the PB-T1 contract package ([PT1], docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md)'
BOUNDED_CHECK = PILOT + '/F4B-DRAFTS/pbt2/independent-review-pbt2a-v21-check.md'
STD_LINE = 'StopConditions estandar (17): S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08, con P-01 = "' + P01_J + '".'
# section 1, las filas que citan las autoridades (primera celda, segunda celda), en ASCII
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
    'PROTO': ('docs/AUTOMATION_PLAN.md section 16.3 a section 16.11; docs/automation/agent-execution/README.md section 4 y section 10; routing.md; model-catalog.md',
              'El protocolo, A1-A8, el RED de la cadena, las 14 comprobaciones, los terminos, las paradas y las celdas'),
}
OD_ROW = ('ACCEPT IN PRINCIPLE. Separate: "apply edits to current editing state" from: "close editor" and from: "write authored DWG state". Workspace mode '
          'must return edited state to DraftSession. Classic RACKEDITAR may continue its compatible existing outer workflow. Avoid two independent '
          'implementations of field validation.')
# section 6.0, las lineas que el contrato trae o en las que se apoya (cada una, una vez en la seccion)
EXPECT_60 = [
    'Identidad: Schema rackcad-gate-contract/v1; Unit I-64; Gate F4B; attempts 1 de 3 (OB-10.1); RoutingEnforcement required; CorrectionsAuthorized true. '
    'Evidencia en docs/automation/evidence/I-64-pilot/<TaskId>/. Los TaskId tienen 26, 31 y 29 caracteres (MAX_PATH; v2.1, V21-05).',
    'git diff <cierre de PB-T1c> <BaseSha> -- src vacio;', 'las anclas de los registros G, H y Q;',
    'sobre el texto real de PB-T1c (v2, V2-05): que editable esta declarado como en D-PBT1-13 (1), que todo lo que cuelga de el es calculado, y que ni '
    'CT-PBT1C-01 a 04 ni ninguna otra guarda impiden reasignarlo en un constructor ni anadir los miembros de INV-PBT2B-03. Si falla, STOP: una v3 del '
    'paquete con una subtarea propia y su censo, antes de emitir nada;',
    'las reglas y las autopruebas de G-L1c;',
    'La comprobacion acotada: un revisor independiente, en solo lectura, sobre este paquete, la seccion de la subtarea y la salida de sus anclajes. Su '
    'resultado, EMITIBLE o NO EMITIBLE, se custodia en F4B-DRAFTS/pbt2/.',
    'en el commit de emision de PB-T2a: este paquete (en docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md), el registro, v2-delta.md, '
    'v21-delta.md, v22-delta.md (v2.2, V22-02), los guiones de medicion (measure_pbt2.py, measure_pbt2_v2.py, forbidden_pbt2.py y '
    'check_questions_v2.py), IR1 con su recomprobacion (IR1-R), las decisiones de Q-PBT2-1 a 4, y el contrato y su generador en su coordinator/;',
    'La delegacion (F4B-E21): el Controller deja vacias las ocho colecciones opacas, y la sesion las pone byte a byte. La comparacion byte a byte, A1-A8 '
    'y nc4 se hacen sobre la delegacion final.',
    '[P];', '[PT1];', 'este paquete, el registro y v2-delta.md;', 'IR1 y las comprobaciones acotadas;',
    'las decisiones del Owner F4B-E03, E09, E11, E19, E21, E22, E23, E24 y E25 (sus archivos de coordinator/) y sus vueltas;',
    'f4b-dispositions.md y los cierres de PB-T1b y PB-T1c;', 'la Proposal V5 (docs/initiatives/I-64-proposal-v5.md);', 'el propio contrato.',
    UC_T2A_PKG,
    'EXTERNAL: docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md (D7, D8, D12 y D13), docs/AUTOMATION_PLAN.md (section 16), '
    'docs/automation/agent-execution/README.md, routing.md y model-catalog.md.',
    CORR_PKG,
    'src/: 87 en PB-T2a, 60 en PB-T2b y 59 en PB-T2c (medidos en 95373af6). En PB-T2c sube a 60 tras PB-T2b, por el archivo del modo;',
    'tests/: la lista curada de PB-T0 (18 entradas); en tests/RackCad.UI.Tests/, la de [PKC2] (51 archivos y Workspace/), las clases de PB-TC2, PB-T1a, '
    'PB-T1b y PB-T1c y las de las subtareas anteriores de PB-T2;',
    'la raiz: las 17 entradas de [PKC2].',
    STD_LINE,
    'Las de [DP], literales en cada contrato (x = A, B o C): PBT2x-C08, PBT2x-C11 y PBT2x-C12, con el texto de [PT1] section 6.0, cambiando solo el '
    'prefijo y, en PBT2x-C11, la referencia "' + C11_REF_OLD + '" por "' + C11_REF_NEW + '" (v2.2, V22-01). Ninguna aceptacion puntual anterior '
    'de un control se extiende.',
    'La parada del limite de D2: PBT2x-C17, con el texto de [PT1] section 6.0 y el literal del Owner. El tiempo de vida incluye el de la ventana de WORKSPACE.',
    RED_CHAIN_HEAD, RED_CHAIN_T2A, CI_RED_HEAD, CI_RED_T2A,
    'EligibleCells: Worker anthropic/subagent/claude-sonnet-5-5 con effort high; Controller openai/process/gpt-6-luna con effort high. Vencen el 2026-12-29 '
    '(model-catalog.md:14, :51, :97). Se comprueban en cada emision y delegacion (A7).',
    'Invariantes comunes (cada contrato los numera INV-PBT2x-07 a 12 y 14). Lo normativo va en el contrato; las notas no (v2, V2-27).',
    'Las propias comunes (con el prefijo de cada contrato): C01, C02 (una guarda del section 4 que no queda en verde vuelve al Owner, OB-T1B.3), C03 (un '
    'cambio observable de RACKEDITAR, RPB o el menu: STOP y vuelta al Owner), C05 (un CT necesitaria AutoCAD, un modal real, el perfil o el disco fuera de '
    'ArchivoTemporal), C06, C07 (el Worker no ajusta lo esperado), C09 y C10 (corregir algo fuera del alcance: el piloto, el panel, el host core, "Editar '
    'todo", la escritura del panel, el campo de escala del panel, T-2, M-07, pb-21-cola, AUTH-13, PB-TC o PB-TC2).',
    'ExpectedEvidence comun (v2, V2-28):',
] + E_COMMON
EXPECT_61 = [
    'AllowedWriteScope (4): ' + ', '.join(ALLOWED[:-1]) + ' y ' + ALLOWED[-1] + '.',
    'ForbiddenWriteScope: 87 de src/ y lo comun.', 'Authorities: las comunes; ninguna UNIT_CHANGE.',
    'StopConditions: las 17 estandar, C08, C11, C12, C17, las comunes, y:', E_T2A_PKG, LB_PKG,
]
EXPECT_52 = [
    'Regimen: el RED es el esqueleto inerte y las dos clases nuevas, que fallan por asercion salvo la guarda CT-PBT2A-04, que pasa en el RED y discrimina '
    'por sus autopruebas (INV-F4T3A-13). El GREEN solo toca los dos archivos de src/.',
    'PushBackDraftApplyStatus, PushBackDraftApplyResult e IPushBackDraftStatePort, completos;',
    'PushBackDraftSessionPort: el constructor no guarda ni comprueba nada; ApplyToDraft devuelve new PushBackDraftApplyResult(PushBackDraftApplyStatus.NotEditable, '
    'false); LaunchFor devuelve null;',
    'PushBackDraftEditorLaunch: el constructor no asigna ni comprueba nada.',
    'Las clases (tests/RackCad.Tests/, namespace RackCad.Tests): PushBackDraftStatePortTests (01, 02 y 03) y PushBackDraftPortSharedReferenceGuardTests (04).',
]
CT_METHODS = ['CT_PBT2A_01_ThePortHandsACopyOfTheEditedStateToTheDraftAndNothingElse',
              'CT_PBT2A_02_TheLaunchCarriesTheStateTheIdentityTheSourceAndThePortAndRefusesWhatIsMissing',
              'CT_PBT2A_03_ThePortRoundTripIsLosslessAndSharesNothingOutsideTheTwoDeclaredReferences',
              'CT_PBT2A_04_NoFileOfThePortMutatesTheResolvedRackOrTheModuleReport']
API_EXCEPTIONS = {
    '### D-PBT2-02. ': ['`throw new ArgumentNullException(nameof(draft));`', '`private readonly DraftSession<PushBackEditableState> draft;`',
                        '`throw new ArgumentNullException(nameof(edited));`', 'llama **solo** a `draft.SetCurrentDraftState(edited)`',
                        '`return new PushBackDraftEditorLaunch(draft.CurrentDraftState, draft.RackId, sourceProject, new PushBackDraftSessionPort(draft));`'],
    '### D-PBT2-03. ': ['`if (state == null) throw new ArgumentNullException(nameof(state));`',
                        '`if (string.IsNullOrWhiteSpace(rackId)) throw new ArgumentException("A rack id is required.", nameof(rackId));`',
                        '`if (port == null) throw new ArgumentNullException(nameof(port));`'],
}
PKG_CHECKS = []
REQ_PKG_ROWS = None
if PKG_OK:
    SEC61, SEC60, SEC1, SEC52 = section('### 6.1 '), section('### 6.0 '), section('## 1. '), section('### 5.2 ')
    F61, F60, F1, F52 = ([md_fold([l]) for l in s if l.strip()] for s in (SEC61, SEC60, SEC1, SEC52))
    # el Objective, entre las comillas angulares, con v<n>
    _obj = [l for l in SEC61 if l.startswith('- **Objective** (ASCII): ' + chr(0xAB))]
    assert len(_obj) == 1 and ascii_fold(_obj[0][_obj[0].index(chr(0xAB)) + 1:_obj[0].rindex(chr(0xBB))]) == OBJECTIVE_PKG, 'el Objective del paquete difiere'
    for x in EXPECT_61:
        once(F61, x, 'section 6.1')
    for x in EXPECT_60:
        once(F60, x, 'section 6.0')
    for x in EXPECT_52:
        once(F52, x, 'section 5.2')
    # los invariantes propios: exactamente seis vinetas y una nota entre "Invariantes propios" y "StopConditions"
    _a = SEC61.index('- **Invariantes propios:**')
    _b = [k for k, l in enumerate(SEC61) if l.startswith('- **`StopConditions`:**')][0]
    _inv_lines = [l for l in SEC61[_a + 1:_b] if l.strip()]
    assert len(_inv_lines) == 7 and md_fold([_inv_lines[6]]) == INV_NOTE, ('los invariantes propios del section 6.1: seis y su nota', len(_inv_lines))
    assert [inv_bullet(l, []) for l in _inv_lines[:6]] == INV_OWN, ('los invariantes propios del paquete difieren', [inv_bullet(l, []) for l in _inv_lines[:6]])
    assert sum(1 for l in SEC61 if l.strip().startswith('- **Nota (no va al contrato).**')) == 1, 'las notas del section 6.1'
    # las paradas propias: exactamente tres vinetas entre "StopConditions" y "ExpectedEvidence"
    _c = [k for k, l in enumerate(SEC61) if l.startswith('- **`ExpectedEvidence`:**')][0]
    _stop_lines = [md_fold([l]) for l in SEC61[_b + 1:_c] if l.strip()]
    assert _stop_lines == ['PBT2A-C04: ' + C04_PKG, 'PBT2A-C13 (v2, V2-26): ' + C13_PKG, 'PBT2A-C15: ' + C15_PKG], ('las paradas propias del section 6.1', _stop_lines)
    # los invariantes comunes del section 6.0: las vinetas 07 a 12 y 14, con sus subvinetas, sin otra vineta
    _h = [k for k, l in enumerate(SEC60) if l.startswith('**Invariantes comunes**')][0]
    _blk = []
    for l in SEC60[_h + 1:]:
        if not l.strip():
            break
        _blk.append(l)
    _items = []
    for l in _blk:
        if l.startswith('- '):
            _items.append([l, []])
        else:
            assert l.startswith('  - ') and _items, ('linea suelta en los invariantes comunes', l[:80])
            _items[-1][1].append(l)
    assert [inv_bullet(f, s) for f, s in _items] == INV_COMMON, ('los invariantes comunes del paquete difieren', [inv_bullet(f, s) for f, s in _items])
    # P-01 en la Snapshot J, y los identificadores de las 17 estandar en el texto
    assert re.findall(r'\b([SP]-\d\d)\b', STD_LINE.replace('P-01 a P-08', ' '.join('P-0%d' % n for n in range(1, 9)))) == STD_IDS + ['P-01'], STD_LINE
    # la API de D-PBT2-02 y D-PBT2-03: los nombres declarados en sus bloques de codigo y el campo draft son, uno a uno, los de PBT2A-C13
    API_NAMES = []
    for head in ('### D-PBT2-02. ', '### D-PBT2-03. '):
        sec = section(head)
        joined = '\n'.join(sec)
        for frag in API_EXCEPTIONS[head]:
            assert frag in joined, ('la sentencia literal de', head, frag)
        blocks = re.findall(r'```csharp\n(.*?)```', joined, re.S)
        assert len(blocks) == 1, ('el bloque de codigo de', head)
        cur = None
        for raw in blocks[0].split('\n'):
            s = raw.strip()
            if not s or s.startswith('///') or s in ('{', '}'):
                continue
            mt = re.match(r'^public (?:readonly |sealed |static )*(enum|struct|interface|class) (\w+)(?:\s*:\s*\w+)?(.*)$', s)
            if mt:
                cur = mt.group(2)
                API_NAMES.append(cur)
                if mt.group(1) == 'enum':
                    API_NAMES += [x.split('=')[0].strip() for x in mt.group(3).strip().strip('{}').split(',')]
                continue
            mc = re.match(r'^public (\w+)\s*\(', s)
            if mc:
                assert mc.group(1) == cur, ('constructor de otro tipo', s)
                continue
            mm = re.match(r'^(?:public )?(?:static )?[\w<>\.]+ (\w+)\s*(\(|\{)', s)
            assert mm, ('declaracion sin leer en', head, s)
            API_NAMES.append(mm.group(1))
    API_NAMES.append('draft')  # el campo privado de la semantica de D-PBT2-02
    # ApplyToDraft se declara en la interfaz y en el adaptador: un nombre, dos declaraciones
    assert len(C13_NAMES) == len(set(C13_NAMES)) and sorted(set(API_NAMES)) == sorted(C13_NAMES), (
        'PBT2A-C13: los nombres no son, uno a uno, los de la API de D-PBT2-02 y D-PBT2-03', sorted(set(API_NAMES) ^ set(C13_NAMES)))
    assert API_NAMES.count('ApplyToDraft') == 2 and len(API_NAMES) == len(C13_NAMES) + 1, API_NAMES
    # el texto de DR:145 es el de D-PBT2-03; y CT-PBT2A-02 lo espera
    assert '"A rack id is required."' in TEXT[DR][144] and "EXCEPCION ArgumentException: A rack id is required. (Parameter 'rackId')" in '\n'.join(F52)
    # los cuatro CT de la lista cerrada, en su tabla, y las quince autopruebas de G-L1c en CT-PBT2A-04
    _ct = [l for l in F52 if l.startswith('| 0')]
    assert [re.match(r'^\| (0\d) (\w+) \|', l).group(2) for l in _ct] == CT_METHODS, 'los CT de la lista cerrada del section 5.2'
    assert ('autoprueba-<x>=falla para ' + SELFTESTS15 + ' |') in _ct[3] and 'las quince autopruebas de G-L1c' in _ct[3], 'CT-PBT2A-04: las quince autopruebas'
    # PBT2B-C13 (section 6.2): los nombres de INV-PBT2B-03 que V2-05 comprueba
    _c13b = [l for l in section('### 6.2 ') if l.startswith('  - **PBT2B-C13**')]
    assert len(_c13b) == 1 and all(('`' + n + '`') in _c13b[0] for n in NEW_T2B), 'los nombres de PBT2B-C13'
    # section 1: las filas de las autoridades; la fila de PB-T2 de [OD], literal en la cabecera del paquete
    for k, (c1, c2) in S1.items():
        once(F1, '| %s | %s |' % (c1, c2), 'section 1, ' + k)
    assert ('> ' + OD_ROW) in PKG_LINES[:20], 'la fila de PB-T2 de [OD] en la cabecera'
    assert '(`F4B-DRAFTS/pbt2/independent-review-v1.md`, citada **IR1**)' in '\n'.join(PKG_LINES[:10]), 'la ruta de IR1 en la cabecera'
    # v2.2: la cabecera cita la comprobacion acotada de PB-T2a por su ruta (CA-T2a, D-2) y su veredicto; el registro v2.1 -> v2.2 tiene V22-01 a 04
    _hdr = '\n'.join(PKG_LINES[:12])
    assert ('(`' + BOUNDED_CHECK[len(PILOT) + 1:] + '`, citada **CA-T2a**) da **EMITIBLE TRAS CORRECCIONES, 0/0/6**') in _hdr, 'la v2.2 en la cabecera: CA-T2a'
    _reg = section('## Registro de cambios v2.1 ')
    assert [re.match(r'^\| (V22-0\d) \|', l).group(1) for l in _reg if l.startswith('| V22-')] == ['V22-01', 'V22-02', 'V22-03', 'V22-04'], 'el registro V22'
    assert (chr(0xAB) + C11_REF_NEW + chr(0xBB)) in [l for l in _reg if l.startswith('| V22-01 |')][0], 'V22-01: la referencia nueva de C11'
    REQ_PKG_ROWS = SEC61
    PKG_CHECKS = ['Objective', 'AllowedWriteScope y alcance (6.1)', 'section 6.0 (%d lineas)' % len(EXPECT_60), 'esqueleto y clases (5.2)',
                  'cabecera y registro de la v2.2 (V22-01 a 04, CA-T2a)',
                  'INV propios literales sin nota', 'paradas C04, C13 y C15', 'INV comunes 07-12 y 14', 'P-01 J', 'API de D-PBT2-02/03 = nombres de C13',
                  'sentencias de excepcion', 'CT-PBT2A-01..04 y las quince autopruebas', 'PBT2B-C13 (V2-05)', 'section 1 (%d filas)' % len(S1),
                  'ExpectedEvidence comun y de 6.1', 'linea base de la CI', 'custodia', 'F4B-E25', 'RequiredTests']


def package_required(sec):
    """La tabla de RequiredTests del section 6.1: (proyecto, filtro, minimo del paquete, ExpectRed, nuevo en negrita)."""
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


# ---------------------------------------------------------------- RequiredTests: (proyecto, clase o prefijo, minimo del paquete, ExpectRed, clase)
# clase: 'new' (no existe en la emision; en negrita en el paquete) o 'fixed' (el minimo del paquete, comprobado contra el recuento)
UIP, CP = UT + 'RackCad.UI.Tests.csproj', TR + 'RackCad.Tests.csproj'
REQ_SPEC = [
    ('Core', 'PushBackDraftStatePortTests', 3, True, 'new'),
    ('Core', 'PushBackDraftPortSharedReferenceGuardTests', 1, False, 'new'),
    ('Core', 'PushBackEditableState', 13, False, 'fixed'),
    ('Core', 'PushBackEditorOutputGate', 1, False, 'fixed'),
    ('Core', 'PushBackSharedReferenceGuardTests', 2, False, 'fixed'),
    ('Core', 'PushBackWindowOnStateSourceGuardTests', 1, False, 'fixed'),    # [T1c]
    ('Core', 'PushBackWindowSharedReferenceGuardTests', 1, False, 'fixed'),  # [T1c]
    ('Core', 'Workspace', 3271, False, 'fixed'),
    ('Core', 'NamespaceFolderGuardTests', 7, False, 'fixed'),
    ('Core', 'PushBackClassicCharacterization', 10, False, 'fixed'),
    ('Core', 'PushBackFullEditorCharacterization', 3, False, 'fixed'),
    ('UI', 'PushBackWindowOnStateTests', 2, False, 'fixed'),                 # [T1c]
    ('UI', 'PushBackEditableStateParityTests', 1, False, 'fixed'),
    ('UI', 'PushBackStateNet', 14, False, 'fixed'),
    ('UI', 'PushBackClassicCharacterization', 55, False, 'fixed'),
    ('UI', 'PushBackFullEditorCharacterization', 16, False, 'fixed'),
    ('UI', 'PushBackDeclaredCorrection', 14, False, 'fixed'),
    ('UI', 'Cantilever', 67, False, 'fixed'),
]
if REQ_PKG_ROWS is not None:
    _pkg_req = package_required(REQ_PKG_ROWS)
    assert [(p, f, m, e, k == 'new') for p, f, m, e, k in REQ_SPEC] == _pkg_req, ('la tabla de RequiredTests del section 6.1 cambio', _pkg_req)
assert len(REQ_SPEC) == 18 and [r for r in REQ_SPEC if r[3]] == REQ_SPEC[:1]
# las clases de cada CT (section 5.2) son los MinSelected de las dos clases nuevas: 3 y 1
assert [r[2] for r in REQ_SPEC[:2]] == [3, 1] and [os.path.basename(p)[:-3] for p in NEW_TESTS] == [r[1] for r in REQ_SPEC[:2]]
# ningun filtro sin ExpectRed contiene una clase con ExpectRed (FullyQualifiedName~ es "contiene")
for p, f, _, er, _ in REQ_SPEC:
    for p2, f2, _, er2, _ in REQ_SPEC:
        if er2 and not er and p == p2:
            assert ('RackCad.Tests.' + f) not in ('RackCad.Tests.' + f2 + '.'), ('un filtro sin ExpectRed contiene la clase nueva', f, f2)

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
    """El criterio de count_tests_git.py (Fact = 1; Theory = sus InlineData; MemberData/ClassData aparte; Skip aparte), sobre los objetos de la revision."""
    root = UT if project == 'UI' else TR
    ns_prefix = 'RackCad.UI.Tests' if project == 'UI' else 'RackCad.Tests'
    files = [p for p in tracked(root) if p.endswith('.cs')]
    files = [p for p in files if '/bin/' not in p and '/obj/' not in p]
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
# CA-T2a section 3, paso 4: el recuento de los RequiredTests no cambia respecto al seco sobre d8ccbcb8 (clases, ejecutables, skip, MemberData)
COUNTS_D8 = {
    ('Core', 'PushBackDraftStatePortTests'): (0, 0, 0, 0), ('Core', 'PushBackDraftPortSharedReferenceGuardTests'): (0, 0, 0, 0),
    ('Core', 'PushBackEditableState'): (3, 13, 0, 0), ('Core', 'PushBackEditorOutputGate'): (1, 1, 0, 0),
    ('Core', 'PushBackSharedReferenceGuardTests'): (1, 2, 0, 0), ('Core', 'PushBackWindowOnStateSourceGuardTests'): (1, 1, 0, 0),
    ('Core', 'PushBackWindowSharedReferenceGuardTests'): (1, 1, 0, 0), ('Core', 'Workspace'): (74, 3271, 0, 33),
    ('Core', 'NamespaceFolderGuardTests'): (1, 7, 0, 0), ('Core', 'PushBackClassicCharacterization'): (1, 7, 0, 1),
    ('Core', 'PushBackFullEditorCharacterization'): (3, 3, 0, 0), ('UI', 'PushBackWindowOnStateTests'): (1, 2, 0, 0),
    ('UI', 'PushBackEditableStateParityTests'): (1, 1, 0, 0), ('UI', 'PushBackStateNet'): (6, 14, 0, 0),
    ('UI', 'PushBackClassicCharacterization'): (6, 55, 0, 0), ('UI', 'PushBackFullEditorCharacterization'): (6, 16, 2, 0),
    ('UI', 'PushBackDeclaredCorrection'): (7, 14, 0, 0), ('UI', 'Cantilever'): (3, 67, 0, 0),
}
assert sorted(COUNTS_D8) == sorted(COUNTS)
COUNTS_CHANGED = ['%s %s %s -> %s' % (p, f, COUNTS_D8[(p, f)], COUNTS[(p, f)]) for p, f in COUNTS if COUNTS[(p, f)] != COUNTS_D8[(p, f)]]
soft(not COUNTS_CHANGED, 'CA-T2a section 3, paso 4: el recuento de los RequiredTests cambio desde d8ccbcb8 (la sesion lo explica y pone al dia '
                         'COUNTS_D8): ' + '; '.join(COUNTS_CHANGED))
_prev_req = {}
for prev in (pbt1b, pbt1c):
    _prev_req.update({(r['Project'], r['Filter']): r['MinSelected'] for r in prev['RequiredTests']})
REQ, REQ_REPORT = [], []
for p, f, pkg_min, er, kind in REQ_SPEC:
    proj = UIP if p == 'UI' else CP
    filt = 'FullyQualifiedName~%s.%s' % ('RackCad.UI.Tests' if p == 'UI' else 'RackCad.Tests', f)
    classes, run, skip, member = COUNTS[(p, f)]
    note = None
    if kind == 'new':
        assert classes == 0 and run == 0, ('la clase nueva ya existe', f)
    else:
        assert classes > 0, ('el filtro no selecciona ninguna clase en la revision', p, f)
        if run < pkg_min:
            # una Theory con MemberData no se cuenta en estatico: vale si un contrato anterior (PB-T1b o PB-T1c) fija el mismo filtro con ese minimo
            assert member > 0 and _prev_req.get((proj, filt)) == pkg_min, (
                'el recuento medido de %s %s (%d, %d MemberData) es menor que el minimo del paquete (%d)' % (p, f, run, member, pkg_min))
            note = ('     estatico %d (+%d Theory con MemberData) < %d del paquete: vale porque un contrato anterior fija el mismo filtro con %d '
                    '(casos de MemberData contados por la CI)' % (run, member, pkg_min, pkg_min))
        elif run > pkg_min:
            note = '     estatico %d > %d del paquete: el MinSelected es el del paquete' % (run, pkg_min)
    REQ.append((proj, filt, pkg_min, er))
    REQ_REPORT.append('%-4s %-45s %-6s paquete=%-5s medido: clases=%d ejecutables=%d skip=%d memberdata=%d -> MinSelected=%d ExpectRed=%s'
                      % (p, f, kind, pkg_min, classes, run, skip, member, pkg_min, er))
    if note:
        REQ_REPORT.append(note)
for proj, _, _, _ in REQ:
    assert in_head(proj), proj
for proj, filt, mins, er in REQ:  # los filtros heredados no bajan su MinSelected
    if (proj, filt) in _prev_req:
        assert _prev_req[(proj, filt)] <= mins, (filt, _prev_req[(proj, filt)], mins)

# ---------------------------------------------------------------- invariantes: los propios (section 6.1) y los comunes (section 6.0), literales


def inv_text(idd, title, body):
    return 'INV-PBT2A-%s%s: %s' % (idd, (' (%s)' % lc(title)) if title else '', body)


INV = [inv_text(*x) for x in INV_OWN + INV_COMMON]
INV_IDS = ['01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12', '14']
assert len(INV) == 13 and [i.split(' ')[0].rstrip(':') for i in INV] == ['INV-PBT2A-' + n for n in INV_IDS]
assert not [i for i in INV if 'Nota' in i], 'una nota en los invariantes'

# ---------------------------------------------------------------- la evidencia (la comun del section 6.0 y la de PB-T2a del section 6.1)
_r, _s8, _core, _ui, _skip = CI_BASE_T1B
assert PB1B_GREEN.startswith(_s8) and (_core, _ui, _skip) == (19086, 2437, 21)
_c1, _u1 = _core + CI_T1C_DELTA[0], _ui + CI_T1C_DELTA[1]
assert (_c1, _u1, _c1 + 4) == (19088, 2439, 19092)  # los numeros de LB_PKG
assert '`44438060317`' in show(DISP) and nspace('Core 19 086 y UI 2 437') in nspace(show(DISP)), 'la CI del cierre de PB-T1b en F4B-E26'
# ---- ci_base_check (inicio): run_pbt2a_checks.py extrae este bloque y lo prueba con commits reales
CI_T1C_EXPECTED = (19088, 2439, 21)  # la prevision del section 6.1 para el cierre de PB-T1c: (Core Passed, UI total, UI Skipped)


def ci_base_check(git, ci, close, explained):
    """CA-T2a M-1 (section 3, paso 3): la linea base de la CI del cierre de PB-T1c, atada a PT1C_CLOSE. Devuelve los errores (vacia si vale)."""
    if not close:
        return ['CI_BASE_T1C sin PT1C_CLOSE: la linea base se ata al cierre de PB-T1c']
    if not (isinstance(ci, tuple) and len(ci) == 5 and re.fullmatch(r'\d+', str(ci[0])) and re.fullmatch(r'[0-9a-f]{8,40}', str(ci[1]))
            and all(isinstance(x, int) for x in ci[2:])):
        return ['CI_BASE_T1C mal formada: %r' % (ci,)]
    run, sha, core, ui, skip = ci
    if git('cat-file', '-e', sha + '^{commit}').returncode != 0:
        return ['el SHA de la CI no esta en el almacen de objetos: ' + sha]
    errors = []
    if git('diff', '--quiet', sha, close, '--', 'src', 'tests').returncode != 0:
        errors.append('el SHA de la CI (%s) no tiene el src y los tests del cierre de PB-T1c (%s)' % (sha[:8], close[:8]))
    if (core, ui, skip) != CI_T1C_EXPECTED and not explained:
        errors.append('las cifras de la CI del cierre de PB-T1c (Core %d, UI %d, Skipped %d) difieren de las previstas (Core %d, UI %d, Skipped %d): '
                      'la sesion las explica por el diff de PB-T1c en CI_T1C_EXPLAINED' % ((core, ui, skip) + CI_T1C_EXPECTED))
    return errors
# ---- ci_base_check (fin)


assert CI_T1C_EXPECTED == (_c1, _u1, _skip)
soft(CI_BASE_T1C, 'falta CI_BASE_T1C (la CI del cierre de PB-T1c): la ExpectedEvidence lleva la linea base prevista del section 6.1')
if CI_BASE_T1C:
    _ci_err = ci_base_check(git, CI_BASE_T1C, PT1C_CLOSE, CI_T1C_EXPLAINED)
    assert not _ci_err, ('CA-T2a M-1', _ci_err)
    _run, _sha, _cc, _uu, _ss = CI_BASE_T1C
    CI_TXT = ('la linea base del cierre de PB-T1c (run %s sobre %s: Core %d Passed; UI %d resultados, %d Skipped y %d Passed): Core + 4 (CT-PBT2A-01 a '
              '04) = %d Passed; UI sin cambio = %d resultados: 0 Failed, %d Skipped y %d Passed, o la diferencia explicada por el diff'
              % (_run, _sha[:8], _cc, _uu, _ss, _uu - _ss, _cc + 4, _uu, _ss, _uu - _ss))
    if (_cc, _uu, _ss) != CI_T1C_EXPECTED:
        assert all(ord(ch) < 0x80 for ch in CI_T1C_EXPLAINED), 'CI_T1C_EXPLAINED tiene que ser ASCII'
        CI_TXT += ' (frente a la prevista del section 6.1, Core %d y UI %d con %d Skipped: %s)' % (CI_T1C_EXPECTED + (CI_T1C_EXPLAINED,))
else:
    CI_TXT = ('la linea base prevista del cierre de PB-T1c (section 6.1 del paquete: el cierre de PB-T1b medido, run %s sobre %s, Core %d Passed y UI %d '
              'resultados con %d Skipped, mas PB-T1c, Core + 2 y UI + 2): Core + 4 (CT-PBT2A-01 a 04) = %d Passed; UI sin cambio = %d resultados: 0 '
              'Failed, %d Skipped y %d Passed, o la diferencia explicada por el diff' % (_r, _s8, _core, _ui, _skip, _c1 + 4, _u1, _skip, _u1 - _skip))


def cap(x):
    return x[0].upper() + x[1:]


def dot(x):
    return x[:-1] + '.' if x.endswith(';') else x


EVID = [
    cap(dot(E_ANTES)),
    CI_RED_HEAD + ' ' + dot(CI_RED_T2A),
    cap(dot(E_RED)),
    RED_CHAIN_HEAD + ' ' + dot(RED_CHAIN_T2A),
    cap(E_GREEN[:-1]) + ', con los conteos de ' + CI_TXT + '.',
    cap(dot(E_LOCAL)),
    cap(dot(E_ENTREGA)),
    cap(E_DESPUES),
    'De PB-T2a (section 6.1 del paquete): ' + E_T2A_PKG[len('ExpectedEvidence: la comun, con '):],
]
assert 'build-plugin sale omitido por su needs' in EVID[1] and 'que es el trailer real de los commits RED y GREEN' in EVID[6]

# ---------------------------------------------------------------- paradas: las 17 estandar, las comunes del section 6.0 y las propias del section 6.1
T1C_STOP = {s['Id']: s['Text'] for s in pbt1c['StopConditions']}


def prefixed(text):
    """El texto de [PT1] section 6.0 en el contrato de PB-T1c, cambiando solo el prefijo (section 6.0 del paquete)."""
    return text.replace('PBT1C', 'PBT2A').replace('PB-T1c', 'PB-T2a')


C08, C11, C12 = prefixed(T1C_STOP['PBT1C-C08']), prefixed(T1C_STOP['PBT1C-C11']), prefixed(T1C_STOP['PBT1C-C12'])
assert C08 == T1C_STOP['PBT1C-C08'] and C12 == T1C_STOP['PBT1C-C12'] and C11.endswith('neither one-off acceptance extends to PB-T2a')
assert 'PBT2A-NC2-EXACT-WHITELIST-01' in C11 and 'SATISFIED_UNDER_PBT2A_NC1_IDENTITY_SIGNAL_SUBSTITUTE' in C11 and 'PBT1C' not in C11
# CA-T2a M-4 (v2.2, V22-01): las diferencias admitidas de cada firma estan en [PT1] section 6.0, no en el section 6.0 de este paquete
assert C11.count('in ' + C11_REF_OLD + ';') == 1, 'la referencia de C11 a las diferencias admitidas'
C11 = C11.replace('in ' + C11_REF_OLD + ';', 'in ' + C11_REF_NEW + ';')
assert C11_REF_OLD not in C11.replace(C11_REF_NEW, '')
_c17 = T1C_STOP['PBT1C-C17']
assert _c17.count(' In PB-T1c the window adopts') == 1 and _c17.endswith(' The Worker chooses none of the three exits: the Owner does')
C17 = (_c17[:_c17.index(' In PB-T1c the window adopts')] + ' The lifetime includes that of the WORKSPACE window (section 6.0 of the package).'
       + ' The Worker chooses none of the three exits: the Owner does')
assert 'inside the editing or draft lifetime' in C17 and 'This is NOT authorization for general shallow copying.' in C17
C02 = T1C_STOP['PBT1C-C02'].replace('PB-TC2, PB-T1a or PB-T1b test', 'PB-TC2, PB-T1a, PB-T1b or PB-T1c test').replace(' (SUS3C-0n)', '')
assert 'PB-T1c test' in C02 and 'SUS3' not in C02
C07 = T1C_STOP['PBT1C-C07'].replace('the anchors of D-PBT1-13 (section 6.0)', 'the anchors of section 6.0')
assert C07 != T1C_STOP['PBT1C-C07'] and C07.endswith('the Worker does not adjust the expectation')
STOPS = P_STOPS + [
    {'Id': 'PBT2A-C01', 'Text': 'doing it right would require writing outside the AllowedWriteScope (the two new files of Application, PushBackDraftStatePort.cs and '
                                'PushBackDraftEditorLaunch.cs, and the two new Core test classes PushBackDraftStatePortTests and PushBackDraftPortSharedReferenceGuardTests)'},
    {'Id': 'PBT2A-C02', 'Text': C02},
    {'Id': 'PBT2A-C03', 'Text': 'an observable change of RACKEDITAR, RACKPUSHBACK (RPB) or the menu against the base appears (in PB-T2a only two new files of '
                                'Application are added; the window, the pilot, the host core, the drafts core, the Plugin and the files of PB-T1b do not change): '
                                'STOP and back to the Owner (D-16, OB-RE.1)'},
    {'Id': 'PBT2A-C04', 'Text': C04_PKG},
    {'Id': 'PBT2A-C05', 'Text': 'a CT would need AutoCAD, a real modal, the user profile or the disk outside ArchivoTemporal'},
    {'Id': 'PBT2A-C06', 'Text': T1C_STOP['PBT1C-C06']},
    {'Id': 'PBT2A-C07', 'Text': C07},
    {'Id': 'PBT2A-C08', 'Text': C08},
    {'Id': 'PBT2A-C09', 'Text': T1C_STOP['PBT1C-C09']},
    {'Id': 'PBT2A-C10', 'Text': 'doing it right would require correcting something outside the scope: the pilot, the panel, the host core, Editar todo, the panel '
                                'write, the scale field of the panel, T-2, M-07, pb-21-cola, AUTH-13, PB-TC or PB-TC2'},
    {'Id': 'PBT2A-C11', 'Text': C11},
    {'Id': 'PBT2A-C12', 'Text': C12},
    {'Id': 'PBT2A-C13', 'Text': C13_PKG},
    {'Id': 'PBT2A-C15', 'Text': C15_PKG},
    {'Id': 'PBT2A-C17', 'Text': C17},
]
assert [s['Id'] for s in STOPS[17:]] == ['PBT2A-C%02d' % n for n in range(1, 14)] + ['PBT2A-C15', 'PBT2A-C17'] and len(STOPS) == 32
for s in STOPS[17:]:  # ninguna parada de PB-T1c copiada sin adaptar
    assert not re.search(r'PBT1C-|SUS3C|In PB-T1c', s['Text']), s['Id']

# ---------------------------------------------------------------- autoridades (section 1 y section 6.0, por ruta, seccion y clase)
D2_ = PILOT + '/F4B-DRAFTS/pbt2/'
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
PT1 = 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md'
PV5 = 'docs/initiatives/I-64-proposal-v5.md'
T1BPKG = 'docs/initiatives/I-64-f4-t1b-gate-contract-package.md'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
assert title_version(lines_of(PT1)) == 'v2.8', '[PT1] no es la v2.8 (section 0 del paquete)'
_ph = lines_of(PHASE)
assert _ph[476].startswith('## 4. Los dos modos') and _ph[503].startswith('| **PB-T2 ') and '(OB-01.3)' in _ph[492], '[P] :477-493 y :504'
_pv = lines_of(PV5)
assert _pv[556].startswith('### D-15 ') and 'Sin cambio observable' in _pv[585] and '**STOP**' in _pv[592], 'la Proposal V5 :557-582 y :586-593'
_close_txt = (' (' + PT1C_ENTRY + ')') if PT1C_ENTRY else ' (por rellenar)'
F4B_E25 = CO + 'owner-f4b-correction-unit-change-decision.md'
# las rutas que se custodian con la emision de PB-T2a y son autoridades (section 6.0, Custodia y Authorities); los nombres que el paquete no fija,
# por el patron de PB-T1c (DUDAS de generator-notes-pbt2a.md)
CUSTODY = [
    (PKG, 'archivo completo (' + PKG_VERSION + '): los registros de cambios v2.1 -> v2.2 (V22-01 a V22-04: la referencia de PBT2x-C11, CA-T2a M-4), '
          'v2 -> v2.1 (V21-01 a V21-07) y v1 -> v2; section 1 (autoridades), section 2 '
          '(hechos), section 3 (D-PBT2-01 a D-PBT2-03, el puerto y el lanzamiento tipado, y D-PBT2-14, D2), section 4 (las guardas), section 5.0 y 5.2 '
          '(el regimen, las recetas, el esqueleto y CT-PBT2A-01 a 04), section 6.0 y 6.1 (este contrato), section 8 (las decisiones del Owner) y section '
          '10 (arrastres)'),
    (MEAS, 'el registro de mediciones del paquete, apartados A a Q (Q-7: la punta 833ea6bf); los registros G, H y Q son los anclajes de PB-T2a'),
    (D2_ + 'v2-delta.md', 'de la v1 a la v2 del paquete, hallazgo por hallazgo (IR1)'),
    (D2_ + 'independent-review-v1.md', 'la revision independiente de la v1 (IR1: NECESITA v2, 0/8/16) y su recomprobacion de la v2 (IR1-R, section 10: LISTO '
                                       'PARA EMITIR TRAS REMEDICION, 0/0/7), aplicada en la v2.1'),
    (BOUNDED_CHECK, 'la comprobacion acotada de PB-T2a (CA-T2a; section 6.0 del paquete: sobre el paquete v2.1, la section 6.1 y la salida de sus '
                    'anclajes): ' + (BOUNDED_RESULT or 'resultado por rellenar') + ' (M-1 a M-4 y la limpieza de F4B-E25 de M-5 en el generador, M-4 '
                    'tambien en la v2.2, V22-01; M-6, en el paquete antes de emitir PB-T2b); su section 3, las comprobaciones de la emision tras el cierre '
                    'de PB-T1c'),
]
assert len(CUSTODY) == 5 and len({p for p, _ in CUSTODY}) == 5 and CUSTODY[4][0] == D2_ + 'independent-review-pbt2a-v21-check.md'
# las de CUSTODY_EXTRA: se custodian y no son autoridades, todas en F4B-DRAFTS/pbt2/ (v22-delta.md: v2.2, V22-02)
CUSTODY_EXTRA = [D2_ + n for n in ('v21-delta.md', 'v22-delta.md', 'measure_pbt2.py', 'measure_pbt2_v2.py', 'forbidden_pbt2.py', 'check_questions_v2.py',
                                   'independent-review-v1-prompt.md', 'pbt2a-anchors-out.txt', 'independent-review-pbt2a-v21-check-prompt.md',
                                   'make_pbt2a_contract-dry.txt')]
assert len(CUSTODY_EXTRA) == 10 == len(set(CUSTODY_EXTRA))
EXISTING = [
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
    (CO + 'owner-f4b-dpb07-extensiondata-decisions.md', 'pregunta 1 (F4B-E23): ' + S1['E23'][1] + '; pregunta 2 (F4B-E24): ' + S1['E24'][1]),
    (PILOT + '/F4B-DRAFTS/pbfull/owner-return-dpb07.md', 'la vuelta al Owner por D-PB-07, respondida por F4B-E23'),
    (PILOT + '/F4B-DRAFTS/pbfull/owner-return-extensiondata.md', 'la vuelta al Owner por la ExtensionData anidada, respondida por F4B-E24'),
    # CA-T2a M-5: la celda del section 1, sin repetir las lineas del section 6.0
    (F4B_E25, 'F4B-E25: ' + S1['E25'][1] + '. Las tres condiciones las comprueba el Controller, y no hace falta reemitir el contrato para corregir '
              '(section 6.0, Las correcciones). En PB-T2a no hay rutas UNIT_CHANGE: sus dos archivos no existen en la AuthorityRevision y no son '
              'autoridades (section 6.0)'),
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-verify-stop.md', 'la vuelta al Owner por la parada de la primera verificacion del intento 2 de PB-T1b (16.3), respondida por F4B-E25'),
    (OWNER_PBT2, 'archivo completo (' + OWNER_PBT2_ENTRY + '): Q-PBT2-1 a Q-PBT2-4 (las decisiones de Q-PBT2-1 a 4 del section 6.0, Custodia), las cuatro '
                 '"(Recomendado)"; la pregunta 4, "Si, con esa condicion", es la autorizacion condicional de emision de PB-T2a, PB-T2b y PB-T2c; la pregunta 3, '
                 '"En PB-T4, con prueba ya", deja fuera del alcance de PB-T2a el traslado de la validacion de campos (section 8.2 del paquete)'),
    (DISP, 'archivo completo: F4B-E01 a ' + DISP_LAST),
    (PB1B_TCP, 'el cierre de PB-T1b, F4B-E26: ' + S1['E26'][1] + '; ' + lc(S1['CLOSES'][1])),
    (PB1C_TCP, 'el cierre de PB-T1c' + _close_txt + ': ' + S1['CLOSES'][1] + '; la ventana sobre la que se comprueba V2-05 (section 6.0)'),
    (PBT1CC, 'F4B-E27, el contrato emitido de PB-T1c: ' + S1['E27'][1] + '; sus listas curadas de tests/, la raiz, las paradas estandar, C08, C11, C12, C17 y las celdas'),
    (PV5, 'la Proposal V5: D-15 (:557-582): ' + S1['PV5D15'][1] + '; D-16 e INV-19 (:586-593): ' + S1['PV5D16'][1]),
    (T1BPKG, 'T1B: ' + S1['T1B'][1]),
    (PILOT + '/F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md', 'el cierre de PB-TC: ' + S1['CLOSES'][1]),
    (PILOT + '/F4B-PBTC2-DECLARED-CORRECTIONS/task-close-package.md', 'el cierre de PB-TC2: ' + S1['CLOSES'][1]),
    (PILOT + '/F4B-PBT1A-LOSSLESS-STATE-NET/task-close-package.md', 'el cierre de PB-T1a: ' + S1['CLOSES'][1]),
]
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D7, D8, D12 y D13: ' + S1['ADR'][1]),
    ('docs/AUTOMATION_PLAN.md', 'section 16 (16.3 a 16.11; 16.3 en una correccion, con F4B-E25): ' + S1['PROTO'][1]),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4): ' + S1['PROTO'][1]),
    ('docs/automation/agent-execution/routing.md', 'archivo completo: ' + S1['PROTO'][1]),
    ('docs/automation/agent-execution/model-catalog.md', 'las celdas medidas y vigentes (:14, :51 y :97: vencen el ' + CELLS_EXPIRY + '): ' + S1['PROTO'][1]),
]
assert len(EXISTING) == 28 and len(EXTERNAL) == 5
assert len({p for p, _ in CUSTODY + EXISTING + EXTERNAL}) == len(CUSTODY + EXISTING + EXTERNAL)
for p, _ in EXISTING + EXTERNAL:
    if p != PB1C_TCP:
        assert in_head(p), ('autoridad inexistente en la revision', p)
soft(in_head(PB1C_TCP), 'el cierre de PB-T1c (autoridad) aun no esta en la revision: ' + PB1C_TCP)
# ninguna UNIT_CHANGE en PB-T2a (section 6.0): sus dos archivos no existen y no son autoridades
assert not {p for p, _ in CUSTODY + EXISTING + EXTERNAL} & set(ALLOWED)
_close = (' (' + PT1C_ENTRY + ')') if PT1C_ENTRY else ' (<cierre de PB-T1c por rellenar>)'
c = {
    'Schema': 'rackcad-gate-contract/v1', 'Unit': 'I-64', 'Gate': 'F4B', 'TaskId': TASK,
    'Objective': OBJECTIVE,
    'AuthorityRevision': AR, 'MainSha': MAIN,
    'Authorities': [{'Path': p, 'Section': s, 'Class': 'UNIT_DOC'} for p, s in CUSTODY + EXISTING] + [
        {'Path': CONTRACT_REL, 'Section': 'este contrato', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'EXTERNAL'} for p, s in EXTERNAL],
    'AllowedWriteScope': ALLOWED,
    'ForbiddenWriteScope': FORBIDDEN,
    'Invariants': INV,
    'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
    'ExpectedEvidence': EVID, 'StopConditions': STOPS, 'EligibleCells': cells, 'RoutingEnforcement': 'required', 'CorrectionsAuthorized': True,
    'IssuedBy': ('Sesion principal de I-64 (Claude) por la decision del Owner ' + OWNER_PBT2_ENTRY + ', pregunta 4, la emision condicional de PB-T2a, PB-T2b '
                 'y PB-T2c (0 bloqueantes y 0 importantes sin resolver en su comprobacion acotada, y la remedicion sobre el cierre de PB-T1c sin cambio '
                 'material), con las preguntas 1 a 3 (docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt2-decisions.md): la emision '
                 'de la primera tarea de PB-T2 tras el cierre de PB-T1c' + _close + ', sus anclajes y su comprobacion acotada; F4B-E11, D2 '
                 '(owner-d1-d2-decisions.md); las decisiones de procedimiento owner-f4b-procedure-decisions.md (F4B-E03); F4B-E19, la Snapshot J '
                 '(owner-config-snapshot-j-decision.md); F4B-E21, las colecciones opacas que pone la sesion (owner-f4b-opaque-collections-decision.md); '
                 'F4B-E23, D-PB-07 (owner-f4b-dpb07-extensiondata-decisions.md); F4B-E25, la regla de F4B sobre las rutas UNIT_CHANGE en una correccion '
                 '(owner-f4b-correction-unit-change-decision.md); y el paquete del contrato de PB-T2 ' + PKG_VERSION + ', con su revision independiente '
                 '(IR1), su recomprobacion (IR1-R) y la comprobacion acotada de PB-T2a'),
    'IssuedUtc': ISSUED,
}
N_AUTH = len(CUSTODY) + len(EXISTING) + 1 + len(EXTERNAL)
assert len(c['Authorities']) == N_AUTH == 39, len(c['Authorities'])
assert not [a for a in c['Authorities'] if a['Class'] == 'UNIT_CHANGE']
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
# lo que depende de la emision de PB-T2a: el paquete y el registro custodiados, el resultado de la comprobacion acotada y las rutas de custodia
soft(PKG_HEAD_VERSION == PKG_VERSION, 'el paquete custodiado en la revision es %s; al escribir hace falta la %s en HEAD (%s)' % (PKG_HEAD_VERSION, PKG_VERSION, PKG))
MEAS_OK = in_head(MEAS) and ('\n### %s. ' % MEAS_SECTION) in show(MEAS).replace('\r\n', '\n')
soft(MEAS_OK, 'el registro (%s) no esta en la revision o no tiene el apartado %s' % (MEAS, MEAS_SECTION))
soft(BOUNDED_RESULT, 'falta BOUNDED_RESULT (el resultado de la comprobacion acotada de PB-T2a, section 6.0)')
assert BOUNDED_RESULT is None or (BOUNDED_RESULT.startswith('EMITIBLE') and all(ord(ch) < 0x80 for ch in BOUNDED_RESULT)), BOUNDED_RESULT
for p, _ in CUSTODY[2:]:
    soft(in_head(p), 'a custodiar con la emision de PB-T2a, aun no en la revision: %s' % p)
for p in CUSTODY_EXTRA:
    soft(in_head(p), 'a custodiar con la emision, sin ser autoridad, aun no en la revision: %s' % p)
if in_head(BOUNDED_CHECK):  # la comprobacion custodiada es la que da BOUNDED_RESULT: sin bloqueantes ni importantes (F4B-E28, pregunta 4)
    _bc = show(BOUNDED_CHECK)
    assert '**Veredicto: EMITIBLE TRAS CORRECCIONES' in _bc and '0 BLOQUEANTES / 0 IMPORTANTES / 6 MENORES' in _bc and '## ' + chr(0xA7) + '3.' in _bc, (
        'la comprobacion acotada custodiada no es la de BOUNDED_RESULT', BOUNDED_CHECK)
# CA-T2a D-5 y section 3, paso 5: la ultima entrada de f4b-dispositions.md es la emision de PB-T2a y dice que se cumplen las condiciones de F4B-E28,
# pregunta 4 (la comprobacion acotada sin bloqueantes ni importantes y la remedicion sin cambio material)
_last = re.split(r'(?m)^## ', _disp)[-1]
EMIT_ENTRY_OK = bool(re.match(r'F4B-E\d+\. Emisi(?:o|' + chr(0xF3) + r')n de F4B-PBT2A-DRAFT-STATE-PORT', _last)) and OWNER_PBT2_ENTRY in _last and 'pregunta 4' in _last
soft(EMIT_ENTRY_OK, 'CA-T2a D-5: la ultima entrada de f4b-dispositions.md (%s) no es la emision de F4B-PBT2A-DRAFT-STATE-PORT con las condiciones de %s, '
                    'pregunta 4' % (DISP_LAST, OWNER_PBT2_ENTRY))
# CA-T2a section 3, paso 6: MainSha = origin/main, recien leido
ORIGIN_MAIN = git('rev-parse', 'origin/main').stdout.decode('ascii').strip()
soft(ORIGIN_MAIN == MAIN, 'CA-T2a section 3, paso 6: MainSha (%s) no es origin/main (%s)' % (MAIN[:8], ORIGIN_MAIN[:8]))
if not DRY:
    assert git('merge-base', '--is-ancestor', AR, REV).returncode == 0, 'la AuthorityRevision no es la revision ni un antepasado suyo'
# CA-T2a section 3: el estado de cada paso de la remedicion (lo que el generador no puede ver lo comprueba la sesion)
REMEDIATION = [
    '1 el cierre: ' + (ANCHOR_T1C if PT1C_CLOSE else 'pendiente (PT1C_CLOSE)'),
    '2 V2-05: listas y literales de PB-T1c OK; capturas del estado y nombres de PBT2B-C13 OK; la ventana: '
    + ('OK' if T1C_WINDOW and all(_checks.values()) else 'pendiente del cierre') + ('; ' + FORM_NOTE if FORM_NOTE else ''),
    '3 la linea base de la CI: ' + ('atada al cierre, %s' % ('cifras previstas' if CI_BASE_T1C[2:] == CI_T1C_EXPECTED else 'cifras explicadas')
                                    if CI_BASE_T1C else 'pendiente (CI_BASE_T1C; que sea la corrida push 4/4 de la punta lo comprueba la sesion)'),
    '4 el producto: Application/Domain/Plugin == a18ba345 OK; G-L1c == 95373af6 OK; recuentos == d8ccbcb8: ' + ('OK' if not COUNTS_CHANGED else 'cambian'),
    '5 la custodia: %d AVISOS de emision' % sum(1 for w in WARN if 'custodi' in w or 'registro' in w or 'D-5' in w),
    '6 el entorno: MainSha == origin/main: %s; celdas hasta %s; Test-Json al escribir; config.toml = Snapshot J, el binario y una sola delegacion '
    'abierta: la sesion' % (ORIGIN_MAIN == MAIN, CELLS_EXPIRY),
    '7 la autorizacion (F4B-E28, pregunta 4): BOUNDED_RESULT = %s; la remedicion sin cambio material y PB-T1c sin intento 3: la sesion' % BOUNDED_RESULT,
]
if DRY:
    print('DRY rev', REV_SHA[:8], '(objetos del checkout principal)' if OBJ_REV else '(HEAD del worktree)', 'allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN),
          '(src', len(SRCF), 'tests', len(TESTF), '[core', len(CORE_T), 'ui', str(len(UI_T)) + '] root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'required', len(REQ), 'evidence', len(EVID), 'authorities', len(c['Authorities']), 'anchors-ok', len(ANCHORS),
          'api-ok', sum(len(v) for v in API.values()), 'bytes', len(b), 'sha256', hashlib.sha256(b).hexdigest().upper())
    print('anclaje:', ANCHOR_SRC, '|', ANCHOR_T1C, '| f4b-dispositions hasta', DISP_LAST, '|', OWNER_PBT2_ENTRY, 'custodiada con Q-PBT2-1..4 (Recomendado)')
    print('paquete leido:', PKG_SRC, '->', PKG_READ_VERSION, '| en la revision:', PKG_HEAD_VERSION, '| registro con %s:' % MEAS_SECTION, MEAS_OK)
    print('textos de la %s comprobados:' % PKG_VERSION, ', '.join(PKG_CHECKS) if PKG_CHECKS else 'ninguno')
    print('G-L1c: %d autopruebas en :223-235 y :238-239 (%s); huellas AllObjects :278 y Fingerprint :1080; iguales que en %s' % (
        len(SELFTESTS), SELFTESTS15, PB1B_CLOSE[:8]))
    print('V2-05: listas de las guardas de PB-T1c sin editable ni los %d nombres de INV-PBT2B-03: %s; literales con editable en sus tres clases: %d, '
          'ninguno fija una asignacion; ventana: %s; suscripciones al estado en W y WC: %d; autopruebas de window_checks: %d (las 4 de forma pasan y '
          'las demas fallan, como deben)' % (
              len(NEW_T2B), ', '.join('%s %d' % (k, len(v)) for k, v in GUARD_LISTS.items()), EDITABLE_LITERALS, EDITABLE_REPORT, len(_subs), WINDOW_SELFTESTS))
    print('D1: needs de build-plugin =', _needs, '| D2: Snapshot J en la revision:', in_head(SNAP_J), '| celdas vigentes hasta', CELLS_EXPIRY)
    print('lectura D-1 (CA-T2a, aceptada): tests/ de Core = 18 de PB-T0 + 5 de PB-T1b + 2 de PB-T1c = %d; UI = %d' % (len(CORE_T), len(UI_T)))
    print('C11 (CA-T2a M-4; v2.2, V22-01): la referencia a las diferencias admitidas es "%s"' % C11_REF_NEW)
    print('remedicion al escribir (CA-T2a section 3):')
    for l in REMEDIATION:
        print('  ' + l)
    _free = [p for p in [p for p, _ in CUSTODY] + CUSTODY_EXTRA if not in_head(p)]
    print('custodia de PB-T2a: %d de %d rutas aun libres en la revision' % (len(_free), len(CUSTODY) + len(CUSTODY_EXTRA)))
    print('RequiredTests (criterio de count_tests_git.py sobre los objetos de la revision):')
    for l in REQ_REPORT:
        print('  ' + l)
    print('AVISOS (%d):' % len(WARN))
    for w in WARN:
        print('  - ' + w)
    if '--print' in ARGS:
        for i, f in enumerate(FORBIDDEN, 1):
            print(i, f)
    if '--print-json' in ARGS:  # los bytes exactos del contrato (sin la traduccion de fin de linea de la consola)
        sys.stdout.flush()
        sys.stdout.buffer.write(b)
    sys.exit(0)
assert PT1C_CLOSE and PT1C_ENTRY and CI_BASE_T1C and BOUNDED_RESULT, 'faltan PT1C_CLOSE, CI_BASE_T1C o BOUNDED_RESULT'
assert not WARN, WARN
print('remedicion (CA-T2a section 3):\n  ' + '\n  '.join(REMEDIATION) + ('\n  ' + FORM_NOTE if FORM_NOTE else ''))
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
