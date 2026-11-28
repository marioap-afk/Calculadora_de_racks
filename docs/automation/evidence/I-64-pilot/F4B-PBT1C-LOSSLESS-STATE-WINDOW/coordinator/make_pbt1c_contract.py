"""Contrato de gate de F4B-PBT1C-LOSSLESS-STATE-WINDOW (ASCII), desde el paquete del contrato de PB-T1 v2.8
(docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md, sections 4, 5.0, 5.4, 6.0 y 6.3; D-PBT1-13 a D-PBT1-15; V28-01 a V28-35 de su
registro de cambios) y el patron de make_pbt1b_contract.py y make_pbt1a_contract.py. La BaseSha es hija del cierre de PB-T1b (F4B-E26).
El ForbiddenWriteScope de src/ sale del algoritmo de [PKC-C] (cover()); los de tests/ y la raiz son las listas curadas del section 6.0: en
tests/RackCad.Tests/, la de PB-T0 (18 entradas; medicion H-12) mas las cinco clases de Core de PB-T1b; en tests/RackCad.UI.Tests/, la de
PB-T1b (la de [PKC2], las dos clases de PB-TC2, las siete de PB-T1a y Workspace/) mas la clase de UI de PB-T1b; la raiz, la de PB-T1b.
Comprueba sobre la revision leida (HEAD del worktree o, con --objects-rev, esa revision en el almacen de objetos del checkout principal):
- el anclaje del section 6.0: la revision desciende del cierre de PB-T1b (PB1B_CLOSE, de la constante o de --pb1b-close), con src igual al
  de ese cierre y al de los GREEN a18ba345 y a0a5f699, y src/RackCad.UI igual al GREEN de PB-TC2 (b5c09f0b);
- las anclas de D-PBT1-13 y D-PBT1-14 en W y WC (linea y texto), los rangos de INV-PBT1C-03 de la v2.8 (V28-08: cada rango empieza y
  acaba donde dice el paquete, documentacion incluida), los siete sitios de SyncName, los escritores de portadores de V28-11, las 40
  costuras de la medicion H, los literales de T-SG, G-10, I-55 y Fact3, y la API de PB-T1b que la ventana usa;
- INV-PBT1C-05 (V28-15, V28-16, V28-30 y V28-33): el censo de declaraciones de W y WC (325, con el criterio de
  check_inv_pbt1c_03_ranges.py) y que las cuatro subcadenas testigo son ASCII, estan en su linea, son unicas y van literales en el contrato;
- el texto del paquete (solo si el leido es la v2.8): el Objective, el AllowedWriteScope, la tabla de RequiredTests, INV-PBT1C-03,
  INV-PBT1C-05 y PBT1C-C13 (literales y SIN sus "Nota (no va al contrato)"), el RED de los filtros sin ExpectRed (V28-12), la
  ExpectedEvidence (V28-01 y V28-17) y la regla de F4B-E25 (V28-22) del section 6.3; las dieciseis autopruebas de CT-PBT1C-04 y la costura
  inerte del section 5.4; P-01 y las listas de custodia de PB-T1c (V28-18c) del section 6.0;
- los MinSelected, medidos con el criterio de count_tests.py (Fact = 1; Theory = sus InlineData; MemberData aparte) sobre los objetos
  de la revision: los fijos del paquete, como minimos; los del paquete "el medido en la emision", medidos;
- D1: build-plugin depende de tests en .github/workflows/ci.yml; D2: la Snapshot J en HEAD.
Lo unico que depende de la emision de PB-T1c (la v2.8 custodiada en HEAD, el apartado Q del registro en HEAD y las rutas de CUSTODY y
CUSTODY_EXTRA en HEAD) es AVISO en seco y error al escribir; todo lo demas, incluido el cierre de PB-T1b (F4B-E23 a F4B-E26, sus archivos y la
linea base de la CI, CI_BASE), es error siempre.
Uso: python make_pbt1c_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print] [--print-json]
                                   [--objects-rev <rev> [--repo <checkout principal>] [--package-file <paquete>]] [--pb1b-close <sha>]
Con --dry no escribe nada: comprueba e imprime los recuentos, los bytes y los AVISOS (con --print, ademas, el ForbiddenWriteScope numerado;
con --print-json, el contrato). --objects-rev exige --dry: no hace chdir al worktree y lee todo con git -C <checkout principal> show
<rev>:<ruta>. Sin --objects-rev hace chdir al worktree y lee HEAD. --package-file exige --objects-rev y --dry: los textos del paquete se
leen de ese archivo (la v2.8 sin custodiar) en lugar del paquete de la revision. Al escribir, el paquete es el custodiado en HEAD y tiene
que ser la v2.8. Al escribir, la sesion lo ejecuta en el worktree tras el commit que custodia la v2.8, el registro con Q y las rutas de
CUSTODY y CUSTODY_EXTRA (entre ellas make_pbt1c_contract-dry.txt, la salida de este --dry).
Al custodiarlo, las rutas locales se redactan como en PB-T0 ('<worktree-de-I-64>', '<checkout-principal>')."""
import json, os, re, sys, subprocess, hashlib, unicodedata

# ---------------------------------------------------------------- el cierre de PB-T1b (F4B-E26) y la emision de PB-T1c
PB1B_CLOSE = '95373af6116cf120bd0a8ba490833524e33c47da'  # la custodia del cierre de PB-T1b (F4B-E26); tambien --pb1b-close <sha>
PB1B_CLOSE_ENTRY = 'F4B-E26'  # la entrada de f4b-dispositions.md que cierra PB-T1b (v2.8, V28-24 y V28-29)
PB1B_GREEN = 'a18ba345eddc5333c22b8bacbad5c2ed4dbf5959'  # el GREEN acreditado de PB-T1b (intento 2): el src del cierre
PB1B_GREEN_A1 = 'a0a5f699b138c1b5251fb0c88d28e3ca9e1610c3'  # el GREEN del intento 1, con el mismo src (git diff 2f5d3dec a18ba345 -- src vacio)
CI_BASE = ('44438060317', 'a18ba345', 19086, 2437, 21)  # la CI del cierre de PB-T1b: (run, sha8, Core Passed, UI total, UI Skipped); V28-29h
PKG_VERSION = 'v2.8'       # la version del paquete que se custodia con la emision de PB-T1c
MEAS_PBT1C = 'Q'           # el apartado del registro con los anclajes de PB-T1c (v2.8, V28-18)
D3_C13_NEW_MEMBER = True   # D3: la v2.8 (V28-07, V28-14 y V28-33, PD-V28-01 opcion (A)): PBT1C-C13 para ante toda declaracion nueva o sobrecarga
MAIN_CHECKOUT = '<checkout-principal>'  # Redaccion para el repo publico: la ruta local del checkout principal  # solo con --objects-rev; se redacta al custodiar ('<checkout-principal>')

ARGS = sys.argv[1:]
VALUED = ('--objects-rev', '--repo', '--pb1b-close', '--package-file')


def opt(name):
    if name not in ARGS:
        return None
    i = ARGS.index(name)
    assert i + 1 < len(ARGS) and not ARGS[i + 1].startswith('--'), ('falta el valor de', name)
    return ARGS[i + 1]


POS = [a for i, a in enumerate(ARGS) if not a.startswith('--') and not (i > 0 and ARGS[i - 1] in VALUED)]
assert len(POS) == 3, 'uso: make_pbt1c_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print] [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
OBJ_REV = opt('--objects-rev')
PB1B_CLOSE = opt('--pb1b-close') or PB1B_CLOSE
DRY = '--dry' in ARGS
assert OBJ_REV is None or DRY, '--objects-rev solo en seco: anade --dry'
PKG_FILE = opt('--package-file')
assert PKG_FILE is None or (OBJ_REV is not None and DRY), '--package-file solo junto con --objects-rev y --dry'
PKG_FILE = os.path.abspath(PKG_FILE) if PKG_FILE else None  # antes de cualquier chdir
assert PKG_FILE is None or os.path.isfile(PKG_FILE), ('no existe el paquete', PKG_FILE)
# las celdas vencen el 2026-12-29 (model-catalog.md:14, :51 y :97; precondicion 6 del paquete)
assert re.fullmatch(r'[0-9]{4}-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9]Z', ISSUED) and ISSUED[:10] < '2026-12-29', ISSUED
assert re.fullmatch(r'[0-9a-f]{40}', AR) and re.fullmatch(r'[0-9a-f]{40}', MAIN), (AR, MAIN)
assert PB1B_CLOSE is None or re.fullmatch(r'[0-9a-f]{40}', PB1B_CLOSE), PB1B_CLOSE

if OBJ_REV is None:
    WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree
    os.chdir(WT)
    REPO, REV = None, 'HEAD'
else:
    REPO, REV = opt('--repo') or MAIN_CHECKOUT, OBJ_REV
DISK = OBJ_REV is None  # solo en el worktree se mira tambien el disco

TASK = 'F4B-PBT1C-LOSSLESS-STATE-WINDOW'
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'
TC2_GREEN = 'b5c09f0bd741f67b7dd8311dce65c26e3d4cf3c4'
F4B_E25 = PILOT + '/F4B-DRAFTS/coordinator/owner-f4b-correction-unit-change-decision.md'  # F4B-E25 (custodiada en 95373af6)
F4B_E25_RET = PILOT + '/F4B-DRAFTS/pbt1b/owner-return-verify-stop.md'                     # su vuelta
WARN = []


def git(*args, inp=None):
    return subprocess.run(['git'] + (['-C', REPO] if REPO else []) + list(args), input=inp, capture_output=True)


_ALL = None  # los archivos de la revision (un solo ls-tree -r -z: sin las comillas de core.quotePath) y sus carpetas


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


def show(path):
    r = git('show', REV + ':' + path)
    assert r.returncode == 0, ('no se puede leer', REV, path)
    return r.stdout.decode('utf-8')


def lines_of(path):
    return show(path).replace('\r\n', '\n').split('\n')


def tracked(prefix):
    return [p for p in _all()[0] if p.startswith(prefix)]


def soft(cond, msg):
    """Lo que solo existira tras el cierre de PB-T1b o con la emision de PB-T1c: error al escribir, AVISO en seco."""
    if cond:
        return True
    assert DRY, msg
    WARN.append(msg)
    return False


REV_SHA = git('rev-parse', REV).stdout.decode('ascii').strip()
assert re.fullmatch(r'[0-9a-f]{40}', REV_SHA), ('revision desconocida', REV)

# ---------------------------------------------------------------- anclaje del section 6.0: src frente al cierre de PB-T1b y src/RackCad.UI frente a PB-TC2
assert git('diff', '--quiet', TC2_GREEN, REV, '--', 'src/RackCad.UI').returncode == 0, 'src/RackCad.UI cambio desde el GREEN de PB-TC2 (b5c09f0b)'
# el cierre de PB-T1b, de la constante o de --pb1b-close; la revision desciende de el y su src es el de los GREEN de PB-T1b (F4B-E26)
assert PB1B_CLOSE, 'falta PB1B_CLOSE (la constante o --pb1b-close)'
assert git('cat-file', '-e', PB1B_CLOSE + '^{commit}').returncode == 0, ('el cierre de PB-T1b no esta en el almacen de objetos', PB1B_CLOSE)
assert git('merge-base', '--is-ancestor', PB1B_CLOSE, REV).returncode == 0, 'la revision no desciende del cierre de PB-T1b'
assert git('diff', '--quiet', PB1B_CLOSE, REV, '--', 'src').returncode == 0, 'src cambio desde el cierre de PB-T1b'
assert git('diff', '--quiet', PB1B_GREEN, REV, '--', 'src').returncode == 0, 'src difiere del GREEN acreditado de PB-T1b (a18ba345)'
assert git('diff', '--quiet', PB1B_GREEN_A1, REV, '--', 'src').returncode == 0, 'src difiere del GREEN del intento 1 de PB-T1b (a0a5f699)'
# y es de verdad el cierre: en el ya estan la entrada que lo cierra (F4B-E26) y el task-close-package de PB-T1b
_disp_close = git('show', PB1B_CLOSE + ':' + PILOT + '/F4B-DRAFTS/f4b-dispositions.md')
assert _disp_close.returncode == 0 and ('\n## %s.' % PB1B_CLOSE_ENTRY) in _disp_close.stdout.decode('utf-8').replace('\r\n', '\n'), (
    'PB1B_CLOSE no es el cierre de PB-T1b: su f4b-dispositions.md no tiene ' + PB1B_CLOSE_ENTRY, PB1B_CLOSE)
assert git('cat-file', '-e', PB1B_CLOSE + ':' + PILOT + '/F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md').returncode == 0, (
    'PB1B_CLOSE no es el cierre de PB-T1b: no tiene su task-close-package.md', PB1B_CLOSE)
ANCHOR_SRC = 'src == cierre de PB-T1b (%s, con %s) == GREEN a18ba345 == a0a5f699' % (PB1B_CLOSE[:8], PB1B_CLOSE_ENTRY)

# ---------------------------------------------------------------- los contratos anteriores
PBT1BC = PILOT + '/F4B-PBT1B-LOSSLESS-STATE-TYPE/coordinator/gate-contract.json'
PBT1AC = PILOT + '/F4B-PBT1A-LOSSLESS-STATE-NET/coordinator/gate-contract.json'
PBT0C = PILOT + '/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json'
PBTCC = PILOT + '/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json'
PBTC2C = PILOT + '/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json'
T1BC = PILOT + '/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json'
pbt1b = json.loads(show(PBT1BC))
pbt1a = json.loads(show(PBT1AC))
pbt0 = json.loads(show(PBT0C))
assert pbt1b['TaskId'] == 'F4B-PBT1B-LOSSLESS-STATE-TYPE' and pbt1a['TaskId'] == 'F4B-PBT1A-LOSSLESS-STATE-NET'

