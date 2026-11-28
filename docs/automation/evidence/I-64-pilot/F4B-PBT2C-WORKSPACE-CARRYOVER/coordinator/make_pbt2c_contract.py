"""Contrato de gate de F4B-PBT2C-WORKSPACE-CARRYOVER (ASCII), desde el paquete del contrato de PB-T2 v2.9
(docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md: section 1, section 5.0, section 5.4, section 6.0 y section 6.3) y el patron de
make_pbt2b_contract.py. La BaseSha es hija del cierre de PB-T2b (PT2B_CLOSE, por rellenar), que desciende del cierre de PB-T2a y de la emision de
PB-T2a (0dc623ca, F4B-E32); W y WC son los que deja PB-T2b.
El ForbiddenWriteScope de src/ sale de cover() de [PKC-C] con W y WC permitidos, mas el archivo del modo de PB-T2b (60); los de tests/ y la raiz, de
las listas del contrato emitido de PB-T2a mas las clases nuevas de PB-T2a y de PB-T2b (section 6.0 y 6.3 del paquete).
Comprueba sobre la revision leida (HEAD del worktree o, con --objects-rev, esa revision en el almacen de objetos del checkout principal):
- el anclaje del section 6.0 para PB-T2c: la revision desciende del cierre de PB-T1c y de la emision de PB-T2a; src solo cambia, desde el GREEN de
  PB-T1c, en los dos archivos de PB-T2a y en los dos de PB-T2b (W y el modo); WC y WX son los blobs de 161a0dad; tests solo gana las clases de
  PB-T1c, PB-T2a y PB-T2b; con PT2B_CLOSE (constante o --pt2b-close), la revision desciende de el, src es igual y es de verdad el cierre de PB-T2b;
- los textos de los tres mutantes [T2b] (MU-T2C-01..03): cada linea una sola vez, en su miembro, y la tabla cerrada del paquete, literal;
- la tabla de gestos de CT-PBT1C-02 (49 con Compara y los cuatro de D2) y, cuando este custodiada, la tabla de lo esperado de cada fila de
  CT-PBT2C-03 (ct3_table_check);
- los datos de CT-PBT2C-01 y 02: las once filas F de SC1, los fixtures de los estilos y del redondeo, las doce lineas de CT-PBTC-07 y el gesto
  neutro (DrawRackNameCheck escucha Click y lo lee ReadInputs);
- el texto del paquete (la v2.9, por --package-file en seco o custodiada en HEAD): el Objective, el AllowedWriteScope, la tabla de RequiredTests,
  los invariantes propios (literales y SIN su "Nota (no va al contrato)") y comunes, las paradas propias C04, C13 y C15, la ExpectedEvidence, la
  linea base de la CI, las lineas de PB-T2c del section 6.0, el regimen y los CT del section 5.4, la cabecera y los registros de la v2.4 a la v2.9 y la regla RR de F4B-E35;
- los MinSelected, frente a los ejecutables (count_tests_git.py sobre los objetos de la revision; v2.4, V24-05);
- D1, D2, F4B-E06, F4B-E09, F4B-E23 y F4B-E28 custodiadas, la parada del intento 1 de PB-T2a (F4B-E33), las celdas.
Lo que depende de los cierres de PB-T2a y de PB-T2b (PT2B_CLOSE, CI_BASE_T2B, el contrato emitido de PB-T2b, el archivo del modo, las clases de
PB-T2b, la linea de MU-T2C-03 y los task-close-package.md) o de la emision de PB-T2b (v26-delta.md, v27-delta.md) o de la custodia de F4B-E35 o de la emision de PB-T2c (la v2.9 en HEAD, BOUNDED_RESULT y su comprobacion acotada,
la tabla de CT-PBT2C-03, las rutas de CUSTODY_EXTRA y la entrada de emision en f4b-dispositions.md) es AVISO en seco y error al escribir; todo lo
demas es error siempre.
Uso: python make_pbt2c_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print] [--print-json]
                                   [--objects-rev <rev> [--repo <checkout principal>] [--package-file <paquete>]] [--pt2b-close <sha>]
Con --dry no escribe nada. --objects-rev exige --dry: no hace chdir al worktree y lee todo con git -C <checkout principal> show <rev>:<ruta>.
--package-file exige --objects-rev y --dry, y tiene que ser la v2.9. Al escribir, el paquete es el custodiado en HEAD y tiene que ser la v2.9.
Al custodiarlo, las rutas locales se redactan ('<worktree-de-I-64>', '<checkout-principal>')."""
import datetime, decimal, hashlib, json, os, re, subprocess, sys, unicodedata

# ---------------------------------------------------------------- los cierres anteriores, la emision de PB-T2a y el cierre de PB-T2b (por rellenar)
PB1B_CLOSE = '95373af6116cf120bd0a8ba490833524e33c47da'  # F4B-E26
PT1C_CLOSE = '5a8366c166cd1034293811e5c2ac0f7c6e05a761'  # F4B-E31: el cierre de PB-T1c
PT1C_CLOSE_ENTRY = 'F4B-E31'
PT1C_GREEN = '161a0dadccaf5b71886344d834146737b90bb4f3'  # el GREEN de PB-T1c: WC y WX de la BaseSha de PB-T2c
PT2A_EMIT = '0dc623ca7a4bf1640654440276aa3d27147f48d4'   # F4B-E32: la emision de PB-T2a
PT2A_EMIT_ENTRY = 'F4B-E32'
PT2A_STOP_ENTRY = 'F4B-E33'  # el intento 1 de PB-T2a, parado con PBT2A-C07 (C-1: el momento de una lectura)
PT2A_STOP_ANALYSIS = 'docs/automation/evidence/I-64-pilot/F4B-PBT2A-DRAFT-STATE-PORT/1/R20261119T060524Z-55f2/analysis.md'
WINDOW_BLOBS = {'WC': '1a3300b4', 'WX': 'fb094bef'}  # PB-T2a y PB-T2b no tocan WC ni WX (INV-PBT2B-01)
PT2B_CLOSE = '56cb026f8a6fd87467a6400e2f3bca015c4abda3'  # F4B-E38: la custodia del cierre de PB-T2b, hija de 03b16c88; tambien --pt2b-close <sha>
PT2B_CLOSE_ENTRY = 'F4B-E38'  # se comprueba frente al titulo "Cierre de F4B-PBT2B-WORKSPACE-MODE-WINDOW"
CI_BASE_T2B = ('44586045707', '03b16c88c01ebfd29a642fc6a9324eb76926bcba', 19094, 2447, 21)  # la corrida push 4/4 del GREEN de PB-T2b (F4B-E38): (run, sha, Core Passed, UI total, UI Skipped)
CI_T2B_EXPECTED = (19094, 2447, 21)  # la prevision del section 6.3: el GREEN de PB-T2b (section 6.2)
CI_T2B_EXPLAINED = None  # POR RELLENAR solo si las cifras de CI_BASE_T2B difieren de las previstas: la explicacion por el diff de PB-T2b
BOUNDED_RESULT = ('EMITIBLE TRAS CORRECCIONES (B 0 / I 1 / M 5; I-01 y M-01 a M-05 corregidas en el paquete; recomprobacion v2.6: EMITIBLE, '
                  'B 0 / I 0 / M 3; M-06 a M-08 aplicadas; recomprobacion v2.8: EMITIBLE, B 0 / I 0 / M 2; M-09 aplicada en la v2.9; '
                  'recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2; M-11 y M-12 aplicadas en la v2.9; '
                  'recomprobacion v2.9: EMITIBLE, B 0 / I 0 / M 2 (seccion 13, final sobre el cierre de PB-T2b); M-13 y M-14 aplicadas)')  # CA-T2c y su seccion 13, final sobre el cierre de PB-T2b; literal de la sesion
PKG_VERSION = 'v2.9'
OWNER_PBT2_ENTRY = 'F4B-E28'
MAIN_CHECKOUT = '<checkout-principal>'  # Redaccion para el repo publico: la ruta local del checkout principal  # solo con --objects-rev; se redacta al custodiar ('<checkout-principal>')

ARGS = sys.argv[1:]
VALUED = ('--objects-rev', '--repo', '--pt2b-close', '--package-file')


def opt(name):
    if name not in ARGS:
        return None
    i = ARGS.index(name)
    assert i + 1 < len(ARGS) and not ARGS[i + 1].startswith('--'), ('falta el valor de', name)
    return ARGS[i + 1]


POS = [a for i, a in enumerate(ARGS) if not a.startswith('--') and not (i > 0 and ARGS[i - 1] in VALUED)]
assert len(POS) == 3, 'uso: make_pbt2c_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print] [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
OBJ_REV = opt('--objects-rev')
PT2B_CLOSE = opt('--pt2b-close') or PT2B_CLOSE
DRY = '--dry' in ARGS
assert OBJ_REV is None or DRY, '--objects-rev solo en seco: anade --dry'
PKG_FILE = opt('--package-file')
assert PKG_FILE is None or (OBJ_REV is not None and DRY), '--package-file solo junto con --objects-rev y --dry'
PKG_FILE = os.path.abspath(PKG_FILE) if PKG_FILE else None
assert PKG_FILE is None or os.path.isfile(PKG_FILE), ('no existe el paquete', PKG_FILE)
assert re.fullmatch(r'[0-9]{4}-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9]Z', ISSUED), ISSUED
assert re.fullmatch(r'[0-9a-f]{40}', AR) and re.fullmatch(r'[0-9a-f]{40}', MAIN), (AR, MAIN)
assert PT2B_CLOSE is None or re.fullmatch(r'[0-9a-f]{40}', PT2B_CLOSE), PT2B_CLOSE

if OBJ_REV is None:
    WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree
    os.chdir(WT)
    REPO, REV = None, 'HEAD'
else:
    REPO, REV = opt('--repo') or MAIN_CHECKOUT, OBJ_REV
DISK = OBJ_REV is None

TASK = 'F4B-PBT2C-WORKSPACE-CARRYOVER'
assert len(TASK) == 29, len(TASK)  # section 6.0, Identidad (MAX_PATH; v2.1, V21-05)
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
    """Lo que solo existira con los cierres de PB-T2a y PB-T2b o con la emision de PB-T2c: error al escribir, AVISO en seco."""
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

# ---------------------------------------------------------------- el anclaje del section 6.0 (PB-T2c)
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
WC = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs'
WX = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml'
MODE = 'src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
PB1C_TCP = PILOT + '/F4B-PBT1C-LOSSLESS-STATE-WINDOW/task-close-package.md'
PB2A_TCP = PILOT + '/F4B-PBT2A-DRAFT-STATE-PORT/task-close-package.md'
PB2B_TCP = PILOT + '/F4B-PBT2B-WORKSPACE-MODE-WINDOW/task-close-package.md'
PBT2AC = PILOT + '/F4B-PBT2A-DRAFT-STATE-PORT/coordinator/gate-contract.json'
PBT2BC = PILOT + '/F4B-PBT2B-WORKSPACE-MODE-WINDOW/coordinator/gate-contract.json'
PBT1CC = PILOT + '/F4B-PBT1C-LOSSLESS-STATE-WINDOW/coordinator/gate-contract.json'
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
for sha, full in (('5a8366c1', PT1C_CLOSE), ('161a0dad', PT1C_GREEN), ('0dc623ca', PT2A_EMIT), ('95373af6', PB1B_CLOSE)):
    assert git('rev-parse', sha).stdout.decode('ascii').strip() == full, ('SHA', sha)
assert git('merge-base', '--is-ancestor', PT1C_CLOSE, REV).returncode == 0, 'la revision no desciende del cierre de PB-T1c'
assert ('\n## %s. Cierre de F4B-PBT1C-LOSSLESS-STATE-WINDOW' % PT1C_CLOSE_ENTRY) in show(DISP, PT1C_CLOSE).replace('\r\n', '\n'), 'PT1C_CLOSE no es el cierre de PB-T1c'
assert git('merge-base', '--is-ancestor', PT2A_EMIT, REV).returncode == 0, 'la revision no desciende de la emision de PB-T2a (0dc623ca)'
assert ('\n## %s. Emisi' % PT2A_EMIT_ENTRY) in show(DISP, PT2A_EMIT).replace('\r\n', '\n') and git('cat-file', '-e', PT2A_EMIT + ':' + PBT2AC).returncode == 0, (
    'PT2A_EMIT no es la emision de PB-T2a')
_disp_rev = show(DISP).replace('\r\n', '\n')
assert ('\n## %s. PB-T2a, intento 1: parada PBT2A-C07' % PT2A_STOP_ENTRY) in _disp_rev, 'la parada del intento 1 de PB-T2a (F4B-E33)'
assert '**C-1. CT-PBT2A-02, la l' in show(PT2A_STOP_ANALYSIS), 'C-1 del analisis de F4B-E33'
pbt2a = json.loads(show(PBT2AC))
pbt1c = json.loads(show(PBT1CC))
assert pbt2a['TaskId'] == 'F4B-PBT2A-DRAFT-STATE-PORT' and pbt1c['TaskId'] == 'F4B-PBT1C-LOSSLESS-STATE-WINDOW'
T2A_SRC = [f for f in pbt2a['AllowedWriteScope'] if f.startswith('src/')]
T2A_TESTS = [f for f in pbt2a['AllowedWriteScope'] if f.startswith('tests/')]
T1C_TESTS = [f for f in pbt1c['AllowedWriteScope'] if f.startswith('tests/')]
assert len(T2A_SRC) == 2 and len(T2A_TESTS) == 2 and len(T1C_TESTS) == 3
# PB-T2b (section 6.2 del paquete): W y el archivo del modo en src, y seis clases de prueba (el AllowedWriteScope de su contrato)
T2B_UI = [UT + n + '.cs' for n in ('PushBackWorkspaceSupport', 'PushBackWorkspaceModeTests', 'PushBackWorkspaceApplyCloseTests',
                                   'PushBackFieldValidationConcordanceTests')]
