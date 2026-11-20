"""Contrato de gate de F4B-PBT3B-FULL-WRITE-RUN (ASCII), desde el paquete del contrato de PB-T3 v4.5 (docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md:
section 3, D-PBT3-09 y D-PBT3-10; section 4 y 4.1; section 5.0 y 5.3; section 6.0 y 6.2), con el patron de make_pbt3a_contract.py. La variante es
(A) (F4B-E29, D-1): nada de la (A') no elegida va al contrato. La base es el cierre de PB-T3a (PT3A_CLOSE, F4B-E43): el src de la BaseSha es el
del cierre (section 6.0, "en PB-T3b y PB-T3c, frente al cierre de la subtarea anterior de PB-T3 (vacio)").
El ForbiddenWriteScope de src/ sale del algoritmo de [PKC-C] (cover()) con el AllowedWriteScope de 11 rutas (PP, UR, la interfaz nueva, las cuatro
clases, el doble con borrado y los tres archivos de guardas de D-1); el de tests/ es el del contrato emitido de PB-T3a mas sus seis clases, con el
prefijo tests/RackCad.Tests/Workspace/ sustituido por la lista explicita de sus archivos MENOS los tres de guardas de D-1 (section 6.0: 76 de 79).
Comprueba sobre la revision leida (HEAD del worktree o, con --objects-rev, esa revision en el almacen de objetos del checkout principal):
- el anclaje (precondiciones 1 y 5; section 6.0): la revision desciende del cierre de PB-T3a, con su entrada (F4B-E43, CLOSED / PASS WITH
  LIMITATIONS), su task-close-package y su contrato emitido; src y tests son los del cierre; el GREEN de PB-T3a (6919e17a) tiene el src y los tests
  del cierre; los ocho archivos de Draft/ estan;
- las anclas de PP, UR, UP, PM, WHC y de las guardas que cita el diseno (D-PBT3-09, D-PBT3-10 y la tabla de S-1 a S-6), con su linea y su texto;
  los seis metodos de D-1, con su atributo; la lista explicita de tests/RackCad.Tests/Workspace/; los nombres nuevos de PBT3B-C13, libres en
  Pilot/, y los archivos nuevos, libres; la declaracion de los miembros de Execution que cambian de firma (D-PBT3-10; R-18);
- el texto del paquete (solo si es la v4.5 revisada, por su sha256): el Objective, el AllowedWriteScope, el esqueleto del RED, el Skip de D-1, la
  tabla de RequiredTests, los invariantes propios (01 a 06 y 15) y comunes (07 a 14), las paradas propias (C04, C13, C14, C16, C17) y comunes (C07 con
  RR y C18), la ExpectedEvidence, la linea base de la CI y el motivo literal del Skip de 5.3, literales, SIN sus "Nota (no va al contrato)" y sin
  texto de la variante (A'); la regla RR del section 6.0 con su lectura para PB-T3b (RR-2: una reparacion no toca los tres archivos de guardas de
  F4 con los Skip) y la tabla de RR-3 en sus filas de PB-T3b (rr3_table_check: lo que se enumera y lo que se deriva de 5.3); el bloque citado de
  PBT3x-C11; las clases y la lista CT-PBT3B-01 a 15 de 5.3; P-01 (la K), las celdas, la CI del RED, UNIT_CHANGE (PP y UR);
- PBT3B-C11: el bloque citado del paquete con el prefijo de PB-T3b y, por las decisiones del Owner F4B-E40 (la firma PD-F4B-01 y su sustituto de nc1)
  y F4B-E43 (el sustituto de nc2 ampliado, la v2), las salidas de nc1 y de nc2 cambiadas (C11_SUBS: cada cambio, una vez y comprobado); el
  sustituto de nc1 con su SHA-256 en la revision; el de nc2 v2 (NC2_V2_SHA), un marcador hasta que se custodie revisado;
- los MinSelected, con el criterio de count_tests_git.py sobre los objetos de la revision (los nuevos, ausentes; los heredados del contrato de PB-T3a,
  medidos y sin bajar; Workspace de Core, el medido menos las diez filas de D-1, que el RED salta);
- D1 (build-plugin con needs [tests, build-ui]); la Snapshot K (snapk_check); F4B-E35 (rr_check y q3_check); F4B-E30 (ninguna clase de los filtros
  de UI crea un RackFrameConfiguratorWindow real);
- la comprobacion acotada de PB-T3b (CA-T3b) frente a BOUNDED_RESULT (bounded_check, estricto), cuando exista.
Lo que depende de la emision de PB-T3b (BOUNDED_RESULT y su comprobacion acotada, NC2_V2_SHA, las rutas de CUSTODY y CUSTODY_EXTRA, F4B-E44) es AVISO
en seco y error al escribir; todo lo demas es error siempre.
Uso: python make_pbt3b_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print] [--print-json] [--json-out <archivo>]
                                   [--objects-rev <rev> [--repo <checkout principal>] [--package-file <paquete>] [--bounded-file <informe de CA-T3b>]]
                                   [--as-write]
Con --dry no escribe nada en el repositorio: comprueba e imprime los recuentos, los bytes y los AVISOS (con --print, ademas, el
ForbiddenWriteScope numerado; con --print-json, el contrato; con --json-out, el contrato a ese archivo, fuera del repositorio). --objects-rev
exige --dry: no hace chdir al worktree y lee todo con git -C <checkout principal> show <rev>:<ruta>. --package-file y --bounded-file exigen
--objects-rev y --dry. --as-write (solo con --dry) trata los AVISOS como errores, como al escribir. Sin --objects-rev hace chdir al worktree y lee
HEAD; al escribir, la sesion lo ejecuta alli tras el commit X de la emision de PB-T3b.
Al custodiarlo, las rutas locales se redactan como en PB-T0 ('<worktree-de-I-64>', '<checkout-principal>')."""
import json, os, re, sys, subprocess, hashlib, unicodedata

