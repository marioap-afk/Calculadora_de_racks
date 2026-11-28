"""Contrato de gate de F4B-PBTC-DECLARED-CORRECTIONS (ASCII), desde el paquete del contrato de PB-TC v2.2
(docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md, section 6) y el patron de make_pbt0_contract.py.
El ForbiddenWriteScope de src/ sale del algoritmo de [PK0-C] (section 6.3 del paquete): por cada entrada de src/ en HEAD se emite
<dir>/ solo si ninguna ruta permitida queda debajo; si queda alguna, se desciende; los archivos se emiten uno a uno.
Uso: python make_pbtc_contract.py <IssuedUtc> <AuthorityRevision> <MainSha> [--dry] [--print]
Con --dry no escribe nada: comprueba e imprime los recuentos y los bytes (con --print, ademas, el ForbiddenWriteScope numerado).
Al custodiarlo, la ruta local del worktree se redacta como en PB-T0 ('<worktree-de-I-64>')."""
import json, os, re, sys, subprocess, hashlib
WT = '<worktree-de-I-64>'  # Redaccion para el repo publico: la ruta local del worktree
os.chdir(WT)
ISSUED, AR, MAIN = sys.argv[1], sys.argv[2], sys.argv[3]
TASK = 'F4B-PBTC-DECLARED-CORRECTIONS'
OUT = 'artifacts/orchestration/I-64/%s/coordinator/gate-contract.json' % TASK
PILOT = 'docs/automation/evidence/I-64-pilot'


def in_head(path):
    return subprocess.run(['git', 'cat-file', '-e', 'HEAD:' + path.rstrip('/')], capture_output=True).returncode == 0


def show(path):
    return subprocess.run(['git', 'show', 'HEAD:' + path], capture_output=True).stdout.decode('ascii')


T1BC = PILOT + '/F4-T1B-PILOT-CHARACTERIZATION/coordinator/gate-contract.json'
PBT0C = PILOT + '/F4B-PBT0-CHARACTERIZATION/coordinator/gate-contract.json'
t1b = json.loads(show(T1BC))
pbt0 = json.loads(show(PBT0C))
P_STOPS = [dict(s) for s in t1b['StopConditions'] if s['Id'][0] in 'SP']
assert [s['Id'] for s in P_STOPS] == ['S-02', 'S-03', 'S-04', 'S-05', 'S-07', 'S-08', 'S-12', 'S-13', 'S-14', 'P-01', 'P-02', 'P-03', 'P-04', 'P-05', 'P-06', 'P-07', 'P-08']
assert P_STOPS[9]['Text'] == 'config.toml no longer matches accepted Snapshot A, B, C or D'
P_STOPS[9]['Text'] = ('config.toml no longer matches the accepted Snapshot in force (today Snapshot I, '
                      'docs/automation/evidence/I-64-pilot/F4-ENVIRONMENT/snapshot-I/snapshot-i.json)')
assert P_STOPS == [s for s in pbt0['StopConditions'] if re.fullmatch(r'[SP]-\d\d', s['Id'])], 'las paradas estandar difieren de las de PB-T0'

UT, TR = 'tests/RackCad.UI.Tests/', 'tests/RackCad.Tests/'
W = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml.cs'
WC = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.Composite.cs'
WX = 'src/RackCad.UI/Systems/PushBack/RackPushBackSystemWindow.xaml'
PRODUCT = [W, WC, WX]
NEW = [UT + 'PushBackDeclaredCorrection%s.cs' % n for n in ('Support', 'RoundingTests', 'DimensionStyleTests', 'PilotParityTests', 'SideBNoticeTests',
                                                            'StandardRestoreTests')]
SUBST = [UT + 'Workspace/WorkspaceDraftPilotWindowLoadCharacterizationTests.cs', UT + 'PushBackDimensionViewsWindowTests.cs',
         UT + 'PushBackFullEditorCharacterizationRoundingLossTests.cs', UT + 'PushBackFullEditorCharacterizationModuleTests.cs']
ALLOWED = PRODUCT + NEW + SUBST
assert len(PRODUCT) == 3 and len(NEW) == 6 and len(SUBST) == 4 and len(ALLOWED) == 13 == len(set(ALLOWED))
assert all(os.path.isfile(p) and in_head(p) for p in PRODUCT + SUBST), [p for p in PRODUCT + SUBST if not (os.path.isfile(p) and in_head(p))]
assert all(not os.path.exists(p) and not in_head(p) for p in NEW), [p for p in NEW if os.path.exists(p) or in_head(p)]


def tracked(prefix):
    out = subprocess.run(['git', 'ls-tree', '-r', '--name-only', 'HEAD', '--', prefix], capture_output=True, text=True).stdout.split('\n')
    return [p for p in out if p and p.startswith(prefix)]


def cover(prefix):
    """Prefijos y archivos minimos que cubren todo prefix salvo las rutas permitidas (algoritmo de [PK0-C])."""
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
# forma esperada del paquete (section 6.3): 3 proyectos, 9 carpetas y 34 archivos de la raiz de src/RackCad.UI, 5 sistemas y 7 hermanos
UIR = 'src/RackCad.UI/'
assert SRCF[:3] == ['src/RackCad.Application/', 'src/RackCad.Domain/', 'src/RackCad.Plugin/'], SRCF[:3]
ui_dirs = sorted(f[len(UIR):] for f in SRCF if f.startswith(UIR) and f.endswith('/') and f[len(UIR):].count('/') == 1)
ui_files = [f for f in SRCF if f.startswith(UIR) and '/' not in f[len(UIR):]]
assert ui_dirs == ['Controls/', 'Editor/', 'Preview/', 'RackFrames/', 'Shell/', 'StructuralSections/', 'Themes/', 'Views/', 'Workspace/'], ui_dirs
assert len(ui_files) == 34, len(ui_files)
assert sorted(f for f in SRCF if f.startswith(UIR + 'Systems/') and f.endswith('/')) == sorted(
    UIR + 'Systems/' + d for d in ('Cantilever/', 'Dynamic/', 'FlowBed/', 'Larguero/', 'Selective/'))