T2B_CORE = [TR + n + '.cs' for n in ('PushBackWorkspaceModeSourceGuardTests', 'PushBackWorkspaceSharedReferenceGuardTests')]
T2B_SRC = [W, MODE]
T2B_ALLOWED = T2B_SRC + T2B_UI + T2B_CORE
# src: desde el GREEN de PB-T1c solo cambian los dos archivos de PB-T2a y los dos de PB-T2b; WC y WX, los de 161a0dad
_src = names_of(git('diff', '--name-only', PT1C_GREEN, REV, '--', 'src'))
assert set(_src) <= set(T2A_SRC + T2B_SRC), ('src cambio fuera de PB-T2a y PB-T2b desde el GREEN de PB-T1c', _src)
assert git('diff', '--quiet', PT1C_GREEN, REV, '--', 'src/RackCad.Domain', 'src/RackCad.Plugin').returncode == 0
for _k, _p in (('WC', WC), ('WX', WX)):
    _b = git('rev-parse', REV + ':' + _p).stdout.decode('ascii').strip()
    assert _b.startswith(WINDOW_BLOBS[_k]) and _b == git('rev-parse', PT1C_GREEN + ':' + _p).stdout.decode('ascii').strip(), ('el blob de ' + _k, _b[:8])
_tests = names_of(git('diff', '--name-only', PB1B_CLOSE, REV, '--', 'tests'))
assert set(_tests) <= set(T1C_TESTS + T2A_TESTS + T2B_UI + T2B_CORE), ('tests cambio fuera de PB-T1c, PB-T2a y PB-T2b desde el cierre de PB-T1b', _tests)
ANCHOR_SRC = 'desciende de %s (%s) y de %s (%s); src desde %s: %s; WC y WX == %s' % (
    PT1C_CLOSE[:8], PT1C_CLOSE_ENTRY, PT2A_EMIT[:8], PT2A_EMIT_ENTRY, PT1C_GREEN[:8], ', '.join(os.path.basename(p) for p in _src) or 'igual', PT1C_GREEN[:8])
W_CHANGED = W in _src
# el cierre de PB-T2b: "git diff <cierre de PB-T2b> <BaseSha> -- src vacio" (section 6.0)
PT2B_ENTRY = None
if PT2B_CLOSE:
    assert git('cat-file', '-e', PT2B_CLOSE + '^{commit}').returncode == 0, ('el cierre de PB-T2b no esta en el almacen de objetos', PT2B_CLOSE)
    assert git('merge-base', '--is-ancestor', PT2A_EMIT, PT2B_CLOSE).returncode == 0, 'PT2B_CLOSE no desciende de la emision de PB-T2a'
    assert git('merge-base', '--is-ancestor', PT2B_CLOSE, REV).returncode == 0, 'la revision no desciende del cierre de PB-T2b'
    assert git('diff', '--quiet', PT2B_CLOSE, REV, '--', 'src').returncode == 0, 'src cambio desde el cierre de PB-T2b'
    _r = git('show', PT2B_CLOSE + ':' + DISP)
    _m = re.findall(r'(?m)^## (F4B-E\d+)\. Cierre de F4B-PBT2B-WORKSPACE-MODE-WINDOW\b', _r.stdout.decode('utf-8')) if _r.returncode == 0 else []
    assert len(_m) == 1, ('PT2B_CLOSE no es el cierre de PB-T2b: su f4b-dispositions.md no tiene la entrada de cierre', PT2B_CLOSE)
    PT2B_ENTRY = _m[0]
    assert PT2B_CLOSE_ENTRY in (None, PT2B_ENTRY), ('PT2B_CLOSE_ENTRY', PT2B_CLOSE_ENTRY, PT2B_ENTRY)
    assert int(PT2B_ENTRY.split('E')[-1]) > int(PT2A_STOP_ENTRY.split('E')[-1]), ('el cierre de PB-T2b es anterior a la parada de PB-T2a', PT2B_ENTRY)
    _r2 = _r.stdout.decode('utf-8')
    assert re.search(r'(?m)^## F4B-E\d+\. Cierre de F4B-PBT2A-DRAFT-STATE-PORT\b', _r2), 'el cierre de PB-T2b sin el de PB-T2a delante'
    assert git('cat-file', '-e', PT2B_CLOSE + ':' + PB2B_TCP).returncode == 0, ('PT2B_CLOSE no es el cierre de PB-T2b: no tiene su task-close-package.md', PT2B_CLOSE)
    assert W_CHANGED and git('cat-file', '-e', REV + ':' + MODE).returncode == 0, 'el cierre de PB-T2b sin W cambiado ni el archivo del modo'
    ANCHOR_T2B = 'src == cierre de PB-T2b (%s, %s)' % (PT2B_CLOSE[:8], PT2B_ENTRY)
else:
    ANCHOR_T2B = 'sin el cierre de PB-T2b'
soft(PT2B_CLOSE, 'falta PT2B_CLOSE (el cierre de PB-T2b, la constante o --pt2b-close): no se comprueba "git diff <cierre de PB-T2b> <BaseSha> -- src vacio"')
# el contrato emitido de PB-T2b: si esta, su AllowedWriteScope es el del section 6.2 y su C11 es la de este contrato con el otro prefijo
PBT2B_OK = soft(in_head(PBT2BC), 'el contrato emitido de PB-T2b aun no esta en la revision (de su emision): ' + PBT2BC)
if PBT2B_OK:
    pbt2b = json.loads(show(PBT2BC))
    assert pbt2b['TaskId'] == 'F4B-PBT2B-WORKSPACE-MODE-WINDOW' and pbt2b['AllowedWriteScope'] == T2B_ALLOWED, 'el contrato de PB-T2b'
# el puerto de PB-T2a (lo usa la ventana de WORKSPACE): con la API de D-PBT2-02 y D-PBT2-03
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
soft(PORT_OK, 'el puerto de PB-T2a aun no esta en la revision: ' + ', '.join(os.path.basename(p) for p in T2A_SRC))