# ---------------------------------------------------------------- la base: el cierre de PB-T3a (F4B-E43)
PT3A_CLOSE = 'c40eea4ad126ec6209dfef40e643e180b364bc97'  # la custodia del cierre de PB-T3a (F4B-E43), hija del GREEN; HEAD al emitir PB-T3b
PT3A_CLOSE_ENTRY = 'F4B-E43'
GREEN_T3A = '6919e17af9fdd5c702d454a97a3c5b6cc6be1739'   # el GREEN de PB-T3a (intento 2), sin reparacion del RED
RED_T3A = '5dbc9661438112c7be283a79ff5d980e78607067'     # el RED de PB-T3a (intento 2)
CI_BASE_T3A = ('44668584248', '6919e17a', 19107, 2451, 21)  # la CI del GREEN de PB-T3a en su cierre: run, GREEN, Core, UI, Skipped de UI
CI_FORECAST_T3A = (19107, 2451, 21)  # lo previsto del GREEN de PB-T3a (section 6.1): la base de "en cadena" de PB-T3b (V42-01)
PT3A_REISSUE = '485b5651bcec132761347b40ed38b60cfec0db04'  # la reemision de PB-T3a con la v4.5 (BASE), X 5bda5569 (F4B-E42)
PT3A_REISSUE_X = '5bda5569330d75fb2f03ae809ada1a47676139da'
PT3A_EMIT = '18e4b78302d9010555e5c7b7a6c6f7384622c0e6'   # la emision de PB-T3a con la v4.4 (F4B-E41)
PT3A_EMIT_ENTRY, PT3A_STOP_ENTRY = 'F4B-E41', 'F4B-E42'
PT3A_TASK = 'F4B-PBT3A-FULL-WRITE-MODEL'
D1_REV = 'd8ccbcb870f4d1804d7e7afd3855ac64b44693a8'      # F4B-E29 (D-1, la variante (A)) en git desde aqui (precondicion 4)
D1_ENTRY = 'F4B-E29'
# ---------------------------------------------------------------- lo que depende de la emision de PB-T3b
EMIT_ENTRY = 'F4B-E44'      # la numeracion prevista (la sesion): F4B-E44, evidencia 269 (X) y 270 (BASE)
X_EV, BASE_EV = '269', '270'
# la autorizacion de emision del Owner (precondicion 10; V43-07): F4B-E35, pregunta 3, que cubre PB-T3a, PB-T3b y PB-T3c en orden
ISSUE_AUTH = ('F4B-E35', 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md')
# el veredicto literal de la comprobacion acotada de PB-T3b (CA-T3b, independent-review-pbt3b-check.md): su primera ronda y su seccion 12, la
# recomprobacion sobre el seco r2 (precondiciones 3 y 10); bounded_check lo compara con el informe
BOUNDED_RESULT = ('EMITIBLE; B 0 / I 0 / M 4; sin importantes; sin decisiones nuevas del Owner; recomprobacion v4.5: EMITIBLE, B 0 / I 0 / M 1')
PKG_VERSION = 'v4.5'       # la version del paquete con la que se emite PB-T3b (custodiada desde 5bda5569)
CUSTODY_SHA256 = {         # el paquete revisado, byte a byte: la v4.5 final (282 234 bytes, LF), ya custodiada
    'docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md': '64ab03df27cc8f441883442ca999036e5f7555ede9269eb3f4f768ab8a07fdbd',
}
# la Snapshot K (F4B-E37; V43-08): P-01 y su autoridad
SNAP_K = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-K/snapshot-k.json'
SNAP_K_DEC = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-config-snapshot-k-decision.md'
SNAP_K_SHA = 'E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE'
SNAP_K_PROBE = 'R20261119T154313Z-2a9a'
# PBT3B-C11 (F4B-E40 y F4B-E43): los sustitutos de nc1 y nc2 que nombra el contrato en lugar de los del bloque citado del paquete
SUBST_F4B = ('docs/automation/evidence/I-64-pilot/F4B-PBT2A-DRAFT-STATE-PORT-nc2-substitute/2/R20261119T125505Z-c2d3/'
             'nc2_exact_whitelist_substitute_f4b.py')  # el v1 (F4B-E34), el que nombra el paquete
SUBST_F4B_SHA = '6ccaf12dbf9122c0bddc5b724bcff573705c95698af38bf750bf69459ee2ffd3'
NC1_SUBST = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc1-cascade-substitute/nc1_cascade_signal_substitute.py'  # PD-F4B-01 (F4B-E40)
NC1_SUBST_SHA = '41440edc4a963bef890e4336dccb885837424008c027cdb172d2b926e0f49b40'  # la version revisada (APTO; limits-declared.md)
# el sustituto de nc2 ampliado (F4B-E43), la v2: su carpeta de custodia (aceptada por la sesion) y el SHA-256 de la version revisada (APTO, B 0 / I 0 /
# M 4, en la seccion "Recomprobacion v2" de su revision; limites M-V1 a M-V4), que custodia el commit X con su revision y sus limites declarados
NC2_DIR = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/nc2-substitute-f4b-v2/'
NC2_V2 = NC2_DIR + 'nc2_exact_whitelist_substitute_f4b_v2.py'
NC2_V2_SHA = 'c033e96ce147963feb9c04733e9bfbec7493fea33d8ff79b1c2e8ab860798e45'
NC2_V2_REVIEW = NC2_DIR + 'review/independent-review-substitute-f4b-v2.md'
NC2_V2_LIMITS = NC2_DIR + 'limits-declared.md'
CUSTODY_HEADS = {          # el registro gana la remedicion de PB-T3b sobre el cierre de PB-T3a (R-21)
    'docs/initiatives/I-64-f4b-pbt3-measurements.md': ['## R-0. ', '## R-6. ', '## R-15. ', '## R-18. ', '## R-19. ', '## R-20. ', '## R-21. '],
}
MAIN_CHECKOUT = '<checkout-principal>'  # Redaccion para el repo publico: la ruta local del checkout principal  # solo con --objects-rev; se redacta al custodiar ('<checkout-principal>')
WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree  # solo al escribir; se redacta al custodiar

ARGS = sys.argv[1:]
VALUED = ('--objects-rev', '--repo', '--package-file', '--json-out', '--bounded-file')


def opt(name):
    if name not in ARGS:
        return None
    i = ARGS.index(name)
    assert i + 1 < len(ARGS) and not ARGS[i + 1].startswith('--'), ('falta el valor de', name)
    return ARGS[i + 1]


POS = [a for i, a in enumerate(ARGS) if not a.startswith('--') and not (i > 0 and ARGS[i - 1] in VALUED)]
assert len(POS) == 3, 'uso: make_pbt3b_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
DRY = '--dry' in ARGS
AS_WRITE = '--as-write' in ARGS
OBJ_REV = opt('--objects-rev')
assert OBJ_REV is None or DRY, '--objects-rev solo en seco: anade --dry'
assert not AS_WRITE or DRY, '--as-write solo en seco'
PKG_FILE = opt('--package-file')
assert PKG_FILE is None or (OBJ_REV is not None and DRY), '--package-file solo junto con --objects-rev y --dry'
PKG_FILE = os.path.abspath(PKG_FILE) if PKG_FILE else None  # antes de cualquier chdir
assert PKG_FILE is None or os.path.isfile(PKG_FILE), ('no existe el paquete', PKG_FILE)
BOUNDED_FILE = opt('--bounded-file')  # el informe de CA-T3b sin custodiar, en seco: bounded_check lo lee como leera el custodiado
assert BOUNDED_FILE is None or (OBJ_REV is not None and DRY), '--bounded-file solo junto con --objects-rev y --dry'
BOUNDED_FILE = os.path.abspath(BOUNDED_FILE) if BOUNDED_FILE else None
assert BOUNDED_FILE is None or os.path.isfile(BOUNDED_FILE), ('no existe el informe de la comprobacion acotada', BOUNDED_FILE)
JSON_OUT = opt('--json-out')
assert JSON_OUT is None or DRY, '--json-out solo en seco'
JSON_OUT = os.path.abspath(JSON_OUT) if JSON_OUT else None
for _root in (MAIN_CHECKOUT, WT):
    assert JSON_OUT is None or not os.path.normcase(JSON_OUT).startswith(os.path.normcase(os.path.abspath(_root))), '--json-out fuera del repositorio'
# las celdas vencen el 2026-12-29 (precondicion 6 y section 6.0 del paquete)
assert re.fullmatch(r'[0-9]{4}-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9]Z', ISSUED) and ISSUED[:10] < '2026-12-29', ISSUED
assert re.fullmatch(r'[0-9a-f]{40}', AR) and re.fullmatch(r'[0-9a-f]{40}', MAIN), (AR, MAIN)

if OBJ_REV is None:
    os.chdir(WT)
    REPO, REV = None, 'HEAD'
else:
    REPO, REV = opt('--repo') or MAIN_CHECKOUT, OBJ_REV
DISK = OBJ_REV is None  # solo en el worktree se mira tambien el disco

TASK = 'F4B-PBT3B-FULL-WRITE-RUN'
assert 24 <= len(TASK) <= 26, len(TASK)  # R-T1-07: MAX_PATH
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
D3 = PILOT + '/F4B-DRAFTS/pbt3/'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
F4B_E25 = CO + 'owner-f4b-correction-unit-change-decision.md'
F4B_E25_RET = PILOT + '/F4B-DRAFTS/pbt1b/owner-return-verify-stop.md'
PT3A_TCP = PILOT + '/' + PT3A_TASK + '/task-close-package.md'
PBT3AC = PILOT + '/' + PT3A_TASK + '/coordinator/gate-contract.json'
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
    """Lo que solo existira con la emision de PB-T3b (su comprobacion acotada, el sustituto de nc2 v2 revisado, la custodia del commit X):
    AVISO en seco, error al escribir (y con --as-write)."""
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
DRAFT = APP + 'Draft/'
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
WS = TR + 'Workspace/'
PP, UR, UP = PILOTD + 'PushBackPilotUpdatePlan.cs', PILOTD + 'PushBackPilotUpdateRun.cs', PILOTD + 'IPushBackPilotUpdatePort.cs'
ERASER = PILOTD + 'IPushBackPilotViewEraser.cs'
PM, IR = PILOTD + 'PushBackPilotMaterializer.cs', PILOTD + 'PushBackPilotIntentReader.cs'
WHC = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
RPC = 'src/RackCad.Plugin/RackPushBackCommands.cs'
PG, UG, IG = WS + 'WorkspacePilotPrepareGuardTests.cs', WS + 'WorkspacePilotUpdateGuardTests.cs', WS + 'WorkspacePilotUpdateIntegrationGuardTests.cs'
D1_GUARDS = [PG, UG, IG]  # los tres archivos de guardas de D-1 con la variante (A) (F4B-E29; V3-05)
DRAFT_NAMES = ['PushBackFullDraftState.cs', 'PushBackFullDraftStateComparer.cs', 'PushBackEditableStateSeat.cs', 'PushBackFullDraftMaterializer.cs',
               'PushBackFullDraftReader.cs', 'PushBackFullComposition.cs', 'PushBackViewDisposition.cs', 'PushBackConflictClassifier.cs']

# ---------------------------------------------------------------- el anclaje (precondiciones 1 y 5; section 6.0: frente al cierre de PB-T3a, vacio)
for _short, _full in (('c40eea4a', PT3A_CLOSE), ('6919e17a', GREEN_T3A), ('5dbc9661', RED_T3A), ('485b5651', PT3A_REISSUE), ('5bda5569', PT3A_REISSUE_X),
                      ('18e4b783', PT3A_EMIT), ('d8ccbcb8', D1_REV)):
    assert git('rev-parse', _short).stdout.decode('ascii').strip() == _full, ('SHA', _short)
for _anc, _name in ((D1_REV, 'F4B-E29 (D-1)'), (PT3A_EMIT, 'la emision de PB-T3a (F4B-E41)'), (PT3A_REISSUE, 'la reemision de PB-T3a (F4B-E42)'),
                    (GREEN_T3A, 'el GREEN de PB-T3a'), (PT3A_CLOSE, 'el cierre de PB-T3a (F4B-E43)')):
    assert git('merge-base', '--is-ancestor', _anc, REV).returncode == 0, ('la revision no desciende de ' + _name)
assert git('merge-base', '--is-ancestor', RED_T3A, GREEN_T3A).returncode == 0, 'el RED de PB-T3a no es antecesor de su GREEN'
assert git('diff', '--quiet', PT3A_CLOSE, REV, '--', 'src', 'tests').returncode == 0, \
    'src o tests cambiaron desde el cierre de PB-T3a (section 6.0: el diff de src frente al cierre de la subtarea anterior, vacio)'
assert git('diff', '--quiet', GREEN_T3A, PT3A_CLOSE, '--', 'src', 'tests').returncode == 0, 'el cierre de PB-T3a no tiene el src y los tests de su GREEN'
_disp_c = show(DISP, PT3A_CLOSE).replace('\r\n', '\n')
_heads = re.findall(r'(?m)^## (F4B-E\d+)\. Cierre de ' + PT3A_TASK + r'\b', _disp_c)
assert _heads == [PT3A_CLOSE_ENTRY], ('PT3A_CLOSE no es el cierre de PB-T3a: su f4b-dispositions.md', _heads)
assert re.split(r'(?m)^## ', _disp_c)[-1].startswith(PT3A_CLOSE_ENTRY + '. '), 'el cierre de PB-T3a no es la ultima entrada de su f4b-dispositions.md'
_i = _disp_c.index('\n## %s. ' % PT3A_CLOSE_ENTRY)
CLOSE_TEXT = _disp_c[_i:]
for _needle in ('CLOSED / PASS WITH LIMITATIONS', GREEN_T3A[:8], RED_T3A[:8], 'owner-pbt3a-nc2-decision.md', 'nc1-cascade-substitute/',
                'SATISFIED_BY_OWNER_DISPOSITION', 'attempts 2 de 3'):
    assert _needle in CLOSE_TEXT, ('la entrada de cierre de PB-T3a sin', _needle)
for _p in (PT3A_TCP, PBT3AC, CO + 'owner-pbt3a-nc2-decision.md', NC1_SUBST):
    assert git('cat-file', '-e', PT3A_CLOSE + ':' + _p).returncode == 0, ('el cierre de PB-T3a sin', _p)
assert 'CLOSED / PASS WITH LIMITATIONS' in show(PT3A_TCP, PT3A_CLOSE).split('\n', 4)[2], 'el task-close-package de PB-T3a no dice CLOSED'
_run, _sha, _core, _ui, _skip = CI_BASE_T3A
assert re.fullmatch(r'\d+', _run) and GREEN_T3A.startswith(_sha) and _run in CLOSE_TEXT, ('la CI del GREEN de PB-T3a no es la de su cierre', CI_BASE_T3A)
assert ('Core %s y UI %s (%s pasadas y %d omitidas)' % (format(_core, ',').replace(',', ' '), format(_ui, ',').replace(',', ' '),
                                                        format(_ui - _skip, ',').replace(',', ' '), _skip)) in CLOSE_TEXT, \
    ('los recuentos de CI_BASE_T3A no son los de la entrada de cierre', CI_BASE_T3A)
for _n in DRAFT_NAMES:
    assert in_head(DRAFT + _n), ('el cierre de PB-T3a sin su archivo de Draft/', _n)
ANCHOR_SRC = ('desciende de %s (%s), %s (%s), %s (reemision, %s), el GREEN %s y el cierre %s (%s); src y tests == el cierre == el GREEN de PB-T3a; '
              'los ocho de Draft/' % (D1_REV[:8], D1_ENTRY, PT3A_EMIT[:8], PT3A_EMIT_ENTRY, PT3A_REISSUE[:8], PT3A_STOP_ENTRY, GREEN_T3A[:8],
                                       PT3A_CLOSE[:8], PT3A_CLOSE_ENTRY))

# ---------------------------------------------------------------- el contrato emitido de PB-T3a: las listas, los filtros, las celdas y las paradas
pbt3a = json.loads(show(PBT3AC))
assert pbt3a['TaskId'] == PT3A_TASK and pbt3a['AuthorityRevision'] == PT3A_REISSUE_X, 'el contrato de PB-T3a no es el reemitido (F4B-E42)'
assert len(pbt3a['AllowedWriteScope']) == 14 and len(pbt3a['ForbiddenWriteScope']) == 203 and len(pbt3a['StopConditions']) == 33, 'el contrato de PB-T3a'
T3A_STOP = {s['Id']: s['Text'] for s in pbt3a['StopConditions']}
P01 = 'config.toml no longer matches the accepted Snapshot in force (today Snapshot %s, %s)'
STD_IDS = ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
P_STOPS = [dict(s) for s in pbt3a['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
assert [s['Id'] for s in P_STOPS] == STD_IDS and P_STOPS[9]['Text'] == P01 % ('K', SNAP_K), 'las 17 estandar de PB-T3a, con P-01 en la K'


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


# la K y su decision estan en la revision desde el cierre de PB-T2b: siempre assert
assert in_head(SNAP_K) and in_head(SNAP_K_DEC), ('la Snapshot K o su decision no estan en la revision', SNAP_K, SNAP_K_DEC)
_err = snapk_check(show(SNAP_K), show(SNAP_K_DEC), SNAP_K_SHA, SNAP_K_PROBE)
assert not _err, ('la Snapshot K (F4B-E37)', _err)

# ---------------------------------------------------------------- el alcance (section 6.2: 11 rutas; section 5.3: las clases)
# las clases de 5.3 con sus CT (la lista cerrada CT-PBT3B-01 a 15), todas en tests/RackCad.Tests/, fuera de Workspace/
CLASS_CTS = [('Core', 'PushBackFullWritePrepareTests', [1, 2, 3, 4, 5, 6, 10, 11, 15]), ('Core', 'PushBackFullWriteRunTests', [7, 8, 9, 12]),
             ('Core', 'PushBackWritePathGuardTests', [13]), ('Core', 'PushBackPilotUpdateEquivalenceGuardTests', [14])]
assert sorted(n for _, _, cts in CLASS_CTS for n in cts) == list(range(1, 16)), 'la lista cerrada CT-PBT3B-01 a 15'
SUPPORT = TR + 'PushBackFullWriteSupport.cs'  # el doble con borrado (V2-06), PilotEraseScopeDouble
TEST_NEW = [TR + c + '.cs' for _, c, _ in CLASS_CTS]
NEW_FILES = [ERASER] + TEST_NEW + [SUPPORT]
ALLOWED = [PP, UR, ERASER] + TEST_NEW + [SUPPORT] + D1_GUARDS
assert len(ALLOWED) == 11 == len(set(ALLOWED))  # INV-PBT3B-01 (11 rutas)
UNIT_CHANGE_PATHS = [PP, UR]  # section 6.0 y 6.2: las rutas existentes de producto que cambia (F4B-E25)
for p in [PP, UR] + D1_GUARDS:
    assert in_head(p) and (not DISK or os.path.exists(p)), ('no existe', p)
assert all(not in_head(p) and (not DISK or not os.path.exists(p)) for p in NEW_FILES), [p for p in NEW_FILES if in_head(p)]
for _pref in ('PushBackFullWrite', 'PushBackWritePath', 'PushBackPilotUpdateEquivalence', 'IPushBackPilotViewEraser', 'PilotEraseScopeDouble'):
    assert not [p for p in tracked('tests/') + tracked('src/') if _pref in p], ('el prefijo ya existe', _pref)
# el doble y las clases nuevas no empiezan por Workspace (section 5.0, el doble con borrado; los filtros de 6.2)
assert not [p for p in NEW_FILES if os.path.basename(p).startswith('Workspace')]


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
# INV-PBT3B-01: en src/, solo PP, UR y el archivo nuevo; el resto del piloto, Draft/, el anfitrion, la ventana, el Plugin y Domain, enteros
assert {'src/RackCad.Domain/', 'src/RackCad.Plugin/', 'src/RackCad.UI/', DRAFT, 'src/RackCad.Application/Workspace/'} <= set(SRCF)
assert [p for p in tracked(PILOTD) if p not in SRCF] == [PP, UR], 'Pilot/: fuera del ForbiddenWriteScope, solo PP y UR'
assert UP in SRCF and PM in SRCF and IR in SRCF, 'UP, PM e IR prohibidos (INV-PBT3B-05; D-PBT3-10)'

# tests/ (section 6.0, V42-05): las listas del contrato de PB-T3a mas sus seis clases; en PB-T3b, tests/RackCad.Tests/Workspace/ no va como prefijo,
# sino su lista explicita menos los tres archivos de guardas de D-1 (76 de 79 en 2f5d3dec y d8ccbcb8; R-15)
T3A_TESTS = [f for f in pbt3a['AllowedWriteScope'] if f.startswith('tests/')]
assert [os.path.basename(f) for f in T3A_TESTS] == ['PushBackFullDraftLoadTests.cs', 'PushBackFullDraftCompositionTests.cs', 'PushBackFullDraftViewTests.cs',
                                                     'PushBackFullDraftClassifierTests.cs', 'PushBackDraftWriteGuardTests.cs',
                                                     'PushBackFullDraftWindowParityTests.cs'], T3A_TESTS
CORE_T3A = [f for f in pbt3a['ForbiddenWriteScope'] if f.startswith(TR)]
UI_T3A = [f for f in pbt3a['ForbiddenWriteScope'] if f.startswith(UT)]
assert len(CORE_T3A) == 29 and len(UI_T3A) == 68 and WS in CORE_T3A and UT + 'Workspace/' in UI_T3A, (len(CORE_T3A), len(UI_T3A))
assert not [f for f in CORE_T3A if f.startswith(WS) and f != WS], 'el contrato de PB-T3a ya nombra archivos de Workspace/'
WS_FILES = sorted(p for p in tracked(WS))
assert all('/' not in p[len(WS):] for p in WS_FILES), 'tests/RackCad.Tests/Workspace/ con subcarpetas: la lista explicita no las cubre'
assert all(g in WS_FILES for g in D1_GUARDS)
WS_LIST = [p for p in WS_FILES if p not in D1_GUARDS]
assert len(WS_FILES) == 79 and len(WS_LIST) == 76, ('tests/RackCad.Tests/Workspace/: 79 archivos, 76 sin los tres de D-1 (R-15)', len(WS_FILES), len(WS_LIST))
_k = CORE_T3A.index(WS)
CORE_T = CORE_T3A[:_k] + WS_LIST + CORE_T3A[_k + 1:] + [f for f in T3A_TESTS if f.startswith(TR)]
UI_T = UI_T3A + [f for f in T3A_TESTS if f.startswith(UT)]
assert len(CORE_T) == 28 + 76 + 5 == len(set(CORE_T)) and len(UI_T) == 69 == len(set(UI_T)), (len(CORE_T), len(UI_T))
assert TR + 'Fixtures/' in CORE_T and UT + 'Workspace/' in UI_T  # Fixtures/I64/ cae bajo Fixtures/
TESTF = CORE_T + UI_T
assert not any(a == t or (t.endswith('/') and a.startswith(t)) for a in ALLOWED for t in TESTF)
ROOT = [f for f in pbt3a['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
assert ROOT == ['docs/', '.github/', 'eng/', 'tools/', 'assets/', 'deploy/', 'RackCad.sln', 'Directory.Build.props', 'Directory.Build.targets', 'global.json',
                'NuGet.Config', 'README.md', 'AGENTS.md', 'CLAUDE.md', '.editorconfig', '.gitattributes', '.gitignore'], ROOT
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == len(set(FORBIDDEN)), 'duplicados en el ForbiddenWriteScope'
for f in FORBIDDEN:
    assert in_head(f) and (not DISK or os.path.exists(f.rstrip('/'))), ('no existe', f)
# todo archivo de tests/ que no esta en el AllowedWriteScope cae en el ForbiddenWriteScope, salvo los que no cubre ninguna lista curada (como en
# PB-T3a); y ningun archivo de Workspace/ sin los tres de D-1 queda fuera
assert all(p in FORBIDDEN for p in WS_LIST)

# ---------------------------------------------------------------- las anclas de PB-T3b en la revision (archivo, linea, texto contenido)
ANCHORS = [
    # D-PBT3-09: PP. El estado de entrada del piloto (su gemela), el Prepare del piloto con su orden (PP:309-334; CT-15), el Build con
    # ComposeForWrite tras la comparacion (P5), (b) a (d) (PP:347-388), la disposicion de hoy (PP:398-401), (g) y el compositor (PP:421-438)
    (PP, 19, 'public sealed class PushBackPilotPrepareInput'), (PP, 21, 'private PushBackPilotPrepareInput('),
    (PP, 58, 'public static PushBackPilotPrepareInput From(DraftSession<PushBackPilotDraftState> draft)'),
    (PP, 183, 'internal PushBackPilotPostExpectation(double annotationScale, IEnumerable<PushBackPilotKeyedText> envelopes)'),
    (PP, 194, 'public bool Equals(PushBackPilotPostExpectation? other) =>'),
    (PP, 199, 'public override int GetHashCode() => HashCode.Combine(AnnotationScale, Envelopes.Count);'),
    (PP, 208, 'public sealed class PushBackPilotUpdatePlan : IEquatable<PushBackPilotUpdatePlan>'), (PP, 210, 'internal PushBackPilotUpdatePlan('),
    (PP, 231, 'public string OriginKey { get; }'), (PP, 255, 'public bool Equals(PushBackPilotUpdatePlan? other) =>'),
    (PP, 275, 'public static class PushBackPilotUpdatePlanner'),
    (PP, 277, 'public const string MissingInputReason = '), (PP, 293, 'public const string ViewUnavailablePrefix = "El piloto no borra vistas y la vista ";'),
    (PP, 297, 'public const string ViewAddressPrefix = "El descriptor de la vista ";'),
    (PP, 299, 'public const string ViewAddressSuffix = " no se puede interpretar: usa RACKEDITAR.";'),
    (PP, 309, 'public static PushBackPilotPrepareResult Prepare(PushBackPilotPrepareInput input, PushBackPilotIntentResult reread, RackCatalog catalog)'),
    (PP, 315, 'if (input.ConsideredStatus != DraftStatus.Clean && input.ConsideredStatus != DraftStatus.Dirty)'),
    (PP, 317, 'if (input.HasPendingText) return PushBackPilotPrepareResult.Without(PushBackPilotPrepareKind.PendingText);'),
    (PP, 318, 'if (input.ConsideredStatus == DraftStatus.Clean) return PushBackPilotPrepareResult.Without(PushBackPilotPrepareKind.NothingToApply);'),
    (PP, 322, 'var comparison = input.StaleBase.CompareWith(reread.StaleBase);'),
    (PP, 325, 'if (!reread.Read.IsAdmitted)'), (PP, 328, 'return Build(input, reread, catalog);'),
    (PP, 337, 'private static PushBackPilotPrepareResult Build(PushBackPilotPrepareInput input, PushBackPilotIntentResult reread, RackCatalog catalog)'),
    (PP, 344, 'var composition = PushBackPilotMaterializer.ComposeForWrite(input.InitialDraftState, input.CurrentDraftState, catalog);'),
    (PP, 350, 'foreach (var entry in reread.StaleBase.Entries.Values.Where(e => e.Membership == DraftMembership.Mutable).OrderBy(e => e.Key, '
              'StringComparer.Ordinal))'),
    (PP, 358, 'var origin = input.InitialDraftState.Origin;'), (PP, 360, 'if (origins.Count == 0) return PushBackPilotPrepareResult.WithInvalid(NoOriginReason);'),
    (PP, 368, 'var preflight = store.PreflightInnerSources(members.Select(m => m.Embed.Design).ToList(), RackSystemKind.PushBack, initiating);'),
    (PP, 380, 'design = new PushBackCompositeEditorAssembler(catalog).BuildDesign(composition.Editor!, composition.Inputs!);'),
    (PP, 383, 'facts = new PushBackViewAvailabilityFacts(cortes.Select(corte => corte.PostIndex), system.IsComposite);'),
    (PP, 399, 'return PushBackPilotPrepareResult.WithInvalid(ViewAddressPrefix + entry.Key + ViewAddressSuffix);'),
    (PP, 400, 'if (RackViewAvailability.Evaluate(decoded, facts).Status != RackViewAvailabilityStatus.Available)'),
    (PP, 401, 'return PushBackPilotPrepareResult.WithInvalid(ViewUnavailablePrefix + entry.Key + ViewUnavailableSuffix);'),
    (PP, 421, 'var designText = store.Serialize(RackProject.ForPushBack(design).WithSourceMetadataFrom(preflight.ResolvedSources[index]));'),
    (PP, 422, 'plans.Add(new PushBackPilotMemberPlan(entry.Key, view, section, designText, ComposeMemberEnvelope(embed, view, section, designText)));'),
    (PP, 425, 'var scale = input.CurrentDraftState.Inputs.Annotations.AnnotationScale;'),
    (PP, 434, 'private static string ComposeMemberEnvelope(RackEmbedDocument source, string view, int section, string designJson)'),
    (PP, 436, 'var composed = RackEmbedComposer.Compose(source, RackEmbedDocument.KindPushBack, source.Id, source.Name, view, section, designJson);'),
    # D-PBT3-10: UR. Precheck (UR:122-134; CT-15), Execute con el patron de M-01 (UR:139-157), ComparePost (UR:163-183; CT-09), DesiredNamesOf,
    # Execution y sus miembros (UR:227-587; R-18), el origen de PREPARE y MUTATE (UR:305, :311, :399), POST con plan.OriginKey (UR:481; V2-01)
    (UR, 122, 'public static PushBackPilotPrecheck Precheck(DraftSession<PushBackPilotDraftState> draft)'),
    (UR, 126, 'var result = PushBackPilotUpdatePlanner.Prepare(PushBackPilotPrepareInput.From(draft), null!, null!);'),
    (UR, 139, 'public static DraftApplyOutcome<PushBackPilotDraftState> Execute('), (UR, 148, 'var execution = new Execution(draft, request, port);'),
    (UR, 155, 'return execution.OutcomeOfAnEscape(ex);'),
    (UR, 163, 'public static string? ComparePost(PushBackPilotUpdatePlan plan, PushBackPilotIntentResult? postRead)'),
    (UR, 166, 'if (postRead == null) return PostRereadMissingReason;'), (UR, 173, 'if (found.Count != expected.Count) return PostKeysDifferReason;'),
    (UR, 181, 'var scale = postRead.Read.InitialState!.Inputs.Annotations.AnnotationScale;'),
    (UR, 190, 'public static IReadOnlyDictionary<string, string> DesiredNamesOf(PushBackPilotUpdatePlan plan, RackCatalog catalog)'),
    (UR, 227, 'private sealed class Execution'), (UR, 229, 'private readonly DraftSession<PushBackPilotDraftState> _draft;'),
    (UR, 234, 'private PushBackPilotPrepareInput? _input;'),
    (UR, 241, 'public Execution(DraftSession<PushBackPilotDraftState> draft, PushBackPilotUpdateRequest request, IPushBackPilotUpdatePort port)'),
    (UR, 250, 'public DraftApplyOutcome<PushBackPilotDraftState> Run()'), (UR, 293, 'public DraftApplyOutcome<PushBackPilotDraftState> OutcomeOfAnEscape(Exception ex)'),
    (UR, 305, '_input = PushBackPilotPrepareInput.From(_draft);'), (UR, 311, 'var origin = ChooseOrigin(values, _input.InitialDraftState.Origin, out var originKey);'),
    (UR, 391, 'private DraftApplyOutcome<PushBackPilotDraftState>? Write(IPushBackPilotMutationScope mutation, Action markSealing)'),
    (UR, 399, 'var origin = ChooseOrigin(values, _input!.InitialDraftState.Origin, out var originKey);'),
    (UR, 460, 'private DraftApplyOutcome<PushBackPilotDraftState> Post(IPushBackPilotUpdateScope scope)'),
    (UR, 481, 'var read = PushBackPilotIntentReader.Read(ScanOf(values.Scan, plan.OriginKey), _catalog);'),
    (UR, 482, 'var cause = ComparePost(plan, read);'),
    (UR, 537, 'private DraftApplyOutcome<PushBackPilotDraftState>? SurvivorsAndLayers(PushBackPilotScanValues values, PushBackPilotUpdatePlan plan)'),
    (UR, 579, 'private DraftApplyOutcome<PushBackPilotDraftState> PostFailed(string cause) =>'),
    (UR, 585, 'private static DraftApplyOutcome<PushBackPilotDraftState> NotAppliedPreflight(string cause) =>'),
    # UP no cambia: el ambito de MUTATE y el resultado de una unidad, que la interfaz nueva reutiliza (D-PBT3-10)
    (UP, 8, 'namespace RackCad.Application.Systems.PushBack.Pilot'), (UP, 42, 'public interface IPushBackPilotMutationScope : IDisposable'),
    (UP, 48, 'PushBackPilotUnitResult WriteUnit(string memberKey, string envelopeText);'), (UP, 139, 'public sealed class PushBackPilotUnitResult'),
    # D-PBT3-09 (d2): la compuerta de salida de PB-T1b; (b): la escala del piloto; PM, la composicion del piloto que la barrera mira
    (APP + 'PushBackEditorOutputGate.cs', None, 'public static bool IsBlocked('), (APP + 'PushBackEditorOutputGate.cs', None, 'public static string StatusOr('),
    (PM, 112, 'public const string NonPilotContentReason = "El borrador cambia contenido que el piloto no edita: no se compone.";'),
    (PM, 196, 'public static PushBackPilotComposition ComposeForWrite('),
    # el registro del anfitrion (WHC:347-352): la receta de los borradores (section 5.0) y "Find<PushBackPilotDraftState> es el unico Find<" (CT-13)
    (WHC, 347, 'var begun = _drafts.Begin('), (WHC, 351, 'PushBackPilotStateComparer.Instance,'),
    # el clasico (RPC): el nombre en blanco (RPC:188) y los cortes y el borrado de obsoletas (RPC:269-271, :320-333)
    (RPC, 269, 'var lateralCortes = new PushBackSystemLateralBuilder().Cortes(system, catalog);'),
    (RPC, 325, 'var availability = RackViewAvailability.Evaluate(decoded, availabilityFacts);'), (RPC, 331, 'staleViewBlocks.Add(viewBlock.BlockId);'),
    # S-1 a S-6 (R-6; section 5.3): los seis metodos de D-1 y las agujas que cambian
    (PG, 121, '[Fact]'), (PG, 122, 'public void TheFileOfThePlanComposesExactlyOnceInAMemberOfItsOwnWithAParameterSourceAndTheNameOfTheRereadEnvelope()'),
    (PG, 127, '[Theory]'), (PG, 128, '[InlineData("source.Id, source.Name,", "source.Id, \\"Otro\\",")]'),
    (PG, 133, 'public void TheComposeGuardTripsOnAnotherCallAnUnsourcedCompositionANullableSourceOrAnotherName(string from, string to)'),
    (PG, 136, 'Assert.Contains(from, plan);'),
    (PG, 229, '[Fact]'), (PG, 230, 'public void TheMemberPlansAreBuiltOnlyFromTheMutableMembersOfTheRereadInOrdinalOrderAndNeverErase()'),
    (UG, 126, '[Fact]'), (UG, 127, 'public void TheRunReleasesBothScopesInFinallyBlocksOfItsOwnAndNeverWithAUsing()'),
    (UG, 134, 'Assert.Equal(1, Src.Count(code, @"\\bmutation\\.Dispose\\s*\\("));'),
    (UG, 140, 'private DraftApplyOutcome<PushBackPilotDraftState> Post(IPushBackPilotUpdateScope scope)'),
    (IG, 883, 'var created = execute.IndexOf("var execution = new Execution(", StringComparison.Ordinal);'),
    (IG, 895, '[Fact]'), (IG, 896, 'public void M01TheEscapeOfTheRunIsClassifiedByTheStateThatTheRunAlreadyHadAndNeverAsAPlainFailureAfterAWriteOrASeal()'),
    (IG, 902, '[Fact]'), (IG, 903, 'public void M01TheEscapeGuardTripsOnACatchThatDoesNotConsultTheStateAndOnAnEscapeThatAnswersAPlainFailureFirst()'),
    (IG, 908, 'Assert.NotEmpty(RunEscapeViolations(run.Replace("var execution = new Execution(draft, request, port);", string.Empty, StringComparison.Ordinal)));'),
    # [PKT1] v2.8: PBT1C-C13 (:1338), la forma de C13; [PKC2]: las paradas estandar (:579-581) y las celdas (:655)
    ('docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md', 1338, '- **PBT1C-C13:** keeping a gesture without observable change'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 579, '### 6.7 `StopConditions`'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 655, 'vencen el 2026-12-29'),
]
TEXT = {}
for path, line, text in ANCHORS:
    if path not in TEXT:
        TEXT[path] = lines_of(path)
    if line is None:
        assert sum(1 for l in TEXT[path] if text in l) == 1, ('ancla por contenido, no unica o ausente', path, text)
    else:
        assert text in TEXT[path][line - 1].strip(), ('ancla', path, line, TEXT[path][line - 1].strip())


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


# S-1 a S-6 (R-6; section 5.3): los seis metodos de D-1, en la base sin Skip; S-2 es la Theory de cinco filas (PrepareGuard:127-139)
D1_METHODS = [('S-1', PG, 121, 'Fact', 'TheFileOfThePlanComposesExactlyOnceInAMemberOfItsOwnWithAParameterSourceAndTheNameOfTheRereadEnvelope', 'E-1'),
              ('S-2', PG, 127, 'Theory', 'TheComposeGuardTripsOnAnotherCallAnUnsourcedCompositionANullableSourceOrAnotherName', 'E-2'),
              ('S-3', PG, 229, 'Fact', 'TheMemberPlansAreBuiltOnlyFromTheMutableMembersOfTheRereadInOrdinalOrderAndNeverErase', 'E-3'),
              ('S-4', UG, 126, 'Fact', 'TheRunReleasesBothScopesInFinallyBlocksOfItsOwnAndNeverWithAUsing', 'E-4'),
              ('S-5', IG, 895, 'Fact', 'M01TheEscapeOfTheRunIsClassifiedByTheStateThatTheRunAlreadyHadAndNeverAsAPlainFailureAfterAWriteOrASeal', 'E-5'),
              ('S-6', IG, 902, 'Fact', 'M01TheEscapeGuardTripsOnACatchThatDoesNotConsultTheStateAndOnAnEscapeThatAnswersAPlainFailureFirst', 'E-6')]
D1_ROWS = 0  # los resultados que el RED deja de ejecutar: una fila por Fact y una por InlineData de la Theory
for _sid, _path, _line, _kind, _name, _eq in D1_METHODS:
    _l = TEXT[_path]
    assert _l[_line - 1].strip() == '[%s]' % _kind, ('el atributo de %s no es [%s] sin Skip en la base' % (_sid, _kind), _l[_line - 1].strip())
    _decl = [k for k in range(_line, _line + 8) if ('public void %s(' % _name) in _l[k]]
    assert len(_decl) == 1, ('el metodo de %s no esta tras su atributo' % _sid, _name)
    _rows = [k for k in range(_line, _decl[0]) if _l[k].strip().startswith('[InlineData(')]
    assert (_kind == 'Theory') == bool(_rows) and (_kind != 'Theory' or len(_rows) == 5), ('las filas de ' + _sid, len(_rows))
    assert sum(1 for x in _l if ('public void %s(' % _name) in x) == 1, ('el metodo de %s no es unico en su archivo' % _sid, _name)
    D1_ROWS += len(_rows) or 1
assert D1_ROWS == 10, D1_ROWS  # cinco Fact y las cinco filas de S-2
for _g in D1_GUARDS:  # ningun Skip en los tres archivos de guardas antes del RED
    assert not [x for x in TEXT[_g] if re.match(r'\s*\[(Fact|Theory)\s*\(\s*Skip', x)], ('un Skip en la base', _g)

# PBT3B-C13 (section 6.2): los nombres nuevos que prescriben D-PBT3-09 y D-PBT3-10 no existen en Pilot/ (R-18, ahora sobre el cierre de PB-T3a);
# los nombres existentes que ganan una segunda declaracion estan una sola vez; los miembros de Execution que cambian de firma estan, con el tipo
# del piloto (D-PBT3-10: PushBackPilotDraftState pasa a TState)
C13_NEW = ['PushBackFullPrepareInput', 'BuildCore', 'BuildFull', 'BlockedPrefix', 'FrontalBOfOneWayPrefix', 'FrontalBOfOneWaySuffix', 'AllViewsObsoleteReason',
           'Erasures', 'PostOriginKey', 'ErasedKeys', 'ComparePostCore', 'EraseUnsupportedReason', 'PostErasedStillPresentPrefix', '_prepare', '_originOf',
           '_postRead', '_origin', 'IPushBackPilotViewEraser', 'EraseUnit']
_gg = git('grep', '-l', '-w', *sum([['-e', n] for n in C13_NEW], []), REV, '--', PILOTD)
assert _gg.returncode == 1 and not _gg.stdout.strip(), ('un nombre nuevo de PBT3B-C13 ya existe en Pilot/', _gg.stdout.decode('utf-8')[:300])
_gg = git('grep', '-l', '-w', '-e', 'PushBackFullPrepareInput', '-e', 'IPushBackPilotViewEraser', '-e', 'PilotEraseScopeDouble', REV, '--', 'src', 'tests')
assert _gg.returncode == 1, ('un tipo nuevo de PB-T3b ya existe en src/ o tests/', _gg.stdout.decode('utf-8')[:300])
_pp, _ur = '\n'.join(TEXT[PP]), '\n'.join(TEXT[UR])
for _txt, _what, _n in ((_pp, 'public static PushBackPilotPrepareResult Prepare(', 1), (_pp, 'private static string ComposeMemberEnvelope(', 1),
                        (_pp, 'internal PushBackPilotUpdatePlan(', 1), (_pp, 'internal PushBackPilotPostExpectation(', 1),
                        (_pp, 'private static PushBackPilotPrepareResult Build(', 1), (_ur, 'public static PushBackPilotPrecheck Precheck(', 1),
                        (_ur, 'public static DraftApplyOutcome<PushBackPilotDraftState> Execute(', 1), (_ur, 'public static string? ComparePost(', 1),
                        (_ur, 'private sealed class Execution', 1), (_ur, 'new Execution(', 1)):
    assert _txt.count(_what) == _n, ('la declaracion existente que PB-T3b generaliza no esta una sola vez', _what, _txt.count(_what))
EXEC_TYPED = ['_draft', 'Run', 'OutcomeOfAnEscape', 'Prepare', 'Mutate', 'Write', 'WithAbortFailure', 'Post', 'ChooseOrigin', 'SurvivorsAndLayers',
              'WithScopeDisposeFailure', 'PostFailed', 'NotAppliedFailure', 'NotAppliedPreflight']
_ex0 = TEXT[UR].index([x for x in TEXT[UR] if x.strip() == 'private sealed class Execution'][0])
_exec = TEXT[UR][_ex0:]
for _m in EXEC_TYPED:
    _d = [x.strip() for x in _exec if re.match(r'\s*(public|private|internal)\b.*\b%s\b\s*(\(|;|=)' % re.escape(_m), x)
          and 'PushBackPilotDraftState' in x]
    assert len(_d) == 1, ('el miembro de Execution de D-PBT3-10 no tiene PushBackPilotDraftState en su firma una sola vez', _m, _d)


def decl_census(lines):
    """Las declaraciones de un archivo, contadas una a una con su nombre (una sobrecarga es una declaracion mas): cada linea que empieza por un
    modificador de acceso, con el nombre del tipo, del constructor, del metodo, de la propiedad, del campo o de la constante (el metodo de R-18)."""
    out = []
    for raw in lines:
        s = code_of(raw).strip()
        m = re.match(r'(?:public|private|internal|protected)\b(.*)$', s)
        if not m:
            continue
        rest = m.group(1)
        t = re.search(r'\b(?:class|struct|interface|enum|record(?:\s+struct)?)\s+(\w+)', rest)
        if t:
            out.append(t.group(1))
            continue
        n = re.search(r'(\w+)\s*(?:<[^()]*>)?\s*(?:\(|\{|;|=>|=)', rest)
        if n:
            out.append(n.group(1))
    return out


CENSUS_BASE = {p: decl_census(TEXT[p]) for p in (PP, UR)}
assert CENSUS_BASE[PP].count('Prepare') == 1 and CENSUS_BASE[UR].count('Execute') == 1 and CENSUS_BASE[UR].count('ComparePost') == 1
assert CENSUS_BASE[UR].count('_input') == 1, 'el campo _input de Execution, que D-PBT3-10 retira'

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

# ---------------------------------------------------------------- el paquete: los textos de la v4.5, literales en ASCII y sin sus notas

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


VARIANT = "(A')"  # la variante (A') de D-1 en ASCII (el prime se pliega a "'"): no elegida (F4B-E29), no va al contrato
PKG = 'docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md'
MEAS = 'docs/initiatives/I-64-f4b-pbt3-measurements.md'
# la v4.5 revisada esta custodiada desde 5bda5569: la de la revision, siempre por su sha256
assert hashlib.sha256(show_bytes(PKG)).hexdigest() == CUSTODY_SHA256[PKG], 'el paquete custodiado en la revision no es la v4.5 revisada (sha256)'
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
def rr_pkg_check(sec_lines, classes, sub='PB-T3b'):
    """La regla RR en el section 6.0 del paquete (V43-01 a V43-06) y su lectura para PB-T3b: el parrafo de F4B-E35, RR-1 a RR-6 una vez cada una;
    en RR-2, que una reparacion de PB-T3b no toca los tres archivos de guardas de F4 con los Skip de D-1 (no los anadio el RED, los cambio); las dos
    lecturas de PB-T3 con el RED de PB-T3b; el RED de la cadena en una correccion de PB-T3b; la CI del RED reparado; la verificacion con una
    reparacion, con el tramo RedSha..RedRepairSha de PB-T3b; y la tabla cerrada de la ayuda de RR-3, cuyas filas de PB-T3b son exactamente las
    clases nuevas (classes). La tabla es texto del paquete, no vinetas: se lee aparte. Devuelve los errores (vacia si vale)."""
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
    rr2 = [fold(l) for l in sec_lines if l.startswith('- **RR-2, ')]
    want2 = ('En PB-T3b, los tres archivos de guardas de F4 con los Skip de D-1 no los anadio el RED, sino que los cambio: una reparacion no los toca, '
             'y un error en un Skip o en su motivo literal es la parada de siempre.')
    if sub == 'PB-T3b' and not (len(rr2) == 1 and rr2[0].rstrip().endswith(want2)):
        errors.append('RR-2 no termina con la lectura de PB-T3b: una reparacion no toca los tres archivos de guardas de F4 con los Skip de D-1')
    if sum(1 for l in sec_lines if l.startswith('- **Las dos lecturas de PB-T3, porque los tres RED ya tocan `src/`**')) != 1:
        errors.append('faltan las dos lecturas de PB-T3 (V43-01)')
    two = [fold(l) for l in sec_lines if l.startswith('- **Las dos lecturas de PB-T3')]
    if sub == 'PB-T3b' and not (two and 'el de PB-T3b, el del API nuevo en PP y UR y la interfaz, ademas de los Skip en tres pruebas existentes' in two[0]):
        errors.append('las dos lecturas de PB-T3 sin el RED de PB-T3b (el API nuevo en PP y UR, la interfaz y los Skip)')
    if sub == 'PB-T3b' and sum(1 for l in sec_lines if l == '- en PB-T3b, el esqueleto del API nuevo con el piloto de la base;') != 1:
        errors.append('el RED de la cadena en una correccion de PB-T3b no es "el esqueleto del API nuevo con el piloto de la base"')
    if sum(1 for l in sec_lines if l.startswith('- **la CI del RED reparado** (F4B-E35, RR-4; V43-04)')) != 1:
        errors.append('falta la CI del RED reparado (V43-04)')
    if t.count('**La verificaci' + o + 'n con una reparaci' + o + 'n del RED** (F4B-E35, RR-7') != 1:
        errors.append('falta la verificacion con una reparacion del RED (V43-05)')
    if sub == 'PB-T3b' and [fold(l) for l in sec_lines].count('  - en PB-T3b, PP, UR ni IPushBackPilotViewEraser.cs, ni los tres archivos de guardas de '
                                                             'F4 (los Skip de D-1);') != 1:
        errors.append('la verificacion con una reparacion sin el tramo RedSha..RedRepairSha de PB-T3b (PP, UR, la interfaz y los tres de guardas)')
    rows = [l for l in sec_lines if re.match(r'^\| PB-T3[abc] \| `', l)]
    mine = [re.match(r'^\| %s \| `(\w+)`' % re.escape(sub), l).group(1) for l in rows if l.startswith('| %s |' % sub)]
    if sorted(mine) != sorted(classes):
        errors.append('la tabla cerrada de RR-3 no nombra exactamente las clases nuevas de %s: %s' % (sub, ascii(mine)))
    return errors
# ---- rr_pkg_check (fin)


# ---- rr3_table_check (inicio)
def rr3_table_check(sec_lines, classes, sec53, sub='PB-T3b'):
    """La tabla cerrada de la ayuda de RR-3 (V43-06; V44-02 y V44-03; CA-T3a M-02 y M-03) en sus filas de PB-T3b: una fila por cada clase nueva
    (classes) y ninguna otra; cada fila, partida por <br>, en partes que empiezan por '**Valores:**' o '**Lineas:**'; sus literales, entre comillas
    invertidas, fuera de parentesis o dentro de un parentesis que solo los enumera (como '(`2.0` y `2.5`)'); los valores, contables como token (una
    cadena entre comillas dobles o un numero de dos o mas caracteres). Lo que la fila no enumera sin interpretar, y que se DERIVA de la lista
    CT-PBT3B de 5.3 (sec53), se resuelve aqui de forma mecanica y se informa aparte: una plantilla con marcas '<...>' ('<Fn> ... [<valor>]', con
    los valores de una CT), un literal cortado con una elipsis (el entero, unico, en 5.3) y 'las N lineas literales de CT-nn' (las de la CT, con
    'y las dos iguales con `x`'). Devuelve (errores, info): info[clase] = (lineas, valores, derivadas), con derivadas = [(que, [literales])]."""
    import re, unicodedata
    fold = lambda s: ''.join(c for c in unicodedata.normalize('NFKD', s) if not unicodedata.combining(c))
    ell = chr(0x2026)
    PLACE = r'(?:^|[\s\[=])<\w+>'  # una marca de plantilla ('<Fn>', '[<valor>]'), no un argumento de tipo pegado a su nombre ('Find<T>')
    words ={'dos': 2, 'tres': 3, 'cuatro': 4, 'cinco': 5, 'seis': 6, 'siete': 7, 'ocho': 8, 'nueve': 9, 'diez': 10, 'once': 11}
    ct_rows = {}
    for l in sec53:
        m = re.match(r'^\| \*\*(\d\d)\*\* \|', l)
        if m:
            ct_rows[m.group(1)] = l

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
        return [x for x, g in lits if g is None or re.fullmatch(r'[\s,y]*', groups[g])], lits

    def ct_lits(ct):
        return re.findall(r'`([^`]+)`', ct_rows.get(ct, ''))

    errors, info, seen = [], {}, []
    for row in [l for l in sec_lines if l.startswith('| %s | `' % sub)]:
        cells = row.strip()[2:-2].split(' | ') if row.strip().endswith(' |') else []
        cm = re.match(r'`(\w+)` \(CT-', cells[1]) if len(cells) == 3 else None
        if not cm:
            errors.append('una fila de %s de la tabla de RR-3 sin la forma "| %s | `Clase` (CT-...) | ... |": %s' % (sub, sub, ascii(row[:80])))
            continue
        cls, body = cm.group(1), cells[2]
        seen.append(cls)
        lines, values, derived = [], [], []
        for seg in [s.strip() for s in body.split('<br>')]:
            lm = re.match(r'\*\*([LlVv])(?:ineas|alores):\*\* ', fold(seg))
            if not lm:
                errors.append('una parte de la fila de %s no empieza por "Lineas:" o "Valores:": %s' % (cls, ascii(fold(seg)[:70])))
                continue
            is_val = lm.group(1) in 'Vv'
            items, _ = scan(seg)
            plain = re.sub(r'`[^`]*`', '', fold(seg))
            for n_word, ct in re.findall(r'las (\w+) lineas literales de CT-(\d\d)', plain):
                k = re.search(r'Las l\S{1,2}neas, literales:', ct_rows.get(ct, ''))
                part = ct_rows.get(ct, '')[k.end():].split('**Se comprueba')[0] if k else ''
                base = [x for x in re.findall(r'`([^`]+)`', part) if ' ' in x]
                twin = re.search(r'y las dos iguales con `([^`]+)`', part)
                got = base + ([twin.group(1) + x[x.index(' '):] for x in base] if twin else [])
                if len(got) != words.get(n_word, -1):
                    errors.append('las %s lineas literales de CT-%s de %s no se resuelven en 5.3: %d' % (n_word, ct, cls, len(got)))
                derived.append(('las %s lineas literales de CT-%s' % (n_word, ct), got))
                lines += got
            if not items and not derived:
                errors.append('la parte de %s no da ningun literal' % cls)
            for x in items:
                if re.search(PLACE, x):
                    mm = re.search(r'`%s` con los valores de CT-(\d\d) \(ii\)' % re.escape(x), seg)
                    pairs = []
                    if mm:
                        r1 = ct_rows.get(mm.group(1), '')
                        ex = re.search(r'el exacto en diez filas \(([^)]*)\) y `([^`]+)` en (F\d+)', r1)
                        if ex:
                            pairs = re.findall(r'(F\d+) (\d+\.\d+)', ex.group(1)) + [(ex.group(3), ex.group(2))]
                    pairs.sort(key=lambda p: int(p[0][1:]))
                    got = [x.replace('<Fn>', f).replace('<valor>', v) for f, v in pairs]
                    if not mm or len(got) != 11 or re.search(PLACE, ' '.join(got)):
                        errors.append('la plantilla %s de %s no se resuelve con los valores de su CT' % (ascii(x), cls))
                    derived.append(('la plantilla %s' % ascii(x), got))
                    lines += got
                elif ell in x:
                    pre = x.split(ell)[0]
                    full = sorted({y for ct in ct_rows for y in ct_lits(ct) if y.startswith(pre) and ell not in y and len(y) > len(pre)})
                    if len(full) != 1:
                        errors.append('el literal cortado %s de %s no tiene uno solo entero en 5.3: %d' % (ascii(x), cls, len(full)))
                    derived.append(('el literal cortado %s' % ascii(pre + ell), full))
                    lines += full
                elif is_val:
                    if not (re.fullmatch(r'"[^"]+"', x) or (re.fullmatch(r'\d+(?:\.\d+)?', x) and len(x) >= 2)):
                        errors.append('un valor de %s que no se puede contar como token: %s' % (cls, ascii(x)))
                    values.append(x)
                else:
                    lines.append(x)
        info[cls] = (lines, values, derived)
    if sorted(seen) != sorted(classes):
        errors.append('las filas de %s de la tabla de RR-3 no son, una por clase, las clases nuevas: %s' % (sub, ascii(seen)))
    return errors, info
# ---- rr3_table_check (fin)


# Los textos de la v4.5 para PB-T3b, en ASCII (md_fold) y sin las "Nota (no va al contrato)". El contrato los lleva tal cual.
OBJECTIVE = (
    'Generalizar en su sitio el plan y la corrida de la actualizacion del Workspace de Push Back para que sirvan el modelo del piloto, conectado, y '
    'el modelo completo de PB-T3a, sin conectar: la barrera de diagnosticos con PushBackEditorOutputGate para los dos modelos (el unico cambio visible '
    'del piloto, sobre un diseno bloqueado), el borrado de laterales obsoletos con la regla del ultimo vinculo y el origen de POST reanclado cuando se '
    'borra el origen, el rechazo del frontal B de un rack sin lado B, el renombrado con el nombre del borrador, ComparePost generico y la capacidad de '
    'borrar como interfaz nueva, con un solo BeginMutation y un solo Seal, segun el paquete del contrato de PB-T3 v4.5, la decision del Owner D-1 '
    '(F4B-E29, un solo codigo) y la lista cerrada CT-PBT3B-01..15. El RED es el esqueleto inerte del API nuevo, la interfaz, las pruebas nuevas y el '
    'Skip de las pruebas sustituidas por D-1; el GREEN toca solo el plan y la corrida. Si hiciera falta mutar el rack resuelto o el informe de modulos '
    'dentro del tiempo de vida del borrador: STOP y se reabre L-1. El Plugin, el anfitrion, el puerto del panel, la ventana, el clasico y Domain no '
    'cambian; no se crean vistas; no se ejecuta AutoCAD.')
OBJ_LABEL = 'Objective (ASCII), con la variante (A), la elegida (F4B-E29): '
ALLOWED_PKG = (
    'AllowedWriteScope (11): PP, UR y el nuevo src/RackCad.Application/Systems/PushBack/Pilot/IPushBackPilotViewEraser.cs; las cuatro clases nuevas de '
    'section 5.3 y tests/RackCad.Tests/PushBackFullWriteSupport.cs (V2-06); solo para los Skip de D-1: tests/RackCad.Tests/Workspace/'
    'WorkspacePilotPrepareGuardTests.cs, .../WorkspacePilotUpdateGuardTests.cs y .../WorkspacePilotUpdateIntegrationGuardTests.cs. El registro '
    'F4B-DRAFTS/superseding-characterization.md no entra: lo escribe la sesion en el commit de emision (section 8.2).')
SKELETON = (
    'El esqueleto exacto del RED: en PP: PushBackFullPrepareInput completo (es un contenedor con su From); la sobrecarga completa de Prepare devuelve '
    'WithInvalid(MissingInputReason); Erasures y ErasedKeys, vacias; PostOriginKey = OriginKey; los constructores nuevos y las constantes nuevas, '
    'completos; en UR: Precheck completo devuelve Go(); Execute completo devuelve NotAppliedFailure(UpdateNotRunReason); la sobrecarga completa de '
    'ComparePost devuelve PostRereadMissingReason; las constantes nuevas, completas; el camino del piloto, sin cambio.')
SKIP_PKG = ('El Skip de D-1, con el motivo literal de section 5.3 (con F4B-E29), en los seis metodos S-1 a S-6. Es el unico cambio permitido en esos '
            'archivos; git diff -U0 lo comprueba.')
# el motivo literal del Skip (section 5.3; V2-26 y V41-02), con <E-n> el equivalente de cada metodo: el Worker lo escribe tal cual, sin ASCII
SKIP_REASON = ('I-64 F4B PB-T3b: sustituida por <E-n> de PushBackPilotUpdateEquivalenceGuardTests (registro declarado superseding-characterization.md; '
               'decisi' + chr(0xF3) + 'n del Owner F4B-E29, D-1).')
REQ_TABLE_PKG = (
    'RequiredTests: | Proyecto | Filtro | MinSelected | ExpectRed | |---|---|---|---| | Core | PushBackFullWrite (CT-01 a 12 y 15) | 13 | true | | Core | '
    'PushBackPilotUpdateEquivalenceGuardTests (CT-14) | 6 | true | | Core | PushBackWritePathGuardTests (CT-13) | 2 | false | | Core | Workspace | el '
    'numero de Passed de la CI del RED (V2-26) | false | | Core | PushBackFullDraft, PushBackDraftWriteGuardTests (PB-T3a) | 10, 3 | false | | | Y los '
    'demas de section 6.1, salvo los de PB-T3a con ExpectRed | | false |')
INV_OWN = [
    'INV-PBT3B-01. Alcance: el diff dentro de las 11 rutas del AllowedWriteScope. En src/, solo PP, UR y el archivo nuevo. En los archivos de guardas de '
    'F4, solo las lineas de atributo con Skip de S-1 a S-6.',
    'INV-PBT3B-02. RED y GREEN: como INV-PBT3A-02. El GREEN solo toca PP y UR. Con una reparacion del RED (F4B-E35, RR; V43-03): RedSha..RedRepairSha '
    'solo toca clases de prueba que anadio el RED (no src/ ni los tres archivos de guardas de F4 con los Skip); el criterio se repite entero sobre '
    'RedRepairSha, con los Skip intactos; y RedRepairSha..CurrentSha es el GREEN, solo PP y UR.',
    'INV-PBT3B-03. El API existente no cambia: cada miembro publico e interno de PP y UR en BaseSha existe en el GREEN con la misma firma.',
    'INV-PBT3B-04. El piloto conserva, en Prepare, su orden, sus textos y sus resultados; en la corrida, sus llamadas al puerto; su ComparePost; y su '
    'origen de POST (PostOriginKey = OriginKey). La unica excepcion es la barrera, sobre un diseno bloqueado (V2-09).',
    'INV-PBT3B-05. Las guardas que siguen (section 4): la llamada a ComposeForWrite en el Build del piloto, tras .CompareWith(; '
    'From(DraftSession<PushBackPilotDraftState>) y una sola new PushBackPilotPrepareInput(; el Prepare del piloto primero en el archivo; ninguna '
    'palabra Transaction en Pilot/; las llamadas al puerto solo en UR; UP sin cambio.',
    'INV-PBT3B-06. Un solo "Actualizar" logico: un Begin, un BeginMutation, un Seal y un Post por corrida, y todo EraseUnit entre BeginMutation y Seal '
    '(CT-07).',
]
INV_15 = ('INV-PBT3B-15. La medicion sobre el GREEN (V2-02, V3-03 y V3-08): antes de su commit GREEN, el Worker hace la corrida temporal de section '
          '5.3, "La medicion sobre el GREEN", y la transcribe en la entrega. Para hacerla, quita en local el Skip de los metodos de D-1 y lo vuelve a '
          'poner antes del commit: es la reactivacion temporal, local y sin commit que admiten INV-PBT3B-07 (las redes, el comun de section 6.0) y '
          'PBT3B-C02. El resultado tiene que ser el esperado de la variante elegida.')
C13_LABEL = 'PBT3B-C13 (V2-30 y V3-02), con la variante (A), la elegida (F4B-E29)'
C13_TXT = (
    'doing the task would require changing the signature of an existing public or internal member of PP or UR; touching UP, the Plugin, WHC or the '
    'panel port; or adding in PP, UR or IPushBackPilotViewEraser.cs a declaration whose name does not exist in PP or UR at BaseSha, or a further '
    'declaration of a name that does (an overload), of any visibility (type, constructor, method, property, field, constant or nested type), other '
    'than those that D-PBT3-09 and D-PBT3-10 prescribe: in PP: the type PushBackFullPrepareInput and its nine members (its private constructor, RackId, '
    'StaleBase, InitialDraftState, CurrentDraftState, HasPendingText, IsDirty, ConsideredStatus and From); in PushBackPilotUpdatePlanner, a second '
    'Prepare (the one that takes a PushBackFullPrepareInput), BuildCore, BuildFull and the constants BlockedPrefix, FrontalBOfOneWayPrefix, '
    'FrontalBOfOneWaySuffix and AllViewsObsoleteReason; in PushBackPilotUpdatePlan, Erasures, PostOriginKey and a second internal constructor (the one '
    'with postOriginKey and erasures); in PushBackPilotPostExpectation, ErasedKeys and a second internal constructor (the one with erasedKeys); in UR: a '
    'second Precheck, a second Execute and a second ComparePost (the ones that take the full model), ComparePostCore and the constants '
    'EraseUnsupportedReason and PostErasedStillPresentPrefix; and, in Execution<TState>, the fields _prepare, _originOf, _postRead and _origin; in '
    'IPushBackPilotViewEraser.cs: the interface IPushBackPilotViewEraser and its method EraseUnit. Two changes of private members are prescribed, and '
    'neither is a new declaration: ComposeMemberEnvelope gains the parameter string name after source and remains the only member with that name; and '
    'Execution becomes Execution<TState>, loses the field _input, and its constructor gains the three delegates of _prepare, _originOf and _postRead, '
    'while each of its other members keeps its name and its parameters, with PushBackPilotDraftState replaced by TState where it appears (the field '
    '_draft, Run, OutcomeOfAnEscape, Prepare, Mutate, Write, WithAbortFailure, Post, ChooseOrigin, SurvivorsAndLayers, WithScopeDisposeFailure, '
    'PostFailed, NotAppliedFailure and NotAppliedPreflight). The Worker stops before adding it.')
STOP_OWN_PKG = [  # las propias del section 6.2: (id, texto del paquete); C04 y C17 remiten a las de PB-T3a y se toman de su contrato
    ('C04', 'as PBT3A-C04.'),
    ('C13', C13_TXT),
    ('C14', 'a behaviour test of the F4 pilot (section 4) fails. STOP: it is C03.'),
    ('C16', 'the measurement of INV-PBT3B-15 differs from what is expected for variant (A), or an F4 guard outside the D-1 list fails in the RED or in '
            'the GREEN. It returns to the Owner with the list.'),
    ('C17', 'the D2 limit.'),
]
EVID_FIRST_PKG = (
    'lo de section 6.1, con la CI del RED con tests en failure, ui-tests y build-ui en success y build-plugin omitido; en la lectura de la verificacion '
    'con una reparacion, la frase propia de la subtarea en lugar de la de PB-T3a: "En PB-T3b, ademas: RedSha..RedRepairSha no toca PP, UR ni '
    'IPushBackPilotViewEraser.cs, ni los tres archivos de guardas de F4 con los Skip de D-1" (V44-04);')
EVID_OWN = [
    'el git diff -U0 BaseSha..CurrentSha de los archivos de guardas de F4, que solo lleva las lineas de atributo con Skip de S-1 a S-6 (V3-03): ni la '
    'reactivacion de la medicion ni ningun otro cambio;',
    'el recuento de Skipped y Passed del filtro Workspace en el RED y en el GREEN;',
    'la medicion de INV-PBT3B-15, con las lineas de censo de S-2 y S-6;',
    'el censo de declaraciones de PP, UR e IPushBackPilotViewEraser.cs en BaseSha y en CurrentSha, contadas una a una con su nombre (una sobrecarga es '
    'una declaracion mas): la diferencia es la lista de PBT3B-C13 de la variante (A), sin _input;',
    'el Trailer real.',
]
CI_LINE_PKG = ('Linea base de la CI: la del cierre de PB-T3a. GREEN previsto, con la variante (A): Core + 21 (13 + 6 + 2), con los Skipped de Core en '
               '+ 6 (o + 10, segun cuente la TRX las filas de S-2; R-11); UI sin cambio. En cadena desde lo previsto de PB-T3a (V42-01): Core 19 128 y '
               'UI 2 451, previstos.')
# las sustituciones de PB-T3b en las vinetas de la ExpectedEvidence de PB-T3a ("lo de section 6.1, con ..."): la CI del RED y la frase propia
EVID_T3A_SUBS = {2: ('(tests y ui-tests en failure, build-ui en success, build-plugin omitido, sin cuelgue)',
                     '(tests en failure, ui-tests y build-ui en success, build-plugin omitido, sin cuelgue)'),
                 8: ('En PB-T3a, ademas: RedSha..RedRepairSha no toca ningun archivo de src/ (el esqueleto de Draft/).',
                     'En PB-T3b, ademas: RedSha..RedRepairSha no toca PP, UR ni IPushBackPilotViewEraser.cs, ni los tres archivos de guardas de F4 con '
                     'los Skip de D-1.')}
STOP_LABELS = {'C07': 'C07 (con la excepcion de F4B-E35, V43-02)', 'C18': 'C18 (nueva, V43-02; el texto de PBT2x-C18 de PB-T2)'}
COMMON_STOPS = ['C01', 'C02', 'C03', 'C05', 'C06', 'C07', 'C09', 'C10', 'C18']


SEC60, SEC62, SEC53 = section('### 6.0 '), section('### 6.2 Contrato de PB-T3b'), section('### 5.3 ')
B60, B62, B62_2, B53 = bullets(SEC60, 0), bullets(SEC62, 0), bullets(SEC62, 2), bullets(SEC53, 0)
# el bloque citado de PBT3x-C11 (V43-09): una sola linea '  > ' del section 6.0
_q11 = [l for l in SEC60 if l.startswith('  > ')]
assert len(_q11) == 1, ('el bloque citado de PBT3x-C11 del section 6.0', len(_q11))
C11_FOLD = ascii_fold(_q11[0][4:]).replace('`', '')
# la variante (A') no elegida (F4B-E29; V41-03): ningun texto que se copia al contrato la lleva (el section 6.2 si la nombra, en sus notas)
_copied = ([b for b in B62 if b.startswith((OBJ_LABEL, 'Linea base de la CI'))] + [b for b in B62_2 if b.startswith(('INV-PBT3B-', 'PBT3B-C'))] +
           B62_2[[i for i, b in enumerate(B62_2) if b.startswith('lo de section 6.1, ')][0]:] +
           [b for b in B60 if re.match(r'^(0[7-9]|1[0-4])\. |^C\d\d(: | \()', b)] + [C11_FOLD])
assert len(_copied) == 2 + 7 + 5 + 6 + 8 + 9 + 1, ('los textos que se copian del paquete', len(_copied))
assert not [b[:60] for b in _copied if VARIANT in b], ("un texto que se copia al contrato lleva la variante (A') no elegida", [b[:60] for b in _copied if VARIANT in b])
assert VARIANT in ascii_fold('\n'.join(SEC62)), "la comprobacion de (A') seria vacia: el section 6.2 ya no la nombra"
assert one(B62, OBJ_LABEL) == OBJ_LABEL + '"' + OBJECTIVE + '"', 'el Objective del paquete difiere'
assert ascii_fold(OBJECTIVE) == OBJECTIVE and ('PB-T3 ' + PKG_VERSION) in OBJECTIVE
assert one(B62, 'AllowedWriteScope (11): ') == ALLOWED_PKG, 'el AllowedWriteScope del section 6.2 difiere'
assert ERASER in ALLOWED_PKG and SUPPORT in ALLOWED_PKG and PG in ALLOWED_PKG and all('.../' + os.path.basename(g) in ALLOWED_PKG for g in (UG, IG))
assert one(B62, 'UNIT_CHANGE: ') == 'UNIT_CHANGE: PP y UR (section 6.0; F4B-E25).', 'UNIT_CHANGE de PB-T3b'
assert one(B62, 'El esqueleto exacto del RED: ') == SKELETON, 'el esqueleto del RED del paquete difiere'
assert one(B62, 'El Skip de D-1, ') == SKIP_PKG, 'el Skip de D-1 del section 6.2 difiere'
assert one(B62, 'RequiredTests: ') == REQ_TABLE_PKG, 'la tabla de RequiredTests del section 6.2 cambio'
for txt in INV_OWN + [INV_15]:
    assert one(B62_2, txt.split('. ')[0] + '.') == txt, (txt.split('. ')[0] + ' del paquete difiere')
assert [b.split('. ')[0] for b in B62_2 if b.startswith('INV-PBT3B-')] == ['INV-PBT3B-%02d' % n for n in (1, 2, 3, 4, 5, 6, 15)]
for cid, txt in STOP_OWN_PKG:
    lab = C13_LABEL if cid == 'C13' else 'PBT3B-' + cid
    assert one(B62_2, lab + ': ') == lab + ': ' + txt, (lab + ' del paquete difiere')
assert [b.split(':')[0].split(' ')[0] for b in B62_2 if b.startswith('PBT3B-C')] == ['PBT3B-C04', 'PBT3B-C13', 'PBT3B-C14', 'PBT3B-C16', 'PBT3B-C17']
_ev0 = [i for i, b in enumerate(B62_2) if b.startswith('lo de section 6.1, ')]
assert len(_ev0) == 1 and B62_2[_ev0[0]:] == [EVID_FIRST_PKG] + EVID_OWN, ('la ExpectedEvidence del section 6.2', B62_2[_ev0[0]:])
assert one(B62, 'ExpectedEvidence: ') == 'ExpectedEvidence: ' + ' '.join([EVID_FIRST_PKG] + EVID_OWN), 'la ExpectedEvidence del section 6.2 difiere'
assert one(B62, 'Linea base de la CI') == CI_LINE_PKG, 'la linea base de la CI del section 6.2 difiere'
# las notas que no van al contrato del section 6.2: seis con la forma de V2-31 y dos con la de (A') con dos puntos (no se copian: no empiezan
# por un rotulo que se copia)
assert sum(1 for l in SEC62 if l.strip().startswith('- **Nota (no va al contrato).**')) == 6, 'las notas que no van al contrato del section 6.2'
assert sum(1 for l in SEC62 if l.strip().startswith('- **Nota (no va al contrato):')) == 1, 'la nota de PBT3B-C13 con (A\')'
# los comunes del section 6.0, que el contrato de PB-T3a ya lleva, comprobados contra el paquete y con el prefijo de PB-T3b
T3A_INV = {i.split('. ')[0]: i for i in pbt3a['Invariants']}
INV_COMMON = []
for n in ['%02d' % k for k in range(7, 15)]:
    _t = one(B60, n + '. ')
    assert T3A_INV['INV-PBT3A-' + n] == 'INV-PBT3A-' + _t, ('el invariante comun %s del paquete difiere del de PB-T3a' % n)
    INV_COMMON.append('INV-PBT3B-' + _t)
STOP_COMMON = {}
for cid in COMMON_STOPS:
    _lab = STOP_LABELS.get(cid, cid)
    _t = one(B60, _lab + ': ')[len(_lab) + 2:]
    assert T3A_STOP['PBT3A-' + cid] == _t, ('la parada comun %s del paquete difiere de la de PB-T3a' % cid)
    STOP_COMMON[cid] = _t
assert 'CT-PBT3A-04, CT-PBT3B-06 and 10' in STOP_COMMON['C07'] and 'Only two exceptions, both in PB-T3b' in STOP_COMMON['C02']
one(B60, 'PBT3x-C08 y PBT3x-C12, con el texto de [PKT1] section 6.0 y el prefijo cambiado: ')
one(B60, 'PBT3x-C11, con el sustituto de nc2 adaptado a F4B (F4B-E34; V43-09). ')
assert T3A_STOP['PBT3A-C11'] == C11_FOLD, 'el bloque citado de PBT3x-C11 del paquete difiere del PBT3A-C11 del contrato de PB-T3a'
# la regla RR del section 6.0 con su lectura para PB-T3b, y la tabla de RR-3 en sus filas de PB-T3b
_err = rr_pkg_check(SEC60, [c for _, c, _ in CLASS_CTS])
assert not _err, ('la regla RR del section 6.0 (PB-T3b)', _err)
_err, RR3_INFO = rr3_table_check(SEC60, [c for _, c, _ in CLASS_CTS], SEC53)
assert not _err, ('la tabla de RR-3 del section 6.0, filas de PB-T3b', _err)
RR3_DERIVED = [(c, w, len(g)) for c, (_, _, d) in RR3_INFO.items() for w, g in d]
RR3_SUMMARY = ('4 clases, una fila cada una; %s; derivadas de 5.3 (no enumeradas en la fila): %s'
               % ('; '.join('%s %d lineas y %d valores' % (c, len(RR3_INFO[c][0]), len(RR3_INFO[c][1])) for _, c, _ in CLASS_CTS),
                  '; '.join('%s: %s -> %d' % (c, w, n) for c, w, n in RR3_DERIVED) or 'ninguna'))
_s60 = md_fold(SEC60)
assert ('P-01 dice "' + P01 % ('K', SNAP_K) + '"') in _s60, 'P-01 del section 6.0 (la Snapshot K)'
assert 'El limite de D2, en cada contrato: PBT3x-C17, con el texto literal del Owner ([PKT1] section 6.0).' in _s60, 'C17 del section 6.0'
assert 'Los TaskId: de 24 a 26 caracteres, por MAX_PATH (R-T1-07).' in _s60 and 'StopConditions estandar (17), copiadas de [PKC2]' in _s60
assert 'PP y UR en PB-T3b' in one(B60, 'cada contrato declara UNIT_CHANGE '), 'UNIT_CHANGE de PB-T3b del section 6.0'
assert 'ui-tests en failure (PB-T3a) o en success (PB-T3b y PB-T3c)' in one(B60, 'ExpectedEvidence lo pide asi: ')
for cell in ('Worker anthropic/subagent/claude-sonnet-5-5, effort high;', 'Controller openai/process/gpt-6-luna, effort high;', 'vencen el 2026-12-29;'):
    one(B60, cell)
_tests_b = one(B60, 'en tests/: ')
assert ('en PB-T3b, ese prefijo no va: va la lista explicita de sus archivos menos los tres archivos de guardas de D-1 de la variante elegida, (A) '
        '(F4B-E29): 76 de 79 en 2f5d3dec y en d8ccbcb8 (V3-05; V41-03; R-15). Esos tres estan en su AllowedWriteScope') in _tests_b, \
    'el ForbiddenWriteScope de tests/ de PB-T3b del section 6.0'
assert 'las clases de PB-T1 y de las subtareas anteriores de PB-T3, tests/RackCad.Tests/Fixtures/I64/ y tests/RackCad.UI.Tests/Workspace/' in _tests_b
_d60 = one(B60, 'el diff de src (V42-01): ')
assert 'en PB-T3b y PB-T3c, frente al cierre de la subtarea anterior de PB-T3 (vacio);' in _d60, 'el anclaje de PB-T3b del section 6.0'
one(B60, 'para PB-T3b, la lista explicita de tests/RackCad.Tests/Workspace/ del ForbiddenWriteScope.')
one(B60, 'para PB-T3b, que la lista de D-1 de la variante elegida es la de R-6.')
one(B60, 'para PB-T3b, el censo de los disenos de las clases de F4 (precondicion 2);')
assert one(B60, 'que ninguna clase de los filtros de UI de la subtarea crea un RackFrameConfiguratorWindow real').endswith(
    'tambien sobre las clases de PB-T2 que entran en la red (V42-06);'), 'la contencion de F4B-E30 del section 6.0'
one(B60, 'las 17 entradas de la raiz.')
# 5.3: las clases con sus CT, el doble, la lista CT-PBT3B-01 a 15 (13 no falla en el RED), el motivo literal del Skip, E-1 a E-6 y lo esperado con (A)
_cls = [('Core', c, n) for b in B53 if b.startswith('en tests/RackCad.Tests/: ') for c, n in re.findall(r'(PushBack\w+Tests) \(([0-9, ya]+)(?:[:)])', b)]


def cts(s):
    out = []
    for part in re.split(r', | y ', s.strip()):
        a = re.fullmatch(r'(\d\d) a (\d\d)', part)
        out += list(range(int(a.group(1)), int(a.group(2)) + 1)) if a else [int(part)]
    return out


assert [(p, c, cts(n)) for p, c, n in _cls] == CLASS_CTS, ('las clases de 5.3', _cls)
one(B53, 'el doble con borrado, en ' + SUPPORT + ' (V2-06).')
assert 'seis [Fact], E-1 a E-6' in one(B53, 'en tests/RackCad.Tests/: '), 'CT-14: seis [Fact]'
_rows = [re.match(r'^\| \*\*(\d\d)\*\*', l).group(1) for l in SEC53 if re.match(r'^\| \*\*\d\d\*\*', l)]
assert _rows == ['%02d' % n for n in range(1, 16)], ('la lista CT-PBT3B-01 a 15 de 5.3', _rows)
assert [n for n in range(1, 16) if [l for l in SEC53 if l.startswith('| **%02d**' % n)][0].rstrip().endswith('**no falla en el RED** |')] == [13]
_eqs = [re.match(r'^\| (E-\d) \| (S-\d) \|', l).groups() for l in SEC53 if re.match(r'^\| E-\d \| S-\d \|', l)]
assert _eqs == [(e, s) for s, _, _, _, _, e in D1_METHODS], ('los equivalentes E-1 a E-6 de 5.3', _eqs)
_sk = [l for l in SEC53 if l.startswith('**El motivo literal del `Skip`**')]
assert len(_sk) == 1 and (chr(0xAB) + SKIP_REASON + chr(0xBB)) in _sk[0] and '`<E-n>` es el equivalente de cada m' in _sk[0], 'el motivo literal del Skip de 5.3'
assert one(B53, 'Lo esperado con (A): ') == ('Lo esperado con (A): S-1, S-3, S-4 y S-5: Failed; S-2: filas 1 y 3 con from-presente=False y Failed; filas 2, '
                                             '4 y 5 con from-presente=True y Passed en vacio (el texto real ya da violaciones, porque S-1 falla); S-6: '
                                             'vacia=True y Passed en vacio.'), 'lo esperado con (A) de la medicion sobre el GREEN'
# R-6 (la lista de D-1 de la variante elegida, section 6.0): S-1 a S-6 con sus clases y lineas en el registro custodiado
_meas = show(MEAS)
for _sid, _path, _line, _kind, _name, _eq in D1_METHODS:
    _short = {PG: 'PrepareGuard', UG: 'UpdateGuard', IG: 'IntegrationGuard'}[_path]
    assert re.search(r'\| %s \| `%s` \(%s:%d' % (_sid, _name, _short, _line), _meas), ('R-6 sin ' + _sid + ' en su linea', _name)
PKG_CHECKS = ["sin (A') en %d textos copiados" % len(_copied), 'Objective', 'AllowedWriteScope (11)', 'UNIT_CHANGE (PP y UR)', 'esqueleto del RED',
              'Skip de D-1', 'RequiredTests (tabla)', 'INV-PBT3B-01..06 y 15', 'paradas propias (C04, C13, C14, C16, C17)', 'ExpectedEvidence (6 vinetas)',
              'linea base de la CI', 'seis notas fuera (6.2) y la de (A\') con dos puntos', 'invariantes comunes 07..14 == PB-T3a',
              'paradas comunes == PB-T3a', 'C11 citado == PBT3A-C11', 'regla RR con la lectura de PB-T3b (RR-2, dos lecturas, cadena, tramo)',
              'tabla de RR-3, filas de PB-T3b', 'P-01 K', 'C17', 'TaskId 24..26', 'UNIT_CHANGE de 6.0', 'CI del RED de PB-T3b (ui-tests en success)', 'celdas',
              'ForbiddenWriteScope de tests/ (76 de 79)', 'anclaje frente al cierre de PB-T3a', 'F4B-E30', 'clases y CT de 5.3', 'E-1..E-6',
              'motivo literal del Skip', 'lo esperado con (A)', 'R-6 con S-1..S-6']
# el sha256 del revisado, al final, para que una copia mutada falle antes en su texto
assert hashlib.sha256(PKG_RAW).hexdigest() == CUSTODY_SHA256[PKG], ('el paquete leido no es el %s revisado (sha256)' % PKG_VERSION,
                                                                     hashlib.sha256(PKG_RAW).hexdigest())
PKG_CHECKS.append('sha256 del revisado')


# ---------------------------------------------------------------- PBT3B-C11: el bloque citado del paquete, con el prefijo de PB-T3b y los sustitutos de
# las decisiones del Owner posteriores a la v4.5: nc1 con la firma PD-F4B-01 y su sustituto (F4B-E40, revisado APTO y custodiado en el cierre de
# PB-T3a, F4B-E43), y nc2 con el sustituto ampliado, la v2 (F4B-E43: "de PB-T3b en adelante"). El texto del paquete nombra el sustituto de nc2 v1 y
# la firma exacta de PD-F4-04: el contrato se aparta de el en estos cambios, y solo en estos (DU de las notas: el texto del paquete no se cambia)
NC2_V2_TXT = NC2_V2_SHA or '<por rellenar: el SHA-256 de la v2 revisada>'
C11_SUBS = [
    ('-> substitute PBT3A-NC2-EXACT-WHITELIST-F4B-01, a copy of PBT2A-NC2-EXACT-WHITELIST-F4B-01 (F4B-E34: ' + SUBST_F4B + ', SHA-256 ' + SUBST_F4B_SHA
     + '; owner-pbt2a-controls-decisions.md, question 2) with ONLY the id and the scope changed, run in the F4B-E21 mode with the injection note and the '
       'raw output of this task\'s accepted planning, recorded as nc2 = SATISFIED_UNDER_PBT3A_EXACT_WHITELIST_SUBSTITUTE_F4B;',
     '-> substitute PBT3B-NC2-EXACT-WHITELIST-F4B-01, the version 2 of PBTx-NC2-EXACT-WHITELIST-F4B-01 (F4B-E43: ' + NC2_V2 + ', SHA-256 '
     + NC2_V2_TXT + '; owner-pbt3a-nc2-decision.md, "Aceptar y ampliar el sustituto"), whose property 8 admits only the closed list of the exact '
       'literals of the evidence entry of the byte-by-byte comparison, one per package (PB-T2 and PB-T3), with its reviewed text unchanged and run '
       'with --task PBT3B in the F4B-E21 mode with the injection note and the raw output of this task\'s accepted planning, recorded as nc2 = '
       'SATISFIED_UNDER_PBT3B_EXACT_WHITELIST_SUBSTITUTE_F4B;'),
    ('(2) nc1 with the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265: false CurrentSha correctly detected; Identity = fail; '
     'Handoff also fails first only because the handoff itself contains the false SHA; outcome BLOCKED / STOP; all other intended checks match; as '
     'applied in F4, f4-dispositions.md:1657-1660, Ci in fail only as a consumer of the false CurrentSha is part of the signature, with Remote in pass) '
     '-> substitute PBT3A-NC1-IDENTITY-SIGNAL-01, a copy of F4T3C2-NC1-IDENTITY-SIGNAL-01 with ONLY the id and the scope changed, recorded as nc1 = '
     'SATISFIED_UNDER_PBT3A_NC1_IDENTITY_SIGNAL_SUBSTITUTE.',
     '(2) nc1 with the signature PD-F4B-01, the false SHA cascade (F4B-E40: owner-pbt2c-nc1-decision.md, its conditions (a) to (f): the mutation only '
     'changes the CurrentSha of the handoff to a SHA that does not exist in the repository; EXECUTION_BLOCKED / STOP with S-04 among the stops; '
     'Handoff is the first check in fail; the failing checks are a subset of Handoff, Identity, Remote, Scope, Ci and Tests that always includes '
     'Handoff and Identity, and all the others pass as in the real verification; each failing evidence cites the false SHA and none attributes the '
     'failure to another cause; the Identity evidence also writes the real HEAD), of which the EXACT signature of PD-F4-04 (source '
     'owner-f4t3-authorization.md:255-265; as applied in F4, f4-dispositions.md:1657-1660) is a particular case -> substitute '
     'PBT3x-NC1-CASCADE-SIGNAL-01 (F4B-E40 and F4B-E43: ' + NC1_SUBST + ', SHA-256 ' + NC1_SUBST_SHA + ', reviewed APTO, with its declared limits), '
     'with its reviewed text unchanged and run with --task PBT3B, recorded as nc1 = SATISFIED_UNDER_PBT3B_NC1_CASCADE_SUBSTITUTE.'),
    ('the only admitted differences are those listed for each signature in section 6.0 of the PB-T1 contract package ([PT1], '
     'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md);',
     'the only admitted differences are those listed for each signature in section 6.0 of the PB-T1 contract package ([PT1], '
     'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md) and, for PD-F4B-01, its conditions (a) to (f);'),
    ('any nc1 that does not reproduce the literal signature - including the variation the Owner accepted only once in PB-TC (F4B-E08), in PB-T2a '
     '(F4B-E34) and in PB-T2b (F4B-E38), and the PB-TC2 case, where the substitute failed its property 5 and the Owner accepted nc1 only for PB-TC2 '
     'attempt 1 (F4B-E12) -,',
     'any nc1 that does not meet PD-F4B-01 - the one-off acceptances of nc1 in PB-TC (F4B-E08), in PB-TC2 attempt 1 (F4B-E12), in PB-T2a (F4B-E34), in '
     'PB-T2b (F4B-E38) and in PB-T2c (F4B-E40) do not extend -, an nc2 that the version 2 of the substitute does not give PASS - the disposition of '
     'the Owner on the nc2 of PB-T3a (F4B-E43) does not extend -,'),
    ('no one-off acceptance extends to PB-T3a', 'no one-off acceptance extends to PB-T3b'),
]
C11 = C11_FOLD
for _old, _new in C11_SUBS:
    assert C11.count(_old) == 1, ('PBT3B-C11: un cambio no aplica una sola vez sobre el bloque citado del paquete', _old[:70])
    C11 = C11.replace(_old, _new)
assert C11.endswith('no one-off acceptance extends to PB-T3b') and not re.search(r'PBT3A|PBT1[ABC]-NC|PBT2[BC]-NC|F4T1B-NC2|IDENTITY-SIGNAL', C11), \
    'PBT3B-C11: el final, los ids y ningun resto de PB-T3a'
assert C11.count('PB-T3a') == 1 and 'the nc2 of PB-T3a (F4B-E43) does not extend' in C11, 'PBT3B-C11: PB-T3a solo en la disposicion de su nc2, que no se extiende'
assert C11.count('SATISFIED_UNDER_PBT3B_EXACT_WHITELIST_SUBSTITUTE_F4B') == 1 and C11.count('SATISFIED_UNDER_PBT3B_NC1_CASCADE_SUBSTITUTE') == 1
assert ascii_fold(C11) == C11
# el sustituto de nc1 de PD-F4B-01, revisado, en la revision con su SHA-256 (custodiado en el cierre de PB-T3a); el de nc2 v2, cuando se custodie
assert hashlib.sha256(show_bytes(NC1_SUBST)).hexdigest() == NC1_SUBST_SHA, ('el sustituto de nc1 de PD-F4B-01 no esta en la revision con su SHA-256', NC1_SUBST)
assert in_head(PILOT + '/F4B-DRAFTS/nc1-cascade-substitute/limits-declared.md') and NC1_SUBST_SHA in show(PILOT + '/F4B-DRAFTS/nc1-cascade-substitute/limits-declared.md')
_dn2 = show(CO + 'owner-pbt3a-nc2-decision.md')
assert ('**Respuesta del Owner:** ' + chr(0xAB) + 'Aceptar y ampliar el sustituto (Recomendado)' + chr(0xBB)) in _dn2 and 'de PB-T3b en adelante' in _dn2, \
    'la decision del Owner sobre nc2 (F4B-E43): la respuesta literal y "de PB-T3b en adelante"'
assert NC2_V2_SHA is None or re.fullmatch(r'[0-9a-f]{64}', NC2_V2_SHA), NC2_V2_SHA
_v2_missing = [p for p in (NC2_V2, NC2_V2_REVIEW, NC2_V2_LIMITS) if not in_head(p)]
if NC2_V2_SHA and in_head(NC2_V2):  # en la revision, el script es el revisado byte a byte, y su revision y sus limites lo nombran por su SHA-256
    assert hashlib.sha256(show_bytes(NC2_V2)).hexdigest() == NC2_V2_SHA, ('el sustituto de nc2 v2 de la revision no es el revisado (sha256)', NC2_V2)
    for _p in (NC2_V2_REVIEW, NC2_V2_LIMITS):
        assert not in_head(_p) or NC2_V2_SHA.upper()[:8] in show(_p).upper(), ('la revision o los limites del sustituto de nc2 v2 no nombran su SHA-256', _p)
NC2_V2_OK = NC2_V2_SHA is not None and not _v2_missing
soft(NC2_V2_OK, 'emision: el sustituto de nc2 ampliado (la v2, F4B-E43, SHA-256 %s) aun no esta custodiado en la revision con su revision y sus limites: '
     'faltan %s' % ('por rellenar' if NC2_V2_SHA is None else NC2_V2_SHA[:8], ', '.join(_v2_missing) or 'nada'))

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


# ---------------------------------------------------------------- RequiredTests (section 6.2): los nuevos, Workspace de Core sin las filas de D-1, los dos
# de PB-T3a sin ExpectRed y "los demas de section 6.1, salvo los de PB-T3a con ExpectRed", con los minimos del contrato de PB-T3a
_t3a_req = [(r['Project'], r['Filter'], r['MinSelected'], r['ExpectRed']) for r in pbt3a['RequiredTests']]
assert len(_t3a_req) == 29, len(_t3a_req)


def fq(p, f):
    return ('FullyQualifiedName~%s.%s' % ('RackCad.UI.Tests' if p == 'UI' else 'RackCad.Tests', f), UIP if p == 'UI' else CP)


_t3a_by = {}
for proj, filt, m, er in _t3a_req:
    _t3a_by[(proj, filt)] = (m, er)
_ws_core = _t3a_by[(CP, fq('Core', 'Workspace')[0])]
assert _ws_core == (3271, False), _ws_core
_er_t3a = [(proj, filt) for proj, filt, m, er in _t3a_req if er]
assert _er_t3a == [(CP, fq('Core', 'PushBackFullDraft')[0]), (UIP, fq('UI', 'PushBackFullDraftWindowParityTests')[0])], _er_t3a
# (proyecto, prefijo, minimo del paquete o None, ExpectRed, clase): 'new' (no existe), 'ws' (Workspace de Core, medido menos las filas de D-1),
# 't3a' (el minimo del contrato de PB-T3a, medido sin bajar)
REQ_SPEC = [('Core', 'PushBackFullWrite', 13, True, 'new'), ('Core', 'PushBackPilotUpdateEquivalenceGuardTests', 6, True, 'new'),
            ('Core', 'PushBackWritePathGuardTests', 2, False, 'new'), ('Core', 'Workspace', None, False, 'ws'),
            ('Core', 'PushBackFullDraft', 10, False, 't3a'), ('Core', 'PushBackDraftWriteGuardTests', 3, False, 't3a')]
for proj, filt, m, er in _t3a_req:
    p = 'UI' if proj == UIP else 'Core'
    _ns = 'FullyQualifiedName~' + ('RackCad.UI.Tests.' if p == 'UI' else 'RackCad.Tests.')
    assert filt.startswith(_ns), filt
    f = filt[len(_ns):]
    if er or (p, f) in [(x[0], x[1]) for x in REQ_SPEC]:
        continue
    REQ_SPEC.append((p, f, m, False, 't3a'))
assert len(REQ_SPEC) == 31 and [r for r in REQ_SPEC if r[3]] == REQ_SPEC[:2] and [r for r in REQ_SPEC if r[4] == 'new'] == REQ_SPEC[:3]
# los minimos de los filtros nuevos son sus CT (5.3): 13 = CT-01 a 12 y 15 (las dos clases de PushBackFullWrite), 6 = E-1 a E-6, 2 = CT-13
assert [m for _, _, m, _, k in REQ_SPEC if k == 'new'] == [13, 6, 2]
assert sum(len(cts) for _, c, cts in CLASS_CTS if c.startswith('PushBackFullWrite')) == 13
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
    else:
        CORE_TOTALS = totals
# F4B-E30 (la contencion; section 6.0): ninguna clase de los filtros de UI de PB-T3b crea un RackFrameConfiguratorWindow real
_cfg = sorted(p for p in UI_FILES if re.search(r'new\s+RackFrameConfiguratorWindow\s*\(', re.sub(r'//[^\n]*', '', re.sub(r'"(?:\\.|[^"\\\n])*"', '""', UI_TEXTS[p]))))
assert not _cfg, ('F4B-E30: un filtro de UI de PB-T3b crea el configurador real', _cfg)
REQ, REQ_REPORT = [], []
for p, f, pkg_min, er, kind in REQ_SPEC:
    filt, proj = fq(p, f)
    classes, run, skip, member = COUNTS[(p, f)]
    note = None
    if kind == 'new':
        assert classes == 0 and run == 0, ('la clase nueva ya existe', f)
        mins = pkg_min
    elif kind == 'ws':
        # el paquete: "el numero de Passed de la CI del RED (V2-26)". En la emision no hay RED: el minimo es lo medido menos las diez filas que el
        # RED salta (cinco Fact y las cinco filas de la Theory S-2), una cota inferior que vale cuente la TRX la Theory como una fila o como cinco
        # (R-11); la CI del RED lo confirma (DU de las notas)
        assert run == _ws_core[0] and skip == 0, ('Workspace de Core en la base', run, skip, _ws_core)
        mins = run - D1_ROWS
        note = '     Workspace de Core: medido %d (el del contrato de PB-T3a) - %d filas de D-1 = %d (la cota de los Passed del RED; DU)' % (run, D1_ROWS, mins)
    else:
        prev, prev_er = _t3a_by[(proj, filt)]
        assert prev == pkg_min, (f, prev, pkg_min)
        if run < pkg_min:
            # una Theory con MemberData no se cuenta en estatico: vale porque el contrato de PB-T3a fija el mismo filtro con ese minimo (medido por la CI)
            assert member > 0, ('el recuento medido de %s %s (%d) es menor que el minimo del contrato de PB-T3a (%d)' % (p, f, run, pkg_min))
            note = '     estatico %d (+%d Theory con MemberData) < %d: vale porque el contrato de PB-T3a fija el mismo filtro con %d' % (run, member, pkg_min, pkg_min)
        mins = pkg_min
    REQ.append((proj, filt, mins, er))
    REQ_REPORT.append('%-4s %-41s %-4s paquete=%-5s medido: clases=%d ejecutables=%d skip=%d memberdata=%d -> MinSelected=%d ExpectRed=%s'
                      % (p, f, kind, pkg_min if pkg_min is not None else '-', classes, run, skip, member, mins, er))
    if note:
        REQ_REPORT.append(note)
for proj, _, _, _ in REQ:
    assert in_head(proj), proj
# ningun filtro heredado selecciona una clase nueva de PB-T3b, y cada filtro nuevo solo las suyas
NEW_CLASSES = [c for _, c, _ in CLASS_CTS] + ['PushBackFullWriteSupport']
for p, f, _, _, k in REQ_SPEC:
    news = [c for c in NEW_CLASSES if p == 'Core' and c.startswith(f)]
    assert (k == 'new') == bool(news), ('un filtro y las clases nuevas', p, f, news)
# los filtros heredados no bajan su MinSelected, salvo Workspace de Core por los Skip de D-1
for proj, filt, mins, er in REQ:
    if (proj, filt) in _t3a_by and filt != fq('Core', 'Workspace')[0]:
        assert _t3a_by[(proj, filt)][0] <= mins, (filt, _t3a_by[(proj, filt)], mins)

cells = json.loads(json.dumps(pbt3a['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna'] and all('high' in c['TestedEfforts'] for c in cells)

# ---------------------------------------------------------------- invariantes: los propios (01 a 06), los comunes (07 a 14) y el 15, literales
INV = INV_OWN + INV_COMMON + [INV_15]
assert [i.split('. ')[0] for i in INV] == ['INV-PBT3B-%02d' % n for n in range(1, 16)] and len(INV) == 15
assert INV[12].startswith('INV-PBT3B-13. Las declaraciones nuevas en src/') and 'PBT3B-C13' in INV[12]

# ---------------------------------------------------------------- la evidencia: la del section 6.1 con lo de PB-T3b, las propias, la linea base de la CI
_t3a_ev = list(pbt3a['ExpectedEvidence'])
assert len(_t3a_ev) == 10 and _t3a_ev[-1].startswith('Linea base de la CI: la del cierre de PB-T2c'), 'la ExpectedEvidence del contrato de PB-T3a'
EVID_61 = []
for k, e in enumerate(_t3a_ev[:9]):
    if k in EVID_T3A_SUBS:
        _a, _b = EVID_T3A_SUBS[k]
        assert e.count(_a) == 1, ('la vineta %d de la ExpectedEvidence de PB-T3a sin su texto de PB-T3a' % k, _a[:60])
        e = e.replace(_a, _b)
    EVID_61.append(e)
# el sustituto de nc2 v2 compara la primera entrada con su lista cerrada de literales (PB-T3, la de 485b5651): tiene que ser la de PB-T3a, byte a byte
assert EVID_61[0] == pbt3a['ExpectedEvidence'][0] == json.loads(show(PBT3AC, PT3A_REISSUE))['ExpectedEvidence'][0], 'la primera evidencia no es la de PB-T3a'
assert 'las autopruebas de las tres guardas' in EVID_61[3]  # literal de section 6.1 (DU de las notas: en PB-T3b son las de CT-13 y CT-14)
_core_new = sum(m for p, _, m, _, k in REQ_SPEC if k == 'new' and p == 'Core')
assert _core_new == 21 and CI_FORECAST_T3A[0] + _core_new == 19128 and CI_FORECAST_T3A[1] == 2451
_run, _sha, _core, _ui, _skip = CI_BASE_T3A
CI_TXT = ('Linea base de la CI: la del cierre de PB-T3a (run %s sobre %s: Core %d; UI %d resultados, %d Skipped y %d Passed). GREEN previsto, con la '
          'variante (A): Core + %d (13 + 6 + 2) = %d resultados, con los seis metodos de D-1 en Skipped (+ 6 o + 10 Skipped, segun cuente la TRX las '
          'filas de la Theory S-2; R-11) y 0 Failed; UI sin cambio: %d resultados, %d Skipped y %d Passed; o la diferencia explicada por el diff.'
          % (_run, _sha, _core, _ui, _skip, _ui - _skip, _core_new, _core + _core_new, _ui, _skip, _ui - _skip))
EVID = EVID_61 + EVID_OWN + [CI_TXT]
assert len(EVID) == 15

# ---------------------------------------------------------------- paradas: las 17 estandar, las comunes del section 6.0 y las propias del section 6.2
STOP_TEXT = dict(STOP_COMMON)
for cid in ('C08', 'C12'):  # las de [DP], con el texto de [PKT1] section 6.0 que lleva el contrato de PB-T3a, sin prefijo dentro
    _t = T3A_STOP['PBT3A-' + cid]
    assert 'PBT3A' not in _t and 'PB-T3a' not in _t, cid
    STOP_TEXT[cid] = _t
STOP_TEXT['C11'] = C11
for cid, txt in STOP_OWN_PKG:
    if cid == 'C04':  # "as PBT3A-C04": el texto de PBT3A-C04 del contrato de PB-T3a
        STOP_TEXT[cid] = T3A_STOP['PBT3A-C04']
    elif cid == 'C17':  # "the D2 limit": el texto literal del Owner que lleva PBT3A-C17 ([PKT1] section 6.0)
        STOP_TEXT[cid] = T3A_STOP['PBT3A-C17']
    else:
        STOP_TEXT[cid] = txt
assert STOP_TEXT['C04'].startswith('a method of a filter with ExpectRed passes on the skeleton') and 'If later PB-T1/PB-T2/PB-T3 needs to mutate' in STOP_TEXT['C17']
_od2 = ' '.join(show(CO + 'owner-d1-d2-decisions.md').split())
_quote = STOP_TEXT['C17'][STOP_TEXT['C17'].index('"') + 1:STOP_TEXT['C17'].rindex('"')]
assert all(' '.join(part.split()) in _od2 for part in re.split(r'(?<=[.:;]) ', _quote) if part.strip()), 'la cita de D2 de C17 no esta en F4B-E11'
STOP_IDS = ['C01', 'C02', 'C03', 'C04', 'C05', 'C06', 'C07', 'C08', 'C09', 'C10', 'C11', 'C12', 'C13', 'C14', 'C16', 'C17', 'C18']
assert sorted(STOP_TEXT) == sorted(STOP_IDS) and len(STOP_IDS) == 17
STOPS = P_STOPS + [{'Id': 'PBT3B-' + cid, 'Text': STOP_TEXT[cid]} for cid in STOP_IDS]
assert len(STOPS) == 34 and len({s['Id'] for s in STOPS}) == 34
for s in STOPS[17:]:  # ninguna parada de PB-T1 o de PB-T3a copiada sin adaptar (C04 dice el texto, no "as PBT3A-C04")
    assert not re.search(r'PBT1[ABC]-(C\d|NC)|SUS3[ABC]|(?<!CT-)PBT3A-|as PBT3A', s['Text']), s['Id']


# ---------------------------------------------------------------- autoridades
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
BM = PILOT + '/F4B-PBT0-CHARACTERIZATION/block-m/block-m-analysis.md'
PKT1 = 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md'
PKG2 = 'docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md'
REVIEW = D3 + 'independent-review-v1.md'
CAT3A = D3 + 'independent-review-pbt3a-v43-check.md'
SUPERSEDING = PILOT + '/F4B-DRAFTS/superseding-characterization.md'
NC1_DIR = PILOT + '/F4B-DRAFTS/nc1-cascade-substitute/'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
_disp = show(DISP).replace('\r\n', '\n')
_disp_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
assert _disp_ids == list(range(1, len(_disp_ids) + 1)) and _disp_ids[-1] >= 43, ('f4b-dispositions.md', _disp_ids[-3:])
DISP_LAST = 'F4B-E%02d' % _disp_ids[-1]
for _e, _needle in (('F4B-E24', 'owner-f4b-dpb07-extensiondata-decisions.md'), ('F4B-E25', 'owner-f4b-correction-unit-change-decision.md'),
                    (D1_ENTRY, 'owner-pbt3-d1-decision.md'), ('F4B-E30', 'PBT1C-C07'), ('F4B-E34', 'owner-pbt2a-controls-decisions.md'),
                    (ISSUE_AUTH[0], 'owner-f4b-pbt3-emission-red-repair-decisions.md'), ('F4B-E37', 'Snapshot K'), ('F4B-E40', 'owner-pbt2c-nc1-decision.md'),
                    (PT3A_EMIT_ENTRY, PT3A_TASK), (PT3A_STOP_ENTRY, 'owner-pbt3a-g9-decision.md'), (PT3A_CLOSE_ENTRY, 'owner-pbt3a-nc2-decision.md')):
    _i = _disp.index('\n## %s. ' % _e)
    assert _needle in _disp[_i:_disp.find('\n## F4B-E', _i + 1) if _disp.find('\n## F4B-E', _i + 1) > 0 else len(_disp)], (_e, _needle)
# la emision de PB-T3b (F4B-E44): la entrega el commit X; hasta entonces, AVISO
E44_OK = bool(re.search(r'(?m)^## %s\. Emisi\S+n de %s' % (EMIT_ENTRY, TASK), _disp))
soft(E44_OK, 'emision: %s (la emision de %s) aun no esta en f4b-dispositions.md' % (EMIT_ENTRY, TASK))
assert DISP_LAST in (PT3A_CLOSE_ENTRY, EMIT_ENTRY), ('f4b-dispositions.md termina en una entrada que no es el cierre de PB-T3a ni la emision de PB-T3b', DISP_LAST)
# F4B-E35: la pregunta 3 (la emision condicional de PB-T3, que cubre PB-T3b tras el cierre de PB-T3a) y la pregunta 4 (la regla RR)
_e35 = show(ISSUE_AUTH[1])
_err = rr_check(_e35) + q3_check(_e35)
assert not _err, ('F4B-E35', _err)
_d1 = show(CO + 'owner-pbt3-d1-decision.md')
assert 'Un solo c' in _d1 and 'docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md' in _d1, 'F4B-E29: la opcion (A) y el paquete de PB-T3'
# la AuthorityRevision contiene F4B-E29 (precondicion 4) y el cierre de PB-T3a: si esta en el almacen; si no (un seco con otra), AVISO
if soft(git('cat-file', '-e', AR + '^{commit}').returncode == 0, 'emision: la AuthorityRevision %s no esta en el almacen de objetos (seco): no se '
                                                                  'comprueba que contenga F4B-E29 y el cierre de PB-T3a' % AR[:8]):
    assert git('merge-base', '--is-ancestor', D1_REV, AR).returncode == 0 and git('merge-base', '--is-ancestor', PT3A_CLOSE, AR).returncode == 0, \
        'la AuthorityRevision no contiene F4B-E29 (precondicion 4) o el cierre de PB-T3a'
F4T4 = sorted(p for p in tracked('docs/initiatives/') if re.fullmatch(r'docs/initiatives/I-64-f4-t4[a-z0-9-]*-gate-contract-package\.md', p))
assert len(F4T4) == 6, F4T4
UNIT_CHANGE = [(PP, 'el plan de la actualizacion del piloto: PB-T3b lo generaliza en su sitio (D-PBT3-09; section 6.0 del paquete, F4B-E25)'),
               (UR, 'la corrida de la actualizacion del piloto: PB-T3b la generaliza en su sitio (D-PBT3-10; section 6.0 del paquete, F4B-E25)')]
EXISTING = [
    (PKG, 'archivo completo (' + PKG_VERSION + ', sha256 ' + CUSTODY_SHA256[PKG][:8] + '): section 3 (D-PBT3-09, el plan generalizado, y D-PBT3-10, la '
          'corrida generalizada y ComparePost, con la variante (A); D-PBT3-12 a D-PBT3-14), section 4 (las guardas, con S-1 a S-6) y 4.1 (el barrido de '
          'colisiones), section 5.0 (las reglas, el momento de cada lectura, los gestos que no cambian nada, la comprobacion de los archivos nuevos y '
          'tocados frente a las guardas y el doble con borrado) y 5.3 (CT-PBT3B-01 a 15, los equivalentes E-1 a E-6, el motivo literal del Skip y la '
          'medicion sobre el GREEN), section 6.0 (lo comun, con la regla RR y su lectura en PB-T3b, la tabla cerrada de RR-3 y el bloque citado de '
          'PBT3x-C11) y 6.2 (el contrato de PB-T3b, con la variante elegida), section 7, section 8 (D-1, decidida en F4B-E29) y section 10'),
    (PHASE, 'paquete de la fase F4B v3.1 ([P]): la fila de PB-T3 del section 5 (:505); H-PB-16 a H-PB-23; OB-04.1 y OB-04.2; OB-05.1 a OB-05.5; '
            'section 6.4; OB-10.1; section 7.3 y 7.4; OB-T1B.1 a OB-T1B.3; R-PB-03, R-PB-05, R-PB-06, R-PB-09 y R-PB-11'),
    (CO + 'owner-f4b-disposition.md', 'archivo completo ([OD]): la fila de PB-T3; D-PB-04 (borrar si); D-PB-05; D-PB-10 (1/3; el intento 3 vuelve al '
                                      'Owner); el bloque sobre T1B ("create a declared superseding characterization/change record"), que D-1 aplica'),
    (BM, 'el bloque M de PB-T0: H-PB-16 confirmado; lo que deja para PB-T3 (:133-139), con M-i (BM:62-66), que CT-PBT3B-04 generaliza'),
    (PKT1, 'el paquete de PB-T1 v2.8 custodiado ([PKT1]): D-PBT1-15 (D2); section 5.0; section 6.0 (C08, C12 y C17); y la forma de PBT1C-C13 por nombre '
           '(:1338), acotada a los archivos que se tocan y con las sobrecargas, que aplica PBT3B-C13'),
    (CO + 'owner-d1-d2-decisions.md', 'archivo completo ([OD2], F4B-E11): D2 con sus diez condiciones y su limite, el texto literal de PBT3B-C17'),
    (CO + 'owner-f4b-procedure-decisions.md', 'archivo completo ([DP], F4B-E03): preguntas 1 y 2 (firmas exactas de los sustitutos y la replanificacion unica)'),
    (CO + 'owner-pbt1a-nc1-decisions.md', 'archivo completo (F4B-E15): las tres comprobaciones deterministas de la verificacion'),
    (CO + 'owner-pbtc-nc1-decision.md', 'la disposicion unica de nc1 en PB-TC (F4B-E08), que no se extiende a PB-T3b'),
    (CO + 'owner-pbtc2-nc1-decisions.md', 'la disposicion de nc1 en PB-TC2 solo para su intento 1 (F4B-E12), que no se extiende a PB-T3b'),
    (SNAP_K_DEC, 'archivo completo (F4B-E37): "Sonda neutra, luego K (Recomendado)": la Snapshot K (' + SNAP_K + ', ConfigSha256 ' + SNAP_K_SHA[:8] +
                 ', sonda neutra ' + SNAP_K_PROBE + '), la que cita P-01'),
    (CO + 'owner-f4b-opaque-collections-decision.md', 'archivo completo (F4B-E21): la sesion pone byte a byte en la delegacion las ocho colecciones '
                                                      'opacas del contrato; la comparacion byte a byte, A1-A8 y nc4 se hacen sobre la delegacion final'),
    (CO + 'owner-pbt1b-rework-decisions.md', 'archivo completo (F4B-E22): M10 y la mitad (a) de M14, ratificadas solo para PB-T1b; este contrato lleva el '
                                             'texto correcto (INV-PBT3B-13, PBT3B-C13 y la CI del RED con build-plugin omitido)'),
    (CO + 'owner-f4b-dpb07-extensiondata-decisions.md', 'pregunta 2 (F4B-E24): la ExtensionData anidada, "Basta asi (Recomendado)": el panel mantiene el '
                                                        'rechazo de AUTH-13, que CT-PBT3B-10 fija en la escritura (D-PBT3-13)'),
    (F4B_E25, 'archivo completo (F4B-E25): la regla de F4B sobre las rutas UNIT_CHANGE en una correccion, con sus tres condiciones; las EXTERNAL, sin '
              'excepcion. PB-T3b declara UNIT_CHANGE PP y UR (section 6.0 del paquete)'),
    (F4B_E25_RET, 'la vuelta al Owner por la parada de la primera verificacion del intento 2 de PB-T1b (16.3), respondida por F4B-E25'),
    (CO + 'owner-pbt3-d1-decision.md', 'archivo completo (F4B-E29): D-1 del paquete (section 8.2), "Un solo codigo (Recomendado)", la variante (A): PB-T3b '
                                       'sustituye S-1 a S-6 con su registro y sus equivalentes E-1 a E-6 (precondicion 4 del paquete)'),
    (ISSUE_AUTH[1], 'archivo completo (F4B-E35): pregunta 3, "Si, con esa condicion (Recomendado)": la emision condicional de PB-T3a, PB-T3b y PB-T3c, en '
                    'ese orden y cada una tras el cierre de la anterior, si su comprobacion acotada da 0 bloqueantes y 0 importantes sin resolver y la '
                    'remedicion no cambia nada material; vuelve al Owner por una decision nueva, un control con una firma no prevista, un intento 3 o '
                    'un efecto material sin medir (precondicion 10). Pregunta 4, "Si, con esas condiciones (Recomendado)": la regla de la reparacion del '
                    'RED, RR-1 a RR-7, que el contrato incorpora en PBT3B-C07 (la excepcion), PBT3B-C18, INV-PBT3B-02 y la ExpectedEvidence; en PB-T3b, '
                    'el tramo RedSha..RedRepairSha no toca PP, UR, la interfaz nueva ni los tres archivos de guardas de F4 con los Skip de D-1 (RR-2)'),
    (CO + 'owner-pbt2a-controls-decisions.md', 'pregunta 2 (F4B-E34): el sustituto de nc2 adaptado a F4B (v1), que el paquete nombra en PBT3x-C11 y que '
                                               'F4B-E43 amplia para PB-T3b en adelante'),
    (CO + 'owner-pbt2c-nc1-decision.md', 'archivo completo (F4B-E40): "Aceptar y ampliar la firma (Recomendado)": la firma nueva de nc1 PD-F4B-01, la '
                                         'cascada del SHA falso, condiciones (a) a (f), para el resto de F4B, con PD-F4-04 como caso particular; su '
                                         'sustituto PBT3x-NC1-CASCADE-SIGNAL-01, el de PBT3B-C11'),
    (NC1_SUBST, 'el sustituto de nc1 de PD-F4B-01 (F4B-E40), SHA-256 ' + NC1_SUBST_SHA + ', revisado (APTO en su tercera ronda) y custodiado en el cierre '
                'de PB-T3a (F4B-E43); PBT3B-C11 lo usa con --task PBT3B, sin editar su texto'),
    (NC1_DIR + 'limits-declared.md', 'la version revisada del sustituto de nc1 y sus limites declarados (M-17, M-18, I-6, M-3 y M-4)'),
    (NC1_DIR + 'independent-review-nc1-cascade.md', 'la revision independiente del sustituto de nc1, en tres rondas: APTO, 0/0/2'),
    (CO + 'owner-pbt3a-nc2-decision.md', 'archivo completo (F4B-E43): "Aceptar y ampliar el sustituto (Recomendado)": nc2 de PB-T3a por disposicion del '
                                         'Owner, solo para PB-T3a; de PB-T3b en adelante, el sustituto de nc2 admite una lista cerrada de los literales '
                                         'exactos de cada paquete, con autoprueba y revision independiente antes de usarlo (PBT3B-C11)'),
    (CO + 'owner-pbt3a-g9-decision.md', 'archivo completo (F4B-E42): "Renombrar ForWrite (Recomendado)": ForFullWrite, el nombre que llama BuildFull en PP '
                                        '(D-PBT3-09; V45-01), y la leccion del barrido de las guardas (section 5.0 y 4.1)'),
    (DISP, 'archivo completo: F4B-E01 a ' + DISP_LAST),
    (PBT3AC, 'el contrato reemitido de PB-T3a (F4B-E42; AuthorityRevision ' + PT3A_REISSUE_X[:8] + '): las listas de tests/ con las clases de PB-T1, PB-T2 '
             'y PB-T3a, los filtros y sus MinSelected, las celdas, las paradas estandar y de [DP], los textos comunes del section 6.0 (07 a 14, C01 a C18 '
             'y C11 citado), INV-PBT3A-02 (el criterio del RED que cita INV-PBT3B-02), PBT3A-C04 (el texto de PBT3B-C04) y la ExpectedEvidence del '
             'section 6.1, que este contrato lleva con lo de PB-T3b'),
    (PT3A_TCP, 'el cierre de PB-T3a (' + PT3A_CLOSE_ENTRY + '): CLOSED / PASS WITH LIMITATIONS sobre ' + GREEN_T3A[:8] + ', attempts 2 de 3; el modelo del '
               'borrador completo en Draft/, que PB-T3b conecta a la escritura sin conectarlo al panel; su CI del GREEN ' + CI_BASE_T3A[0] + ', la linea '
               'base de este contrato'),
    (PILOT + '/' + PT3A_TASK + '/1/' + 'R20261120T001738Z-7d92' + '/analysis.md', 'el analisis de la sesion de la parada de PB-T3a (F4B-E42): el choque '
                                                                                  'latente de ForWrite en PP, en la llamada de BuildFull, que la v4.5 '
                                                                                  'resuelve con ForFullWrite'),
    (MEAS, 'el registro de mediciones del paquete (R-0 a R-20): R-6 (las guardas de F4, S-1 a S-6), R-11, R-15 (tests/RackCad.Tests/Workspace/, 79 y '
           '76), R-18 (las declaraciones de PP y UR) y R-20 (la lectura de G-9 corregida); y la remedicion de PB-T3b sobre el cierre de PB-T3a (R-21)'),
    (REVIEW, 'la revision independiente adversarial del paquete y sus recomprobaciones (section 1 a 13), aplicadas en la v2 a la v4.2'),
    (CAT3A, 'la comprobacion acotada de PB-T3a (CA-T3a), con su recomprobacion de la v4.4 (section 10) y la de la v4.5 (section 11): lo que vale tambien '
            'para PB-T3b del paquete (I-01: CT-PBT3B-01 sin el gesto inerte de BeamDepth)'),
    (D3 + 'compat-pbt3-vs-pbt2.md', 'la revision de compatibilidad con PB-T2, sobre la v4.1: el ForbiddenWriteScope de PB-T3b (98 en src/; V42-05)'),
    (PKG2, 'el paquete de PB-T2 ([P2]): el orden de la fase; la regla RR de sus v2.7 y v2.8 y el arrastre A2-11 (CT-PBT3B-01 (ii))'),
    ('docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md', 'el paquete de PB-TC ([PKC-C]): el algoritmo del ForbiddenWriteScope; CT-PBTC-02 (i) y '
                                                                'CT-PBTC-05, los oraculos de CT-PBT3B-01 (ii)'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 'el paquete de PB-TC2 ([PKC2]): las 17 paradas estandar y la raiz (section 6.7, '
                                                                 ':579-581) y las celdas (section 6.8, :655)'),
    (PILOT + '/F4-DRAFTS/gate-close.md', 'el cierre de F4: el PREPARE, MUTATE y POST del piloto que PB-T3b generaliza; S2-03 (un paso de Deshacer); las '
                                         'guardas G-1 a G-10'),
] + [(p, 'paquete de F4-T4: el plan, la corrida, el puerto y el escritor del piloto y sus guardas (S-1 a S-6 entre ellas)') for p in F4T4] + [
    ('docs/initiatives/I-64-proposal-v5.md', 'la Proposal V5 congelada de I-64: D-10, D-13, D-15 (punto 5) y D-16'),
    ('docs/initiatives/I-64-f4-t2-gate-contract-package.md', 'el regimen del RED con esqueleto inerte, que compila, no lanza y falla por asercion (:282)'),
    ('docs/initiatives/I-64-f4-t3-gate-contract-package.md', 'INV-F4T3A-13 (:371-374): las guardas pueden pasar en el RED y discriminan con autopruebas '
                                                             '(CT-PBT3B-13)'),
    (PILOT + '/F4-DRAFTS/f4-dispositions.md', 'las fuentes de PD-F4-01 (:1103-1106, :1540, :520, :562) y PD-F4-04 (:1657-1660)'),
    (PILOT + '/F4-DRAFTS/coordinator/owner-f4t3-authorization.md', 'la firma literal de PD-F4-04 (:255-265), caso particular de PD-F4B-01'),
    ('docs/automation/state/I-64.yml', 'estado vigente completo'),
    ('docs/automation/evidence/I-64-evidence.md', 'las secciones de F4B (231 y siguientes)'),
]
# las de la emision de PB-T3b (el commit X; el patron de PB-T3a): las de CUSTODY son autoridades
BOUNDED_CHECK = D3 + 'independent-review-pbt3b-check.md'  # el nombre que propone el generador (DU de las notas)
CUSTODY = [
    (D3 + 'pbt3b-anchors-out.txt', 'la salida de la remedicion de PB-T3b sobre el cierre de PB-T3a (precondicion 2 y section 6.0 del paquete): las anclas '
                                   'de PP, UR y de las guardas, S-1 a S-6, la lista de Workspace/, los recuentos, la linea base de la CI, la Snapshot '
                                   'vigente, el censo de los disenos de las clases de F4 y el barrido de las guardas sobre el cierre (section 5.0 y 4.1)'),
    (BOUNDED_CHECK, 'la comprobacion acotada de PB-T3b (CA-T3b), sobre el contrato en seco y la v4.5 (precondicion 3 y section 6.0 del paquete)'),
    (SUPERSEDING, 'el registro declarado de las sustituciones: su seccion de PB-T3b, con S-1 a S-6, sus equivalentes E-1 a E-6, la autoridad ([OD] fila de '
                  'PB-T3, D-PB-04 y F4B-E29) y el motivo, que nombra el motivo literal del Skip (section 5.3 y 8.2 del paquete)'),
]
CUSTODY_HEADS[SUPERSEDING] = ['## PB-T3b ']
CUSTODY_EXTRA = [D3 + n for n in ('independent-review-pbt3b-check-prompt.md', 'make_pbt3b_contract-dry.txt', 'measure_pbt3b.py', 'guard_sweep.py',
                                  'sweep_t3b_diff.py', 'sweep-pbt3b-close.txt')]
assert len(CUSTODY) == 3 and len(CUSTODY_EXTRA) == 6 == len(set(CUSTODY_EXTRA))
CUSTODY_FREE = [D3 + 'pbt3b-anchors-out.txt', BOUNDED_CHECK] + CUSTODY_EXTRA
# el sustituto de nc2 v2 (F4B-E43), autoridad de PBT3B-C11 cuando se custodie revisado (NC2_V2_SHA)
PENDING_AUTH = [(NC2_V2, 'el sustituto de nc2 ampliado (F4B-E43), la v2 de PBTx-NC2-EXACT-WHITELIST-F4B-01, SHA-256 ' + NC2_V2_TXT + ', con su '
                         'autoprueba y su revision independiente: el de PBT3B-C11, con --task PBT3B y su lista cerrada de literales (PB-T2 y PB-T3)'),
                (NC2_V2_LIMITS, 'la version revisada del sustituto de nc2 v2 (SHA-256 ' + NC2_V2_TXT + ') y sus limites declarados (M-V1 a M-V4), que no '
                                'se corrigen'),
                (NC2_V2_REVIEW, 'la revision independiente del sustituto de nc2 (la del v1, con su recomprobacion) y su seccion "Recomprobacion v2", '
                                'sobre la v2: APTO, B 0 / I 0 / M 4')]
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D12 y D13 (sin modelo paralelo; la caracterizacion no se edita para que pase)'),
    ('docs/AUTOMATION_PLAN.md', 'section 16.3, 16.5, 16.8, 16.9, 16.10 y 16.11'),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4)'),
    ('docs/automation/agent-execution/routing.md', 'archivo completo'),
    ('docs/automation/agent-execution/model-catalog.md', 'celdas medidas y vigentes para Controller y Worker'),
]
_auth_all = CUSTODY + EXISTING + PENDING_AUTH + UNIT_CHANGE + EXTERNAL
assert len({p for p, _ in _auth_all}) == len(_auth_all), 'una autoridad repetida'
for p, _ in EXISTING + EXTERNAL + UNIT_CHANGE:
    assert in_head(p), ('autoridad inexistente en la revision', p)
assert MEAS in [p for p, _ in EXISTING] and SUPERSEDING in [p for p, _ in CUSTODY]
c = {
    'Schema': 'rackcad-gate-contract/v1', 'Unit': 'I-64', 'Gate': 'F4B', 'TaskId': TASK,
    'Objective': OBJECTIVE,
    'AuthorityRevision': AR, 'MainSha': MAIN,
    'Authorities': [{'Path': p, 'Section': s, 'Class': 'UNIT_DOC'} for p, s in CUSTODY + EXISTING + PENDING_AUTH] + [
        {'Path': CONTRACT_REL, 'Section': 'este contrato', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_CHANGE'} for p, s in UNIT_CHANGE]
      + [{'Path': p, 'Section': s, 'Class': 'EXTERNAL'} for p, s in EXTERNAL],
    'AllowedWriteScope': ALLOWED,
    'ForbiddenWriteScope': FORBIDDEN,
    'Invariants': INV,
    'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
    'ExpectedEvidence': EVID, 'StopConditions': STOPS, 'EligibleCells': cells, 'RoutingEnforcement': 'required', 'CorrectionsAuthorized': True,
    'IssuedBy': ('Sesion principal de I-64 (Claude), emision de la segunda subtarea de PB-T3 (' + EMIT_ENTRY + ') por ' + ISSUE_AUTH[0] + ', pregunta 3, la '
                 'emision condicional de PB-T3 (' + os.path.basename(ISSUE_AUTH[1]) + '): PB-T3b tras el cierre de PB-T3a (' + PT3A_CLOSE[:8] + ', '
                 + PT3A_CLOSE_ENTRY + '; CLOSED / PASS WITH LIMITATIONS sobre ' + GREEN_T3A[:8] + '), con su remedicion sobre ese cierre y su comprobacion '
                 'acotada (' + (BOUNDED_RESULT or '<resultado por rellenar>') + '); F4B-E29, D-1, la variante (A) (owner-pbt3-d1-decision.md): S-1 a S-6 '
                 'con Skip y sus equivalentes E-1 a E-6, con su registro declarado; F4B-E40, la firma nueva de nc1 PD-F4B-01 y su sustituto '
                 '(owner-pbt2c-nc1-decision.md), y F4B-E43, el sustituto de nc2 ampliado de PB-T3b en adelante (owner-pbt3a-nc2-decision.md), que '
                 'PBT3B-C11 nombra en lugar de los del bloque citado del paquete; F4B-E42, ForFullWrite (owner-pbt3a-g9-decision.md); [OD] (la fila '
                 'de PB-T3, D-PB-04, D-PB-05 y D-PB-10; owner-f4b-disposition.md); F4B-E11, D2 (owner-d1-d2-decisions.md); las decisiones de '
                 'procedimiento owner-f4b-procedure-decisions.md (F4B-E03); F4B-E37, la Snapshot K (owner-config-snapshot-k-decision.md); F4B-E35, '
                 'pregunta 4, la regla RR de la reparacion del RED; F4B-E21, las colecciones opacas que pone la sesion '
                 '(owner-f4b-opaque-collections-decision.md); F4B-E22 (owner-pbt1b-rework-decisions.md); F4B-E24, la ExtensionData anidada '
                 '(owner-f4b-dpb07-extensiondata-decisions.md, pregunta 2); F4B-E25, la regla de F4B sobre las rutas UNIT_CHANGE en una correccion '
                 '(owner-f4b-correction-unit-change-decision.md), con PP y UR; y el paquete del contrato ' + PKG_VERSION + ', con su revision independiente, '
                 'sus recomprobaciones, la revision de compatibilidad con PB-T2 y las comprobaciones acotadas de PB-T3a y de PB-T3b'),
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
# la variante (A') no elegida (F4B-E29): tampoco en el contrato entero, con lo copiado del contrato de PB-T3a
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


# la comprobacion acotada de PB-T3b (CA-T3b; precondiciones 3 y 10): BOUNDED_RESULT, el veredicto literal, y bounded_check, estricto, sobre el informe
soft(BOUNDED_RESULT, 'emision: falta BOUNDED_RESULT, el veredicto literal de la comprobacion acotada de PB-T3b (CA-T3b; precondiciones 3 y 10)')
assert BOUNDED_RESULT is None or (all(ord(ch) < 0x80 for ch in BOUNDED_RESULT) and re.match(r'EMITIBLE(?: TRAS CORRECCIONES)?[.,;]', BOUNDED_RESULT)), \
    ('BOUNDED_RESULT: solo la familia EMITIBLE (EMITIBLE o EMITIBLE TRAS CORRECCIONES; DU-18)', BOUNDED_RESULT)
BOUNDED_SRC = None  # de donde leyo bounded_check el informe de CA-T3b: en seco, el local (--bounded-file) manda, porque crece antes del commit X
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
        _err = bounded_check(_rep, BOUNDED_RESULT, PKG_LINES, reviewer='CA-T3b')
        assert not _err, ('la comprobacion acotada de PB-T3b', BOUNDED_SRC, _err)
else:
    soft(False, 'emision: la comprobacion acotada de PB-T3b (%s) aun no existe (ni en la revision ni --bounded-file)' % BOUNDED_CHECK)
# las autoridades pendientes (el sustituto de nc2 v2, sus limites y su revision) ya dan su AVISO, uno, con NC2_V2_OK
assert [p for p, _ in PENDING_AUTH] == [NC2_V2, NC2_V2_LIMITS, NC2_V2_REVIEW]


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
    if soft(in_head(p), 'emision: a custodiar con la emision de PB-T3b, aun no en la revision: %s' % p):
        _lp = local_paths(show_bytes(p).decode('utf-8', 'replace'))
        assert not _lp, ('rutas locales en un custodiado (DU-11: se redacta antes de custodiar)', p, _lp)
        if p in CUSTODY_HEADS:
            _lines = lines_of(p)
            soft(all(any(l.startswith(h) for l in _lines) for h in CUSTODY_HEADS[p]),
                 'emision: al custodiado le faltan apartados (%s; los trae el commit X): %s'
                 % (', '.join(h.strip() for h in CUSTODY_HEADS[p] if not any(l.startswith(h) for l in _lines)), p))
for p in CUSTODY_EXTRA:
    if soft(in_head(p), 'emision: a custodiar con la emision, sin ser autoridad, aun no en la revision: %s' % p):
        _lp = local_paths(show_bytes(p).decode('utf-8', 'replace'))
        assert not _lp, ('rutas locales en un custodiado (DU-11: se redacta antes de custodiar)', p, _lp)
if DRY:
    print('DRY rev', REV_SHA[:8], '(objetos del checkout principal)' if OBJ_REV else '(HEAD del worktree)', 'allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN),
          '(src', len(SRCF), 'tests', len(TESTF), '[core', len(CORE_T), 'ui', str(len(UI_T)) + '] root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'required', len(REQ), 'evidence', len(EVID), 'authorities', len(c['Authorities']), 'anchors-ok', len(ANCHORS), 'bytes', len(b),
          'sha256', hashlib.sha256(b).hexdigest().upper())
    print('anclaje:', ANCHOR_SRC, '| f4b-dispositions hasta', DISP_LAST)
    print('paquete leido:', PKG_SRC, '->', PKG_IN, '| sha256', hashlib.sha256(PKG_RAW).hexdigest()[:16], '| textos de la %s comprobados:' % PKG_VERSION,
          ', '.join(PKG_CHECKS))
    print('contrato de PB-T3a: %s | tests de PB-T3b: Core %d (las 28 de PB-T3a sin el prefijo Workspace/, 76 de Workspace/ y 5 de PB-T3a) y UI %d '
          '(68 + 1); src: %d' % (PBT3AC + ' en ' + REV_SHA[:8], len(CORE_T), len(UI_T), len(SRCF)))
    print('D-1 (F4B-E29): S-1 a S-6 en %s, sin Skip en la base; %d filas (5 Fact y las 5 de S-2) | Workspace/: %d archivos, %d en la lista explicita'
          % (', '.join(sorted({os.path.basename(p) for _, p, _, _, _, _ in D1_METHODS})), D1_ROWS, len(WS_FILES), len(WS_LIST)))
    print('PBT3B-C13: %d nombres nuevos libres en Pilot/; Prepare, Precheck, Execute y ComparePost una vez; Execution con %d miembros tipados en el '
          'piloto; censo de la base: PP %d y UR %d declaraciones' % (len(C13_NEW), len(EXEC_TYPED), len(CENSUS_BASE[PP]), len(CENSUS_BASE[UR])))
    print('PBT3B-C11: el bloque citado del paquete con %d cambios (nc2 v2 de F4B-E43, nc1 PD-F4B-01 de F4B-E40, las diferencias admitidas, las '
          'variaciones que vuelven al Owner y el final); sustituto de nc1 en la revision con SHA-256 %s; sustituto de nc2 v2: %s'
          % (len(C11_SUBS), NC1_SUBST_SHA[:8], ('en la revision con SHA-256 ' + NC2_V2_SHA[:8]) if NC2_V2_OK else
             (('SHA-256 fijado (%s), por custodiar con su revision y sus limites' % NC2_V2_SHA[:8]) if NC2_V2_SHA else 'por custodiar (NC2_V2_SHA)')))
    _free = [p for p in CUSTODY_FREE if not in_head(p)]
    print('nombres de custodia nuevos de F4B-DRAFTS/pbt3/ libres en la revision: %d de %d' % (len(_free), len(CUSTODY_FREE)))
    print('Snapshot K (F4B-E37): ConfigSha256 %s y sonda neutra %s con Pass | C07, C18 y los comunes == los de PB-T3a | F4B-E35: preguntas 3 y 4 y RR-1 a '
          'RR-7 | ISSUE_AUTH = %s, pregunta 3' % (SNAP_K_SHA[:8], SNAP_K_PROBE, ISSUE_AUTH[0]))
    print('BOUNDED_RESULT: %s | bounded_check: %s | ultima recomprobacion de CA-T3b: %s' % (
        BOUNDED_RESULT or 'por rellenar',
        (('sin errores sobre ' + BOUNDED_SRC) if BOUNDED_RESULT else ('no corre sin BOUNDED_RESULT (informe: %s)' % BOUNDED_SRC)) if BOUNDED_SRC
        else 'sin informe (ni en la revision ni --bounded-file)', BOUNDED_LAST))
    print('emision (%s): %s en f4b-dispositions.md: %s' % (EMIT_ENTRY, EMIT_ENTRY, 'si' if E44_OK else 'no (lo entrega el commit X)'))
    print('tabla de RR-3 (filas de PB-T3b): %s' % RR3_SUMMARY)
    print('D1: needs de build-plugin =', _needs, '| F4B-E30: clases de UI de los filtros que crean el configurador real: %d (de %d archivos)'
          % (len(_cfg), len(UI_FILES)), "| (A') en el contrato: %d" % b.count(VARIANT.encode('ascii')))
    print('RequiredTests (criterio de count_tests_git.py sobre los objetos de la revision):')
    for l in REQ_REPORT:
        print('  ' + l)
    print('AVISOS (%d):' % len(WARN))
    for w in WARN:
        print('  - ' + w)
    if '--print' in ARGS:
        for i, f in enumerate(FORBIDDEN, 1):
            print(i, f)
    if '--print-rr3' in ARGS:
        for cls, (ls, vs, ds) in RR3_INFO.items():
            print(cls, 'valores', ascii(vs))
            for x in ls:
                print('  ', ascii(x))
    if JSON_OUT:
        open(JSON_OUT, 'wb').write(b)
    if '--print-json' in ARGS:  # los bytes exactos del contrato (sin la traduccion de fin de linea de la consola)
        sys.stdout.flush()
        sys.stdout.buffer.write(b)
    sys.exit(0)
assert ISSUE_AUTH and BOUNDED_RESULT and in_head(BOUNDED_CHECK) and NC2_V2_OK and E44_OK, \
    'faltan BOUNDED_RESULT, la comprobacion acotada, el sustituto de nc2 v2 revisado o F4B-E44'
assert not WARN, WARN
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