# Las 17 estandar, de PB-T1b, con P-01 en la Snapshot J (D2 de la v2.8; F4B-E19).
SNAP_I = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json'
SNAP_J = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-J/snapshot-j.json'
P_STOPS = [dict(s) for s in pbt1b['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
assert [s['Id'] for s in P_STOPS] == ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
P01 = 'config.toml no longer matches the accepted Snapshot in force (today Snapshot %s, %s)'
# el contrato de PB-T1b custodiado cita la I; si se reemitiera con la J, tambien vale
assert P_STOPS[9]['Text'] in (P01 % ('I', SNAP_I), P01 % ('J', SNAP_J)), P_STOPS[9]['Text']
P_STOPS[9]['Text'] = P01 % ('J', SNAP_J)
assert [s for s in P_STOPS if s['Id'] != 'P-01'] == [s for s in pbt1a['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id']) and s['Id'] != 'P-01']
assert in_head(SNAP_J), ('la Snapshot J no esta en la revision', SNAP_J)

# ---------------------------------------------------------------- el alcance
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
APP = 'src/RackCad.Application/Systems/PushBack/'
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
WC = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs'
WX = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml'
PRODUCT = [W, WC]
NEW_UI = UT + 'PushBackWindowOnStateTests.cs'
NEW_SG = TR + 'PushBackWindowOnStateSourceGuardTests.cs'
NEW_D2 = TR + 'PushBackWindowSharedReferenceGuardTests.cs'
NEW = [NEW_UI, NEW_SG, NEW_D2]
ALLOWED = PRODUCT + NEW
assert len(ALLOWED) == 5 == len(set(ALLOWED))
assert all(in_head(p) and (not DISK or os.path.isfile(p)) for p in PRODUCT), PRODUCT
assert all(not in_head(p) and (not DISK or not os.path.exists(p)) for p in NEW), [p for p in NEW if in_head(p)]
# los prefijos nuevos estan libres en tests/
for pref in ('PushBackWindowOnState', 'PushBackWindowSharedReferenceGuardTests'):
    assert not [p for p in tracked('tests/') if pref in p], ('el prefijo ya existe', pref)


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
# la forma de PB-TC2, con WX: 59, igual que la de PB-T1a (section 6.0)
assert len(SRCF) == 59, len(SRCF)
assert SRCF == [f for f in pbt1a['ForbiddenWriteScope'] if f.startswith('src/')], 'src/ difiere del de PB-T1a'
assert WX in SRCF and {'src/RackCad.Application/', 'src/RackCad.Domain/', 'src/RackCad.Plugin/'} <= set(SRCF)
# lo que INV-PBT1C-01 exige vacio cae entero en el ForbiddenWriteScope
for p in ('src/RackCad.Application/', 'src/RackCad.Domain/', 'src/RackCad.Plugin/', 'src/RackCad.UI/Editor/', 'src/RackCad.UI/Controls/', WX):
    assert p in SRCF, p

# tests/: las listas de PB-T1b (la curada de PB-T0 en Core; la de PB-T1a en UI) mas las clases de PB-T1b (section 6.0).
CORE_T1B = [f for f in pbt1b['ForbiddenWriteScope'] if f.startswith(TR)]
CORE_T0 = [f for f in pbt0['ForbiddenWriteScope'] if f.startswith(TR)] + [f for f in pbt0['AllowedWriteScope'] if f.startswith(TR)]
assert CORE_T1B == CORE_T0 and len(CORE_T0) == 18, len(CORE_T1B)
T1B_NEW_CORE = [f for f in pbt1b['AllowedWriteScope'] if f.startswith(TR)]
T1B_NEW_UI = [f for f in pbt1b['AllowedWriteScope'] if f.startswith(UT)]
assert len(T1B_NEW_CORE) == 5 and len(T1B_NEW_UI) == 1, (T1B_NEW_CORE, T1B_NEW_UI)
UI_T1B = [f for f in pbt1b['ForbiddenWriteScope'] if f.startswith(UT)]
assert len(UI_T1B) == 61 and UI_T1B[-1] == UT + 'Workspace/', len(UI_T1B)
CORE_T = CORE_T0 + T1B_NEW_CORE
UI_T = UI_T1B[:-1] + T1B_NEW_UI + [UT + 'Workspace/']
assert len(CORE_T) == 23 == len(set(CORE_T)) and len(UI_T) == 62 == len(set(UI_T)), (len(CORE_T), len(UI_T))
TESTF = CORE_T + UI_T
assert not any(a == t or (t.endswith('/') and a.startswith(t)) for a in ALLOWED for t in TESTF)
ROOT = [f for f in pbt1b['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
assert ROOT == ['docs/', '.github/', 'eng/', 'tools/', 'assets/', 'deploy/', 'RackCad.sln', 'Directory.Build.props', 'Directory.Build.targets', 'global.json',
                'NuGet.Config', 'README.md', 'AGENTS.md', 'CLAUDE.md', '.editorconfig', '.gitattributes', '.gitignore'], ROOT
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == 161 == len(set(FORBIDDEN)), len(FORBIDDEN)
for f in FORBIDDEN:
    assert in_head(f) and (not DISK or os.path.exists(f.rstrip('/'))), ('no existe', f)

# ---------------------------------------------------------------- las anclas de D-PBT1-13 y D-PBT1-14 (W y WC en b5c09f0b): (archivo, linea, texto contenido)
ANCHORS = [
    # (1) el estado
    (W, 56, 'private readonly PushBackCompositeEditorState composite ='),
    (W, 57, 'new PushBackCompositeEditorState(new PushBackEditorState(), new PushBackEditorState());'),
    # (2) los reenvios
    (W, 84, 'private readonly List<SelectiveSafetySelection> safetySelections = new List<SelectiveSafetySelection>();'),
    (W, 101, 'private PalletSpecification generalPallet = PushBackEditorInputs.NewDesign().Pallet;'),
    (W, 106, 'private double rackTolerance = DynamicRackDefaults.DefaultPalletTolerance;'),
    (W, 107, 'private double rackPostPeralte = PushBackEditorInputs.NewDesign().PostPeralte;'),
    (W, 108, 'private double rackBeamDepth = DynamicRackDefaults.DefaultBeamDepth;'),
    (W, 109, 'private double rackAnnotationScale = 1.0;'),
    (W, 114, 'private readonly PushBackEditorInputs advanced = PushBackEditorInputs.NewDesign();'),
    (W, 380, 'private int? firstLevelDatum = (int)RackCad.Application.Systems.Shared.RackFirstLevelDatumMode.LowestUsablePunch;'),
    (W, 1249, 'private DimensionViewVisibility? loadedDimensionViews;'),
    (W, 1256, 'private string loadedDimensionStyle;'),
    (W, 1260, 'private bool dimensionViewsTouched;'),
    # (3) las cargas, con el orden de hoy
    (W, 270, 'public void LoadNew()'),
    (W, 273, 'composite.SetActiveSide(RackCad.Domain.Systems.PushBack.PushBackSide.A);'),
    (W, 274, 'composite.SetSideBPresent(false);'),
    (W, 275, 'composite.LoadComposite(null);'),
    (W, 276, 'var inputs = state.LoadNew();'),
    (W, 279, 'session.Identity.Adopt(null, null);'),
    (W, 280, 'LoadFromModel(inputs, string.Empty);'),
    (W, 286, 'safetySelections.Clear();'),
    (W, 287, 'safetySelections.AddRange(new PushBackSafetyAuthority(catalog).Defaults());'),
    (W, 294, 'public void LoadDesignForNew(PushBackDesign design, string rackName, RackProject sourceProject = null)'),
    (W, 299, 'composite.SetActiveSide(RackCad.Domain.Systems.PushBack.PushBackSide.A);'),
    (W, 301, 'var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);'),
    (W, 302, 'LoadCompositeFromDesign(design);'),
    (W, 305, 'session.Identity.Adopt(null, rackName);'),
    (W, 311, 'public void LoadExisting(PushBackDesign design, string rackId, string rackName, RackProject sourceProject = null)'),
    (W, 316, 'composite.SetActiveSide(RackCad.Domain.Systems.PushBack.PushBackSide.A);'),
    (W, 318, 'var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);'),
    (W, 319, 'LoadCompositeFromDesign(design);'),
    (W, 322, 'session.Identity.Adopt(rackId, rackName);'),
    (W, 3307, 'private void Restore_Click(object sender, RoutedEventArgs e)'),
    (W, 3320, 'composite.SetActiveSide(RackCad.Domain.Systems.PushBack.PushBackSide.A);'),
    (W, 3322, 'var inputs = state.LoadFromDesign(SideAOnly(design), assembler.Resolver);'),
    (W, 3327, 'LoadCompositeFromDesign(design, preserveDormantIntent: true);'),
    (W, 3328, 'LoadFromModel(inputs, NameBox.Text);'),
    (WC, 933, 'UpdateCompositeEnabled();'),
    # (4) LoadFromModel: las asignaciones a portadores que se quitan y las siembras que no cambian ni un caracter
    (W, 385, 'private void LoadFromModel(PushBackEditorInputs inputs, string rackName)'),
    (W, 387, 'suppressSync = true;'),
    (W, 388, 'try'),
    (W, 390, 'NameBox.Text = rackName ?? string.Empty;'),
    (W, 391, 'firstLevelDatum = inputs.FirstLevelDatum;'),
    (W, 392, 'var pallet = inputs.Pallet ?? new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg");'),
    (W, 393, 'generalPallet = pallet;'),
    (W, 394, 'DepthBox.SetNumber(pallet.Depth);'),
    (W, 395, 'WeightUnitBox.SelectedItem = string.IsNullOrWhiteSpace(pallet.WeightUnit) ? "kg" : pallet.WeightUnit;'),
    (W, 396, 'PalletsDeepBox.SetNumber(Math.Max(2, inputs.PalletsDeep));'),
    (W, 397, 'ToleranceBox.SetNumber(inputs.PalletTolerance > 0.0 ? inputs.PalletTolerance : DynamicRackDefaults.DefaultPalletTolerance);'),
    (W, 398, 'rackTolerance = inputs.PalletTolerance > 0.0 ? inputs.PalletTolerance : DynamicRackDefaults.DefaultPalletTolerance;'),
    (W, 399, 'PostBox.SelectedId = string.IsNullOrWhiteSpace(inputs.PostCatalogId) ? catalog?.Defaults?.Post : inputs.PostCatalogId;'),
    (W, 400, 'PostPeralteBox.SetNumber(inputs.PostPeralte);'),
    (W, 401, 'rackPostPeralte = inputs.PostPeralte;'),
    (W, 402, 'BeamDepthBox.SetNumber(inputs.BeamDepth > 0.0 ? inputs.BeamDepth : DynamicRackDefaults.DefaultBeamDepth);'),
    (W, 403, 'rackBeamDepth = inputs.BeamDepth > 0.0 ? inputs.BeamDepth : DynamicRackDefaults.DefaultBeamDepth;'),
    (W, 405, '// I-35 (Owner round 2): adopt the four advanced scopes the load recovered, then paint them.'),  # V28-11a: se quita con W:406-411
    (W, 406, 'advanced.ManualHeaderHeightOverride = inputs.ManualHeaderHeightOverride;'),
    (W, 411, 'advanced.SeparatorSpacingOverride = inputs.SeparatorSpacingOverride;'),
    (W, 413, 'var options = inputs.Annotations ?? new DynamicAnnotationOptions();'),
    (W, 414, 'loadedDimensionStyle = options.DimensionStyle;'),
    (W, 415, 'NumberFrontsCheck.IsChecked = options.NumberFronts;'),
    (W, 417, 'DrawRackNameCheck.IsChecked = options.DrawRackName;'),
    (W, 418, 'AnnotationScaleBox.SetNumber(options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0);'),
    (W, 419, 'rackAnnotationScale = options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0;'),
    (W, 420, 'DimensionsBox.SelectedIndex = Math.Min((int)DimensionDetail.Detailed, Math.Max(0, (int)options.Dimensions));'),
    (W, 421, 'ShowDimensionViews(options.DimensionViews);'),
    (W, 423, 'safetySelections.Clear();'),
    (W, 426, 'if (safety != null) safetySelections.Add(safety.DeepCopy());'),
    (W, 432, 'LoadAdvancedRackParameters();'),
    # (5) ReadInputs y SelectedDimensionViews
    (W, 823, 'InOutBeamCatalogId = CellInOutBeamBox.SelectedValue as string ?? cell.InOutBeamCatalogId,'),
    (W, 826, 'IntermediateBeamCatalogId = CellIntermediateBeamBox.SelectedValue as string ?? cell.IntermediateBeamCatalogId,'),
    (W, 833, 'private PushBackEditorInputs ReadInputs()'),
    (W, 838, 'generalPallet = new PalletSpecification('),
    (W, 840, 'generalPallet.Weight, generalPallet.WeightUnit);'),
    (W, 841, 'rackTolerance = Keep(ToleranceBox, rackTolerance);'),
    (W, 844, 'rackAnnotationScale = Keep(AnnotationScaleBox, rackAnnotationScale);'),
    (W, 852, 'generalPallet.Weight, WeightUnitBox.SelectedItem as string ?? "kg"),'),
    (W, 853, 'PalletsDeep = IntVal(PalletsDeepBox, DynamicRackDefaults.DefaultPalletsDeep),'),
    (W, 868, 'PostCatalogId = PostBox.SelectedId,'),
    (W, 874, 'NumberFronts = NumberFrontsCheck.IsChecked == true,'),
    (W, 876, 'DrawRackName = DrawRackNameCheck.IsChecked == true,'),
    (W, 878, 'Dimensions = (DimensionDetail)Math.Min((int)DimensionDetail.Detailed, Math.Max(0, DimensionsBox.SelectedIndex)),'),
    (W, 880, 'DimensionViews = SelectedDimensionViews() // I-50 C-12'),
    (W, 1291, 'private DimensionViewVisibility? SelectedDimensionViews()'),
    # V28-11b: los tres escritores de portadores fuera de LoadFromModel y ReadInputs, que no cambian
    (W, 1268, 'private void ShowDimensionViews(DimensionViewVisibility? views)'),
    (W, 1270, 'loadedDimensionViews = views;'),
    (W, 1271, 'dimensionViewsTouched = false;'),
    (W, 1300, 'private void DimensionViews_Changed(object sender, RoutedEventArgs e)'),
    (W, 1303, 'dimensionViewsTouched = true;'),
    (W, 1295, 'DimensionsFrontalCheck.IsChecked == true,'),
    (W, 1297, 'DimensionsPlantaCheck.IsChecked == true);'),
    # (6) lo que se retira
    (W, 342, 'private static PushBackDesign SideAOnly(PushBackDesign design)'),
    (W, 373, '}'),
    (WC, 813, 'private void LoadCompositeFromDesign(PushBackDesign design)'),
    (WC, 814, '=> LoadCompositeFromDesign(design, preserveDormantIntent: false);'),
    (WC, 827, 'private void LoadCompositeFromDesign(PushBackDesign design, bool preserveDormantIntent)'),
    (WC, 934, '}'),
    # (7) el criterio de bloqueo
    (WC, 783, 'private IReadOnlyList<PushBackCompositeDiagnostic> CompositeDiagnostics()'),
    (WC, 789, 'private string CompositeStatusOr(string fallback)'),
    (WC, 803, 'private bool CompositeHasBlocking(PushBackSystem system)'),
    (WC, 805, 'lastCompositeSystem = system;'),
    (WC, 806, 'return CompositeDiagnostics().Any(diagnostic => diagnostic.IsBlocking);'),
    # (8) el nombre
    (W, 1234, 'private void Name_Changed(object sender, RoutedEventArgs e)'),
    (W, 1237, 'session.Identity.SetName(NameBox.Text?.Trim());'),
    (W, 3334, 'private void InsertBatch_Click(object sender, RoutedEventArgs e)'),
    (W, 3352, 'session.Identity.SetName(NameBox.Text?.Trim());'),
    (W, 3363, 'private void RequestDraw(string view, int section, bool updateOnly)'),
    (W, 3405, 'session.Identity.SetName(NameBox.Text?.Trim());'),
    (W, 3481, 'private void SaveLibrary_Click(object sender, RoutedEventArgs e)'),
    (W, 3504, 'session.Identity.SetName(NameBox.Text?.Trim());'),
    # D-PBT1-14: los constructores y las firmas publicas no cambian
    (W, 131, 'public RackPushBackSystemWindow()'),
    (W, 136, 'public RackPushBackSystemWindow(bool canInsertInAutoCad, Func<string> newIdFactory = null)'),
    (W, 160, 'LoadNew();'),
    # (9) la costura: las de hoy, junto a las que se anade EditableStateForTest
    (W, 165, 'internal RackEditorSession<PushBackDesign, PushBackSystem> Session => session;'),
    (WC, 43, 'internal PushBackCompositeEditorState CompositeState => composite;'),
]
TEXT = {W: lines_of(W), WC: lines_of(WC)}
for path, line, text in ANCHORS:
    assert text in TEXT[path][line - 1].strip(), ('ancla', path, line, TEXT[path][line - 1].strip())
assert len(TEXT[W]) - (TEXT[W][-1] == '') == 3700 and len(TEXT[WC]) - (TEXT[WC][-1] == '') == 936, (len(TEXT[W]), len(TEXT[WC]))
assert 'using RackCad.Application.Systems.PushBack;' in TEXT[W]  # el RED es una sola linea en W: el using ya esta

DECL = re.compile(r"^\s{4,8}(?:\[[^\]]*\]\s*)*((?:public|private|internal|protected|static|readonly|override|virtual|sealed|async|partial|new|const|event)\s+)+"
                  r"[\w<>\[\]\?,\.\s]+?\b(\w+)\s*(\(|\{|=|;|=>)")


def members(lines):
    """Rangos de miembros (la heuristica de members.py: cuenta llaves a profundidad 2)."""
    depth, results, current = 0, [], None
    for i, raw in enumerate(lines, start=1):
        stripped = raw.strip()
        if depth == 2 and current is None:
            m = DECL.match(raw)
            if m and not stripped.startswith('//'):
                current = [m.group(2), i, i]
                if stripped.endswith(';') and '{' not in stripped:
                    results.append(tuple(current))
                    current = None
        s = re.sub(r'"(?:\\.|[^"\\])*"', '""', raw)
        s = re.sub(r"'(?:\\.|[^'\\])'", "''", s)
        s = s.split('//')[0]
        depth += s.count('{') - s.count('}')
        if current is not None:
            current[2] = i
            if depth == 2 and (stripped.endswith('}') or stripped.endswith(';')):
                results.append(tuple(current))
                current = None
    return results


# INV-PBT1C-03: los miembros de la lista existen en BaseSha (en su archivo) y las dos declaraciones nuevas no.
INV03_FIELDS = ['composite', 'safetySelections', 'generalPallet', 'rackTolerance', 'rackPostPeralte', 'rackBeamDepth', 'rackAnnotationScale', 'advanced',
                'firstLevelDatum', 'loadedDimensionViews', 'loadedDimensionStyle', 'dimensionViewsTouched']
INV03_W = ['LoadNew', 'LoadDesignForNew', 'LoadExisting', 'SideAOnly', 'LoadFromModel', 'ReadInputs', 'Name_Changed', 'SelectedDimensionViews',
           'Restore_Click', 'InsertBatch_Click', 'RequestDraw', 'SaveLibrary_Click']
INV03_WC = ['LoadCompositeFromDesign', 'CompositeDiagnostics', 'CompositeStatusOr', 'CompositeHasBlocking']
INV03_NEW = ['SyncName', 'EditableStateForTest']
INV03_FIELD_LINES = {'composite': 56, 'safetySelections': 84, 'generalPallet': 101, 'rackTolerance': 106, 'rackPostPeralte': 107, 'rackBeamDepth': 108,
                     'rackAnnotationScale': 109, 'advanced': 114, 'firstLevelDatum': 380, 'loadedDimensionViews': 1249, 'loadedDimensionStyle': 1256,
                     'dimensionViewsTouched': 1260}
assert sorted(INV03_FIELD_LINES) == sorted(INV03_FIELDS)
MW, MWC = members(TEXT[W]), members(TEXT[WC])
NW, NWC = [m[0] for m in MW], [m[0] for m in MWC]
# los campos, por su ancla: la heuristica de members.py no los separa todos (un comentario al final de la linea alarga el rango del anterior)
MEMBER_NOTES = []
for n, line in INV03_FIELD_LINES.items():
    assert re.search(r'\b%s\b\s*(=|;)' % n, TEXT[W][line - 1]) and sum(1 for l in TEXT[W] if re.search(r'^\s+private\b[^;=(]*\b%s\b\s*(=|;)' % n, l)) == 1, n
    owner = [m for m in MW if m[1] <= line <= m[2]]
    if not owner or owner[-1][0] != n:
        MEMBER_NOTES.append('%s (W:%d) cae, con la heuristica de members.py, en el rango de %s' % (
            n, line, ('%s W:%d-%d' % owner[-1]) if owner else 'ningun miembro'))
for n in INV03_W:
    assert NW.count(n) == 1, ('miembro de INV-PBT1C-03 en W', n, NW.count(n))
assert NWC.count('LoadCompositeFromDesign') == 2 and all(NWC.count(n) == 1 for n in INV03_WC[1:]), 'miembros de INV-PBT1C-03 en WC'
assert not [n for n in INV03_NEW if n in NW + NWC], 'una declaracion nueva de INV-PBT1C-03 ya existe'

# INV-PBT1C-03 de la v2.8 (V28-08): los rangos de BaseSha, documentacion incluida. (archivo, desde, hasta, miembros que contiene)
RANGES = [
    (W, 49, 57, ['composite']), (W, 84, 84, ['safetySelections']), (W, 94, 101, ['generalPallet']),
    (W, 103, 109, ['rackTolerance', 'rackPostPeralte', 'rackBeamDepth', 'rackAnnotationScale']), (W, 111, 114, ['advanced']),
    (W, 375, 380, ['firstLevelDatum']), (W, 1244, 1249, ['loadedDimensionViews']), (W, 1251, 1256, ['loadedDimensionStyle']),
    (W, 1258, 1260, ['dimensionViewsTouched']),
    (W, 268, 289, ['LoadNew']), (W, 291, 307, ['LoadDesignForNew']), (W, 309, 324, ['LoadExisting']), (W, 385, 440, ['LoadFromModel']),
    (W, 833, 890, ['ReadInputs']), (W, 1234, 1238, ['Name_Changed']), (W, 3304, 3330, ['Restore_Click']), (W, 3334, 3361, ['InsertBatch_Click']),
    (W, 3363, 3419, ['RequestDraw']), (W, 3481, 3515, ['SaveLibrary_Click']),
    (W, 325, 374, ['SideAOnly']), (W, 1288, 1298, ['SelectedDimensionViews']),  # lo que se retira, con su documentacion y sus dos lineas en blanco
    (WC, 779, 786, ['CompositeDiagnostics']), (WC, 788, 801, ['CompositeStatusOr']), (WC, 803, 807, ['CompositeHasBlocking']),
    (WC, 811, 934, ['LoadCompositeFromDesign', 'LoadCompositeFromDesign']),
]
assert sorted(n for r in RANGES for n in set(r[3])) == sorted(INV03_FIELDS + INV03_W + INV03_WC)


def code_of(raw):
    s = re.sub(r'"(?:\\.|[^"\\])*"', '""', raw)
    s = re.sub(r"'(?:\\.|[^'\\])'", "''", s)
    return s.split('//')[0]


def member_end(lines, d):
    """Ultima linea del miembro declarado en d (1-based): la llave que cierra lo que abre, o el ';' final si no abre llaves."""
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


# Los bordes en blanco que el paquete declara: lo que se retira de W, "con las dos lineas en blanco que lo rodean", y WC:811, la de arriba.
BLANK_EDGES = {(W, 325): ('start', 'end'), (W, 1288): ('start', 'end'), (WC, 811): ('start',)}


def check_range(path, a, b, names, edges):
    """Un rango de INV-PBT1C-03 empieza y acaba donde dice el paquete: devuelve los miembros (declaracion-final) o lanza AssertionError."""
    lines = TEXT[path]
    decls = [k for k in range(a, b + 1) if MEMBER_DECL.match(lines[k - 1])]
    named = [k for k in decls if any(re.search(r'\b%s\b\s*(\(|=|;|=>|$)' % n, lines[k - 1]) for n in names)]
    assert decls == named and len(named) == len(names), ('rango con otros miembros o sin los suyos', path, a, b, decls, named)
    spans = [(k, member_end(lines, k)) for k in named]
    covered = {x for s, e in spans for x in range(s, e + 1)}
    last = max(e for _, e in spans)
    # los bordes en blanco, exactamente los declarados
    assert (lines[a - 1].strip() == '') == ('start' in edges), ('borde de arriba', path, a)
    assert (lines[b - 1].strip() == '') == ('end' in edges), ('borde de abajo', path, b)
    start, end = (a + 1 if 'start' in edges else a), (b - 1 if 'end' in edges else b)
    # empieza en la primera linea de la documentacion del primer miembro, o en su declaracion: la linea anterior al rango no es comentario
    assert not is_comment(lines[a - 2]), ('el rango deja fuera documentacion por arriba', path, a)
    assert start == spans[0][0] or is_comment(lines[start - 1]), ('el rango no empieza en la documentacion ni en la declaracion', path, a)
    # acaba en el final del ultimo miembro
    assert end == last, ('el rango no acaba en el final del miembro', path, b, last)
    # dentro, fuera de los miembros, solo documentacion o lineas en blanco
    for k in range(a, b + 1):
        if k not in covered:
            assert lines[k - 1].strip() == '' or is_comment(lines[k - 1]), ('linea ajena dentro del rango', path, k, lines[k - 1].strip())
    return spans


RANGE_REPORT = []
for path, a, b, names in RANGES:
    spans = check_range(path, a, b, names, BLANK_EDGES.get((path, a), ()))
    RANGE_REPORT.append('%s:%d-%d %s (miembros %s)' % ('W' if path == W else 'WC', a, b, '/'.join(dict.fromkeys(names)),
                                                         ', '.join('%d-%d' % s for s in spans)))
# autopruebas en negativo de check_range: cada rango desplazado una linea hacia dentro o hacia fuera, por arriba o por abajo, tiene que fallar
RANGE_SELFTESTS = 0
for path, a, b, names in RANGES:
    edges = BLANK_EDGES.get((path, a), ())
    for a2, b2 in ((a + 1, b), (a - 1, b), (a, b - 1), (a, b + 1)):
        if a2 > b2:
            continue
        try:
            check_range(path, a2, b2, names, edges)
        except AssertionError:
            RANGE_SELFTESTS += 1
            continue
        raise AssertionError(('autoprueba de rango que no falla', path, a2, b2))
for path in (W, WC):
    rs = sorted((a, b) for p, a, b, _ in RANGES if p == path)
    assert all(rs[k][1] < rs[k + 1][0] for k in range(len(rs) - 1)), ('rangos solapados', path)
# V28-11b: las unicas asignaciones a un portador fuera de LoadFromModel y ReadInputs son W:1270, :1271 y :1303
CARRIERS = ['generalPallet', 'rackTolerance', 'rackPostPeralte', 'rackBeamDepth', 'rackAnnotationScale', 'firstLevelDatum', 'loadedDimensionViews',
            'loadedDimensionStyle', 'dimensionViewsTouched']
_assign = [k for k, l in enumerate(TEXT[W], 1) if re.match(r'^\s*(?:this\.)?(%s)\s*=(?!=)' % '|'.join(CARRIERS), l) and not (385 <= k <= 440 or 833 <= k <= 890)]
assert _assign == [1270, 1271, 1303], ('escritores de portadores fuera de LoadFromModel y ReadInputs', _assign)
assert not [l for l in TEXT[WC] if re.search(r'\b(%s)\s*=(?!=)' % '|'.join(CARRIERS), l)]
# D-PBT1-13 (8): los siete sitios de SetName y Adopt, exactamente
SITES = [i for i, l in enumerate(TEXT[W], 1) if l.strip().startswith(('session.Identity.SetName(', 'session.Identity.Adopt('))]
assert SITES == [279, 305, 322, 1237, 3352, 3405, 3504], SITES
assert not [l for l in TEXT[WC] if 'session.Identity.' in l]
# INV-PBT1C-06: las 40 costuras de la medicion H (grep -n -E "^\s+internal " de W y WC: 39 en W y CompositeState en WC)
SEAMS = [l for l in TEXT[W] if re.match(r'^\s+internal ', l)] + [l for l in TEXT[WC] if re.match(r'^\s+internal ', l)]
assert len(SEAMS) == 40, len(SEAMS)
# INV-PBT1C-04 y 05: los literales de T-SG (S-9 y S-7), G-10, I-55 y Fact3, en BaseSha
WT_TEXT = '\n'.join(TEXT[W])
assert WT_TEXT.count('UiSupport.PromptSaveToLibrary(this, NameBox.Text, "sistema")') == 1
assert WT_TEXT.count('new RackBomWindow(lastComputation.Bom) { Owner = this }.ShowDialog();') == 1
assert WT_TEXT.count('.RequestUpdate(') == 1 and '.RequestUpdate(' not in '\n'.join(TEXT[WC])
assert 'PhysicalPostIndex(LateralSectionBox.SelectedItem)' in WT_TEXT and 'corte.PostIndex == section' in WT_TEXT
assert 'forceRebuild:' not in WT_TEXT and 'forceRebuild)' not in WT_TEXT
SETNUMBER_W = sum(l.count('SetNumber(') for l in TEXT[W])
SETNUMBER_WC = sum(l.count('SetNumber(') for l in TEXT[WC])

# La API de PB-T1b que la ventana usa (D-PBT1-13 sobre D-PBT1-02 a D-PBT1-11), en el src de la revision.
ES_NEW = APP + 'PushBackEditableState.cs'
GATE = APP + 'PushBackEditorOutputGate.cs'
API = {
    ES_NEW: ['public PushBackCompositeEditorState Editor { get; }', 'public PalletSpecification Pallet { get; set; }', 'public int PalletsDeep { get; set; }',
             'public string PostCatalogId { get; set; }', 'public double PostPeralte { get; set; }', 'public double PalletTolerance { get; set; }',
             'public double BeamDepth { get; set; }', 'public double AnnotationScale { get; set; }', 'public int? FirstLevelDatum { get; set; }',
             'public bool NumberFronts { get; set; }', 'public bool NumberLevels { get; set; }', 'public bool DrawRackName { get; set; }',
             'public DimensionDetail Dimensions { get; set; }', 'public string DimensionStyle { get; set; }',
             'public DimensionViewVisibility? LoadedDimensionViews { get; set; }', 'public bool DimensionViewsTouched { get; set; }',
             'public bool DimensionsFrontal { get; set; }', 'public bool DimensionsLateral { get; set; }', 'public bool DimensionsPlanta { get; set; }',
             'public PushBackEditorInputs Advanced { get; }', 'public List<SelectiveSafetySelection> SafetySelections { get; }', 'public string Name { get; set; }',
             'public PushBackEditorInputs LoadNewEditor()', 'public PushBackEditorInputs LoadDesignEditor(PushBackDesign design, PushBackResolver resolver)',
             'public PushBackEditorInputs RestoreEditor(PushBackDesign lastValidDesign, PushBackResolver resolver)',
             'public void AdoptCarriers(PushBackEditorInputs inputs, RackCatalog catalog)', 'public void SeedDefaultSafety(RackCatalog catalog)',
             'public PushBackEditorInputs BuildInputs(PushBackEditorDesignAssembler assembler)'],
    GATE: ['public static IReadOnlyList<PushBackCompositeDiagnostic> Diagnostics(',
           'public static bool IsBlocked(PushBackCompositeEditorState composite, PushBackSystem system)',
           'public static string StatusOr(PushBackCompositeEditorState composite, PushBackSystem system, string fallback)'],
}
for path, sigs in API.items():
    stripped = [l.strip() for l in lines_of(path)]
    for sig in sigs:
        assert sum(1 for l in stripped if l.startswith(sig)) == 1, ('API de PB-T1b', path, sig)

# D1 (v2.8, M14 de F4B-E22): build-plugin depende de tests, asi que en la CI del RED, con tests en failure, sale omitido.
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

# ---------------------------------------------------------------- el paquete: el Objective, el alcance, la tabla de RequiredTests, INV-PBT1C-03 y la costura


def ascii_fold(s):
    for x, y in ((chr(0xAB), '"'), (chr(0xBB), '"'), (chr(0x2014), '-'), (chr(0x2013), '-'), (chr(0x2192), '->'), (chr(0x2264), '<='), (chr(0x2265), '>='),
                 (chr(0x2026), '...'), (chr(0xA7), 'section ')):
        s = s.replace(x, y)
    s = ''.join(c for c in unicodedata.normalize('NFKD', s) if not unicodedata.combining(c))
    assert all(ord(c) < 0x80 for c in s), [c for c in s if ord(c) >= 0x80]
    return s


def md_fold(lines):
    """Una vineta del paquete con sus subvinetas, en una linea ASCII: sin negritas ni comillas de codigo, sin el guion de vineta."""
    return ascii_fold(' '.join(re.sub(r'^- ', '', l.strip()) for l in lines if l.strip()).replace('**', '').replace('`', ''))


PKG = 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md'
MEAS = 'docs/initiatives/I-64-f4b-pbt1-measurements.md'
PKG_SRC = ('--package-file ' + os.path.basename(PKG_FILE)) if PKG_FILE else (PKG + ' en ' + REV_SHA[:8])
PKG_LINES = (open(PKG_FILE, 'rb').read().decode('utf-8') if PKG_FILE else show(PKG)).replace('\r\n', '\n').split('\n')
_title = [l for l in PKG_LINES[:6] if l.startswith('# I-64 F4B')]
PKG_IN_HEAD = re.search(r', (v\d+(?:\.\d+)?)\s*$', _title[0]).group(1) if len(_title) == 1 else None
PKG_HEAD_VERSION = PKG_IN_HEAD
if PKG_FILE:  # el de la revision, solo para informar: al escribir manda el custodiado
    _t = [l for l in lines_of(PKG)[:6] if l.startswith('# I-64 F4B')]
    PKG_HEAD_VERSION = re.search(r', (v\d+(?:\.\d+)?)\s*$', _t[0]).group(1) if len(_t) == 1 else None
V28 = PKG_IN_HEAD == PKG_VERSION
soft(V28, 'el paquete leido (%s) es la %s: no se comprueban sus textos de la %s (en seco, --package-file con el borrador)' % (PKG_SRC, PKG_IN_HEAD, PKG_VERSION))


def section(head_prefix):
    i = [k for k, l in enumerate(PKG_LINES) if l.startswith(head_prefix)]
    assert len(i) == 1, ('seccion', head_prefix, len(i))
    out = []
    for l in PKG_LINES[i[0] + 1:]:
        if l.startswith('## ') or l.startswith('### '):
            break
        out.append(l)
    return out


OBJECTIVE = ('Hacer que la ventana clasica de Push Back guarde su estado editable en el PushBackEditableState de PB-T1b, en modo CLASSIC y sin cambio '
             'observable, con el cambio minimo de D-PBT1-13 (reenvios con el mismo nombre, la carga de Application delegada, AdoptCarriers dentro de '
             'LoadFromModel, ReadInputs que escribe en el estado, el criterio de bloqueo extraido y la costura EditableStateForTest), segun el paquete del '
             'contrato de PB-T1 ' + PKG_VERSION + ' y la lista cerrada CT-PBT1C-01..04, con las condiciones de seguridad de la decision D2 del Owner (F4B-E11) '
             'que toca la ventana. El RED es la costura inerte y las pruebas nuevas, salvo la guarda de D2; el GREEN toca solo W y WC. Si hiciera falta '
             'mutar el rack resuelto o el informe de modulos dentro del tiempo de vida de la edicion: STOP y se reabre L-1. Los constructores, los cuatro '
             'sitios, el XAML, el Plugin, el piloto y Application no cambian; el modo Workspace queda para PB-T2; no se ejecuta AutoCAD.')
SEAM_RED = 'internal PushBackEditableState EditableStateForTest => new PushBackEditableState();'
SEAM_GREEN = 'internal PushBackEditableState EditableStateForTest => editable;'
# INV-PBT1C-03, INV-PBT1C-05 y PBT1C-C13 de la v2.8 corregida (V28-14, V28-15, V28-16 y V28-19), literales: la vineta y sus subvinetas, en
# ASCII (md_fold), SIN la "Nota (no va al contrato)". El contrato lleva las dos invariantes tras su titulo.
INV03_TITLE = 'INV-PBT1C-03. Donde cambia, sobre el diff. '
INV03_V28 = (
    'Cada hunk de git diff -U0 BaseSha..CurrentSha -- src cae en uno de estos sitios, con los rangos de lineas de BaseSha, documentacion incluida: en '
    'W, los campos de D-PBT1-13 (1) y (2): composite, W:49-57, donde entra tambien la declaracion nueva editable de D-PBT1-13 (1); safetySelections, '
    'W:84; generalPallet, W:94-101; rackTolerance, rackPostPeralte, rackBeamDepth y rackAnnotationScale, W:103-109; advanced, W:111-114; '
    'firstLevelDatum, W:375-380; loadedDimensionViews, W:1244-1249; loadedDimensionStyle, W:1251-1256; y dimensionViewsTouched, W:1258-1260; en W, los '
    'metodos que cambian: LoadNew, W:268-289; LoadDesignForNew, W:291-307; LoadExisting, W:309-324; LoadFromModel, W:385-440; ReadInputs, W:833-890; y '
    'Restore_Click, W:3304-3330; en W, los metodos que solo ganan SyncName();: Name_Changed, W:1234-1238; InsertBatch_Click, W:3334-3361; RequestDraw, '
    'W:3363-3419; y SaveLibrary_Click, W:3481-3515. En cada uno, el unico hunk es la insercion de SyncName(); justo despues de W:1237, :3352, :3405 y '
    ':3504, respectivamente; en WC: CompositeDiagnostics, WC:779-786; CompositeStatusOr, WC:788-801; y CompositeHasBlocking, WC:803-807; lo que se '
    'retira, con su documentacion: en W, SideAOnly, W:325-374, y SelectedDimensionViews, W:1288-1298, cada uno con las dos lineas en blanco que lo '
    'rodean; y en WC, los dos LoadCompositeFromDesign, WC:811-934. En estos tres rangos, los hunks solo quitan lineas; las otras dos declaraciones '
    'nuevas de D-PBT1-13, SyncName (8) y EditableStateForTest (9), cada una en una insercion entre dos miembros (un hunk @@ -a,0 ... fuera de todo '
    'rango) cuyas lineas anadidas son solo su declaracion y, si hace falta, una linea en blanco: private void SyncName() => editable.Name = '
    'session.Identity.Name; y internal PushBackEditableState EditableStateForTest => editable;. Tocar un miembro es cambiar o quitar una linea de su '
    'rango, o insertar dentro de el (en un hunk @@ -a,0 ..., cuando s <= a < e); una insercion entre dos miembros es una declaracion nueva, y solo se '
    'admiten esas dos (la definicion de [PKC2] INV-PBTC2-03). Los comentarios: uno nuevo sobre editable solo cabe dentro del rango de composite '
    '(W:49-57); fuera de los rangos no entra ningun comentario nuevo.')
INV05_TITLE = 'INV-PBT1C-05. Las cuentas, sobre CurrentSha: '
INV05_V28 = (
    'W con una sola .RequestUpdate(; ninguna linea anadida de W o WC contiene forceRebuild, KeyDown, PreviewKeyDown, InputBindings, KeyBinding, '
    'TabIndex, IsTabStop, KeyboardNavigation, .Focus( ni DialogResult; W sigue conteniendo los literales de I-55, Fact3 y el desviador; ninguna linea '
    'con SetNumber( se anade ni se quita; las siembras de LoadFromModel y sus dos variables locales (D-PBT1-13 (4); v2.8, V28-10) no cambian; '
    'editable.AdoptCarriers(inputs, catalog); es la primera sentencia del try de LoadFromModel; UpdateCompositeEnabled(); va en la linea siguiente a '
    'LoadDesignEditor y a RestoreEditor; y el censo de declaraciones (v2.8, V28-15): las declaraciones de miembro de W y WC en CurrentSha, contadas '
    'una a una con su nombre (una sobrecarga es una declaracion mas y cambia la cuenta; v2.8, V28-33), son las de BaseSha, sin las cuatro que se retiran '
    '(SideAOnly, SelectedDimensionViews y los dos LoadCompositeFromDesign) y con las tres que prescribe D-PBT1-13 (editable, SyncName y '
    'EditableStateForTest); los doce reenvios de D-PBT1-13 (1) y (2) conservan su nombre; y las lineas testigo (v2.8, V28-16 y V28-30): ninguna de estas '
    'subcadenas ASCII de la documentacion de lo que se retira, cada una unica en W y WC en BaseSha, queda en W ni en WC en CurrentSha, buscada literal: '
    '"del lado A SOLO, sin la parte compuesta." (W:327), "que se guarda: la cargada, exacta, si el usuario no" (W:1289), "Recupera la intencion '
    'compuesta de un diseno cargado" (WC:812) y "CARGAR OTRO RACK Y RESTAURAR EL ACTUAL NO SON LO MISMO" (WC:817).')
# PBT1C-C13 de la v2.8 final (V28-07, V28-14 y V28-33): en ingles, por nombre, acotada a W y WC y con la sobrecarga; literal en ASCII, sin su nota
C13_V28 = ('keeping a gesture without observable change would require touching a member outside INV-PBT1C-03 or changing the order of a load; or doing '
           'the task would require adding in W or WC a declaration whose name does not exist in W or WC at BaseSha, or a further declaration of a name '
           'that does (an overload), of any visibility (method, property, field, constant or nested type), other than the three that D-PBT1-13 '
           'prescribes: editable (1), SyncName (8) and EditableStateForTest (9). The forwarders of D-PBT1-13 (1) and (2), with the same name and in the '
           'range of their field, redeclare names that already exist in BaseSha and are not new declarations. The Worker stops before adding it.')
C13_V27 = 'keeping a gesture without observable change would require touching a member outside INV-PBT1C-03 or changing the order of a load'
# CT-PBT1C-04 (V28-20 y V28-27): las dieciseis autopruebas, las quince de CT-PBT1B-14 mas la (d)
SELFTESTS15 = '(a), (b), (c), (e), (f1), (f2), (f3), (g1), (g2), (h1), (h2), (i), (j), (k1) y (k2)'
# los rangos del texto, uno a uno con RANGES (sin la frase de los cuatro SyncName();, que da lineas sueltas, y hasta las otras dos declaraciones)
_rtxt = INV03_V28[:INV03_V28.index('las otras dos declaraciones nuevas')]
_sync = _rtxt[_rtxt.index('En cada uno, el unico hunk'):_rtxt.index('respectivamente;') + len('respectivamente;')]
assert _sync == 'En cada uno, el unico hunk es la insercion de SyncName(); justo despues de W:1237, :3352, :3405 y :3504, respectivamente;'
assert [1237, 3352, 3405, 3504] == [k for k in SITES if TEXT[W][k - 1].strip().startswith('session.Identity.SetName(')], 'los cuatro SetName de V28-19'
assert sorted((p, int(x), int(y or x)) for p, x, y in re.findall(r'\b(WC?):(\d+)(?:-(\d+))?', _rtxt.replace(_sync, ''))) == sorted(
    ('W' if p == W else 'WC', a, b) for p, a, b, _ in RANGES), 'los rangos del texto de INV-PBT1C-03 y RANGES difieren'
# INV-PBT1C-05 (V28-15): el censo de declaraciones de BaseSha, con el criterio de check_inv_pbt1c_03_ranges.py (las declaraciones de miembro a
# profundidad 2 sobre el texto sin cadenas ni comentarios); la nota de la v2.8 midio 325 en la base.
_MODS = r'(?:public|private|internal|protected|static|readonly|sealed|override|virtual|abstract|partial|async|const|event|unsafe|extern|volatile)'
_CDECL = re.compile(r'^\s*(?:\[[^\]]*\]\s*)*(?:' + _MODS + r'\s+)+(?:class\s+(\w+)|[^=;({]*?\b(\w+)\s*(?:<[^>]*>)?\s*(?:\(|=>|=|;|\{|$))')


def census_strip(text):
    text = re.sub(r'@"(?:[^"]|"")*"', '""', text)
    text = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', text)
    text = re.sub(r"'(?:\\.|[^'\\\n])'", "''", text)
    text = re.sub(r'/\*.*?\*/', ' ', text, flags=re.S)
    return re.sub(r'//[^\n]*', '', text)


def member_names(text):
    names, depth = [], 0
    for line in census_strip(text).split('\n'):
        if depth == 2:
            m = _CDECL.match(line)
            if m:
                names.append(m.group(1) or m.group(2))
        depth += line.count('{') - line.count('}')
    return names


CENSUS = member_names('\n'.join(TEXT[W])) + member_names('\n'.join(TEXT[WC]))
assert len(CENSUS) == 325, ('el censo de declaraciones de W y WC no es el de la v2.8 (325)', len(CENSUS))
assert CENSUS.count('SideAOnly') == 1 and CENSUS.count('SelectedDimensionViews') == 1 and CENSUS.count('LoadCompositeFromDesign') == 2, 'las cuatro que se retiran'
assert not [n for n in ('editable', 'SyncName', 'EditableStateForTest') if n in CENSUS], 'una de las tres nuevas ya esta declarada'
assert all(CENSUS.count(n) == 1 for n in INV03_FIELDS), ('los doce reenvios', [n for n in INV03_FIELDS if CENSUS.count(n) != 1])
# INV-PBT1C-05 (V28-16 y V28-30): las cuatro subcadenas testigo son ASCII (el contrato lo es: van literales en INV05_V28), estan en su linea de
# BaseSha y son unicas en W y WC; buscadas literales en el texto del contrato, se encuentran en el fuente
WITNESS = [(W, 327, 'del lado A SOLO, sin la parte compuesta.'), (W, 1289, 'que se guarda: la cargada, exacta, si el usuario no'),
           (WC, 812, 'Recupera la intencion compuesta de un diseno cargado'), (WC, 817, 'CARGAR OTRO RACK Y RESTAURAR EL ACTUAL NO SON LO MISMO')]
_wtext = '\n'.join(TEXT[W]) + '\n' + '\n'.join(TEXT[WC])
WITNESS_REPORT = []
for path, line, frag in WITNESS:
    assert all(ord(ch) < 0x80 for ch in frag), ('subcadena testigo no ASCII', path, line, frag)
    assert frag in TEXT[path][line - 1] and _wtext.count(frag) == 1, ('subcadena testigo', path, line, frag)
    assert ('"%s" (%s:%d)' % (frag, 'W' if path == W else 'WC', line)) in INV05_V28, ('la subcadena testigo no esta literal en INV-PBT1C-05', frag)
    WITNESS_REPORT.append('%s:%d ASCII=True unica=True literal-en-el-contrato=True: "%s"' % ('W' if path == W else 'WC', line, frag))
PKG_CHECKS = []
if V28:
    SEC63, SEC54, SEC60 = section('### 6.3 '), section('### 5.4 '), section('### 6.0 ')
    _obj = [l for l in SEC63 if l.startswith('- **Objective** (ASCII): ' + chr(0xAB))]
    assert len(_obj) == 1 and ascii_fold(_obj[0][_obj[0].index(chr(0xAB)) + 1:_obj[0].rindex(chr(0xBB))]) == OBJECTIVE, 'el Objective del paquete difiere'
    _aws = [l for l in SEC63 if l.startswith('- **`AllowedWriteScope`** (5): W, WC, ')]
    assert len(_aws) == 1 and all('`' + p + '`' in _aws[0] for p in NEW), 'el AllowedWriteScope del section 6.3'
    assert '`' + SEAM_RED + '`' in '\n'.join(SEC54), 'la costura inerte del section 5.4'

    def bullet(start, stop):
        """La vineta del section 6.3 que empieza en start, con sus subvinetas, hasta stop o hasta su "Nota (no va al contrato)"."""
        i = [k for k, l in enumerate(SEC63) if start(l)]
        assert len(i) == 1, ('vineta', len(i))
        out = [SEC63[i[0]]]
        for l in SEC63[i[0] + 1:]:
            if stop(l) or l.strip().startswith('- **Nota (no va al contrato).**'):
                break
            out.append(l)
        return md_fold(out)

    _inv03 = bullet(lambda l: '**INV-PBT1C-03.' in l, lambda l: l.startswith('  - **INV-PBT1C-04.'))
    assert _inv03 == INV03_TITLE + INV03_V28, ('INV-PBT1C-03 del paquete difiere del de la v2.8', _inv03)
    _inv05 = bullet(lambda l: '**INV-PBT1C-05.' in l, lambda l: l.startswith('  - **INV-PBT1C-06.'))
    assert _inv05 == INV05_TITLE + INV05_V28, ('INV-PBT1C-05 del paquete difiere del de la v2.8', _inv05)
    _c13 = bullet(lambda l: l.startswith('  - **PBT1C-C13:**'), lambda l: l.startswith('  - **PBT1C-C14'))
    assert _c13 == 'PBT1C-C13: ' + C13_V28, ('PBT1C-C13 del paquete difiere', _c13)
    _notes = sum(1 for l in SEC63 if l.strip().startswith('- **Nota (no va al contrato).**'))
    assert _notes == 3, ('las notas que no van al contrato del section 6.3', _notes)
    _ct04 = [l for l in SEC54 if l.startswith('| **04** (Core) |')]
    assert len(_ct04) == 1 and 'las dieciseis autopruebas de D-PBT1-15 sobre W y WC: las quince de CT-PBT1B-14, ' + SELFTESTS15 in md_fold(_ct04) \
        and 'mas la (d), propia de la ventana' in md_fold(_ct04) and 'con las mismas exclusiones de alias' in md_fold(_ct04), 'CT-PBT1C-04: las dieciseis autopruebas'
    # las lineas testigo, con sus tildes, en el texto crudo de INV-PBT1C-05
    _inv05raw = [l for l in SEC63 if '**INV-PBT1C-05.' in l][0]
    assert all(('%s%s%s (%s:%d)' % (chr(0xAB), f, chr(0xBB), 'W' if p == W else 'WC', n)) in _inv05raw for p, n, f in WITNESS), 'las subcadenas testigo del paquete'
    _sec63 = '\n'.join(SEC63)
    assert 'en el RED falla **solo** por sus dos m' in _sec63 and '(CT-PBT1C-01 y 02). Los dem' in _sec63, 'V28-12: el RED de UI PushBack'
    assert '`build-ui` en `success` y `build-plugin` omitido, porque su `needs` incluye `tests`' in _sec63 and '(`delegation.json`; F4B-E21)' in _sec63, 'V28-01'
    assert 'y el `Trailer` de `worker-handoff.json`, que es el trailer real de los commits RED y GREEN' in _sec63, 'V28-17: el Trailer'
    assert 'las tres condiciones de la regla de F4B del Owner, F4B-E25' in _sec63 and ('`' + F4B_E25 + '`') in _sec63, 'V28-22 y V28-35c'
    assert ('con P-01 = ' + chr(0xAB) + P01 % ('J', SNAP_J) + chr(0xBB)) in '\n'.join(SEC60), 'V28-05: P-01 del section 6.0'
    _cus = '\n'.join(SEC60)
    assert all(('`' + n + '`') in _cus for n in ('F4B-DRAFTS/pbt1/pbt1c-anchors-out.txt', 'F4B-DRAFTS/pbt1/independent-review-pbt1c-v28-check.md',
                                                  'pbt1c-anchors-report.md', 'independent-review-pbt1c-v28-check-prompt.md', 'make_pbt1c_contract-dry.txt',
                                                  'measure_pbt1c_anchors.py', 'count_tests_git.py', 'simulate_pbt1c_green.py',
                                                  'check_inv_pbt1c_03_ranges.py', 'simulate-out.txt')), 'V28-18c: las listas de custodia del section 6.0'
    PKG_CHECKS = ['Objective', 'AllowedWriteScope', 'costura inerte', 'INV-PBT1C-03 literal sin nota', 'INV-PBT1C-05 literal sin nota',
                  'PBT1C-C13 literal sin nota', 'tres notas fuera', 'CT-PBT1C-04 dieciseis', 'lineas testigo', 'V28-12', 'V28-01', 'V28-17 Trailer',
                  'V28-22 F4B-E25', 'P-01 J', 'V28-18c custodia', 'RequiredTests']


def package_required(sec):
    """La tabla de RequiredTests del section 6.3: (proyecto, filtro, minimo del paquete o None, ExpectRed, 'el medido en la emision')."""
    rows = []
    for l in sec:
        m = re.match(r'^  \| (UI|Core) \| (.*?) \| (.*?) \| (true|false) \|$', l)
        if not m:
            continue
        proj, fcell, mcell, er = m.groups()
        filters = [t for t in re.findall(r'`([^`]+)`', fcell) if not t.endswith('*')]
        mcell = mcell.replace('**', '')
        vals, depth, cur = [], 0, ''
        for k, ch in enumerate(mcell):  # se parte por ', ' fuera de parentesis
            depth += (ch == '(') - (ch == ')')
            if depth == 0 and mcell.startswith(', ', k) and len(filters) > 1:
                vals.append(cur.strip())
                cur = ''
                continue
            cur += ch
        vals.append(cur.strip())
        vals = [v.lstrip(', ').strip() for v in vals]
        assert len(vals) == len(filters), ('fila', l, vals)
        for f, v in zip(filters, vals):
            mm = re.match(r'^(\d{1,3}(?:[ \u00a0\u202f]\d{3})+|\d+)(?![\d])(.*)$', v)
            if mm:  # un numero; con "medid..." detras, es el medido en la emision (v2.8, V28-13)
                rows.append((proj, f, int(re.sub(r'\D', '', mm.group(1))), er == 'true', 'medid' in mm.group(2)))
            else:  # la v2.7: "el medido en la emision (... = 557)" o "el medido"
                mm = re.search(r'= (\d+)\)', v)
                rows.append((proj, f, int(mm.group(1)) if mm else None, er == 'true', True))
    return rows


# ---------------------------------------------------------------- RequiredTests: (proyecto, prefijo de clase, minimo del paquete, ExpectRed, clase)
# clase: 'new' (no existe en la emision), 'fixed' (numero fijo del paquete, minimo comprobado), 'measured' (el medido en la emision)
UIP, CP = UT + 'RackCad.UI.Tests.csproj', TR + 'RackCad.Tests.csproj'
REQ_SPEC = [
    ('UI', 'PushBackWindowOnStateTests', 2, True, 'new'),
    ('Core', 'PushBackWindowOnStateSourceGuardTests', 1, True, 'new'),
    ('Core', 'PushBackWindowSharedReferenceGuardTests', 1, False, 'new'),
    ('Core', 'PushBackSharedReferenceGuardTests', 2, False, 'fixed'),
    ('UI', 'PushBack', 557, False, 'measured'),
    ('UI', 'BlankFront', 33, False, 'fixed'),
    ('UI', 'WindowCensusGuardTests', 8, False, 'fixed'),
    ('UI', 'RichEditor', 25, False, 'fixed'),
    ('UI', 'Workspace', 547, False, 'fixed'),
    ('UI', 'PushBackRichPreviewTests', 9, False, 'measured'),
    ('Core', 'PushBackEditableState', 13, False, 'fixed'),
    ('Core', 'PushBackEditorOutputGate', 1, False, 'fixed'),
    ('Core', 'PushBackClassicCharacterization', 10, False, 'fixed'),
    ('Core', 'PushBackFullEditorCharacterization', 3, False, 'fixed'),
    ('Core', 'PushBackModuleEditorCharacterizationTests', 15, False, 'fixed'),
    ('Core', 'I55DeferredProductFixCharacterizationTests', 3, False, 'fixed'),
    ('Core', 'NamespaceFolderGuardTests', 7, False, 'fixed'),
    ('Core', 'Workspace', 3271, False, 'fixed'),
    ('UI', 'Cantilever', 67, False, 'fixed'),
]
if V28:  # v2.8, V28-13: UI PushBack 557 (559 en el GREEN) y PushBackRichPreviewTests 9, los dos "medido", que se vuelven a medir en la emision
    _pkg_req = package_required(SEC63)
    assert [(p, f, m, e, k == 'measured') for p, f, m, e, k in REQ_SPEC] == _pkg_req, ('la tabla de RequiredTests del section 6.3 cambio', _pkg_req)
assert len(REQ_SPEC) == 19 and [r for r in REQ_SPEC if r[3]] == REQ_SPEC[:2]

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
    """El criterio de count_tests.py (Fact = 1; Theory = sus InlineData; MemberData/ClassData aparte; Skip aparte), sobre los objetos de la revision."""
    root = UT if project == 'UI' else TR
    ns_prefix = 'RackCad.UI.Tests' if project == 'UI' else 'RackCad.Tests'
    files = [p for p in tracked(root) if p.endswith('.cs')]
    files = [p for p in files if not ('/bin' in '/' + p[len(root):].rsplit('/', 1)[0] or '/obj' in '/' + p[len(root):].rsplit('/', 1)[0])]
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
_t1b_req = {(r['Project'], r['Filter']): r['MinSelected'] for r in pbt1b['RequiredTests']}
REQ, REQ_REPORT = [], []
for p, f, pkg_min, er, kind in REQ_SPEC:
    proj = UIP if p == 'UI' else CP
    filt = 'FullyQualifiedName~%s.%s' % ('RackCad.UI.Tests' if p == 'UI' else 'RackCad.Tests', f)
    classes, run, skip, member = COUNTS[(p, f)]
    note = None
    if kind == 'new':
        assert classes == 0 and run == 0, ('la clase nueva ya existe', f)
        mins = pkg_min
    elif kind == 'fixed':
        if run < pkg_min:
            # una Theory con MemberData no se cuenta en estatico: solo vale si el filtro y su minimo ya los fijo, medidos por la CI, el contrato de PB-T1b
            assert member > 0 and _t1b_req.get((proj, filt)) == pkg_min, (
                'el recuento medido de %s %s (%d, %d MemberData) es menor que el minimo del paquete (%d): el filtro perdio pruebas o el paquete esta mal'
                % (p, f, run, member, pkg_min))
            note = ('     estatico %d (+%d Theory con MemberData) < %d del paquete: vale porque el contrato de PB-T1b fija el mismo filtro con %d '
                    '(casos de MemberData contados por la CI)' % (run, member, pkg_min, pkg_min))
        mins = pkg_min
    else:
        assert run >= pkg_min, ('el recuento medido de %s %s (%d) es menor que el %d del paquete (V28-13)' % (p, f, run, pkg_min))
        mins = run
    REQ.append((proj, filt, mins, er))
    REQ_REPORT.append('%-4s %-45s %-8s paquete=%-5s medido: clases=%d ejecutables=%d skip=%d memberdata=%d -> MinSelected=%d ExpectRed=%s'
                      % (p, f, kind, pkg_min, classes, run, skip, member, mins, er))
    if note:
        REQ_REPORT.append(note)
for proj, _, _, _ in REQ:
    assert in_head(proj), proj
# los filtros heredados de PB-T1b no bajan su MinSelected
for proj, filt, mins, er in REQ:
    if (proj, filt) in _t1b_req:
        assert _t1b_req[(proj, filt)] <= mins, (filt, _t1b_req[(proj, filt)], mins)

cells = json.loads(json.dumps(pbt1b['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna']
assert cells == pbt1a['EligibleCells'] == json.loads(show(PBTC2C))['EligibleCells'], 'las celdas difieren de las de PB-TC2'

# ---------------------------------------------------------------- invariantes
NEWS_TESTS = ('PushBackWindowOnStateTests (' + UT + '), PushBackWindowOnStateSourceGuardTests y PushBackWindowSharedReferenceGuardTests (' + TR + ')')
CLOSE_TXT = PB1B_CLOSE[:8] if PB1B_CLOSE else '<cierre de PB-T1b por rellenar>'
INV = [
    'INV-PBT1C-01 (alcance): el diff BaseSha..CurrentSha cae dentro de las 5 rutas del AllowedWriteScope; en src/, solo W (' + W + ') y WC (' + WC + '); '
    'git diff BaseSha..CurrentSha -- ' + WX + ' src/RackCad.Application src/RackCad.Domain src/RackCad.Plugin src/RackCad.UI/Editor src/RackCad.UI/Controls '
    'vacio. BaseSha es el commit de emision de este contrato, sobre el cierre de PB-T1b (' + CLOSE_TXT + '), con src igual al de ese cierre y '
    'src/RackCad.UI igual al GREEN de PB-TC2 (b5c09f0b).',
    'INV-PBT1C-02 (RED y GREEN): el RED son las tres clases de prueba nuevas (' + NEWS_TESTS + ') y, en W, solo la linea de la costura inerte del '
    'section 5.4 del paquete (' + SEAM_RED + '); el GREEN solo toca W y WC (D-PBT1-13). Criterio mecanico del RED, como INV-PBT1A-02, sobre cada filtro '
    'con ExpectRed y con los resultados de la CI del RED: todos Failed; un Message sin el prefijo de tipo ^(?:[A-Za-z_][\\w`]*[.+])+[A-Za-z_][\\w`]* : ; '
    'la primera linea de StackTrace en un archivo bajo tests/; ninguno NotExecuted ni Skipped; y Failed == Selected >= el MinSelected de su filtro (2 y 1). '
    'Las pruebas siguen la regla EXCEPCION del section 5.0 del paquete (M-11): cada paso que llama al API nuevo pasa por un ayudante privado que convierte '
    'una excepcion en la linea "EXCEPCION <Tipo>: <Mensaje>", y las lineas esperadas no contienen ninguna. La guarda de D2 '
    '(PushBackWindowSharedReferenceGuardTests, CT-PBT1C-04) pasa en el RED y en el GREEN, y sus dieciseis autopruebas en negativo de D-PBT1-15 (las '
    'quince de CT-PBT1B-14, ' + SELFTESTS15 + ', con (k1) y (k2), alias + molde, mas la (d), propia de la ventana), con las reglas como la CT-PBT1B-14 '
    'corregida (la regla 2 hace de un alias una ficha para la 1 y para la 1b, no para la 1c, con las mismas exclusiones de alias; section 5.4 del '
    'paquete; v2.8, V28-20 y V28-27), fallan sobre el texto inyectado (INV-F4T3A-13). El RED de los filtros sin ExpectRed (v2.8, V28-12): el filtro UI PushBack contiene PushBackWindowOnStateTests, asi que en el RED '
    'falla solo por sus dos metodos (CT-PBT1C-01 y 02); los demas filtros sin ExpectRed, en verde. Una correccion que toque RT o ChainRedFiles vuelve a '
    'poner la costura inerte en su RED (M-11, section 16.8). En el GREEN, todos en verde.',
    'INV-PBT1C-03 (donde cambia, sobre el diff): ' + INV03_V28,
    'INV-PBT1C-04 (los literales de T-SG): las cinco sentencias de CT-22, la de S-9, la de S-7 y las cuatro de CT-23 de '
    'PushBackClassicCharacterizationSourceGuardTests (section 4 del paquete) siguen en W, byte a byte y en su orden; SyncName(); va inmediatamente despues '
    'de cada session.Identity.SetName( y de cada session.Identity.Adopt( (los siete sitios de D-PBT1-13 (8): W:279, :305, :322, :1237, :3352, :3405 y :3504).',
    'INV-PBT1C-05 (las cuentas, sobre CurrentSha): ' + INV05_V28,
    'INV-PBT1C-06 (las costuras): las 40 costuras internas de la medicion H (las lineas que empiezan por internal: 39 en W, de W:165 a W:3527, y '
    'CompositeState en WC:43), con el mismo nombre y tipo; solo se anade EditableStateForTest (' + SEAM_GREEN + ' en el GREEN).',
    'INV-PBT1C-07 (las redes): T1B, PB-T0, PB-TC, PB-TC2, PB-T1a (PushBackStateNet), PB-T1b (PushBackEditableState, PushBackEditorOutputGate, '
    'PushBackSharedReferenceGuardTests y PushBackEditableStateParityTests), Cantilever, las guardas del section 4 del paquete y el censo del piloto, en '
    'verde en el GREEN; ninguna prueba existente se edita, se salta, se reactiva ni se borra.',
    'INV-PBT1C-07b (las huellas): CT-PBT1A-14 (PushBackStateNetFingerprintTests), con F-03, en verde sin tocarla, en local y en la CI.',
    'INV-PBT1C-07c (D2, F4B-E11; D-PBT1-15): CT-PBT1C-02 (parte 2) y CT-PBT1C-04 en verde; las guardas de PB-T1b (CT-PBT1B-14 y 15, '
    'PushBackSharedReferenceGuardTests) siguen en verde sin tocarlas; ninguna linea anadida de W o WC asigna el Name de lastComputation.System, '
    'session.System, lastCompositeSystem ni de la linea base de un lado, ni muta de otro modo esos objetos o el informe de un lado (el Name del estado, que '
    'escriben Name_Changed y SyncName, no es ninguno de ellos).',
    'INV-PBT1C-08 (las pruebas de UI): la clase nueva de UI, public sealed, en [Collection(EditorDiscardPromptCollection.Name)], con [Fact] que acaban en '
    'un Assert.Equal; Ventana(), la carga dentro del try y Cerrar en finally; ninguna ventana mostrada; el aviso de descarte sustituido, con el sufijo '
    '" descartes=<n>" en la ultima linea de cada ventana, leido tras cerrarla; sin teclado ni foco real; prohibidos un MessageBox real, el BOM, Guardar '
    'en biblioteca, el configurador real e Insertar; ninguna asercion dentro de una costura; ningun fixture nuevo en Fixtures/I64; los disenos '
    'fabricados (X-01 y F-01 de CT-PBT1C-01), solo en codigo.',
    'INV-PBT1C-09 (determinismo): el GUID fijo, la cultura invariante, los numeros con "R", el orden ordinal; ni hora ni aleatoriedad.',
    'INV-PBT1C-10 (las corridas locales, F4-E59): solo los filtros de RequiredTests en UI, nunca la suite completa de UI en local; la de Core, si; sin copias '
    'de arboles (MAX_PATH); las corridas temporales terminan en Assert.Fail(string.Join("\\n", lineas)), corren tambien dentro de '
    'EditorDiscardPrompt.Substitute (leccion IRC-M02 del cierre de PB-TC2), no entran en ningun commit y guardan TRX y log como artefactos; el perfil del '
    'usuario no cambia (INV-PBT0-06).',
    'INV-PBT1C-11 (la evidencia): toda ruta de worker-handoff.json es relativa y se comprueba que existe; TreeSha es el arbol real (git write-tree) o null; '
    'el Worker no escribe en docs/; una sustitucion se llamaria SUS3C-0n y es PBT1C-C02.',
    'INV-PBT1C-12 (reglas de F4B, [DP], D-PB-10 y sin AutoCAD): la planificacion segun PBT1C-C08; la verificacion con las tres comprobaciones '
    'deterministas de F4B-E15; los controles segun PBT1C-C11; antes de un intento 3, siempre el Owner (PBT1C-C12); la comprobacion fisica es de Smoke-PB.',
    'INV-PBT1C-14 (las dos referencias de D2, F4B-E11): fuera de PushBackEditorState.workingBaseline y PushBackEditorState.LastModuleReconciliation de cada '
    'lado, ninguna copia del estado comparte un objeto mutable con su original; ninguna linea anadida muta un PushBackSystem ni un '
    'RackModuleReconciliationResult adoptado (las reglas de G-L1 de la v2.4, D-PBT1-15, que dan 0 coincidencias sobre W y WC en BaseSha, registro O-1 y '
    'P-4).',
]
INV_IDS = ['01', '02', '03', '04', '05', '06', '07', '07b', '07c', '08', '09', '10', '11', '12', '14']
assert len(INV) == 15 and [i.split(' ')[0] for i in INV] == ['INV-PBT1C-' + n for n in INV_IDS]

# ---------------------------------------------------------------- la evidencia (D1: la CI del RED de PB-T1c, leccion M14 de F4B-E22)
assert CI_BASE and len(CI_BASE) == 5, 'falta CI_BASE'
_run, _sha, _core, _ui, _skip = CI_BASE
assert re.fullmatch(r'\d+', _run) and re.fullmatch(r'[0-9a-f]{8}', _sha) and PB1B_GREEN.startswith(_sha), CI_BASE
CI_TXT = ('la section 6.3 del paquete sobre la linea base del cierre de PB-T1b (run %s sobre %s: Core %d Passed; UI %d resultados, %d Skipped y %d '
          'Passed): Core + 2 (CT-PBT1C-03 y 04) = %d Passed; UI + 2 (CT-PBT1C-01 y 02) = %d resultados: 0 Failed, %d Skipped y %d Passed'
          % (_run, _sha, _core, _ui, _skip, _ui - _skip, _core + 2, _ui + 2, _skip, _ui + 2 - _skip))
EVID = [
    'Antes del Worker: gate-contract valido; la salida del Controller (output.json, con las ocho colecciones opacas vacias) y la delegacion final, con '
    'esas colecciones puestas por la sesion byte a byte desde el contrato (delegation.json; F4B-E21); la comparacion byte a byte sobre la delegacion '
    'final, la aceptacion A1-A8 y nc4 (primera delegacion de la cadena F4B-PBT1C); AllowedWriteScope de la delegacion = exactamente el del contrato; si '
    'hubo una replanificacion por PBT1C-C08, su registro.',
    'Commit RED y push propios antes del GREEN: las tres clases de prueba nuevas y, en W, solo la linea de la costura inerte del section 5.4 del paquete '
    '(INV-PBT1C-02); el diff de la costura; CI del RED (v2.8, V28-01; la leccion PBT1B-IR-M14, F4B-E22) con tests y ui-tests en failure (Core falla por '
    'CT-PBT1C-03, con ExpectRed true, y UI por CT-PBT1C-01 y 02), build-ui en success y build-plugin omitido (skipped), porque su needs incluye tests '
    '(.github/workflows/ci.yml), sin cuelgue (ni blame-hang ni timeout); los TRX de cada filtro con ExpectRed cumplen el criterio de INV-PBT1C-02, y el '
    'filtro UI PushBack falla solo por los dos metodos de PushBackWindowOnStateTests (V28-12); y la salida de las dieciseis autopruebas en negativo de '
    'la guarda de D2 (CT-PBT1C-04), que pasa en el RED.',
    'Commit GREEN y push: el diff dentro de las 5 rutas y, en src/, solo W y WC (INV-PBT1C-01); la salida de INV-PBT1C-03 a 05 sobre el diff (cada hunk '
    'con su miembro, los literales de T-SG y los siete SyncName, y las cuentas); CI del GREEN 4/4 con las suites completas, con los conteos de ' + CI_TXT
    + ', o la diferencia explicada por el diff.',
    'En local: build Release de la solucion, Plugin incluido y sin avisos nuevos; los filtros de RequiredTests sobre el RED y sobre el GREEN; tres corridas '
    'seguidas en verde de los filtros con ExpectRed sobre el GREEN; la clase de huellas (PushBackStateNetFingerprintTests, CT-PBT1A-14 con F-03) tres '
    'veces seguidas en verde sobre el GREEN; la prueba de la limpieza del perfil.',
    'worker-handoff.json conforme a su esquema con RED, GREEN, alcance real, trailer y conteos por filtro (Passed, Failed y Skipped); el mapa CT a metodo '
    '(CT-PBT1C-01 a 04); rutas relativas comprobadas; el Trailer de worker-handoff.json, que es el trailer real de los commits RED y GREEN (v2.8, V28-17: '
    'la mitad (b) de PBT1B-IR-M14); TreeSha real o null.',
    'Despues: Controller verification valida, con las tres comprobaciones deterministas de F4B-E15 (la busqueda de terminos de 16.10 por script sobre el '
    'texto completo de los siete campos libres; el alcance contado por script contra el AllowedWriteScope de la delegacion; y la evidencia de Identity con '
    'el CurrentSha de la entrega y el HEAD medido escritos literalmente); nc1, nc2 y nc3 segun README section 10, conservando cada salida ordinaria; un '
    'control que no da CONTROL PASS se resuelve SOLO como dice PBT1C-C11; cada uso se registra en F4B-DRAFTS/f4b-dispositions.md; la verificacion 14/14 '
    'sola no basta para cerrar.',
    'Sin AutoCAD: la ventana se prueba en UI.Tests con el producto real y la guarda de texto en RackCad.Tests; la comprobacion fisica es de Smoke-PB.',
]

# ---------------------------------------------------------------- paradas: las 17 estandar, las comunes del section 6.0 y las propias del section 6.3
T1B_STOP = {s['Id']: s['Text'] for s in pbt1b['StopConditions']}
C11 = T1B_STOP['PBT1B-C11'].replace('PBT1B', 'PBT1C').replace('PB-T1b', 'PB-T1c')
assert 'PBT1B' not in C11 and 'PB-T1b' not in C11 and C11.endswith('neither one-off acceptance extends to PB-T1c')
_c17 = T1B_STOP['PBT1B-C17']
assert _c17.count(' In PB-T1b the copier') == 1
C17 = (_c17[:_c17.index(' In PB-T1b the copier')] + ' In PB-T1c the window adopts the state of PB-T1b and adds no copier: no added line of W or WC assigns '
       'the Name of lastComputation.System, session.System, lastCompositeSystem or the baseline of a side, nor otherwise mutates those objects or the report '
       'of a side (INV-PBT1C-07c; the Name of the state, written by Name_Changed and SyncName, is none of them); anything else is this stop. The Worker '
       'chooses none of the three exits: the Owner does')
assert 'PB-T1b the copier' not in C17
C02 = T1B_STOP['PBT1B-C02'].replace('PB-TC2 or PB-T1a test', 'PB-TC2, PB-T1a or PB-T1b test').replace('SUS3B-0n', 'SUS3C-0n')
assert 'PB-T1b test' in C02 and 'SUS3C-0n' in C02
# D3 (v2.8, V28-07): el texto EXACTO de la v2.8, en ASCII y en espanol, como esta en el paquete (comprobado arriba si el paquete leido es la v2.8)
C13 = C13_V28 if D3_C13_NEW_MEMBER else C13_V27
STOPS = P_STOPS + [
    {'Id': 'PBT1C-C01', 'Text': 'doing it right would require writing outside the AllowedWriteScope (W and WC, the new UI test class PushBackWindowOnStateTests and the two new Core test classes PushBackWindowOnStateSourceGuardTests and PushBackWindowSharedReferenceGuardTests)'},
    {'Id': 'PBT1C-C02', 'Text': C02},
    {'Id': 'PBT1C-C03', 'Text': 'an observable change of RACKEDITAR against the base appears (in PB-T1c the window keeps its editable state in PushBackEditableState without observable change; the constructors, the four sites, the XAML, the Plugin, the pilot and Application do not change): STOP and back to the Owner (D-16, OB-RE.1)'},
    {'Id': 'PBT1C-C04', 'Text': '(I-06) a method of a filter with ExpectRed passes on the inert seam of section 5.4 of the package, or fails by exception and not by assertion; it is a contract defect and the Worker changes neither the CT nor the seam'},
    {'Id': 'PBT1C-C05', 'Text': T1B_STOP['PBT1B-C05']},
    {'Id': 'PBT1C-C06', 'Text': T1B_STOP['PBT1B-C06']},
    {'Id': 'PBT1C-C07', 'Text': 'the facts of section 2 of the package or the anchors of D-PBT1-13 (section 6.0) do not hold on the base in a way that changes a CT or the design; or an expected line of a CT is not met on the GREEN; the Worker does not adjust the expectation'},
    {'Id': 'PBT1C-C08', 'Text': T1B_STOP['PBT1B-C08']},
    {'Id': 'PBT1C-C09', 'Text': T1B_STOP['PBT1B-C09']},
    {'Id': 'PBT1C-C10', 'Text': T1B_STOP['PBT1B-C10']},
    {'Id': 'PBT1C-C11', 'Text': C11},
    {'Id': 'PBT1C-C12', 'Text': T1B_STOP['PBT1B-C12']},
    {'Id': 'PBT1C-C13', 'Text': C13},
    {'Id': 'PBT1C-C14', 'Text': 'a fingerprint of CT-PBT1A-14 changes: STOP (C03); the Worker adjusts neither the fingerprint nor the product'},
    {'Id': 'PBT1C-C17', 'Text': C17},
]
assert [s['Id'] for s in STOPS[17:]] == ['PBT1C-C%02d' % n for n in range(1, 15)] + ['PBT1C-C17'] and len(STOPS) == 32
for s in STOPS[17:]:  # ninguna parada de PB-T1b copiada sin adaptar (C13 de la v2.8 cita, a proposito, la leccion PBT1B-IR-M10)
    assert not re.search(r'PBT1B-(C\d|NC)|SUS3B', s['Text']), s['Id']

# ---------------------------------------------------------------- autoridades
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
D1 = PILOT + '/F4B-DRAFTS/pbt1/'
SC = PILOT + '/F4B-DRAFTS/superseding-characterization.md'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
PB1B_TCP = PILOT + '/F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
_disp_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', show(DISP))]
assert _disp_ids == list(range(1, len(_disp_ids) + 1)), ('f4b-dispositions.md no esta numerado seguido', _disp_ids[-3:])
assert len(_disp_ids) >= 22, ('f4b-dispositions.md no llega a F4B-E22, la decision de la que salen D1 y D3', 'F4B-E%02d' % _disp_ids[-1])
DISP_LAST = 'F4B-E%02d' % _disp_ids[-1]
UNIT_CHANGE = [
    (W, 'la ventana clasica de Push Back: en el RED, solo la linea de la costura inerte (section 5.4); en el GREEN, D-PBT1-13 en los miembros de '
        'INV-PBT1C-03 (los campos de (1) y (2), LoadNew, LoadDesignForNew, LoadExisting, SideAOnly retirado, LoadFromModel, ReadInputs, Name_Changed, '
        'SelectedDimensionViews retirado, Restore_Click, InsertBatch_Click, RequestDraw y SaveLibrary_Click) y las declaraciones nuevas SyncName y '
        'EditableStateForTest; los constructores, las firmas publicas y lo demas no cambian (D-PBT1-14). En una correccion tras un GREEN, su presencia en '
        'git diff --name-only AuthorityRevision BaseSha se lee con la regla de F4B-E25 (v2.8, V28-22)'),
    (WC, 'la parte compuesta de la ventana: solo en el GREEN, D-PBT1-13 (6) y (7): los dos LoadCompositeFromDesign retirados, y CompositeDiagnostics, '
         'CompositeStatusOr y CompositeHasBlocking, que delegan en PushBackEditorOutputGate. En una correccion tras un GREEN, su presencia en git diff '
         '--name-only AuthorityRevision BaseSha se lee con la regla de F4B-E25 (v2.8, V28-22)'),
]
EXISTING = [
    (PHASE, 'paquete de la fase F4B v3.1: OB-01.1b, OB-03.1, OB-05.1, OB-06.5, OB-10.1, OB-T1B.1 a OB-T1B.3, OB-RE.1, H-PB-27, R-PB-01, R-PB-05 y R-PB-09; la fila de PB-T1 del section 5'),
    (CO + 'owner-f4b-disposition.md', 'archivo completo ([OD]): la fila de PB-T1, D-PB-05, D-PB-06 y D-PB-10'),
    (CO + 'owner-f4b-procedure-decisions.md', 'archivo completo ([DP], F4B-E03): preguntas 1 y 2 (firmas exactas de los sustitutos y la replanificacion unica)'),
    (CO + 'owner-pbt1-pre-decisions.md', 'archivo completo ([OPT1], F4B-E09): pregunta 2 (c-02 como regla explicita), pregunta 3 (pb-21-cola fuera de la garantia) y pregunta 4 (la autorizacion condicional de emision)'),
    (CO + 'owner-d1-d2-decisions.md', 'archivo completo ([OD2], F4B-E11): D2 con sus diez condiciones y su limite, y PB-T1 PLAN'),
    (CO + 'owner-pbt1a-decisions.md', 'archivo completo (F4B-E14): pregunta 2 (c-09 como regla explicita, D-PBT1-09b; D-PBT1-13 (5)) y pregunta 3 (OWNER-3 y OWNER-4 tal cual)'),
    (CO + 'owner-pbt1a-nc1-decisions.md', 'archivo completo (F4B-E15): las tres comprobaciones deterministas de la verificacion, que valen tambien para PB-T1c'),
    (CO + 'owner-pbtc-nc1-decision.md', 'la disposicion unica de nc1 en PB-TC (F4B-E08), que no se extiende a PB-T1c'),
    (CO + 'owner-pbtc2-nc1-decisions.md', 'la disposicion de nc1 en PB-TC2 solo para su intento 1 (F4B-E12), que no se extiende a PB-T1c'),
    (CO + 'owner-config-snapshot-j-decision.md', 'archivo completo (F4B-E19): la Snapshot J sustituye a la I como linea base activa del Controller (la que cita P-01)'),
    (CO + 'owner-pbt1b-c15-decisions.md', 'archivo completo (F4B-E20): la normalizacion conocida de pb-15-sin-composite en el estado de PB-T1b'),
    (CO + 'owner-f4b-opaque-collections-decision.md', 'archivo completo (F4B-E21): desde PB-T1b y en el resto de F4B, la sesion pone byte a byte en la delegacion las ocho colecciones opacas del contrato; la comparacion byte a byte, A1-A8 y nc4 se hacen sobre la delegacion final'),
    # v2.8, V28-31: F4B-E24 en el section 1, con su vuelta (custodiadas en 95373af6); F4B-E23 (D-PB-07) no hace falta en PB-T1 (V28-25b)
    (CO + 'owner-f4b-dpb07-extensiondata-decisions.md', 'pregunta 2 (F4B-E24): la ExtensionData anidada, "Basta asi (Recomendado)": la pregunta de antes de '
                                                       'PB-T3 (OB-05.2), respondida (D-PBT1-08 y A-4)'),
    (PILOT + '/F4B-DRAFTS/pbfull/owner-return-extensiondata.md', 'la vuelta al Owner por la ExtensionData anidada, respondida por F4B-E24'),
    (F4B_E25, 'archivo completo (F4B-E25, v2.8, V28-22 y V28-23): la regla de F4B sobre las rutas UNIT_CHANGE en una correccion: una ruta UNIT_CHANGE de '
              'git diff --name-only AuthorityRevision BaseSha no incumple 16.3 si todo commit que la toca es un RedSha o un CurrentSha de un Worker de la '
              'misma cadena, la ruta esta en el AllowedWriteScope de esa delegacion y ningun commit de la sesion la toca; las EXTERNAL, sin excepcion. W y '
              'WC son UNIT_CHANGE en este contrato'),
    (F4B_E25_RET, 'la vuelta al Owner por la parada de la primera verificacion del intento 2 de PB-T1b (16.3), respondida por F4B-E25'),
    (CO + 'owner-pbt1b-rework-decisions.md','archivo completo (F4B-E22): pregunta 2, M10 y M14 ratificados solo para PB-T1b, y el paquete de PB-T1c corrige el texto: la CI del RED con build-plugin omitido por su needs (ExpectedEvidence) y la lista cerrada de miembros (INV-PBT1C-03)'),
    (DISP, 'archivo completo: F4B-E01 a ' + DISP_LAST),
    (PILOT + '/F4B-DRAFTS/pbt1a/owner-return-pbt1a.md', 'la vuelta al Owner tras el Worker de PB-T1a: los hechos de c-09 al reconstruir (OWNER-2), OWNER-3 y OWNER-4'),
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-c15.md', 'la vuelta al Owner por la parada PBT1B-C15, respondida por F4B-E20'),
    # v2.8, V28-04c: las vueltas de F4B-E19 y F4B-E21, y la revision y el analisis del intento 1 de PB-T1b, en el section 1 del paquete
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-config.md', 'la vuelta al Owner por el config.toml tras el reinicio, respondida por F4B-E19 (la Snapshot J)'),
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-planning2.md', 'la vuelta al Owner por la salida truncada del Controller, respondida por F4B-E21'),
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-rework.md', 'la vuelta al Owner tras la revision independiente del intento 1 de PB-T1b (M10 y M14), respondida por F4B-E22'),
    (PILOT + '/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T204715Z-a4e2/independent-review.md', 'la revision independiente del intento 1 de PB-T1b (REWORK 0/2/15): PBT1B-IR-M10, PBT1B-IR-M14 y PBT1B-IR-I01, las lecciones que aplican V28-01, V28-07 y V28-09'),
    (PILOT + '/F4B-PBT1B-LOSSLESS-STATE-TYPE/1/R20261118T204715Z-a4e2/analysis.md', 'el analisis de la correccion de PB-T1b (F4B-E22): section 4, los recuentos que no cambian, y la autoprueba alias + molde de I01'),
    (D1 + 'ob065-dossier.md', 'el expediente del analista: section 3 (los huecos de red) y section 4 (las guardas de T1B y de texto)'),
    (D1 + 'owner-return-l1.md', 'la vuelta L-1 al Owner, respondida por F4B-E11 (D2)'),
    (D1 + 'independent-review-v1.md', 'la revision independiente IRT1-1 de la v1'),
    (D1 + 'independent-review-v2-check.md', 'la comprobacion acotada de la v2'),
    (D1 + 'independent-review-v23-check.md', 'la comprobacion acotada de la remedicion (v2.3), aplicada en la v2.4'),
    (D1 + 'pbt1b-anchors-out.txt', 'la salida de los anclajes de PB-T1b (registro P): G-L1 sobre W y WC (P-4) y los recuentos de UI (P-5)'),
    (D1 + 'independent-review-pbt1b-check.md', 'la comprobacion acotada de PB-T1b sobre la v2.5, aplicada en la v2.6'),
    (D1 + 'independent-review-pbt1b-v27-check.md', 'la comprobacion acotada de la v2.7, aplicada en la v2.7'),
    (SC, 'el registro declarado ([SC]): PB-TC y PB-TC2 EJECUTADAS'),
    ('docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md', 'el paquete de PB-T0: CT-PB0-01 (las 667 hojas de pb-21-cola), CT-PB0-17 (T-2: c-01 a c-16), M-01 y D-3'),
    (PBT0C, 'el contrato de PB-T0: la lista curada de tests/RackCad.Tests/ (medicion H-12)'),
    (PILOT + '/F4B-PBT0-CHARACTERIZATION/task-close-package.md', 'el cierre de PB-T0'),
    ('docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md', 'el paquete de PB-TC v2.2 ([PKC]): el algoritmo del ForbiddenWriteScope ([PKC-C]) y CT-PBTC-06'),
    (PBTCC, 'el contrato de PB-TC'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 'el paquete de PB-TC2 v2 ([PKC2]): la celda del Controller (:655), la forma de los alcances y la definicion de tocar un miembro de INV-PBTC2-03'),
    ('docs/initiatives/I-64-f4b-pbtc2-measurements.md', 'el registro de mediciones de PB-TC2'),
    (PBTC2C, 'el contrato de PB-TC2 ([PKC2-C]): las paradas estandar y las celdas'),
    (PILOT + '/F4B-PBTC2-DECLARED-CORRECTIONS/task-close-package.md', 'el cierre de PB-TC2 sobre b5c09f0b, el src/RackCad.UI de la BaseSha de PB-T1c'),
    (PBT1AC, 'el contrato de PB-T1a: el src/ de 59 entradas (la forma de PB-TC2, con WX)'),
    (PILOT + '/F4B-PBT1A-LOSSLESS-STATE-NET/task-close-package.md', 'el cierre de PB-T1a sobre 73d30982 (F4B-E16): la red PushBackStateNet, con CT-PBT1A-14 y F-03, las huellas que PB-T1c mantiene en verde'),
    (PBT1BC, 'el contrato de PB-T1b: las paradas estandar, las celdas, la raiz, las listas curadas de tests/ y sus clases de prueba, y los filtros heredados'),
    (PB1B_TCP, 'el cierre de PB-T1b: el estado editable de Application que la ventana adopta y la linea base de la CI de PB-T1c'),
    ('docs/initiatives/I-64-f4-t1b-gate-contract-package.md', 'el patron de T1B ([T1B]): la caracterizacion vigente de la ventana clasica (55 casos en UI y 10 en Core)'),
    ('docs/initiatives/I-64-f4-t2-gate-contract-package.md', 'el regimen del RED con esqueleto inerte, que compila, no lanza y falla por asercion (:282)'),
    ('docs/initiatives/I-64-f4-t3-gate-contract-package.md', 'INV-F4T3A-13 (:371-374): las guardas pueden pasar en el RED y discriminan con autopruebas'),
    ('docs/initiatives/I-64-f4-t3c-gate-contract-package.md', 'la regla RED (:297): completa y compilable, que no lanza y no cambia el comportamiento'),
    (T1BC, 'el contrato de T1B ([T1B-C])'),
    (PILOT + '/F4-DRAFTS/f4-dispositions.md', 'las fuentes de PD-F4-01 (:1103-1106, :1540, :520, :562) y PD-F4-04 (:1657-1660), F4-E56 (:1917-1928) y F4-E59'),
    (PILOT + '/F4-DRAFTS/coordinator/owner-f4t3-authorization.md', 'la firma literal de PD-F4-04 (:255-265)'),
    ('docs/automation/state/I-64.yml', 'estado vigente completo'),
    ('docs/automation/evidence/I-64-evidence.md', 'las secciones de F4B (231 y siguientes)'),
]
# el cierre de PB-T1b (F4B-E26) ya esta custodiado en 95373af6: todas las autoridades tienen que estar en la revision (sin avisos)
# las listas de PB-T1c del section 6.0, Custodia (v2.8, V28-18c y V28-28b)
CUSTODY = [  # las cuatro rutas de CUSTODY, que son autoridades, en el commit de emision de PB-T1c
    (PKG, 'archivo completo (' + PKG_VERSION + '): el registro de cambios v2.7 -> v2.8 (V28-01 a V28-28), section 2 (hechos), section 3 (D-PBT1-13, la '
          'ventana sobre el estado; D-PBT1-14; D-PBT1-15, D2, con las autopruebas (k1) y (k2)), section 4 (las guardas de T1B y de texto), section 5.0, 5.4 '
          'y 5.5 (las recetas, la regla EXCEPCION, la costura inerte, CT-PBT1C-01 a 04 y el censo control -> prueba), section 6.0 y 6.3 (el contrato, con '
          'F4B-E21, F4B-E25, la correccion de PB-T1b, los rangos de INV-PBT1C-03 y el censo y las lineas testigo de INV-PBT1C-05), section 8 (decisiones '
          'del Owner) y section 10 (arrastres, con A-7 a A-10)'),
    (MEAS, 'el registro de mediciones del paquete, apartados A a ' + MEAS_PBT1C + ' (' + MEAS_PBT1C + ': los anclajes de PB-T1c sobre el cierre de PB-T1b)'),
    (D1 + 'pbt1c-anchors-out.txt', 'la salida del script de los anclajes de PB-T1c (section 6.0 del paquete): src/RackCad.UI frente al cierre de PB-T1b, '
                                   'las anclas de D-PBT1-13, G-L1 sobre W y WC, los recuentos y la linea base de la CI'),
    (D1 + 'independent-review-pbt1c-v28-check.md', 'la comprobacion acotada de la v2.8 (section 6.0 del paquete): EMITIBLE TRAS CORRECCIONES 0/1/7, '
                                                   'aplicada en la v2.8, con lo de D2'),
]
assert len(CUSTODY) == 4 and len({p for p, _ in CUSTODY}) == 4 and not {p for p, _ in CUSTODY} & {p for p, _ in EXISTING}
# las de CUSTODY_EXTRA: se custodian y no son autoridades, todas en F4B-DRAFTS/pbt1/
CUSTODY_EXTRA = [D1 + n for n in ('pbt1c-anchors-report.md', 'independent-review-pbt1c-v28-check-prompt.md', 'make_pbt1c_contract-dry.txt',
                                  'measure_pbt1c_anchors.py', 'count_tests_git.py', 'simulate_pbt1c_green.py', 'check_inv_pbt1c_03_ranges.py',
                                  'simulate-out.txt')]
assert len(CUSTODY_EXTRA) == 8 == len(set(CUSTODY_EXTRA))
CUSTODY_FREE = [p for p, _ in CUSTODY[2:]] + CUSTODY_EXTRA  # los nombres nuevos de F4B-DRAFTS/pbt1/, libres antes de la emision (V28-18c)
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D12 y D13 (sin modelo paralelo; la caracterizacion no se edita para que pase)'),
    ('docs/AUTOMATION_PLAN.md', 'section 16.3 (la lectura de autoridades UNIT_CHANGE; v2.8, V28-04), 16.5, 16.8, 16.9, 16.10 y 16.11'),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4)'),
    ('docs/automation/agent-execution/routing.md', 'archivo completo'),
    ('docs/automation/agent-execution/model-catalog.md', 'celdas medidas y vigentes para Controller y Worker'),
]
assert len({p for p, _ in CUSTODY + EXISTING + UNIT_CHANGE + EXTERNAL}) == len(CUSTODY + EXISTING + UNIT_CHANGE + EXTERNAL)
for p, _ in EXISTING + EXTERNAL + UNIT_CHANGE:
    assert in_head(p), ('autoridad inexistente en la revision', p)
_close = ' (' + PB1B_CLOSE_ENTRY + ')'
assert int(PB1B_CLOSE_ENTRY.split('-E')[1]) in _disp_ids and _disp_ids[-1] >= 26, ('F4B-E23 a F4B-E26 en f4b-dispositions.md', DISP_LAST)
c = {
    'Schema': 'rackcad-gate-contract/v1', 'Unit': 'I-64', 'Gate': 'F4B', 'TaskId': TASK,
    'Objective': OBJECTIVE,
    'AuthorityRevision': AR, 'MainSha': MAIN,
    'Authorities': [{'Path': p, 'Section': s, 'Class': 'UNIT_DOC'} for p, s in CUSTODY + EXISTING] + [
        {'Path': CONTRACT_REL, 'Section': 'este contrato', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_CHANGE'} for p, s in UNIT_CHANGE]
      + [{'Path': p, 'Section': s, 'Class': 'EXTERNAL'} for p, s in EXTERNAL],
    'AllowedWriteScope': ALLOWED,
    'ForbiddenWriteScope': FORBIDDEN,
    'Invariants': INV,
    'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
    'ExpectedEvidence': EVID, 'StopConditions': STOPS, 'EligibleCells': cells, 'RoutingEnforcement': 'required', 'CorrectionsAuthorized': True,
    'IssuedBy': ('Sesion principal de I-64 (Claude) por las decisiones del Owner F4B-E09, con su autorizacion condicional de emision (pregunta 4) '
                 '(docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md), F4B-E11, D2 y PB-T1 PLAN '
                 '(docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md), y F4B-E14 y F4B-E15 '
                 '(docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1a-decisions.md y owner-pbt1a-nc1-decisions.md): la emision de la '
                 'tercera tarea de PB-T1 tras el cierre de PB-T1b' + _close + ', sus anclajes y su comprobacion acotada; las decisiones de procedimiento '
                 'owner-f4b-procedure-decisions.md (F4B-E03); F4B-E19, la Snapshot J (owner-config-snapshot-j-decision.md); F4B-E21, las colecciones '
                 'opacas que pone la sesion (owner-f4b-opaque-collections-decision.md); F4B-E22, que corrige en el paquete de PB-T1c la CI del RED y la '
                 'lista cerrada de miembros (owner-pbt1b-rework-decisions.md); F4B-E25, la regla de F4B sobre las rutas UNIT_CHANGE en una correccion '
                 '(owner-f4b-correction-unit-change-decision.md); y el paquete del contrato ' + PKG_VERSION + ', con su comprobacion acotada'),
    'IssuedUtc': ISSUED,
}
N_AUTH = len(CUSTODY) + len(EXISTING) + 1 + len(UNIT_CHANGE) + len(EXTERNAL)
assert len(c['Authorities']) == N_AUTH == 68, len(c['Authorities'])  # 60 + 4 (V28-04c) + F4B-E25 y su vuelta (V28-22) + F4B-E24 y su vuelta (V28-31)
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
MEAS_HAS_PBT1C = ('\n## ' + MEAS_PBT1C + '. ') in show(MEAS)
soft(PKG_HEAD_VERSION == PKG_VERSION, 'el paquete custodiado en la revision es la %s; al escribir hace falta la %s custodiada en HEAD' % (PKG_HEAD_VERSION, PKG_VERSION))
soft(MEAS_HAS_PBT1C, 'el registro en la revision no tiene el apartado %s (los anclajes de PB-T1c)' % MEAS_PBT1C)
for p, _ in CUSTODY:
    soft(in_head(p), 'a custodiar con la emision de PB-T1c, aun no en la revision: %s' % p)
for p in CUSTODY_EXTRA:
    soft(in_head(p), 'a custodiar con la emision, sin ser autoridad, aun no en la revision: %s' % p)
if DRY:
    print('DRY rev', REV_SHA[:8], '(objetos del checkout principal)' if OBJ_REV else '(HEAD del worktree)', 'allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN),
          '(src', len(SRCF), 'tests', len(TESTF), '[core', len(CORE_T), 'ui', str(len(UI_T)) + '] root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'required', len(REQ), 'evidence', len(EVID), 'authorities', len(c['Authorities']), 'anchors-ok', len(ANCHORS), 'api-ok', sum(len(v) for v in API.values()),
          'seams', len(SEAMS), 'sites', len(SITES), 'bytes', len(b), 'sha256', hashlib.sha256(b).hexdigest().upper())
    print('anclaje:', ANCHOR_SRC, '| src/RackCad.UI == b5c09f0b: True | paquete en la revision:', PKG_HEAD_VERSION, '| registro con el apartado %s:' % MEAS_PBT1C,
          MEAS_HAS_PBT1C, '| f4b-dispositions hasta', DISP_LAST)
    print('paquete leido:', PKG_SRC, '->', PKG_IN_HEAD, '| textos de la %s comprobados:' % PKG_VERSION, ', '.join(PKG_CHECKS) if PKG_CHECKS else 'ninguno')
    print('INV-PBT1C-03 (V28-08): %d rangos comprobados contra la revision (empiezan en la documentacion y acaban en el final del miembro; '
          'bordes en blanco solo los declarados), con %d autopruebas en negativo (cada rango desplazado una linea), todas fallan:' % (len(RANGES), RANGE_SELFTESTS))
    for l in RANGE_REPORT:
        print('  ' + l)
    print('V28-11b: escritores de portadores fuera de LoadFromModel y ReadInputs = W:%s' % ', :'.join(str(k) for k in _assign))
    print('INV-PBT1C-05 (V28-15): censo de declaraciones de W y WC = %d (las cuatro que se retiran presentes; editable, SyncName y EditableStateForTest '
          'ausentes; los doce reenvios, una vez cada uno)' % len(CENSUS))
    print('INV-PBT1C-05 (V28-16): lineas testigo, cada una en su linea y unica en W y WC:')
    for l in WITNESS_REPORT:
        print('  ' + l)
    _free = [p for p in CUSTODY_FREE if not in_head(p)]
    print('V28-18c: nombres de custodia nuevos de F4B-DRAFTS/pbt1/ libres en la revision: %d de %d' % (len(_free), len(CUSTODY_FREE)))
    print('D1: needs de build-plugin =', _needs, '| D2: Snapshot J en la revision:', in_head(SNAP_J), '| D3: C13 con miembro nuevo:', D3_C13_NEW_MEMBER)
    print('SetNumber( en BaseSha: W', SETNUMBER_W, 'WC', SETNUMBER_WC, '(INV-PBT1C-05: el GREEN no anade ni quita ninguna linea con SetNumber()')
    print('RequiredTests (criterio de count_tests.py sobre los objetos de la revision):')
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
assert PB1B_CLOSE and PB1B_CLOSE_ENTRY and CI_BASE, 'faltan PB1B_CLOSE, PB1B_CLOSE_ENTRY o CI_BASE'
assert not WARN, WARN
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
