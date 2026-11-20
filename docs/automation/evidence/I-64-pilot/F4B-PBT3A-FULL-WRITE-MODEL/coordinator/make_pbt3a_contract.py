"""Contrato de gate de F4B-PBT3A-FULL-WRITE-MODEL (ASCII), desde el paquete del contrato de PB-T3 v4.5 (la reemision del intento 2, F4B-E42)
(docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md: section 3, D-PBT3-02 a D-PBT3-08; section 5.0 y 5.2; section 6.0 y 6.1) y el
patron de make_pbt1c_contract.py y de los generadores de PB-T2. PB-T2 va delante (v4.2, V42-01): la BaseSha de PB-T3a es hija del cierre de
PB-T2c (PT2C_CLOSE, por rellenar), que aun no existe; PB-T2b esta cerrada (F4B-E38) y PB-T2c emitida (F4B-E39).
La v4.3 (V43-01 a V43-13) trae: la regla RR de la reparacion del RED (F4B-E35, pregunta 4), con PBT3A-C07 y PBT3A-C18 y la cadena de
INV-PBT3A-02; la Snapshot K en P-01 (F4B-E37; snapk_check, como el generador de PB-T2c); PBT3A-C11 con el sustituto de nc2 adaptado a F4B
(F4B-E34), del bloque citado del paquete y con el SHA-256 del sustituto en la revision; la emision condicional (F4B-E35, pregunta 3), que es
ISSUE_AUTH; y la comprobacion acotada de PB-T3a frente a BOUNDED_RESULT (bounded_check).
La v4.4 (V44-01 a V44-06) aplica la comprobacion acotada CA-T3a (EMITIBLE TRAS CORRECCIONES, B 0 / I 1 / M 4): la ExpectedEvidence del
section 6.1 con nueve vinetas, la novena la lectura entera de la verificacion con una reparacion (M-01); y la tabla de RR-3 con "Lineas"
(subcadenas) y "Valores" (tokens), enumerable sin interpretar (M-02 y M-03; rr3_table_check). BOUNDED_RESULT es el literal de CA-T3a, y
bounded_check lo comprueba en seco contra el informe local (--bounded-file) y, al escribir, contra el custodiado.
La v4.5 (V45-01 a V45-05; F4B-E42, "Renombrar ForWrite"): el intento 1 de PB-T3a paro antes del RED con PBT3A-C02, porque el esqueleto hacia
que PushBackFullComposition.cs nombrara el piloto y declarara ForWrite, termino de la guarda G-9. El miembro cerrado pasa a ForFullWrite
(SKELETON y CLOSED_EXPECTED); la precondicion 11 es la reemision; la custodia gana v45-delta.md, la decision del Owner, el analisis de la sesion
y la comprobacion acotada de la v4.5 (la seccion 11 de CA-T3a), y BOUNDED_RESULT tiene que nombrar esa recomprobacion. La base es la emision de
PB-T3a (18e4b783, F4B-E41), sin cambios en src/ ni en tests/ desde el cierre de PB-T2c.
El ForbiddenWriteScope de src/ sale del algoritmo de [PKC-C] (cover()); los de tests/ y la raiz son los del contrato de PB-T2c mas su clase de
prueba (section 6.0 del paquete, V42-05: las listas curadas de PB-T1 y PB-T2 con sus clases; en tests/RackCad.Tests/, el prefijo Workspace/).
El contrato de PB-T2c sale del emitido en la revision o, en seco, de --pt2c-contract (su contrato en seco).
Comprueba sobre la revision leida (HEAD del worktree o, con --objects-rev, esa revision en el almacen de objetos del checkout principal):
- el anclaje del section 6.0 (V42-01; V43-10): la revision desciende del cierre de PB-T1b (F4B-E26), del de PB-T1c (F4B-E31), de la emision
  de PB-T2a (F4B-E32), del cierre de PB-T2b (F4B-E38) y de la emision de PB-T2c (F4B-E39), con el src del cierre de PB-T2b; Application es la
  del cierre de PB-T1b mas los dos archivos de PB-T2a; Domain y Plugin, los del cierre de PB-T1b; src/RackCad.UI
  solo difiere del cierre de PB-T1c en W y PushBackEditorMode.cs (PB-T2b), con WC igual; tests solo gana las clases de PB-T2; y, con
  PT2C_CLOSE (constante o --pt2c-close), que la revision desciende del cierre de PB-T2c, con su src, su entrada de cierre y su task-close-package;
- las anclas de Application, del piloto, del Plugin y de las pruebas que usa PB-T3a (linea y texto); las de W, por contenido sobre la revision
  (V42-10): la adopcion de Recompute y su catch, el Recompute(); final de LoadFromModel, los ensambladores, LastComputation, la firma de
  LoadExisting y la costura, con los cuerpos de Recompute, LoadFromModel y ReadInputs iguales a los del cierre de PB-T1c (INV-PBT2B-04); el
  ayudante Pasada de [PKT1] section 5.0 frente al de las pruebas; las 28 rutas anidadas de I58F1Matrix; Fixtures/I64; los nombres nuevos libres;
- el texto del paquete (solo si el leido es la v4.5): el Objective, el AllowedWriteScope, el esqueleto del RED, la tabla de RequiredTests, los
  invariantes propios y comunes, las paradas propias y comunes (C07 con la excepcion de RR y C18) y la ExpectedEvidence, literales y SIN sus
  "Nota (no va al contrato)" ni los rotulos de C07 y C18, y sin ningun texto de la variante (A') no elegida; el bloque citado de PBT3A-C11; la
  regla RR del section 6.0, con la tabla cerrada de RR-3 (texto, no vinetas: se lee aparte); la lista cerrada de PBT3A-C13; las clases y la lista
  CT-PBT3A-01 a 14 de 5.2; P-01 (la K), las celdas, la CI del RED, "PB-T3a solo crea archivos", las referencias del anclaje y las nueve clases
  de PB-T2 de 6.0; y su sha256, el revisado;
- los MinSelected, con el criterio de count_tests_git.py (Fact = 1; Theory = sus InlineData; MemberData aparte) sobre los objetos de la
  revision: los nuevos, ausentes; los fijos del paquete, como minimos; los de PB-T1, de PB-T2 y de Workspace, medidos (la nota del section 6.1),
  y los de PB-T2 iguales a los del contrato de PB-T2c;
- D1: build-plugin con needs [tests, build-ui] en .github/workflows/ci.yml; la Snapshot K (snapk_check); F4B-E35 (rr_check y q3_check); el
  sustituto de nc2 de F4B-E34 con su SHA-256; y F4B-E30: ninguna clase de los filtros de UI, tambien las de PB-T2, crea un RackFrameConfiguratorWindow.
Lo que depende del cierre de PB-T2c (PT2C_CLOSE, CI_BASE_T2C, la clase de PB-T2c y su task-close-package) o de la emision de PB-T3a
(BOUNDED_RESULT y su comprobacion acotada, las rutas de CUSTODY y CUSTODY_EXTRA, entre ellas el paquete v4.5 custodiado) es AVISO en seco y error
al escribir; todo lo demas es error siempre.
Uso: python make_pbt3a_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print] [--print-json] [--json-out <archivo>]
                                   [--objects-rev <rev> [--repo <checkout principal>] [--package-file <paquete>] [--pt2c-contract <contrato>]
                                   [--bounded-file <informe de CA-T3a>]] [--pt2c-close <sha>] [--as-write]
Con --dry no escribe nada en el repositorio: comprueba e imprime los recuentos, los bytes y los AVISOS (con --print, ademas, el
ForbiddenWriteScope numerado; con --print-json, el contrato; con --json-out, el contrato a ese archivo, fuera del repositorio). --objects-rev
exige --dry: no hace chdir al worktree y lee todo con git -C <checkout principal> show <rev>:<ruta>. --package-file y --pt2c-contract exigen
--objects-rev y --dry: el paquete (la v4.5 sin custodiar) y el contrato de PB-T2c (el seco, si aun no esta emitido) se leen de esos archivos;
--bounded-file (igual) es el informe de la comprobacion acotada sin custodiar, que bounded_check compara con BOUNDED_RESULT y el paquete.
--as-write (solo con --dry) trata los AVISOS como errores, como al escribir. Sin --objects-rev hace chdir al worktree y lee HEAD; al escribir, la
sesion lo ejecuta alli tras el commit que custodia la v4.5, el registro, la revision, la remedicion, la comprobacion acotada y CUSTODY_EXTRA (entre
ellas make_pbt3a_contract-dry.txt, la salida de este --dry).
Al custodiarlo, las rutas locales se redactan como en PB-T0 ('<worktree-de-I-64>', '<checkout-principal>')."""
import json, os, re, sys, subprocess, hashlib, unicodedata