# Las 17 estandar, del contrato emitido de PB-T2a, con P-01 en la Snapshot J; en PB-T2c, P-01 en la K (abajo; v2.9, V29-06)
SNAP_J = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-J/snapshot-j.json'
P01_J = 'config.toml no longer matches the accepted Snapshot in force (today Snapshot J, %s)' % SNAP_J
P_STOPS = [dict(s) for s in pbt2a['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
STD_IDS = ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
assert [s['Id'] for s in P_STOPS] == STD_IDS and P_STOPS[9]['Text'] == P01_J
assert P_STOPS == [dict(s) for s in pbt1c['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
assert in_head(SNAP_J)
# la Snapshot K (F4B-E37; v2.9, V29-06): P-01 de PB-T2c y su autoridad. El registro y la decision se custodian con el cierre de PB-T2b: AVISO en
# seco y parada al escribir; con los dos en la revision, snapk_check
SNAP_K = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-K/snapshot-k.json'
SNAP_K_DEC = PILOT + '/F4B-DRAFTS/coordinator/owner-config-snapshot-k-decision.md'
SNAP_K_SHA = 'E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE'
SNAP_K_PROBE = 'R20261119T154313Z-2a9a'
P01_K = 'config.toml no longer matches the accepted Snapshot in force (today Snapshot K, %s)' % SNAP_K
P_STOPS_T2C = [dict(s, Text=P01_K) if s['Id'] == 'P-01' else dict(s) for s in P_STOPS]
assert [s['Id'] for s in P_STOPS_T2C] == STD_IDS and P_STOPS_T2C[9]['Text'] == P01_K and P_STOPS_T2C[:9] + P_STOPS_T2C[10:] == P_STOPS[:9] + P_STOPS[10:]


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


SNAPK_OK = soft(in_head(SNAP_K) and in_head(SNAP_K_DEC), 'del cierre de PB-T2b: la Snapshot K (F4B-E37; v2.9, V29-06), su registro y su decision, '
                'aun no en la revision: %s y %s' % (SNAP_K, SNAP_K_DEC))
if SNAPK_OK:
    _err = snapk_check(show(SNAP_K), show(SNAP_K_DEC), SNAP_K_SHA, SNAP_K_PROBE)
    assert not _err, ('la Snapshot K (F4B-E37)', _err)

# ---------------------------------------------------------------- el alcance (section 6.3)
NEW_T2C = UT + 'PushBackWorkspaceCarryoverTests.cs'
ALLOWED = [W, WC, NEW_T2C]
assert in_head(W) and in_head(WC) and (not DISK or (os.path.isfile(W) and os.path.isfile(WC)))
assert not in_head(NEW_T2C) and (not DISK or not os.path.exists(NEW_T2C)), NEW_T2C
assert not [p for p in tracked('src/') + tracked('tests/') if 'PushBackWorkspaceCarryover' in p], 'el prefijo ya existe'
assert git('grep', '-n', '-E', 'CT_PBT2C_0[1-3]_|PushBackWorkspaceCarryover', REV, '--', 'src', 'tests').returncode == 1, 'un nombre de PB-T2c ya existe'


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
MODE_OK = soft(MODE in SRCF, 'el archivo del modo de PB-T2b aun no esta en la revision: el ForbiddenWriteScope de src/ lo lleva igual (59 hoy + 1 = 60)')
if not MODE_OK:
    SRCF = sorted(SRCF + [MODE], key=lambda p: (os.path.dirname(p), os.path.basename(p)))
assert len(SRCF) == 60, len(SRCF)  # section 6.3: 60 de src/ (59 hoy, mas el archivo del modo de PB-T2b)
for p in (WX, MODE, 'src/RackCad.Application/', 'src/RackCad.Domain/', 'src/RackCad.Plugin/', 'src/RackCad.UI/Editor/', 'src/RackCad.UI/Controls/',
          'src/RackCad.UI/Workspace/', 'src/RackCad.UI/Shell/'):
    assert p in SRCF, p
# tests/: las listas del contrato de PB-T2a, mas sus dos clases y las seis de PB-T2b
CORE_T2A = [f for f in pbt2a['ForbiddenWriteScope'] if f.startswith(TR)]
UI_T2A = [f for f in pbt2a['ForbiddenWriteScope'] if f.startswith(UT)]
assert len(CORE_T2A) == 25 and len(UI_T2A) == 63 and UI_T2A[-1] == UT + 'Workspace/', (len(CORE_T2A), len(UI_T2A))
CORE_T = CORE_T2A + T2A_TESTS + T2B_CORE
UI_T = UI_T2A + T2B_UI
assert len(CORE_T) == 29 == len(set(CORE_T)) and len(UI_T) == 67 == len(set(UI_T))
TESTF = CORE_T + UI_T
assert not any(a == t or (t.endswith('/') and a.startswith(t)) for a in ALLOWED for t in TESTF)
ROOT = [f for f in pbt2a['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
assert ROOT == ['docs/', '.github/', 'eng/', 'tools/', 'assets/', 'deploy/', 'RackCad.sln', 'Directory.Build.props', 'Directory.Build.targets', 'global.json',
                'NuGet.Config', 'README.md', 'AGENTS.md', 'CLAUDE.md', '.editorconfig', '.gitattributes', '.gitignore'], ROOT
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == 173 == len(set(FORBIDDEN)), len(FORBIDDEN)
_t2b_missing = [f for f in [MODE] + T2B_UI + T2B_CORE if not in_head(f)]
for f in FORBIDDEN:
    if f not in _t2b_missing:
        assert in_head(f) and (not DISK or os.path.exists(f.rstrip('/'))), ('no existe', f)
soft(not _t2b_missing, 'los archivos de PB-T2b del ForbiddenWriteScope aun no estan en la revision (de su cierre): ' + ', '.join(os.path.basename(f) for f in _t2b_missing))

# ---------------------------------------------------------------- los mutantes [T2b] (section 5.4): una linea cada uno, en su miembro, sobre BaseSha
W_LINES, WC_LINES, WX_LINES = lines_of(W), lines_of(WC), lines_of(WX)
TEXT = {'W': W_LINES, 'WC': WC_LINES}
# (id, miembro, archivo, modo, linea o fragmento, sustitucion, declaracion del miembro). MU-T2C-01 y 02: la linea entera, literal. MU-T2C-03, por
# contenido (v2.6, V26-03; CA-T2c M-02): la unica linea de ApplyToDraft que contiene el fragmento que ya exige R4 de CT-PBT2B-08, con
# Copy(editable) -> Copy(appliedState); PB-T2b no tiene que dejar una sentencia literal
MU3_FRAG = 'draftPort.ApplyToDraft(PushBackEditableStateComparer.Copy(editable))'
MU3_IN, MU3_OUT = 'PushBackEditableStateComparer.Copy(editable)', 'PushBackEditableStateComparer.Copy(appliedState)'
MUTANTS = [
    ('MU-T2C-01', 'Keep', 'W', 'linea', '=> !field.Value.HasValue || SameShown(field.Value.Value, current) ? current : field.Value.Value;',
     '=> !field.Value.HasValue ? current : field.Value.Value;', 'private static double Keep(NumericField field, double current)'),
    ('MU-T2C-02', 'UpdateSideBEmptyNotice', 'WC', 'linea', 'SideBEmptyNoticeText.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;',
     'SideBEmptyNoticeText.Visibility = Visibility.Collapsed;', 'private void UpdateSideBEmptyNotice()'),
    ('MU-T2C-03', 'ApplyToDraft', 'W', 'contenido', MU3_FRAG, MU3_FRAG.replace(MU3_IN, MU3_OUT), 'private void ApplyToDraft()'),
]
MUT_REPORT = []
# ---- mu_check (inicio)
M06 = ('v2.6, V26-08 (CA-T2c M-06): si en el cierre de PB-T2b el fragmento de MU-T2C-03 no esta en una sola linea fisica de W, o la firma de '
       'ApplyToDraft no es "private void ApplyToDraft()", la emision de PB-T2c se para; la sesion redefine la linea del mutante por contenido en una '
       'version nueva del paquete, con su comprobacion acotada; no es decision del Owner, salvo que el cambio altere el comportamiento')
MU_SPLIT = 'draftPort.ApplyToDraft('  # el principio de la llamada de MU-T2C-03: sin el fragmento entero en su linea, la llamada esta partida


def mu_check(lines, mid, member, mode, line, sub, decl, sub_in):
    """La linea de un mutante en el texto de su archivo: (linea, linea de la declaracion) si esta y vale, None si no esta. Aborta (AssertionError)
    si esta mal: repetida, fuera de su miembro, con la sustitucion ya puesta o, por contenido, partida en varias lineas fisicas o con otra firma.
    Por contenido, el mensaje de aborto dice que hacer (M06)."""
    tail = ('. ' + M06) if mode == 'contenido' else ''
    hits = [k for k, l in enumerate(lines, 1) if (l.strip() == line if mode == 'linea' else line in l)]
    if not hits:
        if mode == 'contenido':
            part = [k for k, l in enumerate(lines, 1) if MU_SPLIT in l]
            assert not part, '%s: el fragmento de %s no esta en una sola linea fisica (la llamada empieza en las lineas %s)%s' % (mid, member, part, tail)
        return None
    assert len(hits) == 1, '%s: la linea no es unica (lineas %s)%s' % (mid, hits, tail)
    k = hits[0]
    back = [j for j in range(k - 1, max(0, k - 40), -1) if lines[j - 1].strip().startswith(('private ', 'internal ', 'public ', 'protected '))]
    assert back and lines[back[0] - 1].strip() == decl, '%s: la linea no esta en %s: la declaracion de encima es %r%s' % (
        mid, decl, lines[back[0] - 1].strip() if back else None, tail)
    assert sum(1 for l in lines if (l.strip() == sub if mode == 'linea' else sub in l)) == 0, '%s: la sustitucion ya esta en el texto%s' % (mid, tail)
    if mode == 'contenido':
        assert lines[k - 1].count(sub_in) == 1, '%s: la sustitucion por contenido no es unica en su linea%s' % (mid, tail)
    return k, back[0]
# ---- mu_check (fin)


for mid, member, f, mode, line, sub, decl in MUTANTS:
    assert all(ord(ch) < 0x80 for ch in line + sub) and line != sub, mid  # testigos ASCII del contrato
    found = mu_check(TEXT[f], mid, member, mode, line, sub, decl, MU3_IN)
    present = soft(found, '%s: la linea de %s (%s) aun no esta en la revision (la deja PB-T2b): %s%s' % (
        mid, member, f, line, ('. Al escribir, ' + M06) if mode == 'contenido' else ''))
    if present:
        k, kd = found
        MUT_REPORT.append('%s %s:%d en %s (declarado en %s:%d)%s' % (mid, f, k, member, f, kd, ', por contenido' if mode == 'contenido' else ''))
    else:
        MUT_REPORT.append('%s pendiente del cierre de PB-T2b%s' % (mid, ' (por contenido: ' + line + ')' if mode == 'contenido' else ''))
# CT-PBT2C-01: el gesto neutro no recalcula solo (Click="Input_Changed") y lo lee ReadInputs (v2.4, V24-14)
assert any(l.strip().startswith('<CheckBox x:Name="DrawRackNameCheck"') for l in WX_LINES) and any(
    l.strip() == 'Foreground="{DynamicResource ShellTextPrimaryBrush}" Click="Input_Changed" />' for l in WX_LINES[WX_LINES.index(
        [l for l in WX_LINES if 'x:Name="DrawRackNameCheck"' in l][0]):][:2]), 'DrawRackNameCheck escucha Click'
_ri = [k for k, l in enumerate(W_LINES, 1) if l.strip() == 'private PushBackEditorInputs ReadInputs()']
assert len(_ri) == 1
_ri_end = next(k for k in range(_ri[0], len(W_LINES)) if W_LINES[k - 1].strip() == '}' and W_LINES[k - 1].startswith('        }'))
assert 'editable.DrawRackName = DrawRackNameCheck.IsChecked == true;' in [l.strip() for l in W_LINES[_ri[0]:_ri_end]], 'ReadInputs lee DrawRackNameCheck'
READINPUTS = (_ri[0] - 0, _ri_end)

# ---------------------------------------------------------------- la tabla de gestos de CT-PBT1C-02 y los datos de CT-PBT2C-01 y 02
_ws = show(UT + 'PushBackWindowOnStateTests.cs')
_gs = _ws[_ws.index('private static IEnumerable<Gesto> Gestos()'):_ws.index('private static string DisenoDeUnaCopia(')]
GESTURES = re.findall(r'yield return G\(\s*"([^"]+)"', _gs)
D2_GESTURES = re.findall(r'yield return SinComparar\(\s*"([^"]+)"', _gs)
assert len(GESTURES) == 49 == len(set(GESTURES)) and _gs.count('yield return G(') == 49, ('V24-14: los gestos de CT-PBT1C-02', len(GESTURES))
assert D2_GESTURES == ['campo-en-error', 'calculo-invalido', 'caja-escrita-sin-salir', 'cerrar-sin-pedir'], D2_GESTURES
assert all(all(ord(ch) < 0x80 for ch in g) and ' ' not in g for g in GESTURES)
assert 'ToleranceBox=4' in GESTURES and 'StructureOverrideBox=6' in GESTURES and 'Restaurar' in GESTURES
_sc1 = show(UT + 'PushBackDeclaredCorrectionSupport.cs')
FILAS = re.findall(r'new FilaF \{ Name = "(F\d+)", Path = "([^"]+)", Base = ([\d.]+) \}', _sc1)
FILAS_PKG = [('F1', 'Structure.PalletDepth', '48'), ('F2', 'Structure.PostPeralte', '3'), ('F3', 'Structure.PalletTolerance', '4'),
             ('F4', 'Structure.BeamDepth', '6'), ('F5', 'Structure.AnnotationScale', '1.5'), ('F6', 'Structure.Fronts[0].FirstLevelHeight', '4'),
             ('F7', 'Structure.Fronts[0].Levels[0].PalletFront', '42'), ('F8', 'Structure.Fronts[0].Levels[0].PalletHeight', '60'),
             ('F9', 'Structure.Fronts[0].Levels[0].PalletWeight', '1000'), ('F10', 'Structure.Fronts[0].Levels[0].ClearHeight', '6'),
             ('F11', 'Structure.Fronts[0].Levels[0].BeamLengthOverride', '100')]
assert FILAS == FILAS_PKG and 'private const double Increment = 0.00044;' in _sc1, ('las filas F de SC1', FILAS)
# los valores exactos que fija CT-PBT2C-01: la base mas 0,00044 (F4, BeamDepth, da 6)
EXACT_PKG = 'F1 48.00044, F2 3.00044, F3 4.00044, F5 1.50044, F6 4.00044, F7 42.00044, F8 60.00044, F9 1000.00044, F10 6.00044 y F11 100.00044'
EXACT = ['%s %s' % (name, decimal.Decimal(base) + decimal.Decimal('0.00044')) for name, _, base in FILAS_PKG if name != 'F4']
assert ', '.join(EXACT[:-1]) + ' y ' + EXACT[-1] == EXACT_PKG, ('los exactos de las filas F', EXACT)
FX = TR + 'Fixtures/I64/'


def fx_inner(name):
    env = json.loads(show(FX + name + '.json').lstrip(chr(0xFEFF)))
    return env, json.loads(env['Design'])


_e8, _i8 = fx_inner('pb-08-redondeo')
assert _i8['PushBack']['Structure']['AnnotationScale'] == 1.23456 and '61.23456' in json.dumps(_i8), 'pb-08-redondeo'
assert fx_inner('pb-06-seguridad')[1]['PushBack']['Structure']['DimensionStyle'] == 'STANDARD-ISO'
assert fx_inner('pb-07-combinado')[1]['PushBack']['Structure']['DimensionStyle'] == 'SIMPLEX'
assert fx_inner('pb-01-base')[0]['Name'] == 'PB-01' and fx_inner('pb-20-ab')[1]['PushBack']['SideB']['IsPresent'] is True

# ---------------------------------------------------------------- las anclas de las pruebas que copian o leen los CT de PB-T2c
T_ANCHORS = [
    (UT + 'PushBackDeclaredCorrectionSideBNoticeTests.cs', 40, 'public void CT_PBTC_07_TheSideBNoticeShowsExactlyWhileSideBIsDeclaredWithoutAnActiveFront()'),
    (UT + 'PushBackDeclaredCorrectionSideBNoticeTests.cs', 89, '"pb-01-base carga aviso=Collapsed compuesto=False",'),
    (UT + 'PushBackDeclaredCorrectionSideBNoticeTests.cs', 100, '"pb-20-ab todas las ranuras de B en blanco aviso=Visible compuesto=False",'),
    (UT + 'PushBackDeclaredCorrectionSupport.cs', 49, 'internal static IReadOnlyList<FilaF> FilasF() => new[]'),
    (UT + 'PushBackDeclaredCorrectionSupport.cs', 65, 'internal static string VarianteDe(FilaF fila, string fixture = "pb-01-base")'),
    (UT + 'PushBackDeclaredCorrectionSupport.cs', 191, 'internal static TextBlock Aviso(Window w) => Assert.IsType<TextBlock>(w.FindName("SideBEmptyNoticeText"));'),
    (UT + 'PushBackWindowOnStateTests.cs', 454, 'private static IEnumerable<Gesto> Gestos()'),
    (UT + 'PushBackWindowOnStateTests.cs', 449, 'private static void Recalcular(RackPushBackSystemWindow w)'),
    (UT + 'PushBackFullEditorCharacterizationSupport.cs', 336, 'internal static void SetNumberR(Window window, string name, double value)'),
    (UT + 'PushBackDeclaredCorrection2MixedBoxTests.cs', 96, 'log.Add(linea + " descartes=" + descartes);'),
    ('src/RackCad.Application/Workspace/Drafts/DraftSession.cs', 287, 'public TState CurrentDraftState => _current;'),
    ('src/RackCad.Application/Workspace/Drafts/DraftSession.cs', 348, '_current = _copy(state);'),
    # v2.6, V26-01 (CA-T2c I-01): el Cotas de CT-PBTC-05 que CT-PBT2C-01 aplica a c, y la peticion clasica que nunca lee
    (UT + 'PushBackDeclaredCorrectionDimensionStyleTests.cs', 82, '/// <summary>Las cotas de la ultima computacion: las instancias de rol <c>Dimension</c> '
                                                                  'de los cinco planos del rack.</summary>'),
    (UT + 'PushBackDeclaredCorrectionDimensionStyleTests.cs', 83, 'private static IEnumerable<HeaderBlockInstance> Cotas(RackPushBackSystemWindow w)'),
    (UT + 'PushBackDeclaredCorrectionDimensionStyleTests.cs', 92, '}'),
    (UT + 'PushBackDeclaredCorrectionDimensionStyleTests.cs', 114, '+ " diseno=" + (w.DesignToInsert.Structure.DimensionStyle ?? "<null>")'),
    (UT + 'PushBackDeclaredCorrectionDimensionStyleTests.cs', 115, '+ " sistema=" + (w.SystemToInsert.Structure.DimensionStyle ?? "<null>")'),
]
_tcache = {}
for path, line, text in T_ANCHORS:
    if path not in _tcache:
        _tcache[path] = lines_of(path)
    assert _tcache[path][line - 1].strip() == text, ('ancla de prueba', path, line, _tcache[path][line - 1].strip())
_sbn = _tcache[UT + 'PushBackDeclaredCorrectionSideBNoticeTests.cs']
SIDEB_LINES = [l.strip().rstrip(',').strip('"') for l in _sbn[88:100]]
assert len(SIDEB_LINES) == 12 and SIDEB_LINES[0].startswith('pb-01-base carga') and SIDEB_LINES[-1].startswith('pb-20-ab todas')
assert git('diff', '--quiet', PB1B_CLOSE, REV, '--', UT + 'PushBackDeclaredCorrectionSideBNoticeTests.cs', UT + 'PushBackDeclaredCorrectionSupport.cs').returncode == 0


def census_strip(text):
    text = re.sub(r'@"(?:[^"]|"")*"', '""', text)
    text = re.sub(r'\$?"(?:\\.|[^"\\\n])*"', '""', text)
    text = re.sub(r"'(?:\\.|[^'\\\n])'", "''", text)
    text = re.sub(r'/\*.*?\*/', ' ', text, flags=re.S)
    return re.sub(r'//[^\n]*', '', text)


# ---------------------------------------------------------------- CA-T2a M-6 (a): las clases de los filtros de UI que crean un RackFrameConfiguratorWindow real
UI_FILTERS = ['PushBackWindowOnStateTests', 'PushBackEditableStateParityTests', 'PushBackStateNet', 'PushBackClassicCharacterization', 'PushBackFullEditorCharacterization',
              'PushBackDeclaredCorrection', 'PushBackCompositeWindowTests', 'PushBackCompositeLifecycleWiringTests', 'PushBackCompositeLoadExistingModulesTests',
              'PushBackBothModeCellBeamOverrideTests', 'PushBackModuleEditorWindowTests', 'PushBackAdvancedRackParametersWindowTests', 'PushBackRichPreviewTests',
              'PushBackLibrarySaveGateTests', 'DialogWindowCharacterizationTests', 'BlankFront', 'WindowCensusGuardTests', 'RichEditor', 'Workspace', 'Cantilever',
              'PushBackWorkspaceModeTests', 'PushBackWorkspaceApplyCloseTests', 'PushBackFieldValidationConcordanceTests']
_cls = re.compile(r'^\s*(?:public|internal)\s+(?:sealed\s+|static\s+|partial\s+|abstract\s+)*class\s+(\w+)')
CONFIGURATOR_FILES = []
for p in [p for p in tracked(UT) if p.endswith('.cs') and '/bin/' not in p and '/obj/' not in p]:
    t = show(p)
    classes = [m.group(1) for m in (_cls.match(l) for l in t.split('\n')) if m]
    fq = ['RackCad.UI.Tests.' + c for c in classes]
    if any(('RackCad.UI.Tests.' + f) in q for f in UI_FILTERS for q in fq) and 'new RackFrameConfiguratorWindow(' in census_strip(t):
        CONFIGURATOR_FILES.append(os.path.basename(p))
CONFIGURATOR_FILES.sort()
assert CONFIGURATOR_FILES == ['RichEditorCharacterizationTests.cs', 'RichEditorCloseContractTests.cs', 'RichEditorContractTests.cs'], ('V23-01', CONFIGURATOR_FILES)

# ---------------------------------------------------------------- las decisiones del Owner (F4B-E06, E09, E23 y E28), custodiadas
CO = PILOT + '/F4B-DRAFTS/coordinator/'
OWNER_PBT2 = CO + 'owner-pbt2-decisions.md'
_disp = _disp_rev
_disp_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
assert _disp_ids == list(range(1, len(_disp_ids) + 1)), ('f4b-dispositions.md no esta numerado seguido', _disp_ids[-3:])
DISP_LAST = 'F4B-E%02d' % _disp_ids[-1]
assert _disp_ids[-1] >= 33 and '\n## F4B-E23. ' in _disp and '\n## F4B-E06. ' in _disp and '\n## F4B-E09. ' in _disp
assert ('\n## %s. Decisiones del Owner antes de los contratos de PB-T2' % OWNER_PBT2_ENTRY) in _disp
_own = show(OWNER_PBT2)
for q, ans in (('Q-PBT2-1', 'Aplica sin cerrar; Cerrar avisa (Recomendado)'), ('Q-PBT2-2', 'Lo rechaza, como Actualizar (Recomendado)'),
               ('Q-PBT2-3', 'En PB-T4, con prueba ya (Recomendado)'), ('Q-PBT2-4', 'S' + chr(0xED) + ', con esa condici' + chr(0xF3) + 'n (Recomendado)')):
    _blk = _own[_own.index('## ' + q + '.'):]
    _blk = _blk[:_blk.index('\n## ', 3)] if '\n## ' in _blk[3:] else _blk
    assert ('**Respuesta del Owner:** ' + chr(0xAB) + ans + chr(0xBB)) in _blk, ('la respuesta del Owner', q)
assert 'BOM y biblioteca (Recomendado)' in show(CO + 'owner-f4b-dpb07-extensiondata-decisions.md'), 'F4B-E23'

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
    m = re.match(r'^\s*- \*\*((?:INV-PBT2[ABC]-)?\d\d[a-z]?)\.\s*(.*?)\*\*\s*(.*)$', first)
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
    'Fijar con pruebas nuevas, sin cambiar el producto, las obligaciones heredadas por el modo WORKSPACE de la ventana de Push Back: el paso 4 de '
    'D-PB-06 (los valores exactos y el estilo de cotas llegan al borrador), el aviso de D-PB-08 en WORKSPACE y el censo de controles en el camino al '
    'borrador, segun el paquete del contrato de PB-T2 v<n> y la lista cerrada CT-PBT2C-01..03. El RED son las pruebas nuevas y los tres mutantes de la '
    'tabla cerrada MU-T2C-01..03 en W y WC; el GREEN revierte los mutantes. Ninguna linea de src cambia entre BaseSha y CurrentSha; no se ejecuta '
    'AutoCAD. Si hiciera falta mutar el rack resuelto o el informe de modulos dentro del tiempo de vida del borrador: STOP y se reabre L-1 (decision D2 '
    'del Owner, F4B-E11).')
assert OBJECTIVE_PKG.count('v<n>') == 1
OBJECTIVE = OBJECTIVE_PKG.replace('v<n>', PKG_VERSION)
INV_OWN = [
    ('01', 'Alcance', 'git diff --name-only BaseSha..CurrentSha = la clase nueva; git diff BaseSha..CurrentSha -- src vacio.'),
    ('02', 'RED y GREEN', 'el RED son la clase y los tres mutantes, juntos; el GREEN es git restore --source=BaseSha de W y WC, y no toca tests/. El criterio: '
                          'Failed == Selected >= 3 en el filtro con ExpectRed, por asercion. Con una reparacion del RED (F4B-E35, RR): RedSha..RedRepairSha solo '
                          'toca la clase nueva, el criterio se repite entero sobre RedRepairSha, tambien las corridas aisladas de INV-PBT2C-03 con '
                          'la clase reparada, y RedRepairSha..CurrentSha es el GREEN de arriba (v2.7, V27-03).'),
    ('03', 'Cada mutante, solo', 'aplicado aislado sobre BaseSha, hace fallar por asercion el CT de su fila. La entrega lo transcribe con el TRX de cada '
                                 'corrida temporal.'),
    ('04', 'La atribucion', 'cada prueba existente que falle en el RED de PB-T2c o en las corridas aisladas de los mutantes de INV-PBT2C-03 se atribuye a '
                            'un mutante de la tabla cerrada MU-T2C-01..03 (v2.6, V26-07).'),
]
INV_NOTE = ('Nota (no va al contrato). Es el regimen de PB-T1a. MU-T2C-03 hace fallar CT-PBT2B-08 en Core, y por eso Core falla en el RED (section 16.9, '
            'n.o 9).')
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
C04_PKG = ('a mutant of the table, isolated, does not make the CT of its row fail by an assertion; or a new CT fails on BaseSha without mutants; or it '
           'fails by an exception. The Worker changes neither the CT nor the mutant.')
C13_PKG = ('doing the task would require changing W or WC other than by the three single-line substitutions of the closed table, or adding there any '
           'declaration, or keeping any of them in the GREEN.')
C15_PKG = 'CT-PBT2C-01 does not give the exact value or the style on BaseSha. It is a new fact: return to the Owner (OB-06.4, OB-06.5).'
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
                  'CT-PBT2C-03 o las lineas fijas de CT-PBT2C-01 y 02 (los exactos, la linea de pb-08-redondeo, las dos de estilo, las doce de '
                  'CT-PBTC-07 (PushBackDeclaredCorrectionSideBNoticeTests.cs:89-100, con  descartes=1 en la ultima de cada ventana) y la de '
                  '"Aplicar") no sean, byte a byte, las mismas en RedSha y en RedRepairSha (v2.9, V29-01 y V29-04).')
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
              'la v2.8 (con la v2.5, la v2.6 y la v2.7 en la historia), v23-delta.md, v24-delta.md, v25-delta.md, v26-delta.md, v27-delta.md, '
              'v28-delta.md, measure_pbt2b_t1c.py y el encargo literal de su comprobacion acotada y de su recomprobacion '
              '(independent-review-pbt2b-v23-check-prompt.md), junto al informe (v2.3, V23-06; v2.4, V24-07; v2.5, V25-04; v2.7, V27-07; v2.8, V28-06); '
              'en el de PB-T2c, ademas, esta v2.9, v29-delta.md y el encargo literal de su comprobacion acotada y de su recomprobacion '
              '(independent-review-pbt2c-v25-check-prompt.md), junto al informe (v2.6, V26-06; v2.7, V27-07; v2.9, V29-03);')
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
CI_RED_T2C = 'PB-T2c: tests en failure (CT-PBT2B-08 por MU-T2C-03, atribuido) y ui-tests en failure, build-ui en success.'
RED_CHAIN_HEAD = 'El RED de la cadena en una correccion (section 16.8): una correccion que toque RT o ChainRedFiles exige RED otra vez:'
RED_CHAIN_T2C = 'en PB-T2c, los tres mutantes.'
E_T2C = ['la tabla de atribucion;', 'los tres mutantes aislados;', 'el RED con su CI (tests y ui-tests en failure, build-ui en success, build-plugin omitido).']
LB_PKG = 'Linea base de la CI: la del cierre de PB-T2b, prevista: Core 19 094, UI 2 447. GREEN de PB-T2c: Core 19 094; UI + 3 = 2 450.'
CORR_PKG = ('Las correcciones: se leen con F4B-E25. Una ruta UNIT_CHANGE de git diff --name-only AuthorityRevision BaseSha no incumple 16.3 si se cumplen '
            'sus tres condiciones, que comprueba el Controller. Las EXTERNAL, nunca. No hace falta reemitir el contrato para corregir.')
UC_T2C_PKG = 'PB-T2c: W y WC, con la seccion "solo los tres mutantes del RED (MU-T2C-01 a 03), revertidos por el GREEN", como en PB-T1a.'
STD_LINE = ('StopConditions estandar (17): S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08, con P-01 = "' + P01_K + '". PB-T2a y '
            'PB-T2b se emitieron con la J (F4B-E19); PB-T2c, con la K (F4B-E37; v2.9, V29-06).')
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
    'E37-22': ('F4B-E37, F4B-E21 y F4B-E22 (owner-config-snapshot-k-decision.md, owner-f4b-opaque-collections-decision.md, owner-pbt1b-rework-decisions.md)',
               'La Snapshot K (P-01), que sustituye a la J de F4B-E19 (v2.9, V29-06); la sesion pone las ocho colecciones opacas; las lecciones M10 y M14'),
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
    'git diff <cierre de PB-T2b> <BaseSha> -- src vacio;', 'los textos de los tres mutantes [T2b];',
    'la tabla de gestos de CT-PBT1C-02, con lo esperado de cada fila de CT-PBT2C-03, medida con una corrida temporal de UI de los 49 gestos en '
    'WORKSPACE, fuera de todo commit y con TRX (INV-10; v2.6, V26-02);',
    'La comprobacion acotada: un revisor independiente, en solo lectura, sobre este paquete, la seccion de la subtarea y la salida de sus anclajes. Su '
    'resultado, EMITIBLE o NO EMITIBLE, se custodia en F4B-DRAFTS/pbt2/.',
    RR_CUSTODY,
    'La delegacion (F4B-E21): el Controller deja vacias las ocho colecciones opacas, y la sesion las pone byte a byte. La comparacion byte a byte, A1-A8 '
    'y nc4 se hacen sobre la delegacion final.',
    '[P];', '[PT1];', 'este paquete, el registro y v2-delta.md;', 'IR1 y las comprobaciones acotadas;',
    'las decisiones del Owner F4B-E03, E09, E11, E19, E21, E22, E23, E24 y E25 (sus archivos de coordinator/) y sus vueltas;',
    'f4b-dispositions.md y los cierres de PB-T1b y PB-T1c;', 'la Proposal V5 (docs/initiatives/I-64-proposal-v5.md);', 'el propio contrato.',
    UC_T2C_PKG,
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
    RED_CHAIN_HEAD, RED_CHAIN_T2C, CI_RED_HEAD, CI_RED_T2C,
    'EligibleCells: Worker anthropic/subagent/claude-sonnet-5-5 con effort high; Controller openai/process/gpt-6-luna con effort high. Vencen el 2026-12-29 '
    '(model-catalog.md:14, :51, :97). Se comprueban en cada emision y delegacion (A7).',
    'Invariantes comunes (cada contrato los numera INV-PBT2x-07 a 12 y 14). Lo normativo va en el contrato; las notas no (v2, V2-27).',
    RR_COMMON,
    'ExpectedEvidence comun (v2, V2-28):',
] + E_COMMON + RR_60
EXPECT_63 = [
    'AllowedWriteScope (3): W y WC (solo los mutantes del RED), y tests/RackCad.UI.Tests/PushBackWorkspaceCarryoverTests.cs.',
    'ForbiddenWriteScope: 60 de src/ (59 hoy, mas el archivo del modo de PB-T2b) y lo comun, con las clases de PB-T2a y PB-T2b.',
    'Authorities: las comunes, y W y WC como UNIT_CHANGE ("solo los tres mutantes del RED, revertidos por el GREEN").',
    'En el RED, los filtros sin ExpectRed solo pueden fallar por los mutantes, con la tabla de atribucion. En el GREEN, todos en verde.',
    'StopConditions: las 17 estandar, C08, C11, C12, C17, las comunes, y:', 'ExpectedEvidence: la comun, y:', LB_PKG,
] + E_T2C
EXPECT_54 = [
    'Regimen: el de PB-T1a, copiado en el contrato:',
    'solo pruebas nuevas, en tests/RackCad.UI.Tests/PushBackWorkspaceCarryoverTests.cs, con los ayudantes de PushBackWorkspaceSupport (que no se edita) y '
    'privados propios;',
    'el RED es esa clase y los tres mutantes de la tabla cerrada, juntos; el GREEN los revierte con git restore --source=<BaseSha> de W y WC;',
    'cada mutante, aislado sobre BaseSha, hace fallar por asercion el CT de su fila. Las pruebas existentes que fallen en el RED, o en una corrida aislada '
    'de un mutante (INV-PBT2C-03), se atribuyen a un mutante en una tabla de atribucion, como T-8 de PB-T1a (v2.6, V26-05).',
    'La tabla cerrada de mutantes [T2b] (una sola linea cada uno, en el texto que deja PB-T2b; el generador lo comprueba sobre BaseSha; MU-T2C-03, por '
    'contenido, con lo que ya exige R4 de CT-PBT2B-08: v2.6, V26-03):',
    'Por eso, en el RED, Core falla (section 16.9, n.o 9) y build-plugin sale omitido.',
]
CT_METHODS = ['CT_PBT2C_01_InWorkspaceTheDraftKeepsTheExactValuesAndTheNamedStyle', 'CT_PBT2C_02_InWorkspaceTheSideBNoticeShowsExactlyAsInRackeditar',
              'CT_PBT2C_03_InWorkspaceNoControlIsTheSourceOfTruthOnTheWayToTheDraft']
REQ_T2C_ROW4 = '| UI y Core | las familias de PB-T1, PB-TC, PB-TC2, PB-T0 y T1B del section 6.2 | los del section 6.2 | false |'
PKG_CHECKS = []
REQ_PKG_ROWS = None
FIXED_62 = None
if PKG_OK:
    SEC63, SEC60, SEC1, SEC54, SEC62 = section('### 6.3 '), section('### 6.0 '), section('## 1. '), section('### 5.4 '), section('### 6.2 ')
    F63, F60, F1, F54 = ([md_fold([l]) for l in s if l.strip()] for s in (SEC63, SEC60, SEC1, SEC54))
    _obj = [l for l in SEC63 if l.startswith('- **Objective** (ASCII): ' + chr(0xAB))]
    assert len(_obj) == 1 and ascii_fold(_obj[0][_obj[0].index(chr(0xAB)) + 1:_obj[0].rindex(chr(0xBB))]) == OBJECTIVE_PKG, 'el Objective del paquete difiere'
    for x in EXPECT_63:
        once(F63, x, 'section 6.3')
    for x in EXPECT_60:
        once(F60, x, 'section 6.0')
    for x in EXPECT_54:
        once(F54, x, 'section 5.4')
    assert F63.count(REQ_T2C_ROW4) == 1, 'la fila de las familias del section 6.2 en la tabla de PB-T2c'
    # los invariantes propios: cuatro vinetas y una nota al mismo nivel, que no va al contrato
    _a = SEC63.index('- **Invariantes propios:**')
    _b = [k for k, l in enumerate(SEC63) if l.startswith('- **`StopConditions`:**')][0]
    _items = bullets(SEC63[_a + 1:_b], '  ', '    ')
    _notes = [f for f, _ in _items if f.strip().startswith('- **Nota (no va al contrato).**')]
    assert len(_notes) == 1 and md_fold(_notes) == INV_NOTE, ('la nota de los invariantes de PB-T2c', _notes)
    _own = [inv_bullet(f, subs) for f, subs in _items if f not in _notes]
    assert _own == INV_OWN, ('los invariantes propios del paquete difieren', _own)
    # las paradas propias: tres vinetas
    _c = [k for k, l in enumerate(SEC63) if l.startswith('- **`ExpectedEvidence`:**')][0]
    _stops = [md_fold([l]) for l in SEC63[_b + 1:_c] if l.strip()]
    assert _stops == ['PBT2C-C04: ' + C04_PKG, 'PBT2C-C13 (v2, V2-26): ' + C13_PKG, 'PBT2C-C15: ' + C15_PKG], ('las paradas propias del section 6.3', _stops)
    # los invariantes comunes del section 6.0
    _h = [k for k, l in enumerate(SEC60) if l.startswith('**Invariantes comunes**')][0]
    _blk = []
    for l in SEC60[_h + 1:]:
        if not l.strip():
            break
        _blk.append(l)
    _com = [inv_bullet(f, s) for f, s in bullets(_blk, '', '  ')]
    assert _com == INV_COMMON, ('los invariantes comunes del paquete difieren', _com)
    # la lista cerrada de CT, la tabla de mutantes y los textos de CT-PBT2C-01 a 03 (v2.4, V24-14)
    _ct = [l for l in F54 if re.match(r'^\| \d\d ', l)]
    assert [re.match(r'^\| (\d\d) (\w+) \|', l).group(2) for l in _ct] == CT_METHODS, 'los CT de la lista cerrada del section 5.4'
    _mu = [md_fold([l]) for l in SEC54 if l.startswith('| MU-T2C-')]
    _mu_want = ['| %s | %s (%s) | %s | %s |' % (m[0], m[1], m[2], m[4].replace('|', chr(92) + '|'), m[5]) for m in MUTANTS if m[3] == 'linea'] + [
        '| MU-T2C-03 | ApplyToDraft (W) | la unica linea de ApplyToDraft que contiene %s | en esa linea, %s pasa a %s |' % (MU3_FRAG, MU3_IN, MU3_OUT)]
    assert _mu == _mu_want, ('la tabla cerrada de mutantes', _mu)
    assert ('el exacto en diez filas (' + EXACT_PKG + ') y 6 en F4') in _ct[0], 'CT-PBT2C-01: los valores exactos'
    assert 'pb-08-redondeo WORKSPACE aplicado=True peticion=False AnnotationScale=1.23456 PalletHeight=61.23456 sistema.AnnotationScale=1.23456' in _ct[0]
    assert 'con STANDARD-ISO y SIMPLEX, como CT-PBTC-05' in _ct[0] and 'aplicado es el del section 5.0: el borrador cambio de objeto (v2.4, V24-14)' in _ct[0]
    assert 'sus doce lineas literales (PushBackDeclaredCorrectionSideBNoticeTests.cs:89-100)' in _ct[1]
    assert ('pb-01-base declarar B y aplicar aviso=Visible aplicar-habilitado=True llamadas=1 borrador-B-presente=True diseno-del-borrador-compuesto=False '
            'descartes=0') in _ct[1]
    for fp in ('con Compara = true (los 49 del censo, "Restaurar" incluido)', 'Los cuatro gestos de D2 (campo-en-error, calculo-invalido, '
               'caja-escrita-sin-salir y cerrar-sin-pedir) no entran', '"Aplicar" aplica: aplicado=True igual-a-la-ventana=True diseno-igual=True descartes=0',
               'aplicado=False igual-a-la-ventana=True diseno-igual=True descartes=0',
               'aplicado=False igual-a-la-ventana=False diseno-igual=False descartes=1', 'Lo esperado de cada fila se fija en la remedicion sobre el cierre de PB-T2b'):
        assert fp in _ct[2], ('V24-14: CT-PBT2C-03', fp)
    # v2.6: I-01 (las fuentes, en el borrador), M-01 (como se mide la tabla) y M-03 (como la copia el Worker)
    V26_FP = [
        (0, 'Las fuentes (v2.6, V26-01; CA-T2c I-01): todo se lee del borrador, justo tras "Aplicar", con c = Pasada(Copy(draft.CurrentDraftState), A)'),
        (0, 'en las filas F y en AnnotationScale y PalletHeight de pb-08-redondeo, Persist(c.Design) (que es DisenoDe(draft.CurrentDraftState, A))'),
        (0, 'sistema.AnnotationScale = c.System.Structure.AnnotationScale'),
        (0, 'cotas>0 y estilos salen de las instancias Dimension de los planos de c'),
        (0, 'diseno = c.Design.Structure.DimensionStyle y sistema = c.System.Structure.DimensionStyle'),
        (0, 'con el Cotas de CT-PBTC-05 (PushBackDeclaredCorrectionDimensionStyleTests.cs:82-92) aplicado a c y no a w.LastComputation'),
        (0, 'Nunca de la peticion clasica (DesignToInsert y SystemToInsert'),
        (1, 'diseno-del-borrador-compuesto = c.Design.IsComposite, con c = Pasada(Copy(draft.CurrentDraftState), A) leido justo tras "Aplicar" (v2.6, V26-01)'),
        (2, 'Como se mide (v2.6, V26-02; CA-T2c M-01): la remedicion es una corrida temporal de UI de los 49 gestos en WORKSPACE sobre el cierre de PB-T2b, '
            'fuera de todo commit y con TRX (INV-10)'),
        (2, 'es un hecho nuevo: otra version del paquete y, si muestra una diferencia entre CLASSIC y WORKSPACE, el Owner'),
        (2, 'Como la copia el Worker (v2.6, V26-04; CA-T2c M-03): las 49 lineas esperadas, en su orden, de la tabla custodiada en pbt2c-anchors-out.txt, que '
            'es autoridad, sin las lineas'),
        (2, 'porque:, que son de la remedicion'),
        (2, '<fila> es el nombre del gesto en Gestos()'),
    ]
    for k, fp in V26_FP:
        assert fp in _ct[k], ('V26: CT-PBT2C-0%d' % (k + 1), fp)
    assert ('Se leen del borrador, con Pasada(Copy(draft.CurrentDraftState), A) justo tras "Aplicar", y nunca de la peticion clasica, que en WORKSPACE es '
            'nula (v2.6, V26-01).') in md_fold(section('### D-PBT2-13. ')), 'V26-01: D-PBT2-13'
    # la tabla de mutantes nombra el CT de su fila: MU-01 en CT-PBT2C-01, MU-02 en 02 y MU-03 en 03
    assert [l.rstrip(' |').split('| ')[-1] for l in _ct] == ['MU-T2C-01', 'MU-T2C-02', 'MU-T2C-03'], 'cada CT con su mutante'
    # la regla del section 5.0 que usa PB-T2c (aplicado) y la cabecera y los registros de la v2.4, la v2.5 y la v2.6
    _f50 = [md_fold([l]) for l in section('### 5.0 ') if l.strip()]
    assert any('aplicado (PB-T2c) = !ReferenceEquals(actual, draft.CurrentDraftState), con actual = draft.CurrentDraftState tomado justo antes del clic en "Aplicar"'
               in l for l in _f50), 'el section 5.0: aplicado'
    _ko = [k for k, l in enumerate(PKG_LINES[:40]) if l.startswith('**Lo que pide el Owner**')]
    assert len(_ko) == 1, 'la cabecera termina en "Lo que pide el Owner", en las 40 primeras lineas (v2.9, V29-02)'
    _hdr = '\n'.join(PKG_LINES[:_ko[0]])  # la cabecera entera, con las lineas de emision que tenga delante (v2.9, V29-02)
    assert '- **v2.4:** la comprobaci' in _hdr and '**EMITIBLE TRAS CORRECCIONES, 0/3/7**' in _hdr, 'la cabecera de la v2.4'
    assert '- **v2.5:** la recomprobaci' in _hdr and '**EMITIBLE, nuevos B 0 / I 0 / M 4**' in _hdr, 'la cabecera de la v2.5'
    _r24 = [l for l in section('## Registro de cambios v2.3 ') if l.startswith('| V24-')]
    assert [re.match(r'^\| (V24-\d\d) \|', l).group(1) for l in _r24] == ['V24-%02d' % k for k in range(1, 17)], 'el registro V24'
    _r25 = [l for l in section('## Registro de cambios v2.4 ') if l.startswith('| V25-')]
    assert [re.match(r'^\| (V25-\d\d) \|', l).group(1) for l in _r25] == ['V25-%02d' % k for k in range(1, 5)], 'el registro V25'
    assert ('- **v2.6:** la comprobaci' in _hdr and '`F4B-DRAFTS/pbt2/independent-review-pbt2c-v25-check.md`, citada **CA-T2c**' in _hdr
            and '**EMITIBLE TRAS CORRECCIONES, B 0 / I 1 / M 5**' in _hdr and 'Solo cambia PB-T2c: PB-T2b se emite con la v2.5.' in _hdr), 'la cabecera de la v2.6'
    assert ('Su recomprobaci' + chr(0xF3) + 'n (la ' + chr(0xA7) + '9 de CA-T2c) da **EMITIBLE, nuevos B 0 / I 0 / M 3**') in _hdr, 'la cabecera de la v2.6: su seccion 9'
    # v2.7 (F4B-E35, RR): la cabecera y el registro V27, cada fila con su fuente
    assert RR_HDR in _hdr and 'PB-T2b se emite ya con la v2.7.' in _hdr, 'la cabecera de la v2.7'
    _r27 = [l for l in section('## Registro de cambios v2.6 ') if l.startswith('| V27-')]
    assert [re.match(r'^\| (V27-\d\d) \|', l).group(1) for l in _r27] == ['V27-%02d' % k for k in range(1, 8)], 'el registro V27'
    for _l in _r27:
        assert _l.split(' | ')[1].startswith(RR_REG_SRC), ('el registro V27: la fuente de cada fila', _l[:12])
    # v2.8: las recomprobaciones de la v2.7 (V28-01 a V28-06)
    rr28_check(_hdr, [l for l in section('## Registro de cambios v2.7 ') if l.startswith('| V28-')])
    # v2.9: la seccion 11 de CA-T2c (V29-01 a V29-03)
    assert ('- **v2.9:** la recomprobaci' in _hdr and ('la ' + chr(0xA7) + '11 de CA-T2c) da **EMITIBLE, nuevos B 0 / I 0 / M 2**: M-09 se aplica aqu') in _hdr
            and ('la ' + chr(0xA7) + '12 de CA-T2c, da **EMITIBLE, nuevos B 0 / I 0 / M 2**: M-11 y M-12 se aplican en esta misma v2.9') in _hdr
            and 'La Snapshot K de F4B-E37 sustituye a la J en P-01 de PB-T2c (V29-06).' in _hdr and 'Solo cambia PB-T2c.' in _hdr), 'la cabecera de la v2.9'
    _r29 = [l for l in section('## Registro de cambios v2.8 ') if l.startswith('| V29-')]
    _s29 = [('V29-01', '**CA-T2c ' + chr(0xA7) + '11 M-09**'), ('V29-02', 'La sesi' + chr(0xF3) + 'n (D-23)'), ('V29-03', 'La sesi' + chr(0xF3) + 'n'),
            ('V29-04', '**CA-T2c ' + chr(0xA7) + '12 M-11**'), ('V29-05', '**CA-T2c ' + chr(0xA7) + '12 M-12**'),
            ('V29-06', '**F4B-E37 (Owner)**')]
    assert [re.match(r'^\| (V29-\d\d) \|', l).group(1) for l in _r29] == [r for r, _ in _s29], 'el registro V29'
    for _l, (_rid, _src) in zip(_r29, _s29):
        assert _l.split(' | ')[1] == _src, ('el registro V29: la fuente de cada fila', _rid)
    _r26 = [l for l in section('## Registro de cambios v2.5 ') if l.startswith('| V26-')]
    assert [re.match(r'^\| (V26-\d\d) \|', l).group(1) for l in _r26] == ['V26-%02d' % k for k in range(1, 9)], 'el registro V26'
    # V26-07: M-04 tambien en INV-PBT2C-04; V26-08: M-06, de la seccion 9
    for k, ca in enumerate(['I-01', 'M-01', 'M-02', 'M-03', 'M-04', 'M-05', 'M-04', 'M-06'], 1):
        assert ('**CA-T2c %s**' % ca) in _r26[k - 1], ('el registro V26: cada hallazgo de CA-T2c tiene su fila', ca)
    _v8 = md_fold([_r26[7]])
    for fp in ('no esta en una sola linea fisica de W, o la firma de ApplyToDraft no es private void ApplyToDraft(), el generador de PB-T2c aborta y '
               'la emision de PB-T2c se para', 'la sesion redefine la linea del mutante por contenido en una version nueva del paquete, con su '
               'comprobacion acotada. No es decision del Owner, salvo que el cambio altere el comportamiento'):
        assert fp in _v8, ('V26-08 (CA-T2c M-06)', fp)
    assert all(w in M06 for w in ('una sola linea fisica de W', 'private void ApplyToDraft()', 'la emision de PB-T2c se para',
                                  'version nueva del paquete, con su comprobacion acotada', 'no es decision del Owner, salvo que el cambio altere el '
                                  'comportamiento')), 'el mensaje de aborto de MU-T2C-03 dice lo de V26-08'
    # section 1, [OD] e IR1
    for k, (c1, c2) in S1.items():
        once(F1, '| %s | %s |' % (c1, c2), 'section 1, ' + k)
    assert ('> ' + OD_ROW) in PKG_LINES[:40] and '(`F4B-DRAFTS/pbt2/independent-review-v1.md`, citada **IR1**)' in _hdr  # en toda la cabecera (v2.9, V29-05)
    REQ_PKG_ROWS = SEC63
    FIXED_62 = SEC62
    PKG_CHECKS = ['Objective', 'section 6.3 (%d lineas)' % len(EXPECT_63), 'section 6.0 (%d lineas)' % len(EXPECT_60), 'section 5.4 (%d lineas)' % len(EXPECT_54),
                  'INV propios literales sin su nota', 'paradas C04, C13 y C15', 'INV comunes (10 con V23-01)', 'P-01 K (V29-06)', 'CT-PBT2C-01..03',
                  'la tabla de mutantes MU-T2C-01..03', 'los valores exactos de CT-PBT2C-01', 'las doce lineas de CT-PBTC-07', 'V24-14 (CT-PBT2C-03)',
                  'section 5.0 (aplicado)', 'cabecera y registros V24-01..16, V25-01..04 y V26-01..08', 'V26-01..05 (CT-PBT2C-01..03 y D-PBT2-13)', 'V26-08 y el aborto de MU-T2C-03', 'section 1 (%d filas)' % len(S1), 'ExpectedEvidence comun y de 6.3',
                  'linea base de la CI', 'custodia', 'F4B-E25', 'RequiredTests (6.3 y las familias de 6.2)',
                  'la regla RR de F4B-E35 (V27-01 a V27-07: section 6.0, C07, C18, INV-PBT2C-02, cabecera y registro)',
                  'V28-01 a V28-06 (la verificacion con una reparacion, la ayuda de RR-3, el trailer, C07 y C18, cabecera y registro)',
                  'V29-01 a V29-06 (M-09, M-11, la custodia, cabecera y registro; M-12, IR1 en la cabecera; la Snapshot K)']


def package_required(sec, lead):
    """Las filas de una tabla de RequiredTests: (proyecto, filtro, minimo del paquete, ExpectRed, en negrita)."""
    rows = []
    for l in sec:
        m = re.match(r'^' + lead + r'\| (UI|Core) \| (.*?) \| (.*?) \| (true|false) \|$', l)
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


# ---------------------------------------------------------------- RequiredTests: (proyecto, filtro, minimo, ExpectRed, clase): 'new', 't2b', 'pt2a', 'fixed'
UIP, CP = UT + 'RackCad.UI.Tests.csproj', TR + 'RackCad.Tests.csproj'
FIXED = [  # las familias del section 6.2 (sus filas sin ExpectRed y sin clases de PB-T2a)
    ('Core', 'PushBackWindowOnStateSourceGuardTests', 1), ('Core', 'PushBackWindowSharedReferenceGuardTests', 1), ('Core', 'PushBackSharedReferenceGuardTests', 2),
    ('Core', 'PushBackEditableState', 13), ('Core', 'PushBackEditorOutputGate', 1), ('Core', 'PushBackClassicCharacterization', 10),
    ('Core', 'PushBackFullEditorCharacterization', 3), ('Core', 'PushBackModuleEditorCharacterizationTests', 15), ('Core', 'I55DeferredProductFixCharacterizationTests', 3),
    ('Core', 'NamespaceFolderGuardTests', 7), ('Core', 'Workspace', 3271),
    ('UI', 'PushBackWindowOnStateTests', 2), ('UI', 'PushBackEditableStateParityTests', 1), ('UI', 'PushBackStateNet', 14), ('UI', 'PushBackClassicCharacterization', 55),
    ('UI', 'PushBackFullEditorCharacterization', 16), ('UI', 'PushBackDeclaredCorrection', 14), ('UI', 'PushBackCompositeWindowTests', 52),
    ('UI', 'PushBackCompositeLifecycleWiringTests', 7), ('UI', 'PushBackCompositeLoadExistingModulesTests', 3), ('UI', 'PushBackBothModeCellBeamOverrideTests', 10),
    ('UI', 'PushBackModuleEditorWindowTests', 13), ('UI', 'PushBackAdvancedRackParametersWindowTests', 9), ('UI', 'PushBackRichPreviewTests', 9),
    ('UI', 'PushBackLibrarySaveGateTests', 5), ('UI', 'DialogWindowCharacterizationTests', 20), ('UI', 'BlankFront', 33), ('UI', 'WindowCensusGuardTests', 8),
    ('UI', 'RichEditor', 25), ('UI', 'Workspace', 547), ('UI', 'Cantilever', 67),
]
REQ_SPEC = [('UI', 'PushBackWorkspaceCarryoverTests', 3, True, 'new'),
            ('UI', 'PushBackWorkspaceModeTests', 3, False, 't2b'), ('UI', 'PushBackWorkspaceApplyCloseTests', 4, False, 't2b'),
            ('UI', 'PushBackFieldValidationConcordanceTests', 1, False, 't2b'),
            ('Core', 'PushBackWorkspaceModeSourceGuardTests', 1, False, 't2b'), ('Core', 'PushBackWorkspaceSharedReferenceGuardTests', 1, False, 't2b'),
            ('Core', 'PushBackDraftStatePortTests', 3, False, 'pt2a'), ('Core', 'PushBackDraftPortSharedReferenceGuardTests', 1, False, 'pt2a')] + [
    (p, f, m, False, 'fixed') for p, f, m in FIXED]
assert len(REQ_SPEC) == 39 and [r[1] for r in REQ_SPEC if r[3]] == ['PushBackWorkspaceCarryoverTests']
if REQ_PKG_ROWS is not None:
    _pkg_req = package_required(REQ_PKG_ROWS, '  ')
    assert [(p, f, m, e, k == 'new') for p, f, m, e, k in REQ_SPEC[:8]] == _pkg_req, ('la tabla de RequiredTests del section 6.3 cambio', _pkg_req)
    _fixed62 = [(p, f, m) for p, f, m, e, bold in package_required(FIXED_62, '  ') if not e and not bold
                and f not in ('PushBackDraftStatePortTests', 'PushBackDraftPortSharedReferenceGuardTests')]
    assert _fixed62 == FIXED, ('las familias del section 6.2 cambiaron', _fixed62)
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
COUNTS_BASE = {  # los de 0dc623ca y 17872be8 (los mismos que en make_pbt2b_contract.py)
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
assert sorted(COUNTS_BASE) == sorted((p, f) for p, f, _, _, k in REQ_SPEC if k == 'fixed')
COUNTS_CHANGED = ['%s %s %s -> %s' % (p, f, COUNTS_BASE[(p, f)], COUNTS[(p, f)]) for (p, f) in COUNTS_BASE if COUNTS[(p, f)] != COUNTS_BASE[(p, f)]]
soft(not COUNTS_CHANGED, 'el recuento de los RequiredTests de las familias cambio desde 17872be8 (la sesion lo explica y pone al dia COUNTS_BASE): ' + '; '.join(COUNTS_CHANGED))
_prev_req = {}
for prev in (pbt1c, pbt2a):
    _prev_req.update({(r['Project'], r['Filter']): r['MinSelected'] for r in prev['RequiredTests']})
REQ, REQ_REPORT, T2B_MISSING = [], [], []
for p, f, pkg_min, er, kind in REQ_SPEC:
    proj = UIP if p == 'UI' else CP
    filt = 'FullyQualifiedName~%s.%s' % ('RackCad.UI.Tests' if p == 'UI' else 'RackCad.Tests', f)
    classes, run, skip, member = COUNTS[(p, f)]
    note = None
    if kind == 'new':
        assert classes == 0 and run == 0, ('la clase nueva ya existe', f)
    elif kind == 't2b':
        if classes == 0:
            T2B_MISSING.append(f)
        else:
            assert classes == 1 and run >= pkg_min and not skip, ('la clase de PB-T2b', f, COUNTS[(p, f)])
    elif kind == 'pt2a':
        assert classes == 1 and run >= pkg_min, ('la clase de PB-T2a', f, COUNTS[(p, f)])
    else:
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
soft(not T2B_MISSING, 'las clases de PB-T2b de los RequiredTests aun no estan en la revision (de su cierre): ' + ', '.join(T2B_MISSING))
for proj, filt, mins, er in REQ:
    assert in_head(proj), proj
    if (proj, filt) in _prev_req:
        assert _prev_req[(proj, filt)] <= mins, (filt, _prev_req[(proj, filt)], mins)


# ---------------------------------------------------------------- invariantes
def inv_text(idd, title, body):
    return 'INV-PBT2C-%s%s: %s' % (idd, (' (%s)' % lc(title)) if title else '', body)


_order = ['01', '02', '03', '04', '07', '08', '09', '10', '11', '12', '14']
_by = {x[0]: x for x in INV_OWN + INV_COMMON}
assert sorted(_by) == sorted(_order)
INV = [inv_text(*_by[k]) for k in _order]
assert len(INV) == 11 and len({i.split(' ')[0].rstrip(':') for i in INV}) == 11 and not [i for i in INV if 'Nota' in i]


# ---------------------------------------------------------------- la evidencia
# ---- ci_base_check (inicio)
def ci_base_check(git, ci, close, expected, explained):
    """La linea base de la CI del cierre de PB-T2b, atada a PT2B_CLOSE. Devuelve los errores (vacia si vale)."""
    if not close:
        return ['CI_BASE_T2B sin PT2B_CLOSE: la linea base se ata al cierre de PB-T2b']
    if not (isinstance(ci, tuple) and len(ci) == 5 and re.fullmatch(r'\d+', str(ci[0])) and re.fullmatch(r'[0-9a-f]{8,40}', str(ci[1]))
            and all(isinstance(x, int) for x in ci[2:])):
        return ['CI_BASE_T2B mal formada: %r' % (ci,)]
    run, sha, core, ui, skip = ci
    if git('cat-file', '-e', sha + '^{commit}').returncode != 0:
        return ['el SHA de la CI no esta en el almacen de objetos: ' + sha]
    errors = []
    if git('diff', '--quiet', sha, close, '--', 'src', 'tests').returncode != 0:
        errors.append('el SHA de la CI (%s) no tiene el src y los tests del cierre de PB-T2b (%s)' % (sha[:8], close[:8]))
    if (core, ui, skip) != expected and not explained:
        errors.append('las cifras de la CI del cierre de PB-T2b (Core %d, UI %d, Skipped %d) difieren de las previstas (Core %d, UI %d, Skipped %d): '
                      'la sesion las explica por el diff de PB-T2b en CI_T2B_EXPLAINED' % ((core, ui, skip) + expected))
    return errors
# ---- ci_base_check (fin)


soft(CI_BASE_T2B, 'falta CI_BASE_T2B (la CI del cierre de PB-T2b): la ExpectedEvidence lleva la linea base prevista del section 6.3')
if CI_BASE_T2B:
    _err = ci_base_check(git, CI_BASE_T2B, PT2B_CLOSE, CI_T2B_EXPECTED, CI_T2B_EXPLAINED)
    assert not _err, _err
    _run, _sha, _cc, _uu, _ss = CI_BASE_T2B
    CI_TXT = ('la linea base del cierre de PB-T2b (run %s sobre %s: Core %d Passed; UI %d resultados, %d Skipped y %d Passed): Core + 0 = %d Passed; '
              'UI + 3 (CT-PBT2C-01 a 03) = %d resultados: 0 Failed, %d Skipped y %d Passed, o la diferencia explicada por el diff'
              % (_run, _sha[:8], _cc, _uu, _ss, _uu - _ss, _cc, _uu + 3, _ss, _uu + 3 - _ss))
    if (_cc, _uu, _ss) != CI_T2B_EXPECTED:
        assert all(ord(ch) < 0x80 for ch in CI_T2B_EXPLAINED)
        CI_TXT += ' (frente a la prevista del section 6.3, Core %d y UI %d con %d Skipped: %s)' % (CI_T2B_EXPECTED + (CI_T2B_EXPLAINED,))
else:
    _c2, _u2, _s2 = CI_T2B_EXPECTED
    CI_TXT = ('la linea base prevista del cierre de PB-T2b (section 6.3 del paquete: el GREEN de PB-T2b, Core %d Passed y UI %d resultados con %d '
              'Skipped): Core + 0 = %d Passed; UI + 3 (CT-PBT2C-01 a 03) = %d resultados: 0 Failed, %d Skipped y %d Passed, o la diferencia explicada '
              'por el diff' % (_c2, _u2, _s2, _c2, _u2 + 3, _s2, _u2 + 3 - _s2))
assert (CI_T2B_EXPECTED[0], CI_T2B_EXPECTED[1] + 3, CI_T2B_EXPECTED[1] + 3 - CI_T2B_EXPECTED[2]) == (19094, 2450, 2429)  # LB_PKG


def cap(x):
    return x[0].upper() + x[1:]


def dot(x):
    return x[:-1] + '.' if x.endswith(';') else x


EVID = [
    cap(dot(E_ANTES)),
    CI_RED_HEAD + ' ' + CI_RED_T2C + ' ' + CI_RR,
    cap(dot(E_RED)),
    RED_CHAIN_HEAD + ' ' + RED_CHAIN_T2C,
    cap(E_GREEN[:-1]) + ', con los conteos de ' + CI_TXT + '.',
    cap(dot(E_LOCAL)),
    cap(dot(E_ENTREGA)),
    cap(E_DESPUES),
    RR_VERIF_EVID + ' '.join([RR_V_N10, RR_V_N11, RR_V_CTRL, RR_V_T2C, RR_V_AYUDA_T2C, RR_V_INDICIOS]),  # v2.8, V28-01 y V28-02
    'De PB-T2c (section 6.3 del paquete): ' + ' '.join(E_T2C),
]
assert 'build-plugin sale omitido por su needs' in EVID[1] and 'que es el trailer real de los commits RED, "RED repair" (si lo hay) y GREEN' in EVID[6]
assert 'con las colecciones opacas vacias' in EVID[0] and 'esas colecciones puestas por la sesion' in EVID[0]  # F4B-E21

# ---------------------------------------------------------------- paradas
T2A_STOP = {s['Id']: s['Text'] for s in pbt2a['StopConditions']}


def prefixed(text):
    """El texto de PB-T2a, cambiando solo el prefijo (section 6.0; C11 ya trae la referencia de V22-01)."""
    return text.replace('PBT2A', 'PBT2C').replace('PB-T2a', 'PB-T2c')


C08, C11, C12 = prefixed(T2A_STOP['PBT2A-C08']), prefixed(T2A_STOP['PBT2A-C11']), prefixed(T2A_STOP['PBT2A-C12'])
assert C08 == T2A_STOP['PBT2A-C08'] and C12 == T2A_STOP['PBT2A-C12'] and C11.endswith('neither one-off acceptance extends to PB-T2c')
assert ('in ' + C11_REF + ';') in C11 and 'PBT2C-NC2-EXACT-WHITELIST-01' in C11 and 'PBT2A' not in C11 and 'PB-T2a' not in C11
assert C11 == T2A_STOP['PBT2A-C11'].replace('PBT2A', 'PBT2C').replace('PB-T2a', 'PB-T2c') and C11.count('PBT2C') == T2A_STOP['PBT2A-C11'].count('PBT2A')
if PBT2B_OK:
    _t2b_stop = {s['Id']: s['Text'] for s in pbt2b['StopConditions']}
    assert C11 == _t2b_stop['PBT2B-C11'].replace('PBT2B', 'PBT2C').replace('PB-T2b', 'PB-T2c'), 'la C11 de PB-T2b y la de PB-T2c'
C17 = T2A_STOP['PBT2A-C17']
assert 'The lifetime includes that of the WORKSPACE window (section 6.0 of the package).' in C17 and 'PB-T2a' not in C17
C02 = T2A_STOP['PBT2A-C02'].replace('PB-T1a, PB-T1b or PB-T1c test', 'PB-T1a, PB-T1b, PB-T1c, PB-T2a or PB-T2b test')
assert 'PB-T2b test' in C02
C02 += (' (in the RED of PB-T2c and in the isolated mutant runs of INV-PBT2C-03 an existing test may fail only by a mutant of the closed table '
        'MU-T2C-01..03, attributed as INV-PBT2C-04 says; v2.6, V26-05)')
C09 = T2A_STOP['PBT2A-C09'] + ' (C09 governs the new tests; the configurator layout written by existing tests is attributed as INV-PBT2C-10 says; v2.3, V23-01)'
STOPS = P_STOPS_T2C + [
    {'Id': 'PBT2C-C01', 'Text': 'doing it right would require writing outside the AllowedWriteScope (W and WC, only for the three single-line mutants '
                                'MU-T2C-01..03 of the closed table in the RED, reverted by the GREEN, and the new UI test file '
                                'PushBackWorkspaceCarryoverTests.cs)'},
    {'Id': 'PBT2C-C02', 'Text': C02},
    {'Id': 'PBT2C-C03', 'Text': 'an observable change of RACKEDITAR, RACKPUSHBACK (RPB) or the menu against the base appears (in PB-T2c no line of src '
                                'changes between BaseSha and CurrentSha: the RED mutants are reverted by the GREEN): STOP and back to the Owner (D-16, OB-RE.1)'},
    {'Id': 'PBT2C-C04', 'Text': C04_PKG},
    {'Id': 'PBT2C-C05', 'Text': T2A_STOP['PBT2A-C05']},
    {'Id': 'PBT2C-C06', 'Text': T2A_STOP['PBT2A-C06']},
    {'Id': 'PBT2C-C07', 'Text': T2A_STOP['PBT2A-C07'] + '; ' + C07_RR},
    {'Id': 'PBT2C-C08', 'Text': C08},
    {'Id': 'PBT2C-C09', 'Text': C09},
    {'Id': 'PBT2C-C10', 'Text': T2A_STOP['PBT2A-C10']},
    {'Id': 'PBT2C-C11', 'Text': C11},
    {'Id': 'PBT2C-C12', 'Text': C12},
    {'Id': 'PBT2C-C13', 'Text': C13_PKG},
    {'Id': 'PBT2C-C15', 'Text': C15_PKG},
    {'Id': 'PBT2C-C17', 'Text': C17},
    {'Id': 'PBT2C-C18', 'Text': C18_RR},
]
assert [s['Id'] for s in STOPS[17:]] == ['PBT2C-C%02d' % n for n in (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15, 17, 18)] and len(STOPS) == 33
for s in STOPS[17:]:
    assert not re.search(r'PBT2A-|PBT2B-|PBT1C-|In PB-T1c', s['Text']), s['Id']
    assert all(ord(ch) < 0x80 for ch in s['Text']), s['Id']


# ---------------------------------------------------------------- la tabla de CT-PBT2C-03 que trae la remedicion (section 6.0, anclajes de PB-T2c)
# ---- ct3_table_check (inicio)
def ct3_table_check(text, names):
    """La tabla custodiada de lo esperado de cada fila de CT-PBT2C-03 (v2.4, V24-14): una linea por gesto, en el orden de CT-PBT1C-02,
    '<gesto> aplicado=<b> igual-a-la-ventana=<b> diseno-igual=<b> descartes=<n>', con una de las tres colas de la regla; la de "Aplicar" se niega
    lleva debajo una linea '  porque: <motivo>'. Devuelve los errores (vacia si vale)."""
    allowed = {('True', 'True', 'True', '0'): 'aplica', ('False', 'True', 'True', '0'): 'nada', ('False', 'False', 'False', '1'): 'se niega'}
    rows, errors = [], []
    lines = text.replace('\r\n', '\n').split('\n')
    pat = re.compile(r'^(\S+) aplicado=(True|False) igual-a-la-ventana=(True|False) diseno-igual=(True|False) descartes=(0|1)$')
    for k, l in enumerate(lines):
        m = pat.match(l)
        if not m:
            continue
        tail = m.groups()[1:]
        if tail not in allowed:
            errors.append('fila %s: la cola %s no es ninguna de las tres de la regla' % (m.group(1), ' '.join(tail)))
        elif allowed[tail] == 'se niega' and not (k + 1 < len(lines) and re.match(r'^  porque: \S', lines[k + 1])):
            errors.append('fila %s: "Aplicar" se niega y la remedicion no dice el porque' % m.group(1))
        rows.append(m.group(1))
    if rows != list(names):
        errors.append('las filas no son los %d gestos de CT-PBT1C-02 en su orden (%d filas; la primera distinta: %s)'
                      % (len(names), len(rows), next((r for r, n in zip(rows, names) if r != n), '-')))
    return errors
# ---- ct3_table_check (fin)


# ---------------------------------------------------------------- autoridades
D2_ = PILOT + '/F4B-DRAFTS/pbt2/'
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
PT1 = 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md'
PV5 = 'docs/initiatives/I-64-proposal-v5.md'
T1BPKG = 'docs/initiatives/I-64-f4-t1b-gate-contract-package.md'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
F4B_E25 = CO + 'owner-f4b-correction-unit-change-decision.md'
BOUNDED_CHECK = D2_ + 'independent-review-pbt2c-v25-check.md'
ANCHORS_T2C = D2_ + 'pbt2c-anchors-out.txt'
CA_T2B = D2_ + 'independent-review-pbt2b-v23-check.md'
assert title_version(lines_of(PT1)) == 'v2.8'
_ph, _pv = lines_of(PHASE), lines_of(PV5)
assert _ph[476].startswith('## 4. Los dos modos') and _ph[503].startswith('| **PB-T2 ') and _pv[556].startswith('### D-15 ') and 'Sin cambio observable' in _pv[585]
_close_txt = (' (' + PT2B_ENTRY + ')') if PT2B_ENTRY else ' (por rellenar)'
CUSTODY = [  # las que se custodian con la emision de PB-T2c y son autoridades
    (BOUNDED_CHECK, 'la comprobacion acotada de PB-T2c (section 6.0 del paquete: sobre el paquete, la section 6.3 y la salida de sus anclajes): '
                    + (BOUNDED_RESULT or 'resultado por rellenar')),
    (ANCHORS_T2C, 'la salida de los anclajes de PB-T2c (section 6.0): los textos de los tres mutantes [T2b] sobre BaseSha y la tabla de los 49 gestos de '
                  'CT-PBT1C-02 con lo esperado de cada fila de CT-PBT2C-03, segun la regla de tres casos del section 5.4 (v2.4, V24-14)'),
]
CUSTODY_EXTRA = [D2_ + n for n in ('v29-delta.md', 'independent-review-pbt2c-v25-check-prompt.md', 'make_pbt2c_contract-dry.txt')]
FROM_T2B = [D2_ + n for n in ('v26-delta.md', 'v27-delta.md', 'v28-delta.md')]  # los custodia la emision de PB-T2b (v2.7, V27-07; v2.8, V28-06)
EXISTING = [
    (PKG, 'archivo completo (' + PKG_VERSION + '): section 1 (autoridades), section 5.0 (el regimen, las recetas, el momento de cada lectura y los gestos), '
          'section 5.4 (el regimen de PB-T2c, CT-PBT2C-01 a 03 y la tabla cerrada de mutantes MU-T2C-01 a 03), section 5.5 (el censo), section 6.0 y 6.3 '
          '(este contrato), section 8 (las decisiones del Owner) y section 10 (arrastres); los registros de cambios v2.8 -> v2.9 (V29-01 a V29-06: las secciones 11 y 12 de CA-T2c y la Snapshot K de F4B-E37), v2.7 -> v2.8 (V28-01 a V28-06: las recomprobaciones de la v2.7, la verificacion con una reparacion), v2.6 -> v2.7 (V27-01 a V27-07: F4B-E35, la reparacion del RED), v2.5 -> v2.6 (V26-01 a V26-08: CA-T2c y su section 9), v2.4 -> v2.5 (V25-01 a V25-04), '
          'v2.3 -> v2.4 (V24-14: CT-PBT2C-01 y '
          '03) y anteriores'),
    (MEAS, 'el registro de mediciones del paquete, apartados A a Q'),
    (D2_ + 'v2-delta.md', 'de la v1 a la v2 del paquete, hallazgo por hallazgo (IR1)'),
    (D2_ + 'independent-review-v1.md', 'la revision independiente de la v1 (IR1: NECESITA v2, 0/8/16) y su recomprobacion de la v2 (IR1-R, section 10: LISTO '
                                       'PARA EMITIR TRAS REMEDICION, 0/0/7), aplicada en la v2.1'),
    (D2_ + 'independent-review-pbt2a-v21-check.md', 'la comprobacion acotada de PB-T2a (CA-T2a: EMITIBLE TRAS CORRECCIONES 0/0/6, aplicadas)'),
    (CA_T2B, 'la comprobacion acotada de PB-T2b (CA-T2b: EMITIBLE TRAS CORRECCIONES, B 0 / I 3 / M 7, corregidas en la v2.4), con la revision de las CT de '
             'PB-T2c que pidio la sesion (V24-14)'),
    (PBT2AC, 'F4B-E32, el contrato emitido de PB-T2a: sus listas de tests/, la raiz, las paradas estandar, C08, C11, C12, C17 y las celdas'),
    (PB2A_TCP, 'el cierre de PB-T2a: el puerto IPushBackDraftStatePort y el lanzamiento PushBackDraftEditorLaunch que usa la ventana de WORKSPACE'),
    (PT2A_STOP_ANALYSIS, PT2A_STOP_ENTRY + ', la parada del intento 1 de PB-T2a (PBT2A-C07): C-1, el momento de una lectura, que la v2.4 aplica a las '
                         'recetas (section 5.0, El momento de cada lectura; aplicado de PB-T2c)'),
    (PBT2BC, 'el contrato emitido de PB-T2b: la ventana en dos modos, sus seis clases de prueba (que PB-T2c no edita) y PushBackWorkspaceSupport'),
    (PB2B_TCP, 'el cierre de PB-T2b' + _close_txt + ': W con el modo WORKSPACE, el texto sobre el que se aplican los tres mutantes; ' + S1['CLOSES'][1]),
    (PHASE, '[P], v3.1: ' + S1['P'][1]),
    (PT1, '[PT1], v2.8 (con el que se emitio PB-T1c): ' + S1['PT1'][1]),
    (CO + 'owner-f4b-disposition.md', '[OD]: ' + S1['OD'][1].replace('(literal arriba)', '(literal: ' + OD_ROW + ')')),
    (CO + 'owner-f4b-procedure-decisions.md', '[DP], F4B-E03: ' + S1['DP'][1]),
    (CO + 'owner-pbt1-pre-decisions.md', 'F4B-E09 (con F4B-E06, en f4b-dispositions.md): ' + S1['E06-09'][1] + ': CT-PBT2C-01 y 02'),
    (CO + 'owner-d1-d2-decisions.md', '[OD2], F4B-E11: ' + S1['OD2'][1]),
    (PILOT + '/F4B-DRAFTS/pbt1/owner-return-l1.md', 'la vuelta L-1 al Owner, respondida por F4B-E11 (D2)'),
    (SNAP_K_DEC, 'F4B-E37: ' + S1['E37-22'][1].split('; ')[0]),  # la autoridad de P-01 es la decision de la K (v2.9, V29-06)
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-config.md', 'la vuelta al Owner por el config.toml tras el reinicio, respondida por F4B-E19 (la Snapshot J)'),
    (CO + 'owner-f4b-opaque-collections-decision.md', 'F4B-E21: ' + S1['E37-22'][1].split('; ')[1]),
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-planning2.md', 'la vuelta al Owner por la salida truncada del Controller, respondida por F4B-E21'),
    (CO + 'owner-pbt1b-rework-decisions.md', 'F4B-E22: ' + S1['E37-22'][1].split('; ')[2]),
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-rework.md', 'la vuelta al Owner tras la revision independiente del intento 1 de PB-T1b (M10 y M14), respondida por F4B-E22'),
    (CO + 'owner-f4b-dpb07-extensiondata-decisions.md', 'pregunta 1 (F4B-E23): ' + S1['E23'][1] + '; pregunta 2 (F4B-E24): ' + S1['E24'][1]),
    (PILOT + '/F4B-DRAFTS/pbfull/owner-return-dpb07.md', 'la vuelta al Owner por D-PB-07, respondida por F4B-E23'),
    (PILOT + '/F4B-DRAFTS/pbfull/owner-return-extensiondata.md', 'la vuelta al Owner por la ExtensionData anidada, respondida por F4B-E24'),
    (F4B_E25, 'F4B-E25: ' + S1['E25'][1] + '. Las tres condiciones las comprueba el Controller, y no hace falta reemitir el contrato para corregir '
              '(section 6.0, Las correcciones). W y WC son UNIT_CHANGE en este contrato (section 6.0)'),
    (F4B_E35, F4B_E35_ENTRY + ', pregunta 4: ' + RR_S1[1] + '. El contrato la incorpora en PBT2C-C07 (la excepcion), PBT2C-C18, INV-PBT2C-02 (la cadena '
              'RR-6, con las corridas aisladas de INV-PBT2C-03) y la ExpectedEvidence (la CI del RED reparado y lo que declara la entrega) (section 6.0 '
              'del paquete, v2.7, V27-01 a V27-06). ' + RR_VERIF_AUTH),
    (PILOT + '/F4B-DRAFTS/pbt1b/owner-return-verify-stop.md', 'la vuelta al Owner por la parada de la primera verificacion del intento 2 de PB-T1b (16.3), respondida por F4B-E25'),
    (OWNER_PBT2, 'archivo completo (' + OWNER_PBT2_ENTRY + '): Q-PBT2-1 a Q-PBT2-4, las cuatro "(Recomendado)"; la pregunta 4, "Si, con esa condicion", la '
                 'autorizacion condicional de emision de PB-T2a, PB-T2b y PB-T2c'),
    (DISP, 'archivo completo: F4B-E01 a ' + DISP_LAST),
    (PILOT + '/F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md', 'el cierre de PB-T1b, F4B-E26: ' + S1['E26'][1] + '; ' + lc(S1['CLOSES'][1])),
    (PB1C_TCP, 'el cierre de PB-T1c (' + PT1C_CLOSE_ENTRY + ', sobre 161a0dad): ' + S1['CLOSES'][1] + '; CT-PBT1C-02, cuyos gestos copia CT-PBT2C-03'),
    (PBT1CC, 'F4B-E27, el contrato emitido de PB-T1c: ' + S1['E27'][1]),
    (PV5, 'la Proposal V5: D-15 (:557-582): ' + S1['PV5D15'][1] + '; D-16 e INV-19 (:586-593): ' + S1['PV5D16'][1]),
    (T1BPKG, 'T1B: ' + S1['T1B'][1]),
    (PILOT + '/F4B-PBTC-DECLARED-CORRECTIONS/task-close-package.md', 'el cierre de PB-TC: ' + S1['CLOSES'][1] + '; CT-PBTC-02, 05 y 07, los oraculos de '
                                                                    'CT-PBT2C-01 y 02'),
    (PILOT + '/F4B-PBTC2-DECLARED-CORRECTIONS/task-close-package.md', 'el cierre de PB-TC2: ' + S1['CLOSES'][1]),
    (PILOT + '/F4B-PBT1A-LOSSLESS-STATE-NET/task-close-package.md', 'el cierre de PB-T1a: ' + S1['CLOSES'][1] + '; el regimen de mutantes que copia PB-T2c'),
]
UNIT_CHANGE = [
    (W, 'la ventana de Push Back: solo las sustituciones de una linea MU-T2C-01 (Keep) y MU-T2C-03 (ApplyToDraft) en el RED, revertidas por el GREEN con '
        'git restore --source=BaseSha (section 5.4 y 6.0). En una correccion tras un GREEN, su presencia en git diff --name-only AuthorityRevision BaseSha '
        'se lee con la regla de F4B-E25'),
    (WC, 'la parte compuesta de la ventana: solo la sustitucion de una linea MU-T2C-02 (UpdateSideBEmptyNotice) en el RED, revertida por el GREEN con '
         'git restore --source=BaseSha (section 5.4 y 6.0). En una correccion tras un GREEN, se lee con la regla de F4B-E25'),
]
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D7, D8, D12 y D13: ' + S1['ADR'][1]),
    ('docs/AUTOMATION_PLAN.md', 'section 16 (16.3 a 16.11; 16.3 en una correccion, con F4B-E25): ' + S1['PROTO'][1]),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4): ' + S1['PROTO'][1]),
    ('docs/automation/agent-execution/routing.md', 'archivo completo: ' + S1['PROTO'][1]),
    ('docs/automation/agent-execution/model-catalog.md', 'las celdas medidas y vigentes (:14, :51 y :97: vencen el ' + CELLS_EXPIRY + '): ' + S1['PROTO'][1]),
]
SOFT_AUTH = {PB2A_TCP: 'el cierre de PB-T2a', PBT2BC: 'el contrato emitido de PB-T2b', PB2B_TCP: 'el cierre de PB-T2b',
             F4B_E35: 'F4B-E35, la regla RR (de la custodia del cierre de PB-T2a)',
             CA_T2B: 'la comprobacion acotada de PB-T2b (de su emision)',
             SNAP_K_DEC: 'la decision de la Snapshot K, F4B-E37 (de la custodia del cierre de PB-T2b)'}
assert SNAP_K_DEC == CO + 'owner-config-snapshot-k-decision.md'
assert len(CUSTODY) == 2 and len(CUSTODY_EXTRA) == 3 and len(FROM_T2B) == 3 and len(EXISTING) == 40 and len(UNIT_CHANGE) == 2 and len(EXTERNAL) == 5
assert len({p for p, _ in CUSTODY + EXISTING + UNIT_CHANGE + EXTERNAL}) == len(CUSTODY + EXISTING + UNIT_CHANGE + EXTERNAL)
for p, _ in EXISTING + EXTERNAL + UNIT_CHANGE:
    if p not in SOFT_AUTH and p != PKG:
        assert in_head(p), ('autoridad inexistente en la revision', p)
for p, what in SOFT_AUTH.items():
    if p not in (PBT2BC, SNAP_K_DEC):  # el contrato de PB-T2b y la Snapshot K ya avisan arriba
        soft(in_head(p), '%s (autoridad) aun no esta en la revision: %s' % (what, p))
if in_head(F4B_E35):  # la regla RR que el contrato incorpora (v2.7, V27-06)
    _err = rr_check(show(F4B_E35))
    assert not _err, ('F4B-E35: la regla RR', _err)
for p in FROM_T2B:
    soft(in_head(p), 'de la emision de PB-T2b (v2.7, V27-07), aun no en la revision: %s' % p)
_close = (' (' + PT2B_ENTRY + ')') if PT2B_ENTRY else ' (<cierre de PB-T2b por rellenar>)'
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
                 'sin cambio material) (docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt2-decisions.md): la emision de la tercera '
                 'tarea de PB-T2 tras el cierre de PB-T2b' + _close + ', sus anclajes (los textos de los tres mutantes y la tabla de CT-PBT2C-03) y su '
                 'comprobacion acotada; F4B-E06 y F4B-E09, las obligaciones de PB-T2 (el paso 4 de D-PB-06 y el aviso de D-PB-08 en WORKSPACE); F4B-E11, '
                 'D2 (owner-d1-d2-decisions.md); las decisiones de procedimiento owner-f4b-procedure-decisions.md (F4B-E03); F4B-E37, la Snapshot K, que sustituye a la J de F4B-E19; '
                 'F4B-E21, las colecciones opacas que pone la sesion; F4B-E25, la regla de F4B sobre las rutas UNIT_CHANGE en una correccion; y el paquete '
                 'del contrato de PB-T2 ' + PKG_VERSION + ', con IR1, IR1-R y las comprobaciones acotadas de PB-T2a, PB-T2b y PB-T2c; y F4B-E35, la regla de '
                 'la reparacion del RED (RR-1 a RR-7), que el contrato incorpora en PBT2C-C07, PBT2C-C18, INV-PBT2C-02 y la ExpectedEvidence'),
    'IssuedUtc': ISSUED,
}
assert len(c["Authorities"]) == 50, len(c['Authorities'])
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
# lo que depende de la emision de PB-T2c
soft(PKG_HEAD_VERSION == PKG_VERSION, 'el paquete custodiado en la revision es %s; al escribir hace falta la %s en HEAD (%s)' % (PKG_HEAD_VERSION, PKG_VERSION, PKG))
soft(BOUNDED_RESULT, 'falta BOUNDED_RESULT (el resultado de la comprobacion acotada de PB-T2c, section 6.0)')
assert BOUNDED_RESULT is None or (BOUNDED_RESULT.startswith('EMITIBLE') and all(ord(ch) < 0x80 for ch in BOUNDED_RESULT)), BOUNDED_RESULT
soft(in_head(BOUNDED_CHECK), 'a custodiar con la emision de PB-T2c, aun no en la revision: %s' % BOUNDED_CHECK)
CT3_OK = soft(in_head(ANCHORS_T2C), 'a custodiar con la emision de PB-T2c, aun no en la revision: %s (la tabla de lo esperado de CT-PBT2C-03)' % ANCHORS_T2C)
if CT3_OK:
    _err = ct3_table_check(show(ANCHORS_T2C), GESTURES)
    assert not _err, ('la tabla de CT-PBT2C-03', _err)