assert sorted(f for f in SRCF if f.startswith(UIR + 'Systems/PushBack/')) == sorted(
    UIR + 'Systems/PushBack/' + n for n in ('PushBackBootSection.cs', 'PushBackDefenseSection.cs', 'PushBackMatrixCardModel.cs', 'PushBackPreviewModel.cs',
                                            'PushBackPreviewRenderer.cs', 'PushBackRearTopeDialogAdapter.cs', 'PushBackRearTopeSection.cs'))
assert len(SRCF) == 58, len(SRCF)

UI_FILES = (['RackCad.UI.Tests.csproj', 'StaTestRunner.cs', 'EditorWindowTestSupport.cs', 'EditorDiscardPromptCollection.cs',
             'PushBackFullEditorCharacterizationSupport.cs']
            + ['PushBackFullEditorCharacterization%sTests.cs' % n for n in ('State', 'Composite', 'Subdialog', 'Catalog')]
            + ['PushBackClassicCharacterization%sTests.cs' % n for n in ('Keyboard', 'Close', 'Intent', 'Owner', 'Resource', 'SourceGuard')]
            + ['BlankFrontDesviadorHandoffTests.cs', 'EditorClosePolicyTests.cs', 'RichEditorCharacterizationTests.cs', 'RichEditorCloseContractTests.cs',
               'RichEditorContractTests.cs', 'PushBackShellAdoptionTests.cs', 'PushBackModuleEditorWindowTests.cs', 'PushBackSafetyPerPostFlowTests.cs',
               'PushBackSidebarCompositionTests.cs', 'UiSystemBoundaryGuardTests.cs', 'WindowCensusGuardTests.cs']
            + ['PushBackAdvancedRackParametersWindowTests.cs']   # N2-02: la guarda de D-PB-11 que entra en RequiredTests
            + ['CantileverAdvancedPanelWindowTests.cs', 'CantileverComponentEditorTests.cs', 'CantileverEditorWindowTests.cs', 'NumericFieldTests.cs',
               'NumericFieldValidationTests.cs'])
assert len(UI_FILES) == 32 == len(set(UI_FILES))
WS = [p for p in tracked(UT + 'Workspace/') if p not in ALLOWED]
assert len(WS) == 23 and len(tracked(UT + 'Workspace/')) == 24, len(WS)
TESTF = [TR] + [UT + f for f in UI_FILES] + WS
assert len(TESTF) == 56, len(TESTF)
ROOT = ['docs/', '.github/', 'eng/', 'tools/', 'assets/', 'deploy/', 'RackCad.sln', 'Directory.Build.props', 'Directory.Build.targets', 'global.json', 'NuGet.Config',
        'README.md', 'AGENTS.md', 'CLAUDE.md', '.editorconfig', '.gitattributes', '.gitignore']
FORBIDDEN = SRCF + TESTF + ROOT
assert len(FORBIDDEN) == 131 == len(set(FORBIDDEN)), len(FORBIDDEN)
for f in FORBIDDEN:
    assert os.path.exists(f.rstrip('/')) and in_head(f), ('no existe', f)

PKG = 'docs/initiatives/I-64-f4b-pbtc-gate-contract-package.md'
MEAS = 'docs/initiatives/I-64-f4b-pbtc-measurements.md'
PHASE = 'docs/initiatives/I-64-f4b-push-back-completo-package.md'
CO = PILOT + '/F4B-DRAFTS/coordinator/'
SC = PILOT + '/F4B-DRAFTS/superseding-characterization.md'
CONTRACT_REL = PILOT + '/%s/coordinator/gate-contract.json' % TASK
LIT = ('SUSTITUIDA por PB-TC (%s), registro docs/automation/evidence/I-64-pilot/F4B-DRAFTS/superseding-characterization.md fila SUS-0%d: '
       'la ejecuta %s')
