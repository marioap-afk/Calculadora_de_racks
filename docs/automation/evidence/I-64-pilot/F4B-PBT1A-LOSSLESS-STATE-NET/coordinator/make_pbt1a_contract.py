"""Contrato de gate de F4B-PBT1A-LOSSLESS-STATE-NET (ASCII), desde el paquete del contrato de PB-T1 v2.4
(docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md, sections 5.0, 5.2, 6.0 y 6.1) y el patron de make_pbtc2_contract.py.
El ForbiddenWriteScope de src/ sale del algoritmo de [PKC-C] (cover()): por cada entrada de src/ en HEAD se emite <dir>/ solo si
ninguna ruta permitida queda debajo; si queda alguna, se desciende; los archivos se emiten uno a uno.
Antes de escribir, comprueba sobre HEAD los textos de los quince mutantes de la tabla cerrada (section 5.2 del paquete).
Uso: python make_pbt1a_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print]
Con --dry no escribe nada: comprueba e imprime los recuentos y los bytes (con --print, ademas, el ForbiddenWriteScope numerado).
Al custodiarlo, la ruta local del worktree se redacta como en PB-T0 ('<worktree-de-I-64>')."""
import json, os, re, sys, subprocess, hashlib
WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree
os.chdir(WT)
ISSUED, AR, MAIN = sys.argv[1], sys.argv[2], sys.argv[3]
TASK = 'F4B-PBT1A-LOSSLESS-STATE-NET'
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'


def in_head(path):
    return subprocess.run(['git', 'cat-file', '-e', 'HEAD:' + path.rstrip('/')], capture_output=True).returncode == 0


def show(path):
    return subprocess.run(['git', 'show', 'HEAD:' + path], capture_output=True).stdout.decode('utf-8')