for p in CUSTODY_EXTRA:
    soft(in_head(p), 'a custodiar con la emision, sin ser autoridad, aun no en la revision: %s' % p)


# ---- bounded_check (inicio)
def bounded_check(report, result, pkg_lines, reviewer='CA-T2c'):
    """La comprobacion acotada custodiada frente a BOUNDED_RESULT y al paquete (F4B-E28, pregunta 4: 0 bloqueantes y 0 importantes SIN RESOLVER).
    Lee el primer veredicto y, si las hay, las recomprobaciones (v2.5, V25-01; CA-T2b seccion 10, M-08): cada una con su encabezado
    '## <seccion>n. Recomprobacion sobre la vX.Y' y su linea '**Veredicto de <seccion>n: ... Nuevos: B n / I n / M n.**'; la ultima tiene que dar
    EMITIBLE con 0 bloqueantes y 0 importantes nuevos, y BOUNDED_RESULT la nombra ('recomprobacion vX.Y: <veredicto>, B n / I n / M n').
    La n de la seccion puede ser un numero o una letra, como R (v2.6, CA-T2c M-07). Un encabezado '## ...Recomprobacion...' o una linea que empieza
    por '**Veredicto de <seccion>' sin su forma, o que no se empareja en orden con el otro, da un error claro en lugar de una excepcion.
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
    assert not _err, ('la comprobacion acotada de PB-T2c', _err)
_last = re.split(r'(?m)^## ', _disp)[-1]
EMIT_ENTRY_OK = bool(re.match(r'F4B-E\d+\. Emisi(?:o|' + chr(0xF3) + r')n de F4B-PBT2C-WORKSPACE-CARRYOVER', _last)) and OWNER_PBT2_ENTRY in _last and 'pregunta 4' in _last
soft(EMIT_ENTRY_OK, 'la ultima entrada de f4b-dispositions.md (%s) no es la emision de F4B-PBT2C-WORKSPACE-CARRYOVER con las condiciones de %s, pregunta 4'
     % (DISP_LAST, OWNER_PBT2_ENTRY))
ORIGIN_MAIN = git('rev-parse', 'origin/main').stdout.decode('ascii').strip()
soft(ORIGIN_MAIN == MAIN, 'MainSha (%s) no es origin/main (%s)' % (MAIN[:8], ORIGIN_MAIN[:8]))
if not DRY:
    assert git('merge-base', '--is-ancestor', AR, REV).returncode == 0, 'la AuthorityRevision no es la revision ni un antepasado suyo'
REMEDIATION = [
    '1 el cierre de PB-T2b: ' + (ANCHOR_T2B if PT2B_CLOSE else 'pendiente (PT2B_CLOSE)') + ', posterior a la parada de PB-T2a ' + PT2A_STOP_ENTRY
    + ' y con el cierre de PB-T2a delante; el contrato de PB-T2b: ' + ('presente' if PBT2B_OK else 'pendiente'),
    '2 los textos de los tres mutantes [T2b]: ' + '; '.join(MUT_REPORT),
    '3 la tabla de CT-PBT2C-03: %d gestos (y %d de D2 fuera); lo esperado de cada fila: %s' % (len(GESTURES), len(D2_GESTURES),
                                                                                             'custodiado y comprobado' if CT3_OK else 'pendiente (la remedicion)'),
    '4 la linea base de la CI: ' + ('atada al cierre de PB-T2b' if CI_BASE_T2B else 'pendiente (CI_BASE_T2B; que sea la corrida push 4/4 lo comprueba la sesion)'),
    '5 el producto: src desde 161a0dad solo en PB-T2a y PB-T2b; WC y WX == 161a0dad; recuentos de las familias == 17872be8: ' + ('OK' if not COUNTS_CHANGED else 'cambian'),
    '6 la custodia y la entrada de emision: %d AVISOS' % sum(1 for w in WARN if 'custodi' in w or 'entrada de f4b' in w or 'paquete custodiado' in w),
    '7 el entorno: MainSha == origin/main: %s; celdas hasta %s; Test-Json al escribir; config.toml = Snapshot K (F4B-E37), el binario y una sola delegacion: la sesion'
    % (ORIGIN_MAIN == MAIN, CELLS_EXPIRY),
    '8 la autorizacion (F4B-E28, pregunta 4): BOUNDED_RESULT = %s; la remedicion sin cambio material y PB-T2b sin intento 3: la sesion' % BOUNDED_RESULT,
]
if DRY:
    print('DRY rev', REV_SHA[:8], '(objetos del checkout principal)' if OBJ_REV else '(HEAD del worktree)', 'allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN),
          '(src', len(SRCF), 'tests', len(TESTF), '[core', len(CORE_T), 'ui', str(len(UI_T)) + '] root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'required', len(REQ), 'evidence', len(EVID), 'authorities', len(c['Authorities']), 'test-anchors', len(T_ANCHORS),
          'bytes', len(b), 'sha256', hashlib.sha256(b).hexdigest().upper())
    print('anclaje:', ANCHOR_SRC, '|', ANCHOR_T2B, '| f4b-dispositions hasta', DISP_LAST, '| F4B-E06, E09, E23 y E28 custodiadas')
    print('paquete leido:', PKG_SRC, '->', PKG_READ_VERSION, '| en la revision:', PKG_HEAD_VERSION)
    print('textos de la %s comprobados:' % PKG_VERSION, ', '.join(PKG_CHECKS) if PKG_CHECKS else 'ninguno')
    print('mutantes [T2b] (testigos ASCII):')
    for l in MUT_REPORT:
        print('  ' + l)
    print('CT-PBT2C-01: el gesto neutro DrawRackNameCheck escucha Click y lo lee ReadInputs (W:%d-%d); las once filas F de SC1; pb-08-redondeo, '
          'STANDARD-ISO y SIMPLEX | CT-PBT2C-02: las doce lineas de CT-PBTC-07' % READINPUTS)
    print('CT-PBT2C-03: las filas que tiene que traer la tabla de la remedicion (%d, en este orden; fuera, los de D2: %s):' % (len(GESTURES), ', '.join(D2_GESTURES)))
    for k in range(0, len(GESTURES), 6):
        print('  ' + ' | '.join(GESTURES[k:k + 6]))
    print('V23-01 (CA-T2a M-6 (a)): clases de los filtros de UI que crean un RackFrameConfiguratorWindow real:', ', '.join(CONFIGURATOR_FILES))
    print('D1: needs de build-plugin =', _needs, '| D2: Snapshot J:', in_head(SNAP_J), '| la Snapshot K (F4B-E37) y su decision:', SNAPK_OK, '| celdas hasta', CELLS_EXPIRY)
    print('remedicion al escribir (section 6.0, anclajes de PB-T2c):')
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
assert PT2B_CLOSE and PT2B_ENTRY and CI_BASE_T2B and BOUNDED_RESULT, 'faltan PT2B_CLOSE, CI_BASE_T2B o BOUNDED_RESULT'
assert not WARN, WARN
print('remedicion (section 6.0):\n  ' + '\n  '.join(REMEDIATION))
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