SUS = [
    ('D-PB-06', 1, 'PushBackDeclaredCorrectionPilotParityTests.CT_PBTC_06_TheWindowAndThePilotWriteTheSameDesignForTheFourteenDesigns'),
    ('D-PB-06', 2, 'PushBackDeclaredCorrectionDimensionStyleTests.CT_PBTC_04_TheLoadedDimensionStyleSurvivesTheLoadsAndTheGestures'),
    ('D-PB-06', 3, 'PushBackDeclaredCorrectionRoundingTests.CT_PBTC_01_TheLoadedFieldsKeepTheirExactValueAfterLoading'),
    ('D-PB-11', 4, 'PushBackDeclaredCorrectionStandardRestoreTests.CT_PBTC_14_AStandardRestoreResetsTheAdvancedControlsAndReachesTheUpdate'),
]
SKIP_LITERALS = [LIT % s for s in SUS]
INV = [
    'INV-PBTC-01 (alcance): git diff --name-only BaseSha..CurrentSha dentro de las 13 rutas de AllowedWriteScope; en src/, solo ' + W + ', ' + WC + ' y ' + WX
    + ' (W, WC y WX); git diff BaseSha..CurrentSha -- src/RackCad.UI/Controls src/RackCad.UI/Systems/Cantilever src/RackCad.Application src/RackCad.Domain '
      'src/RackCad.Plugin vacio.',
    'INV-PBTC-02 (RED y GREEN): el RED es solo de pruebas: los seis archivos nuevos y la linea del atributo de SUS-01 a SUS-04, sin ningun archivo de src/, '
    'publicado antes del GREEN. El GREEN es solo W, WC y WX, sin ningun archivo de tests/. Criterio mecanico del RED, SOLO sobre el filtro con ExpectRed y con '
    'los resultados de la CI del RED: todos los resultados con outcome Failed; un Message sin el prefijo de tipo ^(?:[A-Za-z_][\\w`]*[.+])+[A-Za-z_][\\w`]* : ; '
    'la primera linea de StackTrace en un archivo bajo tests/; ninguno NotExecuted ni Skipped; y Failed == Selected >= 10. En el RED, los filtros sin ExpectRed '
    'pasan y sus unicos Skipped son SUS-01 a SUS-04.',
    'INV-PBTC-03 (la correccion (a), sobre el diff): (1) ninguna linea anadida ni quitada de git diff -U0 BaseSha..CurrentSha -- W WC contiene SetNumber(; '
    '(2) en W desaparecen las lineas Val( de W:797-801, :819, :837-839 y :845 y aparecen diez llamadas Keep(, sin contar la declaracion del metodo, una por '
    'caja: en ReadInputs sobre DepthBox, ToleranceBox, PostPeralteBox, BeamDepthBox y AnnotationScaleBox, y en ReadCellValues sobre FirstLevelHeightBox, '
    'CellPalletFrontBox, CellPalletHeightBox, CellPalletWeightBox y CellClearBox; aparte, el largo manual (CellBeamLengthOverrideBox) sigue leyendose con '
    'OptionalVal (W:804) y cambia el cuerpo de OptionalVal (W:3625-3626); (3) Val sigue declarada (W:3594) y W:1865 no cambia; (4) el comportamiento lo '
    'fijan CT-PBTC-01 a 03 y 10 en verde. Estas diez cajas no son los diez campos que se pierden (H-PB-42): entra BeamDepthBox, inocuo por el catalogo, y '
    'no entra el largo manual.',
    'INV-PBTC-04 (la correccion (b)): el estilo de cotas que trae la carga llega sin cambio a cada pasada (CT-PBTC-04 y 05); un rack nuevo, sin estilo; en el '
    'diff de WX no aparece ningun control nuevo de estilo.',
    'INV-PBTC-05 (D-PB-08, D-PBTC-03 del paquete): un TextBlock llamado SideBEmptyNoticeText, el ultimo hijo del StackPanel de StatusContent (WX:681-685), '
    'detras de StatusText y con sus atributos (ShellTextSecondaryBrush, FontSize 11, TextWrapping Wrap, Margin 0,4,0,0, Visibility Collapsed); visible si el '
    'lado B esta declarado y ninguna de sus ranuras esta activa, colapsado en cualquier otro caso; su texto es el literal de owner-pbtc-decisions.md, '
    'pregunta 3, byte a byte; no es un Control ni es enfocable; no cambia la habilitacion de ningun boton, el estado, el diseno, la casilla ni StatusText; la '
    'regla de capacidad sin presencia no cambia.',
    'INV-PBTC-06 (la sustitucion declarada): solo SUS-01, SUS-02, SUS-03 y SUS-04 llevan Skip, cada una con su literal exacto, en este orden: '
    + ' | '.join('SUS-0%d: "%s"' % (i + 1, s) for i, s in enumerate(SKIP_LITERALS))
    + '. En esos archivos, el diff es exactamente la linea del atributo. Ninguna otra prueba existente se edita, se salta, se reactiva ni se borra.',
    'INV-PBTC-07 (T1B y guardas): T1B en verde y sin sustitucion (55 en UI y 10 en Core); G-10, I-55, modulos (con Fact3), desviador, foco, '
    'AnnotationScaleBox, las cuatro de H-18, el censo de ventanas y el corpus de WLC, en verde en la CI completa.',
    'INV-PBTC-08 (lo que sigue valido): CT-PB0-01 a CT-PB0-04, CT-PB0-06 a CT-PB0-18, CT-PB0-20 y CT-PB0-21 en verde; las listas (a)/(b) de CT-PB0-01 y '
    'CT-PB0-17, vacias (M-05); lo de la section 5.3 del paquete, en verde.',
    'INV-PBTC-09 (sin efecto no medido): la salida de la ventana es igual a la del piloto en los 14 disenos de WLC (CT-PBTC-06) y en las 12 filas de '
    'CT-PBTC-10; una diferencia en cualquier otro campo es PBTC-C03.',
    'INV-PBTC-10 (las pruebas de UI): las cinco clases nuevas en EditorDiscardPromptCollection; cada ventana con Ventana() y su carga dentro del try, '
    'cerrada en finally tambien si la carga lanza; sin Abrir ni AbrirFueraDePantalla de PushBackFullEditorCharacterizationSupport; ninguna ventana mostrada; '
    'sin teclado ni foco real (D-PBTC-09); prohibidos un MessageBox real, el BOM, Guardar en biblioteca, el configurador real e Insertar; ninguna asercion '
    'dentro de una costura.',
    'INV-PBTC-11 (las costuras): solo las de H-28 del paquete y el contrato publico de la ventana; ninguna costura nueva, y ningun cambio en '
    'InternalsVisibleTo, los paquetes ni los .csproj.',
    'INV-PBTC-12 (determinismo): el GUID fijo de Ventana(); la cultura invariante; los numeros con "R"; el orden ordinal; ni hora ni aleatoriedad.',
    'INV-PBTC-13 (las corridas locales, F4-E59): nunca la suite completa de UI en local, solo los filtros de RequiredTests; la de Core, si; MAX_PATH: sin '
    'copias de arboles; las corridas temporales (section 5.2 y D-PBTC-10 del paquete) no entran en ningun commit, y sus TRX y logs se guardan como '
    'artefactos citados en la entrega; el perfil del usuario sin cambios, con la prueba de la limpieza de INV-PBT0-06.',
    'INV-PBTC-14 (CI y compilacion): CI del GREEN 4/4 con las suites completas; CI del RED con ui-tests en failure y tests, build-ui y build-plugin en '
    'success, sin blame-hang ni timeout (build-plugin depende de tests y build-ui, .github/workflows/ci.yml:254-258); los conteos de avisos por codigo, '
    'iguales a la linea base (Core 66 xUnit, UI 28 xUnit y Plugin 104 MSB3277).',
    'INV-PBTC-15 (el alcance de la tarea, sobre el diff): cada hunk de git diff -U0 BaseSha..CurrentSha -- src cae dentro de un miembro de esta lista '
    'cerrada, por su rango de lineas en BaseSha, o es la declaracion de un miembro nuevo de la lista. Un hunk TOCA un miembro de rango [s, e] en BaseSha si '
    'cambia o quita una linea de ese rango, o si inserta dentro de el (en un hunk @@ -a,0 ..., cuando s <= a < e); una insercion ENTRE dos miembros es la '
    'declaracion de un miembro nuevo y solo se admite si esta en la lista. En W, miembros existentes: LoadFromModel (W:377-427), solo las lineas de los '
    'portadores y de loadedDimensionStyle; Recompute (W:603-702), solo la llamada del aviso tras W:607 y las lineas de D-PBTC-11 alrededor de W:682; '
    'ReadCellValues (W:781-810); ReadInputs (W:812-857); OptionalVal (W:3625-3626). En W, miembros nuevos: ShownFormat, SameShown y Keep, entre OptionalVal '
    '(W:3626) e IntVal (W:3628); los cuatro portadores rackTolerance, rackPostPeralte, rackBeamDepth y rackAnnotationScale, tras generalPallet (W:101); '
    'loadedDimensionStyle, junto a loadedDimensionViews (W:1216). En WC: la constante del aviso y su metodo nuevo. En WX: el TextBlock del aviso. FUERA, por '
    'la letra (OD-PBTC-5 = (B), D-PBTC-07), ningun hunk toca: Gap_Changed (WC:159-175) ni la siembra de GapBox (WC:715-749, con WC:721); ModuleLength_Changed '
    '(W:1852-1873) ni la siembra de ModuleLengthBox (W:1766-1824, con W:1793); LoadAdvancedRackParameters (W:1555-1579), SetOptional (W:1582-1589), '
    'ReadAdvancedRackParameters (W:1592-1607), su manejador (W:1616-1626) ni RestoreRackParameters_Click (W:1628-1634); Val (W:3594) ni IntVal (W:3628).',
    'INV-PBTC-16 (el registro): el Worker no escribe en docs/; el registro ' + SC + ', su paso de DECLARADA a EJECUTADA y la anotacion del paso 6 de D-PB-06 '
    'en f4b-dispositions.md son de la sesion.',
    'INV-PBTC-17 (reglas de F4B, [DP] y D-PB-10): la planificacion segun PBTC-C08; los controles segun PBTC-C11, con las firmas literales de la section 6.7 '
    'del paquete; antes de un intento 3, siempre el Owner (PBTC-C12).',
    'INV-PBTC-18 (sin AutoCAD): la comprobacion fisica es de Smoke-PB (section 9 del paquete de la fase).',
    'INV-PBTC-19 (D-PB-11, sobre el diff, D-PBTC-11 del paquete): (1) dentro de la rama valida de Recompute (W:658-692) el diff anade exactamente una linea '
    'antes de W:682 que lee moduleOwner.ModuleCommit?.StandardRestoreRequested y, tras W:682, un if cuyo cuerpo son exactamente '
    'state.RestoreAdvancedRackParameters(advanced); y LoadAdvancedRackParameters(); (2) ninguna linea anadida en ese hunk contiene RequestRecompute(; '
    '(3) W en CurrentSha no contiene forceRebuild: ni forceRebuild) y sigue conteniendo RequestStandardRestore() (la guarda Fact3, '
    'tests/RackCad.Tests/PushBackModuleEditorCharacterizationTests.cs:381-401); (4) el comportamiento lo fija CT-PBTC-14 en verde.',
]
assert len(INV) == 19 and [i.split(' ')[0] for i in INV] == ['INV-PBTC-%02d' % n for n in range(1, 20)]
EVID = [
    'Antes del Worker: gate-contract valido; delegacion del Controller valida; comparacion byte a byte, aceptacion A1-A8 y nc4 (primera delegacion de la '
    'cadena F4B-PBTC); AllowedWriteScope de la delegacion = exactamente el del contrato; si hubo una replanificacion por PBTC-C08, su registro.',
    'Paso 1, sobre BaseSha y antes del RED (D-PBTC-10 del paquete): la corrida temporal de CT-PBTC-10 con Assert.Fail(string.Join("\\n", log)) en lugar del '
    'Assert.Equal final y el codigo temporal de K1, K2, K3 y K4 de las filas F1 y M-02; el delta de la base de CT-PBTC-10 transcrito fila por fila en '
    'worker-handoff.json, leido del TRX de esa corrida; K1 a K4 de F1 y M-02; el TRX y el log, como artefactos; la primera diferencia de CT-PBTC-10 en el TRX '
    'de la CI del RED, citada para contrastarla con lo transcrito. Si F1 o M-02 cumplen PBTC-C14, STOP antes del GREEN.',
    'Commit RED y push propios antes del GREEN; CI del RED de INV-PBTC-14; los TRX del filtro con ExpectRed cumplen el criterio mecanico de INV-PBTC-02; los '
    'demas filtros en verde, con sus Skipped de SUS-01 a SUS-04.',
    'Commit GREEN y push: git diff --name-only RedSha..CurrentSha = W, WC y WX; las comprobaciones de INV-PBTC-03, INV-PBTC-15 e INV-PBTC-19 sobre el diff, '
    'con su salida; CI del GREEN 4/4, con los conteos de la section 6.9 del paquete (UI 2418 resultados: 0 Failed, 21 Skipped y 2397 Passed; Core 19070 '
    'Passed) o la diferencia explicada por el diff.',
    'En local: build Release de la solucion, Plugin incluido y sin avisos nuevos, sobre el RED y sobre el GREEN; los filtros de RequiredTests sobre el RED y '
    'sobre el GREEN; tres corridas seguidas en verde del filtro con ExpectRed sobre el GREEN; la comprobacion temporal de la section 5.2 del paquete sobre el '
    'GREEN: SUS-01 a SUS-04 sin Skip, y en SUS-03 y SUS-04 con Assert.Fail(string.Join("\\n", lineas)) en lugar del Assert.Equal final (N2-01), con el TRX y '
    'el log de cada corrida como artefactos; la prueba de la limpieza del perfil.',
    'worker-handoff.json conforme a su esquema con RED, GREEN, alcance real, trailer y conteos por filtro (Passed, Failed y Skipped); el mapa CT a metodo '
    '(section 4.4 del paquete); por cada CT, la primera diferencia en la base con el mensaje del TRX; la transcripcion del paso 1; la tabla de SUS-01 a SUS-04 '
    '(la prueba vieja pasa en la base y, sin Skip sobre el GREEN, falla en las filas o lineas declaradas; en SUS-03 y SUS-04, la comparacion linea a linea del '
    'mensaje de Assert.Fail con las esperadas viejas: en SUS-03 difieren diez y coincide F4, en SUS-04 difieren cuatro y coinciden seis); el literal de D-PB-08 '
    'tal como quedo; las rutas relativas de los artefactos de las corridas temporales; KnownLimitations.',
    'Despues: Controller verification valida; nc1, nc2 y nc3 segun README section 10, conservando cada salida ordinaria; un control que no da CONTROL PASS se '
    'resuelve SOLO como dice PBTC-C11 (el sustituto unicamente con la firma literal exacta; si no, el Owner); cada uso se registra en '
    'F4B-DRAFTS/f4b-dispositions.md; la verificacion 14/14 sola no basta para cerrar.',
    'Sin AutoCAD: la ventana se prueba en UI.Tests con el producto real; la comprobacion fisica es de Smoke-PB.',
]
C11 = ('a negative control (nc1, nc2 or nc3) does not give CONTROL PASS: its ordinary output is kept as is and there are only two exits without returning to the Owner '
       '([DP] question 1, "Extender firmas exactas"): (1) nc2 with the EXACT signature of PD-F4-01 (source f4-dispositions.md:1103-1106: Scope in fail on the removed path; '
       'Contract in fail only by the exactness clause; EXECUTION_BLOCKED/STOP; applied literally at :1540: no check other than the real verification differs; the exactness '
       'clause includes Contract evidence citing the contrast of the narrowed AllowedWriteScope with the contract or with byte-fidelity-check.json, f4-dispositions.md:520, :562, '
       'and nothing else) -> substitute PBTC-NC2-EXACT-WHITELIST-01, a copy of F4T1B-NC2-EXACT-WHITELIST-01 with ONLY the id and the scope changed, recorded as '
       'nc2 = SATISFIED_UNDER_PBTC_EXACT_WHITELIST_SUBSTITUTE; (2) nc1 with the EXACT signature of PD-F4-04 (source owner-f4t3-authorization.md:255-265: false CurrentSha '
       'correctly detected; Identity = fail; Handoff also fails first only because the handoff itself contains the false SHA; outcome BLOCKED / STOP; all other intended checks '
       'match; as applied in F4, f4-dispositions.md:1657-1660, Ci in fail only as a consumer of the false CurrentSha is part of the signature, with Remote in pass) -> substitute '
       'PBTC-NC1-IDENTITY-SIGNAL-01, a copy of F4T3C2-NC1-IDENTITY-SIGNAL-01 with ONLY the id and the scope changed, recorded as '
       'nc1 = SATISFIED_UNDER_PBTC_NC1_IDENTITY_SIGNAL_SUBSTITUTE. Matching is decided never by check name: the control output is compared with the real verification check by '
       'check (the set of failing checks, the first failing check, Outcome and StopClass, and the evidence text); the only admitted differences are those listed for each signature '
       'in section 6.7 of the package; the session records the comparison table in f4b-dispositions.md. Any other signature or variation (PD-F4-05, PD-F4-06, nc2 with Ci or '
       'Trailer also in fail, any nc1 that does not reproduce the literal signature, any failure of nc3, or an evidence text that cannot be assigned to an admitted difference) '
       'returns to the Owner before any substitute')