T1BC = PILOT + '/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json'
PBTCC = PILOT + '/F4B-PBTC-DECLARED-CORRECTIONS/coordinator/gate-contract.json'
PBTC2C = PILOT + '/F4B-PBTC2-DECLARED-CORRECTIONS/coordinator/gate-contract.json'
PBT0C = PILOT + '/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json'
t1b = json.loads(show(T1BC))
pbtc2 = json.loads(show(PBTC2C))
P_STOPS = [dict(s) for s in t1b['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])]
assert [s['Id'] for s in P_STOPS] == ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
assert P_STOPS[9]['Text'] == 'config.toml no longer matches accepted Snapshot A, B, C or D'
P_STOPS[9]['Text'] = ('config.toml no longer matches the accepted Snapshot in force (today Snapshot I, '
                      'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json)')
assert P_STOPS == [s for s in pbtc2['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])], 'las paradas estandar difieren de las de PB-TC2'

UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
WC = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs'
WX = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml'
PRODUCT = [W, WC]
NEW = [UT + 'PushBackStateNet%s.cs' % n for n in ('Support', 'RackControlsTests', 'CellAndFrontTests', 'CompositeTests',
                                                  'ModuleAndSafetyTests', 'ActionsTests', 'FingerprintTests')]
ALLOWED = PRODUCT + NEW
assert len(ALLOWED) == 9 == len(set(ALLOWED))
assert all(os.path.isfile(p) and in_head(p) for p in PRODUCT), [p for p in PRODUCT if not (os.path.isfile(p) and in_head(p))]
assert all(not os.path.exists(p) and not in_head(p) for p in NEW), [p for p in NEW if os.path.exists(p) or in_head(p)]

# La tabla cerrada de mutantes (section 5.2 del paquete v2.4, remedida sobre b5c09f0b en la v2.3): (id, archivo, linea, texto exacto sin sangria).
MUTANTS = [
    ('MU-T1A-01', W, 874, 'NumberFronts = NumberFrontsCheck.IsChecked == true,'),
    ('MU-T1A-02', W, 852, 'generalPallet.Weight, WeightUnitBox.SelectedItem as string ?? "kg"),'),
    ('MU-T1A-03', W, 817, 'DepthStartPosition = IntVal(DepthStartBox, front.DepthStartPosition),'),
    ('MU-T1A-04', W, 827, 'IntermediateBeamDepth = SelectedPeralte(CellIntermediatePeralteBox, cell.IntermediateBeamDepth)'),
    ('MU-T1A-05', WC, 209, 'composite.SetCentralSeparator(CentralSeparatorCheck.IsChecked == true);'),
    ('MU-T1A-06', W, 2133, 'var height = DerivedPostLineHeightBox.Value;'),
    ('MU-T1A-07a', W, 3684, '=> !field.Value.HasValue || SameShown(field.Value.Value, current) ? current : field.Value.Value;'),
    ('MU-T1A-07b', W, 696, 'var standardRestoreLanded = moduleOwner.ModuleCommit?.StandardRestoreRequested == true; // I-64 F4B PB-TC (D-PB-11)'),
    ('MU-T1A-08', W, 1510, 'ForEachEditedSide(side => side.Structure.ApplyFrontValuesTo(ReadCellValues(side).Dynamic, frontTargets));'),
    ('MU-T1A-09', W, 1171, '=> MutateStructure(() => composite.SetSlotCount(state.Structure.Count + 1));'),
    ('MU-T1A-10', W, 3666, ': (IsMixed(field) ? sideValue : null);'),
    ('MU-T1A-11', W, 2682, 'ApplyBootSections(bootSectionA, bootSectionB);'),
    ('MU-T1A-12', W, 3568, 'BomButton.IsEnabled = canOutput;'),
    ('MU-T1A-13', W, 313, 'if (design == null) return;'),
    ('MU-T1A-14', W, 419, 'rackAnnotationScale = options.AnnotationScale > 0.0 ? options.AnnotationScale : 1.0;'),
]
assert len(MUTANTS) == 15 == len({m[0] for m in MUTANTS})
_text = {p: show(p).split('\n') for p in PRODUCT}
for mid, path, line, text in MUTANTS:
    assert _text[path][line - 1].strip() == text, ('mutante', mid, path, line, _text[path][line - 1].strip())
# los textos [TC2] son unicos en su archivo; MU-T1A-13 se ancla por linea (el mismo texto esta tambien en LoadDesignForNew, W:296)
for mid in ('MU-T1A-03', 'MU-T1A-04', 'MU-T1A-07a', 'MU-T1A-10'):
    _, path, line, text = next(m for m in MUTANTS if m[0] == mid)
    assert [i + 1 for i, l in enumerate(_text[path]) if l.strip() == text] == [line], mid


def tracked(prefix):
    out = subprocess.run(['git', 'ls-tree', '-r', '--name-only', 'HEAD', '--', prefix], capture_output=True, text=True).stdout.split('\n')
    return [p for p in out if p and p.startswith(prefix)]


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
# la forma de PB-TC2 (section 6.0 del paquete: 59 entradas de src/, con WX)
UIR = 'src/RackCad.UI/'
assert SRCF[:3] == ['src/RackCad.Application/', 'src/RackCad.Domain/', 'src/RackCad.Plugin/'], SRCF[:3]
ui_dirs = sorted(f[len(UIR):] for f in SRCF if f.startswith(UIR) and f.endswith('/') and f[len(UIR):].count('/') == 1)
ui_files = [f for f in SRCF if f.startswith(UIR) and '/' not in f[len(UIR):]]
assert ui_dirs == ['Controls/', 'Editor/', 'Preview/', 'RackFrames/', 'Shell/', 'StructuralSections/', 'Themes/', 'Views/', 'Workspace/'], ui_dirs
assert len(ui_files) == 34, len(ui_files)
assert sorted(f for f in SRCF if f.startswith(UIR + 'Systems/') and f.endswith('/')) == sorted(
    UIR + 'Systems/' + d for d in ('Cantilever/', 'Dynamic/', 'FlowBed/', 'Larguero/', 'Selective/'))
assert sorted(f for f in SRCF if f.startswith(UIR + 'Systems/PushBack/')) == sorted(
    [UIR + 'Systems/PushBack/' + n for n in ('PushBackBootSection.cs', 'PushBackDefenseSection.cs', 'PushBackMatrixCardModel.cs', 'PushBackPreviewModel.cs',
                                             'PushBackPreviewRenderer.cs', 'PushBackRearTopeDialogAdapter.cs', 'PushBackRearTopeSection.cs')] + [WX])
assert len(SRCF) == 59, len(SRCF)
assert SRCF == [f for f in pbtc2['ForbiddenWriteScope'] if f.startswith('src/')], 'src/ difiere del de PB-TC2'

# tests/: tests/RackCad.Tests/ entero; en UI, la lista curada de [PKC2] (51 archivos y Workspace/) mas las dos clases de PB-TC2.
PBTC2_UI = [f for f in pbtc2['ForbiddenWriteScope'] if f.startswith(UT)]
assert len(PBTC2_UI) == 52 and PBTC2_UI[-1] == UT + 'Workspace/', len(PBTC2_UI)
UI_FILES = [f[len(UT):] for f in PBTC2_UI[:-1]] + ['PushBackDeclaredCorrection2RoundingTests.cs', 'PushBackDeclaredCorrection2MixedBoxTests.cs']
assert len(UI_FILES) == 53 == len(set(UI_FILES)), len(UI_FILES)
assert len(tracked(UT + 'Workspace/')) == 24 and not any(a.startswith(UT + 'Workspace/') for a in ALLOWED)
TESTF = [TR] + [UT + f for f in UI_FILES] + [UT + 'Workspace/']
assert len(TESTF) == 55, len(TESTF)
assert not any(a == t or (t.endswith('/') and a.startswith(t)) for a in ALLOWED for t in TESTF)
ROOT = ['docs/', '.github/', 'eng/', 'tools/', 'assets/', 'deploy/', 'RackCad.sln', 'Directory.Build.props', 'Directory.Build.targets', 'global.json', 'NuGet.Config',
        'README.md', 'AGENTS.md', 'CLAUDE.md', '.editorconfig', '.gitattributes', '.gitignore']
assert ROOT == [f for f in pbtc2['ForbiddenWriteScope'] if not f.startswith(('src/', 'tests/'))], 'la raiz difiere de la de PB-TC2'
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == 131 == len(set(FORBIDDEN)), len(FORBIDDEN)
for f in FORBIDDEN:
    assert os.path.exists(f.rstrip('/')) and in_head(f), ('no existe', f)

PKG = 'docs/initiatives/I-64-f4b-pbt1-gate-contract-package.md'
MEAS = 'docs/initiatives/I-64-f4b-pbt1-measurements.md'
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
D1 = PILOT + '/F4B-DRAFTS/pbt1/'
SC = PILOT + '/F4B-DRAFTS/superseding-characterization.md'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
NEWS = 'PushBackStateNetSupport, PushBackStateNetRackControlsTests, PushBackStateNetCellAndFrontTests, PushBackStateNetCompositeTests, PushBackStateNetModuleAndSafetyTests, PushBackStateNetActionsTests y PushBackStateNetFingerprintTests'
INV = [
    'INV-PBT1A-01 (alcance): git diff --name-only BaseSha..CurrentSha = exactamente los siete archivos nuevos de ' + UT + ' (' + NEWS + '); '
    'git diff BaseSha..CurrentSha -- src vacio. BaseSha es el commit de emision de este contrato, sobre el cierre de PB-TC2 (cd47523b), con src y tests '
    'iguales al GREEN de PB-TC2 (b5c09f0b).',
    'INV-PBT1A-02 (RED y GREEN): el RED son los siete archivos nuevos y los quince mutantes de la tabla cerrada MU-T1A-01..14 (con 07a y 07b), juntos, en ' + W
    + ' (W) y ' + WC + ' (WC), cada uno la sustitucion exacta de una linea de BaseSha (section 5.2 del paquete); el GREEN es git restore --source=BaseSha de W y '
    'WC y no toca tests/. Criterio mecanico del RED, SOLO sobre el filtro con ExpectRed y con los resultados de la CI del RED: todos Failed; un Message sin el '
    'prefijo de tipo ^(?:[A-Za-z_][\\w`]*[.+])+[A-Za-z_][\\w`]* : ; la primera linea de StackTrace en un archivo bajo tests/; ninguno NotExecuted ni Skipped; '
    'y Failed == Selected >= 14. En el RED, los filtros sin ExpectRed pueden fallar SOLO por los mutantes, con la atribucion de la tabla T-8 (cada fallo a '
    'un mutante); Core sigue en success (ningun mutante toca un literal de I-55, Fact3, el desviador, G-10 ni T-SG). En el GREEN, todos en verde.',
    'INV-PBT1A-03 (cada mutante solo): cada mutante de la tabla, aplicado aislado sobre BaseSha, hace fallar por asercion el CT de su fila; la entrega lo '
    'transcribe con el TRX de cada corrida temporal.',
    'INV-PBT1A-04 (las lineas esperadas): las mide el Worker en BaseSha con codigo temporal (paso 1) y las transcribe en la entrega antes del RED; se fija '
    'lo que hay, y lo que contradice el diseno lleva INCUMPLIMIENTO CARACTERIZADO (CT-10, M-07).',
    'INV-PBT1A-05 (CT-14): el corpus (los 17 de tests/RackCad.Tests/Fixtures/I64, X-01, F-01, F-02 y F-03), las recetas de la section 5.0 y los guiones '
    'G-1 a G-6 de la section 5.2 del paquete, en ese orden; F-03 con su unico guion G-F03 y su linea, con pasadas-restaurar=2 y frontal=A esperados.',
    'INV-PBT1A-06 (las mediciones guardadas, M-06 de PB-T0): como artefactos con ruta relativa citados en la entrega.',
    'INV-PBT1A-07 (las redes): T1B, PB-T0, PB-TC, PB-TC2, Cantilever, las guardas de la section 4 del paquete y el censo del piloto, en verde en el GREEN; '
    'ninguna prueba existente se edita, se salta, se reactiva ni se borra.',
    'INV-PBT1A-08 (las pruebas de UI): las clases nuevas, public sealed, en [Collection(EditorDiscardPromptCollection.Name)], con [Fact] que acaban en un '
    'Assert.Equal; Ventana(), la carga dentro del try y Cerrar en finally; ninguna ventana mostrada salvo la de CT-11, fuera de pantalla (section 5.2); el '
    'aviso de descarte sustituido, con el sufijo " descartes=<n>" en la ultima linea de cada ventana, leido tras cerrarla; sin teclado ni foco real; '
    'prohibidos un MessageBox real, el BOM, Guardar en biblioteca, el configurador real e Insertar; ninguna asercion dentro de una costura; ningun fixture '
    'nuevo en Fixtures/I64; las recetas, solo en codigo.',
    'INV-PBT1A-09 (determinismo): el GUID fijo, la cultura invariante, los numeros con "R", el orden ordinal; ni hora ni aleatoriedad.',
    'INV-PBT1A-10 (las corridas locales, F4-E59): solo los filtros de RequiredTests en UI, nunca la suite completa de UI en local; la de Core, si; sin copias '
    'de arboles (MAX_PATH); las corridas temporales terminan en Assert.Fail(string.Join("\\n", lineas)), corren tambien dentro de '
    'EditorDiscardPrompt.Substitute (leccion IRC-M02 del cierre de PB-TC2), no entran en ningun commit y guardan TRX y log como artefactos; el perfil del '
    'usuario no cambia (INV-PBT0-06).',
    'INV-PBT1A-11 (la evidencia): toda ruta de worker-handoff.json es relativa y se comprueba que existe; TreeSha es el arbol real (git write-tree) o null; '
    'el Worker no escribe en docs/; una sustitucion se llamaria SUS3A-0n y es PBT1A-C02.',
    'INV-PBT1A-12 (reglas de F4B, [DP], D-PB-10 y sin AutoCAD): la planificacion segun PBT1A-C08; los controles segun PBT1A-C11; antes de un intento 3, '
    'siempre el Owner (PBT1A-C12); la comprobacion fisica es de Smoke-PB.',
    'INV-PBT1A-13 (la declaracion de autoridad, I-08): el Worker lista en UnexpectedFindings cada linea fijada que muestre un valor del diseno que cambia sin '
    'edicion del usuario (una perdida de clase c, d o nueva) o un control releido sin edicion que cambia el estado o la salida, distinta de c-01, c-02 y '
    'M-07; la sesion la lleva al Owner antes de emitir PB-T1b.',
    'INV-PBT1A-14 (las dos referencias de D2, F4B-E11): PB-T1a no cambia producto (INV-PBT1A-01), asi que no crea ninguna copia del estado ni muta un '
    'PushBackSystem o un RackModuleReconciliationResult; ninguna receta ni ningun mutante muta el rack resuelto ni el informe de modulos de una ventana viva.',
]
assert len(INV) == 14 and [i.split(' ')[0] for i in INV] == ['INV-PBT1A-%02d' % n for n in range(1, 15)]
EVID = [
    'Antes del Worker: gate-contract valido; delegacion del Controller valida; comparacion byte a byte, aceptacion A1-A8 y nc4 (primera delegacion de la '
    'cadena F4B-PBT1A); AllowedWriteScope de la delegacion = exactamente el del contrato; si hubo una replanificacion por PBT1A-C08, su registro.',
    'Paso 1, sobre BaseSha y antes del RED: las corridas temporales con Assert.Fail(string.Join("\\n", lineas)) que miden las lineas esperadas de CT-01 a CT-14, '
    'con su TRX y su log como artefactos, y su transcripcion en la entrega.',
    'Commit RED y push propios antes del GREEN: los siete archivos y los quince mutantes; CI del RED con ui-tests en failure y tests, build-ui y build-plugin en '
    'success, sin blame-hang ni timeout; los TRX del filtro con ExpectRed cumplen el criterio de INV-PBT1A-02; la tabla T-8 con cada prueba existente que falla '
    'en el RED atribuida a un mutante.',
    'Los quince mutantes aislados (INV-PBT1A-03): por mutante, la corrida temporal sobre BaseSha con solo ese mutante, el CT de su fila en Failed por asercion '
    'y su TRX.',
    'Commit GREEN y push: git diff --name-only BaseSha..CurrentSha = los siete archivos; git diff BaseSha..CurrentSha -- src vacio; CI del GREEN 4/4 con las '
    'suites completas, con los conteos de la section 6.1 del paquete (UI 2436 resultados: 0 Failed, 21 Skipped y 2415 Passed; Core 19070 Passed) o la '
    'diferencia explicada por el diff.',
    'En local: build Release de la solucion, Plugin incluido y sin avisos nuevos; los filtros de RequiredTests sobre el RED y sobre el GREEN; tres corridas '
    'seguidas en verde del filtro con ExpectRed sobre el GREEN; la prueba de la limpieza del perfil.',
    'worker-handoff.json conforme a su esquema con RED, GREEN, alcance real, trailer y conteos por filtro (Passed, Failed y Skipped); el mapa CT a metodo; '
    'rutas relativas comprobadas; TreeSha real o null; la lista de INV-PBT1A-13 en UnexpectedFindings (vacia si no hay ninguna).',
    'Despues: Controller verification valida; nc1, nc2 y nc3 segun README section 10, conservando cada salida ordinaria; un control que no da CONTROL PASS se '
    'resuelve SOLO como dice PBT1A-C11; cada uso se registra en F4B-DRAFTS/f4b-dispositions.md; la verificacion 14/14 sola no basta para cerrar.',
    'Sin AutoCAD: la ventana se prueba en UI.Tests con el producto real; la comprobacion fisica es de Smoke-PB.',
]
C11 = ('a negative control (nc1, nc2 or nc3) does not give CONTROL PASS: its ordinary output is kept as is and there are only two exits without returning to the Owner '
       '([DP] question 1, "Extender firmas exactas"): (1) nc2 with the EXACT signature of PD-F4-01 (source f4-dispositions.md:1103-1106: Scope in fail on the removed path; '
       'Contract in fail only by the exactness clause; EXECUTION_BLOCKED/STOP; applied literally at :1540: no check other than the real verification differs; the exactness '
       'clause includes Contract evidence citing the contrast of the narrowed AllowedWriteScope with the contract or with byte-fidelity-check.json, f4-dispositions.md:520, :562, '
       'and nothing else) -> substitute PBT1A-NC2-EXACT-WHITELIST-01, a copy of F4T1B-NC2-EXACT-WHITELIST-01 with ONLY the id and the scope changed, recorded as '
       'nc2 = SATISFIED_UNDER_PBT1A_EXACT_WHITELIST_SUBSTITUTE; (2) nc1 with the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265: false CurrentSha '
       'correctly detected; Identity = fail; Handoff also fails first only because the handoff itself contains the false SHA; outcome BLOCKED / STOP; all other intended checks '
       'match; as applied in F4, f4-dispositions.md:1657-1660, Ci in fail only as a consumer of the false CurrentSha is part of the signature, with Remote in pass) -> substitute '
       'PBT1A-NC1-IDENTITY-SIGNAL-01, a copy of F4T3C2-NC1-IDENTITY-SIGNAL-01 with ONLY the id and the scope changed, recorded as '
       'nc1 = SATISFIED_UNDER_PBT1A_NC1_IDENTITY_SIGNAL_SUBSTITUTE. Matching is decided never by check name: the control output is compared with the real verification check by '
       'check (the set of failing checks, the first failing check, Outcome and StopClass, and the evidence text); the only admitted differences are those listed for each signature '
       'in section 6.0 of the package; the session records the comparison table in f4b-dispositions.md. Any other signature or variation (PD-F4-05, PD-F4-06, nc2 with Ci or '
       'Trailer also in fail, any nc1 that does not reproduce the literal signature - including the PB-TC variation with Remote, Scope, Tests and Trailer also in fail, '
       'which the Owner accepted only once (F4B-E08), and the PB-TC2 case, where the substitute failed its property 5 and the Owner accepted nc1 only for PB-TC2 attempt 1 '
       '(F4B-E12) -, a substitute that does not give PASS, any failure of nc3, or an evidence text that cannot be assigned to an admitted difference) returns to the Owner '
       'before any substitute or acceptance; neither one-off acceptance extends to PB-T1a')
C17 = ('(F4B-E11, D2, IMPORTANT LIMIT) doing the task would need to MUTATE the resolved rack (PushBackSystem) or the module report (RackModuleReconciliationResult) '
       'inside the editing or draft lifetime, or to share by reference any other mutable object between a copy of the state and its original, or a measurement already '
       'finds such a mutation in the editing path: STOP and L-1 is reopened. The Owner text, literal: "This is NOT authorization for general shallow copying. It is '
       'exactly: two declared shared references. If later PB-T1/PB-T2/PB-T3 needs to mutate either: resolved rack; module report; inside the Workspace draft lifetime: '
       'STOP and reopen L-1. At that point the object must either: be cloned; be made immutable; or be replaced by a value snapshot appropriate to the draft model." '
       'PB-T1a creates no state and does not touch the Plugin; the stop holds for the whole chain (for example, if a mutant or a recipe needed to mutate the resolved '
       'rack or the report of a live window). The Worker chooses none of the three exits: the Owner does')
STOPS = P_STOPS + [
    {'Id': 'PBT1A-C01', 'Text': 'doing it right would require writing outside the AllowedWriteScope (W and WC only for the RED mutants, reverted by the GREEN, and the seven new test files)'},
    {'Id': 'PBT1A-C02', 'Text': 'an existing test or helper would have to be edited, skipped, re-enabled or deleted, or InternalsVisibleTo, the packages, a .csproj or tests/RackCad.Tests/Fixtures/I64 would have to change; a guard of section 4 of the package, the pilot census or a PB-T0, PB-TC or PB-TC2 test that does not stay green returns to the Owner (OB-T1B.3); a needed substitution (SUS3A-0n) is a package defect and returns to the session'},
    {'Id': 'PBT1A-C03', 'Text': 'an observable change of RACKEDITAR against the base appears (in PB-T1a the GREEN restores W and WC byte for byte and src does not change): STOP and back to the Owner (D-16, OB-RE.1)'},
    {'Id': 'PBT1A-C04', 'Text': '(I-06) a mutant of the table, applied alone on BaseSha, does not make the CT of its row fail by assertion; or a new CT fails on BaseSha without mutants; or it fails by exception and not by assertion; it is a contract defect and the Worker changes neither the CT nor the mutant'},
    {'Id': 'PBT1A-C05', 'Text': 'a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM window, Guardar en biblioteca, Insertar, the real keyboard or focus, or a write to the user profile or to disk'},
    {'Id': 'PBT1A-C06', 'Text': 'the full CI of the GREEN fails outside the new tests, reproduces on a second run and is not attributable to the new tests (attributable is REWORK; not reproduced is a BLOCKED rerun within the 16.8 limits)'},
    {'Id': 'PBT1A-C07', 'Text': 'the facts of section 2 of the package do not hold on the base in a way that changes a CT or the design; the line of a mutant of the table is not the one on BaseSha; the F-03 script does not give pasadas-restaurar=2 and frontal=A; or an expected line of a CT is not met on the GREEN; the Worker does not adjust the expectation'},
    {'Id': 'PBT1A-C08', 'Text': 'a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner'},
    {'Id': 'PBT1A-C09', 'Text': 'a test would hang, leave a window, a modal or a dispatcher alive, write to real user profile data or depend on machine-specific paths'},
    {'Id': 'PBT1A-C10', 'Text': 'doing it right would require correcting something outside the scope: a T-2 loss, M-07, pb-21-cola, the WORKSPACE mode, the pilot, D-PB-07 or a PB-TC or PB-TC2 correction'},
    {'Id': 'PBT1A-C11', 'Text': C11},
    {'Id': 'PBT1A-C12', 'Text': 'a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)'},
    {'Id': 'PBT1A-C17', 'Text': C17},
]
assert [s['Id'] for s in STOPS[17:]] == ['PBT1A-C%02d' % n for n in range(1, 13)] + ['PBT1A-C17'] and len(STOPS) == 30
UIP, CP = UT + 'RackCad.UI.Tests.csproj', TR + 'RackCad.Tests.csproj'
REQ = [
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackStateNet', 14, True),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization', 55, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization', 16, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionRoundingTests', 3, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionDimensionStyleTests', 2, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionPilotParityTests', 2, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionSideBNoticeTests', 2, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrectionStandardRestoreTests', 1, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection2', 4, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.Cantilever', 67, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeWindowTests', 52, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeLifecycleWiringTests', 7, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackCompositeLoadExistingModulesTests', 3, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackBothModeCellBeamOverrideTests', 10, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackModuleEditorWindowTests', 13, False),
    (UIP, 'FullyQualifiedName~RackCad.UI.Tests.PushBackAdvancedRackParametersWindowTests', 9, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization', 10, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization', 3, False),
    (CP, 'FullyQualifiedName~RackCad.Tests.PushBackModuleEditorCharacterizationTests', 15, False),
]
assert len(REQ) == 19 and [r for r in REQ if r[3]] == [REQ[0]]
for proj, _, _, _ in REQ:
    assert in_head(proj), proj
# las clases de PB-TC se filtran por clase: ninguna es subcadena de una clase de PB-TC2; el prefijo nuevo no es prefijo de ninguna clase existente
for _, f, _, e in REQ[3:8]:
    assert not any(f.split('~')[1].split('.')[-1] in n for n in ('PushBackDeclaredCorrection2RoundingTests', 'PushBackDeclaredCorrection2MixedBoxTests')), f
assert not [p for p in tracked('tests/') if 'PushBackStateNet' in p], 'el prefijo PushBackStateNet ya existe'
cells = json.loads(json.dumps(t1b['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna']
assert cells == pbtc2['EligibleCells'], 'las celdas difieren de las de PB-TC2'
UNIT_CHANGE = [
    (W, 'la ventana clasica de Push Back: solo los mutantes del RED (MU-T1A-01..04, 06..14), revertidos por el GREEN'),
    (WC, 'la parte compuesta de la ventana: solo el mutante del RED MU-T1A-05, revertido por el GREEN'),
]
EXISTING = [
    (PHASE, 'paquete de la fase F4B v3.1: OB-01.1b, OB-03.1, OB-05.1, OB-06.5, OB-10.1, OB-T1B.1 a OB-T1B.3, OB-RE.1, R-PB-01 y R-PB-09; la fila de PB-T1 del section 5'),
    (CO + 'owner-f4b-disposition.md', 'archivo completo ([OD]): la fila de PB-T1, D-PB-05, D-PB-06 y D-PB-10'),
    (CO + 'owner-f4b-procedure-decisions.md', 'archivo completo ([DP], F4B-E03): preguntas 1 y 2 (firmas exactas de los sustitutos y la replanificacion unica)'),
    (CO + 'owner-pbt1-pre-decisions.md', 'archivo completo ([OPT1], F4B-E09): preguntas 1 a 4, con la autorizacion condicional de emision'),
    (CO + 'owner-d1-d2-decisions.md', 'archivo completo ([OD2], F4B-E11): D1, D2 con sus diez condiciones y su limite, y PB-T1 PLAN (la remedicion y la emision normal)'),
    (CO + 'owner-pbtc-nc1-decision.md', 'la disposicion unica de nc1 en PB-TC (F4B-E08), que no se extiende a PB-T1a'),
    (CO + 'owner-pbtc2-nc1-decisions.md', 'la disposicion de nc1 en PB-TC2 solo para su intento 1 (F4B-E12), que no se extiende a PB-T1a'),
    (PILOT + '/F4B-DRAFTS/f4b-dispositions.md', 'archivo completo: F4B-E01 a F4B-E12'),
    (D1 + 'ob065-dossier.md', 'el expediente del analista: section 3 (los huecos de red) y section 4 (las guardas de T1B y de texto)'),
    (D1 + 'owner-return-l1.md', 'la vuelta L-1 al Owner, respondida por F4B-E11 (D2)'),
    (SC, 'el registro declarado ([SC]): PB-TC y PB-TC2 EJECUTADAS'),
    ('docs/initiatives/I-64-f4b-pbt0-gate-contract-package.md', 'el paquete de PB-T0: el regimen del RED por mutantes, CT-PB0-01 a 21, T-1, T-2, T-5, M-01, M-04, D-3, D-5 y CT-16'),
    (PBT0C, 'el contrato de PB-T0: el patron del RED por mutantes'),
    (PILOT + '/F4B-PBT0-CHARACTERIZATION/task-close-package.md', 'el cierre de PB-T0'),
    ('docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md', 'el paquete de PB-TC v2.2 ([PKC]): M-02 a M-05 y el precedente del RED con Core en success (:466)'),
    (PBTCC, 'el contrato de PB-TC'),
    ('docs/initiatives/I-64-f4b-pbtc2-gate-contract-package.md', 'el paquete de PB-TC2 v2 ([PKC2]): M-03, M-07, la celda del Controller (:655) y la forma de los alcances'),
    ('docs/initiatives/I-64-f4b-pbtc2-measurements.md', 'el registro de mediciones de PB-TC2'),
    (PBTC2C, 'el contrato de PB-TC2 ([PKC2-C]): el metodo del ForbiddenWriteScope, las paradas estandar con P-01 y las celdas'),
    (PILOT + '/F4B-PBTC2-DECLARED-CORRECTIONS/task-close-package.md', 'el cierre de PB-TC2 sobre b5c09f0b, la BaseSha de producto de PB-T1a'),
    ('docs/initiatives/I-64-f4-t1b-gate-contract-package.md', 'el patron de T1B ([T1B]): la caracterizacion vigente de la ventana clasica (55 casos en UI y 10 en Core)'),
    (T1BC, 'el contrato de T1B ([T1B-C]): las paradas estandar y las celdas'),
    (PILOT + '/F4-DRAFTS/f4-dispositions.md', 'las fuentes de PD-F4-01 (:1103-1106, :1540, :520, :562) y PD-F4-04 (:1657-1660), F4-E56 (:1917-1928) y F4-E59'),
    (PILOT + '/F4-DRAFTS/coordinator/owner-f4t3-authorization.md', 'la firma literal de PD-F4-04 (:255-265)'),
    ('docs/automation/state/I-64.yml', 'estado vigente completo'),
    ('docs/automation/evidence/I-64-evidence.md', 'las secciones de F4B (231 y siguientes)'),
]
CUSTODY = [  # se custodian en el commit de emision de PB-T1a (section 6.0 del paquete, Custodia)
    (PKG, 'archivo completo (v2.4): section 2 (hechos medidos y remedidos), section 3 (D-PBT1-01, D-PBT1-12, D-PBT1-14 y D-PBT1-15), section 5.0 y 5.2 (recetas, CT-PBT1A-01 a 14 y la tabla cerrada de mutantes), section 6.0 y 6.1 (el contrato), section 8 (decisiones del Owner y la remedicion) y section 10 (arrastres)'),
    (MEAS, 'el registro de mediciones del paquete, apartados A a O (N: la remedicion sobre el producto de PB-TC2; O: la guarda G-L1 de la v2.4)'),
    (D1 + 'independent-review-v1.md', 'la revision independiente IRT1-1 de la v1'),
    (D1 + 'independent-review-v2-check.md', 'la comprobacion acotada de la v2'),
    (D1 + 'independent-review-v23-check.md', 'la comprobacion acotada de la remedicion (v2.3): EMITIBLE TRAS CORRECCIONES 0/1/3, aplicada en la v2.4'),
]
assert len(CUSTODY) == 5 and len({p for p, _ in CUSTODY}) == 5
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D12 y D13 (sin modelo paralelo; la caracterizacion no se edita para que pase)'),
    ('docs/AUTOMATION_PLAN.md', 'section 16.5, 16.8, 16.9, 16.10 y 16.11'),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4)'),
    ('docs/automation/agent-execution/routing.md', 'archivo completo'),
    ('docs/automation/agent-execution/model-catalog.md', 'celdas medidas y vigentes para Controller y Worker'),
]
for p, _ in EXISTING + EXTERNAL + UNIT_CHANGE:
    assert in_head(p), ('autoridad inexistente en HEAD', p)
c = {
    'Schema': 'rackcad-gate-contract/v1', 'Unit': 'I-64', 'Gate': 'F4B', 'TaskId': TASK,
    'Objective': ('Fijar con pruebas de caracterizacion nuevas, sin cambiar el producto, la red de la ventana clasica de Push Back que PB-T1 necesita antes de '
                  'mover su estado: los huecos de D-5, M-03 de PB-TC y de PB-TC2, M-04, M-07, la observacion de CT-16, M-02 de PB-TC y D-3, y las huellas de '
                  'salida del corpus extendido (con F-03) por los guiones G-1 a G-6, segun el paquete del contrato de PB-T1 v2.4, remedido sobre el producto de '
                  'PB-TC2 (b5c09f0b), y la lista cerrada CT-PBT1A-01..14. El RED son las pruebas nuevas y los mutantes de la tabla cerrada MU-T1A-01..14 en W y '
                  'WC; el GREEN revierte los mutantes. Ninguna linea de src cambia entre BaseSha y CurrentSha; no se ejecuta AutoCAD. Si hiciera falta mutar el '
                  'rack resuelto o el informe de modulos dentro del tiempo de vida de la edicion: STOP y se reabre L-1 (decision D2 del Owner, F4B-E11).'),
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
                 '(docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbt1-pre-decisions.md), y F4B-E11, D2 y PB-T1 PLAN '
                 '(docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-d1-d2-decisions.md): la emision normal de la primera tarea de PB-T1 tras '
                 'la remedicion sobre el producto final de PB-TC2 y la comprobacion acotada; las decisiones de procedimiento owner-f4b-procedure-decisions.md '
                 '(F4B-E03); y el paquete del contrato v2.4 con su revision independiente IRT1-1, la comprobacion acotada de la v2 y la '
                 'comprobacion acotada de la remedicion'),
    'IssuedUtc': ISSUED,
}
assert len(c['Authorities']) == 39, len(c['Authorities'])
b = (json.dumps(c, indent=2, ensure_ascii=True) + '\n').encode('ascii')
assert all(x < 0x80 for x in b) and b'\\u' not in b, 'no ASCII'
assert not set(c['AllowedWriteScope']) & set(c['ForbiddenWriteScope'])
for a in c['AllowedWriteScope']:
    assert not any(a.startswith(f) for f in c['ForbiddenWriteScope'] if f.endswith('/')), a
TERMS = ['GATE PASS', 'GATE_PASS', 'Candidato', 'Candidate', 'FINAL_CANDIDATE_SHA', 'gate cerrado', 'gate closed', 'cierre del gate', 'rama integrada', 'integrada en main',
         'integrated into main', 'merged into main', 'lista para integrar', 'ready to merge', 'Owner Validation APROBADA', 'Owner Validation APPROVED']
for t in TERMS:
    assert not re.search(r'(?<![A-Za-z0-9_])' + re.escape(t) + r'(?![A-Za-z0-9_])', b.decode('ascii'), re.I), t
for bad in ('C:' + chr(92), 'C:' + '/', 'D:' + '/', 'Users', 'alej' + 'andra', 'App' + 'Data', 'scratchpad', '.co' + 'dex', 'hs' + '65'):
    assert bad not in b.decode('ascii'), bad
if '--dry' in sys.argv:
    custody_missing = [p for p, _ in CUSTODY if not in_head(p)]
    print('DRY allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), '(src', len(SRCF), 'tests', len(TESTF), 'root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'required', len(REQ), 'evidence', len(EVID), 'authorities', len(c['Authorities']), 'mutants-ok', len(MUTANTS), 'bytes', len(b),
          'sha256', hashlib.sha256(b).hexdigest().upper())
    print('a custodiar en la emision (aun no en HEAD):', len(custody_missing), custody_missing)
    if '--print' in sys.argv:
        for i, f in enumerate(FORBIDDEN, 1):
            print(i, f)
    sys.exit(0)
for p, _ in CUSTODY:
    assert in_head(p), ('falta custodiar antes de escribir el contrato', p)
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