# ---------------------------------------------------------------- lo que depende del cierre de PB-T2c (PB-T2 va delante; V42-01), que aun no existe
PT2C_CLOSE = 'f273eae1b0156e78260b6a5efe980a79bfe075b3'  # la custodia del cierre de PB-T2c (F4B-E40), hija directa del GREEN; tambien --pt2c-close
CI_BASE_T2C = ('44619492077', 'b1968341', 19094, 2450, 21)  # la CI del GREEN de PB-T2c en su cierre (F4B-E40): la prevista
CI_FORECAST = (19094, 2450, 21)  # PREVISTA, no medida: el GREEN de PB-T2c en su contrato (paquete de PB-T2 v2.6); el GREEN de PB-T3a, +13 y +1
# ---------------------------------------------------------------- lo que depende de la emision de PB-T3a
# la autorizacion de emision del Owner (precondicion 10; V43-07): F4B-E35, pregunta 3, "Si, con esa condicion (Recomendado)"
ISSUE_AUTH = ('F4B-E35', 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-f4b-pbt3-emission-red-repair-decisions.md')
# el veredicto literal de la comprobacion acotada de PB-T3a (CA-T3a, independent-review-pbt3a-v43-check.md) para la reemision: el de su
# seccion 11, la recomprobacion sobre la v4.5 (precondicion 11), en curso. El de la emision con la v4.4 fue:
#   'EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; importantes corregidas en el paquete: I-01 (CT-PBT3A-09 y CT-PBT3B-01 sin el gesto inerte de
#    BeamDepth); recomprobacion v4.4: EMITIBLE, B 0 / I 0 / M 1'
BOUNDED_RESULT = ('EMITIBLE TRAS CORRECCIONES; B 0 / I 1 / M 4; importantes corregidas en el paquete: I-01 (CT-PBT3A-09 y CT-PBT3B-01 sin el '
                  'gesto inerte de BeamDepth); recomprobacion v4.4: EMITIBLE, B 0 / I 0 / M 1; recomprobacion v4.5: EMITIBLE, B 0 / I 0 / M 2')
# ---------------------------------------------------------------- PB-T2b cerrada y PB-T2c emitida (medido por objetos en 05beabfd)
PT2B_CLOSE = '56cb026f8a6fd87467a6400e2f3bca015c4abda3'  # F4B-E38: la custodia del cierre de PB-T2b (con la Snapshot K, F4B-E37)
PT2B_CLOSE_ENTRY = 'F4B-E38'
PT2C_EMIT = '05beabfdd9efa5bb530860dc156f5832e4e452c2'   # F4B-E39: la emision de PB-T2c (X 92232ed3)
PT2C_EMIT_ENTRY = 'F4B-E39'
# la Snapshot K (F4B-E37; V43-08): P-01 y su autoridad; su registro, con el SHA-256 del config.toml y la sonda neutra (como el generador de PB-T2c)
SNAP_K = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-K/snapshot-k.json'
SNAP_K_DEC = 'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-config-snapshot-k-decision.md'
SNAP_K_SHA = 'E1F69847610FD810D5286B82C86E00FE85D9F497F89E9FD9645E0EEA113322FE'
SNAP_K_PROBE = 'R20261119T154313Z-2a9a'
# el sustituto de nc2 adaptado a F4B (F4B-E34; V43-09), que PBT3A-C11 copia
SUBST_F4B = ('docs/automation/evidence/I-64-pilot/F4B-PBT2A-DRAFT-STATE-PORT-nc2-substitute/2/R20261119T125505Z-c2d3/'
             'nc2_exact_whitelist_substitute_f4b.py')
SUBST_F4B_SHA = '6ccaf12dbf9122c0bddc5b724bcff573705c95698af38bf750bf69459ee2ffd3'
# ---------------------------------------------------------------- lo que ya existe (medido por objetos en 5ddeb3b4 y 05beabfd)
PB1B_CLOSE = '95373af6116cf120bd0a8ba490833524e33c47da'  # la custodia del cierre de PB-T1b (F4B-E26): Application, Domain y Plugin
PB1B_CLOSE_ENTRY = 'F4B-E26'
PB1B_GREEN = 'a18ba345eddc5333c22b8bacbad5c2ed4dbf5959'  # el GREEN de PB-T1b, con el mismo src del cierre
PT1C_ISSUE_ENTRY = 'F4B-E27'                              # la emision de PB-T1c
PT1C_CLOSE = '5a8366c166cd1034293811e5c2ac0f7c6e05a761'  # la custodia del cierre de PB-T1c (F4B-E31): src/RackCad.UI y WC de la BaseSha
PT1C_CLOSE_ENTRY = 'F4B-E31'
PT2A_EMIT = '0dc623ca7a4bf1640654440276aa3d27147f48d4'   # la emision de PB-T2a (F4B-E32)
PT2A_EMIT_ENTRY = 'F4B-E32'
PT2A_STOP_ENTRY = 'F4B-E33'                               # PB-T2a, intento 1 parado; la correccion, en el intento 2
OWNER_PBT2_ENTRY = 'F4B-E28'                              # las decisiones de PB-T2 (su pregunta 4 no cubre PB-T3a)
D1_ENTRY = 'F4B-E29'       # D-1 (la opcion (A)): decide PB-T3b, no PB-T3a
PKG_VERSION = 'v4.5'       # la version del paquete que se custodia con la reemision de PB-T3a (F4B-E42)
PKG2_VERSION = 'v2.9'      # el paquete de PB-T2 con el que se emitio PB-T2c (F4B-E39), custodiado en 92232ed3
CUSTODY_SHA256 = {         # el paquete revisado, byte a byte: la v4.5 final, con V45-06 (CA-T3a seccion 11, M-06), 282 234 bytes, LF (comprobado)
    'docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md': '64ab03df27cc8f441883442ca999036e5f7555ede9269eb3f4f768ab8a07fdbd',
}
PT3A_EMIT = '18e4b78302d9010555e5c7b7a6c6f7384622c0e6'  # la emision de PB-T3a con la v4.4 (F4B-E41): BASE 18e4b783, X 2d876831
PT3A_EMIT_X = '2d8768318f741f0ace8cf3e3b24f11b9f5877a88'
PT3A_EMIT_ENTRY, PT3A_STOP_ENTRY = 'F4B-E41', 'F4B-E42'  # la emision y la parada del intento 1, con la decision del Owner (Renombrar ForWrite)
RUN1 = 'R20261120T001738Z-7d92'  # el Worker del intento 1 (delegacion R20261120T001442Z-903c), parado con PBT3A-C02 antes del RED
CUSTODY_HEADS = {          # el registro y la revision pueden crecer en la emision (la remedicion, la comprobacion acotada): sus apartados, al menos
    # R-19, la remedicion de la emision; R-20, la lectura de G-9 en R-6 corregida para la reemision (F4B-E42; V45-02)
    'docs/initiatives/I-64-f4b-pbt3-measurements.md': ['## R-0. ', '## R-6. ', '## R-16. ', '## R-17. ', '## R-18. ', '## R-19. ', '## R-20. '],
    'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt3/independent-review-v1.md': ['## 9. Veredicto', '## ' + chr(0xA7) + '10. ', '## ' + chr(0xA7) + '11. ',
                                                                                     '## ' + chr(0xA7) + '12. ', '## ' + chr(0xA7) + '13. '],
    # CA-T3a, con su recomprobacion de la v4.4 (seccion 10) y la de la v4.5 para la reemision (seccion 11; precondicion 11)
    'docs/automation/evidence/I-64-pilot/F4B-DRAFTS/pbt3/independent-review-pbt3a-v43-check.md': ['## ' + chr(0xA7) + '10. ', '## ' + chr(0xA7) + '11. '],
}
MAIN_CHECKOUT = '<checkout-principal>'  # Redaccion para el repo publico: la ruta local del checkout principal  # solo con --objects-rev; se redacta al custodiar ('<checkout-principal>')
WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree  # solo al escribir; se redacta al custodiar

ARGS = sys.argv[1:]
VALUED = ('--objects-rev', '--repo', '--pt2c-close', '--package-file', '--json-out', '--pt2c-contract', '--bounded-file')


def opt(name):
    if name not in ARGS:
        return None
    i = ARGS.index(name)
    assert i + 1 < len(ARGS) and not ARGS[i + 1].startswith('--'), ('falta el valor de', name)
    return ARGS[i + 1]


POS = [a for i, a in enumerate(ARGS) if not a.startswith('--') and not (i > 0 and ARGS[i - 1] in VALUED)]
assert len(POS) == 3, 'uso: make_pbt3a_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--objects-rev <rev>] ...'
ISSUED, AR, MAIN = POS
DRY = '--dry' in ARGS
AS_WRITE = '--as-write' in ARGS
OBJ_REV = opt('--objects-rev')
PT2C_CLOSE = opt('--pt2c-close') or PT2C_CLOSE
assert OBJ_REV is None or DRY, '--objects-rev solo en seco: anade --dry'
assert not AS_WRITE or DRY, '--as-write solo en seco'
PKG_FILE = opt('--package-file')
assert PKG_FILE is None or (OBJ_REV is not None and DRY), '--package-file solo junto con --objects-rev y --dry'
PKG_FILE = os.path.abspath(PKG_FILE) if PKG_FILE else None  # antes de cualquier chdir
assert PKG_FILE is None or os.path.isfile(PKG_FILE), ('no existe el paquete', PKG_FILE)
T2C_FILE = opt('--pt2c-contract')
assert T2C_FILE is None or (OBJ_REV is not None and DRY), '--pt2c-contract solo junto con --objects-rev y --dry'
T2C_FILE = os.path.abspath(T2C_FILE) if T2C_FILE else None
assert T2C_FILE is None or os.path.isfile(T2C_FILE), ('no existe el contrato de PB-T2c', T2C_FILE)
BOUNDED_FILE = opt('--bounded-file')  # el informe de CA-T3a sin custodiar, en seco: bounded_check lo lee como leera el custodiado
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
assert PT2C_CLOSE is None or re.fullmatch(r'[0-9a-f]{40}', PT2C_CLOSE), PT2C_CLOSE

if OBJ_REV is None:
    os.chdir(WT)
    REPO, REV = None, 'HEAD'
else:
    REPO, REV = opt('--repo') or MAIN_CHECKOUT, OBJ_REV
DISK = OBJ_REV is None  # solo en el worktree se mira tambien el disco

TASK = 'F4B-PBT3A-FULL-WRITE-MODEL'
assert 24 <= len(TASK) <= 26, len(TASK)  # R-T1-07: MAX_PATH
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
D3 = PILOT + '/F4B-DRAFTS/pbt3/'
DISP = PILOT + '/F4B-DRAFTS/f4b-dispositions.md'
F4B_E25 = CO + 'owner-f4b-correction-unit-change-decision.md'
F4B_E25_RET = PILOT + '/F4B-DRAFTS/pbt1b/owner-return-verify-stop.md'
PT1C_TASK = 'F4B-PBT1C-LOSSLESS-STATE-WINDOW'
PT1C_TCP = PILOT + '/' + PT1C_TASK + '/task-close-package.md'
PT2A_TASK, PT2B_TASK, PT2C_TASK = 'F4B-PBT2A-DRAFT-STATE-PORT', 'F4B-PBT2B-WORKSPACE-MODE-WINDOW', 'F4B-PBT2C-WORKSPACE-CARRYOVER'
PT2C_TCP = PILOT + '/' + PT2C_TASK + '/task-close-package.md'
PBT2CC = PILOT + '/' + PT2C_TASK + '/coordinator/gate-contract.json'
PKG2 = 'docs/initiatives/I-64-f4b-pbt2-gate-contract-package.md'
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
    """Lo que solo existira con los cierres de PB-T2b y PB-T2c, con la emision de PB-T2c o con la emision de PB-T3a: AVISO en seco, error al
    escribir (y con --as-write)."""
    if cond:
        return True
    assert DRY and not AS_WRITE, msg
    WARN.append(msg)
    return False


REV_SHA = git('rev-parse', REV).stdout.decode('ascii').strip()
assert re.fullmatch(r'[0-9a-f]{40}', REV_SHA), ('revision desconocida', REV)

# ---------------------------------------------------------------- el alcance del producto: alias
APP = 'src/RackCad.Application/Systems/PushBack/'
DRAFT = APP + 'Draft/'
UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
WC = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs'
MODE = 'src/RackCad.UI/Systems/PushBack/PushBackEditorMode.cs'  # el archivo del modo de PB-T2b
SEAM_GREEN = 'internal PushBackEditableState EditableStateForTest => editable;'  # la costura de PB-T1c (D-PBT1-13 (9)); INV-PBT2B-06 la conserva
# PB-T2 (section 6.0, V42-01 y V42-05): los dos archivos de Application de PB-T2a y las nueve clases de prueba de PB-T2
T2A_SRC = [APP + 'PushBackDraftStatePort.cs', APP + 'PushBackDraftEditorLaunch.cs']
T2_CORE_NAMES = ['PushBackDraftStatePortTests', 'PushBackDraftPortSharedReferenceGuardTests', 'PushBackWorkspaceModeSourceGuardTests',
                 'PushBackWorkspaceSharedReferenceGuardTests']
T2_UI_NAMES = ['PushBackWorkspaceSupport', 'PushBackWorkspaceModeTests', 'PushBackWorkspaceApplyCloseTests', 'PushBackFieldValidationConcordanceTests',
               'PushBackWorkspaceCarryoverTests']
T2_TESTS = [TR + n + '.cs' for n in T2_CORE_NAMES] + [UT + n + '.cs' for n in T2_UI_NAMES]


def names_of(r):
    return [p for p in r.stdout.decode('utf-8').split('\n') if p]


# ---------------------------------------------------------------- el anclaje (precondicion 1 y section 6.0 del paquete, V42-01)
for _short, _full in (('95373af6', PB1B_CLOSE), ('a18ba345', PB1B_GREEN), ('5a8366c1', PT1C_CLOSE), ('0dc623ca', PT2A_EMIT), ('56cb026f', PT2B_CLOSE),
                      ('05beabfd', PT2C_EMIT), ('18e4b783', PT3A_EMIT), ('2d876831', PT3A_EMIT_X)):
    assert git('rev-parse', _short).stdout.decode('ascii').strip() == _full, ('SHA', _short)
for _anc, _name in ((PB1B_CLOSE, 'el cierre de PB-T1b (F4B-E26)'), (PT1C_CLOSE, 'el cierre de PB-T1c (F4B-E31)'), (PT2A_EMIT, 'la emision de PB-T2a (F4B-E32)'),
                    (PT2B_CLOSE, 'el cierre de PB-T2b (F4B-E38)'), (PT2C_EMIT, 'la emision de PB-T2c (F4B-E39)'),
                    (PT3A_EMIT, 'la emision de PB-T3a con la v4.4 (F4B-E41)')):
    assert git('merge-base', '--is-ancestor', _anc, REV).returncode == 0, ('la revision no desciende de ' + _name)
# la reemision (precondicion 11 de la v4.5; F4B-E42): sobre la emision de PB-T3a, que no dejo commits del intento 1 en src/ ni en tests/
assert re.search(r'(?m)^## %s\. Emisi[^\n]*%s' % (PT3A_EMIT_ENTRY, 'F4B-PBT3A-FULL-WRITE-MODEL'), show(DISP, PT3A_EMIT).replace('\r\n', '\n')), \
    'PT3A_EMIT no es la emision de PB-T3a (F4B-E41)'
assert git('cat-file', '-e', PT3A_EMIT + ':' + PILOT + '/F4B-PBT3A-FULL-WRITE-MODEL/coordinator/gate-contract.json').returncode == 0, \
    'la emision de PB-T3a sin su contrato'
assert git('diff', '--quiet', PT3A_EMIT, REV, '--', 'src', 'tests').returncode == 0, \
    'src o tests cambiaron desde la emision de PB-T3a: la reemision va sobre ella, sin commits del intento 1 (F4B-E42)'
assert re.search(r'(?m)^## %s\. Cierre de %s\b' % (PT1C_CLOSE_ENTRY, PT1C_TASK), show(DISP, PT1C_CLOSE).replace('\r\n', '\n')), 'PT1C_CLOSE no es el cierre de PB-T1c'
assert git('cat-file', '-e', PT1C_CLOSE + ':' + PT1C_TCP).returncode == 0, 'el cierre de PB-T1c sin su task-close-package.md'
assert re.search(r'(?m)^## %s\. Emisi' % PT2A_EMIT_ENTRY, show(DISP, PT2A_EMIT).replace('\r\n', '\n')), 'PT2A_EMIT no es la emision de PB-T2a'
assert re.search(r'(?m)^## %s\. Cierre de %s\b' % (PT2B_CLOSE_ENTRY, PT2B_TASK), show(DISP, PT2B_CLOSE).replace('\r\n', '\n')), 'PT2B_CLOSE no es el cierre de PB-T2b'
assert git('cat-file', '-e', PT2B_CLOSE + ':' + PILOT + '/' + PT2B_TASK + '/task-close-package.md').returncode == 0, 'el cierre de PB-T2b sin su task-close-package.md'
assert re.search(r'(?m)^## %s\. Emisi[^\n]*%s' % (PT2C_EMIT_ENTRY, PT2C_TASK), show(DISP, PT2C_EMIT).replace('\r\n', '\n')), 'PT2C_EMIT no es la emision de PB-T2c'
# la base del section 6.0 (V42-01; V43-10): el cierre de PB-T2c, con el src del de PB-T2b (el GREEN de PB-T2c solo anade pruebas)
assert git('diff', '--quiet', PT2B_CLOSE, REV, '--', 'src').returncode == 0, 'src difiere del cierre de PB-T2b (V43-10: la base lleva su src)'
# Application: la del cierre de PB-T1b mas los dos archivos de PB-T2a; Domain y Plugin: los del cierre de PB-T1b
for _base, _name in ((PB1B_CLOSE, 'el cierre de PB-T1b'), (PB1B_GREEN, 'el GREEN de PB-T1b (a18ba345)')):
    _app = names_of(git('diff', '--name-only', _base, REV, '--', 'src/RackCad.Application'))
    assert set(_app) <= set(T2A_SRC), ('Application cambio fuera de los dos archivos de PB-T2a desde ' + _name, _app)
    assert git('diff', '--quiet', _base, REV, '--', 'src/RackCad.Domain', 'src/RackCad.Plugin').returncode == 0, ('Domain o el Plugin difieren de ' + _name)
soft(all(in_head(p) for p in T2A_SRC), 'cierre de PB-T2a: los dos archivos de Application de PB-T2a aun no estan en la revision')
# src/RackCad.UI: la del cierre de PB-T2b, que solo cambia W y anade el archivo del modo; WC, la del cierre de PB-T1c
_ui_diff = names_of(git('diff', '--name-only', PT1C_CLOSE, REV, '--', 'src/RackCad.UI'))
assert set(_ui_diff) <= {W, MODE}, ('src/RackCad.UI cambio fuera de W y del archivo del modo desde el cierre de PB-T1c', _ui_diff)
assert git('rev-parse', REV + ':' + WC).stdout == git('rev-parse', PT1C_CLOSE + ':' + WC).stdout, 'WC difiere del cierre de PB-T1c'
soft(W in _ui_diff and in_head(MODE), 'cierre de PB-T2b: W todavia es la del cierre de PB-T1c y PushBackEditorMode.cs aun no esta en la revision')
# tests/: desde el cierre de PB-T1c solo ganan las clases de PB-T2
_tests_diff = names_of(git('diff', '--name-only', PT1C_CLOSE, REV, '--', 'tests'))
assert set(_tests_diff) <= set(T2_TESTS), ('tests cambio fuera de las clases de PB-T2 desde el cierre de PB-T1c', _tests_diff)
# la costura de PB-T1c que usa CT-PBT3A-01, la del GREEN y una sola vez
_wrev = lines_of(W)
assert [l.strip() for l in _wrev if 'EditableStateForTest =>' in l] == [SEAM_GREEN], 'la costura EditableStateForTest de W no es la del GREEN de PB-T1c'
# el cierre de PB-T2c: "git diff <cierre de PB-T2c> <BaseSha> -- src vacio" (section 6.0)
CLOSE_ENTRY, CLOSE_TEXT = None, ''
if soft(PT2C_CLOSE is not None, 'cierre de PB-T2c: falta PT2C_CLOSE (constante o --pt2c-close); no se comprueban la descendencia, "git diff <cierre '
                                'de PB-T2c> <BaseSha> -- src" vacio, la entrada de cierre de f4b-dispositions.md ni el task-close-package de PB-T2c'):
    assert git('cat-file', '-e', PT2C_CLOSE + '^{commit}').returncode == 0, ('el cierre de PB-T2c no esta en el almacen de objetos', PT2C_CLOSE)
    assert git('merge-base', '--is-ancestor', PT2A_EMIT, PT2C_CLOSE).returncode == 0, 'PT2C_CLOSE no desciende de la emision de PB-T2a'
    assert git('merge-base', '--is-ancestor', PT2C_CLOSE, REV).returncode == 0, 'la revision no desciende del cierre de PB-T2c'
    assert git('diff', '--quiet', PT2C_CLOSE, REV, '--', 'src').returncode == 0, 'src cambio desde el cierre de PB-T2c (section 6.0: vacio)'
    _disp_c = show(DISP, PT2C_CLOSE).replace('\r\n', '\n')
    _heads = re.findall(r'(?m)^## (F4B-E\d+)\. Cierre de ' + PT2C_TASK + r'\b', _disp_c)
    assert len(_heads) == 1, ('PT2C_CLOSE no es el cierre de PB-T2c: su f4b-dispositions.md no tiene la entrada "Cierre de ' + PT2C_TASK + '"', PT2C_CLOSE)
    CLOSE_ENTRY = _heads[0]
    assert int(CLOSE_ENTRY.split('E')[-1]) > int(PT2A_STOP_ENTRY.split('E')[-1]), ('el cierre de PB-T2c es anterior a la parada de PB-T2a', CLOSE_ENTRY)
    for _t in (PT2A_TASK, PT2B_TASK):
        assert re.search(r'(?m)^## F4B-E\d+\. Cierre de %s\b' % _t, _disp_c), ('el cierre de PB-T2c sin el cierre de ' + _t + ' delante')
    _i = _disp_c.index('\n## %s. ' % CLOSE_ENTRY)
    _j = _disp_c.find('\n## F4B-E', _i + 1)
    CLOSE_TEXT = _disp_c[_i:_j if _j > 0 else len(_disp_c)]
    assert git('cat-file', '-e', PT2C_CLOSE + ':' + PT2C_TCP).returncode == 0, ('PT2C_CLOSE no tiene el task-close-package de PB-T2c', PT2C_CLOSE)
    assert git('cat-file', '-e', PT2C_CLOSE + ':' + PBT2CC).returncode == 0, ('PT2C_CLOSE sin el contrato emitido de PB-T2c', PT2C_CLOSE)
    assert W in _ui_diff and in_head(MODE), 'el cierre de PB-T2c sin W de PB-T2b ni el archivo del modo'
    # la decision del Owner del cierre (la firma nueva de nc1, PD-F4B-01): la cita la entrada y su archivo esta en el cierre
    assert 'owner-pbt2c-nc1-decision.md' in CLOSE_TEXT and 'PD-F4B-01' in CLOSE_TEXT, 'la entrada de cierre de PB-T2c sin la decision del Owner sobre nc1'
    assert git('cat-file', '-e', PT2C_CLOSE + ':' + PILOT + '/F4B-DRAFTS/coordinator/owner-pbt2c-nc1-decision.md').returncode == 0, \
        'el cierre de PB-T2c sin owner-pbt2c-nc1-decision.md'
if soft(CI_BASE_T2C is not None, 'cierre de PB-T2c: falta CI_BASE_T2C (la CI del GREEN de PB-T2c); la ExpectedEvidence lleva la linea base PREVISTA '
                                 'del paquete (Core %d, UI %d con %d Skipped), no la medida' % CI_FORECAST):
    assert len(CI_BASE_T2C) == 5, CI_BASE_T2C
    _run, _sha, _core, _ui, _skip = CI_BASE_T2C
    assert re.fullmatch(r'\d+', _run) and re.fullmatch(r'[0-9a-f]{8}', _sha) and all(isinstance(x, int) for x in (_core, _ui, _skip)), CI_BASE_T2C
    assert CLOSE_ENTRY, 'CI_BASE_T2C sin PT2C_CLOSE: la linea base se ata al cierre de PB-T2c'
    assert _run in CLOSE_TEXT and _sha in CLOSE_TEXT, ('la CI de CI_BASE_T2C no es la de la entrada de cierre', CLOSE_ENTRY)
    assert git('merge-base', '--is-ancestor', _sha, PT2C_CLOSE).returncode == 0 and \
        git('diff', '--quiet', _sha, PT2C_CLOSE, '--', 'src').returncode == 0, 'el GREEN de CI_BASE_T2C no tiene el src del cierre de PB-T2c'
ANCHOR_SRC = ('desciende de %s (%s), %s (%s), %s (%s), %s (%s), %s (%s) y %s (%s); src == cierre de PB-T2b; Application == cierre de PB-T1b + %s; '
              'Domain y Plugin == cierre de PB-T1b; '
              'src/RackCad.UI == cierre de PB-T1c salvo %s; WC == cierre de PB-T1c; tests == cierre de PB-T1c + %d clases de PB-T2; costura del GREEN; '
              'cierre de PB-T2c: %s' % (PB1B_CLOSE[:8], PB1B_CLOSE_ENTRY, PT1C_CLOSE[:8], PT1C_CLOSE_ENTRY, PT2A_EMIT[:8], PT2A_EMIT_ENTRY,
                                        PT2B_CLOSE[:8], PT2B_CLOSE_ENTRY, PT2C_EMIT[:8], PT2C_EMIT_ENTRY, PT3A_EMIT[:8], PT3A_EMIT_ENTRY,
                                        [os.path.basename(p) for p in T2A_SRC if in_head(p)], [os.path.basename(p) for p in _ui_diff] or 'nada',
                                        len(_tests_diff), ('%s (%s)' % (PT2C_CLOSE[:8], CLOSE_ENTRY)) if CLOSE_ENTRY else 'por rellenar'))

# ---------------------------------------------------------------- los contratos anteriores
PBT1CC = PILOT + '/' + PT1C_TASK + '/coordinator/gate-contract.json'
PBT0C = PILOT + '/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json'
PBTC2C = PILOT + '/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json'
T1BC = PILOT + '/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json'
pbt1c = json.loads(show(PBT1CC))
pbt0 = json.loads(show(PBT0C))
assert pbt1c['TaskId'] == PT1C_TASK and pbt1c['AuthorityRevision'].startswith('af46fcd5'), 'el contrato de PB-T1c no es el emitido (F4B-E27)'
# el contrato de PB-T2c (section 6.0, V42-05): el emitido, en la revision; en seco y mientras no se emita, su contrato en seco por --pt2c-contract
T2C_ITEMS = ('TaskId', 'AllowedWriteScope', 'ForbiddenWriteScope', 'RequiredTests', 'ExpectedEvidence', 'EligibleCells')
if soft(in_head(PBT2CC), 'emision de PB-T2c: el contrato emitido de PB-T2c aun no esta en la revision (%s); las listas de tests/ y los minimos '
                         'de PB-T2 salen de --pt2c-contract (%s)' % (PBT2CC, os.path.basename(T2C_FILE) if T2C_FILE else 'no dado')):
    pbt2c = json.loads(show(PBT2CC))
    T2C_SRC = PBT2CC + ' en ' + REV_SHA[:8]
    if T2C_FILE:  # el seco y el emitido tienen que dar las mismas listas
        _dry2c = json.load(open(T2C_FILE, encoding='ascii'))
        assert all(_dry2c[k] == pbt2c[k] for k in T2C_ITEMS if k != 'ExpectedEvidence'), 'el contrato de PB-T2c de --pt2c-contract difiere del emitido'
else:
    assert T2C_FILE, 'hace falta el contrato de PB-T2c: el emitido en la revision o, en seco, --pt2c-contract <su contrato en seco>'
    pbt2c = json.load(open(T2C_FILE, encoding='ascii'))
    T2C_SRC = '--pt2c-contract ' + os.path.basename(T2C_FILE)
assert pbt2c['TaskId'] == PT2C_TASK and pbt2c['AllowedWriteScope'] == [W, WC, UT + 'PushBackWorkspaceCarryoverTests.cs'], 'el contrato de PB-T2c'
assert pbt2c['EligibleCells'] == pbt1c['EligibleCells'], 'las celdas de PB-T2c difieren de las de PB-T1c'

# Las 17 estandar, de PB-T1c, con P-01 en la Snapshot K (F4B-E37, que sustituye a la J de F4B-E19; section 6.0 del paquete, V43-08)
SNAP_J = 'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-J/snapshot-j.json'
P01 = 'config.toml no longer matches the accepted Snapshot in force (today Snapshot %s, %s)'
STD_IDS = ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
_p_t1c = [dict(s) for s in pbt1c['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
assert [s['Id'] for s in _p_t1c] == STD_IDS and _p_t1c[9]['Text'] == P01 % ('J', SNAP_J), _p_t1c[9]['Text']
P_STOPS = [dict(s, Text=P01 % ('K', SNAP_K)) if s['Id'] == 'P-01' else dict(s) for s in _p_t1c]
# las 17 son las del contrato emitido de PB-T2c, que ya llevan la K
assert P_STOPS == [dict(s) for s in pbt2c['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])], 'las paradas estandar difieren de las de PB-T2c'


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


# la K y su decision se custodiaron con el cierre de PB-T2b (56cb026f), del que la revision ya desciende: siempre assert
assert in_head(SNAP_K) and in_head(SNAP_K_DEC), ('la Snapshot K o su decision no estan en la revision', SNAP_K, SNAP_K_DEC)
_err = snapk_check(show(SNAP_K), show(SNAP_K_DEC), SNAP_K_SHA, SNAP_K_PROBE)
assert not _err, ('la Snapshot K (F4B-E37)', _err)

# ---------------------------------------------------------------- el alcance (section 6.1: 14 rutas; section 5.2: las clases)
SRC_NAMES = ['PushBackFullDraftState.cs', 'PushBackFullDraftStateComparer.cs', 'PushBackEditableStateSeat.cs', 'PushBackFullDraftMaterializer.cs',
             'PushBackFullDraftReader.cs', 'PushBackFullComposition.cs', 'PushBackViewDisposition.cs', 'PushBackConflictClassifier.cs']
SRC_NEW = [DRAFT + n for n in SRC_NAMES]
# las clases de 5.2 con sus CT (la lista cerrada CT-PBT3A-01 a 14)
CLASS_CTS = [('Core', 'PushBackFullDraftLoadTests', [2, 3, 4, 5]), ('Core', 'PushBackFullDraftCompositionTests', [6, 7, 11]),
             ('Core', 'PushBackFullDraftViewTests', [8]), ('Core', 'PushBackFullDraftClassifierTests', [9, 10]),
             ('Core', 'PushBackDraftWriteGuardTests', [12, 13, 14]), ('UI', 'PushBackFullDraftWindowParityTests', [1])]
assert sorted(n for _, _, cts in CLASS_CTS for n in cts) == list(range(1, 15)), 'la lista cerrada CT-PBT3A-01 a 14'
TEST_NEW = [(TR if p == 'Core' else UT) + c + '.cs' for p, c, _ in CLASS_CTS]
ALLOWED = SRC_NEW + TEST_NEW
assert len(ALLOWED) == 14 == len(set(ALLOWED)) and len(TEST_NEW) == 6  # INV-PBT3A-01 (14 rutas) e INV-PBT3A-02 (seis clases)
assert all(not in_head(p) and (not DISK or not os.path.exists(p)) for p in ALLOWED), [p for p in ALLOWED if in_head(p)]
assert not in_head(DRAFT) and not (DISK and os.path.exists(DRAFT)), 'la carpeta Draft/ ya existe'
for _pref in ('PushBackFullDraft', 'PushBackDraftWriteGuardTests'):
    assert not [p for p in tracked('tests/') if _pref in p], ('el prefijo ya existe en tests/', _pref)


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
# INV-PBT3A-01: ningun archivo existente de src/ fuera del ForbiddenWriteScope; el piloto, la ventana, el Plugin y Domain, enteros
assert {'src/RackCad.Domain/', 'src/RackCad.Plugin/', 'src/RackCad.UI/', APP + 'Pilot/', 'src/RackCad.Application/Workspace/'} <= set(SRCF)
assert all(p in SRCF for p in tracked(APP) if '/' not in p[len(APP):]), 'un archivo de Systems/PushBack/ sin cubrir'

# tests/ (section 6.0, V42-05): las listas curadas del contrato de PB-T2c, que ya traen las de PB-T1 con sus clases, mas su clase de prueba
CORE_T1C = [f for f in pbt1c['ForbiddenWriteScope'] if f.startswith(TR)]
UI_T1C = [f for f in pbt1c['ForbiddenWriteScope'] if f.startswith(UT)]
T1C_NEW_CORE = [f for f in pbt1c['AllowedWriteScope'] if f.startswith(TR)]
T1C_NEW_UI = [f for f in pbt1c['AllowedWriteScope'] if f.startswith(UT)]
CORE_T0 = [f for f in pbt0['ForbiddenWriteScope'] if f.startswith(TR)] + [f for f in pbt0['AllowedWriteScope'] if f.startswith(TR)]
assert len(CORE_T1C) == 23 and CORE_T1C[:18] == CORE_T0 and len(UI_T1C) == 62 and UI_T1C[-1] == UT + 'Workspace/', (len(CORE_T1C), len(UI_T1C))
assert len(T1C_NEW_CORE) == 2 and len(T1C_NEW_UI) == 1, (T1C_NEW_CORE, T1C_NEW_UI)
CORE_T2C = [f for f in pbt2c['ForbiddenWriteScope'] if f.startswith(TR)]
UI_T2C = [f for f in pbt2c['ForbiddenWriteScope'] if f.startswith(UT)]
T2C_NEW_UI = [f for f in pbt2c['AllowedWriteScope'] if f.startswith(UT)]
# las de PB-T2c son las de PB-T1c (con sus clases) y las de PB-T2a y PB-T2b: las nueve clases de PB-T2, con la de PB-T2c
assert CORE_T2C[:25] == CORE_T1C + T1C_NEW_CORE and CORE_T2C[25:] == [TR + n + '.cs' for n in T2_CORE_NAMES], ('las listas de Core de PB-T2c', CORE_T2C[25:])
assert UI_T2C[:63] == UI_T1C[:-1] + T1C_NEW_UI + [UT + 'Workspace/'] and UI_T2C[63:] + T2C_NEW_UI == [UT + n + '.cs' for n in T2_UI_NAMES], (
    'las listas de UI de PB-T2c', UI_T2C[63:], T2C_NEW_UI)
CORE_T = CORE_T2C
UI_T = UI_T2C + T2C_NEW_UI
assert len(CORE_T) == 29 == len(set(CORE_T)) and len(UI_T) == 68 == len(set(UI_T)), (len(CORE_T), len(UI_T))
assert TR + 'Workspace/' in CORE_T and TR + 'Fixtures/' in CORE_T and UT + 'Workspace/' in UI_T  # Fixtures/I64/ cae bajo Fixtures/
assert all(t in CORE_T + UI_T for t in T2_TESTS), 'las nueve clases de PB-T2 en el ForbiddenWriteScope'
TESTF = CORE_T + UI_T
assert not any(a == t or (t.endswith('/') and a.startswith(t)) for a in ALLOWED for t in TESTF)
ROOT = [f for f in pbt1c['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))]
assert ROOT == ['docs/', '.github/', 'eng/', 'tools/', 'assets/', 'deploy/', 'RackCad.sln', 'Directory.Build.props', 'Directory.Build.targets', 'global.json',
                'NuGet.Config', 'README.md', 'AGENTS.md', 'CLAUDE.md', '.editorconfig', '.gitattributes', '.gitignore'], ROOT
assert ROOT == [f for f in pbt2c['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))], 'la raiz de PB-T2c'
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == len(set(FORBIDDEN)), 'duplicados en el ForbiddenWriteScope'
T2_MISSING = [t for t in T2_TESTS if not in_head(t)]  # las clases de PB-T2b y PB-T2c, que llegan con sus GREEN
for f in FORBIDDEN:
    if f not in T2_MISSING:
        assert in_head(f) and (not DISK or os.path.exists(f.rstrip('/'))), ('no existe', f)
soft(not T2_MISSING, 'cierres de PB-T2b y PB-T2c: clases de PB-T2 del ForbiddenWriteScope aun no en la revision: ' +
     ', '.join(os.path.basename(f) for f in T2_MISSING))

# ---------------------------------------------------------------- las anclas que usa PB-T3a en la revision (archivo, linea, texto contenido)
PM, IR = APP + 'Pilot/PushBackPilotMaterializer.cs', APP + 'Pilot/PushBackPilotIntentReader.cs'
PIR, PIS = APP + 'Pilot/PushBackPilotIntentResult.cs', APP + 'Pilot/PushBackPilotIntentScan.cs'
PP = APP + 'Pilot/PushBackPilotUpdatePlan.cs'
PES, PESC, PESO = APP + 'PushBackEditableState.cs', APP + 'PushBackEditableStateComparer.cs', APP + 'PushBackEditableStateOwnership.cs'
DRAFTS = 'src/RackCad.Application/Workspace/Drafts/'
WHC = 'src/RackCad.Application/Workspace/WorkspaceHostCore.cs'
SH = 'src/RackCad.Application/Systems/Shared/'
PERS = 'src/RackCad.Application/Persistence/'
RPC = 'src/RackCad.Plugin/RackPushBackCommands.cs'
ANCHORS = [
    # D-PBT3-03 y 04: el materializador del piloto, sus constantes (que no se duplican) y su rechazo del compuesto (PM:148)
    (PM, 10, 'public enum PushBackPilotMaterializationKind'), (PM, 13, 'Admitted,'), (PM, 16, 'OutOfPilot,'), (PM, 19, 'Failed,'),
    (PM, 100, 'public const string UnreadableOrFutureDiagnostic = "Ilegible o de versi'),
    (PM, 102, 'public const string KindDivergentDiagnostic = "El sobre no es un sistema Push Back.";'),
    (PM, 104, 'public const string NoDesignDiagnostic = "El sobre no trae un dise'),
    (PM, 106, 'public const string CompositeOutOfPilotReason = "Rack de dos lados (A/B): fuera del piloto.";'),
    (PM, 108, 'public const string DormantTailOutOfPilotReason = "El dise'),
    (PM, 110, 'public const string CatalogRequiredDiagnostic = "Se necesita el cat'),
    (PM, 112, 'public const string NonPilotContentReason = "El borrador cambia contenido que el piloto no edita: no se compone.";'),
    (PM, 114, 'public const string InvalidScaleReason = "La escala de anotaci'),
    (PM, 116, 'public const string BaseChangedReason = "La base ya no coincide al materializarla de nuevo: no se compone.";'),
    (PM, 118, 'public const string MaterializeFailurePrefix = "No se pudo materializar el dise'),
    (PM, 120, 'public const string ComposeFailurePrefix = "No se pudo componer el dise'),
    (PM, 126, 'public static PushBackPilotMaterialization Materialize(string originText, RackCatalog catalog)'),
    (PM, 148, 'if (design.IsComposite) return PushBackPilotMaterialization.OutOfPilot(CompositeOutOfPilotReason);'),
    (PM, 170, 'return PushBackPilotMaterialization.OutOfPilot(DormantTailOutOfPilotReason);'),
    (PM, 196, 'public static PushBackPilotComposition ComposeForWrite('),
    (PM, 201, 'if (initial == null || current == null) return PushBackPilotComposition.Rejected("Falta el estado inicial o el actual.");'),
    (PM, 242, 'public static PushBackPilotDraftState KeepPilotEditsOn(PushBackPilotDraftState newInitial, PushBackPilotDraftState current)'),
    # D-PBT3-04: el lector del piloto, sus constantes y el orden: AUTH-13 (reglas 10 y 11) antes de la materializacion (regla 12)
    (IR, 23, 'private const string NotEditable = "No editable aqu'),
    (IR, 35, 'public const string UnreadableDiagnostic = NotEditable + "ilegible o de versi'),
    (IR, 47, 'public const string OutOfPilotDiagnostic = NotEditable + "fuera del piloto (F5)";'),
    (IR, 53, 'public const string MaterializeDiagnostic = NotEditable + "no se pudo preparar el borrador";'),
    (IR, 56, 'public const string ClassicEditorHint = "Usa RACKEDITAR para editarlo con el editor cl'),
    (IR, 64, 'public static DraftStaleBase BuildStaleBase(PushBackPilotIntentScan scan)'),
    (IR, 75, 'public static PushBackPilotIntentResult Read(PushBackPilotIntentScan scan, RackCatalog? catalog)'),
    (IR, 217, 'var comparison = RackAuthoredComparatorPorts.PushBack()'),
    (IR, 221, 'if (comparison.Outcome != RackAuthoredComparisonOutcome.Single)'),
    (IR, 222, 'return Verdict.Refused(UnreadableDiagnostic, comparison.Diagnostic);'),
    (IR, 225, 'var materialized = PushBackPilotMaterializer.Materialize(scan.RawEnvelopes[scan.OriginKey], catalog!);'),
    (IR, 228, 'if (materialized.Kind == PushBackPilotMaterializationKind.OutOfPilot)'),
    (IR, 229, 'return Verdict.Refused(OutOfPilotDiagnostic, materialized.Reason);'),
    (PIR, 27, 'internal PushBackPilotIntentResult('), (PIR, 44, 'public DraftStaleBase StaleBase { get; }'),
    (PIR, 47, 'public DraftIntentRead<PushBackPilotDraftState> Read { get; }'), (PIR, 50, 'public string OriginKey { get; }'),
    (PIR, 53, 'public string? Detail { get; }'), (PIR, 56, 'public string? ClassicEditorHint { get; }'),
    (PIS, 24, 'public sealed class PushBackPilotIntentScan'), (PIS, 86, 'public string OriginKey { get; }'),
    (PIS, 92, 'public IReadOnlyDictionary<string, string> RawEnvelopes => _rawEnvelopes;'),
    # D-PBT3-04 y 05: el borrador generico (DS:469-486) y su registro (WHC:347-352, la receta de los borradores de 5.0)
    (DRAFTS + 'DraftSession.cs', 25, 'public sealed class DraftIntentRead<TState>'),
    (DRAFTS + 'DraftSession.cs', 44, 'public static DraftIntentRead<TState> Admitted(DraftStaleBase staleBase, TState initialState)'),
    (DRAFTS + 'DraftSession.cs', 50, 'public static DraftIntentRead<TState> NotAdmitted(string diagnostic) =>'),
    (DRAFTS + 'DraftSession.cs', 345, 'public DraftEditResult SetCurrentDraftState(TState state)'),
    (DRAFTS + 'DraftSession.cs', 469, 'public DraftReloadResult KeepMineOnCurrentVersion(DraftIntentRead<TState> read, Func<TState, TState, TState> keepOn, bool confirmed)'),
    (DRAFTS + 'DraftSession.cs', 482, 'var current = _copy(keepOn(_copy(read.InitialState!), _current));'),
    (DRAFTS + 'DraftSession.cs', 486, '}'),
    (DRAFTS + 'DraftStaleBase.cs', 57, 'public sealed class DraftStaleBase'),
    (DRAFTS + 'DraftRegistry.cs', 143, 'public DraftBeginResult<TState> Begin<TState>(SessionId session, string rackId, DraftIntentRead<TState> read, IEqualityComparer<TState> comparer, Func<TState, TState> copy)'),
    (WHC, 347, 'var begun = _drafts.Begin('), (WHC, 351, 'PushBackPilotStateComparer.Instance,'), (WHC, 352, 'PushBackPilotStateComparer.Copy);'),
    # D-PBT3-02, 03 y 05: el estado de PB-T1b (R-4), su copiador y su comparador, y el mapa (PESO:142 y :165-172)
    (PES, 16, 'public enum PushBackEditableSupport'), (PES, 23, 'DormantCompositeTail = 1'), (PES, 38, 'public sealed class PushBackEditableState'),
    (PES, 40, 'public PushBackEditableState()'), (PES, 55, 'public PushBackCompositeEditorState Editor { get; }'),
    (PES, 63, 'public double PostPeralte { get; set; }'), (PES, 64, 'public double PalletTolerance { get; set; }'),
    (PES, 65, 'public double BeamDepth { get; set; }'), (PES, 66, 'public double AnnotationScale { get; set; }'), (PES, 95, 'public string Name { get; set; }'),
    (PES, 173, 'public void SeedDefaultSafety(RackCatalog catalog)'), (PES, 180, 'public PushBackEditorInputs LoadNew(RackCatalog catalog)'),
    (PES, 189, 'public PushBackEditorInputs LoadDesign(PushBackDesign design, string rackName, PushBackResolver resolver)'),
    (PES, 367, 'public PushBackEditorInputs BuildInputs(PushBackEditorDesignAssembler assembler)'),
    (PES, 454, 'public static PushBackEditableSupport SupportOf(PushBackDesign design)'),
    (PESC, 29, 'public sealed class PushBackEditableStateComparer : IEqualityComparer<PushBackEditableState>'),
    (PESC, 31, 'public static PushBackEditableStateComparer Instance { get; }'), (PESC, 33, 'public bool Equals(PushBackEditableState x, PushBackEditableState y)'),
    (PESC, 45, 'public int GetHashCode(PushBackEditableState obj)'), (PESC, 63, 'public static PushBackEditableState Copy(PushBackEditableState state)'),
    (PESO, 25, 'public enum PushBackDocumentLevel'), (PESO, 28, 'Envelope = 0,'), (PESO, 31, 'Project = 1,'), (PESO, 34, 'Payload = 2'),
    (PESO, 142, 'rows.Add(new PushBackOwnershipRow(PushBackDocumentLevel.Envelope, "$.Name", PushBackFieldOwner.Editor, EditorSource));'),
    (PESO, 167, '"$.Structure", "$.Fronts", "$.LegacyHighEndBeamPeralte", "$.RearTopeSaque", "$.RearTopePieceId",'),
    (PESO, 168, '"$.DefensePieceId", "$.RearTopeOffCells", "$.SideB", "$.Composite",'),
    (PESO, 171, 'rows.Add(new PushBackOwnershipRow(PushBackDocumentLevel.Payload, path, PushBackFieldOwner.Editor, DesignAssemblers));'),
    (APP + 'PushBackEditorComputation.cs', 17, 'public sealed class PushBackEditorComputation'),
    (APP + 'PushBackEditorDesignAssembler.cs', 42, 'public PushBackEditorDesignAssembler(RackCatalog catalog)'),
    (APP + 'PushBackEditorDesignAssembler.cs', 84, 'public PushBackResolver Resolver => pushResolver;'),
    (APP + 'PushBackEditorDesignAssembler.cs', 466, 'public PushBackEditorComputation BuildFrom('),
    (APP + 'PushBackCompositeEditorAssembler.cs', 29, 'public PushBackCompositeEditorAssembler(RackCatalog catalog)'),
    (APP + 'PushBackCompositeEditorAssembler.cs', 38, 'public PushBackDesign BuildDesign('),
    ('src/RackCad.Domain/Systems/PushBack/PushBackSide.cs', 17, 'public enum PushBackSide'),
    ('src/RackCad.Domain/Systems/PushBack/PushBackSide.cs', 19, 'A = 0,'),
    # D-PBT3-06: la disponibilidad pura (RVA:94-116), el codec (RackViewCodec.cs:263-286) y los cortes, como el clasico (RPC:268-271, :320-333)
    (APP + 'PushBackSystemLateralBuilder.cs', 91, 'public IReadOnlyList<DynamicLateralCorte> Cortes(PushBackSystem system, RackCatalog catalog)'),
    (SH + 'RackViewAvailability.cs', 8, 'public enum RackViewAvailabilityStatus'), (SH + 'RackViewAvailability.cs', 10, 'Available,'),
    (SH + 'RackViewAvailability.cs', 94, 'public sealed class PushBackViewAvailabilityFacts : RackViewAvailabilityFacts'),
    (SH + 'RackViewAvailability.cs', 99, 'public PushBackViewAvailabilityFacts(IEnumerable<int> postIndexes, bool isComposite)'),
    (SH + 'RackViewAvailability.cs', 113, 'return address.Variant.PushBackSide == RackPushBackSide.A || isComposite;'),
    (SH + 'RackViewAvailability.cs', 116, '}'), (SH + 'RackViewAvailability.cs', 118, 'public sealed class CantileverViewAvailabilityFacts'),
    (SH + 'RackViewAvailability.cs', 136, 'public static RackViewAvailabilityResult Evaluate('),
    (SH + 'RackViewCodec.cs', 115, 'public static DecodedRackView Decode(string kind, string view, int section)'),
    (SH + 'RackViewCodec.cs', 263, 'private static DecodedRackView DecodePushBack(RackViewSyntax syntax, RackSystemKind kind)'),
    (SH + 'RackViewCodec.cs', 265, 'if (Token(syntax.View, "planta", out var exact) && syntax.Section == -1)'),
    (SH + 'RackViewCodec.cs', 271, 'if (Token(syntax.View, "lateral", out exact) && syntax.Section >= 0)'),
    (SH + 'RackViewCodec.cs', 277, 'if (Token(syntax.View, "frontal", out exact) && syntax.Section >= 0 && syntax.Section <= 3)'),
    (SH + 'RackViewCodec.cs', 285, 'return Invalid(syntax);'), (SH + 'RackViewCodec.cs', 286, '}'),
    (RPC, 269, 'var lateralCortes = new PushBackSystemLateralBuilder().Cortes(system, catalog);'),
    (RPC, 270, 'var availabilityFacts = new PushBackViewAvailabilityFacts('),
    (RPC, 271, 'lateralCortes.Select(item => item.PostIndex), system.IsComposite);'),
    (RPC, 325, 'var availability = RackViewAvailability.Evaluate(decoded, availabilityFacts);'),
    (RPC, 331, 'staleViewBlocks.Add(viewBlock.BlockId);'),
    # D-PBT3-03 y 07: el sobre, el proyecto y su persistencia
    (PERS + 'RackEmbedDocument.cs', 20, 'public const string KindPushBack = "pushback";'), (PERS + 'RackEmbedDocument.cs', 38, 'public string Kind { get; set; }'),
    (PERS + 'RackEmbedDocument.cs', 41, 'public string View { get; set; }'), (PERS + 'RackEmbedDocument.cs', 47, 'public int Section { get; set; } = -1;'),
    (PERS + 'RackEmbedDocument.cs', 50, 'public string Id { get; set; }'), (PERS + 'RackEmbedDocument.cs', 53, 'public string Name { get; set; }'),
    (PERS + 'RackEmbedDocument.cs', 56, 'public string Design { get; set; }'), (PERS + 'RackEmbedDocument.cs', 97, 'public RackEmbedDocument Deserialize(string json)'),
    (PERS + 'RackProjectStore.cs', 43, 'public string Serialize(RackProject project)'),
    (PERS + 'RackProjectStore.cs', 126, 'public bool TryDeserialize(string json, out RackProject project, out bool incompatibleMajor)'),
    (PERS + 'RackProject.cs', 130, 'public static RackProject ForPushBack(PushBackDesign design, PushBackSystem system = null)'),
    # D-PBT3-05 (paso 5): el plan compone sobre el sobre releido (PP:421-422, :434-438); D-PBT3-07: una hoja derivada (DynamicRackLevelGeometry.cs:269)
    (PP, 421, 'var designText = store.Serialize(RackProject.ForPushBack(design).WithSourceMetadataFrom(preflight.ResolvedSources[index]));'),
    (PP, 422, 'plans.Add(new PushBackPilotMemberPlan(entry.Key, view, section, designText, ComposeMemberEnvelope(embed, view, section, designText)));'),
    (PP, 434, 'private static string ComposeMemberEnvelope(RackEmbedDocument source, string view, int section, string designJson)'),
    (PP, 436, 'var composed = RackEmbedComposer.Compose(source, RackEmbedDocument.KindPushBack, source.Id, source.Name, view, section, designJson);'),
    ('src/RackCad.Application/Systems/Dynamic/DynamicRackLevelGeometry.cs', 269, 'palletFront, Math.Max(1, palletCount), design?.PalletTolerance ?? 0.0));'),
    # las pruebas que se usan sin editarlas (5.0, 5.2 y R-16): I-58, I-54 G4B, los kits del piloto, CT-PB0-10, S0, PB-T1b
    (TR + 'I58F1Matrix.cs', 58, 'foreach(string value in new[]{"null","{}","[]"}) {'),
    (TR + 'I58F1Matrix.cs', 59, 'Add("unknown-wrapper-"+value,O.Unreadable,a,Set(a,"Future",value),"CT58-15","CT58-27");'),
    (TR + 'I58F1Matrix.cs', 60, 'Add("unknown-payload-"+value,O.Unreadable,a,Set(a,root+".Future",value),"CT58-15","CT58-27");'),
    (TR + 'I58F1Matrix.cs', 64, 'foreach(string path in Nested(kind)) Add("unknown-"+path,O.Unreadable,a,Set(a,path+".FutureNested","1"),"CT58-16","CT58-27");'),
    (TR + 'I58F1Matrix.cs', 97, 'private static IEnumerable<string> Nested(string k)'),
    (TR + 'I58F1Fixtures.cs', 30, 'internal static class I58F1Fixtures'),
    (TR + 'I58F1Fixtures.cs', 114, 'public static string Set(string raw,string path,string json) {'),
    (TR + 'I58F1Fixtures.cs', 125, 'public static string Rich(string kind)'),
    (TR + 'CustomPropertiesEnvelopeGuardTests.cs', 194, 'internal static readonly IReadOnlyList<ComposeCall> ExpectedComposeCensus = new[]'),
    (TR + 'CustomPropertiesEnvelopeGuardTests.cs', 196, 'new ComposeCall("src/RackCad.Application/Systems/PushBack/Pilot/PushBackPilotUpdatePlan.cs", "ComposeMemberEnvelope", "source"),'),
    (TR + 'Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs', 15, 'internal sealed record IntentItem(RackSiblingScanFact Fact, string? Text);'),
    (TR + 'Workspace/WorkspaceDraftPilotIntentStaleBaseTests.cs', 21, 'internal static class IntentKit'),
    (TR + 'Workspace/WorkspaceDraftPilotStateTests.cs', 30, 'internal static class PilotKit'),
    (TR + 'Workspace/WorkspaceDraftPilotStateTests.cs', 98, 'public static string Persist(PushBackDesign design)'),
    (TR + 'Workspace/WorkspaceDraftPilotStateTests.cs', 106, 'public static List<string> JsonDiff(string before, string after)'),
    (TR + 'Workspace/WorkspaceDraftSessionTests.cs', 134, 'internal static class DraftKit'),
    (TR + 'Workspace/WorkspacePilotUpdateScopeDouble.cs', 19, 'internal sealed class PilotUpdateScopeDouble : IPushBackPilotUpdatePort'),
    (TR + 'PushBackFullEditorCharacterizationLateralCutTests.cs', 23, 'public sealed class PushBackFullEditorCharacterizationLateralCutTests'),
    (UT + 'PushBackFullEditorCharacterizationSupport.cs', 108, 'internal static string EditInner(string envelopeText, Action<JsonNode> edit)'),
    (UT + 'PushBackFullEditorCharacterizationSupport.cs', 122, 'internal static string Persist(PushBackDesign design)'),
    (TR + 'PushBackEditableStateLoadTests.cs', 74, 'private static string EditInner(string envelopeText, Action<JsonNode> edit)'),
    (TR + 'PushBackEditableStateLoadTests.cs', 86, 'private static PushBackEditorComputation Pasada('),
    (TR + 'PushBackSharedReferenceGuardTests.cs', 60, 'private static List<string> Rule1b(string text, IEnumerable<string> tokens)'),
    (TR + 'PushBackSharedReferenceGuardTests.cs', 167, 'result.AddRange(Rule1b(member, aliases.Keys).Select(hit => "R2b " + hit));'),
    # [PKT1] v2.8 custodiada: PBT1C-C13 (:1338), la forma de C13 que aplica el paquete (V3-02)
    ('docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md', 1338, '- **PBT1C-C13:** keeping a gesture without observable change'),
    # [PKC2]: las paradas estandar (section 6.7, :579-581) y las celdas (section 6.8, :655)
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 579, '### 6.7 `StopConditions`'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 581, 'S-02, S-03, S-04, S-05, S-07, S-08, S-12, S-13 y S-14, y P-01 a P-08'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 655, 'vencen el 2026-12-29'),
]
TEXT = {}
for path, line, text in ANCHORS:
    if path not in TEXT:
        TEXT[path] = lines_of(path)
    assert text in TEXT[path][line - 1].strip(), ('ancla', path, line, TEXT[path][line - 1].strip())
assert TEXT[SH + 'RackViewAvailability.cs'][115].strip() == '}' and TEXT[SH + 'RackViewCodec.cs'][285].strip() == '}'
# los textos de CT-PBT3A-04 son las constantes del lector: NotEditable + UnreadableDiagnostic y ClassicEditorHint
_ir = '\n'.join(TEXT[IR])
CT04_DIAG = re.search(r'private const string NotEditable = "([^"]*)";', _ir).group(1) + \
    re.search(r'public const string UnreadableDiagnostic = NotEditable \+ "([^"]*)";', _ir).group(1)
CT04_HINT = re.search(r'public const string ClassicEditorHint = "([^"]*)";', _ir).group(1)

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


# Las anclas de W (D-PBT3-03, CT-PBT3A-01; v4.2, V42-10), POR SU CONTENIDO y medidas sobre la revision, no frente a una linea del paquete.
# El contenido de referencia sale del cierre de PB-T1c (INV-PBT2B-04 congela Recompute, LoadFromModel y ReadInputs desde alli), buscado por texto.
def unique_line(lines, text, what):
    hits = [i for i, l in enumerate(lines) if text in l]
    assert len(hits) == 1, ('ancla de W no unica o ausente', what, len(hits))
    return hits[0]


def member_span(lines, decl, what):
    """(inicio, fin) 0-based, ambos incluidos, del miembro de W cuya declaracion contiene decl (unica)."""
    i = unique_line(lines, decl, what)
    return i, member_end(lines, i + 1) - 1


def braces_end(lines, i):
    """La linea (0-based) que cierra el bloque que se abre en o tras la linea i."""
    depth, opened = 0, False
    for k in range(i, len(lines)):
        code = code_of(lines[k])
        depth += code.count('{') - code.count('}')
        opened = opened or '{' in code
        if opened and depth == 0:
            return k
    raise AssertionError(('bloque de W sin cierre', i))


def w_blocks(lines):
    """Los bloques de las anclas de W en un texto de W: {nombre: (inicio, fin)} 0-based."""
    a0 = unique_line(lines, 'computation = assembler.BuildFrom(', 'la adopcion de Recompute')
    a1 = next(k for k in range(a0, len(lines)) if 'RefreshModuleSelector();' in lines[k])
    c0 = next(k for k in range(a0, a1) if 'catch (Exception error)' in lines[k])
    s0 = unique_line(lines, 'assembler = new PushBackEditorDesignAssembler(catalog);', 'los ensambladores')
    assert 'compositeAssembler = new PushBackCompositeEditorAssembler(catalog);' in lines[s0 + 1], 'los dos ensambladores, juntos'
    l0, l1 = member_span(lines, 'private void LoadFromModel(PushBackEditorInputs inputs, string rackName)', 'LoadFromModel')
    last = max(k for k in range(l0, l1) if lines[k].strip())
    assert lines[last].strip() == 'Recompute();' and lines[l1].strip() == '}', 'el Recompute(); final de LoadFromModel'
    return {'adopcion': (a0, a1), 'catch': (c0, braces_end(lines, c0)), 'ensambladores': (s0, s0 + 1),
            'LastComputation': 2 * (unique_line(lines, 'internal PushBackEditorComputation LastComputation => lastComputation;', 'LastComputation'),),
            'LoadExisting': 2 * (unique_line(lines, 'public void LoadExisting(PushBackDesign design, string rackId, string rackName, '
                                                   'RackProject sourceProject = null)', 'LoadExisting'),),
            'costura': 2 * (unique_line(lines, SEAM_GREEN, 'la costura'),),
            'Recompute(); final de LoadFromModel': (last, last)}


_w1c = lines_of(W, PT1C_CLOSE)
_W1C, _WREV = w_blocks(_w1c), w_blocks(_wrev)
W_REPORT = []
for _n, (_a, _b) in _WREV.items():
    _ra, _rb = _W1C[_n]
    assert _wrev[_a:_b + 1] == _w1c[_ra:_rb + 1], ('el ancla de W cambio de contenido desde el cierre de PB-T1c', _n)
    W_REPORT.append('%s W:%d%s' % (_n, _a + 1, ('-%d' % (_b + 1)) if _b > _a else ''))
# la adopcion y su catch son los que midio el paquete en el cierre de PB-T1b (W:659-704 y W:662-670)
_w1b = lines_of(W, PB1B_CLOSE)
assert _w1c[_W1C['adopcion'][0]:_W1C['adopcion'][1] + 1] == _w1b[658:704] and _w1c[_W1C['catch'][0]:_W1C['catch'][1] + 1] == _w1b[661:670], \
    'la adopcion de Recompute o su catch no son los que cita el paquete'
# INV-PBT2B-04: Recompute, LoadFromModel y ReadInputs, con el mismo cuerpo que en el cierre de PB-T1c
for _decl in ('private void Recompute()', 'private void LoadFromModel(PushBackEditorInputs inputs, string rackName)', 'private PushBackEditorInputs ReadInputs()'):
    _x0, _x1 = member_span(_wrev, _decl, _decl)
    _y0, _y1 = member_span(_w1c, _decl, _decl)
    assert _wrev[_x0:_x1 + 1] == _w1c[_y0:_y1 + 1], ('el cuerpo cambio desde el cierre de PB-T1c (INV-PBT2B-04)', _decl)


# D-PBT3-03: Pass es la copia literal del ayudante Pasada de [PKT1] section 5.0; el de las pruebas de PB-T1b es el mismo
PKT1 = 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md'
_pk1 = lines_of(PKT1)
assert [l for l in _pk1[:6] if l.startswith('# I-64 F4B')][0].rstrip().endswith(', v2.8'), 'el paquete de PB-T1 custodiado no es la v2.8'
_k = [i for i, l in enumerate(_pk1) if l.startswith('**El ayudante `Pasada`**')]
assert len(_k) == 1 and _pk1[_k[0] + 2] == '```csharp', 'el ayudante Pasada de [PKT1] section 5.0'
_e = _pk1.index('```', _k[0] + 3)
PASADA_PKT1 = norm_code(_pk1[_k[0] + 3:_e])
_lt = TEXT[TR + 'PushBackEditableStateLoadTests.cs']
PASADA_TESTS = norm_code(_lt[85:member_end(_lt, 86)])
assert PASADA_PKT1 == PASADA_TESTS and PASADA_PKT1.startswith('private static PushBackEditorComputation Pasada( PushBackEditableState s,'), \
    ('el Pasada de [PKT1] section 5.0 y el de PushBackEditableStateLoadTests difieren', PASADA_PKT1, PASADA_TESTS)
PASADA_STMTS = PASADA_PKT1.count(';')

# CT-PBT3A-04 (D-PBT3-13; R-16): las 28 rutas anidadas de I58F1Matrix.Nested("pushback"), 20 bajo PushBack.Structure y 8 de Push Back
_m = TEXT[TR + 'I58F1Matrix.cs']
assert _m[100].strip().startswith('foreach(string p in new[]{"Fronts.0"') and _m[101].strip().startswith('if(k=="pushback")foreach(string p in new[]{')
NESTED_STRUCTURE = re.findall(r'"([^"]+)"', re.search(r'new\[\]\{([^}]*)\}', _m[100]).group(1))
NESTED_PUSHBACK = re.findall(r'"([^"]+)"', re.search(r'new\[\]\{([^}]*)\}', _m[101]).group(1))
assert len(NESTED_STRUCTURE) == 20 and NESTED_PUSHBACK == ['PushBack.Structure', 'PushBack.Fronts.0', 'PushBack.SideB', 'PushBack.SideB.Fronts.0',
                                                           'PushBack.SideB.FrontConfigs.0', 'PushBack.Composite', 'PushBack.Composite.Topologies.0',
                                                           'PushBack.RearTopeOffCells.0'], (NESTED_STRUCTURE, NESTED_PUSHBACK)
assert 'private static IEnumerable<string> Nested' in _m[96], 'Nested ya no es privado: CT-PBT3A-04 copia la lista porque lo es'
# el corpus de Fixtures/I64 (5.0): los 17, con los tres que nombran los CT, sin pb-* nuevos
FIXTURES = sorted(p[len(TR + 'Fixtures/I64/'):] for p in tracked(TR + 'Fixtures/I64/'))
assert len(FIXTURES) == 17 and all(n.startswith('pb-') and n.endswith('.json') for n in FIXTURES), FIXTURES
assert {'pb-01-base.json', 'pb-20-ab.json', 'pb-21-cola.json'} <= set(FIXTURES)
# los nombres nuevos de D-PBT3-02 a D-PBT3-07 no existen en src/ ni en tests/; el namespace nuevo tampoco
NEW_TYPES = ['PushBackFullDraftState', 'PushBackFullDraftStateComparer', 'PushBackEditableStateSeat', 'PushBackFullDraftMaterialization',
             'PushBackFullDraftMaterializer', 'PushBackFullIntentResult', 'PushBackFullDraftReader', 'PushBackFullWriteComposition',
             'PushBackFullComposition', 'PushBackViewDispositionKind', 'PushBackViewDispositionPlan', 'PushBackViewDispositions',
             'PushBackChangeGroup', 'PushBackChangeClass', 'PushBackChangeEntry', 'PushBackConflictClassification', 'PushBackConflictClassifier']
_gg = git('grep', '-l', '-w', *sum([['-e', n] for n in NEW_TYPES], []), REV, '--', 'src', 'tests')
assert _gg.returncode == 1 and not _gg.stdout.strip(), ('un nombre nuevo ya existe', _gg.stdout.decode('utf-8')[:300])
_gn = git('grep', '-l', '-F', '-e', 'RackCad.Application.Systems.PushBack.Draft', REV, '--', 'src', 'tests')
assert _gn.returncode == 1, 'el namespace RackCad.Application.Systems.PushBack.Draft ya existe'

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

# ---------------------------------------------------------------- el paquete: los textos de la v4, literales en ASCII y sin sus notas


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
MEAS ='docs/initiatives/I-64-f4b-pbt3-measurements.md'
REVIEW = D3 + 'independent-review-v1.md'
ANALYSIS = PILOT + '/F4B-PBT3A-FULL-WRITE-MODEL/1/' + RUN1 + '/analysis.md'  # el analisis de la sesion (F4B-E42), custodiado en el commit X
if PKG_FILE:
    PKG_RAW, PKG_SRC = open(PKG_FILE, 'rb').read(), '--package-file ' + os.path.basename(PKG_FILE)
elif in_head(PKG):
    PKG_RAW, PKG_SRC = show_bytes(PKG), PKG + ' en ' + REV_SHA[:8]
else:
    PKG_RAW, PKG_SRC = None, 'ninguno (no esta custodiado en la revision ni se dio --package-file)'
PKG_LINES = PKG_RAW.decode('utf-8').replace('\r\n', '\n').split('\n') if PKG_RAW else []
_title = [l for l in PKG_LINES[:6] if l.startswith('# I-64 F4B')]
PKG_IN = re.search(r', (v\d+(?:\.\d+)?)\s*$', _title[0]).group(1) if len(_title) == 1 else None
V4 = PKG_IN == PKG_VERSION
soft(V4, 'emision: el paquete leido (%s) es la %s: no se comprueban sus textos de la %s (en seco, --package-file con la v4)' % (PKG_SRC, PKG_IN, PKG_VERSION))


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
def rr_pkg_check(sec_lines, classes):
    """La regla RR en el section 6.0 del paquete (V43-01 a V43-06): el parrafo de F4B-E35, RR-1 a RR-6 una vez cada una, las dos lecturas de PB-T3,
    la CI del RED reparado, la verificacion con una reparacion y la tabla cerrada de la ayuda de RR-3, cuyas clases de PB-T3a son exactamente las
    seis nuevas (classes). La tabla es texto del paquete, no vinetas: se lee aparte. Devuelve los errores (vacia si vale)."""
    import re
    o = chr(0xF3)
    t = '\n'.join(sec_lines)
    errors = []
    if t.count('**La reparaci' + o + 'n del RED** (F4B-E35, pregunta 4') != 1:
        errors.append('falta, una sola vez, el parrafo de la regla RR (F4B-E35, pregunta 4)')
    for k in range(1, 7):
        if sum(1 for l in sec_lines if l.startswith('- **RR-%d, ' % k)) != 1:
            errors.append('RR-%d no esta una sola vez' % k)
    if sum(1 for l in sec_lines if l.startswith('- **Las dos lecturas de PB-T3, porque los tres RED ya tocan `src/`**')) != 1:
        errors.append('faltan las dos lecturas de PB-T3 (V43-01)')
    if sum(1 for l in sec_lines if l.startswith('- **la CI del RED reparado** (F4B-E35, RR-4; V43-04)')) != 1:
        errors.append('falta la CI del RED reparado (V43-04)')
    if t.count('**La verificaci' + o + 'n con una reparaci' + o + 'n del RED** (F4B-E35, RR-7') != 1:
        errors.append('falta la verificacion con una reparacion del RED (V43-05)')
    rows = [l for l in sec_lines if re.match(r'^\| PB-T3[abc] \| `', l)]
    t3a = [re.match(r'^\| PB-T3a \| `(\w+)`', l).group(1) for l in rows if l.startswith('| PB-T3a |')]
    if sorted(t3a) != sorted(classes):
        errors.append('la tabla cerrada de RR-3 no nombra exactamente las seis clases nuevas de PB-T3a: %s' % ascii(t3a))
    if not [l for l in rows if l.startswith('| PB-T3b |')] or not [l for l in rows if l.startswith('| PB-T3c |')]:
        errors.append('la tabla cerrada de RR-3 sin filas de PB-T3b o de PB-T3c')
    return errors
# ---- rr_pkg_check (fin)


# ---- rr3_table_check (inicio)
def rr3_table_check(sec_lines, classes, read_ref):
    """La tabla cerrada de la ayuda de RR-3 (v4.4, V44-02 y V44-03; CA-T3a M-02 y M-03), en sus filas de PB-T3a: una fila por cada una de las
    seis clases nuevas (classes) y ninguna otra; cada fila, partida por <br>, en partes que empiezan por '**Lineas:**' o '**Valores:**' (con
    'CT-nn, ' delante si hace falta) y que dan sus literales entre comillas invertidas, sin las formas que CA-T3a M-03 no podia enumerar; los
    valores, contables como token (una cadena entre comillas dobles o un numero de dos o mas caracteres); y cada referencia a codigo
    ('Archivo.cs:a-b' en 05beabfd), un archivo unico de src/ o tests/ con esas lineas, que read_ref(nombre) devuelve (None si no hay uno solo).
    Devuelve (errores, info): info[clase] = (lineas, valores, {etiqueta de la parte: literales que estan en las lineas referidas}), donde un
    literal 'esta' si es un identificador con limites de palabra o, si no, si esta entre comillas dobles (o tal cual, si ya las lleva)."""
    import re, unicodedata
    fold = lambda s: ''.join(c for c in unicodedata.normalize('NFKD', s) if not unicodedata.combining(c))
    vague = ('los casos y sus constantes', 'que copia', 'los nombres de ct-', 'los terminos prohibidos de ct-')
    errors, info, seen = [], {}, []
    for row in [l for l in sec_lines if l.startswith('| PB-T3a | `')]:
        cells = row.strip()[2:-2].split(' | ') if row.strip().endswith(' |') else []
        cm = re.match(r'`(\w+)` \(CT-', cells[1]) if len(cells) == 3 else None
        if not cm:
            errors.append('una fila de PB-T3a de la tabla de RR-3 sin la forma "| PB-T3a | `Clase` (CT-...) | ... |": %s' % ascii(row[:80]))
            continue
        cls, body = cm.group(1), cells[2]
        seen.append(cls)
        if [v for v in vague if v in fold(body).lower()]:
            errors.append('la fila de %s no se enumera sin interpretar: %s' % (cls, [v for v in vague if v in fold(body).lower()]))
        n_lines = n_vals = 0
        anchored = {}
        for seg in [s.strip() for s in body.split('<br>')]:
            lm = re.match(r'\*\*((?:CT-\d\d, )?)([LlVv])(?:ineas|alores):\*\* ', fold(seg))
            if not lm:
                errors.append('una parte de la fila de %s no empieza por "Lineas:" o "Valores:": %s' % (cls, ascii(fold(seg)[:70])))
                continue
            label = lm.group(1) + ('valores' if lm.group(2) in 'Vv' else 'lineas')
            # los literales entre comillas invertidas; los que van entre parentesis (contados fuera de las comillas) son referencias o
            # explicaciones, como '(el de `SetGap`)' o '(`Archivo.cs:a-b` en `05beabfd`)', no literales de la lista
            parts, depth, lits, outer = seg.split('`'), 0, [], []
            for k, part in enumerate(parts):
                if k % 2:
                    lits.append(part)
                    if depth == 0:
                        outer.append(part)
                else:
                    depth = max(0, depth + part.count('(') - part.count(')'))
            refs = [x for x in lits if re.fullmatch(r'\w+\.cs:\d+-\d+', x) or re.fullmatch(r':\d+-\d+', x) or re.fullmatch(r'[0-9a-f]{8}', x)]
            items = [x for x in outer if x not in refs]
            if not items:
                errors.append('la parte "%s" de %s no da ningun literal' % (label, cls))
            if label.endswith('valores'):
                bad = [v for v in items if not (re.fullmatch(r'"[^"]+"', v) or (re.fullmatch(r'\d+(?:\.\d+)?', v) and len(v) >= 2))]
                if bad:
                    errors.append('valores de %s que no se pueden contar como token: %s' % (cls, ascii(bad)))
                n_vals += len(items)
            else:
                n_lines += len(items)
            src = None
            for r in refs:
                fm = re.fullmatch(r'(\w+\.cs):(\d+)-(\d+)', r) or re.fullmatch(r'():(\d+)-(\d+)', r)
                if not fm:
                    continue
                if fm.group(1):
                    src = read_ref(fm.group(1))
                    if src is None:
                        errors.append('la referencia %s de %s no es un archivo unico de src/ o tests/ en 05beabfd' % (r, cls))
                        continue
                elif src is None:
                    continue
                a, b = int(fm.group(2)), int(fm.group(3))
                if not 1 <= a <= b <= len(src):
                    errors.append('la referencia %s de %s cae fuera del archivo (%d lineas)' % (r, cls, len(src)))
                    continue
                text = '\n'.join(src[a - 1:b])
                hit = [x for x in items if (re.search(r'\b%s\b' % re.escape(x), text) if re.fullmatch(r'\w+', x)
                                            else (x in text if x.startswith('"') else ('"%s"' % x) in text))]
                anchored.setdefault(label, [])
                anchored[label] += [x for x in hit if x not in anchored[label]]
        info[cls] = (n_lines, n_vals, anchored)
    if sorted(seen) != sorted(classes):
        errors.append('las filas de PB-T3a de la tabla de RR-3 no son, una por clase, las seis clases nuevas: %s' % ascii(seen))
    return errors, info
# ---- rr3_table_check (fin)


# Los textos de la v4, en ASCII (md_fold) y sin las "Nota (no va al contrato)" (V2-31). El contrato los lleva tal cual.
OBJECTIVE = (
    'Crear en Application, solo con archivos nuevos en src/RackCad.Application/Systems/PushBack/Draft/, el modelo del borrador completo de Push Back sobre '
    'PushBackEditableState: el estado y su comparador, el asiento (la pasada y la secuencia de LoadExisting), el materializador y el lector sobre el '
    'lector del piloto (AUTH-13 intacto; A/B admitido solo en este modelo; la cola aparcada fuera), la composicion para escribir y para Conservar mi '
    'borrador, la disposicion de las vistas (lateral obsoleto, frontal B de un rack de un sentido, sin direccion, ultimo vinculo) y el clasificador de '
    'D-PB-05, sin conectarlos al panel, segun el paquete del contrato de PB-T3 v4.5 y la lista cerrada CT-PBT3A-01..14. El RED es el esqueleto inerte y '
    'las pruebas nuevas, que fallan por asercion salvo las tres guardas; el GREEN implementa solo los archivos nuevos. Si hiciera falta mutar el rack '
    'resuelto o el informe de modulos dentro del tiempo de vida de la edicion o del borrador: STOP y se reabre L-1. El piloto, el anfitrion, la '
    'ventana, el Plugin y Domain no cambian; no se ejecuta AutoCAD.')
SKELETON = (
    'El esqueleto exacto del RED: PushBackFullDraftState, completo (es un contenedor); el comparador: su constructor privado; Instance; Equals = '
    'ReferenceEquals; GetHashCode = 0; Copy = new PushBackFullDraftState(new PushBackEditableState(), state.Origin); el asiento: Pass devuelve null; '
    'LoadExisting devuelve new PushBackEditableState(); Materialize devuelve Failed(PushBackPilotMaterializer.UnreadableOrFutureDiagnostic); Read '
    'devuelve NotAdmitted(PushBackPilotIntentReader.MaterializeDiagnostic), con la base de PushBackPilotIntentReader.BuildStaleBase(scan); ForFullWrite '
    'devuelve Rejected(PushBackPilotMaterializer.NonPilotContentReason); KeepOn devuelve current; Of da todo Redraw y AllObsolete falso; Classify da '
    'todas las listas vacias y AllViewsObsolete falso. Los tipos, enumerados, constructores y propiedades, completos.')
INV_OWN = [
    'INV-PBT3A-01. Alcance: el diff dentro de las 14 rutas; git diff BaseSha..CurrentSha -- src solo anade los ocho archivos de Draft/ y no modifica '
    'ningun archivo existente.',
    'INV-PBT3A-02. RED y GREEN: el RED es el esqueleto exacto y las seis clases de prueba; el GREEN solo toca los ocho archivos de Draft/. En el RED, '
    'cada filtro con ExpectRed da todos Failed por asercion (la primera linea del StackTrace bajo tests/), con Failed == Selected; el filtro de las '
    'guardas pasa y sus autopruebas fallan sobre el texto inyectado. Con una reparacion del RED (F4B-E35, RR; V43-03): RedSha..RedRepairSha solo toca '
    'clases de prueba que anadio el RED y ningun archivo de src/; el criterio se repite entero sobre RedRepairSha; y RedRepairSha..CurrentSha es el '
    'GREEN de arriba.',
    'INV-PBT3A-03. La API: exactamente la de D-PBT3-02 a D-PBT3-07, con sus constructores.',
    'INV-PBT3A-04. Un solo namespace RackCad.Application.Systems.PushBack.Draft de bloque por archivo; ningun archivo nuevo compone, construye un '
    'RackEmbedDocument ni nombra lo prohibido por D-PBT3-08.',
    'INV-PBT3A-05. El lector completo llama a PushBackPilotIntentReader.Read exactamente una vez por lectura; el materializador completo llama a '
    'PushBackEditableStateSeat.LoadExisting exactamente una vez.',
    'INV-PBT3A-06. Ningun archivo de src/ fuera de Draft/, PP y UR nombra los tipos nuevos (CT-12; V2-28).',
]
INV_COMMON = [  # los invariantes comunes del section 6.0, que cada contrato numera INV-PBT3x-07 a 14
    ('07', '07. Las redes: section 5.1 en verde. Ninguna prueba existente se edita, se salta, se reactiva ni se borra, con dos unicas salvedades, las '
           'dos en PB-T3b (V3-03): los Skip de D-1, con su motivo literal; la reactivacion temporal, local y sin commit de esos mismos metodos para la '
           'medicion de INV-PBT3B-15: el Worker quita el Skip, corre los metodos y lo vuelve a poner antes del commit GREEN. En CurrentSha, el git diff '
           '-U0 BaseSha..CurrentSha de los archivos de guardas solo lleva las lineas de atributo con Skip.'),
    ('08', '08. Las pruebas de UI: las reglas de section 5.0.'),
    ('09', '09. Determinismo: GUID fijo, cultura invariante, "R" y orden ordinal; ni hora ni aleatoriedad.'),
    ('10', '10. Las corridas locales: solo los filtros del contrato en UI; nunca la suite completa de UI en local; la de Core, si; sin copias de '
           'arboles; las corridas temporales terminan en Assert.Fail(string.Join("\\n", lineas)), corren dentro de EditorDiscardPrompt.Substitute '
           'cuando abren ventanas, no entran en ningun commit y guardan TRX y log como artefactos.'),
    ('11', '11. La evidencia: rutas relativas comprobadas en worker-handoff.json; TreeSha real o null; el Trailer, el real; el Worker no escribe en docs/.'),
    ('12', '12. Las reglas de F4B; sin AutoCAD; la verificacion con las tres comprobaciones deterministas de F4B-E15.'),
    ('13', '13. Las declaraciones nuevas en src/: las de la lista cerrada de la parada C13 de cada contrato, y ninguna mas (V3-02). Una sola regla por '
           'contrato: PB-T3a (solo archivos nuevos): la lista de PBT3A-C13 y, ademas, estos ayudantes privados en los ocho archivos nuevos: metodos '
           'privados; tipos anidados privados sin estado; y campos private static readonly de un tipo inmutable (Regex o string); PB-T3b y PB-T3c '
           '(archivos existentes): solo la lista de PBT3B-C13 o PBT3C-C13, por nombre y acotada a los archivos que tocan, sobrecargas incluidas. No se '
           'admite ningun ayudante privado fuera de ella.'),
    ('14', '14. D2: fuera de las dos referencias de cada lado, ninguna copia comparte un objeto mutable con su original. Ninguna linea anadida muta un '
           'PushBackSystem ni un RackModuleReconciliationResult adoptado.'),
]
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
# PBT3A-C11 (V43-09): el bloque citado del section 6.0, literal (el texto de PBT2C-C11 con el prefijo cambiado, salvo la salida de nc2, con el
# sustituto adaptado a F4B de F4B-E34, y la lista de las aceptaciones puntuales de nc1)
C11_PKG = (
    'a negative control (nc1, nc2 or nc3) does not give CONTROL PASS: its ordinary output is kept as is and there are only two exits without returning '
    'to the Owner ([DP] question 1, "Extender firmas exactas"): (1) nc2 with the EXACT signature of PD-F4-01 (source f4-dispositions.md:1103-1106: '
    'Scope in fail on the removed path; Contract in fail only by the exactness clause; EXECUTION_BLOCKED/STOP; applied literally at :1540: no check '
    'other than the real verification differs; the exactness clause includes Contract evidence citing the contrast of the narrowed AllowedWriteScope '
    'with the contract or with byte-fidelity-check.json, f4-dispositions.md:520, :562, and nothing else) -> substitute PBT3A-NC2-EXACT-WHITELIST-F4B-01, '
    'a copy of PBT2A-NC2-EXACT-WHITELIST-F4B-01 (F4B-E34: ' + SUBST_F4B + ', SHA-256 ' + SUBST_F4B_SHA + '; owner-pbt2a-controls-decisions.md, '
    'question 2) with ONLY the id and the scope changed, run in the F4B-E21 mode with the injection note and the raw output of this task\'s accepted '
    'planning, recorded as nc2 = SATISFIED_UNDER_PBT3A_EXACT_WHITELIST_SUBSTITUTE_F4B; if inject_opaque.py changed since that run, the substitute is '
    'reviewed before it is reused; (2) nc1 with the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265: false CurrentSha '
    'correctly detected; Identity = fail; Handoff also fails first only because the handoff itself contains the false SHA; outcome BLOCKED / STOP; all '
    'other intended checks match; as applied in F4, f4-dispositions.md:1657-1660, Ci in fail only as a consumer of the false CurrentSha is part of the '
    'signature, with Remote in pass) -> substitute PBT3A-NC1-IDENTITY-SIGNAL-01, a copy of F4T3C2-NC1-IDENTITY-SIGNAL-01 with ONLY the id and the scope '
    'changed, recorded as nc1 = SATISFIED_UNDER_PBT3A_NC1_IDENTITY_SIGNAL_SUBSTITUTE. Matching is decided never by check name: the control output is '
    'compared with the real verification check by check (the set of failing checks, the first failing check, Outcome and StopClass, and the evidence '
    'text); the only admitted differences are those listed for each signature in section 6.0 of the PB-T1 contract package ([PT1], '
    'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md); the session records the comparison table in f4b-dispositions.md. Any other signature or '
    'variation (PD-F4-05, PD-F4-06, nc2 with Ci or Trailer also in fail, any nc1 that does not reproduce the literal signature - including the '
    'variation the Owner accepted only once in PB-TC (F4B-E08), in PB-T2a (F4B-E34) and in PB-T2b (F4B-E38), and the PB-TC2 case, where the substitute '
    'failed its property 5 and the Owner accepted nc1 only for PB-TC2 attempt 1 (F4B-E12) -, a substitute that does not give PASS, any failure of '
    'nc3, or an evidence text that cannot be assigned to an admitted difference) returns to the Owner before any substitute or acceptance; no '
    'one-off acceptance extends to PB-T3a')
STOP_COMMON = [  # las propias comunes del section 6.0, con el prefijo de cada contrato: (id, rotulo del paquete, texto)
    ('C01', 'doing the task would require writing outside the AllowedWriteScope.'),
    ('C02', 'it would require editing, skipping, re-enabling or deleting an existing test or helper, or touching InternalsVisibleTo, the packages, a '
            '.csproj or Fixtures/I64. Only two exceptions, both in PB-T3b: the Skip of D-1; and the temporary, local and uncommitted re-enabling of '
            'those same methods for the measurement of INV-PBT3B-15, undone before the GREEN commit. Any existing test that fails, outside the D-1 '
            'list, returns to the Owner.'),
    ('C03', 'an observable change of RACKEDITAR appears, or of the pilot in the panel other than the barrier on a blocked design. STOP and return to the Owner.'),
    ('C05', 'a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM, "Guardar en biblioteca", "Insertar", real keyboard or focus, or '
            'writing to the profile or to disk.'),
    ('C06', 'the GREEN CI fails outside the new tests, reproducibly and not attributable to them.'),
    ('C07', 'a fact of section 2 does not hold in a way that changes a CT or the design; or an expected line is not met in the GREEN; or a self-checking '
            'recipe (CT-PBT3A-04, CT-PBT3B-06 and 10) does not exercise its path. The Worker does not adjust what is expected; ' + C07_RR + '.'),
    ('C09', 'a test would hang, leave a window, a modal or a dispatcher alive, write to the profile or depend on machine paths.'),
    ('C10', 'doing the task would require what D-PBT3-14 excludes: creating views; touching the classic editor, the window, the host or the panel port; '
            'connecting the full model; opening AUTH-13; or changing the treatment of the parked tail.'),
    ('C18', C18_RR + '.'),
]
# los rotulos de las comunes que el paquete lleva entre el id y los dos puntos, y que no van al contrato (V43-02, su nota)
STOP_LABELS = {'C07': 'C07 (con la excepcion de F4B-E35, V43-02)', 'C18': 'C18 (nueva, V43-02; el texto de PBT2x-C18 de PB-T2)'}
STOP_OWN = [  # las propias del section 6.1: (id, rotulo del paquete, texto)
    ('C04', 'PBT3A-C04', 'a method of a filter with ExpectRed passes on the skeleton, or fails by an exception and not by an assertion.'),
    ('C13', 'PBT3A-C13 (V2-07 y V2-30)',
     'doing the task would require modifying an existing file under src/; or adding, in the new files, a public or internal declaration (type, '
     'constructor, method, property, field, constant or enum member) whose name is not in the closed list of D-PBT3-02 to D-PBT3-07; or a private '
     'declaration other than those admitted by INV-PBT3A-13. The Worker stops before adding it.'),
    ('C15', 'PBT3A-C15', 'CT-01 gives estado-igual=False or diseno-igual=False for some design: the seat does not match the window. It returns to the '
                         'session, which takes it to the Owner if it is a loss (OB-06.5).'),
    ('C17', 'PBT3A-C17', 'the D2 limit (section 6.0).'),
]
EVID_PKG = [
    'antes del Worker: el contrato, la salida del Controller (output.json, con las ocho colecciones opacas vacias), la delegacion final con esas '
    'colecciones puestas por la sesion (delegation.json; F4B-E21), la comparacion byte a byte sobre la delegacion final, A1-A8 y nc4;',
    'el commit RED y su push antes del GREEN; con una reparacion del RED (F4B-E35, RR-4; V43-04), ademas, el commit "RED repair" y su push antes del '
    'GREEN, con su CI, registrada como la del RED reparado, y los TRX de cada filtro con ExpectRed sobre RedRepairSha;',
    'la CI del RED como en section 6.0 (tests y ui-tests en failure, build-ui en success, build-plugin omitido, sin cuelgue); la del RED reparado, si '
    'lo hay, da lo mismo;',
    'la tabla "CT -> la asercion que falla en el RED" y las autopruebas de las tres guardas;',
    'el GREEN y su CI 4/4;',
    'en local: el build Release con el Plugin sin avisos nuevos, los filtros sobre el RED, sobre RedRepairSha si hay reparacion, y sobre el GREEN, y '
    'tres corridas en verde de los filtros con ExpectRed sobre el GREEN;',
    'la entrega con el mapa CT -> metodo, las rutas relativas comprobadas y el Trailer real de los commits RED, "RED repair" (si lo hay) y GREEN; con '
    'una reparacion del RED, ademas, RedRepairSha, la prueba y la linea corregidas, el error y por que no cambia ninguna linea esperada, con el diff, y '
    'RedSha, que sigue siendo el commit RED original (F4B-E35, RR-5);',
    'la verificacion con las tres comprobaciones de F4B-E15, y nc1 a nc3;',
    # la novena (v4.4, V44-04; CA-T3a M-01): la lectura entera de la verificacion con una reparacion, como la ExpectedEvidence/8 de PB-T2c
    'con una reparacion del RED, la verificacion la lee como parte de la entrega (F4B-E35, RR-7; section 6.0; V43-05 y V44-04, CA-T3a M-01): n.o '
    '10 (Tests): el RED se acredita con RedSha y con la CI del RED original, y sus TRX, con los filtros con ExpectRed entre los fallidos. La '
    'clausula "git diff --name-only RedSha CurrentSha no toca RT U ChainRedFiles" se comprueba desde RedRepairSha: git diff --name-only '
    'RedRepairSha CurrentSha. n.o 11 (Trailer): el trailer va tambien en el commit "RED repair". Lo que el Controller comprueba de RR (si algo '
    'falla, es C18): RR-1, un solo commit "RED repair", hijo directo de RedSha y anterior al GREEN; RR-2, git diff --name-only RedSha '
    'RedRepairSha dentro de las clases de prueba que anadio el RED (git diff --name-only --diff-filter=A BaseSha RedSha -- tests/); RR-4, la CI '
    'de RedRepairSha, registrada como la del RED reparado, y los TRX de los filtros con ExpectRed sobre el; RR-5, la declaracion de la entrega, '
    'con el diff, y RedSha, el commit RED original; RR-6, con el tramo RedSha..RedRepairSha. En PB-T3a, ademas: RedSha..RedRepairSha no toca '
    'ningun archivo de src/ (el esqueleto de Draft/). La ayuda mecanica de RR-3: es parada C07, una violacion cierta de RR-3, que un literal de '
    'la lista cerrada de la clase reparada (section 6.0 del paquete, la tabla de RR-3) cambie entre RedSha y RedRepairSha: una linea o un '
    'fragmento, si no aparece como subcadena exacta el mismo numero de veces; un valor de gesto, si no aparece como token el mismo numero de '
    'veces. Indicios para la revision, no parada: una cadena <campo>= anadida o quitada en git diff -U0 RedSha RedRepairSha, o un cambio en el '
    'primer argumento de un Assert.Equal. Pueden dar falsos positivos, como una linea que solo se mueve de sitio, asi que van a Findings y a la '
    'revision, con la declaracion de RR-5.',
]
assert len(EVID_PKG) == 9, 'la ExpectedEvidence del section 6.1: nueve vinetas (v4.4, V44-04)'
CI_LINE_PKG = ('Linea base de la CI (V42-01): la del cierre de PB-T2c, a medir. Prevista, no medida (el GREEN de PB-T2c en su paquete v2.6): Core '
               '19 094; UI 2 450 (2 429 Passed, 21 Skipped). GREEN previsto: Core + 13 = 19 107; UI + 1 = 2 451.')
# la tabla de RequiredTests del section 6.1, expandida: (proyecto, prefijo de clase, minimo del paquete, ExpectRed, clase)
# clase: 'new' (no existe en la emision), 'fixed' (numero fijo del paquete, minimo comprobado), 'measured' (los de PB-T1 y de Workspace: la nota
# del section 6.1, "se vuelven a medir en la emision"), 'pt2' (los de PB-T2, V42-06: medidos si su clase ya esta; si no, el minimo de su paquete,
# AVISO en seco y error al escribir)
REQ_SPEC = [
    ('Core', 'PushBackFullDraft', 10, True, 'new'),
    ('UI', 'PushBackFullDraftWindowParityTests', 1, True, 'new'),
    ('Core', 'PushBackDraftWriteGuardTests', 3, False, 'new'),
    ('Core', 'Workspace', 3271, False, 'measured'),
    ('UI', 'Workspace', 547, False, 'measured'),
    ('Core', 'PushBackEditableState', 13, False, 'measured'),
    ('Core', 'PushBackEditorOutputGate', 1, False, 'measured'),
    ('Core', 'PushBackSharedReferenceGuardTests', 2, False, 'measured'),
    ('Core', 'PushBackWindowSharedReferenceGuardTests', 1, False, 'measured'),
    ('Core', 'PushBackWindowOnStateSourceGuardTests', 1, False, 'measured'),
    ('UI', 'PushBackWindowOnStateTests', 2, False, 'measured'),
    ('UI', 'PushBackClassicCharacterization', 55, False, 'fixed'),
    ('Core', 'PushBackClassicCharacterization', 10, False, 'fixed'),
    ('UI', 'PushBackFullEditorCharacterization', 16, False, 'fixed'),
    ('Core', 'PushBackFullEditorCharacterization', 3, False, 'fixed'),
    ('UI', 'PushBackStateNet', 14, False, 'measured'),
    ('UI', 'PushBackDeclaredCorrection', 14, False, 'fixed'),
    ('UI', 'Cantilever', 67, False, 'fixed'),
    ('Core', 'I58F1', 10, False, 'fixed'),
    ('Core', 'CustomPropertiesEnvelopeGuardTests', 9, False, 'fixed'),
    ('Core', 'NamespaceFolderGuardTests', 7, False, 'fixed'),
    ('Core', 'PushBackDraftStatePortTests', 3, False, 'pt2'),
    ('Core', 'PushBackDraftPortSharedReferenceGuardTests', 1, False, 'pt2'),
    ('Core', 'PushBackWorkspaceModeSourceGuardTests', 1, False, 'pt2'),
    ('Core', 'PushBackWorkspaceSharedReferenceGuardTests', 1, False, 'pt2'),
    ('UI', 'PushBackWorkspaceModeTests', 3, False, 'pt2'),
    ('UI', 'PushBackWorkspaceApplyCloseTests', 4, False, 'pt2'),
    ('UI', 'PushBackFieldValidationConcordanceTests', 1, False, 'pt2'),
    ('UI', 'PushBackWorkspaceCarryoverTests', 3, False, 'pt2'),
]
assert len(REQ_SPEC) == 29 and [r for r in REQ_SPEC if r[3]] == REQ_SPEC[:2] and [r for r in REQ_SPEC if r[4] == 'new'] == REQ_SPEC[:3]
# las ocho de PB-T2 son las nueve clases salvo la de apoyo (PushBackWorkspaceSupport), sin ExpectRed
assert [f for _, f, _, _, k in REQ_SPEC if k == 'pt2'] == [n for n in T2_CORE_NAMES + T2_UI_NAMES if n != 'PushBackWorkspaceSupport']
# los minimos de los filtros nuevos son sus CT (5.2): 10 en Core, 3 de las guardas y 1 en UI
for p, f, m, _, k in REQ_SPEC[:3]:
    assert m == sum(len(cts) for pp, c, cts in CLASS_CTS if pp == p and c.startswith(f)), (f, m)
# la lista cerrada de PBT3A-C13: los bloques de codigo de D-PBT3-02 a D-PBT3-07, por archivo, con sus constructores (V2-07; la nota de C13)
CLOSED_EXPECTED = {
    'PushBackFullDraftState': ('PushBackFullDraftState.cs', 'class', 'public', ['Editable', 'Origin']),
    'PushBackFullDraftStateComparer': ('PushBackFullDraftStateComparer.cs', 'class', 'private', ['Instance', 'Equals', 'GetHashCode', 'Copy']),
    'PushBackEditableStateSeat': ('PushBackEditableStateSeat.cs', 'class', None, ['Pass', 'LoadExisting']),
    'PushBackFullDraftMaterialization': ('PushBackFullDraftMaterializer.cs', 'class', 'private',
                                         ['Kind', 'IsAdmitted', 'State', 'Reason', 'Admitted', 'OutOfPilot', 'Failed']),
    'PushBackFullDraftMaterializer': ('PushBackFullDraftMaterializer.cs', 'class', None, ['Materialize']),
    'PushBackFullIntentResult': ('PushBackFullDraftReader.cs', 'class', 'internal', ['StaleBase', 'Read', 'OriginKey', 'Detail', 'ClassicEditorHint']),
    'PushBackFullDraftReader': ('PushBackFullDraftReader.cs', 'class', None, ['Read']),
    'PushBackFullWriteComposition': ('PushBackFullComposition.cs', 'class', 'private',
                                     ['IsComposed', 'Editor', 'Inputs', 'Name', 'Reason', 'Composed', 'Rejected']),
    'PushBackFullComposition': ('PushBackFullComposition.cs', 'class', None, ['ForFullWrite', 'KeepOn']),  # v4.5, V45-01 (F4B-E42)
    'PushBackViewDispositionKind': ('PushBackViewDisposition.cs', 'enum', None, ['Redraw', 'ObsoleteLateral', 'FrontalBOfOneWay', 'NoAddress']),
    'PushBackViewDispositionPlan': ('PushBackViewDisposition.cs', 'class', 'internal',
                                    ['Members', 'Redraw', 'ObsoleteLaterals', 'FrontalBOfOneWay', 'NoAddress', 'AllObsolete']),
    'PushBackViewDispositions': ('PushBackViewDisposition.cs', 'class', None, ['Of']),
    'PushBackChangeGroup': ('PushBackConflictClassifier.cs', 'enum', None,
                            ['Name', 'Rack', 'Annotations', 'Safety', 'Fronts', 'Modules', 'SideB', 'Composite', 'OutsideEditor']),
    'PushBackChangeClass': ('PushBackConflictClassifier.cs', 'enum', None, ['ChangedExternally', 'ChangedByUser', 'Overlapping']),
    'PushBackChangeEntry': ('PushBackConflictClassifier.cs', 'class', 'internal',
                            ['Level', 'Path', 'Group', 'Class', 'IsStructural', 'BaseValue', 'ExternalValue', 'DraftValue']),
    'PushBackConflictClassification': ('PushBackConflictClassifier.cs', 'class', 'internal',
                                       ['Entries', 'ChangedExternally', 'ChangedByUser', 'Overlapping', 'Structural', 'ViewsToErase', 'ViewsRejected',
                                        'AllViewsObsolete']),
    'PushBackConflictClassifier': ('PushBackConflictClassifier.cs', 'class', None, ['Classify']),
}
assert sorted(CLOSED_EXPECTED) == sorted(NEW_TYPES) and sorted({v[0] for v in CLOSED_EXPECTED.values()}) == sorted(SRC_NAMES)
CLOSED_COUNT = len(CLOSED_EXPECTED) + sum(1 for v in CLOSED_EXPECTED.values() if v[2]) + sum(len(v[3]) for v in CLOSED_EXPECTED.values())


def closed_list(lines):
    """Los tipos de los bloques ```csharp de D-PBT3-02 a D-PBT3-07: {tipo: (archivo, class|enum, visibilidad del constructor, miembros)}."""
    i0 = [k for k, l in enumerate(lines) if l.startswith('### D-PBT3-02.')]
    i1 = [k for k, l in enumerate(lines) if l.startswith('### D-PBT3-08.')]
    assert len(i0) == 1 and len(i1) == 1 and i0[0] < i1[0], 'D-PBT3-02 a D-PBT3-08'
    types, cur_file, block, t = {}, None, None, None
    for l in lines[i0[0]:i1[0]]:
        if block is None:
            m = re.match(r'^\*\*`(\w+\.cs)`', l)
            if m:
                cur_file = m.group(1)
            if l.startswith('```csharp'):
                block, t = cur_file, None
            continue
        if l.startswith('```'):
            block = None
            continue
        code = l.split('//')[0].rstrip()
        m = re.match(r'^(public|internal) (?:sealed |static )*(class|enum) (\w+)', code)
        if m:
            t = m.group(3)
            assert t not in types, ('tipo repetido en los bloques', t)
            types[t] = [block, m.group(2), None, re.findall(r'(\w+) = \d+', code) if m.group(2) == 'enum' else []]
            continue
        m = re.match(r'^ {4}(public|private|internal) (.*)$', code)
        if m and t:
            rest = re.sub(r'^(?:static |readonly )+', '', m.group(2))
            if rest.startswith(t + '('):
                assert types[t][2] is None, ('dos constructores', t)
                types[t][2] = m.group(1)
            else:
                types[t][3].append(re.search(r'([A-Za-z_]\w*)\s*(?:\(|\{|;|=>)', rest).group(1))
    return {k: tuple(v) for k, v in types.items()}


def package_required(sec):
    """La tabla de RequiredTests del section 6.1, expandida a (proyecto, filtro, minimo, ExpectRed) en su orden."""
    sp = '[ ' + chr(0xA0) + chr(0x202F) + ']'
    rows = []
    for l in sec:
        m = re.match(r'^  \| (Core|UI|Core y UI|UI y Core) \| (.*?) \| (.*?) \| (true|false) \|$', l)
        if not m:
            continue
        projs = m.group(1).split(' y ')
        fgroups = [re.findall(r'`([^`]+)`', g) for g in m.group(2).split(';')]
        mgroups = [[x.strip() for x in g.split(',')] for g in m.group(3).replace('**', '').split(';')]
        er = m.group(4) == 'true'
        assert len(fgroups) == len(mgroups) and (len(projs) == 1 or len(fgroups) in (1, len(projs))), ('fila', l)

        def num(s):
            assert re.fullmatch(r'\d{1,3}(?:' + sp + r'\d{3})*', s), ('minimo', s)
            return int(re.sub(r'\D', '', s))
        for gi, (fs, ms) in enumerate(zip(fgroups, mgroups)):
            assert len(fs) == len(ms), ('fila', l)
            for f, v in zip(fs, ms):
                parts = [p.strip() for p in re.split(r' y ', v)]
                if len(parts) == 2:
                    assert len(projs) == 2, ('fila', l)
                    rows += [(projs[0], f, num(parts[0]), er), (projs[1], f, num(parts[1]), er)]
                else:
                    rows.append((projs[0] if len(projs) == 1 else projs[gi], f, num(parts[0]), er))
    return rows


PKG_CHECKS = []
RR3_SUMMARY = 'sin comprobar (el paquete leido no es la ' + PKG_VERSION + ')'
if V4:
    SEC60, SEC61, SEC52 = section('### 6.0 '), section('### 6.1 Contrato de PB-T3a'), section('### 5.2 ')
    B60, B61, B61_2 = bullets(SEC60, 0), bullets(SEC61, 0), bullets(SEC61, 2)
    # la variante (A') no elegida (F4B-E29; V41-03): ningun texto que se copia al contrato la lleva. Va antes de los literales, para que una copia
    # que la meta falle aqui; y no es vacia, porque el section 6.0 si la nombra, en su nota del ForbiddenWriteScope de PB-T3b
    # el bloque citado de PBT3x-C11 (V43-09) es texto del paquete, no una vineta: se lee aparte, una sola linea '  > '
    _q11 = [l for l in SEC60 if l.startswith('  > ')]
    assert len(_q11) == 1, ('el bloque citado de PBT3x-C11 del section 6.0', len(_q11))
    C11_FOLD = ascii_fold(_q11[0][4:]).replace('`', '')
    _copied = ([b for b in B61 if b.startswith(('Objective (ASCII): ', 'ExpectedEvidence: ', 'Linea base de la CI'))] +
               [b for b in B61_2 if b.startswith(('INV-PBT3A-', 'PBT3A-C'))] +
               [b for b in B60 if re.match(r'^(0[7-9]|1[0-4])\. |^C\d\d(: | \()', b)] + [C11_FOLD])
    assert len(_copied) == 3 + 6 + 4 + 8 + 9 + 1, ('los textos que se copian del paquete', len(_copied))
    assert not [b[:60] for b in _copied if VARIANT in b], ("un texto que se copia al contrato lleva la variante (A') no elegida",
                                                            [b[:60] for b in _copied if VARIANT in b])
    assert VARIANT in ascii_fold('\n'.join(SEC60)), "la comprobacion de (A') seria vacia: el section 6.0 ya no la nombra"
    assert one(B61, 'Objective (ASCII): ') == 'Objective (ASCII): "' + OBJECTIVE + '"', 'el Objective del paquete difiere'
    assert ascii_fold(OBJECTIVE) == OBJECTIVE and ('PB-T3 ' + PKG_VERSION) in OBJECTIVE
    _aws = one(B61, 'AllowedWriteScope (14): ')
    assert ('los ocho archivos nuevos de ' + DRAFT + ': ') in _aws and all(n in _aws for n in SRC_NAMES) and \
        'las cinco clases nuevas de tests/RackCad.Tests/ de section 5.2' in _aws and _aws.endswith(TEST_NEW[-1] + '.'), 'el AllowedWriteScope del section 6.1'
    assert one(B61, 'El esqueleto exacto del RED: ') == SKELETON, 'el esqueleto del RED del paquete difiere'
    _req_pkg = package_required(SEC61)
    assert _req_pkg == [(p, f, m, e) for p, f, m, e, _ in REQ_SPEC], ('la tabla de RequiredTests del section 6.1 cambio', _req_pkg)
    for k, txt in enumerate(INV_OWN, 1):
        assert one(B61_2, 'INV-PBT3A-%02d.' % k) == txt, ('INV-PBT3A-%02d del paquete difiere' % k)
    for cid, label, txt in STOP_OWN:
        assert one(B61_2, label + ': ') == label + ': ' + txt, (label + ' del paquete difiere')
    assert one(B61, 'ExpectedEvidence: ') == 'ExpectedEvidence: ' + ' '.join(EVID_PKG), 'la ExpectedEvidence del section 6.1 difiere'
    assert all(sum(1 for b in B61_2 if b == e) == 1 for e in EVID_PKG), 'la ExpectedEvidence, vineta a vineta'
    assert one(B61, 'Linea base de la CI') == CI_LINE_PKG, 'la linea base de la CI del section 6.1 difiere'
    assert sum(1 for l in SEC61 if l.strip().startswith('- **Nota (no va al contrato).**')) == 3, 'las notas que no van al contrato del section 6.1'
    for n, txt in INV_COMMON:
        assert one(B60, n + '. ') == txt, ('el invariante comun %s del paquete difiere' % n)
    assert sum(1 for l in SEC60 if l.strip().startswith('- **Nota (no va al contrato).**')) == 6, 'las notas que no van al contrato del section 6.0'
    for cid, txt in STOP_COMMON:
        _lab = STOP_LABELS.get(cid, cid)
        assert one(B60, _lab + ': ') == _lab + ': ' + txt, ('la parada comun %s del paquete difiere' % cid)
    one(B60, 'PBT3x-C08 y PBT3x-C12, con el texto de [PKT1] section 6.0 y el prefijo cambiado: ')
    one(B60, 'PBT3x-C11, con el sustituto de nc2 adaptado a F4B (F4B-E34; V43-09). ')
    assert C11_FOLD == C11_PKG, 'el bloque citado de PBT3x-C11 del paquete difiere'
    # la regla RR en el section 6.0 (V43-01 a V43-06): RR-1 a RR-6, las dos lecturas, la verificacion y la tabla cerrada de RR-3
    _err = rr_pkg_check(SEC60, [c for _, c, _ in CLASS_CTS])
    assert not _err, ('la regla RR del section 6.0', _err)
    # la tabla de RR-3 de la v4.4 (V44-02 y V44-03): enumerable y cerrada para las seis clases de PB-T3a; sus referencias, en 05beabfd
    _t2c_tree = [p for p in names_of(git('ls-tree', '-r', '--name-only', PT2C_EMIT)) if p.startswith(('src/', 'tests/'))]

    def _read_ref(base):
        hits = [p for p in _t2c_tree if p.rsplit('/', 1)[-1] == base]
        return lines_of(hits[0], PT2C_EMIT) if len(hits) == 1 else None

    _err, RR3_INFO = rr3_table_check(SEC60, [c for _, c, _ in CLASS_CTS], _read_ref)
    assert not _err, ('la tabla de RR-3 del section 6.0 (V44-02 y V44-03)', _err)
    # lo que CA-T3a M-03 pedia enumerar con referencia: los siete nombres de constantes de CT-02 y las ocho etiquetas de momento de CT-08
    RR3_CT02 = ['UnreadableOrFutureDiagnostic', 'KindDivergentDiagnostic', 'NoDesignDiagnostic', 'CatalogRequiredDiagnostic',
                'CompositeOutOfPilotReason', 'DormantTailOutOfPilotReason', 'MaterializeFailurePrefix']
    RR3_CT08 = ['base', '+frente', '-frente', 'frentes 1 y 2 en blanco', 'declarar B', 'B ranura 1 activa', 'B ranura 1 en blanco', 'retirar B']
    assert sorted(RR3_INFO['PushBackFullDraftLoadTests'][2].get('CT-02, lineas', [])) == sorted(RR3_CT02), \
        ('CT-02 de la tabla de RR-3: los siete nombres de PushBackPilotMaterializer.cs:100-120', RR3_INFO['PushBackFullDraftLoadTests'][2])
    assert sorted(RR3_INFO['PushBackFullDraftViewTests'][2].get('lineas', [])) == sorted(RR3_CT08), \
        ('CT-08 de la tabla de RR-3: las ocho etiquetas de PushBackFullEditorCharacterizationLateralCutTests.cs:99-119',
         RR3_INFO['PushBackFullDraftViewTests'][2])
    RR3_SUMMARY = ('6 clases, una fila cada una; %s; CT-02: 7 nombres en PushBackPilotMaterializer.cs:100-120 y CT-08: 8 etiquetas en '
                   'PushBackFullEditorCharacterizationLateralCutTests.cs:99-119, en %s'
                   % ('; '.join('%s %d lineas y %d valores' % (c, RR3_INFO[c][0], RR3_INFO[c][1]) for _, c, _ in CLASS_CTS), PT2C_EMIT[:8]))
    _s60 = md_fold(SEC60)
    assert ('P-01 dice "' + P01 % ('K', SNAP_K) + '"') in _s60, 'P-01 del section 6.0 (la Snapshot K)'
    assert 'El limite de D2, en cada contrato: PBT3x-C17, con el texto literal del Owner ([PKT1] section 6.0).' in _s60, 'C17 del section 6.0'
    assert 'Los TaskId: de 24 a 26 caracteres, por MAX_PATH (R-T1-07).' in _s60 and 'StopConditions estandar (17), copiadas de [PKC2]' in _s60
    assert one(B60, 'cada contrato declara UNIT_CHANGE ').endswith('(PB-T3a solo crea archivos);'), 'UNIT_CHANGE de PB-T3a'
    assert one(B60, 'en las tres subtareas, Core falla a proposito en el RED, y build-plugin tiene needs: [tests, build-ui] (.github/workflows/ci.yml)')
    assert 'ui-tests en failure (PB-T3a)' in one(B60, 'ExpectedEvidence lo pide asi: ') and 'tests y ui-tests en failure' in EVID_PKG[2]
    for cell in ('Worker anthropic/subagent/claude-sonnet-5-5, effort high;', 'Controller openai/process/gpt-6-luna, effort high;', 'vencen el 2026-12-29;'):
        one(B60, cell)
    _tests_b = one(B60, 'en tests/: ')
    assert 'en PB-T3a y PB-T3c, el prefijo tests/RackCad.Tests/Workspace/, que la lista curada de PB-T0 ya trae;' in _tests_b and \
        'tests/RackCad.Tests/Fixtures/I64/ y tests/RackCad.UI.Tests/Workspace/' in _tests_b, 'el ForbiddenWriteScope de tests/ del section 6.0'
    # V42-05: las nueve clases de PB-T2, nombradas, que son las del contrato de PB-T2c
    _t2_txt = ('las clases de PB-T2 (V42-05): las listas curadas de tests/ del contrato de PB-T2c, mas las clases de su AllowedWriteScope de pruebas. '
               'Son las nueve de PB-T2: en Core, %s y %s; en UI, %s y %s;' % (', '.join(T2_CORE_NAMES[:-1]), T2_CORE_NAMES[-1],
                                                                         ', '.join(T2_UI_NAMES[:-1]), T2_UI_NAMES[-1]))
    assert _t2_txt in _tests_b, 'las nueve clases de PB-T2 del section 6.0'
    # V42-01: las referencias del anclaje de PB-T3a; V42-10: las anclas de W por contenido; V42-06: la contencion de F4B-E30 sobre PB-T2
    assert ('en PB-T3a, frente al cierre de PB-T2c (vacio). Las referencias del anclaje: Application, la del cierre de PB-T1b mas los dos archivos de '
            'PB-T2a (PushBackDraftStatePort.cs y PushBackDraftEditorLaunch.cs); Domain y Plugin, la del cierre de PB-T1b; src/RackCad.UI, la del cierre '
            'de PB-T2b (W y PushBackEditorMode.cs), con WC igual al cierre de PB-T1c;') in one(B60, 'el diff de src (V42-01): '), 'las referencias de V42-01'
    one(B60, 'las anclas de W, por contenido (V42-10): ')
    assert one(B60, 'que ninguna clase de los filtros de UI de la subtarea crea un RackFrameConfiguratorWindow real').endswith(
        'tambien sobre las clases de PB-T2 que entran en la red (V42-06);'), 'la contencion de F4B-E30 del section 6.0'
    one(B60, 'las 17 entradas de la raiz.')
    # 5.2: las clases con sus CT y la lista cerrada CT-PBT3A-01 a 14 (12 a 14, las guardas, no fallan en el RED)
    _cls = [(('UI' if 'RackCad.UI.Tests' in l else 'Core'), c, n) for l in SEC52 if l.startswith('- en `tests/')
            for c, n in re.findall(r'`(PushBack\w+Tests)` \(([0-9, ya]+)\)', l)]

    def cts(s):
        a = re.fullmatch(r'(\d\d) a (\d\d)', s)
        return list(range(int(a.group(1)), int(a.group(2)) + 1)) if a else [int(x) for x in re.split(r', | y ', s)]
    assert [(p, c, cts(n)) for p, c, n in _cls] == CLASS_CTS, ('las clases de 5.2', _cls)
    _rows = [re.match(r'^\| \*\*(\d\d)\*\*', l).group(1) for l in SEC52 if re.match(r'^\| \*\*\d\d\*\*', l)]
    assert _rows == ['%02d' % n for n in range(1, 15)], ('la lista CT-PBT3A-01 a 14 de 5.2', _rows)
    assert [n for n in range(1, 15) if [l for l in SEC52 if l.startswith('| **%02d**' % n)][0].rstrip().endswith('**no falla en el RED** |')] == [12, 13, 14]
    # CT-PBT3A-04: la linea esperada lleva las constantes del lector (IR:23, :35 y :56)
    _ct04 = [l for l in SEC52 if l.startswith('<ruta> piloto=[')]
    assert _ct04 == ['<ruta> piloto=[%s] completo=[%s] pista=[%s]' % (CT04_DIAG, CT04_DIAG, CT04_HINT)], ('la linea de CT-PBT3A-04', _ct04)
    # la lista cerrada de PBT3A-C13 y la API de INV-PBT3A-03 (D-PBT3-02 a D-PBT3-07, con sus constructores)
    _closed = closed_list(PKG_LINES)
    assert _closed == {k: (v[0], v[1], v[2], v[3]) for k, v in CLOSED_EXPECTED.items()}, ('la lista cerrada de D-PBT3-02 a D-PBT3-07 cambio', _closed)
    # el sha256 del revisado, al final, para que una copia mutada falle antes en su texto
    assert hashlib.sha256(PKG_RAW).hexdigest() == CUSTODY_SHA256[PKG], ('el paquete leido no es el %s revisado (sha256)' % PKG_VERSION,
                                                                         hashlib.sha256(PKG_RAW).hexdigest())
    PKG_CHECKS = ["sin (A') en %d textos copiados" % len(_copied), 'Objective', 'AllowedWriteScope', 'esqueleto del RED', 'RequiredTests (con las 8 de PB-T2)',
                  'INV-PBT3A-01..06', 'paradas propias (C04, C13, C15, C17)', 'ExpectedEvidence', 'linea base de la CI (V42-01)', 'tres notas fuera (6.1)',
                  'invariantes comunes 07..14', 'seis notas fuera (6.0; V43)', 'paradas comunes (C01..03, C05..07, C09, C10, C18; C07 con RR)',
                  'C08 y C12 de [PKT1]', 'C11 = la cita del paquete (sustituto F4B, V43-09)', 'regla RR y tabla de RR-3 (V43-03)',
                  'tabla de RR-3 enumerable y cerrada, lineas y valores (V44-02 y V44-03)', 'ExpectedEvidence con nueve vinetas (V44-04)',
                  'ForFullWrite en el esqueleto y en la lista cerrada (V45-01)', 'P-01 K (V43-07)',
                  'C17 del limite de D2',
                  'TaskId 24..26', 'UNIT_CHANGE vacio', 'CI del RED (needs)', 'celdas', 'ForbiddenWriteScope de tests/', 'las nueve clases de PB-T2 (V42-05)',
                  'referencias del anclaje (V42-01)', 'W por contenido (V42-10)', 'F4B-E30 sobre PB-T2 (V42-06)', 'clases y CT de 5.2',
                  'CT-04 con las constantes del lector', 'lista cerrada de C13 (%d declaraciones)' % CLOSED_COUNT, 'sha256 del revisado']

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
# F4B-E30 (la contencion; section 6.0, tambien sobre las clases de PB-T2, V42-06): ninguna clase de los filtros de UI de PB-T3a crea un
# RackFrameConfiguratorWindow real, cuyo OnClosed escribe en los datos locales del usuario; INV-PBT3A-10 corre esos filtros en local
_cfg = sorted(p for p in UI_FILES if re.search(r'new\s+RackFrameConfiguratorWindow\s*\(', re.sub(r'//[^\n]*', '', re.sub(r'"(?:\\.|[^"\\\n])*"', '""', UI_TEXTS[p]))))
assert not _cfg, ('F4B-E30: un filtro de UI de PB-T3a crea el configurador real', _cfg)
UI_T2_FILES = sorted(p for p in UI_FILES if os.path.basename(p)[:-3] in T2_UI_NAMES)
_t1c_req = {(r['Project'], r['Filter']): r['MinSelected'] for r in pbt1c['RequiredTests']}
_t2c_req = {(r['Project'], r['Filter']): r['MinSelected'] for r in pbt2c['RequiredTests']}
REQ, REQ_REPORT, T2_PENDING = [], [], []
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
            # una Theory con MemberData no se cuenta en estatico: vale si el contrato de PB-T1c fija el mismo filtro con ese minimo (medido por la
            # CI) o si cada Theory con MemberData, que da al menos un caso, cubre lo que falta
            prev = _t1c_req.get((proj, filt))
            assert member > 0 and (prev == pkg_min or run + member >= pkg_min), (
                'el recuento medido de %s %s (%d, %d MemberData) es menor que el minimo del paquete (%d)' % (p, f, run, member, pkg_min))
            note = ('     estatico %d (+%d Theory con MemberData) < %d del paquete: vale porque %s' % (
                run, member, pkg_min, 'el contrato de PB-T1c fija el mismo filtro con %d (casos contados por la CI)' % prev if prev == pkg_min
                else 'cada Theory con MemberData da al menos un caso (%d + %d >= %d)' % (run, member, pkg_min)))
        mins = pkg_min
    elif kind == 'pt2' and classes == 0:
        # la clase de PB-T2b o PB-T2c llega con su GREEN: el minimo de su paquete (el de su contrato), AVISO en seco y error al escribir
        T2_PENDING.append(f)
        mins = pkg_min
    else:
        assert run >= pkg_min, ('el recuento medido de %s %s (%d) es menor que el %d del paquete' % (p, f, run, pkg_min))
        mins = run
    if kind == 'pt2':  # los minimos de PB-T2 son los de su contrato de PB-T2c
        assert _t2c_req.get((proj, filt)) == pkg_min, ('el minimo de PB-T2 del paquete difiere del contrato de PB-T2c', f, _t2c_req.get((proj, filt)), pkg_min)
    REQ.append((proj, filt, mins, er))
    REQ_REPORT.append('%-4s %-41s %-8s paquete=%-5s medido: clases=%d ejecutables=%d skip=%d memberdata=%d -> MinSelected=%d ExpectRed=%s'
                      % (p, f, kind, pkg_min, classes, run, skip, member, mins, er))
    if note:
        REQ_REPORT.append(note)
for proj, _, _, _ in REQ:
    assert in_head(proj), proj
soft(not T2_PENDING, 'cierres de PB-T2b y PB-T2c: clases de PB-T2 de los RequiredTests aun no en la revision (MinSelected = el de su paquete): ' +
     ', '.join(T2_PENDING))
for proj, filt, mins, er in REQ:  # los filtros heredados de los contratos de PB-T1c y PB-T2c no bajan su MinSelected
    for _prev in (_t1c_req, _t2c_req):
        if (proj, filt) in _prev:
            assert _prev[(proj, filt)] <= mins, (filt, _prev[(proj, filt)], mins)
for p, f, _, _, k in REQ_SPEC:  # ningun filtro existente selecciona una clase nueva, y cada filtro nuevo solo las suyas
    news = [c for pp, c, _ in CLASS_CTS if pp == p and c.startswith(f)]
    assert (k == 'new') == bool(news), ('un filtro y las clases nuevas', p, f, news)
    t2 = [c for c in (T2_CORE_NAMES if p == 'Core' else T2_UI_NAMES) if c.startswith(f)]  # ni una de PB-T2, salvo su propia fila
    assert (k == 'pt2') == bool(t2) and (k != 'pt2' or t2 == [f]), ('un filtro y las clases de PB-T2', p, f, t2)

cells = json.loads(json.dumps(pbt1c['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna'] and all('high' in c['TestedEfforts'] for c in cells)
assert cells == json.loads(show(PBTC2C))['EligibleCells'], 'las celdas difieren de las de PB-TC2'

# ---------------------------------------------------------------- invariantes: los propios (01 a 06) y los comunes (07 a 14), literales
INV = INV_OWN + ['INV-PBT3A-' + txt for _, txt in INV_COMMON]
assert [i.split('. ')[0] for i in INV] == ['INV-PBT3A-%02d' % n for n in range(1, 15)] and len(INV) == 14
assert INV[12].startswith('INV-PBT3A-13. Las declaraciones nuevas en src/') and 'INV-PBT3A-13' in STOP_OWN[1][2]  # PBT3A-C13 cita INV-PBT3A-13

# ---------------------------------------------------------------- la evidencia: la del section 6.1, literal, y la linea base de la CI
_t2c_ev = ' '.join(pbt2c['ExpectedEvidence'])
assert ('Core + 0 = %d Passed; UI + 3 (CT-PBT2C-01 a 03) = %d resultados: 0 Failed, %d Skipped y %d Passed' % (
    CI_FORECAST[0], CI_FORECAST[1], CI_FORECAST[2], CI_FORECAST[1] - CI_FORECAST[2])) in _t2c_ev, \
    'la linea base prevista (V42-01) no es el GREEN previsto del contrato de PB-T2c'
_new_core = sum(m for p, _, m, _, k in REQ_SPEC if k == 'new' and p == 'Core')
_new_ui = sum(m for p, _, m, _, k in REQ_SPEC if k == 'new' and p == 'UI')
assert (_new_core, _new_ui) == (13, 1) and CI_FORECAST[0] + _new_core == 19107 and CI_FORECAST[1] + _new_ui == 2451
if CI_BASE_T2C:
    _run, _sha, _core, _ui, _skip = CI_BASE_T2C
    CI_TXT = ('Linea base de la CI: la del cierre de PB-T2c (run %s sobre %s: Core %d Passed; UI %d resultados, %d Skipped y %d Passed). GREEN previsto: '
              'Core + %d = %d Passed; UI + %d = %d resultados: 0 Failed, %d Skipped y %d Passed, o la diferencia explicada por el diff.'
              % (_run, _sha, _core, _ui, _skip, _ui - _skip, _new_core, _core + _new_core, _new_ui, _ui + _new_ui, _skip, _ui + _new_ui - _skip))
else:
    CI_TXT = CI_LINE_PKG
EVID = EVID_PKG + [CI_TXT]

# ---------------------------------------------------------------- paradas: las 17 estandar, las comunes del section 6.0 y las propias del section 6.1
T1C_STOP = {s['Id']: s['Text'] for s in pbt1c['StopConditions']}


def from_t1c(cid):
    """Las de [DP] C08 y C12, con el texto de [PKT1] section 6.0 que lleva el contrato de PB-T1c, y solo el prefijo cambiado."""
    t = T1C_STOP['PBT1C-' + cid].replace('PBT1C', 'PBT3A').replace('PB-T1c', 'PB-T3a')
    assert 'PBT1C' not in t and 'PB-T1c' not in t, cid
    return t


# PBT3A-C11 (V43-09): el bloque citado del paquete, no el de PB-T1c. Su sustituto de nc2 es el adaptado a F4B de F4B-E34, con su id, su registro,
# su ruta y su SHA-256, y el sustituto esta en la revision con ese SHA-256
C11 = C11_PKG
assert C11.count('PBT3A-NC2-EXACT-WHITELIST-F4B-01, a copy of PBT2A-NC2-EXACT-WHITELIST-F4B-01 (F4B-E34: ' + SUBST_F4B + ', SHA-256 ' +
                 SUBST_F4B_SHA + ';') == 1, 'PBT3A-C11: el sustituto de nc2 de F4B-E34, su ruta y su SHA-256'
assert C11.count('recorded as nc2 = SATISFIED_UNDER_PBT3A_EXACT_WHITELIST_SUBSTITUTE_F4B;') == 1 and 'PBT3A-NC1-IDENTITY-SIGNAL-01' in C11, 'PBT3A-C11: los registros'
assert C11.endswith('no one-off acceptance extends to PB-T3a') and not re.search(r'PBT1[ABC]-NC|PBT2[BC]-NC|F4T1B-NC2', C11), 'PBT3A-C11: el final y los ids'
assert hashlib.sha256(show_bytes(SUBST_F4B)).hexdigest() == SUBST_F4B_SHA, ('el sustituto de nc2 de F4B-E34 no esta en la revision con su SHA-256', SUBST_F4B)
# PBT3A-C07 (su excepcion) y PBT3A-C18 son, letra a letra, los del contrato emitido de PB-T2c (la nota de V43-02), con el prefijo cambiado
T2C_STOP = {s['Id']: s['Text'] for s in pbt2c['StopConditions']}
assert T2C_STOP['PBT2C-C07'].endswith('; ' + C07_RR) and T2C_STOP['PBT2C-C18'] == C18_RR, 'C07 (la excepcion de RR) o C18 difieren de los de PB-T2c'
_c17 = T1C_STOP['PBT1C-C17']
assert _c17.count(' In PB-T1c the window adopts') == 1 and _c17.startswith('(F4B-E11, D2, IMPORTANT LIMIT) doing the task would need to MUTATE')
C17 = _c17[:_c17.index(' In PB-T1c the window adopts')] + ' The Worker chooses none of the three exits: the Owner does'
assert C17.count('"') == 2 and 'If later PB-T1/PB-T2/PB-T3 needs to mutate either' in C17
# la cita del Owner, literal, esta en F4B-E11 (owner-d1-d2-decisions.md), con los espacios normalizados
_quote = C17[C17.index('"') + 1:C17.rindex('"')]
_od2 = ' '.join(show(CO + 'owner-d1-d2-decisions.md').split())
assert all(' '.join(part.split()) in _od2 for part in re.split(r'(?<=[.:;]) ', _quote) if part.strip()), 'la cita de D2 de C17 no esta en F4B-E11'
_common = dict(STOP_COMMON)
_own = {cid: txt for cid, _, txt in STOP_OWN}
_own['C17'] = C17  # PBT3A-C17: "the D2 limit (section 6.0)", con el texto literal del Owner de [PKT1] section 6.0
STOP_TEXT = dict(_common, **_own)
STOP_TEXT.update({'C08': from_t1c('C08'), 'C11': C11, 'C12': from_t1c('C12')})
STOP_IDS = ['C01', 'C02', 'C03', 'C04', 'C05', 'C06', 'C07', 'C08', 'C09', 'C10', 'C11', 'C12', 'C13', 'C15', 'C17', 'C18']
assert sorted(STOP_TEXT) == sorted(STOP_IDS) and len(STOP_IDS) == 16
STOPS = P_STOPS + [{'Id': 'PBT3A-' + cid, 'Text': STOP_TEXT[cid]} for cid in STOP_IDS]
assert len(STOPS) == 33 and len({s['Id'] for s in STOPS}) == 33
for s in STOPS[17:]:  # ninguna parada de PB-T1 copiada sin adaptar
    assert not re.search(r'PBT1[ABC]-(C\d|NC)|SUS3[ABC]', s['Text']), s['Id']

# ---------------------------------------------------------------- autoridades
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
BM = PILOT + '/F4B-PBT0-CHARACTERIZATION/block-m/block-m-analysis.md'
MEAS1 = 'docs/initiatives/I-64-f4b-pbt1-measurements.md'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
_disp = show(DISP).replace('\r\n', '\n')
_disp_ids = [int(n) for n in re.findall(r'(?m)^## F4B-E(\d+)\.', _disp)]
assert _disp_ids == list(range(1, len(_disp_ids) + 1)) and _disp_ids[-1] >= 39, ('f4b-dispositions.md', _disp_ids[-3:])
DISP_LAST = 'F4B-E%02d' % _disp_ids[-1]
for _e, _needle in (('F4B-E24', 'owner-f4b-dpb07-extensiondata-decisions.md'), ('F4B-E25', 'owner-f4b-correction-unit-change-decision.md'),
                    (PB1B_CLOSE_ENTRY, 'Cierre de F4B-PBT1B-LOSSLESS-STATE-TYPE'), (PT1C_ISSUE_ENTRY, 'Emisi'), (OWNER_PBT2_ENTRY, 'owner-pbt2-decisions.md'),
                    (D1_ENTRY, 'owner-pbt3-d1-decision.md'), ('F4B-E30', 'PBT1C-C07'), (PT1C_CLOSE_ENTRY, 'Cierre de ' + PT1C_TASK),
                    (PT2A_EMIT_ENTRY, 'Emisi'), (PT2A_STOP_ENTRY, 'PBT2A-C07'), ('F4B-E34', 'Cierre de ' + PT2A_TASK),
                    ('F4B-E34', 'owner-pbt2a-controls-decisions.md'), (ISSUE_AUTH[0], 'owner-f4b-pbt3-emission-red-repair-decisions.md'),
                    ('F4B-E37', 'Snapshot K'), (PT2B_CLOSE_ENTRY, 'Cierre de ' + PT2B_TASK), (PT2C_EMIT_ENTRY, PT2C_TASK),
                    ('F4B-E40', 'owner-pbt2c-nc1-decision.md'), (PT3A_EMIT_ENTRY, 'F4B-PBT3A-FULL-WRITE-MODEL')):
    _i = _disp.index('\n## %s. ' % _e)
    assert _needle in _disp[_i:_disp.find('\n## F4B-E', _i + 1)], (_e, _needle)
# la parada del intento 1 y la decision del Owner (F4B-E42): la entrega la commit X de la reemision; hasta entonces, AVISO
E42_OK = bool(re.search(r'(?m)^## %s\. ' % PT3A_STOP_ENTRY, _disp))
if soft(E42_OK, 'reemision: %s (la parada PBT3A-C02 del intento 1 y la decision del Owner, Renombrar ForWrite) aun no esta en f4b-dispositions.md'
        % PT3A_STOP_ENTRY):
    _i = _disp.index('\n## %s. ' % PT3A_STOP_ENTRY)
    _e42 = _disp[_i:_disp.find('\n## F4B-E', _i + 1) if _disp.find('\n## F4B-E', _i + 1) > 0 else len(_disp)]
    assert all(x in _e42 for x in ('PBT3A-C02', 'owner-pbt3a-g9-decision.md', RUN1, 'ForFullWrite')), 'F4B-E42 sin la parada, la decision o el nombre nuevo'
# F4B-E35: la pregunta 3 (la emision condicional de PB-T3, ISSUE_AUTH) y la pregunta 4 (la regla RR), literales del Owner
_e35 = show(ISSUE_AUTH[1])
_err = rr_check(_e35) + q3_check(_e35)
assert not _err, ('F4B-E35', _err)
_d1 = show(CO + 'owner-pbt3-d1-decision.md')
assert 'Un solo c' in _d1 and 'docs/initiatives/I-64-f4b-pbt3-gate-contract-package.md' in _d1, 'F4B-E29: la opcion (A) y el paquete de PB-T3'
_q4 = show(CO + 'owner-pbt2-decisions.md')
assert _q4.count('\n## Q-PBT2-4. ') == 1 and 'PB-T2a, PB-T2b y PB-T2c' in _q4 and 'PB-T3' not in _q4.split('\n## Q-PBT2-4. ', 1)[1], \
    'F4B-E28: su pregunta 4 cubre PB-T2a a PB-T2c y no PB-T3 (precondicion 10)'
# el paquete de PB-T2: el custodiado en la revision; PB-T2c se emitio con la v2.9 (F4B-E39), que nombra la linea base prevista de PB-T3a
_p2t = [l for l in lines_of(PKG2)[:6] if l.startswith('# I-64 F4B')]
PKG2_IN = re.search(r', (v\d+(?:\.\d+)?)\s*$', _p2t[0]).group(1) if len(_p2t) == 1 else None
soft(PKG2_IN == PKG2_VERSION, 'emision de PB-T2c: el paquete de PB-T2 custodiado en la revision es la %s; con PB-T2c queda la %s' % (PKG2_IN, PKG2_VERSION))
F4T4 = sorted(p for p in tracked('docs/initiatives/') if re.fullmatch(r'docs/initiatives/I-64-f4-t4[a-z0-9-]*-gate-contract-package\.md', p))
assert len(F4T4) == 6, F4T4
UNIT_CHANGE = []  # section 6.0: PB-T3a solo crea archivos, no declara rutas UNIT_CHANGE
EXISTING = [
    (PHASE, 'paquete de la fase F4B v3.1 ([P]): la fila de PB-T3 del section 5 (:505); H-PB-16 a H-PB-23; OB-02.2; OB-03.1 y OB-03.2; OB-04.1 y '
            'OB-04.2; OB-05.1 a OB-05.5; section 6.4; OB-10.1; section 7.3 y 7.4 (los textos literales del [OD]); OB-T1B.1 a OB-T1B.3; OB-RE.1; R-PB-03, '
            'R-PB-05, R-PB-06, R-PB-09 y R-PB-11'),
    (CO + 'owner-f4b-disposition.md', 'archivo completo ([OD]): la fila de PB-T3; D-PB-04 (borrar si; crear, solo tras PB-T0); D-PB-05 (los pasos 1 a 7); '
                                      'D-PB-10 (1/3; el intento 3 vuelve al Owner); el bloque sobre T1B y el de RACKEDITAR'),
    (BM, 'el bloque M de PB-T0: H-PB-16 confirmado y la fila 1 del section 6.4 resuelta (:92-97); lo que deja para PB-T3 (:133-139)'),
    (PKT1, 'el paquete de PB-T1 v2.8 custodiado ([PKT1]): D-PBT1-02 a D-PBT1-11; D-PBT1-15 (D2); section 5.0 (las recetas y el ayudante Pasada, que '
           'PushBackEditableStateSeat.Pass copia); section 6.0; section 10 (A-2 y A-3); y las lecciones que aplica el paquete de PB-T3: PBT1C-C13 por '
           'nombre (:1338), lo normativo separado de las notas (V28-19) y el Trailer real (V28-17)'),
    (MEAS1, 'el registro de mediciones de PB-T1: H-12 (la lista curada de tests/RackCad.Tests/ de PB-T0, :155; el modo de fallo repetido del heredoc, :227)'),
    (CO + 'owner-d1-d2-decisions.md', 'archivo completo ([OD2], F4B-E11): D2 con sus diez condiciones y su limite, el texto literal de PBT3A-C17'),
    (CO + 'owner-f4b-procedure-decisions.md', 'archivo completo ([DP], F4B-E03): preguntas 1 y 2 (firmas exactas de los sustitutos y la replanificacion unica)'),
    (CO + 'owner-pbt1a-nc1-decisions.md', 'archivo completo (F4B-E15): las tres comprobaciones deterministas de la verificacion'),
    (CO + 'owner-pbtc-nc1-decision.md', 'la disposicion unica de nc1 en PB-TC (F4B-E08), que no se extiende a PB-T3a'),
    (CO + 'owner-pbtc2-nc1-decisions.md', 'la disposicion de nc1 en PB-TC2 solo para su intento 1 (F4B-E12), que no se extiende a PB-T3a'),
    (SNAP_K_DEC, 'archivo completo (F4B-E37): "Sonda neutra, luego K (Recomendado)": la Snapshot K (' + SNAP_K + ', ConfigSha256 ' + SNAP_K_SHA[:8] +
                 ', sonda neutra ' + SNAP_K_PROBE + ') sustituye a la J de F4B-E19 como linea base activa del Controller; es la que cita P-01 (V43-08)'),
    (CO + 'owner-f4b-opaque-collections-decision.md', 'archivo completo (F4B-E21): en el resto de F4B, la sesion pone byte a byte en la delegacion las '
                                                      'ocho colecciones opacas del contrato; la comparacion byte a byte, A1-A8 y nc4 se hacen sobre la delegacion final'),
    (CO + 'owner-pbt1b-rework-decisions.md', 'archivo completo (F4B-E22): M10 y la mitad (a) de M14, ratificadas solo para PB-T1b; este contrato lleva el '
                                             'texto correcto (INV-PBT3A-13, PBT3A-C13 y la CI del RED con build-plugin omitido)'),
    (CO + 'owner-f4b-dpb07-extensiondata-decisions.md', 'pregunta 2 (F4B-E24): la ExtensionData anidada, "Basta asi (Recomendado)": el panel mantiene el '
                                                        'rechazo de AUTH-13 y PB-T3 lo fija con una prueba (CT-PBT3A-04); precondicion 8 del paquete'),
    (PILOT + '/F4B-DRAFTS/pbfull/owner-return-extensiondata.md', 'la vuelta al Owner por la ExtensionData anidada, respondida por F4B-E24'),
    (F4B_E25, 'archivo completo (F4B-E25): la regla de F4B sobre las rutas UNIT_CHANGE en una correccion, con sus tres condiciones; las EXTERNAL, sin '
              'excepcion. PB-T3a solo crea archivos y no declara rutas UNIT_CHANGE (section 6.0 del paquete); precondicion 8'),
    (F4B_E25_RET, 'la vuelta al Owner por la parada de la primera verificacion del intento 2 de PB-T1b (16.3), respondida por F4B-E25'),
    (CO + 'owner-pbt3-d1-decision.md', 'archivo completo (F4B-E29): D-1 del paquete (section 8.2), "Un solo codigo (Recomendado)", la opcion (A); '
                                       'decide el contrato de PB-T3b y no cambia el de PB-T3a (precondicion 4 del paquete)'),
    (CO + 'owner-pbt2-decisions.md', 'archivo completo (F4B-E28): Q-PBT2-1 y Q-PBT2-2 (A), con la misma compuerta en "Aplicar" y en PREPARE (A2-7 sin '
                                     'objeto; V42-11); Q-PBT2-4, la emision condicional de PB-T2a a PB-T2c, que no cubre PB-T3a (precondicion 10)'),
    (ISSUE_AUTH[1], 'archivo completo (F4B-E35): pregunta 3, "Si, con esa condicion (Recomendado)": la emision condicional de PB-T3a, PB-T3b y '
                    'PB-T3c, en ese orden y cada una tras el cierre de la anterior, si su comprobacion acotada da 0 bloqueantes y 0 importantes sin '
                    'resolver y la remedicion no cambia nada material; vuelve al Owner por una decision nueva, un control con una firma no prevista, '
                    'un intento 3 o un efecto material sin medir (precondicion 10; V43-07). Pregunta 4, "Si, con esas condiciones (Recomendado)": la '
                    'regla de la reparacion del RED, RR-1 a RR-7, que el contrato incorpora en PBT3A-C07 (la excepcion), PBT3A-C18, INV-PBT3A-02 (la '
                    'cadena RR-6) y la ExpectedEvidence; la verificacion del Controller la lee como parte de la entrega (RR-7: n.o 10 y n.o 11 de 16.9, '
                    'RR-1, RR-2, RR-4, RR-5 y RR-6 con el tramo RedSha..RedRepairSha, que en PB-T3a no toca src/, y la ayuda mecanica de RR-3 con la '
                    'lista cerrada de la clase reparada, del section 6.0 del paquete)'),
    (CO + 'owner-pbt2a-controls-decisions.md', 'pregunta 2 (F4B-E34): "Adaptar el sustituto (Recomendado)": el sustituto de nc2 adaptado a F4B, '
                                               'PBT2A-NC2-EXACT-WHITELIST-F4B-01, que el resto de F4B reutiliza con solo el id y el ambito cambiados y solo '
                                               'con la firma exacta de PD-F4-01; es el de PBT3A-C11 (V43-09)'),
    (SUBST_F4B, 'el sustituto de nc2 adaptado a F4B (F4B-E34), SHA-256 ' + SUBST_F4B_SHA + ', que PBT3A-C11 copia con solo el id y el ambito cambiados'),
    (DISP, 'archivo completo: F4B-E01 a ' + DISP_LAST),
    (PILOT + '/F4B-PBT1B-LOSSLESS-STATE-TYPE/task-close-package.md', 'el cierre de PB-T1b (F4B-E26): PushBackEditableState, su copiador y su comparador, '
                                                                    'el mapa de propiedad y PushBackEditorOutputGate, sobre los que se construye el borrador completo'),
    (PT1C_TCP, 'el cierre de PB-T1c (F4B-E31; PB-T1 completa): la ventana sobre el estado, con la costura EditableStateForTest que usa CT-PBT3A-01; '
               'su CI del GREEN 44463212627 (Core 19 088 y UI 2 439)'),
    (PBT1CC, 'el contrato de PB-T1c (F4B-E27): las paradas estandar (con P-01 en la J, que este contrato pasa a la K), las celdas, la raiz, las '
             'paradas de [DP] C08 y C12, la del limite de D2 (C17) y los filtros heredados'),
    (PILOT + '/' + PT2B_TASK + '/task-close-package.md', 'el cierre de PB-T2b (F4B-E38): la ventana en dos modos, el src de la base de PB-T3a (V43-10), '
                                                         'con la primera reparacion del RED y la Snapshot K'),
    (PBT2CC, 'el contrato emitido de PB-T2c (F4B-E39; AuthorityRevision 92232ed3): las listas curadas de tests/ con las clases de PB-T1 y de PB-T2, su '
             'AllowedWriteScope de pruebas, los MinSelected de PB-T2 que no bajan (V42-05 y V42-06), la linea base prevista del GREEN de PB-T2c y los '
             'textos de C07 (la excepcion de RR) y C18 que este contrato toma con el prefijo cambiado (V43-02)'),
    (PKG2, 'el paquete de PB-T2 ([P2]; ' + PKG2_VERSION + ' para PB-T2c): el orden de la fase y la base de PB-T3a (V42-01); el puerto al borrador y el '
           'lanzamiento tipado de PB-T2a; INV-PBT2B-04 (Recompute, LoadFromModel y ReadInputs congelados); los arrastres A2-4 (sustituido por A-T3-11), '
           'A2-7 (sin objeto), A2-11 (CT-PBT3B-01 (ii)) y A2-12 (CT-PBT3A-09 (3)); L-PBT2-12; la linea base prevista del GREEN de PB-T2c'),
    ('docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md', 'el paquete de PB-T0: CT-PB0-10 (la decision pura del borrado que iguala CT-PBT3A-08)'),
    (PBT0C, 'el contrato de PB-T0: la lista curada de tests/RackCad.Tests/ (medicion H-12)'),
    ('docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md', 'el paquete de PB-TC ([PKC-C]): el algoritmo del ForbiddenWriteScope'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 'el paquete de PB-TC2 ([PKC2]): las 17 paradas estandar y la raiz (section 6.7, '
                                                                 ':579-581) y las celdas (section 6.8, :655)'),
    (PBTC2C, 'el contrato de PB-TC2: las celdas'),
    (PILOT + '/F4-DRAFTS/gate-close.md', 'el cierre de F4: el PREPARE, MUTATE y POST del piloto; S2-03 (un paso de Deshacer); las guardas G-1 a G-10'),
] + [(p, 'paquete de F4-T4: el plan, la corrida, el puerto y el escritor del piloto y sus guardas, que PB-T3a no cambia') for p in F4T4] + [
    ('docs/initiatives/I-64-proposal-v5.md', 'la Proposal V5 congelada de I-64: D-10, D-13, D-15 (punto 5) y D-16'),
    ('docs/initiatives/I-64-f4-t1b-gate-contract-package.md', 'el patron de T1B ([T1B]): la caracterizacion vigente de la ventana clasica (55 casos en UI '
                                                               'y 10 en Core), que sigue en verde'),
    (T1BC, 'el contrato de T1B ([T1B-C])'),
    ('docs/initiatives/I-64-f4-t2-gate-contract-package.md', 'el regimen del RED con esqueleto inerte, que compila, no lanza y falla por asercion (:282)'),
    ('docs/initiatives/I-64-f4-t3-gate-contract-package.md', 'INV-F4T3A-13 (:371-374): las guardas pueden pasar en el RED y discriminan con autopruebas '
                                                             '(CT-PBT3A-12 a 14)'),
    ('docs/initiatives/I-64-f4-t3c-gate-contract-package.md', 'la regla RED (:297): completa y compilable, que no lanza y no cambia el comportamiento'),
    (PILOT + '/F4-DRAFTS/f4-dispositions.md', 'las fuentes de PD-F4-01 (:1103-1106, :1540, :520, :562) y PD-F4-04 (:1657-1660), F4-E56 (:1917-1928) y F4-E59'),
    (PILOT + '/F4-DRAFTS/coordinator/owner-f4t3-authorization.md', 'la firma literal de PD-F4-04 (:255-265)'),
    ('docs/automation/state/I-64.yml', 'estado vigente completo'),
    ('docs/automation/evidence/I-64-evidence.md', 'las secciones de F4B (231 y siguientes)'),
]
# las de la emision de PB-T3a (no las fija el paquete; el patron de PB-T1c): las de CUSTODY son autoridades
CUSTODY = [
    (PKG, 'archivo completo (' + PKG_VERSION + '): los registros de cambios v1 -> v4.5 (V2-01 a V2-34, V3-01 a V3-08, V4-01 y V4-02, V41-01 a V41-06, '
          'V42-01 a V42-13, V43-01 a V43-13, V44-01 a V44-06, de la comprobacion acotada CA-T3a, y V45-01 a V45-06, de la decision F4B-E42 y de la '
          'seccion 11 de CA-T3a), las '
          'precondiciones de emision 1 a 11 (la 10, la emision condicional de F4B-E35; la 11, la reemision de PB-T3a en el intento 2), section 1 '
          '(autoridades), section 2 (hechos), section 3 (D-PBT3-01 a D-PBT3-08, el modelo de PB-T3a, con ForFullWrite en D-PBT3-05, y D-PBT3-12 a '
          'D-PBT3-14), section 4 (las guardas) y 4.1 (el barrido de colisiones con las guardas existentes), section 5.0 a 5.2, 5.5 y 5.6 (las reglas, '
          'con el momento de cada lectura, los gestos que no cambian nada y la comprobacion de los archivos nuevos frente a las guardas, las recetas y '
          'CT-PBT3A-01 a 14), section 6.0 y 6.1 (lo comun, con la regla RR, la verificacion con una reparacion, la tabla cerrada de RR-3, el RED de la '
          'cadena en una correccion y la reemision de PB-T3a en el intento 2, y el contrato de PB-T3a), section 7 (riesgos), section 8 (decisiones '
          'del Owner; D-1, decidida en F4B-E29; F4B-E42) y section 10 (arrastres, con A-T3-11)'),
    (MEAS, 'el registro de mediciones del paquete (R-0 a R-18), con la remedicion de PB-T3a sobre el cierre de PB-T2c (R-19; precondiciones 2 y 9) y '
           'la lectura de G-9 de R-6 corregida para la reemision (R-20: G-9 barre tambien Application; F4B-E42, V45-02)'),
    (REVIEW, 'la revision independiente adversarial de la v1 (section 1 a 9: NECESITA v2, 1/11/16) y sus recomprobaciones de la v2 (section 10: '
             'NECESITA v3, 0/3/4), de la v3 (section 11: NECESITA v4, 0/1/1), de la v4 (section 12: LISTO PARA EMITIR TRAS REMEDICION, 0/0/1) y de la '
             'v4.2 (section 13: LISTO PARA EMITIR TRAS REMEDICION, 0/0/1), aplicadas en la v2, la v3, la v4, la v4.1 y la v4.2 (V42-13)'),
    (D3 + 'pbt3a-anchors-out.txt', 'la salida de la remedicion de PB-T3a sobre el cierre de PB-T2c (precondicion 2, section 6.0 del paquete y section 3 '
                                   'de la revision de compatibilidad): las anclas de R-1 a R-18 y las de W por su contenido, el censo de R-6, los '
                                   'recuentos y alcances con las clases de PB-T2, la linea base de la CI y la Snapshot vigente'),
    (D3 + 'independent-review-pbt3a-v43-check.md', 'la comprobacion acotada de PB-T3a, CA-T3a, sobre el contrato en seco y la v4.3 (precondicion 3 '
                                                   'y section 6.0 del paquete): EMITIBLE TRAS CORRECCIONES, B 0 / I 1 / M 4, sin decisiones del '
                                                   'Owner; I-01 (BeamDepth, gesto inerte) y M-01 a M-03 corregidas en la v4.4 (V44-01 a V44-04), M-04 '
                                                   'en el generador; su recomprobacion sobre la v4.4 (section 10: EMITIBLE, B 0 / I 0 / M 1; M-05, '
                                                   'la custodia de sus guiones); y la de la v4.5 para la reemision (section 11; precondicion 11)'),
    # la reemision (F4B-E42): la decision del Owner y el analisis de la sesion sobre el intento 1, autoridades del contrato reemitido
    (CO + 'owner-pbt3a-g9-decision.md', 'archivo completo (' + PT3A_STOP_ENTRY + '): "Renombrar ForWrite (Recomendado)": la parada PBT3A-C02 del '
                                        'intento 1 (' + RUN1 + ', antes del RED) por la guarda G-9; la v4.5 renombra el miembro cerrado ForWrite de '
                                        'D-PBT3-05 a ForFullWrite, sin tocar G-9 ni ninguna prueba existente; su comprobacion acotada; la reemision y la '
                                        'correccion en el intento 2 de 3'),
    (ANALYSIS, 'el analisis de la sesion para la correccion del intento 2 (' + PT3A_STOP_ENTRY + '): FailureClass StopCondition (PBT3A-C02), C-1 '
               '(ForFullWrite), C-2 (el RED entero: el intento 1 no hizo RED; ChainRedSha null y ChainRedFiles vacio), C-3 (el GREEN) y C-4 (la '
               'reparacion del RED sigue); los parches del intento 1 no son autoridad'),
    # el section 1 de la v4.2 la trata como autoridad (DU-3, resuelta por la sesion); compat_measure.py, su medicion, va en CUSTODY_EXTRA
    (D3 + 'compat-pbt3-vs-pbt2.md', 'la revision de compatibilidad con PB-T2, sobre la v4.1: COMPATIBLE CON REMEDICION, B/I/M 0/5/7, sin decisiones '
                                    'nuevas del Owner; los hallazgos I-01 a I-05 y M-01 a M-07 que aplica la v4.2 (V42-01 a V42-12) y, en su section 3, '
                                    'lo que vuelve a medir la remedicion sobre el cierre de PB-T2c'),
]
# las de CUSTODY_EXTRA: se custodian y no son autoridades, todas en F4B-DRAFTS/pbt3/ (las nombran el paquete, su registro y su revision)
# los cat3a_*.py son el metodo que cita el informe de CA-T3a (sus section 1, 2, 4.1, 5, 9 y 10): van, redactados como los demas (DU-11, DU-23);
# cat3a_v44.py y cat3a_bounded.py en su version de la seccion 10 (CA-T3a M-05)
CUSTODY_EXTRA = [D3 + n for n in ('v2-delta.md', 'v3-delta.md', 'v4-delta.md', 'v41-delta.md', 'v42-delta.md', 'v43-delta.md', 'v44-delta.md',
                                  'cat3a_fixtures.py', 'cat3a_custody.py', 'cat3a_compare.py', 'cat3a_c17.py', 'cat3a_bounded.py', 'cat3a_v44.py',
                                  'compat_measure.py', 'independent-review-v1-prompt.md', 'census_pins.py', 'census_methods.py',
                                  'census_methods2.py', 'census-pins-out.txt', 'census-methods-out.txt', 'census-methods2-out.txt', 'count_question.py',
                                  'check_tables.py', 'rv_anchors.py', 'rv_anchors_out.txt', 'independent-review-pbt3a-v43-check-prompt.md',
                                  'make_pbt3a_contract-dry.txt', 'v45-delta.md',
                                  # el metodo del barrido del section 4.1 de la v4.5 (V45-02) y el resumen de sus salidas (la sesion, DU-38)
                                  'name_check.py', 'name-check-sweep-summary.md',
                                  # los guiones y salidas de la seccion 11 de CA-T3a (su M-07)
                                  'cat3a_sweep.py', 'cat3a_sweep_sum.py', 'cat3a_g9.py', 'cat3a_reflect.py', 'cat3a_ctx.py', 'cat3a_chain.py',
                                  'cat3a_sweep_out.txt', 'cat3a_sweep_v44_out.txt', 'cat3a_reflect_out.txt')]
assert len(CUSTODY) == 8 and len(CUSTODY_EXTRA) == 39 == len(set(CUSTODY_EXTRA)) and not {p for p, _ in CUSTODY} & set(CUSTODY_EXTRA)
# en la reemision solo los nombres nuevos tienen que estar libres: los de la emision con la v4.4 ya estan en 2d876831
CUSTODY_FREE = [D3 + 'v45-delta.md', D3 + 'name_check.py', D3 + 'name-check-sweep-summary.md', CO + 'owner-pbt3a-g9-decision.md', ANALYSIS] + \
    [D3 + n for n in ('cat3a_sweep.py', 'cat3a_sweep_sum.py', 'cat3a_g9.py', 'cat3a_reflect.py', 'cat3a_ctx.py', 'cat3a_chain.py',
                      'cat3a_sweep_out.txt', 'cat3a_sweep_v44_out.txt', 'cat3a_reflect_out.txt')]
# la del cierre de PB-T2c (autoridad que solo existe despues); el contrato emitido de PB-T2c ya esta en EXISTING (F4B-E39)
CLOSE_AUTH = [(PT2C_TCP, 'el cierre de PB-T2c (' + (CLOSE_ENTRY or '<por rellenar>') + '): la BaseSha de PB-T3a desciende de el (V42-01), con su '
                         'linea base de la CI'),
              # la decision del Owner del cierre de PB-T2c: la firma nueva de nc1 para el resto de F4B; los contratos de PB-T3 que no nombran su
              # sustituto lo usan por esta decision (su punto 3), asi que entra como autoridad sin cambiar la v4.4 ni PBT3A-C11
              (CO + 'owner-pbt2c-nc1-decision.md', 'archivo completo (' + (CLOSE_ENTRY or '<por rellenar>') + '): "Aceptar y ampliar la firma '
                                                   '(Recomendado)": la firma nueva de nc1 PD-F4B-01, la cascada del SHA falso, condiciones (a) a (f), para '
                                                   'el resto de F4B, con PD-F4-04 como caso particular; su sustituto PBT3x-NC1-CASCADE-SIGNAL-01, con '
                                                   'autoprueba y revision independiente antes de los controles de PB-T3a, registrado como nc1 = '
                                                   'SATISFIED_UNDER_PBT3x_NC1_CASCADE_SUBSTITUTE; cualquier otra variacion vuelve al Owner')]
# la autorizacion de emision (F4B-E35, pregunta 3) es una autoridad de EXISTING, con la regla RR (pregunta 4): no se repite
ISSUE_AUTHS = [(ISSUE_AUTH[1], 'archivo completo (' + ISSUE_AUTH[0] + '): la autorizacion de emision de PB-T3a')] \
    if ISSUE_AUTH and ISSUE_AUTH[1] not in [p for p, _ in EXISTING] else []
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D12 y D13 (sin modelo paralelo; la caracterizacion no se edita para que pase)'),
    ('docs/AUTOMATION_PLAN.md', 'section 16.3, 16.5, 16.8, 16.9, 16.10 y 16.11'),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4)'),
    ('docs/automation/agent-execution/routing.md', 'archivo completo'),
    ('docs/automation/agent-execution/model-catalog.md', 'celdas medidas y vigentes para Controller y Worker'),
]
_auth_all = CUSTODY + EXISTING + CLOSE_AUTH + ISSUE_AUTHS + UNIT_CHANGE + EXTERNAL
assert len({p for p, _ in _auth_all}) == len(_auth_all), 'una autoridad repetida'
for p, _ in EXISTING + EXTERNAL + UNIT_CHANGE:
    assert in_head(p), ('autoridad inexistente en la revision', p)
assert not {p for p, _ in CUSTODY} & {p for p, _ in EXISTING}
ISSUE_TXT = (('%s, pregunta 3, la emision condicional de PB-T3 (%s): PB-T3a, PB-T3b y PB-T3c en orden, cada una tras el cierre de la anterior, con '
              'su comprobacion acotada a 0 bloqueantes y 0 importantes sin resolver y la remedicion sin cambio material'
              % (ISSUE_AUTH[0], os.path.basename(ISSUE_AUTH[1])))
             if ISSUE_AUTH else '<la autorizacion de emision de PB-T3a del Owner, por rellenar>')
CLOSE_TXT = ('%s, %s' % (PT2C_CLOSE[:8], CLOSE_ENTRY)) if CLOSE_ENTRY else '<cierre de PB-T2c por rellenar>'
c = {
    'Schema': 'rackcad-gate-contract/v1', 'Unit': 'I-64', 'Gate': 'F4B', 'TaskId': TASK,
    'Objective': OBJECTIVE,
    'AuthorityRevision': AR, 'MainSha': MAIN,
    'Authorities': [{'Path': p, 'Section': s, 'Class': 'UNIT_DOC'} for p, s in CUSTODY + EXISTING + CLOSE_AUTH + ISSUE_AUTHS] + [
        {'Path': CONTRACT_REL, 'Section': 'este contrato', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_CHANGE'} for p, s in UNIT_CHANGE]
      + [{'Path': p, 'Section': s, 'Class': 'EXTERNAL'} for p, s in EXTERNAL],
    'AllowedWriteScope': ALLOWED,
    'ForbiddenWriteScope': FORBIDDEN,
    'Invariants': INV,
    'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
    'ExpectedEvidence': EVID, 'StopConditions': STOPS, 'EligibleCells': cells, 'RoutingEnforcement': 'required', 'CorrectionsAuthorized': True,
    'IssuedBy': ('Sesion principal de I-64 (Claude), reemision en el intento 2 de 3 por ' + PT3A_STOP_ENTRY + ' (owner-pbt3a-g9-decision.md, '
                 '"Renombrar ForWrite (Recomendado)"): el intento 1 (' + RUN1 + ') paro antes del RED con PBT3A-C02, porque el esqueleto de la v4.4 '
                 'chocaba con la guarda G-9; la v4.5 renombra ForWrite a ForFullWrite, con su comprobacion acotada (' + (BOUNDED_RESULT or
                 '<resultado por rellenar>') + '), y la correccion sigue el analisis de la sesion; sobre la emision de ' + PT3A_EMIT_ENTRY + ' (X '
                 + PT3A_EMIT_X[:8] + ', BASE ' + PT3A_EMIT[:8] + ') por ' + ISSUE_TXT + ': la emision de la primera subtarea de PB-T3 (PB-T3a) tras el cierre de PB-T2c ('
                 + CLOSE_TXT + '; PB-T2 va delante, V42-01), con su remedicion y su comprobacion acotada (precondiciones 2 y 3 del paquete); F4B-E31, el '
                 'cierre de PB-T1c; F4B-E28, las decisiones de PB-T2, cuya pregunta 4 no cubre PB-T3a; [OD] (la fila de PB-T3, D-PB-04, D-PB-05 '
                 'y D-PB-10; owner-f4b-disposition.md); F4B-E11, D2 (owner-d1-d2-decisions.md); las decisiones de procedimiento '
                 'owner-f4b-procedure-decisions.md (F4B-E03); F4B-E37, la Snapshot K (owner-config-snapshot-k-decision.md); F4B-E34, el sustituto de '
                 'nc2 adaptado a F4B (owner-pbt2a-controls-decisions.md, pregunta 2); F4B-E35, pregunta 4, la regla RR de la reparacion del RED; '
                 'F4B-E38, el cierre de PB-T2b; F4B-E39, la emision de PB-T2c; F4B-E40, el cierre de PB-T2c, con la firma nueva de nc1 PD-F4B-01 y su '
                 'sustituto para el resto de F4B (owner-pbt2c-nc1-decision.md); F4B-E21, las colecciones '
                 'opacas que pone la sesion (owner-f4b-opaque-collections-decision.md); F4B-E22 (owner-pbt1b-rework-decisions.md); F4B-E24, la '
                 'ExtensionData anidada (owner-f4b-dpb07-extensiondata-decisions.md, pregunta 2); F4B-E25, la regla de F4B sobre las rutas UNIT_CHANGE '
                 'en una correccion (owner-f4b-correction-unit-change-decision.md); F4B-E29, D-1, que decide PB-T3b y no cambia PB-T3a '
                 '(owner-pbt3-d1-decision.md); y el paquete del contrato ' + PKG_VERSION + ', con su revision independiente, sus cuatro recomprobaciones, '
                 'la revision de compatibilidad con PB-T2 y la comprobacion acotada de PB-T3a'),
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
# la variante (A') no elegida (F4B-E29): tampoco en el contrato entero, con lo copiado de los contratos anteriores
assert VARIANT.encode('ascii') not in b, "el contrato lleva la variante (A') no elegida"
# lo que depende de la emision de PB-T3a: la autorizacion del Owner y las rutas de CUSTODY y CUSTODY_EXTRA; los revisados, byte a byte
soft(ISSUE_AUTH is not None, 'emision: falta ISSUE_AUTH, la autorizacion de emision de PB-T3a del Owner (precondicion 10); IssuedBy lleva un marcador')
BOUNDED_CHECK = D3 + 'independent-review-pbt3a-v43-check.md'  # el nombre que fijo la sesion (DU-2, DU-18)


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


soft(BOUNDED_RESULT, 'reemision: falta BOUNDED_RESULT, el veredicto literal de la comprobacion acotada de la v4.5 (la seccion 11 de CA-T3a; '
                     'precondicion 11; F4B-E42)')
assert BOUNDED_RESULT is None or (all(ord(ch) < 0x80 for ch in BOUNDED_RESULT) and
                                  re.match(r'EMITIBLE(?: TRAS CORRECCIONES)?[.,;]', BOUNDED_RESULT)), \
    ('BOUNDED_RESULT: solo la familia EMITIBLE (EMITIBLE o EMITIBLE TRAS CORRECCIONES; DU-18)', BOUNDED_RESULT)
# la reemision (precondicion 11): la ultima recomprobacion de CA-T3a es la de la version del paquete, y BOUNDED_RESULT la nombra
assert BOUNDED_RESULT is None or ('recomprobacion %s: ' % PKG_VERSION) in BOUNDED_RESULT, \
    ('BOUNDED_RESULT no nombra la recomprobacion de la %s (precondicion 11)' % PKG_VERSION, BOUNDED_RESULT)
BOUNDED_SRC = None  # de donde leyo bounded_check el informe de CA-T3a: en seco, el local (--bounded-file) manda, porque crece antes del commit X
if BOUNDED_FILE:
    _rep = open(BOUNDED_FILE, 'rb').read().decode('utf-8')
    BOUNDED_SRC = '--bounded-file ' + os.path.basename(BOUNDED_FILE)
elif in_head(BOUNDED_CHECK):
    _rep = show(BOUNDED_CHECK)
    BOUNDED_SRC = BOUNDED_CHECK + ' en ' + REV_SHA[:8]
BOUNDED_LAST = None  # la version de la ultima recomprobacion del informe
if BOUNDED_SRC:
    _vers = re.findall(r'(?m)^## ' + chr(0xA7) + r'\w+\. Recomprobaci' + chr(0xF3) + r'n (?:sobre|de) la (v\d+(?:\.\d+)?)\b', _rep.replace('\r\n', '\n'))
    BOUNDED_LAST = _vers[-1] if _vers else None
    if BOUNDED_RESULT and not BOUNDED_FILE and BOUNDED_LAST != PKG_VERSION:
        # el custodiado de la revision es el de antes de la reemision (sin su seccion 11): AVISO en seco, error al escribir
        soft(False, 'reemision: el informe de CA-T3a custodiado en la revision es anterior (su ultima recomprobacion es de la %s); el commit X '
                    'trae su seccion 11' % BOUNDED_LAST)
    elif BOUNDED_RESULT:  # con el marcador sin rellenar, el AVISO de abajo; al escribir, el assert final
        assert BOUNDED_LAST == PKG_VERSION, ('la ultima recomprobacion de CA-T3a no es la de la %s' % PKG_VERSION, BOUNDED_LAST)
        _err = bounded_check(_rep, BOUNDED_RESULT, PKG_LINES)
        assert not _err, ('la comprobacion acotada de PB-T3a', BOUNDED_SRC, _err)
    else:
        soft(BOUNDED_LAST == PKG_VERSION, 'reemision: la ultima recomprobacion de CA-T3a (%s) es de la %s, no de la %s: falta la seccion 11'
             % (BOUNDED_SRC, BOUNDED_LAST, PKG_VERSION))
for p, _ in ISSUE_AUTHS:
    soft(in_head(p), 'emision: la autorizacion de emision de PB-T3a, aun no en la revision: %s' % p)
for p, _ in CLOSE_AUTH:
    soft(in_head(p), 'cierre o emision de PB-T2c: autoridad aun no en la revision: %s' % p)


# ---- local_paths (inicio)
def local_paths(text):
    """Las rutas locales de un archivo a custodiar (DU-11; el repositorio es publico): una ruta con letra de unidad (X seguida de dos puntos y una
    barra), la carpeta de perfiles de Windows seguida de una barra, o la carpeta de datos de aplicacion del perfil. No busca nombres de personas:
    eso lo cubre la redaccion comun. Devuelve las coincidencias distintas (vacia si no hay). Las palabras de la expresion van con una letra entre
    corchetes ([e], [D]): la misma expresion, sin que su texto case con FORBIDDEN de la redaccion comun cuando el generador se custodia."""
    import re
    return sorted(set(m.group(0) for m in re.finditer(r'(?<![A-Za-z0-9])[A-Za-z]:[\\/]|\bUs[e]rs[\\/]|\bApp[D]ata\b', text)))
# ---- local_paths (fin)


for p, _ in CUSTODY:
    if soft(in_head(p), 'emision: a custodiar con la emision de PB-T3a, aun no en la revision: %s' % p):
        _lp = local_paths(show_bytes(p).decode('utf-8', 'replace'))
        assert not _lp, ('rutas locales en un custodiado (DU-11: se redacta antes de custodiar)', p, _lp)
        # en la reemision, lo custodiado con la v4.4 (el paquete, CA-T3a sin su seccion 11) lo sustituye el commit X: AVISO en seco, error al escribir
        if p in CUSTODY_SHA256:
            soft(hashlib.sha256(show_bytes(p)).hexdigest() == CUSTODY_SHA256[p],
                 'reemision: el custodiado en la revision no es el revisado (sha256; la %s lo sustituye en el commit X): %s' % (PKG_VERSION, p))
        if p in CUSTODY_HEADS:
            _lines = lines_of(p)
            soft(all(any(l.startswith(h) for l in _lines) for h in CUSTODY_HEADS[p]),
                 'reemision: al custodiado le faltan apartados (%s; los trae el commit X): %s'
                 % (', '.join(h.strip().replace(chr(0xA7), 'section ') for h in CUSTODY_HEADS[p] if not any(l.startswith(h) for l in _lines)), p))
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
    print('paquete leido:', PKG_SRC, '->', PKG_IN, '| sha256', hashlib.sha256(PKG_RAW).hexdigest()[:16] if PKG_RAW else '-',
          '| textos de la %s comprobados:' % PKG_VERSION, ', '.join(PKG_CHECKS) if PKG_CHECKS else 'ninguno')
    print('contrato de PB-T2c:', T2C_SRC, '| tests de PB-T3a: Core %d y UI %d (PB-T2c: %d y %d, mas su clase) | clases de PB-T2 en la revision: %d de 9'
          % (len(CORE_T), len(UI_T), len(CORE_T2C), len(UI_T2C), 9 - len(T2_MISSING)))
    print('W por contenido (V42-10), medida sobre la revision; el contenido, el del cierre de PB-T1c; la adopcion y su catch, los de W:659-704 y '
          ':662-670 del cierre de PB-T1b; Recompute, LoadFromModel y ReadInputs iguales al cierre de PB-T1c (INV-PBT2B-04):', '; '.join(W_REPORT))
    print('Pasada de [PKT1] section 5.0 == el de PushBackEditableStateLoadTests (%d sentencias); I58F1Matrix.Nested("pushback"): %d rutas (%d + %d); '
          'Fixtures/I64: %d' % (PASADA_STMTS, len(NESTED_STRUCTURE) + len(NESTED_PUSHBACK), len(NESTED_STRUCTURE), len(NESTED_PUSHBACK), len(FIXTURES)))
    print('CT-PBT3A-04: diagnostico [%s] y pista [%s] (las constantes de IR:23, :35 y :56)' % (ascii_fold(CT04_DIAG), ascii_fold(CT04_HINT)))
    print('PBT3A-C13: lista cerrada de D-PBT3-02 a D-PBT3-07 = %d tipos, %d constructores (%s) y %d miembros: %d declaraciones; nombres nuevos libres en '
          'src/ y tests/: %d de %d' % (len(CLOSED_EXPECTED), sum(1 for v in CLOSED_EXPECTED.values() if v[2]),
                                       ', '.join('%s %s' % (v[2], k) for k, v in CLOSED_EXPECTED.items() if v[2]),
                                       sum(len(v[3]) for v in CLOSED_EXPECTED.values()), CLOSED_COUNT, len(NEW_TYPES), len(NEW_TYPES)))
    _free = [p for p in CUSTODY_FREE if not in_head(p)]
    print('nombres de custodia nuevos de F4B-DRAFTS/pbt3/ libres en la revision: %d de %d' % (len(_free), len(CUSTODY_FREE)))
    print('Snapshot K (F4B-E37): ConfigSha256 %s y sonda neutra %s con Pass, la decision con la respuesta literal | PBT3A-C11: el sustituto de nc2 de '
          'F4B-E34 en la revision con SHA-256 %s | C07 (la excepcion de RR) y C18 == los de PB-T2c mas el punto final del paquete | F4B-E35: preguntas 3 y 4 y RR-1 a RR-7 | '
          'ISSUE_AUTH = %s, pregunta 3' % (SNAP_K_SHA[:8], SNAP_K_PROBE, SUBST_F4B_SHA[:8], ISSUE_AUTH[0]))
    print('BOUNDED_RESULT: %s | bounded_check: %s | ultima recomprobacion de CA-T3a: %s' % (
        BOUNDED_RESULT or 'por rellenar',
        (('sin errores sobre ' + BOUNDED_SRC) if BOUNDED_RESULT else ('no corre sin BOUNDED_RESULT (informe: %s)' % BOUNDED_SRC)) if BOUNDED_SRC
        else 'sin informe (ni en la revision ni --bounded-file)', BOUNDED_LAST))
    print('reemision (F4B-E42): sobre %s (%s), sin cambios en src/ ni en tests/ desde ella; %s en f4b-dispositions.md: %s' % (
        PT3A_EMIT[:8], PT3A_EMIT_ENTRY, PT3A_STOP_ENTRY, 'si' if E42_OK else 'no (lo entrega el commit X)'))
    print('tabla de RR-3 (V44-02 y V44-03): %s' % RR3_SUMMARY)
    print('D1: needs de build-plugin =', _needs, '| Snapshot K en la revision:', in_head(SNAP_K), '| F4B-E30: clases de UI de los filtros que crean '
          'el configurador real: %d (de %d archivos; de PB-T2, %d en la revision)' % (len(_cfg), len(UI_FILES), len(UI_T2_FILES)),
          "| (A') en el contrato: %d" % b.count(VARIANT.encode('ascii')), '| paquete de PB-T2 en la revision:', PKG2_IN)
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
    if '--print-json' in ARGS:  # los bytes exactos del contrato (sin la traduccion de fin de linea de la consola)
        sys.stdout.flush()
        sys.stdout.buffer.write(b)
    sys.exit(0)
assert PT2C_CLOSE and CLOSE_ENTRY and CI_BASE_T2C and ISSUE_AUTH and BOUNDED_RESULT and in_head(PBT2CC) and in_head(BOUNDED_CHECK), \
    'faltan PT2C_CLOSE, CI_BASE_T2C, ISSUE_AUTH, BOUNDED_RESULT, la comprobacion acotada o el contrato de PB-T2c'
assert not WARN, WARN
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