STOPS = P_STOPS + [
    {'Id': 'PBTC-C01', 'Text': 'the correction would need to change a file outside the three product paths (for example NumericField, another window, Application or the Plugin); it is an Owner decision (OB-06.6)'},
    {'Id': 'PBTC-C02', 'Text': 'an existing test or helper outside SUS-01 to SUS-04 would have to be edited, skipped, re-enabled or deleted, or a .csproj, InternalsVisibleTo, the packages or tests/RackCad.Tests/ would have to change; a T1B guard or the Fact3 guard that does not stay green returns to the Owner (OB-T1B.3)'},
    {'Id': 'PBTC-C03', 'Text': 'an unmeasured material effect appears (OB-06.4): a difference in the design, the pieces, the levels or positions, or the BOM that does not come from the exact values of the corrected fields, the dimension style or the D-PB-11 reset; or CT-PBTC-06, CT-PBTC-10, CT-PBTC-14 or a test of section 5.3 of the package fails on the GREEN; STOP and back to the Owner'},
    {'Id': 'PBTC-C04', 'Text': 'a method of the ExpectRed filter passes on the base, or fails by exception and not by assertion; it is a contract defect and the Worker does not change the test to make it fail'},
    {'Id': 'PBTC-C05', 'Text': 'a CT would need AutoCAD, a real MessageBox, the real configurator, the BOM, Guardar en biblioteca, Insertar, the real keyboard or focus, or a write to the user profile or to disk'},
    {'Id': 'PBTC-C06', 'Text': 'the full CI of the GREEN fails outside the new tests, reproduces on a second run and is not attributable to the new tests (attributable is REWORK; not reproduced is a BLOCKED rerun within the 16.8 limits)'},
    {'Id': 'PBTC-C07', 'Text': 'the facts of section 2 of the package do not hold on the base in a way that changes a CT or the design, or an expected line of a CT is not met on the GREEN; the Worker does not adjust the expectation'},
    {'Id': 'PBTC-C08', 'Text': 'a planning output rejected ONLY by the literal byte-fidelity comparison of the collections before the Worker allows ONE replanning with a reinforced prompt, without consuming an attempt ([DP] question 2); a second planning fidelity rejection is STOP and returns to the Owner; any other planning rejection (A1-A8 or another cause) is STOP and returns to the Owner'},
    {'Id': 'PBTC-C09', 'Text': 'a test would hang, leave a window, a modal or a dispatcher alive, write to real user profile data or depend on machine-specific paths'},
    {'Id': 'PBTC-C10', 'Text': 'doing it right would require correcting something outside the contracted scope: the H-31 boxes the Owner left out (owner-pbtc-decisions.md question 4, OD-PBTC-5 = (B)): GapBox and Gap_Changed, ModuleLengthBox and ModuleLength_Changed, and the four nullable advanced boxes with ReadAdvancedRackParameters and its handler, or any member of the OUT list of INV-PBTC-15; or (c), (d), the tail of pb-21-cola, D-3 or M-01'},
    {'Id': 'PBTC-C11', 'Text': C11},
    {'Id': 'PBTC-C12', 'Text': 'a correction would be attempt 3 of 3: it returns to the Owner before it is issued (D-PB-10)'},
    {'Id': 'PBTC-C13', 'Text': 'the D-PB-08 notice would need a text or a place other than those the Owner chose (owner-pbtc-decisions.md question 3), would have to be focusable or would block an action; it is an Owner presentation decision (OB-08.4)'},
    {'Id': 'PBTC-C14', 'Text': 'in the base delta measured in step 1 (D-PBTC-10 of the package), row F1 or row M-02 of CT-PBTC-10 has a line under $.PushBack.Structure.Modules or $.PushBack.Structure.HeaderLineOverrides, or is material by the CT-PB0-21 formula (RoundingEffectTests.cs:164: K1 or K4 not empty, K4 being coordinates displaced by more than 0.001, or K2 or K3 true): the Worker stops BEFORE the GREEN (a published RED stays published and is declared), delivers the transcription, and the session takes it to the Owner (OB-06.4; owner-pbtc-decisions.md question 1); if the Owner orders to continue, the order fixes how the chain continues and whether an attempt is consumed'},
]
assert [s['Id'] for s in STOPS[17:]] == ['PBTC-C%02d' % n for n in range(1, 15)] and len(STOPS) == 31
REQ = [
    (UT + 'RackCad.UI.Tests.csproj', 'FullyQualifiedName~RackCad.UI.Tests.PushBackDeclaredCorrection', 10, True),
    (UT + 'RackCad.UI.Tests.csproj', 'FullyQualifiedName~RackCad.UI.Tests.Cantilever', 67, False),
    (UT + 'RackCad.UI.Tests.csproj', 'FullyQualifiedName~RackCad.UI.Tests.PushBackClassicCharacterization', 55, False),
    (UT + 'RackCad.UI.Tests.csproj', 'FullyQualifiedName~RackCad.UI.Tests.PushBackFullEditorCharacterization', 16, False),
    (UT + 'RackCad.UI.Tests.csproj', 'FullyQualifiedName~RackCad.UI.Tests.WorkspaceDraftPilotWindowLoadCharacterizationTests', 3, False),
    (UT + 'RackCad.UI.Tests.csproj', 'FullyQualifiedName~RackCad.UI.Tests.PushBackDimensionViewsWindowTests', 12, False),
    (UT + 'RackCad.UI.Tests.csproj', 'FullyQualifiedName~RackCad.UI.Tests.PushBackAdvancedRackParametersWindowTests', 9, False),
    (UT + 'RackCad.UI.Tests.csproj', 'FullyQualifiedName~RackCad.UI.Tests.PushBackModuleEditorWindowTests', 13, False),
    (TR + 'RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackClassicCharacterization', 10, False),
    (TR + 'RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackFullEditorCharacterization', 3, False),
    (TR + 'RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackBlankFrontAuthorityTests', 9, False),
    (TR + 'RackCad.Tests.csproj', 'FullyQualifiedName~RackCad.Tests.PushBackModuleEditorCharacterizationTests', 15, False),
]
assert len(REQ) == 12 and [r for r in REQ if r[3]] == [REQ[0]]
for proj, _, _, _ in REQ:
    assert in_head(proj), proj
cells = json.loads(json.dumps(t1b['EligibleCells']))
assert [c['Cell'] for c in cells] == ['anthropic/subagent/claude-sonnet-5-5', 'openai/process/gpt-6-luna']
assert cells == pbt0['EligibleCells'], 'las celdas difieren de las de PB-T0'
UNIT_CHANGE = [
    (W, 'la ventana clasica de Push Back: la correccion (a) (Keep y los portadores), el portador del estilo, la llamada del aviso y D-PB-11'),
    (WC, 'la parte compuesta de la ventana: la constante y el metodo del aviso de D-PB-08'),
    (WX, 'el XAML de la ventana: el TextBlock del aviso en StatusContent'),
]
EXISTING = [
    (PHASE, 'paquete de la fase F4B v3.1: OB-06.1 a OB-06.6, OB-08.1 a OB-08.4, la fila de PB-TC y "Por que PB-TC va antes de PB-T1" (section 5), OB-T1B.1 a OB-T1B.3, OB-RE.1, section 9 y section 10'),
    (CO + 'owner-f4b-disposition.md', 'archivo completo ([OD]): D-PB-06 y sus seis pasos, D-PB-08, D-PB-10, T1B y RACKEDITAR'),
    (CO + 'owner-f4b-procedure-decisions.md', 'archivo completo ([DP], F4B-E03): preguntas 1 y 2 (firmas exactas de los sustitutos y la replanificacion unica)'),
    (PILOT + '/F4B-DRAFTS/f4b-dispositions.md', 'archivo completo: F4B-E01 a F4B-E07'),
    (PILOT + '/F4B-PBT0-CHARACTERIZATION/task-close-package.md', 'el cierre de PB-T0: section 5 (los arrastres a PB-TC y las lecciones)'),
    (PBT0C, 'el contrato de PB-T0 ([PK0-C]): el metodo del ForbiddenWriteScope, las paradas estandar con P-01 y las celdas'),
    ('docs/initiatives/I-64-f4-t1b-gate-contract-package.md', 'el patron de T1B ([T1B]): la caracterizacion vigente de la ventana clasica'),
    (T1BC, 'el contrato de T1B ([T1B-C]): las paradas estandar y las celdas'),
    (PILOT + '/F4-DRAFTS/f4-dispositions.md', 'las fuentes de PD-F4-01 (:1103-1106, :1540, :520, :562) y PD-F4-04 (:1657-1660), y F4-E59'),
    (PILOT + '/F4-DRAFTS/coordinator/owner-f4t3-authorization.md', 'la firma literal de PD-F4-04 (:255-265)'),
    ('docs/automation/state/I-64.yml', 'estado vigente completo'),
    ('docs/automation/evidence/I-64-evidence.md', 'las secciones de F4B (231 y siguientes)'),
]
EXTERNAL = [
    ('docs/adr/0029-contrato-funcional-comun-de-ventanas-wpf.md', 'D13 (la caracterizacion no se edita para que pase; un cambio de comportamiento exige decision del Owner)'),
    ('docs/AUTOMATION_PLAN.md', 'section 16.5, 16.8, 16.9 y 16.11'),
    ('docs/automation/agent-execution/README.md', 'section 4 (A1-A8) y section 10 (nc1..nc4)'),
    ('docs/automation/agent-execution/routing.md', 'archivo completo'),
    ('docs/automation/agent-execution/model-catalog.md', 'celdas medidas y vigentes para Controller y Worker'),
]
for p, _ in EXISTING + EXTERNAL + UNIT_CHANGE:
    assert in_head(p), ('autoridad inexistente en HEAD', p)
c = {
    'Schema': 'rackcad-gate-contract/v1', 'Unit': 'I-64', 'Gate': 'F4B', 'TaskId': TASK,
    'Objective': ('Corregir en la ventana clasica de Push Back, como cambios declarados de RACKEDITAR, las perdidas (a) redondeo y (b) estilo de cotas de '
                  'D-PB-06, la perdida de \'Restaurar estandar\' al Actualizar (D-PB-11, H-PB-28), y anadir el aviso no bloqueante de D-PB-08 en la banda '
                  'de estado, segun el paquete del contrato de PB-TC v2.2 y la lista cerrada CT-PBTC-01..08, 10 y 14. El RED son pruebas nuevas que fallan '
                  'por asercion en la base y la sustitucion declarada SUS-01..SUS-04; el GREEN corrige solo la ventana. NumericField, Cantilever, '
                  'Application y el Plugin no cambian; GapBox, ModuleLengthBox y las cajas avanzadas anulables quedan fuera; el paso 4 en el Workspace '
                  'queda para PB-T2; no se ejecuta AutoCAD.'),
    'AuthorityRevision': AR, 'MainSha': MAIN,
    'Authorities': [
        {'Path': PKG, 'Section': 'archivo completo (v2.2): section 2 (hechos medidos), section 3 (D-PBTC-01 a D-PBTC-11), section 4 (CT-PBTC-01 a 08, 10 y 14, y el mapa CT a metodo), section 5 (RED, SUS-01 a SUS-04 y el registro), section 6 (el contrato), section 8 (decisiones del Owner) y section 10 (el caso pendiente de H-31)', 'Class': 'UNIT_DOC'},
        {'Path': MEAS, 'Section': 'el registro de mediciones del paquete, apartados A a L', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_DOC'} for p, s in EXISTING[:3]] + [
        {'Path': CO + 'owner-pbtc-decisions.md', 'Section': 'archivo completo ([OPD], F4B-E06): OD-PBTC-1 (i) con R-03 (i) y la extension declarada, OD-PBTC-2 (i), OD-PBTC-3 (el texto corto en la franja de estado, literal byte a byte), OD-PBTC-4 (A) = D-PB-11 y OD-PBTC-5 (B)', 'Class': 'UNIT_DOC'},
        {'Path': SC, 'Section': 'el registro declarado ([SC]): SUS-01 a SUS-04 en estado DECLARADA, lo no sustituido, los pendientes de PB-T2, el caso de H-31 y el cambio de comportamiento declarado de RACKEDITAR', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_DOC'} for p, s in EXISTING[3:]] + [
        {'Path': CONTRACT_REL, 'Section': 'este contrato', 'Class': 'UNIT_DOC'},
    ] + [{'Path': p, 'Section': s, 'Class': 'UNIT_CHANGE'} for p, s in UNIT_CHANGE]
      + [{'Path': p, 'Section': s, 'Class': 'EXTERNAL'} for p, s in EXTERNAL],
    'AllowedWriteScope': ALLOWED,
    'ForbiddenWriteScope': FORBIDDEN,
    'Invariants': INV,
    'RequiredTests': [{'Project': p, 'Filter': f, 'MinSelected': m, 'ExpectRed': e} for p, f, m, e in REQ],
    'ExpectedEvidence': EVID, 'StopConditions': STOPS, 'EligibleCells': cells, 'RoutingEnforcement': 'required', 'CorrectionsAuthorized': True,
    'IssuedBy': ('Sesion principal de I-64 (Claude) por las decisiones del Owner antes de emitir PB-TC '
                 '(docs/automation/evidence/I-64-pilot/F4B-DRAFTS/coordinator/owner-pbtc-decisions.md, F4B-E06), las decisiones de procedimiento '
                 'owner-f4b-procedure-decisions.md (F4B-E03), el paquete del contrato v2.2 con sus revisiones IRT1, ICT2 e ICT21, y la orden expresa del '
                 'Owner en el chat de emitir PB-TC (F4B-E07)'),
    'IssuedUtc': ISSUED,
}
assert len(c['Authorities']) == 25, len(c['Authorities'])
b = (json.dumps(c, indent=2, ensure_ascii=True) + '\n').encode('ascii')
assert all(x < 0x80 for x in b) and b'\\u' not in b, 'no ASCII'
assert not set(c['AllowedWriteScope']) & set(c['ForbiddenWriteScope'])
for a in c['AllowedWriteScope']:
    assert not any(a.startswith(f) for f in c['ForbiddenWriteScope'] if f.endswith('/')), a
TERMS = ['GATE PASS', 'GATE_PASS', 'Candidato', 'Candidate', 'FINAL_CANDIDATE_SHA', 'gate cerrado', 'gate closed', 'cierre del gate', 'rama integrada', 'integrada en main',
         'integrated into main', 'merged into main', 'lista para integrar', 'ready to merge', 'Owner Validation APROBADA', 'Owner Validation APPROVED']
for t in TERMS:
    assert not re.search(r'(?<![A-Za-z0-9_])' + re.escape(t) + r'(?![A-Za-z0-9_])', b.decode('ascii'), re.I), t
for bad in ('C:' + chr(92), 'C:' + '/', 'D:' + '/', 'Users', 'alej' + 'andra', 'App' + 'Data', 'scratchpad', '.co' + 'dex'):
    assert bad not in b.decode('ascii'), bad
if '--dry' in sys.argv:
    print('DRY allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), '(src', len(SRCF), 'tests', len(TESTF), 'root', len(ROOT), ') inv', len(INV), 'stops', len(STOPS),
          'required', len(REQ), 'evidence', len(EVID), 'authorities', len(c['Authorities']), 'bytes', len(b))
    if '--print' in sys.argv:
        for i, f in enumerate(FORBIDDEN, 1):
            print(i, f)
    sys.exit(0)
os.makedirs(os.path.dirname(OUT), exist_ok=True)
open(OUT, 'wb').write(b)
o = subprocess.run(['pwsh', '-NoProfile', '-Command', f"Get-Content -Raw '{OUT}' | Test-Json -SchemaFile 'docs/automation/agent-execution/schemas/gate-contract.schema.json'"], capture_output=True, text=True)
assert o.stdout.strip().endswith('True'), o.stderr[-300:]
print('schema True allowed', len(ALLOWED), 'forbidden', len(FORBIDDEN), 'inv', len(INV), 'stops', len(STOPS), 'authorities', len(c['Authorities']), 'sha256', hashlib.sha256(b).hexdigest().upper())
